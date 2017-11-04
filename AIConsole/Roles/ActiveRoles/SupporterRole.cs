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
    public class SupporterRole:RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (Model.BallState.Location.X < -1)
                CurrentState = 1;
            else if (Model.BallState.Location.X > -1 && Model.BallState.Location.X < 1)
                CurrentState = 2;
            else if (Model.BallState.Location.X > 1)
                CurrentState = 3;

        }


        double extend = 0.7;
        public SingleWirelessCommand Perform(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {

            Position2D Target = (GameParameters.OurGoalCenter - Model.BallState.Location).GetNormalizeToCopy(extend) + Model.BallState.Location;
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, true, 5, true);
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Position2D Target = (GameParameters.OurGoalCenter - Model.BallState.Location).GetNormalizeToCopy(0.5) + Model.BallState.Location;
            return Target.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() {
            new ActiveRole(),new AttackerRole(),
            new SupporterRole(),new NewSupporterRole(), new DefenderMarkerNormalRole1()};
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
