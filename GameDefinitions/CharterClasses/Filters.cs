//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace MRL.SSL.GameDefinitions
//{
//    public class MaxSizeFilter : IFilter<ChartDataInfo>
//    {
//        TimeSpan length = TimeSpan.FromSeconds(10);
//        public IList<ChartDataInfo> Filter(IList<ChartDataInfo> c)
//        {
//            if (c.Count == 0)
//                return new List<ChartDataInfo>();

//            DateTime end = c[c.Count - 1].Time;

//            int startIndex = 0;
//            for (int i = 0; i < c.Count; i++)
//            {
//                if (end - c[i].Time <= length)
//                {
//                    startIndex = i;
//                    break;
//                }
//            }

//            List<ChartDataInfo> res = new List<ChartDataInfo>(c.Count - startIndex);
//            for (int i = startIndex; i < c.Count; i++)
//            {
//                res.Add(c[i]);
//            }
//            return res;
//        }

//    }

//    public class FilterChain : IFilter<ChartDataInfo>
//    {
//        private readonly IFilter<ChartDataInfo>[] filters;
//        public FilterChain(params IFilter<ChartDataInfo>[] filters)
//        {
//            this.filters = filters;
//        }

//        #region IFilter<PerformanceInfo> Members

//        public IList<ChartDataInfo> Filter(IList<ChartDataInfo> c)
//        {
//            foreach (var filter in filters)
//            {
//                c = filter.Filter(c);
//            }
//            return c;
//        }

//        #endregion
//    }

//    public class AverageFilter : IFilter<ChartDataInfo>
//    {
//        private int number = 2;
//        public int Number
//        {
//            get { return number; }
//            set { number = value; }
//        }

//        public IList<ChartDataInfo> Filter(IList<ChartDataInfo> c)
//        {
//            int num = number - 1;
//            if (c.Count - num <= 0)
//                return c;

//            List<ChartDataInfo> res = new List<ChartDataInfo>(c.Count - num);
//            for (int i = 0; i < c.Count - num; i++)
//            {
//                double doubleSum = 0;
//                long ticksSum = 0;
//                for (int j = i; j < i + number; j++)
//                {
//                    doubleSum += c[j].Value;
//                    ticksSum += c[j].Time.Ticks / number;
//                }
//                doubleSum /= number;
//                res.Add(new ChartDataInfo { Time = new DateTime(ticksSum), Value = doubleSum });
//            }
//            return res;
//        }
//    }
//}
