using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Plays
{
    public class NewNormalPlay : PlayBase
    {
        bool Debug = true;
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            //return false;
            return Status == GameDefinitions.GameStatus.Normal;
        }
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            List<DefenderCommand> defence = new List<DefenderCommand>();

            DefenceTest.BallTest = FreekickDefence.testDefenceState;
            DefenceTest.GenerateBallPos();
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
            FreekickDefence.ballState = ballState;
            FreekickDefence.ballStateFast = ballStateFast;

            defence.Add(new DefenderCommand()
            {
                RoleType = typeof(StaticDefender1)
            });
            defence.Add(new DefenderCommand()
            {
                RoleType = typeof(StaticDefender2)
            });

            List<DefenceInfo> list = FreekickDefence.MatchStatic(engine, Model, defence);

            var first = list.First(f => f.RoleType == typeof(StaticDefender1));
            var second = list.First(f => f.RoleType == typeof(StaticDefender2));
            List<int> activeIDs = new List<int>();

            if (NormalSharedState.CommonInfo.ActiveID.HasValue)
                activeIDs.Add(NormalSharedState.CommonInfo.ActiveID.Value);
            if (NormalSharedState.CommonInfo.SupporterID.HasValue)
                activeIDs.Add(NormalSharedState.CommonInfo.SupporterID.Value);
            if (NormalSharedState.CommonInfo.PickerID.HasValue)
                activeIDs.Add(NormalSharedState.CommonInfo.PickerID.Value);

            int pID;
            double dist, DistFromBorder;
            if (NormalSharedState.CommonInfo.IsPickingFeasible(engine, Model, activeIDs, out pID) && Model.BallState.Location.X < 1.5 //!GameParameters.IsInDangerousZone(Model.BallState.Location,true,0.1,out dist,out DistFromBorder)
                && ActiveParameters.NewActiveParameters.UseSpaceDrible
                && (ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.Pass || ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.PassAndDribble))
            {
                if (Debug)
                {
                    DrawingObjects.AddObject(new Circle(Position2D.Zero, 0.25), "circleppp");
                    DrawingObjects.AddObject(new StringDraw(pID.ToString(), Position2D.Zero), "idppp");
                }
                NormalSharedState.CommonInfo.PickerID = pID;
                NormalSharedState.CommonInfo.PickIsFeasible = true;
            }
            else
                NormalSharedState.CommonInfo.PickIsFeasible = false;



            #region Matcher
            RoleBase r;
            roles = new List<RoleInfo>();

            //r = typeof(ActiveRole2017).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(StaticDefender1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(StaticDefender2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(GerrardRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));



            //if (!NormalSharedState.CommonInfo.AttackerMode)
            //{
            //    r = typeof(StaticDefender2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //    roles.Add(new RoleInfo(r, 1, 0));
            //}


            //if (NormalSharedState.CommonInfo.PickIsFeasible)
            //{
            //    r = typeof(NewPickerRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //    roles.Add(new RoleInfo(r, 1, 0.1));
            //}
            //else
            //{
            //    r = typeof(NewSupporter2Role).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //    roles.Add(new RoleInfo(r, 1, 0.1));
            //}



            //r = typeof(NewAttacker2Role).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r, 0.1, 0));

            //r = typeof(NewAttackerRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r, 0.1, 0));

            //if (!NormalSharedState.CommonInfo.AttackerMode)
            //{
            //    r = typeof(NewAttackerRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //    roles.Add(new RoleInfo(r, 0.1, 0));
            //}
            //else
            //{
            //    r = typeof(NewAttacker3Role).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //    roles.Add(new RoleInfo(r, 0.1, 0));

            //    r = typeof(NewAttacker2Role).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //    roles.Add(new RoleInfo(r, 0.1, 0));
            //}
            //r = typeof(NewRegionalRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r, 0.1, 0.1));
            //      GameParameters.SafeRadi(new SingleObjectState(new Position2D(2.5, 0), Vector2D.Zero, 0), 0);

            Dictionary<int, RoleBase> matched;

            //matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);
            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? goalie = Model.GoalieID;

            int? getballID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(ActiveRole2017)))
                getballID = matched.Where(w => w.Value.GetType() == typeof(ActiveRole2017)).First().Key;

            int? supportID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(NewSupporter2Role)))
                supportID = matched.Where(w => w.Value.GetType() == typeof(NewSupporter2Role)).First().Key;
            //if (matched.Any(w => w.Value.GetType() == typeof(NewMarkerRole)))
            //    supportID = matched.Where(w => w.Value.GetType() == typeof(NewMarkerRole)).First().Key;



            int? pickerID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(NewPickerRole)))
                pickerID = matched.Where(w => w.Value.GetType() == typeof(NewPickerRole)).First().Key;

            int? attackerID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(NewAttackerRole)))
                attackerID = matched.Where(w => w.Value.GetType() == typeof(NewAttackerRole)).First().Key;
            //if (matched.Any(w => w.Value.GetType() == typeof(AttackerRole1)))
            //    attackerID = matched.Where(w => w.Value.GetType() == typeof(AttackerRole1)).First().Key;


            int? attacke2ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(NewAttacker2Role)))
                attacke2ID = matched.Where(w => w.Value.GetType() == typeof(NewAttacker2Role)).First().Key;
            //int? attacker3ID = null;
            //if (matched.Any(w => w.Value.GetType() == typeof(NewAttacker3Role)))
            //    attacker3ID = matched.Where(w => w.Value.GetType() == typeof(NewAttacker3Role)).First().Key;

            int? st1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StaticDefender1)))
                st1 = matched.Where(w => w.Value.GetType() == typeof(StaticDefender1)).First().Key;

            int? st2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StaticDefender2)))
                st2 = matched.Where(w => w.Value.GetType() == typeof(StaticDefender2)).First().Key;


            int? gerrard = null;
            if (matched.Any(w => w.Value.GetType() == typeof(GerrardRole)))
                gerrard = matched.Where(w => w.Value.GetType() == typeof(GerrardRole)).First().Key;

            int? nrg = null;
            if (matched.Any(w => w.Value.GetType() == typeof(NewRegionalRole)))
                nrg = matched.Where(w => w.Value.GetType() == typeof(NewRegionalRole)).First().Key;

            FreekickDefence.Static1ID = st1;
            FreekickDefence.Static2ID = st2;

            #endregion
            NormalSharedState.CommonInfo.ActiveID = getballID;
            NormalSharedState.CommonInfo.SupporterID = supportID;
            if (false && !NormalSharedState.CommonInfo.AttackerMode)
            {
                NormalSharedState.CommonInfo.AttackerID = attackerID;
            }
            else
                NormalSharedState.CommonInfo.AttackerID = attacke2ID;

            NormalSharedState.CommonInfo.PickerID = pickerID;

            #region Assigner
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(StaticGoalieRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<StaticGoalieRole>(goalie.Value).perform(eng, wmd, goalie.Value, (st1.HasValue) ? first.TargetState : Model.BallState, st1, st2);

            //if (goalie.HasValue && goalie != null)
            //    DefenceTest.GoalieRole = Model.OurRobots[goalie.Value].Location;


            DefenceTest.WeHaveGoalie = true;
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, getballID, typeof(ActiveRole2017)))
            {
                Functions[getballID.Value] = (eng, wmd) => GetRole<ActiveRole2017>(getballID.Value).Perform(engine, Model, getballID.Value, false);
            }
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, st1, typeof(StaticDefender1)))
                Functions[st1.Value] = (eng, wmd) => GetRole<StaticDefender1>(st1.Value).Run(engine, Model, st1.Value, first.DefenderPosition.Value, first.Teta, CurrentlyAssignedRoles);
            if (st1.HasValue && Model.OurRobots.ContainsKey(st1.Value))
                DefenceTest.DefenderStaticRole1 = Model.OurRobots[st1.Value].Location;
            DefenceTest.WeHaveDefenderStaticRole1 = true;

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, st2, typeof(StaticDefender2)))
                Functions[st2.Value] = (eng, wmd) => GetRole<StaticDefender2>(st2.Value).Run(engine, Model, st2.Value, second.DefenderPosition.Value, second.Teta, CurrentlyAssignedRoles);


            if (st2.HasValue && Model.OurRobots.ContainsKey(st2.Value))
                DefenceTest.DefenderStaticRole2 = Model.OurRobots[st2.Value].Location;

            DefenceTest.WeHaveDefenderStaticRole2 = true;
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, supportID, typeof(NewSupporter2Role)))
                Functions[supportID.Value] = (eng, wmd) => GetRole<NewSupporter2Role>(supportID.Value).Perform(eng, wmd, supportID.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacke2ID, typeof(NewAttacker2Role)))
                Functions[attacke2ID.Value] = (eng, wmd) => GetRole<NewAttacker2Role>(attacke2ID.Value).Perform(eng, wmd, attacke2ID.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, gerrard, typeof(GerrardRole)))
                Functions[gerrard.Value] = (eng, wmd) => GetRole<GerrardRole>(gerrard.Value).Perform(engine, Model, gerrard.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, pickerID, typeof(NewPickerRole)))
            {
                NormalSharedState.CommonInfo.IsPicking = true;
                Functions[pickerID.Value] = (eng, wmd) => GetRole<NewPickerRole>(pickerID.Value).Perform(engine, Model, pickerID.Value);
            }
            else
            {
                NormalSharedState.CommonInfo.IsPicking = false;
                NormalSharedState.CommonInfo.PickIsFeasible = false;
            }
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attackerID, typeof(NewAttackerRole)))
                Functions[attackerID.Value] = (eng, wmd) => GetRole<NewAttackerRole>(attackerID.Value).Perform(eng, wmd, attackerID.Value);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacke2ID, typeof(NewAttacker2Role)))
            //    Functions[attacke2ID.Value] = (eng, wmd) => GetRole<NewAttacker2Role>(attacke2ID.Value).Perform(eng, wmd, attacke2ID.Value);
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacker3ID, typeof(NewAttacker3Role)))
            //    Functions[attacker3ID.Value] = (eng, wmd) => GetRole<NewAttacker3Role>(attacker3ID.Value).Perform(eng, wmd, attacker3ID.Value);
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, nrg, typeof(NewRegionalRole)))
            //    Functions[ nrg.Value] = (eng, wmd) => GetRole<NewRegionalRole>( nrg.Value).Perform(engine,Model,FreekickDefence.Static1ID.Value,FreekickDefence.Static2ID.Value,nrg.Value);
            #endregion
            FreekickDefence.CalculateStaticPos(engine, Model, CurrentlyAssignedRoles);
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            DefenceTest.MakeOutPut();
            return PreviouslyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            throw new NotImplementedException();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
            NormalSharedState.CommonInfo.Reset();
            foreach (var item in engine.ImplementedActions)
            {
                item.Value.Reset();
            }
        }
    }
}
