using MRL.SSL.AIConsole.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Plays.Our
{
    class AUNormalPlay : PlayBase
    {
        int? OppToMarkID1 = null, OppToMarkID2 = null;
        int field = 1, Robotfield;
        bool Debug = true;
        List<int> oppValue1 = new List<int>();
        List<int> oppValue2 = new List<int>();
        List<int> opp = new List<int>();
        const double markRegion = -3.5;
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            //return false;
            return Status == GameDefinitions.GameStatus.Normal;
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

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            throw new NotImplementedException();
        }
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();

            List<int> oppAttackerIds = new List<int>();
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
            
            List<int> activeIDs = new List<int>();
            int pID;
            if (NormalSharedState.CommonInfo.IsPickingFeasible(engine, Model, activeIDs, out pID) && Model.BallState.Location.X < 1.5 //!GameParameters.IsInDangerousZone(Model.BallState.Location,true,0.1,out dist,out DistFromBorder)
                && ActiveParameters.NewActiveParameters.UseSpaceDrible
                && (ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.Pass
                || ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.PassAndDribble))
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
            //roles.Add(new RoleInfo(r, 0, 0.04));

            r = typeof(OnLineRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));


            r = typeof(OnLineRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            //r = typeof(NewSupporter2Role).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r,.1, 0.1));



            //r = typeof(Marker1Normal8Robot).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r, 0.1, 0));

            r = typeof(OnLineRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(VandersarGoalKeeperRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

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


            
            int? attackerID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(Marker1Normal8Robot)))
                attackerID = matched.Where(w => w.Value.GetType() == typeof(Marker1Normal8Robot)).First().Key;
            //if (matched.Any(w => w.Value.GetType() == typeof(AttackerRole1)))
            //    attackerID = matched.Where(w => w.Value.GetType() == typeof(AttackerRole1)).First().Key;


            int? attacker2ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(Marker2Normal8Robot)))
                attacker2ID = matched.Where(w => w.Value.GetType() == typeof(Marker2Normal8Robot)).First().Key;
            int? on1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OnLineRole1)))
                on1 = matched.Where(w => w.Value.GetType() == typeof(OnLineRole1)).First().Key;

            int? on2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OnLineRole2)))
                on2 = matched.Where(w => w.Value.GetType() == typeof(OnLineRole2)).First().Key;
            int? staticDef3 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(OnLineRole3)))
                staticDef3 = matched.Where(w => w.Value.GetType() == typeof(OnLineRole3)).First().Key;
            //FreekickDefence.Static1ID = st1;
            //FreekickDefence.Static2ID = st2;


            #endregion
            NormalSharedState.CommonInfo.ActiveID = getballID;
            NormalSharedState.CommonInfo.SupporterID = supportID;
            NormalSharedState.CommonInfo.AttackerID = attackerID;
            #region BullShits
            for (int i = 0; i < oppAttackerIds.Count; i++)
            {
                if (Model.Opponents[oppAttackerIds[i]].Location.Y > 0.18)
                {
                    Robotfield = 1;
                }
                if (Model.Opponents[oppAttackerIds[i]].Location.Y < -0.18)
                {
                    Robotfield = 2;
                }
                
            }
            //if (field.HasValue && Robotfield.HasValue)
            //{
            if (Model.BallState.Location.X > -0.6 && field == 1)
            {
                opp = new List<int>();
                oppValue1 = new List<int>();
                oppValue2 = new List<int>();
                for (int i = 0; i < oppAttackerIds.Count; i++)
                {
                    if (Model.Opponents[oppAttackerIds[i]].Location.X > -0.3)
                    {
                        opp.Add(oppAttackerIds[i]);
                    }
                }
                if (Robotfield == 1)
                {

                    for (int j = 0; j < opp.Count; j++)
                    {
                        if (Model.Opponents[opp[j]].Location.Y > 0.18 && Model.Opponents[opp[j]].Location.X > -0.3
                            && Model.Opponents[opp[j]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue1.Add(opp[j]);
                        }
                    }
                    for (int k = 0; k < opp.Count; k++)
                    {

                        if (Model.Opponents[opp[k]].Location.Y < -0.18 && Model.Opponents[opp[k]].Location.X > -0.3
                            && Model.Opponents[opp[k]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue2.Add(opp[k]);
                        }
                    }
                }
                if (Robotfield == 2)
                {
                    for (int j = 0; j < opp.Count; j++)
                    {
                        if (Model.Opponents[opp[j]].Location.Y > 0.18 && Model.Opponents[opp[j]].Location.X > -0.3
                            && Model.Opponents[opp[j]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue1.Add(opp[j]);
                        }
                    }
                    for (int k = 0; k < opp.Count; k++)
                    {
                        if (Model.Opponents[opp[k]].Location.Y < -0.18 && Model.Opponents[opp[k]].Location.X > -0.3
                            && Model.Opponents[opp[k]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue2.Add(opp[k]);
                        }
                    }
                }
            }
            //}
            //if (field.HasValue && Robotfield.HasValue)
            //{
            if (Model.BallState.Location.X > -0.6 && field == 2)
            {
                // oppAttackerIds = new List<int>();
                opp = new List<int>();
                oppValue1 = new List<int>();
                oppValue2 = new List<int>();
                for (int i = 0; i < oppAttackerIds.Count; i++)
                {
                    if (Model.Opponents[oppAttackerIds[i]].Location.X > -0.3)
                    {
                        opp.Add(oppAttackerIds[i]);
                    }
                }
                if (Robotfield == 1)
                {

                    for (int j = 0; j < opp.Count; j++)
                    {
                        if (Model.Opponents[opp[j]].Location.Y < -0.18 && Model.Opponents[opp[j]].Location.X > -0.3
                            && Model.Opponents[opp[j]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue1.Add(opp[j]);
                        }
                    }
                    for (int k = 0; k < opp.Count; k++)
                    {

                        if (Model.Opponents[opp[k]].Location.Y > 0.18 && Model.Opponents[opp[k]].Location.X > -0.3
                            && Model.Opponents[opp[k]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue2.Add(opp[k]);
                        }
                    }
                }
                if (Robotfield == 2)
                {
                    for (int j = 0; j < opp.Count; j++)
                    {
                        if (Model.Opponents[opp[j]].Location.Y < -0.18 && Model.Opponents[opp[j]].Location.X > -0.3
                            && Model.Opponents[opp[j]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue1.Add(opp[j]);
                        }
                    }
                    for (int k = 0; k < opp.Count; k++)
                    {
                        if (Model.Opponents[opp[k]].Location.Y > 0.18 && Model.Opponents[opp[k]].Location.X > -0.3
                            && Model.Opponents[opp[k]].Location.DistanceFrom(Model.BallState.Location) > 0.8)
                        {
                            oppValue2.Add(opp[k]);
                        }
                    }
                }
            }
            //}
            if (oppAttackerIds.Count == 0)
            {
                OppToMarkID1 = 0;
                OppToMarkID2 = 0;
            }
            if (oppAttackerIds.Count != 0 && oppValue1.Count == 0 && oppValue2.Count == 0)
            {
                OppToMarkID1 = oppAttackerIds[0];
                OppToMarkID2 = oppAttackerIds[0];
            }
            if (oppValue1.Count == 1 && oppValue2.Count == 0)
            {
                OppToMarkID1 = oppValue1[0];
                //midd2
                OppToMarkID2 = oppValue1[0];
            }
            if (oppValue1.Count >= 2 && oppValue2.Count == 0)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue1[1];
            }
            if (oppValue1.Count == 0 && oppValue2.Count == 1)
            {
                //midd1
                OppToMarkID1 = oppValue2[0];
                OppToMarkID2 = oppValue2[0];
            }
            if (oppValue1.Count >= 1 && oppValue2.Count == 1)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue2[0];
            }
            if (oppValue1.Count == 0 && oppValue2.Count == 2)
            {
                //mid1
                OppToMarkID1 = oppValue2[0];
                OppToMarkID2 = oppValue2[0];
            }
            if (oppValue1.Count >= 1 && oppValue2.Count == 2)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue2[0];
            }
            if (oppValue1.Count == 0 && oppValue2.Count >= 3)
            {
                OppToMarkID1 = oppValue2[1];
                OppToMarkID2 = oppValue2[0];
            }
            if (oppValue1.Count >= 1 && oppValue2.Count >= 3)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue2[0];
            }
            NormalSharedState.CommonInfo.OnlineRole1Id = on1.HasValue? on1.Value : -1 ;
            NormalSharedState.CommonInfo.OnlineRole2Id = on2.HasValue ? on2.Value : -1;

            #endregion
            #region Assigner
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(VandersarGoalKeeperRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<VandersarGoalKeeperRole>(goalie.Value).Run(engine, Model, goalie.Value);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, getballID, typeof(ActiveRole2017)))
            //    Functions[getballID.Value] = (eng, wmd) => GetRole<ActiveRole2017>(getballID.Value).Perform(engine, Model, getballID.Value, false);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, on1, typeof(OnLineRole1)))
                Functions[on1.Value] = (eng, wmd) => GetRole<OnLineRole1>(on1.Value).Perform(engine,Model, on1.Value);


            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, on2, typeof(OnLineRole2)))
                Functions[on2.Value] = (eng, wmd) => GetRole<OnLineRole2>(on2.Value).Perform(engine, Model, on2.Value);
          
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, supportID, typeof(NewSupporter2Role)))
            //    Functions[supportID.Value] = (eng, wmd) => GetRole<NewSupporter2Role>(supportID.Value).Perform(eng, wmd, supportID.Value);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attackerID, typeof(Marker1Normal8Robot)))
            //    Functions[attackerID.Value] = (eng, wmd) => GetRole<Marker1Normal8Robot>(attackerID.Value).Perform(engine, Model, attackerID.Value, markRegion, OppToMarkID1, oppAttackerIds, oppValue1, oppValue2, field);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacker2ID, typeof(Marker2Normal8Robot)))
            //    Functions[attacker2ID.Value] = (eng, wmd) => GetRole<Marker2Normal8Robot>(attacker2ID.Value).Perform(engine, Model, attacker2ID.Value, markRegion, OppToMarkID2, oppAttackerIds, oppValue1, oppValue2, field);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, staticDef3, typeof(OnLineRole3)))
                Functions[staticDef3.Value] = (eng, wmd) => GetRole<OnLineRole3>(staticDef3.Value).Perform(engine,Model, staticDef3.Value);
            #endregion
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            //DefenceTest.MakeOutPut();
            return PreviouslyAssignedRoles;
        }
    }
}
