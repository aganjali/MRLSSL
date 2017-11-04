using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class MergerAndTrackerSetting
    {
        /// <summary>
        /// 
        /// </summary>
        private bool _correctAngleError = true;
        public bool CorrectAngleError
        {
            get { return _correctAngleError; }
            set { _correctAngleError = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private bool _onGame = true;
        public bool OnGame
        {
            get { return _onGame; }
            set { _onGame = value; }
        }
        int _maxFrameToShadow = 30;
        /// <summary>
        /// Maximum permisible frame rate to calculate adn draw shadow region
        /// </summary>
        public int MaxFrameToShadow
        {
            get { return _maxFrameToShadow; }
            set { _maxFrameToShadow = value; }
        }
        double _actionDelay = 0.04;
        /// <summary>
        /// Determine delay of action
        /// </summary>
        public double ActionDelay
        {
            get { return _actionDelay; }
            set { _actionDelay = value; }
        }
        double _maxBallDist = 0.4;
        /// <summary>
        /// Maximum movement in one frame for ball
        /// </summary>
        public double MaxBallDist
        {
            get { return _maxBallDist; }
            set { _maxBallDist = value; }
        }
        bool _calculateRegion = false;
        /// <summary>
        /// Draw shadow region
        /// </summary>
        public bool CalculateRegion
        {
            get { return _calculateRegion; }
            set { _calculateRegion = value; }
        }
        CameraState _camState = CameraState.All;
        /// <summary>
        /// Select witch Camera(s) Must be influenced 
        /// </summary>
        public CameraState CamState
        {
            get { return _camState; }
            set { _camState = value; }
        }
        int _maxNotSeen = 120;
        /// <summary>
        /// Maximum permissible frame before eliminating object from history 
        /// </summary>
        public int MaxNotSeen
        {
            get { return _maxNotSeen; }
            set { _maxNotSeen = value; }
        }
        int _maxToImagine = 1;
        /// <summary>
        /// Maximum Frame to imagine robotes whenever not seen
        /// </summary>
        public int MaxToImagine
        {
            get { return _maxToImagine; }
            set { _maxToImagine = value; }
        }
        double _maxOpponenetDistance = 0.2;
        /// <summary>
        ///  Maximum movement in one frame for Opponenets
        /// </summary>
        public double MaxOpponenetDistance
        {
            get { return _maxOpponenetDistance; }
            set { _maxOpponenetDistance = value; }
        }
        /// <summary>
        /// Camera States
        /// </summary>
        public enum CameraState
        {
            All = 0,
            Cam0=1,
            Cam1=2,
        }
    }
}
