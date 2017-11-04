using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using MRL.SSL.CommonClasses.Communication;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Visualizer.Classes;

namespace Visualizer.Classes
{
    public class WirelessSender
    {
       
        /// <summary>
        /// 
        /// </summary>
        private bool _sendFlag = false;
        /// <summary>
        /// 
        /// </summary>
        public  bool SendFlag
        {
            get 
            {
                return _sendFlag;
            }
            set
            {
                _sendFlag = value;
                if (value)
                    PortManager = new SerialPortManager();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        SerialPortManager PortManager;
        /// <summary>
        /// 
        /// </summary>
        RobotCommands commands = null;
        /// <summary>
        /// 
        /// </summary>
        byte sequenc = 0;
        /// <summary>
        /// 
        /// </summary>
        public enum WirelessSendState
        {
            ON,
            OFF
        }
        /// <summary>
        /// 
        /// </summary>
        public WirelessSendState State = new WirelessSendState();
        /// <summary>
        /// 
        /// </summary>
        public string PortName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BoudRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        SerialPort SendPort;
        /// <summary>
        /// 
        /// </summary>
        public WirelessSender()
        {
            SendFlag = false;
            PortName = "Com1";
            BoudRate = 115200;
            SendPort = new SerialPort(PortName, BoudRate);
            DataReciever.MustSendToRobot += new DataReciever.DataRecievedAndSendToRobotEventHandler(DataReciever_MustSendToRobot);
        }

        void DataReciever_MustSendToRobot(object sender, Dictionary<int, SingleWirelessCommand> sendDataToRobots)
        {
            if (SendFlag && System.IO.Ports.SerialPort.GetPortNames().Contains(PortName))
                Send(sendDataToRobots);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Send(Dictionary<int, SingleWirelessCommand> sendDataToRobots)
        {
            sequenc++;
            commands.Commands = sendDataToRobots;
            PortManager.SendData(int.Parse(PortName.ToLower().Replace("com", "")), commands.ToByteMulticast(sequenc), false);
        }
    }
}
