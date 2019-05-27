using System;
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
        public void perform(WorldModel Model, int RobotID)
        {
            Position2D init = new Position2D(-2, 0.5);
            Position2D goal = new Position2D(0, .5);

            errt.Run(Model, RobotID, new List<Line>(), new SingleObjectState(init, Vector2D.Zero, 0), new SingleObjectState(goal, Vector2D.Zero, 0), 1, 1, 1, 1, LastPath, PathType.UnSafe);
            errt.eventFinish.WaitOne();
            List<Position2D> ppat = new List<Position2D>();
            errt.Path.ForEach(f => ppat.Add(new Position2D(f.Location.X, f.Location.Y)));
            LastPath = errt.Path;
            for (int i = 0; i < errt.Path.Count; i++)
            {

                Console.WriteLine("Point " + i.ToString() + ":" + "X = " + errt.Path[i].Location.X + "| Y = " + errt.Path[i].Location.Y);
            }
            DrawingObjects.AddObject("path_test_errt", new DrawRegion(ppat, false, false, System.Drawing.Color.Red, System.Drawing.Color.Red));
        }
    }
}