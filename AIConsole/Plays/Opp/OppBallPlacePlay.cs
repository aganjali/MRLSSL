using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OppBallPlacePlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            //return false;
            return Status == GameDefinitions.GameStatus.BallPlace_Opponent;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            if (Model.GoalieID.HasValue)
            {
                //Planner.Add();

            }
            
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;

            #region Matcher
            RoleBase r;
            roles = new List<RoleInfo>();

            r = typeof(AvoiderRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole4).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole5).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole6).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole7).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));





            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? goalie = Model.GoalieID;


            int? AvoiderRole1Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole1)))
                AvoiderRole1Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole1)).First().Key;

            int? AvoiderRole2Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole2)))
                AvoiderRole2Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole2)).First().Key;

            int? AvoiderRole3Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole3)))
                AvoiderRole3Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole3)).First().Key;

            int? AvoiderRole4Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole4)))
                AvoiderRole4Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole4)).First().Key;

            int? AvoiderRole5Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole5)))
                AvoiderRole5Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole5)).First().Key;

            int? AvoiderRole6Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole6)))
                AvoiderRole6Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole6)).First().Key;

            int? AvoiderRole7Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole7)))
                AvoiderRole7Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole7)).First().Key;


            #endregion

            #region Assigner
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(AvoiderGoalieRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<AvoiderGoalieRole>(goalie.Value).perform(wmd, goalie.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole1Id, typeof(AvoiderRole1)))
                Functions[AvoiderRole1Id.Value] = (eng, wmd) => GetRole<AvoiderRole1>(AvoiderRole1Id.Value).Perform(engine , Model , AvoiderRole1Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole2Id, typeof(AvoiderRole2)))
                Functions[AvoiderRole2Id.Value] = (eng, wmd) => GetRole<AvoiderRole2>(AvoiderRole2Id.Value).Perform(engine, Model, AvoiderRole2Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole3Id, typeof(AvoiderRole3)))
                Functions[AvoiderRole3Id.Value] = (eng, wmd) => GetRole<AvoiderRole3>(AvoiderRole3Id.Value).Perform(engine, Model, AvoiderRole3Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole4Id, typeof(AvoiderRole4)))
                Functions[AvoiderRole4Id.Value] = (eng, wmd) => GetRole<AvoiderRole4>(AvoiderRole4Id.Value).Perform(engine, Model, AvoiderRole4Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole5Id, typeof(AvoiderRole5)))
                Functions[AvoiderRole5Id.Value] = (eng, wmd) => GetRole<AvoiderRole5>(AvoiderRole5Id.Value).Perform(engine, Model, AvoiderRole5Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole6Id, typeof(AvoiderRole6)))
                Functions[AvoiderRole6Id.Value] = (eng, wmd) => GetRole<AvoiderRole6>(AvoiderRole6Id.Value).Perform(engine, Model, AvoiderRole6Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole7Id, typeof(AvoiderRole7)))
                Functions[AvoiderRole7Id.Value] = (eng, wmd) => GetRole<AvoiderRole7>(AvoiderRole7Id.Value).Perform(engine, Model, AvoiderRole7Id.Value);


            DefenceTest.WeHaveGoalie = true;

            #endregion


            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(GameDefinitions.WorldModel Model, GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
        }
    }
}
