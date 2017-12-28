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
    public class KarimBagheriStrategy:StrategyBase
    {

        const double tresh = 0.05, stuckTresh = 0.23, angleTresh = 2, waitTresh = 70, finishTresh = 100, initDist = 0.25, maxWaitTresh = 360, passSpeedTresh = StaticVariables.PassSpeedTresh, behindBallTresh = 0.07, fieldMargin = 0.12, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;

        bool first, firstInState, isChip, chipOrigin, changeShooter, shooterFirstMove, passed, goActive, inRotate, goBack, passerMoved, recalcRot, inPassState, nearShooter, shooted;

        bool goDefence = false;
        int[] PositionersID;
        int PasserID, ShooterID, DefenderID, initialPosCounter, finishCounter, timeLimitCounter, RotateDelay, rotateCounter;
        Position2D PasserPos, ShooterPos, DefenderPos, GoaliePos, ShootTarget, PassTarget, firstBallPos, tmpPos0;
        Position2D[] PositionersPos;
        double PasserAng, ShooterAng, DefenderAng, GoalieAng, PassSpeed, KickSpeed, tmpAng0;
        double[] PositionersAng;
        Syncronizer sync;
        int Mode = 0, faildCounter, moveCounter ;
        bool backSensor;
        public override void ResetState()
        {
            backSensor = true;
            goDefence = false;
            moveCounter = 0;
            PositionersID = new int[3];
            PositionersAng = new double[3];
            PositionersPos = new Position2D[3];
            Mode = 0;
            nearShooter = false;
            shooted = false;
            recalcRot = false;
            PasserID = -1;
            ShooterID = -1;
            DefenderID = -1;
            faildCounter = 0;
            PasserAng = 0;
            ShooterAng = 0;
            DefenderAng = 0;
            GoalieAng = 0;

            PassSpeed = 0;
            KickSpeed = Program.MaxKickSpeed;
            initialPosCounter = 0;
            finishCounter = 0;
            timeLimitCounter = 0;
            RotateDelay = 60;
            rotateCounter = 2;
            //moveCounter = 0;
            PasserPos = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            DefenderPos = Position2D.Zero;
            GoaliePos = Position2D.Zero;
            ShootTarget = Position2D.Zero;
            PassTarget = Position2D.Zero;
            firstBallPos = Position2D.Zero;

            chipOrigin = false;
            isChip = false;
            changeShooter = false;
            firstInState = true;
            shooterFirstMove = false;
            passed = false;
            goActive = false;
            inRotate = false;
            goBack = false;
            inPassState = false;
            passerMoved = false;
            tmpPos0 = Position2D.Zero;
            if (sync != null)
                sync.Reset();
            else
                sync = new Syncronizer();
            first = true;
        }
        private bool CalculateIDs(WorldModel Model, List<int> attendIds, ref int[] ids, ref int passerId, ref int defenderId)
        {
            var tmpIds = attendIds.Where(w => (!Model.GoalieID.HasValue || w != Model.GoalieID.Value)).ToList();
            int[] allIds = new int[3];
            double maxDist = double.MinValue;
            int maxIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X > maxDist)
                {
                    maxDist = Model.OurRobots[item].Location.X;
                    maxIdx = item;
                }
            }
            if (maxIdx == -1)
                return false;
            defenderId = maxIdx;
            tmpIds.Remove(maxIdx);
            maxDist = double.MinValue;
            maxIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) > maxDist)
                {
                    maxDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                    maxIdx = item;
                }
            }
            if (maxIdx == -1)
                return false;
            PositionersID[0] = maxIdx;
            tmpIds.Remove(maxIdx);

            for (int i = 0; i < 3; i++)
            {
                double minDist = double.MaxValue;
                int minIdx = -1;
                foreach (var item in tmpIds.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return false;
                allIds[i] = minIdx;
                tmpIds.Remove(allIds[i]);
            }
            passerId = allIds[0];
            PositionersID[1] = allIds[1];
            PositionersID[2] = allIds[2];
            return true;
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
            StrategyName = "KarimBagheri";
            AttendanceSize = 6;
            About = "This strategy will screw them by a shoot from the hell just like KARIM!";
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
                if (!CalculateIDs(Model, Attendance.Keys.ToList(), ref PositionersID, ref PasserID, ref ShooterID))
                    return;
                first = false;
                firstBallPos = Model.BallState.Location;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #region States
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < initDist
                    && Model.OurRobots[PositionersID[0]].Location.DistanceFrom(PositionersPos[0]) < 0.23
                    && Model.OurRobots[PositionersID[1]].Location.DistanceFrom(PositionersPos[1]) < 0.23
                    && Model.OurRobots[PositionersID[2]].Location.DistanceFrom(PositionersPos[2]) < 0.23
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(DefenderPos) < tresh
                    && (!Model.GoalieID.HasValue || !Model.OurRobots.ContainsKey(Model.GoalieID.Value) || (Model.OurRobots[Model.GoalieID.Value].Location.DistanceFrom(GoaliePos) < tresh)))
                    initialPosCounter++;

                if (initialPosCounter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.Go;
                    firstInState = true;
                    timeLimitCounter = 0;
                    initialPosCounter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Go)
            {
                timeLimitCounter++;
                if (Mode == 0 && (Model.OurRobots[ShooterID].Location.X < 0.5 || passed))
                {
                    goBack = true;
                }

                if (passed)
                    finishCounter++;

                if (sync.Finished || sync.Failed || finishCounter > finishTresh || timeLimitCounter > maxWaitTresh)
                    CurrentState = (int)State.Finish;
            }  
            #endregion
            #region DefendersInfo
            CalculateDefenderInfo(engine, Model, out DefenderPos, out GoaliePos, out DefenderAng, out GoalieAng);
            #endregion
            #region PosesAndAngles
            double sgn = Math.Sign(Model.BallState.Location.Y);
            if (CurrentState == (int)State.First)
            {
                ShootTarget = GameParameters.OppGoalCenter;
                if (firstInState)
                {
                    PositionersPos[0] = new Position2D(-2.23, -sgn * 1.32);
                    //PositionersPos[1] = new Position2D(-1.68, -sgn * 0.35);
                    //PositionersPos[2] = new Position2D(-1.68, -sgn * 0.65);
                    double margin = 0.5;
                    Position2D p0 = new Position2D(-1.68, -sgn * 0.65);
                    double d = GameParameters.SafeRadi(new SingleObjectState(-p0, Vector2D.Zero, 0), margin);
                    PositionersPos[2] = GameParameters.OppGoalCenter + (p0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(d);

                    p0 = new Position2D(-1.68, -sgn * 0.35);
                    d = GameParameters.SafeRadi(new SingleObjectState(-p0, Vector2D.Zero, 0), margin);
                    PositionersPos[1] = GameParameters.OppGoalCenter + (p0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(d);

                    PositionersAng[0] = (ShootTarget - PositionersPos[0]).AngleInDegrees;
                    PositionersAng[1] = (ShootTarget - PositionersPos[1]).AngleInDegrees;
                    PositionersAng[2] = (ShootTarget - PositionersPos[2]).AngleInDegrees;
                    
                    double width = 0.8, heigth = 0.8, step = 0.2;
                    Position2D topLeft = new Position2D(0.4, (Model.BallState.Location.Y > 0) ? 0.8 : -(0.8 + heigth));
                    Position2D bestPoint = Position2D.Zero;
                    Line ballGoalLine = new Line(Model.BallState.Location, ShootTarget);
                    Vector2D ballGoalVec = Model.BallState.Location - ShootTarget;
                    Line goalPassTagetLine = new Line(ShootTarget, ShootTarget + Vector2D.FromAngleSize(ballGoalVec.AngleInRadians - sgn* (30.0).ToRadian(), 1));
                    Line ballPassTagetLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(ballGoalVec.AngleInRadians + sgn * (100.0).ToRadian(), 1));
                    if (!ballPassTagetLine.IntersectWithLine(goalPassTagetLine, ref PassTarget))
                    {
                        PassTarget = new Position2D(Model.BallState.Location.X + 0.7, Model.BallState.Location.Y - sgn * 0.7);
                        if (PassTarget.X < -0.6)
                            PassTarget.Y = -0.6;
                        //isChip = chipOrigin;
                        //engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref isChip, ref bestPoint);
                        //chipOrigin = isChip;
                        //PassTarget = bestPoint;
                    }
                    Vector2D BallTarget = PassTarget - Model.BallState.Location;
                    Vector2D InitBall = (GameParameters.OppGoalCenter - Model.BallState.Location);
                    double Teta = Vector2D.AngleBetweenInDegrees(InitBall, BallTarget);
                    double sgn2 = Math.Sign(Teta);
                    Teta = Math.Round(Math.Abs(Teta));
                    Teta = Teta - (Teta % 10);
                    if (Teta > 100 )
                        Teta = 100;
                    
                    Teta *= sgn2;
                    Teta = Teta.ToRadian();
                    ShootTarget = Model.BallState.Location + Vector2D.FromAngleSize(BallTarget.AngleInRadians + Teta, Model.BallState.Location.DistanceFrom(PassTarget));

                    firstInState = false;
                }
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAng = (ShootTarget - PasserPos).AngleInDegrees;
            }
            else if (CurrentState == (int)State.Go)
            {

                if (firstInState)
                {
                    ShootTarget = new Position2D(-3.0,sgn * 0.25);//GameParameters.OppGoalCenter;
                    double margin = 0.25;
                    
                    Circle c = new Circle(Model.BallState.Location, 0.9);
                    double minDist = double.MaxValue; int oppid = -1;
                    foreach (var item in Model.Opponents.Keys)
                    {
                        if (c.IsInCircle(Model.Opponents[item].Location) && Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                        {
                            oppid = item;
                            minDist = Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location);
                        }
                    }
                    if (oppid == -1)
                    {
                        tmpPos0 = new Position2D(Model.BallState.Location.X - .3, Model.BallState.Location.Y + (-1 * Math.Sign(Model.BallState.Location.Y) * .4));
                    }
                    else
                    {
                        tmpPos0 = Model.Opponents[oppid].Location + new Vector2D(0.2, 0);
                        tmpAng0 = (Model.Opponents[oppid].Location - tmpPos0).AngleInDegrees;
                    }

                    Position2D p0 = new Position2D(-1.68, -sgn * 0.6);
                    double d = GameParameters.SafeRadi(new SingleObjectState(-p0, Vector2D.Zero, 0), margin);
                    PositionersPos[2] = GameParameters.OppGoalCenter + (p0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(d);

                    p0 = new Position2D(-1.68, -0.3 * sgn);
                    //Position2D p1 = p0 + new Vector2D(0, Math.Sign(Model.BallState.Location.Y) * 0.3);
                    d = GameParameters.SafeRadi(new SingleObjectState(-p0, Vector2D.Zero, 0), margin);
                    PositionersPos[1] = GameParameters.OppGoalCenter + (p0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(d);

                    PositionersAng[0] = (ShootTarget - PositionersPos[0]).AngleInDegrees; // (ShootTarget - PositionersPos[0]).AngleInDegrees;
                    PositionersAng[1] = (ShootTarget - PositionersPos[1]).AngleInDegrees; //(ShootTarget - PositionersPos[1]).AngleInDegrees;
                    PositionersAng[2] = (ShootTarget - PositionersPos[2]).AngleInDegrees;

                    firstInState = false;
                }
                if (/*sync.SyncStarted && */!passed && Mode == 0)
                {
                    PositionersPos[0] = tmpPos0;
                    PositionersAng[0] = tmpAng0;
                }
                if (inPassState && Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.1)
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
                    PassSpeed =  engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, isChip, true);
                    if (isChip)
                        PassSpeed =  1.25;
                }
                else
                {
                    double r = GameParameters.SafeRadi(new SingleObjectState(-PositionersPos[2], Vector2D.Zero, 0), 0.0);

                    PositionersPos[2] = GameParameters.OppGoalCenter + (PositionersPos[2] - GameParameters.OppGoalCenter).GetNormalizeToCopy(r);
                    PositionersAng[2] = (ShootTarget - PositionersPos[2]).AngleInDegrees;

                    r = GameParameters.SafeRadi(new SingleObjectState(-PositionersPos[1], Vector2D.Zero, 0), 0.0);
                    PositionersPos[1] = GameParameters.OppGoalCenter + (PositionersPos[1] - GameParameters.OppGoalCenter).GetNormalizeToCopy(r);
                    PositionersAng[1] = (ShootTarget - PositionersPos[1]).AngleInDegrees;
                }
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

                double goodness;
                //var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, ShootTarget, PassTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                //if (GoodPointInGoal.HasValue)
                //    ShootTarget = GoodPointInGoal.Value;
            }
            #endregion
        }
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 2.5, 2);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                    Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(engine, Model, PasserID, ShootTarget
                        , false, initDist);

                for (int i = 0; i < PositionersPos.Length; i++)
                {
                    Planner.Add(PositionersID[i], PositionersPos[i], PositionersAng[i], PathType.UnSafe, true, true, true, !passed);
                }

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(DefenderCornerRole1)))
                    Functions[ShooterID] = (eng, wmd) => GetRole<DefenderCornerRole1>(ShooterID).Run(eng, wmd, ShooterID, DefenderPos, DefenderAng);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng, new DefenceInfo(), DefenderPos, DefenderID, true);
            }
            else if (CurrentState == (int)State.Go)
            {
                RotateDelay = 20;
                if (Model.OurRobots[PositionersID[0]].Location.DistanceFrom(PositionersPos[0]) < 0.5)
                    goBack = true;
                if (goBack)
                {

                    if (isChip)
                        sync.SyncChipPass(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, backSensor, rotateCounter);
                    else
                        sync.SyncDirectPass(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, rotateCounter);
                    if (sync.InPassState)
                        inPassState = true;
                    if (sync.InRotate)
                    {
                        moveCounter++;
                        if (moveCounter > 0)
                            passerMoved = true;
                    }
                }
                //else
                //{
                //    if (!recalcRot)
                //        Planner.SetReCalculateTeta(PasserID, true);
                //    recalcRot = true;
                //    if (Planner.AddRotate(Model, PasserID, PassTarget, PasserPos, kickPowerType.Speed, PassSpeed, isChip, rotateCounter, true).IsInRotateDelay)
                //    {
                //        rotateCounter++;
                //    }
                //}
                for (int i = 0; i < PositionersPos.Length; i++)
                {
                    //if (passed && i == 0)
                    //    continue;
                    Planner.Add(PositionersID[i], PositionersPos[i], PositionersAng[i], PathType.UnSafe, false, i == 0, false, !passed);
                }

                //if (goBack)
                //{
                if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.3)
                {
                    goDefence = true;
                }
                if (goDefence)
                {
                    if (Model.OurRobots[PasserID].Location.X < 0)
                        Planner.Add(PasserID, Model.OurRobots[PasserID].Location + new Vector2D(1, 0), 180, PathType.UnSafe, true, true, true, !passed);
                    else
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(DefenderCornerRole1)))
                            Functions[PasserID] = (eng, wmd) => GetRole<DefenderCornerRole1>(PasserID).Run(engine, Model, PasserID, DefenderPos, DefenderAng);
                    }
                }
                //}
                //if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.25)
                //{
                //    Position2D p = new Position2D(-1, Math.Sign(PasserPos.Y) * 1.3);
                //    Planner.Add(PasserID, p, (ShootTarget - p).AngleInDegrees, PathType.UnSafe, false, true, true, false);
                //}
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng, new DefenceInfo(), DefenderPos, DefenderID, true);
                //}
                //else if (Mode == 1)
                //{
                //    RotateDelay = 20;
                //    if (Model.OurRobots[ShooterID].Location.DistanceFrom(PositionersPos[2]) < 0.23)
                //        goBack = true;
                //    if (goBack)
                //    {
                //        if (isChip)
                //            sync.SyncChipPass(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, rotateCounter);
                //        else
                //            sync.SyncDirectPass(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, rotateCounter);
                //        if (sync.InPassState)
                //            inPassState = true;
                //        if (sync.InRotate)
                //        {
                //            moveCounter++;
                //            if (moveCounter > 0)
                //                passerMoved = true;
                //        }
                //    }
                //    else
                //    {
                //        if (!recalcRot)
                //            Planner.SetReCalculateTeta(PasserID, true);
                //        recalcRot = true;
                //        if (Planner.AddRotate(Model, PasserID, PassTarget, PasserPos, kickPowerType.Speed, PassSpeed, isChip, rotateCounter, true).IsInRotateDelay)
                //        {
                //            rotateCounter++;
                //        }
                //    }
                //    //else
                //    //{
                //    for (int i = 0; i < PositionersPos.Length; i++)
                //    {
                //        if (goBack && i == 2)
                //            continue;
                //        Planner.Add(PositionersID[i], PositionersPos[i], PositionersAng[i], PathType.UnSafe, false, false, false, !passed);
                //    }
                //    //}
                //    //if (Model.OurRobots[ShooterID].Location.DistanceFrom(PositionersPos[2]) > 0.2)
                //    //    passerMoved = tr;
                //    //else 


                //    if (passed)
                //    {
                //        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                //            Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(engine, Model, DefenderID, DefenderPos, DefenderAng);
                //    }
                //    else if (passerMoved)
                //    {
                //        Planner.Add(DefenderID, new Position2D(0, -Math.Sign(Model.BallState.Location.Y) * 0.5), 180, PathType.UnSafe, false, false, true, !passed);
                //    }
                //    else
                //    {
                //        Planner.Add(DefenderID, ShooterPos, ShooterAng, PathType.UnSafe, true, true, true, true);
                //    }
                //    if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.25)
                //    {
                //        Position2D p = new Position2D(-1, Math.Sign(PasserPos.Y) * 1.3);
                //        Planner.Add(PasserID, p, (ShootTarget - p).AngleInDegrees, PathType.UnSafe, false, true, true, false);
                //    }
                //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                //        Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, Model, Model.GoalieID.Value, GoaliePos, GoalieAng);
                //}
            }
            return CurrentlyAssignedRoles;
        }
        enum State
        {
            First,
            Go,
            Finish
        }
    }
}
