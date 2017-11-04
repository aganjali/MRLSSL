using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.Planning.GamePlanner
{
    public partial class BallState
    {
        public const int threadsPerBlock = 256;
        public const int AccelSteps = 31;
        public const int maxRobotCount = 6;
        public const int maxLengh = 700;
        public const int maxPoints = maxLengh / Resoloution;
        public const int maxLines = 4;
        public const int Resoloution = 10;
    }
}
