using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.AIConsole.Roles
{
    public static class DataBridge
    {
        //------------------------------------ Roles ------------------------------
        //----------------------CenterBack-------------------------
        public static int CenterBackID = 1000;
        public static Position2D CenterBacktarget = new Position2D();
        public static int? CenterBackOppID = 1000;
        public static bool CenterBackIsBallTarget = false;
        //----------------------CornerRole1-------------------------
        public static int CornerRole1ID = 1000;
        public static Position2D CornerRole1target = new Position2D();
        public static int? CornerRole1OppID = 1000;
        public static bool CornerRole1IsBallTarget = false;

        //---------------------CornerRole2---------------------------
        public static int CornerRole2ID = 1000;
        public static Position2D CornerRole2target = new Position2D();
        public static int? CornerRole2OppID = 1000;
        public static bool CornerRole2IsBallTarget = false;

        //--------------------CornerRole3-----------------------------
        public static int CornerRole3ID = 1000;
        public static Position2D CornerRole3target = new Position2D();
        public static int? CornerRole3OppID = 1000;
        public static bool CornerRole3IsBallTarget = false;

        //--------------------CornerRole4-----------------------------
        public static int CornerRole4ID = 1000;
        public static Position2D CornerRole4target = new Position2D();
        public static int? CornerRole4OppID = 1000;
        public static bool CornerRole4IsBallTarget = false;

        //--------------------LBMarkerRole-----------------------------
        public static int LBMarkerRoleID = 1000;
        public static Position2D LBMarkerRoletarget = new Position2D();
        public static int? LBMarkerRoleOppID = 1000;
        public static bool LBMarkerRoleIsBallTarget = false;

        //--------------------RBMarkerRole-----------------------------
        public static int RBMarkerRoleID = 1000;
        public static Position2D RBMarkerRoletarget = new Position2D();
        public static int? RBMarkerRoleOppID = 1000;
        public static bool RBMarkerRoleIsBallTarget = false;

        //--------------------MarkerRole1-----------------------------
        public static int MarkerRole1ID = 1000;
        public static Position2D MarkerRole1target = new Position2D();
        public static int? MarkerRole1OppID = 1000;
        public static bool MarkerRole1IsBallTarget = false;

        //--------------------MarkerRole2-----------------------------
        public static int MarkerRole2ID = 1000;
        public static Position2D MarkerRole2target = new Position2D();
        public static int? MarkerRole2OppID = 1000;
        public static bool MarkerRole2IsBallTarget = false;

        //--------------------MarkerRole3-----------------------------
        public static int MarkerRole3ID = 1000;
        public static Position2D MarkerRole3target = new Position2D();
        public static int? MarkerRole3OppID = 1000;
        public static bool MarkerRole3IsBallTarget = false;

        //--------------------Regional-----------------------------
        public static int RegionalRoleID = 1000;
        public static Position2D RegionalRoletarget = new Position2D();

        //--------------------Regional 2-----------------------------
        public static int Regional2RoleID = 1000;
        public static Position2D Regional2Roletarget = new Position2D();

        //--------------------Stop Corver-----------------------------
        public static int StopCorverRoleID = 1000;
        public static Position2D StopCorverRoletarget = new Position2D();

        //---------------------- Speciefic Situation -----------------------------

        //----------------------CUT BALL-------------------------------
        public static bool BallCutSituationCR1 = false;
        public static bool BallCutSituationCR2 = false;
        public static bool BallCutSituationCR3 = false;
        public static bool BallCutSituationCR4 = false;
        public static Position2D BallCutPos = new Position2D();
        public static int CutBallRobotIDCR1 = 1000;
        public static int CutBallRobotIDCR2 = 1000;
        public static int CutBallRobotIDCR3 = 1000;
        public static int CutBallRobotIDCR4 = 1000;
        //
        public static bool BallCutSituationCBR = false;
        public static int CutBallRobotIDCBR = 1000;


        //public static int CutBallRobotID = 1000;
        //public static bool BallCutSituation = false;
        public static double balltime = 0;
        public static double Robottime = 0;
        public static Position2D InitialDefenderCut = new Position2D();
        public static Position2D TargetDefenderCut = new Position2D();
        public static bool getActive = false;
        public static bool farFlag = false;

        //----------------------Ball Behind---------------------------------------

        public static bool BallBehind = false;
        public static int BallBehindID = 1000;
        public static Position2D BallBehindPos = new Position2D();
        public static double BallBehindangle = 0;

        //------------------------------------------------------------------------
        public static Dictionary<int, Position2D> ourRobots = new Dictionary<int, Position2D>();
        public static Dictionary<int, Position2D> opponents = new Dictionary<int, Position2D>();
        public static bool staticCutAssign = false;

        public static void SetInitialPoses(WorldModel model)
        {
            foreach (int item in model.OurRobots.Keys)
            {
                if (model.OurRobots[item].Speed.Size < .3)
                {
                    ourRobots.Remove(item);
                    ourRobots.Add(item , model.OurRobots[item].Location);
                }
            }
            foreach (int item in model.Opponents.Keys)
            {
                if (model.Opponents[item].Speed.Size < .3)
                {
                    opponents.Remove(item);
                    opponents.Add(item, model.Opponents[item].Location);
                }
            }
        }
    }
}
