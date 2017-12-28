using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Strategies
{
    public class FacePunchStrategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2, passSpeedTresh = StaticVariables.PassSpeedTresh;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 60, finishTresh = 200, initDist = 0.22, maxWaitTresh = 140, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8, CurveTresh = 0.5, maxFakeWait = 100, changePosDist = 0.3;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        int PasserID, ShooterID, Poser0ID, Poser1ID, Poser2ID, DefenderID;
        Position2D firstBallPos, secondBallPos;
        bool passed;
        bool first;
        bool firstInState;
        bool isChip, chipOrigin;
        bool inPassState;
        bool backSensor, nearShooter, shooted;
        bool inRotate;
        Syncronizer sync;
        Position2D PasserPos;
        Position2D Pos0, Pos1, Pos2, FakePos0, FakePos1;
        Position2D GoaliePos, DefPos, DefenderPos;
        Position2D PassTarget, SecondPassTarget;
        Position2D ShootTarget;
        double PasserAngle, GoalieAng, DefAngle, DefenderAng;
        double PassSpeed;
        double KickSpeed, sgn;
        int Mode;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;

        public override void ResetState()
        {
            Mode = 0;
            first = true;
            firstInState = true;
            PasserID = -1;
            ShooterID = -1;
            Poser1ID = -1;
            Poser2ID = -1;
            firstBallPos = Position2D.Zero;
            passed = false;
            timeLimitCounter = 0;
            PasserPos = Position2D.Zero;
            Pos1 = Position2D.Zero;
            Pos2 = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = Position2D.Zero;
            GoaliePos = Position2D.Zero;
            nearShooter = false;
            shooted = false;
            PasserAngle = 0;

            RotateDelay = 60;
            inRotate = false;
            isChip = false;
            chipOrigin = false;
            PassSpeed = 0;
            backSensor = true;
            inPassState = false;
            KickSpeed = StaticVariables.MaxKickSpeed;
            SecondPassTarget = Position2D.Zero;
            secondBallPos = Position2D.Zero;
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
            StrategyName = "FacePunch";
            AttendanceSize = 6;
            About = "Just a Punch in the Face!!!";
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
            CalculateDefenderInfo(engine, Model, out DefenderPos, out GoaliePos, out DefenderAng, out GoalieAng);
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                sgn = Math.Sign(firstBallPos.Y);
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                PassTarget = new Position2D(-3.5, -sgn * 2);
                Pos0 = new Position2D(-3.7, sgn * 2);
                Pos1 = new Position2D(-1, sgn * 2.4);
                Pos2 = new Position2D(-0.2, -sgn * 0.3);
                FakePos0 = new Position2D(0, sgn * 0.3);
                FakePos1 = new Position2D(1, -sgn * 2);
                Vector2D tmpVec = (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.2);
                DefAngle = tmpVec.AngleInDegrees;
                DefPos = GameParameters.OurGoalCenter + tmpVec;

                AssignIDs(engine, Model, Attendance);
                first = false;
            }

            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if ((PasserID != -1 && !Model.OurRobots.ContainsKey(PasserID)) || (ShooterID != -1 && !Model.OurRobots.ContainsKey(ShooterID))
                || (Poser1ID != -1 && !Model.OurRobots.ContainsKey(Poser1ID)) || !Model.OurRobots.ContainsKey(Poser2ID) || !Model.OurRobots.ContainsKey(Poser0ID))
                return;

            #endregion
            #region State
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[Poser0ID].Location.DistanceFrom(Pos0) < tresh
                    && Model.OurRobots[Poser1ID].Location.DistanceFrom(Pos1) < tresh
                    && Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < tresh
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(DefPos) < tresh)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.FakeMove;
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
            else if (CurrentState == (int)State.FakeMove)
            {
                timeLimitCounter++;
                if (timeLimitCounter > maxFakeWait || (Model.OurRobots.ContainsKey(ShooterID) && Model.OurRobots[ShooterID].Location.DistanceFrom(FakePos1) < CurveTresh))
                {
                    CurrentState = (int)State.Go;
                    firstInState = true;
                    timeLimitCounter = 0;
                    counter = 0;
                }
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
                if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[ShooterID].Location - Model.OurRobots[PasserID].Location) <= 0)
                    shooted = true;
                if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > 0.35)
                    CurrentState = (int)State.Finish;
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
            else if (CurrentState == (int)State.FakeMove)
            {
                if (firstInState)
                {
                    firstInState = false;
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                if (firstInState)
                {
                    DefenderID = Poser1ID;
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
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.5;

                }
            }

            #endregion
        }
        private void CalculateDefenderInfo(GameStrategyEngine engine, WorldModel Model, out Position2D defPos, out Position2D goalipos, out double defAng, out double goaliang)
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
            var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
            var gol = infos.SingleOrDefault(s => s.RoleType == typeof(GoalieCornerRole));
            var normal1 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole1));
            goalipos = (gol.DefenderPosition.HasValue) ? gol.DefenderPosition.Value : Position2D.Zero;
            goaliang = gol.Teta;
            defPos = (normal1.DefenderPosition.HasValue) ? normal1.DefenderPosition.Value : Position2D.Zero;
            defAng = normal1.Teta;
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
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(PasserPos) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(PasserPos);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            PasserID = minIdx;
            tmpIds.Remove(PasserID);
            //---------------------------------------------------------------------------------------------------
            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Pos0) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Pos0);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Poser0ID = minIdx;
            tmpIds.Remove(Poser0ID);
            //---------------------------------------------------------------------------------------------------
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
            tmpIds.Remove(Poser1ID);
            //---------------------------------------------------------------------------------------------------
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
            tmpIds.Remove(Poser2ID);
            //---------------------------------------------------------------------------------------------------
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            DefenderID = ShooterID = tmpIds[0];
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3.5, 2);
                Planner.Add(PasserID, PasserPos, PasserAngle, PathType.Safe, true, true, true, true);
                Planner.Add(ShooterID, DefPos, DefAngle, PathType.Safe, true, true, true, true);
                Planner.Add(Poser0ID, Pos0, (ShootTarget - Pos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
            }
            else if (CurrentState == (int)State.FakeMove)
            {
                Planner.Add(PasserID, PasserPos, PasserAngle, PathType.Safe, true, true, true, true);
                Planner.Add(Poser0ID, Pos0, (ShootTarget - Pos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                if (Model.OurRobots.ContainsKey(ShooterID) && Model.OurRobots[ShooterID].Location.DistanceFrom(FakePos0) > changePosDist)
                    Planner.Add(ShooterID, FakePos0, 180, PathType.Safe, true, true, true, true);
                else
                    Planner.Add(ShooterID, FakePos1, 180, PathType.Safe, true, true, true, true);
            }
            else if (CurrentState == (int)State.Go)
            {
                sync.CatchAndWait = false;
                if (Mode == 0)
                {
                    if (isChip)
                        sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, false, kickPowerType.Speed, backSensor);
                    else
                        sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, false, kickPowerType.Speed, RotateDelay);
                }
                if (passed)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                        Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);
                }
                else
                {
                    Planner.Add(Poser0ID, Pos0, (ShootTarget - Pos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng, new DefenceInfo(), DefenderPos, DefenderID, true);


            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public enum State
        {
            First,
            FakeMove,
            Go,
            Finish
        }
    }
}
