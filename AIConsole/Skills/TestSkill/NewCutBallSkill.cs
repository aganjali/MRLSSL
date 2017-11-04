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
    public class NewCutBallSkill : SkillBase
    {
        List<double> time = new List<double>();
        HiPerfTimer timer = new HiPerfTimer();
        const bool debug = true;
        int dataCount = 30;
        bool isFirst = true;
        Position2D firstBallPos = new Position2D();
        Position2D firstRobotPos = new Position2D();
        Dictionary <int,Position2D> ballHistoryList = new Dictionary<int, Position2D>();
        int worstPathPointKey = -1;
        Position2D lastPosToGo = new Position2D();
        double angle = 0;
        bool firstInref = true;
        Vector2D lastV = new Vector2D();
        double DefrentionalT = 0, LastDr = 0, IntegralT2 = 0;
        double lastPIDangular = 0;
        int index = 0;
        public void Reset()
        {
            worstPathPointKey = -1;
            isFirst = true;
            firstBallPos = new Position2D();
            ballHistoryList = new Dictionary<int, Position2D>();
            lastPosToGo = new Position2D();
            angle = 0;
            firstInref = true;
            lastV = new Vector2D();
            DefrentionalT = 0;
            LastDr = 0;
            IntegralT2 = 0;
            lastPIDangular = 0;
            index = 0;
            dataCount = 30;
        }
        public void CutTheBall(WorldModel Model, int RobotID, Position2D target)
        {
            var detectedBall = DetectBallPos(Model);
            if (isFirst)
            {
                firstRobotPos = Model.OurRobots[RobotID].Location;
                firstBallPos = detectedBall;
                isFirst = false;
                //index = 0;
            }
            else
            {
                if (firstBallPos.DistanceFrom(detectedBall) > 0.05)
                {
                    ballHistoryList.Add(index++, detectedBall);
                    if (ballHistoryList.Count > dataCount)
                    {
                        //if (ballHistoryList.ContainsKey(worstPathPointKey))
                        //{
                        //    ballHistoryList.Remove(worstPathPointKey);
                        //}
                        //else
                        {
                            int minkey = ballHistoryList.Keys.OrderBy(o => o).First();
                            ballHistoryList.Remove(minkey);
                        }
                        
                    }
                   timer.Start();
                    int k  = predictBallPath();
                    timer.Stop();
                    Position2D posFinal = firstBallPos;
                    if (ballHistoryList.ContainsKey(k))
                    {
                        posFinal = ballHistoryList[k];
                    }
                    
                    Line path = new Line(firstBallPos, posFinal) { DrawPen = new Pen(Color.Black, 0.02f) };
                    Line intersectBallLine = path.PerpenducilarLineToPoint(firstRobotPos);
                    Position2D? intersectBallPos = intersectBallLine.IntersectWithLine(path);
                    if (intersectBallPos.HasValue)
                    {
                        lastPosToGo = intersectBallPos.Value;
                        angle = (firstBallPos - intersectBallPos.Value).AngleInDegrees;
                    }
                    DrawingObjects.AddObject(new Circle(lastPosToGo, StaticVariables.BALL_RADIUS * 2, new Pen(Color.YellowGreen, 0.01f)), "Predict ball posgg");
                    Vector2D posTargetVec = (lastPosToGo - target).GetNormalizeToCopy(0.08);
                    lastPosToGo += posTargetVec;
                    angle = (-posTargetVec).AngleInDegrees;

                    #region
                    //double timeRobot = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, lastPosToGo, ActiveParameters.RobotMotionCoefs) * StaticVariables.FRAME_PERIOD;
                    //Position2D predictBall = Model.PredictedBall[timeRobot].Location;
                    //Position2D? predictBallPos = path.PerpenducilarLineToPoint(predictBall).IntersectWithLine(path);
                    //if (predictBallPos.HasValue)// && (lastPosToGo - predictBallPos.Value).InnerProduct(firstBallPos - predictBallPos.Value) < 0)
                    //{
                    //    if (GameParameters.IsInField(predictBallPos.Value, -0.1))
                    //    {
                    //        lastPosToGo = predictBallPos.Value;
                    //    }
                    //    else
                    //    {
                    //        Position2D tmpInt = new Position2D();
                    //        double minDist = double.MaxValue;
                    //        Position2D NearestIntersect = new Position2D();
                    //        List<Line> field = GameParameters.GetFieldLines(-0.1);
                    //        foreach (var item in field)
                    //        {
                    //            if (!item.IntersectWithLine(path, ref tmpInt))
                    //                tmpInt = Model.BallState.Location;
                    //            if ((tmpInt - Model.BallState.Location).InnerProduct(path.Tail - path.Head) > 0)
                    //            {
                    //                double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                    //                if (dist < minDist)
                    //                {
                    //                    NearestIntersect = tmpInt;
                    //                    minDist = dist;
                    //                }
                    //            }
                    //        }
                    //        if (minDist < double.MaxValue)
                    //        {
                    //            lastPosToGo = NearestIntersect;
                    //        }
                    //        else
                    //            lastPosToGo = predictBallPos.Value;
                    //    }

                    //}
                    #endregion
                    if (debug)
                    {
                        //if (predictBallPos.HasValue)
                        //{
                        //    DrawingObjects.AddObject(new Circle(predictBallPos.Value, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Yellow, 0.01f)), "Predict ball pos");
                        //}
                        DrawingObjects.AddObject(path, "fsds");
                        DrawingObjects.AddObject(new Circle(posFinal, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Red, 0.01f)), "adsadad");
                        DrawingObjects.AddObject(new Circle(lastPosToGo, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Yellow, 0.01f)), "Predict ball pos");
                    }
                }

                //Planner.ChangeDefaulteParams(RobotID, false);
                //Planner.SetParameter(5, 10, 6);
                Planner.Add(RobotID, lastPosToGo, angle, PathType.UnSafe, false, true, true, true);
                Planner.AddKick(RobotID, kickPowerType.Speed, 4, false, false);
                #region CutBall
                //if (ballHistoryList.Count > 2)
                //{
                //    Vector2D targetVec = lastPosToGo - Model.OurRobots[RobotID].Location;
                //    double db = Model.BallState.Location.DistanceFrom(lastPosToGo);
                //    double vb = ballHistoryList[ballHistoryList.Count - 1].DistanceFrom(ballHistoryList[ballHistoryList.Count - 2]) / StaticVariables.FRAME_PERIOD;
                //    double dr = Model.OurRobots[RobotID].Location.DistanceFrom(lastPosToGo);
                //    double vr = vb * dr / db;
                //    Vector2D roobotFinalSpeed = targetVec.GetNormalizeToCopy(vr);
                //    SingleWirelessCommand SWC = Accelerate(Model, RobotID, lastPosToGo, angle, roobotFinalSpeed);
                //    Planner.Add(RobotID, SWC);
                //}
                #endregion
            }

            
            Console.WriteLine(timer.Duration * 1000);
            #region old

            //if (firstTime)
            //{
            //    ballFirstPos = DetectBallPos(Model);
            //    ballPosHistory.Add(ballFirstPos);
            //    firstTime = false;
            //}
            //else
            //    ballPosHistory.Add(DetectBallPos(Model));

            //if (ballPosHistory.Count > 1)
            //{
            //    ballDistHistory.Add(ballPosHistory[ballPosHistory.Count - 1].DistanceFrom(ballPosHistory[ballPosHistory.Count - 2]));
            //    double maxVel = double.MinValue;
            //    foreach (var item in ballDistHistory)
            //    {
            //        if ((item / StaticVariables.FRAME_PERIOD) > maxVel)
            //        {
            //            maxVel = item / StaticVariables.FRAME_PERIOD;
            //        }
            //    }
            //    predictBall();
            //}

            //Vector2D ballSpeed = Model.BallState.Speed;
            ////Position2D ballPos = Model.BallState.Location;
            //Position2D predictedBall = ballPos + ballSpeed;
            //Line ballLine = new Line(ballPos, (ballPos + ballSpeed));
            //Line robotLine = ballLine.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);

            //double aaa = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location, ActiveParameters.RobotMotionCoefs);
            //DrawingObjects.AddObject(new StringDraw("Time " + aaa.ToString(), new Position2D(2, 2)), "dsafs");
            //Line targetLine = new Line(Model.OurRobots[RobotID].Location, GameParameters.OppGoalCenter);

            //Position2D? intersect = targetLine.IntersectWithLine(ballLine);
            //if (intersect.HasValue)
            //{
            //    if (debug)
            //        DrawingObjects.AddObject(intersect, "dasd");
            //}
            //if (debug)
            //{
            //    DrawingObjects.AddObject(ballLine, "fdsf");
            //    DrawingObjects.AddObject(targetLine, "dsfsd");
            //    DrawingObjects.AddObject(robotLine, "dfvsdsfsd");
            //}

            //#region
            //double maxAcc = 6;
            //Position2D Target = GameParameters.OppGoalCenter;
            //Vector2D targetVec = Target - Model.OurRobots[RobotID].Location;
            //Line targetLine = new Line(Target, Model.OurRobots[RobotID].Location);
            //Line ballLine = new Line((Model.BallState.Location + Model.BallState.Speed), Model.BallState.Location);
            //Position2D? intersectTemp = targetLine.IntersectWithLine(ballLine);
            //Position2D intersect = new Position2D();
            //if (!intersectTemp.HasValue)
            //    return;
            //else
            //    intersect = intersectTemp.Value;
            //if (intersect.X > Model.OurRobots[RobotID].Location.X)
            //{
            //    Planner.Add(RobotID, new Position2D(Model.OurRobots[RobotID].Location.X + 2, Model.OurRobots[RobotID].Location.Y), 180, true);
            //    return;
            //}
            //intersect = intersect - targetVec.GetNormalizeToCopy(0.08);
            //double robotFinalTeta = targetVec.AngleInDegrees;
            //double db = Model.BallState.Location.DistanceFrom(intersect);
            //double vb = Model.BallState.Speed.Size;
            //double dr = Model.OurRobots[RobotID].Location.DistanceFrom(intersect);
            //double vr = vb * dr / db;
            //double tb = db / vb, tr = dr / Math.Max(Model.OurRobots[RobotID].Speed.Size, 0.01);
            //double kP = 1.2;
            ////double dt = (tb - tr);
            //Vector2D roobotFinalSpeed = targetVec.GetNormalizeToCopy(kP * vr);
            //SingleWirelessCommand SWC = Accelerate(Model, RobotID, Target, robotFinalTeta, roobotFinalSpeed);
            //Vector2D v = new Vector2D(SWC.Vx, SWC.Vy);
            //SWC.KickPower = 150;
            //SWC.isChipKick = false;
            //SWC.BackSensor = true;
            //Planner.Add(RobotID, SWC, false);
            #endregion
        }
        private int predictBallPath()
        {
            Dictionary<int, double> PathScore = new Dictionary<int, double>();
            foreach (var id1 in ballHistoryList.Keys)
            {
          //      if (!PathScore.ContainsKey(id1))
                {
                    double score = 0;
                    //Line tempPath = new Line(firstBallPos, pos);
                    foreach (var id2 in ballHistoryList.Keys)
                    {
                        //Line prepLine = tempPath.PerpenducilarLineToPoint(testPos);
                        //Position2D Intersect = new Position2D();
                        //if (prepLine.IntersectWithLine(tempPath, ref Intersect))
                        //{
                        //    score += Intersect.DistanceFrom(testPos);
                        //}

                        score += Vector2D.offset_to_line(firstBallPos, ballHistoryList[id1], ballHistoryList[id2]);
                    }
                    PathScore.Add(id1, score);
                }
            }
            double minScore = double.MaxValue;
            double maxScore = double.MinValue;
            int bestPathPointKey = -1;
            foreach (var item in PathScore.Keys)
            {
                double v = PathScore[item];
                if (v < minScore)
                {
                    minScore = v;
                    bestPathPointKey = item;
                }
                if (item > maxScore)
                {
                    maxScore = v;
                    worstPathPointKey = item;
                }
            }
            return bestPathPointKey;
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
        private Position2D Vision2AI(Position2D pos, bool isReverseSide)
        {
            if (isReverseSide)
                return new Position2D(-pos.X / 1000, pos.Y / 1000);
            return new Position2D(pos.X / 1000, -pos.Y / 1000);
        }

        public SingleWirelessCommand Accelerate(WorldModel Model, int RobotID, Position2D Target, double TargetAngle, Vector2D CutSpeed)
        {
            if (firstInref && Model.lastVelocity.ContainsKey(RobotID))
            {
                lastV = Model.lastVelocity[RobotID];
                firstInref = false;
            }
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            double robotAngle = Model.OurRobots[RobotID].Angle.Value;
            Vector2D movingDirection = Target - robotLocation;

            double A = 6;
            if (Model.OurRobots[RobotID].Speed.Size > (CutSpeed.Size))
                A = -6;
            if (A > 0)
                lastV += movingDirection.GetNormalizeToCopy(Math.Abs(A) / 60);
            else
                lastV -= movingDirection.GetNormalizeToCopy(Math.Abs(A) / 60);

            if ((CutSpeed - lastV).Size < 1)
                lastV = CutSpeed;
            double Rotation = Model.OurRobots[RobotID].Angle.Value;
            if (Rotation > 180)
                Rotation -= 360;
            if (Rotation < -180)
                Rotation += 360;
            Rotation *= Math.PI / (double)180;
            Vector2D V = new Vector2D();

            V.X = lastV.Y * Math.Cos(Rotation) - lastV.X * Math.Sin(Rotation);
            V.Y = lastV.X * Math.Cos(Rotation) + lastV.Y * Math.Sin(Rotation);
            double ww = AngularController(Model.OurRobots[RobotID], TargetAngle);
            return new SingleWirelessCommand(new Vector2D(V.X, V.Y), ww, false, 0, 0, false, false);
        }

        private double AngularController(SingleObjectState state, double TargetTeta)
        {

            double dt = 0, PID = 0, dr = 0, PID_Max = 40;

            double kP = 4, kI = 0.2, kd = -0.0000, landa = 0.9, AW = 200000;
            //
            dt = (float)(state.Angle - TargetTeta);
            dt = (dt * Math.PI) / 180;
            if (dt > Math.PI)
                dt -= Math.PI * 2;
            if (dt < -Math.PI)
                dt += Math.PI * 2;
            IntegralT2 += dt;
            IntegralT2 *= landa;

            if (IntegralT2 < -PID_Max)
                IntegralT2 = -PID_Max;
            if (IntegralT2 > PID_Max)
                IntegralT2 = PID_Max;

            DefrentionalT = dt - LastDr;
            PID = IntegralT2 * kI + DefrentionalT * kd + dt * kP;

            if (Math.Abs(PID - lastPIDangular) > AW / 60)
                PID = lastPIDangular + Math.Sign(PID - lastPIDangular) * AW / 60;


            if (PID < -PID_Max)
                PID = -PID_Max;
            if (PID > PID_Max)
                PID = PID_Max;

            LastDr = dt;
            lastPIDangular = PID;

            return PID;
        }
    }
}