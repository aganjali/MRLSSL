using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    public class DefenderNormalRole1 : RoleBase, IFirstDefender
    {
        public Position2D Target = new Position2D();
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public SingleWirelessCommand Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta)
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
            //if (Model.Status == GameStatus.Stop)
            //{
            //    Planner.Add(RobotID, GameParameters.OurGoalCenter + (TargetPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(TargetPos, Vector2D.Zero, 0), 0.1)), 180);
            //    return new SingleWirelessCommand();
            //}
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            double distfromborder, dist;
            double teta = 180;
            if (Model.Status == GameStatus.Stop && GameParameters.IsInDangerousZone(ballState.Location, false, .5, out dist, out distfromborder))
            {
                Line Stop1 = new Line(GameParameters.OurGoalCenter.Extend(0, -.3), GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3)));
                Circle Circl = new Circle(ballState.Location, .5);
                Target = Circl.Intersect(Stop1).OrderBy(t => t.DistanceFrom(GameParameters.OurGoalCenter)).First();
            }
            else
            {
                if (CurrentState == (int)DefenderStates.InPenaltyArea)
                {
                    Target = MarkFront(engine, Model, RobotID, inf, 0.05, out teta);
                }
                else if (CurrentState == (int)DefenderStates.Behind)
                {
                    Target = BehindSatate(engine, Model, inf, RobotID, out teta);
                }
                else if (CurrentState == (int)DefenderStates.Normal)
                {
                    Target = TargetPos;
                    Vector2D vec = Target - Model.OurRobots[RobotID].Location;
                    Target = Target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));

                    teta = Teta;
                }
                else if (CurrentState == (int)DefenderStates.KickToGoal)
                {
                    Target = Dive(engine, Model, RobotID);
                    teta = Model.OurRobots[RobotID].Angle.Value;
                }
                else if (CurrentState == (int)DefenderStates.BallInFront)
                {
                    Target = GetBackBallPoint(Model, RobotID, out teta);
                }
                else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
                {
                    if (inf != null)
                    {
                        int? oppid = inf.OppID;
                        if (oppid.HasValue)
                        {
                            Vector2D vec = (Model.Opponents[oppid.Value].Location - Model.OurRobots[RobotID].Location);
                            Target = Model.Opponents[oppid.Value].Location + vec.GetNormalizeToCopy(0.2);
                            teta = (Target - Model.OurRobots[RobotID].Location).AngleInDegrees;
                        }
                    }
                }
                if ((CurrentState == (int)DefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X) || (CurrentState == (int)DefenderStates.Behind && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
                {
                    Target = new Position2D(2.9, Target.Y);
                }
            }
            FreekickDefence.PreviousPositions[typeof(DefenderMarkerNormalRole1)] = Target;
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            if (Model.Status == GameStatus.Stop)
                SWC = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, teta, false, false, 3.5, false);
            else
                SWC = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, teta, false, false, 3.5, true);

            ////////////Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
            SWC.isChipKick = true;
            SWC.RobotID = RobotID;
            SWC.KickSpeed = 2;
            return SWC;
        }
        public SingleWirelessCommand RunStop(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta)
        {
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
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
            //if (Model.Status == GameStatus.Stop)
            //{
            //    Planner.Add(RobotID, GameParameters.OurGoalCenter + (TargetPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(TargetPos, Vector2D.Zero, 0), 0.1)), 180);
            //    return new SingleWirelessCommand();
            //}
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            double distfromborder, dist;
            double teta = 180;
            if (Model.Status == GameStatus.Stop && GameParameters.IsInDangerousZone(ballState.Location, false, .5, out dist, out distfromborder))
            {
                Line Stop1 = new Line(GameParameters.OurGoalCenter.Extend(0, -.3), GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3)));
                Circle Circl = new Circle(ballState.Location, .5);
                Target = Circl.Intersect(Stop1).OrderBy(t => t.DistanceFrom(GameParameters.OurGoalCenter)).First();
            }
            else
            {
                if (CurrentState == (int)DefenderStates.InPenaltyArea)
                {
                    Target = MarkFront(engine, Model, RobotID, inf, 0.05, out teta);
                }
                else if (CurrentState == (int)DefenderStates.Behind)
                {
                    Target = BehindSatate(engine, Model, inf, RobotID, out teta);
                }
                else if (CurrentState == (int)DefenderStates.Normal)
                {
                    Target = TargetPos;
                    Vector2D vec = Target - Model.OurRobots[RobotID].Location;
                    Target = Target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));

                    teta = Teta;
                }
                else if (CurrentState == (int)DefenderStates.KickToGoal)
                {
                    Target = Dive(engine, Model, RobotID);
                    teta = Model.OurRobots[RobotID].Angle.Value;
                }
                else if (CurrentState == (int)DefenderStates.BallInFront)
                {
                    Target = GetBackBallPoint(Model, RobotID, out teta);
                }
                else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
                {
                    if (inf != null)
                    {
                        int? oppid = inf.OppID;
                        if (oppid.HasValue)
                        {
                            Vector2D vec = (Model.Opponents[oppid.Value].Location - Model.OurRobots[RobotID].Location);
                            Target = Model.Opponents[oppid.Value].Location + vec.GetNormalizeToCopy(0.2);
                            teta = (Target - Model.OurRobots[RobotID].Location).AngleInDegrees;
                        }
                    }
                }
                if ((CurrentState == (int)DefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X) || (CurrentState == (int)DefenderStates.Behind && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
                {
                    Target = new Position2D(2.9, Target.Y);
                }
            }
            FreekickDefence.PreviousPositions[typeof(DefenderMarkerNormalRole1)] = Target;
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            if (Model.Status == GameStatus.Stop)
                SWC = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, teta, false, false, 1, false);
            else
                SWC = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, teta, false, false, 2, true);

            ////////////Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
            SWC.isChipKick = true;
            SWC.RobotID = RobotID;
            SWC.KickSpeed = 0;
            return SWC;
        }

        public Position2D MarkFront(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenceInfo info, double margin, out double Teta)
        {
            SingleObjectState Target = (info != null && info.OppID.HasValue) ? Model.Opponents[info.OppID.Value] : ballState;
            Teta = (Target.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            double min = GameParameters.SafeRadi(Target, margin);
            Position2D Pos = GameParameters.OurGoalCenter - (GameParameters.OurGoalCenter - Target.Location).GetNormalizeToCopy(min);
            Pos.DrawColor = System.Drawing.Color.Red;
            DrawingObjects.AddObject(Pos);
            return Pos;
        }
        public Position2D Dive(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D pos = new Position2D();
            Position2D robotLoc = Model.OurRobots[RobotID].Location;
            Position2D ballLoc = ballState.Location;
            Vector2D ballSpeed = ballState.Speed;
            Position2D prep = ballSpeed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location);
            double dist, DistFromBorder, R;
            if (GameParameters.IsInDangerousZone(prep, false, 0.15, out dist, out DistFromBorder))
            {
                R = GameParameters.SafeRadi(new SingleObjectState(prep, new Vector2D(), 0), 0.05);
                pos = GameParameters.OurGoalCenter - ballSpeed.GetNormalizeToCopy(R);
            }
            else
                pos = prep;
            return pos;
        }
        public Position2D BehindSatate(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta)
        {
            Position2D target;
            if (inf != null && inf.OppID.HasValue)
            {
                if (FreekickDefence.CurrentStates.Any(a => a.Key.GetType() == typeof(GoalieNormalRole)) && (DefenderStates)FreekickDefence.CurrentStates.Where(w => w.Key.GetType() == typeof(GoalieNormalRole)).First().Value == DefenderStates.InPenaltyArea)
                    target = InPenaltyAreaState(engine, Model, inf, RobotID, out Teta);
                else if (Model.Opponents[inf.OppID.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) > 2)
                    target = GetBackBallPoint(Model, RobotID, out Teta);
                else
                    target = MarkFront(engine, Model, RobotID, inf, 0.1, out Teta);
            }
            else
                target = GetBackBallPoint(Model, RobotID, out Teta);

            return target;
        }
        public Position2D InPenaltyAreaState(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta)
        {
            Vector2D vec;
            if (ballState.Location.Y >= 0)
                vec = new Vector2D(0, -0.2);
            else
                vec = new Vector2D(0, 0.2);
            double R = GameParameters.SafeRadi(ballState, 0.1);
            Position2D target = GameParameters.OurGoalCenter + (ballState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(R);
            target = target + vec;
            Teta = (ballState.Location - target).AngleInDegrees;
            return target;
        }
        public Position2D GetBackBallPoint(WorldModel Model, int RobotID, out double Teta)
        {
            Vector2D vec = ballState.Location - GameParameters.OurGoalCenter;
            Position2D tar = ballState.Location + vec;
            Vector2D ballSpeed = ballState.Speed;
            Position2D ballLocation = ballState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = tar - ballLocation;
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(0.09);
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;
            double segmentConst = 0.7;
            double rearDistance = 0.15;
            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            Position2D finalPosToGo = midPoint;
            double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

            if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
                finalPosToGo = backBallPoint;
            else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }

            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);
            Teta = (vec).AngleInDegrees;
            return finalPosToGo;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            double dist, dist2;
            double tresh = 0.05;
            double margin;
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            int? ballOwner = engine.GameInfo.OurTeam.BallOwner;
            if (CurrentState == (int)DefenderStates.Normal)
            {
                margin = 0.1;
                if (BallKickedToGoal(Model))
                {
                    CurrentState = (int)DefenderStates.KickToGoal;
                }
                else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
                {
                    CurrentState = (int)DefenderStates.InPenaltyArea;
                }
                else if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                {
                    CurrentState = (int)DefenderStates.Behind;
                }
                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && FreekickDefence.BallIsMovedStrategy)
                {
                    CurrentState = (int)DefenderStates.BallInFront;
                }
                else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                {
                    CurrentState = (int)DefenderStates.OppIndDangerZone;
                }
            }
            else if (CurrentState == (int)DefenderStates.Behind)
            {
                if (engine.Status == GameStatus.Stop)
                {
                    CurrentState = (int)DefenderStates.Normal;
                }
                margin = 0.1;
                if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
                {
                    CurrentState = (int)DefenderStates.InPenaltyArea;
                }
                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && FreekickDefence.BallIsMovedStrategy)
                {
                    CurrentState = (int)DefenderStates.BallInFront;
                }
                else if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) >= tresh + 0.08)
                {
                    CurrentState = (int)DefenderStates.Normal;
                }
            }
            else if (CurrentState == (int)DefenderStates.InPenaltyArea)
            {
                margin = 0.3;
                if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
                {
                    if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) >= tresh + 0.08)
                    {
                        CurrentState = (int)DefenderStates.Normal;
                    }
                    else
                        CurrentState = (int)DefenderStates.Behind;
                }

            }
            else if (CurrentState == (int)DefenderStates.KickToGoal)
            {
                margin = 0.1;
                if (!BallKickedToGoal(Model))
                {
                    if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
                    {
                        CurrentState = (int)DefenderStates.InPenaltyArea;
                    }
                    else if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                    {
                        CurrentState = (int)DefenderStates.Behind;
                    }
                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && FreekickDefence.BallIsMovedStrategy)
                    {
                        CurrentState = (int)DefenderStates.BallInFront;
                    }
                    else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                    {
                        CurrentState = (int)DefenderStates.OppIndDangerZone;
                    }
                    else
                        CurrentState = (int)DefenderStates.Normal;
                }
            }
            else if (CurrentState == (int)DefenderStates.BallInFront)
            {
                margin = 0.1;
                if (engine.Status == GameStatus.Stop || !FreekickDefence.BallIsMovedStrategy)
                {
                    CurrentState = (int)DefenderStates.Normal;
                }
                else
                {
                    if (!ballOwner.HasValue || (ballOwner.HasValue && ballOwner.Value != RobotID))
                    {
                        if (BallKickedToGoal(Model))
                        {
                            CurrentState = (int)DefenderStates.KickToGoal;
                        }
                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
                        {
                            CurrentState = (int)DefenderStates.InPenaltyArea;
                        }
                        else if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                        {
                            CurrentState = (int)DefenderStates.Behind;
                        }
                        else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                        {
                            CurrentState = (int)DefenderStates.OppIndDangerZone;
                        }
                        else
                            CurrentState = (int)DefenderStates.Normal;
                    }
                }
            }
            else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
            {
                margin = 0.1;
                if (inf == null || (inf != null && inf.TargetState.Type != ObjectType.Opponent) || (inf != null && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) > tresh))
                {
                    if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
                    {
                        CurrentState = (int)DefenderStates.InPenaltyArea;
                    }
                    else if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                    {
                        CurrentState = (int)DefenderStates.Behind;
                    }
                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop)
                    {
                        CurrentState = (int)DefenderStates.BallInFront;
                    }
                    else
                        CurrentState = (int)DefenderStates.Normal;
                }
                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && FreekickDefence.BallIsMovedStrategy)
                {
                    CurrentState = (int)DefenderStates.BallInFront;
                }
            }

            DrawingObjects.AddObject(new StringDraw(((DefenderStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location + new Vector2D(0.2, 0.2)));
            FreekickDefence.CurrentStates[this] = CurrentState;
        }

        private bool BallKickedToGoal(WorldModel Model)
        {
            Line line = new Line();
            line = new Line(ballState.Location, ballState.Location - ballState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.15 && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.15)
                if (ballState.Speed.InnerProduct(GameParameters.OurGoalRight - ballState.Location) > 0)
                    if (ballState.Speed.Size > 0.1 && d / ballState.Speed.Size < 2.2)
                        return true;
            return false;
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            if (inf != null)
            {
                Position2D pos = Cost(engine, Model, RobotID, inf.DefenderPosition.Value, inf.Teta);
                double cost = pos.DistanceFrom(Model.OurRobots[RobotID].Location);
                return cost * cost;
            }
            return 100;
        }
        public Position2D Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double p)
        {
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();

            double teta = 180;
            if (CurrentState == (int)DefenderStates.InPenaltyArea)
            {
                Target = MarkFront(engine, Model, RobotID, inf, 0.1, out teta);//InPenaltyAreaState(engine, Model,inf, RobotID, out Teta);
            }
            else if (CurrentState == (int)DefenderStates.Behind)
            {
                Target = BehindSatate(engine, Model, inf, RobotID, out teta);
            }
            else if (CurrentState == (int)DefenderStates.Normal)
            {
                Target = TargetPos;
                Vector2D vec = Target - Model.OurRobots[RobotID].Location;
                Target = Target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));
            }
            else if (CurrentState == (int)DefenderStates.KickToGoal)
            {
                Target = Dive(engine, Model, RobotID);
                teta = Model.OurRobots[RobotID].Angle.Value;
            }
            else if (CurrentState == (int)DefenderStates.BallInFront)
            {
                Target = GetBackBallPoint(Model, RobotID, out teta);
            }
            else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
            {
                if (inf != null)
                {
                    int? oppid = inf.OppID;
                    if (oppid.HasValue)
                    {
                        Vector2D vec = (Model.Opponents[oppid.Value].Location - Model.OurRobots[RobotID].Location);
                        Target = Model.Opponents[oppid.Value].Location + vec.GetNormalizeToCopy(0.2);
                        teta = (Target - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    }
                }
            }
            if ((CurrentState == (int)DefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X) || (CurrentState == (int)DefenderStates.Behind && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
            {
                Target = new Position2D(2.9, Target.Y);
            }
            return Target;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new DefenderNormalRole1(), new DefenderNormalRole2() };
            //SingleObjectState defender2 = null, defender1= Model.OurRobots[RobotID];
            //if (previouslyAssignedRoles.Any(a => a.Value is DefenderNormalRole2))
            //    defender2 = Model.OurRobots[previouslyAssignedRoles.First(a => a.Value is DefenderNormalRole2).Key];
            //Position2D Pos1 = DefenderMatcher.firstdefender.Value;
            //Position2D Pos2 = DefenderMatcher.seconddefender.Value;
            //if (Pos1.Y >= Pos2.Y)
            //{
            //    if (defender2 != null && defender1.Location.Y < defender2.Location.Y)
            //        res.Add(new DefenderNormalRole2());
            //}
            //else if (Pos2.Y > Pos1.Y)
            //{
            //    if (defender2 != null && defender1.Location.Y >= defender2.Location.Y)
            //        res.Add(new DefenderNormalRole2());
            //}   
            if (FreekickDefence.SwitchDefender2Marker1)
            {
                res.Add(new DefenderMarkerNormalRole1());
                res.Add(new DefenderMarkerRole());
            }
            else if (FreekickDefence.SwitchDefender2Marker2)
            {
                res.Add(new DefenderMarkerNormalRole2());
                res.Add(new DefenderMarkerRole2());
            }
            else if (FreekickDefence.SwitchDefender2Marker3)
            {
                res.Add(new DefenderMarkerNormalRole3());
                res.Add(new DefenderMarkerRole3());
            }
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
        public enum DefenderStates
        {
            Normal = 0,
            InPenaltyArea = 1,
            Behind = 2,
            KickToGoal = 3,
            OppIndDangerZone = 4,
            BallInFront = 5,
        }
    }
}
