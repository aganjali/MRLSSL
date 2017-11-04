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
    class BusStopRole1 : RoleBase
    {
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public SingleWirelessCommand RunRole(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            ballState = Model.BallState;
            ballStateFast = Model.BallStateFast;
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID, 0.65), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
        }

        public SingleWirelessCommand RotateRun(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            ballState = Model.BallState;
            ballStateFast = Model.BallStateFast;
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
            if (oppPasser != -1 && Model.Opponents[oppPasser].Location.DistanceFrom(Model.BallState.Location) < 0.35)
                isOppNear = true;

            if (!isOppNear)
            {
                return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID, 0.65), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
            }
            else
            {
                Vector2D ballOppVec = Model.BallState.Location - Model.Opponents[oppPasser].Location;
                ballOppVec.NormalizeTo(StopDistFromBall);
                Vector2D ballOurGoalCenterVec = GameParameters.OurGoalCenter - Model.BallState.Location;
                DrawingObjects.AddObject(new StringDraw("opp to ball angle: " + ballOppVec.AngleInDegrees, GameParameters.OurGoalCenter.Extend(0.7, 0)));
                DrawingObjects.AddObject(new StringDraw("real opp angle: " + Model.Opponents[oppPasser].Angle.Value, GameParameters.OurGoalCenter.Extend(0.9, 0)));

                double d = (Model.Opponents[oppPasser].Angle.Value <= 180 ? Model.Opponents[oppPasser].Angle.Value : Model.Opponents[oppPasser].Angle.Value - 360);
                DrawingObjects.AddObject(new StringDraw("fake opp angle: " + d.ToString(), GameParameters.OurGoalCenter.Extend(1, 0)));
                if (Math.Abs(Math.Abs(d) - Math.Abs(ballOppVec.AngleInDegrees)) < 25)
                {
                    Position2D target = Model.BallState.Location + ballOppVec;
                    Line l1 = new Line(Model.BallState.Location, (Model.BallState.Location.Y > 0 ? GameParameters.OurGoalLeft : GameParameters.OurGoalRight));
                    d = (GameParameters.OurGoalCenter.X - Model.BallState.Location.X) / 2;
                    if (GameParameters.OurGoalCenter.X - target.X > d)
                    {

                    }
                    if (!GameParameters.IsInField(target, 0.01) && Math.Abs(ballOppVec.AngleInDegrees - ballOurGoalCenterVec.AngleInDegrees) > 20)
                        return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
                    return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, target, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
                }
                else
                {
                    return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID, 0.45), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
                }
            }
        }

        public SingleWirelessCommand RunRoleStop(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            if (FreekickDefence.testDefenceState)
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallState;
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID, 0.65), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1.9, false);
        }

        public SingleWirelessCommand LongStop(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {

            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, ballState.Location + (GameParameters.OurGoalCenter - ballState.Location).GetNormalizeToCopy(1), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, true, true, 0, 0, 2);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        private Position2D GetTarget(WorldModel Model, int RobotID, double StopDistFromBall)
        {
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
            ballState = Model.BallState;
            if (Model.Status == GameStatus.Stop)
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
                            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .42, StopDistFromBall);

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
            return target;
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
            return GetTarget(Model, RobotID, 0.65).DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>();
            res.Add(new BusStopRole1());
            res.Add(new ActiveRole());
            //res.Add(new NewDefenderCornerRole1());
            //res.Add(new NewDefenderCornerRole2());
            //res.Add(new NewDefenderCornerRole3());
            //res.Add(new NewDefenderCornerRole4());
            //res.Add(new NewDefenderCornerRole5());
            //if (!FreekickDefence.BallIsMoved)
            //{
            //    res.Add(new NewDefenderCornerRole1());
            //    res.Add(new NewDefenderCornerRole2());
            //    res.Add(new NewDefenderCornerRole3());
            //    res.Add(new NewDefenderCornerRole4());
            //    res.Add(new NewDefenderCornerRole5());
            //}
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
