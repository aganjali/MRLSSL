using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.GameDefinitions
{
    public class WheelSpeedData : WirelessPacket
    {
        private double _wheelspeed1;
        public double Wheelspeed1
        {
            get { return _wheelspeed1; }
            set { _wheelspeed1 = value; }
        }
        private double _wheelspeed2;
        public double Wheelspeed2
        {
            get { return _wheelspeed2; }
            set { _wheelspeed2 = value; }
        }
        private double _wheelspeed3;
        public double Wheelspeed3
        {
            get { return _wheelspeed3; }
            set { _wheelspeed3 = value; }
        }
        private double _wheelspeed4;
        public double Wheelspeed4
        {
            get { return _wheelspeed4; }
            set { _wheelspeed4 = value; }
        }

        public override byte[] Serialize()
        {
            return null;
        }

        public override WirelessPacket Deserialize()
        {
            return ParseContent(ReciveBytes);
        }

        private WheelSpeedData ParseContent(byte[] Content)
        {
            WheelSpeedData WRet = new WheelSpeedData();
            int coCounter = 0;
            while (coCounter < Content.Length)
            {
                switch ((char)Content[coCounter])
                {
                    case 'W':
                        WRet.Wheelspeed1 = BitConverter.ToSingle(Content, coCounter + 1);
                        WRet.Wheelspeed2 = BitConverter.ToSingle(Content, coCounter + 5);
                        WRet.Wheelspeed3 = BitConverter.ToSingle(Content, coCounter + 9);
                        WRet.Wheelspeed4 = BitConverter.ToSingle(Content, coCounter + 13);
                        coCounter += 17;
                        break;
                    case 'V':
                        coCounter += 13;
                        break;
                    case 'C':
                        if ((Content.Length) <= (coCounter + 14))
                        {
                            coCounter += 14;
                            break;
                        }
                        if (Content[coCounter + 14] == 'E' || Content[coCounter + 14] == 'W' || Content[coCounter + 14] == 'V' || Content[coCounter + 14] == 'R')
                            coCounter += 14;
                        else
                            coCounter += 53;

                        break;
                    case 'E':
                        if ((Content.Length) <= (coCounter + 14))
                        {
                            coCounter += 14;
                            break;
                        }
                        if (Content[coCounter + 14] == 'C' || Content[coCounter + 14] == 'W' || Content[coCounter + 14] == 'V' || Content[coCounter + 14] == 'R')
                            coCounter += 14;
                        else
                            coCounter += 53;
                    
                        break;
                    case 'M':
                        coCounter += 31; break;
                    default:
                        coCounter++;
                        break;
                }
            }
            return WRet;
        }

    }
}
