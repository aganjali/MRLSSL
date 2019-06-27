using MRL.SSL.CommonClasses.MathLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class GameEvents
    {
        public int BlueScore { get; set; }
        public int YellowScore { get; set; }
        public int TimeOfstage { get; set; }
        public int YellowGoalie { get; set; }
        public int BlueGoalie { get; set; }
        public Position2D? BallPlacementPosition { get; set; }

        public GameEvents()
        {
        }

    }
}
