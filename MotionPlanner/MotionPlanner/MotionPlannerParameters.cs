using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.Planning.MotionPlanner
{
    public static class MotionPlannerParameters
    {
        public static double RobotRadi = RobotParameters.OurRobotParams.Diameter / 2 + 0.0;
        public static double BallRadi = 0.022, StopBallRadi = 0.4;

        //TODO: DANGER_ZONE CHECK
        public static double DangerZoneH = GameParameters.DefenceAreaHeight;
        public static double DangerZoneW = GameParameters.DefenceAreaWidth + 0.01;
        
        public static double LengthMargin = 0.05;
        public static double WidthMargin = 0.05;
        public static double FieldLength = Math.Abs(GameParameters.OurGoalCenter.X - GameParameters.OppGoalCenter.X) + 2*LengthMargin;
        public static double FieldWidth = Math.Abs(GameParameters.OurLeftCorner.Y - GameParameters.OurRightCorner.Y) + 2*WidthMargin;
        public static double FieldWidth_H = FieldWidth / 2;
        public static double FieldLength_H = FieldLength / 2;
        public static double kSpeedRobot = 0;
        public static double kSpeedBall = 0.0;
        public static double GoalExtendFromDangerZoneMargin = 0.11;
    }
}
