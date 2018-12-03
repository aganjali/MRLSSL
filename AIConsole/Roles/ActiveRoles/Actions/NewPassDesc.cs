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
    class PassDesc : ActionDescriptionBase
    {
        public override NormalSharedState.ActiveActionMode ActionCategory()
        {
            return NormalSharedState.ActiveActionMode.Pass;
        }
        bool reachedBehindBall = false;
        bool passSync = false, kickIsSuitable = false;
        Position2D? lastBall = null;
        List<PassPointData> lastPassPoints = new List<PassPointData>();
        List<PassPointData> lastPassPointsSecond = new List<PassPointData>();
        PassPointData passPoint;
        PassPointData passPointSecond;
        double lastScore;
        public override void DetermineActionState(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            base.DetermineActionState(engine, Model, RobotID, activeRoleState, ref actInfo);
            int? AttackerID = NormalSharedState.CommonInfo.AttackerID;
            actInfo.PassTarget = passPoint.pos;
            actInfo.PassTargetSecond = passPointSecond.pos;
            actInfo.pKind = (passPoint.type == PassType.OT) ? NormalSharedState.ActivePassKind.OneTouch : NormalSharedState.ActivePassKind.Catch;

            double passSpeed = (!actInfo.isChip) ?
                engine.GameInfo.CalculateKickSpeed(Model, RobotID, Model.BallState.Location, actInfo.PassTarget, actInfo.isChip, actInfo.pKind == NormalSharedState.ActivePassKind.OneTouch) * 0.9 :
                Math.Max(0.6, 0.7 * Model.BallState.Location.DistanceFrom(actInfo.PassTarget));// *GamePlannerInfo.ChipCoef[RobotID];

            actInfo.kick = passSpeed;
            actInfo.tolerance = 40;
            actInfo.acc = 20;
            actInfo.strState += (" Clear Pass " + actInfo.pKind);
            actInfo.Target = GameParameters.OppGoalCenter;


            if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.25
                && Vector2D.IsBetween(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180 + Math.PI / 6, 1),
                Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180 + Math.PI / 6, 1),
                actInfo.Target - Model.BallState.Location))
                reachedBehindBall = true;

            if (AttackerID.HasValue && Model.OurRobots.ContainsKey(AttackerID.Value) && NormalSharedState.PassSyncronizer.Sync(engine, Model, RobotID, AttackerID.Value, passSpeed, actInfo.isChip, actInfo.PassTarget, ref actInfo.kick)
                && reachedBehindBall)
                passSync = true;
            //     DrawingObjects.AddObject("passspeedaftersync", new StringDraw("passspeed: "+actInfo.kick, Model.OurRobots[AttackerID.Value].Location + new Vector2D(0.5, 0.5)));
            actInfo.passSync = passSync;

        }

        public override double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            if (Model.BallState.Speed.Size > 0.9)
            {
                return double.MaxValue;
            }
            //  return double.MaxValue;
            if (activeRoleState != NormalSharedState.ActiveRoleState.Conflict && (ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.Pass
                || ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.PassAndDribble) && NormalSharedState.CommonInfo.AttackerID.HasValue)
            {
                if (lastBall.HasValue && Model.BallState.Location.DistanceFrom(lastBall.Value) > 0.6)
                    lastBall = null;
                List<PassPointData> poses = new List<PassPointData>();
                List<PassPointData> posesSecond = new List<PassPointData>();
                Obstacles obs = new Obstacles(Model);
                if (NormalSharedState.CommonInfo.AttackerID.HasValue)
                {
                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID, NormalSharedState.CommonInfo.AttackerID.Value }, null);
                }
                else
                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                actInfo.isChip = obs.Meet(Model.BallState, new SingleObjectState(passPoint.pos, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi + 0.02);

                if (lastBall.HasValue)
                {
                    poses = lastPassPoints;
                    return lastScore;
                }
                else
                {
                    //Position2D topLeft = new Position2D(-0.5, GameParameters.OurRightCorner.Y);
                    #region First Attacker Position
                    double regionX = 0;
                    if (Model.BallState.Location.X > -StaticVariables.FIELD_LENGTH_H / 2)
                    {
                        regionX = Model.BallState.Location.X;
                    }
                    else if (Model.BallState.Location.X > -StaticVariables.FIELD_LENGTH_H)
                    {
                        regionX = -(StaticVariables.FIELD_LENGTH_H - 2 * (StaticVariables.FIELD_LENGTH_H) / 3);
                    }
                    Position2D topLeft = new Position2D(regionX, GameParameters.OurRightCorner.Y);
                    double width = GameParameters.OurGoalCenter.X - 0.5 - 0.25, heigth = 2 * GameParameters.OurLeftCorner.Y, passSpeed = 4, shootSpeed = Program.MaxKickSpeed;
                    int Rows = 5, column = 10;
                    poses = engine.GameInfo.CalculatePassScore(Model, RobotID, NormalSharedState.CommonInfo.AttackerID, topLeft, passSpeed, shootSpeed, width, heigth, Rows, column);
                    #endregion
                    double mScore = double.MinValue;
                    int sgn = 0;
                    foreach (var item in poses)
                    {
                        if (item.score > mScore)
                        {
                            mScore = item.score;
                            sgn = Math.Sign(item.pos.Y);
                        }
                    }

                    topLeft = new Position2D(regionX, sgn < 0 ? 0 : GameParameters.OurRightCorner.Y);
                    width = (GameParameters.OurGoalCenter.X - 0.5 - 0.25);
                    heigth = GameParameters.OurLeftCorner.Y;

                    posesSecond = engine.GameInfo.CalculateAttackerPassScore(Model, RobotID, RobotID/*, attackerPos*/, topLeft, passSpeed, shootSpeed, width, heigth, Rows, column);

                    lastPassPoints = poses;
                    lastPassPointsSecond = posesSecond;
                    lastBall = Model.BallState.Location;
                }
                double maxSc = double.MinValue;
                foreach (var item in poses)
                {
                    if (item.score > maxSc)
                    {
                        maxSc = item.score;
                        passPoint = item;
                    }
                }
                maxSc = double.MinValue;
                foreach (var item in posesSecond)
                {
                    if (item.score > maxSc)
                    {
                        maxSc = item.score;
                        passPointSecond = item;
                    }
                }


                double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), passPoint.pos - Model.BallState.Location));
                double d = (NormalSharedState.CommonInfo.AttackerID.HasValue && Model.OurRobots.ContainsKey(NormalSharedState.CommonInfo.AttackerID.Value))
                    ? Model.OurRobots[NormalSharedState.CommonInfo.AttackerID.Value].Location.DistanceFrom(passPoint.pos) : 10;
                actInfo.isChip = obs.Meet(Model.BallState, new SingleObjectState(passPoint.pos, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi + 0.02);
                double a = (actInfo.isChip) ? 1 : 0.1;
                double drScore = t * d * a + 0.1;
                lastScore = drScore;

                return drScore;

                //double d = Model.OurRobots[NormalSharedState.CommonInfo.AttackerID.Value].Location.DistanceFrom(points[0]);
                //oc = (obs.Meet(new SingleObjectState(from, Vector2D.Zero, 0), new SingleObjectState(points[0], Vector2D.Zero, 0), MotionPlannerParameters.BallRadi)) ? 0 : 1;
                //double b = (obs.Meet(Model.OurRobots[AttackerID.Value], new SingleObjectState(points[0], Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi)) ? 0.1 : 1;
                //double a = Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - points[0], from - points[0])) > 70 ? 0 : 1;
                //double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), points[0] - from));
                //otScore = b * oc * d * a * t;

            }
            return double.MaxValue;
        }

        public override void ProvideActionInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            int? AttackerID = NormalSharedState.CommonInfo.AttackerID;
            NormalSharedState.CommonInfo.Ready2Pass = true;
            NormalSharedState.CommonInfo.PassSpeed = actInfo.kick;
            NormalSharedState.CommonInfo.ShootSpeed = Program.MaxKickSpeed;
            NormalSharedState.CommonInfo.PassTarget = actInfo.PassTarget;
            NormalSharedState.CommonInfo.ShootTarget = actInfo.Target;
            NormalSharedState.CommonInfo.PassKind = actInfo.pKind;
            NormalSharedState.CommonInfo.PassIsChip = actInfo.isChip;

            Vector2D kickVec; Position2D KickTarget;


            if (!passSync)
            {
                kickIsSuitable = IsSuitable4Kick(Model, RobotID, /*!actInfo.isChip*/false, actInfo.tolerance, actInfo.acc, actInfo.PassTarget, actInfo.kick, out kickVec, out KickTarget);
                NormalSharedState.ActiveInfo.isChip = actInfo.isChip;
                NormalSharedState.ActiveInfo.Target = actInfo.PassTarget;//------
                NormalSharedState.ActiveInfo.KickTarget = KickTarget;
            }
            else
            {
                kickIsSuitable = IsSuitable4Kick(Model, RobotID, /*!actInfo.isChip*/false, actInfo.tolerance, actInfo.acc, actInfo.PassTarget, actInfo.kick, out kickVec, out KickTarget);
                NormalSharedState.ActiveInfo.isChip = actInfo.isChip;
                NormalSharedState.ActiveInfo.Target = actInfo.PassTarget;
                NormalSharedState.ActiveInfo.KickTarget = KickTarget;
            }

            if (kickIsSuitable && AttackerID.HasValue && passSync)
            {
                NormalSharedState.ActiveInfo.kickSpeed = kickVec.Size;
                NormalSharedState.CommonInfo.Passed = true;
            }
            else
            {
                NormalSharedState.ActiveInfo.kickSpeed = 0;
                NormalSharedState.CommonInfo.Passed = false;
            }

        }

        public override void Reset()
        {
            base.Reset();
            lastScore = 0;
            reachedBehindBall = false;
            passSync = false;
            kickIsSuitable = false;
            lastBall = null;
            lastPassPoints = new List<PassPointData>();
            passPoint = new PassPointData();

        }
    }
}
