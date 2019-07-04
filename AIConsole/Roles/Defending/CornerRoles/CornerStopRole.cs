using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class CornerStopRole : RoleBase
    {
        private Position2D ballInitialState = new Position2D();
        private bool firsttime = true;

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }
        Queue<double> angles = new Queue<double>(3);
        private bool decrese = false;
        private bool goActive = false;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        /// <summary>
        /// higher Bound is bigger
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="model"></param>
        /// <param name="RobotID"></param>
        /// <param name="lowerBound">1-90</param>
        /// <param name="higherBound">1-90</param>
        public void Run(GameStrategyEngine engine, WorldModel model, int RobotID, double lowerBound, double higherBound)
        {
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = model.BallState;
                ballStateFast = model.BallStateFast;
            }
            if (!goActive)
            {

                if (firsttime)
                {
                    //firsttime = false;
                    ballInitialState = ballState.Location;
                }
                int sgn = -Math.Sign(ballInitialState.Y);
                int backBallRobotID = model.Opponents.OrderBy(t => t.Value.Location.DistanceFrom(ballState.Location)).Select(u => u.Key).FirstOrDefault();
                double angleUpperbound = 0;
                double angle = 0;
                Vector2D targetVector = Vector2D.FromAngleSize((model.Opponents[backBallRobotID].Angle.Value * Math.PI) / 180, 0.60);
                bool goToAngle = false;
                Line leftLine = new Line(model.Opponents[backBallRobotID].Location + Vector2D.FromAngleSize(((model.Opponents[backBallRobotID].Angle.Value * Math.PI) / 180) - (Math.PI / 2), .05), (model.Opponents[backBallRobotID].Location + Vector2D.FromAngleSize(((model.Opponents[backBallRobotID].Angle.Value * Math.PI) / 180) - (Math.PI / 2), .05)) + targetVector);
                Line rightLine = new Line(model.Opponents[backBallRobotID].Location + Vector2D.FromAngleSize(((model.Opponents[backBallRobotID].Angle.Value * Math.PI) / 180) - (Math.PI / 2), .05), (model.Opponents[backBallRobotID].Location + Vector2D.FromAngleSize(((model.Opponents[backBallRobotID].Angle.Value * Math.PI) / 180) + (Math.PI / 2), .05)) + targetVector);
                double distFromLeft = leftLine.Distance(ballInitialState);
                double distFromRight = rightLine.Distance(ballInitialState);
                if (distFromLeft + distFromRight < 0.20 && (ballInitialState - model.Opponents[backBallRobotID].Location).InnerProduct(targetVector) > 0 && model.Opponents[backBallRobotID].Location.DistanceFrom(ballInitialState) < .35)
                {
                    goToAngle = true;

                }

                DrawingObjects.AddObject(new StringDraw((distFromLeft + distFromRight).ToString() + " < .1", ballState.Location.Extend(-1, 0)), "6546546546546");

                Vector2D robotBall = ballInitialState - model.Opponents[backBallRobotID].Location;
                Position2D target = ballInitialState + targetVector.GetNormalizeToCopy(.6);//robotBall.GetNormalizeToCopy(.6);

                angleUpperbound = (GameParameters.OurGoalCenter - ballInitialState).AngleInDegrees;
                Position2D target2 = ballInitialState + (GameParameters.OurGoalCenter - ballInitialState).GetNormalizeToCopy(.6);
                angle = robotBall.AngleInDegrees;
                Vector2D lowerboundVector = Vector2D.FromAngleSize(sgn * ((lowerBound * Math.PI) / 180), 1);
                Line lowerboundLine = new Line(ballInitialState, ballInitialState + lowerboundVector);
                DrawingObjects.AddObject(lowerboundLine, "321645463546465");
                Vector2D highboundVector = Vector2D.FromAngleSize(sgn * ((higherBound * Math.PI) / 180), 1);
                Line highboundLine = new Line(ballInitialState, ballInitialState + highboundVector);
                DrawingObjects.AddObject(highboundLine, "45654686546546456");
                double max = Math.Abs(Vector2D.AngleBetweenInDegrees(lowerboundVector, highboundVector));
                Line robotVectorLine = new Line(ballInitialState, ballInitialState + robotBall);
                DrawingObjects.AddObject(robotVectorLine);
                double difefrence = 0;
                if (goToAngle)
                {
                    double firstAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(targetVector, lowerboundVector));
                    double secondAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(targetVector, highboundVector));
                    angles.Enqueue(secondAngle);
                    if (angles.Count > 2)
                    {
                        if (angles.Last() > angles.First())
                        {
                            decrese = true;
                        }
                        angles.Dequeue();
                    }
                    if (firstAngle + secondAngle > max)
                    {
                        goToAngle = false;
                    }
                    difefrence = Math.Abs(angles.Last() - angles.First());
                }
                int sgndec = -Math.Sign(ballInitialState.Y);
                if (decrese)
                {
                    Vector2D targetvector = Vector2D.FromAngleSize(targetVector.AngleInRadians + (((difefrence > .05) ? .05 : 0.00) * sgndec), .7);
                    target = ballInitialState + targetvector;
                }
                else
                {
                    Vector2D targetvector = Vector2D.FromAngleSize(targetVector.AngleInRadians - (((difefrence > .05) ? .05 : 0.00) * sgndec), .7);
                    target = ballInitialState + targetvector;
                }
                DrawingObjects.AddObject(new Circle(target, .12f, new Pen((goToAngle) ? Brushes.DeepSkyBlue : Brushes.HotPink, .02f)), "2465464");
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 15, 10);
                Planner.AddKick(RobotID, true);
                if (goToAngle)
                {
                    Planner.Add(RobotID, target, (ballState.Location - model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                }
                else
                {
                    Planner.Add(RobotID, target2, (ballState.Location - model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, false);
                }
            }
            else
            {
                GetSkill<GetBallSkill>().Perform(engine, model, RobotID, GameParameters.OppGoalCenter);
                Planner.AddKick(RobotID, 2, kickPowerType.Speed);
            }
            if (ballState.Speed.Size > .6)
            {
                goActive = true;
            }
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            ;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new ActiveRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
    }
}
