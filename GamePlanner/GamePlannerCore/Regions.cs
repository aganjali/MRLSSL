using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.Planning.GamePlanner
{
    public partial class Regions : IDisposable
    {
        public GPosition2D topLeft;
        public GVector2D Size;
        public Regions()
        {
        
            AllocateVariables();
        }
        private void AllocateVariables()
        {
            topLeft = new GPosition2D(0, 0.8f * (float)MRL.SSL.GameDefinitions.GameParameters.OppLeftCorner.Y);
            Size = new GVector2D(0.8f * (float)MRL.SSL.GameDefinitions.GameParameters.OppGoalCenter.X, 0.8f * (float)(MRL.SSL.GameDefinitions.GameParameters.OppRightCorner.Y - MRL.SSL.GameDefinitions.GameParameters.OppLeftCorner.Y));

        }
      
        public void CalculateRegions(List<WorldModel> model, GVector2D GlobalBallSpeed, Position2D ballPassFrom, ref GamePlannerInfo GPInfo, GamePlannerInfo LastGPInfo)
        {

            List<VisibleGoalInterval> ourGoalIntervals, oppGoalIntervals;
            CalculateGoalIntervals(model, out ourGoalIntervals, out oppGoalIntervals, true, true);
            GPInfo.OurGoalIntervals = ourGoalIntervals;
            GPInfo.OppGoalIntervals = oppGoalIntervals;
        }
        public void CalculateGoalIntervals(List<WorldModel> model, out List<VisibleGoalInterval> ourGoalIntervals, out List<VisibleGoalInterval> oppGoalIntervals, bool useOpp, bool useOur)
        {

            WorldModel Model = model.Last();
            ourGoalIntervals = GetVisibleGoalIntervals2(Model, Model.BallState.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, useOpp, useOur, null);
            oppGoalIntervals = GetVisibleGoalIntervals2(Model, Model.BallState.Location, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, useOpp, useOur, null);

        }
        public List<VisibleGoalInterval> GetVisibleIntervals(WorldModel Model, Position2D From, Position2D Start, Position2D End, bool useOpp, bool useOur, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {

            List<VisibleGoalInterval> res = GetVisibleGoalIntervals2(Model, From, Start, End, useOpp, useOur, OpponentIDToExclude, robotIDsToExclude);
            return res;
        }
        public List<VisibleGoalInterval> GetVisibleIntervals(WorldModel Model, Position2D From, Position2D Start, Position2D End, double ang, bool useOpp, bool useOur, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {

                List<VisibleGoalInterval> res = GetVisibleGoalIntervals2(Model, From, Start, End, ang, useOpp, useOur, OpponentIDToExclude, robotIDsToExclude);
                return res;
        }
        public List<VisibleGoalInterval> GetVisibleIntervals(WorldModel Model, Position2D From, Position2D Start, Position2D End, bool useOpp, bool useOur, int? OpponentIDToExclude, List<Position2D?> Pos2Exclude)
        {

            List<VisibleGoalInterval> res = GetVisibleGoalIntervals2(Model, From, Start, End, useOpp, useOur, OpponentIDToExclude, Pos2Exclude);
            return res;
        }
        public Position2D? GetAGoodTargetPointInGoal(WorldModel Model, Position2D? LastSelectedTargetPoint, Vector2D v, Position2D FromLocation, out double goodness, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {

            List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();


            intervals = GetVisibleIntervals(Model, FromLocation, GoalStart, GoalEnd, UseOpponents, UseOurRobots, OpponentIDToExclude, robotIDsToExclude);
            if (LastSelectedTargetPoint.HasValue)
            {

                foreach (VisibleGoalInterval item in intervals)
                {

                    goodness = (item.interval.End - item.interval.Start) * 2 / (Math.Abs(GoalEnd.Y - GoalStart.Y));
                    if (item.interval.Contains((float)LastSelectedTargetPoint.Value.Y))
                        return Position2D.Interpolate(LastSelectedTargetPoint.Value, new Position2D(GoalStart.X, (item.interval.Start + item.interval.End) / 2), 0.01);

                }
            }
            VisibleGoalInterval? Lasrgest = null;
            double LargesWeight = double.MinValue;
            foreach (VisibleGoalInterval item in intervals)
            {
                double w = Math.Abs(item.interval.End - item.interval.Start);
                if (w > LargesWeight)
                {
                    LargesWeight = w;
                    Lasrgest = item;
                }
            }
            if (Lasrgest.HasValue)
            {
                goodness = (Lasrgest.Value.interval.End - Lasrgest.Value.interval.Start) * 2 / (Math.Abs(GoalEnd.Y - GoalStart.Y));
                return new Position2D(GoalStart.X, (Lasrgest.Value.interval.Start + Lasrgest.Value.interval.End) / 2);

            }
            else
            {
                goodness = 0;
                return null;
            }
        }
        public Position2D? GetAGoodTargetPointInGoal(WorldModel Model, Position2D? LastSelectedTargetPoint, Position2D FromLocation, out double goodness, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {

            List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();
            Vector2D vec = GoalEnd - GoalStart;
            Vector2D refrence = new Vector2D(0, 1);
            double ang = -Vector2D.AngleBetweenInRadians(vec, refrence);
            Vector2D goalVec = vec.Size * refrence;

            Vector2D fromStartVec = FromLocation - GoalStart;
            Vector2D fromEndVec = FromLocation - GoalEnd;

            Vector2D tmpFromStartGoal = Vector2D.FromAngleSize(fromStartVec.AngleInRadians + ang, fromStartVec.Size);
            Vector2D tmpFromEndGoal = Vector2D.FromAngleSize(fromEndVec.AngleInRadians + ang, fromEndVec.Size);

            Position2D tmpGoalEnd = GoalStart + goalVec;
            Position2D tmpFromLoc = tmpGoalEnd + tmpFromEndGoal;
            intervals = GetVisibleIntervals(Model, tmpFromLoc, GoalStart, tmpGoalEnd, ang, UseOpponents, UseOurRobots, OpponentIDToExclude, robotIDsToExclude);
            if (LastSelectedTargetPoint.HasValue)
            {
                Vector2D tmp = (LastSelectedTargetPoint.Value - GoalStart);
                Position2D tmpLastTarget = GoalStart + Vector2D.FromAngleSize(tmp.AngleInRadians + ang, tmp.Size);
                foreach (VisibleGoalInterval item in intervals)
                {
                    Position2D pos = new Position2D();
                    goodness = (item.interval.End - item.interval.Start) * 2 / (Math.Abs(tmpGoalEnd.Y - GoalStart.Y));
                    if (item.interval.Contains((float)tmpLastTarget.Y))
                    {
                        pos = Position2D.Interpolate(tmpLastTarget, new Position2D(GoalStart.X, (item.interval.Start + item.interval.End) / 2), 0.01);
                        Vector2D tmp2 = (pos - GoalStart);
                        pos = GoalStart + Vector2D.FromAngleSize(tmp2.AngleInRadians - ang, tmp2.Size);
                        return pos;
                    }
                }
            }
            VisibleGoalInterval? Lasrgest = null;
            double LargesWeight = double.MinValue;
            foreach (VisibleGoalInterval item in intervals)
            {
                double w = Math.Abs(item.interval.End - item.interval.Start);
                if (w > LargesWeight)
                {
                    LargesWeight = w;
                    Lasrgest = item;
                }
            }
            if (Lasrgest.HasValue)
            {
                goodness = (Lasrgest.Value.interval.End - Lasrgest.Value.interval.Start) * 2 / (Math.Abs(tmpGoalEnd.Y - GoalStart.Y));
                Position2D pos = new Position2D(GoalStart.X, (Lasrgest.Value.interval.Start + Lasrgest.Value.interval.End) / 2);
                Vector2D tmp = (pos - GoalStart);
                pos = GoalStart + Vector2D.FromAngleSize(tmp.AngleInRadians - ang, tmp.Size);
                return pos;
            }
            else
            {
                goodness = 0;
                return null;
            }
        }
        public Position2D? GetAGoodTargetPointInGoal(WorldModel Model, Position2D? LastSelectedTargetPoint, Position2D FromLocation, out double goodness, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, List<Position2D?> Pos2Exclude)
        {

                List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();
                intervals = GetVisibleIntervals(Model, FromLocation, GoalStart, GoalEnd, UseOpponents, UseOurRobots, OpponentIDToExclude, Pos2Exclude);
                if (LastSelectedTargetPoint.HasValue)
                {
                    foreach (VisibleGoalInterval item in intervals)
                    {
                        goodness = (item.interval.End - item.interval.Start) * 2 / (Math.Abs(GoalEnd.Y - GoalStart.Y));
                        if (item.interval.Contains((float)LastSelectedTargetPoint.Value.Y))
                            return Position2D.Interpolate(LastSelectedTargetPoint.Value, new Position2D(GoalStart.X, (item.interval.Start + item.interval.End) / 2), 0.01);
                    }
                }
                VisibleGoalInterval? Lasrgest = null;
                double LargesWeight = double.MinValue;
                foreach (VisibleGoalInterval item in intervals)
                {
                    double w = Math.Abs(item.interval.End - item.interval.Start);
                    if (w > LargesWeight)
                    {
                        LargesWeight = w;
                        Lasrgest = item;
                    }
                }
                if (Lasrgest.HasValue)
                {
                    goodness = (Lasrgest.Value.interval.End - Lasrgest.Value.interval.Start) * 2 / (Math.Abs(GoalEnd.Y - GoalStart.Y));
                    return new Position2D(GoalStart.X, (Lasrgest.Value.interval.Start + Lasrgest.Value.interval.End) / 2);
                }
                else
                {
                    goodness = 0;
                    return null;
                }
        }
        private List<VisibleGoalInterval> GetVisibleGoalIntervals2(WorldModel Model, Position2D FromLocation, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {
            List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();
            Position2D goalCenter = Position2D.Interpolate(GoalStart, GoalEnd, 0.5);
            if (Model == null)
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), 1));
            }
            else
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), (float)(GoalEnd - GoalStart).Size * (float)Math.Sin(Math.Abs(Vector2D.AngleBetweenInRadians(GoalEnd - GoalStart, goalCenter - FromLocation)))));
                Vector2D centerDirection = goalCenter - FromLocation;
                if (UseOpponents)
                    if (Model.Opponents != null)
                        foreach (int oppID in Model.Opponents.Keys)
                            if (oppID != OpponentIDToExclude)
                                if (centerDirection.InnerProduct(Model.Opponents[oppID].Location - FromLocation) > 0)
                                    ExcludeObstacle2(intervals, new Circle(Model.Opponents[oppID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));

                if (UseOurRobots)
                {
                    List<int> robotIDsToExcludeList = new List<int>(robotIDsToExclude);
                    foreach (int RobotID in Model.OurRobots.Keys)
                        if (!robotIDsToExcludeList.Contains(RobotID))
                            if (centerDirection.InnerProduct(Model.OurRobots[RobotID].Location - FromLocation) > 0)
                                ExcludeObstacle2(intervals, new Circle(Model.OurRobots[RobotID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));
                }
            }

            return intervals;
        }
        private List<VisibleGoalInterval> GetVisibleGoalIntervals2(WorldModel Model, Position2D FromLocation, Position2D GoalStart, Position2D GoalEnd, double ang, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {
            List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();
            Position2D pos = new Position2D();
            Position2D goalCenter = Position2D.Interpolate(GoalStart, GoalEnd, 0.5);
            if (Model == null)
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), 1));
            }
            else
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), (float)(GoalEnd - GoalStart).Size * (float)Math.Sin(Math.Abs(Vector2D.AngleBetweenInRadians(GoalEnd - GoalStart, goalCenter - FromLocation)))));
                Vector2D centerDirection = goalCenter - FromLocation;
                if (UseOpponents)
                    if (Model.Opponents != null)
                        foreach (int oppID in Model.Opponents.Keys)
                            if (oppID != OpponentIDToExclude)
                            {
                                Vector2D tmp = Model.Opponents[oppID].Location - GoalStart;
                                pos = GoalStart + Vector2D.FromAngleSize(tmp.AngleInRadians + ang, tmp.Size);
                                if (centerDirection.InnerProduct(pos - FromLocation) > 0)
                                    ExcludeObstacle2(intervals, new Circle(pos, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));
                            }

                if (UseOurRobots)
                {
                    List<int> robotIDsToExcludeList = new List<int>(robotIDsToExclude);
                    foreach (int RobotID in Model.OurRobots.Keys)
                        if (!robotIDsToExcludeList.Contains(RobotID))
                        {
                            Vector2D tmp = Model.OurRobots[RobotID].Location - GoalStart;
                            pos = GoalStart + Vector2D.FromAngleSize(tmp.AngleInRadians + ang, tmp.Size);
                            if (centerDirection.InnerProduct(pos - FromLocation) > 0)
                                ExcludeObstacle2(intervals, new Circle(pos, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));
                        }
                }
            }

            return intervals;
        }
        private List<VisibleGoalInterval> GetVisibleGoalIntervals2(WorldModel Model, Position2D FromLocation, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, List<Position2D?> Pos2Exclude)
        {
            List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();
            Position2D goalCenter = Position2D.Interpolate(GoalStart, GoalEnd, 0.5);
            if (Model == null)
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), 1));
            }
            else
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), (float)(GoalEnd - GoalStart).Size * (float)Math.Sin(Math.Abs(Vector2D.AngleBetweenInRadians(GoalEnd - GoalStart, goalCenter - FromLocation)))));
                Vector2D centerDirection = goalCenter - FromLocation;
                if (UseOpponents)
                    if (Model.Opponents != null)
                        foreach (int oppID in Model.Opponents.Keys)
                            if (oppID != OpponentIDToExclude)
                                if (centerDirection.InnerProduct(Model.Opponents[oppID].Location - FromLocation) > 0)
                                    ExcludeObstacle2(intervals, new Circle(Model.Opponents[oppID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));

                if (UseOurRobots)
                {
                    foreach (int RobotID in Model.OurRobots.Keys)
                        if (centerDirection.InnerProduct(Model.OurRobots[RobotID].Location - FromLocation) > 0)
                            ExcludeObstacle2(intervals, new Circle(Model.OurRobots[RobotID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));
                }
                foreach (var Pos in Pos2Exclude)
                    if (Pos.HasValue && centerDirection.InnerProduct(Pos.Value - FromLocation) > 0)
                        ExcludeObstacle2(intervals, new Circle(Pos.Value, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));
            }

            return intervals;
        }
        void ExcludeObstacle2(List<VisibleGoalInterval> intervals, Circle obstacle, Position2D fromLocation, Vector2D centerDirection, Position2D goalCenter, Line goalLine)
        {
            if (intervals.Count == 0)
                return;
            List<Line> tangentLines;
            List<Position2D> tangentPoints;
            int tangents = obstacle.GetTangent(fromLocation, out tangentLines, out tangentPoints);
            Interval toExclude;
            if (tangents == 2)
                toExclude = new Interval(
                    GetExtreme2(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], true, goalLine),
                    GetExtreme2(fromLocation, tangentPoints[1], goalCenter, tangentLines[1], true, goalLine));
            else if (tangents == 1)
                toExclude = new Interval(
                    GetExtreme2(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], true, goalLine),
                    GetExtreme2(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], false, goalLine)
                    );
            else //tangents == 0
            {
                Line l = new Line(fromLocation, obstacle.Center).PerpenducilarLineToPoint(fromLocation);
                toExclude = new Interval(
                    GetExtreme2(fromLocation, fromLocation, goalCenter, l, true, goalLine),
                    GetExtreme2(fromLocation, fromLocation, goalCenter, l, false, goalLine)
                    );
            }
            int i = 0;
            while (i < intervals.Count && intervals[i].interval.End <= toExclude.Start)
                i++;
            if (i < intervals.Count)
                if (intervals[i].interval.Start < toExclude.Start)
                {
                    double temp = intervals[i].interval.End;
                    intervals[i] = new VisibleGoalInterval(new Interval(intervals[i].interval.Start, toExclude.Start), intervals[i].ViasibleWidth);
                    i++;
                    if (temp > toExclude.End)
                    {
                        intervals.Insert(i, new VisibleGoalInterval(new Interval((float)toExclude.End, (float)temp), 0));
                        i++;
                    }
                }
            while (i < intervals.Count && intervals[i].interval.End < toExclude.End)
                intervals.RemoveAt(i);
            if (i < intervals.Count && intervals[i].interval.Start < toExclude.End)
                intervals[i] = new VisibleGoalInterval(new Interval(toExclude.End, intervals[i].interval.End), intervals[i].ViasibleWidth);
        }
        float GetExtreme2(GPosition2D fromLocation, GPosition2D tangentPoint, GPosition2D goalCenter, GLine l, bool Pos, GLine goalLine)
        {
            GVector2D vect = tangentPoint.Sub(fromLocation);
            if (vect.SquareSize() == 0)
                vect = (Pos ? new GVector2D(l.B, l.A) : new GVector2D(-l.B, -l.A));
            if (sign(vect.X) == sign(goalCenter.X - fromLocation.X))
            {
                bool HasValue;
                GPosition2D pos = goalLine.IntersectWithLine(l, out HasValue);
                if (HasValue)
                    return pos.Y;
            }
            if (vect.Y < 0)
                return -1000;
            else
                return 1000;
        }
        static float sign(float x)
        {
            return ((x == 0) ? 0 : (x / (float)Math.Abs(x)));
        }
        public void Dispose()
        {

        }
    }
}
