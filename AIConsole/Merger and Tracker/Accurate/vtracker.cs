//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.GameDefinitions;
//using MRL.SSL.CommonClasses.MathLibrary;

//namespace MRL.SSL.AIConsole.New_MergerAndTracker
//{
//    public class VTracker
//    {
//        public int[,] id2index = new int[StaticVariables.NUM_TEAMS, StaticVariables.MAX_ROBOT_ID];
//        public int[,] index2id = new int[StaticVariables.NUM_TEAMS, StaticVariables.MAX_TEAM_ROBOTS];
//        public bool Exists(int team, int robot) { return (index2id[team, robot] >= 0); }
//        public RobotTracker[,] robots = new RobotTracker[StaticVariables.NUM_TEAMS, StaticVariables.MAX_TEAM_ROBOTS];
//        public BallTracker[] ball = new BallTracker[StaticVariables.MaxBalls];
//        public int[] bId2index = new int[StaticVariables.MaxBalls];
//        public int[] bIndex2id = new int[StaticVariables.MaxBalls];

//        public int MaxNotSeen = 120;
//        public int Max_to_imagine = 1;
//        public double Max_Ball_distance = 0.7;
//        public double Max_Opponent_distance = 0.2;
//        private const bool isOpponentsID_Detected = true;
//        // temp matrix to store ball covariances
//        public FMatrix bcovar;
//        public double Height(int team, int robot)
//        {
//            return RobotParameters.OurRobotParams.Height;
//        }

//        public double Radius(int team, int robot)
//        {
//            return RobotParameters.OurRobotParams.Diameter / 2;
//        }

//        public RobotType Type(int team, int robot)
//        {
//            return RobotType.Default;
//        }

//        public VTracker()
//        {
//            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
//            {
//                for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//                {
//                    index2id[t, i] = -1;
//                    robots[t, i] = new RobotTracker(RobotType.Default, StaticVariables.LATENCY_DELAY);
//                }
//                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
//                    id2index[t, i] = -1;
//            }
//            for (int i = 0; i < StaticVariables.MaxBalls; i++)
//            {
//                bIndex2id[i] = -1;
//                bId2index[i] = -1;
//                ball[i] = new BallTracker();
//                setTracker(i);   
//            }
//        }
//        public frame FillHistory(frame Frame, Dictionary<uint, vraw> Balls)
//        {
//            #region UpdateHistory
//            foreach (var searchKey in Frame.OurRobots.Keys)
//            {
//                Frame.OurRobots[searchKey].vision.notSeen++;
//            }
//            foreach (var searchKey in Frame.OppRobots.Keys)
//            {
//                Frame.OppRobots[searchKey].vision.notSeen++;
//            }
//            foreach (var searchKey in Frame.Balls.Keys)
//            {
//                Frame.Balls[searchKey].vision.notSeen++;
//            }

//            var mustRemoved = Frame.OurRobots.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
//            List<uint> key_or_remove = new List<uint>();
//            foreach (var searchKey in mustRemoved)
//            {
//                key_or_remove.Add(searchKey.Key);
//            }
//            foreach (var i in key_or_remove)
//            {
//                Frame.OurRobots.Remove(i);
//            }
//            mustRemoved = Frame.OppRobots.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
//            key_or_remove = new List<uint>();
//            foreach (var searchKey in mustRemoved)
//            {
//                key_or_remove.Add(searchKey.Key);
//            }
//            foreach (var i in key_or_remove)
//            {
//                Frame.OppRobots.Remove(i);
//            }
//            var mustRemovedBalls = Frame.Balls.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
//            key_or_remove = new List<uint>();
//            foreach (var searchKey in mustRemovedBalls)
//            {
//                key_or_remove.Add(searchKey.Key);
//            }
//            foreach (var i in key_or_remove)
//            {
//                Frame.Balls.Remove(i);
//            }
//            #endregion
//            #region "Ball Section"
//            foreach (var key in Balls.Keys)
//            {
//                double dist, minDist = double.MaxValue;
//                int minHistoryID = -1;
//                int lastCamViewed = -1;

//                foreach (var searchKey in Frame.Balls.Keys)
//                {
//                    if (Frame.Balls[searchKey].vision.notSeen > -1)
//                    {
//                        dist = Balls[key].pos.DistanceFrom(Frame.Balls[searchKey].viewstate.Location);
//                        if (dist < minDist)
//                        {
//                            minDist = dist;
//                            minHistoryID = (int)searchKey;
//                            lastCamViewed = (int)Balls[key].camera;
//                        }
//                    }
//                }
//                int count = -1;
//                if (minHistoryID != -1)
//                {
//                    if (minDist < Max_Ball_distance * 1000)
//                    {
//                        Frame.Balls[(uint)minHistoryID].vision.notSeen = -1;
//                        Frame.Balls[(uint)minHistoryID].vision.lastCamViewd = lastCamViewed;
//                        Frame.Balls[(uint)minHistoryID].vision.pos = Balls[key].pos;
//                        Frame.Balls[(uint)minHistoryID].vision.timestamp = Balls[key].timestamp;
//                        Frame.Balls[(uint)minHistoryID].vision.conf = Balls[key].conf; 
//                    }
//                    else
//                    {

//                        for (uint i = 0; i < Frame.Balls.Count; i++)
//                        {
//                            if (Frame.Balls.ContainsKey(i) == false)
//                            {
//                                count = (int)i;
//                            }
//                        }
//                        if (count == -1)
//                        {
//                            count = Frame.Balls.Count + 1;
//                        }
//                        Frame.Balls.Add((uint)count, new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
//                        Frame.Balls[(uint)count].state.Location = Balls[key].pos;
//                        Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
//                    }
//                }
//                else
//                {

//                    for (int i = 0; i < Frame.Balls.Count; i++)
//                    {
//                        if (Frame.Balls.ContainsKey((uint)i) == false)
//                        {
//                            count = i;
//                        }
//                    }
//                    if (count == -1)
//                    {
//                        count = Frame.Balls.Count + 1;
//                    }
//                    Frame.Balls.Add((uint)count, new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
//                    Frame.Balls[(uint)count].state.Location = Balls[key].pos;
//                    Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
//                }
//            }

//            Dictionary<uint, uint> Related = new Dictionary<uint, uint>();
//            foreach (var key1 in Frame.Balls.Keys)
//            {
//                foreach (var key2 in Frame.Balls.Keys)
//                {
//                    if (key1 != key2)
//                    {
//                        if (Frame.Balls[key1].vision.pos.DistanceFrom(Frame.Balls[key2].vision.pos) < 50)
//                        {
//                            if (Frame.Balls[key1].vision.notSeen < Frame.Balls[key2].vision.notSeen)
//                            {
//                                Related.Add(key1, key2);
//                                break;
//                            }
//                        }
//                    }
//                }
//            }

//            foreach (var key in Related.Keys)
//            {
//                if (Frame.Balls.ContainsKey(Related[key]))
//                    Frame.Balls.Remove(Related[key]);
//            }
//            #endregion
//            return Frame;
//        }
//        public void CallObservs(frame Frame,RobotCommands Commands)
//        {

//            SetConfig(Frame);

//            for (int i = 0; i < StaticVariables.MaxBalls; i++)
//            {
//                if (bIndex2id[i] >= 0)
//                {
//                    ball[i].observe(Frame.Balls[(uint)bIndex2id[i]].vision, Frame.Balls[(uint)bIndex2id[i]].vision.timestamp);
//                }
//            }
//            //HiPerfTimer ht = new HiPerfTimer();
//            //ht.Start();
//            if (Commands != null && Commands.Commands != null)
//            {
//                foreach (var item in Commands.Commands)
//                {
//                    int idx = id2index[0, item.Key];
//                    if (idx >= 0)
//                        robots[0, idx].command(Frame.OurRobots[(uint)item.Key].vision.timestamp, new Vector3D(item.Value.Vy * 1000, -item.Value.Vx * 1000, item.Value.W));  //IMPORTANT: Vx <-> Vy ,  Vx -> -Vx,  for ex: Command = (Vx,Vy, W) => (Vy, -Vx, W)
//                }
//            }

//            //ht.Stop();
//            //Console.WriteLine(ht.Duration * 1000);
//            //foreach (var item in Frame.OurRobots)
//            //{
//            //    float Vx = 1 * 1000;
//            //    float Vy = 0 * 1000;
//            //    float W = 0;
//            //    int idx = id2index[0, item.Key];
//            //    if (idx >= 0)
//            //        robots[0, idx].command(item.Value.vision.timestamp, new Vector3D(Vy, -Vx, W));   //IMPORTANT: Vx <-> Vy ,  Vx -> -Vx,  for ex: Command = (Vx,Vy, W) => (Vy, -Vx, W)
//            //}

//            for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//            {
//                if (index2id[0, i] >= 0)
//                    robots[0, i].observe(Frame.OurRobots[(uint)index2id[0, i]].vision, Frame.OurRobots[(uint)index2id[0, i]].vision.timestamp);
//            }
//            for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//            {
//                if (index2id[1, i] >= 0)
//                    robots[1, i].observe(Frame.OppRobots[(uint)index2id[1, i]].vision, Frame.OppRobots[(uint)index2id[1, i]].vision.timestamp);
//            }

//        }
//        public frame GetEstimated(frame Frame)
//        {

//            for (int i = 0; i < StaticVariables.MaxBalls; i++)
//            {
//                if (bIndex2id[i] >= 0)
//                {
//                    HiPerfTimer ht = new HiPerfTimer();
//                    ht.Start();
//                    vball tmp1 = GetBallData(StaticVariables.BallPredictTime, i);
//                    ht.Stop();
//                   // Console.WriteLine(ht.Duration * 1000);
//                    Frame.Balls[(uint)bIndex2id[i]].state = tmp1.state;
//                    Frame.Balls[(uint)bIndex2id[i]].occluded = tmp1.occluded;
//                    Frame.Balls[(uint)bIndex2id[i]].occluding_team = tmp1.occluding_team;
//                    Frame.Balls[(uint)bIndex2id[i]].occluding_robot = tmp1.occluding_robot;
//                    Frame.Balls[(uint)bIndex2id[i]].occluding_offset = tmp1.occluding_offset;
//                    vball tmp = GetBallData(StaticVariables.viewDelay, i);
//                    Frame.Balls[(uint)bIndex2id[i]].viewstate = tmp.state;
//                }
//            }

//            for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//            {
//                if (index2id[0, i] >= 0)
//                {
//                    vrobot tmp1 = GetRobotData(0, i, StaticVariables.RobotPredictTime);
//                    Frame.OurRobots[(uint)index2id[0, i]].state = tmp1.state;
//                    vrobot tmp = GetRobotData(0, i, StaticVariables.viewDelay);
//                    Frame.OurRobots[(uint)index2id[0, i]].viewstate = tmp.state;
//                }
//            }
//            for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//            {
//                if (index2id[1, i] >= 0)
//                {
//                    vrobot tmp1 = GetRobotData(1, i, StaticVariables.RobotPredictTime);
//                    Frame.OppRobots[(uint)index2id[1, i]].state = tmp1.state;
//                    vrobot tmp = GetRobotData(1, i, StaticVariables.viewDelay);
//                    Frame.OppRobots[(uint)index2id[1, i]].viewstate = tmp.state;
//                }
//            }
//            return Frame;
//        }

//        public void setTracker(int idx)
//        {
//            ball[idx].set_tracker(this);
//        }

//        // set the configuration for the EKBF's 
//        public void SetConfig(ref net_vconfig vcfg)
//        {


//            // clear out idnexes and types
//            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
//            {
//                for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//                {
//                    index2id[t, i] = -1;
//                    robots[t, i].set_type(RobotType.Default);
//                }
//                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
//                    id2index[t, i] = -1;
//            }

//            // set it all
//            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
//            {
//                for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//                {
//                    if (vcfg.teams[t].robots[i].id >= 0)
//                    {
//                        index2id[t, i] = vcfg.teams[t].robots[i].id;
//                        id2index[t, vcfg.teams[t].robots[i].id] = i;
//                        robots[t, i].set_type(vcfg.teams[t].robots[i].type);
//                    }
//                }
//            }
//            for (int i = 0; i < StaticVariables.MaxBalls; i++)
//            {
//                bId2index[i] = -1;
//                bIndex2id[i] = -1;
//                if (vcfg.balls[i].id >= 0)
//                {
//                    bIndex2id[i] = vcfg.balls[i].id;
//                    bId2index[vcfg.balls[i].id] = i;
//                }
//            }
//        }
//        public void SetConfig(frame Frame)
//        {
//            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
//            {
//                for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//                {
//                    index2id[t, i] = -1;
//                 //   robots[t, i].set_type(RobotType.Default);
//                }
//                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
//                    id2index[t, i] = -1;
//            }
//            for (int i = 0; i < Frame.OurRobots.Count; i++)
//            {
//                index2id[0, i] = (int)Frame.OurRobots.ElementAt(i).Key;
//                id2index[0, (int)Frame.OurRobots.ElementAt(i).Key] = i;
//                //robots[0, i].set_type(Frame.type);
//            }
//            for (int i = 0; i < Frame.OppRobots.Count; i++)
//            {
//                index2id[1, i] = (int)Frame.OppRobots.ElementAt(i).Key;
//                id2index[1, (int)Frame.OppRobots.ElementAt(i).Key] = i;
//               // robots[1, i].set_type(Frame.type);
//            }
//            for (int i = 0; i < StaticVariables.MaxBalls; i++)
//            {
//                bId2index[i] = -1;
//                bIndex2id[i] = -1;
//            }
//            for (int i = 0; i < Frame.Balls.Count; i++)
//            {
//                bIndex2id[i] = (int)Frame.Balls.ElementAt(i).Key;
//                bId2index[(int)Frame.Balls.ElementAt(i).Key] = i;
//            }
//        }
//        public void ResetAll()
//        {
//            for (int i = 0; i < StaticVariables.MaxBalls; i++)
//            {
//                if (bIndex2id[i] > 0)
//                    ball[i].reset();
//            }
//            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
//            {
//                for (int i = 0; i < StaticVariables.MAX_TEAM_ROBOTS; i++)
//                {
//                    if (index2id[t, i] >= 0)
//                        robots[t, i].reset();
//                }
//            }
//        }

//        public void GetBallData(ref vball vb, double dt, int idx)
//        {
//            // fill out tracking info
//            bcovar = ball[idx].covariances(dt);
//            for (int i = 0; i < 4; i++)
//            {
//                for (int j = 0; j < 4; j++)
//                {
//                    vb.variances[j * 4 + i] = bcovar[i, j];
//                }
//            }

//            Position2D tmp = ball[idx].position(dt);
//            vb.state.Location = tmp;
//            //vb.state.x = (float)tmp.X;
//            //vb.state.y = (float)tmp.Y;
//            Vector2D tmpv = ball[idx].velocity(dt);
//            //vb.state.vx = (float)tmpv.X;
//            //vb.state.vy = (float)tmpv.Y;
//            vb.state.Speed = tmpv;
//            vb.occluded = ball[idx].occluded;
//            vb.occluding_team = ball[idx].occluding_team;
//            vb.occluding_robot = ball[idx].occluding_robot;
//            vb.occluding_offset = new Vector2D(ball[idx].occluding_offset.X, ball[idx].occluding_offset.Y);
//        }
//        public vball GetBallData( double dt, int idx)
//        {
//            vball vb = new vball();
//            // fill out tracking info
//            HiPerfTimer ht = new HiPerfTimer();
//            ht.Start();
//            bcovar = ball[idx].covariances(dt);
//            ht.Stop();
//           // Console.WriteLine(ht.Duration * 1000 + "      " + idx);
//            //for (int i = 0; i < 4; i++)
//            //{
//            //    for (int j = 0; j < 4; j++)
//            //    {
//            //        vb.variances[j * 4 + i] = bcovar[i, j];
//            //    }
//            //}

//            Position2D tmp = ball[idx].position(dt);
//            vb.state.Location = tmp;
//            //vb.state.x = (float)tmp.X;
//            //vb.state.y = (float)tmp.Y;
//            Vector2D tmpv = ball[idx].velocity(dt);
//            //vb.state.vx = (float)tmpv.X;
//            //vb.state.vy = (float)tmpv.Y;
//            vb.state.Speed = tmpv;
//            vb.occluded = ball[idx].occluded;
//            vb.occluding_team = ball[idx].occluding_team;
//            vb.occluding_robot = ball[idx].occluding_robot;
//            vb.occluding_offset = new Vector2D(ball[idx].occluding_offset.X, ball[idx].occluding_offset.Y);
//            return vb;
//        }
//        public void GetRobotData(ref vrobot vr, int team, int indx, double dt)
//        {
//            // Get the state information from the Kalman filter
//            Position2D tmp = robots[team, indx].position(dt);
//            vr.state.Location = tmp;
//            //vr.state.x = (float)tmp.X;
//            //vr.state.y = (float)tmp.Y;
//            //vr.state.theta = robots[team, indx].direction(dt);
//            vr.state.Angle = (float)robots[team, indx].direction(dt);

//            Vector2D tmpv = robots[team, indx].velocity_raw(dt);
//            vr.state.Speed = tmpv;
//            //vr.state.vx = (float)tmpv.X;
//            //vr.state.vy = (float)tmpv.Y;
//            //vr.state.vtheta = robots[team, indx].angular_velocity(dt);
//            vr.state.AngularSpeed = (float)robots[team, indx].angular_velocity(dt);
//            //vr.state.stuck = robots[team, indx].stuck(dt);
//        }
//        public vrobot GetRobotData( int team, int indx, double dt)
//        {
//            vrobot vr = new vrobot();
//            // Get the state information from the Kalman filter
//            Position2D tmp = robots[team, indx].position(dt);
//            vr.state.Location = tmp;
//            //vr.state.x = (float)tmp.X;
//            //vr.state.y = (float)tmp.Y;
//            //vr.state.theta = robots[team, indx].direction(dt);
//            vr.state.Angle = (float)robots[team, indx].direction(dt);

//            Vector2D tmpv = robots[team, indx].velocity_raw(dt);
//            vr.state.Speed = tmpv;
//            //vr.state.vx = (float)tmpv.X;
//            //vr.state.vy = (float)tmpv.Y;
//            //vr.state.vtheta = robots[team, indx].angular_velocity(dt);
//            vr.state.AngularSpeed = (float)robots[team, indx].angular_velocity(dt);
//            //vr.state.stuck = robots[team, indx].stuck(dt);
//            return vr;
//        }
//        public WorldModel CreateModelWitoutBall(frame Frame)
//        {
//            WorldModel Model = new WorldModel();
//            Model.OurRobots = new Dictionary<int, SingleObjectState>();
//            Model.Opponents = new Dictionary<int, SingleObjectState>();
//            Model.BallState = new SingleObjectState();
//            #region Our
//            Vector2D VRobotInField = new Vector2D() ;
//            Vector2D tmpVec = new Vector2D();
//            foreach (var item in Frame.OurRobots.ToDictionary(k => k.Key, v => v.Value))
//            {
//                if (Frame.OurRobots[item.Key].vision.notSeen < Max_to_imagine + 15)
//                {

//                    VRobotInField.X = item.Value.state.Speed.X * Math.Cos(item.Value.state.Angle.Value) - item.Value.state.Speed.Y * Math.Sin(item.Value.state.Angle.Value);
//                    VRobotInField.Y = item.Value.state.Speed.Y * Math.Cos(item.Value.state.Angle.Value) + item.Value.state.Speed.X * Math.Sin(item.Value.state.Angle.Value);
//                    float? Angle = (float)(-1 * ((180 / Math.PI) * item.Value.state.Angle.Value));
//                    if (Angle > 180)
//                        Angle -= 360;
//                    else if (Angle < -180)
//                        Angle += 360;
//                     Model.OurRobots[(int)item.Key] = new SingleObjectState(ObjectType.OurRobot, new Position2D(item.Value.state.Location.X / 1000, -item.Value.state.Location.Y / 1000), new Vector2D(VRobotInField.X / 1000, -VRobotInField.Y / 1000), new Vector2D(), Angle, -item.Value.state.AngularSpeed);
//                }
//            }
//            #endregion
//            #region Opp
//            foreach (var item in Frame.OppRobots.ToDictionary(k => k.Key, v => v.Value))
//            {
//                if (Frame.OppRobots[item.Key].vision.notSeen < Max_to_imagine + 15)
//                {
//                    VRobotInField.X = item.Value.state.Speed.X * Math.Cos(item.Value.state.Angle.Value) - item.Value.state.Speed.Y * Math.Sin(item.Value.state.Angle.Value);
//                    VRobotInField.Y = item.Value.state.Speed.Y * Math.Cos(item.Value.state.Angle.Value) + item.Value.state.Speed.X * Math.Sin(item.Value.state.Angle.Value);
//                    float? Angle = (float)(-1 * ((180 / Math.PI) * item.Value.state.Angle.Value));
//                    if (Angle > 180)
//                        Angle -= 360;
//                    else if (Angle < -180)
//                        Angle += 360;
//                    Model.Opponents[(int)item.Key] = new SingleObjectState(ObjectType.Opponent, new Position2D(item.Value.state.Location.X / 1000, -item.Value.state.Location.Y / 1000), new Vector2D(VRobotInField.X / 1000, -VRobotInField.Y / 1000), new Vector2D(), Angle, -item.Value.state.AngularSpeed);
//                }
//            }
//            #endregion
//            return Model;
//        }
//        public bool SelectedBallIndexViewed(frame Frame, uint BallIndex)
//        {
//            bool ret = false;
//            //foreach (int key in ballHistory.Keys)
//            //{
//            //    if (key == BallIndex && ballHistory[key].NumNotViewed < Max_to_imagine)
//            //        ret = true;
//            //}
//            if (Frame.Balls.ContainsKey(BallIndex))
//                if (Frame.Balls[BallIndex].vision.notSeen < Max_to_imagine)
//                    ret = true;

//            if (ret == true)
//            {
//                if (Frame.Balls[BallIndex].vision.pos.X / 1000 > GameParameters.OppGoalLeft.X - RobotParameters.OurRobotParams.Diameter / 2.0 && Frame.Balls[BallIndex].vision.pos.X / 1000 < GameParameters.OurGoalLeft.X + RobotParameters.OurRobotParams.Diameter / 2.0 && -Frame.Balls[BallIndex].vision.pos.Y / 1000 > GameParameters.OurRightCorner.Y - RobotParameters.OurRobotParams.Diameter / 2.0 && -Frame.Balls[BallIndex].vision.pos.Y / 1000 < GameParameters.OurLeftCorner.Y + RobotParameters.OurRobotParams.Diameter / 2.0)
//                {
//                    return ret;
//                }
//                else
//                {
//                    ret = false;
//                }
//            }
//            return ret;
//        }
//        public Position2D ReturnSelectedBallIndexPosition(frame Frame, uint BallIndex)
//        {
//            return new Position2D(Frame.Balls[BallIndex].state.Location.X , Frame.Balls[BallIndex].state.Location.Y);
//        }
//        internal Vector2D ReturnSelectedBallIndexSpeed(frame Frame, uint BallIndex)
//        {
//            return new Vector2D(Frame.Balls[BallIndex].state.Speed.X , Frame.Balls[BallIndex].state.Speed.Y );
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using Meta.Numerics.Matrices;
using System.Drawing;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public class VTracker
    {
        public int[,] id2index = new int[StaticVariables.NUM_TEAMS, StaticVariables.MAX_ROBOT_ID];
        public int[,] index2id = new int[StaticVariables.NUM_TEAMS, StaticVariables.MAX_ROBOT_ID];

        public bool Exists(int team, int robot) { return (index2id[team, robot] >= 0); }
        public RobotTracker[,] robots = new RobotTracker[StaticVariables.NUM_TEAMS, StaticVariables.MAX_ROBOT_ID];
        public BallTracker[] ball = new BallTracker[StaticVariables.MaxBalls];
        public BallTracker BallLongPredict;

        public int[] bId2index = new int[StaticVariables.MaxBalls];
        public int[] bIndex2id = new int[StaticVariables.MaxBalls];

        public int MaxNotSeen = 120;
        public int Max_to_imagine = 1;
        public double Max_Ball_distance = 0.7;
        public double Max_Opponent_distance = 0.2;
        private const bool isOpponentsID_Detected = true;
        // temp matrix to store ball covariances
        public RectangularMatrix bcovar;
        public double Height(int team, int robot)
        {
            return RobotParameters.OurRobotParams.Height;
        }

        public double Radius(int team, int robot)
        {
            return RobotParameters.OurRobotParams.Diameter / 2;
        }

        public RobotType Type(int team, int robot)
        {
            return RobotType.Default;
        }

        public VTracker()
        {
            if (robots[0, 0] == null)
            {
                for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
                {
                    for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    {
                        index2id[t, i] = -1;
                        robots[t, i] = new RobotTracker(RobotType.Default, StaticVariables.LATENCY_DELAY);
                    }
                    for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                        id2index[t, i] = -1;
                }
            }
            for (int i = 0; i < StaticVariables.MaxBalls; i++)
            {
                bIndex2id[i] = -1;
                bId2index[i] = -1;
                ball[i] = new BallTracker();
                setTracker(i);
            }
            BallLongPredict = new BallTracker();
            BallLongPredict.set_tracker(this);
        }

        public frame FillHistory(frame Frame, Dictionary<uint, vraw> Balls)
        {
            #region UpdateHistory
            foreach (var searchKey in Frame.OurRobots.Keys)
            {
                Frame.OurRobots[searchKey].vision.notSeen++;
            }
            foreach (var searchKey in Frame.OppRobots.Keys)
            {
                Frame.OppRobots[searchKey].vision.notSeen++;
            }
            foreach (var searchKey in Frame.Balls.Keys)
            {
                Frame.Balls[searchKey].vision.notSeen++;
            }

            var mustRemoved = Frame.OurRobots.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
            List<uint> key_or_remove = new List<uint>();
            foreach (var searchKey in mustRemoved)
            {
                key_or_remove.Add(searchKey.Key);
            }
            foreach (var i in key_or_remove)
            {
                Frame.OurRobots.Remove(i);
            }
            mustRemoved = Frame.OppRobots.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
            key_or_remove = new List<uint>();
            foreach (var searchKey in mustRemoved)
            {
                key_or_remove.Add(searchKey.Key);
            }
            foreach (var i in key_or_remove)
            {
                Frame.OppRobots.Remove(i);
            }
            var mustRemovedBalls = Frame.Balls.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
            key_or_remove = new List<uint>();
            foreach (var searchKey in mustRemovedBalls)
            {
                key_or_remove.Add(searchKey.Key);
            }
            foreach (var i in key_or_remove)
            {
                Frame.Balls.Remove(i);
            }
            #endregion
            #region "Ball Section"
            foreach (var key in Balls.Keys)
            {
                double dist, minDist = double.MaxValue;
                int minHistoryID = -1;
                int lastCamViewed = -1;

                foreach (var searchKey in Frame.Balls.Keys)
                {
                    if (Frame.Balls[searchKey].vision.notSeen > -1)
                    {
                        dist = Balls[key].pos.DistanceFrom(Frame.Balls[searchKey].viewstate.Location);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minHistoryID = (int)searchKey;
                            lastCamViewed = (int)Balls[key].camera;
                        }
                    }
                }
                int count = -1;
                if (minHistoryID != -1)//////
                {
                    if (minDist < Max_Ball_distance * 1000)
                    {
                        Frame.Balls[(uint)minHistoryID].vision.notSeen = -1;
                        Frame.Balls[(uint)minHistoryID].vision.lastCamViewd = lastCamViewed;
                        Frame.Balls[(uint)minHistoryID].vision.pos = Balls[key].pos;
                        Frame.Balls[(uint)minHistoryID].vision.timestamp = Balls[key].timestamp;
                        Frame.Balls[(uint)minHistoryID].vision.conf = Balls[key].conf;
                    }
                    else
                    {

                        for (uint i = 0; i < Frame.Balls.Count; i++)
                        {
                            if (Frame.Balls.ContainsKey(i) == false)
                            {
                                count = (int)i;
                            }
                        }
                        if (count == -1)
                        {
                            count = Frame.Balls.Count + 1;
                        }
                        Frame.Balls.Add((uint)count, new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
                        Frame.Balls[(uint)count].state.Location = Balls[key].pos;
                        Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
                    }
                }
                else
                {

                    for (int i = 0; i < Frame.Balls.Count; i++)
                    {
                        if (Frame.Balls.ContainsKey((uint)i) == false)
                        {
                            count = i;
                        }
                    }
                    if (count == -1)
                    {
                        count = Frame.Balls.Count + 1;
                    }
                    Frame.Balls.Add((uint)count, new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
                    Frame.Balls[(uint)count].state.Location = Balls[key].pos;
                    Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
                }
            }

            Dictionary<uint, uint> Related = new Dictionary<uint, uint>();
            foreach (var key1 in Frame.Balls.Keys)
            {
                foreach (var key2 in Frame.Balls.Keys)
                {
                    if (key1 != key2)
                    {
                        if (Frame.Balls[key1].vision.pos.DistanceFrom(Frame.Balls[key2].vision.pos) < 50)
                        {
                            if (Frame.Balls[key1].vision.notSeen < Frame.Balls[key2].vision.notSeen)
                            {
                                Related.Add(key1, key2);
                                break;
                            }
                        }
                    }
                }
            }

            foreach (var key in Related.Keys)
            {
                if (Frame.Balls.ContainsKey(Related[key]))
                    Frame.Balls.Remove(Related[key]);
            }
            while (Frame.Balls.Count >= StaticVariables.MaxBalls)
                Frame.Balls.Remove(Frame.Balls.ElementAt(Frame.Balls.Count - 1).Key);
            #endregion
            return Frame;
        }
        public frame FillHistory(frame Frame, List<Dictionary<uint, vrobot>> modelHistory, Dictionary<uint, vraw> Balls, bool FillBall, bool FillRobots)
        {
            #region Robot Section
            if (FillRobots)
            {
                foreach (var searchKey in Frame.OurRobots.Keys)
                {
                    Frame.OurRobots[searchKey].vision.notSeen++;
                    Frame.OurRobots[searchKey].visionProblem = false;
                    DrawingObjects.AddObject(new Circle(new Position2D(Frame.OurRobots[searchKey].vision.pos.X / 1000,-Frame.OurRobots[searchKey].vision.pos.Y / 1000), 0.09,new Pen(Color.Red,0.01f)));
                    if (Frame.OurRobots[searchKey].vision.notSeen > 0&& modelHistory.Count > 0)
                    {
                        if (modelHistory[0].ContainsKey(searchKey))
                        {
                            int notseen = Frame.OurRobots[searchKey].vision.notSeen;
                            Frame.OurRobots[searchKey].vision = new vraw(Frame.timeofcapture, modelHistory[0][searchKey].viewstate.Location, (float)(modelHistory[0][searchKey].viewstate.Angle.Value), 1, Frame.OurRobots[searchKey].vision.camera);
                            Frame.OurRobots[searchKey].visionProblem = true;
                            DrawingObjects.AddObject(new Circle(new Position2D(modelHistory[0][searchKey].viewstate.Location.X / 1000, -modelHistory[0][searchKey].viewstate.Location.Y / 1000), 0.09));
                            DrawingObjects.AddObject(new Line(new Position2D(modelHistory[0][searchKey].viewstate.Location.X / 1000, -modelHistory[0][searchKey].viewstate.Location.Y / 1000), new Position2D(modelHistory[0][searchKey].viewstate.Location.X / 1000, -modelHistory[0][searchKey].viewstate.Location.Y / 1000) + Vector2D.FromAngleSize(-modelHistory[0][searchKey].viewstate.Angle.Value, 1)));
                            Frame.OurRobots[searchKey].vision.notSeen = notseen;
                        }
                    }
                }
                foreach (var searchKey in Frame.OppRobots.Keys)
                {
                    Frame.OppRobots[searchKey].vision.notSeen++;
                }
                var mustRemoved = Frame.OurRobots.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
                List<uint> key_or_remove = new List<uint>();
                foreach (var searchKey in mustRemoved)
                {
                    key_or_remove.Add(searchKey.Key);
                }
                foreach (var i in key_or_remove)
                {
                    Frame.OurRobots.Remove(i);
                }
                mustRemoved = Frame.OppRobots.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
                key_or_remove = new List<uint>();
                foreach (var searchKey in mustRemoved)
                {
                    key_or_remove.Add(searchKey.Key);
                }
                foreach (var i in key_or_remove)
                {
                    Frame.OppRobots.Remove(i);
                }
            }
            #endregion
            #region "Ball Section"
            if (FillBall)
            {
                foreach (var searchKey in Frame.Balls.Keys)
                {
                    Frame.Balls[searchKey].vision.notSeen++;
                }
                var mustRemovedBalls = Frame.Balls.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
                var key_or_remove = new List<uint>();
                foreach (var searchKey in mustRemovedBalls)
                {
                    key_or_remove.Add(searchKey.Key);
                }
                foreach (var i in key_or_remove)
                {
                    Frame.Balls.Remove(i);
                }
                foreach (var key in Balls.Keys)
                {
                    double dist, minDist = double.MaxValue;
                    int minHistoryID = -1;
                    int lastCamViewed = -1;

                    foreach (var searchKey in Frame.Balls.Keys)
                    {
                        if (Frame.Balls[searchKey].vision.notSeen > -1)
                        {
                            dist = Balls[key].pos.DistanceFrom(Frame.Balls[searchKey].viewstate.Location);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                minHistoryID = (int)searchKey;
                                lastCamViewed = (int)Balls[key].camera;
                            }
                        }
                    }
                    int count = -1;
                    if (minHistoryID != -1)
                    {
                        if (minDist < Max_Ball_distance * 1000)
                        {
                            Frame.Balls[(uint)minHistoryID].vision.notSeen = -1;
                            Frame.Balls[(uint)minHistoryID].vision.lastCamViewd = lastCamViewed;
                            Frame.Balls[(uint)minHistoryID].vision.pos = Balls[key].pos;
                            Frame.Balls[(uint)minHistoryID].vision.timestamp = Balls[key].timestamp;
                            Frame.Balls[(uint)minHistoryID].vision.conf = Balls[key].conf;
                        }
                        else
                        {

                            for (uint i = 0; i < Frame.Balls.Count; i++)
                            {
                                if (Frame.Balls.ContainsKey(i) == false)
                                {
                                    count = (int)i;
                                }
                            }
                            if (count == -1)
                            {
                                count = Math.Min(Frame.Balls.Count + 1, StaticVariables.MaxBalls - 1);
                            }
                            Frame.Balls[(uint)count] = (new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
                            Frame.Balls[(uint)count].state.Location = Balls[key].pos;
                            Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
                        }
                    }
                    else
                    {

                        for (int i = 0; i < Frame.Balls.Count; i++)
                        {
                            if (Frame.Balls.ContainsKey((uint)i) == false)
                            {
                                count = i;
                            }
                        }
                        if (count == -1)
                        {
                            count = Math.Min(Frame.Balls.Count + 1, StaticVariables.MaxBalls);
                        }
                        Frame.Balls[(uint)count] = (new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
                        Frame.Balls[(uint)count].state.Location = Balls[key].pos;
                        Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
                    }
                }

                Dictionary<uint, uint> Related = new Dictionary<uint, uint>();
                foreach (var key1 in Frame.Balls.Keys)
                {
                    foreach (var key2 in Frame.Balls.Keys)
                    {
                        if (key1 != key2)
                        {
                            if (Frame.Balls[key1].vision.pos.DistanceFrom(Frame.Balls[key2].vision.pos) < 50)
                            {
                                if (Frame.Balls[key1].vision.notSeen < Frame.Balls[key2].vision.notSeen)
                                {
                                    Related.Add(key1, key2);
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (var key in Related.Keys)
                {
                    if (Frame.Balls.ContainsKey(Related[key]))
                        Frame.Balls.Remove(Related[key]);
                }
                while (Frame.Balls.Count >= StaticVariables.MaxBalls)
                    Frame.Balls.Remove(Frame.Balls.ElementAt(Frame.Balls.Count - 1).Key);
            }
            #endregion
            return Frame;
        }
        public frame FillHistoryNew(frame Frame, List<Dictionary<uint, vrobot>> modelHistory, Dictionary<uint, vraw> Balls, bool FillBall, bool FillRobots)
        {
            #region Robot Section
            if (FillRobots)
            {
                foreach (var searchKey in Frame.OurRobots.Keys)
                {
                    Frame.OurRobots[searchKey].vision.notSeen++;
                    Frame.OurRobots[searchKey].visionProblem = false;
                  //  DrawingObjects.AddObject(new Circle(new Position2D(Frame.OurRobots[searchKey].vision.pos.X / 1000, -Frame.OurRobots[searchKey].vision.pos.Y / 1000), 0.09, new Pen(Color.Red, 0.01f)));
                    if (Frame.OurRobots[searchKey].vision.notSeen > 0 && modelHistory.Count > 0)
                    {
                        if (modelHistory[0].ContainsKey(searchKey))
                        {
                            int notseen = Frame.OurRobots[searchKey].vision.notSeen;
                            Frame.OurRobots[searchKey].vision = new vraw(Frame.timeofcapture, modelHistory[0][searchKey].viewstate.Location, (float)(modelHistory[0][searchKey].viewstate.Angle.Value + Math.PI / 2), 1, Frame.OurRobots[searchKey].vision.camera);
                            Frame.OurRobots[searchKey].visionProblem = true;
                            //DrawingObjects.AddObject(new Circle(new Position2D(modelHistory[0][searchKey].viewstate.Location.X / 1000, -modelHistory[0][searchKey].viewstate.Location.Y / 1000), 0.09));
                            //DrawingObjects.AddObject(new Line(new Position2D(modelHistory[0][searchKey].viewstate.Location.X / 1000, -modelHistory[0][searchKey].viewstate.Location.Y / 1000), new Position2D(modelHistory[0][searchKey].viewstate.Location.X / 1000, -modelHistory[0][searchKey].viewstate.Location.Y / 1000) + Vector2D.FromAngleSize(-modelHistory[0][searchKey].viewstate.Angle.Value, 1)));
                            Frame.OurRobots[searchKey].vision.notSeen = notseen;
                        }
                    }
                }
               
                var mustRemoved = Frame.OurRobots.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
                List<uint> key_or_remove = new List<uint>();
                foreach (var searchKey in mustRemoved)
                {
                    key_or_remove.Add(searchKey.Key);
                }
                foreach (var i in key_or_remove)
                {
                    Frame.OurRobots.Remove(i);
                }
            
            }
            #endregion
            #region "Ball Section"
            if (FillBall)
            {
                foreach (var searchKey in Frame.Balls.Keys)
                {
                    Frame.Balls[searchKey].vision.notSeen++;
                }
                var mustRemovedBalls = Frame.Balls.Where(w => w.Value.vision.notSeen >= MaxNotSeen).ToDictionary(k => k.Key, v => v.Value);
                var key_or_remove = new List<uint>();
                foreach (var searchKey in mustRemovedBalls)
                {
                    key_or_remove.Add(searchKey.Key);
                }
                foreach (var i in key_or_remove)
                {
                    Frame.Balls.Remove(i);
                }
                foreach (var key in Balls.Keys)
                {
                    double dist, minDist = double.MaxValue;
                    int minHistoryID = -1;
                    int lastCamViewed = -1;

                    foreach (var searchKey in Frame.Balls.Keys)
                    {
                        if (Frame.Balls[searchKey].vision.notSeen > -1)
                        {
                            dist = Balls[key].pos.DistanceFrom(Frame.Balls[searchKey].viewstate.Location);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                minHistoryID = (int)searchKey;
                                lastCamViewed = (int)Balls[key].camera;
                            }
                        }
                    }
                    int count = -1;
                    if (minHistoryID != -1)
                    {
                        if (minDist < Max_Ball_distance * 1000)
                        {
                            Frame.Balls[(uint)minHistoryID].vision.notSeen = -1;
                            Frame.Balls[(uint)minHistoryID].vision.lastCamViewd = lastCamViewed;
                            Frame.Balls[(uint)minHistoryID].vision.pos = Balls[key].pos;
                            Frame.Balls[(uint)minHistoryID].vision.timestamp = Balls[key].timestamp;
                            Frame.Balls[(uint)minHistoryID].vision.conf = Balls[key].conf;
                        //    DrawingObjects.AddObject(new Circle(Vision2AI(Frame.Balls[(uint)minHistoryID].vision.pos), 0.07, new Pen(Color.Blue, 0.01f)),"ballsss2history"+minHistoryID);
                        }
                        else
                        {

                            for (uint i = 0; i < Frame.Balls.Count; i++)
                            {
                                if (Frame.Balls.ContainsKey(i) == false)
                                {
                                    count = (int)i;
                                }
                            }
                            if (count == -1)
                            {
                                count = Math.Min(Frame.Balls.Count + 1, StaticVariables.MaxBalls - 1);
                            }
                            Frame.Balls[(uint)count] = (new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
                            Frame.Balls[(uint)count].state.Location = Balls[key].pos;
                            Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
                        //    DrawingObjects.AddObject(new Circle(Vision2AI(Frame.Balls[(uint)count].state.Location), 0.07, new Pen(Color.Pink, 0.01f)), "ballsss2history" + count);
                        }
                    }
                    else
                    {

                        for (int i = 0; i < Frame.Balls.Count; i++)
                        {
                            if (Frame.Balls.ContainsKey((uint)i) == false)
                            {
                                count = i;
                            }
                        }
                        if (count == -1)
                        {
                            count = Math.Min(Frame.Balls.Count + 1, StaticVariables.MaxBalls);
                        }
                        Frame.Balls[(uint)count] = (new vball(new vraw(Balls[key].timestamp, Balls[key].pos, 0, Balls[key].conf, Balls[key].camera), new SingleObjectState()));
                        Frame.Balls[(uint)count].state.Location = Balls[key].pos;
                        Frame.Balls[(uint)count].viewstate.Location = Balls[key].pos;
                       // DrawingObjects.AddObject(new Circle(Vision2AI(Frame.Balls[(uint)count].state.Location), 0.07, new Pen(Color.Brown, 0.01f)), "ballsss2history" + count);
                    }
                }

                Dictionary<uint, uint> Related = new Dictionary<uint, uint>();
                foreach (var key1 in Frame.Balls.Keys)
                {
                    foreach (var key2 in Frame.Balls.Keys)
                    {
                        if (key1 != key2)
                        {
                            if (Frame.Balls[key1].vision.pos.DistanceFrom(Frame.Balls[key2].vision.pos) < 50)
                            {
                                if (Frame.Balls[key1].vision.notSeen < Frame.Balls[key2].vision.notSeen)
                                {
                                    Related.Add(key1, key2);
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (var key in Related.Keys)
                {
                    if (Frame.Balls.ContainsKey(Related[key]))
                        Frame.Balls.Remove(Related[key]);
                }
                while (Frame.Balls.Count >= StaticVariables.MaxBalls)
                    Frame.Balls.Remove(Frame.Balls.ElementAt(Frame.Balls.Count - 1).Key);
            }
            #endregion
            return Frame;
        }
   
        public void CallObservs(frame Frame, RobotCommands Commands)
        {
            SetConfig(Frame);
            for (int i = 0; i < StaticVariables.MaxBalls; i++)
            {
                if (bIndex2id[i] >= 0)
                {
                    ball[i].observe(Frame.Balls[(uint)bIndex2id[i]].vision, Frame.Balls[(uint)bIndex2id[i]].vision.timestamp);
                }
            }
            //HiPerfTimer ht = new HiPerfTimer();
            //ht.Start();
            if (Commands != null && Commands.Commands != null)
            {
                foreach (var item in Commands.Commands)
                {
                    int idx = id2index[0, item.Key];
                    if (idx >= 0)
                        robots[0, idx].command(Frame.OurRobots[(uint)item.Key].vision.timestamp, new Vector3D(item.Value.Vy * 1000, -item.Value.Vx * 1000, item.Value.W));  //IMPORTANT: Vx <-> Vy ,  Vx -> -Vx,  for ex: Command = (Vx,Vy, W) => (Vy, -Vx, W)
                }
            }
            for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
            {
                if (index2id[0, i] >= 0)
                    robots[0, i].observe(Frame.OurRobots[(uint)index2id[0, i]].visionProblem,Frame.OurRobots[(uint)index2id[0, i]].vision, Frame.OurRobots[(uint)index2id[0, i]].vision.timestamp);
            }
            for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
            {
                if (index2id[1, i] >= 0)
                    robots[1, i].observe(false, Frame.OppRobots[(uint)index2id[1, i]].vision, Frame.OppRobots[(uint)index2id[1, i]].vision.timestamp);
            }

        }
        public void CallObservs(frame Frame, RobotCommands Commands, bool ObserveBall, bool ObserveRobots)
        {
            //DrawingObjects.AddObject(new StringDraw("9", new Position2D(0.6, 0.9)), "drw9");
            SetConfig(Frame, ObserveBall, ObserveRobots);
            //DrawingObjects.AddObject(new StringDraw("10", new Position2D(0.6, 1)), "drw10");
            if (ObserveRobots)
            {
                if (Commands != null && Commands.Commands != null)
                {
                    //DrawingObjects.AddObject(new StringDraw("11", new Position2D(0.6, 1.1)), "drw11");
                    foreach (var item in Commands.Commands)
                    {
                        int idx = id2index[0, item.Key];
                        if (idx >= 0)
                            robots[0, idx].command(Frame.OurRobots[(uint)item.Key].vision.timestamp, new Vector3D(item.Value.Vy * 1000, -item.Value.Vx * 1000, item.Value.W));  //IMPORTANT: Vx <-> Vy ,  Vx -> -Vx,  for ex: Command = (Vx,Vy, W) => (Vy, -Vx, W)
                    }
                    //DrawingObjects.AddObject(new StringDraw("12", new Position2D(0.6, 1.2)), "drw12");
                }
            }
            if (ObserveRobots || ObserveBall)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[0, i] >= 0)
                        robots[0, i].observe(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, Frame.OurRobots[(uint)index2id[0, i]].vision, Frame.OurRobots[(uint)index2id[0, i]].vision.timestamp);
                    //if (index2id[0, i] == 2)
                    //    DrawingObjects.AddObject(new StringDraw("conf: " + Frame.OurRobots[(uint)index2id[0, i]].vision.conf, Position2D.Zero));
                }
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[1, i] >= 0)
                        robots[1, i].observe(false, Frame.OppRobots[(uint)index2id[1, i]].vision, Frame.OppRobots[(uint)index2id[1, i]].vision.timestamp);
                }

            }
            if (ObserveBall)
            {
                for (int i = 0; i < StaticVariables.MaxBalls; i++)
                {
                    if (bIndex2id[i] >= 0)
                    {
                        ball[i].observe(Frame.Balls[(uint)bIndex2id[i]].vision, Frame.Balls[(uint)bIndex2id[i]].vision.timestamp);
                    }
                }
            }
        }
        public void CallObservsNew(frame Frame, RobotCommands Commands, bool ObserveBall, bool ObserveRobots)
        {
            //DrawingObjects.AddObject(new StringDraw("9", new Position2D(0.6, 0.9)), "drw9");
            SetConfig(Frame, ObserveBall, ObserveRobots);
            //DrawingObjects.AddObject(new StringDraw("10", new Position2D(0.6, 1)), "drw10");
            if (ObserveRobots)
            {
                if (Commands != null && Commands.Commands != null)
                {
                    //DrawingObjects.AddObject(new StringDraw("11", new Position2D(0.6, 1.1)), "drw11");
                    foreach (var item in Commands.Commands)
                    {
                        int idx = id2index[0, item.Key];
                        if (idx >= 0)
                            robots[0, idx].command(Frame.OurRobots[(uint)item.Key].vision.timestamp, new Vector3D(item.Value.Vy * 1000, -item.Value.Vx * 1000, item.Value.W));  //IMPORTANT: Vx <-> Vy ,  Vx -> -Vx,  for ex: Command = (Vx,Vy, W) => (Vy, -Vx, W)
                    }
                    //DrawingObjects.AddObject(new StringDraw("12", new Position2D(0.6, 1.2)), "drw12");
                }
            }
            if (ObserveRobots || ObserveBall)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[0, i] >= 0)
                        robots[0, i].observe(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, Frame.OurRobots[(uint)index2id[0, i]].vision, Frame.OurRobots[(uint)index2id[0, i]].vision.timestamp);
                    //if (index2id[0, i] == 2)
                    //    DrawingObjects.AddObject(new StringDraw("conf: " + Frame.OurRobots[(uint)index2id[0, i]].vision.conf, Position2D.Zero));
                }
                if (ObserveBall)
                {
                    for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    {
                        if (index2id[1, i] >= 0)
                            robots[1, i].observe(false, Frame.OppRobots[(uint)index2id[1, i]].vision, Frame.OppRobots[(uint)index2id[1, i]].vision.timestamp);
                    }
                }
            }
            if (ObserveBall)
            {
                for (int i = 0; i < StaticVariables.MaxBalls; i++)
                {
                    if (bIndex2id[i] >= 0)
                    {
                        ball[i].observe(Frame.Balls[(uint)bIndex2id[i]].vision, Frame.Balls[(uint)bIndex2id[i]].vision.timestamp);
                    }
                }
            }
        }
        public void CallObservsNew(frame Frame, RobotCommands Commands, bool ObserveBall, bool ObserveRobots, bool newCordinate)
        {
            //DrawingObjects.AddObject(new StringDraw("9", new Position2D(0.6, 0.9)), "drw9");
            SetConfig(Frame, ObserveBall, ObserveRobots);
            //DrawingObjects.AddObject(new StringDraw("10", new Position2D(0.6, 1)), "drw10");
            if (ObserveRobots)
            {
                if (Commands != null && Commands.Commands != null)
                {
                    //DrawingObjects.AddObject(new StringDraw("11", new Position2D(0.6, 1.1)), "drw11");
                    foreach (var item in Commands.Commands)
                    {
                        int idx = id2index[0, item.Key];
                        if (idx >= 0)
                        {
                            robots[0, idx].command(Frame.OurRobots[(uint)item.Key].vision.timestamp, (!double.IsNaN(item.Value.Vx) && !double.IsNaN(item.Value.Vy) && !double.IsNaN(item.Value.W) && !double.IsInfinity(item.Value.Vx) && !double.IsInfinity(item.Value.Vy) && !double.IsInfinity(item.Value.W)) ? new Vector3D(item.Value.Vx * 1000, item.Value.Vy * 1000, item.Value.W) : new Vector3D());
                        }
                    }
                    //DrawingObjects.AddObject(new StringDraw("12", new Position2D(0.6, 1.2)), "drw12");
                }
            }
            if (ObserveRobots || ObserveBall)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[0, i] >= 0)
                        robots[0, i].observe(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, Frame.OurRobots[(uint)index2id[0, i]].vision, Frame.OurRobots[(uint)index2id[0, i]].vision.timestamp);
                    //if (index2id[0, i] == 2)
                    //    DrawingObjects.AddObject(new StringDraw("conf: " + Frame.OurRobots[(uint)index2id[0, i]].vision.conf, Position2D.Zero));
                }
                if (ObserveBall)
                {
                    for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    {
                        if (index2id[1, i] >= 0)
                            robots[1, i].observe(false, Frame.OppRobots[(uint)index2id[1, i]].vision, Frame.OppRobots[(uint)index2id[1, i]].vision.timestamp);
                    }
                }
            }
            if (ObserveBall)
            {
                for (int i = 0; i < StaticVariables.MaxBalls; i++)
                {
                    if (bIndex2id[i] >= 0)
                    {
                        ball[i].observe(Frame.Balls[(uint)bIndex2id[i]].vision, Frame.Balls[(uint)bIndex2id[i]].vision.timestamp);
                    }
                }
            }
        }

        public void CallLongPredictedObserve(frame Frame, uint BallIndex, bool reset)
        {
            if (Frame.Balls.ContainsKey(BallIndex))
            {
                if (reset)
                    BallLongPredict.reset();
                BallLongPredict.observe(Frame.Balls[BallIndex].vision, Frame.Balls[BallIndex].vision.timestamp);
            }
        }
    
        public frame GetEstimated(frame Frame)
        {
            for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
            {
                if (index2id[0, i] >= 0)
                {
                    vrobot tmp1 = GetRobotData(Frame.OurRobots[(uint)index2id[0, i]].visionProblem,0, i, StaticVariables.RobotPredictTime);
                    Frame.OurRobots[(uint)index2id[0, i]].state = tmp1.state;
                    vrobot tmp = GetRobotData(Frame.OurRobots[(uint)index2id[0, i]].visionProblem,0, i, StaticVariables.viewDelay);
                    Frame.OurRobots[(uint)index2id[0, i]].viewstate = tmp.state;
                }
            }
            for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
            {
                if (index2id[1, i] >= 0)
                {
                    vrobot tmp1 = GetRobotData(false, 1, i, 0);
                    Frame.OppRobots[(uint)index2id[1, i]].state = tmp1.state;
                    vrobot tmp = GetRobotData(false, 1, i, StaticVariables.viewDelay);
                    Frame.OppRobots[(uint)index2id[1, i]].viewstate = tmp.state;
                }
            }

            for (int i = 0; i < StaticVariables.MaxBalls; i++)
            {
                if (bIndex2id[i] >= 0)
                {
                    vball tmp1 = GetBallData(StaticVariables.BallPredictTime, i);
                    Frame.Balls[(uint)bIndex2id[i]].state = tmp1.state;
                    Frame.Balls[(uint)bIndex2id[i]].occluded = tmp1.occluded;
                    Frame.Balls[(uint)bIndex2id[i]].occluding_team = tmp1.occluding_team;
                    Frame.Balls[(uint)bIndex2id[i]].occluding_robot = tmp1.occluding_robot;
                    Frame.Balls[(uint)bIndex2id[i]].occluding_offset = tmp1.occluding_offset;
                    Frame.Balls[(uint)bIndex2id[i]].variances = tmp1.variances;

                    Frame.Balls[(uint)bIndex2id[i]].colision = tmp1.colision;
                    vball tmp = GetBallData(StaticVariables.viewDelay, i);
                    Frame.Balls[(uint)bIndex2id[i]].viewstate = tmp.state;
                }
            }

            return Frame;
        }
        public frame GetEstimated(frame Frame, bool EstimateBall, bool EstimateRobot)
        {
            if (EstimateRobot)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[0, i] >= 0)
                    {
                        vrobot tmp1 = GetRobotData(Frame.OurRobots[(uint)index2id[0, i]].visionProblem,0, i, StaticVariables.RobotPredictTime);
                        Frame.OurRobots[(uint)index2id[0, i]].state = tmp1.state;
                        vrobot tmp = GetRobotData(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, 0, i, StaticVariables.viewDelay);
                        Frame.OurRobots[(uint)index2id[0, i]].viewstate = tmp.state;
                        //if (index2id[0, i] == 2)
                        //{
                        //    DrawingObjects.AddObject(new StringDraw("stuck: " + GetStuck(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, 0, i, StaticVariables.RobotPredictTime), new Position2D(Frame.OurRobots[(uint)index2id[0, i]].state.Location.X / 1000, -Frame.OurRobots[(uint)index2id[0, i]].state.Location.Y / 1000) + new Vector2D(0.5, 0.5)));
                        //    DrawingObjects.AddObject(new StringDraw("vp: " + Frame.OurRobots[(uint)index2id[0, i]].visionProblem, new Position2D(Frame.OurRobots[(uint)index2id[0, i]].state.Location.X / 1000, -Frame.OurRobots[(uint)index2id[0, i]].state.Location.Y / 1000) + new Vector2D(0.2, 0.2)));
                        //}
                    }
                }
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[1, i] >= 0)
                    {
                        vrobot tmp1 = GetRobotData(false,1, i, StaticVariables.action_delay);
                        Frame.OppRobots[(uint)index2id[1, i]].state = tmp1.state;
                        //  vrobot tmp = GetRobotData(1, i, StaticVariables.viewDelay);
                        // Frame.OppRobots[(uint)index2id[1, i]].viewstate = tmp.state;
                    }
                }
            }
            if (EstimateBall)
            {
                for (int i = 0; i < StaticVariables.MaxBalls; i++)
                {
                    if (bIndex2id[i] >= 0)
                    {
                        vball tmp1 = GetBallData(StaticVariables.action_delay, i);
                        Frame.Balls[(uint)bIndex2id[i]].state = tmp1.state;
                        Frame.Balls[(uint)bIndex2id[i]].occluded = tmp1.occluded;
                        Frame.Balls[(uint)bIndex2id[i]].occluding_team = tmp1.occluding_team;
                        Frame.Balls[(uint)bIndex2id[i]].occluding_robot = tmp1.occluding_robot;
                        Frame.Balls[(uint)bIndex2id[i]].occluding_offset = tmp1.occluding_offset;
                        Frame.Balls[(uint)bIndex2id[i]].variances = tmp1.variances;
                        Frame.Balls[(uint)bIndex2id[i]].colision = tmp1.colision;
                        vball tmp = GetBallData(StaticVariables.viewDelay, i);
                        Frame.Balls[(uint)bIndex2id[i]].viewstate = tmp.state;
                    }
                }
            }
            return Frame;
        }
        public frame GetEstimatedNew(frame Frame, bool EstimateBall, bool EstimateRobot)
        {
            if (EstimateRobot)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[0, i] >= 0)
                    {
                        vrobot tmp1 = GetRobotData(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, 0, i, StaticVariables.RobotPredictTime);
                        Frame.OurRobots[(uint)index2id[0, i]].state = tmp1.state;
                        vrobot tmp = GetRobotData(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, 0, i, StaticVariables.viewDelay);
                        Frame.OurRobots[(uint)index2id[0, i]].viewstate = tmp.state;
                        // if (index2id[0, i] == 6)
                        //{
                        //    DrawingObjects.AddObject(new StringDraw("stuck: " + GetStuck(Frame.OurRobots[(uint)index2id[0, i]].visionProblem, 0, i, StaticVariables.RobotPredictTime), -new Position2D(Frame.OurRobots[(uint)index2id[0, i]].state.Location.X / 1000, -Frame.OurRobots[(uint)index2id[0, i]].state.Location.Y / 1000) + new Vector2D(0.5, 0.5)));
                        //    DrawingObjects.AddObject(new StringDraw("vp: " + Frame.OurRobots[(uint)index2id[0, i]].visionProblem, new Position2D(Frame.OurRobots[(uint)index2id[0, i]].state.Location.X / 1000, -Frame.OurRobots[(uint)index2id[0, i]].state.Location.Y / 1000) + new Vector2D(0.2, 0.2)));
                        //}
                    }
                }
            }
            if (EstimateBall)
            {
                for (int i = 0; i < StaticVariables.MaxBalls; i++)
                {
                    if (bIndex2id[i] >= 0)
                    {
                        vball tmp1 = GetBallData(StaticVariables.action_delay, i);
                     //   DrawingObjects.AddObject(new Circle(Vision2AI(tmp1.state.Location), 0.05, new Pen(Color.Red, 0.01f)));
                        Frame.Balls[(uint)bIndex2id[i]].state = tmp1.state;
                        Frame.Balls[(uint)bIndex2id[i]].occluded = tmp1.occluded;
                        Frame.Balls[(uint)bIndex2id[i]].occluding_team = tmp1.occluding_team;
                        Frame.Balls[(uint)bIndex2id[i]].occluding_robot = tmp1.occluding_robot;
                        Frame.Balls[(uint)bIndex2id[i]].occluding_offset = tmp1.occluding_offset;
                        Frame.Balls[(uint)bIndex2id[i]].variances = tmp1.variances;
                        Frame.Balls[(uint)bIndex2id[i]].colision = tmp1.colision;
                        vball tmp = GetBallData(StaticVariables.viewDelay, i);
                        Frame.Balls[(uint)bIndex2id[i]].viewstate = tmp.state;
                    }
                }
            }
            return Frame;
        }
  
        public void setTracker(int idx)
        {
            ball[idx].set_tracker(this);
        }
        // set the configuration for the EKBF's 
        public void SetConfig(ref net_vconfig vcfg)
        {


            // clear out idnexes and types
            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    index2id[t, i] = -1;
                    robots[t, i].set_type(RobotType.Default);
                }
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    id2index[t, i] = -1;
            }

            // set it all
            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (vcfg.teams[t].robots[i].id >= 0)
                    {
                        index2id[t, i] = vcfg.teams[t].robots[i].id;
                        id2index[t, vcfg.teams[t].robots[i].id] = i;
                        robots[t, i].set_type(vcfg.teams[t].robots[i].type);
                    }
                }
            }
            for (int i = 0; i < StaticVariables.MaxBalls; i++)
            {
                bId2index[i] = -1;
                bIndex2id[i] = -1;
                if (vcfg.balls[i].id >= 0)
                {
                    bIndex2id[i] = vcfg.balls[i].id;
                    bId2index[vcfg.balls[i].id] = i;
                }
            }
        }
        public void SetConfig(frame Frame)
        {
            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    index2id[t, i] = -1;
                    //   robots[t, i].set_type(RobotType.Default);
                }
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    id2index[t, i] = -1;
            }
            for (int i = 0; i < Frame.OurRobots.Count; i++)
            {
                index2id[0, i] = (int)Frame.OurRobots.ElementAt(i).Key;
                id2index[0, (int)Frame.OurRobots.ElementAt(i).Key] = i;
                //robots[0, i].set_type(Frame.type);
            }
            for (int i = 0; i < Frame.OppRobots.Count; i++)
            {
                index2id[1, i] = (int)Frame.OppRobots.ElementAt(i).Key;
                id2index[1, (int)Frame.OppRobots.ElementAt(i).Key] = i;
                // robots[1, i].set_type(Frame.type);
            }
            for (int i = 0; i < StaticVariables.MaxBalls; i++)
            {
                bId2index[i] = -1;
                bIndex2id[i] = -1;
            }
            for (int i = 0; i < Frame.Balls.Count; i++)
            {
                bIndex2id[i] = (int)Frame.Balls.ElementAt(i).Key;
                bId2index[(int)Frame.Balls.ElementAt(i).Key] = i;
            }
        }
        public void SetConfig(frame Frame, bool SetBall, bool SetRobot)
        {

            if (!SetRobot)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    index2id[0, i] = -1;
                    robots[0, i].set_type(RobotType.Default);
                }
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    id2index[0, i] = -1;
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    index2id[1, i] = -1;
                    robots[1, i].set_type(RobotType.Default);
                }
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    id2index[1, i] = -1;
            }
            else
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (!Frame.OurRobots.ContainsKey((uint)i))
                    {
                        robots[0, i].set_type(RobotType.Default);
                        index2id[0, i] = -1;
                        id2index[0, i] = -1;
                    }
                    else
                    {
                        index2id[0, i] = i;
                        id2index[0, i] = i;
                    }

                    if (!Frame.OppRobots.ContainsKey((uint)i))
                    {
                        robots[1, i].set_type(RobotType.Default);
                        index2id[1, i] = -1;
                        id2index[1, i] = -1;
                    }
                    else
                    {
                        index2id[1, i] = i;
                        id2index[1, i] = i;
                    }
                }
            }
            //for (int i = 0; i < Frame.OurRobots.Count; i++)
            //{
            //    index2id[0, i] = (int)Frame.OurRobots.ElementAt(i).Key;
            //    id2index[0, (int)Frame.OurRobots.ElementAt(i).Key] = i;
            //}
            //for (int i = 0; i < Frame.OppRobots.Count; i++)
            //{
            //    index2id[1, i] = (int)Frame.OppRobots.ElementAt(i).Key;
            //    id2index[1, (int)Frame.OppRobots.ElementAt(i).Key] = i;
            //}
          
            if (SetBall)
            {
                //for (int i = 0; i < StaticVariables.MaxBalls; i++)
                //{
                //    bId2index[i] = -1;
                //    bIndex2id[i] = -1;
                //}
                //for (int i = 0; i < Frame.Balls.Count; i++)
                //{
                //    bIndex2id[i] = (int)Frame.Balls.ElementAt(i).Key;
                //    bId2index[(int)Frame.Balls.ElementAt(i).Key] = i;
                //}
                for (int i = 0; i < StaticVariables.MaxBalls; i++)
                {
                    if (!Frame.Balls.ContainsKey((uint)i))
                    {
                        ball[i].reset();
                        bId2index[i] = -1;
                        bIndex2id[i] = -1;
                    }
                    else
                    {
                        bId2index[i] = i;
                        bIndex2id[i] = i;
                    }
                }
            }
        }
        public void SetConfig(frame Frame, bool Sball)
        {
            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    index2id[t, i] = -1;
                    //   robots[t, i].set_type(RobotType.Default);
                }
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                    id2index[t, i] = -1;
            }
            for (int i = 0; i < Frame.OurRobots.Count; i++)
            {
                index2id[0, i] = (int)Frame.OurRobots.ElementAt(i).Key;
                id2index[0, (int)Frame.OurRobots.ElementAt(i).Key] = i;
                //robots[0, i].set_type(Frame.type);
            }
            for (int i = 0; i < Frame.OppRobots.Count; i++)
            {
                index2id[1, i] = (int)Frame.OppRobots.ElementAt(i).Key;
                id2index[1, (int)Frame.OppRobots.ElementAt(i).Key] = i;
                // robots[1, i].set_type(Frame.type);
            }
        }
     
        public void ResetAll()
        {
            for (int i = 0; i < StaticVariables.MaxBalls; i++)
            {
                if (bIndex2id[i] > 0)
                    ball[i].reset();
            }
            for (int t = 0; t < StaticVariables.NUM_TEAMS; t++)
            {
                for (int i = 0; i < StaticVariables.MAX_ROBOT_ID; i++)
                {
                    if (index2id[t, i] >= 0)
                        robots[t, i].reset();
                }
            }
        }
     
        public void GetBallData(ref vball vb, double dt, int idx)
        {
            // fill out tracking info
            bcovar = ball[idx].covariances(dt);
            //  vb.variances = new MathMatrix(bcovar);
            int t = 0, r = 0;
            Position2D tmp = ball[idx].position(dt);
            vb.state.Location = tmp;
            //vb.state.x = (float)tmp.X;
            //vb.state.y = (float)tmp.Y;
            Vector2D tmpv = ball[idx].velocity(dt);
            //vb.state.vx = (float)tmpv.X;
            //vb.state.vy = (float)tmpv.Y;
            vb.state.Speed = tmpv;
            vb.occluded = ball[idx].occluded;
            vb.occluding_team = ball[idx].occluding_team;
            vb.occluding_robot = ball[idx].occluding_robot;
            vb.occluding_offset = new Vector2D(ball[idx].occluding_offset.X, ball[idx].occluding_offset.Y);
            vb.colision = ball[idx].collision(dt, ref t, ref r);
        }
      
        public vball GetBallData(double dt, int idx)
        {
            vball vb = new vball();
            int t = 0, r = 0;
            bcovar = ball[idx].covariances(dt);
            vb.variances = bcovar.Copy();
            Position2D tmp = ball[idx].position(dt);
            vb.state.Location = tmp;
            Vector2D tmpv = ball[idx].velocity(dt);
            vb.state.Speed = tmpv;
            vb.occluded = ball[idx].occluded;
            vb.occluding_team = ball[idx].occluding_team;
            vb.occluding_robot = ball[idx].occluding_robot;
            vb.occluding_offset = new Vector2D(ball[idx].occluding_offset.X, ball[idx].occluding_offset.Y);
            vb.colision = ball[idx].collision(dt, ref t, ref r);
            return vb;
        }
        public void GetRobotData(bool visionProblem, ref vrobot vr, int team, int indx, double dt)
        {
            // Get the state information from the Kalman filter
            Position2D tmp = robots[team, indx].position(visionProblem, dt);
            vr.state.Location = tmp;
            //vr.state.x = (float)tmp.X;
            //vr.state.y = (float)tmp.Y;
            //vr.state.theta = robots[team, indx].direction(dt);
            vr.state.Angle = (float)robots[team, indx].direction(visionProblem, dt);

            Vector2D tmpv = robots[team, indx].velocity_raw(visionProblem, dt);
            vr.state.Speed = tmpv;
            //vr.state.vx = (float)tmpv.X;
            //vr.state.vy = (float)tmpv.Y;
            //vr.state.vtheta = robots[team, indx].angular_velocity(dt);
            vr.state.AngularSpeed = (float)robots[team, indx].angular_velocity(visionProblem, dt);
            //vr.state.stuck = robots[team, indx].stuck(dt);
        }
        public vrobot GetRobotData(bool visionProblem, int team, int indx, double dt)
        {
            vrobot vr = new vrobot();
            Position2D tmp = robots[team, indx].position(visionProblem, dt);
            vr.state.Location = tmp;
            vr.state.Angle = (float)robots[team, indx].direction(visionProblem, dt);

            Vector2D tmpv = robots[team, indx].velocity_raw(visionProblem, dt);
            vr.state.Speed = tmpv;
            vr.state.AngularSpeed = (float)robots[team, indx].angular_velocity(visionProblem, dt);
            return vr;
        }
        public double GetStuck(bool visionProblem, int team, int idx, double dt)
        {
            return robots[team, idx].stuck(visionProblem, dt);
        }
       
        public WorldModel CreateModelWitoutBall(frame Frame)
        {
            WorldModel Model = new WorldModel();
            Model.OurRobots = new Dictionary<int, SingleObjectState>();
            Model.Opponents = new Dictionary<int, SingleObjectState>();
            Model.BallState = new SingleObjectState();
            #region Our
            Vector2D VRobotInField = new Vector2D();
            foreach (var item in Frame.OurRobots.ToDictionary(k => k.Key, v => v.Value))
            {
                if (Frame.OurRobots[item.Key].vision.notSeen < Max_to_imagine + 15)
                {

                    VRobotInField.X = item.Value.state.Speed.X * Math.Cos(item.Value.state.Angle.Value) - item.Value.state.Speed.Y * Math.Sin(item.Value.state.Angle.Value);
                    VRobotInField.Y = item.Value.state.Speed.Y * Math.Cos(item.Value.state.Angle.Value) + item.Value.state.Speed.X * Math.Sin(item.Value.state.Angle.Value);
                    float? Angle = (float)(-1 * ((180 / Math.PI) * item.Value.state.Angle.Value));
                    if (Angle > 180)
                        Angle -= 360;
                    else if (Angle < -180)
                        Angle += 360;
                    Model.OurRobots[(int)item.Key] = new SingleObjectState(ObjectType.OurRobot, new Position2D(item.Value.state.Location.X / 1000, -item.Value.state.Location.Y / 1000), new Vector2D(VRobotInField.X / 1000, -VRobotInField.Y / 1000), new Vector2D(), Angle, -item.Value.state.AngularSpeed);
                }
            }
            #endregion
            #region Opp
            foreach (var item in Frame.OppRobots.ToDictionary(k => k.Key, v => v.Value))
            {
                if (Frame.OppRobots[item.Key].vision.notSeen < Max_to_imagine + 15)
                {
                    VRobotInField.X = item.Value.state.Speed.X * Math.Cos(item.Value.state.Angle.Value) - item.Value.state.Speed.Y * Math.Sin(item.Value.state.Angle.Value);
                    VRobotInField.Y = item.Value.state.Speed.Y * Math.Cos(item.Value.state.Angle.Value) + item.Value.state.Speed.X * Math.Sin(item.Value.state.Angle.Value);
                    float? Angle = (float)(-1 * ((180 / Math.PI) * item.Value.state.Angle.Value));
                    if (Angle > 180)
                        Angle -= 360;
                    else if (Angle < -180)
                        Angle += 360;
                    Model.Opponents[(int)item.Key] = new SingleObjectState(ObjectType.Opponent, new Position2D(item.Value.state.Location.X / 1000, -item.Value.state.Location.Y / 1000), new Vector2D(VRobotInField.X / 1000, -VRobotInField.Y / 1000), new Vector2D(), Angle, -item.Value.state.AngularSpeed);
                }
            }
            #endregion
            return Model;
        }
        public WorldModel CreateModelWitoutBallNew(frame Frame)
        {
            WorldModel Model = new WorldModel();
            Model.OurRobots = new Dictionary<int, SingleObjectState>();
            Model.Opponents = new Dictionary<int, SingleObjectState>();
            Model.BallState = new SingleObjectState();
            #region Our
            Vector2D VRobotInField = new Vector2D();
            foreach (var item in Frame.OurRobots.ToDictionary(k => k.Key, v => v.Value))
            {
                if (Frame.OurRobots[item.Key].vision.notSeen < Max_to_imagine + 15)
                {

                    VRobotInField.X = item.Value.state.Speed.X * Math.Cos(item.Value.state.Angle.Value) - item.Value.state.Speed.Y * Math.Sin(item.Value.state.Angle.Value);
                    VRobotInField.Y = item.Value.state.Speed.X * Math.Sin(item.Value.state.Angle.Value) + item.Value.state.Speed.Y * Math.Cos(item.Value.state.Angle.Value) ;
                    float? Angle = (float)(-1 * ((180 / Math.PI) * item.Value.state.Angle.Value));
                    if (Angle > 180)
                        Angle -= 360;
                    else if (Angle < -180)
                        Angle += 360;
                    Model.OurRobots[(int)item.Key] = new SingleObjectState(ObjectType.OurRobot, new Position2D(item.Value.state.Location.X / 1000, -item.Value.state.Location.Y / 1000), new Vector2D(VRobotInField.X / 1000, -VRobotInField.Y / 1000), new Vector2D(), Angle, -item.Value.state.AngularSpeed);
                }
            }
            #endregion
            return Model;
        }
        public WorldModel CreateModelWitoutBallNew(frame Frame,bool newCordinate)
        {
            WorldModel Model = new WorldModel();
            Model.OurRobots = new Dictionary<int, SingleObjectState>();
            Model.Opponents = new Dictionary<int, SingleObjectState>();
            Model.BallState = new SingleObjectState();
            #region Our
            Vector2D VRobotInField = new Vector2D();
            foreach (var item in Frame.OurRobots.ToDictionary(k => k.Key, v => v.Value))
            {
                if (Frame.OurRobots[item.Key].vision.notSeen < Max_to_imagine + 15)
                {

                    VRobotInField.X = item.Value.state.Speed.X * Math.Cos(item.Value.state.Angle.Value) - item.Value.state.Speed.Y * Math.Sin(item.Value.state.Angle.Value);
                    VRobotInField.Y = item.Value.state.Speed.X * Math.Sin(item.Value.state.Angle.Value) + item.Value.state.Speed.Y * Math.Cos(item.Value.state.Angle.Value);
                    float? Angle = (float)(item.Value.state.Angle.Value + Math.PI / 2).ToDegree();
                    Angle *= -1 ;
                    if (Angle > 180)
                        Angle -= 360;
                    else if (Angle < -180)
                        Angle += 360;
                    Model.OurRobots[(int)item.Key] = new SingleObjectState(ObjectType.OurRobot, new Position2D(item.Value.state.Location.X / 1000, -item.Value.state.Location.Y / 1000), new Vector2D(VRobotInField.X / 1000, -VRobotInField.Y / 1000), new Vector2D(), Angle, -item.Value.state.AngularSpeed);
                }
            }
            #endregion
            return Model;
        }

        public bool SelectedBallIndexViewed(frame Frame, uint BallIndex)
        {
            bool ret = false;
            if (Frame.Balls.ContainsKey(BallIndex))
                if (Frame.Balls[BallIndex].vision.notSeen < Max_to_imagine)
                    ret = true;

            if (ret == true)
            {
                Position2D tmp = Vision2AI(Frame.Balls[BallIndex].vision.pos);
                if (tmp.X > GameParameters.OppGoalLeft.X - StaticVariables.FieldMargin && tmp.X < GameParameters.OurGoalLeft.X + StaticVariables.FieldMargin && tmp.Y > GameParameters.OurRightCorner.Y - StaticVariables.FieldMargin && tmp.Y < GameParameters.OurLeftCorner.Y + StaticVariables.FieldMargin)
                {
                    return ret;
                }
                else
                {
                    ret = false;
                }
            }
            return ret;
        }
        public Position2D ReturnSelectedBallIndexPosition(frame Frame, uint BallIndex)
        {
            return new Position2D(Frame.Balls[BallIndex].state.Location.X, Frame.Balls[BallIndex].state.Location.Y);
        }
        public Vector2D ReturnSelectedBallIndexSpeed(frame Frame, uint BallIndex)
        {
            return new Vector2D(Frame.Balls[BallIndex].state.Speed.X, Frame.Balls[BallIndex].state.Speed.Y);
        }
        bool chkCol = false;
        public List<SingleObjectState> GetPredictedBallStateList(frame Frame, uint BallIndex, double MaxPredictTime, bool isReverseSide, bool checkCollision, bool col)
        {
            bool reset = !checkCollision && col;
            CallLongPredictedObserve(Frame, BallIndex, reset);
            if (reset)
                chkCol = true;
            if (chkCol)
                checkCollision = chkCol;
            double reverse = (isReverseSide) ? -1 : 1;
            double step = StaticVariables.FRAME_PERIOD;
            List<SingleObjectState> res = new List<SingleObjectState>();
            BallLongPredict.covariances(MaxPredictTime, checkCollision);
            int t = 0, r = 0;
            bool b = BallLongPredict.collision(MaxPredictTime, ref t, ref r);
            if (!b)
                chkCol = false;
            int MaxCount = (int)Math.Round(MaxPredictTime / step);
            for (int i = 0; i <= MaxCount; i++)
            {
                res.Add(new SingleObjectState(reverse * Vision2AI(BallLongPredict.position(((double)i) * step, checkCollision)), reverse * Vision2AI(BallLongPredict.velocity(((double)i) * step, checkCollision)), null));
            }
            return res;
        }
        private Position2D Vision2AI(Position2D pos)
        {
            return new Position2D(pos.X / 1000, -pos.Y / 1000);
        }
        private Vector2D Vision2AI(Vector2D vec)
        {
            return new Vector2D(vec.X / 1000, -vec.Y / 1000);
        }
        private double Vision2AI(double ang)
        {
            float Angle = (float)(-1 * ((180 / Math.PI) * ang));
            if (Angle > 180)
                Angle -= 360;
            else if (Angle < -180)
                Angle += 360;
            return Angle;
        }
    }
}
