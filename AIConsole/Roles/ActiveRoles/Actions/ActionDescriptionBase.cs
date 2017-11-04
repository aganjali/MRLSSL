using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using NormalSharedState = MRL.SSL.AIConsole.Engine.NormalSharedState;
using ActiveInfo = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveInfo;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public abstract class ActionDescriptionBase
    {

        protected string name = null;
        protected int frame = 0;
        protected double lambda = 1;

        public double Lambda
        {
            get { return lambda; }
        }

        public virtual string Name
        {
            get
            {
                if (name == null)
                {
                    name = this.GetType().ToString();
                }
                return name;
            }

        }
        public virtual void UpdateLambda()
        {
            lambda = 1 - 0.9 * Math.Exp(-0.2 * frame++);
        }
        protected Vector2D CalculateKickAngle(WorldModel Model, int RobotID, Position2D Target, double finalKickSpeed, double maxKickSpeed)
        {
            double minBallSpeedTresh = 0.3;
            Vector2D b = Model.BallState.Speed;
            if (b.Size < minBallSpeedTresh)
                b = Vector2D.Zero;
            Vector2D c = Vector2D.FromAngleSize((Target - Model.BallState.Location).AngleInRadians,
                finalKickSpeed - Math.Max(0, Math.Min(finalKickSpeed / 2, GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, Target - Model.BallState.Location).Y)));
            return c;
            double k = 0.7;
            Vector2D a = c - k * b;
            if (a.Size > maxKickSpeed)
            {
                double cs = finalKickSpeed / 2;
                double dc = -finalKickSpeed / 2;
                int counter = 0;
                while (counter < 10)
                {
                    dc *= 0.5;
                    double alfa = finalKickSpeed / 2 + cs + dc;
                    c.NormalizeTo(alfa);
                    a = c - k * b;
                    if (maxKickSpeed - a.Size < 0)
                        cs = alfa;
                    counter++;
                }
            }
            if (a.Size > maxKickSpeed)
            {
                a.NormalizeTo(maxKickSpeed);
            }
            Vector2D v1 = Vector2D.FromAngleSize((Target - Model.BallState.Location).AngleInRadians + (30.0).ToRadian(), 1);
            Vector2D v2 = Vector2D.FromAngleSize((Target - Model.BallState.Location).AngleInRadians - (30.0).ToRadian(), 1);
            if (!Vector2D.IsBetween(v2, v1, a))
            {
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(v2, a)) < Math.Abs(Vector2D.AngleBetweenInDegrees(v1, a)))
                {
                    a = v2.GetNormalizeToCopy(a.Size);
                }
                else
                    a = v1.GetNormalizeToCopy(a.Size);
            }
            return a;
        }
        double lastM = double.MinValue;
        protected bool IsSuitable4Kick(WorldModel Model, int RobotID, bool angleCorrection, double Tolerance, double acc, Position2D Target, double kickSpeed, out Vector2D kickVec, out Position2D KickTarget)
        {
            double beta = 1 - Math.Min(acc / Tolerance, 1);
            if (angleCorrection)
            {
                kickVec = CalculateKickAngle(Model, RobotID, Target, kickSpeed, Program.MaxKickSpeed);
                if (kickVec.Size > 0)
                {
                    KickTarget = Model.BallState.Location + kickVec.GetNormalizeToCopy((Target - Model.BallState.Location).Size);

                }
                else
                    KickTarget = Target;

            }
            else
            {
                kickVec = (Target - Model.BallState.Location).GetNormalizeToCopy(kickSpeed);
                KickTarget = Target;
            }
            double g0 = ((KickTarget - Model.BallState.Location).AngleInDegrees - Tolerance / 2.0);
            double g1 = ((KickTarget - Model.BallState.Location).AngleInDegrees + Tolerance / 2.0);
            double robotAng = Model.OurRobots[RobotID].Angle.Value;
            double m = Math.Min(GameParameters.AngleModeD(robotAng - g0), GameParameters.AngleModeD(g1 - robotAng));
            bool res = true;
            if (m < 0)
                res = false;
            else if (m > beta * GameParameters.AngleModeD(g1 - g0) / 2)
                res = true;
            else if (m > lastM)
                res = false;
            lastM = m;
            if (res && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) > .15)
                res = false;
            return res;
        }

        public abstract NormalSharedState.ActiveActionMode ActionCategory();
        public virtual void DetermineActionState(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            UpdateLambda();
            DrawingObjects.AddObject(new StringDraw("lambda: " + lambda.ToString(), Model.OurRobots[RobotID].Location + new Vector2D(-0.4, 0.4)), "lambdakeke");
        }
        public abstract void ProvideActionInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo);

        public abstract double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo);
        public virtual void Print(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActionInfo actInfo)
        {
            DrawingObjects.AddObject(new StringDraw(actInfo.strState, GameParameters.OppGoalCenter + new Vector2D(-0.4, 0))); Color color = color = Color.LightPink;
            if (ActiveInfo.isChip && ActiveInfo.kickSpeed > 0)
            {
                color = Color.SkyBlue;
            }
            if (!ActiveInfo.isChip && ActiveInfo.kickSpeed > 0)
            {
                color = Color.YellowGreen;
            }
            DrawingObjects.AddObject(new Line(Model.BallState.Location, ActiveInfo.Target, new System.Drawing.Pen(color, 0.043f)), "kicktargetlineactive1");
            DrawingObjects.AddObject(new Line(Model.BallState.Location, ActiveInfo.KickTarget, new System.Drawing.Pen(Color.Aqua, 0.045f)), "kicktargetlinkjhhjhjueactive");
        }

        public virtual void Reset()
        {
            lambda = 1;
            frame = 0;
        }
    }
}
