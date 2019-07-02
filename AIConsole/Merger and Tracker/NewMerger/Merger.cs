using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using messages_robocup_ssl_wrapper;
using messages_robocup_ssl_detection;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;
using messages_robocup_ssl_geometry;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public class NewMerger
    {
        List<MathMatrix> coef = new List<MathMatrix>();
        List<MathMatrix> pos = new List<MathMatrix>();
        List<MathMatrix> cam = new List<MathMatrix>();
        public Dictionary<uint, vraw> Balls = new Dictionary<uint, vraw>();
        Dictionary<uint, SSL_WrapperPacket> sslPackets = new Dictionary<uint, SSL_WrapperPacket>();
        public SSL_WrapperPacket sslpacketCam0 = new SSL_WrapperPacket();
        public SSL_WrapperPacket sslpacketCam1 = new SSL_WrapperPacket();
        public SSL_WrapperPacket sslpacketCam2 = new SSL_WrapperPacket();
        public SSL_WrapperPacket sslpacketCam3 = new SSL_WrapperPacket();
        public SSL_WrapperPacket sslpacketCam4 = new SSL_WrapperPacket();
        public SSL_WrapperPacket sslpacketCam5 = new SSL_WrapperPacket();
        public SSL_WrapperPacket sslpacketCam6 = new SSL_WrapperPacket();
        public SSL_WrapperPacket sslpacketCam7 = new SSL_WrapperPacket();
        public SSL_GeometryData sslVisionGeometry = null;
        private bool oneCamera = false;
        private uint cameraID = 0;
        public uint CameraID
        {
            get { return cameraID; }
            set { cameraID = value; }
        }
        public bool OneCamera
        {
            get { return oneCamera; }
            set { oneCamera = value; }
        }
        private const double disEps = 280; //TODO: CHANGE TO 100 
        private bool lastColorIsYellow = false;
        private float minConfidence = 0.6f;
        private mRobot MergeRobot(List<mRobot> robots)
        {
            return new mRobot();
        }
        bool[] cameras_seen = new bool[StaticVariables.CameraCount];
        int num_cameras, num_cameras_seen;
      


        uint frames;

        bool ready;
        public NewMerger()
        {
            num_cameras = 0;
            num_cameras_seen = 0;
        
            ready = false;
            frames = 0;
            for (int i = 0; i < cameras_seen.Length; i++)
            {
                cameras_seen[i] = false;
            }
        }
        //public bool Merge(SSL_WrapperPacket Packet, ref frame Frame, ref frame newFrame, bool isYellow)
        //{
        //    //DrawingObjects.AddObject(new Circle(Vision2AI(new Position2D(-1994, -487), false), StaticVariables.BALL_RADIUS, new Pen(Color.Red, 0.01f)), "badssdddscdsll");
        //    //DrawingObjects.AddObject(new Circle(Vision2AI(new Position2D(-1984, -427), false), StaticVariables.BALL_RADIUS, new Pen(Color.Red, 0.01f)), "badscsdll");
        //    //DrawingObjects.AddObject(new Circle(Vision2AI(new Position2D(-1918, -475), false), StaticVariables.BALL_RADIUS, new Pen(Color.PeachPuff, 0.01f)), "badcdsdscsdll");
        //    newFrame = new frame();
        //    Dictionary<int, mRobot> ourRobots = new Dictionary<int, mRobot>();
        //    Dictionary<int, mRobot> oppRobots = new Dictionary<int, mRobot>();
        //    Dictionary<int, mBall> balls = new Dictionary<int, mBall>();

        //    if (Packet == null || Packet.detection == null)
        //        return false;

        //    if (Packet.detection.camera_id == 0)
        //    {
        //        sslpacketCam0 = Packet;
        //    }
        //    if (Packet.detection.camera_id == 1)
        //    {
        //        sslpacketCam1 = Packet;
        //    }
        //    if (Packet.detection.camera_id == 2)
        //    {
        //        sslpacketCam2 = Packet;
        //    }
        //    if (Packet.detection.camera_id == 3)
        //    {
        //        sslpacketCam3 = Packet;
        //    }

        //    if (MergerParameters.AvailableCamIds.Contains((int)Packet.detection.camera_id))
        //    {
        //        if (sslPackets.ContainsKey(Packet.detection.camera_id))
        //        {
        //            sslPackets[Packet.detection.camera_id] = Packet;
        //        }
        //        else
        //            sslPackets.Add(Packet.detection.camera_id, Packet);
        //    }
        //    if (sslPackets.Count < MergerParameters.AvailableCamIds.Count)
        //        return false;

        //    if (lastColorIsYellow != isYellow)
        //        Frame = new frame();
        //    Frame.timeofcapture = 0;
        //    #region capture Time
        //    for (int i = 0; i < sslPackets.Count; i++)
        //    {
        //        Frame.timeofcapture += sslPackets.ElementAt(i).Value.detection.t_capture;
        //    }
        //    if (sslPackets.Count > 0)
        //        Frame.timeofcapture /= (double)sslPackets.Count;
        //    #endregion

        //    int counterBall = 0;
        //    int counterOur = 0;
        //    int counterOpp = 0;

        //    Balls = new Dictionary<uint, vraw>();
        //    foreach (var pack in sslPackets)
        //    {
        //        List<SSL_DetectionRobot> ourRobotsPacket, oppRobotsPacket;
        //        if (isYellow)
        //        {
        //            ourRobotsPacket = pack.Value.detection.robots_yellow;
        //            oppRobotsPacket = pack.Value.detection.robots_blue;
        //        }
        //        else
        //        {
        //            ourRobotsPacket = pack.Value.detection.robots_blue;
        //            oppRobotsPacket = pack.Value.detection.robots_yellow;
        //        }
        //        foreach (var our in ourRobotsPacket)
        //        {
        //            ourRobots.Add(counterOur, new mRobot(our.confidence, our.robot_id, pack.Value.detection.t_capture, our.orientation, our.x, our.y, pack.Value.detection.camera_id));
        //            counterOur++;
        //        }
        //        foreach (var opp in oppRobotsPacket)
        //        {
        //            oppRobots.Add(counterOpp, new mRobot(opp.confidence, opp.robot_id, pack.Value.detection.t_capture, opp.orientation, opp.x, opp.y, pack.Value.detection.camera_id));
        //            counterOpp++;
        //        }
        //        //List<SSL_DetectionBall> balls = pack.Value.detection.balls;
        //        for (int j = 0; j < pack.Value.detection.balls.Count && j < StaticVariables.MaxBalls; j++)
        //        {
        //            var b = pack.Value.detection.balls[j];
        //            if (b.confidence > minConfidence)
        //                balls.Add(balls.Count, new mBall(b.confidence, pack.Value.detection.t_capture, b.x, b.y, (int)pack.Value.detection.camera_id));
        //        }
        //    }
        //    uint count = 0;
        //    #region Ball
        //    if (balls.Count > 0)
        //    {
        //        List<mBall> finalBall = new List<mBall>();
        //        List<List<mBall>> selectedBallList = new List<List<mBall>>();
        //        List<int> selectedBallIdList = new List<int>();
        //        foreach (var ball in balls)
        //        {
        //            if (!selectedBallIdList.Contains(ball.Key))
        //            {
        //                List<mBall> ballList = new List<mBall>();
        //                ballList.Add(ball.Value);
        //                selectedBallIdList.Add(ball.Key);
        //                foreach (var ball2 in balls)
        //                {
        //                    if (!selectedBallIdList.Contains(ball2.Key) && ball.Value.CamID != ball2.Value.CamID && ball.Value.Pos.DistanceFrom(ball2.Value.Pos) < disEps)
        //                    {
        //                        ballList.Add(ball2.Value);
        //                        selectedBallIdList.Add(ball2.Key);
        //                    }
        //                }
        //                selectedBallList.Add(ballList);
        //            }
        //        }

        //        foreach (var item in selectedBallList)
        //        {
        //            if (item.Count > 1)
        //            {

        //            }
        //            DrawingObjects.AddObject(new StringDraw(item.Count.ToString(), new Position2D(-1.2, 0)), "dsadsa");
        //            MathMatrix coefMat;
        //            MathMatrix commonMat;
        //            MathMatrix ballPos;
        //            if (item.Count == 3)
        //            {
        //                List<mBall> temp = item.OrderByDescending(t => t.Confidence).ToList();
        //                temp.RemoveAt(2);
        //                commonMat = MatrixMaker(temp.OrderByDescending(t => t.CamID).ToList(), out coefMat);
        //                if (coefMat == null)
        //                    continue;
        //                ballPos = coefMat * commonMat.Transpose;
        //                double confidence = 0;
        //                double time = 0;
        //                foreach (var dic in temp)
        //                {
        //                    confidence += dic.Confidence;
        //                    time += dic.Time;
        //                }
        //                confidence /= (double)temp.Count;
        //                time /= (double)temp.Count;

        //                int key = 0;
        //                List<int> ids = new List<int>();
        //                foreach (var id in temp)
        //                    ids.Add(id.CamID);
        //                foreach (var id in ids)
        //                {
        //                    key *= 10;
        //                    key += id;
        //                }
        //                finalBall.Add(new mBall(confidence, time, ballPos[0, 0], ballPos[1, 0], ids[0])); // final merged balls

        //            }
        //            else
        //            {
        //                commonMat = MatrixMaker(item.OrderByDescending(t => t.CamID).ToList(), out coefMat);
        //                if (coefMat == null)
        //                    continue;
        //                ballPos = coefMat * commonMat.Transpose;
        //                double confidence = 0;
        //                double time = 0;
        //                foreach (var dic in item)
        //                {
        //                    confidence += dic.Confidence;
        //                    time += dic.Time;
        //                }
        //                confidence /= (double)item.Count;
        //                time /= (double)item.Count;

        //                int key = 0;
        //                List<int> ids = new List<int>();
        //                foreach (var id in item)
        //                    ids.Add(id.CamID);
        //                foreach (var id in ids)
        //                {
        //                    key *= 10;
        //                    key += id;
        //                }
        //                finalBall.Add(new mBall(confidence, time, ballPos[0, 0], ballPos[1, 0], ids[0])); // final merged balls
        //            }

        //        }
        //        foreach (var item in finalBall)
        //        {
        //            Balls.Add(count, new vraw(item.Time, item.Pos, 0, (float)item.Confidence, (uint)item.CamID));
        //            StaticVariables.BallPositions = new List<Position2D>();
        //            StaticVariables.BallPositions.Add(item.Pos);
        //            count++;
        //        }
        //        foreach (var item in balls)
        //        {
        //            DrawingObjects.AddObject(new Circle(Vision2AI(item.Value.Pos, false), StaticVariables.BALL_RADIUS, new Pen(Color.GreenYellow, 0.01f)), "ball" + item.Value.Pos.toString());
        //            //Balls.Add(count, new vraw(item.Value.Time, item.Value.Pos, 0, (float)item.Value.Confidence, (uint)item.Value.CamID));
        //            StaticVariables.BallPositions = new List<Position2D>();
        //            StaticVariables.BallPositions.Add(item.Value.Pos);
        //            count++;
        //        }
        //    }

        //    #endregion
        //    #region our
        //    if (ourRobots.Count > 0)
        //    {
        //        List<mRobot> finalRobot = new List<mRobot>();
        //        List<List<mRobot>> selectedOurRobotList = new List<List<mRobot>>();
        //        List<int> selectedOurRobotIdList = new List<int>();
        //        foreach (var robot in ourRobots)
        //        {
        //            if (!selectedOurRobotIdList.Contains(robot.Key))
        //            {
        //                List<mRobot> ourList = new List<mRobot>();
        //                ourList.Add(robot.Value);
        //                selectedOurRobotIdList.Add(robot.Key);
        //                foreach (var robot2 in ourRobots)
        //                {
        //                    if (!selectedOurRobotIdList.Contains(robot2.Key) && robot.Value.CamID != robot2.Value.CamID && robot.Value.RobotID == robot2.Value.RobotID)
        //                    {
        //                        ourList.Add(robot2.Value);
        //                        selectedOurRobotIdList.Add(robot2.Key);
        //                    }
        //                }
        //                selectedOurRobotList.Add(ourList);
        //            }
        //        }

        //        foreach (var item in selectedOurRobotList)
        //        {
        //            if (item.Count == 3 || item.Count == 4)
        //            {
        //                #region Old
        //                List<mRobot> temp = item.OrderByDescending(t => t.Confidence).ToList();
        //                temp.RemoveAt(2);
        //                MathMatrix coefMat;
        //                MathMatrix commonMat = MatrixMaker(temp.OrderByDescending(t => t.CamID).ToList(), out coefMat);
        //                if (coefMat == null)
        //                    continue;
        //                MathMatrix robotPos = coefMat * commonMat.Transpose;
        //                double confidence = 0;
        //                double time = 0;
        //                foreach (var dic in temp)
        //                {
        //                    confidence += dic.Confidence;
        //                    time += dic.Time;
        //                }
        //                confidence /= (double)temp.Count;
        //                time /= (double)temp.Count;

        //                int key = 0;
        //                List<int> ids = new List<int>();
        //                foreach (var id in temp)
        //                    ids.Add(id.CamID);
        //                foreach (var id in ids)
        //                {
        //                    key *= 10;
        //                    key += id;
        //                }
        //                finalRobot.Add(new mRobot(confidence, temp[0].RobotID, time, temp[0].Oriention, robotPos[0, 0], robotPos[1, 0], ids[0])); // final merged Our Robots
        //                #endregion
        //                #region New
        //                //double confidence = 0;
        //                //double time = 0;
        //                //double x = 0, y = 0;
        //                //foreach (var dic in item)
        //                //{
        //                //    confidence += dic.Confidence;
        //                //    time += dic.Time;
        //                //    x += dic.Pos.X;
        //                //    y += dic.Pos.Y;
        //                //}
        //                //confidence /= (double)item.Count;
        //                //time /= (double)item.Count;
        //                //x /= (double)item.Count;
        //                //y /= (double)item.Count;
        //                //int key = 0;
        //                //List<int> ids = new List<int>();
        //                //foreach (var id in item)
        //                //    ids.Add(id.CamID);
        //                //foreach (var id in ids)
        //                //{
        //                //    key *= 10;
        //                //    key += id;
        //                //}
        //                //finalRobot.Add(new mRobot(confidence, item[0].RobotID, time, item[0].Oriention, x, y, ids[0])); // final merged Our Robots
        //                #endregion
        //            }
        //            else
        //            {
        //                MathMatrix coefMat;
        //                MathMatrix commonMat = MatrixMaker(item.OrderByDescending(t => t.CamID).ToList(), out coefMat);
        //                if (coefMat == null)
        //                    continue;
        //                MathMatrix robotPos = coefMat * commonMat.Transpose;
        //                //if (item[0].RobotID == 0)
        //                //{
        //                //    cam.Add(commonMat);
        //                //    pos.Add(robotPos);
        //                //    coef.Add(coefMat);
        //                //}
        //                double confidence = 0;
        //                double time = 0;
        //                foreach (var dic in item)
        //                {
        //                    confidence += dic.Confidence;
        //                    time += dic.Time;
        //                }
        //                confidence /= (double)item.Count;
        //                time /= (double)item.Count;

        //                int key = 0;
        //                List<int> ids = new List<int>();
        //                foreach (var id in item)
        //                    ids.Add(id.CamID);
        //                foreach (var id in ids)
        //                {
        //                    key *= 10;
        //                    key += id;
        //                }
        //                finalRobot.Add(new mRobot(confidence, item[0].RobotID, time, item[0].Oriention, robotPos[0, 0], robotPos[1, 0], ids[0])); // final merged Our Robots
        //            }
        //        }
        //        foreach (var item in finalRobot)
        //        {
        //            Frame.OurRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
        //            newFrame.OurRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
        //        }
        //    }
        //    #endregion
        //    #region opp
        //    if (oppRobots.Count > 0)
        //    {
        //        List<mRobot> finalRobot = new List<mRobot>();
        //        List<List<mRobot>> selectedOppRobotList = new List<List<mRobot>>();
        //        List<int> selectedOppRobotIdList = new List<int>();
        //        foreach (var robot in oppRobots)
        //        {
        //            if (!selectedOppRobotIdList.Contains(robot.Key))
        //            {
        //                List<mRobot> ourList = new List<mRobot>();
        //                ourList.Add(robot.Value);
        //                selectedOppRobotIdList.Add(robot.Key);
        //                foreach (var robot2 in oppRobots)
        //                {
        //                    if (!selectedOppRobotIdList.Contains(robot2.Key) && robot.Value.CamID != robot2.Value.CamID && robot.Value.RobotID == robot2.Value.RobotID)
        //                    {
        //                        ourList.Add(robot2.Value);
        //                        selectedOppRobotIdList.Add(robot2.Key);
        //                    }
        //                }
        //                selectedOppRobotList.Add(ourList);
        //            }
        //        }

        //        foreach (var item in selectedOppRobotList)
        //        {
        //            if (item.Count == 3)
        //            {
        //                List<mRobot> temp = item.OrderByDescending(t => t.Confidence).ToList();
        //                temp.RemoveAt(2);
        //                MathMatrix coefMat;
        //                MathMatrix commonMat = MatrixMaker(temp.OrderByDescending(t => t.CamID).ToList(), out coefMat);
        //                if (coefMat == null)
        //                    continue;
        //                MathMatrix robotPos = coefMat * commonMat.Transpose;
        //                double confidence = 0;
        //                double time = 0;
        //                foreach (var dic in temp)
        //                {
        //                    confidence += dic.Confidence;
        //                    time += dic.Time;
        //                }
        //                confidence /= (double)temp.Count;
        //                time /= (double)temp.Count;

        //                int key = 0;
        //                List<int> ids = new List<int>();
        //                foreach (var id in temp)
        //                    ids.Add(id.CamID);
        //                foreach (var id in ids)
        //                {
        //                    key *= 10;
        //                    key += id;
        //                }
        //                finalRobot.Add(new mRobot(confidence, temp[0].RobotID, time, temp[0].Oriention, robotPos[0, 0], robotPos[1, 0], ids[0])); // final merged Our Robots
        //            }
        //            else
        //            {
        //                MathMatrix coefMat;
        //                MathMatrix commonMat = MatrixMaker(item.OrderByDescending(t => t.CamID).ToList(), out coefMat);
        //                if (coefMat == null)
        //                    continue;
        //                MathMatrix robotPos = coefMat * commonMat.Transpose;
        //                double confidence = 0;
        //                double time = 0;
        //                foreach (var dic in item)
        //                {
        //                    confidence += dic.Confidence;
        //                    time += dic.Time;
        //                }
        //                confidence /= (double)item.Count;
        //                time /= (double)item.Count;

        //                int key = 0;
        //                List<int> ids = new List<int>();
        //                foreach (var id in item)
        //                    ids.Add(id.CamID);
        //                foreach (var id in ids)
        //                {
        //                    key *= 10;
        //                    key += id;
        //                }
        //                finalRobot.Add(new mRobot(confidence, item[0].RobotID, time, item[0].Oriention, robotPos[0, 0], robotPos[1, 0], ids[0])); // final merged Our Robots
        //            }
        //        }
        //        foreach (var item in finalRobot)
        //        {
        //            Frame.OppRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
        //            newFrame.OppRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
        //        }
        //    }
        //    #endregion

        //    sslPackets = new Dictionary<uint, SSL_WrapperPacket>();
        //    lastColorIsYellow = isYellow;
        //    return true;
        //}
        int firstRunCounter = 0;
        Dictionary<int, double> diffTimes = new Dictionary<int, double>();
        List<int> lastAvailableCameras = new List<int>();
        CmuMerger cmuMerger = new CmuMerger();
        public bool Merge(SSL_WrapperPacket Packet, ref frame Frame, ref frame newFrame, bool isYellow, Position2D selectedBall,ref bool selectedBallChanged, bool isReverse)
        {
            //DrawingObjects.AddObject(new Circle(Vision2AI(new Position2D(-1994, -487), false), StaticVariables.BALL_RADIUS, new Pen(Color.Red, 0.01f)), "badssdddscdsll");
            //DrawingObjects.AddObject(new Circle(Vision2AI(new Position2D(-1984, -427), false), StaticVariables.BALL_RADIUS, new Pen(Color.Red, 0.01f)), "badscsdll");
            //DrawingObjects.AddObject(new Circle(Vision2AI(new Position2D(-1918, -475), false), StaticVariables.BALL_RADIUS, new Pen(Color.PeachPuff, 0.01f)), "badcdsdscsdll");
            newFrame = new frame();
            Dictionary<int, mRobot> ourRobots = new Dictionary<int, mRobot>();
            Dictionary<int, mRobot> oppRobots = new Dictionary<int, mRobot>();
            Dictionary<int, mBall> balls = new Dictionary<int, mBall>();

            if (Packet == null || Packet.detection == null)
                return false;
            // count how many cameras are sending, at first
            uint camera = Packet.detection.camera_id;
            if (!lastAvailableCameras.All(a => MergerParameters.AvailableCamIds.Contains(a))
                || !MergerParameters.AvailableCamIds.All(a => lastAvailableCameras.Contains(a)))
            {
                firstRunCounter = 0;
                num_cameras = 0;
                frames = 0;
                for (int i = 0; i < cameras_seen.Length; i++)
                {
                    cameras_seen[i] = false;
                }
                num_cameras_seen = 0;
            }
            lastAvailableCameras = MergerParameters.AvailableCamIds.ToList();
            if (!MergerParameters.AvailableCamIds.Contains((int)Packet.detection.camera_id))
                return false;
            if (num_cameras <= 0)
            {
                if (!cameras_seen[camera])
                {
                    num_cameras_seen++;
                    cameras_seen[camera] = true;
                }

                if (frames++ >= 100)
                {
                    num_cameras = num_cameras_seen;

                    for (int i = 0; i < cameras_seen.Length; i++)
                    {
                        cameras_seen[i] = false;
                    }
                    num_cameras_seen = 0;
                }
                else
                {
                    return false;
                }
            }

            if (Packet.detection.camera_id == 0)
            {
                sslpacketCam0 = Packet;
            }
            if (Packet.detection.camera_id == 1)
            {
                sslpacketCam1 = Packet;
            }
            if (Packet.detection.camera_id == 2)
            {
                sslpacketCam2 = Packet;
            }
            if (Packet.detection.camera_id == 3)
            {
                sslpacketCam3 = Packet;
            }
            if (Packet.detection.camera_id == 4)
            {
                sslpacketCam4 = Packet;
            } if (Packet.detection.camera_id == 5)
            {
                sslpacketCam5 = Packet;
            } if (Packet.detection.camera_id == 6)
            {
                sslpacketCam6 = Packet;
            } if (Packet.detection.camera_id == 7)
            {
                sslpacketCam7 = Packet;
            }
            

            
            if (sslPackets.ContainsKey(Packet.detection.camera_id))
            {
                sslPackets[Packet.detection.camera_id] = Packet;
            }
            else
                sslPackets.Add(Packet.detection.camera_id, Packet);
            

            if (!cameras_seen[camera])
            {
                num_cameras_seen++;
                cameras_seen[camera] = true;
            }

            ready = (num_cameras_seen == num_cameras);
            if (!ready)
                return false;
            
            //if (sslPackets.Count < MergerParameters.AvailableCamIds.Count)
            //    return false;

            
            if (firstRunCounter < 10)
            {
                firstRunCounter++;
                if (firstRunCounter == 1)
                {
                    diffTimes.Clear();
                    for (int i = 0; i < StaticVariables.VisionPcCounts; i++)
                    {
                        diffTimes.Add(i, 0);
                    }
                }
                Dictionary<int, double> minTime = new Dictionary<int, double>();
                for (int i = 0; i < StaticVariables.VisionPcCounts; i++)
                {
                    
                    int cameraPerPc = StaticVariables.CameraCount / StaticVariables.VisionPcCounts;
                    var packs = sslPackets.Where(w => w.Value.detection.camera_id >= i * cameraPerPc && w.Value.detection.camera_id < (i + 1) * cameraPerPc);
                    if (packs.Count() > 0)
                    {

                        minTime[i] = packs.Min(m => m.Value.detection.t_capture);
                    }
                    else
                        minTime[i] = 0;
                }
                double maxTime = minTime.Max(m => m.Value);
                for (int i = 0; i < StaticVariables.VisionPcCounts; i++)
                {
                    diffTimes[i] *= (firstRunCounter - 1);
                    diffTimes[i] += maxTime - minTime[i];
                    diffTimes[i] /= (firstRunCounter);
                }
            }
            else 
            {
                ;
            }

            if (lastColorIsYellow != isYellow)
                Frame = new frame();
            Frame.timeofcapture = 0;
            #region capture Time
            for (int i = 0; i < sslPackets.Count; i++)
            {
                int pc = GetCameraPC(sslPackets.ElementAt(i).Value.detection.camera_id);
                sslPackets.ElementAt(i).Value.detection.t_capture += diffTimes[pc];
                Frame.timeofcapture += sslPackets.ElementAt(i).Value.detection.t_capture;
                Frame.timeList[sslPackets.ElementAt(i).Value.detection.camera_id] = sslPackets.ElementAt(i).Value.detection.t_capture;
            }
            if (sslPackets.Count > 0)
                Frame.timeofcapture /= (double)sslPackets.Count;

            #endregion

            int counterBall = 0;
            int counterOur = 0;
            int counterOpp = 0;

            Balls = new Dictionary<uint, vraw>();
            int cameras = 0;
            foreach (var pack in sslPackets)
            {
                cameras++;
                cmuMerger.UpdateVision(pack.Value.detection, isYellow, selectedBall,ref selectedBallChanged, isReverse, cameras == sslPackets.Count);
                Frame.OtherBalls = cmuMerger.World.OtherBalls;
                //////////List<SSL_DetectionRobot> ourRobotsPacket, oppRobotsPacket;
                //////////if (isYellow)
                //////////{
                //////////    ourRobotsPacket = pack.Value.detection.robots_yellow;
                //////////    oppRobotsPacket = pack.Value.detection.robots_blue;
                //////////}
                //////////else
                //////////{
                //////////    ourRobotsPacket = pack.Value.detection.robots_blue;
                //////////    oppRobotsPacket = pack.Value.detection.robots_yellow;
                //////////}
                //////////foreach (var our in ourRobotsPacket)
                //////////{
                //////////    ourRobots.Add(counterOur, new mRobot(our.confidence, our.robot_id, pack.Value.detection.t_capture, our.orientation, our.x, our.y, pack.Value.detection.camera_id));
                //////////    counterOur++;
                //////////}
                //////////foreach (var opp in oppRobotsPacket)
                //////////{
                //////////    oppRobots.Add(counterOpp, new mRobot(opp.confidence, opp.robot_id, pack.Value.detection.t_capture, opp.orientation, opp.x, opp.y, pack.Value.detection.camera_id));
                //////////    counterOpp++;
                //////////}
                ////////////List<SSL_DetectionBall> balls = pack.Value.detection.balls;
                //////////for (int j = 0; j < pack.Value.detection.balls.Count && j < StaticVariables.MaxBalls; j++)
                //////////{
                //////////    var b = pack.Value.detection.balls[j];
                //////////    if (b.confidence > minConfidence)
                //////////        balls.Add(balls.Count, new mBall(b.confidence, pack.Value.detection.t_capture, b.x, b.y, (int)pack.Value.detection.camera_id));
                //////////}
            }
            if (cmuMerger.World.Balls.Count > 0)
            {
                var ball = cmuMerger.World.Balls[0].vision.pos;
                ball = Vision2AI(ball, isReverse);
                DrawingObjects.AddObject(new Circle(ball, 0.04, new Pen(Color.RosyBrown, 0.01f)), "cmu ball");
                StaticVariables.BallPositions = new List<Position2D>();
                StaticVariables.BallPositions.Add(cmuMerger.World.Balls[0].vision.pos);
            }

            uint count = 0;
            #region comment
            ////////#region Ball
            ////////StaticVariables.FrameHasBall = false;
            ////////if (balls.Count > 0)
            ////////{
            ////////    List<mBall> finalBall = new List<mBall>();
            ////////    List<List<mBall>> selectedBallList = new List<List<mBall>>();
            ////////    List<int> selectedBallIdList = new List<int>();
            ////////    foreach (var ball in balls)
            ////////    {
            ////////        if (!selectedBallIdList.Contains(ball.Key))
            ////////        {
            ////////            List<mBall> ballList = new List<mBall>();
            ////////            ballList.Add(ball.Value);
            ////////            selectedBallIdList.Add(ball.Key);
            ////////            foreach (var ball2 in balls)
            ////////            {
            ////////                if (!selectedBallIdList.Contains(ball2.Key) && ball.Value.CamID != ball2.Value.CamID && ball.Value.Pos.DistanceFrom(ball2.Value.Pos) < disEps)
            ////////                {
            ////////                    ballList.Add(ball2.Value);
            ////////                    selectedBallIdList.Add(ball2.Key);
            ////////                }
            ////////            }
            ////////            selectedBallList.Add(ballList);
            ////////        }
            ////////    }

            ////////    foreach (var item in selectedBallList)
            ////////    {
            ////////        MathMatrix coefMat;
            ////////        MathMatrix commonMat;
            ////////        MathMatrix ballPos;

            ////////        double confidence = 0;
            ////////        double time = 0;
            ////////        double x = 0, y = 0;
            ////////        foreach (var ball in item)
            ////////        {
            ////////            commonMat = MatrixMaker(ball.Pos, ball.CamID, out coefMat);
            ////////            if (coefMat == null)
            ////////                continue;
            ////////            ballPos = coefMat * commonMat.Transpose;
            ////////            x += ballPos[0, 0];
            ////////            y += ballPos[1, 0];
            ////////            confidence += ball.Confidence;
            ////////            time += ball.Time;
            ////////        }
            ////////        confidence /= (double)item.Count;
            ////////        time /= (double)item.Count;
            ////////        x /= (double)item.Count;
            ////////        y /= (double)item.Count;
            ////////        int key = 0;
            ////////        List<int> ids = new List<int>();
            ////////        foreach (var id in item)
            ////////            ids.Add(id.CamID);
            ////////        foreach (var id in ids)
            ////////        {
            ////////            key *= 10;
            ////////            key += id;
            ////////        }
            ////////        finalBall.Add(new mBall(confidence, time, x, y, ids[0])); // final merged balls
            ////////    }
            ////////    StaticVariables.BallPositions = new List<Position2D>();

            ////////    foreach (var item in finalBall)
            ////////    {
            ////////        if (IsInField(item.Pos, 100))
            ////////        {
            ////////            StaticVariables.FrameHasBall = true;
            ////////            Balls.Add(count, new vraw(item.Time, item.Pos, 0, (float)item.Confidence, (uint)item.CamID));
            ////////            StaticVariables.BallPositions.Add(item.Pos);
            ////////            count++;
            ////////        }
            ////////    }
            ////////    //foreach (var item in balls)
            ////////    //{
            ////////    //    DrawingObjects.AddObject(new Circle(Vision2AI(item.Value.Pos, false), StaticVariables.BALL_RADIUS, new Pen(Color.GreenYellow, 0.01f)), "ball" + item.Value.Pos.toString());
            ////////    //    //Balls.Add(count, new vraw(item.Value.Time, item.Value.Pos, 0, (float)item.Value.Confidence, (uint)item.Value.CamID));
            ////////    //    StaticVariables.BallPositions = new List<Position2D>();
            ////////    //    StaticVariables.BallPositions.Add(item.Value.Pos);
            ////////    //    count++;
            ////////    //}
            ////////}
            ////////#endregion

            ////////#region our
            ////////if (ourRobots.Count > 0)
            ////////{
            ////////    List<mRobot> finalRobot = new List<mRobot>();
            ////////    List<List<mRobot>> selectedOurRobotList = new List<List<mRobot>>();
            ////////    List<int> selectedOurRobotIdList = new List<int>();
            ////////    foreach (var robot in ourRobots)
            ////////    {
            ////////        if (!selectedOurRobotIdList.Contains(robot.Key))
            ////////        {
            ////////            List<mRobot> ourList = new List<mRobot>();
            ////////            ourList.Add(robot.Value);
            ////////            selectedOurRobotIdList.Add(robot.Key);
            ////////            foreach (var robot2 in ourRobots)
            ////////            {
            ////////                if (!selectedOurRobotIdList.Contains(robot2.Key) && robot.Value.CamID != robot2.Value.CamID && robot.Value.RobotID == robot2.Value.RobotID)
            ////////                {
            ////////                    ourList.Add(robot2.Value);
            ////////                    selectedOurRobotIdList.Add(robot2.Key);
            ////////                }
            ////////            }
            ////////            selectedOurRobotList.Add(ourList);
            ////////        }
            ////////    }
            ////////    foreach (var item in selectedOurRobotList)
            ////////    {
            ////////        MathMatrix coefMat;
            ////////        MathMatrix commonMat;
            ////////        MathMatrix ourRobotPos;

            ////////        double confidence = 0;
            ////////        double time = 0;
            ////////        double x = 0, y = 0;
            ////////        foreach (var ball in item)
            ////////        {
            ////////            commonMat = MatrixMaker(ball.Pos, ball.CamID, out coefMat);
            ////////            if (coefMat == null)
            ////////                continue;
            ////////            ourRobotPos = coefMat * commonMat.Transpose;
            ////////            x += ourRobotPos[0, 0];
            ////////            y += ourRobotPos[1, 0];
            ////////            confidence += ball.Confidence;
            ////////            //if (ball.Time  < time)
            ////////            //{
            ////////            //    time = ball.Time;
            ////////            //}
            ////////            time += ball.Time;
            ////////        }
            ////////        confidence /= (double)item.Count;
            ////////        time /= (double)item.Count;
            ////////        x /= (double)item.Count;
            ////////        y /= (double)item.Count;
            ////////        int key = 0;
            ////////        List<int> ids = new List<int>();
            ////////        foreach (var id in item)
            ////////            ids.Add(id.CamID);
            ////////        foreach (var id in ids)
            ////////        {
            ////////            key *= 10;
            ////////            key += id;
            ////////        }
            ////////        finalRobot.Add(new mRobot(confidence, item[0].RobotID, time, item[0].Oriention, x, y, ids[0]));
            ////////    }
            ////////    foreach (var item in finalRobot)
            ////////    {
            ////////        Frame.OurRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
            ////////        newFrame.OurRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
            ////////    }
            ////////}
            ////////#endregion
            ////////#region opp
            ////////if (oppRobots.Count > 0)
            ////////{
            ////////    List<mRobot> finalRobot = new List<mRobot>();
            ////////    List<List<mRobot>> selectedOppRobotList = new List<List<mRobot>>();
            ////////    List<int> selectedOppRobotIdList = new List<int>();
            ////////    foreach (var robot in oppRobots)
            ////////    {
            ////////        if (!selectedOppRobotIdList.Contains(robot.Key))
            ////////        {
            ////////            List<mRobot> ourList = new List<mRobot>();
            ////////            ourList.Add(robot.Value);
            ////////            selectedOppRobotIdList.Add(robot.Key);
            ////////            foreach (var robot2 in oppRobots)
            ////////            {
            ////////                if (!selectedOppRobotIdList.Contains(robot2.Key) && robot.Value.CamID != robot2.Value.CamID && robot.Value.RobotID == robot2.Value.RobotID)
            ////////                {
            ////////                    ourList.Add(robot2.Value);
            ////////                    selectedOppRobotIdList.Add(robot2.Key);
            ////////                }
            ////////            }
            ////////            selectedOppRobotList.Add(ourList);
            ////////        }
            ////////    }
            ////////    foreach (var item in selectedOppRobotList)
            ////////    {
            ////////        MathMatrix coefMat;
            ////////        MathMatrix commonMat;
            ////////        MathMatrix ourRobotPos;

            ////////        double confidence = 0;
            ////////        double time = 0;
            ////////        double x = 0, y = 0;
            ////////        foreach (var ball in item)
            ////////        {
            ////////            commonMat = MatrixMaker(ball.Pos, ball.CamID, out coefMat);
            ////////            if (coefMat == null)
            ////////                continue;
            ////////            ourRobotPos = coefMat * commonMat.Transpose;
            ////////            x += ourRobotPos[0, 0];
            ////////            y += ourRobotPos[1, 0];
            ////////            confidence += ball.Confidence;
            ////////            time += ball.Time;
            ////////        }
            ////////        confidence /= (double)item.Count;
            ////////        time /= (double)item.Count;
            ////////        x /= (double)item.Count;
            ////////        y /= (double)item.Count;
            ////////        int key = 0;
            ////////        List<int> ids = new List<int>();
            ////////        foreach (var id in item)
            ////////            ids.Add(id.CamID);
            ////////        foreach (var id in ids)
            ////////        {
            ////////            key *= 10;
            ////////            key += id;
            ////////        }
            ////////        finalRobot.Add(new mRobot(confidence, item[0].RobotID, time, item[0].Oriention, x, y, ids[0]));
            ////////    }
            ////////    foreach (var item in finalRobot)
            ////////    {
            ////////        Frame.OppRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
            ////////        newFrame.OppRobots[(uint)item.RobotID] = new vrobot(new vraw(item.Time, item.Pos, (float)item.Oriention, (float)item.Confidence, (uint)item.CamID), new SingleObjectState());
            ////////    }
            ////////}
            ////////#endregion
            #endregion
            newFrame = cmuMerger.World;
            foreach (var item in cmuMerger.World.OurRobots)
            {
                Frame.OurRobots[(uint)item.Key] = new vrobot(new vraw(item.Value.vision), new SingleObjectState());
            }
            foreach (var item in cmuMerger.World.OppRobots)
            {
                Frame.OppRobots[(uint)item.Key] = new vrobot(new vraw(item.Value.vision), new SingleObjectState());
            }
            Balls = new Dictionary<uint, vraw>();
            if (cmuMerger.World.Balls.Count > 0)
                Balls.Add(0, new vraw(cmuMerger.World.Balls[0].vision));
            sslPackets = new Dictionary<uint, SSL_WrapperPacket>();
            lastColorIsYellow = isYellow;

            for (int i = 0; i < cameras_seen.Length; i++)
            {
                cameras_seen[i] = false;
            }
            num_cameras_seen = 0;

            return true;
        }
   
        private int GetCameraPC(uint id)
        {
            int cameraPerPc = StaticVariables.CameraCount / StaticVariables.VisionPcCounts;
            return (int)id / cameraPerPc;
        }

        private static bool IsInField(Position2D pos, double margin)
        {
            if (pos.X < -GameParameters.OurGoalCenter.X * 1000 - margin || pos.X > GameParameters.OurGoalCenter.X * 1000 + margin)
                return false;
            if (pos.Y < -GameParameters.OurLeftCorner.Y * 1000 - margin || pos.Y > GameParameters.OurLeftCorner.Y * 1000 + margin)
                return false;
            return true;
        }

        private MathMatrix MatrixMaker(List<mBall> selectedBall, out MathMatrix coefMat)
        {

            MathMatrix refMat = MathMatrix.IdentityMatrix(1, 1);
            List<int> camIdList = new List<int>();
            foreach (var item in selectedBall)
                camIdList.Add(item.CamID);
            coefMat = MergerParameters.GetMatrix(camIdList);

            switch (selectedBall.Count)
            {
                case 1:
                    refMat = new MathMatrix(1, 3);
                    refMat[0, 0] = selectedBall[0].Pos.X;
                    refMat[0, 1] = selectedBall[0].Pos.Y;
                    refMat[0, 2] = 1;
                    break;
                case 2:
                    refMat = new MathMatrix(1, 5);
                    refMat[0, 0] = selectedBall[0].Pos.X;
                    refMat[0, 1] = selectedBall[0].Pos.Y;
                    refMat[0, 2] = selectedBall[1].Pos.X;
                    refMat[0, 3] = selectedBall[1].Pos.Y;
                    refMat[0, 4] = 1;
                    break;
                case 3:
                    refMat = new MathMatrix(1, 7);
                    refMat[0, 0] = selectedBall[0].Pos.X;
                    refMat[0, 1] = selectedBall[0].Pos.Y;
                    refMat[0, 2] = selectedBall[1].Pos.X;
                    refMat[0, 3] = selectedBall[1].Pos.Y;
                    refMat[0, 4] = selectedBall[2].Pos.X;
                    refMat[0, 5] = selectedBall[2].Pos.Y;
                    refMat[0, 6] = 1;
                    break;
                case 4:
                    refMat = new MathMatrix(1, 9);
                    refMat[0, 0] = selectedBall[0].Pos.X;
                    refMat[0, 1] = selectedBall[0].Pos.Y;
                    refMat[0, 2] = selectedBall[1].Pos.X;
                    refMat[0, 3] = selectedBall[1].Pos.Y;
                    refMat[0, 4] = selectedBall[2].Pos.X;
                    refMat[0, 5] = selectedBall[2].Pos.Y;
                    refMat[0, 6] = selectedBall[3].Pos.X;
                    refMat[0, 7] = selectedBall[3].Pos.Y;
                    refMat[0, 8] = 1;
                    break;
            }
            return refMat;
        }

        private MathMatrix MatrixMaker(List<mRobot> selectedRobot, out MathMatrix coefMat)
        {

            MathMatrix refMat = MathMatrix.IdentityMatrix(1, 1);
            List<int> camIdList = new List<int>();
            foreach (var item in selectedRobot)
                camIdList.Add(item.CamID);
            coefMat = MergerParameters.GetMatrix(camIdList);
            switch (selectedRobot.Count)
            {
                case 1:
                    refMat = new MathMatrix(1, 3);
                    refMat[0, 0] = selectedRobot[0].Pos.X;
                    refMat[0, 1] = selectedRobot[0].Pos.Y;
                    refMat[0, 2] = 1;
                    break;
                case 2:
                    refMat = new MathMatrix(1, 5);
                    refMat[0, 0] = selectedRobot[0].Pos.X;
                    refMat[0, 1] = selectedRobot[0].Pos.Y;
                    refMat[0, 2] = selectedRobot[1].Pos.X;
                    refMat[0, 3] = selectedRobot[1].Pos.Y;
                    refMat[0, 4] = 1;
                    break;
                case 3:
                    refMat = new MathMatrix(1, 7);
                    refMat[0, 0] = selectedRobot[0].Pos.X;
                    refMat[0, 1] = selectedRobot[0].Pos.Y;
                    refMat[0, 2] = selectedRobot[1].Pos.X;
                    refMat[0, 3] = selectedRobot[1].Pos.Y;
                    refMat[0, 4] = selectedRobot[2].Pos.X;
                    refMat[0, 5] = selectedRobot[2].Pos.Y;
                    refMat[0, 6] = 1;
                    break;
                case 4:
                    refMat = new MathMatrix(1, 9);
                    refMat[0, 0] = selectedRobot[0].Pos.X;
                    refMat[0, 1] = selectedRobot[0].Pos.Y;
                    refMat[0, 2] = selectedRobot[1].Pos.X;
                    refMat[0, 3] = selectedRobot[1].Pos.Y;
                    refMat[0, 4] = selectedRobot[2].Pos.X;
                    refMat[0, 5] = selectedRobot[2].Pos.Y;
                    refMat[0, 6] = selectedRobot[3].Pos.X;
                    refMat[0, 7] = selectedRobot[3].Pos.Y;
                    refMat[0, 8] = 1;
                    break;
            }
            return refMat;
        }

        private MathMatrix MatrixMaker(Position2D selectedPos, int camID, out MathMatrix coefMat)
        {
            MathMatrix refMat = MathMatrix.IdentityMatrix(1, 1);
            List<int> camIdList = new List<int>();
            camIdList.Add(camID);
            coefMat = MergerParameters.GetMatrix(camIdList);

            refMat = new MathMatrix(1, 3);
            refMat[0, 0] = selectedPos.X;
            refMat[0, 1] = selectedPos.Y;
            refMat[0, 2] = 1;

            return refMat;
        }

        private Position2D Vision2AI(Position2D pos, bool isReverseSide)
        {
            if (isReverseSide)
                return new Position2D(-pos.X / 1000, pos.Y / 1000);
            return new Position2D(pos.X / 1000, -pos.Y / 1000);
        }

    }
}