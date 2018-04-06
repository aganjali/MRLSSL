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
    public class Middle3AtackerStrategy : StrategyBase
    {
        const double tresh = 0.01, angleTresh = 2, waitTresh = 40, wait2Tresh = 60, finishTresh = 150, initDist = 0.22,
           maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistShoot = 0.5,
           faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, passSpeedTresh = StaticVariables.PassSpeedTresh, secondPassTresh = 0.5;

        Syncronizer sync;
        int PasserID, ShooterID, Poser1ID;
        Position2D Pos1, Pos2, PasserPos, firstBallPos;
        Position2D ShootTarget, PassTarget;
        double PasserAngle, PassSpeed, KickSpeed, sgn;
        bool passed, first, firstInState, inPassState, chipOrigin, isChip, backSensor;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter, rotateCounter;
        int Mode;
        bool goBack;
        public override void ResetState()
        {
            rotateCounter = 2;
            goBack = true;
            Mode = 0;
            passed = false;
            first = true;
            firstInState = true;
            inPassState = false;
            chipOrigin = false;
            isChip = false;
            backSensor = true;
            RotateDelay = 320;
            sync = new Syncronizer();
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
            StrategyName = "Middle3Atacker";
            AttendanceSize = 3;
            About = "this strategy Just Do The Flip :D";
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
            CharterData.AddData("BallSpeed", Model.BallState.Speed.Size);
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                sgn = Math.Sign(firstBallPos.Y);
                KickSpeed = 5;
                ShootTarget = GameParameters.OppGoalCenter;
                PassTarget = new Position2D(-2.5, -sgn * 2.2);
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                Pos1 = new Position2D(0.0, -sgn * 0.2);
                Pos2 = new Position2D(-0.3, sgn * 0.2);

                AssignIDs(engine, Model, Attendance);
                first = false;
            }

            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #endregion

            #region State
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[Poser1ID].Location.DistanceFrom(Pos1) < tresh
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2) < tresh)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.Pass;
                    firstInState = true;
                    timeLimitCounter = 0;
                    counter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    faildCounter++;
                else
                    faildCounter = Math.Max(faildCounter - 2, 0);
                if (faildCounter > 4)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Pass)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if (sync.CatchState != 1)
                    {
                        if ((v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistShoot) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistShoot))
                        {
                            faildCounter++;
                            if (faildCounter > faildMaxCounter)
                                CurrentState = (int)State.Finish;
                        }
                        else
                            faildCounter = Math.Max(0, faildCounter - 5);
                    }
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

                if (sync.CatchState == (int)CatchAndRotateBallRole.State.Rotate && ++counter > wait2Tresh)
                {
                    counter = 0;
                    CurrentState = (int)State.Finish;
                    firstInState = true;
                }
            }

            #endregion

            #region Pos
            if (CurrentState == (int)State.First)
            {
                if (firstInState)
                {
                }

            }
            else if (CurrentState == (int)State.Pass)
            {
                if (firstInState)
                {

                    firstInState = false;
                }

                if (inPassState && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;

                if (!passed)
                {
                    isChip = chipOrigin;
                    if (!chipOrigin)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                        isChip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);
                    }

                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, false, false);
                    if (isChip)
                        PassSpeed = Math.Max(Model.BallState.Location.DistanceFrom(PassTarget) * 0.5, 0.8);
                }
            }
            #endregion
        }
        private void AssignIDs(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> att)
        {
            var tmpIds = att.Keys.ToList();
            double minDist = double.MaxValue;
            int minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            PasserID = minIdx;
            tmpIds.Remove(minIdx);
            //-------------------------------------------------------------------------------------------------------------------------------------------
            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Pos1) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Pos1);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Poser1ID = minIdx;
            tmpIds.Remove(minIdx);
            //-------------------------------------------------------------------------------------------------------------------------------------------
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            ShooterID = tmpIds[0];
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3.5, 2);

                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(ShooterID, Pos2, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(PasserID, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);
            }
            else if (CurrentState == (int)State.Pass)
            {
                sync.CatchAndWait = false;

                //Planner.Add(ShooterID, new Position2D(1, -sgn * 0.5), 180, PathType.UnSafe, true, true, true, true);

                if (isChip)
                {
                    KickSpeed = 3;
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, false, kickPowerType.Speed, backSensor);

                }
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, false, kickPowerType.Speed, RotateDelay);
                if (sync.InPassState)
                    inPassState = true;

                if (goBack && sync.SyncStarted)
                {
                    Planner.Add(ShooterID, new Position2D(1, -sgn * 0.5), 180, PathType.UnSafe, true, true, true, true);
                    if (Model.OurRobots[ShooterID].Location.X > 0.5)
                    {
                        goBack = false;
                    }
                }

                if (passed)
                {
                    Planner.Add(PasserID, Model.OurRobots[PasserID].Location, Model.OurRobots[PasserID].Angle.Value, PathType.UnSafe, true, true, true, true);
                }
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        enum State
        {
            First,
            Pass,
            Finish
        }
    }
}
