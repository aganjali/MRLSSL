using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;
namespace MRL.SSL.Planning.GamePlanner
{
    public class GamePlannerEngine : IDisposable
    {

        private List<MRL.SSL.GameDefinitions.WorldModel> _modelHistory;
        private const int _historyCount = 100;
        BallState ballstate;
        Scoring scoring;
        DistanceAndAngle distAndAngle;
        GamePlannerInfo gpInfo, lastGpInfo;
        bool started = false;

        public GamePlannerInfo LastGPInfo
        {
            get { return lastGpInfo; }
            set { lastGpInfo = value; }
        }
        public GamePlannerInfo GPInfo
        {
            get { return gpInfo; }
            set { gpInfo = value; }
        }

        public GamePlannerEngine()
        {
            gpInfo = new GamePlannerInfo();
            lastGpInfo = new GamePlannerInfo();
        }
        public void Start()
        {
            if (!started)
            {
                _modelHistory = new List<MRL.SSL.GameDefinitions.WorldModel>();
                ballstate = new BallState();
                scoring = new Scoring();
                distAndAngle = new DistanceAndAngle();
                gpInfo = new GamePlannerInfo();
                lastGpInfo = new GamePlannerInfo();
                started = true;
            }
        }
        public GamePlannerInfo CalculateInfo(MRL.SSL.GameDefinitions.WorldModel Model, bool logViewer)
        {
            if (_modelHistory != null)
            {
                if (_modelHistory.Count >= _historyCount)
                    _modelHistory.RemoveAt(0);
                _modelHistory.Add(Model);
                gpInfo = new GamePlannerInfo();
                GVector2D BallSpeedGlobal = Model.BallState.Speed;
                ballstate.GoodBallCatchingLines(_modelHistory, BallSpeedGlobal, ref gpInfo);

                scoring.CalculateRobotsScores(_modelHistory, BallSpeedGlobal, ref gpInfo, lastGpInfo, logViewer);
                
                distAndAngle.Calculate(_modelHistory, BallSpeedGlobal, ref gpInfo);
                lastGpInfo = new GamePlannerInfo();
                lastGpInfo = gpInfo;
            }
            return gpInfo;
        }

        public void Dispose()
        {
            scoring.Dispose();
            ballstate.Dispose();
            started = false;
        }

    }
}
