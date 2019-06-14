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
        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID)
        {
             if (CurrentState == (int)PlayMode.Attack)
            {

                p = new Position2D(6 + (Model.BallState.Location.X), (Model.BallState.Location.Y) / 3);
                if (p.X > 3)
                {
                    p = new Position2D(3, (Model.BallState.Location.Y) / 3);
                }
                Planner.Add(robotID, p, 180, PathType.UnSafe, false, true, true, true, false);

            }
            else if (CurrentState == (int)PlayMode.Defence)
            {
                Dictionary<int, SingleObjectState> rightOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y < 0).ToDictionary(o => o.Key, o => o.Value);
                Dictionary<int, SingleObjectState> leftOpps = Model.Opponents.Where(o => o.Value.Location.X > 0 && o.Value.Location.Y > 0).ToDictionary(o => o.Key, o => o.Value);

                Position2D target = new Position2D();
                if (Model.BallState.Location.Y <= 0 ) //Gerrard position when ball is in right side
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
                        target = GetSkill<MarkSkill>().OnDangerZoneMark(robotID , Model , Model.Opponents[minDistId].Location);
                      
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
                        target = GetSkill<MarkSkill>().OnDangerZoneMark(robotID, Model, Model.Opponents[minDistId].Location);
                        
                    }
                    else
                    {
                        DrawingObjects.AddObject(new Circle(MarkSkill.ourDangerZoneRightCorner, 0.1, new Pen(Color.Red, 0.01f)));
                        target = MarkSkill.ourDangerZoneRightCorner + (MarkSkill.ourDangerZoneRightCorner - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.10);
                        //target = Position2D.Zero + (Position2D.Zero - new Position2D(2,2));
                    }
                }

                Planner.Add(robotID, target, 180, PathType.UnSafe, false, true, true, true, false);

            }
        }




        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 1;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (Model.BallState.Location.X < 0)
            {
                CurrentState = (int)PlayMode.Attack;
            }
            else if (Model.BallState.Location.X > 0)
            {
                CurrentState = (int)PlayMode.Defence;
            }
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
            return new List<RoleBase>() { new PathTestRole() };
        }
        enum PlayMode
        {
            Attack,
            Defence
        }
    }
}
