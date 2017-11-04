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
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using MRL.SSL.GameDefinitions;
using System.IO.Ports;
using System.Threading;
using MRL.SSL.Visualizer.Extentions;
using MRL.SSL.CommonClasses.MathLibrary;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using MRL.SSL.Visualizer.Classes;
using System.IO;
using Enterprise;

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for ControlChartWindow.xaml
    /// </summary>
    public partial class ControlChartWindow : Window
    {
        class SaveClass
        {
            public int Time { get; set; }
            public double cvx { get; set; }
            public double cvy { get; set; }
            public double cw { get; set; }
            public double svx { get; set; }
            public double svy { get; set; }
            public double sw { get; set; }
            public double evx { get; set; }
            public double evy { get; set; }
            public double ew { get; set; }
            public double w1 { get; set; }
            public double w2 { get; set; }
            public double w3 { get; set; }
            public double w4 { get; set; }

            public double x { get; set; }
            public double y { get; set; }
            public double t { get; set; }
        }

        List<SaveClass> saveFile;
        bool saving = false;
        SerialPort sendPort;
        SerialPort recievePort;
        bool IsSave = false;
        public Thread sendThread, recieveThread;
        List<PerformanceData> data;
        List<LineAndMarker<ElementMarkerPointsGraph>>
           linegraphsVX,
           linegraphsVY,
           linegraphsW;
        string sendPortName = "com1", recievePortName = "com2";
        bool ShowChart = false;
        List<string> seriese;
        DebugMode DBMode = new DebugMode();
        private debug_mode Mode = debug_mode.mod_debug;

        public ControlChartWindow()
        {
            InitializeComponent();
            sendPort = new SerialPort(sendPortName, 115200);
            recievePort = new SerialPort(recievePortName, 115200);
            recievePort.ReadTimeout = 50;
            
            data = new List<PerformanceData>();

            //
            linegraphsVX = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            linegraphsVY = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            linegraphsW = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            
            //
            matrixCheckBox.Unchecked += new RoutedEventHandler(matrixCheckBox_Unchecked);
            seriese = new List<string>();
            List<string> ports = System.IO.Ports.SerialPort.GetPortNames().ToList();
            ports.ForEach(p =>
            {
                sendcomComboBox.Items.Add(p);
                recieveComboBox.Items.Add(p);
            });
            WindowExtensions.Closing += new WindowExtensions.TabItemClosing(WindowExtensions_Closing);
            sendThread = new Thread(new ThreadStart(Send));
            recieveThread = new Thread(new ThreadStart(Recieve));
            sendThread.Start();
            recieveThread.Start();
        }

        

        void WindowExtensions_Closing(object Sender, string Tag)
        {
            if (Tag == "controlchart")
            {
                sendThread.Abort();
                recieveThread.Abort();
                if (sendPort.IsOpen)
                    sendPort.Close();
                if (recievePort.IsOpen)
                    recievePort.Close();
            }
        }

        private void Send()
        {
            RequestPacketinDebugMode rpd = new RequestPacketinDebugMode();

            while (true)
            {
                if (sendPort.IsOpen)
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        if (Mode != DBMode.Mode && robotnumberComboBox.Text!="")
                        {
                            DBMode.Mode = Mode;
                            DBMode.RobotID = byte.Parse(robotnumberComboBox.Text);
                            byte[] ser = SetValidations(DBMode.Serialize());
                            sendPort.Write(ser, 0, ser.Length);
                            Thread.Sleep(15);
                        }
                        else
                        {
                            if (robotnumberComboBox.Text != "")
                            {
                                rpd.RobotID = byte.Parse(robotnumberComboBox.Text);
                                rpd.Omega = (float)wTextBox.Value;
                                rpd.V = new Position2D((double)vxTextBox.Value, (double)vyTextBox.Value);
                                rpd.SpeedData = true;
                                rpd.WS = true;
                                byte[] ser = SetValidations(rpd.Serialize());
                                sendPort.Write(ser, 0, ser.Length);
                            }
                        }
                    }));
                }
                Thread.Sleep(20);
            }
        }

        private byte[] SetValidations(byte[] val)
        {
            List<byte> ret = new List<byte>();
            ret.Add(128);
            ret.Add(128);
            ret.Add(128);
            foreach (var item in val)
                ret.Add(item);
            ret.Add(129);
            ret.Add(129);
            ret.Add(129);
            return ret.ToArray();
        }

        private DebugModeWrapper DBWrapper;

        private DebugModeWrapper RecieveOnePacket(bool isPWMSet)
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
                catch (Exception e)
                {
                    Logger.Write(LogType.Exception, e.ToString());

                    Iteration++;
                    System.Windows.Forms.Application.DoEvents();
                    if (Iteration > 30)
                    {
                        recievePort.ReadExisting();
                        return null;
                    }
                }
            }
            return null;
        }

        int counter = 0;
        double ay = 0,ax = 0, lastVy = 0, lastVx = 0;
        double[] AY = new double[20];
        double[] AX = new double[20];
        int count = 0;
        bool first = true;
        private void Recieve()
        {

            Dispatcher.Invoke((Action)(() =>
            {
                linegraphsVX.Add(CreatePerformanceGraph(debugPlotter1, new ChartItem() { Name = "Command1" }, Brushes.Blue, "Command.1"));
                linegraphsVY.Add(CreatePerformanceGraph(debugPlotter2, new ChartItem() { Name = "Command2" }, Brushes.Blue, "Command.2"));
                linegraphsW.Add(CreatePerformanceGraph(debugPlotter3, new ChartItem() { Name = "Command3" }, Brushes.Blue, "Command.3"));

                linegraphsVX.Add(CreatePerformanceGraph(debugPlotter1, new ChartItem() { Name = "Speed1" }, Brushes.Red, "SpeedData.1"));
                linegraphsVY.Add(CreatePerformanceGraph(debugPlotter2, new ChartItem() { Name = "Speed2" }, Brushes.Red, "SpeedData.2"));
                linegraphsW.Add(CreatePerformanceGraph(debugPlotter3, new ChartItem() { Name = "Speed3" }, Brushes.Red, "SpeedData.3"));

                linegraphsVX.Add(CreatePerformanceGraph(debugPlotter1, new ChartItem() { Name = "encoder1" }, Brushes.Green, "EncoderData.1"));
                linegraphsVY.Add(CreatePerformanceGraph(debugPlotter2, new ChartItem() { Name = "encoder2" }, Brushes.Green, "EncoderData.2"));
                linegraphsW.Add(CreatePerformanceGraph(debugPlotter3, new ChartItem() { Name = "encoder3" }, Brushes.Green, "EncoderData.3"));

              /*  linegraphsVX.Add(CreatePerformanceGraph(debugPlotter1, new ChartItem() { Name = "Ax" }, Brushes.Gold, "Ax"));
                linegraphsVY.Add(CreatePerformanceGraph(debugPlotter2, new ChartItem() { Name = "Ay" }, Brushes.Gold, "Ay"));
                linegraphsW.Add(CreatePerformanceGraph(debugPlotter3, new ChartItem() { Name = "W" }, Brushes.Gold, "W"));*/
                
            }));
            int counter = 0;
            while (true)
            {
               // if (recievePort.IsOpen)
                {

                    DebugModeWrapper ret = null;
                    //Dispatcher.Invoke((Action)(() =>
                    //{
                    //    ret = RecieveOnePacket(false);
                    //}));
                    if (ret == null)
                        ret = new DebugModeWrapper(new byte[32]);

                    if (ret != null)
                    {
                        //recievePort.ReadExisting();
                        Dispatcher.Invoke((Action)(() =>
                        {
                            
                            Vector2D V = new Vector2D();
                            double Rotation = 0;
                            if (ShowChart)
                            {
                                int ms = (int)DataReciever.CurrentWrapper.Model.TimeElapsed.TotalMilliseconds;//(int)DateTime.Now.TimeOfDay.TotalMilliseconds;
                                bool robotFound = false;
                                SingleObjectState so = null;
                                SingleWirelessCommand SWC = null;
                                if (DataReciever.CurrentWrapper.RobotCommnd != null && DataReciever.CurrentWrapper.RobotCommnd.ContainsKey(int.Parse(robotnumberComboBox.Text))
                                     && DataReciever.CurrentWrapper.Model.OurRobots.ContainsKey(int.Parse(robotnumberComboBox.Text)))
                                {
                                    robotFound = true;
                                    so = DataReciever.CurrentWrapper.Model.OurRobots[int.Parse(robotnumberComboBox.Text)];
                                    SWC = DataReciever.CurrentWrapper.RobotCommnd[int.Parse(robotnumberComboBox.Text)];
                                }
                                if (robotFound)
                                {
                                    Rotation = so.Angle.Value;
                                    Vector2D temp = new Vector2D(so.Speed.Y, so.Speed.X);
                                    Rotation *= Math.PI / (double)180;

                                    V.X = temp.X * Math.Cos(Rotation) - temp.Y * Math.Sin(Rotation);
                                    V.Y = temp.Y * Math.Cos(Rotation) + temp.X * Math.Sin(Rotation);


                                    //encoder
                                    PerformanceData envx = data.Single(s => s.Name == "Command.1");
                                    PerformanceData envy = data.Single(s => s.Name == "Command.2");
                                    PerformanceData enw = data.Single(s => s.Name == "Command.3");
                                    envx.Add(new ChartDataInfo() { Value = SWC.Vx, Time = ms });
                                    envy.Add(new ChartDataInfo() { Value = SWC.Vy, Time = ms });
                                    enw.Add(new ChartDataInfo() { Value = SWC.W, Time = ms });
                                    //speed
                                    PerformanceData selctedvx = data.Single(s => s.Name == "SpeedData.1");
                                    PerformanceData selctedvy = data.Single(s => s.Name == "SpeedData.2");
                                    PerformanceData selctedw = data.Single(s => s.Name == "SpeedData.3");
                                    selctedvx.Add(new ChartDataInfo() { Value = V.X, Time = ms });
                                    selctedvy.Add(new ChartDataInfo() { Value = V.Y, Time = ms });
                                    selctedw.Add(new ChartDataInfo() { Value = so.AngularSpeed.Value, Time = ms });
                                    //Acceleration
                                    //PerformanceData selctedax = data.Single(s => s.Name == "Ax");
                                    //PerformanceData selcteday = data.Single(s => s.Name == "Ay");
                                    //PerformanceData selcteaw = data.Single(s => s.Name == "W");
                                    
                                    AY[count] = (V.Y - lastVy) * 60;
                                    AX[count] = (V.X - lastVx) * 60;
                                    lastVy = V.Y; 
                                    lastVx = V.X;
                                    count++;

                                    if (count == AX.Length)
                                    {
                                        first = false;
                                        count = 0;
                                    }
                                    if (!first)
                                    {
                                        ay = 0;
                                        ax = 0;
                                        for (int i = 0; i < AX.Length; i++)
                                        {
                                            ay += AY[i];
                                            ax += AX[i];
                                        }
                                        ay /= AX.Length;
                                        ax /= AX.Length;
                                    }
                                    
                                    //selctedax.Add(new ChartDataInfo() { Value = ax, Time = ms });
                                    //selcteday.Add(new ChartDataInfo() { Value = ay, Time = ms });
                                    //selcteaw.Add(new ChartDataInfo() { Value = so.AngularSpeed.Value, Time = ms });

                                    //
                                    MathMatrix Ain = new MathMatrix(3, 3);
                                    Ain[0, 0] = 0.7028; Ain[0, 1] = 0.0583; Ain[0, 2] = -0.0184;

                                    Ain[1, 0] = 0.0402; Ain[1, 1] = 0.8462; Ain[1, 2] = 0.0130;

                                    Ain[2, 0] = 0.1248; Ain[2, 1] = 0.0610; Ain[2, 2] = 0.9611;

                                    MathMatrix B = new MathMatrix(3, 1);
                                    B[0, 0] = 0.0044;
                                    B[1, 0] = 0.0036;
                                    B[2, 0] = 0.0289;


                                    MathMatrix Vc = new MathMatrix(3, 1);
                                    Vc[0, 0] = SWC.Vx;
                                    Vc[1, 0] = SWC.Vy;
                                    Vc[2, 0] = SWC.W;

                                    MathMatrix vd = Ain * (Vc - B);
                                    
                                    PerformanceData cvx = data.Single(s => s.Name == "EncoderData.1");
                                    PerformanceData cvy = data.Single(s => s.Name == "EncoderData.2");
                                    PerformanceData cw = data.Single(s => s.Name == "EncoderData.3");
                                    //cvx.Add(new ChartDataInfo() { Value = vd[0, 0], Time = ms });
                                    //cvy.Add(new ChartDataInfo() { Value = vd[1, 0], Time = ms });
                                    //cw.Add(new ChartDataInfo() { Value = vd[2, 0], Time = ms });
                                    if (DataReciever.CurrentWrapper.MtrixData != null)
                                    {
                                        if (!double.IsNaN(DataReciever.CurrentWrapper.MtrixData.vx))
                                            cvx.Add(new ChartDataInfo() { Value = DataReciever.CurrentWrapper.MtrixData.vx, Time = ms });
                                        if (!double.IsNaN(DataReciever.CurrentWrapper.MtrixData.vy))
                                            cvy.Add(new ChartDataInfo() { Value = DataReciever.CurrentWrapper.MtrixData.vy, Time = ms });
                                        if (!double.IsNaN(DataReciever.CurrentWrapper.MtrixData.w))
                                            cw.Add(new ChartDataInfo() { Value = DataReciever.CurrentWrapper.MtrixData.w, Time = ms });
                                    }
                                }
                                //command
                                double wheelRadius = 0.025;
                                double w1 = ret.WSpeedData.Wheelspeed1 * wheelRadius;
                                double w2 = ret.WSpeedData.Wheelspeed3 * wheelRadius;
                                double w3 = ret.WSpeedData.Wheelspeed4 * wheelRadius;
                                double w4 = ret.WSpeedData.Wheelspeed2 * wheelRadius;
                                //double vx = -w1* 0.5446 - w2 * 0.5446 + w3 * 0.7071 + w4 * 0.7071;
                                //double vy = w1 * 0.8386 - w2 * 0.8386 - w3 * 0.7071 + w4 * 0.7071;
                                //double w = ret.WSpeedData.Wheelspeed1;//100 / (9.0) * (w1 + w2 + w3 + w4);


                                double vx = w1 * -0.3994 + w2 * -0.3994 + w3 * 0.3994 + w4 * 0.3994;
                                double vy = w1 * 0.3485 + w2 * -0.3485 + w3 * -0.2938 + w4 * 0.2938;
                                double w = w1 * 0.0254 + w2 * 0.0254 + w3 * 0.0196 + w4 * 0.0196;

                                //PerformanceData cvx = data.Single(s => s.Name == "EncoderData.1");
                                //PerformanceData cvy = data.Single(s => s.Name == "EncoderData.2");
                                //PerformanceData cw = data.Single(s => s.Name == "EncoderData.3");

                                // cvx.Add(new ChartDataInfo() { Value = vx, Time = ms });
                                // cvy.Add(new ChartDataInfo() { Value = vy, Time = ms });
                                // cw.Add(new ChartDataInfo() { Value = w, Time = ms });

                                if (saving)
                                {
                                    if (counter % 20 == 0 && recImage.Visibility == Visibility.Hidden)
                                        recImage.Visibility = Visibility.Visible;
                                    else if (counter % 8 == 0 && recImage.Visibility == Visibility.Visible)
                                        recImage.Visibility = Visibility.Hidden;
                                    counter++;
                                    countLabel.Content = counter;
                                    saveFile.Add(new SaveClass()
                                    {
                                        Time = ms,
                                        cvx = (SWC != null) ? SWC.Vx : 0,
                                        cvy = (SWC != null) ? SWC.Vy : 0,
                                        cw = (SWC != null) ? SWC.W : 0,
                                        evx = vx,
                                        evy = vy,
                                        ew = w,
                                        svx = (robotFound) ? V.X : 0,
                                        svy = (robotFound) ? V.Y : 0,
                                        sw = (robotFound && so.AngularSpeed.HasValue) ? so.AngularSpeed.Value : 0,
                                        w1 = w1,
                                        w2 = w2,
                                        w3 = w3,
                                        w4 = w4,
                                        x = so.Location.X,
                                        y = so.Location.Y,
                                        t = so.Angle.Value,
                                    });
                                }

                            }
                        }));
                    }
                    Thread.Sleep(50);
                }
            }
        }

        private LineAndMarker<ElementMarkerPointsGraph> CreatePerformanceGraph(ChartPlotter chartplot, ChartItem category, Brush br, string key)
        {
            data.Add(new PerformanceData() { Name = key });
            var ds = new EnumerableDataSource<ChartDataInfo>(data.Last());
            ds.SetXMapping(pi => pi.Time);
            ds.SetYMapping(pi => pi.Value);
            ds.AddMapping(ShapeElementPointMarker.ToolTipTextProperty,
                Y => String.Format("Time : {0}\nValue : {1}", Y.Time, Y.Value.ToString("f3")));
            LineAndMarker<ElementMarkerPointsGraph> chart = chartplot.AddLineGraph(ds,
                new Pen(br, 2),
                new CircleElementPointMarker
                {
                    Size = 7,
                    Brush = Brushes.Black,
                    Fill = Brushes.Wheat
                },
                new PenDescription(String.Format("{0}-{1}", category.Name, key)));

            chart.LineGraph.Name = category.Name;
            chart.MarkerGraph.DataSource = null;
            return chart;

        }

        private void recieveComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            recievePort.Close();
           // recievePort.Dispose();
           // recievePort = new SerialPort();
            recievePort.PortName = recieveComboBox.SelectedItem.As<string>();
            recievePort.Open();

        }

        private void sendcomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sendPort.Close();

            sendPortName = sendcomComboBox.SelectedItem.ToString();
            sendPort = new SerialPort();
            sendPort.PortName = sendPortName;
            sendPort.BaudRate = 115200;

            if (!sendPort.IsOpen)
                sendPort.Open();

        }
        bool MarkersIsShown = false;
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            ShowChart = !ShowChart;
            if (ShowChart)
            {
                playImage.SetImageSource("redPause.png");
            }
            else
            {
                playImage.SetImageSource("redPlay.png");
            }
        }

        private void showMarkers(bool shown)
        {
            if (!shown)
            {
                linegraphsVX.ForEach(p => p.MarkerGraph.Marker = null);
                linegraphsVY.ForEach(p => p.MarkerGraph.Marker = null);
                linegraphsW.ForEach(p => p.MarkerGraph.Marker = null);
            }
            else
            {
                linegraphsVX.ForEach(p =>
                {
                    p.MarkerGraph.DataSource = null;
                    p.MarkerGraph.Marker = new CircleElementPointMarker
                    {
                        Size = 7,
                        Brush = Brushes.Black,
                        Fill = Brushes.Wheat
                    };
                    p.MarkerGraph.DataSource = p.LineGraph.DataSource;
                });
                linegraphsVY.ForEach(p =>
                {
                    p.MarkerGraph.DataSource = null;
                    p.MarkerGraph.Marker = new CircleElementPointMarker
                    {
                        Size = 7,
                        Brush = Brushes.Black,
                        Fill = Brushes.Wheat
                    };
                    p.MarkerGraph.DataSource = p.LineGraph.DataSource;
                });
                linegraphsW.ForEach(p =>
                {
                    p.MarkerGraph.DataSource = null;
                    p.MarkerGraph.Marker = new CircleElementPointMarker
                    {
                        Size = 7,
                        Brush = Brushes.Black,
                        Fill = Brushes.Wheat
                    };
                    p.MarkerGraph.DataSource = p.LineGraph.DataSource;
                });
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            vxTextBox.Value = 0;
            vyTextBox.Value = 0;
            wTextBox.Value = 0;
        }

        private void mode3RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Mode = debug_mode.mod_debug;
        }

        private void mode1RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Mode = debug_mode.mod_normal;
        }

        private void mode2RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Mode = debug_mode.mod_control;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (saving)
            {
                saving = false;
                recImage.Visibility = Visibility.Visible;
                saveToFile();
                counter = 0;
                countLabel.Content = 0;
                
            }
            else
            {
                counter = 0;
                countLabel.Content = 0;
                saveFile = new List<SaveClass>();
                saving = true;
            }
        }

        private void saveToFile()
        {
            if (!File.Exists(@"d:\vData"))
                System.IO.Directory.CreateDirectory(@"d:\vData");
            StreamWriter swdata = new StreamWriter(@"d:\vData\data.txt");


            swdata.Write("Time\t");
            swdata.Write("Command VX\t");
            swdata.Write("Speed VX\t");
            swdata.Write("Encoder VX\t");

            swdata.Write("Command VY\t");
            swdata.Write("Speed VY\t");
            swdata.Write("Encoder VY\t");

            swdata.Write("Command W\t");
            swdata.Write("Speed W\t");
            swdata.Write("Encoder W\t");

            swdata.Write("W1\t");
            swdata.Write("W2\t");
            swdata.Write("W3\t");
            swdata.Write("W4\t");

            swdata.Write("X\t");
            swdata.Write("Y\t");
            swdata.WriteLine("Teta");

            foreach (var item in saveFile)
            {
                swdata.Write(item.Time + "\t");
                swdata.Write(item.cvx.ToString("f3") + "\t");
                swdata.Write(item.svx.ToString("f3") + "\t");
                swdata.Write(item.evx.ToString("f3") + "\t");

                swdata.Write(item.cvy.ToString("f3") + "\t");
                swdata.Write(item.svy.ToString("f3") + "\t");
                swdata.Write(item.evy.ToString("f3") + "\t");

                swdata.Write(item.cw.ToString("f3") + "\t");
                swdata.Write(item.sw.ToString("f3") + "\t");
                swdata.Write(item.ew.ToString("f3") + "\t");

                swdata.Write(item.w1.ToString("f3") + "\t");
                swdata.Write(item.w2.ToString("f3") + "\t");
                swdata.Write(item.w3.ToString("f3") + "\t");
                swdata.Write(item.w4.ToString("f3") + "\t");

                swdata.Write(item.x.ToString("f3") + "\t");
                swdata.Write(item.y.ToString("f3") + "\t");
                swdata.WriteLine(item.t.ToString("f3"));

            }
            swdata.Close();
        }

        private void matrixCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("Bool");
            DataSender.CurrentWrapper.WithMatrix = matrixCheckBox.IsChecked.Value;
            DataSender.SendOn.Set();
        }

        void matrixCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("Bool");
            DataSender.CurrentWrapper.WithMatrix = matrixCheckBox.IsChecked.Value;
            DataSender.SendOn.Set();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void markerButton_Checked(object sender, RoutedEventArgs e)
        {
            showMarkers(true);
        }

        private void markerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            showMarkers(false);
        }

        private void getMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            GetMatrixWindow w= new GetMatrixWindow();
            w.ShowDialog();
            if (w.IsFilled)
            {
                DataSender.CurrentWrapper.SendData.Add("Matrixs");
                DataSender.OPMatrix.E = w.E;
                DataSender.OPMatrix.D = w.D;
                DataSender.SendOn.Set();
            }
        }

        
    }
}
