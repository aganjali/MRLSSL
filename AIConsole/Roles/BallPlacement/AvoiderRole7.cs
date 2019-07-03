using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
namespace MRL.SSL.AIConsole.Roles
{
    class AvoiderRole7 : RoleBase
    {
        List<Position2D> targets = new List<Position2D>();
        int avoiderIndex = 6;
        double distAvoid = 0.6;

        public void Perform(GameStrategyEngine engine, WorldModel model, int robotId)
        {
            Position2D goalie;
            Obstacle obs;
            targets = FreekickDefence.CalculateAvoiderTargets(engine, model, out obs, out goalie);
            Position2D target = targets[avoiderIndex];
            Planner.Add(robotId, target, 180, PathType.UnSafe, true, true, false, false, false/*, new List<Obstacle>() {
                   obs
                }*/);
        }
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Position2D goalie;
            Obstacle obs;
            targets = FreekickDefence.CalculateAvoiderTargets(engine, Model, out obs, out goalie);
            Position2D target = targets[avoiderIndex];
            return target.DistanceFrom(Model.OurRobots[RobotID].Location);
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
            return RoleCategory.Positioner;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new AvoiderRole1(), new AvoiderRole2(), new AvoiderRole3(), new AvoiderRole4(), new AvoiderRole5(), new AvoiderRole6(), new AvoiderRole7() };
        }
    }
}
