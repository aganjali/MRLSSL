using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Skills.GoalieSkills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Roles
{
    class VandersarGoalKeeperRole : RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }
        #region variables
        bool firstFlag = true;
        List<Position2D> intervalPos = new List<Position2D>();
        #endregion
        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D posToGo = new Position2D();
            double angle = 0;
            //if (firstFlag)
            //{
            //    CurrentState = (int)state.ballTarget;
            //    firstFlag = false;
            //}

            SingleObjectState ball = Model.BallState;
            if (CurrentState == (int)state.ballTarget)
            {
                //NormalSharedState
                Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalLeft, GameParameters.OurGoalRight, 0.33);
                Position2D p2 = Position2D.Interpolate(GameParameters.OurGoalRight, GameParameters.OurGoalLeft, 0.33);
                double eXx = GetVisibleWidth(Model, new List<Position2D> { Model.OurRobots[6].Location, Model.OurRobots[7].Location }, new Line(p1, p2));
                double distanceFromCenter = Map(eXx, 0, 0.4, 0.1, 0.6);
                DrawingObjects.AddObject(new StringDraw(eXx.ToString(), new Position2D(2.7, 0)));
                posToGo = GameParameters.OurGoalCenter + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(distanceFromCenter);
                angle = (ball.Location - posToGo).AngleInDegrees;
            }
            else if (CurrentState == (int)state.preDive)
            {
                SingleObjectState neearestOppToBall = new SingleObjectState();
                if (Model.Opponents.Count > 0)
                    neearestOppToBall = Model.Opponents.OrderBy(o => o.Value.Location.DistanceFrom(ball.Location)).ToList()[0].Value;

                posToGo = RobotToPosIntersect(neearestOppToBall, new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight)).Value;
                angle = (ball.Location - posToGo).AngleInDegrees;
            }
            else if (CurrentState == (int)state.dive)
            {
                GetSkill<GoalieDiveSkill2017>().vandersarDive(engine, Model, RobotID, ref posToGo, ref angle);
            }
            Vector2D extended = Model.BallState.Location - GameParameters.OurGoalCenter;
            extended = extended.GetNormalizeToCopy(0);
            
            Planner.Add(RobotID, posToGo + extended, angle, PathType.UnSafe, false, false, false, true, false);
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            SingleObjectState ball = Model.BallState;
            intervalPos = engine.GameInfo.IntervalSelect(engine.GameInfo.GetVisibleIntervals(Model, ball.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, true, true, null, new int[3] { RobotID, 1, 2 }));
            List<VisibleGoalInterval> vsi = engine.GameInfo.GetVisibleIntervals(Model, ball.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, true, true, null, new int[3] { RobotID, 1, 2 });
            for (int i = 0; i < intervalPos.Count; i++)
            {
                intervalPos[i] = new Position2D(-intervalPos[i].X, intervalPos[i].Y);
            }
            foreach (var item in intervalPos)
            {
                DrawingObjects.AddObject(new Circle(item, 0.09, new Pen(Color.White, 0.01f)), item.toString());
            }
            double minOppToBall = 5;

            SingleObjectState neearestOppToBall = new SingleObjectState();
            if (Model.Opponents.Count > 0)
            {
                minOppToBall = Model.Opponents.OrderBy(o => o.Value.Location.DistanceFrom(ball.Location)).Select(o => o.Value.Location.DistanceFrom(ball.Location)).Min();
                neearestOppToBall = Model.Opponents.OrderBy(o => o.Value.Location.DistanceFrom(ball.Location)).ToList()[0].Value;
            }
            Position2D? intersectGoal = null;
            intersectGoal = RobotToPosIntersect(neearestOppToBall, new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight));
            if (Model.Opponents.Count > 0 && minOppToBall < 0.14 && ball.Speed.Size < 0.5 && intersectGoal.HasValue && Position2D.IsBetween(intervalPos[0], intervalPos[1], intersectGoal.Value))
            {
                CurrentState = (int)state.preDive;
            }
            else if (BallKickedToOurGoal(Model))
            {
                CurrentState = (int)state.dive;
            }
            else
            {
                CurrentState = (int)state.ballTarget;
            }

        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 0.1;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new VandersarGoalKeeperRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
        public Dictionary<int, Position2D> findTargets(List<VisibleGoalInterval> goalVisibleInterval, WorldModel model, int defRightId, int defLeftId)
        {
            if (!model.GoalieID.HasValue || !model.OurRobots.ContainsKey(model.GoalieID.Value) || !model.OurRobots.ContainsKey(defRightId) || !model.OurRobots.ContainsKey(defLeftId))
            {
                return new Dictionary<int, Position2D>();
            }
            //VisibleGoalInterval def1Interval = new VisibleGoalInterval(new Interval());

            return new Dictionary<int, Position2D>();
        }
        private static Position2D? RobotToPosIntersect(SingleObjectState RobotState, Line intersectLine)
        {
            if (!RobotState.Angle.HasValue)
                return null;
            Vector2D robotVec = Vector2D.FromAngleSize(RobotState.Angle.Value * Math.PI / 180, 5);
            Line l1 = intersectLine;
            Line l2 = new Line(RobotState.Location, RobotState.Location + robotVec);
            DrawingObjects.AddObject(l2, "hello im a line" + RobotState.Location.toString());
            return l1.IntersectWithLine(l2);
        }
        public bool BallKickedToOurGoal(WorldModel Model)
        {

            double tresh = 0.20;
            double tresh2 = 1.3;

            Line line = new Line();
            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            DrawingObjects.AddObject(new StringDraw((d / Model.BallState.Speed.Size < tresh2).ToString(), new Position2D(-1, 0)));
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + tresh && BallGoal.Y > GameParameters.OurGoalRight.Y - tresh)
                if (Model.BallState.Speed.InnerProduct(GameParameters.OurGoalRight - Model.BallState.Location) > 0)
                    if (Model.BallState.Speed.Size > 0.1 && d / Model.BallState.Speed.Size < tresh2)
                        return true;
            return false;
        }
        public double GetVisibleWidth(WorldModel model, List<Position2D> obstacles, Line targetSegment)
        {
            double visibleWidth = targetSegment.Head.DistanceFrom(targetSegment.Tail);
            if (obstacles.Count < 1)
                return visibleWidth;
            List<Circle> circleObs = new List<Circle>();
            foreach (var item in obstacles)
            {
                circleObs.Add(new Circle(item, 0.09));
            }
            foreach (var item in circleObs)
            {
                List<Line> tangentLines = new List<Line>();
                List<Position2D> tangentPoses = new List<Position2D>();
                item.GetTangent(model.BallState.Location, out tangentLines, out tangentPoses);
                if (tangentLines.Count < 2)
                    continue;
                Position2D? intersect1 = tangentLines[0].IntersectWithLine(targetSegment);
                Position2D? intersect2 = tangentLines[1].IntersectWithLine(targetSegment);
                if (intersect1.HasValue && intersect2.HasValue &&
                    Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect1.Value) &&
                    Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect2.Value))
                    visibleWidth -= intersect1.Value.DistanceFrom(intersect2.Value);
                else if (intersect1.HasValue && Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect1.Value))
                {
                    if (Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Head))
                        visibleWidth -= intersect1.Value.DistanceFrom(targetSegment.Head);
                    else
                        visibleWidth -= intersect1.Value.DistanceFrom(targetSegment.Tail);

                }
                else if (intersect2.HasValue && Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect2.Value))
                {
                    if (Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Head))
                        visibleWidth -= intersect2.Value.DistanceFrom(targetSegment.Head);
                    else
                        visibleWidth -= intersect2.Value.DistanceFrom(targetSegment.Tail);
                }
                else if (Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Head) &&
                    Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Tail))
                {
                    //visibleWidth = 0.00001;
                }
            }
            return Math.Max(0, visibleWidth);
        }
        
        double Map(double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        enum state
        {
            ballTarget,
            robotTarget,
            preDive,
            dive,
            inPenaltyArea
        }
    }
}
