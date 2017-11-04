using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class Defender : RoleBase
    {
        public SingleWirelessCommand DefendRB(GameStrategyEngine engine, WorldModel model, int RobotID)
        {
            Position2D Defence = model.BallState.Location;
            Vector2D v1 = (GameParameters.OurGoalLeft - Defence);
            Vector2D v2 = (GameParameters.OurGoalRight - Defence);
            double alfa = Vector2D.AngleBetweenInRadians(v1, v2);
            double d = Defence.DistanceFrom(GameParameters.OurGoalCenter);
            double dd = dDcalculator(alfa, d);
            //double gdd = gddcalculator(alfa, d);
            //Position2D gdpos = new Position2D(GameParameters.OurGoalCenter.X, GameParameters.OurGoalCenter.Y - gdd);
            //double dis = gdpos.DistanceFrom(Defence);
            //Position2D target = gdpos + (Defence - gdpos).GetNormalizeToCopy(dis - dd);
            //return GetSkill<GotoPointSkill>().GotoPoint(engine,
            //    model, RobotID, target,
            //    (Defence - model.OurRobots[RobotID].Location).AngleInDegrees,
            //    false, false, false, 0, 0, 2);

            Position2D pos = GetDefencePos(model, Defence);
            pos = pos.Extend(0, -RobotParameters.OurRobotParams.Diameter / 2);

            var line = Vector2D.Bisector((pos - Defence), (GameParameters.OurGoalRight - Defence), Defence);
            DrawingObjects.AddObject(line, "rbinter");
            Position2D? inter = line.IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));
            if (inter.HasValue)
            {
                Position2D target = inter.Value + (Defence - inter.Value).GetNormalizeToCopy((Defence - inter.Value).Size - dd);
                return GetSkill<GotoPointSkill>().GotoPoint(engine, model, RobotID, target, (Defence - model.OurRobots[RobotID].Location).AngleInDegrees,
              false, false, false, 0, 0, 2);
            }
            return new SingleWirelessCommand();
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 0;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        double dDcalculator(double alfa, double d)
        {
            double p00 = -7.789;
            double p10 = 37.15;
            double p01 = 3.51;
            double p20 = -42.82;
            double p11 = -7.731;
            double p02 = -0.2383;
            return p00 + p10 * alfa + p01 * d + p20 * alfa * alfa + p11 * alfa * d + p02 * d * d;
        }

        double gddcalculator(double alfa, double d)
        {
            throw new NotImplementedException();
        }
        double GDcalculator(double alfa, double d)
        {
            double p00 = -0.1182;
            double p10 = 1.919;
            double p01 = 0.0793;
            double p20 = -2.339;
            double p11 = -0.254;
            double p02 = -0.004689;
            return p00 + p10 * alfa + p01 * d + p20 * alfa * alfa + p11 * alfa * d + p02 * d * d;
        }

        double dGcalculator(double alfa, double d)
        {
            double p00 = 10.96;
            double p10 = -55.24;
            double p01 = -2.471;
            double p20 = 67.73;
            double p11 = 8.536;
            double p02 = 0.2449;
            return p00 + p10 * alfa + p01 * d + p20 * alfa * alfa + p11 * alfa * d + p02 * d * d;
        }
        private Position2D GetDefencePos(WorldModel model, Position2D Defence)
        {
            Vector2D v1 = (GameParameters.OurGoalLeft - Defence);
            Vector2D v2 = (GameParameters.OurGoalRight - Defence);
            double alfa = Vector2D.AngleBetweenInRadians(v1, v2);
            double d = Defence.DistanceFrom(GameParameters.OurGoalCenter);
            double gd = GDcalculator(alfa, d);
            double dg = dGcalculator(alfa, d);

            Position2D gdpos = new Position2D(GameParameters.OurGoalCenter.X, GameParameters.OurGoalCenter.Y + gd);
            double dis = gdpos.DistanceFrom(Defence);
            Position2D target = gdpos + (Defence - gdpos).GetNormalizeToCopy(dis - dg);
            return target;
        }
    }
}
