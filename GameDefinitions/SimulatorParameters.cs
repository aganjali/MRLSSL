using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.GameDefinitions
{
    public class SimulatorParameters
    {
        WorldModel _model;
        Dictionary<int, SingleWirelessCommand> _commands;

        public Dictionary<int, SingleWirelessCommand> Commands
        {
            get { return _commands; }
            set { _commands = value; }
        }

        public WorldModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

    }
}
