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
    public class mRobot
    {
        public mRobot()
        {

        }

        public mRobot(double confidence, int robotID, double time, double oriention, double x, double y, int camID)
        {
            Confidence = confidence;
            RobotID = robotID;
            Oriention = oriention;
            Pos = new Position2D(x, y);
            CamID = camID;
            Time = time;
        }

        public mRobot(float confidence, uint robotID, double time, float oriention, float x, float y, uint camID)
        {
            Confidence = (double)confidence;
            RobotID = (int)robotID;
            Oriention = (double)oriention;
            Pos = new Position2D((double)x, (double)y);
            CamID = (int)camID;
            Time = time;
        }
        public double Confidence { get; set; }
        public int RobotID { get; set; }
        public double Oriention { get; set; }
        public Position2D Pos { get; set; }
        public int CamID { get; set; }
        public double Time { get; set; }
    }

    public class mBall
    {
        public mBall(float confidence, float time, float x, float y, uint camID)
        {
            Confidence = (double)confidence;
            Pos = new Position2D((double)x, (double)y);
            CamID = (int)camID;
            Time = (double)time;
        }

        public mBall(double confidence, double time, double x, double y, int camID)
        {
            Confidence = confidence;
            Pos = new Position2D(x, y);
            CamID = camID;
            Time = time;
        }
        public double Confidence { get; set; }
        public Position2D Pos { get; set; }
        public int CamID { get; set; }
        public double Time { get; set; }
    }

    public struct BallState
    {
        public PointF Position;
        public int camviewed;
        public double confidence;
        public BallState(PointF Pos, int cam, double conf)
        {
            Position = Pos;
            camviewed = cam;
            confidence = conf;
        }
    }
    public class vraw
    {

        public vraw(double t, Position2D p, float ang, float _conf, uint cam)
        {
            timestamp = t;
            pos = p;
            angle = ang;
            conf = _conf;
            notSeen = -1;
            camera = cam;
        }
        public vraw(vraw From)
        {
            this.angle = From.angle;
            this.camera = From.camera;
            this.conf = From.conf;
            this.lastCamViewd = From.lastCamViewd;
            this.notSeen = From.notSeen;
            this.pos = From.pos;
            this.timestamp = From.timestamp;
            this.sinTheta = From.sinTheta;
            this.cosTheta = From.cosTheta;
        }
        public vraw()
        { }

        public double timestamp = 0;
        public Position2D pos = new Position2D();
        public float angle = 0;
        public float sinTheta = 0;
        public float cosTheta = 0;
        public float conf = 0;
        public int notSeen = -1;
        public uint camera = 0;
        public int lastCamViewd = 0;
    }
    public static class util
    {
        public static double M_2PI = 2 * Math.PI;
        public static double anglemod(double a)
        {
            a -= M_2PI * (int)Math.Round(a / M_2PI);
            return (a);
        }
        public static double bound(double x, double low, double high)
        {
            if (x < low) x = low;
            if (x > high) x = high;
            return (x);
        }
    }
    public struct Vector3D
    {
        double x, y, z;
        public Vector3D(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double X
        {
            get { return x; }
            set { x = value; }
        }
    }
    public class vrobot:ICloneable
    {
        public vrobot()
        { }
        public vrobot(vraw r, SingleObjectState s)
        {
            vision = r;
            state = s;
            viewstate = new SingleObjectState();
        }

        public SingleObjectState state = new SingleObjectState();
        public SingleObjectState viewstate = new SingleObjectState();
        public vraw vision = new vraw();
        public bool visionProblem = false;
        public vraw AI = new vraw();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class vball
    {
        public vball()
        { }
        public vball(int var)
        {
            state = new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0);
            viewstate = new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0);
            vision = new vraw();
            occluding_offset = new Vector2D(); ;
            occluded = OccludeFlag.Visible;
            occluding_robot = 0;
            occluding_team = 0;
        }
        public vball(vraw r, SingleObjectState s)
        {
            state = s;
            viewstate = new SingleObjectState();
            vision = r;
            occluding_offset = new Vector2D(); ;
            occluded = OccludeFlag.Visible;
            occluding_robot = 0;
            occluding_team = 0;
        }
        public SingleObjectState state = new SingleObjectState();
        public SingleObjectState viewstate = new SingleObjectState();
        public RectangularMatrix variances;
        public bool colision = false;
        public vraw vision = new vraw();
        public Vector2D occluding_offset = new Vector2D();
        public OccludeFlag occluded = OccludeFlag.Visible;
        public int occluding_team = 0, occluding_robot = 0;
    }
    public struct vconfig_robot
    {
        public vconfig_robot(int Id, RobotType Type)
        {
            id = Id;
            type = Type;
        }
        public int id;             // The robot's id
        public RobotType type;           // The robot type (e.g. omni or diff)
    }; // 2
    public struct vconfig_team
    {
        public int cover_type;
        public vconfig_robot[] robots;// = new vconfig_robot[StaticVariables.MAX_TEAM_ROBOTS];
    } // 1+5*2 = 11
    public struct vconfig_ball
    {
        public int id;// = new vconfig_robot[StaticVariables.MAX_TEAM_ROBOTS];
    }
    public struct net_vconfig
    {
        public net_vconfig(int teamNo, int robotNo, int BallNo)
        {
            msgtype = 0;
            teams = new vconfig_team[teamNo];
            for (int i = 0; i < teamNo; i++)
            {
                teams[i].robots = new vconfig_robot[robotNo];
                for (int j = 0; j < robotNo; j++)
                {
                    teams[i].robots[j].id = -1;
                    teams[i].robots[j].type = RobotType.Default;
                }
            }
            balls = new vconfig_ball[BallNo];
            for (int i = 0; i < BallNo; i++)
            {
                balls[i].id = -1;
            }

        }
        public int msgtype; // = NET_VISION_CONFIG
        public vconfig_team[] teams;// = new vconfig_team[StaticVariables.NUM_TEAMS];
        public vconfig_ball[] balls;
    }
    public struct net_vframe
    {
        public net_vframe(int teamNo, int robotsNo, int BallNo)
        {
            msgtype = 0;
            timestamp = 0;
            ball = new vball[BallNo];
            for (int i = 0; i < BallNo; i++)
                ball[i] = new vball(16);
            robots = new vrobot[teamNo, robotsNo];
            refstate = 0;
            config = new net_vconfig(teamNo, robotsNo, BallNo);

        }
        public int msgtype;
        public double timestamp;
        public vball[] ball;// = new vball();
        public vrobot[,] robots;// = new vrobot[StaticVariables.NUM_TEAMS, StaticVariables.MAX_TEAM_ROBOTS];
        public int refstate;
        public net_vconfig config;// = new net_vconfig();
    }

    //public class vframe
    //{
    //    public vframe(vraw r, SingleObjectState s)
    //    {
    //        raw = r;
    //        State = s;
    //    }

    //    public vraw raw = new vraw();
    //    public SingleObjectState State = new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0);
    //}
    public class frame
    {
        public Dictionary<uint, vrobot> OurRobots = new Dictionary<uint, vrobot>();
        public Dictionary<uint, vrobot> OppRobots = new Dictionary<uint, vrobot>();
        public Dictionary<uint, vball> Balls = new Dictionary<uint, vball>();
        public RobotType type = RobotType.Default;
        public double timeofcapture = 0;
        public Dictionary<uint, double> timeList = new Dictionary<uint,double>();

        public Dictionary<uint, vball> OtherBalls = new Dictionary<uint, vball>();
    }
}
