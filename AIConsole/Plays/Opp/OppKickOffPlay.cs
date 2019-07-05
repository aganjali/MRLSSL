using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OppKickOffPlay : PlayBase
    {
        Position2D lastballstate;
        bool flag = false;
        bool ballismoved = false;
        bool oppcathball = false;

        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {

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
            if (!(engine.EngineID == 0 && (Status == GameDefinitions.GameStatus.KickOff_Opponent_Go || Status == GameStatus.KickOff_Opponent_Waiting))
                && !(engine.EngineID == 1 && (Status == GameDefinitions.GameStatus.KickOff_OurTeam_Go || Status == GameStatus.KickOff_OurTeam_Waiting)))
            {
                flag = false;
                return false;
            }
            else
            {
                if ((
                    Status == GameDefinitions.GameStatus.KickOff_Opponent_Go || Status == GameStatus.KickOff_Opponent_Waiting || Status == GameDefinitions.GameStatus.KickOff_OurTeam_Waiting || Status == GameStatus.KickOff_OurTeam_Go) || flag)
                {
                    if (!flag)
                        lastballstate = ballState.Location;
                    flag = true;

                    double d1, d2;
                    int? ourOwner = engine.GameInfo.OurTeam.BallOwner;
                    if (ourOwner.HasValue && !GameParameters.IsInDangerousZone(Model.OurRobots[ourOwner.Value].Location, false, 0.2, out d1, out d2)/* && ballState.Location.X < 1.5*/ && ballismoved)
                    {
                        //if (!(engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(ballState.Location) < 0.3))
                        //{
                        flag = false;
                        Status = GameStatus.Normal;
                        //}
                    }
                    return true;
                }
                else
                    return false;
            }
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            FreekickDefence.weAreInKickoff = true;
            FreekickDefence.SwitchToActiveReset();
            DefenceTest.BallTest = FreekickDefence.testDefenceState;
            DefenceTest.GenerateBallPos();
            Planner.IsStopBall(ballismoved);
            Planner.IsStopBall(true);

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
            FreekickDefence.RearRegional = false;
            FreekickDefence.WeAreInCorner = false;
            double maxdist = 3;
            if (!ballismoved)
                maxdist = 2.5;
            //int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            int? goalieID = Model.GoalieID;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            //float max = float.MinValue;
            List<int> attids = new List<int>();
            List<int> oppAttackerIds = new List<int>();//engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key)).Where(w => w.Value > 0.6 && w.Key != goalieID).Select(s => s.Key).ToList();
            if (goalieID.HasValue)
            {
                attids = Model.Opponents.Where(w => w.Value.Location.X > -1 && w.Key != goalieID).Select(s => s.Key).ToList();
                oppAttackerIds = engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key)).Where(w => w.Value > 0.6 && w.Key != goalieID).Select(s => s.Key).ToList();
            }
            else
            {
                attids = Model.Opponents.Where(w => w.Value.Location.X > -1).Select(s => s.Key).ToList();
                oppAttackerIds = engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key)).Where(w => w.Value > 0.6).Select(s => s.Key).ToList();
            }
            //if (engine.GameInfo.OppTeam.Scores.Count > 0)
            //    max = engine.GameInfo.OppTeam.Scores.Max(s => s.Value);
            ///
            /// 
            /// scroe normalization is canceled
            /// 
            ///

            oppAttackerIds.AddRange(attids);
            List<DefenderCommand> defendcommands = new List<DefenderCommand>();

            List<Position2D> points = new List<CommonClasses.MathLibrary.Position2D>() {
                                    new Position2D(0,-1.5),
                                    new Position2D(0,-.5),
                                    new Position2D(0,.5),
                                    new Position2D(0,1.5),
                                };

            int? id = null;
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(GoalieCornerRole)
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole1),
                OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
            });

            if (!oppcathball && engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(ballState.Location) < 0.12)
                oppcathball = true;
            if (lastballstate.DistanceFrom(ballState.Location) > 0.07 && oppcathball)
                ballismoved = true;

            Type freeRole;
            if (!ballismoved)
                freeRole = typeof(StopRole1);
            else
                freeRole = typeof(ActiveRole);
            double dX = 0.8;
            #region opp < 2
            if (oppAttackerIds.Count < 2)
            {

                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(RegionalDefenderRole),
                    OppID = null,
                    RegionalDistFromDangerZone = 0.1,
                    RegionalDefendPoints = points,
                });

                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(RegionalDefenderRole2),
                    OppID = null,
                    RegionalDistFromDangerZone = 0.1,
                    RegionalDefendPoints = points,
                });
                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);

                roles = new List<RoleInfo>();
                // AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                AddRoleInfo(roles, freeRole, 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                AddRoleInfo(roles, typeof(RegionalDefenderRole), 1, 0);
                AddRoleInfo(roles, typeof(RegionalDefenderRole2), 1, 0);






                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();

                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, regional2 = null, regional = null, golie = null, gotopoint = null;


                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                regional2 = getID(assigenroles, typeof(RegionalDefenderRole2));
                regional = getID(assigenroles, typeof(RegionalDefenderRole));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                gotopoint = getID(assigenroles, freeRole);


                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                var reg = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole));
                var reg2 = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole2));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole1)))
                    Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                DefenceTest.WeHaveDefenderCornerRole1 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(RegionalDefenderRole)))
                    Functions[regional.Value] = (eng, wmd) => GetRole<RegionalDefenderRole>(regional.Value).positionnig(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                DefenceTest.WeHaveDefenderRegionalRole1 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional2, typeof(RegionalDefenderRole2)))
                    Functions[regional2.Value] = (eng, wmd) => GetRole<RegionalDefenderRole2>(regional2.Value).positionnig(eng, wmd, regional2.Value, reg2.DefenderPosition.Value, reg2.Teta);
                DefenceTest.WeHaveDefenderRegionalRole2 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, gotopoint, freeRole))
                {
                    if (freeRole == typeof(StopRole1))
                    {
                        Functions[gotopoint.Value] = (eng, wmd) => GetRole<StopRole1>(gotopoint.Value).RunRole(eng, wmd, gotopoint.Value);
                        DefenceTest.WeHaveGoalie = true;
                    }
                    else
                        Functions[gotopoint.Value] = (eng, wmd) => GetRole<ActiveRole>(gotopoint.Value).Perform(eng, wmd, gotopoint.Value, null);
                }

            }
            #endregion

            #region opp < 4
            else if (oppAttackerIds.Count < 4)
            {
                //defendcommands.Add(new DefenderCommand() { RoleType = typeof(DefenderCornerRole2), OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(1).Key : id });
                if (!ballismoved)
                {
                    if (oppAttackerIds.Count == 2)
                    {
                        Position2D opp2mark = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? Model.Opponents[engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key].Location : ballState.Location;
                        Line l = new Line(opp2mark, GameParameters.OurGoalCenter);
                        Line l1 = new Line(new Position2D(dX, -1), new Position2D(dX, 1));
                        Position2D tmpP = Position2D.Zero;
                        l.IntersectWithLine(l1, ref tmpP);
                        double d = tmpP.DistanceFrom(GameParameters.OurGoalCenter);
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole),
                            MarkMaximumDist = d,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                    }
                    else
                    {
                        Position2D opp2mark = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? Model.Opponents[engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key].Location : ballState.Location;
                        Line l = new Line(opp2mark, GameParameters.OurGoalCenter);
                        Line l1 = new Line(new Position2D(dX, -1), new Position2D(dX, 1));
                        Position2D tmpP = Position2D.Zero;
                        l.IntersectWithLine(l1, ref tmpP);
                        double d = tmpP.DistanceFrom(GameParameters.OurGoalCenter);
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole),
                            MarkMaximumDist = d,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                    }
                }
                else
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole),
                        MarkMaximumDist = 3,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });
                }
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderMarkerRole2),
                    MarkMaximumDist = maxdist,
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(RegionalDefenderRole),
                    OppID = null,
                    RegionalDistFromDangerZone = 0.1,
                    RegionalDefendPoints = points,
                });

                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);

                roles = new List<RoleInfo>();

                // AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                AddRoleInfo(roles, freeRole, 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);

                AddRoleInfo(roles, typeof(DefenderMarkerRole2), 1, 0);

                AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                AddRoleInfo(roles, typeof(RegionalDefenderRole), 1, 0);





                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null,marker = null, regional = null, golie = null, mark2 = null, marker2 = null;

                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                // n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                marker2 = getID(assigenroles, typeof(DefenderMarkerRole2));
                marker = getID(assigenroles, typeof(DefenderMarkerRole));
                regional = getID(assigenroles, typeof(RegionalDefenderRole));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                mark2 = getID(assigenroles, freeRole);



                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                //var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                var reg = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole));
                var mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));
                var mrk2 = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));


                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole1)))
                    Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                DefenceTest.WeHaveDefenderCornerRole1 = true;
                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                //    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(RegionalDefenderRole)))
                    Functions[regional.Value] = (eng, wmd) => GetRole<RegionalDefenderRole>(regional.Value).positionnig(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                DefenceTest.WeHaveDefenderRegionalRole1 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                    Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                DefenceTest.WeHaveDefenderMarkerRole1 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker2, typeof(DefenderMarkerRole2)))
                    Functions[marker2.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(marker2.Value).Mark(eng, wmd, marker2.Value, mrk2.DefenderPosition.Value, mrk2.Teta);
                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(StopRole1))
                    {
                        Functions[mark2.Value] = (eng, wmd) => GetRole<StopRole1>(mark2.Value).RunRole(eng, wmd, mark2.Value);
                        DefenceTest.WeHaveStopCover1 = true;
                    }
                    else
                        Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                }
            }
            #endregion

            #region opp < 5
            else if (oppAttackerIds.Count < 5)
            {
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                });
                double d = maxdist;
                if (!ballismoved)
                {
                    Position2D opp2mark = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? Model.Opponents[engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key].Location : ballState.Location;
                    Line l = new Line(opp2mark, GameParameters.OurGoalCenter);
                    Line l1 = new Line(new Position2D(dX, -1), new Position2D(dX, 1));
                    Position2D tmpP = Position2D.Zero;
                    l.IntersectWithLine(l1, ref tmpP);
                    d = tmpP.DistanceFrom(GameParameters.OurGoalCenter);
                }
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderMarkerRole),
                    MarkMaximumDist = d,
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                });

                if (!ballismoved)
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole2),
                        MarkMaximumDist = maxdist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                    });
                else
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole2),
                        MarkMaximumDist = maxdist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });



                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
                roles = new List<RoleInfo>();

                //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                AddRoleInfo(roles, freeRole, 1, 0);
                AddRoleInfo(roles, typeof(DefenderMarkerRole2), 1, 0);



                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, marker = null, regional = null, golie = null, mark2 = null;

                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                marker = getID(assigenroles, typeof(DefenderMarkerRole));
                regional = getID(assigenroles, typeof(DefenderMarkerRole2));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2


                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                var reg = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                var mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));



                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole1)))
                    Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                DefenceTest.WeHaveDefenderCornerRole1 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(DefenderMarkerRole2)))
                    Functions[regional.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                    Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                DefenceTest.WeHaveDefenderMarkerRole1 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(StopRole1))
                    {
                        Functions[mark2.Value] = (eng, wmd) => GetRole<StopRole1>(mark2.Value).RunRole(eng, wmd, mark2.Value);
                        DefenceTest.WeHaveStopCover1 = true;
                    }
                    else
                        Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                }
            }
            #endregion

            #region opp >= 5
            else if (oppAttackerIds.Count > 4)
            {
                defendcommands = new List<DefenderCommand>();
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(GoalieCornerRole)
                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole1),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                });
                double d = maxdist;
                if (!ballismoved)
                {
                    Position2D opp2mark = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? Model.Opponents[engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key].Location : ballState.Location;
                    Line l = new Line(opp2mark, GameParameters.OurGoalCenter);
                    Line l1 = new Line(new Position2D(dX, -1), new Position2D(dX, 1));
                    Position2D tmpP = Position2D.Zero;
                    l.IntersectWithLine(l1, ref tmpP);
                    d = tmpP.DistanceFrom(GameParameters.OurGoalCenter);
                }
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderMarkerRole),
                    MarkMaximumDist = d,
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                });
                if (!ballismoved)
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole2),
                        MarkMaximumDist = maxdist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                    });
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderCornerRole3),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id
                    });
                }
                else
                {

                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole2),
                        MarkMaximumDist = maxdist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderCornerRole1),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });
                }



                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
                roles = new List<RoleInfo>();

                //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                if (!ballismoved)
                    AddRoleInfo(roles, typeof(DefenderCornerRole3), 1, 0);
                else
                    AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);

                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                AddRoleInfo(roles, freeRole, 1, 0);
                AddRoleInfo(roles, typeof(DefenderMarkerRole2), 1, 0);



                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, marker = null, regional = null, golie = null, mark2 = null;

                if (!ballismoved)

                    n1 = getID(assigenroles, typeof(DefenderCornerRole3));
                else
                    n1 = getID(assigenroles, typeof(DefenderCornerRole1));

                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                marker = getID(assigenroles, typeof(DefenderMarkerRole));
                regional = getID(assigenroles, typeof(DefenderMarkerRole2));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2

                DefenceInfo normal1 = new DefenceInfo();
                if (!ballismoved)
                    normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole3));
                else
                    normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));

                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                var reg = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                var mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));


                if (!ballismoved)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole3)))
                        Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole3>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                    DefenceTest.WeHaveDefenderCornerRole3 = true;
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole1)))
                        Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                    DefenceTest.WeHaveDefenderCornerRole1 = true;
                }
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(DefenderMarkerRole2)))
                    Functions[regional.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                    Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                DefenceTest.WeHaveDefenderMarkerRole1 = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieStopRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieStopRole>(golie.Value).Positioning(engine, wmd, golie.Value);
                DefenceTest.WeHaveGoalie = true;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(StopRole1))
                    {
                        Functions[mark2.Value] = (eng, wmd) => GetRole<StopRole1>(mark2.Value).RunRole(eng, wmd, mark2.Value);
                        DefenceTest.WeHaveStopCover1 = true;
                    }
                    else
                        Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                }
            }
            #endregion

            if (Model.OurRobots.Count > 6)
            {
                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                AddRoleInfo(roles, typeof(StaticPositionerRole), 1, 0);
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);
                int? SPR = null;
                SPR = getID(assigenroles, typeof(StaticPositionerRole));
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, SPR, typeof(StaticPositionerRole)))
                    Functions[SPR.Value] = (eng, wmd) => GetRole<StaticPositionerRole>(SPR.Value).perform(engine, Model, SPR.Value,new Position2D(1,3)); 
            }
            ControlParameters.BallIsMoved = ballismoved;
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            DefenceTest.MakeOutPut();
            return CurrentlyAssignedRoles;

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
            ControlParameters.BallIsMoved = false;
            oppcathball = false;
            ballismoved = false;
            PreviouslyAssignedRoles.Clear();
            FreekickDefence.RestartActiveFlags();
        }
        private void AddRoleInfo(List<RoleInfo> roles, Type role, double weight, double margin)
        {
            RoleBase r = role.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, weight, margin));
        }
        int? getID(Dictionary<int, RoleBase> current, Type roletype)
        {
            if (current.Any(a => a.Value.GetType() == roletype))
                return current.Single(a => a.Value.GetType() == roletype).Key;
            return null;
        }
    }
}
