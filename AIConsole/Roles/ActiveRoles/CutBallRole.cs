using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class CutBallRole:RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            
        }

        public void CutIt(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, double KickPower, bool isChip)
        {
            
            if (Model.BallState.Speed.Size > 0.2/* && Math.Abs(Model.OurRobots[RobotID].Location.X - Model.BallState.Location.X) < 2*/)
                 GetSkill<CutBallSkill>().go(Model, RobotID);
            else
                 GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees, false, false);

        }

        public void Perform(WorldModel Model, int RobotID)
        {
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
