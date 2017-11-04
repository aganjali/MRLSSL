using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class ComponetsTestRole : RoleBase
    {

        public SingleWirelessCommand Perform(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            if (RobotComponentsController.RobotCommands != null && RobotComponentsController.RobotCommands.ContainsKey(RobotID))
            {
                //RobotComponentsController.RobotCommands[RobotID].BackSensor = false;
               // RobotComponentsController.RobotCommands[RobotID].SpinBack = 9;
                //RobotComponentsController.RobotCommands[RobotID].KickPower = 255;
                Planner.Add(RobotID, RobotComponentsController.RobotCommands[RobotID]);
                return RobotComponentsController.RobotCommands[RobotID];
            }
            return new MRL.SSL.GameDefinitions.SingleWirelessCommand();
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
