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
    class OnLineRole1 : RoleBase
    {

        string CurState;
        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID)
        {
            double x = Model.BallState.Location.X;
            double y = Model.BallState.Location.Y;
            Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalRight, GameParameters.OurGoalLeft, 0.33);


            Position2D rightIntersect;
            Position2D leftIntersect;

            Line left = new Line(p1, Model.BallState.Location);
            Line right = new Line(GameParameters.OurGoalRight.Extend(0, 0), Model.BallState.Location);
            Line intevallToBall = new Line(Position2D.Interpolate(right.Head, left.Head, 0.5), Model.BallState.Location);
            double distToPenaltyAreaThreshold = 0.00;
            Line l1 = new Line(GameParameters.OurGoalLeft.Extend(-1.20, 0.60 + distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(0, 0.60 + distToPenaltyAreaThreshold));
            Line l2 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(-1.20 - distToPenaltyAreaThreshold, 0.6 + distToPenaltyAreaThreshold));
            Line l3 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalRight.Extend(0, -0.60 - distToPenaltyAreaThreshold));
            Position2D centerRobot = new Position2D();
            Position2D lastCenterRobot = new Position2D();
            DrawingObjects.AddObject(intevallToBall);
            DrawingObjects.AddObject(l1);
            //DrawingObjects.AddObject(l3);

            if (GameParameters.SegmentIntersect(intevallToBall, l1).HasValue) // left
            {
                if (!IsInOurDangerZone(Model.BallState.Location) && GameParameters.IsInField(Model.BallState.Location, 0))
                {
                    centerRobot = l1.IntersectWithLine(intevallToBall).Value;
                    lastCenterRobot = centerRobot;
                }

            }
            else if (GameParameters.SegmentIntersect(intevallToBall, l3).HasValue) //right
            {
                if (!IsInOurDangerZone(Model.BallState.Location) && GameParameters.IsInField(Model.BallState.Location, 0))
                {
                    centerRobot = l3.IntersectWithLine(intevallToBall).Value;
                    lastCenterRobot = centerRobot;
                }

            }
            else //top
            {
                if (!IsInOurDangerZone(Model.BallState.Location) && GameParameters.IsInField(Model.BallState.Location , 0))
                {
                    centerRobot = l2.IntersectWithLine(intevallToBall).Value;
                }
                else
                    centerRobot = Model.OurRobots[RobotID].Location;
                

            }

            Line robot = intevallToBall.PerpenducilarLineToPoint(centerRobot);
            rightIntersect = (Position2D)right.IntersectWithLine(robot);
            leftIntersect = (Position2D)left.IntersectWithLine(robot);
            double r = (rightIntersect - centerRobot).Size;
            double rprime = (leftIntersect - centerRobot).Size;
            double d = (rightIntersect - Model.BallState.Location).Size;
            double dprime = (leftIntersect - Model.BallState.Location).Size;

            double BallFrames = CalBallFrames(d);
            double RobotFrames = CalRobotFrames(r - 0.09);
            DrawingObjects.AddObject(right);

            DrawingObjects.AddObject(left);

            Vector2D v = new Vector2D();
            Position2D pos = new Position2D();

            if (r < rprime)
            {
                int diff = (int)(BallFrames - RobotFrames);
                if (diff < 0)
                    diff = 0;
                double rich = RichTheBall(diff);
                v = (leftIntersect - centerRobot).GetNormalizeToCopy(rich);
                CurState = "r < r prime";
                if (BallFrames < RobotFrames)
                {
                    CurState += " ballFrame < robotFrame";

                    pos = centerRobot;
                    //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, pos, new Pen(Color.White, 0.01f)), "Asgharnane");
                }
                else
                {
                    CurState += " ballFrame > robotFrame";
                    pos = centerRobot;

                }
            }
            else if (r > rprime)
            {
                int diff = (int)(BallFrames - RobotFrames);
                if (diff < 0)
                    diff = 0;
                double rich = RichTheBall(diff);
                v = (rightIntersect - centerRobot).GetNormalizeToCopy(rich);
                CurState = "r > rprime";
                if (BallFrames < RobotFrames)
                {
                    CurState += " ballFrame < robotFrame";
                    pos = centerRobot;
                    //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, pos, new Pen(Color.White, 0.1f)), "Asgharnane");
                }
                else
                {
                    CurState += " ballFrame > robotFrame";
                    pos = centerRobot;
                    //DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, pos, new Pen(Color.White, 0.1f)), "Asgharnane");


                }
            }
            var angle = (Model.BallState.Location - pos).AngleInDegrees;
            NormalSharedState.CommonInfo.OnlineRole1Target = pos;

            Planner.Add(RobotID, pos, angle, PathType.UnSafe, false, true, true, true, false);
            //DrawingObjects.AddObject(new StringDraw(CurState, new Position2D(3, 0)));
            //Planner.Add(RobotID, pos, 0, false);
        }

        public double CalBallFrames(double dist)
        {
            double speed = 6.5;
            return (60 * dist) / speed;
        }
        public double CalRobotFrames(double d)
        {
            return (15 * d) / 0.13;

        }
        public double RichTheBall(double frame)
        {
            return (frame * 0.13) / 15;
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(NormalSharedState.CommonInfo.OnlineRole1Target);

        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() {new OnLineRole1(),new OnLineRole2(),new OnLineRole3() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
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
