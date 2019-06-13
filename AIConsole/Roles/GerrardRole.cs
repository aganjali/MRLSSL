using MRL.SSL.AIConsole.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class GerrardRole : RoleBase
    {
        //int currentState = 0;
        Position2D p;
        public void Perform(GameStrategyEngine engine, WorldModel Model, int robotID)
        {
            if (CurrentState == (int)PlayMode.Attack)
            {
                if (Model.BallState.Location.X < -3)
                {

                    p = new Position2D(6 + (Model.BallState.Location.X), (Model.BallState.Location.Y) / 3);


                    Planner.Add(robotID, p, 180);

                }
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 1;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (Model.BallState.Location.X < 0)
            {
                CurrentState = (int)PlayMode.Attack;
            }
            else if (Model.BallState.Location.X > 0)
            {
                CurrentState = (int)PlayMode.Defence;
            }
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new PathTestRole() };
        }
        enum PlayMode
        {
            Attack,
            Defence
        }
    }
}
