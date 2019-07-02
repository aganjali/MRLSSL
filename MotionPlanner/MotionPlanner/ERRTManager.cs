using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Threading;
using System.Runtime.InteropServices;
using MRL.SSL.Planning.GPUDirect;

namespace MRL.SSL.Planning.MotionPlanner
{
    public class ERRTManager:IDisposable
    {
        List<ERRT> errts = new List<ERRT>();
       // List<float> avdBall, avdR, avdZ,avdOz, avoid, obsX, obsY, obs, TotalFPath;
        List<SingleObjectState> states = new List<SingleObjectState>();
        Dictionary<int, List<SingleObjectState>> FinalPathes;
        Dictionary<int, List<SingleObjectState>> lastFinalPathes;
        Dictionary<int, double> CurrentPathWeightes;
        Dictionary<int, double> LastPathWeightes;

        Dictionary<int, SingleObjectState> LastGoals = new Dictionary<int, SingleObjectState>(); 
       
        float[] finalPath;
        float[] tmpfpath;
        
        public int SmoothingCount = 30;
        public float Kspring = 0.35f;
        public float Kspring2 = 0;
        public int n = 2;
        int maxPathCount, maxRRTCount;

        public int MaxPathCount
        {
            get { return maxPathCount; }
            set { maxPathCount = value; }
        }
        HiPerfTimer h = new HiPerfTimer();
        public int MaxRRTCount
        {
            get { return maxRRTCount; }
            set { maxRRTCount = value; }
        }
        public ERRTManager(int _maxRRTCount, int _maxPathCount, bool useERRT)
        {
            maxPathCount = _maxPathCount;
            maxRRTCount =  2*_maxRRTCount;
            errts = new List<ERRT>();
           
            LastGoals = new Dictionary<int, SingleObjectState>();
            FinalPathes = new Dictionary<int, List<SingleObjectState>>();
            lastFinalPathes = new Dictionary<int, List<SingleObjectState>>();
            CurrentPathWeightes = new Dictionary<int, double>();
            LastPathWeightes = new Dictionary<int, double>();
            tmpfpath = new float[2* maxPathCount];
            for (int i = 0; i < _maxRRTCount; i++)
            {
                errts.Add(new ERRT(useERRT, i));
            }

            finalPath = new float[2 * maxPathCount * maxRRTCount];
            //Start(3, 3, 0.3f, maxPathCount, maxRRTCount);
            ////ElasticInit(maxPathCount, maxRRTCount);
            //float[] p = new float[2], p2 = new float[3], p3 = new float[2];
            //int[] pc = { 1 };
            //float[] obsf = new float[2];
            //ForceTree(p, pc, 1, p2, p3, 1, obsf, 1, 0.1f, 0.1f, 1);
        }
        public Dictionary<int, List<SingleObjectState>> Run(WorldModel Model, Dictionary<int, SingleObjectState> InitialStates, Dictionary<int, SingleObjectState> GoalStates, List<int> RobotIds, Dictionary<int, PathType> Types, Dictionary<int, int> avoidballs, Dictionary<int, int> avoidrobots, Dictionary<int, int> avoidzones, Dictionary<int, int> avoidOppzones, bool useErrt, bool StopBall, Dictionary<int, List<Obstacle>> virtualObs)
        {
         
            foreach (var item in RobotIds)
            {
                if (!GoalStates.ContainsKey(item))
                    GoalStates[item] = Model.OurRobots[item];
                if (!avoidballs.ContainsKey(item))
                    avoidballs[item] = 1;
                if (!avoidzones.ContainsKey(item))
                    avoidzones[item] = 1;
                if (!avoidrobots.ContainsKey(item))
                    avoidrobots[item] = 1;
                if (!avoidOppzones.ContainsKey(item))
                    avoidOppzones[item] = 0;
                if (!Types.ContainsKey(item))
                    Types[item] = PathType.UnSafe;

                if (!Types.ContainsKey(item))
                    Types[item] = PathType.UnSafe;
                if (!virtualObs.ContainsKey(item))
                    virtualObs[item] = null;

            }
            int Count =  Math.Min(errts.Count, RobotIds.Count);
            WaitHandle[] waits = new WaitHandle[errts.Count];

           // h.Start();
            for (int i = 0; i < errts.Count; i++)
            {
                if (i < Count)
                {
                    int id = RobotIds[i];
                    if (LastGoals.ContainsKey(id) && LastGoals[id].Location.DistanceFrom(GoalStates[id].Location) > 0.01)
                    {
                        errts[i].LastSmoothPath = new List<SingleObjectState>();
                    }
                    errts[i].Run(new WorldModel(Model), id, InitialStates[id], GoalStates[id], avoidballs[id], avoidzones[id], avoidOppzones[id], avoidrobots[id], (FinalPathes.ContainsKey(id) ? FinalPathes[id] : null), Types[id], StopBall, virtualObs[id]);
                 

                }
                else
                    errts[i].Run(true, StopBall);
            }
         
            for (int i = 0; i < errts.Count; i++)
            {
                waits[i] = errts[i].eventFinish;
            }

            WaitHandle.WaitAll(waits);
            //h.Stop();
            //Console.WriteLine((h.Duration * 1000).ToString());



            FinalPathes = new Dictionary<int, List<SingleObjectState>>();
            CurrentPathWeightes.Clear();
            LastPathWeightes.Clear();
            for (int i = 0; i <  Count; i++)
            {
                int id = RobotIds[i];
                if (errts[i].Obstacles.Meet(errts[i].SmoothPath[0], errts[i].SmoothPath[1], MotionPlannerParameters.RobotRadi, errts[i].GoalRemovalObs, true))
                {
                    errts[i].SmoothPath.RemoveAt(0);
                }
                else
                {

                }
                CurrentPathWeightes[id] = PathWeightCalculator(Model, errts[i].SmoothPath, errts[i], id, errts[i].Obstacles, GoalStates[id]);
                
            }

            for (int i = Count; i < 2 * Count; i++)
            {
                int id = RobotIds[i - Count];
                if (errts[i - Count].LastSmoothPath.Count == 0)
                {
                    DrawingObjects.AddObject("pathcircleeee" + id.ToString(), new Circle(Position2D.Zero, 1, new System.Drawing.Pen(System.Drawing.Color.Red, 0.1f)));

                    //FinalPathes[id] = new List<SingleObjectState>();
                    FinalPathes[id] = errts[i - Count].SmoothPath;//.ForEach(f => FinalPathes[id].Add(new SingleObjectState(f)));
                }
                else
                {
                    LastPathWeightes[id] = PathWeightCalculator(Model, errts[i - Count].LastSmoothPath, errts[i - Count], id, errts[i - Count].Obstacles, GoalStates[id]);

                    if (LastPathWeightes[id] < CurrentPathWeightes[id] + 1)
                    {

                        //FinalPathes[id] = new List<SingleObjectState>();
                        FinalPathes[id] = errts[i - Count].LastSmoothPath;//.ForEach(f => FinalPathes[id].Add(new SingleObjectState(f)));
                        if (errts[i - Count].LastSmoothPath.Count >= 2 && errts[i - Count].LastSmoothPath[0].Location.DistanceFrom(GoalStates[id].Location) < 0.1)
                        {
                            errts[i - Count].Failed = false;
                        }
                        
                    }
                    else
                    {
                        //FinalPathes[id] = errts[i].SmoothPath;
                        //FinalPathes[id] = new List<SingleObjectState>();
                        FinalPathes[id] = errts[i - Count].SmoothPath;//.ForEach(f => FinalPathes[id].Add(new SingleObjectState(f)));
                    }
                }
                
            }
            for (int i = 0; i < Count; i++)
            {
                int id = RobotIds[i];
                errts[i].LastSmoothPath = new List<SingleObjectState>();
                FinalPathes[id].ForEach(f => errts[i].LastSmoothPath.Add(new SingleObjectState(f)));
                //errts[i].LastSmoothPath = FinalPathes[id];
                
                List <Position2D> ppat = new List<Position2D>();
                FinalPathes[id].ForEach(f => ppat.Add(new Position2D(f.Location.X, f.Location.Y)));
                DrawingObjects.AddObject("path" + id.ToString(), new DrawRegion(ppat, false, false, System.Drawing.Color.Red, System.Drawing.Color.Red));

                //List<Position2D> ppat2 = new List<Position2D>();
                //errts[i].Path.ForEach(f => ppat2.Add(new Position2D(f.Location.X, f.Location.Y)));
                //DrawingObjects.AddObject("path2222" + id.ToString(), new DrawRegion(ppat2, false, false, System.Drawing.Color.Red, System.Drawing.Color.Blue));

            }
            LastGoals = new Dictionary<int, SingleObjectState>();
            for (int i = 0; i < Count; i++)
                LastGoals.Add(RobotIds[i], GoalStates[RobotIds[i]]);
            return FinalPathes; 
        }

        private double PathWeightCalculator(WorldModel Model, List<SingleObjectState> Path, ERRT eRRT, int RobotID, Obstacles obs, SingleObjectState Goal)
        {
            double speed = 0, angle = 0, length = 0;
            double _angleWeight = 60;
            double _countWeight = 0;
            double _speedWieght = 4;
            double _lengthWieght = 4;
            if (Path.Count < 2)
                return double.MaxValue;
            Vector2D s  = Path[Path.Count - 2].Location - Path[Path.Count - 1].Location;
            speed = Math.Abs(Vector2D.AngleBetweenInRadians(s, Model.OurRobots[RobotID].Speed));
            if (speed < Math.PI / 12)
                speed = 0;
            Vector2D Vec, Vec2;
            bool met = false, metZone =false;
            Position2D tmpp =Position2D.Zero;
            for (int i = Path.Count - 1; i > 1; i--)
            {

                if (!met && obs.Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01, (i == Path.Count - 1) ? eRRT.IniRemovalObs : null, true))
                    met = true;
                if (!metZone && 
                    (
                        (obs.ObstaclesList.ContainsKey(-2) && obs.ObstaclesList[-2].Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01, true))
                        || (obs.ObstaclesList.ContainsKey(-3) && obs.ObstaclesList[-3].Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01, true))
                        || (obs.ObstaclesList.ContainsKey(-4) && obs.ObstaclesList[-4].Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01, true))
                    )
                   )
                    metZone = true;
                Vec = Path[i].Location - Path[i - 1].Location;
                length += Vec.Size;
                Vec2 = Path[i - 1].Location - Path[i - 2].Location;
                angle += Math.Abs(Vector2D.AngleBetweenInRadians(Vec, Vec2));
            }
            Vec = Path[1].Location - Path[0].Location;
            if (!met && obs.Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi- 0.01,(Path.Count > 2) ? eRRT.GoalRemovalObs:eRRT.SumRemovalObs, true))
                met = true;
             if (!metZone && 
                    (
                        (obs.ObstaclesList.ContainsKey(-2) && obs.ObstaclesList[-2].Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi - 0.01, true))
                        || (obs.ObstaclesList.ContainsKey(-3) && obs.ObstaclesList[-3].Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi - 0.01, true))
                        || (obs.ObstaclesList.ContainsKey(-4) && obs.ObstaclesList[-4].Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi - 0.01, true))
                    )
                   )
                    metZone = true;
             //if (
             //        (
             //            (obs.ObstaclesList.ContainsKey(-2) && obs.ObstaclesList[-2].Meet(Path[0], MotionPlannerParameters.RobotRadi - 0.01, true))
             //            || (obs.ObstaclesList.ContainsKey(-3) && obs.ObstaclesList[-3].Meet(Path[0], MotionPlannerParameters.RobotRadi - 0.01, true))
             //            || (obs.ObstaclesList.ContainsKey(-4) && obs.ObstaclesList[-4].Meet(Path[0], MotionPlannerParameters.RobotRadi - 0.01, true))
             //        )
             //       )
             //{
             //    ;
             //}
            length += Vec.Size;
            angle /= Path.Count;
            if(metZone)
                return (200000 + _angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length);
            if(met)
                return (100000 + _angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length);
            if ((Goal.Location - Path[0].Location).Size > 0.01)
                return (50000 + _angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length + (Goal.Location - Path[0].Location).Size * 100);
            return (_angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length);
        }
      
        public void Dispose()
        {
            for (int i = 0; i < errts.Count; i++)
            {
                errts[i].Dispose();
            }
        }
    }
}
