using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;

namespace MRL.SSL.AIConsole.Strategies
{
    class Counfusing : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2, passSpeedTresh = StaticVariables.PassSpeedTresh;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 30, finishTresh = 200, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        bool first;
        bool isSecondDefender;
        int PasserID, DefenderID, ShooterID, Poser1ID, Poser2ID, Poser3ID;
        Position2D firstBallPos, secondBallPos;
        bool passed;
        Position2D PasserPos;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        Position2D Pos1, Pos2, Pos3, Pos4, Pos5, Pos6, Pos7, Pos8, Pos9, Pos10, Pos11, Pos12, center;
        bool firstInState;
        Position2D DefenderPos, DefenderPos1, GoaliePos, ShooterPos;
        Position2D PassTarget, SecondPassTarget;
        Position2D ShootTarget;
        double PasserAngle, DefenderAng, DefenderAng1, GoalieAng, Poser1Ang, Poser2Ang, Poser3Ang, ShooterAng;
        bool inRotate;
        int rotateCounter;
        bool isChip, chipOrigin;
        double PassSpeed;
        bool inPassState;
        bool backSensor;
        double KickSpeed;
        int Mode;

        bool flag = false;

        public override void ResetState()
        {
            Mode = 0;
            first = true;
            firstInState = true;
            isSecondDefender = true;
            PasserID = -1;
            DefenderID = -1;
            ShooterID = -1;
            Poser1ID = -1;
            Poser2ID = -1;
            Poser3ID = -1;
            firstBallPos = Position2D.Zero;
            passed = false;
            timeLimitCounter = 0;
            PasserPos = Position2D.Zero;
            Pos1 = Position2D.Zero;
            Pos2 = Position2D.Zero;
            Pos3 = Position2D.Zero;
            DefenderPos = Position2D.Zero;
            DefenderPos1 = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = Position2D.Zero;
            GoaliePos = Position2D.Zero;
            ShooterPos = Position2D.Zero;

            DefenderAng = 0;
            DefenderAng1 = 0;
            GoalieAng = 0;
            Poser1Ang = 0;
            Poser2Ang = 0;
            Poser3Ang = 0;
            ShooterAng = 0;
            PasserAngle = 0;

            RotateDelay = 60;
            inRotate = false;
            rotateCounter = 2;
            isChip = false;
            chipOrigin = false;
            PassSpeed = 0;
            backSensor = true;
            inPassState = false;
            KickSpeed = 8;
            SecondPassTarget = Position2D.Zero;
            secondBallPos = Position2D.Zero;
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
            StrategyName = "Counfusing";
            AttendanceSize = 5;
            About = "it's just counfusing";
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

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                AssignIDs(engine, Model, Attendance);
                first = false;
            }
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if ((PasserID != -1 && !Model.OurRobots.ContainsKey(PasserID)) || (ShooterID != -1 && !Model.OurRobots.ContainsKey(ShooterID))
                || (Poser1ID != -1 && !Model.OurRobots.ContainsKey(Poser1ID)) || !Model.OurRobots.ContainsKey(Poser2ID) || !Model.OurRobots.ContainsKey(Poser3ID))
                return;

            #region State
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[Poser1ID].Location.DistanceFrom(DefenderPos1) < 0.1
                    && Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < 0.1
                    && Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < 0.1
                    && Model.OurRobots[DefenderID].Location.DistanceFrom(DefenderPos) < 0.1)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.FirstPass;
                    isSecondDefender = false;
                    firstInState = true;
                    timeLimitCounter = 0;
                    counter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    faildCounter++;
                else
                    faildCounter = Math.Max(faildCounter - 2, 0);
                if (faildCounter > 4)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.FirstPass)
            {////////////////////////////////////////////////////////////////
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh + wait2Tresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if (sync.CatchState == 1)
                    {

                    }
                    else
                    {
                        if ((v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[Poser1ID].Location) > faildBallDistSecondPass) ||
                            (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[Poser1ID].Location) <= faildBallDistSecondPass))
                        {
                            faildCounter++;
                            if (faildCounter > faildMaxCounter)
                                CurrentState = (int)State.Finish;
                        }
                        else
                            faildCounter = Math.Max(0, faildCounter - 5);
                    }


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

                if (sync.CatchState == (int)CatchAndRotateBallRole.State.Rotate && ++counter > wait2Tresh)
                {
                    counter = 0;
                    CurrentState = (int)State.SecondPass;
                    firstInState = true;
                    finishCounter = 0;
                    secondBallPos = Model.BallState.Location;
                }
            }///////////////////////////////////////
            else if (CurrentState == (int)State.SecondPass)
            {

                if (passed)
                    finishCounter++;
                if (finishCounter > 100 + wait2Tresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = SecondPassTarget - secondBallPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed && (v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) <= faildBallDistSecondPass))
                {
                    faildCounter++;
                    if (faildCounter > faildMaxCounter)
                        CurrentState = (int)State.Finish;
                }
                else
                    faildCounter = Math.Max(0, faildCounter - 5);
            }
            #endregion

            if (CurrentState == (int)State.SecondPass || passed)
            {
                if (Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < 0.1)
                {
                    isSecondDefender = true;
                }
            }

            #region Determaind Needs
            CalculateDefenderInfo(engine, Model, out DefenderPos, out DefenderPos1, out GoaliePos, out DefenderAng, out DefenderAng1, out GoalieAng, isSecondDefender);
            double sgn = Math.Sign(firstBallPos.Y);
            if (CurrentState == (int)State.First)
            {
                if (firstInState)
                {
                    ShootTarget = GameParameters.OppGoalCenter;
                    PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                    PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;

                    center = new Position2D(0, 0);
                    Vector2D vec1 = new Vector2D();
                    vec1 = (center - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.5);

                    Vector2D vec2 = new Vector2D();
                    vec2 = (center - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.5);

                    Vector2D vec3 = new Vector2D();
                    vec3 = (center - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.5);

                    Pos10 = new Position2D(0, 1);
                    Pos11 = new Position2D(0, -1);
                    Pos12 = new Position2D(-2, 0);
                    Pos7 = new Position2D(-2.2, 0);



                    sgn = Math.Sign(firstBallPos.Y);


                    if ((Model.BallState.Location.X < 0 && Model.BallState.Location.Y < 0) || (Model.BallState.Location.X > 0 && Model.BallState.Location.Y > 0))
                    {
                        PassTarget = new Position2D(sgn * 3.7, sgn * 2.3);
                        Pos4 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (85 * Math.PI / 180), vec1.Size));
                        Pos5 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (75 * Math.PI / 180), vec1.Size));
                        Pos6 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (15 * Math.PI / 180), vec2.Size));
                        Pos8 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (-15 * Math.PI / 180), vec2.Size));
                        Pos9 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec3.AngleInRadians + (-10 * Math.PI / 180), vec3.Size));
                        SecondPassTarget = new Position2D(sgn * 3.4, -sgn * 2.2);
                    }
                    else if ((Model.BallState.Location.X < 0 && Model.BallState.Location.Y > 0) || (Model.BallState.Location.X > 0 && Model.BallState.Location.Y < 0))
                    {
                        PassTarget = new Position2D(-sgn * 3.7, sgn * 2.3);
                        Pos4 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (-85 * Math.PI / 180), vec1.Size));
                        Pos5 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (-75 * Math.PI / 180), vec1.Size));
                        Pos6 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec2.AngleInRadians + (-15 * Math.PI / 180), vec2.Size));
                        Pos8 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec1.AngleInRadians + (15 * Math.PI / 180), vec1.Size));
                        Pos9 = GameParameters.OppGoalCenter + (Vector2D.FromAngleSize(vec3.AngleInRadians + (10 * Math.PI / 180), vec3.Size));
                        SecondPassTarget = new Position2D(-sgn * 3.4, -sgn * 2.2);

                    }

                    //PassTarget = new Position2D(0.5, 0);


                    // SecondPassTarget = new Position2D(-GameParameters.OurGoalCenter.X / 2, -sgn * Math.Abs(GameParameters.OurLeftCorner.Y) / 2);
                    Pos1.X = 2.73;
                    Pos1.Y = firstBallPos.Y;
                    Pos2.X = 0.48;
                    Pos2.Y = -sgn * 2.5;
                    Pos3.X = -2.8;
                    Pos3.Y = sgn * 0.9;
                    Pos3 = GameParameters.OppGoalCenter + (Pos3 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-Pos3, Vector2D.Zero, 0), margin));

                    firstInState = false;
                }

            }
            else if (CurrentState == (int)State.FirstPass)
            {
                if (firstInState)
                {
                    Planner.SetReCalculateTeta(PasserID, true);
                    firstInState = false;
                    //ShooterID = Poser1ID;
                    //Poser1ID = -1;
                    Circle c = new Circle(Model.BallState.Location, 0.9);


                    //double minDist = double.MaxValue; int oppid = -1;
                    //foreach (var item in Model.Opponents.Keys)
                    //{
                    //    if (c.IsInCircle(Model.Opponents[item].Location) && Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                    //    {
                    //        oppid = item;
                    //        minDist = Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location);
                    //    }
                    //}
                    //if (oppid == -1)
                    //{
                    //    Pos2 = new Position2D(Model.BallState.Location.X - .3, Model.BallState.Location.Y + (-1 * Math.Sign(Model.BallState.Location.Y) * .4));
                    //    Poser2Ang = 180;
                    //}
                    //else
                    //{
                    //    Vector2D ballOppPerp = (Model.Opponents[oppid].Location - Model.BallState.Location).GetPerp();
                    //    if (Model.BallState.Location.Y < 0)
                    //        ballOppPerp *= -1;
                    //    Pos2 = Model.Opponents[oppid].Location + ballOppPerp.GetNormalizeToCopy(0.15);
                    //    Poser2Ang = (Model.Opponents[oppid].Location - Pos2).AngleInDegrees;
                    //}
                }

                if (inPassState && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed)
                {
                    isChip = chipOrigin;
                    if (!chipOrigin)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                        isChip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);
                    }

                    //  PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, true, false);
                    //if (isChip)
                    //PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * 0.3; we can use this one
                    PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.7;//it should be change 
                    //Console.WriteLine("PassSpeed :" + PassSpeed / GamePlannerInfo.DirectCoef[PasserID]);
                }
                else
                {
                    //CharterData.AddData("passspeedqueiroz", Model.BallState.Speed.Size);
                }

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    //int tmp = DefenderID;
                    //DefenderID = PasserID;
                    //PasserID = ShooterID;
                    //ShooterID = Poser2ID;
                    //Poser2ID = -1;

                    passed = false;
                    inPassState = false;
                    firstInState = false;
                }
                if (sync.CatchKicked && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed)
                {
                    isChip = chipOrigin = true;

                    PassSpeed = 6;// engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, isChip, true);
                    if (isChip)
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.6;
                    // PassSpeed = Math.Max(1.5, Model.BallState.Location.DistanceFrom(SecondPassTarget) * 0.6);
                }
            }
            #endregion


            //-----------------------------------------------------------------------------------------------
            if (CurrentState == (int)State.SecondPass || passed)
            {
                // Pos2 = new Position2D(-GameParameters.OurGoalCenter.X * 0.5, -sgn * (Math.Abs(GameParameters.OurLeftCorner.Y) / 2));
                Pos2 = SecondPassTarget;
                Pos3.X = 2.73;
                Pos3.Y = firstBallPos.Y;

                Pos1.X = -2.8;
                Pos1.Y = sgn * 0.9;
                Pos1 = GameParameters.OppGoalCenter + (Pos1 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-Pos1, Vector2D.Zero, 0), margin));

                //Pos1 = new Position2D(-0.7, -sgn * 0.2);
                ShooterPos = new Position2D(-GameParameters.OurGoalCenter.X * 0.6, sgn * (Math.Abs(GameParameters.OurLeftCorner.Y) - 0.4));

                //     sgn = Math.Sign(firstBallPos.Y);

                //   ShooterPos = new Position2D(Model.BallState.Location.X, -sgn * 1.5); ;

                //if (DefenderID != -1 && Model.OurRobots[DefenderID].Location.DistanceFrom(Pos1) < 0.3)
                //{
                //    goDefened = true;
                //}


            }

            if (CurrentState == (int)State.SecondPass)
            {
                //if (ShooterID != -1 && (Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < 0.1 || (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.5)))
                //{
                //    goOffend = true;
                //}
            }


            positionDebugDrawing(Pos4, "pos4");
            positionDebugDrawing(Pos5, "pos5");
            positionDebugDrawing(Pos6, "pos6");
            positionDebugDrawing(Pos7, "pos7");
            positionDebugDrawing(Pos8, "pos8");
            positionDebugDrawing(Pos9, "pos9");
            positionDebugDrawing(Pos12, "pos12");
            positionDebugDrawing(PassTarget, "passtarget");
            positionDebugDrawing(SecondPassTarget, "2edpasstarget");



        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3.5, 2);
                if (Planner.AddRotate(Model, PasserID, ShootTarget, 0, kickPowerType.Speed, PassSpeed, isChip, rotateCounter, true).IsInRotateDelay)
                {
                    inRotate = true;
                    rotateCounter++;
                }
                //Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos7, (ShootTarget - Pos7).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser3ID, Pos5, (ShootTarget - Pos5).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser1ID, Pos4, (ShootTarget - Pos4).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(DefenderID, Pos8, (ShootTarget - Pos8).AngleInDegrees, PathType.UnSafe, true, true, true, true);

                // if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser1ID, typeof(DefenderCornerRole2)))
                //    Functions[Poser1ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Poser1ID).Run(eng, wmd, Poser1ID, DefenderPos1, DefenderAng1);
                //Planner.Add(ShooterID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);

            }
            else if (CurrentState == (int)State.FirstPass)
            {

                sync.CatchAndWait = true;
                Planner.Add(Poser2ID, Pos9, (ShootTarget - Pos9).AngleInDegrees, PathType.Safe, true, true, true, true);

                if (Model.OurRobots[Poser1ID].Location.DistanceFrom(Pos12) < 0.1)
                {
                    flag = true;
                }


                if (flag)
                {

                    Planner.Add(Poser2ID, SecondPassTarget, (ShootTarget - ShooterPos).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    //sync.SyncChipCatch(engine, Model, PasserID, PasserPos, Poser1ID, PassTarget, SecondPassTarget, 2, PassSpeed, 30, true, kickPowerType.Speed, backSensor);
                    // if (isChip)
                    //sync.kMotionChipCatch = 3;
                    sync.SyncChipCatch(engine, Model, PasserID, 0, Poser1ID, PassTarget, SecondPassTarget, PassSpeed, 2.5, 60, true, kickPowerType.Speed, backSensor);
                    //else
                    // sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, Poser1ID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, 30);
                }
                else
                {
                    Planner.Add(Poser1ID, Pos12, (ShootTarget - Pos12).AngleInDegrees, PathType.Safe, true, true, true, true);
                }



                //else
                //    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, Poser1ID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay, rotateCounter);
                if (sync.InPassState)
                    inPassState = true;

                Planner.Add(DefenderID, Pos8, (ShootTarget - Pos8).AngleInDegrees, PathType.UnSafe, true, true, true, true);

                if (passed && sync.CatchState == 1)
                {
                    //Planner.Add(DefenderID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                if (passed)
                {
                    Planner.Add(PasserID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                }

                if (isSecondDefender)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser3ID, typeof(DefenderCornerRole2)))
                        Functions[Poser3ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Poser3ID).Run(eng, wmd, Poser3ID, DefenderPos1, DefenderAng1);
                }
                else
                    Planner.Add(Poser3ID, Pos6, (ShootTarget - Pos6).AngleInDegrees, PathType.Safe, true, true, true, true);


            }
            else if (CurrentState == (int)State.SecondPass)
            {


                Planner.Add(DefenderID, Pos10, (ShootTarget - Pos10).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos11, (ShootTarget - Pos11).AngleInDegrees, PathType.Safe, true, true, true, true);

                sync.CatchAndWait = false;
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, Poser1ID, PassTarget, SecondPassTarget, 2.5, 2.3, RotateDelay, true, kickPowerType.Speed, backSensor, rotateCounter);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, Poser1ID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, true, kickPowerType.Speed, RotateDelay, rotateCounter);

                if (Model.OurRobots[DefenderID].Location.DistanceFrom(Pos10) < 0.3 && Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos11) < 0.3)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser3ID, typeof(DefenderCornerRole2)))
                        Functions[Poser3ID] = (eng, wmd) => GetRole<DefenderCornerRole2>(Poser3ID).Run(eng, wmd, Poser3ID, DefenderPos1, DefenderAng1);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                        Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);
                }

                if (passed)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Poser2ID, typeof(ActiveRole)))
                        Functions[Poser2ID] = (eng, wmd) => GetRole<ActiveRole>(Poser2ID).Perform(eng, wmd, Poser2ID, null);
                }
                else
                    Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);

                Planner.Add(PasserID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                //    Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);

            }

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
            //    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng, new DefenceInfo(), DefenderPos, PasserID, true);



            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        private void CalculateDefenderInfo(GameStrategyEngine engine, WorldModel Model, out Position2D defPos, out Position2D defPos1, out Position2D goalipos, out double defAng, out double defAng1, out double goaliang, bool secondDefender = false)
        {
            defPos1 = new Position2D();
            defAng1 = 0;
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
            if (secondDefender)
            {
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(1).Key : id
                });
            }

            var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
            var gol = infos.SingleOrDefault(s => s.RoleType == typeof(GoalieCornerRole));
            var normal1 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole1));
            if (secondDefender)
            {
                var normal2 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole2));
                defPos1 = (normal2.DefenderPosition.HasValue) ? normal2.DefenderPosition.Value : Position2D.Zero;
                defAng1 = normal2.Teta;
            }
            goalipos = (gol.DefenderPosition.HasValue) ? gol.DefenderPosition.Value : Position2D.Zero;
            goaliang = gol.Teta;
            defPos = (normal1.DefenderPosition.HasValue) ? normal1.DefenderPosition.Value : Position2D.Zero;
            defAng = normal1.Teta;

        }
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }
        private void AssignIDs(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> att)
        {
            var tmpIds = RemoveGoaliID(Model, att);
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
            PasserID = minIdx;
            tmpIds.Remove(minIdx);

            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && -Model.OurRobots[item].Location.X < minDist)
                {
                    minDist = -Model.OurRobots[item].Location.X;
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            DefenderID = minIdx;
            tmpIds.Remove(minIdx);

            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X < minDist)
                {
                    minDist = Model.OurRobots[item].Location.X;
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Poser3ID = minIdx;
            tmpIds.Remove(minIdx);

            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X < minDist)
                {
                    minDist = Model.OurRobots[item].Location.X;
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Poser2ID = minIdx;
            tmpIds.Remove(minIdx);
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            Poser1ID = tmpIds[0];
            ShooterID = -1;
        }

        enum State
        {
            First,
            FirstPass,
            SecondPass,
            Finish
        }

        public void positionDebugDrawing(Position2D pos, string posName)
        {
            DrawingObjects.AddObject(new Circle(pos, 0.2), "b" + pos.toString());
            DrawingObjects.AddObject(new StringDraw(posName.ToString(), pos.Extend(0.25, 0)), "aasdasdb" + pos.toString());
        }
    }
}

//if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserId, typeof(ActiveRole)))
//                   Functions[PasserId] = (eng, wmd) => GetRole<ActiveRole>(PasserId).PerformWithoutKick(eng, wmd, PasserId, shootTarget, false, initDist); // we can use this for going behinde the ball whiteout kike