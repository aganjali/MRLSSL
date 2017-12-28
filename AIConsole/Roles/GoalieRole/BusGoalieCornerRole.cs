#region old goali
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.AIConsole.Engine;
//using MRL.SSL.GameDefinitions;
//using MRL.SSL.Planning.GamePlanner.Types;
//using MRL.SSL.AIConsole.Skills;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.AIConsole.Skills.GoalieSkills;
//using MRL.SSL.Planning.MotionPlanner;
//using System.Drawing;

//namespace MRL.SSL.AIConsole.Roles
//{
//    public class BusGoalieCornerRole : RoleBase, IGoalie
//    {
//        public static Position2D RobotLoc = new Position2D();
//        private Position2D ballSavedForStates = new Position2D();
//        public static Position2D ballSavedPos = new Position2D();
//        public static bool firstTimeBallInChipKick = true;
//        List<Position2D> Ball = new List<Position2D>();
//        Position2D ballInitialState = new Position2D();
//        List<double> robotAgles = new List<double>();
//        private bool WeHadBallInStartOfchip = false;
//        public static bool ballinRoll = false;
//        Position2D Target = new Position2D();
//        static int CurrentChipState = 0;
//        int CounterInChip = 0;
//        bool firsttime = true;
//        bool debug = false;
//        List<double> angles = new List<double>();
//        int currentState;
//        private bool onceaTime = true;
//        private double initialAngle = 0;
//        private int frameOfChipInEllipseEnvironment = 4;
//        private bool falled = false;
//        private bool onceaTime2 = true;
//        Position2D lastTarget = new Position2D();
//        double angle = 0;
//        private bool onceaTime3 = true;
//        private bool onceaTime4 = true;
//        private int status = 2;
//        private bool onceaTime5 = true;
//        int mainRobot = 0;
//        bool weHadFullSupport = true;
//        bool goToPrependicular = true;
//        bool gofront = true;
//        public SingleObjectState ballState = new SingleObjectState();
//        public SingleObjectState ballStateFast = new SingleObjectState();
//        double staticAngle = 0;
//        private bool onceaTime6 = true;
//        int counterISChipStart = 0;//WC2017
//        bool cut = false;//WC2017
//        Position2D lastPosNormal = Position2D.Zero;//WC2017
//        double ballcoeff;
//        double robotCoeff;
//        Position2D intersectTime;
//        Position2D lastBallPosOutOfDangerZone = new Position2D();
//        Position2D initialpos = Position2D.Zero;

//        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta, DefenceInfo gol, bool strategyGoalie, bool ballismoved, Dictionary<int, RoleBase> AssignedRoles)
//        {
//            TargetPos = new Position2D(Math.Min(TargetPos.X, GameParameters.OurGoalCenter.X - .1), TargetPos.Y);
//            angle = -Model.BallState.Speed.AngleInDegrees;

//            ballState = Model.BallState;
//            ballStateFast = Model.BallStateFast;

//            //DefenceInfo inf = null;
//            bool busInfoHaseValue = false;
//            Position2D DefencePos = new Position2D();
//            int? busDefenderID = null;
//            //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(DefenderNormalRole1)))
//            //    inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(DefenderNormalRole1)).First();
//            BusRole1 bus1 = new BusRole1();
//            BusRole2 bus2 = new BusRole2();
//            BusRole3 bus3 = new BusRole3();
//            BusRole4 bus4 = new BusRole4();
//            foreach (var item in AssignedRoles)
//            {
//                if (item.Value.GetType() == typeof(BusRole1))
//                {
//                    bus1 = (BusRole1)item.Value;
//                    if (bus1.OppID.HasValue && Model.Opponents.ContainsKey(bus1.OppID.Value) && bus1.OppID.Value == gol.OppID.Value)
//                    {
//                        DefencePos = (bus1.overlaptarget.HasValue ? bus1.overlaptarget.Value : bus1.target);
//                        busDefenderID = item.Key;
//                        busInfoHaseValue = true;
//                        break;
//                    }
//                }
//                if (item.Value.GetType() == typeof(BusRole2))
//                {
//                    bus2 = (BusRole2)item.Value;
//                    if (bus2.OppID.HasValue && Model.Opponents.ContainsKey(bus2.OppID.Value) && bus2.OppID.Value == gol.OppID.Value)
//                    {
//                        DefencePos = (bus2.overlaptarget.HasValue ? bus2.overlaptarget.Value : bus2.target);
//                        busDefenderID = item.Key;
//                        busInfoHaseValue = true;
//                        break;
//                    }
//                }
//                if (item.Value.GetType() == typeof(BusRole3))
//                {
//                    bus3 = (BusRole3)item.Value;
//                    if (bus3.OppID.HasValue && Model.Opponents.ContainsKey(bus3.OppID.Value) && bus3.OppID.Value == gol.OppID.Value)
//                    {
//                        DefencePos = (bus3.overlaptarget.HasValue ? bus3.overlaptarget.Value : bus3.target);
//                        busDefenderID = item.Key;
//                        busInfoHaseValue = true;
//                        break;
//                    }
//                }
//                if (item.Value.GetType() == typeof(BusRole4))
//                {
//                    bus4 = (BusRole4)item.Value;
//                    if (bus4.OppID.HasValue && Model.Opponents.ContainsKey(bus4.OppID.Value) && bus4.OppID.Value == gol.OppID.Value)
//                    {
//                        DefencePos = (bus4.overlaptarget.HasValue ? bus4.overlaptarget.Value : bus4.target);
//                        busDefenderID = item.Key;
//                        busInfoHaseValue = true;
//                        break;
//                    }
//                }
//            }

//            if (busInfoHaseValue)
//            {
//                TargetPos = GameParameters.OurGoalRight + new Vector2D(-0.12, 0);
//            }

//            #region Normal
//            if (CurrentState == (int)GoalieStates.Normal)
//            {
//                Position2D PosTarget = GameParameters.OurGoalCenter;
//                angle = -Model.BallState.Speed.AngleInDegrees;
//                if (gol.TargetState != null)
//                {
//                    double x = 0;
//                    double y = 0;
//                    if ((gol.TargetState.Type == ObjectType.Opponent && !GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .3, out x, out y))
//                        || (gol.TargetState.Type == ObjectType.Ball && !GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .2, out x, out y)))//WC2017
//                    {
//                        if (busDefenderID.HasValue && Model.OurRobots.ContainsKey(busDefenderID.Value))
//                        {
//                            if (Model.OurRobots[busDefenderID.Value].Location.DistanceFrom(DefencePos) > .1)
//                            {
//                                Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
//                                Vector2D targetvector = gol.TargetState.Location - GameParameters.OurGoalCenter;//WC2017

//                                bool gotoperp = false;
//                                Position2D perp = targetvector.PrependecularPoint(GameParameters.OurGoalCenter, currentPos);

//                                if (perp.X > GameParameters.OurGoalCenter.X)
//                                {
//                                    Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
//                                    Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
//                                    Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
//                                    if (intersectPos.HasValue)
//                                    {
//                                        perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
//                                    }
//                                }
//                                if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < .9)
//                                {
//                                    gotoperp = true;
//                                }
//                                if (gotoperp)
//                                {
//                                    PosTarget = perp;
//                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                                }
//                                else
//                                {
//                                    Position2D tg = GameParameters.OurGoalCenter + (gol.TargetState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.6);
//                                    PosTarget = new Position2D(Math.Min(tg.X, GameParameters.OurGoalCenter.X - .1), tg.Y);
//                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                                }
//                            }
//                            else
//                            {
//                                Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
//                                Vector2D targetvector = gol.TargetState.Location - (GameParameters.OurGoalCenter + (GameParameters.OppGoalRight - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.OppGoalRight.DistanceFrom(GameParameters.OurGoalCenter) / 2));//WC2017

//                                bool gotoperp = false;
//                                Position2D perp = targetvector.PrependecularPoint(GameParameters.OurGoalCenter, currentPos);

//                                if (perp.X > GameParameters.OurGoalCenter.X)
//                                {
//                                    Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
//                                    Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
//                                    Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
//                                    if (intersectPos.HasValue)
//                                    {
//                                        perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
//                                    }
//                                }
//                                if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < .9)
//                                {
//                                    gotoperp = true;
//                                }
//                                if (gotoperp)
//                                {
//                                    PosTarget = perp;
//                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                                }
//                                else
//                                {
//                                    Position2D tg = GameParameters.OurGoalCenter + (gol.TargetState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.6);
//                                    PosTarget = new Position2D(Math.Min(tg.X, GameParameters.OurGoalCenter.X - .1), tg.Y);
//                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                                }
//                            }
//                        }
//                        else//WC2017
//                        {
//                            Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
//                            Vector2D targetvector = gol.TargetState.Location - GameParameters.OurGoalCenter;//WC2017

//                            bool gotoperp = false;
//                            Position2D perp = targetvector.PrependecularPoint(GameParameters.OurGoalCenter, currentPos);

//                            if (perp.X > GameParameters.OurGoalCenter.X)
//                            {
//                                Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
//                                Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
//                                Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
//                                if (intersectPos.HasValue)
//                                {
//                                    perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
//                                }
//                            }
//                            if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < .9)
//                            {
//                                gotoperp = true;
//                            }
//                            if (gotoperp)
//                            {
//                                PosTarget = perp;
//                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                            }
//                            else
//                            {
//                                Position2D tg = GameParameters.OurGoalCenter + (gol.TargetState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.9);
//                                PosTarget = new Position2D(Math.Min(tg.X, GameParameters.OurGoalCenter.X - .1), tg.Y);
//                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                            }
//                        }
//                    }
//                    else if (gol.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .3, out x, out y)) //WC2017
//                    {
//                        if (gol != null && gol.TargetState != null
//                            && gol.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(gol.TargetState.Location, false, 0, out x, out y))
//                        {
//                            Planner.AddKick(RobotID, true);
//                            Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
//                            DrawingObjects.AddObject(new Circle(Model.BallState.Location + (gol.TargetState.Location - Model.BallState.Location).GetNormalizeToCopy(.3), .05, new Pen(Color.Gray, .03f)), "987893z2s1dfgh56er751221");
//                            PosTarget = gol.TargetState.Location;
//                            angle = -Model.BallState.Speed.AngleInDegrees;
//                        }
//                        else
//                        {
//                            if (gol == null || gol.TargetState == null)
//                            {
//                                gol.TargetState = Model.BallState;
//                            }
//                            Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
//                            Vector2D targetvector = gol.TargetState.Location - GameParameters.OurGoalCenter;//WC2017

//                            bool gotoperp = false;
//                            Position2D perp = targetvector.PrependecularPoint(GameParameters.OurGoalCenter, currentPos);

//                            if (perp.X > GameParameters.OurGoalCenter.X)
//                            {
//                                Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
//                                Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
//                                Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
//                                if (intersectPos.HasValue)
//                                {
//                                    perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
//                                }
//                            }
//                            if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < .9)
//                            {
//                                gotoperp = true;
//                            }
//                            if (gotoperp)
//                            {
//                                PosTarget = perp;
//                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                            }
//                            else
//                            {
//                                Position2D tg = GameParameters.OurGoalCenter + (gol.TargetState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.9);
//                                PosTarget = new Position2D(Math.Min(tg.X, GameParameters.OurGoalCenter.X - .1), tg.Y);
//                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                            }
//                        }
//                    }
//                    else if (gol.TargetState.Type == ObjectType.Ball && GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .2, out x, out y)) //WC2017
//                    {
//                        if (Model.BallState.Speed.Size > 1 && GameParameters.IsInDangerousZone(Model.BallState.Location, falled, 0.0, out x, out y))
//                        {
//                            DrawingObjects.AddObject(new StringDraw("targetPos," + gol.TargetState.Type.ToString() + " , bs>1 , margin = 0.0", new Position2D(5.4, 0)), "56463ds25f146hb4lkagpewg545252eg654");
//                            //1
//                            Position2D p = Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(0.2);
//                            Line ballline = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
//                            List<Position2D> posIntersectList = new List<Position2D>() { new Position2D(3.60, 1.21), new Position2D(5, 0.61) };//GameParameters.LineIntersectWithDangerZone(ballline, true);
//                            List<Position2D> TempPos = new List<Position2D>();
//                            foreach (var item in posIntersectList)
//                            {
//                                if (item.X > GameParameters.OurGoalCenter.X)
//                                {
//                                    Line l = new Line(Model.BallState.Location, item);
//                                    Position2D? pi = l.IntersectWithLine(new Line(GameParameters.OurLeftCorner, GameParameters.OurRightCorner));
//                                    Vector2D v = (pi.Value - Model.BallState.Location).GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(pi.Value) - 0.11);
//                                    TempPos.Add(Model.BallState.Location + v);
//                                }
//                                else
//                                {
//                                    TempPos.Add(item);
//                                }
//                            }
//                            posIntersectList = TempPos.OrderBy(o => o.DistanceFrom(Model.BallState.Location)).ToList();
//                            if (posIntersectList.Count > 0)
//                            {
//                                Position2D? posi = posIntersectList.FirstOrDefault();
//                                if (posi.HasValue)
//                                {
//                                    p = posi.Value + (GameParameters.OurGoalCenter - posi.Value).GetNormalizeToCopy(0.2);
//                                }
//                                if (p.X > GameParameters.OurGoalCenter.X)
//                                {
//                                    p = p + (p - Model.BallState.Location).GetNormalizeToCopy(p.DistanceFrom(Model.BallState.Location) - 0.1);
//                                }
//                            }
//                            else
//                            {
//                                //2
//                                p = GameParameters.OurGoalCenter + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) - 0.2);
//                                PosTarget = p;
//                                angle = -Model.BallState.Speed.AngleInDegrees;
//                                DrawingObjects.AddObject(new Circle(p, 0.02, new Pen(Brushes.DarkRed, 0.05f)), "ds21f32ds1ffsd53fds58ref2w365ef4fr4");
//                            }
//                        }
//                        else
//                        {
//                            DrawingObjects.AddObject(new StringDraw("targetPos," + gol.TargetState.Type.ToString() + ", bs<1 ,margin != 0.0", new Position2D(5.4, 0)), "56fbghf58465465sdfgb564hshhsh4");
//                            Position2D p = Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(0.15);
//                            if (p.X > GameParameters.OurGoalCenter.X)
//                            {
//                                p = new Position2D(GameParameters.OurGoalCenter.X, p.Y);
//                            }
//                            DrawingObjects.AddObject(new Circle(p, 0.02, new Pen(Brushes.DarkRed, 0.05f)), "ds21f32ds1sd35f4qwnghppsb2ff58ref2w365ef4fr4");
//                            PosTarget = p;
//                            angle = -Model.BallState.Speed.AngleInDegrees;
//                        }
//                    }

//                    if (initialpos == Position2D.Zero)
//                    {
//                        initialpos = Model.BallState.Location;
//                    }
//                    Line goalerLine = new Line(Model.OurRobots[RobotID].Location, PosTarget);
//                    Line ballLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed);
//                    Position2D? PosIntersect = ballLine.IntersectWithLine(goalerLine);

//                    if (!BallKickedToOurGoal(Model) && PosIntersect.HasValue && (PosIntersect.Value.X > Model.OurRobots[RobotID].Location.X + 0.9))//&& (PosTarget.X > Model.OurRobots[RobotID].Location.X + 0.9))//(PosTarget.DistanceFrom(GameParameters.OurGoalCenter) < Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
//                    {
//                        initialpos = Model.BallState.Location;
//                        intersectTime = IntersectFind(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location);
//                        double velocity = Model.BallState.Speed.Size;
//                        ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersectTime));
//                        robotCoeff = predicttime(Model, RobotID, initialpos, intersectTime);
//                        double robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location, intersectTime);
//                        if (ballcoeff > 0 && intersectTime != (new Position2D(100, 100)) && (((robotCoeff - ballcoeff) > -0.2 && (robotCoeff - ballcoeff) < 0.3)))
//                        {
//                            lastPosNormal = Model.OurRobots[RobotID].Location;
//                            cut = true;
//                        }
//                        else if ((robotCoeff - ballcoeff) > 0.4 || (robotCoeff - ballcoeff) < -0.3)
//                        {
//                            cut = false;
//                            lastPosNormal = Position2D.Zero;
//                        }


//                        if (cut)
//                        {
//                            if (GameParameters.IsInDangerousZone(lastPosNormal, false, 0.2, out x, out y) && lastPosNormal != Position2D.Zero)
//                                PosTarget = lastPosNormal;
//                        }
//                    }
//                    if (!cut)
//                    {
//                        lastPosNormal = Position2D.Zero;
//                    }
//                    DrawingObjects.AddObject(new StringDraw("time = " + (robotCoeff - ballcoeff).ToString(), new Position2D(5, 2)), "3asf21asf21a3sdsrytwr68hj514trehj");
//                    DrawingObjects.AddObject(new Circle(intersectTime, 0.02, new Pen(Brushes.Red, 0.05f)), "ret5w4ert5uh1terj31eyt58kj6tr5j");

//                    DrawingObjects.AddObject(new StringDraw((gol.TargetState.Type == ObjectType.Opponent) ? "Robot" : "Ball", new Position2D(4.6, 0.0)), "12345ccxz6789abcdefgha35s21c4");
//                    DrawingObjects.AddObject(new Circle(gol.TargetState.Location, 0.15, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.02f)), "as.d02s3d21f3ds21f32ds14f8s2dqfjk6");
//                    DrawingObjects.AddObject(new Circle(PosTarget, 0.02, new Pen(Brushes.Blue, 0.05f)), "56cxvcxdv564sd2v44asd32dsd3f54dddss654654");
//                    DrawingObjects.AddObject(new StringDraw("cut:" + cut.ToString() + " , " + PosTarget.toString(), new Position2D(5.2, 2)), "5646543sd8sd3vcsd153vsdqwdeqwftry76546ij654");
//                    Planner.Add(RobotID, PosTarget, angle, PathType.UnSafe, false, false, false, false);
//                }
//                else
//                {
//                    DrawingObjects.AddObject(new StringDraw("Gol == null,exception", new Position2D(5.3, 2)), "564sdv12ds65f3fdsf63654654");
//                    Planner.Add(RobotID, GameParameters.OurGoalCenter, angle, PathType.UnSafe, false, false, false, false);
//                }
//            }
//            #endregion

//            #region In Penalty Area
//            else if (CurrentState == (int)GoalieStates.InPenaltyArea)
//            {
//                Vector2D ballSpeed = ballStateFast.Speed;
//                double v = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - ballStateFast.Location));
//                double maxIncomming = 1.5, maxVertical = 11, maxOutGoing = 1;
//                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
//                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
//                    + maxVertical;
//                double maxSpeedToGet = 0.5;
//                double dist, dist2;
//                double margin = 0.1;
//                double distToBall = ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
//                if (distToBall == 0)
//                    distToBall = 0.5;
//                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

//                Line ballspeed = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
//                Line goalline = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
//                Position2D? intersect = ballspeed.IntersectWithLine(goalline);
//                bool skip = false;
//                bool goActive = false;
//                bool Gointersect = false;
//                Position2D target = new Position2D();

//                if (intersect.HasValue)
//                {
//                    Position2D intersects = intersect.Value;
//                    if (((intersects.Y > GameParameters.OurGoalLeft.Y + .15 && intersects.Y < 1.15) || (intersects.Y < GameParameters.OurGoalLeft.Y - .15 && intersects.Y > -1.15)) && Model.BallState.Speed.Size > .3 && Model.BallState.Speed.InnerProduct(GameParameters.OurGoalCenter - Model.BallState.Location) > 0)
//                    {
//                        skip = true;
//                    }
//                    else
//                    {
//                        skip = false;
//                    }
//                    //if (((intersects.Y < GameParameters.OurGoalLeft.Y + .15 && intersects.Y > 0) || (intersects.Y > GameParameters.OurGoalLeft.Y - .15 && intersects.Y < 0)))
//                    //{
//                    //    togoal = true;
//                    //}
//                    //else
//                    //{
//                    //    togoal = false;
//                    //}
//                }
//                else
//                {
//                    skip = false;
//                }

//                if ((acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
//                {
//                    goActive = true;
//                }
//                else
//                {
//                    goActive = false;
//                }
//                if (acceptable2 * 1.2 < ballSpeed.Size)
//                {
//                    Gointersect = true;
//                }
//                else
//                {
//                    Gointersect = false;
//                }
//                if (skip)
//                {
//                    DrawingObjects.AddObject(new StringDraw("skip", GameParameters.OurGoalCenter.Extend(0.6, 0)), "5645646465564");
//                }
//                if (goActive)
//                {
//                    DrawingObjects.AddObject(new StringDraw("goActive", GameParameters.OurGoalCenter.Extend(0.7, 0)), "654564654565464");
//                }
//                if (Gointersect)
//                {
//                    DrawingObjects.AddObject(new StringDraw("gointersect", GameParameters.OurGoalCenter.Extend(0.8, 0)), "987989856654564");
//                }
//                if (skip)
//                {
//                    double dist1 = 0;
//                    double dist21 = 0;
//                    Circle s = new Circle(GameParameters.OurGoalCenter, .5);
//                    Line ballrobot = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
//                    List<Position2D> interscts = s.Intersect(ballrobot);
//                    if (interscts.Count > 0)
//                    {
//                        Position2D? intersectp = interscts.OrderBy(y => y.DistanceFrom(Model.BallState.Location)).FirstOrDefault();
//                        if (intersectp.HasValue && GameParameters.IsInDangerousZone(intersectp.Value, false, -.5, out dist1, out dist21) && GameParameters.IsInField(intersectp.Value, 0))
//                        {
//                            target = new Position2D(Math.Min(intersectp.Value.X, GameParameters.OurGoalCenter.X - .1), intersectp.Value.Y);
//                            Planner.Add(RobotID, target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
//                        }
//                        else
//                        {
//                            skip = false;
//                        }
//                    }
//                    else
//                    {
//                        skip = false;
//                    }
//                }
//                else if (goActive)
//                {
//                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
//                    Position2D tar = TargetToKick(Model, RobotID);
//                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
//                }
//                else if (Gointersect)
//                {
//                    Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
//                    List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true);
//                    if (intersects.Count > 0)
//                    {
//                        Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
//                        if (GameParameters.IsInField(pos, -.1))
//                        {
//                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
//                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
//                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
//                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
//                            Vector2D targetvector = GameParameters.OurGoalCenter - pos;
//                            bool gotoperp = false;
//                            Planner.ChangeDefaulteParams(RobotID, false);
//                            Planner.SetParameter(RobotID, 8, 8);
//                            Position2D perp = new Position2D(Math.Min(targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location).X, GameParameters.OurGoalCenter.X - .1), targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location).Y);
//                            ;
//                            if (perp.DistanceFrom(Model.OurRobots[RobotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
//                            {
//                                gotoperp = true;
//                            }
//                            if (gotoperp)
//                            {
//                                Planner.Add(RobotID, perp, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
//                            }
//                            else
//                            {
//                                Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
//                            }
//                        }
//                        else if (GameParameters.IsInField(Model.BallState.Location, -.1))
//                        {
//                            pos = Model.BallState.Location;
//                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
//                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
//                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
//                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
//                            Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
//                        }
//                        else
//                        {
//                            pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(Model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
//                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
//                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
//                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
//                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
//                            Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
//                        }
//                    }
//                    else
//                    {
//                        Position2D pos = Model.BallState.Location;
//                        if (GameParameters.IsInField(pos, -.1))
//                        {
//                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
//                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
//                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
//                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
//                            Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
//                        }
//                    }
//                }
//                else
//                {
//                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
//                    Position2D tar = TargetToKick(Model, RobotID);
//                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
//                }
//                Obstacles obs = new Obstacles(Model);
//                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
//                Vector2D j = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.35);
//                double kickSpeed = 4;
//                if (ballState.Location.X > GameParameters.OurGoalCenter.X - 0.1 || Math.Abs(Model.OurRobots[RobotID].Angle.Value) < 100 || obs.Meet(ballState, new SingleObjectState(ballState.Location + j, Vector2D.Zero, 0), 0.022))
//                    kickSpeed = 0;
//                Planner.AddKick(RobotID, kickPowerType.Speed, kickSpeed, (kickSpeed > 0) ? true : false, false);
//            }
//            #endregion

//            #region Ball In Start of Chip
//            else if (CurrentState == (int)GoalieStates.BallInStartOfChip)
//            {
//                Planner.AddKick(RobotID, true);
//                Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
//                Planner.Add(RobotID, GameParameters.OurGoalCenter.Extend(-0.15, 0), 180, PathType.UnSafe, false, false, false, false);
//            }
//            #endregion

//            #region Kick To Goal
//            else if (CurrentState == (int)GoalieStates.KickToGoal)
//            {
//                if (Model.BallState.Speed.InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0)
//                    GetSkill<GoaliDiveSkill>().Dive(engine, Model, RobotID, true, 200);
//                else
//                {
//                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
//                    Position2D tar = TargetToKick(Model, RobotID);
//                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
//                }
//            }
//            #endregion

//            #region Kick To Robot
//            else if (CurrentState == (int)GoalieStates.KickToRobot) //Kick to robot
//            {
//                Planner.Add(RobotID, Model.OurRobots[RobotID].Location, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false);
//                Planner.AddKick(RobotID, true);
//                Planner.AddKick(RobotID, kickPowerType.Speed, true, 2.5);
//            }
//            #endregion
//        }

//        public override RoleCategory QueryCategory()
//        {
//            return RoleCategory.Goalie;
//        }

//        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
//        {
//            //if (Model.BallState.Location.DistanceFrom(ballSavedForStates) < .2 || (Model.BallState.Location - ballInitialState).InnerProduct(Model.BallState.Location - Model.BallFallingPoint) > 0 || Model.BallState.Speed.Size < .5)
//            //{
//            //    falled = true;
//            //}
//            if (!GameParameters.IsInField(Model.BallState.Location, 0.05))
//                CurrentState = (int)GoalieStates.Normal;
//            else
//            {
//                Vector2D ballSpeed = Model.BallStateFast.Speed;
//                double v = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - Model.BallStateFast.Location));
//                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
//                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
//                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
//                    + maxVertical;
//                double maxSpeedToGet = 0.5;
//                double dist, dist2;
//                double margin = 0.1;
//                double distToBall = Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
//                if (distToBall == 0)
//                    distToBall = 0.5;
//                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

//                double innerProduct = Vector2D.InnerProduct(Model.BallStateFast.Speed, (Model.OurRobots[RobotID].Location - Model.BallStateFast.Location));
//                double difAngle = Vector2D.AngleBetweenInDegrees(Model.BallStateFast.Speed, (Model.BallStateFast.Location - Model.OurRobots[RobotID].Location));

//                Circle c = new Circle(Model.OurRobots[RobotID].Location, 0.12);
//                Line l = new Line(Model.BallStateFast.Location, Model.BallStateFast.Location + Model.BallStateFast.Speed);
//                List<Position2D> inters = c.Intersect(l);

//                #region Normal
//                if (CurrentState == (int)GoalieStates.Normal)
//                {
//                    if (firsttime)
//                    {
//                        ballInitialState = Model.BallState.Location;
//                        firsttime = false;
//                    }
//                    if (Model.BallState.Speed.Size > .1)
//                        Ball.Add(Model.BallState.Location);
//                    List<int> RobotIds = Model.Opponents.OrderBy(i => i.Value.Location.DistanceFrom(ballInitialState)).Select(p => p.Key).ToList();
//                    Vector2D ballFirstBallState = new Vector2D();
//                    if (RobotIds.Count > 0)
//                    {
//                        int robotBackBall = Model.Opponents.OrderBy(y => y.Value.Location.DistanceFrom(ballInitialState)).Select(t => t.Key).FirstOrDefault();
//                        angles.Add(Model.Opponents[robotBackBall].Angle.Value);
//                        robotAgles.Add(Model.Opponents[RobotIds[0]].Angle.Value);
//                        ballFirstBallState = Model.BallState.Location - Model.Opponents[RobotIds[0]].Location;
//                        DrawingObjects.AddObject(new StringDraw("ekhtelafe zavie: " + (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(robotAgles.Last().ToRadian(), 1), ballFirstBallState))).ToString(), GameParameters.OurRightCorner.Extend(0.3, 0)), "dsfdsf3232dfdsfas328re");
//                        DrawingObjects.AddObject(new StringDraw("CounterInChip: " + CounterInChip.ToString(), GameParameters.OurRightCorner.Extend(0.2, 0)), "dsfdsf3564232dfdsfas328re");

//                        if (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(robotAgles.Last().ToRadian(), 1), ballFirstBallState)) > 7 && Model.BallState.Location.DistanceFrom(ballInitialState) > .07 && Model.BallState.Location.DistanceFrom(ballInitialState) < .2 && ballInitialState.X > 3.5 && CounterInChip < counterISChipStart)
//                        {
//                            if (onceaTime)
//                            {
//                                initialAngle = angles[angles.Count() - frameOfChipInEllipseEnvironment];
//                                onceaTime = false;
//                            }
//                            CurrentState = (int)GoalieStates.BallInStartOfChip;
//                            //WeHadBallInStartOfchip = true;
//                        }
//                        if (Model.BallState.Speed.Size < .1)
//                        {
//                            ballinRoll = false;
//                        }
//                    }
//                    if (BallKickedToOurGoal(Model))
//                        CurrentState = (int)GoalieStates.KickToGoal;
//                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
//                        CurrentState = (int)GoalieStates.KickToRobot;
//                    else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
//                        CurrentState = (int)GoalieStates.InPenaltyArea;

//                }
//                #endregion

//                #region In Penalty Area
//                else if (CurrentState == (int)GoalieStates.InPenaltyArea)
//                {
//                    Reset();
//                    margin = 0.2;
//                    if (BallKickedToOurGoal(Model) &&
//                        (!GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2)
//                        || acceptableballRobotSpeed * 1.2 < ballSpeed.Size))
//                        CurrentState = (int)GoalieStates.KickToGoal;
//                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
//                        CurrentState = (int)GoalieStates.KickToRobot;
//                    else if (!GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) || acceptable2 * 1.2 < ballSpeed.Size)
//                        CurrentState = (int)GoalieStates.Normal;
//                }
//                #endregion

//                #region Kick To Goal
//                else if (CurrentState == (int)GoalieStates.KickToGoal)
//                {
//                    Reset();
//                    margin = 0.1;
//                    if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
//                        CurrentState = (int)GoalieStates.KickToRobot;
//                    else if (!BallKickedToOurGoal(Model) && GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
//                        CurrentState = (int)GoalieStates.InPenaltyArea;
//                    else if (!BallKickedToOurGoal(Model))
//                        CurrentState = (int)GoalieStates.Normal;
//                }
//                #endregion

//                #region Kick To Robot
//                else if (CurrentState == (int)GoalieStates.KickToRobot)
//                {
//                    Reset();
//                    if (ballSpeed.Size < 1.5 || BallKickedToOurGoal(Model) || inters.Count == 0 || innerProduct < -0.1)
//                    {
//                        if (BallKickedToOurGoal(Model))
//                            CurrentState = (int)GoalieStates.KickToGoal;
//                        else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) && (acceptable2 < ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
//                            CurrentState = (int)GoalieStates.InPenaltyArea;
//                        else
//                            CurrentState = (int)GoalieStates.Normal;
//                    }
//                }
//                #endregion

//                #region Ball in Start of Chip
//                else if (CurrentState == (int)GoalieStates.BallInStartOfChip)
//                {
//                    CounterInChip++;
//                    if (CounterInChip > counterISChipStart)
//                    {
//                        CurrentState = (int)GoalieStates.Normal;
//                    }
//                }
//                #endregion
//            }
//            FreekickDefence.CurrentStates[this] = CurrentState;
//            currentState = CurrentState;
//            DrawingObjects.AddObject(new StringDraw(((GoalieStates)CurrentState).ToString(), GameParameters.OurGoalCenter.Extend(0.3, 0)), "gstate");
//        }

//        public bool BallKickedToOurGoal(WorldModel Model)
//        {
//            SingleObjectState BallState = Model.BallState;
//            double tresh = 0.25;
//            double tresh2 = 1.3;
//            if ((GoalieStates)currentState == GoalieStates.KickToGoal)
//            {
//                tresh = 0.3;
//                tresh2 = 1.4;
//            }
//            Line line = new Line();
//            line = new Line(BallState.Location, BallState.Location - BallState.Speed);
//            Position2D BallGoal = new Position2D();
//            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
//            double d = BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
//            DrawingObjects.AddObject(new StringDraw((d / BallState.Speed.Size < tresh2).ToString(), new Position2D(-1, 0)));
//            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + tresh && BallGoal.Y > GameParameters.OurGoalRight.Y - tresh)
//                if (BallState.Speed.InnerProduct(GameParameters.OurGoalRight - BallState.Location) > 0)
//                    if (BallState.Speed.Size > 0.1 && d / BallState.Speed.Size < tresh2)
//                        return true;
//            return false;
//        }

//        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            return 20 * RobotID;
//        }

//        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            return new List<RoleBase>() { new GoalieNormalRole() };
//        }

//        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            throw new NotImplementedException();
//        }

//        public Position2D TargetToKick(WorldModel Model, int robotID)
//        {
//            Vector2D v = ballState.Location - GameParameters.OurGoalCenter;
//            v = Vector2D.FromAngleSize(Math.Sign(v.AngleInRadians) * Math.Max(Math.Abs(v.AngleInRadians), (110.0).ToRadian()), v.Size);
//            return ballState.Location + v.GetNormalizeToCopy(2);
//        }

//        private int sengmentNumber(Position2D robotInitialPos, Position2D ballfallingPoint, Vector2D firstVector, Vector2D SecondVector, Vector2D robotAngle)
//        {
//            double forFirstSegment = .3;
//            double x, y;
//            Line sevenSegment = new Line(new Position2D(RobotLoc.X - .07, GameParameters.OurLeftCorner.Y), new Position2D(RobotLoc.X - .07, GameParameters.OurRightCorner.Y));
//            Line fallline = new Line(new Position2D(ballfallingPoint.X, GameParameters.OurLeftCorner.Y), new Position2D(ballfallingPoint.X, GameParameters.OurRightCorner.Y), new Pen(Brushes.Red, .01f));

//            if (GameParameters.IsInDangerousZone(ballSavedForStates, false, 0, out x, out y))
//            {
//                if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) > 0 && ballfallingPoint.X > RobotLoc.X - .07)
//                {
//                    return 7;
//                }
//                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) > 0)
//                {
//                    return 1;
//                }
//                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) > 0)
//                {
//                    return 6;
//                }
//                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) < 0)
//                {
//                    return 3;
//                }
//                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) < 0)
//                {
//                    return 4;
//                }
//                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) < Math.Abs(robotInitialPos.Y) + forFirstSegment && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) - forFirstSegment)
//                {
//                    return 2;
//                }
//                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) < Math.Abs(robotInitialPos.Y) + forFirstSegment && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) - forFirstSegment)
//                {
//                    return 5;
//                }
//                else
//                {
//                    return 0;
//                }
//            }
//            else
//            {
//                return 0;
//            }
//        }

//        private int segmentNumber(WorldModel model, Position2D input, Vector2D robotAngle, int RobotID)
//        {
//            Vector2D SecondVector = GameParameters.OurGoalCenter.Extend(0, Math.Sign(ballInitialState.Y) * 0.975) - ballInitialState;
//            Vector2D firstVector = new Position2D(2.60, 0.00) - ballInitialState;
//            Position2D InfrontOfGoal = GameParameters.OurGoalCenter.Extend(-0.10, 0.00);
//            double forFirstSegment = 0.30;
//            double x, y;
//            Position2D RobotLoc = model.OurRobots[RobotID].Location;
//            if (GameParameters.IsInDangerousZone(input, false, 0.00, out x, out y))
//            {
//                if (input.X > 3)
//                {
//                    return 0;
//                }
//                if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) > 0 && input.X > RobotLoc.X - 0.07)
//                {
//                    return 7;
//                }
//                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) > 0)
//                {
//                    return 1;
//                }
//                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) > 0)
//                {
//                    return 6;
//                }
//                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) < 0)
//                {
//                    return 3;
//                }
//                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) < 0)
//                {
//                    return 4;
//                }
//                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) < Math.Abs(InfrontOfGoal.Y) + forFirstSegment && Math.Abs(model.BallState.Location.Y) > Math.Abs(InfrontOfGoal.Y) - forFirstSegment)
//                {
//                    return 2;
//                }
//                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) < Math.Abs(InfrontOfGoal.Y) + forFirstSegment && Math.Abs(model.BallState.Location.Y) > Math.Abs(InfrontOfGoal.Y) - forFirstSegment)
//                {
//                    return 5;
//                }
//                else
//                {
//                    return 0;
//                }
//            }
//            else
//            {
//                return 0;
//            }
//        }

//        private bool IsBetween(Vector2D firstVector, Vector2D secondvector, Vector2D robotAngle)
//        {
//            if (ballInitialState.Y < 0)
//            {
//                //first vector have bigger angle
//                double angle = robotAngle.AngleInRadians;
//                if (angle < firstVector.AngleInRadians && angle > secondvector.AngleInRadians)
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }
//            else
//            {
//                //second vector have bigger angle
//                double angle = robotAngle.AngleInRadians;

//                List<int> f = new List<int>();


//                if (angle < secondvector.AngleInRadians && angle > firstVector.AngleInRadians)
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }
//        }


//        //
//        private bool firstTime3 = true;
//        public static Position2D lasttargetPoint = new Position2D();
//        private int counter = 0;
//        int Deccelcounter = 0;
//        Position2D lasttarget = new Position2D();
//        Position2D lastinitpos = new Position2D();

//        // time from current pos
//        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos)
//        {
//            Position2D initialstate = initialpos;
//            Position2D target = lastpos;

//            if (firstTime3)
//            {
//                firstTime3 = false;
//                lasttargetPoint = lastpos;
//            }
//            if (target.DistanceFrom(lasttargetPoint) > .05)
//            {
//                counter = 0;
//                Deccelcounter = 0;
//                firstTime3 = true;
//            }
//            Position2D currentPos = Model.OurRobots[RobotID].Location;
//            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
//            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
//            double vmax = Math.Sqrt(2 * 3.14 * daccel);
//            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
//            double ta = root(3.14, Va, daccel - Model.OurRobots[RobotID].Location.DistanceFrom(initialstate));
//            double tc = (Model.OurRobots[RobotID].Location.DistanceFrom(target) - deccelDX) / vmax;
//            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

//            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
//            double Td = Math.Min(.850, vmax / 3.04);
//            double total = 0;


//            counter++;
//            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
//            {
//                Deccelcounter++;
//            }
//            if (Deccelcounter > 10)
//            {
//                int g = 0;
//            }
//            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
//            {

//                if (currentPos.DistanceFrom(initialstate) < daccel)
//                {
//                    //1
//                    total = ta + tc2 + Td;
//                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
//                }
//                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
//                {
//                    //4
//                    total = tc + Td;
//                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
//                }
//                if (currentPos.DistanceFrom(target) < deccelDX)
//                {
//                    //3
//                    total = td;
//                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
//                }
//            }
//            else
//            {
//                if (currentPos.DistanceFrom(initialstate) < daccel)
//                {
//                    //2
//                    total = ta + Td;
//                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
//                }
//                else
//                {
//                    //3
//                    total = td;
//                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
//                }
//            }
//            lasttarget = target;
//            lastinitpos = initialstate;
//            return total;

//        }
//        // total time of path
//        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos, bool inittarget)
//        {
//            Position2D initialstate = initialpos;
//            Position2D target = lastpos;

//            if (firstTime3)
//            {
//                firstTime3 = false;
//                lasttargetPoint = lastpos;
//            }
//            if (target.DistanceFrom(lasttargetPoint) > .05)
//            {
//                counter = 0;
//                Deccelcounter = 0;
//                firstTime3 = true;
//            }
//            Position2D currentPos = initialpos;
//            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
//            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
//            double vmax = Math.Sqrt(2 * 3.14 * daccel);
//            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
//            double ta = root(3.14, Va, daccel - currentPos.DistanceFrom(initialstate));
//            double tc = (currentPos.DistanceFrom(target) - deccelDX) / vmax;
//            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

//            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
//            double Td = Math.Min(.850, vmax / 3.04);
//            double total = 0;


//            counter++;
//            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
//            {
//                Deccelcounter++;
//            }
//            if (Deccelcounter > 10)
//            {
//                int g = 0;
//            }
//            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
//            {

//                if (currentPos.DistanceFrom(initialstate) < daccel)
//                {
//                    //1
//                    total = ta + tc2 + Td;
//                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
//                }
//                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
//                {
//                    //4
//                    total = tc + Td;
//                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
//                }
//                if (currentPos.DistanceFrom(target) < deccelDX)
//                {
//                    //3
//                    total = td;
//                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
//                }
//            }
//            else
//            {
//                if (currentPos.DistanceFrom(initialstate) < daccel)
//                {
//                    //2
//                    total = ta + Td;
//                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
//                }
//                else
//                {
//                    //3
//                    total = td;
//                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
//                }
//            }
//            lasttarget = target;
//            lastinitpos = initialstate;
//            return total;

//        }
//        // time of arriving to intersect when we want to stop in the intersect pos
//        double predicttime(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
//        {
//            Position2D currentPos = model.OurRobots[RobotID].Location;
//            double deccelDX = Math.Min(1.09, .54 * init.DistanceFrom(target));
//            double accelDx = Math.Min(0.942, .46 * init.DistanceFrom(target));
//            double vmax = Math.Sqrt(2 * 3.14 * accelDx);
//            double adeccel = 3.04;
//            double aaccel = 3.14;

//            double deltaXIntersectTarget = intersect.DistanceFrom(target);

//            double coeff1 = deltaXIntersectTarget / deccelDX;
//            double v0deccel = vmax * coeff1;
//            double tTemp = v0deccel / adeccel;
//            double tdeccel = predicttime(model, RobotID, init, target, true) - tTemp;

//            double deltaxInitialIntersect = init.DistanceFrom(intersect);
//            double coeff2 = deltaxInitialIntersect / accelDx;

//            double V0accel = coeff2 * vmax;
//            double taccel = V0accel / aaccel;

//            double tcruise = ((deltaxInitialIntersect - accelDx) / vmax) + (vmax / accelDx);

//            double deltaXInitialTarget = init.DistanceFrom(target);
//            double ttotal = 0;
//            if (deltaXIntersectTarget < accelDx + deccelDX) // Accel - Deccel
//            {
//                if (deltaXIntersectTarget > deccelDX)
//                {
//                    ttotal = taccel;
//                }
//                else
//                {
//                    ttotal = tdeccel;
//                }
//            }
//            else // Accel - Cruise - Deccel
//            {
//                if (deltaxInitialIntersect < accelDx)
//                {
//                    ttotal = taccel;
//                }
//                else if (deltaXIntersectTarget < deccelDX)
//                {
//                    ttotal = tdeccel;
//                }
//                else
//                {
//                    ttotal = tcruise;
//                }
//            }
//            return ttotal;
//        }
//        // time of arriving to intersect when we dont to stop in the intersect pos
//        double timeRobotToTargetInIntersect(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
//        {
//            double timeInittarget = predicttime(model, RobotID, init, target, true);
//            double timeInitIntersect = predicttime(model, RobotID, init, target, intersect);
//            double timeIntersecttarget = timeInittarget - timeInitIntersect;
//            double timeRobotTarget = predicttime(model, RobotID, init, target);
//            double timeRobotIntesect = timeRobotTarget - timeIntersecttarget;
//            return timeRobotIntesect;
//        }
//        private Position2D IntersectFind(WorldModel model, int RobotID, Position2D initpoint, Position2D target)
//        {
//            Position2D robotSpeedPos = model.OurRobots[RobotID].Location + model.OurRobots[RobotID].Speed;
//            Position2D ballspeedpos = model.BallState.Location + model.BallState.Speed;

//            double x4 = target.X;

//            double x3 = initpoint.X;
//            double y4 = target.Y;
//            double y3 = initpoint.Y;
//            double x2 = ballspeedpos.X;
//            double y2 = ballspeedpos.Y;
//            double x1 = model.BallState.Location.X;
//            double y1 = model.BallState.Location.Y;
//            //double x = (((((x1 * y2) - (y1 * x2)) * (x3 - x4)) - ((x1 - x2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));
//            //double y = (((((x1 * y2) - (y1 * x2)) * (y3 - y4)) - ((y1 - y2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));

//            Line first = new Line(new Position2D(x1, y1), new Position2D(x2, y2));
//            Line second = new Line(new Position2D(x3, y3), new Position2D(x4, y4));
//            Position2D intersect = new Position2D();
//            if (first.IntersectWithLine(second).HasValue)
//                intersect = first.IntersectWithLine(second).Value;
//            else
//            {
//                intersect = new Position2D(100, 100);
//            }
//            return intersect;
//        }
//        static double root(double a, double initialV, double deltaX)
//        {
//            double t = 0;
//            double delta = (initialV * initialV) - (2 * a * -deltaX);
//            if (delta == 0)
//            {
//                t = -initialV / (.5 * a);
//            }
//            if (delta > 0)
//            {

//                double t1 = (-initialV - Math.Sqrt(delta)) / a;
//                double t2 = (-initialV + Math.Sqrt(delta)) / a;
//                if (t2 > 0 && t1 < 0)
//                    t = t2;
//                else if (t1 > 0 && t2 < 0)
//                    t = t1;
//                else if (t1 > 0 && t2 > 0)
//                    if (t1 < t2)
//                        t = t1;
//                    else
//                        t = t2;
//            }
//            if (delta < 0)
//                return -1000;
//            return t;
//        }



//        public void Reset()
//        {
//            onceaTime = true;
//            onceaTime2 = true;
//            firstTimeBallInChipKick = true;
//            ballSavedForStates = new Position2D();
//            ballSavedPos = new Position2D();
//            RobotLoc = new Position2D();
//        }

//        public enum GoalieStates
//        {
//            Normal = 0,
//            InPenaltyArea = 1,
//            KickToGoal = 2,
//            KickToRobot = 3,
//            BallInStartOfChip = 5,
//        }

//        public enum ChipState
//        {
//            Left,
//            Right
//        }

//    }
//}
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills.GoalieSkills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public class BusGoalieCornerRole : RoleBase, IGoalie
    {
        public static Position2D RobotLoc = new Position2D();
        private Position2D ballSavedForStates = new Position2D();
        public static Position2D ballSavedPos = new Position2D();
        public static bool firstTimeBallInChipKick = true;
        List<Position2D> Ball = new List<Position2D>();
        Position2D ballInitialState = new Position2D();
        List<double> robotAgles = new List<double>();
        private bool WeHadBallInStartOfchip = false;
        public static bool ballinRoll = false;
        Position2D Target = new Position2D();
        static int CurrentChipState = 0;
        int CounterInChip = 0;
        bool firsttime = true;
        bool debug = false;
        List<double> angles = new List<double>();
        int currentState;
        private bool onceaTime = true;
        private double initialAngle = 0;
        private int frameOfChipInEllipseEnvironment = 4;
        private bool falled = false;
        private bool onceaTime2 = true;
        Position2D lastTarget = new Position2D();
        double angle = 0;
        private bool onceaTime3 = true;
        private bool onceaTime4 = true;
        private int status = 2;
        private bool onceaTime5 = true;
        int mainRobot = 0;
        bool weHadFullSupport = true;
        bool goToPrependicular = true;
        bool gofront = true;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        double staticAngle = 0;
        private bool onceaTime6 = true;
        int counterISChipStart = 0;//WC2017
        bool cut = false;//WC2017
        Position2D lastPosNormal = Position2D.Zero;//WC2017
        double ballcoeff;
        double robotCoeff;
        Position2D intersectTime;
        Position2D lastBallPosOutOfDangerZone = new Position2D();
        Position2D initialpos = Position2D.Zero;

        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta, DefenceInfo gol, bool strategyGoalie, bool ballismoved, Dictionary<int, RoleBase> AssignedRoles)
        {
            Position2D rightCenter = GameParameters.OurGoalCenter + (GameParameters.OurGoalRight - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.OurGoalRight.DistanceFrom(GameParameters.OurGoalCenter) / 2);

            TargetPos = new Position2D(Math.Min(TargetPos.X, GameParameters.OurGoalCenter.X - .1), TargetPos.Y);
            angle = -Model.BallState.Speed.AngleInDegrees;

            ballState = Model.BallState;
            ballStateFast = Model.BallStateFast;

            //DefenceInfo inf = null;
            bool busInfoHaseValue = false;
            Position2D DefencePos = new Position2D();
            int? busDefenderID = null;
            //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(DefenderNormalRole1)))
            //    inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(DefenderNormalRole1)).First();
            BusRole1 bus1 = new BusRole1();
            BusRole2 bus2 = new BusRole2();
            BusRole3 bus3 = new BusRole3();
            BusRole4 bus4 = new BusRole4();
            foreach (var item in AssignedRoles)
            {
                if (item.Value.GetType() == typeof(BusRole1))
                {
                    bus1 = (BusRole1)item.Value;
                    if (bus1.OppID.HasValue && Model.Opponents.ContainsKey(bus1.OppID.Value) && bus1.OppID.Value == gol.OppID.Value)
                    {
                        DefencePos = (bus1.overlaptarget.HasValue ? bus1.overlaptarget.Value : bus1.target);
                        busDefenderID = item.Key;
                        busInfoHaseValue = true;
                        break;
                    }
                }
                if (item.Value.GetType() == typeof(BusRole2))
                {
                    bus2 = (BusRole2)item.Value;
                    if (bus2.OppID.HasValue && Model.Opponents.ContainsKey(bus2.OppID.Value) && bus2.OppID.Value == gol.OppID.Value)
                    {
                        DefencePos = (bus2.overlaptarget.HasValue ? bus2.overlaptarget.Value : bus2.target);
                        busDefenderID = item.Key;
                        busInfoHaseValue = true;
                        break;
                    }
                }
                if (item.Value.GetType() == typeof(BusRole3))
                {
                    bus3 = (BusRole3)item.Value;
                    if (bus3.OppID.HasValue && Model.Opponents.ContainsKey(bus3.OppID.Value) && bus3.OppID.Value == gol.OppID.Value)
                    {
                        DefencePos = (bus3.overlaptarget.HasValue ? bus3.overlaptarget.Value : bus3.target);
                        busDefenderID = item.Key;
                        busInfoHaseValue = true;
                        break;
                    }
                }
                if (item.Value.GetType() == typeof(BusRole4))
                {
                    bus4 = (BusRole4)item.Value;
                    if (bus4.OppID.HasValue && Model.Opponents.ContainsKey(bus4.OppID.Value) && bus4.OppID.Value == gol.OppID.Value)
                    {
                        DefencePos = (bus4.overlaptarget.HasValue ? bus4.overlaptarget.Value : bus4.target);
                        busDefenderID = item.Key;
                        busInfoHaseValue = true;
                        break;
                    }
                }
            }

            if (busInfoHaseValue)
            {
                TargetPos = rightCenter;//GameParameters.OurGoalRight + new Vector2D(-0.12, 0);
            }

            #region Normal
            if (CurrentState == (int)GoalieStates.Normal)
            {
                Position2D PosTarget = GameParameters.OurGoalCenter;
                angle = -Model.BallState.Speed.AngleInDegrees;
                if (gol.TargetState != null)
                {
                    double x = 0;
                    double y = 0;
                    if ((gol.TargetState.Type == ObjectType.Opponent && !GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .3, out x, out y))
                        || (gol.TargetState.Type == ObjectType.Ball && !GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .2, out x, out y)))//WC2017
                    {
                        if (busDefenderID.HasValue && Model.OurRobots.ContainsKey(busDefenderID.Value))
                        {
                            if (Model.OurRobots[busDefenderID.Value].Location.DistanceFrom(DefencePos) > .1)
                            {
                                Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
                                Vector2D targetvector = gol.TargetState.Location - rightCenter;//WC2017

                                bool gotoperp = false;
                                Position2D perp = targetvector.PrependecularPoint(rightCenter, currentPos);

                                if (perp.X > rightCenter.X)
                                {
                                    Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                                    Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
                                    Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
                                    if (intersectPos.HasValue)
                                    {
                                        perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
                                    }
                                }
                                if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(rightCenter) < .9)
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    PosTarget = perp;
                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                }
                                else
                                {
                                    Position2D tg = rightCenter + (gol.TargetState.Location - rightCenter).GetNormalizeToCopy(.6);
                                    PosTarget = new Position2D(Math.Min(tg.X, rightCenter.X - .1), tg.Y);
                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                }
                            }
                            else
                            {
                                Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
                                Vector2D targetvector = gol.TargetState.Location - rightCenter;//WC2017

                                bool gotoperp = false;
                                Position2D perp = targetvector.PrependecularPoint(rightCenter, currentPos);

                                if (perp.X > rightCenter.X)
                                {
                                    Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                                    Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
                                    Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
                                    if (intersectPos.HasValue)
                                    {
                                        perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
                                    }
                                }
                                if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(rightCenter) < .9)
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    PosTarget = perp;
                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                }
                                else
                                {
                                    Position2D tg = rightCenter + (gol.TargetState.Location - rightCenter).GetNormalizeToCopy(.6);
                                    PosTarget = new Position2D(Math.Min(tg.X, rightCenter.X - .1), tg.Y);
                                    angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                }
                            }
                        }
                        else//WC2017
                        {
                            Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
                            Vector2D targetvector = gol.TargetState.Location - rightCenter;//WC2017

                            bool gotoperp = false;
                            Position2D perp = targetvector.PrependecularPoint(rightCenter, currentPos);

                            if (perp.X > rightCenter.X)
                            {
                                Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                                Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
                                Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
                                if (intersectPos.HasValue)
                                {
                                    perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
                                }
                            }
                            if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(rightCenter) < .9)
                            {
                                gotoperp = true;
                            }
                            if (gotoperp)
                            {
                                PosTarget = perp;
                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                            }
                            else
                            {
                                Position2D tg = rightCenter + (gol.TargetState.Location - rightCenter).GetNormalizeToCopy(.9);
                                PosTarget = new Position2D(Math.Min(tg.X, rightCenter.X - .1), tg.Y);
                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                            }
                        }
                    }
                    else if (gol.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .3, out x, out y)) //WC2017
                    {
                        if (gol != null && gol.TargetState != null
                            && gol.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(gol.TargetState.Location, false, 0, out x, out y))
                        {
                            Planner.AddKick(RobotID, true);
                            Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
                            DrawingObjects.AddObject(new Circle(Model.BallState.Location + (gol.TargetState.Location - Model.BallState.Location).GetNormalizeToCopy(.3), .05, new Pen(Color.Gray, .03f)), "987893z2s1dfgh56er751221");
                            PosTarget = gol.TargetState.Location;
                            angle = -Model.BallState.Speed.AngleInDegrees;
                        }
                        else
                        {
                            if (gol == null || gol.TargetState == null)
                            {
                                gol.TargetState = Model.BallState;
                            }
                            Position2D currentPos = Model.OurRobots[RobotID].Location;//WC2017
                            Vector2D targetvector = gol.TargetState.Location - rightCenter;//WC2017

                            bool gotoperp = false;
                            Position2D perp = targetvector.PrependecularPoint(rightCenter, currentPos);

                            if (perp.X > rightCenter.X)
                            {
                                Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                                Line targetLine = new Line(gol.TargetState.Location, gol.TargetState.Location + targetvector);
                                Position2D? intersectPos = goalLine.IntersectWithLine(targetLine);
                                if (intersectPos.HasValue)
                                {
                                    perp = gol.TargetState.Location + (intersectPos.Value - gol.TargetState.Location).GetNormalizeToCopy(intersectPos.Value.DistanceFrom(gol.TargetState.Location) - 0.1);//WC2017
                                }
                            }
                            if (perp.DistanceFrom(currentPos) > .1 && perp.DistanceFrom(rightCenter) < .9)
                            {
                                gotoperp = true;
                            }
                            if (gotoperp)
                            {
                                PosTarget = perp;
                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                            }
                            else
                            {
                                Position2D tg = rightCenter + (gol.TargetState.Location - rightCenter).GetNormalizeToCopy(.9);
                                PosTarget = new Position2D(Math.Min(tg.X, rightCenter.X - .1), tg.Y);
                                angle = (gol.TargetState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                            }
                        }
                    }
                    else if (gol.TargetState.Type == ObjectType.Ball && GameParameters.IsInDangerousZone(gol.TargetState.Location, false, .2, out x, out y)) //WC2017
                    {
                        if (Model.BallState.Speed.Size > 1 && GameParameters.IsInDangerousZone(Model.BallState.Location, falled, 0.0, out x, out y))
                        {
                            DrawingObjects.AddObject(new StringDraw("targetPos," + gol.TargetState.Type.ToString() + " , bs>1 , margin = 0.0", new Position2D(5.4, 0)), "56463ds25f146hb4lkagpewg545252eg654");
                            //1
                            Position2D p = Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(0.2);
                            Line ballline = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                            List<Position2D> posIntersectList = new List<Position2D>() { new Position2D(3.60, 1.21), new Position2D(5, 0.61) };//GameParameters.LineIntersectWithDangerZone(ballline, true);
                            List<Position2D> TempPos = new List<Position2D>();
                            foreach (var item in posIntersectList)
                            {
                                if (item.X > rightCenter.X)
                                {
                                    Line l = new Line(Model.BallState.Location, item);
                                    Position2D? pi = l.IntersectWithLine(new Line(GameParameters.OurLeftCorner, GameParameters.OurRightCorner));
                                    Vector2D v = (pi.Value - Model.BallState.Location).GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(pi.Value) - 0.11);
                                    TempPos.Add(Model.BallState.Location + v);
                                }
                                else
                                {
                                    TempPos.Add(item);
                                }
                            }
                            posIntersectList = TempPos.OrderBy(o => o.DistanceFrom(Model.BallState.Location)).ToList();
                            if (posIntersectList.Count > 0)
                            {
                                Position2D? posi = posIntersectList.FirstOrDefault();
                                if (posi.HasValue)
                                {
                                    p = posi.Value + (rightCenter - posi.Value).GetNormalizeToCopy(0.2);
                                }
                                if (p.X > rightCenter.X)
                                {
                                    p = p + (p - Model.BallState.Location).GetNormalizeToCopy(p.DistanceFrom(Model.BallState.Location) - 0.1);
                                }
                            }
                            else
                            {
                                //2
                                p = rightCenter + (Model.BallState.Location - rightCenter).GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(rightCenter) - 0.2);
                                PosTarget = p;
                                angle = -Model.BallState.Speed.AngleInDegrees;
                                DrawingObjects.AddObject(new Circle(p, 0.02, new Pen(Brushes.DarkRed, 0.05f)), "ds21f32ds1ffsd53fds58ref2w365ef4fr4");
                            }
                        }
                        else
                        {
                            DrawingObjects.AddObject(new StringDraw("targetPos," + gol.TargetState.Type.ToString() + ", bs<1 ,margin != 0.0", new Position2D(5.4, 0)), "56fbghf58465465sdfgb564hshhsh4");
                            Position2D p = Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(0.15);
                            if (p.X > rightCenter.X)
                            {
                                p = new Position2D(rightCenter.X, p.Y);
                            }
                            DrawingObjects.AddObject(new Circle(p, 0.02, new Pen(Brushes.DarkRed, 0.05f)), "ds21f32ds1sd35f4qwnghppsb2ff58ref2w365ef4fr4");
                            PosTarget = p;
                            angle = -Model.BallState.Speed.AngleInDegrees;
                        }
                    }

                    if (initialpos == Position2D.Zero)
                    {
                        initialpos = Model.BallState.Location;
                    }
                    Line goalerLine = new Line(Model.OurRobots[RobotID].Location, PosTarget);
                    Line ballLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed);
                    Position2D? PosIntersect = ballLine.IntersectWithLine(goalerLine);

                    if (!BallKickedToOurGoal(Model) && PosIntersect.HasValue && (PosIntersect.Value.X > Model.OurRobots[RobotID].Location.X + 0.9))//&& (PosTarget.X > Model.OurRobots[RobotID].Location.X + 0.9))//(PosTarget.DistanceFrom(GameParameters.OurGoalCenter) < Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
                    {
                        initialpos = Model.BallState.Location;
                        intersectTime = IntersectFind(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location);
                        double velocity = Model.BallState.Speed.Size;
                        ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersectTime));
                        robotCoeff = predicttime(Model, RobotID, initialpos, intersectTime);
                        double robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location, intersectTime);
                        if (ballcoeff > 0 && intersectTime != (new Position2D(100, 100)) && (((robotCoeff - ballcoeff) > -0.2 && (robotCoeff - ballcoeff) < 0.3)))
                        {
                            lastPosNormal = Model.OurRobots[RobotID].Location;
                            cut = true;
                        }
                        else if ((robotCoeff - ballcoeff) > 0.4 || (robotCoeff - ballcoeff) < -0.3)
                        {
                            cut = false;
                            lastPosNormal = Position2D.Zero;
                        }


                        if (cut)
                        {
                            if (GameParameters.IsInDangerousZone(lastPosNormal, false, 0.2, out x, out y) && lastPosNormal != Position2D.Zero)
                                PosTarget = lastPosNormal;
                        }
                    }
                    if (!cut)
                    {
                        lastPosNormal = Position2D.Zero;
                    }
                    DrawingObjects.AddObject(new StringDraw("time = " + (robotCoeff - ballcoeff).ToString(), new Position2D(5, 2)), "3asf21asf21a3sdsrytwr68hj514trehj");
                    DrawingObjects.AddObject(new Circle(intersectTime, 0.02, new Pen(Brushes.Red, 0.05f)), "ret5w4ert5uh1terj31eyt58kj6tr5j");

                    DrawingObjects.AddObject(new StringDraw((gol.TargetState.Type == ObjectType.Opponent) ? "Robot" : "Ball", new Position2D(4.6, 0.0)), "12345ccxz6789abcdefgha35s21c4");
                    DrawingObjects.AddObject(new Circle(gol.TargetState.Location, 0.15, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.02f)), "as.d02s3d21f3ds21f32ds14f8s2dqfjk6");
                    DrawingObjects.AddObject(new Circle(PosTarget, 0.02, new Pen(Brushes.Blue, 0.05f)), "56cxvcxdv564sd2v44asd32dsd3f54dddss654654");
                    DrawingObjects.AddObject(new StringDraw("cut:" + cut.ToString() + " , " + PosTarget.toString(), new Position2D(5.2, 2)), "5646543sd8sd3vcsd153vsdqwdeqwftry76546ij654");
                    Planner.Add(RobotID, PosTarget, angle, PathType.UnSafe, false, false, false, false);
                }
                else
                {
                    DrawingObjects.AddObject(new StringDraw("Gol == null,exception", new Position2D(5.3, 2)), "564sdv12ds65f3fdsf63654654");
                    Planner.Add(RobotID, rightCenter, angle, PathType.UnSafe, false, false, false, false);
                }
            }
            #endregion

            #region In Penalty Area
            else if (CurrentState == (int)GoalieStates.InPenaltyArea)
            {
                Vector2D ballSpeed = ballStateFast.Speed;
                double v = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - ballStateFast.Location));
                double maxIncomming = 1.5, maxVertical = 11, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
                    + maxVertical;
                double maxSpeedToGet = 0.5;
                double dist, dist2;
                double margin = 0.1;
                double distToBall = ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

                Line ballspeed = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
                Line goalline = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                Position2D? intersect = ballspeed.IntersectWithLine(goalline);
                bool skip = false;
                bool goActive = false;
                bool Gointersect = false;
                Position2D target = new Position2D();

                if (intersect.HasValue)
                {
                    Position2D intersects = intersect.Value;
                    if (((intersects.Y > GameParameters.OurGoalLeft.Y + .15 && intersects.Y < 1.15) || (intersects.Y < GameParameters.OurGoalLeft.Y - .15 && intersects.Y > -1.15)) && Model.BallState.Speed.Size > .3 && Model.BallState.Speed.InnerProduct(GameParameters.OurGoalCenter - Model.BallState.Location) > 0)
                    {
                        skip = true;
                    }
                    else
                    {
                        skip = false;
                    }
                    //if (((intersects.Y < GameParameters.OurGoalLeft.Y + .15 && intersects.Y > 0) || (intersects.Y > GameParameters.OurGoalLeft.Y - .15 && intersects.Y < 0)))
                    //{
                    //    togoal = true;
                    //}
                    //else
                    //{
                    //    togoal = false;
                    //}
                }
                else
                {
                    skip = false;
                }

                if ((acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                {
                    goActive = true;
                }
                else
                {
                    goActive = false;
                }
                if (acceptable2 * 1.2 < ballSpeed.Size)
                {
                    Gointersect = true;
                }
                else
                {
                    Gointersect = false;
                }
                if (skip)
                {
                    DrawingObjects.AddObject(new StringDraw("skip", GameParameters.OurGoalCenter.Extend(0.6, 0)), "5645646465564");
                }
                if (goActive)
                {
                    DrawingObjects.AddObject(new StringDraw("goActive", GameParameters.OurGoalCenter.Extend(0.7, 0)), "654564654565464");
                }
                if (Gointersect)
                {
                    DrawingObjects.AddObject(new StringDraw("gointersect", GameParameters.OurGoalCenter.Extend(0.8, 0)), "987989856654564");
                }
                if (skip)
                {
                    double dist1 = 0;
                    double dist21 = 0;
                    Circle s = new Circle(GameParameters.OurGoalCenter, .5);
                    Line ballrobot = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                    List<Position2D> interscts = s.Intersect(ballrobot);
                    if (interscts.Count > 0)
                    {
                        Position2D? intersectp = interscts.OrderBy(y => y.DistanceFrom(Model.BallState.Location)).FirstOrDefault();
                        if (intersectp.HasValue && GameParameters.IsInDangerousZone(intersectp.Value, false, -.5, out dist1, out dist21) && GameParameters.IsInField(intersectp.Value, 0))
                        {
                            target = new Position2D(Math.Min(intersectp.Value.X, GameParameters.OurGoalCenter.X - .1), intersectp.Value.Y);
                            Planner.Add(RobotID, target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                        else
                        {
                            skip = false;
                        }
                    }
                    else
                    {
                        skip = false;
                    }
                }
                else if (goActive)
                {
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = TargetToKick(Model, RobotID);
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                }
                else if (Gointersect)
                {
                    Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                    List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true);
                    if (intersects.Count > 0)
                    {
                        Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
                        if (GameParameters.IsInField(pos, -.1))
                        {
                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                            Vector2D targetvector = GameParameters.OurGoalCenter - pos;
                            bool gotoperp = false;
                            Planner.ChangeDefaulteParams(RobotID, false);
                            Planner.SetParameter(RobotID, 8, 8);
                            Position2D perp = new Position2D(Math.Min(targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location).X, GameParameters.OurGoalCenter.X - .1), targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location).Y);
                            ;
                            if (perp.DistanceFrom(Model.OurRobots[RobotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
                            {
                                gotoperp = true;
                            }
                            if (gotoperp)
                            {
                                Planner.Add(RobotID, perp, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                            else
                            {
                                Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                        else if (GameParameters.IsInField(Model.BallState.Location, -.1))
                        {
                            pos = Model.BallState.Location;
                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                            Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                        else
                        {
                            pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(Model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                            Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                    }
                    else
                    {
                        Position2D pos = Model.BallState.Location;
                        if (GameParameters.IsInField(pos, -.1))
                        {
                            Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                            Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                            double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                            Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                            Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                    }
                }
                else
                {
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = TargetToKick(Model, RobotID);
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                }
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                Vector2D j = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.35);
                double kickSpeed = 4;
                if (ballState.Location.X > GameParameters.OurGoalCenter.X - 0.1 || Math.Abs(Model.OurRobots[RobotID].Angle.Value) < 100 || obs.Meet(ballState, new SingleObjectState(ballState.Location + j, Vector2D.Zero, 0), 0.022))
                    kickSpeed = 0;
                Planner.AddKick(RobotID, kickPowerType.Speed, kickSpeed, (kickSpeed > 0) ? true : false, false);
            }
            #endregion

            #region Ball In Start of Chip
            else if (CurrentState == (int)GoalieStates.BallInStartOfChip)
            {
                Planner.AddKick(RobotID, true);
                Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
                Planner.Add(RobotID, GameParameters.OurGoalCenter.Extend(-0.15, 0), 180, PathType.UnSafe, false, false, false, false);
            }
            #endregion

            #region Kick To Goal
            else if (CurrentState == (int)GoalieStates.KickToGoal)
            {
                if (Model.BallState.Speed.InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0)
                    GetSkill<GoalieDiveSkill2017>().Dive(engine, Model, RobotID, true, 200);
                else
                {
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = TargetToKick(Model, RobotID);
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                }
            }
            #endregion

            #region Kick To Robot
            else if (CurrentState == (int)GoalieStates.KickToRobot) //Kick to robot
            {
                Planner.Add(RobotID, Model.OurRobots[RobotID].Location, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false);
                Planner.AddKick(RobotID, true);
                Planner.AddKick(RobotID, kickPowerType.Speed, true, 2.5);
            }
            #endregion
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            //if (Model.BallState.Location.DistanceFrom(ballSavedForStates) < .2 || (Model.BallState.Location - ballInitialState).InnerProduct(Model.BallState.Location - Model.BallFallingPoint) > 0 || Model.BallState.Speed.Size < .5)
            //{
            //    falled = true;
            //}
            if (!GameParameters.IsInField(Model.BallState.Location, 0.05))
                CurrentState = (int)GoalieStates.Normal;
            else
            {
                Vector2D ballSpeed = Model.BallStateFast.Speed;
                double v = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - Model.BallStateFast.Location));
                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
                    + maxVertical;
                double maxSpeedToGet = 0.5;
                double dist, dist2;
                double margin = 0.1;
                double distToBall = Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

                double innerProduct = Vector2D.InnerProduct(Model.BallStateFast.Speed, (Model.OurRobots[RobotID].Location - Model.BallStateFast.Location));
                double difAngle = Vector2D.AngleBetweenInDegrees(Model.BallStateFast.Speed, (Model.BallStateFast.Location - Model.OurRobots[RobotID].Location));

                Circle c = new Circle(Model.OurRobots[RobotID].Location, 0.12);
                Line l = new Line(Model.BallStateFast.Location, Model.BallStateFast.Location + Model.BallStateFast.Speed);
                List<Position2D> inters = c.Intersect(l);

                #region Normal
                if (CurrentState == (int)GoalieStates.Normal)
                {
                    if (firsttime)
                    {
                        ballInitialState = Model.BallState.Location;
                        firsttime = false;
                    }
                    if (Model.BallState.Speed.Size > .1)
                        Ball.Add(Model.BallState.Location);
                    List<int> RobotIds = Model.Opponents.OrderBy(i => i.Value.Location.DistanceFrom(ballInitialState)).Select(p => p.Key).ToList();
                    Vector2D ballFirstBallState = new Vector2D();
                    if (RobotIds.Count > 0)
                    {
                        int robotBackBall = Model.Opponents.OrderBy(y => y.Value.Location.DistanceFrom(ballInitialState)).Select(t => t.Key).FirstOrDefault();
                        angles.Add(Model.Opponents[robotBackBall].Angle.Value);
                        robotAgles.Add(Model.Opponents[RobotIds[0]].Angle.Value);
                        ballFirstBallState = Model.BallState.Location - Model.Opponents[RobotIds[0]].Location;
                        DrawingObjects.AddObject(new StringDraw("ekhtelafe zavie: " + (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(robotAgles.Last().ToRadian(), 1), ballFirstBallState))).ToString(), GameParameters.OurRightCorner.Extend(0.3, 0)), "dsfdsf3232dfdsfas328re");
                        DrawingObjects.AddObject(new StringDraw("CounterInChip: " + CounterInChip.ToString(), GameParameters.OurRightCorner.Extend(0.2, 0)), "dsfdsf3564232dfdsfas328re");

                        if (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(robotAgles.Last().ToRadian(), 1), ballFirstBallState)) > 7 && Model.BallState.Location.DistanceFrom(ballInitialState) > .07 && Model.BallState.Location.DistanceFrom(ballInitialState) < .2 && ballInitialState.X > 3.5 && CounterInChip < counterISChipStart)
                        {
                            if (onceaTime)
                            {
                                initialAngle = angles[angles.Count() - frameOfChipInEllipseEnvironment];
                                onceaTime = false;
                            }
                            CurrentState = (int)GoalieStates.BallInStartOfChip;
                            //WeHadBallInStartOfchip = true;
                        }
                        if (Model.BallState.Speed.Size < .1)
                        {
                            ballinRoll = false;
                        }
                    }
                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                        CurrentState = (int)GoalieStates.InPenaltyArea;

                }
                #endregion

                #region In Penalty Area
                else if (CurrentState == (int)GoalieStates.InPenaltyArea)
                {
                    Reset();
                    margin = 0.2;
                    if (BallKickedToOurGoal(Model) &&
                        (!GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2)
                        || acceptableballRobotSpeed * 1.2 < ballSpeed.Size))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (!GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) || acceptable2 * 1.2 < ballSpeed.Size)
                        CurrentState = (int)GoalieStates.Normal;
                }
                #endregion

                #region Kick To Goal
                else if (CurrentState == (int)GoalieStates.KickToGoal)
                {
                    Reset();
                    margin = 0.1;
                    if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (!BallKickedToOurGoal(Model) && GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if (!BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.Normal;
                }
                #endregion

                #region Kick To Robot
                else if (CurrentState == (int)GoalieStates.KickToRobot)
                {
                    Reset();
                    if (ballSpeed.Size < 1.5 || BallKickedToOurGoal(Model) || inters.Count == 0 || innerProduct < -0.1)
                    {
                        if (BallKickedToOurGoal(Model))
                            CurrentState = (int)GoalieStates.KickToGoal;
                        else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist, out dist2) && (acceptable2 < ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                            CurrentState = (int)GoalieStates.InPenaltyArea;
                        else
                            CurrentState = (int)GoalieStates.Normal;
                    }
                }
                #endregion

                #region Ball in Start of Chip
                else if (CurrentState == (int)GoalieStates.BallInStartOfChip)
                {
                    CounterInChip++;
                    if (CounterInChip > counterISChipStart)
                    {
                        CurrentState = (int)GoalieStates.Normal;
                    }
                }
                #endregion
            }
            FreekickDefence.CurrentStates[this] = CurrentState;
            currentState = CurrentState;
            DrawingObjects.AddObject(new StringDraw(((GoalieStates)CurrentState).ToString(), GameParameters.OurGoalCenter.Extend(0.3, 0)), "gstate");
        }

        public bool BallKickedToOurGoal(WorldModel Model)
        {
            SingleObjectState BallState = Model.BallState;
            double tresh = 0.25;
            double tresh2 = 1.3;
            if ((GoalieStates)currentState == GoalieStates.KickToGoal)
            {
                tresh = 0.3;
                tresh2 = 1.4;
            }
            Line line = new Line();
            line = new Line(BallState.Location, BallState.Location - BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            DrawingObjects.AddObject(new StringDraw((d / BallState.Speed.Size < tresh2).ToString(), new Position2D(-1, 0)));
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + tresh && BallGoal.Y > GameParameters.OurGoalRight.Y - tresh)
                if (BallState.Speed.InnerProduct(GameParameters.OurGoalRight - BallState.Location) > 0)
                    if (BallState.Speed.Size > 0.1 && d / BallState.Speed.Size < tresh2)
                        return true;
            return false;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 20 * RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new GoalieNormalRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public Position2D TargetToKick(WorldModel Model, int robotID)
        {
            Vector2D v = ballState.Location - GameParameters.OurGoalCenter;
            v = Vector2D.FromAngleSize(Math.Sign(v.AngleInRadians) * Math.Max(Math.Abs(v.AngleInRadians), (110.0).ToRadian()), v.Size);
            return ballState.Location + v.GetNormalizeToCopy(2);
        }

        private int sengmentNumber(Position2D robotInitialPos, Position2D ballfallingPoint, Vector2D firstVector, Vector2D SecondVector, Vector2D robotAngle)
        {
            double forFirstSegment = .3;
            double x, y;
            Line sevenSegment = new Line(new Position2D(RobotLoc.X - .07, GameParameters.OurLeftCorner.Y), new Position2D(RobotLoc.X - .07, GameParameters.OurRightCorner.Y));
            Line fallline = new Line(new Position2D(ballfallingPoint.X, GameParameters.OurLeftCorner.Y), new Position2D(ballfallingPoint.X, GameParameters.OurRightCorner.Y), new Pen(Brushes.Red, .01f));

            if (GameParameters.IsInDangerousZone(ballSavedForStates, false, 0, out x, out y))
            {
                if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) > 0 && ballfallingPoint.X > RobotLoc.X - .07)
                {
                    return 7;
                }
                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) > 0)
                {
                    return 1;
                }
                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) > 0)
                {
                    return 6;
                }
                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) < 0)
                {
                    return 3;
                }
                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) + forFirstSegment && (ballInitialState - robotInitialPos).InnerProduct(ballSavedForStates - robotInitialPos) < 0)
                {
                    return 4;
                }
                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) < Math.Abs(robotInitialPos.Y) + forFirstSegment && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) - forFirstSegment)
                {
                    return 2;
                }
                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(ballSavedForStates.Y) < Math.Abs(robotInitialPos.Y) + forFirstSegment && Math.Abs(ballSavedForStates.Y) > Math.Abs(robotInitialPos.Y) - forFirstSegment)
                {
                    return 5;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        private int segmentNumber(WorldModel model, Position2D input, Vector2D robotAngle, int RobotID)
        {
            Vector2D SecondVector = GameParameters.OurGoalCenter.Extend(0, Math.Sign(ballInitialState.Y) * 0.975) - ballInitialState;
            Vector2D firstVector = new Position2D(2.60, 0.00) - ballInitialState;
            Position2D InfrontOfGoal = GameParameters.OurGoalCenter.Extend(-0.10, 0.00);
            double forFirstSegment = 0.30;
            double x, y;
            Position2D RobotLoc = model.OurRobots[RobotID].Location;
            if (GameParameters.IsInDangerousZone(input, false, 0.00, out x, out y))
            {
                if (input.X > 3)
                {
                    return 0;
                }
                if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) > 0 && input.X > RobotLoc.X - 0.07)
                {
                    return 7;
                }
                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) > 0)
                {
                    return 1;
                }
                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) > 0)
                {
                    return 6;
                }
                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) < 0)
                {
                    return 3;
                }
                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) > Math.Abs(InfrontOfGoal.Y) + forFirstSegment && (model.BallState.Location - InfrontOfGoal).InnerProduct(input - InfrontOfGoal) < 0)
                {
                    return 4;
                }
                else if (IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) < Math.Abs(InfrontOfGoal.Y) + forFirstSegment && Math.Abs(model.BallState.Location.Y) > Math.Abs(InfrontOfGoal.Y) - forFirstSegment)
                {
                    return 2;
                }
                else if (!IsBetween(firstVector, SecondVector, robotAngle) && Math.Abs(input.Y) < Math.Abs(InfrontOfGoal.Y) + forFirstSegment && Math.Abs(model.BallState.Location.Y) > Math.Abs(InfrontOfGoal.Y) - forFirstSegment)
                {
                    return 5;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        private bool IsBetween(Vector2D firstVector, Vector2D secondvector, Vector2D robotAngle)
        {
            if (ballInitialState.Y < 0)
            {
                //first vector have bigger angle
                double angle = robotAngle.AngleInRadians;
                if (angle < firstVector.AngleInRadians && angle > secondvector.AngleInRadians)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //second vector have bigger angle
                double angle = robotAngle.AngleInRadians;

                List<int> f = new List<int>();


                if (angle < secondvector.AngleInRadians && angle > firstVector.AngleInRadians)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        //
        private bool firstTime3 = true;
        public static Position2D lasttargetPoint = new Position2D();
        private int counter = 0;
        int Deccelcounter = 0;
        Position2D lasttarget = new Position2D();
        Position2D lastinitpos = new Position2D();

        // time from current pos
        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos)
        {
            Position2D initialstate = initialpos;
            Position2D target = lastpos;

            if (firstTime3)
            {
                firstTime3 = false;
                lasttargetPoint = lastpos;
            }
            if (target.DistanceFrom(lasttargetPoint) > .05)
            {
                counter = 0;
                Deccelcounter = 0;
                firstTime3 = true;
            }
            Position2D currentPos = Model.OurRobots[RobotID].Location;
            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);
            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
            double ta = root(3.14, Va, daccel - Model.OurRobots[RobotID].Location.DistanceFrom(initialstate));
            double tc = (Model.OurRobots[RobotID].Location.DistanceFrom(target) - deccelDX) / vmax;
            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
            double Td = Math.Min(.850, vmax / 3.04);
            double total = 0;


            counter++;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
            {
                Deccelcounter++;
            }
            if (Deccelcounter > 10)
            {
                int g = 0;
            }
            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
            {

                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //1
                    total = ta + tc2 + Td;
                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
                }
                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
                {
                    //4
                    total = tc + Td;
                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
                }
                if (currentPos.DistanceFrom(target) < deccelDX)
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
                }
            }
            else
            {
                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //2
                    total = ta + Td;
                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
                }
                else
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
                }
            }
            lasttarget = target;
            lastinitpos = initialstate;
            return total;

        }
        // total time of path
        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos, bool inittarget)
        {
            Position2D initialstate = initialpos;
            Position2D target = lastpos;

            if (firstTime3)
            {
                firstTime3 = false;
                lasttargetPoint = lastpos;
            }
            if (target.DistanceFrom(lasttargetPoint) > .05)
            {
                counter = 0;
                Deccelcounter = 0;
                firstTime3 = true;
            }
            Position2D currentPos = initialpos;
            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);
            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
            double ta = root(3.14, Va, daccel - currentPos.DistanceFrom(initialstate));
            double tc = (currentPos.DistanceFrom(target) - deccelDX) / vmax;
            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
            double Td = Math.Min(.850, vmax / 3.04);
            double total = 0;


            counter++;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
            {
                Deccelcounter++;
            }
            if (Deccelcounter > 10)
            {
                int g = 0;
            }
            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
            {

                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //1
                    total = ta + tc2 + Td;
                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
                }
                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
                {
                    //4
                    total = tc + Td;
                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
                }
                if (currentPos.DistanceFrom(target) < deccelDX)
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
                }
            }
            else
            {
                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //2
                    total = ta + Td;
                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
                }
                else
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
                }
            }
            lasttarget = target;
            lastinitpos = initialstate;
            return total;

        }
        // time of arriving to intersect when we want to stop in the intersect pos
        double predicttime(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
        {
            Position2D currentPos = model.OurRobots[RobotID].Location;
            double deccelDX = Math.Min(1.09, .54 * init.DistanceFrom(target));
            double accelDx = Math.Min(0.942, .46 * init.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * accelDx);
            double adeccel = 3.04;
            double aaccel = 3.14;

            double deltaXIntersectTarget = intersect.DistanceFrom(target);

            double coeff1 = deltaXIntersectTarget / deccelDX;
            double v0deccel = vmax * coeff1;
            double tTemp = v0deccel / adeccel;
            double tdeccel = predicttime(model, RobotID, init, target, true) - tTemp;

            double deltaxInitialIntersect = init.DistanceFrom(intersect);
            double coeff2 = deltaxInitialIntersect / accelDx;

            double V0accel = coeff2 * vmax;
            double taccel = V0accel / aaccel;

            double tcruise = ((deltaxInitialIntersect - accelDx) / vmax) + (vmax / accelDx);

            double deltaXInitialTarget = init.DistanceFrom(target);
            double ttotal = 0;
            if (deltaXIntersectTarget < accelDx + deccelDX) // Accel - Deccel
            {
                if (deltaXIntersectTarget > deccelDX)
                {
                    ttotal = taccel;
                }
                else
                {
                    ttotal = tdeccel;
                }
            }
            else // Accel - Cruise - Deccel
            {
                if (deltaxInitialIntersect < accelDx)
                {
                    ttotal = taccel;
                }
                else if (deltaXIntersectTarget < deccelDX)
                {
                    ttotal = tdeccel;
                }
                else
                {
                    ttotal = tcruise;
                }
            }
            return ttotal;
        }
        // time of arriving to intersect when we dont to stop in the intersect pos
        double timeRobotToTargetInIntersect(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
        {
            double timeInittarget = predicttime(model, RobotID, init, target, true);
            double timeInitIntersect = predicttime(model, RobotID, init, target, intersect);
            double timeIntersecttarget = timeInittarget - timeInitIntersect;
            double timeRobotTarget = predicttime(model, RobotID, init, target);
            double timeRobotIntesect = timeRobotTarget - timeIntersecttarget;
            return timeRobotIntesect;
        }
        private Position2D IntersectFind(WorldModel model, int RobotID, Position2D initpoint, Position2D target)
        {
            Position2D robotSpeedPos = model.OurRobots[RobotID].Location + model.OurRobots[RobotID].Speed;
            Position2D ballspeedpos = model.BallState.Location + model.BallState.Speed;

            double x4 = target.X;

            double x3 = initpoint.X;
            double y4 = target.Y;
            double y3 = initpoint.Y;
            double x2 = ballspeedpos.X;
            double y2 = ballspeedpos.Y;
            double x1 = model.BallState.Location.X;
            double y1 = model.BallState.Location.Y;
            //double x = (((((x1 * y2) - (y1 * x2)) * (x3 - x4)) - ((x1 - x2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));
            //double y = (((((x1 * y2) - (y1 * x2)) * (y3 - y4)) - ((y1 - y2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));

            Line first = new Line(new Position2D(x1, y1), new Position2D(x2, y2));
            Line second = new Line(new Position2D(x3, y3), new Position2D(x4, y4));
            Position2D intersect = new Position2D();
            if (first.IntersectWithLine(second).HasValue)
                intersect = first.IntersectWithLine(second).Value;
            else
            {
                intersect = new Position2D(100, 100);
            }
            return intersect;
        }
        static double root(double a, double initialV, double deltaX)
        {
            double t = 0;
            double delta = (initialV * initialV) - (2 * a * -deltaX);
            if (delta == 0)
            {
                t = -initialV / (.5 * a);
            }
            if (delta > 0)
            {

                double t1 = (-initialV - Math.Sqrt(delta)) / a;
                double t2 = (-initialV + Math.Sqrt(delta)) / a;
                if (t2 > 0 && t1 < 0)
                    t = t2;
                else if (t1 > 0 && t2 < 0)
                    t = t1;
                else if (t1 > 0 && t2 > 0)
                    if (t1 < t2)
                        t = t1;
                    else
                        t = t2;
            }
            if (delta < 0)
                return -1000;
            return t;
        }



        public void Reset()
        {
            onceaTime = true;
            onceaTime2 = true;
            firstTimeBallInChipKick = true;
            ballSavedForStates = new Position2D();
            ballSavedPos = new Position2D();
            RobotLoc = new Position2D();
        }

        public enum GoalieStates
        {
            Normal = 0,
            InPenaltyArea = 1,
            KickToGoal = 2,
            KickToRobot = 3,
            BallInStartOfChip = 5,
        }

        public enum ChipState
        {
            Left,
            Right
        }

    }
}
