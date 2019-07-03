using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Roles
{
    class AvoiderGoalieRole : RoleBase
    {
        double distAvoid = 0.6;

        public void perform(WorldModel model, int robotId) {
            Position2D target = GameParameters.OurGoalCenter.Extend(-0.1,0);
            Planner.Add(robotId, target, 180, PathType.UnSafe, true, true, false, false, false, new List<Obstacle>() {
                    new Obstacle()
                    {
                        State = new SingleObjectState(Position2D.Interpolate(StaticVariables.ballPlacementPos,model.BallState.Location,0.5), Vector2D.Zero, 0),
                        R = new Vector2D(distAvoid, model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos)/2 + distAvoid),
                        Type = ObstacleType.Rectangle
                    }
                });
        }
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 10;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            ;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new AvoiderGoalieRole() };
        }
    }
}
