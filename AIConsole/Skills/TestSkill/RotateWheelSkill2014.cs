using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.GameDefinitions.General_Settings;

namespace MRL.SSL.AIConsole.Skills
{
    class RotateWheelSkill2014 : SkillBase
    {
        static int? goalieID = null;
        SingleWirelessCommand SWC;
        Obstacle obs = new Obstacle();

        public static double vx = 0, vy = 0, w = 0;
        public RotateWheelSkill2014()
        {
            SWC = new SingleWirelessCommand();
        }
        public SingleWirelessCommand Rotate(GameStrategyEngine engine, WorldModel model, int RobotID, double kickSpeed)
        {
            goalieID = GoalieID(model);
            Line goalLine = new Line(GameParameters.OppGoalLeft, GameParameters.OppGoalRight);
            Line ballAndRobotLine = new Line(model.BallState.Location, model.OurRobots[RobotID].Location);
            Position2D? intersect = ballAndRobotLine.IntersectWithLine(goalLine);
            if (!intersect.HasValue)
            {
                intersect = GameParameters.OppGoalLeft;
            }
            DrawingObjects.AddObject(intersect, "intersectLine");
            obs.Type = ObstacleType.OppRobot;
            obs.State = model.Opponents[goalieID.Value];
            if(intersect.HasValue&&goalieID.HasValue&&!obs.Meet(


        }
        public int? GoalieID(WorldModel model)
        {
            int? goalieID = null;
            double min = double.MaxValue;
            foreach (var item in model.Opponents.Keys)
            {
                if (model.Opponents[item].Location.DistanceFrom(GameParameters.OppGoalCenter) < min)
                {
                    min = model.Opponents[item].Location.DistanceFrom(GameParameters.OppGoalCenter);
                   goalieID=item;
                }
            }

            return goalieID;
        }
    }

}
