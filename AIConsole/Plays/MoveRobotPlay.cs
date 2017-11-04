using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;

namespace MRL.SSL.AIConsole.Plays
{
    class MoveRobotPlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            return Status == GameDefinitions.GameStatus.MoveRobot;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {

            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);

            if (RobotComponentsController.SelectedID.HasValue)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, RobotComponentsController.SelectedID.Value, typeof(MoveRobotRole)))
                    Functions[RobotComponentsController.SelectedID.Value] = (eng, wm) => GetRole<MoveRobotRole>(RobotComponentsController.SelectedID.Value).MoveToTarget(engine, wm,
                        RobotComponentsController.SelectedID.Value);
                var our = Model.OurRobots;
                if (RobotComponentsController.SelectedID.HasValue)
                    our = Model.OurRobots.Where(w => w.Key != RobotComponentsController.SelectedID.Value).ToDictionary(k => k.Key, v => v.Value);

                //foreach (var item in our)
                //{
                //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, item.Key, typeof(HaltRole)))
                //        Functions[item.Key] = (eng, wm) => GetRole<HaltRole>(item.Key).Halt(wm,
                //            item.Key);
                //}
                for (int i = 0; i < our.Count; i++)
                {
                    int key = our.ElementAt(i).Key;
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, key, typeof(HaltRole)))
                        Functions[key] = (eng, wm) => GetRole<HaltRole>(key).Halt(wm,
                            key);
                }

            }
            else
            {
                var our = Model.OurRobots;
                for (int i = 0; i < our.Count; i++)
                {
                    int key = our.ElementAt(i).Key;
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, key, typeof(HaltRole)))
                        Functions[key] = (eng, wm) => GetRole<HaltRole>(key).Halt(wm,
                            key);
                }
              
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
