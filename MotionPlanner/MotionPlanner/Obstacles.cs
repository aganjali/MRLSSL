using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.Planning.MotionPlanner
{
    public class Obstacles
    {
        const double marginRobot = 0.1, marginBall = 0.1, marginOppZone = 0.2;
        Dictionary<int, Obstacle> obstaclesList = new Dictionary<int, Obstacle>();
        public Dictionary<int, Obstacle> ObstaclesList
        {
            get { return obstaclesList; }
            set { obstaclesList = value; }
        }
        WorldModel Model ;
        int avoidDangerZone = 0, avoidOppZone = 0, avoidRobots = 0, avoidBall = 0;

        public int AvoidOppZone
        {
            get { return avoidOppZone; }
            set
            {
                if (value != 0)
                    AddOppZone();
                else
                    RemoveOppZone();
            }
        }

        public int AvoidRobots
        {
            get { return avoidRobots; }
            set 
            {
                if (value != 0)
                    AddObstacle(1, 0, 0, 0, null, null, false);
                else
                    RemoveRobots();
            }
        }

        public int AvoidDangerZone
        {
            get { return avoidDangerZone; }
            set 
            {
                if(value != 0)
                    AddDangerZone(); 
                else
                    RemoveZone();
            }
        }

        public int AvoidBall
        {
            get { return avoidBall; }
            set 
            {
                if (value != 0)
                    AddBall(Model.BallState, false);
                else
                    RemoveBall();
            }
        }
        
        public Obstacles(WorldModel Model):this()
        {
            this.Model = Model;
        }
        public Obstacles()
        {
            this.Model = new WorldModel();
            avoidDangerZone = 0;
            avoidRobots = 0; 
            avoidBall = 0;
            avoidOppZone = 0;
        }

        public void Clear()
        {
            obstaclesList.Clear();
            avoidBall = 0;
            avoidDangerZone = 0;
            avoidRobots = 0;
            avoidOppZone = 0;
        }
        public void AddVirtualObstacle(Obstacle obs)
        {
            int id = -8;
            if (obstaclesList.Count > 0)
            {
                id = obstaclesList.Keys.OrderBy(o => o).First();
                if (id <= -8)
                    id -= 1;
            }
            if (obs.Type == ObstacleType.Circle)
            {
                AddCircle(obs.State, obs.R.X, obs.Margin, id);
            }
            else if(obs.Type == ObstacleType.Rectangle)
            {
                AddRectangle(obs.State, obs.R.X , obs.R.Y, obs.Margin, id);
            }
        }
        public void AddCircle(Position2D p, double Radi, double margin, int id)
        {
            id = (id > -8) ? -8 : id;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.Circle;
            obs.R = new Vector2D(Radi, Radi);
            obs.Margin = margin;
            obs.State = new SingleObjectState(p, new Vector2D(), null);
            obstaclesList[id] =  obs;
        }
        public void AddCircle(SingleObjectState s, double Radi, double margin, int id)
        {
            id = (id > -8) ? -8 : id;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.Circle;
            obs.R = new Vector2D(Radi, Radi);
            obs.Margin = margin;
            obs.State = new SingleObjectState(s);
            obstaclesList[id] = obs;
        }
    
        public void AddRectangle(Position2D p, double width, double height, double margin, int id)
        {
            id = (id > -8) ? -8 : id;
            Obstacle obs = new Obstacle();
            obs.Margin = margin;
            obs.Type = ObstacleType.Rectangle;
            obs.R = new Vector2D(width / 2, height / 2);
            obs.State = new SingleObjectState(p, new Vector2D(), null);
            obstaclesList[id] = obs;
        }
        public void AddRectangle(SingleObjectState s, double width, double height, double margin, int id)
        {
            id = (id > -8) ? -8 : id;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.Rectangle;
            obs.R = new Vector2D(width / 2, height / 2);
            obs.Margin = margin;
            obs.State = new SingleObjectState(s);
            obstaclesList[id] = obs;
        }
    
        public void AddRobot(SingleObjectState s,bool Our, int id)
        {
            id = Math.Max(0, id);
            id = (Our) ? id : id + 16;
            avoidRobots = 1;
            Obstacle obs = new Obstacle();
            obs.Type = (Our) ? ObstacleType.OurRobot : ObstacleType.OppRobot;
            obs.Margin = marginRobot;
            obs.R = new Vector2D(MotionPlannerParameters.RobotRadi , MotionPlannerParameters.RobotRadi );
            obs.State = new SingleObjectState(s);
            obstaclesList[id] = obs ;
        }
        public void AddRobot(SingleObjectState s, bool Our, int id, double kSpeedRobot)
        {
            id = Math.Max(0, id);
            id = (Our) ? id : id + 16;
            avoidRobots = 1;
            Obstacle obs = new Obstacle();
            obs.Margin = marginRobot;
            obs.Type = (Our) ? ObstacleType.OurRobot : ObstacleType.OppRobot;
            obs.R = new Vector2D(MotionPlannerParameters.RobotRadi , MotionPlannerParameters.RobotRadi);
            obs.State = new SingleObjectState(s.Location + kSpeedRobot * s.Speed, s.Speed, 0);
            obstaclesList[id] = obs;
        }
        public void AddRobot(Position2D p,bool Our, int id)
        {
            id = Math.Max(0, id);
            id = (Our) ? id : id + 16;
            avoidRobots = 1;
            Obstacle obs = new Obstacle();
            obs.Margin = marginRobot;
            obs.Type = (Our) ? ObstacleType.OurRobot : ObstacleType.OppRobot;
            obs.R = new Vector2D(MotionPlannerParameters.RobotRadi , MotionPlannerParameters.RobotRadi);
            obs.State = new SingleObjectState(p, new Vector2D(), null);
            obstaclesList[id] = obs;
        }
    
        public void AddBall(Position2D p, bool stopBall)
        {
            int id = -1;
            avoidBall = 1;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.Ball;
            obs.Margin = +marginBall;
            if (!stopBall)
            {
                obs.R = new Vector2D(MotionPlannerParameters.BallRadi , MotionPlannerParameters.BallRadi );    
            }
            else
                obs.R = new Vector2D(MotionPlannerParameters.StopBallRadi , MotionPlannerParameters.StopBallRadi );
            obs.State = new SingleObjectState(p, new Vector2D(), null);
            obstaclesList[id] = obs;
        }
        public void AddBall(SingleObjectState s, bool stopBall)
        {
            int id = -1;
            avoidBall = 1;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.Ball;
            obs.Margin = +marginBall;
            if (!stopBall)
            {
                obs.R = new Vector2D(MotionPlannerParameters.BallRadi , MotionPlannerParameters.BallRadi );
            }
            else
                obs.R = new Vector2D(MotionPlannerParameters.StopBallRadi , MotionPlannerParameters.StopBallRadi );
            obs.State = new SingleObjectState(s);
            obstaclesList[id] = obs;
        }
        public void AddBall(SingleObjectState s, double kSpeedBall, bool stopBall)
        {
            int id = -1;
            avoidBall = 1;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.Ball;
            obs.Margin = +marginBall;
            if (!stopBall)
            {
                obs.R = new Vector2D(MotionPlannerParameters.BallRadi , MotionPlannerParameters.BallRadi );
            }
            else
                obs.R = new Vector2D(MotionPlannerParameters.StopBallRadi , MotionPlannerParameters.StopBallRadi );
            obs.State = new SingleObjectState(s.Location + kSpeedBall * s.Speed, s.Speed, 0);
            obstaclesList[id] = obs;
        }
    
        public void AddDangerZone()
        {
            avoidDangerZone = 1;
            Obstacle obs = new Obstacle();
            int id = -2;
            obs.Type = ObstacleType.ZoneRectangle;
            obs.Margin = 0;
            obs.R = new Vector2D(MotionPlannerParameters.DangerZoneH / 2, MotionPlannerParameters.DangerZoneW / 2);
            obs.State = new SingleObjectState(GameParameters.OurGoalCenter - new Vector2D(MotionPlannerParameters.DangerZoneH / 2, 0), new Vector2D(), null);
            obstaclesList[id] = (obs);

            //int id = -2;
            //obs.Type = ObstacleType.ZoneCircle;
            //obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW, MotionPlannerParameters.DangerZoneW);
            //obs.State = new SingleObjectState(GameParameters.OurGoalCenter , new Vector2D(), null);
            //obstaclesList[id] = (obs);
            ////obs.Type = ObstacleType.ZoneRectangle;
            ////obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW / 2, MotionPlannerParameters.DangerZoneH / 2);
            ////obs.State = new SingleObjectState(GameParameters.OurGoalCenter + new Vector2D(-MotionPlannerParameters.DangerZoneW / 2, 0), new Vector2D(), null);
            ////obstaclesList[id] = obs;
            //obs = new Obstacle();
            //id = -3;
            //obs.Type = ObstacleType.ZoneCircle;
            //obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW, MotionPlannerParameters.DangerZoneW);
            //obs.State = new SingleObjectState(GameParameters.OurGoalCenter + new Vector2D(0, MotionPlannerParameters.DangerZoneH / 2), new Vector2D(), null);
            //obstaclesList[id] = (obs);
            //obs = new Obstacle();
            //id = -4;
            //obs.Type = ObstacleType.ZoneCircle;
            //obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW, MotionPlannerParameters.DangerZoneW);
            //obs.State = new SingleObjectState(GameParameters.OurGoalCenter + new Vector2D(0, -MotionPlannerParameters.DangerZoneH / 2), new Vector2D(), null);
            //obstaclesList[id] = obs;
        }
        public void AddOppZone()
        {
            avoidOppZone = 1;
            Obstacle obs = new Obstacle();
            int id = -5;
            obs.Type = ObstacleType.ZoneRectangle;
            obs.Margin = marginOppZone;
            obs.R = new Vector2D(MotionPlannerParameters.DangerZoneH / 2  , MotionPlannerParameters.DangerZoneW / 2 );
            obs.State = new SingleObjectState(GameParameters.OppGoalCenter + new Vector2D(MotionPlannerParameters.DangerZoneH / 2, 0), new Vector2D(), null);
            obstaclesList[id] = (obs);

            //int id = -5;
            //obs.Type = ObstacleType.ZoneCircle;
            //obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW + RobotParameters.OurRobotParams.Diameter / 2, MotionPlannerParameters.DangerZoneW + RobotParameters.OurRobotParams.Diameter / 2);
            //obs.State = new SingleObjectState(GameParameters.OppGoalCenter, new Vector2D(), null);
            //obstaclesList[id] = (obs);
            ////obs.Type = ObstacleType.ZoneRectangle;
            ////obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW / 2, MotionPlannerParameters.DangerZoneH / 2);
            ////obs.State = new SingleObjectState(GameParameters.OurGoalCenter + new Vector2D(-MotionPlannerParameters.DangerZoneW / 2, 0), new Vector2D(), null);
            ////obstaclesList[id] = obs;
            //obs = new Obstacle();
            //id = -6;
            //obs.Type = ObstacleType.ZoneCircle;
            //obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW + RobotParameters.OurRobotParams.Diameter / 2, MotionPlannerParameters.DangerZoneW + RobotParameters.OurRobotParams.Diameter / 2);
            //obs.State = new SingleObjectState(GameParameters.OppGoalCenter + new Vector2D(0, MotionPlannerParameters.DangerZoneH / 2), new Vector2D(), null);
            //obstaclesList[id] = (obs);
            //obs = new Obstacle();
            //id = -7;
            //obs.Type = ObstacleType.ZoneCircle;
            //obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW + RobotParameters.OurRobotParams.Diameter / 2, MotionPlannerParameters.DangerZoneW + RobotParameters.OurRobotParams.Diameter / 2);
            //obs.State = new SingleObjectState(GameParameters.OppGoalCenter + new Vector2D(0, -MotionPlannerParameters.DangerZoneH / 2), new Vector2D(), null);
            //obstaclesList[id] = obs;
        }

        public void AddObstacle(int Robots,int Ball, int DangerZone, int oppDangerZone, List<int> ourIdsToExclude, List<int> oppIdsToExclude, bool stopBall = false)
        {
            if (Robots != 0)
            {
                foreach (var item in Model.OurRobots)
                    if (ourIdsToExclude == null || !ourIdsToExclude.Contains(item.Key))
                        AddRobot(item.Value, true, item.Key);
                foreach (var item in Model.Opponents)
                    if (oppIdsToExclude == null || !oppIdsToExclude.Contains(item.Key))
                        AddRobot(item.Value, false, item.Key);
            }
            if (Ball != 0)
                AddBall(Model.BallState, stopBall);
            if (DangerZone != 0)
                AddDangerZone();
            if (oppDangerZone != 0)
                AddOppZone();
        }
        public void AddObstacle(int Robots, int Ball, int DangerZone, int oppDangerZone, List<int> ourIdsToExclude, List<int> oppIdsToExclude, double kSpeedBall, double kSpeedRobot, bool stopBall)
        {
            if (Robots != 0)
            {
                foreach (var item in Model.OurRobots)
                    if (ourIdsToExclude == null || !ourIdsToExclude.Contains(item.Key))
                        AddRobot(item.Value, true, item.Key,kSpeedRobot);
                foreach (var item in Model.Opponents)
                    if (oppIdsToExclude == null || !oppIdsToExclude.Contains(item.Key))
                        AddRobot(item.Value, false, item.Key,kSpeedRobot);
            }
            if (Ball != 0)
                AddBall(Model.BallState, kSpeedBall, stopBall);
            if (DangerZone != 0)
                AddDangerZone();
            if (oppDangerZone != 0)
                AddOppZone();
        }

        public void RemoveRobots()
        {
            if (avoidRobots != 0)
            {
                avoidRobots = 0;
                var robots = obstaclesList.Where(w => (w.Value.Type == ObstacleType.OurRobot) || (w.Value.Type == ObstacleType.OppRobot)).ToDictionary(k => k.Key, v => v.Value);
                foreach (var item in robots)
                    obstaclesList.Remove(item.Key);
            }
        }
        public void RemoveRobot(bool Our, int id)
        {
            id = (Our) ? id : id + 16;
            if (obstaclesList.ContainsKey(id))
                obstaclesList.Remove(id);
        }
   
        public void RemoveZone()
        {
            if(avoidDangerZone != 0)
            {
                if (obstaclesList.ContainsKey(-2))
                    obstaclesList.Remove(-2);
                if (obstaclesList.ContainsKey(-3))
                    obstaclesList.Remove(-3);
                if (obstaclesList.ContainsKey(-4))
                    obstaclesList.Remove(-4);
            }
        }
        public void RemoveOppZone()
        {
            if (avoidOppZone != 0)
            {
                if (obstaclesList.ContainsKey(-5))
                    obstaclesList.Remove(-5);
                if (obstaclesList.ContainsKey(-6))
                    obstaclesList.Remove(-6);
                if (obstaclesList.ContainsKey(-7))
                    obstaclesList.Remove(-7);
            }
        }
        
        public void Remove(int id)
        {
            if (id == -1)
                RemoveBall();
            else if (id == -2 || id == -3 || id == -4)
                RemoveZone();
            else if (id == -5 || id == -6 || id == -7)
                RemoveOppZone();
            else if (obstaclesList.ContainsKey(id))
                obstaclesList.Remove(id);
        }
        public void RemoveBall()
        {
            if (avoidBall != 0)
            {
                avoidBall = 0;
                if (obstaclesList.ContainsKey(-1))
                    obstaclesList.Remove(-1);
            }
        }
        
        public bool Meet(SingleObjectState From, SingleObjectState To, double obstacleRadi, Dictionary<int, double> margins = null, bool useMargin = false)
        {
            int i = 0;
            double margin = (margins != null && margins.ContainsKey(obstaclesList.ElementAt(i).Key)) ? margins[obstaclesList.ElementAt(i).Key] : 0;
            while (i < obstaclesList.Count && !obstaclesList.ElementAt(i).Value.Meet(From, To, obstacleRadi + margin, useMargin))
            {
                i++;
                if (i < ObstaclesList.Count)
                    margin = (margins != null && margins.ContainsKey(obstaclesList.ElementAt(i).Key)) ? margins[obstaclesList.ElementAt(i).Key] : 0;
            }
            return (i != obstaclesList.Count);
        }
        public bool Meet(SingleObjectState From, SingleObjectState To, double obstacleRadi, out int idx, Dictionary<int, double> margins = null, bool useMargin = false)
        {
            int i = 0;
            idx = -1000;
            double margin = (margins != null && margins.ContainsKey(obstaclesList.ElementAt(i).Key)) ? margins[obstaclesList.ElementAt(i).Key] : 0;

            while (i < obstaclesList.Count && !obstaclesList.ElementAt(i).Value.Meet(From, To, obstacleRadi + margin, useMargin))
            {
                i++;
                if (i < ObstaclesList.Count)
                    margin = (margins != null && margins.ContainsKey(obstaclesList.ElementAt(i).Key)) ? margins[obstaclesList.ElementAt(i).Key] : 0;

            }
            idx = (i < obstaclesList.Count) ? obstaclesList.ElementAt(i).Key : -1000;
            return (i != obstaclesList.Count);
        }
        public bool MeetDangerZone(SingleObjectState From, SingleObjectState To, double obstacleRadi, Dictionary<int, double> margins = null, bool useMargin = false)
        {
            double margin = 0;

            foreach (var item in obstaclesList.Keys)
            {
                if (item > -2 || item < -4)
                    continue;
                margin = (margins != null && margins.ContainsKey(item)) ? margins[item] : 0;
                if (obstaclesList[item].Meet(From, To, obstacleRadi + margin, useMargin))
                    return true;
            }
            return false;
        }
        public bool Meet(SingleObjectState S1, double obstacleRadi, Dictionary<int, double> margins = null, bool useMargin = false)
        {
            int i = 0;
            double margin = (margins != null && margins.ContainsKey(obstaclesList.ElementAt(i).Key)) ? margins[obstaclesList.ElementAt(i).Key] : 0;

            while (i < obstaclesList.Count && !obstaclesList.ElementAt(i).Value.Meet(S1, obstacleRadi + margin, useMargin)) 
            {
                i++;
                if (i < ObstaclesList.Count)
                    margin = (margins != null && margins.ContainsKey(obstaclesList.ElementAt(i).Key)) ? margins[obstaclesList.ElementAt(i).Key] : 0;

            }
            return (i != obstaclesList.Count);
        }
    }
}
