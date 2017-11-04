using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Roles
{
    public class DiveStopRole:RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }
        public SingleWirelessCommand Positioning(GameStrategyEngine engine, WorldModel Model, int RobotID, int? oppId)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            if (oppId.HasValue)
            {
                Position2D oppLoc = Model.Opponents[oppId.Value].Location;
                Position2D ballLoc = Model.BallState.Location;
                Vector2D OppGoalVec = GameParameters.OurGoalCenter - oppLoc;
                Vector2D OppBallVec = ballLoc - oppLoc;
                Line Bisector = Vector2D.Bisector(OppGoalVec, OppBallVec, oppLoc);
               // Vector2D bisectorVec = (Bisector.Tail - Bisector.Head).GetNormalizeToCopy() ;

            }
            return SWC;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
