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
using MRL.SSL.Planning.GamePlanner;
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
        bool goRogue = true;
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
                int on1ID = NormalSharedState.CommonInfo.OnlineRole1Id;
                int on2ID = NormalSharedState.CommonInfo.OnlineRole2Id;

                Position2D p1 = Position2D.Interpolate(GameParameters.OurGoalLeft, GameParameters.OurGoalRight, 0.33);
                Position2D p2 = Position2D.Interpolate(GameParameters.OurGoalRight, GameParameters.OurGoalLeft, 0.33);
                double eXx = GetVisibleWidth(Model, engine);
                eXx = Math.Min(eXx, 1.2);
                double distanceFromCenter = Math.Round( Map(eXx, 0, 1.2, 0.1, 0.6),2);
                DrawingObjects.AddObject(new StringDraw(distanceFromCenter.ToString(), new Position2D(6, 1)));
                posToGo = GameParameters.OurGoalCenter + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(distanceFromCenter);
                posToGo.X = Math.Min(posToGo.X,5.85);
                angle = (ball.Location - posToGo).AngleInDegrees;
                Planner.Add(RobotID, posToGo, angle, PathType.UnSafe, false, false, false, true, false);
            }
            else if (CurrentState == (int)state.robotTarget)
            {
                DrawingObjects.AddObject(new StringDraw("robot Target ", new Position2D(6, 1.5)));

                SingleObjectState neearestOppToBall = new SingleObjectState();
                if (Model.Opponents.Count > 0) 
                    neearestOppToBall = Model.Opponents.OrderBy(o => o.Value.Location.DistanceFrom(ball.Location)).ToList()[0].Value;

                posToGo = RobotToPosIntersect(neearestOppToBall, new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight)).Value;
                angle = (ball.Location - posToGo).AngleInDegrees;
                Planner.Add(RobotID, posToGo, angle, PathType.UnSafe, false, false, false, true, false);
            }
            else if (CurrentState == (int)state.dive)
            {
                GetSkill<GoalieDiveSkill2017>().vandersarDive(engine, Model, RobotID, ref posToGo, ref angle);
                DrawingObjects.AddObject(new StringDraw("Dive ",new Position2D(6,1.5)));
            }
            else if (CurrentState == (int)state.inPenaltyArea)
            {
                GetBallSkill getBallSkill = new GetBallSkill();
                getBallSkill.SetAvoidDangerZone(false,true);
                Position2D target = new Position2D();
                getBallSkill.Perform(engine, Model, RobotID, target,false,0.05);
                Planner.AddKick(RobotID,kickPowerType.Speed,true,3);
            }
            //Vector2D extended = Model.BallState.Location - GameParameters.OurGoalCenter;

        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

            SingleObjectState ball = Model.BallState;
            intervalPos = engine.GameInfo.IntervalSelect(engine.GameInfo.GetVisibleIntervals(Model, ball.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, true, true, null, new int[1] { RobotID }));
            List<VisibleGoalInterval> vsi = engine.GameInfo.GetVisibleIntervals(Model, ball.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, true, true, null, new int[1] { RobotID });
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
            double dist = 0, distFromBorder = 0 ;

            if (Model.Opponents.Count > 0 && minOppToBall < 0.14 && ball.Speed.Size < 0.5 && intersectGoal.HasValue && Position2D.IsBetween(intervalPos[0], intervalPos[1], intersectGoal.Value))
            {
                CurrentState = (int)state.ballTarget;
            }
            else if (BallKickedToOurGoal(Model))
            {
                CurrentState = (int)state.dive;
            }
            else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out dist, out distFromBorder)
                && Model.BallState.Speed.Size < 1 )
            {
                CurrentState = (int)state.inPenaltyArea;
            }
            else
            {
                CurrentState = (int)state.ballTarget;
            }
            //CurrentState = stateCalculator(CurrentState, Model);


        }

        private int stateCalculator(int currentState,WorldModel model)
        {
            int ret = 0;
            double dist = 0, distFromBorder = 0;
            int def1ID = NormalSharedState.CommonInfo.OnlineRole1Id;
            int def2ID = NormalSharedState.CommonInfo.OnlineRole2Id;
            Position2D def1Target = NormalSharedState.CommonInfo.OnlineRole1Target;
            Position2D def2Target = NormalSharedState.CommonInfo.OnlineRole2Target;
            Position2D def1CurrentPos = model.OurRobots[def1ID].Location;
            Position2D def2CurrentPos = model.OurRobots[def2ID].Location;
            const double fullyTresh = 0.04;
            const double almostTresh = 0.10;
            const double inPenaltyAreaSpeedTresh = 1;// m/s
            

            bool DefsFullyArrived = def1CurrentPos.DistanceFrom(def1Target) < fullyTresh 
                                    && def2CurrentPos.DistanceFrom(def2Target) < fullyTresh;
            bool DefsAlmostArrived = def1CurrentPos.DistanceFrom(def1Target) < almostTresh
                                    && def2CurrentPos.DistanceFrom(def2Target) < almostTresh;

            bool def1missing = def1ID == -1;
            bool def2missing = def2ID == -1;
            bool setDive = BallKickedToOurGoal(model);
            bool setInPenaltyArea = GameParameters.IsInDangerousZone(model.BallState.Location, false, 0, out dist, out distFromBorder)
                && model.BallState.Speed.Size < inPenaltyAreaSpeedTresh;
            bool setRobotTarget = false;
            #region preDive Configs
            int? oppBallOwner = model.Opponents.OrderBy(t => t.Value.Location.DistanceFrom(model.BallState.Location)).Select(y => y.Key).FirstOrDefault();
            bool ballIsInFront = false;
            if (oppBallOwner.HasValue && model.Opponents.ContainsKey(oppBallOwner.Value))
            {
                int oppBallOwnerID = oppBallOwner.Value;
                Line robotLeftLine = new Line(model.Opponents[oppBallOwnerID].Location + Vector2D.FromAngleSize((model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180) + Math.PI / 2, .11)
                    , (model.Opponents[oppBallOwnerID].Location + Vector2D.FromAngleSize((model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180) + Math.PI / 2, .11)) + Vector2D.FromAngleSize((model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180), 1));
                Line robotRightLine = new Line(model.Opponents[oppBallOwnerID].Location + Vector2D.FromAngleSize((model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180) - Math.PI / 2, .11)
                    , (model.Opponents[oppBallOwnerID].Location + Vector2D.FromAngleSize((model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180) - Math.PI / 2, .11)) + Vector2D.FromAngleSize((model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180), 1));

                if (robotLeftLine.Distance(model.BallState.Location) + robotRightLine.Distance(model.BallState.Location) < .23)
                {
                    DrawingObjects.AddObject(new StringDraw("ball in front", model.OurRobots[model.GoalieID.Value].Location.Extend(0.2, 0)));
                    ballIsInFront = true;
                }
                else
                {
                    ballIsInFront = false;
                }

                //if (def1missing || def2missing)
                //{
                //    if (StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05 || StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05)
                //    {
                //        if (StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05 && StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .05)
                //        {
                //            List<int> IDsExeptthanwanted = new List<int>();
                //            IDsExeptthanwanted = model.OurRobots.Where(t => t.Key != FreekickDefence.Static1ID.Value).Select(y => y.Key).ToList();
                //            Obstacles obstacles2 = new Obstacles(model);
                //            obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, model.Opponents.Keys.ToList());
                //            if (obstacles2.Meet(model.Opponents[oppBallOwnerID], new SingleObjectState(model.Opponents[oppBallOwnerID].Location + Vector2D.FromAngleSize(model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                //            {
                //                goRogue = false;
                //            }
                //            else
                //            {
                //                goRogue = true;
                //            }
                //        }
                //        else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .05)
                //        {
                //            List<int> IDsExeptthanwanted = new List<int>();
                //            IDsExeptthanwanted = model.OurRobots.Where(t => t.Key != FreekickDefence.Static2ID.Value).Select(y => y.Key).ToList();
                //            Obstacles obstacles2 = new Obstacles(model);
                //            obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, model.Opponents.Keys.ToList());
                //            if (obstacles2.Meet(model.Opponents[oppBallOwnerID], new SingleObjectState(model.Opponents[oppBallOwnerID].Location + Vector2D.FromAngleSize(model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                //            {
                //                goRogue = false;
                //            }
                //            else
                //            {
                //                goRogue = true;
                //            }
                //        }
                //        else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05)
                //        {
                //            goRogue = true;
                //        }
                //        else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .05)
                //        {
                //            List<int> IDsExeptthanwanted = new List<int>();
                //            IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != FreekickDefence.Static2ID.Value && t.Key != FreekickDefence.Static1ID.Value).Select(y => y.Key).ToList();
                //            Obstacles obstacles2 = new Obstacles(Model);
                //            obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, Model.Opponents.Keys.ToList());
                //            if (obstacles2.Meet(Model.Opponents[oppBallOwnerID], new SingleObjectState(Model.Opponents[oppBallOwnerID].Location + Vector2D.FromAngleSize(Model.Opponents[oppBallOwnerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                //            {
                //                goRogue = false;
                //            }
                //            else
                //            {
                //                goRogue = true;
                //            }
                //        }
                //    }
                //}
            }

            #endregion
            if (def1ID == -1 || def2ID == -1)
                goRogue = true;
            if (currentState == (int)state.ballTarget)
            {
                if (setDive)
                {
                    ret = (int)state.dive;
                }
                else if (setRobotTarget)  
                {
                    ret = (int)state.robotTarget;
                }
                else if (setInPenaltyArea)
                {
                    ret = (int)state.inPenaltyArea;
                }
            }
            else if (currentState == (int)state.robotTarget)
            {
                if (setDive)
                {
                    ret = (int)state.dive;
                }
                else if (setInPenaltyArea)
                {
                    ret = (int)state.inPenaltyArea;
                }
                else
                {
                    ret = (int)state.ballTarget;

                }
            }
            else if (currentState == (int)state.inPenaltyArea)
            {
                if (setDive)
                {
                    ret = (int)state.dive;
                }
                else if (setRobotTarget)
                {
                    ret = (int)state.robotTarget;
                }
                else
                {
                    ret = (int)state.ballTarget;

                }

            }
            else if (currentState == (int)state.dive)
            {
                if (setInPenaltyArea)
                {
                    ret = (int)state.inPenaltyArea;
                } 
                else if (setRobotTarget)
                {
                    ret = (int)state.robotTarget;
                }
                else
                {
                    ret = (int)state.ballTarget;

                }
            }
            return ret;
        }
        private int? calculatePass(WorldModel model) {
            int? passRecieverID = null;
            const double movingSpeedTresh = 1.2;
            const double staticRadiusTresh = 0.13;

            Dictionary<int, SingleObjectState> staticOpps = new Dictionary<int, SingleObjectState>();
            Dictionary<int, SingleObjectState> movingOpps = new Dictionary<int, SingleObjectState>();
            Dictionary<int, double> times = new Dictionary<int, double>();
            foreach (var item in model.Opponents)
            {
                if (item.Value.Speed.Size > movingSpeedTresh)
                {
                    movingOpps.Add(item.Key,item.Value);
                }
                else
                {
                    staticOpps.Add(item.Key,item.Value);
                }
            }
            #region static opps
            foreach (var item in staticOpps)
            {
                Circle Robot = new Circle(item.Value.Location, staticRadiusTresh);

            }
            #endregion
            #region moving opps

            #endregion


            return passRecieverID;
        }
        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 10;
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
        public double GetVisibleWidth(WorldModel model, GameStrategyEngine engine)
        {
            double ret= 0;
            if (!model.GoalieID.HasValue)
            {
                return 2;
            }
            int[] robotToExclude = new int[1]{ model.GoalieID.Value };
            List<VisibleGoalInterval> intervals = engine.GameInfo.GetVisibleIntervals(model, model.BallState.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, true,null, robotToExclude);
            //List<VisibleGoalInterval> intervals = engine.GameInfo.OurGoalIntervals;
            //List<List<Position2D>> visibleGoalSpaces = new List<List<Position2D>>();
            foreach (var item in intervals)
            {
                double ourGoalX = GameParameters.OurGoalCenter.X;
                DrawingObjects.AddObject(new Line(new Position2D(ourGoalX, item.interval.Start), new Position2D(ourGoalX, item.interval.End), new Pen(Color.Red, 0.01f)));
                //List<Position2D> openPontList = new List<Position2D>() { new Position2D(ourGoalX, item.interval.Start) , new Position2D(ourGoalX, item.interval.End) };
                //visibleGoalSpaces.Add(openPontList);
                double currentWidth = new Position2D(ourGoalX, item.interval.Start).DistanceFrom(new Position2D(ourGoalX, item.interval.End));
                ret += currentWidth;
            }
            return ret;
            //DrawingObjects.AddObject(new StringDraw(visible));
            //double visibleWidth = targetSegment.Head.DistanceFrom(targetSegment.Tail);
            //if (obstacles.Count < 1)
            //    return visibleWidth;
            //List<Circle> circleObs = new List<Circle>();
            //foreach (var item in obstacles)
            //{
            //    circleObs.Add(new Circle(item, 0.09));
            //}
            //foreach (var item in circleObs)
            //{
            //    List<Line> tangentLines = new List<Line>();
            //    List<Position2D> tangentPoses = new List<Position2D>();
            //    item.GetTangent(model.BallState.Location, out tangentLines, out tangentPoses);
            //    if (tangentLines.Count < 2)
            //        continue;
            //    Position2D? intersect1 = tangentLines[0].IntersectWithLine(targetSegment);
            //    Position2D? intersect2 = tangentLines[1].IntersectWithLine(targetSegment);
            //    if (intersect1.HasValue && intersect2.HasValue &&
            //        Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect1.Value) &&
            //        Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect2.Value))
            //        visibleWidth -= intersect1.Value.DistanceFrom(intersect2.Value);
            //    else if (intersect1.HasValue && Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect1.Value))
            //    {
            //        if (Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Head))
            //            visibleWidth -= intersect1.Value.DistanceFrom(targetSegment.Head);
            //        else
            //            visibleWidth -= intersect1.Value.DistanceFrom(targetSegment.Tail);

            //    }
            //    else if (intersect2.HasValue && Position2D.IsBetween(targetSegment.Head, targetSegment.Tail, intersect2.Value))
            //    {
            //        if (Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Head))
            //            visibleWidth -= intersect2.Value.DistanceFrom(targetSegment.Head);
            //        else
            //            visibleWidth -= intersect2.Value.DistanceFrom(targetSegment.Tail);
            //    }
            //    else if (Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Head) &&
            //        Position2D.IsBetween(intersect1.Value, intersect2.Value, targetSegment.Tail))
            //    {
            //        //visibleWidth = 0.00001;
            //    }
            //}
            //return Math.Max(0, visibleWidth);
        }

        double Map(double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        enum state
        {
            ballTarget,
            robotTarget,
            dive,
            inPenaltyArea
        }
    }
}
