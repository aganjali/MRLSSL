using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MRL.SSL.GameDefinitions.Visualizer_Classes
{
    public static class LogLoading
    {
        public static string CurrentLogPlayerTag = "";
        private static AiToVisualizerWrapper _loaded = new AiToVisualizerWrapper();

        public static AiToVisualizerWrapper Loaded
        {
            get { return LogLoading._loaded; }
            set 
            { 
                LogLoading._loaded = value;
                if (load != null)
                    load(_loaded, CurrentLogPlayerTag);
            }
        }

        private static LoggerDrawingObject _LoadedDrawingObject = new LoggerDrawingObject();

        public static LoggerDrawingObject LoadedDrawingObject
        {
            get { return LogLoading._LoadedDrawingObject; }
            set { 
                LogLoading._LoadedDrawingObject = value;
                if (loadDrawingObject != null)
                    loadDrawingObject(_LoadedDrawingObject, CurrentLogPlayerTag);
            }
        }

        public delegate void LoadedDrawPacket(LoggerDrawingObject sender,string playerTag);
        public static event LoadedDrawPacket loadDrawingObject;

        public delegate void LoadedPacket(AiToVisualizerWrapper sender,string playerTag);
        public static event LoadedPacket load;
        
    }
}
