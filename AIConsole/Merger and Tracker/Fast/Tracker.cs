using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using Enterprise;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public class State
    {
        Position2D Location;
        double time;
        public State(Position2D loc, double time_in_second)
        {
            this.Location = loc;
            this.time = time_in_second;
        }
    }
    public class History
    {
        public int ID;
        public int lastCamViewed;
        public int NumNotViewed;
        public Position2D Position;
        
        public Vector2D action_EstimatedAcceleration;
        public Vector2D view_EstimatedAcceleration;

        public Vector2D view_EstimatedVelocity;
        public Vector2D action_EstimatedVelocity;

        public Position2D view_EstimatedPosition;
        public Position2D action_EstimatedPosition;

        public float view_EstimatedAngle;
        public float action_EstimatedAngle;

        public ObjectType cType;
        public double? Angle;
        public KalmanFilter cKalman = new KalmanFilter();
        public Queue<State> Path = new Queue<State>(70);

        public History()
        {
            NumNotViewed = 0;
        }
        public History(ObjectType type, Position2D Pos, int id)
        {
            NumNotViewed = 0;
            this.ID = id;
            this.Position = Pos;
            this.view_EstimatedPosition = new Position2D(0, 0);
            this.cType = type;
            this.cKalman.setBallOrRobot(this.cType);
        }
        public History(ObjectType type, Position2D Pos, Position2D ePos, int id)
        {
            NumNotViewed = 0;
            this.ID = id;
            this.Position = Pos;
            this.view_EstimatedPosition = ePos;
            this.cType = type;
            this.cKalman.setBallOrRobot(this.cType);
        }
    }
    public class Tracker
    {
        public int MaxNotSeen = 120;
        public int Max_to_imagine = 1;
        public double Max_Ball_distance = 0.7;
        public double Max_Opponent_distance = 0.2;
        private const bool isOpponentsID_Detected = true;

        public Dictionary<int, History> ourRobotHistory = new Dictionary<int, History>();
        public Dictionary<int, History> opponentRobotHistory = new Dictionary<int, History>();
        public Dictionary<int, History> ballHistory = new Dictionary<int, History>();
        public WorldModel model = new WorldModel();
        public void FillHistory(WorldModel MergedData, Dictionary<int, BallState> Balls)
        {
             #region "History Update"
                 #region "our robot"
                 foreach (int searchKey in ourRobotHistory.Keys)
                 {
                      ourRobotHistory[searchKey].NumNotViewed++;
                 }

                 IEnumerable<KeyValuePair<int,History>> items1 = ourRobotHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                 List<int> key_or_remove = new List<int>();
                 foreach (KeyValuePair<int,History> searchKey in items1)
                 {
                     key_or_remove.Add(searchKey.Key);
                 }
                 foreach (int i in key_or_remove)
                 {
                     ourRobotHistory.Remove(i);
                 }

                 #endregion

                 #region "opponent robot"
                 foreach (int searchKey in opponentRobotHistory.Keys)
                 {
                     opponentRobotHistory[searchKey].NumNotViewed++;
                 }
                 IEnumerable<KeyValuePair<int, History>> items2 = opponentRobotHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                 List<int> key_op_remove = new List<int>();
                 foreach (KeyValuePair<int, History> searchKey in items2)
                 {
                     key_op_remove.Add(searchKey.Key);
                 }
                 foreach (int i in key_op_remove)
                 {
                     opponentRobotHistory.Remove(i);
                 }
                 #endregion

                 #region "Balls"
                 foreach (int searchKey in ballHistory.Keys)
                 {
                     ballHistory[searchKey].NumNotViewed++;
                 }
                 IEnumerable<KeyValuePair<int, History>> items3 = ballHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                 List<int> key_ball_remove = new List<int>();
                 foreach (KeyValuePair<int, History> searchKey in items3)
                 {
                     key_ball_remove.Add(searchKey.Key);
                 }
                 foreach (int i in key_ball_remove)
                 {
                     ballHistory.Remove(i);
                 }
                 #endregion
             #endregion
                 // update id of our robots
             #region "Our Robots Section"
             foreach (int key in MergedData.OurRobots.Keys)
             {
                 if (!ourRobotHistory.ContainsKey(key))
                 {
                     ourRobotHistory.Add(key, new History(ObjectType.OurRobot, MergedData.OurRobots[key].Location, key));
                     ourRobotHistory[key].cKalman.kalmanFilter(ourRobotHistory[key].Position, MergedData.OurRobots[key].Angle);
                     ourRobotHistory[key].action_EstimatedPosition = MergedData.OurRobots[key].Location;
                     ourRobotHistory[key].view_EstimatedPosition = MergedData.OurRobots[key].Location;
                     ourRobotHistory[key].Angle = MergedData.OurRobots[key].Angle;
                 }
                 else
                 {
                     ourRobotHistory[key].NumNotViewed = 0;
                     ourRobotHistory[key].Position = MergedData.OurRobots[key].Location;
                     ourRobotHistory[key].cKalman.kalmanFilter(ourRobotHistory[key].Position, MergedData.OurRobots[key].Angle);
                     ourRobotHistory[key].Angle = MergedData.OurRobots[key].Angle;
                 }
             }
             #endregion
             // find id of opponents
             foreach (int key in MergedData.Opponents.Keys)
             {
                 double dist,minDist = double.MaxValue;
                 int minHistoryID = -1;

                 #region "Opponent ID Detected"
                 if (isOpponentsID_Detected == true)
                 {
                     if (!opponentRobotHistory.ContainsKey(key))
                     {
                         opponentRobotHistory.Add(key, new History(ObjectType.OurRobot, MergedData.Opponents[key].Location, key));
                         opponentRobotHistory[key].cKalman.kalmanFilter(opponentRobotHistory[key].Position, MergedData.Opponents[key].Angle);
                         opponentRobotHistory[key].action_EstimatedPosition = MergedData.Opponents[key].Location;
                         opponentRobotHistory[key].view_EstimatedPosition = MergedData.Opponents[key].Location;
                         opponentRobotHistory[key].Angle = MergedData.Opponents[key].Angle;
                     }
                     else
                     {
                         opponentRobotHistory[key].NumNotViewed = 0;
                         opponentRobotHistory[key].Position = MergedData.Opponents[key].Location;
                         opponentRobotHistory[key].cKalman.kalmanFilter(opponentRobotHistory[key].Position, MergedData.Opponents[key].Angle);
                         opponentRobotHistory[key].Angle = MergedData.Opponents[key].Angle;
                     }
                 }
                 #endregion
                 #region "Opponent ID Not Detected"
                 if (!isOpponentsID_Detected)
                 {
                     foreach (int searchKey in opponentRobotHistory.Keys)
                     {
                         dist = distance(MergedData.Opponents[key].Location, opponentRobotHistory[searchKey].view_EstimatedPosition);
                         if (dist < minDist)
                         {
                             minDist = dist;
                             minHistoryID = searchKey;
                         }
                     }
                     if (minHistoryID != -1)
                     {
                         if (minDist < Max_Opponent_distance)
                         {
                             opponentRobotHistory[minHistoryID].NumNotViewed = 0;
                             opponentRobotHistory[minHistoryID].Position = MergedData.Opponents[key].Location;
                             opponentRobotHistory[minHistoryID].cKalman.kalmanFilter(opponentRobotHistory[minHistoryID].Position);
                             opponentRobotHistory[minHistoryID].Angle = MergedData.Opponents[key].Angle;
                         }
                         else
                         {
                             int count = -1;
                             for (int i = 0; i < opponentRobotHistory.Count; i++)
                             {
                                 if (opponentRobotHistory.ContainsKey(i) == false)
                                 {
                                     count = i;
                                 }
                             }
                             if (count == -1)
                             {
                                 count = opponentRobotHistory.Count + 1;
                             }
                             opponentRobotHistory.Add(count, new History(ObjectType.Opponent, MergedData.Opponents[key].Location, count));
                             opponentRobotHistory[count].cKalman.kalmanFilter(MergedData.Opponents[key].Location);
                             opponentRobotHistory[count].action_EstimatedPosition = MergedData.Opponents[key].Location;
                             opponentRobotHistory[count].view_EstimatedPosition = MergedData.Opponents[key].Location;
                             opponentRobotHistory[count].Angle = MergedData.Opponents[key].Angle;
                         }
                     }
                     else
                     {
                         opponentRobotHistory.Add(0, new History(ObjectType.Opponent, MergedData.Opponents[key].Location, 0));
                         opponentRobotHistory[0].cKalman.kalmanFilter(MergedData.Opponents[key].Location);
                         opponentRobotHistory[0].action_EstimatedPosition = MergedData.Opponents[key].Location;
                         opponentRobotHistory[0].view_EstimatedPosition = MergedData.Opponents[key].Location;
                         opponentRobotHistory[0].Angle = MergedData.Opponents[key].Angle;
                     }
                 }
                 #endregion
             }

             // find id of Balls
             #region "Ball Section"
             foreach (int key in Balls.Keys)
             {
                 double dist, minDist = double.MaxValue;
                 int minHistoryID = -1;
                 int lastCamViewed = -1;

                 foreach (int searchKey in ballHistory.Keys)
                 {
                     if (ballHistory[searchKey].NumNotViewed != 0)
                     {
                         dist = distance(Balls[key].Position, ballHistory[searchKey].view_EstimatedPosition);
                         if (dist < minDist)
                         {
                             minDist = dist;
                             minHistoryID = searchKey;
                             lastCamViewed = Balls[key].camviewed;
                         }
                     }
                 }
                 if (minHistoryID != -1)
                 {
                     if (minDist < Max_Ball_distance)
                     {
                         ballHistory[minHistoryID].NumNotViewed = 0;
                         ballHistory[minHistoryID].lastCamViewed = lastCamViewed;
                         ballHistory[minHistoryID].Position = Balls[key].Position;
                         ballHistory[minHistoryID].cKalman.kalmanFilter(ballHistory[minHistoryID].Position);
                     }
                     else
                     {
                         int count = -1;
                         for (int i = 0; i < ballHistory.Count; i++)
                         {
                             if (ballHistory.ContainsKey(i) == false)
                             {
                                 count = i;
                             }
                         }
                         if (count == -1)
                         {
                             count = ballHistory.Count + 1;
                         }
                         ballHistory.Add(count, new History(ObjectType.Ball, Balls[key].Position, count));
                         ballHistory[count].cKalman.kalmanFilter(Balls[key].Position);
                         ballHistory[count].action_EstimatedPosition = Balls[key].Position;
                         ballHistory[count].view_EstimatedPosition = Balls[key].Position;
                     }
                 }
                 else
                 {
                     int count = -1;
                     for (int i = 0; i < ballHistory.Count; i++)
                     {
                         if (ballHistory.ContainsKey(i) == false)
                         {
                             count = i;
                         }
                     }
                     if (count == -1)
                     {
                         count = ballHistory.Count + 1;
                     }
                     ballHistory.Add(count, new History(ObjectType.Ball, Balls[key].Position, count));
                     ballHistory[count].cKalman.kalmanFilter(Balls[key].Position);
                     ballHistory[count].action_EstimatedPosition = Balls[key].Position;
                     ballHistory[count].view_EstimatedPosition = Balls[key].Position;
                 }
             }
             #endregion
        }
        public frame FillHistory(frame Frame,frame newFrame, Dictionary<uint, vraw> Balls, bool FillBall, bool FillRobot)
        {
            #region "History Update"
            if (FillRobot)
            {
                #region "our robot"
                foreach (int searchKey in ourRobotHistory.Keys)
                {
                    ourRobotHistory[searchKey].NumNotViewed++;
                }

                IEnumerable<KeyValuePair<int, History>> items1 = ourRobotHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                List<int> key_or_remove = new List<int>();
                foreach (KeyValuePair<int, History> searchKey in items1)
                {
                    key_or_remove.Add(searchKey.Key);
                }
                foreach (int i in key_or_remove)
                {
                    ourRobotHistory.Remove(i);
                    if (Frame.OurRobots.ContainsKey((uint)i))
                        Frame.OurRobots.Remove((uint)i);
                }

                #endregion

                #region "opponent robot"
                foreach (int searchKey in opponentRobotHistory.Keys)
                {
                    opponentRobotHistory[searchKey].NumNotViewed++;
                }
                IEnumerable<KeyValuePair<int, History>> items2 = opponentRobotHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                List<int> key_op_remove = new List<int>();
                foreach (KeyValuePair<int, History> searchKey in items2)
                {
                    key_op_remove.Add(searchKey.Key);
                }
                foreach (int i in key_op_remove)
                {
                    opponentRobotHistory.Remove(i);
                    if (Frame.OppRobots.ContainsKey((uint)i))
                        Frame.OppRobots.Remove((uint)i);
                }
                #endregion
            }
            if (FillBall)
            {
                #region "Balls"
                foreach (int searchKey in ballHistory.Keys)
                {
                    ballHistory[searchKey].NumNotViewed++;
                }
                IEnumerable<KeyValuePair<int, History>> items3 = ballHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                List<int> key_ball_remove = new List<int>();
                foreach (KeyValuePair<int, History> searchKey in items3)
                {
                    key_ball_remove.Add(searchKey.Key);
                }
                foreach (int i in key_ball_remove)
                {
                    ballHistory.Remove(i);
                }
                #endregion
            }
            #endregion

            Position2D tmpPos;
            float? tmpAng;
            if (FillRobot)
            {
                #region "Our Robots Section"
                foreach (int key in newFrame.OurRobots.Keys)
                {
                    tmpPos = Vision2AI(newFrame.OurRobots[(uint)key].vision.pos);
                    tmpAng = (float)Vision2AI(newFrame.OurRobots[(uint)key].vision.angle);
                    if (!ourRobotHistory.ContainsKey(key))
                    {
                        ourRobotHistory.Add(key, new History(ObjectType.OurRobot, tmpPos, key));
                        ourRobotHistory[key].cKalman.kalmanFilter(ourRobotHistory[key].Position, tmpAng);
                        ourRobotHistory[key].action_EstimatedPosition = tmpPos;
                        ourRobotHistory[key].view_EstimatedPosition = tmpPos;
                        ourRobotHistory[key].Angle = tmpAng;
                    }
                    else
                    {
                        ourRobotHistory[key].NumNotViewed = 0;
                        ourRobotHistory[key].Position = tmpPos;
                        ourRobotHistory[key].cKalman.kalmanFilter(ourRobotHistory[key].Position, tmpAng);
                        ourRobotHistory[key].Angle = tmpAng;
                    }
                }
                #endregion
                // find id of opponents
                foreach (int key in newFrame.OppRobots.Keys)
                {
                    double dist, minDist = double.MaxValue;
                    int minHistoryID = -1;
                    tmpPos = Vision2AI(newFrame.OppRobots[(uint)key].vision.pos);
                    tmpAng = (float)Vision2AI(newFrame.OppRobots[(uint)key].vision.angle);
                    #region "Opponent ID Detected"
                    if (isOpponentsID_Detected == true)
                    {
                        if (!opponentRobotHistory.ContainsKey(key))
                        {
                            opponentRobotHistory.Add(key, new History(ObjectType.OurRobot, tmpPos, key));
                            opponentRobotHistory[key].cKalman.kalmanFilter(opponentRobotHistory[key].Position, tmpAng);
                            opponentRobotHistory[key].action_EstimatedPosition = tmpPos;
                            opponentRobotHistory[key].view_EstimatedPosition = tmpPos;
                            opponentRobotHistory[key].Angle = tmpAng;
                        }
                        else
                        {
                            opponentRobotHistory[key].NumNotViewed = 0;
                            opponentRobotHistory[key].Position = tmpPos;
                            opponentRobotHistory[key].cKalman.kalmanFilter(opponentRobotHistory[key].Position, tmpAng);
                            opponentRobotHistory[key].Angle = tmpAng;
                        }
                    }
                    #endregion
                    #region "Opponent ID Not Detected"
                    if (!isOpponentsID_Detected)
                    {
                        foreach (int searchKey in opponentRobotHistory.Keys)
                        {
                            dist = distance(tmpPos, opponentRobotHistory[searchKey].view_EstimatedPosition);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                minHistoryID = searchKey;
                            }
                        }
                        if (minHistoryID != -1)
                        {
                            if (minDist < Max_Opponent_distance)
                            {
                                opponentRobotHistory[minHistoryID].NumNotViewed = 0;
                                opponentRobotHistory[minHistoryID].Position = tmpPos;
                                opponentRobotHistory[minHistoryID].cKalman.kalmanFilter(opponentRobotHistory[minHistoryID].Position);
                                opponentRobotHistory[minHistoryID].Angle = tmpAng;
                            }
                            else
                            {
                                int count = -1;
                                for (int i = 0; i < opponentRobotHistory.Count; i++)
                                {
                                    if (opponentRobotHistory.ContainsKey(i) == false)
                                    {
                                        count = i;
                                    }
                                }
                                if (count == -1)
                                {
                                    count = opponentRobotHistory.Count + 1;
                                }
                                opponentRobotHistory.Add(count, new History(ObjectType.Opponent, tmpPos, count));
                                opponentRobotHistory[count].cKalman.kalmanFilter(tmpPos);
                                opponentRobotHistory[count].action_EstimatedPosition = tmpPos;
                                opponentRobotHistory[count].view_EstimatedPosition = tmpPos;
                                opponentRobotHistory[count].Angle = tmpAng;
                            }
                        }
                        else
                        {
                            opponentRobotHistory.Add(0, new History(ObjectType.Opponent, tmpPos, 0));
                            opponentRobotHistory[0].cKalman.kalmanFilter(tmpPos);
                            opponentRobotHistory[0].action_EstimatedPosition = tmpPos;
                            opponentRobotHistory[0].view_EstimatedPosition = tmpPos;
                            opponentRobotHistory[0].Angle = tmpAng;
                        }
                    }
                    #endregion
                }
            }
            // find id of Balls
            if (FillBall)
            {
                #region "Ball Section"
                foreach (var key in Balls.Keys)
                {
                    double dist, minDist = double.MaxValue;
                    int minHistoryID = -1;
                    int lastCamViewed = -1;
                    tmpPos = Vision2AI(Balls[key].pos);
                    foreach (int searchKey in ballHistory.Keys)
                    {
                        if (ballHistory[searchKey].NumNotViewed != 0)
                        {
                            dist = distance(tmpPos, ballHistory[searchKey].view_EstimatedPosition);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                minHistoryID = searchKey;
                                lastCamViewed = (int)Balls[key].camera;
                            }
                        }
                    }
                    if (minHistoryID != -1)
                    {
                        if (minDist < Max_Ball_distance)
                        {
                            ballHistory[minHistoryID].NumNotViewed = 0;
                            ballHistory[minHistoryID].lastCamViewed = lastCamViewed;
                            ballHistory[minHistoryID].Position = tmpPos;
                            ballHistory[minHistoryID].cKalman.kalmanFilter(ballHistory[minHistoryID].Position);
                        }
                        else
                        {
                            int count = -1;
                            for (int i = 0; i < ballHistory.Count; i++)
                            {
                                if (ballHistory.ContainsKey(i) == false)
                                {
                                    count = i;
                                }
                            }
                            if (count == -1)
                            {
                                count = ballHistory.Count + 1;
                            }
                            ballHistory.Add(count, new History(ObjectType.Ball, tmpPos, count));
                            ballHistory[count].cKalman.kalmanFilter(tmpPos);
                            ballHistory[count].action_EstimatedPosition = tmpPos;
                            ballHistory[count].view_EstimatedPosition = tmpPos;
                        }
                    }
                    else
                    {
                        int count = -1;
                        for (int i = 0; i < ballHistory.Count; i++)
                        {
                            if (ballHistory.ContainsKey(i) == false)
                            {
                                count = i;
                            }
                        }
                        if (count == -1)
                        {
                            count = ballHistory.Count + 1;
                        }
                        ballHistory.Add(count, new History(ObjectType.Ball, tmpPos, count));
                        ballHistory[count].cKalman.kalmanFilter(tmpPos);
                        ballHistory[count].action_EstimatedPosition = tmpPos;
                        ballHistory[count].view_EstimatedPosition = tmpPos;
                    }
                }
                #endregion
            }
            return Frame;
        }
        public frame FillHistoryNew(frame Frame, frame newFrame, Dictionary<uint, vraw> Balls, bool FillBall, bool FillRobot)
        {
            #region "History Update"    
            #region "opponent robot"
                foreach (int searchKey in opponentRobotHistory.Keys)
                {
                    opponentRobotHistory[searchKey].NumNotViewed++;
                }
                IEnumerable<KeyValuePair<int, History>> items2 = opponentRobotHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                List<int> key_op_remove = new List<int>();
                foreach (KeyValuePair<int, History> searchKey in items2)
                {
                    key_op_remove.Add(searchKey.Key);
                }
                foreach (int i in key_op_remove)
                {
                    opponentRobotHistory.Remove(i);
                    if (Frame.OppRobots.ContainsKey((uint)i))
                        Frame.OppRobots.Remove((uint)i);
                }
                #endregion
            if (FillRobot)
            {
                #region "our robot"
                foreach (int searchKey in ourRobotHistory.Keys)
                {
                    ourRobotHistory[searchKey].NumNotViewed++;
                }

                IEnumerable<KeyValuePair<int, History>> items1 = ourRobotHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                List<int> key_or_remove = new List<int>();
                foreach (KeyValuePair<int, History> searchKey in items1)
                {
                    key_or_remove.Add(searchKey.Key);
                }
                foreach (int i in key_or_remove)
                {
                    ourRobotHistory.Remove(i);
                    if (Frame.OurRobots.ContainsKey((uint)i))
                        Frame.OurRobots.Remove((uint)i);
                }

                #endregion
            }
            if (FillBall)
            {
                #region "Balls"
                foreach (int searchKey in ballHistory.Keys)
                {
                    ballHistory[searchKey].NumNotViewed++;
                }
                IEnumerable<KeyValuePair<int, History>> items3 = ballHistory.Where(item => item.Value.NumNotViewed >= MaxNotSeen);
                List<int> key_ball_remove = new List<int>();
                foreach (KeyValuePair<int, History> searchKey in items3)
                {
                    key_ball_remove.Add(searchKey.Key);
                }
                foreach (int i in key_ball_remove)
                {
                    ballHistory.Remove(i);
                }
                #endregion
            }
            #endregion

            Position2D tmpPos;
            float? tmpAng;

            // find id of opponents
            #region "Opponnet Robots Section"
            foreach (int key in newFrame.OppRobots.Keys)
            {
                double dist, minDist = double.MaxValue;
                int minHistoryID = -1;
                tmpPos = Vision2AI(newFrame.OppRobots[(uint)key].vision.pos);
                tmpAng = (float)Vision2AI(newFrame.OppRobots[(uint)key].vision.angle);
                #region "Opponent ID Detected"
                if (isOpponentsID_Detected == true)
                {
                    if (!opponentRobotHistory.ContainsKey(key))
                    {
                        opponentRobotHistory.Add(key, new History(ObjectType.OurRobot, tmpPos, key));
                        opponentRobotHistory[key].cKalman.kalmanFilter(opponentRobotHistory[key].Position, tmpAng);
                        opponentRobotHistory[key].action_EstimatedPosition = tmpPos;
                        opponentRobotHistory[key].view_EstimatedPosition = tmpPos;
                        opponentRobotHistory[key].Angle = tmpAng;
                    }
                    else
                    {
                        opponentRobotHistory[key].NumNotViewed = 0;
                        opponentRobotHistory[key].Position = tmpPos;
                        opponentRobotHistory[key].cKalman.kalmanFilter(opponentRobotHistory[key].Position, tmpAng);
                        opponentRobotHistory[key].Angle = tmpAng;
                    }
                }
                #endregion
                #region "Opponent ID Not Detected"
                if (!isOpponentsID_Detected)
                {
                    foreach (int searchKey in opponentRobotHistory.Keys)
                    {
                        dist = distance(tmpPos, opponentRobotHistory[searchKey].view_EstimatedPosition);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minHistoryID = searchKey;
                        }
                    }
                    if (minHistoryID != -1)
                    {
                        if (minDist < Max_Opponent_distance)
                        {
                            opponentRobotHistory[minHistoryID].NumNotViewed = 0;
                            opponentRobotHistory[minHistoryID].Position = tmpPos;
                            opponentRobotHistory[minHistoryID].cKalman.kalmanFilter(opponentRobotHistory[minHistoryID].Position);
                            opponentRobotHistory[minHistoryID].Angle = tmpAng;
                        }
                        else
                        {
                            int count = -1;
                            for (int i = 0; i < opponentRobotHistory.Count; i++)
                            {
                                if (opponentRobotHistory.ContainsKey(i) == false)
                                {
                                    count = i;
                                }
                            }
                            if (count == -1)
                            {
                                count = opponentRobotHistory.Count + 1;
                            }
                            opponentRobotHistory.Add(count, new History(ObjectType.Opponent, tmpPos, count));
                            opponentRobotHistory[count].cKalman.kalmanFilter(tmpPos);
                            opponentRobotHistory[count].action_EstimatedPosition = tmpPos;
                            opponentRobotHistory[count].view_EstimatedPosition = tmpPos;
                            opponentRobotHistory[count].Angle = tmpAng;
                        }
                    }
                    else
                    {
                        opponentRobotHistory.Add(0, new History(ObjectType.Opponent, tmpPos, 0));
                        opponentRobotHistory[0].cKalman.kalmanFilter(tmpPos);
                        opponentRobotHistory[0].action_EstimatedPosition = tmpPos;
                        opponentRobotHistory[0].view_EstimatedPosition = tmpPos;
                        opponentRobotHistory[0].Angle = tmpAng;
                    }
                }
                #endregion
            }
            #endregion
       
            if (FillRobot)
            {
                #region "Our Robots Section"
                foreach (int key in newFrame.OurRobots.Keys)
                {
                    tmpPos = Vision2AI(newFrame.OurRobots[(uint)key].vision.pos);
                    tmpAng = (float)Vision2AI(newFrame.OurRobots[(uint)key].vision.angle);
                    if (!ourRobotHistory.ContainsKey(key))
                    {
                        ourRobotHistory.Add(key, new History(ObjectType.OurRobot, tmpPos, key));
                        ourRobotHistory[key].cKalman.kalmanFilter(ourRobotHistory[key].Position, tmpAng);
                        ourRobotHistory[key].action_EstimatedPosition = tmpPos;
                        ourRobotHistory[key].view_EstimatedPosition = tmpPos;
                        ourRobotHistory[key].Angle = tmpAng;
                    }
                    else
                    {
                        ourRobotHistory[key].NumNotViewed = 0;
                        ourRobotHistory[key].Position = tmpPos;
                        ourRobotHistory[key].cKalman.kalmanFilter(ourRobotHistory[key].Position, tmpAng);
                        ourRobotHistory[key].Angle = tmpAng;
                    }
                }
                #endregion
            }
            // find id of Balls
            if (FillBall)
            {
                #region "Ball Section"
                foreach (var key in Balls.Keys)
                {
                    double dist, minDist = double.MaxValue;
                    int minHistoryID = -1;
                    int lastCamViewed = -1;
                    tmpPos = Vision2AI(Balls[key].pos);
                    foreach (int searchKey in ballHistory.Keys)
                    {
                        if (ballHistory[searchKey].NumNotViewed != 0)
                        {
                            dist = distance(tmpPos, ballHistory[searchKey].view_EstimatedPosition);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                minHistoryID = searchKey;
                                lastCamViewed = (int)Balls[key].camera;
                            }
                        }
                    }
                    if (minHistoryID != -1)
                    {
                        if (minDist < Max_Ball_distance)
                        {
                            ballHistory[minHistoryID].NumNotViewed = 0;
                            ballHistory[minHistoryID].lastCamViewed = lastCamViewed;
                            ballHistory[minHistoryID].Position = tmpPos;
                            ballHistory[minHistoryID].cKalman.kalmanFilter(ballHistory[minHistoryID].Position);
                        }
                        else
                        {
                            int count = -1;
                            for (int i = 0; i < ballHistory.Count; i++)
                            {
                                if (ballHistory.ContainsKey(i) == false)
                                {
                                    count = i;
                                }
                            }
                            if (count == -1)
                            {
                                count = ballHistory.Count + 1;
                            }
                            ballHistory.Add(count, new History(ObjectType.Ball, tmpPos, count));
                            ballHistory[count].cKalman.kalmanFilter(tmpPos);
                            ballHistory[count].action_EstimatedPosition = tmpPos;
                            ballHistory[count].view_EstimatedPosition = tmpPos;
                        }
                    }
                    else
                    {
                        int count = -1;
                        for (int i = 0; i < ballHistory.Count; i++)
                        {
                            if (ballHistory.ContainsKey(i) == false)
                            {
                                count = i;
                            }
                        }
                        if (count == -1)
                        {
                            count = ballHistory.Count + 1;
                        }
                        ballHistory.Add(count, new History(ObjectType.Ball, tmpPos, count));
                        ballHistory[count].cKalman.kalmanFilter(tmpPos);
                        ballHistory[count].action_EstimatedPosition = tmpPos;
                        ballHistory[count].view_EstimatedPosition = tmpPos;
                    }
                }
                #endregion
            }
            return Frame;
        }
        public void CreateEstimation(double totalDelay, double viewDelay, GameStatus status)
        {
            Dictionary<int, int> Related = new Dictionary<int, int>();
            foreach (int key1 in ballHistory.Keys)
            {
                foreach (int key2 in ballHistory.Keys)
                {
                    if (key1 != key2)
                    {
                        if (ballHistory[key1].Position.DistanceFrom(ballHistory[key2].Position) < 0.05)
                        {
                            if (ballHistory[key1].NumNotViewed < ballHistory[key2].NumNotViewed)
                            {
                                Related.Add(key1, key2);
                                break;
                            }
                        }
                    }
                }
            }

            foreach (int key in Related.Keys)
            {
                if (ballHistory.ContainsKey(Related[key]))
                    ballHistory.Remove(Related[key]);
            }

            foreach (int key in ballHistory.Keys)
            {
                ballHistory[key].cKalman.SetStatus(status);

                ballHistory[key].action_EstimatedAcceleration = ballHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                ballHistory[key].view_EstimatedAcceleration = ballHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                ballHistory[key].action_EstimatedVelocity = ballHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                ballHistory[key].view_EstimatedVelocity = ballHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                ballHistory[key].action_EstimatedPosition = ballHistory[key].cKalman.getEstimatedPosition(totalDelay);
                ballHistory[key].view_EstimatedPosition = ballHistory[key].cKalman.getEstimatedPosition(viewDelay);
                try
                {
                    ballHistory[key].Path.Enqueue(new State(ballHistory[key].view_EstimatedPosition, DateTime.Now.Millisecond / 1000.0));
                }

                catch (Exception ex)
                {
                    Logger.Write(LogType.Exception, ex.ToString());

                    State tmp = ballHistory[key].Path.Dequeue();
                    ballHistory[key].Path.Enqueue(new State(ballHistory[key].view_EstimatedPosition, DateTime.Now.Millisecond / 1000.0));
                }
            }
            foreach (int key in ourRobotHistory.Keys)
            {
                ourRobotHistory[key].action_EstimatedAcceleration = ourRobotHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                ourRobotHistory[key].view_EstimatedAcceleration = ourRobotHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                ourRobotHistory[key].action_EstimatedVelocity = ourRobotHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                ourRobotHistory[key].view_EstimatedVelocity = ourRobotHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                ourRobotHistory[key].action_EstimatedPosition = ourRobotHistory[key].cKalman.getEstimatedPosition(totalDelay);
                ourRobotHistory[key].view_EstimatedPosition = ourRobotHistory[key].cKalman.getEstimatedPosition(viewDelay);

                ourRobotHistory[key].action_EstimatedAngle = ourRobotHistory[key].cKalman.getEstimatedAngle(totalDelay);
                ourRobotHistory[key].view_EstimatedAngle = ourRobotHistory[key].cKalman.getEstimatedAngle(viewDelay);
                
            }
            foreach (int key in opponentRobotHistory.Keys)
            {
                opponentRobotHistory[key].action_EstimatedAcceleration = opponentRobotHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                opponentRobotHistory[key].view_EstimatedAcceleration = opponentRobotHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                opponentRobotHistory[key].action_EstimatedVelocity = opponentRobotHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                opponentRobotHistory[key].view_EstimatedVelocity = opponentRobotHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                opponentRobotHistory[key].action_EstimatedPosition = opponentRobotHistory[key].cKalman.getEstimatedPosition(totalDelay);
                opponentRobotHistory[key].view_EstimatedPosition = opponentRobotHistory[key].cKalman.getEstimatedPosition(viewDelay);

                opponentRobotHistory[key].action_EstimatedAngle = opponentRobotHistory[key].cKalman.getEstimatedAngle(totalDelay);
                opponentRobotHistory[key].view_EstimatedAngle = opponentRobotHistory[key].cKalman.getEstimatedAngle(viewDelay);
            }
        }
        public void CreateEstimation(double totalDelay, double viewDelay, GameStatus status, bool EstimateBall, bool EstimateRobot)
        {
            Dictionary<int, int> Related = new Dictionary<int, int>();
            if (EstimateBall)
            {
                foreach (int key1 in ballHistory.Keys)
                {
                    foreach (int key2 in ballHistory.Keys)
                    {
                        if (key1 != key2)
                        {
                            if (ballHistory[key1].Position.DistanceFrom(ballHistory[key2].Position) < 0.05)
                            {
                                if (ballHistory[key1].NumNotViewed < ballHistory[key2].NumNotViewed)
                                {
                                    Related.Add(key1, key2);
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (int key in Related.Keys)
                {
                    if (ballHistory.ContainsKey(Related[key]))
                        ballHistory.Remove(Related[key]);
                }

                foreach (int key in ballHistory.Keys)
                {
                    ballHistory[key].cKalman.SetStatus(status);

                    ballHistory[key].action_EstimatedAcceleration = ballHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                    ballHistory[key].view_EstimatedAcceleration = ballHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                    ballHistory[key].action_EstimatedVelocity = ballHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                    ballHistory[key].view_EstimatedVelocity = ballHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                    ballHistory[key].action_EstimatedPosition = ballHistory[key].cKalman.getEstimatedPosition(totalDelay);
                    ballHistory[key].view_EstimatedPosition = ballHistory[key].cKalman.getEstimatedPosition(viewDelay);
                    try
                    {
                        ballHistory[key].Path.Enqueue(new State(ballHistory[key].view_EstimatedPosition, DateTime.Now.Millisecond / 1000.0));
                    }

                    catch (Exception ex)
                    {
                        Logger.Write(LogType.Exception, ex.ToString());

                        State tmp = ballHistory[key].Path.Dequeue();
                        ballHistory[key].Path.Enqueue(new State(ballHistory[key].view_EstimatedPosition, DateTime.Now.Millisecond / 1000.0));
                    }
                }
            }
            if (EstimateRobot)
            {
                foreach (int key in ourRobotHistory.Keys)
                {
                    ourRobotHistory[key].action_EstimatedAcceleration = ourRobotHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                    ourRobotHistory[key].view_EstimatedAcceleration = ourRobotHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                    ourRobotHistory[key].action_EstimatedVelocity = ourRobotHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                    ourRobotHistory[key].view_EstimatedVelocity = ourRobotHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                    ourRobotHistory[key].action_EstimatedPosition = ourRobotHistory[key].cKalman.getEstimatedPosition(totalDelay);
                    ourRobotHistory[key].view_EstimatedPosition = ourRobotHistory[key].cKalman.getEstimatedPosition(viewDelay);

                    ourRobotHistory[key].action_EstimatedAngle = ourRobotHistory[key].cKalman.getEstimatedAngle(totalDelay);
                    ourRobotHistory[key].view_EstimatedAngle = ourRobotHistory[key].cKalman.getEstimatedAngle(viewDelay);

                }
                foreach (int key in opponentRobotHistory.Keys)
                {
                    opponentRobotHistory[key].action_EstimatedAcceleration = opponentRobotHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                    opponentRobotHistory[key].view_EstimatedAcceleration = opponentRobotHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                    opponentRobotHistory[key].action_EstimatedVelocity = opponentRobotHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                    opponentRobotHistory[key].view_EstimatedVelocity = opponentRobotHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                    opponentRobotHistory[key].action_EstimatedPosition = opponentRobotHistory[key].cKalman.getEstimatedPosition(totalDelay);
                    opponentRobotHistory[key].view_EstimatedPosition = opponentRobotHistory[key].cKalman.getEstimatedPosition(viewDelay);

                    opponentRobotHistory[key].action_EstimatedAngle = opponentRobotHistory[key].cKalman.getEstimatedAngle(totalDelay);
                    opponentRobotHistory[key].view_EstimatedAngle = opponentRobotHistory[key].cKalman.getEstimatedAngle(viewDelay);
                }
            }
        }
        public void CreateEstimationNew(double totalDelay, double viewDelay, GameStatus status, bool EstimateBall, bool EstimateRobot)
        {
            Dictionary<int, int> Related = new Dictionary<int, int>();
            foreach (int key in opponentRobotHistory.Keys)
            {
                opponentRobotHistory[key].action_EstimatedAcceleration = opponentRobotHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                opponentRobotHistory[key].view_EstimatedAcceleration = opponentRobotHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                opponentRobotHistory[key].action_EstimatedVelocity = opponentRobotHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                opponentRobotHistory[key].view_EstimatedVelocity = opponentRobotHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                opponentRobotHistory[key].action_EstimatedPosition = opponentRobotHistory[key].cKalman.getEstimatedPosition(totalDelay);
                opponentRobotHistory[key].view_EstimatedPosition = opponentRobotHistory[key].cKalman.getEstimatedPosition(viewDelay);

                opponentRobotHistory[key].action_EstimatedAngle = opponentRobotHistory[key].cKalman.getEstimatedAngle(totalDelay);
                opponentRobotHistory[key].view_EstimatedAngle = opponentRobotHistory[key].cKalman.getEstimatedAngle(viewDelay);
            }
            if (EstimateBall)
            {
                foreach (int key1 in ballHistory.Keys)
                {
                    foreach (int key2 in ballHistory.Keys)
                    {
                        if (key1 != key2)
                        {
                            if (ballHistory[key1].Position.DistanceFrom(ballHistory[key2].Position) < 0.05)
                            {
                                if (ballHistory[key1].NumNotViewed < ballHistory[key2].NumNotViewed)
                                {
                                    Related.Add(key1, key2);
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (int key in Related.Keys)
                {
                    if (ballHistory.ContainsKey(Related[key]))
                        ballHistory.Remove(Related[key]);
                }

                foreach (int key in ballHistory.Keys)
                {
                    ballHistory[key].cKalman.SetStatus(status);

                    ballHistory[key].action_EstimatedAcceleration = ballHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                    ballHistory[key].view_EstimatedAcceleration = ballHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                    ballHistory[key].action_EstimatedVelocity = ballHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                    ballHistory[key].view_EstimatedVelocity = ballHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                    ballHistory[key].action_EstimatedPosition = ballHistory[key].cKalman.getEstimatedPosition(totalDelay);
                    ballHistory[key].view_EstimatedPosition = ballHistory[key].cKalman.getEstimatedPosition(viewDelay);
                    try
                    {
                        ballHistory[key].Path.Enqueue(new State(ballHistory[key].view_EstimatedPosition, DateTime.Now.Millisecond / 1000.0));
                    }

                    catch (Exception ex)
                    {
                        Logger.Write(LogType.Exception, ex.ToString());

                        State tmp = ballHistory[key].Path.Dequeue();
                        ballHistory[key].Path.Enqueue(new State(ballHistory[key].view_EstimatedPosition, DateTime.Now.Millisecond / 1000.0));
                    }
                }
            }
            if (EstimateRobot)
            {
                foreach (int key in ourRobotHistory.Keys)
                {
                    ourRobotHistory[key].action_EstimatedAcceleration = ourRobotHistory[key].cKalman.getEstimatedAcceleration(totalDelay);
                    ourRobotHistory[key].view_EstimatedAcceleration = ourRobotHistory[key].cKalman.getEstimatedAcceleration(viewDelay);

                    ourRobotHistory[key].action_EstimatedVelocity = ourRobotHistory[key].cKalman.getEstimatedVelocity(totalDelay);
                    ourRobotHistory[key].view_EstimatedVelocity = ourRobotHistory[key].cKalman.getEstimatedVelocity(viewDelay);

                    ourRobotHistory[key].action_EstimatedPosition = ourRobotHistory[key].cKalman.getEstimatedPosition(totalDelay);
                    ourRobotHistory[key].view_EstimatedPosition = ourRobotHistory[key].cKalman.getEstimatedPosition(viewDelay);

                    ourRobotHistory[key].action_EstimatedAngle = ourRobotHistory[key].cKalman.getEstimatedAngle(totalDelay);
                    ourRobotHistory[key].view_EstimatedAngle = ourRobotHistory[key].cKalman.getEstimatedAngle(viewDelay);

                }
             
            }
        }
        public void CreateWorldModel()
        {
            model = new WorldModel();
            model.BallState = new SingleObjectState();

            if (model.OurRobots == null)
                model.OurRobots = new Dictionary<int, SingleObjectState>();
            if (model.Opponents == null)
                model.Opponents = new Dictionary<int, SingleObjectState>();
            foreach (int key in ourRobotHistory.Keys)
            {
                if (ourRobotHistory[key].NumNotViewed < Max_to_imagine + 15)
                {
                    model.OurRobots.Add(key, new SingleObjectState(ObjectType.OurRobot, ourRobotHistory[key].action_EstimatedPosition, ourRobotHistory[key].action_EstimatedVelocity, ourRobotHistory[key].action_EstimatedAcceleration, (float)ourRobotHistory[key].action_EstimatedAngle, 0));
                }
            }
            foreach (int key in opponentRobotHistory.Keys)
            {
                if (opponentRobotHistory[key].NumNotViewed < Max_to_imagine + 15)
                {
                    model.Opponents.Add(key, new SingleObjectState(ObjectType.Opponent, opponentRobotHistory[key].action_EstimatedPosition, opponentRobotHistory[key].action_EstimatedVelocity, opponentRobotHistory[key].action_EstimatedAcceleration, (float)opponentRobotHistory[key].action_EstimatedAngle, 0));
                }
            }
        }
        public void CreateWorldModelNew(bool our)
        {
            model = new WorldModel();
            model.BallState = new SingleObjectState();

            if (model.OurRobots == null)
                model.OurRobots = new Dictionary<int, SingleObjectState>();
            if (model.Opponents == null)
                model.Opponents = new Dictionary<int, SingleObjectState>();
            if (our)
            {
                foreach (int key in ourRobotHistory.Keys)
                {
                    if (ourRobotHistory[key].NumNotViewed < Max_to_imagine + 15)
                    {
                        model.OurRobots.Add(key, new SingleObjectState(ObjectType.OurRobot, ourRobotHistory[key].action_EstimatedPosition, ourRobotHistory[key].action_EstimatedVelocity, ourRobotHistory[key].action_EstimatedAcceleration, (float)ourRobotHistory[key].action_EstimatedAngle, 0));
                    }
                }
            }
            foreach (int key in opponentRobotHistory.Keys)
            {
                if (opponentRobotHistory[key].NumNotViewed < Max_to_imagine + 15)
                {
                    model.Opponents.Add(key, new SingleObjectState(ObjectType.Opponent, opponentRobotHistory[key].action_EstimatedPosition, opponentRobotHistory[key].action_EstimatedVelocity, opponentRobotHistory[key].action_EstimatedAcceleration, (float)opponentRobotHistory[key].action_EstimatedAngle, 0));
                }
            }
        }
        public bool SelectedBallIndexViewed(int BallIndex)
        {
            bool ret = false;
            //foreach (int key in ballHistory.Keys)
            //{
            //    if (key == BallIndex && ballHistory[key].NumNotViewed < Max_to_imagine)
            //        ret = true;
            //}
            if (ballHistory.ContainsKey(BallIndex))
                if (ballHistory[BallIndex].NumNotViewed < Max_to_imagine)
                    ret = true;

            if (ret == true)
            {
                if (ballHistory[BallIndex].Position.X > GameParameters.OppGoalLeft.X - RobotParameters.OurRobotParams.Diameter / 2.0 && ballHistory[BallIndex].Position.X < GameParameters.OurGoalLeft.X + RobotParameters.OurRobotParams.Diameter / 2.0 && ballHistory[BallIndex].Position.Y > GameParameters.OurRightCorner.Y - RobotParameters.OurRobotParams.Diameter / 2.0 && ballHistory[BallIndex].Position.Y < GameParameters.OurLeftCorner.Y + RobotParameters.OurRobotParams.Diameter / 2.0)
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
        public Position2D ReturnSelectedBallIndexPosition(int BallIndex)
        {
            return ballHistory[BallIndex].action_EstimatedPosition;
        }
        private double distance(Position2D Src, Position2D Dst)
        {
            double dx = (Src.X - Dst.X);
            double dy = (Src.Y - Dst.Y);
            return Math.Sqrt(dx * dx +  dy * dy);
        }

        internal Vector2D ReturnSelectedBallIndexSpeed(int BallIndex)
        {
            return ballHistory[BallIndex].action_EstimatedVelocity;
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
