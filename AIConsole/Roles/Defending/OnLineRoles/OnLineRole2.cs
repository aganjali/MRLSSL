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
    class OnLineRole2 : RoleBase
    {




        string CurState;
        Position2D lastBallPos = new Position2D();
        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID)
        {
            double x = Model.BallState.Location.X;
            double y = Model.BallState.Location.Y;
            Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalLeft, GameParameters.OurGoalRight, 0.33);


            Position2D rightIntersect;
            Position2D leftIntersect;
            Line right = new Line(p1, Model.BallState.Location);
            Line left = new Line(GameParameters.OurGoalLeft.Extend(0, 0), Model.BallState.Location);

            double distToPenaltyAreaThreshold = 0.00;
            Line l1 = new Line(GameParameters.OurGoalLeft.Extend(-1.20, 0.60 + distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(0, 0.60 + distToPenaltyAreaThreshold));
            Line l2 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalLeft.Extend(-1.20 - distToPenaltyAreaThreshold, 0.6 + distToPenaltyAreaThreshold));
            Line l3 = new Line(GameParameters.OurGoalRight.Extend(-1.20 - distToPenaltyAreaThreshold, -0.6 - distToPenaltyAreaThreshold), GameParameters.OurGoalRight.Extend(0, -0.60 - distToPenaltyAreaThreshold));
            Position2D centerRobot = new Position2D();
            if (GameParameters.IsInField(Model.BallState.Location, 0))
            {
                lastBallPos = Model.BallState.Location;
            }
            Line intevallToBall = new Line(Position2D.Interpolate(right.Head, left.Head, 0.5), lastBallPos);
            //DrawingObjects.AddObject(intevallToBall);

            if (GameParameters.SegmentIntersect(intevallToBall, l1).HasValue) // left4
            {
                if (!IsInOurDangerZone(lastBallPos))
                {
                    centerRobot = l1.IntersectWithLine(intevallToBall).Value.Extend(0, 0.1);
                }
                else
                    centerRobot = lastBallPos.Extend(0.10, 0);

            }
            else if (GameParameters.SegmentIntersect(intevallToBall, l3).HasValue) //right
            {
                if (!IsInOurDangerZone(lastBallPos))
                {
                    centerRobot = l3.IntersectWithLine(intevallToBall).Value.Extend(0, -0.1);
                }
                else
                    centerRobot = lastBallPos.Extend(0.10, 0);

            }
            else if (GameParameters.SegmentIntersect(intevallToBall, l2).HasValue)//top
            {
                if (!IsInOurDangerZone(lastBallPos))
                {
                    centerRobot = l2.IntersectWithLine(intevallToBall).Value.Extend(-0.1, 0);
                }
                else
                    centerRobot = lastBallPos.Extend(0, 0.10);


            }
            else
            {
                centerRobot = lastBallPos;
            }


            Line robot = intevallToBall.PerpenducilarLineToPoint(centerRobot);
            DrawingObjects.AddObject(robot);
            rightIntersect = (Position2D)right.IntersectWithLine(robot);
            leftIntersect = (Position2D)left.IntersectWithLine(robot);
            double r = (rightIntersect - centerRobot).Size;
            double rprime = (leftIntersect - centerRobot).Size;
            double d = (rightIntersect - Model.BallState.Location).Size;
            double dprime = (leftIntersect - Model.BallState.Location).Size;

            double BallFrames = CalBallFrames(dprime);
            double RobotFrames = CalRobotFrames(rprime - 0.09);


            DrawingObjects.AddObject(right);
            DrawingObjects.AddObject(left);

            Vector2D v = new Vector2D();
            Position2D pos = new Position2D();
            if (OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
            {
                centerRobot = OppFreeKickDefenceUtils.Dive(engine, Model, RobotID);
            }
            DrawingObjects.AddObject(new Circle(centerRobot, 0.09, new Pen(Color.Purple, 0.01f)));
            var angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            NormalSharedState.CommonInfo.OnlineRole2Target = pos;
            Planner.AddKick(RobotID, kickPowerType.Speed, true, 3);
            Planner.Add(RobotID, centerRobot, angle, PathType.UnSafe, false, true, true, true, false);
            // DrawingObjects.AddObject(new StringDraw(CurState, new Position2D(3, 0)));
            //Planner.Add(RobotID, pos, 0, false);

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
            return Model.OurRobots[RobotID].Location.DistanceFrom(NormalSharedState.CommonInfo.OnlineRole2Target);

        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new OnLineRole1(), new OnLineRole2(), new GerrardRole() };

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
        public double CalBallFrames(double dist)
        {
            double speed = 6.5;
            return (StaticVariables.FRAME_RATE * dist) / speed;
        }
        public double CalRobotFrames(double d)
        {
            return (15 * d) / 0.13;

        }
        public double RichTheBall(double frame)
        {
            return (frame * 0.13) / 15;
        }
    }
}
