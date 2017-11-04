using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class RegionalDefenderRole2 : RoleBase, IRegionalDefender
    {
        Position2D targ = new Position2D();
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public SingleWirelessCommand positionnig(GameStrategyEngine engine, WorldModel model, int robotid, Position2D targrt, double teta)
        {
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = model.BallState;
                ballStateFast = model.BallStateFast;
            }
            targ = targrt;
            FreekickDefence.PreviousPositions[typeof(RegionalDefenderRole2)] = targ;
            return GetSkill<GotoPointSkill>().GotoPoint(model, robotid, targrt, teta, true, false, 3.5, true);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
            FreekickDefence.CurrentStates[this] = CurrentState;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (FreekickDefence.CurrentInfos.Any(f => f.RoleType == this.GetType()))
            {
                double cost = FreekickDefence.CurrentInfos.First(f => f.RoleType == this.GetType()).DefenderPosition.Value.DistanceFrom(Model.OurRobots[RobotID].Location);
                return cost * cost;
            }
            return 100;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (!FreekickDefence.switchAllMode)
            {
                List<RoleBase> res = new List<RoleBase>() {
                new RegionalDefenderRole2(),
                };
                if (FreekickDefence.freeSwitchbetweenRegionalAndMarker)
                {
                    res.Add(new NewDefenderMarkerRole2());
                    res.Add(new NewDefenderMarkerRole3());
                    res.Add(new NewDefenderMrkerRole());
                }
                if (FreekickDefence.DefenderRegionalRole2ToActive)
                {
                    res.Add(new ActiveRole());
                }
                return res;
            }
            else
            {
                List<RoleBase> res = new List<RoleBase>();
                res.Add(new DefenderCornerRole1());
                res.Add(new DefenderCornerRole2());
                res.Add(new DefenderCornerRole3());
                res.Add(new DefenderMarkerRole2());
                res.Add(new DefenderCornerRole4());
                res.Add(new DefenderMarkerRole());
                res.Add(new NewDefenderMrkerRole());
                res.Add(new NewDefenderMarkerRole2());
                res.Add(new RegionalDefenderRole());
                res.Add(new RegionalDefenderRole2());
                if (FreekickDefence.DefenderRegionalRole2ToActive)
                {
                    res.Add(new ActiveRole());
                }
                return res;
            }

        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
    }
}
