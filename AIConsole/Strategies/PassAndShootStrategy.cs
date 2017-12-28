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
    public class PassAndShootStrategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 1;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 100, initDist = 0.25, maxWaitTresh = 600;
        bool first, passTargetCalculated, Debug = true;
        int PasserId, ShooterID;
        Position2D PasserPos, ShooterPos, PassTarget, ShootTarget;
        double PasserAngle, ShooterAngle, RotateTeta, PassSpeed, KickPower;
        int counter, finishCounter, RotateDelay, timeLimitCounter;
        Syncronizer sync;
        int mode;
        bool chip, passed, chipOrigin;
        Position2D firstBallPos;
        bool backSensor;

        public override void ResetState()
        {

            UseInMiddle = true;
            backSensor = false;
            mode = 0;
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
            PasserId = -1;
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
            UseOnlyInMiddle = true; //TODO: true for main game
            StrategyName = "SimplePassShoot2013";
            AttendanceSize = 2;
            UseInMiddle = true;
            About = "this is Simple Pass Shoot strategy!";
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
                ShooterID = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != PasserId)
                    {
                        ShooterID = item;
                        break;
                    }
                }
                if (ShooterID == -1)
                    return;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (!Model.OurRobots.ContainsKey(PasserId) || !Model.OurRobots.ContainsKey(ShooterID))
                return;

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
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.06)
                    CurrentState = (int)State.Finish;
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
                if (mode == 0)
                    ShootTarget = GameParameters.OppGoalCenter;
                else
                    ShootTarget = Position2D.Zero;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                ShooterPos = PasserPos + new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                //  ShooterPos = new Position2D(2,1.6);//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);
                ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - 0.2, Math.Abs(ShooterPos.X)));
                ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - 0.2, Math.Abs(ShooterPos.Y)));
                ShooterPos.Y *= 1;
                ShooterAngle = 180;
                inrot = false;
            }
            else if (CurrentState == (int)State.Go)
            {

                if (!passTargetCalculated)
                {
                    if (mode == 0)
                    {
                        ShootTarget = GameParameters.OppGoalCenter;//(Model.BallState.Location.Y >= 0) ? GameParameters.OppGoalRight : GameParameters.OppGoalLeft;

                        double width = 0.9, heigth = 0.9, step = 0.2;
                        double margin = 0;
                        if (Model.BallState.Location.X < -2)
                            margin = 0.8;
                        Position2D topLeft = new Position2D(Model.BallState.Location.X + width + margin, ((Model.BallState.Location.Y > 0) ? -(1 + heigth) : 1));


                        if (Model.BallState.Location.X < -(GameParameters.OurGoalCenter.X - GameParameters.DefenceareaRadii))
                            chipOrigin = true;
                        if (chipOrigin)
                            RotateTeta = 0;
                        chip = chipOrigin;
                        bool b = engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref PassTarget);
                        //PassTarget = Model.BallState.Location + new Vector2D(2, -Math.Sign(Model.BallState.Location.Y) * .8);
                        // PassTarget = new Position2D(-2, -Math.Sign(Model.BallState.Location.Y) * 0.2);
                        chipOrigin = chip;
                        //PassTarget = Model.BallState.Location + new Vector2D(0.8, 0.5 * -Math.Sign(Model.BallState.Location.Y));
                        double goodness;
                        var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, PassTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                        if (GoodPointInGoal.HasValue)
                            ShootTarget = GoodPointInGoal.Value;
                    }
                    else
                    {
                        ShootTarget = GameParameters.OppGoalCenter;//(Model.BallState.Location.Y >= 0) ? GameParameters.OppGoalRight : GameParameters.OppGoalLeft;
                        double width = 0.9, heigth = 0.9, step = 0.2;
                        double margin = 0;
                        if (Model.BallState.Location.X < -2)
                            margin = 0.8;
                        Position2D topLeft = new Position2D(Model.BallState.Location.X + width + margin, ((Model.BallState.Location.Y > 0) ? -(1 + heigth) : 1));


                        bool b = engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref PassTarget);

                        chipOrigin = false;

                        chip = chipOrigin;

                    }
                    //    PassTarget = new Position2D(.5,-1.7);//for middle freekick new Position2D(-2.8, -.8);// for cornel:new Position2D(-2.5, -.4);// new Position2D(Model.BallState.Location.X, -Model.BallState.Location.Y);
                    passTargetCalculated = true;

                }
                if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05)
                    passed = true;
                if (mode == 0)
                {
                    chip = chipOrigin;
                    if (!passed && !chipOrigin)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                        chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.15);

                    }
                }
                else
                {
                    chip = chipOrigin;
                }
                if (!passed)
                {
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, false, true);
                    if (chip)
                    {
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.6;
                    }
                }
            }

            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            //   CharterData.AddData("ballspeed", Model.BallState.Speed.Size);
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            if (CurrentState == (int)State.First)
            {
                //Planner.ChangeDefaulteParams(PasserId, false);
                //Planner.SetParameter(PasserId, 2.5, 2);
                //Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserId, typeof(ActiveRole)))
                    Functions[PasserId] = (eng, wmd) => GetRole<ActiveRole>(PasserId).PerformWithoutKick(engine, Model, PasserId, ShootTarget, false, initDist);

                //Planner.ChangeDefaulteParams(ShooterID, false);
                //Planner.SetParameter(ShooterID, 3, 2.5);
                Planner.Add(ShooterID, ShooterPos, ShooterAngle, PathType.Safe, true, true, true, true);
            }
            else if (CurrentState == (int)State.Go)
            {
                //Planner.ChangeDefaulteParams(ShooterID, false);
                //Planner.SetParameter(ShooterID, 7, 5);
                if (mode == 0)
                {
                    if (Debug)
                        DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
                    if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) > 0.15)
                    {
                        Planner.Add(PasserId, new Position2D(-1.2, 0), 180, PathType.UnSafe, false, true, true, true);
                    }
                }
                else
                {

                    Vector2D BallTarget = ShootTarget - Model.BallState.Location;
                    Vector2D InitBall = Model.BallState.Location - PasserPos;
                    double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));

                    if (Debug)
                        DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserPos, PasserId, ShooterID, ShootTarget, PassTarget, ShootTarget, KickPower, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserPos, PasserId, ShooterID, ShootTarget, PassTarget, ShootTarget, kickPowerType.Speed, KickPower, KickPower, RotateDelay, true, backSensor);
                    if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) > 0.15)
                    {
                        Planner.Add(PasserId, new Position2D(-1.2, 0), 180, PathType.UnSafe, false, true, true, true);
                    }
                }
                inPassState = sync.InPassState;
            }
            return CurrentlyAssignedRoles;
        }
        bool inrot = false, inPassState = false;
        public enum State
        {
            First,
            Go,
            Finish
        }
    }
}
