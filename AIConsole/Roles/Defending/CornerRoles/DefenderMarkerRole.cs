using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public class DefenderMarkerRole : RoleBase, IMarkerDefender
    {
        Position2D lastopploc;
        Position2D targ = new Position2D();
        bool first = true, calculateCost = false;
        bool wasAcc = false;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        //public static bool CoverOppRobot = false;
        //public static bool GoToNormalPlay = false;
        public void Mark(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D target, double teta)
        {
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
            double Teta;
            DefenceInfo inf = new DefenceInfo();
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.First(f => f.RoleType == this.GetType());
            //DrawingObjects.AddObject(new StringDraw("OppID " + FreekickDefence.LastOppToMark1.Value.ToString(), new Position2D(-1, 0)));
            DrawingObjects.AddObject(new StringDraw(((DefenderStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location + new Vector2D(0.5, 0.5)), "marker");
            Position2D pos = CalculateTarget(engine, Model, RobotID, inf, target, teta, out Teta, FreekickDefence.LastOppToMark1);
         

            //if (CurrentState == (int)MarkerState.CoverOurGoal)
            //{
            //    CoverOppRobot = false;
            //}
            //if (CurrentState == (int)MarkerState.CoverOppRobot)
            //{
            //    CoverOppRobot = true;
            //}
            //if (CoverOppRobot && ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .5)
            //{
            //    GoToNormalPlay = true;
            //}
            //if (!CoverOppRobot)
            //{
            //    GoToNormalPlay = false;
            //}

            //if (CoverOppRobot)
            //{
            //    Teta = (targetforkick.Extend(2, 0) - Model.OurRobots[RobotID].Location).AngleInDegrees; // (Target - postosee).AngleInDegrees;
            //    //Planner.AddKick(RobotID, kickPowerType.Speed, true, (targetforkick - Model.OurRobots[RobotID].Location).Size / 2);
            //}
            if (FreekickDefence.weAreInKickoff)
            {
                pos = GameParameters.OurGoalCenter + (pos - GameParameters.OurGoalCenter).GetNormalizeToCopy(Math.Min(3.5, pos.DistanceFrom(GameParameters.OurGoalCenter)));
            }
            pos = CommonDefenceUtils.CheckForStopZone(FreekickDefence.BallIsMoved, pos, Model);
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 8, 5);
            Teta = (pos - GameParameters.OurGoalCenter).AngleInDegrees;

            FreekickDefence.PreviousPositions[typeof(DefenderMarkerRole)] = pos;
            Planner.Add(RobotID, pos, Teta, PathType.UnSafe, false, true, true, false);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
            {
                DefenceInfo inf = FreekickDefence.CurrentInfos.First(f => f.RoleType == this.GetType());
                if (first)
                {
                    ////     if (!FreekickDefence.LastOppToMark1.HasValue || CurrentState == (int)MarkerState.CoverOurGoal)
                    FreekickDefence.LastOppToMark1 = inf.OppID;
                    wasAcc = false;
                    first = false;
                }
                if (CurrentState == (int)MarkerState.CoverOurGoal)
                {
                    if (OppFreeKickMarkerUtils.PassedToOpponent(Model, RobotID, FreekickDefence.LastOppToMark1, inf.DefenderPosition, ref wasAcc))
                    {
                        // CoverOppRobot = true;
                        CurrentState = (int)MarkerState.CoverOppRobot;
                        lastopploc = Model.Opponents[FreekickDefence.LastOppToMark1.Value].Location;
                    }
                }
                else if (CurrentState == (int)MarkerState.CoverOppRobot)
                {
                    if (!OppFreeKickMarkerUtils.PassedToOpponent(Model, RobotID, FreekickDefence.LastOppToMark1, inf.DefenderPosition, ref wasAcc) || (FreekickDefence.LastOppToMark1.HasValue && lastopploc.DistanceFrom(Model.Opponents[FreekickDefence.LastOppToMark1.Value].Location) > 0.3))
                    {
                        //  CoverOppRobot = false;
                        CurrentState = (int)MarkerState.CoverOurGoal;
                    }
                }
                //if (lastopploc != Model.Opponents[inf.OppID.Value].Location)
                //{
                //    CoverOppRobot = false;
                //    CurrentState = (int)MarkerState.CoverOurGoal;
                //}
                if (!calculateCost)
                {
                    if (!FreekickDefence.LastOppToMark1.HasValue || CurrentState == (int)MarkerState.CoverOurGoal)
                    {
                        wasAcc = false;
                        FreekickDefence.LastOppToMark1 = inf.OppID;
                    }
                    FreekickDefence.CurrentStates[this] = CurrentState;
                    if (CurrentState == (int)MarkerState.CoverOppRobot && (ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .15 && Model.OurRobots[RobotID].Location.DistanceFrom(ballState.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location)) < .07))
                    {
                        FreekickDefence.SwitchToActiveMarker1 = true;
                    }
                    else
                    {
                        FreekickDefence.SwitchToActiveMarker1 = false;
                    }
                }

            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            DefenceInfo inf = new DefenceInfo();
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
            {
                first = false;
                //calculateCost = true;
                //   DetermineNextState(engine, Model, RobotID, new Dictionary<int, RoleBase>());
                CurrentState = (FreekickDefence.CurrentStates.ContainsKey(this)) ? FreekickDefence.CurrentStates[this] : CurrentState;
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
                double Teta;
                double cost = CalculateTarget(engine, Model, RobotID, inf, inf.DefenderPosition.Value, inf.Teta, out Teta, FreekickDefence.LastOppToMark1).DistanceFrom(Model.OurRobots[RobotID].Location);
                return cost * cost;
            }
            return 100;
        }
        Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenceInfo inf, Position2D Target, double teta, out double Teta, int? oppid)
        {

            Position2D pos = new Position2D();
            SingleObjectState oppState = (oppid.HasValue && Model.Opponents.ContainsKey(oppid.Value)) ? Model.Opponents[oppid.Value] : ballState;

            if (CurrentState == (int)MarkerState.CoverOurGoal)
            {
                Teta = teta;
                pos = Target;
            }
            else
            {

                Position2D postosee = new Position2D(GameParameters.OppGoalCenter.X, -1 * Math.Sign(ballState.Location.Y) * 2);

                Teta = (Target - postosee).AngleInDegrees;
                Circle OpprobotCenter = new Circle(Model.Opponents[oppid.Value].Location, .22);
                Vector2D BallVectorSpeed = ballState.Speed.GetNormalizeToCopy(10);
                Line BallMotionLine = new Line(ballState.Location, ballState.Location + BallVectorSpeed);
                List<Position2D> Intersections = OpprobotCenter.Intersect(BallMotionLine);
                Position2D first = Intersections.OrderBy(o => o.DistanceFrom(ballState.Location)).FirstOrDefault();
                pos = first.Extend(.05, 0);
                if (first == Position2D.Zero)
                {
                    pos = oppState.Location + (ballState.Location - oppState.Location).GetNormalizeToCopy(OppFreeKickMarkerUtils.DistCutPassFromOpp);
                }
                Position2D targetforkick = (ballState.Location.Y > 0) ? GameParameters.OppLeftCorner : GameParameters.OppRightCorner;
                Teta = (targetforkick.Extend(2, 0) - Model.OurRobots[RobotID].Location).AngleInDegrees; // (Target - postosee).AngleInDegrees;
            }
            Position2D stoppos = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, CommonDefenceUtils.StopZone);
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, null, Model.Opponents.Keys.ToList());
            if (pos.DistanceFrom(stoppos) < RobotParameters.OurRobotParams.Diameter + 0.02 && obs.Meet(new SingleObjectState(stoppos, Vector2D.Zero, 0), 0.1))
            {
                Vector2D vec = pos - stoppos;
                pos = stoppos + vec.GetNormalizeToCopy(RobotParameters.OurRobotParams.Diameter + 0.02);
            }

            double Mindist = GameParameters.SafeRadi(new SingleObjectState(pos, Vector2D.Zero, 0), OppFreeKickMarkerUtils.MinDistBehindFromZone);
            bool meet = false;
            double d = pos.DistanceFrom(GameParameters.OurGoalCenter);

            if (!isInZone && d < Mindist)
            {
                isInZone = true;
            }
            else if (isInZone && d > Mindist + 0.1)
            {
                isInZone = false;
            }
            if (isInZone)
            {
                Obstacles obstacles = new Obstacles(Model);

                List<int> exclude = new List<int> { RobotID, Model.GoalieID ?? 100 };
                obstacles.AddObstacle(1, 0, 0, 0, exclude, ((inf.OppID.HasValue) ? new List<int>() { inf.OppID.Value } : null));

                meet = obstacles.Meet(Model.OurRobots[RobotID], new SingleObjectState(GameParameters.OurGoalCenter, Vector2D.Zero, null), 0.07);
                if (meet)
                {
                    //kpos = (pos - GameParameters.OurGoalCenter).GetNormalizeToCopy(.1) + pos;
                }
            }
            pos = CommonDefenceUtils.CheckForStopZone(FreekickDefence.BallIsMoved, pos, Model);
            return pos;
        }
        bool isInZone = false;

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }


        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new DefenderMarkerRole(),
                                                        new DefenderMarkerRole2(),
                                                        new DefendGotoPointRole(),
                                                        new DefenderCornerRole2()
                                                         };
            if (FreekickDefence.SwitchToActiveMarker1)
            {
                res.Add(new ActiveRole());
            }
            if (FreekickDefence.SwitchDefender2Marker1)
            {
                res.Add(new DefenderNormalRole1());
                res.Add(new DefenderCornerRole1());
            }
            if (FreekickDefence.SwitchDefender32Marker1)
            {
                res.Add(new DefenderCornerRole3());
            }

            if (FreekickDefence.SwitchDefender42Marker1)
            {
                res.Add(new DefenderCornerRole4());
            }
            if (FreekickDefence.LastSwitchDefender2Marker1)//New IO2014
            {
                res.Add(new DefenderNormalRole1());
                res.Add(new DefenderCornerRole1());
            }
            if (FreekickDefence.LastSwitchDefender32Marker1)//New IO2014
            {
                res.Add(new DefenderCornerRole3());
            }
            if (FreekickDefence.LastSwitchDefender42Marker1)//New IO2014
            {
                res.Add(new DefenderCornerRole4());
            }
            return res;
        }

    }
}
