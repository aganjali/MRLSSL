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
    class PassShootCornerStrategy : StrategyBase
    {

        const double step = 0.5, passerShooterDist = 1;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 80, initDist = 0.25, maxWaitTresh = 600;
        bool first, passTargetCalculated, Debug = true;
        int PasserID, ShooterID, Poser3ID, Poser1ID, Poser2ID;
        Position2D PasserPos, ShooterPos, PassTarget, ShootTarget;
        double PasserAngle, ShooterAngle, RotateTeta, PassSpeed, KickPower;
        int counter, finishCounter, RotateDelay, timeLimitCounter;
        Syncronizer sync;
        bool chip, passed, chipOrigin;
        Position2D firstBallPos;
        bool backSensor;

        bool inrot = false, inPassState = false;
        private Position2D Pos1;
        private Position2D Pos2;
        private Position2D Pos3;

        public override void ResetState()
        {
            UseInMiddle = true;
            backSensor = false;
            firstBallPos = Position2D.Zero;
            chip = false;
            chipOrigin = false;
            passed = false;
            CurrentState = InitialState;
            first = true;
            passTargetCalculated = false;
            RotateTeta = 90;
            PassSpeed = 4;
            KickPower = Program.MaxKickSpeed;
            timeLimitCounter = 0;
            PasserID = -1;
            ShooterID = -1;

            PasserPos = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;

            PasserAngle = 0;
            ShooterAngle = 0;
            counter = 0;
            finishCounter = 0;
            RotateDelay = 60;
            inPassState = false;

            inrot = false;

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
            UseOnlyInMiddle = false;
            StrategyName = "Simple Corner Pass Shoot";
            AttendanceSize = 6;
            UseInMiddle = true;
            About = "this is Corner Simple Pass Shoot strategy!";
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
                var tmpIds = RemoveGoaliID(Model, Attendance);
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

                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && -Model.OurRobots[item].Location.X < minDist)
                    {
                        minDist = -Model.OurRobots[item].Location.X;
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                ShooterID = minIdx;
                tmpIds.Remove(minIdx);

                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.X;
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                Poser2ID = minIdx;
                tmpIds.Remove(minIdx);

                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.X;
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                Poser1ID = minIdx;
                tmpIds.Remove(minIdx);
                if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                    return;
                Poser3ID = tmpIds[0];

                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (!Model.OurRobots.ContainsKey(PasserID) || !Model.OurRobots.ContainsKey(ShooterID))
                return;

            #region States
            if (CurrentState == (int)State.First)
            {
                double dAngle = Model.OurRobots[PasserID].Angle.Value - PasserAngle;
                timeLimitCounter++;
                if (dAngle > 180)
                    dAngle -= 360;
                else if (dAngle < -180)
                    dAngle += 360;

                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < tresh)
                    counter++;
                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    timeLimitCounter = 0;
                    CurrentState = (int)State.Go;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.06)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Go)
            {
                timeLimitCounter++;
                if (passed)
                {
                    finishCounter++;
                }
                if (sync.Finished || sync.Failed || finishCounter > finishTresh || timeLimitCounter > maxWaitTresh)
                    CurrentState = (int)State.Finish;
            }
            #endregion
            #region PosAndAngles
            if (CurrentState == (int)State.First)
            {
                Pos1 = new Position2D(-2.85, -Math.Sign(Model.BallState.Location.Y) * 1.9);
                Pos2 = new Position2D(0.65, -Math.Sign(Model.BallState.Location.Y) * 0.75);
                Pos3 = new Position2D(1, Math.Sign(Model.BallState.Location.Y) * 1.2);
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                ShooterPos = new Position2D(-1.5, Math.Sign(Model.BallState.Location.Y));
                ShooterAngle = 180;
                inrot = false;
            }
            else if (CurrentState == (int)State.Go)
            {

                if (!passTargetCalculated)
                {
                    ShootTarget = GameParameters.OppGoalCenter;//(Model.BallState.Location.Y >= 0) ? GameParameters.OppGoalRight : GameParameters.OppGoalLeft;
                    PassTarget = ShooterPos;
                    chipOrigin = chip;
                    double goodness;

                    var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, PassTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    if (GoodPointInGoal.HasValue)
                        ShootTarget = GoodPointInGoal.Value;
                    passTargetCalculated = true;
                    Pos2 = new Position2D(0.3, Math.Sign(Model.BallState.Location.Y) * 0.2);
                    Pos3 = new Position2D(1.1, Math.Sign(Model.BallState.Location.Y) * 0.1);
                }
                if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05)
                    passed = true;

                chip = chipOrigin;
                if (!passed && !chipOrigin)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.15);
                }

                if (!passed)
                {
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, false, true);
                    if (chip)
                    {
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.65;
                    }
                }
            }

            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {

            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);

            if (CurrentState == (int)State.First)
            {
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                    Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(engine, Model, PasserID, ShootTarget, false, initDist);
                Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.Safe, true, true, true, true);
            }
            else if (CurrentState == (int)State.Go)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));
                if (!chip)
                    sync.SyncDirectPass(engine, Model, PasserID, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                else
                    sync.SyncChipPass(engine, Model, PasserID, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
                if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.15)
                {
                    Planner.Add(PasserID, Model.OurRobots[PasserID].Location, 180, PathType.UnSafe, false, true, true, true);
                }
                inPassState = sync.InPassState;
                if (passed)
                {
                    Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else
                {
                    Planner.Add(Poser2ID, Model.OurRobots[Poser2ID].Location, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(Poser3ID, Model.OurRobots[Poser3ID].Location, (ShootTarget - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
            }
            return CurrentlyAssignedRoles;
        }

        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }

        public enum State
        {
            First,
            Go,
            Finish
        }
    }
}
