using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.GameDefinitions;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.Planning.GamePlanner.Types;


using NormalSharedState = MRL.SSL.AIConsole.Engine.NormalSharedState;
using NewActiveParameters = MRL.SSL.GameDefinitions.ActiveParameters.NewActiveParameters;
using AtackerInfo = MRL.SSL.AIConsole.Engine.NormalSharedState.NewAttackerInfo;
using ActiveInfo = MRL.SSL.AIConsole.Engine.NormalSharedState.NewActiveInfo;
using ActiveRoleState = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveRoleState;
using MRL.SSL.Planning.GamePlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class NewPenaltyShooterRole : RoleBase
    {
        int? OppGoalerID = null;
        int counter = 0;

        double lastM = double.MinValue;
        Position2D target2Kick;
        Position2D target;
        double backBall = 0.1;
        Position2D chipTarget = new Position2D();

        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            if (CurrentState == (int)PenaltyStates.findball)
            {
                Position2D intit = new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y + 0.15);
                Position2D backball = Model.BallState.Location - (intit - Model.BallState.Location).GetNormalizeToCopy(0.6);
                double teta = (GameParameters.OppGoalCenter - Model.BallState.Location).AngleInDegrees;
                Planner.IsStopBall(true);
                GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, backball, teta, false, true, true, 0, 0, 3);
            }
            else
            {
                Planner.IsStopBall(false);

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.15)
                {
                    backBall = 0.08;
                    if (CurrentState == (int)PenaltyStates.Shoot)
                    {

                        //TODO: use speed good boy ;)
                        Planner.AddKick(RobotID, kickPowerType.Speed, Program.MaxKickSpeed, false, false);
                    }
                    else if (CurrentState == (int)PenaltyStates.dribble)
                    {
                        Planner.AddKick(RobotID, kickPowerType.Power, 60, false, false);
                    }
                    else if (CurrentState == (int)PenaltyStates.Accuracy)
                    {
                        backBall = 0.15;
                        Planner.AddKick(RobotID, kickPowerType.Power, 0, false, false);
                    }
                    else if (CurrentState == (int)PenaltyStates.isChip)
                    {
                        if (Model.BallState.Location.Y > 0)
                        {
                            chipTarget = GameParameters.OppLeftCorner;
                        }
                        else if (Model.BallState.Location.Y < 0)
                        {
                            chipTarget = GameParameters.OppRightCorner;
                        }
                        Planner.AddKick(RobotID, kickPowerType.Speed, 0.7, true, false);
                        //backBall = 0.08;
                    }
                }
                GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
                if (CurrentState == (int)PenaltyStates.isChip)
                {
                    GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, chipTarget, false, backBall);
                    
                }
                else
                GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, target2Kick, false, backBall);
            }
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (Model.Status == GameStatus.Penalty_OurTeam_Waiting)
                CurrentState = (int)PenaltyStates.findball;
            if (CurrentState == (int)PenaltyStates.findball)
            {
                double min = double.MaxValue;
                foreach (int OppID in Model.Opponents.Keys)
                {
                    if (Model.Opponents[OppID].Location.DistanceFrom(GameParameters.OppGoalCenter) < min)
                    {
                        min = Model.Opponents[OppID].Location.DistanceFrom(GameParameters.OppGoalCenter);
                        OppGoalerID = OppID;
                    }
                }
            }
            if (Model.Status == GameStatus.Penalty_OurTeam_Go)
            {
                DrawingObjects.AddObject(new StringDraw("is go ", new Position2D(-1,0)));
                bool Suitable4Kick = IsSuitable4Kick(engine, Model, RobotID, true, 10, 10, Program.MaxKickSpeed, out target2Kick);
                //if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.7)
                //    counter++;
                //if (counter > 60)
                //{
                if (OppGoalerID.HasValue && Model.Opponents[OppGoalerID.Value].Location.DistanceFrom(Model.BallState.Location) < 1 && Model.BallState.Location.X > -1.5
                    && (((Model.BallState.Location - GameParameters.OppGoalCenter).GetNormnalizedCopy()).InnerProduct((Model.BallState.Location - Model.Opponents[OppGoalerID.Value].Location).GetNormnalizedCopy()) > 0))

                    CurrentState = (int)PenaltyStates.isChip;
                else if (Model.BallState.Location.X < -1.5 && Suitable4Kick)
                    CurrentState = (int)PenaltyStates.Shoot;
                else if (Model.BallState.Location.X < -1.5)
                {
                    CurrentState = (int)PenaltyStates.Accuracy;

                }
                else
                    CurrentState = (int)PenaltyStates.dribble;
                //}
            }

            DrawingObjects.AddObject(new StringDraw(((PenaltyStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location.Extend(0.15, 0)), "starteemds");
            DrawingObjects.AddObject(new Circle(target2Kick, 0.03, new Pen(Color.OrangeRed, 0.01f)), "starteghtru7emds");
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return GameParameters.OppGoalCenter.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new PenaltyShooterRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        private PassPointData CalculateDribbleScore2(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D From, Position2D Target, int? opp2Ex)
        {
            int count = 2;
            double step = Math.PI / (count + 1);
            double angle = Math.PI / 2, length = 1;
            double[] scores = new double[count];
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
            PassPointData minP = new PassPointData(Position2D.Zero, double.MinValue, PassType.Drible);
            for (int i = 0; i < count; i++)
            {
                angle += step;
                angle = GameParameters.AngleModeR(angle);
                Vector2D v = Vector2D.FromAngleSize(angle, length);
                Position2D p = From + v;
                double a = obs.Meet(new SingleObjectState(From, Vector2D.Zero, 0), new SingleObjectState(p, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi) ? 0.5 : 1;
                double b = GameParameters.IsInField(p + v.GetNormalizeToCopy(1), 0) ? 1 : -1;
                double minDist = double.MaxValue;
                foreach (var item in Model.Opponents)
                {
                    if (opp2Ex.HasValue && item.Key == opp2Ex.Value)
                        continue;
                    double d = item.Value.Location.DistanceFrom(p);
                    if (d < minDist)
                        minDist = d;
                }
                if (minDist < RobotParameters.OurRobotParams.Diameter + 0.1)
                    minDist = 0;
                var intervals = engine.GameInfo.GetVisibleIntervals(Model, p, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                double c = 0.1;
                double maxW = double.MinValue;
                int idx = -1;
                for (int j = 0; j < intervals.Count; j++)
                {
                    var item = intervals[j];
                    if (Math.Abs(item.interval.Start - item.interval.End) > maxW)
                    {
                        idx = j;
                        maxW = Math.Abs(item.interval.Start - item.interval.End);
                    }
                }
                if (idx > -1)
                {
                    c = Math.Min(Math.Abs(GameParameters.OppGoalRight.Y - GameParameters.OppGoalLeft.Y), Math.Abs(intervals[idx].interval.Start - intervals[idx].interval.End));
                    c = c / Math.Abs(GameParameters.OppGoalRight.Y - GameParameters.OppGoalLeft.Y);
                    c += 0.1;
                    c = Math.Min(c, 1);
                }
                minDist = Math.Min(3.0, minDist) / 3.0;
                double s = a * b * minDist * c;
                if (s > minP.score)
                {
                    minP.score = s;
                    minP.pos = p;
                }
            }

            return minP;
        }

        bool IsSuitable4Kick(GameStrategyEngine engine, WorldModel Model, int RobotID, bool angleCorrection, double Tolerance, double acc, double kickSpeed, out Position2D KickTarget)
        {
            List<VisibleGoalInterval> interval = engine.GameInfo.OppGoalIntervals;
            VisibleGoalInterval inter = new VisibleGoalInterval();
            bool flag = false;
            double max = double.MinValue;
            KickTarget = GameParameters.OppGoalCenter;
            foreach (var item in interval)
            {
                if (Math.Abs(item.interval.Start - item.interval.End) > max)
                {
                    inter = item;
                    max = Math.Abs(item.interval.Start - item.interval.End);
                    flag = true;
                }
            }

            double beta = 1 - Math.Min(acc / Tolerance, 1);

            if (flag)
                KickTarget = new Position2D(GameParameters.OppGoalCenter.X, inter.interval.Start + Math.Abs(inter.interval.Start - inter.interval.End) / 2);

            double g0 = ((KickTarget - Model.BallState.Location).AngleInDegrees - Tolerance / 2.0);
            double g1 = ((KickTarget - Model.BallState.Location).AngleInDegrees + Tolerance / 2.0);
            double robotAng = Model.OurRobots[RobotID].Angle.Value;
            double m = Math.Min(GameParameters.AngleModeD(robotAng - g0), GameParameters.AngleModeD(g1 - robotAng));
            bool res = true;
            if (m < 0)
                res = false;
            else if (m > beta * GameParameters.AngleModeD(g1 - g0) / 2)
                res = true;
            else if (m > lastM)
                res = false;
            lastM = m;
            return res;
        }

        enum PenaltyStates
        {
            findball,
            Shoot,
            Accuracy,
            isChip,
            dribble
        }
    }
}