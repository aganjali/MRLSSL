using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
namespace MRL.SSL.Planning.GamePlanner.Types
{
    public class GPTeam
    {
        Color _color;
        Dictionary<int, List<Line>> catchBallLines;
        Dictionary<int, List<float>> timeHeads, timeTails;
        //Dictionary<int, GVector2D> distanceFromOurGoal;
        Dictionary<int, GVector2D> distanceFromBall;
        //Dictionary<int, GVector2D> distanceFromOppGoal;
        Dictionary<int, float> goalViewAngle, reflectAngle;
        Distances distance;
        Dictionary<int, double> inDangerousZone;
        Dictionary<int, MarkingType> markingStatesToBall;
        Dictionary<int, MarkingType> markingStatesToTarget;

        public Dictionary<int, MarkingType> MarkingStatesToTarget
        {
            get { return markingStatesToTarget; }
            set { markingStatesToTarget = value; }
        }


        public Dictionary<int, MarkingType> MarkingStatesToBall
        {
            get { return markingStatesToBall; }
            set { markingStatesToBall = value; }
        }

        public Dictionary<int, double> InDangerousZone
        {
            get { return inDangerousZone; }
            set { inDangerousZone = value; }
        }
      
     
        public Distances Distance
        {
            get { return distance; }
            set
            {
                distance = value;
            }
        }
        
        public Dictionary<int, GVector2D> DistanceFromBall
        {
            get { return distanceFromBall; }
            set { distanceFromBall = value; }
        }
        public Dictionary<int, float> ReflectAngle
        {
            get { return reflectAngle; }
            set { reflectAngle = value; }
        }
        Dictionary<int, float> scores;
        int? goaliID, ballOwner;

        public int? BallOwner
        {
            get { return ballOwner; }
            set { ballOwner = value; }
        }

        public int? GoaliID
        {
            get { return goaliID; }
            set { goaliID = value; }
        }
        public Dictionary<int, float> Scores
        {
            get { return scores; }
            set { scores = value; }
        }
        public Dictionary<int, float> GoalViewAngle
        {
            get { return goalViewAngle; }
            set { goalViewAngle = value; }
        }

        public Dictionary<int, List<float>> TimeHeads
        {
            get { return timeHeads; }
            set { timeHeads = value; }
        }
        public Dictionary<int, List<float>> TimeTails
        {
            get { return timeTails; }
            set { timeTails = value; }
        }
        public GPTeam()
        {
            init();
            _color = Color.Yellow;
        }
        public GPTeam(GPTeam gp)
        {
            this.MarkingStatesToBall = gp.MarkingStatesToBall.ToDictionary(k => k.Key, v => v.Value);
            this.MarkingStatesToTarget = gp.MarkingStatesToTarget.ToDictionary(k => k.Key, v => v.Value);
        }
        public GPTeam(Color teamColor)
        {
            init();
            _color = teamColor;
        }
        private void init()
        {
            catchBallLines = new Dictionary<int, List<Line>>();
            timeHeads = new Dictionary<int, List<float>>();
            timeTails = new Dictionary<int, List<float>>();
            distanceFromBall = new Dictionary<int, GVector2D>();
            //distanceFromOppGoal = new Dictionary<int, GVector2D>();
            //distanceFromOurGoal = new Dictionary<int, GVector2D>();
            goalViewAngle = new Dictionary<int, float>();
            scores = new Dictionary<int, float>();
            reflectAngle = new Dictionary<int, float>();
            goaliID = null;
            ballOwner = null;
            distance = new Distances();
            inDangerousZone = new Dictionary<int, double>();
            MarkingStatesToBall = new Dictionary<int, MarkingType>();
            MarkingStatesToTarget = new Dictionary<int, MarkingType>();
        }
        public Dictionary<int, List<Line>> CatchBallLines
        {
            get { return catchBallLines; }
            set { catchBallLines = value; }
        }
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }
    }
    public class Distances
    {
        Dictionary<float, Dictionary<int, float>> distances;
        public Distances()
        {
            distances = new Dictionary<float, Dictionary<int, float>>();
        }
        public Dictionary<int, float> this[float from, float to]
        {
            get
            {
                if (distances == null || distances.Count == 0)
                    return new Dictionary<int, float>();
                if (from <= 0)
                    from = 0;
                if (from >= GameParameters.OurGoalCenter.X - GameParameters.OppGoalCenter.X)
                    from = (float)(GameParameters.OurGoalCenter.X - GameParameters.OppGoalCenter.X);
                from = (float)Math.Round(from, 1);
                if (from - Math.Floor(from) > 0.5)
                    from = (float)Math.Ceiling(from);
                else if (from - Math.Floor(from) < 0.5)
                    from = (float)Math.Floor(from) + 0.5f;

                if (to <= 0)
                    to = 0;
                if (to >= GameParameters.OurGoalCenter.X - GameParameters.OppGoalCenter.X)
                    to = (float)(GameParameters.OurGoalCenter.X - GameParameters.OppGoalCenter.X);
                to = (float)Math.Round(to, 1);
                if (to - Math.Floor(to) > 0.5)
                    to = (float)Math.Ceiling(to);
                else if (to - Math.Floor(to) < 0.5)
                    to = (float)Math.Floor(to) + 0.5f;

                Dictionary<int, float> result = new Dictionary<int, float>();
                distances.Where(w => w.Key > from && w.Key <= to).ToList().ForEach(p => p.Value.ToList().ForEach(q => result.Add(q.Key, q.Value)));
                return result;
                
            }

        }
        public float this[int id, float distFromOwnGoal]
        {
            set
            {
                if (distances != null)
                {
                    distFromOwnGoal = (float)Math.Round(distFromOwnGoal, 1);
                    if (distFromOwnGoal - Math.Floor(distFromOwnGoal) > 0.5)
                        distFromOwnGoal = (float)Math.Ceiling(distFromOwnGoal);
                    else if (distFromOwnGoal - Math.Floor(distFromOwnGoal) < 0.5)
                        distFromOwnGoal = (float)Math.Floor(distFromOwnGoal) + 0.5f;
                    if (distFromOwnGoal > 0 && distFromOwnGoal < GameParameters.OurGoalCenter.X - GameParameters.OppGoalCenter.X)
                    {
                        if (!distances.ContainsKey(distFromOwnGoal))
                        {
                            Dictionary<int, float> dic = new Dictionary<int, float>();
                            dic[id] = value;
                            distances[distFromOwnGoal] = dic;
                        }
                        else
                            distances[distFromOwnGoal][id] = value; 
                    }
                }
            }
            
        }
    }
}
