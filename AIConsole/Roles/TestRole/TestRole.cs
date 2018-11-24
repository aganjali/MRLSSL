using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.IO;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public class TestRole : RoleBase
    {
        public void RrtTest(WorldModel model, GameStrategyEngine engine, int robotid)
        {
            int AvoidRobot = 1, AvoidBall = 1, AvoidZone = 1, AvoidOppZone = 1;
            Obstacles obs = new Obstacles(model);
            obs.AddObstacle(AvoidRobot, AvoidBall, AvoidZone, AvoidOppZone, new List<int>() { robotid }, null);

            if (rrt == null)
                rrt = new ERRT(false);
            rrt.Run(model, robotid, new List<Line>(), new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0), new SingleObjectState(new Position2D(2, 2), Vector2D.Zero, 0), AvoidBall, AvoidZone, AvoidOppZone, AvoidRobot, null, PathType.UnSafe, false);
            rrt.eventFinish.WaitOne();
            List<Position2D> path = new List<Position2D>();
            List<Position2D> li = new List<Position2D>();
            Position2D F = new Position2D();
            //for (int i = 0; i < rrt.PathCount; i++)
            //{
            //    path.Add(new Position2D(rrt.FPath[2 * i], rrt.FPath[2 * i + 1]));
            //}
            for (int i = 0; i < rrt.PathCount; i++)
            {
                //if (!obs.Meet(new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0), new SingleObjectState(new Position2D(rrt.FPath[2 * i], rrt.FPath[2 * i + 1]), Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi))
                //{}

                    path.Add(new Position2D(rrt.FPath[2 * i], rrt.FPath[2 * i + 1]));
                
            }

            DrawingObjects.AddObject("pathnode", new DrawRegion(path, false, false, Color.Red));
        }
        ERRT rrt;


        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 0;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return false;
        }
    }

}
