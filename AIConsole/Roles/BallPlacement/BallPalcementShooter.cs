using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class BallPalcementShooter : RoleBase
    {

        StarkCatchSkill catchSkill = new StarkCatchSkill();
        private Position2D target;
        private Position2D OtherTarget;
        private double angle = 0;
        bool avoidBall = false;
        bool avoidRobot = false;
        bool passed = false;
        int counter = 0;
        int myOtherID = -1;
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;

        }
        const double behindBallTresh = 0.2;
        const double BallTresh = 0.2;
        const double eatBallTresh = 0.01;
        const double finishTresh = 0.7;
        double backBall;
        int finishCounter = 0;
        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, int OtherRobot, int Mode)
        {
            GetBallSkill activeSkill = new GetBallSkill();
            //var speed = Math.Min(Math.Max(0.9, 0.3 * Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos)), 4);
            double speed = CalShootSpeed(Model, StaticVariables.ballPlacementPos);
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 2.5);
            DrawingObjects.AddObject(new StringDraw("CurrentState= " + (states)CurrentState, "bpshooter_state", Model.OurRobots[RobotID].Location + new Vector2D(1, 1)));
            if (CurrentState == (int)states.pass)
            {
                if (!GameParameters.IsInField(Model.BallState.Location,0))
                {
                    const double angleTreshInDegree = 20;
                    //double shootSpeedTresh = CalShooterSpeed(Model , StaticVariables.ballPlacementPos);

                    Position2D ballLoc = Model.BallState.Location;
                    Position2D shootTarget = new Position2D();
                    if (ballLoc.X >GameParameters.OurLeftCorner.X - 0.1)
                    {
                        if (ballLoc.DistanceFrom(GameParameters.OurRightCorner)< ballLoc.DistanceFrom(GameParameters.OurLeftCorner))
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize(-angleTreshInDegree * Math.PI/180,1);
                            shootTarget = ballLoc + shootVec;
                        }
                        else
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize(angleTreshInDegree * Math.PI / 180, 1);
                            shootTarget = ballLoc + shootVec;
                        }
                    }
                    else if (ballLoc.X < -(GameParameters.OurLeftCorner.X - 0.1))
                    {
                        if (ballLoc.DistanceFrom(GameParameters.OppRightCorner) < ballLoc.DistanceFrom(GameParameters.OppLeftCorner))
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize((-180-angleTreshInDegree) * Math.PI / 180, 1);
                            shootTarget = ballLoc + shootVec;
                        }
                        else
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize((-180+angleTreshInDegree) * Math.PI / 180, 1);
                            shootTarget = ballLoc + shootVec;
                        }
                    }
                    else if (ballLoc.Y > (GameParameters.OurLeftCorner.Y - 0.1) )
                    {
                        if (ballLoc.X>0)
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize((90 + angleTreshInDegree) * Math.PI / 180, 1);
                            shootTarget = ballLoc + shootVec;
                        }
                        else if (ballLoc.X<0)
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize((90 - angleTreshInDegree) * Math.PI / 180, 1);
                            shootTarget = ballLoc + shootVec;
                        }
                    }
                    else if (ballLoc.Y < -(GameParameters.OppLeftCorner.Y - 0.1))
                    {
                        if (ballLoc.X > 0)
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize((-90 - angleTreshInDegree) * Math.PI / 180, 1);
                            shootTarget = ballLoc + shootVec;
                        }
                        else if (ballLoc.X < 0)
                        {
                            Vector2D shootVec = Vector2D.FromAngleSize((-90 + angleTreshInDegree) * Math.PI / 180, 1);
                            shootTarget = ballLoc + shootVec;
                        }
                    }
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, false);
                    GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, shootTarget, false, 0.08, true);
                    Planner.AddKick(RobotID, kickPowerType.Speed, false, speed);
                    return;
                }
                if (Model.OurRobots[OtherRobot].Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.20 /*|| Model.BallState.Speed.Size > 0.2*/)
                {
                    double dist, boarder;
                    if (GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.20, out dist, out boarder)
                           || GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.20, out dist, out boarder))
                    {
                        //activeSkill.SetAvoidDangerZone(false, false);
                        GetSkill<GetBallSkill>().SetAvoidDangerZone(false, false);
                    }

                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, false);
                    GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, StaticVariables.ballPlacementPos, false, 0.2, true);
                    //Planner.AddKick(RobotID, kickPowerType.Speed, false, speed);
                    return;

                }
                else
                {
                    double dist, boarder;
                    if (GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.20, out dist, out boarder)
                           || GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.20, out dist, out boarder))
                    {
                        // activeSkill.SetAvoidDangerZone(false, false);
                        GetSkill<GetBallSkill>().SetAvoidDangerZone(false, false);
                    }
                    GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, StaticVariables.ballPlacementPos, false, 0.08, true);
                    Planner.AddKick(RobotID, kickPowerType.Speed, false, speed);

                    Vector2D vec1 = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value, 1);
                    Vector2D vec2 = (StaticVariables.ballPlacementPos - Model.BallState.Location);
                    if (Math.Abs(Vector2D.AngleBetweenInDegrees(vec1, vec2)) < 20)
                    {
                        DrawingObjects.AddObject(new StringDraw(Vector2D.AngleBetweenInDegrees(vec1, vec2).ToString(), Color.Red, Position2D.Zero.Extend(2, 0)));
                        Planner.AddKick(RobotID, kickPowerType.Speed, false, speed);
                    }
                    return;

                }
            }
            else if (CurrentState == (int)states.positioning)
            {
                Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
                target = (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh));
                // target = (Mode == 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                angle = (StaticVariables.ballPlacementPos - target).AngleInDegrees;
                avoidBall = true;
                avoidRobot = true;
                backBall = behindBallTresh;
                // backBall = 0.1;
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 6, 1);
            }
            else if (CurrentState == (int)states.eatBall)
            {
                Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
                target = (Model.BallState.Location + vec1.GetNormalizeToCopy(eatBallTresh));
                // target = (Mode == 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(eatBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(eatBallTresh));
                OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                angle = (StaticVariables.ballPlacementPos - target).AngleInDegrees;
                avoidBall = false;
                avoidRobot = false;
                backBall = eatBallTresh;
                //backBall = 0.1;
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 6, 1);

            }
            else if (CurrentState == (int)states.moveBall)
            {
                Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
                target = (StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(eatBallTresh));
                // target = (Mode == 0) ? (StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(eatBallTresh)) : (StaticVariables.ballPlacementPos - vec1.GetNormalizeToCopy(eatBallTresh));
                OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                angle = (StaticVariables.ballPlacementPos - target).AngleInDegrees;
                avoidBall = false;
                avoidRobot = false;
                backBall = eatBallTresh;
                // backBall = 0.1;
                double dist = Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos);
                //if (dist >= 0.15 && dist < 10 && stopCounter++ < 120 )
                //{
                //Planner.Add(RobotID,Model.OurRobots[RobotID]);
                //return;
                //}
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 6, 0.5);

            }
            else if (CurrentState == (int)states.finish)
            {
                if (Model.BallConfidenc < 0.5)
                {
                    Vector2D vec22 = Model.OurRobots[RobotID].Location - StaticVariables.ballPlacementPos;
                    target = StaticVariables.ballPlacementPos + vec22.GetNormalizeToCopy(finishTresh);
                    angle = (Model.BallState.Location - target).AngleInDegrees;
                }
                else if (Model.BallConfidenc >= 0.5)
                {
                    Vector2D vec22 = Model.OurRobots[RobotID].Location - Model.BallState.Location;
                    target = Model.BallState.Location + vec22.GetNormalizeToCopy(finishTresh);
                    angle = (Model.BallState.Location - target).AngleInDegrees;
                }
                if (!GameParameters.IsInField(target, -RobotParameters.OurRobotParams.Diameter))
                {
                    Position2D TempTarget = new Position2D();
                    List<Position2D> poses = new List<Position2D>();
                    for (double i = -Math.PI; i < Math.PI; i += Math.PI / 2)
                    {
                        Vector2D extendVec = Vector2D.FromAngleSize(i, 0.5);
                        poses.Add(TempTarget + extendVec);
                    }
                    foreach (var item in poses)
                    {
                        if (Model.OurRobots.Where(o => o.Value.Location.DistanceFrom(item) < 0.12).ToList().Count < 1
                            && Model.Opponents.Where(o => o.Value.Location.DistanceFrom(item) < 0.12).ToList().Count < 1)
                        {
                            TempTarget = item;
                            break;
                        }
                    }
                    TempTarget = Model.BallState.Location + (Model.BallState.Location - TempTarget).GetNormalizeToCopy(0.1);
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, false);
                    GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, TempTarget, false, TempTarget.DistanceFrom(Model.BallState.Location), true);
                    return;
                }

                avoidBall = true;
                avoidRobot = true;
                //backBall = finishTresh;
                //backBall = 0.1;
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 6, 0.5);
                Planner.Add(RobotID, target, angle, PathType.UnSafe, avoidBall, avoidRobot, false, false, false);
                return;
            }

            // activeSkill.SetAvoidDangerZone(false, false);
            GetSkill<GetBallSkill>().SetAvoidDangerZone(false, false);
            GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, StaticVariables.ballPlacementPos, false, backBall, true);
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            //if (myOtherID == -1)
            //{
            //    return;
            //}
            bool moveFailed = Model.BallConfidenc > 0.9 && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > behindBallTresh && StaticVariables.ballPlacementPos.DistanceFrom(Model.BallState.Location) > .10;
            bool moveFinished = Model.BallConfidenc > 0.5 && target.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.10;

            if (CurrentState == (int)states.pass)
            {
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 1)
                {
                    CurrentState = (int)states.positioning;
                }
            }
            else if (CurrentState == (int)states.positioning)
            {
                Vector2D vec1 = (StaticVariables.ballPlacementPos - Model.BallState.Location);
                Vector2D vec2 = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value, 1);
                double ballSpeedTresh = .3;
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 1)
                {
                    CurrentState = (int)states.pass;
                }
                else if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < 0.12 && Model.BallState.Speed.Size < ballSpeedTresh
                     && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.10/* && Math.Abs(Vector2D.AngleBetweenInDegrees(vec1, vec2)) < 10*/)
                {
                    CurrentState = (int)states.eatBall;
                }
                else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) <= 0.1)
                {
                    CurrentState = (int)states.finish;
                }
            }
            else if (CurrentState == (int)states.eatBall)
            {
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 1)
                {
                    CurrentState = (int)states.pass;
                }
                else if (Model.BallConfidenc < 0.08 || target.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.08)
                {
                    CurrentState = (int)states.moveBall;
                }
                //else if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > 0.25)
                //{
                //    CurrentState = (int)states.positioning;
                //}
                else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.10)
                {
                    CurrentState = (int)states.finish;
                }
            }
            else if (CurrentState == (int)states.moveBall)
            {
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= .12)
                {
                    CurrentState = (int)states.pass;
                }
                else if (moveFailed)
                {
                    CurrentState = (int)states.positioning;
                }
                else if (moveFinished)
                {
                    CurrentState = (int)states.positioning;
                }
                else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1 || (Model.BallConfidenc < 0.09 && Model.OurRobots[RobotID].Location.DistanceFrom(target) < 0.1))
                {
                    counter++;
                    if (counter >= 20)
                    {
                        CurrentState = (int)states.finish;
                        counter = 0;
                    }


                }
                //else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1 && ++counter >= 60)
                //{
                //    CurrentState = (int)states.finish;
                //    counter = 0;
                //}
                else if (Model.BallConfidenc < 0.5 && Model.OurRobots[RobotID].Location.DistanceFrom((StaticVariables.ballPlacementPos + (Model.BallState.Location - StaticVariables.ballPlacementPos).GetNormalizeToCopy(eatBallTresh))) < 0.1)
                {
                    CurrentState = (int)states.finish;
                }
            }
            else if (CurrentState == (int)states.finish)
            {
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 0.6)
                {
                    CurrentState = (int)states.pass;
                }
                else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 0.15)
                {
                    CurrentState = (int)states.positioning;
                }
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new BallPalcementShooter() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
        enum states
        {
            //ReachBehindBall,
            pass=1,
            positioning=2,
            eatBall=3,
            moveBall=4,
            finish=0
        }
        public double CalShootSpeed(WorldModel model , Position2D posToShoot)
        {
            Vector2D vec = model.BallState.Location - posToShoot;
            return Map(vec.Size , 3 , 9 , 3 , 4.5);
        }
        double Map(double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return Math.Min(toSource - fromSource, Math.Max(0, (value - fromSource))) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        public void Reset()
        {
            //catchSkill = new StarkCatchSkill();
            CurrentState = 0;
        }
    }
}
