using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Strategies
{
    public class DirectCornerStrategy:StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 100, initDist = 0.22, maxWaitTresh = 420, oppZoneMarg = 0.2;
        bool first, passTargetCalculated;
        int PasserId, PickerID, PositionerID;
        Position2D PasserPos, PickerPos, PositionerPos, PassTarget, ShootTarget, lastShooterPos, lastPasserPos, lastPositionerPos, firstBallPos;
        double PasserAngle, PickerAngle, PositionerAng, RotateTeta, PassSpeed, KickPower, ballMovedTresh = 0.07;
        int counter, finishCounter, RotateDelay, timeLimitCounter;
        Syncronizer sync;
        bool chip, passed, chipOrigin, Debug = true;
        int minOppId;
        int rotateCounter;
        bool inRotate;
        bool pickIsPossible;
        GetBallSkill getBallSkill = new GetBallSkill(); 
        Line extendedBallTarLine, oppFeasibleLine, ourFeasibleLine;
        Circle dangerC;
        Position2D center;
        
        public override void ResetState()
        {
            minOppId = -1;
            firstBallPos = Position2D.Zero;
            pickIsPossible = false;
            chip = false;
            chipOrigin = false;
            passed = false;

            rotateCounter = 2;
            inRotate = false;
            CurrentState = InitialState;
            first = true;
            passTargetCalculated = false;
            RotateTeta = 40;
            PassSpeed = Program.MaxKickSpeed;//4.5;
            KickPower = Program.MaxKickSpeed;
            timeLimitCounter = 0;
            PasserId = -1;
            PickerID = -1;
            //inPassState = false;
            PasserPos = Position2D.Zero;
            PickerPos = Position2D.Zero;
            PositionerPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;
            lastShooterPos = Position2D.Zero;
            lastPositionerPos = Position2D.Zero;
            lastPasserPos = Position2D.Zero;
            PasserAngle = 0;
            PickerAngle = 0;
            PositionerAng = 0;
            counter = 0;
            finishCounter = 0;
            RotateDelay = 40;
            getBallSkill = new GetBallSkill();
            extendedBallTarLine = new Line();
            oppFeasibleLine = new Line();
            ourFeasibleLine = new Line();
            dangerC = new Circle();
            center = Position2D.Zero;
            if (sync != null)
            {
                sync.Reset();
            }
            else
            {
                sync = new Syncronizer();
            }
        }

        public override void InitializeStates(GameStrategyEngine engine, GameDefinitions.WorldModel Model, Dictionary<int, GameDefinitions.SingleObjectState> attendance)
        {
            Attendance = attendance;
            CurrentState = (int)State.First;
            InitialState = 0;
            FinalState = 2;
            TrapState = 2;
        }

        public override void FillInformation()
        {
            StrategyName = "DirectCorner";
            AttendanceSize = 3;
            About = "this strategy will shoot directly to opp goal from corner !";
        }

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, ref GameDefinitions.GameStatus Status)
        {
            if (CurrentState == (int)State.Finish)
            {
                Status = GameStatus.Normal;
                return false;
            }
            return true;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                double minDist = double.MaxValue;
                int minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PasserId = minIdx;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PasserId && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PickerID = minIdx;
                PositionerID = -1;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PickerID && item != PasserId && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID = minIdx;
                first = false;
            }
            #endregion

            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #region States
            if (CurrentState == (int)State.First)
            {
                double dAngle = Model.OurRobots[PasserId].Angle.Value - PasserAngle;
                timeLimitCounter++;
                if (dAngle > 180)
                    dAngle -= 360;
                else if (dAngle < -180)
                    dAngle += 360;

                if (inRotate && Model.OurRobots[PositionerID].Location.DistanceFrom(PositionerPos) < tresh && Model.OurRobots[PickerID].Location.DistanceFrom(PickerPos) < tresh)
                    counter++;
                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    timeLimitCounter = 0;
                    CurrentState = (int)State.Go;
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                timeLimitCounter++;
                if (passed)
                    finishCounter++;
                if (sync.Finished || sync.Failed || finishCounter > finishTresh || passed || timeLimitCounter > maxWaitTresh)
                    CurrentState = (int)State.Finish;
            }
            #endregion
            #region PosAndAngles
            if (CurrentState == (int)State.First)
            {
                ShootTarget = GameParameters.OppGoalCenter ;//+ new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, 0);
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);

                //PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                PickerPos = new Position2D(-1.4, 0.8 * Math.Sign(Model.BallState.Location.Y));
                PickerAngle = 180;

                PositionerPos = new Position2D(-1.6, 0); ;
                PositionerAng = (ShootTarget - PositionerPos).AngleInDegrees;
                RotateTeta = 30;
               // inrot = false;
            }
            else if (CurrentState == (int)State.Go)
            {
                if (!passTargetCalculated)
                {
                    double margin = 0.01;

                    double minD = double.MaxValue;
                    Circle c = new Circle(Model.BallState.Location, 0.8);
                    foreach (var item in Model.Opponents.Keys)
                    {
                        if (Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) < minD && c.IsInCircle(Model.Opponents[item].Location))
                        {
                            minD = Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location);
                            minOppId = item;
                        }
                    }
                    if (minOppId != -1)
                    {
                        Position2D tmpTar = GameParameters.OppGoalCenter + new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, 0);
                        Vector2D ballTarVec = tmpTar - Model.BallState.Location;
                        var perpBallTar = ballTarVec.GetPerp();
                        if (perpBallTar.X < 0)
                            perpBallTar *= -1;
                        perpBallTar.NormalizeTo(0.025);
                        extendedBallTarLine = new Line(Model.BallState.Location + perpBallTar, tmpTar + perpBallTar);
                        extendedBallTarLine.DrawPen = new Pen(Color.Blue, 0.01f);
                        perpBallTar.NormalizeTo(perpBallTar.Size + RobotParameters.OurRobotParams.Diameter / 2 + margin);
                        
                        oppFeasibleLine = new Line(Model.BallState.Location + perpBallTar, tmpTar + perpBallTar);
                        oppFeasibleLine.DrawPen = new Pen(Color.Orange, 0.01f);
                        Position2D tmpH = oppFeasibleLine.Head + (oppFeasibleLine.Head - GameParameters.OppGoalCenter).GetNormalizeToCopy(RobotParameters.OurRobotParams.Diameter);
                        Position2D tmpT = oppFeasibleLine.Tail + (oppFeasibleLine.Tail - GameParameters.OppGoalCenter).GetNormalizeToCopy(RobotParameters.OurRobotParams.Diameter);
                        Line ourFeasibleLine = new Line(tmpH, tmpT);
                        ourFeasibleLine.DrawPen = new Pen(Color.Red, 0.01f);
                        dangerC = new Circle(new Position2D(GameParameters.OppGoalCenter.X, Math.Sign(Model.BallState.Location.Y) * 0.175), GameParameters.DefenceareaRadii + RobotParameters.OurRobotParams.Diameter + margin, new Pen(Color.RosyBrown,0.01f));
                        var intersects = dangerC.Intersect(ourFeasibleLine);
                        Position2D? tmpInter = null;
                        if (intersects.Count > 1)
                        {
                            if (intersects[0].DistanceFrom(Model.BallState.Location) < intersects[1].DistanceFrom(Model.BallState.Location))
                                tmpInter = intersects[0];
                            else
                                tmpInter = intersects[1];
                            pickIsPossible = true;
                        }
                        else if (intersects.Count == 1)
                        {
                            tmpInter = intersects[0];
                            pickIsPossible = true;
                        }
                        center = Position2D.Zero;
                        if (pickIsPossible)
                        {
                            int i = 0;
                            Vector2D ourFeasibleVec = (tmpH - tmpT).GetNormalizeToCopy(RobotParameters.OurRobotParams.Diameter / 2);
                            center = tmpInter.Value + ourFeasibleVec;
                            ourFeasibleVec.NormalizeTo(0.02);
                            while (i < 20 && dangerC.Intersect(new Circle(center, RobotParameters.OurRobotParams.Diameter / 2)).Count > 1)
                            {
                                center += ourFeasibleVec;
                                i++;
                            }
                            i = 0;
                            center.DrawColor = Color.Violet;
                            if (ourFeasibleLine.Distance(Model.Opponents[minOppId].Location) > RobotParameters.OurRobotParams.Diameter + 0.02)
                            {
                                ourFeasibleLine.PerpenducilarLineToPoint(Model.Opponents[minOppId].Location).IntersectWithLine(ourFeasibleLine, ref PickerPos);
                                PickerPos = Model.Opponents[minOppId].Location + (PickerPos - Model.Opponents[minOppId].Location).GetNormalizeToCopy(RobotParameters.OurRobotParams.Diameter + 0.02);
                            }
                            else
                            {
                                Position2D c2 = new Position2D();
                                ourFeasibleLine.PerpenducilarLineToPoint(Model.Opponents[minOppId].Location).IntersectWithLine(ourFeasibleLine, ref c2); ;
                                PickerPos = c2;
                                ourFeasibleVec.NormalizeTo(0.02);
                                while (i < 20 
                                    && new Circle(Model.Opponents[minOppId].Location, RobotParameters.OurRobotParams.Diameter / 2).Intersect(new Circle(PickerPos, RobotParameters.OurRobotParams.Diameter / 2)).Count > 1
                                    && Position2D.IsBetween(center,c2,PickerPos))
                                {
                                    PickerPos -= ourFeasibleVec;
                                    i++;
                                }
                                if(i > 0)
                                    PickerPos += ourFeasibleVec;
                            }

                        }

                        
                    }
                    //ShootTarget = GameParameters.OppGoalCenter + new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, 0);
                    PositionerAng = (ShootTarget - PositionerPos).AngleInDegrees;
                    passTargetCalculated = true;

                }
                if (Debug)
                {
                    DrawingObjects.AddObject(dangerC, "dangerC");
                    DrawingObjects.AddObject(extendedBallTarLine, "extendedBallLine");
                    DrawingObjects.AddObject(oppFeasibleLine, "oppFeas");
                    DrawingObjects.AddObject(ourFeasibleLine, "ourFeasible");
                    DrawingObjects.AddObject(center, "centerPoi");
                }
                if (Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > ballMovedTresh)
                    passed = true;
           
            }
            if (Debug)
            {
                if (pickIsPossible)
                    DrawingObjects.AddObject(new Circle(PickerPos, 0.1, new Pen(Color.Aqua, 0.01f)), "circl");
            }
            #endregion
        }
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            if (CurrentState == (int)State.First)
            {
                if (Planner.AddRotate(Model, PasserId, ShootTarget, RotateTeta, kickPowerType.Speed, PassSpeed, false, Model.BallState.Location.Y > 0, rotateCounter + RotateDelay, true).IsInRotateDelay)
                {
                    inRotate = true;
                    rotateCounter++;
                }

                Planner.ChangeDefaulteParams(PositionerID, false);
                Planner.SetParameter(PositionerID, 3, 2.5);
                Planner.Add(PickerID, PickerPos, PickerAngle, PathType.UnSafe, true, true, true, true);

                Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, true, true, true);
            }
            else if (CurrentState == (int) State.Go)
            {
                RotateDelay = 20;
                if (Model.OurRobots[PickerID].Location.DistanceFrom(PickerPos) > 0.23)
                    rotateCounter++;
                if (Planner.AddRotate(Model, PasserId, ShootTarget, RotateTeta, kickPowerType.Speed, PassSpeed, false, Model.BallState.Location.Y > 0, rotateCounter + RotateDelay, true).IsInRotateDelay)
                {
                    inRotate = true;
                } 
                Planner.Add(PickerID, PickerPos, PickerAngle, PathType.UnSafe, true, true, true, true);

                Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, true, true, true);

            }
            return new Dictionary<int, RoleBase>();
        }
        enum State
        {
            First,
            Go,
            Finish
        }
    }
}
