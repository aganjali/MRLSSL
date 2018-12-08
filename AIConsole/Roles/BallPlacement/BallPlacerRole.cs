using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
namespace MRL.SSL.AIConsole.Roles
{
    class BallPlacerRole : RoleBase
    {
        int counter = 0;
        PlacementModes currentMode = PlacementModes.Pass;
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }
        int? catcherID = null;
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (currentMode == PlacementModes.OneRobot)
            {
                if (CurrentState == (int)state.GoBehind)
                {
                    if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.1)
                        CurrentState = (int)state.GoPlace;
                }
                else if (CurrentState == (int)state.GoPlace)
                {
                    if (Model.OurRobots[RobotID].Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1)
                        CurrentState = (int)state.Place;
                }
            }
            else if (currentMode == PlacementModes.Pass)
            {
                if (CurrentState == (int)state.GoBehind)
                {

                    if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.21 && catcherID.HasValue && Model.OurRobots[catcherID.Value].Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.10)
                        CurrentState = (int)state.Pass;
                }
                if (Model.BallState.Speed.Size > 0.6)
                {
                    CurrentState = (int)state.Halt;
                }
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, int catcherId, PlacementModes mode)
        {
            currentMode = mode;
            catcherID = catcherId;
            if (currentMode == PlacementModes.OneRobot)
            {
                if (CurrentState == (int)state.GoBehind)
                {
                    Planner.ChangeDefaulteParams(RobotID, false);
                    Planner.SetParameter(RobotID, 1, 1);
                    GetSkill<GetBallSkill>().PerformForStrategy(engine, Model, RobotID, StaticVariables.ballPlacementPos);
                    Planner.AddKick(RobotID, true);
                }
                else if (CurrentState == (int)state.GoPlace)
                {
                    Planner.ChangeDefaulteParams(RobotID, false);
                    Planner.SetParameter(RobotID,1, 0.1);
                    Planner.Add(RobotID, StaticVariables.ballPlacementPos, (StaticVariables.ballPlacementPos - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, true, false, false);
                    Planner.AddKick(RobotID, true);
                }
                else
                {
                    Planner.Add(RobotID, Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Angle.Value, PathType.UnSafe, false, false, false, false);
                    if (counter++ > 30)
                        Planner.AddKick(RobotID, false);
                    else
                        Planner.AddKick(RobotID, true);
                }
            }
            else if (currentMode == PlacementModes.Pass)
            {
                if (CurrentState == (int)state.GoBehind)
                {
                    GetSkill<GetBallSkill>().PerformForStrategy(engine, Model, RobotID, StaticVariables.ballPlacementPos, false, 0.2);
                    Planner.AddKick(RobotID, true);
                }
                else if (CurrentState == (int)state.Pass)
                {
                    //Planner.AddRotate(Model,RobotID,StaticVariables.ballPlacementPos,0,kickPowerType.Speed,4,false);

                    var speed = Math.Min(Math.Max(0.9, 0.5 * Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos)), 5);
                    GetSkill<GetBallSkill>().PerformStatic(engine, Model, RobotID, StaticVariables.ballPlacementPos);
                    Planner.AddKick(RobotID, kickPowerType.Speed, false, speed);
                    double dist, boarder;
                    if (GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.20, out dist, out boarder))
                    {
                        Planner.AddRotate(Model, RobotID, StaticVariables.ballPlacementPos,0,kickPowerType.Speed,speed,false);
                    }
                }
                else if (CurrentState == (int)state.Halt)
                {

                    Vector2D vec = Vector2D.FromAngleSize((StaticVariables.ballPlacementPos - GameParameters.OppGoalCenter).AngleInRadians + 2 * 3 * Math.PI / 180, 0.7);
                    Planner.Add(RobotID, Model.OurRobots[RobotID].Location, (Model.BallState.Location - GameParameters.OppGoalCenter).AngleInDegrees);
                }

            }
        }

        public void Reset()
        {
            CurrentState = 0;
            counter = 0;
        }

        enum state
        {
            GoBehind,
            GoPlace,
            Place,
            Pass,
            Halt
        }
        public enum PlacementModes
        {
            OneRobot,
            Pass
        }
    }
}
