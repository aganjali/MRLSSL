using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class PenaltyGoalieRole:RoleBase
    {
        Position2D Target;
        double lastAng;
        bool go = false;
        public MRL.SSL.GameDefinitions.SingleWirelessCommand RunRole(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (CurrentState == (int)GoallerState.Dive && go)
            {
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 10, 6);
                Planner.Add(RobotID, new SingleObjectState(Target, Vector2D.Zero, 180), PathType.UnSafe, false, false, false);
            }
            else if(CurrentState != (int)GoallerState.Dive)
            {
                Planner.Add(RobotID, new SingleObjectState(Target, Vector2D.Zero, (float)(Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees), PathType.UnSafe, false, false, false);
            }
            Planner.AddKick(RobotID, kickPowerType.Speed, true, 4);
            return new SingleWirelessCommand();
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID * 20;
        }
        int counter = 0;
        public override void DetermineNextState(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (CurrentState != (int)GoallerState.Dive)
            {
                go = false;
                Target = CalculateGoallerPosition(Model);
                if (Model.Status == GameStatus.Penalty_Opponent_Go)
                {
                    counter = 0;
                    CurrentState = (int)GoallerState.Dive;
                    Target = CalculateTargetToKeep(Model, RobotID);
                    lastAng = Model.OurRobots[RobotID].Angle.Value;
                }
                else
                    counter = 0;
            }
            //if (CurrentState == (int)GoallerState.Dive)
            //{

               
            //}
            if (CurrentState == (int)GoallerState.Dive)
                counter++;
            if (counter > 280 /*|| Model.BallState.Speed.Size > 0.2*/)
                go = true;
            DrawingObjects.AddObject(new StringDraw("counter: " + counter, GameParameters.OurGoalCenter + new Vector2D(0.5, 0)));
        }  

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        enum GoallerState
        {
            NoMove,
            Left,
            Right,
            Dive

        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new PenaltyKeeperRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        private int? CalculatePenaltyShooterID(WorldModel Model)
        {
            double MinDist = 10;
            int? OppPenaltyShooterId = null;
            foreach (int OppID in Model.Opponents.Keys)
            {
                if ((GameParameters.OurGoalCenter - Model.Opponents[OppID].Location).Size < MinDist)
                {
                    MinDist = (Model.BallState.Location - Model.Opponents[OppID].Location).Size;
                    OppPenaltyShooterId = OppID;
                }
            }
            return OppPenaltyShooterId;
        }

        private Position2D CalculateGoallerPosition(WorldModel Model)
        {
            Position2D res = GameParameters.OurGoalCenter.Extend(-.09,0);
            int? oppiD = CalculatePenaltyShooterID(Model);
            if (!oppiD.HasValue)
                return GameParameters.OurGoalCenter.Extend(-0.09,0);
            Vector2D neww = Vector2D.FromAngleSize((Model.Opponents[oppiD.Value].Angle.Value * Math.PI / 180), 1);
            Line l11 = new Line(Model.BallState.Location, Model.BallState.Location + neww);
            Line l22 = new Line(new Position2D(GameParameters.OurGoalRight.X - 0.2, GameParameters.OurGoalRight.Y), new Position2D(GameParameters.OurGoalLeft.X - 0.2, GameParameters.OurGoalLeft.Y));
            Position2D? interseccc = l11.IntersectWithLine(l22);
            if (interseccc.HasValue)
            {

                double y = Math.Sign(interseccc.Value.Y) * Math.Min(Math.Abs(interseccc.Value.Y), GameParameters.OurGoalLeft.Y - 0.18);
                interseccc = new Position2D(interseccc.Value.X, y);
                res = interseccc.Value;
            }
            else
                res = GameParameters.OurGoalCenter;
            return res;
        }

        private Position2D CalculateTargetToKeep(WorldModel Model, int RobotID)
        {
            Position2D res = GameParameters.OurGoalCenter;
            double dr = Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalRight);
            double dl = Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalLeft);
            Position2D p = (dr > dl) ? GameParameters.OurGoalRight : GameParameters.OurGoalLeft;
            res = (p - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(0.08) + Model.OurRobots[RobotID].Location;
            res = Position2D.Interpolate(p, res, 0.5);
            res = new Position2D(Model.OurRobots[RobotID].Location.X, res.Y);
            return res;
        }


    }
}
