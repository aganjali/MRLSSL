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
        List<float> avdBall, avdR, avdZ,avdOz, avoid, obsX, obsY, obs, TotalFPath;
        List<SingleObjectState> states = new List<SingleObjectState>();
        Dictionary<int, List<SingleObjectState>> FinalPathes;
        Dictionary<int, List<SingleObjectState>> lastFinalPathes;
        Dictionary<int, double> CurrentPathWeightes;
        Dictionary<int, double> LastPathWeightes;
        Dictionary<int, float[]> FinalPathesFloat;
        Dictionary<int, SingleObjectState> LastGoals = new Dictionary<int, SingleObjectState>(); 
        int[] EachPathCount;
        float[] finalPath;
        float[] tmpfpath;
        
        public int SmoothingCount = 50;
        public float Kspring = 0.35f;
        public float Kspring2 = 0;
        public int n = 2;
        int maxPathCount, maxRRTCount;

        public int MaxPathCount
        {
            get { return maxPathCount; }
            set { maxPathCount = value; }
        }

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
            avdBall = new List<float>();
            avdR = new List<float>();
            avdZ = new List<float>();
            avdOz = new List<float>();
            avoid = new List<float>();
            obsX = new List<float>();
            obsY = new List<float>();
            obs = new List<float>();
            TotalFPath = new List<float>();
            LastGoals = new Dictionary<int, SingleObjectState>();
            FinalPathes = new Dictionary<int, List<SingleObjectState>>();
            lastFinalPathes = new Dictionary<int, List<SingleObjectState>>();
            FinalPathesFloat = new Dictionary<int, float[]>();
            CurrentPathWeightes = new Dictionary<int, double>();
            LastPathWeightes = new Dictionary<int, double>();
            tmpfpath = new float[2* maxPathCount];
            for (int i = 0; i < _maxRRTCount; i++)
            {
                errts.Add(new ERRT(useERRT));
            }
            EachPathCount = new int[maxRRTCount];
            finalPath = new float[2 * maxPathCount * maxRRTCount];
            //Start(3, 3, 0.3f, maxPathCount, maxRRTCount);
            ////ElasticInit(maxPathCount, maxRRTCount);
            //float[] p = new float[2], p2 = new float[3], p3 = new float[2];
            //int[] pc = { 1 };
            //float[] obsf = new float[2];
            //ForceTree(p, pc, 1, p2, p3, 1, obsf, 1, 0.1f, 0.1f, 1);
        }
        public Dictionary<int, List<SingleObjectState>> Run(WorldModel Model,Dictionary<int,bool> CutOtherPath, Dictionary<int, SingleObjectState> InitialStates, Dictionary<int, SingleObjectState> GoalStates, List<int> RobotIds, Dictionary<int, PathType> Types, Dictionary<int, int> avoidballs, Dictionary<int, int> avoidrobots, Dictionary<int, int> avoidzones, Dictionary<int, int> avoidOppzones, bool useErrt, bool StopBall)
        {
            TotalFPath.Clear();
            Dictionary<int, Line> Lines = new Dictionary<int, Line>();
            Dictionary<int, List<Line>> intersectsLines = new Dictionary<int, List<Line>>();
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
                if (!CutOtherPath.ContainsKey(item))
                    CutOtherPath[item] = true;
                Lines[item] = new Line(InitialStates[item].Location, GoalStates[item].Location);
                intersectsLines[item] = new List<Line>();
            }
            int Count =  Math.Min(errts.Count, RobotIds.Count);
            WaitHandle[] waits = new WaitHandle[errts.Count];
            avdBall.Clear();
            avdR.Clear();
            avdZ.Clear();
            avdOz.Clear();
            Position2D Intersect = new Position2D();


            for (int i = 0; i < RobotIds.Count; i++)
            {
                int id1 = RobotIds[i];
                for (int j = 0; j < RobotIds.Count; j++)
                {
                    if (i >= j)
                        continue;
                    int id2 = RobotIds[j];
                    if (CutOtherPath[id1] && CutOtherPath[id2])
                        continue;
                    bool b = Lines[id1].IntersectWithLine(Lines[id2], ref Intersect)
                        && ((Intersect - InitialStates[id1].Location).InnerProduct(GoalStates[id1].Location - InitialStates[id1].Location) >= 0)
                        && ((Intersect - GoalStates[id1].Location).InnerProduct(InitialStates[id1].Location - GoalStates[id1].Location) >= 0)
                        && ((Intersect - InitialStates[id2].Location).InnerProduct(GoalStates[id2].Location - InitialStates[id2].Location) >= 0)
                        && ((Intersect - GoalStates[id2].Location).InnerProduct(InitialStates[id2].Location - GoalStates[id2].Location) >= 0);

                    if (!CutOtherPath[id1] && b && (InitialStates[id1].Location.DistanceFrom(GoalStates[id1].Location) >= InitialStates[id2].Location.DistanceFrom(GoalStates[id2].Location)))
                        intersectsLines[id1].Add(Lines[id2]);
                    else if (!CutOtherPath[id2] && b)
                    {
                        intersectsLines[id2].Add(Lines[id1]);
                    }

                }
            }
            for (int i = 0; i < errts.Count; i++)
            {
                if (i < Count)
                {
                    int id = RobotIds[i];
                    errts[i].Run(new WorldModel(Model), id, intersectsLines[id], InitialStates[id], GoalStates[id], avoidballs[id], avoidzones[id], avoidOppzones[id], avoidrobots[id], (FinalPathes.ContainsKey(id) ? FinalPathes[id] : null), Types[id], StopBall);
                    avdBall.Add(avoidballs[id]);
                    avdR.Add(avoidrobots[id]);
                    avdZ.Add(avoidzones[id]);
                    avdOz.Add(avoidOppzones[id]);

                }
                else
                    errts[i].Run(true, StopBall);
            }
            avoid.Clear();
            avoid.AddRange(avdBall);
            avoid.AddRange(avdBall);
            avoid.AddRange(avdZ);
            avoid.AddRange(avdZ);
            avoid.AddRange(avdOz);
            avoid.AddRange(avdOz);
            avoid.AddRange(avdR);
            avoid.AddRange(avdR);
            for (int i = 0; i < errts.Count; i++)
            {
                waits[i] = errts[i].eventFinish;
            }

            WaitHandle.WaitAll(waits);

            for (int i = 0; i < 2 * errts.Count; i++)
            {
                if (i < Count)
                {
                    EachPathCount[i] = errts[i].PathCount;
                    TotalFPath.AddRange(errts[i].FPath);

                    //List<Position2D> ppat = new List<Position2D>();
                    //errts[i].Path.ForEach(f => ppat.Add(new Position2D(f.Location.X, f.Location.Y)));
                    //DrawingObjects.AddObject(new DrawRegion(ppat, false, true, System.Drawing.Color.YellowGreen, System.Drawing.Color.YellowGreen), "kksksss" + i.ToString());
                }
                else if (i < 2 * Count)
                {
                    if (FinalPathes.ContainsKey(RobotIds[i - Count]) && FinalPathes[RobotIds[i - Count]].Count > 1)
                    {
                        int id = RobotIds[i - Count];
                        int pathc = FinalPathes[id].Count;
                        FinalPathesFloat[id][2 * (pathc - 1)] = (float)InitialStates[id].Location.X;
                        FinalPathesFloat[id][2 * (pathc - 1) + 1] = (float)InitialStates[id].Location.Y;
                        EachPathCount[i] = pathc;
                        TotalFPath.AddRange(FinalPathesFloat[id]);
                    }
                    else
                    {
                        EachPathCount[i] = 0;
                        TotalFPath.AddRange(tmpfpath);
                    }
                }
                else
                {
                    EachPathCount[i] = 0;
                    TotalFPath.AddRange(tmpfpath);
                }
            }
            obsX.Clear();
            obsY.Clear();
            obs.Clear();
            var o = AddAllObstacles(Model, ref obsX, ref obsY, ref obs, RobotIds, MotionPlannerParameters.kSpeedBall, MotionPlannerParameters.kSpeedRobot);
            
            GPPlanner.ForceTree(TotalFPath.ToArray(), EachPathCount, 2 * Count, avoid.ToArray(), finalPath, SmoothingCount, o, obsX.Count, Kspring, Kspring2, n, (StopBall)?1:0);
            FinalPathes.Clear();
            CurrentPathWeightes.Clear();
            LastPathWeightes.Clear();
            for (int i = 0; i <  Count; i++)
            {
                states.Clear();
                float[] possFloat = new float[2 * maxPathCount];
                for (int j = 0; j < EachPathCount[i]; j++)
                {
                    if (j < errts[i].Path.Count)
                    {
                        states.Add(new SingleObjectState(errts[i].Path[j]));
                        states[j].Location = new Position2D(finalPath[2 * j + i * 2 * maxPathCount], finalPath[2 * j + i * 2 * maxPathCount + 1]);
                        possFloat[2 * j] = (float)errts[i].Path[j].Location.X;
                        possFloat[2 * j + 1] = (float)errts[i].Path[j].Location.Y;
                    }
                }
                int id = RobotIds[i];
                FinalPathes[id] = states.ToList();
                FinalPathesFloat[id] = possFloat.ToArray();
                CurrentPathWeightes[id] = PathWeightCalculator(Model,intersectsLines[id], FinalPathes[id], id, errts[i].Obstacles, GoalStates[id]);
                //List<Position2D> ppat = new List<Position2D>();
                //FinalPathes[id].ForEach(f => ppat.Add(new Position2D(f.Location.X, f.Location.Y)));
                //DrawingObjects.AddObject(new DrawRegion(ppat, false, true, System.Drawing.Color.Blue, System.Drawing.Color.Blue), "kksk" + i.ToString());
            }

            lastFinalPathes.Clear();
            for (int i = Count; i < 2 * Count; i++)
            {
                int id = RobotIds[i - Count];
                if (LastGoals.ContainsKey(id) && LastGoals[id].Location.DistanceFrom(GoalStates[id].Location) > 0.01)
                    continue;
                states.Clear();
                float[] possFloat = new float[2 * maxPathCount];
                for (int j = 0; j < EachPathCount[i]; j++)
                {
                    states.Add(new SingleObjectState(new Position2D(finalPath[2 * j + i * 2 * maxPathCount], finalPath[2 * j + i * 2 * maxPathCount + 1]), Vector2D.Zero, 0));
                    possFloat[2 * j] = (float)states[j].Location.X;
                    possFloat[2 * j + 1] = (float)states[j].Location.Y;
                }
                lastFinalPathes[id] = states.ToList();
                LastPathWeightes[id] = PathWeightCalculator(Model,intersectsLines[id], lastFinalPathes[id], id, errts[i - Count].Obstacles,GoalStates[id]);

                if (LastPathWeightes[id] < CurrentPathWeightes[id] + 1)
                {
                    FinalPathes[id] = lastFinalPathes[id].ToList();
                    FinalPathesFloat[id] = possFloat.ToArray();
                }
                List<Position2D> ppat = new List<Position2D>();
                FinalPathes[id].ForEach(f => ppat.Add(new Position2D(f.Location.X, f.Location.Y)));
                DrawingObjects.AddObject("path" + i.ToString(), new DrawRegion(ppat, false, false, System.Drawing.Color.Red, System.Drawing.Color.Red));
            }

            LastGoals = new Dictionary<int, SingleObjectState>();
            for (int i = 0; i < Count; i++)
                LastGoals.Add(RobotIds[i], GoalStates[RobotIds[i]]);
            return FinalPathes; 
        }

        private double PathWeightCalculator(WorldModel Model, List<Line> Lines, List<SingleObjectState> Path, int RobotID, Obstacles obs, SingleObjectState Goal)
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
                foreach (var item in Lines)
                {
                    if (met)
                        break;
                    if (item.IntersectWithLine(new Line(Path[i].Location, Path[i - 1].Location), ref tmpp)
                        && ((tmpp - item.Tail).InnerProduct(item.Head - item.Tail) >= 0)
                        && ((tmpp - item.Head).InnerProduct(item.Tail - item.Head) >= 0)
                        && ((tmpp - Path[i].Location).InnerProduct(Path[i - 1].Location - Path[i].Location) >= 0)
                        && ((tmpp - Path[i - 1].Location).InnerProduct(Path[i].Location - Path[i - 1].Location) >= 0))
                    {
                        met = true;
                    }
                }
                if (!met && obs.Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01))
                    met = true;
                if (!metZone && 
                    (
                        (obs.ObstaclesList.ContainsKey(-2) && obs.ObstaclesList[-2].Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01))
                        || (obs.ObstaclesList.ContainsKey(-3) && obs.ObstaclesList[-3].Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01))
                        || (obs.ObstaclesList.ContainsKey(-4) && obs.ObstaclesList[-4].Meet(Path[i], Path[i - 1], MotionPlannerParameters.RobotRadi - 0.01))
                    )
                   )
                    metZone = true;
                Vec = Path[i].Location - Path[i - 1].Location;
                length += Vec.Size;
                Vec2 = Path[i - 1].Location - Path[i - 2].Location;
                angle += Math.Abs(Vector2D.AngleBetweenInRadians(Vec, Vec2));
            }
            Vec = Path[1].Location - Path[0].Location;
            if (!met && obs.Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi- 0.01))
                met = true;
             if (!metZone && 
                    (
                        (obs.ObstaclesList.ContainsKey(-2) && obs.ObstaclesList[-2].Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi - 0.01))
                        || (obs.ObstaclesList.ContainsKey(-3) && obs.ObstaclesList[-3].Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi - 0.01))
                        || (obs.ObstaclesList.ContainsKey(-4) && obs.ObstaclesList[-4].Meet(Path[1], Path[0], MotionPlannerParameters.RobotRadi - 0.01))
                    )
                   )
                    metZone = true;
             if (
                     (
                         (obs.ObstaclesList.ContainsKey(-2) && obs.ObstaclesList[-2].Meet(Path[0], MotionPlannerParameters.RobotRadi - 0.01))
                         || (obs.ObstaclesList.ContainsKey(-3) && obs.ObstaclesList[-3].Meet(Path[0], MotionPlannerParameters.RobotRadi - 0.01))
                         || (obs.ObstaclesList.ContainsKey(-4) && obs.ObstaclesList[-4].Meet(Path[0], MotionPlannerParameters.RobotRadi - 0.01))
                     )
                    )
             {
                 ;
             }
            length += Vec.Size;
            angle /= Path.Count;
            if(metZone)
                return (200000 + _angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length);
            if(met)
                return (100000 + _angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length);
            if ((Goal.Location - Path[0].Location).Size > 0.01)
                return (50000 + _angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length);
            return (_angleWeight * angle + _countWeight * Path.Count + _speedWieght * speed + _lengthWieght * length);
        }
        private float[] AddAllObstacles(WorldModel Model, ref List<float> obX, ref List<float> obY, ref List<float> ob,List<int> ids)
        {
            //List<int> tmpids = ids.ToList();
            obX.Add((float)Model.BallState.Location.X);
            obY.Add((float)Model.BallState.Location.Y);
            
            obX.Add((float)GameParameters.OurGoalCenter.X);
            obY.Add((float)GameParameters.OurGoalCenter.Y);
            //obX.Add((float)GameParameters.OurGoalCenter.X);
            //obY.Add((float)GameParameters.DefenceAreaFrontWidth / 2);
            //obX.Add((float)GameParameters.OurGoalCenter.X);
            //obY.Add(-(float)GameParameters.DefenceAreaFrontWidth / 2);

            obX.Add((float)GameParameters.OppGoalCenter.X);
            obY.Add((float)GameParameters.OppGoalCenter.Y);
            //obX.Add((float)GameParameters.OppGoalCenter.X);
            //obY.Add((float)GameParameters.DefenceAreaFrontWidth / 2);
            //obX.Add((float)GameParameters.OppGoalCenter.X);
            //obY.Add(-(float)GameParameters.DefenceAreaFrontWidth / 2);

            List<int> otherIds = Model.OurRobots.Keys.Except(ids).ToList();
            ids.ForEach(f => { obsX.Add((float)Model.OurRobots[f].Location.X); obsY.Add((float)Model.OurRobots[f].Location.Y); });
            otherIds.ForEach(f => { obsX.Add((float)Model.OurRobots[f].Location.X); obsY.Add((float)Model.OurRobots[f].Location.Y); });
            Model.Opponents.Values.ToList().ForEach(f => { obsX.Add((float)f.Location.X); obsY.Add((float)f.Location.Y); });
            ob.AddRange(obsX);
            ob.AddRange(obsY);
            return ob.ToArray();
        }
        private float[] AddAllObstacles(WorldModel Model, ref List<float> obX, ref List<float> obY, ref List<float> ob, List<int> ids,double kSpeedBall,double kSpeedRobot)
        {
            //List<int> tmpids = ids.ToList();

            obX.Add((float)(Model.BallState.Location.X + kSpeedBall * Model.BallState.Speed.X));
            obY.Add((float)(Model.BallState.Location.Y + kSpeedBall * Model.BallState.Speed.Y));

            obX.Add((float)GameParameters.OurGoalCenter.X);
            obY.Add((float)GameParameters.OurGoalCenter.Y);
            //obX.Add((float)GameParameters.OurGoalCenter.X);
            //obY.Add((float)GameParameters.DefenceAreaFrontWidth / 2);
            //obX.Add((float)GameParameters.OurGoalCenter.X);
            //obY.Add(-(float)GameParameters.DefenceAreaFrontWidth / 2);

            obX.Add((float)GameParameters.OppGoalCenter.X);
            obY.Add((float)GameParameters.OppGoalCenter.Y);
            //obX.Add((float)GameParameters.OppGoalCenter.X);
            //obY.Add((float)GameParameters.DefenceAreaFrontWidth / 2);
            //obX.Add((float)GameParameters.OppGoalCenter.X);
            //obY.Add(-(float)GameParameters.DefenceAreaFrontWidth / 2);

            List<int> otherIds = Model.OurRobots.Keys.Except(ids).ToList();
            ids.ForEach(f => { obsX.Add((float)(Model.OurRobots[f].Location.X + kSpeedRobot * Model.OurRobots[f].Speed.X)); obsY.Add((float)(Model.OurRobots[f].Location.Y + kSpeedRobot * Model.OurRobots[f].Speed.Y)); });
            otherIds.ForEach(f => { obsX.Add((float)(Model.OurRobots[f].Location.X + kSpeedRobot * Model.OurRobots[f].Speed.X)); obsY.Add((float)(Model.OurRobots[f].Location.Y+kSpeedRobot * Model.OurRobots[f].Speed.Y)); });
            Model.Opponents.Values.ToList().ForEach(f => { obsX.Add((float)(f.Location.X + kSpeedRobot * f.Speed.X)); obsY.Add((float)(f.Location.Y + kSpeedRobot * f.Speed.Y)); });
            ob.AddRange(obsX);
            ob.AddRange(obsY);
            return ob.ToArray();
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
