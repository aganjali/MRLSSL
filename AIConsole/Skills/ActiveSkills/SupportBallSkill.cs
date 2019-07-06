using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using GetBallState = MRL.SSL.AIConsole.Engine.NormalSharedState.GetBallState;
namespace MRL.SSL.AIConsole.Skills
{
    public class SupportBallSkill:SkillBase
    {
        GetBallState activeState = GetBallState.Static;
        State CurrentState = State.OutGoing;
        int outGoingState = 0;
        bool Debug = false, TurnBall = false, firstInInc = true, firstInState = true, midPointSet = false;

        const double MaxIntegral = 5000;

        bool inRefrence = false;
        bool firstInref = true, AvoidDangerZone = true, AvoidRobots = true;
        Vector2D lastV = new Vector2D();
        MRL.SSL.Planning.MotionPlanner.PID pidSideX = new MRL.SSL.Planning.MotionPlanner.PID();
        MRL.SSL.Planning.MotionPlanner.PID pidSideY = new MRL.SSL.Planning.MotionPlanner.PID();
        double DefrentionalT = 0, LastDr = 0, IntegralT2 = 0;
        double lastErr = 0, iErr = 0;
        double lastPIDangular = 0;
        double ourDangerMarg = 0.3;
        double ourSafeRad = 0.2;
        Vector2D MaxSpeed = new Vector2D(5, 5);

        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, int? ActiveID, int ActiveState, Position2D Target, Position2D activeTarget, double behindBallDist, bool far, Position2D incomPoint, int angSt)
        {
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 6, 4);
            activeState = (GetBallState)ActiveState;
            Target = Model.BallState.Location + (Target - Model.BallState.Location).GetNormalizeToCopy(behindBallDist);
            if (ActiveID.HasValue && Model.OurRobots.ContainsKey(ActiveID.Value))
            {
                DetermineNextState(Model, RobotID, ActiveID.Value, Target, activeTarget, far, incomPoint);
                if (CurrentState == State.OutGoing)
                {
                    OutGoingSupport(Model, RobotID, ActiveID.Value, activeTarget, Target, behindBallDist);
                }
                else if (CurrentState == State.Incomming)
                {
                    if (TurnBall)
                        IncommingSupportTurnBall(Model, RobotID, ActiveID.Value, activeTarget, Target, far, incomPoint);
                    else
                        IncommingSupportTurnRobot(Model, RobotID, ActiveID.Value, activeTarget, Target, far, incomPoint);
                }
                else if (CurrentState == State.GotoPoint)
                {
                    // Planner.Add(RobotID, new SingleObjectState(Target, Model.BallState.Speed, (float)((angSt == 0) ? (Model.BallState.Location - Target).AngleInDegrees : (Target - Model.BallState.Location).AngleInDegrees)), PathType.UnSafe, false, false, true, false);
                    GotoPointTrack(Model, RobotID, new SingleObjectState(Target, Model.BallState.Speed, 0), Model.BallState.Location);
                }
                DrawingObjects.AddObject(new StringDraw("StateSupport: " + CurrentState, Model.OurRobots[RobotID].Location + new Vector2D(0.5, 0.5)));
                DrawingObjects.AddObject(new StringDraw("turnBall: " + TurnBall, Model.OurRobots[RobotID].Location + new Vector2D(0.6, 0.5)));
            }
            else
            {

                DrawingObjects.AddObject(new StringDraw("No Active", Model.OurRobots[RobotID].Location + new Vector2D(0.5, 0.5)));
                Planner.Add(RobotID, Target, (float)((angSt == 0) ? (Model.BallState.Location - Target).AngleInDegrees : (Target - Model.BallState.Location).AngleInDegrees), PathType.UnSafe, false, AvoidRobots, true, false);
            }
        }

        private void DetermineNextState(WorldModel Model, int RobotID, int ActiveID, Position2D Target, Position2D activeTarget, bool far, Position2D incomPoint)
        {
            if (CurrentState == State.OutGoing)
            {
                if (firstInState)
                {
                    outGoingState = 0;
                    firstInState = false;
                }
                if (IsBehindBallAndActive(Model, RobotID, ActiveID, Target, activeTarget))
                {
                    CurrentState = State.GotoPoint;
                    firstInState = true;
                }
                else if (activeState == GetBallState.Incomming)
                    CurrentState = State.Incomming;
            }
            else if (CurrentState == State.Incomming)
            {
                if (firstInState)
                {
                    outGoingState = 0;
                    firstInState = false;
                }
              
                bool tmpTurnBall;
                bool isBehind = IsBehindBallAndActiveIncomming(Model, RobotID, ActiveID, Target, activeTarget, far, incomPoint, out tmpTurnBall);
              
                if (isBehind)
                {
                    CurrentState = State.GotoPoint;
                    firstInState = true;
                }
                else if (activeState != GetBallState.Incomming)
                    CurrentState = State.OutGoing;

                if (!isBehind && firstInInc)
                { 
                    TurnBall = tmpTurnBall;
                    firstInInc = false;
                }

            }
            else if (CurrentState == State.GotoPoint)
            {
                if (firstInState)
                {
                    outGoingState = 0;
                    firstInState = false;
                }

                if (activeState != GetBallState.Incomming && !IsBehindBallAndActive(Model, RobotID, ActiveID, Target, activeTarget))
                    CurrentState = State.OutGoing;
                else if(activeState == GetBallState.Incomming)
                {
                    bool tmpTurnBall;
                    bool isBehind = IsBehindBallAndActiveIncomming(Model, RobotID, ActiveID, Target, activeTarget, far, incomPoint, out tmpTurnBall);
                    if (!isBehind)
                    {
                        CurrentState = State.Incomming;
                    }
                    if (!isBehind && firstInInc)
                    {
                        TurnBall = tmpTurnBall;
                        firstInInc = false;
                    }
                }
                
            }
            if (CurrentState != State.Incomming)
                firstInInc = true;
        }

        public void GotoPointTrack(WorldModel Model, int RobotID, SingleObjectState ball, Position2D Target)
        {
            double BallSpeedCoef = 0.9;
            double BallDistanceTresh = 0.6;
            double AngleTresh = Math.PI / 4;
            double AngleTresh2 = Math.PI / 3;
            double segmentConst = 0.7;
            double maxRearDistance = 0.05;
            double trackBackBall = 0.09;
            double refrenceBackBall = 0.03;

            SingleObjectState targetInRefrence = new SingleObjectState();

            Vector2D ballSpeed = ball.Speed;
            Position2D ballLocation = ball.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = Target - ballLocation;

            Vector2D targetRobot = Model.OurRobots[RobotID].Location - Target;
            Vector2D targetRobotInRef = GameParameters.InRefrence(targetRobot, -ballTargetVec);

            double rearDistance = maxRearDistance;
            //if (targetRobotInRef.Y - ballTargetVec.Size >= 0.1)
            //{
            //    rearDistance = Math.Min(maxRearDistance, Math.Abs(Vector2D.AngleBetweenInRadians(ballSpeed, ballTargetVec)) / (Math.PI / 2) * maxRearDistance);
            //}

            SingleObjectState robot = new SingleObjectState(Model.OurRobots[RobotID]), robotInRefrence = new SingleObjectState();

            Vector2D ballTarget = Target - ball.Location;
            rearDistance = Math.Min(maxRearDistance, maxRearDistance * Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) / (Math.PI / 2));
            Position2D backBallPoint = ((ballLocation + 0.0 * Model.BallState.Speed) - ballTargetVec.GetNormalizeToCopy(trackBackBall));
            double dist, DistFromBorder;
            if (AvoidDangerZone && GameParameters.IsInDangerousZone(backBallPoint, false, ourDangerMarg, out dist, out DistFromBorder))
            {
                backBallPoint = GameParameters.OurGoalCenter + (backBallPoint - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(backBallPoint, Vector2D.Zero, 0), ourSafeRad));
            }
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;

            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            midPoint.DrawColor = Color.Magenta;
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(midPoint, 0.01, new Pen(Color.Magenta, 0.01f)), "suppmidpoint");
                DrawingObjects.AddObject(backBallPoint, "suppbackBallPoint");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBackBallVec, new Pen(Color.YellowGreen, 0.01f)), "supprobotbackballline");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBallVec, new Pen(Color.Red, 0.01f)), "supprobotballline");
                DrawingObjects.AddObject(new Line(ballLocation, ballLocation + ballTargetVec, new Pen(Color.Blue, 0.01f)), "suppballtargetline");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p1line");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p2line");
                DrawingObjects.AddObject(new Line(ball.Location, ball.Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2), new Pen(Color.YellowGreen, 0.01f)), "supprobotAngleLine");
            }
            Position2D finalPosToGo = midPoint;
            //double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            //double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            //    double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));
            Position2D Target2GO = new Position2D();
            Vector2D FieldSize = new Vector2D(GameParameters.OurGoalCenter.X  + 0.7, Math.Abs(GameParameters.OurLeftCorner.Y) + 0.7);

            Position2D backBall = ball.Location - ballTarget.GetNormalizeToCopy(refrenceBackBall);

            if (AvoidDangerZone && GameParameters.IsInDangerousZone(backBall, false, ourDangerMarg, out dist, out DistFromBorder))
            {
                backBall = GameParameters.OurGoalCenter + (backBall - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(backBall, Vector2D.Zero, 0), ourSafeRad));
            }
            Vector2D robotBackBall = backBall - robot.Location;
            Line l = new Line(ball.Location, Target, new Pen(Color.Wheat, 0.02f));
            Line l2 = l.PerpenducilarLineToPoint(robot.Location);
            Position2D? intersect = l.IntersectWithLine(l2);
            Target2GO = backBall;

            Vector2D Reference = -ballTarget;
            double angleRefrence = Reference.AngleInRadians;

            Target2GO.X = Math.Min(Math.Abs(Target2GO.X), FieldSize.X) * Math.Sign(Target2GO.X);
            Target2GO.Y = Math.Min(Math.Abs(Target2GO.Y), FieldSize.Y) * Math.Sign(Target2GO.Y);

            Position2D tempPos2 = new Position2D();
            tempPos2.X = Target2GO.Y * Math.Cos(angleRefrence) - Target2GO.X * Math.Sin(angleRefrence);
            tempPos2.Y = Target2GO.X * Math.Cos(angleRefrence) + Target2GO.Y * Math.Sin(angleRefrence);
            Target2GO = tempPos2;


            Position2D tempPos1 = new Position2D();
            tempPos1.X = robot.Location.Y * Math.Cos(angleRefrence) - robot.Location.X * Math.Sin(angleRefrence);
            tempPos1.Y = robot.Location.X * Math.Cos(angleRefrence) + robot.Location.Y * Math.Sin(angleRefrence);
            robot.Location = tempPos1;
            double dx = Target2GO.X - robot.Location.X, dY = Target2GO.Y - robot.Location.Y;
            double dv = Model.OurRobots[RobotID].Speed.Size - ball.Speed.Size;
            if (!inRefrence /* && dv > SpeedTresh */&& ball.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < AngleTresh || (Math.Abs(dx) < 0.04 && dY < 0))
                inRefrence = true;
            else if (inRefrence && !(ball.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < AngleTresh2 || (Math.Abs(dx) < 0.13 && dY < 0)))
                inRefrence = false;
            if (inRefrence)
            {
                if (Debug)
                    DrawingObjects.AddObject(new Circle(ball.Location, 0.2, new Pen(Color.Black, 0.01f)), "suppInRef");
                GotoPointTrackInRefrence(Model,ball, RobotID, Target, refrenceBackBall);
                return;
            }
            else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }
            pidSideX.Reset();
            pidSideY.Reset();
            Reset();
            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.03, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGo1");
            double extendCoef = BallSpeedCoef;

            if (targetRobotInRef.Y - ballTargetVec.Size >= 0.1)
            {
                extendCoef = Math.Min(BallSpeedCoef, Math.Abs(Vector2D.AngleBetweenInRadians(ballSpeed, ballTargetVec)) / (Math.PI / 2) * BallSpeedCoef);
            }
            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size + extendCoef * ballSpeed.Size);

            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.05, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGo2");
            firstInref = true;

            Planner.Add(RobotID, new SingleObjectState(finalPosToGo, Vector2D.Zero, (float)ballTargetVec.AngleInDegrees), PathType.UnSafe, false, AvoidRobots, AvoidDangerZone, false);
        }

        private void GotoPointTrackInRefrence(WorldModel Model, SingleObjectState ball, int RobotID, Position2D Target, double BackDistance)
        {

            pidSideY.Coef = new PIDCoef(ActiveParameters.KpSideY, ActiveParameters.KiSideY, ActiveParameters.KdSideY);


            pidSideX.Coef = new PIDCoef(ActiveParameters.KpSideX, ActiveParameters.KiSideX, ActiveParameters.KdSideX);

            Position2D Target2GO = new Position2D();
            Vector2D FieldSize = new Vector2D(GameParameters.OurLeftCorner.X  + 0.7, Math.Abs(GameParameters.OurLeftCorner.Y)  + 0.7);
            SingleObjectState stat = new SingleObjectState(Model.OurRobots[RobotID]);


            Position2D tempPos2 = new Position2D();
            #region HighLevel
            SingleObjectState targetInRefrence = new SingleObjectState();
            SingleObjectState robot = Model.OurRobots[RobotID], robotInRefrence = new SingleObjectState();

            Vector2D ballTarget = Target - ball.Location;
            Position2D backBall = ball.Location - ballTarget.GetNormalizeToCopy(BackDistance);
            double dist, DistFromBorder;
            if (AvoidDangerZone && GameParameters.IsInDangerousZone(backBall, false, ourDangerMarg, out dist, out DistFromBorder))
            {
                backBall = GameParameters.OurGoalCenter + (backBall - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(backBall, Vector2D.Zero, 0), ourSafeRad));
            }
            Vector2D robotBackBall = backBall - robot.Location;
            Line l = new Line(ball.Location, Target, new Pen(Color.Wheat, 0.02f));
            Line l2 = l.PerpenducilarLineToPoint(robot.Location);
            Position2D? intersect = l.IntersectWithLine(l2);
            Target2GO = backBall;

            Vector2D Reference = -ballTarget;
            double angleRefrence = Reference.AngleInRadians;

            DrawingObjects.AddObject(new Line(Target2GO, Target2GO + Reference, new Pen(Color.Wheat, 0.01f)), "supprefrence");
            #endregion

            Target2GO.X = Math.Min(Math.Abs(Target2GO.X), FieldSize.X) * Math.Sign(Target2GO.X);
            Target2GO.Y = Math.Min(Math.Abs(Target2GO.Y), FieldSize.Y) * Math.Sign(Target2GO.Y);

            #region Refrence Convertion
            tempPos2.X = Target2GO.Y * Math.Cos(angleRefrence) - Target2GO.X * Math.Sin(angleRefrence);
            tempPos2.Y = Target2GO.X * Math.Cos(angleRefrence) + Target2GO.Y * Math.Sin(angleRefrence);
            Target2GO = tempPos2;

            Vector2D tempVec2 = new Vector2D();
            tempVec2.X = ball.Speed.Y * Math.Cos(angleRefrence) - ball.Speed.X * Math.Sin(angleRefrence);
            tempVec2.Y = ball.Speed.X * Math.Cos(angleRefrence) + ball.Speed.Y * Math.Sin(angleRefrence);
            Vector2D finalSpeed = tempVec2;

            Position2D tempPos1 = new Position2D(); ;
            tempPos1.X = stat.Location.Y * Math.Cos(angleRefrence) - stat.Location.X * Math.Sin(angleRefrence);
            tempPos1.Y = stat.Location.X * Math.Cos(angleRefrence) + stat.Location.Y * Math.Sin(angleRefrence);
            stat.Location = tempPos1;


            Vector2D vel = Model.OurRobots[RobotID].Speed;
            if (Model.lastVelocity.ContainsKey(RobotID))
                vel = Model.lastVelocity[RobotID];

            Vector2D tempRobotSpeed = new Vector2D();
            tempRobotSpeed.X = vel.Y * Math.Cos(angleRefrence) - vel.X * Math.Sin(angleRefrence);
            tempRobotSpeed.Y = vel.X * Math.Cos(angleRefrence) + vel.Y * Math.Sin(angleRefrence);
            Vector2D RobotSpeedInRefrence = tempRobotSpeed;
            if (firstInref)
            {
                firstInref = false;
                lastV = RobotSpeedInRefrence;

            }

            stat.Speed = lastV;

            double dX = Target2GO.X - stat.Location.X, dY =
                Target2GO.Y - stat.Location.Y, v0X = stat.Speed.X, v0Y = stat.Speed.Y, dvx = finalSpeed.X - lastV.X, dvy = finalSpeed.Y - lastV.Y;

            #endregion

            #region Applying Acceleration
            Vector2D Vtemp = Vector2D.Zero;


            // double vy = (ActiveParameters.KpTotalVySide * (1 / (-Math.Abs(pidSideY.Calculate(dX, 0)) - ActiveParameters.vyOffsetSide + ActiveParameters.KpxVySide * -Math.Abs(finalSpeed.X))) + Math.Min(ActiveParameters.KpyVySide * (finalSpeed.Y + dY), 0));
            double extY = (ActiveParameters.KpxVySide * Math.Abs(dX) + ActiveParameters.KpyVySide * finalSpeed.Y) / pidSideY.Coef.Kp;
            extY = Math.Min(extY, 0.1);
            double vy = -pidSideY.Calculate(dY + extY, 0);
            vy = Math.Min(0, vy);



            double extendedVx = 0;
            extendedVx = (ball.Speed.X * ballTarget.Y - ball.Speed.Y * ballTarget.X)/** Model.OurRobots[RobotID].Location.DistanceFrom(backBall)*/;
            if (Debug)
            {
                //DrawingObjects.AddObject(new StringDraw("dx: " + (dX), new Position2D(0.8, 0)));
                //DrawingObjects.AddObject(new StringDraw("extendedVx: " + (extendedVx), new Position2D(0.7, 0)));
                //DrawingObjects.AddObject(new StringDraw("currX: " + (dX + (ActiveParameters.KpxVxSide * finalSpeed.X * Math.Abs(dY)) / pidSideX.KP), new Position2D(0.6, 0)));
            }

            //double pidout = -pidSideX.Calculate(dX + ActiveParameters.KpxVxSide/pidSideX.KP * finalSpeed.X, 0);
            double extX = (ActiveParameters.KpxVxSide * finalSpeed.X /** Math.Abs(Math.Sin(Model.BallState.Speed.AngleInRadians))*/) / pidSideX.Coef.Kp;
            double pidout = -pidSideX.Calculate(dX + extX, 0);
            Vtemp = new Vector2D(pidout, vy);

            Vector2D p = new Vector2D(Target2GO.X + extX, Target2GO.Y + extY);
            p = GameParameters.InRefrence(p, Reference);
            Position2D pp = new Position2D(p.X, p.Y);
            pp.DrawColor = Color.LemonChiffon;
            DrawingObjects.AddObject(pp, "suppextendedpointPIDx");

            MaxSpeed.X = 5;
            if (Vtemp.Size > MaxSpeed.X)
                Vtemp.NormalizeTo(MaxSpeed.X);
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("suppVc: " + Vtemp, new Position2D(1, 0)));
                DrawingObjects.AddObject(new StringDraw("suppVcSize: " + Vtemp.Size, new Position2D(1.2, 0)));
            }
            //if (Math.Abs(Vtemp.X) > MaxSpeed.X)
            //    Vtemp.X = Math.Sign(Vtemp.X) * Math.Abs(MaxSpeed.X);

            //MaxSpeed = new Vector2D(2, Math.Abs(vy));
            //if (Math.Abs(Vtemp.Y) > MaxSpeed.Y)
            //    Vtemp.Y = Math.Sign(Vtemp.Y) * Math.Abs(MaxSpeed.Y);


            lastV.X = Vtemp.X;
            lastV.Y = Vtemp.Y;

            #endregion
            #region Convert In Robot Axis

            Vector2D tempRobot = new Vector2D(Vtemp.X, Vtemp.Y);
            Vector2D VRobotInField = new Vector2D();
            //VRobotInField.X = tempRobot.X * Math.Cos(-angleRefrence) - tempRobot.Y * Math.Sin(-angleRefrence);
            //VRobotInField.Y = tempRobot.Y * Math.Cos(-angleRefrence) + tempRobot.X * Math.Sin(-angleRefrence);
            VRobotInField.X = tempRobot.Y * Math.Cos(angleRefrence) - tempRobot.X * Math.Sin(angleRefrence);
            VRobotInField.Y = tempRobot.X * Math.Cos(angleRefrence) + tempRobot.Y * Math.Sin(angleRefrence);
            DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + VRobotInField, new Pen(Color.Crimson, 0.01f)), "suppVcomanndLine");
            Vector2D temp = new Vector2D(VRobotInField.X, VRobotInField.Y);
            double Rotation = Model.OurRobots[RobotID].Angle.Value;
            if (Rotation > 180)
                Rotation -= 360;
            if (Rotation < -180)
                Rotation += 360;
            Rotation *= Math.PI / (double)180;

            Vector2D V = new Vector2D();
            //V.X = temp.X * Math.Cos(Rotation) - temp.Y * Math.Sin(Rotation);
            //V.Y = temp.Y * Math.Cos(Rotation) + temp.X * Math.Sin(Rotation);
            V.X = temp.Y * Math.Cos(Rotation) - temp.X * Math.Sin(Rotation);
            V.Y = temp.X * Math.Cos(Rotation) + temp.Y * Math.Sin(Rotation);

            double w = 0;
            if (ballTarget.Size > 1e-4)
                w = (ball.Speed.X * ballTarget.Y - Model.BallState.Speed.Y * ballTarget.X) / ballTarget.Size;
            w = w.ToDegree();
            w *= Model.OurRobots[RobotID].Location.DistanceFrom(backBall);
            w *= 1.2;
            if (w > 30)
                w = 30;
            else if (w < -30)
                w = -30;

            double ang = ballTarget.AngleInDegrees + w;
            double wout = AngularController(Model, RobotID, ang);
            double alfamax = 20;
            double alfa = (wout - (-Model.lastW[RobotID])) * StaticVariables.FRAME_RATE;
            double ww = wout;
            if (Math.Abs(alfa) > alfamax)
                ww = (-Model.lastW[RobotID]) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("suppwout: " + wout, new Position2D(-2, 0)));
                DrawingObjects.AddObject(new StringDraw("suppww: " + ww, new Position2D(-2.2, 0)));
            }
            double amax = 10;
            Vector2D A = (VRobotInField - Model.lastVelocity[RobotID]) * StaticVariables.FRAME_RATE;

            if (Debug)
            {
                DrawingObjects.AddObject(new Line(ball.Location, ball.Location + Vector2D.FromAngleSize(ang * Math.PI / 180, 2), new Pen(Color.Violet, 0.01f)), "supprobotAngleLine2");
                DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Model.lastVelocity[RobotID], new Pen(Color.Violet, 0.01f)), "supprobotlastv");
                DrawingObjects.AddObject(new StringDraw("suppA: " + A, new Position2D(-0.8, 0)));
                DrawingObjects.AddObject(new StringDraw("suppAsize: " + A.Size, new Position2D(-0.6, 0)));
            }

            Vector2D vc = Vector2D.Zero;
            if (A.Size <= amax)
                vc = VRobotInField;
            else
            {
                A.NormalizeTo(amax);
                vc = Model.lastVelocity[RobotID] + A * StaticVariables.FRAME_PERIOD;
            }
            if (Debug)
            {
                DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location + Model.lastVelocity[RobotID], Model.OurRobots[RobotID].Location + Model.lastVelocity[RobotID] + A, new Pen(Color.Blue, 0.01f)), "suppAc");
                DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + vc, new Pen(Color.Black, 0.01f)), "suppvfinalcmd");

                DrawingObjects.AddObject(new StringDraw("suppvnew: " + vc, new Position2D(-1, 0)));
                DrawingObjects.AddObject(new StringDraw("supplastv: " + Model.lastVelocity[RobotID], new Position2D(-1.2, 0)));
            }
            V = GameParameters.InRefrence(vc, Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1));
            Planner.Add(RobotID, new SingleWirelessCommand(new Vector2D(V.X, V.Y), ww, false, 0, 0, false, false), false);
            return;
            #endregion
        }

        private void Reset()
        {
            firstInref = true;
            IntegralT2 = 0;
            lastPIDangular = 0;
            lastErr = 0;
            iErr = 0;
        }

        private double AngularController(WorldModel Model, int RobotID, double angle)
        {
            double Kp = 6, Ki = 0/*0.05*/, Kd = 0.07/*0.32*/, lamda = 0.99;

            double err = (angle - Model.OurRobots[RobotID].Angle.Value) * Math.PI / 180;

            if (err > Math.PI)
                err -= 2 * Math.PI;
            else if (err < -Math.PI)
                err += 2 * Math.PI;
            if (firstInref)
                lastErr = err;
            double dErr = (err - lastErr) / StaticVariables.FRAME_PERIOD;
            iErr = iErr * lamda + err * StaticVariables.FRAME_PERIOD;
            lastErr = err;
            if (iErr > MaxIntegral)
                iErr = MaxIntegral;
            else if (iErr < -MaxIntegral)
                iErr = -MaxIntegral;


            double outPut = Kp * err + Ki * iErr + Kd * dErr;
            return -outPut;
        }

        private void OutGoingSupport(WorldModel Model, int RobotID, int ActiveID, Position2D Target, Position2D SupportTarget, double behindBallDist)
        {
            double BallSpeedCoef = 0.9;
            double BallDistanceTresh = 0.6;
            double AngleTresh = Math.PI / 4;
            double segmentConst = 0.7;
            if (outGoingState == 1)
                Target = Model.BallState.Location + (Model.BallState.Location - SupportTarget);
            DrawingObjects.AddObject(new StringDraw("suppstate out: " + outGoingState, new Position2D(1, 0)));
            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLocation = Model.BallState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = Target - ballLocation;

            Vector2D targetRobot = Model.OurRobots[RobotID].Location - Target;
            Vector2D targetRobotInRef = GameParameters.InRefrence(targetRobot, -ballTargetVec);
            Vector2D ballRobotInRef = GameParameters.InRefrence(-robotBallVec, -ballTargetVec);
            Vector2D ballActiveInRef = GameParameters.InRefrence(Model.OurRobots[ActiveID].Location - Model.BallState.Location, -ballTargetVec);

            double trackBackBall = behindBallDist;//Math.Max(Math.Min(0.2 + Model.OurRobots[ActiveID].Location.DistanceFrom(Model.BallState.Location), 0.8), behindBallDist);
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(trackBackBall);


            bool b = Math.Sign(Vector2D.AngleBetweenInDegrees(Model.OurRobots[RobotID].Location - Model.BallState.Location, ballTargetVec)) != Math.Sign(Vector2D.AngleBetweenInDegrees(Model.OurRobots[ActiveID].Location - Model.BallState.Location, ballTargetVec));
            Position2D inter =Position2D.Zero;
            
            bool isB2w = (new Line(Model.OurRobots[RobotID].Location,backBallPoint).IntersectWithLine(new Line(Model.BallState.Location,Model.OurRobots[ActiveID].Location),ref inter) 
                && (inter - Model.OurRobots[ActiveID].Location).InnerProduct(inter - Model.BallState.Location) < 0
                && (inter - Model.OurRobots[RobotID].Location).InnerProduct(inter - backBallPoint) < 0); //Math.Sign(ballRobotInRef.Y) == Math.Sign(ballActiveInRef.Y) && Math.Abs(ballActiveInRef.Y) < Math.Abs(ballRobotInRef.Y) && Math.Abs(ballActiveInRef.X) >= Math.Abs(ballRobotInRef.X);//(Model.OurRobots[ActiveID].Location - Model.OurRobots[RobotID].Location).InnerProduct(Model.BallState.Location - Model.OurRobots[RobotID].Location) > 0 && (Model.OurRobots[ActiveID].Location - Model.BallState.Location).InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0;

            double distActfromRef = new Line(Model.BallState.Location, Model.OurRobots[RobotID].Location).Distance(Model.OurRobots[ActiveID].Location);
            double maxRearDistance = (!b && isB2w) ? distActfromRef + 0.1 : 0.12;//(Model.OurRobots[ActiveID].Location.DistanceFrom(Model.BallState.Location) < 0.25 + 0.2)? Model.OurRobots[ActiveID].Location.DistanceFrom(Model.BallState.Location) +0.2:0.25;
      
            double rearDistance = maxRearDistance;

            rearDistance = Math.Min(maxRearDistance, maxRearDistance * Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) / (Math.PI / 2));
           
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;

            Line ballRobotLine = new Line(Model.BallState.Location, Model.OurRobots[RobotID].Location);
            Position2D Intersect = Position2D.Zero;
            ballRobotLine.IntersectWithLine(ballRobotLine.PerpenducilarLineToPoint(Model.OurRobots[ActiveID].Location), ref Intersect);
            Vector2D tmpV = robotBallVec;
            if (!b && isB2w)
                tmpV = Intersect - Model.OurRobots[RobotID].Location;

            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(tmpV.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            midPoint.DrawColor = Color.Magenta;
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(midPoint, 0.01, new Pen(Color.Magenta, 0.01f)), "suppmidpointsOutGoingS");
                DrawingObjects.AddObject(backBallPoint, "suppbackBallPointsOutGoingS");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBackBallVec, new Pen(Color.YellowGreen, 0.01f)), "supprobotbackballlinesOutGoingS");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBallVec, new Pen(Color.Red, 0.01f)), "supprobotballlinesOutGoingS");
                DrawingObjects.AddObject(new Line(ballLocation, ballLocation + ballTargetVec, new Pen(Color.Blue, 0.01f)), "suppballtargetlinesOutGoingS");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p1lineOutGoingS");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p2lineOutGoingS");
                DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2), new Pen(Color.YellowGreen, 0.01f)), "supprobotAngleLineOutGoingS");
            }
            Position2D finalPosToGo = midPoint;

            if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < AngleTresh)
            {
                if (Debug)
                    DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.2, new Pen(Color.Black, 0.01f)), "suppInRefsOutGoingS");
                outGoingState = 1;
            }
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }
            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.03, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGo1sOutGoingS");
            double extendCoef = BallSpeedCoef;

            if (targetRobotInRef.Y - ballTargetVec.Size >= 0.1)
            {
                extendCoef = Math.Min(BallSpeedCoef, Math.Abs(Vector2D.AngleBetweenInRadians(ballSpeed, ballTargetVec)) / (Math.PI / 2) * BallSpeedCoef);
            }
            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size + extendCoef * ballSpeed.Size);

            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.05, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGo2sOutGoingS");


            Planner.Add(RobotID, new SingleObjectState(finalPosToGo, Vector2D.Zero, (float)(Model.BallState.Location - SupportTarget).AngleInDegrees), PathType.UnSafe, false, AvoidRobots, AvoidDangerZone);
        }
     
        private void IncommingSupportTurnBall(WorldModel Model, int RobotID, int ActiveID, Position2D activeTarget, Position2D Target, bool far, Position2D incomPoint)
        {
            double BallSpeedCoef = 0.9;
            double BallDistanceTresh = 0.6;
            double AngleTresh = Math.PI / 4;
            double segmentConst = 0.7;
            double maxRearDistance = 0.1;
            double trackBackBall = 0.5;
            
            if (outGoingState == 1)
                activeTarget = Model.BallState.Location + (Model.BallState.Location - Target);

            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLocation = Model.BallState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballActiveTargetVec = activeTarget - ballLocation;

            Vector2D targetRobot = Model.OurRobots[RobotID].Location - activeTarget;
            Vector2D targetRobotInRef = GameParameters.InRefrence(targetRobot, -ballActiveTargetVec);

            double rearDistance = maxRearDistance;

            rearDistance = Math.Min(maxRearDistance, maxRearDistance * Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballActiveTargetVec)) / (Math.PI / 2));
            Position2D backBallPoint = ballLocation - ballActiveTargetVec.GetNormalizeToCopy(trackBackBall);

            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;

            Vector2D tmp1 = robotBallVec.GetNormalizeToCopy(robotBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (Model.OurRobots[ActiveID].Location.DistanceFrom(p2) > Model.OurRobots[ActiveID].Location.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            midPoint.DrawColor = Color.Magenta;
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(midPoint, 0.01, new Pen(Color.Magenta, 0.01f)), "suppmidpointsIncTurnBall");
                DrawingObjects.AddObject(backBallPoint, "suppbackBallPointsIncTurnBall");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBackBallVec, new Pen(Color.YellowGreen, 0.01f)), "supprobotbackballlinesIncTurnBall");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBallVec, new Pen(Color.Red, 0.01f)), "supprobotballlinesIncTurnBall");
                DrawingObjects.AddObject(new Line(ballLocation, ballLocation + ballActiveTargetVec, new Pen(Color.Blue, 0.01f)), "suppballtargetlinesIncTurnBall");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p1lineIncTurnBall");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p2lineIncTurnBall");
                DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2), new Pen(Color.YellowGreen, 0.01f)), "supprobotAngleLineIncTurnBall");
            }
            Position2D finalPosToGo = midPoint;
          
         
            if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballActiveTargetVec)) < AngleTresh)
            {
                if (Debug)
                    DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.2, new Pen(Color.Black, 0.01f)), "suppInRefsIncTurnBall");
                outGoingState = 1;
            }
      
            Vector2D robotMidPointVec = finalPosToGo - robotLocation;
            double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
            if (Math.Abs(Angle) < Math.PI / 15)
                finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);

            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.03, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGo1sIncTurnBall");
            double extendCoef = BallSpeedCoef;

            if (targetRobotInRef.Y - ballActiveTargetVec.Size >= 0.1)
            {
                extendCoef = Math.Min(BallSpeedCoef, Math.Abs(Vector2D.AngleBetweenInRadians(ballSpeed, ballActiveTargetVec)) / (Math.PI / 2) * BallSpeedCoef);
            }
            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size + extendCoef * ballSpeed.Size);

            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.05, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGo2sIncTurnBall");

            Planner.Add(RobotID, new SingleObjectState(finalPosToGo, Vector2D.Zero, (float)ballActiveTargetVec.AngleInDegrees), PathType.UnSafe, false, AvoidRobots, AvoidDangerZone, false);
        }
     
        private void IncommingSupportTurnRobot(WorldModel Model, int RobotID, int ActiveID, Position2D activeTarget, Position2D Target, bool far, Position2D incomPoint)
        {
            double BallSpeedCoef = 0.9;
            double BallDistanceTresh = 0.6;
            double AngleTresh = Math.PI / 4;
            double segmentConst = 0.7;
            double maxRearDistance = 0.15;
            double trackBackBall = 0.5;

            if (outGoingState == 1)
                activeTarget = Model.OurRobots[ActiveID].Location + (Model.OurRobots[ActiveID].Location - Target);

            Vector2D activeSpeed = Model.OurRobots[ActiveID].Speed;
            Position2D activeLocation = Model.OurRobots[ActiveID].Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotActiveVec = activeLocation - robotLocation;
            Vector2D activeActiveTargetVec = activeTarget - activeLocation;

            Vector2D activeTargetRobot = Model.OurRobots[RobotID].Location - activeTarget;
            Vector2D activeTargetRobotInRef = GameParameters.InRefrence(activeTargetRobot, -activeActiveTargetVec);

            double rearDistance = maxRearDistance;

            rearDistance = Math.Min(maxRearDistance, maxRearDistance * Math.Abs(Vector2D.AngleBetweenInRadians(robotActiveVec, activeActiveTargetVec)) / (Math.PI / 2));
            Position2D backActivePoint = activeLocation - activeActiveTargetVec.GetNormalizeToCopy(trackBackBall);

            Vector2D robotBackActiveVec = backActivePoint - robotLocation;
            Vector2D activeBackActiveVec = backActivePoint - activeLocation;

            Vector2D tmp1 = robotBackActiveVec.GetNormalizeToCopy(robotBackActiveVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (Model.BallState.Location.DistanceFrom(p2) > Model.BallState.Location.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            midPoint.DrawColor = Color.Magenta;
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(midPoint, 0.01, new Pen(Color.Magenta, 0.01f)), "suppmidpointIncTurnRobot");
                DrawingObjects.AddObject(backActivePoint, "suppbackBallPointIncTurnRobot");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBackActiveVec, new Pen(Color.YellowGreen, 0.01f)), "supprobotbackballlineIncTurnRobot");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotActiveVec, new Pen(Color.Red, 0.01f)), "supprobotballlineIncTurnRobot");
                DrawingObjects.AddObject(new Line(activeLocation, activeLocation + activeActiveTargetVec, new Pen(Color.Blue, 0.01f)), "suppballtargetlineIncTurnRobot");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p1lineIncTurnRobot");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "supptmp1p2lineIncTurnRobot");
                DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2), new Pen(Color.YellowGreen, 0.01f)), "supprobotAngleLineIncTurnRobot");
            }
            Position2D finalPosToGo = midPoint;

            if (Model.OurRobots[ActiveID].Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians(robotActiveVec, activeActiveTargetVec)) < AngleTresh)
            {
                if (Debug)
                    DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.2, new Pen(Color.Black, 0.01f)), "suppInRefIncTurnRobot");
                outGoingState = 1;
                return;
            }
            //else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, activeSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + activeSpeed.GetNormalizeToCopy((activeLocation - robotLocation).Size);
            }
            
            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.03, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGoIncTurnRobot");
            double extendCoef = BallSpeedCoef;

            if (activeTargetRobotInRef.Y - activeActiveTargetVec.Size >= 0.1)
            {
                extendCoef = Math.Min(BallSpeedCoef, Math.Abs(Vector2D.AngleBetweenInRadians(activeSpeed, activeActiveTargetVec)) / (Math.PI / 2) * BallSpeedCoef);
            }
            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backActivePoint - Model.OurRobots[RobotID].Location).Size + extendCoef * activeSpeed.Size);

            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.05, new Pen(Color.Magenta, 0.01f)), "suppfinalPosToGoIncTurnRobot");
   
            Planner.Add(RobotID, new SingleObjectState(finalPosToGo, Vector2D.Zero, (float)activeActiveTargetVec.AngleInDegrees), PathType.UnSafe, false, AvoidRobots, AvoidDangerZone, false);
        }
  
        private bool IsBehindBallAndActiveIncomming(WorldModel Model, int RobotID, int ActiveID, Position2D Target, Position2D activeTarget, bool far, Position2D incomPoint, out bool turnBall)
        {
            Line ballActiveLine = new Line(Model.BallState.Location, Model.OurRobots[ActiveID].Location);
            Line robotTargetLine = new Line(Model.OurRobots[RobotID].Location, Target);
            Position2D inter = Position2D.Zero;
            turnBall = false;
            if (ballActiveLine.IntersectWithLine(robotTargetLine, ref inter) && (inter - ballActiveLine.Head).InnerProduct(inter - ballActiveLine.Tail) < 0 )
            {
                turnBall = Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < Model.OurRobots[ActiveID].Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                return false;
            }
            return true;
        }

        private bool IsBehindBallAndActive(WorldModel Model, int RobotID, int ActiveID, Position2D Target, Position2D activeTarget)
        {
            Vector2D activeBall = Model.BallState.Location - Model.OurRobots[ActiveID].Location;
            //Vector2D robotTarget = Target - Model.OurRobots[RobotID].Location;
            Vector2D ballTarget = Target - Model.BallState.Location;
            //Vector2D ballActiveTarget = - ;
            Vector2D refrence = Model.BallState.Location-activeTarget;
            Vector2D ballRobot = (Model.OurRobots[RobotID].Location - Model.BallState.Location);
            Vector2D ballActive = (Model.OurRobots[ActiveID].Location - Model.BallState.Location);

            Line activeBallLine = new Line(Model.BallState.Location, Model.OurRobots[ActiveID].Location, new Pen(Color.Red,0.01f));
            Line robotTargetLine = new Line(Target, Model.OurRobots[RobotID].Location, new Pen(Color.Blue, 0.01f));
            Line ballTargetLine = new Line(Model.BallState.Location, Target, new Pen(Color.Brown, 0.01f));
            Line ballActiveTargetLine = new Line(Model.BallState.Location, activeTarget, new Pen(Color.Black, 0.01f));
            if (Debug)
            {
                DrawingObjects.AddObject(activeBallLine, "suppactiveBallLine");
                DrawingObjects.AddObject(robotTargetLine, "supprobotTargetLine");
                DrawingObjects.AddObject(ballTargetLine, "suppballTargetLine");
                DrawingObjects.AddObject(ballActiveTargetLine, "suppballActiveTargetLine");
            }
            Position2D inter = Position2D.Zero;
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("supp1", new Position2D(-2, -1.9)));
            }

            Vector2D ballTargetInRef = GameParameters.InRefrence(ballTarget, refrence);
            double sgnBallTarget = Math.Sign(ballTargetInRef.Y);
            ballTargetInRef *= sgnBallTarget;
            Vector2D ballRobotInRef = GameParameters.InRefrence(ballRobot, refrence) * sgnBallTarget;
            Vector2D ballActiveInRef = GameParameters.InRefrence(-activeBall, refrence) * sgnBallTarget;

            if (!(ballRobotInRef.Y >= 0 && ballRobotInRef.Y >= ballActiveInRef.Y))
            {
                double tmpSgn = Math.Sign(ballTargetInRef.Y);

                double angB2wTarAndAct = Vector2D.AngleBetweenInDegrees(ballTarget, ballActive);
                double angB2wTarAndRef = Vector2D.AngleBetweenInDegrees(ballTarget, refrence * tmpSgn);
                double angB2wRobotAndAct = Vector2D.AngleBetweenInDegrees(ballRobot, ballActive);
                bool eq = Math.Sign(angB2wTarAndAct) == Math.Sign(angB2wTarAndRef) && Math.Sign(angB2wTarAndRef) == Math.Sign(angB2wRobotAndAct);
                if (!((tmpSgn >= 0 && eq) || (eq && tmpSgn < 0 && Math.Sign(angB2wTarAndRef) != Math.Sign(Vector2D.AngleBetweenInDegrees(ballActive, refrence * tmpSgn)))))
                    return false;
            }
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("supp2", new Position2D(-2, -1.7)));
            }
            if(Model.BallState.Speed.Size > ActiveParameters.staticBallSpeedTresh)
            {
                Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormnalizedCopy(), new Pen(Color.DarkGoldenrod, 0.01f));
                if (Debug)
                {
                    DrawingObjects.AddObject(ballSpeedLine, "suppballspeedline");
                }
                if (robotTargetLine.IntersectWithLine(ballSpeedLine, ref inter) && (inter - robotTargetLine.Head).InnerProduct(inter - robotTargetLine.Tail) < 0)
                {
                    inter.DrawColor = Color.DarkViolet;
                    if (Debug)
                    {
                        DrawingObjects.AddObject(inter, "supprobottargetballspeedline");
                    }
                    Vector2D ballInter = inter - Model.BallState.Location;
                    if (ballInter.InnerProduct(Model.BallState.Speed) > 0)
                        return false;
                }
            }
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("supp3", new Position2D(-2, -1.5)));
            }
            Obstacles obs = new Obstacles(Model);
            //obs.AddRobot(Model.OurRobots[ActiveID].Location, true, ActiveID);
            obs.AddBall(Model.BallState.Location, false);
            if (obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(Target, Vector2D.Zero, 0), 0.08))
                return false;
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, 0.15, new Pen(Color.DarkViolet, 0.01f)));
            }
            return true;
        }
      
        enum State
        {
            OutGoing = 0,
            GotoPoint = 1,
            Incomming  = 2
        }
    }
}
