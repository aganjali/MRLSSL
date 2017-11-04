using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions.Wireless_Packets
{
    public class RobotStatus
    {
        int _sequenceNumber;

        public int SequenceNumber
        {
            get { return _sequenceNumber; }
            set { _sequenceNumber = value; }
        }

        int _robotID;
        public int RobotID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }

        int _batteryLife;
        public int BatteryLife
        {
            get { return _batteryLife; }
            set { _batteryLife = value; }
        }

        bool _sensore;
        public bool Sensore
        {
            get { return _sensore; }
            set { _sensore = value; }
        }
        
    }
}
