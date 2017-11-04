using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Configuration for simulator
    /// </summary>
    public struct SimulatorInfo
    {
        public double OurRobotCount;
        public double OppRobotCount;

		public SimulatorInfo(double ourRobotCount, double oppRobotCount)
        {
            OurRobotCount = ourRobotCount;
            OppRobotCount = oppRobotCount;
        }

    }
}
