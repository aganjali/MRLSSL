using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MRL.SSL.GameDefinitions
{
    public enum debug_mode { mod_control = 3, mod_debug = 2, mod_normal = 1 };

    public class DebugMode : WirelessPacket
    {
        #region "Properties"
        private byte _rid;
        public byte RobotID
        {
            get { return _rid; }
            set { _rid = value; }
        }

        private debug_mode _mode;
        public debug_mode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        #endregion

        private byte Count = 0;

        public override byte[] Serialize()
        {
            SendBytes = new byte[32];

            float w = 0;
            Int16 sp = 0;
            PointF V = new PointF(0, 0);

            if (Count > 254)
                Count = 0;
            else
                Count++;

            if (Mode == debug_mode.mod_control)
                SendBytes[0] |= (byte)24; //Control
            else if (Mode == debug_mode.mod_debug)
                SendBytes[0] |= (byte)8;  //Debug
            else
                SendBytes[0] |= (byte)16; //Normal

            SendBytes[1] = Count;
            SendBytes[2] |= RobotID;
            SendBytes[2] |= (Math.Sign(V.X) == -1) ? (byte)8 : (byte)0;
            SendBytes[2] |= (Math.Sign(V.Y) == -1) ? (byte)16 : (byte)0;
            SendBytes[2] |= (Math.Sign(w) == -1) ? (byte)32 : (byte)0;
            SendBytes[2] |= (byte)64;

            V.X = (V.X < 0) ? -V.X : V.X;
            V.Y = (V.Y < 0) ? -V.Y : V.Y;
            w = (w < 0) ? -w : w;
            sp = (short)((sp < 0) ? -sp : sp);

            SendBytes[2] |= (byte)(sp << 7);
            SendBytes[3] |= (byte)((sp >> 1));
            SendBytes[3] |= (!false) ? (byte)0 : (byte)(1 << 2);
            SendBytes[3] |= (byte)(((int)V.X) << 3);
            SendBytes[3] |= (byte)(((int)V.Y) << 5);
            SendBytes[3] |= (byte)(((int)w >> 3) << 7);
            SendBytes[4] = (byte)(int)((V.X - (int)V.X) * 256);
            SendBytes[5] = (byte)(int)((V.Y - (int)V.Y) * 256);

            SendBytes[6] = (byte)((int)((w - (int)w) * 32) | ((int)w) << 5);

            SendBytes[7] = 0;
            return SendBytes;
        }

        public override WirelessPacket Deserialize()
        {
            return null;
        }
    }
}
