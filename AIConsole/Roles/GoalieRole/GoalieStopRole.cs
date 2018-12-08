using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.CommonClasses.MathLibrary;


namespace MRL.SSL.AIConsole.Roles
{
    public class GoalieStopRole : RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();

        public SingleWirelessCommand Positioning(GameStrategyEngine engine, WorldModel Model, int RobotID)
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
            Vector2D vec = (ballState.Location - GameParameters.OurGoalCenter);
            Position2D Target = GameParameters.OurGoalCenter + vec.GetNormalizeToCopy(0.3);
            //Line l = new Line(GameParameters.OurGoalLeft.Extend(-0.15, 0), GameParameters.OurGoalRight.Extend(-0.15, 0));
            double margin = 0.14;
            Target.X = Math.Min(GameParameters.OurGoalLeft.X - margin, Target.X);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, (ballState.Location - Target).AngleInDegrees, false, false, 3.5, false);
        }
        public SingleWirelessCommand PositioningStop(GameStrategyEngine engine, WorldModel Model, int RobotID)
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
            Vector2D vec = (ballState.Location - GameParameters.OurGoalCenter);
            Position2D Target = GameParameters.OurGoalCenter + vec.GetNormalizeToCopy(0.3);
            //Line l = new Line(GameParameters.OurGoalLeft.Extend(-0.15, 0), GameParameters.OurGoalRight.Extend(-0.15, 0));
            double margin = 0.14;
            Target.X = Math.Min(GameParameters.OurGoalLeft.X - margin, Target.X);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, (ballState.Location - Target).AngleInDegrees, false, false, 1, false);
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID * 20;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new GoalieStopRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
