using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class SetChannelOnRobot : WirelessPacket
    {
        private byte packetKind = 2;
        private byte packetBaudRate = 0;

        private byte _RobotID;
        public byte RobotID
        {
            get { return _RobotID; }
            set { _RobotID = value; }
        }

        private byte _ChannelNumSender;
        public byte ChannelNumSender
        {
            get { return _ChannelNumSender; }
            set { _ChannelNumSender = value; }
        }

        private byte _ChannelNumReciever;
        public byte ChannelNumReciever
        {
            get { return _ChannelNumReciever; }
            set { _ChannelNumReciever = value; }
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
            SendBytes[4] = packetBaudRate;
            SendBytes[5] = ChannelNumSender;
            SendBytes[6] = ChannelNumReciever;
            for (int i = 7; i < 32; i++)
                SendBytes[i] = 255;

            if (RobotID < 8)
                SendBytes[32] = (byte)(SendBytes[32] | (int)Math.Pow(2, RobotID));
            else
                SendBytes[33] = (byte)(SendBytes[33] | (int)Math.Pow(2, RobotID));
            SendBytes[33] = (byte)(SendBytes[33] | (2 << 4));
            SendBytes[34] = packetKind;
            SendBytes[35] = RobotID;
            SendBytes[36] = packetBaudRate;
            SendBytes[37] = ChannelNumSender;
            SendBytes[38] = ChannelNumReciever;
            for (int i = 39; i < 64; i++)
                SendBytes[i] = 255;
            return SendBytes;
        }
        public override WirelessPacket Deserialize()
        {
            return null;
        }
    }
}
