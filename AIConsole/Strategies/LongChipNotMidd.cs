using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;


namespace MRL.SSL.AIConsole.Strategies
{
    public class LongChipNotStrategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2;
        const double tresh = 0.06, angleTresh = 2, waitTresh = 40, finishTresh = 100, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 4, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        bool first, passTargetCalculated, Debug = false, nearShooter, shooted;
        int PasserId, PositionerID0, PositionerID1;
        Position2D PasserPos, ShooterPos, PassTarget, ShootTarget, PositionerPos0, PositionerPos1, firstBallPos;
        double PasserAngle, ShooterAngle, RotateTeta, PassSpeed, KickPower;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        kickPowerType kickType;
        bool chip, passed, chipOrigin;


        public override void ResetState()
        {
            UseInMiddle = true;
            kickType = kickPowerType.Speed;
            faildCounter = 0;
            nearShooter = false;
            shooted = false;
            chip = false;
            chipOrigin = false;
            passed = false;
            CurrentState = InitialState;
            first = true;
            passTargetCalculated = false;
            RotateTeta = 60;
            PassSpeed = 4;
            KickPower = 8;
            timeLimitCounter = 0;
            PasserId = -1;
            PositionerID0 = 0;
            PositionerID1 = 0;

            PositionerPos0 = Position2D.Zero;
            PositionerPos1 = Position2D.Zero;
            PasserPos = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;
            firstBallPos = Position2D.Zero;

            PasserAngle = 0;
            ShooterAngle = 0;
            counter = 0;
            finishCounter = 0;
            RotateDelay = 60;
            inPassState = false;
            if (sync != null)
            {
                sync.Reset();
            }
            else
            {
                sync = new Syncronizer();
            }
        }
        double margin = 1.5;
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
            UseInMiddle = true;
            StrategyName = "LongChipNotMidStrategy";
            AttendanceSize = 3;
            UseOnlyInMiddle = false;
            About = "this is a long chip Pass strategy!";
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
                double minDist = double.MaxValue;
                int minIdx = -1;
                double maxDist = double.MinValue;
                int maxIdx = -1;

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
                PositionerID0 = minIdx;
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (PasserId != item && PositionerID0 != item && Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID1 = minIdx;

                firstBallPos = Model.BallState.Location;
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

                if (Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1 && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < 0.23)
                    counter++;
                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    timeLimitCounter = 0;
                    CurrentState = (int)State.Go;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Go)
            {
                timeLimitCounter++;
                if (passed)
                    finishCounter++;
                if (sync.Finished || sync.Failed || finishCounter > finishTresh || timeLimitCounter > maxWaitTresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    //if ((v.Y < faildFarPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionerID0].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PositionerID0].Location) <= faildBallDistSecondPass))
                    //{
                        faildCounter++;
                        if (faildCounter > faildMaxCounter)
                            CurrentState = (int)State.Finish;
                    //}
                    //else
                    //    faildCounter = Math.Max(0, faildCounter - 5);
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

                if (Model.BallState.Location.DistanceFrom(PositionerPos1) < 1 || Model.BallState.Location.DistanceFrom(PositionerPos0) < 1)
                    CurrentState = (int)State.Finish;

            }
            #endregion
            #region PosAndAngles
            if (CurrentState == (int)State.First)
            {
                margin = 0.5;
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                ShooterPos = new Position2D(1.7, PasserPos.Y);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                //  ShooterPos = new Position2D(2,1.6);//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);
                ShooterPos.X = Math.Sign(ShooterPos.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - 0.2, Math.Abs(ShooterPos.X)));
                ShooterPos.Y = Math.Sign(ShooterPos.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - 0.2, Math.Abs(ShooterPos.Y)));
                ShooterPos.Y *= 1;
                ShooterAngle = 180;

                Vector2D v = GameParameters.OppGoalCenter - Model.BallState.Location;

                Position2D safeBall = GameParameters.OppGoalCenter + (Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-Model.BallState.Location, Vector2D.Zero, 0), margin));
                Position2D tmpP0 = safeBall + Vector2D.FromAngleSize(v.AngleInRadians - Math.Sign(Model.BallState.Location.Y) * Math.PI / 2, 0.15);
                Position2D tmpP1 = safeBall + Vector2D.FromAngleSize(v.AngleInRadians + Math.Sign(Model.BallState.Location.Y) * Math.PI / 2, 0.15);

                PositionerPos0 = tmpP0;
                //PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));

                PositionerPos1 = tmpP1;
                //PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));

                //PositionerPos1 = new Position2D(-2.12, 0.15 * -Math.Sign(Model.BallState.Location.Y));
                //PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));

                //PositionerPos0 = new Position2D(-2.12, 0.15 * Math.Sign(Model.BallState.Location.Y));
                //PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                inrot = false;
            }
            else if (CurrentState == (int)State.Go)
            {

                if (!passTargetCalculated)
                {
                    ShootTarget = GameParameters.OppGoalCenter;
                    PassTarget = Position2D.Interpolate(PositionerPos0, PositionerPos1, 0.5);//PositionerPos0 + new Vector2D(-0.2, (PositionerPos0.Y + PositionerPos1.Y) / 2);
                    passTargetCalculated = true;

                }

                if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05)
                    passed = true;
                chip = chipOrigin;
                if (!passed && !chipOrigin)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);

                }
                if (!passed)
                {
                    if (Model.BallState.Location.X > GameParameters.OurGoalCenter.X / 2)
                    {
                        PassSpeed = 4;//Model.BallState.Location.DistanceFrom(PassTarget);
                        kickType = kickPowerType.Speed;
                    }
                    else
                    {
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget + (GameParameters.OppGoalCenter - PassTarget).GetNormalizeToCopy(0.3)) * 0.4;
                        kickType = kickPowerType.Speed;
                    }
                    //     PassSpeed = 1.5;
                    chip = true;
                    // Model.BallState.Location.DistanceFrom(PassTarget);//engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, chip, true);
                    double goodness;
                    var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, PassTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    if (GoodPointInGoal.HasValue)
                        ShootTarget = GoodPointInGoal.Value;
                }
            }

            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {

            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserId, false);
                Planner.SetParameter(PasserId, 2.5, 2);
                Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);

                Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

            }
            else if (CurrentState == (int)State.Go)
            {
                if (Debug)
                    DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));
                if (Planner.AddRotate(Model, PasserId, PassTarget, 0, kickType, PassSpeed, true).InKickState)
                    inPassState = true;
                Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserId].Location) > 0.25)
                {
                    Planner.Add(PasserId, new Position2D(-1.2, Math.Sign(PasserPos.Y) * 0), 180, PathType.UnSafe, false, true, true, true);
                }
                
            }
            Functions = new Dictionary<int, CommonDelegate>();
            return new Dictionary<int, RoleBase>();
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
