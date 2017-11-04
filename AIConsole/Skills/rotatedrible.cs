using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class rotatedrible : SkillBase
    {
        SingleWirelessCommand SWC = new SingleWirelessCommand();
        public double OmegaCoef = 0;
        public double VyCoef = 0;
        public double ClockWise = -1;
        public double RotateTeta = 90;
        public double FakeRotateTeta = 90;
        public double RealRotateTeta = 90;
        public double gotoPointTresh = 0.01;
        public double gotoPointExtendSize = 0.13;
        public double gotoPointTeta = 0;
        public double RotateRadius = 0.155;
        public double VarAngle2 = 0;
        public double GotoPointDelay = 60;
        static double FramePeriod = 0.016;
        static double Vmax = 2.5;
        static double a = 3;
        static double ax = a / 70;
        static double ay = a * 90;
        static double Epsilon = 1E-2;
        static double Epsilon2 = 7;
        private double MaxI = 5000;


        private double MaxSize = 3.5;
        private double Variance = 0;
        private double radius;
        private double p;
        private double middleFrame = 0;
        private double IErr = 0, LastE = 0;
        private double IErrX = 0, LastEX = 0;
        private bool BallAvoided = false;
        private bool FirstTime = true;
        private bool flag = false;
        private bool intoe = true;
        private bool fake = true;
        private bool Savedata = false;
        private bool getdata = false;
        private bool openLoop = false;
        public int GotoPointCounter = 0;
        public int RotateCounter = 0;
        private int Frame = 0;
        private int counter = 0;
        private int C_counterPos = 0;
        private int co = 0;
        private int CounterErr = 0;
        private int TotalFrameCount = 0;
        private Position2D firstballdtate;
        public Position2D GotoPointTarget = new Position2D();
        private List<double> Angles = new List<double>();
        private int CounterErr2 = 0;

        public SingleWirelessCommand rotateWFeedbackBig(WorldModel Model, Position2D Target, double Teta, kickPowerType kicktype, double Kickspeed, int RobotID, bool IsChipKick, bool backSensor = false)
        {
            VyCoef = 320;
            OmegaCoef = 0.83;
            a = 2.5;
            ax = a / 64;
            ay = a * 100;
            Vmax = 3;
            RotateRadius = 0.60;
            RotateCounter++;

            double TetaInRadian = ClockWise * Teta * (Math.PI / 180);
            if (Teta < Epsilon)
            {
                SWC.Vx = 0;
                SWC.Vy = 0;
                SWC.W = 0;

                SWC.isChipKick = IsChipKick;
                SWC.BackSensor = backSensor;
                if (kicktype == kickPowerType.Power)
                    SWC.KickPower = Kickspeed;
                else
                    SWC.KickSpeed = Kickspeed;

                if (RotateCounter < 30)
                    SWC.Vy = 1;
            }
            #region Trapazoidal
            else if ((RotateRadius * (TetaInRadian) > (Vmax * Vmax / a)))//Trapezoidal
            {
                double t1 = Vmax / a;
                double t2 = (((RotateRadius * TetaInRadian) - ((Vmax * Vmax) / a)) / Vmax) + (Vmax / a);
                double t = t1 + t2;
                int t1FrameCount = (int)Math.Floor((t1 / (FramePeriod)));
                int t2FrameCount = (int)Math.Floor(t2 / (FramePeriod));
                int tFrameCount = (int)Math.Floor(t / (FramePeriod));
                double Omega = 0;
                if (RotateCounter <= t1FrameCount)
                {
                    SWC.Vx += ClockWise * ax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);
                }
                else if (RotateCounter <= t2FrameCount)
                {
                    SWC.Vx = ClockWise * Vmax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);
                }
                else if (RotateCounter <= tFrameCount)
                {
                    SWC.Vx -= ClockWise * ax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);
                }
                else
                {
                    SWC.Vx = 0;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;

                    SWC.isChipKick = IsChipKick;
                    SWC.BackSensor = backSensor;
                    if (kicktype == kickPowerType.Power)
                        SWC.KickPower = Kickspeed;
                    else
                        SWC.KickSpeed = Kickspeed;
                }
            }
            #endregion
            #region Triangle

            else if ((RotateRadius * TetaInRadian) <= (Vmax * Vmax / a))//Triangle
            {
                ///////////
                //For FeedBack
                double t1 = Math.Sqrt((ClockWise) * (RotateRadius * TetaInRadian) / a);
                int t1FrameCount = (int)Math.Round((t1 / (FramePeriod)));
                int t2FrameCount = 2 * t1FrameCount;

                int FeedBackFrame = t1FrameCount + (t1FrameCount / 2);
                ///////////
                double Omega = 0;
                int FeedBackFrame1 = t1FrameCount / 2;
                int FeedBackFrame2 = t1FrameCount + FeedBackFrame1;

                if (RotateCounter == FeedBackFrame1)
                {
                    Vector2D Targettoball = Target - Model.BallState.Location;
                    double robotangle = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                    Vector2D FromRobotAngle = Vector2D.FromAngleSize(robotangle, 1);
                    double VarAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(Targettoball, FromRobotAngle));
                    DrawingObjects.AddObject(new StringDraw(VarAngle.ToString(), Color.Red, new Position2D(2.8, 0)));
                    Angles.Add(VarAngle);
                }
                if (RotateCounter == FeedBackFrame2 && counter == 0)
                {
                    counter++;
                    Vector2D Targettoball = Target - Model.BallState.Location;
                    double robotangle = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                    Vector2D FromRobotAngle = Vector2D.FromAngleSize(robotangle, 1);
                    double VarAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(Targettoball, FromRobotAngle));
                    DrawingObjects.AddObject(new StringDraw(VarAngle.ToString(), Color.GreenYellow, new Position2D(2.8, 0)));
                    Angles.Add(VarAngle);
                    double AngleC = Math.Abs(Teta - Angles[1]);
                    if (AngleC > Angles[0])
                    {
                        double Variation = Math.Abs(AngleC - Angles[0]);
                        Frame = ((int)Variation / 3);
                    }
                    else if (AngleC < Angles[0] && intoe)
                    {
                        double Variation = Math.Abs(AngleC - Angles[0]);
                        Frame = ((int)Variation / 3);
                        VyCoef += Frame * 40;
                        OmegaCoef += Frame * .023;
                        intoe = false;
                    }
                    //    VyCoef += (5*(int)Variation );
                    //    if (Model.BallState.Location.Y < 0)
                    //    {
                    //        VyCoef += (.06*(int)Variation);
                    //    }
                    //    //OmegaCoef += (Frame / 15)+.1;

                    //    OmegaCoef += (Teta) * .0034*(Frame);
                    //}
                    //if (Model.BallState.Location.Y < 0)
                    //{

                    //    OmegaCoef -= (.0058 * OmegaCoef);
                    //}

                    DrawingObjects.AddObject(new StringDraw(Frame.ToString(), Color.Blue, new Position2D(2.9, 0)));
                }
                Vector2D Targettoball1 = Target - Model.BallState.Location;
                double robotangle1 = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                Vector2D FromRobotAngle1 = Vector2D.FromAngleSize(robotangle1, 1);
                double VarAngle1 = Math.Abs(Vector2D.AngleBetweenInDegrees(Targettoball1, FromRobotAngle1));
                DrawingObjects.AddObject(new StringDraw(VarAngle1.ToString(), Color.Blue, new Position2D(2.7, 0)));
                DrawingObjects.AddObject(new Line(Target, Model.BallState.Location, new Pen(Brushes.Red, .01f)));
                DrawingObjects.AddObject(Model.OurRobots[RobotID].Angle);
                DrawingObjects.AddObject(new StringDraw(RotateCounter.ToString(), new Position2D(0, 2)));
                if (RotateCounter <= t1FrameCount + 1)
                {
                    if (RotateCounter == 1)
                    {
                        firstballdtate = Model.BallState.Location;
                        radius = Target.DistanceFrom(firstballdtate);
                    }
                    getdata = true;
                    SWC.Vx += ClockWise * ax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);
                    ///For FeedBack
                    Vector2D Targettoball = Target - Model.BallState.Location;
                    double robotangle = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                    Vector2D RobotAngle = new Vector2D();
                    RobotAngle.NormalizeTo(1);
                    Vector2D FromRobotAngle = RobotAngle.GetRotate(robotangle);
                    DrawingObjects.AddObject(new StringDraw(RotateCounter.ToString(), new Position2D(2.8, 0)));
                }
                else if (RotateCounter <= t2FrameCount + Frame + 1)
                {
                    SWC.Vx -= ClockWise * ax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);
                    DrawingObjects.AddObject(new StringDraw(RotateCounter.ToString(), new Position2D(2.8, 0)));

                }
                else /*if (RotateCounter > FrameNumT_2 + 1 && FrameCounter < FrameNumT_2 + 8)*/
                {
                    SWC.Vx = 0;
                    SWC.Vy = 1;
                    //  SWC.Vy = 0;
                    //Omega = ClockWise * (SWC.Vx / RotateRadius);
                    //SWC.W = Omega * OmegaCoef;
                    SWC.W = 0;
                    if (RotateCounter > t2FrameCount + Frame + 20)
                    {
                        Vector2D firstlast = Model.BallState.Location - firstballdtate;
                        Vector2D targetvec = Target - firstballdtate;
                        double angle = Vector2D.AngleBetweenInDegrees(firstlast, targetvec);
                        p = ((angle * Math.PI) / 180) * radius;
                        Savedata = true;
                    }
                    SWC.isChipKick = IsChipKick;
                    SWC.BackSensor = backSensor;
                    if (kicktype == kickPowerType.Power)
                        SWC.KickPower = Kickspeed;
                    else
                        SWC.KickSpeed = Kickspeed;

                    getdata = false;
                }
                ///FeedBack
                ///in variation of Frames


            #endregion

            }
            DrawingObjects.AddObject(new StringDraw("Vx" + SWC.Vx.ToString(), new Position2D(Model.OurRobots[RobotID].Location.X + .3, Model.OurRobots[RobotID].Location.Y)));
            DrawingObjects.AddObject(new StringDraw("Vy" + SWC.Vy.ToString(), new Position2D(Model.OurRobots[RobotID].Location.X + .2, Model.OurRobots[RobotID].Location.Y)));
            DrawingObjects.AddObject(new StringDraw("W" + SWC.W.ToString(), new Position2D(Model.OurRobots[RobotID].Location.X + .1, Model.OurRobots[RobotID].Location.Y)));


            if (Teta == 0)
            {
                SWC.Vy = .05;
                SWC.SpinBack = 0;
                SWC.Vx = 0;
                SWC.W = 0;
            }
            return SWC;
        }

    }
}
