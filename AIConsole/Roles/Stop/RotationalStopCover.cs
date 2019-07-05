using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Roles
{
    class RotationalStopCover : RoleBase
    {

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }
        public void RotateRun(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID, bool useInFar, double distToBall, bool RotateInOppVec = true, bool rotate = true)
        {
            RotateInOppVec = true;
            bool debug = true;
            Position2D pos = Model.BallState.Location;
            Position2D ballPosInNearLeft = GameParameters.OurLeftCorner + (Position2D.Zero - GameParameters.OurLeftCorner).GetNormalizeToCopy(0.3);
            Position2D ballPosInNearRight = GameParameters.OurRightCorner + (Position2D.Zero - GameParameters.OurRightCorner).GetNormalizeToCopy(0.3);
            Position2D ballPosInFarLeft = GameParameters.OppLeftCorner + (Position2D.Zero - GameParameters.OppLeftCorner).GetNormalizeToCopy(0.3);
            Position2D ballPosInFarRight = GameParameters.OppRightCorner + (Position2D.Zero - GameParameters.OppRightCorner).GetNormalizeToCopy(0.3);
            Position2D ballPosInMiddleLeft = new Position2D(0, GameParameters.OurLeftCorner.Y) + (Position2D.Zero - new Position2D(0, GameParameters.OurLeftCorner.Y)).GetNormalizeToCopy(0.3);
            Position2D ballPosInMiddleRight = new Position2D(0, GameParameters.OurRightCorner.Y) + (Position2D.Zero - new Position2D(0, GameParameters.OurRightCorner.Y)).GetNormalizeToCopy(0.3);
            //if (CurrentState == (int)BallMode.NearRight)
            //{
            //    pos = ballPosInNearRight;
            //}
            //else if (CurrentState == (int)BallMode.NearLeft)
            //{
            //    pos = ballPosInNearLeft;
            //}
            //else if (CurrentState == (int)BallMode.MiddleRight)
            //{
            //    pos = ballPosInMiddleRight;
            //}
            //else if (CurrentState == (int)BallMode.MiddleLeft)
            //{
            //    pos = ballPosInMiddleLeft;
            //}
            //else if (CurrentState == (int)BallMode.FarRight)
            //{
            //    pos = ballPosInFarRight;
            //}
            //else if (CurrentState == (int)BallMode.FarLeft)
            //{
            //    pos = ballPosInFarLeft;
            //}

            Vector2D vecOurRightGoal = GameParameters.OurGoalCenter.Extend(0, -(GameParameters.OurGoalCenter.DistanceFrom(GameParameters.OurGoalLeft)) / 2) - pos;

            Vector2D vecOurLeftGoal = GameParameters.OurGoalCenter.Extend(0, (GameParameters.OurGoalCenter.DistanceFrom(GameParameters.OurGoalLeft)) / 2) - pos;

            Vector2D vecOppLeftGoal = GameParameters.OppGoalCenter.Extend(0, -(GameParameters.OppGoalCenter.DistanceFrom(GameParameters.OppGoalLeft)) / 2) - pos;

            Vector2D vecOppRightGoal = GameParameters.OppGoalCenter.Extend(0, (GameParameters.OppGoalCenter.DistanceFrom(GameParameters.OppGoalLeft)) / 2) - pos;

            Vector2D vecOurRightCorner = GameParameters.OurGoalCenter.Extend(0, -(GameParameters.OurGoalCenter.DistanceFrom(GameParameters.OurLeftCorner)) / 1.2) - pos;

            Vector2D vecOurLeftCorner = GameParameters.OurGoalCenter.Extend(0, (GameParameters.OurGoalCenter.DistanceFrom(GameParameters.OurLeftCorner)) / 1.2) - pos;

            Vector2D vecOppLeftCorner = GameParameters.OppGoalCenter.Extend(0, -(GameParameters.OppGoalCenter.DistanceFrom(GameParameters.OppLeftCorner)) / 1.2) - pos;

            Vector2D vecOppRightCorner = GameParameters.OppGoalCenter.Extend(0, (GameParameters.OppGoalCenter.DistanceFrom(GameParameters.OppLeftCorner)) / 1.2) - pos;
            Line l1 = new Line(Model.BallState.Location, pos + vecOurRightGoal);
            Line l2 = new Line(Model.BallState.Location, pos + vecOurLeftGoal);
            Line l3 = new Line(Model.BallState.Location, pos + vecOppLeftGoal);
            Line l4 = new Line(Model.BallState.Location, pos + vecOppRightGoal);
            Line l5 = new Line(Model.BallState.Location, pos + vecOurRightCorner);
            Line l6 = new Line(Model.BallState.Location, pos + vecOurLeftCorner);
            Line l7 = new Line(Model.BallState.Location, pos + vecOppLeftCorner);
            Line l8 = new Line(Model.BallState.Location, pos + vecOppRightCorner);


            double StopDistFromBall = Math.Min(Math.Max(distToBall, 0.9), 3);
            bool NearOppFromBall = false;
            double minDist = double.MaxValue;
            int oppFirstPasser = -1;
            foreach (var opp in Model.Opponents)
            {
                if (Model.BallState.Location.DistanceFrom(opp.Value.Location) < minDist)
                {
                    minDist = Model.BallState.Location.DistanceFrom(opp.Value.Location);
                    oppFirstPasser = opp.Key;
                }
            }
            if (oppFirstPasser != -1 && Model.Opponents[oppFirstPasser].Location.DistanceFrom(Model.BallState.Location) < 0.18)
                NearOppFromBall = true;

            if (!NearOppFromBall)
            {
                GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID, StopDistFromBall), (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
            }
            else if (Model.Opponents.ContainsKey(oppFirstPasser))
            {
                Position2D target = GetTarget(Model, RobotID, StopDistFromBall);
                int oppid = oppFirstPasser;

                Vector2D mainVec = new Vector2D();
                Vector2D checkMainVec = new Vector2D();
                if (rotate)
                {
                    if (RotateInOppVec)
                        mainVec = Vector2D.FromAngleSize(Model.Opponents[oppid].Angle.Value * Math.PI / 180, 1);
                    else
                    {
                        mainVec = Model.BallState.Location - Model.Opponents[oppid].Location;
                        checkMainVec = Model.Opponents[oppid].Location - Model.BallState.Location;
                    }
                }
                else
                {
                    mainVec = Model.BallState.Location - GameParameters.OurGoalCenter;
                }
                Position2D ret = Model.BallState.Location + mainVec.GetNormalizeToCopy(StopDistFromBall);
                Line oppballLine1 = new Line(Model.BallState.Location, Model.Opponents[oppFirstPasser].Location);
                Line oppballLine2 = new Line(Model.BallState.Location, ret);

                //DrawingObjects.AddObject(oppballLine1);
                DrawingObjects.AddObject(oppballLine2);
                DrawingObjects.AddObject(new StringDraw(mainVec.AngleInDegrees.ToString(), oppballLine2.Tail));

                //check and solve fail or error pos    
                if (CurrentState == (int)BallMode.NearRight)
                {
                    double angle = (mainVec.AngleInDegrees > 0 ? mainVec.AngleInDegrees : 180 + (180 + mainVec.AngleInDegrees));
                    if (angle < vecOurRightGoal.AngleInDegrees)
                    {
                        ret = Model.BallState.Location + vecOurRightGoal.GetNormalizeToCopy(StopDistFromBall);
                    }
                    if (angle > vecOppLeftCorner.AngleInDegrees)
                    {
                        ret = Model.BallState.Location + vecOppLeftCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                }
                else if (CurrentState == (int)BallMode.NearLeft)
                {
                    double angle = (mainVec.AngleInDegrees > 0 ? mainVec.AngleInDegrees : 180 + (180 + mainVec.AngleInDegrees));
                    double angleVecOppRightCorner = (vecOppRightCorner.AngleInDegrees > 0 ? vecOppRightCorner.AngleInDegrees : 180 + (180 + vecOppRightCorner.AngleInDegrees));
                    double angleVecOurLeftGoal = (vecOurLeftGoal.AngleInDegrees > 0 ? vecOurLeftGoal.AngleInDegrees : 180 + (180 + vecOurLeftGoal.AngleInDegrees));
                    if (angle > angleVecOurLeftGoal)
                    {
                        ret = Model.BallState.Location + vecOurLeftGoal.GetNormalizeToCopy(StopDistFromBall);
                    }
                    if (angle < angleVecOppRightCorner)
                    {
                        ret = Model.BallState.Location + vecOppRightCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                }
                else if (CurrentState == (int)BallMode.MiddleRight)
                {
                    double angle = (mainVec.AngleInDegrees > 0 ? mainVec.AngleInDegrees : (360 + mainVec.AngleInDegrees));
                    if (mainVec.AngleInDegrees < 0 && angle>270)
                    {
                        ret = Model.BallState.Location + vecOurRightCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                    if (mainVec.AngleInDegrees > vecOppRightCorner.AngleInDegrees || (angle < 270 && angle > vecOppRightCorner.AngleInDegrees))
                    {
                        ret = Model.BallState.Location + vecOppRightCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                }
                else if (CurrentState == (int)BallMode.MiddleLeft)
                {
                    vecOppLeftCorner = GameParameters.OppGoalCenter.Extend(0, -(GameParameters.OppGoalCenter.DistanceFrom(GameParameters.OppLeftCorner)) / 1.2) - Model.Opponents[oppFirstPasser].Location;
                    vecOurLeftCorner = GameParameters.OurGoalCenter.Extend(0, (GameParameters.OurGoalCenter.DistanceFrom(GameParameters.OurLeftCorner)) / 1.2) - Model.Opponents[oppFirstPasser].Location;
                    double angle = Model.Opponents[oppid].Angle.Value;//mainVec.AngleInDegrees;//(mainVec.AngleInDegrees > 0 ? mainVec.AngleInDegrees : (360 + mainVec.AngleInDegrees));
                    
                    if (angle<230 && angle>115)
                    {
                        ret = Model.BallState.Location + vecOppLeftCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                    else if (angle > 0 && angle<=115)
                    {
                        ret = Model.BallState.Location + vecOurLeftCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                         

                    //double checkangle = (checkMainVec.AngleInDegrees > 0 ? checkMainVec.AngleInDegrees : (360 + checkMainVec.AngleInDegrees));
                    //if (angle > 180)
                    //    angle -= 360;

                   
                    //double angleVecOppleftCorner = vecOppLeftCorner.AngleInDegrees;//(vecOppLeftCorner.AngleInDegrees > 0 ? vecOppLeftCorner.AngleInDegrees : (360 + vecOppLeftCorner.AngleInDegrees));
                    //double angleVecOurleftCorner = vecOurLeftCorner.AngleInDegrees;//(vecOurLeftCorner.AngleInDegrees > 0 ? vecOurLeftCorner.AngleInDegrees : (360 + vecOurLeftCorner.AngleInDegrees));
                    //if (angle > angleVecOppleftCorner || angle > angleVecOurleftCorner)
                    //{
                    //    //to zavie hastim
                    //}
                    //else
                    //{
                    //    angleVecOppleftCorner = (vecOppLeftCorner.AngleInDegrees > 0 ? vecOppLeftCorner.AngleInDegrees : (360 + vecOppLeftCorner.AngleInDegrees));
                    //    angleVecOurleftCorner = (vecOurLeftCorner.AngleInDegrees > 0 ? vecOurLeftCorner.AngleInDegrees : (360 + vecOurLeftCorner.AngleInDegrees));
                    //    angle = (angle > 0 ? angle : (360 + angle));
                    //    double p1 = Math.Abs(angle - angleVecOppleftCorner);
                    //    double p2 = Math.Abs(angle - angleVecOurleftCorner);
                    //    if (p1 < p2)
                    //    {
                    //        ret = Model.BallState.Location + vecOppLeftCorner.GetNormalizeToCopy(StopDistFromBall);
                    //    }
                    //    else
                    //    {
                    //        ret = Model.BallState.Location + vecOurLeftCorner.GetNormalizeToCopy(StopDistFromBall);
                    //    }
                    //}
                }
                else if (CurrentState == (int)BallMode.FarRight)
                {
                    double angle = Model.Opponents[oppid].Angle.Value;//mainVec.AngleInDegrees;//(mainVec.AngleInDegrees > 0 ? mainVec.AngleInDegrees : (360 + mainVec.AngleInDegrees));
                    if (angle > 90 && angle <= 270)
                    {
                        ret = Model.BallState.Location + vecOppRightCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                    else if ( angle > 270)
                    {
                        ret = Model.BallState.Location + vecOurRightCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                }
                else if (CurrentState == (int)BallMode.FarLeft)
                {
                    double angle = Model.Opponents[oppid].Angle.Value;//mainVec.AngleInDegrees;//(mainVec.AngleInDegrees > 0 ? mainVec.AngleInDegrees : (360 + mainVec.AngleInDegrees));
                    if (angle < 270 && angle > 90)
                    {
                        ret = Model.BallState.Location + vecOppRightCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                    else if (angle > 0 && angle <= 90)
                    {
                        ret = Model.BallState.Location + vecOurLeftCorner.GetNormalizeToCopy(StopDistFromBall);
                    }
                }
                //if (ret == Position2D.Zero || ret.DistanceFrom(Model.BallState.Location) < StopDistFromBall)
                //{
                //    ret = Model.BallState.Location + (Model.BallState.Location - Model.Opponents[oppid].Location).GetNormalizeToCopy(StopDistFromBall);
                //}
                if (debug)
                {
                    //DrawingObjects.AddObject(l1);
                    //DrawingObjects.AddObject(l2);
                    //DrawingObjects.AddObject(l3);
                    //DrawingObjects.AddObject(l4);
                    //DrawingObjects.AddObject(l5);
                    DrawingObjects.AddObject(l6);
                    DrawingObjects.AddObject(l7);
                    //DrawingObjects.AddObject(l8);
                    DrawingObjects.AddObject(new StringDraw(vecOurRightGoal.AngleInDegrees.ToString(), l1.Tail));
                    DrawingObjects.AddObject(new StringDraw(vecOurLeftGoal.AngleInDegrees.ToString(), l2.Tail));
                    DrawingObjects.AddObject(new StringDraw(vecOppLeftGoal.AngleInDegrees.ToString(), l3.Tail));
                    DrawingObjects.AddObject(new StringDraw(vecOppRightGoal.AngleInDegrees.ToString(), l4.Tail));
                    DrawingObjects.AddObject(new StringDraw(vecOurRightCorner.AngleInDegrees.ToString(), l5.Tail));
                    DrawingObjects.AddObject(new StringDraw(vecOurLeftCorner.AngleInDegrees.ToString(), l6.Tail));
                    DrawingObjects.AddObject(new StringDraw(vecOppLeftCorner.AngleInDegrees.ToString(), l7.Tail));
                    DrawingObjects.AddObject(new StringDraw(vecOppRightCorner.AngleInDegrees.ToString(), l8.Tail));
                }
                target = ret;
                GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
            }
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (Model.BallState.Location.X > 2.5)
            {
                //near
                if (Model.BallState.Location.Y > 0)
                {
                    CurrentState = (int)BallMode.NearLeft;
                }
                else if (Model.BallState.Location.Y < 0)
                {
                    CurrentState = (int)BallMode.NearRight;
                }
            }
            else if (Model.BallState.Location.X < -2.5)
            {
                //far
                if (Model.BallState.Location.Y > 0)
                {
                    CurrentState = (int)BallMode.FarLeft;
                }
                else if (Model.BallState.Location.Y < 0)
                {
                    CurrentState = (int)BallMode.FarRight;
                }
            }
            else if (Model.BallState.Location.X > -2.5 && Model.BallState.Location.X < 2.5)
            {
                //middle
                if (Model.BallState.Location.Y > 0)
                {
                    CurrentState = (int)BallMode.MiddleLeft;
                }
                else if (Model.BallState.Location.Y < 0)
                {
                    CurrentState = (int)BallMode.MiddleRight;
                }
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return GetTarget(Model, RobotID, 0.65).DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        private Position2D GetTarget(WorldModel Model, int RobotID, double dist)
        {
            double StopDistFromBall = dist;//.65;
            Position2D target = new Position2D();
            Position2D Target = new Position2D();
            Position2D DefenderStopRole1 = GameParameters.OurGoalCenter + new Vector2D(-1, -0.17);
            Position2D DefenderStopRole2 = GameParameters.OurGoalCenter + new Vector2D(-1, 0.17);
            Circle C = new Circle(Model.BallState.Location, 0.7);
            double angle = Math.Sign(Model.BallState.Location.Y) * 1 * Math.PI / 3;
            if (C.IsInCircle(DefenderStopRole1) || C.IsInCircle(DefenderStopRole2))
            {
                angle = Math.Sign(Model.BallState.Location.Y) * 1.5 * Math.PI / 3;
            }
            Position2D ball = Model.BallState.Location;

            if (Model.Status == GameStatus.Stop)
            {
                if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.4)
                {
                    target = new Position2D(2.08, 1);
                }
                else
                {
                    if (ball.X > 2.5 && Math.Abs(ball.Y) > 1.85)//corner
                    {
                        if (ball.Y < 0)
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .1, StopDistFromBall);
                        else
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .42, StopDistFromBall);

                    }
                    else if (ball.X > 2.5 && Math.Abs(ball.Y) > 1.75)//corner
                    {
                        if (ball.Y < 0)
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .4, StopDistFromBall);
                        else
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .82, StopDistFromBall);

                    }
                    else if (ball.X > 2.5 && Math.Abs(ball.Y) > 1.15)//corner
                    {
                        if (ball.Y < 0)
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * .65, StopDistFromBall);
                        else
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * 1, StopDistFromBall);

                    }
                    else if (ball.X > 1.5 && Math.Abs(ball.Y) > 1.15 && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 1.8)
                    {
                        target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (ball.X > 1.5 && Math.Abs(ball.Y) > 1.15 && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.8)
                    {
                        if (ball.Y > 0)
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + Math.Sign(ball.Y) * -.9, StopDistFromBall);
                        else
                            target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians + Math.Sign(ball.Y) * -.5, StopDistFromBall);
                    }
                    else if (ball.X > 2.2 && Math.Abs(ball.Y) > 1.15)
                    {
                        target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (Math.Abs(ball.X) <= 1.5 && Math.Abs(ball.Y) > 1.15)
                    {
                        target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                    }

                    else if (ball.X < -1.5 && Math.Abs(ball.Y) > 1.15)
                    {
                        target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (Math.Abs(ball.Y) <= 1.15 && ball.X < .95)
                    {
                        target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else if (Math.Abs(ball.Y) <= 1.15 && ball.X < 1.5 && ball.X >= .95)
                    {
                        target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                    }
                    else
                    {
                        target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                    }
                }
            }
            else
            {

                Target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, StopDistFromBall);
                target = OverlapSolving(Model, RobotID, Target);

            }

            return target;
        }

        Position2D OverlapSolving(WorldModel Model, int RobotID, Position2D Target)
        {
            Dictionary<int, SingleObjectState> ourRobots = Model.OurRobots.Where(u => u.Key != RobotID).ToDictionary(t => t.Key, y => y.Value);
            if (Model.GoalieID.HasValue)
            {
                ourRobots = Model.OurRobots.Where(t => t.Key != Model.GoalieID.Value && t.Key != RobotID).ToDictionary(y => y.Key, h => h.Value);
            }
            Position2D extendpoint = Target;
            int counter = 0;
            foreach (var item in ourRobots)
            {
                if (item.Value.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .25)
                {
                    counter++;
                    Vector2D RobotOGC = GameParameters.OurGoalCenter - Model.OurRobots[RobotID].Location;
                    Position2D LeftPoint = item.Value.Location + Vector2D.FromAngleSize(RobotOGC.AngleInRadians - Math.PI / 2, .24);
                    Position2D RightPoint = item.Value.Location + Vector2D.FromAngleSize(RobotOGC.AngleInRadians + Math.PI / 2, .24);
                    Line RobotOGCLn = new Line(Model.BallState.Location, GameParameters.OppGoalCenter);
                    DrawingObjects.AddObject(RobotOGC);
                    DrawingObjects.AddObject(LeftPoint);
                    DrawingObjects.AddObject(RightPoint);
                    Vector2D RobotOGCVEc = RobotOGCLn.Tail - RobotOGCLn.Head;
                    Position2D PerpPOsLeft = new Line(LeftPoint + Vector2D.FromAngleSize((RobotOGCLn.Tail - RobotOGCLn.Head).AngleInRadians + (Math.PI / 2), 1), LeftPoint).IntersectWithLine(RobotOGCLn).Value;
                    Position2D PerpPOsRight = new Line(RightPoint + Vector2D.FromAngleSize((RobotOGCLn.Tail - RobotOGCLn.Head).AngleInRadians + (Math.PI / 2), 1), RightPoint).IntersectWithLine(RobotOGCLn).Value;
                    double Disttoleft = PerpPOsLeft.DistanceFrom(LeftPoint);
                    double DisttoRight = PerpPOsRight.DistanceFrom(RightPoint);
                    if (Disttoleft < DisttoRight)
                    {
                        Target = LeftPoint;
                    }
                    else
                    {
                        Target = RightPoint;
                    }

                }
                if (counter == 0)
                {
                    Target = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, .6);

                }
            }

            extendpoint = Target;
            return extendpoint;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>();
            res.Add(new StopRole1());
            res.Add(new ActiveRole());
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        enum BallMode
        {
            NearRight,
            NearLeft,
            MiddleRight,
            MiddleLeft,
            FarRight,
            FarLeft
        }
    }
}
