using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Skills.GoalieSkills
{
    public class GoaliDiveSkill : SkillBase
    {

        int index = 0;
        int dataCount = 30;
        Line BallLine = new Line();
        private Position2D Vision2AI(Position2D pos, bool isReverseSide)
        {
            if (isReverseSide)
                return new Position2D(-pos.X / 1000, pos.Y / 1000);
            return new Position2D(pos.X / 1000, -pos.Y / 1000);
        }
        bool firstDirect = true, firstChip = true;
        Position2D firstBallPos = Position2D.Zero;
        Dictionary<int, Position2D> ballHistoryList = new Dictionary<int, Position2D>();
        public GoaliDiveSkill()
        {
            //Controller = new Controller();
        }
        public SingleWirelessCommand Dive(GameStrategyEngine engine, WorldModel Model, int RobotID, bool isChipKick, double kickPower, bool intercept = false)
        {
            DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, 0.2)
            {
                DrawPen = new System.Drawing.Pen(System.Drawing.Color.RosyBrown, 0.02f)
            }, "Dive");
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLoc = Model.BallState.Location, robotLoc = Model.OurRobots[RobotID].Location;
            Position2D tmp = ballLoc + ballSpeed.GetNormalizeToCopy(10);
            //DrawingObjects.AddObject(new Line(ballLoc, tmp), "5654646456564656");
            Line L1 = new Line(tmp, ballLoc);
            Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
            Position2D? intWithGoalLine = L1.IntersectWithLine(goalLine);
            Position2D? posToGo = null;

            if (intWithGoalLine.HasValue)
            {
                if (robotLoc.Y > intWithGoalLine.Value.Y)
                    if (robotLoc.Y < 0 && intWithGoalLine.Value.Y < GameParameters.OurGoalRight.Y)
                    {
                        if (robotLoc.DistanceFrom(GameParameters.OurGoalRight) < 0.2)
                        {
                            DrawingObjects.AddObject(new Circle(GameParameters.OurGoalRight.Extend(-0.12, 0.02), .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "34564564664564654");
                            Planner.Add(RobotID, GameParameters.OurGoalRight.Extend(-0.12, 0.02), (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                            return new SingleWirelessCommand();
                        }
                    }

                if (robotLoc.Y < intWithGoalLine.Value.Y)
                    if (robotLoc.Y > 0 && intWithGoalLine.Value.Y > GameParameters.OurGoalLeft.Y)
                    {
                        if (robotLoc.DistanceFrom(GameParameters.OurGoalLeft) < 0.2)
                        {
                            DrawingObjects.AddObject(new Circle(GameParameters.OurGoalRight.Extend(-0.12, -0.02), .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "564564654646458");
                            Planner.Add(RobotID, GameParameters.OurGoalRight.Extend(-0.12, -0.02), (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                            return new SingleWirelessCommand();
                        }
                    }

                Line tmpL = L1.PerpenducilarLineToPoint(robotLoc);
                Position2D? pp = tmpL.IntersectWithLine(L1);
                Position2D? poss = L1.IntersectWithLine(new Line(robotLoc, robotLoc + new Vector2D(0, 0.1)));
                if (!poss.HasValue)
                    poss = ballLoc;
                if (pp.Value.X < robotLoc.X)
                {
                    double tb1 = pp.Value.DistanceFrom(ballLoc) / ballSpeed.Size;
                    double tb2 = poss.Value.DistanceFrom(ballLoc) / ballSpeed.Size;
                    double tr1 = CalculateTime(Model.OurRobots[RobotID], pp.Value, 2.2, 3.3);
                    double tr2 = CalculateTime(Model.OurRobots[RobotID], poss.Value, 2.2, 3.3);
                    if (tr1 <= tb1)
                    {
                        Vector2D tmpv = pp.Value - poss.Value;
                        tmpv.NormalizeTo(tmpv.Size * 0.2);
                        posToGo = poss.Value + tmpv;
                    }
                    else
                        posToGo = poss.Value;
                }
                else
                    posToGo = poss.Value;
                double a = 6;
                Position2D posToGo2 = posToGo.Value + (posToGo.Value - robotLoc).GetNormalizeToCopy(10);
                if (posToGo.Value.DistanceFrom(robotLoc) < 0.5)
                {
                    double timeR = CalculateTime(Model.OurRobots[RobotID], posToGo.Value, 2.2, 3.3);
                    double timeB = posToGo.Value.DistanceFrom(ballLoc) / ballSpeed.Size;
                    if (timeR == 0 && Model.OurRobots[RobotID].Speed.Size < .5)
                        a = 0;

                    else if (timeR < timeB)
                    {
                        a = CalculateAccel(Model.OurRobots[RobotID], posToGo.Value, timeB, 3.3);

                        if (a < 0)
                        {
                            posToGo2 = new Position2D(posToGo2.X, -posToGo2.Y);
                            a *= -1;
                        }
                    }

                    DrawingObjects.AddObject(new Circle(posToGo2, .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "4654464546645");
                    DrawingObjects.AddObject(new StringDraw("posToGo2:" + posToGo2.toString(), new Position2D(4.43, 0)), "879889797987987");
                    DrawingObjects.AddObject(new StringDraw("a:" + a.ToString(), new Position2D(4.6, 0)), "6546549844564");
                    DrawingObjects.AddObject(new StringDraw("timeRs:" + timeR.ToString(), new Position2D(4.7, 0)), "65546546464654");
                    DrawingObjects.AddObject(new StringDraw("timeBs:" + timeB.ToString(), new Position2D(4.8, 0)), "3132131256465456");
                    DrawingObjects.AddObject(new StringDraw("Robot Speed:" + Model.OurRobots[RobotID].Speed.Size.ToString(), new Position2D(5.1, 0)), "654654646546");
                    DrawingObjects.AddObject(new StringDraw("Ball Speed Fast:" + Model.BallState.Speed.Size.ToString(), new Position2D(5, 0)), "365565646546546546");
                    DrawingObjects.AddObject(new StringDraw("Ball Speed :" + Model.BallState.Speed.Size.ToString(), new Position2D(4.9, 0)), "312321356465465456");

                    if (timeR < timeB)
                    {
                        Line ballspeed = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                        Line goalline = new Line(Model.OurRobots[RobotID].Location.Extend(0, -1), Model.OurRobots[RobotID].Location.Extend(0, 1));
                        Position2D intersects = ballspeed.IntersectWithLine(goalline).Value;
                        if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 2)
                        {
                            intersects = intersects + (intersects - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(.05);
                        }
                        Planner.Add(RobotID, intersects, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        return new SingleWirelessCommand();
                    }
                    else
                    {
                        Planner.ChangeDefaulteParams(RobotID, false);
                        Planner.SetParameter(RobotID, a, 5);
                    }
                }

                double timeRs = CalculateTime(Model.OurRobots[RobotID], posToGo.Value, 2.2, 3.3);
                double timeBs = posToGo.Value.DistanceFrom(ballLoc) / ballSpeed.Size;


                var detectedBall = DetectBallPos(Model);
                Position2D extendedIntersect = detectedBall;

                if (firstDirect)
                {
                    firstBallPos = detectedBall;
                    firstDirect = false;
                }
                else
                {
                    ballHistoryList.Add(index++, detectedBall);
                    if (ballHistoryList.Count > dataCount)
                    {
                        int minkey = ballHistoryList.Keys.OrderBy(o => o).First();
                        ballHistoryList.Remove(minkey);
                    }
                    int k = predictBallPath();
                    Position2D posFinal = firstBallPos;
                    if (ballHistoryList.ContainsKey(k))
                    {
                        posFinal = ballHistoryList[k];
                    }

                    BallLine = new Line(firstBallPos, posFinal) { DrawPen = new Pen(Color.Red, 0.02f) };

                }


                DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + (BallLine.Tail - BallLine.Head).GetNormalizeToCopy(10), new Pen(Brushes.HotPink, .02f)), "987564654531312");



                Planner.Add(RobotID, posToGo2, Model.OurRobots[RobotID].Angle.Value, false);
                //SWC = Controller.CalculateTargetSpeed(Model, RobotID, posToGo2, Model.OurRobots[RobotID].Angle.Value, null);
                Planner.AddKick(RobotID, kickPowerType.Speed, isChipKick, 3);


                //SWC.isChipKick = isChipKick;
                //SWC.KickPower = kickPower;
                //SWC.BackSensor = false;
            }
            return SWC;
        }
        public SingleWirelessCommand NewDive(GameStrategyEngine engine, WorldModel Model, int RobotID, bool isChipKick, double kickPower, bool intercept = false)
        {
            DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, 0.2)
            {
                DrawPen = new System.Drawing.Pen(System.Drawing.Color.RosyBrown, 0.02f)
            }, "Dive");
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLoc = Model.BallState.Location, robotLoc = Model.OurRobots[RobotID].Location;
            Position2D tmp = ballLoc + ballSpeed.GetNormalizeToCopy(10);
            //DrawingObjects.AddObject(new Line(ballLoc, tmp), "5654646456564656");
            Line L1 = new Line(tmp, ballLoc);
            Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
            Position2D? intWithGoalLine = L1.IntersectWithLine(goalLine);
            Position2D? posToGo = null;

            if (intWithGoalLine.HasValue)
            {
                if (robotLoc.Y > intWithGoalLine.Value.Y)
                    if (robotLoc.Y < 0 && intWithGoalLine.Value.Y < GameParameters.OurGoalRight.Y)
                    {
                        if (robotLoc.DistanceFrom(GameParameters.OurGoalRight) < 0.2)
                        {
                            DrawingObjects.AddObject(new Circle(GameParameters.OurGoalRight.Extend(-0.12, 0.02), .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "34564564664564654");
                            Planner.Add(RobotID, GameParameters.OurGoalRight.Extend(-0.12, 0.02), (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                            return new SingleWirelessCommand();
                        }
                    }

                if (robotLoc.Y < intWithGoalLine.Value.Y)
                    if (robotLoc.Y > 0 && intWithGoalLine.Value.Y > GameParameters.OurGoalLeft.Y)
                    {
                        if (robotLoc.DistanceFrom(GameParameters.OurGoalLeft) < 0.2)
                        {
                            DrawingObjects.AddObject(new Circle(GameParameters.OurGoalRight.Extend(-0.12, -0.02), .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "564564654646458");
                            Planner.Add(RobotID, GameParameters.OurGoalRight.Extend(-0.12, -0.02), (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                            return new SingleWirelessCommand();
                        }
                    }

                Line tmpL = L1.PerpenducilarLineToPoint(robotLoc);
                //DrawingObjects.AddObject(new Line(L1.Head, L1.Tail, new Pen(Brushes.Yellow, 0.02f)), L1.Head.Y.ToString() + "5364+6126");
                Position2D? pp = tmpL.IntersectWithLine(L1);
                DrawingObjects.AddObject(new Circle(pp.Value, 0.03, new Pen(Brushes.Red, 0.02f), true, 1f,false), pp.Value.X.ToString() + "9856.25");
                Position2D? poss = L1.IntersectWithLine(new Line(robotLoc, robotLoc + new Vector2D(0, 0.1)));
                DrawingObjects.AddObject(new Circle(poss.Value, 0.03, new Pen(Brushes.BlueViolet, 0.02f), true, 1f, false), poss.Value.X.ToString() + "9856563435425");

                if (!poss.HasValue)
                    poss = ballLoc;
                if (pp.Value.X < robotLoc.X)
                {
                    double tb1 = pp.Value.DistanceFrom(ballLoc) / ballSpeed.Size;
                    double tb2 = poss.Value.DistanceFrom(ballLoc) / ballSpeed.Size;
                    double tr1 = CalculateTime(Model.OurRobots[RobotID], pp.Value, 2.2, 3.3);
                    double tr2 = CalculateTime(Model.OurRobots[RobotID], poss.Value, 2.2, 3.3);
                    if (tr1 <= tb1)
                    {
                        Vector2D tmpv = pp.Value - poss.Value;
                        DrawingObjects.AddObject(new Line(pp.Value, poss.Value, new Pen(Brushes.Yellow, 0.02f)), pp.Value.Y.ToString() + "5364+6126");
                        tmpv.NormalizeTo(tmpv.Size * 0.2);
                        DrawingObjects.AddObject(new StringDraw("pp to poss dist: " + (pp.Value.DistanceFrom(poss.Value).ToString()), new Position2D(3, -2)), poss.Value.Y.ToString() + "998446");
                        DrawingObjects.AddObject(new StringDraw("tmpv dist: " + tmpv.Size.ToString(), new Position2D(3.1, -2)), tmpv.Size.ToString() + "357196537");
                        posToGo = poss.Value + tmpv;
                    }
                    else
                        posToGo = poss.Value;
                }
                else
                    posToGo = poss.Value;
                double a = 6;
                Position2D posToGo2 = posToGo.Value + (posToGo.Value - robotLoc).GetNormalizeToCopy(10);
                if (posToGo.Value.DistanceFrom(robotLoc) < 0.5)
                {
                    double timeR = CalculateTime(Model.OurRobots[RobotID], posToGo.Value, 2.2, 3.3);
                    double timeB = posToGo.Value.DistanceFrom(ballLoc) / ballSpeed.Size;
                    if (timeR == 0 && Model.OurRobots[RobotID].Speed.Size < .5)
                        a = 0;

                    else if (timeR < timeB)
                    {
                        a = CalculateAccel(Model.OurRobots[RobotID], posToGo.Value, timeB, 3.3);

                        if (a < 0)
                        {
                            posToGo2 = new Position2D(posToGo2.X, -posToGo2.Y);
                            a *= -1;
                        }
                    }

                    DrawingObjects.AddObject(new Circle(posToGo2, .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "4654464546645");
                    DrawingObjects.AddObject(new StringDraw("posToGo2:" + posToGo2.toString(), new Position2D(4.43, 0)), "879889797987987");
                    DrawingObjects.AddObject(new StringDraw("a:" + a.ToString(), new Position2D(4.6, 0)), "6546549844564");
                    DrawingObjects.AddObject(new StringDraw("timeRs:" + timeR.ToString(), new Position2D(4.7, 0)), "65546546464654");
                    DrawingObjects.AddObject(new StringDraw("timeBs:" + timeB.ToString(), new Position2D(4.8, 0)), "3132131256465456");
                    DrawingObjects.AddObject(new StringDraw("Robot Speed:" + Model.OurRobots[RobotID].Speed.Size.ToString(), new Position2D(5.1, 0)), "654654646546");
                    DrawingObjects.AddObject(new StringDraw("Ball Speed Fast:" + Model.BallState.Speed.Size.ToString(), new Position2D(5, 0)), "365565646546546546");
                    DrawingObjects.AddObject(new StringDraw("Ball Speed :" + Model.BallState.Speed.Size.ToString(), new Position2D(4.9, 0)), "312321356465465456");

                    if (timeR < timeB)
                    {
                        Line ballspeed = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                        Line goalline = new Line(Model.OurRobots[RobotID].Location.Extend(0, -1), Model.OurRobots[RobotID].Location.Extend(0, 1));
                        Position2D left, right;
                        Line targetToCenterGoal = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                        Line perp = targetToCenterGoal.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                        Position2D? intersect = perp.IntersectWithLine(targetToCenterGoal);
                        if (intersect.HasValue)
                        {
                            left = intersect.Value + (Model.OurRobots[RobotID].Location - intersect.Value).GetNormalizeToCopy(1);
                            right = intersect.Value + (intersect.Value - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(1);
                            goalline = new Line(left, right);
                        }
                        DrawingObjects.AddObject(new Line(goalline.Head, goalline.Tail, new Pen(Brushes.Silver, 0.02f)), "687864654");
                        Position2D intersects = ballspeed.IntersectWithLine(goalline).Value;
                        if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 2)
                        {
                            intersects = intersects + (intersects - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(.05);
                        }
                        Planner.Add(RobotID, intersects, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        return new SingleWirelessCommand();
                    }
                    else
                    {
                        Planner.ChangeDefaulteParams(RobotID, false);
                        Planner.SetParameter(RobotID, a, 5);
                    }
                }

                double timeRs = CalculateTime(Model.OurRobots[RobotID], posToGo.Value, 2.2, 3.3);
                double timeBs = posToGo.Value.DistanceFrom(ballLoc) / ballSpeed.Size;


                var detectedBall = DetectBallPos(Model);
                Position2D extendedIntersect = detectedBall;

                if (firstDirect)
                {
                    firstBallPos = detectedBall;
                    firstDirect = false;
                }
                else
                {
                    ballHistoryList.Add(index++, detectedBall);
                    if (ballHistoryList.Count > dataCount)
                    {
                        int minkey = ballHistoryList.Keys.OrderBy(o => o).First();
                        ballHistoryList.Remove(minkey);
                    }
                    int k = predictBallPath();
                    Position2D posFinal = firstBallPos;
                    if (ballHistoryList.ContainsKey(k))
                    {
                        posFinal = ballHistoryList[k];
                    }

                    BallLine = new Line(firstBallPos, posFinal) { DrawPen = new Pen(Color.Red, 0.02f) };

                }


                DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + (BallLine.Tail - BallLine.Head).GetNormalizeToCopy(10), new Pen(Brushes.HotPink, .02f)), "987564654531312");



                Planner.Add(RobotID, posToGo2, Model.OurRobots[RobotID].Angle.Value, false);
                //SWC = Controller.CalculateTargetSpeed(Model, RobotID, posToGo2, Model.OurRobots[RobotID].Angle.Value, null);
                Planner.AddKick(RobotID, kickPowerType.Speed, isChipKick, 3);
                if (posToGo.HasValue)
                {
                    DrawingObjects.AddObject(new Circle(posToGo.Value, 0.03, new Pen(Brushes.Aqua, 0.02f), true, 1f, false), posToGo.Value.X.ToString() + "5646654654");
                    DrawingObjects.AddObject(new Circle(posToGo2, 0.03, new Pen(Brushes.Aquamarine, 0.02f), true, 1f, false), posToGo2.X.ToString() + "9856.25");
                }
                //SWC.isChipKick = isChipKick;
                //SWC.KickPower = kickPower;
                //SWC.BackSensor = false;
            }
            return SWC;
        }


        private Position2D DetectBallPos(WorldModel Model)
        {
            double minDist = double.MaxValue;
            Position2D ballPos = new Position2D();
            foreach (var item in StaticVariables.BallPositions)
            {
                if (Vision2AI(item, Model.FieldIsInverted).DistanceFrom(Model.BallState.Location) < minDist)
                {
                    ballPos = Vision2AI(item, Model.FieldIsInverted);
                    minDist = Vision2AI(item, Model.FieldIsInverted).DistanceFrom(Model.BallState.Location);
                }
            }
            return ballPos;
        }

        private int predictBallPath()
        {
            Dictionary<int, double> PathScore = new Dictionary<int, double>();
            foreach (var id1 in ballHistoryList.Keys)
            {
                {
                    double score = 0;
                    foreach (var id2 in ballHistoryList.Keys)
                    {
                        score += Vector2D.offset_to_line(firstBallPos, ballHistoryList[id1], ballHistoryList[id2]);
                    }
                    PathScore.Add(id1, score);
                }
            }
            double minScore = double.MaxValue;
            int bestPathPointKey = -1;
            foreach (var item in PathScore.Keys)
            {
                double v = PathScore[item];
                if (v < minScore)
                {
                    minScore = v;
                    bestPathPointKey = item;
                }
            }
            return bestPathPointKey;
        }


        private double CalculateTime(SingleObjectState RobotState, Position2D Target, double maxAccel, double maxSpeed)
        {
            Vector2D vec = Target - RobotState.Location;
            double d = vec.Size;
            double angle = Vector2D.AngleBetweenInRadians(vec, RobotState.Speed);
            double SpeedInRefrence = RobotState.Speed.Size * Math.Cos(angle);
            double time = 0;

            double deccelDX = Math.Min(1, .5 * RobotState.Location.DistanceFrom(Target));
            double daccel = Math.Min(1, .5 * RobotState.Location.DistanceFrom(Target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);

            double rootp = root(2.2, Math.Abs(RobotState.Speed.Size), deccelDX);
            if (rootp > 0)
            {
                time = rootp * 2;
                return time;
            }
            //if (SpeedInRefrence < 0)
            //{
            //    time += -SpeedInRefrence / maxAccel;
            //    d += SpeedInRefrence * SpeedInRefrence / (2 * maxAccel);
            //    SpeedInRefrence = 0;
            //}
            //if (d > 0.06)
            //{
            //    double dvmax = (maxSpeed * maxSpeed - SpeedInRefrence * SpeedInRefrence) / (2 * maxAccel);
            //    if (dvmax > d)
            //    {
            //        double vf = Math.Sqrt(SpeedInRefrence * SpeedInRefrence + 2 * maxAccel * d);
            //        time += (vf - SpeedInRefrence) / maxAccel;
            //        d = 0;
            //    }
            //    else
            //    {
            //        time += (maxSpeed - SpeedInRefrence) / maxAccel;
            //        time += (d - dvmax) / maxSpeed;
            //    }
            //    return time;
            //}
            return 0;
        }
        private double CalculateAccel(SingleObjectState RobotState, Position2D Target, double timeB, double maxSpeed)
        {
            double accel = 0;
            Vector2D vec = Target - RobotState.Location;
            double d = vec.Size;
            double angle = Vector2D.AngleBetweenInRadians(vec, RobotState.Speed);
            double v0 = RobotState.Speed.Size * Math.Cos(angle);
            double vf = 2 * d / timeB - v0;
            if (vf <= maxSpeed)
                accel = (vf - v0) / timeB;
            else if (Math.Abs(maxSpeed * timeB - d) > 0.0001)
                accel = (maxSpeed - v0) * (maxSpeed - v0) / (2 * (maxSpeed * timeB - d));
            else
                accel = 0;
            return accel;
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

    }
}
