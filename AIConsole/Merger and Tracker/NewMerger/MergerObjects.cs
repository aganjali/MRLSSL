using messages_robocup_ssl_detection;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{

    public class CmuMerger
    {
        private static int MaxCameras = StaticVariables.CameraCount;

        public class Observation
        {
            bool valid;
            int last_valid;
            double time;
            float conf;
            Position2D loc;
            float angle;

            public Observation()
            {
                Valid = false;
                Last_valid = 0;
                Time = 0;
                Conf = 0;
                Loc = Position2D.Zero;
                Angle = 0;
            }

            public bool Valid { get => valid; set => valid = value; }
            public int Last_valid { get => last_valid; set => last_valid = value; }
            public double Time { get => time; set => time = value; }
            public float Conf { get => conf; set => conf = value; }
            public Position2D Loc { get => loc; set => loc = value; }
            public float Angle { get => angle; set => angle = value; }
        };

        public class ObjectMerger
        {

            int affinity;
            Observation[] obs = new Observation[StaticVariables.CameraCount];
            private static int AFFINITY_PERSIST = 30;

            public int Affinity { get => affinity; set => affinity = value; }
            public Observation[] Obs { get => obs; set => obs = value; }

            public ObjectMerger()
            {
                affinity = -1;
                for (int i = 0; i < obs.Length; i++)
                {
                    obs[i] = new Observation();
                }
            }
            public int MergeObservations()
            {
                int last_affinity = affinity;
                if (affinity < 0 || !obs[affinity].Valid)
                {
                    if(affinity >= 0)
                    {

                    }

                    affinity = -1;
                    double minDist = double.MaxValue;
                    for (int o = 0; o < MaxCameras; o++)
                    {
                        if (obs[o].Valid)
                        {
                            if (last_affinity >= 0)
                            {
                                double d = obs[last_affinity].Loc.DistanceFrom(obs[o].Loc);
                                if (d < minDist)
                                {
                                    minDist = d;
                                    affinity = o;
                                }
                            }
                            else
                            {
                                affinity = o;
                                break;
                            }
                            //affinity = o;
                            //break;

                        }
                    }
                }

                if (affinity < 0 && last_affinity >= 0 && obs[last_affinity].Last_valid < AFFINITY_PERSIST)
                {
                    affinity = last_affinity;
                }

               
                return affinity;
            }
            public int MergeObservations(int ballAffinity)
            {
                int last_affinity = affinity;
                if (ballAffinity >= 0 && affinity != ballAffinity && obs[ballAffinity].Valid)
                    affinity = ballAffinity;
                else if (affinity < 0 || !obs[affinity].Valid)
                {
                    if (affinity >= 0)
                    {

                    }

                    affinity = -1;
                    double minDist = double.MaxValue;
                    for (int o = 0; o < MaxCameras; o++)
                    {
                        if (obs[o].Valid)
                        {
                            if (last_affinity >= 0)
                            {
                                double d = obs[last_affinity].Loc.DistanceFrom(obs[o].Loc);
                                if (d < minDist)
                                {
                                    minDist = d;
                                    affinity = o;
                                }
                            }
                            else
                            {
                                affinity = o;
                                break;
                            }
                            //affinity = o;
                            //break;

                        }
                    }
                }

                if (affinity < 0 && last_affinity >= 0 && obs[last_affinity].Last_valid < AFFINITY_PERSIST)
                {
                    affinity = last_affinity;
                }


                return affinity;
            }
        };


       

        frame world;


        uint frames;

        // Observation last_ball;

        ObjectMerger[,] robots = new ObjectMerger[StaticVariables.NUM_TEAMS, StaticVariables.MAX_ROBOT_ID];

        ObjectMerger ball;
        private Dictionary<uint, List<SSL_DetectionBall>> balls= new Dictionary<uint, List<SSL_DetectionBall>>();
        private double last_capture_time;

        public CmuMerger()
        {
            world = new frame();
            ball = new ObjectMerger();
            for (int i = 0; i < StaticVariables.NUM_TEAMS; i++)
            {
                for (int j = 0; j < StaticVariables.MAX_ROBOT_ID; j++)
                {
                    robots[i, j] = new ObjectMerger();
                }
            }
        }
        public void MakeWorld(bool isYellow)
        {
            world.timeofcapture = last_capture_time;  // GetTimeMicros() / 1e6;
            if (ball.MergeObservations() >= 0)
            {
                Observation obs = ball.Obs[ball.Affinity];
                vraw wb = new vraw
                {
                    conf = (true || obs.Last_valid < 2) ? obs.Conf : 0,
                    pos = obs.Loc,
                    angle = obs.Angle,
                    camera = (uint)ball.Affinity,
                    timestamp = obs.Time
                };
                world.Balls[0] = new vball(wb, new SingleObjectState());

            }

            for (int team = 0; team < StaticVariables.NUM_TEAMS; team++)
            {
                for (int id = 0; id < StaticVariables.MAX_ROBOT_ID; id++)
                {
                    ObjectMerger robot = robots[team, id];
                    if (robot.MergeObservations(ball.Affinity) >= 0)
                    {
                        Observation obs = robot.Obs[robot.Affinity];
                        vraw wr = new vraw
                        {
                            conf = (true || obs.Last_valid < 2) ? obs.Conf : 0,
                            pos = obs.Loc,
                            angle = obs.Angle,
                            camera = (uint)robot.Affinity,
                            timestamp = obs.Time
                        };

                        if ((team == 0 && !isYellow) || (team == 1 && isYellow))
                        {
                            world.OurRobots[(uint)id] = new vrobot(wr, new SingleObjectState());
                        }
                        else
                        {
                            world.OppRobots[(uint)id] = new vrobot(wr, new SingleObjectState());
                        }
                    }
                }
            }

         
          
        }

        // returns whether a new referee message is available
        public void UpdateVision(SSL_DetectionFrame d, bool isYellow, Position2D selectedBall,ref bool selectedBallChanged, bool isReverse, bool ready)
        {
            uint camera = d.camera_id;

            //// count how many cameras are sending, at first
            //if (num_cameras <= 0)
            //{
            //    if (!cameras_seen[camera])
            //    {
            //        num_cameras_seen++;
            //        cameras_seen[camera] = true;
            //    }

            //    if (frames++ >= 100)
            //    {
            //        num_cameras = num_cameras_seen;

            //        for (int i = 0; i < cameras_seen.Length; i++)
            //        {
            //            cameras_seen[i] = false;
            //        }
            //        num_cameras_seen = 0;
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}

            double time = d.t_capture;
            last_capture_time = time;
            for(int i =0; i < d.robots_blue.Count; i++) {
                var r = d.robots_blue[i];
                Observation obs = robots[0, r.robot_id].Obs[camera];
                obs.Valid = true;
                // obs.last_valid = 0;
                obs.Time = time;
                obs.Conf = r.confidence;
                obs.Loc = new Position2D(r.x, r.y);
                obs.Angle = r.orientation;
            }
            for (int i = 0; i < d.robots_yellow.Count; i++)
            {
                var r = d.robots_yellow[i];
                Observation obs = robots[1, r.robot_id].Obs[camera];
                obs.Valid = true;
                // obs.last_valid = 0;
                obs.Time = time;
                obs.Conf = r.confidence;
                obs.Loc = new Position2D(r.x, r.y);
                obs.Angle = r.orientation;
            }

            // take the ball from this camera that is closest to the last position of the
            // ball and not too far away
            float max_dist = float.MaxValue;
            Position2D last_ball_loc = Position2D.Zero;
            if (ball.Affinity >= 0)
            {
                Observation last_ball = ball.Obs[ball.Affinity];
                if (last_ball.Last_valid < 30)
                {
                    max_dist = 10000 * StaticVariables.FRAME_PERIOD * last_ball.Last_valid * 100;
                    last_ball_loc = last_ball.Loc;
                }
                else
                {

                }
                
            }
            else
            {
                
            }
            if (d.balls.Count > 0)
            {
                bool found = false;
                float min_dist = float.MaxValue;
                SSL_DetectionBall closest = new SSL_DetectionBall();
                foreach (var b in d.balls)
                {
                    if (!balls.ContainsKey(camera))
                        balls[camera] = new List<SSL_DetectionBall>();
                    balls[camera].Add(b);
                    if (selectedBallChanged)
                        last_ball_loc = AI2Vision( selectedBall, isReverse);
                    
                    float dist = (float)last_ball_loc.DistanceFrom(new Position2D(b.x, b.y));
                    if (dist < min_dist && dist < max_dist)
                    {
                        if (GameParameters.IsInField(Vision2AI(new Position2D(b.x, b.y), isReverse), 0.05))
                        {
                            closest = b;
                            min_dist = dist;
                            found = true;
                        }
                    }


                }
                if (found)
                {
                    Observation obs = ball.Obs[camera];
                    obs.Valid = true;
                     //obs.Last_valid = 0;
                    obs.Time = time;
                    obs.Conf = closest.confidence;
                    obs.Loc = new Position2D(closest.x, closest.y);

                }
               

            }
            else
            {
                //if(camera == ball.Affinity)
                //{
                //    Observation obs = ball.Obs[camera];
                //    obs.Valid = true;
                //    obs.Last_valid ++;
                //}
            }

            //if (!cameras_seen[camera])
            //{
            //    num_cameras_seen++;
            //    cameras_seen[camera] = true;
            //}

            //ready = (num_cameras_seen == num_cameras);
            if (ready)
            {
               if(selectedBallChanged)
                {
                    float min_dist = float.MaxValue;
                    SSL_DetectionBall closest = new SSL_DetectionBall();
                    int cam = -1;
                    foreach (var item in balls.Keys)
                    {
                        foreach (var b in balls[item])
                        {
                            float dist = (float)AI2Vision(selectedBall, isReverse).DistanceFrom(new Position2D(b.x, b.y));
                            if (dist < min_dist)
                            {
                                closest = b;
                                min_dist = dist;
                                cam = (int)item;
                            }
                        }
                    }
                    ball.Affinity = cam;
                    ball.Obs[ball.Affinity].Valid = true;
                    ball.Obs[ball.Affinity].Last_valid = 1;
                    for (int i = 0; i < ball.Obs.Length; i++)
                    {
                        if (i != cam)
                        {
                            var obs = ball.Obs[i];
                            obs.Valid = false;
                            obs.Last_valid = 0;
                        }
                    }
                    selectedBallChanged = false;
                    
                }

                world = new frame();
                // condense all observations and convert to World object
                MakeWorld(isYellow);
                uint c = 0;
                foreach (var item in balls.Keys)
                {
                    foreach (var b in balls[item])
                    {
                        world.OtherBalls.Add(c++, new vball(new vraw(0, new Position2D(b.x, b.y), 0, 1, item), new SingleObjectState()));
                    }
                }
                balls = new Dictionary<uint, List<SSL_DetectionBall>>();
                // forget all previous observations
                for (int i = 0; i < robots.GetLength(0); i++)
                {
                    for (int j = 0; j < robots.GetLength(1); j++)
                    {
                        var robot = robots[i, j];
                        for (int k = 0; k < robot.Obs.Length; k++)
                        {
                            var obs = robot.Obs[k];
                            if (obs.Valid)
                            {
                                obs.Valid = false;
                                obs.Last_valid = 1;
                            }
                            else
                            {
                                obs.Last_valid++;
                            }
                        }
                    }
                }
                for(int i = 0; i < ball.Obs.Length; i++)
                {
                    var obs = ball.Obs[i];
                    if (obs.Valid)
                    {
                        obs.Valid = false;
                        obs.Last_valid = 1;
                    }
                    else
                    {
                        obs.Last_valid++;
                    }
                }


            }
        }
        private Position2D Vision2AI(Position2D pos, bool isReverse)
        {
            int sgn = (isReverse) ? -1 : 1;
            return new Position2D(pos.X * sgn/ 1000, -pos.Y * sgn / 1000);
        }
        private Position2D AI2Vision(Position2D pos, bool isReverse)
        {
            int sgn = (isReverse) ? -1 : 1;
            return new Position2D(pos.X * sgn * 1000, -pos.Y * sgn * 1000);
        }
        public frame World { get => world; }
    }
}
