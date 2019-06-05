using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRL.SSL.AIConsole.Skills.TestSkill
{
    class PathTestSkill : SkillBase
    {
        List<SingleObjectState> LastPath = null;
        ERRT errt = new ERRT(false);

        public void Perform(WorldModel Model, int RobotID)
        {
            Position2D init = new Position2D(-2, 0.5);
            Position2D goal = new Position2D(0, .5);
            int N = 2;

            errt.Run(Model, RobotID, new List<Line>(), new SingleObjectState(init, Vector2D.Zero, 0), new SingleObjectState(goal, Vector2D.Zero, 0), 1, 1, 1, 1, LastPath, PathType.UnSafe, false);
            errt.eventFinish.WaitOne();
            List<Position2D> ppat = new List<Position2D>();
            //errt.Path.ForEach(f => ppat.Add(new Position2D(f.Location.X, f.Location.Y)));
            List<Position2D> spat = new List<Position2D>();
            List<Position2D> noSmooth = new List<Position2D>();
            errt.Path.ForEach(f => noSmooth.Add(new Position2D(f.Location.X, f.Location.Y)));

            //for (int i = 0; i < errt.Path.Count; i++)
            for (int k = 0; k < N; k++)
            {
                ppat = new List<Position2D>();
                var path = errt.Path;
                for (int i = 0; i < path.Count - 1; i += 2)
                {
                    ppat.Add(CalQ(path[i].Location.X, path[i].Location.Y, path[i + 1].Location.X, path[i + 1].Location.Y));
                    ppat.Add(CalR(path[i].Location.X, path[i].Location.Y, path[i + 1].Location.X, path[i + 1].Location.Y));

                }
                ppat.Add(init);
                errt.Path = new List<SingleObjectState>();
                ppat.ForEach(f => errt.Path.Add(new SingleObjectState(f, Vector2D.Zero, 0)));
            }
            LastPath = errt.Path;
            // ppat.Add(goal);

            //for (int i = 0; i < ppat.Count; i++)
            //{

            //    Console.WriteLine("Point " + i.ToString() + ":" + "X = " + errt.Path[i].Location.X + "| Y = " + errt.Path[i].Location.Y);
            //    spat.Add(CalQ((float)ppat[i].X, (float)ppat[i].Y, (float)ppat[i + 1].X, (float)ppat[i + 1].Y));
            //    spat.Add(CalR((float)ppat[i].X, (float)ppat[i].Y, (float)ppat[i + 1].X, (float)ppat[i + 1].Y));


            //}
            //spat.Add(goal);

            var a = CalQ(2, 2, 2, 6);
            var b = CalR(2, 2, 2, 6);
            DrawingObjects.AddObject("OneSmooth", new DrawRegion(ppat, false, false, System.Drawing.Color.Red, System.Drawing.Color.Red));
            DrawingObjects.AddObject("Raw", new DrawRegion(noSmooth, false, false, System.Drawing.Color.Blue, System.Drawing.Color.Blue));
            // DrawingObjects.AddObject("SecondSmooth", new DrawRegion(spat, false, false, System.Drawing.Color.Yellow, System.Drawing.Color.Yellow));



        }
        Random r = new Random(DateTime.Now.Millisecond);
        public void RandomInterpolateSmoothingTest(WorldModel Model, int RobotID)
        {
            Position2D init = new Position2D(-2, 0.5);
            Position2D goal = new Position2D(0, .5);
            List<SingleObjectState> ppat = new List<SingleObjectState>();
            
            int avoidBall = 1, avoidRobot = 1, avoidZone = 1, avoidOppZone = 1;            
            int N = 10;

            errt.Run(Model, RobotID, new List<Line>(), new SingleObjectState(init, Vector2D.Zero, 0), new SingleObjectState(goal, Vector2D.Zero, 0), avoidBall, 
                avoidZone, avoidOppZone, avoidRobot, LastPath, PathType.UnSafe, false);
            errt.eventFinish.WaitOne();
            for (int m = 0; m < errt.Path.Count; m++)
            {
                ppat.Add(errt.Path[m]);
            }
            Obstacles obs = new Obstacles(Model);

            obs.AddObstacle(avoidRobot, avoidBall, avoidZone, avoidOppZone, new List<int>() { RobotID }, null, MotionPlannerParameters.kSpeedBall, MotionPlannerParameters.kSpeedRobot, false);

            for (int i = 0; i < errt.Path.Count ; i++)
            {
                List<int> nodes = new List<int>();
                for (int k = 0; k < ppat.Count; k++)
                {
                    nodes.Add(k);
                }
                if (ppat.Count == 2)
                    break;

                int s = r.Next(0, nodes.Count);
                int sKey = nodes[s];
                SingleObjectState start = ppat[sKey];
                nodes.RemoveAt(s);
                if (s > 0 && s < ppat.Count - 1)
                    nodes.RemoveAt(s);
                if (s - 1 >= 0 && nodes.Count > s - 1)
                    nodes.RemoveAt(s - 1);
                if (nodes.Count == 0)
                    continue;
                int e = r.Next(0, nodes.Count);
                int eKey = nodes[e];
                SingleObjectState end = ppat[eKey];


                if (!obs.Meet(start, end, MotionPlannerParameters.RobotRadi))
                {
                    int min = (sKey < eKey) ? sKey : eKey;
                    int max = (sKey > eKey) ? sKey : eKey;
                    if (max - min > 1)
                        ppat.RemoveRange(min + 1, max - min - 1);
                }
            }
        
        
            
            List<Position2D> smoothPath = new List<Position2D>(), rawPath = new List<Position2D>();
            LastPath = errt.Path;
            ppat.ForEach(f => smoothPath.Add(f.Location));
            errt.Path.ForEach(f => rawPath.Add(f.Location));

            DrawingObjects.AddObject("OneSmooth", new DrawRegion(smoothPath, false, false, System.Drawing.Color.Red, System.Drawing.Color.Red));
            DrawingObjects.AddObject("Raw", new DrawRegion(rawPath, false, false, System.Drawing.Color.Blue, System.Drawing.Color.Blue));

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
    }
}
