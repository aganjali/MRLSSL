using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Strategies
{
    public class LineCornerStrategy : StrategyBase
    {
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 30, finishTresh = 200, finish2Tresh = 150, initDist = 0.22,
           maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistShoot = 0.5,
           faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, passSpeedTresh = StaticVariables.PassSpeedTresh, secondPassTresh = 0.5, shooterHaveBallTresh = 0.3, KickFailedTresh = 0.5, faildBallMovedDist2 = 0.2, maxFaildMovedDist2 = 0.4;

        Syncronizer sync;
        int PasserID, Defender1ID, ShooterID, Poser1ID, Poser2ID, Poser3ID;
        Position2D Defender1Pos, Defender2Pos, GoaliePos, Pos1, Pos2, Pos3, Pos4, PasserPos, firstBallPos, firstBallPos2;
        Position2D ShootTarget, PassTarget, secondPassTarget;
        double Defender1Ang, Defender2Ang, GoalieAng, PasserAngle, PassSpeed, KickSpeed, sgn;
        bool second,passed, first, firstInState, inPassState, chipOrigin, isChip, backSensor, isBallNearShooter;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        int Mode, SubMode;
        CatchAndRotateBallRole catchnrot;
        public override void ResetState()
        {
            SubMode = 1;
            Mode = 1;
            catchnrot = new CatchAndRotateBallRole();
            passed = false;
            first = true;
            second = true;
            firstInState = true;
            inPassState = false;
            chipOrigin = false;
            isChip = false;
            backSensor = true;
            RotateDelay = 60;
            isBallNearShooter = false;
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
            StrategyName = "LineCorner";
            AttendanceSize = 6;
            About = "this strategy use a line of robots!!!";
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
                PassTarget = new Position2D(-1, sgn * 2.3);
                secondPassTarget = new Position2D(-2.6, -sgn * 1.7);
                ShootTarget = GameParameters.OppGoalCenter;
                KickSpeed = StaticVariables.MaxKickSpeed;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                Pos1 = new Position2D(-1.5, sgn * 2);
                Pos2 = new Position2D(-2.0, sgn * 2);
                Pos3 = new Position2D(-2.5, sgn * 2);
                Pos4 = new Position2D(-3, sgn * 1);

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
                    && Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < tresh
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < tresh)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    finishCounter = 0;
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
                if (finishCounter > finish2Tresh)
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
                    CurrentState = (int)State.SecondPass;
                    firstInState = true;
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (second)
                {
                    firstBallPos2 = Model.BallState.Location;
                    second = false;
                }
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

                Vector2D refrence = secondPassTarget - PassTarget;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (Model.BallState.Location.DistanceFrom(firstBallPos2) > faildBallMovedDist2 && Model.BallState.Location.DistanceFrom(firstBallPos2) < maxFaildMovedDist2)
                {
                    faildCounter++;
                    if (faildCounter > 3)
                        CurrentState = (int)State.Finish;
                }
                else
                    faildCounter = Math.Max(0, faildCounter - 1);

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[Poser2ID].Location) < shooterHaveBallTresh)
                    isBallNearShooter = true;
                if (isBallNearShooter && Model.BallState.Location.DistanceFrom(Model.OurRobots[Poser2ID].Location) > KickFailedTresh)
                {
                    CurrentState = (int)State.Finish;
                }
            }
            DrawingObjects.AddObject(new Circle(firstBallPos2, 0.5, new System.Drawing.Pen(Color.Cyan, 0.2f)), "dfgdf");
            #endregion

            #region Pos
            if (CurrentState == (int)State.First)
            {
                if (firstInState)
                {
                }

            }
            else if (CurrentState == (int)State.FirstPass)
            {
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
                        PassSpeed = Math.Max(Model.BallState.Location.DistanceFrom(PassTarget) * 0.5, 1);
                }
                else //if (Model.OurRobots[ShooterID].Location.DistanceFrom(Model.BallState.Location) < 1)
                {
                    if (firstInState)
                    {
                        if (Mode == 1)
                        {
                            var tmp = Poser1ID;
                            Poser1ID = Poser2ID;
                            Poser2ID = tmp;
                        }

                        firstInState = false;
                    }
                    Pos3 = new Position2D(-3, sgn * 0.2);
                    Defender1ID = Poser1ID;
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    passed = false;
                    firstInState = false;
                    KickSpeed = Math.Max(Model.BallState.Location.DistanceFrom(secondPassTarget) * 0.75, 1.7);
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
            //-------------------------------------------------------------------------------------------------------------------------------------------
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            ShooterID = tmpIds[0];
            Defender1ID = -1;
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
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(ShooterID, PassTarget, (ShootTarget - PassTarget).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(PasserID, PasserPos, (ShootTarget - PasserPos).AngleInDegrees, PathType.UnSafe, true, true, true, true);
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                sync.CatchAndWait = true;
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, RotateDelay, true, kickPowerType.Speed, backSensor);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay);

                if (sync.InPassState)
                    inPassState = true;

                if (passed)
                {
                    if (sync.InRotate)
                        Planner.Add(Poser2ID, secondPassTarget, (PassTarget - secondPassTarget).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    else
                        Planner.Add(Poser2ID, Model.OurRobots[Poser2ID].Location, (ShootTarget - Model.OurRobots[Poser2ID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);

                    Planner.Add(PasserID, Pos4, (ShootTarget - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                }

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                sync.CatchAndWait = false;
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, KickSpeed, RotateDelay, true, kickPowerType.Speed, backSensor);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, secondPassTarget, PassSpeed, KickSpeed, true, kickPowerType.Speed, RotateDelay);
                if (!passed)
                {
                    Planner.Add(Poser2ID, secondPassTarget, (PassTarget - secondPassTarget).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                }
                else
                {
                    //Planner.Add(ShooterID, Model.OurRobots[ShooterID].Location, (ShootTarget - secondPassTarget).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(DefenderCornerRole2)))
                        Functions[ShooterID] = (eng, wmd) => GetRole<DefenderCornerRole2>(ShooterID).Run(eng, Model, ShooterID, Defender1Pos, Defender2Ang);
                    if (SubMode == 0)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser2ID, typeof(ActiveRole)))
                            Functions[Poser2ID] = (eng, wmd) => GetRole<ActiveRole>(Poser2ID).Perform(eng, wmd, Poser2ID, null);
                    }
                    else
                    {
                        catchnrot.CatchAndRotate(engine, Model, Poser2ID, Model.OurRobots[Poser2ID], ShootTarget, true, false, kickPowerType.Speed, true, true, Program.MaxKickSpeed, true, RotateDelay);
                    }
                }

                Planner.Add(PasserID, Pos4, (ShootTarget - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);

            }

            if (Defender1ID != -1)
            {
                //if (Model.OurRobots[Defender1ID].Location.X < 0)
                //{
                //    var tmpPos = new Position2D(2, Model.OurRobots[Defender1ID].Location.Y);
                //    Planner.Add(Defender1ID, tmpPos, (ShootTarget - Model.OurRobots[Defender1ID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                //}
                //else
                //{
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderCornerRole1)))
                    Functions[Defender1ID] = (eng, wmd) => GetRole<DefenderCornerRole1>(Defender1ID).Run(eng, wmd, Defender1ID, Defender1Pos, Defender2Ang);
                //}
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieNormalRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieNormalRole>(Model.GoalieID.Value).Run(eng, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng);// TODO: Change to static goalie corner  role



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
