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
    class DefendGotoPointRole : RoleBase
    {
        Position2D target = new Position2D();
        public SingleWirelessCommand GotoPoint(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall)
        {
            target = TargetLocation;
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, TargetLocation, Teta, AvoidObstacles, AvoidBall);
        }
        public SingleWirelessCommand GotoPointTrack(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall, Vector2D finalv)
        {
            target = TargetLocation;
            return GetSkill<GotoPointSkill>().GotoPointTrack(Model, RobotID, TargetLocation, Teta, true, false, finalv);
        }
        public SingleWirelessCommand GotoPoint(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool HasSpinBack, bool AvoidObstacles, bool AvoidBall, double ChipKick, double DirectKick, double MaxSpeed)
        {
            target = TargetLocation;
            return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, TargetLocation, Teta, HasSpinBack, AvoidObstacles, AvoidBall, ChipKick, DirectKick, MaxSpeed);
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
            return Model.OurRobots[RobotID].Location.DistanceFrom(target);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new DefendGotoPointRole(), new DefenderMarkerRole(), new ActiveRole() };
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public delegate SingleWirelessCommand MethodDelegate(params object[] args);

        public SingleWirelessCommand InvokeMyMethod(MethodDelegate method, params object[] args)
        {
            return (SingleWirelessCommand)method.DynamicInvoke(args);
        }
    }
}
