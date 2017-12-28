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
    public class PassAndShoot3Strategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 0.5;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 100, initDist = 0.22, maxWaitTresh = 420, oppZoneMarg = 0.2;
        bool first, passTargetCalculated;
        int PasserId, ShooterID, PositionerID, goOtCounter;
        Position2D PasserPos, ShooterPos, PositionerPos, PassTarget, ShootTarget, lastShooterPos, lastPasserPos, lastPositionerPos;
        double PasserAngle, ShooterAngle, PositionerAng, RotateTeta, PassSpeed, KickPower;
        int counter, finishCounter, RotateDelay, timeLimitCounter;
        Syncronizer sync;
        bool chip, passed, chipOrigin, Debug = true, goActive, ballMoved;
        int Mode;
        bool shooterIsNear, getLastPasserPos, ballIndanger;
        GetBallSkill getBallSkill = new GetBallSkill(); bool backSensor;
        public override void ResetState()
        {
            backSensor = true;
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
            RotateTeta = 90;
            PassSpeed = 4.5;
            KickPower = Program.MaxKickSpeed;
            timeLimitCounter = 0;
            PasserId = -1;
            ShooterID = -1;
            inPassState = false;
            PasserPos = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PositionerPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;
            lastShooterPos = Position2D.Zero;
            lastPositionerPos = Position2D.Zero;
            lastPasserPos = Position2D.Zero;
            PasserAngle = 0;
            ShooterAngle = 0;
            PositionerAng = 0;
            goOtCounter = 0;
            counter = 0;
            finishCounter = 0;
            RotateDelay = 40;
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
            StrategyName = "SimplePassShoot3";
            AttendanceSize = 3;
            About = "this is Simple Pass Shoot strategy with 3 attendace!";
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
        Position2D topLeft = Position2D.Zero;
        double AngleT;
        Position2D firstBallPos;
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {

            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
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
                PasserId = minIdx;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PasserId && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
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
                    if (item != ShooterID && item != PasserId && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
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
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                //  if (Mode != 2)
                {
                    ShooterPos = PasserPos + new Vector2D(2, 0).GetNormalizeToCopy(passerShooterDist);
                }
                if (Mode == 3)
                {
                    ShooterPos = PasserPos + new Vector2D(4, 0).GetNormalizeToCopy(3);
                }
                //else
                //{
                //    ShooterPos = new Position2D(-2.2, Math.Sign(Model.BallState.Location.Y) * 0.7);
                //    ShooterPos = GameParameters.OppGoalCenter + (ShooterPos - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-ShooterPos, Vector2D.Zero, 0), oppZoneMarg + 0.2));
                //}
                if (Mode != 3)
                {
                    ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - .3, Math.Abs(ShooterPos.X)));
                }
                ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - .3, Math.Abs(ShooterPos.Y)));
                if (Mode == 3)
                { ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - .7, Math.Abs(ShooterPos.Y))); }
                ShooterPos.Y *= 1;

                ShooterAngle = 180;

                PositionerPos = new Position2D(Math.Max(Model.BallState.Location.X - 1, GameParameters.OppGoalCenter.X + 0.3), Math.Sign(-Model.BallState.Location.Y) * 1.4); ;
                PositionerAng = (ShootTarget - PositionerPos).AngleInDegrees;

                inrot = false;
            }
            else if (CurrentState == (int)State.Go)
            {

                if (!passTargetCalculated)
                {

                    if (Model.BallState.Location.X < -2.2 || Mode == 0)
                        chipOrigin = true;
                    if (chipOrigin)
                        RotateTeta = 0;

                    lastShooterPos = ShooterPos;

                    if (Mode == 0)
                    {
                        ShootTarget = GameParameters.OppGoalCenter.Extend(0, -Math.Sign(Model.BallState.Location.Y) * 0.2);
                        ShooterPos = PositionerPos + new Vector2D(0.25, Math.Sign(Model.BallState.Location.Y));// * 0.3);
                        PositionerPos = new Position2D(-1.8, -Math.Sign(Model.BallState.Location.Y) * 0.3);
                        if (Model.BallState.Location.X > -2.4)
                        {
                            AngleT = 80;
                            PassTarget = new Position2D(Model.BallState.Location.X, Math.Sign(Model.BallState.Location.Y) * 0.1);
                        }
                        else
                        {
                            AngleT = 60;
                            PassTarget = new Position2D(-2.5, Math.Sign(Model.BallState.Location.Y) * 0.3);
                        }

                        if (!passed)
                        {
                            passVec = Vector2D.FromAngleSize(Model.OurRobots[PasserId].Angle.Value * Math.PI / 180, 1);
                        }
                    }
                    else if (Mode == 1)
                    {
                        ShootTarget = GameParameters.OppGoalCenter.Extend(0, -Math.Sign(Model.BallState.Location.Y) * 0.1);
                        double width = 0.4, heigth = 0.6, step = 0.1;
                        topLeft = new Position2D(-1.9, ((Model.BallState.Location.Y > 0) ? -(heigth + 0.7) : 0.7));
                        ShooterPos = new Position2D(-1, Math.Sign(Model.BallState.Location.Y) * 0.5);
                        ShooterPos = GameParameters.OppGoalCenter.Extend(0, -.2) + (ShooterPos - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-ShooterPos, Vector2D.Zero, 0), oppZoneMarg));
                        chipOrigin = false;
                        chip = chipOrigin;
                        bool b = engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref PassTarget);
                        chipOrigin = chip;
                        AngleT = 70;
                        //PassTarget = ShooterPos + new Vector2D(-0.3, 0);
                        //PassTarget = new Position2D(-1.95, Math.Sign(Model.BallState.Location.Y) * 0.5);
                    }
                    else if (Mode == 2)
                    {
                        chipOrigin = false;
                        ShootTarget = GameParameters.OppGoalCenter.Extend(0, Math.Sign(Model.BallState.Location.Y) * 0.2);
                        double width = 0.3, heigth = 0.8, step = 0.1;
                        topLeft = new Position2D(Model.BallState.Location.X + 1.6, ((Model.BallState.Location.Y < 0) ? -(heigth + 1.2) : 1.2));
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);

                        PassTarget = Model.BallState.Location + new Vector2D(1.3, -Math.Sign(Model.BallState.Location.Y) * 0.2);
                        if (obs.Meet(new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.2))
                        {
                            chip = chipOrigin;
                            bool b = engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref PassTarget);
                            chipOrigin = chip;
                        }
                        if (obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07))
                            chipOrigin = true;
                        if (chipOrigin)
                            RotateTeta = 0;
                        AngleT = 70;
                        RotateTeta = 50;
                    }
                    else if (Mode == 3)
                    {
                        ShootTarget = GameParameters.OppGoalCenter.Extend(0, -Math.Sign(Model.BallState.Location.Y) * 0.15);
                        double width = 0.4, heigth = 0.2, step = 0.1;
                        topLeft = new Position2D(-2.1, ((Model.BallState.Location.Y > 0) ? -(heigth + .5) : .5));
                        chipOrigin = true;
                        chip = chipOrigin;
                        bool b = engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref PassTarget);
                        chipOrigin = chip;

                        RotateTeta = 0;
                        PositionerPos = new Position2D();//ShooterPos + new Vector2D(-0.7, 0);

                    }

                    PositionerAng = (ShootTarget - PositionerPos).AngleInDegrees;
                    passTargetCalculated = true;

                }

                if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh)
                    passed = true;
                if (passed && Mode == 0)
                    ShootTarget = GameParameters.OppGoalCenter.Extend(0, Math.Sign(Model.BallState.Location.Y) * 0.1);
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
                        PassSpeed = Math.Max(.85, Model.BallState.Location.DistanceFrom(PassTarget));// * .5) ; 
                    else if (Mode == 2)
                        PassSpeed = (!chip) ? .9 * engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PositionerPos, chip, true) : Model.BallState.Location.DistanceFrom(PassTarget);
                    else if (Mode == 1)
                        PassSpeed = (!chip) ? engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, chip, true) : Model.BallState.Location.DistanceFrom(PassTarget) * .7;
                    else if (Mode == 3)
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.47;
                    if (chip)
                    {
                        //PassSpeed = Math.Max(PassSpeed, nearestOppDist) * GamePlannerInfo.ChipCoef[PasserId];
                    }
                }

                ShooterAngle = (ShootTarget - ShooterPos).AngleInDegrees;
                if (Mode == 2)
                {
                    ShooterPos = new Position2D(-2.1, Math.Sign(Model.BallState.Location.Y) * 0.2);
                    ShooterPos = GameParameters.OppGoalCenter + (ShooterPos - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-ShooterPos, Vector2D.Zero, 0), (!passed) ? oppZoneMarg : 0));

                }
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
        Vector2D passVec = Vector2D.Zero;
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
            }
            else if (CurrentState == (int)State.Go)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));

                if (Mode == 0)
                {
                    //if (!chip)
                    PassSpeed *= 1.8;
                    sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShooterPos, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    //else
                    //sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShooterPos, ShootTarget, kickPowerType.Speed, PassSpeed, KickPower, RotateDelay, false, backSensor);

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
                                //getBallSkill.OutGoingBackTrack(engine, Model, shooterID, shootTarget);
                                //getBallSkill.OutGoingSideTrack(Model, PositionerID, ShootTarget);
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(ActiveRole)))
                                    Functions[PositionerID] = (eng, wmd) => GetRole<ActiveRole>(PositionerID).PerformForStrategy(eng, Model, PositionerID, null, null, true, true, false);
                                //Planner.AddKick(shooterID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, shooterID, typeof(ActiveRole)))
                                //    Functions[shooterID] = (eng, wmd) => GetRole<ActiveRole>(shooterID).Perform(eng, Model, shooterID, null, null, true, true, false);
                                Planner.AddKick(PositionerID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                            }
                            //if ((Model.BallState.Speed.Size < 2.5 || Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionerID].Location) < 0.7) && GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                            //    ballMoved = true;

                            //if (ballMoved)
                            //{

                            //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(ActiveRole)))
                            //        Functions[PositionerID] = (eng, wmd) => GetRole<ActiveRole>(PositionerID).Perform(eng, Model, PositionerID, null, null, true, true, false);
                            //    Planner.AddKick(PositionerID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                            //    //getBallSkill.OutGoingBackTrack(engine, Model, PositionerID, ShootTarget);
                            //    Planner.AddKick(PositionerID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                            //}
                            else
                            {
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(OneTouchRole)))
                                    Functions[PositionerID] = (eng, wmd) => GetRole<OneTouchRole>(PositionerID).Perform(eng, wmd, PositionerID, true, new SingleObjectState(new Position2D(-1.9, 0), Vector2D.Zero, 0), Model.OurRobots[PasserId], true, ShootTarget, Program.MaxKickSpeed, false, AngleT, !ballMoved);
                                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(ActiveRole)))
                                //    Functions[PositionerID] = (eng, wmd) => GetRole<ActiveRole>(PositionerID).Perform(eng, Model, PositionerID, null, null, true, true, false);
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
                    Planner.ChangeDefaulteParams(ShooterID, false);
                    Planner.SetParameter(ShooterID, 8, 5);

                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, PositionerID, /*ShooterPos + new Vector2D(-0.1, 0)*/PassTarget/*, ShooterPos*/, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, PositionerID, /*ShooterPos + new Vector2D(-0.1, 0)*/PassTarget,/* ShooterPos, */ShootTarget/*, kickPowerType.Speed*/, PassSpeed, KickPower, RotateDelay, backSensor/*, false*/);
                    Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.UnSafe, false, true, true, true);

                    //if (!passed)
                    //{
                    //    Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, true, true, true);
                    //}
                    //else
                    //{
                    //    Planner.ChangeDefaulteParams(PositionerID, false);
                    //    Planner.SetParameter(PositionerID, 7, 4);

                    //    goOtCounter++;
                    //    if (goOtCounter > 0)
                    //        ballMoved = true;
                    //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID, typeof(OneTouchRole)))
                    //        Functions[PositionerID] = (eng, wmd) => GetRole<OneTouchRole>(PositionerID).Perform(eng, wmd, PositionerID, true,  new SingleObjectState(PassTarget + new Vector2D(0.5,0), Vector2D.Zero, 0), Model.OurRobots[PasserId], (chip), ShootTarget, Program.MaxKickSpeed, false, AngleT, ballMoved);

                    //}
                }
                else if (Mode == 2)
                {
                    goOtCounter++;
                    if (goOtCounter >= 0)
                    {
                        Planner.ChangeDefaulteParams(PositionerID, false);
                        Planner.SetParameter(PositionerID, 8, 5);
                        if (!chip)
                            sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, PositionerID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                        else
                            sync.SyncChipPass(engine, Model, PasserId, RotateTeta, PositionerID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
                    }
                    //Planner.ChangeDefaulteParams(ShooterID, false);
                    //Planner.SetParameter(ShooterID, 8, 5);
                    if (goOtCounter > 20)
                    {
                        Planner.ChangeDefaulteParams(ShooterID, false);
                        Planner.SetParameter(ShooterID, 5, 3.0);
                        Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.UnSafe, true, true, true, !passed);
                    }
                }
                else if (Mode == 3)
                {
                    Planner.ChangeDefaulteParams(PositionerID, false);
                    Planner.SetParameter(PositionerID, 7, 4);
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
                    if (sync.SyncStarted)
                        Planner.Add(PositionerID, PositionerPos, PositionerAng, PathType.UnSafe, true, true, true, true);
                }

                if (Model.OurRobots[PasserId].Location.DistanceFrom(Model.BallState.Location) > 0.25 && inPassState)
                {
                    if (Mode != 2)
                        Planner.Add(PasserId, new Position2D(-1.0, 0), 180, PathType.UnSafe, false, false, true, false);
                    else
                        Planner.Add(PasserId, GameParameters.OppGoalCenter + (PasserPos - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PasserPos, Vector2D.Zero, 0), 0)), 180, PathType.UnSafe, false, false, true, false);
                }

                inrot = sync.InRotate;
                inPassState = sync.InPassState;
            }

            return CurrentlyAssignedRoles;
        }

        bool inrot = false, inPassState = false;
        enum State
        {
            First,
            Go,
            Finish
        }
    }
}
