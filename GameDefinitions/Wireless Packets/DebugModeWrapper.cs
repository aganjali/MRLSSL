using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.Extentions;

namespace MRL.SSL.GameDefinitions
{
    public class DebugModeWrapper :WirelessPacket
    {
        public DebugModeWrapper(byte[] RecievedData)
        {
            ReciveBytes = RecievedData;

            _wSpeedData = new WheelSpeedData();
            _vData = new VelocityData();
            _pIDCoeff = new PIDCoeffData();
            _pIDError = new PIDErrorData();
            _motorCurrData = new MotorCurrentData();

            _wSpeedData.ReciveBytes = ReciveBytes;
            _vData.ReciveBytes = ReciveBytes;
            _pIDError.ReciveBytes = RecievedData;
            _pIDCoeff.ReciveBytes = RecievedData;
            _motorCurrData.ReciveBytes = RecievedData;
        }

        private MotorCurrentData _motorCurrData = null;

        public MotorCurrentData MotorCurrData
        {
            get { return _motorCurrData; }
            set { _motorCurrData = value; }
        }

        private PIDCoeffData _pIDCoeff = null;
        public PIDCoeffData PIDCoeff
        {
            get { return _pIDCoeff; }
            set { _pIDCoeff = value; }
        }

        private PIDErrorData _pIDError = null;
        public PIDErrorData PIDError
        {
            get { return _pIDError; }
            set { _pIDError = value; }
        }

        private VelocityData _vData = null;
        public VelocityData VData
        {
            get { return _vData; }
            set { _vData = value; }
        }

        private WheelSpeedData _wSpeedData = null;
        public WheelSpeedData WSpeedData
        {
            get { return _wSpeedData; }
            set { _wSpeedData = value; }
        }

        public override byte[] Serialize()
        {
            return null;
        }

        public override WirelessPacket Deserialize()
        {
            WSpeedData = _wSpeedData.Deserialize().As<WheelSpeedData>();
            VData = _vData.Deserialize().As<VelocityData>();
            PIDError = _pIDError.Deserialize().As<PIDErrorData>();
            PIDCoeff = _pIDCoeff.Deserialize().As<PIDCoeffData>();
            MotorCurrData = _motorCurrData.Deserialize().As<MotorCurrentData>();
            return this;
        }

    }
}
