using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole;
using MRL.SSL.Planning.GamePlanner.Types;
namespace MRL.SSL.AIConsole.Plays
{
    public class OurIndirectFreeKick : PlayBase
    {
        StrategyBase selectedStrategy;
        int minDefenceAttendance = 0;
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            if ((engine.EngineID == 0 && (Status == GameStatus.IndirectFreeKick_OurTeam || Status == GameStatus.DirectFreeKick_OurTeam))
                || (engine.EngineID == 1 && (Status == GameStatus.IndirectFreeKick_Opponent || Status == GameStatus.DirectFreeKick_Opponent)))
            {
                if (selectedStrategy == null)
                {
                    return false;
                }
                else
                {
                    return selectedStrategy.IsFeasiblel(engine, Model, ref Status);
                }
            }
            else
            {
                return false;
            }

        }

        Dictionary<int, RoleBase> DefencePrevRoles;

        public OurIndirectFreeKick()
        {
            //init();
        }
        int minDefenceToAssign = 0;
        public void init(GameStrategyEngine engine, WorldModel Model)
        {
            strategyManager.StrategyList = engine.ImplementedStrategies.Clone();

            selectedStrategy = strategyManager.SelectStrategy(engine, Model, Model.OurRobots.Count - minDefenceAttendance, minDefenceAttendance, ref minDefenceToAssign, StrategyManager.StrategyPolicy.HighestWithRandom);
            if (selectedStrategy != null)
                selectedStrategy.ResetState();
        }
        WorldModel lastModel;
        List<int> visionProblemIds;
        bool first;

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            List<int> ids = new List<int>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            Model.markingStatesToBall = engine.GameInfo.OurTeam.MarkingStatesToBall.ToDictionary(k => k.Key, v => (int)v.Value);
            Model.markingStatesToTarget = engine.GameInfo.OurTeam.MarkingStatesToTarget.ToDictionary(k => k.Key, v => (int)v.Value);
            if (first)
            {
                lastModel = new WorldModel(Model);
                first = false;
            }
            WorldModel lastModel2 = new WorldModel(Model);
            lastModel2.SetMarkingStates(Model);
            visionProblemIds = new List<int>();
            foreach (var item in lastModel.OurRobots)
            {
                if (!lastModel2.OurRobots.ContainsKey(item.Key))
                {
                    visionProblemIds.Add(item.Key);
                    lastModel2.OurRobots[item.Key] = item.Value;
                    lastModel2.markingStatesToBall[item.Key] = (int)MarkingType.Blocked;//lastModel.markingStatesToBall[item.Key];
                    lastModel2.markingStatesToTarget[item.Key] = (int)MarkingType.Blocked;//lastModel.markingStatesToTarget[item.Key];
                }
            }

            int strategyAttendance = 0;
            if (selectedStrategy != null)
            {
                strategyAttendance = selectedStrategy.AttendanceSize;
            }
            DefenceAssigner(engine, lastModel2, strategyAttendance, minDefenceToAssign, out ids, ref Functions, out DefencePrevRoles);
            //ids = new List<int>();
            Dictionary<int, SingleObjectState> attendanceInStrategy = new Dictionary<int, SingleObjectState>();
            foreach (var item in ids.ToList())
            {
                attendanceInStrategy.Add(item, lastModel2.OurRobots[item]);
            }

            Dictionary<int, CommonDelegate> StrategyFunctions = new Dictionary<int, CommonDelegate>();

            CurrentlyAssignedRoles = selectedStrategy.Run(engine, lastModel2, attendanceInStrategy, out StrategyFunctions);


            foreach (var item in DefencePrevRoles)
                if (!CurrentlyAssignedRoles.ContainsKey(item.Key))
                    CurrentlyAssignedRoles.Add(item.Key, DefencePrevRoles[item.Key]);

            foreach (var item in StrategyFunctions)
                Functions.Add(item.Key, item.Value);

            foreach (var item in visionProblemIds)
            {
                if (CurrentlyAssignedRoles.ContainsKey(item))
                    CurrentlyAssignedRoles.Remove(item);
                if (Functions.ContainsKey(item))
                    Functions.Remove(item);
            }

            lastModel = lastModel2;

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;

            return PreviouslyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return null;
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            first = true;
            lastModel = null;
            visionProblemIds = new List<int>();
            FreekickDefence.BallIsMovedStrategy = false;

            FreekickDefence.RestartActiveFlags();
            init(engine, Model);
            PreviouslyAssignedRoles.Clear();
        }
    }
}
