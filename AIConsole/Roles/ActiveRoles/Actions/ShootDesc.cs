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
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public class ShootDesc : ActionDescriptionBase
    {
        Queue<double> ballSpeedsQueue = new Queue<double>();
        public override void DetermineActionState(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            base.DetermineActionState(engine, Model, RobotID, activeRoleState, ref actInfo);

            if (activeRoleState == NormalSharedState.ActiveRoleState.KickAnyway)
                actInfo = KickAnyWayInfo(Model, RobotID, actInfo);
            else if (activeRoleState == NormalSharedState.ActiveRoleState.Open2Kick)
                actInfo = Open2KickInfo(engine, Model, RobotID, actInfo);
            else if (activeRoleState == NormalSharedState.ActiveRoleState.Sweep)
                actInfo = SweepInfo(Model, RobotID, actInfo);
            else
                actInfo = ClearInfo(Model, RobotID, actInfo);


        }

        private NormalSharedState.ActionInfo ClearInfo(WorldModel Model, int RobotID, NormalSharedState.ActionInfo actInfo)
        {
            if (ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.Chip)
            {
                actInfo.kick = (GameParameters.OppGoalCenter - Model.BallState.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - Model.BallState.Location).Size - 0.5).Size;
                actInfo.isChip = true;
                actInfo.Target = GameParameters.OppGoalCenter;
                actInfo.strState += " Clear Chip";
                actInfo.tolerance = 20;
                actInfo.acc = 20;
            }
            else if (ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.Force)
            {
                actInfo.Target = GameParameters.OppGoalCenter;
                actInfo.kick = 1;
                actInfo.isChip = true;
                actInfo.tolerance = 180;
                actInfo.acc = 180;
                actInfo.strState += " Clear Force";
            }
            else
            {
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, Model.Opponents.Keys.ToList());
                Vector2D v = Vector2D.Zero;
                if (ActiveParameters.NewActiveParameters.kickDefult == ActiveParameters.NewActiveParameters.KickDefult.Center)
                {
                    actInfo.Target = GameParameters.OppGoalCenter;
                    v = (actInfo.Target - Model.BallState.Location).GetNormalizeToCopy(1.5);
                    bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                    actInfo.kick = (!b) ? 4 : Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.7;
                    if (Model.BallState.Location.X < 0)
                    {
                        actInfo.kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.7;
                    }
                    actInfo.isChip = b;
                    actInfo.strState += " Clear Direct Center";
                }
                else
                {
                    actInfo.Target = (Model.BallState.Location.Y < 0 ? new Vector2D(0, -0.25) : new Vector2D(0, 0.25)) + GameParameters.OppGoalCenter;
                    v = (actInfo.Target - Model.BallState.Location).GetNormalizeToCopy(1.5);
                    bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                    actInfo.kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.7;
                    if (Model.BallState.Location.X < 0)
                    {
                        actInfo.kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.7;
                    }
                    actInfo.isChip = b;
                    actInfo.strState += " Clear Direct Rear";
                }
                actInfo.tolerance = 10;
                actInfo.acc = 10;
            }
            return actInfo;
        }

        private NormalSharedState.ActionInfo SweepInfo(WorldModel Model, int RobotID, NormalSharedState.ActionInfo actInfo)
        {
            double safeRadi = 0.5;
            bool danger = false;
            actInfo.Target = Model.BallState.Location + new Vector2D(-1, 0);
            if (actInfo.minDist < safeRadi)
            {
                danger = true;
                if (Model.BallState.Location.Y > 0)
                    actInfo.Target = Model.BallState.Location - (Model.BallState.Location - new Position2D(0, -3)).GetNormalizeToCopy(1);
                else
                    actInfo.Target = Model.BallState.Location - (Model.BallState.Location - new Position2D(0, 3)).GetNormalizeToCopy(1);
            }
            actInfo.kick = 4;
            //actInfo.kick = Model.BallState.Location.DistanceFrom(;
            actInfo.isChip = true;
            if (ActiveParameters.NewActiveParameters.sweepMode == ActiveParameters.NewActiveParameters.SweepDefult.Direct)
            {
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                Vector2D ballTarget = (actInfo.Target - Model.BallState.Location).GetNormalizeToCopy(safeRadi);

                if ((danger && !obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget, Vector2D.Zero, 0), 0.03))
                    || (!danger && obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget.GetNormalizeToCopy(3), Vector2D.Zero, 0), 0.05)))
                {
                    actInfo.isChip = true;
                }
                else
                {
                    actInfo.isChip = false;
                    actInfo.kick = Program.MaxKickSpeed;
                }
            }
            actInfo.tolerance = 180;
            actInfo.acc = actInfo.tolerance;
            actInfo.strState += "Sweeping";
            return actInfo;
        }

        private NormalSharedState.ActionInfo Open2KickInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActionInfo actInfo)
        {
            SingleObjectState ball = Model.BallState;
            Vector2D angleVec = Vector2D.FromAngleSize(Math.PI * Model.OurRobots[RobotID].Angle.Value / 180.0, 1);
            Line angleLine = new Line(ball.Location, ball.Location + angleVec);
            Line goalLine = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);
            Position2D? intersectVsGoalLine = angleLine.IntersectWithLine(goalLine);

            if (NormalSharedState.CommonInfo.ActiveIsCatchingPass)
            {
                actInfo.Target = (NormalSharedState.CommonInfo.GoodPointInGoal.HasValue) ? NormalSharedState.CommonInfo.GoodPointInGoal.Value : GameParameters.OppGoalCenter;
                actInfo.kick = 0;
                actInfo.isChip = false;
                actInfo.tolerance = 180;
                actInfo.acc = actInfo.tolerance;
                if (Model.OurRobots[RobotID].Angle.Value > 90 || Model.OurRobots[RobotID].Angle.Value < -90)
                    actInfo.kick = Program.MaxKickSpeed;
                actInfo.strState += " kick the pass";
            }
            else
            {
                if (ActiveParameters.NewActiveParameters.KickInRegion)
                {
                    List<VisibleGoalInterval> intervals = engine.GameInfo.OppGoalIntervals;
                    foreach (var item in intervals)
                    {
                        Position2D ps = new Position2D(GameParameters.OppGoalCenter.X, item.interval.Start), pe = new Position2D(GameParameters.OppGoalCenter.X, item.interval.End);
                        Vector2D v1 = ps - intersectVsGoalLine.Value;
                        Vector2D v2 = pe - intersectVsGoalLine.Value;
                        if (v1.InnerProduct(v2) < 0)
                        {
                            double tmp = Math.Abs(Vector2D.AngleBetweenInDegrees(ps - ball.Location, pe - ball.Location));
                            if (tmp > ActiveParameters.NewActiveParameters.KickInRegionAcc)
                            {
                                actInfo.Target = Position2D.Interpolate(ps, pe, 0.5);
                                actInfo.tolerance = tmp;
                                actInfo.kick = Program.MaxKickSpeed;
                                actInfo.acc = actInfo.tolerance - ActiveParameters.NewActiveParameters.KickInRegionAcc;
                                actInfo.strState += " KickInRegion";
                                break;
                            }
                        }
                    }
                }
                else
                {
                    actInfo.Target = (NormalSharedState.CommonInfo.GoodPointInGoal.HasValue) ? NormalSharedState.CommonInfo.GoodPointInGoal.Value : GameParameters.OppGoalCenter;

                    Vector2D ballSpeed = -ball.Speed;
                    ballSpeedsQueue.Enqueue(ballSpeed.Size);
                    if (ballSpeedsQueue.Count > 20)
                        ballSpeedsQueue.Dequeue();

                    DrawingObjects.AddObject(new StringDraw("speed ball: " + ballSpeed.Size.ToString(), new Position2D(2, 2)), "56-985vfl");
                    DrawingObjects.AddObject(new Line(actInfo.Target, Model.OurRobots[RobotID].Location, new Pen(Color.Brown, 0.02f)), "main target pos");
                    DrawingObjects.AddObject(new Line(ball.Location, ball.Location + ballSpeed, new Pen(Color.Cyan, 0.02f)), "main targesdft pos");
                    Vector2D targetBall = actInfo.Target - ball.Location;

                    double speed = 0;
                    //foreach (var item in ballSpeedsQueue)
                    //    speed += item;
                    //speed /= ballSpeedsQueue.Count;
                    //speed = (0.09 / Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location)) * ballSpeed.Size;
                    //if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.09)
                    speed = ballSpeed.Size;
                    ballSpeed.NormalizeTo(speed);
                    Vector2D newTarget = ballSpeed + targetBall.GetNormalizeToCopy(Program.MaxKickSpeed);
                    Position2D newTargetExtended = ball.Location + newTarget;
                    Line oppGoalLine = new Line(GameParameters.OppGoalLeft, GameParameters.OppGoalRight);
                    Line newTargetLine = new Line(ball.Location, newTargetExtended);
                    Position2D? newTargetPos = oppGoalLine.IntersectWithLine(newTargetLine);
                    if (newTargetPos.HasValue)
                        actInfo.Target = newTargetPos.Value;

                    actInfo.kick = Model.BallState.Location.X > 0 ? Program.OurKickSpeed : Program.MaxKickSpeed;
                    actInfo.isChip = false;
                    actInfo.acc = ActiveParameters.NewActiveParameters.kickAccuracy;
                    actInfo.tolerance = ActiveParameters.NewActiveParameters.kickAccuracy;
                    actInfo.strState += " AccuratedKick";
                }
            }
            return actInfo;
        }

        private NormalSharedState.ActionInfo KickAnyWayInfo(WorldModel Model, int RobotID, NormalSharedState.ActionInfo actInfo)
        {
            actInfo.isChip = false;
            actInfo.kick = 0;
            actInfo.Target = GameParameters.OppGoalCenter;

            Vector2D v1 = GameParameters.OppGoalLeft - Model.BallState.Location;
            Vector2D v2 = GameParameters.OppGoalRight - Model.BallState.Location;
            Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1);

            actInfo.tolerance = Math.Abs(Vector2D.AngleBetweenInDegrees(v1, v2));
            actInfo.acc = actInfo.tolerance;

            actInfo.kick = Model.BallState.Location.X > 0 ? Program.OurKickSpeed : Program.MaxKickSpeed;
            actInfo.strState += "kickAnyWay";
            return actInfo;
        }

        public override double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            if (activeRoleState == NormalSharedState.ActiveRoleState.KickAnyway || activeRoleState == NormalSharedState.ActiveRoleState.Sweep)
                return -1;
            else if (ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.Pass && Model.BallState.Location.X > 1)
                return 100000;
            else if (activeRoleState == NormalSharedState.ActiveRoleState.Open2Kick)
                return 0 + 0.1;//Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter)*0.1;
            else if (activeRoleState != NormalSharedState.ActiveRoleState.Conflict &&
                !((ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.Pass
                && NormalSharedState.CommonInfo.AttackerID.HasValue)
                || ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.PassAndDribble))
            {
                return 0;//Math.Min(5.5, Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter)) / 5.5;
            }
            return 100000;
            //return double.MaxValue;
        }

        public override NormalSharedState.ActiveActionMode ActionCategory()
        {
            return NormalSharedState.ActiveActionMode.Shoot;
        }

        public override void ProvideActionInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            Vector2D kickVec; Position2D KickTarget;
            bool kickIsSuitable = IsSuitable4Kick(Model, RobotID, true, actInfo.tolerance, actInfo.acc, actInfo.Target, actInfo.kick, out kickVec, out KickTarget);

            NormalSharedState.ActiveInfo.isChip = actInfo.isChip;
            NormalSharedState.ActiveInfo.Target = actInfo.Target;
            NormalSharedState.ActiveInfo.KickTarget = KickTarget;

            if (kickIsSuitable)
                NormalSharedState.ActiveInfo.kickSpeed = kickVec.Size;
            else
                NormalSharedState.ActiveInfo.kickSpeed = 0;
        }

        public override void Reset()
        {
            base.Reset();
            ballSpeedsQueue = new Queue<double>();
        }
    }
}
