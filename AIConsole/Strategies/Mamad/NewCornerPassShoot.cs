using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;

namespace MRL.SSL.AIConsole.Strategies.new_RC2017
{
    class NewCornerPassShoot : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2, passSpeedTresh = StaticVariables.PassSpeedTresh;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 30, finishTresh = 200, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        int PasserID, ShooterID, PoserID1, PoserID2;
        bool first;
        Position2D firstBallPos;
        bool chip, passed, chipOrigin;
        Syncronizer sync;
        double PasserAngle, ShooterAngle, Poser1Angle, Poser2Angle, RotateTeta, PassSpeed, KickPower;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        int rotateCounter;
        Position2D Passerpos, ShooterPos, PoserPos1, PoserPos2, ShooterFirstPos;
        Position2D ShootTarget;
        bool inrot = false, inPassState = false;
        bool passTargetCalculated;
        Position2D PassTarget;
        Position2D center = new Position2D(0, 0);

        public override void ResetState()
        {
            PasserID = -1;
            ShooterID = -1;
            PoserID1 = -1;
            PoserID2 = -1;

            PasserAngle = 0;
            ShooterAngle = 0;
            Poser1Angle = 0;
            Poser2Angle = 0;

            timeLimitCounter = 0;
            rotateCounter = 2;
            RotateDelay = 60;
            PassSpeed = 4;
            RotateTeta = 90;


            first = true;
            passTargetCalculated = false;
            inPassState = false;

            firstBallPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            Passerpos = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PoserPos1 = Position2D.Zero;
            PoserPos2 = Position2D.Zero;
            ShooterFirstPos = Position2D.Zero;

            if (sync != null)
            {
                sync.Reset();
            }
            else
            {
                sync = new Syncronizer();
            }
        }

        public override void InitializeStates(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> attendance)
        {
            Attendance = attendance;
            CurrentState = (int)State.First;
            InitialState = 0;
            FinalState = 2;
            TrapState = 2;
        }

        public override void FillInformation()
        {
            StrategyName = "M_NewCornerPassShoot_Corner";
            AttendanceSize = 4;
            About = "it's new corner simple pass shoot !!!";
        }

        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, ref GameStatus Status)
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
                firstBallPos = Model.BallState.Location;
                var tmpIds = RemoveGoaliID(Model, Attendance).OrderByDescending(o => o).ToList();
                if (tmpIds.Count <= 0)
                {
                    return;
                }
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

                PasserID = tmpIds.FirstOrDefault();
                tmpIds.RemoveAt(0);

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

                ShooterID = tmpIds.FirstOrDefault();
                tmpIds.RemoveAt(0);

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

                PoserID1 = tmpIds.FirstOrDefault();
                tmpIds.RemoveAt(0);

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

                PoserID2 = tmpIds.FirstOrDefault();
                tmpIds.RemoveAt(0);


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

                if (Model.OurRobots[PasserID].Location.DistanceFrom(Passerpos) < tresh && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < tresh)
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

            double sgn = Math.Sign(firstBallPos.Y);

            if (CurrentState == (int)State.First)
            {

                Vector2D vec1 = new Vector2D();
                vec1 = (center - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.5);
               




                Vector2D vec2 = new Vector2D();
                vec2 = (center - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.5);
                if ((Model.BallState.Location.X < 0 && Model.BallState.Location.Y < 0) || (Model.BallState.Location.X > 0 && Model.BallState.Location.Y > 0))
                {
                    ShooterPos = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (70 * Math.PI / 180), vec2.Size));
                    PoserPos1 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (60 * Math.PI / 180), vec2.Size));
                    PoserPos2 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (50 * Math.PI / 180), vec2.Size));

                }
                else if ((Model.BallState.Location.X < 0 && Model.BallState.Location.Y > 0) || (Model.BallState.Location.X > 0 && Model.BallState.Location.Y < 0))
                {
                    ShooterPos = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (-70 * Math.PI / 180), vec2.Size));
                    PoserPos1 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (-60 * Math.PI / 180), vec2.Size));
                    PoserPos2 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (-50 * Math.PI / 180), vec2.Size));


                }

                ShootTarget = GameParameters.OppGoalCenter;
                Passerpos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - Passerpos).AngleInDegrees;
                ShooterAngle = 180;
                inrot = false;


            }
            else if (CurrentState == (int)State.Go)
            {

                Vector2D vec1 = new Vector2D();
                vec1 = (center - GameParameters.OppGoalCenter).GetNormalizeToCopy(2);

                Vector2D vec2 = new Vector2D();
                vec2 = (center - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.5);
                if ((Model.BallState.Location.X < 0 && Model.BallState.Location.Y < 0) || (Model.BallState.Location.X > 0 && Model.BallState.Location.Y > 0))
                {
                    ShooterPos = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (55 * Math.PI / 180), vec1.Size));
                    PoserPos1 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (20 * Math.PI / 180), vec2.Size));
                    PoserPos2 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (-30 * Math.PI / 180), vec2.Size));

                }
                else if ((Model.BallState.Location.X < 0 && Model.BallState.Location.Y > 0) || (Model.BallState.Location.X > 0 && Model.BallState.Location.Y < 0))
                {
                    ShooterPos = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (-55 * Math.PI / 180), vec1.Size));
                    PoserPos1 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (-20 * Math.PI / 180), vec2.Size));
                    PoserPos2 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (30 * Math.PI / 180), vec2.Size));


                }

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
                    // Pos1 = new Position2D(0.3, Math.Sign(Model.BallState.Location.Y) * 0.2);
                    // Pos2 = new Position2D(1.1, Math.Sign(Model.BallState.Location.Y) * 0.1);
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


            positionDebugDrawing(ShooterPos, "ShooterPos");
            positionDebugDrawing(ShooterFirstPos, "ShooterFirstPos");
            positionDebugDrawing(PoserPos1, "PoserPos1");
            positionDebugDrawing(PoserPos2, "PoserPos2");
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            if (chip)
            {
             //  sync.kMotionChipCatch = 1.22;
                //sync.kMotionChip = 1.2;
            }
            else
            {
              //  sync.kMotionDirectCatch = 1.2;
                //sync.kMotionDirect = 1;
            }
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.First)
            {
                Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.UnSafe, false, true, true, true, false);
                Planner.Add(PoserID1, PoserPos1, (ShootTarget - PoserPos1).AngleInDegrees, PathType.UnSafe, false, true, true, true, false);
                Planner.Add(PoserID2, PoserPos2, (ShootTarget - PoserPos2).AngleInDegrees, PathType.UnSafe, false, true, true, true, false);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                    Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(engine, Model, PasserID, ShootTarget, false, initDist);

            }
            if (CurrentState == (int)State.Go)
            {

                Planner.Add(PoserID1, PoserPos1, (ShootTarget - PoserPos1).AngleInDegrees, PathType.UnSafe, false, true, true, true, false);
                Planner.Add(PoserID2, PoserPos2, (ShootTarget - PoserPos2).AngleInDegrees, PathType.UnSafe, false, true, true, true, false);

                if (chip)
                    sync.SyncChipCatch(engine, Model, PasserID, Passerpos, ShooterID, PassTarget, ShootTarget, PassSpeed, PassSpeed, RotateDelay, true, kickPowerType.Speed, false, rotateCounter);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, Passerpos, ShooterID, PassTarget, ShootTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay, rotateCounter);

                //if (!chip)
                //    sync.SyncDirectPass(engine, Model, PasserID, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                //else
                //    sync.SyncChipPass(engine, Model, PasserID, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, false);

                if (sync.InPassState)
                    inPassState = true;

                if (passed)
                {
                    //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser3ID, typeof(DefenderCornerRole2)))
                    //  Functions[Poser3ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Poser3ID).Run(eng, wmd, Poser3ID, DefenderPos1, DefenderAng1);

                    //  if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                    //     Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).Perform(eng, wmd, ShooterID, null);


                    Planner.Add(PasserID, Model.OurRobots[PasserID].Location, ShooterAngle, PathType.UnSafe, false, true, true, true, false);
                }

            }
            return CurrentlyAssignedRoles;
        }
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }

        public void positionDebugDrawing(Position2D pos, string posName)
        {
            DrawingObjects.AddObject(new Circle(pos, 0.2), "b" + pos.toString());
            DrawingObjects.AddObject(new StringDraw(posName.ToString(), pos.Extend(0.25, 0)), "aasdasdb" + pos.toString());
        }
        public enum State
        {
            First,
            Go,
            Finish
        }
    }
}
