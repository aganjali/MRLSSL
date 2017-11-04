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
    class GotoPointRole : RoleBase
    {
        Position2D target = new Position2D();
        public SingleWirelessCommand GotoPoint(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall)
        {
            target = TargetLocation;
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, TargetLocation, Teta, AvoidObstacles, AvoidBall);
        }

        int index = 0;
        public SingleWirelessCommand GotoPointPath(WorldModel Model, int RobotID, List<Position2D> Path, double TargetAngle)
        {
            if (index != Path.Count)
            {
                if (Model.OurRobots[RobotID].Location.DistanceFrom(Path[index]) < 0.2)
                {
                    index++;
                }
                return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Path[index], TargetAngle, true, true);
            }
            //else
            //{

            //    return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Path[index] + (Path[index + 1] - Path[index]).GetNormalizeToCopy(1.2), TargetAngle, true, true);
            //}

            return new SingleWirelessCommand();

        }

        public SingleWirelessCommand GotoPointTrack(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall, Vector2D finalv)
        {
            return GetSkill<GotoPointSkill>().GotoPointTrack(Model, RobotID, TargetLocation, Teta, true, false, finalv);
        }
        public SingleWirelessCommand GotoPoint(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool HasSpinBack, bool AvoidObstacles, bool AvoidBall, double ChipKick, double DirectKick, double MaxSpeed)
        {
            return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, TargetLocation, Teta, HasSpinBack, AvoidObstacles, AvoidBall, ChipKick, DirectKick, MaxSpeed);
        }
        public SingleWirelessCommand GotoPoint(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool HasSpinBack, bool AvoidObstacles, bool AvoidBall, bool AvoidZone, double KickSpeed, bool isChip)
        {
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, TargetLocation, Teta, AvoidObstacles, AvoidBall, AvoidZone, KickSpeed, isChip, HasSpinBack);
        }
        public SingleWirelessCommand GotoPoint(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool HasSpinBack, bool AvoidObstacles, bool AvoidBall, bool isChipkick, double kickspeed, double MaxSpeed)
        {
            SingleWirelessCommand swc = GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, TargetLocation, Teta, HasSpinBack, AvoidObstacles, AvoidBall, 0, 0, MaxSpeed);
            swc.isChipKick = isChipkick;
            swc.BackSensor = true;
            swc.KickSpeed = kickspeed;
            return swc;
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
            List<RoleBase> res = new List<RoleBase>() { new GotoPointRole() };
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
