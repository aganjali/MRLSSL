using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Strategies
{
    public class DefenceRushStrategy : StrategyBase
    {
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 60, finishTresh = 200, initDist = 0.22,
            maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistShoot = 0.5,
            faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, passSpeedTresh = StaticVariables.PassSpeedTresh, secondPassTresh = 0.5;

        Syncronizer sync;
        int PasserID, Defender1ID, Defender2ID, ShooterID, Poser1ID, Poser2ID;
        Position2D Defender1Pos, Defender2Pos, GoaliePos, Pos1, Pos2, PasserPos, firstBallPos;
        Position2D ShootTarget, PassTarget, secondPassTarget;
        double Defender1Ang, Defender2Ang, GoalieAng, PasserAngle, PassSpeed, KickSpeed, sgn;
        bool passed, first, firstInState, inPassState, chipOrigin, isChip, backSensor;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        int Mode;
        public override void ResetState()
        {
            Mode = 2;
            chipOrigin = false;
            PasserID = -1;
            Defender1ID = -1;
            Defender2ID = -1;
            ShooterID = -1;
            Poser1ID = -1;
            Poser2ID = -1;
            Defender1Pos = new Position2D();
            Defender2Pos = new Position2D();
            GoaliePos = new Position2D();
            Pos1 = new Position2D();
            Pos2 = new Position2D();
            PasserPos = new Position2D();
            firstBallPos = new Position2D();
            ShootTarget = new Position2D();
            PassTarget = new Position2D();
            Defender1Ang = 0;
            Defender2Ang = 0;
            GoalieAng = 0;
            PasserAngle = 0;
            PassSpeed = 0;
            sgn = 0;
            passed = false;
            first = true;
            firstInState = true;
            inPassState = false;
            isChip = false;
            backSensor = true;
            counter = 0;
            finishCounter = 0;
            RotateDelay = 60;
            timeLimitCounter = 0;
            faildCounter = 0;
            sync = new Syncronizer();
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
            UseInMiddle = true;
            StrategyName = "Defense Rush";
            AttendanceSize = 6;
            About = "this strategy try to seduse opponent Defence system by rushing our defence... :D";
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

            CalculateDefenderInfo(engine, Model, out Defender1Pos, out Defender2Pos, out GoaliePos, out Defender1Ang, out Defender2Ang, out GoalieAng);
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                sgn = Math.Sign(firstBallPos.Y);
                secondPassTarget = new Position2D(-3, sgn * 1.3);
                ShootTarget = GameParameters.OppGoalCenter;
                KickSpeed = StaticVariables.MaxKickSpeed;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                Pos1 = new Position2D(-3.5, -sgn * 2);
                Pos2 = new Position2D(-.8, sgn * 2.2);

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
                    && Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < tresh
                    && Model.OurRobots[Defender1ID].Location.DistanceFrom(Defender1Pos) < tresh
                    && Model.OurRobots[Defender2ID].Location.DistanceFrom(Defender2Pos) < tresh)
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
                if (finishCounter > finishTresh + wait2Tresh)
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
                    if (Mode == 2)
                    {
                        if (sync.CatchState == (int)CatchAndRotateBallRole.State.Rotate)
                        {
                            counter = 0;
                            CurrentState = (int)State.SecondPass;
                            firstInState = true;
                        }
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
            else if (CurrentState == (int)State.SecondPass)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;

                Vector2D passerVec = Model.OurRobots[ShooterID].Location - secondPassTarget;
                if (sync.CatchState != 1)
                {
                    if (Math.Abs(Vector2D.AngleBetweenInDegrees(passerVec, Model.BallState.Speed)) > 90 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > secondPassTresh)
                    {
                        passed = true;
                    }
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
                    if (Mode == 0)
                    {
                        ShooterID = Poser2ID;
                        PassTarget = Model.OurRobots[ShooterID].Location;
                        Pos2 = new Position2D(-1, 0);
                        Pos1 = new Position2D(-3.2, -sgn * 1.7);
                        int tmp = Poser1ID;
                        if (Model.OurRobots[Defender1ID].Location.DistanceFrom(Pos1) < Model.OurRobots[Defender2ID].Location.DistanceFrom(Pos1))
                        {
                            Poser2ID = Defender2ID;
                            Poser1ID = Defender1ID;
                        }
                        else
                        {
                            Poser1ID = Defender2ID;
                            Poser2ID = Defender1ID;
                        }

                        Defender1ID = tmp;
                        Defender2ID = -1;
                    }
                    else if (Mode == 1 || Mode == 2)
                    {
                        PassTarget = new Position2D(-.5, 0);
                        Pos1 = new Position2D(-3.2, -sgn * 1.7);
                        int tmp = Poser1ID;
                        if (Model.OurRobots[Defender1ID].Location.DistanceFrom(Pos2) < Model.OurRobots[Defender2ID].Location.DistanceFrom(Pos2))
                        {
                            Poser1ID = Defender2ID;
                            ShooterID = Defender1ID;
                        }
                        else
                        {
                            ShooterID = Defender2ID;
                            Poser1ID = Defender1ID;
                        }

                        Defender1ID = Poser2ID;
                        Defender2ID = tmp;
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
                        PassSpeed = Math.Max(Model.BallState.Location.DistanceFrom(PassTarget) * 0.5, 0.8);
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    passed = false;
                    firstInState = false;
                    KickSpeed = Math.Max(Model.BallState.Location.DistanceFrom(secondPassTarget) * 0.6, 1);
                }
            }
            #endregion
        }

        private void CalculateDefenderInfo(GameStrategyEngine engine, WorldModel Model, out Position2D def1Pos, out Position2D def2Pos, out Position2D goalipos, out double def1Ang, out double def2Ang, out double goaliang)
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
                OppID = engine.GameInfo.OppTeam.Scores.Count > 0 ? engine.GameInfo.OppTeam.Scores.ElementAt(0).Key : id
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole2),
                OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(1).Key : id
            });
            var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
            var gol = infos.SingleOrDefault(s => s.RoleType == typeof(GoalieCornerRole));
            var normal1 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole1));
            var normal2 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole2));
            goalipos = (gol.DefenderPosition.HasValue) ? gol.DefenderPosition.Value : Position2D.Zero;
            goaliang = gol.Teta;
            def1Pos = (normal1.DefenderPosition.HasValue) ? normal1.DefenderPosition.Value : Position2D.Zero;
            def1Ang = normal1.Teta;
            def2Pos = (normal2.DefenderPosition.HasValue) ? normal2.DefenderPosition.Value : Position2D.Zero;
            def2Ang = normal2.Teta;
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
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Defender1Pos) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Defender1Pos);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Defender1ID = minIdx;
            tmpIds.Remove(minIdx);
            //-------------------------------------------------------------------------------------------------------------------------------------------
            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Defender2Pos) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Defender2Pos);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Defender2ID = minIdx;
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
            Poser1ID = tmpIds[0];
            ShooterID = -1;
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3.5, 2);

                Planner.Add(Poser1ID, Pos1, (PasserPos - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (PasserPos - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(PasserID, PasserPos, (ShootTarget - PasserPos).AngleInDegrees, PathType.Safe, true, true, true, true);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderCornerRole1)))
                    Functions[Defender1ID] = (eng, wmd) => GetRole<DefenderCornerRole1>(Defender1ID).Run(eng, wmd, Defender1ID, Defender1Pos, Defender1Ang);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(DefenderCornerRole2)))
                    Functions[Defender2ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Defender2ID).Run(eng, wmd, Defender2ID, Defender2Pos, Defender2Ang);
            }
            else if (CurrentState == (int)State.Pass)
            {

                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                if (Mode != 2)
                {
                    sync.CatchAndWait = false;
                    if (sync.CatchState == 1)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                            Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).Perform(eng, wmd, ShooterID, null);
                    }
                    else
                    {
                        if (isChip)
                            sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, false, kickPowerType.Speed, backSensor);
                        else
                            sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, false, kickPowerType.Speed, RotateDelay);
                    }
                }
                else
                {
                    sync.CatchAndWait = true;
                    if (isChip)
                        sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, RotateDelay, true, kickPowerType.Speed, backSensor);
                    else
                        sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay);
                }
                if (sync.InPassState)
                    inPassState = true;

                if (passed)
                {
                    Planner.Add(PasserID, Model.OurRobots[PasserID].Location, (ShootTarget - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    if (Mode != 2)
                    {
                        if (Model.OurRobots[Defender1ID].Location.X < 0)
                        {
                            Position2D tmpPos = new Position2D(2, Model.OurRobots[Defender1ID].Location.Y);
                            Planner.Add(Defender1ID, tmpPos, (tmpPos - Model.OurRobots[Defender1ID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                        }
                        else
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderCornerRole1)))
                                Functions[Defender1ID] = (eng, wmd) => GetRole<DefenderCornerRole1>(Defender1ID).Run(eng, wmd, Defender1ID, Defender1Pos, Defender1Ang);
                        }

                    }
                    if (Mode == 0)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser2ID, typeof(DefenderCornerRole2)))
                            Functions[Poser2ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Poser2ID).Run(eng, wmd, Poser2ID, Defender2Pos, Defender2Ang);
                    }
                }
                else
                {
                    if (Mode == 0)
                        Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {

                sync.CatchAndWait = false;
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, KickSpeed, RotateDelay, true, kickPowerType.Speed, backSensor);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, KickSpeed, true, kickPowerType.Speed, RotateDelay);
                if (passed)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                        Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).Perform(eng, wmd, PasserID, null);

                    if (Model.OurRobots[Defender1ID].Location.X < 0)
                    {
                        Position2D tmpPos = new Position2D(2, Model.OurRobots[Defender1ID].Location.Y);
                        Planner.Add(Defender1ID, tmpPos, (tmpPos - Model.OurRobots[Defender1ID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                    else
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderCornerRole1)))
                            Functions[Defender1ID] = (eng, wmd) => GetRole<DefenderCornerRole1>(Defender1ID).Run(eng, wmd, Defender1ID, Defender1Pos, Defender1Ang);
                    }

                    if (Model.OurRobots[Defender2ID].Location.X < 0)
                    {
                        Position2D tmpPos = new Position2D(2, Model.OurRobots[Defender2ID].Location.Y);
                        Planner.Add(Defender2ID, tmpPos, (tmpPos - Model.OurRobots[Defender2ID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                    else
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(DefenderCornerRole2)))
                            Functions[Defender2ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Defender1ID).Run(eng, wmd, Defender2ID, Defender2Pos, Defender2Ang);
                    }
                }
                else
                    Planner.Add(PasserID, secondPassTarget, (Model.BallState.Location - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng, new DefenceInfo(), Defender1Pos, Defender1ID, true);

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
