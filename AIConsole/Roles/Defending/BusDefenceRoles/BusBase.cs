using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles.Defending;

namespace MRL.SSL.AIConsole.Roles
{
    public class BusBase
    {
        public static bool firstTime = true;
        public static double angle = 0;
        public static Position2D pos = new Position2D();
        public static Position2D firstPosBallInBehind = new Position2D();
        public static int behindCounter = 0;
        public const double radios = 0.09;
        public static Dictionary<Type, busInfo> BusInfos = new Dictionary<Type, busInfo>();

        public void updateBusState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, busInfo info)
        {
            if (BusInfos.ContainsKey(info.RoleType))
                BusInfos[info.RoleType] = info;
            else
                BusInfos.Add(info.RoleType, info);

        }

        public busInfo? getBusState(Type type)
        {
            if (BusInfos.ContainsKey(type))
                return BusInfos[type];
            return null;
        }


        public static Position2D? OverlapSolvingOLd2(WorldModel Model, Type busType, Position2D target)
        {
            BusBase busbase = new BusBase();
            busInfo? bus = new busInfo();
            bus = busbase.getBusState(busType);
            Position2D? overlapTarget = null;
            double minDist = 0.19;
            Dictionary<Type, busInfo> overlapTargets = new Dictionary<Type, busInfo>();
            foreach (var item in BusInfos.Where(w => w.Key != busType))
            {
                if (bus.HasValue && bus.Value.Location.DistanceFrom(item.Value.Location) < minDist)
                {
                    overlapTargets.Add(item.Key, item.Value);
                }
            }
            if (overlapTargets.Count > 0 && bus.HasValue)
            {
                int minAngle = 9;
                overlapTargets.Add(busType, bus.Value);
                int d2 = 0;
                busInfo leftPos = new busInfo();
                busInfo rightPos = new busInfo();
                Dictionary<Type, Position2D> sortTarget = new Dictionary<Type, Position2D>();
                foreach (var item in overlapTargets)
                {

                }
                double minIndex = 4;
                foreach (var item in overlapTargets)
                {
                    if (item.Value.DefenderIndex < minIndex)
                    {
                        minIndex = item.Value.DefenderIndex;
                        sortTarget.Add(item.Key, item.Value.Location);
                    }
                }

                Vector2D v1 = sortTarget.FirstOrDefault().Value - GameParameters.OppGoalCenter;
                foreach (var item in sortTarget)
                {
                    Position2D p = GameParameters.OppGoalCenter + v1;
                    bus = busbase.getBusState(item.Key);
                    overlapTargets[item.Key] = new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, p, bus.Value.DefenderIndex, bus.Value.BusOverlapLeft, true);
                    if (busType == item.Key)
                    {
                        overlapTarget = p;
                        break;
                    }
                    v1=Vector2D.FromAngleSize(v1.AngleInRadians+(minAngle*Math.PI/180),v1.Size);
                }
            }

            //return target;
            return overlapTarget;
        }

        public static Position2D? OverlapSolvingold1(WorldModel Model, Type busType, Position2D target)
        {
            Position2D? overlaptarget = null;
            BusBase busbase = new BusBase();
            busInfo? bus = new busInfo();
            bus = busbase.getBusState(busType);
            int minAngle = 8;
            Vector2D v1 = target - GameParameters.OurGoalCenter;
            int d1 = 0, d2 = 0, deltaD;
            Dictionary<Type, busInfo> updatedBus = new Dictionary<Type, busInfo>();
            int leftCount = 1, rightCount = 1;
            foreach (var item in BusInfos.Where(w => w.Value.RoleType != busType))
            {
                Vector2D v2 = item.Value.Location - GameParameters.OurGoalCenter;
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(v1, v2)) < minAngle)
                {
                    d1 = (int)(v1.AngleInDegrees < 0 ? convertAngleP(v1.AngleInDegrees) : v1.AngleInDegrees);
                    d2 = (int)(v2.AngleInDegrees < 0 ? convertAngleP(v2.AngleInDegrees) : v2.AngleInDegrees);
                    int diffrenceD = (int)Math.Abs(minAngle - Math.Abs(Math.Abs(d1) - Math.Abs(d2)));
                    deltaD = (diffrenceD == 0 ? 2 : diffrenceD);
                    deltaD = ((deltaD / 2) == 0 ? deltaD : deltaD + 1);
                    DrawingObjects.AddObject(new StringDraw("d1 real: " + d1.ToString(), new Position2D(0.2, 3.5)));
                    if (d1 <= 85 || d1 >= 265)
                    {
                        overlaptarget = target;
                    }
                    else
                    {
                        if (bus.Value.DefenderIndex < item.Value.DefenderIndex)
                        {
                            //leftCount++;
                            for (int i = d1; ; i--)
                            {
                                int newD = (int)(i > 180 ? convertAngleM(i) : i);
                                Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                                Position2D newLocation = GameParameters.OurGoalCenter + v;
                                Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                                Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                                posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                                DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                                overlaptarget = posOnDangerzon;
                                if (i < d1 - ((!item.Value.BusOverlapRight ? (deltaD / 2) : deltaD) * leftCount))
                                    break;
                            }
                            if (updatedBus.ContainsKey(busType))
                                updatedBus[busType] = new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, bus.Value.BusOverlapLeft, true);
                            else
                                updatedBus.Add(busType, new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, bus.Value.BusOverlapLeft, true));
                        }
                        else
                        {
                            //rightCount++;
                            for (int i = d1; ; i++)
                            {
                                int newD = (int)(i > 180 ? convertAngleM(i) : i);
                                Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                                Position2D newLocation = GameParameters.OurGoalCenter + v;
                                Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                                Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                                posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                                DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                                overlaptarget = posOnDangerzon;
                                if (i > d1 + ((!item.Value.BusOverlapLeft ? (deltaD / 2) : deltaD) * rightCount))
                                    break;
                            }
                            if (updatedBus.ContainsKey(busType))
                                updatedBus[busType] = new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, true, bus.Value.BusOverlapRight);
                            else
                                updatedBus.Add(busType, new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, true, bus.Value.BusOverlapRight));
                        }
                    }
                }
                else //if (Math.Abs(bus.Value.DefenderIndex - item.Value.DefenderIndex) < 2)
                {
                    bool busOverlapRight = false;
                    bool busOverlapLeft = false;
                    if (bus.HasValue && bus.Value.DefenderIndex < item.Value.DefenderIndex)
                    {
                        if (updatedBus.ContainsKey(busType))
                        {
                            busOverlapRight = updatedBus[busType].BusOverlapRight;// (!updatedBus[busType].BusOverlapRight ? false : bus.Value.BusOverlapRight);
                            busOverlapLeft = updatedBus[busType].BusOverlapLeft;//(!updatedBus[busType].BusOverlapRight ? false : bus.Value.BusOverlapRight);
                            updatedBus[busType] = new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, busOverlapLeft, busOverlapRight);
                        }
                        else
                            updatedBus.Add(busType, new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, busOverlapLeft, busOverlapRight));
                    }
                    else if (bus.HasValue && bus.Value.DefenderIndex > item.Value.DefenderIndex)
                    {
                        if (updatedBus.ContainsKey(busType))
                        {
                            busOverlapRight = updatedBus[busType].BusOverlapRight;// (!updatedBus[busType].BusOverlapRight ? false : bus.Value.BusOverlapRight);
                            busOverlapLeft = updatedBus[busType].BusOverlapLeft;//(!updatedBus[busType].BusOverlapRight ? false : bus.Value.BusOverlapRight);
                            updatedBus[busType] = new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, busOverlapLeft, busOverlapRight);
                        }
                        else
                            updatedBus.Add(busType, new busInfo(bus.Value.RoleType, bus.Value.State, bus.Value.RobotID, bus.Value.RegionID, bus.Value.OppID, bus.Value.angleDegree, bus.Value.Location, overlaptarget, bus.Value.DefenderIndex, busOverlapLeft, busOverlapRight));
                    }
                }
            }

            foreach (var item in updatedBus)
            {
                BusInfos[item.Key] = item.Value;
            }

            return overlaptarget;

        }
        public static Position2D? OverlapSolving(WorldModel Model, Type busType, Position2D target)
        {
            Position2D? overlaptarget = null;
            BusBase busbase = new BusBase();
            busInfo? bus = new busInfo();
            bus = busbase.getBusState(busType);
            int minAngle = 8;
            Vector2D v1 = target - GameParameters.OurGoalCenter;
            int d1 = 0, d2 = 0, deltaD;
            List<Type> leftOverlap = new List<Type>();
            List<Type> rightOverlap = new List<Type>();
            foreach (var item in BusInfos.Where(w => w.Value.RoleType != busType))
            {
                Vector2D v2 = item.Value.Location - GameParameters.OurGoalCenter;
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(v1, v2)) < minAngle)
                {
                    if (bus.Value.DefenderIndex < item.Value.DefenderIndex)
                        rightOverlap.Add(bus.Value.RoleType);

                    if (bus.Value.DefenderIndex > item.Value.DefenderIndex)
                        leftOverlap.Add(bus.Value.RoleType);

                    d1 = (int)(v1.AngleInDegrees < 0 ? convertAngleP(v1.AngleInDegrees) : v1.AngleInDegrees);
                    d2 = (int)(v2.AngleInDegrees < 0 ? convertAngleP(v2.AngleInDegrees) : v2.AngleInDegrees);
                    int diffrenceD = (int)Math.Abs(minAngle - Math.Abs(Math.Abs(d1) - Math.Abs(d2)));
                    deltaD = (diffrenceD == 0 ? 1 : diffrenceD) + 2;
                    DrawingObjects.AddObject(new StringDraw("d1 real: " + d1.ToString(), new Position2D(0.2, 3.5)));
                    DrawingObjects.AddObject(new StringDraw("d2 real: " + d2.ToString(), new Position2D(0.4, 3.5)));
                    if (!item.Value.BusOverlapRight && item.Value.BusOverlapLeft && bus.Value.DefenderIndex < item.Value.DefenderIndex)
                    {
                        //bus.Value.DefenderIndex > item.Value.DefenderIndex
                        for (int i = d1 - (deltaD / 2); ; i--)
                        {
                            d1 = i;
                            int newD = (int)(d1 > 180 ? convertAngleM(d1) : d1);
                            Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                            Position2D newLocation = GameParameters.OurGoalCenter + v;
                            Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                            Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                            posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                            DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                            if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > 20)
                            {
                            }
                            overlaptarget = posOnDangerzon;
                            if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > minAngle / 2)
                                break;
                        }
                    }
                    else if (!item.Value.BusOverlapLeft && item.Value.BusOverlapRight && bus.Value.DefenderIndex > item.Value.DefenderIndex)
                    {
                        for (int i = d1 + (deltaD / 2); ; i++)
                        {
                            d1 = i;
                            int newD = (int)(d1 > 180 ? convertAngleM(d1) : d1);
                            Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                            Position2D newLocation = GameParameters.OurGoalCenter + v;
                            Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                            Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                            posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                            DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                            if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > 20)
                            {
                            }
                            overlaptarget = posOnDangerzon;
                            if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > minAngle / 2)
                                break;
                        }
                    }
                    else if (item.Value.BusOverlapLeft && item.Value.BusOverlapRight)
                    {
                        if (d1 <= 85)
                        {
                            overlaptarget = target;
                            leftOverlap.Add(busType);
                        }
                        else if (d1 >= 265)
                        {
                            overlaptarget = target;
                            rightOverlap.Add(busType);
                        }
                        else
                        {
                            if (d1 == d2)
                            {
                                if (bus.Value.DefenderIndex < item.Value.DefenderIndex)
                                {
                                    for (int i = d1 - (minAngle / 2); ; i--)
                                    {
                                        d1 = i;
                                        int newD = (int)(d1 > 180 ? convertAngleM(d1) : d1);
                                        Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                                        Position2D newLocation = GameParameters.OurGoalCenter + v;
                                        Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                                        Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                                        posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                                        DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                                        if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > 20)
                                        {
                                        }
                                        overlaptarget = posOnDangerzon;
                                        if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > minAngle / 2)
                                            break;
                                    }
                                }
                                else
                                {
                                    for (int i = d1 + (minAngle / 2); ; i++)
                                    {
                                        d1 = i;
                                        int newD = (int)(d1 > 180 ? convertAngleM(d1) : d1);
                                        Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                                        Position2D newLocation = GameParameters.OurGoalCenter + v;
                                        Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                                        Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                                        posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                                        DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                                        if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > 20)
                                        {
                                        }
                                        overlaptarget = posOnDangerzon;
                                        if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > minAngle / 2)
                                            break;
                                    }
                                }
                            }
                            else if (d1 < d2)
                            {
                                for (int i = d1 - (deltaD / 2); ; i--)
                                {
                                    d1 = i;
                                    int newD = (int)(d1 > 180 ? convertAngleM(d1) : d1);
                                    Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                                    Position2D newLocation = GameParameters.OurGoalCenter + v;
                                    Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                                    Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                                    posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                                    DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                                    if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > 20)
                                    {
                                    }
                                    overlaptarget = posOnDangerzon;
                                    if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > minAngle / 2)
                                        break;
                                }
                            }
                            else if (d1 > d2)
                            {
                                for (int i = d1 + (deltaD / 2); ; i++)
                                {
                                    d1 = i;
                                    int newD = (int)(d1 > 180 ? convertAngleM(d1) : d1);
                                    Vector2D v = Vector2D.FromAngleSize(newD * (Math.PI / 180), 10);
                                    Position2D newLocation = GameParameters.OurGoalCenter + v;
                                    Line l1 = new Line(GameParameters.OurGoalCenter, newLocation);
                                    Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                                    posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                                    DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.02f)));
                                    if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > 20)
                                    {
                                    }
                                    overlaptarget = posOnDangerzon;
                                    if (Math.Abs(Math.Abs(d1) - Math.Abs(d2)) > minAngle / 2)
                                        break;
                                }
                            }
                        }
                    }
                }

                DrawingObjects.AddObject(new StringDraw("d1 fake: " + d1.ToString(), new Position2D(0.3, 3.5)));
                DrawingObjects.AddObject(new StringDraw("d2 fake: " + d2.ToString(), new Position2D(0.5, 3.5)));
            }
            foreach (var item in leftOverlap)
            {
                busInfo? b = new busInfo();
                b = busbase.getBusState(item);
                BusInfos[item] = new busInfo(b.Value.RoleType, b.Value.State, b.Value.RobotID, b.Value.RegionID, b.Value.OppID, b.Value.angleDegree, b.Value.Location, overlaptarget, b.Value.DefenderIndex, true, b.Value.BusOverlapRight);
            }
            foreach (var item in rightOverlap)
            {
                busInfo? b = new busInfo();
                b = busbase.getBusState(item);
                BusInfos[item] = new busInfo(b.Value.RoleType, b.Value.State, b.Value.RobotID, b.Value.RegionID, b.Value.OppID, b.Value.angleDegree, b.Value.Location, overlaptarget, b.Value.DefenderIndex, b.Value.BusOverlapLeft, true);
            }
            return overlaptarget;
        }

        public static double convertAngleP(double angle)
        {
            if (angle < 0)
                return 180 + (180 - Math.Abs(angle));
            else
                return angle;
        }
        public static double convertAngleM(double angle)
        {
            if (angle > 180)
                return (-180) + (180 - angle);
            else
                return angle;
        }

        public static bool BallInBehind(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            double tempDist, tempDist2;
            if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.13, out tempDist, out tempDist2) && !GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out tempDist, out tempDist2) && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .30 && Model.BallState.Speed.Size < .05)
            {
                DataBridge.BallBehind = true;
                DataBridge.BallBehindID = RobotID;
                return true;
            }
            return false;
        }

        public static Position2D BehindSatate(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta, Dictionary<RoleBase, int> CurrentStates)
        {
            Position2D target = new Position2D();
            SingleObjectState behind = BehindTarget(Model, RobotID);
            Teta = (Model.OurRobots[RobotID].Location - Model.BallState.Location).AngleInDegrees; //behind.Angle.Value;
            target = behind.Location;
            DataBridge.BallBehindPos = target;
            DataBridge.BallBehindangle = Teta;
            return target;
        }

        public static SingleObjectState BehindTarget(WorldModel Model, int RobotID)
        {
            if (Model.BallState.Location.DistanceFrom(firstPosBallInBehind) > 0.05)
                firstTime = true;
            if (firstTime)
            {
                firstTime = false;
                angle = Model.OurRobots[RobotID].Angle.Value;
                pos = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.14);
                firstPosBallInBehind = Model.BallState.Location;
            }
            if (Model.OurRobots[RobotID].Location.DistanceFrom(pos) < .03)
            {
                behindCounter++;
            }
            else
            {
                behindCounter = 0;
            }
            if (behindCounter > 40)
            {
                pos = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.06);
            }
            DrawingObjects.AddObject(new Circle(pos, 0.05));
            return new SingleObjectState(pos, Vector2D.Zero, (float)angle);

        }

        public static Position2D MarkFront(GameStrategyEngine engine, WorldModel Model, int RobotID, int? opp, double margin, out double Teta)
        {
            SingleObjectState Target = (opp != null && opp.HasValue) ? Model.Opponents[opp.Value] : Model.BallState;
            Teta = (Target.Location - GameParameters.OurGoalCenter).AngleInDegrees;
            double min = GameParameters.SafeRadi(Target, margin);
            Position2D Pos = GameParameters.OurGoalCenter - (GameParameters.OurGoalCenter - Target.Location).GetNormalizeToCopy(min);
            Pos.DrawColor = System.Drawing.Color.Red;
            DrawingObjects.AddObject(Pos);
            return Pos;
        }

        public static bool BallKickedToGoal(GameStrategyEngine engine, WorldModel Model)
        {
            Line line = new Line();
            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            DrawingObjects.AddObject(new Circle(BallGoal, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Orange, 0.02f)), "kick123456To123456Goal");
            double d = Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.15 && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.15)
                if (Model.BallState.Speed.InnerProduct(GameParameters.OurGoalRight - Model.BallState.Location) > 0)
                    if (Model.BallState.Speed.Size > 0.1 && d / Model.BallState.Speed.Size < 2.2)
                        return true;
            return false;
        }
    }

    public struct BusRegion
    {
        public int region;
        public int key;
        public double value;
        public SingleObjectState opp;
        public BusRegion getBusRegion(int _region, int _key, double _value, SingleObjectState _opp)
        {
            BusRegion br = new BusRegion();
            br.region = _region;
            br.key = _key;
            br.value = _value;
            br.opp = _opp;
            return br;
        }
    }

    public struct busInfo
    {
        public Type RoleType;
        public int State;
        public int RobotID;
        public int DefenderIndex;
        public int RegionID;
        public int? OppID;
        public double angleDegree;
        public Position2D Location;
        public Position2D? OverlapLocation;
        public bool BusOverlapLeft;
        public bool BusOverlapRight;
        public busInfo(Type _RoleType, int _State, int _RobotID, int _RegionID, int? _OppID, double _angleDegree, Position2D _Location, Position2D? _OverlapLocation, int _DefenderIndex, bool _BusOverlapLeft = false, bool _BusOverlapRight = false)
        {
            RoleType = _RoleType;
            State = _State;
            RobotID = _RobotID;
            RegionID = _RegionID;
            OppID = _OppID;
            angleDegree = _angleDegree;
            Location = _Location;
            OverlapLocation = _OverlapLocation;
            DefenderIndex = _DefenderIndex;
            BusOverlapLeft = _BusOverlapLeft;
            BusOverlapRight = _BusOverlapRight;
        }
    }

    public enum BusState
    {
        Regional,
        InPenaltyArea,
        BallInBehind,
        BallInFront,
        KickToGoal,
        KickToRobot,
        Mark
    }
}
