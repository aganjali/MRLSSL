using MRL.SSL.AIConsole.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Skills;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class palcment8 : RoleBase
    {
        Position2D p;
        int tempState = 0;

        //TODO: Shit hack for calculate cost
        static bool temp = true;
        static bool right = true;
        public Position2D target = new Position2D();
        public Position2D Target = new Position2D();
        public static Position2D targetOverLap8;
        public Position2D TargetFainal = new Position2D();
        public SingleObjectState ballState = new SingleObjectState();
        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID)
        {
            int angle = 180;
            Position2D target = CalculateTarget(Model, robotID);
            Planner.Add(robotID, target, angle, PathType.UnSafe, false, true, true, true, false);

            //if (CurrentState == (int)PlayMode.Attack)
            //{


            //    Planner.Add(robotID, target, 180, PathType.UnSafe, false, true, true, true, false);

            //}

            //else if (CurrentState == (int)PlayMode.Defence)
            //{


            //    Planner.Add(robotID, target, 180, PathType.UnSafe, false, true, true, true, false);

            //}
        }




        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);
            var tar = CalculateTarget(Model, RobotID);
            double d = Model.OurRobots[RobotID].Location.DistanceFrom(tar);
            return d * d;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

            if (Model.BallState.Location.X > 0.5 && CurrentState == (int)PlayMode.Attack)
            {
                CurrentState = (int)PlayMode.Defence;
                tempState = CurrentState;

            }
            else if (Model.BallState.Location.X < -0.5 && CurrentState == (int)PlayMode.Defence)
            {
                CurrentState = (int)PlayMode.Attack;
                tempState = CurrentState;
            }
            else
                CurrentState = tempState;
        }
        private int Calculateangle(WorldModel Model, int RobotID)
        {

            return 1;
        }
        private Position2D CalculateTarget(WorldModel Model, int RobotID)
        {
            var st1ID = FreekickDefence.Static1ID;
            var st2ID = FreekickDefence.Static2ID;
            Position2D target = new Position2D();
            if (CurrentState == (int)PlayMode.Attack)
            {

                target = new Position2D(6 + (Model.BallState.Location.X), (Model.BallState.Location.Y) / 3);
                if (target.X > 3)
                {
                    target = new Position2D(3, (Model.BallState.Location.Y) / 3);
                }

            }

            else if (CurrentState == (int)PlayMode.Defence)
            {
                //Hysteresis
                if (Model.BallState.Location.Y > 0.1 && right)
                {
                    right = false;
                    temp = right;
                }
                else if (Model.BallState.Location.Y < -0.1 && !right)
                {
                    right = true;
                    temp = right;
                }
                else
                    right = temp;


                Dictionary<int, SingleObjectState> rightOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y < 0).ToDictionary(o => o.Key, o => o.Value);
                Dictionary<int, SingleObjectState> leftOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y > 0).ToDictionary(o => o.Key, o => o.Value);

                if (right) //Gerrard position when ball is in right side
                {
                    if (leftOpps.Count > 0)
                    {
                        double minDistRobot = double.MaxValue;
                        int minDistId = 0;
                        foreach (var item in leftOpps)
                        {
                            if (item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter) < minDistRobot)
                            {
                                minDistRobot = item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter);
                                minDistId = item.Key;
                            }
                        }
                        if (!IsInOurDangerZone(Model.Opponents[minDistId].Location))
                        {

                            target = GetSkill<MarkSkill>().OnDangerZoneMark(RobotID, Model, Model.Opponents[minDistId].Location);
                            Vector2D v = new Vector2D();
                            if (st2ID.HasValue)
                            {
                                Position2D st2 = Model.OurRobots[st2ID.Value].Location;
                                v = target - Model.OurRobots[st2ID.Value].Location;
                                if (target.Y < st2.Y + 0.2)
                                {
                                    target = new Position2D(Model.OurRobots[RobotID].Location.X, st2.Y + 0.20);
                                }
                            }
                            else
                            {
                            }
                        }
                        else
                            target = Model.OurRobots[RobotID].Location;

                    }
                    else
                    {
                        target = MarkSkill.ourDangerZoneLeftCorner + (MarkSkill.ourDangerZoneLeftCorner - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.10);

                    }
                }
                else //Gerrard position when ball is in left side
                {
                    if (rightOpps.Count > 0)
                    {
                        double minDistRobot = double.MaxValue;
                        int minDistId = 0;
                        foreach (var item in rightOpps)
                        {
                            if (item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter) < minDistRobot)
                            {
                                minDistRobot = item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter);
                                minDistId = item.Key;
                            }
                        }
                        if (!IsInOurDangerZone(Model.Opponents[minDistId].Location))
                        {
                            target = GetSkill<MarkSkill>().OnDangerZoneMark(RobotID, Model, Model.Opponents[minDistId].Location);
                            Vector2D v = new Vector2D();
                            if (st1ID.HasValue)
                            {
                                Position2D st1 = Model.OurRobots[st1ID.Value].Location;
                                v = target - Model.OurRobots[st1ID.Value].Location;
                                if (Math.Abs(target.Y) < Math.Abs(st1.Y) + 0.2)
                                {
                                    target = new Position2D(Model.OurRobots[RobotID].Location.X, st1.Y - 0.20);
                                }
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            target = Model.OurRobots[RobotID].Location;

                        }


                    }
                    else
                    {
                        DrawingObjects.AddObject(new Circle(MarkSkill.ourDangerZoneRightCorner, 0.1, new Pen(Color.Red, 0.01f)));
                        target = MarkSkill.ourDangerZoneRightCorner + (MarkSkill.ourDangerZoneRightCorner - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.10);

                    }

                }

            }

            double distfromborder, dist;
            double teta = 180;
            if ((Model.Status == GameStatus.BallPlace_Opponent || Model.Status == GameStatus.BallPlace_OurTeam) && (GameParameters.IsInDangerousZone(ballState.Location, false, .5, out dist, out distfromborder) || GameParameters.IsInDangerousZone(StaticVariables.ballPlacementPos, false, .5, out dist, out distfromborder))/* &&(GameParameters.OurGoalCenter.DistanceFrom(StaticVariables.ballPlacementPos) < 1 || GameParameters.OurGoalCenter.DistanceFrom(Model.BallState.Location) < 1)*/)
            {
                Line Stop1 = new Line(GameParameters.OurGoalCenter.Extend(0, -.3), GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3)));
                Circle Circl = new Circle(ballState.Location, .5);
                target = Circl.Intersect(Stop1).OrderBy(t => t.DistanceFrom(GameParameters.OurGoalCenter)).First();
                TargetFainal = target;
                DrawingObjects.AddObject(new Circle(target, 0.04, new Pen(Color.Magenta, 0.01f)), "ugfuys");

                DrawingObjects.AddObject(new StringDraw("target1= " + target, new Position2D(4, 5)), "target1");
                Position2D p1, p2;
                double dist1, dist2;
                Line ll = new Line();
                Line line1 = new Line();
                ll = new Line(StaticVariables.ballPlacementPos, Model.BallState.Location);
                // DrawingObjects.AddObject(new Line(StaticVariables.ballPlacementPos, Model.BallState.Location, new Pen(Color.BlueViolet, 0.01f)), "l13");

                Position2D p = (ll).PerpenducilarLineToPoint(target).IntersectWithLine(ll).Value;
                // DrawingObjects.AddObject(new Circle(p, 0.04, new Pen(Color.HotPink, 0.01f)), "p1");

                Line l2 = new Line(target, p);
                // DrawingObjects.AddObject(new Line(target, p, new Pen(Color.BlueViolet, 0.01f)), "l21");


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
                        // DrawingObjects.AddObject(new Circle(Target, 0.04, new Pen(Color.Khaki, 0.01f)), "cTargbgfet1");
                        line1 = new Line(target, Target);
                        List<Position2D> pos = cTarget.Intersect(line1);
                        p1 = pos.First();
                        p2 = pos.Last();
                        // DrawingObjects.AddObject(new Circle(p1, 0.04, new Pen(Color.White, 0.01f)), "cTarfhgetr1");
                        // DrawingObjects.AddObject(new Circle(p2, 0.04, new Pen(Color.Red, 0.01f)), "cTargexfgtt1");
                        dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                        dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                        if (dist1 < dist2)
                        {
                            TargetFainal = p1;
                        }
                        else
                        {
                            TargetFainal = p2;
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
                        // DrawingObjects.AddObject(new Circle(Target, 0.04, new Pen(Color.Khaki, 0.01f)), "fgchh");
                        line1 = new Line(target, Target);
                        List<Position2D> pos = cTarget.Intersect(line1);
                        p1 = pos.First();
                        p2 = pos.Last();

                        dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                        dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                        if (dist1 < dist2)
                        {
                            TargetFainal = p1;
                        }
                        else
                        {
                            TargetFainal = p2;
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
                    if (p.DistanceFrom(Model.BallState.Location) > 0.65)
                    {
                        TargetFainal = p;
                    }
                    else
                    {
                        Circle cTarget = new Circle(Model.BallState.Location, .65);
                        //DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.04, new Pen(Color.White, 0.01f)), "cTargfghfet1");
                        line1 = new Line(p, Model.BallState.Location);
                        List<Position2D> pos = cTarget.Intersect(line1);
                        p1 = pos.First();
                        p2 = pos.Last();

                        dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                        dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                        if (dist1 < dist2)
                        {
                            TargetFainal = p1;
                        }
                        else
                        {
                            TargetFainal = p2;
                        }
                    }
                    if (p.DistanceFrom(StaticVariables.ballPlacementPos) > 0.65)
                    {
                        TargetFainal = p;
                    }
                    else
                    {
                        Circle cTarget = new Circle(StaticVariables.ballPlacementPos, .65);
                        //DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.04, new Pen(Color.White, 0.01f)), "cTarfhgfhget1");
                        line1 = new Line(p, StaticVariables.ballPlacementPos);
                        List<Position2D> pos = cTarget.Intersect(line1);
                        p1 = pos.First();
                        p2 = pos.Last();

                        dist1 = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
                        dist2 = Model.OurRobots[RobotID].Location.DistanceFrom(p2);
                        if (dist1 < dist2)
                        {
                            TargetFainal = p1;
                        }
                        else
                        {
                            TargetFainal = p2;
                        }
                    }
                }
            }
            else if ((Model.Status == GameStatus.BallPlace_Opponent || Model.Status == GameStatus.BallPlace_OurTeam))
            {
                //Line Stop1 = new Line(GameParameters.OurGoalCenter.Extend(0, -.3), GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3)));
                //Vector2D jdh = ((GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3))) - GameParameters.OurGoalCenter.Extend(0, -.3));
                //Vector2D jh = Vector2D.FromAngleSize(((GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3))) - GameParameters.OurGoalCenter.Extend(0, -.3)).AngleInDegrees, 0.75);
                //target = jh + GameParameters.OurGoalCenter.Extend(0, -.3);
                //TargetFainal = target;
                Line Stop1 = new Line(GameParameters.OurGoalCenter.Extend(0, -.3), GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3)));
                Vector2D jdh = ((GameParameters.OurGoalCenter.Extend(0, -.3) + (ballState.Location - GameParameters.OurGoalCenter.Extend(0, -.3))) - GameParameters.OurGoalCenter.Extend(0, -.3));
                Position2D posOnDangerzon = GameParameters.LineIntersectWithOurDangerZone(Stop1).First();
                posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                target = posOnDangerzon;
                TargetFainal = target;
            }
            targetOverLap8 = TargetFainal;
            return TargetFainal;

        }
        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new GerrardRole(), new StaticDefender1(), new StaticDefender2() };
        }

        public bool IsInOurDangerZone(Position2D pos)
        {
            if (pos.X > 4.8 && pos.Y < 1.2 && pos.Y > -1.2)
            {
                return true;
            }
            else
                return false;
        }

        enum PlayMode
        {
            Defence,
            Attack
        }
        enum Side
        {
            right,
            left
        }
    }
}
