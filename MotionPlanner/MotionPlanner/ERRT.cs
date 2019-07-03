using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
//using KDTreeDLL;
using KdTreeFast;
using System.Threading;
using KdTreeFast.Math;
using System.Drawing;

namespace MRL.SSL.Planning.MotionPlanner
{
    public class ERRT:IDisposable
    {
        public bool Finished = false;
        Obstacles obstacles = new Obstacles();
        private const double goalProbbality = 0.2;
        private const double wayPointProbbality = 0.3;
        private const double distanceTresh = 0.1;
        private const int numWayPoints = 15;
        private const double extendSize = 0.15;
        public const int maxNodes = 50;
        public const int maxNodes2Try = 500;
        private const int maxTreeCount = 1000;
        public List<SingleObjectState> Path = new List<SingleObjectState>();
        public List<SingleObjectState> SmoothPath = new List<SingleObjectState>();
        public List<SingleObjectState> LastSmoothPath = new List<SingleObjectState>();
        public List<SingleObjectState> LastNotModifiedSmoothPath = new List<SingleObjectState>();

        private SingleObjectState[] WayPoints = new SingleObjectState[numWayPoints];
        public AutoResetEvent eventR = new AutoResetEvent(false);
        public AutoResetEvent eventFinish = new AutoResetEvent(false);
        ThreadLocal<XorShift> rand = null;
        bool useERrrt = true;
        public int AvoidBall = 1, AvoidZone = 1, AvoidOppZone = 0, AvoidRobot = 1;
        bool first = true;
        public bool UseERrrt
        {
            get { return useERrrt; }
            set { useERrrt = value; }
        }
        public DistributedThread FindPathThread;
        WorldModel Model;
        int RobotID = 0;
        SingleObjectState Init = new SingleObjectState();
        SingleObjectState Goal = new SingleObjectState();
        List<SingleObjectState> lastPath = null;
        bool StopBall = false;
        PathType pathType = PathType.Safe;
        List<Obstacle> virtualObs = null;
        public bool Start = false;
        public float Time = 0;
        //   KDTree tree = new KDTree(2);//, fromGoalTree = new KDTree(2);
        KdTree<float, SingleObjectState> tree;
        public bool Failed { get; internal set; }

        // public float[] FPath = new float[2 * maxNodes];
        public int PathCount = 0;
        public ERRT(bool useExtendedRRT, int pID)
        {
            useERrrt = useExtendedRRT;
            Finished = false;
            first = true;
            
            FindPathThread = new DistributedThread(new ThreadStart(FindPath));
            FindPathThread.ProcessorAffinity = 0xFF;// (1 << (pID / 2)) | (1 << (pID % 2));  
            FindPathThread.Start();

        }
        public Obstacles Obstacles
        {
            get { return obstacles; }
            set { obstacles = value; }
        }

        public Dictionary<int, double> GoalRemovalObs;
        public Dictionary<int, double> IniRemovalObs;
        public Dictionary<int, double> SumRemovalObs;

        Dictionary<int, double> iniRemovalObs = new Dictionary<int, double>();
        Dictionary<int, double> goalRemovalObs = new Dictionary<int, double>();
        Dictionary<int, double> sumRemovalObs = new Dictionary<int, double>();

        public SingleObjectState RandomState()
        {
            //double k = 1;
            //Position2D p = new Position2D((MotionPlannerParameters.FieldLength_H - MotionPlannerParameters.FieldLength * rand.Value.randFloat()) * k + nearest.X,
            //    (MotionPlannerParameters.FieldWidth_H - MotionPlannerParameters.FieldWidth * rand.Value.randFloat()) * k + nearest.Y);
            
            //p = GameParameters.InFieldSize(p, 0);
            return new SingleObjectState(new Position2D(MotionPlannerParameters.FieldLength_H - MotionPlannerParameters.FieldLength * rand.Value.randFloat() ,
                MotionPlannerParameters.FieldWidth_H - MotionPlannerParameters.FieldWidth * rand.Value.randFloat()) , Vector2D.Zero, null);
        }
        public SingleObjectState ChoosTarget( SingleObjectState goal, SingleObjectState[] wayPoints)
        {
            double r = rand.Value.randFloat();
            if (r < goalProbbality)
                return goal;
            else if (r < (wayPointProbbality + goalProbbality) && useERrrt)
            {
                int l = rand.Value.randInt() % numWayPoints;
                if (wayPoints[l] != null)
                    return wayPoints[l];

            }
            return RandomState();
        }
        public SingleObjectState Extend(SingleObjectState Nearest, SingleObjectState target, SingleObjectState init, SingleObjectState goal, Obstacles obs)
        {
            Vector2D t = target.Location - Nearest.Location;
            double l = t.Size;
            if (l > extendSize)
                t.Scale(extendSize / l);
            Position2D tt = Nearest.Location + t;
            SingleObjectState res = new SingleObjectState(ObjectType.OurRobot, tt, Vector2D.Zero, Vector2D.Zero, null, null);
           
            Line li = new Line(Nearest.Location, res.Location);
            bool bi = init.Location.DistanceFrom(Nearest.Location) < 0.01 ;
            bool bg = goal.Location.DistanceFrom(res.Location) <= 0.1;

            if (obs.Meet(Nearest, res, MotionPlannerParameters.RobotRadi, (bi && bg) ? sumRemovalObs : (bi ? iniRemovalObs : (bg ? goalRemovalObs : null)), true))
                return null;
            else
                return res;
        }
        HiPerfTimer timer = new HiPerfTimer();
        public void Run(WorldModel model, int robotID, SingleObjectState InitileState, SingleObjectState GoalState, int avoidBall, int avoidZone, int avoidOppZone, int avoidRobot, List<SingleObjectState> LastPath, PathType Type, bool stopBall, List<Obstacle> virtualObs)
        {

            Model = model;
            RobotID = robotID;
            Init = InitileState;
            Goal = GoalState;
            pathType = Type;
            AvoidBall = avoidBall;
            AvoidRobot = avoidRobot;
            AvoidZone = avoidZone;
            AvoidOppZone = avoidOppZone;
            lastPath = LastPath;
            StopBall = stopBall;
            this.virtualObs = virtualObs;
            eventR.Set();
        }
        public void Run(bool Set, bool stopBall)
        {
            StopBall = stopBall;
            LastSmoothPath = new List<SingleObjectState>();
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
                    rand = new ThreadLocal<XorShift>(() => new XorShift((uint)Guid.NewGuid().GetHashCode()));
                    eventFinish.Set();
                    eventR.WaitOne();
                }

               // timer.Start();
                if (/*useERrrt*/false && lastPath != null && lastPath.Count > 0)
                {
                    for (int i = 0; i < numWayPoints; i++)
                    {
                        int j = rand.Value.randInt() % lastPath.Count;
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
                int nodeCountOffset = 0;

                if (Failed && tree != null && tree.Count >= 2000)
                {
                    Failed = false;
                }

                if (!Failed)
                {
                    //tree = new KDTree(2); //fromGoalTree = new KDTree(2);
                    tree = new KdTree<float, SingleObjectState>(2, new FloatMath());
                }
                else
                    nodeCountOffset = tree.Count;
                Obstacles obs = new Obstacles(Model);

                obs.AddObstacle(AvoidRobot, AvoidBall, AvoidZone, AvoidOppZone, new List<int>() { RobotID }, null, MotionPlannerParameters.kSpeedBall, MotionPlannerParameters.kSpeedRobot, StopBall);
                if (virtualObs != null)
                {
                    foreach (var item in virtualObs)
                    {
                        obs.AddVirtualObstacle(item);

                    }
                }
                List<SingleObjectState> res = new List<SingleObjectState>();
                Init.ParentState = null;

                CheckInitialStates(init, goal, obs, pathType, out iniRemovalObs, out goalRemovalObs, out sumRemovalObs);
                //foreach (var item in mustRemove)
                //{
                //    if (obs.ObstaclesList.ContainsKey(item))
                //        obs.ObstaclesList.Remove(item);
                //}
                SingleObjectState NearestState ;
                //if (!Failed)
                //    NearestState = new SingleObjectState(init);
                //else
                    NearestState = new SingleObjectState(init);
                //SingleObjectState fromGoalNearestState = new SingleObjectState(goal);
                
                if (!obs.Meet(init, goal, MotionPlannerParameters.RobotRadi , sumRemovalObs, true))
                {
                    res.Add(goal);
                    //FPath[2 * PathCount] = (float)goal.Location.X;
                    //FPath[2 * PathCount + 1] = (float)goal.Location.Y;
                    PathCount++;
                    if (init.Location != Init.Location)
                    {
                        res.Add(init);
                        //FPath[2 * PathCount] = (float)init.Location.X;
                        //FPath[2 * PathCount + 1] = (float)init.Location.Y;
                        PathCount++;
                    }
                    //FPath[2 * PathCount] = (float)Init.Location.X;
                    //FPath[2 * PathCount + 1] = (float)Init.Location.Y;
                    PathCount++;
                    res.Add(Init);
                    Path = res;
                    Failed = false;

                }
                else
                {
                    int nodes2try = 0;
                    //do
                    //{
                        float[] d = { (float)init.Location.X, (float)init.Location.Y };
                        init.ParentState = null;
                        //if (nodes2try >= maxNodes)
                        //{
                        //    // tree = new KDTree(2);
                        //    tree = new KdTree<float, SingleObjectState>(2, new FloatMath(), AddDuplicateBehavior.Skip);

                        //    NearestState = new SingleObjectState(init);
                        //    obs.Clear();
                        //    obs.AddObstacle(0, 0, 1, 0, null, null, StopBall);
                        //}
                        //if (!Failed)
                        {
                            //   tree.insert(d, init);
                            tree.Add(d, init);
                        }
                        nodes2try = 0;
                        while (NearestState.Location.DistanceFrom(goal.Location) > 0.1 && (tree.Count - nodeCountOffset) < maxNodes && nodes2try < maxNodes2Try)
                        {
                            nodes2try++;
                            target = ChoosTarget(goal, WayPoints);
                            
                            if (tree.Count > 0 && (!Failed || (Failed && nodes2try > 1))  )
                            {
                                float[] d2 = { (float)target.Location.X, (float)target.Location.Y };
                                //NearestState = tree.nearest(d2);
                                NearestState = tree.GetNearestNeighbours(d2, 1).First().Value;
                            }
                            Extended = Extend(NearestState, target,init, goal, obs);
                            if (Extended != null)
                            {
                                if (Extended.Location == goal.Location)
                                    break;

                                float[] ed = { (float)Extended.Location.X, (float)Extended.Location.Y };

                                //SingleObjectState sos = tree.FindValueAt(ed); 
                                //if (sos == null)
                                {
                                    Extended.ParentState = NearestState;
                                    //tree.insert(ed, Extended);
                                    tree.Add(ed, Extended);
                                   // tree.Add(ed, Extended);
                                }
                                //else
                                //    tree.Add(ed, Extended);
                            }
                        }

                   // } while ((NearestState.Location != goal.Location && nodes2try >= maxNodes2Try && obs.MeetDangerZone(NearestState, goal, MotionPlannerParameters.RobotRadi)));

                    if (NearestState.Location != goal.Location)
                    {
                        //SingleObjectState sos = new SingleObjectState(NearestState.Location, Vector2D.Zero, 0)
                        //{
                        //    ParentState = NearestState
                        //};
                        //NearestState = sos;
                        if (obs.Meet(NearestState, goal, MotionPlannerParameters.RobotRadi, goalRemovalObs, true))
                            Failed = true;
                        else
                            Failed = false;
                        goal.ParentState = NearestState;
                        NearestState = new SingleObjectState(goal)
                        {
                            ParentState = goal.ParentState
                        };
                    }
                    else
                        Failed = false;

                    while (NearestState != null)
                    {
                        //FPath[2 * PathCount] = (float)NearestState.Location.X;
                        //FPath[2 * PathCount + 1] = (float)NearestState.Location.Y;
                        PathCount++;
                        res.Add(NearestState);
                        NearestState = NearestState.ParentState;
                    }


                    if (Init.Location != init.Location)
                    {
                        //FPath[2 * PathCount] = (float)Init.Location.X;
                        //FPath[2 * PathCount + 1] = (float)Init.Location.Y;
                        PathCount++;
                        res.Add(Init);
                    }
                    Path = res;

                }
                SmoothPath = RandomInterpolateSmoothing(Path, obs,init,goal, false);
                if (LastSmoothPath.Count > 1)
                {
                    LastSmoothPath[LastSmoothPath.Count - 1] = new SingleObjectState(Init);
                    LastSmoothPath = RandomInterpolateSmoothing(LastSmoothPath, obs, init, goal, true);
                }
                else
                    LastSmoothPath = new List<SingleObjectState>();

                IniRemovalObs = iniRemovalObs.ToDictionary(k => k.Key, v => v.Value);
                GoalRemovalObs = goalRemovalObs.ToDictionary(k => k.Key, v => v.Value);
                SumRemovalObs = sumRemovalObs.ToDictionary(k => k.Key, v => v.Value);

                iniRemovalObs = new Dictionary<int, double>();
                goalRemovalObs = new Dictionary<int, double>();
                sumRemovalObs = new Dictionary<int, double>();

                obstacles = obs;
                Finished = true;
                //timer.Stop();
                //Console.WriteLine("RobotID " + RobotID + " time: " + timer.Duration * 1000 + " ms");
                eventFinish.Set();
                
                eventR.WaitOne();
            }
        }
        private void CheckInitialStates(SingleObjectState Init, SingleObjectState Goal, Obstacles obs, PathType pathType, out Dictionary<int, double> initRemoveObstacles, out Dictionary<int, double> goalRemoveObstacles, out Dictionary<int, double> sumRemoveObstacles)
        {
            bool goalinzone = false;
            double d1 = 0, d2 = 0, distanceInDangerZone;
            Vector2D Goal2OurVec = new Vector2D() ;
            if (GameParameters.IsInDangerousZone(Goal.Location, false, 0.1, out distanceInDangerZone, out d1) && AvoidZone != 0)
            {
                Goal.Location.X = Math.Min(GameParameters.OurGoalCenter.X , Goal.Location.X);
                //Goal2OurVec = Goal.Location - GameParameters.OurGoalCenter;
                ////double safeR = GameParameters.SafeRadi(Goal, MotionPlannerParameters.DangerZoneW - GameParameters.DefenceareaRadii + 0.01);
                //Goal2OurVec.NormalizeTo(d1 + MotionPlannerParameters.GoalExtendFromDangerZoneMargin); 
                //Goal.Location = Goal2OurVec + GameParameters.OurGoalCenter;
                Goal.Location = GameParameters.IntersectWithDangerZone(Goal.Location, true, MotionPlannerParameters.GoalExtendFromDangerZoneMargin);

                goalinzone = true;
            }
            Vector2D Init2OurVec = new Vector2D() ;
            if (GameParameters.IsInDangerousZone(Init.Location, false, 0.1, out distanceInDangerZone, out d2) && AvoidZone != 0)
            {
                Init.Location.X = Math.Min(GameParameters.OurGoalCenter.X, Init.Location.X);
                //Init2OurVec = Init.Location - GameParameters.OurGoalCenter;
                //Init2OurVec.NormalizeTo(d2 + 0.16);
                //Init.Location = Init2OurVec + GameParameters.OurGoalCenter;
                Init.Location = GameParameters.IntersectWithDangerZone(Init.Location, true, 0.16);

            }

            if (GameParameters.IsInDangerousZone(Goal.Location, true, 0.35, out distanceInDangerZone, out d1) && AvoidOppZone != 0)
            {
                DrawingObjects.AddObject(new Circle(Goal.Location, 0.1, new System.Drawing.Pen(Color.Aqua, 0.1f)), "goalbeforex");

               
                Goal.Location.X = Math.Max(GameParameters.OppGoalCenter.X , Goal.Location.X);
                //Vector2D oppGoal2OurVec = Goal.Location - GameParameters.OppGoalCenter;
                //oppGoal2OurVec.NormalizeTo(d1 + 0.26);
                //Goal.Location = oppGoal2OurVec + GameParameters.OppGoalCenter;
                Goal.Location = GameParameters.IntersectWithDangerZone(Goal.Location, false, 0.36);
                DrawingObjects.AddObject(new Circle(Goal.Location, 0.1, new System.Drawing.Pen(Color.Beige, 0.1f)), "goalafterex");

            }

            if (GameParameters.IsInDangerousZone(Init.Location, true, 0.35, out distanceInDangerZone, out d2) && AvoidOppZone != 0)
            {
                Init.Location.X = Math.Max(GameParameters.OppGoalCenter.X , Init.Location.X);
                Init.Location = GameParameters.IntersectWithDangerZone(Init.Location, false, 0.36);
                //Vector2D oppInit2OurVec = Init.Location - GameParameters.OppGoalCenter;
                //oppInit2OurVec.NormalizeTo(d2 + 0.26);
                //Init.Location = oppInit2OurVec + GameParameters.OppGoalCenter;
            }
            initRemoveObstacles = new Dictionary<int, double>();
            goalRemoveObstacles = new Dictionary<int, double>();
            sumRemoveObstacles = new Dictionary<int, double>();
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
                    if (item.Value.Type != ObstacleType.ZoneCircle && item.Value.Type != ObstacleType.ZoneRectangle && item.Value.Type != ObstacleType.Rectangle)
                    {
                        if (ii == 1 && item.Value.Meet(Init, MotionPlannerParameters.RobotRadi, true))
                        {
                            if(!item.Value.Meet(Init, MotionPlannerParameters.RobotRadi, false))
                            {
                                initRemoveObstacles[item.Key] = -item.Value.Margin;
                                sumRemoveObstacles[item.Key] = initRemoveObstacles[item.Key];

                            }
                            else if (!item.Value.Meet(Init, 0, false))
                            {

                                initRemoveObstacles[item.Key] = - MotionPlannerParameters.RobotRadi - item.Value.Margin;
                                sumRemoveObstacles[item.Key] = initRemoveObstacles[item.Key];
                            }
                            else
                                obs.ObstaclesList.Remove(item.Key);

                            //mustRemoveObstacles.Add(item.Key);
                        }
                        if (item.Value.Meet(Goal, MotionPlannerParameters.RobotRadi, true))
                        {
                            //if(tmpid == item.Key)
                            jj++;
                            inSomeObs = true;
                            if (pathType == PathType.UnSafe)
                            {
                                //obs.ObstaclesList.Remove(item.Key);
                                if (!item.Value.Meet(Goal, MotionPlannerParameters.RobotRadi, false))
                                {
                                    
                                    goalRemoveObstacles[item.Key] = - item.Value.Margin;
                                    if(sumRemoveObstacles.ContainsKey(item.Key))
                                        sumRemoveObstacles[item.Key] = Math.Min(sumRemoveObstacles[item.Key], goalRemoveObstacles[item.Key]);
                                    else
                                        sumRemoveObstacles[item.Key] = goalRemoveObstacles[item.Key];
                                }
                                else if (!item.Value.Meet(Goal, 0, false))
                                {
                                    goalRemoveObstacles[item.Key] = -MotionPlannerParameters.RobotRadi - item.Value.Margin;

                                    if (sumRemoveObstacles.ContainsKey(item.Key))
                                        sumRemoveObstacles[item.Key] = Math.Min(sumRemoveObstacles[item.Key], goalRemoveObstacles[item.Key]);
                                    else
                                        sumRemoveObstacles[item.Key] = goalRemoveObstacles[item.Key];
                                }
                                else
                                {
                                    obs.ObstaclesList.Remove(item.Key);
                                }
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
                                        vec.NormalizeTo(vec.Size + Math.Max(item.Value.R.X, item.Value.R.Y) + 0.02);
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
            FindPathThread.Stop();
        }
        //---------->\
        List<SingleObjectState> LastPath = null;

        public List<SingleObjectState> RandomInterpolateSmoothing(List<SingleObjectState> path, Obstacles obs, SingleObjectState init, SingleObjectState goal, bool justInitChanged)
        {
            List<SingleObjectState> ppat = new List<SingleObjectState>();
            //   return path;
            for (int m = 0; m < path.Count; m++)
            {
                ppat.Add(new SingleObjectState(path[m]));
            }
            if (ppat.Count <= 2)
                return ppat;
            bool ji = justInitChanged;

            //    return ppat;
            //   if (!obs.Meet(ppat[0], ppat[1], MotionPlannerParameters.RobotRadi))
            {
                int N = (ji) ? 6 : path.Count / 2;// 4;
                for (int i = 0; i < N; i++)
                {
                    List<int> nodes = new List<int>();
                    for (int k = 0; k < ppat.Count; k++)
                    {
                        nodes.Add(k);
                    }
                    if (ppat.Count == 2)
                        break;

                    int s = (ji) ? (nodes.Count - 1) : rand.Value.randInt(0, nodes.Count);
                    ji = false;
                    int sKey = nodes[s];
                    SingleObjectState start = ppat[sKey];
                    nodes.RemoveAt(s);
                    if (s > 0 && s < ppat.Count - 1)
                        nodes.RemoveAt(s);
                    if (s - 1 >= 0 && nodes.Count > s - 1)
                        nodes.RemoveAt(s - 1);
                    if (nodes.Count == 0)
                        continue;
                    int e = rand.Value.randInt(0, nodes.Count);
                    int eKey = nodes[e];
                    SingleObjectState end = ppat[eKey];

                    bool bi = (start.Location.DistanceFrom(init.Location) < 0.01 || end.Location.DistanceFrom(init.Location) < 0.01) ;

                    bool bg = (start.Location.DistanceFrom(goal.Location) < 0.01 || end.Location.DistanceFrom(goal.Location) < 0.01);

                    if (!obs.Meet(start, end, MotionPlannerParameters.RobotRadi, (bi && bg) ? sumRemovalObs : (bi ? iniRemovalObs : (bg ? goalRemovalObs : null)), true))
                    {
                        int min = (sKey < eKey) ? sKey : eKey;
                        int max = (sKey > eKey) ? sKey : eKey;
                        if (max - min > 1)
                            ppat.RemoveRange(min + 1, max - min - 1);
                    }
                }

            }
            if (ppat.Count > 2)
            {
                double step = 0.2;
                for (int i = 1; i < ppat.Count - 1; i++)
                {
                    SingleObjectState next = ppat[i + 1];
                    SingleObjectState prev = ppat[i - 1];
                    SingleObjectState current = ppat[i];
                    Vector2D nextCurrent = current.Location - next.Location;
                    Vector2D prevCurrent = current.Location - prev.Location;
                    int count = (int)Math.Max(nextCurrent.Size / step, prevCurrent.Size / step);
                    double nextStep = nextCurrent.Size / count, prevStep = prevCurrent.Size / count;
                    for (int j = 1; j < count; j++)
                    {
                        SingleObjectState n = new SingleObjectState(next.Location + nextCurrent.GetNormalizeToCopy(j * nextStep), Vector2D.Zero, 0);
                        SingleObjectState p = new SingleObjectState(prev.Location + prevCurrent.GetNormalizeToCopy(j * prevStep), Vector2D.Zero, 0);
                        bool bi = (p.Location.DistanceFrom(init.Location) < 0.01 || n.Location.DistanceFrom(init.Location) < 0.01);
                        bool bg = (p.Location.DistanceFrom(goal.Location) < 0.01 || n.Location.DistanceFrom(goal.Location) < 0.01);

                        if (!obs.Meet(p, n, MotionPlannerParameters.RobotRadi, (bi && bg) ? sumRemovalObs : (bi ? iniRemovalObs : (bg ? goalRemovalObs : null)), true))
                        {
                            ppat.RemoveAt(i);
                            bi = (p.Location.DistanceFrom(init.Location) < 0.01 || next.Location.DistanceFrom(init.Location) < 0.01);
                            bg = (p.Location.DistanceFrom(goal.Location) < 0.01 || next.Location.DistanceFrom(goal.Location) < 0.01);

                            if (!obs.Meet(p, next, MotionPlannerParameters.RobotRadi, (bi && bg) ? sumRemovalObs : (bi ? iniRemovalObs : (bg ? goalRemovalObs : null)), true))
                            {
                                ppat.Insert(i, p);
                            }
                            else
                            {
                                ppat.Insert(i, p);
                                ppat.Insert(i + 1, n);
                            }
                            break;
                        }
                    }
                }
            }
            //if (!justInitChanged)
            //{
            //    Position2D r = CalR(ppat[0].Location.X, ppat[0].Location.Y, ppat[1].Location.X, ppat[1].Location.Y);
            //    Position2D q;
            //    List<SmoothData> extera = new List<SmoothData>();
            //    for (int i = 1; i < ppat.Count - 1; i++)
            //    {
            //        q = CalQ(ppat[i].Location.X, ppat[i].Location.Y, ppat[i + 1].Location.X, ppat[i + 1].Location.Y);
            //        if (!obs.Meet(new SingleObjectState(r, Vector2D.Zero, 0), new SingleObjectState(q, Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi))
            //        {

            //            extera.Add(new SmoothData()
            //            {
            //                rIndex = i,
            //                qIndex = i + 1,
            //                Q = new SingleObjectState(q, Vector2D.Zero, 0),
            //                R = new SingleObjectState(r, Vector2D.Zero, 0)
            //            });


            //        }

            //        r = CalR(ppat[i].Location.X, ppat[i].Location.Y, ppat[i + 1].Location.X, ppat[i + 1].Location.Y);
            //    }
            //    foreach (var item in extera)
            //    {
            //        ppat.RemoveAt(item.rIndex);
            //        ppat.Insert(item.rIndex, item.R);
            //        DrawingObjects.AddObject(new Circle(item.R.Location, 0.1, new System.Drawing.Pen(Color.Aqua, 0.01f)), "rrrr" + item.rIndex);

            //        ppat.Insert(item.qIndex, item.Q);
            //        DrawingObjects.AddObject(new Circle(item.Q.Location, 0.1, new System.Drawing.Pen(Color.Beige, 0.01f)), "qqqq" + item.qIndex);

            //    }
            //}
            //  else
            //   {
            //  }
            return ppat;
        }
        Position2D CalQ(double x1, double y1, double x2, double y2)
        {
            x1 = 3f / 4f * x1;
            y1 = 3f / 4f * y1;
            x2 = 1f / 4f * x2;
            y2 = 1f / 4f * y2;
            return new Position2D((x1 + x2), (y1 + y2));

        }
        Position2D CalR(double x1, double y1, double x2, double y2)
        {
            x1 = 1f / 4f * x1;
            y1 = 1f / 4f * y1;
            x2 = 3f / 4f * x2;
            y2 = 3f / 4f * y2;
            return new Position2D((x1 + x2), (y1 + y2));

        }
        class SmoothData{
            public SingleObjectState Q;
                public SingleObjectState R;
            public int rIndex, qIndex;
        }
    }

}
    public enum PathType
    {
        Safe,
        UnSafe
    }

