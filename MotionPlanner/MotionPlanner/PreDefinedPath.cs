//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.GameDefinitions;
//using MRL.SSL.Planning.MotionPlanner;
//using MathNet.Numerics;
//namespace MRL.SSL.Planning.MotionPlanner
//{
//    class PreDefinedPath
//    {
//        double Vmax = 0.066667, Amax = 4;
//        Position2D initPos = new Position2D(-1, 1);
//        bool flag = true;
//        SingleWirelessCommand SWC = new SingleWirelessCommand();
//        double Vx = 0, Vy = 0, Ax, Ay, dist, TheoricalVx, TheoricalVy, finalAy, finalAx;
//        Controller controller = new Controller();
//        bool inEndPhaseX = false, inEndPhaseY = false;
//        double TunningDistance = 0.08;
//        AdaptiveTunner aTunner = new AdaptiveTunner();

//        double[] X = new double[3];
//        double[] Y = new double[3];
//        double[] P = new double[3];
//        Position2D zeroPos, extendPos;
//        Vector2D newRefrence = new Vector2D();

//        public void run(WorldModel model,double robotID , Position2D firstRobotPos, Position2D betweenPos, Position2D destPos)
//        {
//            double robotX = 0, robotY = 0;
//            if (flag)
//                zeroPos = new Position2D((firstRobotPos.X + destPos.X) / 2, (firstRobotPos.Y + destPos.Y) / 2);
//                Position2D extendedPos = zeroPos - (firstRobotPos - destPos).GetPerp().GetNormnalizedCopy();//zeroPos + (new Position2D(X[0], Y[0]) - new Position2D(X[2], Y[2])).GetPerp().GetNormnalizedCopy();
//                newRefrence = -(extendedPos - zeroPos);

//                //robot position to refrence

//                flag = false;
//            }
//            robotX = model.OurRobots[robotID].Location.X;
//            robotY = model.OurRobots[robotID].Location.Y;
//            var robotToRef = GameParameters.InRefrence(new Vector2D(robotX, robotY), newRefrence);

//            robotX = robotToRef.X;
//            robotY = robotToRef.Y;
//            DrawingObjects.AddObject(new Line(zeroPos, zeroPos + newRefrence));
//            DrawingObjects.AddObject(firstRobotPos);
//            DrawingObjects.AddObject(zeroPos);
//            DrawingObjects.AddObject(Position2D.Interpolate(firstRobotPos, destPos, 1));
//            DrawingObjects.AddObject(destPos);
//            //main points to refrence
//            X[0] = GameParameters.InRefrence(new Vector2D(firstRobotPos.X, firstRobotPos.Y), newRefrence).X;
//            Y[0] = GameParameters.InRefrence(new Vector2D(firstRobotPos.X, firstRobotPos.Y), newRefrence).Y;
//            X[1] = GameParameters.InRefrence(new Vector2D(betweenPos.X, betweenPos.Y), newRefrence).X;
//            Y[1] = GameParameters.InRefrence(new Vector2D(betweenPos.X, betweenPos.Y), newRefrence).Y;
//            X[2] = GameParameters.InRefrence(new Vector2D(destPos.X, destPos.Y), newRefrence).X;
//            Y[2] = GameParameters.InRefrence(new Vector2D(destPos.X, destPos.Y), newRefrence).Y;

//            P = Polyfit(X, Y);
//            double trajAccel = 0, trajTime = 0;


//            dist = getParabolicLenght(robotX, X[2]);// +Math.Abs(robotY - (robotX * robotX));

//            controller.compute_motion_1d(1, ControlParameters.Accuercy, -dist, Math.Pow(Math.Sqrt(Math.Pow(Vy, 2) + Math.Pow(Vx, 2)), 2), 0, Amax, Vmax, ControlParameters.aFactor, ref trajAccel, ref trajTime);

//            int accelerationSign = Math.Sign(trajAccel);// *-1;
//            if (Math.Sqrt(TheoricalVy * TheoricalVy + TheoricalVx * TheoricalVx) < 4
//                && Math.Sqrt(TheoricalVy * TheoricalVy + TheoricalVx * TheoricalVx) > 0)
//            {
//                Vmax += 0.0666667 * accelerationSign;
//            }

//            TheoricalVx = calculateVx(Vmax, robotX, P[1], P[2]);
//            TheoricalVy = calculateVy(TheoricalVx, robotX, P[1], P[2]);
//            Vx = TheoricalVx;
//            Vy = TheoricalVy;


//            #region Accuercy
//            Vector2D Vtemp = new Vector2D(Vx, Vy);
//            SingleObjectState stat = new SingleObjectState(model.OurRobots[robotID]);
//            aTunner.UpdateCoefs(stat.Location, stat.Speed, stat.Angle.Value, stat.AngularSpeed.Value, new Position2D(X[2], X[2] * X[2]), 0);
//            double dX = robotX - X[2];
//            double dY = robotY - X[2] * X[2];

//            if (Math.Abs(dX) < 0.001)
//            {
//                dX = 0;

//                //  aTunner.Check4CollisionReset(PIDType.X);
//            }

//            if (Math.Abs(dY) < 0.001)
//            {
//                dY = 0;
//                // aTunner.Check4CollisionReset(PIDType.Y);
//            }
//            if ((dist < TunningDistance))//|| (pathLength < _tunningDistance))
//            {

//                inEndPhaseX = true;
//                //Vtemp.X = xTunner.Tune(dX, stat.Speed.X, 1, RobotID);
//                Vtemp.X = aTunner.Tune(dX, robotID, PIDType.X);
//                double alfamax = 9;
//                double alfa = (Vtemp.X - stat.Speed.X) * StaticVariables.FRAME_RATE;
//                if (Math.Abs(alfa) > alfamax)
//                    Vtemp.X = (stat.Speed.X) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;
//                Vx = Vtemp.X;
//            }
//            else
//            {
//                if (inEndPhaseX)
//                    aTunner.Check4CollisionReset(PIDType.X);

//                inEndPhaseX = false;
//                aTunner.Reset(PIDType.X);
//            }
//            // xTunner.Reset();

//            if ((dist < TunningDistance))// && path.Count <= 2) || (path.Count > 2 && pathLength < _tunningDistance))
//            {
//                inEndPhaseY = true;
//                //Vtemp.Y = yTunner.Tune(dY, stat.Speed.Y, 2, RobotID);
//                Vtemp.Y = aTunner.Tune(dY, robotID, PIDType.Y);
//                double alfamax = 9;
//                double alfa = (Vtemp.Y - stat.Speed.Y) * StaticVariables.FRAME_RATE;
//                if (Math.Abs(alfa) > alfamax)
//                    Vtemp.Y = (stat.Speed.Y) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;
//                Vy = Vtemp.Y;
//            }
//            else
//            {
//                if (inEndPhaseY)
//                    aTunner.Check4CollisionReset(PIDType.Y);
//                inEndPhaseY = false;
//                aTunner.Reset(PIDType.Y);
//            }


//            //yTunner.Reset()
//            //if (RobotID == 4||RobotID == 2)
//            //{
//            //    DrawingObjects.AddObject("TargetCircle" + RobotID, new Circle(Target, 0.09, new Pen(Color.BlueViolet, 0.01f)));
//            //    DrawingObjects.AddObject("errorXControl" + RobotID, new StringDraw(dX.ToString(), Target + new Vector2D(0, 0.2)));
//            //}

//            //lastAngularSpeed = ww;
//            //lastV = Vtemp;

//            //double Rotation = Model.OurRobots[RobotID].Angle.Value;
//            //Rotation *= Math.PI / (double)180;

//            //      Rotation +=  100 * lastAngularSpeed * StaticVariables.FRAME_PERIOD;

//            // lastLastW = lastAngularSpeed;

//            #endregion

//            if (Math.Abs(dist) < 0.01)
//            {
//                Vy = 0;
//                Vx = 0;
//            }
//            #region To Field Axis
//            //Vector2D fieldRefrence = new Vector2D(0,1);
//            Vector2D tempVector = new Vector2D(Vx, Vy);

//            tempVector = GameParameters.InRefrence(tempVector, newRefrence);
//            Vx = tempVector.X;
//            Vy = tempVector.Y;

//            #endregion
//            #region To Robot Axis
//            Vector2D inRobotRefrence = new Vector2D(Vx, Vy);
//            Vector2D robotRefrence = Vector2D.FromAngleSize(model.OurRobots[robotID].Angle.Value * Math.PI / 180, 1);
//            inRobotRefrence = GameParameters.InRefrence(inRobotRefrence, robotRefrence);
//            #endregion

//            DrawingObjects.AddObject(new StringDraw("Vx = " + Vx.ToString(), new Position2D(robotX + 0.10, robotY)));
//            DrawingObjects.AddObject(new StringDraw("Vy = " + Vy.ToString(), new Position2D(robotX + 0.20, robotY)));
//            DrawingObjects.AddObject(new StringDraw("sqrt(Vy ^ 2 + Vx ^ 2) = " + (Math.Sqrt((Math.Pow(Vy, 2) + Math.Pow(Vx, 2)))).ToString(), new Position2D(robotX + 0.30, robotY)));
//            DrawingObjects.AddObject(new StringDraw("accelerationSign " + accelerationSign, new Position2D(robotX + 0.40, robotY)));
//            DrawingObjects.AddObject(new StringDraw("dist " + dist, new Position2D(robotX + 0.50, robotY)));
//            SWC.Vy = inRobotRefrence.Y;
//            SWC.Vx = inRobotRefrence.X;
//            SWC.W = 0;
//            Planner.Add(robotID, SWC);
//        }
//        public double calculateVy(double Vx, double x, double p1, double p2)
//        {
//            return Vx * (p1 + (2 * p2 * x));
//        }
//        public double calculateVx(double Vmax, double x, double p1, double p2)
//        {
//            return Math.Sqrt((Vmax * Vmax / ((Math.Pow(p1 + 2 * p2 * x, 2)) + 1)));
//        }
//        public double calculateAx(double Vx, double V0x)// double x)
//        {
//            //return SolveQuadratic((4 * Math.Pow(x, 2)) + 1, 8 * x * Math.Pow(Vx, 2), 4 * Math.Pow(Vx, 4) - 16).First();
//            return (Vx - V0x) / (0.016666666667);
//        }
//        public double calculateAy(double Ax, double x, double Vx)//double Vy, double V0y)
//        {
//            return 2 * Math.Pow(Vx, 2) + 2 * x * Ax; ;
//            //return (Vy - V0y) / (0.016666666667);

//        }
//        public double getParabolicLenght(double x1, double x2)
//        {
//            int sgn1 = Math.Sign(x1);
//            int sgn2 = Math.Sign(x2);

//            x1 = Math.Abs(x1);
//            x2 = Math.Abs(x2);
//            double firstHalf = x1 * Math.Sqrt(1 + Math.Pow(x1, 2)) + 0.25 * Math.Log(2 * x1 + Math.Sqrt(1 + 4 * Math.Pow(x1, 2)));
//            double secondHalf = x2 * Math.Sqrt(1 + Math.Pow(x2, 2)) + 0.25 * Math.Log(2 * x2 + Math.Sqrt(1 + 4 * Math.Pow(x2, 2)));
//            if (sgn1 != sgn2)
//                return firstHalf + secondHalf;
//            else
//                return secondHalf - firstHalf;


//        }
//        public static double[] Polyfit(double[] x, double[] y)
//        {
//            return Fit.Polynomial(x, y, 2);
//        }
//    }
//}
