using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class RearStopRole : RoleBase
    {
        public static bool firstistrue = true;
        public float firstangle = 0;
        public static bool recieved = false;
        public Position2D firstballstate = new Position2D();
        public Position2D firstRobotLocation = new Position2D();
        public float firstOppAngle = 0;
        public int counter = 0;
        public static bool realChip = false;
        public static bool initialangle = true;
        public static bool goActive = false;
        int OppId = 0;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D target = new Position2D();
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            DrawingObjects.AddObject(new Circle(Model.BallFallingPoint, .15), "1653132121313");
            
            if (CurrentState == (int)CoverStates.Active)
            {
                FreekickDefence.StopToActive = true ;
            }


            else if (CurrentState == (int)CoverStates.InfrontSupport)
            {
                Vector2D leftVec = GameParameters.OurGoalLeft - firstballstate;
                Vector2D rightVec = GameParameters.OurGoalRight - firstballstate;
                double anglebet = Math.Abs(Vector2D.AngleBetweenInRadians(leftVec, rightVec));                                       
                double R = (.18) / anglebet+ 0.5;//diameter + Threshold

                target = firstballstate + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - firstballstate).AngleInRadians, R);

            }

            else if (CurrentState ==(int)CoverStates.Movement)
            {
                Vector2D leftVec = GameParameters.OurGoalLeft - firstballstate;
                Vector2D rightVec = GameParameters.OurGoalRight - firstballstate;
                double anglebet = Math.Abs(Vector2D.AngleBetweenInRadians(leftVec, rightVec));
                double R = (.18) / anglebet;//diameter + Threshold

                Vector2D Oppsangle = Vector2D.FromAngleSize(firstOppAngle * Math.PI / 180, R);
                target = firstballstate + Oppsangle;
            }
            Planner.Add(RobotID, target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees , PathType.UnSafe, true, true, true, true);
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            OppId = Model.Opponents.OrderBy(o => o.Value.Location.DistanceFrom(ballState.Location)).Select(y => y.Key).FirstOrDefault();

            if (firstistrue)
            {
                firstistrue = false;
                firstballstate = Model.BallState.Location;
            }

            if (Model.BallState.Location.DistanceFrom(firstballstate) > .2)
            {
                if (initialangle)
                {
                    firstOppAngle = Model.Opponents[OppId].Angle.Value;
                    firstangle = Model.OurRobots[RobotID].Angle.Value;
                    firstRobotLocation = Model.OurRobots[RobotID].Location;
                    initialangle = false;
                }
            }
                            Vector2D leftVec = GameParameters.OurGoalLeft - firstballstate;
                Vector2D rightVec = GameParameters.OurGoalRight - firstballstate;
                double anglebet =Math.Abs( Vector2D.AngleBetweenInRadians(leftVec, rightVec));
                double R = (.18 + .05) / anglebet;//diameter + Threshold
                double idealAlpha = ((.18 + .05)/2)/R;
                double nonIdealDist = .09 + .05 + .36;//Radius + Threshold + 2*Robot Diameter it means we can catch ball when we moved from our last position// Hadi && Meisam
                double nonIdealAlpha = nonIdealDist / R;
                Vector2D robotKickerAngle =Vector2D.FromAngleSize(firstOppAngle*Math.PI/180 , 1);
                double anglebetween =Math.Abs(  Vector2D.AngleBetweenInRadians( robotKickerAngle,GameParameters.OurGoalCenter - firstballstate));


                Line PrepheadLine = new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize((Model.OurRobots[RobotID].Angle.Value * Math.PI / 180) + (Math.PI/2), 1));
                if ((PrepheadLine.Distance(Model.BallState.Location) < 0.04 ||Math.Abs( Vector2D.AngleBetweenInDegrees( Model.BallState.Speed , Vector2D.FromAngleSize(firstOppAngle * Math.PI / 180, 1)))<3 )&& FreekickDefence.BallIsMoved)
                {
                    CurrentState = (int)CoverStates.Active;
                }

            if (!initialangle && CurrentState!=(int)CoverStates.Active)
            {

                if (anglebetween < idealAlpha)
                {
                    CurrentState = (int)CoverStates.InfrontSupport;
                }
                else if(anglebetween <nonIdealAlpha)
                {
                    CurrentState = (int)CoverStates.Movement;
                    
                }
                else if(FreekickDefence.BallIsMoved)
                {
                    CurrentState = (int)CoverStates.Active;
                }
                else
                {
                    CurrentState = (int)CoverStates.InfrontSupport;
                }
            }

            //if (CurrentState == (int)CoverStates.Movement)
            //{
            //    if (Vector2D.AngleBetweenInDegrees(Model.OurRobots[RobotID].Location - firstballstate, Vector2D.FromAngleSize(firstOppAngle * Math.PI / 180, 1)) < idealAlpha)
            //    {
            //        CurrentState = (int)CoverStates.CorrectionalMovementStayOver;
            //    }
            //}
            DrawingObjects.AddObject(new StringDraw(((DefenderStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location + new Vector2D(0.25, 0)) , "67465456564654656565446");
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Vector2D leftVec = GameParameters.OurGoalLeft - firstballstate;
            Vector2D rightVec = GameParameters.OurGoalRight - firstballstate;
            double anglebet = Math.Abs(Vector2D.AngleBetweenInRadians(leftVec, rightVec));
            double R = (.18) / anglebet;//diameter + Threshold
            
            Position2D target = firstballstate + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - firstballstate).AngleInRadians, R);
            double dist = target.DistanceFrom(Model.OurRobots[RobotID].Location);
            return dist * dist;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new RearStopRole()  , new ActiveRole()};
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public enum CoverStates
        {
            InfrontSupport = 0,
            Active = 1,
            Movement = 2,
            CorrectionalMovementStayOver = 3

        }
    }
}
