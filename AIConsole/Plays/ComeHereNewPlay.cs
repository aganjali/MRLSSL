using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Plays
{
    class ComeHereNewPlay:PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {

            return engine.Status == GameDefinitions.GameStatus.ComeHere && !RotateParameters.TuneFlag;/* || Model.Status == GameDefinitions.GameStatus.TestDefend || Model.Status == GameStatus.TestOffend;*/
            return false;
        }

        bool first = false;
        bool second = false;
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            var list = Model.OurRobots.Keys.ToList();
            list.Sort();
            Dictionary<int, SingleObjectState> ours = new Dictionary<int, SingleObjectState>();
            foreach (var item in list)
            {
                ours.Add(item, Model.OurRobots[item]);
            }

            if (!TuneVariables.Default.Position2Ds.ContainsKey("ComeHereArea"))
                TuneVariables.Default.Add("ComeHereArea", new Position2D());
            Position2D center = TuneVariables.Default.GetValue<Position2D>("ComeHereArea").Extend(.5, 0);
            double firstRadi = 0.5, secondRadi = 0.2;
            Dictionary<int, Position2D> firstPoses = new Dictionary<int, Position2D>();
            Dictionary<int, Position2D> secondPoses = new Dictionary<int, Position2D>();
            double robotCount = ours.Count;;
            int k = 0;
            foreach (var i in ours.Keys)
            {
                firstPoses.Add(i,center + Vector2D.FromAngleSize(k * (2 * Math.PI / robotCount), firstRadi));
                secondPoses.Add(i,center + Vector2D.FromAngleSize(k * (2 * Math.PI / robotCount), secondRadi));
                k++;
            }
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            
            if (!first)
            {
                for(int j = 0;j < robotCount;j++)
                {
                    int item = list[j];
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, item, typeof(GotoPointRole)))
                        Functions[item] = (eng, wmd) => GetRole<GotoPointRole>(item).GotoPoint(wmd, item, firstPoses[item], (center - firstPoses[item]).AngleInDegrees, true, true);
                }
            }

            if (!ours.Any(p => p.Value.Location.DistanceFrom(firstPoses[p.Key]) > 0.01) || first)
            {
                first = true;
                for (int j = 0; j < robotCount; j++)
                {
                    int item = list[j];
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, item, typeof(GotoPointRole)))
                        Functions[item] = (eng, wmd) => GetRole<GotoPointRole>(item).GotoPoint(wmd, item, secondPoses[item], (center - secondPoses[item]).AngleInDegrees, true, true);
                }
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }


        int? getID(Dictionary<int, RoleBase> current, Type roletype)
        {
            if (current.Any(a => a.Value.GetType() == roletype))
                return current.Single(a => a.Value.GetType() == roletype).Key;
            return null;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            throw new NotImplementedException();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model,GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
            first = false;
        }
    }
}
