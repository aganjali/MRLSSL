using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Plays
{
    public class PassShootTune : PlayBase
    {
        public static bool ActiveFlag = false;
        public static int PasserID = 0;
        public static int ShooterID = 0;
        public static double shootSpeed = 8;
        public static double passSpeed = 4;
        public static double shooterDistance = 0;//
        public static double Shoota = 0;
        public static double Passa = 0;
        public static double Shootb = 0;
        public static double Passb = 0;
        public static bool start = false;
        public static bool test = false;
        public static bool Acceptdata = false;
        static bool onceatime = false;
        static bool firsttime = true;
        static bool arrived = false;
        static double realspeed = 0;
        static double shootspeed = 0;
        static Position2D lastBallState = new Position2D();
        static Position2D lastBallStateSecond = new Position2D();
        static List<Position2D> Passposes = new List<Position2D>();
        static List<Position2D> Shootposes = new List<Position2D>();
        static bool endPass = false;
        static bool startShoot = false;
        static double lambda = 0;
        static double beta = 0;
        static Position2D initialpos = new Position2D();
        static bool onceBallSave = true;
        static List<Vector2D> aCoefs = new List<Vector2D>();
        static List<Vector2D> bCoefs = new List<Vector2D>();
        static List<Vector2D> cCoefs = new List<Vector2D>();
        static Vector2D PassVector = new Vector2D();
        static Vector2D ShootVector = new Vector2D();
        static bool endTune = false;
        static List<double> RobotAngle = new List<double>();
        static double RobotArriveBallangle = 0;
        private static int counter = 0;
        static DrawCollection fg = new DrawCollection();
        static DrawCollection fg2 = new DrawCollection();
        private double distance_Shooter_from_BAll = 0;
        static double Ydistance = .1;
        private static int counter2 = 0;
        private static int counter3 = 0;
        private static bool startcount = false;
        private static bool clear = true;
        private static Position2D robotpos = new Position2D();
        private static bool firsttimeRobotPos = true;

        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            if (Model.Status == GameDefinitions.GameStatus.PassShootTune)
            {
                return true;
            }
            return false;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            if (firsttime)
            {
                firsttime = false;
                lastBallState = Model.BallState.Location;
                initialpos = Model.OurRobots[ShooterID].Location;
                distance_Shooter_from_BAll = Model.OurRobots[ShooterID].Location.DistanceFrom(Model.BallState.Location);
            }
            Acceptdata = PassShootParameter.AcceptData;
            Ydistance = PassShootParameter.YDistance;
            shootSpeed = PassShootParameter.shootSpeed;
            passSpeed = PassShootParameter.passSpeed;
            PasserID = PassShootParameter.PasserID;
            ShooterID = PassShootParameter.ShooterID;
            shooterDistance = PassShootParameter.shooterDistance;
            start = PassShootParameter.start;
            test = PassShootParameter.test;
            if (Model.BallState.Location.DistanceFrom(lastBallState) > .02)
            {
                counter2++;
            }
            if(Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location + Vector2D.FromAngleSize(Model.OurRobots[ShooterID].Angle.Value*Math.PI/180 , .1))<.03)
            {
                startcount = true;
            }
            if (startcount)
            {
                counter3++;
            }

            if (counter2 == 10)
            {
                    #region Drawing for Tool
                realspeed = Model.BallState.Speed.Size + (double)((double)(10 * 5) / 60);
                #endregion
            }
            if(counter3 ==10)
            {
                shootspeed = Model.BallState.Speed.Size + (double)((double)(10 * 5) / 60);
            }
            DrawingObjects.AddObject(new StringDraw("Pass Velocity Real: " + realspeed, Model.BallState.Location.Extend(-.7, 0)), "5454648454654");
            DrawingObjects.AddObject(new StringDraw("Ball Current Velocity: " + Model.BallState.Speed.Size, Model.BallState.Location.Extend(-.5, 0)), "654654656");
            DrawingObjects.AddObject(new StringDraw("Shoot Velocity Real: " + shootspeed, Model.BallState.Location.Extend(-.6, 0)), "5454697889");


            //DrawingObjects.AddObject(new Line(Model.OurRobots[PasserID].Location, Model.OurRobots[PasserID].Location + Vector2D.FromAngleSize(Model.OurRobots[PasserID].Angle.Value * Math.PI / 180, 2)),"54546454647");

            Position2D target = Model.BallState.Location + new Vector2D(0, -Math.Sign(Model.BallState.Location.Y) * shooterDistance);

            PassShootParameter.distancepercent = 100 - (Model.OurRobots[ShooterID].Location.DistanceFrom(target) * 100 / initialpos.DistanceFrom(target));

            double extendSize = .15;

            Position2D RobotPassPoint = Model.OurRobots[ShooterID].Location + Vector2D.FromAngleSize(Model.OurRobots[ShooterID].Angle.Value * Math.PI / 180, extendSize);
            Position2D desirepos = Model.BallState.Location + new Vector2D(0, -1 * (Math.Sign(Model.BallState.Location.Y) * shooterDistance));

            desirepos = (Math.Abs(desirepos.Y) > 2) ? desirepos + new Vector2D(0, -Math.Sign(desirepos.Y) * Math.Abs(desirepos.Y - 1.9)) : desirepos;
            desirepos = (desirepos + (Model.OurRobots[ShooterID].Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(.15)).Extend(Ydistance, 0);
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(desirepos) < .01)
            {
                arrived = true;
            }
            if (arrived && start && !endTune )//&& !Acceptdata)
            {
                if (firsttimeRobotPos)
                {
                    firsttimeRobotPos = true;
                    robotpos = RobotPassPoint;
                }
                if (Model.BallState.Speed.Size > .5)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(OneTouchRole)))
                        Functions[ShooterID] = (eng, wmd) => GetRole<OneTouchRole>(ShooterID).Perform(eng, wmd, ShooterID, Model.OurRobots[PasserID], false, GameParameters.OppGoalCenter, shootSpeed, false, false, 0, 0, passSpeed);
                }
                if (Model.BallState.Speed.Size < .4)
                {
                    Planner.AddRotate(Model, PasserID, robotpos, 0, kickPowerType.Speed, passSpeed, false);
                }
            }
            else if(!endTune )//&&!Acceptdata)
            {
                Planner.Add(ShooterID, desirepos, (GameParameters.OppGoalCenter - Model.OurRobots[ShooterID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
            }
            else
            {
                Planner.Add(ShooterID, Model.OurRobots[ShooterID].Location, (GameParameters.OppGoalCenter - Model.OurRobots[ShooterID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                Planner.Add(PasserID, Model.OurRobots[PasserID].Location, (GameParameters.OppGoalCenter - Model.OurRobots[ShooterID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
            }
            if (!endTune && arrived && start)
            {
                if (Model.BallState.Location.DistanceFrom(lastBallState) > 1 && Model.BallState.Location.DistanceFrom(lastBallState) < 2)
                {
                    Passposes.Add(Model.BallState.Location);
                }
                if (Model.BallState.Location.DistanceFrom(lastBallState) > 2)
                {
                    endPass = true;
                }
                if (endPass && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < .3)
                {
                    onceatime = true;
                    if ((Model.OurRobots[ShooterID].Location + Vector2D.FromAngleSize(Model.OurRobots[ShooterID].Angle.Value * Math.PI / 180, .09)).DistanceFrom(Model.BallState.Location) < .1)
                    {
                        RobotArriveBallangle = Model.OurRobots[ShooterID].Angle.Value;
                    }
                    RobotAngle.Add(Model.OurRobots[ShooterID].Angle.Value);
                    startShoot = true;
                }
                if (startShoot && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > .3)
                {
                    if (onceBallSave)
                    {
                        onceBallSave = false;
                        lastBallStateSecond = Model.BallState.Location;
                    }
                    if (Model.BallState.Location.DistanceFrom(lastBallStateSecond) > .4 && Model.BallState.Location.DistanceFrom(lastBallStateSecond) < 1)
                    {
                        Shootposes.Add(Model.BallState.Location);
                    }
                    if (Model.BallState.Location.DistanceFrom(lastBallStateSecond) > 1.4 && onceatime)
                    {
                        onceatime = false;
                        endTune = true;
                    }
                }
            }

            if (PassShootParameter.clear)
            {
                cCoefs.Clear();
                bCoefs.Clear();
                aCoefs.Clear();

            }


            if (endTune )//&& Acceptdata)
            {
                counter++;
                endTune = false;
                #region Caculate Pass vector
                List<double> xPass = Passposes.Select(o => o.X).ToList();
                List<double> yPass = Passposes.Select(o => o.Y).ToList();
                MathMatrix A = new MathMatrix(Passposes.Count, 2);
                for (int i = 0 ; i < Passposes.Count ; i++)
                {
                    A[i, 0] = 1;
                    A[i, 1] = Passposes[i].X;
                }
                MathMatrix B = new MathMatrix(Passposes.Count, 1);
                for (int i = 0 ; i < Passposes.Count ; i++)
                {
                    B[i, 0] = Passposes[i].Y;
                }
                MathMatrix X = new MathMatrix(2, 1);

                X = ((A.Transpose * A).Inverse * A.Transpose) * B;
                double b = X[0, 0];
                double a = X[1, 0];

                Line PassLine = new Line(0, a, b);
                Position2D shooterPos = Model.OurRobots[ShooterID].Location;
                Position2D passerPos = Model.OurRobots[PasserID].Location;
                Position2D tailofPass = new Position2D(shooterPos.X, (a * shooterPos.X) + b);
                Position2D headofPass = new Position2D(passerPos.X, (a * passerPos.X) + b);

                #endregion
                PassVector = tailofPass - headofPass;
                PassVector.NormalizeTo(passSpeed);
                #region PassSpeedCalculator
                double passSpeedPhase2 = passSpeed * 5 / 7;
                double ballAccelPhase1 = -5;
                double ballAccelPhase2 = -0.3;
                double dxPhase1 = 0;
                dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - passSpeed * passSpeed) / (2 * ballAccelPhase1);
                double dxPhase2 = Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) - dxPhase1;
                double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
                double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;
                PassVector.NormalizeTo(vf);
                #endregion
                #region Calculate Shoot Vector
                MathMatrix As = new MathMatrix(Shootposes.Count, 2);
                for (int i = 0 ; i < Shootposes.Count ; i++)
                {
                    As[i, 0] = 1;
                    As[i, 1] = Shootposes[i].X;
                }
                MathMatrix Bs = new MathMatrix(Shootposes.Count, 1);
                for (int i = 0 ; i < Shootposes.Count ; i++)
                {
                    Bs[i, 0] = Shootposes[i].Y;
                }
                MathMatrix Xs = new MathMatrix(2, 1);

                Xs = ((As.Transpose * As).Inverse * As.Transpose) * Bs;
                double bs = Xs[0, 0];
                double aS = Xs[1, 0];


                Position2D tailofShoot = new Position2D(GameParameters.OppGoalCenter.X, (aS * GameParameters.OppGoalCenter.X) + bs);
                Position2D headofShoot = new Position2D(Model.OurRobots[ShooterID].Location.X, (aS * Model.OurRobots[ShooterID].Location.X) + bs);
                ShootVector = tailofShoot - headofShoot;
                ShootVector.NormalizeTo(shootspeed);

                PassShootParameter.Passa = a;
                PassShootParameter.Passb = b;
                PassShootParameter.Shoota = aS;
                PassShootParameter.Shootb = bs;
                #endregion

                double robotangle = RobotArriveBallangle;//RobotAngle.Average();
                Vector2D Rh = new Vector2D(Math.Cos(robotangle.ToRadian()), Math.Sin(robotangle.ToRadian()));
                Vector2D Rp = new Vector2D(-Math.Sin(robotangle.ToRadian()), Math.Cos(robotangle.ToRadian()));
                //Vector2D O = shootSpeed * Rh;
                //Vector2D M = Rp.InnerProduct(PassVector) * Rp;//Beta coef
                //Vector2D N = PassVector - (2 * Rh).InnerProduct(PassVector) * Rh;//lembda coef

                Vector2D O = ShootVector;
                Vector2D M = Rp.InnerProduct(PassVector) * Rp;
                Vector2D N = (shootSpeed * Rh) + (Rh.InnerProduct(PassVector) * Rh);
                cCoefs.Add(O);
                bCoefs.Add(N);
                aCoefs.Add(M);


                MathMatrix matrixO = new MathMatrix(2 * cCoefs.Count, 1);
                MathMatrix matrixN = new MathMatrix(2 * bCoefs.Count, 1);
                MathMatrix matrixM = new MathMatrix(2 * aCoefs.Count, 1);
                MathMatrix MatrixMN = new MathMatrix(2 * cCoefs.Count, 2);
                for (int j = 0 ; j < 2 ; j++)
                {
                    for (int i = 0 ; i < cCoefs.Count ; i++)
                    {

                        if (j == 0)
                        {
                            matrixO[i, 0] = cCoefs[i].X;
                            matrixN[i, 0] = bCoefs[i].X;
                            matrixM[i, 0] = aCoefs[i].X;

                        }
                        if (j == 1)
                        {
                            matrixO[i + (cCoefs.Count), 0] = cCoefs[i].Y;
                            matrixN[i + (cCoefs.Count), 0] = bCoefs[i].Y;
                            matrixM[i + (cCoefs.Count), 0] = aCoefs[i].Y;
                        }

                    }
                }
                for (int j = 0 ; j < 2 ; j++)
                {
                    for (int i = 0 ; i < 2 * cCoefs.Count ; i++)
                    {
                        if (j == 0)
                            MatrixMN[i, j] = matrixM[i, 0];
                        if (j == 1)
                            MatrixMN[i, j] = matrixN[i, 0];
                    }
                }
                MathMatrix results = ((MatrixMN.Transpose * MatrixMN).Inverse * MatrixMN.Transpose) * matrixO;
                beta = results[0, 0];
                lambda = results[1, 0];
                //if (aCoefs.Count > 1)
                //{

                //    Vector2D a1 = aCoefs.LastOrDefault();
                //    Vector2D b1 = bCoefs.LastOrDefault();
                //    Vector2D e1 = cCoefs.LastOrDefault();
                //    Vector2D c1 = aCoefs[aCoefs.Count - 2];
                //    Vector2D d1 = bCoefs[bCoefs.Count - 2];
                //    Vector2D f1 = cCoefs[cCoefs.Count - 2];
                //    lambda = (d1.InnerProduct(e1) - b1.InnerProduct(f1)) / (a1.InnerProduct(d1) - b1.InnerProduct(c1));
                //    beta = (a1.InnerProduct(f1) - c1.InnerProduct(e1)) / (a1.InnerProduct(d1) - b1.InnerProduct(c1));

                //}

            }
            if (arrived && test)
            {
                if (Model.BallState.Speed.Size > .5)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(OneTouchRole)))
                        Functions[ShooterID] = (eng, wmd) => GetRole<OneTouchRole>(ShooterID).Perform(eng, wmd, ShooterID, Model.OurRobots[PasserID], false, GameParameters.OppGoalCenter, shootSpeed, false, true, 0.33484161129381196, 0.79767488714666024, passSpeed);
                }
                Planner.AddRotate(Model, PasserID, RobotPassPoint, 0, kickPowerType.Speed, passSpeed, false);
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            fg = new DrawCollection();
            fg2 = new DrawCollection();
            arrived = false;
            endTune = false;
            firsttime = true;
            onceatime = false;
            onceBallSave = true;
            startShoot = false;
            Shootposes = new List<Position2D>();
            Passposes = new List<Position2D>();
            RobotArriveBallangle = 0;
            counter2 = 0;
            startcount = false;
            counter3 = 0;
            firsttimeRobotPos = true;
        }
    }
}
