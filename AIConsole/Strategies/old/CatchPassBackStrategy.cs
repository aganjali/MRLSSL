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
    public class CatchPassBackStrategy : StrategyBase
    {
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 60, finishTresh = 150, secondPassFinishTresh = 150, initDist = 0.22,
            maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistShoot = 0.5, faildBallDistSecondPass = 0.5,
            faildMaxCounter = 10, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, passSpeedTresh = StaticVariables.PassSpeedTresh, secondPassTresh = 0.5;

        CatchAndRotateBallRole catchnRotate;
        Syncronizer sync;
        int PasserID, Poser1ID, Poser2ID;
        Position2D Pos1, Pos2, Pos3, PasserPos, firstBallPos, secondBallPos, shooterPos;
        Position2D ShootTarget, PassTarget, secondPassTarget;
        double PasserAngle, PassSpeed, KickSpeed;
        bool passed, first, firstInState, inPassState, chipOrigin, isChip, isKickChip, backSensor;
        int sgn, counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        int Mode;

        public override void ResetState()
        {
            RotateDelay = 60;
            Mode = 1;
            passed = false;
            first = true;
            firstInState = true;
            inPassState = false;
            chipOrigin = false;
            isChip = false;
            backSensor = true;
            sync = new Syncronizer();
            catchnRotate = new CatchAndRotateBallRole();
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
            StrategyName = "Catch&PassBackStrategy";
            AttendanceSize = 3;
            About = "....!!!!";
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
                sgn = Math.Sign(firstBallPos.Y);
                ShootTarget = GameParameters.OppGoalCenter;
                KickSpeed = StaticVariables.MaxKickSpeed;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                Pos1 = new Position2D(-3.5, -sgn * 2.5);//
                Pos3 = new Position2D(3, Model.BallState.Location.Y + (-sgn * .3));
                PassTarget = new Position2D(-1.5, 0);
                if (Mode == 0)
                {
                    Pos2 = new Position2D(-0.3, -sgn * 2.5);
                    secondPassTarget = new Position2D(-0.8, sgn * 2.5);
                }
                else if (Mode == 1)
                {
                    Pos2 = new Position2D(-0.3, sgn * 2.5);
                    secondPassTarget = new Position2D(-0.3, -sgn * 2.5);
                }

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
                    && Model.OurRobots[Poser2ID].Location.DistanceFrom(PassTarget) < tresh)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.FirstPass;
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
            else if (CurrentState == (int)State.FirstPass)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;

                if (passed)
                {
                    if (sync.CatchState == 1)
                    {
                        counter = 0;
                        CurrentState = (int)State.SecondPass;
                        firstInState = true;
                    }
                    else
                    {
                        Vector2D refrence = PassTarget - firstBallPos;
                        Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                        if (passed && (v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) <= faildBallDistSecondPass))
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
                    CurrentState = (int)State.SecondPass;
                    firstInState = true;
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > secondPassFinishTresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = secondPassTarget - secondBallPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed && (v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[Poser2ID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[Poser2ID].Location) <= faildBallDistSecondPass))
                {
                    faildCounter++;
                    if (faildCounter > faildMaxCounter)
                        CurrentState = (int)State.Finish;
                }
                else
                    faildCounter = Math.Max(0, faildCounter - 5);

                if (catchnRotate.CurrentState == (int)CatchAndRotateBallRole.State.Rotate)
                {
                    CurrentState = (int)State.Finish;
                }
            }

            #endregion

            #region Pos
            if (CurrentState == (int)State.First)
            {
                if (firstInState)
                {
                    firstInState = false;
                }

            }
            else if (CurrentState == (int)State.FirstPass)
            {
                if (firstInState)
                {
                    firstInState = false;
                }
                if (inPassState && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed)
                {
                    //isChip = chipOrigin = true;
                    if (!chipOrigin)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                        isChip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);
                    }

                    double oppDist = double.MaxValue;
                    foreach (var oppKey in Model.Opponents.Keys)
                    {
                        Obstacle obs = new Obstacle();
                        obs.State = Model.Opponents[oppKey];
                        obs.R = new Vector2D(RobotParameters.OpponentParams.Diameter / 2, RobotParameters.OpponentParams.Diameter / 2);
                        if (obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07))
                        {
                            if (Model.Opponents[oppKey].Location.DistanceFrom(Model.BallState.Location) < oppDist)
                            {
                                oppDist = Model.Opponents[oppKey].Location.DistanceFrom(Model.BallState.Location);
                            }
                        }
                    }

                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, false, false);
                    if (isChip)
                    {
                        PassSpeed = Math.Max((Model.BallState.Location.DistanceFrom(PassTarget)) * 0.9, 1);
                        if (oppDist < 20)
                        {
                            PassSpeed = Math.Max(PassSpeed, oppDist * 0.5);
                        }
                    }

                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    finishCounter = 0;
                    secondBallPos = Model.BallState.Location;
                    inPassState = false;
                    passed = false;
                    firstInState = false;
                }

                if (!passed)
                {
                    if (inPassState && (Model.BallState.Location.DistanceFrom(Model.OurRobots[Poser2ID].Location) > secondPassTresh))
                        passed = true;
                    if (!isKickChip)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                        isKickChip = obs.Meet(Model.BallState, new SingleObjectState(secondPassTarget, Vector2D.Zero, 0), 0.07);
                    }

                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, Poser2ID, Model.BallState.Location, secondPassTarget, false, false);
                    if (isKickChip)
                        PassSpeed = Math.Max((Model.BallState.Location.DistanceFrom(secondPassTarget)) * 0.6, 0.8);
                }
            }
            if (Mode == 0)
            {
                if (sync.CatchState == 1)
                {
                    shooterPos = secondPassTarget;
                }
                else
                {
                    shooterPos = new Position2D(-0.3, sgn * 2.5);

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
            Poser2ID = tmpIds[0];
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3.5, 2);

                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos3, (Model.BallState.Location - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(PasserID, PasserPos, (ShootTarget - PasserPos).AngleInDegrees, PathType.Safe, true, true, true, true);
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                sync.CatchAndWait = true;
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, Poser2ID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, RotateDelay, true, kickPowerType.Speed, backSensor);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, Poser2ID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay);
                if (sync.InPassState)
                    inPassState = true;
                if (passed)
                {
                    if (Mode == 0)
                    {
                        Planner.Add(Poser1ID, Pos2, (Model.BallState.Location - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                        Planner.Add(PasserID, shooterPos, (Model.BallState.Location - shooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                    else
                    {
                        Planner.Add(PasserID, Pos2, (Model.BallState.Location - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                        Planner.Add(Poser1ID, secondPassTarget, (Model.BallState.Location - secondPassTarget).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                sync.CatchAndWait = false;
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, Poser2ID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, RotateDelay, isKickChip, kickPowerType.Speed, backSensor);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, Poser2ID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, isKickChip, kickPowerType.Speed, RotateDelay);

                if (sync.CatchKicked)
                    inPassState = true;

                if (Mode == 0)
                {
                    Planner.Add(Poser1ID, Pos2, (Model.BallState.Location - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PasserID, shooterPos, (Model.BallState.Location - shooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else
                {
                    Planner.Add(PasserID, Pos2, (Model.BallState.Location - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(Poser1ID, secondPassTarget, (Model.BallState.Location - secondPassTarget).AngleInDegrees, PathType.Safe, true, true, true, true);
                }

                if (passed)
                {
                    if (Mode == 0)
                    {
                        catchnRotate.CatchAndRotate(engine, Model, PasserID, Model.OurRobots[PasserID], ShootTarget, isKickChip, false, kickPowerType.Speed, false, true, KickSpeed, true);
                        //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                        //    Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).Perform(eng, wmd, PasserID, null);
                    }
                    else
                    {
                        catchnRotate.CatchAndRotate(engine, Model, Poser1ID, Model.OurRobots[Poser1ID], ShootTarget, isKickChip, false, kickPowerType.Speed, false, true, KickSpeed, true);
                        //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser1ID, typeof(ActiveRole)))
                        //    Functions[Poser1ID] = (eng, wmd) => GetRole<ActiveRole>(Poser1ID).Perform(eng, wmd, Poser1ID, null);
                    }
                }
            }

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        enum State
        {
            First,
            FirstPass,
            SecondPass,
            Finish
        }
    }
}
