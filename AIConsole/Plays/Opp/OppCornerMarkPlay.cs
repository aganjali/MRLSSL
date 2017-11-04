using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.GameDefinitions.General_Settings;
using System.Drawing;

namespace MRL.SSL.AIConsole.Plays
{
    class OppCornerMarkPlay : PlayBase
    {
        bool flagFirst = true;
        Position2D lastballstate;
        bool ballIsMoved = false, oppBallOwner = false;
        private static bool weHaveActive = false,weHaveAttacker=false;
        bool flag = false;
        bool firstSet = true;
        int count = 0;
        int lastOppCount = 0;
        bool firstSetBall = true;
        int? oppBallOwnerId, goalie = null, MarkerID1 = null, MarkerID2 = null, MarkerID3 = null, MarkerID4 = null, MarkerID5 = null, OppToMarkID1 = null, OppToMarkID2 = null, OppToMarkID3 = null, OppToMarkID4 = null, OppToMarkID5 = null;
        List<int?> oppValue = new List<int?>();
        Position2D firstBallPos = new Position2D();
        public double RegionalRegion = 2;
        Dictionary<int, SingleObjectState> tempOppRobots;

        Dictionary<int, float> lastScores;

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            //return true;
            //return false;
            if (!(engine.EngineID == 0 && (Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent))
                && !(engine.EngineID == 1 && (Status == GameDefinitions.GameStatus.DirectFreeKick_OurTeam || Status == GameStatus.IndirectFreeKick_OurTeam)))
            {
                flag = false;
                return false;
            }
            else
            {
                if (Model.BallState.Location.X > RegionalRegion && (
                    Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent || Status == GameDefinitions.GameStatus.DirectFreeKick_OurTeam || Status == GameStatus.IndirectFreeKick_OurTeam) || flag)
                {
                    if (!flag)
                        lastballstate = Model.BallState.Location;
                    flag = true;

                    double d1, d2;
                    int? oppOwnerId = engine.GameInfo.OppTeam.BallOwner;
                    int? ourOwnerId = engine.GameInfo.OurTeam.BallOwner;
                    if (ourOwnerId.HasValue && !GameParameters.IsInDangerousZone(Model.OurRobots[ourOwnerId.Value].Location, false, 0.2, out d1, out d2)/* && ballState.Location.X < 1.5*/ && ballIsMoved)
                    {
                        //if (!(engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < 0.3))
                        //{
                        flag = false;
                        Status = GameStatus.Normal;
                        // }
                    }
                    if (ballIsMoved && !(engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < 0.3))
                    {
                        count++;
                        if (count > 120)
                        {
                            flag = false;
                            Status = GameStatus.Normal;
                        }
                    }
                    if (engine.GameInfo.OppTeam.BallOwner.HasValue)
                    {
                        count = 0;
                    }
                    MarkerRole1 mr1 = new MarkerRole1();
                    int? mr1ID = null;
                    MarkerRole2 mr2 = new MarkerRole2();
                    int? mr2ID = null;
                    MarkerRole3 mr3 = new MarkerRole3();
                    int? mr3ID = null;
                    MarkerRole4 mr4 = new MarkerRole4();
                    int? mr4ID = null;
                    MarkerRole5 mr5 = new MarkerRole5();
                    int? mr5ID = null;
                    foreach (var item in PreviouslyAssignedRoles)
                    {
                        if (item.GetType is MarkerRole1)
                        { mr1 = (MarkerRole1)item.Value; mr1ID = item.Key; }
                        if (item.GetType is MarkerRole2)
                        { mr2 = (MarkerRole2)item.Value; mr2ID = item.Key; }
                        if (item.GetType is MarkerRole3)
                        { mr3 = (MarkerRole3)item.Value; mr3ID = item.Key; }
                        if (item.GetType is MarkerRole4)
                        { mr4 = (MarkerRole4)item.Value; mr4ID = item.Key; }
                        if (item.GetType is MarkerRole5)
                        { mr5 = (MarkerRole5)item.Value; mr5ID = item.Key; }
                    }
                    if (Model.BallState.Speed.Size < 1)
                    {
                        if (mr1.CurrentState == (int)State.cutball && mr1ID.HasValue && Model.BallState.Location.DistanceFrom(Model.OurRobots[mr1ID.Value].Location) < 0.3)
                            Status = GameStatus.Normal;
                        if (mr2.CurrentState == (int)State.cutball && mr2ID.HasValue && Model.BallState.Location.DistanceFrom(Model.OurRobots[mr2ID.Value].Location) < 0.3)
                            Status = GameStatus.Normal;
                        if (mr3.CurrentState == (int)State.cutball && mr3ID.HasValue && Model.BallState.Location.DistanceFrom(Model.OurRobots[mr3ID.Value].Location) < 0.3)
                            Status = GameStatus.Normal;
                        if (mr4.CurrentState == (int)State.cutball && mr4ID.HasValue && Model.BallState.Location.DistanceFrom(Model.OurRobots[mr4ID.Value].Location) < 0.3)
                            Status = GameStatus.Normal;
                        if (mr5.CurrentState == (int)State.cutball && mr5ID.HasValue && Model.BallState.Location.DistanceFrom(Model.OurRobots[mr5ID.Value].Location) < 0.3)
                            Status = GameStatus.Normal;
                    }
                    return true;
                }
                else
                    return false;
            }
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            List<int> oppAttackerIds = new List<int>();

            double markRegion = -3.5;
            Dictionary<int, float> scores;
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
            if (scores.Count > 5)
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

            FreekickDefence.BallIsMoved = ballIsMoved;

            if (ballIsMoved)
            {
                oppBallOwnerId = engine.GameInfo.OppTeam.BallOwner;
                if (oppBallOwnerId.HasValue)
                {
                    oppAttackerIds = oppAttackerIds.Where(w => w != oppBallOwnerId.Value).ToList();
                }
            }

            #region matcher
            RoleBase r;
            roles = new List<RoleInfo>();

            r = typeof(MarkerRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(MarkerRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(MarkerRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(MarkerRole4).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(MarkerRole5).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));


            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            goalie = Model.GoalieID;
            if (matched.Any(w => w.Value.GetType() == typeof(MarkerRole1)))
            {
                MarkerID1 = matched.Where(w => w.Value.GetType() == typeof(MarkerRole1)).First().Key;
            }
            if (matched.Any(w => w.Value.GetType() == typeof(MarkerRole2)))
            {
                MarkerID2 = matched.Where(w => w.Value.GetType() == typeof(MarkerRole2)).First().Key;
            }
            if (matched.Any(w => w.Value.GetType() == typeof(MarkerRole3)))
            {
                MarkerID3 = matched.Where(w => w.Value.GetType() == typeof(MarkerRole3)).First().Key;
            }
            if (matched.Any(w => w.Value.GetType() == typeof(MarkerRole4)))
            {
                MarkerID4 = matched.Where(w => w.Value.GetType() == typeof(MarkerRole4)).First().Key;
            }
            if (matched.Any(w => w.Value.GetType() == typeof(MarkerRole5)))
            {
                MarkerID5 = matched.Where(w => w.Value.GetType() == typeof(MarkerRole5)).First().Key;
            }
            #region opp to mark id
            if (lastOppCount!=oppAttackerIds.Count || lastScores != scores)
            {
                flagFirst = true;
            }
            if (flagFirst)
            {
                lastScores =scores;
                List<int?> tempOppValue=new List<int?>();
                int? minScore = null; ;
                foreach (var o in oppValue)
                {
                    bool f = false;
                    foreach (var s in oppAttackerIds)
                    {
                        if (o.HasValue && o.Value == s)
                        {
                            f=true;                            
                        }
                    }
                    if (f)
                        tempOppValue.Add(o);
                    else
                        tempOppValue.Add(null);
                }
                oppValue= tempOppValue;
                //oppValue = new List<int?>();
                for (int i = 0; i < 5; i++)
                    oppValue.Add(null);
                if (firstSetBall)
                {
                    firstBallPos = Model.BallState.Location;
                    firstSetBall = false;
                }
                flagFirst = false;
            }
            for (int i = 0; i < 5; i++)
            {
                if (!oppValue[i].HasValue || (oppValue[i].HasValue && !Model.Opponents.ContainsKey(oppValue[i].Value)))
                {
                    oppValue[i] = null;
                    foreach (var opp in oppAttackerIds.OrderBy(w => w))
                    {
                        bool oppAdd = true;
                        foreach (var item in oppValue)
                        {
                            if (opp == item)
                            {
                                oppAdd = false;
                            }
                        }
                        if (oppAdd)
                        {
                            oppValue[i] = opp;
                        }
                    }
                }
            }
            //for (int i = 0; i < oppvalue.Count; i++)
            //{
            //    if (oppvalue[i].HasValue && ((oppAttackerIds.Contains(oppvalue[i].Value) && Model.Opponents[oppvalue[i].Value].Location.X < markRegion) || !oppAttackerIds.Contains(oppvalue[i].Value)))
            //    {
            //        oppvalue[i] = null;
            //    }
            //}

            OppToMarkID1 = oppValue[0];
            OppToMarkID2 = oppValue[1];
            OppToMarkID3 = oppValue[2];
            OppToMarkID4 = oppValue[3];
            OppToMarkID5 = oppValue[4];
            #endregion

            #endregion

            #region assigner
            //var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));
            DefenceInfo gol = new DefenceInfo();
            gol.DefenderPosition = GameParameters.OurGoalCenter;
            gol.OppID = null;
            gol.RoleType = typeof(GoalieCornerRole);
            gol.TargetState = Model.BallState;
            gol.Teta = 180;


            ballIsMoved = Model.BallState.Location.DistanceFrom(firstBallPos) > .06;

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(MarkerGoalieRole)))
            //    Functions[goalie.Value] = (eng, wmd) => GetRole<MarkerGoalieRole>(goalie.Value).perform(eng, wmd, goalie.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(GoalieCornerRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(goalie.Value).Run(eng, wmd, goalie.Value, GameParameters.OurGoalCenter, (Model.BallState.Location - GameParameters.OurGoalCenter).AngleInDegrees, gol, new Position2D(), null, true);
            if (ballIsMoved && oppAttackerIds.Count<1)
            {
                weHaveActive = true;
            }
            if (!weHaveActive)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, MarkerID1, typeof(MarkerRole1)))
                    Functions[MarkerID1.Value] = (eng, wmd) => GetRole<MarkerRole1>(MarkerID1.Value).Perform(eng, wmd, MarkerID1.Value, OppToMarkID1, markRegion, ballIsMoved,oppAttackerIds);
            }
            else
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, MarkerID1.Value, typeof(ActiveRole2017)))
                    Functions[MarkerID1.Value] = (eng, wmd) => GetRole<ActiveRole2017>(MarkerID1.Value).Perform(eng, wmd, MarkerID1.Value, false);
            }
            if (ballIsMoved && oppAttackerIds.Count<2)
            {
                weHaveAttacker = true;
            }
            if (weHaveAttacker)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, MarkerID2, typeof(NewAttackerRole)))
                    Functions[MarkerID2.Value] = (eng, wmd) => GetRole<NewAttackerRole>(MarkerID2.Value).Perform(eng, wmd, MarkerID2.Value);
            }
            else
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, MarkerID2, typeof(MarkerRole2)))
                    Functions[MarkerID2.Value] = (eng, wmd) => GetRole<MarkerRole2>(MarkerID2.Value).Perform(eng, wmd, MarkerID2.Value, OppToMarkID2, markRegion, ballIsMoved, oppAttackerIds);
            }
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, MarkerID3, typeof(MarkerRole3)))
                Functions[MarkerID3.Value] = (eng, wmd) => GetRole<MarkerRole3>(MarkerID3.Value).Perform(eng, wmd, MarkerID3.Value, OppToMarkID3, markRegion, ballIsMoved, oppAttackerIds);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, MarkerID4, typeof(MarkerRole4)))
                Functions[MarkerID4.Value] = (eng, wmd) => GetRole<MarkerRole4>(MarkerID4.Value).Perform(eng, wmd, MarkerID4.Value, OppToMarkID4, markRegion, ballIsMoved, oppAttackerIds);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, MarkerID5, typeof(MarkerRole5)))
                Functions[MarkerID5.Value] = (eng, wmd) => GetRole<MarkerRole5>(MarkerID5.Value).Perform(eng, wmd, MarkerID5.Value, OppToMarkID5, markRegion, ballIsMoved, oppAttackerIds);


            #endregion

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return null;
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            tempOppRobots = new Dictionary<int, SingleObjectState>();
            flagFirst = true;
            ballIsMoved = false;
            oppBallOwner = false;
            weHaveActive = false;
            weHaveAttacker = false;
            firstSetBall = true;
            lastOppCount = 0;
            flag = false;
            firstSet = true;
            OppToMarkID1 = null;
            OppToMarkID2 = null;
            OppToMarkID3 = null;
            OppToMarkID4 = null;
            OppToMarkID5 = null;
            oppValue.Clear();
        }
        private int accumulatedOppId(WorldModel model, List<int> ImportantIds)
        {
            Dictionary<int, Vector2D> opponentIdsVector = new Dictionary<int, Vector2D>();
            List<int> ids = new List<int>();

            foreach (int item in ImportantIds)
            {
                if (model.Opponents.ContainsKey(item))
                {
                    Vector2D opponentGC = model.Opponents[item].Location - GameParameters.OurGoalCenter;
                    opponentIdsVector.Add(item, opponentGC);
                }

            }
            Dictionary<int, double> angles = new Dictionary<int, double>();
            var opps = model.Opponents.Where(w => ImportantIds.Contains(w.Key)).OrderBy(o => o.Value.Location.Y).ToList();

            for (int i = 1; i < opps.Count - 1; i++)
            {
                int key = opps[i].Key;
                double prev = 0, next = 0;
                //if (i > 0)
                //{
                int pkey = opps[i - 1].Key;
                prev = Math.Abs(Vector2D.AngleBetweenInDegrees(opponentIdsVector[key], opponentIdsVector[pkey]));
                //}
                //if (i < opps.Count - 1)
                //{
                int nkey = opps[i + 1].Key;
                next = Math.Abs(Vector2D.AngleBetweenInDegrees(opponentIdsVector[key], opponentIdsVector[nkey]));
                //}
                angles[key] = next + prev;
            }
            angles = angles.OrderBy(o => o.Value).ToDictionary(k => k.Key, v => v.Value);
            var minAng = (angles.Count > 0) ? angles.FirstOrDefault().Key : -1;
            if (opponentIdsVector.ContainsKey(minAng) && model.Opponents.ContainsKey(minAng))
            {
                return minAng;
            }
            else
            {
                if (opponentIdsVector.Count > 0)
                {
                    if (model.Opponents.ContainsKey(opponentIdsVector.First().Key))
                    {
                        return opponentIdsVector.First().Key;
                    }
                }
                foreach (var item in ImportantIds)
                {
                    if (model.Opponents.ContainsKey(item))
                    {
                        return item;
                    }
                }
            }

            if (model.Opponents.Keys.Count > 0)
            {
                return model.Opponents.Keys.First();
            }
            return -1;
            //foreach (int item in opponentIdsVector.Keys)
            //{
            //    angles.Add(item, 0);
            //    foreach (int item2 in opponentIdsVector.Keys)
            //    {
            //        Vector2D opponentGC = model.Opponents[item2].Location - GameParameters.OurGoalCenter;
            //        angles[item] += Math.Abs(Vector2D.AngleBetweenInDegrees(opponentIdsVector[item], opponentGC));
            //    }

            //}

            //int denseRobot = angles.OrderBy(t => t.Value).Select(y => y.Key).FirstOrDefault() ;
            // return denseRobot;
        }
        private int accumulatedOppId2(WorldModel model, List<int> ImportantIds)
        {
            Dictionary<int, Vector2D> opponentIdsVector = new Dictionary<int, Vector2D>();
            List<int> ids = new List<int>();

            foreach (int item in ImportantIds)
            {
                if (model.Opponents.ContainsKey(item))
                {
                    Vector2D opponentGC = model.Opponents[item].Location - GameParameters.OurGoalCenter;
                    opponentIdsVector.Add(item, opponentGC);
                }

            }
            Dictionary<int, double> angles = new Dictionary<int, double>();
            var opps = model.Opponents.Where(w => ImportantIds.Contains(w.Key)).OrderBy(o => o.Value.Location.Y).ToList();

            for (int i = 1; i < opps.Count - 1; i++)
            {
                int key = opps[i].Key;
                double next = 0;

                int nkey = opps[i + 1].Key;
                next = Math.Abs(Vector2D.AngleBetweenInDegrees(opponentIdsVector[key], opponentIdsVector[nkey]));

                angles[key] = next;
            }
            angles = angles.OrderBy(o => o.Value).ToDictionary(k => k.Key, v => v.Value);
            var minAng = (angles.Count > 0) ? angles.FirstOrDefault().Key : -1;
            if (opponentIdsVector.ContainsKey(minAng) && model.Opponents.ContainsKey(minAng))
            {
                int minIdx = -1;
                for (int i = 0; i < opps.Count; i++)
                {
                    if (opps[i].Key == minAng)
                    {
                        minIdx = i;
                    }
                }
                if (minIdx != -1 && minIdx < opps.Count - 1)
                {
                    if (model.Opponents.ContainsKey(opps[minIdx + 1].Key))
                    {
                        if (model.Opponents[minAng].Location.DistanceFrom(GameParameters.OurGoalCenter) < model.Opponents[opps[minIdx + 1].Key].Location.DistanceFrom(GameParameters.OurGoalCenter))
                        {
                            return opps[minIdx + 1].Key;
                        }
                        else
                            return minAng;
                    }
                }
                return minAng;
            }
            else
            {
                if (opponentIdsVector.Count > 0)
                {
                    if (model.Opponents.ContainsKey(opponentIdsVector.First().Key))
                    {
                        return opponentIdsVector.First().Key;
                    }
                }
                foreach (var item in ImportantIds)
                {
                    if (model.Opponents.ContainsKey(item))
                    {
                        return item;
                    }
                }
            }

            if (model.Opponents.Keys.Count > 0)
            {
                return model.Opponents.Keys.First();
            }
            return -1;
            //foreach (int item in opponentIdsVector.Keys)
            //{
            //    angles.Add(item, 0);
            //    foreach (int item2 in opponentIdsVector.Keys)
            //    {
            //        Vector2D opponentGC = model.Opponents[item2].Location - GameParameters.OurGoalCenter;
            //        angles[item] += Math.Abs(Vector2D.AngleBetweenInDegrees(opponentIdsVector[item], opponentGC));
            //    }

            //}

            //int denseRobot = angles.OrderBy(t => t.Value).Select(y => y.Key).FirstOrDefault() ;
            // return denseRobot;
        }

        private void AddRoleInfo(List<RoleInfo> roles, Type role, double weight, double margin)
        {
            RoleBase r = role.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, weight, margin));
        }
    }

    enum State
    {
        regional,
        Stop,
        StopBallIsMoved,
        marknear,
        markfar,
        cutball,
        IntheWay,
        goback,
        behind
    }
}