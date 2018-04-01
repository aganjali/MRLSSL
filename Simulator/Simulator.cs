using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using StillDesign.PhysX;
using MRL.SSL.CommonClasses;
using StillDesign.PhysX.MathPrimitives;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.IO;
using System.Drawing;
using System.Diagnostics;
namespace Simulator
{
    public class MRLSimulator : SimulatorBase, System.IDisposable
    {
        public RectangleF field = new RectangleF(6.0f, 4.5f, 12.0f, 9.0f);
        private Scene _scene;
        Thread _sendThread, _reciveThread, _simulatorThread;
        private Dictionary<int, Robot> blueRobots;
        private Dictionary<int, Robot> yellowRobots;
        private Dictionary<int, Actor> balls;
        private int _recievePort = 10013;
        private CommunicationController _networkManager;
        private string _aiName = "mrl-ai-pc";
        private int _sendPort = 10013;
        public Actor robotActor { get; set; }
        public Scene Scene
        {
            get { return _scene; }
            set { _scene = value; }
        }


        public string AiName
        {
            get { return _aiName; }
            set { _aiName = value; }
        }

        public int SendPort
        {
            get { return _sendPort; }
            set { _sendPort = value; }
        }

        public int RecievePort
        {
            get { return _recievePort; }
            set { _recievePort = value; }
        }

        public MRLSimulator()
        {
            device = new SlimDX.Direct3D10.Device(SlimDX.Direct3D10.DriverType.Hardware, SlimDX.Direct3D10.DeviceCreationFlags.None);
            _scene = Engine.Scene;
            _simulatorThread = new Thread(new ThreadStart(Run));
            _simulatorThread.Start();
            Material defultMaterial = _scene.Materials[0];
            defultMaterial.DynamicFriction = 0.75f;
            defultMaterial.StaticFriction = 0.5f;
            /*defultMaterial.Restitution = 0.75f;*/
            balls = new Dictionary<int, Actor>();
            addBall(0, Position2D.Zero);
            //engine.Run();
        }
        private List<messages_robocup_ssl_detection.SSL_DetectionFrame> GenerateCameraModel(Camera[] cams)
        {
            messages_robocup_ssl_detection.SSL_DetectionFrame[] packet = new messages_robocup_ssl_detection.SSL_DetectionFrame[StaticVariables.CameraCount];
            for (int i = 0; i < StaticVariables.CameraCount; i++)
            {
                packet[i] = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            }
            foreach (var item in cams)
            {
                if (item == null)
                {
                    return null;
                }
            }
            if (yellowRobots != null)
            {
                foreach (var item in yellowRobots.Keys.ToList())
                {
                    foreach (var c in cams)
                    {
                        if (c.IsInCamera(new Vector3(yellowRobots[item].RobotActor.GlobalPosition.X, yellowRobots[item].RobotActor.GlobalPosition.Z, yellowRobots[item].RobotActor.GlobalPosition.Y)))
                        {
                            packet[c.ID].robots_yellow.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = yellowRobots[item].RobotActor.GlobalPosition.X * 1000,
                                y = yellowRobots[item].RobotActor.GlobalPosition.Z * 1000,
                                robot_id = yellowRobots[item].ID,
                                orientation = (float)(Math.PI * ((GetAngle(yellowRobots[item].RobotActor.GlobalOrientationQuat) + 90) / 180.0))
                            });
                            break;
                        }
                    }
                }
            }
            if (blueRobots != null)
            {
                foreach (var item in blueRobots.Keys.ToList())
                {
                    foreach (var c in cams)
                    {
                        if (c.IsInCamera(new Vector3(blueRobots[item].RobotActor.GlobalPosition.X, blueRobots[item].RobotActor.GlobalPosition.Z, blueRobots[item].RobotActor.GlobalPosition.Y)))
                        {
                            packet[c.ID].robots_blue.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = blueRobots[item].RobotActor.GlobalPosition.X * 1000,
                                y = blueRobots[item].RobotActor.GlobalPosition.Z * 1000,
                                robot_id = blueRobots[item].ID,
                                orientation = (float)(Math.PI * ((GetAngle(blueRobots[item].RobotActor.GlobalOrientationQuat) + 90) / 180.0))
                            });
                            break;
                        }
                    }
                    
                }
            }
            if (balls != null)
            {
                foreach (int item in balls.Keys)
                {
                    foreach (var c in cams)
                    {
                        if (c.IsInCamera(new Vector3(balls[item].GlobalPosition.X, balls[item].GlobalPosition.Z, balls[item].GlobalPosition.Y)))
                        {
                            packet[c.ID].balls.Add(new messages_robocup_ssl_detection.SSL_DetectionBall()
                            {
                                confidence = 1,
                                x = c.CalculateObjectPosition(new Vector3(balls[item].GlobalPosition.X, balls[item].GlobalPosition.Z, balls[item].GlobalPosition.Y)).X,
                                y = c.CalculateObjectPosition(new Vector3(balls[item].GlobalPosition.X, balls[item].GlobalPosition.Z, balls[item].GlobalPosition.Y)).Z,
                                z = c.CalculateObjectPosition(new Vector3(balls[item].GlobalPosition.X, balls[item].GlobalPosition.Z, balls[item].GlobalPosition.Y)).Y
                            });
                        }
                    }
                }
            }
            return packet.ToList();

        }
        private float GetAngle(Quaternion q1)
        {
            ///** assumes q1 is a normalised quaternion */
            double test = q1.X * q1.Y + q1.Z * q1.W;
            double heading = 0, attitude = 0, bank = 0;
            if (test > 0.499)
            { // singularity at north pole
                heading = 2 * Math.Atan2(q1.W, q1.W);
                attitude = Math.PI / 2;
                bank = 0;
                return (float)heading;
            } if (test < -0.499)
            { // singularity at south pole
                heading = -2 * Math.Atan2(q1.X, q1.W);
                attitude = -Math.PI / 2;
                bank = 0;
                return (float)heading;
            } double sqx = q1.X * q1.X; double sqy = q1.Y * q1.Y; double sqz = q1.Z * q1.Z;
            heading = Math.Atan2(2 * q1.Y * q1.W - 2 * q1.X * q1.Z, 1 - 2 * sqy - 2 * sqz);
            attitude = Math.Asin(2 * test);

            heading *= 180 / Math.PI;
            bank *= 180 / Math.PI;
            attitude *= 180 / Math.PI;

            return (float)heading;
        }

        private Actor CreateBall(Vector3 position)
        {
            ActorDescription AC = new ActorDescription();
            BodyDescription BD = new BodyDescription(0.043f);
            SphereShapeDescription spd = new SphereShapeDescription(0.043f / 2);
            AC.Shapes.Add(spd);
            AC.BodyDescription = BD;
            AC.GlobalPose = Matrix.Translation(position);
            Material ballMaterial = _scene.CreateMaterial(new MaterialDescription() { DynamicFriction = 0.25f, /*StaticFriction = 0.25f, Restitution = 0.75f */});
            Actor ball = _scene.CreateActor(AC);
            return ball;
        }
        public void ConnectToAi()
        {
            _networkManager = new CommunicationController("any", _recievePort, _aiName, _sendPort);
            _sendThread = new Thread(new ThreadStart(SendDataRun));
            _reciveThread = new Thread(new ThreadStart(ReciveDataRun));
            _reciveThread.Start();
            _sendThread.Start();
        }
        void SendDataRun()
        {
            double timeCapture = 0;
            
            Camera[] cams = new Camera[StaticVariables.CameraCount];
            
            float widthStep = field.Width * 2.0f / StaticVariables.CameraCount;
            float heightStep = field.Height / 2.0f;
            
            for (int c = 0; c < StaticVariables.CameraCount; c++)
            {
                int i = c % 2, j = c / 2;
                float cx = field.Width - (j * widthStep + widthStep / 2.0f), cy =  field.Height - (i * heightStep + heightStep / 2.0f);
                cams[i] = new Camera(cx, cy, 4.0f, new RectangleF(cx + widthStep / 2 + 0.5f, cy + heightStep / 2 + 0.5f, widthStep + 1.0f, heightStep + 1.0f), c);
            }
            Stopwatch sw = new Stopwatch();
            //    HiPerfTimer t2 = new HiPerfTimer();

            long elapsed = 0;

            while (true)
            {
                elapsed = 0;

                sw.Restart();
                List<messages_robocup_ssl_detection.SSL_DetectionFrame> lists = GenerateCameraModel(cams);
                messages_robocup_ssl_wrapper.SSL_WrapperPacket wrapper = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
                MemoryStream stream = new MemoryStream();
                wrapper.detection = new messages_robocup_ssl_detection.SSL_DetectionFrame();
                
                for (int i = 0; i < cams.Length; i++)
			    {
                    elapsed = sw.ElapsedMilliseconds;
                    if (i == 0)
	                    elapsed = 0;
			        wrapper.detection = lists[i];
                    wrapper.detection.t_capture = timeCapture + elapsed;
                    wrapper.detection.camera_id = (uint)i;

                    stream = new MemoryStream();
                    ProtoBuf.Serializer.Serialize<messages_robocup_ssl_wrapper.SSL_WrapperPacket>(stream, wrapper);
                    _networkManager.SendData(stream);
                }
                


                while ((elapsed = sw.ElapsedMilliseconds) < 16) ;

                timeCapture += elapsed;

                if (elapsed > 20)
                {

                }
            }
        }

        void ReciveDataRun()
        {
            GoogleSerializer deserilizer = new GoogleSerializer();
            while (true)
            {
                MemoryStream reciveStream = _networkManager.RecieveData();
                SimulatorParameters simParams = new SimulatorParameters();
                deserilizer.stream = reciveStream;
                simParams = deserilizer.DeserilializeSimParameters();
                if (simParams != null)
                {
                    ApplyForces(simParams);
                }

            }
        }

        Dictionary<int, bool> blueRunning = new Dictionary<int, bool>();
        Dictionary<int, bool> blueUserMode = new Dictionary<int, bool>();

        Dictionary<int, bool> yellowRunning = new Dictionary<int, bool>();
        Dictionary<int, bool> yellowUserMode = new Dictionary<int, bool>();

        Dictionary<int, bool> ballRunning = new Dictionary<int, bool>();
        Dictionary<int, bool> ballUserMode = new Dictionary<int, bool>();

        public void addRobot(int id, Color color, Position2D position, double angle)
        {
            if (color == Color.Blue)
            {
                if (blueRobots == null)
                    blueRobots = new Dictionary<int, Robot>();
                if (blueRunning == null)
                    blueRunning = new Dictionary<int, bool>();
                if (blueUserMode == null)
                    blueUserMode = new Dictionary<int, bool>();
                robotActor = ConvexLoader.LoadConvexMesh("Robot.DAE", _scene, device);
                robotActor.GlobalPose = Matrix.Translation(new Vector3((float)-position.X, 0, (float)position.Y));
                if (!blueRobots.ContainsKey(id))
                {
                    blueRunning.Add(id, false);
                    blueRobots.Add(id, new Robot(this, (uint)id, mass, color, robotActor));
                    blueUserMode.Add(id, false);
                }
            }
            else
            {
                if (yellowRobots == null)
                    yellowRobots = new Dictionary<int, Robot>();
                if (yellowRunning == null)
                    yellowRunning = new Dictionary<int, bool>();
                if (yellowUserMode == null)
                    yellowUserMode = new Dictionary<int, bool>();
                robotActor = ConvexLoader.LoadConvexMesh("Robot.DAE", _scene, device);
                robotActor.GlobalPose = Matrix.Translation(new Vector3((float)-position.X, 0, (float)position.Y));
                if (!yellowRobots.ContainsKey(id))
                {
                    yellowRunning.Add(id, false);
                    yellowRobots.Add(id, new Robot(this, (uint)id, mass, color, robotActor));
                    yellowUserMode.Add(id, false);
                }
            }
        }

        public void addBall(int id, Position2D position)
        {
            if (balls == null)
                balls = new Dictionary<int, Actor>();
            if (ballRunning == null)
                ballRunning = new Dictionary<int, bool>();
            if (ballUserMode == null)
                ballUserMode = new Dictionary<int, bool>();

            if (!balls.ContainsKey(id))
            {
                ballUserMode.Add(id, false);
                ballRunning.Add(id, false);
                balls.Add(id, CreateBall(new Vector3((float)position.X, 0, (float)position.Y)));
            }
        }

        public void setBallPosition(int id, Position2D position)
        {
            if (balls != null && balls.ContainsKey(id))
            {
                while (ballRunning[id])
                {
                    ballUserMode[id] = true;
                }
                ballUserMode[id] = true;
                balls[id].GlobalPose = Matrix.Translation(new Vector3((float)-position.X, 0, (float)position.Y));
                ballUserMode[id] = false;
            }
        }

        public void setBallSpeed(int id, Vector2D speed)
        {
            if (balls != null && balls.ContainsKey(id))
            {
                while (ballRunning[id])
                {
                    ballUserMode[id] = true;
                }
                ballUserMode[id] = true;
                balls[id].LinearVelocity = new Vector3((float)-speed.X, 0, (float)speed.Y);
                ballUserMode[id] = false;
            }
        }


        public void removeRobot(int id, Color color)
        {
            if (color == Color.Blue)
            {
                if (blueRobots != null && blueRobots.ContainsKey(id))
                {
                    while (blueRunning[id])
                    {
                        blueUserMode[id] = true;
                    }
                    blueUserMode[id] = true;
                    blueRobots.Remove(id);
                }
            }
            else
            {
                {
                    if (yellowRobots != null && yellowRobots.ContainsKey(id))
                    {
                        while (yellowRunning[id])
                        {
                            yellowUserMode[id] = true;
                        }
                        yellowUserMode[id] = true;
                        yellowRobots.Remove(id);
                    }
                }
            }
        }
        public void setRobotPosition(int id, Color color, Position2D position, float Angle)
        {
            if (color == Color.Blue)
            {
                if (blueRobots != null && blueRobots.ContainsKey(id))
                {
                    while (blueRunning[id])
                    {
                        blueUserMode[id] = true;
                    }
                    blueUserMode[id] = true;
                    blueRobots[id].SetRobotPosition(new Vector3((float)-position.X, 0, (float)position.Y), Angle - 90);
                    blueUserMode[id] = false;
                }
            }
            else
            {
                if (yellowRobots != null && yellowRobots.ContainsKey(id))
                {
                    while (yellowRunning[id])
                    {
                        yellowUserMode[id] = true;
                    }
                    yellowUserMode[id] = true;
                    yellowRobots[id].SetRobotPosition(new Vector3((float)-position.X, 0, (float)position.Y), Angle);
                    yellowUserMode[id] = false;
                }
            }
        }
        private void ApplyForces(SimulatorParameters simParams)
        {
            if (blueRobots != null)
                foreach (int item in blueRobots.Keys)
                {
                    var swc = simParams.Commands.Where(c => c.Key == item && c.Value.Color.ToArgb() == Color.Blue.ToArgb()).Select(c => c.Value).FirstOrDefault();
                    if (swc != null && !blueUserMode[item])
                    {
                        blueRunning[item] = true;
                        //blueRobots[item].SetMotorSpeed(new Vector3((float)swc.Vx, (float)0, (float)swc.Vy), (float)swc.W);
                        blueRobots[item].SetRobotSpeed(new Vector3((float)swc.Vx, (float)0, (float)swc.Vy), (float)swc.W);
                        kick(item, swc.Color, swc.KickPower * 0.25f, (swc.isChipKick));
                        blueRunning[item] = false;
                    }
                }

            if (yellowRobots != null)
                foreach (int item in yellowRobots.Keys)
                {
                    var swc = simParams.Commands.Where(c => c.Key == item && c.Value.Color.ToArgb() == Color.Yellow.ToArgb()).Select(c => c.Value).FirstOrDefault();
                    if (swc != null && !yellowUserMode[item])
                    {
                        yellowRunning[item] = true;
                        // yellowRobots[item].SetMotorSpeed(new Vector3((float)swc.Vx, (float)0, (float)swc.Vy), (float)swc.W);
                        yellowRobots[item].SetRobotSpeed(new Vector3((float)swc.Vx, (float)0, (float)swc.Vy), (float)swc.W);
                        kick(item, swc.Color, swc.KickPower * 0.25f, (swc.isChipKick));
                        yellowRunning[item] = false;
                    }
                }
        }

        private void kick(int id, Color color, double kickPower, bool isChip)
        {
            Position2D robotops = new Position2D();
            if (color.ToArgb() == Color.Blue.ToArgb())
                robotops = new Position2D(blueRobots[id].RobotActor.GlobalPosition.X, blueRobots[id].RobotActor.GlobalPosition.Z);
            else
                robotops = new Position2D(yellowRobots[id].RobotActor.GlobalPosition.X, yellowRobots[id].RobotActor.GlobalPosition.Z);
            double angr = 0;
            if (color.ToArgb() == Color.Blue.ToArgb())
                angr = GetAngle(blueRobots[id].RobotActor.GlobalOrientationQuat) + 90;
            else
                angr = GetAngle(yellowRobots[id].RobotActor.GlobalOrientationQuat) + 90;

            if (angr < -180)
                angr += 360;
            if (angr > 180)
                angr -= 360;
            if (balls != null)
            {
                foreach (int item in balls.Keys)
                {
                    Position2D ball = new Position2D(balls[item].GlobalPosition.X, balls[item].GlobalPosition.Z);
                    double dist = ball.DistanceFrom(robotops);
                    Vector2D vec = new Vector2D((ball.X - robotops.X), (ball.Y - robotops.Y));
                    double ang = vec.AngleInDegrees;
                    double angDif = ang - angr;
                    if (angDif < -180)
                        angDif += 360;
                    if (angDif > 180)
                        angDif -= 360;
                    if (Math.Abs(angDif) < 20 && dist < 0.15 && balls[item].GlobalPosition.Y < 0.05f)
                    {
                        Vector2D v = Vector2D.FromAngleSize(angr * Math.PI / 180, kickPower / 10);
                        if (!ballUserMode[item])
                        {
                            ballRunning[item] = true;
                            if (isChip)
                                balls[item].LinearVelocity = new Vector3((float)v.X, (float)(kickPower / 40), (float)v.Y);
                            else
                                balls[item].LinearVelocity = new Vector3((float)v.X, 0, (float)v.Y);
                            ballRunning[item] = false;
                        }
                    }
                }
            }
        }
        private void CreatBox(Vector3 Size, Vector3 Location)
        {
            BoxShapeDescription boxShapeDesc = new BoxShapeDescription(Size);
            ActorDescription actorDesc = new ActorDescription();
            actorDesc.Name = String.Format("Box {0}", Location.X);
            actorDesc.BodyDescription = new BodyDescription(10.0f);
            actorDesc.GlobalPose = Matrix.Translation(Location.X, Size.Y / 2, Location.Z) * Matrix.RotationY((float)(Math.PI / 2));
            actorDesc.Shapes.Add(boxShapeDesc);

            Actor actor = _scene.CreateActor(actorDesc);
            actor.BodyFlags.Kinematic = true;
        }
        private void InitField()
        {

        }

        public void Dispose()
        {
            _networkManager.Dispose();
            _sendThread.Abort();
            _reciveThread.Abort();
            Shutdown();
            _simulatorThread.Abort();
        }



        public float mass { get; set; }

        public ActorDescription actorDes { get; set; }

        public SlimDX.Direct3D10.Device device { get; set; }


        protected override void Update(TimeSpan elapsed)
        {
        }
    }
    public enum ModelType
    {
        WorldModel,
        CameraModel
    }
}
