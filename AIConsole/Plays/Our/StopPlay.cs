using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Drawing;

namespace MRL.SSL.AIConsole.Plays
{
    class StopPlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            return Status == GameDefinitions.GameStatus.Stop;
            return false;
        }
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            DefenceTest.BallTest = FreekickDefence.testDefenceState;
            DefenceTest.MakeOutPut();
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            //if (engine.GameInfo.OurTeam.BallOwner.HasValue)
            //{
            //    DrawingObjects.AddObject(new StringDraw(Model.BallState.Location.DistanceFrom(Model.OurRobots[3].Location).ToString(),new Position2D (2,2)));
            //    DrawingObjects.AddObject(new Circle(Model.OurRobots[engine.GameInfo.OurTeam.BallOwner.Value].Location, .15, new Pen(Color.DarkOrange, .02f)));
            //}
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            NormalDefenceAssigner def = new Engine.NormalDefenceAssigner();
            Dictionary<RoleBase, Position2D?> Positions = new Dictionary<RoleBase, Position2D?>();
            Dictionary<RoleBase, double> Angles = new Dictionary<RoleBase, double>();
            bool isInDangerZone = false;
            double d, d2;
            RoleBase rt;
            List<RoleInfo> roles = new List<RoleInfo>();
            if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.2, out d, out d2))
                isInDangerZone = true;
            if (!isInDangerZone)
            {
                def.Assign(engine, Model, out Positions, out Angles, false, false, false, false);
                rt = typeof(DefenderNormalRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
                rt = typeof(DefenderNormalRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
            }
            else
            {
                rt = typeof(DefenderStopRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
                rt = typeof(DefenderStopRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
            }

            rt = typeof(StopRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(StopRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(StopRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? Defender1ID = null;

            if (!isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(DefenderNormalRole1)))
                Defender1ID = matched.Where(w => w.Value.GetType() == typeof(DefenderNormalRole1)).First().Key;
            else if (isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(DefenderStopRole1)))
                Defender1ID = matched.Where(w => w.Value.GetType() == typeof(DefenderStopRole1)).First().Key;

            int? Defender2ID = null;
            if (!isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(DefenderNormalRole2)))
                Defender2ID = matched.Where(w => w.Value.GetType() == typeof(DefenderNormalRole2)).First().Key;
            else if (isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(DefenderStopRole2)))
                Defender2ID = matched.Where(w => w.Value.GetType() == typeof(DefenderStopRole2)).First().Key;

            int? stop1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StopRole1)))
                stop1 = matched.Where(w => w.Value.GetType() == typeof(StopRole1)).First().Key;

            int? stop2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StopRole2)))
                stop2 = matched.Where(w => w.Value.GetType() == typeof(StopRole2)).First().Key;

            int? stop3 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StopRole3)))
                stop3 = matched.Where(w => w.Value.GetType() == typeof(StopRole3)).First().Key;

            if (!isInDangerZone)
            {
                Position2D GoaliPos = new Position2D();
                double gteta = 0;
                if (Positions.Any(w => w.Key.GetType() == typeof(GoalieNormalRole)))
                    GoaliPos = Positions.Where(w => w.Key.GetType() == typeof(GoalieNormalRole)).First().Value.Value;
                if (Angles.Any(w => w.Key.GetType() == typeof(GoalieNormalRole)))
                    gteta = Angles.Where(w => w.Key.GetType() == typeof(GoalieNormalRole)).First().Value;

                Position2D Def1Pos = new Position2D();
                double d1teta = 0;
                if (Positions.Any(w => w.Key.GetType() == typeof(DefenderNormalRole1)))
                    Def1Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderNormalRole1)).First().Value.Value;
                if (Angles.Any(w => w.Key.GetType() == typeof(DefenderNormalRole1)))
                    d1teta = Angles.Where(w => w.Key.GetType() == typeof(DefenderNormalRole1)).First().Value;

                Position2D Def2Pos = new Position2D();
                double d2teta = 0;
                if (Positions.Any(w => w.Key.GetType() == typeof(DefenderNormalRole2)))
                    Def2Pos = Positions.Where(w => w.Key.GetType() == typeof(DefenderNormalRole2)).First().Value.Value;
                if (Angles.Any(w => w.Key.GetType() == typeof(DefenderNormalRole2)))
                    d2teta = Angles.Where(w => w.Key.GetType() == typeof(DefenderNormalRole2)).First().Value;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieNormalRole)))
                    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieNormalRole>(Model.GoalieID.Value).RunStop(eng, wmd, Model.GoalieID.Value, GoaliPos, gteta);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderNormalRole1)))
                    Functions[Defender1ID.Value] = (eng, wmd) => GetRole<DefenderNormalRole1>(Defender1ID.Value).RunStop(eng, wmd, Defender1ID.Value, Def1Pos, d1teta);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(DefenderNormalRole2)))
                    Functions[Defender2ID.Value] = (eng, wmd) => GetRole<DefenderNormalRole2>(Defender2ID.Value).RunStop(eng, wmd, Defender2ID.Value, Def2Pos, d2teta);
            }
            else
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieStopRole)))
                    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieStopRole>(Model.GoalieID.Value).PositioningStop(eng, wmd, Model.GoalieID.Value);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderStopRole1)))
                    Functions[Defender1ID.Value] = (eng, wmd) => GetRole<DefenderStopRole1>(Defender1ID.Value).PositioningStop(engine, Model, Defender1ID.Value, true, 150);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(DefenderStopRole2)))
                    Functions[Defender2ID.Value] = (eng, wmd) => GetRole<DefenderStopRole2>(Defender2ID.Value).PositioningStop(engine, Model, Defender2ID.Value, true, 150);
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop1, typeof(StopRole1)))
                Functions[stop1.Value] = (eng, wmd) => GetRole<StopRole1>(stop1.Value).RunRoleStop(eng, wmd, stop1.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop2, typeof(StopRole2)))
                Functions[stop2.Value] = (eng, wmd) => GetRole<StopRole2>(stop2.Value).RunRoleStop(eng, wmd, stop2.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop3, typeof(StopRole3)))
                Functions[stop3.Value] = (eng, wmd) => GetRole<StopRole3>(stop3.Value).RunRoleStop(eng, wmd, stop3.Value);

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return null;
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
            FreekickDefence.BallIsMoved = false;
            FreekickDefence.BallIsMovedStrategy = false;
            FreekickDefence.RestartActiveFlags();
            PreviouslyAssignedRoles.Clear();
        }
    }
}
