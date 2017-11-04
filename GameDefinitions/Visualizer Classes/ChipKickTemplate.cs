using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions.Visualizer_Classes
{
    public class ChipKickTemplate
    {
        public int RobotID { get; set; }
        public int Power { get; set; }
        public double Lenght { get; set; }
        public double SafeRadi { get; set; }
        public bool HasSpin { get; set; }
        public double Time { get; set; }
        public bool BackSensore { get; set; }
    }
}
