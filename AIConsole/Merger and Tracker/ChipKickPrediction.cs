using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using MRL.SSL.GameDefinitions;
using messages_robocup_ssl_wrapper;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonCLasses.MathLibarary;

namespace MRL.SSL.AIConsole
{
    public class ChipKickPrediction
    {
        double oppAngle = 0;
        double oppAngle2 = 0;
        Position2D oppRobot = new Position2D();
        double[,] camQ = new double[4, 4] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
        Vector3D[] camT = { new Vector3D(), new Vector3D(), new Vector3D(), new Vector3D() };
        Queue<Position2D> points = new Queue<Position2D>();
        private static Position2D firstBallPos = new Position2D();
        static Position3D firstPos = new Position3D();
        static Position3D lastPos = new Position3D();
        Position2D currentBallPos = new Position2D();
        Position3D CamFirstPos = new Position3D();
        Position2D lastBallPos = new Position2D();
        List<double> deltaT = new List<double>();
        Vector3D lastParabolaV = new Vector3D();
        static private bool oppHasBall = false;
        static private bool ballIsMove = false;
        Queue<double> t = new Queue<double>();
        Vector3D parabolaV = new Vector3D();
        MathMatrix A = new MathMatrix(0, 0);
        MathMatrix X = new MathMatrix(3, 1);
        MathMatrix B = new MathMatrix(0, 0);
        Vector2D robotvector = new Vector2D();
        private bool cameras_notset = true;
        Position3D pos1 = new Position3D();
        Position3D pos = new Position3D();
        static bool ballConflict = false;
        static int lastBallSelected = 0;
        private bool debug = true;
        bool firstBallFlag = true;
        int? oppBallOwnerID = null;
        SSL_WrapperPacket packet1;
        SSL_WrapperPacket packet2;
        const double vMin = 0.03;
        static int ourCamID = -1;
        static int oppCamID = -1;
        static int camID = -1;
        bool isotchiInInitialstate = true;
        double currentT;
        double t0;
        int cam0 = 2;
        int cam1 = 0;
        int cam2 = 1;
        int cam3 = 3;
        int lastcamID = 0;
        bool returnLastBallpos = false;
        Position2D lastBallPosition = new Position2D();
        static bool start = false;
        public static bool Start
        {
            get
            {
                return ChipKickPrediction.start;
            }
            set
            {
                ChipKickPrediction.start = value;
            }
        }

        static int counter = 0;
        public static int Counter
        {
            get
            {
                return ChipKickPrediction.counter;
            }
            set
            {
                ChipKickPrediction.counter = value;
            }
        }

        static Position2D finalPoint = new Position2D();

        public static Position2D FinalPoint
        {
            get
            {
                return ChipKickPrediction.finalPoint;
            }
            set
            {
                ChipKickPrediction.finalPoint = value;
            }
        }
        static Queue<Position2D> Poses = new Queue<Position2D>();
        private bool firstTime = true;
        public Position2D Predict(SSL_WrapperPacket sslPacket, bool isYellow, bool isReverse, ref bool IsChip)
        {

            if (sslPacket.detection != null)
                if (sslPacket.detection.camera_id == cam1)
                    packet1 = sslPacket;
                else
                    packet2 = sslPacket;

            CameraParameters(sslPacket, isYellow);
            if (sslPacket.detection != null && sslPacket.detection.balls.Count != 0)
            {
                //if (sslPacket.detection.camera_id != cam0 && sslPacket.detection.camera_id != cam3)
                //{

                    currentBallPos = BallDitector(sslPacket.detection);
                    Poses.Enqueue(new Position2D(currentBallPos.X / 1000, -currentBallPos.Y / 1000));
                    currentT = sslPacket.detection.t_capture;
                    BallIsMoved(sslPacket, isYellow, currentBallPos, firstBallPos, isReverse);

                    lastBallPos = currentBallPos;

                    if (ballIsMove && oppHasBall && sslPacket.detection.camera_id == camID)
                    {
                        AddBall(currentBallPos, currentT, (int)sslPacket.detection.camera_id);
                        if (points.Count >= 3)
                        {
                            counter++;
                            SetMatrix((int)sslPacket.detection.camera_id);
                            X = (Inverse.invert(A.Transpose * A) * A.Transpose) * B;
                            parabolaV = Image2FieldVec(new Vector3D(X[1, 0], X[2, 0], X[0, 0]), (int)sslPacket.detection.camera_id) / 1000.0;

                            if (parabolaV.Z < 0)
                                parabolaV = lastParabolaV;
                            else
                                lastParabolaV = parabolaV;

                            finalPoint = new Position2D(firstPos.X + parabolaV.X * ((parabolaV.Z / 9.8) * 2), firstPos.Y + parabolaV.Y * ((parabolaV.Z / 9.8) * 2));
                            bool Debug = true;
                            if (Debug)
                            {
                                DrawingObjects.AddObject(new StringDraw("X0=" + firstPos.X.ToString(), new Position2D(.5, 1)), "ss");
                                DrawingObjects.AddObject(new StringDraw("Y0=" + firstPos.Y.ToString(), new Position2D(.3, 1)), "ss1");
                                DrawingObjects.AddObject(new StringDraw("Z0=" + firstPos.Z.ToString(), new Position2D(.1, 1)), "ss2");
                                DrawingObjects.AddObject(new StringDraw("Vx=" + parabolaV.X.ToString(), new Position2D(-.1, 1)), "aa");
                                DrawingObjects.AddObject(new StringDraw("Vy=" + parabolaV.Y.ToString(), new Position2D(-.3, 1)), "aa1");
                                DrawingObjects.AddObject(new StringDraw("Vz=" + parabolaV.Z.ToString(), new Position2D(-.5, 1)), "aa2");
                            }
                        }
                    }
                    else
                    {
                        if (packet1 != null && packet2 != null)
                        {
                            if (packet1.detection.balls.Count > 0 && packet2.detection.balls.Count > 0)
                            {
                                List<messages_robocup_ssl_detection.SSL_DetectionBall> ball1 = packet1.detection.balls;
                                List<messages_robocup_ssl_detection.SSL_DetectionBall> ball2 = packet2.detection.balls;
                                foreach (var item in ball1)
                                {
                                    foreach (var item1 in ball2)
                                    {
                                        if (Math.Abs(item.x - item1.x) < 50f && Math.Abs(item.y - item1.y) < 30f)
                                        {
                                            ballConflict = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    IsChip = SimpleChipDitector(sslPacket, new Position2D(currentBallPos.X / 1000, currentBallPos.Y / 1000), new Position2D(firstBallPos.X / 1000, firstBallPos.Y / 1000));

                    //DrawingObjects.AddObject(new StringDraw((IsChip) ? "Ischip" : "IsDirect", new Position2D(1, 0)), "54654654");
                    double displacement = 0;
                    if (Poses.Count > 8)
                    {
                        displacement = Poses.First().DistanceFrom(Poses.Last());
                        Poses.Dequeue();
                    }
                    //DrawingObjects.AddObject(new StringDraw("displacement: " + displacement, new Position2D(.2, 0)), "5497889564");
                    if (displacement < .02)
                    {
                        Clear();
                    }
                    else
                        ballIsMove = true;
                //}
            }
            double coeff = 1;
            coeff = (isReverse) ? -1 : 1;
            Position2D pos = new Position2D();
            if (returnLastBallpos)
                pos = lastBallPosition;
            else
                pos = new Position2D(coeff * finalPoint.X, coeff * finalPoint.Y);
            if (!returnLastBallpos)
                lastBallPosition = new Position2D(coeff * finalPoint.X, coeff * finalPoint.Y);
            return pos;

        }

        public Position3D Field2Image(Position2D pF, int cameraId)
        {
            Quater qField2Cam = new Quater(camQ[cameraId, 0], camQ[cameraId, 1], camQ[cameraId, 2], camQ[cameraId, 3]);
            Vector3D transVec = camT[cameraId];
            Vector3D pFVec = new Vector3D(pF.X, pF.Y, 0);
            qField2Cam.Normalize();
            Vector3D pCVec = qField2Cam.RotateVectorByQuaternion(pFVec) + transVec;
            return new Position3D(pCVec.X, pCVec.Y, pCVec.Z);
        }

        public Position3D Image2Field(Position3D pF, int cameraId)
        {
            Quater qField2Cam = new Quater(camQ[cameraId, 0], camQ[cameraId, 1], camQ[cameraId, 2], camQ[cameraId, 3]);
            Vector3D transVec = camT[cameraId];
            Vector3D pFVec = new Vector3D(pF.X, pF.Y, pF.Z);

            qField2Cam.Normalize();
            qField2Cam.Invert();
            pFVec -= transVec;
            Vector3D pCVec = qField2Cam.RotateVectorByQuaternion(pFVec);
            return new Position3D(pCVec.X, -pCVec.Y, pCVec.Z);
        }

        public Vector3D Field2ImageVec(Vector3D pFVec, int cameraId)
        {
            Quater qField2Cam = new Quater(camQ[cameraId, 0], camQ[cameraId, 1], camQ[cameraId, 2], camQ[cameraId, 3]);
            qField2Cam.Normalize();
            return qField2Cam.RotateVectorByQuaternion(pFVec);
        }

        public Vector3D Image2FieldVec(Vector3D pFVec, int cameraId)
        {
            Quater qField2Cam = new Quater(camQ[cameraId, 0], camQ[cameraId, 1], camQ[cameraId, 2], camQ[cameraId, 3]);
            qField2Cam.Normalize();
            qField2Cam.Invert();
            Vector3D pCVec = qField2Cam.RotateVectorByQuaternion(pFVec);
            return new Vector3D(pCVec.X, -pCVec.Y, pCVec.Z);
        }

        public bool SelectedBallIndexViewed(int BallIndex, List<messages_robocup_ssl_detection.SSL_DetectionBall> detectBalls)
        {
            bool ret = false;

            if (-detectBalls[BallIndex].x / 1000 > GameParameters.OppGoalLeft.X - RobotParameters.OurRobotParams.Diameter / 2.0 && -detectBalls[BallIndex].x / 1000 < GameParameters.OurGoalLeft.X + RobotParameters.OurRobotParams.Diameter / 2.0 && detectBalls[BallIndex].y / 1000 > GameParameters.OurRightCorner.Y - RobotParameters.OurRobotParams.Diameter / 2.0 && detectBalls[BallIndex].y / 1000 < GameParameters.OurLeftCorner.Y + RobotParameters.OurRobotParams.Diameter / 2.0)
            {
                return true;
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        public Position2D BallDitector(messages_robocup_ssl_detection.SSL_DetectionFrame detects)
        {
            double mindis = double.MaxValue;
            int minindex = -1;
            List<messages_robocup_ssl_detection.SSL_DetectionBall> balls = detects.balls;
            int camID = (int)detects.camera_id;
            for (int i = 0; i < balls.Count; i++)
            {
                double dist = distance(balls[lastBallSelected].x, balls[lastBallSelected].y, balls[i].x, balls[i].y);
                if (dist < mindis)
                {
                    minindex = i;
                    mindis = distance(balls[lastBallSelected].x, balls[lastBallSelected].y, balls[i].x, balls[i].y);

                }
            }
            if (minindex != -1)
            {
                lastBallSelected = minindex;
            }
            if (firstTime)
            {
                firstTime = false;
                lastcamID = camID;
            }


            int ballId = lastBallSelected;
            if (firstBallFlag)
            {
                lastBallPos = new Position2D(balls[ballId].x, balls[ballId].y);
                firstBallPos = lastBallPos;
                firstBallFlag = false;
            }
            if (camID != lastcamID && !returnLastBallpos)
            {
                returnLastBallpos = true;
            }
            lastcamID = camID;
            return new Position2D(balls[ballId].x, balls[ballId].y);
        }

        public void AddBall(Position2D ballPos, double captureT, int camId)
        {
            if (t.Count == 0)
            {
                points.Enqueue(ballPos);
                firstPos = new Position3D(ballPos.X / 1000, -(ballPos.Y / 1000), 0);
                CamFirstPos = Field2Image(new Position2D(ballPos.X, ballPos.Y), camId);
                t.Enqueue(captureT);
                t0 = t.FirstOrDefault();
            }
            else
            {
                lastPos = new Position3D(ballPos.X / 1000, -(ballPos.Y / 1000), 0);
                t.Enqueue(captureT);
                t0 = t.FirstOrDefault();
                points.Enqueue(ballPos);
            }
        }

        public void SetMatrix(int camId)
        {
            deltaT = new List<double>();
            A = new MathMatrix(points.Count * 2, 3);
            B = new MathMatrix(points.Count * 2, 1);
            Vector3D g = new Vector3D();
            Position3D currentPoint = new Position3D();
            double alpha;
            double beta;
            for (int i = 0; i < points.Count; i++)
            {
                deltaT.Add(t.ElementAt(i) - t0);
                currentPoint = Field2Image(points.ElementAt(i), camId);
                g = Field2ImageVec(new Vector3D(0, 0, -9806.65), camId);
                alpha = (currentPoint.X / currentPoint.Z);
                beta = (currentPoint.Y / currentPoint.Z);
                A[i * 2, 0] = alpha * deltaT[i];
                A[i * 2, 1] = -deltaT[i];
                A[i * 2, 2] = 0;
                A[i * 2 + 1, 0] = beta * deltaT[i];
                A[i * 2 + 1, 1] = 0;
                A[i * 2 + 1, 2] = -deltaT[i];
                B[i * 2, 0] = (0.5 * g.X * deltaT[i] * deltaT[i]) + (CamFirstPos.X) - (0.5 * g.Z * alpha * deltaT[i] * deltaT[i]) - (CamFirstPos.Z * alpha);
                B[i * 2 + 1, 0] = (0.5 * g.Y * deltaT[i] * deltaT[i]) + (CamFirstPos.Y) - (0.5 * g.Z * beta * deltaT[i] * deltaT[i]) - (CamFirstPos.Z * beta);
            }
        }

        public void DeepClearance()
        {
            firstBallPos = new Position2D();
            oppBallOwnerID = null;
            ballIsMove = false;
            oppHasBall = false;
            camID = -1;
            counter = 0;
        }

        public void Clear()
        {
            isotchiInInitialstate = true;
            oppAngle = 0;
            t = new Queue<double>();
            points = new Queue<Position2D>();
            A = new MathMatrix(0, 0);
            B = new MathMatrix(0, 0);
            pos = new Position3D();
            t0 = 0;
            ourCamID = -1;
            oppCamID = -1;
            firstBallFlag = true;
            CamFirstPos = new Position3D();
            pos1 = new Position3D();
            parabolaV = new Vector3D();
            lastParabolaV = new Vector3D();
            deltaT = new List<double>();
            points = new Queue<Position2D>();
            lastBallPos = new Position2D();
            currentBallPos = new Position2D();
            X = new MathMatrix(3, 1);
            firstPos = new Position3D();
            lastPos = new Position3D();
            finalPoint = new Position2D();
            cameras_notset = true;
            ballConflict = false;
            lastcamID = cam1;
            returnLastBallpos = false;
            firstTime = true;
        }

        private void FinalParameters(Position3D position, Vector3D velocity)
        {
            double Ts = 0;
            double T1 = 0;
            double T2 = 0;
            double g = 9.806;
            T1 = ((-velocity.Z) + Math.Sqrt((velocity.Z * velocity.Z) - (4 * 0.5 * g * (-position.Z)))) / g;
            T2 = ((-velocity.Z) - Math.Sqrt((velocity.Z * velocity.Z) - (4 * 0.5 * g * (-position.Z)))) / g;
            if (T1 < T2)
                Ts = T1;
            else
                Ts = T2;
        }

        private void BallIsMoved(SSL_WrapperPacket packet, bool isYellow, Position2D ball, Position2D lastBall, bool isReverse)
        {
            List<messages_robocup_ssl_detection.SSL_DetectionRobot> oppRobots = new List<messages_robocup_ssl_detection.SSL_DetectionRobot>();
            double minDis = double.MaxValue;
            int? robotID = null;
            if (isYellow)
                oppRobots = packet.detection.robots_blue;
            else
                oppRobots = packet.detection.robots_yellow;
            foreach (var item in oppRobots)
            {
                Position2D itemLoc = new Position2D(item.x, item.y);
                if (itemLoc.DistanceFrom(ball) < minDis)
                {
                    minDis = itemLoc.DistanceFrom(ball);
                    robotID = (int)item.robot_id;
                }
            }
            if (!oppHasBall && minDis / 1000 < 0.12)
            {
                oppBallOwnerID = robotID;
                oppHasBall = true;
                camID = (int)packet.detection.camera_id;
            }
            if (!ballIsMove && oppHasBall && lastBall.DistanceFrom(ball) / 1000 > vMin)
            {
                foreach (var item in oppRobots)
                {
                    if (item.robot_id == oppBallOwnerID.Value)
                    {
                        oppAngle = item.orientation;
                    }
                }
                //ballIsMove = true;
            }
            if (oppHasBall)
            {
                foreach (var item in oppRobots)
                {
                    if (item.robot_id == oppBallOwnerID.Value)
                    {
                        oppRobot = new Position2D(-item.x / 1000, item.y / 1000);
                        oppAngle2 = item.orientation;
                    }
                }
                //ballIsMove = true;
            }
            if (oppBallOwnerID.HasValue && ballConflict)
            {
                double angle = 0;
                foreach (var item in oppRobots)
                {
                    if (item.robot_id == oppBallOwnerID.Value)
                    {
                        angle = Math.PI - item.orientation;
                    }
                }
                angle = angle.ToDegree();
                if (!isReverse)
                {
                    //90 to 270 our goali
                    if (angle > 90 && angle < 270)
                    {
                        camID = ourCamID;
                    }
                }
                else
                {
                    //90 to 270 opp goali
                    if (!(angle > 90 && angle < 270))
                    {
                        camID = ourCamID;
                    }
                }
            }
        }

        private double distance(double x1, double y1, double x2, double y2)
        {
            Position2D firstPoint = new Position2D(x1 / 1000, y1 / 1000);
            Position2D secondPoint = new Position2D(x2 / 1000, y2 / 1000);
            return firstPoint.DistanceFrom(secondPoint);
        }

        private bool SimpleChipDitector(SSL_WrapperPacket sslPacket, Position2D currentPos, Position2D firstPos)
        {
            DrawingObjects.AddObject(new StringDraw("counter: " + counter, new Position2D(3, -2)), "5465464878987");
            oppAngle = -oppAngle;
            Vector2D ballVec = new Position2D(-currentPos.X, currentPos.Y) - oppRobot;
            //firstPos - currentPos;
            if (new Position2D(-currentPos.X, currentPos.Y).DistanceFrom(oppRobot) < .125)
            {
                robotvector = Vector2D.FromAngleSize(Math.PI - oppAngle2, 1);
            }
            double dista = new Position2D(-currentPos.X, currentPos.Y).DistanceFrom(new Position2D(-firstBallPos.X / 1000, firstBallPos.Y / 1000));
            bool trueorfalse = false;
            if (counter >= 13)
            {
                if (parabolaV.Z > 1 && isotchiInInitialstate)
                {
                    trueorfalse = true;
                }
            }
            else
                trueorfalse = false;
            if (Math.Abs(Vector2D.AngleBetweenInDegrees(ballVec, robotvector)) < 6 && dista < .07 && dista > .04)
            {
                isotchiInInitialstate = false;
                trueorfalse = false;
            }

            return trueorfalse;
        }

        public void CameraParameters(SSL_WrapperPacket packet, bool isYellow)
        {
            if (packet.detection != null)
            {
                List<messages_robocup_ssl_detection.SSL_DetectionRobot> ourRobot = new List<messages_robocup_ssl_detection.SSL_DetectionRobot>();
                if (isYellow)
                    ourRobot = packet.detection.robots_yellow;
                else
                    ourRobot = packet.detection.robots_blue;
                foreach (var item in ourRobot)
                {
                    if ((int)item.robot_id == ControlParameters.GoalieID)
                        ourCamID = (int)packet.detection.camera_id;
                }
                if (ourCamID == cam1)
                    oppCamID = cam2;
                else
                    oppCamID = cam1;
            }
            if (packet.geometry != null && cameras_notset == true && packet != null)
            {
                if (packet.geometry.calib.Count > 0)
                {
                    for (int i = 0; i < packet.geometry.calib.Count; i++)
                    {
                        camQ[packet.geometry.calib[i].camera_id, 0] = packet.geometry.calib[i].q0;
                        camQ[packet.geometry.calib[i].camera_id, 1] = packet.geometry.calib[i].q1;
                        camQ[packet.geometry.calib[i].camera_id, 2] = packet.geometry.calib[i].q2;
                        camQ[packet.geometry.calib[i].camera_id, 3] = packet.geometry.calib[i].q3;
                        camT[(int)packet.geometry.calib[i].camera_id] = new Vector3D(packet.geometry.calib[i].tx, packet.geometry.calib[i].ty, packet.geometry.calib[i].tz);
                    }
                    cameras_notset = false;
                }
            }
        }
    }
}