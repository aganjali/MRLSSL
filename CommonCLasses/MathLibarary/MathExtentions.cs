using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    public static class MathExtentions
    {
        public static double ToRadian(this double source)
        {
            return source * Math.PI / 180;
        }
        public static double ToDegree(this double source)
        {
            return source * 180 / Math.PI;
        }
    }
}
