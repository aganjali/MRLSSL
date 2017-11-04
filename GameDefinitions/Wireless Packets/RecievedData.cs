using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MRL.SSL.CommonClasses.Communication;
using MRL.SSL.GameDefinitions.General_Settings;
using System.IO.Ports;
using Enterprise;

namespace MRL.SSL.GameDefinitions.Wireless_Packets
{

    public static class RecievedData
    {
        public enum RecieveState
        {
            On,
            Off
        }

        public static Dictionary<int, RobotStatus> LatestData = new Dictionary<int, RobotStatus>();
        static ManualResetEvent _recieveSignal;
        static Thread recieveThread;
        static bool State = false;
        public static int portno;
        static SerialPortManager serialport;

        public static void Initialize()
        {
            recieveThread = new Thread(new ThreadStart(recieve));
            _recieveSignal = new ManualResetEvent(true);
            serialport = new SerialPortManager();
            portno = AISettings.Default.RecievePort;



            recieveThread.Start();
            _recieveSignal.Set();
        }

        public static bool GetStatus()
        {
            return State;
        }

        public static void ChangeState(RecieveState newState)
        {
            switch (newState)
            {
                case RecieveState.On:
                    _recieveSignal.Set();
                    State = true;
                    break;
                case RecieveState.Off:
                    State = false;
                    _recieveSignal.Reset();
                    break;
                default:
                    break;
            }

        }

        static void recieve()
        {
            byte[] temp;
            temp = new byte[1];
            while (_recieveSignal.WaitOne())
            {
                try
                {
                    //serialport[portno].Read(temp, 0, 1);
                    //if (temp[0] == 125)
                    //    firstPart++;
                    //else
                    //    firstPart = 0;
                    //temp[0] = 0;
                    //if (firstPart == 3)
                    //{
                    //    firstPart = 0;
                    //    buffer = new byte[6];
                    //    serialport[portno].Read(buffer, 0, 6);
                    //    if (buffer.Skip(3).Any(a => a != 126))
                    //        continue;
                    //    RobotStatus rs = new RobotStatus();
                    //    byte fourth = buffer[0];
                    //    rs.RobotID = (byte)(fourth & (byte)15);
                    //    rs.SequenceNumber = (byte)((fourth & (byte)240) >> 4);
                    //    rs.Sensore = (((int)buffer[1]) == 0) ? false : true;
                    //    rs.BatteryLife = buffer[2];
                    //    LatestData[rs.RobotID] = rs;
                    //}
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex.ToString());
                }
            }
        }
    }
}
