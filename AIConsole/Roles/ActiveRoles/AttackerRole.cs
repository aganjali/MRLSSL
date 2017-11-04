//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.AIConsole.Engine;
//using MRL.SSL.GameDefinitions;
//using MRL.SSL.AIConsole.Skills;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.Planning.MotionPlanner;

//namespace MRL.SSL.AIConsole.Roles
//{
//    public class AttackerRole : RoleBase
//    {
//        public override RoleCategory QueryCategory()
//        {
//            return RoleCategory.Positioner;
//        }
//        Position2D Target = Position2D.Zero;

//        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
//        {

//            if (CurrentState == (int)State.OutLeft)
//            {

//                if (Model.BallState.Location.Y > 0.1)
//                {
//                    CurrentState = (int)State.OutRight;
//                }
//            }
//            else if (CurrentState == (int)State.OutRight)
//            {

//                if (Model.BallState.Location.Y < -0.1)
//                {
//                    CurrentState = (int)State.OutLeft;
//                }
//            }

//            if (CurrentState == (int)State.OutLeft)
//                Target = new Position2D(-1.5, 1.7);
//            else
//            {
//                Target = new Position2D(-1.5, -1.7);
//            }
//        }

//        double y = 0.5;
//        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID)
//        {
//            Vector2D ballRobotVec = Model.BallState.Location - Model.OurRobots[RobotID].Location;
//            Vector2D ballspeed = Model.BallState.Speed;
//            double inner = Vector2D.InnerProduct(ballRobotVec, ballspeed);
//            double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobotVec, ballspeed));
//            bool near = false;
//            BallKickedToUs(Model, RobotID, ref near);
//            if (near)
//            {
//                Position2D target1 = GameParameters.OppGoalCenter;
//                GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, null, false, target1, 8, false);
//                return;
//            }
//            else
//            {
//                //if (CurrentState == (int)State.InLeft)
//                //    Target = new Position2D(-2.7, 0.5);
//                //else if (CurrentState == (int)State.InRight)
//                //    Target = new Position2D(-2.7, -0.5);
//                if (CurrentState == (int)State.OutLeft)
//                    Target = new Position2D(-2.5, 1.5);
//                else
//                {
//                    Target = new Position2D(-2.5, -1.5);
//                }
//            }

//            GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees, true, false);
//        }
//        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D PassPoint, int? ActiveID, bool PassIsChip)
//        {
//            Vector2D ballRobotVec = Model.BallState.Location - Model.OurRobots[RobotID].Location;
//            Vector2D ballspeed = Model.BallState.Speed;
//            double inner = Vector2D.InnerProduct(ballRobotVec, ballspeed);
//            double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobotVec, ballspeed));
//            bool near = false;
//            bool kicked = BallKickedToUs(Model, RobotID, ref near, PassPoint);
//            if (kicked && near)
//            {
//                Position2D target1 = GameParameters.OppGoalCenter;
//                GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, (ActiveID.HasValue && Model.OurRobots.ContainsKey(ActiveID.Value)) ? Model.OurRobots[ActiveID.Value] : null, PassIsChip, target1, 8, false);
//                return;
//            }
//            else if (kicked)
//            {
//                Planner.Add(RobotID, (PassPoint - GameParameters.OppGoalCenter).GetNormalizeToCopy(0.1) + PassPoint, (GameParameters.OppGoalCenter - PassPoint).AngleInDegrees, PathType.UnSafe, false, false, true, false);
//                return;
//            }
//            else
//            {
//                GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees, true, false);
//                return;
//            }


//        }
//        Position2D Target4Cost = new Position2D();

//        public double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID)
//        {

//            Vector2D ballRobotVec = Model.BallState.Location - Model.OurRobots[RobotID].Location;
//            Vector2D ballspeed = Model.BallState.Speed;
//            double inner = Vector2D.InnerProduct(ballRobotVec, ballspeed);
//            double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobotVec, ballspeed));
//            bool near = false;
//            BallKickedToUs(Model, RobotID, ref near);
//            if (near)
//            {
//                Position2D target1 = GameParameters.OppGoalCenter;

//                return ballspeed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location).DistanceFrom(Model.OurRobots[RobotID].Location);
//            }
//            else
//            {

//                if (CurrentState == (int)State.OutLeft)
//                    Target = new Position2D(-1.5, 1.7);
//                else
//                {
//                    Target = new Position2D(-1.5, -1.7);
//                }

//                return Model.OurRobots[RobotID].Location.DistanceFrom(Target);
//            }
//        }
//        public double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D PassPoint)
//        {

//            Vector2D ballRobotVec = Model.BallState.Location - Model.OurRobots[RobotID].Location;
//            Vector2D ballspeed = Model.BallState.Speed;
//            double inner = Vector2D.InnerProduct(ballRobotVec, ballspeed);
//            double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobotVec, ballspeed));
//            bool near = false;
//            bool kicked = BallKickedToUs(Model, RobotID, ref near, PassPoint);
//            if (kicked && near)
//            {
//                Position2D target1 = GameParameters.OppGoalCenter;

//                return ballspeed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location).DistanceFrom(Model.OurRobots[RobotID].Location);
//            }
//            else if (kicked)
//            {
//                return PassPoint.DistanceFrom(Model.OurRobots[RobotID].Location);
//            }
//            else
//            {
//                return Model.OurRobots[RobotID].Location.DistanceFrom(Target);
//            }

//        }


//        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);
//            return Cost(engine, Model, RobotID, Target + new Vector2D(-0.5, 0));
//        }

//        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            List<RoleBase> res = new List<RoleBase>() { 
//                new ActiveRole(),new AttackerRole(),
//            new NewSupporterRole(), new DefenderMarkerNormalRole1()};
//            return res;
//        }


//        ActiveParameters parameters = new ActiveParameters();
//        private bool BallKickedToUs(WorldModel Model, int RobotID, ref bool isNear)
//        {
//            Line line = new Line();
//            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
//            Position2D BallGoal = line.CalculateY(Model.OurRobots[RobotID].Location.X);
//            isNear = false;
//            if (Model.BallState.Speed.Size > 0.5)
//                if (Model.BallState.Speed.InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0)
//                {
//                    List<Position2D> poses = new Circle(Model.OurRobots[RobotID].Location, 2).Intersect(line);

//                    List<Position2D> poses2 = new Circle(Model.OurRobots[RobotID].Location, ActiveParameters.nearIncomingRadi).Intersect(line);
//                    foreach (var item in poses2)
//                    {
//                        DrawingObjects.AddObject(item);
//                    }
//                    if (poses2.Count > 0 && Model.BallState.Speed.Size > 0.5)
//                        isNear = true;

//                    if (poses.Count > 0)
//                        return true;
//                }
//            return false;
//        }
//        private bool BallKickedToUs(WorldModel Model, int RobotID, ref bool isNear, Position2D PassPoint)
//        {
//            Line line = new Line();
//            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
//            isNear = false;
//            if (Model.BallState.Speed.Size > 0.5)
//                if (Model.BallState.Speed.InnerProduct(PassPoint - Model.BallState.Location) > 0)
//                {
//                    List<Position2D> poses = new Circle(PassPoint, 2).Intersect(line);

//                    List<Position2D> poses2 = new Circle(PassPoint, ActiveParameters.nearIncomingRadi).Intersect(line);
//                    foreach (var item in poses2)
//                    {
//                        DrawingObjects.AddObject(item);
//                    }
//                    if (poses2.Count > 0 && Model.BallState.Speed.Size > 0.5)
//                        isNear = true;

//                    if (poses.Count > 0)
//                        return true;
//                }
//            return false;
//        }

//        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            throw new NotImplementedException();
//        }
//        private enum State
//        {
//            OutLeft = 0,
//            OutRight = 1,
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    public class AttackerRole : RoleBase
    {
        double distForceStartTresh = 1.5, angleForceStartTresh = 20;

        Position2D firstPos = Position2D.Zero, Target = Position2D.Zero;
        bool firstInForceStart = true;
        PassState CurrentPassState = PassState.OutLeft;
        int directStateX = 0, directStateY = 0;
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }
        bool firstIn = true, calcWaitPoint = true;
        const double ypoint = 1.5;
        int goCounter = 0, counterTresh = 10, waitPointIdx = 0, nextIdx = 0, angleTresh = 20;
        List<Position2D> points = new List<Position2D>() { new Position2D(2, ypoint), new Position2D(1, ypoint), new Position2D(0, ypoint), new Position2D(-1, ypoint), new Position2D(-2, ypoint), 
            new Position2D(2, -ypoint), new Position2D(1, -ypoint), new Position2D(0, -ypoint), new Position2D(-1, -ypoint), new Position2D(-2, -ypoint), 
            new Position2D(2, 0), new Position2D(1, 0), new Position2D(0, 0), new Position2D(-1, 0), new Position2D(-2, 0) 
        };
        bool force = false, gotForce = false;

        public bool Force
        {
            get { return force; }
            set { force = value; }
        } 
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            bool inWait = false;
            #region ForceStart
            if (force)
            {
                CurrentState = (int)State.ForceStart;
                if (firstInForceStart)
                {
                    firstPos = Model.BallState.Location;

                    if (Model.BallState.Location.X > 1.5 || Math.Abs(Model.BallState.Location.Y) < 1)
                    {
                        Target = Model.BallState.Location + new Vector2D(-1.5, 0);
                    }
                    else if (Model.BallState.Location.X > -1.2)
                        Target = Model.BallState.Location + new Vector2D(-1.5, 0);//Model.BallState.Location + new Vector2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.7);
                    else
                        Target = Model.BallState.Location + new Vector2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.7);


                    Target.X = Math.Sign(Target.X) * Math.Min(Math.Abs(Target.X), GameParameters.OurGoalCenter.X - 0.5);
                    Target.Y = Math.Sign(Target.Y) * Math.Min(Math.Abs(Target.Y), GameParameters.OurLeftCorner.Y - 0.5);

                    firstInForceStart = false;
                }

                Vector2D ballFirstPos = Model.BallState.Location - firstPos;
                double dist =Model.BallState.Location.DistanceFrom(firstPos);
                if (Model.BallState.Location.X < -2.0 || (dist > distForceStartTresh || (dist > 0.1 && dist <= distForceStartTresh && Math.Abs(Vector2D.AngleBetweenInDegrees(ballFirstPos, Target - firstPos)) > angleForceStartTresh)))
                {
                    force = false;
                }
            }
            #endregion
            #region PlayModes
            if (!force)
            {
                #region Direct Or Force
                if (ActiveParameters.playMode == ActiveParameters.PlayMode.Direct || ActiveParameters.playMode == ActiveParameters.PlayMode.Force)
                {
                    CurrentState = (int)State.DirectMode;
                    if (Model.BallState.Speed.Size < 0.15)
                        goCounter++;
                    if(Model.BallState.Speed.Size > 1.5)
                        goCounter = 0;
                    if (goCounter > counterTresh)
                    {
                        if (Model.BallState.Location.X < -2)
                            directStateX = 2;
                        else if (Model.BallState.Location.X > 2)
                            directStateX = -1;
                        else

                            if (directStateX == 0 && Model.OurRobots[RobotID].Location.X < Model.BallState.Location.X - 0.1)
                                directStateX = 1;
                            else if (directStateX == 1 && Model.OurRobots[RobotID].Location.X > Model.BallState.Location.X + 0.3)
                                directStateX = 0;

                        if (Model.BallState.Location.Y < -2 * GameParameters.OurLeftCorner.Y / 3)
                        {
                            directStateY = 0;
                        }
                        else if (Model.BallState.Location.Y < 0)
                        {
                            if (directStateY != 2)
                                directStateY = 1;
                            else if (Model.BallState.Location.Y < -0.5)
                                directStateY = 1;
                        }
                        else if (Model.BallState.Location.Y < 2 * GameParameters.OurLeftCorner.Y / 3)
                        {
                            if (directStateY != 1)
                                directStateY = 2;
                            else if (Model.BallState.Location.Y > 0.5)
                                directStateY = 2;
                        }
                        else
                            directStateY = 3;


                        if (directStateX == 2)
                        {
                            if (Model.BallState.Location.Y > 0)
                                Target = new Position2D(Model.BallState.Location.X + 0.8, Math.Min(0, Model.BallState.Location.Y - 0.7));
                            else
                                Target = new Position2D(Model.BallState.Location.X + 0.8, Math.Max(0, Model.BallState.Location.Y + 0.7));
                        }
                        else if (directStateX == -1)
                        {
                            if (Model.BallState.Location.Y > 0)
                                Target = new Position2D(1.8, Math.Min(0, Model.BallState.Location.Y - 0.7));
                            else
                                Target = new Position2D(1.8, Math.Max(0, Model.BallState.Location.Y + 0.7));
                        }
                        else if (directStateX == 1)
                        {
                            if (firstIn)
                            {
                                nextIdx = waitPointIdx;
                                if (nextIdx >= 10)
                                {
                                    if (Model.BallState.Location.Y < 0)
                                        nextIdx -= 10;
                                    else
                                        nextIdx -= 5;
                                }
                                //Target = new Position2D(Model.BallState.Location.X + 0.8, Math.Sign(Model.OurRobots[RobotID].Location.Y) * 1.7);
                            }
                            if (Model.OurRobots[RobotID].Location.DistanceFrom(Target) < 0.2)
                                nextIdx--;
                            if (waitPointIdx >= 10)
                                nextIdx = Math.Max(10, nextIdx);
                            else if (waitPointIdx >= 10)
                                nextIdx = Math.Max(5, nextIdx);
                            else
                                nextIdx = Math.Max(0, nextIdx);
                            Target = points[nextIdx];
                            firstIn = false;
                        }
                        else if (directStateY == 0)
                        {
                            Target = new Position2D(Model.BallState.Location.X + 0.8, 0);
                        }
                        else if (directStateY == 3)
                        {
                            Target = new Position2D(Model.BallState.Location.X + 0.8, 0);
                        }
                        else if (directStateY == 1)
                        {
                            Target = new Position2D(Model.BallState.Location.X + 0.8, 1 * GameParameters.OurLeftCorner.Y / 3);
                        }
                        else if (directStateY == 2)
                        {
                            Target = new Position2D(Model.BallState.Location.X + 0.8, -1 * GameParameters.OurLeftCorner.Y / 3);
                        }
                    }
                    else if (Math.Abs(Vector2D.AngleBetweenInDegrees(Model.BallState.Speed, Model.OurRobots[RobotID].Location - Model.BallState.Location)) > angleTresh)
                    {
                        inWait = true;
                        if (calcWaitPoint)
                        {
                            double min = double.MaxValue;;
                            for (int i = 0; i < points.Count; i++)
                            {
                                double dist = Model.OurRobots[RobotID].Location.DistanceFrom(points[i]);
                                if (dist < min)
                                {
                                    min = dist;
                                    waitPointIdx = i;
                                }
                            }
                            Target = points[waitPointIdx];
                            calcWaitPoint = false;
                        }
                    }

                }
                #endregion
                #region Chip
                else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Chip)
                {
                    CurrentState = (int)State.ChipMode;
                    Target = new Position2D(-2, Math.Sign(Model.BallState.Location.Y) * 0.2);
                }
                #endregion
                #region Pass
                else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Pass)
                {
                    CurrentState = (int)State.PassMode;
                    if (CurrentPassState == PassState.OutLeft)
                    {
                        if (Model.BallState.Location.Y > 0.1)
                        {
                            CurrentPassState = PassState.OutRight;
                        }
                    }
                    else if (CurrentPassState == PassState.OutRight)
                    {

                        if (Model.BallState.Location.Y < -0.1)
                        {
                            CurrentPassState = (int)PassState.OutLeft;
                        }
                    }

                    if (CurrentPassState == PassState.OutLeft)
                        Target = new Position2D(-1.5, 1.7);
                    else
                    {
                        Target = new Position2D(-1.5, -1.7);
                    }
                }
                #endregion
                #region WaitPoint
                if (!inWait)
                {
                    calcWaitPoint = true;
                }
                #endregion
            }
            #endregion

            Target.X = Math.Min(1.8, Target.X);
            Target.Y = Math.Min(Math.Abs(Target.Y), GameParameters.OurLeftCorner.Y - 0.2) * Math.Sign(Target.Y);

            if (directStateX != 1)
                firstIn = true;

            if (!force)
            {
                firstInForceStart = true;
                firstPos = Position2D.Zero;
            }
          
        }


        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D PassPoint, int? ActiveID, bool PassIsChip, bool forceStart)
        {
            if (!force && !gotForce)
            {
                force = forceStart;
                gotForce = true;
            }
            DrawingObjects.AddObject("AttackerState", new StringDraw("State:" + (State)CurrentState, Target + new Vector2D(0.5, 0.5)));
            DrawingObjects.AddObject(new Circle(Target, 0.1, new System.Drawing.Pen(System.Drawing.Color.Red, 0.01f)), "TargetAttacker");
            DrawingObjects.AddObject(new StringDraw("YState: " + directStateY, Target + new Vector2D(0.8, 0.5)));
            DrawingObjects.AddObject(new StringDraw("XState: " + directStateX, Target + new Vector2D(0.7, 0.5)));
 
            if (CurrentState == (int)State.PassMode)
            {
                Vector2D ballRobotVec = Model.BallState.Location - Model.OurRobots[RobotID].Location;
                Vector2D ballspeed = Model.BallState.Speed;
                double inner = Vector2D.InnerProduct(ballRobotVec, ballspeed);
                double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobotVec, ballspeed));
                bool near = false;
                bool kicked = BallKickedToUs(Model, RobotID, ref near, PassPoint);
                if (kicked && near)
                {
                    Position2D target1 = GameParameters.OppGoalCenter;
                    GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, (ActiveID.HasValue && Model.OurRobots.ContainsKey(ActiveID.Value)) ? Model.OurRobots[ActiveID.Value] : null, PassIsChip, target1, 8, false);
                    return;
                }
                else if (kicked)
                {
                    Planner.Add(RobotID, (PassPoint - GameParameters.OppGoalCenter).GetNormalizeToCopy(0.1) + PassPoint, (GameParameters.OppGoalCenter - PassPoint).AngleInDegrees, PathType.UnSafe, false, false, true, true);
                    return;
                }
                else
                {
                    GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees, true, false);
                    return;
                }
            }
            else
                Planner.Add(RobotID, Target, 180, PathType.UnSafe, (force), true, true, true);

        }
       
   
        public double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            if (CurrentState == (int)State.PassMode)
            {
                Position2D PassPoint = Target + new Vector2D(-0.5, 0);
                Vector2D ballRobotVec = Model.BallState.Location - Model.OurRobots[RobotID].Location;
                Vector2D ballspeed = Model.BallState.Speed;
                double inner = Vector2D.InnerProduct(ballRobotVec, ballspeed);
                double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobotVec, ballspeed));
                bool near = false;
                bool kicked = BallKickedToUs(Model, RobotID, ref near, PassPoint);
                if (kicked && near)
                {
                    Position2D target1 = GameParameters.OppGoalCenter;

                    return ballspeed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location).DistanceFrom(Model.OurRobots[RobotID].Location);
                }
                else if (kicked)
                {
                    return PassPoint.DistanceFrom(Model.OurRobots[RobotID].Location);
                }
                else
                {
                    return Model.OurRobots[RobotID].Location.DistanceFrom(Target);
                }
            }
            else
                return Target.DistanceFrom(Model.OurRobots[RobotID].Location);
        }


        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);
            return Cost(engine, Model, RobotID);
            //double failMargin = 0.1, maxFaildConst = 90, epsilon = 0.1;
            //double d = 0;
            //if (engine.GameInfo.OurTeam.CatchBallLines.ContainsKey(RobotID) &&
            //    engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
            //{
            //    d = engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head.DistanceFrom(Model.OurRobots[RobotID].Location);
            //}
            //else
            //{
            //    d = (double)MRL.SSL.Planning.GamePlanner.BallState.maxLengh / 100.0 + failMargin;

            //    Vector2D ballSpeed = Model.BallState.Speed;

            //    Vector2D v = GameParameters.InRefrence(Model.OurRobots[RobotID].Location - Model.BallState.Location, ballSpeed);
            //    if (v.Y > epsilon)
            //    {
            //        d += Math.Abs(v.X) / v.Y;
            //    }
            //    else
            //    {
            //        d += maxFaildConst;
            //        d += Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
            //    }
            //}
            //return 1 / Math.Max(d, 0.001);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { 
                new ActiveRole(),new AttackerRole(),
            new NewSupporterRole(), new DefenderMarkerNormalRole1()};
            return res;
        }


  //      ActiveParameters parameters = new ActiveParameters();
     
        private bool BallKickedToUs(WorldModel Model, int RobotID, ref bool isNear)
        {
            Line line = new Line();
            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
            Position2D BallGoal = line.CalculateY(Model.OurRobots[RobotID].Location.X);
            isNear = false;
            if (Model.BallState.Speed.Size > 0.5)
                if (Model.BallState.Speed.InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0)
                {
                    List<Position2D> poses = new Circle(Model.OurRobots[RobotID].Location, 2).Intersect(line);

                    List<Position2D> poses2 = new Circle(Model.OurRobots[RobotID].Location, ActiveParameters.nearIncomingRadi).Intersect(line);
                    foreach (var item in poses2)
                    {
                        DrawingObjects.AddObject(item);
                    }
                    if (poses2.Count > 0 && Model.BallState.Speed.Size > 0.5)
                        isNear = true;

                    if (poses.Count > 0)
                        return true;
                }
            return false;
        }
        private bool BallKickedToUs(WorldModel Model, int RobotID, ref bool isNear, Position2D PassPoint)
        {
            Line line = new Line();
            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
            isNear = false;
            if (Model.BallState.Speed.Size > 0.5)
                if (Model.BallState.Speed.InnerProduct(PassPoint - Model.BallState.Location) > 0)
                {
                    List<Position2D> poses = new Circle(PassPoint, 2).Intersect(line);

                    List<Position2D> poses2 = new Circle(PassPoint, ActiveParameters.nearIncomingRadi).Intersect(line);
                    foreach (var item in poses2)
                    {
                        DrawingObjects.AddObject(item);
                    }
                    if (poses2.Count > 0 && Model.BallState.Speed.Size > 0.5)
                        isNear = true;

                    if (poses.Count > 0)
                        return true;
                }
            return false;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
       
        private enum State
        {
            PassMode = 0,
            DirectMode = 1,
            ChipMode = 2,
            Force = 3,
            ForceStart = 4
        }
        private enum PassState
        {
            OutLeft = 0,
            OutRight = 1
        }
    }
}


