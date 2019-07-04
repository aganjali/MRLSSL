using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    public class DefenderStopRole2 : RoleBase
    {
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }
        public SingleWirelessCommand Positioning(GameStrategyEngine engine, WorldModel Model, int RobotID, bool isChipKick, double kickPower)
        {
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
            Position2D targ = CalculateTarget(engine, Model, RobotID);
            double teta = (ballState.Location - targ).AngleInDegrees;
            bool AvoidBall = false;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(targ) > 1)
                AvoidBall = true;
            AvoidBall = true;

            SingleWirelessCommand SWC = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, targ, teta, false, AvoidBall, 3.5, true);
            SWC.isChipKick = isChipKick;
            SWC.KickPower = kickPower;
            return SWC;
        }
        public SingleWirelessCommand PositioningStop(GameStrategyEngine engine, WorldModel Model, int RobotID, bool isChipKick, double kickPower)
        {
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
            Position2D targ = CalculateTarget(engine, Model, RobotID);
            double teta = (ballState.Location - targ).AngleInDegrees;
            bool AvoidBall = false;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(targ) > 1)
                AvoidBall = true;
            AvoidBall = true;
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            SingleWirelessCommand SWC = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, targ, teta, true, AvoidBall, StaticVariables.stopMaxSpeed, false);
            SWC.isChipKick = isChipKick;
            SWC.KickPower = kickPower;
            return SWC;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }

        public Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D Target = GameParameters.OurGoalCenter + new Vector2D(-1, 0.17);
            //Position2D ballLoc = ballState.Location;
            //Vector2D vec = ballLoc - GameParameters.OurGoalCenter;
            //double ang = Vector2D.AngleBetweenInDegrees(vec, (GameParameters.OurGoalLeft - GameParameters.OurGoalCenter));
            //if (ang < 20)
            //    ballLoc = GameParameters.OurGoalCenter + Vector2D.FromAngleSize(110 * Math.PI / 180, vec.Size);
            //else if (ang > 160)
            //    ballLoc = GameParameters.OurGoalCenter + Vector2D.FromAngleSize(-110 * Math.PI / 180, vec.Size);
            //vec = ballLoc - GameParameters.OurGoalCenter;
            //Line ballGoalLine = new Line(ballLoc, GameParameters.OurGoalCenter);
            //Vector2D tmp = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, RobotParameters.OurRobotParams.Diameter / 2 + 0.06);
            //Line defenderLine = new Line(ballGoalLine.Head + tmp, ballGoalLine.Tail + tmp);
            //Line GoalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
            //Position2D? pos = GoalLine.IntersectWithLine(defenderLine);
            //if (pos.HasValue)
            //{
            //    Target = pos.Value + (defenderLine.Head - defenderLine.Tail).GetNormalizeToCopy(1.1);
            //}
            if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1)
            {
                Target = new Position2D(1.8, -.5);
            }
            else
            {
                double Tresh = .8;
                if (ballState.Location.Y < Target.Y)
                    Tresh = 1.2;
                Circle C = new Circle(ballState.Location, Tresh);
                if (C.IsInCircle(Target))
                {
                    double d, d2;
                    Vector2D v = ballState.Location - Target;
                    if (GameParameters.IsInDangerousZone(ballState.Location, false, 0, out d, out d2))
                        Target = ballState.Location - Math.Sign(v.X) * (v.GetNormalizeToCopy(0.9));
                    else
                        Target = ballState.Location - Math.Sign(v.X) * (v.GetNormalizeToCopy(0.6));
                }
            }
            return Target;
        }
        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return CalculateTarget(engine, Model, RobotID).DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new DefenderStopRole2() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
