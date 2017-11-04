using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Engine
{
    public class NormalDefenceAssigner
    {
        public NormalDefenceAssigner()
        { }
        public Dictionary<RoleBase, DefenceInfo> Assign(GameStrategyEngine engine, WorldModel Model, out Dictionary<RoleBase, Position2D?> Positions, out Dictionary<RoleBase, double> Angles, List<DefenderCommand> DefenceCommands)
        {
            List<DefenceInfo> Infoes = new List<DefenceInfo>();
            DefenderCommand df1, df2, dfgoli;
            List<DefenderCommand> Commands = new List<DefenderCommand>();
            Dictionary<int, float> scores;
            if (engine.GameInfo.OppTeam.GoaliID.HasValue)
                scores = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
            else
                scores = engine.GameInfo.OppTeam.Scores;
            dfgoli = new DefenderCommand() { RoleType = typeof(GoalieNormalRole) };
            if (scores.Count == 0)
            {
                df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
            }
            else if (scores.Count == 1)
            {
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
            }
            else
            {
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderNormalRole2) };
            }
            Commands.Add(dfgoli);
            Commands.Add(df1);
            Commands.Add(df2);

            foreach (var item in DefenceCommands)
            {
                Commands.Add(new DefenderCommand() { RoleType = item.RoleType, MarkMaximumDist = item.MarkMaximumDist, OppID = item.OppID, RegionalDefendPoints = item.RegionalDefendPoints, RegionalDistFromDangerZone = item.RegionalDistFromDangerZone });
            }
            Infoes = FreekickDefence.Match(engine, Model, Commands , true);
            Positions = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.DefenderPosition);
            Angles = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.Teta);
            return Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v);
        }
        public Dictionary<RoleBase, DefenceInfo> Assign(GameStrategyEngine engine, WorldModel Model, out Dictionary<RoleBase, Position2D?> Positions, out Dictionary<RoleBase, double> Angles, bool AddMarkerForFirstAttaker, bool AddMarkerForSecondAttaker, bool AddMarkerForThirdAttaker, bool AddMarkerForBall)
        {
            
            List<DefenceInfo> Infoes = new List<DefenceInfo>();
            DefenderCommand df1, df2, dfgoli, dfm1 = null, dfm2 = null, dfm3 = null, dfm4 = null;
            List<DefenderCommand> Commands = new List<DefenderCommand>();
            Dictionary<int, float> scores;
            if (engine.GameInfo.OppTeam.GoaliID.HasValue)
                scores = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
            else
                scores = engine.GameInfo.OppTeam.Scores;
            dfgoli = new DefenderCommand() { RoleType = typeof(GoalieNormalRole) };
            if (scores.Count == 0)
            {
                df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            else if (scores.Count == 1)
            {
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            else if (scores.Count == 2)
            {
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3,};
                dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                dfm2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            else
            {
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = scores.ElementAt(2).Key, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            Commands.Add(dfgoli);
            Commands.Add(df1);
            Commands.Add(df2);

            if (dfm1 != null)
                Commands.Add(dfm1);
            if (dfm2 != null)
                Commands.Add(dfm2);
            if (dfm3 != null)
                Commands.Add(dfm3);
            if (dfm4 != null)
                Commands.Add(dfm4);
            Infoes = FreekickDefence.Match(engine, Model, Commands , true);

            Positions = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.DefenderPosition);
            Angles = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.Teta);
            return Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v);
        }
        public Dictionary<RoleBase, DefenceInfo> Assign(GameStrategyEngine engine, WorldModel Model, out Dictionary<RoleBase, Position2D?> Positions, out Dictionary<RoleBase, double> Angles, bool AddMarkerForFirstAttaker, bool AddMarkerForSecondAttaker, bool AddMarkerForThirdAttaker, bool AddMarkerForBall, double maxMarkD)
        {
            maxMarkD = Math.Max(1, maxMarkD);
            List<DefenceInfo> Infoes = new List<DefenceInfo>();
            DefenderCommand df1, df2, dfgoli, dfm1 = null, dfm2 = null, dfm3 = null, dfm4 = null;
            List<DefenderCommand> Commands = new List<DefenderCommand>();
            Dictionary<int, float> scores;
            if (engine.GameInfo.OppTeam.GoaliID.HasValue)
                scores = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
            else
                scores = engine.GameInfo.OppTeam.Scores;
            dfgoli = new DefenderCommand() { RoleType = typeof(GoalieCornerRole) };
            if (scores.Count == 0)
            {
                //df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole1) };
                //df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
                df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderCornerRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderCornerRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = maxMarkD };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = maxMarkD };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = maxMarkD };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = maxMarkD };
            }
            else if (scores.Count == 1)
            {
                //df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                //df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderCornerRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderCornerRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = maxMarkD };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = maxMarkD };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = maxMarkD };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = maxMarkD };
            }
            else if (scores.Count == 2)
            {
                //df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                //df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderNormalRole2) };
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderCornerRole1) };
                df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderCornerRole2) };
                
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = maxMarkD };
                if (AddMarkerForSecondAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = maxMarkD, };
                dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = maxMarkD };
                dfm2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = maxMarkD };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = maxMarkD };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = maxMarkD };
            }
            else
            {
                //df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderNormalRole1) };
                //df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderNormalRole2) };
                df1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderCornerRole1) };
                df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderCornerRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = maxMarkD };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = maxMarkD };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = scores.ElementAt(2).Key, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = maxMarkD };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = maxMarkD };
            }
            Commands.Add(dfgoli);
            Commands.Add(df1);
            Commands.Add(df2);

            if (dfm1 != null)
                Commands.Add(dfm1);
            if (dfm2 != null)
                Commands.Add(dfm2);
            if (dfm3 != null)
                Commands.Add(dfm3);
            if (dfm4 != null)
                Commands.Add(dfm4);
            Infoes = FreekickDefence.Match(engine, Model, Commands , true);


            Positions = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.DefenderPosition);
            Angles = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.Teta);
            return Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v);
        }
     
        public Dictionary<RoleBase, DefenceInfo> AssignTest(GameStrategyEngine engine, WorldModel Model, out Dictionary<RoleBase, Position2D?> Positions, out Dictionary<RoleBase, double> Angles, bool AddMarkerForFirstAttaker, bool AddMarkerForSecondAttaker, bool AddMarkerForThirdAttaker, bool AddMarkerForBall)
        {
            List<DefenceInfo> Infoes = new List<DefenceInfo>();
            DefenderCommand df1, df2, dfgoli, dfm1 = null, dfm2 = null, dfm3 = null, dfm4 = null;
            List<DefenderCommand> Commands = new List<DefenderCommand>();
            Dictionary<int, float> scores;
            if (engine.GameInfo.OppTeam.GoaliID.HasValue)
                scores = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
            else
                scores = engine.GameInfo.OppTeam.Scores;
            dfgoli = new DefenderCommand() { RoleType = typeof(GoalieNormalRole) };
            if (scores.Count == 0)
            {
                df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            else if (scores.Count == 1)
            {
                df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            else if (scores.Count == 2)
            {
                df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            else
            {
                df1 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderNormalRole1) };
                df2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderNormalRole2) };
                if (AddMarkerForFirstAttaker)
                    dfm1 = new DefenderCommand() { OppID = scores.First().Key, RoleType = typeof(DefenderMarkerNormalRole1), MarkMaximumDist = 3 };
                if (AddMarkerForSecondAttaker)
                    dfm2 = new DefenderCommand() { OppID = scores.ElementAt(1).Key, RoleType = typeof(DefenderMarkerNormalRole2), MarkMaximumDist = 3 };
                if (AddMarkerForThirdAttaker)
                    dfm3 = new DefenderCommand() { OppID = scores.ElementAt(2).Key, RoleType = typeof(DefenderMarkerNormalRole3), MarkMaximumDist = 3 };
                if (AddMarkerForBall)
                    dfm4 = new DefenderCommand() { OppID = null, RoleType = typeof(DefenderMarkerNormalRole4), MarkMaximumDist = 3 };
            }
            Commands.Add(dfgoli);
            Commands.Add(df1);
            Commands.Add(df2);

            if (dfm1 != null)
                Commands.Add(dfm1);
            if (dfm2 != null)
                Commands.Add(dfm2);
            if (dfm3 != null)
                Commands.Add(dfm3);
            if (dfm4 != null)
                Commands.Add(dfm4);
            Infoes = FreekickDefence.Match(engine, Model, Commands , true);

            Positions = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.DefenderPosition);
            Angles = Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v.Teta);
            return Infoes.ToDictionary(k => k.RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase, v => v);
        }
    }
}
