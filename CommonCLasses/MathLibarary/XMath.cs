using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    public class XMath
    {
        /// <summary>
        /// get the maximum
        /// </summary>
        /// <param name="values">a array of obj</param>
        /// <returns>Maximum</returns>
        public static double Max(params double[] values)
        {
            double val = double.MinValue;
            foreach (double v in values)
                if (v > val)
                    val = v;
            return val;
        }
        /// <summary>
        /// get the minimum
        /// </summary>
        /// <param name="values">a array of obj</param>
        /// <returns>Minimum</returns>
        public static double Min(params double[] values)
        {
            double val = double.MaxValue;
            foreach (double v in values)
                if (v < val)
                    val = v;
            return val;
        }

    }
}
