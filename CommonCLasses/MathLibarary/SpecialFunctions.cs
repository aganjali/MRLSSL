using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    /// <summary>
    /// Here is the place of sum useful function
    /// </summary>
    public class SpecialFunctions
    {
        /// <summary>
        /// ??
        /// </summary>
        /// <param name="x"></param>
        /// <param name="startX"></param>
        /// <param name="valueAtStartX"></param>
        /// <param name="endX"></param>
        /// <param name="valueAtEndX"></param>
        /// <returns></returns>
        public static double GetLimitedLinear(double x, double startX, double valueAtStartX, double endX, double valueAtEndX)
        {
            if (x <= startX)
                return valueAtStartX;
            if (x >= endX)
                return valueAtEndX;
            return (x - startX) * (valueAtEndX - valueAtStartX) / (endX - startX) + valueAtStartX;
        }
    }
}
