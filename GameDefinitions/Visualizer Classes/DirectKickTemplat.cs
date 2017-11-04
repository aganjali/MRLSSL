using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class DirectKickTemplat
    {
        public int RobotID { get; set; }
        public int Power { get; set; }
        public double Speed { get; set; }
        public string Key
        {
            get
            {
                return RobotID.ToString() + "@" + Power.ToString();
            }
        }
        public DirectKickTemplat(int power, double speed,int robotID)
        {
            RobotID = robotID;
            Power = power;
            Speed = speed;
        }
        public DirectKickTemplat()
        {
        }
    }
}
