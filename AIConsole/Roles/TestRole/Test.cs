using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.IO;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class Test : RoleBase
    {
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {

            //return (Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter));
            return RobotID;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> l = new List<RoleBase>();
            return l;
        }
        

        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotId , double r)
        {
            GetSkill<Test1>().MoveToPoint(Model, engine, RobotId , new Position2D(2,2) , r);

        }
    }
}
