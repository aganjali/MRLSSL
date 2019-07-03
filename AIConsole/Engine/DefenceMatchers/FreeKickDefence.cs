using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions.General_Settings;
using System.Drawing;
using System.IO;
using System.Xml;
using MRL.SSL.Planning.GamePlanner;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Engine
{

    public static class FreekickDefence
    {

#if NEW

        #region newFreeKickDefence
        public static double AdditionalSafeRadi = 0.08;

        public static bool testDefenceState = false;

        public static bool newmotion = true;

        public static bool RearRegional = false;
        public static bool WeAreInCorner = false;
        public static int? LastOppToMark = null;
        public static int? LastOppToMark1 = null;
        public static int? LastOppToMark1Fake = null;
        public static int? LastOppToMark2 = null;
        public static int? LastOppToMark2Fake = null;
        public static int? LastOppToMark3 = null;
        public static int? LastOppToMark4 = null;

        public static bool switchAllMode = false;

        public static int? Static1ID = null;
        public static int? Static2ID = null;
        public static int? StaticCBID = null;
        public static int? StaticLBID = null;
        public static int? StaticRBID = null;

        public static int? OppToMark1 = null;
        public static int? OppToMark1Fake = null;
        public static int? OppToMark2 = null;
        public static int? OppToMark2Fake = null;
        public static int? OppToMark3 = null;
        public static int? OppToMark4 = null;
        public static int? OppToMarkRegion = null;

        public static bool SwitchToActiveMarker1 = false;
        public static bool SwitchToActiveMarker2 = false;
        public static bool SwitchToActiveMarker3 = false;

        public static bool SwitchDefender2Marker1 = false;
        public static bool SwitchDefender2Marker2 = false;
        public static bool SwitchDefender2Marker3 = false;

        public static bool SwitchDefender32Marker1 = false;
        public static bool SwitchDefender32Marker2 = false;
        public static bool SwitchDefender32Marker3 = false;

        public static bool SwitchDefender42Marker1 = false;
        public static bool SwitchDefender42Marker2 = false;
        public static bool SwitchDefender42Marker3 = false;

        public static bool EaththeBall = false;
        public static bool ReadyForEatStatic1 = false;
        public static bool ReadyForEatStatic2 = false;
        public static bool ReadyForEatStaticCB = false;

        public static bool LastSwitchDefender2Marker1 = false;
        public static bool LastSwitchDefender2Marker2 = false;
        public static bool LastSwitchDefender2Marker3 = false;

        public static bool LastSwitchDefender32Marker1 = false;
        public static bool LastSwitchDefender32Marker2 = false;
        public static bool LastSwitchDefender32Marker3 = false;

        public static bool LastSwitchDefender42Marker1 = false;
        public static bool LastSwitchDefender42Marker2 = false;
        public static bool LastSwitchDefender42Marker3 = false;

        public static bool weAreInKickoff = false;


        public static bool CenterBackRole1ToActive = false;
        public static bool DefenderCornerRole1ToActive = false;
        public static bool DefenderCornerRole2ToActive = false;
        public static bool DefenderCornerRole3ToActive = false;
        public static bool DefenderCornerRole4ToActive = false;
        public static bool DefenderMarkerRole1ToActive = false;
        public static bool DefenderMarkerRole2ToActive = false;
        public static bool DefenderMarkerRole3ToActive = false;
        public static bool DefenderRegionalRole1ToActive = false;
        public static bool DefenderRegionalRole2ToActive = false;
        public static bool DefenderGoToPointToActive = false;


        //mostafa
        public static bool SwitchToActiveLBMarker = true;
        public static bool SwitchToLMMarkerLBMarker = false;
        public static bool SwitchToLFMarkerLBMarker = false;

        public static bool SwitchToActiveCBMarker = false;
        public static bool SwitchToCMNMarkerCBMarker = false;
        public static bool SwitchToCFMMarkerCBMarker = false;

        public static bool SwitchToActiveRBMarker = true;
        public static bool SwitchToRMMarkerRBMarker = false;
        public static bool SwitchToRFMarkerRBMarker = false;

        public static bool SwitchCBMarkerToLBMarker = false;
        public static bool SwitchCBMarkerToRBMarker = false;

        public static bool SwitchLBMarkerToRBMarker = false;
        public static bool SwitchRBMarkerToLBMarker = false;

        public static bool rightFirstSet = true;
        public static bool leftFirstSet = true;

        public static Line LBLine = new Line(GameParameters.OurGoalRight, GameParameters.OppGoalLeft);
        public static Line RBLine = new Line(GameParameters.OurGoalLeft, GameParameters.OppGoalRight);
        public static Line MiddleLine = new Line(GameParameters.OurGoalCenter, GameParameters.OppGoalCenter);

        public static /*centerside*/int CenterCurrentSide = (int)SelectSide.left;
        public static Position2D CenterCurrentPosition = Position2D.Zero;
        public static int oppCount = 0;
        public static int oppCountRight = 0;
        public static int oppCountLeft = 0;
        //

        public static bool OppInLBArea = false;
        public static bool OppInLBAreaReal = false;
        public static bool OppInLBAreaFake = false;
        public static bool OppInRBArea = false;
        public static bool OppInRBAreaFake = false;
        public static bool OppInRBAreaReal = false;

        public static bool freeSwitchbetweenRegionalAndMarker = false;

        public static bool StopToActive = false;

        public static bool DontShitPlease = false;
        public static int firstpicker = 0;
        public static int secondpicker = 0;
        public static int thirdpicker = 0;
        public static double InnerProductValue = 0.05;

        private static RBstate LastRB = RBstate.Ball;
        private static int? LastOwner = null;
        public static CenterDefenderStates StaticCenterState = CenterDefenderStates.Normal;
        public static int centerCurrentState = (int)CenterDefenderStates.Normal;
        public static DefenderStates StaticFirstState = DefenderStates.Normal;
        public static DefenderStates StaticSecondState = DefenderStates.Normal;

        public static SingleObjectState ballState = new SingleObjectState();
        public static SingleObjectState ballStateFast = new SingleObjectState();

        private static Position2D lastfirst = new Position2D(), lastsecond = new Position2D();
        public static bool BallIsMoved = false;
        public static List<DefenceInfo> CurrentInfos = new List<DefenceInfo>();
        public static List<DefenderCommand> CurrentlyAddedDefenders = new List<DefenderCommand>();
        public static DefenceInfo GoalieInfo = new DefenceInfo();
        public static GoaliPositioningMode CurrentGoalieMode = GoaliPositioningMode.InRightSide;
        public static Dictionary<RoleBase, int> CurrentStates = new Dictionary<RoleBase, int>();

        public static Dictionary<Type, Position2D> PreviousPositions = new Dictionary<Type, Position2D>();
        public static bool BallIsMovedStrategy = false;

        public static List<DefenceInfo> Match(GameStrategyEngine engine, WorldModel Model, List<DefenderCommand> Commands, bool OverLapIFirstDefender)
        {
            CurrentlyAddedDefenders = Commands;
            List<DefenceInfo> res = new List<DefenceInfo>();
            DefenceInfo TempGoali = new DefenceInfo();
            DefenderCommand goaliCommand = Commands.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IGoalie))).FirstOrDefault();
            Dictionary<Type, bool> MarkerstoRemove = new Dictionary<Type, bool>();
            Dictionary<Type, bool> FirstCornertoRemove = new Dictionary<Type, bool>();
            Dictionary<Type, bool> OtherstoRemove = new Dictionary<Type, bool>();
            foreach (var item in Commands)
            {
                if (item.RoleType.GetInterfaces().Any(a => a == typeof(CenterBackNormalRole)))
                {
                    DefenceInfo def, goali;
                    def = CalculateNormalFirst(engine, Model, item, out goali, goaliCommand, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))
                        {
                            FirstCornertoRemove[item.RoleType] = true;
                        }
                    }
                    TempGoali = goali;
                    if (item.willUse)
                        res.Add(def);
                }
                else if (item.RoleType.GetInterfaces().Any(a => a == typeof(IFirstDefender)))
                {
                    DefenceInfo def, goali;
                    def = CalculateNormalFirst(engine, Model, item, out goali, goaliCommand, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            FirstCornertoRemove[item.RoleType] = true;
                        }
                    }
                    TempGoali = goali;
                    if (item.willUse)
                        res.Add(def);
                }
                else if (item.RoleType.GetInterfaces().Any(a => a == typeof(ISecondDefender)))
                {
                    DefenceInfo def, goali;
                    def = CalculateNormalSecond(engine, Model, item, out goali, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            OtherstoRemove[item.RoleType] = true;
                        }
                    }
                    res.Add(def);
                }
                else if (item.RoleType.GetInterfaces().Any(a => a == typeof(IFirstDefender)))
                {
                    DefenceInfo def, goali;
                    def = CalculateNormalFirst(engine, Model, item, out goali, goaliCommand, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            FirstCornertoRemove[item.RoleType] = true;
                        }
                    }
                    TempGoali = goali;
                    if (item.willUse)
                        res.Add(def);
                }
                else if (item.RoleType.GetInterfaces().Any(a => a == typeof(ISecondDefender)))
                {
                    DefenceInfo def, goali;
                    def = CalculateNormalSecond(engine, Model, item, out goali, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            OtherstoRemove[item.RoleType] = true;
                        }
                    }
                    res.Add(def);
                }
                else if (item.RoleType.GetInterfaces().Any(a => a == typeof(IMarkerDefender)))
                {
                    DefenceInfo def;
                    def = Mark(engine, Model, item, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            MarkerstoRemove[item.RoleType] = true;
                        }
                    }
                    res.Add(def);
                }
            }
            List<DefenceInfo> temp = res.ToList();

            foreach (var item in Commands.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IRegionalDefender))).ToList())
            {
                DefenceInfo def;
                def = RegionalDefence(engine, Model, item, temp, CurrentInfos);
                if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                {
                    if (PreviousPositions.ContainsKey(item.RoleType))
                    {
                        OtherstoRemove[item.RoleType] = true;
                    }
                }
                res.Add(def);
                temp.Add(def);
            }

            res = OverLapSolving(Model, res, FirstCornertoRemove, MarkerstoRemove, OtherstoRemove, OverLapIFirstDefender, true, true);

            Position2D? CenterBack = null;
            Position2D? LeftBack = null;
            Position2D? RightBack = null;

            Position2D? d = null;
            Position2D? d3 = null;
            Position2D? d4 = null;
            Position2D? m = null;
            Position2D? m2 = null;
            Position2D? m3 = null;
            int markerID = 0;
            for (int i = 0; i < res.Count; i++)
            {
                if (res[i].RoleType == typeof(DefenderCornerRole1) || res[i].RoleType == typeof(DefenderNormalRole1))
                {
                    d = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderCornerRole3))
                {
                    d3 = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderCornerRole4))
                {
                    d4 = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderMarkerRole) || res[i].RoleType == typeof(DefenderMarkerNormalRole1) || res[i].RoleType == typeof(NewDefenderMrkerRole))
                {
                    m = res[i].DefenderPosition;
                    markerID = 1;
                }
                else if (res[i].RoleType == typeof(DefenderMarkerRole2) || res[i].RoleType == typeof(DefenderMarkerNormalRole2) || res[i].RoleType == typeof(NewDefenderMarkerRole2))
                {
                    m2 = res[i].DefenderPosition;
                    markerID = 2;
                }
                else if (res[i].RoleType == typeof(DefenderMarkerRole3) || res[i].RoleType == typeof(DefenderMarkerNormalRole3) || res[i].RoleType == typeof(NewDefenderMarkerRole3))
                {
                    m3 = res[i].DefenderPosition;
                    markerID = 3;
                }
                else if (res[i].RoleType == typeof(CenterBackNormalRole) || res[i].RoleType == typeof(DefenderCornerRole1))
                {
                    CenterBack = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(LeftBackMarkerNormalRole))
                {
                    LeftBack = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(RightBackMarkerNormalRole))
                {
                    RightBack = res[i].DefenderPosition;
                }
                if (res[i].RoleType.GetInterfaces().Any(a => a == typeof(IFirstDefender)) || res[i].RoleType.GetInterfaces().Any(a => a == typeof(ISecondDefender)) || res[i].RoleType.GetInterfaces().Any(a => a == typeof(CenterBackNormalRole)) || res[i].RoleType.GetInterfaces().Any(a => a == typeof(LeftBackMarkerNormalRole)) || res[i].RoleType.GetInterfaces().Any(a => a == typeof(RightBackMarkerNormalRole)))
                    continue;

                Position2D pos = res[i].DefenderPosition.Value;
                res[i].DefenderPosition = CommonDefenceUtils.CheckForStopZone(BallIsMoved, pos, Model);
            }
            SwitchCBMarkerToLBMarker = false;
            SwitchCBMarkerToRBMarker = false;
            if ((LeftBack.HasValue || RightBack.HasValue) && (CenterBack.HasValue))
            {
                double dist, DistancFromBorder;
                double margin = 0.3;

                if ((CenterBack.HasValue && LeftBack.HasValue) && GameParameters.IsInDangerousZone(CenterBack.Value, false, margin, out dist, out DistancFromBorder) && GameParameters.IsInDangerousZone(LeftBack.Value, false, margin, out dist, out DistancFromBorder))
                {
                    SwitchCBMarkerToLBMarker = true;
                }
                if ((CenterBack.HasValue && RightBack.HasValue) && GameParameters.IsInDangerousZone(CenterBack.Value, false, margin, out dist, out DistancFromBorder) && GameParameters.IsInDangerousZone(RightBack.Value, false, margin, out dist, out DistancFromBorder))
                {
                    SwitchCBMarkerToLBMarker = true;
                }

            }
            SwitchDefender2Marker1 = false;
            SwitchDefender2Marker2 = false;
            SwitchDefender2Marker3 = false;
            SwitchDefender32Marker1 = false;
            SwitchDefender32Marker2 = false;
            SwitchDefender32Marker3 = false;
            SwitchDefender42Marker1 = false;
            SwitchDefender42Marker2 = false;
            SwitchDefender42Marker3 = false;

            if ((m.HasValue || m2.HasValue || m3.HasValue) && (d.HasValue || d3.HasValue))
            {
                double dist, DistFromBorder;
                double margin = 0.3;

                if ((m.HasValue && d.HasValue) && GameParameters.IsInDangerousZone(m.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender2Marker1 = true;
                }
                if ((m2.HasValue && d.HasValue) && GameParameters.IsInDangerousZone(m2.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender2Marker2 = true;
                }
                if ((m3.HasValue && d.HasValue) && GameParameters.IsInDangerousZone(m3.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender2Marker3 = true;
                }
                if ((m.HasValue && d3.HasValue) && GameParameters.IsInDangerousZone(m.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d3.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender32Marker1 = true;
                }
                if ((m2.HasValue && d3.HasValue) && GameParameters.IsInDangerousZone(m2.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d3.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender32Marker2 = true;
                }
                if ((m3.HasValue && d3.HasValue) && GameParameters.IsInDangerousZone(m3.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d3.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender32Marker3 = true;
                }
                if ((m.HasValue && d4.HasValue) && GameParameters.IsInDangerousZone(m.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d4.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender42Marker1 = true;
                }
                if ((m2.HasValue && d4.HasValue) && GameParameters.IsInDangerousZone(m2.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d4.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender42Marker2 = true;
                }
                if ((m3.HasValue && d4.HasValue) && GameParameters.IsInDangerousZone(m3.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d4.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender42Marker3 = true;
                }
            }

            res.Add(TempGoali);
            GoalieInfo = TempGoali;
            CurrentInfos = res;
            CurrentStates = new Dictionary<RoleBase, int>();

            return res;
        }
        public static List<DefenceInfo> OverLapSolving(WorldModel Model, List<DefenceInfo> infos, Dictionary<Type, bool> FirstCornertoRemove, Dictionary<Type, bool> MarkerstoRemove, Dictionary<Type, bool> OtherstoRemove, bool OverLapIFirstDefender, bool OverLapIMarkerDefender, bool OverLapOthers)
        {
            List<DefenceInfo> res = new List<DefenceInfo>();
            List<Forbiden> forbidens = new List<Forbiden>();
            List<DefenceInfo> markers = new List<DefenceInfo>();
            DefenceInfo normalfirst = null;
            List<DefenceInfo> others = new List<DefenceInfo>();
            if (OverLapIFirstDefender)
                normalfirst = infos.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IFirstDefender))).FirstOrDefault();
            if (OverLapIMarkerDefender)
                markers = infos.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IMarkerDefender))).ToList();
            if (OverLapOthers)
                others = infos.Where(w => w.RoleType.GetInterfaces().Any(a => a != typeof(IFirstDefender)) && w.RoleType.GetInterfaces().Any(a => a != typeof(IMarkerDefender))).ToList();
            double tresh = 0.02;
            List<bool> useMarkersInOverlap = new List<bool>();
            foreach (var item in markers)
            {
                if (!MarkerstoRemove.ContainsKey(item.RoleType) || !MarkerstoRemove[item.RoleType])
                    useMarkersInOverlap.Add(true);
                else
                    useMarkersInOverlap.Add(false);
            }
            List<bool> useOthersInOverlap = new List<bool>();
            foreach (var item in others)
            {
                if (!OtherstoRemove.ContainsKey(item.RoleType) || !OtherstoRemove[item.RoleType])
                    useOthersInOverlap.Add(true);
                else
                    useOthersInOverlap.Add(false);
            }
            double robotRadius = RobotParameters.OurRobotParams.Diameter / 2;
            if (normalfirst != null && (!FirstCornertoRemove.ContainsKey(normalfirst.RoleType) || !FirstCornertoRemove[normalfirst.RoleType]))
                forbidens.Add(new Forbiden()
                {
                    center = normalfirst.DefenderPosition.Value,
                    radius = robotRadius
                });

            UpdateForbidens(Model, ref forbidens, markers, useMarkersInOverlap, tresh, robotRadius);
            UpdateForbidens(Model, ref forbidens, others, useOthersInOverlap, tresh, robotRadius);

            if (normalfirst != null)
                res.Add(normalfirst);

            res.AddRange(markers);
            res.AddRange(others);

            res.ForEach(p => p.Teta = (p.TargetState.Location - p.DefenderPosition.Value).AngleInDegrees);
            return res;

        }
        private static void UpdateForbidens(WorldModel Model, ref List<Forbiden> forbidens, List<DefenceInfo> defenders, List<bool> useInUpdate, double tresh, double robotRadius)
        {
            for (int i = 0; i < defenders.Count; i++)
            {
                foreach (var item in forbidens)
                {
                    bool overLap = false;
                    if (useInUpdate[i] && defenders[i].DefenderPosition.Value.DistanceFrom(item.center) < robotRadius + item.radius + tresh)
                    {
                        overLap = true;
                        Vector2D vec = defenders[i].DefenderPosition.Value - item.center, vec2;
                        if (vec.Size == 0)
                        {
                            vec = defenders[i].TargetState.Location - defenders[i].DefenderPosition.Value;
                            if (defenders[i].DefenderPosition.Value.Y >= 0)
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI / 2, 1);
                            else
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, 1);
                            defenders[i].DefenderPosition = item.center + vec2.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                        }
                        else
                            defenders[i].DefenderPosition = item.center + vec.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                    }
                    double dist, DistFromBorder;
                    if (overLap && GameParameters.IsInDangerousZone(defenders[i].DefenderPosition.Value, false, 0, out dist, out DistFromBorder))// GameParameters.SafeRadi(defenders[i].TargetState, 0)) ;// (RobotParameters.OurRobotParams.Diameter / 2) + .9)
                    {
                        Vector2D robotourgoal = (item.center - GameParameters.OurGoalCenter).GetNormalizeToCopy(item.center.DistanceFrom(GameParameters.OurGoalCenter));
                        Position2D extendpoint = item.center;
                        Vector2D robotitem = Vector2D.FromAngleSize((-robotourgoal).AngleInRadians + (Math.PI / 2), item.radius + robotRadius + .02);//(extendpoint -/* item.center*/ defenders[i].DefenderPosition.Value).GetNormalizeToCopy((item.radius + robotRadius + .02));

                        Position2D extendpoint2 = extendpoint + robotitem;
                        Position2D extendpoint3 = extendpoint - robotitem;
                        if (extendpoint3.DistanceFrom(defenders[i].DefenderPosition.Value) < extendpoint2.DistanceFrom(defenders[i].DefenderPosition.Value))
                        {
                            extendpoint2 = extendpoint3;
                        }

                        if (extendpoint2.X > 3.15 || extendpoint2.DistanceFrom(GameParameters.OurGoalCenter) < item.center.DistanceFrom(GameParameters.OurGoalCenter))
                        {
                            if (extendpoint2 == extendpoint3)
                                extendpoint2 = extendpoint + robotitem;
                            else
                                extendpoint2 = extendpoint - robotitem;
                        }
                        Vector2D reverse = GameParameters.InRefrence(extendpoint2 - extendpoint, robotourgoal);
                        if (!BallIsMoved && Model.Status != GameStatus.Normal)
                        {
                            if (extendpoint2.DistanceFrom(ballState.Location) < .6)
                            {
                                Vector2D TargetBall = extendpoint2 - ballState.Location;
                                extendpoint2 = ballState.Location + TargetBall.GetNormalizeToCopy(.7);
                            }
                        }
                        defenders[i].DefenderPosition = extendpoint2;
                    }
                }
                Forbiden temp = new Forbiden()
                {
                    center = defenders[i].DefenderPosition.Value,
                    radius = robotRadius
                };

                foreach (var item in forbidens.ToList())
                {
                    if (temp.center.DistanceFrom(item.center) < (item.radius + temp.radius + robotRadius * 2))
                    {
                        temp = Merge(item, temp);
                        forbidens.Remove(item);
                    }
                }
                // DrawingObjects.AddObject(new Circle(temp.center, temp.radius, new Pen(Color.Red, .02f)));
                forbidens.Add(temp);
            }
        }
        private static void UpdateForbidens(WorldModel Model, ref List<Forbiden> forbidens, List<DefenceInfo> defenders, double tresh, double robotRadius)
        {
            for (int i = 0; i < defenders.Count; i++)
            {
                foreach (var item in forbidens)
                {
                    bool overLap = false;
                    if (defenders[i].DefenderPosition.Value.DistanceFrom(item.center) < robotRadius + item.radius + tresh)
                    {
                        overLap = true;
                        Vector2D vec = defenders[i].DefenderPosition.Value - item.center, vec2;
                        if (vec.Size == 0)
                        {
                            vec = defenders[i].TargetState.Location - defenders[i].DefenderPosition.Value;
                            if (defenders[i].DefenderPosition.Value.Y >= 0)
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI / 2, 1);
                            else
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, 1);
                            defenders[i].DefenderPosition = item.center + vec2.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                        }
                        else
                            defenders[i].DefenderPosition = item.center + vec.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                    }
                    double dist, DistFromBorder;
                    if (overLap && GameParameters.IsInDangerousZone(defenders[i].DefenderPosition.Value, false, 0.1, out dist, out DistFromBorder))// GameParameters.SafeRadi(defenders[i].TargetState, 0)) ;// (RobotParameters.OurRobotParams.Diameter / 2) + .9)
                    {
                        Vector2D robotourgoal = (item.center - GameParameters.OurGoalCenter).GetNormalizeToCopy(item.center.DistanceFrom(GameParameters.OurGoalCenter));
                        Position2D extendpoint = item.center;
                        Vector2D robotitem = Vector2D.FromAngleSize((-robotourgoal).AngleInRadians + (Math.PI / 2), item.radius + robotRadius + .02);//(extendpoint -/* item.center*/ defenders[i].DefenderPosition.Value).GetNormalizeToCopy((item.radius + robotRadius + .02));

                        Position2D extendpoint2 = extendpoint + robotitem;
                        Position2D extendpoint3 = extendpoint - robotitem;
                        if (extendpoint3.DistanceFrom(defenders[i].DefenderPosition.Value) < extendpoint2.DistanceFrom(defenders[i].DefenderPosition.Value))
                        {
                            extendpoint2 = extendpoint3;
                        }

                        if (extendpoint2.X > 3.15 || extendpoint2.DistanceFrom(GameParameters.OurGoalCenter) < item.center.DistanceFrom(GameParameters.OurGoalCenter))
                        {
                            if (extendpoint2 == extendpoint3)
                                extendpoint2 = extendpoint + robotitem;
                            else
                                extendpoint2 = extendpoint - robotitem;
                        }
                        Vector2D reverse = GameParameters.InRefrence(extendpoint2 - extendpoint, robotourgoal);
                        if (!BallIsMoved && Model.Status != GameStatus.Normal)
                        {
                            if (extendpoint2.DistanceFrom(ballState.Location) < .68)
                            {
                                Vector2D TargetBall = extendpoint2 - ballState.Location;
                                extendpoint2 = ballState.Location + TargetBall.GetNormalizeToCopy(.7);
                            }
                        }
                        defenders[i].DefenderPosition = extendpoint2;
                    }
                }
                Forbiden temp = new Forbiden()
                {
                    center = defenders[i].DefenderPosition.Value,
                    radius = robotRadius
                };

                foreach (var item in forbidens.ToList())
                {
                    if (temp.center.DistanceFrom(item.center) < (item.radius + temp.radius + robotRadius * 2))
                    {
                        temp = Merge(item, temp);
                        forbidens.Remove(item);
                    }
                }
                forbidens.Add(temp);
            }
        }
        private static Forbiden Merge(Forbiden F1, Forbiden F2)
        {
            Forbiden res = new Forbiden();
            res.radius = (F1.center.DistanceFrom(F2.center) + F1.radius + F2.radius) / 2;
            double dx = res.radius - F1.radius;
            Vector2D vec = F2.center - F1.center;
            res.center = F1.center + vec.GetNormalizeToCopy(dx);
            return res;
        }

        #region New Normal Defender
        //mostafa
        public class Area
        {
            public static double minNearAreaX = 1.25;
            public static double maxNearAreaX = 4;

            public static double minNearMiddleAreaX = 0;
            public static double maxNearMiddleAreaX = 1.25;

            public static double minFarMiddleAreaX = -1.5;
            public static double maxFarMiddleAreaX = 0;

            public static double minFarAreaX = -4;
            public static double maxFarAreaX = -1.5;

            public static double MiddleX = 0;


            public Line Near = new Line(new Position2D(minNearAreaX, 3), new Position2D(minNearAreaX, -3));
            public Line NearMiddle = new Line(new Position2D(minNearMiddleAreaX, 3), new Position2D(minNearMiddleAreaX, -3));
            public Line Middle = new Line(new Position2D(MiddleX, 3), new Position2D(MiddleX, -3));
            public Line FarMiddle = new Line(new Position2D(minFarAreaX, 3), new Position2D(minFarAreaX, -3));
            public Line Far = new Line(new Position2D(minFarAreaX, 3), new Position2D(minFarAreaX, -3));

            List<int> mostSignificateLeftSideOpp = new List<int>();
            List<int> mostSignificateRightSideOpp = new List<int>();

            public PositionMode ChooseBallMode(GameStrategyEngine engine, WorldModel Model, PositionMode currentMode)
            {
                PositionMode tempMode = currentMode;
                double BallX = Model.BallState.Location.X;

                //if (BallX <= minNearAreaX && BallX > maxNearAreaX)
                //{
                tempMode = PositionMode.InNear;
                //}
                //else if (BallX > minNearMiddleAreaX && BallX < maxNearMiddleAreaX)
                //{
                //    tempMode = PositionMode.InNearMiddle;
                //}
                //else if (BallX > minFarMiddleAreaX && BallX < maxFarMiddleAreaX)
                //{
                //    tempMode = PositionMode.InFarMiddle;
                //}
                //else if (BallX <= maxFarAreaX && BallX < minFarAreaX)
                //{
                //    tempMode = PositionMode.InFar;
                //}

                return tempMode;
            }

            public PositionMode ChoosePositionMode(GameStrategyEngine engine, WorldModel Model, int RobotID, bool OppRobot)
            {
                PositionMode tempMode = 0;
                //double TargetX;
                //if (!OppRobot)
                //{
                //    TargetX = Model.OurRobots[RobotID].Location.X;
                //}
                //else
                //{
                //    TargetX = Model.Opponents[RobotID].Location.X;
                //}


                //if (TargetX <= Area.minNearAreaX && TargetX > Area.maxNearAreaX)
                //{
                tempMode = PositionMode.InNear;
                //}
                //else if (TargetX > Area.minNearMiddleAreaX && TargetX < Area.maxNearMiddleAreaX)
                //{
                //    tempMode = PositionMode.InNearMiddle;
                //}
                //else if (TargetX > Area.minFarMiddleAreaX && TargetX < Area.maxFarMiddleAreaX)
                //{
                //    tempMode = PositionMode.InFarMiddle;
                //}
                //else if (TargetX <= Area.maxFarAreaX && TargetX < minFarAreaX)
                //{
                //    tempMode = PositionMode.InFar;
                //}

                return tempMode;
            }
        #region
            //public int? OppRobotsInleftBackArea(GameStrategyEngine Engine, WorldModel Model)
            //{
            //    List<int> temp = new List<int>();
            //    Line oppLineX = new Line();
            //    Position2D? intersectP = new Position2D();
            //    foreach (var item in Model.Opponents.Keys)
            //    {
            //        if (Model.Opponents[item].Location.X > 0)
            //        {
            //            oppLineX = new Line(new Position2D(Model.Opponents[item].Location.X, GameParameters.OurLeftCorner.Y), new Position2D(Model.Opponents[item].Location.X, GameParameters.OurGoalRight.Y));
            //            intersectP = oppLineX.IntersectWithLine(LBLine);
            //            if (intersectP.HasValue)
            //            {
            //                if (Model.Opponents[item].Location.Y < intersectP.Value.Y && !(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == item))
            //                {
            //                    temp.Add(item);
            //                }
            //            }
            //        }
            //    }

            //    //int? id = null;
            //    //FreekickDefence.OppToMark1 = Engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? Engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;
            //    int? retID = null;
            //    if (temp.Count > 0)
            //    {                    
            //        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;//.Last().Key;//.ElementAt(0).Key;
            //        if (retID.HasValue)
            //        {
            //            OppToMark1 = retID.Value;
            //        }
            //    }

            //    return retID;
            //}

            //public int? OppRobotsInRightBackArea(GameStrategyEngine Engine, WorldModel Model)
            //{
            //    List<int> temp = new List<int>();
            //    Line oppLineX = new Line();
            //    Position2D? intersectP = new Position2D();
            //    foreach (var item in Model.Opponents.Keys)
            //    {
            //        if (Model.Opponents[item].Location.X > 0)
            //        {
            //            oppLineX = new Line(new Position2D(Model.Opponents[item].Location.X, GameParameters.OurLeftCorner.Y), new Position2D(Model.Opponents[item].Location.X, GameParameters.OurGoalRight.Y));
            //            intersectP = oppLineX.IntersectWithLine(RBLine);
            //            if (intersectP.HasValue)
            //            {
            //                if (Model.Opponents[item].Location.Y > intersectP.Value.Y && !(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == item) )
            //                {
            //                    temp.Add(item);
            //                }
            //            }
            //        }
            //    }
            //    //int? id = null;
            //    //FreekickDefence.OppToMark1 = Engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? Engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;
            //    int? retID = null;
            //    if (temp.Count > 0)
            //    {
            //        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;//.Last().Key;//.ElementAt(0).Key;
            //        if (retID.HasValue)
            //        {
            //            OppToMark2 = retID.Value;
            //        }
            //    }
            //    return retID;
            //}
        #endregion
            bool markMAxScore = false;
            int markSide = (int)SelectSide.center;
            int lastMarkSide = (int)SelectSide.left;
            public int? OpponnentToMarkeRobots(GameStrategyEngine Engine, WorldModel Model, bool LeftSide, bool RightSide)
            {
                bool left = LeftSide, right = RightSide;
                List<int> oppRobots = new List<int>();
                Line oppLineX = new Line();
                Position2D? intersectP = new Position2D();
                foreach (var item in Model.Opponents.Keys)
                {
                    if (Model.Opponents[item].Location.X > 0)
                    {
                        oppLineX = new Line(new Position2D(Model.Opponents[item].Location.X, GameParameters.OurLeftCorner.Y), new Position2D(Model.Opponents[item].Location.X, GameParameters.OurGoalRight.Y));
                        intersectP = oppLineX.IntersectWithLine(RBLine);
                        if (intersectP.HasValue)
                        {
                            if (!(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == item) && (Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) > 0.7))// || Math.Abs(Model.Opponents[item].Location.Y - Model.BallState.Location.Y)>0.5))
                            {
                                oppRobots.Add(item);
                            }
                        }
                    }
                }
                oppCount = oppRobots.Count;
                int? retIDRegion = null;
                int? retID = null;
                int? retIdFakeRight = null;
                int? retIdFakeLeft = null;
                if (oppRobots.Count > 0)
                {
        #region opp count = 1
                    if (oppRobots.Count == 1)
                    {
                        if (Model.Opponents[oppRobots[0]].Location.Y > 0.5)
                        {
                            lastMarkSide = markSide;
                            markSide = (int)SelectSide.center;
                        }
                        else if (Model.Opponents[oppRobots[0]].Location.Y < -0.5)
                        {
                            lastMarkSide = markSide;
                            markSide = (int)SelectSide.center;
                        }
                        else
                        {
                            markSide = lastMarkSide;
                        }
                        if (markSide == (int)SelectSide.center)
                        {
                            if (StaticLBID.HasValue && StaticRBID.HasValue)
                            {
                                if (Model.Opponents[oppRobots[0]].Location.Y > 0)
                                {
                                    LeftSide = true;
                                    RightSide = false;
                                    LastOppToMark2 = OppToMark2;
                                    OppToMark2 = null;
                                }
                                else
                                {
                                    LeftSide = false;
                                    RightSide = true;
                                    LastOppToMark1 = OppToMark1;
                                    OppToMark1 = null;
                                }
                            }
                            else if (StaticLBID.HasValue && !StaticRBID.HasValue)
                            {
                                LeftSide = true;
                                RightSide = false;
                                LastOppToMark2 = OppToMark2;
                                OppToMark2 = null;
                            }
                            else if (!StaticLBID.HasValue && StaticRBID.HasValue)
                            {
                                LeftSide = false;
                                RightSide = true;
                                LastOppToMark1 = OppToMark1;
                                OppToMark1 = null;
                            }

                        }
                        DrawingObjects.AddObject(new StringDraw("Mark Side:" + markSide.ToString(), GameParameters.OurLeftCorner.Extend(.5, 0)), "g56s4hkjfgs564");
                        if (LeftSide && !RightSide)
                        {
                            if (OppToMark2.HasValue)
                            {
                                foreach (var item in oppRobots)
                                {
                                    if (item == OppToMark2.Value)
                                    {
                                        mostSignificateLeftSideOpp = oppRobots.Where(q => q != OppToMark2.Value).ToList();
                                    }
                                }
                            }
                            else
                            {
                                mostSignificateLeftSideOpp = oppRobots;
                            }
                            if (mostSignificateLeftSideOpp.Count > 0)
                            {
                                retID = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateLeftSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                if (retID.HasValue)
                                {
                                    LastOppToMark1 = OppToMark1;
                                    OppToMark1 = retID.Value;
                                }
                                else
                                {
                                    LastOppToMark1 = OppToMark1;
                                    OppToMark1 = null;
                                }
                            }
                        }
                        else if (!LeftSide && RightSide)
                        {
                            if (OppToMark1.HasValue)
                            {
                                foreach (var item in oppRobots)
                                {
                                    if (item == OppToMark1.Value)
                                    {
                                        mostSignificateRightSideOpp = oppRobots.Where(q => q != OppToMark1.Value).ToList();
                                    }
                                }
                            }
                            else
                            {
                                mostSignificateRightSideOpp = oppRobots;
                            }

                            if (mostSignificateRightSideOpp.Count > 0)
                            {
                                retID = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateRightSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;//.Last().Key;//.ElementAt(0).Key;
                                if (retID.HasValue)
                                {
                                    LastOppToMark2 = OppToMark2;
                                    OppToMark2 = retID.Value;
                                }
                                else
                                {
                                    LastOppToMark2 = OppToMark2;
                                    OppToMark2 = null;
                                }
                            }
                        }
                        if (left && !right && OppToMark1 == null)
                        {
                            retID = null;
                        }
                        if (right && !left && OppToMark2 == null)
                        {
                            retID = null;
                        }
                    }
        #endregion
        #region opp count > 1
                    else if (oppRobots.Count > 1)
                    {
                        /// markin with side
                        if (LeftSide)
                        {
        #region we have two merker
                            if (FreekickDefence.StaticRBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticRBID.Value))
                            {
                                List<int> temp = new List<int>();
                                List<int> tempRegionRight = new List<int>();
                                foreach (var item in Model.Opponents.Keys)
                                {
                                    if (Model.Opponents[item].Location.X > 0)
                                    {
                                        oppLineX = new Line(new Position2D(Model.Opponents[item].Location.X, GameParameters.OurLeftCorner.Y), new Position2D(Model.Opponents[item].Location.X, GameParameters.OurGoalRight.Y));
                                        intersectP = oppLineX.IntersectWithLine(LBLine);
                                        if (intersectP.HasValue)
                                        {
                                            if (Model.Opponents[item].Location.Y > intersectP.Value.Y && !(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == item)
                                                && ((OppToMark2.HasValue && item != OppToMark2.Value) || !OppToMark2.HasValue))
                                            {
                                                temp.Add(item);
                                            }
                                            else if (!(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == item))
                                            {
                                                if ((OppToMark2.HasValue && item != OppToMark2) || !OppToMark2.HasValue)
                                                {
                                                    tempRegionRight.Add(item);
                                                }
                                            }
                                        }
                                    }
                                }
        #region temp
                                if (temp.Count == 1)
                                {
        #region count ==1
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;//.Last().Key;//.ElementAt(0).Key;
                                    if (OppToMark2.HasValue && retID.HasValue && OppToMark2.Value != retID.Value)
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else if (OppToMark2.HasValue && retID.HasValue && OppToMark2.Value == retID.Value && !(FreekickDefence.StaticRBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticRBID.Value)))
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else if (temp.Count > 1 && OppToMark2.HasValue && retID.HasValue && OppToMark2.Value == retID.Value && FreekickDefence.StaticRBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticRBID.Value))
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark1 = OppToMark1;
                                            OppToMark1 = retID.Value;
                                        }
                                    }
                                    else if (!OppToMark2.HasValue)
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else if (temp.Count > 1)
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark1 = OppToMark1;
                                            OppToMark1 = retID.Value;
                                        }
                                    }
        #endregion
                                }
                                else if (temp.Count > 1)
                                {
        #region count ==2
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;//.Last().Key;//.ElementAt(0).Key;
                                    if (OppToMark2.HasValue && retID.HasValue && OppToMark2.Value != retID.Value)
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else if (OppToMark2.HasValue && retID.HasValue && OppToMark2.Value == retID.Value && !(FreekickDefence.StaticRBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticRBID.Value)))
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else if (temp.Count > 1 && OppToMark2.HasValue && retID.HasValue && OppToMark2.Value == retID.Value && FreekickDefence.StaticRBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticRBID.Value))
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark1 = OppToMark1;
                                            OppToMark1 = retID.Value;
                                        }
                                    }
                                    else if (!OppToMark2.HasValue)
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else if (temp.Count > 1)
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark1 = OppToMark1;
                                            OppToMark1 = retID.Value;
                                        }
                                    }
                                    retIdFakeLeft = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                    if (retIdFakeLeft.HasValue && !(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == retIdFakeLeft.Value)
                                                && ((OppToMark2.HasValue && retIdFakeLeft.Value != OppToMark2.Value) || !OppToMark2.HasValue))
                                    {
                                        if (retIdFakeLeft.HasValue)
                                        {
                                            LastOppToMark1Fake = OppToMark1Fake;
                                            OppToMark1Fake = retIdFakeLeft;
                                        }
                                        else
                                        {
                                            LastOppToMark1Fake = OppToMark1Fake;
                                            OppToMark1Fake = null;
                                        }
                                    }
        #endregion
                                }
        #endregion

        #region tempRegion
                                if (tempRegionRight.Count > 0)
                                {
                                    retIDRegion = Engine.GameInfo.OppTeam.Scores.Where(q => tempRegionRight.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                    if (OppToMark2.HasValue && retIDRegion.HasValue && retIDRegion.Value != OppToMark2.Value && !OppToMark1.HasValue)
                                    {
                                        OppToMarkRegion = retIDRegion.Value;
                                        retID = retIDRegion.Value;
                                    }
                                    else
                                    {
                                        retIDRegion = null;
                                        OppToMarkRegion = null;
                                    }
                                    if (retIDRegion.HasValue)
                                    {
                                        OppToMarkRegion = retIDRegion.Value;
                                        foreach (var item in tempRegionRight)
                                        {
                                            if (OppToMarkRegion.HasValue && OppToMarkRegion.Value == item)
                                            {
                                                DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.Red, 0.02f)), Model.Opponents[item].Location.X.ToString() + "575656");
                                            }
                                            else
                                                DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.RoyalBlue, 0.02f)), Model.Opponents[item].Location.X.ToString() + "575656");
                                        }
                                    }
                                    else
                                    {
                                        OppToMarkRegion = null;
                                    }
                                }
        #endregion

                            }
        #endregion
        #region we have one marker
                            else
                            {
                                mostSignificateLeftSideOpp = oppRobots.ToList();
                                if (mostSignificateLeftSideOpp.Count == 1)
                                {
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateLeftSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                    if (retID.HasValue)
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = null;
                                    }
                                }
                                else if (mostSignificateLeftSideOpp.Count > 1)
                                {
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateLeftSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                    if (retID.HasValue)
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = retID.Value;
                                    }
                                    else
                                    {
                                        LastOppToMark1 = OppToMark1;
                                        OppToMark1 = null;
                                    }
                                    retIdFakeLeft = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateLeftSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                    if (retIdFakeLeft.HasValue && !(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == retIdFakeLeft.Value)
                                                && ((OppToMark2.HasValue && retIdFakeLeft.Value != OppToMark2.Value) || !OppToMark2.HasValue))
                                    {
                                        if (retIdFakeLeft.HasValue)
                                        {
                                            LastOppToMark1Fake = OppToMark1Fake;
                                            OppToMark1Fake = retIdFakeLeft;
                                        }
                                        else
                                        {
                                            LastOppToMark1Fake = OppToMark1Fake;
                                            OppToMark1Fake = null;
                                        }
                                    }
                                }
                            }
        #endregion

                            //return retID;
                        }
                        if (RightSide)
                        {
        #region we have two merker
                            if (FreekickDefence.StaticLBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticLBID.Value))
                            {
                                List<int> temp = new List<int>();
                                List<int> tempRegionLeft = new List<int>();
                                foreach (var item in Model.Opponents.Keys)
                                {
                                    if (Model.Opponents[item].Location.X > 0)
                                    {
                                        oppLineX = new Line(new Position2D(Model.Opponents[item].Location.X, GameParameters.OurLeftCorner.Y), new Position2D(Model.Opponents[item].Location.X, GameParameters.OurGoalRight.Y));
                                        intersectP = oppLineX.IntersectWithLine(RBLine);
                                        if (intersectP.HasValue)
                                        {
                                            if (Model.Opponents[item].Location.Y < intersectP.Value.Y && !(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == item)
                                                && ((OppToMark1.HasValue && item != OppToMark1) || !OppToMark1.HasValue))
                                            {
                                                temp.Add(item);
                                            }
                                            else if (!(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == item))
                                            {
                                                if ((OppToMark1.HasValue && item != OppToMark1.Value) || !OppToMark1.HasValue)
                                                {
                                                    tempRegionLeft.Add(item);
                                                }
                                            }
                                        }
                                    }
                                }

        #region temp
                                //oppCountRight = temp.Count;
                                if (temp.Count == 1)
                                {
        #region count==1
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;//.Last().Key;//.ElementAt(0).Key;
                                    if (OppToMark1.HasValue && retID.HasValue && OppToMark1.Value != retID.Value)
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else if (OppToMark1.HasValue && retID.HasValue && OppToMark1.Value == retID.Value && !(FreekickDefence.StaticLBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticLBID.Value)))
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else if (temp.Count > 1 && OppToMark1.HasValue && retID.HasValue && OppToMark1.Value == retID.Value && FreekickDefence.StaticLBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticLBID.Value))
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark2 = OppToMark2;
                                            OppToMark2 = retID.Value;
                                        }
                                    }
                                    else if (!OppToMark1.HasValue)
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else if (temp.Count > 1)
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark2 = OppToMark2;
                                            OppToMark2 = retID.Value;
                                        }
                                    }
        #endregion
                                }
                                else if (temp.Count > 1)
                                {
        #region count ==2
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                    if (OppToMark1.HasValue && retID.HasValue && OppToMark1.Value != retID.Value)
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else if (OppToMark1.HasValue && retID.HasValue && OppToMark1.Value == retID.Value && !(FreekickDefence.StaticLBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticLBID.Value)))
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else if (temp.Count > 1 && OppToMark1.HasValue && retID.HasValue && OppToMark1.Value == retID.Value && FreekickDefence.StaticLBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticLBID.Value))
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark2 = OppToMark2;
                                            OppToMark2 = retID.Value;
                                        }
                                    }
                                    else if (!OppToMark1.HasValue)
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else if (temp.Count > 1)
                                    {
                                        retID = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;//.Last().Key;//.ElementAt(0).Key;
                                        if (retID.HasValue)
                                        {
                                            LastOppToMark2 = OppToMark2;
                                            OppToMark2 = retID.Value;
                                        }
                                    }

                                    retIdFakeRight = Engine.GameInfo.OppTeam.Scores.Where(q => temp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                    if (retIdFakeRight.HasValue && !(Engine.GameInfo.OppTeam.BallOwner.HasValue && Engine.GameInfo.OppTeam.BallOwner.Value == retIdFakeRight.Value)
                                                && ((OppToMark1.HasValue && retIdFakeRight.Value != OppToMark1) || !OppToMark1.HasValue))
                                    {
                                        if (retIdFakeRight.HasValue)
                                        {
                                            LastOppToMark2Fake = OppToMark2Fake;
                                            OppToMark2Fake = retIdFakeRight;
                                        }
                                        else
                                        {
                                            LastOppToMark2Fake = OppToMark2Fake;
                                            OppToMark2Fake = null;
                                        }
                                    }
        #endregion
                                }
        #endregion

        #region tempRegion
                                if (tempRegionLeft.Count > 0)
                                {
                                    retIDRegion = Engine.GameInfo.OppTeam.Scores.Where(q => tempRegionLeft.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                    if (OppToMark1.HasValue && retIDRegion.HasValue && retIDRegion.Value != OppToMark1.Value && !OppToMark2.HasValue)
                                    {
                                        OppToMarkRegion = retIDRegion.Value;
                                        retID = retIDRegion.Value;
                                    }
                                    else
                                    {
                                        retIDRegion = null;
                                        OppToMarkRegion = null;
                                    }
                                    foreach (var item in tempRegionLeft)
                                    {
                                        if (OppToMarkRegion.HasValue && OppToMarkRegion.Value == item)
                                        {
                                            DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.Red, 0.02f)), Model.Opponents[item].Location.X.ToString() + "575656");
                                        }
                                        else
                                            DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.RoyalBlue, 0.02f)), Model.Opponents[item].Location.X.ToString() + "575656");
                                    }

                                    //if (retID == null && !(OppToMark2.HasValue && Model.Opponents.ContainsKey(OppToMark2.Value)) && retIDRegion.Value != LastOppToMark2.Value)
                                    //{
                                    //    retID = retIDRegion.Value;
                                    //}
                                }
        #endregion
                            }
        #endregion
        #region we have one marker
                            else
                            {
                                mostSignificateRightSideOpp = oppRobots.ToList();
                                if (mostSignificateRightSideOpp.Count == 1)
                                {
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateRightSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                    if (retID.HasValue)
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = null;
                                    }
                                }
                                else if (mostSignificateRightSideOpp.Count > 1)
                                {
                                    retID = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateRightSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(0).Key;
                                    if (retID.HasValue)
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = retID.Value;
                                    }
                                    else
                                    {
                                        LastOppToMark2 = OppToMark2;
                                        OppToMark2 = null;
                                    }
                                    retIdFakeRight = Engine.GameInfo.OppTeam.Scores.Where(q => mostSignificateRightSideOpp.Contains(q.Key)).OrderByDescending(w => w.Value).ElementAt(1).Key;
                                    if (retIdFakeRight.HasValue)
                                    {
                                        LastOppToMark2Fake = OppToMark2Fake;
                                        OppToMark2Fake = retIdFakeRight;
                                    }
                                    else
                                    {
                                        LastOppToMark2Fake = OppToMark2Fake;
                                        OppToMark2Fake = null;
                                    }
                                }
                            }
        #endregion
                            //return retID;
                        }
                    }
        #endregion
                }

        #region region markID To opp markID
                //if (retIDRegion.HasValue)
                //{
                //    if (!OppToMark1.HasValue)
                //    {
                //        LastOppToMark1 = OppToMark1;
                //        OppToMark1 = retIDRegion.Value;
                //        retID = OppToMark1;
                //    }
                //    else if (!OppToMark2.HasValue)
                //    {
                //        LastOppToMark2 = OppToMark2;
                //        OppToMark2 = retIDRegion.Value;
                //        retID = OppToMark2;
                //    }
                //    else if (!OppToMark1.HasValue && !OppToMark2.HasValue)
                //    {
                //        LastOppToMark1 = OppToMark1;
                //        OppToMark1 = retIDRegion.Value;
                //        retID = OppToMark1;
                //    }
                //}
                //else if (!retIDRegion.HasValue && !OppToMark1.HasValue && !OppToMark2.HasValue)
                //{
                //    retID = null;
                //}
        #endregion


                return retID;
            }
            public Line Interval(GameStrategyEngine Engine, WorldModel Model)
            {
                Line line = new Line();
                Regions regions = new Regions();
                List<VisibleGoalInterval> ourGoalIntervals = new List<VisibleGoalInterval>();
                List<VisibleGoalInterval> oppGoalIntervals = new List<VisibleGoalInterval>();
                List<WorldModel> model = new List<WorldModel>();
                //(List<WorldModel> model, out List<VisibleGoalInterval> ourGoalIntervals, out List<VisibleGoalInterval> oppGoalIntervals, bool useOpp, bool useOur)
                regions.CalculateGoalIntervals(model, out ourGoalIntervals, out oppGoalIntervals, false, true);
                List<Position2D> p = new List<Position2D>();
                foreach (var item in ourGoalIntervals)
                {

                    p.Add(new Position2D(GameParameters.OurGoalCenter.X, item.ViasibleWidth));
                }

                return line;
            }

            private int? GetOppBallOwnerID(WorldModel Model, GamePlannerInfo GPInfo)
            {
                int? oppBallOwner = null;
                Circle c = new Circle(Position2D.Zero, 0.5);
                List<int> listID = new List<int>();
                double minDist = 10;
                double dist = 0;
                foreach (var item in Model.Opponents)
                {
                    if (!GPInfo.OppTeam.GoaliID.HasValue || (GPInfo.OppTeam.GoaliID.HasValue && GPInfo.OppTeam.GoaliID.Value != item.Key))
                    {
                        c = new Circle(item.Value.Location, 0.5);
                        if (c.Center.DistanceFrom(Model.BallState.Location) <= c.Radious)
                        {
                            if (GPInfo.OppTeam.CatchBallLines.ContainsKey(item.Key) && GPInfo.OppTeam.CatchBallLines[item.Key].Count > 0)
                            {
                                dist = c.Center.DistanceFrom(Model.BallState.Location);
                                if (dist < c.Radious)
                                {
                                    if (dist < minDist)
                                    {
                                        minDist = dist;
                                        oppBallOwner = item.Key;
                                    }
                                }
                            }
                        }
                    }
                }
                lastOppBallOwner = oppBallOwner;
                return oppBallOwner;
            }
            int? lastOppBallOwner = null;
            int? lastOwnerID = null;
        }

        #endregion

        #region NormalDefender
        class FirstBounds
        {
            public double minGoaliDist = 0.4;
            public double maxGoaliDist = 0.65;
            public double minGoalidx = 0.11;
            public double mindefX = 1.1;
            public double maxdefX = 1.5;
            public double margin = 0;
            public double prepAng;
            public Position2D FirstGoalCorner = new Position2D();
            public Position2D SecondGoalCorner = new Position2D();
            public Position2D TargetPos = new Position2D();
            public double GoaliX = 0;
            public double GoaliDist = 0;
            public double DefenderDist = 0;
            public Line FirstGoalLine = new Line();
            public Line SecondGoalLine = new Line();
        }
        private static GoaliPositioningMode ChooseMode(SingleObjectState TargetState, GoaliPositioningMode currentMode)
        {
            GoaliPositioningMode temp = currentMode;
            if (TargetState.Location.Y < -0.2)
                temp = GoaliPositioningMode.InLeftSide;
            else if (TargetState.Location.Y > 0.2)
                temp = GoaliPositioningMode.InRightSide;
            return temp;
        }
        private static FirstBounds CalculateBounds(WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode)
        {
            FirstBounds firstbounds = new FirstBounds();
            firstbounds.mindefX = GameParameters.SafeRadi(TargetState, FreekickDefence.AdditionalSafeRadi);
            firstbounds.maxGoaliDist = 0.6;
            if (Mode == GoaliPositioningMode.InRightSide)
            {
                firstbounds.margin = -0.02;
                firstbounds.prepAng = -Math.PI / 2;
                firstbounds.FirstGoalCorner = GameParameters.OurGoalRight;
                firstbounds.SecondGoalCorner = GameParameters.OurGoalLeft;
            }
            else
            {
                firstbounds.margin = 0.02;
                firstbounds.prepAng = Math.PI / 2;
                firstbounds.FirstGoalCorner = GameParameters.OurGoalLeft;
                firstbounds.SecondGoalCorner = GameParameters.OurGoalRight;
            }
            firstbounds.TargetPos = new Position2D(Math.Min(TargetState.Location.X, GameParameters.OurGoalCenter.X - 0.12), Math.Sign(TargetState.Location.Y) * Math.Min(Math.Abs(TargetState.Location.Y), GameParameters.OurLeftCorner.Y + 0.3));
            //if (TargetState.Location.X < -GameParameters.OurGoalCenter.X / 3)
            //    maxdefX = 2.5;
            if (firstbounds.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) <= firstbounds.mindefX)
            {
                firstbounds.mindefX = Math.Max(firstbounds.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) + 0.1, firstbounds.mindefX);
                firstbounds.maxdefX = Math.Max(firstbounds.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) + 0.1, firstbounds.mindefX);
            }
            double db = firstbounds.TargetPos.DistanceFrom(GameParameters.OurGoalCenter);
            double dbx = Math.Abs(firstbounds.TargetPos.X - GameParameters.OurGoalCenter.X);

            db = db / (GameParameters.OurGoalCenter.DistanceFrom(new Position2D(0, 0) + new Vector2D(-1.5, 0)));
            db = (db > 1) ? 1 : db;

            dbx = dbx / (GameParameters.OurGoalCenter.X + 1.5);
            dbx = (dbx > 1) ? 1 : dbx;

            firstbounds.GoaliDist = firstbounds.minGoaliDist + db * (firstbounds.maxGoaliDist - firstbounds.minGoaliDist);
            firstbounds.GoaliX = firstbounds.minGoalidx + dbx * (firstbounds.maxGoaliDist - firstbounds.minGoalidx);

            double ddb = firstbounds.TargetPos.DistanceFrom(GameParameters.OurGoalCenter);
            ddb -= (GameParameters.SafeRadi(TargetState, 0) + 0.4);/////////////
            ddb = ddb / (GameParameters.OurGoalCenter.DistanceFrom(new Position2D(0, 0) + new Vector2D(-2, 0)));
            ddb = (ddb > 1) ? 1 : ddb;
            ddb = (ddb < 0) ? 0 : ddb;
            firstbounds.DefenderDist = firstbounds.mindefX + ddb * (firstbounds.maxdefX - firstbounds.mindefX);
            firstbounds.FirstGoalLine = new Line(firstbounds.TargetPos, new Position2D(firstbounds.FirstGoalCorner.X, firstbounds.FirstGoalCorner.Y + firstbounds.margin));
            firstbounds.SecondGoalLine = new Line(firstbounds.TargetPos, new Position2D(firstbounds.SecondGoalCorner.X, firstbounds.SecondGoalCorner.Y - firstbounds.margin));
            return firstbounds;
        }
        private static Position2D CalculateGoaliPos(WorldModel Model, SingleObjectState TargetState, FirstBounds fb, out bool isOnGoalLine, out Line DefendTargetGoaliLine)
        {
            Position2D GoaliPos;
            Circle goaliBands = new Circle(GameParameters.OurGoalCenter, fb.GoaliDist);
            Vector2D FGoalVec = fb.FirstGoalLine.Head - fb.FirstGoalLine.Tail;
            isOnGoalLine = false;
            Vector2D tmpVec = Vector2D.FromAngleSize(FGoalVec.AngleInRadians + fb.prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            DefendTargetGoaliLine = new Line(fb.FirstGoalLine.Head + tmpVec, (fb.FirstGoalLine.Head + tmpVec) - FGoalVec);
            List<Position2D> Intersects;
            Intersects = goaliBands.Intersect(DefendTargetGoaliLine);
            if (Intersects.Count == 0)
            {
                Line l = new Line(fb.FirstGoalCorner, goaliBands.Center);
                List<Position2D> tmpInts = goaliBands.Intersect(l);
                GoaliPos = (Math.Sign(tmpInts[0].Y) == Math.Sign(fb.FirstGoalCorner.Y)) ? tmpInts[0] : tmpInts[1];
            }
            else if (Intersects.Count == 1)
                GoaliPos = Intersects[0];
            else
                GoaliPos = (Intersects[0].X < Intersects[1].X) ? Intersects[0] : Intersects[1];
            if (GameParameters.OurGoalCenter.X - GoaliPos.X > fb.GoaliX)
            {
                Line tmpLine = new Line(new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, -1), new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, 1));
                Position2D? tmpp = tmpLine.IntersectWithLine(DefendTargetGoaliLine);
                GoaliPos = tmpp.Value;
                isOnGoalLine = true;
            }

            return GoaliPos;
        }
        private static Position2D? CalculateDefenderPos(WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode, Position2D GoaliPos, FirstBounds fb, out Line DefenderBoundLine1, out Line SecondGoalParallelLine)
        {
            Circle cGoali = new Circle(GoaliPos, RobotParameters.OurRobotParams.Diameter / 2);

            Vector2D FirstGoalvec = fb.FirstGoalLine.Tail - fb.FirstGoalLine.Head;
            Vector2D GoaliDefendTargetVec = cGoali.Center - fb.TargetPos;
            double angle = Math.Abs(Vector2D.AngleBetweenInRadians(FirstGoalvec, GoaliDefendTargetVec));

            Vector2D tmpVec;
            if (Mode == GoaliPositioningMode.InRightSide)
                tmpVec = Vector2D.FromAngleSize(GoaliDefendTargetVec.AngleInRadians + angle, 1);
            else
                tmpVec = Vector2D.FromAngleSize(GoaliDefendTargetVec.AngleInRadians - angle, 1);

            DefenderBoundLine1 = new Line(fb.TargetPos, fb.TargetPos + tmpVec);
            Vector2D DefenderBoundVec1 = DefenderBoundLine1.Head - DefenderBoundLine1.Tail;
            Vector2D tmpV = Vector2D.FromAngleSize(DefenderBoundVec1.AngleInRadians + fb.prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            Line tmpL = new Line(DefenderBoundLine1.Head + tmpV, (DefenderBoundLine1.Head + tmpV) - DefenderBoundVec1);
            Vector2D SecondGoalVec = fb.SecondGoalLine.Head - fb.SecondGoalLine.Tail;
            Vector2D tmpV2 = Vector2D.FromAngleSize(SecondGoalVec.AngleInRadians - fb.prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            SecondGoalParallelLine = new Line(fb.SecondGoalLine.Head + tmpV2, (fb.SecondGoalLine.Head + tmpV2) - SecondGoalVec);

            Position2D? DefenderPos = null;
            Line DefenderLine = new Line();
            Circle DefenderBound = new Circle(GameParameters.OurGoalCenter, fb.DefenderDist);
            if ((Mode == GoaliPositioningMode.InRightSide && (-DefenderBoundVec1).AngleInDegrees >= (-SecondGoalVec).AngleInDegrees) || (Mode == GoaliPositioningMode.InLeftSide && (-DefenderBoundVec1).AngleInDegrees <= (-SecondGoalVec).AngleInDegrees))
            {
                List<Position2D> tmpInts = DefenderBound.Intersect(fb.SecondGoalLine);
                if (tmpInts.Count > 1)
                {
                    if (Math.Sign(tmpInts[0].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(tmpInts[1].Y) == Math.Sign(fb.TargetPos.Y))
                        DefenderPos = (tmpInts[0].X < tmpInts[1].X) ? tmpInts[0] : tmpInts[1];
                    else if (Math.Sign(tmpInts[0].Y) == Math.Sign(fb.TargetPos.Y))
                        DefenderPos = tmpInts[0];
                    else if (Math.Sign(tmpInts[1].Y) == Math.Sign(fb.TargetPos.Y))
                        DefenderPos = tmpInts[1];
                    else
                        DefenderPos = (tmpInts[0].X < tmpInts[1].X) ? tmpInts[0] : tmpInts[1];
                }
                else if (tmpInts.Count == 1)
                    DefenderPos = tmpInts[0];
            }
            else
                DefenderPos = tmpL.IntersectWithLine(SecondGoalParallelLine);
            return DefenderPos;
        }
        private static List<Position2D> ReCalculatePositions(WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode, Position2D GoaliPos, FirstBounds fb, Position2D? DefenderPos, Line DefenderBoundLine1, bool isOnGoalLine, Line DefendTargetGoalLine, Line SecondGoalParallelLine)
        {
            Line DefenderLine = new Line();
            Circle DefenderBound = new Circle(GameParameters.OurGoalCenter, fb.DefenderDist);
            Circle GoaliBound = new Circle(GameParameters.OurGoalCenter, fb.GoaliDist);

            if (DefenderPos.HasValue)
            {
                if (DefenderPos.Value.DistanceFrom(fb.TargetPos) > 0.05)
                {
                    DefenderLine = new Line(fb.TargetPos, DefenderPos.Value);
                    if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) > fb.maxdefX && DefenderPos.Value.X < GameParameters.OurGoalCenter.X)
                    {
                        Circle DefenderBound2 = new Circle(GameParameters.OurGoalCenter, fb.maxdefX);
                        List<Position2D> possd = DefenderBound2.Intersect(DefenderLine);
                        Position2D tmpDefenderPos = new Position2D();
                        if (possd.Count > 1)
                        {
                            if (Math.Sign(possd[0].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(possd[1].Y) != Math.Sign(fb.TargetPos.Y))
                                tmpDefenderPos = possd[0];
                            else if (Math.Sign(possd[1].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(possd[0].Y) != Math.Sign(fb.TargetPos.Y))
                                tmpDefenderPos = possd[1];
                            else
                                tmpDefenderPos = (possd[0].X < possd[1].X) ? possd[0] : possd[1];
                        }
                        else if (possd.Count == 1)
                            tmpDefenderPos = possd[0];
                        Circle DefenderCircle = new Circle(tmpDefenderPos, RobotParameters.OurRobotParams.Diameter / 2);
                        List<Line> tngDefL;
                        List<Position2D> tngDefP;
                        int tangCount = DefenderCircle.GetTangent(fb.TargetPos, out tngDefL, out tngDefP);
                        double tngAng;
                        double vAng;
                        if (tangCount < 2)
                            return new List<Position2D>() { GoaliPos, tmpDefenderPos };
                        tngAng = Math.Abs(Vector2D.AngleBetweenInRadians(tngDefP[0] - fb.TargetPos, tngDefP[1] - fb.TargetPos));
                        vAng = Math.Abs(Vector2D.AngleBetweenInRadians(fb.SecondGoalLine.Tail - fb.SecondGoalLine.Head, DefenderBoundLine1.Tail - DefenderBoundLine1.Head));
                        double errAng = (vAng - tngAng) / 3;
                        Vector2D vec;
                        if (Mode == GoaliPositioningMode.InRightSide)
                            vec = Vector2D.FromAngleSize((tmpDefenderPos - fb.TargetPos).AngleInRadians + errAng / 2, 1);
                        else
                            vec = Vector2D.FromAngleSize((tmpDefenderPos - fb.TargetPos).AngleInRadians - errAng / 2, 1);

                        Line tmpLine = new Line(fb.TargetPos, fb.TargetPos + vec);
                        List<Position2D> ppp = DefenderBound2.Intersect(tmpLine);
                        if (ppp.Count > 1)
                        {
                            if (Math.Sign(ppp[0].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(ppp[1].Y) != Math.Sign(fb.TargetPos.Y))
                                DefenderPos = ppp[0];
                            else if (Math.Sign(ppp[1].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(ppp[0].Y) != Math.Sign(fb.TargetPos.Y))
                                DefenderPos = ppp[1];
                            else
                                DefenderPos = (ppp[0].X < ppp[1].X) ? ppp[0] : ppp[1];
                        }
                        else if (ppp.Count == 1)
                            DefenderPos = ppp[0];
                        Vector2D vec2;
                        if (Mode == GoaliPositioningMode.InRightSide)
                            vec2 = Vector2D.FromAngleSize((GoaliPos - fb.TargetPos).AngleInRadians + errAng, 1);
                        else
                            vec2 = Vector2D.FromAngleSize((GoaliPos - fb.TargetPos).AngleInRadians - errAng, 1);
                        Line NewGoaliLine = new Line(fb.TargetPos, fb.TargetPos + vec2);

                        if (isOnGoalLine)
                        {
                            Line tmpLine2 = new Line(new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, -1), new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, 1));
                            Position2D? tmpp = tmpLine2.IntersectWithLine(NewGoaliLine);
                            GoaliPos = tmpp.Value;
                        }
                        else
                        {
                            List<Position2D> ppp2 = GoaliBound.Intersect(NewGoaliLine);
                            if (ppp2.Count > 1)
                                GoaliPos = (ppp2[0].X < ppp2[1].X) ? ppp2[0] : ppp2[1];
                            else if (ppp2.Count == 1)
                                GoaliPos = ppp2[0];
                        }
                    }
                    else if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) < fb.mindefX || (DefenderPos.Value.X > GameParameters.OurGoalCenter.X))
                    {

                        List<Position2D> possd = DefenderBound.Intersect(DefenderLine);
                        Position2D pdef = new Position2D();
                        if (possd.Count > 1)
                        {
                            //if (Math.Sign(possd[0].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[1].Y) != Math.Sign(TargetPos.Y))
                            //    pdef = possd[0];
                            //else if (Math.Sign(possd[1].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[0].Y) != Math.Sign(TargetPos.Y))
                            //    pdef = possd[1];
                            //else
                            //if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) < mindefX)
                            //    pdef = DefenderPos.Value;
                            //if(DefenderPos.Value.X > GameParameters.OurGoalCenter.X)
                            pdef = (possd[0].X < possd[1].X) ? possd[0] : possd[1];

                        }
                        //pdef = (possd[0].X < possd[1].X) ? possd[0] : possd[1];
                        else if (possd.Count == 1)
                            pdef = possd[0];
                        DefenderPos = pdef;
                    }
                }
                List<Position2D> tangposes;
                List<Line> tanglines;
                new Circle(DefenderPos.Value, RobotParameters.OurRobotParams.Diameter / 2).GetTangent(fb.TargetPos, out tanglines, out tangposes);
                if (tangposes.Count > 1)
                {
                    Position2D selectedtng;
                    Position2D Otng;
                    Line selectedtngLine, OtngLine;
                    if (tangposes[0].Y >= tangposes[0].Y)
                    {
                        selectedtng = tangposes[0];
                        Otng = tangposes[1];
                        selectedtngLine = tanglines[0];
                        OtngLine = tanglines[1];
                    }
                    else
                    {
                        selectedtng = tangposes[1];
                        Otng = tangposes[0];
                        selectedtngLine = tanglines[1];
                        OtngLine = tanglines[0];
                    }

                    if (((selectedtng - fb.TargetPos).AngleInDegrees >= (GameParameters.OurGoalLeft - fb.TargetPos).AngleInDegrees) && ((Otng - fb.TargetPos).AngleInDegrees <= (GameParameters.OurGoalRight - fb.TargetPos).AngleInDegrees))
                    {
                        //Line ll = new Line(fb.TargetPos, GoaliPos);
                        //Line goal = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                        //Position2D? intersect = ll.IntersectWithLine(goal);
                        //if (intersect.HasValue)
                        //{
                        //    GoaliPos = GoaliPos + (GoaliPos - fb.TargetPos).GetNormalizeToCopy(0.4);
                        //    if (GoaliPos.X > GameParameters.OurGoalCenter.X - 0.2)
                        //    {
                        //        GoaliPos = intersect.Value + (fb.TargetPos - intersect.Value).GetNormalizeToCopy(0.2);
                        //    }
                        //}
                        //if (Math.Abs(GoaliPos.Y) > 0.25)
                        //    GoaliPos = (GameParameters.OurGoalCenter + new Vector2D(0, -0.2)) + (fb.TargetPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.15);

                        Vector2D pvec = selectedtng - fb.FirstGoalCorner;
                        GoaliPos = fb.FirstGoalCorner + pvec.GetNormalizeToCopy(0.4);

                    }
                }
                if (fb.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) < DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter))
                {
                    Position2D? tp = DefendTargetGoalLine.IntersectWithLine(SecondGoalParallelLine);
                    if (tp.HasValue)
                    {
                        if (tp.Value.X > GameParameters.OurGoalCenter.X - fb.minGoalidx)
                        {
                            Vector2D tmpGv = GameParameters.OurGoalCenter - fb.TargetPos;
                            tp = fb.TargetPos + tmpGv.GetNormalizeToCopy(0.25);
                        }
                        GoaliPos = tp.Value;
                    }
                }
                return new List<Position2D>() { GoaliPos, DefenderPos.Value };
            }
            return new List<Position2D>() { GoaliPos, new Position2D() };
        }
        public static Position2D? CalculatePos(GameStrategyEngine engine, WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode, out Position2D? GoaliPos)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Line DefenderBoundLine1, DefendTargetGoalLine, SecondGoalParallelLine;
            if (TargetState != null)
            {
                bool isOnGoalLine;
                FirstBounds fb = CalculateBounds(Model, TargetState, Mode);
                Position2D tmpGoaliPos = CalculateGoaliPos(Model, TargetState, fb, out isOnGoalLine, out DefendTargetGoalLine);
                Position2D? tmpDefenderPos = CalculateDefenderPos(Model, TargetState, Mode, tmpGoaliPos, fb, out DefenderBoundLine1, out SecondGoalParallelLine);
                List<Position2D> Positions = ReCalculatePositions(Model, TargetState, Mode, tmpGoaliPos, fb, tmpDefenderPos, DefenderBoundLine1, isOnGoalLine, DefendTargetGoalLine, SecondGoalParallelLine);

                Positions[1] = CommonDefenceUtils.CheckForStopZone(BallIsMoved, Positions[1], Model);
                if (Positions[0].X > GameParameters.OurGoalCenter.X - fb.minGoalidx)
                    Positions[0] = new Position2D(GameParameters.OurGoalCenter.X - fb.minGoalidx, Positions[0].Y);
                GoaliPos = Positions[0];
                return Positions[1];
            }
            GoaliPos = null;
            return null;

        }
        #endregion

        #region 1st Defender
        private static DefenceInfo CalculateNormalFirst(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, out DefenceInfo GoaliRes, DefenderCommand GoaliCommand, List<DefenceInfo> CurrentInfo)
        {
            DefenceInfo def = new DefenceInfo();
            GoaliRes = new DefenceInfo();

            Position2D? goalieball;
            Position2D? goalierobot;
            Position2D? g;
            ballState = Model.BallState;
            GoaliPositioningMode BallMode = ChooseMode(ballState, CurrentGoalieMode);
            Position2D? Defendball = CalculatePos(engine, Model, ballState, BallMode, out goalieball);
            if (ballState.Speed.Size < 0.5 || !Command.OppID.HasValue || !Model.Opponents.ContainsKey(Command.OppID.Value))
            {
                GoaliRes.OppID = (Command.OppID.HasValue && Model.Opponents.ContainsKey(Command.OppID.Value)) ? Command.OppID : null;
                if ((Command.OppID.HasValue && Model.Opponents.ContainsKey(Command.OppID.Value)))
                    GoaliRes.OppID = Command.OppID;
                else
                    GoaliRes.OppID = null;
                GoaliRes.RoleType = GoaliCommand.RoleType;
                GoaliRes.TargetState = ballState;
                GoaliRes.Mode = BallMode;
                GoaliRes.DefenderPosition = goalieball;

                if ((Command.OppID.HasValue && Model.Opponents.ContainsKey(Command.OppID.Value)))
                    def.OppID = Command.OppID;
                else
                    def.OppID = null;
                def.RoleType = Command.RoleType;
                def.TargetState = ballState;
                def.Mode = BallMode;
                def.DefenderPosition = Defendball;
                DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "6854654645");
            }
            else
            {
                SingleObjectState oppstate = Model.Opponents[Command.OppID.Value];
                GoaliPositioningMode RobotMode = ChooseMode(oppstate, CurrentGoalieMode);
                Position2D? Defendrobot = CalculatePos(engine, Model, oppstate, RobotMode, out goalierobot);

                Position2D temprobot = oppstate.Location + oppstate.Speed * 0.5;
                temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
                temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
                temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
                temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);

                SingleObjectState nextrobot = new SingleObjectState()
                {
                    Type = ObjectType.Opponent,
                    Location = temprobot,
                    Speed = oppstate.Speed
                };
                GoaliPositioningMode nextRobotMode = ChooseMode(nextrobot, CurrentGoalieMode);
                Position2D? Defendrobotnext = CalculatePos(engine, Model, nextrobot, nextRobotMode, out g);

                Position2D tempball = ballState.Location + ballState.Speed * 0.5;
                tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
                tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
                tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
                tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);
                SingleObjectState nextball = new SingleObjectState()
                {
                    Type = ObjectType.Ball,
                    Location = tempball,
                    Speed = ballState.Speed
                };
                GoaliPositioningMode nextBallMode = ChooseMode(nextball, CurrentGoalieMode);
                Position2D? Defendballnext = CalculatePos(engine, Model, nextball, nextBallMode, out g);

                if (Defendrobot.HasValue && Defendball.HasValue)
                {

                    double db = Defendball.Value.DistanceFrom(Defendballnext.Value);
                    double dr = Defendrobot.Value.DistanceFrom(Defendrobotnext.Value);
                    if (db > dr + 0.065)
                    {
                        GoaliRes.OppID = Command.OppID;
                        GoaliRes.RoleType = GoaliCommand.RoleType;
                        GoaliRes.TargetState = oppstate;
                        GoaliRes.Mode = RobotMode;
                        GoaliRes.DefenderPosition = goalierobot;

                        def.OppID = Command.OppID;
                        def.RoleType = Command.RoleType;
                        def.TargetState = oppstate;
                        def.Mode = RobotMode;
                        def.DefenderPosition = Defendrobot;
                        DrawingObjects.AddObject(new StringDraw("RobotMode", new Position2D(-1, -1)), "564651321");
                        DrawingObjects.AddObject(new StringDraw("OppID: " + Command.OppID.Value.ToString(), new Position2D(-.8, -1)), "9873164");
                    }
                    else if (dr > db + 0.065)
                    {
                        GoaliRes.OppID = Command.OppID;
                        GoaliRes.RoleType = GoaliCommand.RoleType;
                        GoaliRes.TargetState = ballState;
                        GoaliRes.Mode = BallMode;
                        GoaliRes.DefenderPosition = goalieball;

                        def.OppID = Command.OppID;
                        def.RoleType = Command.RoleType;
                        def.TargetState = ballState;
                        def.Mode = BallMode;
                        def.DefenderPosition = Defendball;
                        DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "5464465");
                    }
                    else
                    {
                        if (CurrentInfo != null && CurrentInfo.Any(a => a.RoleType == Command.RoleType))
                        {
                            DefenceInfo dinfo = CurrentInfo.Single(s => s.RoleType == Command.RoleType);

                            if (dinfo.TargetState.Type == ObjectType.Ball)
                            {
                                GoaliRes.OppID = Command.OppID;
                                GoaliRes.RoleType = GoaliCommand.RoleType;
                                GoaliRes.TargetState = ballState;
                                GoaliRes.Mode = BallMode;
                                GoaliRes.DefenderPosition = goalieball;

                                def.OppID = Command.OppID;
                                def.RoleType = Command.RoleType;
                                def.TargetState = ballState;
                                def.Mode = BallMode;
                                def.DefenderPosition = Defendball;
                                DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "546465465");
                            }
                            else
                            {
                                GoaliRes.OppID = Command.OppID;
                                GoaliRes.RoleType = GoaliCommand.RoleType;
                                GoaliRes.TargetState = oppstate;
                                GoaliRes.Mode = RobotMode;
                                GoaliRes.DefenderPosition = goalierobot;

                                def.OppID = Command.OppID;
                                def.RoleType = Command.RoleType;
                                def.TargetState = oppstate;
                                def.Mode = RobotMode;
                                def.DefenderPosition = Defendrobot;
                                DrawingObjects.AddObject(new StringDraw("RobotMode", new Position2D(-1, -1)), "8765456498");
                                DrawingObjects.AddObject(new StringDraw("OppID: " + Command.OppID.Value.ToString(), new Position2D(-.8, -1)), "5165464");
                            }
                        }
                        else
                        {
                            GoaliRes.OppID = Command.OppID;
                            GoaliRes.RoleType = GoaliCommand.RoleType;
                            GoaliRes.TargetState = ballState;
                            GoaliRes.Mode = BallMode;
                            GoaliRes.DefenderPosition = goalieball;

                            def.OppID = Command.OppID;
                            def.RoleType = Command.RoleType;
                            def.TargetState = ballState;
                            def.Mode = BallMode;
                            def.DefenderPosition = Defendball;
                            DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "8756123645");
                        }
                    }
                }
                else if (Defendrobot.HasValue)
                {
                    GoaliRes.OppID = Command.OppID;
                    GoaliRes.RoleType = GoaliCommand.RoleType;
                    GoaliRes.TargetState = oppstate;
                    GoaliRes.Mode = RobotMode;
                    GoaliRes.DefenderPosition = goalierobot;

                    def.OppID = Command.OppID;
                    def.RoleType = Command.RoleType;
                    def.TargetState = oppstate;
                    def.Mode = RobotMode;
                    def.DefenderPosition = Defendrobot;
                    DrawingObjects.AddObject(new StringDraw("RobotMode", new Position2D(-1, -1)), "74654875");
                    DrawingObjects.AddObject(new StringDraw("OppID: " + Command.OppID.Value.ToString(), new Position2D(-.8, -1)), "8797546");
                }
                else
                {
                    GoaliRes.OppID = Command.OppID;
                    GoaliRes.RoleType = GoaliCommand.RoleType;
                    GoaliRes.TargetState = ballState;
                    GoaliRes.Mode = BallMode;
                    GoaliRes.DefenderPosition = goalieball;

                    def.OppID = Command.OppID;
                    def.RoleType = Command.RoleType;
                    def.TargetState = ballState;
                    def.Mode = BallMode;
                    def.DefenderPosition = Defendball;
                    DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "4956678");
                }
            }
            CurrentGoalieMode = GoaliRes.Mode;
            GoaliRes.Teta = (GoaliRes.TargetState.Location - GoaliRes.DefenderPosition.Value).AngleInDegrees;
            return def;
        }
        #endregion

        #region 2nd defender
        private static DefenceInfo CalculateNormalSecond(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, out DefenceInfo GoaliRes, List<DefenceInfo> CurrentInfo)
        {
            DefenceInfo def = new DefenceInfo();
            GoaliRes = new DefenceInfo();

            Position2D? goalieball;
            Position2D? goalierobot;
            Position2D? g;
            ballState = Model.BallState;
            GoaliPositioningMode BallMode = ChooseMode(ballState, CurrentGoalieMode);
            Position2D? Defendball = CalculatePos(engine, Model, ballState, BallMode, out goalieball);
            if (!Command.OppID.HasValue || !Model.Opponents.ContainsKey(Command.OppID.Value))
            {
                GoaliRes.OppID = null;
                GoaliRes.TargetState = ballState;
                GoaliRes.Mode = BallMode;
                GoaliRes.DefenderPosition = goalieball;

                def.OppID = null;
                def.RoleType = Command.RoleType;
                def.TargetState = ballState;
                def.Mode = BallMode;
                def.DefenderPosition = Defendball;
            }
            else
            {
                SingleObjectState oppstate = Model.Opponents[Command.OppID.Value];
                GoaliPositioningMode RobotMode = ChooseMode(oppstate, CurrentGoalieMode);
                Position2D? Defendrobot = CalculatePos(engine, Model, oppstate, RobotMode, out goalierobot);

                GoaliRes.OppID = Command.OppID.Value;
                GoaliRes.TargetState = oppstate;
                GoaliRes.Mode = RobotMode;
                GoaliRes.DefenderPosition = goalierobot;

                def.OppID = Command.OppID.Value;
                def.RoleType = Command.RoleType;
                def.TargetState = oppstate;
                def.Mode = RobotMode;
                def.DefenderPosition = Defendrobot;
            }

            return def;
        }
        #endregion

        #region Marker
        private static DefenceInfo Mark(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, List<DefenceInfo> CurrentInfo)
        {

            DefenceInfo def = new DefenceInfo();
            int? oppid = Command.OppID;
            Position2D? target;
            def.RoleType = Command.RoleType;
            def.OppID = oppid;

            Position2D Target = Position2D.Zero;
            SingleObjectState state;
            if (oppid.HasValue && Model.Opponents.ContainsKey(oppid.Value))
                state = Model.Opponents[oppid.Value];
            else
                state = ballState;

            Vector2D oppSpeedVector = state.Speed;
            Vector2D oppOurGoalCenter = GameParameters.OurGoalCenter - state.Location;
            double innerpOppOurGoal = oppSpeedVector.InnerProduct(oppOurGoalCenter);



            double oppSpeed = state.Speed.Size;
            double minDist = (GameParameters.OurGoalCenter - state.Location).Size;// GameParameters.SafeRadi(state, MarkerDefenceUtils.MinDistMarkMargin);

            Position2D minimum = GameParameters.OurGoalCenter + (state.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(minDist);
            Position2D maximum = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.2);
            Position2D posToGo = Position2D.Zero;



            double MarkFromDist = MarkerDefenceUtils.MarkFromDist;

            posToGo = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(MarkFromDist);

            if (minimum.DistanceFrom(GameParameters.OurGoalCenter) > posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = minimum;
            else if (maximum.DistanceFrom(GameParameters.OurGoalCenter) < posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = maximum;
            else
                Target = posToGo;


            maximum.DrawColor = System.Drawing.Color.Blue;
            minimum.DrawColor = System.Drawing.Color.Yellow;
            DrawingObjects.AddObject(Target);
            DrawingObjects.AddObject(minimum);
            DrawingObjects.AddObject(maximum);
            if (Command.MarkMaximumDist > 1)
            {
                Position2D maxpos = GameParameters.OurGoalCenter + (Target - GameParameters.OurGoalCenter).GetNormalizeToCopy(Command.MarkMaximumDist);
                if (GameParameters.OurGoalCenter.DistanceFrom(Target) > GameParameters.OurGoalCenter.DistanceFrom(maxpos))
                    //Target = maxpos;
                    Target.DrawColor = System.Drawing.Color.Blue;
                DrawingObjects.AddObject(Target);
            }
            Target = CommonDefenceUtils.CheckForStopZone(BallIsMoved, Target, Model);

            if (Target.X > GameParameters.OurGoalCenter.X)
                Target.X = GameParameters.OurGoalCenter.X;
            if (Math.Abs(Target.Y) > Math.Abs(GameParameters.OurLeftCorner.Y))
                Target = new Line(Target, GameParameters.OurGoalCenter).CalculateX(Math.Abs(GameParameters.OurLeftCorner.Y) * Math.Sign(Target.Y));

            target = Target;

            def.TargetState = state;
            def.DefenderPosition = target;
            def.Teta = (state.Location - GameParameters.OurGoalCenter).AngleInDegrees;
            return def;
        }
        private static DefenceInfo MarkStatic(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, List<DefenceInfo> CurrentInfo)
        {

            DefenceInfo def = new DefenceInfo();
            int? oppid = Command.OppID;
            Position2D? target;
            def.RoleType = Command.RoleType;
            def.OppID = oppid;

            Position2D Target = Position2D.Zero;
            SingleObjectState state;
            if (oppid.HasValue && Model.Opponents.ContainsKey(oppid.Value))
                state = Model.Opponents[oppid.Value];
            else
                state = ballState;

            Vector2D oppSpeedVector = state.Speed;
            Vector2D oppOurGoalCenter = GameParameters.OurGoalCenter - state.Location;
            double innerpOppOurGoal = oppSpeedVector.InnerProduct(oppOurGoalCenter);



            double oppSpeed = state.Speed.Size;
            double minDist = GameParameters.SafeRadi(state, StaticMarkerDefenceUtils.MinDistMarkMargin);

            Position2D minimum = GameParameters.OurGoalCenter + (state.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(minDist);
            Position2D maximum = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.2);
            Position2D posToGo = Position2D.Zero;



            double MarkFromDist = StaticMarkerDefenceUtils.MarkFromDist;

            posToGo = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(MarkFromDist);

            if (minimum.DistanceFrom(GameParameters.OurGoalCenter) > posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = minimum;
            else if (maximum.DistanceFrom(GameParameters.OurGoalCenter) < posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = maximum;
            else
                Target = posToGo;


            maximum.DrawColor = System.Drawing.Color.Blue;
            minimum.DrawColor = System.Drawing.Color.Yellow;
            DrawingObjects.AddObject(Target);
            DrawingObjects.AddObject(minimum);
            DrawingObjects.AddObject(maximum);

            if (Target.X > GameParameters.OurGoalCenter.X)
                Target.X = GameParameters.OurGoalCenter.X;
            if (Math.Abs(Target.Y) > Math.Abs(GameParameters.OurLeftCorner.Y))
                Target = new Line(Target, GameParameters.OurGoalCenter).CalculateX(Math.Abs(GameParameters.OurLeftCorner.Y) * Math.Sign(Target.Y));

            target = Target;

            def.TargetState = state;
            def.DefenderPosition = target;
            def.Teta = (state.Location - target.Value).AngleInDegrees;
            return def;
        }

        #endregion

        #region RegionalDefence
        private static DefenceInfo ST = new DefenceInfo();
        private static DefenceInfo RegionalDefence(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, List<DefenceInfo> DeterminedDefenders, List<DefenceInfo> CurrentInfo)
        {
            bool Reg2 = false;
            if (Command.RoleType == typeof(RegionalDefenderRole2))
                Reg2 = true;
            DefenceInfo def = new DefenceInfo();
            if (Command.RoleType == typeof(RegionalDefenderRole))
            {
                List<Position2D?> Pos2Exclude = DeterminedDefenders.Where(y => y.DefenderPosition.Value.DistanceFrom(GameParameters.OurGoalCenter) < 1.6).Select(s => s.DefenderPosition).ToList();
                DrawingObjects.AddObject(new Circle(GameParameters.OurGoalCenter, 1.5, new Pen(Color.Red, .01f)));
                List<Vector2D> Vectors = new List<Vector2D>();


                Position2D? target = null;
                //target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                Pos2Exclude.ForEach(u => Vectors.Add(u.Value - GameParameters.OurGoalCenter));
                Vectors.Add(GameParameters.OurRightCorner - GameParameters.OurGoalCenter);
                Vectors.Add(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter);
                //Vectors.OrderByDescending(i => Math.Abs(Vector2D.AngleBetweenInDegrees(i, GameParameters.OurLeftCorner - GameParameters.OurGoalCenter)));
                //double anglebetween = Vector2D.AngleBetweenInDegrees(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter, Vectors[0]);
                //List<Vector2D> vec2 = new List<Vector2D>();
                List<double> Beta = new List<double>();
                foreach (var item in Vectors)
                {
                    Beta.Add((item.AngleInDegrees > 0) ? item.AngleInDegrees - 90 : 270 + item.AngleInDegrees);
                }
                List<double> SortedBeta = Beta.OrderBy(o => o).ToList();
                List<double> DifBEta = new List<double>();
                double max = double.MinValue;
                int maxindex = 0;
                for (int i = 0; i < SortedBeta.Count - 1; i++)
                {
                    if (Math.Sign((GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2) * Math.PI / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2 * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2))).Y) != ballState.Location.Y)
                    {
                        if ((SortedBeta[i + 1] - SortedBeta[i]) * 1.3 > max)
                        {
                            max = SortedBeta[i + 1] - SortedBeta[i];
                            maxindex = i;
                        }
                    }
                    else if (Math.Sign((GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2) * Math.PI / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2 * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2))).Y) == ballState.Location.Y)
                    {
                        if (SortedBeta[i + 1] - SortedBeta[i] > max)
                        {
                            max = SortedBeta[i + 1] - SortedBeta[i];
                            maxindex = i;
                        }
                    }
                }
                double targetangle = 0;
                double targetbeta = (SortedBeta[maxindex] + SortedBeta[maxindex + 1]) / 2;
                if (targetbeta > 0 && targetbeta < 90)
                {
                    targetangle = targetbeta + 90;
                }
                else
                {
                    targetangle = targetbeta - 270;
                }
                double MaxAngle = double.MinValue;
                foreach (var item in Vectors)
                {
                    DrawingObjects.AddObject(new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + item, new Pen(Color.Black, .01f)), item.X.ToString());
                }
                if (Math.Abs(targetangle) > 90 && CurrentInfo.Any(t => t.RoleType == typeof(RegionalDefenderRole2)))
                {
                    targetbeta = SortedBeta[maxindex] + (((SortedBeta[maxindex + 1] - SortedBeta[maxindex])) / 3);
                    if (targetbeta > 0 && targetbeta < 90)
                    {
                        targetangle = targetbeta + 90;
                    }
                    else
                    {
                        targetangle = targetbeta - 270;
                    }
                    Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2));
                    target = GameParameters.OurGoalCenter + FinalVector;
                }
                else
                {
                    Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2));
                    target = GameParameters.OurGoalCenter + FinalVector;
                }
                //double maxGoodNess = double.MinValue;
                //Position2D? target = null;
                //Position2D tar = new Position2D(GameParameters.OurGoalCenter.X - 1.1, 0);

                //foreach (var item in Command.RegionalDefendPoints)
                //{

                //    double goodness;
                //    double total = 0;
                //    Pos2Exclude.ForEach(p => total += item.DistanceFrom(p.Value));
                //    target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                //    goodness += total / 50;
                //    if (goodness > maxGoodNess)
                //    {
                //        maxGoodNess = goodness;
                //        if (target.HasValue)
                //        {
                //            Vector2D vec = item - target.Value;
                //            tar = target.Value + vec.GetNormalizeToCopy(1.1 + Command.RegionalDistFromDangerZone);
                //        }
                //        else
                //        {
                //            Vector2D vec = item - GameParameters.OurGoalCenter;
                //            double r = GameParameters.SafeRadi(new SingleObjectState(item, new Vector2D(), 0), Command.RegionalDistFromDangerZone);
                //            tar = GameParameters.OurGoalCenter + vec.GetNormalizeToCopy(r);
                //        }
                //    }
                //}
                ST = def;
                def.OppID = null;
                def.RoleType = Command.RoleType;

                def.DefenderPosition = target;
                if (target.HasValue && FreekickDefence.RearRegional)
                {
                    def.DefenderPosition = GameParameters.OurGoalCenter + (target.Value - GameParameters.OurGoalCenter).GetNormalizeToCopy(4);
                    Position2D targete = def.DefenderPosition.Value;
                    if (Math.Abs(targete.Y) > 1)
                    {
                        def.DefenderPosition = new Position2D(Math.Min(Math.Max(targete.X, .5), 1), Math.Sign(targete.Y));
                    }
                    //def.DefenderPosition = GameParameters.OurGoalCenter+ (targete - GameParameters.OurGoalCenter).GetNormalizeToCopy(2);
                }
                def.TargetState = ballState;

                return def;
            }
            else
            {
                List<Position2D?> Pos2Exclude = DeterminedDefenders.Where(y => y.DefenderPosition.Value.DistanceFrom(GameParameters.OurGoalCenter) < 1.6).Select(s => s.DefenderPosition).ToList();
                DrawingObjects.AddObject(new Circle(GameParameters.OurGoalCenter, 1.5, new Pen(Color.Red, .01f)));
                List<Vector2D> Vectors = new List<Vector2D>();


                Position2D? target = null;
                //target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                Pos2Exclude.ForEach(u => Vectors.Add(u.Value - GameParameters.OurGoalCenter));
                Vectors.Add(GameParameters.OurRightCorner - GameParameters.OurGoalCenter);
                Vectors.Add(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter);
                //Vectors.OrderByDescending(i => Math.Abs(Vector2D.AngleBetweenInDegrees(i, GameParameters.OurLeftCorner - GameParameters.OurGoalCenter)));
                //double anglebetween = Vector2D.AngleBetweenInDegrees(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter, Vectors[0]);
                //List<Vector2D> vec2 = new List<Vector2D>();
                List<double> Beta = new List<double>();
                foreach (var item in Vectors)
                {
                    Beta.Add((item.AngleInDegrees > 0) ? item.AngleInDegrees - 90 : 270 + item.AngleInDegrees);
                }
                List<double> SortedBeta = Beta.OrderBy(o => o).ToList();
                List<double> DifBEta = new List<double>();
                double max = double.MinValue;
                int maxindex = 0;
                for (int i = 0; i < SortedBeta.Count - 1; i++)
                {
                    if (SortedBeta[i + 1] - SortedBeta[i] > max)
                    {
                        max = SortedBeta[i + 1] - SortedBeta[i];
                        maxindex = i;

                    }
                }
                double targetangle = 0;
                double targetbeta = (SortedBeta[maxindex] + SortedBeta[maxindex + 1]) / 2;
                if (targetbeta > 0 && targetbeta < 90)
                {
                    targetangle = targetbeta + 90;
                }
                else
                {
                    targetangle = targetbeta - 270;
                }
                double MaxAngle = double.MinValue;
                foreach (var item in Vectors)
                {
                    DrawingObjects.AddObject(new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + item, new Pen(Color.Black, .01f)), item.X.ToString());
                }

                Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2));
                target = GameParameters.OurGoalCenter + FinalVector;

                //double maxGoodNess = double.MinValue;
                //Position2D? target = null;
                //Position2D tar = new Position2D(GameParameters.OurGoalCenter.X - 1.1, 0);

                //foreach (var item in Command.RegionalDefendPoints)
                //{

                //    double goodness;
                //    double total = 0;
                //    Pos2Exclude.ForEach(p => total += item.DistanceFrom(p.Value));
                //    target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                //    goodness += total / 50;
                //    if (goodness > maxGoodNess)
                //    {
                //        maxGoodNess = goodness;
                //        if (target.HasValue)
                //        {
                //            Vector2D vec = item - target.Value;
                //            tar = target.Value + vec.GetNormalizeToCopy(1.1 + Command.RegionalDistFromDangerZone);
                //        }
                //        else
                //        {
                //            Vector2D vec = item - GameParameters.OurGoalCenter;
                //            double r = GameParameters.SafeRadi(new SingleObjectState(item, new Vector2D(), 0), Command.RegionalDistFromDangerZone);
                //            tar = GameParameters.OurGoalCenter + vec.GetNormalizeToCopy(r);
                //        }
                //    }
                //}
                def.OppID = null;
                def.RoleType = Command.RoleType;
                def.DefenderPosition = target;
                def.TargetState = ballState;

                return def;
            }
        }
        #endregion

        #region Static_Defenders
        private static bool isMarker = false;

        private static Position2D? secondLastPos = null;
        public static List<DefenceInfo> MatchStatic(GameStrategyEngine engine, WorldModel Model, List<DefenderCommand> Commands)
        {
            CurrentlyAddedDefenders = Commands;
            List<DefenceInfo> res = new List<DefenceInfo>();
            DefenceInfo TempGoali = new DefenceInfo();
            DefenderCommand goaliCommand = Commands.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IGoalie))).FirstOrDefault();

            foreach (var item in Commands)
            {
                if (item.RoleType == typeof(StaticDefender1))
                {
                    DefenceInfo def;
                    int? rb = StaticRB(engine, Model, CurrentInfos);
                    SingleObjectState target = (rb.HasValue) ? Model.Opponents[rb.Value] : new SingleObjectState(ballState.Location + (ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size)), ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size), 0f);

                    target = new SingleObjectState(target.Location, target.Speed, target.Angle);
                    def = CalculateFirstStatic(Model, target, rb);

                    //DrawingObjects.AddObject(new Circle(def.DefenderPosition.Value, 0.2, new System.Drawing.Pen((rb.HasValue) ? System.Drawing.Color.Orange : System.Drawing.Color.Blue, 0.01f)));
                    res.Add(def);
                }
                else if (item.RoleType == typeof(StaticDefender2))
                {
                    DefenceInfo def;
                    int? rb = StaticRB(engine, Model, CurrentInfos);
                    SingleObjectState target = (rb.HasValue) ? Model.Opponents[rb.Value] : new SingleObjectState(ballState.Location + (ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size)), ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size), 0f);


                    def = CalculateSecondStatic(Model, target, rb);
                    //DrawingObjects.AddObject(new Circle(def.DefenderPosition.Value, 0.2, new System.Drawing.Pen((rb.HasValue) ? System.Drawing.Color.Orange : System.Drawing.Color.Blue, 0.01f)));
                    res.Add(def);
                }
                else if (item.RoleType == typeof(MarkerRoleStatic))
                {
                    DefenceInfo def = MarkStatic(engine, Model, item, CurrentInfos);
                    res.Add(def);
                }
                else if (item.RoleType == typeof(CenterBackNormalRole))
                {
                    DefenceInfo def;
                    int? rb = StaticRB(engine, Model, CurrentInfos);
                    SingleObjectState target = (rb.HasValue) ? Model.Opponents[rb.Value] : new SingleObjectState(ballState.Location + (ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size)), ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size), 0f);

                    target = new SingleObjectState(target.Location, target.Speed, target.Angle);
                    def = CalculateFirstStatic(Model, target, rb);

                    //DrawingObjects.AddObject(new Circle(def.DefenderPosition.Value, 0.2, new System.Drawing.Pen((rb.HasValue) ? System.Drawing.Color.Orange : System.Drawing.Color.Blue, 0.01f)));
                    res.Add(def);
                }
                else if (item.RoleType == typeof(LeftBackMarkerNormalRole))
                {
                    DefenceInfo def = MarkStatic(engine, Model, item, CurrentInfos);
                    res.Add(def);
                }
                else if (item.RoleType == typeof(RightBackMarkerNormalRole))
                {
                    DefenceInfo def = MarkStatic(engine, Model, item, CurrentInfos);
                    res.Add(def);
                }
            }
            List<DefenceInfo> temp = res.ToList();
            res = OverLapSolvingStatic(Model, res);
            CurrentInfos = res;
            CurrentStates = new Dictionary<RoleBase, int>();
            return res;
        }
        public static List<DefenceInfo> OverLapSolvingStatic(WorldModel Model, List<DefenceInfo> infos)
        {
            List<DefenceInfo> res = new List<DefenceInfo>();
            List<Forbiden> forbidens = new List<Forbiden>();

            DefenceInfo staticCenterBack = infos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).FirstOrDefault();
            DefenceInfo staticFirst = infos.Where(w => w.RoleType == typeof(StaticDefender1)).FirstOrDefault();
            DefenceInfo staticSecond = infos.Where(w => w.RoleType == typeof(StaticDefender2)).FirstOrDefault();

            List<DefenceInfo> staticCenterBackList = infos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).ToList();
            List<DefenceInfo> staticSeceonds = infos.Where(w => w.RoleType == typeof(StaticDefender2)).ToList();
            List<DefenceInfo> staticFirsts = infos.Where(w => w.RoleType == typeof(StaticDefender1)).ToList();
            List<DefenceInfo> marker = infos.Where(w => w.RoleType == typeof(MarkerRoleStatic)).ToList();

            double tresh = 0.0;
            double robotRadius = RobotParameters.OurRobotParams.Diameter / 2;
            if (StaticCenterState == CenterDefenderStates.Normal)
            {
                if (staticCenterBack != null)
                    forbidens.Add(new Forbiden()
                    {
                        center = staticCenterBack.DefenderPosition.Value,
                        radius = robotRadius
                    });
                UpdateForbidens(Model, ref forbidens, staticCenterBackList, tresh, robotRadius);
                if (staticCenterBack != null)
                    res.Add(staticCenterBack);
            }
            else
            {
                UpdateForbidens(Model, ref forbidens, staticCenterBackList, tresh, robotRadius);
                res.AddRange(staticCenterBackList);
            }

            if (StaticSecondState == DefenderStates.Normal)
            {
                if (staticFirst != null)
                    forbidens.Add(new Forbiden()
                    {
                        center = staticFirst.DefenderPosition.Value,
                        radius = robotRadius
                    });
                UpdateForbidens(Model, ref forbidens, staticSeceonds, tresh, robotRadius);
                if (staticFirst != null)
                    res.Add(staticFirst);
                res.AddRange(staticSeceonds);
            }
            else
            {
                if (staticSecond != null)
                    forbidens.Add(new Forbiden()
                    {
                        center = staticSecond.DefenderPosition.Value,
                        radius = robotRadius
                    });
                UpdateForbidens(Model, ref forbidens, staticFirsts, tresh, robotRadius);
                if (staticSecond != null)
                    res.Add(staticSecond);
                res.AddRange(staticFirsts);
            }
            UpdateForbidens(Model, ref forbidens, marker, tresh, robotRadius);
            res.AddRange(marker);
            res.ForEach(p =>
            {
                //double d1, d2;
                //if (!GameParameters.IsInDangerousZone(p.TargetState.Location, false, 0.2, out d1, out d2))
                //{
                //    p.Teta = (p.TargetState.Location - p.DefenderPosition.Value).AngleInDegrees;
                //}
                //else
                {
                    if (p.RoleType == typeof(CenterBackNormalRole))
                    {
                        Position2D t = GameParameters.OurGoalCenter - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5);
                        p.Teta = (p.DefenderPosition.Value - t).AngleInDegrees;
                    }
                    else if (p.RoleType == typeof(StaticDefender1))
                    {
                        Position2D t = GameParameters.OurGoalLeft - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5);
                        p.Teta = (p.DefenderPosition.Value - t).AngleInDegrees;
                    }
                    else if (p.RoleType == typeof(StaticDefender2))
                    {
                        Position2D t = GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5);
                        p.Teta = (p.DefenderPosition.Value - t).AngleInDegrees;
                    }
                }
            });
            return res;

        }
        private static DefenceInfo CalculateFirstStatic(WorldModel Model, SingleObjectState state, int? oppid)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Position2D DefencePos = new Position2D();
            double marg = .04 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .04));
            double radi = GameParameters.SafeRadi(state, marg);

            double d1, d2;

            Position2D ball = GameParameters.InFieldSize(ballState.Location);
            Position2D st = GameParameters.InFieldSize(state.Location);
            bool indangerzone = false;
            if (GameParameters.IsInDangerousZone(ball, false, 0.2, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                radi = GameParameters.SafeRadi(state, marg);
            }
            if (GameParameters.IsInDangerousZone(ball, false, -0.1, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                indangerzone = true;
            }
            //Position2D NextPos = new Position2D();
            double minDist = double.MaxValue;
            Circle C1 = new Circle(GameParameters.OurGoalCenter, radi);
            if (!indangerzone)
            {
                //Line L1 = new Line(st,GameParameters.OurGoalLeft - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
                Line L1 = new Line(st, GameParameters.OurGoalLeft - new Vector2D(0, GameParameters.OurGoalLeft.DistanceFrom(GameParameters.OurGoalCenter) / 2 - .05));

                List<Position2D> P = C1.Intersect(L1);

                for (int i = 0; i < P.Count; i++)
                {
                    if ((P[i].DistanceFrom(state.Location) < minDist))
                    {
                        minDist = P[i].DistanceFrom(st);
                        DefencePos = P[i];
                    }
                }
            }
            else
            {
                Vector2D v = st - GameParameters.OurGoalCenter;
                Position2D p = (GameParameters.OurGoalCenter + v.GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(st, Vector2D.Zero, 0), marg)));
                p = p + Vector2D.FromAngleSize(v.AngleInRadians - Math.PI / 2, 0.1);
                DefencePos = GameParameters.OurGoalCenter + (p - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(p, Vector2D.Zero, 0), marg));
            }
            //Line L2 = new Line(st, GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
            //List<Position2D> P2 = C1.Intersect(L2);

            //minDist = double.MaxValue;
            //for (int i = 0; i < P2.Count; i++)
            //{
            //    if (P2[i].DistanceFrom(st) < minDist)
            //    {
            //        minDist = P2[i].DistanceFrom(st);
            //        NextPos = P2[i];
            //    }
            //}
            if (ballState.Location.Y > 0)
            {
                Vector2D l1 = ball - GameParameters.OurGoalCenter;
                Vector2D l2 = GameParameters.OurLeftCorner - GameParameters.OurGoalCenter;
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13)
                {
                    Line tmpL = new Line(st, GameParameters.OurGoalCenter.Extend(0, 0.005));
                    List<Position2D> poses = C1.Intersect(tmpL);
                    minDist = double.MaxValue;
                    for (int i = 0; i < poses.Count; i++)
                    {
                        if ((poses[i].DistanceFrom(state.Location) < minDist))
                        {
                            DefencePos = poses[i];
                            minDist = poses[i].DistanceFrom(state.Location);
                        }
                    };
                }
            }
            else
            {
                Vector2D l1 = ball - GameParameters.OurGoalCenter;
                Vector2D l2 = GameParameters.OurRightCorner - GameParameters.OurGoalCenter;
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13 || ball.X > GameParameters.OurGoalCenter.X - 0.09)
                {
                    DefencePos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize(-1.79, radi);
                }
            }
            //}
            //else
            {
                //var dist = ball.DistanceFrom(st);

                //var stdefence = ball + Vector2D.FromAngleSize((st - ball).AngleInRadians, 4);


                //Line l = new Line(ball, stdefence);

                //List<Position2D> inters = C1.Intersect(l);
                //Position2D pos;
                //if (inters.Count > 0)
                //{
                //    pos = inters.First(f => f.DistanceFrom(st) <= inters.Min(m => m.DistanceFrom(st)));
                //    radi = GameParameters.SafeRadi(new SingleObjectState() { Location = pos }, 0.12);
                //    pos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize((pos - GameParameters.OurGoalCenter).AngleInRadians + 0.1, radi);

                //    DefencePos = pos;
                //}



                //  DrawingObjects.AddObject(l);
            }
            //if (DefencePos.DistanceFrom(NextPos) < RobotParameters.OurRobotParams.Diameter)
            //{
            //    DefencePos = new Position2D((NextPos.X + DefencePos.X) / 2.0, (NextPos.Y + DefencePos.Y) / 2.0);
            //}
            //if (DefencePos.X > GameParameters.OurGoalCenter.X - RobotParameters.OurRobotParams.Diameter)
            //{
            //    DefencePos.X = GameParameters.OurGoalCenter.X - RobotParameters.OurRobotParams.Diameter;
            //}
            //DefencePos.DrawColor = System.Drawing.Color.Red;
            //DrawingObjects.AddObject(DefencePos);
            if (DefencePos.X > GameParameters.OurGoalCenter.X - 0.09)
                DefencePos = new Position2D(GameParameters.OurGoalCenter.X - 0.09, DefencePos.Y);
            lastfirst = DefencePos;
            DefenceInfo di = new DefenceInfo();
            di.DefenderPosition = lastfirst;
            di.RoleType = typeof(CenterBackNormalRole);
            di.TargetState = state;
            di.OppID = oppid;
            return di;
        }
        private static DefenceInfo CalculateSecondStatic(WorldModel Model, SingleObjectState state, int? oppid)
        {

            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Position2D DefencePos = new Position2D();
            double marg = .04 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .04));
            double radi = GameParameters.SafeRadi(state, marg);

            double d1, d2;

            Position2D ball = GameParameters.InFieldSize(ballState.Location);
            Position2D st = GameParameters.InFieldSize(state.Location);

            if (GameParameters.IsInDangerousZone(ball, false, 0.2, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                radi = GameParameters.SafeRadi(state, marg);
            }
            bool indangerzone = false;
            if (GameParameters.IsInDangerousZone(ball, false, -0.1, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                indangerzone = true;
            }
            //  DrawingObjects.AddObject
            isMarker = false;
            //DrawingObjects.AddObject(C1);
            //DrawingObjects.AddObject(L1);
            double minDist = double.MaxValue;
            Circle C1 = new Circle(GameParameters.OurGoalCenter, radi);
            //Position2D NextPos = new Position2D();
            if (!indangerzone)
            {
                // Line L1 = new Line(st, GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
                Line L1 = new Line(st, GameParameters.OurGoalRight + new Vector2D(0, GameParameters.OurGoalLeft.DistanceFrom(GameParameters.OurGoalCenter) / 2 - .05));
                List<Position2D> P = C1.Intersect(L1);

                for (int i = 0; i < P.Count; i++)
                {
                    if ((P[i].DistanceFrom(state.Location) < minDist))
                    {
                        minDist = P[i].DistanceFrom(st);
                        DefencePos = P[i];
                    }
                }

                //Line L2 = new Line(st, GameParameters.OurGoalLeft - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
                //List<Position2D> P2 = C1.Intersect(L2);

                //  minDist = double.MaxValue;
                //for (int i = 0; i < P2.Count; i++)
                //{
                //    if (P2[i].DistanceFrom(st) < minDist)
                //    {
                //        minDist = P2[i].DistanceFrom(st);
                //        NextPos = P2[i];
                //    }
                //}
            }
            else
            {
                Vector2D v = st - GameParameters.OurGoalCenter;
                Position2D p = (GameParameters.OurGoalCenter + v.GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(st, Vector2D.Zero, 0), marg)));
                p = p + Vector2D.FromAngleSize(v.AngleInRadians + Math.PI / 2, 0.1);
                DefencePos = GameParameters.OurGoalCenter + (p - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(p, Vector2D.Zero, 0), marg));
            }

            if (ball.Y < 0)
            {
                Vector2D l1 = ball - GameParameters.OurGoalCenter;
                Vector2D l2 = GameParameters.OurRightCorner - GameParameters.OurGoalCenter;
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13)
                {
                    Line tmpL = new Line(st, GameParameters.OurGoalCenter.Extend(0, 0.005));
                    List<Position2D> poses = C1.Intersect(tmpL);
                    minDist = double.MaxValue;
                    for (int i = 0; i < poses.Count; i++)
                    {
                        if ((poses[i].DistanceFrom(state.Location) < minDist))
                        {
                            DefencePos = poses[i];
                            minDist = poses[i].DistanceFrom(st);
                        }
                    };

                }
            }
            else
            {
                Vector2D l1 = ballState.Location - GameParameters.OurGoalCenter;
                Vector2D l2 = GameParameters.OurLeftCorner - GameParameters.OurGoalCenter;
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13 || ball.X > GameParameters.OurGoalCenter.X - 0.09)
                {
                    DefencePos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize(1.79, radi);
                }

            }
            //}
            //else
            {

                //var dist = ballState.Location.DistanceFrom(st);

                //var stdefence = ballState.Location + Vector2D.FromAngleSize((st - ball).AngleInRadians, 4);

                //C1 = new Circle(GameParameters.OurGoalCenter, radi);

                //Line l = new Line(ball, stdefence);

                //List<Position2D> inters = C1.Intersect(l);
                //var pos = inters.First(f => f.DistanceFrom(st) <= inters.Min(m => m.DistanceFrom(st)));
                //radi = GameParameters.SafeRadi(new SingleObjectState() { Location = pos }, 0.12);
                //pos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize((pos - GameParameters.OurGoalCenter).AngleInRadians - 0.1, radi);





                // DrawingObjects.AddObject(C1);
                //DrawingObjects.AddObject(l);
            }

            if (DefencePos.X > GameParameters.OurGoalCenter.X - 0.09)
                DefencePos = new Position2D(GameParameters.OurGoalCenter.X - 0.09, DefencePos.Y);
            lastsecond = DefencePos;
            DefenceInfo di = new DefenceInfo();
            //if (DefencePos.DistanceFrom(NextPos) < RobotParameters.OurRobotParams.Diameter/2 && secondLastPos.HasValue)
            //{
            //  DefencePos = secondLastPos.Value;
            //    //    int? MarkerID = FindMarker(Model);
            //    //    if (MarkerID != null)
            //    //    {
            //    //        isMarker = true;
            //    //        radi = GameParameters.SafeRadi(Model.Opponents[MarkerID.Value], 0.02);
            //    //        di.TargetState = Model.Opponents[MarkerID.Value];
            //    //        DefencePos = GameParameters.OurGoalCenter + (Model.Opponents[MarkerID.Value].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(radi);
            //    //    }
            //    //    else
            //    //    {
            //    //        di.TargetState = ballState;
            //    //        if (ballState.Location.Y < 0)
            //    //        {
            //    //            Position2D Corner = new Position2D(0, GameParameters.OurLeftCorner.Y);
            //    //            DefencePos = GameParameters.OurGoalCenter + (Corner - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.2);
            //    //        }
            //    //        else
            //    //        {
            //    //            Position2D Corner = new Position2D(0, GameParameters.OurRightCorner.Y);
            //    //            DefencePos = GameParameters.OurGoalCenter + (Corner - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.2);
            //    //        }
            //    //    }
            //}
            //else
            //{
            //    secondLastPos = DefencePos;
            //}
            //DefencePos.DrawColor = System.Drawing.Color.Blue;
            //DrawingObjects.AddObject(DefencePos);
            di.OppID = oppid;
            di.TargetState = state;
            di.DefenderPosition = lastsecond;
            di.RoleType = typeof(StaticDefender2);
            secondLastPos = DefencePos;
            return di;
        }
        private static int? FindNearestOpponentToBall(WorldModel Model)
        {
            int? retIndex = null;
            double MinDist = double.MaxValue;
            foreach (int key in Model.Opponents.Keys)
            {
                if (ballState.Location.DistanceFrom(Model.Opponents[key].Location) < MinDist)
                {
                    MinDist = ballState.Location.DistanceFrom(Model.Opponents[key].Location);
                    retIndex = key;
                }
            }
            return retIndex;
        }
        private static int? FindMarker(WorldModel Model, SingleObjectState ballState, SingleObjectState ballStateFast)
        {
            int? retIndex = null;
            double MinDist = double.MaxValue;
            int? BallCacher = FindNearestOpponentToBall(Model);
            if (BallCacher != null)
            {
                foreach (int key in Model.Opponents.Keys)
                {
                    if (key != BallCacher)
                    {
                        if (GameParameters.OurGoalCenter.DistanceFrom(Model.Opponents[key].Location) < MinDist)
                        {
                            MinDist = ballState.Location.DistanceFrom(Model.Opponents[key].Location);
                            retIndex = key;
                        }
                    }
                }
            }
            return retIndex;
        }
        public static int? GetOurBallOwner(WorldModel Model, int? firstID, int? secondID)
        {
            List<int> defenders = new List<int>();
            if (firstID.HasValue)
                defenders.Add(firstID.Value);
            if (secondID.HasValue)
                defenders.Add(secondID.Value);
            if (defenders.Count == 0)
            {
                LastOwner = null;
                return null;
            }
            if (!GameParameters.IsInField(ballState.Location, 0.1))
                return null;
            Position2D pos = new Position2D();
            double minDistOpp = 100;
            if (Model.Opponents.Count > 0)
                minDistOpp = Model.Opponents.Min(m => m.Value.Location.DistanceFrom(ballState.Location));
            if (minDistOpp < 0.5)
            {
                LastOwner = null;
                return null;
            }

            if (LastOwner.HasValue && Model.OurRobots.ContainsKey(LastOwner.Value))
            {
                pos = Model.OurRobots[LastOwner.Value].Location;
            }
            else if (firstID.HasValue && secondID.HasValue)
            {
                Position2D perp1 = new Position2D(), perp2 = new Position2D();
                perp1 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[firstID.Value].Location);
                perp2 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[secondID.Value].Location);

                if (perp1.DistanceFrom(ballState.Location) > perp2.DistanceFrom(ballStateFast.Location))
                    pos = Model.OurRobots[firstID.Value].Location;
                else
                    pos = Model.OurRobots[secondID.Value].Location;
            }
            else if (firstID.HasValue)
            {
                pos = Model.OurRobots[firstID.Value].Location;
            }

            if (pos.DistanceFrom(ballStateFast.Location) > 0.8)
                return null;
            Vector2D ballSpeed = ballStateFast.Speed;
            double v = Vector2D.AngleBetweenInRadians(ballSpeed, (pos - ballStateFast.Location));
            double maxIncomming = 2, maxVertical = 0.5, maxOutGoing = 1;
            double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
                + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
                + maxVertical;


            double stateCoef = 1;
            if (FreekickDefence.StaticCenterState == CenterDefenderStates.BallInFront || FreekickDefence.StaticFirstState == DefenderStates.BallInFront || FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
                stateCoef = 1.2;

            if (ballSpeed.Size < acceptableballRobotSpeed * stateCoef)
            {
                double accour = 2, accopp = 3;

                double dist = Model.OurRobots.Min(m => m.Value.Location.DistanceFrom(ballState.Location));
                var robot = Model.OurRobots.First(f => f.Value.Location.DistanceFrom(ballState.Location) == dist);
                var T_our = Model.OurRobots.Where(w => (LastOwner.HasValue) ? w.Key == LastOwner.Value : w.Key == robot.Key).Select(s => new
                {
                    robotID = s.Key,
                    t = 2 * Math.Sqrt(s.Value.Location.DistanceFrom(ballState.Location) / accour)
                });
                int goalieId = Model.GoalieID.Value;
                var Our_other = Model.OurRobots.Where(w => !defenders.Contains(w.Key) && w.Key != goalieId).Select(s => new
                {
                    t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(ballState.Location)) / accopp)
                });
                var opp = Model.Opponents.Where(w => w.Value.Location.DistanceFrom(ballState.Location) == minDistOpp).Select(s => new
                {
                    t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(ballState.Location)) / accopp)
                });
                var T_other = Our_other.Union(opp);
                double minT_other = 100;
                double minT_our = 100;
                if (T_other.Count() > 0)
                    minT_other = T_other.Min(m => m.t);
                if (T_our.Count() > 0)
                    minT_our = T_our.Min(m => m.t);

                if (minT_our < minT_other * stateCoef)
                {
                    LastOwner = T_our.First(f => f.t == minT_our).robotID;
                    return LastOwner;
                }
            }
            LastOwner = null;
            return null;
        }
        public static void CalculateStaticPos(GameStrategyEngine engine, WorldModel Model, Dictionary<int, RoleBase> currentRoles)
        {
            double dis1, dis2;

            // int? ballOwner = engine.GameInfo.OurTeam.BallOwner;

            double t;
            var d1 = CurrentInfos.FirstOrDefault(s => s.RoleType == typeof(StaticDefender1));
            if (d1 == null)
                d1 = new DefenceInfo()
                {
                    RoleType = typeof(StaticDefender1)
                };

            var d2 = CurrentInfos.FirstOrDefault(s => s.RoleType == typeof(StaticDefender2));
            if (d2 == null)
                d2 = new DefenceInfo()
                {
                    RoleType = typeof(StaticDefender2)
                };

            var d3 = CurrentInfos.FirstOrDefault(s => s.RoleType == typeof(CenterBackNormalRole));
            if (d3 == null)
                d3 = new DefenceInfo()
                {
                    RoleType = typeof(CenterBackNormalRole)
                };

            var d1Role = currentRoles.FirstOrDefault(s => s.Value.GetType() == typeof(StaticDefender1));
            var d2Role = currentRoles.FirstOrDefault(s => s.Value.GetType() == typeof(StaticDefender2));
            var d3Role = currentRoles.FirstOrDefault(s => s.Value.GetType() == typeof(CenterBackNormalRole));

            bool containId1 = d1Role.Value != null && Model.OurRobots.ContainsKey(d1Role.Key);
            bool containId2 = d2Role.Value != null && Model.OurRobots.ContainsKey(d2Role.Key);
            bool containID3 = d3Role.Value != null && Model.OurRobots.ContainsKey(d3Role.Key);

            int? nullid = null;
            int? ballOwnerForCenetr = GetOurBallOwner(Model, (containID3) ? d3Role.Key : nullid, null);
            int? ballOwner = GetOurBallOwner(Model, (containId1) ? d1Role.Key : nullid, (containId2) ? d2Role.Key : nullid);

            if (GameParameters.IsInDangerousZone(ballState.Location, false, 0.1, out dis1, out dis2) && ballStateFast.Speed.Size < 2)
            {
                LastOwner = null;

                if (containId1)
                    DrawingObjects.AddObject(new StringDraw("InPenaltyArea", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                if (containId2)
                    DrawingObjects.AddObject(new StringDraw("InPenaltyArea", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                if (containID3)
                    DrawingObjects.AddObject(new StringDraw("InPenaltyArea", "ds3", Model.OurRobots[d3Role.Key].Location + new Vector2D(0.2, 0.2)));

                StaticSecondState = DefenderStates.Normal;
                StaticFirstState = DefenderStates.Normal;
                StaticCenterState = CenterDefenderStates.Normal;
                return;
            }
            if ((containId1 || containId2 || containID3) && BallKickedToGoal(Model))
            {
                LastOwner = null;
                Position2D perp1 = new Position2D(), perp2 = new Position2D(), perp3 = new Position2D();

                if (containId1)
                    perp1 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[d1Role.Key].Location);

                if (containId2)
                    perp2 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[d2Role.Key].Location);

                if (containID3)
                    perp3 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[d3Role.Key].Location);

                if (containId1 && (!containId2 || perp1.DistanceFrom(Model.OurRobots[d1Role.Key].Location) < perp2.DistanceFrom(Model.OurRobots[d2Role.Key].Location)))
                {
                    d1.DefenderPosition = Dive(engine, Model, d1Role.Key);
                    StaticFirstState = DefenderStates.KickToGoal;
                    StaticSecondState = DefenderStates.Normal;
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("KickToGoal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                }
                else if (containId2)
                {
                    d2.DefenderPosition = Dive(engine, Model, d2Role.Key);
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("KickToGoal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                    StaticFirstState = DefenderStates.Normal;
                    StaticSecondState = DefenderStates.KickToGoal;
                }
                else if (containID3)
                {
                    d3.DefenderPosition = Dive(engine, Model, d3Role.Key);
                    StaticCenterState = CenterDefenderStates.KickToGoal;
                    if (containID3)
                        DrawingObjects.AddObject(new StringDraw("KickToGoal", "ds3", Model.OurRobots[d3Role.Key].Location + new Vector2D(0.2, 0.2)));

                }
                CurrentInfos = OverLapSolvingStatic(Model, CurrentInfos.Where(w => w.RoleType != null).ToList());
            }
            else if (((ballOwnerForCenetr.HasValue && (containID3 && ballOwnerForCenetr.Value == d3Role.Key))
                    || ballOwner.HasValue && ((containId1 && ballOwner.Value == d1Role.Key) || (containId2 && ballOwner.Value == d2Role.Key)) && Model.BallState.Location.X > 2.2))
            {
                if (containId1 && ballOwner.Value == d1Role.Key)
                {
                    d1.DefenderPosition = GetBackBallPoint(Model, d1Role.Key, out t);
                    StaticFirstState = DefenderStates.BallInFront;
                    StaticSecondState = DefenderStates.Normal;
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("BallInFront", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                }
                else if (containId2 && ballOwner.Value == d2Role.Key)
                {
                    d2.DefenderPosition = GetBackBallPoint(Model, d2Role.Key, out t);
                    StaticFirstState = DefenderStates.Normal;
                    StaticSecondState = DefenderStates.BallInFront;
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("BallInFront", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                }
                else if (containID3 && ballOwnerForCenetr.Value == d3Role.Key)
                {
                    d3.DefenderPosition = GetBackBallPoint(Model, d3Role.Key, out t);
                    StaticCenterState = CenterDefenderStates.BallInFront;
                    if (containID3)
                        DrawingObjects.AddObject(new StringDraw("BallInFront", "ds1", Model.OurRobots[d3Role.Key].Location + new Vector2D(0.2, 0.2)));
                }
                CurrentInfos = OverLapSolvingStatic(Model, CurrentInfos.Where(w => w.RoleType != null).ToList());
            }
            else
            {
                LastOwner = null;
                if (containId1)
                    DrawingObjects.AddObject(new StringDraw("Normal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                if (containId2)
                    DrawingObjects.AddObject(new StringDraw("Normal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                if (containID3)
                    DrawingObjects.AddObject(new StringDraw("Normal", "ds1", Model.OurRobots[d3Role.Key].Location + new Vector2D(0.2, 0.2)));
                StaticFirstState = DefenderStates.Normal;
                StaticSecondState = DefenderStates.Normal;
                StaticCenterState = CenterDefenderStates.Normal;
            }
        }
        private static bool InconmmingOutgoing(WorldModel Model, int RobotID, ref bool isNear)
        {
            Position2D temprobot = Model.Opponents[RobotID].Location + Model.Opponents[RobotID].Speed * 0.04;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + ballStateFast.Speed * 0.04;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            if (ballStateFast.Speed.Size > 2)
            {
                double coef = 1;
                if (LastRB == RBstate.Robot)
                    coef = 1.2;

                double ballspeedAngle = ballStateFast.Speed.AngleInDegrees;
                double robotballInner = Model.Opponents[RobotID].Speed.InnerProduct((ballState.Location - Model.Opponents[RobotID].Location).GetNormnalizedCopy());
                bool ballinGoal = false;
                Line line = new Line();
                line = new Line(ballStateFast.Location, ballStateFast.Location - ballStateFast.Speed);
                Position2D BallGoal = new Position2D();
                BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
                double d = ballStateFast.Location.DistanceFrom(GameParameters.OurGoalCenter);
                if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.65 / coef && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.65 / coef)
                    if (ballStateFast.Speed.InnerProduct(GameParameters.OurGoalRight - ballStateFast.Location) > 0)
                        ballinGoal = true;

                if (ballState.Speed.InnerProduct((temprobot - tempball).GetNormnalizedCopy()) > 1.2 / coef
                    && robotballInner < 2 * coef && robotballInner > -1
                    && !ballinGoal)
                    return true;

            }
            return false;
        }
        private static int? StaticRB(GameStrategyEngine engine, WorldModel Model, List<DefenceInfo> CurrentInfo)
        {

            if (StaticFirstState == DefenderStates.BallInFront || StaticSecondState == DefenderStates.BallInFront || StaticCenterState == CenterDefenderStates.BallInFront)
            {
                LastRB = RBstate.Ball;
                return null;
            }
            Position2D? g;
            var opps = engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
            double d1, d2;
            if (opps.Count > 0 && GameParameters.IsInDangerousZone(ballState.Location, false, 0, out d1, out d2))
            {
                if (!GameParameters.IsInDangerousZone(Model.Opponents[opps.First()].Location, false, 0, out d1, out d2))
                {
                    LastRB = RBstate.Robot;
                    return opps.First();
                }
                else if (opps.Count > 1)
                {
                    LastRB = RBstate.Robot;
                    return opps.Skip(1).First();
                }
                else
                {
                    LastRB = RBstate.Ball;
                    return null;
                }
            }
            else if (opps.Count > 0 && GameParameters.IsInDangerousZone(Model.Opponents[opps.First()].Location, false, 0.03, out d1, out d2))
            {
                LastRB = RBstate.Ball;
                return null;
            }
            if (opps.Count == 0 || ballStateFast.Speed.Size < 1)
            {
                LastRB = RBstate.Ball;
                return null;
            }
            SingleObjectState oppstate = Model.Opponents[opps.First()];

            Position2D temprobot = oppstate.Location + oppstate.Speed * 0.2;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + ballState.Speed * 0.2;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            SingleObjectState ball = ballState;
            Vector2D ballRobot = temprobot - tempball;
            Vector2D robotTarget = GameParameters.OurGoalCenter - temprobot;
            double ballAngle = Vector2D.AngleBetweenInDegrees(ballRobot, robotTarget);

            if (InconmmingOutgoing(Model, opps.First(), ref incomningNear))
            {
                LastRB = RBstate.Robot;
                return opps.First();
            }

            LastRB = RBstate.Ball;
            return null;
        }

        #endregion

        #region comment
        //private static int? StaticRB(GameStrategyEngine engine, WorldModel Model, List<DefenceInfo> CurrentInfo)
        //{

        //    Position2D? g;
        //    var opps = engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
        //    double d1, d2;
        //    if (opps.Count > 0 && GameParameters.IsInDangerousZone(ballState.Location, false, 0.03, out d1, out d2))
        //    {
        //        if (!GameParameters.IsInDangerousZone(Model.Opponents[opps.First()].Location, false, 0.03, out d1, out d2))
        //            return opps.First();
        //        else if (opps.Count > 1)
        //        {
        //            return opps.Skip(1).First();
        //        }
        //        else
        //            return null;
        //    }
        //    if (opps.Count == 0 || ballState.Speed.Size < 0.5)
        //        return null;

        //    SingleObjectState oppstate = Model.Opponents[opps.First()];
        //    Position2D? Defendball = CalculateFirstStatic(Model, ballState, opps.First()).DefenderPosition;
        //    Position2D? Defendrobot = CalculateFirstStatic(Model, oppstate, opps.First()).DefenderPosition;

        //    Position2D temprobot = oppstate.Location + oppstate.Speed * 0.5;
        //    temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
        //    temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
        //    temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
        //    temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);
        //    SingleObjectState nextrobot = new SingleObjectState() { Type = ObjectType.Opponent, Location = temprobot, Speed = oppstate.Speed };
        //    Position2D? Defendrobotnext = CalculateFirstStatic(Model, nextrobot, opps.First()).DefenderPosition;

        //    Position2D tempball = ballState.Location + ballState.Speed * 0.5;
        //    tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
        //    tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
        //    tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
        //    tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);
        //    SingleObjectState nextball = new SingleObjectState() { Type = ObjectType.Ball, Location = tempball, Speed = ballState.Speed };
        //    Position2D? Defendballnext = CalculateFirstStatic(Model, nextball, opps.First()).DefenderPosition;

        //    if (Defendrobot.HasValue && Defendball.HasValue)
        //    {
        //        double db = Defendball.Value.DistanceFrom(Defendballnext.Value);
        //        double dr = Defendrobot.Value.DistanceFrom(Defendrobotnext.Value);
        //        if (db > dr + 0.065)
        //        {
        //            return opps.First();
        //        }
        //        else if (dr > db + 0.065)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            if (CurrentInfo != null && CurrentInfo.Any(a => a.RoleType == typeof(StaticDefender1)))
        //            {
        //                DefenceInfo dinfo = CurrentInfo.Single(s => s.RoleType == typeof(StaticDefender1));

        //                if (dinfo.TargetState.Type == ObjectType.Ball)
        //                {
        //                    return null;
        //                }
        //                else
        //                {
        //                    return opps.First();
        //                }
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //    else if (Defendrobot.HasValue)
        //    {
        //        return opps.First();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #region first_Normal_With IO RB
        //#region 1st Defender
        //private static DefenceInfo CalculateNormalFirst ( GameStrategyEngine engine , WorldModel Model , DefenderCommand Command , out DefenceInfo GoaliRes , DefenderCommand GoaliCommand , List<DefenceInfo> CurrentInfo )
        //{
        //    DefenceInfo def = new DefenceInfo ();
        //    GoaliRes = new DefenceInfo ();

        //    Position2D? goalieball;
        //    Position2D? goalierobot;
        //    Position2D? g;
        //    GoaliPositioningMode BallMode = ChooseMode ( ballState , CurrentGoalieMode );
        //    Position2D? Defendball = CalculatePos ( engine , Model , ballState , BallMode , out goalieball );
        //    if ( ballState.Speed.Size < 0.5 || !Command.OppID.HasValue || !Model.Opponents.ContainsKey ( Command.OppID.Value ) )
        //    {
        //        GoaliRes.OppID = ( Command.OppID.HasValue && Model.Opponents.ContainsKey ( Command.OppID.Value ) ) ? Command.OppID : null;
        //        if ( ( Command.OppID.HasValue && Model.Opponents.ContainsKey ( Command.OppID.Value ) ) )
        //            GoaliRes.OppID = Command.OppID;
        //        else
        //            GoaliRes.OppID = null;
        //        GoaliRes.RoleType = GoaliCommand.RoleType;
        //        GoaliRes.TargetState = ballState;
        //        GoaliRes.Mode = BallMode;
        //        GoaliRes.DefenderPosition = goalieball;

        //        if ( ( Command.OppID.HasValue && Model.Opponents.ContainsKey ( Command.OppID.Value ) ) )
        //            def.OppID = Command.OppID;
        //        else
        //            def.OppID = null;
        //        def.RoleType = Command.RoleType;
        //        def.TargetState = ballState;
        //        def.Mode = BallMode;
        //        def.DefenderPosition = Defendball;
        //    }
        //    else
        //    {
        //        SingleObjectState oppstate = Model.Opponents [Command.OppID.Value];
        //        GoaliPositioningMode RobotMode = ChooseMode ( oppstate , CurrentGoalieMode );
        //        Position2D? Defendrobot = CalculatePos ( engine , Model , oppstate , RobotMode , out goalierobot );

        //        Position2D temprobot = oppstate.Location + oppstate.Speed * 0.5;
        //        temprobot.X = Math.Max ( temprobot.X , GameParameters.OppGoalCenter.X + 0.05 );
        //        temprobot.X = Math.Min ( temprobot.X , GameParameters.OurGoalCenter.X - 0.05 );
        //        temprobot.Y = Math.Max ( temprobot.Y , GameParameters.OurRightCorner.Y + 0.05 );
        //        temprobot.Y = Math.Min ( temprobot.Y , GameParameters.OurLeftCorner.Y - 0.05 );

        //        SingleObjectState nextrobot = new SingleObjectState ()
        //        {
        //            Type = ObjectType.Opponent ,
        //            Location = temprobot ,
        //            Speed = oppstate.Speed
        //        };
        //        GoaliPositioningMode nextRobotMode = ChooseMode ( nextrobot , CurrentGoalieMode );
        //        Position2D? Defendrobotnext = CalculatePos ( engine , Model , nextrobot , nextRobotMode , out g );

        //        Position2D tempball = ballState.Location + ballState.Speed * 0.5;
        //        tempball.X = Math.Max ( tempball.X , GameParameters.OppGoalCenter.X + 0.05 );
        //        tempball.X = Math.Min ( tempball.X , GameParameters.OurGoalCenter.X - 0.05 );
        //        tempball.Y = Math.Max ( tempball.Y , GameParameters.OurRightCorner.Y + 0.05 );
        //        tempball.Y = Math.Min ( tempball.Y , GameParameters.OurLeftCorner.Y - 0.05 );
        //        SingleObjectState nextball = new SingleObjectState ()
        //        {
        //            Type = ObjectType.Ball ,
        //            Location = tempball ,
        //            Speed = ballState.Speed
        //        };
        //        GoaliPositioningMode nextBallMode = ChooseMode ( nextball , CurrentGoalieMode );
        //        Position2D? Defendballnext = CalculatePos ( engine , Model , nextball , nextBallMode , out g );

        //        if ( Defendrobot.HasValue && Defendball.HasValue )
        //        {

        //            double db = Defendball.Value.DistanceFrom ( Defendballnext.Value );
        //            double dr = Defendrobot.Value.DistanceFrom ( Defendrobotnext.Value );
        //            if ( db > dr + 0.065 )
        //            {
        //                GoaliRes.OppID = Command.OppID;
        //                GoaliRes.RoleType = GoaliCommand.RoleType;
        //                GoaliRes.TargetState = oppstate;
        //                GoaliRes.Mode = RobotMode;
        //                GoaliRes.DefenderPosition = goalierobot;

        //                def.OppID = Command.OppID;
        //                def.RoleType = Command.RoleType;
        //                def.TargetState = oppstate;
        //                def.Mode = RobotMode;
        //                def.DefenderPosition = Defendrobot;
        //            }
        //            else if ( dr > db + 0.065 )
        //            {
        //                GoaliRes.OppID = Command.OppID;
        //                GoaliRes.RoleType = GoaliCommand.RoleType;
        //                GoaliRes.TargetState = ballState;
        //                GoaliRes.Mode = BallMode;
        //                GoaliRes.DefenderPosition = goalieball;

        //                def.OppID = Command.OppID;
        //                def.RoleType = Command.RoleType;
        //                def.TargetState = ballState;
        //                def.Mode = BallMode;
        //                def.DefenderPosition = Defendball;
        //            }
        //            else
        //            {
        //                if ( CurrentInfo != null && CurrentInfo.Any ( a => a.RoleType == Command.RoleType ) )
        //                {
        //                    DefenceInfo dinfo = CurrentInfo.Single ( s => s.RoleType == Command.RoleType );

        //                    if ( dinfo.TargetState.Type == ObjectType.Ball )
        //                    {
        //                        GoaliRes.OppID = Command.OppID;
        //                        GoaliRes.RoleType = GoaliCommand.RoleType;
        //                        GoaliRes.TargetState = ballState;
        //                        GoaliRes.Mode = BallMode;
        //                        GoaliRes.DefenderPosition = goalieball;

        //                        def.OppID = Command.OppID;
        //                        def.RoleType = Command.RoleType;
        //                        def.TargetState = ballState;
        //                        def.Mode = BallMode;
        //                        def.DefenderPosition = Defendball;
        //                    }
        //                    else
        //                    {
        //                        GoaliRes.OppID = Command.OppID;
        //                        GoaliRes.RoleType = GoaliCommand.RoleType;
        //                        GoaliRes.TargetState = oppstate;
        //                        GoaliRes.Mode = RobotMode;
        //                        GoaliRes.DefenderPosition = goalierobot;

        //                        def.OppID = Command.OppID;
        //                        def.RoleType = Command.RoleType;
        //                        def.TargetState = oppstate;
        //                        def.Mode = RobotMode;
        //                        def.DefenderPosition = Defendrobot;
        //                    }
        //                }
        //                else
        //                {
        //                    GoaliRes.OppID = Command.OppID;
        //                    GoaliRes.RoleType = GoaliCommand.RoleType;
        //                    GoaliRes.TargetState = ballState;
        //                    GoaliRes.Mode = BallMode;
        //                    GoaliRes.DefenderPosition = goalieball;

        //                    def.OppID = Command.OppID;
        //                    def.RoleType = Command.RoleType;
        //                    def.TargetState = ballState;
        //                    def.Mode = BallMode;
        //                    def.DefenderPosition = Defendball;
        //                }
        //            }
        //        }
        //        else if ( Defendrobot.HasValue )
        //        {
        //            GoaliRes.OppID = Command.OppID;
        //            GoaliRes.RoleType = GoaliCommand.RoleType;
        //            GoaliRes.TargetState = oppstate;
        //            GoaliRes.Mode = RobotMode;
        //            GoaliRes.DefenderPosition = goalierobot;

        //            def.OppID = Command.OppID;
        //            def.RoleType = Command.RoleType;
        //            def.TargetState = oppstate;
        //            def.Mode = RobotMode;
        //            def.DefenderPosition = Defendrobot;
        //        }
        //        else
        //        {
        //            GoaliRes.OppID = Command.OppID;
        //            GoaliRes.RoleType = GoaliCommand.RoleType;
        //            GoaliRes.TargetState = ballState;
        //            GoaliRes.Mode = BallMode;
        //            GoaliRes.DefenderPosition = goalieball;

        //            def.OppID = Command.OppID;
        //            def.RoleType = Command.RoleType;
        //            def.TargetState = ballState;
        //            def.Mode = BallMode;
        //            def.DefenderPosition = Defendball;
        //        }
        //    }
        //    GoaliRes.Teta = ( GoaliRes.TargetState.Location - GoaliRes.DefenderPosition.Value ).AngleInDegrees;
        //    return def;
        //}
        //#endregion
        //#region 2nd defender
        //private static DefenceInfo CalculateNormalSecond ( GameStrategyEngine engine , WorldModel Model , DefenderCommand Command , out DefenceInfo GoaliRes , List<DefenceInfo> CurrentInfo )
        //{
        //    DefenceInfo def = new DefenceInfo ();
        //    GoaliRes = new DefenceInfo ();

        //    Position2D? goalieball;
        //    Position2D? goalierobot;
        //    Position2D? g;
        //    GoaliPositioningMode BallMode = ChooseMode ( ballState , CurrentGoalieMode );
        //    Position2D? Defendball = CalculatePos ( engine , Model , ballState , BallMode , out goalieball );
        //    if ( !Command.OppID.HasValue || !Model.Opponents.ContainsKey ( Command.OppID.Value ) )
        //    {
        //        GoaliRes.OppID = null;
        //        GoaliRes.TargetState = ballState;
        //        GoaliRes.Mode = BallMode;
        //        GoaliRes.DefenderPosition = goalieball;

        //        def.OppID = null;
        //        def.RoleType = Command.RoleType;
        //        def.TargetState = ballState;
        //        def.Mode = BallMode;
        //        def.DefenderPosition = Defendball;
        //    }
        //    else
        //    {
        //        SingleObjectState oppstate = Model.Opponents [Command.OppID.Value];
        //        GoaliPositioningMode RobotMode = ChooseMode ( oppstate , CurrentGoalieMode );
        //        Position2D? Defendrobot = CalculatePos ( engine , Model , oppstate , RobotMode , out goalierobot );

        //        GoaliRes.OppID = Command.OppID.Value;
        //        GoaliRes.TargetState = oppstate;
        //        GoaliRes.Mode = RobotMode;
        //        GoaliRes.DefenderPosition = goalierobot;

        //        def.OppID = Command.OppID.Value;
        //        def.RoleType = Command.RoleType;
        //        def.TargetState = oppstate;
        //        def.Mode = RobotMode;
        //        def.DefenderPosition = Defendrobot;
        //    }

        //    return def;
        //}
        //#endregion
        #endregion
        #endregion

        static int? lastopp = null;

        static bool incomningNear = false;
        private static double extendStatticDefenceTarget = 0;

        private static bool BallKickedToGoal(WorldModel Model)
        {
            Line line = new Line();
            line = new Line(ballStateFast.Location, ballStateFast.Location - ballStateFast.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballStateFast.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.15 && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.15)
                if (ballStateFast.Speed.InnerProduct(GameParameters.OurGoalRight - ballStateFast.Location) > 0)
                    if (ballStateFast.Speed.Size > 0.1 && d / ballStateFast.Speed.Size < 2.2)
                        return true;
            return false;
        }

        public static Position2D Dive(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D pos = new Position2D();
            Position2D robotLoc = Model.OurRobots[RobotID].Location;
            Position2D ballLoc = ballStateFast.Location;
            Vector2D ballSpeed = ballStateFast.Speed;
            Position2D prep = ballSpeed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location);
            double dist, DistFromBorder, R;
            if (GameParameters.IsInDangerousZone(prep, false, 0, out dist, out DistFromBorder))
            {
                R = GameParameters.SafeRadi(new SingleObjectState(prep, new Vector2D(), 0), .05 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .05)));
                pos = GameParameters.OurGoalCenter - ballSpeed.GetNormalizeToCopy(R);
            }
            else
                pos = prep;
            return pos;
        }

        public static Position2D GetBackBallPoint(WorldModel Model, int RobotID, out double Teta)
        {
            Vector2D vec = ballState.Location - GameParameters.OurGoalCenter;
            Position2D tar = ballState.Location + vec;
            Vector2D ballSpeed = ballState.Speed;
            Position2D ballLocation = ballState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = tar - ballLocation;
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(0.09);
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;
            double segmentConst = 0.7;
            double rearDistance = 0.15;
            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            Position2D finalPosToGo = midPoint;
            double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

            if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
                finalPosToGo = backBallPoint;
            else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }

            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);
            Teta = (vec).AngleInDegrees;
            return finalPosToGo;
        }

        public static bool oldDefFlags = false;
        public static void RestartActiveFlags()
        {
            SwitchToActiveMarker1 = false;
            SwitchToActiveMarker2 = false;
            SwitchToActiveMarker3 = false;
            SwitchDefender2Marker1 = false;
            SwitchDefender2Marker2 = false;
            SwitchDefender2Marker3 = false;
            SwitchDefender32Marker1 = false;
            SwitchDefender32Marker2 = false;
            SwitchDefender32Marker3 = false;
            SwitchDefender42Marker1 = false;
            SwitchDefender42Marker2 = false;
            SwitchDefender42Marker3 = false;

            SwitchCBMarkerToLBMarker = false;
            SwitchCBMarkerToRBMarker = false;
        }

        public static void SwitchToActiveReset()
        {
            DefenderCornerRole1ToActive = false;
            DefenderCornerRole2ToActive = false;
            DefenderCornerRole3ToActive = false;
            DefenderCornerRole4ToActive = false;
            DefenderMarkerRole1ToActive = false;
            DefenderMarkerRole2ToActive = false;
            DefenderMarkerRole3ToActive = false;
            DefenderRegionalRole1ToActive = false;
            DefenderRegionalRole2ToActive = false;
            DefenderGoToPointToActive = false;
        }
        #endregion
#else
        #region oldFreeKickDefence
        public static double AdditionalSafeRadi = 0.08;

        public static bool testDefenceState = false;

        public static bool newmotion = true;

        public static bool RearRegional = false;
        public static bool WeAreInCorner = false;
        public static int? LastOppToMark = null;
        public static int? LastOppToMark1 = null;
        public static int? LastOppToMark2 = null;
        public static int? LastOppToMark3 = null;
        public static int? LastOppToMark4 = null;

        public static bool switchAllMode = true;

        public static int? Static1ID = null;
        public static int? Static2ID = null;

        public static int? OppToMark1 = null;
        public static int? OppToMark2 = null;
        public static int? OppToMark3 = null;
        public static int? OppToMark4 = null;

        public static bool SwitchToActiveMarker1 = false;
        public static bool SwitchToActiveMarker2 = false;
        public static bool SwitchToActiveMarker3 = false;

        public static bool SwitchDefender2Marker1 = false;
        public static bool SwitchDefender2Marker2 = false;
        public static bool SwitchDefender2Marker3 = false;

        public static bool SwitchDefender32Marker1 = false;
        public static bool SwitchDefender32Marker2 = false;
        public static bool SwitchDefender32Marker3 = false;

        public static bool SwitchDefender42Marker1 = false;
        public static bool SwitchDefender42Marker2 = false;
        public static bool SwitchDefender42Marker3 = false;

        public static bool EaththeBall = false;
        public static bool ReadyForEatStatic1 = false;
        public static bool ReadyForEatStatic2 = false;

        public static bool LastSwitchDefender2Marker1 = false;
        public static bool LastSwitchDefender2Marker2 = false;
        public static bool LastSwitchDefender2Marker3 = false;

        public static bool LastSwitchDefender32Marker1 = false;
        public static bool LastSwitchDefender32Marker2 = false;
        public static bool LastSwitchDefender32Marker3 = false;

        public static bool LastSwitchDefender42Marker1 = false;
        public static bool LastSwitchDefender42Marker2 = false;
        public static bool LastSwitchDefender42Marker3 = false;

        public static bool weAreInKickoff = false;

        public static bool DefenderCornerRole1ToActive = false;
        public static bool DefenderCornerRole2ToActive = false;
        public static bool DefenderCornerRole3ToActive = false;
        public static bool DefenderCornerRole4ToActive = false;
        public static bool DefenderMarkerRole1ToActive = false;
        public static bool DefenderMarkerRole2ToActive = false;
        public static bool DefenderMarkerRole3ToActive = false;
        public static bool DefenderRegionalRole1ToActive = false;
        public static bool DefenderRegionalRole2ToActive = false;
        public static bool DefenderGoToPointToActive = false;


        public static bool freeSwitchbetweenRegionalAndMarker = false;

        public static bool StopToActive = false;

        public static bool DontShitPlease = false;
        public static int firstpicker = 0;
        public static int secondpicker = 0;
        public static int thirdpicker = 0;

        private static RBstate LastRB = RBstate.Ball;
        private static int? LastOwner = null;
        public static DefenderStates StaticFirstState = DefenderStates.Normal;
        public static DefenderStates StaticSecondState = DefenderStates.Normal;

        public static SingleObjectState ballState = new SingleObjectState();
        public static SingleObjectState ballStateFast = new SingleObjectState();

        private static Position2D lastfirst = new Position2D(), lastsecond = new Position2D();
        public static bool BallIsMoved = false;
        public static List<DefenceInfo> CurrentInfos = new List<DefenceInfo>();
        public static List<DefenderCommand> CurrentlyAddedDefenders = new List<DefenderCommand>();
        public static DefenceInfo GoalieInfo = new DefenceInfo();
        public static GoaliPositioningMode CurrentGoalieMode = GoaliPositioningMode.InRightSide;
        public static Dictionary<RoleBase, int> CurrentStates = new Dictionary<RoleBase, int>();

        public static Dictionary<Type, Position2D> PreviousPositions = new Dictionary<Type, Position2D>();
        public static bool BallIsMovedStrategy = false;

        public static List<Position2D> CalculateAvoiderTargets(GameStrategyEngine engine, WorldModel Model, out Obstacle obstacle, out Position2D goalieTarget, double avoidDist = 0.7)
        {
            List<Position2D> ret = new List<Position2D>();
            Position2D BallPlacementPos = StaticVariables.ballPlacementPos;
            Position2D ballLoc = Model.BallState.Location;

            Line mainAvoidLine = new Line(BallPlacementPos, ballLoc);
            Vector2D avoidVec = mainAvoidLine.Head - mainAvoidLine.Tail;
            Line extendedAvoidLine = new Line(mainAvoidLine.Head + avoidVec.GetNormalizeToCopy(avoidDist), mainAvoidLine.Tail - avoidVec.GetNormalizeToCopy(avoidDist));
            Vector2D avoidPrepL = Vector2D.FromAngleSize(avoidVec.AngleInRadians + Math.PI / 2, avoidDist);
            Vector2D avoidPrepR = Vector2D.FromAngleSize(avoidVec.AngleInRadians - Math.PI / 2, avoidDist);
            List<Line> avoidBounds = new List<Line>();
            avoidBounds.Add(new Line(extendedAvoidLine.Head + avoidPrepL, extendedAvoidLine.Tail + avoidPrepL, new Pen(Color.AliceBlue, 0.01f)));
            avoidBounds.Add(new Line(extendedAvoidLine.Head + avoidPrepR, extendedAvoidLine.Tail + avoidPrepR, new Pen(Color.AliceBlue, 0.01f)));
            avoidBounds.Add(new Line(avoidBounds[1].Head, avoidBounds[0].Head, new Pen(Color.AliceBlue, 0.01f)));
            avoidBounds.Add(new Line(avoidBounds[1].Tail, avoidBounds[0].Tail, new Pen(Color.AliceBlue, 0.01f)));
            goalieTarget = GameParameters.OurGoalCenter.Extend(-0.1, 0);

            if (GameParameters.OurGoalCenter.X - GameParameters.DefenceAreaHeight - BallPlacementPos.X >= avoidDist
                && GameParameters.OurGoalCenter.X - GameParameters.DefenceAreaHeight - ballLoc.X >= avoidDist)
            {
                Position2D extendedPos = GameParameters.OurGoalCenter.Extend(-(GameParameters.DefenceAreaHeight + RobotParameters.OurRobotParams.Diameter), -GameParameters.DefenceAreaWidth / 2);
                for (int i = 0; i < 7; i++)
                {
                    ret.Add(extendedPos.Extend(0, i * (RobotParameters.OurRobotParams.Diameter + 0.19)));
                    //DrawingObjects.AddObject(extendedPos.Extend(0, i * (RobotParameters.OurRobotParams.Diameter + 0.19)), "position" + i.ToString());
                }
            }
            else
            {

                bool hasIntersect = false;
                int c = 0;
                foreach (var item in avoidBounds)
                {
                    DrawingObjects.AddObject(item, "lineavoid" + c++);
                    if (GameParameters.LineIntersectWithOurDangerZone(item).Count > 0)
                    {
                        hasIntersect = true;
                        break;
                    }
                }
                if (!hasIntersect)
                {
                    //GameParameters.LineIntersectWithOurDangerZone(new Line)
                    Position2D extendedPos = GameParameters.OurGoalCenter.Extend(-(GameParameters.DefenceAreaHeight / 2 + RobotParameters.OurRobotParams.Diameter), -GameParameters.DefenceAreaWidth / 2.5);
                    for (int i = 0; i < 7; i++)
                    {
                        ret.Add(extendedPos.Extend(0, i * (RobotParameters.OurRobotParams.Diameter + 0.15)));
                        //DrawingObjects.AddObject(extendedPos.Extend(0, i * (RobotParameters.OurRobotParams.Diameter + 0.19)), "position" + i.ToString());
                    }
                }
                else
                {

                    double tmp_dist, distBorder;
                    if ((GameParameters.OurGoalCenter.X - BallPlacementPos.X < avoidDist + RobotParameters.OurRobotParams.Diameter
                            && GameParameters.OurGoalCenter.X - ballLoc.X < avoidDist + RobotParameters.OurRobotParams.Diameter) || (GameParameters.IsInDangerousZone(Model.BallState.Location, false, avoidDist, out tmp_dist, out distBorder) && GameParameters.OurGoalCenter.X - ballLoc.X < avoidDist + RobotParameters.OurRobotParams.Diameter))
                    {
                        int idx = -1, otheridx = -1;
                        Line farBound = null;
                        double minDist = double.MaxValue;
                        for (int i = 0; i < 2; i++)
                        {
                            var bound = avoidBounds[i];
                            var d = bound.Distance(Position2D.Zero);
                            if (d < minDist)
                            {
                                idx = i;
                                if (idx == 0)
                                    otheridx = 1;
                                else
                                    otheridx = 0;
                                minDist = d;
                                farBound = bound;
                            }
                        }
                        //var prep = farBound.PerpenducilarLineToPoint(Position2D.Zero).IntersectWithLine(farBound);
                        //if (!prep.HasValue)
                        //    prep = Position2D.Zero;
                        Vector2D v = (avoidBounds[idx].Head - avoidBounds[otheridx].Head).GetNormalizeToCopy(0.5);
                        Position2D extendedPos = farBound.Tail + v;
                        goalieTarget = extendedPos;
                        for (int i = 0; i < 7; i++)
                        {
                            extendedPos = extendedPos + (farBound.Head - farBound.Tail).GetNormalizeToCopy(0.3);
                            ret.Add(extendedPos);
                            //DrawingObjects.AddObject(extendedPos.Extend(0, i * (RobotParameters.OurRobotParams.Diameter + 0.19)), "position" + i.ToString());
                        }
                       // goalieTarget = extendedPos = extendedPos + (farBound.Head - farBound.Tail).GetNormalizeToCopy(0.3);

                    }
                    else
                    {
                        Position2D extendedPos = GameParameters.OurGoalCenter.Extend(-(RobotParameters.OurRobotParams.Diameter) / 2, -GameParameters.DefenceAreaWidth / 2.5);
                        for (int i = 0; i < 7; i++)
                        {
                            ret.Add(extendedPos.Extend(0, i * (RobotParameters.OurRobotParams.Diameter + 0.19)));
                            //DrawingObjects.AddObject(extendedPos.Extend(0, i * (RobotParameters.OurRobotParams.Diameter + 0.19)), "position" + i.ToString());
                        }
                    }
                }
            }
            Position2D center = Position2D.Interpolate(StaticVariables.ballPlacementPos, Model.BallState.Location, 0.5);
            Vector2D r = avoidBounds[0].Head - center;
            r.X = Math.Abs(r.X);
            r.Y = Math.Abs(r.Y);
            DrawingObjects.AddObject(new Line(center, center + r, new Pen(Color.Black, 0.01f)), "lineobstcleavoid");
            obstacle = new Obstacle()
            {
                State = new SingleObjectState(center, Vector2D.Zero, 0),
                R = r,
                Type = ObstacleType.Rectangle
            };
            return ret;
        }

        public static List<DefenceInfo> Match(GameStrategyEngine engine, WorldModel Model, List<DefenderCommand> Commands, bool OverLapIFirstDefender)
        {

            CurrentlyAddedDefenders = Commands;
            List<DefenceInfo> res = new List<DefenceInfo>();
            DefenceInfo TempGoali = new DefenceInfo();
            DefenderCommand goaliCommand = Commands.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IGoalie))).FirstOrDefault();
            Dictionary<Type, bool> MarkerstoRemove = new Dictionary<Type, bool>();
            Dictionary<Type, bool> FirstCornertoRemove = new Dictionary<Type, bool>();
            Dictionary<Type, bool> OtherstoRemove = new Dictionary<Type, bool>();
            foreach (var item in Commands)
            {
                if (item.RoleType.GetInterfaces().Any(a => a == typeof(IFirstDefender)))
                {
                    DefenceInfo def, goali;
                    def = CalculateNormalFirst(engine, Model, item, out goali, goaliCommand, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            FirstCornertoRemove[item.RoleType] = true;
                        }
                    }
                    TempGoali = goali;
                    if (item.willUse)
                        res.Add(def);
                }
                else if (item.RoleType.GetInterfaces().Any(a => a == typeof(ISecondDefender)))
                {
                    DefenceInfo def, goali;
                    def = CalculateNormalSecond(engine, Model, item, out goali, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            OtherstoRemove[item.RoleType] = true;
                        }
                    }
                    res.Add(def);
                }
                else if (item.RoleType.GetInterfaces().Any(a => a == typeof(IMarkerDefender)))
                {
                    DefenceInfo def;
                    def = Mark(engine, Model, item, CurrentInfos);
                    if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                    {
                        if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            MarkerstoRemove[item.RoleType] = true;
                        }
                    }
                    res.Add(def);
                }
            }
            List<DefenceInfo> temp = res.ToList();

            foreach (var item in Commands.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IRegionalDefender))).ToList())
            {
                DefenceInfo def;
                def = RegionalDefence(engine, Model, item, temp, CurrentInfos);
                if (CurrentStates.Any(i => i.Key.GetType() == item.RoleType) && CurrentStates.Where(o => o.Key.GetType() == item.RoleType).First().Value != (int)DefenderStates.Normal)
                {
                    if (PreviousPositions.ContainsKey(item.RoleType))// && PreviousPositions[item.RoleType].DistanceFrom(def.DefenderPosition.Value) > RobotParameters.OurRobotParams.Diameter / 2)
                    {
                        OtherstoRemove[item.RoleType] = true;
                    }
                }
                res.Add(def);
                temp.Add(def);
            }

            res = OverLapSolving(Model, res, FirstCornertoRemove, MarkerstoRemove, OtherstoRemove, OverLapIFirstDefender, true, true);

            Position2D? d = null;
            Position2D? d3 = null;
            Position2D? d4 = null;
            Position2D? m = null;
            Position2D? m2 = null;
            Position2D? m3 = null;
            for (int i = 0; i < res.Count; i++)
            {
                if (res[i].RoleType == typeof(DefenderCornerRole1) || res[i].RoleType == typeof(DefenderNormalRole1))
                {
                    d = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderCornerRole3))
                {
                    d3 = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderCornerRole4))
                {
                    d4 = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderMarkerRole) || res[i].RoleType == typeof(DefenderMarkerNormalRole1) || res[i].RoleType == typeof(NewDefenderMrkerRole))
                {
                    m = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderMarkerRole2) || res[i].RoleType == typeof(DefenderMarkerNormalRole2) || res[i].RoleType == typeof(NewDefenderMarkerRole2))
                {
                    m2 = res[i].DefenderPosition;
                }
                else if (res[i].RoleType == typeof(DefenderMarkerRole3) || res[i].RoleType == typeof(DefenderMarkerNormalRole3) || res[i].RoleType == typeof(NewDefenderMarkerRole3))
                {
                    m3 = res[i].DefenderPosition;
                }
                if (res[i].RoleType.GetInterfaces().Any(a => a == typeof(IFirstDefender)) || res[i].RoleType.GetInterfaces().Any(a => a == typeof(ISecondDefender)))
                    continue;
                Position2D pos = res[i].DefenderPosition.Value;
                res[i].DefenderPosition = CommonDefenceUtils.CheckForStopZone(BallIsMoved, pos, Model);
            }

            SwitchDefender2Marker1 = false;
            SwitchDefender2Marker2 = false;
            SwitchDefender2Marker3 = false;
            SwitchDefender32Marker1 = false;
            SwitchDefender32Marker2 = false;
            SwitchDefender32Marker3 = false;
            SwitchDefender42Marker1 = false;
            SwitchDefender42Marker2 = false;
            SwitchDefender42Marker3 = false;

            if ((m.HasValue || m2.HasValue || m3.HasValue) && (d.HasValue || d3.HasValue))
            {
                double dist, DistFromBorder;
                double margin = 0.3;

                if ((m.HasValue && d.HasValue) && GameParameters.IsInDangerousZone(m.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender2Marker1 = true;
                }
                if ((m2.HasValue && d.HasValue) && GameParameters.IsInDangerousZone(m2.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender2Marker2 = true;
                }
                if ((m3.HasValue && d.HasValue) && GameParameters.IsInDangerousZone(m3.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender2Marker3 = true;
                }
                if ((m.HasValue && d3.HasValue) && GameParameters.IsInDangerousZone(m.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d3.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender32Marker1 = true;
                }
                if ((m2.HasValue && d3.HasValue) && GameParameters.IsInDangerousZone(m2.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d3.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender32Marker2 = true;
                }
                if ((m3.HasValue && d3.HasValue) && GameParameters.IsInDangerousZone(m3.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d3.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender32Marker3 = true;
                }
                if ((m.HasValue && d4.HasValue) && GameParameters.IsInDangerousZone(m.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d4.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender42Marker1 = true;
                }
                if ((m2.HasValue && d4.HasValue) && GameParameters.IsInDangerousZone(m2.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d4.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender42Marker2 = true;
                }
                if ((m3.HasValue && d4.HasValue) && GameParameters.IsInDangerousZone(m3.Value, false, margin, out dist, out DistFromBorder) && GameParameters.IsInDangerousZone(d4.Value, false, margin, out dist, out DistFromBorder))
                {
                    SwitchDefender42Marker3 = true;
                }
            }

            res.Add(TempGoali);
            GoalieInfo = TempGoali;
            CurrentInfos = res;
            CurrentStates = new Dictionary<RoleBase, int>();

            return res;
        }
        public static Dictionary<int, Position2D> OverlapSolvingOnlinRoles(Position2D zeroKeyPos, Position2D firstKeyPos)
        {
            Dictionary<int, Position2D> ret = new Dictionary<int, Position2D>();
            double robotRedius = RobotParameters.OurRobotParams.Diameter / 2;
            Circle c1 = new Circle(zeroKeyPos, robotRedius);
            Circle c2 = new Circle(firstKeyPos, robotRedius);

            if (c1.Intersect(c2).Count < 0)
            {
                ret.Add(0, zeroKeyPos);
                ret.Add(1, firstKeyPos);
            }
            else
            {
                Position2D middlePos = Position2D.Interpolate(zeroKeyPos, firstKeyPos, 0.5);
                Vector2D extend = (firstKeyPos - zeroKeyPos).GetNormalizeToCopy(robotRedius + 0.01);
                ret.Add(0, middlePos + extend);
                ret.Add(1, middlePos - extend);
                DrawingObjects.AddObject(middlePos);
                DrawingObjects.AddObject(new Circle(ret[0], robotRedius, new Pen(Color.Red, 0.01f)));
                DrawingObjects.AddObject(new Circle(ret[1], robotRedius, new Pen(Color.Red, 0.01f)));

                // create vecor and add to positions and add positions into ret dict
            }
            return ret;
        }
        public static List<DefenceInfo> OverLapSolving(WorldModel Model, List<DefenceInfo> infos, Dictionary<Type, bool> FirstCornertoRemove, Dictionary<Type, bool> MarkerstoRemove, Dictionary<Type, bool> OtherstoRemove, bool OverLapIFirstDefender, bool OverLapIMarkerDefender, bool OverLapOthers)
        {
            List<DefenceInfo> res = new List<DefenceInfo>();
            List<Forbiden> forbidens = new List<Forbiden>();
            List<DefenceInfo> markers = new List<DefenceInfo>();
            DefenceInfo normalfirst = null;
            List<DefenceInfo> others = new List<DefenceInfo>();
            if (OverLapIFirstDefender)
                normalfirst = infos.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IFirstDefender))).FirstOrDefault();
            if (OverLapIMarkerDefender)
                markers = infos.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IMarkerDefender))).ToList();
            if (OverLapOthers)
                others = infos.Where(w => w.RoleType.GetInterfaces().Any(a => a != typeof(IFirstDefender)) && w.RoleType.GetInterfaces().Any(a => a != typeof(IMarkerDefender))).ToList();
            double tresh = 0.02;
            List<bool> useMarkersInOverlap = new List<bool>();
            foreach (var item in markers)
            {
                if (!MarkerstoRemove.ContainsKey(item.RoleType) || !MarkerstoRemove[item.RoleType])
                    useMarkersInOverlap.Add(true);
                else
                    useMarkersInOverlap.Add(false);
            }
            List<bool> useOthersInOverlap = new List<bool>();
            foreach (var item in others)
            {
                if (!OtherstoRemove.ContainsKey(item.RoleType) || !OtherstoRemove[item.RoleType])
                    useOthersInOverlap.Add(true);
                else
                    useOthersInOverlap.Add(false);
            }
            double robotRadius = RobotParameters.OurRobotParams.Diameter / 2;
            if (normalfirst != null && (!FirstCornertoRemove.ContainsKey(normalfirst.RoleType) || !FirstCornertoRemove[normalfirst.RoleType]))
                forbidens.Add(new Forbiden()
                {
                    center = normalfirst.DefenderPosition.Value,
                    radius = robotRadius
                });

            UpdateForbidens(Model, ref forbidens, markers, useMarkersInOverlap, tresh, robotRadius);
            UpdateForbidens(Model, ref forbidens, others, useOthersInOverlap, tresh, robotRadius);

            if (normalfirst != null)
                res.Add(normalfirst);

            res.AddRange(markers);
            res.AddRange(others);

            res.ForEach(p => p.Teta = (p.TargetState.Location - p.DefenderPosition.Value).AngleInDegrees);
            return res;

        }
        private static void UpdateForbidens(WorldModel Model, ref List<Forbiden> forbidens, List<DefenceInfo> defenders, List<bool> useInUpdate, double tresh, double robotRadius)
        {
            for (int i = 0; i < defenders.Count; i++)
            {
                foreach (var item in forbidens)
                {
                    bool overLap = false;
                    if (useInUpdate[i] && defenders[i].DefenderPosition.Value.DistanceFrom(item.center) < robotRadius + item.radius + tresh)
                    {
                        overLap = true;
                        Vector2D vec = defenders[i].DefenderPosition.Value - item.center, vec2;
                        if (vec.Size == 0)
                        {
                            vec = defenders[i].TargetState.Location - defenders[i].DefenderPosition.Value;
                            if (defenders[i].DefenderPosition.Value.Y >= 0)
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI / 2, 1);
                            else
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, 1);
                            defenders[i].DefenderPosition = item.center + vec2.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                        }
                        else
                            defenders[i].DefenderPosition = item.center + vec.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                    }
                    double dist, DistFromBorder;
                    if (overLap && GameParameters.IsInDangerousZone(defenders[i].DefenderPosition.Value, false, 0, out dist, out DistFromBorder))// GameParameters.SafeRadi(defenders[i].TargetState, 0)) ;// (RobotParameters.OurRobotParams.Diameter / 2) + .9)
                    {
                        Vector2D robotourgoal = (item.center - GameParameters.OurGoalCenter).GetNormalizeToCopy(item.center.DistanceFrom(GameParameters.OurGoalCenter));
                        Position2D extendpoint = item.center;
                        Vector2D robotitem = Vector2D.FromAngleSize((-robotourgoal).AngleInRadians + (Math.PI / 2), item.radius + robotRadius + .02);//(extendpoint -/* item.center*/ defenders[i].DefenderPosition.Value).GetNormalizeToCopy((item.radius + robotRadius + .02));

                        Position2D extendpoint2 = extendpoint + robotitem;
                        Position2D extendpoint3 = extendpoint - robotitem;
                        if (extendpoint3.DistanceFrom(defenders[i].DefenderPosition.Value) < extendpoint2.DistanceFrom(defenders[i].DefenderPosition.Value))
                        {
                            extendpoint2 = extendpoint3;
                        }

                        if (extendpoint2.X > 3.15 || extendpoint2.DistanceFrom(GameParameters.OurGoalCenter) < item.center.DistanceFrom(GameParameters.OurGoalCenter))
                        {
                            if (extendpoint2 == extendpoint3)
                                extendpoint2 = extendpoint + robotitem;
                            else
                                extendpoint2 = extendpoint - robotitem;
                        }
                        Vector2D reverse = GameParameters.InRefrence(extendpoint2 - extendpoint, robotourgoal);
                        if (!BallIsMoved && Model.Status != GameStatus.Normal)
                        {
                            if (extendpoint2.DistanceFrom(ballState.Location) < .6)
                            {
                                Vector2D TargetBall = extendpoint2 - ballState.Location;
                                extendpoint2 = ballState.Location + TargetBall.GetNormalizeToCopy(.7);
                            }
                        }
                        defenders[i].DefenderPosition = extendpoint2;
                    }
                }
                Forbiden temp = new Forbiden()
                {
                    center = defenders[i].DefenderPosition.Value,
                    radius = robotRadius
                };

                foreach (var item in forbidens.ToList())
                {
                    if (temp.center.DistanceFrom(item.center) < (item.radius + temp.radius + robotRadius * 2))
                    {
                        temp = Merge(item, temp);
                        forbidens.Remove(item);
                    }
                }
                // DrawingObjects.AddObject(new Circle(temp.center, temp.radius, new Pen(Color.Red, .02f)));
                forbidens.Add(temp);
            }
        }
        private static void UpdateForbidens(WorldModel Model, ref List<Forbiden> forbidens, List<DefenceInfo> defenders, double tresh, double robotRadius)
        {
            for (int i = 0; i < defenders.Count; i++)
            {
                foreach (var item in forbidens)
                {
                    bool overLap = false;
                    if (defenders[i].DefenderPosition.Value.DistanceFrom(item.center) < robotRadius + item.radius + tresh)
                    {
                        overLap = true;
                        Vector2D vec = defenders[i].DefenderPosition.Value - item.center, vec2;
                        if (vec.Size == 0)
                        {
                            vec = defenders[i].TargetState.Location - defenders[i].DefenderPosition.Value;
                            if (defenders[i].DefenderPosition.Value.Y >= 0)
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI / 2, 1);
                            else
                                vec2 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, 1);
                            defenders[i].DefenderPosition = item.center + vec2.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                        }
                        else
                            defenders[i].DefenderPosition = item.center + vec.GetNormalizeToCopy(robotRadius + item.radius + tresh);
                    }
                    double dist, DistFromBorder;
                    if (overLap && GameParameters.IsInDangerousZone(defenders[i].DefenderPosition.Value, false, 0.1, out dist, out DistFromBorder))// GameParameters.SafeRadi(defenders[i].TargetState, 0)) ;// (RobotParameters.OurRobotParams.Diameter / 2) + .9)
                    {
                        Vector2D robotourgoal = (item.center - GameParameters.OurGoalCenter).GetNormalizeToCopy(item.center.DistanceFrom(GameParameters.OurGoalCenter));
                        Position2D extendpoint = item.center;
                        Vector2D robotitem = Vector2D.FromAngleSize((-robotourgoal).AngleInRadians + (Math.PI / 2), item.radius + robotRadius + .02);//(extendpoint -/* item.center*/ defenders[i].DefenderPosition.Value).GetNormalizeToCopy((item.radius + robotRadius + .02));

                        Position2D extendpoint2 = extendpoint + robotitem;
                        Position2D extendpoint3 = extendpoint - robotitem;
                        if (extendpoint3.DistanceFrom(defenders[i].DefenderPosition.Value) < extendpoint2.DistanceFrom(defenders[i].DefenderPosition.Value))
                        {
                            extendpoint2 = extendpoint3;
                        }

                        if (extendpoint2.X > 3.15 || extendpoint2.DistanceFrom(GameParameters.OurGoalCenter) < item.center.DistanceFrom(GameParameters.OurGoalCenter))
                        {
                            if (extendpoint2 == extendpoint3)
                                extendpoint2 = extendpoint + robotitem;
                            else
                                extendpoint2 = extendpoint - robotitem;
                        }
                        Vector2D reverse = GameParameters.InRefrence(extendpoint2 - extendpoint, robotourgoal);
                        if (!BallIsMoved && Model.Status != GameStatus.Normal)
                        {
                            if (extendpoint2.DistanceFrom(ballState.Location) < .68)
                            {
                                Vector2D TargetBall = extendpoint2 - ballState.Location;
                                extendpoint2 = ballState.Location + TargetBall.GetNormalizeToCopy(.7);
                            }
                        }
                        defenders[i].DefenderPosition = extendpoint2;
                    }
                }
                Forbiden temp = new Forbiden()
                {
                    center = defenders[i].DefenderPosition.Value,
                    radius = robotRadius
                };

                foreach (var item in forbidens.ToList())
                {
                    if (temp.center.DistanceFrom(item.center) < (item.radius + temp.radius + robotRadius * 2))
                    {
                        temp = Merge(item, temp);
                        forbidens.Remove(item);
                    }
                }
                forbidens.Add(temp);
            }
        }
        private static Forbiden Merge(Forbiden F1, Forbiden F2)
        {
            Forbiden res = new Forbiden();
            res.radius = (F1.center.DistanceFrom(F2.center) + F1.radius + F2.radius) / 2;
            double dx = res.radius - F1.radius;
            Vector2D vec = F2.center - F1.center;
            res.center = F1.center + vec.GetNormalizeToCopy(dx);
            return res;
        }

        #region NormalDefender
        class FirstBounds
        {
            public double minGoaliDist = 0.4;
            public double maxGoaliDist = 0.85;
            public double minGoalidx = 0.11;
            public double mindefX = 1.1;
            public double maxdefX = 1.6;
            public double margin = 0;
            public double prepAng;
            public Position2D FirstGoalCorner = new Position2D();
            public Position2D SecondGoalCorner = new Position2D();
            public Position2D TargetPos = new Position2D();
            public double GoaliX = 0;
            public double GoaliDist = 0;
            public double DefenderDist = 0;
            public Line FirstGoalLine = new Line();
            public Line SecondGoalLine = new Line();
        }
        private static GoaliPositioningMode ChooseMode(SingleObjectState TargetState, GoaliPositioningMode currentMode)
        {
            GoaliPositioningMode temp = currentMode;
            if (TargetState.Location.Y < -0.2)
                temp = GoaliPositioningMode.InLeftSide;
            else if (TargetState.Location.Y > 0.2)
                temp = GoaliPositioningMode.InRightSide;
            return temp;
        }
        private static FirstBounds CalculateBounds(WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode)
        {
            FirstBounds fb = new FirstBounds();
            fb.mindefX = GameParameters.SafeRadi(TargetState, FreekickDefence.AdditionalSafeRadi);
            fb.maxGoaliDist = 0.6;
            if (Mode == GoaliPositioningMode.InRightSide)
            {
                fb.margin = -0.02;
                fb.prepAng = -Math.PI / 2;
                fb.FirstGoalCorner = GameParameters.OurGoalRight;
                fb.SecondGoalCorner = GameParameters.OurGoalLeft;
            }
            else
            {
                fb.margin = 0.02;
                fb.prepAng = Math.PI / 2;
                fb.FirstGoalCorner = GameParameters.OurGoalLeft;
                fb.SecondGoalCorner = GameParameters.OurGoalRight;
            }
            fb.TargetPos = new Position2D(Math.Min(TargetState.Location.X, GameParameters.OurGoalCenter.X - 0.12), Math.Sign(TargetState.Location.Y) * Math.Min(Math.Abs(TargetState.Location.Y), GameParameters.OurLeftCorner.Y + 0.3));
            //if (TargetState.Location.X < -GameParameters.OurGoalCenter.X / 3)
            //    maxdefX = 2.5;
            if (fb.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) <= fb.mindefX)
            {
                fb.mindefX = Math.Max(fb.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) + 0.1, fb.mindefX);
                fb.maxdefX = Math.Max(fb.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) + 0.1, fb.mindefX);
            }
            double db = fb.TargetPos.DistanceFrom(GameParameters.OurGoalCenter);
            double dbx = Math.Abs(fb.TargetPos.X - GameParameters.OurGoalCenter.X);

            db = db / (GameParameters.OurGoalCenter.DistanceFrom(new Position2D(0, 0) + new Vector2D(-1.5, 0)));
            db = (db > 1) ? 1 : db;

            dbx = dbx / (GameParameters.OurGoalCenter.X + 1.5);
            dbx = (dbx > 1) ? 1 : dbx;

            fb.GoaliDist = fb.minGoaliDist + db * (fb.maxGoaliDist - fb.minGoaliDist);
            fb.GoaliX = fb.minGoalidx + dbx * (fb.maxGoaliDist - fb.minGoalidx);

            double ddb = fb.TargetPos.DistanceFrom(GameParameters.OurGoalCenter);
            ddb -= (GameParameters.SafeRadi(TargetState, 0) + 0.4);/////////////
            ddb = ddb / (GameParameters.OurGoalCenter.DistanceFrom(new Position2D(0, 0) + new Vector2D(-2, 0)));
            ddb = (ddb > 1) ? 1 : ddb;
            ddb = (ddb < 0) ? 0 : ddb;
            fb.DefenderDist = fb.mindefX + ddb * (fb.maxdefX - fb.mindefX);
            fb.FirstGoalLine = new Line(fb.TargetPos, new Position2D(fb.FirstGoalCorner.X, fb.FirstGoalCorner.Y + fb.margin));
            fb.SecondGoalLine = new Line(fb.TargetPos, new Position2D(fb.SecondGoalCorner.X, fb.SecondGoalCorner.Y - fb.margin));
            return fb;
        }
        private static Position2D CalculateGoaliPos(WorldModel Model, SingleObjectState TargetState, FirstBounds fb, out bool isOnGoalLine, out Line DefendTargetGoaliLine)
        {
            Position2D GoaliPos;
            Circle goaliBands = new Circle(GameParameters.OurGoalCenter, fb.GoaliDist);
            Vector2D FGoalVec = fb.FirstGoalLine.Head - fb.FirstGoalLine.Tail;
            isOnGoalLine = false;
            Vector2D tmpVec = Vector2D.FromAngleSize(FGoalVec.AngleInRadians + fb.prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            DefendTargetGoaliLine = new Line(fb.FirstGoalLine.Head + tmpVec, (fb.FirstGoalLine.Head + tmpVec) - FGoalVec);
            List<Position2D> Intersects;
            Intersects = goaliBands.Intersect(DefendTargetGoaliLine);
            if (Intersects.Count == 0)
            {
                Line l = new Line(fb.FirstGoalCorner, goaliBands.Center);
                List<Position2D> tmpInts = goaliBands.Intersect(l);
                GoaliPos = (Math.Sign(tmpInts[0].Y) == Math.Sign(fb.FirstGoalCorner.Y)) ? tmpInts[0] : tmpInts[1];
            }
            else if (Intersects.Count == 1)
                GoaliPos = Intersects[0];
            else
                GoaliPos = (Intersects[0].X < Intersects[1].X) ? Intersects[0] : Intersects[1];
            if (GameParameters.OurGoalCenter.X - GoaliPos.X > fb.GoaliX)
            {
                Line tmpLine = new Line(new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, -1), new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, 1));
                Position2D? tmpp = tmpLine.IntersectWithLine(DefendTargetGoaliLine);
                GoaliPos = tmpp.Value;
                isOnGoalLine = true;
            }

            return GoaliPos;
        }
        private static Position2D? CalculateDefenderPos(WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode, Position2D GoaliPos, FirstBounds fb, out Line DefenderBoundLine1, out Line SecondGoalParallelLine)
        {
            Circle cGoali = new Circle(GoaliPos, RobotParameters.OurRobotParams.Diameter / 2);

            Vector2D FirstGoalvec = fb.FirstGoalLine.Tail - fb.FirstGoalLine.Head;
            Vector2D GoaliDefendTargetVec = cGoali.Center - fb.TargetPos;
            double angle = Math.Abs(Vector2D.AngleBetweenInRadians(FirstGoalvec, GoaliDefendTargetVec));

            Vector2D tmpVec;
            if (Mode == GoaliPositioningMode.InRightSide)
                tmpVec = Vector2D.FromAngleSize(GoaliDefendTargetVec.AngleInRadians + angle, 1);
            else
                tmpVec = Vector2D.FromAngleSize(GoaliDefendTargetVec.AngleInRadians - angle, 1);

            DefenderBoundLine1 = new Line(fb.TargetPos, fb.TargetPos + tmpVec);
            Vector2D DefenderBoundVec1 = DefenderBoundLine1.Head - DefenderBoundLine1.Tail;
            Vector2D tmpV = Vector2D.FromAngleSize(DefenderBoundVec1.AngleInRadians + fb.prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            Line tmpL = new Line(DefenderBoundLine1.Head + tmpV, (DefenderBoundLine1.Head + tmpV) - DefenderBoundVec1);
            Vector2D SecondGoalVec = fb.SecondGoalLine.Head - fb.SecondGoalLine.Tail;
            Vector2D tmpV2 = Vector2D.FromAngleSize(SecondGoalVec.AngleInRadians - fb.prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            SecondGoalParallelLine = new Line(fb.SecondGoalLine.Head + tmpV2, (fb.SecondGoalLine.Head + tmpV2) - SecondGoalVec);

            Position2D? DefenderPos = null;
            Line DefenderLine = new Line();
            Circle DefenderBound = new Circle(GameParameters.OurGoalCenter, fb.DefenderDist);
            if ((Mode == GoaliPositioningMode.InRightSide && (-DefenderBoundVec1).AngleInDegrees >= (-SecondGoalVec).AngleInDegrees) || (Mode == GoaliPositioningMode.InLeftSide && (-DefenderBoundVec1).AngleInDegrees <= (-SecondGoalVec).AngleInDegrees))
            {
                List<Position2D> tmpInts = DefenderBound.Intersect(fb.SecondGoalLine);
                if (tmpInts.Count > 1)
                {
                    if (Math.Sign(tmpInts[0].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(tmpInts[1].Y) == Math.Sign(fb.TargetPos.Y))
                        DefenderPos = (tmpInts[0].X < tmpInts[1].X) ? tmpInts[0] : tmpInts[1];
                    else if (Math.Sign(tmpInts[0].Y) == Math.Sign(fb.TargetPos.Y))
                        DefenderPos = tmpInts[0];
                    else if (Math.Sign(tmpInts[1].Y) == Math.Sign(fb.TargetPos.Y))
                        DefenderPos = tmpInts[1];
                    else
                        DefenderPos = (tmpInts[0].X < tmpInts[1].X) ? tmpInts[0] : tmpInts[1];
                }
                else if (tmpInts.Count == 1)
                    DefenderPos = tmpInts[0];
            }
            else
                DefenderPos = tmpL.IntersectWithLine(SecondGoalParallelLine);
            return DefenderPos;
        }
        private static List<Position2D> ReCalculatePositions(WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode, Position2D GoaliPos, FirstBounds fb, Position2D? DefenderPos, Line DefenderBoundLine1, bool isOnGoalLine, Line DefendTargetGoalLine, Line SecondGoalParallelLine)
        {
            Line DefenderLine = new Line();
            Circle DefenderBound = new Circle(GameParameters.OurGoalCenter, fb.DefenderDist);
            Circle GoaliBound = new Circle(GameParameters.OurGoalCenter, fb.GoaliDist);

            if (DefenderPos.HasValue)
            {
                if (DefenderPos.Value.DistanceFrom(fb.TargetPos) > 0.05)
                {
                    DefenderLine = new Line(fb.TargetPos, DefenderPos.Value);
                    if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) > fb.maxdefX && DefenderPos.Value.X < GameParameters.OurGoalCenter.X)
                    {
                        Circle DefenderBound2 = new Circle(GameParameters.OurGoalCenter, fb.maxdefX);
                        List<Position2D> possd = DefenderBound2.Intersect(DefenderLine);
                        Position2D tmpDefenderPos = new Position2D();
                        if (possd.Count > 1)
                        {
                            if (Math.Sign(possd[0].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(possd[1].Y) != Math.Sign(fb.TargetPos.Y))
                                tmpDefenderPos = possd[0];
                            else if (Math.Sign(possd[1].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(possd[0].Y) != Math.Sign(fb.TargetPos.Y))
                                tmpDefenderPos = possd[1];
                            else
                                tmpDefenderPos = (possd[0].X < possd[1].X) ? possd[0] : possd[1];
                        }
                        else if (possd.Count == 1)
                            tmpDefenderPos = possd[0];
                        Circle DefenderCircle = new Circle(tmpDefenderPos, RobotParameters.OurRobotParams.Diameter / 2);
                        List<Line> tngDefL;
                        List<Position2D> tngDefP;
                        int tangCount = DefenderCircle.GetTangent(fb.TargetPos, out tngDefL, out tngDefP);
                        double tngAng;
                        double vAng;
                        if (tangCount < 2)
                            return new List<Position2D>() { GoaliPos, tmpDefenderPos };
                        tngAng = Math.Abs(Vector2D.AngleBetweenInRadians(tngDefP[0] - fb.TargetPos, tngDefP[1] - fb.TargetPos));
                        vAng = Math.Abs(Vector2D.AngleBetweenInRadians(fb.SecondGoalLine.Tail - fb.SecondGoalLine.Head, DefenderBoundLine1.Tail - DefenderBoundLine1.Head));
                        double errAng = (vAng - tngAng) / 3;
                        Vector2D vec;
                        if (Mode == GoaliPositioningMode.InRightSide)
                            vec = Vector2D.FromAngleSize((tmpDefenderPos - fb.TargetPos).AngleInRadians + errAng / 2, 1);
                        else
                            vec = Vector2D.FromAngleSize((tmpDefenderPos - fb.TargetPos).AngleInRadians - errAng / 2, 1);

                        Line tmpLine = new Line(fb.TargetPos, fb.TargetPos + vec);
                        List<Position2D> ppp = DefenderBound2.Intersect(tmpLine);
                        if (ppp.Count > 1)
                        {
                            if (Math.Sign(ppp[0].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(ppp[1].Y) != Math.Sign(fb.TargetPos.Y))
                                DefenderPos = ppp[0];
                            else if (Math.Sign(ppp[1].Y) == Math.Sign(fb.TargetPos.Y) && Math.Sign(ppp[0].Y) != Math.Sign(fb.TargetPos.Y))
                                DefenderPos = ppp[1];
                            else
                                DefenderPos = (ppp[0].X < ppp[1].X) ? ppp[0] : ppp[1];
                        }
                        else if (ppp.Count == 1)
                            DefenderPos = ppp[0];
                        Vector2D vec2;
                        if (Mode == GoaliPositioningMode.InRightSide)
                            vec2 = Vector2D.FromAngleSize((GoaliPos - fb.TargetPos).AngleInRadians + errAng, 1);
                        else
                            vec2 = Vector2D.FromAngleSize((GoaliPos - fb.TargetPos).AngleInRadians - errAng, 1);
                        Line NewGoaliLine = new Line(fb.TargetPos, fb.TargetPos + vec2);

                        if (isOnGoalLine)
                        {
                            Line tmpLine2 = new Line(new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, -1), new Position2D(GameParameters.OurGoalCenter.X - fb.GoaliX, 1));
                            Position2D? tmpp = tmpLine2.IntersectWithLine(NewGoaliLine);
                            GoaliPos = tmpp.Value;
                        }
                        else
                        {
                            List<Position2D> ppp2 = GoaliBound.Intersect(NewGoaliLine);
                            if (ppp2.Count > 1)
                                GoaliPos = (ppp2[0].X < ppp2[1].X) ? ppp2[0] : ppp2[1];
                            else if (ppp2.Count == 1)
                                GoaliPos = ppp2[0];
                        }
                    }
                    else if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) < fb.mindefX || (DefenderPos.Value.X > GameParameters.OurGoalCenter.X))
                    {

                        List<Position2D> possd = DefenderBound.Intersect(DefenderLine);
                        Position2D pdef = new Position2D();
                        if (possd.Count > 1)
                        {
                            //if (Math.Sign(possd[0].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[1].Y) != Math.Sign(TargetPos.Y))
                            //    pdef = possd[0];
                            //else if (Math.Sign(possd[1].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[0].Y) != Math.Sign(TargetPos.Y))
                            //    pdef = possd[1];
                            //else
                            //if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) < mindefX)
                            //    pdef = DefenderPos.Value;
                            //if(DefenderPos.Value.X > GameParameters.OurGoalCenter.X)
                            pdef = (possd[0].X < possd[1].X) ? possd[0] : possd[1];

                        }
                        //pdef = (possd[0].X < possd[1].X) ? possd[0] : possd[1];
                        else if (possd.Count == 1)
                            pdef = possd[0];
                        DefenderPos = pdef;
                    }
                }
                List<Position2D> tangposes;
                List<Line> tanglines;
                new Circle(DefenderPos.Value, RobotParameters.OurRobotParams.Diameter / 2).GetTangent(fb.TargetPos, out tanglines, out tangposes);
                if (tangposes.Count > 1)
                {
                    Position2D selectedtng;
                    Position2D Otng;
                    Line selectedtngLine, OtngLine;
                    if (tangposes[0].Y >= tangposes[0].Y)
                    {
                        selectedtng = tangposes[0];
                        Otng = tangposes[1];
                        selectedtngLine = tanglines[0];
                        OtngLine = tanglines[1];
                    }
                    else
                    {
                        selectedtng = tangposes[1];
                        Otng = tangposes[0];
                        selectedtngLine = tanglines[1];
                        OtngLine = tanglines[0];
                    }

                    if (((selectedtng - fb.TargetPos).AngleInDegrees >= (GameParameters.OurGoalLeft - fb.TargetPos).AngleInDegrees) && ((Otng - fb.TargetPos).AngleInDegrees <= (GameParameters.OurGoalRight - fb.TargetPos).AngleInDegrees))
                    {
                        //Line ll = new Line(fb.TargetPos, GoaliPos);
                        //Line goal = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                        //Position2D? intersect = ll.IntersectWithLine(goal);
                        //if (intersect.HasValue)
                        //{
                        //    GoaliPos = GoaliPos + (GoaliPos - fb.TargetPos).GetNormalizeToCopy(0.4);
                        //    if (GoaliPos.X > GameParameters.OurGoalCenter.X - 0.2)
                        //    {
                        //        GoaliPos = intersect.Value + (fb.TargetPos - intersect.Value).GetNormalizeToCopy(0.2);
                        //    }
                        //}
                        //if (Math.Abs(GoaliPos.Y) > 0.25)
                        //    GoaliPos = (GameParameters.OurGoalCenter + new Vector2D(0, -0.2)) + (fb.TargetPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.15);

                        Vector2D pvec = selectedtng - fb.FirstGoalCorner;
                        GoaliPos = fb.FirstGoalCorner + pvec.GetNormalizeToCopy(0.4);

                    }
                }
                if (fb.TargetPos.DistanceFrom(GameParameters.OurGoalCenter) < DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter))
                {
                    Position2D? tp = DefendTargetGoalLine.IntersectWithLine(SecondGoalParallelLine);
                    if (tp.HasValue)
                    {
                        if (tp.Value.X > GameParameters.OurGoalCenter.X - fb.minGoalidx)
                        {
                            Vector2D tmpGv = GameParameters.OurGoalCenter - fb.TargetPos;
                            tp = fb.TargetPos + tmpGv.GetNormalizeToCopy(0.25);
                        }
                        GoaliPos = tp.Value;
                    }
                }
                return new List<Position2D>() { GoaliPos, DefenderPos.Value };
            }
            return new List<Position2D>() { GoaliPos, new Position2D() };
        }
        public static Position2D? CalculatePos(GameStrategyEngine engine, WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode, out Position2D? GoaliPos)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Line DefenderBoundLine1, DefendTargetGoalLine, SecondGoalParallelLine;
            if (TargetState != null)
            {
                bool isOnGoalLine;
                FirstBounds fb = CalculateBounds(Model, TargetState, Mode);
                Position2D tmpGoaliPos = CalculateGoaliPos(Model, TargetState, fb, out isOnGoalLine, out DefendTargetGoalLine);
                Position2D? tmpDefenderPos = CalculateDefenderPos(Model, TargetState, Mode, tmpGoaliPos, fb, out DefenderBoundLine1, out SecondGoalParallelLine);
                List<Position2D> Positions = ReCalculatePositions(Model, TargetState, Mode, tmpGoaliPos, fb, tmpDefenderPos, DefenderBoundLine1, isOnGoalLine, DefendTargetGoalLine, SecondGoalParallelLine);

                Positions[1] = CommonDefenceUtils.CheckForStopZone(BallIsMoved, Positions[1], Model);
                if (Positions[0].X > GameParameters.OurGoalCenter.X - fb.minGoalidx)
                    Positions[0] = new Position2D(GameParameters.OurGoalCenter.X - fb.minGoalidx, Positions[0].Y);
                GoaliPos = Positions[0];
                return Positions[1];
            }
            GoaliPos = null;
            return null;

        }
        #endregion

        #region 1st Defender
        private static DefenceInfo CalculateNormalFirst(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, out DefenceInfo GoaliRes, DefenderCommand GoaliCommand, List<DefenceInfo> CurrentInfo)
        {
            DefenceInfo def = new DefenceInfo();
            GoaliRes = new DefenceInfo();

            Position2D? goalieball;
            Position2D? goalierobot;
            Position2D? g;
            ballState = Model.BallState;
            GoaliPositioningMode BallMode = ChooseMode(ballState, CurrentGoalieMode);
            Position2D? Defendball = CalculatePos(engine, Model, ballState, BallMode, out goalieball);
            if (ballState.Speed.Size < 0.5 || !Command.OppID.HasValue || !Model.Opponents.ContainsKey(Command.OppID.Value))
            {
                GoaliRes.OppID = (Command.OppID.HasValue && Model.Opponents.ContainsKey(Command.OppID.Value)) ? Command.OppID : null;
                if ((Command.OppID.HasValue && Model.Opponents.ContainsKey(Command.OppID.Value)))
                    GoaliRes.OppID = Command.OppID;
                else
                    GoaliRes.OppID = null;
                GoaliRes.RoleType = GoaliCommand.RoleType;
                GoaliRes.TargetState = ballState;
                GoaliRes.Mode = BallMode;
                GoaliRes.DefenderPosition = goalieball;

                if ((Command.OppID.HasValue && Model.Opponents.ContainsKey(Command.OppID.Value)))
                    def.OppID = Command.OppID;
                else
                    def.OppID = null;
                def.RoleType = Command.RoleType;
                def.TargetState = ballState;
                def.Mode = BallMode;
                def.DefenderPosition = Defendball;
                DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "6854654645");
            }
            else
            {
                SingleObjectState oppstate = Model.Opponents[Command.OppID.Value];
                GoaliPositioningMode RobotMode = ChooseMode(oppstate, CurrentGoalieMode);
                Position2D? Defendrobot = CalculatePos(engine, Model, oppstate, RobotMode, out goalierobot);

                Position2D temprobot = oppstate.Location + oppstate.Speed * 0.5;
                temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
                temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
                temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
                temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);

                SingleObjectState nextrobot = new SingleObjectState()
                {
                    Type = ObjectType.Opponent,
                    Location = temprobot,
                    Speed = oppstate.Speed
                };
                GoaliPositioningMode nextRobotMode = ChooseMode(nextrobot, CurrentGoalieMode);
                Position2D? Defendrobotnext = CalculatePos(engine, Model, nextrobot, nextRobotMode, out g);

                Position2D tempball = ballState.Location + ballState.Speed * 0.5;
                tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
                tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
                tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
                tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);
                SingleObjectState nextball = new SingleObjectState()
                {
                    Type = ObjectType.Ball,
                    Location = tempball,
                    Speed = ballState.Speed
                };
                GoaliPositioningMode nextBallMode = ChooseMode(nextball, CurrentGoalieMode);
                Position2D? Defendballnext = CalculatePos(engine, Model, nextball, nextBallMode, out g);

                if (Defendrobot.HasValue && Defendball.HasValue)
                {

                    double db = Defendball.Value.DistanceFrom(Defendballnext.Value);
                    double dr = Defendrobot.Value.DistanceFrom(Defendrobotnext.Value);
                    if (db > dr + 0.065)
                    {
                        GoaliRes.OppID = Command.OppID;
                        GoaliRes.RoleType = GoaliCommand.RoleType;
                        GoaliRes.TargetState = oppstate;
                        GoaliRes.Mode = RobotMode;
                        GoaliRes.DefenderPosition = goalierobot;

                        def.OppID = Command.OppID;
                        def.RoleType = Command.RoleType;
                        def.TargetState = oppstate;
                        def.Mode = RobotMode;
                        def.DefenderPosition = Defendrobot;
                        DrawingObjects.AddObject(new StringDraw("RobotMode", new Position2D(-1, -1)), "564651321");
                        DrawingObjects.AddObject(new StringDraw("OppID: " + Command.OppID.Value.ToString(), new Position2D(-.8, -1)), "9873164");
                    }
                    else if (dr > db + 0.065)
                    {
                        GoaliRes.OppID = Command.OppID;
                        GoaliRes.RoleType = GoaliCommand.RoleType;
                        GoaliRes.TargetState = ballState;
                        GoaliRes.Mode = BallMode;
                        GoaliRes.DefenderPosition = goalieball;

                        def.OppID = Command.OppID;
                        def.RoleType = Command.RoleType;
                        def.TargetState = ballState;
                        def.Mode = BallMode;
                        def.DefenderPosition = Defendball;
                        DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "5464465");
                    }
                    else
                    {
                        if (CurrentInfo != null && CurrentInfo.Any(a => a.RoleType == Command.RoleType))
                        {
                            DefenceInfo dinfo = CurrentInfo.Single(s => s.RoleType == Command.RoleType);

                            if (dinfo.TargetState.Type == ObjectType.Ball)
                            {
                                GoaliRes.OppID = Command.OppID;
                                GoaliRes.RoleType = GoaliCommand.RoleType;
                                GoaliRes.TargetState = ballState;
                                GoaliRes.Mode = BallMode;
                                GoaliRes.DefenderPosition = goalieball;

                                def.OppID = Command.OppID;
                                def.RoleType = Command.RoleType;
                                def.TargetState = ballState;
                                def.Mode = BallMode;
                                def.DefenderPosition = Defendball;
                                DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "546465465");
                            }
                            else
                            {
                                GoaliRes.OppID = Command.OppID;
                                GoaliRes.RoleType = GoaliCommand.RoleType;
                                GoaliRes.TargetState = oppstate;
                                GoaliRes.Mode = RobotMode;
                                GoaliRes.DefenderPosition = goalierobot;

                                def.OppID = Command.OppID;
                                def.RoleType = Command.RoleType;
                                def.TargetState = oppstate;
                                def.Mode = RobotMode;
                                def.DefenderPosition = Defendrobot;
                                DrawingObjects.AddObject(new StringDraw("RobotMode", new Position2D(-1, -1)), "8765456498");
                                DrawingObjects.AddObject(new StringDraw("OppID: " + Command.OppID.Value.ToString(), new Position2D(-.8, -1)), "5165464");
                            }
                        }
                        else
                        {
                            GoaliRes.OppID = Command.OppID;
                            GoaliRes.RoleType = GoaliCommand.RoleType;
                            GoaliRes.TargetState = ballState;
                            GoaliRes.Mode = BallMode;
                            GoaliRes.DefenderPosition = goalieball;

                            def.OppID = Command.OppID;
                            def.RoleType = Command.RoleType;
                            def.TargetState = ballState;
                            def.Mode = BallMode;
                            def.DefenderPosition = Defendball;
                            DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "8756123645");
                        }
                    }
                }
                else if (Defendrobot.HasValue)
                {
                    GoaliRes.OppID = Command.OppID;
                    GoaliRes.RoleType = GoaliCommand.RoleType;
                    GoaliRes.TargetState = oppstate;
                    GoaliRes.Mode = RobotMode;
                    GoaliRes.DefenderPosition = goalierobot;

                    def.OppID = Command.OppID;
                    def.RoleType = Command.RoleType;
                    def.TargetState = oppstate;
                    def.Mode = RobotMode;
                    def.DefenderPosition = Defendrobot;
                    DrawingObjects.AddObject(new StringDraw("RobotMode", new Position2D(-1, -1)), "74654875");
                    DrawingObjects.AddObject(new StringDraw("OppID: " + Command.OppID.Value.ToString(), new Position2D(-.8, -1)), "8797546");
                }
                else
                {
                    GoaliRes.OppID = Command.OppID;
                    GoaliRes.RoleType = GoaliCommand.RoleType;
                    GoaliRes.TargetState = ballState;
                    GoaliRes.Mode = BallMode;
                    GoaliRes.DefenderPosition = goalieball;

                    def.OppID = Command.OppID;
                    def.RoleType = Command.RoleType;
                    def.TargetState = ballState;
                    def.Mode = BallMode;
                    def.DefenderPosition = Defendball;
                    DrawingObjects.AddObject(new StringDraw("BallMode", new Position2D(-1, -1)), "4956678");
                }
            }
            CurrentGoalieMode = GoaliRes.Mode;
            GoaliRes.Teta = (GoaliRes.TargetState.Location - GoaliRes.DefenderPosition.Value).AngleInDegrees;
            return def;
        }
        #endregion

        #region 2nd defender
        private static DefenceInfo CalculateNormalSecond(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, out DefenceInfo GoaliRes, List<DefenceInfo> CurrentInfo)
        {
            DefenceInfo def = new DefenceInfo();
            GoaliRes = new DefenceInfo();

            Position2D? goalieball;
            Position2D? goalierobot;
            ballState = Model.BallState;
            GoaliPositioningMode BallMode = ChooseMode(ballState, CurrentGoalieMode);
            Position2D? Defendball = CalculatePos(engine, Model, ballState, BallMode, out goalieball);
            if (!Command.OppID.HasValue || !Model.Opponents.ContainsKey(Command.OppID.Value))
            {
                GoaliRes.OppID = null;
                GoaliRes.TargetState = ballState;
                GoaliRes.Mode = BallMode;
                GoaliRes.DefenderPosition = goalieball;

                def.OppID = null;
                def.RoleType = Command.RoleType;
                def.TargetState = ballState;
                def.Mode = BallMode;
                def.DefenderPosition = Defendball;
            }
            else
            {
                SingleObjectState oppstate = Model.Opponents[Command.OppID.Value];
                GoaliPositioningMode RobotMode = ChooseMode(oppstate, CurrentGoalieMode);
                Position2D? Defendrobot = CalculatePos(engine, Model, oppstate, RobotMode, out goalierobot);

                GoaliRes.OppID = Command.OppID.Value;
                GoaliRes.TargetState = oppstate;
                GoaliRes.Mode = RobotMode;
                GoaliRes.DefenderPosition = goalierobot;

                def.OppID = Command.OppID.Value;
                def.RoleType = Command.RoleType;
                def.TargetState = oppstate;
                def.Mode = RobotMode;
                def.DefenderPosition = Defendrobot;
            }

            return def;
        }
        #endregion

        #region Marker
        private static DefenceInfo Mark(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, List<DefenceInfo> CurrentInfo)
        {

            DefenceInfo def = new DefenceInfo();
            int? oppid = Command.OppID;
            Position2D? target;
            def.RoleType = Command.RoleType;
            def.OppID = oppid;

            Position2D Target = Position2D.Zero;
            SingleObjectState state;
            if (oppid.HasValue && Model.Opponents.ContainsKey(oppid.Value))
                state = Model.Opponents[oppid.Value];
            else
                state = ballState;

            Vector2D oppSpeedVector = state.Speed;
            Vector2D oppOurGoalCenter = GameParameters.OurGoalCenter - state.Location;
            double innerpOppOurGoal = oppSpeedVector.InnerProduct(oppOurGoalCenter);



            double oppSpeed = state.Speed.Size;
            double minDist = GameParameters.SafeRadi(state, MarkerDefenceUtils.MinDistMarkMargin);

            Position2D minimum = GameParameters.OurGoalCenter + (state.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(minDist);
            Position2D maximum = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.2);
            Position2D posToGo = Position2D.Zero;



            double MarkFromDist = MarkerDefenceUtils.MarkFromDist;

            posToGo = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(MarkFromDist);

            if (minimum.DistanceFrom(GameParameters.OurGoalCenter) > posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = minimum;
            else if (maximum.DistanceFrom(GameParameters.OurGoalCenter) < posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = maximum;
            else
                Target = posToGo;


            maximum.DrawColor = System.Drawing.Color.Blue;
            minimum.DrawColor = System.Drawing.Color.Yellow;
            DrawingObjects.AddObject(Target);
            DrawingObjects.AddObject(minimum);
            DrawingObjects.AddObject(maximum);
            if (Command.MarkMaximumDist > 1)
            {
                Position2D maxpos = GameParameters.OurGoalCenter + (Target - GameParameters.OurGoalCenter).GetNormalizeToCopy(Command.MarkMaximumDist);
                if (GameParameters.OurGoalCenter.DistanceFrom(Target) > GameParameters.OurGoalCenter.DistanceFrom(maxpos))
                    //Target = maxpos;
                    Target.DrawColor = System.Drawing.Color.Crimson;
                DrawingObjects.AddObject(Target);
            }
            Target = CommonDefenceUtils.CheckForStopZone(BallIsMoved, Target, Model);

            if (Target.X > GameParameters.OurGoalCenter.X)
                Target.X = GameParameters.OurGoalCenter.X;
            if (Math.Abs(Target.Y) > Math.Abs(GameParameters.OurLeftCorner.Y))
                Target = new Line(Target, GameParameters.OurGoalCenter).CalculateX(Math.Abs(GameParameters.OurLeftCorner.Y) * Math.Sign(Target.Y));

            target = Target;

            def.TargetState = state;
            def.DefenderPosition = target;
            def.Teta = (state.Location - GameParameters.OurGoalCenter).AngleInDegrees;
            return def;
        }
        private static DefenceInfo MarkStatic(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, List<DefenceInfo> CurrentInfo)
        {

            DefenceInfo def = new DefenceInfo();
            int? oppid = Command.OppID;
            Position2D? target;
            def.RoleType = Command.RoleType;
            def.OppID = oppid;

            Position2D Target = Position2D.Zero;
            SingleObjectState state;
            if (oppid.HasValue && Model.Opponents.ContainsKey(oppid.Value))
                state = Model.Opponents[oppid.Value];
            else
                state = ballState;

            Vector2D oppSpeedVector = state.Speed;
            Vector2D oppOurGoalCenter = GameParameters.OurGoalCenter - state.Location;
            double innerpOppOurGoal = oppSpeedVector.InnerProduct(oppOurGoalCenter);



            double oppSpeed = state.Speed.Size;
            double minDist = GameParameters.SafeRadi(state, StaticMarkerDefenceUtils.MinDistMarkMargin);

            Position2D minimum = GameParameters.OurGoalCenter + (state.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(minDist);
            Position2D maximum = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.2);
            Position2D posToGo = Position2D.Zero;



            double MarkFromDist = StaticMarkerDefenceUtils.MarkFromDist;

            posToGo = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(MarkFromDist);

            if (minimum.DistanceFrom(GameParameters.OurGoalCenter) > posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = minimum;
            else if (maximum.DistanceFrom(GameParameters.OurGoalCenter) < posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = maximum;
            else
                Target = posToGo;


            maximum.DrawColor = System.Drawing.Color.Blue;
            minimum.DrawColor = System.Drawing.Color.Yellow;
            DrawingObjects.AddObject(Target);
            DrawingObjects.AddObject(minimum);
            DrawingObjects.AddObject(maximum);

            if (Target.X > GameParameters.OurGoalCenter.X)
                Target.X = GameParameters.OurGoalCenter.X;
            if (Math.Abs(Target.Y) > Math.Abs(GameParameters.OurLeftCorner.Y))
                Target = new Line(Target, GameParameters.OurGoalCenter).CalculateX(Math.Abs(GameParameters.OurLeftCorner.Y) * Math.Sign(Target.Y));

            target = Target;

            def.TargetState = state;
            def.DefenderPosition = target;
            def.Teta = (state.Location - target.Value).AngleInDegrees;
            return def;
        }

        #endregion

        #region RegionalDefence
        private static DefenceInfo ST = new DefenceInfo();
        private static DefenceInfo RegionalDefence(GameStrategyEngine engine, WorldModel Model, DefenderCommand Command, List<DefenceInfo> DeterminedDefenders, List<DefenceInfo> CurrentInfo)
        {
            DefenceInfo def = new DefenceInfo();
            if (Command.RoleType == typeof(RegionalDefenderRole))
            {
                List<Position2D?> Pos2Exclude = DeterminedDefenders.Where(y => y.DefenderPosition.Value.DistanceFrom(GameParameters.OurGoalCenter) < 1.6).Select(s => s.DefenderPosition).ToList();
                DrawingObjects.AddObject(new Circle(GameParameters.OurGoalCenter, 1.5, new Pen(Color.Red, .01f)));
                List<Vector2D> Vectors = new List<Vector2D>();


                Position2D? target = null;
                //target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                Pos2Exclude.ForEach(u => Vectors.Add(u.Value - GameParameters.OurGoalCenter));
                Vectors.Add(GameParameters.OurRightCorner - GameParameters.OurGoalCenter);
                Vectors.Add(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter);
                //Vectors.OrderByDescending(i => Math.Abs(Vector2D.AngleBetweenInDegrees(i, GameParameters.OurLeftCorner - GameParameters.OurGoalCenter)));
                //double anglebetween = Vector2D.AngleBetweenInDegrees(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter, Vectors[0]);
                //List<Vector2D> vec2 = new List<Vector2D>();
                List<double> Beta = new List<double>();
                foreach (var item in Vectors)
                {
                    Beta.Add((item.AngleInDegrees > 0) ? item.AngleInDegrees - 90 : 270 + item.AngleInDegrees);
                }
                List<double> SortedBeta = Beta.OrderBy(o => o).ToList();
                List<double> DifBEta = new List<double>();
                double max = double.MinValue;
                int maxindex = 0;
                for (int i = 0; i < SortedBeta.Count - 1; i++)
                {
                    if (Math.Sign((GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2) * Math.PI / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2 * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2))).Y) != ballState.Location.Y)
                    {
                        if ((SortedBeta[i + 1] - SortedBeta[i]) * 1.3 > max)
                        {
                            max = SortedBeta[i + 1] - SortedBeta[i];
                            maxindex = i;
                        }
                    }
                    else if (Math.Sign((GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2) * Math.PI / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2 * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2))).Y) == ballState.Location.Y)
                    {
                        if (SortedBeta[i + 1] - SortedBeta[i] > max)
                        {
                            max = SortedBeta[i + 1] - SortedBeta[i];
                            maxindex = i;
                        }
                    }
                }
                double targetangle = 0;
                double targetbeta = (SortedBeta[maxindex] + SortedBeta[maxindex + 1]) / 2;
                if (targetbeta > 0 && targetbeta < 90)
                {
                    targetangle = targetbeta + 90;
                }
                else
                {
                    targetangle = targetbeta - 270;
                }
                foreach (var item in Vectors)
                {
                    DrawingObjects.AddObject(new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + item, new Pen(Color.Black, .01f)), item.X.ToString());
                }
                if (Math.Abs(targetangle) > 90 && CurrentInfo.Any(t => t.RoleType == typeof(RegionalDefenderRole2)))
                {
                    targetbeta = SortedBeta[maxindex] + (((SortedBeta[maxindex + 1] - SortedBeta[maxindex])) / 3);
                    if (targetbeta > 0 && targetbeta < 90)
                    {
                        targetangle = targetbeta + 90;
                    }
                    else
                    {
                        targetangle = targetbeta - 270;
                    }
                    Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2));
                    target = GameParameters.OurGoalCenter + FinalVector;
                }
                else
                {
                    Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2));
                    target = GameParameters.OurGoalCenter + FinalVector;
                }
                //double maxGoodNess = double.MinValue;
                //Position2D? target = null;
                //Position2D tar = new Position2D(GameParameters.OurGoalCenter.X - 1.1, 0);

                //foreach (var item in Command.RegionalDefendPoints)
                //{

                //    double goodness;
                //    double total = 0;
                //    Pos2Exclude.ForEach(p => total += item.DistanceFrom(p.Value));
                //    target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                //    goodness += total / 50;
                //    if (goodness > maxGoodNess)
                //    {
                //        maxGoodNess = goodness;
                //        if (target.HasValue)
                //        {
                //            Vector2D vec = item - target.Value;
                //            tar = target.Value + vec.GetNormalizeToCopy(1.1 + Command.RegionalDistFromDangerZone);
                //        }
                //        else
                //        {
                //            Vector2D vec = item - GameParameters.OurGoalCenter;
                //            double r = GameParameters.SafeRadi(new SingleObjectState(item, new Vector2D(), 0), Command.RegionalDistFromDangerZone);
                //            tar = GameParameters.OurGoalCenter + vec.GetNormalizeToCopy(r);
                //        }
                //    }
                //}
                ST = def;
                def.OppID = null;
                def.RoleType = Command.RoleType;

                def.DefenderPosition = target;
                if (target.HasValue && FreekickDefence.RearRegional)
                {
                    def.DefenderPosition = GameParameters.OurGoalCenter + (target.Value - GameParameters.OurGoalCenter).GetNormalizeToCopy(4);
                    Position2D targete = def.DefenderPosition.Value;
                    if (Math.Abs(targete.Y) > 1)
                    {
                        def.DefenderPosition = new Position2D(Math.Min(Math.Max(targete.X, .5), 1), Math.Sign(targete.Y));
                    }
                    //def.DefenderPosition = GameParameters.OurGoalCenter+ (targete - GameParameters.OurGoalCenter).GetNormalizeToCopy(2);
                }
                def.TargetState = ballState;

                return def;
            }
            else
            {
                List<Position2D?> Pos2Exclude = DeterminedDefenders.Where(y => y.DefenderPosition.Value.DistanceFrom(GameParameters.OurGoalCenter) < 1.6).Select(s => s.DefenderPosition).ToList();
                DrawingObjects.AddObject(new Circle(GameParameters.OurGoalCenter, 1.5, new Pen(Color.Red, .01f)));
                List<Vector2D> Vectors = new List<Vector2D>();


                Position2D? target = null;
                //target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                Pos2Exclude.ForEach(u => Vectors.Add(u.Value - GameParameters.OurGoalCenter));
                Vectors.Add(GameParameters.OurRightCorner - GameParameters.OurGoalCenter);
                Vectors.Add(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter);
                //Vectors.OrderByDescending(i => Math.Abs(Vector2D.AngleBetweenInDegrees(i, GameParameters.OurLeftCorner - GameParameters.OurGoalCenter)));
                //double anglebetween = Vector2D.AngleBetweenInDegrees(GameParameters.OurLeftCorner - GameParameters.OurGoalCenter, Vectors[0]);
                //List<Vector2D> vec2 = new List<Vector2D>();
                List<double> Beta = new List<double>();
                foreach (var item in Vectors)
                {
                    Beta.Add((item.AngleInDegrees > 0) ? item.AngleInDegrees - 90 : 270 + item.AngleInDegrees);
                }
                List<double> SortedBeta = Beta.OrderBy(o => o).ToList();
                List<double> DifBEta = new List<double>();
                double max = double.MinValue;
                int maxindex = 0;
                for (int i = 0; i < SortedBeta.Count - 1; i++)
                {
                    if (SortedBeta[i + 1] - SortedBeta[i] > max)
                    {
                        max = SortedBeta[i + 1] - SortedBeta[i];
                        maxindex = i;

                    }
                }
                double targetangle = 0;
                double targetbeta = (SortedBeta[maxindex] + SortedBeta[maxindex + 1]) / 2;
                if (targetbeta > 0 && targetbeta < 90)
                {
                    targetangle = targetbeta + 90;
                }
                else
                {
                    targetangle = targetbeta - 270;
                }
                foreach (var item in Vectors)
                {
                    DrawingObjects.AddObject(new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + item, new Pen(Color.Black, .01f)), item.X.ToString());
                }

                Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2));
                target = GameParameters.OurGoalCenter + FinalVector;

                //double maxGoodNess = double.MinValue;
                //Position2D? target = null;
                //Position2D tar = new Position2D(GameParameters.OurGoalCenter.X - 1.1, 0);

                //foreach (var item in Command.RegionalDefendPoints)
                //{

                //    double goodness;
                //    double total = 0;
                //    Pos2Exclude.ForEach(p => total += item.DistanceFrom(p.Value));
                //    target = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, item, out goodness, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, false, null, Pos2Exclude);
                //    goodness += total / 50;
                //    if (goodness > maxGoodNess)
                //    {
                //        maxGoodNess = goodness;
                //        if (target.HasValue)
                //        {
                //            Vector2D vec = item - target.Value;
                //            tar = target.Value + vec.GetNormalizeToCopy(1.1 + Command.RegionalDistFromDangerZone);
                //        }
                //        else
                //        {
                //            Vector2D vec = item - GameParameters.OurGoalCenter;
                //            double r = GameParameters.SafeRadi(new SingleObjectState(item, new Vector2D(), 0), Command.RegionalDistFromDangerZone);
                //            tar = GameParameters.OurGoalCenter + vec.GetNormalizeToCopy(r);
                //        }
                //    }
                //}
                def.OppID = null;
                def.RoleType = Command.RoleType;
                def.DefenderPosition = target;
                def.TargetState = ballState;

                return def;
            }
        }
        #endregion

        #region Static_Defenders
        private static bool isMarker = false;

        private static Position2D? secondLastPos = null;
        public static List<DefenceInfo> MatchStatic(GameStrategyEngine engine, WorldModel Model, List<DefenderCommand> Commands)
        {



            CurrentlyAddedDefenders = Commands;
            List<DefenceInfo> res = new List<DefenceInfo>();
            DefenceInfo TempGoali = new DefenceInfo();
            DefenderCommand goaliCommand = Commands.Where(w => w.RoleType.GetInterfaces().Any(a => a == typeof(IGoalie))).FirstOrDefault();

            foreach (var item in Commands)
            {
                if (item.RoleType == typeof(StaticDefender1))
                {
                    DefenceInfo def;
                    int? rb = StaticRB(engine, Model, CurrentInfos);
                    SingleObjectState target = (rb.HasValue) ? Model.Opponents[rb.Value] : new SingleObjectState(ballState.Location + (ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size)), ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size), 0f);

                    target = new SingleObjectState(target.Location, target.Speed, target.Angle);
                    def = CalculateFirstStatic(Model, target, rb);

                    //DrawingObjects.AddObject(new Circle(def.DefenderPosition.Value, 0.2, new System.Drawing.Pen((rb.HasValue) ? System.Drawing.Color.Orange : System.Drawing.Color.Blue, 0.01f)));
                    res.Add(def);
                }
                else if (item.RoleType == typeof(StaticDefender2))
                {
                    DefenceInfo def;
                    int? rb = StaticRB(engine, Model, CurrentInfos);
                    SingleObjectState target = (rb.HasValue) ? Model.Opponents[rb.Value] : new SingleObjectState(ballState.Location + (ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size)), ballState.Speed.GetNormalizeToCopy(extendStatticDefenceTarget * ballState.Speed.Size), 0f);


                    def = CalculateSecondStatic(Model, target, rb);
                    //DrawingObjects.AddObject(new Circle(def.DefenderPosition.Value, 0.2, new System.Drawing.Pen((rb.HasValue) ? System.Drawing.Color.Orange : System.Drawing.Color.Blue, 0.01f)));
                    res.Add(def);
                }
                else if (item.RoleType == typeof(MarkerRoleStatic))
                {
                    DefenceInfo def = MarkStatic(engine, Model, item, CurrentInfos);
                    res.Add(def);
                }
            }
            List<DefenceInfo> temp = res.ToList();
            res = OverLapSolvingStatic(Model, res);
            CurrentInfos = res;
            CurrentStates = new Dictionary<RoleBase, int>();
            return res;
        }
        public static List<DefenceInfo> OverLapSolvingStatic(WorldModel Model, List<DefenceInfo> infos)
        {
            List<DefenceInfo> res = new List<DefenceInfo>();
            List<Forbiden> forbidens = new List<Forbiden>();

            DefenceInfo staticFirst = infos.Where(w => w.RoleType == typeof(StaticDefender1)).FirstOrDefault();
            DefenceInfo staticSecond = infos.Where(w => w.RoleType == typeof(StaticDefender2)).FirstOrDefault();

            List<DefenceInfo> staticSeceonds = infos.Where(w => w.RoleType == typeof(StaticDefender2)).ToList();
            List<DefenceInfo> staticFirsts = infos.Where(w => w.RoleType == typeof(StaticDefender1)).ToList();
            List<DefenceInfo> marker = infos.Where(w => w.RoleType == typeof(MarkerRoleStatic)).ToList();
            double tresh = 0.0;
            double robotRadius = RobotParameters.OurRobotParams.Diameter / 2;
            if (StaticSecondState == DefenderStates.Normal)
            {
                if (staticFirst != null)
                    forbidens.Add(new Forbiden()
                    {
                        center = staticFirst.DefenderPosition.Value,
                        radius = robotRadius
                    });
                UpdateForbidens(Model, ref forbidens, staticSeceonds, tresh, robotRadius);
                if (staticFirst != null)
                    res.Add(staticFirst);
                res.AddRange(staticSeceonds);
            }
            else
            {
                if (staticSecond != null)
                    forbidens.Add(new Forbiden()
                    {
                        center = staticSecond.DefenderPosition.Value,
                        radius = robotRadius
                    });
                UpdateForbidens(Model, ref forbidens, staticFirsts, tresh, robotRadius);
                if (staticSecond != null)
                    res.Add(staticSecond);
                res.AddRange(staticFirsts);
            }
            UpdateForbidens(Model, ref forbidens, marker, tresh, robotRadius);
            res.AddRange(marker);
            res.ForEach(p =>
            {
                //double d1, d2;
                //if (!GameParameters.IsInDangerousZone(p.TargetState.Location, false, 0.2, out d1, out d2))
                //{
                //    p.Teta = (p.TargetState.Location - p.DefenderPosition.Value).AngleInDegrees;
                //}
                //else
                {
                    if (p.RoleType == typeof(StaticDefender1))
                    {
                        Position2D t = GameParameters.OurGoalLeft - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5);
                        p.Teta = (p.DefenderPosition.Value - t).AngleInDegrees;
                    }
                    else if (p.RoleType == typeof(StaticDefender2))
                    {
                        Position2D t = GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5);
                        p.Teta = (p.DefenderPosition.Value - t).AngleInDegrees;
                    }

                }
            });
            return res;

        }
        private static DefenceInfo CalculateFirstStatic(WorldModel Model, SingleObjectState state, int? oppid)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Position2D DefencePos = new Position2D();
            double marg = .04 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .04));
            double radi = GameParameters.SafeRadi(state, marg);

            double d1, d2;

            Position2D ball = GameParameters.InFieldSize(ballState.Location);
            Position2D st = GameParameters.InFieldSize(state.Location);
            bool indangerzone = false;
            if (GameParameters.IsInDangerousZone(ball, false, 0.2, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                radi = GameParameters.SafeRadi(state, marg);
            }
            if (GameParameters.IsInDangerousZone(ball, false, -0.1, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                indangerzone = true;
            }
            //Position2D NextPos = new Position2D();
            double minDist = double.MaxValue;
            Circle C1 = new Circle(GameParameters.OurGoalCenter, radi);
            Position2D lastBallPos;
            if (!indangerzone)
            {
                ////Line L1 = new Line(st,GameParameters.OurGoalLeft - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
                //Line L1 = new Line(st, GameParameters.OurGoalLeft - new Vector2D(0, GameParameters.OurGoalLeft.DistanceFrom(GameParameters.OurGoalCenter) / 2 - .05));

                //List<Position2D> P = C1.Intersect(L1);

                //for (int i = 0; i < P.Count; i++)
                //{
                //    if ((P[i].DistanceFrom(state.Location) < minDist))
                //    {
                //        minDist = P[i].DistanceFrom(st);
                //        DefencePos = P[i];
                //    }
                //}

                double x = Model.BallState.Location.X;
                double y = Model.BallState.Location.Y;
                Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalRight, GameParameters.OurGoalLeft, 0.33);


                Position2D rightIntersect;
                Position2D leftIntersect;

                Line left = new Line(p1, Model.BallState.Location);
                Line right = new Line(GameParameters.OurGoalRight.Extend(0, 0), Model.BallState.Location);

                double distToPenaltyAreaThreshold = 0.00;
                Line l1 = new Line(GameParameters.OurGoalLeft.Extend(-1.20, 0.60 + distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(0, 0.60 + distToPenaltyAreaThreshold));
                Line l2 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(-1.20 - distToPenaltyAreaThreshold, 0.6 + distToPenaltyAreaThreshold));
                Line l3 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalRight.Extend(0, -0.60 - distToPenaltyAreaThreshold));
                Position2D centerRobot = new Position2D();
                double dist, distFrom;
                bool IsInOurDangerZone = GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0, out dist, out distFrom);
                //  DrawingObjects.AddObject(l1);
                //DrawingObjects.AddObject(l3);
                //if (GameParameters.IsInField(Model.BallState.Location, 0))
                //{
                //    lastBallPos = Model.BallState.Location;
                //}


                Line intevallToBall = new Line(Position2D.Interpolate(right.Head, left.Head, 0.5), Model.BallState.Location);
                // DrawingObjects.AddObject(intevallToBall);

                if (GameParameters.SegmentIntersect(intevallToBall, l1).HasValue) // left
                {
                    centerRobot = l1.IntersectWithLine(intevallToBall).Value.Extend(0, 0.1);
                }
                else if (GameParameters.SegmentIntersect(intevallToBall, l3).HasValue) //right
                {
                    centerRobot = l3.IntersectWithLine(intevallToBall).Value.Extend(0, -0.1);
                }
                else if (GameParameters.SegmentIntersect(intevallToBall, l2).HasValue) //top
                {
                    centerRobot = l2.IntersectWithLine(intevallToBall).Value.Extend(-0.1, 0);
                }
                else
                {
                    centerRobot = Model.BallState.Location;
                }
                DefencePos = centerRobot;
            }
            else
            {
                Vector2D v = st - GameParameters.OurGoalCenter;
                Position2D p = (GameParameters.OurGoalCenter + v.GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(st, Vector2D.Zero, 0), marg)));
                p = p + Vector2D.FromAngleSize(v.AngleInRadians - Math.PI / 2, 0.1);
                DefencePos = GameParameters.OurGoalCenter + (p - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(p, Vector2D.Zero, 0), marg));
            }
            //Line L2 = new Line(st, GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
            //List<Position2D> P2 = C1.Intersect(L2);

            //minDist = double.MaxValue;
            //for (int i = 0; i < P2.Count; i++)
            //{
            //    if (P2[i].DistanceFrom(st) < minDist)
            //    {
            //        minDist = P2[i].DistanceFrom(st);
            //        NextPos = P2[i];
            //    }
            //}
            if (ballState.Location.Y > 0)
            {
                //Vector2D l1 = ball - GameParameters.OurGoalCenter;
                //Vector2D l2 = GameParameters.OurLeftCorner - GameParameters.OurGoalCenter;
                //if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13)
                //{
                //    Line tmpL = new Line(st, GameParameters.OurGoalCenter.Extend(0, 0.005));
                //    List<Position2D> poses = C1.Intersect(tmpL);
                //    minDist = double.MaxValue;
                //    for (int i = 0; i < poses.Count; i++)
                //    {
                //        if ((poses[i].DistanceFrom(state.Location) < minDist))
                //        {
                //            DefencePos = poses[i];
                //            minDist = poses[i].DistanceFrom(state.Location);
                //        }
                //    };
                //}
            }
            else
            {
                //Vector2D l1 = ball - GameParameters.OurGoalCenter;
                //Vector2D l2 = GameParameters.OurRightCorner - GameParameters.OurGoalCenter;
                //if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13 || ball.X > GameParameters.OurGoalCenter.X - 0.09)
                //{
                //    DefencePos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize(-1.79, radi);
                //}
            }
            //}
            //else
            {
                //var dist = ball.DistanceFrom(st);

                //var stdefence = ball + Vector2D.FromAngleSize((st - ball).AngleInRadians, 4);


                //Line l = new Line(ball, stdefence);

                //List<Position2D> inters = C1.Intersect(l);
                //Position2D pos;
                //if (inters.Count > 0)
                //{
                //    pos = inters.First(f => f.DistanceFrom(st) <= inters.Min(m => m.DistanceFrom(st)));
                //    radi = GameParameters.SafeRadi(new SingleObjectState() { Location = pos }, 0.12);
                //    pos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize((pos - GameParameters.OurGoalCenter).AngleInRadians + 0.1, radi);

                //    DefencePos = pos;
                //}



                //  DrawingObjects.AddObject(l);
            }
            //if (DefencePos.DistanceFrom(NextPos) < RobotParameters.OurRobotParams.Diameter)
            //{
            //    DefencePos = new Position2D((NextPos.X + DefencePos.X) / 2.0, (NextPos.Y + DefencePos.Y) / 2.0);
            //}
            //if (DefencePos.X > GameParameters.OurGoalCenter.X - RobotParameters.OurRobotParams.Diameter)
            //{
            //    DefencePos.X = GameParameters.OurGoalCenter.X - RobotParameters.OurRobotParams.Diameter;
            //}
            //DefencePos.DrawColor = System.Drawing.Color.Red;
            //DrawingObjects.AddObject(DefencePos);
            if (DefencePos.X > GameParameters.OurGoalCenter.X - 0.09)
                DefencePos = new Position2D(GameParameters.OurGoalCenter.X - 0.09, DefencePos.Y);
            lastfirst = DefencePos;
            DefenceInfo di = new DefenceInfo();
            di.DefenderPosition = lastfirst;
            di.RoleType = typeof(StaticDefender1);
            di.TargetState = state;
            di.OppID = oppid;
            return di;
        }
        private static DefenceInfo CalculateSecondStatic(WorldModel Model, SingleObjectState state, int? oppid)
        {

            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Position2D DefencePos = new Position2D();
            double marg = .04 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .04));
            double radi = GameParameters.SafeRadi(state, marg);

            double d1, d2;

            Position2D ball = GameParameters.InFieldSize(ballState.Location);
            Position2D st = GameParameters.InFieldSize(state.Location);

            if (GameParameters.IsInDangerousZone(ball, false, 0.2, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                radi = GameParameters.SafeRadi(state, marg);
            }
            bool indangerzone = false;
            if (GameParameters.IsInDangerousZone(ball, false, -0.1, out d1, out d2) && ballStateFast.Speed.Size < 2)
            {
                indangerzone = true;
            }
            //  DrawingObjects.AddObject
            isMarker = false;
            //DrawingObjects.AddObject(C1);
            //DrawingObjects.AddObject(L1);
            double minDist = double.MaxValue;
            Circle C1 = new Circle(GameParameters.OurGoalCenter, radi);
            //Position2D NextPos = new Position2D();
            if (!indangerzone)
            {
                //// Line L1 = new Line(st, GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
                //Line L1 = new Line(st, GameParameters.OurGoalRight + new Vector2D(0, GameParameters.OurGoalLeft.DistanceFrom(GameParameters.OurGoalCenter) / 2 - .05));
                //List<Position2D> P = C1.Intersect(L1);

                //for (int i = 0; i < P.Count; i++)
                //{
                //    if ((P[i].DistanceFrom(state.Location) < minDist))
                //    {
                //        minDist = P[i].DistanceFrom(st);
                //        DefencePos = P[i];
                //    }
                //}

                ////Line L2 = new Line(st, GameParameters.OurGoalLeft - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5));
                ////List<Position2D> P2 = C1.Intersect(L2);

                ////  minDist = double.MaxValue;
                ////for (int i = 0; i < P2.Count; i++)
                ////{
                ////    if (P2[i].DistanceFrom(st) < minDist)
                ////    {
                ////        minDist = P2[i].DistanceFrom(st);
                ////        NextPos = P2[i];
                ////    }
                ////}            double x = Model.BallState.Location.X;
                double y = Model.BallState.Location.Y;
                Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalLeft, GameParameters.OurGoalRight, 0.33);
                double dist, distFrom;
                bool IsInOurDangerZone = GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0, out dist, out distFrom);
                Position2D rightIntersect;
                Position2D leftIntersect;
                Line right = new Line(p1, Model.BallState.Location);
                Line left = new Line(GameParameters.OurGoalLeft.Extend(0, 0), Model.BallState.Location);

                double distToPenaltyAreaThreshold = 0.00;
                Line l1 = new Line(GameParameters.OurGoalLeft.Extend(-1.20, 0.60 + distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(0, 0.60 + distToPenaltyAreaThreshold));
                Line l2 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(-1.20 - distToPenaltyAreaThreshold, 0.6 + distToPenaltyAreaThreshold));
                Line l3 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalRight.Extend(0, -0.60 - distToPenaltyAreaThreshold));
                Position2D centerRobot = new Position2D();
                //if (GameParameters.IsInField(Model.BallState.Location, 0))
                //{
                //    lastBallPos = Model.BallState.Location;
                //}
                Line intevallToBall = new Line(Position2D.Interpolate(right.Head, left.Head, 0.5), Model.BallState.Location);
                //DrawingObjects.AddObject(intevallToBall);

                if (GameParameters.SegmentIntersect(intevallToBall, l1).HasValue) // left
                {
                    centerRobot = l1.IntersectWithLine(intevallToBall).Value.Extend(0, 0.1);
                }
                else if (GameParameters.SegmentIntersect(intevallToBall, l3).HasValue) //right
                {
                    centerRobot = l3.IntersectWithLine(intevallToBall).Value.Extend(0, -0.1);
                }
                else if (GameParameters.SegmentIntersect(intevallToBall, l2).HasValue) //top
                {
                    centerRobot = l2.IntersectWithLine(intevallToBall).Value.Extend(-0.1, 0);
                }
                else
                {
                    centerRobot = Model.BallState.Location;
                }
                DefencePos = centerRobot;
            }
            else
            {
                Vector2D v = st - GameParameters.OurGoalCenter;
                Position2D p = (GameParameters.OurGoalCenter + v.GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(st, Vector2D.Zero, 0), marg)));
                p = p + Vector2D.FromAngleSize(v.AngleInRadians + Math.PI / 2, 0.1);
                DefencePos = GameParameters.OurGoalCenter + (p - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(p, Vector2D.Zero, 0), marg));
            }

            if (ball.Y < 0)
            {
                //Vector2D l1 = ball - GameParameters.OurGoalCenter;
                //Vector2D l2 = GameParameters.OurRightCorner - GameParameters.OurGoalCenter;
                //if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13)
                //{
                //    Line tmpL = new Line(st, GameParameters.OurGoalCenter.Extend(0, 0.005));
                //    List<Position2D> poses = C1.Intersect(tmpL);
                //    minDist = double.MaxValue;
                //    for (int i = 0; i < poses.Count; i++)
                //    {
                //        if ((poses[i].DistanceFrom(state.Location) < minDist))
                //        {
                //            DefencePos = poses[i];
                //            minDist = poses[i].DistanceFrom(st);
                //        }
                //    };

                //}
            }
            else
            {
                //Vector2D l1 = ballState.Location - GameParameters.OurGoalCenter;
                //Vector2D l2 = GameParameters.OurLeftCorner - GameParameters.OurGoalCenter;
                //if (Math.Abs(Vector2D.AngleBetweenInDegrees(l1, l2)) < 13 || ball.X > GameParameters.OurGoalCenter.X - 0.09)
                //{
                //    DefencePos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize(1.79, radi);
                //}

            }
            //}
            //else
            {

                //var dist = ballState.Location.DistanceFrom(st);

                //var stdefence = ballState.Location + Vector2D.FromAngleSize((st - ball).AngleInRadians, 4);

                //C1 = new Circle(GameParameters.OurGoalCenter, radi);

                //Line l = new Line(ball, stdefence);

                //List<Position2D> inters = C1.Intersect(l);
                //var pos = inters.First(f => f.DistanceFrom(st) <= inters.Min(m => m.DistanceFrom(st)));
                //radi = GameParameters.SafeRadi(new SingleObjectState() { Location = pos }, 0.12);
                //pos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize((pos - GameParameters.OurGoalCenter).AngleInRadians - 0.1, radi);





                // DrawingObjects.AddObject(C1);
                //DrawingObjects.AddObject(l);
            }

            if (DefencePos.X > GameParameters.OurGoalCenter.X - 0.09)
                DefencePos = new Position2D(GameParameters.OurGoalCenter.X - 0.09, DefencePos.Y);
            lastsecond = DefencePos;
            DefenceInfo di = new DefenceInfo();
            //if (DefencePos.DistanceFrom(NextPos) < RobotParameters.OurRobotParams.Diameter/2 && secondLastPos.HasValue)
            //{
            //  DefencePos = secondLastPos.Value;
            //    //    int? MarkerID = FindMarker(Model);
            //    //    if (MarkerID != null)
            //    //    {
            //    //        isMarker = true;
            //    //        radi = GameParameters.SafeRadi(Model.Opponents[MarkerID.Value], 0.02);
            //    //        di.TargetState = Model.Opponents[MarkerID.Value];
            //    //        DefencePos = GameParameters.OurGoalCenter + (Model.Opponents[MarkerID.Value].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(radi);
            //    //    }
            //    //    else
            //    //    {
            //    //        di.TargetState = ballState;
            //    //        if (ballState.Location.Y < 0)
            //    //        {
            //    //            Position2D Corner = new Position2D(0, GameParameters.OurLeftCorner.Y);
            //    //            DefencePos = GameParameters.OurGoalCenter + (Corner - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.2);
            //    //        }
            //    //        else
            //    //        {
            //    //            Position2D Corner = new Position2D(0, GameParameters.OurRightCorner.Y);
            //    //            DefencePos = GameParameters.OurGoalCenter + (Corner - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.2);
            //    //        }
            //    //    }
            //}
            //else
            //{
            //    secondLastPos = DefencePos;
            //}
            //DefencePos.DrawColor = System.Drawing.Color.Blue;
            //DrawingObjects.AddObject(DefencePos);
            di.OppID = oppid;
            di.TargetState = state;
            di.DefenderPosition = lastsecond;
            di.RoleType = typeof(StaticDefender2);
            secondLastPos = DefencePos;
            return di;
        }
        private static int? FindNearestOpponentToBall(WorldModel Model)
        {
            int? retIndex = null;
            double MinDist = double.MaxValue;
            foreach (int key in Model.Opponents.Keys)
            {
                if (ballState.Location.DistanceFrom(Model.Opponents[key].Location) < MinDist)
                {
                    MinDist = ballState.Location.DistanceFrom(Model.Opponents[key].Location);
                    retIndex = key;
                }
            }
            return retIndex;
        }
        private static int? FindMarker(WorldModel Model, SingleObjectState ballState, SingleObjectState ballStateFast)
        {
            int? retIndex = null;
            double MinDist = double.MaxValue;
            int? BallCacher = FindNearestOpponentToBall(Model);
            if (BallCacher != null)
            {
                foreach (int key in Model.Opponents.Keys)
                {
                    if (key != BallCacher)
                    {
                        if (GameParameters.OurGoalCenter.DistanceFrom(Model.Opponents[key].Location) < MinDist)
                        {
                            MinDist = ballState.Location.DistanceFrom(Model.Opponents[key].Location);
                            retIndex = key;
                        }
                    }
                }
            }
            return retIndex;
        }
        public static int? GetOurBallOwner(WorldModel Model, int? firstID, int? secondID)
        {
            List<int> defenders = new List<int>();
            if (firstID.HasValue)
                defenders.Add(firstID.Value);
            if (secondID.HasValue)
                defenders.Add(secondID.Value);
            if (defenders.Count == 0)
            {
                LastOwner = null;
                return null;
            }
            if (!GameParameters.IsInField(ballState.Location, 0.1))
                return null;
            Position2D pos = new Position2D();
            double minDistOpp = 100;
            if (Model.Opponents.Count > 0)
                minDistOpp = Model.Opponents.Min(m => m.Value.Location.DistanceFrom(ballState.Location));
            if (minDistOpp < 0.5)
            {
                LastOwner = null;
                return null;
            }



            if (LastOwner.HasValue && Model.OurRobots.ContainsKey(LastOwner.Value))
            {
                pos = Model.OurRobots[LastOwner.Value].Location;
            }
            else if (firstID.HasValue && secondID.HasValue)
            {
                Position2D perp1 = new Position2D(), perp2 = new Position2D();
                perp1 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[firstID.Value].Location);
                perp2 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[secondID.Value].Location);

                if (perp1.DistanceFrom(ballState.Location) > perp2.DistanceFrom(ballStateFast.Location))
                    pos = Model.OurRobots[firstID.Value].Location;
                else
                    pos = Model.OurRobots[secondID.Value].Location;
            }
            else if (firstID.HasValue)
            {
                pos = Model.OurRobots[firstID.Value].Location;
            }

            if (pos.DistanceFrom(ballStateFast.Location) > 0.8)
                return null;
            Vector2D ballSpeed = ballStateFast.Speed;
            double v = Vector2D.AngleBetweenInRadians(ballSpeed, (pos - ballStateFast.Location));
            double maxIncomming = 2, maxVertical = 0.5, maxOutGoing = 1;
            double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
                + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
                + maxVertical;


            double stateCoef = 1;
            if (FreekickDefence.StaticFirstState == DefenderStates.BallInFront || FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
                stateCoef = 1.2;

            if (ballSpeed.Size < acceptableballRobotSpeed * stateCoef)
            {
                double accour = 2, accopp = 3;

                double dist = Model.OurRobots.Min(m => m.Value.Location.DistanceFrom(ballState.Location));
                var robot = Model.OurRobots.First(f => f.Value.Location.DistanceFrom(ballState.Location) == dist);
                var T_our = Model.OurRobots.Where(w => (LastOwner.HasValue) ? w.Key == LastOwner.Value : w.Key == robot.Key).Select(s => new
                {
                    robotID = s.Key,
                    t = 2 * Math.Sqrt(s.Value.Location.DistanceFrom(ballState.Location) / accour)
                });
                int goalieId = Model.GoalieID.Value;
                var Our_other = Model.OurRobots.Where(w => !defenders.Contains(w.Key) && w.Key != goalieId).Select(s => new
                {
                    t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(ballState.Location)) / accopp)
                });
                var opp = Model.Opponents.Where(w => w.Value.Location.DistanceFrom(ballState.Location) == minDistOpp).Select(s => new
                {
                    t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(ballState.Location)) / accopp)
                });
                var T_other = Our_other.Union(opp);
                double minT_other = 100;
                double minT_our = 100;
                if (T_other.Count() > 0)
                    minT_other = T_other.Min(m => m.t);
                if (T_our.Count() > 0)
                    minT_our = T_our.Min(m => m.t);

                if (minT_our < minT_other * stateCoef)
                {
                    LastOwner = T_our.First(f => f.t == minT_our).robotID;
                    return LastOwner;
                }
            }
            LastOwner = null;
            return null;
        }
        public static void CalculateStaticPos(GameStrategyEngine engine, WorldModel Model, Dictionary<int, RoleBase> currentRoles)
        {
            double dis1, dis2;

            // int? ballOwner = engine.GameInfo.OurTeam.BallOwner;

            double t;
            var d1 = CurrentInfos.FirstOrDefault(s => s.RoleType == typeof(StaticDefender1));
            if (d1 == null)
                d1 = new DefenceInfo()
                {
                    RoleType = typeof(StaticDefender1)
                };

            var d2 = CurrentInfos.FirstOrDefault(s => s.RoleType == typeof(StaticDefender2));
            if (d2 == null)
                d2 = new DefenceInfo()
                {
                    RoleType = typeof(StaticDefender2)
                };

            var d1Role = currentRoles.FirstOrDefault(s => s.Value.GetType() == typeof(StaticDefender1));
            var d2Role = currentRoles.FirstOrDefault(s => s.Value.GetType() == typeof(StaticDefender2));

            bool containId1 = d1Role.Value != null && Model.OurRobots.ContainsKey(d1Role.Key);
            bool containId2 = d2Role.Value != null && Model.OurRobots.ContainsKey(d2Role.Key);

            int? nullid = null;
            int? ballOwner = GetOurBallOwner(Model, (containId1) ? d1Role.Key : nullid, (containId2) ? d2Role.Key : nullid);

            if (GameParameters.IsInDangerousZone(ballState.Location, false, 0.2, out dis1, out dis2) && ballStateFast.Speed.Size < 2)
            {
                LastOwner = null;

                if (containId1)
                    DrawingObjects.AddObject(new StringDraw("InPenaltyArea", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                if (containId2)
                    DrawingObjects.AddObject(new StringDraw("InPenaltyArea", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));

                StaticSecondState = DefenderStates.Normal;
                StaticFirstState = DefenderStates.Normal;
                return;
            }
            if ((containId1 || containId2) && BallKickedToGoal(Model))
            {
                LastOwner = null;
                Position2D perp1 = new Position2D(), perp2 = new Position2D();

                if (containId1)
                    perp1 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[d1Role.Key].Location);

                if (containId2)
                    perp2 = ballStateFast.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[d2Role.Key].Location);

                if (containId1 && (!containId2 || perp1.DistanceFrom(Model.OurRobots[d1Role.Key].Location) < perp2.DistanceFrom(Model.OurRobots[d2Role.Key].Location)))
                {
                    d1.DefenderPosition = Dive(engine, Model, d1Role.Key);
                    StaticFirstState = DefenderStates.KickToGoal;
                    StaticSecondState = DefenderStates.Normal;
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("KickToGoal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                }
                else if (containId2)
                {
                    d2.DefenderPosition = Dive(engine, Model, d2Role.Key);
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("KickToGoal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                    StaticFirstState = DefenderStates.Normal;
                    StaticSecondState = DefenderStates.KickToGoal;
                }
                CurrentInfos = OverLapSolvingStatic(Model, CurrentInfos.Where(w => w.RoleType != null).ToList());
            }
            else if (ballOwner.HasValue && ((containId1 && ballOwner.Value == d1Role.Key) || (containId2 && ballOwner.Value == d2Role.Key)))
            {
                if (containId1 && ballOwner.Value == d1Role.Key)
                {
                    d1.DefenderPosition = GetBackBallPoint(Model, d1Role.Key, out t);
                    StaticFirstState = DefenderStates.BallInFront;
                    StaticSecondState = DefenderStates.Normal;
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("BallInFront", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                }
                else if (containId2 && ballOwner.Value == d2Role.Key)
                {
                    d2.DefenderPosition = GetBackBallPoint(Model, d2Role.Key, out t);
                    StaticFirstState = DefenderStates.Normal;
                    StaticSecondState = DefenderStates.BallInFront;
                    if (containId1)
                        DrawingObjects.AddObject(new StringDraw("Normal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                    if (containId2)
                        DrawingObjects.AddObject(new StringDraw("BallInFront", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                }
                CurrentInfos = OverLapSolvingStatic(Model, CurrentInfos.Where(w => w.RoleType != null).ToList());
            }
            else
            {

                LastOwner = null;
                if (containId1)
                    DrawingObjects.AddObject(new StringDraw("Normal", "ds1", Model.OurRobots[d1Role.Key].Location + new Vector2D(0.2, 0.2)));
                if (containId2)
                    DrawingObjects.AddObject(new StringDraw("Normal", "ds2", Model.OurRobots[d2Role.Key].Location + new Vector2D(0.2, 0.2)));
                StaticFirstState = DefenderStates.Normal;
                StaticSecondState = DefenderStates.Normal;
            }
        }
        private static bool InconmmingOutgoing(WorldModel Model, int RobotID, ref bool isNear)
        {
            Position2D temprobot = Model.Opponents[RobotID].Location + Model.Opponents[RobotID].Speed * 0.04;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + ballStateFast.Speed * 0.04;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            if (ballStateFast.Speed.Size > 2)
            {
                double coef = 1;
                if (LastRB == RBstate.Robot)
                    coef = 1.2;

                double ballspeedAngle = ballStateFast.Speed.AngleInDegrees;
                double robotballInner = Model.Opponents[RobotID].Speed.InnerProduct((ballState.Location - Model.Opponents[RobotID].Location).GetNormnalizedCopy());
                bool ballinGoal = false;
                Line line = new Line();
                line = new Line(ballStateFast.Location, ballStateFast.Location - ballStateFast.Speed);
                Position2D BallGoal = new Position2D();
                BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
                double d = ballStateFast.Location.DistanceFrom(GameParameters.OurGoalCenter);
                if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.65 / coef && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.65 / coef)
                    if (ballStateFast.Speed.InnerProduct(GameParameters.OurGoalRight - ballStateFast.Location) > 0)
                        ballinGoal = true;

                if (ballState.Speed.InnerProduct((temprobot - tempball).GetNormnalizedCopy()) > 1.2 / coef
                    && robotballInner < 2 * coef && robotballInner > -1
                    && !ballinGoal)
                    return true;

            }
            return false;
        }
        private static int? StaticRB(GameStrategyEngine engine, WorldModel Model, List<DefenceInfo> CurrentInfo)
        {

            if (StaticFirstState == DefenderStates.BallInFront || StaticSecondState == DefenderStates.BallInFront)
            {
                LastRB = RBstate.Ball;
                return null;
            }
            var opps = engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
            double d1, d2;
            if (opps.Count > 0 && GameParameters.IsInDangerousZone(ballState.Location, false, 0, out d1, out d2))
            {
                if (!GameParameters.IsInDangerousZone(Model.Opponents[opps.First()].Location, false, 0, out d1, out d2))
                {
                    LastRB = RBstate.Robot;
                    return opps.First();
                }
                else if (opps.Count > 1)
                {
                    LastRB = RBstate.Robot;
                    return opps.Skip(1).First();
                }
                else
                {
                    LastRB = RBstate.Ball;
                    return null;
                }
            }
            else if (opps.Count > 0 && GameParameters.IsInDangerousZone(Model.Opponents[opps.First()].Location, false, 0.03, out d1, out d2))
            {
                LastRB = RBstate.Ball;
                return null;
            }
            if (opps.Count == 0 || ballStateFast.Speed.Size < 1)
            {
                LastRB = RBstate.Ball;
                return null;
            }
            SingleObjectState oppstate = Model.Opponents[opps.First()];

            Position2D temprobot = oppstate.Location + oppstate.Speed * 0.2;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + ballState.Speed * 0.2;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            SingleObjectState ball = ballState;
            Vector2D ballRobot = temprobot - tempball;
            Vector2D robotTarget = GameParameters.OurGoalCenter - temprobot;
            double ballAngle = Vector2D.AngleBetweenInDegrees(ballRobot, robotTarget);

            if (InconmmingOutgoing(Model, opps.First(), ref incomningNear))
            {
                LastRB = RBstate.Robot;
                return opps.First();
            }

            LastRB = RBstate.Ball;
            return null;
        }

        #endregion

        #region comment
        //private static int? StaticRB(GameStrategyEngine engine, WorldModel Model, List<DefenceInfo> CurrentInfo)
        //{

        //    Position2D? g;
        //    var opps = engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
        //    double d1, d2;
        //    if (opps.Count > 0 && GameParameters.IsInDangerousZone(ballState.Location, false, 0.03, out d1, out d2))
        //    {
        //        if (!GameParameters.IsInDangerousZone(Model.Opponents[opps.First()].Location, false, 0.03, out d1, out d2))
        //            return opps.First();
        //        else if (opps.Count > 1)
        //        {
        //            return opps.Skip(1).First();
        //        }
        //        else
        //            return null;
        //    }
        //    if (opps.Count == 0 || ballState.Speed.Size < 0.5)
        //        return null;

        //    SingleObjectState oppstate = Model.Opponents[opps.First()];
        //    Position2D? Defendball = CalculateFirstStatic(Model, ballState, opps.First()).DefenderPosition;
        //    Position2D? Defendrobot = CalculateFirstStatic(Model, oppstate, opps.First()).DefenderPosition;

        //    Position2D temprobot = oppstate.Location + oppstate.Speed * 0.5;
        //    temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
        //    temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
        //    temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
        //    temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);
        //    SingleObjectState nextrobot = new SingleObjectState() { Type = ObjectType.Opponent, Location = temprobot, Speed = oppstate.Speed };
        //    Position2D? Defendrobotnext = CalculateFirstStatic(Model, nextrobot, opps.First()).DefenderPosition;

        //    Position2D tempball = ballState.Location + ballState.Speed * 0.5;
        //    tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
        //    tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
        //    tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
        //    tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);
        //    SingleObjectState nextball = new SingleObjectState() { Type = ObjectType.Ball, Location = tempball, Speed = ballState.Speed };
        //    Position2D? Defendballnext = CalculateFirstStatic(Model, nextball, opps.First()).DefenderPosition;

        //    if (Defendrobot.HasValue && Defendball.HasValue)
        //    {
        //        double db = Defendball.Value.DistanceFrom(Defendballnext.Value);
        //        double dr = Defendrobot.Value.DistanceFrom(Defendrobotnext.Value);
        //        if (db > dr + 0.065)
        //        {
        //            return opps.First();
        //        }
        //        else if (dr > db + 0.065)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            if (CurrentInfo != null && CurrentInfo.Any(a => a.RoleType == typeof(StaticDefender1)))
        //            {
        //                DefenceInfo dinfo = CurrentInfo.Single(s => s.RoleType == typeof(StaticDefender1));

        //                if (dinfo.TargetState.Type == ObjectType.Ball)
        //                {
        //                    return null;
        //                }
        //                else
        //                {
        //                    return opps.First();
        //                }
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //    else if (Defendrobot.HasValue)
        //    {
        //        return opps.First();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #region first_Normal_With IO RB
        //#region 1st Defender
        //private static DefenceInfo CalculateNormalFirst ( GameStrategyEngine engine , WorldModel Model , DefenderCommand Command , out DefenceInfo GoaliRes , DefenderCommand GoaliCommand , List<DefenceInfo> CurrentInfo )
        //{
        //    DefenceInfo def = new DefenceInfo ();
        //    GoaliRes = new DefenceInfo ();

        //    Position2D? goalieball;
        //    Position2D? goalierobot;
        //    Position2D? g;
        //    GoaliPositioningMode BallMode = ChooseMode ( ballState , CurrentGoalieMode );
        //    Position2D? Defendball = CalculatePos ( engine , Model , ballState , BallMode , out goalieball );
        //    if ( ballState.Speed.Size < 0.5 || !Command.OppID.HasValue || !Model.Opponents.ContainsKey ( Command.OppID.Value ) )
        //    {
        //        GoaliRes.OppID = ( Command.OppID.HasValue && Model.Opponents.ContainsKey ( Command.OppID.Value ) ) ? Command.OppID : null;
        //        if ( ( Command.OppID.HasValue && Model.Opponents.ContainsKey ( Command.OppID.Value ) ) )
        //            GoaliRes.OppID = Command.OppID;
        //        else
        //            GoaliRes.OppID = null;
        //        GoaliRes.RoleType = GoaliCommand.RoleType;
        //        GoaliRes.TargetState = ballState;
        //        GoaliRes.Mode = BallMode;
        //        GoaliRes.DefenderPosition = goalieball;

        //        if ( ( Command.OppID.HasValue && Model.Opponents.ContainsKey ( Command.OppID.Value ) ) )
        //            def.OppID = Command.OppID;
        //        else
        //            def.OppID = null;
        //        def.RoleType = Command.RoleType;
        //        def.TargetState = ballState;
        //        def.Mode = BallMode;
        //        def.DefenderPosition = Defendball;
        //    }
        //    else
        //    {
        //        SingleObjectState oppstate = Model.Opponents [Command.OppID.Value];
        //        GoaliPositioningMode RobotMode = ChooseMode ( oppstate , CurrentGoalieMode );
        //        Position2D? Defendrobot = CalculatePos ( engine , Model , oppstate , RobotMode , out goalierobot );

        //        Position2D temprobot = oppstate.Location + oppstate.Speed * 0.5;
        //        temprobot.X = Math.Max ( temprobot.X , GameParameters.OppGoalCenter.X + 0.05 );
        //        temprobot.X = Math.Min ( temprobot.X , GameParameters.OurGoalCenter.X - 0.05 );
        //        temprobot.Y = Math.Max ( temprobot.Y , GameParameters.OurRightCorner.Y + 0.05 );
        //        temprobot.Y = Math.Min ( temprobot.Y , GameParameters.OurLeftCorner.Y - 0.05 );

        //        SingleObjectState nextrobot = new SingleObjectState ()
        //        {
        //            Type = ObjectType.Opponent ,
        //            Location = temprobot ,
        //            Speed = oppstate.Speed
        //        };
        //        GoaliPositioningMode nextRobotMode = ChooseMode ( nextrobot , CurrentGoalieMode );
        //        Position2D? Defendrobotnext = CalculatePos ( engine , Model , nextrobot , nextRobotMode , out g );

        //        Position2D tempball = ballState.Location + ballState.Speed * 0.5;
        //        tempball.X = Math.Max ( tempball.X , GameParameters.OppGoalCenter.X + 0.05 );
        //        tempball.X = Math.Min ( tempball.X , GameParameters.OurGoalCenter.X - 0.05 );
        //        tempball.Y = Math.Max ( tempball.Y , GameParameters.OurRightCorner.Y + 0.05 );
        //        tempball.Y = Math.Min ( tempball.Y , GameParameters.OurLeftCorner.Y - 0.05 );
        //        SingleObjectState nextball = new SingleObjectState ()
        //        {
        //            Type = ObjectType.Ball ,
        //            Location = tempball ,
        //            Speed = ballState.Speed
        //        };
        //        GoaliPositioningMode nextBallMode = ChooseMode ( nextball , CurrentGoalieMode );
        //        Position2D? Defendballnext = CalculatePos ( engine , Model , nextball , nextBallMode , out g );

        //        if ( Defendrobot.HasValue && Defendball.HasValue )
        //        {

        //            double db = Defendball.Value.DistanceFrom ( Defendballnext.Value );
        //            double dr = Defendrobot.Value.DistanceFrom ( Defendrobotnext.Value );
        //            if ( db > dr + 0.065 )
        //            {
        //                GoaliRes.OppID = Command.OppID;
        //                GoaliRes.RoleType = GoaliCommand.RoleType;
        //                GoaliRes.TargetState = oppstate;
        //                GoaliRes.Mode = RobotMode;
        //                GoaliRes.DefenderPosition = goalierobot;

        //                def.OppID = Command.OppID;
        //                def.RoleType = Command.RoleType;
        //                def.TargetState = oppstate;
        //                def.Mode = RobotMode;
        //                def.DefenderPosition = Defendrobot;
        //            }
        //            else if ( dr > db + 0.065 )
        //            {
        //                GoaliRes.OppID = Command.OppID;
        //                GoaliRes.RoleType = GoaliCommand.RoleType;
        //                GoaliRes.TargetState = ballState;
        //                GoaliRes.Mode = BallMode;
        //                GoaliRes.DefenderPosition = goalieball;

        //                def.OppID = Command.OppID;
        //                def.RoleType = Command.RoleType;
        //                def.TargetState = ballState;
        //                def.Mode = BallMode;
        //                def.DefenderPosition = Defendball;
        //            }
        //            else
        //            {
        //                if ( CurrentInfo != null && CurrentInfo.Any ( a => a.RoleType == Command.RoleType ) )
        //                {
        //                    DefenceInfo dinfo = CurrentInfo.Single ( s => s.RoleType == Command.RoleType );

        //                    if ( dinfo.TargetState.Type == ObjectType.Ball )
        //                    {
        //                        GoaliRes.OppID = Command.OppID;
        //                        GoaliRes.RoleType = GoaliCommand.RoleType;
        //                        GoaliRes.TargetState = ballState;
        //                        GoaliRes.Mode = BallMode;
        //                        GoaliRes.DefenderPosition = goalieball;

        //                        def.OppID = Command.OppID;
        //                        def.RoleType = Command.RoleType;
        //                        def.TargetState = ballState;
        //                        def.Mode = BallMode;
        //                        def.DefenderPosition = Defendball;
        //                    }
        //                    else
        //                    {
        //                        GoaliRes.OppID = Command.OppID;
        //                        GoaliRes.RoleType = GoaliCommand.RoleType;
        //                        GoaliRes.TargetState = oppstate;
        //                        GoaliRes.Mode = RobotMode;
        //                        GoaliRes.DefenderPosition = goalierobot;

        //                        def.OppID = Command.OppID;
        //                        def.RoleType = Command.RoleType;
        //                        def.TargetState = oppstate;
        //                        def.Mode = RobotMode;
        //                        def.DefenderPosition = Defendrobot;
        //                    }
        //                }
        //                else
        //                {
        //                    GoaliRes.OppID = Command.OppID;
        //                    GoaliRes.RoleType = GoaliCommand.RoleType;
        //                    GoaliRes.TargetState = ballState;
        //                    GoaliRes.Mode = BallMode;
        //                    GoaliRes.DefenderPosition = goalieball;

        //                    def.OppID = Command.OppID;
        //                    def.RoleType = Command.RoleType;
        //                    def.TargetState = ballState;
        //                    def.Mode = BallMode;
        //                    def.DefenderPosition = Defendball;
        //                }
        //            }
        //        }
        //        else if ( Defendrobot.HasValue )
        //        {
        //            GoaliRes.OppID = Command.OppID;
        //            GoaliRes.RoleType = GoaliCommand.RoleType;
        //            GoaliRes.TargetState = oppstate;
        //            GoaliRes.Mode = RobotMode;
        //            GoaliRes.DefenderPosition = goalierobot;

        //            def.OppID = Command.OppID;
        //            def.RoleType = Command.RoleType;
        //            def.TargetState = oppstate;
        //            def.Mode = RobotMode;
        //            def.DefenderPosition = Defendrobot;
        //        }
        //        else
        //        {
        //            GoaliRes.OppID = Command.OppID;
        //            GoaliRes.RoleType = GoaliCommand.RoleType;
        //            GoaliRes.TargetState = ballState;
        //            GoaliRes.Mode = BallMode;
        //            GoaliRes.DefenderPosition = goalieball;

        //            def.OppID = Command.OppID;
        //            def.RoleType = Command.RoleType;
        //            def.TargetState = ballState;
        //            def.Mode = BallMode;
        //            def.DefenderPosition = Defendball;
        //        }
        //    }
        //    GoaliRes.Teta = ( GoaliRes.TargetState.Location - GoaliRes.DefenderPosition.Value ).AngleInDegrees;
        //    return def;
        //}
        //#endregion
        //#region 2nd defender
        //private static DefenceInfo CalculateNormalSecond ( GameStrategyEngine engine , WorldModel Model , DefenderCommand Command , out DefenceInfo GoaliRes , List<DefenceInfo> CurrentInfo )
        //{
        //    DefenceInfo def = new DefenceInfo ();
        //    GoaliRes = new DefenceInfo ();

        //    Position2D? goalieball;
        //    Position2D? goalierobot;
        //    Position2D? g;
        //    GoaliPositioningMode BallMode = ChooseMode ( ballState , CurrentGoalieMode );
        //    Position2D? Defendball = CalculatePos ( engine , Model , ballState , BallMode , out goalieball );
        //    if ( !Command.OppID.HasValue || !Model.Opponents.ContainsKey ( Command.OppID.Value ) )
        //    {
        //        GoaliRes.OppID = null;
        //        GoaliRes.TargetState = ballState;
        //        GoaliRes.Mode = BallMode;
        //        GoaliRes.DefenderPosition = goalieball;

        //        def.OppID = null;
        //        def.RoleType = Command.RoleType;
        //        def.TargetState = ballState;
        //        def.Mode = BallMode;
        //        def.DefenderPosition = Defendball;
        //    }
        //    else
        //    {
        //        SingleObjectState oppstate = Model.Opponents [Command.OppID.Value];
        //        GoaliPositioningMode RobotMode = ChooseMode ( oppstate , CurrentGoalieMode );
        //        Position2D? Defendrobot = CalculatePos ( engine , Model , oppstate , RobotMode , out goalierobot );

        //        GoaliRes.OppID = Command.OppID.Value;
        //        GoaliRes.TargetState = oppstate;
        //        GoaliRes.Mode = RobotMode;
        //        GoaliRes.DefenderPosition = goalierobot;

        //        def.OppID = Command.OppID.Value;
        //        def.RoleType = Command.RoleType;
        //        def.TargetState = oppstate;
        //        def.Mode = RobotMode;
        //        def.DefenderPosition = Defendrobot;
        //    }

        //    return def;
        //}
        //#endregion
        #endregion
        #endregion

        static int? lastopp = null;

        static bool incomningNear = false;
        private static double extendStatticDefenceTarget = 0;


        private static bool BallKickedToGoal(WorldModel Model)
        {
            Line line = new Line();
            line = new Line(ballStateFast.Location, ballStateFast.Location - ballStateFast.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballStateFast.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.15 && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.15)
                if (ballStateFast.Speed.InnerProduct(GameParameters.OurGoalRight - ballStateFast.Location) > 0)
                    if (ballStateFast.Speed.Size > 0.1 && d / ballStateFast.Speed.Size < 2.2)
                        return true;
            return false;
        }

        public static Position2D Dive(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D pos = new Position2D();
            Position2D robotLoc = Model.OurRobots[RobotID].Location;
            Position2D ballLoc = ballStateFast.Location;
            Vector2D ballSpeed = ballStateFast.Speed;
            Position2D prep = ballSpeed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location);
            double dist, DistFromBorder, R;
            if (GameParameters.IsInDangerousZone(prep, false, 0, out dist, out DistFromBorder))
            {
                R = GameParameters.SafeRadi(new SingleObjectState(prep, new Vector2D(), 0), .05 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .05)));
                pos = GameParameters.OurGoalCenter - ballSpeed.GetNormalizeToCopy(R);
            }
            else
                pos = prep;
            return pos;
        }

        public static Position2D GetBackBallPoint(WorldModel Model, int RobotID, out double Teta)
        {
            Vector2D vec = ballState.Location - GameParameters.OurGoalCenter;
            Position2D tar = ballState.Location + vec;
            Vector2D ballSpeed = ballState.Speed;
            Position2D ballLocation = ballState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = tar - ballLocation;
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(0.09);
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;
            double segmentConst = 0.7;
            double rearDistance = 0.15;
            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            Position2D finalPosToGo = midPoint;
            double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

            if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
                finalPosToGo = backBallPoint;
            else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }

            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);
            Teta = (vec).AngleInDegrees;
            return finalPosToGo;
        }

        public static void RestartActiveFlags()
        {
            SwitchToActiveMarker1 = false;
            SwitchToActiveMarker2 = false;
            SwitchToActiveMarker3 = false;
            SwitchDefender2Marker1 = false;
            SwitchDefender2Marker2 = false;
            SwitchDefender2Marker3 = false;
            SwitchDefender32Marker1 = false;
            SwitchDefender32Marker2 = false;
            SwitchDefender32Marker3 = false;
            SwitchDefender42Marker1 = false;
            SwitchDefender42Marker2 = false;
            SwitchDefender42Marker3 = false;
        }

        public static void SwitchToActiveReset()
        {
            DefenderCornerRole1ToActive = false;
            DefenderCornerRole2ToActive = false;
            DefenderCornerRole3ToActive = false;
            DefenderCornerRole4ToActive = false;
            DefenderMarkerRole1ToActive = false;
            DefenderMarkerRole2ToActive = false;
            DefenderMarkerRole3ToActive = false;
            DefenderRegionalRole1ToActive = false;
            DefenderRegionalRole2ToActive = false;
            DefenderGoToPointToActive = false;
        }
        #endregion
#endif

    }
}