using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;

namespace MRL.SSL.AIConsole.Strategies
{
    class DrawerKickOffStrategy:StrategyBase
    {

        const double tresh = 0.2, stuckTresh = 0.23, angleTresh = 2, waitTresh = 10, finishTresh = 120, initDist = 0.22, maxWaitTresh = 180, passSpeedTresh = 0.08, behindBallTresh = 0.07, fieldMargin = 0.12;

        bool first, firstInState, isChip, chipOrigin, shooterFirstMove, goActive, passed, changeShooter, inrot;
        int[] PositionersID;
        int PasserID, ShooterID, initialPosCounter, finishCounter, timeLimitCounter, RotateDelay, opp2PeakID, rotateCounter, drawerIdx;
        Position2D PasserPos, ShooterPos, ShootTarget, PassTarget;
        Position2D[] PositionersPos;
        double PasserAng, ShooterAng, PassSpeed, KickSpeed, firstPassSpeed, secondPassSpeed, sgn ;
        double[] PositionersAng;
        Syncronizer sync;
        Position2D firstBallPos;
        bool backSensor;
        private bool CalculateIDs(WorldModel Model, List<int> attendIds, ref int[] ids, ref int passerId, ref int shooterId)
        {
            var tmpIds = attendIds.ToList();
            int[] allIds = new int[4];
            for (int i = 0; i < 4; i++)
            {
                double minDist = double.MaxValue;
                int minIdx = -1;
                foreach (var item in tmpIds.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.Y - Model.BallState.Location.Y < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.Y - Model.BallState.Location.Y;
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return false;
                allIds[i] = minIdx;
                tmpIds.Remove(allIds[i]);
            }
            
            ids[1] = allIds[0];
            passerId = allIds[1];
            shooterId = (Model.OurRobots[allIds[2]].Location.X > Model.OurRobots[allIds[3]].Location.X) ? allIds[2] : allIds[3];
            ids[0] = (Model.OurRobots[allIds[2]].Location.X <= Model.OurRobots[allIds[3]].Location.X) ? allIds[2] : allIds[3];
            
            return true;
        }


        public override void ResetState()
        {
            backSensor = true;
            firstBallPos = Position2D.Zero;
            int posCount = 2;// Math.Max(AttendanceSize - 2, 0);
            sgn = 1;
            PositionersID = new int[posCount];
            PositionersAng = new double[posCount];
            PositionersPos = new Position2D[posCount];
            PasserID = -1;
            ShooterID = -1;
            PasserAng = 0;
            ShooterAng = 0;

            PassSpeed = 0;
            initialPosCounter = 0;
            finishCounter = 0;
            timeLimitCounter = 0;
            RotateDelay = 5;
            ShootTarget = Position2D.Zero;
            PassTarget = Position2D.Zero;
            chipOrigin = true;
            isChip = true;
            inrot = false;
            firstInState = true;
            passed = false;
            goActive = false;
            changeShooter = false;
            rotateCounter = 2;
            drawerIdx = 1;
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
            StrategyName = "DrawerKickOff";
            AttendanceSize = 4;
            UseInMiddle = true;
            About = "this strategy tries to make some space to rush in to the opp field from sides in kick off";
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
            #region first
            if (first)
            {
                CalculateIDs(Model, Attendance.Keys.ToList(), ref PositionersID, ref PasserID, ref ShooterID);
                Random rand = new Random();
                int r = rand.Next(-1, 1);
                sgn = (r == -1) ? r : 1;
                drawerIdx = (sgn == -1) ? 1 : 0;
                first = false;
                firstBallPos = Model.BallState.Location;
            }
            #endregion

            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #region States
            if (CurrentState==(int)State.InitialState)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[PositionersID[0]].Location.DistanceFrom(PositionersPos[0]) < tresh
                    && Model.OurRobots[PositionersID[1]].Location.DistanceFrom(PositionersPos[1]) < tresh
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < tresh)
                    initialPosCounter++;

                if (((initialPosCounter > waitTresh && rotateCounter>=RotateDelay) || timeLimitCounter > maxWaitTresh)&&engine.Status== GameStatus.KickOff_OurTeam_Go)
                {
                    double goodness;
                    ShootTarget = GameParameters.OppGoalCenter;
                    var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, PassTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    if (GoodPointInGoal.HasValue)
                        ShootTarget = GoodPointInGoal.Value;
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    if (!obs.Meet(Model.BallState, new SingleObjectState(ShootTarget, Vector2D.Zero, 0), 0.05))
                        CurrentState = (int)State.Straight;
                    else
                        CurrentState = (int)State.Rush;
                    firstInState = true;
                    timeLimitCounter = 0;
                    initialPosCounter = 0;
                }
            }
            else if (CurrentState == (int)State.Rush)
            {
                if (passed)
                    finishCounter++;
                if (sync.Finished || sync.Failed || finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Straight)
            {
                finishCounter++;
                if (passed || sync.Finished || sync.Failed || finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;
            }
            #endregion
            #region PosesAndAngles
            if (CurrentState==(int)State.InitialState)
            {
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + new Vector2D(initDist, 0);
                PasserAng = (Model.BallState.Location - PasserPos).AngleInDegrees;

                ShooterPos = new Position2D(2,  sgn * 0.2);
                ShooterAng = (ShootTarget - ShooterPos).AngleInDegrees;

                PositionersPos[0] = new Position2D(0.15, 1.8);

                PositionersAng[0] = (ShootTarget - PositionersPos[0]).AngleInDegrees;

                PositionersPos[1] = new Position2D(0.15, -1.8);
                PositionersAng[1] = (ShootTarget - PositionersPos[1]).AngleInDegrees;
                PassTarget = new Position2D(-3.2, sgn * 2);
                PassSpeed = 0.95;//engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, isChip, false)*5;
            }
            else if (CurrentState == (int)State.Rush)
            {
                if (shooterFirstMove)
                {
                    PositionersPos[drawerIdx] = new Position2D(0.15, sgn * 0.3);
                    PositionersAng[drawerIdx] = (ShootTarget - PositionersPos[drawerIdx]).AngleInDegrees;
                }
                if (!shooterFirstMove)
                    ShooterPos.Y = sgn * 1.5;
                else
                    ShooterPos = PassTarget;
                if (!shooterFirstMove && Math.Abs(Model.OurRobots[ShooterID].Location.Y) > 1.2)
                    shooterFirstMove = true;
                if (!changeShooter && Model.OurRobots[ShooterID].Location.X < 0.4 && Model.OurRobots[ShooterID].Location.X > 0)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    if (!obs.Meet(Model.OurRobots[PositionersID[drawerIdx]], new SingleObjectState(Model.OurRobots[PositionersID[drawerIdx]].Location + new Vector2D(-0.5, 0), Vector2D.Zero, 0), 0.1)
                        &&obs.Meet(Model.OurRobots[ShooterID], new SingleObjectState(Model.OurRobots[ShooterID].Location + new Vector2D(-1, 0), Vector2D.Zero, 0), 0.1))
                    {
                        int tmp = ShooterID;
                        ShooterID = PositionersID[drawerIdx];
                        PositionersID[drawerIdx] = tmp;
                        changeShooter = true;
                    }
                }
                if (inrot && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (passed && Model.OurRobots[ShooterID].Location.X < 0.15)
                    goActive = true;

            }
            else if (CurrentState == (int)State.Straight)
            {
                if (Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
            }
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();

            Functions = new Dictionary<int, CommonDelegate>();
            if (CurrentState==(int)State.InitialState)
            {
                //if (Planner.AddRotate(Model, PasserID, PassTarget, 0, kickPowerType.Speed, PassSpeed, isChip, 0/*Math.Max(rotateCounter, RotateDelay)*/, backSensor).IsInRotateDelay)
                //{
                //    rotateCounter++;
                //    inrot = true;
                //}

                Planner.ChangeDefaulteParams(ShooterID, false);
                Planner.SetParameter(ShooterID, 3, 2.5);
                Planner.Add(ShooterID, ShooterPos, ShooterAng, PathType.UnSafe, true, true, true, true);
                for (int i = 0; i < PositionersPos.Length; i++)
                {
                    Planner.ChangeDefaulteParams(PositionersID[i], false);
                    Planner.SetParameter(PositionersID[i], 3, 2.5);
                    Planner.Add(PositionersID[i], PositionersPos[i], PositionersAng[i], PathType.UnSafe, true, true, true, true);
                }
            }
            else if (CurrentState== (int)State.Rush)
            {
                if (Planner.AddRotate(Model, PasserID, PassTarget, 0, kickPowerType.Speed, PassSpeed, isChip, 0/*Math.Max(rotateCounter, RotateDelay)*/, backSensor).IsInRotateDelay)
                {
                    rotateCounter++;
                    inrot = true;
                }
                if (!goActive)
                {
                    if (!shooterFirstMove)
                        rotateCounter++;
                    if (!shooterFirstMove || !inrot)
                    {

                        Planner.Add(PasserID, Model.BallState.Location + (Model.BallState.Location - PassTarget).GetNormalizeToCopy(0.13), (PassTarget - Model.BallState.Location).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                            inrot = true;
                    }
                    else
                    {
                        

                        SingleWirelessCommand swc=new SingleWirelessCommand(new Vector2D(0, 0.1), 0, true, 0, 0, true, false);
                        swc.RobotID = PasserID;
                        swc.isChipKick = true;
                        swc.KickSpeed = 1.2;
                        //swc.KickSpeed = 255;//PassSpeed;
                        Planner.Add(PasserID, swc);
                    }
                    //Planner.ChangeDefaulteParams(ShooterID, false);
                    //Planner.SetParameter(ShooterID, 6, 5);
                    Planner.Add(ShooterID, ShooterPos, ShooterAng, PathType.UnSafe, false, false, false, false);
                   
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                        Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).Perform(engine, Model, ShooterID, PasserID);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(SupporterRole)))
                        Functions[PasserID] = (eng, wmd) => GetRole<SupporterRole>(PasserID).Perform(engine, Model, PasserID);
                }
                for (int i = 0; i < PositionersPos.Length; i++)
                {
                    if (i == drawerIdx && changeShooter)
                    {
                        //Planner.ChangeDefaulteParams(PositionersID[drawerIdx], false);
                        //Planner.SetParameter(PositionersID[drawerIdx], 7, 6);
                    }
                    Planner.Add(PositionersID[i], PositionersPos[i], PositionersAng[i], PathType.UnSafe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.Straight)
            {
                Planner.AddRotate(Model, PasserID, ShootTarget, 0, kickPowerType.Speed, KickSpeed, false, rotateCounter, backSensor);
                Planner.Add(ShooterID, ShooterPos, ShooterAng, PathType.UnSafe, true, true, true, true);
                for (int i = 0; i < PositionersPos.Length; i++)
                {
                    Planner.Add(PositionersID[i], PositionersPos[i], PositionersAng[i], PathType.UnSafe, true, true, true, true);
                }
            }
            return CurrentlyAssignedRoles;
        }
        enum State
        {
            InitialState,
            Rush,
            Straight,
            Finish
        }
    }
}
