using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.IO;
using Meta.Numerics.Matrices;
using Enterprise;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Commands of each robot and frame Number and time
    /// </summary>
    public class RobotCommands : ICloneable
    {
        public bool IsPenaltyMode = false;
        public int PenaltyShooterID = 0;
        public DateTime TimeSent;
        public int SequenceNumber;
        public Dictionary<int, SingleWirelessCommand> Commands;
        public Dictionary<int, SingleWirelessCommand> Command;

        //
        int Count = 0;
        public byte[] ToByteMulticast(byte sequenc)
        {
            try
            {
                if (sequenc > 255)
                {
                    Count = 0;
                    sequenc = 0;
                }
                else
                    Count++;
                byte[] wirelesscommand = new byte[64];
                byte[] finalCommand = new byte[70];

                //0'th Bit
                if (IsPenaltyMode)
                {
                    wirelesscommand[0] = 3;
                    wirelesscommand[32] = 3;
                }
                else
                {
                    wirelesscommand[0] = 0;
                    wirelesscommand[32] = 0;
                }

                //1'th bit
                wirelesscommand[1] = sequenc;
                sequenc++;
                wirelesscommand[33] = sequenc;


                int counter = 2;
                int IdCunter = 3;
                if (Commands.Count <= 8)
                {
                    for (int i = 0; i < Commands.Count / 2; i++)
                    {
                        var item = Commands.ElementAt(i);
                        int key = item.Key;
                        SingleWirelessCommand swc = Commands.ElementAt(i).Value ?? new SingleWirelessCommand();
                        swc.Vx = Math.Sign(swc.Vx) * Math.Min(Math.Abs(swc.Vx), 3.98);
                        swc.Vy = Math.Sign(swc.Vy) * Math.Min(Math.Abs(swc.Vy), 3.98);
                        swc.W = Math.Sign(swc.W) * Math.Min(Math.Abs(swc.W), 7.95);
                        //swc.Vx = 0;
                        //swc.Vy = -1;
                        //swc.W = 0;
                        // swc.SpinBack = 1;
                        int msb = (byte)(key >> 3) & (byte)1;
                        wirelesscommand[0] |= (byte)((byte)msb << IdCunter);
                        IdCunter++;
                        wirelesscommand[counter] |= (byte)((byte)key & (byte)7);
                        wirelesscommand[counter] |= (Math.Sign(swc.Vx) == -1) ? (byte)8 : (byte)0;
                        wirelesscommand[counter] |= (Math.Sign(swc.Vy) == -1) ? (byte)16 : (byte)0;
                        wirelesscommand[counter] |= (Math.Sign(swc.W) == -1) ? (byte)32 : (byte)0;
                        wirelesscommand[counter] |= (swc.statusRequest) ? (byte)64 : (byte)0;
                        wirelesscommand[counter] |= (swc.isDelayedKick) ? (byte)128 : (byte)0;

                        //
                        //swc.Vy = Math.Abs(swc.Vy);
                        //swc.Vx = Math.Abs(swc.Vx);
                        //swc.W = Math.Abs(swc.W);

                        double vy = Math.Abs(swc.Vy);
                        double vx = Math.Abs(swc.Vx);
                        double w = Math.Abs(swc.W);

                        if (swc.SpinBack > 0)
                            swc.SpinBack = 2;
                        wirelesscommand[counter + 1] |= (swc.spinBackward) ? (byte)1 : (byte)0;//Backward
                        wirelesscommand[counter + 1] |= (byte)(((int)swc.SpinBack));
                        wirelesscommand[counter + 1] |= (swc.isChipKick) ? (byte)(0) : (byte)(1 << 2);
                        wirelesscommand[counter + 1] |= (byte)(((int)vx) << 3);
                        wirelesscommand[counter + 1] |= (byte)(((int)vy) << 5);
                        //(LAST MODE FOR W)wirelesscommand[counter + 1] |= (byte)(((int)swc.W >> 1) << 7);
                        wirelesscommand[counter + 1] |= (byte)(((int)w >> 3) << 7);

                        //

                        wirelesscommand[counter + 2] = (byte)(int)((vx - (int)vx) * 256);

                        //

                        wirelesscommand[counter + 3] = (byte)(int)((vy - (int)vy) * 256);

                        //

                        //LAST MODE FOR W wirelesscommand[counter + 4] = (byte)((int)((swc.W - (int)swc.W) * 128) | ((((int)swc.W) & 1) << 7));
                        wirelesscommand[counter + 4] = (byte)((int)((w - (int)w) * 32) | ((int)w) << 5);

                        //
                        wirelesscommand[counter + 5] = (byte)swc._kickPowerByte;

                        //
                        counter += 6;
                    }
                    counter = 34;
                    IdCunter = 3;
                    for (int i = Commands.Count / 2; i < Commands.Count; i++)
                    {
                        var item = Commands.ElementAt(i);
                        int key = item.Key;
                        SingleWirelessCommand swc = Commands.ElementAt(i).Value ?? new SingleWirelessCommand();
                        swc.Vx = Math.Sign(swc.Vx) * Math.Min(Math.Abs(swc.Vx), 3.98);
                        swc.Vy = Math.Sign(swc.Vy) * Math.Min(Math.Abs(swc.Vy), 3.98);
                        swc.W = Math.Sign(swc.W) * Math.Min(Math.Abs(swc.W), 7.95);
                        //swc.Vx = 0;
                        //swc.Vy = -1;
                        //swc.W = 0;
                        // swc.SpinBack = 1;
                        int msb = (byte)(key >> 3) & (byte)1;
                        wirelesscommand[32] |= (byte)((byte)msb << IdCunter);
                        IdCunter++;
                        wirelesscommand[counter] |= (byte)((byte)key & (byte)7);
                        wirelesscommand[counter] |= (Math.Sign(swc.Vx) == -1) ? (byte)8 : (byte)0;
                        wirelesscommand[counter] |= (Math.Sign(swc.Vy) == -1) ? (byte)16 : (byte)0;
                        wirelesscommand[counter] |= (Math.Sign(swc.W) == -1) ? (byte)32 : (byte)0;
                        wirelesscommand[counter] |= (swc.statusRequest) ? (byte)64 : (byte)0;
                        wirelesscommand[counter] |= (swc.isDelayedKick) ? (byte)128 : (byte)0;

                        //
                        //swc.Vy = Math.Abs(swc.Vy);
                        //swc.Vx = Math.Abs(swc.Vx);
                        //swc.W = Math.Abs(swc.W);

                        double vy = Math.Abs(swc.Vy);
                        double vx = Math.Abs(swc.Vx);
                        double w = Math.Abs(swc.W);

                        if (swc.SpinBack > 0)
                            swc.SpinBack = 2;
                        wirelesscommand[counter + 1] |= (swc.spinBackward) ? (byte)1 : (byte)0;//Backward
                        wirelesscommand[counter + 1] |= (byte)(((int)swc.SpinBack));
                        wirelesscommand[counter + 1] |= (swc.isChipKick) ? (byte)(0) : (byte)(1 << 2);
                        wirelesscommand[counter + 1] |= (byte)(((int)vx) << 3);
                        wirelesscommand[counter + 1] |= (byte)(((int)vy) << 5);
                        //(LAST MODE FOR W)wirelesscommand[counter + 1] |= (byte)(((int)swc.W >> 1) << 7);
                        wirelesscommand[counter + 1] |= (byte)(((int)w >> 3) << 7);

                        //

                        wirelesscommand[counter + 2] = (byte)(int)((vx - (int)vx) * 256);

                        //

                        wirelesscommand[counter + 3] = (byte)(int)((vy - (int)vy) * 256);

                        //

                        //LAST MODE FOR W wirelesscommand[counter + 4] = (byte)((int)((swc.W - (int)swc.W) * 128) | ((((int)swc.W) & 1) << 7));
                        wirelesscommand[counter + 4] = (byte)((int)((w - (int)w) * 32) | ((int)w) << 5);

                        //
                        wirelesscommand[counter + 5] = (byte)swc._kickPowerByte;

                        //
                        counter += 6;
                    }
                    finalCommand[0] = 128;
                    finalCommand[1] = 128;
                    finalCommand[2] = 128;
                    for (int i = 0; i < 64; i++)
                    {
                        finalCommand[i + 3] = wirelesscommand[i];

                    }
                    finalCommand[67] = 129;
                    finalCommand[68] = 129;
                    finalCommand[69] = 129;

                }
                else
                {
                    throw new Exception();
                }
                return finalCommand;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return new byte[70];
            }
        }

        #region new method

        int RobotNum;
        byte[] packet;
        int[] roAddressStrart = { 5, 15, 25, 37, 47, 57, 69, 79, 89 };
        int[] roAddressEnd = { 14, 24, 34, 46, 56, 66, 78, 88, 98 };

        public byte[] SendrobotPacket(byte sequenc, SingleWirelessCommand Rwc, int RobotID)
        {
            Dictionary<int, SingleWirelessCommand> listWcommands = Commands;
            var listWcommand = listWcommands.OrderBy(o => o.Key);
            RobotNum = 0;
            packet = new byte[12];

            packet[0] = (byte)RobotID;
            packet[1] = sequenc;
            packet[2] = Rwc.Kind;

            byte[] Vx = getV(Rwc.Vx);
            packet[3] = Vx[0];
            packet[4] = Vx[1];

            byte[] Vy = getV(Rwc.Vy);
            packet[5] = Vy[0];
            packet[6] = Vy[1];

            byte[] W = getW(Rwc.W);
            packet[7] = W[0];
            packet[8] = W[1];

            packet[9] = (byte)(Math.Min(Rwc.KickPower, 255));
            packet[10] = getMode(Rwc);
            packet[11] = (Rwc.BackSensor) ? (byte)1 : (byte)0;//getChkSum(robotdata);
            return packet;
        }


        public byte[] CreatPacket(byte sequenc)
        {
            Dictionary<int, SingleWirelessCommand> listWcommands = Commands;
            var listWcommand = listWcommands.OrderBy(o => o.Key);
            RobotNum = 0;
            packet = new byte[102];
            packet[0] = 128;
            packet[1] = 128;
            packet[2] = 128;
            packet[99] = 129;
            packet[100] = 129;
            packet[101] = 129;

            if (listWcommand.Count() <= 8)
            {
                foreach (var item in listWcommand)
                {
                    RobotNum++;
                    if (RobotNum <= 3)   //first half packet
                    {
                        if (item.Key < 8)
                        {
                            packet[3] = (byte)(packet[3] | (int)Math.Pow(2, item.Key));
                        }
                        else if (item.Key >= 8)
                        {
                            int ID = item.Key - 8;
                            packet[4] = (byte)(packet[4] | (int)Math.Pow(2, ID));
                        }
                        RoPack(roAddressStrart[RobotNum - 1], roAddressEnd[RobotNum - 1], item.Value);
                    }
                    else if (RobotNum > 3 && RobotNum <= 6)               //sec half packet
                    {
                        if (item.Key < 8)
                        {
                            packet[35] = (byte)(packet[35] | (int)Math.Pow(2, item.Key));
                        }
                        else if (item.Key >= 8)
                        {
                            int ID = item.Key - 8;
                            packet[36] = (byte)(packet[36] | (int)Math.Pow(2, ID));
                        }
                        RoPack(roAddressStrart[RobotNum - 1], roAddressEnd[RobotNum - 1], item.Value);
                    }
                    else                 //sec half packet
                    {
                        if (item.Key < 8)
                        {
                            packet[67] = (byte)(packet[67] | (int)Math.Pow(2, item.Key));
                        }
                        else if (item.Key >= 8)
                        {
                            int ID = item.Key - 8;
                            packet[68] = (byte)(packet[68] | (int)Math.Pow(2, ID));
                        }
                        RoPack(roAddressStrart[RobotNum - 1], roAddressEnd[RobotNum - 1], item.Value);
                    }
                }
                packet[4] = (byte)(packet[4] | (sequenc << 4));
                packet[36] = (byte)(packet[36] | ((sequenc + 1) << 4));
                packet[68] = (byte)(packet[68] | ((sequenc + 2) << 4));
            }

            return packet;
        }

        private void RoPack(int start, int end, SingleWirelessCommand robotdata)
        {
            // packet[start] ~  packet[end] = robotdata==ropack
            byte[] roPack = new byte[10];

            if (robotdata.Kind == 5)
            {
                roPack[0] = robotdata.Kind;

                if (robotdata.SRC._clockWise == 1)
                    roPack[1] = 1;
                roPack[1] |= (byte)(robotdata.SRC._teta << 1);
                roPack[2] |= (byte)(((int)robotdata.SRC._vy) >> 8);
                roPack[3] |= (byte)(((int)robotdata.SRC._vy) & 255);

                roPack[4] |= (byte)((int)robotdata.SRC._omega);
                int d = (int)((robotdata.SRC._omega - (int)robotdata.SRC._omega) * 2048);
                roPack[4] |= (byte)((d & 0x3F) << 2);
                roPack[5] = (byte)(d >> 6);
                roPack[6] = (byte)robotdata.SRC.framecount;

                roPack[7] = (byte)(Math.Min(robotdata.KickPower, 255));
                roPack[8] = getMode(robotdata);
                roPack[9] = (robotdata.BackSensor) ? (byte)1 : (byte)0;//getChkSum(robotdata);

                for (int i = start, j = 0; i <= end; i++, j++)
                {
                    packet[i] = roPack[j];
                }
            }
            else
            {
                roPack[0] = robotdata.Kind;

                byte[] Vx = getV(robotdata.Vx);
                roPack[1] = Vx[0];
                roPack[2] = Vx[1];

                byte[] Vy = getV(robotdata.Vy);
                roPack[3] = Vy[0];
                roPack[4] = Vy[1];

                byte[] W = getW(robotdata.W);
                roPack[5] = W[0];
                roPack[6] = W[1];

                roPack[7] = (byte)(Math.Min(robotdata.KickPower, 255));
                roPack[8] = getMode(robotdata);
                roPack[9] = (robotdata.BackSensor) ? (byte)1 : (byte)0;//getChkSum(robotdata);

                getW(robotdata.W);
                for (int i = start, j = 0; i <= end; i++, j++)
                {
                    packet[i] = roPack[j];
                }
            }

        }

        private byte[] getV(double V)
        {
            byte[] VByte = new byte[2];
            VByte[0] |= (Math.Sign(V) == -1) ? (byte)1 : (byte)0;
            V = Math.Abs(V);
            VByte[0] |= (byte)(((int)V) << 1);
            //-------
            int d = (int)((V - (int)V) * 2048);
            VByte[1] = (byte)(int)(d >> 3);

            byte dec = (byte)d;
            VByte[0] |= (byte)(((byte)(dec << 5)));

            return VByte;
        }

        private byte[] getW(double W)
        {
            byte[] WByte = new byte[2];
            WByte[0] |= (Math.Sign(W) == -1) ? (byte)1 : (byte)0;
            W = Math.Abs(W);
            WByte[0] |= (byte)(((int)W) << 1);
            //--------
            int d = (int)((W - (int)W) * 512);
            WByte[1] = (byte)(int)(d >> 1);
            WByte[0] |= (byte)((d & 1) << 7);

            return WByte;
        }

        private byte getMode(SingleWirelessCommand robotdata)
        {
            byte mode = 0;

            if (!robotdata.isChipKick)
                mode |= 1;
            if (robotdata.isDelayedKick)
                mode |= 2;
            if (robotdata.spinBackward)
                mode |= 4;
            if (robotdata.statusRequest)
                mode |= 128;
            if (-1 * robotdata.SpinBack < 0)
                mode |= 8;
            mode |= (byte)((int)Math.Abs(robotdata.SpinBack) << 4);

            return mode;
        }

        private byte getChkSum(SingleWirelessCommand robotdata)
        {
            byte sum = 0;
            return sum;
        }

        #endregion

        #region
        //public byte[] ToByteMulticast(byte sequenc)
        //{
        //    try
        //    {
        //        if (sequenc > 255)
        //        {
        //            Count = 0;
        //            sequenc = 0;
        //        }
        //        else
        //            Count++;
        //        byte[] wirelesscommand = new byte[32];
        //        byte[] finalCommand = new byte[38];
        //        MemoryStream ms = new MemoryStream(wirelesscommand);
        //        BinaryWriter bw = new BinaryWriter(ms, ASCIIEncoding.ASCII);
        //        //0'th Bit
        //        if (IsPenaltyMode)
        //            wirelesscommand[0] = 3;
        //        else
        //            wirelesscommand[0] = 0;
        //        bw.Write(wirelesscommand[0]);
        //        //1'th bit
        //        wirelesscommand[1] = sequenc;
        //        bw.Write(wirelesscommand[1]);

        //        int counter = 2;
        //        int IdCunter = 3;
        //        if (Commands.Count <= 8)
        //        {
        //            foreach (int key in Commands.Keys)
        //            {
        //                if (Commands[key] == null)
        //                    Commands[key] = new SingleWirelessCommand();
        //                //if (key == 2)
        //                //    Commands[key].isChipKick = false;
        //                SingleWirelessCommand swc = Commands[key];
        //                swc.Vx = Math.Sign(swc.Vx) * Math.Min(Math.Abs(swc.Vx), 3.98);
        //                swc.Vy = Math.Sign(swc.Vy) * Math.Min(Math.Abs(swc.Vy), 3.98);
        //                swc.W = Math.Sign(swc.W) * Math.Min(Math.Abs(swc.W), 7.95);
        //                //swc.Vx = 0;
        //                //swc.Vy = -1;
        //                //swc.W = 0;
        //                // swc.SpinBack = 1;
        //                int msb = (byte)(key >> 3) & (byte)1;
        //                wirelesscommand[0] |= (byte)((byte)msb << IdCunter);
        //                IdCunter++;
        //                wirelesscommand[counter] |= (byte)((byte)key & (byte)7);
        //                wirelesscommand[counter] |= (Math.Sign(swc.Vx) == -1) ? (byte)8 : (byte)0;
        //                wirelesscommand[counter] |= (Math.Sign(swc.Vy) == -1) ? (byte)16 : (byte)0;
        //                wirelesscommand[counter] |= (Math.Sign(swc.W) == -1) ? (byte)32 : (byte)0;
        //                wirelesscommand[counter] |= (swc.statusRequest) ? (byte)64 : (byte)0;
        //                wirelesscommand[counter] |= (swc.isDelayedKick) ? (byte)128 : (byte)0;
        //                bw.Write(wirelesscommand[counter]);
        //                //
        //                //swc.Vy = Math.Abs(swc.Vy);
        //                //swc.Vx = Math.Abs(swc.Vx);
        //                //swc.W = Math.Abs(swc.W);

        //                double vy = Math.Abs(swc.Vy);
        //                double vx = Math.Abs(swc.Vx);
        //                double w = Math.Abs(swc.W);

        //                if (swc.SpinBack > 0)
        //                    swc.SpinBack = 2;
        //                wirelesscommand[counter + 1] |= (swc.spinBackward) ? (byte)1 : (byte)0;//Backward
        //                wirelesscommand[counter + 1] |= (byte)(((int)swc.SpinBack));
        //                wirelesscommand[counter + 1] |= (swc.isChipKick) ? (byte)(0) : (byte)(1 << 2);
        //                wirelesscommand[counter + 1] |= (byte)(((int)vx) << 3);
        //                wirelesscommand[counter + 1] |= (byte)(((int)vy) << 5);
        //                //(LAST MODE FOR W)wirelesscommand[counter + 1] |= (byte)(((int)swc.W >> 1) << 7);
        //                wirelesscommand[counter + 1] |= (byte)(((int)w >> 3) << 7);
        //                bw.Write(wirelesscommand[counter + 1]);
        //                //

        //                wirelesscommand[counter + 2] = (byte)(int)((vx - (int)vx) * 256);
        //                bw.Write(wirelesscommand[counter + 2]);
        //                //

        //                wirelesscommand[counter + 3] = (byte)(int)((vy - (int)vy) * 256);
        //                bw.Write(wirelesscommand[counter + 3]);
        //                //

        //                //LAST MODE FOR W wirelesscommand[counter + 4] = (byte)((int)((swc.W - (int)swc.W) * 128) | ((((int)swc.W) & 1) << 7));
        //                wirelesscommand[counter + 4] = (byte)((int)((w - (int)w) * 32) | ((int)w) << 5);
        //                bw.Write(wirelesscommand[counter + 4]);
        //                //
        //                wirelesscommand[counter + 5] = (byte)swc._kickPowerByte;
        //                bw.Write(wirelesscommand[counter + 5]);
        //                //
        //                counter += 6;
        //            }
        //            finalCommand[0] = 128;
        //            finalCommand[1] = 128;
        //            finalCommand[2] = 128;
        //            for (int i = 0; i < 32; i++)
        //            {
        //                finalCommand[i + 3] = wirelesscommand[i];
        //            }
        //            finalCommand[35] = 129;
        //            finalCommand[36] = 129;
        //            finalCommand[37] = 129;

        //        }
        //        else
        //        {
        //            throw new Exception();
        //        }
        //        return finalCommand;
        //    }
        //    catch { return new byte[32]; }
        //}
        #endregion

        public byte[] ToByteMatrix(byte sequenc, RectangularMatrix Mat, bool isEMat, bool save)
        {
            if (sequenc > 255)
            {
                Count = 0;
                sequenc = 0;
            }
            else
                Count++;
            byte[] wirelesscommand = new byte[32];
            byte[] finalCommand = new byte[38];
            MemoryStream ms = new MemoryStream(wirelesscommand);
            BinaryWriter bw = new BinaryWriter(ms, ASCIIEncoding.ASCII);
            //0'th Bit

            wirelesscommand[0] = 4;

            bw.Write(wirelesscommand[0]);
            //1'th bit
            wirelesscommand[1] = sequenc;
            bw.Write(wirelesscommand[1]);

            int counter = 2;
            int key = Command.First().Key;

            if (Command[key] == null)
                Command[key] = new SingleWirelessCommand();

            SingleWirelessCommand swc = Commands[key];
            swc.Vx = Math.Sign(swc.Vx) * Math.Min(Math.Abs(swc.Vx), 3.98);
            swc.Vy = Math.Sign(swc.Vy) * Math.Min(Math.Abs(swc.Vy), 3.98);
            swc.W = Math.Sign(swc.W) * Math.Min(Math.Abs(swc.W), 7.98);
            wirelesscommand[2] |= (byte)((byte)key & (byte)7);
            wirelesscommand[2] |= (Math.Sign(swc.Vx) == -1) ? (byte)8 : (byte)0;
            wirelesscommand[2] |= (Math.Sign(swc.Vy) == -1) ? (byte)16 : (byte)0;
            wirelesscommand[2] |= (Math.Sign(swc.W) == -1) ? (byte)32 : (byte)0;
            wirelesscommand[2] |= (swc.statusRequest) ? (byte)64 : (byte)0;
            wirelesscommand[2] |= (swc.isDelayedKick) ? (byte)128 : (byte)0;
            bw.Write(wirelesscommand[counter]);

            double vy = Math.Abs(swc.Vy);
            double vx = Math.Abs(swc.Vx);
            double w = Math.Abs(swc.W);

            if (swc.SpinBack > 0)
                swc.SpinBack = 2;
            wirelesscommand[3] |= (swc.spinBackward) ? (byte)1 : (byte)0;//Backward
            wirelesscommand[3] |= (byte)(((int)swc.SpinBack));
            wirelesscommand[3] |= (swc.isChipKick) ? (byte)(0) : (byte)(1 << 2);
            wirelesscommand[3] |= (byte)(((int)vx) << 3);
            wirelesscommand[3] |= (byte)(((int)vy) << 5);
            wirelesscommand[3] |= (byte)(((int)w >> 3) << 7);
            bw.Write(wirelesscommand[3]);
            //
            wirelesscommand[4] = (byte)(int)((vx - (int)vx) * 256);
            bw.Write(wirelesscommand[4]);
            //
            wirelesscommand[5] = (byte)(int)((vy - (int)vy) * 256);
            bw.Write(wirelesscommand[5]);
            //
            wirelesscommand[6] = (byte)((int)((w - (int)w) * 32) | ((int)w) << 5);
            bw.Write(wirelesscommand[6]);
            //
            wirelesscommand[7] = (byte)swc._kickPowerByte;
            bw.Write(wirelesscommand[7]);
            ///////////////////////////////////
            //                               //
            //      Send Matrix Data         //
            //                               //
            ///////////////////////////////////  
            if (save)
                wirelesscommand[0] |= 16;
            if (isEMat)
            {
                wirelesscommand[0] |= 8;
                for (int i = 0; i < 24; i += 2)
                {
                    double d = Math.Abs(Mat[(i / 2) % 4, i / 8]);
                    int intPart = (int)d;
                    int decimalPart = (int)((d - (int)d) * 512);
                    wirelesscommand[8 + i] = (byte)((intPart) & 63);
                    wirelesscommand[8 + i] |= (Math.Sign(Mat[(i / 2) % 4, i / 8]) == -1) ? (byte)64 : (byte)0;
                    wirelesscommand[8 + i] |= (byte)((decimalPart & 256) >> 1);
                    wirelesscommand[9 + i] = (byte)(decimalPart & 255);
                }
            }
            else
            {
                for (int i = 0; i < 8; i += 2)
                {
                    double d = Math.Abs(Mat[(i / 2) % 4, 0]);
                    int intPart = (int)d;
                    int decimalPart = (int)((d - (int)d) * 512);
                    wirelesscommand[8 + i] = (byte)((intPart) & 63);
                    wirelesscommand[8 + i] |= (Math.Sign(Mat[(i / 2) % 4, 0]) == -1) ? (byte)64 : (byte)0;
                    wirelesscommand[8 + i] |= (byte)((decimalPart & 256) >> 1);
                    wirelesscommand[9 + i] = (byte)(decimalPart & 255);
                }
            }
            finalCommand[0] = 128;
            finalCommand[1] = 128;
            finalCommand[2] = 128;
            for (int i = 0; i < 32; i++)
            {
                finalCommand[i + 3] = wirelesscommand[i];
            }
            finalCommand[35] = 129;
            finalCommand[36] = 129;
            finalCommand[37] = 129;
            return finalCommand;
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}