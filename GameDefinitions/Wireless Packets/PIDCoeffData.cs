using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class PIDCoeffData : WirelessPacket
    {
        #region "PID Coefficient"
        public float[] P = new float[5], I = new float[5], D = new float[5];
        private int _WheelNum = 0;
        public int WheelNum
        {
            get { return _WheelNum; }
            set { _WheelNum = value; }
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
        private PIDCoeffData ParseContent(byte[] Content)
        {
            PIDCoeffData PCRet = new PIDCoeffData();
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
                            PCRet.WheelNum = Content[coCounter + 1];
                            PCRet.P[PCRet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 2);
                            PCRet.I[PCRet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 6);
                            PCRet.D[PCRet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 10);

                            coCounter += 14;
                            break;
                        }
                        if (Content[coCounter + 14] == 'E' || Content[coCounter + 14] == 'W' || Content[coCounter + 14] == 'V' || Content[coCounter + 14] == 'R')
                        {
                            PCRet.WheelNum = Content[coCounter + 1];
                            PCRet.P[PCRet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 2);
                            PCRet.I[PCRet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 6);
                            PCRet.D[PCRet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 10);

                            coCounter += 14;
                        }
                        else
                        {
                            int wNum = Content[coCounter + 1];
                            PCRet.P[wNum] = BitConverter.ToSingle(Content, coCounter + 2);
                            PCRet.I[wNum] = BitConverter.ToSingle(Content, coCounter + 6);
                            PCRet.D[wNum] = BitConverter.ToSingle(Content, coCounter + 10);

                            wNum = Content[coCounter + 14];
                            PCRet.P[wNum] = BitConverter.ToSingle(Content, coCounter + 15);
                            PCRet.I[wNum] = BitConverter.ToSingle(Content, coCounter + 19);
                            PCRet.D[wNum] = BitConverter.ToSingle(Content, coCounter + 23);

                            wNum = Content[coCounter + 27];
                            PCRet.P[wNum] = BitConverter.ToSingle(Content, coCounter + 28);
                            PCRet.I[wNum] = BitConverter.ToSingle(Content, coCounter + 32);
                            PCRet.D[wNum] = BitConverter.ToSingle(Content, coCounter + 36);

                            wNum = Content[coCounter + 40];
                            PCRet.P[wNum] = BitConverter.ToSingle(Content, coCounter + 41);
                            PCRet.I[wNum] = BitConverter.ToSingle(Content, coCounter + 45);
                            PCRet.D[wNum] = BitConverter.ToSingle(Content, coCounter + 49);
                            coCounter += 53;
                        }
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
            return PCRet;
        }

    }
}
