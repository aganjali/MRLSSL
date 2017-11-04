using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.IO;

namespace MRL.SSL.AIConsole.Plays
{
    class RotateTunePlay : PlayBase
    {

        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {

            //return false;
            if (engine.Status == GameDefinitions.GameStatus.ComeHere && RotateParameters.TuneFlag)
            {
                return true;
            }

            return false;

        }
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();


            if (Model.BallState.Speed.Size > .4)
            {
                Planner.Add(RotateParameters.RoboID, new Position2D(), Model.OurRobots[RotateParameters.RoboID].Angle.Value);
            }
            else
                Planner.AddRotate(Model, RotateParameters.RoboID, GameParameters.OppGoalCenter, RotateParameters.angle, kickPowerType.Power, 150, false, 60, false);
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {

        }

        public static bool speed = false;
    }
}
