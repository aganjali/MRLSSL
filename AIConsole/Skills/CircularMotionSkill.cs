using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
namespace MRL.SSL.AIConsole.Skills
{
    public class CircularMotionSkill
    {
        int counter = 0;
        public bool reachedFlag = false;
        double StaticW = 3.2;
        double angleTresh = 11;
        SingleWirelessCommand SWC = new SingleWirelessCommand();
        public void perform(WorldModel Model, int RobotID/*, double KickSpeed */, Position2D KickTarget, double R, bool isLeft, double Vy = 1)
        {
            counter++;
            SWC.RobotID = RobotID;
            SWC.SpinBack = 200;
            SWC.Vx = 0;

            Vector2D robotToTargetVec = KickTarget - Model.OurRobots[RobotID].Location;
            double robotAngle = GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value);
            //DrawingObjects.AddObject(new StringDraw("Robot Angle : " + robotAngle.ToString(), Color.FromArgb(1, 233, 146, 53), Model.OurRobots[RobotID].Location.Extend(0.4, 0)));
            //DrawingObjects.AddObject(new StringDraw("Final Angle : " + robotToTargetVec.AngleInDegrees.ToString(), Color.FromArgb(1, 233, 146, 53), Model.OurRobots[RobotID].Location.Extend(0.5, 0)));
            Vector2D robotAngleVec = Vector2D.FromAngleSize(Math.PI * Model.OurRobots[RobotID].Angle.Value / 180.0, Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OppGoalCenter));
            //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + robotAngleVec, new Pen(Color.FromArgb(2, 233, 146, 53), .03f)));
            //DrawingObjects.AddObject(new StringDraw("Vy = " + SWC.Vy.ToString(), Model.OurRobots[RobotID].Location.Extend(0.6, 0)));
            //DrawingObjects.AddObject(new StringDraw("Diffrence =" + (robotAngleVec.AngleInDegrees - robotToTargetVec.AngleInDegrees).ToString(), Model.OurRobots[RobotID].Location.Extend(0.7, 0)));

            if (!reachedFlag)
            {
                double at = (counter / 5) * 0.03;//* (robotAngleVec.AngleInDegrees - robotToGoalVec.AngleInDegrees) / 1000
                Vy = (R == 0) ? 0 : Vy;
                SWC.Vy = Vy;// +at;
                if (Vy != 0)
                {
                    SWC.W = isLeft ? Math.Abs(Vy) / R : -(Math.Abs(Vy) / R);
                }
                else
                {
                    SWC.W = isLeft ? StaticW : -StaticW;
                }
                if (robotAngleVec.AngleInDegrees - robotToTargetVec.AngleInDegrees > -angleTresh &&
                    robotAngleVec.AngleInDegrees - robotToTargetVec.AngleInDegrees < +angleTresh)
                {
                    //counter++;
                    //if (counter > 12)
                    //{
                    if (SWC.SpinBack == 0)
                        reachedFlag = true;
                    SWC.SpinBack = 0;
                    //}
                }
                SWC.KickPower = 0;
                Planner.Add(RobotID, SWC);
            }
            else
            {
                SWC.SpinBack = 0;
                SWC.KickPower = 0;
                SWC.W = 0;
                SWC.Vy = 0;
            }
        }
        public void staticRotate(WorldModel Model, int RobotID, Position2D KickTarget, bool isLeft, double angleTresh = 11, double W = 3)
        {
            StaticW = W;
            perform(Model, RobotID, KickTarget, 0, isLeft, 0);
        }

    }
}
