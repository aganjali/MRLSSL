using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.AIConsole.Roles.Defending.Normal;

namespace MRL.SSL.AIConsole.Roles
{
   public class NewRegionalRole : RoleBase
   {
       Position2D pos = new Position2D();
       Position2D target = new Position2D();
        double dist = 0.0;
        double dista = 0.0;
        double distm = 0.0;
        List<DefenceInfo> Cur = new List<DefenceInfo>();

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

        }

        public void Perform(GameStrategyEngine engine, WorldModel Model, int StaticDefender1ID, int StaticDefender2ID, int nrg)
        {
           
            pos = Target(engine, Model, StaticDefender1ID, StaticDefender2ID);
            NormalSharedState.CommonInfo.RegionalDefenderTarget = target;
            Planner.Add(nrg, target, (Model.BallState.Location - pos).AngleInDegrees);

        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(NormalSharedState.CommonInfo.RegionalDefenderTarget);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new ActiveRole2017(), new NewRegionalRole(),
             new NewSupporter2Role(),
             new NewAttackerRole()};
            return res;
          
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public Position2D Target(GameStrategyEngine engine, WorldModel Model, int Marker, int Attacker)
        {

            //vector of robot to goli
            List<Vector2D> Vectors = new List<Vector2D>();
            Vectors.Add(Model.OurRobots[Marker].Location - GameParameters.OurGoalCenter);
            Vectors.Add(Model.OurRobots[Attacker].Location - GameParameters.OurGoalCenter);
            Vectors.Add(GameParameters.OurGoalRight - GameParameters.OurGoalCenter);


            dista = Math.Abs(4.5 - (Model.OurRobots[Attacker].Location.X));

            distm = Math.Abs(4.5 - (Model.OurRobots[Marker].Location.X));

            dist = (distm + dista) / 2;

            if (dist >= 3.30)
            {
                dist = 3.35;
            }
            else if (dist <= 1.5)
            {
                dist = 3;
            }

            List<double> Beta = new List<double>();
            foreach (var item in Vectors)
            {
                Beta.Add((item.AngleInDegrees > 0) ? item.AngleInDegrees - 90 : 270 + item.AngleInDegrees);
            }
            List<double> SortedBeta = Beta.OrderBy(o => o).ToList();
            List<double> DifBEta = new List<double>();
            double max = double.MinValue;
            int maxindex = 0;

            //find angle
            for (int i = 0; i < SortedBeta.Count - 1; i++)
            {
                if (Math.Sign((GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2) * Math.PI / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2 * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2))).Y) != Model.BallState.Location.Y)
                {
                    if ((SortedBeta[i + 1] - SortedBeta[i]) * 1.3 > max)
                    {
                        max = SortedBeta[i + 1] - SortedBeta[i];
                        maxindex = i;
                    }
                }
                else if (Math.Sign((GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2) * Math.PI / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize(((SortedBeta[i] + SortedBeta[i + 1]) / 2 * Math.PI), 3) / 180, Vector2D.Zero, 0f), .2))).Y) == Model.BallState.Location.Y)
                {
                    if (SortedBeta[i + 1] - SortedBeta[i] > max)
                    {
                        max = SortedBeta[i + 1] - SortedBeta[i];
                        maxindex = i;
                    }
                }
            }
            double targetangle = 0;
            double targetbeta = (SortedBeta[maxindex] + SortedBeta[maxindex + 1]) / 2;
            if (targetbeta > 0 && targetbeta < 90)
            {
                targetangle = targetbeta + 90;
            }
            else
            {
                targetangle = targetbeta - 270;
            }
            foreach (var item in Vectors)
            {
                DrawingObjects.AddObject(new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + item, new Pen(Color.Black, .01f)), item.X.ToString());
            }
            if (Math.Abs(targetangle) > 90 && Cur.Any(t => t.RoleType == typeof(RegionalDefenderRole2)))
            {
                targetbeta = SortedBeta[maxindex] + (((SortedBeta[maxindex + 1] - SortedBeta[maxindex])) / 3);
                if (targetbeta > 0 && targetbeta < 90)
                {
                    targetangle = targetbeta + 90;
                }
                else
                {
                    targetangle = targetbeta - 270;
                }
                Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), dist) / 180, Vector2D.Zero, 0f), .2));
                target = GameParameters.OurGoalCenter + FinalVector;
            }
            else
            {
                target = GameParameters.OurGoalCenter + (Vector2D.FromAngleSize((targetangle * Math.PI) / 180, dist));
                // Vector2D FinalVector = Vector2D.FromAngleSize((targetangle * Math.PI) / 180, GameParameters.SafeRadi(new SingleObjectState(GameParameters.OurGoalCenter + Vector2D.FromAngleSize((targetangle * Math.PI), dist) / 180)/*, Vector2D.Zero, 0f), .2)*/)));
                // target = GameParameters.OurGoalCenter + FinalVector;
            }

            DrawingObjects.AddObject(new Line((Model.OurRobots[Attacker].Location), (GameParameters.OurGoalCenter)));
            DrawingObjects.AddObject(new Line((Model.OurRobots[Marker].Location), (GameParameters.OurGoalCenter)));
            DrawingObjects.AddObject(new Line(GameParameters.OurLeftCorner, GameParameters.OurGoalCenter));
            DrawingObjects.AddObject(new Line(GameParameters.OurRightCorner, GameParameters.OurGoalCenter));
            DrawingObjects.AddObject(new Circle(target, 0.3));
            return target;
        }
    }
}
