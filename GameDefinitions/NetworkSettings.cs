using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class NetworkSettings
    {
        string _aiName;
        /// <summary>
        /// AI neme
        /// </summary>
        public string AiName
        {
            get { return _aiName; }
            set { _aiName = value; }
        }
        int _aiPort;
        /// <summary>
        /// AI port
        /// </summary>
        public int AiPort
        {
            get { return _aiPort; }
            set { _aiPort = value; }
        }
        string _cMCname;
        /// <summary>
        /// Visualizer Name
        /// </summary>
        public string CMCname
        {
            get { return _cMCname; }
            set { _cMCname = value; }
        }
        int _cMCport;
        /// <summary>
        /// Visualizer Port
        /// </summary>
        public int CMCport
        {
            get { return _cMCport; }
            set { _cMCport = value; }
        }
        string _simulatorName;
        /// <summary>
        /// Simulator Name
        /// </summary>
        public string SimulatorName
        {
            get { return _simulatorName; }
            set { _simulatorName = value; }
        }
        int _simulatorSendPort;
        /// <summary>
        /// Simulator Send Port
        /// </summary>
        public int SimulatorSendPort
        {
            get { return _simulatorSendPort; }
            set { _simulatorSendPort = value; }
        }
        int _simulatorRecievePort;
        /// <summary>
        /// Simulator Recieve Port
        /// </summary>
        public int SimulatorRecievePort
        {
            get { return _simulatorRecievePort; }
            set { _simulatorRecievePort = value; }
        }
        string _refIP;
        /// <summary>
        /// Refree Box Name
        /// </summary>
        public string RefIP
        {
            get { return _refIP; }
            set { _refIP = value; }
        }
        int _refPort;
        /// <summary>
        /// Refree Box Port
        /// </summary>
        public int RefPort
        {
            get { return _refPort; }
            set { _refPort = value; }
        }
        string _sSLVisionIP;
        /// <summary>
        /// ssl vision ip
        /// </summary>
        public string SSLVisionIP
        {
            get { return _sSLVisionIP; }
            set { _sSLVisionIP = value; }
        }
        int _sSLVisionPort;
        /// <summary>
        /// ssl vision port
        /// </summary>
        public int SSLVisionPort
        {
            get { return _sSLVisionPort; }
            set { _sSLVisionPort = value; }
        }
    }
}
