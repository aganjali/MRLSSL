using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class RequestPacketinDebugMode : WirelessPacket
    {
        #region "Properties"
        private byte _RobotID;

        public byte RobotID
        {
            get { return _RobotID; }
            set { _RobotID = value; }
        }
        private Position2D _V;

        public Position2D V
        {
            get { return _V; }
            set { _V = value; }
        }
        private float _Omega;

        public float Omega
        {
            get { return _Omega; }
            set { _Omega = value; }
        }
        private bool _isallWheel;

        public bool IsallWheel
        {
            get { return _isallWheel; }
            set { _isallWheel = value; }
        }
        private int _WheelNum;

        public int WheelNum
        {
            get { return _WheelNum; }
            set { _WheelNum = value; }
        }
        private bool _SpeedData;

        public bool SpeedData
        {
            get { return _SpeedData; }
            set { _SpeedData = value; }
        }
        private bool _PIDError;

        public bool PIDError
        {
            get { return _PIDError; }
            set { _PIDError = value; }
        }
        private bool _PIDCoef;

        public bool PIDCoef
        {
            get { return _PIDCoef; }
            set { _PIDCoef = value; }
        }
        private bool _WS;

        public bool WS
        {
            get { return _WS; }
            set { _WS = value; }
        }
        private bool _PWM;

        public bool PWM
        {
            get { return _PWM; }
            set { _PWM = value; }
        }

        private Int16 _PWM1;
        
        public Int16 PWM1
        {
            get { return _PWM1; }
            set { _PWM1 = value; }
        }

        private Int16 _PWM2;

        public Int16 PWM2
        {
            get { return _PWM2; }
            set { _PWM2 = value; }
        }
        private Int16 _PWM3;

        public Int16 PWM3
        {
            get { return _PWM3; }
            set { _PWM3 = value; }
        }
        private Int16 _PWM4;

        public Int16 PWM4
        {
            get { return _PWM4; }
            set { _PWM4 = value; }
        }
        
        #endregion
        public override byte[] Serialize()
        {
            SendBytes = new byte[32];
            SendBytes[0] = RobotID;

            SendBytes[1] = BitConverter.GetBytes((float)V.X)[0];
            SendBytes[2] = BitConverter.GetBytes((float)V.X)[1];
            SendBytes[3] = BitConverter.GetBytes((float)V.X)[2];
            SendBytes[4] = BitConverter.GetBytes((float)V.X)[3];

            SendBytes[5] = BitConverter.GetBytes((float)V.Y)[0];
            SendBytes[6] = BitConverter.GetBytes((float)V.Y)[1];
            SendBytes[7] = BitConverter.GetBytes((float)V.Y)[2];
            SendBytes[8] = BitConverter.GetBytes((float)V.Y)[3];

            SendBytes[9] = BitConverter.GetBytes(Omega)[0];
            SendBytes[10] = BitConverter.GetBytes(Omega)[1];
            SendBytes[11] = BitConverter.GetBytes(Omega)[2];
            SendBytes[12] = BitConverter.GetBytes(Omega)[3];

            if (PWM)
                SendBytes[13] |= 128;

            if (WS)
                SendBytes[13] |= 1;

            if (PIDCoef)
                SendBytes[13] |= 2;

            if (PIDError)
                SendBytes[13] |= 4;

            if (SpeedData)
                SendBytes[13] |= 8;

            if (WheelNum == 2)
            {
                SendBytes[13] |= 16;
                SendBytes[13] |= 0;
            }
            if (WheelNum == 3)
            {
                SendBytes[13] |= 0;
                SendBytes[13] |= 32;
            }
            if (WheelNum == 4)
            {
                SendBytes[13] |= 16;
                SendBytes[13] |= 32;
            }

            if (IsallWheel)
                SendBytes[13] |= 64;

            SendBytes[14] = BitConverter.GetBytes(PWM1)[0];
            SendBytes[15] = BitConverter.GetBytes(PWM1)[1];
            SendBytes[16] = BitConverter.GetBytes(PWM2)[0];
            SendBytes[17] = BitConverter.GetBytes(PWM2)[1];
            SendBytes[18] = BitConverter.GetBytes(PWM3)[0];
            SendBytes[19] = BitConverter.GetBytes(PWM3)[1];
            SendBytes[20] = BitConverter.GetBytes(PWM4)[0];
            SendBytes[21] = BitConverter.GetBytes(PWM4)[1];

            return SendBytes;
        }
        public override WirelessPacket Deserialize()
        {
            return null;
        }
    }
}
