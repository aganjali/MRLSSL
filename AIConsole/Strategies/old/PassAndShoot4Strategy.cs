
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Strategies
{
    public class PassAndShoot4Strategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2;
        const double ballMovedTresh = 0.07;
        const double tresh = 0.2, angleTresh = 2, waitTresh = 10, finishTresh = 100, initDist = 0.22, maxWaitTresh = 240, oppZoneMarg = 0.2;
        bool first, passTargetCalculated;
        int PasserId, ShooterID, PositionerID, goOtCounter;
        Position2D PasserPos, ShooterPos, PositionerPos, Positioner2Pos, PassTarget, ShootTarget, lastShooterPos, lastPasserPos, lastPositionerPos;
        double PasserAngle, ShooterAngle, PositionerAng, Positioner2Ang, RotateTeta, PassSpeed, KickPower;
        int counter, finishCounter, RotateDelay, timeLimitCounter;
        Syncronizer sync;
        bool chip, passed, chipOrigin, Debug = false, goActive, ballMoved;
        int Mode;
        bool shooterIsNear, getLastPasserPos, ballIndanger;
        GetBallSkill getBallSkill = new GetBallSkill();
        bool backSensor;
        Position2D topLeft = Position2D.Zero;
        double AngleT;
        Position2D firstBallPos;
        int PositionerID2;
        bool inrot = false, inPassState = false;
        Vector2D passVec = Vector2D.Zero;

        public override void ResetState()
        {
            backSensor = false;
            firstBallPos = Position2D.Zero;
            shooterIsNear = false;
            chip = false;
            chipOrigin = false;
            passed = false;
            Mode = 0;

            CurrentState = InitialState;
            first = true;
            goActive = false;
            passTargetCalculated = false;
            getLastPasserPos = false;
            ballMoved = false;
            RotateTeta = 30;
            PassSpeed = 4.5;
            KickPower = Program.MaxKickSpeed;
            timeLimitCounter = 0;
            PasserId = -1;
            ShooterID = -1;
            PositionerID2 = -1;
            inPassState = false;
            PasserPos = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PositionerPos = Position2D.Zero;
            Positioner2Pos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;
            lastShooterPos = Position2D.Zero;
            lastPositionerPos = Position2D.Zero;
            lastPasserPos = Position2D.Zero;
            PasserAngle = 0;
            ShooterAngle = 0;
            PositionerAng = 0;
            Positioner2Ang = 0;
            goOtCounter = 0;
            counter = 0;
            finishCounter = 0;
            RotateDelay = 10;
            AngleT = 60;
            ballIndanger = false;
            getBallSkill = new GetBallSkill();
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
            StrategyName = "SimplePassShoot4";
            AttendanceSize = 4;
            About = "this is Simple Pass Shoot strategy with 4 attendace!";
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

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {

            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                double minDist = double.MinValue;
                int minIdx = -1;

                foreach (var item in Attendance.Keys.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X > minDist)
                    {
                        minDist = Model.OurRobots[item].Location.X;
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID2 = minIdx;

                minDist = double.MaxValue;
                minIdx = -1;

                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PositionerID2 && Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PasserId = minIdx;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PositionerID2 && item != PasserId && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                ShooterID = minIdx;
                PositionerID = -1;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PositionerID2 && item != ShooterID && item != PasserId && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID = minIdx;
                first = false;
            }
            #endregion

            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #region States
            if (CurrentState == (int)State.First)
            {
                double dAngle = Model.OurRobots[PasserId].Angle.Value - PasserAngle;
                timeLimitCounter++;
                if (dAngle > 180)
                    dAngle -= 360;
                else if (dAngle < -180)
                    dAngle += 360;

                if (Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < tresh)
                    counter++;
                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    timeLimitCounter = 0;
                    CurrentState = (int)State.Go;
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                timeLimitCounter++;
                if (passed)
                    finishCounter++;
                if (sync.Finished || sync.Failed || finishCounter > finishTresh || timeLimitCounter > maxWaitTresh)
                    CurrentState = (int)State.Finish;
            }
            #endregion
            #region PosAndAngles
            if (CurrentState == (int)State.First)
            {
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + new Vector2D(0, Math.Sign(Model.BallState.Location.Y) * initDist);// (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees; 
                if (Mode == 2)
                {
                    PasserAngle = (Model.OurRobots[ShooterID].Location - PasserPos).AngleInDegrees;
                }
                else if (Mode == 1)
                {
                    PasserAngle = (Model.BallState.Location - PasserPos).AngleInDegrees;
                }
                if (Mode == 0 || Mode == 1 || Mode == 2)
                {
                    ShooterPos = new Position2D(-3.5, Math.Sign(Model.BallState.Location.Y) * 1.5);// PasserPos + new Vector2D(2, 0).GetNormalizeToCopy(passerShooterDist);
                }

                if (Mode == 0 || Mode == 1 || Mode == 2)
                {
                    ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - .3, Math.Abs(ShooterPos.X)));
                }
                ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - .3, Math.Abs(ShooterPos.Y)));

                ShooterPos.Y *= 1;

                ShooterAngle = 180;

                PositionerPos = new Position2D(Math.Max(Model.BallState.Location.X - 1, GameParameters.OppGoalCenter.X + 0.3), Math.Sign(-Model.BallState.Location.Y) * 1.5); ;
                PositionerAng = (ShootTarget - PositionerPos).AngleInDegrees;

                Positioner2Pos = new Position2D(-1, Math.Sign(Model.BallState.Location.Y) * 0); ;
                Positioner2Ang = (ShootTarget - Positioner2Pos).AngleInDegrees;

                inrot = false;
            }
            else if (CurrentState == (int)State.Go)
            {

                if (!passTargetCalculated)
                {
                    lastShooterPos = ShooterPos;

                    if (Mode == 0)
                    {
                        if (Model.BallState.Location.X < -2.2 || Mode == 0)
                            chipOrigin = true;
                        if (chipOrigin)
                            RotateTeta = 0;
                        ShootTarget = GameParameters.OppGoalCenter.Extend(0, Math.Sign(Model.BallState.Location.Y) * 0.2);

                        ShooterPos = PositionerPos + new Vector2D(0.25, Math.Sign(Model.BallState.Location.Y) * 0.3);
                        PositionerPos = new Position2D(-1.8, -Math.Sign(Model.BallState.Location.Y) * 0.3);
                        if (Model.BallState.Location.X > -2.4)
                        {
                            AngleT = 80;
                            PassTarget = new Position2D(Model.BallState.Location.X, Math.Sign(Model.BallState.Location.Y) * 0.1);
                        }
                        else
                        {
                            AngleT = 60;
                            PassTarget = new Position2D(-4.5, Math.Sign(Model.BallState.Location.Y) * 0.3);
                        }

                        if (!passed)
                        {
                            passVec = Vector2D.FromAngleSize(Model.OurRobots[PasserId].Angle.Value * Math.PI / 180, 1);
                        }
                    }
                    else if (Mode == 1)
                    {
                        ShootTarget = GameParameters.OppGoalCenter.Extend(0, Math.Sign(Model.BallState.Location.Y) * 0.35);

                        ShooterPos = PositionerPos + new Vector2D(0.25, Math.Sign(Model.BallState.Location.Y) * 0.3);
                        PositionerPos = new Position2D(-1.8, 0);

                        PassTarget = Model.BallState.Location + new Vector2D(1.8, Math.Sign(-Model.BallState.Location.Y) * 0.5);//Math.Max(.8, Model.BallState.Location.DistanceFrom(PassTarget) * .5);
                        if (!passed)
                        {
                            passVec = Vector2D.FromAngleSize(Model.OurRobots[PasserId].Angle.Value * Math.PI / 180, 1);
                        }
                    }
                    else if (Mode == 2)
                    {
                        chipOrigin = true;
                        if (chipOrigin)
                            RotateTeta = 0;
                        ShootTarget = GameParameters.OppGoalCenter.Extend(0, Math.Sign(Model.BallState.Location.Y) * 0.2);

                        ShooterPos = PositionerPos + new Vector2D(0.25, Math.Sign(Model.BallState.Location.Y) * 0.65);
                        ShooterPos.X = -3.5;
                        PositionerPos = new Position2D(-1.8, -Math.Sign(Model.BallState.Location.Y) * 0.3);

                        //AngleT = 80;
                        PassTarget = ShooterPos; //=new Position2D(Model.BallState.Location.X, Math.Sign(Model.BallState.Location.Y) * 0.1);


                        if (!passed)
                        {
                            passVec = Vector2D.FromAngleSize(Model.OurRobots[PasserId].Angle.Value * Math.PI / 180, 1);
                        }
                    }
                    PositionerAng = (ShootTarget - PositionerPos).AngleInDegrees;
                    Positioner2Ang = (ShootTarget - Positioner2Pos).AngleInDegrees;
                    passTargetCalculated = true;

                }

                if (sync.SyncStarted && !passed)
                {
                    if (Mode == 0)
                    {
                        Positioner2Pos = Model.BallState.Location + new Vector2D(1, Math.Sign(-Model.BallState.Location.Y) * 0.9);
                        Positioner2Ang = (ShootTarget - Positioner2Pos).AngleInDegrees;
                    }
                }
                if (sync.SyncStarted && passed)
                {

                    if (Mode == 1)
                    {
                        PositionerPos = new Position2D(-2.4, -Math.Sign(Model.BallState.Location.Y) * 0.4);
                    }
                }
                if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > ballMovedTresh)
                    passed = true;

                chip = chipOrigin;
                if (!passed)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    int idx;
                    chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07, out idx) || chipOrigin;
                    double nearestOppDist = double.MinValue;
                    if (obs.ObstaclesList.ContainsKey(idx))
                        nearestOppDist = obs.ObstaclesList[idx].State.Location.DistanceFrom(Model.BallState.Location) + 0.2;

                    if (Mode == 0)
                        PassSpeed = Math.Max(1.2, Model.BallState.Location.DistanceFrom(PassTarget) * .5);
                    else if (Mode == 1)
                        PassSpeed = (!chip) ? engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, chip, true) : Model.BallState.Location.DistanceFrom(PassTarget) * 0.50;
                    else if (Mode == 2)
                        PassSpeed = Math.Max(1.2, Model.BallState.Location.DistanceFrom(PassTarget) * .57);
                    if (chip)
                    {
                        PassSpeed = Math.Max(PassSpeed, nearestOppDist);// *GamePlannerInfo.ChipCoef[PasserId];
                    }
                }

                ShooterAngle = (ShootTarget - ShooterPos).AngleInDegrees;

            }
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(ShootTarget, 0.05, new Pen(Color.Red, 0.01f)), "ShootTarget");
                DrawingObjects.AddObject(new Circle(PassTarget, 0.05, new Pen(Color.Blue, 0.01f)), "PassTarget");
                DrawingObjects.AddObject("straState", new StringDraw("Strategy State: " + (State)CurrentState, GameParameters.OppGoalCenter + new Vector2D(-0.3, 0)));
                DrawingObjects.AddObject(new Circle(topLeft, 0.05, new Pen(Color.Blue, 0.01f)), "TopLeftPos");
            }
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserId, false);
                Planner.SetParameter(PasserId, 2.5, 2);
                Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);

                Planner.ChangeDefaulteParams(ShooterID, false);
                Planner.SetParameter(ShooterID, 3, 2.5);
                Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.UnSafe, true, true, true, true);


                Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, true, true, true);
                Planner.Add(PositionerID2, Positioner2Pos, Positioner2Ang, PathType.UnSafe, true, true, true, true);
            }
            else if (CurrentState == (int)State.Go)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));

                if (Mode == 0)
                {
                    Planner.ChangeDefaulteParams(PositionerID2, false);
                    Planner.SetParameter(PositionerID2, 6, 5);
                    Planner.Add(PositionerID2, Positioner2Pos, Positioner2Ang, PathType.UnSafe, false, false, true, true);

                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShooterPos, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShooterPos, ShootTarget, kickPowerType.Speed, PassSpeed, KickPower, RotateDelay, false, backSensor);

                    if (sync.SyncStarted && !getLastPasserPos && Model.OurRobots.ContainsKey(PasserId))
                    {
                        lastPasserPos = Model.OurRobots[PasserId].Location;
                        getLastPasserPos = true;
                    }

                    if (sync.SyncStarted && Model.OurRobots.ContainsKey(PasserId) && Model.OurRobots[PasserId].Location.DistanceFrom(lastPasserPos) > 0.01)
                        shooterIsNear = true;


                    if (shooterIsNear)
                    {
                        if (!passed)
                        {
                            Planner.ChangeDefaulteParams(PositionerID, false);
                            Planner.SetParameter(PositionerID, 7, 4);

                            Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, true, true, true);
                        }
                        else
                        {
                            double dist, DistFromBorder;
                            if ((Model.BallState.Speed.Size < 1.2 || Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionerID].Location) < 0.5) && GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                                ballMoved = true;

                            if (ballMoved)
                            {
                                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(ActiveRole)))
                                //    Functions[PositionerID] = (eng, wmd) => GetRole<ActiveRole>(PositionerID).PerformForStrategy(eng, Model, PositionerID, null, null, true, true, false);

                                getBallSkill.OutGoingBackTrack(engine, Model, PositionerID, ShootTarget);

                                Planner.AddKick(PositionerID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                            }
                            else
                            {
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(OneTouchRole)))
                                    Functions[PositionerID] = (eng, wmd) => GetRole<OneTouchRole>(PositionerID).Perform(eng, wmd, PositionerID, true, new SingleObjectState(new Position2D(-1.9, 0), Vector2D.Zero, 0), Model.OurRobots[PasserId], true, ShootTarget, Program.MaxKickSpeed, false, AngleT, !ballMoved);

                                Planner.AddKick(PositionerID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                                lastPositionerPos = Model.OurRobots[PositionerID].Location;
                            }
                            Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.UnSafe, true, true, true, true);

                        }
                    }
                    else if (Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < 2)
                        Planner.Add(PositionerID, ShooterPos + new Vector2D(0.6, 0), PositionerAng, PathType.UnSafe, true, true, true, true);
                    else
                        Planner.Add(PositionerID, new SingleWirelessCommand());
                }
                else if (Mode == 1)
                {
                    //Planner.ChangeDefaulteParams(ShooterID, false);
                    //Planner.SetParameter(ShooterID, 6, 5);
                    Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.UnSafe, false, !passed, true, !passed);

                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, PositionerID2, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, PositionerID2, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);

                    if (sync.SyncStarted && !getLastPasserPos && Model.OurRobots.ContainsKey(PasserId))
                    {
                        lastPasserPos = Model.OurRobots[PasserId].Location;
                        getLastPasserPos = true;
                    }

                    if (sync.SyncStarted && Model.OurRobots.ContainsKey(PasserId) && Model.OurRobots[PasserId].Location.DistanceFrom(lastPasserPos) > 0.01)
                        shooterIsNear = true;


                    if (shooterIsNear)
                    {
                        //if (!passed)
                        {
                            //Planner.ChangeDefaulteParams(PositionerID, false);
                            //Planner.SetParameter(PositionerID, 7, 4);

                            Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, !passed, true, !passed);
                        }
                        //else
                        //{
                        //    double dist, DistFromBorder;
                        //    if ((Model.BallState.Speed.Size < 1.2 || Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionerID].Location) < 0.5) && GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                        //        ballMoved = true;

                        //    if (ballMoved)
                        //    {
                        //        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(ActiveRole)))
                        //            Functions[PositionerID] = (eng, wmd) => GetRole<ActiveRole>(PositionerID).PerformForStrategy(eng, Model, PositionerID, null, null, true, true, false);

                        //        Planner.AddKick(PositionerID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                        //    }
                        //    else
                        //    {
                        //        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(OneTouchRole)))
                        //            Functions[PositionerID] = (eng, wmd) => GetRole<OneTouchRole>(PositionerID).Perform(eng, wmd, PositionerID, true, new SingleObjectState(new Position2D(-1.9, 0), Vector2D.Zero, 0), Model.OurRobots[PasserId], true, ShootTarget, Program.MaxKickSpeed, false, AngleT, !ballMoved);

                        //        Planner.AddKick(PositionerID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                        //        lastPositionerPos = Model.OurRobots[PositionerID].Location;
                        //    }
                        //  //  Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.UnSafe, true, true, true, true);

                        //}
                    }
                    else if (Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < 2)
                        Planner.Add(PositionerID, ShooterPos + new Vector2D(0.6, 0), PositionerAng, PathType.UnSafe, true, true, true, true);
                    else
                        Planner.Add(PositionerID, new SingleWirelessCommand());
                }
                else if (Mode == 2)
                {
                    Planner.ChangeDefaulteParams(PositionerID2, false);
                    Planner.SetParameter(PositionerID2, 6, 5);
                    Planner.Add(PositionerID2, new Position2D(-2.5, (Math.Sign(PasserPos.Y) * 1.5)), (ShootTarget - PasserPos).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, true, true, true);

                    //if (!passed)
                    //{
                        if (!chip)
                            sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShooterPos, ShootTarget, PassSpeed, KickPower, RotateDelay);
                        else
                            sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShooterPos, ShootTarget, kickPowerType.Speed, PassSpeed, KickPower, RotateDelay, true, backSensor);
                    //}
                    //else
                    //{
                    //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                    //        Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).Perform(eng, wmd, ShooterID, null);
                    //}

                }
                if (Model.OurRobots[PasserId].Location.DistanceFrom(Model.BallState.Location) > 0.25 && inPassState)
                {
                    if (Mode == 0)
                        Planner.Add(PasserId, new Position2D(-1.0, 0), 180, PathType.UnSafe, false, false, true, false);
                    else if (Mode == 1)
                    {
                        Planner.Add(PasserId, new Position2D(-0.5, Math.Sign(firstBallPos.Y) * (Math.Abs(GameParameters.OurLeftCorner.Y) - 0.4)), 180, PathType.UnSafe, false, true, true, false);
                    }
                    else if (Mode == 2)
                        Planner.Add(PasserId, Positioner2Pos, 180, PathType.UnSafe, false, true, true, false);
                }

                inrot = sync.InRotate;
                inPassState = sync.InPassState;
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
