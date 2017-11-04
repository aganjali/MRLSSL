using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class AiToVisualizerWrapper
    {
        public bool StrategySended { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool SendActiveSetting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool SendTuneVariables { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool GameParametersSend { get; set; }
        /// <summary>
        /// Model that send to Visualizer
        /// </summary>
        public WorldModel Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WorldModel GlobalModel { get; set; }
        /// <summary>
        /// All balls that seen vision 
        /// </summary>
        public Dictionary<int,Position2D> AllBalls { get; set; }
        /// <summary>
        /// Techniques
        /// </summary>
        public Dictionary<string,string> Techniques { get; set; }
        /// <summary>
        /// Engines
        /// </summary>
        public Dictionary<int,Engines> Engines { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<int, SingleWirelessCommand> RobotCommnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public RobotData MtrixData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BallStatus { get; set; }

    }
}
