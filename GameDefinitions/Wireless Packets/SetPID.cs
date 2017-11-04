using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class SetPID : WirelessPacket
    {
        private float _kp;
        public float Kp
        {
            get { return _kp; }
            set { _kp = value; }
        }
        private float _ki;
        public float Ki
        {
            get { return _ki; }
            set { _ki = value; }
        }
        private float _kd;
        public float Kd
        {
            get { return _kd; }
            set { _kd = value; }
        }
        private float _l;
        public float L
        {
            get { return _l; }
            set { _l = value; }
        }
        private Int16 _cof = 0;

        public Int16 Cof
        {
            get { return _cof; }
            set { _cof = value; }
        }

        private byte _RobotNumber = 0;
        public byte RobotNumber
        {
            get { return _RobotNumber; }
            set { _RobotNumber = value; }
        }
        private byte _WheelNumber = 0;
        public byte WheelNumber
        {
            get { return _WheelNumber; }
            set { _WheelNumber = value; }
        }

        private float _feedforward;
        public float feedforward
        {
            get { return _feedforward; }
            set { _feedforward = value; }
        }
        public override byte[] Serialize()
        {
            SendBytes = new byte[32];
            SendBytes[0] = 1;
            SendBytes[1] = 0;
            SendBytes[2] = (byte)RobotNumber;

            SendBytes[3] = BitConverter.GetBytes(Kp)[0];
            SendBytes[4] = BitConverter.GetBytes(Kp)[1];
            SendBytes[5] = BitConverter.GetBytes(Kp)[2];
            SendBytes[6] = BitConverter.GetBytes(Kp)[3];

            SendBytes[7] = BitConverter.GetBytes(Ki)[0];
            SendBytes[8] = BitConverter.GetBytes(Ki)[1];
            SendBytes[9] = BitConverter.GetBytes(Ki)[2];
            SendBytes[10] = BitConverter.GetBytes(Ki)[3];

            SendBytes[11] = BitConverter.GetBytes(Kd)[0];
            SendBytes[12] = BitConverter.GetBytes(Kd)[1];
            SendBytes[13] = BitConverter.GetBytes(Kd)[2];
            SendBytes[14] = BitConverter.GetBytes(Kd)[3];

            SendBytes[15] = BitConverter.GetBytes(L)[0];
            SendBytes[16] = BitConverter.GetBytes(L)[1];
            SendBytes[17] = BitConverter.GetBytes(L)[2];
            SendBytes[18] = BitConverter.GetBytes(L)[3];

            SendBytes[19] = BitConverter.GetBytes(feedforward)[0];
            SendBytes[20] = BitConverter.GetBytes(feedforward)[1];
            SendBytes[21] = BitConverter.GetBytes(feedforward)[2];
            SendBytes[22] = BitConverter.GetBytes(feedforward)[3];

            SendBytes[23] = BitConverter.GetBytes(Cof)[0];
            SendBytes[24] = BitConverter.GetBytes(Cof)[1];

            SendBytes[25] = (byte)WheelNumber;
            return SendBytes;
        }

        public override WirelessPacket Deserialize()
        {
            return null;
        }
    }
}
