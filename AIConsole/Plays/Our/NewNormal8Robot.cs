using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles.Defending.Normal;
using MRL.SSL.AIConsole.Roles.Defending;

namespace MRL.SSL.AIConsole.Plays.Our
{
    class MarkerAttackerRole1 : PlayBase
    {
        bool ballIsMoved = false, oppBallOwner = false;
        bool flagFirst = true;
        bool firstSetBall = true;
        int? goalie = null;
        int field = 1, Robotfield;
        int? OppToMarkID1 = null, OppToMarkID2 = null, OppToSt3 = null, attackerID = null, attackerID2 = null;
        int? oppID1, oppID2;
        int lastOppCount = 0;
        int? oppBallOwnerId;
        List<int?> oppValue = new List<int?>();
        List<int> opp = new List<int>();
        List<int> oppValue1 = new List<int>();
        List<int> oppValue2 = new List<int>();
        Dictionary<double, int> Closest;
        Position2D firstBallPos = new Position2D();
        Position2D lastballstate;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        Dictionary<double, int> lastScores;
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            //return false;
            return Status == GameDefinitions.GameStatus.Normal;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {

            Dictionary<double, int?> IDopp = new Dictionary<double, int?>();

            int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            List<DefenderCommand> defence = new List<DefenderCommand>();

            List<int> oppAttackerIds = new List<int>();
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

            double markRegion = -3.5;
            Dictionary<int, float> scores;
            Dictionary<double, int> dd = new Dictionary<double, int>();
            if (goalieID.HasValue)
            {
                scores = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
                scores.OrderByDescending(t => t.Value);
                //oppAttackerIds = scores.Where(w => Model.Opponents[w.Key].Location.X > markRegion && w.Key != goalieID).Select(s => s.Key).ToList();
            }
            else
            {
                scores = engine.GameInfo.OppTeam.Scores;
                scores.OrderByDescending(t => t.Value);
                //oppAttackerIds = scores.Where(w => Model.Opponents[w.Key].Location.X > markRegion && w.Key != goalieID).Select(s => s.Key).ToList();
            }
            if (scores.Count > 7)
            {
                Dictionary<int, float> tempOpp = scores;
                oppAttackerIds = new List<int>();
                for (int i = 0; i < tempOpp.Count - 1; i++)
                {
                    oppAttackerIds.Add(tempOpp.ElementAt(i).Key);
                }
            }
            else
            {
                Dictionary<int, float> tempOpp = scores;
                oppAttackerIds = new List<int>();
                for (int i = 0; i < tempOpp.Count; i++)
                {
                    oppAttackerIds.Add(tempOpp.ElementAt(i).Key);
                }
            }


            if (!oppBallOwner && engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < 0.15)
                oppBallOwner = true;
            if (Model.BallState.Speed.Size < 10 && lastballstate.DistanceFrom(Model.BallState.Location) > 0.07 && oppBallOwner)
            {
                ballIsMoved = true;
            }

            #region Matcher
            RoleBase r;
            roles = new List<RoleInfo>();

            r = typeof(ActiveRole2017).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));


            r = typeof(StaticDefender1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(StaticDefender2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            //r = typeof(staticDefender3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r, 1, 0));
            r = typeof(GerrardRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(NewSupporter2Role).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));
            r = typeof(Marker1Normal8Robot).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(MarkerAttackerRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));



            Dictionary<int, RoleBase> matched;
            if (FreekickDefence.Static1ID.HasValue && !Model.OurRobots.ContainsKey(FreekickDefence.Static1ID.Value))
                FreekickDefence.Static1ID = null;
            if (FreekickDefence.Static2ID.HasValue && !Model.OurRobots.ContainsKey(FreekickDefence.Static2ID.Value))
                FreekickDefence.Static2ID = null;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? goalie = Model.GoalieID;

            int? ActiveID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(ActiveRole2017)))
                ActiveID = matched.Where(w => w.Value.GetType() == typeof(ActiveRole2017)).First().Key;

            int? supportID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(NewSupporter2Role)))
                supportID = matched.Where(w => w.Value.GetType() == typeof(NewSupporter2Role)).First().Key;

            int? st1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StaticDefender1)))
                st1 = matched.Where(w => w.Value.GetType() == typeof(StaticDefender1)).First().Key;

            int? st2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StaticDefender2)))
                st2 = matched.Where(w => w.Value.GetType() == typeof(StaticDefender2)).First().Key;

            int? st3 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(GerrardRole)))
                st3 = matched.Where(w => w.Value.GetType() == typeof(GerrardRole)).First().Key;


            int? Marker1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(Marker1Normal8Robot)))
                Marker1 = matched.Where(w => w.Value.GetType() == typeof(Marker1Normal8Robot)).First().Key;


            int? Marker2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(MarkerAttackerRole2)))
                Marker2 = matched.Where(w => w.Value.GetType() == typeof(MarkerAttackerRole2)).First().Key;

            FreekickDefence.Static1ID = st1;
            FreekickDefence.Static2ID = st2;

            #endregion
            NormalSharedState.CommonInfo.ActiveID = ActiveID;
            NormalSharedState.CommonInfo.SupporterID = supportID;
            NormalSharedState.CommonInfo.AttackerID = Marker1;

            #region Assigner

            ballIsMoved = Model.BallState.Location.DistanceFrom(firstBallPos) > .06;


            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(StaticGoalieRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<StaticGoalieRole>(goalie.Value).perform(eng, wmd, goalie.Value, (st1.HasValue) ? first.TargetState : Model.BallState, st1, st2);
            DefenceTest.WeHaveGoalie = true;
            
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ActiveID, typeof(ActiveRole2017)))
                Functions[ActiveID.Value] = (eng, wmd) => GetRole<ActiveRole2017>(ActiveID.Value).Perform(engine, Model, ActiveID.Value, false);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, st1, typeof(StaticDefender1)))
                Functions[st1.Value] = (eng, wmd) => GetRole<StaticDefender1>(st1.Value).Run(engine, Model, st1.Value, first.DefenderPosition.Value, first.Teta, CurrentlyAssignedRoles);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, st2, typeof(StaticDefender2)))
                Functions[st2.Value] = (eng, wmd) => GetRole<StaticDefender2>(st2.Value).Run(engine, Model, st2.Value, second.DefenderPosition.Value, second.Teta, CurrentlyAssignedRoles);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, st3, typeof(GerrardRole)))
                Functions[st3.Value] = (eng, wmd) => GetRole<GerrardRole>(st3.Value).Perform(engine, Model, st3.Value);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, supportID, typeof(NewSupporter2Role)))
                Functions[supportID.Value] = (eng, wmd) => GetRole<NewSupporter2Role>(supportID.Value).Perform(eng, wmd, supportID.Value);

            
            #region bullshit
            if (Model.BallState.Location.Y > 0.18)
            {
                field = 1;
            }
            if (Model.BallState.Location.Y < -0.18)
            {
                field = 2;
            }
            if (Model.BallState.Location.Y >= -0.18 && Model.BallState.Location.Y <= 0.18)
            {
                if (field == 1)
                {
                    field = 1;
                }
                if (field == 2)
                {
                    field = 2;
                }
            }


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
                if (Model.Opponents[oppAttackerIds[i]].Location.Y >= -0.18 && Model.Opponents[oppAttackerIds[i]].Location.Y <= 0.18)
                {
                    if (Robotfield == 1)
                    {
                        Robotfield = 1;
                    }
                    if (Robotfield == 2)
                    {
                        Robotfield = 2;
                    }
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
                OppToMarkID1=0;
                OppToMarkID2=0;
                OppToSt3 = 0;
            }
            if (oppAttackerIds.Count !=0 && oppValue1.Count == 0 && oppValue2.Count == 0)
            {
                OppToMarkID1 = oppAttackerIds[0];
                OppToMarkID2 = oppAttackerIds[0];
                OppToSt3 = oppAttackerIds[0];
            }
            if (oppValue1.Count == 1 && oppValue2.Count == 0)
            {
                OppToMarkID1 = oppValue1[0];
                //midd2
                OppToMarkID2 = oppValue1[0];
                OppToSt3 = oppValue1[0];
            }
            if (oppValue1.Count >= 2 && oppValue2.Count == 0)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue1[1];
                OppToSt3 = oppValue1[1];
            }
            if (oppValue1.Count == 0 && oppValue2.Count == 1)
            {
                //midd1
                OppToMarkID1 = oppValue2[0];
                OppToMarkID2 = oppValue2[0];
                OppToSt3 = oppValue2[0];
            }
            if (oppValue1.Count >= 1 && oppValue2.Count == 1)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue2[0];
                OppToSt3 = oppValue2[0];
            }
            if (oppValue1.Count == 0 && oppValue2.Count == 2)
            {
                //mid1
                OppToMarkID1 = oppValue2[0];
                OppToMarkID2 = oppValue2[0];
                OppToSt3 = oppValue2[1];
            }
            if (oppValue1.Count >= 1 && oppValue2.Count == 2)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue2[0];
                OppToSt3 = oppValue2[1];
            }
            if (oppValue1.Count == 0 && oppValue2.Count >= 3)
            {
                OppToMarkID1 = oppValue2[1];
                OppToMarkID2 = oppValue2[0];
                OppToSt3 = oppValue2[2];
            }
            if (oppValue1.Count >= 1 && oppValue2.Count >= 3)
            {
                OppToMarkID1 = oppValue1[0];
                OppToMarkID2 = oppValue2[0];
                OppToSt3 = oppValue2[1];
            }
            //for (int o = 0; o < oppValue1.Count; o++)
            //{
            //    if (Model.Opponents[oppValue1[o]].Location.X < -0.3)
            //    {
            //        oppValue1.Remove(oppValue1[o]);
            //    }
            //}
            //for (int oo = 0; oo < oppValue2.Count; oo++)
            //{
            //    if (Model.Opponents[oppValue2[oo]].Location.X < -0.3)
            //    {
            //        oppValue2.Remove(oppValue2[oo]);
            //    }
            //}
            #endregion



            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Marker1, typeof(Marker1Normal8Robot)))
                Functions[Marker1.Value] = (eng, wmd) => GetRole<Marker1Normal8Robot>(Marker1.Value).Perform(engine, Model, Marker1.Value, markRegion, OppToMarkID1, oppAttackerIds, oppValue1, oppValue2, field);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Marker2, typeof(MarkerAttackerRole2)))
                Functions[Marker2.Value] = (eng, wmd) => GetRole<MarkerAttackerRole2>(Marker2.Value).Perform(engine, Model, Marker2.Value, markRegion, OppToMarkID2, oppAttackerIds, oppValue1, oppValue2, field);

            //else if (Model.BallState.Location.X > 0.6 && !oppBallOwner)
            //{
            //    OppToMarkID1 = attackerID;
            //    OppToMarkID2 = attackerID2;
            //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attackerID, typeof(NewAttackerRole)))
            //        Functions[attackerID.Value] = (eng, wmd) => GetRole<NewAttackerRole>(attackerID.Value).Perform(engine, Model, attackerID.Value);

            //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attackerID2, typeof(NewAttacker2Role)))
            //        Functions[attackerID2.Value] = (eng, wmd) => GetRole<NewAttacker2Role>(attackerID2.Value).Perform(engine, Model, attackerID2.Value);
            //    if (opp.Count <= 3)
            //    {
            //        attacker
            //        OppToMarkID1 = attackerID;
            //        OppToMarkID2 = attackerID2;
            //        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attackerID, typeof(NewAttackerRole)))
            //            Functions[attackerID.Value] = (eng, wmd) => GetRole<NewAttackerRole>(attackerID.Value).Perform(engine, Model, attackerID.Value);

            //        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attackerID2, typeof(NewAttacker2Role)))
            //            Functions[attackerID2.Value] = (eng, wmd) => GetRole<NewAttacker2Role>(attackerID2.Value).Perform(engine, Model, attackerID2.Value);

            //    }
            //    else if (opp.Count > 3)
            //    {
            //        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Marker1, typeof(Marker1Normal8Robot)))
            //            Functions[Marker1.Value] = (eng, wmd) => GetRole<Marker1Normal8Robot>(Marker1.Value).Perform(engine, Model, Marker1.Value, markRegion, OppToMarkID1, oppAttackerIds, oppValue1, oppValue2);

            //        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Marker2, typeof(Marker2Normal8Robot)))
            //            Functions[Marker2.Value] = (eng, wmd) => GetRole<Marker2Normal8Robot>(Marker2.Value).Perform(engine, Model, Marker2.Value, markRegion, OppToMarkID2, oppAttackerIds, oppValue1, oppValue2);
            //    }
            //}

            #endregion
            FreekickDefence.CalculateStaticPos(engine, Model, CurrentlyAssignedRoles);
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return PreviouslyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            throw new NotImplementedException();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            field = 1;
            //Robotfield = null;
            lastOppCount = 0;
            firstSetBall = true;
            flagFirst = true;
            OppToMarkID1 = null;
            OppToMarkID2 = null;
            OppToSt3 = null;
            oppValue.Clear();
            opp = new List<int>();
            oppValue1 = new List<int>();
            oppValue2 = new List<int>();
            PreviouslyAssignedRoles.Clear();
            NormalSharedState.CommonInfo.Reset();
            foreach (var item in engine.ImplementedActions)
            {
                item.Value.Reset();
            }
        }
    }
}
