﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Plays;
using MRL.SSL.AIConsole.Roles.Defending.Normal;
using MRL.SSL.AIConsole.Roles.Defending;

namespace MRL.SSL.AIConsole.Roles
{
    public class StaticDefender2 : RoleBase
    {
        public Position2D lastTarget = new Position2D();
        public Position2D Target = new Position2D();

        public static Position2D eatballTarget = new Position2D();

        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        //GetBallRole b;

        public static double staticAngle = 0;
        public static bool angleUpdateFlag = false;

        public static Position2D lackState = new Position2D();
        private static bool saveLackFrame = true;

        static int WantedRobotID = 0;
        static int pursueRobotID = 0;

        static bool onceaTime = true;
        static int ballSign = -1;

        public SingleWirelessCommand Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta, Dictionary<int, RoleBase> roles)
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
            double teta = 180;
            DefenceInfo inf2 = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
                inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();
            DefenceInfo inf1 = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender1)))
                inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender1)).First();
            DrawingObjects.AddObject(new Circle(inf2.DefenderPosition.Value, 0.13, new System.Drawing.Pen((inf2.OppID.HasValue) ? System.Drawing.Color.Blue : System.Drawing.Color.Orange, 0.01f)));


            bool robotAvoidance = false;
            #region new IO 2015
            if (FreekickDefence.newmotion)
            {
                if (inf2 != null && inf2.DefenderPosition.HasValue && inf2 != null && inf2.DefenderPosition.HasValue && FreekickDefence.Static1ID.HasValue && FreekickDefence.Static2ID.HasValue)
                {
                    double distanceST1ToST2 = Model.OurRobots[FreekickDefence.Static1ID.Value].Location.DistanceFrom(Model.OurRobots[FreekickDefence.Static2ID.Value].Location);
                    double distanceST1ToHisDefence = Model.OurRobots[FreekickDefence.Static1ID.Value].Location.DistanceFrom(inf1.DefenderPosition.Value);
                    double distanceST2ToHisDefence = Model.OurRobots[FreekickDefence.Static2ID.Value].Location.DistanceFrom(inf2.DefenderPosition.Value);
                    double distanceST1ToOtherDefence = Model.OurRobots[FreekickDefence.Static1ID.Value].Location.DistanceFrom(inf2.DefenderPosition.Value);
                    double distanceST2ToOtherDefence = Model.OurRobots[FreekickDefence.Static2ID.Value].Location.DistanceFrom(inf1.DefenderPosition.Value);

                    Vector2D ballGoalCenter = (Model.BallState.Location /*+ Model.BallState.Speed*/) - GameParameters.OurGoalCenter;
                    Line ballGoalcentere = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + ballGoalCenter);
                    double robot1distance = Math.Abs(Vector2D.AngleBetweenInDegrees(ballGoalCenter, Model.OurRobots[FreekickDefence.Static1ID.Value].Location - GameParameters.OurGoalCenter));//ballGoalcentere.Distance(Model.OurRobots[FreekickDefence.Static1ID.Value].Location);

                    double robot2distance = Math.Abs(Vector2D.AngleBetweenInDegrees(ballGoalCenter, Model.OurRobots[FreekickDefence.Static2ID.Value].Location - GameParameters.OurGoalCenter));//ballGoalcentere.Distance(Model.OurRobots[FreekickDefence.Static2ID.Value].Location);

                    List<int> ids = new List<int>() { pursueRobotID, WantedRobotID };

                    if (!ids.Contains(FreekickDefence.Static1ID.Value) || !ids.Contains(FreekickDefence.Static2ID.Value) || !Model.OurRobots.ContainsKey(WantedRobotID) || !Model.OurRobots.ContainsKey(pursueRobotID))
                    {
                        onceaTime = true;
                    }

                    if (Math.Sign(robot2distance - robot1distance) != ballSign)
                    {
                        onceaTime = true;
                    }
                    //age axzin ta in ba in dashte bashe 
                    //age azin ta in bedoone in nadashte bashe
                    if (onceaTime)
                    {
                        onceaTime = false;
                        if (robot2distance <= robot1distance)
                        {
                            WantedRobotID = FreekickDefence.Static2ID.Value;
                            pursueRobotID = FreekickDefence.Static1ID.Value;
                        }
                        else
                        {
                            WantedRobotID = FreekickDefence.Static1ID.Value;
                            pursueRobotID = FreekickDefence.Static2ID.Value;
                        }
                        ballSign = Math.Sign(robot2distance - robot1distance);
                    }

                    bool isBall = (inf2.TargetState.Type == ObjectType.Ball) ? true : false;

                    if (pursueRobotID == RobotID && Model.OurRobots[RobotID].Location.DistanceFrom(inf2.DefenderPosition.Value) > .05)
                    {
                        TargetPos = SwimingFish(Model, WantedRobotID, pursueRobotID, inf2.DefenderPosition.Value, Math.Max(inf2.DefenderPosition.Value.DistanceFrom(inf1.DefenderPosition.Value), .2), out robotAvoidance);
                        TargetPos = GameParameters.OurGoalCenter + (TargetPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(TargetPos, Vector2D.Zero, 0f), RobotParameters.OurRobotParams.Diameter / 2 ));

                    }
                    else
                    {
                        robotAvoidance = true;
                    }

                }
            }
            #endregion


            if (FreekickDefence.StaticSecondState == DefenderStates.Normal || FreekickDefence.StaticSecondState == DefenderStates.KickToGoal)
            {
                {
                    Target = TargetPos;

                    if (FreekickDefence.StaticFirstState == DefenderStates.KickToGoal)
                    {
                        Position2D t = GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 1.5);
                        teta = (Model.OurRobots[RobotID].Location - t).AngleInDegrees;
                    }
                    else
                        teta = Teta;

                }
            }
            else if (FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
            {
                var free = true;//FreeToKick ( engine , Model );

                GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, TargetToKick(Model, RobotID));

                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, new List<int>());
                if (!obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, .5), Vector2D.Zero, 0f), .04))
                {
                    Planner.AddKick(RobotID, kickPowerType.Speed, true, 2);
                }
                return new SingleWirelessCommand();
            }
            if (FreekickDefence.EaththeBall)
            {
                if (firstTime2)
                {
                    firstTime2 = false;
                    eatballTarget = TargetPos;

                    teta = Teta;
                    robotAvoidance = false;
                    avoidance = false;
                    FreekickDefence.ReadyForEatStatic2 = true;
                }
            }
            else
            {
                FreekickDefence.ReadyForEatStatic2 = false;
                firstTime2 = true;
                avoidance = true;
            }

            if (FreekickDefence.ReadyForEatStatic2)
            {
                Target = eatballTarget;
            }
            else
            {
                Target = TargetPos;
            }


            lackState = TargetPos;
            DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, .1, new Pen(Brushes.Silver, .03f)), "6545646897648987654564");

            DrawingObjects.AddObject(new Circle(lackState, .1, new Pen(Brushes.HotPink, .03f)), "5464654654654");

            if (!FreekickDefence.newmotion)
            {
                avoidance = true;
                robotAvoidance = true;
            }
            var angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;

            Planner.Add(RobotID, Target, angle, PathType.UnSafe, false, robotAvoidance, avoidance, avoidance);

            return new SingleWirelessCommand();

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
        public Position2D MarkFront(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenceInfo info, double margin, out double Teta, Dictionary<int, RoleBase> roles)
        {

            SingleObjectState Target = (info != null && info.OppID.HasValue) ? Model.Opponents[info.OppID.Value] : ballState;
            //if (engine.GameInfo.OppTeam.Scores.Count > 1)
            //{
            //    var opp = Model.Opponents[engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Skip(1).First().Key];
            //    if (opp.Location.X < 1)
            //        Target = Model.Opponents[engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).First().Key];
            //    else
            //        Target = opp;

            //}
            //else if (engine.GameInfo.OppTeam.Scores.Count == 1)
            if (engine.GameInfo.OppTeam.Scores.Count > 0)
                Target = Model.Opponents[engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).First().Key];
            Teta = (Target.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            double min = GameParameters.SafeRadi(Target, margin);


            Position2D Pos = GameParameters.OurGoalCenter - (GameParameters.OurGoalCenter - Target.Location).GetNormalizeToCopy(min);
            if (roles.Any(a => a.Value is StaticDefender1))
            {
                var df1 = roles.Single(a => a.Value is StaticDefender1);
                double dist = Pos.DistanceFrom(Model.OurRobots[df1.Key].Location);
                if (dist < 0.25)
                    Pos = Pos + Vector2D.FromAngleSize((Target.Location - Pos).AngleInRadians - Math.PI / 2, 0.25 - dist);
            }
            Pos.DrawColor = System.Drawing.Color.Red;
            DrawingObjects.AddObject(Pos);
            return Pos;
        }
        public Position2D GetBackBallPoint(WorldModel Model, int RobotID, out double Teta)
        {
            Vector2D vec = ballState.Location - GameParameters.OppGoalCenter;
            Vector2D ballSpeed = ballState.Speed;
            Position2D ballLocation = ballState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = GameParameters.OppGoalCenter - ballLocation;
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
            Teta = (-vec).AngleInDegrees;
            return finalPosToGo;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            //double dist, dist2;
            //double tresh = 0.05;
            //double margin;
            //DefenceInfo inf = null;
            //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
            //    inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();
            //int? ballOwner = engine.GameInfo.OurTeam.BallOwner;
            //if (CurrentState == (int)DefenderStates.Normal)
            //{
            //    margin = 0.1;

            //    if (BallKickedToGoal(Model))
            //    {
            //        Position2D perp1 = new Position2D(), perp2 = new Position2D();

            //        if (!AssignedRoles.Any(a => a.Value is StaticDefender1))
            //            CurrentState = (int)DefenderStates.KickToGoal;
            //        else
            //        {
            //            int def1 = AssignedRoles.Single(a => a.Value is StaticDefender1).Key;
            //            perp2 = ballState.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location);
            //            perp1 = ballState.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[def1].Location);
            //            if (perp2.DistanceFrom(Model.OurRobots[RobotID].Location) < perp1.DistanceFrom(Model.OurRobots[def1].Location))
            //                CurrentState = (int)DefenderStates.KickToGoal;
            //            else
            //                CurrentState = (int)DefenderStates.KickToGoal2;
            //        }
            //    }
            //    else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
            //    {
            //        CurrentState = (int)DefenderStates.InPenaltyArea;
            //    }
            //    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop)
            //    {
            //        CurrentState = (int)DefenderStates.BallInFront;
            //    }
            //    else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
            //    {
            //        CurrentState = (int)DefenderStates.OppIndDangerZone;
            //    }
            //}

            //else if (CurrentState == (int)DefenderStates.InPenaltyArea)
            //{
            //    margin = 0.3;
            //    if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
            //    {
            //        CurrentState = (int)DefenderStates.Normal;
            //    }
            //    else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
            //    {
            //        CurrentState = (int)DefenderStates.OppIndDangerZone;
            //    }
            //}
            //else if (CurrentState == (int)DefenderStates.KickToGoal)
            //{
            //    margin = 0.1;
            //    if (!BallKickedToGoal(Model))
            //    {
            //        if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
            //        {
            //            CurrentState = (int)DefenderStates.InPenaltyArea;
            //        }
            //        else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop)
            //        {
            //            CurrentState = (int)DefenderStates.BallInFront;
            //        }
            //        else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
            //        {
            //            CurrentState = (int)DefenderStates.OppIndDangerZone;
            //        }
            //        else
            //            CurrentState = (int)DefenderStates.Normal;
            //    }
            //}
            //else if (CurrentState == (int)DefenderStates.BallInFront)
            //{
            //    margin = 0.1;
            //    if (engine.Status == GameStatus.Stop)
            //    {
            //        CurrentState = (int)DefenderStates.Normal;
            //    }
            //    else
            //    {
            //        if (!ballOwner.HasValue || (ballOwner.HasValue && ballOwner.Value != RobotID))
            //        {
            //            if (BallKickedToGoal(Model))
            //            {
            //                Position2D perp1 = new Position2D(), perp2 = new Position2D();

            //                if (!AssignedRoles.Any(a => a.Value is StaticDefender1))
            //                    CurrentState = (int)DefenderStates.KickToGoal;
            //                else
            //                {
            //                    int def1 = AssignedRoles.Single(a => a.Value is StaticDefender1).Key;
            //                    perp2 = ballState.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location);
            //                    perp1 = ballState.Speed.PrependecularPoint(ballState.Location, Model.OurRobots[def1].Location);
            //                    if (perp2.DistanceFrom(Model.OurRobots[RobotID].Location) < perp1.DistanceFrom(Model.OurRobots[def1].Location))
            //                        CurrentState = (int)DefenderStates.KickToGoal;
            //                    else
            //                        CurrentState = (int)DefenderStates.KickToGoal2;
            //                }
            //            }
            //            else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
            //            {
            //                CurrentState = (int)DefenderStates.InPenaltyArea;
            //            }
            //            else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
            //            {
            //                CurrentState = (int)DefenderStates.OppIndDangerZone;
            //            }
            //            else
            //                CurrentState = (int)DefenderStates.Normal;
            //            b = null;
            //        }
            //    }
            //}
            //else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
            //{
            //    margin = 0.1;
            //    if (inf == null || (inf != null && inf.TargetState.Type != ObjectType.Opponent) || (inf != null && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) > tresh))
            //    {
            //        if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
            //        {
            //            CurrentState = (int)DefenderStates.InPenaltyArea;
            //        }
            //        else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop)
            //        {
            //            CurrentState = (int)DefenderStates.BallInFront;
            //        }
            //        else
            //            CurrentState = (int)DefenderStates.Normal;
            //    }
            //    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop)
            //    {
            //        CurrentState = (int)DefenderStates.BallInFront;
            //    }
            //}
            //else if (CurrentState == (int)DefenderStates.KickToGoal2)
            //{
            //    margin = 0.1;
            //    if (!BallKickedToGoal(Model))
            //    {
            //        if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))
            //        {
            //            CurrentState = (int)DefenderStates.InPenaltyArea;
            //        }

            //        else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop)
            //        {
            //            CurrentState = (int)DefenderStates.BallInFront;
            //        }
            //        else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
            //        {
            //            CurrentState = (int)DefenderStates.OppIndDangerZone;
            //        }
            //        else
            //            CurrentState = (int)DefenderStates.Normal;
            //    }
            //}

            //DrawingObjects.AddObject(new StringDraw(((DefenderStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location + new Vector2D(0.2, 0.2)), "Def2");
            //FreekickDefence.CurrentStates[this] = CurrentState;
            CurrentState = 0;
        }

        private bool BallKickedToGoal(WorldModel Model)
        {
            Line line = new Line();
            line = new Line(ballState.Location, ballState.Location - ballState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + .23 && BallGoal.Y > GameParameters.OurGoalRight.Y - .23)
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

                Position2D pos = inf.DefenderPosition.Value;//Cost(engine, Model, RobotID, inf.DefenderPosition.Value, inf.Teta, state, previouslyAssignedRoles);
                //Todo: I alter it for optimswarm
                double dist = pos.DistanceFrom(Model.OurRobots[RobotID].Location);
                ////double dist = NewNormalPlay.calculatedDef2Temp.DistanceFrom(Model.OurRobots[RobotID].Location);

                return dist * dist;
            }
            return 100;
        }
        public Position2D Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double p, int state, Dictionary<int, RoleBase> roles)
        {
            double teta = 180;
            DefenceInfo inf = null;
            Position2D Target = new Position2D();
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();

            //if (state == (int)DefenderStates.InPenaltyArea)
            //{
            //    Target = MarkFront(engine, Model, RobotID, inf, 0.1, out teta, roles);
            //}
            //else
            if (state == (int)DefenderStates.Normal || state == (int)DefenderStates.InPenaltyArea)
            {
                Target = TargetPos;
                Vector2D vec = Target - Model.OurRobots[RobotID].Location;
                Target = Target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));
            }
            else if (state == (int)DefenderStates.BallInFront)
            {
                Target = GetBackBallPoint(Model, RobotID, out teta);
            }
            else if (state == (int)DefenderStates.OppIndDangerZone)
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
            else if (state == (int)DefenderStates.KickToGoal)
            {
                Target = Dive(engine, Model, RobotID);
                teta = Model.OurRobots[RobotID].Angle.Value;
            }
            if ((state == (int)DefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
            {
                Target = new Position2D(2.9, Target.Y);
            }
            return Target;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Position2D tempball = ballState.Location + ballState.Speed * 0.16;
            double d1, d2;
            List<RoleBase> res = new List<RoleBase>() { new StaticDefender1(), new StaticDefender2(), new staticDefender3(), new GerrardRole()};
            if (FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new ActiveRole());
            }
            if (FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new NewActiveRole());
            }
            
            if (FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new ActiveRole2017());
            }
           
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }


        bool FreeToKick(GameStrategyEngine engine, WorldModel Model)
        {
            double accercy = 0.2;
            double minGoodnessToKick = 0.3;
            double KickAnyWayTresh = -1.7;
            double goodnessKick = 0;
            bool Chipkick = false;
            Position2D? PointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, ballState.Location, out goodnessKick, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null);


            bool FreeToKick = false;
            bool FreeToChip = false;
            bool Free = false;
            if (goodnessKick < minGoodnessToKick /*|| !PointInGoal.HasValue*/)
            {
                int? oppOwnerId = engine.GameInfo.OppTeam.BallOwner;
                //if (/*oppOwnerId.HasValue && */Model.Opponents.ContainsKey(oppOwnerId.Value))
                {
                    Line lGoal = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);

                    bool flag1 = false, flag2 = false, flag3 = false;
                    foreach (var item in Model.Opponents)
                    {
                        Line l = new Line(Model.Opponents[item.Key].Location, ballState.Location);
                        Position2D? intersect = l.IntersectWithLine(lGoal);
                        if (intersect.HasValue && Math.Abs(intersect.Value.Y) < Math.Abs(GameParameters.OppGoalLeft.Y) + 0.2 &&
                            Model.Opponents[item.Key].Location.DistanceFrom(ballState.Location) < 0.25)
                        {
                            flag1 = true;
                            break;
                        }

                        else if (intersect.HasValue && Math.Abs(intersect.Value.Y) < Math.Abs(GameParameters.OppGoalLeft.Y) + 0.2 &&
                            Model.Opponents[item.Key].Location.DistanceFrom(ballState.Location) < 1)
                        {

                            flag2 = true;
                        }
                        else if (intersect.HasValue && Math.Abs(intersect.Value.Y) < Math.Abs(GameParameters.OppGoalLeft.Y) + 0.2 &&
                            Model.Opponents[item.Key].Location.DistanceFrom(ballState.Location) >= 1)
                        {

                            flag3 = true;
                        }
                    }

                    if (flag1)
                    {
                        Free = false;
                    }
                    else if (flag2)
                    {

                        Free = false;
                    }
                    else if (flag3)
                    {
                        Free = true;
                    }
                }
                Free = false;
            }
            else
            {
                Free = true;
            }
            return Free;

        }
        bool FreeToKick(GameStrategyEngine engine, WorldModel Model, int robotID)
        {
            Obstacles obs = new Obstacles(Model);
            List<int> ids = new List<int>();
            ids.Add(robotID);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { robotID }, null);
            var meet = obs.Meet(new SingleObjectState(ballState.Location, Vector2D.Zero, 0), new SingleObjectState(ballState.Location + Vector2D.FromAngleSize(Model.OurRobots[robotID].Angle.Value * Math.PI / 180, 4), Vector2D.Zero, 0), 0.04);
            return !meet;
        }


        public Position2D TargetToKick(WorldModel Model, int robotID)
        {
            //double coef = 1;
            //if (targetState == 0)
            //    coef = 1.5;
            //double minopp = double.MaxValue;
            //if (Model.Opponents.Count > 0)
            //    minopp = Model.Opponents.Min(m => m.Value.Location.DistanceFrom(ballState.Location));
            //double ourDist = Model.OurRobots[robotID].Location.DistanceFrom(ballState.Location);
            //if (minopp > 0.5 && ourDist < 0.2 / coef)
            //{
            //    targetState = 0;
            //    return new Position2D(ballState.Location.X - 2, ballState.Location.Y); // GameParameters.OppGoalCenter;
            //}
            //targetState = 1;
            //return ballState.Location + (ballState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(2);
            return new Position2D(GameParameters.OppLeftCorner.X - .5, Math.Sign(Model.OurRobots[robotID].Location.Y) * Math.Abs(GameParameters.OppLeftCorner.Y));
        }
        int targetState = 0;
        private bool firstTime2 = true;
        private bool avoidance = true;

        static Position2D SwimingFish(WorldModel Model, int wantedId, int pursueID, Position2D defenceTarget, double distBetween2Roles, out bool robotAvoidance)
        {

            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { wantedId, pursueID }, new List<int>());
            Position2D pursueRobot = Model.OurRobots[pursueID].Location;
            Position2D wantedrobot = Model.OurRobots[wantedId].Location;
            double deltaxCurrent = pursueRobot.DistanceFrom(wantedrobot);
            double targetdistcance = distBetween2Roles;
            double extendSize = -(targetdistcance - deltaxCurrent);
            if (extendSize < 0 && Model.OurRobots[pursueID].Speed.InnerProduct(wantedrobot - pursueRobot) > 0)
            {
                //extendSize += (-Model.OurRobots[pursueID].Speed.Size);
            }
            Position2D targete = defenceTarget;
            Vector2D targetv = (wantedrobot - pursueRobot).GetNormalizeToCopy(((Math.Sign(extendSize) * deltaxCurrent) + (extendSize)));
            Position2D tg = wantedrobot + targetv;
            Obstacles obstmp = new Obstacles(Model);
            Obstacles obstmp2 = new Obstacles(Model);
            Obstacles obstmp3 = new Obstacles(Model);
            obstmp3.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID }, new List<int>());// mohavate jarime va robotha bedoone robote taghib shavande
            obstmp2.AddObstacle(1, 0, 1, 0, new List<int>() { pursueID }, new List<int>());

            obstmp.AddObstacle(0, 0, 1, 0, new List<int>(), new List<int>(), false);
            if (deltaxCurrent > Model.OurRobots[pursueID].Location.DistanceFrom(targete) && obstmp.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = true;
            }
            else if (deltaxCurrent > Model.OurRobots[pursueID].Location.DistanceFrom(targete) && obstmp2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = true;
            }
            else if (!obstmp2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = false;
            }
            else
            {
                if (!obs.Meet(Model.OurRobots[pursueID], Model.OurRobots[wantedId], .15))
                {

                    robotAvoidance = false;
                }
                if (!obs.Meet(Model.OurRobots[pursueID], Model.OurRobots[wantedId], .15))
                {

                    robotAvoidance = false;
                }
                else
                {
                    robotAvoidance = true;
                }
            }
            Obstacles obstacles = new Obstacles(Model);
            obstacles.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID }, new List<int>());

            List<int> IDsExeptthanwanted = new List<int>();
            IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != wantedId).Select(y => y.Key).ToList();

            Obstacles obstacles2 = new Obstacles(Model);
            obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, new List<int>());

            Obstacles obstacles3 = new Obstacles(Model);
            obstacles3.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID, wantedId }, new List<int>());
            Position2D sttarget = Model.OurRobots[pursueID].Location + (Model.OurRobots[wantedId].Location - Model.OurRobots[pursueID].Location).GetNormalizeToCopy(Model.OurRobots[wantedId].Location.DistanceFrom(Model.OurRobots[pursueID].Location) - .2);

            if (obstacles.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12) && !obstacles2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12))
            {
                tg = targete;
                robotAvoidance = true;
            }
            if (obstacles3.Meet(Model.OurRobots[pursueID], Model.OurRobots[wantedId], .15) && obstacles.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12) && sttarget.DistanceFrom(Model.OurRobots[pursueID].Location) < targete.DistanceFrom(Model.OurRobots[pursueID].Location) && targete.DistanceFrom(Model.OurRobots[pursueID].Location) > .15)
            {
                tg = Model.OurRobots[pursueID].Location + (Model.OurRobots[wantedId].Location - Model.OurRobots[pursueID].Location).GetNormalizeToCopy(Model.OurRobots[wantedId].Location.DistanceFrom(Model.OurRobots[pursueID].Location) - .2);
                robotAvoidance = true;
            }
            else
            {
                //tg = targete;
                //robotAvoidance = false;

            }
            //if (obstacles.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12) && obstacles2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12))
            //{
            //    tg = wantedrobot + targetv;
            //    robotAvoidance = true;
            //}
            //else if (obstacles3.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12))
            //{
            //    tg = targete;
            //    robotAvoidance = false;
            //}
            return tg;

        }
        static Position2D SwimingFish(WorldModel Model, int pursueID, Position2D defenceTarget, out bool robotAvoidance)
        {
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID }, new List<int>());
            Position2D pursueRobot = Model.OurRobots[pursueID].Location;
            Position2D wantedrobot = defenceTarget;
            double deltaxCurrent = pursueRobot.DistanceFrom(wantedrobot);
            double extendSize = deltaxCurrent;
            //if (extendSize < 0 && Model.OurRobots[pursueID].Speed.InnerProduct(wantedrobot - pursueRobot) > 0)
            //{
            //    extendSize += (-Model.OurRobots[pursueID].Speed.Size);
            //}
            Position2D targete = defenceTarget;
            Vector2D targetv = (wantedrobot - pursueRobot).GetNormalizeToCopy(((Math.Sign(extendSize) * deltaxCurrent) + (extendSize)));
            Position2D tg = wantedrobot + targetv;
            Obstacles obstmp = new Obstacles(Model);
            Obstacles obstmp2 = new Obstacles(Model);

            obstmp2.AddObstacle(1, 0, 1, 0, new List<int>() { pursueID }, new List<int>());

            obstmp.AddObstacle(0, 0, 1, 0, new List<int>(), new List<int>());
            if (deltaxCurrent > Model.OurRobots[pursueID].Location.DistanceFrom(targete) && obstmp.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = false;
            }
            else if (!obstmp2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = false;
            }
            else
            {
                if (!obs.Meet(Model.OurRobots[pursueID], Model.BallState, .15))
                {
                    tg = targete;
                    robotAvoidance = false;
                }
                else
                {
                    robotAvoidance = true;
                }
            }
            return tg;

        }

        //private static bool isMarker = false;
        //public SingleWirelessCommand RunRole(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        //{

        //    SingleWirelessCommand SWC = new SingleWirelessCommand();
        //    Position2D? Pos = FindPos(Model);
        //    if (Pos != null)
        //    {
        //        Position2D DefencePos = Pos.Value;
        //        DrawingObjects.AddObject(new Circle(DefencePos, RobotParameters.OurRobotParams.Diameter / 2.0, new Pen(Brushes.Aqua, 0.02f)));

        //        if (isMarker == false)
        //        {
        //            SWC = GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, DefencePos, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInRadians * (180.0 / Math.PI), false, true, false, 0, 0, 1);
        //        }
        //        else
        //        {
        //            int? MarkerID = FindMarker(Model);
        //            if (MarkerID != null)
        //            {
        //                SWC = GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, DefencePos, (Model.Opponents[MarkerID.Value].Location - Model.OurRobots[RobotID].Location).AngleInRadians * (180.0 / Math.PI), false, true, false, 0, 0, 1);
        //            }
        //        }
        //    }
        //    return SWC;
        //}
        //private static Position2D? FindPos(WorldModel Model)
        //{
        //    SingleWirelessCommand SWC = new SingleWirelessCommand();
        //    Position2D DefencePos = new Position2D();
        //    Position2D NextPos = new Position2D();

        //    Line L1 = new Line(ballState.Location, GameParameters.OurGoalRight + new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 2));

        //    double radi = GameParameters.SafeRadi(ballState, 0);
        //    Circle C1 = new Circle(GameParameters.OurGoalCenter, radi);
        //    List<Position2D> P = C1.Intersect(L1);

        //    isMarker = false;

        //    double minDist = double.MaxValue;
        //    for (int i = 0; i < P.Count; i++)
        //    {
        //        if (P[i].DistanceFrom(ballState.Location) < minDist)
        //        {
        //            minDist = P[i].DistanceFrom(ballState.Location);
        //            DefencePos = P[i];
        //        }
        //    }

        //    Line L2 = new Line(ballState.Location, GameParameters.OurGoalLeft - new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 2));
        //    List<Position2D> P2 = C1.Intersect(L2);

        //    minDist = double.MaxValue;
        //    for (int i = 0; i < P2.Count; i++)
        //    {
        //        if (P2[i].DistanceFrom(ballState.Location) < minDist)
        //        {
        //            minDist = P2[i].DistanceFrom(ballState.Location);
        //            NextPos = P2[i];
        //        }
        //    }


        //    if (DefencePos.DistanceFrom(NextPos) < RobotParameters.OurRobotParams.Diameter)
        //    {
        //        int? MarkerID = FindMarker(Model);
        //        if (MarkerID != null)
        //        {
        //            isMarker = true;
        //            DefencePos = GameParameters.OurGoalCenter + (Model.Opponents[MarkerID.Value].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.8);
        //        }
        //        else
        //        {
        //            if (ballState.Location.Y < 0)
        //            {
        //                Position2D Corner = new Position2D(0, GameParameters.OurLeftCorner.Y);
        //                DefencePos = GameParameters.OurGoalCenter + (Corner - GameParameters.OurGoalCenter).GetNormalizeToCopy(radi);
        //            }
        //            else
        //            {
        //                Position2D Corner = new Position2D(0, GameParameters.OurRightCorner.Y);
        //                DefencePos = GameParameters.OurGoalCenter + (Corner - GameParameters.OurGoalCenter).GetNormalizeToCopy(radi);
        //            }
        //        }
        //    }

        //    return DefencePos;
        //}
        //private static int? FindNearestOpponentToBall(WorldModel Model)
        //{
        //    int? retIndex = null;
        //    double MinDist = double.MaxValue;
        //    foreach (int key in Model.Opponents.Keys)
        //    {
        //        if (ballState.Location.DistanceFrom(Model.Opponents[key].Location) < MinDist)
        //        {
        //            MinDist = ballState.Location.DistanceFrom(Model.Opponents[key].Location);
        //            retIndex = key;
        //        }
        //    }
        //    return retIndex;
        //}
        //private static int? FindMarker(WorldModel Model)
        //{
        //    int? retIndex = null;
        //    double MinDist = double.MaxValue;
        //    int? BallCacher = FindNearestOpponentToBall(Model);
        //    if (BallCacher != null)
        //    {
        //        foreach (int key in Model.Opponents.Keys)
        //        {
        //            if (key != BallCacher)
        //            {
        //                if (GameParameters.OurGoalCenter.DistanceFrom(Model.Opponents[key].Location) < MinDist)
        //                {
        //                    MinDist = ballState.Location.DistanceFrom(Model.Opponents[key].Location);
        //                    retIndex = key;
        //                }
        //            }
        //        }
        //    }
        //    return retIndex;
        //}

        //public override RoleCategory QueryCategory()
        //{
        //    return RoleCategory.Defender;
        //}

        //public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        //{
        //    CurrentState = 1;
        //}

        //public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        //{
        //    return FindPos(Model).Value.DistanceFrom(Model.OurRobots[RobotID].Location);
        //}

        //public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        //{
        //    return new List<RoleBase>() { new StaticDefender1(), new StaticDefender2() };
        //}

        //public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
