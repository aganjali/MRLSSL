using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;

namespace MRL.SSL.AIConsole.Strategies
{
    public class PassShootMiddle : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 100, initDist = 0.22, maxWaitTresh = 250, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        bool inrot = false, inPassState = false;
        bool first, passTargetCalculated, Debug = false, nearShooter, shooted;
        int PasserId, ShooterID, PositionerID0, PositionerID1;
        Position2D PasserPos, ShooterPos, PassTarget, ShootTarget, PositionerPos0, PositionerPos1, firstBallPos, lastPositioner1Pos;
        Position2D tmpPos0;
        double PasserAngle, ShooterAngle, RotateTeta, PassSpeed, KickPower, tmpAng0;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        int Mode;
        bool chip, passed, chipOrigin, goOneTouch;
        bool backSensor;

        public override void ResetState()
        {
            tmpPos0 = Position2D.Zero;
            tmpAng0 = 0;
            backSensor = true;
            Mode = 1;
            goOneTouch = false;
            faildCounter = 0;
            nearShooter = false;
            shooted = false;
            chip = false;
            chipOrigin = false;
            passed = false;
            CurrentState = InitialState;
            first = true;
            passTargetCalculated = false;
            RotateTeta = 60;
            PassSpeed = 4;
            KickPower = Program.MaxKickSpeed;
            timeLimitCounter = 0;
            PasserId = -1;
            ShooterID = -1;
            PositionerID0 = 0;
            PositionerID1 = 0;

            lastPositioner1Pos = Position2D.Zero;
            PositionerPos0 = Position2D.Zero;
            PositionerPos1 = Position2D.Zero;
            PasserPos = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;
            firstBallPos = Position2D.Zero;

            PasserAngle = 0;
            ShooterAngle = 0;
            counter = 0;
            finishCounter = 0;
            RotateDelay = 60;
            inPassState = false;
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
            StrategyName = "PassShootMiddle";
            AttendanceSize = 4;
            About = "this is Middle Pass Shoot strategy!";
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

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {

            #region First
            if (first)
            {
                double minDist = double.MaxValue;
                int minIdx = -1;
                double maxDist = double.MinValue;
                int maxIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X > maxDist)
                    {
                        maxDist = Model.OurRobots[item].Location.X;
                        maxIdx = item;
                    }
                }
                if (maxIdx == -1)
                    return;
                ShooterID = maxIdx;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != ShooterID && Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
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
                    if (item != ShooterID && item != PasserId && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID0 = minIdx;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (PasserId != item && ShooterID != item && PositionerID0 != item && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID1 = minIdx;

                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (!Model.OurRobots.ContainsKey(PasserId) || !Model.OurRobots.ContainsKey(ShooterID))
                return;

            #region States
            if (CurrentState == (int)State.First)
            {
                double dAngle = Model.OurRobots[PasserId].Angle.Value - PasserAngle;
                timeLimitCounter++;
                if (dAngle > 180)
                    dAngle -= 360;
                else if (dAngle < -180)
                    dAngle += 360;

                if (Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < tresh && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1 && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < 0.23)
                    counter++;
                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    timeLimitCounter = 0;
                    CurrentState = (int)State.Go;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Go)
            {
                timeLimitCounter++;
                if (passed)
                    finishCounter++;
                if (sync.Finished || sync.Failed || finishCounter > finishTresh || timeLimitCounter > maxWaitTresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if ((v.Y < faildFarPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistSecondPass))
                    {
                        faildCounter++;
                        if (faildCounter > faildMaxCounter)
                            CurrentState = (int)State.Finish;
                    }
                    else
                        faildCounter = Math.Max(0, faildCounter - 5);
                }
                else
                {
                    if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist && Model.BallState.Location.DistanceFrom(firstBallPos) < maxFaildMovedDist)
                    {
                        faildCounter++;
                        if (faildCounter > 3)
                            CurrentState = (int)State.Finish;
                    }
                    else
                        faildCounter = Math.Max(0, faildCounter - 1);
                }

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.3)
                    nearShooter = true;
                if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[ShooterID].Location - Model.OurRobots[PasserId].Location) <= 0)
                    shooted = true;
                if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > 0.35)
                    CurrentState = (int)State.Finish;

            }
            #endregion
            #region PosAndAngles
            if (CurrentState == (int)State.First)
            {
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                if (Mode == 0)
                {
                    ShooterPos = new Position2D(1.8, PasserPos.Y);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    //  ShooterPos = new Position2D(2,1.6);//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);
                    ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - 0.2, Math.Abs(ShooterPos.X)));
                    ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - 0.2, Math.Abs(ShooterPos.Y)));
                    ShooterPos.Y *= 1;
                    ShooterAngle = 180;

                    PositionerPos1 = new Position2D(Model.BallState.Location.X - 0.8, -Math.Sign(Model.BallState.Location.Y) * 1);
                    PositionerPos0 = new Position2D(-2.3, 1 * Math.Sign(Model.BallState.Location.Y));
                    PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                }
                else if (Mode == 1)
                {
                    ShooterPos = new Position2D(-1.5, 0.4 * Math.Sign(-Model.BallState.Location.Y));//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    //  ShooterPos = new Position2D(2,1.6);//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);
                    ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - 0.2, Math.Abs(ShooterPos.X)));
                    ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - 0.2, Math.Abs(ShooterPos.Y)));
                    ShooterPos.Y *= 1;
                    ShooterAngle = 180;

                    PositionerPos1 = new Position2D(1.8, PasserPos.Y);
                    PositionerPos0 = new Position2D(-2.3, -1 * Math.Sign(Model.BallState.Location.Y));
                    PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                    lastPositioner1Pos = PositionerPos1;
                }
                else if (Mode == 2)
                {
                    ShooterPos = new Position2D(Model.BallState.Location.X + 0.2, -Model.BallState.Location.Y);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    //  ShooterPos = new Position2D(2,1.6);//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);
                    ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - 0.2, Math.Abs(ShooterPos.X)));
                    ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - 0.2, Math.Abs(ShooterPos.Y)));
                    ShooterPos.Y *= 1;
                    ShooterAngle = (ShootTarget - ShooterPos).AngleInDegrees;

                    PositionerPos1 = new Position2D(1.8, PasserPos.Y);
                    PositionerPos0 = new Position2D(-2.3, -1 * Math.Sign(Model.BallState.Location.Y));
                    PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                }
                inrot = false;
            }
            else if (CurrentState == (int)State.Go)
            {
                double width = 1, heigth = 1, step = 0.2;
                Position2D topLeft = new Position2D();
                       
                if (!passTargetCalculated)
                {
                    if (Model.BallState.Location.X < -(GameParameters.OurGoalCenter.X /*- GameParameters.DefenceareaRadii*/))
                        chipOrigin = true;
                    if (chipOrigin)
                        RotateTeta = 0;

                    if (Mode == 0)
                    {
                        ShootTarget = GameParameters.OppGoalCenter;//(Model.BallState.Location.Y >= 0) ? GameParameters.OppGoalRight : GameParameters.OppGoalLeft;
                        width = 0.9; heigth = 0.9; step = 0.3;

                        if (Model.BallState.Location.X < -2)
                            margin = 0.8;
                        topLeft = new Position2D(Model.BallState.Location.X + width + margin, ((Model.BallState.Location.Y > 0) ? -(1 + heigth) : 1));

                        chip = chipOrigin;
                        bool b = engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref PassTarget);
                        chipOrigin = chip;
                        Circle c = new Circle(Model.BallState.Location, 0.9);
                        double minDist = double.MaxValue; int oppid = -1;
                        foreach (var item in Model.Opponents.Keys)
                        {
                            if (c.IsInCircle(Model.Opponents[item].Location) && Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                            {
                                oppid = item;
                                minDist = Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location);
                            }
                        }
                        if (oppid == -1)
                        {
                            tmpPos0 = new Position2D(Model.BallState.Location.X - .3, Model.BallState.Location.Y + (-1 * Math.Sign(Model.BallState.Location.Y) * .4));
                        }
                        else
                        {
                            tmpPos0 = Model.Opponents[oppid].Location + new Vector2D(0.2, 0);
                            tmpAng0 = (Model.Opponents[oppid].Location - tmpPos0).AngleInDegrees;
                        }
                    }
                    else if (Mode == 1)
                    {
                        ShootTarget = new Position2D(-3.0, 0.25 * Math.Sign(Model.BallState.Location.Y));
                        width = 1; heigth = 1; step = 0.2;
                        PositionerPos1 = new Position2D(Model.BallState.Location.X, -Model.BallState.Location.Y);
                        PassTarget = new Position2D(-0.4, Math.Sign(Model.BallState.Location.Y) * 0.4);
                    }
                    else if (Mode == 2)
                    {
                        ShootTarget = GameParameters.OppGoalCenter;
                        PassTarget = ShooterPos;
                        PositionerPos1 = new Position2D(Math.Max(Model.BallState.Location.X - 0.5, -1.9), 0 * Math.Sign(Model.BallState.Location.Y));

                    }

                    passTargetCalculated = true;

                }

                if (Mode == 0)
                {
                    if (sync.SyncStarted && !passed)
                    {
                        PositionerPos1 = tmpPos0;
                        double goodness;
                        //var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, PassTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                        //if (GoodPointInGoal.HasValue)
                        //    ShootTarget = GoodPointInGoal.Value;
                        ShootTarget = new Position2D(GameParameters.OppGoalCenter.X, Math.Sign(Model.BallState.Location.Y) * 0.2);                      
                    } 

                }
                else if (Mode == 1)
                {
                    if (Model.OurRobots[PositionerID1].Location.DistanceFrom(lastPositioner1Pos) > 0.35)
                        goOneTouch = true;
                }
                else if (Mode == 2)
                {
                    if (Model.OurRobots[PositionerID1].Location.DistanceFrom(lastPositioner1Pos) > 0.35)
                        goOneTouch = true;
                }

                if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > BallMovedTresh)
                    passed = true;
                chip = chipOrigin;
                if (!passed && !chipOrigin)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, new List<int>(){ShooterID, PasserId}, null);
                    chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);

                }
                if (!passed)
                {
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, chip, true);
                }
            }

            #endregion

        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserId, false);
                Planner.SetParameter(PasserId, 2.5, 2);
                Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);

                Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.Safe, true, true, true, true);
            }
            else if (CurrentState == (int)State.Go)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));
                if (Mode == 0)
                {
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);

                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);


                }
                else if (Mode == 1)
                {
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, PositionerID1, PassTarget, PositionerPos1, ShootTarget, PassSpeed, KickPower, RotateDelay, false);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, PositionerID1, PassTarget, PositionerPos1, ShootTarget, kickPowerType.Speed, PassSpeed, KickPower, RotateDelay, false,backSensor);

                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);

                    if (sync.SyncStarted && passed)
                        Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (goOneTouch)
                    {
                        bool gotoPoint = false; 
                        if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh && passed)
                            gotoPoint = true;
                        Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
                        if (passed)
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(OneTouchRole)))
                                Functions[ShooterID] = (eng, wmd) => GetRole<OneTouchRole>(ShooterID).Perform(eng, wmd, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0),
                                    Model.OurRobots[PasserId], chip, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                        }
                        if(!gotoPoint)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.Safe, false, true, true, true);
                    }
                }
                else if (Mode == 2)
                {
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, PositionerID1, PassTarget, PositionerPos1, ShootTarget, PassSpeed, KickPower, RotateDelay, false);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, PositionerID1, PassTarget, PositionerPos1, ShootTarget, kickPowerType.Speed, PassSpeed, KickPower, RotateDelay, false,backSensor);
                    
                    if (sync.SyncStarted && passed)
                        Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.UnSafe, true, false, true, true);

                    Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
                    if (passed)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(OneTouchRole)))
                            Functions[ShooterID] = (eng, wmd) => GetRole<OneTouchRole>(ShooterID).Perform(eng, wmd, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0),
                                Model.OurRobots[PasserId], chip, ShootTarget, KickPower, false, true, PassSpeed);
                        DrawingObjects.AddObject(new Circle(Pos2go, 1, new System.Drawing.Pen(System.Drawing.Color.Red, .01f)), "Pos2Gohhh");
                    }
                    else
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.Safe, false, true, true, true);
                   
                }
                if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) > 0.25)
                {
                    Planner.Add(PasserId, new Position2D(-1.2, Math.Sign(PasserPos.Y) * 0.7), 180, PathType.UnSafe, false, true, true, true);
                }
                inPassState = sync.InPassState;
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        public enum State
        {
            First,
            Go,
            Finish
        }
    }
}

