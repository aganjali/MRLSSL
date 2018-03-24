using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole;
namespace MRL.SSL.AIConsole.Skills
{
    class PreDefinedPath
    {
        double Vmax = 4, Amax = 4;
        Position2D initPos = new Position2D(-1, 1);
        //bool flag = true;
        SingleWirelessCommand SWC = new SingleWirelessCommand();
        double Vx, Vy, Ax, Ay, dist,TheoricalVx;
        Controller controller = new Controller();
        int robotID = 0;
        double finalAx, finalVx, timeX, timeY;
        public void run(WorldModel model)
        {
            double robotX = model.OurRobots[robotID].Location.X;
            double robotY = model.OurRobots[robotID].Location.Y;
            //if (model.OurRobots[robotID].Location.DistanceFrom(new Position2D(-1, 1)) > 0.05 && flag)
            //{
            //    Planner.Add(robotID, new Position2D(-1, 1), 270);
            //    if (model.OurRobots[robotID].Location.DistanceFrom(new Position2D(-1, 1)) < 0.05)
            //    {
            //        flag = false;
            //    }

            //}
            //else
            //{

            //}

            //
            //controller.compute_motion_1d(0);
            double trajAccel = 0, trajTime = 0;

            TheoricalVx = calculateVx(Vmax, robotX);
            Ax = calculateAx(Vx, robotX);
            Ay = calculateAy(Ax,robotX,Vx);
            double x1 = 0.108;
            dist = getParabolicLenght(robotX, x1);
            controller.compute_motion_1d(1, 1, dist, Math.Sqrt(Math.Pow(Vy, 2) + Math.Pow(Vx, 2)) , 0, Amax, Vmax, 1, ref trajAccel, ref trajTime);
            int accelerationSign = Math.Sign(trajAccel) * -1;
            if (true)//Math.Sqrt(Math.Pow(Vy,2) + Math.Pow(Vx,2)) < Vmax )
            {
                Vx = Vx + (Ax / 60);
                Vy = Vy + (Ay / 60); 
            }
            
            Vector2D inFieldRefrence = new Vector2D(Vx, Vy);
            Vector2D robotRefrence = Vector2D.FromAngleSize(model.OurRobots[robotID].Angle.Value * Math.PI / 180, 1);
            inFieldRefrence = GameParameters.InRefrence(inFieldRefrence, robotRefrence);

            
            DrawingObjects.AddObject(new StringDraw("Vx = " + Vx.ToString(), new Position2D(robotX + 0.10, robotY)));
            DrawingObjects.AddObject(new StringDraw("Vy = " + Vy.ToString(), new Position2D(robotX + 0.20, robotY)));
            DrawingObjects.AddObject(new StringDraw("sqrt(Vy ^ 2 + Vx ^ 2) = " + (Math.Sqrt((Math.Pow(Vy, 2) + Math.Pow(Vx, 2)))).ToString(), new Position2D(robotX + 0.30, robotY)));
            DrawingObjects.AddObject(new StringDraw("accelerationSign " + accelerationSign, new Position2D(robotX + 0.40, robotY)));
            SWC.Vy = inFieldRefrence.Y;
            SWC.Vx = inFieldRefrence.X;
            SWC.W = 0;

            Planner.Add(robotID, SWC);
        }
        public double calculateVy(double Vx, double x)
        {
            return 2 * x * Vx;
        }
        public double calculateVx(double Vmax, double x)
        {
            return Math.Sqrt((Math.Pow(Vmax, 2) / ((4 * Math.Pow(x, 2)) + 1)));
        }
        public double calculateAx(double Vx, double x)
        {
            return SolveQuadratic((4 * Math.Pow(x, 2)) + 1, 8 * x * Math.Pow(Vx, 2), 4 * Math.Pow(Vx, 4) - 16).First();
        }
        public double calculateAy(double Ax, double x, double Vx)
        {
            double AY;
            AY = 2 * Math.Pow(Vx, 2) + 2 * x * Ax;
            return AY;
        }
        public double getParabolicLenght(double x1, double x2)
        {
            x1 = Math.Abs(x1);
            x2 = Math.Abs(x2);
            double firstHalf = x1 * Math.Sqrt(1 + Math.Pow(x1, 2)) + 0.25 * Math.Log(2 * x1 + Math.Sqrt(1 + 4 * Math.Pow(x1, 2)));
            double secondHalf = x2 * Math.Sqrt(1 + Math.Pow(x2, 2)) + 0.25 * Math.Log(2 * x2 + Math.Sqrt(1 + 4 * Math.Pow(x2, 2)));

            return firstHalf + secondHalf;
        }

        private List<double> SolveQuadratic(double a, double b, double c)
        {

            double sqrtpart = (b * b) - (4 * a * c);

            double x, x1, x2, img;

            if (sqrtpart > 0)
            {

                x1 = (-b + System.Math.Sqrt(sqrtpart)) / (2 * a);

                x2 = (-b - System.Math.Sqrt(sqrtpart)) / (2 * a);

                return new List<double> { x1, x2 };

            }

            else if (sqrtpart < 0)
            {

                sqrtpart = -sqrtpart;

                x = -b / (2 * a);

                img = System.Math.Sqrt(sqrtpart) / (2 * a);
                return new List<double> { };
            }

            else
            {

                x = (-b + System.Math.Sqrt(sqrtpart)) / (2 * a);
                return new List<double> { x };
            }

        }
        double Trajectory_1D(double deltaS, double startVel, double finalVel, double aMax, double vMax, ref double time)
        {

            double SamplingTime = 1.0 / (double)StaticVariables.FRAME_RATE;
            if (deltaS == 0 && startVel == finalVel)
            {
                time = 0;
                return 0;
            }
            double timeToFinalVel = Math.Abs(startVel - finalVel) / aMax;
            // - ya + finalVel - startVel
            double distanceToFinalVel = Math.Abs(finalVel - startVel) / 2 * timeToFinalVel;
            double timeTemp, timeAcc, timeDec;
            if (Math.Abs(startVel) > Math.Abs(finalVel))
            {
                timeTemp = (Math.Sqrt((startVel * startVel + finalVel * finalVel) / 2 + Math.Abs(deltaS) * aMax) - Math.Abs(startVel)) / aMax;

                if (timeTemp < 0)
                    timeTemp = 0;
                timeAcc = timeTemp;
                timeDec = timeTemp + timeToFinalVel;
            }
            else if (distanceToFinalVel > Math.Abs(deltaS))
            {
                timeTemp = (Math.Sqrt(startVel * startVel + 2 * aMax * Math.Abs(deltaS)) - Math.Abs(startVel)) / aMax;
                timeAcc = timeTemp;
                timeDec = 0;
            }
            else
            {
                timeTemp = (Math.Sqrt((startVel * startVel + finalVel * finalVel) / 2 + Math.Abs(deltaS) * aMax) - Math.Abs(finalVel)) / aMax;
                if (timeTemp < 0)
                    timeTemp = 0;
                timeAcc = timeTemp + timeToFinalVel;
                timeDec = timeTemp;
            }

            double trajTime = timeAcc + timeDec;
            if (timeAcc * aMax + Math.Abs(startVel) > vMax)
                trajTime += (vMax - (aMax * timeAcc + Math.Abs(startVel))) * (vMax - (aMax * timeAcc + Math.Abs(startVel))) / (aMax * vMax);

            double trajAcc = 0;
            if (timeAcc < SamplingTime && timeDec < 0.1)
                trajAcc = (finalVel - startVel) * (SamplingTime);
            else if (timeAcc < SamplingTime && timeDec > 0)
                trajAcc = Math.Sign(finalVel - startVel) * (aMax * timeAcc + (double)StaticVariables.FRAME_RATE * (aMax * (SamplingTime - timeAcc))); /* trajAcc = (aMax * timeAcc) + (-aMax * (SamplingTime - timeAcc))*/
            else
                trajAcc = aMax * Math.Sign(deltaS);

            time = trajTime;
            return trajAcc;

        }
    }
}
