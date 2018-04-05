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

namespace MRL.SSL.AIConsole.Plays.Our
{
    class NormalPlay : PlayBase
    {
        bool ballIsMoved = false, oppBallOwner = false;
        private static bool weHaveActive = false, weHaveAttacker = false;
        bool flagFirst = true;
        bool firstSetBall = true;
        int lastOppCount = 0;
        int? oppBallOwnerId, goalie = null, OppToMarkID1 = null, OppToMarkID2 = null, OppToMarkID3 = null, OppToMarkID4 = null, OppToMarkID5 = null, OppToMarkID6 = null, OppToMarkID7 = null, OppToMarkID8 = null;
        List<int?> oppValue = new List<int?>();
        List<int?> scores1 = new List<int?>();
        Dictionary<int, float> lastScores;
        Dictionary<int, float> scores;
        Position2D firstBallPos = new Position2D();
        Position2D lastballstate;
        Position2D ww;
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            //return false;
            return Status == GameDefinitions.GameStatus.Normal;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            //Separating the goalkeeper
            int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;

            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            List<int> oppAttackerIds = new List<int>();
            double markRegion = -3.5;

            List<DefenderCommand> defence = new List<DefenderCommand>();
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

            #region matcher
            RoleBase r;
            roles = new List<RoleInfo>();

            //8robot
            r = typeof(ActiveRole2017).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(SupporterRole).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(StaticDefender1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(StaticDefender2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            //attackerjadid ro bezar
            if (!oppBallOwner && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter))
            {
                r = typeof(NormalAttacker1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));

                r = typeof(NormalAttacker2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));
            }
            else if (oppAttackerIds.Count <= 0)
            {
                r = typeof(NormalAttacker1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));

                r = typeof(NormalAttacker2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));
            }
            else if (oppAttackerIds.Count > 0 && oppAttackerIds.Count <= 1)
            {
                r = typeof(NormalMarkerRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));
                r = typeof(NormalAttacker1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));
            }
            else
            {
                r = typeof(NormalMarkerRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));

                r = typeof(NormalMarkerRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(r, 10, 0.04));
            }

            //r = typeof(RegionalDefenceUtils).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(r, 10, 0.04));



            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            goalie = Model.GoalieID;

            int? ActiveID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(ActiveRole2017)))
                ActiveID = matched.Where(w => w.Value.GetType() == typeof(ActiveRole2017)).First().Key;

            int? SupporterID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(SupporterRole)))
                SupporterID = matched.Where(w => w.Value.GetType() == typeof(SupporterRole)).First().Key;

            int? Defender1ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StaticDefender1)))
                Defender1ID = matched.Where(w => w.Value.GetType() == typeof(StaticDefender1)).First().Key;

            int? Defender2ID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(StaticDefender2)))
                Defender2ID = matched.Where(w => w.Value.GetType() == typeof(StaticDefender2)).First().Key;

            int? RegionalID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(RegionalDefenceUtils)))
                RegionalID = matched.Where(w => w.Value.GetType() == typeof(RegionalDefenceUtils)).First().Key;

            int? AttackerID = null;
            int? AttackerID2 = null;
            int? NormalMarkerRoleID1 = null;
            int? NormalMarkerRoleID2 = null;
            if (!oppBallOwner && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter))
            {
                if (matched.Any(w => w.Value.GetType() == typeof(NormalAttacker1)))
                    AttackerID = matched.Where(w => w.Value.GetType() == typeof(NormalAttacker1)).First().Key;

                if (matched.Any(w => w.Value.GetType() == typeof(NormalAttacker2)))
                    AttackerID2 = matched.Where(w => w.Value.GetType() == typeof(NormalAttacker2)).First().Key;
            }
            //else if ()
            //{
            //    if (matched.Any(w => w.Value.GetType() == typeof(NormalAttacker1)))
            //        AttackerID = matched.Where(w => w.Value.GetType() == typeof(NormalAttacker1)).First().Key;

            //    if (matched.Any(w => w.Value.GetType() == typeof(NormalAttacker2)))
            //        AttackerID2 = matched.Where(w => w.Value.GetType() == typeof(NormalAttacker2)).First().Key;
            //}
            else if (oppAttackerIds.Count > 0 && oppAttackerIds.Count <= 1)
            {

                if (matched.Any(w => w.Value.GetType() == typeof(NormalMarkerRole1)))
                    NormalMarkerRoleID1 = matched.Where(w => w.Value.GetType() == typeof(NormalMarkerRole1)).First().Key;

                if (matched.Any(w => w.Value.GetType() == typeof(NormalAttacker1)))
                    AttackerID = matched.Where(w => w.Value.GetType() == typeof(NormalAttacker1)).First().Key;
            }
            else if (oppAttackerIds.Count <= 0)
            {
                if (matched.Any(w => w.Value.GetType() == typeof(NormalAttacker1)))
                    AttackerID = matched.Where(w => w.Value.GetType() == typeof(NormalAttacker1)).First().Key;

                if (matched.Any(w => w.Value.GetType() == typeof(NormalAttacker2)))
                    AttackerID2 = matched.Where(w => w.Value.GetType() == typeof(NormalAttacker2)).First().Key;
            }
            else
            {
                if (matched.Any(w => w.Value.GetType() == typeof(NormalMarkerRole1)))
                    NormalMarkerRoleID1 = matched.Where(w => w.Value.GetType() == typeof(NormalMarkerRole1)).First().Key;

                if (matched.Any(w => w.Value.GetType() == typeof(NormalMarkerRole2)))
                    NormalMarkerRoleID2 = matched.Where(w => w.Value.GetType() == typeof(NormalMarkerRole2)).First().Key;
            }
            FreekickDefence.Static1ID = Defender1ID;
            FreekickDefence.Static2ID = Defender2ID;

            //if (lastOppCount != oppAttackerIds.Count || lastScores != scores)
            //{
            //    flagFirst = true;
            //}
            //if (flagFirst)
            //{
            //    lastScores = scores;
            //    List<int?> tempOppValue = new List<int?>();
            //    int? minScore = null; ;
            //    foreach (var o in oppValue)
            //    {
            //        bool f = false;
            //        foreach (var s in oppAttackerIds)
            //        {
            //            if (o.HasValue && o.Value == s)
            //            {
            //                f = true;
            //            }
            //        }
            //        if (f)
            //            tempOppValue.Add(o);
            //        else
            //            tempOppValue.Add(null);
            //    }
            //    oppValue = tempOppValue;
            //    //oppValue = new List<int?>();
            //    if (oppValue.Count < 8)
            //    {
            //        for (int i = 0; i < 8; i++)
            //            oppValue.Add(null);
            //    }
            //    if (firstSetBall)
            //    {
            //        firstBallPos = Model.BallState.Location;
            //        firstSetBall = false;
            //    }
            //    flagFirst = false;
            //}
            //for (int i = 0; i < 8; i++)
            //{
            //    if (!oppValue[i].HasValue || (oppValue[i].HasValue && !Model.Opponents.ContainsKey(oppValue[i].Value)))
            //    {
            //        oppValue[i] = null;
            //        foreach (var opp in oppAttackerIds)
            //        {
            //            bool oppAdd = true;
            //            foreach (var item in oppValue)
            //            {
            //                if (opp == item)
            //                {
            //                    oppAdd = false;
            //                }
            //            }
            //            if (oppAdd)
            //            {
            //                oppValue[i] = opp;
            //            }
            //        }
            //    }
            //}
         
            #endregion

            NormalSharedState.CommonInfo.ActiveID = ActiveID;
            NormalSharedState.CommonInfo.SupporterID = SupporterID;
            if (!NormalSharedState.CommonInfo.AttackerMode)
            {
                NormalSharedState.CommonInfo.AttackerID = AttackerID;
            }
            else
                NormalSharedState.CommonInfo.AttackerID = AttackerID2;

            //NormalSharedState.CommonInfo.PickerID = pickerID;

            #region assigner

            DefenceInfo gol = new DefenceInfo();
            gol.DefenderPosition = GameParameters.OurGoalCenter;
            gol.OppID = null;
            gol.RoleType = typeof(GoalieCornerRole);
            gol.TargetState = Model.BallState;
            gol.Teta = 180;


            ballIsMoved = Model.BallState.Location.DistanceFrom(firstBallPos) > .06;

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(GoalieCornerRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(goalie.Value).Run(eng, wmd, goalie.Value, GameParameters.OurGoalCenter, (Model.BallState.Location - GameParameters.OurGoalCenter).AngleInDegrees, gol, new Position2D(), null, true);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ActiveID, typeof(ActiveRole2017)))
            {
                Functions[ActiveID.Value] = (eng, wmd) => GetRole<ActiveRole2017>(ActiveID.Value).Perform(engine, Model, ActiveID.Value, false);
            }
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(StaticDefender1)))
            {
                Functions[Defender1ID.Value] = (eng, wmd) => GetRole<StaticDefender1>(Defender1ID.Value).Run(engine, Model, Defender1ID.Value, first.DefenderPosition.Value, first.Teta, CurrentlyAssignedRoles);
            }
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(StaticDefender2)))
            {
                Functions[Defender2ID.Value] = (eng, wmd) => GetRole<StaticDefender2>(Defender2ID.Value).Run(engine, Model, Defender2ID.Value, second.DefenderPosition.Value, second.Teta, CurrentlyAssignedRoles);
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, SupporterID, typeof(NewSupporter2Role)))
                Functions[SupporterID.Value] = (eng, wmd) => GetRole<NewSupporter2Role>(SupporterID.Value).Perform(eng, wmd, SupporterID.Value);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, RegionalID, typeof(RegionalDefenderRole)))
            //    Functions[RegionalID.Value] = (eng, wmd) => GetRole<RegionalDefenderRole>(RegionalID.Value).per;
            Circle c = new Circle(Model.BallState.Location, 0.6);
            if (!oppBallOwner && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter))
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AttackerID, typeof(NormalAttacker1)))
                    Functions[AttackerID.Value] = (eng, wmd) => GetRole<NormalAttacker1>(AttackerID.Value).Perform(eng, wmd, AttackerID.Value);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AttackerID2, typeof(NormalAttacker2)))
                    Functions[AttackerID2.Value] = (eng, wmd) => GetRole<NormalAttacker2>(AttackerID2.Value).Perform(eng, wmd, AttackerID2.Value);
            }
            else
            {
                if (oppAttackerIds.Count <= 0)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AttackerID, typeof(NormalAttacker1)))
                        Functions[AttackerID.Value] = (eng, wmd) => GetRole<NormalAttacker1>(AttackerID.Value).Perform(eng, wmd, AttackerID.Value);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AttackerID2, typeof(NormalAttacker2)))
                        Functions[AttackerID2.Value] = (eng, wmd) => GetRole<NormalAttacker2>(AttackerID2.Value).Perform(eng, wmd, AttackerID2.Value);
                }
                if (oppAttackerIds.Count > 0 && oppAttackerIds.Count <= 1)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                   
                        if (Model.Opponents[OppToMarkID1.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) > Model.Opponents[OppToMarkID1.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) && AttackerID.HasValue)
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AttackerID, typeof(NormalAttacker1)))
                                Functions[AttackerID.Value] = (eng, wmd) => GetRole<NormalAttacker1>(AttackerID.Value).Perform(eng, wmd, AttackerID.Value);
                        }
                        if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location) && NormalMarkerRoleID1.HasValue)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                        else if (c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location) && NormalMarkerRoleID1.HasValue)
                        {
                            Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                        }
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AttackerID, typeof(NormalAttacker1)))
                            Functions[AttackerID.Value] = (eng, wmd) => GetRole<NormalAttacker1>(AttackerID.Value).Perform(eng, wmd, AttackerID.Value);
                    }
                    else if (NormalMarkerRoleID1.HasValue && c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location) )
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                    else if (AttackerID.HasValue && c.IsInCircle(Model.Opponents[AttackerID.Value].Location))
                    {
                        Planner.Add(AttackerID.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                }
                else if (oppAttackerIds.Count > 1 && oppAttackerIds.Count <= 2)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                    OppToMarkID2 = oppAttackerIds[1];
                   
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (NormalMarkerRoleID1.HasValue && c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                    
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID2, typeof(NormalMarkerRole2)))
                            Functions[NormalMarkerRoleID2.Value] = (eng, wmd) => GetRole<NormalMarkerRole2>(NormalMarkerRoleID2.Value).Perform(engine, Model, NormalMarkerRoleID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID2.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                }
                else if (oppAttackerIds.Count > 2 && oppAttackerIds.Count <= 3)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                    OppToMarkID2 = oppAttackerIds[1];
                    OppToMarkID3 = oppAttackerIds[2];
                    
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }

                    
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID2, typeof(NormalMarkerRole2)))
                            Functions[NormalMarkerRoleID2.Value] = (eng, wmd) => GetRole<NormalMarkerRole2>(NormalMarkerRoleID2.Value).Perform(engine, Model, NormalMarkerRoleID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID2.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                }
                else if (oppAttackerIds.Count > 3 && oppAttackerIds.Count <= 4)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                    OppToMarkID2 = oppAttackerIds[1];
                    OppToMarkID3 = oppAttackerIds[2];
                    OppToMarkID4 = oppAttackerIds[3];
                   
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }

                    if (!c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID2, typeof(NormalMarkerRole2)))
                            Functions[NormalMarkerRoleID2.Value] = (eng, wmd) => GetRole<NormalMarkerRole2>(NormalMarkerRoleID2.Value).Perform(engine, Model, NormalMarkerRoleID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID2.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                       
                    }
                }
                else if (oppAttackerIds.Count > 4 && oppAttackerIds.Count <= 5)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                    OppToMarkID2 = oppAttackerIds[1];
                    OppToMarkID3 = oppAttackerIds[2];
                    OppToMarkID4 = oppAttackerIds[3];
                    OppToMarkID5 = oppAttackerIds[4];
                   
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                    
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID2, typeof(NormalMarkerRole2)))
                            Functions[NormalMarkerRoleID2.Value] = (eng, wmd) => GetRole<NormalMarkerRole2>(NormalMarkerRoleID2.Value).Perform(engine, Model, NormalMarkerRoleID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID2.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                }
                else if (oppAttackerIds.Count > 5 && oppAttackerIds.Count <= 6)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                    OppToMarkID2 = oppAttackerIds[1];
                    OppToMarkID3 = oppAttackerIds[2];
                    OppToMarkID4 = oppAttackerIds[3];
                    OppToMarkID5 = oppAttackerIds[4];
                    OppToMarkID6 = oppAttackerIds[5];
                    
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                   
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID2, typeof(NormalMarkerRole2)))
                            Functions[NormalMarkerRoleID2.Value] = (eng, wmd) => GetRole<NormalMarkerRole2>(NormalMarkerRoleID2.Value).Perform(engine, Model, NormalMarkerRoleID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID2.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                }
                else if (oppAttackerIds.Count > 6 && oppAttackerIds.Count <= 7)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                    OppToMarkID2 = oppAttackerIds[1];
                    OppToMarkID3 = oppAttackerIds[2];
                    OppToMarkID4 = oppAttackerIds[3];
                    OppToMarkID5 = oppAttackerIds[4];
                    OppToMarkID6 = oppAttackerIds[5];
                    OppToMarkID7 = oppAttackerIds[6];
                   
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                   
                    if ( !c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID2, typeof(NormalMarkerRole2)))
                            Functions[NormalMarkerRoleID2.Value] = (eng, wmd) => GetRole<NormalMarkerRole2>(NormalMarkerRoleID2.Value).Perform(engine, Model, NormalMarkerRoleID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID2.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                }
                else if (oppAttackerIds.Count > 7 && oppAttackerIds.Count <= 8)
                {
                    OppToMarkID1 = oppAttackerIds[0];
                    OppToMarkID2 = oppAttackerIds[1];
                    OppToMarkID3 = oppAttackerIds[2];
                    OppToMarkID4 = oppAttackerIds[3];
                    OppToMarkID5 = oppAttackerIds[4];
                    OppToMarkID6 = oppAttackerIds[5];
                    OppToMarkID7 = oppAttackerIds[6];
                    OppToMarkID8 = oppAttackerIds[7];
                   
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID1, typeof(NormalMarkerRole1)))
                            Functions[NormalMarkerRoleID1.Value] = (eng, wmd) => GetRole<NormalMarkerRole1>(NormalMarkerRoleID1.Value).Perform(engine, Model, NormalMarkerRoleID1.Value, OppToMarkID1, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID1.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID1.Value, new Position2D(2, 2), (Model.BallState.Location - new Position2D(2, 2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                   
                    if (!c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, NormalMarkerRoleID2, typeof(NormalMarkerRole2)))
                            Functions[NormalMarkerRoleID2.Value] = (eng, wmd) => GetRole<NormalMarkerRole2>(NormalMarkerRoleID2.Value).Perform(engine, Model, NormalMarkerRoleID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
                    }
                    else if (c.IsInCircle(Model.Opponents[OppToMarkID2.Value].Location))
                    {
                        Planner.Add(NormalMarkerRoleID2.Value, new Position2D(2, -2), (Model.BallState.Location - new Position2D(2, -2)).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                    }
                }
            }
            #endregion

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
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
            flagFirst = true;
            oppBallOwner = false;
            firstSetBall = true;
            OppToMarkID1 = null;
            OppToMarkID2 = null;
            OppToMarkID3 = null;
            OppToMarkID4 = null;
            OppToMarkID5 = null;
            OppToMarkID6 = null;
            OppToMarkID7 = null;
            OppToMarkID8 = null;
            oppValue.Clear();
        }
    }
}
