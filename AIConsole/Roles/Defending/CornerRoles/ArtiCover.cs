using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Roles
{
    class ArtiCover : RoleBase
    {
        public void Run(WorldModel model, int RobotID)
        {
            Position2D target = new Position2D();
            Vector2D right = GameParameters.OurGoalLeft - model.BallState.Location;
            Vector2D left = GameParameters.OurGoalRight - model.BallState.Location;
            double angle =Math.Abs( Vector2D.AngleBetweenInRadians(GameParameters.OurGoalLeft - model.BallState.Location , GameParameters.OurGoalRight - model.BallState.Location));
            if( angle > Math.PI)
            angle = (2*Math.PI )-angle;
            double radius = .18 / angle;


            int OppBallOwner = model.Opponents.OrderBy(i => i.Value.Location.DistanceFrom(model.BallState.Location)).Select(Y => Y.Key).ToList().First();
            Vector2D robotAngle = Vector2D.FromAngleSize(model.Opponents[OppBallOwner].Angle.Value * Math.PI / 180, radius);

            double robotLeft = Math.Abs(Vector2D.AngleBetweenInRadians(robotAngle, left));
            robotLeft = (robotLeft > Math.PI) ? (2 * Math.PI) - robotLeft : robotLeft;

            double robotRight = Math.Abs(Vector2D.AngleBetweenInRadians(robotAngle, right));
            robotRight = (robotRight > Math.PI) ? (2 * Math.PI) - robotRight : robotRight;

            double leftRight = Math.Abs(Vector2D.AngleBetweenInRadians(left, right));
            leftRight = (leftRight > Math.PI) ? (2 * Math.PI) - leftRight : leftRight;

            if (robotLeft + robotRight < leftRight+ .017)
            {
                target = model.BallState.Location + robotAngle.GetNormalizeToCopy(radius);
            }
            else
            {
                target = model.BallState.Location + (GameParameters.OurGoalCenter - model.BallState.Location).GetNormalizeToCopy(radius);
            }
            Planner.Add(RobotID, target, (model.BallState.Location - model.OurRobots[RobotID].Location).AngleInDegrees);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
    }
}
