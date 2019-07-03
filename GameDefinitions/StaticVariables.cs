using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
namespace MRL.SSL.GameDefinitions
{
    public static class StaticVariables
    {
        public static double[] ChipCoef = { 1, 1, 1.0, 1, 1.0, 1, 1, 1.0, 1, 1, 1, 1, 1, 1, 1, 1 };
        public static double[] DirectCoef = { 1.0, 1.0, 1.0, 1, 1.0, 1, 1, 1, 1.0, 1, 1, 1, 1, 1, 1, 1 };

        public static double BallPredictTime = 1.5;
        public static double RobotPredictTime = 0.03333333333333333333333333333333;
        public static double action_delay = 0.03;
        public static float FRAME_RATE = 73.0f;
        public static float FRAME_PERIOD = (1.0f / FRAME_RATE);
        public static double viewDelay = FRAME_PERIOD;// 0.01666666666666666666666666666667;
        public static double MaxPredictTime = 10;
        public static int MaxBalls = 16;
        public static double LATENCY_DELAY = FRAME_PERIOD;//0.01666666666666666666666666666667;
  
        public static double ROBOT_PRINT_KALMAN_ERROR = 0;
        public static int ROBOT_FAST_PREDICT = 0;
        public static int ROBOT_USE_AVERAGES_IN_PROPAGATION = 0;
        public static double ROBOT_CONFIDENCE_THRESHOLD = 0.5;
        public static double ROBOT_POSITION_VARIANCE = 4;
        public static double ROBOT_THETA_VARIANCE = 0.00006;
        public static double ROBOT_NONE_VELOCITY_VARIANCE = 250000;
        public static double ROBOT_NONE_ANGVEL_VARIANCE = 0.0;
        public static double ROBOT_DIFF_VELOCITY_VARIANCE = 10000;
        public static double ROBOT_DIFF_VELOCITY_VARIANCE_PERP = 25;
        public static double ROBOT_DIFF_ANGVEL_VARIANCE = 0.25;
        public static double ROBOT_OMNI_VELOCITY_VARIANCE = 10000.0;
        public static double ROBOT_OMNI_ANGVEL_VARIANCE = 1.0;
        public static double ROBOT_STUCK_VARIANCE = 0.2;
        public static double ROBOT_VELOCITY_NEXT_STEP_COVARIANCE = 1;
        public static double ROBOT_STUCK_DECAY = 0.9;
        public static double ROBOT_STUCK_THRESHOLD = 0.81;
        public static bool BALL_PRINT_KALMAN_ERROR = false;
        public static double BALL_VELOCITY_VARIANCE_NEAR_ROBOT = 40000;
        public static int NUM_TEAMS = 2, /*MAX_TEAM_ROBOTS = 12,*/ MAX_ROBOT_ID = 16;
        public static double ROBOT_DEF_WIDTH_H = 90;
        public static double BALL_VELOCITY_VARIANCE_NO_ROBOT = 100;
        public static double CAMERA_HEIGHT, BALL_RADIUS = 0.022;
        public static double FieldMargin = 0.25;
        public static double BALL_TEAMMATE_COLLISION_RADIUS = 100; 
        public static double BALL_OPPONENT_COLLISION_RADIUS = 100;
        public static double BALL_CONFIDENCE_THRESHOLD = .1;
        public static double BALL_POSITION_VARIANCE = 16;
        public static double BALL_IMPROBABILITY_FILTERING = 0;
        public static double BALL_LIKELIHOOD_THRESHOLD = 0.5;
        public const double PassSpeedTresh = 0.4;
        public static double BALL_OCCLUDE_TIME = 0.1;
        public static double BALL_FRICTION = 0.0306;
        public static double GRAVITY = 9800;
        public static bool BALL_WALLS_SLOPED = false;
        public static double FIELD_LENGTH_H { get; set; }
        public static double GOAL_WIDTH_H { get; set; }
        public static double FIELD_WIDTH_H { get; set; }
        public static double M_SQRT1_2 = Math.Sqrt(2) / 2;
        public static bool BALL_WALLS_OOB = false;
        public static double MaxRobotCounts = 8.0;
        public static double WALL_WIDTH { get; set; }
        public static double MaxKickSpeed = 6.5;

        public static bool OldRefbox = true ;
        public static List<Position2D> BallPositions = new List<Position2D>();
        public static bool FrameHasBall = false;
        public static Position2D ballPlacementPos = new Position2D();
        public const int CameraCount = 2;
        public const int VisionPcCounts = 2;
    }

}
