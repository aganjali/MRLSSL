using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public abstract class WirelessPacket
    {
        public abstract byte[] Serialize();
        public abstract WirelessPacket Deserialize();
        public byte[] SendBytes;
        public byte[] ReciveBytes;
    }
}
