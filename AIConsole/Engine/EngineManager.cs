using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.Extentions;
using MRL.SSL.AIConsole.Roles;
using System.IO;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions.General_Settings;
using Enterprise;
using System.Diagnostics;
using MRL.SSL.GameDefinitions.Wireless_Packets;
using System.Drawing;
using MRL.SSL.Planning.GamePlanner;
using MRL.SSL.AIConsole.Merger_and_Tracker;
using MRL.SSL.Planning.MotionPlanner;
using System.Runtime.InteropServices;
using MRL.SSL.Planning.GPUDirect;

namespace MRL.SSL.AIConsole.Engine
{
    public class EngineManager : IDisposable
    {
        #region Attributes

        /// <summary>
        /// robot sensores
        /// </summary>
        Dictionary<int, bool> sensorSatates;
        /// <summary>
        /// lockslim for main thrad
        /// </summary>
        private ReaderWriterLockSlim _dsLock;
        public ReaderWriterLockSlim DsLock
        {
            get { return _dsLock; }
            set { _dsLock = value; }
        }
        /// <summary>
        ///  
        /// </summary>
        MotionModel _motionModel;
        /// <summary>
        /// PortManager to manage serial ports
        /// </summary>
        public static MRL.SSL.CommonClasses.Communication.SerialPortManager PortManager = new MRL.SSL.CommonClasses.Communication.SerialPortManager();
        GlobalMerger globalMerger;
        /// <summary>
        /// 
        /// </summary>
        RobotData rettovisRobotData;
        /// <summary>
        /// 
        /// </summary>
        WorldModel VisualizerModel;
        /// <summary>
        /// 
        /// </summary>
        ModelRecieveMode RecieveMode;
        /// <summary>
        /// 
        /// </summary>
        WirelessSenderDevice senderStation;
        /// <summary>
        /// 
        /// </summary>
        ReaderWriterLockSlim _refereeCommandsLock = new ReaderWriterLockSlim();
        /// <summary>
        /// 
        /// </summary>
        Queue<char> RefereeCommands;
        /// <summary>
        /// 
        /// </summary>
        GameStatus statusForMerged;
        /// <summary>
        /// 
        /// </summary>
        AiToVisDataWrapper visData;
        /// <summary>
        /// 
        /// </summary>
        List<Robots> RobotsEngines;
        /// <summary>
        /// 
        /// </summary>
        MRL.SSL.AIConsole.Roles.HaltRole _haltrole;
        /// <summary>
        /// 
        /// </summary>
        SimulatorParameters _simParameter;
        /// <summary>
        /// 
        /// </summary>
        Dictionary<int, SingleWirelessCommand> rettovis;
        /// <summary>
        /// 
        /// </summary>
        int RobotIdToGetbattery = 0;
        /// <summary>
        /// 
        /// </summary>
        bool sendRequest = false;
        /// <summary>
        /// define controllers to comminucat with simulator , vision and visualizer
        /// </summary>
        CommunicationController _comcont, _sharedVisionConnection, _simulConnection;
        /// <summary>
        /// visualizer requests from ai 
        /// </summary>
        public Dictionary<string, bool> VisRequests { get; set; }
        /// <summary>
        /// an instance of this class 
        /// </summary>
        static EngineManager _manager;
        internal static EngineManager Manager
        {
            get { return EngineManager._manager; }
        }
        /// <summary>
        /// current game events that recieve from refreebox
        /// </summary>
        GameEvents _currentEvents;
        /// <summary>
        /// threads that must be run
        /// </summary>
        Thread _reciveThread, _cmcThread, _SendVisThread;
        /// <summary>
        /// engines that running 
        /// </summary>
        Dictionary<int, GameStrategyEngine> _runningEngines;
        public Dictionary<int, GameStrategyEngine> RunningEngines
        {
            get { return _runningEngines; }
        }
        #endregion
        /// <summary>
        /// initialize engines and other parameters , run send and recieve thread
        /// </summary>
        byte sequenceNum = 0;
        int frame = 0;
        //     SaveModelData SMD = new SaveModelData(6, false);
        bool strategyChanged = false;
        bool strategyApplied = false;
        bool getBalls = true;
        Dictionary<int, Position2D> balls2send = new Dictionary<int, Position2D>();
        HiPerfTimer timer = new HiPerfTimer();
        public EngineManager()
        {
            GPPlanner.Initilize();
            getBalls = true;
            balls2send = new Dictionary<int, Position2D>();
            rettovis = new Dictionary<int, SingleWirelessCommand>();
            _simParameter = new SimulatorParameters();
            _haltrole = new MRL.SSL.AIConsole.Roles.HaltRole();
            RobotsEngines = new List<Robots>();
            visData = new AiToVisDataWrapper();
            statusForMerged = GameStatus.Halt;
            RefereeCommands = new Queue<char>();
            senderStation = WirelessSenderDevice.AI;
            rettovisRobotData = new RobotData();
            // globalMerger = new MRL.SSL.AIConsole.Merger_and_Tracker.GlobalMerger();
            globalMerger = new GlobalMerger();
            _motionModel = new MotionModel();
            _dsLock = new ReaderWriterLockSlim();
            sensorSatates = new Dictionary<int, bool>();
            _runningEngines = new Dictionary<int, GameStrategyEngine>();
            _currentEvents = new GameEvents();
            VisRequests = new Dictionary<string, bool>();
            _manager = this;
            _cmcThread = new Thread(new ThreadStart(EngineManagerRun));
            _reciveThread = new Thread(new ThreadStart(Recieve));
            _SendVisThread = new Thread(new ThreadStart(sendDataToVis));
            InitialCommunication();
            foreach (int key in GameSettings.Default.Engines.Keys)
                _runningEngines.Add(key, new GameStrategyEngine(key));
            _reciveThread.Start();
            _cmcThread.Start();
            _SendVisThread.Start();
            Planner.initialize();
        }
        /// <summary>
        /// Initial udp sockets to comminucat with simulator , vision and visualizer
        /// </summary>
        public void InitialCommunication()
        {

            Logger.WriteStart("Initilize Communication");

            Logger.WriteInfo("simulator port");
            if (_simulConnection != null)
                _simulConnection.Dispose();
            _simulConnection = new CommunicationController("any", AISettings.Default.SimulatorSendPort
                , AISettings.Default.SimulatorName, AISettings.Default.SimulatorRecievePort);

            Logger.WriteInfo("vision port");
            if (_sharedVisionConnection != null)
                _sharedVisionConnection.Dispose();
            _sharedVisionConnection = new CommunicationController(AISettings.Default.SSLVisionIP, AISettings.Default.SSLVisionPort);

            Logger.WriteInfo("visualizer port");
            if (_comcont != null)
                _comcont.Dispose();
            _comcont = new CommunicationController("any", AISettings.Default.x, AISettings.Default.VisName, AISettings.Default.VisPort);

        }
        RobotCommands tmpCmd;
        private WorldModel ModelToSend;
        private WorldModel globalModelToSend;
        AutoResetEvent reciveFinished = new AutoResetEvent(false);
        Dictionary<int, Vector2D> lastVel = new Dictionary<int, Vector2D>();
        Dictionary<int, double> lastOmega = new Dictionary<int, double>();
        /// <summary>
        /// main thread that creat engines, send data to robot and send data to visualizer
        /// </summary>
        /// 


        void EngineManagerRun()
        {

            _currentEvents = new GameEvents();
            GoogleSerializer _gSerializer = new GoogleSerializer();
            WorldModel GlobalModell = new WorldModel();
            Logger.Write(LogType.Start, "EngineManager is Running");
            if (!TuneVariables.Default.Doubles.ContainsKey("GoalieID"))
                TuneVariables.Default.Add("GoalieID", (double)0);

            while (true)
            {

                WorldModel Model = null;

                WorldModel Model4Run0 = new WorldModel();
                WorldModel Model4Run1 = new WorldModel();

                messages_robocup_ssl_wrapper.SSL_WrapperPacket sslPacket = null;
                RobotCommands commands = null;

                #region recieve from simulator or sslvision
                if (RecieveMode == ModelRecieveMode.SSLVision || RecieveMode == ModelRecieveMode.Simulator)
                {
                    MemoryStream sharedVisionStream = null;
                    if (RecieveMode == ModelRecieveMode.Simulator)
                    {

                        sharedVisionStream = _simulConnection.RecieveData();

                        //  Console.WriteLine("t = "+ timer2.Duration * 1000);
                    }
                    else
                    {
                        sharedVisionStream = _sharedVisionConnection.RecieveVisionData();
                    }



                    if (sharedVisionStream != null)
                    {

                        sslPacket = _gSerializer.DeserializeSSLVisionPacket(sharedVisionStream);

                        if (sslPacket != null && sslPacket.detection != null && sslPacket.detection.robots_yellow.Count > 7)
                        {

                        }
                        Model = globalMerger.GenerateWorldModel4Cam(sslPacket, tmpCmd, GameSettings.Default.Engines[0].ReverseColor, GameSettings.Default.Engines[0].ReverseSide, TrackerType.Accurate, /*(RecieveMode == ModelRecieveMode.Simulator) ? TrackerType.Fast :*/ TrackerType.Accurate, true, statusForMerged);

                        if (Model != null)
                        {
                            //timer.Stop();
                            //Console.WriteLine("time: " + timer.Duration* 1000);
                            //timer.Start();
                        }
                        if (Model != null)
                            Model.BallStateSlow = new SingleObjectState();
                        if (Model != null)
                        {

                            #region calc fast ball
                            WorldModel tmpModel = new WorldModel();
                            _motionModel.FillDerivatives(globalMerger.Frame, globalMerger.selectedBall_Index, Model, out tmpModel);
                            if (tmpModel.BallStateFast != null)
                                Model.BallStateFast = tmpModel.BallStateFast;
                            if (RecieveMode == ModelRecieveMode.Simulator)
                                Model.BallStateFast = Model.BallState;
                            #endregion
                            foreach (var item in Model.OurRobots.Keys)
                            {
                                if (!lastVel.ContainsKey(item))
                                    lastVel[item] = Vector2D.Zero;
                                if (!lastOmega.ContainsKey(item))
                                    lastOmega[item] = 0;
                            }
                            Model.lastVelocity = lastVel.ToDictionary(p => p.Key, p => p.Value);
                            Model.lastW = lastOmega.ToDictionary(p => p.Key, p => p.Value);

                            Model.GoalieID = goaliID;
                            if (Model.BallStateSlow == null)
                                Model.BallStateSlow = new SingleObjectState();
                            GlobalModell = new WorldModel(Model);
                            Model.FieldIsInverted = GameSettings.Default.Engines[0].ReverseSide;
                            Model.OurScore = (Model.OurMarkerISYellow) ? _currentEvents.YellowScore : _currentEvents.BlueScore;
                            Model.OpponentScore = (Model.OurMarkerISYellow) ? _currentEvents.BlueScore : _currentEvents.YellowScore;
                            if (!StaticVariables.OldRefbox)
                            {
                                Model.GoalieID = (Model.OurMarkerISYellow) ? _currentEvents.YellowGoalie : _currentEvents.BlueGoalie;
                            }
                            int m = (int)(_currentEvents.TimeOfstage / 60);
                            int s = (int)((_currentEvents.TimeOfstage - m * 60));
                            Model.TimeLeft = new TimeSpan(0, m, s);
                        }
                    }
                }
                #endregion

                #region recieve from visualizer
                else if (RecieveMode == ModelRecieveMode.Analizer || RecieveMode == ModelRecieveMode.Visualizer)
                {
                    Model = VisualizerModel;
                    if (Model != null)
                        GlobalModell = new WorldModel(Model);
                }
                #endregion

                #region Model not null and continue
                if (Model != null)
                {

                    try
                    {

                        //sequenceNum++;

                        if (Model.BallState == null || double.IsInfinity(Model.BallState.Location.X) || double.IsNaN(Model.BallState.Location.X))
                            Model.BallState.Location = new MRL.SSL.CommonClasses.MathLibrary.Position2D();
                        if (Model.BallStateSlow == null)
                            Model.BallStateSlow = new SingleObjectState();
                        _dsLock.EnterUpgradeableReadLock();
                        ///
                        /// Calculate Velocities and accelerations
                        ///
                        try
                        {
                            UpdateRunningEngines();

                            #region Calculate refree command for each engine

                            _refereeCommandsLock.EnterUpgradeableReadLock();
                            while (RefereeCommands.Count > 0)
                            {
                                _refereeCommandsLock.EnterWriteLock();
                                char ch = RefereeCommands.Dequeue();
                                foreach (int key in GameSettings.Default.Engines.Keys)
                                {
                                    _runningEngines[key].Status = GameStatusCalculator.CalculateGameStatus(_runningEngines[key].Status, ch, Model.OurMarkerISYellow);
                                    //TODO: ballPlacement add to model
                                    statusForMerged = _runningEngines[key].Status;
                                }
                                _refereeCommandsLock.ExitWriteLock();
                            }
                            _refereeCommandsLock.ExitUpgradeableReadLock();
                            #endregion
                            if (RecieveMode != ModelRecieveMode.Visualizer)
                            {
                                #region Set Engine params from Settings
                                WorldModel globalModel = Model;
                                if (_runningEngines.Count == 1)
                                {
                                    Engines rEngine0 = new Engines(0, GameSettings.Default.Engines[0].ReverseColor, GameSettings.Default.Engines[0].ReverseSide);
                                    WorldModel localModel0 = new WorldModel();
                                    double reverseSide0 = (rEngine0.ReverseSide) ? -1 : 1;
                                    float reverseAngle0 = (rEngine0.ReverseSide) ? 180 : 0;
                                    int colorargb0 = rEngine0.GetColor.ToArgb();
                                    localModel0.FieldIsInverted = rEngine0.ReverseSide;
                                    #region Engine 0
                                    localModel0.lastVelocity = globalModel.lastVelocity.ToDictionary(p => p.Key, p => p.Value);
                                    localModel0.lastW = globalModel.lastW.ToDictionary(p => p.Key, p => p.Value);
                                    //localModel = globalMerger.StartAnalyse(sslPacket, GameSettings.Default.Engines[key].ReverseColor, GameSettings.Default.Engines[key].ReverseSide, statusForMerged);
                                    localModel0.CurrentVisionPacket0 = globalModel.CurrentVisionPacket0;
                                    localModel0.CurrentVisionPacket1 = globalModel.CurrentVisionPacket1;
                                    localModel0.CurrentVisionPacket2 = globalModel.CurrentVisionPacket2;
                                    localModel0.CurrentVisionPacket3 = globalModel.CurrentVisionPacket3;
                                    localModel0.CurrentVisionPacket4 = globalModel.CurrentVisionPacket4;
                                    localModel0.CurrentVisionPacket5 = globalModel.CurrentVisionPacket5;
                                    localModel0.CurrentVisionPacket6 = globalModel.CurrentVisionPacket6;
                                    localModel0.CurrentVisionPacket7 = globalModel.CurrentVisionPacket7;
                                    localModel0.SslVisionGeometry = globalModel.SslVisionGeometry;

                                    localModel0.SequenceNumber = sequenceNum;

                                    localModel0.OurRobots = new Dictionary<int, SingleObjectState>();
                                    localModel0.Opponents = new Dictionary<int, SingleObjectState>();
                                    if (RobotsEngines.Count > 0)
                                    {
                                        Color ourColor = (globalModel.OurMarkerISYellow) ? Color.Yellow : Color.Blue;
                                        foreach (Robots robo in RobotsEngines.ToList().Where(w => w.EngineId == 0))
                                        {
                                            SingleObjectState ourstate = null, oppstate = null;
                                            if (robo.TeamColor.ToArgb() == ourColor.ToArgb() && globalModel.OurRobots.ContainsKey(robo.Id))
                                                ourstate = globalModel.OurRobots[robo.Id];
                                            else if (robo.TeamColor.ToArgb() != ourColor.ToArgb() && globalModel.Opponents.ContainsKey(robo.Id))
                                                oppstate = globalModel.Opponents[robo.Id];
                                            if (ourstate != null && !localModel0.OurRobots.ContainsKey(robo.Id))
                                                localModel0.OurRobots.Add(robo.Id, new SingleObjectState(ObjectType.OurRobot, reverseSide0 * ourstate.Location, reverseSide0 * ourstate.Speed, reverseSide0 * ourstate.Acceleration, (ourstate.Angle.HasValue) ? LimitAngle(ourstate.Angle.Value + reverseAngle0) : (float?)null, ourstate.AngularSpeed));
                                            if (oppstate != null && !localModel0.Opponents.ContainsKey(robo.Id))
                                                localModel0.Opponents.Add(robo.Id, new SingleObjectState(ObjectType.Opponent, reverseSide0 * oppstate.Location, reverseSide0 * oppstate.Speed, reverseSide0 * oppstate.Acceleration, (oppstate.Angle.HasValue) ? LimitAngle(oppstate.Angle.Value + reverseAngle0) : (float?)null, oppstate.AngularSpeed));
                                        }

                                    }
                                    else
                                    {

                                        foreach (int k in globalModel.OurRobots.Keys)
                                            if (!localModel0.OurRobots.ContainsKey(k))
                                            {
                                                SingleObjectState state = globalModel.OurRobots[k];
                                                //localModel.OurRobots.Add((k == 0) ? 20 : k, new SingleObjectState(ObjectType.OurRobot, reverseSide * state.Location, reverseSide * state.Speed, reverseSide * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle) : (float?)null, state.AngularSpeed));
                                                localModel0.OurRobots.Add((k == 0) ? 0 : k, new SingleObjectState(ObjectType.OurRobot, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed));
                                            }
                                        foreach (int oid in globalModel.Opponents.Keys)
                                        {
                                            SingleObjectState state = globalModel.Opponents[oid];
                                            localModel0.Opponents.Add(oid, new SingleObjectState(ObjectType.Opponent, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed));
                                        }
                                    }

                                    if (globalModel.BallState != null)
                                    {
                                        SingleObjectState state = globalModel.BallState;
                                        localModel0.BallState = new SingleObjectState(ObjectType.Ball, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed);
                                    }
                                    if (globalModel.BallStateSlow != null)
                                    {
                                        SingleObjectState state = globalModel.BallStateSlow;
                                        localModel0.BallStateSlow = new SingleObjectState(ObjectType.Ball, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed);
                                    }
                                    if (globalModel.BallStateFast != null)
                                    {
                                        SingleObjectState state = globalModel.BallStateFast;
                                        localModel0.BallStateFast = new SingleObjectState(ObjectType.Ball, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed);
                                    }
                                    localModel0.BallHeight = globalModel.BallHeight;
                                    localModel0.FieldIsInverted = rEngine0.ReverseSide;
                                    localModel0.GlobalKickingProhibited = globalModel.GlobalKickingProhibited;
                                    localModel0.GoalieID = globalModel.GoalieID;
                                    localModel0.OpponentScore = (rEngine0.ReverseSide) ? globalModel.OurScore : globalModel.OpponentScore;
                                    localModel0.OurMarkerISYellow = globalModel.OurMarkerISYellow;//^ rEngine.ReverseColor;
                                    localModel0.OurScore = (rEngine0.ReverseSide) ? globalModel.OpponentScore : globalModel.OurScore;
                                    localModel0.SequenceNumber = globalModel.SequenceNumber;
                                    localModel0.TimeElapsed = globalModel.TimeElapsed;
                                    localModel0.TimeOfAction = globalModel.TimeOfAction;
                                    localModel0.BallConfidenc = globalModel.BallConfidenc;
                                    localModel0.PredictedBall = new PredictedStates();
                                    localModel0.PredictedBall.states = globalModel.PredictedBall.states.ToList();
                                    foreach (var item in RecievedData.LatestData)
                                    {
                                        if (localModel0.OurRobots.ContainsKey(item.Key))
                                        {
                                            localModel0.OurRobots[item.Key].SequenceNumber = item.Value.SequenceNumber;
                                            localModel0.OurRobots[item.Key].Sensore = item.Value.Sensore;
                                            localModel0.OurRobots[item.Key].BatteryLife = item.Value.BatteryLife;
                                        }
                                    }

                                    //RunningEngines[rEngine0.Id].GameInfo = GPEngine.CalculateInfo(localModel0);
                                    //  ht2 = new HiPerfTimer();
                                    //  ht2.Start();
                                    _runningEngines[rEngine0.Id].PlayGame(localModel0, strategyChanged, false);
                                    //   ht2.Stop();

                                    if (strategyChanged)
                                        strategyChanged = false;

                                    #endregion
                                    Model = localModel0;
                                    Model4Run0 = localModel0;
                                }
                                else if (_runningEngines.Count > 1)
                                {
                                    Engines rEngine1 = new Engines(1, GameSettings.Default.Engines[1].ReverseColor, GameSettings.Default.Engines[1].ReverseSide);
                                    WorldModel localModel1 = new WorldModel();
                                    double reverseSide1 = (rEngine1.ReverseSide) ? -1 : 1;
                                    float reverseAngle1 = (rEngine1.ReverseSide) ? 180 : 0;
                                    int colorargb1 = rEngine1.GetColor.ToArgb();
                                    localModel1.FieldIsInverted = rEngine1.ReverseSide;
                                    Engines rEngine0 = new Engines(0, GameSettings.Default.Engines[0].ReverseColor, GameSettings.Default.Engines[0].ReverseSide);
                                    WorldModel localModel0 = new WorldModel();
                                    double reverseSide0 = (rEngine0.ReverseSide) ? -1 : 1;
                                    float reverseAngle0 = (rEngine0.ReverseSide) ? 180 : 0;
                                    int colorargb0 = rEngine0.GetColor.ToArgb();
                                    localModel0.FieldIsInverted = rEngine0.ReverseSide;
                                    #region Engine 0

                                    //localModel = globalMerger.StartAnalyse(sslPacket, GameSettings.Default.Engines[key].ReverseColor, GameSettings.Default.Engines[key].ReverseSide, statusForMerged);
                                    localModel0.CurrentVisionPacket0 = globalModel.CurrentVisionPacket0;
                                    localModel0.CurrentVisionPacket1 = globalModel.CurrentVisionPacket1;
                                    localModel0.SequenceNumber = sequenceNum;
                                    localModel0.lastVelocity = globalModel.lastVelocity.ToDictionary(p => p.Key, p => p.Value);
                                    localModel0.lastW = globalModel.lastW.ToDictionary(p => p.Key, p => p.Value);

                                    localModel0.OurRobots = new Dictionary<int, SingleObjectState>();
                                    localModel0.Opponents = new Dictionary<int, SingleObjectState>();
                                    if (RobotsEngines.Count > 0)
                                    {

                                        foreach (Robots robo in RobotsEngines.Where(w => w.EngineId == 0).ToList())
                                        {
                                            SingleObjectState state = null;
                                            if (globalModel.OurRobots.ContainsKey(robo.Id))
                                                state = globalModel.OurRobots[robo.Id];
                                            else if (globalModel.Opponents.ContainsKey(robo.Id))
                                                state = globalModel.Opponents[robo.Id];
                                            if (!localModel0.OurRobots.ContainsKey(robo.Id) && state != null)
                                                localModel0.OurRobots.Add(robo.Id, new SingleObjectState(ObjectType.OurRobot, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed));
                                        }
                                        foreach (Robots robo in RobotsEngines.Where(w => w.EngineId == 1).ToList())
                                        {
                                            SingleObjectState state = null;
                                            if (globalModel.OurRobots.ContainsKey(robo.Id))
                                                state = globalModel.OurRobots[robo.Id];
                                            else if (globalModel.Opponents.ContainsKey(robo.Id))
                                                state = globalModel.Opponents[robo.Id];
                                            if (!localModel0.Opponents.ContainsKey(robo.Id) && state != null)
                                                localModel0.Opponents.Add(robo.Id, new SingleObjectState(ObjectType.Opponent, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed));
                                        }

                                    }
                                    else
                                    {

                                        foreach (int k in globalModel.OurRobots.Keys)
                                            if (!localModel0.OurRobots.ContainsKey(k))
                                            {
                                                SingleObjectState state = globalModel.OurRobots[k];
                                                //localModel.OurRobots.Add((k == 0) ? 20 : k, new SingleObjectState(ObjectType.OurRobot, reverseSide * state.Location, reverseSide * state.Speed, reverseSide * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle) : (float?)null, state.AngularSpeed));
                                                localModel0.OurRobots.Add((k == 0) ? 0 : k, new SingleObjectState(ObjectType.OurRobot, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed));
                                            }
                                        foreach (int oid in globalModel.Opponents.Keys)
                                        {
                                            SingleObjectState state = globalModel.Opponents[oid];
                                            localModel0.Opponents.Add(oid, new SingleObjectState(ObjectType.Opponent, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed));
                                        }
                                    }

                                    if (globalModel.BallState != null)
                                    {
                                        SingleObjectState state = globalModel.BallState;
                                        localModel0.BallState = new SingleObjectState(ObjectType.Ball, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed);
                                    }
                                    if (globalModel.BallStateSlow != null)
                                    {
                                        SingleObjectState state = globalModel.BallStateSlow;
                                        localModel0.BallStateSlow = new SingleObjectState(ObjectType.Ball, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed);
                                    }
                                    if (globalModel.BallStateFast != null)
                                    {
                                        SingleObjectState state = globalModel.BallStateFast;
                                        localModel0.BallStateFast = new SingleObjectState(ObjectType.Ball, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed);
                                    }
                                    localModel0.BallHeight = globalModel.BallHeight;
                                    localModel0.FieldIsInverted = rEngine0.ReverseSide;
                                    localModel0.GlobalKickingProhibited = globalModel.GlobalKickingProhibited;
                                    localModel0.GoalieID = globalModel.GoalieID;
                                    localModel0.OpponentScore = (rEngine0.ReverseSide) ? globalModel.OurScore : globalModel.OpponentScore;
                                    localModel0.OurMarkerISYellow = globalModel.OurMarkerISYellow;//^ rEngine.ReverseColor;
                                    localModel0.OurScore = (rEngine0.ReverseSide) ? globalModel.OpponentScore : globalModel.OurScore;
                                    localModel0.SequenceNumber = globalModel.SequenceNumber;
                                    localModel0.TimeElapsed = globalModel.TimeElapsed;
                                    localModel0.TimeOfAction = globalModel.TimeOfAction;
                                    localModel0.BallConfidenc = globalModel.BallConfidenc;
                                    localModel0.PredictedBall.states = globalModel.PredictedBall.states.ToList();
                                    foreach (var item in RecievedData.LatestData)
                                    {
                                        if (localModel0.OurRobots.ContainsKey(item.Key))
                                        {
                                            localModel0.OurRobots[item.Key].SequenceNumber = item.Value.SequenceNumber;
                                            localModel0.OurRobots[item.Key].Sensore = item.Value.Sensore;
                                            localModel0.OurRobots[item.Key].BatteryLife = item.Value.BatteryLife;
                                        }
                                    }



                                    //  RunningEngines[rEngine0.Id].GameInfo = GPEngine.CalculateInfo(localModel0);

                                    _runningEngines[rEngine0.Id].PlayGame(localModel0, strategyChanged, false);
                                    if (strategyChanged)
                                        strategyChanged = false;
                                    Model4Run0 = localModel0;
                                    #endregion
                                    #region Engine 1

                                    localModel1.CurrentVisionPacket0 = globalModel.CurrentVisionPacket0;
                                    localModel1.CurrentVisionPacket1 = globalModel.CurrentVisionPacket1;
                                    localModel1.SequenceNumber = sequenceNum;
                                    localModel1.lastVelocity = globalModel.lastVelocity.ToDictionary(p => p.Key, p => p.Value);
                                    localModel1.lastW = globalModel.lastW.ToDictionary(p => p.Key, p => p.Value);

                                    localModel1.OurRobots = new Dictionary<int, SingleObjectState>();
                                    localModel1.Opponents = new Dictionary<int, SingleObjectState>();
                                    if (RobotsEngines.Count > 0)
                                    {

                                        foreach (Robots robo in RobotsEngines.ToList().Where(w => w.EngineId == 1))
                                        {
                                            SingleObjectState state = null;
                                            if (globalModel.OurRobots.ContainsKey(robo.Id))
                                                state = globalModel.OurRobots[robo.Id];
                                            else if (globalModel.Opponents.ContainsKey(robo.Id))
                                                state = globalModel.Opponents[robo.Id];
                                            if (!localModel1.OurRobots.ContainsKey(robo.Id) && state != null)
                                                localModel1.OurRobots.Add(robo.Id, new SingleObjectState(ObjectType.OurRobot, reverseSide1 * state.Location, reverseSide1 * state.Speed, reverseSide1 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle1) : (float?)null, state.AngularSpeed));
                                        }
                                        foreach (Robots robo in RobotsEngines.ToList().Where(w => w.EngineId == 0))
                                        {
                                            SingleObjectState state = null;
                                            if (globalModel.OurRobots.ContainsKey(robo.Id))
                                                state = globalModel.OurRobots[robo.Id];
                                            else if (globalModel.Opponents.ContainsKey(robo.Id))
                                                state = globalModel.Opponents[robo.Id];
                                            if (!localModel1.Opponents.ContainsKey(robo.Id) && state != null)
                                                localModel1.Opponents.Add(robo.Id, new SingleObjectState(ObjectType.Opponent, reverseSide1 * state.Location, reverseSide1 * state.Speed, reverseSide1 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle1) : (float?)null, state.AngularSpeed));
                                        }

                                    }
                                    else
                                    {

                                        foreach (int k in globalModel.OurRobots.Keys)
                                            if (!localModel1.OurRobots.ContainsKey(k))
                                            {
                                                SingleObjectState state = globalModel.OurRobots[k];
                                                //localModel.OurRobots.Add((k == 0) ? 20 : k, new SingleObjectState(ObjectType.OurRobot, reverseSide * state.Location, reverseSide * state.Speed, reverseSide * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle) : (float?)null, state.AngularSpeed));
                                                localModel1.OurRobots.Add((k == 0) ? 0 : k, new SingleObjectState(ObjectType.OurRobot, reverseSide1 * state.Location, reverseSide1 * state.Speed, reverseSide1 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle1) : (float?)null, state.AngularSpeed));
                                            }
                                        foreach (int oid in globalModel.Opponents.Keys)
                                        {
                                            SingleObjectState state = globalModel.Opponents[oid];
                                            localModel1.Opponents.Add(oid, new SingleObjectState(ObjectType.Opponent, reverseSide1 * state.Location, reverseSide1 * state.Speed, reverseSide1 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle1) : (float?)null, state.AngularSpeed));
                                        }
                                    }

                                    if (globalModel.BallState != null)
                                    {
                                        SingleObjectState state = globalModel.BallState;
                                        localModel1.BallState = new SingleObjectState(ObjectType.Ball, reverseSide1 * state.Location, reverseSide1 * state.Speed, reverseSide1 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle1) : (float?)null, state.AngularSpeed);
                                    }
                                    if (globalModel.BallStateSlow != null)
                                    {
                                        SingleObjectState state = globalModel.BallStateSlow;
                                        localModel1.BallStateSlow = new SingleObjectState(ObjectType.Ball, reverseSide1 * state.Location, reverseSide1 * state.Speed, reverseSide1 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle1) : (float?)null, state.AngularSpeed);
                                    }
                                    if (globalModel.BallStateFast != null)
                                    {
                                        SingleObjectState state = globalModel.BallStateFast;
                                        localModel1.BallStateFast = new SingleObjectState(ObjectType.Ball, reverseSide0 * state.Location, reverseSide0 * state.Speed, reverseSide0 * state.Acceleration, (state.Angle.HasValue) ? LimitAngle(state.Angle.Value + reverseAngle0) : (float?)null, state.AngularSpeed);
                                    }
                                    localModel1.BallHeight = globalModel.BallHeight;
                                    localModel1.FieldIsInverted = rEngine1.ReverseSide;
                                    localModel1.GlobalKickingProhibited = globalModel.GlobalKickingProhibited;
                                    localModel1.GoalieID = globalModel.GoalieID;
                                    localModel1.OpponentScore = (rEngine1.ReverseSide) ? globalModel.OurScore : globalModel.OpponentScore;
                                    localModel1.OurMarkerISYellow = globalModel.OurMarkerISYellow;//^ rEngine.ReverseColor;
                                    localModel1.OurScore = (rEngine1.ReverseSide) ? globalModel.OpponentScore : globalModel.OurScore;
                                    localModel1.SequenceNumber = globalModel.SequenceNumber;
                                    localModel1.TimeElapsed = globalModel.TimeElapsed;
                                    localModel1.TimeOfAction = globalModel.TimeOfAction;
                                    localModel1.BallConfidenc = globalModel.BallConfidenc;
                                    localModel1.PredictedBall.states = globalModel.PredictedBall.states.ToList();
                                    foreach (var item in RecievedData.LatestData)
                                    {
                                        if (localModel1.OurRobots.ContainsKey(item.Key))
                                        {
                                            localModel1.OurRobots[item.Key].SequenceNumber = item.Value.SequenceNumber;
                                            localModel1.OurRobots[item.Key].Sensore = item.Value.Sensore;
                                            localModel1.OurRobots[item.Key].BatteryLife = item.Value.BatteryLife;
                                        }
                                    }


                                    _runningEngines[rEngine1.Id].PlayGame(localModel1, strategyChanged, false);
                                    if (strategyChanged)
                                        strategyChanged = false;
                                    Model4Run1 = localModel1;

                                    #endregion
                                    Model = localModel0;
                                }


                                #endregion
                            }
                            else
                            {
                                _runningEngines[0].PlayGame(Model, strategyChanged, true);
                            }
                            #region create and send command to robots our simulator port


                            if (RecieveMode == ModelRecieveMode.SSLVision)
                            {

                                if (senderStation == WirelessSenderDevice.AI)
                                {

                                    if (RunningEngines.Count != 2)
                                        commands = Planner.Run(Model4Run0, out lastVel, out lastOmega);
                                    else
                                    {
                                        List<WorldModel> models = new List<WorldModel>();
                                        models.Add(Model4Run0);
                                        models.Add(Model4Run1);

                                        commands = Planner.Run(models, out lastVel, out lastOmega);
                                    }


                                    if (Model.Status == GameStatus.Penalty_OurTeam_Go)
                                        commands.IsPenaltyMode = true;
                                    else
                                        commands.IsPenaltyMode = false;

                                    if (!commands.IsPenaltyMode && commands.Commands != null)
                                    {
                                        foreach (var item in commands.Commands)
                                            if (item.Value != null)
                                                item.Value.statusRequest = sendRequest;
                                    }


                                    #region SEND
                                    //Send Data to Robots
                                    //  commands.Commands = new Dictionary<int, SingleWirelessCommand>();
                                    //if ((Model4Run0.OurRobots.ContainsKey(0) && !commands.Commands.ContainsKey(0)) || (Model4Run0.OurRobots.ContainsKey(1) && !commands.Commands.ContainsKey(1)))
                                    //    Logger.Write(LogType.Info, "0 : " + commands.Commands.ContainsKey(0) + "\t1: " + commands.Commands.ContainsKey(1));


                                    // PortManager.SendData(AISettings.Default.SerialPort, commands.CreatPacket(sequenceNum), false);
                                    PortManager.SendData(AISettings.Default.SerialPort, commands.CreatPacket(frame), false);
                                    frame++;
                                    if (frame == 60)
                                    {
                                        frame = 0;
                                    }
                                    foreach (var item in commands.Commands.Keys.ToList())
                                    {
                                        DrawingObjects.AddObject(new StringDraw("kick: " + commands.Commands[item].KickPower, Model.OurRobots[item].Location + new Vector2D(-0.3, -0.3)), "kickPower" + item);
                                    }
                                    #endregion
                                    RobotComponentsController.PreviousCommands = commands.Commands;
                                    sequenceNum += 3;
                                    if (sequenceNum > 13)
                                        sequenceNum = 1;
                                }
                                else
                                {
                                    //PortManager.Enabled = false;
                                }
                            }
                            else if (RecieveMode == ModelRecieveMode.Simulator)
                            {
                                if (RunningEngines.Count != 2)
                                    commands = Planner.Run(Model4Run0, out lastVel, out lastOmega);
                                else
                                {
                                    List<WorldModel> models = new List<WorldModel>();
                                    models.Add(Model4Run0);
                                    models.Add(Model4Run1);

                                    commands = Planner.Run(models, out lastVel, out lastOmega);
                                }
                                if (_runningEngines != null && _runningEngines.Count > 0)
                                {
                                    if (_runningEngines.Count == 1)
                                    {
                                        if (_runningEngines[0].LastRunningPlay != null)
                                        {
                                            foreach (var item in commands.Commands)
                                                commands.Commands[item.Key].Color = (Model.OurMarkerISYellow) ? Color.Yellow : Color.Blue;
                                        }
                                    }
                                    else if (_runningEngines.Count > 1)
                                    {
                                        foreach (var item in commands.Commands)
                                        {
                                            if (RobotsEngines.Any(a => a.Id == item.Key))
                                            {
                                                var robo = RobotsEngines.Single(s => s.Id == item.Key);
                                                commands.Commands[item.Key].Color = robo.TeamColor;
                                            }
                                            else
                                                commands.Commands[item.Key].Color = (Model.OurMarkerISYellow) ? Color.Yellow : Color.Blue;
                                        }
                                    }
                                    _simParameter.Commands = commands.Commands;
                                    _simulConnection.SendData(_gSerializer.SerializeSimulatorParameters(_simParameter));
                                }
                                else
                                    _simulConnection.SendData(null);
                            }
                            else if (RecieveMode == ModelRecieveMode.Visualizer)
                            {
                                commands = Planner.Run(Model, out lastVel, out lastOmega);
                            }
                            #endregion
                            if (commands != null)
                                tmpCmd = (RobotCommands)commands.Clone();
                            else
                                tmpCmd = null;
                            _dsLock.ExitUpgradeableReadLock();
                        }
                        finally { }

                    }
                    catch (Exception ex)
                    {
                        _dsLock.ExitUpgradeableReadLock();
                        Logger.Write(LogType.Exception, ex.ToString());

                    }
                }

                #endregion
                #region Send Data to Visualizer


                if (Model != null)
                {

                    ModelToSend = Model;
                    DrawingObjects.AddObject(new StringDraw("Ball Confidence : " + ModelToSend.BallConfidenc.ToString("0.###"), "Ball Confidence", System.Drawing.Color.Black, ModelToSend.BallState.Location + new Vector2D(-0.1, 0))
                    {
                        IsShown = true
                    }, "Ball Confidence");
                    globalModelToSend = GlobalModell;
                    if (commands == null)
                        commands = new RobotCommands();
                    if (commands.Commands == null)
                        commands.Commands = new Dictionary<int, SingleWirelessCommand>();
                    rettovis = commands.Commands;
                    if (getBalls)
                        balls2send = globalMerger.ballsViwed.ToDictionary(k => k.Key, v => v.Value);
                    reciveFinished.Set();


                }
                #endregion

            }
        }
        /// <summary>
        /// reciever from visualizer
        /// </summary>
        void Recieve()
        {
            MemoryStream recieveStream;
            GoogleSerializer _gserilizer = new GoogleSerializer();
            while (true)
            {
                recieveStream = _comcont.RecieveData();
                if (recieveStream != null)
                {
                    VisualizerToAiWrapper vi = _gserilizer.DeserializeVisToAiWrapper(recieveStream);
                    VisRequests = vi.RequestTable.ToDictionary(k => k.Key, v => v.Value);
                    if (vi.GoalieChanged)
                        goaliID = ControlParameters.GoalieID;

                    foreach (var item in vi.SendData)
                    {
                        if (item == "BatteryRequest")
                        {
                            sendRequest = vi.BatteryRequest;
                            RobotIdToGetbattery = vi.RobotID;
                            Logger.WriteInfo("BatteryRequest from vis");
                        }
                        if (item == "RefreeCommand")
                        {
                            EnqueueCommand(vi.RefreeCommand.ToCharArray()[0]);
                            Logger.WriteInfo("RefreeCommand recieve from vis");
                        }
                        if (item == "BallIndex")
                        {
                            globalMerger.setBallIndex(vi.SelectedBallIndex, vi.SelectedBallLoc);
                            Logger.WriteInfo("BallIndex recieve from vis");
                        }
                        if (item == "MergerTracker")
                        {
                            globalMerger.SetParameters(vi.MergerTracker);
                            Logger.WriteInfo("MergerTracker recieve from vis");
                        }
                        if (item == "Techniques")
                        {
                            GameSettings.Default.Technique = new SerializableDictionary<string, string>();
                            vi.Techniques.ToList().ForEach(p => GameSettings.Default.Technique.Add(p.Key, p.Value));
                            GameSettings.Default.Save();
                            Logger.WriteInfo("Techniques recieve from vis");
                        }
                        if (item == "RecieveMode")
                        {
                            RecieveMode = vi.RecieveMode;
                            Logger.WriteInfo("Recieve mode changed to {0} by vis", RecieveMode.ToString());
                        }
                        if (item == "Engines")
                        {
                            if (vi.Engine != null)
                                CreatEngines(vi.Engine);
                        }
                        if (item == "SenderDevice")
                        {
                            senderStation = vi.SenderDevice;
                            Logger.WriteInfo("SenderDevice changed to {0} by vis", senderStation.ToString());
                        }
                        if (item == "Bool")
                            Program.withMatrix = vi.WithMatrix;
                        if (item == "Robots")
                        {
                            RobotsEngines = vi.RobotList;
                            Logger.WriteInfo("robots recieve from vis");
                        }
                        if (item == "Tactic")
                        {

                        }
                        if (item == "Strategy")
                        {
                            strategyChanged = true;
                            StrategyInfo.Save("Aistrategy");
                        }
                        if (item == "Sensors")
                        {
                            sensorSatates = vi.SensoreState;
                            Logger.WriteInfo("sensors recieve from vis");
                        }
                    }
                    VisualizerModel = vi.Model;
                    try
                    {
                        GameSettings.Default.Save();
                        TuneVariables.Default.Save();
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(LogType.Exception, ex.ToString());
                    }
                }
            }
        }
        /// <summary>
        /// Standardization angle between 0 and 360
        /// </summary>
        /// <param name="Angle">angle</param>
        /// <returns>standared angle</returns>
        float LimitAngle(float Angle)
        {
            if (Angle > 360)
                return Angle - 360;
            else if (Angle < 0)
                return Angle + 360;
            else return Angle;
        }
        /// <summary>
        /// add command to queue
        /// </summary>
        /// <param name="CommandChar"></param>
        public void EnqueueCommand(char CommandChar)
        {
            _refereeCommandsLock.EnterWriteLock();
            RefereeCommands.Enqueue(CommandChar);
            _refereeCommandsLock.ExitWriteLock();
        }
        /// <summary>
        /// fill game events that recieved from refreebox
        /// </summary>
        /// <param name="val">game event</param>
        public void FillEvents(GameEvents val)
        {
            _currentEvents = val;
        }
        /// <summary>
        /// dispos all thread , sockets and serial ports
        /// </summary>
        public void Dispose()
        {
            Planner.ShutDown();
            GPPlanner.ShutDown();
            if (_reciveThread != null)
                _reciveThread.Abort();
            if (_cmcThread != null)
                _cmcThread.Abort();
            if (_comcont != null)
                _comcont.Dispose();
            if (_SendVisThread != null)
                _SendVisThread.Abort();
            if (_simulConnection != null)
                _simulConnection.Dispose();
            if (_sharedVisionConnection != null)
                _sharedVisionConnection.Dispose();
        }
        /// <summary>
        /// update runnig engines from games settings that fill by visualizer
        /// </summary>
        void UpdateRunningEngines()
        {
            var mustAdd = GameSettings.Default.Engines.Where(w => !_runningEngines.ContainsKey(w.Key));
            var mustDel = _runningEngines.Where(w => !GameSettings.Default.Engines.ContainsKey(w.Key));
            mustAdd.ToList().ForEach(p => _runningEngines.Add(p.Key, new GameStrategyEngine(p.Key)));
            mustDel.ToList().ForEach(p => _runningEngines.Remove(p.Key));
        }
        /// <summary>
        /// convert engines by sended from visualizer to setting type 
        /// </summary>
        /// <param name="source">the engines that send by visualizer</param>
        public void CreatEngines(Dictionary<int, Engines> source)
        {
            if (GameSettings.Default.Engines == null)
                GameSettings.Default.Engines = new SerializableDictionary<int, Engines>();
            foreach (int item in source.Keys)
            {
                GameSettings.Default.Engines[item] = new Engines(item, source[item].ReverseColor, source[item].ReverseSide);
            }
        }
        private void sendDataToVis()
        {
            GoogleSerializer _gSerializer = new GoogleSerializer();
            while (true)
            {
                reciveFinished.WaitOne();
                #region Send Data to Visualizer
                if (ModelToSend != null)
                {
                    WorldModel model = new WorldModel(ModelToSend);
                    CharterData.AddData("Speed", (double)(model.BallState.Speed.Size));
                    //     CharterData.AddData("SpeedSlow", (double)(model.BallStateSlow.Speed.Size));
                    visData.Model = model;//new WorldModel(model);
                    getBalls = false;
                    visData.Balls = balls2send;//globalMerger.ballsViwed.ToDictionary(k => k.Key, v => v.Value);

                    AiToVisualizerWrapper aitovis = new AiToVisualizerWrapper();
                    aitovis.AllBalls = balls2send;//.ToDictionary(k => k.Key, v => v.Value);
                    aitovis.Model = model;
                    aitovis.GlobalModel = new WorldModel(globalModelToSend);
                    aitovis.RobotCommnd = rettovis;
                    aitovis.MtrixData = rettovisRobotData;
                    if (VisRequests.ContainsKey("Techniques"))
                    {
                        Logger.Write(LogType.Debug, "Techniques Sended to vis");
                        aitovis.Techniques = GameSettings.Default.Technique;
                    }
                    if (VisRequests.ContainsKey("Engines"))
                    {
                        Logger.Write(LogType.Debug, "Engines Sended to vis");
                        aitovis.Engines = GameSettings.Default.Engines;
                    }
                    if (VisRequests.ContainsKey("GameParameters"))
                    {
                        Logger.Write(LogType.Debug, "Game Parameters Sended to vis");
                        aitovis.GameParametersSend = true;
                    }
                    if (VisRequests.ContainsKey("Customvariables"))
                    {
                        Logger.Write(LogType.Debug, "Custom Variable Sended to vis");
                        aitovis.SendTuneVariables = true;

                        TuneVariables.Default.Save();
                    }
                    if (VisRequests.ContainsKey("Strategies"))
                    {
                        Logger.Write(LogType.Debug, "Strategies Sended to vis");
                        aitovis.StrategySended = true;
                        //StrategyInfo.Save();
                    }


                    //if ( VisRequests.ContainsKey ( "ActiveSettings" ) )
                    //    aitovis.SendActiveSetting = true;
                    VisRequests.Clear();

                    _gSerializer.SerializeAiToVisWrapper(aitovis);
                    getBalls = true;
                    //lock (_comcont)
                    _comcont.SendData(_gSerializer.stream);
                    //_threeDvis.SendData(_gSerializer.stream);
                    rettovis = new Dictionary<int, SingleWirelessCommand>();
                    rettovisRobotData = new RobotData();

                }
                #endregion
            }
        }


        public int? goaliID = 0;
    }
}