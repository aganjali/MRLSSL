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
    class OppCornerPlay : PlayBase
    {
        Position2D lastballstate;
        bool flag = false;
        bool ballismoved = false, oppcathball = false;
        private int counter = 0;
        private bool active = false;
        private bool activeFromNormal = false;
        private double margin = .2;
        private bool noRegional = false;
        public static int RobotCounterInSpecialStrategyofImmortal = 0;
        public static bool DontShitPlease = true;
        private bool usenewmarker = true;
        private bool debug = false;
        private int lastState = 0;
        private static int idActive = 0;

        public SingleObjectState ballState = new SingleObjectState(); // defence test
        public SingleObjectState ballStateFast = new SingleObjectState(); // defence test
        private static bool wehaveActive = false;

        bool addStopcover = false;

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            return false;
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
            double Region = RegionalDefenceUtils.RegionalRegion;

            if (!(engine.EngineID == 0 && (Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent))
                && !(engine.EngineID == 1 && (Status == GameDefinitions.GameStatus.DirectFreeKick_OurTeam || Status == GameStatus.IndirectFreeKick_OurTeam)))
            {
                flag = false;
                return false;
            }
            else
            {
                if (ballState.Location.X > Region && (
                    Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent || Status == GameDefinitions.GameStatus.DirectFreeKick_OurTeam || Status == GameStatus.IndirectFreeKick_OurTeam) || flag)
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
                    //DrawingObjects.AddObject(new StringDraw((engine.GameInfo.OppTeam.BallOwner.HasValue) ? "Opp BallOwner HasValue" : "oppBallOwner dont HasValue", new Position2D(-1, 1)));
                    //DrawingObjects.AddObject(new StringDraw((engine.GameInfo.OppTeam.BallOwner.HasValue) ? "Opp BallOwner=" + engine.GameInfo.OppTeam.BallOwner.Value : "oppBallOwner dont HasValue", new Position2D(-1.1, 1)));
                    //DrawingObjects.AddObject(new StringDraw((engine.GameInfo.OppTeam.BallOwner.HasValue) ? "Opp BallOwner=" + engine.GameInfo.OppTeam.BallOwner.Value : "oppBallOwner dont HasValue", new Position2D(-1.1, 1)));
                    return true;
                }
                else
                    return false;
            }
        }

        int? getID(Dictionary<int, RoleBase> current, Type roletype)
        {
            if (current.Any(a => a.Value.GetType() == roletype))
                return current.Single(a => a.Value.GetType() == roletype).Key;
            return null;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            FreekickDefence.weAreInKickoff = false;
            FreekickDefence.SwitchToActiveReset();
            DataBridge.SetInitialPoses(Model);

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
            FreekickDefence.RearRegional = false;
            double MaxMArkDist = MarkerDefenceUtils.MaxMarkDist;
            int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            List<int> oppAttackerIds = new List<int>();
            List<int> attids = new List<int>();
            if (goalieID.HasValue)
            {
                attids = Model.Opponents.Where(w => w.Value.Location.X > -0.5 && w.Key != goalieID).Select(s => s.Key).ToList();
                oppAttackerIds = engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key)).Where(w => w.Value > 0.6).Where(w => w.Key != goalieID).Select(s => s.Key).ToList();
            }
            else
            {
                attids = Model.Opponents.Where(w => w.Value.Location.X > -0.5).Select(s => s.Key).ToList();
                oppAttackerIds = engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key)).Where(w => w.Value > 0.6).Select(s => s.Key).ToList();
            }

            oppAttackerIds.AddRange(attids);
            List<DefenderCommand> defendcommands = new List<DefenderCommand>();
            List<Position2D> points = new List<CommonClasses.MathLibrary.Position2D>() {
                                    new Position2D(0,-3),
                                    new Position2D(0,-1.5),
                                    new Position2D(0,-.5),
                                    new Position2D(0,.5),
                                    new Position2D(0,1.5),
                                    new Position2D(0,3)
                                };
            int? id = null;
            ballismoved = FreekickDefence.BallIsMoved;
            if (!oppcathball && engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(ballState.Location) < 0.12)
                oppcathball = true;
            if (ballState.Speed.Size < 10 && lastballstate.DistanceFrom(ballState.Location) > 0.07 && oppcathball)
            {
                ballismoved = true;
                FreekickDefence.BallIsMoved = true;
            }
            if (ballismoved)
                counter++;
            double dist, DistFromBorder;
            if (counter > 30 && !GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out DistFromBorder) && !wehaveActive)
            {
                if (!DataBridge.BallCutSituationCR1 && !DataBridge.BallCutSituationCR2 && !DataBridge.BallCutSituationCR3 && !DataBridge.BallCutSituationCR4)
                    active = true;
                if (FreekickDefence.switchAllMode)
                    active = true;

            }
            if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out DistFromBorder) && !wehaveActive)
            {
                if (DataBridge.getActive && Model.OurRobots[DataBridge.CutBallRobotIDCR1].Location.DistanceFrom(Model.BallState.Location) < .6)
                {
                    activeFromNormal = true;
                    idActive = DataBridge.CutBallRobotIDCR1;
                }
            }

            if (activeFromNormal)
            {
                DrawingObjects.AddObject(new Circle(Position2D.Zero, .2, new Pen(Brushes.LimeGreen, .02f)), "564546564645646456");
            }
            if (active)
            {

                DrawingObjects.AddObject(new Circle(Position2D.Zero, .2, new Pen(Brushes.HotPink, .02f)), "2121313213211313214");

            }
            if (active || activeFromNormal)
            {
                wehaveActive = true;
            }
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(GoalieCornerRole)
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole1),
                OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
            });

            if (oppAttackerIds.Count == 4)
            {
                FreekickDefence.WeAreInCorner = true;
                if (FreekickDefence.BallIsMoved)
                {
                    noRegional = true;
                }
            }
            else
            {
                FreekickDefence.WeAreInCorner = false;
            }

            #region opp < 2
            if (oppAttackerIds.Count < 3)
            {
                noRegional = false;
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

                if (!usenewmarker)
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id,
                        MarkMaximumDist = MaxMArkDist,
                    });
                }
                else
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(NewDefenderMrkerRole),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id,
                        MarkMaximumDist = MaxMArkDist,
                    });
                }
                FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;

                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);

                roles = new List<RoleInfo>();
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                Type freeRole;
                Type freerole2;
                if (!ballismoved)
                {
                    freeRole = typeof(DefendGotoPointRole);
                    freerole2 = typeof(RegionalDefenderRole2);
                }
                else
                {
                    if (wehaveActive)
                        freeRole = typeof(ActiveRole);
                    else
                    {
                        freeRole = typeof(DefendGotoPointRole);
                        FreekickDefence.DefenderGoToPointToActive = true;
                    }
                    if (!usenewmarker)
                        freerole2 = typeof(DefenderMarkerRole);
                    else
                        freerole2 = typeof(NewDefenderMrkerRole);
                }
                AddRoleInfo(roles, freeRole, 1, 0);
                AddRoleInfo(roles, typeof(RegionalDefenderRole), 1, 0);
                AddRoleInfo(roles, freerole2, 1, 0);


                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();


                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, regionalandmarker = null, regional = null, golie = null, gotopoint = null;


                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));

                regionalandmarker = getID(assigenroles, freerole2);
                regional = getID(assigenroles, typeof(RegionalDefenderRole));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                gotopoint = getID(assigenroles, freeRole);


                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                var reg = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole));
                var reg2 = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole2));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));
                DefenceInfo marker;
                if (!usenewmarker)
                    marker = infos.Single(y => y.RoleType == typeof(DefenderMarkerRole));
                else
                    marker = infos.Single(y => y.RoleType == typeof(NewDefenderMrkerRole));

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole1)))
                    Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                DefenceTest.WeHaveDefenderCornerRole1 = true;
                if (n1.HasValue)
                    DefenceTest.DefenderCornerRole1 = Model.OurRobots[n1.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (n2.HasValue)
                    DefenceTest.DefenderCornerRole2 = Model.OurRobots[n2.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(RegionalDefenderRole)))
                    Functions[regional.Value] = (eng, wmd) => GetRole<RegionalDefenderRole>(regional.Value).positionnig(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                DefenceTest.WeHaveDefenderRegionalRole1 = true;
                if (regional.HasValue)
                    DefenceTest.DefenderRegionalRole1 = Model.OurRobots[regional.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golie.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;



                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, gotopoint, freeRole))
                {
                    if (freeRole == typeof(DefendGotoPointRole))
                    {
                        Functions[gotopoint.Value] = (eng, wmd) => GetRole<DefendGotoPointRole>(gotopoint.Value).GotoPoint(Model, gotopoint.Value, Position2D.Zero,
                            (GameParameters.OppGoalCenter - Model.OurRobots[gotopoint.Value].Location).AngleInDegrees, true, true);
                    }
                    else
                        Functions[gotopoint.Value] = (eng, wmd) => GetRole<ActiveRole>(gotopoint.Value).Perform(eng, wmd, gotopoint.Value, null);
                }






                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regionalandmarker, freerole2))
                {
                    if (freerole2 == typeof(RegionalDefenderRole2))
                    {
                        Functions[regionalandmarker.Value] = (eng, wmd) => GetRole<RegionalDefenderRole2>(regionalandmarker.Value).positionnig(eng, wmd, regionalandmarker.Value, reg2.DefenderPosition.Value, reg2.Teta);
                        DefenceTest.WeHaveDefenderRegionalRole2 = true;
                        if (regionalandmarker.HasValue)
                            DefenceTest.DefenderRegionalRole2 = Model.OurRobots[regionalandmarker.Value].Location;
                    }
                    else
                    {
                        Functions[regionalandmarker.Value] = (eng, wmd) => GetRole<NewDefenderMrkerRole>(regionalandmarker.Value).mark(eng, wmd, regionalandmarker.Value, reg2.OppID);


                        DefenceTest.WeHaveDefenderMarkerRole1 = true;
                        if (regionalandmarker.HasValue)
                            DefenceTest.DefenderRegionalRole2 = Model.OurRobots[regionalandmarker.Value].Location;

                    }
                }
                FreekickDefence.freeSwitchbetweenRegionalAndMarker = false;
                lastState = 2;
            }
            #endregion

            #region opp < 4
            else if (oppAttackerIds.Count < 4)
            {
                noRegional = false;
                defendcommands.Add(new DefenderCommand()
                {

                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                });
                if (!usenewmarker)
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole),
                        MarkMaximumDist = MaxMArkDist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                    });
                    FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id;
                }
                else
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(NewDefenderMrkerRole),
                        MarkMaximumDist = MaxMArkDist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                    });
                    FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id;
                }
                if (!ballismoved)
                {
                    if (!usenewmarker)
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole2),
                            MarkMaximumDist = MaxMArkDist,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                    }
                    else
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMarkerRole2),
                            MarkMaximumDist = MaxMArkDist,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;

                    }
                    FreekickDefence.OppToMark3 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
                    if (!FreekickDefence.OppToMark3.HasValue)
                    {
                        noRegional = false;
                    }
                    if (!noRegional)
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(RegionalDefenderRole),
                            OppID = null,
                            RegionalDistFromDangerZone = 0.1,
                            RegionalDefendPoints = points,
                        });
                    }
                    else
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMarkerRole3),
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id,
                            MarkMaximumDist = MaxMArkDist,
                        });
                    }
                }
                else
                {
                    if (!usenewmarker)
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole2),
                            MarkMaximumDist = MaxMArkDist,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                        });
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;
                    }
                    else
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMarkerRole2),
                            MarkMaximumDist = MaxMArkDist,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                        });
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;
                    }
                    if (!wehaveActive)
                    {
                        if (!noRegional)
                        {
                            defendcommands.Add(new DefenderCommand()
                            {
                                RoleType = typeof(RegionalDefenderRole),
                                OppID = null,
                                RegionalDistFromDangerZone = 0.1,
                                RegionalDefendPoints = points,
                            });
                        }
                        else
                        {
                            defendcommands.Add(new DefenderCommand()
                            {
                                RoleType = typeof(NewDefenderMarkerRole3),
                                OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id,
                                MarkMaximumDist = MaxMArkDist,
                            });
                            FreekickDefence.OppToMark3 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
                        }
                    }
                }


                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
                roles = new List<RoleInfo>();
                Type freeRole;
                if (!ballismoved)
                {
                    if (!noRegional)
                        freeRole = typeof(RegionalDefenderRole);
                    else
                        freeRole = typeof(NewDefenderMarkerRole3);
                    AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);
                    AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                    if (!usenewmarker)
                    {
                        AddRoleInfo(roles, typeof(DefenderMarkerRole2), 1, 0);
                        AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                    }
                    else
                    {
                        AddRoleInfo(roles, typeof(NewDefenderMarkerRole2), 1, 0);
                        AddRoleInfo(roles, typeof(NewDefenderMrkerRole), 1, 0);

                    }
                    AddRoleInfo(roles, freeRole, 1, 0);
                }
                else
                {
                    if (wehaveActive)
                        freeRole = typeof(ActiveRole);
                    else
                    {
                        if (!noRegional)
                        {
                            freeRole = typeof(RegionalDefenderRole);
                            FreekickDefence.DefenderRegionalRole1ToActive = true;

                        }
                        else
                        {
                            freeRole = typeof(NewDefenderMarkerRole3);
                            FreekickDefence.DefenderMarkerRole3ToActive = true;

                        }

                    }




                    AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);
                    AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                    AddRoleInfo(roles, freeRole, 1, 0);
                    if (!usenewmarker)
                    {
                        AddRoleInfo(roles, typeof(DefenderMarkerRole2), 1, 0);
                        AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                    }
                    else
                    {

                        AddRoleInfo(roles, typeof(NewDefenderMarkerRole2), 1, 0);
                        AddRoleInfo(roles, typeof(NewDefenderMrkerRole), 1, 0);
                    }
                }

                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();


                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);

                int? n1 = null, n2 = null, marker = null, marker2ID = null, golierole = null, regOrActive = null;

                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                if (!usenewmarker)
                {
                    marker = getID(assigenroles, typeof(DefenderMarkerRole));
                    marker2ID = getID(assigenroles, typeof(DefenderMarkerRole2));
                }
                else
                {
                    marker = getID(assigenroles, typeof(NewDefenderMrkerRole));
                    marker2ID = getID(assigenroles, typeof(NewDefenderMarkerRole2));
                }
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golierole = Model.GoalieID;

                regOrActive = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2

                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));
                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                DefenceInfo mark;
                DefenceInfo marker2;
                if (!usenewmarker)
                {
                    mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                    marker2 = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                }
                else
                {

                    mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                    marker2 = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));
                }
                DefenceInfo reg = new DefenceInfo();
                if (!ballismoved || !wehaveActive)
                    if (!noRegional)
                        reg = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole));
                    else
                        reg = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole3));

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole1)))
                    Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                DefenceTest.WeHaveDefenderCornerRole1 = true;
                if (n1.HasValue)
                    DefenceTest.DefenderCornerRole1 = Model.OurRobots[n1.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (n2.HasValue)
                    DefenceTest.DefenderCornerRole2 = Model.OurRobots[n2.Value].Location;



                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regOrActive, freeRole))
                {
                    if (freeRole == typeof(RegionalDefenderRole))
                    {
                        Functions[regOrActive.Value] = (eng, wmd) => GetRole<RegionalDefenderRole>(regOrActive.Value).positionnig(eng, wmd, regOrActive.Value, reg.DefenderPosition.Value, reg.Teta);
                        DefenceTest.WeHaveDefenderRegionalRole1 = true;
                        if (regOrActive.HasValue)
                            DefenceTest.DefenderRegionalRole1 = Model.OurRobots[regOrActive.Value].Location;
                    }
                    else if (freeRole == typeof(NewDefenderMarkerRole3))
                    {
                        Functions[regOrActive.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole3>(regOrActive.Value).mark(eng, wmd, regOrActive.Value, reg.OppID);
                        DefenceTest.WeHaveDefenderMarkerRole3 = true;
                        if (regOrActive.HasValue)
                            DefenceTest.DefenderMarkerRole3 = Model.OurRobots[regOrActive.Value].Location;
                    }

                    else if (freeRole == typeof(ActiveRole))
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regOrActive, typeof(ActiveRole)))
                            Functions[regOrActive.Value] = (eng, wmd) => GetRole<ActiveRole>(regOrActive.Value).Perform(eng, wmd, regOrActive.Value, null);
                    }
                }

                if (!usenewmarker)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                        Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(NewDefenderMrkerRole)))
                        Functions[marker.Value] = (eng, wmd) => GetRole<NewDefenderMrkerRole>(marker.Value).mark(eng, wmd, marker.Value, mark.OppID);
                }
                DefenceTest.WeHaveDefenderMarkerRole1 = true;
                if (marker.HasValue)
                    DefenceTest.DefenderMarkerRole1 = Model.OurRobots[marker.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golierole, typeof(GoalieCornerRole)))
                    Functions[golierole.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golierole.Value).Run(engine, wmd, golierole.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golierole.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golierole.Value].Location;
                if (!usenewmarker)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker2ID, typeof(DefenderMarkerRole2)))
                        Functions[marker2ID.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(marker2ID.Value).Mark(eng, Model, marker2ID.Value, marker2.DefenderPosition.Value, marker2.Teta);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker2ID, typeof(NewDefenderMarkerRole2)))
                        Functions[marker2ID.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(marker2ID.Value).Mark(eng, wmd, marker2ID.Value, marker2.OppID);
                }
                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                if (marker2ID.HasValue)
                    DefenceTest.DefenderMarkerRole2 = Model.OurRobots[marker2ID.Value].Location;
                FreekickDefence.freeSwitchbetweenRegionalAndMarker = false;
                lastState = 3;
            }
            #endregion

            #region opp = 4
            else if (oppAttackerIds.Count == 4)
            {

                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole1),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                });
                if (!usenewmarker)
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                    });
                    FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                }
                else
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(NewDefenderMrkerRole),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                    });
                    FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                }

                if (!ballismoved || !wehaveActive)
                {
                    if (!usenewmarker)
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole2),
                            MarkMaximumDist = MaxMArkDist,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                        });
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
                    }
                    else
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMarkerRole2),
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                        });
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
                    }
                }

                FreekickDefence.OppToMark3 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 4 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id;
                if (!noRegional || FreekickDefence.OppToMark3 == null)
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(RegionalDefenderRole),
                        OppID = null,
                        RegionalDistFromDangerZone = 0.1,
                        RegionalDefendPoints = points,
                    });
                else
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(NewDefenderMarkerRole3),
                        MarkMaximumDist = MaxMArkDist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 4 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id
                    });

                }

                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);

                roles = new List<RoleInfo>();

                //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                //AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); New 
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); // New
                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                if (!usenewmarker)
                    AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                else
                    AddRoleInfo(roles, typeof(NewDefenderMrkerRole), 1, 0);
                Type freeRole;
                if (!ballismoved || !wehaveActive)
                {
                    if (!usenewmarker)
                    {
                        freeRole = typeof(DefenderMarkerRole2);
                        FreekickDefence.DefenderMarkerRole2ToActive = true;
                    }

                    else
                    {
                        freeRole = typeof(NewDefenderMarkerRole2);
                        FreekickDefence.DefenderMarkerRole2ToActive = true;
                    }
                }
                else
                {
                    freeRole = typeof(ActiveRole);
                }
                AddRoleInfo(roles, freeRole, 1, 0);
                if (!noRegional || FreekickDefence.OppToMark3 == null)
                    AddRoleInfo(roles, typeof(RegionalDefenderRole), 1, 0);
                else
                    AddRoleInfo(roles, typeof(NewDefenderMarkerRole3), 1, 0);


                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();

                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);



                int?  n2 = null, marker = null, regional = null, golie = null, mark2 = null, n3 = null;

                //n1 = getID(assigenroles, typeof(DefenderCornerRole1)); // New
                n3 = getID(assigenroles, typeof(DefenderCornerRole1)); // New
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                if (!usenewmarker)
                    marker = getID(assigenroles, typeof(DefenderMarkerRole));
                else
                    marker = getID(assigenroles, typeof(NewDefenderMrkerRole));
                if (!noRegional || FreekickDefence.OppToMark3 == null)
                    regional = getID(assigenroles, typeof(RegionalDefenderRole));
                else
                    regional = getID(assigenroles, typeof(NewDefenderMarkerRole3));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;

                mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2


                //var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal3 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));// New 
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                DefenceInfo reg;
                if (!noRegional || FreekickDefence.OppToMark3 == null)
                    reg = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole));
                else
                    reg = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole3));

                DefenceInfo mark;
                if (!usenewmarker)
                    mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                else
                    mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                DefenceInfo marker2 = new DefenceInfo();
                if (!ballismoved || !wehaveActive)
                {
                    if (!usenewmarker)
                        marker2 = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                    else
                        marker2 = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));
                }

                DrawingObjects.AddObject(new StringDraw("Its New Role \n Don't have Overlap Solving \n With Defender Corner 1", Color.HotPink, normal3.DefenderPosition.Value.Extend(.3, 0)));

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n3, typeof(DefenderCornerRole1)))
                    Functions[n3.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n3.Value).Run(eng, wmd, n3.Value, normal3.DefenderPosition.Value, normal3.Teta); //New 
                DefenceTest.WeHaveDefenderCornerRole1 = true;
                if (n3.HasValue)
                    DefenceTest.DefenderCornerRole1 = Model.OurRobots[n3.Value].Location;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (n2.HasValue)
                    DefenceTest.DefenderCornerRole2 = Model.OurRobots[n2.Value].Location;

                if (!noRegional || FreekickDefence.OppToMark3 == null)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(RegionalDefenderRole)))
                        Functions[regional.Value] = (eng, wmd) => GetRole<RegionalDefenderRole>(regional.Value).positionnig(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                    DefenceTest.WeHaveDefenderRegionalRole1 = true;
                    if (regional.HasValue)
                        DefenceTest.DefenderRegionalRole1 = Model.OurRobots[regional.Value].Location;
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(NewDefenderMarkerRole3)))
                        Functions[regional.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole3>(regional.Value).mark(eng, wmd, regional.Value, reg.OppID.Value);
                    DefenceTest.WeHaveDefenderMarkerRole3 = true;
                    if (regional.HasValue)
                        DefenceTest.DefenderMarkerRole3 = Model.OurRobots[regional.Value].Location;
                }
                if (!usenewmarker)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                        Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(NewDefenderMrkerRole)))
                        Functions[marker.Value] = (eng, wmd) => GetRole<NewDefenderMrkerRole>(marker.Value).mark(eng, wmd, marker.Value, mark.OppID);
                }
                DefenceTest.WeHaveDefenderMarkerRole1 = true;
                if (marker.HasValue)
                    DefenceTest.DefenderMarkerRole1 = Model.OurRobots[marker.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal3.DefenderPosition.Value, n3.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golie.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(DefenderMarkerRole2) || freeRole == typeof(NewDefenderMarkerRole2))
                    {
                        if (!usenewmarker)
                            Functions[mark2.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(mark2.Value).Mark(eng, Model, mark2.Value, marker2.DefenderPosition.Value, marker2.Teta);
                        else
                            Functions[mark2.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(mark2.Value).Mark(eng, wmd, mark2.Value, marker2.OppID);
                        DefenceTest.WeHaveDefenderMarkerRole2 = true;
                        if (mark2.HasValue)
                            DefenceTest.DefenderMarkerRole2 = Model.OurRobots[mark2.Value].Location;
                    }

                    else
                        Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                }
                FreekickDefence.freeSwitchbetweenRegionalAndMarker = false;
                lastState = 4;
            }
            #endregion

            #region opp >= 5
            else if (oppAttackerIds.Count < 6)
            {
                bool ballnew = false;
                if (ballnew)
                {
                    noRegional = false;
                    if (lastState == 4)
                    {
                        FreekickDefence.freeSwitchbetweenRegionalAndMarker = true;
                    }
                    else
                    {
                        FreekickDefence.freeSwitchbetweenRegionalAndMarker = false;
                    }
                    #region role Initial set
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderCornerRole4),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 4 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id
                    });
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderCornerRole1),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderCornerRole2),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                    });
                    if (!usenewmarker)
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole),
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                    else
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMrkerRole),
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                    FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                    if (!ballismoved || !wehaveActive)
                    {
                        if (!addStopcover)
                        {
                            if (!usenewmarker)
                                defendcommands.Add(new DefenderCommand()
                                {
                                    RoleType = typeof(DefenderMarkerRole2),
                                    MarkMaximumDist = MaxMArkDist,
                                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                                });
                            else
                                defendcommands.Add(new DefenderCommand()

                                {
                                    RoleType = typeof(NewDefenderMarkerRole2),
                                    MarkMaximumDist = MaxMArkDist,
                                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                                });

                        }
                        else
                        {

                            if (!usenewmarker)
                                defendcommands.Add(new DefenderCommand()
                                {
                                    RoleType = typeof(StopRole1),
                                    MarkMaximumDist = MaxMArkDist,
                                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                                });
                            else
                                defendcommands.Add(new DefenderCommand()

                                {
                                    RoleType = typeof(StopRole1),
                                    MarkMaximumDist = MaxMArkDist,
                                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                                });
                        }
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
                    }

                    #endregion
                    var infos = FreekickDefence.Match(engine, Model, defendcommands, false);
                    #region Role Add
                    roles = new List<RoleInfo>();

                    //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                    //AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); New 
                    AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); // New
                    AddRoleInfo(roles, typeof(DefenderCornerRole4), 1, 0); // New
                    AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                    if (!usenewmarker)
                        AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                    else
                        AddRoleInfo(roles, typeof(NewDefenderMrkerRole), 1, 0);

                    Type freeRole;
                    if (!ballismoved || !wehaveActive)
                    {
                        if (!addStopcover)
                        {
                            if (!usenewmarker)
                                freeRole = typeof(DefenderMarkerRole2);
                            else
                                freeRole = typeof(NewDefenderMarkerRole2);
                        }
                        else
                        {
                            freeRole = typeof(StopRole1);
                        }
                                
                    }
                    else
                        freeRole = typeof(ActiveRole);

                    AddRoleInfo(roles, freeRole, 1, 0);

                    List<int> ids = new List<int>();
                    if (Model.GoalieID.HasValue)
                        ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                    else
                        ids = Model.OurRobots.Select(s => s.Key).ToList();

                    var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);
                    #endregion
                    #region IDExport

                    int?  n2 = null, marker = null, golie = null, mark2 = null, n3 = null, n4 = null;

                    n3 = getID(assigenroles, typeof(DefenderCornerRole1)); // New
                    n4 = getID(assigenroles, typeof(DefenderCornerRole4)); // New
                    n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                    if (!usenewmarker)
                        marker = getID(assigenroles, typeof(DefenderMarkerRole));
                    else
                        marker = getID(assigenroles, typeof(NewDefenderMrkerRole));
                    if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                        golie = Model.GoalieID;

                    mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2

                    var normal3 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));// New 
                    var normal4 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole4));// New 
                    var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                    DefenceInfo mark;
                    if (!usenewmarker)
                        mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                    else
                        mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                    var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                    DefenceInfo marker2 = new DefenceInfo();
                    if (!ballismoved || !wehaveActive)
                    {
                        if (!addStopcover)
                        {
                            if (!usenewmarker)
                                marker2 = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                            else
                                marker2 = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));
                        }
                        else
                        {
                            marker2 = infos.Single(s => s.RoleType == typeof(StopRole1));
                        }
                    }
                    #endregion
                    #region Role Assigners
                    if (normal3.DefenderPosition.HasValue)
                        DrawingObjects.AddObject(new StringDraw("Its New Role \n Don't have Overlap Solving \n With Defender Corner 1", Color.HotPink, normal3.DefenderPosition.Value.Extend(.3, 0)));

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n3, typeof(DefenderCornerRole1)))
                        Functions[n3.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n3.Value).Run(eng, wmd, n3.Value, normal3.DefenderPosition.Value, normal3.Teta); //New 
                    DefenceTest.WeHaveDefenderCornerRole1 = true;
                    if (n3.HasValue)
                        DefenceTest.DefenderCornerRole1 = Model.OurRobots[n3.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n4, typeof(DefenderCornerRole4)))
                        Functions[n4.Value] = (eng, wmd) => GetRole<DefenderCornerRole4>(n4.Value).Run(eng, wmd, n4.Value, normal4.DefenderPosition.Value, normal4.Teta);
                    DefenceTest.WeHaveDefenderCornerRole4 = true;
                    if (n4.HasValue)
                        DefenceTest.DefenderCornerRole4 = Model.OurRobots[n4.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                        Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                    DefenceTest.WeHaveDefenderCornerRole2 = true;
                    if (n2.HasValue)
                        DefenceTest.DefenderCornerRole2 = Model.OurRobots[n2.Value].Location;

                    if (!usenewmarker)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                            Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                    }
                    else
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(NewDefenderMrkerRole)))
                            Functions[marker.Value] = (eng, wmd) => GetRole<NewDefenderMrkerRole>(marker.Value).mark(eng, wmd, marker.Value, mark.OppID);
                    }
                    DefenceTest.WeHaveDefenderMarkerRole1 = true;
                    if (marker.HasValue)
                        DefenceTest.DefenderMarkerRole1 = Model.OurRobots[marker.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                        Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal3.DefenderPosition.Value, n3.Value, true);
                    DefenceTest.WeHaveGoalie = true;
                    if (golie.HasValue)
                        DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                    {
                        if (!addStopcover)
                        {
                            if (freeRole == typeof(DefenderMarkerRole2) || freeRole == typeof(NewDefenderMarkerRole2))
                            {
                                if (!usenewmarker)
                                {
                                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(DefenderMarkerRole2)))
                                        Functions[mark2.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(mark2.Value).Mark(eng, wmd, mark2.Value, marker2.DefenderPosition.Value, marker2.Teta);
                                }
                                else
                                {
                                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(NewDefenderMarkerRole2)))
                                        Functions[mark2.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(mark2.Value).Mark(eng, wmd, mark2.Value, marker2.OppID);
                                }
                                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                                if (mark2.HasValue)
                                    DefenceTest.DefenderMarkerRole2 = Model.OurRobots[mark2.Value].Location;
                            }
                            else
                            {

                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(ActiveRole)))

                                    Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                            }

                        }
                        else
                        {
                            if (freeRole == typeof(StopRole1) )
                            {
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(StopRole1)))
                                    Functions[mark2.Value] = (eng, wmd) => GetRole<StopRole1>(mark2.Value).RotateRun(engine, Model, mark2.Value);//Mark(eng, wmd, mark2.Value, marker2.DefenderPosition.Value, marker2.Teta);
                               
                                DefenceTest.WeHaveDefenderMarkerRole2 = false;
                            }
                            else
                            {
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(ActiveRole)))
                                    Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                            }   
                        }
                    }
                    #endregion
                }
                else
                {
                    noRegional = false;
                    if (lastState == 4)
                    {
                        FreekickDefence.freeSwitchbetweenRegionalAndMarker = true;
                    }
                    else
                    {
                        FreekickDefence.freeSwitchbetweenRegionalAndMarker = false;
                    }
                    #region role Initial set


                    defendcommands.Add(new DefenderCommand()

                    {
                        RoleType = typeof(NewDefenderMarkerRole2),
                        MarkMaximumDist = MaxMArkDist,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                    });
                    FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 3 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderCornerRole1),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderCornerRole2),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                    });
                    if (!usenewmarker)
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole),
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                    else
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMrkerRole),
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                    FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                    if (!ballismoved || !wehaveActive)
                    {
                        if (!addStopcover)
                        {
                            if (!usenewmarker)
                                defendcommands.Add(new DefenderCommand()
                                {
                                    RoleType = typeof(DefenderMarkerRole3),
                                    MarkMaximumDist = MaxMArkDist,
                                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 4 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id

                                });
                            else
                            {
                                defendcommands.Add(new DefenderCommand()
                                {
                                    RoleType = typeof(NewDefenderMarkerRole3),
                                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 4 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id
                                });

                            }
                        }
                        else
                        {
                            defendcommands.Add(new DefenderCommand()
                            {
                                RoleType = typeof(StopRole1),
                                OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 4 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id
                            });
                        }
                        FreekickDefence.OppToMark3 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 4 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(4).Key : id;
                    }

                    #endregion
                    var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
                    #region Role Add
                    roles = new List<RoleInfo>();

                    //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                    //AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); New 
                    AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); // New
                    AddRoleInfo(roles, typeof(NewDefenderMarkerRole2), 1, 0); // New
                    AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                    if (!usenewmarker)
                        AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                    else
                        AddRoleInfo(roles, typeof(NewDefenderMrkerRole), 1, 0);

                    Type freeRole;
                    if (!ballismoved || !wehaveActive)
                    {
                        if (!addStopcover)
                        {
                            if (!usenewmarker)
                            {
                                freeRole = typeof(DefenderMarkerRole3);
                                FreekickDefence.DefenderMarkerRole3ToActive = true;
                            }
                            else
                            {
                                freeRole = typeof(NewDefenderMarkerRole3);
                                FreekickDefence.DefenderMarkerRole3ToActive = true;
                            }
                        }
                        else
                        {
                            freeRole = typeof(StopRole1);
                            FreekickDefence.DefenderMarkerRole3ToActive = true;
                        }
                    }
                    else
                        freeRole = typeof(ActiveRole);

                    AddRoleInfo(roles, freeRole, 1, 0);

                    List<int> ids = new List<int>();
                    if (Model.GoalieID.HasValue)
                        ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                    else
                        ids = Model.OurRobots.Select(s => s.Key).ToList();


                    if (freeRole == typeof(ActiveRole))
                    {

                    }

                    var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);
                    #endregion
                    #region IDExport

                    int?  n2 = null, marker = null, golie = null, mark2 = null, n3 = null, n4 = null;

                    n3 = getID(assigenroles, typeof(DefenderCornerRole1)); // New
                    n4 = getID(assigenroles, typeof(NewDefenderMarkerRole2)); // New
                    n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                    if (!usenewmarker)
                        marker = getID(assigenroles, typeof(DefenderMarkerRole));
                    else
                        marker = getID(assigenroles, typeof(NewDefenderMrkerRole));
                    if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                        golie = Model.GoalieID;

                    mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2

                    var normal3 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));// New 
                    var normal4 = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));// New 
                    var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                    DefenceInfo mark;
                    if (!usenewmarker)
                        mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                    else
                        mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                    var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                    DefenceInfo marker2 = new DefenceInfo();
                    if (!ballismoved || !wehaveActive)
                    {
                        if (!addStopcover)
                        {
                            if (!usenewmarker)
                                marker2 = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole3));
                            else
                                marker2 = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole3));
                        }
                        else
                        {
                            marker2 = infos.Single(s => s.RoleType == typeof(StopRole1));
                        }
                    }
                    #endregion
                    #region Role Assigners
                    if (normal3.DefenderPosition.HasValue)
                        DrawingObjects.AddObject(new StringDraw("Its New Role \n Don't have Overlap Solving \n With Defender Corner 1", Color.HotPink, normal3.DefenderPosition.Value.Extend(.3, 0)));

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n3, typeof(DefenderCornerRole1)))
                        Functions[n3.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n3.Value).Run(eng, wmd, n3.Value, normal3.DefenderPosition.Value, normal3.Teta); //New 
                    DefenceTest.WeHaveDefenderCornerRole1 = true;
                    if (n3.HasValue)
                        DefenceTest.DefenderCornerRole1 = Model.OurRobots[n3.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n4, typeof(NewDefenderMarkerRole2)))
                        Functions[n4.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(n4.Value).Mark(eng, wmd, n4.Value, normal4.OppID.Value);
                    DefenceTest.WeHaveDefenderCornerRole4 = true;
                    if (n4.HasValue)
                        DefenceTest.DefenderCornerRole4 = Model.OurRobots[n4.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                        Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                    DefenceTest.WeHaveDefenderCornerRole2 = true;
                    if (n2.HasValue)
                        DefenceTest.DefenderCornerRole2 = Model.OurRobots[n2.Value].Location;

                    if (!usenewmarker)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                            Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                    }
                    else
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(NewDefenderMrkerRole)))
                            Functions[marker.Value] = (eng, wmd) => GetRole<NewDefenderMrkerRole>(marker.Value).mark(eng, wmd, marker.Value, mark.OppID.Value);
                    }
                    DefenceTest.WeHaveDefenderMarkerRole1 = true;
                    if (marker.HasValue)
                        DefenceTest.DefenderMarkerRole1 = Model.OurRobots[marker.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                        Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal3.DefenderPosition.Value, n3.Value, true);
                    DefenceTest.WeHaveGoalie = true;
                    if (golie.HasValue)
                        DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                    {
                        if (!addStopcover)
                        {
                            if (freeRole == typeof(DefenderMarkerRole3) || freeRole == typeof(NewDefenderMarkerRole3))
                            {
                                if (!usenewmarker)
                                {
                                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(DefenderMarkerRole3)))
                                        Functions[mark2.Value] = (eng, wmd) => GetRole<DefenderMarkerRole3>(mark2.Value).Mark(eng, wmd, mark2.Value, marker2.DefenderPosition.Value, marker2.Teta);
                                }
                                else
                                {


                                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(NewDefenderMarkerRole3)))
                                        Functions[mark2.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole3>(mark2.Value).mark(eng, wmd, mark2.Value, marker2.OppID.Value);

                                }
                                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                                if (mark2.HasValue)
                                    DefenceTest.DefenderMarkerRole2 = Model.OurRobots[mark2.Value].Location;
                            }
                            else
                            {

                                DefenceTest.WeHaveDefenderMarkerRole2 = false;
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(ActiveRole)))
                                    Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                            }
                        }
                        else
                        {
                            if (freeRole == typeof(StopRole1))
                            {
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(StopRole1)))
                                    Functions[mark2.Value] = (eng, wmd) => GetRole<StopRole1>(mark2.Value).RotateRun(engine, Model, mark2.Value);//.Mark(eng, wmd, mark2.Value, marker2.DefenderPosition.Value, marker2.Teta);
                                DefenceTest.WeHaveDefenderMarkerRole2 = false;
                                if (mark2.HasValue)
                                    DefenceTest.DefenderMarkerRole2 = Model.OurRobots[mark2.Value].Location;
                            }
                            else
                            {
                                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(ActiveRole)))
                                    Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion


            #region opp >= 5
            else if (oppAttackerIds.Count > 5)
            {

                noRegional = false;
                if (lastState == 4)
                {
                    FreekickDefence.freeSwitchbetweenRegionalAndMarker = true;
                }
                else
                {
                    FreekickDefence.freeSwitchbetweenRegionalAndMarker = false;
                }
                #region role Initial set
                int denseRobot = accumulatedOppId2(Model, oppAttackerIds);
                Dictionary<int, float> scores = engine.GameInfo.OppTeam.Scores.Where(y => y.Key != denseRobot).ToDictionary(y => y.Key, t => t.Value);
                defendcommands.Add(new DefenderCommand()

                {
                    RoleType = typeof(NewDefenderMarkerRole2),
                    MarkMaximumDist = MaxMArkDist,
                    OppID = scores.Count > 3 ? scores.ElementAt(3).Key : id
                });
                FreekickDefence.OppToMark2 = scores.Count > 3 ? scores.ElementAt(3).Key : id;
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole1),
                    OppID = scores.Count > 0 ? scores.ElementAt(0).Key : id
                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = scores.Count > 1 ? scores.ElementAt(1).Key : id
                });
                if (!usenewmarker)
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(DefenderMarkerRole),
                        OppID = scores.Count > 2 ? scores.ElementAt(2).Key : id
                    });
                else
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(NewDefenderMrkerRole),
                        OppID = scores.Count > 2 ? scores.ElementAt(2).Key : id
                    });
                FreekickDefence.OppToMark1 = scores.Count > 2 ? scores.ElementAt(2).Key : id;
                if (!ballismoved || !wehaveActive)
                {
                    if (!usenewmarker)
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole3),
                            MarkMaximumDist = MaxMArkDist,
                            OppID = scores.Count > 4 ? scores.ElementAt(4).Key : id

                        });
                    else
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMarkerRole3),
                            OppID = scores.Count > 4 ? scores.ElementAt(4).Key : id
                        });

                    }
                    FreekickDefence.OppToMark3 = scores.Count > 4 ? scores.ElementAt(4).Key : id;
                }

                #endregion
                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
                #region Role Add
                roles = new List<RoleInfo>();

                //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                //AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); New 
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0); // New
                AddRoleInfo(roles, typeof(NewDefenderMarkerRole2), 1, 0); // New
                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                if (!usenewmarker)
                    AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                else
                    AddRoleInfo(roles, typeof(NewDefenderMrkerRole), 1, 0);

                Type freeRole;
                if (!ballismoved || !wehaveActive)
                {
                    if (!usenewmarker)
                    {
                        freeRole = typeof(DefenderMarkerRole3);
                        FreekickDefence.DefenderMarkerRole3ToActive = true;
                    }
                    else
                    {
                        freeRole = typeof(NewDefenderMarkerRole3);
                        FreekickDefence.DefenderMarkerRole3ToActive = true;
                    }
                }
                else
                    freeRole = typeof(ActiveRole);

                AddRoleInfo(roles, freeRole, 1, 0);

                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();


                if (freeRole == typeof(ActiveRole))
                {

                }

                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);
                #endregion
                #region IDExport

                int? n2 = null, marker = null, golie = null, mark2 = null, n3 = null, n4 = null;

                n3 = getID(assigenroles, typeof(DefenderCornerRole1)); // New
                n4 = getID(assigenroles, typeof(NewDefenderMarkerRole2)); // New
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                if (!usenewmarker)
                    marker = getID(assigenroles, typeof(DefenderMarkerRole));
                else
                    marker = getID(assigenroles, typeof(NewDefenderMrkerRole));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;

                mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2

                var normal3 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));// New 
                var normal4 = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));// New 
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                DefenceInfo mark;
                if (!usenewmarker)
                    mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                else
                    mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                DefenceInfo marker2 = new DefenceInfo();
                if (!ballismoved || !wehaveActive)
                {
                    if (!usenewmarker)
                        marker2 = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole3));
                    else
                        marker2 = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole3));
                }
                #endregion
                #region Role Assigners
                if (normal3.DefenderPosition.HasValue)
                    DrawingObjects.AddObject(new StringDraw("Its New Role \n Don't have Overlap Solving \n With Defender Corner 1", Color.HotPink, normal3.DefenderPosition.Value.Extend(.3, 0)));

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n3, typeof(DefenderCornerRole1)))
                    Functions[n3.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n3.Value).Run(eng, wmd, n3.Value, normal3.DefenderPosition.Value, normal3.Teta); //New 
                DefenceTest.WeHaveDefenderCornerRole1 = true;
                if (n3.HasValue)
                    DefenceTest.DefenderCornerRole1 = Model.OurRobots[n3.Value].Location;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n4, typeof(NewDefenderMarkerRole2)))
                    Functions[n4.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(n4.Value).Mark(eng, wmd, n4.Value, normal4.OppID.Value);
                DefenceTest.WeHaveDefenderCornerRole4 = true;
                if (n4.HasValue)
                    DefenceTest.DefenderCornerRole4 = Model.OurRobots[n4.Value].Location;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (n2.HasValue)
                    DefenceTest.DefenderCornerRole2 = Model.OurRobots[n2.Value].Location;

                if (!usenewmarker)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(DefenderMarkerRole)))
                        Functions[marker.Value] = (eng, wmd) => GetRole<DefenderMarkerRole>(marker.Value).Mark(eng, wmd, marker.Value, mark.DefenderPosition.Value, mark.Teta);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, marker, typeof(NewDefenderMrkerRole)))
                        Functions[marker.Value] = (eng, wmd) => GetRole<NewDefenderMrkerRole>(marker.Value).mark(eng, wmd, marker.Value, mark.OppID.Value);
                }
                DefenceTest.WeHaveDefenderMarkerRole1 = true;
                if (marker.HasValue)
                    DefenceTest.DefenderMarkerRole1 = Model.OurRobots[marker.Value].Location;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal3.DefenderPosition.Value, n3.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golie.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(DefenderMarkerRole3) || freeRole == typeof(NewDefenderMarkerRole3))
                    {
                        if (!usenewmarker)
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(DefenderMarkerRole3)))
                                Functions[mark2.Value] = (eng, wmd) => GetRole<DefenderMarkerRole3>(mark2.Value).Mark(eng, wmd, mark2.Value, marker2.DefenderPosition.Value, marker2.Teta);
                        }
                        else
                        {


                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(NewDefenderMarkerRole3)))
                                Functions[mark2.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole3>(mark2.Value).mark(eng, wmd, mark2.Value, marker2.OppID.Value);

                        }
                        DefenceTest.WeHaveDefenderMarkerRole2 = true;
                        if (mark2.HasValue)
                            DefenceTest.DefenderMarkerRole2 = Model.OurRobots[mark2.Value].Location;
                    }
                    else
                    {

                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, typeof(ActiveRole)))
                            Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                    }
                }
                #endregion

            }
            #endregion
            #region Switches Handling
            FreekickDefence.LastSwitchDefender2Marker1 = FreekickDefence.SwitchDefender2Marker1;//New IO2014
            FreekickDefence.LastSwitchDefender2Marker2 = FreekickDefence.SwitchDefender2Marker2;//New IO2014 
            FreekickDefence.LastSwitchDefender2Marker3 = FreekickDefence.SwitchDefender2Marker3;//New IO2014
            FreekickDefence.LastSwitchDefender32Marker1 = FreekickDefence.SwitchDefender32Marker1;//New IO2014
            FreekickDefence.LastSwitchDefender32Marker2 = FreekickDefence.SwitchDefender32Marker2;//New IO2014
            FreekickDefence.LastSwitchDefender32Marker3 = FreekickDefence.SwitchDefender32Marker3;//New IO2014
            FreekickDefence.LastSwitchDefender42Marker1 = FreekickDefence.SwitchDefender42Marker1;//New IO2014
            FreekickDefence.LastSwitchDefender42Marker2 = FreekickDefence.SwitchDefender42Marker2;//New IO2014
            FreekickDefence.LastSwitchDefender42Marker3 = FreekickDefence.SwitchDefender42Marker3;//New IO2014
            #endregion

            ControlParameters.BallIsMoved = ballismoved;
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;

            DefenceTest.MakeOutPut();

            return CurrentlyAssignedRoles;
        }

        private void AddRoleInfo(List<RoleInfo> roles, Type role, double weight, double margin)
        {
            RoleBase r = role.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, weight, margin));
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
            DataBridge.BallCutSituationCR1 = false;
            DataBridge.BallCutSituationCR2 = false;
            DataBridge.BallCutSituationCR3 = false;
            DataBridge.BallCutSituationCR4 = false;
            DataBridge.getActive = false;
            wehaveActive = false;
            activeFromNormal = false;
            ControlParameters.BallIsMoved = false;
            ballismoved = false;
            oppcathball = false;
            active = false;
            counter = 0;
            PreviouslyAssignedRoles.Clear();
            FreekickDefence.RestartActiveFlags();
            GoalieCornerRole.ballinRoll = false;
            GoalieCornerRole.firstTimeBallInChipKick = true;
            GoalieCornerRole.ballSavedPos = new Position2D();
            GoalieCornerRole.RobotLoc = new Position2D();
            //GoalieCornerRole.firstTimeAngle = true;
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
    }
}
