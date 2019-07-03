using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using MRL.SSL.Visualizer.Extentions;
using System.Windows.Media.Imaging;
using Visualizer.Classes;
using System.IO;
using MRL.SSL.GameDefinitions;
using System.Threading;
using System.Windows.Threading;

using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.CommonClasses;
using MRL.SSL.Visualizer.Classes;
using System.Drawing;
using System.Threading.Tasks;

using MRL.SSL.GameDefinitions.Visualizer_Classes;
using System.Collections;
using System.Reflection;
using Enterprise;
using MRL.SSL.Visualizer.Windows;

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for FieldWindow.xaml
    /// </summary>
    public partial class FieldWindow : Window
    {
        DrawMode drawMode = DrawMode.Non;
        bool mouseLeftDown = false;
        bool mouseRightDown = false;
        Position2D startloc = new Position2D();
        public Color PaintColor { get; set; }
        public float PaintWidth { get; set; }
        float? angle = new float();
        public enum DrawMode
        {
            Non,
            Line,
            Circle,
        }

        List<AiToVisualizerWrapper> ret;
        GoogleSerializer gSerialize;
        Thread th;
        State CurrentState = State.Stop;
        List<LoggerDrawingObject> list, list1;
        List<LogCharterData> lcd;
        LoggerDrawingObject MainLdo;

        int delay = (int)(StaticVariables.FRAME_PERIOD * 1000);

        enum State
        {
            Play,
            Stop,
            Pause
        }

        bool reopen = false;

        int currentSeek = 0;

        bool memorylog = true;

        int LastSeek = 0;

        public FieldWindow(int a)
        {
            InitializeComponent();
        }

        public FieldWindow()
        {
            InitializeComponent();
            LogLoading.load += new LogLoading.LoadedPacket(LogLoading_load);
            LogLoading.loadDrawingObject += new LogLoading.LoadedDrawPacket(LogLoading_loadDrawingObject);

            mainPanel.fileseekSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(fileseekSlider_ValueChanged);
            mainPanel.openButton.Click += new RoutedEventHandler(openButton_Click);
            mainPanel.playButton.Click += new RoutedEventHandler(playButton_Click);
            mainPanel.stopButton.Click += new RoutedEventHandler(stopButton_Click);
            mainPanel.slowButton.Click += new RoutedEventHandler(slowButton_Click);
            mainPanel.fastButton.Click += new RoutedEventHandler(fastButton_Click);
            DataReciever.ChartRun = false;
            mainField.LogPlayerMode = true;
            MainLdo = new LoggerDrawingObject();
            lcd = new List<LogCharterData>();
            gSerialize = new GoogleSerializer();
            ret = new List<AiToVisualizerWrapper>();
            list = new List<LoggerDrawingObject>();
            list1 = new List<LoggerDrawingObject>();

            LogProssesor.ListMemoryLog.ForEach(p => ret.Add(p));
            mainPanel.fileseekSlider.Maximum = ret.Count - 1;
            mainPanel.fileseekSlider.Value = 0;
            mainPanel.seekViewerLabel.Content = "00:00:00 / 00:00:00";
            MainCharter.Log = true;


            PaintColor = Color.Black;
            PaintWidth = 0.01f;
            mainField.MouseMove += new System.Windows.Forms.MouseEventHandler(mainField_MouseMove);
            mainField.MouseDown += new System.Windows.Forms.MouseEventHandler(mainField_MouseDown);
            mainField.MouseUp += new System.Windows.Forms.MouseEventHandler(mainField_MouseUp);
            mainField.AnalizeMode = true;


        }

        void fastButton_Click(object sender, RoutedEventArgs e)
        {
            if (delay > 2)
                delay /= 2;
            mainPanel.xLabel.Content = calculateXlabel() + "x";
        }

        void slowButton_Click(object sender, RoutedEventArgs e)
        {
            if (delay < 128)
                delay *= 2;
            mainPanel.xLabel.Content = calculateXlabel() + "x";
        }



        float calculateXlabel()
        {
            switch (delay)
            {
                case 2: return 128f;
                case 4: return 64f;
                case 8: return 32;
                case 16: return 16;
                case 32: return 8;
                case 64: return 4;
                default:
                    return 1f;
            }
        }

        void stopButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState = State.Stop;
        }

        void fileseekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Dispatcher.Invoke((Action)(() =>
            //{
            //    gSerialize.SMD.Save();
            //    gSerialize.SMD.Clear();
            //}));
            frameLabel.Content = (int)mainPanel.fileseekSlider.Value + " / " + mainPanel.fileseekSlider.Maximum;
            LastSeek = (int)mainPanel.fileseekSlider.Value;
            mainField.CurrentWrapper = ret[(int)mainPanel.fileseekSlider.Value];
            currentSeek = (int)mainPanel.fileseekSlider.Value;

            LogProssesor.CurrentReaded = ret[(int)mainPanel.fileseekSlider.Value];
            if (!memorylog)
            {
                LoggerDrawingObject l = list[(int)mainPanel.fileseekSlider.Value];
                l.ObjectTree = MainLdo.ObjectTree;
                mainField.Logdrawingobjects = l;
            }
        }

        private void thread()
        {
            while (true)
            {
                try
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Visualizer)
                        {
                            DataSender.CurrentWrapper.SendData.Add("Model");
                            DataSender.CurrentWrapper.RecieveMode = ModelRecieveMode.Visualizer;
                            DataSender.CurrentWrapper.SendData.Add("RecieveMode");
                            DataSender.CurrentWrapper.Model = ret[currentSeek].GlobalModel;
                            DataSender.SendOn.Set();
                        }
                        PlayAction();
                    }));
                    Thread.Sleep(delay);
                }
                catch (TargetInvocationException ex)
                {
                    Logger.WriteError("---Main Log Player Loading---" + ex.ToString());
                }
            }
        }

        void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentState == State.Stop)
            {
                currentSeek = 0;
                mainPanel.playbuttonImage.SetImageSource("pause.png");
                CurrentState = State.Play;
                th = new Thread(new ThreadStart(thread));
                th.Start();
            }
            else if (CurrentState == State.Play)
            {
                currentSeek = (int)mainPanel.fileseekSlider.Value;
                mainPanel.playbuttonImage.SetImageSource("play.png");
                CurrentState = State.Pause;
            }
            else if (CurrentState == State.Pause)
            {
                mainPanel.playbuttonImage.SetImageSource("pause.png");
                CurrentState = State.Play;
            }
        }

        void openButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState = State.Stop;
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Multiselect = true;
            op.Filter = "SSL Log Files (*.log)|*.log";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            mainPanel.seekViewerLabel.Content = "Loading...";

            Task.WaitAll();
            gSerialize = new GoogleSerializer();
            var t = new Task((Action)(() =>
                 {
                     try
                     {
                         Dispatcher.Invoke((Action)(() =>
                         {
                             ret = new List<AiToVisualizerWrapper>();
                             FileStream logfile = new FileStream(op.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                             byte[] b = new byte[logfile.Length];
                             logfile.Read(b, 0, (int)logfile.Length);
                             MemoryStream stram = new MemoryStream(b);
                             string comment;
                             loadingProgressBar.Maximum = (int)(stram.Length - 1);
                             list.Clear();
                             gSerialize.DeserializeLogFile(stram, out comment, out list1, out lcd, "main");
                             list.ForEach(p => DrawTree(ref p));
                             if (th == null)
                             {
                                 mainPanel.fileseekSlider.Maximum = (ret.Count == 0) ? 0 : ret.Count - 1;
                                 mainPanel.fileseekSlider.Value = 0;
                             }
                             else
                             {
                                 CurrentState = State.Stop;
                                 reopen = true;
                             }
                             string[] ss = op.FileName.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                             mainPanel.addressLabel.Content = ss[ss.Length - 1];
                             mainPanel.addressLabel.ToolTip = op.FileName;
                             memorylog = false;
                         }));
                     }
                     catch (TargetInvocationException ex)
                     {
                         Logger.WriteError("------Log Openning---------\n" + ex.ToString());
                     }
                 }));
            t.Start();

        }

        void LogLoading_load(AiToVisualizerWrapper sender, string tag)
        {
            if (tag == "main")
            {
                ret.Add(sender);
                try
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        loadingProgressBar.Value = DrawingObjects.CurrentPositionReaded;
                        if (loadingProgressBar.Value == loadingProgressBar.Maximum)
                            loadingProgressBar.Visibility = Visibility.Collapsed;
                        else
                            loadingProgressBar.Visibility = Visibility.Visible;

                        mainPanel.fileseekSlider.Maximum = ret.Count - 1;
                        TimeSpan ts = (ret.Count > 0) ? ret[ret.Count - 1].Model.TimeElapsed - ret[0].Model.TimeElapsed : new TimeSpan();
                    }));
                }
                catch (TargetInvocationException ex)
                {
                    Logger.WriteError("----Log Loading----\n" + ex.ToString());
                }
            }
        }

        void LogLoading_loadDrawingObject(LoggerDrawingObject sender, string tag)
        {
            if (tag == "main")
            {
                list.Add(sender);
                try
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        DrawTree(ref sender);
                    }));
                }
                catch (TargetInvocationException ex)
                {
                    Logger.WriteError("---Log object Loading---" + ex.ToString());
                }
            }
        }

        private void DrawTree(ref LoggerDrawingObject ldo)
        {
            #region Draw Tree
            if (MainLdo.ObjectTree.Count == 0)
                MainLdo.ObjectTree.Add(new LogTreeViewModel("DrawingObjects"));

            if (LogTreeViewModel.GetItemByName("Global", MainLdo.ObjectTree[0]) == null)
            {
                MainLdo.ObjectTree[0].Children.Add(new LogTreeViewModel("Global"));
                MainLdo.ObjectTree[0].Children.Last().Parent = MainLdo.ObjectTree[0];
            }
            LogTreeViewModel globalroot = LogTreeViewModel.GetItemByName("Global", MainLdo.ObjectTree[0]);
            foreach (var item in ldo.drawingObject.Keys)
            {
                if (ldo.drawingObject[item].GetType() == typeof(DrawCollection))
                {
                    #region DrawCollection
                    if (LogTreeViewModel.GetItemByName("Draw Collections", MainLdo.ObjectTree[0]) == null)
                    {
                        MainLdo.ObjectTree[0].Children.Add(new LogTreeViewModel("Draw Collections"));
                        MainLdo.ObjectTree[0].Children.Last().Parent = MainLdo.ObjectTree[0];
                        refresh(MainLdo);
                    }
                    if (LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]) == null)
                    {
                        LogTreeViewModel t = LogTreeViewModel.GetItemByName("Draw Collections", MainLdo.ObjectTree[0]);
                        t.Children.Add(new LogTreeViewModel(item));
                        t.Children.Last().Parent = LogTreeViewModel.GetItemByName("Draw Collections", MainLdo.ObjectTree[0]);
                        refresh(MainLdo);
                    }
                    foreach (var item1 in ldo.drawingObject[item].As<DrawCollection>().drawingObject.Keys)
                    {
                        #region draw lines in groups
                        if (ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].GetType() == typeof(Line) && ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<Line>().IsShown)
                        {
                            if (LogTreeViewModel.GetItemByName("Lines", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])) == null)
                            {
                                LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]).Children.Add(new LogTreeViewModel("Lines"));
                                LogTreeViewModel.GetItemByName("Lines", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Parent = LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]);
                                refresh(MainLdo);
                            }
                            if (LogTreeViewModel.GetItemByName(item1, LogTreeViewModel.GetItemByName("Lines", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]))) == null)
                            {
                                LogTreeViewModel.GetItemByName("Lines", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Children.Add(new LogTreeViewModel(item1));
                                LogTreeViewModel t = LogTreeViewModel.GetItemByName(item1, MainLdo.ObjectTree[0]);
                                t.Parent = LogTreeViewModel.GetItemByName("Lines", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]));
                                t.SetIsChecked(ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<Line>().IsShown, true, true);
                                refresh(MainLdo);
                            }
                        }
                        #endregion

                        #region Draw circles in groups
                        else if (ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].GetType() == typeof(Circle) && ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<Circle>().IsShown)
                        {
                            if (LogTreeViewModel.GetItemByName("Circles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])) == null)
                            {
                                LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]).Children.Add(new LogTreeViewModel("Circles"));
                                LogTreeViewModel.GetItemByName("Circles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Parent = LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]);
                                refresh(MainLdo);
                            }
                            if (LogTreeViewModel.GetItemByName(item1, LogTreeViewModel.GetItemByName("Circles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]))) == null)
                            {
                                LogTreeViewModel.GetItemByName("Circles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Children.Add(new LogTreeViewModel(item1));
                                LogTreeViewModel t = LogTreeViewModel.GetItemByName(item1, MainLdo.ObjectTree[0]);
                                t.Parent = LogTreeViewModel.GetItemByName("Circles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]));
                                t.SetIsChecked(ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<Circle>().IsShown, true, true);
                                refresh(MainLdo);
                            }
                        }
                        #endregion

                        #region Drae Rectangles in Groups

                        else if (ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].GetType() == typeof(FlatRectangle) && ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<FlatRectangle>().IsShown)
                        {
                            if (LogTreeViewModel.GetItemByName("Rectangles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])) == null)
                            {
                                LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]).Children.Add(new LogTreeViewModel("Rectangles"));
                                LogTreeViewModel.GetItemByName("Rectangles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Parent = LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]);
                                refresh(MainLdo);
                            }
                            if (LogTreeViewModel.GetItemByName(item1, LogTreeViewModel.GetItemByName("Rectangles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]))) == null)
                            {
                                LogTreeViewModel.GetItemByName("Rectangles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Children.Add(new LogTreeViewModel(item1));
                                LogTreeViewModel t = LogTreeViewModel.GetItemByName(item1, MainLdo.ObjectTree[0]);
                                t.Parent = LogTreeViewModel.GetItemByName("Rectangles", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]));
                                t.SetIsChecked(ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<FlatRectangle>().IsShown, true, true);
                                refresh(MainLdo);
                            }
                        }
                        #endregion

                        #region draw SingleObjectState
                        else if (ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].GetType() == typeof(SingleObjectState) && ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<SingleObjectState>().IsShown)
                        {
                            if (LogTreeViewModel.GetItemByName("Robots", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])) == null)
                            {
                                LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]).Children.Add(new LogTreeViewModel("Robots"));
                                LogTreeViewModel.GetItemByName("Robots", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Parent = LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]);
                                refresh(MainLdo);
                            }
                            if (LogTreeViewModel.GetItemByName(item1, LogTreeViewModel.GetItemByName("Robots", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]))) == null)
                            {
                                LogTreeViewModel.GetItemByName("Robots", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Children.Add(new LogTreeViewModel(item1));
                                LogTreeViewModel t = LogTreeViewModel.GetItemByName(item1, MainLdo.ObjectTree[0]);
                                t.Parent = LogTreeViewModel.GetItemByName("Robots", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]));
                                t.SetIsChecked(ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<SingleObjectState>().IsShown, true, true);
                                refresh(MainLdo);
                            }
                        }
                        #endregion

                        #region Draw Text in Gruops
                        else if (ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].GetType() == typeof(StringDraw) && ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<StringDraw>().IsShown)
                        {
                            if (LogTreeViewModel.GetItemByName("Texts", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])) == null)
                            {
                                LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]).Children.Add(new LogTreeViewModel("Texts"));
                                LogTreeViewModel.GetItemByName("Texts", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Parent = LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]);
                                refresh(MainLdo);
                            }
                            if (LogTreeViewModel.GetItemByName(item1, LogTreeViewModel.GetItemByName("Texts", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]))) == null)
                            {
                                LogTreeViewModel.GetItemByName("Texts", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Children.Add(new LogTreeViewModel(item1));
                                LogTreeViewModel t = LogTreeViewModel.GetItemByName(item1, MainLdo.ObjectTree[0]);
                                t.Parent = LogTreeViewModel.GetItemByName("Texts", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]));
                                t.SetIsChecked(ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<StringDraw>().IsShown, true, true);
                                refresh(MainLdo);
                            }
                        }
                        #endregion
                        #region Draw Region in Gruops
                        else if (ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].GetType() == typeof(DrawRegion) && ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<DrawRegion>().IsShown)
                        {
                            if (LogTreeViewModel.GetItemByName("Path&Region", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])) == null)
                            {
                                LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]).Children.Add(new LogTreeViewModel("Path&Region"));
                                LogTreeViewModel.GetItemByName("Path&Region", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Parent = LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]);
                                refresh(MainLdo);
                            }
                            if (LogTreeViewModel.GetItemByName(item1, LogTreeViewModel.GetItemByName("Path&Region", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]))) == null)
                            {
                                LogTreeViewModel.GetItemByName("Path&Region", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0])).Children.Add(new LogTreeViewModel(item1));
                                LogTreeViewModel t = LogTreeViewModel.GetItemByName(item1, MainLdo.ObjectTree[0]);
                                t.Parent = LogTreeViewModel.GetItemByName("Path&Region", LogTreeViewModel.GetItemByName(item, MainLdo.ObjectTree[0]));
                                t.SetIsChecked(ldo.drawingObject[item].As<DrawCollection>().drawingObject[item1].As<DrawRegion>().IsShown, true, true);
                                refresh(MainLdo);
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (ldo.drawingObject[item].GetType() == typeof(Line) && ldo.drawingObject[item].As<Line>().IsShown)
                {
                    #region Draw Lines
                    if (LogTreeViewModel.GetItemByName("Lines", globalroot) == null)
                    {
                        globalroot.Children.Add(new LogTreeViewModel("Lines"));
                        LogTreeViewModel.GetItemByName("Lines", globalroot).Parent = globalroot;
                        refresh(MainLdo);
                    }
                    if (LogTreeViewModel.GetItemByName(item, globalroot) == null)
                    {
                        LogTreeViewModel.GetItemByName("Lines", globalroot).Children.Add(new LogTreeViewModel(item));
                        LogTreeViewModel t = LogTreeViewModel.GetItemByName(item, globalroot);
                        t.Parent = LogTreeViewModel.GetItemByName("Lines", globalroot);
                        t.SetIsChecked(ldo.drawingObject[item].As<Line>().IsShown, true, true);
                        refresh(MainLdo);
                    }
                    #endregion
                }
                else if (ldo.drawingObject[item].GetType() == typeof(Circle) && ldo.drawingObject[item].As<Circle>().IsShown)
                {
                    #region Draw Circles
                    if (LogTreeViewModel.GetItemByName("Circles", globalroot) == null)
                    {
                        globalroot.Children.Add(new LogTreeViewModel("Circles"));
                        LogTreeViewModel.GetItemByName("Circles", globalroot).Parent = globalroot;
                        refresh(MainLdo);
                    }
                    if (LogTreeViewModel.GetItemByName(item, globalroot) == null)
                    {
                        LogTreeViewModel.GetItemByName("Circles", globalroot).Children.Add(new LogTreeViewModel(item));
                        LogTreeViewModel t = LogTreeViewModel.GetItemByName(item, globalroot);
                        t.Parent = LogTreeViewModel.GetItemByName("Circles", globalroot);
                        t.SetIsChecked(ldo.drawingObject[item].As<Circle>().IsShown, true, true);
                        refresh(MainLdo);
                    }
                    #endregion
                }
                else if (ldo.drawingObject[item].GetType() == typeof(FlatRectangle) && ldo.drawingObject[item].As<FlatRectangle>().IsShown)
                {
                    #region DrawRectangles
                    if (LogTreeViewModel.GetItemByName("Rectangles", globalroot) == null)
                    {
                        globalroot.Children.Add(new LogTreeViewModel("Rectangles"));
                        LogTreeViewModel.GetItemByName("Rectangles", globalroot).Parent = globalroot;
                        refresh(MainLdo);
                    }
                    if (LogTreeViewModel.GetItemByName(item, globalroot) == null)
                    {
                        LogTreeViewModel.GetItemByName("Rectangles", globalroot).Children.Add(new LogTreeViewModel(item));
                        LogTreeViewModel t = LogTreeViewModel.GetItemByName(item, globalroot);
                        t.Parent = LogTreeViewModel.GetItemByName("Rectangles", globalroot);
                        t.SetIsChecked(ldo.drawingObject[item].As<FlatRectangle>().IsShown, true, true);
                        refresh(MainLdo);
                    }
                    #endregion
                }
                else if (ldo.drawingObject[item].GetType() == typeof(StringDraw) && ldo.drawingObject[item].As<StringDraw>().IsShown)
                {
                    #region Draw Texts
                    if (LogTreeViewModel.GetItemByName("Texts", globalroot) == null)
                    {
                        globalroot.Children.Add(new LogTreeViewModel("Texts"));
                        LogTreeViewModel.GetItemByName("Texts", globalroot).Parent = globalroot;
                        refresh(MainLdo);
                    }
                    if (LogTreeViewModel.GetItemByName(item, globalroot) == null)
                    {
                        LogTreeViewModel.GetItemByName("Texts", globalroot).Children.Add(new LogTreeViewModel(item));
                        LogTreeViewModel t = LogTreeViewModel.GetItemByName(item, globalroot);
                        t.Parent = LogTreeViewModel.GetItemByName("Texts", globalroot);
                        t.SetIsChecked(ldo.drawingObject[item].As<StringDraw>().IsShown, true, true);
                        refresh(MainLdo);
                    }
                    #endregion
                }
                else if (ldo.drawingObject[item].GetType() == typeof(DrawRegion) && ldo.drawingObject[item].As<DrawRegion>().IsShown)
                {
                    #region Draw Region
                    if (LogTreeViewModel.GetItemByName("Path&Region", globalroot) == null)
                    {
                        globalroot.Children.Add(new LogTreeViewModel("Path&Region"));
                        LogTreeViewModel.GetItemByName("Path&Region", globalroot).Parent = globalroot;
                        refresh(MainLdo);
                    }
                    if (LogTreeViewModel.GetItemByName(item, globalroot) == null)
                    {
                        LogTreeViewModel.GetItemByName("Path&Region", globalroot).Children.Add(new LogTreeViewModel(item));
                        LogTreeViewModel t = LogTreeViewModel.GetItemByName(item, globalroot);
                        t.Parent = LogTreeViewModel.GetItemByName("Path&Region", globalroot);
                        t.SetIsChecked(ldo.drawingObject[item].As<DrawRegion>().IsShown, true, true);
                        refresh(MainLdo);
                    }
                    #endregion
                }

            }
            #endregion
        }

        private void Addt1Tot2(Dictionary<string, object> t1, Dictionary<string, object> t2)
        {
            foreach (var item in t1.Keys)
                if (!t2.ContainsKey(item))
                    t2.Add(item, t1[item]);
        }

        private void refresh(LoggerDrawingObject ld)
        {
            objectTreeView.ItemsSource = ld.ObjectTree.ToList();
        }

        private void PlayAction()
        {
            try
            {
                if (CurrentState == State.Play)
                {
                    mainPanel.fileseekSlider.Value++;
                    mainField.ShowRobotAngle = LogProssesor.ShowRobotAng;
                    mainField.ShowSpeedVector = LogProssesor.ShowSpeedVector;
                    mainField.ShowBallTail = LogProssesor.ShowBallTail;
                    mainField.RobotTail = LogProssesor.ShowRobotTail;
                    mainField.BallTailCount = LogProssesor.BallTailCount;
                    mainField.RobotTailCount = LogProssesor.RobotTailCount;
                    if (mainPanel.fileseekSlider.Value >= ret.Count - 1)
                        CurrentState = State.Stop;
                }
                if (CurrentState == State.Stop)
                {
                    if (reopen)
                    {
                        mainPanel.fileseekSlider.Maximum = (ret.Count == 0) ? 0 : ret.Count - 1;
                        mainPanel.fileseekSlider.Value = 0;
                        reopen = false;
                    }
                    mainPanel.playbuttonImage.SetImageSource("play.png");
                    mainPanel.fileseekSlider.Value = 0;
                    th.Abort();
                    th = null;
                }
                TimeSpan ts = (ret.Count > 0) ? ret[ret.Count - 1].Model.TimeElapsed - ret[0].Model.TimeElapsed : new TimeSpan();
                TimeSpan current = ret[(int)mainPanel.fileseekSlider.Value].Model.TimeElapsed - ret[0].Model.TimeElapsed;
                mainPanel.seekViewerLabel.Content = current.Hours.ToString("D2") + ":" + current.Minutes.ToString("D2") + ":" + current.Seconds.ToString("D2") + " / " + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
                gamestatusLabel.Content = "Game Status : " + ret[(int)mainPanel.fileseekSlider.Value].Model.Status;
                if (list.Count > 0)
                    ballstatusLabel.Content = "Ball Status : " + list[(int)mainPanel.fileseekSlider.Value].BallStatus;
            }
            catch (TargetInvocationException ex)
            {
                Logger.WriteError("---Play Log---\n" + ex.ToString());
            }

        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        #region paint


        void mainField_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                if (drawMode == DrawMode.Line)
                {
                    mainField.AddAnalizeObject(new Line(startloc, e.Location.ToPosition(mainField.Transform), new System.Drawing.Pen(PaintColor, PaintWidth)));
                    MessageBox.Show("Distance :" + startloc.DistanceFrom(e.Location.ToPosition(mainField.Transform)).ToString(), "Line Dist");
                }
                else if (drawMode == DrawMode.Circle)
                    mainField.AddAnalizeObject(new Circle(startloc, e.Location.ToPosition(mainField.Transform).DistanceFrom(startloc), new System.Drawing.Pen(PaintColor, PaintWidth)));

                mouseLeftDown = false;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (mainField.Transform != null)
                    MessageBox.Show("MainField  " + e.Location.ToPosition(mainField.Transform).ToString());
                mouseRightDown = false;
                angle = null;
            }
        }

        void mainField_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                mouseRightDown = true;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                startloc = e.Location.ToPosition(mainField.Transform);
                mouseLeftDown = true;
            }
        }

        void mainField_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (mouseRightDown && mouseLeftDown)
            {
                angle = (float)(e.Location.ToPosition(mainField.Transform) - startloc).AngleInDegrees;
            }
            else if (mouseLeftDown)
            {
                if (drawMode == DrawMode.Line)
                {
                    mainField.DrawLine(new Line(startloc, e.Location.ToPosition(mainField.Transform), new System.Drawing.Pen(PaintColor, PaintWidth)));

                }
                else if (drawMode == DrawMode.Circle)
                {
                    mainField.DrawCircle(new Circle(startloc, e.Location.ToPosition(mainField.Transform).DistanceFrom(startloc), new System.Drawing.Pen(PaintColor, PaintWidth)));
                }

            }
        }

        private void lineButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("lineButton", toolGrid);
            drawMode = DrawMode.Line;

        }

        private void lineButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject();
        }

        private void circleButton_Checked(object sender, RoutedEventArgs e)
        {

            UncheckeOther("circleButton", toolGrid);
            drawMode = DrawMode.Circle;
        }

        private void circleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject();
        }

        private void UncheckeOther(string name, Grid grid)
        {
            List<UIElement> list = grid.Children.Cast<UIElement>().Where(w => w.GetType() == typeof(Telerik.Windows.Controls.RadToggleButton) && ((Telerik.Windows.Controls.RadToggleButton)w).Name != name).ToList();
            list.ForEach(a => ((Telerik.Windows.Controls.RadToggleButton)a).IsChecked = false);
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            mainField.Undo();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            mainField.Clear();
        }

        private void storksmallButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("storksmallButton", strokGrid);
            PaintWidth = 0.01f;
        }

        private void storksmallButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void strokmiddleButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("strokmiddleButton", strokGrid);
            PaintWidth = 0.02f;
        }

        private void strokmiddleButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void stroklargButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("stroklargButton", strokGrid);
            PaintWidth = 0.03f;
        }

        private void stroklargButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void blackButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("blackButton", colorGrid);
            PaintColor = Color.Black;
        }

        private void redButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("redButton", colorGrid);
            PaintColor = Color.Red;
        }

        private void yellowButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("yellowButton", colorGrid);
            PaintColor = Color.Yellow;
        }

        private void greenButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("greenButton", colorGrid);
            PaintColor = Color.GreenYellow;
        }

        private void witeButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("witeButton", colorGrid);
            PaintColor = Color.White;
        }

        private void blueButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("blueButton", colorGrid);
            PaintColor = Color.Blue;
        }
        #endregion

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mainField.Invalidate();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mainField.Invalidate();
        }




    }
}
