using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Strategies
{
    public class CatchAndRotateNewStrategy : StrategyBase
    {
        const double tresh = 0.05, stuckTresh = 0.23, angleTresh = 2, waitTresh = 20, finishTresh = 130, initDist = 0.22, maxWaitTresh = 180, passSpeedTresh = StaticVariables.PassSpeedTresh, 
            behindBallTresh = 0.07, fieldMargin = 0.12, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.65, faildBallDistSecondPass = 0.5, 
            faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;

        bool first, firstInState, isChip, chipOrigin, shooterFirstMove, goActive, passed, inRotate, support, active, Debug = true, getLastPasserPos, goFake, nearShooter, shooted;
        int[] PositionersID;
        int PasserID, ShooterIdx, SupporterIdx, initialPosCounter, finishCounter, timeLimitCounter, RotateDelay, rotateCounter, mode, faildCounter, inCatchCounter;
        Position2D PasserPos, ShootTarget, PassTarget, SupporterPos, lastPasserPos, FakePos;
        Position2D[] PositionersPos;
        double PasserAng, PassSpeed, KickSpeed, SupporterAng, RotateTeta;
        double[] PositionersAng;
        Syncronizer sync;
        Vector2D refrence; bool backSensor;
        private bool CalculateIDs(WorldModel Model, List<int> attendIds, ref int[] ids, ref int passerId)
        {
            var tmpIds = attendIds.ToList();
            int[] allIds = new int[AttendanceSize - 1];
            double minDist = double.MaxValue;
            int minIdx = -1;
            foreach (var item in tmpIds.ToList())
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return false;
            passerId = minIdx;
            tmpIds.Remove(passerId);
            for (int i = 0; i < AttendanceSize - 1; i++)
            {
                minDist = double.MaxValue;
                minIdx = -1;
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
            for (int i = 0; i < allIds.Length; i++)
                ids[i] = allIds[i];
            return true;
        }
        private Position2D CalculatePassTarget(GameStrategyEngine engine, WorldModel Model, int id0, int id1, out int id, out int supId, out Position2D shootTar, out int Mode)
        {
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
            Position2D passTarget = new Position2D();
            
            //Position2D topLeft = new Position2D(-1.2, (Model.BallState.Location.Y > 0) ? 0.3 : -(0.3 + height));
            //bool ch = true;
            //engine.GameInfo.BestPassPoint(Model, shootTar, topLeft, width, height, (int)(width / step), (int)(height / step), ref ch, ref passTarget);
            passTarget = new Position2D(-1.5, 0.7);
            shootTar =  GameParameters.OppGoalCenter.Extend(0, -Math.Sign(Model.BallState.Location.Y) * 0.2);
            Mode = 1;
            if (!obs.Meet(Model.OurRobots[id0], new SingleObjectState(passTarget, Vector2D.Zero, 0), 0.15))
            {
                id = id0;
                supId = id1;
                return passTarget;
            }
            Circle c = new Circle(Model.OurRobots[id1].Location, 0.6);
            double r =Math.Abs((GameParameters.OppGoalCenter - Model.OurRobots[id1].Location).AngleInDegrees);
            bool marked = false;
            foreach (var item in obs.ObstaclesList)
            {
                if (c.IsInCircle(item.Value.State.Location))
                {
                    Vector2D v =item.Value.State.Location - Model.OurRobots[id1].Location;
                    if (Math.Abs(v.AngleInDegrees) < r)
                    {
                        marked = true;
                        break;
                    }
                }
            }
            if (!marked)
            {
                Mode = 1;
                passTarget = Model.OurRobots[id1].Location;
                id = id1;
                supId = id0;
                return passTarget;
            }
            passTarget = Model.OurRobots[id0].Location + new Vector2D(-2.5, 0);
            if (!obs.Meet(Model.OurRobots[id0], new SingleObjectState(passTarget, Vector2D.Zero, 0), 0.3))
            {
                Mode = 2;
                id = id0;
                supId = id1;
                return passTarget;
            }
            Mode = 3;
            shootTar = GameParameters.OppGoalCenter.Extend(0, Math.Sign(Model.BallState.Location.Y) * 0.2);
            double height = 1.0, width = 0.2, step = 0.1;

            Position2D topLeft = new Position2D(-1.6, (Model.BallState.Location.Y > 0) ? 0.3 : -(0.3 + height));
            bool ch = true;
         //   engine.GameInfo.BestPassPoint(Model, shootTar, topLeft, width, height, (int)(width / step), (int)(height / step), ref ch, ref passTarget);
            passTarget = new Position2D(-1.8, 0);
            id = id1;
            supId = id0;
            return passTarget;

        }

        public override void ResetState()
        {
            UseInMiddle = true;
            backSensor = false;
            int posCount = 2;
            inCatchCounter = 0;
            PositionersID = new int[posCount];
            PositionersAng = new double[posCount];
            PositionersPos = new Position2D[posCount];
            PasserID = -1;
            PasserAng = 0;
            SupporterAng = 0;
            SupporterIdx = -1;
            ShooterIdx = -1;
            PassSpeed = 0;
            KickSpeed = Program.MaxKickSpeed;
            initialPosCounter = 0;
            finishCounter = 0;
            timeLimitCounter = 0;
            RotateDelay = 30;
            ShootTarget = Position2D.Zero;
            PassTarget = Position2D.Zero;
            SupporterPos = Position2D.Zero;
            PasserPos = Position2D.Zero;
            lastPasserPos = Position2D.Zero;
            FakePos = Position2D.Zero;
            refrence = Vector2D.Zero;
            faildCounter = 0;
            getLastPasserPos = false;
            chipOrigin = false;
            isChip = false;
            passed = false;
            support = false;
            goFake = false;
            firstInState = true;
            shooterFirstMove = true;
            goActive = false;
            inRotate = false;
            back = false;
            nearShooter = false;
            shooted = false;
            mode = 0;
            rotateCounter = 0;
            RotateTeta = 30;
            firstBallPos = Position2D.Zero;
            if (sync != null)
                sync.Reset();
            else
                sync = new Syncronizer();
            first = true;
        }

        public override void InitializeStates(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> attendance)
        {
            Attendance = attendance;
            CurrentState = (int)State.InitialState;
            InitialState = 0;
            FinalState = 2;
            TrapState = 2;
        }

        public override void FillInformation()
        {
            UseInMiddle = true;
            StrategyName = "CatchAndRotateNew";
            AttendanceSize = 3;
            UseInMiddle = true;
            About = "this strategy will try to catch ball in front of danger zone then rotate and f**k opp";
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
        bool back;
        Position2D firstBallPos;
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {
            #region First
            if (first)
            {
                if (!CalculateIDs(Model, Attendance.Keys.ToList(), ref PositionersID, ref PasserID))
                    return;

                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion

            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #region States
            if (CurrentState == (int)State.InitialState)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[PositionersID[0]].Location.DistanceFrom(PositionersPos[0]) < tresh
                    && Model.OurRobots[PositionersID[1]].Location.DistanceFrom(PositionersPos[1]) < tresh)
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
                if (passed)
                    finishCounter++;
                if (sync.Finished || sync.Failed || finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;

                refrence = Model.OurRobots[PositionersID[ShooterIdx]].Location- PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if (CatchState == 0 && Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionersID[ShooterIdx]].Location) < 0.3)
                    {
                        inCatchCounter++;
                    }
                    bool b = false;
                    if (inCatchCounter > 30)
                    {
                        if (CatchState == 0 && v.Y < faildFarPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionersID[ShooterIdx]].Location) <= faildBallDistSecondPass)
                            b = true;
                    }
                    if ((v.Y < faildFarPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionersID[ShooterIdx]].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionersID[ShooterIdx]].Location) <= faildBallDistSecondPass) || b)
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

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionersID[ShooterIdx]].Location) < 0.3)
                    nearShooter = true;
                if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[PositionersID[ShooterIdx]].Location - Model.OurRobots[PasserID].Location) <= 0)
                    shooted = true;
                if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionersID[ShooterIdx]].Location) > 0.35)
                    CurrentState = (int)State.Finish;
            }

            #endregion
            #region PosesAndAngles
            double sgn = Math.Sign(Model.BallState.Location.Y);
            if (CurrentState == (int)State.InitialState)
            {
                ShootTarget = GameParameters.OppGoalCenter;

                if (Model.BallState.Location.X < 1)
                {
                    back = false;
                }
                else
                    back = true;

                PasserPos = (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist) + Model.BallState.Location;
                PasserAng = (ShootTarget - PasserPos).AngleInDegrees;
                PositionersPos[1] = new Position2D(1, -sgn * 1.7);
                PositionersPos[0] = new Position2D(-1.9, sgn * 1.5);
                PositionersAng[1] = (Model.BallState.Location - PositionersPos[1]).AngleInDegrees;
                PositionersAng[0] = (Model.BallState.Location - PositionersPos[0]).AngleInDegrees;

            }
            else if (CurrentState == (int)State.Go)
            {
                if (firstInState)
                {

                    PassTarget = CalculatePassTarget(engine, Model, PositionersID[1], PositionersID[0], out ShooterIdx, out SupporterIdx, out ShootTarget, out mode);
                    ShooterIdx = (PositionersID[0] == ShooterIdx) ? 0 : 1;
                    SupporterIdx = (PositionersID[0] == SupporterIdx) ? 0 : 1;
                    //PositionersPos[ShooterIdx] = PassTarget;
                    PositionersAng[ShooterIdx] = (PassTarget - Model.BallState.Location).AngleInDegrees;
                    
                    if (mode == 0 || mode == 2)
                    {
                        FakePos = new Position2D(-1.9, Math.Sign(Model.BallState.Location.Y) * 0.5);
                        FakePos = GameParameters.OppGoalCenter + (FakePos - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-FakePos, Vector2D.Zero, 0), 0.25));
                    }
                    else if(mode == 1 || mode == 3)
                    {
                        FakePos = new Position2D(-1.9, -Math.Sign(Model.BallState.Location.Y) * 1.6);
                    }

                    firstInState = false;
                }
                ShootTarget = GameParameters.OppGoalCenter.Extend(0, Math.Sign(Model.BallState.Location.Y) * 0.2);
                if (!back || mode == 0 || mode == 2 || mode == 3)
                    chipOrigin = true;
                if (inRotate && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed)
                {
                    if (!chipOrigin)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                        isChip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);
                    }
                    else
                        isChip = true;
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, isChip, false)*1;

                    if (isChip)
                    {
                        if (mode == 2 || mode == 1)
                            PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.65;
                        else if (mode == 0)
                            PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 1.5;
                        else if (back)
                            PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget);
                        else
                            PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.9;
                    }
                }
                if (!passed && isChip)
                {
                    double margin = 0.15;
                    Position2D tmpPassTarget = (PassTarget - Model.BallState.Location).GetNormalizeToCopy(PassSpeed) + Model.BallState.Location;
                    if (tmpPassTarget.X < margin  && Model.BallState.Location.X > 0)
                    { 
                        Position2D p = Position2D.Zero;
                        new Line(new Position2D(margin, Math.Abs(GameParameters.OurLeftCorner.Y)), new Position2D(margin, -Math.Abs(GameParameters.OurLeftCorner.Y))).IntersectWithLine(new Line(Model.BallState.Location, tmpPassTarget), ref p);
                        PassSpeed = Model.BallState.Location.DistanceFrom(p);
                    }
                    PassSpeed = Math.Max(PassSpeed, 0.8);
                   //if(PassSpeed <1)
                   //    PassSpeed *= GamePlannerInfo.ChipCoef[PasserID]; ;
                }
                if (passed && (Model.BallState.Location.X < 0.7))
                    support = true;
                
                //if (passed)
                //{
                //    PasserPos = Position2D.Zero;
                //    PasserAng = (Model.BallState.Location - PasserPos).AngleInDegrees;
                //}
                if (passed && ((Model.OurRobots[PositionersID[SupporterIdx]].Location.X) - Model.BallState.Location.X > 0.05))
                    active = true;

            }
            #endregion
        }
        int CatchState = 0;
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.InitialState)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 2.5, 2);
                Planner.Add(PasserID, PasserPos, PasserAng, PathType.UnSafe, true, true, true, true);
                for (int i = 0; i < PositionersPos.Length; i++)
                {
                    Planner.Add(PositionersID[i], PositionersPos[i], PositionersAng[i], PathType.UnSafe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, 0, PositionersID[ShooterIdx], PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, backSensor, 0);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, 0, PositionersID[ShooterIdx], PassTarget, ShootTarget, PassSpeed, KickSpeed, RotateDelay, 0);
                
                if (sync.SyncStarted && !getLastPasserPos && Model.OurRobots.ContainsKey(PasserID))
                {
                    lastPasserPos = Model.OurRobots[PasserID].Location;
                    getLastPasserPos = true;
                }

                if (sync.InRotate)
                    goFake = true;

                //if (support)
                //{
                //    //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionersID[SupporterIdx], typeof(SupporterRole)))
                //    //    Functions[PositionersID[SupporterIdx]] = (eng, wmd) => GetRole<SupporterRole>(PositionersID[SupporterIdx]).Perform(eng, wmd, PositionersID[SupporterIdx]);
                //}
                //else
                if (goFake)
                {
                    Planner.Add(PositionersID[SupporterIdx], FakePos, (FakePos - PasserPos).AngleInDegrees, PathType.UnSafe, true, true, true, !passed);
                }
                else
                    Planner.Add(PositionersID[SupporterIdx], PositionersPos[SupporterIdx], PositionersAng[SupporterIdx], PathType.UnSafe, true, true, true, !passed);
                
                inRotate = sync.InPassState;

                if (Model.OurRobots[PasserID].Location.DistanceFrom(Model.BallState.Location) > 0.35 && sync.InRotate)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(SupporterRole)))
                        Functions[PasserID] = (eng, wmd) => GetRole<SupporterRole>(PasserID).Perform(eng, wmd, PasserID);
                    //Planner.Add(PasserID, PasserPos, 180, PathType.UnSafe, false, false, true, false);
                }
            }
            CatchState = sync.CatchState;
            return CurrentlyAssignedRoles;
        }

        enum State
        {
            InitialState,
            Go,
            Finish
        }
    }
}
