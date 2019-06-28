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
    class palcment3 : RoleBase
    {
        bool flag = true;
        public static Position2D targetOverLap3;
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
            Planner.SetParameter(RobotID, 1, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2, false, true, false, new List<Obstacle>() {
                    new Obstacle()
                    {
                        State = new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0),
                        R = new Vector2D(2, 4),
                        Type = ObstacleType.Rectangle
                    }
                });
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
            double StopDistFromBall = 0.7;
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
                return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2, false, true, false, new List<Obstacle>() {
                    new Obstacle()
                    {
                        State = new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0),
                        R = new Vector2D(2, 4),
                        Type = ObstacleType.Rectangle
                    }
                });
            }
            else
            {
                Vector2D ballOppVec = Model.BallState.Location - Model.Opponents[oppPasser].Location;
                ballOppVec.NormalizeTo(StopDistFromBall);
                Position2D target = Model.BallState.Location + ballOppVec;
                if (!GameParameters.IsInField(target, 0.01))
                    return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2, false, true, false, new List<Obstacle>() {
                    new Obstacle()
                    {
                        State = new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0),
                        R = new Vector2D(2, 4),
                        Type = ObstacleType.Rectangle
                    }
                });
                return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, target, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2, false, true, false, new List<Obstacle>() {
                    new Obstacle()
                    {
                        State = new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0),
                        R = new Vector2D(2, 4),
                        Type = ObstacleType.Rectangle
                    }
                });
            }

        }

        public SingleWirelessCommand RunRoleStop(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID, Position2D Target)
        {
            if (Model.Status == GameStatus.BallPlace_Opponent || Model.Status == GameStatus.BallPlace_OurTeam)
            {
                ballState = new SingleObjectState(StaticVariables.ballPlacementPos, new Vector2D(), 0);
                ballStateFast = new SingleObjectState(StaticVariables.ballPlacementPos, new Vector2D(), 0);
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            List<int> ourRobot = new List<int>();



            Line ourGoalBall = new Line();
            Line line1 = new Line();
            Line ll = new Line();
            Line line2 = new Line();
            Line line11 = new Line();
            Line line22 = new Line();
            Vector2D vec = StaticVariables.ballPlacementPos - Model.BallState.Location;
            Vector2D exVec1 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI / 2, 0.5);
            Vector2D exVec2 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, 0.5);
            Vector2D exVec11 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI, 0.5);
            Vector2D exVec22 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI, 0.5);
            Vector2D vecc = Model.BallState.Location - StaticVariables.ballPlacementPos;
            Vector2D exVec111 = Vector2D.FromAngleSize(vecc.AngleInRadians + Math.PI, 0.5);
            Vector2D exVec222 = Vector2D.FromAngleSize(vecc.AngleInRadians - Math.PI, 0.5);
            line11 = new Line(Model.BallState.Location + exVec1 + exVec11, Model.BallState.Location + exVec2 + exVec22);
            line22 = new Line(StaticVariables.ballPlacementPos + exVec1 + exVec111, StaticVariables.ballPlacementPos + exVec2 + exVec222);
            line1 = new Line(Model.BallState.Location + exVec1 + exVec11, StaticVariables.ballPlacementPos + exVec1 + exVec111);
            line2 = new Line(Model.BallState.Location + exVec2 + exVec22, StaticVariables.ballPlacementPos + exVec2 + exVec222);
            ll = new Line(StaticVariables.ballPlacementPos, Model.BallState.Location);
            ourGoalBall = new Line(GameParameters.OurGoalCenter, StaticVariables.ballPlacementPos);
            double distLine22, distLine2;
            distLine2 = (Model.BallState.Location + exVec2 + exVec22).DistanceFrom(StaticVariables.ballPlacementPos + exVec2 + exVec222);
            distLine22 = (StaticVariables.ballPlacementPos + exVec1 + exVec111).DistanceFrom(StaticVariables.ballPlacementPos + exVec2 + exVec222);


            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2, false, true, false, new List<Obstacle>() {
                    new Obstacle()
                    {
                        State = new SingleObjectState(new Position2D(distLine22/2, distLine2/2), Vector2D.Zero, 0),
                        R = new Vector2D(distLine22/2, distLine2/2),
                        Type = ObstacleType.Rectangle
                    }
                });
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
            Line ourGoalBall = new Line();
            Line line1 = new Line();
            Line ll = new Line();
            Line line2 = new Line();
            Line line11 = new Line();
            Line line22 = new Line();

            double StopDistFromBall = .65;
            Position2D target = new Position2D();
            Position2D posToGo = new Position2D();
            Position2D posToGo1 = new Position2D();
            Position2D Target = new Position2D();
            Position2D tar = new Position2D();
            Position2D DefenderStopRole1 = GameParameters.OurGoalCenter + new Vector2D(-1, -0.17);
            Position2D DefenderStopRole2 = GameParameters.OurGoalCenter + new Vector2D(-1, 0.17);
            Circle C = new Circle(ballState.Location, 0.7);
            double angle = Math.Sign(ballState.Location.Y) * 1 * Math.PI / 3;
            if (C.IsInCircle(DefenderStopRole1) || C.IsInCircle(DefenderStopRole2))
            {
                angle = Math.Sign(ballState.Location.Y) * 1.5 * Math.PI / 3;
            }
            Position2D ball = ballState.Location;

            if (flag)
            {
                if (Model.Status == GameStatus.Stop || Model.Status == GameStatus.BallPlace_Opponent || Model.Status == GameStatus.BallPlace_OurTeam)
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
            }
            tar = target;
            //DrawingObjects.AddObject(new Circle(target, 0.04, new Pen(Color.Yellow, 0.01f)), "ballcirc3");
            #region Debug
            //if (false)
            //{
            //    line1.DrawPen = new Pen(Color.Red, 0.02f);
            //    line2.DrawPen = new Pen(Color.Blue, 0.02f);
            //    DrawingObjects.AddObject(line1);
            //    DrawingObjects.AddObject(line2);
            //    line11.DrawPen = new Pen(Color.Black, 0.02f);
            //    line22.DrawPen = new Pen(Color.Pink, 0.02f);
            //    DrawingObjects.AddObject(line11);
            //    DrawingObjects.AddObject(line22);
            //    DrawingObjects.AddObject(new Circle(Model.BallState.Location + exVec22, 0.04, new Pen(Color.Blue, 0.01f)), "ballcirc");
            //    DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos + exVec222, 0.04, new Pen(Color.Black, 0.01f)), "ballcircl");
            //}
            #endregion
            DrawingObjects.AddObject(new StringDraw("target3= " + target, new Position2D(3.5, 5)), "target3");
            Position2D p1, p2;
            double dist1, dist2;

            ll = new Line(StaticVariables.ballPlacementPos, Model.BallState.Location);
            // DrawingObjects.AddObject(new Line(StaticVariables.ballPlacementPos, Model.BallState.Location, new Pen(Color.BlueViolet, 0.01f)), "l13");

            Position2D p = (ll).PerpenducilarLineToPoint(target).IntersectWithLine(ll).Value;
            //DrawingObjects.AddObject(new Circle(p, 0.04, new Pen(Color.HotPink, 0.01f)), "p3");

            Line l2 = new Line(target, p);
            //DrawingObjects.AddObject(new Line(target, p, new Pen(Color.BlueViolet, 0.01f)), "l23");


            if (Model.BallState.Location.X > StaticVariables.ballPlacementPos.X)
            {
                if (p.X < Model.BallState.Location.X && p.X > StaticVariables.ballPlacementPos.X)
                {
                    Target.X = p.X;
                    //DrawingObjects.AddObject(new StringDraw("Target.Xb3= " + Target.X, new Position2D(4, 5)), "Target.Xb3");
                    if (Model.BallState.Location.Y > StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y < Model.BallState.Location.Y && p.Y > StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            // DrawingObjects.AddObject(new StringDraw("Target.Yb3= " + Target.Y, new Position2D(4.5, 5)), "Target.Yb3");
                        }
                    }
                    else if (Model.BallState.Location.Y < StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y > Model.BallState.Location.Y && p.Y < StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            // DrawingObjects.AddObject(new StringDraw("Target.Yp3= " + Target.Y, new Position2D(4.5, 5)), "Target.Yp3");
                        }
                    }
                }
                if (target.DistanceFrom(Target) < 0.65)
                {
                    Circle cTarget = new Circle(Target, .65);
                    // DrawingObjects.AddObject(new Circle(Target, 0.04, new Pen(Color.Khaki, 0.01f)), "cTarget3");
                    line1 = new Line(target, Target);
                    List<Position2D> pos = cTarget.Intersect(line1);
                    p1 = pos.First();
                    p2 = pos.Last();
                    //DrawingObjects.AddObject(new Circle(p1, 0.04, new Pen(Color.White, 0.01f)), "cTargetr3");
                    //DrawingObjects.AddObject(new Circle(p2, 0.04, new Pen(Color.Red, 0.01f)), "cTargett3");
                    dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                    dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                    if (dist1 < dist2)
                    {
                        tar = p1;
                    }
                    else
                    {
                        tar = p2;
                    }
                }
            }
            else if (Model.BallState.Location.X < StaticVariables.ballPlacementPos.X)
            {
                if (p.X > Model.BallState.Location.X && p.X < StaticVariables.ballPlacementPos.X)
                {
                    Target.X = p.X;
                    // DrawingObjects.AddObject(new StringDraw("Target.Xp3= " + Target.X, new Position2D(4, 5)), "Target.Xp3");
                    if (Model.BallState.Location.Y > StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y < Model.BallState.Location.Y && p.Y > StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            //DrawingObjects.AddObject(new StringDraw("Target.Yb3= " + Target.Y, new Position2D(4.5, 5)), "Target.Yb3");
                        }
                    }
                    else if (Model.BallState.Location.Y < StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y > Model.BallState.Location.Y && p.Y < StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            // DrawingObjects.AddObject(new StringDraw("Target.Yp3= " + Target.Y, new Position2D(4.5, 5)), "Target.Yp3");
                        }
                    }
                }
                if (target.DistanceFrom(Target) < 0.65)
                {
                    Circle cTarget = new Circle(Target, .65);
                    // DrawingObjects.AddObject(new Circle(Target, 0.04, new Pen(Color.Khaki, 0.01f)), "cTarget3");
                    line1 = new Line(target, Target);
                    List<Position2D> pos = cTarget.Intersect(line1);
                    p1 = pos.First();
                    p2 = pos.Last();

                    dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                    dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                    if (dist1 < dist2)
                    {
                        tar = p1;
                    }
                    else
                    {
                        tar = p2;
                    }

                    //Circle cTarget = new Circle(Target, .65);
                    //DrawingObjects.AddObject(new Circle(Target, .65, new Pen(Color.Blue, 0.01f)), "Target");
                    //List<Position2D> pos = cTarget.Intersect(ll);
                    //p1 = pos.First();
                    //p2 = pos.Last();

                    //dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                    //dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                    //if (dist1 < dist2)
                    //{
                    //    tar = p1;
                    //}
                    //else
                    //{
                    //    tar = p2;
                    //}

                }
            }
            else
            {
                //if (p.DistanceFrom(Model.BallState.Location) > 0.65)
                //{
                //    tar = p;
                //}
                //else
                //{
                //    Circle cTarget = new Circle(Model.BallState.Location, .65);
                //    // DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.04, new Pen(Color.White, 0.01f)), "cTarget3");
                //    line1 = new Line(p, Model.BallState.Location);
                //    List<Position2D> pos = cTarget.Intersect(line1);
                //    p1 = pos.First();
                //    p2 = pos.Last();

                //    dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                //    dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                //    if (dist1 < dist2)
                //    {
                //        tar = p1;
                //    }
                //    else
                //    {
                //        tar = p2;
                //    }
                //}
                if (p.DistanceFrom(StaticVariables.ballPlacementPos) > 0.65)
                {
                    tar = p;
                }
                else
                {
                    Circle cTarget = new Circle(StaticVariables.ballPlacementPos, .65);
                    // DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.04, new Pen(Color.White, 0.01f)), "cTarget3");
                    line1 = new Line(p, StaticVariables.ballPlacementPos);
                    List<Position2D> pos = cTarget.Intersect(line1);
                    p1 = pos.First();
                    p2 = pos.Last();

                    dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                    dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                    if (dist1 < dist2)
                    {
                        tar = p1;
                    }
                    else
                    {
                        tar = p2;
                    }
                }
            }
            //else
            //{
            //    posToGo.X = 0.6;
            //}

            //else
            //{
            //    posToGo.Y = 0.6;
            //}



            targetOverLap3 = tar;
            DrawingObjects.AddObject(new Circle(tar, 0.06, new Pen(Color.Black, 0.01f)), "tar3");
            return tar;
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
            res.Add(new palcment3());
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
