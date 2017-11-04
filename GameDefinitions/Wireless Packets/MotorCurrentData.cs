using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MRL.SSL.GameDefinitions
{
    public class MotorCurrentData : WirelessPacket
    {
        private float _motor1;
        private float _motor2;
        private float _motor3;
        private float _motor4;
        private float _motor5;

        public float Motor1
        {
            get{return _motor1;}
            set { _motor1 = value; }
        }
        public float Motor2
        {
            get { return _motor2; }
            set { _motor2 = value; }
        }
        public float Motor3
        {
            get { return _motor3; }
            set { _motor3 = value; }
        }
        public float Motor4
        {
            get { return _motor4; }
            set { _motor4 = value; }
        }
        public float Motor5
        {
            get { return _motor5; }
            set { _motor5 = value; }
        }
        

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public override WirelessPacket Deserialize()
        {
            return ParseContent(ReciveBytes);
        }
        private MotorCurrentData ParseContent(byte[] Content)
        {
            MotorCurrentData VRet = new MotorCurrentData();
            int coCounter = 0;
            
            
            while (coCounter < Content.Length)
            {
                switch ((char)Content[coCounter])
                {
                    case 'W':
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
                        {
                            VRet._motor1 = (float)Math.Round(BitConverter.ToSingle(Content, coCounter + 2), 2);
                            coCounter += 7;
                            VRet._motor2 = (float)Math.Round(BitConverter.ToSingle(Content, coCounter), 2);
                            coCounter += 5;
                            VRet._motor3 = (float)Math.Round(BitConverter.ToSingle(Content, coCounter), 2);
                            coCounter += 5;
                            VRet._motor4 = (float)Math.Round(BitConverter.ToSingle(Content, coCounter), 2);
                            coCounter += 5;
                            VRet._motor5 = (float)Math.Round(BitConverter.ToSingle(Content, coCounter), 2);
                            coCounter += 3;
                            break;
                        }
                    default:
                        coCounter++;
                        break;
                }
            }

            return VRet;
        }
    }
}
