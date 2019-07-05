using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class StopRole1 : RoleBase, IMarkerDefender
    {
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public SingleWirelessCommand RunRole(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
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
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1,1);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1, true);
        }

        public SingleWirelessCommand RotateRun(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
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
            double StopDistFromBall = .65;
            bool isOppNear = false;
            double minDist = double.MaxValue;
            int oppPasser = -1;
            foreach (var opp in Model.Opponents)
            {
                if (minDist > Model.BallState.Location.DistanceFrom(opp.Value.Location))
                {
                    minDist = Model.BallState.Location.DistanceFrom(opp.Value.Location);
                    oppPasser = opp.Key;
                }
            }
            if (oppPasser != -1 && Model.Opponents[oppPasser].Location.DistanceFrom(Model.BallState.Location) < 0.4)
                isOppNear = true;

            if (!isOppNear)
            {
                return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1, true);
            }
            else
            {
                Vector2D ballOppVec = Model.BallState.Location - Model.Opponents[oppPasser].Location;
                ballOppVec.NormalizeTo(StopDistFromBall);
                Position2D target = Model.BallState.Location + ballOppVec;
                if (!GameParameters.IsInField(target, 0.01))
                    return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1, true);
                return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, target, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1, true);
            }

        }

        public SingleWirelessCommand RunRoleStop(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            if (Model.Status == GameStatus.BallPlace_Opponent)
            {
                ballState = new SingleObjectState(StaticVariables.ballPlacementPos, new Vector2D(), 0);
                ballStateFast = new SingleObjectState(StaticVariables.ballPlacementPos, new Vector2D(), 0);
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            if (Model.OurRobots[RobotID].Speed.X > StaticVariables.stopMaxSpeed || Model.OurRobots[RobotID].Speed.Y > StaticVariables.stopMaxSpeed)
            {
                Planner.Add(RobotID, Model.OurRobots[RobotID]);
                return new SingleWirelessCommand();
            }
            else
            {
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, StaticVariables.stopMaxSpeed);
                return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, StaticVariables.stopMaxSpeed, false);
            }
        }

        public SingleWirelessCommand LongStop(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {

            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, ballState.Location + (GameParameters.OurGoalCenter - ballState.Location).GetNormalizeToCopy(1), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, true, true, 0, 0, 1);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        private Position2D GetTarget(WorldModel Model, int RobotID)
        {
            double StopDistFromBall = .65;
            Position2D target = new Position2D();
            Position2D Target = new Position2D();
            Position2D DefenderStopRole1 = GameParameters.OurGoalCenter + new Vector2D(-1, -0.17);
            Position2D DefenderStopRole2 = GameParameters.OurGoalCenter + new Vector2D(-1, 0.17);
            Circle C = new Circle(ballState.Location, 0.7);
            double angle = Math.Sign(ballState.Location.Y) * 1 * Math.PI / 3;
            if (C.IsInCircle(DefenderStopRole1) || C.IsInCircle(DefenderStopRole2))
            {
                angle = Math.Sign(ballState.Location.Y) * 1.5 * Math.PI / 3;
            }
            Position2D ball = ballState.Location;
            ////if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.3)
            ////{
            ////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 1 * angle, .6);
            ////}
            ////else if (Math.Abs(ball.Y) > 1.85 && ball.X > 2.45)//Our Corner
            ////{
            ////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * Math.PI / 4, 0.6);
            ////}
            ////else if (Math.Abs(ball.Y) > 1.85 && ball.X < 2.45 && ball.X > -2.45)//side
            ////{
            ////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0, 0.6);
            ////}
            ////else if (Math.Abs(ball.Y) > 1.85 && ball.X < -2.45)//opp corner
            ////{
            ////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, 0.6);
            ////}
            ////else//other
            ////{
            ////    if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < GameParameters.SafeRadi(ballState, 1.4)) // reverse
            ////    {
            ////        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + Math.Sign(ball.Y) * -1 * Math.PI / 5, 0.6);
            ////    }
            ////    else
            ////    {
            ////        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, 0.6);
            ////    }

            ////}
            ////if (Model.Status != GameStatus.Stop)
            ////{
            ////    List<int> OurRobotCustom = Model.OurRobots.Where(u => u.Key != RobotID).Select(t => t.Key).ToList();
            ////    foreach (var item in OurRobotCustom)
            ////    {

            ////        if (target.DistanceFrom(Model.OurRobots[item].Location) < RobotParameters.OurRobotParams.Diameter + .01)
            ////        {
            ////            Vector2D RobotBall = target - ballState.Location;
            ////            target = ballState.Location + RobotBall.GetNormalizeToCopy(.75);
            ////        }
            ////    }
            ////}
            if (Model.Status == GameStatus.Stop || Model.Status == GameStatus.BallPlace_Opponent)
            {
                if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.4)
                {
                    target = new Position2D(2.08, 1);
                }
                else
                {
                    if (ball.X > 2.5 && Math.Abs(ball.Y) > 1.85)//corner
                    {
                        if (ball.Y < 0)
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .1, StopDistFromBall);
                        else
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .4, StopDistFromBall);

                    }
                    else if (ball.X > 2.5 && Math.Abs(ball.Y) > 1.75)//corner
                    {
                        if (ball.Y < 0)
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .4, StopDistFromBall);
                        else
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .82, StopDistFromBall);

                    }
                    else if (ball.X > 2.5 && Math.Abs(ball.Y) > 1.15)//corner
                    {
                        if (ball.Y < 0)
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .65, StopDistFromBall);
                        else
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * 1, StopDistFromBall);

                    }
                    else if (ball.X > 1.5 && Math.Abs(ball.Y) > 1.15 && ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 1.8)
                    {
                        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (ball.X > 1.5 && Math.Abs(ball.Y) > 1.15 && ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.8)
                    {
                        if (ball.Y > 0)
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + Math.Sign(ball.Y) * -.9, StopDistFromBall);
                        else
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + Math.Sign(ball.Y) * -.5, StopDistFromBall);
                    }
                    else if (ball.X > 2.2 && Math.Abs(ball.Y) > 1.15)
                    {
                        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (Math.Abs(ball.X) <= 1.5 && Math.Abs(ball.Y) > 1.15)
                    {
                        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                    }

                    else if (ball.X < -1.5 && Math.Abs(ball.Y) > 1.15)
                    {
                        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (Math.Abs(ball.Y) <= 1.15 && ball.X < .95)
                    {
                        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (Math.Abs(ball.Y) <= 1.15 && ball.X < 1.5 && ball.X >= .95)
                    {
                        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else
                    {
                        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                    }
                }
            }
            else
            {

                Target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, StopDistFromBall);
                target = OverlapSolving(Model, RobotID, Target);

            }

            //if (ballState.Location.DistanceFrom(MRL.SSL.GameDefinitions.GameParameters.OurGoalCenter) > 1.3)
            //{
            //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, 0.6);
            //    DrawingObjects.AddObject(new Circle(target, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.01f)), "tar1");
            //    return target;
            //}

            //if ((ballState.Location.Y <= -0.175 && ballState.Location.Y >= -0.676 && ballState.Location.X < GameParameters.OurGoalCenter.X && ballState.Location.DistanceFrom(new Position2D(GameParameters.OurGoalRight.X, -0.175)) <= GameParameters.DefenceareaRadii) || (ballState.Location.Y >= 0.175 && ballState.Location.Y <= 0.676 && ballState.Location.X < GameParameters.OurGoalCenter.X && ballState.Location.DistanceFrom(new Position2D(GameParameters.OurGoalLeft.X, 0.175)) <= GameParameters.DefenceareaRadii) || (ballState.Location.Y <= 0.175 && ballState.Location.Y >= -0.175 && (GameParameters.OurGoalCenter.X - ballState.Location.X) <= 0.5 && (GameParameters.OurGoalCenter.X - ballState.Location.X) >= 0))
            //{
            //    if (Math.Abs(ballState.Location.Y) < 0.5)
            //        target = new Position2D(ballState.Location.X, 0.5 * Math.Sign(ballState.Location.Y)) + Vector2D.FromAngleSize((new Position2D(ballState.Location.X, 0.5 * Math.Sign(ballState.Location.Y)) - GameParameters.OurGoalCenter).AngleInRadians, 0.6);
            //    else
            //        target = ballState.Location + Vector2D.FromAngleSize((ballState.Location - GameParameters.OurGoalCenter).AngleInRadians, 0.6);
            //    DrawingObjects.AddObject(new Circle(target, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.01f)), "tar1");
            //    return target;
            //}
            ////return target;
            ////double a = GameParameters.DefenceAreaFrontWidth;
            ////a = GameParameters.DefenceareaRadii;

            //if (ballState.Location.Y > 0.76)
            //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalLeft - ballState.Location).AngleInRadians - 1.95, 0.6);
            //else if (ballState.Location.Y < 0.76 && ballState.Location.Y >= 0)
            //    target = ballState.Location + Vector2D.FromAngleSize((new Position2D(GameParameters.OurLeftCorner.X, .076) - ballState.Location).AngleInRadians - 1.4, 0.60);
            //else if (ballState.Location.Y < 0 && ballState.Location.Y > -.76)
            //    target = ballState.Location + Vector2D.FromAngleSize((new Position2D(GameParameters.OurRightCorner.X, .076) - ballState.Location).AngleInRadians + 1.1, 0.60);
            //else
            //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalRight - ballState.Location).AngleInRadians + 1.15, 0.6);

            //DrawingObjects.AddObject(new Circle(target, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.01f)));
            ;
            return target;
            ////return ballState.Location + Vector2D.FromAngleSize((GameParameters.OppGoalCenter - ballState.Location).AngleInRadians, 0.6);
        }

        Position2D OverlapSolving(WorldModel Model, int RobotID, Position2D Target)
        {
            Dictionary<int, SingleObjectState> ourRobots = Model.OurRobots.Where(u => u.Key != RobotID).ToDictionary(t => t.Key, y => y.Value);
            if (Model.GoalieID.HasValue)
            {
                ourRobots = Model.OurRobots.Where(t => t.Key != Model.GoalieID.Value && t.Key != RobotID).ToDictionary(y => y.Key, h => h.Value);
            }
            Position2D extendpoint = Target;
            int counter = 0;
            foreach (var item in ourRobots)
            {
                if (item.Value.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .25)
                {
                    counter++;
                    Vector2D RobotOGC = GameParameters.OurGoalCenter - Model.OurRobots[RobotID].Location;
                    Position2D LeftPoint = item.Value.Location + Vector2D.FromAngleSize(RobotOGC.AngleInRadians - Math.PI / 2, .24);
                    Position2D RightPoint = item.Value.Location + Vector2D.FromAngleSize(RobotOGC.AngleInRadians + Math.PI / 2, .24);
                    Line RobotOGCLn = new Line(ballState.Location, GameParameters.OppGoalCenter);
                    DrawingObjects.AddObject(RobotOGC);
                    DrawingObjects.AddObject(LeftPoint);
                    DrawingObjects.AddObject(RightPoint);
                    Vector2D RobotOGCVEc = RobotOGCLn.Tail - RobotOGCLn.Head;
                    Position2D PerpPOsLeft = new Line(LeftPoint + Vector2D.FromAngleSize((RobotOGCLn.Tail - RobotOGCLn.Head).AngleInRadians + (Math.PI / 2), 1), LeftPoint).IntersectWithLine(RobotOGCLn).Value;
                    Position2D PerpPOsRight = new Line(RightPoint + Vector2D.FromAngleSize((RobotOGCLn.Tail - RobotOGCLn.Head).AngleInRadians + (Math.PI / 2), 1), RightPoint).IntersectWithLine(RobotOGCLn).Value;
                    double Disttoleft = PerpPOsLeft.DistanceFrom(LeftPoint);
                    double DisttoRight = PerpPOsRight.DistanceFrom(RightPoint);
                    if (Disttoleft < DisttoRight)
                    {
                        Target = LeftPoint;
                    }
                    else
                    {
                        Target = RightPoint;
                    }
                    //Vector2D OGCandRobot = GameParameters.OurGoalCenter - item.Value.Location;
                    //Vector2D extendtempvec = Model.OurRobots[RobotID].Location - item.Value.Location;
                    ////extendtempvec.NormalizeTo(0.22);
                    //extendpoint = item.Value.Location + extendtempvec.GetNormalizeToCopy(.25);


                    //if (OGCandRobot.InnerProduct(extendtempvec) > 0)
                    //    extendpoint = Model.OurRobots[RobotID].Location - extendtempvec;
                    //Vector2D ExtendPoint2 = extendpoint - ballState.Location;

                    //ExtendPoint2.NormalizeTo(.6);
                    //extendpoint = ballState.Location + ExtendPoint2;

                }
                if (counter == 0)
                {
                    Target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, .6);

                }
            }

            extendpoint = Target;
            return extendpoint;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return GetTarget(Model, RobotID).DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>();
            res.Add(new StopRole1());
            res.Add(new ActiveRole());
            //if (!FreekickDefence.BallIsMoved)
            //{
            //    res.Add(new NewDefenderMarkerRole2());
            //    res.Add(new NewDefenderMrkerRole());
            //    res.Add(new NewDefenderMarkerRole3());
            //}
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
