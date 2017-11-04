using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using Enterprise;

namespace MRL.SSL.CommonClasses.Communication
{
    public class SerialPortManager
    {
        Dictionary<int, SerialPort> _ports = new Dictionary<int, SerialPort>();
        /// <summary>
        /// Generating Serial Port,Depends on the how many serial port is in use
        /// </summary>
        /// <param name="PortNo">Number of Port,Port1 or Port2</param>
        /// <returns>Serial Port with the sepecefic number</returns>
        public SerialPort this[int PortNo]
        {
            get
            {
                SerialPort sp = null;
                _lock.EnterUpgradeableReadLock();
                bool exists = _ports.TryGetValue(PortNo, out sp);
                if (exists)
                {
                    _lock.ExitUpgradeableReadLock();
                    return sp;
                }
                _lock.EnterWriteLock();
                if (!_ports.ContainsKey(PortNo))
                {
                    sp = CreateSerialPort(PortNo);
                    _ports.Add(PortNo, sp);
                }
                _lock.ExitWriteLock();
                _lock.ExitUpgradeableReadLock();
                if (sp != null)
                    return sp;
                return this[PortNo];
            }
        }
        /// <summary>
        /// Reciviong Port Number in full duplex communication = 7
        /// </summary>
        private int _receivePortNo = 7;
        private SerialPort _receivePort;
        /// <summary>
        /// in serial port recive get the port number and after that creat it and add it to ports
        /// </summary>
        public int RecievePortNo
        {
            get { return _receivePortNo; }
            set
            {
                _receivePortNo = value;
                if (!_ports.TryGetValue(value, out _receivePort))
                    _ports.Add(value, _receivePort = CreateSerialPort(value));
            }
        }

        private bool _enabled = true;
        /// <summary>
        /// Enable the ports
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (!_enabled)
                {
                    _lock.EnterWriteLock();
                    int[] portKeys = _ports.Keys.ToArray<int>();
                    foreach (int key in portKeys)
                    {
                        SerialPort sp = this[key];
                        lock (sp)
                        {
                            sp.Close();
                            sp.Dispose();
                            _ports.Remove(key);
                        }
                    }
                    _lock.ExitWriteLock();
                }
            }
        }

        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Write some byte in specific port
        /// </summary>
        /// <param name="PortNo">Number of Port,Port1 or Port2</param>
        /// <param name="Data">the bytes that have to write on the port</param>
        /// <param name="RequestToRead">boolian request data that show if packet has to have a request bit or not</param>
        public void SendData(int PortNo, byte[] Data, bool RequestToRead)
        {
            if (!_enabled)
                return;

            SerialPort sp = this[PortNo];
            lock (sp)
            {
                sp.Write(Data, 0, Data.Length);
            }



        }
        /// <summary>
        /// Creat Serial Port
        /// </summary>
        /// <param name="No">Port Number</param>
        /// <returns>a serial port that is in pc</returns>
        protected SerialPort CreateSerialPort(int No)
        {
            SerialPort sp = new SerialPort();
            sp.BaudRate = 115200;//38400;
            sp.DataBits = 8;
            sp.StopBits = StopBits.One;
            sp.Parity = Parity.None;
            sp.PortName = GetPortName(No);
            if (!sp.IsOpen)
                sp.Open();
            return sp;
        }
        /// <summary>
        /// Generate a string for Port,1 will be Port1
        /// </summary>
        /// <param name="No">Port Number</param>
        /// <returns>string format of Port Name</returns>
        public string GetPortName(int No)
        {
            if (No > 0)
                return "COM" + No.ToString();
            else
                return "COM9";
        }

        private TimeSpan _dataRequestTimeOut = TimeSpan.FromMilliseconds(1000);
        /// <summary>
        /// a timer for invalidating the data request timeout
        /// </summary>
        public TimeSpan DataRequestTimeOut
        {
            get { return _dataRequestTimeOut; }
            set { _dataRequestTimeOut = value; }
        }


    }
}