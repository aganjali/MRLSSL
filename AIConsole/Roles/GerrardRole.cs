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
    class GerrardRole : RoleBase
    {
        Position2D p;
        int tempState = 0;

        //TODO: Shit hack for calculate cost
        static bool temp = true;
        static bool right = true;

        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID)
        {
            double angle = 180;

            Position2D target = CalculateTarget(Model, robotID, out angle);
            Planner.Add(robotID, target, angle, PathType.UnSafe, false, true, true, true, false);


        }




        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);
            double angle = 0;
            var tar = CalculateTarget(Model, RobotID, out angle);
            double d = Model.OurRobots[RobotID].Location.DistanceFrom(tar);
            return d * d;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            int? st1ID = FreekickDefence.Static1ID;
            int? st2ID = FreekickDefence.Static2ID;
        

            if (st1ID.HasValue && st2ID.HasValue && (Model.OurRobots[st1ID.Value].Location.X > 5.1 || Model.OurRobots[st2ID.Value].Location.X > 5.1))
            {

                CurrentState = (int)PlayMode.SingleDefender;
                tempState = CurrentState;

            }

            else if (Model.BallState.Location.X > 0.1 && (CurrentState == (int)PlayMode.Defence || CurrentState == (int)PlayMode.SingleDefender))
            {
                CurrentState = (int)PlayMode.Defence;
                tempState = CurrentState;

            }
            else if (Model.BallState.Location.X < -0.1 && (CurrentState == (int)PlayMode.Defence || CurrentState == (int)PlayMode.SingleDefender))
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
        private Position2D CalculateTarget(WorldModel Model, int robotID, out double angle)
        {
            angle = 180;
            var st1ID = FreekickDefence.Static1ID;
            var st2ID = FreekickDefence.Static2ID;
            Position2D target = new Position2D();
            Vector2D v = new Vector2D();

            if (CurrentState == (int)PlayMode.SingleDefender)
            {

                Dictionary<int, SingleObjectState> Opps = Model.Opponents.Where(o => o.Value.Location.X > -0.5 &&  o.Value.Location.X < 4 
                && o.Value.Location.Y < 3 && o.Value.Location.Y > -3
                ).ToDictionary(o => o.Key, o => o.Value);
                Dictionary<int, double> oppsDist = new Dictionary<int, double>();
                double minDistRobot = double.MaxValue;
                int? minDistId = null;
                foreach (var item in Opps)
                {
                    if (item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter) < minDistRobot)
                    {
                        minDistRobot = item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter);
                        minDistId = item.Key;
                    }
                }
                var a = minDistId;
                if (minDistId.HasValue)
                {
                    if (Opps.Count > 0)
                    {
                        Position2D st2 = Model.OurRobots[st2ID.Value].Location;
                        v = target - Model.OurRobots[st2ID.Value].Location;
                        if (target.X > st2.X - 0.25)
                        {
                            if (Model.Opponents[minDistId.Value].Location.Y > 0)
                            {
                                target = new Position2D(st2.X - 0.25, Model.OurRobots[robotID].Location.Y);
                            }
                            else if (Model.Opponents[minDistId.Value].Location.Y < 0)
                            {
                                target = new Position2D(st2.X - 0.25, Model.OurRobots[robotID].Location.Y);
                            }
                        }
                    }
                    target = GetSkill<MarkSkill>().OnDangerZoneMark(robotID, Model, Model.Opponents[minDistId.Value].Location);
                    angle = (Model.Opponents[minDistId.Value].Location - Model.OurRobots[robotID].Location).AngleInDegrees;


                }
                else
                {
                    target = GameParameters.OurGoalCenter.Extend(-1.7, 0);
                }



            }
            else if (CurrentState == (int)PlayMode.Attack)
            {

                target = new Position2D(6 + (Model.BallState.Location.X), (Model.BallState.Location.Y) / 3);
                if (target.X > 3)
                {
                    target = new Position2D(3, (Model.BallState.Location.Y) / 3);
                }
                Planner.Add(robotID, target, 180, PathType.UnSafe, false, true, true, true, false);

            }
            else if (CurrentState == (int)PlayMode.Defence)
            {
                Dictionary<int, SingleObjectState> rightOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y < 0).ToDictionary(o => o.Key, o => o.Value);
                Dictionary<int, SingleObjectState> leftOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y > 0).ToDictionary(o => o.Key, o => o.Value);


                if (Model.BallState.Location.Y <= 0) //Gerrard position when ball is in right side
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
                        if (!IsInOurDangerZone(Model.Opponents[minDistId].Location) && !IsInOurDangerZone(Model.BallState.Location))
                        {
                            target = GetSkill<MarkSkill>().OnDangerZoneMark(robotID, Model, Model.Opponents[minDistId].Location);
                            angle = (Model.Opponents[minDistId].Location - Model.OurRobots[robotID].Location).AngleInDegrees;

                            if (st1ID.HasValue)
                            {
                                Position2D st1 = Model.OurRobots[st1ID.Value].Location;
                                v = target - Model.OurRobots[st1ID.Value].Location;
                                if (target.Y < st1.Y + 0.25)
                                {
                                    target = new Position2D(Model.OurRobots[robotID].Location.X, st1.Y + 0.25);
                                }
                            }
                        }
                        else
                        {
                            target = new Position2D(3, (Model.BallState.Location.Y) / 3);

                        }

                    }
                    else
                    {
                        target = MarkSkill.ourDangerZoneLeftCorner + (MarkSkill.ourDangerZoneLeftCorner - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.10);

                    }
                }
                else//Gerrard position when ball is in left side
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
                        if (!IsInOurDangerZone(Model.Opponents[minDistId].Location) && !IsInOurDangerZone(Model.BallState.Location))
                        {
                            target = GetSkill<MarkSkill>().OnDangerZoneMark(robotID, Model, Model.Opponents[minDistId].Location);
                            angle = (Model.Opponents[minDistId].Location - Model.OurRobots[robotID].Location).AngleInDegrees;

                            if (st2ID.HasValue)
                            {
                                Position2D st2 = Model.OurRobots[st2ID.Value].Location;
                                v = target - Model.OurRobots[st2ID.Value].Location;
                                if (target.Y > st2.Y - 0.25)
                                {
                                    target = new Position2D(Model.OurRobots[robotID].Location.X, st2.Y - 0.25);
                                }
                            }
                        }
                        else
                        {
                            target = new Position2D(3, (Model.BallState.Location.Y) / 3);

                        }
                    }
                    else
                    {
                        DrawingObjects.AddObject(new Circle(MarkSkill.ourDangerZoneRightCorner, 0.1, new Pen(Color.Red, 0.01f)));
                        target = MarkSkill.ourDangerZoneRightCorner + (MarkSkill.ourDangerZoneRightCorner - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.10);
                        //target = Position2D.Zero + (Position2D.Zero - new Position2D(2,2));
                    }
                }

            }
            return target;

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
            Attack,
            SingleDefender
        }
        enum Side
        {
            right,
            left
        }
    }
}
//if (CurrentState == (int)PlayMode.Attack)
//{

//    target = new Position2D(6 + (Model.BallState.Location.X), (Model.BallState.Location.Y) / 3);
//    if (target.X > 3)
//    {
//        target = new Position2D(3, (Model.BallState.Location.Y) / 3);
//    }

//}

//else if (CurrentState == (int)PlayMode.Defence)
//{
//    //Hysteresis
//    if (Model.BallState.Location.Y > 0.1 && right)
//    {
//        right = false;
//        temp = right;
//    }
//    else if (Model.BallState.Location.Y < -0.1 && !right)
//    {
//        right = true;
//        temp = right;
//    }
//    else
//        right = temp;


//    Dictionary<int, SingleObjectState> rightOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y < 0).ToDictionary(o => o.Key, o => o.Value);
//    Dictionary<int, SingleObjectState> leftOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y > 0).ToDictionary(o => o.Key, o => o.Value);

//    if (right) //Gerrard position when ball is in right side
//    {
//        if (leftOpps.Count > 0)
//        {
//            double minDistRobot = double.MaxValue;
//            int minDistId = 0;
//            foreach (var item in leftOpps)
//            {
//                if (item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter) < minDistRobot)
//                {
//                    minDistRobot = item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter);
//                    minDistId = item.Key;
//                }
//            }
//            if (!IsInOurDangerZone(Model.Opponents[minDistId].Location))
//            {

//                target = GetSkill<MarkSkill>().OnDangerZoneMark(RobotID, Model, Model.Opponents[minDistId].Location);
//                Vector2D v = new Vector2D();
//                if (st2ID.HasValue)
//                {
//                    Position2D st2 = Model.OurRobots[st2ID.Value].Location;
//                    v = target - Model.OurRobots[st2ID.Value].Location;
//                    if (target.Y < st2.Y + 0.25)
//                    {
//                        target = new Position2D(Model.OurRobots[RobotID].Location.X, st2.Y + 0.25);
//                    }
//                }
//                else
//                {
//                }
//            }
//            else
//                target = Model.OurRobots[RobotID].Location;

//        }
//        else
//        {
//            target = MarkSkill.ourDangerZoneLeftCorner + (MarkSkill.ourDangerZoneLeftCorner - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.10);

//        }
//    }
//    else //Gerrard position when ball is in left side
//    {
//        if (rightOpps.Count > 0)
//        {
//            double minDistRobot = double.MaxValue;
//            int minDistId = 0;
//            foreach (var item in rightOpps)
//            {
//                if (item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter) < minDistRobot)
//                {
//                    minDistRobot = item.Value.Location.DistanceFrom(GameParameters.OurGoalCenter);
//                    minDistId = item.Key;
//                }
//            }
//            if (!IsInOurDangerZone(Model.Opponents[minDistId].Location))
//            {
//                target = GetSkill<MarkSkill>().OnDangerZoneMark(RobotID, Model, Model.Opponents[minDistId].Location);
//                Vector2D v = new Vector2D();
//                if (st1ID.HasValue)
//                {
//                    Position2D st1 = Model.OurRobots[st1ID.Value].Location;
//                    v = target - Model.OurRobots[st1ID.Value].Location;
//                    if (Math.Abs(target.Y) < Math.Abs(st1.Y) + 0.25)
//                    {
//                        target = new Position2D(Model.OurRobots[RobotID].Location.X, st1.Y - 0.25);
//                    }
//                }
//                else
//                {
//                }
//            }
//            else
//            {
//                target = Model.OurRobots[RobotID].Location;

//            }


//        }
//        else
//        {
//            DrawingObjects.AddObject(new Circle(MarkSkill.ourDangerZoneRightCorner, 0.1, new Pen(Color.Red, 0.01f)));
//            target = MarkSkill.ourDangerZoneRightCorner + (MarkSkill.ourDangerZoneRightCorner - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.10);

//        }

//    }
//}