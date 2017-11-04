using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class SaveModelData
    {
        private int RobotID = 0;
        // private string  path = @"d:\visiondata.txt";
        private bool _getData = false;
        private bool _save = false;
        private messages_robocup_ssl_wrapper.SSL_WrapperPacket tmpPacket1 = new messages_robocup_ssl_wrapper.SSL_WrapperPacket(), tmpPacket2 = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
        private messages_robocup_ssl_detection.SSL_DetectionRobot tmpRobot = new messages_robocup_ssl_detection.SSL_DetectionRobot();
        private messages_robocup_ssl_detection.SSL_DetectionBall tmpBall = new messages_robocup_ssl_detection.SSL_DetectionBall();
        private SingleObjectState tmpObjectState = new SingleObjectState();
        private WorldModel tmpModel = new WorldModel();
        private RobotCommands tmpCmd = new RobotCommands();

        private SingleWirelessCommand tmpSWC = new SingleWirelessCommand();
        private bool getBallData = false;
        private bool isYellow = false;
        public bool SaveData
        {
            get { return _save; }
            set { _save = value; }
        }
        public SaveModelData(bool getBall)
        {
            getBallData = getBall;
            tmpPacket1.detection = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            tmpPacket2.detection = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            tmpBall.x = 10000;
            tmpBall.x = 10000;
            tmpPacket1.detection.balls.Add(tmpBall);
            tmpPacket2.detection.balls.Add(tmpBall);
            tmpPacket2.detection.camera_id = 1;
            tmpObjectState.Location.X = 10000;
            tmpObjectState.Location.Y = 10000;
            tmpObjectState.Speed.X = 10000;
            tmpObjectState.Speed.Y = 10000;
            tmpModel.BallState = new SingleObjectState();
            tmpModel.BallState = tmpObjectState.Copy();
            tmpModel.TimeElapsed = new TimeSpan();
        }
        public SaveModelData(int robotID, bool isyellow)
        {
            RobotID = robotID;
            isYellow = isyellow;
            tmpPacket1.detection = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            tmpPacket2.detection = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            tmpRobot.x = 10000;
            tmpRobot.y = 10000;
            tmpRobot.orientation = 10000;
            tmpRobot.robot_id = (uint)robotID;
            tmpPacket1.detection.robots_yellow.Add(tmpRobot);
            tmpPacket2.detection.robots_yellow.Add(tmpRobot);
            tmpPacket1.detection.robots_blue.Add(tmpRobot);
            tmpPacket2.detection.robots_blue.Add(tmpRobot);
            tmpPacket2.detection.camera_id = 1;
            tmpObjectState.Location.X = 10000;
            tmpObjectState.Location.Y = 10000;
            tmpObjectState.Angle = 10000;
            tmpObjectState.AngularSpeed = 10000;
            tmpObjectState.Speed.X = 10000;
            tmpObjectState.Speed.Y = 10000;
            tmpObjectState.Acceleration.X = 10000;
            tmpObjectState.Acceleration.Y = 10000;
            tmpModel.OurRobots = new Dictionary<int, SingleObjectState>();
            tmpModel.OurRobots.Add(RobotID, tmpObjectState);
            tmpModel.TimeElapsed = new TimeSpan();
            tmpModel.FirstBallCatchingPoint = new Position2D(10000,10000);
            tmpSWC.Vx = 10000;
            tmpSWC.Vy = 10000;
            tmpSWC.W = 10000;
            tmpCmd.Commands = new Dictionary<int, SingleWirelessCommand>();
            tmpCmd.Commands.Add(RobotID, tmpSWC);
        }
        public bool GetData
        {
            get { return _getData; }
            set { _getData = value; }
        }
        private Vector2D _alfa;

        public Vector2D Alfa
        {
            get { return _alfa; }
            set { _alfa = value; }
        }
        private Position2D _target;
        public Position2D Target
        {
            get { return _target; }
            set { _target = value; }
        }

        private messages_robocup_ssl_wrapper.SSL_WrapperPacket _sslpacket;

        public messages_robocup_ssl_wrapper.SSL_WrapperPacket SSLPacket
        {
            get { return _sslpacket; }
            set { _sslpacket = value; }
        }
        private WorldModel _model;
        public WorldModel Model
        {
            get { return _model; }
            set { _model = value; }
        }
        private RobotCommands _command;

        public RobotCommands Command
        {
            get { return _command; }
            set { _command = value; }
        }
        private List<messages_robocup_ssl_wrapper.SSL_WrapperPacket> _packetList = new List<messages_robocup_ssl_wrapper.SSL_WrapperPacket>();
        private List<WorldModel> _modelList = new List<WorldModel>();
        List<Position2D> _TargetList = new List<Position2D>();
        List<Vector2D> _AlfaList = new List<Vector2D>();
        public List<WorldModel> ModelList
        {
            get { return _modelList; }
            set { _modelList = value; }
        }
        private List<RobotCommands> _commandList = new List<RobotCommands>();

        public List<RobotCommands> CommandList
        {
            get { return _commandList; }
            set { _commandList = value; }
        }
        public List<messages_robocup_ssl_wrapper.SSL_WrapperPacket> PacketList
        {
            get { return _packetList; }
            set { _packetList = value; }
        }
        public List<Position2D> PositionsList = new List<Position2D>();
        int Counter = 0;
        public void Add()
        {

            if (!getBallData)
            {
                if (_sslpacket != null && _sslpacket.detection != null)
                {

                    if (isYellow && _sslpacket.detection.robots_yellow.Any(a => a.robot_id == RobotID))
                        _packetList.Add((messages_robocup_ssl_wrapper.SSL_WrapperPacket)_sslpacket.Clone());
                    else if (!isYellow && _sslpacket.detection.robots_blue.Any(a => a.robot_id == RobotID))
                        _packetList.Add((messages_robocup_ssl_wrapper.SSL_WrapperPacket)_sslpacket.Clone());
                    else if (_sslpacket.detection.camera_id == 0)
                        _packetList.Add(tmpPacket1);
                    else
                        _packetList.Add(tmpPacket2);
                }
                else
                    _packetList.Add(tmpPacket1);
                Counter++;
                if (_model != null && _model.OurRobots.Any(a => a.Key == RobotID))
                    _modelList.Add(new WorldModel(_model));
                else
                    _modelList.Add(tmpModel);

                if (_command != null && _command.Commands.Any(a => a.Key == RobotID))
                    _commandList.Add((RobotCommands)_command.Clone());
                else
                    _commandList.Add(tmpCmd);
                if (_model != null && _model.FirstBallCatchingPoint.HasValue)
                    PositionsList.Add(_model.FirstBallCatchingPoint.Value);
                else
                    PositionsList.Add(new Position2D(10000, 10000));
                _TargetList.Add(_target);
                _AlfaList.Add(_alfa);
            }
            else
            {
                if (_sslpacket != null && _sslpacket.detection != null)
                {
                    if (_sslpacket.detection.balls != null && _sslpacket.detection.balls.Count != 0)
                        _packetList.Add((messages_robocup_ssl_wrapper.SSL_WrapperPacket)_sslpacket.Clone());
                    else if (_sslpacket.detection.camera_id == 0)
                        _packetList.Add(tmpPacket1);
                    else
                        _packetList.Add(tmpPacket2);
                }
                else
                    _packetList.Add(tmpPacket1);
                if (_model != null && _model.BallState != null)
                    _modelList.Add(new WorldModel(_model));
                else
                    _modelList.Add(tmpModel);

            }
        }
        public void Clear()
        {
            _packetList.Clear();
            _modelList.Clear();
            _commandList.Clear();
            PositionsList.Clear();
            _TargetList.Clear();
            _AlfaList.Clear();
            Counter = 0;
        }
        public void Save()
        {
            if (!getBallData)
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write("X\tY\tO\tTc\tTs\tID\t\n");
                foreach (var item in _packetList)
                {
                    messages_robocup_ssl_detection.SSL_DetectionRobot robot;
                    if (isYellow)
                        robot = item.detection.robots_yellow.Single(s => s.robot_id == RobotID);
                    else
                        robot = item.detection.robots_blue.Single(s => s.robot_id == RobotID);
                    sw.Write(robot.x.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.orientation.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_capture.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_sent.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.camera_id.ToString() + "\t\n");
                    sw.Flush();
                }
                FileStream fs = new FileStream(@"d:\NewRobotRawdata.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("X\tY\tO\tVx\tVy\tW\tAx\tAy\tT\tN\t\n");
                foreach (var item in _modelList)
                {
                    SingleObjectState robot = item.OurRobots.Single(s => s.Key == RobotID).Value;
                    double Vx = robot.Speed.Y * Math.Cos(robot.Angle.Value * Math.PI / 180) - robot.Speed.X * Math.Sin(robot.Angle.Value * Math.PI / 180);
                    double Vy = robot.Speed.X * Math.Cos(robot.Angle.Value * Math.PI / 180) + robot.Speed.Y * Math.Sin(robot.Angle.Value * Math.PI / 180);

                    sw.Write(robot.Location.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.Location.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.Angle.ToString() + "\t");
                    sw.Flush();
                    sw.Write(Vx.ToString() + "\t");
                    sw.Flush();
                    sw.Write(Vy.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.AngularSpeed.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.OurRobots.Single(s => s.Key == RobotID).Value.Acceleration.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.OurRobots.Single(s => s.Key == RobotID).Value.Acceleration.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.TimeElapsed.TotalMilliseconds.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\NewRobotFilterData.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("X\tY\tT\t\n");
                for(int i = 0; i < PositionsList.Count; i++)
                {
                    sw.Write(PositionsList[i].X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(PositionsList[i].Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_modelList[i].TimeElapsed.TotalMilliseconds.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\FirstBallCatchingPoint.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("CvX\tCvY\tCw\n");
                foreach (var item in _commandList)
                {
                    SingleWirelessCommand robotcmd = item.Commands.Single(s => s.Key == RobotID).Value;
                    sw.Write(robotcmd.Vx.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robotcmd.Vy.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robotcmd.W.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\NewRobotCommandData.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("TarX\tTarY\tAlfaX\tAlfaY\n");
                
                for (int i = 0; i < _TargetList.Count; i++)
                {
                    sw.Write(_TargetList[i].X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_TargetList[i].Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_AlfaList[i].X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_AlfaList[i].Y.ToString() + "\n");
                    sw.Flush();
                }

                fs = new FileStream(@"d:\TargetAndAlfa.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();


            }
            else
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write("X\tY\tTc\tTs\tID\t\n");
                foreach (var item in _packetList)
                {

                    sw.Write(item.detection.balls[0].x.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.balls[0].y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_capture.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_sent.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.camera_id.ToString() + "\t\n");
                    sw.Flush();
                }
                FileStream fs = new FileStream(@"d:\Ballrawdata.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("X\tY\tVx\tVy\tAx\tAy\tT\t\n");
                foreach (var item in _modelList)
                {
                    sw.Write(item.BallState.Location.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Location.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Speed.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Speed.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Acceleration.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Acceleration.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.TimeElapsed.TotalMilliseconds.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\Ballfilterddata.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();
               
            }
            Clear();
        }
        public void Save(string filename)
        {
            if (!getBallData)
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write("X\tY\tO\tTc\tTs\tID\t\n");
                foreach (var item in _packetList)
                {
                    messages_robocup_ssl_detection.SSL_DetectionRobot robot;
                    if (isYellow)
                        robot = item.detection.robots_yellow.Where(s => s.robot_id == RobotID).First();
                    else
                        robot = item.detection.robots_blue.Where(s => s.robot_id == RobotID).First();
                    sw.Write(robot.x.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.orientation.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_capture.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_sent.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.camera_id.ToString() + "\t\n");
                    sw.Flush();
                }
                FileStream fs = new FileStream(@"d:\NewRobotRawdata" + filename + ".txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("X\tY\tO\tVx\tVy\tW\tAx\tAy\tT\tN\t\n");
                foreach (var item in _modelList)
                {
                    SingleObjectState robot = item.OurRobots.Single(s => s.Key == RobotID).Value;
                    double Vx = robot.Speed.Y * Math.Cos(robot.Angle.Value * Math.PI / 180) - robot.Speed.X * Math.Sin(robot.Angle.Value * Math.PI / 180);
                    double Vy = robot.Speed.X * Math.Cos(robot.Angle.Value * Math.PI / 180) + robot.Speed.Y * Math.Sin(robot.Angle.Value * Math.PI / 180);

                    sw.Write(robot.Location.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.Location.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.Angle.ToString() + "\t");
                    sw.Flush();
                    sw.Write(Vx.ToString() + "\t");
                    sw.Flush();
                    sw.Write(Vy.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robot.AngularSpeed.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.OurRobots.Single(s => s.Key == RobotID).Value.Acceleration.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.OurRobots.Single(s => s.Key == RobotID).Value.Acceleration.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.TimeElapsed.TotalMilliseconds.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\FilterData" + filename + ".txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("X\tY\tT\t\n");
                for (int i = 0; i < PositionsList.Count; i++)
                {
                    sw.Write(PositionsList[i].X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(PositionsList[i].Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_modelList[i].TimeElapsed.TotalMilliseconds.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\FirstBallCatchingPoint" + filename + ".txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("CvX\tCvY\tCw\n");
                foreach (var item in _commandList)
                {
                    SingleWirelessCommand robotcmd = item.Commands.Single(s => s.Key == RobotID).Value;
                    sw.Write(robotcmd.Vx.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robotcmd.Vy.ToString() + "\t");
                    sw.Flush();
                    sw.Write(robotcmd.W.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\NewRobotCommandData" + filename + ".txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("TarX\tTarY\tAlfaX\tAlfaY\n");

                for (int i = 0; i < _TargetList.Count; i++)
                {
                    sw.Write(_TargetList[i].X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_TargetList[i].Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_AlfaList[i].X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(_AlfaList[i].Y.ToString() + "\n");
                    sw.Flush();
                }

                fs = new FileStream(@"d:\TargetAndAlfa"+filename +".txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

            }
            else
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write("X\tY\tTc\tTs\tID\t\n");
                foreach (var item in _packetList)
                {

                    sw.Write(item.detection.balls[0].x.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.balls[0].y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_capture.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.t_sent.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.detection.camera_id.ToString() + "\t\n");
                    sw.Flush();
                }
                FileStream fs = new FileStream(@"d:\Ballrawdata" + filename + ".txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

                ms = new MemoryStream();
                sw = new StreamWriter(ms);
                sw.Write("X\tY\tVx\tVy\tAx\tAy\tT\t\n");
                foreach (var item in _modelList)
                {
                    sw.Write(item.BallState.Location.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Location.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Speed.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Speed.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Acceleration.X.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.BallState.Acceleration.Y.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.TimeElapsed.TotalMilliseconds.ToString() + "\t\n");
                    sw.Flush();
                }
                fs = new FileStream(@"d:\Ballfilterddata" + filename + ".txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();

            }
            Clear();
        }

    }
}
