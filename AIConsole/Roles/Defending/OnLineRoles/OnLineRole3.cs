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


        Position2D lastBallPos = new Position2D();
        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID)
        {
            Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalLeft, GameParameters.OurGoalRight, 0.33);

            Line right = new Line(GameParameters.OurGoalRight, Model.BallState.Location);
            Line left = new Line(GameParameters.OurGoalLeft.Extend(0, 0), Model.BallState.Location);

            double distToPenaltyAreaThreshold = 0.0;
            double RectWidth = 1.55;
            double RectHeight = 0.95;
            Line l1 = new Line(GameParameters.OurGoalLeft.Extend(-RectWidth, RectHeight + distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(0, RectHeight + distToPenaltyAreaThreshold));
            Line l2 = new Line(GameParameters.OurGoalRight.Extend(-RectWidth - distToPenaltyAreaThreshold, -RectHeight - distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(-RectWidth - distToPenaltyAreaThreshold, RectHeight + distToPenaltyAreaThreshold));
            Line l3 = new Line(GameParameters.OurGoalRight.Extend(-RectWidth - distToPenaltyAreaThreshold, -RectHeight - distToPenaltyAreaThreshold), GameParameters.OurGoalRight.Extend(0, -RectHeight - distToPenaltyAreaThreshold));
            Position2D centerRobot = new Position2D();
            DrawingObjects.AddObject(l1);
            DrawingObjects.AddObject(l2);
            DrawingObjects.AddObject(l3);

            if (GameParameters.IsInField(Model.BallState.Location, 0))
            {
                lastBallPos = Model.BallState.Location;
            }
            Line intevallToBall = new Line(Position2D.Interpolate(right.Head, left.Head, 0.5), lastBallPos , new Pen(Color.BlueViolet , 0.01f));
            DrawingObjects.AddObject(intevallToBall);
            //dance dance
            if (GameParameters.SegmentIntersect(intevallToBall, l1).HasValue) // left
            {
                if (!IsInOurDangerZone(lastBallPos))
                {
                    centerRobot = l1.IntersectWithLine(intevallToBall).Value;
                }
                else
                    centerRobot = lastBallPos.Extend( 0, 0.20);

            }
            else if (GameParameters.SegmentIntersect(intevallToBall, l3).HasValue) //right
            {
                if (!IsInOurDangerZone(lastBallPos))
                {
                    centerRobot = l3.IntersectWithLine(intevallToBall).Value;
                }
                else
                    centerRobot = lastBallPos.Extend( 0, -0.20);


            }
            else //top
            {
                if (!IsInOurDangerZone(lastBallPos))
                {
                    centerRobot = l2.IntersectWithLine(intevallToBall).Value;
                }
                else
                {
                    centerRobot = lastBallPos.Extend(-1.2, 0);

                }




            }
            if (OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
            {
                centerRobot = OppFreeKickDefenceUtils.Dive(engine, Model, RobotID);
            }
            DrawingObjects.AddObject(new Circle(centerRobot, 0.09, new Pen(Color.Purple, 0.01f)));

            var angle = (lastBallPos - Model.OurRobots[RobotID].Location).AngleInDegrees;
            NormalSharedState.CommonInfo.OnlineRole3Target = centerRobot;
            Planner.AddKick(RobotID, kickPowerType.Speed, true, 3);
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
