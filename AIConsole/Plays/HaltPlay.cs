using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.AIConsole.Plays
{
    class HaltPlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            if (Model.Status == GameDefinitions.GameStatus.Halt)
                return true;
            return false;
        }
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {

            Vector2D pass = new Vector2D(2, 2 * Math.Sqrt(3));
            Vector2D shoot = new Vector2D(8, 0);
            double size = pass.InnerProduct(shoot) * Math.Cos(Vector2D.AngleBetweenInDegrees(pass, shoot));



            double DangerZonedist = new Position2D(2.67, -.87).DistanceFrom(new Position2D(2.69, -.91));
            double defenceLineDist = new Position2D(2.67, -.87).DistanceFrom(new Position2D(2.71, -1.03));
            RotateTunePlay.speed = false;
            ControlParameters.BallIsMoved = false;
            Dictionary<int, RoleBase> currently = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();

            foreach (var item in Model.OurRobots.Keys)
            {
                int index = item; // Warning: Very Important to use "item" like here
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, currently, index, typeof(HaltRole)))
                    Functions[index] = (eng, wmd) => GetRole<HaltRole>(index).Halt(Model, index);
            }

            PreviouslyAssignedRoles = currently;
            return PreviouslyAssignedRoles;
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            ControlParameters.BallIsMoved = false;
            FreekickDefence.BallIsMovedStrategy = false;

            FreekickDefence.RestartActiveFlags();
            PreviouslyAssignedRoles.Clear();
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            throw new NotImplementedException();
        }
    }
}
