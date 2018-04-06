using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Strategies
{
    class ChangingPositionerMiddle : StrategyBase
    {
        #region Param
        Syncronizer sync;
        const double initDist = 0.15, tresh = 0.01, waitTresh = 20, finishTresh = 500, maxWaitTresh = 300, faildMaxCounter = 6, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.3, faildFarPassSpeedTresh = 0.3, faildBallDistSecondPass = 0.3, faildNearPassSpeedTresh = -0.03;
        bool first = true, inPassState = false, Debug = true;
        bool passed, passerFirsFlag, chipOrigin;
        int PasserID, ShooterID, PositionerID0, PositionerID1;
        int counter;
        double passerAngle, PassSpeed, shooterAngle, RotateTeta, GoalieAng, DefenderAng, DefenderAng2;
        int timeLimitCounter, finishCounter, faildCounter;
        Position2D firstBallPos, passerPos, shooterPos, passTarget, shootTarget, passerFirstPos, Positioner0Pos, Positioner0SecPos, Positioner1SecPos, Positioner1Pos, positioner1FirstPos, positioner0FirstPos, DefenderPos, GoaliePos, DefenderPos2;
        /// <summary>
        /// mode 0=> Direct shoot, mode 1=> Chip Shoot, mode 2 => From Middle 
        /// </summary>
        int mode;

        #endregion

        public override void ResetState()
        {
            mode = 2;
            PasserID = -1;
            ShooterID = -1;
            PositionerID0 = -1;
            PositionerID1 = -1;
            passerAngle = 0;
            RotateTeta = 90;
            shooterAngle = 0;
            timeLimitCounter = 0;
            faildCounter = 0;
            finishCounter = 0;
            DefenderAng2 = 0;
            counter = 0;
            PassSpeed = 8;
            GoalieAng = 0;
            DefenderAng = 0;
            firstBallPos = Position2D.Zero;
            DefenderPos2 = Position2D.Zero;
            DefenderPos = Position2D.Zero;
            GoaliePos = Position2D.Zero;
            passerFirstPos = Position2D.Zero;
            shooterPos = Position2D.Zero;
            passerPos = Position2D.Zero;
            passTarget = Position2D.Zero;
            shootTarget = Position2D.Zero;
            Positioner0Pos = Position2D.Zero;
            Positioner0SecPos = Position2D.Zero;
            Positioner1Pos = Position2D.Zero;
            Positioner1SecPos = Position2D.Zero;
            passerFirsFlag = true;
            chipOrigin = false;
            inPassState = false;
            first = true;
            passed = false;
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
            FinalState = 2;
            TrapState = 2;
        }

        public override void FillInformation()
        {
            UseInMiddle = false;
            UseOnlyInMiddle = false;
            StrategyName = "CP Middle";
            AttendanceSize = 4;
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
        private void CalculateDefenderInfo(GameStrategyEngine engine, WorldModel Model, out Position2D defPos, out Position2D goalipos, out double defAng, out double goaliang)//,out Position2D defPos2, out double defAng2)
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
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole2),
                OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(1).Key : id
            });
            var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
            var gol = infos.SingleOrDefault(s => s.RoleType == typeof(GoalieCornerRole));
            var normal2 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole2));
            goalipos = (gol.DefenderPosition.HasValue) ? gol.DefenderPosition.Value : Position2D.Zero;
            goaliang = gol.Teta;
            defPos = (normal2.DefenderPosition.HasValue) ? normal2.DefenderPosition.Value : Position2D.Zero;
            defAng = normal2.Teta;
            //defPos2 = (normal2.DefenderPosition.HasValue) ? normal2.DefenderPosition.Value : Position2D.Zero;
            //defAng2 = normal2.Teta;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {
            if (first)
            {
                firstBallPos = Model.BallState.Location;

                double minDist = double.MaxValue;
                int minIdx = -1;
                double maxDist = double.MinValue;
                int maxIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X > maxDist)
                    {
                        maxDist = Model.OurRobots[item].Location.X;
                        maxIdx = item;
                    }
                }
                if (maxIdx == -1)
                    return;
                ShooterID = maxIdx;


                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in Attendance.Keys.ToList())
                {
                    if (item != ShooterID && Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
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
                    if (item != PasserID && item != ShooterID && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
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
                    if (item != PasserID && item != ShooterID && item != PositionerID0 && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID1 = minIdx;


                first = false;
            }


            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (!Model.OurRobots.ContainsKey(PasserID) || !Model.OurRobots.ContainsKey(ShooterID))
                return;

            CalculateDefenderInfo(engine, Model, out DefenderPos, out GoaliePos, out DefenderAng, out GoalieAng);//,out DefenderPos2,out DefenderAng2);
            if (CurrentState == (int)State.First)
            {
                double dAngle = Model.OurRobots[PasserID].Angle.Value - passerAngle;
                timeLimitCounter++;
                if (dAngle > 180)
                    dAngle -= 360;
                else if (dAngle < -180)
                    dAngle += 360;

                if (Model.OurRobots[PasserID].Location.DistanceFrom(passerPos) < tresh && Model.OurRobots[ShooterID].Location.DistanceFrom(shooterPos) < tresh && Model.OurRobots[PositionerID0].Location.DistanceFrom(Positioner0Pos) < tresh && Model.OurRobots[PositionerID1].Location.DistanceFrom(DefenderPos) < tresh)
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

                Vector2D refrence = passTarget - passerPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if ((v.Y < faildFarPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistSecondPass))
                    {
                        faildCounter++;
                        if (faildCounter > faildMaxCounter)
                        {
                            CurrentState = (int)State.Finish;
                            DrawingObjects.AddObject(new StringDraw("Strategy Failed", Color.DarkRed, new Position2D(1, 1.2)), "jhnhyujjmnjhuhbmjhy");
                        }
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
                        {
                            CurrentState = (int)State.Finish;
                            DrawingObjects.AddObject(new StringDraw("Strategy Failed", Color.DarkRed, new Position2D(1, 1.2)), "jhnhyuuhbmjhy");
                        }
                    }
                    else
                        faildCounter = Math.Max(0, faildCounter - 1);
                }
            }
            #region mode0
            if (mode == 0)
            {
                if (CurrentState == (int)State.First)
                {
                    shootTarget = Position2D.Zero;
                    passerPos = Model.BallState.Location + (Model.BallState.Location - shootTarget).GetNormalizeToCopy(initDist);
                    if (passerFirsFlag)
                    {
                        passerFirstPos = passerPos;
                        passerFirsFlag = false;
                    }
                    passerAngle = (passTarget - Model.OurRobots[PasserID].Location).AngleInDegrees;
                    if (firstBallPos.Y > 0)
                    {
                        shooterPos = Model.BallState.Location.Extend(5, -4.5);
                        Positioner0Pos = GameParameters.OurGoalCenter.Extend(-2, 0);
                        Positioner1Pos = Model.BallState.Location.Extend(4.5, -0.5);
                    }
                    else
                    {
                        shooterPos = Model.BallState.Location.Extend(5, 4.5);
                        Positioner0Pos = Model.BallState.Location.Extend(4.5, 0.5);
                        Positioner1Pos = GameParameters.OurGoalCenter.Extend(-2, 0);
                    }
                    passTarget = Model.BallState.Location.Extend(2, 0);

                }
                else if (CurrentState == (int)State.Go)
                {
                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, passTarget, false, false);
                    if (firstBallPos.Y < 0)
                    {
                        Positioner0SecPos = new Position2D(-2.2, 1.4);
                        Positioner1SecPos = new Position2D(-1.4, 2.57);
                    }
                    else
                    {
                        Positioner0SecPos = new Position2D(-2.4, -1.1);
                        Positioner1SecPos = new Position2D(-1.25, -2.2);
                    }
                    double goodness;
                    var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, passTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    if (GoodPointInGoal.HasValue)
                        shootTarget = GoodPointInGoal.Value;
                    else
                        shootTarget = GameParameters.OppGoalCenter;
                    shooterAngle = (Model.BallState.Location - Model.OurRobots[ShooterID].Location).AngleInDegrees;
                    if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05)
                        passed = true;
                }
            }
            #endregion
            #region mode 1
            if (mode == 1)
            {
                if (CurrentState == (int)State.First)
                {
                    shootTarget = Position2D.Zero;
                    passerPos = Model.BallState.Location + (Model.BallState.Location - shootTarget).GetNormalizeToCopy(initDist);
                    if (passerFirsFlag)
                    {
                        passerFirstPos = passerPos;
                        passerFirsFlag = false;
                    }
                    passerAngle = (passTarget - Model.OurRobots[PasserID].Location).AngleInDegrees;
                    if (firstBallPos.Y > 0)
                    {
                        Positioner0Pos = Model.BallState.Location.Extend(4.9, -4);
                        Positioner1Pos = GameParameters.OurGoalCenter.Extend(-2.5, 0);
                        shooterPos = Model.BallState.Location.Extend(6, -0.5);
                        passTarget = Model.BallState.Location.Extend(0.5, -4);
                    }
                    else
                    {
                        Positioner0Pos = Model.BallState.Location.Extend(4.5, 4);
                        Positioner1Pos = GameParameters.OurGoalCenter.Extend(-2.5, 0);
                        shooterPos = Model.BallState.Location.Extend(6, 0.5);
                        passTarget = Model.BallState.Location.Extend(0.5, 4);
                    }
                }
                else if (CurrentState == (int)State.Go)
                {
                    if (Model.BallState.Location.X < -(GameParameters.OurGoalCenter.X - GameParameters.DefenceAreaHeight))
                        chipOrigin = true;
                    if (chipOrigin)
                        RotateTeta = 0;


                    PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * 0.98;
                    if (firstBallPos.Y > 0)
                    {
                        Positioner0SecPos = new Position2D(-2, 0.93);//Model.BallState.Location.Extend(2, 0);
                        Positioner1SecPos = new Position2D(-2.2, 2.2);//Model.BallState.Location.Extend(1, -2);
                    }
                    else
                    {
                        Positioner0SecPos = new Position2D(-2.5, -1.03);//Model.BallState.Location.Extend(1, 2);
                        Positioner1SecPos = new Position2D(-1.8, -2.5);//Model.BallState.Location.Extend(2, 0);
                    }
                    double goodness;
                    var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, passTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    if (GoodPointInGoal.HasValue)
                        shootTarget = GoodPointInGoal.Value;
                    else
                        shootTarget = GameParameters.OppGoalCenter;
                    shooterAngle = (Model.BallState.Location - Model.OurRobots[ShooterID].Location).AngleInDegrees;
                    if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05)
                        passed = true;
                }
            }
            #endregion
            #region mode2
            if (mode == 2)
            {
                if (CurrentState == (int)State.First)
                {
                    shootTarget = Position2D.Zero;
                    passerPos = Model.BallState.Location + (Model.BallState.Location - shootTarget).GetNormalizeToCopy(initDist);
                    passerAngle = (passTarget - Model.OurRobots[PasserID].Location).AngleInDegrees;
                    if (passerFirsFlag)
                    {
                        passerFirstPos = passerPos;
                        passerFirsFlag = false;
                    }

                    if (Model.BallState.Location.Y < 0)
                    {
                        if (Model.BallState.Location.X > 0)
                        {
                            PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * 0.75;
                            shooterPos = Model.BallState.Location.Extend(2, 0);
                            Positioner0Pos = Model.BallState.Location.Extend(2, 4);
                            passTarget = Model.BallState.Location.Extend(-3, 4);
                        }
                        else
                        {
                            PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * 0.65;
                            shooterPos = Model.BallState.Location.Extend(2, 0);
                            Positioner0Pos = Model.BallState.Location.Extend(2, 4);
                            passTarget = Model.BallState.Location.Extend(-2, 4);
                        }
                    }
                    else
                    {
                        if (Model.BallState.Location.X > 0)
                        {
                            PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * 0.65;
                            shooterPos = Model.BallState.Location.Extend(2, 0);
                            Positioner0Pos = Model.BallState.Location.Extend(2, -4);
                            passTarget = Model.BallState.Location.Extend(-3, -4);
                        }
                        else
                        {
                            PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * 0.6;
                            shooterPos = Model.BallState.Location.Extend(2, 0);
                            Positioner0Pos = Model.BallState.Location.Extend(2, -4);
                            passTarget = Model.BallState.Location.Extend(-2, -4);
                        }
                    }
                }
                else if (CurrentState == (int)State.Go)
                {
                    //PassSpeed *= 0.99;
                    if (firstBallPos.Y < 0)
                    {
                        if (firstBallPos.X > 0)
                        {
                            Positioner0SecPos = firstBallPos.Extend(-3, 0);
                        }
                        else
                        {
                            Positioner0SecPos = firstBallPos.Extend(-2, 0);
                        }
                    }
                    else
                    {
                        if (firstBallPos.X > 0)
                        {
                            Positioner0SecPos = firstBallPos.Extend(-4, 0);
                        }
                        else
                        {
                            Positioner0SecPos = firstBallPos.Extend(-2, 0);
                        }
                    }

                    double goodness;
                    var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, passTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    if (GoodPointInGoal.HasValue)
                        shootTarget = GoodPointInGoal.Value;
                    else
                        shootTarget = GameParameters.OppGoalCenter;

                    shooterAngle = (Model.BallState.Location - Model.OurRobots[ShooterID].Location).AngleInDegrees;
                    if (inPassState && Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05)
                        passed = true;
                }
            }
            #endregion
            if (Debug)
            {
                DrawingObjects.AddObject(new Circle(Model.OurRobots[ShooterID].Location, 0.2, new Pen(Color.Orange, 0.01f)), "dfcghjkiuytre");
                DrawingObjects.AddObject(new Circle(Model.OurRobots[PositionerID0].Location, 0.2, new Pen(Color.Brown, 0.01f)), "b vyt");
                DrawingObjects.AddObject(new Circle(Model.OurRobots[PasserID].Location, 0.2, new Pen(Color.BlueViolet, 0.01f)), "mnewfghj");
                DrawingObjects.AddObject(new Circle(Model.OurRobots[PositionerID1].Location, 0.2, new Pen(Color.CadetBlue, 0.01f)), "mnadffgghgewfghj");
                DrawingObjects.AddObject(new Circle(Positioner0SecPos, 0.2, new Pen(Color.OrangeRed, 0.01f)), "passtarget2");
                DrawingObjects.AddObject(new Circle(Positioner1SecPos, 0.2, new Pen(Color.Red, 0.01f)), "passtarsecget2");
                DrawingObjects.AddObject(new Circle(shooterPos, 0.2, new Pen(Color.Blue, 0.01f)), "shooterpos");
                DrawingObjects.AddObject(new Circle(Positioner0Pos, 0.2, new Pen(Color.Red, 0.01f)), "PositionerPos");
                DrawingObjects.AddObject(new Circle(Positioner1Pos, 0.2, new Pen(Color.DarkViolet, 0.01f)), "pasqw0starget");
                DrawingObjects.AddObject(new Circle(passTarget, 0.2, new Pen(Color.Yellow, 0.01f)), "passtarget");
                DrawingObjects.AddObject(new Circle(Positioner0SecPos, 0.2, new Pen(Color.Black, 0.01f)), "positioner0www");
                DrawingObjects.AddObject(new Circle(shooterPos, 0.2, new Pen(Color.Blue, 0.01f)), "shooteeerpos");
                DrawingObjects.AddObject(new Circle(passTarget, 0.2, new Pen(Color.Yellow, 0.01f)), "passtargqqqqqet");
                DrawingObjects.AddObject(new Circle(passTarget, 0.2, new Pen(Color.Yellow, 0.01f)), "ccccc");

            }
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            if (mode == 0)
            {
                if (CurrentState == (int)State.First)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                        Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(eng, wmd, PasserID, shootTarget, false, initDist);
                    Planner.Add(PositionerID0, Positioner0Pos, (Model.OurRobots[PasserID].Location - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(ShooterID, shooterPos, (GameParameters.OppGoalCenter - Model.OurRobots[ShooterID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, Positioner1Pos, (Model.OurRobots[PasserID].Location - Model.OurRobots[PositionerID1].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else if (CurrentState == (int)State.Go)
                {
                    Planner.Add(PositionerID1, Positioner1SecPos, (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID1].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID0, Positioner0SecPos, (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    //if (Model.OurRobots[PositionerID0].Location.DistanceFrom(Positioner0Pos) > 0.05 && Model.OurRobots[PositionerID1].Location.DistanceFrom(Positioner1Pos) > 0.05)
                    //{
                    sync.SyncDirectPass(engine, Model, PasserID, 60, ShooterID, passTarget, shootTarget, PassSpeed, 8, 60);
                    //}
                    if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.15)
                    {
                        Planner.Add(PasserID, passerFirstPos, (GameParameters.OppGoalCenter - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                        Planner.Add(PositionerID0, GameParameters.OppGoalCenter + new Vector2D(1, -1), (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                        Planner.Add(PositionerID1, GameParameters.OppGoalCenter + new Vector2D(1, -1.5), (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID1].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                }
                inPassState = sync.InPassState;
            }
            if (mode == 1)
            {
                if (CurrentState == (int)State.First)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                        Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(eng, wmd, PasserID, shootTarget, false, initDist);
                    Planner.Add(PositionerID0, Positioner0Pos, (Model.OurRobots[PasserID].Location - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID1, Positioner1Pos, (Model.OurRobots[PasserID].Location - Model.OurRobots[PositionerID1].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(ShooterID, shooterPos, (GameParameters.OppGoalCenter - Model.OurRobots[ShooterID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else if (CurrentState == (int)State.Go)
                {
                    Planner.Add(PositionerID1, Positioner1SecPos, (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID1].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(PositionerID0, Positioner0SecPos, (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);

                    //if (Model.OurRobots[PositionerID0].Location.DistanceFrom(Positioner0Pos) > 0.05 && Model.OurRobots[PositionerID1].Location.DistanceFrom(Positioner1Pos) > 0.05)
                    //{
                    sync.SyncChipPass(engine, Model, PasserID, 60, ShooterID, passTarget, shootTarget, PassSpeed, 8, 60, false);
                    //}
                    // Planner.Add(PositionerID0, Positioner0SecPos, (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.15)
                    {
                        Planner.Add(PasserID, passerFirstPos, (GameParameters.OppGoalCenter - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                        Planner.Add(PositionerID0, GameParameters.OppGoalCenter.Extend(1, -1), (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                        Planner.Add(PositionerID1, GameParameters.OppGoalCenter.Extend(1, -1.5), (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID1].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                }
                inPassState = sync.InPassState;
            }
            if (mode == 2)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PositionerID1, typeof(DefenderCornerRole2)))
                    Functions[PositionerID1] = (eng, wmd) => GetRole<DefenderCornerRole2>(PasserID).Run(eng, wmd, PositionerID1, DefenderPos, DefenderAng);

                if (CurrentState == (int)State.First)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                        Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(eng, wmd, PasserID, shootTarget, false, initDist);
                    Planner.Add(PositionerID0, Positioner0Pos, (Model.OurRobots[PasserID].Location - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    Planner.Add(ShooterID, shooterPos, (GameParameters.OppGoalCenter - Model.OurRobots[ShooterID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else if (CurrentState == (int)State.Go)
                {
                    sync.SyncChipPass(engine, Model, PasserID, 60, ShooterID, passTarget, shootTarget, PassSpeed, 8, 60, false);
                    if (Model.OurRobots[ShooterID].Location.DistanceFrom(shooterPos) > 0.1)
                    {
                        Planner.Add(PositionerID0, Positioner0SecPos, (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                    if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > 0.15)
                    {
                        Planner.Add(PasserID, passerFirstPos, (GameParameters.OppGoalCenter - Model.OurRobots[PasserID].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                        Planner.Add(PositionerID0, GameParameters.OppGoalCenter.Extend(1, -1), (GameParameters.OppGoalCenter - Model.OurRobots[PositionerID0].Location).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                }
                inPassState = sync.InPassState;
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
