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

using System.Threading;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Visualizer.Extentions;
using System.IO;
using MRL.SSL.GameDefinitions.Visualizer_Classes;
using SlimDX;

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for LogAnalizerWindow.xaml
    /// </summary>
    public partial class LogAnalizerWindow : Window
    {

        List<AiToVisualizerWrapper> FirstLog, SecondLog;
        List<LogCharterData> lcd1;
        List<LogCharterData> lcd2;
        List<LoggerDrawingObject> list1, list2;
        GoogleSerializer gSerialize;
        Thread t1, t2;
        State CurrentState1 = State.Stop;
        State CurrentState2 = State.Stop;

        enum State
        {
            Play,
            Stop,
            Pause
        }

        public LogAnalizerWindow()
        {
            InitializeComponent();
            mainField1.LogAnalizerMode = true;
            mainField2.LogAnalizerMode = true;
            mainField1.LogPlayerMode = true;
            mainField2.LogPlayerMode = true;

            list1 = new List<LoggerDrawingObject>();
            list2 = new List<LoggerDrawingObject>();
            gSerialize = new GoogleSerializer();
            FirstLog = new List<AiToVisualizerWrapper>();
            SecondLog = new List<AiToVisualizerWrapper>();
            mainField1.Transform = new Matrix3x2() { M11 = 0, M12 = 100, M21 = -100, M22 = 0, M31 = 260, M32 = 340 };
            mainField2.Transform = new Matrix3x2() { M11 = 0, M12 = 100, M21 = -100, M22 = 0, M31 = 260, M32 = 340 };
            mainField1.CurrentWrapper = new AiToVisualizerWrapper();
            mainField2.CurrentWrapper = new AiToVisualizerWrapper();
            mainPanel1.openButton.Click += new RoutedEventHandler(panel1_openButton_Click);
            mainPanel1.playButton.Click += new RoutedEventHandler(panel1_playButton_Click);
            mainPanel1.stopButton.Click += new RoutedEventHandler(panel1_stopButton_Click);
            mainPanel1.fileseekSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(panel1_fileseekSlider_ValueChanged);
            lcd1 = new List<LogCharterData>();
            lcd2 = new List<LogCharterData>();
            mainPanel2.openButton.Click += new RoutedEventHandler(panel2_openButton_Click);
            mainPanel2.playButton.Click += new RoutedEventHandler(panel2_playButton_Click);
            mainPanel2.stopButton.Click += new RoutedEventHandler(panel2_stopButton_Click);
            mainPanel2.fileseekSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(panel2_fileseekSlider_ValueChanged);
            mainPanel2.seekViewerLabel.Content = "00:00:00 / 00:00:00";
            mainPanel1.seekViewerLabel.Content = "00:00:00 / 00:00:00";
            LogLoading.load += new LogLoading.LoadedPacket(LogLoading_load);
            LogLoading.loadDrawingObject += new LogLoading.LoadedDrawPacket(LogLoading_loadDrawingObject);
            
        }

        void LogLoading_loadDrawingObject(LoggerDrawingObject sender, string playerTag)
        {
            if (playerTag == "first")
            {
                if (playerTag == "main")
                {
                    list1.Add(sender);
                    Dispatcher.Invoke((Action)(() =>
                    {
                        //DrawTree(ref sender);
                        // list.Add(sender);
                    }));
                }
            }
            else if (playerTag == "second")
            {
                if (playerTag == "main")
                {
                    list2.Add(sender);
                    Dispatcher.Invoke((Action)(() =>
                    {
                        //DrawTree(ref sender);
                        // list.Add(sender);
                    }));
                }
            }
        }

        void LogLoading_load(AiToVisualizerWrapper sender, string playerTag)
        {
            if (playerTag == "first")
            {
                FirstLog.Add(sender);
                Dispatcher.Invoke((Action)(() =>
                {
                   // loadingProgressBar.Value = DrawingObjects.CurrentPositionReaded;

                    mainPanel1.fileseekSlider.Maximum = FirstLog.Count - 1;
                    TimeSpan ts = (FirstLog.Count > 0) ? FirstLog[FirstLog.Count - 1].Model.TimeElapsed - FirstLog[0].Model.TimeElapsed : new TimeSpan();

                    // mainPanel.seekViewerLabel.Content = "00:00:00" + " / " + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
                }));
            }
            else if (playerTag == "second")
            {
                SecondLog.Add(sender);
                Dispatcher.Invoke((Action)(() =>
                {
                    //loadingProgressBar.Value = DrawingObjects.CurrentPositionReaded;

                    mainPanel2.fileseekSlider.Maximum = FirstLog.Count - 1;
                    TimeSpan ts = (SecondLog.Count > 0) ? SecondLog[SecondLog.Count - 1].Model.TimeElapsed - SecondLog[0].Model.TimeElapsed : new TimeSpan();

                    // mainPanel.seekViewerLabel.Content = "00:00:00" + " / " + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
                }));
            }
        }

        void panel1_fileseekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mainField1.CurrentWrapper = FirstLog[(int)mainPanel1.fileseekSlider.Value];
            mainField1.Logdrawingobjects = list1[(int)mainPanel1.fileseekSlider.Value];
        }

        void panel1_stopButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState1 = State.Stop;
        }

        void panel1_playButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentState1 == State.Stop)
            {
                mainPanel1.playbuttonImage.SetImageSource("pause.png");
                CurrentState1 = State.Play;
                t1 = new Thread(new ThreadStart(thread1));
                t1.Start();
            }
            else if (CurrentState1 == State.Play)
            {
                mainPanel1.playbuttonImage.SetImageSource("play.png");
                CurrentState1 = State.Pause;
            }
            else if (CurrentState1 == State.Pause)
            {
                mainPanel1.playbuttonImage.SetImageSource("pause.png");
                CurrentState1 = State.Play;
            }
        }

        void panel1_openButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Multiselect = true;
            //op.InitialDirectory = AISettings.LoggerAddress;
            op.Filter = "Log Files (*.log)|*.log";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            mainPanel1.addressLabel.Content = "Loading... Please Wait";
            Thread openT = new Thread(new ThreadStart(() =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    FileStream logfile = new FileStream(op.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[logfile.Length];
                    logfile.Read(b, 0, (int)logfile.Length);
                    MemoryStream stram = new MemoryStream(b);
                    string comment=""; 
                    gSerialize.DeserializeLogFile(stram,out comment, out list1, out lcd1,"first");
                    mainPanel1.fileseekSlider.Maximum = FirstLog.Count - 1;
                    mainPanel1.fileseekSlider.Value = 0;
                    //TimeSpan ts = (FirstLog.Count > 0) ? FirstLog[FirstLog.Count - 1].Model.TimeElapsed - FirstLog[0].Model.TimeElapsed : new TimeSpan();
                   // headerlLabel.Content = "Comment : " + comment + " - Lenght : " + ts.Hours.ToString("D3") + ":" + ts.Minutes.ToString("D3") + ":" + ts.Seconds.ToString("D3");
                   // mainPanel1.seekViewerLabel.Content = "00:00:00" + " / " + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
                  //  mainPanel1.addressLabel.Content = "Ready";
                }));
            }));
            openT.Start();
        }

        void panel2_fileseekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mainField2.CurrentWrapper = SecondLog[(int)mainPanel2.fileseekSlider.Value];
            mainField2.Logdrawingobjects = list2[(int)mainPanel2.fileseekSlider.Value];
        }

        void panel2_stopButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState2 = State.Stop;
        }

        void panel2_playButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentState2 == State.Stop)
            {
                mainPanel2.playbuttonImage.SetImageSource("pause.png");
                CurrentState2 = State.Play;
                t2 = new Thread(new ThreadStart(thread2));
                t2.Start();
            }
            else if (CurrentState2 == State.Play)
            {
                mainPanel2.playbuttonImage.SetImageSource("play.png");
                CurrentState2 = State.Pause;
            }
            else if (CurrentState2 == State.Pause)
            {
                mainPanel2.playbuttonImage.SetImageSource("pause.png");
                CurrentState2 = State.Play;
            }
        }

        void panel2_openButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Multiselect = true;
           // op.InitialDirectory = AISettings.LoggerAddress;
            op.Filter = "Log Files (*.log)|*.log";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            mainPanel2.addressLabel.Content = "Loading... Please Wait";
            Thread openT = new Thread(new ThreadStart(() =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    FileStream logfile = new FileStream(op.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[logfile.Length];
                    logfile.Read(b, 0, (int)logfile.Length);
                    MemoryStream stram = new MemoryStream(b);
                    string comment="";
                    gSerialize.DeserializeLogFile(stram,out comment ,out list2, out lcd2,"second");
                    mainPanel2.fileseekSlider.Maximum = SecondLog.Count - 1;
                    mainPanel2.fileseekSlider.Value = 0;
                    //TimeSpan ts = (SecondLog.Count > 0) ? SecondLog[SecondLog.Count - 1].Model.TimeElapsed - SecondLog[0].Model.TimeElapsed : new TimeSpan();
                    //header2Label.Content = "Comment : " + comment + " - Lenght : " + ts.Hours.ToString("D3") + ":" + ts.Minutes.ToString("D3") + ":" + ts.Seconds.ToString("D3");
                    //mainPanel2.seekViewerLabel.Content = "00:00:00" + " / " + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
                    //mainPanel2.addressLabel.Content = "Ready";
                }));
            }));
            openT.Start();
        }

        private void thread1()
        {
            while (true)
            {
                Dispatcher.Invoke((Action)(() => PlayAction1()));
                Thread.Sleep(16);
            }
        }

        private void thread2()
        {
            while (true)
            {
                Dispatcher.Invoke((Action)(() => PlayAction2()));
                Thread.Sleep(16);
            }
        }

        private void PlayAction1()
        {
            if (CurrentState1 == State.Play)
            {
                mainPanel1.fileseekSlider.Value++;

                if (mainPanel1.fileseekSlider.Value >= FirstLog.Count - 1)
                    CurrentState1 = State.Stop;
            }
            if (CurrentState1 == State.Stop)
            {
                mainPanel1.playbuttonImage.SetImageSource("play.png");
                mainPanel1.fileseekSlider.Value = 0;
                t1.Abort();
                t1 = null;
            }
            TimeSpan ts = (FirstLog.Count > 0) ? FirstLog[FirstLog.Count - 1].Model.TimeElapsed - FirstLog[0].Model.TimeElapsed : new TimeSpan();
            TimeSpan current = FirstLog[(int)mainPanel1.fileseekSlider.Value].Model.TimeElapsed - FirstLog[0].Model.TimeElapsed;
            mainPanel1.seekViewerLabel.Content = current.Hours.ToString("D2") + ":" + current.Minutes.ToString("D2") + ":" + current.Seconds.ToString("D2") + " / " + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
            gamestatus1Label.Content = "Game Status : " + FirstLog[(int)mainPanel1.fileseekSlider.Value].Model.Status;
            ballstatus1Label.Content = "Ball Status : " + list1[(int)mainPanel1.fileseekSlider.Value].BallStatus;
        }

        private void PlayAction2()
        {
            if (CurrentState2 == State.Play)
            {
                mainPanel2.fileseekSlider.Value++;

                if (mainPanel2.fileseekSlider.Value >= SecondLog.Count - 1)
                    CurrentState2 = State.Stop;
            }
            if (CurrentState2 == State.Stop)
            {
                mainPanel2.playbuttonImage.SetImageSource("play.png");
                mainPanel2.fileseekSlider.Value = 0;
                t2.Abort();
                t2 = null;
            }
            TimeSpan ts = (SecondLog.Count > 0) ? SecondLog[SecondLog.Count - 1].Model.TimeElapsed - SecondLog[0].Model.TimeElapsed : new TimeSpan();
            TimeSpan current = SecondLog[(int)mainPanel2.fileseekSlider.Value].Model.TimeElapsed - SecondLog[0].Model.TimeElapsed;
            mainPanel2.seekViewerLabel.Content = current.Hours.ToString("D2") + ":" + current.Minutes.ToString("D2") + ":" + current.Seconds.ToString("D2") + " / " + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
            gamestatus2Label.Content = "Game Status : " + FirstLog[(int)mainPanel2.fileseekSlider.Value].Model.Status;
            ballstatus2Label.Content = "Ball Status : " + list1[(int)mainPanel2.fileseekSlider.Value].BallStatus;
        }
    }
}
