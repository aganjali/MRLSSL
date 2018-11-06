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
    class Test1Skill : SkillBase
    {

        bool reachCenter = true;
        double i = 0;
        double step = 10;
        double temp = 0;
        bool flag = true;
        bool isBetween = false;
        Position2D first = new Position2D(1, 1);
        Position2D second = new Position2D(2, 2);
        Position2D target = new Position2D();
        Position2D extendedTarget = new Position2D();
        public void MoveToPoint(WorldModel model, GameStrategyEngine engine, int robotId, Position2D center, double r)
        {




            

            if (reachCenter && model.OurRobots[robotId].Location.DistanceFrom(extendedTarget) > 0.009)
            {
                extendedTarget = center;
            }
            else
            {
                reachCenter = false;
                Vector2D v = Vector2D.FromAngleSize(i.ToRadian(), r);
                Vector2D robotVector = new Vector2D();
                robotVector = model.OurRobots[robotId].Location - center;
                target = center + v;
                Vector2D o = Vector2D.FromAngleSize((i - step).ToRadian(), r);
                Vector2D k = v - o;
                k.NormalizeTo(1);
                extendedTarget = target + k;
                if (i == 0)
                {
                    extendedTarget = target;
                }
                Position2D tar = new Position2D();
                Position2D last= new Position2D();
                tar = center + o;
                last = center + v;
                isBetween = Position2D.IsBetween(tar, last, model.OurRobots[robotId].Location);
                if (isBetween)
                {
                    i += step;
                    reachCenter = false;
                    if (i == 360)
                    {
                        i = 0;
                    }

                }

            }

            Planner.Add(robotId, extendedTarget, (extendedTarget - model.OurRobots[robotId].Location).AngleInDegrees);

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
