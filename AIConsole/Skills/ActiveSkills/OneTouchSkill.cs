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
    public class OneTouchSkill : SkillBase
    {
        #region NewPerform
        const bool debug = true;
        int dataCount = 15;
        Dictionary<int, Position2D> ballHistoryList = new Dictionary<int, Position2D>();
        int index = 0;
        double angleTresh = 65;
        #endregion

        bool ballGet = false;
        bool robotGet = false;

        public OneTouchSkill()
        {
            //  Controller = new Control.Controller();

        }
        bool Firsttime = true;
        Line ballLine = null;
        List<Position2D> intersects = new List<Position2D>();
        Position2D minimum;
        SingleObjectState robot = new SingleObjectState();
        bool firstGetRobotLoc = true;
        public void Reset()
        {
            angleTresh = 75;
            //dataCount = 2;
            ballHistoryList = new Dictionary<int, Position2D>();
            index = 0;
            ballGet = false;
            robotGet = false;
            ballLine = null;
            intersects = new List<Position2D>();
            robot = new SingleObjectState();
        }

        int count = 0, count2 = 0;
        bool firstDirect = true, firstChip = true;
        Line BallLine = new Line();
        Line PrepLine = new Line();
        double sumang = 0;
        double lastang = 0;
        Queue<double> angleQ = new Queue<double>();
        public Position2D OneTouchPoint = Position2D.Zero;
        public double worldangle = 0;
        private bool Firsttime2 = true;
        double distance_Shooter_from_BAll = 0;
        Position2D firstBallPos = Position2D.Zero;
        Position2D lastExtendedIntersect = Position2D.Zero;
        Position2D lastIntersect = Position2D.Zero;
        double chipRadious = 1.5;
        double fieldMargin = -0.12;
        bool useCMU = false;
        double normlizeD = 0.095;
        OneTouchMode otMode = OneTouchMode.Random;
        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID, SingleObjectState PasserState, bool passIsChip, Position2D Target, double KickPower, bool isChip, bool correctang, double PassSpeed = 0)
        {
            double Angle = 0;
            Position2D extendedIntersect;
            if (passIsChip)
            {
                extendedIntersect = GetChipPass(engine, Model, robotID, PasserState, Target);
                Angle = (Target - extendedIntersect).AngleInDegrees;
            }
            else
            {
                //extendedIntersect = GetDirectPass2(engine, Model, robotID, Target);
                extendedIntersect = GetDirectPass2New(engine, Model, Model.OurRobots[robotID], Target, otMode);
                Angle = (Target - extendedIntersect).AngleInDegrees; ;// AngleCorrection(Model, true, KickPower, Target, extendedIntersect + (Target - extendedIntersect).GetNormalizeToCopy(normlizeD), robotID, PassSpeed, true);
            }
            Position2D tmpTarget = extendedIntersect + Vector2D.FromAngleSize(Angle.ToRadian(), 1);
            extendedIntersect = extendedIntersect - (tmpTarget - extendedIntersect).GetNormalizeToCopy(normlizeD);

            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
            DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((tmpTarget - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");

            OneTouchPoint = extendedIntersect;

            Planner.Add(robotID, extendedIntersect, Angle, true);
            Planner.AddKick(robotID, kickPowerType.Speed, isChip, KickPower);
            return;
        }

        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID, SingleObjectState PasserState, bool passIsChip, Position2D Target, double KickPower, bool isChip, double PassSpeed = 0)
        {
            double Angle = 0;
            Position2D extendedIntersect;
            if (passIsChip)
            {
                extendedIntersect = GetChipPass(engine, Model, robotID, PasserState, Target);
                Angle = (Target - extendedIntersect).AngleInDegrees;


            }
            else
            {
                // extendedIntersect = GetDirectPass2(engine, Model, robotID, Target);
                extendedIntersect = GetDirectPass2New(engine, Model, Model.OurRobots[robotID], Target, otMode);

                if (!isChip)
                    Angle = AngleCorrection(Model, useCMU, KickPower, Target, extendedIntersect /*+ (Target - extendedIntersect).GetNormalizeToCopy(normlizeD)*/, robotID, PassSpeed, true);
                else
                    Angle = AngleCorrection(Model, useCMU, 8, Target, extendedIntersect /*+ (Target - extendedIntersect).GetNormalizeToCopy(normlizeD)*/, robotID, PassSpeed, true);

            }
            Position2D tmpTarget = extendedIntersect + Vector2D.FromAngleSize(Angle.ToRadian(), 1);
            extendedIntersect = extendedIntersect - (tmpTarget - extendedIntersect).GetNormalizeToCopy(normlizeD);

            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
            DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((tmpTarget - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");

            OneTouchPoint = extendedIntersect;

            Planner.Add(robotID, extendedIntersect, Angle, true);
            Planner.AddKick(robotID, kickPowerType.Speed, isChip, KickPower);
            return;
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState RobotState, SingleObjectState PasserState, bool passIsChip, Position2D Target, double KickPower, bool isChip, bool GotoPoint, double PassSpeed = 0)
        {
            double Angle = 0;
            Position2D extendedIntersect;
            if (passIsChip)
            {
                extendedIntersect = GetChipPass(engine, Model, RobotState, PasserState, Target);
                Angle = (Target - extendedIntersect).AngleInDegrees;
            }
            else
            {
                //      extendedIntersect = GetDirectPass2(engine, Model, RobotState, Target);
                extendedIntersect = GetDirectPass2New(engine, Model, RobotState, Target, otMode);
                Angle = AngleCorrection(Model, useCMU, KickPower, Target, extendedIntersect /*+ (Target - extendedIntersect).GetNormalizeToCopy(normlizeD)*/, RobotID, PassSpeed, true);
            }

            Position2D tmpTarget = extendedIntersect + Vector2D.FromAngleSize(Angle.ToRadian(), 1);
            extendedIntersect = extendedIntersect - (tmpTarget - extendedIntersect).GetNormalizeToCopy(normlizeD);

            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
            DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((tmpTarget - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");

            OneTouchPoint = extendedIntersect;
            if (GotoPoint)
            {
                Planner.Add(RobotID, extendedIntersect, Angle, true);
                Planner.AddKick(RobotID, kickPowerType.Speed, isChip, KickPower);
            }
            return;
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, bool avoidRobots, SingleObjectState RobotState, SingleObjectState PasserState, bool passIsChip, Position2D Target, double KickPower, bool isChip, bool GotoPoint, double AngleTresh, double PassSpeed = 0)
        {
            double Angle = 0;
            Position2D extendedIntersect;
            if (passIsChip)
            {
                extendedIntersect = GetChipPass(engine, Model, RobotState, PasserState, Target, AngleTresh);
                Angle = (Target - extendedIntersect).AngleInDegrees;
            }
            else
            {
                // extendedIntersect = GetDirectPass2(engine, Model, RobotState, Target);
                extendedIntersect = GetDirectPass2New(engine, Model, RobotState, Target, otMode);
                Angle = AngleCorrection(Model, useCMU, KickPower, Target, extendedIntersect/* + (Target - extendedIntersect).GetNormalizeToCopy(normlizeD)*/, RobotID, PassSpeed, true);
            }
            Position2D tmpTarget = extendedIntersect + Vector2D.FromAngleSize(Angle.ToRadian(), 1);
            extendedIntersect = extendedIntersect - (tmpTarget - extendedIntersect).GetNormalizeToCopy(normlizeD);

            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
            DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((tmpTarget - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");

            OneTouchPoint = extendedIntersect;
            if (GotoPoint)
            {
                Planner.Add(RobotID, extendedIntersect, Angle, PathType.UnSafe, false, avoidRobots, true, true);
                Planner.AddKick(RobotID, kickPowerType.Speed, isChip, KickPower);
            }
            return;
        }

        public void PerformActive(GameStrategyEngine engine, WorldModel Model, int robotID, SingleObjectState RobotState, SingleObjectState PasserState, bool passIsChip, Position2D Target, double KickPower, bool isChip, OneTouchMode mode = OneTouchMode.Normal, double passSpeed = 0)
        {
            double Angle = 0;
            Position2D extendedIntersect;
            //dataCount = 10;
             //normlizeD = 0.08;
            if (passIsChip)
            {
                extendedIntersect = GetChipPass(engine, Model, robotID, PasserState, Target);
                Angle = (Target - extendedIntersect).AngleInDegrees;
            }
            else
            {
                extendedIntersect = GetDirectPass2(engine, Model, RobotState, Target);
                Angle = AngleCorrection(Model, useCMU, KickPower, Target, extendedIntersect + (Target - extendedIntersect).GetNormalizeToCopy(normlizeD), robotID, passSpeed, false);
            }
            Position2D tmpTarget = extendedIntersect + Vector2D.FromAngleSize(Angle.ToRadian(), 1);
            extendedIntersect = extendedIntersect - (tmpTarget - extendedIntersect).GetNormalizeToCopy(normlizeD);
            if (debug)
            {
                DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
                DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((tmpTarget - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
                DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
            }

            OneTouchPoint = extendedIntersect;
            //Planner.ChangeDefaulteParams(robotID, false);
            //Planner.SetParameter(robotID, 10, 4);
            Planner.Add(robotID, extendedIntersect, Angle, true);
            Planner.AddKick(robotID, kickPowerType.Speed, isChip, KickPower);
            return;
        }


        //---------------------------------Pass Shoot Tune Tool
        public void PerformManualcoefs(GameStrategyEngine engine, WorldModel Model, int robotID, SingleObjectState PasserState, bool passIsChip, Position2D Target, double KickPower, double beta, double lambda, bool isChip, double PassSpeed)
        {
            double Angle = 0;
            Position2D extendedIntersect;
            if (passIsChip)
            {
                extendedIntersect = GetChipPass(engine, Model, robotID, PasserState, Target);
                Angle = (Target - extendedIntersect).AngleInDegrees;
            }
            else
            {

                extendedIntersect = GetDirectPass2New(engine, Model, Model.OurRobots[robotID], Target, OneTouchMode.Normal);
                if (!isChip)
                    Angle = AngleCorrectionManualCoef(Model, false, KickPower, Target, extendedIntersect + (Target - extendedIntersect).GetNormalizeToCopy(normlizeD), robotID, PassSpeed, beta, lambda, true);
                else
                    Angle = AngleCorrection(Model, useCMU, KickPower, Target, extendedIntersect + (Target - extendedIntersect).GetNormalizeToCopy(normlizeD), robotID, PassSpeed, true);

            }
            OneTouchPoint = extendedIntersect;
            Position2D tmpTarget = extendedIntersect + Vector2D.FromAngleSize(Angle.ToRadian(), 1);
            extendedIntersect = extendedIntersect - (tmpTarget - extendedIntersect).GetNormalizeToCopy(normlizeD);
            double ang = 0;
            if (Model.BallState.Speed.Size > .2)
            {
                ang = Angle;
            }
            else
            {
                ang = (GameParameters.OppGoalCenter - Model.OurRobots[robotID].Location).AngleInDegrees;
            }

            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
            #region Drawing for Tool
            DrawingObjects.AddObject(new StringDraw("Distance to target: " + Model.OurRobots[robotID].Location.DistanceFrom(extendedIntersect).ToString(), Model.OurRobots[robotID].Location.Extend(+.3, 0)), "547987987878");
            DrawingObjects.AddObject(new StringDraw("DiferenceAngle: " + Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(Model.OurRobots[robotID].Angle.Value * Math.PI / 180, 1), Vector2D.FromAngleSize(Angle.ToRadian(), 1))), Model.OurRobots[robotID].Location.Extend(.4, 0)), "6889798787987");
            #endregion
            Planner.Add(robotID, extendedIntersect, Angle, true);
            Planner.AddKick(robotID, kickPowerType.Speed, isChip, KickPower);
            return;
        }
        public void PerformWithoutCorrection(GameStrategyEngine engine, WorldModel Model, int robotID, SingleObjectState PasserState, bool passIsChip, Position2D Target, double KickPower, bool isChip, double PassSpeed)
        {
            double Angle = 0;
            Position2D extendedIntersect;
            if (passIsChip)
            {
                extendedIntersect = GetChipPass(engine, Model, robotID, PasserState, Target);
                Angle = (Target - extendedIntersect).AngleInDegrees;
            }
            else
            {
                extendedIntersect = GetDirectPass2New(engine, Model, Model.OurRobots[robotID], Target, OneTouchMode.Normal);
                Angle = (Target - extendedIntersect).AngleInDegrees;
            }
            OneTouchPoint = extendedIntersect;
            Position2D tmpTarget = extendedIntersect + Vector2D.FromAngleSize(Angle.ToRadian(), 1);
            extendedIntersect = extendedIntersect - (tmpTarget - extendedIntersect).GetNormalizeToCopy(normlizeD);
            double ang = 0;
            if (Model.BallState.Speed.Size > .2)
            {
                ang = Angle;
            }
            else
            {
                ang = (GameParameters.OppGoalCenter - Model.OurRobots[robotID].Location).AngleInDegrees;
            }

            #region Drawing for Tool
            DrawingObjects.AddObject(new StringDraw("Distance to target: " + Model.OurRobots[robotID].Location.DistanceFrom(extendedIntersect).ToString(), Model.OurRobots[robotID].Location.Extend(+.3, 0)), "547987987878");
            DrawingObjects.AddObject(new StringDraw("DiferenceAngle: " + Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(Model.OurRobots[robotID].Angle.Value * Math.PI / 180, 1), Vector2D.FromAngleSize(Angle.ToRadian(), 1))), Model.OurRobots[robotID].Location.Extend(.4, 0)), "6889798787987");
            #endregion
            DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");

            Planner.Add(robotID, extendedIntersect, ang, true);
            //Planner.AddBackSensor(robotID, true);
            Planner.AddKick(robotID, kickPowerType.Speed, KickPower, isChip, true);
        }

        public double AngleCorrectionManualCoef(WorldModel Model, bool CMUFurmule, double ShootSpeed, Position2D Target, Position2D ShooterPos, int shooterID, double PassSpeed, double beta, double lambda, bool Debug)
        {

            Vector2D V1 = Vector2D.Zero;
            Vector2D V0 = Vector2D.Zero;
            PassSpeed = Math.Max(0, PassSpeed);
            double Shootspeed = Math.Min(ShootSpeed, Program.MaxKickSpeed);
            Vector2D Rp = Vector2D.Zero;
            Vector2D Rh = Vector2D.Zero;
            if (Firsttime2)
            {
                distance_Shooter_from_BAll = ShooterPos.DistanceFrom(Model.BallState.Location);
                Firsttime2 = false;
            }
            //  double ballDeccel = (-0.1341 * PassSpeed * PassSpeed) + (1.4299 * PassSpeed) - 0.4284;
            //double FinalV = Math.Sqrt(Math.Abs(Math.Pow(PassSpeed, 2) - (2 * ballDeccel * distance_Shooter_from_BAll)));
            //FinalV = Math.Max(FinalV, 0);
            double passSpeedPhase2 = PassSpeed * 5 / 7;
            double ballAccelPhase1 = -5;
            double ballAccelPhase2 = -0.3;
            double dxPhase1 = 0;
            dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
            double dxPhase2 = distance_Shooter_from_BAll - dxPhase1;//2014
            double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
            double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;

            V0 = Model.BallState.Speed;
            V0.NormalizeTo(vf);
            Vector2D TargetVector = Target - ShooterPos;
            double Alfa = TargetVector.AngleInRadians;
            double CorrectAngle = 100;
            int counter = 0;
            while (Math.Abs(CorrectAngle) > .001 && counter < 200)
            {
                Rh = new Vector2D(Math.Cos(Alfa), Math.Sin(Alfa));
                Rp = new Vector2D(-Math.Sin(Alfa), Math.Cos(Alfa));
                V1 = FormulaManualcoef(ShootSpeed, V0, Rp, Rh, TargetVector, CMUFurmule, ShooterPos, beta, lambda, Debug);
                CorrectAngle = Vector2D.AngleBetweenInRadians(TargetVector, V1);
                Alfa = Alfa + CorrectAngle;
                counter++;
            }
            if (Math.Abs(CorrectAngle) > 0.08)
                Alfa = (Target - ShooterPos).AngleInRadians;
            Position2D Robotaim = ShooterPos + Vector2D.FromAngleSize((Model.OurRobots[shooterID].Angle.Value * Math.PI) / 180, 10);
            #region Draw
            //Debug = true;
            if (Debug == true)
            {
                DrawingObjects.AddObject(new Line(ShooterPos, Robotaim)
                {
                    DrawPen = new Pen(Color.BlueViolet, 0.01f)
                }, "RCC");
                DrawingObjects.AddObject(new StringDraw("Correct Angle:" + CorrectAngle.ToString(), new Position2D(2.8, 0)));
                DrawingObjects.AddObject(new StringDraw("Correct Counter:" + counter.ToString(), new Position2D(2.5, 0)));
                DrawingObjects.AddObject(new StringDraw("VHope: " + vf.ToString(), Model.BallState.Location.Extend(-.2, 0)), "VHope");
                DrawingObjects.AddObject(new StringDraw("VBall: " + Model.BallState.Speed.Size.ToString(), Model.BallState.Location.Extend(-.4, 0)), "VBall");
                // DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2)) { DrawPen = new Pen(Color.Brown, 0.01f) }, "RobotAngleVec");
            }
            #endregion
            return Alfa * 180 / Math.PI;
        }

        public Vector2D FormulaManualcoef(double ShootSpeed, Vector2D V0, Vector2D Rp, Vector2D Rh, Vector2D TargetVector, bool CMUFurmule, Position2D shooterPos, double Beta, double Lambda, bool Debug)//2014
        {
            Vector2D V1 = Vector2D.Zero;
            if (CMUFurmule == true)
            {

                V1 = Beta * V0.InnerProduct(Rp) * Rp + ShootSpeed * Rh + Lambda * (V0 - 2 * (V0.InnerProduct(Rh) * Rh));
                #region Draw
                if (Debug == true)
                {
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos - V0)
                    {
                        DrawPen = new Pen(Color.YellowGreen, 0.01f)
                    }, "V0Ot");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + V1)
                    {
                        DrawPen = new Pen(Color.Blue, 0.01f)
                    }, "V1Ot");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rh)
                    {
                        DrawPen = new Pen(Color.Violet, 0.01f)
                    }, "Rh");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rp)
                    {
                        DrawPen = new Pen(Color.Black, 0.01f)
                    }, "Rp");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rp)
                    {
                        DrawPen = new Pen(Color.Black, 0.01f)
                    }, "Rp");
                }
                #endregion
            }
            else
            {
                V1 = Beta * V0.InnerProduct(Rp) * Rp + Lambda * ShootSpeed * Rh + Lambda * V0.InnerProduct(Rh) * Rh;
                #region Draw
                if (Debug == true)
                {
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos - V0)
                    {
                        DrawPen = new Pen(Color.YellowGreen, 0.01f)
                    }, "V0Ot1");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + V1)
                    {
                        DrawPen = new Pen(Color.HotPink, 0.01f)
                    }, "V1Ot1");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rh)
                    {
                        DrawPen = new Pen(Color.Violet, 0.01f)
                    }, "Rh1");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rp)
                    {
                        DrawPen = new Pen(Color.Black, 0.01f)
                    }, "Rp1");
                }
                #endregion
            }
            return V1;
        }
        //----------------------------------------


        Position2D lastPredict = Position2D.Zero;
        private Position2D GetDirectPass(GameStrategyEngine engine, WorldModel Model, int robotID, Position2D Target)
        {
            if (firstDirect)
            {
                firstChip = true;
                robot = new SingleObjectState(Model.OurRobots[robotID]);
                firstDirect = false;
                firstBallPos = Model.BallState.Location;
                angleQ.Clear();
            }

            angleQ.Enqueue((Model.BallState.Speed).AngleInRadians);
            sumang += angleQ.Last();
            double tmpang = sumang / (double)angleQ.Count;
            BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(tmpang, 4));
            PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
            if (angleQ.Count > 5)
            {
                lastang = angleQ.First();
                angleQ.Dequeue();
                sumang -= lastang;
            }
            Position2D intersect = new Position2D();
            if (PrepLine.IntersectWithLine(BallLine, ref intersect))
            {
                DrawingObjects.AddObject(new Circle(intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                Position2D extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(fieldMargin); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
                if (new Circle(Model.OurRobots[robotID].Location, 0.2).IsInCircle(Model.BallState.Location))
                    extendedIntersect = lastExtendedIntersect;
                lastExtendedIntersect = extendedIntersect;
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
                //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((Target - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
                return extendedIntersect;
            }
            return Model.BallState.Location;
        }
        private Position2D GetDirectPass(GameStrategyEngine engine, WorldModel Model, SingleObjectState RobotState, Position2D Target)
        {
            if (firstDirect)
            {
                firstChip = true;
                robot = new SingleObjectState(RobotState);
                firstBallPos = Model.BallState.Location;
                firstDirect = false;
                angleQ.Clear();
            }

            angleQ.Enqueue((Model.BallState.Speed).AngleInRadians);
            sumang += angleQ.Last();
            double tmpang = sumang / (double)angleQ.Count;
            BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(tmpang, 4));
            PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
            if (angleQ.Count > 5)
            {
                lastang = angleQ.First();
                angleQ.Dequeue();
                sumang -= lastang;
            }
            Position2D intersect = new Position2D();

            if (PrepLine.IntersectWithLine(BallLine, ref intersect))
            {
                DrawingObjects.AddObject(new Circle(intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                Position2D extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(fieldMargin); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
                //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((Target - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
                return extendedIntersect;
            }
            return Model.BallState.Location;
        }

        private Position2D GetDirectPass2(GameStrategyEngine engine, WorldModel Model, SingleObjectState RobotState, Position2D Target)
        {
            Position2D extendedIntersect = Position2D.Zero;
            if (firstGetRobotLoc)
            {
                robot = new SingleObjectState(RobotState);
                firstGetRobotLoc = false;
            }
            if (firstDirect)
            {

                firstBallPos = Model.BallState.Location;

            }
            Position2D predictedBall = (Model.PredictedBall[0.5].Location == lastPredict) ? (Model.BallState.Location + Model.BallState.Speed) : Model.PredictedBall[0.5].Location, Intersect = Position2D.Zero;
            lastPredict = Model.PredictedBall[0.5].Location;

            BallLine = new Line(firstBallPos, predictedBall);
            PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);

            if (PrepLine.IntersectWithLine(BallLine, ref Intersect) && ((predictedBall - Intersect).InnerProduct(firstBallPos - Intersect) >= 0 || firstDirect))
            {

                DrawingObjects.AddObject(new Circle(Intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                extendedIntersect = Intersect;// -(Target - Intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(fieldMargin); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        Intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = Intersect;// -(Target - Intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
            }
            else
            {
                extendedIntersect = lastExtendedIntersect;
                DrawingObjects.AddObject(new Circle(Intersect, 0.5, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "stopotpointcalc");
            }
            //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
            //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((tmpTarget - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
            //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");

            //     firstDirect = false;
            lastExtendedIntersect = extendedIntersect;

            return extendedIntersect;

        }
        private Position2D GetDirectPass2(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target)
        {

            Position2D extendedIntersect = Position2D.Zero;
            DrawingObjects.AddObject(new StringDraw("firstGetRobotLoC: " + firstGetRobotLoc, new Position2D(-1.5, 0)));
            if (firstGetRobotLoc)
            {
                robot = new SingleObjectState(Model.OurRobots[RobotID]);
                firstGetRobotLoc = false;
                lastPredict = Model.PredictedBall[0.5].Location;
            }
            if (firstDirect)
            {
                firstBallPos = Model.BallState.Location;
            }
            DrawingObjects.AddObject(new Circle(robot.Location, 0.15, new System.Drawing.Pen(System.Drawing.Color.Chocolate, 0.01f)), "robotlocot");
            DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, 0.17, new System.Drawing.Pen(System.Drawing.Color.Black, 0.01f)), "robotlocotFomSkill");
            Position2D predictedBall = (Model.PredictedBall[0.5].Location == lastPredict) ? (Model.BallState.Location + Model.BallState.Speed) : Model.PredictedBall[0.5].Location, Intersect = Position2D.Zero;
            lastPredict = Model.PredictedBall[0.5].Location;
            BallLine = new Line(firstBallPos, predictedBall);
            PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);

            if (PrepLine.IntersectWithLine(BallLine, ref Intersect) && ((predictedBall - Intersect).InnerProduct(firstBallPos - Intersect) >= 0 || firstDirect))
            {

                DrawingObjects.AddObject(new Circle(Intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                extendedIntersect = Intersect;// -(Target - Intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(fieldMargin);
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        Intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = Intersect;// -(Target - Intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
            }
            else
            {
                extendedIntersect = lastExtendedIntersect;
                DrawingObjects.AddObject(new Circle(Intersect, 0.5, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "stopotpointcalc");
            }
            lastExtendedIntersect = extendedIntersect;
            //   firstDirect = false;
            //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
            //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((Target - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
            //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
            return extendedIntersect;

        }

        private Position2D GetDirectPass2New(GameStrategyEngine engine, WorldModel Model, SingleObjectState RobotState, Position2D Target, OneTouchMode mode)
        {
            var detectedBall = DetectBallPos(Model);
            Position2D extendedIntersect = detectedBall;

            ballHistoryList.Add(index++, detectedBall);
            if (ballHistoryList.Count > dataCount)
            {
                int minkey = ballHistoryList.Keys.OrderBy(o => o).First();
                ballHistoryList.Remove(minkey);
            }
            if (mode == OneTouchMode.Normal)
            {


                if (firstDirect)
                {
                    robot = new SingleObjectState(RobotState);
                    lastPredict = Model.PredictedBall[0.5].Location;
                    firstBallPos = detectedBall;
                    firstDirect = false;
                    Line l = new Line(detectedBall, Model.PredictedBall[0.5].Location);
                    l.PerpenducilarLineToPoint(RobotState.Location).IntersectWithLine(l, ref extendedIntersect);
                }
                else
                {
                    // int k = predictBallPath(out minkey);
                    int k = predictBallPath();
                    Position2D posFinal = firstBallPos;
                    Position2D posFirst = firstBallPos;
                    //if (ballHistoryList.ContainsKey(minkey))
                    //{
                    //    posFirst = ballHistoryList[minkey];
                    //}
                    if (ballHistoryList.ContainsKey(k))
                    {
                        posFinal = ballHistoryList[k];
                    }

                    //       posBaLLHis.Add(posFinal);

                    Position2D intersect = Position2D.Zero;
                    BallLine = new Line(posFirst, posFinal) { DrawPen = new Pen(Color.Red, 0.01f) };

                    PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);

                    if (PrepLine.IntersectWithLine(BallLine, ref intersect))
                    {
                        extendedIntersect = intersect;
                        Vector2D BallLineVec = BallLine.Head - BallLine.Tail;
                        double angb2w = Vector2D.AngleBetweenInDegrees(Target - extendedIntersect, BallLineVec);
                        if (Math.Abs(angb2w) > angleTresh)
                        {
                            Vector2D v = Vector2D.FromAngleSize(BallLineVec.AngleInRadians + Math.Sign(angb2w) * angleTresh * Math.PI / 180, 1);
                            Line robotTargetLine = new Line(Target, Target - v);
                            Position2D inter = Position2D.Zero;
                            if (robotTargetLine.IntersectWithLine(BallLine, ref inter))
                            {
                                if (inter.DistanceFrom(extendedIntersect) > 1)
                                    inter = extendedIntersect + (inter - extendedIntersect).GetNormalizeToCopy(1);
                                extendedIntersect = inter;
                            }
                        }
                        if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                        {
                            Position2D tmpInt = new Position2D();
                            double minDist = double.MaxValue;
                            Position2D NearestIntersect = new Position2D();
                            List<Line> field = GameParameters.GetFieldLines(fieldMargin);
                            foreach (var item in field)
                            {
                                if (!item.IntersectWithLine(BallLine, ref tmpInt))
                                    tmpInt = Model.BallState.Location;
                                if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                                {
                                    double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                                    if (dist < minDist)
                                    {
                                        NearestIntersect = tmpInt;
                                        minDist = dist;
                                    }
                                }
                            }
                            if (minDist < double.MaxValue)
                            {
                                intersect = NearestIntersect;
                                extendedIntersect = intersect;
                            }
                            else
                                extendedIntersect = Model.BallState.Location;
                        }
                        DrawingObjects.AddObject(BallLine, "fsds");
                        DrawingObjects.AddObject(new Circle(posFinal, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Red, 0.01f)), "adsadad");
                        DrawingObjects.AddObject(new Circle(extendedIntersect, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Yellow, 0.01f)), "Predict ball pos");

                    }
                    else
                    {
                        extendedIntersect = lastExtendedIntersect;
                    }
                }
                lastExtendedIntersect = extendedIntersect;
                return extendedIntersect;
            }
            else if (mode == OneTouchMode.Random)
            {
                //var detectedBall = DetectBallPos(Model);
                //Position2D extendedIntersect = detectedBall;
                //ballHistoryList.Add(index++, detectedBall);
                if (firstDirect)
                {
                    robot = new SingleObjectState(RobotState);
                    lastPredict = Model.PredictedBall[0.5].Location;
                    Line l = new Line(detectedBall, Model.PredictedBall[0.5].Location);
                    l.PerpenducilarLineToPoint(RobotState.Location).IntersectWithLine(l, ref extendedIntersect);
                    firstBallPos = detectedBall;
                    firstDirect = false;

                }
                else
                {
                    //   ballHistoryList.Add(index++, detectedBall);

                    int minkey;
                    int k = predictBallPath(out minkey);

                    //int k = predictBallPath();
                    Position2D posFinal = firstBallPos;
                    Position2D posFirst = firstBallPos;
                    if (ballHistoryList.ContainsKey(minkey))
                    {
                        posFirst = ballHistoryList[minkey];
                    }
                    if (ballHistoryList.ContainsKey(k))
                    {
                        posFinal = ballHistoryList[k];
                    }

                    Position2D intersect = Position2D.Zero;
                    BallLine = new Line(posFirst, posFinal) { DrawPen = new Pen(Color.Red, 0.01f) };
                    //BallLine = new Line(firstBallPos, posFinal) { DrawPen = new Pen(Color.Red, 0.01f) };
                    PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);

                    if (PrepLine.IntersectWithLine(BallLine, ref intersect))
                    {
                        extendedIntersect = intersect;
                        Vector2D BallLineVec = BallLine.Head - BallLine.Tail;
                        double angb2w = Vector2D.AngleBetweenInDegrees(Target - extendedIntersect, BallLineVec);
                        if (Math.Abs(angb2w) > angleTresh)
                        {
                            Vector2D v = Vector2D.FromAngleSize(BallLineVec.AngleInRadians + Math.Sign(angb2w) * angleTresh * Math.PI / 180, 1);
                            Line robotTargetLine = new Line(Target, Target - v);
                            Position2D inter = Position2D.Zero;
                            if (robotTargetLine.IntersectWithLine(BallLine, ref inter))
                            {
                                if (inter.DistanceFrom(extendedIntersect) > 1)
                                    inter = extendedIntersect + (inter - extendedIntersect).GetNormalizeToCopy(1);
                                extendedIntersect = inter;
                            }
                        }
                        if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                        {
                            Position2D tmpInt = new Position2D();
                            double minDist = double.MaxValue;
                            Position2D NearestIntersect = new Position2D();
                            List<Line> field = GameParameters.GetFieldLines(fieldMargin);
                            foreach (var item in field)
                            {
                                if (!item.IntersectWithLine(BallLine, ref tmpInt))
                                    tmpInt = Model.BallState.Location;
                                if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                                {
                                    double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                                    if (dist < minDist)
                                    {
                                        NearestIntersect = tmpInt;
                                        minDist = dist;
                                    }
                                }
                            }
                            if (minDist < double.MaxValue)
                            {
                                intersect = NearestIntersect;
                                extendedIntersect = intersect;
                            }
                            else
                                extendedIntersect = Model.BallState.Location;
                        }
                        DrawingObjects.AddObject(BallLine, "fsds");
                        DrawingObjects.AddObject(new Circle(posFinal, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Red, 0.01f)), "adsadad");
                        DrawingObjects.AddObject(new Circle(extendedIntersect, StaticVariables.BALL_RADIUS * 2, new Pen(Color.Yellow, 0.01f)), "Predict ball pos");

                    }
                    else
                    {
                        extendedIntersect = lastExtendedIntersect;
                    }
                }
                lastExtendedIntersect = extendedIntersect;
                return extendedIntersect;
            }
            else
                return new Position2D();
        }


        private int predictBallPath(out int firstkey, int tmp)
        {
            firstkey = -1;
            Dictionary<int, double> PathScore = new Dictionary<int, double>();
            foreach (var fid in ballHistoryList.Keys)
            {
                foreach (var id1 in ballHistoryList.Keys)
                {
                    int key = fid * 10 + id1;
                    if (id1 == fid || PathScore.ContainsKey(id1 * 10 + fid))
                    {
                        continue;
                    }
                    double score = 0;
                    foreach (var id2 in ballHistoryList.Keys)
                    {
                        score += Vector2D.offset_to_line(ballHistoryList[fid], ballHistoryList[id1], ballHistoryList[id2]);
                    }

                    PathScore[key] = score;

                }
            }

            double minScore = double.MaxValue;
            int bestPathPointKey = -1;
            foreach (var item in PathScore.Keys)
            {
                double v = PathScore[item];
                if (v < minScore)
                {
                    minScore = v;
                    bestPathPointKey = item % 10;
                    firstkey = item / 10;
                }
            }
            return bestPathPointKey;
        }

        private int predictBallPath(out int minkey)
        {
            Dictionary<int, double> PathScore = new Dictionary<int, double>();
            minkey = -1;
            if (ballHistoryList.Count == 0)
            {
                return -1;
            }
            minkey = ballHistoryList.Keys.Min();
            foreach (var id1 in ballHistoryList.Keys)
            {
                if (id1 == minkey)
                {
                    continue;
                }
                {
                    double score = 0;
                    foreach (var id2 in ballHistoryList.Keys)
                    {
                        score += Vector2D.offset_to_line(ballHistoryList[minkey], ballHistoryList[id1], ballHistoryList[id2]);
                    }
                    PathScore[id1] = score;
                }
            }
            double minScore = double.MaxValue;
            int bestPathPointKey = -1;
            foreach (var item in PathScore.Keys)
            {
                double v = PathScore[item];
                if (v < minScore)
                {
                    minScore = v;
                    bestPathPointKey = item;
                }
            }
            return bestPathPointKey;
        }
        private int predictBallPath()
        {
            Dictionary<int, double> PathScore = new Dictionary<int, double>();

            foreach (var id1 in ballHistoryList.Keys)
            {
                if (ballHistoryList[id1].DistanceFrom(firstBallPos) < 1e-3)
                {
                    continue;
                }
                {
                    double score = 0;
                    foreach (var id2 in ballHistoryList.Keys)
                    {
                        score += Vector2D.offset_to_line(firstBallPos, ballHistoryList[id1], ballHistoryList[id2]);
                    }
                    PathScore[id1] = score;
                }
            }
            double minScore = double.MaxValue;
            int bestPathPointKey = -1;
            foreach (var item in PathScore.Keys)
            {
                double v = PathScore[item];
                if (v < minScore)
                {
                    minScore = v;
                    bestPathPointKey = item;
                }
            }
            return bestPathPointKey;
        }
        private Position2D DetectBallPos(WorldModel Model)
        {
            double minDist = double.MaxValue;
            Position2D ballPos = new Position2D();
            foreach (var item in StaticVariables.BallPositions)
            {
                if (Vision2AI(item, Model.FieldIsInverted).DistanceFrom(Model.BallState.Location) < minDist)
                {
                    ballPos = Vision2AI(item, Model.FieldIsInverted);
                    minDist = Vision2AI(item, Model.FieldIsInverted).DistanceFrom(Model.BallState.Location);
                }
            }
            return ballPos;
        }
        private Position2D Vision2AI(Position2D pos, bool isReverseSide)
        {
            if (isReverseSide)
                return new Position2D(-pos.X / 1000, pos.Y / 1000);
            return new Position2D(pos.X / 1000, -pos.Y / 1000);
        }

        private Position2D GetChipPass(GameStrategyEngine engine, WorldModel Model, SingleObjectState RobotState, SingleObjectState passerState, Position2D Target)
        {
            Circle C = new Circle(RobotState.Location, chipRadious);
            if (firstChip)
            {
                double minChipRad = 1;
                double maxChipRad = 1.5;
                firstDirect = true;
                robot = new SingleObjectState(RobotState);
                firstChip = false;
                angleQ.Clear();
                if (passerState != null)
                    BallLine = new Line(passerState.Location, passerState.Location + Vector2D.FromAngleSize(GameParameters.AngleModeD(passerState.Angle.Value).ToRadian(), 4));//Vector2D.FromAngleSize(passerState.Angle.Value * Math.PI / 180, 4));
                else
                    BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize((RobotState.Location - Model.BallState.Location).AngleInRadians, 4));
                PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
                firstBallPos = Model.BallState.Location;
                double minDist = 2, maxDist = 3.5;
                double d = Math.Min(maxDist, Math.Max(0, Model.BallState.Location.DistanceFrom(C.Center) - minDist)) / (maxDist - minDist);
                chipRadious = minChipRad + d * (maxChipRad - minChipRad);
                C = new Circle(RobotState.Location, chipRadious);
            }

            if (C.IsInCircle(Model.BallState.Location))
                ballGet = true;
            if (ballGet)
            {
                angleQ.Enqueue(Model.BallState.Speed.AngleInRadians);
                sumang += angleQ.Last();
                double tmpang = sumang / (double)angleQ.Count;
                Position2D predictedBall = Model.PredictedBall[0.5].Location, Intersect = Position2D.Zero;
                BallLine = new Line(firstBallPos, predictedBall);
                //  BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(tmpang, 4));
                PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
                if (angleQ.Count > 5)
                {
                    lastang = angleQ.First();
                    angleQ.Dequeue();
                    sumang -= lastang;
                }
            }
            Position2D intersect = new Position2D();
            PrepLine.IntersectWithLine(BallLine, ref intersect);
            double angTresh = 55;
            Vector2D BallLineVec = BallLine.Head - BallLine.Tail;
            double angb2w = Vector2D.AngleBetweenInDegrees(Target - intersect, BallLineVec);
            DrawingObjects.AddObject(new StringDraw("angb2w : " + angb2w, new Position2D(-2, -1.5)));
            DrawingObjects.AddObject(new Circle(intersect, 0.07, new System.Drawing.Pen(System.Drawing.Color.Beige, 0.01f)), "onetouchperfTargetBefore");
            if (Math.Abs(angb2w) > angTresh)
            {
                Vector2D v = Vector2D.FromAngleSize(BallLineVec.AngleInRadians + Math.Sign(angb2w) * angTresh * Math.PI / 180, 1);
                Line robotTargetLine = new Line(Target, Target - v);
                Position2D inter = Position2D.Zero;
                if (robotTargetLine.IntersectWithLine(BallLine, ref inter))
                {
                    if (inter.DistanceFrom(intersect) > 1)
                        inter = intersect + (inter - intersect).GetNormalizeToCopy(1);
                    intersect = inter;
                }
            }
            //if ()
            {
                DrawingObjects.AddObject(new Circle(intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                Position2D extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
                //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((Target - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
                return extendedIntersect;
            }
            return Model.BallState.Location;
        }
        private Position2D GetChipPass(GameStrategyEngine engine, WorldModel Model, SingleObjectState RobotState, SingleObjectState passerState, Position2D Target, double AngleTresh)
        {
            Circle C = new Circle(RobotState.Location, chipRadious);
            if (firstChip)
            {
                double minChipRad = 1;
                double maxChipRad = 1.5;
                firstDirect = true;
                robot = new SingleObjectState(RobotState);
                firstChip = false;
                angleQ.Clear();
                if (passerState != null)
                    BallLine = new Line(passerState.Location, passerState.Location + Vector2D.FromAngleSize(GameParameters.AngleModeD(passerState.Angle.Value).ToRadian(), 4));
                else
                    BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize((RobotState.Location - Model.BallState.Location).AngleInRadians, 4));
                PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
                firstBallPos = Model.BallState.Location;
                double minDist = 2, maxDist = 3.5;
                double d = Math.Min(maxDist, Math.Max(0, Model.BallState.Location.DistanceFrom(C.Center) - minDist)) / (maxDist - minDist);
                chipRadious = minChipRad + d * (maxChipRad - minChipRad);

            }
            if (C.IsInCircle(Model.BallState.Location))
                ballGet = true;
            if (ballGet)
            {
                angleQ.Enqueue(Model.BallState.Speed.AngleInRadians);
                sumang += angleQ.Last();
                double tmpang = sumang / (double)angleQ.Count;
                Position2D predictedBall = Model.PredictedBall[0.5].Location, Intersect = Position2D.Zero;
                BallLine = new Line(firstBallPos, predictedBall);
                //BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(tmpang, 4));
                PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
                if (angleQ.Count > 5)
                {
                    lastang = angleQ.First();
                    angleQ.Dequeue();
                    sumang -= lastang;
                }
            }
            Position2D intersect = new Position2D();
            PrepLine.IntersectWithLine(BallLine, ref intersect);
            double angTresh = 55;
            Vector2D BallLineVec = BallLine.Head - BallLine.Tail;
            double angb2w = Vector2D.AngleBetweenInDegrees(Target - intersect, BallLineVec);
            DrawingObjects.AddObject(new StringDraw("angb2w : " + angb2w, new Position2D(-2, -1.5)));
            DrawingObjects.AddObject(new Circle(intersect, 0.07, new System.Drawing.Pen(System.Drawing.Color.Beige, 0.01f)), "onetouchperfTargetBefore");
            if (Math.Abs(angb2w) > AngleTresh)
            {
                Vector2D v = Vector2D.FromAngleSize(BallLineVec.AngleInRadians + Math.Sign(angb2w) * AngleTresh * Math.PI / 180, 1);
                Line robotTargetLine = new Line(Target, Target - v);
                Position2D inter = Position2D.Zero;
                if (robotTargetLine.IntersectWithLine(BallLine, ref inter))
                {
                    if (inter.DistanceFrom(intersect) > 1)
                        inter = intersect + (inter - intersect).GetNormalizeToCopy(1);
                    intersect = inter;
                }
            }
            //if ()
            {
                DrawingObjects.AddObject(new Circle(intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                Position2D extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
                //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((Target - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
                return extendedIntersect;
            }
            return Model.BallState.Location;
        }
        private Position2D GetChipPass(GameStrategyEngine engine, WorldModel Model, int robotID, SingleObjectState passerState, Position2D Target)
        {
            Circle C = new Circle(Model.OurRobots[robotID].Location, chipRadious);
            if (firstChip)
            {
                firstDirect = true;
                robot = new SingleObjectState(Model.OurRobots[robotID]);
                firstChip = false;
                angleQ.Clear();
                if (passerState != null)
                    BallLine = new Line(passerState.Location, passerState.Location + Vector2D.FromAngleSize(GameParameters.AngleModeD(passerState.Angle.Value).ToRadian(), 4));
                else
                    BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize((Model.OurRobots[robotID].Location - Model.BallState.Location).AngleInRadians, 4));
                PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
            }
            if (C.IsInCircle(Model.BallState.Location))
                ballGet = true;
            if (ballGet)
            {
                angleQ.Enqueue(Model.BallState.Speed.AngleInRadians);
                sumang += angleQ.Last();
                double tmpang = sumang / (double)angleQ.Count;
                BallLine = new Line(Model.BallState.Location, Model.BallState.Location + Vector2D.FromAngleSize(tmpang, 4));
                PrepLine = BallLine.PerpenducilarLineToPoint(robot.Location);
                if (angleQ.Count > 5)
                {
                    lastang = angleQ.First();
                    angleQ.Dequeue();
                    sumang -= lastang;
                }
            }
            Position2D intersect = new Position2D();
            PrepLine.IntersectWithLine(BallLine, ref intersect);
            double angTresh = 70;
            Vector2D BallLineVec = BallLine.Head - BallLine.Tail;
            double angb2w = Vector2D.AngleBetweenInDegrees(Target - intersect, BallLineVec);
            DrawingObjects.AddObject(new StringDraw("angb2w : " + angb2w, new Position2D(-2, -1.5)));
            if (Math.Abs(angb2w) > angTresh)
            {
                Vector2D v = Vector2D.FromAngleSize(BallLineVec.AngleInRadians + Math.Sign(angb2w) * angTresh * Math.PI / 180, 1);
                Line robotTargetLine = new Line(Target, Target - v);
                Position2D inter = Position2D.Zero;
                if (robotTargetLine.IntersectWithLine(BallLine, ref inter))
                {
                    if (inter.DistanceFrom(intersect) > 1)
                        inter = intersect + (inter - intersect).GetNormalizeToCopy(1);
                    intersect = inter;
                }
            }
            //if (PrepLine.IntersectWithLine(BallLine, ref intersect))
            {
                DrawingObjects.AddObject(new Circle(intersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.01f)), "onetouchperform2inter");
                Position2D extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                DrawingObjects.AddObject(BallLine, "onetouchperform2BallLine");

                if (!GameParameters.IsInField(extendedIntersect, fieldMargin))
                {
                    Position2D tmpInt = new Position2D();
                    double minDist = double.MaxValue;
                    Position2D NearestIntersect = new Position2D();
                    List<Line> field = GameParameters.GetFieldLines(fieldMargin); ;
                    foreach (var item in field)
                    {
                        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                            tmpInt = Model.BallState.Location;
                        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                        {
                            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                            if (dist < minDist)
                            {
                                NearestIntersect = tmpInt;
                                minDist = dist;
                            }
                        }
                    }
                    if (minDist < double.MaxValue)
                    {
                        intersect = NearestIntersect;// +(Model.BallState.Location - NearestIntersect).GetNormalizeToCopy(0.4);
                        extendedIntersect = intersect;// -(Target - intersect).GetNormalizeToCopy(normlizeD);
                    }
                    else
                        extendedIntersect = Model.BallState.Location;
                }
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.02, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2");
                //DrawingObjects.AddObject(new Line(extendedIntersect, extendedIntersect + Vector2D.FromAngleSize((Target - extendedIntersect).AngleInRadians, 0.1)), "onetouchperform2ang");
                //DrawingObjects.AddObject(new Circle(extendedIntersect, 0.09, new System.Drawing.Pen(System.Drawing.Color.Goldenrod, 0.01f)), "onetouchperform2Circle");
                return extendedIntersect;
            }
            return Model.BallState.Location;
        }

        public double AngleCorrection(WorldModel Model, bool CMUFurmule, double ShootSpeed, Position2D Target, Position2D ShooterPos, int shooterID, double PassSpeed, bool Debug)
        {
            Vector2D V1 = Vector2D.Zero;
            Vector2D V0 = Vector2D.Zero;
            //PassSpeed -= 1.4;
            PassSpeed = Math.Max(0, PassSpeed);
            double Shootspeed = Math.Min(ShootSpeed, Program.MaxKickSpeed);
            Vector2D Rp = Vector2D.Zero;
            Vector2D Rh = Vector2D.Zero;
            if (Firsttime2)
            {
                distance_Shooter_from_BAll = ShooterPos.DistanceFrom(Model.BallState.Location);
                Firsttime2 = false;
            }
            //  double ballDeccel = (-0.1341 * PassSpeed * PassSpeed) + (1.4299 * PassSpeed) - 0.4284;
            //double FinalV = Math.Sqrt(Math.Abs(Math.Pow(PassSpeed, 2) - (2 * ballDeccel * distance_Shooter_from_BAll)));
            //FinalV = Math.Max(FinalV, 0);
            double passSpeedPhase2 = PassSpeed * 5 / 7;
            double ballAccelPhase1 = -5;
            double ballAccelPhase2 = -0.3;
            double dxPhase1 = 0;
            dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
            double dxPhase2 = Model.BallState.Location.DistanceFrom(ShooterPos) - dxPhase1;
            double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
            double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;

            V0 = Model.BallState.Speed;
            V0.NormalizeTo(vf);
            Vector2D TargetVector = Target - ShooterPos;
            double Alfa = TargetVector.AngleInRadians;
            double CorrectAngle = 100;
            int counter = 0;
            while (Math.Abs(CorrectAngle) > .001 && counter < 200)
            {
                Rh = new Vector2D(Math.Cos(Alfa), Math.Sin(Alfa));
                Rp = new Vector2D(-Math.Sin(Alfa), Math.Cos(Alfa));
                V1 = Formula(ShootSpeed, V0, Rp, Rh, TargetVector, CMUFurmule, ShooterPos, Debug);
                CorrectAngle = Vector2D.AngleBetweenInRadians(TargetVector, V1);
                Alfa = Alfa + CorrectAngle;
                counter++;
            }
            if (Math.Abs(CorrectAngle) > 0.08)
                Alfa = (Target - ShooterPos).AngleInRadians;
            Position2D Robotaim = ShooterPos + Vector2D.FromAngleSize((Model.OurRobots[shooterID].Angle.Value * Math.PI) / 180, 10);
            #region Draw
            if (Debug == true)
            {
                DrawingObjects.AddObject(new Line(ShooterPos, Robotaim) { DrawPen = new Pen(Color.BlueViolet, 0.01f) }, "RCC");
                DrawingObjects.AddObject(new StringDraw("Correct Angle:" + CorrectAngle.ToString(), new Position2D(2.8, 0)));
                DrawingObjects.AddObject(new StringDraw("Correct Counter:" + counter.ToString(), new Position2D(2.5, 0)));
                DrawingObjects.AddObject(new StringDraw("VHope: " + vf.ToString(), Model.BallState.Location.Extend(-.2, 0)), "VHope");
                DrawingObjects.AddObject(new StringDraw("VBall: " + Model.BallState.Speed.Size.ToString(), Model.BallState.Location.Extend(-.4, 0)), "VBall");
                // DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 2)) { DrawPen = new Pen(Color.Brown, 0.01f) }, "RobotAngleVec");
            }
            #endregion
            return Alfa * 180 / Math.PI;
        }
        public Vector2D Formula(double ShootSpeed, Vector2D V0, Vector2D Rp, Vector2D Rh, Vector2D TargetVector, bool CMUFurmule, Position2D shooterPos, bool Debug)
        {
            double Beta, Lambda;
            Vector2D V1 = Vector2D.Zero;
            if (CMUFurmule == true)
            {
                Beta = 0.33484161129381196;// 0.62;
                Lambda = 0.79767488714666024; //-0.1;
                V1 = Beta * V0.InnerProduct(Rp) * Rp + ShootSpeed * Rh + Lambda * (V0 - 2 * (V0.InnerProduct(Rh) * Rh));
                #region Draw
                if (Debug == true)
                {
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos - V0) { DrawPen = new Pen(Color.YellowGreen, 0.01f) }, "V0Ot");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + V1) { DrawPen = new Pen(Color.Blue, 0.01f) }, "V1Ot");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rh) { DrawPen = new Pen(Color.Violet, 0.01f) }, "Rh");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rp) { DrawPen = new Pen(Color.Black, 0.01f) }, "Rp");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rp) { DrawPen = new Pen(Color.Black, 0.01f) }, "Rp");
                }
                #endregion
            }
            else
            {
                double beta = 0.61;//0.21487181946512507;//0.25388318490370415;// 0.20476196652135509;//0.34697347895620012;//0.34408405559924121;//0.36452211460804;//0.495828428151975;//0.33484161129381196;//0.33484161129381196;//0.27072061029398603;//0.40125980413525519;//0.0048043998389737039;//0.5151173;//0.5909789227003156;  //0.1432;
                double lambda = 0.84;//0.82093449656452611;//1.2155844203367729;//1.3208007922549565;//0.82093449656452611; //1.0244028803451253;//1.2790495510714845;//1.2364707933941481;//1.1591227020542936;//0.79767488714666024;//0.79767488714666024;//1.2391641302082057;//1.3704293134568455;//1.3166224628080128;//1.258953;//1.3419605064800277; //.0724;
                V1 = beta * V0.InnerProduct(Rp) * Rp + lambda * ShootSpeed * Rh + lambda * V0.InnerProduct(Rh) * Rh;
                #region Draw
                if (Debug == true)
                {
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos - V0) { DrawPen = new Pen(Color.YellowGreen, 0.01f) }, "V0Ot1");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + V1) { DrawPen = new Pen(Color.HotPink, 0.01f) }, "V1Ot1");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rh) { DrawPen = new Pen(Color.Violet, 0.01f) }, "Rh1");
                    DrawingObjects.AddObject(new Line(shooterPos, shooterPos + Rp) { DrawPen = new Pen(Color.Black, 0.01f) }, "Rp1");
                }
                #endregion
            }
            return V1;
        }
    }
}
