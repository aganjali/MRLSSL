using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class MarkerRoleStatic : RoleBase
    {
        bool first = true, calculateCost = false;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        //public SingleObjectState ballStateFast = FreekickDefence.ballState;

        public void Mark(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> current)
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
            double Teta;
            DefenceInfo inf = new DefenceInfo();
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.First(f => f.RoleType == this.GetType());

            Position2D pos = CalculateTarget(engine, Model, RobotID, inf, inf.DefenderPosition.Value, inf.Teta, out Teta, FreekickDefence.LastOppToMark);
            FreekickDefence.PreviousPositions[typeof(MarkerRoleStatic)] = pos;
            Planner.Add(RobotID, pos, Teta, PathType.UnSafe, false, true, true, false);

        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            DefenceInfo inf = new DefenceInfo();
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.First(f => f.RoleType == this.GetType());
            if (first)
            {
                //  if (!FreekickDefence.LastOppToMark.HasValue || CurrentState == (int)MarkerState.CoverOurGoal)
                FreekickDefence.LastOppToMark = inf.OppID;
                first = false;
            }
            if (CurrentState == (int)MarkerState.CoverOurGoal)
            {
                if (StaticMarkerDefenceUtils.PassedToOpponent(Model, RobotID, FreekickDefence.LastOppToMark))
                    CurrentState = (int)MarkerState.CoverOppRobot;
            }
            else if (CurrentState == (int)MarkerState.CoverOppRobot)
            {
                if (!StaticMarkerDefenceUtils.PassedToOpponent(Model, RobotID, FreekickDefence.LastOppToMark))
                {
                    CurrentState = (int)MarkerState.CoverOurGoal;
                }
            }
            if (!calculateCost)
            {
                if (!FreekickDefence.LastOppToMark.HasValue || CurrentState == (int)MarkerState.CoverOurGoal)
                    FreekickDefence.LastOppToMark = inf.OppID;
                FreekickDefence.CurrentStates[this] = CurrentState;
            }

        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            DefenceInfo inf = new DefenceInfo();
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
            {

                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
                first = false;
                calculateCost = true;
                // DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles;
                CurrentState = (FreekickDefence.CurrentStates.ContainsKey(this)) ? FreekickDefence.CurrentStates[this] : CurrentState;
                double Teta;
                Position2D target = CalculateTarget(engine, Model, RobotID, inf, inf.DefenderPosition.Value, inf.Teta, out Teta, FreekickDefence.LastOppToMark);
                return target.DistanceFrom(Model.OurRobots[RobotID].Location);
            }
            return 100;
        }
        bool isInZone = false;
        Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenceInfo inf, Position2D Target, double teta, out double Teta, int? oppid)
        {
            SingleObjectState oppState = (oppid.HasValue && Model.Opponents.ContainsKey(oppid.Value)) ? Model.Opponents[oppid.Value] : ballState;
            Position2D pos = new Position2D();
            if (CurrentState == (int)MarkerState.CoverOurGoal)
            {
                Teta = (oppState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                pos = Target;
            }
            else
            {
                Position2D postosee = new Position2D(GameParameters.OppGoalCenter.X, -1 * Math.Sign(ballState.Location.Y) * 2);
                Teta = (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                pos = oppState.Location + (ballState.Location - oppState.Location).GetNormalizeToCopy(StaticMarkerDefenceUtils.DistCutPassFromOpp);
            }
            double Mindist = GameParameters.SafeRadi(new SingleObjectState(pos, Vector2D.Zero, 0), StaticMarkerDefenceUtils.MinDistBehindFromZone);
            bool meet = false;
            double d = pos.DistanceFrom(GameParameters.OurGoalCenter);

            if (!isInZone && d < Mindist)
            {
                isInZone = true;
            }
            else if (isInZone && d > Mindist + 0.1)
            {
                isInZone = false;
            }
            if (isInZone)
            {
                Obstacles obstacles = new Obstacles(Model);

                List<int> exclude = new List<int> { RobotID, Model.GoalieID ?? 100 };
                obstacles.AddObstacle(1, 0, 0, 0, exclude, ((inf.OppID.HasValue) ? new List<int>() { inf.OppID.Value } : null));

                meet = obstacles.Meet(Model.OurRobots[RobotID], new SingleObjectState(GameParameters.OurGoalCenter, Vector2D.Zero, null), 0.07);
                if (meet)
                {
                    pos = (pos - GameParameters.OurGoalCenter).GetNormalizeToCopy(.1) + pos;
                }
            }
            return pos;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new MarkerRoleStatic() };
            if (CurrentState == (int)MarkerState.CoverOurGoal)
                res.Add(new ActiveRole());

            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        private static DefenceInfo Mark(GameStrategyEngine engine, WorldModel Model, int oppId)
        {
            DefenceInfo def = new DefenceInfo();
            int? oppid = oppId;
            Position2D? target;

            def.OppID = oppid;

            Position2D Target = Position2D.Zero;
            SingleObjectState state;
            if (oppid.HasValue && Model.Opponents.ContainsKey(oppid.Value))
                state = Model.Opponents[oppid.Value];
            else
                state = Model.BallState;

            Vector2D oppSpeedVector = state.Speed;
            Vector2D oppOurGoalCenter = GameParameters.OurGoalCenter - state.Location;
            double innerpOppOurGoal = oppSpeedVector.InnerProduct(oppOurGoalCenter);



            double oppSpeed = state.Speed.Size;
            double minDist = GameParameters.SafeRadi(state, 0.1);

            Position2D minimum = GameParameters.OurGoalCenter + (state.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(minDist);
            Position2D maximum = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.2);
            Position2D posToGo = Position2D.Zero;

            posToGo = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.25);

            if (minimum.DistanceFrom(GameParameters.OurGoalCenter) > posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = minimum;
            else if (maximum.DistanceFrom(GameParameters.OurGoalCenter) < posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = maximum;
            else
                Target = posToGo;
            if (Target.X > GameParameters.OurGoalCenter.X || Math.Abs(Target.Y) > Math.Abs(GameParameters.OurLeftCorner.Y))
                Target = minimum;


            maximum.DrawColor = System.Drawing.Color.Blue;
            minimum.DrawColor = System.Drawing.Color.Yellow;
            DrawingObjects.AddObject(Target);
            DrawingObjects.AddObject(minimum);
            DrawingObjects.AddObject(maximum);


            target = Target;

            def.TargetState = state;
            def.DefenderPosition = target;
            def.Teta = (state.Location - target.Value).AngleInDegrees;
            return def;
        }
    }
}
