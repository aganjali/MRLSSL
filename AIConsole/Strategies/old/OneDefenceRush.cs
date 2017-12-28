using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Strategies
{
    public class OneDefenceRush : StrategyBase
    {
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 60, finishTresh = 200, initDist = 0.22,
            maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistShoot = 0.5,
            faildMaxCounter = 10, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, passSpeedTresh = StaticVariables.PassSpeedTresh, failedRobotDis = 0.5, failedBallDis = 0.5, faildBallDistSecondPass = 0.5;

        Syncronizer sync;
        int PasserID, Defender1ID, ShooterID, Poser1ID, Poser2ID;
        Position2D Defender1Pos, Pos1, Pos2, Pos3, PasserPos, firstBallPos, secondBallPos;
        Position2D ShootTarget, PassTarget;
        double Defender1Ang, PasserAngle, PassSpeed, KickSpeed;
        bool passed, first, firstInState, inPassState, chipOrigin, isChip, backSensor, goBack;
        int sgn, counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter, rotateCounter;
        int Mode;

        public override void ResetState()
        {
            goBack = true;
            rotateCounter = 2;
            RotateDelay = 2;
            Mode = 1;
            passed = false;
            first = true;
            firstInState = true;
            inPassState = false;
            chipOrigin = false;
            isChip = false;
            backSensor = true;
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
            StrategyName = "OneDefenceRushStrategy";
            AttendanceSize = 4;
            About = "this strategy just want to score a goal out of opponent team!!!!";
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
            CalculateDefenderInfo(engine, Model, out Defender1Pos, out Defender1Ang);
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                sgn = Math.Sign(firstBallPos.Y);
                ShootTarget = GameParameters.OppGoalCenter;
                KickSpeed = StaticVariables.MaxKickSpeed;

                PassTarget = new Position2D(-2.5, -sgn * 0.3);
                Pos1 = new Position2D(-3.5, -sgn * 1.5);//
                Pos2 = new Position2D(-3.3, -sgn * 1.4);//
                Pos3 = new Position2D(-0.3, sgn * 1);//
                Pos2 = Pos1 + (Pos2 - Pos1).GetNormalizeToCopy(0.3);
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

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
                    && Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < tresh)
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
                    if (sync.CatchState == 1)
                    {
                        if (Mode == 0)
                        {
                            CurrentState = (int)State.Finish;
                        }
                    }
                    else
                    {
                        if (Model.BallState.Location.DistanceFrom(PassTarget) < failedBallDis && Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) > failedRobotDis)
                        {
                            CurrentState = (int)State.Finish;
                        }
                        if ((v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistShoot) ||
                            (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistShoot))
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
                    if (Mode == 0)
                        CurrentState = (int)State.Finish;
                    else
                        CurrentState = (int)State.SecondPass;
                    firstInState = true;
                    finishCounter = 0;
                    secondBallPos = Model.BallState.Location;
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > 100 + wait2Tresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = Pos3 - secondBallPos;
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

            #endregion

            #region Pos
            if (CurrentState == (int)State.First)
            {
                if (firstInState)
                {
                    firstInState = false;
                }

            }
            else if (CurrentState == (int)State.Pass)
            {
                if (firstInState)
                {
                    Planner.SetReCalculateTeta(PasserID, true);
                    if (Mode == 0)
                    {
                        Pos2 = Pos2 + (GameParameters.OppGoalCenter - Pos2).GetNormalizeToCopy(0.3);
                        Pos1 = Pos1 + (Pos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(0.3);
                        ShooterID = Poser1ID;
                        Poser1ID = -1;
                    }
                    else if (Mode == 1)
                    {
                        ShooterID = Poser2ID;
                        Poser2ID = -1;
                    }

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
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.6;
                }
                //if (sync.SyncStarted)
                //{
                //    Pos1 = new Position2D(-2.4, -sgn*2.01);
                //}
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    passed = false;
                    inPassState = false;
                    firstInState = false;
                }
                if (sync.CatchKicked && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;

                if (!passed)
                {
                    //isChip = chipOrigin = true;

                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    isChip = obs.Meet(Model.BallState, new SingleObjectState(Pos3, Vector2D.Zero, 0), 0.07);

                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, Pos3, isChip, true);
                    if (isChip)
                        PassSpeed = Math.Max(1.5, Model.BallState.Location.DistanceFrom(Pos3) * 0.6);
                }

            }
            #endregion
        }
        private void CalculateDefenderInfo(GameStrategyEngine engine, WorldModel Model, out Position2D def1Pos, out double def1Ang)
        {
            List<DefenderCommand> defendcommands = new List<DefenderCommand>();
            int? id = null;
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(GoalieCornerRole)
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole1),
                OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(0).Key : id
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole2),
                OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(1).Key : id
            });
            var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
            var normal1 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole2));

            def1Pos = (normal1.DefenderPosition.HasValue) ? normal1.DefenderPosition.Value : Position2D.Zero;
            def1Ang = normal1.Teta;
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
            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Pos2) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Pos2);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Poser2ID = minIdx;
            tmpIds.Remove(minIdx);
            //-------------------------------------------------------------------------------------------------------------------------------------------
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            Defender1ID = tmpIds[0];
            ShooterID = -1;
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3, 2);

                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);

                PasserPos = Model.BallState.Location + (Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(0.2);
                if (Planner.AddRotate(Model, PasserID, PassTarget, PasserPos, kickPowerType.Speed, PassSpeed, isChip, rotateCounter, backSensor).IsInRotateDelay)
                {
                    rotateCounter++;
                }

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderCornerRole2)))
                    Functions[Defender1ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Defender1ID).Run(eng, wmd, Defender1ID, Defender1Pos, Defender1Ang);
            }
            else if (CurrentState == (int)State.Pass)
            {

                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, false, true, true);
                if (Mode == 0)
                {
                    if (isChip)
                        sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, false, kickPowerType.Speed, backSensor, rotateCounter);
                    else
                        sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, false, kickPowerType.Speed, RotateDelay, rotateCounter);
                }
                else if (Mode == 1)
                {
                    sync.CatchAndWait = true;
                    if (isChip)
                        sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, Pos3, PassSpeed, PassSpeed, RotateDelay, false, kickPowerType.Speed, backSensor, rotateCounter);
                    else
                        sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, Pos3, PassSpeed, PassSpeed, false, kickPowerType.Speed, RotateDelay, rotateCounter);
                }
                if (sync.InPassState)
                    inPassState = true;
                //}

                Planner.Add(Defender1ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);
                if (passed)
                {
                    Planner.Add(PasserID, Model.OurRobots[PasserID].Location, (ShootTarget - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                    //    Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).Perform(eng, wmd, ShooterID, null);
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                sync.CatchAndWait = false;
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, secondBallPos, ShooterID, PassTarget, Pos3, PassSpeed, PassSpeed, RotateDelay, false, kickPowerType.Speed, backSensor, rotateCounter);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, secondBallPos, ShooterID, PassTarget, Pos3, PassSpeed, PassSpeed, false, kickPowerType.Speed, RotateDelay, rotateCounter);
                if (passed)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(ActiveRole)))
                        Functions[Defender1ID] = (eng, wmd) => GetRole<ActiveRole>(Defender1ID).Perform(eng, wmd, Defender1ID, null);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(DefenderCornerRole2)))
                        Functions[PasserID] = (eng, wmd) => GetRole<DefenderCornerRole2>(PasserID).Run(eng, wmd, PasserID, Defender1Pos, Defender1Ang);
                }
            }


            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        enum State
        {
            First,
            Pass,
            SecondPass,
            Finish
        }
    }
}
