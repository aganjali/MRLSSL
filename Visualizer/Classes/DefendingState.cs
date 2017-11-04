using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.Visualizer.Classes
{
    public class DefendingState
    {
        public double alfa { get; set; }
        public double d { get; set; }
        public double dG { get; set; }
        public double dD { get; set; }
        public double GD { get; set; }
        public Position2D Goali { get; set; }
        public Position2D Defender { get; set; }
        public Position2D Ball{ get; set; }
        
    }
}
