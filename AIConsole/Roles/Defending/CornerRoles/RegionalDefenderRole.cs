using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.GameDefinitions.General_Settings;

namespace MRL.SSL.AIConsole.Roles
{
    class RegionalDefenderRole : RoleBase, IRegionalDefender
    {
        Position2D targ = new Position2D();
        private int Counter = 0;
        private int TreshTime = 20;
        private double RegionalGo = 2.8;
        private int? OppGoingToMark;
        private bool FifthRobot = false;
        private int counter1;

        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();

        public SingleWirelessCommand positionnig(GameStrategyEngine engine, WorldModel model, int robotid, Position2D targrt, double teta)
        {
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = model.BallState;
                ballStateFast = model.BallStateFast;
            }
            targ = targrt;
            //if (CurrentState == (int)States.Marker)
            //{
            //    SingleObjectState Opp = model.Opponents[OppGoingToMark.Value];
            //    double DistMarking = GameParameters.SafeRadi(Opp, .1);
            //    Vector2D ForMark = Opp.Location - GameParameters.OurGoalCenter;
            //    ForMark.NormalizeTo(DistMarking);
            //    Position2D MarkerTarget = GameParameters.OurGoalCenter + ForMark;
            //    targ = MarkerTarget;
            //    teta = ForMark.AngleInDegrees;

            //    List<int> RobotIDs = model.OurRobots.Keys.ToList();
            //    RobotIDs.Remove(robotid);
            //    foreach (var item in RobotIDs)
            //    {
            //        if (targ.DistanceFrom(model.OurRobots[item].Location) < .18)
            //        {
            //            Opp = model.Opponents[OppGoingToMark.Value];
            //            DistMarking = GameParameters.SafeRadi(Opp, .2);
            //            ForMark = Opp.Location - GameParameters.OurGoalCenter;
            //            ForMark.NormalizeTo(DistMarking);
            //            MarkerTarget = GameParameters.OurGoalCenter + ForMark;
            //            targ = MarkerTarget;
            //            teta = ForMark.AngleInDegrees;
            //        }
            //    }
            //}
            FreekickDefence.PreviousPositions[typeof(RegionalDefenderRole)] = targ;
            return GetSkill<GotoPointSkill>().GotoPoint(model, robotid, targ, teta, true, false, 3.5, true);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            var OppIMPIDs = Model.Opponents.Where(y => y.Value.Location.X > -1).Select(u => u.Key).ToList();
            List<int> OppAttIDs = engine.GameInfo.OppTeam.Scores.Where(w => OppIMPIDs.Contains(w.Key)).Where(w => w.Value > 0.6).Select(s => s.Key).ToList();

            List<int> OppMarked = new List<int>();
            int MarkerRobotID1, MarkerRobotID2, MarkerRobotID3, MarkerRobotID4, MarkerRobotID5, MarkerRobotID6, MarkerRobotID7;
            DefenceInfo MarkerRole1 = new DefenceInfo(), MarkerRole2 = new DefenceInfo(), MarkerRole3 = new DefenceInfo(), MarkerRole4 = new DefenceInfo(), MarkerRole5 = new DefenceInfo(), MarkerRole6 = new DefenceInfo(), MarkerRole7 = new DefenceInfo();
            bool LagRobot1 = false, LagRobot2 = false, LagRobot3 = false, LagRobot4 = false, LagRobot5 = false, LagRobot6 = false, LagRobot7 = false;
            if (FreekickDefence.CurrentInfos.Any(g => g.RoleType == typeof(DefenderMarkerRole)) && AssignedRoles.Any(i => i.Value.GetType() == typeof(DefenderMarkerRole)))
            {
                MarkerRole1 = FreekickDefence.CurrentInfos.Where(o => o.RoleType == typeof(DefenderMarkerRole)).FirstOrDefault();
                MarkerRobotID1 = AssignedRoles.Where(i => i.Value.GetType() == typeof(DefenderMarkerRole)).Select(t => t.Key).FirstOrDefault();
                LagRobot1 = Lag(Model, MarkerRole1, MarkerRobotID1);
                if (MarkerRole1.OppID.HasValue)
                    OppAttIDs.Remove(MarkerRole1.OppID.Value);//Fifth Robot
            }
            if (FreekickDefence.CurrentInfos.Any(g => g.RoleType == typeof(DefenderMarkerRole2)) && AssignedRoles.Any(i => i.Value.GetType() == typeof(DefenderMarkerRole2)))
            {
                MarkerRole2 = FreekickDefence.CurrentInfos.Where(o => o.RoleType == typeof(DefenderMarkerRole2)).FirstOrDefault();
                MarkerRobotID2 = AssignedRoles.Where(i => i.Value.GetType() == typeof(DefenderMarkerRole2)).Select(t => t.Key).FirstOrDefault();
                LagRobot2 = Lag(Model, MarkerRole2, MarkerRobotID2);
                if (MarkerRole2.OppID.HasValue)
                    OppAttIDs.Remove(MarkerRole2.OppID.Value);//Fifth Robot
            }
            if (FreekickDefence.CurrentInfos.Any(g => g.RoleType == typeof(DefenderMarkerRole3)) && AssignedRoles.Any(i => i.Value.GetType() == typeof(DefenderMarkerRole3)))
            {
                MarkerRole3 = FreekickDefence.CurrentInfos.Where(o => o.RoleType == typeof(DefenderMarkerRole3)).FirstOrDefault();
                MarkerRobotID3 = AssignedRoles.Where(i => i.Value.GetType() == typeof(DefenderMarkerRole3)).Select(t => t.Key).FirstOrDefault();
                LagRobot3 = Lag(Model, MarkerRole3, MarkerRobotID3);
                if (MarkerRole3.OppID.HasValue)
                    OppAttIDs.Remove(MarkerRole3.OppID.Value);//Fifth Robot
            }
            if (FreekickDefence.CurrentInfos.Any(g => g.RoleType == typeof(DefenderCornerRole1)) && AssignedRoles.Any(i => i.Value.GetType() == typeof(DefenderCornerRole1)))
            {
                MarkerRole4 = FreekickDefence.CurrentInfos.Where(o => o.RoleType == typeof(DefenderCornerRole1)).FirstOrDefault();
                MarkerRobotID4 = AssignedRoles.Where(i => i.Value.GetType() == typeof(DefenderCornerRole1)).Select(t => t.Key).FirstOrDefault();
                LagRobot4 = Lag(Model, MarkerRole4, MarkerRobotID4);
                if (MarkerRole4.OppID.HasValue)
                    OppAttIDs.Remove(MarkerRole4.OppID.Value);//Fifth Robot
            }
            if (FreekickDefence.CurrentInfos.Any(g => g.RoleType == typeof(DefenderCornerRole2)) && AssignedRoles.Any(i => i.Value.GetType() == typeof(DefenderCornerRole2)))
            {
                MarkerRole5 = FreekickDefence.CurrentInfos.Where(o => o.RoleType == typeof(DefenderCornerRole2)).FirstOrDefault();
                MarkerRobotID5 = AssignedRoles.Where(i => i.Value.GetType() == typeof(DefenderCornerRole2)).Select(t => t.Key).FirstOrDefault();
                LagRobot5 = Lag(Model, MarkerRole5, MarkerRobotID5);
                if (MarkerRole5.OppID.HasValue)
                    OppAttIDs.Remove(MarkerRole5.OppID.Value);//Fifth Robot
            }
            if (FreekickDefence.CurrentInfos.Any(g => g.RoleType == typeof(DefenderCornerRole3)) && AssignedRoles.Any(i => i.Value.GetType() == typeof(DefenderCornerRole3)))
            {
                MarkerRole6 = FreekickDefence.CurrentInfos.Where(o => o.RoleType == typeof(DefenderCornerRole3)).FirstOrDefault();
                MarkerRobotID6 = AssignedRoles.Where(i => i.Value.GetType() == typeof(DefenderCornerRole3)).Select(t => t.Key).FirstOrDefault();
                LagRobot6 = Lag(Model, MarkerRole6, MarkerRobotID6);
                if (MarkerRole6.OppID.HasValue)
                    OppAttIDs.Remove(MarkerRole6.OppID.Value);//Fifth Robot
            }
            #region Fifth Robot

            OppGoingToMark = null;
            if (OppAttIDs.Count > 0 && Model.Opponents[Model.Opponents.OrderBy(w => w.Value.Location.DistanceFrom(Model.OurRobots[RobotID].Location)).FirstOrDefault().Key].Location.DistanceFrom(Model.OurRobots[RobotID].Location) < RegionalGo)
            {
                if (OppAttIDs.Count == 1)
                {
                    if (Model.Opponents[OppAttIDs.FirstOrDefault()].Location.DistanceFrom(Model.OurRobots[RobotID].Location) < RegionalGo)
                    {
                        OppGoingToMark = OppAttIDs.FirstOrDefault();
                        CurrentState = (int)States.Marker;
                        FifthRobot = true;
                    }
                }
                else if (OppAttIDs.Count > 1)
                {
                    int OppRobotID = Model.Opponents.OrderBy(w => w.Value.Location.DistanceFrom(Model.OurRobots[RobotID].Location)).FirstOrDefault().Key;
                    OppGoingToMark = OppRobotID;
                    CurrentState = (int)States.Marker;
                    FifthRobot = true;
                }
            }
            #endregion
            else
            {
                FifthRobot = false;
                if ((LagRobot1 || LagRobot2 || LagRobot3 || LagRobot4 || LagRobot5 || LagRobot5 || LagRobot6 || LagRobot7))
                {
                    Counter++;
                    if (LagRobot1)
                        OppGoingToMark = MarkerRole1.OppID;
                    if (LagRobot2)
                        OppGoingToMark = MarkerRole2.OppID;
                    if (LagRobot3)
                        OppGoingToMark = MarkerRole3.OppID;
                    if (LagRobot4)
                        OppGoingToMark = MarkerRole4.OppID;
                    if (LagRobot5)
                        OppGoingToMark = MarkerRole5.OppID;
                    if (LagRobot6)
                        OppGoingToMark = MarkerRole6.OppID;
                    if (LagRobot7)
                        OppGoingToMark = MarkerRole7.OppID;
                    FifthRobot = false;
                }
                else
                    Counter = 0;
                if (Counter == 0 && CurrentState == (int)States.Marker)
                {
                    counter1++;
                }
                if (Counter > TreshTime && CurrentState == (int)States.Regional)
                {

                    CurrentState = (int)States.Marker;
                }
                if (Model.Opponents.ContainsKey(OppAttIDs.FirstOrDefault()))
                {
                    if (CurrentState == (int)States.Marker && (Counter == 0 && counter1 > 30 || Model.Opponents[OppAttIDs.FirstOrDefault()].Location.DistanceFrom(Model.OurRobots[RobotID].Location) > RegionalGo) && !FifthRobot)
                    {
                        CurrentState = (int)States.Regional;
                    }
                }
            }
            if (FifthRobot && OppGoingToMark.HasValue)
            {
                if (!(Model.Opponents[OppGoingToMark.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) < 2.40 && (GameParameters.OurGoalCenter - Model.Opponents[OppGoingToMark.Value].Location).InnerProduct(Model.Opponents[OppGoingToMark.Value].Speed) > .1))
                {
                    CurrentState = (int)States.Regional;
                }
                else if ((Model.Opponents[OppGoingToMark.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) < 2.20 && Math.Abs((Model.Opponents[OppGoingToMark.Value].Location - GameParameters.OurGoalCenter).AngleInDegrees) < Math.Abs((Model.OurRobots[RobotID].Location - GameParameters.OurGoalCenter).AngleInDegrees)) || Model.Opponents[OppGoingToMark.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.5)
                {
                    CurrentState = (int)States.Marker;
                }
            }
            if (!OppGoingToMark.HasValue)
            {
                CurrentState = (int)States.Regional;
            }
            if (CurrentState == (int)States.Marker)
                DrawingObjects.AddObject(new StringDraw("Marker", new Position2D(1, 1)));
            else
                DrawingObjects.AddObject(new StringDraw("Regional", new Position2D(1, 1)));
            DrawingObjects.AddObject(new StringDraw(Counter.ToString(), new Position2D(1.2, 1)));
            FreekickDefence.CurrentStates[this] = CurrentState;

        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (FreekickDefence.CurrentInfos.Any(f => f.RoleType == this.GetType()))
            {
                double d = FreekickDefence.CurrentInfos.First(f => f.RoleType == this.GetType()).DefenderPosition.Value.DistanceFrom(Model.OurRobots[RobotID].Location);
                ;
                return d * d;
            }
            return 100;
        }
        bool Lag(WorldModel Model, DefenceInfo MarkerInfo, int MarkerID)
        {
            double MArkFromDist = MarkerDefenceUtils.MaxMarkDist;// TuneVariables.Default.GetValue<double>("MarkFromDist");
            if (Model.OurRobots[MarkerID].Location.DistanceFrom(MarkerInfo.DefenderPosition.Value) > .6)
            {
                //return true;
            }
            return false;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (!FreekickDefence.switchAllMode)
            {
                if (FreekickDefence.WeAreInCorner && FreekickDefence.BallIsMoved)
                {
                    List<RoleBase> res = new List<RoleBase>();
                    res.Add(new RegionalDefenderRole());
                    res.Add(new NewDefenderMarkerRole3());
                    if (FreekickDefence.DefenderRegionalRole1ToActive)
                    {
                        res.Add(new ActiveRole());
                    }
                    return res;
                }
                else
                {
                    List<RoleBase> res = new List<RoleBase>();
                    res.Add(new RegionalDefenderRole());
                    if (!FreekickDefence.RearRegional)
                    {
                        res.Add(new DefenderCornerRole1());
                        res.Add(new DefenderCornerRole2());
                        res.Add(new DefenderCornerRole3());
                        res.Add(new DefenderCornerRole4());

                        
                    }
                    if (FreekickDefence.freeSwitchbetweenRegionalAndMarker || FreekickDefence.RearRegional)
                    {
                        res.Add(new NewDefenderMarkerRole2());
                        res.Add(new NewDefenderMarkerRole3());
                        res.Add(new NewDefenderMrkerRole());
                    
                    }
                    if (FreekickDefence.DefenderRegionalRole1ToActive)
                    {
                        res.Add(new ActiveRole());
                    }
                    return res;
                }
            }
            else
            {
                List<RoleBase> res = new List<RoleBase>();
                res.Add(new DefenderCornerRole1());
                res.Add(new DefenderCornerRole2());
                res.Add(new DefenderCornerRole3());
                res.Add(new DefenderMarkerRole2());
                res.Add(new DefenderCornerRole4());
                res.Add(new DefenderMarkerRole());
                res.Add(new NewDefenderMrkerRole());
                res.Add(new NewDefenderMarkerRole2());
                res.Add(new RegionalDefenderRole());
                res.Add(new RegionalDefenderRole2());
                if (FreekickDefence.DefenderRegionalRole1ToActive)
                {
                    res.Add(new ActiveRole());
                }
                return res;
            }
            
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
        public enum States
        {
            Regional,
            Marker
        }
    }
}
