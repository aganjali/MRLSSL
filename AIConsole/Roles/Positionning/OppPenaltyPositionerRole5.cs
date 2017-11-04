using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.AIConsole.Roles
{
    class OppPenaltyPositionerRole5 : RoleBase
    {
        static Position2D target1 = new Position2D(1.3, 0);
        public MRL.SSL.GameDefinitions.SingleWirelessCommand RunRole(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, target1, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, true, true, 0, 0, 2);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(target1);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new OppPenaltyPositionerRole1(), new OppPenaltyPositionerRole2(), new OppPenaltyPositionerRole3(),
                                        new OppPenaltyPositionerRole4(),new OppPenaltyPositionerRole4()};
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
