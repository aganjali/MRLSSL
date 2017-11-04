using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.AIConsole.Plays
{
    class ComponentsTestPlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            return Status == GameDefinitions.GameStatus.ComponetsTest;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            
            foreach (int id in Model.OurRobots.Keys)
            {
                int key = id;
                Functions[key] = (a,b)=> GetRole<ComponetsTestRole>(key).Perform(engine, Model, key);
                if (!PreviouslyAssignedRoles.ContainsKey(key) || !(PreviouslyAssignedRoles[key] is ComponetsTestRole))
                    CurrentlyAssignedRoles[key] = new ComponetsTestRole();
                else
                    CurrentlyAssignedRoles[key] = PreviouslyAssignedRoles[key];
            }

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            throw new NotImplementedException();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model,GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
        }
    }
}
