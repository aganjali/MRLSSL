using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Skills
{
    class CutBallSkill:SkillBase
    {
        public CutBallSkill()
        {

        }
        public Vector2D lastSpeed = new Vector2D();

        bool firstInref = true;
        //public void go(WorldModel Model, int RobotID)
        //{

        //    double maxAcc = 6;
        //    Position2D Target = GameParameters.OppGoalCenter;
        //    Vector2D targetVec = Target - Model.OurRobots[RobotID].Location;
        //    Line targetLine = new Line(Target, Model.OurRobots[RobotID].Location);
        //    Line ballLine = new Line((Model.BallState.Location + Model.BallState.Speed), Model.BallState.Location);
        //    Position2D? intersectTemp = targetLine.IntersectWithLine(ballLine);
        //    Position2D intersect = new Position2D();
        //    if (!intersectTemp.HasValue)
        //        return;
        //    else
        //        intersect = intersectTemp.Value;
        //    if (intersect.X > Model.OurRobots[RobotID].Location.X)
        //    {
        //        Planner.Add(RobotID, new Position2D(Model.OurRobots[RobotID].Location.X + 2, Model.OurRobots[RobotID].Location.Y), 180, true);
        //        return;
        //    }

        //    intersect = intersect - targetVec.GetNormalizeToCopy(0.094);
        //    double robotFinalTeta = targetVec.AngleInDegrees;
        //    double db = Model.BallState.Location.DistanceFrom(intersect);
        //    double vb = Model.BallState.Speed.Size;
        //    double dr = Model.OurRobots[RobotID].Location.DistanceFrom(intersect);
        //    double vr = vb * dr / db;
        //    double tb = db / vb, tr = dr / Math.Max(Model.OurRobots[RobotID].Speed.Size, 0.01);
        //    double kP = 1;
        //    double kPx = 0.6, dt = (tb - tr);

        //    Vector2D v2Go = targetVec.GetNormalizeToCopy(vr * kP);
        //    Planner.Add(RobotID, Accelerate(Model, RobotID, Target, (Target - Model.OurRobots[RobotID].Location).AngleInDegrees, v2Go), false);
        //    BackCalculateInRefrence(Model, RobotID, new Position2D(0, 2), GameParameters.OppGoalCenter, 0.01, v2Go);


        //}
        Vector2D lastV = new Vector2D();
        public SingleWirelessCommand   Accelerate(WorldModel Model, int RobotID, Position2D Target, double TargetAngle,Vector2D CutSpeed)
        {
            if (firstInref && Model.lastVelocity.ContainsKey(RobotID))
            {
                lastV = Model.lastVelocity[RobotID];
                firstInref = false;
            }
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            double robotAngle = Model.OurRobots[RobotID].Angle.Value;
            Vector2D movingDirection = Target - robotLocation;
            
            double A = 6;
            if (Model.OurRobots[RobotID].Speed.Size > (CutSpeed.Size) )
                A = -6;
            if (A > 0)
                lastV += movingDirection.GetNormalizeToCopy(Math.Abs(A) / 60);
            else
                lastV -= movingDirection.GetNormalizeToCopy(Math.Abs(A) / 60);

            if ((CutSpeed - lastV).Size < 1)
                lastV = CutSpeed;
            double Rotation = Model.OurRobots[RobotID].Angle.Value;
            if (Rotation > 180)
                Rotation -= 360;
            if (Rotation < -180)
                Rotation += 360;
            Rotation *= Math.PI / (double)180;
            Vector2D V = new Vector2D();

            V.X = lastV.Y * Math.Cos(Rotation) - lastV.X * Math.Sin(Rotation);
            V.Y = lastV.X * Math.Cos(Rotation) + lastV.Y * Math.Sin(Rotation);
            double ww = AngularController(Model.OurRobots[RobotID], TargetAngle);
            return new SingleWirelessCommand(new Vector2D(V.X, V.Y), ww, false, 0, 0, false, false);
        }

        double DefrentionalT = 0, LastDr = 0, IntegralT2 = 0;
        double lastPIDangular = 0;
        private double AngularController(SingleObjectState state, double TargetTeta)
        {

            double dt = 0, PID = 0, dr = 0, PID_Max = 40;

            double kP = 4, kI = 0.2, kd = -0.0000, landa = 0.9, AW = 200000;
            //
            dt = (float)(state.Angle - TargetTeta);
            dt = (dt * Math.PI) / 180;
            if (dt > Math.PI)
                dt -= Math.PI * 2;
            if (dt < -Math.PI)
                dt += Math.PI * 2;
            IntegralT2 += dt;
            IntegralT2 *= landa;

            if (IntegralT2 < -PID_Max)
                IntegralT2 = -PID_Max;
            if (IntegralT2 > PID_Max)
                IntegralT2 = PID_Max;

            DefrentionalT = dt - LastDr;
            PID = IntegralT2 * kI + DefrentionalT * kd + dt * kP;

            if (Math.Abs(PID - lastPIDangular) > AW / 60)
                PID = lastPIDangular + Math.Sign(PID - lastPIDangular) * AW / 60;


            if (PID < -PID_Max)
                PID = -PID_Max;
            if (PID > PID_Max)
                PID = PID_Max;

            LastDr = dt;
            lastPIDangular = PID;

            return PID;
        }

        public void go(WorldModel Model, int RobotID)
        {
            double maxAcc = 6;
            Position2D Target = GameParameters.OppGoalCenter;
            Vector2D targetVec = Target - Model.OurRobots[RobotID].Location;
            Line targetLine = new Line(Target, Model.OurRobots[RobotID].Location);
            Line ballLine = new Line((Model.BallState.Location + Model.BallState.Speed), Model.BallState.Location);
            Position2D? intersectTemp = targetLine.IntersectWithLine(ballLine);
            Position2D intersect = new Position2D();
            if (!intersectTemp.HasValue)
                return;
            else
                intersect = intersectTemp.Value;
            if (intersect.X > Model.OurRobots[RobotID].Location.X)
            {
                Planner.Add(RobotID, new Position2D(Model.OurRobots[RobotID].Location.X + 2, Model.OurRobots[RobotID].Location.Y), 180, true);
                return; 
            }

            intersect = intersect - targetVec.GetNormalizeToCopy(0.08);
            double robotFinalTeta = targetVec.AngleInDegrees;
            double db = Model.BallState.Location.DistanceFrom(intersect);
            double vb = Model.BallState.Speed.Size;
            double dr = Model.OurRobots[RobotID].Location.DistanceFrom(intersect);
            double vr = vb * dr / db;
            double tb = db / vb, tr = dr / Math.Max(Model.OurRobots[RobotID].Speed.Size, 0.01);
            double kP = 1.2;
            double kPx = 0.6, dt = (tb - tr);
            Vector2D roobotFinalSpeed = targetVec.GetNormalizeToCopy(kP * vr);
            //if ((lastSpeed - roobotFinalSpeed).Size > maxAcc / 60)
            //{
            //    roobotFinalSpeed.NormalizeTo(maxAcc / 60);
            //}
            //lastSpeed = roobotFinalSpeed;
            //
            SingleWirelessCommand SWC = Accelerate(Model, RobotID, Target, robotFinalTeta,roobotFinalSpeed);
            Vector2D v = new Vector2D(SWC.Vx, SWC.Vy);
           // v.NormalizeTo(kP * vr);
           // SWC.Vy = v.Y;
            //SWC.Vx = v.X + kPx * Math.Abs(dt);

            SWC.KickPower = 150;
            SWC.isChipKick = false;
            SWC.BackSensor = true;
            Planner.Add(RobotID, SWC, false);
        }

    }
}
