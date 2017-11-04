using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class VelocityData : WirelessPacket
    {
        #region "Public Variables"
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
        public double Wheel1
        {
            get
            {
                double WS1 = (-18.7806f * V.X) + (28.9196f * V.Y) + (2.7794f * Omega);
                return WS1;
            }

        }
        public double Wheel2
        {
            get
            {
                double WS2 = (24.38299f * V.X) + (24.38299f * V.Y) + (2.7794f * Omega);
                return WS2;
            }
        }
        public double Wheel3
        {
            get
            {
                double WS3 = (-18.7806f * V.X) - (28.9196f * V.Y) + (2.7794f * Omega);
                return WS3;
            }
        }
        public double Wheel4
        {
            get
            {
                double WS4 = (24.38299f * V.X) - (24.38299f * V.Y) + (2.7794f * Omega);
                return WS4;
            }
        }
        #endregion

        public override byte[] Serialize()
        {
            return null;
        }
        public override WirelessPacket Deserialize()
        {
            return ParseContent(ReciveBytes);
        }
        private VelocityData ParseContent(byte[] Content)
        {
            VelocityData VRet = new VelocityData();
            int coCounter = 0;
            while (coCounter < Content.Length)
            {
                switch ((char)Content[coCounter])
                {
                    case 'W':
                        coCounter += 17;
                        break;
                    case 'V':
                        VRet.V = new Position2D(BitConverter.ToSingle(Content, coCounter + 1), BitConverter.ToSingle(Content, coCounter + 5));
                        VRet.Omega = BitConverter.ToSingle(Content, coCounter + 9);
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
            return VRet;
        }

    }
}
