using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.GameDefinitions;
using MRL.SSL.GameDefinitions.General_Settings;

namespace MRL.SSL.AIConsole.Roles
{
    class PenaltyShooterRole : RoleBase
    {
        SingleObjectState lastoppstate = null;
        Position2D initializeTarget = new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y + 0.15);
        bool initialized = false;
        public SingleWirelessCommand Perform(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID, double kickPower, Dictionary<int, RoleBase> AssignedRoles)
        {
            OppGoallerState state = CalculateGoallerState(engine, Model);
            if (!initialized)
            {
                initialized = true;
                if (GameSettings.Default.Tactic["Penalty"] == (int)PenaltyShoter.Corner)
                {
                    if (state == OppGoallerState.Left)
                        initializeTarget = new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y + 0.15);
                    else
                        initializeTarget = new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y - 0.15);
                }
                else
                {
                    initializeTarget = GameParameters.OppGoalCenter;
                }
            }
            Position2D firstTarget = Model.BallState.Location - (initializeTarget - Model.BallState.Location).GetNormalizeToCopy(0.3);
            Position2D secondTarget = Model.BallState.Location - (initializeTarget - Model.BallState.Location).GetNormalizeToCopy(0.12);

            if (CurrentState == (int)PenaltyState.ToBallFar)
            {
                //GetSkill<GotoPointSkill>().SetController(new Vector2D(0.7, 0.7), new Vector2D(2, 2), 2, 15, 8, true);
                return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, firstTarget, (initializeTarget - Model.OurRobots[RobotID].Location).AngleInDegrees, false, true, true, 0, 0, 0.5);
            }
            if (CurrentState == (int)PenaltyState.ToBallNear)
            {
                //GetSkill<GotoPointSkill>().SetController(new Vector2D(0.08, 0.08), new Vector2D(2, 2), 2, 15, 8, true);
                return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, secondTarget, (initializeTarget - Model.OurRobots[RobotID].Location).AngleInDegrees, false, true, false, 0, 0, 0.5);
                //return GetSkill<GotoPointSkill>().GotoPointInRefrence(Model, RobotID,secondTarget-Model.BallState.Location ,secondTarget, (initializeTarget - Model.OurRobots[RobotID].Location).AngleInDegrees, true, false);
            }
            else
                return GetSkill<RotateWheelsSkill>().Rotate(engine, Model, RobotID, kickPower , lastoppstate);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        int RNDTime = 0;
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            int? goalieId =engine.GameInfo.OppTeam.GoaliID; 
            if (goalieId.HasValue && Model.Opponents.ContainsKey(goalieId.Value))
            {
                lastoppstate = Model.Opponents[goalieId.Value];
            }
            else if(lastoppstate== null)
            {
                lastoppstate = new SingleObjectState(GameParameters.OppGoalLeft, Vector2D.Zero, 0);
            }
            Position2D firstTarget = Model.BallState.Location - (initializeTarget - Model.BallState.Location).GetNormalizeToCopy(0.3);
            Position2D secondTarget = Model.BallState.Location - (initializeTarget - Model.BallState.Location).GetNormalizeToCopy(0.11);
            DrawCollection dg = new DrawCollection();
            dg.AddObject(new Circle(firstTarget, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.01f)));
            dg.AddObject(new Circle(secondTarget, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.01f)));
            int Min = 0, Max = 7;

            DrawingObjects.AddObject("Penalty", dg);
            if (CurrentState == (int)PenaltyState.ToBallFar)
            {
                double dis = (firstTarget - Model.OurRobots[RobotID].Location).Size;
                if (dis <= 0.04)
                {
                    Random Rnd = new Random();
                    RNDTime = Rnd.Next(Min, Max);
                    RNDTime *= 20;
                    RNDTime += 60;
                    CurrentState = (int)PenaltyState.ToBallNear;
                }
            }
            else if (CurrentState == (int)PenaltyState.ToBallNear)
            {

                double dis = Model.OurRobots[RobotID].Location.DistanceFrom(secondTarget);
                if (dis <= 0.06 && Model.Status == GameStatus.Penalty_OurTeam_Go)
                    counter++;
                if (counter > RNDTime)
                    CurrentState = (int)PenaltyState.BackBall;
            }
            else if (CurrentState == (int)PenaltyState.BackBall)
            {
                //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotIndex].Location) > 0.078)
                //    CurrentState = (int)PenaltyState.ToBallNear;
                //else
                CurrentState = (int)PenaltyState.BackBall;
            }
            if (CurrentState != (int)PenaltyState.ToBallNear)
            {
                counter = 0;
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return GameParameters.OppGoalCenter.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new PenaltyShooterRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
        public enum PenaltyState
        {
            ToBallFar,
            ToBallNear,
            BackBall
        }

        private int? OppGoallerId(GameStrategyEngine engine, WorldModel Model)
        {
            double MinDist = 10;
            int? OppGoallerId = null;
            foreach (int OppID in Model.Opponents.Keys)
            {
                if ((GameParameters.OppGoalCenter - Model.Opponents[OppID].Location).Size < MinDist)
                {
                    MinDist = (GameParameters.OppGoalCenter - Model.Opponents[OppID].Location).Size;
                    OppGoallerId = OppID;
                }
            }
            if (MinDist > 0.5)
                return null;
            return OppGoallerId;
        }
        Position2D oppGollerPosition = GameParameters.OppGoalCenter;
        private int counter;
        private OppGoallerState CalculateGoallerState(GameStrategyEngine engine, WorldModel Model)
        {

            Random r = new Random();
            int v = r.Next((int)OppGoallerState.Left, (int)OppGoallerState.Right);
            if (Model.Opponents.Count == 0)
                return OppGoallerState.Left;
            int? oppid = OppGoallerId(engine, Model);

            if (oppid.HasValue)
                oppGollerPosition = Model.Opponents[oppid.Value].Location;
            if (oppGollerPosition.Y > GameParameters.OppGoalCenter.Y)
                return OppGoallerState.Left;
            else if (oppGollerPosition.Y < GameParameters.OppGoalCenter.Y)
                return OppGoallerState.Right;
            else
                return (OppGoallerState)v;
        }
        enum OppGoallerState
        {
            Left,
            Right,
        }
    }
}
