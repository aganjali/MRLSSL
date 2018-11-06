using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Skills
{
    class Test1 : SkillBase
    {

        bool reachCenter = true;
        double i = 0;
        double temp = 0;
        bool flag = true;
        Position2D first = new Position2D(1, 1);
        Position2D second = new Position2D(2, 2);
        Position2D target = new Position2D();
        public void MoveToPoint(WorldModel model, GameStrategyEngine engine, int robotId, Position2D center, double r)
        {


            Planner.Add(robotId, center, (target - model.OurRobots[robotId].Location).AngleInDegrees);

            if (reachCenter && model.OurRobots[robotId].Location.DistanceFrom(target) > 0.009)
            {
                target = center;
            }
            else
            {
                reachCenter = false;
                Vector2D v = Vector2D.FromAngleSize(r, i.ToRadian());
                target = center + v;

                if (model.OurRobots[robotId].Location.DistanceFrom(target) < 0.009)
                {
                    i+=10;
                    reachCenter = false;
                    if (i == 360)
                    {
                        i = 0;
                    }

                }

            }

            Planner.Add(robotId, target, (target - model.OurRobots[robotId].Location).AngleInDegrees);

            //if (model.OurRobots[robotId].Location.DistanceFrom(first) > 0.009 && flag)
            //{
            //    target = first;
            //}
            //else
            //{
            //    target = second;
            //    flag = false;
            //    if (model.OurRobots[robotId].Location.DistanceFrom(second) < 0.009)
            //    {
            //        flag = true;
            //    }
            //}
            //Planner.Add(robotId, target, (target - model.OurRobots[robotId].Location).AngleInDegrees);
        }


    }
}
