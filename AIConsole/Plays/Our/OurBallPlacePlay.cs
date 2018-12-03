using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using System.Drawing;

namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OurBallPlacePlay : PlayBase
    {

        int? placerID = null;
        int? catcherID = null;
        bool firstFlag = true;
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
                firstFlag = false;
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, placerID, typeof(BallPlacerRole)))
                Functions[placerID.Value] = (eng, wmd) => GetRole<BallPlacerRole>(placerID.Value).Perform(eng, wmd, placerID.Value);
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, catcherID, typeof(BallPalcementCatcher)))
                Functions[ catcherID.Value] = (eng, wmd) => GetRole<BallPalcementCatcher>(catcherID.Value).Perform(engine,Model,catcherID.Value);
            foreach (var item in Model.OurRobots.Keys)
            {
                if (!(placerID.HasValue && item == placerID.Value ) && !(catcherID.HasValue && item == catcherID.Value ))
                {
                    int index = item; // Warning: Very Important to use "item" like here
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, index, typeof(HaltRole)))
                        Functions[index] = (eng, wmd) => GetRole<HaltRole>(index).Halt(Model, index);
                }
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
        }
    }
}
