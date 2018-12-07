using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OurBallPlacePlay : PlayBase
    {

        int? placerID = null;
        int? catcherID = null;
        bool firstFlag = true;
        Position2D firstBallPos = new Position2D();
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            if (Status == GameDefinitions.GameStatus.BallPlace_OurTeam)
                return true;
            firstFlag = true;
            return false;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.04, new Pen(Color.Orange, 0.01f)), "ballcircle");
            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.5, new Pen(Color.GreenYellow, 0.01f), true, 0.1f, false), "ballcigfdsrcle");



            if (firstFlag)
            {
                placerID = null;
                catcherID = null;
                double min = double.MaxValue;
                foreach (var item in Model.OurRobots.Keys)
                {
                    if (!(Model.GoalieID.HasValue && item == Model.GoalieID.Value))
                    {
                        if (Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < min)
                        {
                            min = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                            placerID = item;
                        }
                    }
                }
                min = double.MaxValue;
                foreach (var item in Model.OurRobots.Keys)
                {
                    if (!(Model.GoalieID.HasValue && item == Model.GoalieID.Value) && !(placerID.HasValue && item == placerID.Value))
                    {
                        if (Model.OurRobots[item].Location.DistanceFrom(StaticVariables.ballPlacementPos) < min)
                        {
                            min = Model.OurRobots[item].Location.DistanceFrom(StaticVariables.ballPlacementPos);
                            catcherID = item;
                        }
                    }
                }
                firstBallPos = Model.BallState.Location;
                firstFlag = false;
            }

            if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.5 )//&& Model.BallState.Speed.Size > 0.2 )
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, placerID, typeof(BallPlacerRole)))
                    Functions[placerID.Value] = (eng, wmd) => GetRole<BallPlacerRole>(placerID.Value).Perform(eng, wmd, placerID.Value, catcherID.Value);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, catcherID, typeof(BallPalcementCatcher)))
                    Functions[catcherID.Value] = (eng, wmd) => GetRole<BallPalcementCatcher>(catcherID.Value).Perform(engine, Model, catcherID.Value); 
            }
            foreach (var item in Model.OurRobots.Keys)
            {
                var counter = 0;
                if ((!(placerID.HasValue && item == placerID.Value) && !(catcherID.HasValue && item == catcherID.Value)) || Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.5 && Model.BallState.Speed.Size < 0.4)
                {
                    counter++;
                    int index = item; // Warning: Very Important to use "item" like here
                    Vector2D vec = Vector2D.FromAngleSize((StaticVariables.ballPlacementPos - GameParameters.OppGoalCenter).AngleInRadians + counter * 4 * Math.PI / 180, 0.7);
                    Planner.Add(item, StaticVariables.ballPlacementPos + vec, (Model.BallState.Location - GameParameters.OppGoalCenter).AngleInDegrees);
                    //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, index, typeof(HaltRole)))
                    //    Functions[index] = (eng, wmd) => GetRole<HaltRole>(index).Halt(Model, index);
                }
            }
            if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.10 && Model.BallState.Speed.Size < 0.25)
            {
                if (placerID.HasValue)
                    GetRole<BallPlacerRole>(placerID.Value).Reset();
                if (catcherID.HasValue)
                    GetRole<BallPalcementCatcher>(catcherID.Value).Reset(); 
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(GameDefinitions.WorldModel Model, GameStrategyEngine engine)
        {
            if (placerID.HasValue)
                GetRole<BallPlacerRole>(placerID.Value).Reset();
            if (catcherID.HasValue)
                GetRole<BallPalcementCatcher>(placerID.Value).Reset();
            firstFlag = true;
        }
    }
}
