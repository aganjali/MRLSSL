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

        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, int Mode)
        {
            GetBallSkill activeSkill = new GetBallSkill();
            DrawingObjects.AddObject(new StringDraw("CurrentState= " + (states)CurrentState, "bpcatcher_state", Model.OurRobots[RobotID].Location + new Vector2D(1, 1)));
            var speed = Math.Min(Math.Max(0.9, 0.5 * Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos)), 5);
            if (CurrentState == (int)states.pass)
            {

                if ((Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.20 && Model.BallState.Speed.Size > 0.5)
                    /* && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.6 */|| Model.BallState.Speed.Size > 1)
                {

                    catchSkill.perform(engine, Model, RobotID, false, StaticVariables.ballPlacementPos, false, 60, 0.2, 0.15);
                    return;
                }
                else
                {
                    Vector2D vec1 = (StaticVariables.ballPlacementPos - Model.BallState.Location);
                    target = StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(0.15);
                    DrawingObjects.AddObject(new Circle(target, 0.1, new Pen(Color.Black, 0.01f)), "ci");
                    DrawingObjects.AddObject(new Line(StaticVariables.ballPlacementPos, Model.BallState.Location, new Pen(Color.Black, 0.01f)), "cwi");
                    //target = StaticVariables.ballPlacementPos;
                    angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    avoidBall = true;
                    avoidRobot = true;
                }
            }
            else if (CurrentState == (int)states.waitForBall)
            {
                //Vector2D vec1 = StaticVariables.ballPlacementPos - Model.OurRobots[myOtherID].Location;
                //target = (StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(0.09));
                //angle = (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees;
                Vector2D vec1 = StaticVariables.ballPlacementPos - Model.BallState.Location;
                target = StaticVariables.ballPlacementPos + vec1.GetNormalizeToCopy(0.15);
                angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                avoidBall = false;
                avoidRobot = false;
                if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1)
                {
                    Planner.Add(RobotID, Model.OurRobots[RobotID]);
                    return;
                }
            }
            else if (CurrentState == (int)states.finish)
            {
                if (Model.BallConfidenc<0.5)
                {
                    Vector2D vec2 = (Model.OurRobots[RobotID].Location - StaticVariables.ballPlacementPos);
                    target = StaticVariables.ballPlacementPos + vec2.GetNormalizeToCopy(0.5);
                    angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                }
                else if (Model.BallConfidenc>=0.5)
                {
                    Vector2D vec2 = (Model.OurRobots[RobotID].Location - Model.BallState.Location);
                    target = Model.BallState.Location + vec2.GetNormalizeToCopy(0.5);
                    angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                }
               
               
                DrawingObjects.AddObject(new Circle(target, 0.1, new Pen(Color.Blue, 0.01f)), "cvi");
                // DrawingObjects.AddObject(new Line(StaticVariables.ballPlacementPos, Model.BallState.Location, new Pen(Color.Blue)), "cwvi");


                //DrawingObjects.AddObject(new StringDraw("CurrentState= finish", new Position2D(4.5 + 0.2 * Mode, 5)), "faisnarl");
                //Vector2D vec1 = (Model.BallState.Location - StaticVariables.ballPlacementPos).GetNormalizeToCopy(0.30);
                //target = (StaticVariables.ballPlacementPos - vec1);
                //GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, target, false, .35, true);
                //angle = (StaticVariables.ballPlacementPos - Model.OurRobots[RobotID].Location).AngleInDegrees;
                avoidBall = true;
                avoidRobot = true;

            }
            //DrawingObjects.AddObject(new Line(StaticVariables.ballPlacementPos, target, new Pen(Color.Red, 0.02f)), "jdv");

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
                    if (counter >= 70)
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
                    if (counter >= 30)
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
                else if (Model.BallConfidenc > 0.8 && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) >= 0.1)
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
