using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

using GetBallState = MRL.SSL.AIConsole.Engine.NormalSharedState.GetBallState;

namespace MRL.SSL.AIConsole.Skills
{
    public class GetBallSkill : SkillBase
    {
        public GetBallSkill()
        {
        }
        double MaxIntegral;
        double MaxIntegralBack;
        bool Debug = true;
        bool inNear = false;

        Vector2D FieldSize = new Vector2D(7.1, 5.6);

        public bool InNear
        {
            get
            {
                return inNear;
            }
        }
        bool outgoingSide = false;
        private bool AvoidOurDangerZone = true;
        private bool AvoidOppDangerZone = true;
        GetBallState CurrentState = 0;

        public GetBallState CurrState
        {
            get
            {
                return CurrentState;
            }
        }
        //     ActiveParameters parameters = new ActiveParameters();
        OneTouchSkill oneTouch = new OneTouchSkill();
        Position2D incommingPred = Position2D.Zero;

        public Position2D IncommingPred
        {
            get
            {
                return incommingPred;
            }
        }
        bool firstInref = true;

        Vector2D lastV = new Vector2D();
        MRL.SSL.Planning.MotionPlanner.PID pidSideX = new MRL.SSL.Planning.MotionPlanner.PID();
        MRL.SSL.Planning.MotionPlanner.PID pidSideY = new MRL.SSL.Planning.MotionPlanner.PID();
        MRL.SSL.Planning.MotionPlanner.PID pidBackX = new MRL.SSL.Planning.MotionPlanner.PID();
        MRL.SSL.Planning.MotionPlanner.PID pidBackY = new MRL.SSL.Planning.MotionPlanner.PID();

        //double DefrentionalT = 0, LastDr = 0, IntegralT2 = 0,lastPIDangular = 0;

        double lastErr = 0, iErr = 0;
        double lastErrBack = 0, iErrBack = 0;
        double ourDangerMarg = 0.3;
        double oppDangerMarg = 0.0;
        double ourSafeRad = 0.2;
        double oppSafeRad = 0.0;

        bool firstInActive = true;

        Vector2D MaxSpeed = new Vector2D(5, 5);
        int counterAngle = 0;
        int counterAngleBack = 0;
        static double lastm = 0;
        bool resetOt = false;
        int stateCounter = 0;
        int incomingFarCounter = 0;
        double iDist = 0;
        bool inBackIncoming = false;
        Position2D lastTail = Position2D.Zero, lastHead = Position2D.Zero;
        Position2D? lastInter = null;
        bool inRefrence = false;
        Position2D? lastIncomingPred = null;
        double incomingGoDist = 0;
        bool incomingInit = true;
        bool incomingStopChange = false;
        Position2D firstPosIncoming = new Position2D();
        double CalculatedPassSpeed = -1;
        public void SetAvoidDangerZone(bool avoidOur, bool avoidOpp)
        {
            AvoidOurDangerZone = avoidOur;
            AvoidOppDangerZone = avoidOpp;
        }

        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID, ref Position2D Target, SingleObjectState passerState, bool isPassChip, double passSpeed, bool isFromAPassState, MRL.SSL.AIConsole.Engine.NormalSharedState.ActivePassKind passKind, bool useDefaultBackBall = true, double backBall = 0.1, bool ballPlacement = false)
        {
            DrawingObjects.AddObject(new StringDraw(passKind.ToString(), Position2D.Zero + new Vector2D(0.35, 0.25)), "passkind");
            //      Planner.ChangeDefaulteParams(robotID, false);
            //        Planner.SetParameter(robotID, 7, 4);
            DetermineNextState(Model, robotID, Target, isFromAPassState, passSpeed, isPassChip);
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw(CurrentState.ToString(), Position2D.Zero.Extend(0.5, 0)));
            }
            resetOt = true;
            if (CurrentState == GetBallState.Incomming)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw((inNear) ? "Near" : "Far", Position2D.Zero + new Vector2D(0.25, 0.25)));
                double nearP = Model.BallState.Location.DistanceFrom(Model.OurRobots[robotID].Location) / Model.BallState.Speed.Size;
                Vector2D robotBall = (isFromAPassState) ? Model.BallState.Location - NormalSharedState.CommonInfo.PassTarget : Model.BallState.Location - Model.OurRobots[robotID].Location;
                Vector2D robotTarget = (isFromAPassState) ? Target - NormalSharedState.CommonInfo.PassTarget : Target - Model.OurRobots[robotID].Location;

                double sp = (isFromAPassState && !isPassChip) ? passSpeed : Model.BallState.Speed.Size;
                double ang = Vector2D.AngleBetweenInDegrees(robotBall, robotTarget);

                DrawingObjects.AddObject(new StringDraw("ang: " + ang, new Position2D(2, 1.5) + new Vector2D(0.25, 0)), "angstr");
                DrawingObjects.AddObject(new StringDraw("sp: " + sp, new Position2D(2, 1.5) + new Vector2D(0.5, 0)), "spstr");
                DrawingObjects.AddObject(new StringDraw("nearP: " + nearP, new Position2D(2, 1.5) + new Vector2D(0.75, 0)), "angStr");

                if (inNear)
                {
                    //if (isFromAPassState)
                    //    oneTouch.Perform(engine, Model, robotID, new SingleObjectState(NormalSharedState.CommonInfo.PassTarget, Vector2D.Zero, 0), passerState, isPassChip, Target, 0, false, true, passSpeed);
                    //else
                    //    oneTouch.Perform(engine, Model, robotID, passerState, isPassChip, Target, 0, false, true, passSpeed);
                    if (isFromAPassState)
                        oneTouch.PerformActive(engine, Model, robotID, new SingleObjectState(NormalSharedState.CommonInfo.PassTarget, Vector2D.Zero, 0), passerState, isPassChip, Target, 0, false, OneTouchMode.Random, passSpeed);
                    else
                        oneTouch.PerformActive(engine, Model, robotID, Model.OurRobots[robotID], passerState, isPassChip, Target, 0, false, OneTouchMode.Random, Math.Max(CalculatedPassSpeed, 0));
                    incommingPred = oneTouch.OneTouchPoint;
                    resetOt = false;
                }
                else
                {
                    IncomingFar(engine, Model, robotID, Target);
                }
                //     OutGoingBackTrack(engine, Model, robotID, Target);
            }
            else if (CurrentState == GetBallState.Outgoing)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw((outgoingSide) ? "Side" : "Back", Position2D.Zero + new Vector2D(0.25, 0.25)));
                if (outgoingSide)
                {
                    OutGoingSideTrack(Model, robotID, Target, useDefaultBackBall, backBall, ballPlacement );
                }
                else
                {
                    OutGoingBackTrack(engine, Model, robotID, Target, useDefaultBackBall, backBall, ballPlacement);
                }
            }
            else if (CurrentState == GetBallState.Static)
            {
                OutGoingSideTrack(Model, robotID, Target, useDefaultBackBall, backBall);
            }
            if (resetOt)
            {
                oneTouch = new OneTouchSkill();
                oneTouch.Reset();
            }
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID, Position2D Target, bool useDefaultBackBall = true, double backBall = 0.1, bool ballPlacement = false)
        {
            Perform(engine, Model, robotID, ref Target, null, false, 0, false, NormalSharedState.ActivePassKind.OneTouch, useDefaultBackBall, backBall, ballPlacement);
        }

        public void PerformForStrategy(GameStrategyEngine engine, WorldModel Model, int robotID, Position2D Target, bool useDefaultBackBall = true, double backBall = 0.1)
        {
            //Planner.ChangeDefaulteParams(robotID, false);
            //Planner.SetParameter(robotID, 8, 4);
            DetermineNextState(Model, robotID, Target, false, 0, false);
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw(CurrentState.ToString(), Position2D.Zero));
            }
            resetOt = true;
            if (CurrentState == GetBallState.Incomming)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw((inNear) ? "Near" : "Far", Position2D.Zero + new Vector2D(0.25, 0.25)));
                if (inNear)
                {
                    oneTouch.Perform(engine, Model, robotID, null, false, Target, 0, false, false);
                    incommingPred = oneTouch.OneTouchPoint;
                    resetOt = false;
                }
                else
                {
                    IncomingFarForStrategy(engine, Model, robotID, Target);
                }
                //     OutGoingBackTrack(engine, Model, robotID, Target);
            }
            else if (CurrentState == GetBallState.Outgoing)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw((outgoingSide) ? "Side" : "Back", Position2D.Zero + new Vector2D(0.25, 0.25)));
                if (outgoingSide)
                {
                    OutGoingSideTrack(Model, robotID, Target, useDefaultBackBall, backBall);
                }
                else
                {
                    OutGoingBackTrack(engine, Model, robotID, Target, useDefaultBackBall, backBall);
                }
            }
            else if (CurrentState == GetBallState.Static)
            {
                OutGoingSideTrack(Model, robotID, Target, useDefaultBackBall, backBall);
            }
            if (resetOt)
                oneTouch = new OneTouchSkill();
        }
        public void PerformStatic(GameStrategyEngine engine, WorldModel Model, int robotID, Position2D Target)
        {
            DetermineNextState(Model, robotID, Target, false, 0, false);
            if (Debug)
                DrawingObjects.AddObject(new StringDraw(CurrentState.ToString(), Position2D.Zero));
            resetOt = true;
            if (CurrentState == GetBallState.Incomming)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw((inNear) ? "Near" : "Far", Position2D.Zero + new Vector2D(0.25, 0.25)));
                if (inNear)
                {
                    oneTouch.Perform(engine, Model, robotID, null, false, Target, 0, false);
                    resetOt = false;
                }
                else
                {
                    IncomingFar(engine, Model, robotID, Target);
                }
            }
            else if (CurrentState == GetBallState.Outgoing)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw((outgoingSide) ? "Side" : "Back", Position2D.Zero + new Vector2D(0.25, 0.25)));
                if (outgoingSide)
                {
                    OutGoingSideTrack(Model, robotID, Target);
                }
                else
                {
                    OutGoingBackTrack(engine, Model, robotID, Target);
                }
            }
            else if (CurrentState == GetBallState.Static)
            {
                Static(Model, robotID, Target);
            }
            if (resetOt)
                oneTouch = new OneTouchSkill();
        }

        private void DetermineNextState(WorldModel Model, int RobotID, Position2D Target, bool isFromAPassState, double PassSpeed, bool isPassChip)
        {
            //bool ReadyKick = BinaryShootingFunction(Model, RobotID, (GameParameters.OppGoalRight - Model.BallState.Location).AngleInRadians, (GameParameters.OppGoalLeft - Model.BallState.Location).AngleInRadians);
            //if (ReadyKick)
            //{
            //    DrawingObjects.AddObject(new StringDraw("KICK", Model.OurRobots[RobotID].Location.Extend(.6, 0)), "dsfsd");
            //}
            //if (!ReadyKick)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Don't KICK", Model.OurRobots[RobotID].Location.Extend(.6, 0)), "54654");
            //}

            SingleObjectState robot = Model.OurRobots[RobotID];
            SingleObjectState ball = Model.BallState;
            Vector2D ballRobot = robot.Location - ball.Location;

            if (CurrentState == GetBallState.Incomming)
            {
                if (firstInActive)
                {
                    if (!BallKickedToUs(Model, RobotID, Target, ref inNear, isFromAPassState, PassSpeed, isPassChip))
                    {
                        if (ball.Speed.Size > ActiveParameters.staticBallSpeedTresh)
                            CurrentState = GetBallState.Outgoing;
                        else
                            CurrentState = GetBallState.Static;
                    }
                }
                else
                {
                    if (ball.Speed.Size < ActiveParameters.staticBallSpeedTresh)
                        stateCounter++;
                    else
                        stateCounter = Math.Max(stateCounter - 2, 0);

                    if (stateCounter > 2)
                        CurrentState = GetBallState.Static;
                    if (ballRobot.InnerProduct(ball.Speed) < 0)
                        CurrentState = GetBallState.Outgoing;
                }
                Line line = new Line();
                line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
                List<Position2D> poses2 = new Circle(Model.OurRobots[RobotID].Location, ActiveParameters.nearIncomingRadi).Intersect(line);
                if (Debug)
                    foreach (var item in poses2)
                    {
                        if (Debug)
                            DrawingObjects.AddObject(item);
                    }
                //Vector2D robotTarget = Target - robot.Location;
                //Vector2D robotBall = Model.BallState.Location - Model.OurRobots[RobotID].Location;
                Vector2D robotBall = (isFromAPassState) ? Model.BallState.Location - NormalSharedState.CommonInfo.PassTarget : Model.BallState.Location - Model.OurRobots[RobotID].Location;
                Vector2D robotTarget = (isFromAPassState) ? Target - NormalSharedState.CommonInfo.PassTarget : Target - Model.OurRobots[RobotID].Location;


                double d = Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                double v = (Model.BallState.Speed.Size > 0.01) ? Model.BallState.Speed.Size : 0.01;

                if (poses2.Count > 0 && Model.BallState.Speed.Size > ActiveParameters.nearBallSpeedTresh && Math.Abs(Vector2D.AngleBetweenInDegrees(robotBall, robotTarget)) < 78)
                {
                    if (!inNear && (d / v) > 0.55 && (d / v) < 1.2)
                    {
                        inNear = true;
                    }
                }
                else
                    inNear = false;
            }
            else if (CurrentState == GetBallState.Outgoing)
            {
                if (ball.Speed.Size < ActiveParameters.staticBallSpeedTresh && outgoingSide)
                {
                    stateCounter++;
                }
                else
                    stateCounter = Math.Max(stateCounter - 2, 0);

                if (stateCounter > 6)
                    CurrentState = GetBallState.Static;

                if (BallKickedToUs(Model, RobotID, Target, ref inNear, isFromAPassState, PassSpeed, isPassChip) && ball.Location.DistanceFrom(robot.Location) > ActiveParameters.incomingBallDistanceTresh)
                    CurrentState = GetBallState.Incomming;

            }
            else if (CurrentState == GetBallState.Static)
            {

                if (BallKickedToUs(Model, RobotID, Target, ref inNear, isFromAPassState, PassSpeed, isPassChip))
                    CurrentState = GetBallState.Incomming;
                else if (ball.Speed.Size > ActiveParameters.staticBallSpeedTresh)
                    CurrentState = GetBallState.Outgoing;
            }

            if (Math.Abs(Vector2D.AngleBetweenInDegrees(/*ballTarget*/Target - Model.OurRobots[RobotID].Location, ball.Speed)) > ActiveParameters.outgoingSideAngleTresh)
                outgoingSide = false;
            else
                outgoingSide = true;

            //inNear = false;
            //CurrentState = GetBallState.Incomming;

            if (CurrentState != GetBallState.Incomming)
            {
                inNear = false;
                incommingPred = Position2D.Zero;
            }

            if (inNear || CurrentState != GetBallState.Incomming)
            {
                ResetIncomingFar();
            }
            if (!inNear || CurrentState != GetBallState.Incomming)
            {
                CalculatedPassSpeed = -1;
            }
            if (CurrentState == GetBallState.Incomming && inNear)
            {
                if (Model.BallState.Speed.Size > CalculatedPassSpeed)
                {
                    CalculatedPassSpeed = Model.BallState.Speed.Size;
                }
            }
            firstInActive = false;
        }

        private bool BallKickedToUs(WorldModel Model, int RobotID, Position2D Target, ref bool isNear, bool isFromAPassState, double PassSpeed, bool isPassChip)
        {
            Line line = new Line();
            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
            Position2D BallGoal = line.CalculateY(Model.OurRobots[RobotID].Location.X);

            if (Model.BallState.Speed.Size > ActiveParameters.kickedToUsBallSpeedTresh)
            {
                if (Model.BallState.Speed.InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0)
                {
                    List<Position2D> poses = new Circle(Model.OurRobots[RobotID].Location, ActiveParameters.kickedToUsRadi).Intersect(line);

                    List<Position2D> poses2 = new Circle(Model.OurRobots[RobotID].Location, ActiveParameters.nearIncomingRadi).Intersect(line);
                    if (Debug)
                        foreach (var item in poses2)
                        {
                            DrawingObjects.AddObject(item);
                        }
                    double nearP = Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) / Model.BallState.Speed.Size;
                    Vector2D robotBall = (isFromAPassState) ? Model.BallState.Location - NormalSharedState.CommonInfo.PassTarget : Model.BallState.Location - Model.OurRobots[RobotID].Location;
                    Vector2D robotTarget = (isFromAPassState) ? Target - NormalSharedState.CommonInfo.PassTarget : Target - Model.OurRobots[RobotID].Location;
                    double sp = (isFromAPassState && !isPassChip) ? PassSpeed : Model.BallState.Speed.Size;
                    double ang = Vector2D.AngleBetweenInDegrees(robotBall, robotTarget);
                    if (poses2.Count > 0 && ((nearP > 0.55 && nearP < 1.2 && sp > ActiveParameters.nearBallSpeedTresh)) && Math.Abs(ang) < 78)
                        isNear = true;
                    else
                    {
                        ;
                    }
                    if (poses.Count > 0)
                        return true;
                }
            }
            return false;
        }
        int ctr = 0;
        public void OutGoingSideTrack(WorldModel Model, int RobotID, Position2D Target, bool useDefaultBackBall = true, double backB = 0.1, bool ballPlacement = false)
        {
            double BallSpeedCoef = 0.9;
            double BallDistanceTresh = 0.5;
            double AngleTresh = Math.PI / 4;
            double AngleTresh2 = Math.PI / 3;
            double segmentConst = 0.8;
            double maxRearDistance = 0.19;
            double trackBackBall = 0.1;
            double refrenceBackBall = 0.07;
            if (!useDefaultBackBall)
            {
                trackBackBall = backB;
                refrenceBackBall = backB;
            }

            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLocation = Model.BallState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = Target - ballLocation;

            Vector2D targetRobot = Model.OurRobots[RobotID].Location - Target;
            Vector2D targetRobotInRef = GameParameters.InRefrence(targetRobot, -ballTargetVec);
            SingleObjectState ball = Model.BallState, targetInRefrence = new SingleObjectState();
            SingleObjectState robot = new SingleObjectState(Model.OurRobots[RobotID]), robotInRefrence = new SingleObjectState();

            Vector2D ballTarget = Target - ball.Location;
            double rearDistance = maxRearDistance;
            double dr = Vector2D.offset_to_line(ball.Location, Target, robot.Location);
            double drbt = (Math.Max(Math.Min(0.8, dr), 0.00) - 0.00) / 0.8;
            double drbtAng = (Math.Max(Math.Min(0.2, dr), 0.00) - 0.00) / 0.2;
            if (targetRobotInRef.Y - ballTargetVec.Size >= 0.16)
            {

                rearDistance = drbt * maxRearDistance;
                //  DrawingObjects.AddObject(new StringDraw("dr: " + dr, Model.BallState.Location + new Vector2D(1,-1)),"drbt");
                // rearDistance = Math.Min(maxRearDistance, Math.Abs(Vector2D.AngleBetweenInRadians(/*ballSpeed*/robotBallVec, ballTargetVec)) / (Math.PI / 2) * maxRearDistance);
            }



            // rearDistance = Math.Min(maxRearDistance, maxRearDistance * Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) / (Math.PI / 2));

            Position2D backBallPoint = ((ballLocation + 0.0 * Model.BallState.Speed) - ballTargetVec.GetNormalizeToCopy(trackBackBall));
            backBallPoint = ExtractBackBallFromOurZone(backBallPoint);
            backBallPoint = ExtractBackBallFromOppZone(backBallPoint);
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;

            //Position2D tmpBackBallPoint = ((ballLocation) - ballTargetVec.GetNormalizeToCopy(trackBackBall));
            //Vector2D tmpRobotBackBallVec = tmpBackBallPoint - robotLocation;
            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);//robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
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
                DrawingObjects.AddObject(new Circle(midPoint, 0.01, new Pen(Color.Magenta, 0.01f)), "midpoint");
                DrawingObjects.AddObject(backBallPoint, "backBallPoint");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBackBallVec, new Pen(Color.YellowGreen, 0.01f)), "robotbackballline");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBallVec, new Pen(Color.Red, 0.01f)), "robotballline");
                DrawingObjects.AddObject(new Line(ballLocation, ballLocation + ballTargetVec, new Pen(Color.Blue, 0.01f)), "balltargetline");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "tmp1p1line");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "tmp1p2line");
                DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2), new Pen(Color.YellowGreen, 0.01f)), "robotAngleLine");
            }
            Position2D finalPosToGo = midPoint;
            //double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            //double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            //    double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));
            Position2D Target2GO = new Position2D();
            //    Vector2D FieldSize = new Vector2D(2.10, 3.15);

            Position2D backBall = ball.Location - ballTarget.GetNormalizeToCopy(refrenceBackBall);

            backBall = ExtractBackBallFromOurZone(backBall);
            backBall = ExtractBackBallFromOppZone(backBall);

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
            double dv = Model.OurRobots[RobotID].Speed.Size - Model.BallState.Speed.Size;

            if (!inRefrence)
            {
                if ((Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < AngleTresh && dY < -0.1)
                    || (Math.Abs(dx) < 0.031 && dY < -0.07))
                {
                    ctr = 0;
                    inRefrence = true;
                }
            }
            if (inRefrence)
            {
                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) >= BallDistanceTresh || Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) >= AngleTresh2 || (dY >= -0.06
                && Math.Abs(dx) >= 0.07))
                    inRefrence = false;

                if ((dY >= -0.1 && Math.Abs(dx) >= 0.05))
                    ctr++;
                else
                    ctr = Math.Max(0, ctr - 2);

                if (ctr > 15)
                {
                    inRefrence = false;

                }
                DrawingObjects.AddObject(new StringDraw("ctr: " + ctr.ToString(), Model.BallState.Location + new Vector2D(1, 1)), "ctrstr");
            }
            //inRefrence = false;
            if (inRefrence)
            {
                if (Debug)
                    DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.2, new Pen(Color.Black, 0.01f)), "InRef");
                SideCalculateInRefrence(Model, RobotID, Target, refrenceBackBall, ballPlacement);
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
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.03, new Pen(Color.Magenta, 0.01f)), "finalPosToGo1");
            double extendCoef = BallSpeedCoef;

            if (targetRobotInRef.Y - ballTargetVec.Size >= 0.1)
            {
                extendCoef = Math.Min(BallSpeedCoef, Math.Abs(Vector2D.AngleBetweenInRadians(ballSpeed, ballTargetVec)) / (Math.PI / 2) * BallSpeedCoef);
            }
            double tmpd = 4.2 * (Math.Max(0.4, Math.Min(Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location), 4)) - 0.4) / 3.6;
            DrawingObjects.AddObject(new StringDraw("angleCoef: " + extendCoef.ToString(), new Position2D(-3, 2)), "fsd564fdslk");
            DrawingObjects.AddObject(new StringDraw("distanceCoef: " + tmpd.ToString(), new Position2D(-3.2, 2)), "fsdf897dslk");
            extendCoef += tmpd;
            DrawingObjects.AddObject(new StringDraw("ballSpeedCoef: " + extendCoef.ToString(), new Position2D(-3.4, 2)), "fsdfdslk");
            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size + extendCoef * ballSpeed.Size);

            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.05, new Pen(Color.Magenta, 0.01f)), "finalPosToGo2");
            firstInref = true;
            double extAng = Math.Sign(Vector2D.AngleBetweenInDegrees(ballTarget, -robotBallVec)) * drbtAng * 30 * 0;
            DrawingObjects.AddObject(new Line(ballLocation, ballLocation + Vector2D.FromAngleSize((ballTargetVec.AngleInRadians + extAng).ToRadian(), ballTargetVec.Size), new Pen(Color.Cyan, 0.01f)), "balltargetline2");
            Planner.Add(RobotID, new SingleObjectState(finalPosToGo, Vector2D.Zero, (float)(ballTargetVec.AngleInDegrees + extAng)), PathType.UnSafe, false, true, AvoidOurDangerZone, AvoidOppDangerZone);
        }
        //public void OutGoingSideTrackNew(WorldModel Model, int RobotID, Position2D Target)
        //{
        //    double BallSpeedCoef = 0.9;
        //    double BallDistanceTresh = 0.6;
        //    double SpeedTresh = -1;
        //    double AngleTresh = Math.PI / 4;
        //    double AngleTresh2 = Math.PI / 3;
        //    double segmentConst = 0.7;
        //    double maxRearDistance = 0.15;
        //    double trackBackBall = 0.09;
        //    double refrenceBackBall = 0.03;

        //    Vector2D ballSpeed = Model.BallState.Speed;
        //    double maxd = 1;
        //    double d = Math.Min(maxd, Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location)) / maxd;
        //    Position2D ballLocation = Model.BallState.Location + d * 0.7 * Model.BallState.Speed;
        //    Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
        //    Position2D robotLocation = Model.OurRobots[RobotID].Location;
        //    Vector2D robotBallVec = ballLocation - robotLocation;
        //    Vector2D ballTargetVec = Target - ballLocation;

        //    Vector2D targetRobot = Model.OurRobots[RobotID].Location - Target;
        //    Vector2D targetRobotInRef = GameParameters.InRefrence(targetRobot, -ballTargetVec);

        //    double rearDistance = maxRearDistance;
        //    if (targetRobotInRef.Y - ballTargetVec.Size >= 0.1)
        //    {
        //        rearDistance = Math.Min(maxRearDistance, Math.Abs(Vector2D.AngleBetweenInRadians(/*ballSpeed*/robotBallVec, ballTargetVec)) / (Math.PI / 4) * maxRearDistance);
        //    }

        //    SingleObjectState ball = Model.BallState, targetInRefrence = new SingleObjectState();
        //    SingleObjectState robot = new SingleObjectState(Model.OurRobots[RobotID]), robotInRefrence = new SingleObjectState();

        //    Vector2D ballTarget = Target - ball.Location;
        //    //rearDistance = Math.Min(maxRearDistance, maxRearDistance * Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) / (Math.PI / 2));
        //    Position2D backBallPoint = ((ballLocation + 0.0 * Model.BallState.Speed) - ballTargetVec.GetNormalizeToCopy(trackBackBall));

        //    backBallPoint = ExtractBackBallFromOurZone(backBallPoint);
        //    backBallPoint = ExtractBackBallFromOppZone(backBallPoint);

        //    Vector2D robotBackBallVec = backBallPoint - robotLocation;
        //    Vector2D ballBackBallVec = backBallPoint - ballLocation;

        //    Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
        //    Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
        //    Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
        //    Position2D midPoint = p1;
        //    if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
        //    {
        //        midPoint = p2;
        //    }
        //    midPoint.DrawColor = Color.Magenta;
        //    if (Debug)
        //    {
        //        DrawingObjects.AddObject(new Circle(midPoint, 0.01, new Pen(Color.Magenta, 0.01f)), "midpoint");
        //        DrawingObjects.AddObject(backBallPoint, "backBallPoint");
        //        DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBackBallVec, new Pen(Color.YellowGreen, 0.01f)), "robotbackballline");
        //        DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBallVec, new Pen(Color.Red, 0.01f)), "robotballline");
        //        DrawingObjects.AddObject(new Line(ballLocation, ballLocation + ballTargetVec, new Pen(Color.Blue, 0.01f)), "balltargetline");
        //        DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "tmp1p1line");
        //        DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "tmp1p2line");
        //        DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2), new Pen(Color.YellowGreen, 0.01f)), "robotAngleLine");
        //    }
        //    Position2D finalPosToGo = midPoint;
        //    //double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
        //    //double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

        //    //    double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));
        //    Position2D Target2GO = new Position2D();
        //    Vector2D ballTargetRef = (Target - Model.BallState.Location);
        //    Position2D backBallRef = Model.BallState.Location - ballTargetRef.GetNormalizeToCopy(refrenceBackBall);//ball.Location - ballTarget.GetNormalizeToCopy(refrenceBackBall);

        //    backBallRef = ExtractBackBallFromOurZone(backBallRef);
        //    backBallRef = ExtractBackBallFromOppZone(backBallRef);

        //    Vector2D robotBackBall = backBallRef - robot.Location;
        //    //Line l = new Line(ball.Location, Target, new Pen(Color.Wheat, 0.02f));
        //    //Line l2 = l.PerpenducilarLineToPoint(robot.Location);
        //    //Position2D? intersect = l.IntersectWithLine(l2);
        //    Target2GO = backBallRef;

        //    Vector2D Reference = -ballTargetRef;
        //    double angleRefrence = Reference.AngleInRadians;

        //    Target2GO.X = Math.Min(Math.Abs(Target2GO.X), FieldSize.X) * Math.Sign(Target2GO.X);
        //    Target2GO.Y = Math.Min(Math.Abs(Target2GO.Y), FieldSize.Y) * Math.Sign(Target2GO.Y);

        //    Position2D tempPos2 = new Position2D();
        //    tempPos2.X = Target2GO.Y * Math.Cos(angleRefrence) - Target2GO.X * Math.Sin(angleRefrence);
        //    tempPos2.Y = Target2GO.X * Math.Cos(angleRefrence) + Target2GO.Y * Math.Sin(angleRefrence);
        //    Target2GO = tempPos2;


        //    Position2D tempPos1 = new Position2D();
        //    tempPos1.X = robot.Location.Y * Math.Cos(angleRefrence) - robot.Location.X * Math.Sin(angleRefrence);
        //    tempPos1.Y = robot.Location.X * Math.Cos(angleRefrence) + robot.Location.Y * Math.Sin(angleRefrence);
        //    robot.Location = tempPos1;
        //    double dx = Target2GO.X - robot.Location.X, dY = Target2GO.Y - robot.Location.Y;
        //    double dv = Model.OurRobots[RobotID].Speed.Size - Model.BallState.Speed.Size;
        //    if (!inRefrence /* && dv > SpeedTresh */&& Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians((Model.OurRobots[RobotID].Location - Model.BallState.Location), ballTargetRef)) < AngleTresh || (Math.Abs(dx) < 0.1 && dY < 0))
        //        inRefrence = true;
        //    else if (inRefrence && !(Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < AngleTresh2 || (Math.Abs(dx) < 0.15 && dY < 0)))
        //        inRefrence = false;
        //    //inRefrence = true;
        //    if (inRefrence)
        //    {
        //        if (Debug)
        //            DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.2, new Pen(Color.Black, 0.01f)), "InRef");
        //        SideCalculateInRefrence(Model, RobotID, Target, refrenceBackBall);
        //        return;
        //    }
        //    else
        //    {
        //        Vector2D robotMidPointVec = finalPosToGo - robotLocation;
        //        double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
        //        if (Math.Abs(Angle) < Math.PI / 15)
        //            finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((Model.BallState.Location - robotLocation).Size);
        //    }
        //    pidSideX.Reset();
        //    pidSideY.Reset();
        //    Reset();
        //    if (Debug)
        //        DrawingObjects.AddObject(new Circle(finalPosToGo, 0.03, new Pen(Color.Magenta, 0.01f)), "finalPosToGo1");
        //    double extendCoef = BallSpeedCoef;

        //    if (targetRobotInRef.Y - ballTargetVec.Size >= 0.1)
        //    {
        //        extendCoef = Math.Min(BallSpeedCoef, Math.Abs(Vector2D.AngleBetweenInRadians(ballSpeed, ballTargetRef)) / (Math.PI / 2) * BallSpeedCoef);
        //    }
        //    finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size + extendCoef * ballSpeed.Size);

        //    if (Debug)
        //        DrawingObjects.AddObject(new Circle(finalPosToGo, 0.05, new Pen(Color.Magenta, 0.01f)), "finalPosToGo2");
        //    firstInref = true;

        //    Planner.Add(RobotID, new SingleObjectState(finalPosToGo, Vector2D.Zero, (float)ballTargetVec.AngleInDegrees), PathType.UnSafe, false, true, AvoidOurDangerZone, AvoidOppDangerZone);
        //}
        public void OutGoingBackTrack(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, bool useDefaultBackBall = true, double backB = 0.1, bool ballPlacement = false)
        {
            double segmentConst = 0.65;
            double rearDistance = 0.09;
            double trackBackBall = 0.09;
            double refrenceBackBall = 0.01;
            double BallDistanceTresh = 0.3;
            double AngleTresh = Math.PI / 4;
            if (!Position2D.IsBetween(Target, Model.BallState.Location, Model.OurRobots[RobotID].Location))
                rearDistance = 0.09;
            if (!useDefaultBackBall)
            {
                trackBackBall = backB;
                refrenceBackBall = backB;
            }
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("1", new Position2D(1, -1)));
            }
            Position2D predictedPoint = BallPredictedOutGoing(engine, Model, RobotID, Target);
            Vector2D BallRobot = Model.OurRobots[RobotID].Location - Model.BallState.Location;
            Vector2D BallPredicted = predictedPoint - Model.BallState.Location;
            Position2D ballLocation = predictedPoint;
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("2", new Position2D(1, -1.1)));
            }
            Position2D Left = GameParameters.OppGoalLeft;
            Position2D Right = GameParameters.OppGoalLeft;
            //Vector2D ballSpeed = Model.BallState.Speed;

            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = Target - ballLocation;

            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("3", new Position2D(1, -1.2)));
            }
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(trackBackBall);
            backBallPoint = ExtractBackBallFromOurZone(backBallPoint);
            backBallPoint = ExtractBackBallFromOppZone(backBallPoint);

            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("4", new Position2D(1, -1.3)));
            }

            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);

            Position2D midPoint = p1;

            if (Model.BallState.Location.DistanceFrom(p2) > Model.BallState.Location.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            Line lr = new Line(midPoint, Model.OurRobots[RobotID].Location);
            Position2D intr = Position2D.Zero;
            if (lr.PerpenducilarLineToPoint(Model.BallState.Location).IntersectWithLine(lr, ref intr) && intr.DistanceFrom(Model.BallState.Location) < rearDistance)
            {
                Position2D hp = Model.BallState.Location + (intr - Model.BallState.Location).GetNormalizeToCopy(rearDistance);
                midPoint = Model.OurRobots[RobotID].Location + (hp - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((midPoint - Model.OurRobots[RobotID].Location).Size);
            }
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("5", new Position2D(1, -1.4)));
            }
            midPoint.DrawColor = Color.Magenta;
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(midPoint, 0.01, new Pen(Color.Magenta, 0.01f)), "midpoint");
                DrawingObjects.AddObject(backBallPoint, "backBallPoint");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBackBallVec, new Pen(Color.YellowGreen, 0.01f)), "robotbackballline");
                DrawingObjects.AddObject(new Line(robotLocation, robotLocation + robotBallVec, new Pen(Color.Red, 0.01f)), "robotballline");
                DrawingObjects.AddObject(new Line(ballLocation, ballLocation + ballTargetVec, new Pen(Color.Blue, 0.01f)), "balltargetline");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "tmp1p1line");
                DrawingObjects.AddObject(new Line((robotLocation + tmp1), (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance), new Pen(Color.Black, 0.01f)), "tmp1p2line");
                DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2), new Pen(Color.YellowGreen, 0.01f)), "robotAngleLine");
                DrawingObjects.AddObject(midPoint);
            }

            Position2D finalPosToGo = midPoint;
            double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            //double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("6", new Position2D(1, -1.5)));
            }
            Position2D Target2GO = new Position2D();
            //Vector2D FieldSize = new Vector2D(3.10, 2.15);

            SingleObjectState ball = Model.BallState, targetInRefrence = new SingleObjectState();
            SingleObjectState robot = new SingleObjectState(Model.OurRobots[RobotID]), robotInRefrence = new SingleObjectState();
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("7", new Position2D(1, -1.6)));
            }

            Vector2D ballTarget = Target - ball.Location;
            Position2D backBall = ball.Location - ballTarget.GetNormalizeToCopy(refrenceBackBall);
            backBall = ExtractBackBallFromOurZone(backBall);
            backBall = ExtractBackBallFromOppZone(backBall);

            Vector2D robotBackBall = backBall - robot.Location;
            Line l = new Line(ball.Location, Target, new Pen(Color.Wheat, 0.02f));
            Line l2 = l.PerpenducilarLineToPoint(robot.Location);
            Position2D? intersect = l.IntersectWithLine(l2);
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("8", new Position2D(1, -1.7)));
            }
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
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("dist" + Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location), new Position2D(1.1, -1.0)));
                DrawingObjects.AddObject(new StringDraw("Ang" + Math.Abs(Vector2D.AngleBetweenInDegrees(robotBallVec, ballTargetVec)), new Position2D(1.2, -1.0)));
                //DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + robotBallVec.GetNormalizeToCopy(1), new Pen(Color.Violet, 0.01f)), "robotballLine");
                //DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + ballTargetVec.GetNormalizeToCopy(1), new Pen(Color.Crimson, 0.01f)), "balltargetLine");
            }

            if ((Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < BallDistanceTresh && Math.Abs(Vector2D.AngleBetweenInRadians((Model.BallState.Location - Model.OurRobots[RobotID].Location), (Target - Model.BallState.Location))) < AngleTresh) || (Math.Abs(dx) < 0.4 && dY < -0.2))
            {
                if (Debug)
                    DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.2, new Pen(Color.Black, 0.01f)), "InRef");
                BackCalculateInRefrence(Model, RobotID, Target, refrenceBackBall, ballPlacement);
                return;
            }
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("12", new Position2D(1, -1.8)));
            }
            pidBackX.Reset();
            pidBackY.Reset();
            Reset();
            double extendCoef = 0.8;
            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.03, new Pen(Color.Magenta, 0.01f)), "finalPosToGo1");
            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size + extendCoef * Model.BallState.Speed.Size);
            if (Debug)
                DrawingObjects.AddObject(new Circle(finalPosToGo, 0.05, new Pen(Color.Magenta, 0.01f)), "finalPosToGo2");
            firstInref = true;
            Planner.Add(RobotID, new SingleObjectState(finalPosToGo, Vector2D.Zero, (float)ballTargetVec.AngleInDegrees), PathType.UnSafe, false, true, AvoidOurDangerZone, AvoidOppDangerZone);
        }

        private void BackCalculateInRefrence(WorldModel Model, int RobotID, Position2D Target, double BackDistance, bool ballPlacement)
        {

            pidBackY.Coef = new PIDCoef(ActiveParameters.KpBackY, ActiveParameters.KiBackY, ActiveParameters.KdBackY);


            pidBackX.Coef = new PIDCoef(ActiveParameters.KpBackX, ActiveParameters.KiBackX, ActiveParameters.KdBackX);


            Position2D Target2GO = new Position2D();
            //  Vector2D FieldSize = new Vector2D(3.10, 2.15);
            SingleObjectState stat = new SingleObjectState(Model.OurRobots[RobotID]);

            Position2D tempPos2 = new Position2D();
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("9", new Position2D(1, -1.8)));
            }
            #region HighLevel
            SingleObjectState ball = Model.BallState, targetInRefrence = new SingleObjectState();
            SingleObjectState robot = Model.OurRobots[RobotID], robotInRefrence = new SingleObjectState();

            Vector2D ballTarget = Target - ball.Location;
            Position2D backBall = ball.Location - ballTarget.GetNormalizeToCopy(BackDistance);
            backBall = ExtractBackBallFromOurZone(backBall);
            backBall = ExtractBackBallFromOppZone(backBall);

            Vector2D robotBackBall = backBall - robot.Location;
            Line l = new Line(ball.Location, Target, new Pen(Color.Wheat, 0.02f));
            Line l2 = l.PerpenducilarLineToPoint(robot.Location);
            Position2D? intersect = l.IntersectWithLine(l2);
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("10", new Position2D(1, -1.9)));
            }
            Target2GO = backBall;
            Vector2D Reference = -ballTarget;
            double angleRefrence = Reference.AngleInRadians;
            if (Debug)
                DrawingObjects.AddObject(new Line(Target2GO, Target2GO + Reference, new Pen(Color.Wheat, 0.01f)), "refrence");
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

            Position2D tempPos1 = new Position2D();
            ;
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


            double pidout = -pidBackX.Calculate(dX, 0);

            double vy = (ballPlacement) ? 0.5 : 1.5;
            double accel = (ActiveParameters.KpTotalVyBack * (1 / (Math.Abs(pidBackY.Calculate(dX, 0)) + ActiveParameters.vyOffsetBack + ActiveParameters.KpxVyBack * Math.Abs(finalSpeed.X))) + Math.Max(ActiveParameters.KpyVyBack * (finalSpeed.Y), 0));

            Vtemp = new Vector2D(pidout + ActiveParameters.KpxVxBack * finalSpeed.X, stat.Speed.Y - accel / StaticVariables.FRAME_RATE);

            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("accel" + accel, new Position2D(1.3, -1)));
                DrawingObjects.AddObject(new StringDraw("vy:" + Vtemp.Y, new Position2D(1.4, -1)));
            }

            if (Math.Abs(Vtemp.X) > MaxSpeed.X)
                Vtemp.X = Math.Sign(Vtemp.X) * Math.Abs(MaxSpeed.X);

            MaxSpeed = new Vector2D(2, Math.Abs(vy));
            if (Math.Abs(Vtemp.Y) > MaxSpeed.Y)
                Vtemp.Y = Math.Sign(Vtemp.Y) * Math.Abs(MaxSpeed.Y);

            lastV.X = Vtemp.X;
            lastV.Y = Vtemp.Y;

            if (Vtemp.Size > MaxSpeed.X)
                Vtemp.NormalizeTo(MaxSpeed.X);

            #endregion
            #region Convert In Robot Axis

            Vector2D tempRobot = new Vector2D(Vtemp.X, Vtemp.Y);
            Vector2D VRobotInField = new Vector2D();
            VRobotInField.X = tempRobot.X * Math.Cos(-angleRefrence) - tempRobot.Y * Math.Sin(-angleRefrence);
            VRobotInField.Y = tempRobot.Y * Math.Cos(-angleRefrence) + tempRobot.X * Math.Sin(-angleRefrence);


            Vector2D temp = new Vector2D(VRobotInField.X, VRobotInField.Y);
            double Rotation = Model.OurRobots[RobotID].Angle.Value;
            if (Rotation > 180)
                Rotation -= 360;
            if (Rotation < -180)
                Rotation += 360;
            Rotation *= Math.PI / (double)180;

            Vector2D V = new Vector2D();
            V.X = temp.X * Math.Cos(Rotation) - temp.Y * Math.Sin(Rotation);
            V.Y = temp.Y * Math.Cos(Rotation) + temp.X * Math.Sin(Rotation);

            double ww = AngularControllerBack(Model.lastW[RobotID], Model.OurRobots[RobotID], ballTarget.AngleInDegrees);

            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("11", new Position2D(1, -2)));
            }
            Planner.Add(RobotID, new SingleWirelessCommand(new Vector2D(V.X, V.Y), ww, false, 0, 0, false, false), false);
            return;
            #endregion
        }
        private void SideCalculateInRefrence(WorldModel Model, int RobotID, Position2D Target, double BackDistance, bool ballPlacement)
        {

            pidSideY.Coef = new PIDCoef(ActiveParameters.KpSideY, ActiveParameters.KiSideY, ActiveParameters.KdSideY);
            pidSideX.Coef = new PIDCoef(ActiveParameters.KpSideX, ActiveParameters.KiSideX, ActiveParameters.KdSideX);

            Position2D Target2GO = new Position2D();
            // Vector2D FieldSize = new Vector2D(3.10, 2.15);
            SingleObjectState stat = new SingleObjectState(Model.OurRobots[RobotID]);

            Position2D tempPos2 = new Position2D();
            #region HighLevel
            SingleObjectState ball = Model.BallState, targetInRefrence = new SingleObjectState();
            SingleObjectState robot = Model.OurRobots[RobotID], robotInRefrence = new SingleObjectState();

            Vector2D ballTarget = Target - ball.Location;
            Position2D backBall = ball.Location - ballTarget.GetNormalizeToCopy(BackDistance);

            backBall = ExtractBackBallFromOurZone(backBall);
            backBall = ExtractBackBallFromOppZone(backBall);

            Vector2D robotBackBall = backBall - robot.Location;
            Line l = new Line(ball.Location, Target, new Pen(Color.Wheat, 0.02f));
            Line l2 = l.PerpenducilarLineToPoint(robot.Location);
            Position2D? intersect = l.IntersectWithLine(l2);
            Target2GO = backBall;

            Vector2D Reference = -ballTarget;
            double angleRefrence = Reference.AngleInRadians;
            if (Debug)
                DrawingObjects.AddObject(new Line(Target2GO, Target2GO + Reference, new Pen(Color.Wheat, 0.01f)), "refrence");
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

            Position2D tempPos1 = new Position2D();
            ;
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

            double dX = Target2GO.X - stat.Location.X, dY = Target2GO.Y - stat.Location.Y, v0X = stat.Speed.X, v0Y = stat.Speed.Y, dvx = finalSpeed.X - lastV.X, dvy = finalSpeed.Y - lastV.Y;

            #endregion

            #region Applying Acceleration
            Vector2D Vtemp = Vector2D.Zero;


            double kx = 1;// -Math.Min(Math.Abs(dX), 0.1) / 0.1;
            // double vy = (ActiveParameters.KpTotalVySide * (1 / (-Math.Abs(pidSideY.Calculate(dX, 0)) - ActiveParameters.vyOffsetSide + ActiveParameters.KpxVySide * -Math.Abs(finalSpeed.X))) + Math.Min(ActiveParameters.KpyVySide * (finalSpeed.Y + dY), 0));

            //double extY = (- ActiveParameters.KpxVySide / Math.Min(0.15, Math.Max(Math.Abs(dX), 0.008)) + kx * ActiveParameters.KpyVySide * finalSpeed.Y) / pidSideY.Coef.Kp;
            double TempdX = Math.Max( Math.Abs( dX) , 0.02);
            TempdX = Math.Exp(-(TempdX - 0.02) * (TempdX - 0.02) / (ActiveParameters.KpxVySide * ActiveParameters.KpxVySide));
            double extY = ((TempdX) * kx * ActiveParameters.KpyVySide * finalSpeed.Y) / pidSideY.Coef.Kp;
            extY = Math.Min(extY, 0.15);

            //double extY = * (ActiveParameters.KpyVySide * finalSpeed.Y) / pidSideY.Coef.Kp;


            DrawingObjects.AddObject(new StringDraw("dx: " + dX.ToString(), new Position2D(-4.5, -2)), "42342355551");
            // DrawingObjects.AddObject(new StringDraw("kx: " + kx.ToString(), new Position2D(-4.7, -2)), "4234235555");
            DrawingObjects.AddObject(new StringDraw("exty: " + extY.ToString(), new Position2D(-4.9, -2)), "423423555555");
            double vy = -pidSideY.Calculate(dY + extY, 0);
            vy = Math.Min(0, vy);

            double extendedVx = 0;
            extendedVx = (Model.BallState.Speed.X * ballTarget.Y - Model.BallState.Speed.Y * ballTarget.X)/** Model.OurRobots[RobotID].Location.DistanceFrom(backBall)*/;
            if (Debug)
            {
                //DrawingObjects.AddObject(new StringDraw("dx: " + (dX), new Position2D(0.8, 0)));
                //DrawingObjects.AddObject(new StringDraw("extendedVx: " + (extendedVx), new Position2D(0.7, 0)));
                //DrawingObjects.AddObject(new StringDraw("currX: " + (dX + (ActiveParameters.KpxVxSide * finalSpeed.X * Math.Abs(dY)) / pidSideX.KP), new Position2D(0.6, 0)));
            }

            //double pidout = -pidSideX.Calculate(dX + ActiveParameters.KpxVxSide/pidSideX.KP * finalSpeed.X, 0);
            double ddx = Math.Sign(-dX) * Math.Abs(dX) * 0.0 * Math.Max(1 - Math.Abs(finalSpeed.X), 0) * Math.Min(Math.Max(Math.Abs(finalSpeed.Y), 0), 1);

            double extX = (ActiveParameters.KpxVxSide /** dX*/ * (finalSpeed.X + ddx) /** Math.Abs(Math.Sin(Model.BallState.Speed.AngleInRadians))*/) / pidSideX.Coef.Kp;
            //double extX = (ActiveParameters.KpxVxSide * Math.Min(Math.Max(stat.Location.DistanceFrom(Target2GO) * 1.6 / 0.6, 1), 1) * finalSpeed.X) / pidSideX.Coef.Kp;

            DrawingObjects.AddObject(new StringDraw("ddx: " + ddx.ToString(), new Position2D(-4.5, -1)), "423423");
            DrawingObjects.AddObject(new StringDraw("VxBall: " + finalSpeed.X.ToString(), new Position2D(-4.7, -1)), "4234233");
            if (Debug)
                CharterData.AddData("dX", -(dX));
            double pidout = -pidSideX.Calculate(dX + extX, 0);
            MaxSpeed.Y = (ballPlacement)?0.5:2.5;
            Vtemp = new Vector2D(pidout, Math.Min(Math.Abs(vy), MaxSpeed.Y) * Math.Sign(vy)); //todo: V OUT

            Vector2D p = new Vector2D(Target2GO.X + extX, Target2GO.Y + extY);
            p = GameParameters.InRefrence(p, Reference);
            Position2D pp = new Position2D(p.X, p.Y);
            pp.DrawColor = Color.LemonChiffon;
            if (Debug)
                DrawingObjects.AddObject(pp, "extendedpointPIDx");

            MaxSpeed.X = 5;

            if (Vtemp.Size > MaxSpeed.X)
                Vtemp.NormalizeTo(MaxSpeed.X);

            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Vc: " + Vtemp, new Position2D(1, 0)));
                DrawingObjects.AddObject(new StringDraw("VcSize: " + Vtemp.Size, new Position2D(1.2, 0)));
                DrawingObjects.AddObject(new StringDraw("Vx: " + pidout, new Position2D(1.3, 0)));
                DrawingObjects.AddObject(new StringDraw("Vy: " + vy, new Position2D(1.4, 0)));
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
            DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + VRobotInField, new Pen(Color.Crimson, 0.01f)), "VcomanndLine");
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
                w = (Model.BallState.Speed.X * ballTarget.Y - Model.BallState.Speed.Y * ballTarget.X) / ballTarget.Size;
            w = w.ToDegree();
            w *= Model.OurRobots[RobotID].Location.DistanceFrom(backBall);
            w *= 1.1;
            if (w > 30)
                w = 30;
            else if (w < -30)
                w = -30;
            w = 0;
            double ang = ballTarget.AngleInDegrees + w;
            double wout = AngularController(Model, RobotID, ang);
            double alfamax = 50;// 20;
            double alfa = (wout - (-Model.lastW[RobotID])) * StaticVariables.FRAME_RATE;
            double ww = wout;
            if (Math.Abs(alfa) > alfamax)
                ww = (-Model.lastW[RobotID]) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("wout: " + wout, new Position2D(-2, 0)));
                DrawingObjects.AddObject(new StringDraw("ww: " + ww, new Position2D(-2.2, 0)));
            }
            double amax = 6;
            Vector2D A = (VRobotInField - Model.lastVelocity[RobotID]) * StaticVariables.FRAME_RATE;

            if (Debug)
            {
                //    DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(ang * Math.PI / 180, 2), new Pen(Color.Violet, 0.01f)), "robotAngleLine2");
                //    DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Model.lastVelocity[RobotID], new Pen(Color.Violet, 0.01f)), "robotlastv");
                //    DrawingObjects.AddObject(new StringDraw("A: " + A, new Position2D(-0.8, 0)));
                //    DrawingObjects.AddObject(new StringDraw("Asize: " + A.Size, new Position2D(-0.6, 0)));
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
                //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location + Model.lastVelocity[RobotID], Model.OurRobots[RobotID].Location + Model.lastVelocity[RobotID] + A, new Pen(Color.Blue, 0.01f)), "Ac");
                //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + vc, new Pen(Color.Black, 0.01f)), "vfinalcmd");

                //DrawingObjects.AddObject(new StringDraw("vnew: " + vc, new Position2D(-1, 0)));
                //DrawingObjects.AddObject(new StringDraw("lastv: " + Model.lastVelocity[RobotID], new Position2D(-1.2, 0)));
            }
            V = GameParameters.InRefrence(vc, Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1));
            Planner.Add(RobotID, new SingleWirelessCommand(new Vector2D(V.X, V.Y)/*new Vector2D()*/, ww, false, 0, 0, false, false), false);
            return;
            #endregion
        }

        private double CalculateTime(SingleObjectState RobotState, Position2D Target, double maxAccel, double maxSpeed)
        {
            Vector2D vec = Target - RobotState.Location;
            double d = vec.Size;
            double angle = Vector2D.AngleBetweenInRadians(vec, RobotState.Speed);
            double SpeedInRefrence = RobotState.Speed.Size * Math.Cos(angle);
            double time = 0;

            double deccelDX = Math.Min(1, .5 * RobotState.Location.DistanceFrom(Target));
            double daccel = Math.Min(1, .5 * RobotState.Location.DistanceFrom(Target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);

            double rootp = GoalieSkills.GoalieDiveSkill2017.root(2.2, Math.Abs(RobotState.Speed.Size), deccelDX);
            if (rootp > 0)
            {
                time = rootp * 2;
                return time;
            }
            return 0;
        }
        public void IncomingFar(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target)
        {
            if (incomingInit)
            {
                incomingInit = false;
                firstPosIncoming = Model.OurRobots[RobotID].Location;
            }
            DrawingObjects.AddObject(new Circle(firstPosIncoming, 0.2, new Pen(Color.Green, 0.03f)));
            Position2D predictedBall = Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, firstPosIncoming);//BallPredictedIncomingNew(engine, Model, RobotID, lastIncomingPred);//BallPredictedIncoming(engine, Model, RobotID, lastIncomingPred, 0);
            //predictedBall.X = Math.Sign(predictedBall.X) * Math.Min(Math.Abs(predictedBall.X), GameParameters.OurGoalCenter.X - 0.1);
            //predictedBall.Y = Math.Sign(predictedBall.Y) * Math.Min(Math.Abs(predictedBall.Y), GameParameters.OurLeftCorner.Y - 0.1);
            double fieldMargin = -0.1;
            Line BallLine = new Line(Model.BallState.Location, predictedBall);
            predictedBall += (-Model.BallState.Speed).GetNormalizeToCopy(incomingGoDist - ActiveParameters.IncomingBackBall);
            double timeR = CalculateTime(Model.OurRobots[RobotID], predictedBall, 4, 3.7);
            double timeB = (predictedBall.DistanceFrom(Model.BallState.Location) / Model.BallState.Speed.Size);

            int stopRefreshingFrame = 40;
            int treshDifranceFrame = 10;
            if (timeB - (treshDifranceFrame / StaticVariables.FRAME_RATE) < timeR && timeB > stopRefreshingFrame / StaticVariables.FRAME_RATE)
            {
                incomingGoDist -= Model.BallState.Speed.Size * StaticVariables.FRAME_PERIOD * 2;
            }
            else if (timeB - (treshDifranceFrame / StaticVariables.FRAME_RATE) < timeR && timeB < stopRefreshingFrame / StaticVariables.FRAME_RATE)
            {
                if (timeB < timeR)
                {
                    incomingGoDist -= 0.10;
                }
            }
            if (timeB - (treshDifranceFrame / StaticVariables.FRAME_RATE) > timeR && timeB > stopRefreshingFrame / StaticVariables.FRAME_RATE)
            {
                incomingGoDist += Model.BallState.Speed.Size * StaticVariables.FRAME_PERIOD * 3;
            }
            if (incomingGoDist != 0)
                //incomingGoDist = (incomingGoDist > 0) ? Math.Min(2.5, incomingGoDist) : Math.Max(-2.5, incomingGoDist);
                //if (timeB > 1.2 || incomingFarCounter < 10)
                //{
                //    incomingFarCounter++;
                //    //double tresh = 0.09 * timeB - timeR
                //    if (lastIncomingPred.HasValue && (timeB - .3 < timeR)) //&& Model.OurRobots[RobotID].Location.DistanceFrom(lastIncomingPred.Value) < 0.10 )
                //    {
                //        incomingGoDist -= 0.09;
                //    }
                //    else if (lastIncomingPred.HasValue && (timeB - 0.3 > timeR))
                //    {
                //        incomingGoDist += 0.09;
                //    }

                //}
                DrawingObjects.AddObject(new StringDraw(incomingGoDist.ToString(), Color.Red, new Position2D(+.5, -2)));
            DrawingObjects.AddObject(new StringDraw("timeB= " + timeB + "     " + "TimeR" + timeR, Color.Red, new Position2D(+.6, -2)));
            lastIncomingPred = predictedBall;
            if (!GameParameters.IsInField(predictedBall, fieldMargin))
            {
                Position2D tmpInt = new Position2D();
                double minDist = double.MaxValue;
                Position2D NearestIntersect = new Position2D();
                List<Line> field = GameParameters.GetFieldLines(fieldMargin);
                foreach (var item in field)
                {
                    if (!item.IntersectWithLine(BallLine, ref tmpInt))
                        tmpInt = Model.BallState.Location;
                    if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                    {
                        double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                        if (dist < minDist)
                        {
                            NearestIntersect = tmpInt;
                            minDist = dist;
                        }
                    }
                }
                if (minDist < double.MaxValue)
                {
                    predictedBall = NearestIntersect;
                }
                else
                    predictedBall = Model.BallState.Location;
                DrawingObjects.AddObject(new Circle(predictedBall, 0.2, new Pen(Color.Red, 0.03f)));

            }
            if (Debug)
                DrawingObjects.AddObject(new Circle(predictedBall, 0.03)
                {
                    DrawPen = new Pen(Color.Chocolate, 0.01f)
                });
            Position2D backBall = predictedBall;//+ (predictedBall - Target).GetNormalizeToCopy(ActiveParameters.IncomingBackBall);
            incommingPred = predictedBall;
            if (AvoidOppDangerZone)
            {
                double dist, DistFromBorder;
                if (GameParameters.IsInDangerousZone(backBall, true, 0.1, out dist, out DistFromBorder))
                {
                    backBall = GameParameters.OppGoalCenter + (backBall - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-backBall, Vector2D.Zero, 0), 0));
                }
            }
            DrawingObjects.AddObject("angleinconmfar", new StringDraw("ang incoming" + (Model.BallState.Location - backBall).AngleInDegrees.ToString(), new Position2D(-2.5, -2.5)));
            Planner.Add(RobotID, new SingleObjectState(backBall, Vector2D.Zero, (float)(Model.BallState.Location - backBall).AngleInDegrees), PathType.UnSafe, false, true, AvoidOurDangerZone, false);
        }
        //public void IncomingFar(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target)
        //{

        //    Position2D predictedBall = BallPredictedIncomingNew(engine, Model, RobotID, lastIncomingPred);//BallPredictedIncoming(engine, Model, RobotID, lastIncomingPred, 0);
        //    lastIncomingPred = predictedBall;
        //    //predictedBall.X = Math.Sign(predictedBall.X) * Math.Min(Math.Abs(predictedBall.X), GameParameters.OurGoalCenter.X - 0.1);
        //    //predictedBall.Y = Math.Sign(predictedBall.Y) * Math.Min(Math.Abs(predictedBall.Y), GameParameters.OurLeftCorner.Y - 0.1);
        //    double fieldMargin = -0.1;
        //    Line BallLine = new Line(Model.BallState.Location, predictedBall);
        //    if (!GameParameters.IsInField(predictedBall, fieldMargin))
        //    {
        //        Position2D tmpInt = new Position2D();
        //        double minDist = double.MaxValue;
        //        Position2D NearestIntersect = new Position2D();
        //        List<Line> field = GameParameters.GetFieldLines(fieldMargin);
        //        foreach (var item in field)
        //        {
        //            if (!item.IntersectWithLine(BallLine, ref tmpInt))
        //                tmpInt = Model.BallState.Location;
        //            if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
        //            {
        //                double dist = Model.BallState.Location.DistanceFrom(tmpInt);
        //                if (dist < minDist)
        //                {
        //                    NearestIntersect = tmpInt;
        //                    minDist = dist;
        //                }
        //            }
        //        }
        //        if (minDist < double.MaxValue)
        //        {
        //            predictedBall = NearestIntersect;
        //        }
        //        else
        //            predictedBall = Model.BallState.Location;
        //    }
        //    if (Debug)
        //        DrawingObjects.AddObject(new Circle(predictedBall, 0.03)
        //        {
        //            DrawPen = new Pen(Color.Chocolate, 0.01f)
        //        });
        //    Position2D backBall = predictedBall+ (predictedBall - Target).GetNormalizeToCopy(ActiveParameters.IncomingBackBall);
        //    incommingPred = predictedBall;
        //    if (AvoidOppDangerZone)
        //    {
        //        double dist, DistFromBorder;
        //        if (GameParameters.IsInDangerousZone(backBall, true, 0.1, out dist, out DistFromBorder))
        //        {
        //            backBall = GameParameters.OppGoalCenter + (backBall - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-backBall, Vector2D.Zero, 0), 0));
        //        }
        //    }
        //    DrawingObjects.AddObject("angleinconmfar", new StringDraw("ang incoming" + (Model.BallState.Location - backBall).AngleInDegrees.ToString(), new Position2D(-2.5, -2.5)));
        //    Planner.Add(RobotID, new SingleObjectState(backBall, Vector2D.Zero, (float)(Model.BallState.Location - backBall).AngleInDegrees), PathType.UnSafe, false, true, AvoidOurDangerZone, false);
        //}
        public void IncomingFarForStrategy(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target)
        {
            Position2D predictedBall = BallPredictedIncoming(engine, Model, RobotID, lastIncomingPred, 1);
            lastIncomingPred = predictedBall;
            //predictedBall.X = Math.Sign(predictedBall.X) * Math.Min(Math.Abs(predictedBall.X), GameParameters.OurGoalCenter.X - 0.1);
            //predictedBall.Y = Math.Sign(predictedBall.Y) * Math.Min(Math.Abs(predictedBall.Y), GameParameters.OurLeftCorner.Y - 0.1);
            double fieldMargin = -0.1;
            Line BallLine = new Line(Model.BallState.Location, predictedBall);
            if (!GameParameters.IsInField(predictedBall, fieldMargin))
            {
                Position2D tmpInt = new Position2D();
                double minDist = double.MaxValue;
                Position2D NearestIntersect = new Position2D();
                List<Line> field = GameParameters.GetFieldLines(fieldMargin);
                foreach (var item in field)
                {
                    if (!item.IntersectWithLine(BallLine, ref tmpInt))
                        tmpInt = Model.BallState.Location;
                    if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                    {
                        double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                        if (dist < minDist)
                        {
                            NearestIntersect = tmpInt;
                            minDist = dist;
                        }
                    }
                }
                if (minDist < double.MaxValue)
                {
                    predictedBall = NearestIntersect;
                }
                else
                    predictedBall = Model.BallState.Location;
            }
            if (Debug)
                DrawingObjects.AddObject(new Circle(predictedBall, 0.03)
                {
                    DrawPen = new Pen(Color.Chocolate, 0.01f)
                });
            Position2D backBall = predictedBall + (predictedBall - Target).GetNormalizeToCopy(ActiveParameters.IncomingBackBall);
            incommingPred = predictedBall;
            Planner.Add(RobotID, new SingleObjectState(backBall, Vector2D.Zero, (float)(Target - backBall).AngleInDegrees), PathType.UnSafe, false, true, AvoidOurDangerZone, AvoidOppDangerZone);
        }
        public SingleWirelessCommand Static(WorldModel Model, int RobotID, Position2D Target)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            //    Controller.DontGoInDangerZone = true;
            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLocation = /*CalculateBall(Model, RobotID);*/Model.BallState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = Target - ballLocation;
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(0.09);
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;
            double segmentConst = 0.7;
            double rearDistance = 0.15;
            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            Position2D finalPosToGo = midPoint;
            double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

            if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
                finalPosToGo = backBallPoint;
            else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }

            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);

            //  SWC = Controller.CalculateTargetSpeed(Model, RobotID, finalPosToGo, ballTargetVec.AngleInDegrees, null);
            Planner.Add(RobotID, finalPosToGo, (float)ballTargetVec.AngleInDegrees, PathType.UnSafe, false, false, AvoidOurDangerZone, AvoidOppDangerZone);
            return SWC;
        }

        private Position2D BallPredictedOutGoing(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target)
        {
            Position2D cathingPoint = Position2D.Zero;
            if (Position2D.IsBetween(Target, Model.BallState.Location, Model.OurRobots[RobotID].Location))
            {
                if (engine.GameInfo.OurTeam.CatchBallLines.ContainsKey(RobotID) && engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
                    cathingPoint = Position2D.Interpolate(engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head, engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Tail, 0);
                else
                    cathingPoint = Model.BallState.Location;
            }
            else
                cathingPoint = Model.PredictedBall[.5].Location;
            cathingPoint = Position2D.Interpolate(Model.BallState.Location, cathingPoint, 0.9);
            //cathingPoint = Model.PredictedBall[0.5].Location;
            cathingPoint.X = Math.Sign(cathingPoint.X) * Math.Min(Math.Abs(cathingPoint.X), GameParameters.OurGoalCenter.X - 0.1);
            cathingPoint.Y = Math.Sign(cathingPoint.Y) * Math.Min(Math.Abs(cathingPoint.Y), GameParameters.OurLeftCorner.Y - 0.1);
            if (Debug)
                DrawingObjects.AddObject(new Circle(cathingPoint, 0.05)
                {
                    DrawPen = new Pen(Color.Purple, 0.01f)
                });


            return cathingPoint;
        }
        private Position2D BallPredictedIncoming(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D? lastPredict, int mode)
        {
            incomingFarCounter++;
            double d = Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location));
            double bmaxSpeed = 1, bminSpeed = 0.1, minK = 0.12, maxK = 0.35,
                k = (Math.Min(1, Math.Max(0, d - bminSpeed) / (bmaxSpeed - bminSpeed))) * (maxK - minK) + minK;
            k = 0.15;
            if (lastPredict.HasValue)
            {
                if (!lastInter.HasValue)
                {
                    var intes = new Circle(Model.OurRobots[RobotID].Location, RobotParameters.OurRobotParams.Diameter / 2 + ((mode == 0) ? 0.04 : 0.05)).Intersect(new Line(Model.BallState.Location, lastPredict.Value));

                    if (intes.Count > 0)
                    {
                        lastInter = (intes.Count == 1) ? intes[0] : Position2D.Interpolate(intes[0], intes[1], 0.5);
                        // (lastPredict.Value - Model.BallState.Location).PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location);
                        return lastInter.Value;
                    }
                }
                else
                {
                    var intes = new Circle(Model.OurRobots[RobotID].Location, RobotParameters.OurRobotParams.Diameter / 2 + ((mode == 0) ? 0.04 : 0.05)).Intersect(new Line(Model.BallState.Location, lastInter.Value));

                    if (intes.Count > 0)
                        return lastInter.Value;

                }
            }
            lastInter = null;
            if (lastPredict.HasValue && incomingFarCounter > 25)
                return Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, lastPredict.Value);
            if (engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
            {
                if (Debug)
                {
                    DrawingObjects.AddObject(engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head);
                    DrawingObjects.AddObject(engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Tail);
                }
                Position2D pp = Position2D.Interpolate(engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head, engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Tail, k);
                return Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, pp);
            }
            lastInter = null;
            return Model.BallState.Location;
            Position2D robot = Model.OurRobots[RobotID].Location;
            Position2D ball = Model.BallState.Location;
            Vector2D ballRobot = robot - ball;

            List<double> errTime = new List<double>(), robotTime = new List<double>();
            List<Position2D> ballPath = new List<Position2D>();
            double a = 5;
            Vector2D ballSpeed = Model.BallState.Speed;
            Line ballLine = new Line(Model.BallState.Location, Model.BallState.Location + ballSpeed);
            Position2D inter = Position2D.Zero;
            if (!ballLine.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location).IntersectWithLine(ballLine, ref inter))
                inter = Model.BallState.Location;
            Vector2D robotInter = inter - Model.OurRobots[RobotID].Location;
            bool hasLast = lastPredict.HasValue;
            Position2D lastP = (hasLast) ? lastPredict.Value : Position2D.Zero;
            Position2D p = Model.BallState.Location + Model.BallState.Speed;
            if (engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
            {
                var hed = engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head;

                var teil = engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Tail;

                Position2D perp = Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location);
                //if ((hed - perp).InnerProduct(Model.BallState.Location - perp) < 0)
                //    p = perp;
                //else
                //    p = Position2D.Interpolate(hed, teil, 0.15/*1.0 - (double)Math.Min(incomingFarCounter, maxCounter) / (double)maxCounter*/);
                Position2D tmpP = Model.BallState.Location + new Vector2D(0.1, 0.08);
                tmpP.DrawColor = Color.Blue;
                double inner = (hed - perp).InnerProduct(Model.BallState.Location - perp);
                if (inner < 0 && inBackIncoming)
                {
                    inBackIncoming = false;
                    tmpP.DrawColor = Color.Red;
                    iDist = 0;
                }
                else if (inner >= 0 && !inBackIncoming)
                {
                    inBackIncoming = true;
                    tmpP.DrawColor = Color.PowderBlue;
                    iDist = 0;
                }
                if (inner > 0)
                {
                    double ks = 0.11, ke = 0.7;
                    iDist += perp.DistanceFrom(hed) * StaticVariables.FRAME_PERIOD;
                    p = perp + ks * Model.BallState.Speed;
                    p += (hed - perp).GetNormalizeToCopy(ke * iDist);
                }
                else
                {
                    p = Position2D.Interpolate(hed, teil, 0.1);
                }
                double normMargin = 0.2;
                Position2D tmpB = Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(normMargin);
                if ((p - tmpB).InnerProduct(Model.BallState.Speed) <= 0)
                    p = Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(normMargin);

                p.DrawColor = Color.Aqua;
                teil.DrawColor = Color.DarkViolet;
                hed.DrawColor = Color.YellowGreen;
                if (Debug)
                {
                    DrawingObjects.AddObject(tmpP);
                    DrawingObjects.AddObject(p);
                    DrawingObjects.AddObject(hed);
                    DrawingObjects.AddObject(teil);
                }
            }
            if (!inBackIncoming || (inBackIncoming && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.5))
            {
                if (hasLast && (robotInter.Size < 0.1 || engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count == 0))
                    return lastP;
                else if (hasLast && robotInter.Size < 0.3 && Model.OurRobots[RobotID].Speed.InnerProduct(robotInter) > 0 && Model.BallState.Speed.Size > 0.07)
                {
                    Vector2D robotSpeedInRef = new Vector2D(0, Model.OurRobots[RobotID].Speed.Size);
                    Vector2D tmpv = lastP - Model.OurRobots[RobotID].Location;
                    if (tmpv.Size > 1e-4)
                        robotSpeedInRef = GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, tmpv);
                    double tr = robotSpeedInRef.Y / a, tb = Model.BallState.Location.DistanceFrom(lastP) / Model.BallState.Speed.Size;
                    if (tb - tr >= -0.17)
                        return lastP;
                }
            }
            return p;
            //for (double j = 0; j < maxTimeBall && m < Model.PredictedBall.states.Count; j += StaticVariables.FRAME_PERIOD, m++)
            //{

            //    Position2D ballPredict = Model.PredictedBall[j].Location;
            //    ballPath.Add(ballPredict);
            //    Vector2D ballPredictedBall = ballPredict - ball;
            //    Vector2D R = ballPredictedBall - ballRobot;
            //    double timeR = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + R, ActiveParameters.RobotMotionCoefs) * StaticVariables.FRAME_PERIOD;

            //    errTime.Add(j - timeR);
            //    robotTime.Add(timeR);
            //    if (j - timeR >= treshErr)
            //    {
            //        if (j < timeMin)
            //        {
            //            timeMin = j;
            //        }
            //    }
            //}
            //double t =Math.Min(maxTimeBall, timeMin);
            //DrawingObjects.AddObject(new StringDraw("time: " + t, new Position2D(-2,0.3)));
            //return Model.PredictedBall[t].Location;
        }

        private Position2D BallPredictedIncomingNew(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D? lastPredict)
        {
            Obstacle obs = new Obstacle();
            obs.State = new SingleObjectState(Model.OurRobots[RobotID].Location, Vector2D.Zero, 0);
            obs.R = new Vector2D(0.06, 0.06);
            obs.Type = ObstacleType.Circle;
            Position2D prependPoint = Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location);
            if (obs.Meet(new SingleObjectState(Model.BallState.Location, Vector2D.Zero, 0), new SingleObjectState(Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10), Vector2D.Zero, 0), 0.01)
                && ((lastPredict.HasValue && Position2D.IsBetween(lastPredict.Value, Model.BallState.Location, prependPoint)) || !lastPredict.HasValue))
            {
                return prependPoint;
            }
            if (Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location).DistanceFrom(Model.OurRobots[RobotID].Location) < .2)
            {
                incomingFarCounter++;
            }
            if (incomingFarCounter >= 10 && lastPredict.HasValue)
            {
                return Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, lastPredict.Value); ;
            }
            else
            {
                if (engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
                {
                    if (Debug)
                    {
                        DrawingObjects.AddObject(engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head);
                        DrawingObjects.AddObject(engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Tail);
                    }
                    Position2D pp = Position2D.Interpolate(engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head, engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Tail, 0.22);
                    return Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, pp);
                }
                else
                    return Model.BallState.Speed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location);
            }
        }
        private double AngularController(WorldModel Model, int RobotID, double angle)
        {
            //double Kp = 22, Ki = 2/*0.05*/, Kd = 3.95/*0.32*/, lamda = 0.99, PID_Max = 40;
            double Kp =9.5, Ki = 0.00/*0.05*/, Kd = 0.01/*0.008*/, lamda = 0.99, PID_Max = 40;
            
            MaxIntegral = 100;
            double err = (angle - Model.OurRobots[RobotID].Angle.Value) * Math.PI / 180;

            if (err > Math.PI)
                err -= 2 * Math.PI;
            else if (err < -Math.PI)
                err += 2 * Math.PI;
            if (counterAngle == 0)
            {
                lastErr = err;
                iErr = 0;
            }
            counterAngle++;
            double dErr = (err - lastErr) / StaticVariables.FRAME_PERIOD;
            iErr = iErr * lamda + err * StaticVariables.FRAME_PERIOD;
            lastErr = err;
            if (iErr > MaxIntegral)
                iErr = MaxIntegral;
            else if (iErr < -MaxIntegral)
                iErr = -MaxIntegral;
            //if (Debug)
            //    CharterData.AddData("errwwwwww", err);

            double outPut = Kp * err + Ki * iErr + Kd * dErr;
            if (outPut < -PID_Max)
                outPut = -PID_Max;
            if (outPut > PID_Max)
                outPut = PID_Max;
            return -outPut;

        }

        private void Reset()
        {
            firstInref = true;
            lastErr = 0;
            iErr = 0;
            lastErrBack = 0;
            iErrBack = 0;
            CalculatedPassSpeed = -1;
            counterAngle = 0;
            counterAngleBack = 0;
            incomingInit = true;
            incomingGoDist = 0;
        }
        void ResetIncomingFar()
        {
            incomingFarCounter = 0;
            iDist = 0;
            inBackIncoming = false;
            lastIncomingPred = null;
            incomingFarCounter = 0;
        }
        private bool BinaryShootingFunction(WorldModel model, int RobotID, double lowerBound, double upperBound)
        {
            double constantFraction = 0.9;
            double Robotangle = (model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
            double m = Math.Min(recursiveAngleNormalizationFunction(Robotangle - lowerBound), recursiveAngleNormalizationFunction(upperBound - Robotangle));
            byte output = 0;
            if (m < 0)
            {
                output = 0;
            }
            else if (m > (constantFraction * (upperBound - lowerBound)) / 2)
            {
                output = 1;
            }
            else if (m > lastm)
            {
                output = 0;
            }
            else
            {
                output = 1;
            }
            lastm = m;
            if (output == 0)
                return false;
            else
                return true;
        }
        private double recursiveAngleNormalizationFunction(double normalVar)
        {
            if (normalVar < -Math.PI)
            {
                return recursiveAngleNormalizationFunction(normalVar + (2 * Math.PI));
            }
            else if (normalVar > Math.PI)
            {
                return recursiveAngleNormalizationFunction(normalVar - (2 * Math.PI));
            }
            else
            {
                return normalVar;
            }
        }
        private double AngularControllerBack(double lastW, SingleObjectState state, double TargetTeta)
        {
            //double Kp = 22, Ki = 2/*0.05*/, Kd = 3.9/*0.32*/, lamda = 0.99, PID_Max = 40;
            //double Kp = 10, Ki = 0.0/*0.05*/, Kd = 0.0/*0.008*/, lamda = 0.99, PID_Max = 40;
            //double Kp = 9, Ki = 0.01/*0.05*/, Kd = 0.002/*0.008*/, lamda = 0.99, PID_Max = 40;
            double Kp = 9.5, Ki = 0.01/*0.05*/, Kd = 0.0/*0.008*/, lamda = 0.99, PID_Max = 40;
            MaxIntegralBack = 100;
            double err = (TargetTeta - state.Angle.Value) * Math.PI / 180;

            if (err > Math.PI)
                err -= 2 * Math.PI;
            else if (err < -Math.PI)
                err += 2 * Math.PI;
            if (counterAngleBack == 0)
            {
                lastErr = err;
                iErr = 0;
            }
            counterAngleBack++;
            double dErr = (err - lastErr) / StaticVariables.FRAME_PERIOD;
            iErrBack = iErrBack * lamda + err * StaticVariables.FRAME_PERIOD;
            lastErrBack = err;
            if (iErr > MaxIntegralBack)
                iErr = MaxIntegralBack;
            else if (iErr < -MaxIntegralBack)
                iErr = -MaxIntegralBack;
            //if (Debug)
            //    CharterData.AddData("errwwwwww", err);

            double outPut = Kp * err + Ki * iErrBack + Kd * dErr;
            if (outPut < -PID_Max)
                outPut = -PID_Max;
            if (outPut > PID_Max)
                outPut = PID_Max;
            outPut = -outPut;

            double alfamax = 50;
            double alfa = (outPut - (-lastW)) * StaticVariables.FRAME_RATE;
            double ww = outPut;
            if (Math.Abs(alfa) > alfamax)
                ww = (-lastW) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;

            return ww;
        }

        private Position2D ExtractBackBallFromOppZone(Position2D backBallPoint)
        {
            double dist, DistFromBorder;
            if (AvoidOppDangerZone && GameParameters.IsInDangerousZone(backBallPoint, true, oppDangerMarg, out dist, out DistFromBorder))
            {
                backBallPoint = GameParameters.OppGoalCenter + (backBallPoint - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(new Position2D(-backBallPoint.X, backBallPoint.Y), Vector2D.Zero, 0), oppSafeRad));
            }
            return backBallPoint;
        }
        private Position2D ExtractBackBallFromOurZone(Position2D backBallPoint)
        {
            double dist, DistFromBorder;
            if (AvoidOurDangerZone && GameParameters.IsInDangerousZone(backBallPoint, false, ourDangerMarg, out dist, out DistFromBorder))
            {
                backBallPoint = GameParameters.OurGoalCenter + (backBallPoint - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(backBallPoint, Vector2D.Zero, 0), ourSafeRad));
            }
            return backBallPoint;
        }

    }
}