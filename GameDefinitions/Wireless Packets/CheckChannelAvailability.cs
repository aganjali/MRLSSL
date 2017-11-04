using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class CheckChannelAvailability : WirelessPacket
    {
        private bool isAvailable;
        public bool IsAvailable
        {
            get { return isAvailable; }
            set { isAvailable = value; }
        }

        private byte packetKind = 2;

        private byte _RobotID;
        public byte RobotID
        {
            get { return _RobotID; }
            set { _RobotID = value; }
        }

        public override byte[] Serialize()
        {
            SendBytes = new byte[64];
            if (RobotID < 8)
                SendBytes[0] = (byte)(SendBytes[0] | (int)Math.Pow(2, RobotID));
            else
                SendBytes[1] = (byte)(SendBytes[1] | (int)Math.Pow(2, RobotID));
            SendBytes[1] = (byte)(SendBytes[1] | (1 << 4));

            SendBytes[2] = packetKind;
            SendBytes[3] = RobotID;
            
            


            for (int i = 4; i < 32; i++)
                SendBytes[i] = 255;

            if (RobotID < 8)
                SendBytes[32] = (byte)(SendBytes[32] | (int)Math.Pow(2, RobotID));
            else
                SendBytes[33] = (byte)(SendBytes[33] | (int)Math.Pow(2, RobotID));
            SendBytes[33] = (byte)(SendBytes[33] | (2 << 4));

            SendBytes[34] = packetKind;
            SendBytes[35] = RobotID;
            
            for (int i = 36; i < 64; i++)
                SendBytes[i] = 255;

            return SendBytes;
        }

        public override WirelessPacket Deserialize()
        {
            CheckChannelAvailability CCAV = new CheckChannelAvailability();
            if (ReciveBytes[2] != packetKind || ReciveBytes[3] != RobotID || ReciveBytes[4] != 255)
            {
                CCAV.IsAvailable = false;
                return CCAV;
            }

            
            CCAV.IsAvailable = true;
            return CCAV;
        }
    }
}
