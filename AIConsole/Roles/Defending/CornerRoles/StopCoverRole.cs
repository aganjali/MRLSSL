using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles.Defending
{
    class StopCoverCornerRole : RoleBase
    {
        Position2D target = new Position2D();
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();

        public SingleWirelessCommand Stop(GameStrategyEngine engine, WorldModel Model, int RobotID)
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
            var list = engine.GameInfo.OppTeam.Scores.OrderByDescending(v => v.Value);
            int oppid = list.First().Key;
            Vector2D r = ballState.Location - Model.Opponents[oppid].Location;

            Position2D ret = ballState.Location + r.GetNormalizeToCopy(0.6);



            if (GameParameters.IsInField(ret, 0))
                target = ret;

            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, target, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1.5, true);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Stop(engine, Model, RobotID);
            return Model.OurRobots[RobotID].Location.DistanceFrom(target);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new StopCoverCornerRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
