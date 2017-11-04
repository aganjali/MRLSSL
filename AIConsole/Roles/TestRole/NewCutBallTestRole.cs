using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class NewCutBallTestRole : RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID)
        {

            if (CurrentState == (int)CutBallStatus.Go)
            {
                //GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, 0, true, true);
                GetSkill<NewCutBallSkill>().CutTheBall(Model, RobotID, GameDefinitions.GameParameters.OppGoalCenter);
            }
            else
            {
                //GetSkill<RotateSkill>().Rotate(Model, RobotID);
                GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, 0, true, true);
            }
        }

     

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            var lastState = CurrentState;
            if (Model.BallState.Speed.Size > 0.05)
                CurrentState = (int)CutBallStatus.Go;
            else
                CurrentState = (int)CutBallStatus.Wait;
            if (lastState != CurrentState)
            {
                GetSkill<NewCutBallSkill>().Reset();    
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }

    enum CutBallStatus
    {
        Wait,
        Go
    }
}
