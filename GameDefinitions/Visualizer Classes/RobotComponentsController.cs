using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public static class RobotComponentsController
    {
        public static Dictionary<int, SingleWirelessCommand> PreviousCommands { get; set; }
        public static Dictionary<int, SingleWirelessCommand> RobotCommands { get; set; }
        public static Position2D Target = new Position2D();
        public static double Angle = 0;
        public static int? SelectedID = null;
    }
}
