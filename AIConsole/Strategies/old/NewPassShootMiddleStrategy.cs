using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;

namespace MRL.SSL.AIConsole.Strategies
{
    public class NewPassShootMiddleStrategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 150, finish2Tresh = 200, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05,
            faildBallDistSecondPass = 0.5, faildMaxCounter = 45, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, SecondPassTresh = 0.5, passSpeedTresh = StaticVariables.PassSpeedTresh;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.5;
        const double distBehindBallTresh = 0.07;

        bool inPassState = false;
        bool first, passTargetCalculated, Debug = false, nearShooter, shooted;
        int PasserId, ShooterID, PositionerID0, PositionerID1;
        Position2D PasserPos, ShooterPos, PassTarget, ShootTarget, SecondPassTarget, PositionerPos0, PositionerPos1, firstBallPos, lastPositioner1Pos, secondPasserFakePos;
        Position2D tmpPos0, secondBallPos;
        double PasserAngle, ShooterAngle, RotateTeta, PassSpeed, KickPower, SecondPassSpeed;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        int Mode;
        bool chip, passed, chipOrigin;
        bool backSensor;
        CatchAndRotateBallRole catchnrot = new CatchAndRotateBallRole();
        public override void ResetState()
        {
            tmpPos0 = Position2D.Zero;
            backSensor = true;
            Mode = 0;
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
            FinalState = 3;
            TrapState = 3;
        }

        public override void FillInformation()
        {
            StrategyName = "NewPassShootMiddle";
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

        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }
        private void AssignIDs(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> att)
        {
            var tmpIds = RemoveGoaliID(Model, att);
            double minDist = double.MaxValue;
            int minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(ShooterPos) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(ShooterPos);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            ShooterID = minIdx;
            tmpIds.Remove(ShooterID);
            //---------------------------------------------------------------------------------------------------
            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(PasserPos) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(PasserPos);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            PasserId = minIdx;
            tmpIds.Remove(PasserId);
            //---------------------------------------------------------------------------------------------------
            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(PositionerPos0) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(PositionerPos0);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            PositionerID0 = minIdx;
            tmpIds.Remove(PositionerID0);
            //---------------------------------------------------------------------------------------------------
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            PositionerID1 = tmpIds[0];
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {

            #region First
            if (first)
            {
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                if (Mode == 0)
                {
                    ShooterPos = new Position2D(Model.BallState.Location.X + 0.2, -Model.BallState.Location.Y);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    //  ShooterPos = new Position2D(2,1.6);//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);
                    ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - 0.2, Math.Abs(ShooterPos.X)));
                    ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - 0.2, Math.Abs(ShooterPos.Y)));
                    ShooterPos.Y *= 1;
                    ShooterAngle = (ShootTarget - ShooterPos).AngleInDegrees;

                    PositionerPos1 = new Position2D(1.8, PasserPos.Y + (-Math.Sign(PasserPos.Y) * 0.2));
                    PositionerPos0 = new Position2D(-2.3, -1 * Math.Sign(Model.BallState.Location.Y));
                    SecondPassTarget = new Position2D(-3, Math.Sign(Model.BallState.Location.Y) * 2);
                    PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                    secondPasserFakePos = new Position2D(0.7, ShooterPos.Y);
                }

                AssignIDs(engine, Model, Attendance);

                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (!Model.OurRobots.ContainsKey(PasserId) || !Model.OurRobots.ContainsKey(ShooterID) || !Model.OurRobots.ContainsKey(PositionerID0) || !Model.OurRobots.ContainsKey(PositionerID1))
                return;

            #region States
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < tresh && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1 && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < 0.23)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    timeLimitCounter = 0;
                    CurrentState = (int)State.Pass;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Pass)
            {
                timeLimitCounter++;
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh)
                {
                    CurrentState = (int)State.Finish;
                }
                //if (sync.Finished || sync.Failed) //|| finishCounter > finishTresh || timeLimitCounter > maxWaitTresh)
                //    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if (sync.CatchKicked)
                    {
                        secondBallPos = Model.BallState.Location;
                        CurrentState = (int)State.SecondPass;
                    }

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



                //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.3)
                //    nearShooter = true;
                //if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[ShooterID].Location - firstBallPos) <= 0)
                //    shooted = true;
                //if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) < SecondPassTresh)
                //    CurrentState = (int)State.SecondPass;

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > finish2Tresh)
                {
                    CurrentState = (int)State.Finish;
                }
                Vector2D refrence = SecondPassTarget - secondBallPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed && (v.Y < 0.1 && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) <= faildBallDistSecondPass))
                {
                    faildCounter++;
                    if (faildCounter > faildMaxCounter)
                        CurrentState = (int)State.Finish;
                }
                else
                    faildCounter = Math.Max(0, faildCounter - 5);
            }

            #endregion
            #region PosAndAngles
            if (CurrentState == (int)State.First)
            {

            }
            else if (CurrentState == (int)State.Pass)
            {
                if (!passTargetCalculated)
                {
                    chipOrigin = false;// true;
                    if (chipOrigin)
                        RotateTeta = 0;

                    if (Mode == 0)
                    {
                        ShootTarget = GameParameters.OppGoalCenter;
                        PassTarget = ShooterPos;
                        PositionerPos1 = new Position2D(-1.5, Math.Sign(PasserPos.Y));
                        SecondPassSpeed = Math.Max(SecondPassTarget.DistanceFrom(PassTarget) * 0.6, 1);
                    }
                    passTargetCalculated = true;
                }

                if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > BallMovedTresh)
                    passed = true;

                if (!passed)
                {
                    chip = chipOrigin;
                    if (!passed && !chipOrigin)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, new List<int>() { ShooterID, PasserId }, null);
                        chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);

                    }
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, false, false);
                    if (chip)
                    {
                        PassSpeed = Math.Max(Model.BallState.Location.DistanceFrom(PassTarget) * 0.5, 1);
                    }
                }

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
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
                Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(ShooterID, secondPasserFakePos, ShooterAngle, PathType.UnSafe, true, true, true, true);
            }
            else if (CurrentState == (int)State.Pass)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));
                if (Mode == 0)
                {
                    sync.CatchAndWait = false;
                    if (!chip)
                        sync.SyncDirectCatch(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, SecondPassTarget, PassSpeed, SecondPassSpeed, true, kickPowerType.Speed, RotateDelay);
                    else
                        sync.SyncChipCatch(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, SecondPassTarget, PassSpeed, SecondPassSpeed, RotateDelay, true, kickPowerType.Speed, backSensor);

                    //if (sync.SyncStarted)
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.UnSafe, true, false, true, true);

                    if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) > 0.25)
                    {
                        Planner.Add(PasserId, SecondPassTarget, (PassTarget - Model.OurRobots[PasserId].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    }

                    
                }
                inPassState = sync.InPassState;
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                catchnrot.CatchAndRotate(engine, Model, PasserId, Model.OurRobots[PasserId], ShootTarget, true, false, kickPowerType.Speed, true, true, KickPower, true, RotateDelay);
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        public enum State
        {
            First,
            Pass,
            SecondPass,
            Finish
        }
    }
}
