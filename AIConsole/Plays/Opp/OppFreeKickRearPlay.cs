using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OppFreeKickRearPlay : PlayBase
    {
        Position2D lastballstate;
        bool flag = false;
        bool rearGotoPoint = false;

        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
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
            if (!(engine.EngineID == 0 && (Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent))
                && !(engine.EngineID == 1 && (Status == GameDefinitions.GameStatus.DirectFreeKick_OurTeam || Status == GameStatus.IndirectFreeKick_OurTeam)))
            {
                flag = false;
                return false;
            }
            else
            {
                if (ballState.Location.X < -.2 && (
                    Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_OurTeam || Status == GameStatus.DirectFreeKick_OurTeam) || flag)
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

        int? getID(Dictionary<int, RoleBase> current, Type roletype)
        {
            if (current.Any(a => a.Value.GetType() == roletype))
                return current.Single(a => a.Value.GetType() == roletype).Key;
            return null;
        }
        bool ballismoved = false, oppcathball = false;
        private bool usenewmarker = true;
        private bool debug = true;

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            FreekickDefence.weAreInKickoff = false;
            FreekickDefence.SwitchToActiveReset();
            DefenceTest.BallTest = FreekickDefence.testDefenceState;
            DefenceTest.GenerateBallPos();
            Planner.IsStopBall(!FreekickDefence.BallIsMoved);
            //Planner.IsStopBall(true);

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
            FreekickDefence.WeAreInCorner = false;
            //if (engine.GameInfo.OurTeam.BallOwner.HasValue)
            //{
            //    DrawingObjects.AddObject(new Circle(Model.OurRobots[engine.GameInfo.OurTeam.BallOwner.Value].Location, .15, new Pen(Color.DarkOrange, .02f)));
            //}
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            //float max = float.MinValue;
            int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            List<int> attids = new List<int>();
            List<int> oppAttackerIds = new List<int>();
            if (goalieID.HasValue)
            {
                attids = Model.Opponents.Where(w => w.Value.Location.X > -1.5 && w.Key != goalieID.Value).Select(s => s.Key).ToList();
                oppAttackerIds=  engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key) && w.Key != goalieID).Where(w => w.Value > 0.6).Select(s => s.Key).ToList();
            }
            else
            {
                attids = Model.Opponents.Where(w => w.Value.Location.X > -1.5).Select(s => s.Key).ToList();
               oppAttackerIds= engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key)).Where(w => w.Value > 0.6).Select(s => s.Key).ToList();
            }
            if (!ballismoved && ballState.Location.X < -1.66)
                rearGotoPoint = true;
            Position2D TargetLocation = Position2D.Zero;
            double Teta = 0;
            if (rearGotoPoint)
            {
                TargetLocation = ballState.Location + (GameParameters.OurGoalCenter - ballState.Location).GetNormalizeToCopy(1.55);
                Teta = (ballState.Location - GameParameters.OurGoalCenter).AngleInDegrees;
            }
            //if (engine.GameInfo.OppTeam.Scores.Count > 0)
            //    max = engine.GameInfo.OppTeam.Scores.Max(s => s.Value);
            ///
            /// 
            /// scroe normalization is canceled
            /// 
            ///

           
            oppAttackerIds.AddRange(attids);
            foreach (var item in oppAttackerIds)
            {
                DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, .12, new Pen(Brushes.HotPink, .02f)), Model.Opponents[item].Location.X.ToString());
            }
            List<DefenderCommand> defendcommands = new List<DefenderCommand>();

            List<Position2D> points = new List<CommonClasses.MathLibrary.Position2D>() {
                                    new Position2D(0, -3),
                                    new Position2D(0,-1.5),
                                    new Position2D(0,-.5),
                                    new Position2D(0,.5),
                                    new Position2D(0,1.5),
                                    new Position2D(0,3),
                                };

            int? id = null;

            ballismoved = FreekickDefence.BallIsMoved;
            if (!oppcathball && engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(ballState.Location) < 0.15)
                oppcathball = true;
            if (ballState.Speed.Size < 10 && lastballstate.DistanceFrom(ballState.Location) > 0.07 && oppcathball)
            {
                ballismoved = true;
                FreekickDefence.BallIsMoved = true;
            }

            Type freeRole;
            if (!FreekickDefence.StopToActive)
            {
                freeRole = typeof(RearStopRole);
            }
            else
                freeRole = typeof(ActiveRole);


            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(GoalieCornerRole)
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole1),
                OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
            });

            #region opp < 2
            if (oppAttackerIds.Count < 2)
            {

                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id,

                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(RegionalDefenderRole),
                    OppID = null,
                    RegionalDistFromDangerZone = 0.1,
                    RegionalDefendPoints = points,
                });

                Type t;

                if (!ballismoved)
                {
                    t = typeof(RegionalDefenderRole2);
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(RegionalDefenderRole2),
                        OppID = null,
                        RegionalDistFromDangerZone = 0.1,
                        RegionalDefendPoints = points,
                    });
                }
                else
                {
                    t = typeof(NewDefenderMarkerRole2);
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = typeof(NewDefenderMarkerRole2),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id,
                        RegionalDistFromDangerZone = 0.1,
                        RegionalDefendPoints = points,
                    });
                    FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;
                }
                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);

                roles = new List<RoleInfo>();
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                AddRoleInfo(roles, freeRole, 1, 0);
                AddRoleInfo(roles, typeof(RegionalDefenderRole), 1, 0);
                AddRoleInfo(roles, t, 1, 0);
                AddRoleInfo(roles, typeof(CornerStopRole), 1, 0);
                AddRoleInfo(roles, typeof(strategyPositioner1Role), 1, 0);



                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, regional2 = null, regional = null, golie = null, gotopoint = null, attacker = null, stopCover = null ;


                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));

                regional2 = getID(assigenroles, t);
                regional = getID(assigenroles, typeof(RegionalDefenderRole));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                gotopoint = getID(assigenroles, freeRole);
                stopCover = getID(assigenroles, typeof(CornerStopRole));
                attacker = getID(assigenroles, typeof(strategyPositioner1Role));

                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                var reg = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole));
                var reg2 = infos.Single(s => s.RoleType == t);
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));
                #region new attacker and rotational stopcover 

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stopCover, typeof(CornerStopRole)))
                    Functions[stopCover.Value] = (eng, wmd) => GetRole<CornerStopRole>(stopCover.Value).Run(engine, Model, stopCover.Value, -20, 90);



                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacker, typeof(strategyPositioner1Role)))
                    Functions[attacker.Value] = (eng, wmd) => GetRole<strategyPositioner1Role>(attacker.Value).Perform(engine, Model, attacker.Value);
                #endregion
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
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional2, t))
                {
                    if (!ballismoved)
                    {
                        Functions[regional2.Value] = (eng, wmd) => GetRole<RegionalDefenderRole2>(regional2.Value).positionnig(eng, wmd, regional2.Value, reg2.DefenderPosition.Value, reg2.Teta);
                        DefenceTest.WeHaveDefenderRegionalRole2 = true;
                        if (regional2.HasValue)
                            DefenceTest.DefenderRegionalRole2 = Model.OurRobots[regional2.Value].Location;
                    }
                    else
                    {
                        if (!usenewmarker)
                        {
                            Functions[regional2.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(regional2.Value).Mark(eng, wmd, regional2.Value, reg2.DefenderPosition.Value, reg2.Teta);

                        }
                        else
                            Functions[regional2.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(regional2.Value).Mark(eng, wmd, regional2.Value, reg2.OppID);
                        DefenceTest.WeHaveDefenderMarkerRole2 = true;
                        if (regional2.HasValue)
                            DefenceTest.DefenderMarkerRole2 = Model.OurRobots[regional2.Value].Location;
                    }
                }

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, golie, typeof(GoalieCornerRole)))
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golie.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, gotopoint, freeRole))
                {
                    if (freeRole == typeof(RearStopRole))
                    {
                        Functions[gotopoint.Value] = (eng, wmd) => GetRole<RearStopRole>(gotopoint.Value).Run(eng, wmd, gotopoint.Value);
                        DefenceTest.WeHaveStopCover1 = true;
                        if (gotopoint.HasValue)
                            DefenceTest.StopCover1 = Model.OurRobots[gotopoint.Value].Location;
                    }
                    else if (freeRole == typeof(ActiveRole))
                        Functions[gotopoint.Value] = (eng, wmd) => GetRole<ActiveRole>(gotopoint.Value).Perform(eng, wmd, gotopoint.Value, null);
                    //////else
                    //////    Functions[gotopoint.Value] = (eng, wmd) => GetRole<DefendGotoPointRole>(gotopoint.Value).GotoPoint(wmd, gotopoint.Value, TargetLocation, Teta, true, true);

                }
                #region debug
                if (debug)
                {
                    if (n1.HasValue && normal1.OppID.HasValue)
                    {
                        Position2D secondp = normal1.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n1.Value].Location;
                        Position2D thirdp = Model.Opponents[normal1.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "3546465465");
                        DrawingObjects.AddObject(secondLine, "10321231654");
                        DrawingObjects.AddObject(new Circle(normal1.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "453774343");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n1.Value].Location, .11, new Pen(Brushes.White, .01f)), "534354373");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal1.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434634243");
                    }
                    if (n2.HasValue && normal2.OppID.HasValue)
                    {
                        Position2D secondp = normal2.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n2.Value].Location;
                        Position2D thirdp = Model.Opponents[normal2.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "5664631321");
                        DrawingObjects.AddObject(secondLine, "12213213456");
                        DrawingObjects.AddObject(new Circle(normal2.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "434344636");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n2.Value].Location, .11, new Pen(Brushes.White, .01f)), "24524363434");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal2.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45342463");
                    }
                    if (regional2.HasValue && reg2.OppID.HasValue && !usenewmarker)
                    {
                        Position2D secondp = reg2.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[regional2.Value].Location;
                        Position2D thirdp = Model.Opponents[reg2.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "165432131");
                        DrawingObjects.AddObject(secondLine, "12213213");
                        DrawingObjects.AddObject(new Circle(reg2.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "45324344053");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional2.Value].Location, .11, new Pen(Brushes.White, .01f)), "2010603");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg2.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45205364");

                    }
                    else if (regional2.HasValue && reg2.OppID.HasValue && usenewmarker)
                    {
                        Position2D firstp = Model.OurRobots[regional2.Value].Location;
                        Position2D thirdp = Model.Opponents[reg2.OppID.Value].Location;
                        Line firstLine = new Line(firstp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "53773873");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional2.Value].Location, .11, new Pen(Brushes.White, .01f)), "3697445396336");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg2.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434269796967");

                    }
                }
                #endregion

            }
            #endregion

            #region opp < 4
            else if (oppAttackerIds.Count < 4)
            {
                FreekickDefence.RearRegional = true;
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                });
                if (!usenewmarker)
                {
                    if (oppAttackerIds.Count == 2)
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole),
                            MarkMaximumDist = 3,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                        });
                    else
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole),
                            MarkMaximumDist = 3,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                }
                else
                {
                    if (oppAttackerIds.Count == 2)
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMrkerRole),
                            MarkMaximumDist = 3,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                        });
                        FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id;
                    }
                    else
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMrkerRole),
                            MarkMaximumDist = 3,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                        });
                        FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                    }
                }
                if (!ballismoved)
                {
                    //if (oppAttackerIds.Count == 2)
                    //    defendcommands.Add(new DefenderCommand() { RoleType = typeof(DefenderMarkerRole), MarkMaximumDist = 3, OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(1).Key : id });
                    //else
                    //    defendcommands.Add(new DefenderCommand() { RoleType = typeof(DefenderMarkerRole), MarkMaximumDist = 3, OppID = engine.GameInfo.OppTeam.Scores.Count > 2 ? engine.GameInfo.OppTeam.Scores.ElementAt(2).Key : id });

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
                    //if (oppAttackerIds.Count == 2)
                    //    defendcommands.Add(new DefenderCommand() { RoleType = typeof(DefenderMarkerRole), MarkMaximumDist = 3, OppID = engine.GameInfo.OppTeam.Scores.Count > 1 ? engine.GameInfo.OppTeam.Scores.ElementAt(1).Key : id });
                    //else
                    //    defendcommands.Add(new DefenderCommand() { RoleType = typeof(DefenderMarkerRole), MarkMaximumDist = 3, OppID = engine.GameInfo.OppTeam.Scores.Count > 2 ? engine.GameInfo.OppTeam.Scores.ElementAt(2).Key : id });
                    if (!usenewmarker)
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(DefenderMarkerRole2),
                            MarkMaximumDist = 3,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Where(h => h.Key != goalieID).ToList().ElementAt(0).Key : id
                        });

                    }
                    else
                    {
                        defendcommands.Add(new DefenderCommand()
                        {
                            RoleType = typeof(NewDefenderMarkerRole2),
                            MarkMaximumDist = 3,
                            OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Where(h => h.Key != goalieID).ToList().ElementAt(0).Key : id
                        });
                        FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Where(h => h.Key != goalieID).ToList().ElementAt(0).Key : id;
                    }
                }


                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);

                roles = new List<RoleInfo>();

                //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);

                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                AddRoleInfo(roles, freeRole, 1, 0);
                if (!usenewmarker)
                    AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                else
                    AddRoleInfo(roles, typeof(NewDefenderMrkerRole), 1, 0);
                if (!ballismoved)
                {
                    //AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                    AddRoleInfo(roles, typeof(RegionalDefenderRole), 1, 0);
                }
                else
                {
                    //AddRoleInfo(roles, typeof(DefenderMarkerRole), 1, 0);
                    if (!usenewmarker)
                        AddRoleInfo(roles, typeof(DefenderMarkerRole2), 1, 0);
                    else
                        AddRoleInfo(roles, typeof(NewDefenderMarkerRole2), 1, 0);
                }
                AddRoleInfo(roles, typeof(CornerStopRole), 1, 0);
                AddRoleInfo(roles, typeof(strategyPositioner1Role), 1, 0);

                


                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, marker = null, regional = null, golie = null, mark2 = null,stopCover = null,attacker = null;

                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                if (!usenewmarker)
                    marker = getID(assigenroles, typeof(DefenderMarkerRole));
                else
                    marker = getID(assigenroles, typeof(NewDefenderMrkerRole));
                if (!ballismoved)
                    regional = getID(assigenroles, typeof(RegionalDefenderRole));
                else
                {
                    if (!usenewmarker)
                        regional = getID(assigenroles, typeof(DefenderMarkerRole2));
                    else
                        regional = getID(assigenroles, typeof(NewDefenderMarkerRole2));
                }
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                mark2 = getID(assigenroles, freeRole);
                stopCover = getID(assigenroles, typeof(CornerStopRole));
                attacker = getID(assigenroles, typeof(strategyPositioner1Role));


                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));

                DefenceInfo reg;
                if (!ballismoved)
                    reg = infos.Single(s => s.RoleType == typeof(RegionalDefenderRole));
                else
                {
                    if (!usenewmarker)
                        reg = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                    else
                        reg = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));
                }
                DefenceInfo mark;
                if (!usenewmarker)
                    mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                else
                    mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                #region new attacker and rotational stopcover 

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stopCover, typeof(CornerStopRole)))
                    Functions[stopCover.Value] = (eng, wmd) => GetRole<CornerStopRole>(stopCover.Value).Run(engine, Model, stopCover.Value, -20, 90);



                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacker, typeof(strategyPositioner1Role)))
                    Functions[attacker.Value] = (eng, wmd) => GetRole<strategyPositioner1Role>(attacker.Value).Perform(engine, Model, attacker.Value);
                #endregion
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
                if (!ballismoved)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(RegionalDefenderRole)))
                        Functions[regional.Value] = (eng, wmd) => GetRole<RegionalDefenderRole>(regional.Value).positionnig(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                    DefenceTest.WeHaveDefenderRegionalRole1 = true;
                    if (regional.HasValue)
                        DefenceTest.DefenderRegionalRole1 = Model.OurRobots[regional.Value].Location;
                }
                else
                {
                    if (!usenewmarker)
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(DefenderMarkerRole2)))
                            Functions[regional.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                    }
                    else
                    {
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(NewDefenderMarkerRole2)))
                            Functions[regional.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.OppID);
                    }
                    DefenceTest.WeHaveDefenderMarkerRole2 = true;
                    if (regional.HasValue)
                        DefenceTest.DefenderMarkerRole2 = Model.OurRobots[regional.Value].Location;
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
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, new DefenceInfo(), normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golie.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(RearStopRole))
                    {
                        Functions[mark2.Value] = (eng, wmd) => GetRole<RearStopRole>(mark2.Value).Run(eng, wmd, mark2.Value);
                        DefenceTest.WeHaveStopCover1 = true;
                        if (mark2.HasValue)
                            DefenceTest.StopCover1 = Model.OurRobots[mark2.Value].Location;
                    }
                    else if (freeRole == typeof(ActiveRole))
                        Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                    //else
                    //    Functions[mark2.Value] = (eng, wmd) => GetRole<DefendGotoPointRole>(mark2.Value).GotoPoint(wmd, mark2.Value, TargetLocation, Teta, true, true);
                }
                #region debug
                if (debug)
                {
                    if (n1.HasValue && normal1.OppID.HasValue)
                    {
                        Position2D secondp = normal1.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n1.Value].Location;
                        Position2D thirdp = Model.Opponents[normal1.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "3546465465");
                        DrawingObjects.AddObject(secondLine, "10321231654");
                        DrawingObjects.AddObject(new Circle(normal1.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "453774343");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n1.Value].Location, .11, new Pen(Brushes.White, .01f)), "534354373");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal1.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434634243");
                    }
                    if (n2.HasValue && normal2.OppID.HasValue)
                    {
                        Position2D secondp = normal2.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n2.Value].Location;
                        Position2D thirdp = Model.Opponents[normal2.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "5664631321");
                        DrawingObjects.AddObject(secondLine, "12213213456");
                        DrawingObjects.AddObject(new Circle(normal2.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "434344636");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n2.Value].Location, .11, new Pen(Brushes.White, .01f)), "24524363434");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal2.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45342463");
                    }
                    if (marker.HasValue && mark.OppID.HasValue && !usenewmarker)
                    {
                        Position2D secondp = mark.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[marker.Value].Location;
                        Position2D thirdp = Model.Opponents[mark.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "165432131");
                        DrawingObjects.AddObject(secondLine, "12213213");
                        DrawingObjects.AddObject(new Circle(mark.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "45324344053");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[marker.Value].Location, .11, new Pen(Brushes.White, .01f)), "2010603");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[mark.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45205364");

                    }
                    else if (marker.HasValue && mark.OppID.HasValue && usenewmarker)
                    {
                        Position2D firstp = Model.OurRobots[marker.Value].Location;
                        Position2D thirdp = Model.Opponents[mark.OppID.Value].Location;
                        Line firstLine = new Line(firstp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "53773873");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[marker.Value].Location, .11, new Pen(Brushes.White, .01f)), "3697445396336");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[mark.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434269796967");

                    }
                    if (regional.HasValue && reg.OppID.HasValue && !usenewmarker)
                    {
                        Position2D secondp = reg.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[regional.Value].Location;
                        Position2D thirdp = Model.Opponents[reg.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "555968795464");
                        DrawingObjects.AddObject(secondLine, "51324165464");
                        DrawingObjects.AddObject(new Circle(reg.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "205463653");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional.Value].Location, .11, new Pen(Brushes.White, .01f)), "634537634");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "453534");
                    }
                    else if (regional.HasValue && reg.OppID.HasValue && usenewmarker)
                    {
                        Position2D firstp = Model.OurRobots[regional.Value].Location;
                        Position2D thirdp = Model.Opponents[reg.OppID.Value].Location;
                        Line firstLine = new Line(firstp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "2134656454");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional.Value].Location, .11, new Pen(Brushes.White, .01f)), "9876546465");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "3213216564");
                    }
                }
                #endregion
            }
            #endregion

            #region opp < 5
            else if (oppAttackerIds.Count < 5)
            {
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = typeof(DefenderCornerRole2),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(1).Key : id
                });

                defendcommands.Add(new DefenderCommand()
                {


                    RoleType = (!usenewmarker) ? typeof(DefenderMarkerRole) : typeof(NewDefenderMrkerRole),
                    MarkMaximumDist = 3,
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                });
                FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                if (!ballismoved)
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2),
                        MarkMaximumDist = 3,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                    });
                    FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
                }
                else
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2),
                        MarkMaximumDist = 3,
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });
                    FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;
                }

                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);

                roles = new List<RoleInfo>();

                //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);
                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                AddRoleInfo(roles, (!usenewmarker) ? typeof(DefenderMarkerRole) : typeof(NewDefenderMrkerRole), 1, 0);

                AddRoleInfo(roles, freeRole, 1, 0);

                AddRoleInfo(roles, (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2), 1, 0);
                AddRoleInfo(roles, typeof(CornerStopRole), 1, 0);
                AddRoleInfo(roles, typeof(strategyPositioner1Role), 1, 0);

                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, marker = null, regional = null, golie = null, mark2 = null, stopCover = null, attacker = null ;

                n1 = getID(assigenroles, typeof(DefenderCornerRole1));
                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                marker = getID(assigenroles, (!usenewmarker) ? typeof(DefenderMarkerRole) : typeof(NewDefenderMrkerRole));
                regional = getID(assigenroles, (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2));

                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;

                mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2
                stopCover = getID(assigenroles, typeof(CornerStopRole));
                attacker = getID(assigenroles, typeof(strategyPositioner1Role));

                var normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                DefenceInfo reg;
                DefenceInfo mark;
                if (!usenewmarker)
                {
                    reg = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                    mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                }
                else
                {
                    reg = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));
                    mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                }
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                #region new attacker and rotational stopcover 

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stopCover, typeof(CornerStopRole)))
                    Functions[stopCover.Value] = (eng, wmd) => GetRole<CornerStopRole>(stopCover.Value).Run(engine, Model, stopCover.Value, -20, 90);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacker, typeof(strategyPositioner1Role)))
                    Functions[attacker.Value] = (eng, wmd) => GetRole<strategyPositioner1Role>(attacker.Value).Perform(engine, Model, attacker.Value);
                #endregion
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
                if (!usenewmarker)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(DefenderMarkerRole2)))
                        Functions[regional.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(NewDefenderMarkerRole2)))
                        Functions[regional.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.OppID);
                }
                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                if (regional.HasValue)
                    DefenceTest.DefenderMarkerRole2 = Model.OurRobots[regional.Value].Location;
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
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golie.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(RearStopRole))
                    {
                        Functions[mark2.Value] = (eng, wmd) => GetRole<RearStopRole>(mark2.Value).Run(eng, wmd, mark2.Value);
                        if (mark2.HasValue)
                            DefenceTest.StopCover1 = Model.OurRobots[mark2.Value].Location;
                        DefenceTest.WeHaveStopCover1 = true;
                    }
                    else if (freeRole == typeof(ActiveRole))
                        Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                    //else
                    //    Functions[mark2.Value] = (eng, wmd) => GetRole<DefendGotoPointRole>(mark2.Value).GotoPoint(wmd, mark2.Value, TargetLocation, Teta, true, true);
                }
                #region debug
                if (debug)
                {
                    if (n1.HasValue && normal1.OppID.HasValue)
                    {
                        Position2D secondp = normal1.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n1.Value].Location;
                        Position2D thirdp = Model.Opponents[normal1.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "3546465465");
                        DrawingObjects.AddObject(secondLine, "10321231654");
                        DrawingObjects.AddObject(new Circle(normal1.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "453774343");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n1.Value].Location, .11, new Pen(Brushes.White, .01f)), "534354373");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal1.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434634243");
                    }
                    if (n2.HasValue && normal2.OppID.HasValue)
                    {
                        Position2D secondp = normal2.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n2.Value].Location;
                        Position2D thirdp = Model.Opponents[normal2.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "5664631321");
                        DrawingObjects.AddObject(secondLine, "12213213456");
                        DrawingObjects.AddObject(new Circle(normal2.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "434344636");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n2.Value].Location, .11, new Pen(Brushes.White, .01f)), "24524363434");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal2.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45342463");
                    }
                    if (marker.HasValue && mark.OppID.HasValue && !usenewmarker)
                    {
                        Position2D secondp = mark.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[marker.Value].Location;
                        Position2D thirdp = Model.Opponents[mark.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "165432131");
                        DrawingObjects.AddObject(secondLine, "12213213");
                        DrawingObjects.AddObject(new Circle(mark.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "45324344053");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[marker.Value].Location, .11, new Pen(Brushes.White, .01f)), "2010603");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[mark.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45205364");

                    }
                    else if (marker.HasValue && mark.OppID.HasValue && usenewmarker)
                    {
                        Position2D firstp = Model.OurRobots[marker.Value].Location;
                        Position2D thirdp = Model.Opponents[mark.OppID.Value].Location;
                        Line firstLine = new Line(firstp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "53773873");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[marker.Value].Location, .11, new Pen(Brushes.White, .01f)), "3697445396336");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[mark.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434269796967");

                    }
                    if (regional.HasValue && reg.OppID.HasValue && !usenewmarker)
                    {
                        Position2D secondp = reg.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[regional.Value].Location;
                        Position2D thirdp = Model.Opponents[reg.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "555968795464");
                        DrawingObjects.AddObject(secondLine, "51324165464");
                        DrawingObjects.AddObject(new Circle(reg.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "205463653");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional.Value].Location, .11, new Pen(Brushes.White, .01f)), "634537634");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "453534");
                    }
                    else if (regional.HasValue && reg.OppID.HasValue && usenewmarker)
                    {
                        Position2D firstp = Model.OurRobots[regional.Value].Location;
                        Position2D thirdp = Model.Opponents[reg.OppID.Value].Location;
                        Line firstLine = new Line(firstp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "2134656454");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional.Value].Location, .11, new Pen(Brushes.White, .01f)), "9876546465");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "3213216564");
                    }
                }
                #endregion
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
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id,
                    willUse = ((!ballismoved) ? false : true)
                });
                defendcommands.Add(new DefenderCommand()
                {
                    RoleType = (!usenewmarker) ? typeof(DefenderMarkerRole) : typeof(NewDefenderMrkerRole),
                    OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id
                });
                FreekickDefence.OppToMark1 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(2).Key : id;
                if (!ballismoved)
                {
                    defendcommands.Add(new DefenderCommand()
                    {
                        RoleType = (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id
                    });
                    FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 2 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(3).Key : id;
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
                        RoleType = (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2),
                        OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id
                    });
                    FreekickDefence.OppToMark2 = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 1 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : id;
                    //defendcommands.Add(new DefenderCommand()
                    //{
                    //    RoleType = typeof(DefenderCornerRole1),
                    //    OppID = engine.GameInfo.OppTeam.Scores.Count > 0 ? engine.GameInfo.OppTeam.Scores.ElementAt(0).Key : id
                    //});
                }



                var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
                roles = new List<RoleInfo>();

                //AddRoleInfo(roles, typeof(GoalieCornerRole), 1, 0);
                if (!ballismoved)
                    AddRoleInfo(roles, typeof(DefenderCornerRole3), 1, 0);
                else
                    AddRoleInfo(roles, typeof(DefenderCornerRole1), 1, 0);


                AddRoleInfo(roles, typeof(DefenderCornerRole2), 1, 0);
                AddRoleInfo(roles, (!usenewmarker) ? typeof(DefenderMarkerRole) : typeof(NewDefenderMrkerRole), 1, 0);
                AddRoleInfo(roles, freeRole, 1, 0);
                AddRoleInfo(roles, (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2), 1, 0);
                AddRoleInfo(roles, typeof(CornerStopRole), 1, 0);
                AddRoleInfo(roles, typeof(strategyPositioner1Role), 1, 0);



                List<int> ids = new List<int>();
                if (Model.GoalieID.HasValue)
                    ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
                else
                    ids = Model.OurRobots.Select(s => s.Key).ToList();
                var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);


                int? n1 = null, n2 = null, marker = null, regional = null, golie = null, mark2 = null,attacker=null,stopCover=null;

                if (!ballismoved)
                    n1 = getID(assigenroles, typeof(DefenderCornerRole3));
                else
                    n1 = getID(assigenroles, typeof(DefenderCornerRole1));

                n2 = getID(assigenroles, typeof(DefenderCornerRole2));
                marker = getID(assigenroles, (!usenewmarker) ? typeof(DefenderMarkerRole) : typeof(NewDefenderMrkerRole));
                regional = getID(assigenroles, (!usenewmarker) ? typeof(DefenderMarkerRole2) : typeof(NewDefenderMarkerRole2));
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                    golie = Model.GoalieID;
                mark2 = getID(assigenroles, freeRole);  ///////////////////////////////////// TODO: OR DEFENDER 2
                stopCover = getID(assigenroles, typeof(CornerStopRole));
                attacker = getID(assigenroles, typeof(strategyPositioner1Role));

                DefenceInfo normal1 = new DefenceInfo();
                if (!ballismoved)
                    normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole3));
                else
                    normal1 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole1));
                DefenceInfo reg;
                DefenceInfo mark;
                var normal2 = infos.Single(s => s.RoleType == typeof(DefenderCornerRole2));
                if (!usenewmarker)
                {
                    reg = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole2));
                    mark = infos.Single(s => s.RoleType == typeof(DefenderMarkerRole));
                }
                else
                {
                    reg = infos.Single(s => s.RoleType == typeof(NewDefenderMarkerRole2));
                    mark = infos.Single(s => s.RoleType == typeof(NewDefenderMrkerRole));
                }
                var gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));

                #region new attacker and rotational stopcover 

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stopCover, typeof(CornerStopRole)))
                    Functions[stopCover.Value] = (eng, wmd) => GetRole<CornerStopRole>(stopCover.Value).Run(engine,Model, stopCover.Value, -20,90);



                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, attacker, typeof(strategyPositioner1Role)))
                    Functions[attacker.Value] = (eng, wmd) => GetRole<strategyPositioner1Role>(attacker.Value).Perform(engine,Model, attacker.Value);
                #endregion
                if (!ballismoved)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole3)))
                        Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole3>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                    DefenceTest.WeHaveDefenderCornerRole3 = true;
                    if (n1.HasValue)
                        DefenceTest.DefenderCornerRole3 = Model.OurRobots[n1.Value].Location;
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n1, typeof(DefenderCornerRole1)))
                        Functions[n1.Value] = (eng, wmd) => GetRole<DefenderCornerRole1>(n1.Value).Run(eng, wmd, n1.Value, normal1.DefenderPosition.Value, normal1.Teta);
                    DefenceTest.WeHaveDefenderCornerRole1 = true;
                    if (n1.HasValue)
                        DefenceTest.DefenderCornerRole1 = Model.OurRobots[n1.Value].Location;
                }
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, n2, typeof(DefenderCornerRole2)))
                    Functions[n2.Value] = (eng, wmd) => GetRole<DefenderCornerRole2>(n2.Value).Run(eng, wmd, n2.Value, normal2.DefenderPosition.Value, normal2.Teta);
                DefenceTest.WeHaveDefenderCornerRole2 = true;
                if (n2.HasValue)
                    DefenceTest.DefenderCornerRole2 = Model.OurRobots[n2.Value].Location;
                if (!usenewmarker)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(DefenderMarkerRole2)))
                        Functions[regional.Value] = (eng, wmd) => GetRole<DefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.DefenderPosition.Value, reg.Teta);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, regional, typeof(NewDefenderMarkerRole2)))
                        Functions[regional.Value] = (eng, wmd) => GetRole<NewDefenderMarkerRole2>(regional.Value).Mark(eng, wmd, regional.Value, reg.OppID);
                }
                DefenceTest.WeHaveDefenderMarkerRole2 = true;
                if (regional.HasValue)
                    DefenceTest.DefenderMarkerRole2 = Model.OurRobots[regional.Value].Location;
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
                    Functions[golie.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(golie.Value).Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal1.DefenderPosition.Value, n1.Value, true);
                DefenceTest.WeHaveGoalie = true;
                if (golie.HasValue)
                    DefenceTest.GoalieRole = Model.OurRobots[golie.Value].Location;
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, mark2, freeRole))
                {
                    if (freeRole == typeof(RearStopRole))
                    {
                        Functions[mark2.Value] = (eng, wmd) => GetRole<RearStopRole>(mark2.Value).Run(eng, wmd, mark2.Value);
                        DefenceTest.WeHaveStopCover1 = true;
                        if (mark2.HasValue)
                            DefenceTest.StopCover1 = Model.OurRobots[mark2.Value].Location;
                    }
                    else if (freeRole == typeof(ActiveRole))
                        Functions[mark2.Value] = (eng, wmd) => GetRole<ActiveRole>(mark2.Value).Perform(eng, wmd, mark2.Value, null);
                    //else
                    //    Functions[mark2.Value] = (eng, wmd) => GetRole<DefendGotoPointRole>(mark2.Value).GotoPoint(wmd, mark2.Value, TargetLocation, Teta, true, true);
                }
                #region debug
                if (debug)
                {
                    if (n1.HasValue && normal1.OppID.HasValue)
                    {
                        Position2D secondp = normal1.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n1.Value].Location;
                        Position2D thirdp = Model.Opponents[normal1.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "3546465465");
                        DrawingObjects.AddObject(secondLine, "10321231654");
                        DrawingObjects.AddObject(new Circle(normal1.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "453774343");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n1.Value].Location, .11, new Pen(Brushes.White, .01f)), "534354373");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal1.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434634243");
                    }
                    if (n2.HasValue && normal2.OppID.HasValue)
                    {
                        Position2D secondp = normal2.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[n2.Value].Location;
                        Position2D thirdp = Model.Opponents[normal2.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "5664631321");
                        DrawingObjects.AddObject(secondLine, "12213213456");
                        DrawingObjects.AddObject(new Circle(normal2.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "434344636");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[n2.Value].Location, .11, new Pen(Brushes.White, .01f)), "24524363434");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[normal2.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45342463");
                    }
                    if (marker.HasValue && mark.OppID.HasValue && !usenewmarker)
                    {
                        Position2D secondp = mark.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[marker.Value].Location;
                        Position2D thirdp = Model.Opponents[mark.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "165432131");
                        DrawingObjects.AddObject(secondLine, "12213213");
                        DrawingObjects.AddObject(new Circle(mark.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "45324344053");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[marker.Value].Location, .11, new Pen(Brushes.White, .01f)), "2010603");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[mark.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "45205364");

                    }
                    else if (marker.HasValue && mark.OppID.HasValue && usenewmarker)
                    {
                        Position2D firstp = Model.OurRobots[marker.Value].Location;
                        Position2D thirdp = Model.Opponents[mark.OppID.Value].Location;
                        Line firstLine = new Line(firstp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "53773873");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[marker.Value].Location, .11, new Pen(Brushes.White, .01f)), "3697445396336");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[mark.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "434269796967");

                    }
                    if (regional.HasValue && reg.OppID.HasValue && !usenewmarker)
                    {
                        Position2D secondp = reg.DefenderPosition.Value;
                        Position2D firstp = Model.OurRobots[regional.Value].Location;
                        Position2D thirdp = Model.Opponents[reg.OppID.Value].Location;
                        Line firstLine = new Line(firstp, secondp, new Pen(Brushes.White, .01f));
                        Line secondLine = new Line(secondp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "555968795464");
                        DrawingObjects.AddObject(secondLine, "51324165464");
                        DrawingObjects.AddObject(new Circle(reg.DefenderPosition.Value, .11, new Pen(Brushes.Blue, .01f)), "205463653");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional.Value].Location, .11, new Pen(Brushes.White, .01f)), "634537634");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "453534");
                    }
                    else if (regional.HasValue && reg.OppID.HasValue && usenewmarker)
                    {
                        Position2D firstp = Model.OurRobots[regional.Value].Location;
                        Position2D thirdp = Model.Opponents[reg.OppID.Value].Location;
                        Line firstLine = new Line(firstp, thirdp, new Pen(Brushes.White, .01f));
                        DrawingObjects.AddObject(firstLine, "2134656454");
                        DrawingObjects.AddObject(new Circle(Model.OurRobots[regional.Value].Location, .11, new Pen(Brushes.White, .01f)), "9876546465");
                        DrawingObjects.AddObject(new Circle(Model.Opponents[reg.OppID.Value].Location, .11, new Pen(Brushes.Red, .01f)), "3213216564");
                    }

                }
                #endregion
            }
            #endregion
            // added io2018 vahid
            //if (Model.OurRobots.Count > 6)
            //{
            //    List<int> ids = new List<int>();
            //    if (Model.GoalieID.HasValue)
            //        ids = Model.OurRobots.Where(w => w.Key != Model.GoalieID.Value).Select(s => s.Key).ToList();
            //    else
            //        ids = Model.OurRobots.Select(s => s.Key).ToList();
            //    AddRoleInfo(roles, typeof(StaticPositionerRole), 1, 0);
            //    var assigenroles = _roleMatcher.MatchRoles(engine, Model, ids, roles, PreviouslyAssignedRoles);
            //    int? SPR = null;
            //    SPR = getID(assigenroles, typeof(StaticPositionerRole));
            //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, SPR, typeof(StaticPositionerRole)))
            //        Functions[SPR.Value] = (eng, wmd) => GetRole<StaticPositionerRole>(SPR.Value).perform(engine, Model, SPR.Value, new Position2D(0, 4 * Math.Sign(Model.BallState.Location.Y)));
            //}
            ControlParameters.BallIsMoved = ballismoved;
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            DefenceTest.BallTest = FreekickDefence.testDefenceState;
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
            ControlParameters.BallIsMoved = false;
            ballismoved = false;
            oppcathball = false;
            rearGotoPoint = false;
            PreviouslyAssignedRoles.Clear();
            FreekickDefence.RestartActiveFlags();
            FreekickDefence.StopToActive = false;
            RearStopRole.firstistrue = true;
            RearStopRole.initialangle = true;
            RearStopRole.realChip = false;
            RearStopRole.recieved = false;
            RearStopRole.goActive = false;

        }
    }
}
