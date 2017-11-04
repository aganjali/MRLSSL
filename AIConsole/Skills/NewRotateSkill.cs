using MRL.SSL.CommonCLasses.MathLibarary;
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
    class NewRotateSkill:SkillBase
    {
        #region param
        bool isGo = false;
        bool isFirst = true;
        int catcherId = 0;
        int passerId = 0;
        bool isInRotate = false;
        public double ClockWise = 1;
        public Position2D GotoPointTarget = new Position2D();
        SingleWirelessCommand swc = new SingleWirelessCommand();
        Vector2D ballTargetVec = new Vector2D();
        Position2D robotFinalTargetPos = new Position2D();
        Vector2D targetVec = new Vector2D();
        public double gotoPointExtendSize = 0.13;
        public double gotoPointTeta = 0;
        private bool BallAvoided = false;
        private bool flag = false;
        public double BackBallDist = 0;
        public double gotoPointTresh = 0.006;
        public int GotoPointCounter = 0;
        public double GotoPointDelay = 60;
        bool isInRotateDelay = false;
        double robotFirstAngle = 0;
        bool isFirstangle = false;
        double angle = 0;

        int kickerID = 1;
        double radious = 0;
        double maxAngularVelocity = 5;
        bool isRotate = false;
        int counter = 0;
        Circle c;
        #endregion

        public void NewRotate(WorldModel Model, Position2D target, double teta, kickPowerType kickType, double kickSpeed, int RobotID, bool isChip)
        {
            if (Model.BallState.Location.DistanceFrom(Model.OurRobots[kickerID].Location) < 0.1)
            {
                isRotate = true;
                counter++;
            }
            if (!isRotate)
            {
                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[kickerID].Location) > 0.13)
                {
                    if (isFirstangle)
                    {
                        robotFirstAngle = Model.OurRobots[kickerID].Angle.Value;
                    }
                    if (robotFirstAngle > 180)
                    {
                        robotFirstAngle -= 360;
                    }
                    ballTargetVec = target - Model.BallState.Location;
                    DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + ballTargetVec), "tyui");
                    targetVec = Vector2D.FromAngleSize((ballTargetVec.AngleInDegrees + teta).ToRadian(), 0.13);
                    DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + targetVec, new Pen(Color.AliceBlue, 0.01f)), "rdhrhygjhj");
                    robotFinalTargetPos = Model.BallState.Location - targetVec;
                    DrawingObjects.AddObject(robotFinalTargetPos, "76776");
                    Planner.Add(kickerID, robotFinalTargetPos, targetVec.AngleInDegrees, PathType.Safe, true, true, true, true, false);
                    angle = targetVec.AngleInDegrees;
                    DrawingObjects.AddObject(new StringDraw("angle:" + angle, new Position2D(1, 2)), "rgh");

                }
                else
                {
                    DrawingObjects.AddObject(new StringDraw("Angle:" + angle, new Position2D(2, 1)), "ertyuk");
                    Planner.Add(kickerID, Model.OurRobots[kickerID].Location + (Model.BallState.Location - Model.OurRobots[kickerID].Location).GetNormalizeToCopy(0.03), angle, PathType.Safe, false, true, true, true, true);


                }
            }
            else
            {
                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[kickerID].Location) > 0.14)
                {
                    isRotate = false;
                }
                if (counter > 20)
                {
                    double ang = ballTargetVec.AngleInDegrees;
                    if (ang < 0)
                    {
                        ang += 360;
                    }
                    DrawingObjects.AddObject(new StringDraw(Math.Abs(Model.OurRobots[kickerID].Angle.Value - ang).ToString(), new Position2D(1, 1)), "dsfdsfds");

                    if (Math.Abs(Model.OurRobots[kickerID].Angle.Value - ang) < 10)
                        swc.SpinBack = 0;
                    else
                        swc.SpinBack = 1;

                    if (Math.Abs(Model.OurRobots[kickerID].Angle.Value - ang) < 5)
                        Planner.AddKick(kickerID, kickPowerType.Power, 180, false, false);

                    radious = 0.09;
                    swc.W += 0.08;
                    swc.RobotID = kickerID;
                    swc.KickSpeed = 0;
                    swc.Vx = swc.W * radious;
                    swc.Vy = 0;
                    swc.W = Math.Min(maxAngularVelocity, swc.W);
                    Planner.Add(kickerID, swc);
                }
                else
                {
                    Planner.Add(kickerID, Model.OurRobots[kickerID].Location + ((Model.BallState.Location - Model.OurRobots[kickerID].Location).GetNormalizeToCopy(0.008)), angle, PathType.Safe, false, true, true, true, true);
                }

            }
        }
    }
}
