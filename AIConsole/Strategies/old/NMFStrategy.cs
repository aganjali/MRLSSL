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
    public class NMFStrategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2, passSpeedTresh = StaticVariables.PassSpeedTresh;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 60, finishTresh = 200, initDist = 0.22,
            maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5,
            faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, finish2Tresh = 100;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        const double margin = 0.25;
        const double distBehindBallTresh = 0.07;
        Syncronizer sync;
        CatchAndRotateBallRole catchnRot;
        Position2D firstBallPos, secondBallPos;
        Position2D PasserPos;
        Position2D Pos1, Pos2, Pos3;
        Position2D DefenderPos, GoaliePos, ShooterPos;
        Position2D PassTarget, SecondPassTarget, Mode2PassTarget;
        Position2D ShootTarget;
        double PasserAngle, DefenderAng, GoalieAng, Poser1Ang, Poser2Ang, Poser3Ang, ShooterAng;
        double PassSpeed;
        double KickSpeed;
        int PasserID, DefenderID, ShooterID, Poser1ID, Poser2ID, Poser3ID;
        int Mode;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        int rotateCounter;
        bool inRotate;
        bool isChip, chipOrigin;
        bool passed;
        bool firstInState;
        bool inPassState;
        bool backSensor;
        bool first;


        public override void ResetState()
        {
            Mode = 3;
            first = true;
            firstInState = true;
            PasserID = -1;
            DefenderID = -1;
            ShooterID = -1;
            Poser1ID = -1;
            Poser2ID = -1;
            Poser3ID = -1;
            firstBallPos = Position2D.Zero;
            passed = false;
            timeLimitCounter = 0;
            PasserPos = Position2D.Zero;
            Pos1 = Position2D.Zero;
            Pos2 = Position2D.Zero;
            Pos3 = Position2D.Zero;
            DefenderPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = Position2D.Zero;
            GoaliePos = Position2D.Zero;
            ShooterPos = Position2D.Zero;

            DefenderAng = 0;
            GoalieAng = 0;
            Poser1Ang = 0;
            Poser2Ang = 0;
            Poser3Ang = 0;
            ShooterAng = 0;
            PasserAngle = 0;

            RotateDelay = 60;
            inRotate = false;
            rotateCounter = 2;
            isChip = false;
            chipOrigin = false;
            PassSpeed = 0;
            backSensor = true;
            inPassState = false;
            KickSpeed = 8;
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
            catchnRot = new CatchAndRotateBallRole();
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
            StrategyName = "NMF";
            AttendanceSize = 6;
            About = "this strategy will try to make good opportunity for our normal play..!!!";
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
            //double sgn = 0;
            CalculateDefenderInfo(engine, Model, out DefenderPos, out GoaliePos, out DefenderAng, out GoalieAng);
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                double sgn = Math.Sign(firstBallPos.Y);
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                Pos1.X = -2.7;
                Pos1.Y = sgn * 0.3;
                Pos2 = Pos1.Extend(0, sgn * 0.3);//= oppGoalCircle.Intersect(new Line(oppGoal, Pos1.Extend(RobotParameters.OurRobotParams.Diameter, RobotParameters.OurRobotParams.Diameter))).OrderByDescending(t => t.DistanceFrom(Position2D.Zero)).First();
                Pos3 = Pos1.Extend(0, -sgn * 0.3);// = oppGoalCircle.Intersect(new Line(oppGoal, Pos1.Extend(-RobotParameters.OurRobotParams.Diameter, -RobotParameters.OurRobotParams.Diameter))).OrderByDescending(t => t.DistanceFrom(Position2D.Zero)).First();

                PassTarget = new Position2D(-0.5, 0);
                SecondPassTarget = new Position2D(-2.5, sgn * 0.4);
                Mode2PassTarget = new Position2D(-3.5, -sgn * 1.6);
                AssignIDs(engine, Model, Attendance);
                first = false;
            }

            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if ((PasserID != -1 && !Model.OurRobots.ContainsKey(PasserID)) || (ShooterID != -1 && !Model.OurRobots.ContainsKey(ShooterID))
                || (Poser1ID != -1 && !Model.OurRobots.ContainsKey(Poser1ID)) || !Model.OurRobots.ContainsKey(Poser3ID))
                return;
            #endregion

            #region State
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[Poser1ID].Location.DistanceFrom(Pos1) < 0.01
                    && Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < 0.01
                    && Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < 0.01
                    && Model.OurRobots[DefenderID].Location.DistanceFrom(DefenderPos) < 0.01)
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
                if (finishCounter > finishTresh + wait2Tresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if (sync.CatchState == 1)
                    {

                    }
                    else
                    {
                        if ((v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistSecondPass))
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
                if (sync.CatchState == (int)CatchAndRotateBallRole.State.Rotate)
                {
                    if (++counter > wait2Tresh)
                    {
                        finishCounter = 0;
                        if (Mode == 0)
                        {
                            counter = 0;
                            CurrentState = (int)State.SecondPass;
                            firstInState = true;
                            secondBallPos = Model.BallState.Location;
                        }
                        else if (Mode == 1 || Mode == 2 || Mode == 3)
                        {
                            counter = 0;
                            CurrentState = (int)State.Finish;
                            firstInState = true;
                            secondBallPos = Model.BallState.Location;
                        }
                    }
                }


            }
            else if (CurrentState == (int)State.SecondPass)
            {
                Vector2D refrence = SecondPassTarget - secondBallPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    finishCounter++;
                    if ((v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistSecondPass))
                    {
                        faildCounter++;
                        if (faildCounter > faildMaxCounter)
                            CurrentState = (int)State.Finish;
                    }
                    else
                        faildCounter = Math.Max(0, faildCounter - 5);

                    if (catchnRot.CurrentState == (int)CatchAndRotateBallRole.State.Rotate)
                        CurrentState = (int)State.Finish;
                }

                if (finishCounter > finish2Tresh)
                    CurrentState = (int)State.Finish;
            }
            #endregion

            #region Pos and Ang
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
                    Planner.SetReCalculateTeta(PasserID, true);
                    firstInState = false;
                    ShooterID = DefenderID;
                    DefenderID = -1;
                }

                if (inPassState && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed)
                {
                    if (Mode == 2 || Mode == 3)
                    {
                        isChip = chipOrigin = true;
                    }
                    else
                    {
                        isChip = chipOrigin;
                        if (!chipOrigin)
                        {
                            Obstacles obs = new Obstacles(Model);
                            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                            isChip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);
                        }
                    }
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, false, false);
                    if (isChip)
                    {
                        if (Mode == 2 || Mode == 3)
                        {
                            PassSpeed = Math.Max(Model.BallState.Location.DistanceFrom(Mode2PassTarget) * 0.6, 3);
                        }
                        else
                            PassSpeed = Math.Max(Model.BallState.Location.DistanceFrom(PassTarget) * 0.6, 1);
                    }
                    //DrawingObjects.AddObject(new StringDraw("Pass Speed: " + PassSpeed.ToString(), new Position2D(-1, 1)), "fsdpassspeed");
                }

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    DefenderID = PasserID;
                    PasserID = ShooterID;
                    ShooterID = Poser2ID;
                    Poser2ID = -1;

                    passed = false;
                    inPassState = false;
                    firstInState = false;
                }
                if (sync.CatchKicked && Model.BallState.Speed.Size > 0.2)
                    passed = true;
                if (!passed)
                {
                    isChip = chipOrigin = true;

                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, SecondPassTarget, isChip, true);
                    if (isChip)
                        PassSpeed = Math.Max(Model.BallState.Location.DistanceFrom(SecondPassTarget) * 0.8, 1);
                }
            }

            #endregion

            /* DrawingObjects.AddObject(new StringDraw("poser1 ID: " + Poser1ID.ToString(), new Position2D(-1.2, 1)), "fsddvsd");
            DrawingObjects.AddObject(new StringDraw("poser2 ID: " + Poser2ID.ToString(), new Position2D(-1.4, 1)), "fsddvssdfd");
            DrawingObjects.AddObject(new StringDraw("poser3 ID: " + Poser3ID.ToString(), new Position2D(-1.6, 1)), "fsddvsdsdfdsh");
            DrawingObjects.AddObject(new StringDraw("defender ID: " + DefenderID.ToString(), new Position2D(-1.8, 1)), "fsddkhjvsd");
            DrawingObjects.AddObject(new StringDraw("passer ID: " + PasserID.ToString(), new Position2D(-2, 1)), "fsddvstyhd");
            DrawingObjects.AddObject(new StringDraw("shooter ID: " + ShooterID.ToString(), new Position2D(-2.2, 1)), "fsddvsvfdvtyhd");
            DrawingObjects.AddObject(new StringDraw("Current State: " + CurrentState.ToString(), new Position2D(-2.4, 1)), "fsddvsvfdvtvcsyhd");*/
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
            DefenderID = minIdx;
            tmpIds.Remove(minIdx);

            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Pos3) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Pos3);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Poser3ID = minIdx;
            tmpIds.Remove(minIdx);

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
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            Poser1ID = tmpIds[0];
            ShooterID = -1;
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3.5, 2);
                if (Planner.AddRotate(Model, PasserID, ShootTarget, PasserPos, kickPowerType.Speed, PassSpeed, isChip, rotateCounter, true).IsInRotateDelay)
                {
                    rotateCounter++;
                }
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                    Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);
                //Planner.Add(ShooterID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.UnSafe, true, true, true, true);
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                if (Mode == 0)
                {
                    sync.CatchAndWait = true;
                    Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (isChip)
                        sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, RotateDelay, true, kickPowerType.Speed, backSensor, rotateCounter);
                    else
                        sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay, rotateCounter);
                }
                else if (Mode == 1 || Mode == 2 || Mode == 3)
                {
                    Planner.Add(ShooterID, PassTarget, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (Mode == 1)
                    {
                        if (isChip)
                            sync.SyncChipCatch(engine, Model, PasserID, PasserPos, Poser1ID, SecondPassTarget, ShootTarget, PassSpeed, StaticVariables.MaxKickSpeed, RotateDelay, false, kickPowerType.Speed, backSensor, rotateCounter);
                        else
                            sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, Poser1ID, SecondPassTarget, ShootTarget, PassSpeed, StaticVariables.MaxKickSpeed, false, kickPowerType.Speed, RotateDelay, rotateCounter);

                        Planner.Add(Poser2ID, Pos2.Extend(-.3, 0), (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, false, true, true);
                        Planner.Add(Poser3ID, Pos3.Extend(-.3, 0), (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, false, true, true);
                    }
                    else if (Mode == 2)
                    {
                        if (isChip)
                            sync.SyncChipCatch(engine, Model, PasserID, PasserPos, Poser3ID, Mode2PassTarget, ShootTarget, PassSpeed, StaticVariables.MaxKickSpeed, RotateDelay, false, kickPowerType.Speed, backSensor, rotateCounter);
                        else
                            sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, Poser3ID, Mode2PassTarget, ShootTarget, PassSpeed, StaticVariables.MaxKickSpeed, false, kickPowerType.Speed, RotateDelay, rotateCounter);

                        Planner.Add(Poser2ID, Pos2.Extend(-.3, 0), (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, false, true, true);
                        Planner.Add(Poser1ID, Pos1.Extend(-.3, 0), (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, false, true, true);
                    }
                    else if (Mode == 3)
                    {
                        if (isChip)
                            sync.SyncChipPass(engine, Model, PasserID, PasserPos, Poser1ID, Mode2PassTarget, ShootTarget, PassSpeed, StaticVariables.MaxKickSpeed, RotateDelay, backSensor, rotateCounter);
                        else
                            sync.SyncDirectPass(engine, Model, PasserID, PasserPos, Poser1ID, Mode2PassTarget, ShootTarget, PassSpeed, StaticVariables.MaxKickSpeed, RotateDelay, rotateCounter);

                        Planner.Add(Poser2ID, Pos2.Extend(-.3, 0), (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, false, true, true);
                        Planner.Add(Poser3ID, Pos3.Extend(-.3, 0), (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, false, true, true);
                    }
                }

                if (sync.InPassState)
                    inPassState = true;

                if (passed)
                {
                    if (Model.OurRobots[PasserID].Location.X >= 0)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(DefenderCornerRole1)))
                            Functions[PasserID] = (eng, wmd) => GetRole<DefenderCornerRole1>(PasserID).Run(eng, wmd, PasserID, DefenderPos, DefenderAng);
                    }
                    else
                    {
                        Position2D tempPos = new Position2D(3, Model.OurRobots[PasserID].Location.Y);
                        Planner.Add(PasserID, tempPos, (tempPos - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    }
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                sync.CatchAndWait = false;
                Planner.Add(Poser3ID, Pos3.Extend(-.4, 0), (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2.Extend(-.4, 0), (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, true);

                if (isChip)
                    sync.SyncChipCatch(engine, Model, DefenderID, PasserPos, PasserID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, RotateDelay, true, kickPowerType.Speed, backSensor, rotateCounter);
                else
                    sync.SyncDirectCatch(engine, Model, DefenderID, PasserPos, PasserID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay, rotateCounter);

                if (passed)
                {
                    catchnRot.CatchAndRotate(engine, Model, Poser1ID, Model.OurRobots[Poser1ID], ShootTarget, true, false, kickPowerType.Speed, true, true, KickSpeed, true, RotateDelay);
                }
                else
                {
                    Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                }

                if (DefenderID != -1)
                {
                    if (Model.OurRobots[DefenderID].Location.X >= 0)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                            Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);
                    }
                    else
                    {
                        Position2D tempPos = new Position2D(-Model.OurRobots[DefenderID].Location.X, Model.OurRobots[DefenderID].Location.Y);
                        Planner.Add(DefenderID, tempPos, (tempPos - Model.OurRobots[DefenderID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    }
                }
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng, new DefenceInfo(), DefenderPos, DefenderID, true);

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
