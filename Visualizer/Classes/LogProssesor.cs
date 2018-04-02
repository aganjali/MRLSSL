using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.Extentions;
using MRL.SSL.Visualizer.Classes;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Windows.Threading;
using System.Diagnostics;

namespace Visualizer.Classes
{
    public static class LogProssesor
    {
        public static bool MomentName = false;
        public static int RobotTailCount = 300;
        /// <summary>
        /// 
        /// </summary>
        public static int BallTailCount = 300;
        /// <summary>
        /// 
        /// </summary>
        public static bool ShowRobotAng = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool ShowSpeedVector = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool ShowRobotTail = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool ShowBallTail = true;
        /// <summary>
        /// 
        /// </summary>
        public static string Comment = "without Comment";
        /// <summary>
        /// 
        /// </summary>
        public static Stopwatch LoggerTime = new Stopwatch();
        /// <summary>
        /// 
        /// </summary>
        static public bool VisionLog = false;
        /// <summary>
        /// 
        /// </summary>
        static public int LoggerDelay = 1;
        /// <summary>
        /// 
        /// </summary>
        public static bool loggerfinished = true;
        /// <summary>
        /// 
        /// </summary>
        public static List<AiToVisualizerWrapper> ListMemoryLog = new List<AiToVisualizerWrapper>();
        /// <summary>
        /// 
        /// </summary>
        public static AiToVisualizerWrapper MemoryLog 
        {
            set
            {
                ListMemoryLog.Add(value);
                if (ListMemoryLog.Count > 600)
                    ListMemoryLog.RemoveAt(0);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private static AiToVisualizerWrapper _currentReaded = new AiToVisualizerWrapper();
        public static AiToVisualizerWrapper CurrentReaded
        {
            set
            {
                _currentReaded = value;
                if (_currentReaded != null && RefreshedData != null)
                    RefreshedData(null);
            }
            get
            {
                return _currentReaded;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static FileStream logfile;
        /// <summary>
        /// Qeue of memory stream that must be save to disk
        /// </summary>
        public static Queue<MemoryStream> LogPacketsQeue = new Queue<MemoryStream>();
        /// <summary>
        /// current memory stream that must be add to qeue
        /// </summary>
        private static MemoryStream _current;
        public static MemoryStream Current
        {
            set
            {
                LogProssesor._current = value;
                LogPacketsQeue.Enqueue(LogProssesor._current);
            }
        }
        /// <summary>
        /// default log address(mydocument\MRLLog)
        /// </summary>
        private static string DefaultAddress = Path.Combine(@"D:\" /*Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)*/, "MRLLog");
        /// <summary>
        /// control flag
        /// </summary>
        public static bool UseDefaulName = true, UseDefaultAddress = true;
        /// <summary>
        /// user log filename
        /// </summary>
        public static string UserFileName = "userFileName";
        /// <summary>
        /// user log address
        /// </summary>
        public static string UserLogAddress = @"c:\";
        /// <summary>
        /// Main thread for give log from data
        /// </summary>
        static Thread mainThread = null;
        /// <summary>
        /// manual event handler for logger thread
        /// </summary>
        static ManualResetEvent LogSingnal = new ManualResetEvent(false);
        /// <summary>
        /// control flag
        /// </summary>
        static bool isStarted = false, wait = true;
        /// <summary>
        /// start logger
        /// </summary>
        public static void StartLog()
        {
            StartThread();
            if (wait)
            {
                LogSingnal.Set();
                wait = false;
                LoggerTime.Reset();
                LoggerTime.Start();
            }
        }
        /// <summary>
        /// pause logger
        /// </summary>
        public static void PauseLog()
        {
            if (mainThread.ThreadState == System.Threading.ThreadState.Running)
            {
                if (!wait)
                {
                    LogSingnal.Reset();
                    wait = true;
                    LoggerTime.Stop();
                }
            }
        }
        /// <summary>
        /// start new thread and dispose last thread
        /// </summary>
        public static void StartThread()
        {
            if (isStarted) return;
            isStarted = true;
            mainThread = new Thread(new ThreadStart(Logger));
            LogPacketsQeue = new Queue<MemoryStream>();
            mainThread.Start();
        }
        /// <summary>
        /// abort thread
        /// </summary>
        public static void Abort()
        {
            mainThread.Abort();
            wait = true;
        }
        /// <summary>
        /// Stop and save
        /// </summary>
        public static void StopAndSave()
        {
            if (logfile != null)
            {
                LoggerTime.Stop();
                LogSingnal.Reset();
                isStarted = false;
                loggerfinished = false;
                Thread t = new Thread(new ThreadStart(FishishLogging));
                t.Start();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private static void FishishLogging()
        {
            while (LogPacketsQeue.Count > 0)
            {
                MemoryStream s = LogPacketsQeue.Dequeue();
                logfile.Write(s.ToArray(), 0, s.ToArray().Length);
                Thread.Sleep(LoggerDelay);
            }
        
            logfile.Flush();
            logfile.Close();
            logfile = null;
            LogPacketsQeue = new Queue<MemoryStream>();
            loggerfinished = true;
            if (loggerFinished != null)
                loggerFinished(null);
        }
        /// <summary>
        /// main method to give log form data
        /// </summary>
        private static void Logger()
        {
            if (!Directory.Exists(DefaultAddress))
                Directory.CreateDirectory(DefaultAddress);
            if (!Directory.Exists(UserLogAddress))
                Directory.CreateDirectory(UserLogAddress);
            string name = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString() + "." + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ".log";
            if (UseDefaultAddress && UseDefaulName)
                logfile = new FileStream(Path.Combine(DefaultAddress, name), FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            else if (UseDefaulName && !UseDefaultAddress)
                logfile = new FileStream(Path.Combine(UserLogAddress, name), FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            else if (!UseDefaulName && UseDefaultAddress)
                logfile = new FileStream(Path.Combine(DefaultAddress, UserFileName + ".log"), FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            else
                logfile = new FileStream(Path.Combine(UserLogAddress, UserFileName), FileMode.Create, FileAccess.ReadWrite, FileShare.None);


            GoogleSerializer gs = new GoogleSerializer();
            gs.Serialize(Comment);
            logfile.Write(gs.stream.ToArray(), 0, gs.stream.ToArray().Length);
            while (LogSingnal.WaitOne())
            {
                if (LogPacketsQeue.Count > 0)
                {
                    MemoryStream s = LogPacketsQeue.Dequeue();
                    logfile.Write(s.ToArray(), 0, s.ToArray().Length);
                    Thread.Sleep(LoggerDelay);
                }
            }
            if (MomentName)
            {
                UseDefaulName = true;
                MomentName = false;
            }
        }
        static void Save()
        {
            //visua
        }

        public delegate void ShowLogWrapperEventHandler(object sender);
        public static event ShowLogWrapperEventHandler RefreshedData;

        public delegate void LoggerFinishedEventHandler(object sender);
        public static event LoggerFinishedEventHandler loggerFinished;
    }
}
