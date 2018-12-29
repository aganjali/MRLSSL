using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using System.IO;

namespace MRL.SSL.Planning.MotionPlanner
{
    public static class Planner
    {
        private static ERRTManager errtManager;
        private static Dictionary<int, Controller> controllers;
        private static Dictionary<int, SingleObjectState> goals = new Dictionary<int, SingleObjectState>();
        private static Dictionary<int, int> aballs = new Dictionary<int, int>();

        private static Dictionary<int, int> robotIds = new Dictionary<int, int>();
        private static Dictionary<int, SingleObjectState> initialStates = new Dictionary<int, SingleObjectState>();
        private static Dictionary<int, int> arobots = new Dictionary<int, int>();
        private static Dictionary<int, int> azones = new Dictionary<int, int>();
        private static Dictionary<int, int> aOppzones = new Dictionary<int, int>();

        private static Dictionary<int, PathType> types = new Dictionary<int, PathType>();
        private static Dictionary<int, CommandType> CommandTypes = new Dictionary<int, CommandType>();
        private static Dictionary<int, CommandType> LastCommandTypes = new Dictionary<int, CommandType>();
        private static Dictionary<int, SingleWirelessCommand> SWCommands = new Dictionary<int, SingleWirelessCommand>();
        private static Dictionary<int, kickData> kickdatas = new Dictionary<int, kickData>();
        private static Dictionary<int, Rotate> rotates = new Dictionary<int, Rotate>();
        private static Dictionary<int, bool> addedRotates = new Dictionary<int, bool>();
        private static Dictionary<int, SingleWirelessCommand> commands = new Dictionary<int, SingleWirelessCommand>();
        private static Dictionary<int, bool> ResetControllers = new Dictionary<int, bool>();
        private static Dictionary<int, bool> UseDefultParams = new Dictionary<int, bool>();
        private static Dictionary<int, bool> BackSensors = new Dictionary<int, bool>();
        private static Dictionary<int, bool> SpinBacks = new Dictionary<int, bool>();
        private static Dictionary<int, bool> CutOtherPaths = new Dictionary<int, bool>();
        private static Dictionary<int, bool> ReCalculateTeta = new Dictionary<int, bool>();

        private static Random rand = new Random();
        public static ParameterList defultParams = new ParameterList();
        private static Dictionary<int, Vector2D> lastVs = new Dictionary<int, Vector2D>();
        private static Dictionary<int, double> lastWs = new Dictionary<int, double>();

        private static Dictionary<int, int> lastVelResetCounter = new Dictionary<int, int>();
        public static Dictionary<int, Vector2D> AlfaList = new Dictionary<int, Vector2D>();
        private static bool stopBall = false;
        public static void IsStopBall(bool s)
        {
            stopBall = s;
        }
        public static void Add(int RobotID, SingleObjectState finalState, PathType type, bool avoidBall, bool avoidRobots, bool avoidOurDangerZone, bool avoidOppDangerZone)
        {
            goals[RobotID] = finalState;
            if (avoidBall)
                aballs[RobotID] = 1;
            else
                aballs[RobotID] = 0;

            if (avoidRobots)
                arobots[RobotID] = 1;
            else
                arobots[RobotID] = 0;

            if (avoidOurDangerZone)
                azones[RobotID] = 1;
            else
                azones[RobotID] = 0;
            if (avoidOppDangerZone)
                aOppzones[RobotID] = 1;
            else
                aOppzones[RobotID] = 0;
            types[RobotID] = type;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, SingleObjectState finalState, PathType type, bool avoidBall, bool avoidRobots, bool avoidOurDangerZone, bool avoidOppDangerZone, bool spin)
        {
            goals[RobotID] = finalState;
            if (avoidBall)
                aballs[RobotID] = 1;
            else
                aballs[RobotID] = 0;

            if (avoidRobots)
                arobots[RobotID] = 1;
            else
                arobots[RobotID] = 0;

            if (avoidOurDangerZone)
                azones[RobotID] = 1;
            else
                azones[RobotID] = 0;
            if (avoidOppDangerZone)
                aOppzones[RobotID] = 1;
            else
                aOppzones[RobotID] = 0;
            types[RobotID] = type;
            kickdatas[RobotID] = new kickData(spin);
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, Position2D finalState, double angle, PathType type, bool avoidBall, bool avoidRobots, bool avoidOurDangerZone, bool avoidOppDangerZone)
        {
            goals[RobotID] = new SingleObjectState(finalState, Vector2D.Zero, (float)angle);
            if (avoidBall)
                aballs[RobotID] = 1;
            else
                aballs[RobotID] = 0;

            if (avoidRobots)
                arobots[RobotID] = 1;
            else
                arobots[RobotID] = 0;

            if (avoidOurDangerZone)
                azones[RobotID] = 1;
            else
                azones[RobotID] = 0;
            if (avoidOppDangerZone)
                aOppzones[RobotID] = 1;
            else
                aOppzones[RobotID] = 0;
            types[RobotID] = type;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, SingleObjectState finalState, PathType type, bool avoidBall, bool avoidRobots, bool avoidOurDangerZone)
        {
            goals[RobotID] = finalState;
            if (avoidBall)
                aballs[RobotID] = 1;
            else
                aballs[RobotID] = 0;

            if (avoidRobots)
                arobots[RobotID] = 1;
            else
                arobots[RobotID] = 0;

            if (avoidOurDangerZone)
                azones[RobotID] = 1;
            else
                azones[RobotID] = 0;
            aOppzones[RobotID] = 0;
            types[RobotID] = type;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }

        public static void Add(int RobotID, Position2D finalState, double angle, PathType type, bool avoidBall, bool avoidRobots, bool avoidOurDangerZone, bool avoidOppDangerZone, bool spin)
        {
            goals[RobotID] = new SingleObjectState(finalState, Vector2D.Zero, (float)angle);
            if (avoidBall)
                aballs[RobotID] = 1;
            else
                aballs[RobotID] = 0;

            if (avoidRobots)
                arobots[RobotID] = 1;
            else
                arobots[RobotID] = 0;

            if (avoidOurDangerZone)
                azones[RobotID] = 1;
            else
                azones[RobotID] = 0;
            if (avoidOppDangerZone)
                aOppzones[RobotID] = 1;
            else
                aOppzones[RobotID] = 0;
            types[RobotID] = type;
            kickdatas[RobotID] = new kickData(spin);
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;


        }
        public static void Add(int RobotID, SingleObjectState finalState, PathType type, bool avoidBall, bool avoidRobots)
        {
            goals[RobotID] = finalState;
            if (avoidBall)
                aballs[RobotID] = 1;
            else
                aballs[RobotID] = 0;

            if (avoidRobots)
                arobots[RobotID] = 1;
            else
                arobots[RobotID] = 0;
            azones[RobotID] = 1;
            aOppzones[RobotID] = 0;
            types[RobotID] = type;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, SingleObjectState finalState)
        {
            goals[RobotID] = finalState;
            aballs[RobotID] = 0;
            arobots[RobotID] = 0;
            azones[RobotID] = 1;
            aOppzones[RobotID] = 0;
            types[RobotID] = PathType.UnSafe;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, Position2D finalState, double finalAngle)
        {
            goals[RobotID] = new SingleObjectState(finalState, Vector2D.Zero, (float)finalAngle);
            aballs[RobotID] = 0;
            arobots[RobotID] = 0;
            azones[RobotID] = 1;
            aOppzones[RobotID] = 0;
            types[RobotID] = PathType.UnSafe;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, Position2D finalState, double finalAngle, bool avoidDangerZone)
        {
            goals[RobotID] = new SingleObjectState(finalState, Vector2D.Zero, (float)finalAngle);
            aballs[RobotID] = 0;
            arobots[RobotID] = 0;
            if (avoidDangerZone)
                azones[RobotID] = 1;
            else
                azones[RobotID] = 0;
            aOppzones[RobotID] = 0;
            types[RobotID] = PathType.UnSafe;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, SingleObjectState finalState, bool avoidDangerZone)
        {
            goals[RobotID] = finalState;
            aballs[RobotID] = 0;
            arobots[RobotID] = 0;
            if (avoidDangerZone)
                azones[RobotID] = 1;
            else
                azones[RobotID] = 0;
            aOppzones[RobotID] = 0;
            types[RobotID] = PathType.UnSafe;
            CommandTypes[RobotID] = CommandType.GotoPointCommand;
            if (!UseDefultParams.ContainsKey(RobotID))
                UseDefultParams[RobotID] = true;
        }
        public static void Add(int RobotID, SingleWirelessCommand SWC, bool resetController = true)
        {
            ResetControllers[RobotID] = resetController;
            CommandTypes[RobotID] = CommandType.WirelessCommand;
            SWCommands[RobotID] = SWC;
        }

        public static int GetRotateTime(double teta)
        {
            int Time = 0;
            teta = Math.Round(teta);
            teta = teta - (teta % 10);
            if (teta > 120 && teta < 150)
                teta = 120;
            else if (teta >= 150)
                teta = 180;
            if (teta <= 10)
                Time = 10;
            else if (teta <= 30)
            {
                Time = 27;
            }
            else if (teta <= 40)
            {
                Time = 29;
            }
            else if (teta <= 50)
            {
                Time = 32;
            }
            else if (teta <= 60)
            {
                Time = 34;
            }

            else if (teta <= 70)
            {
                Time = 37;
            }
            else if (teta <= 80)
            {
                Time = 42;
            }
            else if (teta <= 90)
            {
                Time = 43;
            }
            else if (teta <= 100)
            {
                Time = 48;
            }
            else if (teta <= 110)
            {
                Time = 45;
            }
            else if (teta < 150)
            {
                Time = 47;
            }
            else
            {
                Time = 62;
            }
            return Time;

        }
        public static int GetRotateTime(WorldModel Model, Position2D Initial, Position2D Target)
        {
            Vector2D BallTarget = Target - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - Initial;
            double teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            return GetRotateTime(teta);
        }
        public static int GetRotateTime(double Teta, double FakeTeta)
        {
            Teta = Math.Round(Teta);
            Teta = Teta - (Teta % 10);
            FakeTeta = Math.Round(FakeTeta);
            FakeTeta = FakeTeta - (FakeTeta % 10);

            if (Teta > 120 && Teta < 150)
                Teta = 120;
            if (Teta > 150)
                Teta = 180;

            if (Teta + FakeTeta > 120 && Teta + FakeTeta < 150)
                FakeTeta = 120 - Teta;
            else if (Teta + FakeTeta > 150)
                FakeTeta = 180 - Teta;
            int tmpTeta = GetRotateTime(Teta + FakeTeta);
            int tmpFakeTeta = GetRotateTime(FakeTeta);
            return tmpTeta + tmpFakeTeta;
        }
        public static int GetRotateTime(WorldModel Model, Position2D Initial, Position2D Target, double FakeTeta)
        {
            Vector2D BallTarget = Target - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - Initial;
            double teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            return GetRotateTime(teta, FakeTeta);
        }
        //public static int GetMotionTime(WorldModel Model, int RobotId, Position2D Init, Position2D Target)
        //{
        //    return GetMotionTime(Model, RobotId, Init, Target, ActiveParameters.RobotMotionCoefs);
        //}
        //  public static int GetMotionTime(WorldModel Model, int RobotId, Position2D Init, Position2D Target, double[,] coefs)
        //  {
        //      Vector2D init2Target = Target - Init;
        //      double timeR = 0;

        //      Vector2D r = Vector2D.FromAngleSize((Model.OurRobots.ContainsKey(RobotId)) ? Model.OurRobots[RobotId].Angle.Value * Math.PI / 180 : 0, 1);
        //      Vector2D vec = Vector2D.FromAngleSize(Math.Abs(Vector2D.AngleBetweenInRadians(r, init2Target)), Math.Min(init2Target.Size, 2));
        ////      if(vec.Size < 0.05)
        //      for (int k = 0; k < 6; k++)
        //          for (int l = 0; l < 6 - k; l++)
        //              timeR += coefs[k, l] * Math.Pow(vec.Size, k) * Math.Pow(vec.AngleInRadians, l);
        //      //      timeR /= StaticVariables.FRAME_PERIOD;
        //      timeR += (Math.Max(0, init2Target.Size - 2) / 2.8) / StaticVariables.FRAME_PERIOD;

        //      if (timeR > 360)
        //          timeR = 360;
        //      if (timeR < 0)
        //          timeR = 0;


        //      int t = (int)Math.Max(0, timeR);//(int)Math.Round(timeR / StaticVariables.FRAME_PERIOD);

        //      return t;
        //  }

        public static int GetMotionTime(WorldModel Model, int RobotId, Position2D Init, Position2D Target, double[,] coefs)
        {
            double maxD = 5;
            Vector2D init2Target = Target - Init;
            double timeR = 0;

            Vector2D r = Vector2D.FromAngleSize((Model.OurRobots.ContainsKey(RobotId)) ? Model.OurRobots[RobotId].Angle.Value * Math.PI / 180 : 0, 1);
            Vector2D vec = Vector2D.FromAngleSize(Math.Abs(Vector2D.AngleBetweenInRadians(r, init2Target)), Math.Min(init2Target.Size, maxD));

            for (int k = 0; k < 6; k++)
                for (int l = 0; l < 6 - k; l++)
                    timeR += coefs[k, l] * Math.Pow(vec.Size, k) * Math.Pow(vec.AngleInRadians, l);
            //      timeR /= StaticVariables.FRAME_PERIOD;
            timeR += (Math.Max(0, init2Target.Size - maxD) / 2.8) / StaticVariables.FRAME_PERIOD;

            if (timeR > 420)
                timeR = 420;
            if (timeR < 0)
                timeR = 0;


            int t = (int)Math.Max(0, timeR);//(int)Math.Round(timeR / StaticVariables.FRAME_PERIOD);

            return t;
        }

        public static int GetMotionTime(WorldModel Model, int RobotId, Position2D Init, Position2D Target, double[,] coefs, bool test)
        {
            Vector2D init2Target = Target - Init;
            double timeR = 0;
            Vector2D lastV = (Model.lastVelocity.ContainsKey(RobotId)) ? Model.lastVelocity[RobotId] : Vector2D.Zero;
            double v0 = GameParameters.InRefrence(lastV, init2Target).Y;
            double v0r = GameParameters.InRefrence(Model.OurRobots[RobotId].Speed, init2Target).Y;

            double sgn = Math.Sign(v0);
            double a = 6;
            double d = v0 * v0 / (2 * a);
            double t0 = sgn * (v0 / a);

            Vector2D r = Vector2D.FromAngleSize((Model.OurRobots.ContainsKey(RobotId)) ? Model.OurRobots[RobotId].Angle.Value * Math.PI / 180 : 0, 1);
            //Vector2D vec = Vector2D.FromAngleSize(Math.Abs(Vector2D.AngleBetweenInRadians(r, init2Target)), Math.Min(init2Target.Size, 2));
            double dt = Math.Max(init2Target.Size + d, 0);
            Vector2D vec = Vector2D.FromAngleSize(Math.Abs(Vector2D.AngleBetweenInRadians(r, init2Target)), Math.Min(dt, 2));

            for (int k = 0; k < 6; k++)
                for (int l = 0; l < 6 - k; l++)
                    timeR += coefs[k, l] * Math.Pow(vec.Size, k) * Math.Pow(vec.AngleInRadians, l);
            //      timeR /= StaticVariables.FRAME_PERIOD;
            timeR += (Math.Max(0, dt - 2) / 2.8) / StaticVariables.FRAME_PERIOD;

            timeR -= (t0 / StaticVariables.FRAME_PERIOD);
            if (timeR > 360)
                timeR = 360;
            if (timeR < 0)
                timeR = 0;


            int t = (int)Math.Max(0, timeR);//(int)Math.Round(timeR / StaticVariables.FRAME_PERIOD);

            return t;
        }

        public static void SetParameter(int RobotID, double maxAccel, bool f)
        {
            ParameterList pl = new ParameterList(ControlParameters.GetList()[0]);
            pl.Accel = maxAccel;
            ControlParameters.Set(RobotID, pl);
        }
        public static void SetParameter(int RobotID, double maxSpeed)
        {
            ParameterList pl = new ParameterList(ControlParameters.GetList()[0]);
            pl.MaxSpeed = maxSpeed;
            ControlParameters.Set(RobotID, pl);
        }
        public static void SetParameter(int RobotID, double accel, double maxSpeed)
        {
            ParameterList pl = new ParameterList(ControlParameters.GetList()[0]);
            pl.MaxSpeed = maxSpeed;
            pl.Accel = accel;
            ControlParameters.Set(RobotID, pl);
        }

        public static Rotate AddRotate(WorldModel Model, int RobotID, Position2D Target, double Teta, kickPowerType kickType, double KickSpeed, bool isChipKick, int gotoPointDelay = 60, bool backSensor = true)
        {
            addedRotates[RobotID] = true;
            if (!rotates.ContainsKey(RobotID) || (ReCalculateTeta.ContainsKey(RobotID) && ReCalculateTeta[RobotID]))
            {
                if (!rotates.ContainsKey(RobotID))
                    rotates[RobotID] = new Rotate();
                Teta = Math.Round(Teta);
                Teta = Teta - (Teta % 10);
                if (Teta > 120 && Teta < 150)
                    Teta = 120;
                else if (Teta >= 150)
                    Teta = 180;
                else if (Teta > 15 && Teta < 30)
                    Teta = 30;
                else if (Teta < 15)
                    Teta = 0;
                ReCalculateTeta[RobotID] = false;
                rotates[RobotID].RotateTeta = Teta;
                rotates[RobotID].SetParams(Model, rotates[RobotID].RotateTeta, RobotID);
            }
            else
                ReCalculateTeta[RobotID] = false;
            if (Teta > 0)
                backSensor = false;
            Teta = rotates[RobotID].RotateTeta;
            rotates[RobotID].GotoPointDelay = gotoPointDelay;
            if (rotates[RobotID].IsGotoPointState(Model, RobotID, Target, rotates[RobotID].RotateTeta))
            {
                CommandTypes[RobotID] = CommandType.GotoPointCommand;
                ChangeDefaulteParams(RobotID, false);
                SetParameter(RobotID, 2);

                rotates[RobotID].Static(Model, RobotID, Model.BallState.Location + (Model.BallState.Location - rotates[RobotID].GotoPointTarget).GetNormalizeToCopy(0.3), rotates[RobotID].BackBallDist);
                //Add(RobotID, new SingleObjectState(rotates[RobotID].GotoPointTarget, Vector2D.Zero, (float)rotates[RobotID].gotoPointTeta), PathType.UnSafe, true, true, true);
            }
            else
            {
                SingleWirelessCommand SWC = rotates[RobotID].rotate(Model, Target, Teta, kickType, KickSpeed, RobotID, isChipKick, backSensor);
                BackSensors[RobotID] = backSensor;
                //SWC.SpinBack = 1;
                Add(RobotID, SWC, false);
            }
            return rotates[RobotID];
        }
        public static Rotate AddRotate(WorldModel Model, int RobotID, Position2D Target, Position2D InitialPoint, kickPowerType kickType, double KickSpeed, bool isChipKick, int gotoPointDelay = 60, bool backSensor = true)
        {
            Vector2D BallTarget = Target - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPoint;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            return AddRotate(Model, RobotID, Target, Teta, kickType, KickSpeed, isChipKick, gotoPointDelay, backSensor);

        }
        public static Rotate AddRotate(WorldModel Model, int RobotID, Position2D Target, int MinTeta, int MaxTeta, kickPowerType kickType, double KickSpeed, bool isChipKick, int gotoPointDelay = 60, bool backSensor = true)
        {
            double Teta = MinTeta;
            if (!rotates.ContainsKey(RobotID))
                Teta = rand.NextDouble() * (MaxTeta - MinTeta) + MinTeta;
            return AddRotate(Model, RobotID, Target, Teta, kickType, KickSpeed, isChipKick, gotoPointDelay, backSensor);
        }
        public static Rotate AddRotate(WorldModel Model, int RobotID, Position2D Target, double Teta, kickPowerType kickType, double KickSpeed, bool isChipKick, bool ClockWise, int gotoPointDelay = 60, bool backSensor = true)
        {
            addedRotates[RobotID] = true;
            if (!rotates.ContainsKey(RobotID) || (ReCalculateTeta.ContainsKey(RobotID) && ReCalculateTeta[RobotID]))
            {
                if (!rotates.ContainsKey(RobotID))
                    rotates[RobotID] = new Rotate();
                Teta = Math.Round(Teta);
                Teta = Teta - (Teta % 10);
                if (Teta > 120 && Teta < 150)
                    Teta = 120;
                else if (Teta >= 150)
                    Teta = 180;
                else if (Teta > 15 && Teta < 30)
                    Teta = 30;
                else if (Teta < 15)
                    Teta = 0;
                ReCalculateTeta[RobotID] = false;
                rotates[RobotID].RotateTeta = Teta;
                rotates[RobotID].SetParams(Model, rotates[RobotID].RotateTeta, RobotID, ClockWise);
            }
            else
                ReCalculateTeta[RobotID] = false;
            if (Teta > 0)
                backSensor = false;
            Teta = rotates[RobotID].RotateTeta;
            rotates[RobotID].GotoPointDelay = gotoPointDelay;
            if (rotates[RobotID].IsGotoPointState(Model, RobotID, Target, rotates[RobotID].RotateTeta))
            {
                CommandTypes[RobotID] = CommandType.GotoPointCommand;
                ChangeDefaulteParams(RobotID, false);
                SetParameter(RobotID, 2);
                rotates[RobotID].Static(Model, RobotID, Model.BallState.Location + (Model.BallState.Location - rotates[RobotID].GotoPointTarget).GetNormalizeToCopy(0.3), rotates[RobotID].BackBallDist);
                //Add(RobotID, new SingleObjectState(rotates[RobotID].GotoPointTarget, Vector2D.Zero, (float)rotates[RobotID].gotoPointTeta), PathType.UnSafe, true, true, true);
            }
            else
            {
                SingleWirelessCommand SWC = rotates[RobotID].rotate(Model, Target, Teta, kickType, KickSpeed, RobotID, isChipKick, backSensor);
                Add(RobotID, SWC);
                BackSensors[RobotID] = backSensor;

            }
            return rotates[RobotID];
        }
        public static Rotate AddRotate(WorldModel Model, int RobotID, Position2D Target, Position2D InitPos, kickPowerType kickType, double KickSpeed, bool isChipKick, bool ClockWise, int gotoPointDelay = 60, bool backSensor = true)
        {
            Vector2D BallTarget = Target - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            return AddRotate(Model, RobotID, Target, Teta, kickType, KickSpeed, isChipKick, ClockWise, gotoPointDelay, backSensor);
        }
        public static Rotate AddRotate(WorldModel Model, int RobotID, bool fast, Position2D Target, double Teta, kickPowerType kickType, double KickSpeed, bool isChipKick, int gotoPointDelay = 60, bool backSensor = true)
        {
            addedRotates[RobotID] = true;
            if (!rotates.ContainsKey(RobotID) || (ReCalculateTeta.ContainsKey(RobotID) && ReCalculateTeta[RobotID]))
            {
                if (!rotates.ContainsKey(RobotID))
                    rotates[RobotID] = new Rotate();
                Teta = Math.Round(Teta);
                Teta = Teta - (Teta % 10);
                if (Teta > 120 && Teta < 150)
                    Teta = 120;
                else if (Teta >= 150)
                    Teta = 180;
                else if (Teta > 15 && Teta < 30)
                    Teta = 30;
                else if (Teta < 15)
                    Teta = 0;
                ReCalculateTeta[RobotID] = false;
                rotates[RobotID].RotateTeta = Teta;
                rotates[RobotID].SetParams(Model, rotates[RobotID].RotateTeta, RobotID);
            }
            else
                ReCalculateTeta[RobotID] = false;
            if (Teta > 0)
                backSensor = false;
            Teta = rotates[RobotID].RotateTeta;
            rotates[RobotID].GotoPointDelay = gotoPointDelay;
            if (rotates[RobotID].IsGotoPointState(Model, RobotID, Target, rotates[RobotID].RotateTeta))
            {
                CommandTypes[RobotID] = CommandType.GotoPointCommand;
                ChangeDefaulteParams(RobotID, false);
                SetParameter(RobotID, 2);
                rotates[RobotID].Static(Model, RobotID, Model.BallState.Location + (Model.BallState.Location - rotates[RobotID].GotoPointTarget).GetNormalizeToCopy(0.3), rotates[RobotID].BackBallDist);
                //Add(RobotID, new SingleObjectState(rotates[RobotID].GotoPointTarget, Vector2D.Zero, (float)rotates[RobotID].gotoPointTeta), PathType.UnSafe, true, true, true);
            }
            else
            {
                SingleWirelessCommand SWC = rotates[RobotID].rotate(Model, fast, Target, Teta, kickType, KickSpeed, RobotID, isChipKick, backSensor);
                Add(RobotID, SWC);
                BackSensors[RobotID] = backSensor;

            }
            return rotates[RobotID];
        }

        public static void SetReCalculateTeta(int RobotID, bool b)
        {
            ReCalculateTeta[RobotID] = b;
        }

        public static Rotate AddFakeRotate(WorldModel Model, int RobotID, Position2D Target, double FakeTeta, double RealTeta, kickPowerType kickType, double KickSpeed, bool isChipKick, int gotoPointDelay = 60, bool backSensor = true)
        {
            addedRotates[RobotID] = true;
            if (!rotates.ContainsKey(RobotID))
            {
                rotates[RobotID] = new Rotate();

                RealTeta = Math.Round(RealTeta);
                RealTeta = RealTeta - (RealTeta % 10);
                FakeTeta = Math.Round(FakeTeta);
                FakeTeta = FakeTeta - (FakeTeta % 10);

                if (RealTeta > 120 && RealTeta < 150)
                    RealTeta = 120;
                if (RealTeta > 150)
                    RealTeta = 180;

                if (RealTeta + FakeTeta > 120 && RealTeta + FakeTeta < 150)
                    FakeTeta = 120 - RealTeta;
                else if (RealTeta + FakeTeta > 150)
                    FakeTeta = 180 - RealTeta;

                rotates[RobotID].FakeRotateTeta = FakeTeta;
                rotates[RobotID].RealRotateTeta = RealTeta;
                rotates[RobotID].SetParams(Model, rotates[RobotID].FakeRotateTeta, RobotID);
                rotates[RobotID].GotoPointDelay = gotoPointDelay;
            }
            if (rotates[RobotID].IsGotoPointState(Model, RobotID, Target, rotates[RobotID].RealRotateTeta))
            {
                CommandTypes[RobotID] = CommandType.GotoPointCommand;
                rotates[RobotID].Static(Model, RobotID, Model.BallState.Location + (Model.BallState.Location - rotates[RobotID].GotoPointTarget).GetNormalizeToCopy(0.3), rotates[RobotID].BackBallDist);
                //   Add(RobotID, new SingleObjectState(rotates[RobotID].GotoPointTarget, Vector2D.Zero, (float)rotates[RobotID].gotoPointTeta), PathType.UnSafe, true, true, true);
            }
            else
            {
                SingleWirelessCommand SWC = rotates[RobotID].rotateWithFake(Model, Target, rotates[RobotID].RealRotateTeta, rotates[RobotID].FakeRotateTeta, kickType, KickSpeed, RobotID, isChipKick, backSensor);
                BackSensors[RobotID] = backSensor;
                Add(RobotID, SWC);
            }
            return rotates[RobotID];
        }
        public static Rotate AddFakeRotate(WorldModel Model, int RobotID, Position2D Target, Position2D InitialPoint, double FakeTeta, kickPowerType kickType, double KickSpeed, bool isChipKick, int gotoPointDelay = 60, bool backSensor = true)
        {
            Vector2D BallTarget = Target - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPoint;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            return AddFakeRotate(Model, RobotID, Target, FakeTeta, Teta, kickType, KickSpeed, isChipKick, gotoPointDelay);
        }
        public static Rotate AddFakeRotate(WorldModel Model, int RobotID, Position2D Target, double FakeTeta, int MinTeta, int MaxTeta, kickPowerType kickType, double KickSpeed, bool isChipKick, int gotoPointDelay = 60, bool backSensor = true)
        {
            double Teta = MinTeta;
            if (!rotates.ContainsKey(RobotID))
                Teta = rand.NextDouble() * (MaxTeta - MinTeta) + MinTeta;
            return AddFakeRotate(Model, RobotID, Target, FakeTeta, Teta, kickType, KickSpeed, isChipKick, gotoPointDelay);
        }

        public static void AddKick(int RobotID, kickPowerType type, bool isChip, double power)
        {
            kickdatas[RobotID] = new kickData(type, power, isChip, false);
            BackSensors[RobotID] = false;
        }
        public static void AddKick(int RobotID, kickPowerType type, double power, bool isChip, bool SpinBack)
        {
            kickdatas[RobotID] = new kickData(type, power, isChip, SpinBack, false);
            BackSensors[RobotID] = false;
        }
        public static void AddKick(int RobotID, kickPowerType type, double power, bool SpinKick)
        {
            kickdatas[RobotID] = new kickData(SpinKick, type, power, false);
            BackSensors[RobotID] = false;
        }
        public static void AddKick(int RobotID, bool SpinBack)
        {
            kickdatas[RobotID] = new kickData(SpinBack);
            BackSensors[RobotID] = false;
        }
        public static void AddKick(int RobotID, double KickSpeed, kickPowerType type = kickPowerType.Speed)
        {
            kickdatas[RobotID] = new kickData(type, KickSpeed, false, false);
            BackSensors[RobotID] = false;
        }

        public static void AddBackSensor(int RobotID, bool backSensor)
        {
            BackSensors[RobotID] = backSensor;
        }
        public static void AddCutOhterPath(int RobotID, bool cut)
        {
            CutOtherPaths[RobotID] = cut;
        }
        public static void ChangeDefaulteParams(int RobotID, bool useDefaultparams)
        {
            UseDefultParams[RobotID] = useDefaultparams;
        }

        public static void initialize()
        {
            errtManager = new ERRTManager(8, 100, false);//TODO: change for eight
            controllers = new Dictionary<int, Controller>();
            //ControlParameters.SetParams(
            //defultParams = 
        }

        static SaveModelData smd = new SaveModelData(2, true);

        public static RobotCommands Run(WorldModel Model, out Dictionary<int, Vector2D> lastVel, out Dictionary<int, double> lastOmega)
        {
            initialStates = new Dictionary<int, SingleObjectState>();

            foreach (var item in Model.OurRobots.Keys)
            {
                if (!addedRotates.ContainsKey(item) && rotates.ContainsKey(item))
                {
                    rotates.Remove(item);
                }
                if (!addedRotates.ContainsKey(item) && ReCalculateTeta.ContainsKey(item))
                    ReCalculateTeta.Remove(item);
                if (CommandTypes.ContainsKey(item))
                {
                    if (CommandTypes[item] == CommandType.GotoPointCommand)
                    {
                        if (!controllers.ContainsKey(item))
                            controllers[item] = new Controller();
                        initialStates[item] = Model.OurRobots[item];
                    }
                    else
                    {
                        commands[item] = SWCommands[item];
                        Vector2D v = Vector2D.Zero;
                        double Rotation = Model.OurRobots[item].Angle.Value * Math.PI / 180;
                        v.X = SWCommands[item].Vy * Math.Cos(Rotation) - SWCommands[item].Vx * Math.Sin(Rotation);
                        v.Y = SWCommands[item].Vx * Math.Cos(Rotation) + SWCommands[item].Vy * Math.Sin(Rotation);
                        lastVs[item] = v;
                        lastWs[item] = -SWCommands[item].W;
                        if (ResetControllers.ContainsKey(item) && ResetControllers[item])
                        {
                            controllers[item] = new Controller();
                            lastVs[item] = new Vector2D();
                            lastWs[item] = 0;
                        }
                    }
                }
                if (!CutOtherPaths.ContainsKey(item))
                    CutOtherPaths[item] = true;
            }
            Dictionary<int, List<SingleObjectState>> paths = new Dictionary<int, List<SingleObjectState>>();
            //DrawingObjects.AddObject(new Circle(), "c");

            paths = errtManager.Run(Model, CutOtherPaths, initialStates, goals, initialStates.Keys.ToList(), types, aballs, arobots, azones, aOppzones, false, stopBall);
            //paths.Add(0, new List<SingleObjectState> { new SingleObjectState(new Position2D(1, 1), new Vector2D(), 0), new SingleObjectState(new Position2D(1.0000000000000000001, .999999999999999), new Vector2D(), 0) });
            //Vector2D tmpLastV = Vector2D.Zero;

            foreach (var item in paths.Keys)
            {
                Vector2D lastV = new Vector2D();
                double lastW = 0;
                if (lastVs.ContainsKey(item))
                    lastV = lastVs[item];
                if (lastWs.ContainsKey(item))
                    lastW = lastWs[item];
                //if (item == 2)
                //    tmpLastV = lastV;
                double lastWW = lastW;
                // try
                {
                    commands[item] = controllers[item].CalculateTargetSpeed(Model, item, paths[item][paths[item].Count - 2].Location, (double)goals[item].Angle, paths[item], (UseDefultParams.ContainsKey(item)) ? UseDefultParams[item] : false, ref lastV, ref lastW);
                }

                //catch (Exception e)
                //{

                //    throw;
                //}

                //    Vector2D v = GameParameters.RotateCoordinates(new Vector2D(commands[item].Vx, commands[item].Vy), Model.OurRobots[item].Angle.Value + 100 * lastW.ToDegree() * StaticVariables.FRAME_PERIOD);
                //    v = GameParameters.RotateCoordinates(v, Model.OurRobots[item].Angle.Value);
                //    commands[item].Vx = v.X;
                //    commands[item].Vy = v.Y;
                AlfaList[item] = new Vector2D(controllers[item].aTunner.VelCoef[0], controllers[item].aTunner.VelCoef[1]);
                lastVs[item] = lastV;
                lastWs[item] = lastW;
            }
            #region SaveData
            //WorldModel tmpM = new WorldModel(Model);
            //RobotCommands tmpC = new RobotCommands();
            //tmpC.Commands = commands;
            //if (tmpM.OurRobots.ContainsKey(2))
            //    tmpM.OurRobots[2].Speed = tmpLastV;
            //if (!getaa)
            //{
            //    MRL.SSL.GameDefinitions.General_Settings.TuneVariables.Default.Add("GetData", smd.GetData);
            //    getaa = true;
            //}
            //if (!saveaa)
            //{
            //    MRL.SSL.GameDefinitions.General_Settings.TuneVariables.Default.Add("SaveData", smd.SaveData);
            //    saveaa = true;
            //}
            //bool tmpGet = MRL.SSL.GameDefinitions.General_Settings.TuneVariables.Default.GetValue<bool>("GetData");
            //bool tmpSave = MRL.SSL.GameDefinitions.General_Settings.TuneVariables.Default.GetValue<bool>("SaveData");

            //if (!getAdded)
            //{
            //    smd.GetData = tmpGet;
            //    getAdded = true;

            //}
            //if (!saveAdded)
            //{
            //    smd.SaveData = tmpSave;
            //    saveAdded = true;
            //}


            //if (smd.GetData)
            //{
            //    smd.SSLPacket = null;
            //    smd.Model = tmpM;
            //    smd.Command = tmpC;
            //    smd.Add();
            //    if (smd.SaveData)
            //    {
            //        smd.Save(" " + idx++);
            //        smd.GetData = false;
            //        smd.SaveData = false;
            //    }
            //}

            //if (!tmpGet)
            //{
            //    getAdded = false;
            //}
            //if (!tmpSave)
            //{
            //    saveAdded = false;
            //}
            #endregion
            foreach (var item in Model.OurRobots.Keys)
            {
                //     DrawingObjects.AddObject(new StringDraw(Model.OurRobots[item].Location.toString(), Model.OurRobots[item].Location + new Vector2D(0.5, 0.5)));
                if (commands.ContainsKey(item))
                    commands[item].RobotID = item;
                if (kickdatas.ContainsKey(item))
                {
                    if (commands.ContainsKey(item))
                    {
                        commands[item].isChipKick = kickdatas[item].IsChip;
                        commands[item].isDelayedKick = kickdatas[item].IsSpinKick;
                        commands[item].SpinBack = (kickdatas[item].SpinBack) ? 1 : 0;
                        commands[item].BackSensor = kickdatas[item].BackSensor;
                        if (kickdatas[item].PowerType == kickPowerType.Power)
                            commands[item].KickPower = kickdatas[item].data;
                        else
                            commands[item].KickSpeed = kickdatas[item].data;
                    }
                    else
                    {
                        commands[item] = new SingleWirelessCommand(Vector2D.Zero, 0, kickdatas[item].IsChip, 0, (kickdatas[item].SpinBack) ? 1 : 0, kickdatas[item].IsSpinKick, false);
                        commands[item].RobotID = item;
                        commands[item].BackSensor = kickdatas[item].BackSensor;
                        if (kickdatas[item].PowerType == kickPowerType.Power)
                            commands[item].KickPower = kickdatas[item].data;
                        else
                            commands[item].KickSpeed = kickdatas[item].data;
                    }
                    if (BackSensors.ContainsKey(item))
                        commands[item].BackSensor = BackSensors[item];
                    if (SpinBacks.ContainsKey(item))
                        commands[item].SpinBack = (SpinBacks[item]) ? 1 : 0;
                }
            }
            List<int> mustRemove = new List<int>();

            foreach (var item in lastVs.Keys)
            {
                if (!Model.OurRobots.ContainsKey(item))
                {
                    mustRemove.Add(item);
                    lastVelResetCounter[item]++;
                }
                else
                {
                    lastVelResetCounter[item] = 0;
                }
            }

            foreach (var item in mustRemove)
            {
                if (lastVelResetCounter[item] == 60)
                {
                    lastVs.Remove(item);
                    if (lastWs.ContainsKey(item))
                        lastWs.Remove(item);

                    lastVelResetCounter[item] = 0;
                }
            }
            //Todo: Changed for fixing robot falls 
            //foreach (var item in lastVs.Keys)
            //{
            //    if (!Model.OurRobots.ContainsKey(item))
            //    {
            //        mustRemove.Add(item);
            //    }
            //}

            //foreach (var item in mustRemove)
            //{
            //    lastVs.Remove(item);
            //    if (lastWs.ContainsKey(item))
            //        lastWs.Remove(item);

            //}
            LastCommandTypes = CommandTypes;
            SWCommands = new Dictionary<int, SingleWirelessCommand>();
            CommandTypes = new Dictionary<int, CommandType>();
            RobotCommands robotCommand = new RobotCommands();
            kickdatas = new Dictionary<int, kickData>();
            addedRotates.Clear();
            ResetControllers.Clear();
            robotCommand.Commands = commands;
            commands = new Dictionary<int, SingleWirelessCommand>();
            BackSensors = new Dictionary<int, bool>();
            SpinBacks = new Dictionary<int, bool>();
            UseDefultParams = new Dictionary<int, bool>();
            CutOtherPaths = new Dictionary<int, bool>();
            lastVel = lastVs;
            lastOmega = lastWs;
            return robotCommand;
            //   }
            //   catch { return new RobotCommands(); }
        }
        public static RobotCommands Run(List<WorldModel> Models, out Dictionary<int, Vector2D> lastVel, out Dictionary<int, double> lastOmega)
        {

            initialStates = new Dictionary<int, SingleObjectState>();
            foreach (var model in Models.ToList())
            {
                foreach (var item in model.OurRobots.Keys)
                {
                    if (!addedRotates.ContainsKey(item) && rotates.ContainsKey(item))
                        rotates.Remove(item);

                    if (!addedRotates.ContainsKey(item) && ReCalculateTeta.ContainsKey(item))
                        ReCalculateTeta.Remove(item);

                    if (CommandTypes.ContainsKey(item))
                    {
                        if (CommandTypes[item] == CommandType.GotoPointCommand)
                        {
                            if (!controllers.ContainsKey(item))
                                controllers[item] = new Controller();
                            initialStates[item] = model.OurRobots[item];
                        }
                        else
                        {
                            commands[item] = SWCommands[item];
                            Vector2D v = Vector2D.Zero;
                            double Rotation = model.OurRobots[item].Angle.Value * Math.PI / 180;
                            v.X = SWCommands[item].Vy * Math.Cos(Rotation) - SWCommands[item].Vx * Math.Sin(Rotation);
                            v.Y = SWCommands[item].Vx * Math.Cos(Rotation) + SWCommands[item].Vy * Math.Sin(Rotation);
                            lastVs[item] = v;
                            lastWs[item] = -SWCommands[item].W;
                            if (ResetControllers.ContainsKey(item) && ResetControllers[item])
                            {
                                controllers[item] = new Controller();
                                lastVs[item] = new Vector2D();
                                lastWs[item] = 0;
                            }
                        }
                    }
                    if (!CutOtherPaths.ContainsKey(item))
                        CutOtherPaths[item] = true;
                }
                Dictionary<int, SingleObjectState> tmpGoals = new Dictionary<int, SingleObjectState>();
                Dictionary<int, SingleObjectState> tmpinits = new Dictionary<int, SingleObjectState>();
                Dictionary<int, int> tmpArobots = new Dictionary<int, int>();
                Dictionary<int, int> tmpAzones = new Dictionary<int, int>();
                Dictionary<int, int> tmpAOppzones = new Dictionary<int, int>();
                Dictionary<int, int> tmpAballs = new Dictionary<int, int>();
                Dictionary<int, PathType> tmpTypes = new Dictionary<int, PathType>();



                foreach (var id in initialStates.Keys.ToList())
                {
                    if (model.OurRobots.ContainsKey(id))
                    {
                        tmpGoals[id] = goals[id];
                        tmpinits[id] = initialStates[id];
                        tmpArobots[id] = arobots[id];
                        tmpAzones[id] = azones[id];
                        tmpAOppzones[id] = aOppzones[id];
                        tmpAballs[id] = aballs[id];
                        tmpTypes[id] = types[id];
                    }
                }
                Dictionary<int, List<SingleObjectState>> paths = errtManager.Run(model, CutOtherPaths, tmpinits, tmpGoals, tmpinits.Keys.ToList(), tmpTypes, tmpAballs, tmpArobots, tmpAzones, tmpAOppzones, false, stopBall);


                foreach (var item in paths.Keys)
                {
                    Vector2D lastV = new Vector2D();
                    double lastW = 0;
                    if (lastVs.ContainsKey(item))
                        lastV = lastVs[item];
                    if (lastWs.ContainsKey(item))
                        lastW = lastWs[item];
                    commands[item] = controllers[item].CalculateTargetSpeed(model, item, paths[item][paths[item].Count - 2].Location, (double)goals[item].Angle, paths[item], (UseDefultParams.ContainsKey(item)) ? UseDefultParams[item] : false, ref lastV, ref lastW);
                    lastVs[item] = lastV;
                    lastWs[item] = lastW;
                }


                foreach (var item in model.OurRobots.Keys)
                {
                    if (commands.ContainsKey(item))
                        commands[item].RobotID = item;
                    if (kickdatas.ContainsKey(item))
                    {
                        if (commands.ContainsKey(item))
                        {
                            commands[item].isChipKick = kickdatas[item].IsChip;
                            commands[item].isDelayedKick = kickdatas[item].IsSpinKick;
                            commands[item].SpinBack = (kickdatas[item].SpinBack) ? 1 : 0;
                            commands[item].BackSensor = kickdatas[item].BackSensor;
                            if (kickdatas[item].PowerType == kickPowerType.Power)
                                commands[item].KickPower = kickdatas[item].data;
                            else
                                commands[item].KickSpeed = kickdatas[item].data;
                        }
                        else
                        {
                            commands[item] = new SingleWirelessCommand(Vector2D.Zero, 0, kickdatas[item].IsChip, 0, (kickdatas[item].SpinBack) ? 1 : 0, kickdatas[item].IsSpinKick, false);
                            commands[item].RobotID = item;
                            commands[item].BackSensor = kickdatas[item].BackSensor;
                            if (kickdatas[item].PowerType == kickPowerType.Power)
                                commands[item].KickPower = kickdatas[item].data;
                            else
                                commands[item].KickSpeed = kickdatas[item].data;
                        }
                        if (BackSensors.ContainsKey(item))
                            commands[item].BackSensor = BackSensors[item];
                        if (SpinBacks.ContainsKey(item))
                            commands[item].SpinBack = (SpinBacks[item]) ? 1 : 0;
                    }
                }
            }
            LastCommandTypes = CommandTypes;
            SWCommands = new Dictionary<int, SingleWirelessCommand>();
            CommandTypes = new Dictionary<int, CommandType>();
            RobotCommands robotCommand = new RobotCommands();
            kickdatas = new Dictionary<int, kickData>();
            addedRotates.Clear();
            ResetControllers.Clear();
            robotCommand.Commands = commands;
            commands = new Dictionary<int, SingleWirelessCommand>();
            BackSensors = new Dictionary<int, bool>();
            SpinBacks = new Dictionary<int, bool>();
            CutOtherPaths = new Dictionary<int, bool>();
            UseDefultParams = new Dictionary<int, bool>();

            lastVel = lastVs;
            lastOmega = lastWs;
            return robotCommand;
            //   }
            //   catch { return new RobotCommands(); }
        }
        public static void ShutDown()
        {
            errtManager.Dispose();
        }



        public static bool PassIntersectCheck(WorldModel Model, Position2D PassPoint)
        {
            Vector2D BallTargetVec = PassPoint - Model.BallState.Location;
            List<Vector2D> oppSpeeds = new List<Vector2D>();
            double k = 0.1;
            bool hasInters = false;
            foreach (var item in Model.Opponents.Take(4))
            {
                Vector2D v = k * item.Value.Speed;
                oppSpeeds.Add(v);
                Position2D? inter = null;
                Line BallTargetLine = new Line(Model.BallState.Location, Model.BallState.Location + BallTargetVec);
                Line opSpeed = new Line(item.Value.Location, item.Value.Location + v);
                //inter = Vector2D.Intersect(Model.BallState.Location, BallTargetVec, item.Value.Location, v);
                inter = BallTargetLine.IntersectWithLine(opSpeed);
                if (inter.HasValue && (inter.Value - item.Value.Location).InnerProduct(v) >= 0)
                {
                    Vector2D vec = inter.Value - item.Value.Location;
                    //DrawingObjects.AddObject(new Circle(inter.Value, 0.05) { DrawPen = new Pen(Color.Red, 0.01f) });
                    if (vec.Size <= v.Size || inter.Value.DistanceFrom(item.Value.Location) < 0.3)
                    {
                        hasInters = true;
                        break;
                    }
                }

                else
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + BallTargetVec, Vector2D.Zero, 0), 0.3))
                        hasInters = true;
                }

            }
            return (!hasInters);

        }
    }
    public enum CommandType
    {
        WirelessCommand,
        GotoPointCommand
    }
    public struct kickData
    {
        public kickData(kickPowerType powertype, double power, bool isChip, bool spinback, bool backsensor)
        {
            IsChip = isChip;
            IsSpinKick = false;
            PowerType = powertype;
            data = power;
            SpinBack = spinback;
            BackSensor = backsensor;
        }
        public kickData(kickPowerType powertype, double power, bool isChip, bool backsensor)
        {
            SpinBack = false;
            data = power;
            IsChip = isChip;
            IsSpinKick = false;
            PowerType = powertype;
            BackSensor = backsensor;
        }
        public kickData(bool isSpinKick, kickPowerType powertype, double power, bool backsensor)
        {
            SpinBack = false;
            data = power;
            IsChip = false;
            IsSpinKick = isSpinKick;
            PowerType = powertype;
            BackSensor = backsensor;
        }
        public kickData(bool spinback)
        {
            SpinBack = spinback;
            PowerType = kickPowerType.Power;
            data = 0;
            IsChip = false;
            IsSpinKick = false;
            BackSensor = false;
        }
        public bool BackSensor;
        public bool SpinBack;
        public bool IsChip;
        public bool IsSpinKick;
        public kickPowerType PowerType;
        public double data;
    }
    public enum kickPowerType
    {
        Power,
        Speed
    }
}
