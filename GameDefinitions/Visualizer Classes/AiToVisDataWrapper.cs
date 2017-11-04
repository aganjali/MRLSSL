using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses;
using MRL.SSL.CommonClasses.MathLibrary;


namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// A wrapper that send from AI PC to Visualizer PC contains WorldModel and VisualizerData
    /// </summary>
    public class AiToVisDataWrapper
    {
        public AiToVisDataWrapper()
        {
            _model = new WorldModel();
        }
        string _ballStatus;
        /// <summary>
        /// 
        /// </summary>
        public string BallStatus
        {
            get { return _ballStatus; }
            set { _ballStatus = value; }
        }
        /// <summary>
        /// Constract and initial model and VisualizerData
        /// </summary>
        /// <param name="Model">Input world model</param>
        public AiToVisDataWrapper(WorldModel Model)
        {
            _model = Model;
        }
        /// <summary>
        /// Construct adn initial model 
        /// </summary>
        private WorldModel _model;
        /// <summary>
        /// model
        /// </summary>
        public WorldModel Model
        {
            get { return _model; }
            set { _model = value; }
        }
        private Dictionary<string, double> _customVariables;
        /// <summary>
        /// Variables that users want to tune in AI,Declare in cunstructor
        /// </summary>
        public Dictionary<string, double> CustomVariables
        {
            get { return _customVariables; }
            set { _customVariables = value; }
        }
        private Dictionary<int, Position2D> _Balls;
        /// <summary>
        /// Balls Position & IDs
        /// </summary>
        public Dictionary<int, Position2D> Balls
        {
            get { return _Balls; }
            set { _Balls = value; }
        }
        private Dictionary<string, bool> _requestTable;
        /// <summary>
        /// a table for requests in customized data commiunication
        /// </summary>
        public Dictionary<string, bool> RequestTable
        {
            get { return _requestTable; }
            set { _requestTable = value; }
        }
        private Dictionary<string, int> _techniques;
        /// <summary>
        /// Techniques that send to visualizer
        /// </summary>
        public Dictionary<string, int> Techniques
        {
            get { return _techniques; }
            set { _techniques = value; }
        }
    }
}
