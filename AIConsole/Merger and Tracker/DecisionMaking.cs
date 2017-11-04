using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using Meta.Numerics.Matrices;
using Enterprise;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    enum CachState
    {
        Engaled = 0, OurRobot = 1, OpponentRobot = 2
    }
    struct Cacher
    {
        public CachState State;
        public Position2D position;
        public Position2D? head;
        public int CacherID;
    }
    struct VectorLine
    {
        public float X1;
        public float X2;
        public float Y1;
        public float Y2;
        public float m;

        public float CalcStep()
        {
            float ret = 0;
            try
            {
                ret = (float)(Math.Atan2((Y2 - Y1), (X2 - X1)) * (180 / Math.PI));
            }

            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                ret = 90;
            }
            return ret;
        }
        public VectorLine(float X_1, float Y_1, float X_2, float Y_2)
        {
            X1 = X_1; Y1 = Y_1;
            X2 = X_2; Y2 = Y_2;
            m = 0;
            m = CalcStep();
        }
        public VectorLine(double X_1, double Y_1, double X_2, double Y_2)
        {
            X1 = (float)X_1; Y1 = (float)Y_1;
            X2 = (float)X_2; Y2 = (float)Y_2;
            m = 0;
            m = CalcStep();
        }
        public VectorLine(Position2D pos1, Position2D pos2)
        {
            X1 = (float)pos1.X; Y1 = (float)pos1.Y;
            X2 = (float)pos2.X; Y2 = (float)pos2.Y;
            m = 0; m = CalcStep();
        }
        internal float FindMinD(PointF d)
        {
            float landa = ((X2 - X1) * (d.X - X1) + (Y2 - Y1) * (d.Y - Y1)) / ((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
            if (landa >= 0 && landa <= 1)
            {
                PointF P0P1 = new PointF(d.X - X1, d.Y - Y1);
                PointF a = new PointF(X2 - X1, Y2 - Y1);
                float aSize = (float)Math.Sqrt(a.X * a.X + a.Y * a.Y);
                float EP = ExternalProduct(a, P0P1);
                float Numer = (float)Math.Sqrt(EP * EP);

                return (float)(Numer / aSize);
            }
            else
            {
                return 100;
            }
        }
        internal float FindMinD(Position2D d)
        {
            float landa = (float)((X2 - X1) * (d.X - X1) + (Y2 - Y1) * (d.Y - Y1)) / ((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
            if (landa >= 0 && landa <= 1)
            {
                PointF P0P1 = new PointF((float)d.X - X1, (float)d.Y - Y1);
                PointF a = new PointF(X2 - X1, Y2 - Y1);
                float aSize = (float)Math.Sqrt(a.X * a.X + a.Y * a.Y);
                float EP = ExternalProduct(a, P0P1);
                float Numer = (float)Math.Sqrt(EP * EP);

                return (float)(Numer / aSize);
            }
            else
            {
                return 100;
            }
        }
        private float ExternalProduct(PointF d1, PointF d2)
        {
            return (float)(d1.X * d2.Y - d1.Y * d2.X);
        }
        internal float AngleBetween(VectorLine L2)
        {
            float denom = L2.Size() * this.Size();
            float Numer = (X2 - X1) * (L2.X2 - L2.X1) + (Y2 - Y1) * (L2.Y2 - L2.Y1);
            if (Math.Abs(Numer - denom) < 0.000001)
            {
                return 0;
            }
            else
            {
                return (float)(Math.Acos(Numer / denom) * (180 / Math.PI));
            }
        }
        public VectorLine Shadow(VectorLine L2)
        {
            float teta = AngleBetween(L2);
            float coeff = (float)((L2.Size() / this.Size()) * Math.Cos((Math.PI / 180) * teta));
            VectorLine Vl = new VectorLine(X1, Y1, X1 + coeff * (X2 - X1), Y1 + coeff * (Y2 - Y1));
            return Vl;

        }
        public float Size()
        {
            PointF a = new PointF(X2 - X1, Y2 - Y1);
            return (float)Math.Sqrt(a.X * a.X + a.Y * a.Y);
        }
    }
    public class BallRegression
    {
        public double Error;
        public int PonSpaceCounter = 0;
        public double[] Z = new double[1000];
        public double[] d = new double[1000];
        public Position2D[] PonSpace = new Position2D[1000];
        public BallRegression()
        {
            PonSpaceCounter = 0;
        }
    }
    class DecisionMaking
    {
        #region "Shadow Section"
        /// <summary>
        /// Create Shadow Region 
        /// </summary>
        private HiddenBallGuesser HBG = new HiddenBallGuesser();
        private Cacher LastCacher;
        public void ShowRegion(bool Show)
        {
            HBG.DrawRegion = Show;
        }
        public void AddCamera(PointF Camera_Position, float Camera_Height, RectangleF ViewedArea)
        {
            CameraInfo cam = new CameraInfo();
            if (HBG.Cameras == null)
                HBG.Cameras = new List<CameraInfo>();
            cam.CenterLocation = Camera_Position;
            cam.Height = Camera_Height;
            cam.VisibleArea = ViewedArea;
            HBG.Cameras.Add(cam);
        }
        public bool FindShadows(WorldModel model, PointF lastBall_Location)
        {
            bool ret = HBG.GuessHiddenBallPosition(model, lastBall_Location);
            return ret;
        }
        public bool isPointinShadow()
        {
            return HBG.isPointinShadow;
        }
        private Cacher FindCacher(WorldModel wm)
        {
            Cacher ret = new Cacher();

            CachState cs = CachState.Engaled;
            Position2D ballpos = wm.BallState.Location;
            double Mindist = double.MaxValue;

            int MinOurIndex = 0;
            int MinOppIndex = 0;

            foreach (int key in wm.OurRobots.Keys)
            {
                double dist = distance(wm.OurRobots[key].Location, ballpos);
                if (dist < Mindist)
                {
                    Mindist = dist;
                    MinOurIndex = key;
                }
            }
            if (Mindist < RobotParameters.OurRobotParams.Diameter / 2.0 + 0.07)
            {
                cs = CachState.OurRobot;
                ret.State = cs;
                ret.position = wm.OurRobots[MinOurIndex].Location;
                double r = RobotParameters.OurRobotParams.Diameter / 2.0;
                if (wm.OurRobots[MinOurIndex].Angle != null)
                {
                    ret.head = new Position2D(ret.position.X + r * Math.Cos((double)(wm.OurRobots[MinOurIndex].Angle * Math.PI / 180)), ret.position.Y + r * Math.Sin((double)(wm.OurRobots[MinOurIndex].Angle * Math.PI / 180)));
                    //VisualizerData.AddMomently(new Circle(ret.head, 0.01,new Pen(Color.Beige,0.01f)), "head");
                }
                else
                {
                    ret.head = null;
                }
                ret.CacherID = MinOurIndex;
            }


            Mindist = double.MaxValue;

            foreach (int key in wm.Opponents.Keys)
            {
                double dist = distance(wm.Opponents[key].Location, ballpos);
                if (dist < Mindist)
                {
                    Mindist = dist;
                    MinOppIndex = key;
                }
            }
            if (Mindist < RobotParameters.OurRobotParams.Diameter / 2.0 + 0.07)
            {
                if (cs == CachState.OurRobot)
                {
                    cs = CachState.Engaled;
                    ret.State = cs;
                    ret.position = new Position2D(0, 0);
                    ret.CacherID = -1;
                }
                else
                {
                    cs = CachState.OpponentRobot;
                    ret.State = cs;
                    ret.position = wm.Opponents[MinOppIndex].Location;
                    double r = RobotParameters.OpponentParams.Diameter / 2.0;
                    if (wm.Opponents[MinOppIndex].Angle != null)
                    {
                        ret.head = new Position2D(ret.position.X + r * Math.Cos((double)(wm.Opponents[MinOppIndex].Angle * Math.PI / 180)), ret.position.Y + r * Math.Sin((double)(wm.Opponents[MinOppIndex].Angle * Math.PI / 180)));
                        //VisualizerData.AddMomently(new Circle(ret.head, 0.01,new Pen(Color.Beige,0.01f)), "head");
                    }
                    else
                    {
                        ret.head = null;
                    }
                    ret.CacherID = MinOppIndex;
                }
            }
            return ret;
        }
        private VectorLine anticipatedVec;
        private double MovementvarianceSum = 0;
        private int MovementvarianceCount = 0;
        private bool cheapkickActed = false;
        private Dictionary<int, double> DiversityHistory = new Dictionary<int, double>();
        public Position2D esPos;
        public bool isCheapKick(WorldModel wm)
        {
            Cacher s = FindCacher(wm);
            if (s.State != CachState.Engaled)
            {
                if (cheapkickActed == false)
                {
                    if (s.head == null) return false;
                    VectorLine tmp1 = new VectorLine(LastCacher.position, (Position2D)s.head);
                    VectorLine tmp2 = new VectorLine(LastCacher.position, wm.BallState.Location);
                    if (tmp1.AngleBetween(tmp2) > 10)
                    {
                        DiversityHistory.Clear();
                        LastCacher.State = CachState.Engaled;
                        MovementvarianceSum = 0;
                        MovementvarianceCount = 0;
                    }
                    else
                    {
                        DiversityHistory.Clear();
                        LastCacher = s;
                        MovementvarianceSum = 0;
                        MovementvarianceCount = 0;

                        if (LastCacher.head == null)
                        {
                            anticipatedVec = new VectorLine(LastCacher.position, wm.BallState.Location);
                        }
                        else
                        {
                            anticipatedVec = new VectorLine(LastCacher.position, (Position2D)LastCacher.head);
                        }
                    }
                }
            }
            else if (s.State == CachState.Engaled && s.head == null)
            {
                LastCacher.State = CachState.Engaled;
            }
            else
            {
                if (LastCacher.State != CachState.Engaled)
                {
                    //Find Out about is ball cheapkicked
                    VectorLine mesuredVec = new VectorLine(LastCacher.position, wm.BallState.Location);
                    MovementvarianceCount++;
                    DiversityHistory.Add(MovementvarianceCount, anticipatedVec.AngleBetween(mesuredVec));
                    if (MovementvarianceCount > 80)
                    {
                        if (isCheapKickBehaviour(DiversityHistory) == true)
                        {
                            VectorLine shadow = anticipatedVec.Shadow(mesuredVec);
                            esPos.X = shadow.X2;
                            esPos.Y = shadow.Y2;
                            cheapkickActed = true;
                            return true;
                        }
                        else
                        {
                            MovementvarianceCount = 0;
                            DiversityHistory.Clear();
                            cheapkickActed = false;
                        }
                    }
                }
            }
            return false;
        }
        private bool isCheapKickBehaviour(Dictionary<int, double> DiversityHistory)
        {
            int MaxKey = -1;
            double MaxDiv = double.MinValue;
            foreach (int key in DiversityHistory.Keys)
            {
                //find Maximum Diversity
                if (DiversityHistory[key] > MaxDiv)
                {
                    MaxDiv = DiversityHistory[key];
                    MaxKey = key;
                }
            }
            for (int key = MaxKey; key < DiversityHistory.Count - 5; key += 5)
            {
                if ((DiversityHistory[key] - DiversityHistory[key + 5]) < -10) // maximum permissible noise in detection
                {
                    return false;
                }
            }
            return true;
        }
        private double distance(Position2D src, Position2D dst)
        {
            double DX = (src.X - dst.X);
            double DY = (src.Y - dst.Y);
            return Math.Sqrt(DX * DX + DY * DY);
        }
        #endregion
        public Dictionary<int, BallRegression> Repository = new Dictionary<int, BallRegression>(360);
        internal Position2D CalculateChipKickPosition(double RealTeta, Position2D Pc, Position2D P0, Position2D Pi, bool isReverseSide, out double Height)
        {
            double alpha_xi = 0;
            Position2D BestCurrentPosition = Pi;
            double x_prime = 0, y_prime = 0, z = 0;
            double a = 0, b = 1, d = 0, a_prime = 0, b_prime = 0;
            Height = 0;

            #region "calculatin teta real position in 3D Space"
            for (int teta = -4 + (int)RealTeta; teta < 4 + (int)RealTeta; teta++)
            {
                int cTeta = teta;
                if (cTeta < 0)
                    cTeta += 360;
                if (cTeta > 360)
                    cTeta -= 360;

                Line line = new Line(P0, P0 + Vector2D.FromAngleSize((cTeta * Math.PI) / 180.0, 1));
                if (isReverseSide)
                {
                    DrawingObjects.AddObject(new Line(new Position2D(-line.Head.X, -line.Head.Y), new Position2D(-line.Tail.X, -line.Tail.Y)), "  ");
                    DrawingObjects.AddObject(new Circle(new Position2D(-P0.X, -P0.Y), 0.05, new Pen(Color.Gold, 0.02f)));
                }
                else
                {
                    DrawingObjects.AddObject(line, "  ");
                    DrawingObjects.AddObject(new Circle(P0, 0.05, new Pen(Color.Gold, 0.02f)));
                }
                a = line.A; b = line.B; d = -line.C;

                a_prime = (Pi.Y - Pc.Y) / (Pi.X - Pc.X);
                b_prime = (Pi.X * Pc.Y - Pi.Y * Pc.X) / (Pi.X - Pc.X);
                x_prime = (d - (b * b_prime)) / (a + (b * a_prime));
                y_prime = (a_prime * d + b_prime * a) / (a + (b * a_prime));

                z = 4 * ((x_prime - Pc.X) / (Pc.X - Pi.X)) + 4;

                alpha_xi = Math.Sqrt((x_prime - P0.X) * (x_prime - P0.X) + (y_prime - P0.Y) * (y_prime - P0.Y));

                if (!Repository.ContainsKey(cTeta))
                    Repository.Add(cTeta, new BallRegression());

                if (Repository[cTeta].PonSpaceCounter < 1000)
                {
                    Repository[cTeta].Z[Repository[cTeta].PonSpaceCounter] = z;
                    Repository[cTeta].d[Repository[cTeta].PonSpaceCounter] = alpha_xi;
                    Repository[cTeta].PonSpace[Repository[cTeta].PonSpaceCounter] = new Position2D(x_prime, y_prime);
                    Repository[cTeta].PonSpaceCounter++;
                }
            }
            #endregion

            #region "Finding Best Teta"
            double MinError = double.MaxValue;
            foreach (int key in Repository.Keys)
            {
                RectangularMatrix X = new RectangularMatrix(2, Repository[key].PonSpaceCounter);
                RectangularMatrix Y = new RectangularMatrix(1, Repository[key].PonSpaceCounter);

                if (Repository[key].PonSpaceCounter > 4)
                {
                    for (int i = 0; i < Repository[key].PonSpaceCounter; i++)
                    {
                        X[0, i] = Repository[key].d[i] * Repository[key].d[i];
                        X[1, i] = Repository[key].d[i];
                        Y[0, i] = Repository[key].Z[i];
                    }
                    Repository[key].Error = 0;
                    RectangularMatrix P = Y * X.Transpose() * ((SquareMatrix)(X * X.Transpose())).Inverse();
                    for (int i = 0; i < Repository[key].PonSpaceCounter; i++)
                    {
                        double Es_Y = P[0, 0] * Repository[key].d[i] * Repository[key].d[i] + P[0, 1] * Repository[key].d[i];
                        Repository[key].Error += (Repository[key].Z[i] - Es_Y) * (Repository[key].Z[i] - Es_Y);
                    }
                    if (Repository[key].Error <= MinError)
                    {
                        MinError = Repository[key].Error;
                        BestCurrentPosition = Repository[key].PonSpace[Repository[key].PonSpaceCounter - 1];
                        Height = Repository[key].Z[Repository[key].PonSpaceCounter - 1];
                    }
                }
            }
            #endregion

            return BestCurrentPosition;

        }

        internal Position2D FindNearestCamera(Position2D lastBall_Location)
        {
            Position2D CameraPosition = new Position2D();
            double Min_dist = double.MaxValue;
            if (HBG.Cameras != null)
            {
                for (int i = 0; i < HBG.Cameras.Count; i++)
                {
                    double dist = lastBall_Location.DistanceFrom(HBG.Cameras[i].CenterLocation);
                    if (dist < Min_dist)
                    {
                        Min_dist = dist;
                        CameraPosition = HBG.Cameras[i].CenterLocation;
                    }
                }
            }
            return CameraPosition;
        }
    }
}
