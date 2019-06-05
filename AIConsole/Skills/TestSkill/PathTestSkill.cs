using MRL.SSL.AIConsole.Engine;

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Skills.TestSkill
{
    class PathTestSkill : SkillBase
    {
        List<SingleObjectState> LastPath = null;
        ERRT errt = new ERRT(true);
        float[] temp;

        public void Perform(WorldModel Model, int RobotID)
        {
            Position2D init = new Position2D(-2, 0.5);
            Position2D goal = new Position2D(0, .5);


            errt.Run(Model, RobotID, new List<Line>(), new SingleObjectState(init, Vector2D.Zero, 0), new SingleObjectState(goal, Vector2D.Zero, 0), 1, 1, 1, 1, LastPath, PathType.UnSafe , false);

            
            errt.Run(Model, RobotID, new List<Line>(), new SingleObjectState(init, Vector2D.Zero, 0), new SingleObjectState(goal, Vector2D.Zero, 0), 1, 1, 1, 1, LastPath, PathType.UnSafe, false);

            errt.eventFinish.WaitOne();
            List<Position2D> ppat = new List<Position2D>();
            errt.Path.ForEach(f => ppat.Add(new Position2D(f.Location.X, f.Location.Y)));

            List<Position2D> spat = new List<Position2D>();
            List<Position2D> noSmooth = new List<Position2D>();
            errt.Path.ForEach(f => noSmooth.Add(new Position2D(f.Location.X, f.Location.Y)));
            LastPath = errt.Path;
            //for (int i = 0; i < errt.Path.Count; i++)
            var path = errt.FPath;
            for (int i = 0; i < path.Length; i += 2)
            {
                if (path[i] != 0)
                {

                    ppat.Add(CalQ(path[i], path[i + 1], path[i + 2], path[i + 3]));
                    ppat.Add(CalR(path[i], path[i + 1], path[i + 2], path[i + 3]));




                }

            }
            ppat.Add(goal);

            //for (int i = 0; i < ppat.Count; i++)
            //{

            //    Console.WriteLine("Point " + i.ToString() + ":" + "X = " + errt.Path[i].Location.X + "| Y = " + errt.Path[i].Location.Y);
            //    spat.Add(CalQ((float)ppat[i].X, (float)ppat[i].Y, (float)ppat[i + 1].X, (float)ppat[i + 1].Y));
            //    spat.Add(CalR((float)ppat[i].X, (float)ppat[i].Y, (float)ppat[i + 1].X, (float)ppat[i + 1].Y));


            //}
            //spat.Add(goal);

            var a = CalQ(2, 2, 2, 6);
            var b = CalR(2, 2, 2, 6);
            DrawingObjects.AddObject("OneSmooth", new DrawRegion(ppat, false, false, System.Drawing.Color.Blue, System.Drawing.Color.Blue));
            DrawingObjects.AddObject("Raw", new DrawRegion(noSmooth, false, false, System.Drawing.Color.Red, System.Drawing.Color.Red));
           // DrawingObjects.AddObject("SecondSmooth", new DrawRegion(spat, false, false, System.Drawing.Color.Yellow, System.Drawing.Color.Yellow));



        }
        Position2D CalQ(float x1, float y1, float x2, float y2)
        {
            x1 = 3f / 4f * x1;
            y1 = 3f / 4f * y1;
            x2 = 1f / 4f * x2;
            y2 = 1f / 4f * y2;
            return new Position2D((x1 + x2), (y1 + y2));

        }
        Position2D CalR(float x1, float y1, float x2, float y2)
        {
            x1 = 1f / 4f * x1;
            y1 = 1f / 4f * y1;
            x2 = 3f / 4f * x2;
            y2 = 3f / 4f * y2;
            return new Position2D((x1 + x2), (y1 + y2));
            //LastPath = errt.Path;
            //for (int i = 0; i < errt.Path.Count; i++)
            //{

            //    Console.WriteLine("Point " + i.ToString() + ":" + "X = " + errt.Path[i].Location.X + "| Y = " + errt.Path[i].Location.Y);
            //}
            //DrawingObjects.AddObject("path_test_errt", new DrawRegion(ppat, false, false, System.Drawing.Color.Red, System.Drawing.Color.Red));
        }
    }

}

