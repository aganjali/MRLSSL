using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Parameters of robots
    /// </summary>
    public class RobotParameters
    {
        public float Height, Diameter, MaxAcceleration, MaxSpeed, MaxDeccelaration, MaxRotationalAcceleration, MaxRotationalDecceleration, MaxRotationalSpeed, FrontFlatLine;
        public static RobotParameters OurRobotParams, OpponentParams;
        /// <summary>
        /// Construct robot parameters 
        /// </summary>
        static RobotParameters()
        {
            OurRobotParams = new RobotParameters();
            OurRobotParams.Diameter = 0.18f;
            //OurRobotParams.Diameter = 0.25f;
            OurRobotParams.Height = 0.15f;
            OurRobotParams.MaxAcceleration = 6f;
            OurRobotParams.MaxDeccelaration = 12f;
            OurRobotParams.MaxRotationalAcceleration = 6f;
            OurRobotParams.MaxRotationalDecceleration = 10f;
            OurRobotParams.MaxRotationalSpeed = 1.44f;
            OurRobotParams.MaxSpeed = 2.5f;
            OurRobotParams.FrontFlatLine = 0.087f;
            OpponentParams = OurRobotParams.MemberwiseClone() as RobotParameters;
        }
    }
}
