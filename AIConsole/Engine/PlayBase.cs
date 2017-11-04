using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;

namespace MRL.SSL.AIConsole.Engine
{
    public abstract class PlayBase
    {
        public Dictionary<int, RoleBase> PreviouslyAssignedRoles = new Dictionary<int, RoleBase>();

        public abstract bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status);
        public abstract Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions);
        public abstract Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model);
        public abstract PlayResult QueryPlayResult();
        public abstract void ResetPlay(WorldModel Model, GameStrategyEngine engine);
        protected List<RoleInfo> roles;
        protected RoleMatcher _roleMatcher = new RoleMatcher();
        protected StrategyManager strategyManager = new StrategyManager();
        List<int> StraIds = new List<int>();

        public Dictionary<int, RoleBase> DefenceAssigner(GameStrategyEngine engine, WorldModel Model, int StrategyAttendance, int minDefenceAttendance, out List<int> StrategyIds, ref Dictionary<int, CommonDelegate> Functions, out Dictionary<int, RoleBase> PrevAssignedRoles)
        {
            NormalDefenceAssigner Defence = new NormalDefenceAssigner();
            Dictionary<RoleBase, Position2D?> Positions;
            Dictionary<RoleBase, double> Angles;
            Dictionary<int, RoleBase> matched = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            RoleBase r;
            int defenderRobots;
            //if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
            //    defenderRobots = Math.Max(Model.OurRobots.Count - 1 - StrategyAttendance, minDefenceAttendance);
            //else
            defenderRobots = Math.Max(Model.OurRobots.Count - StrategyAttendance, minDefenceAttendance);
            defenderRobots = Math.Min(defenderRobots, Model.OurRobots.Count);
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(defenderRobots);
            bool marker4firstAttacker = false, marker4SecondAttacker = false, marker4thirdAttacker = false;
            Dictionary<int, float> scores;
            if (engine.GameInfo.OppTeam.GoaliID.HasValue)
                scores = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
            else
                scores = engine.GameInfo.OppTeam.Scores;
            #region Matcher
            List<RoleInfo> roles = new List<RoleInfo>();

            int? GoalieID = null;
            if (defenderRobots > 0 && (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value)))
            {
                defenderRobots--;
                GoalieID = Model.GoalieID;
            }
            if (defenderRobots > 0)
            {
                //r = typeof(DefenderNormalRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                //roles.Add(new RoleInfo(r, 1, 0));
                r = typeof(DefenderCornerRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 1, 0));
            }
            if (defenderRobots > 1)
            {
                //r = typeof(DefenderNormalRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                //roles.Add(new RoleInfo(r, 1, 0));
                r = typeof(DefenderCornerRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 1, 0));
            }
            if (defenderRobots > 2)
            {

                if (scores.Count > 2)
                {
                    r = typeof(DefenderMarkerNormalRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                    roles.Add(new RoleInfo(r, 1, 0));
                    marker4thirdAttacker = true;
                }
                else
                {
                    r = typeof(DefenderMarkerNormalRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                    roles.Add(new RoleInfo(r, 1, 0));
                    marker4firstAttacker = true;
                }

            }
            if (defenderRobots > 3)
            {
                if (scores.Count > 2)
                {
                    r = typeof(DefenderMarkerNormalRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                    roles.Add(new RoleInfo(r, 1, 0));
                    marker4firstAttacker = true;
                }
                else
                {
                    r = typeof(DefenderMarkerNormalRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                    roles.Add(new RoleInfo(r, 1, 0));
                    marker4SecondAttacker = true;
                }
            }
            if (defenderRobots > 4)
            {
                if (scores.Count > 2)
                {
                    r = typeof(DefenderMarkerNormalRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                    roles.Add(new RoleInfo(r, 1, 0));
                    marker4SecondAttacker = true;
                }
                else
                {
                    r = typeof(DefenderMarkerNormalRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                    roles.Add(new RoleInfo(r, 1, 0));
                    marker4thirdAttacker = true;
                }
            }
            Dictionary<RoleBase, DefenceInfo> output = Defence.Assign(engine, Model, out Positions, out Angles, marker4firstAttacker, marker4SecondAttacker, marker4thirdAttacker, false, (GameParameters.OurGoalCenter.X - Model.BallState.Location.X) - 1);
            List<int> ids, tmp;
            if (GoalieID.HasValue)
            {
                tmp = Model.OurRobots.OrderBy(t => t.Value.Location.DistanceFrom(GameParameters.OurRightCorner)).ToDictionary(d => d.Key, t => t.Value).Keys.Where(w => w != Model.GoalieID.Value).ToList();
            }
            else
            {
                tmp = Model.OurRobots.OrderBy(t => t.Value.Location.DistanceFrom(GameParameters.OurRightCorner)).ToDictionary(d => d.Key, t => t.Value).Keys.ToList();
            }
            ids = tmp.Except(StraIds).ToList();
            //foreach (var item in tmp.ToList())
            //{
            //    foreach (var item2 in StraIds.ToList())
            //    {
            //        if (item == item2 && ids.Contains(item))
            //            ids.Remove(item);
            //    }
            //}
            if (ids.Count < roles.Count)
            {
                foreach (var item in tmp.ToList())
                {
                    if (ids.Count >= roles.Count)
                        break;
                    if (!ids.Contains(item))
                        ids.Add(item);
                }
            }




            //if (Model.GoalieID.HasValue)
            matched = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);
            //else
            //    matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);


            //int? def1ID = null;
            //if (matched.Any(w => w.Value.GetType() == typeof(DefenderNormalRole1)))
            //    def1ID = matched.Where(w => w.Value.GetType() == typeof(DefenderNormalRole1)).First().Key;
            //int? def2ID = null;
            //if (matched.Any(w => w.Value.GetType() == typeof(DefenderNormalRole2)))
            //    def2ID = matched.Where(w => w.Value.GetType() == typeof(DefenderNormalRole2)).First().Key;

            int? def1ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(DefenderCornerRole1)))
                def1ID = matched.Where(w => w.Value.GetType() == typeof(DefenderCornerRole1)).First().Key;
            int? def2ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(DefenderCornerRole2)))
                def2ID = matched.Where(w => w.Value.GetType() == typeof(DefenderCornerRole2)).First().Key;

            int? marker1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(DefenderMarkerNormalRole1)))
                marker1 = matched.Where(w => w.Value.GetType() == typeof(DefenderMarkerNormalRole1)).First().Key;
            int? marker2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(DefenderMarkerNormalRole2)))
                marker2 = matched.Where(w => w.Value.GetType() == typeof(DefenderMarkerNormalRole2)).First().Key;
            int? marker3 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(DefenderMarkerNormalRole3)))
                marker3 = matched.Where(w => w.Value.GetType() == typeof(DefenderMarkerNormalRole3)).First().Key;
            #endregion
            #region GetInformations
            Position2D? GoaliPos = Positions.Where(w => w.Key.GetType() == typeof(GoalieCornerRole)).First().Value;
            //Position2D? Defender1Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderNormalRole1)).First().Value;
            //Position2D? Defender2Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderNormalRole2)).First().Value;
            Position2D? Defender1Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderCornerRole1)).First().Value;
            Position2D? Defender2Pos = null;
            //if (Positions.Any(a => a.Key.GetType() == typeof(DefenderCornerRole2)))
            Defender2Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderCornerRole2)).First().Value;

            Position2D? Marker1Pos = null;
            if (Positions.Any(a => a.Key.GetType() == typeof(DefenderMarkerNormalRole1)))
                Marker1Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderMarkerNormalRole1)).First().Value;
            Position2D? Marker2Pos = null;
            if (Positions.Any(a => a.Key.GetType() == typeof(DefenderMarkerNormalRole2)))
                Marker2Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderMarkerNormalRole2)).First().Value;
            Position2D? Marker3Pos = null;
            if (Positions.Any(a => a.Key.GetType() == typeof(DefenderMarkerNormalRole3)))
                Marker3Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderMarkerNormalRole3)).First().Value;

            double Goaliang = Angles.Where(w => w.Key.GetType() == typeof(GoalieCornerRole)).First().Value;
            //double Defender1ang = Angles.Where(w => w.Key.GetType() == typeof(DefenderNormalRole1)).First().Value;
            //double Defender2ang = Angles.Where(w => w.Key.GetType() == typeof(DefenderNormalRole2)).First().Value;
            double Defender1ang = Angles.Where(w => w.Key.GetType() == typeof(DefenderCornerRole1)).First().Value;
            double Defender2ang = 0;
            //if (Angles.Any(a => a.Key.GetType() == typeof(DefenderCornerRole2)))
            Defender2ang = Angles.Where(w => w.Key.GetType() == typeof(DefenderCornerRole2)).First().Value;
            double Marker1ang = 0;
            if (Angles.Any(a => a.Key.GetType() == typeof(DefenderMarkerNormalRole1)))
                Marker1ang = Angles.Where(w => w.Key.GetType() == typeof(DefenderMarkerNormalRole1)).First().Value;
            double Marker2ang = 0;
            if (Angles.Any(a => a.Key.GetType() == typeof(DefenderMarkerNormalRole2)))
                Marker2ang = Angles.Where(w => w.Key.GetType() == typeof(DefenderMarkerNormalRole2)).First().Value;
            double Marker3ang = 0;
            if (Angles.Any(a => a.Key.GetType() == typeof(DefenderMarkerNormalRole3)))
                Marker3ang = Angles.Where(w => w.Key.GetType() == typeof(DefenderMarkerNormalRole3)).First().Value;
            #endregion
            #region Assigner
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, GoalieID, typeof(GoalieCornerRole)))
                Functions[GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(GoalieID.Value).Run(engine, Model, GoalieID.Value, GoaliPos.Value, Goaliang, new DefenceInfo(), new Position2D(), def1ID, true);
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, def1ID, typeof(DefenderNormalRole1)))
            //    Functions[def1ID.Value] = (eng, wmd) => GetRole<DefenderNormalRole1>(def1ID.Value).Run(eng, wmd, def1ID.Value, Defender1Pos.Value, Defender1ang);
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, def2ID, typeof(DefenderNormalRole2)))
            //    Functions[def2ID.Value] = (eng, wmd) => GetRole<DefenderNormalRole2>(def2ID.Value).Run(eng, wmd, def2ID.Value, Defender2Pos.Value, Defender2ang);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, def1ID, typeof(DefenderCornerRole1)))
                Functions[def1ID.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(def1ID.Value).Run(eng, wmd, def1ID.Value, Defender1Pos.Value, Defender1ang);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, def2ID, typeof(DefenderCornerRole2)))
                Functions[def2ID.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(def2ID.Value).Run(eng, wmd, def2ID.Value, Defender2Pos.Value, Defender2ang);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker1, typeof(DefenderMarkerNormalRole1)))
                Functions[marker1.Value] = (eng, wmd) => GetRole<DefenderMarkerNormalRole1>(marker1.Value).Mark(eng, wmd, marker1.Value, Marker1Pos.Value, Marker1ang);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker2, typeof(DefenderMarkerNormalRole2)))
                Functions[marker2.Value] = (eng, wmd) => GetRole<DefenderMarkerNormalRole2>(marker2.Value).Mark(eng, wmd, marker2.Value, Marker2Pos.Value, Marker2ang);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker3, typeof(DefenderMarkerNormalRole3)))
                Functions[marker3.Value] = (eng, wmd) => GetRole<DefenderMarkerNormalRole3>(marker3.Value).Mark(eng, wmd, marker3.Value, Marker3Pos.Value, Marker3ang);
            #endregion
            #region Select Strategy Ids
            StrategyIds = new List<int>();
            List<int> tmpids = new List<int>();
            tmpids = Model.OurRobots.Keys.ToList();
            if (GoalieID.HasValue)
                tmpids = tmpids.Where(w => w != GoalieID.Value).ToList();
            if (def1ID.HasValue)
                tmpids = tmpids.Where(w => w != def1ID.Value).ToList();
            if (def2ID.HasValue)
                tmpids = tmpids.Where(w => w != def2ID.Value).ToList();
            if (marker1.HasValue)
                tmpids = tmpids.Where(w => w != marker1.Value).ToList();
            if (marker2.HasValue)
                tmpids = tmpids.Where(w => w != marker2.Value).ToList();
            if (marker3.HasValue)
                tmpids = tmpids.Where(w => w != marker3.Value).ToList();
            StrategyIds = tmpids;
            #endregion
            StraIds = StrategyIds.ToList();
            PrevAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        protected T GetRole<T>(int robotID) where T : RoleBase, new()
        {
            if (!PreviouslyAssignedRoles.ContainsKey(robotID))
                PreviouslyAssignedRoles.Add(robotID, typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase);
            else if (!(PreviouslyAssignedRoles[robotID] is T))
                PreviouslyAssignedRoles[robotID] = typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            return (T)PreviouslyAssignedRoles[robotID];
        }
    }

    public enum PlayResult
    {
        InPlay,
        Aborted,
        Success,
        Fail,
        Completed
    }
}
