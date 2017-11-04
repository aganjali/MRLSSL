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
    class PenaltyKeeperRole : RoleBase
    {
        public MRL.SSL.GameDefinitions.SingleWirelessCommand RunRole(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            Position2D Target;
            if (CurrentState == (int)GoallerState.Dive)
            {
                Target = CalculateTargetToKeep(Model);
            }
            else
            {
                Target = CalculateGoallerPosition(Model);
            }

            int? oppiD = CalculatePenaltyShooterID(Model);
            //GetSkill<GotoPointSkill>().SetController(new Vector2D(4, 4), new Vector2D(4.5, 4.5), 1, 5, 6, false);
            if (!oppiD.HasValue)
                return new SingleWirelessCommand();
            Vector2D neww = Vector2D.FromAngleSize((Model.Opponents[oppiD.Value].Angle.Value * Math.PI / 180), 1);
            Line l11 = new Line(Model.Opponents[oppiD.Value].Location, Model.Opponents[oppiD.Value].Location + neww);
            Line l22 = new Line(new Position2D(GameParameters.OurGoalRight.X - 0.1, GameParameters.OurGoalRight.Y), new Position2D(GameParameters.OurGoalLeft.X - 0.1, GameParameters.OurGoalLeft.Y));
            Position2D? interseccc = l11.IntersectWithLine(l22);
            if (interseccc.HasValue)
            {

                double y = Math.Sign(interseccc.Value.Y) * Math.Min(Math.Abs(interseccc.Value.Y), GameParameters.OurGoalLeft.Y - 0.18);
                interseccc = new Position2D(interseccc.Value.X, y);
                Planner.Add(RobotID, new SingleObjectState(interseccc.Value, Vector2D.Zero, (float)(Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees), PathType.UnSafe, false, false, false);
                //return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, interseccc.Value, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, false, false, 255, 0, 2);
            }
            else
                Planner.Add(RobotID, new SingleObjectState(GameParameters.OurGoalCenter, Vector2D.Zero, (float)(Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees), PathType.UnSafe, false, false, false);
            Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
            return new SingleWirelessCommand();
            ////Target = new Position2D(Target.X , Target.Y);
            //GetSkill<GotoPointSkill>().SetController(new Vector2D(4, 4), new Vector2D(4.5, 4.5), 1, 5, 6, false);
            //if (Target.Y > GameParameters.OurGoalLeft.Y - 0.16)
            //    return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, new Position2D(GameParameters.OurGoalLeft.X - 0.1, GameParameters.OurGoalLeft.Y - 0.16), (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, false, false, 255, 0, 2);
            //else if (Target.Y < GameParameters.OurGoalRight.Y + 0.16)
            //    return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, new Position2D(GameParameters.OurGoalRight.X - 0.1, GameParameters.OurGoalRight.Y + 0.16), (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, false, false, 255, 0, 2);
            //else
            //    return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, Target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, false, false, 255, 0, 2);
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID * 20;
        }

        public override void DetermineNextState(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (Model.Status == GameStatus.Penalty_Opponent_Go)
            {
                if (Model.BallState.Speed.Size > 0.01)
                    CurrentState = (int)GoallerState.Dive;
            }
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        enum GoallerState
        {
            NoMove,
            Left,
            Right,
            Dive

        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new PenaltyKeeperRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        private int? CalculatePenaltyShooterID(WorldModel Model)
        {
            double MinDist = 10;
            int? OppPenaltyShooterId = null;
            foreach (int OppID in Model.Opponents.Keys)
            {
                if ((GameParameters.OurGoalCenter - Model.Opponents[OppID].Location).Size < MinDist)
                {
                    MinDist = (Model.BallState.Location - Model.Opponents[OppID].Location).Size;
                    OppPenaltyShooterId = OppID;
                }
            }
            return OppPenaltyShooterId;
        }

        private Position2D CalculateGoallerPosition(WorldModel Model)
        {
            Line l1;
            if (CalculatePenaltyShooterID(Model).HasValue)
                l1 = new Line(Model.Opponents[CalculatePenaltyShooterID(Model).Value].Location, Model.BallState.Location);
            else
                l1 = new Line(Model.BallState.Location, Model.BallState.Location);
            Line l2 = new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft);
            Position2D? Intersect = l1.IntersectWithLine(l2);
            Position2D Target = new Position2D();
            if (Intersect.HasValue)
                Target = new Position2D(Intersect.Value.X - 0.08, Intersect.Value.Y - (Math.Sign(Intersect.Value.Y) * 0.11));
            else
                Target = new Position2D(GameParameters.OurGoalCenter.X - 0.08, GameParameters.OurGoalCenter.Y);
            return Target;
        }

        private Position2D CalculateTargetToKeep(WorldModel Model)
        {
            Line l1 = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed);
            Line l2 = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);

            Position2D? Target = l1.IntersectWithLine(l2);
            if (Target.HasValue)
                return Target.Value;
            return new Position2D(GameParameters.OurGoalCenter.X - 0.1, GameParameters.OurGoalCenter.Y);
        }


    }
}
