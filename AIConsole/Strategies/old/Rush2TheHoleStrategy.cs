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

namespace MRL.SSL.AIConsole.Strategies
{
    public class Rush2TheHoleStrategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 100, initDist = 0.22, maxWaitTresh = 300, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        bool inrot = false, inPassState = false;
        bool first, passTargetCalculated, Debug = false, nearShooter, shooted;
        int PasserId, DefenderID, PositionerID0, ShooterID, PositionerID1, PositionerID2;
        Position2D PasserPos, PickerPos, PositionerPos1, PassTarget, ShootTarget, PositionerPos0, ShooterPos, firstBallPos, lastPositioner1Pos, PositionerPos2, SubModePos;
        Position2D tmpPos0;
        Position2D goaliPos, defenderPos;
        double goaliAng, defenderAng;
        double PasserAngle, PickerAngle, PositionerAngle1, RotateTeta, PassSpeed, KickPower, tmpAng0, PositionerAngle0, PositionerAngle2;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        int Mode;
        int SubMode;
        bool chip, passed, chipOrigin, goOneTouch;
        bool backSensor;
        double ShooterAngle;
        bool reached1stPos;
        bool goActive;
        GetBallSkill getBallSkill;
        public override void ResetState()
        {
            getBallSkill = new GetBallSkill();
            goActive = false;
            reached1stPos = false;
            ShooterAngle = 0;
            PositionerAngle0 = 0;
            tmpPos0 = Position2D.Zero;
            tmpAng0 = 0;
            backSensor = true;
            Mode = 3;
            SubMode = 0;
            goOneTouch = false;
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
            KickPower = Program.MaxKickSpeed;
            timeLimitCounter = 0;
            PasserId = -1;
            DefenderID = -1;
            PositionerID0 = 1;
            ShooterID = -1;
            PositionerID1 = -1;

            PositionerPos1 = Position2D.Zero;
            lastPositioner1Pos = Position2D.Zero;
            PositionerPos0 = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PasserPos = Position2D.Zero;
            PickerPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;
            firstBallPos = Position2D.Zero;

            PasserAngle = 0;
            PickerAngle = 0;
            PositionerAngle1 = 0;

            counter = 0;
            finishCounter = 0;
            RotateDelay = 20;
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

        public override void InitializeStates(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> attendance)
        {
            Attendance = attendance;
            CurrentState = (int)State.First;
            InitialState = 0;

            FinalState = 3;
            TrapState = 3;
        }

        public override void FillInformation()
        {
            StrategyName = "Rush2Hole5rStrategy";
            AttendanceSize = 6;
            About = "this strategy will try to enter to the opp hole!";
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
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
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
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {
            #region Ids
            if (first)
            {
                var tmpIds = RemoveGoaliID(Model, Attendance);
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
                PasserId = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (-Model.OurRobots[item].Location.X < minDist)
                    {
                        minDist = -Model.OurRobots[item].Location.X;
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                ShooterID = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID0 = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                if (Mode == 3)
                    PositionerID2 = minIdx;
                else
                    DefenderID = minIdx;

                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID1 = minIdx;
                tmpIds.Remove(minIdx);
                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (Mode != 3 && (!Model.OurRobots.ContainsKey(PasserId) || !Model.OurRobots.ContainsKey(PositionerID0)
                || !Model.OurRobots.ContainsKey(PositionerID1) || !Model.OurRobots.ContainsKey(DefenderID)
                || !Model.OurRobots.ContainsKey(ShooterID)))
                return;
            else if (Mode == 3 && (!Model.OurRobots.ContainsKey(PasserId) || !Model.OurRobots.ContainsKey(PositionerID0)
                || !Model.OurRobots.ContainsKey(PositionerID1) || !Model.OurRobots.ContainsKey(PositionerID2)
                || !Model.OurRobots.ContainsKey(ShooterID)))
            {
                return;
            }
            #region States
            if (CurrentState == (int)State.First)
            {
                double dAngle = Model.OurRobots[PasserId].Angle.Value - PasserAngle;
                timeLimitCounter++;
                if (dAngle > 180)
                    dAngle -= 360;
                else if (dAngle < -180)
                    dAngle += 360;

                if (Mode != 3 && Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[DefenderID].Location.DistanceFrom(ShooterPos) < tresh && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(defenderPos) < 0.23
                    && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < tresh && (!Model.GoalieID.HasValue || !Model.OurRobots.ContainsKey(Model.GoalieID.Value) || Model.OurRobots[Model.GoalieID.Value].Location.DistanceFrom(goaliPos) < tresh))
                    counter++;
                else if (Mode == 3 && Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1 && Model.OurRobots[PositionerID2].Location.DistanceFrom(PositionerPos2) < 0.1
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(defenderPos) < 0.23
                    && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < tresh && (!Model.GoalieID.HasValue || !Model.OurRobots.ContainsKey(Model.GoalieID.Value) || Model.OurRobots[Model.GoalieID.Value].Location.DistanceFrom(goaliPos) < tresh))
                {

                }
                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    counter = 0;
                    timeLimitCounter = 0;
                    if (Mode != 3)
                    {
                        CurrentState = (int)State.Move;
                    }
                    else
                        CurrentState = (int)State.Go;

                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Move)
            {

                if (Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < 0.1
                    && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    timeLimitCounter = 0;
                    counter = 0;
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
                if (Mode == 1)
                {
                    if (Model.BallState.Location.DistanceFrom(Model.OurRobots[DefenderID].Location) < 0.3)
                        nearShooter = true;
                    if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[DefenderID].Location - Model.OurRobots[PasserId].Location) <= 0)
                        shooted = true;
                    if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[DefenderID].Location) > 0.35)
                        CurrentState = (int)State.Finish;
                }
                else if (Mode == 2 || Mode == 0 || Mode == 3)
                {
                    if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.3)
                        nearShooter = true;
                    if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[ShooterID].Location - Model.OurRobots[PasserId].Location) <= 0)
                        shooted = true;
                    if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > 0.35)
                        CurrentState = (int)State.Finish;
                }
            }
            #endregion

            CalculateDefenderInfo(engine, Model, out defenderPos, out goaliPos, out defenderAng, out goaliAng);
            if (!passed)
                defenderPos = new Position2D(1.8, Math.Sign(Model.BallState.Location.Y) * 1);
            if (CurrentState == (int)State.First)
            {
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                if (Mode == 0)
                {
                    PositionerPos1 = new Position2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.8);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    PositionerAngle1 = 180;

                    ShooterPos = new Position2D(-0.7, 0);
                    ShooterAngle = 180;
                    PositionerPos0 = new Position2D(-0.7, 0.8 * Math.Sign(Model.BallState.Location.Y));
                }
                else if (Mode == 1)
                {
                    PositionerPos1 = new Position2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.8);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    PositionerAngle1 = 180;

                    ShooterPos = new Position2D(-0.7, 0.3 * Math.Sign(Model.BallState.Location.Y));
                    ShooterAngle = 180;
                    PositionerPos0 = new Position2D(-0.7, 0.8 * Math.Sign(Model.BallState.Location.Y));
                }
                else if (Mode == 2)
                {
                    PositionerPos1 = new Position2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.8);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    PositionerAngle1 = 180;

                    ShooterPos = new Position2D(-0.7, 0);
                    ShooterAngle = 180;
                    PositionerPos0 = new Position2D(-0.7, 0.8 * Math.Sign(Model.BallState.Location.Y));
                }
                else if (Mode == 3)
                {
                    SubModePos = new Position2D(-3.8, -Math.Sign(Model.BallState.Location.Y) * 1.5);
                    PositionerPos1 = new Position2D(-2.7, -Math.Sign(Model.BallState.Location.Y) * 0.4);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                    PositionerAngle1 = (ShootTarget - PositionerPos1).AngleInDegrees;

                    PositionerPos2 = new Position2D(-2.7, -Math.Sign(Model.BallState.Location.Y) * 0.2);
                    PositionerPos2 = GameParameters.OppGoalCenter + (PositionerPos2 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos2, Vector2D.Zero, 0), margin));
                    ShooterAngle = (ShootTarget - PositionerPos2).AngleInDegrees;

                    PositionerPos0 = new Position2D(-2.7, -Math.Sign(Model.BallState.Location.Y) * 0);
                    PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));
                    PositionerAngle0 = (ShootTarget - PositionerPos0).AngleInDegrees;
                }
                inrot = false;
            }
            else if (CurrentState == (int)State.Move)
            {
                if (Mode == 0)
                {
                    PositionerPos0 = new Position2D(-1.5, Math.Sign(Model.BallState.Location.Y) * 1.5);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);

                    PositionerPos1 = new Position2D(-2.23, 1.24 * -Math.Sign(Model.BallState.Location.Y));
                    PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                }
                else if (Mode == 1)
                {
                    PositionerPos0 = new Position2D(-1.5, Math.Sign(Model.BallState.Location.Y) * 1.5);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);

                    PositionerPos1 = new Position2D(-2.23, 1.24 * -Math.Sign(Model.BallState.Location.Y));
                    PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                }
                else if (Mode == 2)
                {
                    PositionerPos1 = new Position2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.8);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    PositionerAngle1 = 180;

                    ShooterPos = new Position2D(-0.7, 0);
                    ShooterAngle = 180;
                    PositionerPos0 = new Position2D(-0.7, 0.8 * Math.Sign(Model.BallState.Location.Y));
                }
            }
            else if (CurrentState == (int)State.Go)
            {

                if (!passTargetCalculated)
                {
                    if (Model.BallState.Location.X < -3.2 && Mode != 1 && Mode != 3)
                        chipOrigin = true;
                    if (chipOrigin)
                        RotateTeta = 0;

                    if (Mode == 0)
                    {
                        ShootTarget = new Position2D(GameParameters.OppGoalCenter.X, Math.Sign(Model.BallState.Location.Y) * 0.2);

                        chip = chipOrigin;
                        PassTarget = new Position2D(-2.2, -Math.Sign(Model.BallState.Location.Y) * 0.2);

                    }
                    else if (Mode == 1)
                    {
                        ShootTarget = new Position2D(GameParameters.OppGoalCenter.X, Math.Sign(Model.BallState.Location.Y) * 0.2);

                        chip = chipOrigin;
                        PassTarget = new Position2D(-1.5, Math.Sign(Model.BallState.Location.Y) * 0.3);

                    }
                    else if (Mode == 2)
                    {
                        ShootTarget = new Position2D(GameParameters.OppGoalCenter.X, Math.Sign(Model.BallState.Location.Y) * 0.2);


                        chip = chipOrigin;
                        PassTarget = new Position2D(-2.2, Math.Sign(Model.BallState.Location.Y) * -0.2);

                        PositionerPos0 = new Position2D(-1.5, Math.Sign(Model.BallState.Location.Y) * 1.5);

                        PositionerPos1 = new Position2D(-2.23, 1.24 * -Math.Sign(Model.BallState.Location.Y));
                        PositionerPos1 = GameParameters.OppGoalCenter + (PositionerPos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos1, Vector2D.Zero, 0), margin));
                    }
                    else if (Mode == 3)
                    {
                        chip = chipOrigin;
                        PassTarget = new Position2D(Model.BallState.Location.X + 2, Model.BallState.Location.Y - Math.Sign(Model.BallState.Location.Y) * 0.8);
                        ShootTarget = new Position2D(GameParameters.OppGoalCenter.X, Math.Sign(Model.BallState.Location.Y) * 0.3);
                    }

                    passTargetCalculated = true;
                }
                if (sync.InPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > BallMovedTresh)
                    passed = true;
                chip = chipOrigin;
                if (Mode == 0)
                {
                    if (sync.SyncStarted)
                    {
                        if (reached1stPos)
                        {
                            ShooterPos = new Position2D(-1.1, Math.Sign(firstBallPos.Y) * 1.2);
                        }
                        else
                            ShooterPos = new Position2D(-0.9, Math.Sign(firstBallPos.Y) * 0.5);
                        if (!reached1stPos && Model.OurRobots[DefenderID].Location.DistanceFrom(ShooterPos) < 0.2)
                            reached1stPos = true;

                    }
                }
                else if (Mode == 1)
                {
                    if (sync.InRotate)
                    {
                        ShooterPos = new Position2D(-2, -Math.Sign(Model.BallState.Location.Y) * 0.6);
                    }
                }
                else if (Mode == 2)
                {
                    if (sync.SyncStarted)
                    {
                        if (reached1stPos)
                        {
                            ShooterPos = new Position2D(-1.1, Math.Sign(firstBallPos.Y) * 1.2);
                        }
                        else
                            ShooterPos = new Position2D(-0.9, Math.Sign(firstBallPos.Y) * 0.5);
                        if (!reached1stPos && Model.OurRobots[DefenderID].Location.DistanceFrom(ShooterPos) < 0.2)
                            reached1stPos = true;

                    }
                }
                if (!passed && !chipOrigin)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { ShooterID, PasserId }, null);
                    chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);

                }
                if (!passed)
                {
                    PassSpeed = (chip) ? Math.Max(0.8, Model.BallState.Location.DistanceFrom(PassTarget) * 0.55) : engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, chip, true);
                }

            }
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();

            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserId, false);
                Planner.SetParameter(PasserId, 2.5, 2);
                Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);

                Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                if (Mode == 0 || Mode == 1 || Mode == 2)
                {
                    Planner.Add(ShooterID, defenderPos, defenderAng, PathType.Safe, true, true, true, true);
                    Planner.Add(DefenderID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else if (Mode == 3)
                {
                    Planner.Add(PositionerID2, PositionerPos2, (ShootTarget - PositionerPos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                    if (SubMode == 0)
                    {
                        Planner.Add(ShooterID, SubModePos, (ShootTarget - SubModePos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                    else
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(DefenderCornerRole1)))
                            Functions[ShooterID] = (eng, wmd) => GetRole<DefenderCornerRole1>(ShooterID).Run(eng, wmd, ShooterID, defenderPos, defenderAng);
                    }
                }
            }
            else if (CurrentState == (int)State.Move)
            {
                if (Mode == 0 || Mode == 1 || Mode == 2)
                {
                    Planner.ChangeDefaulteParams(PasserId, false);
                    Planner.SetParameter(PasserId, 2.5, 2);
                    Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);

                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(ShooterID, defenderPos, defenderAng, PathType.Safe, true, true, true, true);

                    Planner.Add(DefenderID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                if (Mode == 0)
                {

                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(DefenderID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);

                    double dist, DistFromBorder;
                    if ((Model.BallState.Speed.Size < 0.2 || Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.7) || GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                        goActive = true;
                    if (goActive)
                    {
                        getBallSkill.OutGoingBackTrack(engine, Model, ShooterID, ShootTarget);
                        double kick = 0;
                        if (Math.Abs(Vector2D.AngleBetweenInDegrees(ShootTarget - Model.BallState.Location, Vector2D.FromAngleSize(Model.OurRobots[ShooterID].Angle.Value * Math.PI / 180, 1))) < 5)
                            kick = Program.MaxKickSpeed;
                        Planner.AddKick(ShooterID, kickPowerType.Speed, false, kick);
                    }

                    if (passed)
                    {

                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                            Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, defenderPos, defenderAng);
                    }
                }
                else if (Mode == 1)
                {
                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                    if (sync.InRotate)
                        Planner.Add(ShooterID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, DefenderID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, DefenderID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);

                    if (Model.OurRobots[ShooterID].Location.X < 0)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID0, typeof(DefenderCornerRole1)))
                            Functions[PositionerID0] = (eng, wmd) => GetRole<DefenderCornerRole1>(PositionerID0).Run(eng, wmd, PositionerID0, defenderPos, defenderAng);
                    }

                }
                else if (Mode == 2)
                {
                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(DefenderID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);

                    double dist, DistFromBorder;
                    if ((Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.7) || GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                        goActive = true;
                    if (goActive)
                    {
                        getBallSkill.OutGoingBackTrack(engine, Model, ShooterID, ShootTarget);
                        double kick = 0;
                        if (Math.Abs(Vector2D.AngleBetweenInDegrees(ShootTarget - Model.BallState.Location, Vector2D.FromAngleSize(Model.OurRobots[ShooterID].Angle.Value * Math.PI / 180, 1))) < 5)
                            kick = Program.MaxKickSpeed;
                        Planner.AddKick(ShooterID, kickPowerType.Speed, false, kick);
                    }

                    if (passed)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                            Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, defenderPos, defenderAng);
                    }
                }
                else if (Mode == 3)
                {
                    Planner.Add(PositionerID2, PositionerPos2, (ShootTarget - PositionerPos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);

                    if (passed)
                    {
                        if (Model.BallState.Location.X > 0)
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserId, typeof(DefenderCornerRole1)))
                                Functions[PasserId] = (eng, wmd) => GetRole<DefenderCornerRole1>(PasserId).Run(eng, wmd, PasserId, defenderPos, defenderAng);
                        }
                        else
                        {
                            Planner.Add(PasserId, new Position2D(1, Math.Sign(firstBallPos.Y) * 2.7), 180, PathType.UnSafe, true, true, true, true);
                        }
                    }
                }
                //if (passed)
                //{
                //    Planner.Add(PasserId, new Position2D(0.5, 0), 180, PathType.Safe, true, true, true, true);
                //}
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, goaliPos, goaliAng, new DefenceInfo(), defenderPos, DefenderID, true);

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        enum State
        {
            First,
            Move,
            Go,
            Finish
        }
    }
}
