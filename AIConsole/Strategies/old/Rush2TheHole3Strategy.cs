using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Roles;
namespace MRL.SSL.AIConsole.Strategies.old
{
    public class Rush2TheHole3Strategy:StrategyBase
    {
        const double tresh = 0.07, stuckTresh = 0.23, angleTresh = 2, waitTresh = 60, finishTresh = 110, initDist = 0.22, maxWaitTresh = 360, passSpeedTresh = 0.08, behindBallTresh = StaticVariables.PassSpeedTresh, fieldMargin = 0.12, faildFarPassSpeedTresh = 0.2, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        bool first, firstInState, passed, nearShooter, shooted, inPassSatate, chip, chipOrigin, ballMoved, check4Shooter, targetChanged, targetReChanged, goActive;
        int PasserID, ShooterID, PositionerID0, PositionerID1, PositionerID2;
        Position2D firstBallPos, PasserPos, PositionerPos0, PositionerPos1, PositionerPos2, passTarget, lastPos1, shootTarget, lastPos1Pos;
        Position2D? PassInter;
        int timeLimitCounter, initialPosCounter, finishCounter, faildCounter, rotateCounter;
        double PassSpeed;
        GetBallSkill getBallSkill;
        int Mode; bool backSensor;
        public override void ResetState()
        {
            backSensor = true;
            goActive = false;
            targetReChanged = false;
            targetChanged = false;
            check4Shooter = false;
            first = true;
            firstInState = true;
            passed = false;
            nearShooter = false;
            shooted = false;
            inPassSatate = false;
            chip = false; 
            chipOrigin = true;
            ballMoved = false;
            lastPos1Pos = Position2D.Zero;

            PasserID =-1;
            ShooterID = -1;
            PositionerID0 =-1;
            PositionerID1 =-1;
            PositionerID2=-1;
            
            timeLimitCounter = 0;
            initialPosCounter = 0;
            finishCounter = 0;
            faildCounter = 0;
            rotateCounter = 2;

            PassSpeed = 0;
            Mode = 0;

            getBallSkill = new GetBallSkill();
            PassInter = null;
            lastPos1 = Position2D.Zero;
            firstBallPos = Position2D.Zero;
            PasserPos = Position2D.Zero;
            PositionerPos0 = Position2D.Zero;
            PositionerPos1 = Position2D.Zero;
            PositionerPos2 = Position2D.Zero;
            shootTarget = Position2D.Zero;
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
            StrategyName = "Rush2TheHole3";
            AttendanceSize = 4;
            About = "this strategy will rushed 2 the Opp Hole!";
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

        double margin = 0.25;
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region First
            if (first)
            {
                double minDist = double.MaxValue;
                int minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
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
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PasserID && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID0 = minIdx;
                PositionerID1 = -1;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PositionerID0 && item != PasserID && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID1 = minIdx;
                PositionerID2 = -1;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PositionerID0 && item != PositionerID1 && item != PasserID && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID2 = minIdx;

                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #region States
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < tresh
                    && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < tresh)
                    initialPosCounter++;

                if (initialPosCounter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.Go;
                    firstInState = true;
                    timeLimitCounter = 0;
                    initialPosCounter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    faildCounter++;
                else
                    faildCounter = Math.Max(faildCounter - 2, 0);
                if (faildCounter > 4)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Go)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = passTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if ((v.Y < 0.1 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistSecondPass))
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
            #region PosAndAngles
            double sgn = Math.Sign(Model.BallState.Location.Y);
            if (CurrentState==(int)State.First)
            {
                if (firstInState)
                {
                    PasserPos = Model.BallState.Location + new Vector2D(0, Math.Sign(Model.BallState.Location.Y) * initDist);
                    if (Mode == 0)
                    {
                        PositionerPos0 = new Position2D(-1.95, 0.26 * -sgn);
                        PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                        PositionerPos1 = new Position2D(-1.95, 0.52 * -sgn);
                        PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                        PositionerPos2 = new Position2D(-2.05, 0.7 * -sgn);
                        PositionerPos2 = GameParameters.OppGoalCenter + (PositionerPos2 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos2, Vector2D.Zero, 0), margin));
                        passTarget = new Position2D(-2.75, -sgn * 0.5);
                    }
                    else if (Mode == 1)
                    {
                        PositionerPos0 = new Position2D(-1.85, 0.15 * sgn);
                        PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                        PositionerPos1 = new Position2D(-1.85, 0.15 * -sgn);
                        PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                        PositionerPos2 = new Position2D(-1.9, 0.4 * -sgn);
                        PositionerPos2 = GameParameters.OppGoalCenter + (PositionerPos2 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos2, Vector2D.Zero, 0), margin));

                        passTarget = new Position2D(-2.5, -sgn * 0.5);
                    }
                    shootTarget = GameParameters.OppGoalCenter;
                    firstInState = false;
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                if (firstInState)
                {
                    ShooterID = PositionerID1;
                    lastPos1 = PositionerPos1; 
                    shootTarget = GameParameters.OppGoalCenter.Extend(0, -sgn * 0.2);
                    if (Mode == 0)
                    {
                        PositionerPos1 = new Position2D(-2.96, -.75 * sgn);
                        PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                    }
                    else if (Mode == 1)
                    {
                        PositionerPos0 = new Position2D(-2.1, 1.17* -sgn);
                        PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                        PositionerPos1 = new Position2D(-2.8, .4 * -sgn);
                        PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                        PositionerPos2 = new Position2D(-2.6, 1.8 * -sgn);
                        PositionerPos2 = GameParameters.OppGoalCenter + (PositionerPos2 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos2, Vector2D.Zero, 0), margin));

                    }
                    firstInState = false;
                }
                if (inPassSatate && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                chip = chipOrigin;
                if (!passed && !chipOrigin)
                {
                    chip = chipOrigin;
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    chip = obs.Meet(Model.BallState, new SingleObjectState(passTarget, Vector2D.Zero, 0), 0.07);
                }
                if (!passed)
                {
                    Vector2D PassVec = Vector2D.FromAngleSize(Model.OurRobots[PasserID].Angle.Value * Math.PI / 180, 1);
                    Line BallLine = new Line(Model.BallState.Location, Model.BallState.Location + PassVec);
                    Line tmpL = new Line(new Position2D(0, -sgn * ((Mode == 0) ? 0.8 : 0.5)), new Position2D(1, -sgn * ((Mode == 0) ? 0.8 : 0.5)));
                    PassInter = tmpL.IntersectWithLine(BallLine);
                    PassSpeed = 4; // engine.GameInfo.CalculateKickSpeed(Model, passerID, Model.BallState.Location, passTarget, chip, true);
                    if (chip || passTarget.X < -1.4)
                        PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * .6;

                    //double goodness;
                    //var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, shootTarget, passTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    //if (GoodPointInGoal.HasValue)
                    //    shootTarget = GoodPointInGoal.Value;
                }
                if (Mode == 0)
                {
                    if (Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < 0.2 && !ballMoved)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? new List<int>() { engine.GameInfo.OppTeam.GoaliID.Value } : null);
                        double radi = 0.08;
                        if (obs.Meet(Model.OurRobots[PositionerID1], new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, 0), radi) &&!obs.Meet(Model.OurRobots[PositionerID2], new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, 0), radi))
                        {
                            ShooterID = PositionerID2;
                        }
                    }
                }
                else if (Mode ==1)
                {
                    if (Model.OurRobots[PositionerID1].Location.DistanceFrom(lastPos1) > 0.3)
                        check4Shooter = true;
                    if (check4Shooter && !targetChanged)
                    {
                        PositionerPos1 = Model.OurRobots[PositionerID1].Location + (Model.OurRobots[PositionerID1].Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(2);
                        targetChanged = true;
                        lastPos1Pos = Model.OurRobots[PositionerID1].Location;
                    }
                    if (targetChanged && !targetReChanged && Model.OurRobots[PositionerID1].Location.DistanceFrom(lastPos1Pos) > 0.3)
                    {
                        PositionerPos1 = new Position2D(-2.00, 0.2 * -sgn);
                        PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                        targetReChanged = true;
                    }
                    if (targetReChanged && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < 0.3 && !passed/*&& !ballMoved*/)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? new List<int>() { engine.GameInfo.OppTeam.GoaliID.Value } : null);
                        double radi = 0.08;
                        if (obs.Meet(Model.OurRobots[PositionerID1], new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, 0), radi) && !obs.Meet(Model.OurRobots[PositionerID0], new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, 0), radi))
                        {
                            ShooterID = PositionerID0;
                        }
                    }
                }
            }
            #endregion
        }
      
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.First)
            {
                if (Planner.AddRotate(Model, PasserID, passTarget, 0, kickPowerType.Speed, PassSpeed, chip, rotateCounter, backSensor).IsInRotateDelay)
                    rotateCounter++;
                Planner.Add(PositionerID0, PositionerPos0, (shootTarget - PositionerPos0).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                Planner.Add(PositionerID1, PositionerPos1, (shootTarget - PositionerPos1).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                Planner.Add(PositionerID2, PositionerPos2, (shootTarget - PositionerPos2).AngleInDegrees, PathType.UnSafe, false, true, true, true);
            }
            else if (CurrentState == (int)State.Go)
            {

               
                if (Mode == 0)
                {
                    bool b = ShooterID == PositionerID1 && Model.OurRobots[ShooterID].Location.DistanceFrom(lastPos1) < 0;

                    var tmpRot = Planner.AddRotate(Model, PasserID, passTarget, 0, kickPowerType.Speed, PassSpeed, chip, rotateCounter);
                    if (tmpRot.InKickState)
                        inPassSatate = true;
                    if (tmpRot.IsInRotateDelay && b)
                        rotateCounter++;

                    Planner.Add(PositionerID0, PositionerPos0, (shootTarget - PositionerPos0).AngleInDegrees, PathType.UnSafe, false, !passed, true, !passed);
                    if (passed)
                    {
                        double dist, DistFromBorder;

                        //if ((Model.BallState.Speed.Size < 0.25 || Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.35)&& GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                            ballMoved = true;
                        if ((Model.BallState.Speed.Size < 0.2 || Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.7) && GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                            goActive = true;
                        if (goActive)
                        {
                            getBallSkill.OutGoingBackTrack(engine, Model, ShooterID, shootTarget);
                            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                            //    Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).Perform(eng, wmd, ShooterID, null, null, false, true, false);
                            double kick = 0;
                            if (Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - Model.BallState.Location, Vector2D.FromAngleSize(Model.OurRobots[ShooterID].Angle.Value * Math.PI / 180, 1))) < 5)
                                kick = Program.MaxKickSpeed;
                            Planner.AddKick(ShooterID, kickPowerType.Speed, false, kick);
                        }
                        else if (ballMoved && PassInter.HasValue)
                        {
                            Planner.Add(ShooterID, PassInter.Value, (shootTarget - PassInter.Value).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                        else
                        {
                            Position2D p1 = (ShooterID == PositionerID1) ? PositionerPos1 : PositionerPos2;
                            Position2D p = GameParameters.OppGoalCenter + (p1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-p1, Vector2D.Zero, 0), -0.2));
                            Planner.Add(ShooterID, p, (shootTarget - p).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                        int otherid = (ShooterID == PositionerID1) ? PositionerID2 : PositionerID1;
                        Position2D otherPos = (ShooterID == PositionerID1) ? PositionerPos2 : PositionerPos1;
                        Planner.Add(otherid, otherPos, (shootTarget - otherPos).AngleInDegrees, PathType.UnSafe, false, !passed, true, !passed);
                    }
                    else
                    {
                        Planner.Add(PositionerID1, PositionerPos1, (shootTarget - PositionerPos1).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                        Planner.Add(PositionerID2, PositionerPos2, (shootTarget - PositionerPos2).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                    }
                }
                else if (Mode == 1)
                {
                    bool b = ShooterID == PositionerID1 && Model.OurRobots[ShooterID].Location.DistanceFrom(lastPos1) < 0.6;

                    var tmpRot = Planner.AddRotate(Model, PasserID, passTarget, 0, kickPowerType.Speed, PassSpeed, chip, rotateCounter, backSensor);
                    if (tmpRot.InKickState)
                        inPassSatate = true;
                    if (tmpRot.IsInRotateDelay && b)
                        rotateCounter++;

                    Planner.Add(PositionerID2, PositionerPos2, (shootTarget - PositionerPos2).AngleInDegrees, PathType.UnSafe, false, !passed, true, !passed); 
                    if (passed)
                    {
                        double dist, DistFromBorder;

                        //if ((Model.BallState.Speed.Size < 0.15 || (Model.BallState.Speed.Size < 0.25 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.45) && GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder)))
                            ballMoved = true;
                        if ((Model.BallState.Speed.Size < 0.15 ||(Model.BallState.Speed.Size < 0.15 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.65)) && GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.2, out dist, out DistFromBorder))
                            goActive = true;
                        if (goActive)
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                                Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).PerformForStrategy(eng, wmd, ShooterID, null, null, false, true, false);
                            double kick = 0;
                            if (Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - Model.BallState.Location, Vector2D.FromAngleSize(Model.OurRobots[ShooterID].Angle.Value * Math.PI / 180, 1))) < 10)
                                kick = Program.MaxKickSpeed;
                            Planner.AddKick(ShooterID, kickPowerType.Speed, false, kick);
                        }
                        else if (ballMoved && PassInter.HasValue)
                        {
                            Planner.Add(ShooterID, /*PassInter.Value*/new Position2D(-2.4, -Math.Sign(PasserPos.Y) * 0.5), (shootTarget - PassInter.Value).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            double kick = Program.MaxKickSpeed;
                            Planner.AddKick(ShooterID, kickPowerType.Speed, false, kick);
                        }
                        else
                        {
                            Position2D p1 = (ShooterID == PositionerID1) ? PositionerPos1 : PositionerPos0;
                            Position2D p = GameParameters.OppGoalCenter + (p1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-p1, Vector2D.Zero, 0), -0.2));
                            Planner.Add(ShooterID, p, (shootTarget - p).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            double kick = Program.MaxKickSpeed;
                            Planner.AddKick(ShooterID, kickPowerType.Speed, false, kick);
                        }
                        int otherid = (ShooterID == PositionerID1) ? PositionerID0 : PositionerID1;
                        Position2D otherPos = (ShooterID == PositionerID1) ? PositionerPos0 : PositionerPos1;
                        Planner.Add(otherid, otherPos, (shootTarget - otherPos).AngleInDegrees, PathType.UnSafe, false, !passed, true, !passed);
                    }
                    else
                    {
                        Planner.ChangeDefaulteParams(PositionerID0, false);
                        
                        Planner.SetParameter(PositionerID0, 3, 2.5);
                        if (!targetChanged)
                        {
                            Planner.ChangeDefaulteParams(PositionerID1, false);
                            Planner.SetParameter(PositionerID1, 3, 2.5);
                        }
                        Planner.Add(PositionerID1, PositionerPos1, (shootTarget - PositionerPos1).AngleInDegrees, PathType.UnSafe, false, targetChanged, true, true);
                        Planner.Add(PositionerID0, PositionerPos0, (shootTarget - PositionerPos0).AngleInDegrees, PathType.UnSafe, false, targetChanged, true, true);
                    }
                }
                if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.25)
                {
                    Position2D p = new Position2D(-1, Math.Sign(PasserPos.Y) * 0.3);
                    Planner.Add(PasserID, p, (shootTarget - p).AngleInDegrees, PathType.UnSafe, false, true, true, false);
                }
            }
            return CurrentlyAssignedRoles;
        }
        public enum State
        {
            First,
            Go,
            Finish
        }
    }
}
