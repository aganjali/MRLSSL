using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class HaltRole : RoleBase
    {
        public GameDefinitions.SingleWirelessCommand Halt(GameDefinitions.WorldModel Model, int RobotID)
        {
            SingleWirelessCommand s = new SingleWirelessCommand();
            s.W = 0;

            bool Regular = true;// Regular = TuneVariables.Default.GetValue<bool>("RegularHalt");
            if (Regular && Model.OurRobots.ContainsKey(RobotID))
            {
                //if (first)
                //{
                //    first = false;
                //    TargetLocation = Model.OurRobots[RobotID].Location + 0.8 * Model.OurRobots[RobotID].Speed;
                //}
                //return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, TargetLocation, Model.OurRobots[RobotID].Angle.Value, false, false, 3.5, false);
                Dictionary<int,SingleWirelessCommand> prev=RobotComponentsController.PreviousCommands ;
                if (prev != null && prev.ContainsKey(RobotID))
                {
                    s.Vx = prev[RobotID].Vx / 1.08;
                    s.Vy = prev[RobotID].Vy / 1.08;
                   // s.W = prev[RobotID].W / 1.05;
                }
            }
            Planner.Add(RobotID, s);
            return s;
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
