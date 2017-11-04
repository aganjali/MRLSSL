using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Visualizer.Extentions;
using Enterprise;

namespace Visualizer.Classes
{
    public static class WirelessReciever
    {
        public static int RecieveCount = 0;
        static int Iteration = 0;
        static int MaxIteration = 10;
        public static BatteryStatus CurrentRecieved = new BatteryStatus();
        public static DebugModeWrapper DBWrapper;
        public enum WirelessRecieveState
        {
            OFF,
            ON
        }
        public static WirelessRecieveState State = WirelessRecieveState.OFF;
        private static string _protName = "Com1";
        public static string ProtName
        {
            get { return WirelessReciever._protName; }
            set
            {
                WirelessReciever._protName = value;
                if (State==WirelessRecieveState.ON)
                {
                    State = WirelessRecieveState.OFF;
                    if (recievePort.IsOpen)
                        recievePort.Close();
                    recievePort.PortName = value;
                    State = WirelessRecieveState.ON;
                }
                else
                {
                    if (recievePort.IsOpen)
                        recievePort.Close();
                    recievePort.PortName = value;
                }
            }
        }
        public static int BoudRate = 115200;
        public static ManualResetEvent recieveState = new ManualResetEvent(false);
        static Thread recieveThread = null;
        static SerialPort recievePort = new SerialPort(_protName, BoudRate);
       
        public static void ClosePort()
        {
            if (recievePort.IsOpen)
                recievePort.Close();
        }
        public static void OpenPort()
        {
            if (!recievePort.IsOpen)
                recievePort.Open();
        }
        public static void ChangeState(WirelessRecieveState Set)
        {
            if (Set == WirelessRecieveState.OFF)
            {
                if (State == WirelessRecieveState.OFF)
                    return;
                else
                {
                    State = WirelessRecieveState.OFF;
                    ClosePort();
                    recieveState.Reset();
                }
            }
            else
            {
                if (State == WirelessRecieveState.ON)
                    return;
                else
                {
                    State = WirelessRecieveState.ON;
                    if (recieveThread == null)
                    {
                        OpenPort();
                        recieveThread = new Thread(new ThreadStart(Recieve));
                        recieveThread.Start();
                    }
                    recieveState.Set();
                }
            }
        }
        private static void Recieve()
        {

            if (!recievePort.IsOpen)
                recievePort.Open();
            recievePort.ReadTimeout = 10000;
            recievePort.ReceivedBytesThreshold = 38;
            MotorCurrentData last = null;
            recievePort.DataReceived += new SerialDataReceivedEventHandler(recievePort_DataReceived);

            //while (recieveState.WaitOne())
            //{
            //    DBWrapper = RecieveOnePacket(false);
            //    if (DBWrapper != null && DBWrapper.MotorCurrData != null && MotorCurrentRecieved != null)
            //    {
            //        last = DBWrapper.MotorCurrData;
            //        MotorCurrentRecieved(DBWrapper.MotorCurrData);
            //    }
            //    else if (MotorCurrentRecieved != null && last != null)
            //    {
            //        MotorCurrentRecieved(last);
            //    }

            //    //CurrentRecieved = RecieveOneBatteryPacket();
            //    //if (StatusRecieved != null && CurrentRecieved != null)
            //    //    StatusRecieved(CurrentRecieved);

            //    //if (!recievePort.IsOpen)
            //    //    recievePort.ReadExisting();
            //    Thread.Sleep(16);
            //}
        }

        static void recievePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            RecieveCount++;
            //List<byte> bBuffer = new List<byte>();
            //while (recievePort.BytesToRead > 0)
            //    bBuffer.Add((byte)recievePort.ReadByte());
            //string ex = recievePort.ReadExisting();
            //bBuffer.AddRange(strtobyte(ex));
            BatteryStatus BatteryLS;
            byte[] temp = new byte[38];
            int buf = recievePort.BytesToRead;
            recievePort.Read(temp, 0, 38);
            recievePort.ReadExisting();
            if (temp[0] == 128 && temp[1] == 128 && temp[2] == 128
                && temp[35] == 129 && temp[36] == 129 && temp[37] == 129)
            {
                List<byte> res = temp.ToList();
                res.RemoveRange(0, 3);
                res.RemoveAt(res.Count - 1);
                res.RemoveAt(res.Count - 1);
                res.RemoveAt(res.Count - 1);
                if (res.Count > 0)
                {
                    byte sens= res[5];
                    BatteryLS = new BatteryStatus(res.ToArray());
                    BatteryLS = (BatteryStatus)BatteryLS.Deserialize();
                    
                    if (StatusRecieved != null && BatteryLS != null)
                        StatusRecieved(BatteryLS);

                    if (SensorRecieved != null)
                        SensorRecieved((sens == 1) ? true : false, (int)res[0]);
                }
            }
        }

        private static List<byte> strtobyte(string str)
        {
            List<byte> ret=new List<byte>();
            str.ToList().ForEach(p => ret.Add((byte)p));
            return ret;
        }

        private static BatteryStatus RecieveOneBatteryPacket()
        {
            BatteryStatus BatteryLS;
            Iteration = 0;
            int num128 = 0;
            byte[] tempByte = new byte[1];
            byte[] ContentByte;

            bool midPacket = false;
            bool isAvailable = false;
            while (isAvailable == false)
            {
                try
                {
                    if (midPacket == false)
                        recievePort.Read(tempByte, 0, 1);
                    else
                    {
                        ContentByte = new byte[35];
                        recievePort.Read(ContentByte, 0, 35);
                        if (ContentByte[32] == 129 && ContentByte[33] == 129 && ContentByte[34] == 129)
                        {
                            List<byte> tmp = ContentByte.ToList<byte>();
                            tmp.RemoveAt(tmp.Count - 1);
                            tmp.RemoveAt(tmp.Count - 1);
                            tmp.RemoveAt(tmp.Count - 1);
                            ContentByte = tmp.ToArray();
                            if (ContentByte.Length != 0)
                            {
                                BatteryLS = new BatteryStatus(ContentByte);
                                BatteryLS = (BatteryStatus)BatteryLS.Deserialize();
                            }
                            else
                                BatteryLS = null;
                        }
                        else
                            BatteryLS = null;
                        return BatteryLS;
                    }
                    if (tempByte[0] != 128 && num128 != 0) { num128 = 0; }
                    if (tempByte[0] == 128 && midPacket == false) { num128++; }
                    if (num128 == 3)
                    {
                        num128 = 0;
                        midPacket = true;
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Exception, e.ToString());

                    Iteration++;
                    if (MaxIteration >= 1)
                        System.Windows.Forms.Application.DoEvents();
                    if (Iteration < MaxIteration)
                        continue;
                    else
                        return null;
                }
            }
            return null;
        }

        private static DebugModeWrapper RecieveOnePacket(bool isPWMSet)
        {
            int num128 = 0;
            int Iteration = 0;
            int SizeofPacket = -1;
            byte[] tempByte = new byte[1];
            byte[] ContentByte;

            bool midPacket = false;
            bool isAvailable = false;
            while (isAvailable == false)
            {
                try
                {
                    if (SizeofPacket == -1)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        recievePort.Read(tempByte, 0, 1);
                    }
                    else
                    {
                        ContentByte = new byte[SizeofPacket];
                        recievePort.Read(ContentByte, 0, SizeofPacket);
                        if (ContentByte[SizeofPacket - 3] == 129 && ContentByte[SizeofPacket - 2] == 129 && ContentByte[SizeofPacket - 1] == 129)
                        {
                            List<byte> tmp = ContentByte.ToList<byte>();
                            tmp.RemoveAt(tmp.Count - 1);
                            tmp.RemoveAt(tmp.Count - 1);
                            tmp.RemoveAt(tmp.Count - 1);
                            ContentByte = tmp.ToArray();
                            if (ContentByte.Length != 0)
                            {
                                DBWrapper = new DebugModeWrapper(ContentByte);
                                DBWrapper.Deserialize();
                                if (isPWMSet == true)
                                {
                                    DBWrapper.WSpeedData.Wheelspeed1 *= 0.1212;
                                    DBWrapper.WSpeedData.Wheelspeed2 *= 0.1212;
                                    DBWrapper.WSpeedData.Wheelspeed3 *= 0.1212;
                                    DBWrapper.WSpeedData.Wheelspeed4 *= 0.1212;
                                }
                            }
                            else
                                DBWrapper = null;
                        }
                        else
                            DBWrapper = null;
                        return DBWrapper;
                    }
                    if (tempByte[0] != 128 && num128 != 0) { num128 = 0; }
                    if (tempByte[0] == 128 && midPacket == false) { num128++; }
                    if (num128 == 3)
                    {
                        num128 = 0;
                        tempByte[0] = 0;
                        midPacket = true;

                        recievePort.Read(tempByte, 0, 1);
                        SizeofPacket = tempByte[0] - 4;
                        if (SizeofPacket < 0)
                        {
                            return null;
                        }
                    }
                }
                catch (Exception err)
                {
                     
                    Logger.Write(LogType.Exception, err.ToString());
                    if (Iteration > 10)
                        return null;
                    Iteration++;
                    continue;
                }
            }
            return null;
        }

        public delegate void RecievedBattery(BatteryStatus sender);
        public static event RecievedBattery StatusRecieved;

        public delegate void RecievedSensor(bool isOn,int robotID);
        public static event RecievedSensor SensorRecieved;

        public delegate void RecievedCurrent(MotorCurrentData sender);
        public static event RecievedCurrent MotorCurrentRecieved;


    }
}
