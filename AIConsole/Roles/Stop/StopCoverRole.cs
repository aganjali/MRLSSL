using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class StopCoverRole : RoleBase
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
            int oppid = 0;
            oppid = engine.GameInfo.OppTeam.Scores.OrderByDescending(v => v.Value).Select(u => u.Key).FirstOrDefault();
            //list.FirstOrDefault();// list.FirstOrDefault().Key;

            Vector2D r = Vector2D.FromAngleSize(Model.Opponents[oppid].Angle.Value * Math.PI / 180, 1);

            Position2D ret = ballState.Location + r.GetNormalizeToCopy(0.6);
            if (Math.Abs(ret.X) > 3 || Math.Abs(ret.Y) > 2)
            {
                ret = ballState.Location + (GameParameters.OurGoalCenter - ballState.Location).GetNormalizeToCopy(.6);
            }
            target = ret;
            //target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, .6);
            return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, target, (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, true, true, 0, 0, 1.5);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            int oppid = 0;
            oppid = engine.GameInfo.OppTeam.Scores.OrderByDescending(v => v.Value).Select(u => u.Key).FirstOrDefault();
            Vector2D r = ballState.Location - Model.Opponents[oppid].Location;
            Position2D ret = ballState.Location + r.GetNormalizeToCopy(0.6);
            if (Math.Abs(ret.X) > 3 || Math.Abs(ret.Y) > 2)
            {
                ret = ballState.Location + (GameParameters.OurGoalCenter - ballState.Location).GetNormalizeToCopy(.6);
            }
            double d = Model.OurRobots[RobotID].Location.DistanceFrom(ret);
            return d * d;
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
}
