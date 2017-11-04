using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Skills
{
    public class CatchBallSkill : SkillBase
    {
        bool first = true;
        SingleObjectState FirstRobotState = null;
        Position2D FirstBallPos = Position2D.Zero, lastExtendedIntersect = Position2D.Zero;
        State CurrentState = State.Catch;
        double normlizeD = 0.07;
        bool firstGetRobotLoc = true;

        #region NewPerform
        const bool debug = true;
        int dataCount = 30;
        Dictionary<int, Position2D> ballHistoryList = new Dictionary<int, Position2D>();
        int index = 0;
        double angleTresh = 70;
        #endregion
        double fieldMargin = -0.1;
        bool firstDirect = true;
        SingleObjectState robot = new SingleObjectState();
        Position2D lastPredict = Position2D.Zero;

        private void DetermineNextState()
        {
            CurrentState = State.Wait;
            //if(
        }
        public void Catch(GameStrategyEngine engine, WorldModel Model, int RobotID, bool passIsChip, SingleObjectState RobotState, bool SpinBack, bool gotoPoint)
        {
            DetermineNextState();
            Position2D extendedIntersect;
            Position2D catchPoint = Position2D.Zero;
            double Angle = 0;
            if (CurrentState == State.Wait)
            {
                if (passIsChip)
                {
                    extendedIntersect = GetChipPass(engine, Model, RobotID, RobotState, ref Angle);
                }
                else
                {
                    extendedIntersect = GetDirectPass(engine, Model, RobotID, RobotState, ref Angle, out catchPoint);
                    //extendedIntersect = GetDirectPass2New(engine, Model, RobotState, OneTouchMode.Normal, ref Angle);
                    Vector2D vec = extendedIntersect - RobotState.Location;
                    //extendedIntersect = extendedIntersect + vec;
                    //catchPoint = catchPoint + vec;
                }
                if (gotoPoint)
                {
                    //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.7)
                    //{
                    //    if (isCatch || Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.1)
                    //    {
                    //        isCatch = true;
                    //        Planner.Add(RobotID, extendedIntersect, Angle, PathType.UnSafe, false, true, true, false);
                    //    }
                    //    else
                    //    {
                    //        Planner.Add(RobotID, catchPoint, Angle, PathType.UnSafe, false, true, true, false);
                    //    }
                    //}
                    //else
                    //{
                    Planner.Add(RobotID, extendedIntersect, Angle, PathType.UnSafe, false, true, true, false);
                    //}

                    Planner.AddKick(RobotID, SpinBack);

                    DrawingObjects.AddObject(new StringDraw(extendedIntersect.toString(), new Position2D(3, 3)), "sdasas:");
                    DrawingObjects.AddObject(new StringDraw(Model.OurRobots[RobotID].Location.toString(), new Position2D(3.3, 3)), "sdaa:scdsas:");
                    DrawingObjects.AddObject(new StringDraw("dis: " + Model.OurRobots[RobotID].Location.DistanceFrom(extendedIntersect).ToString(), new Position2D(3.6, 3)), "sdasa6867:6ts:");
                }
            }
            else if (CurrentState == State.Catch)
            {

            }
            return;
        }

        public void Catch(GameStrategyEngine engine, WorldModel Model, int RobotID, bool passIsChip, SingleObjectState RobotState, bool SpinBack)
        {
            Catch(engine, Model, RobotID, passIsChip, RobotState, SpinBack, true);

            return;
        }

        Line BallLine = new Line(), PrepLine = new Line();
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
        private Position2D Vision2AI(Position2D pos, bool isReverseSide)
        {
            if (isReverseSide)
                return new Position2D(-pos.X / 1000, pos.Y / 1000);
            return new Position2D(pos.X / 1000, -pos.Y / 1000);
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
                        score += Vector2D.offset_to_line(FirstBallPos, ballHistoryList[id1], ballHistoryList[id2]);
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
        private Position2D GetDirectPass2New(GameStrategyEngine engine, WorldModel Model, SingleObjectState RobotState, OneTouchMode mode, ref double Angle)
        {
            if (mode == OneTouchMode.Normal)
            {
                var detectedBall = DetectBallPos(Model);
                Position2D extendedIntersect = detectedBall;

                if (firstDirect)
                {
                    robot = new SingleObjectState(RobotState);
                    lastPredict = Model.PredictedBall[0.5].Location;
                    FirstBallPos = detectedBall;
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
                    Position2D posFinal = FirstBallPos;
                    if (ballHistoryList.ContainsKey(k))
                    {
                        posFinal = ballHistoryList[k];
                    }

                    Position2D intersect = Position2D.Zero;
                    BallLine = new Line(FirstBallPos, posFinal) { DrawPen = new Pen(Color.Pink, 0.01f) };
                    PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);

                    if (PrepLine.IntersectWithLine(BallLine, ref intersect))
                    {
                        extendedIntersect = intersect;
                        Vector2D BallLineVec = BallLine.Head - BallLine.Tail;
                        //double angb2w = Vector2D.AngleBetweenInDegrees(Target - extendedIntersect, BallLineVec);
                        //if (Math.Abs(angb2w) > angleTresh)
                        //{
                        //    Vector2D v = Vector2D.FromAngleSize(BallLineVec.AngleInRadians + Math.Sign(angb2w) * angleTresh * Math.PI / 180, 1);
                        //    Line robotTargetLine = new Line(Target, Target - v);
                        //    Position2D inter = Position2D.Zero;
                        //    if (robotTargetLine.IntersectWithLine(BallLine, ref inter))
                        //    {
                        //        if (inter.DistanceFrom(extendedIntersect) > 1)
                        //            inter = extendedIntersect + (inter - extendedIntersect).GetNormalizeToCopy(1);
                        //        extendedIntersect = inter;
                        //    }
                        //}
                        if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                        {
                            Position2D tmpInt = new Position2D();
                            double minDist = double.MaxValue;
                            Position2D NearestIntersect = new Position2D();
                            List<Line> field = GameParameters.GetFieldLines(fieldMargin);
                            foreach (var item in field)
                            {
                                if (!item.IntersectWithLine(BallLine, ref tmpInt))
                                    tmpInt = Model.BallState.Location;
                                if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                                {
                                    double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                                    if (dist < minDist)
                                    {
                                        NearestIntersect = tmpInt;
                                        minDist = dist;
                                    }
                                }
                            }
                            if (minDist < double.MaxValue)
                            {
                                intersect = NearestIntersect;
                                extendedIntersect = intersect;
                            }
                            else
                                extendedIntersect = Model.BallState.Location;
                        }
                        DrawingObjects.AddObject(BallLine, "fsds");
                        DrawingObjects.AddObject(new Circle(posFinal, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Red, 0.01f)), "adsadad");
                        DrawingObjects.AddObject(new Circle(extendedIntersect, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Yellow, 0.01f)), "Predict ball pos");


                    }
                    else
                    {
                        extendedIntersect = lastExtendedIntersect;
                    }
                }
                Angle = (FirstBallPos - extendedIntersect).AngleInDegrees;
                lastExtendedIntersect = extendedIntersect;
                return extendedIntersect;
            }
            else if (mode == OneTouchMode.Random)
            {
                var detectedBall = DetectBallPos(Model);
                Position2D extendedIntersect = detectedBall;

                if (firstDirect)
                {
                    robot = new SingleObjectState(RobotState);
                    lastPredict = Model.PredictedBall[0.5].Location;
                    FirstBallPos = detectedBall;
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
                    Position2D posFinal = FirstBallPos;
                    if (ballHistoryList.ContainsKey(k))
                    {
                        posFinal = ballHistoryList[k];
                    }

                    Position2D intersect = Position2D.Zero;
                    BallLine = new Line(FirstBallPos, posFinal) { DrawPen = new Pen(Color.Pink, 0.03f) };
                    PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);

                    if (PrepLine.IntersectWithLine(BallLine, ref intersect))
                    {
                        extendedIntersect = intersect;
                        Vector2D BallLineVec = BallLine.Head - BallLine.Tail;
                        //double angb2w = Vector2D.AngleBetweenInDegrees(Target - extendedIntersect, BallLineVec);
                        //if (Math.Abs(angb2w) > angleTresh)
                        //{
                        //    Vector2D v = Vector2D.FromAngleSize(BallLineVec.AngleInRadians + Math.Sign(angb2w) * angleTresh * Math.PI / 180, 1);
                        //    Line robotTargetLine = new Line(Target, Target - v);
                        //    Position2D inter = Position2D.Zero;
                        //    if (robotTargetLine.IntersectWithLine(BallLine, ref inter))
                        //    {
                        //        if (inter.DistanceFrom(extendedIntersect) > 1)
                        //            inter = extendedIntersect + (inter - extendedIntersect).GetNormalizeToCopy(1);
                        //        extendedIntersect = inter;
                        //    }
                        //}
                        if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                        {
                            Position2D tmpInt = new Position2D();
                            double minDist = double.MaxValue;
                            Position2D NearestIntersect = new Position2D();
                            List<Line> field = GameParameters.GetFieldLines(fieldMargin);
                            foreach (var item in field)
                            {
                                if (!item.IntersectWithLine(BallLine, ref tmpInt))
                                    tmpInt = Model.BallState.Location;
                                if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                                {
                                    double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                                    if (dist < minDist)
                                    {
                                        NearestIntersect = tmpInt;
                                        minDist = dist;
                                    }
                                }
                            }
                            if (minDist < double.MaxValue)
                            {
                                intersect = NearestIntersect;
                                extendedIntersect = intersect;
                            }
                            else
                                extendedIntersect = Model.BallState.Location;
                        }
                        DrawingObjects.AddObject(BallLine, "fsds");
                        DrawingObjects.AddObject(new Circle(posFinal, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Red, 0.01f)), "adsadad");
                        DrawingObjects.AddObject(new Circle(extendedIntersect, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Yellow, 0.01f)), "Predict ball pos");

                    }
                    else
                    {
                        extendedIntersect = lastExtendedIntersect;
                    }
                }
                Angle = (FirstBallPos - extendedIntersect).AngleInDegrees;
                lastExtendedIntersect = extendedIntersect;
                return extendedIntersect;
            }
            else
                return new Position2D();
        }

        private Position2D GetDirectPass2(GameStrategyEngine engine, WorldModel Model, SingleObjectState RobotState, Position2D Target)
        {
            Position2D extendedIntersect = Position2D.Zero;
            if (firstGetRobotLoc)
            {
                robot = new SingleObjectState(RobotState);
                firstGetRobotLoc = false;
            }
            if (firstDirect)
            {

                FirstBallPos = Model.BallState.Location;

            }
            Position2D predictedBall = (Model.PredictedBall[0.5].Location == lastPredict) ? (Model.BallState.Location + Model.BallState.Speed) : Model.PredictedBall[0.5].Location, Intersect = Position2D.Zero;
            lastPredict = Model.PredictedBall[0.5].Location;

            BallLine = new Line(FirstBallPos, predictedBall);
            PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);

            if (PrepLine.IntersectWithLine(BallLine, ref Intersect) && ((predictedBall - Intersect).InnerProduct(FirstBallPos - Intersect) >= 0 || firstDirect))
            {

                DrawingObjects.AddObject(new Circle(Intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                extendedIntersect = Intersect;// -(Target - Intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(fieldMargin); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        Intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = Intersect;// -(Target - Intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
            }
            else
            {
                extendedIntersect = lastExtendedIntersect;
                DrawingObjects.AddObject(new Circle(Intersect, 0.5, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "stopotpointcalc");
            }
            //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
            //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((tmpTarget - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
            //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");

            //     firstDirect = false;
            lastExtendedIntersect = extendedIntersect;

            return extendedIntersect;

        }

        private Position2D GetDirectPass(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState RobotState, ref double Angle, out Position2D catchPoint)
        {
            double predictTime = 0.5;
            if (first)
            {
                FirstRobotState = new SingleObjectState(RobotState);
                FirstBallPos = Model.BallState.Location;
                first = false;
            }

            SingleObjectState ball = Model.BallState;
            SingleObjectState predictedBall = Model.PredictedBall[predictTime];
            Position2D intersect = Position2D.Zero;
            Vector2D ballPredictVec = predictedBall.Location - ball.Location;


            if (ballPredictVec.Size > 0 && Model.PredictedBall.states.Count > 1)
                BallLine = new Line(ball.Location, ball.Location + ballPredictVec);
            else
                BallLine = new Line(ball.Location, Model.OurRobots[RobotID].Location);

            PrepLine = BallLine.PerpenducilarLineToPoint(FirstRobotState.Location);
            Position2D extendedIntersect = Position2D.Zero;
            if (PrepLine.IntersectWithLine(BallLine, ref intersect) /*&& ((predictedBall.Location - intersect).InnerProduct(FirstBallPos - intersect) >= 0)*/)
            {

                DrawingObjects.AddObject(new Circle(intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                extendedIntersect = intersect + (intersect - FirstBallPos).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, 0.05))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines();
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = ball.Location;
                        if ((tmpInt - ball.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = ball.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        intersect = NearestIntersect;
                        extendedIntersect = intersect + (intersect - FirstBallPos).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = ball.Location;
                }
            }
            else
            {
                extendedIntersect = lastExtendedIntersect;
                DrawingObjects.AddObject(new Circle(intersect, 0.5, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "stopotpointcalc");
            }

            lastExtendedIntersect = extendedIntersect;
            Angle = (FirstBallPos - extendedIntersect).AngleInDegrees;
            catchPoint = extendedIntersect + ballPredictVec.GetNormalizeToCopy(2);
            return extendedIntersect;
        }

        bool ballGet = false;
        double sumang = 0;
        Queue<double> angleQ = new Queue<double>();
        double chipRadious = 1.5;
        private Position2D GetChipPass(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState RobotState, ref double Angle)
        {
            Circle C = new Circle(RobotState.Location, chipRadious);
            if (first)
            {
                FirstRobotState = new SingleObjectState(RobotState);
                FirstBallPos = Model.BallState.Location;

                angleQ.Clear();
                BallLine = new Line(FirstRobotState.Location, FirstBallPos);
                sumang = 0;
                PrepLine = BallLine.PerpenducilarLineToPoint(FirstRobotState.Location);
                double minChipRad = 0.6;
                double maxChipRad = 1.2;
                double minDist = 2, maxDist = 3.5;
                double d = Math.Min(maxDist, Math.Max(0, Model.BallState.Location.DistanceFrom(C.Center) - minDist)) / (maxDist - minDist);
                chipRadious = minChipRad + d * (maxChipRad - minChipRad);
                first = false;
            }
            if (C.IsInCircle(Model.BallState.Location))
                ballGet = true;
            if (ballGet)
            {
                bool add = true;
                if (angleQ.Count > 0)
                {
                    double tmpavg = sumang / (double)angleQ.Count;
                    double d = Math.Abs(Model.BallState.Speed.AngleInRadians - tmpavg);
                    if (d > 2 * Math.PI)
                        d -= Math.PI;
                    if (d > Math.PI / 6)
                        add = false;
                }
                if (add)
                    angleQ.Enqueue(Model.BallState.Speed.AngleInRadians);
                sumang += angleQ.Last();
                double tmpang = sumang / (double)angleQ.Count;
                BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(tmpang, 4));
                PrepLine = BallLine.PerpenducilarLineToPoint(FirstRobotState.Location);
                if (angleQ.Count > 5)
                {
                    double lastang = angleQ.First();
                    angleQ.Dequeue();
                    sumang -= lastang;
                }
            }
            Position2D intersect = new Position2D();
            Position2D extendedIntersect = Position2D.Zero;
            if (PrepLine.IntersectWithLine(BallLine, ref intersect))
            {
                DrawingObjects.AddObject(new Circle(intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                extendedIntersect = intersect + (intersect - FirstBallPos).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, 0.05))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = intersect + (intersect - FirstBallPos).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
                DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
                DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
            }
            else
            {
                extendedIntersect = lastExtendedIntersect;
                DrawingObjects.AddObject(new Circle(intersect, 0.5, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "stopotpointcalc");
            }

            lastExtendedIntersect = extendedIntersect;
            Angle = (FirstBallPos - extendedIntersect).AngleInDegrees;
            return extendedIntersect;
        }

        enum State
        {
            Wait,
            Catch
        }
    }
}
