using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Roles
{
    class BallPalcementCatcher : RoleBase
    {
        
        StarkCatchSkill catchSkill = new StarkCatchSkill();
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }
        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID)
        {
            
            if (CurrentState ==(int) states.ReachBehindBall)
            {
                var angle = -( (StaticVariables.ballPlacementPos) - (Model.BallState.Location) ).AngleInDegrees;
                Planner.Add(RobotID, StaticVariables.ballPlacementPos, angle, PathType.UnSafe, true, true, false, false, false);
            }
            else if (CurrentState == (int)states.GoCatch)
            {
                catchSkill.perform(engine,Model,RobotID,false,StaticVariables.ballPlacementPos,true);
            }
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (CurrentState == (int)states.ReachBehindBall)
            {
                if (Model.OurRobots[RobotID].Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.05 || Model.BallState.Speed.Size > 0.5)
                {
                    CurrentState = (int)states.GoCatch;
                }
            }
            else if (CurrentState == (int)states.GoCatch)
            {
                
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return StaticVariables.ballPlacementPos.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
        enum states
        {
            ReachBehindBall,
            GoCatch
        }

        public void Reset()
        {
            catchSkill = new StarkCatchSkill();
            CurrentState = 0;
        }
    }
}
