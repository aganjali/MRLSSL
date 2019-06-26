using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Skills.GoalieSkills
{
    class GoalieDiveSkill2017 : SkillBase
    {
        GetBallSkill gb = new GetBallSkill();
        public void Dive(GameStrategyEngine Engine, WorldModel Model, int RobotID, bool IsChipKick, double KickPower)
        {
            bool debug = true;
            //double robotToGoalCornersDistTresh = 0.70;

            Position2D ballLoc = Model.BallState.Location, robotLoc = Model.OurRobots[RobotID].Location;
            Line ballSpeedLine = new Line(Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(1), ballLoc);
            Position2D? intWithGoalLine = ballSpeedLine.IntersectWithLine(new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight));
            Position2D? posToGo = null;

            if (intWithGoalLine.HasValue)
            {
                if (debug)
                {
                    //DrawingObjects.AddObject(new StringDraw("L to R innerProduct =" + (Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), Model.BallState.Speed) / Model.BallState.Speed.Size).ToString(), new Position2D(1, 1)));
                    //DrawingObjects.AddObject(new StringDraw(Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree().ToString(), new Position2D(1.2, 1)));
                    //DrawingObjects.AddObject(new StringDraw("R to L innerProduct =" + (Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), Model.BallState.Speed) / Model.BallState.Speed.Size).ToString(), new Position2D(1.3, 1)));
                    //DrawingObjects.AddObject(new StringDraw(Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree().ToString(), new Position2D(1.4, 1)));
                }
                if (robotLoc.Y > intWithGoalLine.Value.Y &&
                    intWithGoalLine.Value.Y < GameParameters.OurGoalRight.Y)
                {
                    //11/25/2017 added vahid
                    Position2D? target = null;
                    if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree() <= 145)
                    {
                        target = GameParameters.OurGoalRight.Extend(-0.12, 0.02);
                    }
                    //else if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree() > 145)
                    //{
                    //    target = GameParameters.OurGoalRight.Extend(-0.12, -0.07);
                    //}
                    if (target.HasValue)
                    {
                        DrawingObjects.AddObject(new Circle(target.Value, .13, new System.Drawing.Pen(Brushes.HotPink, .02f)));
                        Planner.Add(RobotID, target.Value, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                        return;
                    }
                }
                if (robotLoc.Y < intWithGoalLine.Value.Y &&
                    intWithGoalLine.Value.Y > GameParameters.OurGoalLeft.Y)
                {
                    //11/25/2017 added vahid
                    Position2D? target = null;
                    if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree() <= 145)
                    {
                        target = GameParameters.OurGoalLeft.Extend(-0.12, -0.02);
                    }
                    //else if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree() > 145)
                    //{
                    //    target = GameParameters.OurGoalLeft.Extend(-0.10, -0.05);
                    //}
                    if (target.HasValue)
                    {
                        DrawingObjects.AddObject(new Circle(target.Value, .20, new System.Drawing.Pen(Brushes.HotPink, .02f)));
                        Planner.Add(RobotID, target.Value, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                        return;
                    }
                }
                Position2D? goalieYLineWithBallSpeedIntersect = ballSpeedLine.IntersectWithLine(new Line(robotLoc, robotLoc.Extend(0, 1)));
                Position2D? PrepWithBallSpeedIntersect = Model.BallState.Speed.PrependecularPoint(ballLoc, robotLoc);

                if (!goalieYLineWithBallSpeedIntersect.HasValue)
                    goalieYLineWithBallSpeedIntersect = ballLoc;

                Vector2D horizontalToPrepVec = (PrepWithBallSpeedIntersect.Value - goalieYLineWithBallSpeedIntersect.Value);
                horizontalToPrepVec.NormalizeTo(horizontalToPrepVec.Size * 0.2);
                if ((goalieYLineWithBallSpeedIntersect.Value + horizontalToPrepVec).X < robotLoc.X)
                {
                    double tBall = PrepWithBallSpeedIntersect.Value.DistanceFrom(ballLoc) / Model.BallState.Speed.Size;
                    double tRobot = CalculateTime(Model.OurRobots[RobotID], PrepWithBallSpeedIntersect.Value, 2.2, 3.3);

                    if (tRobot <= tBall)
                    {

                        posToGo = goalieYLineWithBallSpeedIntersect.Value + horizontalToPrepVec;
                    }
                    else
                        posToGo = goalieYLineWithBallSpeedIntersect.Value;

                }
                else
                {
                    posToGo = goalieYLineWithBallSpeedIntersect.Value;
                }
                //11/25/2017 added vahid
                if (!Position2D.IsBetween(GameParameters.OurGoalRight, GameParameters.OurGoalLeft, posToGo.Value))
                {

                    Position2D dangerZoneLineCenter = new Position2D(GameParameters.OurGoalCenter.X - 1, 0);
                    Line RightLine = new Line(GameParameters.OurGoalRight.Extend(-0.05, -0.13), dangerZoneLineCenter);
                    Line LeftLine = new Line(GameParameters.OurGoalLeft.Extend(-0.05, 0.13), dangerZoneLineCenter);
                    if (PrepWithBallSpeedIntersect.HasValue && PrepWithBallSpeedIntersect.Value.X > GameParameters.OurGoalCenter.X - 0.80 && PrepWithBallSpeedIntersect.Value.X < GameParameters.OurGoalCenter.X - 0.12 &&
                        PrepWithBallSpeedIntersect.Value.Y > -.85 && PrepWithBallSpeedIntersect.Value.Y < .85)
                    {
                        posToGo = PrepWithBallSpeedIntersect.Value;
                    }//todo
                    else
                    {
                        if (Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), Model.BallState.Speed) / Model.BallState.Speed.Size >= 0 &&
                        Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), Model.BallState.Speed) / Model.BallState.Speed.Size <= 1)
                        {
                            posToGo = ballSpeedLine.IntersectWithLine(RightLine);
                            DrawingObjects.AddObject(RightLine);
                        }
                        else
                        {
                            posToGo = ballSpeedLine.IntersectWithLine(LeftLine);
                            DrawingObjects.AddObject(LeftLine);
                        }
                    }

                }
                DrawingObjects.AddObject(new Circle(posToGo.Value, .11, new System.Drawing.Pen(Brushes.White, .02f)));
                //if (posToGo.HasValue && Vector2D.AngleBetweenInDegrees((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) > 45 ||
                //    Vector2D.AngleBetweenInDegrees((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) < 135)
                //{
                //    Position2D tempPos = PrepWithBallSpeedIntersect.Value;
                //    while (tempPos.X > 4.5 - 0.20)
                //    {
                //        tempPos += (goalieYLineWithBallSpeedIntersect.Value - PrepWithBallSpeedIntersect.Value).GetNormalizeToCopy(0.05);
                //    }
                //    posToGo = tempPos;
                //    //posToGo = 
                //    DrawingObjects.AddObject(new Circle(posToGo.Value, 0.12, new Pen(Color.White, 0.01f)));
                //}
                Position2D posToGoExtended = posToGo.Value + (posToGo.Value - robotLoc).GetNormalizeToCopy(10);

                double a = 6;
                if (posToGo.Value.DistanceFrom(robotLoc) < .7)
                {
                    double timeR = CalculateTime(Model.OurRobots[RobotID], posToGo.Value, 2.2, 3.3);
                    double timeB = (posToGo.Value.DistanceFrom(ballLoc) / Model.BallState.Speed.Size) * 0.90;
                    if (debug)
                    {
                        DrawingObjects.AddObject(new Circle(posToGoExtended, .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "4654464546645");
                        DrawingObjects.AddObject(new StringDraw("a:" + a.ToString(), new Position2D(4.7, 0)), "6546549844564");
                        DrawingObjects.AddObject(new StringDraw("timeRobot:" + (timeR * 60).ToString(), new Position2D(4.8, 0)), "65546546464654");
                        DrawingObjects.AddObject(new StringDraw("timeBall:" + (timeB * 60).ToString(), new Position2D(4.9, 0)), "3132131256465456");
                        DrawingObjects.AddObject(new StringDraw("Robot Speed:" + Model.OurRobots[RobotID].Speed.Size.ToString(), new Position2D(5, 0)), "654654646546");
                        DrawingObjects.AddObject(new StringDraw("Ball Speed :" + Model.BallState.Speed.Size.ToString(), new Position2D(5.1, 0)), "312321356465465456");

                    }
                    if (timeR == 0 && Model.OurRobots[RobotID].Speed.Size < .5)
                        a = 0;
                    else if (timeR < timeB)
                    {

                        //a = CalculateAccel(Model.OurRobots[RobotID], posToGo.Value, timeB, 3.3);
                        //if (a < 0)
                        //{
                        //    posToGoExtended = new Position2D(posToGoExtended.X, -posToGoExtended.Y);
                        //    a *= -1;
                        //}
                        Planner.Add(RobotID, posToGo.Value, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                        Planner.AddKick(RobotID, kickPowerType.Speed, true, 3);
                        return;
                    }
                    //11/25/2017 added vahid
                    //if (Model.BallState.Speed.Size < 2.5)
                    //{
                    //    a = 4;
                    //}

                    a = Math.Min(a, 6);

                }
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, a, 5);


                Planner.Add(RobotID, posToGoExtended, Model.OurRobots[RobotID].Angle.Value, false);
                Planner.AddKick(RobotID, kickPowerType.Speed, true, 3);
            }

        }
        public void vandersarDive(GameStrategyEngine Engine, WorldModel Model, int RobotID, ref Position2D pos2Go, ref double angle)
        {
            bool debug = true;
            //double robotToGoalCornersDistTresh = 0.70;

            Position2D ballLoc = Model.BallState.Location, robotLoc = Model.OurRobots[RobotID].Location;
            Line ballSpeedLine = new Line(Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(1), ballLoc);
            Position2D? intWithGoalLine = ballSpeedLine.IntersectWithLine(new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight));
            Position2D? posToGo = null;

            if (intWithGoalLine.HasValue)
            {
                if (debug)
                {
                    //DrawingObjects.AddObject(new StringDraw("L to R innerProduct =" + (Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), Model.BallState.Speed) / Model.BallState.Speed.Size).ToString(), new Position2D(1, 1)));
                    //DrawingObjects.AddObject(new StringDraw(Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree().ToString(), new Position2D(1.2, 1)));
                    //DrawingObjects.AddObject(new StringDraw("R to L innerProduct =" + (Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), Model.BallState.Speed) / Model.BallState.Speed.Size).ToString(), new Position2D(1.3, 1)));
                    //DrawingObjects.AddObject(new StringDraw(Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree().ToString(), new Position2D(1.4, 1)));
                }
                if (robotLoc.Y > intWithGoalLine.Value.Y &&
                    intWithGoalLine.Value.Y < GameParameters.OurGoalRight.Y)
                {
                    //11/25/2017 added vahid
                    Position2D? target = null;
                    if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), -Model.BallState.Speed) / ( Model.BallState.Speed.Size * GameParameters.GoalWidth)).ToDegree() <= 145)
                    {
                        target = GameParameters.OurGoalRight.Extend(-0.12, 0.02);
                    }
                    //else if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalRight - GameParameters.OurGoalLeft), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree() > 145)
                    //{
                    //    target = GameParameters.OurGoalRight.Extend(-0.12, -0.07);
                    //}
                    if (target.HasValue)
                    {
                        DrawingObjects.AddObject(new Circle(target.Value, .13, new System.Drawing.Pen(Brushes.HotPink, .02f)));
                        Planner.Add(RobotID, target.Value, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                        pos2Go = target.Value;
                        angle = (Model.BallState.Location - robotLoc).AngleInDegrees;
                        return;
                    }
                }
                else if (robotLoc.Y < intWithGoalLine.Value.Y &&
                    intWithGoalLine.Value.Y > GameParameters.OurGoalLeft.Y)
                {
                    //11/25/2017 added vahid
                    Position2D? target = null;
                    if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) / (Model.BallState.Speed.Size).ToDegree() * GameParameters.GoalWidth) <= 145)
                    {
                        target = GameParameters.OurGoalLeft.Extend(-0.12, -0.02);
                    }
                    //else if (Math.Acos(Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) / Model.BallState.Speed.Size).ToDegree() > 145)
                    //{
                    //    target = GameParameters.OurGoalLeft.Extend(-0.10, -0.05);
                    //}
                    if (target.HasValue)
                    {
                        DrawingObjects.AddObject(new Circle(target.Value, .20, new System.Drawing.Pen(Brushes.HotPink, .02f)));
                        Planner.Add(RobotID, target.Value, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                        pos2Go = target.Value;
                        angle = (Model.BallState.Location - robotLoc).AngleInDegrees;
                        return;
                    }
                }
                Position2D? goalieYLineWithBallSpeedIntersect = ballSpeedLine.IntersectWithLine(new Line(robotLoc, robotLoc.Extend(0, 1)));
                Position2D? PrepWithBallSpeedIntersect = Model.BallState.Speed.PrependecularPoint(ballLoc, robotLoc);

                if (!goalieYLineWithBallSpeedIntersect.HasValue)
                    goalieYLineWithBallSpeedIntersect = ballLoc;

                Vector2D horizontalToPrepVec = (PrepWithBallSpeedIntersect.Value - goalieYLineWithBallSpeedIntersect.Value);
                horizontalToPrepVec.NormalizeTo(horizontalToPrepVec.Size * 0.2);
                if ((goalieYLineWithBallSpeedIntersect.Value + horizontalToPrepVec).X < robotLoc.X)
                {
                    double tBall = PrepWithBallSpeedIntersect.Value.DistanceFrom(ballLoc) / Model.BallState.Speed.Size ;
                    double tRobot = CalculateTime(Model.OurRobots[RobotID], PrepWithBallSpeedIntersect.Value, 2.2, 3.3);

                    if (tRobot <= tBall)
                    {

                        posToGo = goalieYLineWithBallSpeedIntersect.Value + horizontalToPrepVec;
                    }
                    else
                        posToGo = goalieYLineWithBallSpeedIntersect.Value;

                }
                else
                {
                    posToGo = goalieYLineWithBallSpeedIntersect.Value;
                }
                //11/25/2017 added vahid
                if (!Position2D.IsBetween(GameParameters.OurGoalRight, GameParameters.OurGoalLeft, posToGo.Value))
                {

                    Position2D dangerZoneLineCenter = new Position2D(GameParameters.OurGoalCenter.X - GameParameters.DefenceAreaHeight, 0);
                    Line RightLine = new Line(GameParameters.OurGoalRight.Extend(-0.05, -0.13), dangerZoneLineCenter);
                    Line LeftLine = new Line(GameParameters.OurGoalLeft.Extend(-0.05, 0.13), dangerZoneLineCenter);
                    if (PrepWithBallSpeedIntersect.HasValue && PrepWithBallSpeedIntersect.Value.X > GameParameters.OurGoalCenter.X - 0.80 && PrepWithBallSpeedIntersect.Value.X < GameParameters.OurGoalCenter.X - 0.12 &&
                        PrepWithBallSpeedIntersect.Value.Y > -.85 && PrepWithBallSpeedIntersect.Value.Y < .85)
                    {
                        posToGo = PrepWithBallSpeedIntersect.Value;
                    }//todo
                    else
                    {
                        if (Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), Model.BallState.Speed) / (Model.BallState.Speed.Size * GameParameters.GoalWidth) >= 0 &&
                        Vector2D.InnerProduct((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), Model.BallState.Speed) / (Model.BallState.Speed.Size * GameParameters.GoalWidth) <= 1)
                        {
                            posToGo = ballSpeedLine.IntersectWithLine(RightLine);
                            DrawingObjects.AddObject(RightLine);
                        }
                        else
                        {
                            posToGo = ballSpeedLine.IntersectWithLine(LeftLine);
                            DrawingObjects.AddObject(LeftLine);
                        }
                    }

                }
                DrawingObjects.AddObject(new Circle(posToGo.Value, .11, new System.Drawing.Pen(Brushes.White, .02f)));
                //if (posToGo.HasValue && Vector2D.AngleBetweenInDegrees((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) > 45 ||
                //    Vector2D.AngleBetweenInDegrees((GameParameters.OurGoalLeft - GameParameters.OurGoalRight), -Model.BallState.Speed) < 135)
                //{
                //    Position2D tempPos = PrepWithBallSpeedIntersect.Value;
                //    while (tempPos.X > 4.5 - 0.20)
                //    {
                //        tempPos += (goalieYLineWithBallSpeedIntersect.Value - PrepWithBallSpeedIntersect.Value).GetNormalizeToCopy(0.05);
                //    }
                //    posToGo = tempPos;
                //    //posToGo = 
                //    DrawingObjects.AddObject(new Circle(posToGo.Value, 0.12, new Pen(Color.White, 0.01f)));
                //}
                Position2D posToGoExtended = posToGo.Value + (posToGo.Value - robotLoc).GetNormalizeToCopy(10);

                double a = 6;
                if (posToGo.Value.DistanceFrom(robotLoc) < .7)
                {
                    double timeR = CalculateTime(Model.OurRobots[RobotID], posToGo.Value, 2.2, 3.3);
                    double timeB = (posToGo.Value.DistanceFrom(ballLoc) / Model.BallState.Speed.Size) * 0.90;
                    if (debug)
                    {
                        DrawingObjects.AddObject(new Circle(posToGoExtended, .25, new System.Drawing.Pen(Brushes.HotPink, .02f)), "4654464546645");
                        DrawingObjects.AddObject(new StringDraw("a:" + a.ToString(), new Position2D(4.7, 0)), "6546549844564");
                        DrawingObjects.AddObject(new StringDraw("timeRobot:" + (timeR * 60).ToString(), new Position2D(4.8, 0)), "65546546464654");
                        DrawingObjects.AddObject(new StringDraw("timeBall:" + (timeB * 60).ToString(), new Position2D(4.9, 0)), "3132131256465456");
                        DrawingObjects.AddObject(new StringDraw("Robot Speed:" + Model.OurRobots[RobotID].Speed.Size.ToString(), new Position2D(5, 0)), "654654646546");
                        DrawingObjects.AddObject(new StringDraw("Ball Speed :" + Model.BallState.Speed.Size.ToString(), new Position2D(5.1, 0)), "312321356465465456");

                    }
                    if (timeR == 0 && Model.OurRobots[RobotID].Speed.Size < .5)
                        a = 0;
                    else if (timeR < timeB)
                    {

                        //a = CalculateAccel(Model.OurRobots[RobotID], posToGo.Value, timeB, 3.3);
                        //if (a < 0)
                        //{
                        //    posToGoExtended = new Position2D(posToGoExtended.X, -posToGoExtended.Y);
                        //    a *= -1;
                        //}
                        Planner.Add(RobotID, posToGo.Value, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                        Planner.AddKick(RobotID, kickPowerType.Speed, true, 3);
                        pos2Go = posToGo.Value;
                        angle = (Model.BallState.Location - robotLoc).AngleInDegrees;
                        return;
                    }
                    //11/25/2017 added vahid
                    //if (Model.BallState.Speed.Size < 2.5)
                    //{
                    //    a = 4;
                    //}

                    a = Math.Min(a, 6);

                }
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, a, 5);


                Planner.Add(RobotID, posToGoExtended, Model.OurRobots[RobotID].Angle.Value, false);
                pos2Go = posToGoExtended;
                angle = Model.OurRobots[RobotID].Angle.Value;
                Planner.AddKick(RobotID, kickPowerType.Speed, true, 3);
                return;
            }

        }
        private double CalculateTime(SingleObjectState RobotState, Position2D Target, double maxAccel, double maxSpeed)
        {
            Vector2D vec = Target - RobotState.Location;
            double d = vec.Size;
            double angle = Vector2D.AngleBetweenInRadians(vec, RobotState.Speed);
            double SpeedInRefrence = RobotState.Speed.Size * Math.Cos(angle);
            double time = 0;

            double deccelDX = Math.Min(1, .5 * RobotState.Location.DistanceFrom(Target));
            double daccel = Math.Min(1, .5 * RobotState.Location.DistanceFrom(Target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);

            double rootp = root(2.2, Math.Abs(RobotState.Speed.Size), deccelDX);
            if (rootp > 0)
            {
                time = rootp * 2;
                return time;
            }
            //if (SpeedInRefrence < 0)
            //{
            //    time += -SpeedInRefrence / maxAccel;
            //    d += SpeedInRefrence * SpeedInRefrence / (2 * maxAccel);
            //    SpeedInRefrence = 0;
            //}
            //if (d > 0.06)
            //{
            //    double dvmax = (maxSpeed * maxSpeed - SpeedInRefrence * SpeedInRefrence) / (2 * maxAccel);
            //    if (dvmax > d)
            //    {
            //        double vf = Math.Sqrt(SpeedInRefrence * SpeedInRefrence + 2 * maxAccel * d);
            //        time += (vf - SpeedInRefrence) / maxAccel;
            //        d = 0;
            //    }
            //    else
            //    {
            //        time += (maxSpeed - SpeedInRefrence) / maxAccel;
            //        time += (d - dvmax) / maxSpeed;
            //    }
            //    return time;
            //}
            return 0;
        }
        private double CalculateAccel(SingleObjectState RobotState, Position2D Target, double timeB, double maxSpeed)
        {
            double accel = 0;
            Vector2D vec = Target - RobotState.Location;
            double d = vec.Size;
            double angle = Vector2D.AngleBetweenInRadians(vec, RobotState.Speed);
            double v0 = RobotState.Speed.Size * Math.Cos(angle);
            double vf = 2 * d / timeB - v0;
            if (vf <= maxSpeed)
                accel = (vf - v0) / timeB;
            else if (Math.Abs(maxSpeed * timeB - d) > 0.0001)
                accel = (maxSpeed - v0) * (maxSpeed - v0) / (2 * (maxSpeed * timeB - d));
            else
                accel = 0;
            return accel;
        }

        public static double root(double a, double initialV, double deltaX)
        {
            double t = 0;
            double delta = (initialV * initialV) - (2 * a * -deltaX);
            if (delta == 0)
            {
                t = -initialV / (.5 * a);
            }
            if (delta > 0)
            {

                double t1 = (-initialV - Math.Sqrt(delta)) / a;
                double t2 = (-initialV + Math.Sqrt(delta)) / a;
                if (t2 > 0 && t1 < 0)
                    t = t2;
                else if (t1 > 0 && t2 < 0)
                    t = t1;
                else if (t1 > 0 && t2 > 0)
                    if (t1 < t2)
                        t = t1;
                    else
                        t = t2;
            }
            if (delta < 0)
                return -1000;
            return t;
        }
    }

}
