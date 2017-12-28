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
    public class PassShootMiddle5Strategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 10, finishTresh = 100, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3,
            faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, mode2FinishTresh = 30;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        bool inrot = false, inPassState = false;
        bool first, passTargetCalculated, Debug = false, nearShooter, shooted;
        int PasserId, PickerID, PositionerID0, ShooterID, PositionerID1;
        Position2D PasserPos, PickerPos, PositionerPos1, PassTarget, ShootTarget, PositionerPos0, ShooterPos, firstBallPos, lastPositioner1Pos;
        Position2D tmpPos0;
        Position2D goaliPos, defenderPos;
        double goaliAng, defenderAng;
        double PasserAngle, PickerAngle, PositionerAngle1, RotateTeta, PassSpeed, KickPower, tmpAng0;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        int Mode;
        bool chip, passed, chipOrigin, goOneTouch;
        bool backSensor;
        int ShooterAngle;
        bool alert = true;

        public override void ResetState()
        {
            ShooterAngle = 0;
            tmpPos0 = Position2D.Zero;
            tmpAng0 = 0;
            backSensor = true;
            Mode = (int)StrategyMode.WithPicker;
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
            PickerID = -1;
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
            StrategyName = "PassShootMiddle5rStrategy";
            AttendanceSize = 6;
            About = "this strategy is passShoot with pick in midd line!";
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
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region Ids
            if (first)
            {
                var tmpIds = RemoveGoaliID(Model, Attendance);
                double minDist = double.MaxValue;
                int minIdx = -1;
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
                PickerID = minIdx;
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
                PositionerID1 = minIdx;
                tmpIds.Remove(minIdx);
                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (!Model.OurRobots.ContainsKey(PasserId) || !Model.OurRobots.ContainsKey(PositionerID0)
                || !Model.OurRobots.ContainsKey(PositionerID1) || !Model.OurRobots.ContainsKey(PickerID)
                || !Model.OurRobots.ContainsKey(ShooterID))
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
                //TODO: Check Positioner1ID
                if (Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh && Model.OurRobots[PickerID].Location.DistanceFrom(PickerPos) < tresh && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < 0.23
                    && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < tresh && (!Model.GoalieID.HasValue || !Model.OurRobots.ContainsKey(Model.GoalieID.Value) || Model.OurRobots[Model.GoalieID.Value].Location.DistanceFrom(goaliPos) < tresh))
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    counter = 0;
                    timeLimitCounter = 0;
                    if (Mode == 0 || Mode == 2)
                        CurrentState = (int)State.Pick;
                    else if (Mode == 1)
                        CurrentState = (int)State.Go;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Pick)
            {

                if (Model.OurRobots[PickerID].Location.DistanceFrom(PickerPos) < 0.1)
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

                if (Mode == 2 && finishCounter > mode2FinishTresh)
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

                    //if (alert && PassTarget.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.4)
                    //{
                    //    CurrentState = (int)State.Finish;
                    //}

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
                if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[ShooterID].Location - Model.OurRobots[PasserId].Location) <= 0)
                    shooted = true;
                if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > 0.35)
                    CurrentState = (int)State.Finish;
            }
            #endregion
            CalculateDefenderInfo(engine, Model, out defenderPos, out goaliPos, out defenderAng, out goaliAng);
            #region PosesAndAngles
            if (CurrentState == (int)State.First)
            {
                ShootTarget = GameParameters.OppGoalCenter;
                PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                if (Mode == 0 || Mode == 2)
                {
                    PositionerPos1 = new Position2D(PasserPos.X + 0.8, PasserPos.Y);//+ new Vector2D(1, 0).GetNormalizeToCopy(passerShooterDist);
                    //  ShooterPos = new Position2D(2,1.6);//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);
                    PositionerPos1.X = Math.Sign(PositionerPos1.X) * Math.Abs(Math.Min(GameParameters.OurGoalCenter.X - 0.2, Math.Abs(PositionerPos1.X)));
                    PositionerPos1.Y = Math.Sign(PositionerPos1.Y) * Math.Abs(Math.Min(Math.Abs(GameParameters.OurLeftCorner.Y) - 0.2, Math.Abs(PositionerPos1.Y)));
                    PositionerPos1.X = Math.Max(0.5, PositionerPos1.X);
                    PositionerAngle1 = 180;

                    ShooterPos = new Position2D(-1.5, -Math.Sign(Model.BallState.Location.Y) * 0.42);
                    PickerPos = new Position2D(2, 0.3 * Math.Sign(Model.BallState.Location.Y));//for middle freekick new Position2D(.5, -1.3);//fotcornell: new Position2D(-1.6, 1.3);//new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y) + new Vector2D(-1, 0).GetNormalizeToCopy(3);

                    PositionerPos0 = new Position2D(-2.23, -1.24 * Math.Sign(Model.BallState.Location.Y));
                    PositionerPos0 = GameParameters.OppGoalCenter + (PositionerPos0 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-PositionerPos0, Vector2D.Zero, 0), margin));

                }


                inrot = false;
            }
            else if (CurrentState == (int)State.Pick)
            {
                if (Mode == 0 || Mode == 2)
                {
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
                        PickerPos = new Position2D(Model.BallState.Location.X - .3, Model.BallState.Location.Y + (-1 * Math.Sign(Model.BallState.Location.Y) * .4));
                        PickerAngle = 180;
                    }
                    else
                    {
                        Vector2D ballOppPerp = (Model.Opponents[oppid].Location - Model.BallState.Location).GetPerp();
                        if (Model.BallState.Location.Y < 0)
                            ballOppPerp *= -1;
                        PickerPos = Model.Opponents[oppid].Location + ballOppPerp.GetNormalizeToCopy(0.15);
                        PickerAngle = (Model.Opponents[oppid].Location - PickerPos).AngleInDegrees;
                    }
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                double width = 1, heigth = 1, step = 0.2;
                Position2D topLeft = new Position2D();

                if (!passTargetCalculated)
                {
                    if (Model.BallState.Location.X < -2.2)
                        chipOrigin = true;
                    if (chipOrigin)
                        RotateTeta = 0;

                    if (Mode == 0 || Mode == 2)
                    {
                        ShootTarget = GameParameters.OppGoalCenter;//(Model.BallState.Location.Y >= 0) ? GameParameters.OppGoalRight : GameParameters.OppGoalLeft;
                        width = 0.9; heigth = 0.9; step = 0.3;

                        if (Model.BallState.Location.X < -2)
                            margin = 0.8;
                        topLeft = new Position2D(Model.BallState.Location.X + width + margin, ((Model.BallState.Location.Y > 0) ? -(1 + heigth) : 1));

                        chip = chipOrigin;
                        PassTarget = new Position2D(-0.6, Math.Sign(Model.BallState.Location.Y) * 0.1);
                        //bool b = engine.GameInfo.BestPassPoint(Model, ShootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref PassTarget);
                        //chipOrigin = chip;
                    }
                    PositionerPos1 = new Position2D(-1.3, -Math.Sign(Model.BallState.Location.Y) * 1.5);
                    passTargetCalculated = true;
                }
                if (sync.InPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > BallMovedTresh)
                    passed = true;
                if (Mode == (int)StrategyMode.PickerIsActive && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > BallMovedTresh)
                {
                    passed = true;
                }
                //if (alert)
                //{
                //    chipOrigin = true;
                //}
                chip = chipOrigin;
                if (!passed && !chipOrigin)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { ShooterID, PasserId }, null);
                    chip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);

                }
                if (!passed)
                {
                    //if (alert)
                    //{
                    //    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, chip, true) * 2;
                    //}
                    //else
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserId, Model.BallState.Location, PassTarget, chip, true) * 2;
                }

            }
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            if (CurrentState == (int)State.First)
            {
                if (Mode == 0 || Mode == 2)
                {
                    Planner.ChangeDefaulteParams(PasserId, false);
                    Planner.SetParameter(PasserId, 2.5, 2);
                    Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);

                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PickerID, PickerPos, (ShootTarget - PickerPos).AngleInDegrees, PathType.Safe, true, true, true, true);

                    Planner.Add(ShooterID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.Pick)
            {
                if (Mode == 0 || Mode == 2)
                {
                    Planner.ChangeDefaulteParams(PasserId, false);
                    Planner.SetParameter(PasserId, 2.5, 2);
                    Planner.Add(PasserId, PasserPos, PasserAngle, PathType.UnSafe, true, true, true, true);

                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PickerID, PickerPos, (ShootTarget - PickerPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(ShooterID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.Go)
            {
                if (Mode == 0)
                {

                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PickerID, PickerPos, (ShootTarget - PickerPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                    if (!chip)
                        sync.SyncDirectPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay);
                    else
                        sync.SyncChipPass(engine, Model, PasserId, RotateTeta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);

                    if (passed)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserId, typeof(DefenderCornerRole1)))
                            Functions[PasserId] = (eng, wmd) => GetRole<DefenderCornerRole1>(PasserId).Run(eng, wmd, PasserId, defenderPos, defenderAng);
                    }


                }
                else if (Mode == 2)
                {
                    Planner.Add(PositionerID0, PositionerPos0, (ShootTarget - PositionerPos0).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PickerID, PickerPos, (ShootTarget - PickerPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, PositionerPos1, (ShootTarget - PositionerPos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                    Position2D tmpTarget = Model.BallState.Location + (GameParameters.OppGoalCenter - Model.BallState.Location).GetNormalizeToCopy(2);
                    double tmpPassSpeed = 1.5;
                    Planner.AddRotate(Model, PasserId, tmpTarget, 0, kickPowerType.Speed, tmpPassSpeed, true);

                    DrawingObjects.AddObject(new Circle(tmpTarget, 0.1, new System.Drawing.Pen(System.Drawing.Color.DeepPink, 0.01f)), "fadsfdsazf");
                    DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new System.Drawing.Pen(System.Drawing.Color.Yellow, 0.01f)), "fadsfdsazdsff");

                    if (passed)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PickerID, typeof(ActiveRole)))
                            Functions[PickerID] = (eng, wmd) => GetRole<ActiveRole>(PickerID).Perform(eng, wmd, PickerID, null);

                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserId, typeof(DefenderCornerRole1)))
                            Functions[PasserId] = (eng, wmd) => GetRole<DefenderCornerRole1>(PasserId).Run(eng, wmd, PasserId, defenderPos, defenderAng);
                    }
                }
            }
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, goaliPos, goaliAng, new DefenceInfo(), defenderPos, PasserId, true);

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        enum State
        {
            First,
            Pick,
            Go,
            Finish
        }

        enum StrategyMode
        {
            WithPicker = 0,
            NoPicker = 1,
            PickerIsActive = 2
        }
    }
}
