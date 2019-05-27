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
    class palcment7 : RoleBase
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
            Planner.SetParameter(RobotID, 1, 0.7);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID, ballState), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1, true);
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
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID, ballState), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1.9, false);
        }

        private Position2D GetTarget(WorldModel Model, int RobotID, SingleObjectState ballfakepos)
        {
            Line ourGoalBall = new Line();
            Line line1 = new Line();
            Line line2 = new Line();
            Line line11 = new Line();
            Line line22 = new Line();
            Line ll = new Line();
            Position2D Target = new Position2D();
            Position2D tar = new Position2D();
            Position2D target = new Position2D();
            double StopDistFromBall = .65;


            Position2D ball = ballState.Location;
            if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.4)
            {
                target = new Position2D(1.8, .5);
            }
            else
            {
                target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 1, StopDistFromBall);
                //if (ball.X > 2.5 && ball.Y > 1.5)
                //{
                //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 0.8, StopDistFromBall);
                //}
            }

            #region Ballpalcement
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
            ourGoalBall = new Line(GameParameters.OurGoalCenter, StaticVariables.ballPlacementPos);
            #region Debug
            if (false)
            {
                line1.DrawPen = new Pen(Color.Red, 0.02f);
                line2.DrawPen = new Pen(Color.Blue, 0.02f);
                DrawingObjects.AddObject(line1);
                DrawingObjects.AddObject(line2);
                line11.DrawPen = new Pen(Color.Black, 0.02f);
                line22.DrawPen = new Pen(Color.Pink, 0.02f);
                DrawingObjects.AddObject(line11);
                DrawingObjects.AddObject(line22);
                DrawingObjects.AddObject(new Circle(Model.BallState.Location + exVec22, 0.04, new Pen(Color.Blue, 0.01f)), "ballcirc");
                DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos + exVec222, 0.04, new Pen(Color.Black, 0.01f)), "ballcircl");
            }
            #endregion
            #endregion
            tar = target;
            //DrawingObjects.AddObject(new StringDraw("target7= " + target, new Position2D(3.5, 5)), "target7");
            Position2D p1, p2;
            double dist1, dist2;

            ll = new Line(StaticVariables.ballPlacementPos, Model.BallState.Location);
            // DrawingObjects.AddObject(new Line(StaticVariables.ballPlacementPos, Model.BallState.Location, new Pen(Color.BlueViolet, 0.01f)), "l17");

            Position2D p = (ll).PerpenducilarLineToPoint(target).IntersectWithLine(ll).Value;
            //DrawingObjects.AddObject(new Circle(p, 0.04, new Pen(Color.HotPink, 0.01f)), "p7");

            Line l2 = new Line(target, p);
            //DrawingObjects.AddObject(new Line(target, p, new Pen(Color.BlueViolet, 0.01f)), "l27");


            if (Model.BallState.Location.X > StaticVariables.ballPlacementPos.X)
            {
                if (p.X < Model.BallState.Location.X && p.X > StaticVariables.ballPlacementPos.X)
                {
                    Target.X = p.X;
                    //DrawingObjects.AddObject(new StringDraw("Target.Xb7= " + Target.X, new Position2D(4, 5)), "Target.Xb7");
                    if (Model.BallState.Location.Y > StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y < Model.BallState.Location.Y && p.Y > StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            //DrawingObjects.AddObject(new StringDraw("Target.Yb7= " + Target.Y, new Position2D(4.5, 5)), "Target.Yb7");
                        }
                    }
                    else if (Model.BallState.Location.Y < StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y > Model.BallState.Location.Y && p.Y < StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            //DrawingObjects.AddObject(new StringDraw("Target.Yp7= " + Target.Y, new Position2D(4.5, 5)), "Target.Yp7");
                        }
                    }
                }
                if (target.DistanceFrom(Target) < 0.65)
                {
                    Circle cTarget = new Circle(Target, .65);
                    // DrawingObjects.AddObject(new Circle(Target, 0.65, new Pen(Color.Khaki, 0.01f)), "cTarget7");
                    line1 = new Line(target, Target);
                    //DrawingObjects.AddObject(new Line(target, Target, new Pen(Color.BlueViolet, 0.01f)), "target7");
                    List<Position2D> pos = cTarget.Intersect(line1);
                    p1 = pos.First();
                    p2 = pos.Last();
                    //DrawingObjects.AddObject(new Circle(p1, 0.04, new Pen(Color.White, 0.01f)), "cTargetr7");
                    //DrawingObjects.AddObject(new Circle(p2, 0.04, new Pen(Color.Red, 0.01f)), "cTargett7");
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
                    // DrawingObjects.AddObject(new StringDraw("Target.Xp7= " + Target.X, new Position2D(4, 5)), "Target.Xp7");
                    if (Model.BallState.Location.Y > StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y < Model.BallState.Location.Y && p.Y > StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            //DrawingObjects.AddObject(new StringDraw("Target.Yb7= " + Target.Y, new Position2D(4.5, 5)), "Target.Yb7");
                        }
                    }
                    else if (Model.BallState.Location.Y < StaticVariables.ballPlacementPos.Y)
                    {
                        if (p.Y > Model.BallState.Location.Y && p.Y < StaticVariables.ballPlacementPos.Y)
                        {
                            Target.Y = p.Y;
                            // DrawingObjects.AddObject(new StringDraw("Target.Yp7= " + Target.Y, new Position2D(4.5, 5)), "Target.Yp7");
                        }
                    }
                }
                if (target.DistanceFrom(Target) < 0.65)
                {
                    Circle cTarget = new Circle(Target, .65);
                    // DrawingObjects.AddObject(new Circle(Target, 0.04, new Pen(Color.Khaki, 0.01f)), "cTarget7");
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
                }
            }
            else
            {
                if (p.DistanceFrom(Model.BallState.Location) > 0.65)
                {
                    tar = p;
                }
                else
                {
                    Circle cTarget = new Circle(Model.BallState.Location, .65);
                    // DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.04, new Pen(Color.White, 0.01f)), "cTarget7");
                    line1 = new Line(p, Model.BallState.Location);
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
                if (p.DistanceFrom(StaticVariables.ballPlacementPos) > 0.65)
                {
                    tar = p;
                }
                else
                {
                    Circle cTarget = new Circle(StaticVariables.ballPlacementPos, .65);
                    // DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.04, new Pen(Color.White, 0.01f)), "cTarget7");
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
            DrawingObjects.AddObject(new StringDraw("target7= " + tar, new Position2D(4, 5)), "target7");
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
            foreach (var item in ourRobots)
            {
                if (item.Value.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .25)
                {
                    Vector2D OGCandRobot = GameParameters.OurGoalCenter - item.Value.Location;
                    Vector2D extendtempvec = Model.OurRobots[RobotID].Location - item.Value.Location;
                    extendpoint = item.Value.Location + extendtempvec.GetNormalizeToCopy(.25);
                    if (OGCandRobot.InnerProduct(extendtempvec) > 0)
                        extendpoint = Model.OurRobots[RobotID].Location - extendtempvec;
                    Vector2D ExtendPoint2 = extendpoint - ballState.Location;
                    ExtendPoint2.NormalizeTo(.6);
                    extendpoint = ballState.Location + ExtendPoint2;

                }
            }

            return extendpoint;
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new palcment7(), new ActiveRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return GetTarget(Model, RobotID, ballState).DistanceFrom(Model.OurRobots[RobotID].Location);
        }
    }
}
