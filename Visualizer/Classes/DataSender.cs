using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using MRL.SSL.GameDefinitions;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions.Visualizer_Classes;
namespace MRL.SSL.Visualizer.Classes
{
    public static class DataSender
    {
        

        private static bool isInit = false;
        public static bool ThreadIsRun = false, ThreadOnWait = true;
        /// <summary>
        /// sender calss
        /// </summary>
     //   private static CommunicationController _analyzeComcont = new CommunicationController("any", AISettings.Default.VisPort, AISettings.Default.AnalayzerName, AISettings.Default.AnalayzerPort);
        /// <summary>
        /// currend visualizer to ai data class
        /// </summary>
        public static VisualizerToAiWrapper CurrentWrapper = new VisualizerToAiWrapper();
        /// <summary>
        /// data Seneder thread
        /// </summary>
        internal static Thread SendThread = new Thread(new ThreadStart(Sender));
        /// <summary>
        /// Serializer
        /// </summary>
        private static GoogleSerializer googleSerializer = new GoogleSerializer();
        /// <summary>
        /// an auto event handler for send data to Ai
        /// </summary>
        public static AutoResetEvent SendOn = new AutoResetEvent(false);
        public static OptimizationMatrix OPMatrix = new OptimizationMatrix();
        /// <summary>
        /// Sender Function
        /// </summary>
        private static void Sender()
        {
            while (SendOn.WaitOne())
            {
                if (!isInit)
                {
                    DataSender.CurrentWrapper.RequestTable.Add("ActiveSettings", true);
                    DataSender.SendOn.Set(); 
                    isInit = true;
                }
                googleSerializer.SerializeVisToAiWrapper(CurrentWrapper);
                MemoryStream ms = googleSerializer.stream;
                DataReciever._comcontroller.SendData(ms);
           //     _analyzeComcont.SendData(ms);
                CurrentWrapper.SendData.Clear();
                CurrentWrapper.RequestTable.Clear();
                
                //Thread.Sleep(16);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Start()
        {
            if (!ThreadIsRun)
            {
                SendThread = new Thread(new ThreadStart(Sender));
                ThreadIsRun = true;
                SendThread.Start();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Abort()
        {
            if (ThreadIsRun)
            {
                SendThread.Abort();
            }
        }
        /// <summary>
        /// Sended Data event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Data"></param>
        public delegate void DataSendEventHandler(object sender);
        public static event DataSendEventHandler DataSend;
    }
}
