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
    class BallPalcementCatcher : RoleBase
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
        const double eatBallTresh = 0.07;
        const double finishTresh = 0.4;

        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, int OtherID, int Mode)
        {
            myOtherID = OtherID;
            GetBallSkill activeSkill = new GetBallSkill();
            var speed = Math.Min(Math.Max(0.9, 0.5 * Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos)), 5);
            if (CurrentState == (int)states.pass)
            {
                DrawingObjects.AddObject(new StringDraw("CurrentState= passca", new Position2D(4.5 + 0.2 * Mode, 5)), "passca");
                //Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
                //target = (Model.BallState.Location - vec1.GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos)));
                //angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
                //avoidBall = true;
                //avoidRobot = true;
                if ((Model.OurRobots[RobotID].Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.20 && Model.BallState.Speed.Size > 0.5 )
                    /* && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.6 */|| Model.BallState.Speed.Size > 1)
                {
                    catchSkill.perform(engine, Model, RobotID, false, StaticVariables.ballPlacementPos, false, 60, 0.2, 0.15);
                    return;
                }
                else
                {
                    // Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;

                    target = StaticVariables.ballPlacementPos;
                    //target = (Mode == 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                    //OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                    // angle = (Mode != 0) ? (target - StaticVariables.ballPlacementPos).AngleInDegrees : (StaticVariables.ballPlacementPos - target).AngleInDegrees;
                    angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
                    avoidBall = true;
                    avoidRobot = true;
                }
            }
            else if (CurrentState == (int)states.positioning)
            {
                DrawingObjects.AddObject(new StringDraw("CurrentState= positioning", new Position2D(4.5 + 0.2 * Mode, 5)), "fwainal");
                Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
                target = (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                //target = (Mode == 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                //OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                // angle = (Mode != 0) ? (target - StaticVariables.ballPlacementPos).AngleInDegrees : (StaticVariables.ballPlacementPos - target).AngleInDegrees;
                angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
                avoidBall = true;
                avoidRobot = true;
            }
            //else if (CurrentState == (int)states.eatBall)
            //{
            //    counter = 0;
            //    DrawingObjects.AddObject(new StringDraw("CurrentState= eatBall", new Position2D(4.5 + 0.2 * Mode, 5)), "fainral");
            //    Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
            //    target = (Model.BallState.Location - vec1.GetNormalizeToCopy(eatBallTresh));
            //    //target = (Mode == 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(eatBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(eatBallTresh));
            //    // OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
            //    //angle = (Mode != 0) ? (target - StaticVariables.ballPlacementPos).AngleInDegrees : (StaticVariables.ballPlacementPos - target).AngleInDegrees;
            //    angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
            //    avoidBall = false;
            //    avoidRobot = false;
            //}
            //else if (CurrentState == (int)states.moveBall)
            //{
            //    counter = 0;
            //    DrawingObjects.AddObject(new StringDraw("CurrentState= moveBall", new Position2D(4.5 + 0.2 * Mode, 5)), "fainarl");
            //    Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
            //    target = (StaticVariables.ballPlacementPos - vec1.GetNormalizeToCopy(eatBallTresh));
            //    // target = (Mode == 0) ? (StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(eatBallTresh)) : (StaticVariables.ballPlacementPos - vec1.GetNormalizeToCopy(eatBallTresh));
            //    //OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
            //    //angle = (Mode != 0) ? -(StaticVariables.ballPlacementPos - target).AngleInDegrees : (StaticVariables.ballPlacementPos - target).AngleInDegrees;
            //    angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
            //    avoidBall = false;
            //    avoidRobot = false;
            //}
            else if (CurrentState == (int)states.waitForBall)
            {
                DrawingObjects.AddObject(new StringDraw(((states)CurrentState).ToString(), new Position2D(4.5 + 0.2 * Mode, 5)), "fainarl");
                Vector2D vec1 = StaticVariables.ballPlacementPos - Model.OurRobots[myOtherID].Location;
                target = (StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(0.09));
                // target = (Mode == 0) ? (StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(eatBallTresh)) : (StaticVariables.ballPlacementPos - vec1.GetNormalizeToCopy(eatBallTresh));
                //OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(behindBallTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(behindBallTresh));
                //angle = (Mode != 0) ? -(StaticVariables.ballPlacementPos - target).AngleInDegrees : (StaticVariables.ballPlacementPos - target).AngleInDegrees;
                angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
                avoidBall = false;
                avoidRobot = false;
            }
            else if (CurrentState == (int)states.finish)
            {
                DrawingObjects.AddObject(new StringDraw("CurrentState= finish", new Position2D(4.5 + 0.2 * Mode, 5)), "faisnarl");
                Vector2D vec1 = Model.BallState.Location - StaticVariables.ballPlacementPos;
                target = (StaticVariables.ballPlacementPos - vec1.GetNormalizeToCopy(finishTresh));
                // target = (Mode == 0) ? (StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(finishTresh)) : (StaticVariables.ballPlacementPos - vec1.GetNormalizeToCopy(finishTresh));
                //OtherTarget = (Mode != 0) ? (Model.BallState.Location + vec1.GetNormalizeToCopy(finishTresh)) : (Model.BallState.Location - vec1.GetNormalizeToCopy(finishTresh));
                //angle = (Mode != 0) ? -(StaticVariables.ballPlacementPos - target).AngleInDegrees : (StaticVariables.ballPlacementPos - target).AngleInDegrees;
                angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
                avoidBall = true;
                avoidRobot = true;
            }
            DrawingObjects.AddObject(new Line(StaticVariables.ballPlacementPos, target, new Pen(Color.Red, 0.02f)), "jdv");

            Planner.SetParameter(RobotID, 0.4);
            Planner.Add(RobotID, target, angle, PathType.UnSafe, avoidBall, avoidRobot, false, false, false);
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (myOtherID == -1)
            {
                return;
            }
            bool eatFailed = Model.BallConfidenc > 0.5 && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > behindBallTresh;
            bool moveFinished = Model.BallConfidenc > 0.5 && target.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.10;

            if (CurrentState == (int)states.pass)
            {
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 1 && Model.BallState.Speed.Size < 0.3 || catchSkill.currentState == 3 || catchSkill.currentState == 4)
                {
                    CurrentState = (int)states.waitForBall;
                }
                else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1)
                {
                    counter++;
                    if (counter >= 140)
                    {
                        CurrentState = (int)states.finish;
                        counter = 0;
                    }


                }
                if (catchSkill.currentState == 3 || catchSkill.currentState == 4)
                {
                    catchSkill = new StarkCatchSkill();
                }
            }
            else if (CurrentState == (int)states.waitForBall)
            {
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 1)
                {
                    CurrentState = (int)states.pass;
                }
                else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1)
                {
                    counter++;
                    if (counter>=140)
                    {
                        CurrentState = (int)states.finish;
                        counter = 0;
                    }
                    
                    
                }
            }
            //else if (CurrentState == (int)states.positioning)
            //{
            //    double ballSpeedTresh = 0.3;
            //    if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 0.6)
            //    {
            //        CurrentState = (int)states.pass;
            //    }
            //    else if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < .08 && Model.BallState.Speed.Size < ballSpeedTresh
            //          && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.10 && counter >= 60
            //          && Model.OurRobots[myOtherID].Location.DistanceFrom(Model.BallState.Location) + Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) <= 0.4)
            //    {
            //        CurrentState = (int)states.eatBall;
            //    }
            //    else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1)
            //    {
            //        CurrentState = (int)states.finish;
            //    }
            //}
            //else if (CurrentState == (int)states.eatBall)
            //{
            //    if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.6)
            //    {
            //        CurrentState = (int)states.pass;
            //    }
            //    else if (Model.BallConfidenc < 0.5 && target.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.08)
            //    {
            //        CurrentState = (int)states.moveBall;
            //    }
            //    else if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > 0.25)
            //    {
            //        CurrentState = (int)states.positioning;
            //    }
            //    else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) <= 0.1)
            //    {
            //        CurrentState = (int)states.finish;
            //    }
            //}
            //else if (CurrentState == (int)states.moveBall)
            //{
            //    if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.6)
            //    {
            //        CurrentState = (int)states.pass;
            //    }
            //    else if (eatFailed)
            //    {
            //        CurrentState = (int)states.positioning;
            //    }
            //    else if (moveFinished)
            //    {
            //        CurrentState = (int)states.positioning;
            //    }
            //    else if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) <= 0.1)
            //    {
            //        CurrentState = (int)states.finish;
            //    }
            //    else if (Model.BallConfidenc < 0.5 && Model.OurRobots[RobotID].Location.DistanceFrom((StaticVariables.ballPlacementPos + (Model.BallState.Location - StaticVariables.ballPlacementPos).GetNormalizeToCopy(eatBallTresh))) <= 0.1)
            //    {
            //        CurrentState = (int)states.finish;
            //    }
            //}
            else if (CurrentState == (int)states.finish)
            {
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.6)
                {
                    CurrentState = (int)states.pass;
                }
                else if (Model.BallConfidenc > 0.95 && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 0.1)
                {
                    CurrentState = (int)states.waitForBall;
                }
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return StaticVariables.ballPlacementPos.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new BallPalcementCatcher() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
        enum states
        {
            //ReachBehindBall,
            pass,
            positioning,
            waitForBall,
            //eatBall,
            //moveBall,
            finish
        }

        public void Reset()
        {
            //catchSkill = new StarkCatchSkill();
            CurrentState = 0;
        }
    }
}
