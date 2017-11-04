using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Represent a command that will send to robot 
    /// </summary>
    public class SingleRobotCommand
    {
        /// <summary>
        /// Speed of robot in each Direction
        /// </summary>
        public Vector2D Speed;
        public double Kick, ChipKick;
        public double DriblerSpeed, AngularSpeed;
        public bool KickOnBalllContact, ReleaseWheels;
    }
}
