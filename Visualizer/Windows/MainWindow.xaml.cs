using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Visualizer.Classes;
using MRL.SSL.Visualizer.Extentions;
using MRL.SSL.Visualizer.Classes;
using MRL.SSL.GameDefinitions;
using System.Diagnostics;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Windows.Forms.DataVisualization.Charting;
using Visualizer.UserControls;
using MRL.SSL.CommonClasses;
using MRL.SSL.GameDefinitions.General_Settings;
using System.Threading;
using System.Configuration;
using MRL.SSL.GameDefinitions.Visualizer_Classes;
using Microsoft.Win32;
using System.Reflection;
using Enterprise;
using Simulator;
using System.Net.Sockets;
using System.Net;
using MRL.SSL.Visualizer.Windows;
using System.IO;


namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Queue<AveragingInfo> _averagingQueue;
        int _lastUsedFrameCount = 0;
        Stopwatch _stopwatch;
        Vector speedtemp;
        WirelessSender WS;
        bool selectBallMode = false, imageFlag = false;
        int i = 0, j = 0;
        int? selectedObjID = null;
        SingleObjectState selectedObject = null;
        Position2D FieldMouseocation;
        bool batteryMustRefreshed = false;
        Thread deviceReader;
        ManualResetEvent deviceReaderSignal;
        public bool ballPlacementFlag;
        List<Robots> currentRobots = new List<Robots>();
        RefreeBoxWindow refBoxWindow = new RefreeBoxWindow();

        public MainWindow()
        {
            
            InitializeComponent();
            //  var f = GameSettings.Default.Score;
            _averagingQueue = new Queue<AveragingInfo>();
            _stopwatch = new Stopwatch();
            FieldMouseocation = new Position2D();
            selectedObject = new SingleObjectState();
            DataReciever.DataRecieved += new DataReciever.DataRecievedEventHandler(DataReciever_DataRecieved);
            DataReciever.DataChanged += new DataReciever.DrawingObjectChanged(DataReciever_DataChanged);
            DataReciever.ChartIsRun += new DataReciever.CharterRun(DataReciever_ChartIsRun);
            DataReciever.FraneRate += new DataReciever.FPS(DataReciever_FraneRate);
            DataReciever.EnginesRecived += new DataReciever.EnginesRecievedEventHandler(DataReciever_EnginesRecived);
            DataReciever.CalcKickVector += new DataReciever.KickVectorEventHandler(DataReciever_CalcKickVector);
            Field.MouseClick += new System.Windows.Forms.MouseEventHandler(Field_MouseClick);
            Field.MouseDown += new System.Windows.Forms.MouseEventHandler(Field_MouseDown);
            Field.MouseUp += new System.Windows.Forms.MouseEventHandler(Field_MouseUp);
            Field.MouseMove += new System.Windows.Forms.MouseEventHandler(Field_MouseMove);
            Field.addOurRobotToolStripMenuItem.Click += new EventHandler(addOurRobotToolStripMenuItem_Click);
            Field.addOppRobotToolStripMenuItem.Click += new EventHandler(addOppRobotToolStripMenuItem_Click);
            Field.addBallToolStripMenuItem.Click += new EventHandler(addBallToolStripMenuItem_Click);
            Field.creatAllRobotToolStripMenuItem.Click += new EventHandler(creatAllRobotToolStripMenuItem_Click);
            Field.MouseWheel += new System.Windows.Forms.MouseEventHandler(Field_MouseWheel);
            WirelessReciever.StatusRecieved += new WirelessReciever.RecievedBattery(WirelessReciever_StatusRecieved);
            WirelessReciever.SensorRecieved += new WirelessReciever.RecievedSensor(WirelessReciever_SensorRecieved);
            WS = new WirelessSender();
            mainTabControl.Visibility = Visibility.Collapsed;
            RobotControllerUserControl.CommandChanged += new RobotControllerUserControl.CommandChangedEventHandler(RobotControllerUserControl_CommandChanged);
            TuneVariables.Refreshed += new TuneVariables.RefreshedEventHandler(TuneVariables_Refreshed);
            LogProssesor.loggerFinished += new LogProssesor.LoggerFinishedEventHandler(LogProssesor_loggerFinished);
            deviceReader = new Thread(new ThreadStart(getCommand));
            deviceReaderSignal = new ManualResetEvent(false);
            deviceReader.Start();
            //windowsFormsHost2.Child = Field;

        }

        float angle = 0;

        void Field_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!Field.WheelActive && selectedObject != null && DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
            {
                System.Drawing.Color color = System.Drawing.Color.Blue;

                if (selectedObject.Type == ObjectType.OurRobot && DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                    color = System.Drawing.Color.Yellow;
                else if (selectedObject.Type == ObjectType.Opponent && !DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                    color = System.Drawing.Color.Yellow;
                else
                    color = System.Drawing.Color.Blue;
                angle += Math.Sign(e.Delta) * (360 * Math.Abs(e.Delta)) / 3600;
                if (angle > 360)
                    angle = 0;
                selectedObject.Angle = angle;
                simulator.setRobotPosition(selectedObjID.Value, color, selectedObject.Location, (float)angle);
                // simulator.ChangeModel(selectedObject, selectedObjID);
            }
        }

        void addBallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
            {
                if (Field.Model == null)
                    Field.Model = new WorldModel();

                Field.Model.BallState = new SingleObjectState(ObjectType.Ball, FieldMouseocation, Vector2D.Zero, Vector2D.Zero, 90, null);
                simulator.setBallPosition(0, Field.Model.BallState.Location);
                Field.Invalidate();
            }
        }

        void addOppRobotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
            {
                //if (Field.Model == null)
                //    Field.Model = new WorldModel();
                //if (Field.Model.Opponents == null)
                //    Field.Model.Opponents = new Dictionary<int, SingleObjectState>();
                int? id = null;

                //Field.Model.Opponents.Add(id, new SingleObjectState(ObjectType.Opponent, FieldMouseocation, Vector2D.Zero, Vector2D.Zero, 90, null));

                if (currentRobots != null)
                {
                    if (currentRobots.Count != 0)
                        id = new List<int>() { 8, 9, 10, 11, 12, 13, 14, 15 }.FirstOrDefault(s => !currentRobots.Any(a => a.Id == s && a.TeamColor.ToArgb() == System.Drawing.Color.Yellow.ToArgb()));
                    else
                        id = 8;

                    //System.Drawing.Color color = System.Drawing.Color.Black;
                    //if (DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                    //    color = System.Drawing.Color.Blue;
                    //else
                    //    color = System.Drawing.Color.Yellow;

                    if (id != 0 && id.HasValue)
                        simulator.addRobot(id.Value, System.Drawing.Color.Yellow, FieldMouseocation, 90);
                }
                Field.Invalidate();
            }
        }

        void addOurRobotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
            {
                int? id = null;
                if (currentRobots != null)
                {
                    if (currentRobots.Count != 0)
                        id = new List<int>() {  0,1, 2, 3, 4, 5, 6, 7 }.FirstOrDefault(s => !currentRobots.Any(a => a.Id == s && a.TeamColor.ToArgb() == System.Drawing.Color.Blue.ToArgb()));
                    else
                        id = 0;

                    if (id.HasValue)
                        simulator.addRobot(id.Value, System.Drawing.Color.Blue, FieldMouseocation, 90);
                }
                Field.Invalidate();
            }
        }

        void creatAllRobotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WorldModel simModel = new WorldModel();
            simModel.OurRobots = new Dictionary<int, SingleObjectState>();
            simModel.OurRobots.Add(1, new SingleObjectState(ObjectType.OurRobot, new Position2D(0, 2), Vector2D.Zero, Vector2D.Zero, 90, null));
            simModel.OurRobots.Add(2, new SingleObjectState(ObjectType.OurRobot, new Position2D(0, 2), Vector2D.Zero, Vector2D.Zero, 90, null));
            simModel.OurRobots.Add(3, new SingleObjectState(ObjectType.OurRobot, new Position2D(0, 2), Vector2D.Zero, Vector2D.Zero, 90, null));
            simModel.OurRobots.Add(4, new SingleObjectState(ObjectType.OurRobot, new Position2D(0, 2), Vector2D.Zero, Vector2D.Zero, 90, null));
            simModel.OurRobots.Add(5, new SingleObjectState(ObjectType.OurRobot, new Position2D(0, 2), Vector2D.Zero, Vector2D.Zero, 90, null));

            simModel.Opponents = new Dictionary<int, SingleObjectState>();
            simModel.Opponents.Add(1, new SingleObjectState(ObjectType.Opponent, new Position2D(0, -2), Vector2D.Zero, Vector2D.Zero, -90, null));
            simModel.Opponents.Add(2, new SingleObjectState(ObjectType.Opponent, new Position2D(0, -2), Vector2D.Zero, Vector2D.Zero, -90, null));
            simModel.Opponents.Add(3, new SingleObjectState(ObjectType.Opponent, new Position2D(0, -2), Vector2D.Zero, Vector2D.Zero, -90, null));
            simModel.Opponents.Add(4, new SingleObjectState(ObjectType.Opponent, new Position2D(0, -2), Vector2D.Zero, Vector2D.Zero, -90, null));
            simModel.Opponents.Add(5, new SingleObjectState(ObjectType.Opponent, new Position2D(0, -2), Vector2D.Zero, Vector2D.Zero, -90, null));
            //simulator.InitializeWorldModel(simModel);            
        }

        bool leftClickDown = false;

        bool rightClickDown = false;

        Position2D firstposition = new Position2D();

        void Field_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Field.Transform != null)
                fieldTabItem.Header = "MainField  " + e.Location.ToPosition(Field.Transform).ToString();
            if (!rightClickDown && leftClickDown && selectedObjID.HasValue)
            {
                Position2D mouseLoc = PixelToMetric(e.Location);
                if (selectedObject != null)
                {
                    selectedObject.Location = mouseLoc;
                    if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
                    {
                        if (selectedObject.Type != ObjectType.Ball)
                        {
                            System.Drawing.Color color = System.Drawing.Color.Blue;

                            if (selectedObject.Type == ObjectType.OurRobot && DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                color = System.Drawing.Color.Yellow;
                            else if (selectedObject.Type == ObjectType.Opponent && !DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                color = System.Drawing.Color.Yellow;
                            else
                                color = System.Drawing.Color.Blue;

                            simulator.setRobotPosition(selectedObjID.Value, color, selectedObject.Location, selectedObject.Angle.Value);
                        }
                        else
                        {
                            simulator.setBallPosition(0, selectedObject.Location);
                            simulator.setBallSpeed(0, Vector2D.Zero);
                        }

                    }
                }
            }
            else if (selectBallMode && rightClickDown && leftClickDown && selectedObject != null)
            {
                Position2D mouseLoc = PixelToMetric(e.Location);
                Field.SpeedVector = new Line(selectedObject.Location, mouseLoc);
            }
        }

        void Field_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Field.WheelActive = true;
            if (!rightClickDown && leftClickDown)
            {
                leftClickDown = false;
                DataSender.CurrentWrapper.OjbMoved = false;
                Position2D mouseLoc = PixelToMetric(e.Location);
                if (selectedObject != null)
                {
                    selectedObject.Location = mouseLoc;
                    selectedObject.IsShown = false;
                    if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
                    {
                        if (selectedObject.Type != ObjectType.Ball)
                        {
                            System.Drawing.Color color = System.Drawing.Color.Blue;

                            if (selectedObject.Type == ObjectType.OurRobot && DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                color = System.Drawing.Color.Yellow;
                            else if (selectedObject.Type == ObjectType.Opponent && !DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                color = System.Drawing.Color.Yellow;
                            else
                                color = System.Drawing.Color.Blue;

                            simulator.setRobotPosition(selectedObjID.Value, color, selectedObject.Location, selectedObject.Angle.Value);
                        }
                        else
                        {
                            simulator.setBallPosition(0, selectedObject.Location);
                            simulator.setBallSpeed(0, Vector2D.Zero);
                        }
                    }

                }
            }
            else if (rightClickDown && leftClickDown && DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
            {
                rightClickDown = false;
                if (selectBallMode && selectedObject != null)
                {
                    Field.SpeedVector = null;
                    Position2D mouseLoc = PixelToMetric(e.Location);
                    speedtemp = mouseLoc - selectedObject.Location;
                    selectedObject.Speed = speedtemp;
                    FieldMouseocation = PixelToMetric(e.Location);
                    if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
                    {
                        if (selectedObject.Type != ObjectType.Ball)
                        {
                            System.Drawing.Color color = System.Drawing.Color.Blue;

                            if (selectedObject.Type == ObjectType.OurRobot && DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                color = System.Drawing.Color.Yellow;
                            else if (selectedObject.Type == ObjectType.Opponent && !DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                color = System.Drawing.Color.Yellow;
                            else
                                color = System.Drawing.Color.Blue;

                            simulator.setRobotPosition(selectedObjID.Value, color, selectedObject.Location, selectedObject.Angle.Value);
                        }
                        else
                        {
                            simulator.setBallPosition(selectedObjID.Value, selectedObject.Location);
                            simulator.setBallSpeed(0, 5 * (mouseLoc - selectedObject.Location));
                        }
                    }

                }
            }
            selectedObject = null;
        }

        void refreshBatteryToControl()
        {
            foreach (var item in BatteryInfo.Info)
                if (robotstatusListBox.Items.Cast<RobotViewerUserControl>().Any(s => s.Number == item.Key))
                    robotstatusListBox.Items.Cast<RobotViewerUserControl>().Single(s => s.Number == item.Key).BatteryHeart = item.Value;


        }

        void Field_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                leftClickDown = true;
                if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Simulator && simIsInit)
                {
                    Field.WheelActive = false;
                    Position2D mouseLoc = PixelToMetric(e.Location);
                    firstposition = mouseLoc;
                    ObjectType type;
                    int? selected = SelectedObject(mouseLoc, out type);
                    if (selected.HasValue)
                    {
                        selectedObjID = selected;
                        DataSender.CurrentWrapper.OjbMoved = true;
                        if (type == ObjectType.Opponent)
                        {
                            selectedObject = new SingleObjectState(type, mouseLoc, Vector2D.Zero, Vector2D.Zero, Field.Model.Opponents[selected.Value].Angle, 0);
                            if (DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                simulator.setRobotPosition(selected.Value, System.Drawing.Color.Blue, selectedObject.Location, selectedObject.Angle.Value);
                            else
                                simulator.setRobotPosition(selected.Value, System.Drawing.Color.Yellow, selectedObject.Location, selectedObject.Angle.Value);
                            DataSender.CurrentWrapper.SelectedToMove = selectedObject;
                        }
                        else if (type == ObjectType.OurRobot)
                        {
                            selectedObject = new SingleObjectState(type, mouseLoc, Vector2D.Zero, Vector2D.Zero, Field.Model.OurRobots[selected.Value].Angle, 0);
                            if (DataReciever.CurrentWrapper.Model.OurMarkerISYellow)
                                simulator.setRobotPosition(selected.Value, System.Drawing.Color.Yellow, selectedObject.Location, selectedObject.Angle.Value);
                            else
                                simulator.setRobotPosition(selected.Value, System.Drawing.Color.Blue, selectedObject.Location, selectedObject.Angle.Value);
                            DataSender.CurrentWrapper.SelectedToMove = selectedObject;
                        }
                        else
                        {
                            selectedObject = new SingleObjectState(type, mouseLoc, Vector2D.Zero, Vector2D.Zero, Field.Model.BallState.Angle, 0);
                            simulator.setBallPosition(selected.Value, selectedObject.Location);
                            DataSender.CurrentWrapper.SelectedToMove = selectedObject;
                        }
                        //DataSender.CurrentWrapper.SelectedID = selected.Value;
                        //Thread t = new Thread((ThreadStart)(() =>
                        //{
                        //    while (DataSender.CurrentWrapper.OjbMoved)
                        //    {
                        //        DataSender.CurrentWrapper.SendData.Add("MovedObj");
                        //        DataSender.SendOn.Set();
                        //        Thread.Sleep(16);
                        //    }
                        //}));
                        //t.Start();
                    }
                }
                else if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.SSLVision)
                {
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                rightClickDown = true;
                FieldMouseocation = PixelToMetric(e.Location);

            }
        }

        void WirelessReciever_StatusRecieved(BatteryStatus sender)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (robotstatusListBox.Items.Cast<RobotViewerUserControl>().Any(a => a.Number == sender.RobotID))
                {
                    int batterypercent = (int)Math.Round((sender.BatteryLife - 14) * (100 / 2.87f));
                    if (batterypercent < 0)
                        batterypercent = 0;
                    else if (batterypercent > 100)
                        batterypercent = 100;
                    BatteryInfo.Info[sender.RobotID] = batterypercent;
                }
            }));
        }

        void WirelessReciever_SensorRecieved(bool isOn, int robotID)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (robotstatusListBox.Items.Cast<RobotViewerUserControl>().Any(a => a.Number == robotID))
                    robotstatusListBox.Items.Cast<RobotViewerUserControl>().Single(s => s.Number == robotID).Sensor = isOn;

            }));
            DataSender.CurrentWrapper.SensoreState[robotid] = isOn;
            DataSender.CurrentWrapper.SendData.Add("Sensors");
            DataSender.SendOn.Set();

        }

        void DataReciever_EnginesRecived(object sender)
        {
            List<Engines> list = new List<Engines>();
            foreach (var item in DataReciever.CurrentWrapper.Engines.Keys)
                list.Add(DataReciever.CurrentWrapper.Engines[item]);
            Dispatcher.Invoke((Action)(() =>
            {
                engineListView.ItemsSource = list.ToList();
            }));
        }

        void playButton_Click(object sender, EventArgs e)
        {
            CharterThread.ChangState();
        }
        void strategyManagerButton_Click(object sender, EventArgs e)
        {
            new StrategyManagerWindow().ShowAsSingleTab(fieldTabControl, "strategymanager");
            //new StrategyDesigner().ShowAsSingleTab(fieldTabControl, "strategymanager");
        }
        void CharterThread_ShowData(object sender)
        {
            //  Dispatcher.Invoke((Action)(() => RefreshCharter(0)));
        }

        private void logPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            //if (mainTabControl.Visibility != Visibility.Visible)
            //    mainTabControl.Visibility = Visibility.Visible;
            //double width;
            new LoggerPropertiesWindow().ShowAsSingleTab(fieldTabControl, "Logsetting");
            //mainTabControl.Width = width;
        }

        private void techniqueButton_Click(object sender, RoutedEventArgs e)
        {
            //if (mainTabControl.Visibility != Visibility.Visible)
            //    mainTabControl.Visibility = Visibility.Visible;
            // double width;
            //new TechniqueWindow().ShowAsSingleTab(fieldTabControl, "technique");
            // mainTabControl.Width = width;
        }

        private void tacticButton_Click(object sender, RoutedEventArgs e)
        {
            //if (mainTabControl.Visibility != Visibility.Visible)
            //    mainTabControl.Visibility = Visibility.Visible;
            // double width;
            new TacticWindow().ShowAsSingleTab(fieldTabControl, "tactic");
            // mainTabControl.Width = width;
        }
        private void refreeBoxButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainTabControl.Visibility != Visibility.Visible)
                mainTabControl.Visibility = Visibility.Visible;
            //double width;
            refBoxWindow.ShowAsSingleTab(mainTabControl, "refreebox");

            //  mainTabControl.Width = width;
        }

        private void recieveButton_Click(object sender, RoutedEventArgs e)
        {
            DataReciever.ChartRun = true;
            DataReciever.ChangeState();
            if (imageFlag)
            {
                imageFlag = false;
                recieveImage.SetImageSource("display-off-48x48.png");
            }
            else
            {
                imageFlag = true;
                recieveImage.SetImageSource("display-32x32.png");
            }
        }

        int counter = 0;
        int robotid = 1;
        GameStatus lastStatus = GameStatus.Halt;

        void DataReciever_DataRecieved(object sender, System.IO.MemoryStream Data)
        {
            try
            {
                //SaveModelData SMD;
                //Field.VisualizerObject.AddObject(new Line(Position2D.Zero, new Position2D(0, 1)),"Line");
                if (lastStatus == GameStatus.MoveRobot && DataReciever.CurrentWrapper.Model.Status != GameStatus.MoveRobot)
                {
                    RobotComponentsController.SelectedID = null;
                    RobotComponentsController.Angle = 0;
                    RobotComponentsController.Target = Position2D.Zero;
                    DataSender.CurrentWrapper.SendData.Add("MoveRobot");
                    DataSender.SendOn.Set();
                }
                if (DataReciever.ShowAIData)
                    Field.VisionMode = false;
                else
                    Field.VisionMode = true;

                Field.SelectedRobotID = RobotComponentsController.SelectedID;
                Field.CurrentWrapper = DataReciever.CurrentWrapper;

                if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Analizer || DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Visualizer) return;
                if (DataReciever.CurrentWrapper.Model != null)
                {
                    lastStatus = DataReciever.CurrentWrapper.Model.Status;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        eventsLabel.Content = "Our Score : " + DataReciever.CurrentWrapper.Model.OurScore + "   Opponent Score : "
                            + DataReciever.CurrentWrapper.Model.OpponentScore + "  Time Left : " + DataReciever.CurrentWrapper.Model.TimeLeft.ToString();
                    }));
                }

                LogProssesor.MemoryLog = DataReciever.CurrentWrapper;
                //VisualizerConsole.WriteLine(GameParameters.OurGoalCenter.X.ToString(), System.Drawing.Color.GreenYellow);
                Dispatcher.Invoke((Action)(() =>
                {
                    //ballstatusLabel.Content = WirelessReciever.RecieveCount;
                    if (changeBatery)
                    {
                        changeBatery = false;

                        counter++;
                        //if (counter % 200 == 0 && robotid < 7)
                        //{
                        //DataSender.CurrentWrapper.RobotID = robotid++;
                        DataSender.CurrentWrapper.RobotID = 1;
                        DataSender.CurrentWrapper.BatteryRequest = batteryMustRefreshed;
                        DataSender.CurrentWrapper.SendData.Add("BatteryRequest");
                        DataSender.SendOn.Set();
                        //}
                        //else if (robotid == 7)
                        //{
                        //    counter = 0;
                        //    robotid = 1;
                        //    batteryMustRefreshed = false;
                        //}
                    }


                    //if (VisualizerConsole.Data.Count > 0)
                    //{
                    //    mainConsol.mainListBox.Items.Add(VisualizerConsole.Data.ToList().Last());
                    //    if (mainConsol.mainListBox.Items.Count > 100)
                    //        mainConsol.mainListBox.Items.RemoveAt(0);
                    //    mainConsol.mainListBox.ScrollIntoView(mainConsol.mainListBox.Items[mainConsol.mainListBox.Items.Count - 1]);
                    //}

                    //mainConsol.mainListBox.Items.MoveCurrentToLast();
                    //VisualizerConsole.Data.Clear();


                    SyncRobotViewers();
                    refreshBatteryToControl();

                }));

                Dispatcher.Invoke((Action)(() =>
                {
                    if (DataReciever.LoggerIsRun)
                    {
                        loggertimeLabel.Content = LogProssesor.LoggerTime.Elapsed;
                        j++;
                        if (j == 40)
                        {
                            if (logImage.Visibility == Visibility.Visible)
                                logImage.Visibility = Visibility.Hidden;
                            else
                                logImage.Visibility = Visibility.Visible;
                            j = 0;
                        }
                    }
                    else if (!LogProssesor.loggerfinished)
                    {
                        logImage.SetImageSource("floppy.png");
                        logImage.Height = 27;
                        j++;
                        if (j > 40)
                        {
                            if (logImage.Visibility == Visibility.Visible)
                                logImage.Visibility = Visibility.Hidden;
                            else
                                logImage.Visibility = Visibility.Visible;
                            j = 0;
                        }
                    }
                }));
            }
            catch (TargetInvocationException ex)
            {
                Logger.WriteError("----Main Data Reciever----\n" + ex.ToString());
            }

        }

        void LogProssesor_loggerFinished(object sender)
        {

            Dispatcher.Invoke((Action)(() =>
            {
                logImage.SetImageSource("record.png");
                logImage.Height = 15;
                logImage.Visibility = Visibility.Visible;
            }));

        }

        bool RobotListChanged = false;

        private void SyncRobotViewers()
        {
            RobotListChanged = false;
            if (DataReciever.CurrentWrapper.GlobalModel == null) return;
            if (DataReciever.CurrentWrapper.GlobalModel.OurRobots != null)
                foreach (var item in DataReciever.CurrentWrapper.GlobalModel.OurRobots.Keys.ToList())
                {
                    if (!currentRobots.Any(a => a.Id == item && a.TeamColor.ToArgb() == DataReciever.OurColor.ToArgb()))
                    {
                        if (DataReciever.ViewedRobots.Any(a => a.Id == item && a.TeamColor == DataReciever.OurColor))
                            currentRobots.Add(DataReciever.ViewedRobots.First(a => a.Id == item && a.TeamColor == DataReciever.OurColor));
                        else
                            currentRobots.Add(new Robots() { EngineId = 0, Id = item, TeamColor = DataReciever.OurColor });
                        RobotListChanged = true;

                    }

                    #region robotControllerView
                    if (!robotstatusListBox.Items.Cast<RobotViewerUserControl>().Any(a => a.Number == item))
                        robotstatusListBox.Items.Add(new RobotViewerUserControl() { Name = "robot" + item.ToString(), Number = item });
                    if (!robotControllerStackPanel.Children.Cast<RobotControllerUserControl>().Any(a => a.RobotID == item))
                    {
                        RobotControllerUserControl r = new RobotControllerUserControl() { RobotID = item };
                        robotControllerStackPanel.Children.Add(r);
                    }
                    #endregion
                }
            if (DataReciever.CurrentWrapper.Model.Opponents != null)
                foreach (var item in DataReciever.CurrentWrapper.GlobalModel.Opponents.Keys.ToList())
                {
                    if (!currentRobots.Any(a => a.Id == item && a.TeamColor.ToArgb() == DataReciever.OpponentColor.ToArgb()))
                    {
                        try
                        {
                            if (DataReciever.ViewedRobots.Any(a => a.Id == item && a.TeamColor.ToArgb() == DataReciever.OpponentColor.ToArgb()))
                                currentRobots.Add(DataReciever.ViewedRobots.First(a => a.Id == item && a.TeamColor.ToArgb() == DataReciever.OpponentColor.ToArgb()));
                            else
                                currentRobots.Add(new Robots() { EngineId = 0, Id = item, TeamColor = DataReciever.OpponentColor });
                            RobotListChanged = true;
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(LogType.Exception, ex.ToString());
                        }
                    }
                }
            //List<Robots> rlist = currentRobots.ToList().Where(w =>
            //    !DataReciever.CurrentWrapper.GlobalModel.Opponents.ToList().Any(a => a.Key == w.Id && w.TeamColor == DataReciever.OpponentColor)
            //    && !DataReciever.CurrentWrapper.GlobalModel.OurRobots.ToList().Any(a => a.Key == w.Id && w.TeamColor == DataReciever.OurColor)).ToList<Robots>();

            int delcount = currentRobots.RemoveAll(w => !DataReciever.CurrentWrapper.GlobalModel.Opponents.ToList().Any(a => a.Key == w.Id && w.TeamColor == DataReciever.OpponentColor)
                && !DataReciever.CurrentWrapper.GlobalModel.OurRobots.ToList().Any(a => a.Key == w.Id && w.TeamColor == DataReciever.OurColor));
            //foreach (var item in rlist)
            //{
            //    if (currentRobots.Any(a => a.EngineId == item.EngineId && a.Id == item.Id && a.TeamColor == item.TeamColor))
            //        currentRobots.Remove(item);
            //}
            //if (rlist.Count > 0)
            //    robotListView.ItemsSource = currentRobots.ToList();
            if (RobotListChanged || delcount > 0)
            {
                robotListView.ItemsSource = currentRobots.ToList();
                sendEngineInfo();
            }

            #region update Viewers
            if (DataReciever.CurrentWrapper.GlobalModel.OurRobots == null) return;

            if (robotstatusListBox.Items.Count > 0)
            {
                List<RobotViewerUserControl> list = robotstatusListBox.Items.Cast<RobotViewerUserControl>().Where(w => !DataReciever.CurrentWrapper.GlobalModel.OurRobots.ContainsKey(w.Number)).ToList<RobotViewerUserControl>();
                foreach (var item in list)
                {
                    robotstatusListBox.Items.Remove(item);
                    robotControllerStackPanel.Children.Remove(robotControllerStackPanel.Children.Cast<RobotControllerUserControl>().Single(a => a.RobotID == item.Number));
                    //robotListView.Items.Remove(robotListView.Items.Cast<Robots>().Single(s => s.Id == item.Number && s.TeamColor == DataReciever.OurColor));
                }

                //robotControllerStackPanel.Children.(robotControllerStackPanel.Children.Cast<RobotControllerUserControl>().Single(a => a.RobotID == item.Number));



                foreach (var item in robotstatusListBox.Items.Cast<RobotViewerUserControl>())
                {
                    if (Field.CurrentWrapper.RobotCommnd != null && Field.CurrentWrapper.RobotCommnd.ContainsKey(item.Number))
                    {
                        if (Field.CurrentWrapper.RobotCommnd[item.Number].isChipKick)
                        {
                            item.ChipKick = true;
                            item.Kick = false;
                        }
                        else if (Field.CurrentWrapper.RobotCommnd[item.Number].KickPower != 0)
                        {
                            item.Kick = true;
                            item.ChipKick = false;
                        }
                        else
                        {
                            item.Kick = false;
                            item.ChipKick = false;
                        }
                    }
                }

            }
            #endregion
        }

        private void UpdateRobotController()
        {
            if (RobotComponentsController.RobotCommands == null)
                RobotComponentsController.RobotCommands = new Dictionary<int, SingleWirelessCommand>();
            else
                RobotComponentsController.RobotCommands.Clear();
            foreach (var item in robotControllerStackPanel.Children.Cast<RobotControllerUserControl>().ToList())
            {
                SingleWirelessCommand swc = new SingleWirelessCommand();
                swc._kickPower = item.KickPower;
                swc._kickPowerByte = item.KickPower;
                swc.isChipKick = item.ChipKick;
                swc.isDelayedKick = item.HasDelay;
                swc.SpinBack = (item.SpinBack) ? 1 : 0;
                swc.Vx = item.Velocity.X;
                swc.Vy = item.Velocity.Y;
                swc.W = item.W;
                swc.BackSensor = item.BackSensore;
                RobotComponentsController.RobotCommands.Add(item.RobotID, swc);
            }
        }

        void RobotControllerUserControl_CommandChanged(object sender)
        {
            UpdateRobotController();
            DataSender.CurrentWrapper.SendData.Add("Command");
            DataSender.SendOn.Set();
        }

        void DataReciever_DataChanged(object sender)
        {
            Dispatcher.Invoke((Action)(() => RedrawTree()));
        }

        void RedrawTree()
        {
            objectTreeView.ItemsSource = DrawingObjects.ObjectTree.ToList();
        }

        void DataReciever_ChartIsRun(int frameCount)
        {
            //Dispatcher.Invoke((Action)(() =>
            //{
            //    MainCharter.points.Add(new Point(frameCount, frameCount));
            //    MainCharter.source.RaiseDataChanged();
            //}));
        }

        private void mainTabControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mainTabControl.Items.Count == 0)
                mainTabControl.Visibility = Visibility.Collapsed;
        }

        void DataReciever_FraneRate(int framCount)
        {
            Dispatcher.Invoke((Action)(() => CalculateAverage(framCount)));
        }

        private void CalculateAverage(int framCount)
        {
            _lastUsedFrameCount = framCount;
            _stopwatch.Start();
            AveragingInfo ai = new AveragingInfo();
            ai.Frames = _lastUsedFrameCount;
            ai.EndTime = _stopwatch.Elapsed;
            _averagingQueue.Enqueue(ai);
            if (_averagingQueue.Count > 10)
            {
                AveragingInfo lai = _averagingQueue.Dequeue();
                i++;
                if (i == 30)
                {
                    statusLabel.Content = ((ai.Frames - lai.Frames) / (ai.EndTime - lai.EndTime).TotalSeconds).ToString("f2") + " fps ";
                    i = 0;
                }
                refereLabel.Content = " Command : " + DataReciever.CurrentWrapper.Model.Status.ToString();
                ballstatusLabel.Content = "Ball Status : " + DrawingObjects.BallStatuse;
                //refereLabel.Content = WirelessReciever.RecieveCount;
            }

        }

        public struct AveragingInfo
        {
            public TimeSpan EndTime;
            public int Frames;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataReciever.Abort();
            DataSender.Abort();
            CharterThread.Abort();
            if (simulator != null)
                simulator.Dispose();
            Application.Current.Shutdown();
            TuneVariables.Default.Save();
            GameSettings.Default.Save();
            AISettings.Default.Save();

            deviceReader.Abort();
            SaveVisSetting();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LookupTable.Load();
            ControlParameters.Load("ControlParameters");
            StrategyInfo.Load();


            InitializeVisFromSetting();
            List<string> ports = System.IO.Ports.SerialPort.GetPortNames().ToList();
            TuneVariables.Default.Save();
            InitializeVisFromSetting();
            ports.ForEach(p => portComboBox.Items.Add(p));
            DataSender.CurrentWrapper.RequestTable.Add("Engines", true);
            //DataSender.CurrentWrapper.RequestTable.Add("Techniques", true);
            DataSender.CurrentWrapper.RequestTable.Add("Strategies", true);
            DataSender.Start();
            DataSender.SendOn.Set();
            WirelessReciever.State = WirelessReciever.WirelessRecieveState.OFF;
            RefereshMainParameters();
            if (mainTabControl.Visibility != Visibility.Visible)
                mainTabControl.Visibility = Visibility.Visible;
            refBoxWindow.ShowAsSingleTab(mainTabControl, "refreebox");
        }

        void Default_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void InitializeVisFromSetting()
        {
            LogProssesor.LoggerDelay = 1;//VisualizerSettings.Default.LoggerDelay;
            LogProssesor.UseDefaulName = VisualizerSettings.Default.LogUseDefaultName;
            LogProssesor.UseDefaultAddress = VisualizerSettings.Default.LogUseDefaultAddress;
            LogProssesor.UserFileName = VisualizerSettings.Default.LogUserFileName;
            LogProssesor.UserLogAddress = VisualizerSettings.Default.LogUserLogAddress;
        }

        private void SaveVisSetting()
        {
            VisualizerSettings.Default.LoggerDelay = LogProssesor.LoggerDelay;
            VisualizerSettings.Default.LogUseDefaultName = LogProssesor.UseDefaulName;
            VisualizerSettings.Default.LogUseDefaultAddress = LogProssesor.UseDefaultAddress;
            VisualizerSettings.Default.LogUserFileName = LogProssesor.UserFileName;
            VisualizerSettings.Default.LogUserLogAddress = LogProssesor.UserLogAddress;

            VisualizerSettings.Default.Save();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {


            DataSender.CurrentWrapper.RequestTable.Add("Customvariables", true);
            DataSender.SendOn.Set();

        }

        void TuneVariables_Refreshed(object sender)
        {
            Dispatcher.Invoke((Action)(() => refreshCustomVar()));
        }

        void refreshCustomVar()
        {
            Dictionary<string, double> d = TuneVariables.Default.Doubles;
            List<DoubleTableFormat> doubledata = new List<DoubleTableFormat>();
            List<PositionTableFormat> positiondata = new List<PositionTableFormat>();
            List<BoolTableFormat> booldata = new List<BoolTableFormat>();
            foreach (var item in TuneVariables.Default.Doubles)
            {
                DoubleTableFormat t = new DoubleTableFormat();
                t.Value.Text = item.Value.ToString();
                t.Name = item.Key;
                doubledata.Add(t);
            }
            foreach (var item in TuneVariables.Default.Position2Ds)
            {
                PositionTableFormat p = new PositionTableFormat();
                p.X.Text = item.Value.X.ToString("f2");
                p.Y.Text = item.Value.Y.ToString("f2");
                p.Name = item.Key;
                positiondata.Add(p);
            }
            foreach (var item in TuneVariables.Default.Booleans)
            {
                BoolTableFormat b = new BoolTableFormat();
                b.Value.IsChecked = item.Value;
                b.Name = item.Key;
                booldata.Add(b);
            }
            position2dListView.ItemsSource = positiondata;
            doublecustomVarListView.ItemsSource = doubledata;
            boolListView.ItemsSource = booldata;
            TuneVariables.Default.Save();
        }

        private void doubleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoubleTableFormat t = doublecustomVarListView.SelectedItem.As<DoubleTableFormat>();
            t.Value.Text = e.OriginalSource.As<TextBox>().Text;
            double result;
            if (double.TryParse(t.Value.Text, out result))
            {
                TuneVariables.Default.Doubles[t.Name] = result;
                TuneVariables.Default.Save();
                DataSender.CurrentWrapper.SendData.Add("CustomVariable");
                DataSender.SendOn.Set();
            }
        }

        private void doubleTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                doublecustomVarListView.SelectedItem = null;
                return;
            }

            doublecustomVarListView.SelectedItem = doublecustomVarListView.ItemContainerGenerator.ItemFromContainer(dep);
        }

        private void xTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PositionTableFormat t = position2dListView.SelectedItem.As<PositionTableFormat>();
            t.X.Text = e.OriginalSource.As<TextBox>().Text;
            double x;
            if (double.TryParse(t.X.Text, out x))
            {
                TuneVariables.Default.Position2Ds[t.Name] = new Position2D(x, TuneVariables.Default.Position2Ds[t.Name].Y);
                TuneVariables.Default.Save();
                DataSender.CurrentWrapper.SendData.Add("CustomVariable");
                DataSender.SendOn.Set();
            }
        }

        private void yTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PositionTableFormat t = position2dListView.SelectedItem.As<PositionTableFormat>();
            t.Y.Text = e.OriginalSource.As<TextBox>().Text;
            double y;
            if (double.TryParse(t.Y.Text, out y))
            {
                TuneVariables.Default.Position2Ds[t.Name] = new Position2D(TuneVariables.Default.Position2Ds[t.Name].X, y);
                TuneVariables.Default.Save();
                DataSender.CurrentWrapper.SendData.Add("CustomVariable");
                DataSender.SendOn.Set();
            }
        }

        private void xTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                position2dListView.SelectedItem = null;
                return;
            }
            position2dListView.SelectedItem = position2dListView.ItemContainerGenerator.ItemFromContainer(dep);
        }

        private void yTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                position2dListView.SelectedItem = null;
                return;
            }
            position2dListView.SelectedItem = position2dListView.ItemContainerGenerator.ItemFromContainer(dep);
        }

        private void maxframTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();
        }

        MergerAndTrackerSetting Packet()
        {
            try
            {
                MergerAndTrackerSetting ret = new MergerAndTrackerSetting()
                {
                    ActionDelay = actiondelayTextBox.Text.ToDouble(),
                    CalculateRegion = (calcregionCheckBox.IsChecked.HasValue && calcregionCheckBox.IsChecked.Value) ? true : false,
                    MaxBallDist = maxballDistTextBox.Text.ToDouble(),
                    MaxFrameToShadow = maxframTextBox.Text.ToInt(),
                    MaxNotSeen = maxnotseenTextBox.Text.ToInt(),
                    MaxOpponenetDistance = maxoppdisTextBox.Text.ToDouble(),
                    MaxToImagine = maxtoimagineTextBox.Text.ToInt(),
                    OnGame = (ongameCheckBox.IsChecked.HasValue && ongameCheckBox.IsChecked.Value) ? true : false,
                    CorrectAngleError = (correctangleCheckBox.IsChecked.HasValue && correctangleCheckBox.IsChecked.Value) ? true : false,
                };
                if (cameraComboBox.SelectedIndex == 0)
                    ret.CamState = MergerAndTrackerSetting.CameraState.All;
                else if (cameraComboBox.SelectedIndex == 1)
                    ret.CamState = MergerAndTrackerSetting.CameraState.Cam0;
                else
                    ret.CamState = MergerAndTrackerSetting.CameraState.Cam1;
                return ret;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                return new MergerAndTrackerSetting();
            }

        }

        private void cameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();

        }

        private void calcregionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();
        }

        private void ongameCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();
        }

        private void ongameCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();

        }

        private void calcregionCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();
        }

        private void startLog_Click(object sender, RoutedEventArgs e)
        {
            //if (e.As<MouseEventArgs>().LeftButton == MouseButtonState.Pressed)
            //{
            if (!DataReciever.LoggerIsRun)
            {
                LogProssesor.UseDefaulName = true;
                DataReciever.LoggerIsRun = true;
                LogProssesor.StartLog();
            }
            else
            {
                LogProssesor.Abort();
                logImage.Visibility = Visibility.Visible;
                DataReciever.LoggerIsRun = false;
                LogProssesor.StopAndSave();
            }
            //}
            //else
            //{
            //    if (!DataReciever.LoggerIsRun)
            //    {
            //        GetLogNameWindow w = new GetLogNameWindow();
            //        w.ShowDialog();
            //        if (w.Ok)
            //        {
            //            LogProssesor.UseDefaulName = false;
            //            LogProssesor.UserFileName = w.Name;
            //            LogProssesor.MomentName = true;
            //        }
            //        DataReciever.LoggerIsRun = true;
            //        LogProssesor.StartLog();
            //    }
            //    else
            //    {
            //        LogProssesor.Abort();
            //        logImage.Visibility = Visibility.Visible;
            //        DataReciever.LoggerIsRun = false;
            //        LogProssesor.StopAndSave();
            //    }
            //}
        }

        private void logPlayer_Click(object sender, RoutedEventArgs e)
        {
            FieldWindow w = new FieldWindow();
            w.ShowAsSingleTab(fieldTabControl2, "logplayer");
        }

        private void selectballButtun_Click(object sender, RoutedEventArgs e)
        {
            if (selectBallMode)
            {
                Field.ContextMenuStrip = Field.mainMenuStrip;
                selectBallMode = false;
                selectballImage.Opacity = 0.5f;
            }
            else
            {
                Field.ContextMenuStrip = null;
                selectBallMode = true;
                selectballImage.Opacity = 1f;
            }
        }

        void Field_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (selectBallMode)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        Position2D mouseLoc = PixelToMetric(e.Location);
                        int? selected = SelectedBall(mouseLoc);
                        if (selected.HasValue)
                        {
                            DataSender.CurrentWrapper.SendData.Add("BallIndex");
                            DataSender.CurrentWrapper.SelectedBallIndex = selected;
                            DataSender.CurrentWrapper.SelectedBallLoc = mouseLoc;
                            DataSender.SendOn.Set();
                        }
                    }
                }
                else if (DataReciever.CurrentWrapper.Model != null && DataReciever.CurrentWrapper.Model.Status == GameStatus.MoveRobot)
                {
                    ObjectType ot;
                    int? select = SelectedObject(e.Location.ToPosition(Field.Transform), out ot);
                    if (ot == ObjectType.OurRobot && select.HasValue)
                    {
                        RobotComponentsController.SelectedID = select.Value;
                        RobotComponentsController.Target = DataReciever.CurrentWrapper.Model.OurRobots[select.Value].Location;
                        RobotComponentsController.Angle = DataReciever.CurrentWrapper.Model.OurRobots[select.Value].Angle.Value;
                    }
                    else if (RobotComponentsController.SelectedID.HasValue)
                    {
                        if (DataReciever.CurrentWrapper.Model.OurRobots.ContainsKey(RobotComponentsController.SelectedID.Value))
                        {
                            RobotComponentsController.Target = e.Location.ToPosition(Field.Transform);
                            RobotComponentsController.Angle = (e.Location.ToPosition(Field.Transform) - DataReciever.CurrentWrapper.Model.OurRobots[RobotComponentsController.SelectedID.Value].Location).AngleInDegrees;
                        }
                    }
                    DataSender.CurrentWrapper.SendData.Add("MoveRobot");
                    DataSender.SendOn.Set();

                }

                else if (position2dListView.SelectedItem != null)
                {
                    PositionTableFormat p = position2dListView.SelectedItem.As<PositionTableFormat>();
                    Position2D mouseLoc = PixelToMetric(e.Location);
                    p.X.Text = mouseLoc.X.ToString();
                    p.Y.Text = mouseLoc.Y.ToString();
                    double x;
                    double y;
                    if (double.TryParse(p.X.Text, out x) && double.TryParse(p.Y.Text, out y))
                    {
                        TuneVariables.Default.Position2Ds[p.Name] = new Position2D(Math.Round(x, 2), Math.Round(y, 2));
                        TuneVariables.Default.Save();
                        DataSender.CurrentWrapper.SendData.Add("CustomVariable");
                        DataSender.SendOn.Set();
                    }
                }
                if (refBoxWindow.ballPlacementBlue)
                {
                    Position2D mouseLoc = PixelToMetric(e.Location);
                    StaticVariables.ballPlacementPos = new Position2D(mouseLoc.X, mouseLoc.Y);
                    DataSender.CurrentWrapper.SendData.Add("ballPlacementPos");
                    if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                        DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
                    DataSender.CurrentWrapper.RefreeCommand = "B";
                    DataSender.SendOn.Set();
                    refBoxWindow.ballPlacementBlue = false;

                }
                else if (refBoxWindow.ballPlacementYellow)
                {
                    Position2D mouseLoc = PixelToMetric(e.Location);
                    StaticVariables.ballPlacementPos = new Position2D(mouseLoc.X, mouseLoc.Y);
                    DataSender.CurrentWrapper.SendData.Add("ballPlacementPos");
                    if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                        DataSender.CurrentWrapper.SendData.Add("RefreeCommand");

                    DataSender.CurrentWrapper.RefreeCommand = "b";
                    DataSender.SendOn.Set();
                    refBoxWindow.ballPlacementYellow = false;
                }

            }

        }

        int? SelectedBall(Position2D p)
        {
            int? ret = null;
            if (DataReciever.CurrentWrapper.AllBalls != null)
            {
                foreach (int key in DataReciever.CurrentWrapper.AllBalls.Keys)
                {
                    Position2D loc = DataReciever.CurrentWrapper.AllBalls[key];
                    if (Math.Pow(p.X - loc.X, 2) + Math.Pow(p.Y - loc.Y, 2) < Math.Pow(GameParameters.BallDiameter / 2, 2))
                        ret = key;
                }
            }
            return ret;
        }

        int? SelectedObject(Position2D p, out ObjectType type)
        {
            int? ret = null;
            type = ObjectType.Ball;
            if (Field.Model == null)
                return ret;
            if (Field.Model.BallState != null)
            {
                Position2D loc = Field.Model.BallState.Location;
                if (Math.Pow(p.X - loc.X, 2) + Math.Pow(p.Y - loc.Y, 2) < Math.Pow(GameParameters.BallDiameter / 2, 2))
                {
                    ret = 0;
                    type = ObjectType.Ball;
                    return ret;
                }
            }
            if (Field.Model.OurRobots != null)
            {
                foreach (var item in Field.Model.OurRobots.Keys)
                {
                    Position2D loc = Field.Model.OurRobots[item].Location;
                    if (Math.Pow(p.X - loc.X, 2) + Math.Pow(p.Y - loc.Y, 2) < Math.Pow(RobotParameters.OurRobotParams.Diameter / 2, 2))
                    {
                        ret = item;
                        type = ObjectType.OurRobot;
                        return ret;
                    }
                }
            }
            if (Field.Model.Opponents != null)
            {
                foreach (var item in Field.Model.Opponents.Keys)
                {
                    Position2D loc = Field.Model.Opponents[item].Location;
                    if (Math.Pow(p.X - loc.X, 2) + Math.Pow(p.Y - loc.Y, 2) < Math.Pow(RobotParameters.OpponentParams.Diameter / 2, 2))
                    {
                        ret = item;
                        type = ObjectType.Opponent;
                        return ret;
                    }
                }
            }
            return ret;
        }

        private Position2D PixelToMetric(System.Drawing.Point MouseLocation)
        {
            if (Field.Transform == null) return new Position2D(); ;
            double X = MouseLocation.X;
            double Y = MouseLocation.Y;
            //if (ShowMode == FieldOrientation.Verticaly)
            {
                X = (X - Field.Transform.Value.M31) / Field.Transform.Value.M21;
                Y = (Y - Field.Transform.Value.M32) / Field.Transform.Value.M12;
                return new Position2D(Y, X);
            }
            //else
            //{
            //X = (X - Field.Transform.Value.M31) / Field.Transform.Value.M11;
            //Y = (Y - Field.Transform.Value.M32) / Field.Transform.Value.M22;
            //return new Position2D(X, Y);
            //}

        }

        private void analizePanelButton_Click(object sender, RoutedEventArgs e)
        {
            new AnalizerWindow().ShowAsSingleTab(fieldTabControl2, "analize");
        }

        private void robotstatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainTabControl.Visibility != Visibility.Visible)
                mainTabControl.Visibility = Visibility.Visible;
            new RobotStatusWindow().ShowAsSingleTab(mainTabControl, "robotstatus");
        }

        bool addengine = false;

        private void addengineMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (engineListView.Items.Count == 0) return;
            addengine = true;
            int id = engineListView.Items.Cast<Engines>().ToList().Last().Id + 1;
            List<Engines> list = engineListView.Items.Cast<Engines>().ToList();
            list.Add(new Engines(id, false, false));
            GameSettings.Default.Engines.Add(id, new Engines(id, false, false));
            engineListView.ItemsSource = list;
            DataSender.CurrentWrapper.SendData.Add("Engines");
            CreatEngines();
            DataSender.CurrentWrapper.Engine = GameSettings.Default.Engines;
            DataSender.SendOn.Set();
            GameSettings.Default.Save();
            addengine = false;
        }

        private void deleteengineMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (engineListView.Items.Count < 2) return;
            if (engineListView.SelectedItem == null) return;
            addengine = true;
            List<Engines> list = engineListView.Items.Cast<Engines>().ToList();
            Engines en = engineListView.SelectedItem.As<Engines>();
            GameSettings.Default.Engines.Remove(en.Id);
            list.Remove(engineListView.SelectedItem.As<Engines>());

            engineListView.ItemsSource = list;
            DataSender.CurrentWrapper.SendData.Add("Engines");
            CreatEngines();
            DataSender.CurrentWrapper.Engine = GameSettings.Default.Engines;
            DataSender.SendOn.Set();
            GameSettings.Default.Save();
            addengine = false;
        }

        private void refreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.RequestTable.Add("Engines", true);
            DataSender.SendOn.Set();
        }

        private void enginecolorCheckBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                engineListView.SelectedItem = null;
                return;
            }
            engineListView.SelectedItem = engineListView.ItemContainerGenerator.ItemFromContainer(dep);
        }

        private void enginecolorCheckBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!addengine)
            {
                DataSender.CurrentWrapper.SendData.Add("Engines");
                CreatEngines();
                DataSender.CurrentWrapper.Engine = GameSettings.Default.Engines;
                DataSender.SendOn.Set();
                GameSettings.Default.Save();
            }

        }

        private void reverseengineCheckBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                engineListView.SelectedItem = null;
                return;
            }
            engineListView.SelectedItem = engineListView.ItemContainerGenerator.ItemFromContainer(dep);
        }

        private void reverseengineCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!addengine)
            {
                DataSender.CurrentWrapper.SendData.Add("Engines");
                CreatEngines();
                DataSender.CurrentWrapper.Engine = GameSettings.Default.Engines;
                DataSender.SendOn.Set();
                GameSettings.Default.Save();
            }
        }

        private void reverseengineCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!addengine)
            {
                DataSender.CurrentWrapper.SendData.Add("Engines");
                CreatEngines();
                DataSender.CurrentWrapper.Engine = GameSettings.Default.Engines;
                DataSender.SendOn.Set();
                GameSettings.Default.Save();
            }
        }

        Dictionary<int, Engines> CreatEngines()
        {
            List<Engines> list = engineListView.Items.Cast<Engines>().ToList();
            Dictionary<int, Engines> ret = new Dictionary<int, Engines>();
            if (GameSettings.Default.Engines == null)
                GameSettings.Default.Engines = new SerializableDictionary<int, Engines>();
            foreach (var item in list)
            {
                if (!GameSettings.Default.Engines.ContainsKey(item.Id))
                    GameSettings.Default.Engines.Add(item.Id, item);
                else
                    GameSettings.Default.Engines[item.Id] = item;
                ret.Add(item.Id, item);
            }
            return ret;

        }

        private void debugButton_Click(object sender, RoutedEventArgs e)
        {
            //new DebugModeWindow().ShowAsSingleTab(fieldTabControl, "debug");
        }

        private void changMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (objectTreeView.SelectedItem == null) return;
            ChangeObjectPenWindow w = new ChangeObjectPenWindow();
            if (DrawingObjects.drawingObject.ContainsKey(objectTreeView.SelectedItem.As<TreeViewModel>().Name))
            {
                object obj = DrawingObjects.drawingObject[objectTreeView.SelectedItem.As<TreeViewModel>().Name];
                if (obj.GetType() == typeof(DrawCollection))
                    return;
                else
                {
                    if (obj.GetType() == typeof(Line))
                    {
                        w.pen = obj.As<Line>().DrawPen;
                        w.ShowDialog();
                        // DrawingObjects.drawingObject[objectTreeView.SelectedItem.As<MRL.SSL.CommonClasses.Extentions.TreeViewModel>().Name].As<Line>().DrawPen = w.pen;
                    }
                    else if (obj.GetType() == typeof(Circle))
                    {
                        w.pen = obj.As<Circle>().DrawPen;
                        w.ShowDialog();
                    }

                }
            }
        }

        private void showOppRobotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.DrawOppRobot = true;
        }

        private void showOppRobotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.DrawOppRobot = false;
        }

        private void showTextCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.ShowTexts = true;
        }

        private void showTextCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.ShowTexts = false;
        }

        private void showOurRobotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.DrawOurRobot = true;
        }

        private void showOurRobotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.DrawOurRobot = false;
        }

        private void consoleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void consoleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            //if (mainConsol != null)
            //    mainConsol.Visibility = Visibility.Collapsed;
            // conSplit.Visibility = Visibility.Collapsed;
        }

        private void consoleButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (mainConsol != null)
            //    mainConsol.Visibility = Visibility.Visible;
            //conSplit.Visibility = Visibility.Visible;
        }

        private void wheeldataButton_Click(object sender, RoutedEventArgs e)
        {
            //new WheelDataChartWindow().ShowAsSingleTab(fieldTabControl, "wheelchart");
        }

        private void wirelessrecieveButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.Ports.SerialPort.GetPortNames().Contains(WirelessReciever.ProtName))
            {
                if (WirelessReciever.State == WirelessReciever.WirelessRecieveState.OFF)
                {
                    wirelessrecieveButton.Content = "Recieve OFF";
                    WirelessReciever.ChangeState(WirelessReciever.WirelessRecieveState.ON);
                }
                else
                {
                    wirelessrecieveButton.Content = "Recieve ON";
                    WirelessReciever.ChangeState(WirelessReciever.WirelessRecieveState.OFF);
                }
            }
        }

        private void loganalizerButtun_Click(object sender, RoutedEventArgs e)
        {
            new LogAnalizerWindow().ShowAsSingleTab(fieldTabControl, "loganalizer");
        }

        private void wirelessswitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (WS.SendFlag)
            {
                //switchImage.Opacity = 0.5;
                WS.SendFlag = false;
                DataSender.CurrentWrapper.SendData.Add("SenderDevice");
                DataSender.CurrentWrapper.SenderDevice = WirelessSenderDevice.AI;
                DataSender.SendOn.Set();
            }
            else
            {
                //switchImage.Opacity = 1;
                WS.SendFlag = true;
                DataSender.CurrentWrapper.SendData.Add("SenderDevice");
                DataSender.CurrentWrapper.SenderDevice = WirelessSenderDevice.Visualizer;
                DataSender.SendOn.Set();
            }

        }

        private void visionMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Field.ContextMenuStrip = null;
            DataSender.CurrentWrapper.RecieveMode = ModelRecieveMode.SSLVision;
            DataSender.CurrentWrapper.SendData.Add("RecieveMode");
            DataSender.SendOn.Set();
            recievemodeImage.SetImageSource("visionmode.png");
            changreciveModeButton.IsOpen = false;
        }

        private void logviwerMwnuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Field.ContextMenuStrip = null;
            DataSender.CurrentWrapper.RecieveMode = ModelRecieveMode.Visualizer;
            DataSender.CurrentWrapper.SendData.Add("RecieveMode");
            DataSender.SendOn.Set();
            recievemodeImage.SetImageSource("logviwermode.png");
            changreciveModeButton.IsOpen = false;
        }

        private void analizerMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Field.ContextMenuStrip = null;
            DataSender.CurrentWrapper.RecieveMode = ModelRecieveMode.Analizer;
            DataSender.CurrentWrapper.SendData.Add("RecieveMode");
            DataSender.SendOn.Set();
            recievemodeImage.SetImageSource("analizermdoe.png");
            changreciveModeButton.IsOpen = false;
        }

        private void whirelesssettingButton_Click(object sender, RoutedEventArgs e)
        {
            //new WirelessSettingWindow().ShowAsSingleTab(fieldTabControl, "wirelesssetting");
        }

        private void simulatorMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Field.ContextMenuStrip = Field.mainMenuStrip;
            DataSender.CurrentWrapper.RecieveMode = ModelRecieveMode.Simulator;
            DataSender.CurrentWrapper.SendData.Add("RecieveMode");
            DataSender.SendOn.Set();
            recievemodeImage.SetImageSource("simulatormode.png");
            changreciveModeButton.IsOpen = false;
            DrawingObjects.drawingObject.Clear();
            DrawingObjects.ObjectTree.Clear();
        }

        private void visionButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataReciever.ShowAIData)
            {
                visionImage.Opacity = 1f;
                DataReciever.ShowAIData = false;
            }
            else
            {
                visionImage.Opacity = 0.5f;
                DataReciever.ShowAIData = true;
            }
        }

        MRLSimpleSimulator simulator;

        bool simIsInit = false;

        private void simstateButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!simIsInit)
            //{

            //    simulator = new MRLSimulator();

            //    simulator.AiName = AISettings.Default.SimulatorName;
            //    simulator.RecievePort = AISettings.Default.SimulatorRecievePort;
            //    simulator.SendPort = AISettings.Default.SimulatorSendPort;
            //    simulator.ConnectToAi();
            //    simImage.Opacity = 1;
            //    simIsInit = true;
            //    if (Field.Model != null)
            //    {
            //        if (Field.Model.OurRobots != null)
            //            Field.Model.OurRobots.Clear();
            //        if (Field.Model.Opponents != null)
            //            Field.Model.Opponents.Clear();
            //    }
            //    DrawingObjects.ObjectTree.Clear();
            //    DrawingObjects.drawingObject.Clear();
            //    Field.Invalidate();
            //    simstateLabel.Content = "Visualizer Connected to Simulator";
            //}
            //else
            //{
            //    //simulator.
            //    simImage.Opacity = 0.6f;
            //    simulator.Dispose();
            //    simIsInit = false;
            //    simstateLabel.Content = "Visualizer Disconnected From Simulator";
            //}
        }

        private void simstateButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void simstateButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!simIsInit)
            {

                simulator = new MRLSimpleSimulator();

                simulator.AiName = AISettings.Default.SimulatorName;
                simulator.RecievePort = AISettings.Default.SimulatorRecievePort;
                simulator.SendPort = AISettings.Default.SimulatorSendPort;
                simulator.ConnectToAi();
                simImage.Opacity = 1;
                simIsInit = true;
                if (Field.Model != null)
                {
                    if (Field.Model.OurRobots != null)
                        Field.Model.OurRobots.Clear();
                    if (Field.Model.Opponents != null)
                        Field.Model.Opponents.Clear();
                }
                DrawingObjects.ObjectTree.Clear();
                DrawingObjects.drawingObject.Clear();
                Field.Invalidate();
                simstateLabel.Content = "Visualizer Connected to Simulator";
            }
            else
            {
                //simulator.
                simImage.Opacity = 0.6f;
                simulator.Dispose();
                simIsInit = false;
                simstateLabel.Content = "Visualizer Disconnected From Simulator";
            }
        }

        private void getparametersButton_Click(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.RequestTable.Add("GameParameters", true);
            DataSender.SendOn.Set();

        }

        private void controldataButton_Click(object sender, RoutedEventArgs e)
        {
            new ControlChartWindow().ShowAsSingleTab(fieldTabControl, "controlchart");
        }

        private void lookuprefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //if (LookUpTable.Default.DirectKick == null) return;
            //List<DirectKickTemplat> DK = new List<DirectKickTemplat>();
            //LookUpTable.Default.DirectKick.ToList().ForEach(pg => DK.Add(new DirectKickTemplat(int.Parse(pg.Key.ToString().Split(new char[] { '@' })[1]), pg.Value, int.Parse(pg.Key.ToString().Split(new char[] { '@' })[0]))));
            //directListView.ItemsSource = DK.ToList();

            if (LookupTable.Data == null) return;
            List<ChipKickTemplate> ck = new List<ChipKickTemplate>();
            LookupTable.Data.ToList().ForEach(p => p.Value.KickInfo.ToList().ForEach(f => ck.Add(new ChipKickTemplate() { RobotID = p.Key, Lenght = f.Length, Power = f.Power, SafeRadi = f.SafeRadi, Time = f.Time, BackSensore = f.BackSensore, HasSpin = f.HasSpin })));
            chikickListView.ItemsSource = ck.ToList();
            DataSender.CurrentWrapper.SendData.Add("LockupTable");
            DataSender.SendOn.Set();
        }

        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (directListView.SelectedItem == null) return;
            if (LookUpTable.Default.DirectKick.ContainsKey(directListView.SelectedItem.As<DirectKickTemplat>().Key))
            {
                LookUpTable.Default.DirectKick.Remove(directListView.SelectedItem.As<DirectKickTemplat>().Key);
                LookUpTable.Default.Save();
                List<DirectKickTemplat> DK = new List<DirectKickTemplat>();
                LookUpTable.Default.DirectKick.ToList().ForEach(pg => DK.Add(new DirectKickTemplat(int.Parse(pg.Key.ToString().Split(new char[] { '@' })[1]), pg.Value, int.Parse(pg.Key.ToString().Split(new char[] { '@' })[0]))));
                directListView.ItemsSource = DK.ToList();
                DataSender.CurrentWrapper.SendData.Add("LockupTable");
                DataSender.SendOn.Set();
            }

        }

        private void addMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddDirectkickWindow adw = new AddDirectkickWindow();
            adw.ShowDialog();
            if (!adw.MustBeAdd) return;
            string p = adw.RobotID.ToString() + "@" + adw.Power.ToString();
            double speed = adw.Speed;
            if (LookUpTable.Default.DirectKick == null)
                LookUpTable.Default.DirectKick = new SerializableDictionary<string, double>();
            LookUpTable.Default.DirectKick[p] = speed;
            LookUpTable.Default.Save();
            List<DirectKickTemplat> DK = new List<DirectKickTemplat>();
            LookUpTable.Default.DirectKick.ToList().ForEach(pg => DK.Add(new DirectKickTemplat(int.Parse(pg.Key.ToString().Split(new char[] { '@' })[1]), pg.Value, int.Parse(pg.Key.ToString().Split(new char[] { '@' })[0]))));
            directListView.ItemsSource = DK.ToList();
            DataSender.CurrentWrapper.SendData.Add("LockupTable");
            DataSender.SendOn.Set();
        }

        void RefreshLookup()
        {
            if (LookUpTable.Default.DirectKick != null)
            {
                List<DirectKickTemplat> DK = new List<DirectKickTemplat>();
                LookUpTable.Default.DirectKick.ToList().ForEach(pg => DK.Add(new DirectKickTemplat(int.Parse(pg.Key.ToString().Split(new char[] { '@' })[1]), pg.Value, int.Parse(pg.Key.ToString().Split(new char[] { '@' })[0]))));
                directListView.ItemsSource = DK.ToList();
            }

            if (LookupTable.Data != null)
            {

                List<ChipKickTemplate> ck = new List<ChipKickTemplate>();
                LookupTable.Data.ToList().ForEach(p => p.Value.KickInfo.ToList().ForEach(f => ck.Add(new ChipKickTemplate() { RobotID = p.Key, Lenght = f.Length, Power = f.Power, SafeRadi = f.SafeRadi, HasSpin = f.HasSpin, Time = f.Time, BackSensore = f.BackSensore })));
                chikickListView.ItemsSource = ck.ToList();
                DataSender.CurrentWrapper.SendData.Add("LockupTable");
                DataSender.SendOn.Set();
            }
        }

        private void refreshbatteryButton_Click(object sender, RoutedEventArgs e)
        {
            //batteryMustRefreshed = true;

        }

        private void portComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            WirelessReciever.ProtName = portComboBox.SelectedItem.ToString();

        }

        private void motorcurrentButton_Click(object sender, RoutedEventArgs e)
        {
            new MotorCurrentChartWindow().ShowAsSingleTab(fieldTabControl, "current");
        }

        private void robottoengineMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (robotListView.SelectedItem == null || engineListView.SelectedItem == null) return;
            Robots selected = robotListView.SelectedItem.As<Robots>();
            currentRobots.Single(s => s.EngineId == selected.EngineId && s.Id == selected.Id && s.TeamColor == selected.TeamColor).EngineId = engineListView.SelectedItem.As<Engines>().Id;
            robotListView.ItemsSource = currentRobots.ToList();
            sendEngineInfo();
        }

        private void sendEngineInfo()
        {
            //List<Robots> rlist = robotListView.Items.Cast<Robots>().ToList();
            CreatEngines();
            DataSender.CurrentWrapper.SendData.Add("Engines");
            DataSender.CurrentWrapper.SendData.Add("Robots");
            DataSender.CurrentWrapper.RobotList = currentRobots;
            DataSender.CurrentWrapper.Engine = GameSettings.Default.Engines;
            DataSender.SendOn.Set();
        }

        private void valueCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool t = boolListView.SelectedItem.As<BoolTableFormat>().Value.IsChecked.Value;
            string name = boolListView.SelectedItem.As<BoolTableFormat>().Name;
            TuneVariables.Default.Booleans[name] = t;
            TuneVariables.Default.Save();
            DataSender.CurrentWrapper.SendData.Add("CustomVariable");
            DataSender.SendOn.Set();
        }

        private void valueCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            bool t = boolListView.SelectedItem.As<BoolTableFormat>().Value.IsChecked.Value;
            string name = boolListView.SelectedItem.As<BoolTableFormat>().Name;
            TuneVariables.Default.Booleans[name] = t;
            TuneVariables.Default.Save();
            DataSender.CurrentWrapper.SendData.Add("CustomVariable");
            DataSender.SendOn.Set();
        }

        private void valueCheckBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                boolListView.SelectedItem = null;
                return;
            }
            boolListView.SelectedItem = boolListView.ItemContainerGenerator.ItemFromContainer(dep);
        }

        private void chipkicktuningButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainTabControl.Visibility != Visibility.Visible)
                mainTabControl.Visibility = Visibility.Visible;
            //new AutoCreatLookupWindow().ShowAsSingleTab(mainTabControl, "autocal");
        }

        private void joystickButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainTabControl.Visibility != Visibility.Visible)
                mainTabControl.Visibility = Visibility.Visible;
            JoystickWindow joyStick = new JoystickWindow();
            joyStick.ShowAsSingleTab(mainTabControl, "joy");
            joyStick.RaiseCustomEvent += new RoutedEventHandler(this.startLog_Click);

        }

        bool CalculateKickVectors = false;

        private void kickVectorButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CalculateKickVectors = false;
            int robotID = 3;
            WorldModel Model = DataReciever.CurrentWrapper.Model;
            if (v1s.Count > 0 && v0s.Count > 0 && v1s.Count == v0s.Count && Model.OurRobots.ContainsKey(robotID))
            {
                Vector2D rh = Vector2D.FromAngleSize(Model.OurRobots[robotID].Angle.Value * Math.PI / 180, 1);
                Vector2D rp = Vector2D.FromAngleSize(rh.AngleInRadians - Math.PI / 2, 1);
                // double alfa = DataReciever.CurrentWrapper.Model.OurRobots[robotID].Angle.Value - 90;
                MathMatrix A = new MathMatrix(v0s.Count * 2, 2);
                MathMatrix B = new MathMatrix(2, 1);
                MathMatrix C = new MathMatrix(v0s.Count * 2, 1);
                for (int i = 0; i < (v0s.Count * 2) - 1; i += 2)
                {
                    int index = (int)(i / 2);
                    A[i, 0] = Math.Pow(rp.X, 2) * v0s[index].X + rp.X * rp.Y * v0s[index].Y;
                    A[i, 1] = v0s[index].X - 2 * (v0s[index].X * Math.Pow(rh.X, 2) + rh.Y * rh.X * v0s[index].Y);
                    A[i + 1, 0] = Math.Pow(rp.Y, 2) * v0s[index].Y + rp.X * rp.Y * v0s[index].X;
                    A[i + 1, 1] = v0s[index].Y - 2 * (v0s[index].Y * Math.Pow(rh.Y, 2) + rh.X * rh.Y * v0s[index].X);
                    C[i, 0] = v1s[index].X;
                    C[i + 1, 0] = v1s[index].Y;
                }
                B = ((A.Transpose * A).Inverse * A.Transpose) * C;
            }
            v1s = new List<Vector2D>();
            v0s = new List<Vector2D>();
        }

        private void kickVectorButton_Checked(object sender, RoutedEventArgs e)
        {
            CalculateKickVectors = true;
        }


        Vector2D maxSpeed = Vector2D.Zero, maxSpeedNorm = Vector2D.Zero;
        Dictionary<double, Vector2D> BallVec = new Dictionary<double, Vector2D>();
        Dictionary<double, Vector2D> BallVecNorm = new Dictionary<double, Vector2D>();
        bool isFirst = true;
        Position2D lastBall = new Position2D();

        List<Vector2D> v0s = new List<Vector2D>();
        List<Vector2D> v1s = new List<Vector2D>();
        int gotSpeed = 0;
        void DataReciever_CalcKickVector(object sender)
        {

            WorldModel model = DataReciever.CurrentWrapper.Model;
            //Vector2D v11 = Vector2D.Zero;
            //Vector2D rp = new Vector2D(0.401939192917693, 0.915666361289238);
            //Vector2D rh = new Vector2D(-0.915666361289238, 0.401939192917693);
            //maxSpeed = new Vector2D(1.2332411534551, 0.607928158365133);
            //double beta = 0;
            //double l = 0.5;
            //v11 = rp.GetNormalizeToCopy(beta * rp.InnerProduct(maxSpeed)) + (maxSpeed - rh.GetNormalizeToCopy(2 * rh.InnerProduct(maxSpeed))).GetNormalizeToCopy(l);

            //#region

            int robotID = 3;
            if (!model.OurRobots.ContainsKey(robotID)) return;
            Vector2D rh = Vector2D.FromAngleSize(model.OurRobots[robotID].Angle.Value * Math.PI / 180, 1);
            Vector2D rp = Vector2D.FromAngleSize(rh.AngleInRadians - Math.PI / 2, 1);
            if (CalculateKickVectors && model.OurRobots.ContainsKey(robotID))
            {
                if (isFirst)
                {
                    lastBall = model.BallState.Location;
                    isFirst = false;
                }

                Position2D robotHead = model.OurRobots[robotID].Location - Vector2D.FromAngleSize(model.OurRobots[robotID].Angle.Value * Math.PI / 180, RobotParameters.OurRobotParams.Diameter / 2);
                DrawingObjects.AddObject(robotHead, "head");
                double difangle = Math.Abs((robotHead - model.BallState.Location).AngleInDegrees - model.OurRobots[robotID].Angle.Value);
                CharterData.AddData("DiffAngle", difangle);
                //if (model.BallState.Speed.Size < 0.1 && BallVec.Count > 0 && BallVecNorm.Count > 0 && !asked)
                //{
                //    asked = true;
                //    if (MessageBox.Show("Do you want add new data to list?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                //        return;
                //}
                double lastballCurrent = Math.Abs(lastBall.DistanceFrom(model.OurRobots[robotID].Location) - model.BallState.Location.DistanceFrom(model.OurRobots[robotID].Location));
                double ballSpeedSize = model.BallState.Speed.Size;
                Vector2D ballSpeed = model.BallState.Speed;
                Vector2D robotBall = model.OurRobots[robotID].Location - model.BallState.Location;
                double innerp = robotBall.InnerProduct(ballSpeed);
                //  VisualizerConsole.WriteLine("InnerProduct = " + innerp.ToString(), System.Drawing.Color.White);
                if (ballSpeedSize < 0.05)
                {
                    //if (BallVec.Count > 0)
                    //{
                    //    maxSpeed = BallVec[BallVec.Max(v => v.Key)];

                    //    VisualizerConsole.WriteLine("V0 = " + maxSpeed.ToString(), System.Drawing.Color.Yellow);
                    //}
                    //if (BallVecNorm.Count > 0)
                    //{
                    //    asked = false;
                    //    maxSpeedNorm = BallVecNorm[BallVecNorm.Max(v => v.Key)];
                    //    // maxSpeedNorm.Normnalize();

                    //    VisualizerConsole.WriteLine("V1 = " + maxSpeedNorm.ToString(), System.Drawing.Color.Green);
                    //}
                    //if (BallVecNorm.Count > 0 && BallVec.Count > 0)
                    if (maxSpeed.Size > 0.3 && maxSpeedNorm.Size > 0.3)
                    {

                        gotSpeed = 0;

                        BallVecNorm = new Dictionary<double, Vector2D>();
                        BallVec = new Dictionary<double, Vector2D>();
                        if (MessageBox.Show("Do you want add new data to list?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            v1s.Add(maxSpeedNorm);
                            v0s.Add(maxSpeed);
                            Vector2D v11 = rp.GetNormalizeToCopy(0.15 * rp.InnerProduct(maxSpeed)) + (maxSpeed - rh.GetNormalizeToCopy(2 * rh.InnerProduct(maxSpeed))).GetNormalizeToCopy(0.2);
                            DrawingObjects.AddObject(new Line(new Position2D(0, 0.5), new Position2D(0, 0.5) + v11, new System.Drawing.Pen(System.Drawing.Color.Yellow, 0.03f) { EndCap = System.Drawing.Drawing2D.LineCap.Triangle }));
                            DrawingObjects.AddObject(new Line(new Position2D(0, 0.5), new Position2D(0, 0.5) + maxSpeedNorm, new System.Drawing.Pen(System.Drawing.Color.Black, 0.03f) { EndCap = System.Drawing.Drawing2D.LineCap.Triangle }));
                        }
                        maxSpeed = Vector2D.Zero;
                        maxSpeedNorm = Vector2D.Zero;
                    }


                }
                else if (innerp > 0)
                {
                    if (model.BallState.Speed.Size > 0.3)
                        maxSpeed = model.BallState.Speed;
                    //  BallVec[model.BallState.Speed.Size] = model.BallState.Speed;
                }
                else
                {
                    if (model.BallState.Speed.Size > 0.3)
                    {
                        if (gotSpeed == 10)
                        {
                            gotSpeed = 0;
                            maxSpeedNorm = model.BallState.Speed;
                        }
                        gotSpeed++;
                    }
                    //BallVecNorm[model.BallState.Speed.Size] = model.BallState.Speed;
                }

                lastBall = model.BallState.Location;


            }
            else
            {
                isFirst = true;
                maxSpeed = Vector2D.Zero; maxSpeedNorm = Vector2D.Zero;
                BallVec = new Dictionary<double, Vector2D>();
                BallVecNorm = new Dictionary<double, Vector2D>();
                lastBall = new Position2D();
                gotSpeed = 0;
            }
            //if (VisualizerConsole.Data.Count > 0)
            //{
            //    Dispatcher.Invoke((Action)(() =>
            //    {
            //        mainConsol.mainListBox.Items.Add(VisualizerConsole.Data.ToList().Last());
            //        if (mainConsol.mainListBox.Items.Count > 100)
            //            mainConsol.mainListBox.Items.RemoveAt(0);
            //        mainConsol.mainListBox.ScrollIntoView(mainConsol.mainListBox.Items[mainConsol.mainListBox.Items.Count - 1]);
            //    }));
            //}
            //#endregion
        }


        private void showrobottailTextCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.RobotTail = true;
            LogProssesor.ShowRobotTail = true;
        }

        private void showrobottailTextCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.RobotTail = false;
            LogProssesor.ShowRobotTail = false;
        }

        private void showrobotangleTextCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.ShowRobotAngle = true;
            LogProssesor.ShowRobotAng = true;
        }

        private void showrobotangleTextCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.ShowRobotAngle = false;
            LogProssesor.ShowRobotAng = false;
        }

        private void showballTailCheckBox_Checked(object sender, RoutedEventArgs e)
        {

            if (Field != null)
                Field.ShowBallTail = true;
            LogProssesor.ShowBallTail = true;
        }

        private void showballTailCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.ShowBallTail = false;
            LogProssesor.ShowBallTail = false;
        }

        private void robotTailNum_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Field != null)
                Field.RobotTailCount = (int)robotTailNum.Value;
            LogProssesor.RobotTailCount = (int)robotTailNum.Value;
        }

        private void ballTailNum_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Field != null)
                Field.BallTailCount = (int)ballTailNum.Value;
            LogProssesor.BallTailCount = (int)ballTailNum.Value;
        }

        private void showvectorsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.ShowSpeedVector = true;
            LogProssesor.ShowSpeedVector = true;
        }

        private void showvectorsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Field != null)
                Field.ShowSpeedVector = false;
            LogProssesor.ShowSpeedVector = false;
        }

        private void RadPanelBarItem_Expanded(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            List<DirectKickTemplat> DK = new List<DirectKickTemplat>();
            LookUpTable.Default.DirectKick.ToList().ForEach(pg => DK.Add(new DirectKickTemplat(int.Parse(pg.Key.ToString().Split(new char[] { '@' })[1]), pg.Value, int.Parse(pg.Key.ToString().Split(new char[] { '@' })[0]))));
            directListView.ItemsSource = DK.ToList();
        }

        private void setLookuptableButton_Click(object sender, RoutedEventArgs e)
        {
            //if (mainTabControl.Visibility != Visibility.Visible)
            //    mainTabControl.Visibility = Visibility.Visible;

            //new SetLooKupToArmWindow().ShowAsSingleTab(mainTabControl, "lookupsetter");
        }

        private void calcangButton_Click(object sender, RoutedEventArgs e)
        {
            //int id;
            //if (int.TryParse(robotidTextBox.Text, out id))
            //{
            //    if (!DataReciever.CurrentWrapper.Model.OurRobots.ContainsKey(id)) return;
            //    if (TuneVariables.Default.AngleError == null)
            //        TuneVariables.Default.AngleError = new SerializableDictionary<int, double>();
            //    Vector2D robot = Vector2D.FromAngleSize(DataReciever.CurrentWrapper.Model.OurRobots[id].Angle.Value * Math.PI / 180, 1);
            //    Vector2D robotball = DataReciever.CurrentWrapper.Model.BallState.Location - DataReciever.CurrentWrapper.Model.OurRobots[id].Location;
            //    TuneVariables.Default.AngleError[id] = Vector2D.AngleBetweenInDegrees(robot, robotball);
            //    DataSender.CurrentWrapper.SendData.Add("AngleError");
            //    DataSender.SendOn.Set();
            //}
            //if (mainTabControl.Visibility != Visibility.Visible)
            //    mainTabControl.Visibility = Visibility.Visible;
            //new AngleErrorWindow().ShowAsSingleTab(mainTabControl, "errorlist"); ;
        }

        private void correctangleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();
        }

        private void correctangleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("MergerTracker");
            DataSender.CurrentWrapper.MergerTracker = Packet();
            DataSender.SendOn.Set();
        }

        private void addchipMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddChipkickWindow w = new AddChipkickWindow();
            w.ShowDialog();
            if (w.MustBeAdd)
            {
                if (LookupTable.Data == null)
                    LookupTable.Data = new SerializableDictionary<int, MRL.SSL.GameDefinitions.Visualizer_Classes.MetricChipKick>();


                if (LookupTable.Data.Any(p => p.Key == w.RobotID && p.Value.KickInfo.Any(a => a.Power == w.Power && a.HasSpin == w.HasSpin && a.BackSensore == w.BackSensore)))
                {
                    LookupTable.Data.Single(p => p.Key == w.RobotID).Value.KickInfo.Single(a => a.Power == w.Power && a.HasSpin == w.HasSpin && a.BackSensore == w.BackSensore).Length = w.Lenght;
                    LookupTable.Data.Single(p => p.Key == w.RobotID).Value.KickInfo.Single(a => a.Power == w.Power && a.HasSpin == w.HasSpin && a.BackSensore == w.BackSensore).SafeRadi = w.Safe;
                }
                else
                {
                    if (LookupTable.Data.ContainsKey(w.RobotID))
                        LookupTable.Data[w.RobotID].KickInfo.Add(new MRL.SSL.GameDefinitions.Visualizer_Classes.ChipKickInfo() { Length = w.Lenght, Power = w.Power, SafeRadi = w.Safe, HasSpin = w.HasSpin, Time = w.Time, BackSensore = w.BackSensore });
                    else
                    {
                        LookupTable.Data.Add(w.RobotID, new MRL.SSL.GameDefinitions.Visualizer_Classes.MetricChipKick());
                        LookupTable.Data[w.RobotID].KickInfo.Add(new MRL.SSL.GameDefinitions.Visualizer_Classes.ChipKickInfo() { Length = w.Lenght, Power = w.Power, SafeRadi = w.Safe, HasSpin = w.HasSpin, Time = w.Time, BackSensore = w.BackSensore });
                    }
                    LookupTable.Save();
                }

                RefreshLookup();
            }
        }

        private void delMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (chikickListView.SelectedItem == null) return;
            ChipKickTemplate ch = chikickListView.SelectedItem.As<ChipKickTemplate>();
            List<ChipKickInfo> list = LookupTable.Data.Single(p => p.Key == ch.RobotID).Value.KickInfo;
            LookupTable.Data.Single(p => p.Key == ch.RobotID).Value.KickInfo.Remove(list.Single(s => s.Power == ch.Power && s.HasSpin == ch.HasSpin && s.BackSensore == ch.BackSensore));
            RefreshLookup();
        }

        private void changerobotidButton_Click(object sender, RoutedEventArgs e)
        {
            //ChangeidWindow w = new ChangeidWindow();
            //w.ShowDialog();
            //if (w.MustBeChange)
            //{
            //    if (w.IsChipKick)
            //        LookupTable.ChipKickReplaceRobotID(w.FirstRobotID, w.SecondRobotID);
            //    RefreshLookup();
            //}
        }

        bool changeBatery = false;


        private void refreshbatteryButton_Checked(object sender, RoutedEventArgs e)
        {
            changeBatery = true;
            batteryMustRefreshed = true;
        }

        private void refreshbatteryButton_Unchecked(object sender, RoutedEventArgs e)
        {
            changeBatery = true;
            batteryMustRefreshed = false;
        }

        private void mainGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                toolBar1.Visibility = Visibility.Collapsed;
                robotcontrollerExpander.Visibility = Visibility.Collapsed;
                headerGrid.Visibility = Visibility.Collapsed;
            }
            if (e.Key == Key.F12)
            {
                toolBar1.Visibility = Visibility.Visible;
                robotcontrollerExpander.Visibility = Visibility.Visible;
                headerGrid.Visibility = Visibility.Visible;
            }
        }

        private void windowsFormsHost2_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void graberButon_Click(object sender, RoutedEventArgs e)
        {
            //new ChipkickCalibrationWindow().ShowAsSingleTab(fieldTabControl, "chipkickCal");
        }

        private void parameterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                mainParametersListView.SelectedItem = null;
                return;
            }

            mainParametersListView.SelectedItem = mainParametersListView.ItemContainerGenerator.ItemFromContainer(dep);
        }

        private void parameterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Map t = mainParametersListView.SelectedItem.As<Map>();
            string data = e.OriginalSource.As<TextBox>().Text;

            if (t.Value.GetType() == typeof(double))
            {
                double b;
                if (double.TryParse(data, out b))
                    MainGameParameters.Default[t.Name] = b;
            }
            else if (t.Value.GetType() == typeof(bool))
            {
                bool b;
                if (bool.TryParse(data, out b))
                    MainGameParameters.Default[t.Name] = b;
            }
            else if (t.Value.GetType() == typeof(string))
            {
                MainGameParameters.Default[t.Name] = data;
            }
            else if (t.Value.GetType() == typeof(int))
            {
                int b;
                if (int.TryParse(data, out b))
                    MainGameParameters.Default[t.Name] = b;
            }

            MainGameParameters.Default.Save();
        }

        private void mainrefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefereshMainParameters();
        }

        public class Map
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }
        public void RefereshMainParameters()
        {
            List<Map> ret = new List<Map>();
            foreach (SettingsProperty item in MainGameParameters.Default.Properties)
                ret.Add(new Map() { Name = item.Name, Value = MainGameParameters.Default[item.Name] });
            mainParametersListView.ItemsSource = ret.ToList();

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            bool? res = sfd.ShowDialog();
            if (res.HasValue && res.Value)
                MainGameParameters.Default.SaveToFile(sfd.SafeFileName);
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? res = ofd.ShowDialog();
            if (res.HasValue && res.Value)
                MainGameParameters.Default.LoadFromFile(ofd.SafeFileName);
        }

        private void fieldTabItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (eventsLabel.Visibility == Visibility.Collapsed)
                eventsLabel.Visibility = Visibility.Visible;
            else
                eventsLabel.Visibility = Visibility.Collapsed;
        }

        private void startLog_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void startLog_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!DataReciever.LoggerIsRun)
            {
                GetLogNameWindow w = new GetLogNameWindow();
                w.ShowDialog();
                if (w.Ok)
                {
                    LogProssesor.UseDefaulName = false;
                    LogProssesor.UserFileName = w.LogName;
                    LogProssesor.MomentName = true;
                }
                DataReciever.LoggerIsRun = true;
                LogProssesor.StartLog();
            }
            else
            {
                LogProssesor.Abort();
                logImage.Visibility = Visibility.Visible;
                DataReciever.LoggerIsRun = false;
                LogProssesor.StopAndSave();
            }
        }

        private void activesettingButton_Click(object sender, RoutedEventArgs e)
        {
            new ActiveRoleParametersWindow().ShowAsSingleTab(fieldTabControl, "act");
        }

        private void mobileRefreeBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mobileRefreeBox_Checked(object sender, RoutedEventArgs e)
        {
            deviceReaderSignal.Set();

        }

        private void mobileRefreeBox_Unchecked(object sender, RoutedEventArgs e)
        {

            deviceReaderSignal.Reset();
        }

        void getCommand()
        {
            UdpClient u = new UdpClient();
            u.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            u.Client.Bind(new IPEndPoint(IPAddress.Any, 10004));
            IPEndPoint temp = null;
            while (deviceReaderSignal.WaitOne())
            {

                var data = u.Receive(ref temp);
                char c = (char)data[0];

                if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                    DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
                DataSender.SendOn.Set();
                DataSender.CurrentWrapper.RefreeCommand = c.ToString();
                Thread.Sleep(100);
            }
        }

        private void defendingButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainTabControl.Visibility != Visibility.Visible)
                mainTabControl.Visibility = Visibility.Visible;
            new DefenderOptimusWindow().ShowAsSingleTab(mainTabControl, "Defender");
        }

        private void Scoring_Click(object sender, RoutedEventArgs e)
        {
            if (mainTabControl.Visibility != Visibility.Visible)
                mainTabControl.Visibility = Visibility.Visible;
            new ScoreWindow().ShowAsSingleTab(mainTabControl, "Scoring");
        }

        private void loadloockupButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LoadLoockup(ofd.FileName);
        }

        private void saveloockupdButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SaveLoockupTable(sfd.FileName);
        }

        public void SaveLoockupTable(string filename)
        {

            GoogleSerializer gs = new GoogleSerializer();
            MemoryStream ret = gs.SerializeLoockup();
            using (var file = new FileStream(filename + ".ltbl", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var byt = ret.ToArray();
                file.Write(byt, 0, byt.Length);
            }
        }

        public void LoadLoockup(string filename)
        {
            GoogleSerializer gs = new GoogleSerializer();
            byte[] byt = new byte[10];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                byt = new byte[(int)file.Length];
                file.Read(byt, 0, (int)file.Length);
            }

            MemoryStream ms = new MemoryStream(byt);
            ms.Position = 0;
            gs.DeserializeLoockUp(ms);
            DataSender.CurrentWrapper.SendData.Add("LockupTable");
            DataSender.SendOn.Set();

        }

        private void reflecttunningButton_Click(object sender, RoutedEventArgs e)
        {
            //new ReflectTunningWindow().ShowAsSingleTab(fieldTabControl, "reflect");
        }

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            new ControlParameteresWindow().ShowAsSingleTab(fieldTabControl, "controlparameters");
        }

        private void rotateBatton_Click(object sender, RoutedEventArgs e)
        {
            new RotateParametersWindow().ShowAsSingleTab(fieldTabControl, "RotateParameters");
        }

        private void goalieIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (goalieIDTextBox.Text == "") return;
            ControlParameters.GoalieID = int.Parse(goalieIDTextBox.Text);
            DataSender.CurrentWrapper.GoalieChanged = true;
            DataSender.SendOn.Set();
        }

        private void visionCalButton_Click(object sender, RoutedEventArgs e)
        {
            //new VisionCalibrate().ShowAsSingleTab(fieldTabControl, "Vision Parameters");
            //FieldWindow visionField = new FieldWindow(2);
            //visionField.charterEpander.Visibility = Visibility.Hidden;
            //visionField.TreeExpander.Visibility = Visibility.Hidden;
            //visionField.PaintExpander.Visibility = Visibility.Hidden;
            //visionField.loadingProgressBar.Visibility = Visibility.Hidden;
            //visionField.mainPanel.Visibility = Visibility.Hidden;
            //visionField.Name = "ass";
            //visionField.Name = "ass";
            //visionField.ShowAsSingleTab(fieldTabControl, "Vision Parameters");
            new MergerCalib().ShowAsSingleTab(fieldTabControl, "Vision Parameters");
        }

        private void PassShootTuneTool_Click(object sender, RoutedEventArgs e)
        {
            new PassAndSHootTuneWindow().Show();

        }

    }
}