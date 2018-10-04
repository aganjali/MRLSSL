using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;


namespace MRL.SSL.AIConsole.Plays
{
    class OurNewPenaltyPlay : PlayBase
    {

        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            return false;
            return Status == GameStatus.Penalty_OurTeam_Go || Status == GameStatus.Penalty_OurTeam_Waiting;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            int? goalie = Model.GoalieID;
            int? penaltyShooter = Model.OurRobots.Keys.Where(q => q != goalie.Value).ToList().First();

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(GotoPointRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<GotoPointRole>(goalie.Value).GotoPoint(eng, wmd, goalie.Value, GameParameters.OurGoalCenter.Extend(-0.09, 0), 180, false, false, false, false, 0, false);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, penaltyShooter, typeof(NewPenaltyShooterRole)))
                Functions[penaltyShooter.Value] = (eng, wmd) => GetRole<NewPenaltyShooterRole>(penaltyShooter.Value).Perform(eng, wmd, penaltyShooter.Value);

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {

        }
    }
}
