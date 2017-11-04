using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.GameDefinitions
{
    public class VisToAiDataWrapper
    {
        private bool _requestLockupTable;
        /// <summary>
        /// request for lockupTable
        /// </summary>
        public bool RequestLockupTable
        {
            get { return _requestLockupTable; }
            set { _requestLockupTable = value; }
        }
        private bool _sendPlaySetting;
        /// <summary>
        /// if be true send play settings to Ai
        /// </summary>
        public bool SendPlaySetting
        {
            get { return _sendPlaySetting; }
            set { _sendPlaySetting = value; }
        }
        private bool _sendTechinques;
        /// <summary>
        /// if be true send technique to ai
        /// </summary>
        public bool SendTechinques
        {
            get { return _sendTechinques; }
            set { _sendTechinques = value; }
        }
        private bool _requestTechniques;
        /// <summary>
        /// request from vis for ai (if be true must give techniques in next frame) 
        /// </summary>
        public bool RequestTechniques
        {
            get { return _requestTechniques; }
            set { _requestTechniques = value; }
        }
        private bool _changTechniques;
        /// <summary>
        /// if technique be chang this flag be true
        /// </summary>
        public bool ChangTechniques
        {
            get { return _changTechniques; }
            set { _changTechniques = value; }
        }
        private bool _changSim;
        /// <summary>
        /// if simulator parameters be change this flag be true
        /// </summary>
        public bool ChangSim
        {
            get { return _changSim; }
            set { _changSim = value; }
        }
        private bool _sendCustomVar;
        /// <summary>
        /// if vis send custom variable to ai this flag be true 
        /// </summary>
        public bool SendCustomVar
        {
            get { return _sendCustomVar; }
            set { _sendCustomVar = value; }
        }
        private bool _requestCustomVar;
        /// <summary>
        /// request from vis to ai for give customvariable
        /// </summary>
        public bool RequestCustomVar
        {
            get { return _requestCustomVar; }
            set { _requestCustomVar = value; }
        }
        private bool _disableVisualizerExrtas;
        /// <summary>
        /// if be true user drawing in visualizer be cancel
        /// </summary>
        public bool DisableVisualizerExrtas
        {
            get { return _disableVisualizerExrtas; }
            set { _disableVisualizerExrtas = value; }
        }
        private bool _selectionBallMode;
        /// <summary>
        /// flag for ball selection mode
        /// </summary>
        public bool SelectionBallMode
        {
            get { return _selectionBallMode; }
            set { _selectionBallMode = value; }
        }
        private Position2D _ballLocation;
        /// <summary>
        /// position of selected ball
        /// </summary>
        public Position2D BallLocation
        {
            get { return _ballLocation; }
            set { _ballLocation = value; }
        }
        private int? _selectedBallIndex;
        /// <summary>
        /// index of selected ball in ball list
        /// </summary>
        public int? SelectedBallIndex
        {
            get { return _selectedBallIndex; }
            set { _selectedBallIndex = value; }
        }
        private bool _isSimulating;
        /// <summary>
        /// is simulator data or vision data
        /// </summary>
        public bool IsSimulating
        {
            get { return _isSimulating; }
            set { _isSimulating = value; }
        }
        private bool _refreeCommandChange;
        /// <summary>
        /// if refree command be chang this flag be true
        /// </summary>
        public bool RefreeCommandChange
        {
            get { return _refreeCommandChange; }
            set { _refreeCommandChange = value; }
        }
        Dictionary<int, SingleWirelessCommand> _robotcommand;
        /// <summary>
        /// commands for our robots 
        /// </summary>
        public Dictionary<int, SingleWirelessCommand> Robotcommand
        {
            get { return _robotcommand; }
            set { _robotcommand = value; }
        }
        private string _refreeCommand;
        /// <summary>
        /// refree commnad that send from vis to ai
        /// </summary>
        public string RefreeCommand
        {
            get { return _refreeCommand; }
            set { _refreeCommand = value; }
        }
        private Dictionary<string, double> _customVariables;
        /// <summary>
        /// Variables that users want to tune in AI,Declare in cunstructor
        /// </summary>
        public Dictionary<string, double> CustomVariables
        {
            get { return _customVariables; }
            set { _customVariables = value; }
        }
        private Dictionary<string, bool> _requestTable;
        /// <summary>
        /// a table for requests in customized data commiunication
        /// </summary>
        public Dictionary<string, bool> RequestTable
        {
            get { return _requestTable; }
            set { _requestTable = value; }
        }

        public List<Robots> RobotList { get; set; }
        /// <summary>
        /// Constructing all parameters
        /// </summary>
        public VisToAiDataWrapper()
        {
            _isSimulating = false;
            _refreeCommand = "H";
            RobotList = new List<Robots>();
            _customVariables = new Dictionary<string, double>();
            _requestTable = new Dictionary<string, bool>();
        }
    }
}
