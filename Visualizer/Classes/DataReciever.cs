using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MRL.SSL.GameDefinitions;
using System.IO;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.CommonClasses.Extentions;
using Visualizer.Classes;
using Visualizer;
using MRL.SSL.CommonClasses;
using MRL.SSL.Visualizer.Windows;
using MRL.SSL.GameDefinitions.General_Settings;
using messages_robocup_ssl_wrapper;
using Enterprise;

namespace MRL.SSL.Visualizer.Classes
{
    public static class DataReciever
    {
        static public int CamID = 0;
        static public bool OneCamera = false;
        static bool isComplete = false;
        static bool cam0Rec = false;
        static bool cam1Rec = false;
        static private double min_confidence = 0.6;
        static public double Min_confidence
        {
            get { return min_confidence; }
            set { min_confidence = value; }
        }

        static public messages_robocup_ssl_wrapper.SSL_WrapperPacket sslPacketCam0 = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
        static public messages_robocup_ssl_wrapper.SSL_WrapperPacket sslPacketCam1 = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
        static private int BallCount = 0;

        static public List<Robots> ViewedRobots = new List<Robots>();
        static public Color OpponentColor = Color.Yellow;
        static public Color OurColor = Color.Blue;
        static bool isInit = false;
        private static bool _showAIData = true;
        public static bool ShowAIData
        {
            get
            {
                return _showAIData;
            }
            set
            {
                _showAIData = value;
                //if (value)
                //{
                //    RecieveOn.Reset();
                //    _comcontroller.Dispose();

                //    _comcontroller = new CommunicationController(AISettings.Default.VisName, AISettings.Default.VisPort, AISettings.Default.AiName, AISettings.Default.AiPort);
                //    RecieveOn.Set();
                //}
                //else
                //{

                //    RecieveOn.Reset();
                //    _comcontroller.Dispose();

                //    _comcontroller = new CommunicationController(AISettings.Default.SSLVisionIP, AISettings.Default.SSLVisionPort);
                //    RecieveOn.Set();
                //}
            }
        }
        public static bool visComStarted = false;
        public static bool LoggerIsRun = false;

        /// <summary>
        /// percent of arrive to point in pass shoot tune
        /// </summary>
        public static int distancePercent = 0;

        /// <summary>
        /// frames that recieved
        /// </summary>
        public static int FramCount = 0;
        /// <summary>
        /// chrtert is run
        /// </summary>
        public static bool ChartRun = true;
        /// <summary>
        /// on recieve mode must be true
        /// </summary>
        internal static bool recieveFlag = false;
        /// <summary>
        /// thread is run or no
        /// </summary>
        internal static bool threadIsRun = false;
        /// <summary>
        /// google serializer
        /// </summary>
        private static GoogleSerializer googleSerializer = new GoogleSerializer();
        /// <summary>
        /// main cimmonication controller
        /// </summary>
        public static CommunicationController _comcontroller = new CommunicationController(AISettings.Default.VisName, AISettings.Default.VisPort, AISettings.Default.AiName, AISettings.Default.x);
        public static CommunicationController _comcontrollervision;
        //public static CommunicationController _sharedVisionConnection = new CommunicationController(AISettings.Default.SSLVisionIP, AISettings.Default.SSLVisionPort);
        /// <summary>
        /// data reciever thread
        /// </summary>
        internal static Thread RecieveThread = new Thread(new ThreadStart(Reciever));
        /// <summary>
        /// an event handler for recieving data from Ai
        /// </summary>
        public static ManualResetEvent RecieveOn = new ManualResetEvent(false);
        /// <summary>
        /// current WorldModel that sended from ai
        /// </summary>
        public static AiToVisualizerWrapper CurrentWrapper = new AiToVisualizerWrapper();
        /// <summary>
        /// current data packet that sended from Ai
        /// </summary>
        public static MemoryStream CurrentPacket = new MemoryStream();

        private static GoogleSerializer tempSerialize = new GoogleSerializer();
        /// <summary>
        /// set recive mode on/off
        /// </summary>
        public static void ChangeState()
        {

            if (!threadIsRun)
            {
                RecieveThread.Start();
                threadIsRun = true;
            }
            if (recieveFlag)
            {
                RecieveOn.Reset();
                recieveFlag = false;
            }
            else
            {
                RecieveOn.Set();
                recieveFlag = true;
            }
        }

        public static EventHandler RecivePacketOn;

        /// <summary>
        /// Recieve Data From AI computer
        /// </summary>

        private static void Reciever()
        {
            while (RecieveOn.WaitOne())
            {
                try
                {
                    //Recive Data From AI
                    if (ShowAIData)
                    {
                        MemoryStream data = _comcontroller.RecieveData();

                        if (data != null)
                        {
                            if (DataRecieved != null)
                            {
                                if (LoggerIsRun)
                                {
                                    LogProssesor.VisionLog = false;
                                    LogProssesor.Current = data;
                                }
                                // CurrentWrapper.Techniques = null;
                                CurrentWrapper = googleSerializer.DeserializeAiToVisWrapper(data, !CharterThread.waitting);
                                if (CurrentWrapper == null)
                                    continue;
                                if (CurrentWrapper.StrategySended)
                                    if (StrategyRecived != null)
                                        StrategyRecived(null);

                                if (CurrentWrapper.Techniques != null && CurrentWrapper.Techniques.Count > 0)
                                    if (TechniquesRecived != null)
                                        TechniquesRecived(null);
                                if (CurrentWrapper.Engines != null && CurrentWrapper.Engines.Count > 0)
                                    if (EnginesRecived != null)
                                        EnginesRecived(null);
                                DrawTree();
                                if (CurrentWrapper.GlobalModel != null)
                                {
                                    OpponentColor = CurrentWrapper.GlobalModel.OurMarkerISYellow ? Color.Blue : Color.Yellow;
                                    OurColor = CurrentWrapper.GlobalModel.OurMarkerISYellow ? Color.Yellow : Color.Blue;

                                    if (CurrentWrapper.GlobalModel.Opponents != null)
                                        CurrentWrapper.GlobalModel.Opponents.ToList().ForEach(p =>
                                        {
                                            if (!ViewedRobots.Any(a => a.Id == p.Key && a.TeamColor.ToArgb() == OpponentColor.ToArgb()))
                                                ViewedRobots.Add(new Robots() { EngineId = 0, Id = p.Key, TeamColor = OpponentColor });
                                        });
                                    if (CurrentWrapper.GlobalModel.OurRobots != null)
                                        CurrentWrapper.GlobalModel.OurRobots.ToList().ForEach(p =>
                                        {
                                            if (!ViewedRobots.Any(a => a.Id == p.Key && a.TeamColor.ToArgb() == OurColor.ToArgb()))
                                                ViewedRobots.Add(new Robots() { EngineId = 0, Id = p.Key, TeamColor = OurColor });
                                        });
                                }
                                DataRecieved(null, data);
                                FramCount++;
                                FraneRate(FramCount);
                                if (ChartRun)
                                    ChartIsRun(FramCount);
                                if (MustSendToRobot != null)
                                    MustSendToRobot(null, CurrentWrapper.RobotCommnd);
                                if (CalcKickVector != null)
                                    CalcKickVector(null);
                            }
                        }
                        if (!isInit)
                        {
                            isInit = true;
                            DataSender.CurrentWrapper.RequestTable.Add("GameParameters", true);
                            DataSender.SendOn.Set();
                        }
                    }
                    else
                    {
                        if (!visComStarted)
                        {
                            //  globalMerger = new GlobalMerger();
                            _comcontrollervision = new CommunicationController(AISettings.Default.SSLVisionIP, AISettings.Default.SSLVisionPort);
                            visComStarted = true;
                        }

                        MemoryStream sharedVisionStream = new MemoryStream();
                        sharedVisionStream = _comcontrollervision.RecieveVisionData();
                        if (sharedVisionStream != null)
                        {
                            SSL_WrapperPacket sslPacket = googleSerializer.DeserializeSSLVisionPacket(sharedVisionStream);
                            //switch (sslPacket.detection.camera_id)
                            //{
                            //    case 0:
                            //        VisionCalibrate.packet0 = sslPacket;
                            //        break;
                            //    case 1:
                            //        VisionCalibrate.packet1 = sslPacket;
                            //        break;
                            //    case 2:
                            //        VisionCalibrate.packet2 = sslPacket;
                            //        break;
                            //    case 3:
                            //        VisionCalibrate.packet3 = sslPacket;
                            //        break;
                            //}

                            PacketRecieved(null, sslPacket);

                            //CurrentWrapper.AllBalls = new Dictionary<int, Position2D>();
                            //CurrentWrapper.Model = new WorldModel();
                            //  CurrentWrapper.Model = Converter(sslPacket, false);
                            //if (DataRecieved != null && CurrentWrapper.Model != null)
                            //    DataRecieved(null, sharedVisionStream);
                            //DrawTree();
                            if (LoggerIsRun)
                            {
                                tempSerialize.stream = new MemoryStream();
                                tempSerialize.SerializeAiToVisWrapper(CurrentWrapper);
                                LogProssesor.Current = tempSerialize.stream;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex.ToString());
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private static void DrawTree()
        {
            try
            {
                #region Draw Tree
                if (DrawingObjects.ObjectTree.Count == 0)
                    DrawingObjects.ObjectTree.Add(new TreeViewModel("DrawingObjects"));

                if (TreeViewModel.GetItemByName("Global", DrawingObjects.ObjectTree[0]) == null)
                {
                    DrawingObjects.ObjectTree[0].Children.Add(new TreeViewModel("Global"));
                    DrawingObjects.ObjectTree[0].Children.Last().Parent = DrawingObjects.ObjectTree[0];
                }
                TreeViewModel globalroot = TreeViewModel.GetItemByName("Global", DrawingObjects.ObjectTree[0]);
                Dictionary<string, object> copy = new Dictionary<string, object>();
                // lock (DrawingObjects.drawingObject)
                DrawingObjects.drawingObject.ToList().ForEach(p =>
                {
                    if (p.Key != null && p.Value != null)
                        copy[p.Key] = p.Value;
                });

                foreach (var item in copy)
                {
                    if (item.Value != null && item.Key != null)
                    {
                        if (DrawingObjects.drawingObject.ContainsKey(item.Key) && item.Value.GetType() == typeof(DrawCollection))
                        {
                            #region DrawCollection
                            if (TreeViewModel.GetItemByName("Draw Collections", DrawingObjects.ObjectTree[0]) == null)
                            {
                                DrawingObjects.ObjectTree[0].Children.Add(new TreeViewModel("Draw Collections") { IsChecked = true });
                                DrawingObjects.ObjectTree[0].Children.Last().Parent = DrawingObjects.ObjectTree[0];
                                DataChanged(null);
                            }
                            if (TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]) == null)
                            {
                                TreeViewModel t = TreeViewModel.GetItemByName("Draw Collections", DrawingObjects.ObjectTree[0]);
                                t.Children.Add(new TreeViewModel(item.Key) { IsChecked = true });
                                t.Children.Last().Parent = TreeViewModel.GetItemByName("Draw Collections", DrawingObjects.ObjectTree[0]);
                                DataChanged(null);
                            }
                            foreach (var item1 in item.Value.As<DrawCollection>().drawingObject.ToList())
                            {
                                if (item1.Key != null)
                                {
                                    #region draw lines in groups
                                    if (item.Value.As<DrawCollection>().drawingObject.ContainsKey(item1.Key) && item1.Value.GetType() == typeof(Line) && item1.Value.As<Line>().IsShown)
                                    {
                                        if (TreeViewModel.GetItemByName("Lines", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])) == null)
                                        {
                                            TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]).Children.Add(new TreeViewModel("Lines"));
                                            TreeViewModel.GetItemByName("Lines", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Parent = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                                            DataChanged(null);
                                        }
                                        if (TreeViewModel.GetItemByName(item1.Key, TreeViewModel.GetItemByName("Lines", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]))) == null)
                                        {
                                            TreeViewModel.GetItemByName("Lines", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Children.Add(new TreeViewModel(item1.Key));
                                            TreeViewModel t = TreeViewModel.GetItemByName(item1.Key, DrawingObjects.ObjectTree[0]);
                                            t.Parent = TreeViewModel.GetItemByName("Lines", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]));
                                            t.SetIsChecked(item1.Value.As<Line>().IsShown, true, true);
                                            DataChanged(null);
                                        }
                                    }
                                    #endregion

                                    #region Draw circles in groups
                                    else if (item.Value.As<DrawCollection>().drawingObject.ContainsKey(item1.Key) && item1.Value.GetType() == typeof(Circle) && item1.Value.As<Circle>().IsShown)
                                    {
                                        if (TreeViewModel.GetItemByName("Circles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])) == null)
                                        {
                                            TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]).Children.Add(new TreeViewModel("Circles"));
                                            TreeViewModel.GetItemByName("Circles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Parent = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                                            DataChanged(null);
                                        }
                                        if (TreeViewModel.GetItemByName(item1.Key, TreeViewModel.GetItemByName("Circles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]))) == null)
                                        {
                                            TreeViewModel.GetItemByName("Circles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Children.Add(new TreeViewModel(item1.Key));
                                            TreeViewModel t = TreeViewModel.GetItemByName(item1.Key, DrawingObjects.ObjectTree[0]);
                                            t.Parent = TreeViewModel.GetItemByName("Circles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]));
                                            t.SetIsChecked(item1.Value.As<Circle>().IsShown, true, true);
                                            DataChanged(null);
                                        }
                                    }
                                    #endregion

                                    #region Drae Rectangles in Groups
                                    else if (item.Value.As<DrawCollection>().drawingObject.ContainsKey(item1.Key) && item1.Value.GetType() == typeof(FlatRectangle) && item1.Value.As<FlatRectangle>().IsShown)
                                    {
                                        if (TreeViewModel.GetItemByName("Rectangles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])) == null)
                                        {
                                            TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]).Children.Add(new TreeViewModel("Rectangles"));
                                            TreeViewModel.GetItemByName("Rectangles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Parent = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                                            DataChanged(null);
                                        }
                                        if (TreeViewModel.GetItemByName(item1.Key, TreeViewModel.GetItemByName("Rectangles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]))) == null)
                                        {
                                            TreeViewModel.GetItemByName("Rectangles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Children.Add(new TreeViewModel(item1.Key));
                                            TreeViewModel t = TreeViewModel.GetItemByName(item1.Key, DrawingObjects.ObjectTree[0]);
                                            t.Parent = TreeViewModel.GetItemByName("Rectangles", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]));
                                            t.SetIsChecked(item1.Value.As<FlatRectangle>().IsShown, true, true);
                                            DataChanged(null);
                                        }
                                    }
                                    #endregion

                                    #region draw SingleObjectState
                                    else if (item.Value.As<DrawCollection>().drawingObject.ContainsKey(item1.Key) && item1.Value.GetType() == typeof(SingleObjectState) && item1.Value.As<SingleObjectState>().IsShown)
                                    {
                                        if (TreeViewModel.GetItemByName("Robots", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])) == null)
                                        {
                                            TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]).Children.Add(new TreeViewModel("Robots"));
                                            TreeViewModel.GetItemByName("Robots", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Parent = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                                            DataChanged(null);
                                        }
                                        if (TreeViewModel.GetItemByName(item1.Key, TreeViewModel.GetItemByName("Robots", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]))) == null)
                                        {
                                            TreeViewModel.GetItemByName("Robots", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Children.Add(new TreeViewModel(item1.Key));
                                            TreeViewModel t = TreeViewModel.GetItemByName(item1.Key, DrawingObjects.ObjectTree[0]);
                                            t.Parent = TreeViewModel.GetItemByName("Robots", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]));
                                            t.SetIsChecked(item1.Value.As<SingleObjectState>().IsShown, true, true);
                                            DataChanged(null);
                                        }
                                    }
                                    #endregion

                                    #region Draw Text in Gruops
                                    else if (item.Value.As<DrawCollection>().drawingObject.ContainsKey(item1.Key) && item1.Value.GetType() == typeof(StringDraw) && item1.Value.As<StringDraw>().IsShown)
                                    {
                                        if (TreeViewModel.GetItemByName("Texts", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])) == null)
                                        {
                                            TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]).Children.Add(new TreeViewModel("Texts"));
                                            TreeViewModel.GetItemByName("Texts", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Parent = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                                            DataChanged(null);
                                        }
                                        if (TreeViewModel.GetItemByName(item1.Key, TreeViewModel.GetItemByName("Texts", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]))) == null)
                                        {
                                            TreeViewModel.GetItemByName("Texts", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Children.Add(new TreeViewModel(item1.Key));
                                            TreeViewModel t = TreeViewModel.GetItemByName(item1.Key, DrawingObjects.ObjectTree[0]);
                                            t.Parent = TreeViewModel.GetItemByName("Texts", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]));
                                            t.SetIsChecked(item1.Value.As<StringDraw>().IsShown, true, true);
                                            DataChanged(null);
                                        }
                                    }
                                    #endregion

                                    #region drawregion in Groups
                                    else if (item.Value.As<DrawCollection>().drawingObject.ContainsKey(item1.Key) && item1.Value.GetType() == typeof(DrawRegion) && item1.Value.As<DrawRegion>().IsShown)
                                    {
                                        if (TreeViewModel.GetItemByName("Regions", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])) == null)
                                        {
                                            TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]).Children.Add(new TreeViewModel("Regions"));
                                            TreeViewModel.GetItemByName("Regions", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Parent = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                                            DataChanged(null);
                                        }
                                        if (TreeViewModel.GetItemByName(item1.Key, TreeViewModel.GetItemByName("Regions", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]))) == null)
                                        {
                                            TreeViewModel.GetItemByName("Regions", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0])).Children.Add(new TreeViewModel(item1.Key));
                                            TreeViewModel t = TreeViewModel.GetItemByName(item1.Key, DrawingObjects.ObjectTree[0]);
                                            t.Parent = TreeViewModel.GetItemByName("Regions", TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]));
                                            t.SetIsChecked(item1.Value.As<DrawRegion>().IsShown, true, true);
                                            DataChanged(null);
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        else if (DrawingObjects.drawingObject.ContainsKey(item.Key) && item.Value.GetType() == typeof(Line) && item.Value.As<Line>().IsShown)
                        {
                            #region Draw Lines
                            if (TreeViewModel.GetItemByName("Lines", globalroot) == null)
                            {
                                globalroot.Children.Add(new TreeViewModel("Lines"));
                                TreeViewModel.GetItemByName("Lines", globalroot).Parent = globalroot;
                                DataChanged(null);
                            }
                            if (TreeViewModel.GetItemByName(item.Key, globalroot) == null)
                            {
                                TreeViewModel.GetItemByName("Lines", globalroot).Children.Add(new TreeViewModel(item.Key));
                                TreeViewModel t = TreeViewModel.GetItemByName(item.Key, globalroot);
                                t.Parent = TreeViewModel.GetItemByName("Lines", globalroot);
                                t.SetIsChecked(item.Value.As<Line>().IsShown, true, true);
                                DataChanged(null);
                            }
                            #endregion
                        }
                        else if (DrawingObjects.drawingObject.ContainsKey(item.Key) && item.Value.GetType() == typeof(Circle) && item.Value.As<Circle>().IsShown)
                        {
                            #region Draw Circles
                            if (TreeViewModel.GetItemByName("Circles", globalroot) == null)
                            {
                                globalroot.Children.Add(new TreeViewModel("Circles"));
                                TreeViewModel.GetItemByName("Circles", globalroot).Parent = globalroot;
                                DataChanged(null);
                            }
                            if (TreeViewModel.GetItemByName(item.Key, globalroot) == null)
                            {
                                TreeViewModel.GetItemByName("Circles", globalroot).Children.Add(new TreeViewModel(item.Key));
                                TreeViewModel t = TreeViewModel.GetItemByName(item.Key, globalroot);
                                t.Parent = TreeViewModel.GetItemByName("Circles", globalroot);
                                t.SetIsChecked(item.Value.As<Circle>().IsShown, true, true);
                                DataChanged(null);
                            }
                            #endregion
                        }
                        else if (DrawingObjects.drawingObject.ContainsKey(item.Key) && item.Value.GetType() == typeof(FlatRectangle) && item.Value.As<FlatRectangle>().IsShown)
                        {
                            #region DrawRectangles
                            if (TreeViewModel.GetItemByName("Rectangles", globalroot) == null)
                            {
                                globalroot.Children.Add(new TreeViewModel("Rectangles"));
                                TreeViewModel.GetItemByName("Rectangles", globalroot).Parent = globalroot;
                                DataChanged(null);
                            }
                            if (TreeViewModel.GetItemByName(item.Key, globalroot) == null)
                            {
                                TreeViewModel.GetItemByName("Rectangles", globalroot).Children.Add(new TreeViewModel(item.Key));
                                TreeViewModel t = TreeViewModel.GetItemByName(item.Key, globalroot);
                                t.Parent = TreeViewModel.GetItemByName("Rectangles", globalroot);
                                t.SetIsChecked(item.Value.As<FlatRectangle>().IsShown, true, true);
                                DataChanged(null);
                            }
                            #endregion
                        }
                        else if (DrawingObjects.drawingObject.ContainsKey(item.Key) && item.Value.GetType() == typeof(StringDraw) && item.Value.As<StringDraw>().IsShown)
                        {
                            #region Draw Texts
                            if (TreeViewModel.GetItemByName("Texts", globalroot) == null)
                            {
                                globalroot.Children.Add(new TreeViewModel("Texts"));
                                TreeViewModel.GetItemByName("Texts", globalroot).Parent = globalroot;
                                DataChanged(null);
                            }
                            if (TreeViewModel.GetItemByName(item.Key, globalroot) == null)
                            {
                                TreeViewModel.GetItemByName("Texts", globalroot).Children.Add(new TreeViewModel(item.Key));
                                TreeViewModel t = TreeViewModel.GetItemByName(item.Key, globalroot);
                                t.Parent = TreeViewModel.GetItemByName("Texts", globalroot);
                                t.SetIsChecked(item.Value.As<StringDraw>().IsShown, true, true);
                                DataChanged(null);
                            }
                            #endregion
                        }
                        else if (DrawingObjects.drawingObject.ContainsKey(item.Key) && item.Value.GetType() == typeof(DrawRegion) && item.Value.As<DrawRegion>().IsShown)
                        {
                            #region Draw Texts
                            if (TreeViewModel.GetItemByName("Region & Paths", globalroot) == null)
                            {
                                globalroot.Children.Add(new TreeViewModel("Region & Paths"));
                                TreeViewModel.GetItemByName("Region & Paths", globalroot).Parent = globalroot;
                                DataChanged(null);
                            }
                            if (TreeViewModel.GetItemByName(item.Key, globalroot) == null)
                            {
                                TreeViewModel.GetItemByName("Region & Paths", globalroot).Children.Add(new TreeViewModel(item.Key));
                                TreeViewModel t = TreeViewModel.GetItemByName(item.Key, globalroot);
                                t.Parent = TreeViewModel.GetItemByName("Region & Paths", globalroot);
                                t.SetIsChecked(item.Value.As<DrawRegion>().IsShown, true, true);
                                DataChanged(null);
                            }
                            #endregion
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.ToString());
            }
        }
        /// <summary>
        /// abort thraed
        /// </summary>
        public static void Abort()
        {
            RecieveThread.Abort();
            _comcontroller.Dispose();
        }

        /// <summary>
        /// event for Packet Rciveing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Data"></param>
        public delegate void PacketRecievedEventHandler(object sender, SSL_WrapperPacket Packet);
        public static event PacketRecievedEventHandler PacketRecieved;

        /// <summary>
        /// main event for draw field and objects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Data"></param>
        public delegate void DataRecievedEventHandler(object sender, MemoryStream Data);
        public static event DataRecievedEventHandler DataRecieved;
        /// <summary>
        /// event for darw object tree
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DrawingObjectChanged(object sender);
        public static event DrawingObjectChanged DataChanged;
        /// <summary>
        /// an events for drawing chart
        /// </summary>
        /// <param name="sender"></param>
        public delegate void CharterRun(int frameCount);
        public static event CharterRun ChartIsRun;
        /// <summary>
        /// an event calculate frame rate
        /// </summary>
        /// <param name="framCount">frames count</param>
        public delegate void FPS(int framCount);
        public static event FPS FraneRate;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        public delegate void TechniquesRecievedEventHandler(object sender);
        public static event TechniquesRecievedEventHandler TechniquesRecived;

        public delegate void StrategyRecievedEventHandler(object sender);
        public static event StrategyRecievedEventHandler StrategyRecived;

        public delegate void EnginesRecievedEventHandler(object sender);
        public static event EnginesRecievedEventHandler EnginesRecived;

        public delegate void DataRecievedAndSendToRobotEventHandler(object sender, Dictionary<int, SingleWirelessCommand> sendDataToRobots);
        public static event DataRecievedAndSendToRobotEventHandler MustSendToRobot;

        public delegate void KickVectorEventHandler(object sender);
        public static event KickVectorEventHandler CalcKickVector;

    }
}
