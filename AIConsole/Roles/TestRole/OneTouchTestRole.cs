using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    public class OneTouchTestRole:RoleBase
    {
        Position2D Tar = Position2D.Zero;
        Position2D initPos = new Position2D(-1, 2.5);
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target)
        {
            Tar = Target;
            if (CurrentState == (int) State.Stop)
            {
                Planner.Add(RobotID, initPos, 180, PathType.UnSafe, true, true, true, true);
            }
            else if (CurrentState == (int) State.Cut)
            {
               //Planner.Add(RobotID, Model.BallState.Location, (-Model.BallState.Speed).AngleInDegrees, PathType.UnSafe, true, true, true, true);

            }
            else if (CurrentState == (int) State.After)
            {
                Planner.Add(RobotID, new SingleWirelessCommand(), false);
            }
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (CurrentState == (int) State.Stop)
            {
                if (Passed(engine, Model,RobotID))
                {
                    CurrentState = (int)State.Cut;
                }
            }
            else if (CurrentState == (int) State.Cut)
            {
                if (Shooted(engine, Model,RobotID,Tar))
                {
                    CurrentState = (int)State.After;
                }
            }
            else if (CurrentState == (int) State.After)
            {
                if (Model.BallState.Speed.Size < 0.1)
                {
                    CurrentState = (int)State.Stop;
                }
            }
        }

        private bool Shooted(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Tar)
        {
            return (Model.BallState.Speed).InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) < 0 && Model.BallState.Speed.Size > 0.7;
        }

        private bool Passed(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            return ((Model.BallState.Speed).InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0 && Model.BallState.Speed.Size > 0.3);
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return double.MaxValue;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new OneTouchTestRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
        enum State
        {
            Stop = 0,
            Cut = 1,
            After = 2
        }
    }
    
}
