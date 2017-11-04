using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class SetChannelOnAI : WirelessPacket
    {
        private byte packetKind = 2;
        public byte packetBaudRate = 0;
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

            SendBytes[0] = 255;
            SendBytes[1] = 255;
            SendBytes[2] = packetBaudRate;
            SendBytes[3] = ChannelNumSender;
            SendBytes[4] = ChannelNumReciever;
            for (int i = 5; i < 32; i++)
            {
                SendBytes[i] = 255;
            }

            SendBytes[32] = 255;
            SendBytes[33] = 255;
            SendBytes[34] = packetBaudRate;
            SendBytes[35] = ChannelNumSender;
            SendBytes[36] = ChannelNumReciever;
            for (int i = 37; i < 64; i++)
            {
                SendBytes[i] = 255;
            }

            return SendBytes;
        }

        public override WirelessPacket Deserialize()
        {
            return null;
        }
    }
}
