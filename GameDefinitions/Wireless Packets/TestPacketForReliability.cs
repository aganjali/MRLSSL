using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class TestPacketForReliability : WirelessPacket
    {
        private bool _PacketRecievedCorrectly;
        public bool PacketRecievedCorrectly
        {
            get { return _PacketRecievedCorrectly; }
            set { _PacketRecievedCorrectly = value; }
        }
        private short _PacketNumber;
        public short PacketNumber
        {
            get { return _PacketNumber; }
            set { _PacketNumber = value; }
        }
        private byte _RobotID;
        private byte packetKind = 2;
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
            SendBytes[4] = 238;
            SendBytes[5] = (byte)PacketNumber;


            for (int i = 6; i < 32; i++)
                SendBytes[i] = 255;

            if (RobotID < 8)
                SendBytes[32] = (byte)(SendBytes[32] | (int)Math.Pow(2, RobotID));
            else
                SendBytes[33] = (byte)(SendBytes[33] | (int)Math.Pow(2, RobotID));
            SendBytes[33] = (byte)(SendBytes[33] | (2 << 4));

            SendBytes[34] = packetKind;
            SendBytes[35] = RobotID;
            SendBytes[36] = 238;
            SendBytes[37] = (byte)PacketNumber;

            for (int i = 38; i < 64; i++)
                SendBytes[i] = 255;

            return SendBytes;
        }

        public override WirelessPacket Deserialize()
        {
            TestPacketForReliability TPR = new TestPacketForReliability();
            TPR.PacketRecievedCorrectly = false;

            if (ReciveBytes[2] == packetKind)
            {
                if (ReciveBytes[3] == RobotID)
                {
                    if (ReciveBytes[4] == 238)
                    {
                        if (ReciveBytes[5] == (byte)PacketNumber)
                        {
                            TPR.PacketRecievedCorrectly = true;
                            return TPR;
                        }
                        else
                        {
                            TPR.PacketRecievedCorrectly = false;
                            return TPR;
                        }
                    }
                }
            }

            return TPR;
        }
    }
}
