using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class PIDErrorData : WirelessPacket
    {
        #region "PID Errors"
        public float[] PErr = new float[5], IErr = new float[5], DErr = new float[5];
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
        private PIDErrorData ParseContent(byte[] Content)
        {
            PIDErrorData PERet = new PIDErrorData();
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
                            PERet.WheelNum = Content[coCounter + 1];
                            PERet.PErr[PERet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 2);
                            PERet.IErr[PERet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 6);
                            PERet.DErr[PERet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 10);

                            coCounter += 14;
                            break;
                        }
                        if (Content[coCounter + 14] == 'C' || Content[coCounter + 14] == 'W' || Content[coCounter + 14] == 'V' || Content[coCounter + 14] == 'R')
                        {
                            PERet.WheelNum = Content[coCounter + 1];
                            PERet.PErr[PERet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 2);
                            PERet.IErr[PERet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 6);
                            PERet.DErr[PERet.WheelNum] = BitConverter.ToSingle(Content, coCounter + 10);

                            coCounter += 14;
                        }
                        else
                        {
                            int wNum = Content[coCounter + 1];
                            PERet.PErr[wNum] = BitConverter.ToSingle(Content, coCounter + 2);
                            PERet.IErr[wNum] = BitConverter.ToSingle(Content, coCounter + 6);
                            PERet.DErr[wNum] = BitConverter.ToSingle(Content, coCounter + 10);

                            wNum = Content[coCounter + 14];
                            PERet.PErr[wNum] = BitConverter.ToSingle(Content, coCounter + 15);
                            PERet.IErr[wNum] = BitConverter.ToSingle(Content, coCounter + 19);
                            PERet.DErr[wNum] = BitConverter.ToSingle(Content, coCounter + 23);

                            wNum = Content[coCounter + 27];
                            PERet.PErr[wNum] = BitConverter.ToSingle(Content, coCounter + 28);
                            PERet.IErr[wNum] = BitConverter.ToSingle(Content, coCounter + 32);
                            PERet.DErr[wNum] = BitConverter.ToSingle(Content, coCounter + 36);

                            wNum = Content[coCounter + 40];
                            PERet.PErr[wNum] = BitConverter.ToSingle(Content, coCounter + 41);
                            PERet.IErr[wNum] = BitConverter.ToSingle(Content, coCounter + 45);
                            PERet.DErr[wNum] = BitConverter.ToSingle(Content, coCounter + 49);
                            coCounter += 53;
                        }
                        break;
                    case 'M':
                        coCounter += 31; break;
                    default:
                        coCounter++;
                        break;
                }
            }
            return PERet;
        }

    }
}
