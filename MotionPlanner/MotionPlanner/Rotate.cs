using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using System.IO;


namespace MRL.SSL.Planning.MotionPlanner
{
    public class Rotate
    {
        bool oldRotate = true;
        SingleWirelessCommand SWC = new SingleWirelessCommand();
        static public double OmegaCoef = 0;
        static public double VyCoef = 0;
        public double ClockWise = 1;
        public double RotateTeta = 90;
        public double FakeRotateTeta = 90;
        public double RealRotateTeta = 90;
        public double gotoPointTresh = 0.006;
        public double gotoPointExtendSize = 0.13;
        public double gotoPointTeta = 0;
        public double RotateRadius = .155;//0.155;
        public double VarAngle2 = 0;
        public double GotoPointDelay = 60;
        static double FramePeriod = 0.016;
        static double Vmax = 2.5;
        static double a = 3;
        static double ax = a / 70;
        static double ay = a * 90;
        static double Epsilon = 1E-2;
        static double Epsilon2 = 1;
        private double MaxI = 5000;
        private double MaxSize = 3.5;
        private double radius;
        private double p;
        private double IErr = 0, LastE = 0;
        private double IErrX = 0, LastEX = 0;
        private double IErrY = 0, LastEY = 0;
        private bool BallAvoided = false;
        private bool flag = false;
        private bool intoe = true;
        private bool fake = true;
        private bool close = false;
        public int GotoPointCounter = 0;
        public int RotateCounter = 0;
        private int Frame = 0;
        public static int counter1 = 0;
        private int CounterErr = 0;
        private Position2D firstballdtate;
        List<double> AnglesList = new List<double>();
        public Position2D GotoPointTarget = new Position2D();
        bool isInRotateDelay = false;
        bool kickBall = false;
        double AngleGo = 0;
        double CheckFrame = 0;
        double ThreshholdAngleWasWent = 62.5;
        double CorrectAngle = 0;
        double Distance = 0;
        double threshholdCorrectAnglesmall = 0;
        double threshholdCorrectAngleBig = 0;
        double correctBallAngle = 0;
        bool inKickState = false;
        public static int counter = 0;

        public bool InKickState
        {
            get { return inKickState; }
            set { inKickState = value; }
        }
        public bool IsInRotateDelay
        {
            get { return isInRotateDelay; }
        }

        private List<double> Angles = new List<double>();

        private int CounterErr2 = 0;
        private Vector2D OUT = Vector2D.Zero;
        private static readonly Dictionary<int, Dictionary<int, double>> VyCoeff = new Dictionary<int, Dictionary<int, double>>();
        private static readonly Dictionary<int, Dictionary<int, double>> Omega = new Dictionary<int, Dictionary<int, double>>();

        static Rotate()
        {
            #region Robot 0
            VyCoeff[0] = new Dictionary<int, double>();
            Omega[0] = new Dictionary<int, double>();

            VyCoeff[0][30] = 450;
            Omega[0][30] = 1.1;
            VyCoeff[0][40] = 450;
            Omega[0][40] = 1.12;
            VyCoeff[0][50] = 450;
            Omega[0][50] = 1.1;
            VyCoeff[0][60] = 450;
            Omega[0][60] = 1.08;
            VyCoeff[0][70] = 450;
            Omega[0][70] = 1.1;
            VyCoeff[0][80] = 450;
            Omega[0][80] = 1.1;
            VyCoeff[0][90] = 450;
            Omega[0][90] = 1.09;
            VyCoeff[0][100] = 450;
            Omega[0][100] = 1.12;
            VyCoeff[0][110] = 450;
            Omega[0][110] = 1.12;
            VyCoeff[0][120] = 450;
            Omega[0][120] = 1.08;
            VyCoeff[0][180] = 300;
            Omega[0][180] = .99;
            #endregion
            #region Robot 1
            VyCoeff[1] = new Dictionary<int, double>();
            Omega[1] = new Dictionary<int, double>();

            VyCoeff[1][30] = 600;
            Omega[1][30] = 1;
            VyCoeff[1][40] = 500;
            Omega[1][40] = 1.08;
            VyCoeff[1][50] = 500;
            Omega[1][50] = 1.08;
            VyCoeff[1][60] = 500;
            Omega[1][60] = 1.06;
            VyCoeff[1][70] = 400;
            Omega[1][70] = 1.08;
            VyCoeff[1][80] = 400;
            Omega[1][80] = 1.07;
            VyCoeff[1][90] = 400;
            Omega[1][90] = .97;
            VyCoeff[1][100] = 400;
            Omega[1][100] = 1.1;
            VyCoeff[1][110] = 400;
            Omega[1][110] = 1.08;
            VyCoeff[1][120] = 380;
            Omega[1][120] = 1.05;
            VyCoeff[1][180] = 170;
            Omega[1][180] = .96;
            #endregion
            #region Robot 2
            VyCoeff[2] = new Dictionary<int, double>();
            Omega[2] = new Dictionary<int, double>();

            VyCoeff[2][30] = 400;
            Omega[2][30] = 1.08;
            VyCoeff[2][40] = 400;
            Omega[2][40] = 1.08;
            VyCoeff[2][50] = 400;
            Omega[2][50] = 1.08;
            VyCoeff[2][60] = 400;
            Omega[2][60] = 1.05;
            VyCoeff[2][70] = 400;
            Omega[2][70] = 1.08;
            VyCoeff[2][80] = 400;
            Omega[2][80] = 1.1;
            VyCoeff[2][90] = 400;
            Omega[2][90] = 1.03;
            VyCoeff[2][100] = 400;
            Omega[2][100] = 1.08;
            VyCoeff[2][110] = 400;
            Omega[2][110] = 1.08;
            VyCoeff[2][120] = 300;
            Omega[2][120] = 1.06;
            VyCoeff[2][180] = 200;
            Omega[2][180] = .965;
            #endregion
            #region Robot 3
            VyCoeff[3] = new Dictionary<int, double>();
            Omega[3] = new Dictionary<int, double>();

            VyCoeff[3][30] = 600;
            Omega[3][30] = 1.14;
            VyCoeff[3][40] = 600;
            Omega[3][40] = 1.12;
            VyCoeff[3][50] = 600;
            Omega[3][50] = 1.2;
            VyCoeff[3][60] = 600;
            Omega[3][60] = 1;
            VyCoeff[3][70] = 600;
            Omega[3][70] = 1.09;
            VyCoeff[3][80] = 600;
            Omega[3][80] = 1.1;
            VyCoeff[3][90] = 600;
            Omega[3][90] = 1.07;
            VyCoeff[3][100] = 600;
            Omega[3][100] = 1.05;
            VyCoeff[3][110] = 600;
            Omega[3][110] = 1.09;
            VyCoeff[3][120] = 600;
            Omega[3][120] = 1.07;
            VyCoeff[3][180] = 300;
            Omega[3][180] = .93;
            #endregion
            #region Robot 4
            VyCoeff[4] = new Dictionary<int, double>();
            Omega[4] = new Dictionary<int, double>();

            VyCoeff[4][0] = 500;
            Omega[4][0] = 1.1;
            VyCoeff[4][40] = 500;
            Omega[4][40] = 1.1;
            VyCoeff[4][50] = 500;
            Omega[4][50] = 1.19;
            VyCoeff[4][60] = 400;
            Omega[4][60] = 1.01;
            VyCoeff[4][70] = 400;
            Omega[4][70] = 1;
            VyCoeff[4][80] = 400;
            Omega[4][80] = 1.14;
            VyCoeff[4][90] = 600;
            Omega[4][90] = 1.05;
            VyCoeff[4][100] = 500;
            Omega[4][100] = 1.15;
            VyCoeff[4][110] = 300;
            Omega[4][110] = 1.13;
            VyCoeff[4][120] = 280;
            Omega[4][120] = 1.15;
            VyCoeff[4][180] = 400;
            Omega[4][180] = 1;
            #endregion
            #region Robot 5
            VyCoeff[5] = new Dictionary<int, double>();
            Omega[5] = new Dictionary<int, double>();

            VyCoeff[5][30] = 400;
            Omega[5][30] = 1.1;
            VyCoeff[5][40] = 400;
            Omega[5][40] = 1.08;
            VyCoeff[5][50] = 400;
            Omega[5][50] = 1.09;
            VyCoeff[5][60] = 400;
            Omega[5][60] = 1.08;
            VyCoeff[5][70] = 400;
            Omega[5][70] = 1.09;
            VyCoeff[5][80] = 400;
            Omega[5][80] = 1.04;
            VyCoeff[5][90] = 500;
            Omega[5][90] = 1.05;
            VyCoeff[5][100] = 400;
            Omega[5][100] = 1.11;
            VyCoeff[5][110] = 400;
            Omega[5][110] = 1.12;
            VyCoeff[5][120] = 400;
            Omega[5][120] = 1.05;
            VyCoeff[5][180] = 250;
            Omega[5][180] = .99;
            #endregion
            #region Robot 6
            VyCoeff[6] = new Dictionary<int, double>();
            Omega[6] = new Dictionary<int, double>();
            VyCoeff[6][30] = 700;
            Omega[6][30] = 1.11;
            VyCoeff[6][40] = 700;
            Omega[6][40] = 1.09;
            VyCoeff[6][50] = 700;
            Omega[6][50] = 1.1;
            VyCoeff[6][60] = 700;
            Omega[6][60] = 1.1;
            VyCoeff[6][70] = 700;
            Omega[6][70] = 1.1;
            VyCoeff[6][80] = 700;
            Omega[6][80] = 1.1;
            VyCoeff[6][90] = 700;
            Omega[6][90] = 1.11;
            VyCoeff[6][100] = 700;
            Omega[6][100] = 1.1;
            VyCoeff[6][110] = 650;
            Omega[6][110] = 1.1;
            VyCoeff[6][120] = 600;
            Omega[6][120] = 1.1;
            VyCoeff[6][180] = 300;
            Omega[6][180] = .98;
            #endregion
            #region Robot 7
            VyCoeff[7] = new Dictionary<int, double>();
            Omega[7] = new Dictionary<int, double>();
            VyCoeff[7][30] = 500;
            Omega[7][30] = 1.09;
            VyCoeff[7][40] = 500;
            Omega[7][40] = 1.1;
            VyCoeff[7][50] = 500;
            Omega[7][50] = 1.2;
            VyCoeff[7][60] = 500;
            Omega[7][60] = 1.04;
            VyCoeff[7][70] = 500;
            Omega[7][70] = 1.07;
            VyCoeff[7][80] = 500;
            Omega[7][80] = 1.07;
            VyCoeff[7][90] = 500;
            Omega[7][90] = 1.11;
            VyCoeff[7][100] = 500;
            Omega[7][100] = 1.15;
            VyCoeff[7][110] = 500;
            Omega[7][110] = 1.1;
            VyCoeff[7][120] = 350;
            Omega[7][120] = 1.05;
            VyCoeff[7][180] = 150;
            Omega[7][180] = .97;
            #endregion
            #region Robot 8
            VyCoeff[8] = new Dictionary<int, double>();
            Omega[8] = new Dictionary<int, double>();
            VyCoeff[8][30] = 400;
            Omega[8][30] = 1.07;
            VyCoeff[8][40] = 400;
            Omega[8][40] = 1.03;
            VyCoeff[8][50] = 400;
            Omega[8][50] = 1.12;
            VyCoeff[8][60] = 400;
            Omega[8][60] = 1.05;
            VyCoeff[8][70] = 400;
            Omega[8][70] = 1.078;
            VyCoeff[8][80] = 400;
            Omega[8][80] = 1.08;
            VyCoeff[8][90] = 450;
            Omega[8][90] = .95;
            VyCoeff[8][100] = 400;
            Omega[8][100] = 1.07;
            VyCoeff[8][110] = 400;
            Omega[8][110] = 1.07;
            VyCoeff[8][120] = 400;
            Omega[8][120] = 1.07;
            VyCoeff[8][180] = 200;
            Omega[8][180] = .96;
            #endregion

        }


        bool isInRotate = false;
        private int CorrectionCounter = 0;
        public double BackBallDist = 0;
        public SingleWirelessCommand Static(WorldModel Model, int RobotID, Position2D Target, double backBallDist)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            //    Controller.DontGoInDangerZone = true;
            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLocation = /*CalculateBall(Model, RobotID);*/Model.BallState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = Target - ballLocation;
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(backBallDist);
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;
            double segmentConst = 0.7;
            double rearDistance = 0.2;
            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            Position2D finalPosToGo = midPoint;
            double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

            if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
                finalPosToGo = backBallPoint;
            else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 120)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }

            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);

            //  SWC = Controller.CalculateTargetSpeed(Model, RobotID, finalPosToGo, ballTargetVec.AngleInDegrees, null);
            Planner.Add(RobotID, finalPosToGo, (float)ballTargetVec.AngleInDegrees, PathType.UnSafe, true, true, false, false);
            return SWC;
        }
        public bool IsGotoPointState(WorldModel Model, int RobotID, Position2D Target, double Teta)
        {
            if (isInRotate)
                return false;
            SingleObjectState Ball = Model.BallState;

            double TetaInRadian = ClockWise * (Teta * Math.PI / 180);
            double robotangle = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
            Vector2D BallTarget = Target - Ball.Location;
            double tmpTeta = (BallTarget.AngleInRadians + TetaInRadian);
            GotoPointTarget = Ball.Location - Vector2D.FromAngleSize(tmpTeta, gotoPointExtendSize);
            gotoPointTeta = tmpTeta * 180 / Math.PI;
            if (!BallAvoided)
            {
                flag = false;
                BackBallDist = 0.15;
                Position2D target = Model.BallState.Location + (GotoPointTarget - Model.BallState.Location).GetNormalizeToCopy(BackBallDist);
                if (Model.OurRobots[RobotID].Location.DistanceFrom(target) > gotoPointTresh)
                {
                    GotoPointTarget = target;
                    return true;
                }
                BallAvoided = true;
            }
            BackBallDist = gotoPointExtendSize;
            //GotoPointTarget = GotoPointTarget.Extend(0, .02);
            if (Model.OurRobots[RobotID].Location.DistanceFrom(GotoPointTarget) < gotoPointTresh /*&& Model.OurRobots[RobotID].Speed.Size <= 0.05*/)
                flag = true;
            if (flag && GotoPointCounter < GotoPointDelay)
            {
                GotoPointCounter++;
                isInRotateDelay = true;
            }
            if (GotoPointCounter < GotoPointDelay)
                return true;
            isInRotate = true;
            return false;
        }
        bool Debug = false;

        public bool correcting = true;
        public static bool correct = true;
        #region Tune Variable
        public static double CustomVYCoef = 0;
        public static double CustomOmegaCoef = 0;
        public static bool InTuneMode = false;

        #endregion
        /// <summary>
        /// Normal Rotate
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Target"></param>
        /// <param name="Teta"></param>
        /// <param name="kicktype"></param>
        /// <param name="Kickspeed"></param>
        /// <param name="RobotID"></param>
        /// <param name="IsChipKick"></param>
        /// <param name="backSensor"></param>
        /// <returns></returns>


        /// <summary>
        /// Rotate with Manual Direction
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Target"></param>
        /// <param name="Teta"></param>
        /// <param name="kicktype"></param>
        /// <param name="Kickspeed"></param>
        /// <param name="RobotID"></param>
        /// <param name="IsChipKick"></param>
        /// <param name="ClockWizeRotate"></param>
        /// <param name="backSensor"></param>
        /// <returns></returns>
        /// 
        Position2D firstBallPos = Position2D.Zero;
        bool first = true;
        public SingleWirelessCommand rotate(WorldModel Model, bool fast, Position2D Target, double Teta, kickPowerType kicktype, double Kickspeed, int RobotID, bool IsChipKick, bool backSensor = false)
        {

            if (first)
            {
                firstBallPos = Model.BallState.Location;
                first = false;
            }
            //correcting = false;
            if (correcting)
            {
                if (RotateParameters.TuneFlag)
                {
                    OmegaCoef = RotateParameters.Omegacoeff;
                    VyCoef = RotateParameters.Vycoeff;
                }
                SWC.RobotID = RobotID;
                RotateCounter++;
                counter1 = RotateCounter;
                double TetaInRadian = ClockWise * Teta * (Math.PI / 180);
                CorrectionCounter++;

                Vector2D Ballrobot = Model.BallState.Location - Model.OurRobots[RobotID].Location;
                Vector2D TargetBall = Target - Model.BallState.Location;

                double tetaErr = TargetBall.AngleInDegrees;
                double angB2w = Vector2D.AngleBetweenInDegrees(TargetBall, Ballrobot);

                double teta = (tetaErr * Math.PI) / 180;

                if (Math.Abs(Teta) < 10)
                    close = true;

                if (close)
                {
                    double ay = 3, maxVy = 1, angErrTresh = 1, xErrTresh = 0.0053;

                    double errAng = 0;
                    double W = -AngularController(Model, tetaErr, RobotID, ref errAng);
                    Vector2D targetball = Target - Model.BallState.Location;
                    double errX = 0;
                    double output = XController(Model, targetball, RobotID, ref errX);

                    OUT.X = output;
                    if (!fast)
                    {
                        ay = 3;
                        maxVy = 1;
                        OUT.Y += ay * StaticVariables.FRAME_PERIOD;

                        if (OUT.Y > maxVy)
                            OUT.Y = maxVy;
                    }
                    else
                    {
                        ay = 4;
                        maxVy = 2;
                        OUT.Y += ay * StaticVariables.FRAME_PERIOD;

                        if (OUT.Y > maxVy)
                            OUT.Y = maxVy;
                    }
                    OUT.NormalizeTo(Math.Min(OUT.Size, MaxSize));
                    Vector2D v = Vector2D.FromAngleSize((targetball).AngleInRadians - Math.PI / 2 + OUT.AngleInRadians, OUT.Size);
                    v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);
                    double Vx = v.X;
                    SWC.W = W;
                    SWC.Vx = Vx;
                    SWC.Vy = OUT.Y;
                    if (!fast)
                    {
                        if ((Math.Abs(errAng) < angErrTresh && Math.Abs(errX) < xErrTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .14) || ((RotateCounter > 60) || (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.04)))
                        {
                            kickBall = true;
                            inKickState = true;
                        }
                    }
                    else
                    {
                        if ((Math.Abs(errAng) < angErrTresh && Math.Abs(errX) < xErrTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .15) || Model.BallState.Location.DistanceFrom(firstBallPos) > 0.04)
                        {
                            kickBall = true;
                            inKickState = true;
                        }
                    }
                    if (kickBall)
                    {
                        SWC.isChipKick = IsChipKick;
                        if (IsChipKick)
                        {
                            //SWC.BackSensor = backSensor;
                            SWC.isDelayedKick = backSensor;
                        }
                        if (kicktype == kickPowerType.Power)
                            SWC.KickPower = Kickspeed;
                        else
                            SWC.KickSpeed = Kickspeed;

                    }

                }

                #region Triangle
                else //if ((RotateRadius * TetaInRadian) <= (Vmax * Vmax / a))//Triangle
                {
                    #region NEW
                    if (!oldRotate)
                    {
                        double t1 = Math.Sqrt((ClockWise) * (RotateRadius * TetaInRadian) / a);
                        int t1FrameCount = (int)Math.Round((t1 / (FramePeriod)));
                        SWC.AddRotate(5, new SingleRotateCommand(ClockWise, (int)Teta, t1FrameCount, VyCoef, OmegaCoef), IsChipKick);
                        if (kicktype == kickPowerType.Power)
                            SWC.KickPower = Kickspeed;
                        else
                            SWC.KickSpeed = Kickspeed;
                    }
                    #endregion
                    #region Old
                    else
                    {
                        double t1 = Math.Sqrt((ClockWise) * (RotateRadius * TetaInRadian) / a);
                        int t1FrameCount = (int)Math.Round((t1 / (FramePeriod)));
                        int t2FrameCount = 2 * t1FrameCount;
                        double Omega1 = 0;

                        if (RotateCounter <= t1FrameCount + 1)
                        {
                            SWC.Vx += ClockWise * ax;
                            Omega1 = SWC.Vx / RotateRadius;
                            SWC.W = Omega1 * OmegaCoef;
                            SWC.Vy += ay / (VyCoef * Teta);
                        }
                        else if (RotateCounter <= t2FrameCount + 1)
                        {
                            SWC.Vx -= ClockWise * ax;
                            Omega1 = SWC.Vx / RotateRadius;
                            SWC.W = Omega1 * OmegaCoef;
                            SWC.Vy += ay / (VyCoef * Teta);

                        }
                        else
                        {
                            SWC.Vx = 0;
                            Omega1 = ClockWise * (SWC.Vx / RotateRadius);
                            SWC.W = Omega1 * OmegaCoef;

                            inKickState = true;
                            SWC.isChipKick = IsChipKick;
                            SWC.isDelayedKick = backSensor;
                            //SWC.BackSensor = backSensor;
                            if (kicktype == kickPowerType.Power)
                                SWC.KickPower = Kickspeed;
                            else
                                SWC.KickSpeed = Kickspeed;
                        }

                    }
                    #endregion
                }

                #endregion
                if (oldRotate)
                {
                    AnglesList.Add(Model.OurRobots[RobotID].Angle.Value);


                    if (CorrectionCounter == CheckFrame)
                    {
                        AngleGo = Math.Abs(AnglesList[CorrectionCounter - 1] - AnglesList[0]);
                        Vector2D targetRobot = Target - Model.OurRobots[RobotID].Location;
                        Vector2D RobotHeadVector = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1);
                        CorrectAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(targetRobot, RobotHeadVector));
                        Distance = Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location);
                        Vector2D BallRobot = Model.BallState.Location - Model.OurRobots[RobotID].Location;
                        correctBallAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(BallRobot, RobotHeadVector));
                        //correcting = false;
                    }
                    if (((AngleGo < ThreshholdAngleWasWent - 2 || AngleGo > ThreshholdAngleWasWent + 2) && CorrectionCounter == CheckFrame) && ((CorrectAngle < threshholdCorrectAnglesmall || CorrectAngle > threshholdCorrectAngleBig) && CorrectionCounter == CheckFrame) || ((correctBallAngle > 6) && CorrectionCounter == CheckFrame))
                    {
                        if (Debug)
                        {
                            DrawingObjects.AddObject(new Circle(Model.BallState.Location, .3), "dff");
                            DrawingObjects.AddObject(new StringDraw(correctBallAngle.ToString(), new Position2D(1, -1)), "dfs");
                            DrawingObjects.AddObject(new StringDraw(CorrectAngle.ToString(), new Position2D(1.1, -1)), "dfm");
                            DrawingObjects.AddObject(new StringDraw(Distance.ToString(), new Position2D(1.2, -1)), "dfc");
                            //DrawingObjects.AddObject(new StringDraw(correctBallAngle.ToString(), new Position2D(1.3, -1)), "dfs");
                        }
                        correcting = false;
                    }
                    if (Debug)
                    {
                        DrawingObjects.AddObject(new StringDraw("AngleGo " + AngleGo.ToString(), new Position2D(2, -1)), "df1");
                        DrawingObjects.AddObject(new StringDraw("CorrectionCounter " + CorrectionCounter.ToString(), new Position2D(2.1, -1)), "df2");
                        DrawingObjects.AddObject(new StringDraw("CheckFrame " + CheckFrame.ToString(), new Position2D(2.2, -1)), "df3");
                        DrawingObjects.AddObject(new StringDraw("ThreshholdAngleWasWent" + ThreshholdAngleWasWent.ToString(), new Position2D(2.3, -1)), "df4");
                        DrawingObjects.AddObject(new StringDraw("CorrectAngle " + CorrectAngle.ToString(), new Position2D(2.4, -1)), "df5");
                        DrawingObjects.AddObject(new StringDraw("threshholdCorrectAnglesmall " + threshholdCorrectAnglesmall.ToString(), new Position2D(2.5, -1)), "df6");
                        DrawingObjects.AddObject(new StringDraw("threshholdCorrectAngleBig " + threshholdCorrectAngleBig.ToString(), new Position2D(2.6, -1)), "df7");
                        DrawingObjects.AddObject(new StringDraw("correctBallAngle " + correctBallAngle.ToString(), new Position2D(2.7, -1)), "df8");
                    }
                    if(CorrectionCounter==CheckFrame)
                    {
                        correcting = false;
                    }
                    // correct = correcting;
                    // SWC.isDelayedKick = true;
                }
                return SWC;
            }
            else if (!correcting)
            {
                SingleWirelessCommand SWC = WithPID(Model, Target, RobotID, IsChipKick, Kickspeed, kicktype, backSensor, true, 100, 100, 400);
                return SWC;
            }
            return new SingleWirelessCommand();
        }
        public SingleWirelessCommand rotate(WorldModel Model, Position2D Target, double Teta, kickPowerType kicktype, double Kickspeed, int RobotID, bool IsChipKick, bool backSensor = false)
        {
            return rotate(Model, false, Target, Teta, kicktype, Kickspeed, RobotID, IsChipKick, backSensor);
        }

        private static Vector2D OUT2 = Vector2D.Zero;
        bool firstInPID = true;
        Vector2D lastV = Vector2D.Zero;
        double lastW = 0;
        double lastYcmd = 0;
        public SingleWirelessCommand WithPID(WorldModel Model, SingleObjectState Ball, double MargY, Position2D Target, int RobotID, bool IsChipKick, double Kickspeed, kickPowerType kicktype, bool backSensor, bool ballIsStatic, double alfamaxX, double alfamaxY, double alfamaxW, bool turnBall = false, double angErrTresh = 0.05, double xErrTresh = 0.01)
        {
            //ballIsStatic = true;
            //if (Model.BallState.Speed.Size > .05)
            //    ballIsStatic = false;
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            SWC.RobotID = RobotID;
            if (firstInPID)
            {
                lastV = (Model.lastVelocity.ContainsKey(RobotID)) ? Model.lastVelocity[RobotID] : Vector2D.Zero;
                lastW = (Model.lastW.ContainsKey(RobotID)) ? Model.lastW[RobotID] : 0;
                firstInPID = false;
            }
            Vector2D Ballrobot = Ball.Location - Model.OurRobots[RobotID].Location;
            Vector2D TargetBall = Target - Ball.Location;

            double tetaErr = TargetBall.AngleInDegrees;
            double ay = 3, maxVy = 1;

            double errAng = 0;
            double W = AngularController2(Model, tetaErr, RobotID, ref errAng);
            Vector2D targetball = Target - Ball.Location;

            double errY = 0;
            OUT2.Y = YController2(Model, targetball, RobotID, MargY, ref errY);

            Vector2D robotball = Model.OurRobots[RobotID].Location - Model.BallState.Location;
            Vector2D ErrVector = GameParameters.InRefrence(robotball, targetball);
            double Error = -ErrVector.X;

            if (Math.Abs(errAng) < angErrTresh && Math.Abs(Error) < xErrTresh)
            {
                lastYcmd = Math.Min(lastYcmd + ay * StaticVariables.FRAME_PERIOD, maxVy);
                //Vector2D tmpV = GameParameters.RotateCoordinates(new Vector2D(0, lastYcmd), Model.OurRobots[RobotID].Angle.Value);
                //tmpV = GameParameters.InRefrence(tmpV, targetball);
                OUT2.Y = lastYcmd;// Math.Min(OUT2.Y + ay * StaticVariables.FRAME_PERIOD, maxVy);
                //  DrawingObjects.AddObject(new Circle(Ball.Location, 0.1), "sssrc");
            }
            else
                lastYcmd = 0;

            double errX = 0;
            OUT2.X = XController2(Model, targetball, RobotID, !kickBall && turnBall, errY, ref errX);



            //DrawingObjects.AddObject(new Line(Ball.Location, Ball.Location + targetball, new Pen(Color.White, 0.01f)), "sssr");

            //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + OUT2, new Pen(Color.Blue, 0.01f)), "sss0");

            Vector2D v = GameParameters.InRefrence(OUT2, targetball);//Vector2D.FromAngleSize((targetball).AngleInRadians - Math.PI / 2 + OUT2.AngleInRadians, OUT2.Size);
            //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + v, new Pen(Color.Red, 0.01f)), "sss");

            double alfa = (v.X - lastV.X) * StaticVariables.FRAME_RATE;
            if (Math.Abs(alfa) > alfamaxX)
                v.X = (lastV.X) + Math.Sign(alfa) * alfamaxX * StaticVariables.FRAME_PERIOD;

            alfa = (v.Y - lastV.Y) * StaticVariables.FRAME_RATE;
            if (Math.Abs(alfa) > alfamaxY)
                v.Y = (lastV.Y) + Math.Sign(alfa) * alfamaxY * StaticVariables.FRAME_PERIOD;

            alfa = (W - lastW) * StaticVariables.FRAME_RATE;
            if (Math.Abs(alfa) > alfamaxW)
                W = (lastW) + Math.Sign(alfa) * alfamaxW * StaticVariables.FRAME_PERIOD;

            lastV = v;
            lastW = W;

            W *= -1;
            //   DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + v, new Pen(Color.YellowGreen, 0.01f)), "sss2");

            v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);

            double Vx = v.X;
            SWC.W = W;
            SWC.Vx = Vx;
            SWC.Vy = v.Y;

            if (Math.Abs(errAng) < angErrTresh && Math.Abs(Error) < xErrTresh || (ballIsStatic && Ball.Location.DistanceFrom(firstBallPos) > 0.07))
            {
                kickBall = true;
                inKickState = true;
            }
            //    SWC.Vy = .2;
            //if (!ballIsStatic && Ball.Location.DistanceFrom(firstBallPos) > 0.07)
            //{
            //    SWC.Vy = 0;
            //    SWC.Vx = 0;
            //    SWC.W = 0;
            //}
            if (kickBall)
            {
                //     OUT2.Y = Math.Min(OUT2.Y + ay * StaticVariables.FRAME_PERIOD, maxVy);
                SWC.isChipKick = IsChipKick;
                if (IsChipKick)
                {
                    //   SWC.BackSensor = backSensor;
                    SWC.isDelayedKick = backSensor;
                }
                if (kicktype == kickPowerType.Power)
                    SWC.KickPower = Kickspeed;
                else
                    SWC.KickSpeed = Kickspeed;
            }
            return SWC;
        }
        public SingleWirelessCommand WithPID(WorldModel Model, Position2D Target, int RobotID, bool IsChipKick, double Kickspeed, kickPowerType kicktype, bool backSensor, bool ballIsStatic, double alfamaxX, double alfamaxY, double alfamaxW)
        {
            return WithPID(Model, Model.BallState, -0.11, Target, RobotID, IsChipKick, Kickspeed, kicktype, backSensor, ballIsStatic, alfamaxX, alfamaxY, alfamaxW);
        }
        public double AngularController2(WorldModel model, double teta, int RobotID, ref double Error)
        {
            double OutPut = 0;
            if (model.OurRobots[RobotID].Angle.HasValue)
            {
                double lambda = .99, ki = 0.185, kp =8.0, kd = 0.05;

                teta = (teta * Math.PI) / 180;
                Error = teta - (model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                if (Error > Math.PI)
                {
                    Error -= (2 * Math.PI);
                }
                else if (Error < -(Math.PI))
                {
                    Error += (2 * Math.PI);
                }

                double dErr = 0;
                if (CounterErr == 0)
                {
                    LastE = Error;
                }
                dErr = (Error - LastE) / StaticVariables.FRAME_PERIOD;
                IErr = (IErr * lambda) + Error * StaticVariables.FRAME_PERIOD;
                if (Math.Abs(IErr) > MaxI)
                {
                    IErr = MaxI * Math.Sign(IErr);
                }
                OutPut = (kp * Error) + (ki * IErr) + (kd * dErr);
                //    CharterData.AddData("errw", Error);
                LastE = Error;
                CounterErr++;
            }
            return OutPut;
        }

        public double XController2(WorldModel model, Vector2D Refrence, int RobotID, bool turnB, double errY, ref double Error)
        {
            double lambda2 = 0.99, ki2 = .5, kp2 = 9.3, kd2 = 0;
            double output = 0;
            if (model.OurRobots[RobotID].Angle.HasValue)
            {
                Vector2D robotball = model.OurRobots[RobotID].Location - model.BallState.Location;
                Vector2D ErrVector = GameParameters.InRefrence(robotball, Refrence);
                double k = (turnB) ? 0.5 : 0;
                double refX = (k * Math.Abs(errY) * Math.Sign(ErrVector.X));
                refX = Math.Sign(refX) * Math.Min(Math.Abs(refX), .5);
                Error = refX - ErrVector.X;

                double dErr = 0;
                if (CounterErr2 == 0)
                {
                    LastEX = Error;
                }
                dErr = (Error - LastEX) / StaticVariables.FRAME_PERIOD;
                IErrX = (IErrX * lambda2) + Error * StaticVariables.FRAME_PERIOD;
                if (Math.Abs(IErrX) > MaxI)
                {
                    IErrX = MaxI * Math.Sign(IErrX);
                }
                output = (kp2 * Error) + (ki2 * IErrX) + (kd2 * dErr);

                LastEX = Error;
                CounterErr2++;
            }
          //  CharterData.AddData("errx", Error);
            return output;
        }
        int CounterErr3 = 0;
        public double YController2(WorldModel model, Vector2D Refrence, int RobotID, double MargY, ref double Error)
        {
            double lambda2 = 0.99, ki2 = .51, kp2 = 9, kd2 = 0;
            double output = 0;
            if (model.OurRobots[RobotID].Angle.HasValue)
            {
                Vector2D BallRobot = model.OurRobots[RobotID].Location - model.BallState.Location;
                Vector2D ErrVector = GameParameters.InRefrence(BallRobot, Refrence);
                Error = MargY - ErrVector.Y;
                double dErr = 0;
                if (CounterErr3 == 0)
                {
                    LastEY = Error;
                }
                dErr = (Error - LastEY) / StaticVariables.FRAME_PERIOD;
                IErrY = (IErrY * lambda2) + Error * StaticVariables.FRAME_PERIOD;
                if (Math.Abs(IErrY) > MaxI)
                {
                    IErrY = MaxI * Math.Sign(IErrY);
                }
                output = (kp2 * Error) + (ki2 * IErrY) + (kd2 * dErr);
          //      CharterData.AddData("erry", Error);

                LastEY = Error;
                CounterErr3++;
            }

            return output;
        }
        //public double YController2(WorldModel model, Vector2D Refrence, int RobotID, ref double Error)
        //{
        //    return YController2(model, Refrence, RobotID, -0.11, ref Error);
        //}
        /// <summary>
        /// Rotate With Feed Back in Middle Frame
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Target"></param>
        /// <param name="Teta"></param>
        /// <param name="kicktype"></param>
        /// <param name="Kickspeed"></param>
        /// <param name="RobotID"></param>
        /// <param name="IsChipKick"></param>
        /// <param name="backSensor"></param>
        /// <returns></returns>
        public SingleWirelessCommand rotateWFeedback(WorldModel Model, Position2D Target, double Teta, kickPowerType kicktype, double Kickspeed, int RobotID, bool IsChipKick, bool backSensor = false)
        {
            SWC.RobotID = RobotID;
            // For 0 degree of Rotate
            Vector2D Ballrobot = Model.BallState.Location - Model.OurRobots[RobotID].Location;
            Vector2D TargetBall = Target - Model.BallState.Location;

            double tetaErr = TargetBall.AngleInDegrees;
            double angB2w = Vector2D.AngleBetweenInDegrees(TargetBall, Ballrobot);

            double teta = (tetaErr * Math.PI) / 180;

            if (Math.Abs(Teta) < 10 && Math.Abs(angB2w) < Epsilon2)
                close = true;

            if (close)
            {
                double ay = 3, maxVy = 1, angErrTresh = 0.05, xErrTresh = 0.01;

                double errAng = 0;
                double W = -AngularController(Model, tetaErr, RobotID, ref errAng);
                Vector2D targetball = Target - Model.BallState.Location;
                double errX = 0;
                double output = XController(Model, targetball, RobotID, ref errX);

                OUT.X = output;

                OUT.Y = ay * StaticVariables.FRAME_PERIOD;

                if (OUT.Y > maxVy)
                    OUT.Y = maxVy;

                OUT.NormalizeTo(Math.Min(OUT.Size, MaxSize));
                Vector2D v = Vector2D.FromAngleSize((targetball).AngleInRadians - Math.PI / 2 + OUT.AngleInRadians, OUT.Size);
                v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);
                double Vx = v.X;
                SWC.W = W;
                SWC.Vx = Vx;
                SWC.Vy = OUT.AngleInDegrees;

                if (Math.Abs(errAng) < angErrTresh && Math.Abs(errX) < xErrTresh)
                {
                    SWC.isChipKick = IsChipKick;
                    if (IsChipKick)
                        SWC.BackSensor = backSensor;
                    if (kicktype == kickPowerType.Power)
                        SWC.KickPower = Kickspeed;
                    else
                        SWC.KickSpeed = Kickspeed;
                }

            }
            else
            {

                RotateCounter++;
                double TetaInRadian = ClockWise * Teta * (Math.PI / 180);

                #region Trapazoidal
                if ((RotateRadius * (TetaInRadian) > (Vmax * Vmax / a)))//Trapezoidal
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
                        if (IsChipKick)
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
                    double VariationCoef = 3, frameCoefY = 40, frameCoefOmega = 0.023;

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

                        Angles.Add(VarAngle);
                    }
                    if (RotateCounter == FeedBackFrame2 && counter == 0)
                    {
                        counter++;
                        Vector2D Targettoball = Target - Model.BallState.Location;
                        double robotangle = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                        Vector2D FromRobotAngle = Vector2D.FromAngleSize(robotangle, 1);
                        double VarAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(Targettoball, FromRobotAngle));

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
                            Frame = ((int)(Variation / VariationCoef));
                            VyCoef += Frame * frameCoefY;
                            OmegaCoef += Frame * frameCoefOmega;
                            intoe = false;
                        }

                    }
                    Vector2D Targettoball1 = Target - Model.BallState.Location;
                    double robotangle1 = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                    Vector2D FromRobotAngle1 = Vector2D.FromAngleSize(robotangle1, 1);
                    double VarAngle1 = Math.Abs(Vector2D.AngleBetweenInDegrees(Targettoball1, FromRobotAngle1));

                    if (RotateCounter <= t1FrameCount + 1)
                    {
                        if (RotateCounter == 1)
                        {
                            firstballdtate = Model.BallState.Location;
                            radius = Target.DistanceFrom(firstballdtate);
                        }
                        SWC.Vx += ClockWise * ax;
                        Omega = SWC.Vx / RotateRadius;
                        SWC.W = Omega * OmegaCoef;
                        SWC.Vy += ay / (VyCoef * Teta);
                        ///For FeedBack
                        Vector2D Targettoball = Target - Model.BallState.Location;
                        double robotangle = (Model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                        Vector2D RobotAngle = new Vector2D();
                        RobotAngle.Normnalize();
                        Vector2D FromRobotAngle = RobotAngle.GetRotate(robotangle);
                    }
                    else if (RotateCounter <= t2FrameCount + Frame + 1)
                    {
                        SWC.Vx -= ClockWise * ax;
                        Omega = SWC.Vx / RotateRadius;
                        SWC.W = Omega * OmegaCoef;
                        SWC.Vy += ay / (VyCoef * Teta);


                    }
                    else
                    {
                        SWC.Vx = 0;
                        SWC.W = 0;
                        if (RotateCounter > t2FrameCount + Frame + 20)
                        {
                            Vector2D firstlast = Model.BallState.Location - firstballdtate;
                            Vector2D targetvec = Target - firstballdtate;
                            double angle = Vector2D.AngleBetweenInDegrees(firstlast, targetvec);
                            p = ((angle * Math.PI) / 180) * radius;
                        }
                        SWC.isChipKick = IsChipKick;
                        if (IsChipKick)
                        {
                            SWC.BackSensor = backSensor;
                        }
                        if (kicktype == kickPowerType.Power)
                            SWC.KickPower = Kickspeed;
                        else
                            SWC.KickSpeed = Kickspeed;

                    }


                #endregion

                }
                if (Teta == 0)
                {
                    SWC.Vy = .05;
                    SWC.SpinBack = 0;
                    SWC.Vx = 0;
                    SWC.W = 0;
                }
            }
            return SWC;
        }

        /// <summary>
        /// Rotate with Fake Rotation
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Target"></param>
        /// <param name="Teta"></param>
        /// <param name="FakeTeta"></param>
        /// <param name="kicktype"></param>
        /// <param name="Kickspeed"></param>
        /// <param name="RobotID"></param>
        /// <param name="IsChipKick"></param>
        /// <param name="backSensor"></param>
        /// <returns></returns>
        public SingleWirelessCommand rotateWithFake(WorldModel Model, Position2D Target, double Teta, double FakeTeta, kickPowerType kicktype, double Kickspeed, int RobotID, bool IsChipKick, bool backSensor = false)
        {
            SWC.RobotID = RobotID;
            RotateCounter++;
            double faketurn = (fake) ? -1 : 1;
            double TetaInRadian;
            if (!fake)
            {
                TetaInRadian = ClockWise * (FakeTeta + Teta) * (Math.PI / 180);
            }
            else
            {
                TetaInRadian = ClockWise * FakeTeta * (Math.PI / 180);
            }

            if (FakeTeta + Teta < Epsilon)
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
                    SWC.Vx += faketurn * ClockWise * ax;
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
                    SWC.Vx -= faketurn * ClockWise * ax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);
                }
                else if (fake)
                {
                    fake = false;
                    RotateCounter = 0;
                    SWC.Vx = 0;
                    SWC.Vy = 0;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SetParams(Model, FakeTeta + Teta, RobotID);
                }
                else
                {
                    SWC.Vy = 0.5;
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

                double t1 = Math.Sqrt((ClockWise) * (RotateRadius * TetaInRadian) / a);
                int t1FrameCount = (int)Math.Round((t1 / (FramePeriod)));
                int t2FrameCount = 2 * t1FrameCount;
                double Omega = 0;
                if (RotateCounter <= t1FrameCount + 1)
                {
                    SWC.Vx += faketurn * ClockWise * ax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);
                }
                else if (RotateCounter <= t2FrameCount + 1)
                {
                    SWC.Vx -= faketurn * ClockWise * ax;
                    Omega = SWC.Vx / RotateRadius;
                    SWC.W = Omega * OmegaCoef;
                    SWC.Vy += ay / (VyCoef * Teta);

                }
                else if (fake)
                {
                    fake = false;
                    RotateCounter = 0;
                    SWC.Vx = 0;
                    SWC.Vy = 0;
                    Omega = ClockWise * (SWC.Vx / RotateRadius);
                    SWC.W = Omega * OmegaCoef;
                    SetParams(Model, FakeTeta + Teta, RobotID);
                }
                else /*if (RotateCounter > FrameNumT_2 + 1 && FrameCounter < FrameNumT_2 + 8)*/
                {
                    SWC.Vx = 0;
                    SWC.Vy = 0.5;
                    Omega = ClockWise * (SWC.Vx / RotateRadius);
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

            return SWC;
        }

        public void SetParams(WorldModel model, double Teta, int RobotID)
        {
            SetParams(model, Teta, RobotID, (model.BallState.Location.Y <= 0) ? true : false);
        }
        public void SetParams(WorldModel Model, double Teta, int RobotID, bool IsClockWise)
        {
            if (!oldRotate)
            {
                if (IsClockWise)
                    ClockWise = 1;
                else
                    ClockWise = -1;

                if (!RotateParametersNew.VyValues.ContainsKey(RobotID))//!VyCoeff.ContainsKey(RobotID))
                    RobotID = RotateParametersNew.VyValues.Keys.First();//VyCoeff.Keys.First();
                if (RotateParametersNew.VyValues[RobotID].ContainsKey((int)Teta))//(VyCoeff[RobotID].ContainsKey((int)Teta))
                    VyCoef = RotateParametersNew.VyValues[RobotID][(int)Teta];// VyCoeff[RobotID][(int)Teta];
                else
                {
                    var tmpDic = RotateParametersNew.VyValues[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);// VyCoeff[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);
                    VyCoef = tmpDic.First().Value;
                }
                if (!RotateParametersNew.OmegaValues.ContainsKey(RobotID))//(!Omega.ContainsKey(RobotID))
                    RobotID = RotateParametersNew.OmegaValues.Keys.First();// Omega.Keys.First();
                if (RotateParametersNew.OmegaValues[RobotID].ContainsKey((int)Teta))//(Omega[RobotID].ContainsKey((int)Teta))
                    OmegaCoef = RotateParametersNew.OmegaValues[RobotID][(int)Teta];// Omega[RobotID][(int)Teta];
                else
                {
                    var tmpDic = RotateParametersNew.OmegaValues[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);// Omega[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);
                    OmegaCoef = tmpDic.First().Value;
                }

                //if (Teta >= 180)
                //{
                //    a = 2.5;
                //    ax = a / 64;
                //    ay = a * 100;
                //    Vmax = 3;
                //}
                #region threshholds
                if (Teta == 120)
                {
                    ThreshholdAngleWasWent = 45.88280296;
                    threshholdCorrectAnglesmall = 68;
                    threshholdCorrectAngleBig = 76;
                    CheckFrame = 23;
                }
                if (Teta == 110)
                {
                    ThreshholdAngleWasWent = 41.43997701;
                    threshholdCorrectAnglesmall = 62;
                    threshholdCorrectAngleBig = 70;
                    CheckFrame = 21;
                }
                if (Teta == 100)
                {
                    ThreshholdAngleWasWent = 38.31626511;
                    threshholdCorrectAnglesmall = 57;
                    threshholdCorrectAngleBig = 61;
                    CheckFrame = 21;
                }
                if (Teta == 90)
                {
                    ThreshholdAngleWasWent = 43.1689754;//39.83420308;
                    threshholdCorrectAnglesmall = 42;//46;
                    threshholdCorrectAngleBig = 45;//50;
                    CheckFrame = 21;
                }
                else if (Teta == 80)
                {
                    ThreshholdAngleWasWent = 34.7378243;//26.75855446;
                    threshholdCorrectAnglesmall = 41;//49;
                    threshholdCorrectAngleBig = 46;//52;
                    CheckFrame = 17;
                }
                else if (Teta == 70)
                {
                    ThreshholdAngleWasWent = 41.09393292;//29.78207906;
                    threshholdCorrectAnglesmall = 26;//36;
                    threshholdCorrectAngleBig = 31;//41;
                    CheckFrame = 19;
                }
                else if (Teta == 60)
                {
                    ThreshholdAngleWasWent = 35.36829575;//29.99962997;
                    threshholdCorrectAnglesmall = 21;//27;
                    threshholdCorrectAngleBig = 25;//31;
                    CheckFrame = 19;
                }
                else if (Teta == 50)
                {
                    ThreshholdAngleWasWent = 33.83410079;// 28.8251063;
                    threshholdCorrectAnglesmall = 14;//19;
                    threshholdCorrectAngleBig = 18;//22;
                    CheckFrame = 18;
                }
                else if (Teta == 40)
                {
                    ThreshholdAngleWasWent = 24.37563663;
                    threshholdCorrectAnglesmall = 11;//16.5;
                    threshholdCorrectAngleBig = 15;//20;
                    CheckFrame = 16;
                }
                else if (Teta == 30)
                {
                    ThreshholdAngleWasWent = 23.30184535;
                    threshholdCorrectAnglesmall = 6;
                    threshholdCorrectAngleBig = 11;
                    CheckFrame = 15;
                }
                #endregion
            }
            else
            {
                if (IsClockWise)
                    ClockWise = 1;
                else
                    ClockWise = -1;

                if (!RotateParameters.VyValues.ContainsKey(RobotID))//!VyCoeff.ContainsKey(RobotID))
                    RobotID = RotateParameters.VyValues.Keys.First();//VyCoeff.Keys.First();
                if (RotateParameters.VyValues[RobotID].ContainsKey((int)Teta))//(VyCoeff[RobotID].ContainsKey((int)Teta))
                    VyCoef = RotateParameters.VyValues[RobotID][(int)Teta];// VyCoeff[RobotID][(int)Teta];
                else
                {
                    var tmpDic = RotateParameters.VyValues[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);// VyCoeff[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);
                    VyCoef = tmpDic.First().Value;
                }
                if (!RotateParameters.OmegaValues.ContainsKey(RobotID))//(!Omega.ContainsKey(RobotID))
                    RobotID = RotateParameters.OmegaValues.Keys.First();// Omega.Keys.First();
                if (RotateParameters.OmegaValues[RobotID].ContainsKey((int)Teta))//(Omega[RobotID].ContainsKey((int)Teta))
                    OmegaCoef = RotateParameters.OmegaValues[RobotID][(int)Teta];// Omega[RobotID][(int)Teta];
                else
                {
                    var tmpDic = RotateParameters.OmegaValues[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);// Omega[RobotID].OrderBy(o => Math.Abs(o.Value - Teta)).ToDictionary(k => k.Key, v => v.Value);
                    OmegaCoef = tmpDic.First().Value;
                }

                if (Teta >= 180)
                {
                    a = 2.5;
                    ax = a / 64;
                    ay = a * 100;
                    Vmax = 3;
                }
                #region threshholds
                if (Teta == 120)
                {
                    ThreshholdAngleWasWent = 45.88280296;
                    threshholdCorrectAnglesmall = 68;
                    threshholdCorrectAngleBig = 76;
                    CheckFrame = 23;
                }
                if (Teta == 110)
                {
                    ThreshholdAngleWasWent = 41.43997701;
                    threshholdCorrectAnglesmall = 62;
                    threshholdCorrectAngleBig = 70;
                    CheckFrame = 21;
                }
                if (Teta == 100)
                {
                    ThreshholdAngleWasWent = 38.31626511;
                    threshholdCorrectAnglesmall = 57;
                    threshholdCorrectAngleBig = 61;
                    CheckFrame = 21;
                }
                if (Teta == 90)
                {
                    ThreshholdAngleWasWent = 43.1689754;//39.83420308;
                    threshholdCorrectAnglesmall = 42;//46;
                    threshholdCorrectAngleBig = 45;//50;
                    CheckFrame = 21;
                }
                else if (Teta == 80)
                {
                    ThreshholdAngleWasWent = 34.7378243;//26.75855446;
                    threshholdCorrectAnglesmall = 41;//49;
                    threshholdCorrectAngleBig = 46;//52;
                    CheckFrame = 17;
                }
                else if (Teta == 70)
                {
                    ThreshholdAngleWasWent = 41.09393292;//29.78207906;
                    threshholdCorrectAnglesmall = 26;//36;
                    threshholdCorrectAngleBig = 31;//41;
                    CheckFrame = 19;
                }
                else if (Teta == 60)
                {
                    ThreshholdAngleWasWent = 35.36829575;//29.99962997;
                    threshholdCorrectAnglesmall = 21;//27;
                    threshholdCorrectAngleBig = 25;//31;
                    CheckFrame = 19;
                }
                else if (Teta == 50)
                {
                    ThreshholdAngleWasWent = 33.83410079;// 28.8251063;
                    threshholdCorrectAnglesmall = 14;//19;
                    threshholdCorrectAngleBig = 18;//22;
                    CheckFrame = 18;
                }
                else if (Teta == 40)
                {
                    ThreshholdAngleWasWent = 24.37563663;
                    threshholdCorrectAnglesmall = 11;//16.5;
                    threshholdCorrectAngleBig = 15;//20;
                    CheckFrame = 16;
                }
                else if (Teta == 30)
                {
                    ThreshholdAngleWasWent = 23.30184535;
                    threshholdCorrectAnglesmall = 6;
                    threshholdCorrectAngleBig = 11;
                    CheckFrame = 15;
                }
                #endregion
            }
        }
        public double AngularController(WorldModel model, double teta, int RobotID, ref double Error)
        {
            double OutPut = 0;
            if (model.OurRobots[RobotID].Angle.HasValue)
            {
                double lambda = .99, ki = 1, kp = 0, kd = 0;

                teta = (teta * Math.PI) / 180;
                Error = teta - (model.OurRobots[RobotID].Angle.Value * Math.PI) / 180;
                if (Error > Math.PI)
                {
                    Error -= (2 * Math.PI);
                }
                else if (Error < -(Math.PI))
                {
                    Error += (2 * Math.PI);
                }

                double dErr = 0;
                if (CounterErr > 0)
                {
                    LastE = Error;
                }
                dErr = (Error - LastE) / StaticVariables.FRAME_PERIOD;
                IErr = (IErr * lambda) + Error * StaticVariables.FRAME_PERIOD;
                if (Math.Abs(IErr) > MaxI)
                {
                    IErr = MaxI * Math.Sign(IErr);
                }
                OutPut = (kp * Error) + (ki * IErr) + (kd * dErr);

                LastE = Error;
                CounterErr++;
            }
            return OutPut;
        }

        public double XController(WorldModel model, Vector2D Refrence, int RobotID, ref double Error)
        {
            double lambda2 = 0.99, ki2 = 0.7, kp2 = 6, kd2 = 0.1;
            double output = 0;
            if (model.OurRobots[RobotID].Angle.HasValue)
            {
                Vector2D robotball = model.BallState.Location - model.OurRobots[RobotID].Location.Extend(0, 0);

                Vector2D ErrVector = GameParameters.InRefrence(robotball, Refrence);
                Error = -ErrVector.X;

                double dErr = 0;
                if (CounterErr2 > 0)
                {
                    LastEX = Error;
                }
                dErr = (Error - LastEX) / StaticVariables.FRAME_PERIOD;
                IErrX = (IErrX * lambda2) + Error * StaticVariables.FRAME_PERIOD;
                if (Math.Abs(IErrX) > MaxI)
                {
                    IErrX = MaxI * Math.Sign(IErrX);
                }
                output = (kp2 * Error) + (ki2 * IErrX) + (kd2 * dErr);

                LastEX = Error;
                CounterErr2++;
            }

            return output;
        }

        class Rotatedata
        {
            public SingleObjectState RobotStateCommand = new SingleObjectState();
            public SingleObjectState RobotStateVision = new SingleObjectState();
            public SingleObjectState RobotStateKAlman = new SingleObjectState();
            public double TargetAngle = 0;
        }
    }
}
