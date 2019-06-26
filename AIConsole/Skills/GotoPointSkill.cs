using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Skills
{
    class GotoPointSkill : SkillBase
    {
        SingleWirelessCommand SRC;
        public bool IsInFieald = true;
        private static Dictionary<int, List<Obstacle>> VirtualObstacles = new Dictionary<int, List<Obstacle>>();
        public GotoPointSkill()
        {
            SRC = new SingleWirelessCommand();
        }
        public SingleWirelessCommand GotoPoint(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool HasSpinBack, bool AvoidObstacles, bool AvoidBall, double ChipKick, double DirectKick, double MaxSpeed)
        {
            Planner.Add(RobotID, new SingleObjectState(TargetLocation, Vector2D.Zero, (float)Teta), PathType.UnSafe, AvoidBall, AvoidObstacles, IsInFieald);
            bool chip = (ChipKick > 0) ? true : false;
            Planner.AddKick(RobotID, kickPowerType.Power, (chip) ? ChipKick : DirectKick, chip, HasSpinBack);
            return SRC;
        }

        public SingleWirelessCommand GotoPoint(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall)
        {
            Planner.Add(RobotID, new SingleObjectState(TargetLocation, Vector2D.Zero, (float)Teta), PathType.UnSafe, AvoidBall, AvoidObstacles, true, true);
            return SRC;
        }



        public SingleWirelessCommand GotoPoint(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall, double MaxSpeed)
        {
            Planner.Add(RobotID, new SingleObjectState(TargetLocation, Vector2D.Zero, (float)Teta), PathType.UnSafe, AvoidBall, AvoidObstacles, IsInFieald);
            return SRC;
        }
        public SingleWirelessCommand GotoPoint(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall, double MaxSpeed, bool DontGoInDangerZone)
        {
            Planner.Add(RobotID, new SingleObjectState(TargetLocation, Vector2D.Zero, (float)Teta), PathType.UnSafe, AvoidBall, AvoidObstacles, DontGoInDangerZone);
            return SRC;
        }
        public SingleWirelessCommand GotoPoint(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall, double MaxSpeed, bool DontGoInDangerZone, bool DontGoInDangerZoneOpp, bool Spin, List<Obstacle> otherObs)
        {
            Planner.Add(RobotID, TargetLocation, Teta, PathType.Safe, AvoidBall, AvoidObstacles, DontGoInDangerZone, DontGoInDangerZoneOpp, Spin, otherObs);
            return SRC;
        }
        public SingleWirelessCommand GotoPointTrack(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall, Vector2D finalv)
        {

            return SRC;
        }


        public SingleWirelessCommand GotoPoint(WorldModel Model, int RobotID, Position2D TargetLocation, double Teta, bool AvoidObstacles, bool AvoidBall, bool AvoidZone, double KickSpeed, bool isChip, bool HasSpinBack)
        {
            Planner.Add(RobotID, new SingleObjectState(TargetLocation, Vector2D.Zero, (float)Teta), PathType.UnSafe, AvoidBall, AvoidObstacles, AvoidZone, AvoidZone);
            bool chip = isChip;
            Planner.AddKick(RobotID, kickPowerType.Speed, KickSpeed, chip, HasSpinBack);
            return SRC;
        }
    }
}
