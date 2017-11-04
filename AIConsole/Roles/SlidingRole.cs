using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Roles
{
    class SlidingRole : RoleBase
    {
        int id = 7;
        Position2D target = new Position2D();
        Line rightpositionLine = new Line();
        Line leftpositionLine = new Line();
        Position2D rightPosition = new Position2D();
        Position2D leftPosition = new Position2D();
        Position2D finalTarget = new Position2D();
        bool clockWise = true;
        double angle = 0;
        bool gotoTarget = false;
        bool gotoFinalTarget = false;
        bool gotoKick = false;
        int counter = 0;
        Position2D nextTarget = new Position2D();
        Circle oppCircle = null;
        bool meet = false;
        List<int> opponent = new List<int>();

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public void Run(WorldModel Model)
        {


            GetSkill<SlidingSkill>().sliding(Model,0.5);
            //if (meet)
            //{
            //    if (CurrentState == (int)State.back)
            //    {
            //        Planner.Add(id, Model.BallState.Location.Extend(0.3, 0), (GameParameters.OppGoalCenter - Model.BallState.Location).AngleInDegrees, PathType.Safe, false, true, true, true, true);
            //    }
            //    else if (CurrentState == (int)State.side)
            //    {
            //        Planner.Add(id, nextTarget, (GameParameters.OppGoalCenter - Model.BallState.Location).AngleInDegrees, PathType.Safe, false, true, true, true, true);
            //    }
            //    else if (CurrentState == (int)State.gotoKick)
            //    {
            //        Planner.AddKick(id, kickPowerType.Speed, 120, false, true);
            //    }
            //}
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            //Position2D ballPos = Model.BallState.Location;
            //Line ballgoalLine = new Line(Model.BallState.Location, GameParameters.OppGoalCenter);
            //Line leftTargetGoalLine = new Line(GameParameters.OppGoalCenter, Model.BallState.Location.Extend(0, 1));
            //Line rightTargetGoalLine = new Line(GameParameters.OppGoalCenter, Model.BallState.Location.Extend(0, -1));
            //Line backTargetLine = new Line(Model.OurRobots[id].Location, new Position2D(0.3, 0));
            //Position2D robotPos = Model.OurRobots[id].Location;

            //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[id].Location) < 0.13)
            //{
            //    Planner.Add(id, Model.OurRobots[id].Location + (Model.BallState.Location - Model.OurRobots[id].Location).GetNormalizeToCopy(0.001), (GameParameters.OppGoalCenter - Model.BallState.Location).AngleInDegrees, PathType.Safe, false, true, true, true, true);
            //    counter++;
            //}
            //if (counter > 60)
            //{
            //    opponent = NearOpponent(Model);
            //    if (opponent.Count != 0)
            //    {
            //        foreach (var item in opponent)
            //        {
            //            oppCircle = new Circle(Model.Opponents[item].Location, RobotParameters.OpponentParams.Diameter / 2 + 0.09);
            //            DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, RobotParameters.OpponentParams.Diameter / 2 + 0.07, new Pen(Color.Orange, 0.01f)), "dfghj");
            //            if (oppCircle.Intersect(ballgoalLine).Count != 0)
            //            {
            //                meet = true;
            //            }
            //            else
            //            {
            //                meet = false;
            //            }

            //            if (meet)
            //            {
            //                CurrentState = (int)State.back;
            //            }
            //            else
            //            {
            //                CurrentState = (int)State.gotoKick;
            //            }
            //            if (CurrentState == (int)State.back)
            //            {
            //                if (Model.OurRobots[id].Location.DistanceFrom(Model.BallState.Location.Extend(0.3, 0)) < 0.03)
            //                {
            //                    CurrentState = (int)State.side;
            //                }
            //            }
            //            if (CurrentState == (int)State.side)
            //            {
            //                if (oppCircle.Intersect(leftTargetGoalLine).Count == 0)
            //                {
            //                    nextTarget = Model.BallState.Location.Extend(0, 1);
            //                    gotoTarget = true;
            //                }
            //                else if (oppCircle.Intersect(rightTargetGoalLine).Count == 0)
            //                {
            //                    gotoTarget = true;
            //                    nextTarget = Model.BallState.Location.Extend(0, -1);
            //                }
            //            }
            //            else
            //            {
            //                CurrentState = (int)State.back;
            //            }

            //            if (gotoTarget)
            //            {
            //                Planner.ChangeDefaulteParams(id, false);
            //                Planner.SetParameter(id, 15, 15);
            //                DrawingObjects.AddObject(new StringDraw("target:" + nextTarget.Size, new Position2D(1.5, 1.5)), "ujhnb");
            //                Planner.Add(id, nextTarget, (GameParameters.OppGoalCenter - Model.BallState.Location).AngleInDegrees, PathType.Safe, false, true, true, true, true);
            //                DrawingObjects.AddObject(nextTarget, "rfghjm");
            //                gotoKick = true;
            //            }

            //        }
            //    }
            //}

        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
        public void Reset()
        {
            GetSkill<SlidingSkill>().Reset();
        }
        public List<int> NearOpponent(WorldModel Model)
        {
            oppCircle = null;
            List<int> nearopponentList = new List<int>();
            foreach (var item in Model.Opponents.Keys)
            {
                if (Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) < 0.5)
                {
                    nearopponentList.Add(item);
                }
            }
            return nearopponentList;
        }
    }
    public enum State
    {
        back,
        side,
        gotoKick
    }
}
