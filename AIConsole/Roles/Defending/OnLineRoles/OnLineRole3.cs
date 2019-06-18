using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using Enterprise;


namespace MRL.SSL.AIConsole.Roles
{
    class OnLineRole3 : RoleBase
    {



        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID)
        {
            Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalLeft, GameParameters.OurGoalRight, 0.33);

            Line right = new Line(GameParameters.OurGoalRight, Model.BallState.Location);
            Line left = new Line(GameParameters.OurGoalLeft.Extend(0, 0), Model.BallState.Location);
            Line intevallToBall = new Line(Position2D.Interpolate(right.Head, left.Head, 0.5), Model.BallState.Location , new Pen(Color.Yellow , 0.02f));

            DrawingObjects.AddObject(intevallToBall);
            double distToPenaltyAreaThreshold = 0.00;
            double RectWidth = 1.50;
            double RectHeight = 0.90;
            Line l1 = new Line(GameParameters.OurGoalLeft.Extend(-RectWidth, RectHeight + distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(0, RectHeight + distToPenaltyAreaThreshold));
            Line l2 = new Line(GameParameters.OurGoalRight.Extend(-RectWidth - distToPenaltyAreaThreshold, -RectHeight - distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(-RectWidth - distToPenaltyAreaThreshold, RectHeight + distToPenaltyAreaThreshold));
            Line l3 = new Line(GameParameters.OurGoalRight.Extend(-RectWidth - distToPenaltyAreaThreshold, -RectHeight - distToPenaltyAreaThreshold), GameParameters.OurGoalRight.Extend(0, -RectHeight - distToPenaltyAreaThreshold));
            Position2D centerRobot = new Position2D();
            DrawingObjects.AddObject(intevallToBall);
            DrawingObjects.AddObject(l1);
            DrawingObjects.AddObject(l2);
            DrawingObjects.AddObject(l3);
            if (GameParameters.SegmentIntersect(intevallToBall, l1).HasValue) // left
            {
                if (!IsInOurDangerZone(Model.BallState.Location) && GameParameters.IsInField(Model.BallState.Location, 0))
                {
                    centerRobot = l1.IntersectWithLine(intevallToBall).Value;
                }

            }
            else if (GameParameters.SegmentIntersect(intevallToBall, l3).HasValue) //right
            {
                if (!IsInOurDangerZone(Model.BallState.Location) && GameParameters.IsInField(Model.BallState.Location, 0))
                {
                    centerRobot = l3.IntersectWithLine(intevallToBall).Value;
                }

            }
            else //top
            {
                if (!IsInOurDangerZone(Model.BallState.Location) && GameParameters.IsInField(Model.BallState.Location, 0))
                {
                    centerRobot = l2.IntersectWithLine(intevallToBall).Value;
                }
                else
                    centerRobot = Model.OurRobots[RobotID].Location;


            }
            var angle = (Model.BallState.Location - centerRobot).AngleInDegrees;
            NormalSharedState.CommonInfo.OnlineRole3Target = centerRobot;

            Planner.Add(RobotID, centerRobot, angle, PathType.UnSafe, false, true, true, true, false);

        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(NormalSharedState.CommonInfo.OnlineRole3Target);
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new OnLineRole1(), new OnLineRole2(), new OnLineRole3() };

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
    }
}
