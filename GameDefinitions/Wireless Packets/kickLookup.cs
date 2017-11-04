using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class kickLookup : WirelessPacket
    {
        private byte PacketKind = 3;
        private byte _robotID;
        public byte RobotID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }
        private byte _power;
        public byte Power
        {
            get { return _power; }
            set { _power = value; }
        }
        private float _speed;
        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        private bool isChip;
        public bool IsChip
        {
            get { return isChip; }
            set { isChip = value; }
        }
        public int Save { get; set; }

        public override byte[] Serialize()
        {
            SendBytes = new byte[32];
            SendBytes[0] = PacketKind;
            SendBytes[1] = 0;
            SendBytes[2] = RobotID;
            SendBytes[3] = Power;

            SendBytes[4] = (byte)((IsChip) ? 1 : 0);
            SendBytes[5] = (byte)(_speed * 10);
            SendBytes[6] = (byte)Save;

            //SendBytes[4] = BitConverter.GetBytes(_speed)[0];
            //SendBytes[5] = BitConverter.GetBytes(_speed)[1];
            //SendBytes[6] = BitConverter.GetBytes(_speed)[2];
            //SendBytes[7] = BitConverter.GetBytes(_speed)[3];
            return SendBytes;
        }

        public override WirelessPacket Deserialize()
        {
            throw new NotImplementedException();
        }
    }
}
