using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Engine
{
    public class StrategyManager
    {
        public List<StrategyBase> StrategyList = new List<StrategyBase>();
        public void Add(StrategyBase strategy)
        {
            //if (strategy.GetType() == 
            {
                //StrategyBase s = (strategy.GetConstructor(new Type[] { }).Invoke(new object[] { }) as StrategyBase);
                if (!StrategyList.Any(p => p.StrategyName == strategy.StrategyName))
                    StrategyList.Add(strategy);
                else
                {

                    for (int index = 0; index < StrategyList.Count; index ++ )
                    {
                        if (StrategyList[index].StrategyName == strategy.StrategyName)
                        {
                            StrategyList[index] = strategy;
                        }
                        
                    }
                }
            }
        }
        Random rnd = new Random();
        Random rndProb = new Random();
        public double Probability = 0.7;
        public StrategyBase SelectStrategy(GameStrategyEngine engine, WorldModel Model, int AttendanceSize, int DefenceAttendance, ref int minDefenceAttendance, StrategyPolicy policy)
        {
            Position2D ball =  Model.BallState.Location;
            StrategyBase ret = null;
            List<StrategyBase> strategyListTemp = new List<StrategyBase>();

            strategyListTemp = StrategyList.Where(c => 
                c.Enable 
                && (c.AttendanceSize <= AttendanceSize || (c.AttendanceSize + DefenceAttendance >= StaticVariables.MaxRobotCounts 
                && ((c.AttendanceSize >= StaticVariables.MaxRobotCounts 
                && ((Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value) && c.AttendanceSize <= Model.OurRobots.Count) || ((!Model.GoalieID.HasValue || !Model.OurRobots.ContainsKey(Model.GoalieID.Value)) && c.AttendanceSize <= Model.OurRobots.Count + 1))) ||c.AttendanceSize < StaticVariables.MaxRobotCounts))) 
                && c.zone != null && c.zone.Any(p => p.X > ball.X && p.Y < ball.X) 
                && c.status != null && c.status.Any(g => g == engine.Status)
                && ((Math.Abs(ball.Y) > 2 && !c.UseOnlyInMiddle) || (c.UseInMiddle && Math.Abs(ball.Y) <= 2))
                ).ToList();


            if (strategyListTemp.Count == 0)
                return null; 
            if (policy == StrategyPolicy.Highest)
            {
                strategyListTemp.Sort(delegate(StrategyBase p1, StrategyBase p2) { return p1.Score.CompareTo(p2.Score); });
                ret = strategyListTemp.First();
            }
            else if (policy == StrategyPolicy.Random)
            {
                ret = strategyListTemp.ElementAt(rnd.Next(strategyListTemp.Count));
            }
            else
            {
                double probSum = strategyListTemp.Sum(p => p.Probability);
                if (probSum > 0)
                {
                    double tmpSum = 0;
                    for (int i = 0; i < strategyListTemp.Count; i++)
                    {
                        tmpSum += (strategyListTemp[i].Probability / probSum);
                        strategyListTemp[i].Probability = tmpSum;
                    }
                }
                double prob = rndProb.NextDouble();
                int j = -1;
                for (int i = 0; i < strategyListTemp.Count; i++)
                {
                    if (prob <= strategyListTemp[i].Probability)
                    {
                        j = i;
                        break;
                    }
                }
                ret = strategyListTemp.ElementAt(j);

            }
            minDefenceAttendance = (ret != null) ? Model.OurRobots.Count - ret.AttendanceSize : DefenceAttendance;
            minDefenceAttendance = Math.Max(minDefenceAttendance, 0);
            return ret;
        }

        public void Reward(StrategyBase strategy, double RewardValue)
        {
            foreach (var item in StrategyList)
            {
                if (item == strategy)
                {
                    item.Score += RewardValue;
                }
            }
        }

        public enum AttendanceCompare
        {
            Equal = 0,
            Greater = 1,
            Lower = 2
        }
        public enum StrategyPolicy
        {
            Random = 0,
            Highest = 1,
            HighestWithRandom = 2
        }
    }
}
