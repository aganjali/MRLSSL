using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.GameDefinitions.General_Settings;
using System.Collections.Generic;

namespace MRL.SSL.AIConsole.Roles
{
    class newRotate
    {
        #region Parameter
        public double ClockWise = 1;
        int teta = 0;
        Position2D target = new Position2D();
        public double omegaCoef = 0;
        public double VyCoef = 0;
        #endregion

        //public SingleWirelessCommand Rotate(WorldModel Model, int RobotID, bool spin,bool isCorner,bool )
        //{
        //    ClockWise = 1;
        //    if (isCorner)
        //    {
        //        omegaCoef = 0.9;//TODO:set from visualizer
        //        VyCoef = 350;//TODO:set from visualizer
        //        teta = 30;
                         
        //    }
        //}
            
    }
}
