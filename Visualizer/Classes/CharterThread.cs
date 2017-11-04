using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MRL.SSL.Visualizer.Classes
{
    public static class CharterThread
    {
        public static int Delay = 10;
        public static bool waitting = true;
        public static ManualResetEvent Event;
        private static Thread thread;
        
        public static void InitialThread()
        {
            thread = new Thread(new ThreadStart(action));
            Event = new ManualResetEvent(false);
            
            thread.Start();
        }
        public static void ChangState()
        {
            if (waitting)
            {
                Event.Set();
                waitting = false;
            }
            else
            {
                Event.Reset();
                waitting = true;
            }
        }
        public static void Abort()
        {
            thread.Abort();
            thread = null;
        }
        private static void action()
        {
            while (Event.WaitOne())
            {
                if(ShowData!=null)
                    ShowData(null);
                Thread.Sleep(16 + Delay);
            }
        }

        public delegate void ChartData(object sender);
        public static event ChartData ShowData;

    }
}
