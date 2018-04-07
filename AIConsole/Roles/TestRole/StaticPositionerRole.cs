using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.IO;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class StaticPositionerRole : RoleBase
    {
        static Position2D Target = Position2D.Zero;
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }
        public void perform(GameStrategyEngine engine, WorldModel Model, int robotID,Position2D target)
        {
            Target = target;
            Planner.Add(robotID, target, (Model.BallState.Location - target).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Target.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new StaticPositionerRole()};
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
