using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class SingleRotateCommand
    {
        public double _clockWise;
        public int _teta;
        public int framecount;
        public double _vy;
        public double _omega;

        public SingleRotateCommand()
        {
        }

        public SingleRotateCommand(double clockWise, int teta, int count, double vy, double omega)
        {
            _clockWise = clockWise;
            _teta = teta;
            framecount = count;
            _vy = vy;
            _omega = omega;
        }
    }
}
