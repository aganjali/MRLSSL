using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.AIConsole.Engine
{
    class StaticRoleAssigner
    {
        public static Dictionary<int, SingleWirelessCommand> Commands = new Dictionary<int, SingleWirelessCommand>();

        public static bool AssignRole(GameStrategyEngine engine, WorldModel Model, Dictionary<int, RoleBase> PreviouslyAssignedRoles, Dictionary<int, RoleBase> CurrentlyAssignedRoles,int robotID , Type RoleType)
        {
            RoleBase RoleToBeAssigned = RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            if (Model.OurRobots.ContainsKey(robotID))
            {

                if (PreviouslyAssignedRoles != null && PreviouslyAssignedRoles.ContainsKey(robotID) && PreviouslyAssignedRoles[robotID].GetType() == RoleType)
                    CurrentlyAssignedRoles[robotID] = PreviouslyAssignedRoles[robotID];
                else
                    CurrentlyAssignedRoles[robotID] = RoleToBeAssigned;
                return true;
            }
            return false;
        }

        public static bool AssignRole(GameStrategyEngine engine, WorldModel Model, Dictionary<int, RoleBase> PreviouslyAssignedRoles, Dictionary<int, RoleBase> CurrentlyAssignedRoles, int? robotID, Type RoleType)
        {
            if (!robotID.HasValue) return false;
            RoleBase RoleToBeAssigned = RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            if (Model.OurRobots.ContainsKey(robotID.Value))
            {

                if (PreviouslyAssignedRoles != null && PreviouslyAssignedRoles.ContainsKey(robotID.Value) && PreviouslyAssignedRoles[robotID.Value].GetType() == RoleType)
                    CurrentlyAssignedRoles[robotID.Value] = PreviouslyAssignedRoles[robotID.Value];
                else
                    CurrentlyAssignedRoles[robotID.Value] = RoleToBeAssigned;
                return true;
            }
            return false;
        }
    }
}
