using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NormalSharedState = MRL.SSL.AIConsole.Engine.NormalSharedState;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Roles
{
    public class DibbleDesc : ActionDescriptionBase
    {
        PassPointData lastDriblePoint;
        Position2D? lastBall = null;
        public override NormalSharedState.ActiveActionMode ActionCategory()
        {
            return NormalSharedState.ActiveActionMode.Drible;

        }

        public override void DetermineActionState(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            base.DetermineActionState(engine, Model, RobotID, activeRoleState, ref actInfo);
            actInfo.PassTarget = lastDriblePoint.pos;
            actInfo.dKind = NormalSharedState.ActiveDribleKind.SpaceDrible;
            actInfo.Target = actInfo.PassTarget;
            actInfo.isChip = true;
            actInfo.kick = engine.GameInfo.DriblePower[RobotID] * 0.2;// Math.Max(Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.5, 0.5);
            actInfo.tolerance = 20;
            actInfo.acc = 15;
            actInfo.strState += " Clear Dribble ";
        }
        double lastScore;
        public override double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            if (Model.BallState.Speed.Size > 0.5 || Model.BallState.Location.X > GameParameters.OurGoalCenter.X * 0.266)
            {
                return double.MaxValue;
            }
            //return double.MaxValue;
            if (activeRoleState != NormalSharedState.ActiveRoleState.Conflict &&
             ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.PassAndDribble)
            {

                if (lastBall.HasValue && Model.BallState.Location.DistanceFrom(lastBall.Value) > 0.6)
                    lastBall = null;
                PassPointData pos = new PassPointData();
                if (lastBall.HasValue)
                {
                    pos = lastDriblePoint;
                    return lastScore;
                }
                else
                {
                    int? op2Ex = null;
                    if (actInfo.minIdx != -1 && Model.Opponents.ContainsKey(actInfo.minIdx))
                        op2Ex = actInfo.minIdx;
                    pos = engine.GameInfo.CalculateDribbleScore(Model, RobotID, Model.BallState.Location, GameParameters.OppGoalCenter, op2Ex);
                    lastDriblePoint = pos;
                    lastBall = Model.BallState.Location;
                }
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), pos.pos - Model.BallState.Location));
                double d = Model.OurRobots[RobotID].Location.DistanceFrom(pos.pos) * 0.7;
                double a = (obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(pos.pos, Vector2D.Zero, 0), RobotParameters.OurRobotParams.Diameter / 2)) ? 1 : 0.1;
                double drScore = t * d * a + 0.1;
                lastScore = drScore;
                return drScore;
            }
            //if (activeRoleState !=  NormalSharedState.ActiveRoleState.KickAnyway)
            //{
            //    return 0;
            //}
            return double.MaxValue;
        }

        public override void ProvideActionInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            Vector2D kickVec; Position2D KickTarget;
            bool kickIsSuitable = IsSuitable4Kick(Model, RobotID, !actInfo.isChip, actInfo.tolerance, actInfo.acc, actInfo.Target, actInfo.kick, out kickVec, out KickTarget);

            NormalSharedState.ActiveInfo.isChip = actInfo.isChip;
            NormalSharedState.ActiveInfo.Target = actInfo.Target;
            NormalSharedState.ActiveInfo.KickTarget = KickTarget;

            if (kickIsSuitable)
            {
                NormalSharedState.ActiveInfo.kickSpeed = kickVec.Size;
            }
            else
            {
                NormalSharedState.ActiveInfo.kickSpeed = 0;
            }
        }

        public override void Reset()
        {
            base.Reset();
            lastScore = 0;
            lastDriblePoint = new PassPointData(Position2D.Zero, double.MinValue, PassType.Drible);
            lastBall = null;
        }
    }
}
