using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.GameDefinitions
{
    public class BatteryStatus : WirelessPacket
    {
        private float _BatteryStatus;
        private byte _RobotID;
        public byte RobotID
        {
            get { return _RobotID; }
            set { _RobotID = value; }
        }
        public float BatteryLife
        {
            get { return _BatteryStatus; }
            set { _BatteryStatus = value;}
        }
        public BatteryStatus()
        {
        }
        public BatteryStatus(byte[] RecievedData)
        {
            ReciveBytes = RecievedData;
        }
        public override byte[] Serialize()
        {
            return null;
        }
        public override WirelessPacket Deserialize()
        {
            BatteryStatus Ret = new BatteryStatus();
            bool returnedPacket = CheckBattreyFormat(RobotID);
            if (returnedPacket == true)
            {
                byte[] BattryCharge = new byte[4];
                Ret.RobotID = ReciveBytes[0];
                BattryCharge[0] = ReciveBytes[1];
                BattryCharge[1] = ReciveBytes[2];
                BattryCharge[2] = ReciveBytes[3];
                BattryCharge[3] = ReciveBytes[4];
                Ret.BatteryLife = BitConverter.ToSingle(BattryCharge, 0);
                return Ret;
            }
            return null;
        }
        private bool CheckBattreyFormat(byte RobotID)
        {
            if (ReciveBytes[0] /*!= RobotID*/ > 8)
                return false;
            return true;
        }

    }
}
