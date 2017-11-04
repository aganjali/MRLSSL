using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions.Visualizer_Classes;

namespace MRL.SSL.GameDefinitions
{
    public class VisualizerToAiWrapper
    {
        public bool GoalieChanged = false;
        public bool WithMatrix { get; set; }
        public int RobotID { get; set; }
        public bool BatteryRequest { get; set; }
        public ModelRecieveMode RecieveMode { get; set; }
        public WirelessSenderDevice SenderDevice { get; set; }
        public List<string> SendData { get; set; }
        public string RefreeCommand { get; set; }
        public int? SelectedBallIndex { get; set; }
        public Position2D SelectedBallLoc { get; set; }
        public Dictionary<string,string> Techniques { get; set; }
        public Dictionary<string,bool> RequestTable { get; set; }
        public MergerAndTrackerSetting MergerTracker { get; set; }
        public Dictionary<int,Engines> Engine { get; set; }
        public WorldModel Model { get; set; }
        public SingleObjectState SelectedToMove { get; set; }
        public int? SelectedID { get; set; }
        public bool OjbMoved { get; set; }
        public List<Robots> RobotList { get; set; }
        public OptimizationMatrix OPMatrix { get; set; }
        public Dictionary<int, bool> SensoreState { get; set; }
        public VisualizerToAiWrapper()
        {
            SensoreState = new Dictionary<int, bool>();
            OPMatrix = new OptimizationMatrix();
            RecieveMode = ModelRecieveMode.SSLVision;
            SenderDevice = WirelessSenderDevice.AI;
            Engine = new Dictionary<int, Engines>();
            MergerTracker = new MergerAndTrackerSetting();
            SendData = new List<string>();
            RefreeCommand = "H";
            SelectedID = null;
            SelectedBallIndex = null;
            BatteryRequest = false;
            Techniques = new Dictionary<string, string>();
            RequestTable = new Dictionary<string, bool>();
            SelectedToMove = new SingleObjectState();
            SelectedBallLoc = new Position2D();
            RobotList = new List<Robots>();
            OPMatrix.Identitymatrix();
        }
    }
}
