using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using KDTreeDLL;
using System.Threading;

namespace MRL.SSL.Planning.MotionPlanner
{
    public class ERRT:IDisposable
    {
        public bool Finished = false;
        Obstacles obstacles = new Obstacles();
        private const double goalProbbality = 0.2;
        private const double wayPointProbbality = 0.4;
        private const double distanceTresh = 0.1;
        private const int numWayPoints = 30;
        private const double extendSize = 0.15;
        public const int maxNodes = 100;
        public const int maxNodes2Try = 500;
        public List<SingleObjectState> Path = new List<SingleObjectState>();
        private SingleObjectState[] WayPoints = new SingleObjectState[numWayPoints];
        public AutoResetEvent eventR = new AutoResetEvent(false);
        public AutoResetEvent eventFinish = new AutoResetEvent(false);
        Random rand = new Random();
        bool useERrrt = true;
        public int AvoidBall = 1, AvoidZone = 1, AvoidOppZone = 0, AvoidRobot = 1;
        bool first = true;
        public bool UseERrrt
        {
            get { return useERrrt; }
            set { useERrrt = value; }
        }
        public Thread FindPathThread;
        WorldModel Model;
        int RobotID = 0;
        SingleObjectState Init = new SingleObjectState();
        Dictionary<int, bool> Intersects = new Dictionary<int, bool>();
        List<Line> Lines = new List<Line>(); 
        SingleObjectState Goal = new SingleObjectState();
        List<SingleObjectState> lastPath = null;
        bool StopBall = false;
        PathType pathType = PathType.Safe;
        public bool Start = false;
        public float Time = 0;
        KDTree tree = new KDTree(2);
        public float[] FPath = new float[2 * maxNodes];
        public int PathCount = 0;
        public ERRT(bool useExtendedRRT)
        {
            useERrrt = useExtendedRRT;
            Finished = false;
            first = true;
            FindPathThread = new Thread(new ThreadStart(FindPath));
            FindPathThread.Start();
        }
        public Obstacles Obstacles
        {
            get { return obstacles; }
            set { obstacles = value; }
        }
        public SingleObjectState RandomState()
        {
            return new SingleObjectState(new Position2D(MotionPlannerParameters.FieldLength_H - MotionPlannerParameters.FieldLength * rand.NextDouble(), MotionPlannerParameters.FieldWidth_H - MotionPlannerParameters.FieldWidth * rand.NextDouble()), Vector2D.Zero, null);
        }
        public SingleObjectState ChoosTarget(SingleObjectState goal, SingleObjectState[] wayPoints)
        {
            double r = rand.NextDouble();
            if (r < goalProbbality)
                return goal;
            else if (r < (wayPointProbbality + goalProbbality) && useERrrt)
            {
                int l = rand.Next() % numWayPoints;
                if (wayPoints[l] != null)
                    return wayPoints[l];

            }
            return RandomState();
        }
        public SingleObjectState Extend(SingleObjectState Nearest, SingleObjectState target, Obstacles obs,List<Line> lines)
        {
            Vector2D t = target.Location - Nearest.Location;
            double l = t.Size;
            if (l > extendSize)
                t.Scale(extendSize / l);
            Position2D tt = Nearest.Location + t;
            SingleObjectState res = new SingleObjectState(ObjectType.OurRobot, tt, Vector2D.Zero, Vector2D.Zero, null, null);
            bool b = false;
            Line li = new Line(Nearest.Location, res.Location);
            Position2D tmp = Position2D.Zero;
            foreach (var item in lines)
            {
                if (li.IntersectWithLine(item, ref tmp) && (tmp - li.Head).InnerProduct(li.Tail - li.Head) >= 0 && (tmp - li.Tail).InnerProduct(li.Head - li.Tail) >= 0 && (tmp - item.Tail).InnerProduct(item.Head - item.Tail) >= 0 && (tmp - item.Head).InnerProduct(item.Tail - item.Head) >= 0)
                {
                    b = true;
                    break;
                }
            }
            if (b || obs.Meet(Nearest, res, MotionPlannerParameters.RobotRadi))
                return null;
            else
                return res;
        }

        public void Run(WorldModel model, int robotID, List<Line> lines, SingleObjectState InitileState, SingleObjectState GoalState, int avoidBall, int avoidZone, int avoidOppZone, int avoidRobot, List<SingleObjectState> LastPath, PathType Type, bool stopBall)
        {

            Model = model;
            RobotID = robotID;
            Init = InitileState;
            Goal = GoalState;
            Lines = lines;
            pathType = Type;
            AvoidBall = avoidBall;
            AvoidRobot = avoidRobot;
            AvoidZone = avoidZone;
            AvoidOppZone = avoidOppZone;
            lastPath = LastPath;
            StopBall = stopBall;
            eventR.Set();
        }
        public void Run(bool Set, bool stopBall)
        {
            StopBall = stopBall;
            if (Set)
                eventFinish.Set();
        }
        //IMPORTANT NOTE:The Generated Path Must has at least 2 state: first State is the init state and last state is the goal state
        private void FindPath()
        {
            
            while (true)
            {
                if (first)
                {
                    first = false;
                    eventFinish.Set();
                    eventR.WaitOne();
                }
                if (useERrrt && lastPath != null && lastPath.Count > 0)
                {
                    for (int i = 0; i < numWayPoints; i++)
                    {
                        int j = rand.Next() % lastPath.Count;
                        WayPoints[i] = lastPath[j];
                        WayPoints[i].ParentState = null;
                    }
                } 
                //if (RobotID == 2)
                //{
                //    ;
                //}
                PathCount = 0;
                SingleObjectState init = new SingleObjectState(Init);
                SingleObjectState goal = new SingleObjectState(Goal);

                SingleObjectState target, Extended;
                tree = new KDTree(2);
                Obstacles obs = new Obstacles(Model);

                obs.AddObstacle(AvoidRobot, AvoidBall, AvoidZone, AvoidOppZone, new List<int>() { RobotID }, null, MotionPlannerParameters.kSpeedBall, MotionPlannerParameters.kSpeedRobot, StopBall);
                List<SingleObjectState> res = new List<SingleObjectState>();
                Init.ParentState = null;

                List<int> mustRemove;
                CheckInitialStates(init, goal, obs, pathType, out mustRemove);
                foreach (var item in mustRemove)
                {
                    if (obs.ObstaclesList.ContainsKey(item))
                        obs.ObstaclesList.Remove(item);
                }
                SingleObjectState NearestState = new SingleObjectState(init);
                bool b = false;
                Line li = new Line(init.Location, goal.Location);
                Position2D tmp = Position2D.Zero;

                foreach (var item in Lines)
                {
                    if (li.IntersectWithLine(item, ref tmp) && (tmp - li.Head).InnerProduct(li.Tail - li.Head) >= 0 && (tmp - li.Tail).InnerProduct(li.Head - li.Tail) >= 0 && (tmp - item.Tail).InnerProduct(item.Head - item.Tail) >= 0 && (tmp - item.Head).InnerProduct(item.Tail - item.Head) >= 0)
                    {
                        b = true;
                        break;
                    }
                }
                if (!b && !obs.Meet(init, goal, MotionPlannerParameters.RobotRadi))
                {
                    res.Add(goal);
                    FPath[2 * PathCount] = (float)goal.Location.X;
                    FPath[2 * PathCount + 1] = (float)goal.Location.Y;
                    PathCount++;
                    if (init.Location != Init.Location)
                    {
                        res.Add(init);
                        FPath[2 * PathCount] = (float)init.Location.X;
                        FPath[2 * PathCount + 1] = (float)init.Location.Y;
                        PathCount++;
                    }
                    FPath[2 * PathCount] = (float)Init.Location.X;
                    FPath[2 * PathCount + 1] = (float)Init.Location.Y;
                    PathCount++;
                    res.Add(Init);
                    Path = res;

                }
                else
                {
                    int nodes2try = 0;
                    do
                    {
                        double[] d = { init.Location.X, init.Location.Y };
                        init.ParentState = null;
                        if (nodes2try >= maxNodes)
                        {
                            tree = new KDTree(2);
                            NearestState = new SingleObjectState(init);
                            obs.Clear();
                            obs.AddObstacle(0, 0, 1, 0, null, null, StopBall);
                        }
                        tree.insert(d, init);
                        nodes2try = 0;
                        while (NearestState.Location.DistanceFrom(goal.Location) > 0.1 && tree.Count < maxNodes && nodes2try < maxNodes2Try)
                        {
                            nodes2try++;
                            target = ChoosTarget(goal, WayPoints);
                            if (tree.Count > 0)
                            {
                                double[] d2 = { target.Location.X, target.Location.Y };
                                NearestState = tree.nearest(d2);
                            }
                            Extended = Extend(NearestState, target, obs, Lines);
                            if (Extended != null)
                            {
                                if (Extended.Location == goal.Location)
                                    break;

                                double[] ed = { Extended.Location.X, Extended.Location.Y };

                                SingleObjectState sos = tree.search(ed);
                                if (sos == null)
                                {
                                    Extended.ParentState = NearestState;
                                    tree.insert(ed, Extended);
                                }
                            }
                        }
                    } while ((NearestState.Location != goal.Location && nodes2try >= maxNodes2Try && obs.MeetDangerZone(NearestState, goal, MotionPlannerParameters.RobotRadi)));


                    if (NearestState.Location != goal.Location)
                    {
                        goal.ParentState = NearestState;
                        NearestState = new SingleObjectState(goal);
                        NearestState.ParentState = goal.ParentState;
                    }

                    while (NearestState != null)
                    {
                        FPath[2 * PathCount] = (float)NearestState.Location.X;
                        FPath[2 * PathCount + 1] = (float)NearestState.Location.Y;
                        PathCount++;
                        res.Add(NearestState);
                        NearestState = NearestState.ParentState;
                    }


                    if (Init.Location != init.Location)
                    {
                        FPath[2 * PathCount] = (float)Init.Location.X;
                        FPath[2 * PathCount + 1] = (float)Init.Location.Y;
                        PathCount++;
                        res.Add(Init);
                    }
                    Path = res;
                }
                obstacles = obs;
                Finished = true;
                eventFinish.Set();
                eventR.WaitOne();
            }
        }
        private void CheckInitialStates(SingleObjectState Init, SingleObjectState Goal, Obstacles obs, PathType pathType, out List<int> mustRemoveObstacles)
        {
            bool goalinzone = false;
            double d1 = 0, d2 = 0, distanceInDangerZone;
            Vector2D Goal2OurVec = new Vector2D() ;
            if (GameParameters.IsInDangerousZone(Goal.Location, false, 0.1, out distanceInDangerZone, out d1) && AvoidZone != 0)
            {
                Goal.Location.X = Math.Min(GameParameters.OurGoalCenter.X , Goal.Location.X);
                Goal2OurVec = Goal.Location - GameParameters.OurGoalCenter;
                //double safeR = GameParameters.SafeRadi(Goal, MotionPlannerParameters.DangerZoneW - GameParameters.DefenceareaRadii + 0.01);
                Goal2OurVec.NormalizeTo(d1 + MotionPlannerParameters.GoalExtendFromDangerZoneMargin); 
                Goal.Location = Goal2OurVec + GameParameters.OurGoalCenter;
                goalinzone = true;
            }
            Vector2D Init2OurVec = new Vector2D() ;
            if (GameParameters.IsInDangerousZone(Init.Location, false, 0.1, out distanceInDangerZone, out d2) && AvoidZone != 0)
            {
                Init.Location.X = Math.Min(GameParameters.OurGoalCenter.X, Init.Location.X);
                Init2OurVec = Init.Location - GameParameters.OurGoalCenter;
                Init2OurVec.NormalizeTo(d2 + 0.16);
                Init.Location = Init2OurVec + GameParameters.OurGoalCenter;
            }

            if (GameParameters.IsInDangerousZone(Goal.Location, true, 0.25, out distanceInDangerZone, out d1) && AvoidOppZone != 0)
            {
                Goal.Location.X = Math.Max(GameParameters.OppGoalCenter.X , Goal.Location.X);
                Vector2D oppGoal2OurVec = Goal.Location - GameParameters.OppGoalCenter;
                oppGoal2OurVec.NormalizeTo(d1 + 0.26);
                Goal.Location = oppGoal2OurVec + GameParameters.OppGoalCenter;
            }
            
            if (GameParameters.IsInDangerousZone(Init.Location, true, 0.25, out distanceInDangerZone, out d2) && AvoidOppZone != 0)
            {
                Init.Location.X = Math.Max(GameParameters.OppGoalCenter.X , Init.Location.X);
                Vector2D oppInit2OurVec = Init.Location - GameParameters.OppGoalCenter;
                oppInit2OurVec.NormalizeTo(d2 + 0.26);
                Init.Location = oppInit2OurVec + GameParameters.OppGoalCenter;
            }
            mustRemoveObstacles = new List<int>();
       
            var obsdic = obs.ObstaclesList.ToDictionary(k => k.Key,v=>v.Value);
            bool inSomeObs = true;
            bool goalinobs = false;
       //     bool initinobs = false;
            Vector2D vec = new Vector2D();
            Position2D tmpPos = new Position2D();
            int ii = 0;
            int jj = 0;
            int tmpid = -1000;
            while (inSomeObs && ii < 10)
            {
                inSomeObs = false;
                ii++;
                obsdic = obs.ObstaclesList.ToDictionary(k => k.Key, v => v.Value);
                foreach (var item in obsdic)
                {
                    if (item.Value.Type != ObstacleType.ZoneCircle && item.Value.Type != ObstacleType.ZoneRectangle)
                    {
                        if (ii == 1 && item.Value.Meet(Init, MotionPlannerParameters.RobotRadi))
                            mustRemoveObstacles.Add(item.Key);
                        
                        if (item.Value.Meet(Goal, MotionPlannerParameters.RobotRadi))
                        {
                            //if(tmpid == item.Key)
                            jj++;
                            inSomeObs = true;
                            if (pathType == PathType.UnSafe)
                            {
                                obs.ObstaclesList.Remove(item.Key);
                            }
                            else
                            {
                                if (goalinzone)
                                {
                                    //Goal2OurVec = Goal.Location - GameParameters.OurGoalCenter;
                                    Goal2OurVec.NormalizeTo(Goal2OurVec.Size + MotionPlannerParameters.RobotRadi + 0.02);
                                    Goal.Location = Goal2OurVec + GameParameters.OurGoalCenter;
                                    tmpid = item.Key;
                                }
                                else
                                {
                                    if (goalinobs)
                                    {
                                        if (vec.Size < 1E-5)
                                            vec = Init.Location - Goal.Location;
                                        vec.NormalizeTo(vec.Size + Math.Max(item.Value.R.X, item.Value.R.Y)  + 0.02 );
                                        Goal.Location = tmpPos + vec;
                                    }
                                    else
                                    {
                                        goalinobs = true;
                                        vec = Goal.Location - item.Value.State.Location;
                                        tmpPos = item.Value.State.Location;
                                        if (vec.Size < 1E-5)
                                            vec = Init.Location - Goal.Location;
                                        vec.NormalizeTo(Math.Max(item.Value.R.X, item.Value.R.Y) + MotionPlannerParameters.RobotRadi + 0.03);
                                        Goal.Location = item.Value.State.Location + vec;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //obsdic = obs.ObstaclesList.ToDictionary(k => k.Key, v => v.Value);
            //inSomeObs = true;
            //ii = 0;
            //jj = 0;
            //tmpPos = new Position2D();
            //while (inSomeObs && ii < 10)
            //{
            //    ii++;
            //    inSomeObs = false;
            //    foreach (var item in obsdic)
            //    {
            //        if (item.Value.Type != ObstacleType.ZoneCircle && item.Value.Type != ObstacleType.ZoneRectangle)
            //        {
            //            if (item.Value.Meet(Init, robotRadi))
            //            {
            //                jj++;
            //                inSomeObs = true;
            //                if (pathType == PathType.UnSafe)
            //                {
            //                    if (obs.ObstaclesList.ContainsKey(item.Key))
            //                        obs.ObstaclesList.Remove(item.Key);
            //                }
            //                else
            //                {
            //                    if (initinzone)
            //                    {
            //                        //Init2OurVec = Init.Location - GameParameters.OurGoalCenter;
            //                        Init2OurVec.NormalizeTo(Init2OurVec.Size + robotRadi + 0.02);
            //                        Init.Location = Init2OurVec + GameParameters.OurGoalCenter;
            //                    }
            //                    else
            //                    {
            //                        if (initinobs)
            //                        {

            //                            if (vec.Size < 1E-5)
            //                                vec = Goal.Location - Init.Location;
            //                            vec.NormalizeTo(vec.Size + Math.Max(item.Value.R.X, item.Value.R.Y) + robotRadi + 0.02 );
            //                            Init.Location = tmpPos + vec;
            //                        }
            //                        else
            //                        {
            //                            initinobs = true;
            //                            vec = Init.Location - item.Value.State.Location;
            //                            tmpPos = item.Value.State.Location;
            //                            if (vec.Size < 1E-5)
            //                                vec = Goal.Location - Init.Location;
            //                            vec.NormalizeTo(Math.Max(item.Value.R.X, item.Value.R.Y) + robotRadi + 0.03);
            //                            Init.Location = item.Value.State.Location + vec;
            //                        }

            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public void Dispose()
        {
            FindPathThread.Abort();
        }
    }
    public enum PathType
    {
        Safe,
        UnSafe
    }
}
