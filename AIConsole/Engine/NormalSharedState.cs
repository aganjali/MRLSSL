using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Engine
{
    public static class NormalSharedState
    {
        public struct ActionInfo
        {
            public Position2D Target;
            public Position2D PassTarget;
            public ActivePassKind pKind;
            public ActiveDribleKind dKind;

            public double kick;
            public bool isChip;
            public string strState;
            public double tolerance;
            public double acc;
            public bool sync;
            public bool passSync;
            public double minDist;
            public int minIdx;
            public ActionInfo(bool b)
            {
                Target = GameParameters.OppGoalCenter;
                PassTarget = Position2D.Zero;
                pKind = ActivePassKind.OneTouch;
                dKind = ActiveDribleKind.SpaceDrible;

                kick = 0;
                isChip = false;
                passSync = false;
                sync = false;
                strState = "";
                tolerance = 0;
                acc = 0;
                minDist = double.MaxValue;
                minIdx = -1;
            }
        }

        #region Info Classes
        public static class CommonInfo
        {
            public static bool AttackerMode = false;
            static Position2D? OneTouchPassPoint = null;
            static Position2D? CatchPassPoint = null;

            static double avoidPickingOppXMargin = 0.1;
            static double avoidPickingOppDist = 0.2;
            static double avoidPickingOppAngle = 45;
            static double pickSupportRangeDist = 0.7;

            public static bool Ready2Pass = false;
            public static double PassSpeed = 0, ShootSpeed = Program.MaxKickSpeed;
            public static Position2D PassTarget = Position2D.Zero;
            public static double passTreshhold = 2;
            public static bool PassIsChip = false;
            public static ActivePassKind PassKind = ActivePassKind.OneTouch;
            public static Position2D ShootTarget = GameParameters.OppGoalCenter;
            public static Position2D? GoodPointInGoal = GameParameters.OppGoalCenter;
            public static SingleObjectState PasserState = null;
            public static bool Passed = false;

            public static bool IsPicking = false;
            public static int? PickerID = null;
            public static int? ActiveID = null, AttackerID = null, SupporterID = null, lastActiveID = null;
            public static int OppConfID = -1;

            public static bool PickIsFeasible = false;
            public static bool ActiveIsCatchingPass = false;

            static void FindOurNearest(GameStrategyEngine engine, WorldModel Model, List<int> behinds, ref bool isNear, ref double minTime, ref double minId)
            {
                float vMax = 4, aMax = 5;
                foreach (var item in behinds)
                {
                    if (!Model.OurRobots.ContainsKey(item))
                        continue;
                    if (!BallKickedToUs(Model, item, GameParameters.OppGoalCenter, ref isNear))
                    {
                        Position2D pickPoint = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(PickerInfo.PickDistance);
                        //if (oppID != -1 && engine.GameInfo.OppTeam.CatchBallLines.ContainsKey(oppID) && engine.GameInfo.OppTeam.CatchBallLines[oppID].Count > 0)
                        //    pickPoint = engine.GameInfo.OppTeam.CatchBallLines[oppID].First().Head;
                        Vector2D refrence = pickPoint - Model.OurRobots[item].Location;
                        refrence.NormalizeTo(Math.Max(0, refrence.Size - 0.1));
                        Vector2D vRef = GameParameters.InRefrence(Model.OurRobots[item].Speed, refrence);
                        double t = CalculateTime((float)refrence.Size, (float)vRef.Y, vMax, aMax);
                        if (t < minTime)
                        {
                            minTime = t;
                            minId = item;
                        }
                    }
                }
            }
            static void FindOppNearestTime(GameStrategyEngine engine, WorldModel Model, out double minTimeOpp, out int minIDOpp)
            {
                double dist;
                double DistFromBorder;
                minTimeOpp = double.MaxValue;
                minIDOpp = -1;

                foreach (var item in Model.Opponents.Keys)
                {
                    if (!GameParameters.IsInDangerousZone(Model.Opponents[item].Location, true, 0.3, out dist, out DistFromBorder))
                    {
                        if (engine.GameInfo.OppTeam.TimeHeads.ContainsKey(item) && engine.GameInfo.OppTeam.TimeHeads[item].Count > 0 && engine.GameInfo.OppTeam.TimeHeads[item].First() < minTimeOpp)
                        {
                            minTimeOpp = engine.GameInfo.OppTeam.TimeHeads[item].First();
                            minIDOpp = item;
                        }
                    }
                }
            }
            static bool BallKickedToUs(WorldModel Model, int RobotID, Position2D Target, ref bool isNear)
            {
                Line line = new Line();
                line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
                Position2D BallGoal = line.CalculateY(Model.OurRobots[RobotID].Location.X);
                isNear = false;
                if (Model.BallState.Speed.Size > ActiveParameters.kickedToUsBallSpeedTresh)
                    if (Model.BallState.Speed.InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0)
                    {
                        List<Position2D> poses = new Circle(Model.OurRobots[RobotID].Location, ActiveParameters.kickedToUsRadi).Intersect(line);

                        List<Position2D> poses2 = new Circle(Model.OurRobots[RobotID].Location, ActiveParameters.nearIncomingRadi).Intersect(line);
                        foreach (var item in poses2)
                        {
                            DrawingObjects.AddObject(item);
                        }
                        double nearP = Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) / Model.BallState.Speed.Size;
                        Vector2D robotBall = Model.BallState.Location - Model.OurRobots[RobotID].Location;
                        Vector2D robotTarget = Target - Model.OurRobots[RobotID].Location;

                        if (poses2.Count > 0 && ((nearP > 1 && nearP < 1.5 && Model.BallState.Speed.Size > ActiveParameters.nearBallSpeedTresh)) && Math.Abs(Vector2D.AngleBetweenInDegrees(robotBall, robotTarget)) < 70)
                            isNear = true;

                        if (poses.Count > 0)
                            return true;
                    }
                return false;
            }
            static float CalculateTime(float dR, float v0, float v_max, float a_max)
            {
                float tTotal = 0;
                float sgndR = sign(dR);
                if (dR == 0)
                {
                    if (Math.Abs(v0) <= 0.02)
                    {
                        return tTotal;
                    }
                    float a = a_max * -sign(v0);
                    float tStop = -v0 / a;
                    float dStop = -v0 * v0 / (2 * a);
                    dR -= dStop;
                    v0 = 0;
                    float tMaxAcc = v_max / a_max;
                    float dMaxAcc = v_max * v_max / a;
                    float tReturn;
                    if (Math.Abs(dMaxAcc) <= Math.Abs(dR))
                    {
                        tReturn = 2 * tMaxAcc + (Math.Abs(dR) - Math.Abs(dMaxAcc)) / v_max;
                    }
                    else
                    {
                        tReturn = (float)(2 * Math.Sqrt(a * dR) / a_max);
                    }
                    tTotal = tStop + tReturn;
                }
                else
                {
                    float sgnV0dR = sign(v0 * dR);
                    if (sgnV0dR < 0)
                    {
                        float a = a_max * sign(dR);
                        float tStop = -v0 / a;
                        float dStop = -v0 * v0 / (2 * a);
                        dR -= dStop;
                        v0 = 0;
                        float tMaxAcc = v_max / a_max;
                        float dMaxAcc = v_max * v_max / a;
                        float tReturn;
                        if (Math.Abs(dMaxAcc) <= Math.Abs(dR))
                        {
                            tReturn = 2 * tMaxAcc + (Math.Abs(dR) - Math.Abs(dMaxAcc)) / v_max;
                        }
                        else
                        {
                            tReturn = (float)(2 * Math.Sqrt(a * dR) / a_max);
                        }
                        tTotal = tStop + tReturn;
                    }
                    else
                    {
                        float a = -sign(dR) * a_max;
                        float tStop = -v0 / a;
                        float dStop = -v0 * v0 / (2 * a);
                        if (Math.Abs(dStop) > Math.Abs(dR))
                        {
                            dR -= dStop;
                            v0 = 0;
                            a = sign(dR) * a_max;
                            float tMaxAcc = v_max / a_max;
                            float dMaxAcc = v_max * v_max / a;
                            float tReturn;
                            if (Math.Abs(dMaxAcc) <= Math.Abs(dR))
                            {
                                tReturn = 2 * tMaxAcc + (Math.Abs(dR) - Math.Abs(dMaxAcc)) / v_max;
                            }
                            else
                            {
                                tReturn = (float)(2 * Math.Sqrt(a * dR) / a_max);
                            }
                            tTotal = tReturn + tStop;
                        }
                        else
                        {
                            a = sign(dR) * a_max;
                            float vm = sign(dR) * v_max;
                            float d2vm = (2 * vm * vm - v0 * v0) / (2 * a);
                            float t2vm = (2 * vm - v0) / a;

                            if (Math.Abs(d2vm) <= Math.Abs(dR))
                            {
                                tTotal = t2vm + (Math.Abs(dR) - Math.Abs(d2vm)) / v_max;
                            }
                            else
                            {
                                float v = (float)(sign(dR) * Math.Sqrt(a * dR + v0 * v0 / 2));
                                tTotal = (2 * v - v0) / a;
                            }
                        }
                    }
                }
                return tTotal;
            }
            static float sign(float x)
            {
                return (x == 0) ? 0 : (x / Math.Abs(x));
            }

            public static bool IsPickingFeasible(GameStrategyEngine engine, WorldModel Model, List<int> ActiveIds, out int pickerID)
            {
                List<int> behinds = new List<int>();
                bool res = false;
                pickerID = -1;
                double minTimeOpp;
                int minIDOpp;
                bool isNear = false;

                FindOppNearestTime(engine, Model, out minTimeOpp, out minIDOpp);
                if (minIDOpp != -1)
                {
                    DrawingObjects.AddObject(new Circle(Model.Opponents[minIDOpp].Location, 0.25, new Pen(Color.YellowGreen, 0.01f)), "oppActive");
                    DrawingObjects.AddObject(new StringDraw("opp: " + minTimeOpp * 60, new Position2D(2, 2)), "oppActiveTime");
                }

                if (minIDOpp == -1)
                    return false;


                if (IsPicking)
                {
                    if (ActiveInfo.CurrentAction == ActiveActionMode.Shoot || ActiveInfo.CurrentAction == ActiveActionMode.Pass)
                    {
                        if (Model.BallState.Speed.Size > passTreshhold && Model.BallState.Speed.InnerProduct(ActiveInfo.Target - Model.BallState.Location) >= 0)
                            return false;
                    }
                    if (PickerID.HasValue && Model.OurRobots.ContainsKey(PickerID.Value))
                    {
                        if ((Position2D.IsBetween(Model.BallState.Location, Model.OurRobots[PickerID.Value].Location, Model.Opponents[minIDOpp].Location)
                            && Model.Opponents[minIDOpp].Location.DistanceFrom(Model.BallState.Location) <= Model.OurRobots[PickerID.Value].Location.DistanceFrom(Model.BallState.Location)))
                        //&& Model.Opponents[minIDOpp].Location.DistanceFrom(Model.BallState.Location) <= avoidPickingOppDist
                        //&& ActiveID.HasValue && Model.OurRobots[ActiveID.Value].Location.DistanceFrom(Model.BallState.Location) <= avoidPickingOppDist))
                        {
                            //Vector2D r = Vector2D.FromAngleSize((Model.Opponents[minIDOpp].Angle.Value + avoidPickingOppAngle) * Math.PI / 180, 1);
                            //Vector2D l = Vector2D.FromAngleSize((Model.Opponents[minIDOpp].Angle.Value - avoidPickingOppAngle) * Math.PI / 180, 1);
                            //Vector2D v = Model.BallState.Location - Model.Opponents[minIDOpp].Location;

                            //if (Vector2D.IsBetween(r, l, v))
                            return false;
                        }
                        return true;
                    }

                    //if (PickerID.HasValue && ActiveID.HasValue&& Model.BallState.Location.DistanceFrom(Model.OurRobots[ActiveID.Value].Location) <= 0.5 && Position2D.IsBetween(Model.OurRobots[PickerID.Value].Location, Model.BallState.Location, Model.OurRobots[ActiveID.Value].Location))
                    //    if(
                    //    return false;

                }


                bool b = false;
                foreach (var item in ActiveIds)
                {
                    if (!Model.OurRobots.ContainsKey(item))
                        continue;
                    Vector2D robotBall = Model.BallState.Location - Model.OurRobots[item].Location;
                    Vector2D refrence = GameParameters.OurGoalCenter - Model.BallState.Location;

                    if (GameParameters.InRefrence(robotBall, refrence).Y > 0 && Model.BallState.Location.DistanceFrom(Model.OurRobots[item].Location) > 0.07)
                        behinds.Add(item);

                    if (engine.GameInfo.OurTeam.TimeHeads.ContainsKey(item) && engine.GameInfo.OurTeam.TimeHeads[item].Count > 0
                        && (engine.GameInfo.OurTeam.TimeHeads[item].First() - minTimeOpp) * 60 < -20)
                    {
                        b = true;
                    }
                    if (engine.GameInfo.OurTeam.TimeHeads.ContainsKey(item) && engine.GameInfo.OurTeam.TimeHeads[item].Count > 0)
                        DrawingObjects.AddObject(new StringDraw("t" + item + ": " + engine.GameInfo.OurTeam.TimeHeads[item].First() * 60, new Position2D(1.9 - item * 0.1, 2)), "ourActiveTime" + item);
                }
                double minTime = double.MaxValue, minId = -1;
                FindOurNearest(engine, Model, behinds, ref isNear, ref minTime, ref minId);

                DrawingObjects.AddObject(new StringDraw("ourBhnd: " + minTime * 60, new Position2D(2.1, 2)), "ourPickTime");
                if (minId != -1 && (minTime - minTimeOpp) * 60 <= -15)
                {

                    if (!b)
                    {
                        DrawingObjects.AddObject(new StringDraw("behind", new Position2D(2.2, 2)), "pickkind");
                        pickerID = (int)minId;
                        res = true;
                    }
                }
                else
                {
                    behinds = new List<int>();
                    foreach (var item in ActiveIds)
                    {
                        if (!Model.OurRobots.ContainsKey(item))
                            continue;
                        if (Model.BallState.Location.DistanceFrom(Model.OurRobots[item].Location) > 0.07 && Position2D.IsBetween(Model.Opponents[minIDOpp].Location, Model.BallState.Location, Model.OurRobots[item].Location))
                            behinds.Add(item);

                    }

                    minTime = double.MaxValue; minId = -1;
                    FindOurNearest(engine, Model, behinds, ref isNear, ref minTime, ref minId);

                    DrawingObjects.AddObject(new StringDraw("ourBtw: " + minTime * 60, new Position2D(2.3, 2)), "ourPickTimeBtw");
                    if (minId != -1 && (minTime - minTimeOpp) * 60 <= -15)
                    {
                        if (!b)
                        {
                            DrawingObjects.AddObject(new StringDraw("Btw", new Position2D(2.1, 2)), "pickkind");
                            pickerID = (int)minId;
                            res = true;
                        }
                    }

                }
                if ((pickerID != -1 && Position2D.IsBetween(Model.BallState.Location, Model.OurRobots[pickerID].Location, Model.Opponents[minIDOpp].Location)
                   && Model.Opponents[minIDOpp].Location.DistanceFrom(Model.BallState.Location) <= Model.OurRobots[pickerID].Location.DistanceFrom(Model.BallState.Location)))
                //&& Model.Opponents[minIDOpp].Location.DistanceFrom(Model.BallState.Location) <= avoidPickingOppDist))
                {
                    //Vector2D r = Vector2D.FromAngleSize((Model.Opponents[minIDOpp].Angle.Value + avoidPickingOppAngle) * Math.PI / 180, 1);
                    //Vector2D l = Vector2D.FromAngleSize((Model.Opponents[minIDOpp].Angle.Value - avoidPickingOppAngle) * Math.PI / 180, 1);
                    Vector2D v = Model.BallState.Location - Model.Opponents[minIDOpp].Location;
                    res = false;
                    //if (Vector2D.IsBetween(r, l, v))
                    //{
                    //    foreach (var item in ActiveIds)
                    //    {
                    //        if (!Model.OurRobots.ContainsKey(item))
                    //            continue;
                    //        if (item == pickerID)
                    //            continue;
                    //        if (Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) <= avoidPickingOppDist)
                    //        {
                    //            pickerID = -1;
                    //            res = false;
                    //        }
                    //    }
                    //}
                }
                return res;
            }
            public static List<Position2D> GetPassPoints(bool resetOt, bool resetCr, GameStrategyEngine engine, WorldModel Model, Position2D From, Position2D Target, Position2D topLeft, Vector2D Size, int Rows, int Columns, double passSpeed, double shootSpeed)
            {
                bool otAdd = false, crAdd = false;
                List<Position2D> res = new List<Position2D>();
                if (!resetOt && OneTouchPassPoint.HasValue)
                {
                    otAdd = true;
                    res.Add(OneTouchPassPoint.Value);
                }
                if (!resetCr && CatchPassPoint.HasValue)
                {
                    crAdd = true;
                    res.Add(CatchPassPoint.Value);
                }
                if (!crAdd || !otAdd)
                {
                    List<int> exIds = new List<int>();
                    if (ActiveID.HasValue)
                        exIds.Add(ActiveID.Value);
                    if (AttackerID.HasValue)
                        exIds.Add(AttackerID.Value);
                    var poses = engine.GameInfo.BestPassPoint(Model, From, Target, topLeft, exIds, ActiveID,
                         passSpeed, shootSpeed, Size.X, Size.Y, Rows, Columns);
                    if (!otAdd)
                    {
                        res.Insert(0, poses[0]);
                        OneTouchPassPoint = poses[0];
                    }
                    if (!crAdd)
                        res.Insert(1, poses[1]);
                    CatchPassPoint = poses[1];
                }
                return res;
            }

            //ActiveActionMode DeterminePassKind(GameStrategyEngine engine, WorldModel Model, int RobotID, int? AttackerID, Position2D from, Position2D shootTarget, double passSpeed, double shootSpeed, bool canDrible, out Position2D Target, out ActivePassKind pKind, out ActiveDribleKind dKind, out bool isChip)
            //{
            //    ActiveActionMode res = ActiveActionMode.Shoot;
            //    Obstacles obs = new Obstacles(Model);
            //    if (lastBall.HasValue && Model.BallState.Location.DistanceFrom(lastBall.Value) > 0.6)
            //        lastBall = null;
            //    if (lastBall.HasValue)
            //    {
            //        if (AttackerID.HasValue && Model.OurRobots.ContainsKey(AttackerID.Value))
            //            obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID, AttackerID.Value }, null);
            //        else
            //            obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
            //        isChip = obs.Meet(new SingleObjectState(from, Vector2D.Zero, 0), new SingleObjectState(lastPassTarget, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi);

            //        res = lastPassAct;
            //        Target = lastPassTarget;
            //        dKind = lastdKind;
            //        pKind = lastPassKind;

            //        return res;
            //    }
            //    var points = NormalSharedState.CommonInfo.GetPassPoints(true, true, engine, Model, from, shootTarget, new Position2D(0.75, GameParameters.OurRightCorner.Y),
            //                new Vector2D(3.5, 2 * GameParameters.OurLeftCorner.Y), 5, 10, passSpeed, shootSpeed);

            //    double otScore = double.MaxValue, crScore = double.MaxValue, drScore = double.MaxValue;

            //    double oc = 1, cc = 1;
            //    if (AttackerID.HasValue && Model.OurRobots.ContainsKey(AttackerID.Value))
            //    {
            //        obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID, AttackerID.Value }, null);

            //        if (points.Count > 0)
            //        {
            //            double d = Model.OurRobots[AttackerID.Value].Location.DistanceFrom(points[0]);
            //            oc = (obs.Meet(new SingleObjectState(from, Vector2D.Zero, 0), new SingleObjectState(points[0], Vector2D.Zero, 0), MotionPlannerParameters.BallRadi)) ? 0 : 1;
            //            double b = (obs.Meet(Model.OurRobots[AttackerID.Value], new SingleObjectState(points[0], Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi)) ? 0.1 : 1;
            //            double a = Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - points[0], from - points[0])) > 70 ? 0 : 1;
            //            double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), points[0] - from));
            //            otScore = b * oc * d * a * t;
            //        }
            //        if (points.Count > 1)
            //        {
            //            double d = Model.OurRobots[AttackerID.Value].Location.DistanceFrom(points[1]);
            //            cc = (obs.Meet(new SingleObjectState(from, Vector2D.Zero, 0), new SingleObjectState(points[1], Vector2D.Zero, 0), MotionPlannerParameters.BallRadi)) ? 0 : 1;
            //            double b = (obs.Meet(Model.OurRobots[AttackerID.Value], new SingleObjectState(points[1], Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi)) ? 0.1 : 1;
            //            double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), points[1] - from));
            //            crScore = b * cc * d * t;
            //        }
            //    }
            //    Position2D dPoint;
            //    bool dIsFeasible = NormalSharedState.CommonInfo.GetDribblePoint(engine, Model, RobotID, from, shootTarget, out dPoint);
            //    if (canDrible && dIsFeasible)
            //    {
            //        points.Add(dPoint);
            //        double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), points.Last() - from));
            //        double d = Model.OurRobots[RobotID].Location.DistanceFrom(points.Last()) * 1.5;
            //        double a = (obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(points.Last(), Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi)) ? 0.1 : 1;
            //        drScore = t * d * a;
            //    }
            //    otScore *= 1;
            //    crScore *= 1.5;
            //    drScore *= 6;

            //    dKind = ActiveDribleKind.SpaceDrible;
            //    pKind = ActivePassKind.OneTouch;
            //    Target = shootTarget;
            //    isChip = false;
            //    bool pCalced = false;
            //    foreach (var item in points)
            //    {
            //        if (item != Position2D.Zero)
            //            pCalced = true;
            //    }
            //    if ((otScore <= 1e5 || crScore <= 1e5 || drScore <= 1e5) && (pCalced))
            //    {
            //        if (otScore <= crScore && otScore <= drScore)
            //        {
            //            isChip = (oc != 0) ? false : true;
            //            Target = points[0];
            //            res = ActiveActionMode.Pass;
            //            pKind = ActivePassKind.OneTouch;
            //        }
            //        else if (crScore < otScore && crScore <= drScore)
            //        {
            //            isChip = (cc != 0) ? false : true;
            //            Target = points[1];
            //            res = ActiveActionMode.Pass;
            //            pKind = ActivePassKind.Catch;
            //        }
            //        else if (dIsFeasible && drScore < otScore && drScore < crScore)
            //        {
            //            isChip = true;
            //            Target = points.Last();
            //            res = ActiveActionMode.Drible;
            //            dKind = ActiveDribleKind.SpaceDrible;
            //        }
            //        if (!lastBall.HasValue)
            //        {
            //            lastBall = Model.BallState.Location;
            //        }

            //    }
            //    lastPassTarget = Target;
            //    lastPassIsChip = isChip;
            //    lastdKind = dKind;
            //    lastPassKind = pKind;
            //    lastPassAct = res;
            //    return res;
            //}

            public static bool GetDribblePoint(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D From, Position2D Target, out Position2D minP)
            {
                int count = 10;
                double step = Math.PI / count;
                double angle = Math.PI / 2, length = 2, length2 = 1;
                double[] costs = new double[count - 1];
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                double maxScore = double.MinValue;
                minP = Position2D.Zero;
                bool res = false;
                for (int i = 1; i < count; i++)
                {
                    angle += step;
                    angle = GameParameters.AngleModeR(angle);
                    Vector2D v = Vector2D.FromAngleSize(angle, length2);
                    Position2D p = From + v;
                    double a = obs.Meet(new SingleObjectState(From, Vector2D.Zero, 0), new SingleObjectState(p, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi) ? 0 : 1;
                    double b = GameParameters.IsInField(p, 0) ? 1 : 0;
                    double d = 1;
                    Circle c = new Circle(p, length);
                    foreach (var item in Model.Opponents.Keys)
                    {
                        if (c.IsInCircle(Model.Opponents[item].Location))
                        {
                            d = 0;
                            break;
                        }
                    }
                    double e = Math.Min(Math.Abs(p.Y - GameParameters.OurLeftCorner.Y), Math.Abs(p.Y - GameParameters.OurRightCorner.Y));
                    double s = a * b * d * e;
                    if (s > maxScore)
                    {
                        res = true;
                        maxScore = s;
                        minP = p;
                    }
                }

                return res;
            }
            public static Position2D GetDribblePoint(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D From, Position2D Target, int? opp2Ex, out double score)
            {
                int count = 5;
                double step = Math.PI / (count + 1);
                double angle = Math.PI / 2, length = 1.0;
                double[] scores = new double[count];
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                score = double.MinValue;
                Position2D minP = Position2D.Zero;
                for (int i = 0; i < count; i++)
                {
                    angle += step;
                    angle = GameParameters.AngleModeR(angle);
                    Vector2D v = Vector2D.FromAngleSize(angle, length);
                    Position2D p = From + v;
                    double a = obs.Meet(new SingleObjectState(From, Vector2D.Zero, 0), new SingleObjectState(p, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi) ? 0.5 : 1;
                    double b = GameParameters.IsInField(p + v, 0) ? 1 : -1;
                    double minDist = double.MaxValue;
                    foreach (var item in Model.Opponents)
                    {
                        if (opp2Ex.HasValue && item.Key == opp2Ex.Value)
                            continue;
                        double d = item.Value.Location.DistanceFrom(p);
                        if (d < minDist)
                            minDist = d;
                    }
                    if (minDist < RobotParameters.OurRobotParams.Diameter + 0.1)
                        minDist = 0;
                    var intervals = engine.GameInfo.GetVisibleIntervals(Model, p, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    double c = 0.1;
                    double maxW = double.MinValue;
                    int idx = -1;
                    for (int j = 0; j < intervals.Count; j++)
                    {
                        var item = intervals[j];
                        if (Math.Abs(item.interval.Start - item.interval.End) > maxW)
                        {
                            idx = j;
                            maxW = Math.Abs(item.interval.Start - item.interval.End);
                        }
                    }
                    if (idx > -1)
                    {
                        c = Math.Min(Math.Abs(GameParameters.OppGoalRight.Y - GameParameters.OppGoalLeft.Y), Math.Abs(intervals[idx].interval.Start - intervals[idx].interval.End));
                        c = c / Math.Abs(GameParameters.OppGoalRight.Y - GameParameters.OppGoalLeft.Y);
                        c += 0.1;
                        c = Math.Min(c, 1);
                    }
                    minDist = Math.Min(3.0, minDist) / 3.0;
                    double s = a * b * minDist * c;
                    if (s > score)
                    {
                        score = s;
                        minP = p;
                    }
                }

                return minP;
            }

            public static void Reset()
            {
                OppConfID = -1;
                GoodPointInGoal = GameParameters.OppGoalCenter;
                PasserState = null;
                OneTouchPassPoint = null;
                CatchPassPoint = null;
                Ready2Pass = false;
                Passed = false;
                PassSpeed = 0;
                ShootSpeed = Program.MaxKickSpeed;
                ShootTarget = GameParameters.OppGoalCenter;
                IsPicking = false;
                PickerID = null;
                ActiveID = null;
                AttackerID = null;
                SupporterID = null;
                PickIsFeasible = false;
                ActiveIsCatchingPass = false;



            }
            public static void ResetPass()
            {

                OneTouchPassPoint = null;
                CatchPassPoint = null;
                Ready2Pass = false;
                PassSpeed = 5;
                ShootSpeed = Program.MaxKickSpeed;
                PassTarget = Position2D.Zero;
                ShootTarget = GameParameters.OppGoalCenter;
                PassKind = ActivePassKind.OneTouch;
                PassIsChip = false;
                Passed = false;
            }
        }
        public static class ActiveInfo
        {
            public static ActiveActionMode CurrentAction = ActiveActionMode.None;
            public static ActiveRoleState CurrentState = ActiveRoleState.Open2Kick;
            public static GetBallState ActiveSkillState = GetBallState.Static;
            public static Position2D Target = GameParameters.OppGoalCenter;
            public static Position2D IncomingPred = Position2D.Zero;
            public static Position2D KickTarget = GameParameters.OppGoalCenter;

            public static bool isRotateStarted = false;
            public static bool IsNear = false;
            public static bool spin = false, isChip = false;
            public static double kickSpeed = 0;

            public static int confWaitCounter = 0;
            public const int confMaxWaitTresh = 30;
            public const int confRotateMaxWaitTresh = 50;

        }

        public static class NewActiveInfo
        {
            public static ActiveRoleState CurrentState = ActiveRoleState.Open2Kick;
            public static GetBallState ActiveSkillState = GetBallState.Static;
            public static Position2D IncomingPred = Position2D.Zero;
            public static Position2D KickTarget = GameParameters.OppGoalCenter;
            public static Position2D? passTarget = null;

            public static int? AttackerToPass = null;

            public static bool IsNear = false;
            public static bool spin = false;
            public static bool isChip = false;
            public static double kickSpeed = 0;

        }
        public static class NewAttackerInfo
        {
            public static bool IsAttacker1 = false;
            public static bool IsAttacker2 = false;
            public static int? Attacker1ID = null;
            public static int? Attacker2ID = null;
            public static Position2D? PosToGo1 = null;
            public static Position2D? PosToGo2 = null;

            public static void Reset()
            {
                Attacker1ID = null;
                Attacker2ID = null;
            }
        }
      
        public static class AttackerInfo
        {
            public static int? OppMarkID = null;
            public static AttacekrState CurrentState = AttacekrState.WaitForPass;
            public static Position2D MarkPoint = Position2D.Zero;
        }
        public static class PickerInfo
        {
            public static PickerState CurrentState = PickerState.GotoPoint;

            public static Position2D TargetPoint = GameParameters.OurGoalCenter;
            public static double PickDistance = 0.3;
        }
        #endregion

        #region PassSyncer
        public class PassSyncronizer
        {

            public static bool Sync(GameStrategyEngine engine, WorldModel Model, int ActiveID, int AttackerID, double maxPassSpeed, bool isChip, Position2D PassTarget, ref double PassSpeed)
            {
                double passTime = 0;
                double passMotion = 0;

                passMotion = Planner.GetMotionTime(Model, AttackerID, Model.OurRobots[AttackerID].Location, PassTarget, ActiveParameters.RobotMotionCoefs) * ((1 - Math.Min(1, Model.OurRobots[AttackerID].Speed.Size / 3.0)) * (1 - 0.75) + 0.75);

                int counter = 0;
                if (Model.OurRobots[AttackerID].Location.DistanceFrom(PassTarget) < 0.4)
                    return true;
                if (!isChip)
                {
                    double minPassSpeed = 3;
                    maxPassSpeed = Math.Max(maxPassSpeed, minPassSpeed);
                    PassSpeed = maxPassSpeed;
                    double u = maxPassSpeed - minPassSpeed;
                    double du = -u;
                    while (counter < 10)
                    {
                        du *= 0.5;
                        PassSpeed = minPassSpeed + u + du;
                        double passSpeedPhase2 = PassSpeed * 5 / 7;
                        double ballAccelPhase1 = -5;
                        double ballAccelPhase2 = -0.3;
                        double dxPhase1 = 0;
                        passTime = ((passSpeedPhase2 - PassSpeed) / ballAccelPhase1) / StaticVariables.FRAME_PERIOD;
                        dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
                        double dxPhase2 = Model.BallState.Location.DistanceFrom(PassTarget) - dxPhase1;
                        double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
                        double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;
                        passTime += ((vf - passSpeedPhase2) / ballAccelPhase2) / StaticVariables.FRAME_PERIOD;

                        if (passMotion * 1.0 <= passTime)
                            u = PassSpeed - minPassSpeed;
                        counter++;
                    }
                }
                else
                {
                    passTime = 1.1 * Math.Sqrt(5 * maxPassSpeed) / 5 / StaticVariables.FRAME_PERIOD;
                }
                if (passMotion * 1.0 <= passTime)
                    return true;
                return false;
            }
        }
        #endregion

        #region Enums
        public enum ActiveActionMode
        {
            None,
            Shoot,
            Pass,
            Drible,
            Conflict
        }
        public enum ActivePassKind
        {
            OneTouch,
            Catch,
            Through
        }
        public enum ActiveDribleKind
        {
            SpaceDrible,
            RotateAndKick
        }
        public enum AttacekrState
        {
            WaitForPass,
            CatchPass,
            MarkGotoPoint,
            MarkGoTowardBall,

        }
        public enum PickerState
        {
            GotoPoint,
            Block
        }
        public enum GetBallState
        {
            Incomming = 0,
            Outgoing = 1,
            Static = 2
        }
        public enum ActiveRoleState
        {
            Open2Kick,
            KickWayOpend,
            KickAnyway,
            Sweep,
            Clear,
            Conflict,
            LittleSpace,
            Close2Kick,
            SaveBall,
            Pass
        }

        public enum BallZone
        {
            DangerZone,
            MiddleZone,
            AttackZone
        }
        #endregion
    }

}
