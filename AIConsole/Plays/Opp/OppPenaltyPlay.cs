using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OppPenaltyPlay : PlayBase
    {
        Position2D? ballFirst = null;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        GameDefinitions.GameStatus LastState = GameStatus.Normal;
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            //return false;
            double dist, DistFromBorder;
            if (LastState == GameStatus.Penalty_OurTeam_Go && (!GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.07, out dist, out DistFromBorder) ||
                    (ballFirst.HasValue && Model.BallState.Location.DistanceFrom(ballFirst.Value) > 0.2)))
            { 
                LastState = GameStatus.Normal;
                Status = GameStatus.Normal;
                return false;
            }
            LastState = Status;
            return ((Status == GameDefinitions.GameStatus.Penalty_Opponent_Go || Status == GameDefinitions.GameStatus.Penalty_Opponent_Waiting) && engine.EngineID == 0)|| ((Status == GameStatus.Penalty_OurTeam_Go || Status == GameStatus.Penalty_OurTeam_Waiting) && engine.EngineID == 1);

            //return ((Status == GameDefinitions.GameStatus.Penalty_OurTeam_Go || Status == GameDefinitions.GameStatus.Penalty_OurTeam_Waiting));
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            if (!ballFirst.HasValue)
                ballFirst = Model.BallState.Location;
            FreekickDefence.weAreInKickoff = false;
            DefenceTest.BallTest = FreekickDefence.testDefenceState;
            DefenceTest.MakeOutPut();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            
            int RobotID = Model.GoalieID.Value;
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, RobotID, typeof(PenaltyGoalieRole)))
            //    Functions[RobotID] = (eng, wmd) => GetRole<PenaltyGoalieRole>(RobotID).RunRole(engine, Model, RobotID, PreviouslyAssignedRoles);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, RobotID, typeof(IntelligencePenaltyGoalKeeperRole)))
                Functions[RobotID] = (eng, wmd) => GetRole<IntelligencePenaltyGoalKeeperRole>(RobotID).Run(engine, Model, RobotID);
            CurrentlyAssignedRoles = Assigner(engine, Model, out Functions);
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return null;
        }

        public Dictionary<int, RoleBase> Assigner(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> rolesTobeAssigned = new Dictionary<int, RoleBase>();


            RoleBase rt;
            List<RoleInfo> roles = new List<RoleInfo>();

            //rt = typeof(PenaltyKeeperRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(OppPenaltyPositionerRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(OppPenaltyPositionerRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(OppPenaltyPositionerRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(OppPenaltyPositionerRole4).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(OppPenaltyPositionerRole5).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));


            Dictionary<int, RoleBase> matched;
            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);
            int? goalie = Model.GoalieID;
            if (matched.Any(w => w.Value.GetType() == typeof(IntelligencePenaltyGoalKeeperRole)))
                goalie = matched.Where(w => w.Value.GetType() == typeof(IntelligencePenaltyGoalKeeperRole)).First().Key;

            int? pos1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole1)))
                pos1 = matched.Where(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole1)).First().Key;

            int? pos2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole2)))
                pos2 = matched.Where(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole2)).First().Key;

            int? pos3 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole3)))
                pos3 = matched.Where(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole3)).First().Key;

            int? pos4 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole4)))
                pos4 = matched.Where(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole4)).First().Key;

            int? pos5 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole5)))
                pos5 = matched.Where(w => w.Value.GetType() == typeof(OppPenaltyPositionerRole5)).First().Key;


            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(matched.Count);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(IntelligencePenaltyGoalKeeperRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<IntelligencePenaltyGoalKeeperRole>(wmd.GoalieID.Value).Run(eng, wmd, wmd.GoalieID.Value);


            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, pos1, typeof(OppPenaltyPositionerRole1)))
                Functions[pos1.Value] = (eng, wmd) => GetRole<OppPenaltyPositionerRole1>(pos1.Value).RunRole(eng, wmd, pos1.Value);


            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, pos2, typeof(OppPenaltyPositionerRole2)))
                Functions[pos2.Value] = (eng, wmd) => GetRole<OppPenaltyPositionerRole2>(pos2.Value).RunRole(eng, wmd, pos2.Value);


            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, pos3, typeof(OppPenaltyPositionerRole3)))
                Functions[pos3.Value] = (eng, wmd) => GetRole<OppPenaltyPositionerRole3>(pos3.Value).RunRole(eng, wmd, pos3.Value);


            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, pos4, typeof(OppPenaltyPositionerRole4)))
                Functions[pos4.Value] = (eng, wmd) => GetRole<OppPenaltyPositionerRole4>(pos4.Value).RunRole(eng, wmd, pos4.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, pos5, typeof(OppPenaltyPositionerRole5)))
                Functions[pos5.Value] = (eng, wmd) => GetRole<OppPenaltyPositionerRole5>(pos5.Value).RunRole(eng, wmd, pos5.Value);

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;

            return CurrentlyAssignedRoles;
        }

        private void AddRoleInfo(List<RoleInfo> roles, Type role, double weight, double margin)
        {
            RoleBase r = role.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, weight, margin));
        }


        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model,GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
            LastState = GameStatus.Normal;
            FreekickDefence.RestartActiveFlags();
            PenaltyGoalKeeperLearningUnit.kicked = false;
            PenaltyGoalKeeperLearningUnit.OnceExecute2 = true;
            PenaltyGoalKeeperLearningUnit.OnceExecute = true;
            //PenaltyGoalKeeperLearningUnit.kickFrame= 0;///////////////////////////////////////////////////////////////////////////////////////
            IntelligencePenaltyGoalKeeperRole.nextStateCal = false;
            IntelligencePenaltyGoalKeeperRole.nextState = false;
            IntelligencePenaltyGoalKeeperRole.nextState2Cal = false;
            IntelligencePenaltyGoalKeeperRole.nextState2 = false;
            IntelligencePenaltyGoalKeeperRole.firstTime1 = true;
            IntelligencePenaltyGoalKeeperRole.firstTime4 = true;
            IntelligencePenaltyGoalKeeperRole.firstTime5 = true;
            IntelligencePenaltyGoalKeeperRole.firstTime6 = true;
            IntelligencePenaltyGoalKeeperRole.firstTime7 = true;
            IntelligencePenaltyGoalKeeperRole.firstTime8 = true;
            IntelligencePenaltyGoalKeeperRole.firstTime9 = true;
            IntelligencePenaltyGoalKeeperRole.onceFlag = true;
            IntelligencePenaltyGoalKeeperRole.timeLine = 0;
            IntelligencePenaltyGoalKeeperRole.firstTime2 = true;
            IntelligencePenaltyGoalKeeperRole.onceFlag2 = true;
            IntelligencePenaltyGoalKeeperRole.firstTime3 = true;
            IntelligencePenaltyGoalKeeperRole.diveFrame = 0;
            IntelligencePenaltyGoalKeeperRole.initialStateInDive = new SingleObjectState();
            IntelligencePenaltyGoalKeeperRole.centerToLeftOrRight = 0;
            IntelligencePenaltyGoalKeeperRole.currentV = 0;
            IntelligencePenaltyGoalKeeperRole.currentW = 0;
            IntelligencePenaltyGoalKeeperRole.SWC = new SingleWirelessCommand();
            IntelligencePenaltyGoalKeeperRole.diveCounter = 0;
            IntelligencePenaltyGoalKeeperRole.counterCenter = 0;
            PenaltyGoalKeeperLearningUnit.firstTime= true;
            
        }
    }
}
