using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// A Model that describes the world of ssl robot
    /// </summary>
    public class WorldModel
    {
        public WorldModel()
        {

        }

        public WorldModel(WorldModel Model)
        {
            if (Model.BallStateFast != null)
                BallStateFast = new SingleObjectState(Model.BallStateFast);
            BallStateSlow = Model.BallStateSlow;
            BallState = new SingleObjectState(Model.BallState);
            BallHeight = Model.BallHeight;
            if (Model.OurRobots != null)
                OurRobots = Model.OurRobots.Clone();
            if (Model.Opponents != null)
                Opponents = Model.Opponents.Clone();
            TimeOfAction = Model.TimeOfAction;
            SequenceNumber = Model.SequenceNumber;
            BallConfidenc = Model.BallConfidenc;
            GoalieID = Model.GoalieID;
            Status = Model.Status;
            OurScore = Model.OurScore;
            OpponentScore = Model.OpponentScore;
            TimeElapsed = Model.TimeElapsed;
            OurMarkerISYellow = Model.OurMarkerISYellow;
            FieldIsInverted = Model.FieldIsInverted;
            GlobalKickingProhibited = Model.GlobalKickingProhibited;
            CurrentVisionPacket0 = Model.CurrentVisionPacket0;
            CurrentVisionPacket1 = Model.CurrentVisionPacket1;
            FirstBallCatchingPoint = Model.FirstBallCatchingPoint;
            predictedBall = new PredictedStates();
            predictedBall.states = Model.predictedBall.states.Clone();
            lastVelocity = Model.lastVelocity.ToDictionary(p => p.Key, q => q.Value);
            lastW = Model.lastW.ToDictionary(p => p.Key, q => q.Value);
            BallFallingPoint = Model.BallFallingPoint;
            IsChip = Model.IsChip;
        }
        public void SetMarkingStates(WorldModel Model)
        {
            markingStatesToBall = Model.markingStatesToBall.ToDictionary(k => k.Key, v => v.Value);
            markingStatesToTarget = Model.markingStatesToTarget.ToDictionary(k => k.Key, v => v.Value);
        }
        public static WorldModel CurrentWorldModel, PreviousWorldModel;
        public SingleObjectState BallState, BallStateSlow, BallStateFast;

        public Dictionary<int, int> markingStatesToBall;
        public Dictionary<int, int> markingStatesToTarget;
       
        public double? BallHeight;
        public Dictionary<int, SingleObjectState> OurRobots, Opponents;
        public DateTime TimeOfAction;
        public int SequenceNumber;
        public double BallConfidenc;
        public int? GoalieID;
        public GameStatus Status;
        public int OurScore, OpponentScore;
        public TimeSpan TimeElapsed;
        public bool OurMarkerISYellow, FieldIsInverted, GlobalKickingProhibited ,IsChip;
        public Dictionary<int, Vector2D> lastVelocity = new Dictionary<int, Vector2D>();
        public Dictionary<int, double> lastW = new Dictionary<int, double>();
        public TimeSpan TimeLeft;
        public messages_robocup_ssl_wrapper.SSL_WrapperPacket CurrentVisionPacket0;
        public messages_robocup_ssl_wrapper.SSL_WrapperPacket CurrentVisionPacket1;
        public messages_robocup_ssl_wrapper.SSL_WrapperPacket CurrentVisionPacket2;
        public messages_robocup_ssl_wrapper.SSL_WrapperPacket CurrentVisionPacket3;
        public MRL.SSL.CommonClasses.MathLibrary.Position2D? FirstBallCatchingPoint = null;
        PredictedStates predictedBall = new PredictedStates();
        public Position2D BallFallingPoint;

        public PredictedStates PredictedBall
        {
            get { return predictedBall; }
            set { predictedBall = value; }
        }

    }
    public class PredictedStates
    {
        public List<SingleObjectState> states;
        public PredictedStates()
        {
            states = new List<SingleObjectState>();
        }
        public static double FRAME_RATE = 60;
        public static double FRAME_PERIOD = 1 / FRAME_RATE;
        /// <summary>
        /// Get Predicted Ball State In Specified Time
        /// </summary>
        /// <param name="time"> time in second </param>
        /// <returns></returns>
        public SingleObjectState this[double time]
        {
            get
            {
                if (states.Count == 0)
                    return new SingleObjectState();
                int idx = (int)Math.Round(time / FRAME_PERIOD);
                if (idx > states.Count - 1)
                    idx = states.Count - 1;
                if (idx < 0)
                    idx = 0;
                return states[idx];
            }

        }
        public double this[Position2D pos]
        {
            get
            {
                if (states.Count == 0)
                    return 0;
                Vector2D ballDirection = (states.Last().Location - states.First().Location);
                Position2D ball = states.First().Location;
                int i = states.Count;
                double t = 1.5, dt = -1.5, alfa=0;
                while (i > 0)
                {
                    i /= 2;
                    dt *= 0.5;
                    alfa = t + dt;
                    Position2D tmpPos = this[alfa].Location;
                    Vector2D tmpVec = tmpPos - pos;
                    if ( tmpVec.InnerProduct(ballDirection) >= 0)
                        t = alfa;
                }
                return alfa;
            }

        }
    }
}
