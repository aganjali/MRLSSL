using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.Planning.GPUDirect
{
    public class GPUParams
    {
        public static int maxRobotCount = 6;
        public static float maxRobotAccel = 5.5f;//4;
        public static float maxRobotSpeed = 4;//3.5f;
        public static float ballDecel = 0.4f;
        public static int maxPathCount = 100;
        public static int maxRRTCount = 2 * maxRobotCount;
    }
}
