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
    class OurPenaltyPlay : PlayBase
    {
        GameDefinitions.GameStatus LastState = GameStatus.Normal;
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
           return false;
            double dist, DistFromBorder;
            if (LastState == GameStatus.Penalty_OurTeam_Go && !GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.07, out dist, out DistFromBorder))
            {
                Status = GameStatus.Normal;
                return false;
            }
            LastState = Status;
            return (engine.EngineID == 0 && (Status == GameDefinitions.GameStatus.Penalty_OurTeam_Go || Status == GameDefinitions.GameStatus.Penalty_OurTeam_Waiting))
               || (engine.EngineID == 1 && (Status == GameDefinitions.GameStatus.Penalty_Opponent_Go || Status == GameDefinitions.GameStatus.Penalty_Opponent_Waiting));
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            List<DefenceInfo> infos;
            CurrentlyAssignedRoles = NormalDefenceAssigner(engine, Model, out infos, out Functions);
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return null;
        }

        public Dictionary<int, RoleBase> NormalDefenceAssigner(GameStrategyEngine engine, WorldModel Model, out List<DefenceInfo> Infoes, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> rolesTobeAssigned = new Dictionary<int, RoleBase>();
            Infoes = new List<DefenceInfo>();
            DefenderCommand df1, df2, dfgoli;
            List<DefenderCommand> Commands = new List<DefenderCommand>();
            Dictionary<int, float> scores;
            if (engine.GameInfo.OppTeam.GoaliID.HasValue)
                scores = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
            else
                scores = engine.GameInfo.OppTeam.Scores;
            dfgoli = new DefenderCommand()
            {
                RoleType = typeof(GoalieNormalRole)
            };
            if (scores.Count == 0)
            {
                df1 = new DefenderCommand()
                {
                    OppID = null,
                    RoleType = typeof(DefenderNormalRole1)
                };
                df2 = new DefenderCommand()
                {
                    OppID = null,
                    RoleType = typeof(DefenderNormalRole2)
                };
            }
            else if (scores.Count == 1)
            {
                df1 = new DefenderCommand()
                {
                    OppID = scores.First().Key,
                    RoleType = typeof(DefenderNormalRole1)
                };
                df2 = new DefenderCommand()
                {
                    OppID = null,
                    RoleType = typeof(DefenderNormalRole2)
                };
            }
            else
            {
                df1 = new DefenderCommand()
                {
                    OppID = scores.First().Key,
                    RoleType = typeof(DefenderNormalRole1)
                };
                df2 = new DefenderCommand()
                {
                    OppID = scores.ElementAt(1).Key,
                    RoleType = typeof(DefenderNormalRole2)
                };
            }
            Commands.Add(dfgoli);
            Commands.Add(df1);
            Commands.Add(df2);
            Infoes = FreekickDefence.Match(engine, Model, Commands , true);

            RoleBase rt;
            List<RoleInfo> roles = new List<RoleInfo>();
            rt = typeof(PenaltyShooterRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 10, 0));

            rt = typeof(DefenderNormalRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(DefenderNormalRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));



            rt = typeof(PenaltyPositionningRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(PenaltyPositionningRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            Dictionary<int, RoleBase> matched;
            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? Defender1ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(DefenderNormalRole1)))
                Defender1ID = matched.Where(w => w.Value.GetType() == typeof(DefenderNormalRole1)).First().Key;

            int? Defender2ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(DefenderNormalRole2)))
                Defender2ID = matched.Where(w => w.Value.GetType() == typeof(DefenderNormalRole2)).First().Key;

            int? stop1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(PenaltyShooterRole)))
                stop1 = matched.Where(w => w.Value.GetType() == typeof(PenaltyShooterRole)).First().Key;

            int? stop2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(PenaltyPositionningRole1)))
                stop2 = matched.Where(w => w.Value.GetType() == typeof(PenaltyPositionningRole1)).First().Key;

            int? stop3 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(PenaltyPositionningRole2)))
                stop3 = matched.Where(w => w.Value.GetType() == typeof(PenaltyPositionningRole2)).First().Key;


            DefenceInfo GoaliInfo = null, Def1Info = null, Def2Info = null;

            if (Infoes.Any(w => w.RoleType == typeof(GoalieNormalRole)))
                GoaliInfo = Infoes.Where(w => w.RoleType == typeof(GoalieNormalRole)).First();
            if (Infoes.Any(w => w.RoleType == typeof(DefenderNormalRole1)))
                Def1Info = Infoes.Where(w => w.RoleType == typeof(DefenderNormalRole1)).First();
            if (Infoes.Any(w => w.RoleType == typeof(DefenderNormalRole2)))
                Def2Info = Infoes.Where(w => w.RoleType == typeof(DefenderNormalRole2)).First();

            Position2D GoaliPos = GameParameters.OurGoalCenter + new Vector2D(0.1, 0), Def1Pos = GameParameters.OurGoalCenter + new Vector2D(1, 0.12), Def2Pos = GameParameters.OurGoalCenter + new Vector2D(1, -0.12);
            double GoaliTeta = 180, Def1Teta = 180, Def2Teta = 180;
            if (GoaliInfo != null)
            {
                GoaliPos = GoaliInfo.DefenderPosition.Value;
                GoaliTeta = GoaliInfo.Teta;
            }
            if (Def1Info != null)
            {
                Def1Pos = Def1Info.DefenderPosition.Value;
                Def1Teta = Def1Info.Teta;
            }
            if (Def2Info != null)
            {
                Def2Pos = Def2Info.DefenderPosition.Value;
                Def2Teta = Def2Info.Teta;
            }

            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(matched.Count);


            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieNormalRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieNormalRole>(wmd.GoalieID.Value).Run(eng, wmd, Model.GoalieID.Value, GoaliPos, GoaliTeta);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderNormalRole1)))
                Functions[Defender1ID.Value] = (eng, wmd) => GetRole<DefenderNormalRole1>(Defender1ID.Value).Run(eng, wmd, Defender1ID.Value, Def1Pos, Def1Teta);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(DefenderNormalRole2)))
                Functions[Defender2ID.Value] = (eng, wmd) => GetRole<DefenderNormalRole2>(Defender2ID.Value).Run(eng, wmd, Defender2ID.Value, Def2Pos, Def2Teta);

            //if (Model.Status == GameStatus.Penalty_OurTeam_Go)
            //{
            //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop1, typeof(GetBallRole)))
            //        Functions[stop1.Value] = (eng, wmd) => GetRole<GetBallRole>(stop1.Value).Perform(eng, wmd, stop1.Value, GameParameters.OppGoalRight.Extend(0, -0.1), 255, false, 0.1);
            //}
            //else
            //{

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop1, typeof(PenaltyShooterRole)))
                Functions[stop1.Value] = (eng, wmd) => GetRole<PenaltyShooterRole>(stop1.Value).Perform(eng, wmd, stop1.Value, Program.MaxKickSpeed, PreviouslyAssignedRoles);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop2, typeof(PenaltyPositionningRole1)))
                Functions[stop2.Value] = (eng, wmd) => GetRole<PenaltyPositionningRole1>(stop2.Value).RunRole(eng, wmd, stop2.Value, PreviouslyAssignedRoles);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop3, typeof(PenaltyPositionningRole2)))
                Functions[stop3.Value] = (eng, wmd) => GetRole<PenaltyPositionningRole2>(stop3.Value).RunRole(eng, wmd, stop3.Value, PreviouslyAssignedRoles);


            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return PreviouslyAssignedRoles;
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

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
            FreekickDefence.RestartActiveFlags();
            FreekickDefence.BallIsMoved = false;
        }
    }
}
