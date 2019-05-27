using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.Extentions;
using MRL.SSL.CommonControls.Direct2D;
using DWriteFactory = SlimDX.DirectWrite.Factory;
using PointF = System.Drawing.PointF;
using Color = System.Drawing.Color;
using SizeF = System.Drawing.SizeF;
using RectangleF = System.Drawing.RectangleF;
using SlimDX;
using MRL.SSL.CommonClasses;
using MRL.SSL.CommonControls.Extention;
using System.Threading.Tasks;
using SlimDX.Direct2D;

namespace MRL.SSL.CommonControls.D2DControls
{
    public partial class NewFieldVisualizer : D2DControl
    {
        public Dictionary<int, Position2D> MergerPoint { get; set; }
        public bool strategyviewer { get; set; }
        public DrawCollection VisualizerObject = new DrawCollection();
        FieldOrientation _orentation = FieldOrientation.Verticaly;
        /// <summary>
        /// Vertical Or Horizental
        /// </summary>
        public FieldOrientation Orentation
        {
            get { return _orentation; }
            set { _orentation = value; }
        }
        public int? SelectedRobotID { get; set; }
        LoggerDrawingObject LocalDrawingObject = new LoggerDrawingObject();
        Line _speedVector;
        int _ballTailCount = 200, _robotTailCount = 300;
        public int RobotTailCount
        {
            get
            {
                return _robotTailCount;
            }
            set
            {
                _robotTailCount = value;
                _robotSign.Clear();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int BallTailCount
        {
            get
            {
                return _ballTailCount;
            }
            set
            {
                _ballTailCount = value;
                _balls.Clear();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        List<Circle> _hist2;
        /// <summary>
        /// 
        /// </summary>
        public Line SpeedVector
        {
            get
            {
                return _speedVector;
            }
            set
            {
                _speedVector = value;
                this.Invalidate();
            }
        }
        long _frameCount = 0;
        bool leftclickdown = false;
        public bool ShowTexts { get; set; }
        Dictionary<int, List<Position2D>> _robotSign = new Dictionary<int, List<Position2D>>();
        /// <summary>
        /// 
        /// </summary>
        public bool ShowSpeedVector { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ShowBallTail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ShowRobotAngle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LoggerDrawingObject Logdrawingobjects { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool VisionMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool MergerCalibMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool RobotTail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool LogPlayerMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool LogAnalizerMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool DrawOurRobot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool DrawOppRobot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AnalizeMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool MoveMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Object> AnalizeOjects = new List<object>();
        /// <summary>
        /// 
        /// </summary>
        Object momentlyObject = new Object();
        /// <summary>
        /// 
        /// </summary>
        DWriteFactory dwriteFactory;
        /// <summary>
        /// 
        /// </summary>
        //SlimDX.DirectWrite.TextFormat textFormat;
        /// <summary>
        /// 
        /// </summary>
        // Brush helpBrush, PathBrush;
        /// <summary>
        /// 
        /// </summary>
        //StrokeStyle helperStrok;
        /// <summary>
        /// 
        /// </summary>
        bool mouseIsdown;
        List<SingleObjectState> _balls;
        object selected;
        /// <summary>
        /// control cosntractor
        /// </summary>
        public NewFieldVisualizer()
            : base()
        {
            InitializeComponent();
            DrawOurRobot = true;
            DrawOppRobot = true;
            ShowTexts = true;
            Logdrawingobjects = new LoggerDrawingObject();
            LogPlayerMode = false;
            _hist2 = new List<Circle>();
            dwriteFactory = new DWriteFactory(SlimDX.DirectWrite.FactoryType.Isolated);

            _balls = new List<SingleObjectState>(_ballTailCount);
            this.MouseMove += new MouseEventHandler(NewFieldVisualizer_MouseMove);
            this.MouseDown += new MouseEventHandler(NewFieldVisualizer_MouseDown);
            this.MouseUp += new MouseEventHandler(NewFieldVisualizer_MouseUp);
            ShowBallTail = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NewFieldVisualizer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                leftclickdown = false;
            if (AnalizeMode)
                mouseIsdown = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NewFieldVisualizer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                leftclickdown = true;
            if (AnalizeMode)
            {
                Position2D loc = PixelToMetric(e.Location);
                selected = null;
                selected = AnalizeOjects.Where(w => w.GetType() == typeof(SingleObjectState) &&
                             Math.Pow(w.As<SingleObjectState>().Location.X - loc.X, 2) + Math.Pow(w.As<SingleObjectState>().Location.Y - loc.Y, 2)
                            < Math.Pow(RobotParameters.OurRobotParams.Diameter / 2, 2)).FirstOrDefault();
                if (selected != null)
                    mouseIsdown = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NewFieldVisualizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (AnalizeMode && MoveMode)
            {
                if (e.Button == MouseButtons.Left && mouseIsdown)
                {
                    Position2D loc = PixelToMetric(e.Location);
                    selected.As<SingleObjectState>().Location = PixelToMetric(e.Location);
                    this.Invalidate();
                }
            }
        }
        private AiToVisualizerWrapper _currentWrapper;
        /// <summary>
        /// 
        /// </summary>
        public AiToVisualizerWrapper CurrentWrapper
        {
            get { return _currentWrapper; }
            set
            {
                if (value != null)
                {
                    _currentWrapper = value;
                    if (VisionMode)
                        _currentWrapper.RobotCommnd = new Dictionary<int, SingleWirelessCommand>();
                    Model = _currentWrapper.Model;
                }
            }
        }
        /// <summary>
        /// WorldModel that be Show
        /// </summary>
        private WorldModel _model;
        public WorldModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                _frameCount++;
                this.Invalidate();
            }

        }
        /// <summary>
        /// my Brushes
        /// </summary>
        // Brush WhiteBrush, BallBrush, RobotBrush, BorderBrush, TextBrush, BlackBrush;
        //PathGeometry pg;
        /// <summary>
        /// OnPaint event for FieldVisualizer
        /// </summary>
        /// <param name="renderTarget"></param>
        protected override void OnPaintContent()
        {
            if (MergerCalibMode)
            {
                DrawField();
                DrawMergerPoint();
                DrawMergerBall();
                DrawGoalsDepth();
                DrawObj(momentlyObject);
            }
            else if (VisionMode)
            {
                DrawField();
                DrawBallTail();
                DrawAllRobots();
                DrawRobotTail();
                if (_speedVector != null)
                    DrawVector(_speedVector);
                DrawObjects();
                DrawVisualizerObjects();
            }
            else if (strategyviewer)
            {
                DrawField();
                DrawAllRobots();
            }
            else
            {
                DrawField();
                DrawAccordingToStatus();
                if (!LogPlayerMode && !AnalizeMode)
                    DrawObjects();
                else if (LogPlayerMode)
                {
                    LocalDrawingObject.drawingObject.Clear();
                    LocalDrawingObject.ObjectTree = Logdrawingobjects.ObjectTree;
                    Logdrawingobjects.drawingObject.ToList().ForEach(p =>
                    {
                        LocalDrawingObject.AddObject(p.Key, p.Value);
                    });
                    DrawObjects(LogAnalizerMode);
                }
                DrawBallTail();
                //////DrawSelectedRobotID(renderTarget);
                DrawOtherBalls();
                DrawPainted();
                DrawObj(momentlyObject);
                DrawGoalsDepth();
                DrawRobotTail();
                DrawAllRobots();
                DrawKickStatus();
                if (_speedVector != null)
                    DrawVector(_speedVector);
                if (!LogPlayerMode && !AnalizeMode)
                    DrawAllText();
            }
        }
        /// <summary>
        /// darwin Merger Points
        /// </summary>
        private void DrawMergerPoint()
        {
            if (MergerPoint == null)
                return;
            Brush BlackBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
            Brush BallBrush = new SolidColorBrush(renderTarget, new Color4(Color.HotPink));
            BallBrush.Opacity = 1.0f;
            foreach (var item in MergerPoint)
            {
                renderTarget.DrawEllipse(BlackBrush, new Ellipse().GetEllipse((item.Value), (float)GameParameters.BallDiameter / 2, (float)GameParameters.BallDiameter / 2), 0.01f);
                Ellipse ellipse = new Ellipse();
                ellipse.Center = (item.Value);
                ellipse.RadiusY = ellipse.RadiusX = (float)GameParameters.BallDiameter / 2;
                renderTarget.FillEllipse(BallBrush, ellipse);
            }
            BlackBrush.Dispose();
            BallBrush.Dispose();

        }
        private void DrawMergerBall()
        {
            if (_currentWrapper.AllBalls == null)
                return;
            Brush BlackBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
            Brush BallBrush = new SolidColorBrush(renderTarget, new Color4(Color.Orange));
            BallBrush.Opacity = 1.0f;
            foreach (var item in _currentWrapper.AllBalls)
            {
                renderTarget.DrawEllipse(BlackBrush, new Ellipse().GetEllipse((item.Value), (float)GameParameters.BallDiameter / 2, (float)GameParameters.BallDiameter / 2), 0.01f);
                Ellipse ellipse = new Ellipse();
                ellipse.Center = (item.Value);
                ellipse.RadiusY = ellipse.RadiusX = (float)GameParameters.BallDiameter / 2;
                renderTarget.FillEllipse(BallBrush, ellipse);
            }
            BlackBrush.Dispose();
            BallBrush.Dispose();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawBallTail()
        {
            if (_model == null) return;
            _balls.Add(_model.BallState);
            if (_balls.Count > _ballTailCount)
                _balls.RemoveAt(0);
            if (ShowBallTail)
                for (int i = 0; i < _balls.Count; i++)
                    DrawRobot(_balls[i], 0, (i * 1f / 239f));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawRobotTail()
        {
            if (_model == null) return;
            WorldModel tModel = new WorldModel(_model);
            if (tModel.OurRobots != null && DrawOurRobot)
                foreach (int key in tModel.OurRobots.Keys)
                {
                    if (RobotTail)
                    {

                        if (!_robotSign.ContainsKey(key))
                            _robotSign.Add(key, new List<Position2D>());
                        if (Model.OurRobots.ContainsKey(key))
                        {
                            _robotSign[key].Add(Model.OurRobots[key].Location);
                            if (_robotSign[key].Count > _robotTailCount)
                                _robotSign[key].RemoveAt(0);
                            DrawPath(_robotSign[key]);
                        }
                    }
                    else
                        _robotSign.Clear();
                }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PathData"></param>
        /// <param name="renderTarget"></param>
        private void DrawPath(List<Position2D> PathData)
        {

            Brush PathBrush = new SolidColorBrush(renderTarget, new Color4(Color.Blue));
            for (int i = 1; i < PathData.Count; i++)
                renderTarget.DrawLine(PathBrush, (PathData[i - 1]), (PathData[i]), 0.01f);
            PathBrush.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="speedVector"></param>
        private void DrawVector(Line speedVector)
        {
            Brush BlackBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
            renderTarget.DrawLine(BlackBrush, speedVector.Head, speedVector.Tail, 0.01f);
            BlackBrush.Dispose();
        }
        /// <summary>
        /// Draw fild visualizer
        /// </summary>
        /// <param name="renderTarget">render host control</param>
        void DrawField()
        {
            #region Field Drawing

            Brush WhiteBrush = new SolidColorBrush(renderTarget, new Color4(Color.White));
            //Main Rectangle
            {
                PathGeometry pg = new PathGeometry(d2dfactory);
                GeometrySink gs = pg.Open();
                gs.BeginFigure((GameParameters.OurLeftCorner), FigureBegin.Hollow);
                gs.AddLine((GameParameters.OurRightCorner));
                gs.AddLine((GameParameters.OppLeftCorner));
                gs.AddLine((GameParameters.OppRightCorner));
                gs.EndFigure(FigureEnd.Closed);
                gs.Close();
                renderTarget.DrawGeometry(pg, WhiteBrush, 0.01f);
                renderTarget.DrawLine(WhiteBrush,
                    (new System.Drawing.PointF(0, (float)(GameParameters.OppLeftCorner.Y + GameParameters.OurRightCorner.Y) / 2)),
                    (new System.Drawing.PointF(0, (float)(float)-(GameParameters.OurRightCorner.Y + GameParameters.OppLeftCorner.Y) / 2)), 0.01f);
                //Center Line For Merger Calib
                if (MergerCalibMode)
                {
                    renderTarget.DrawLine(WhiteBrush, new System.Drawing.PointF((float)GameParameters.OurGoalCenter.X, (float)GameParameters.OurGoalCenter.Y), new System.Drawing.PointF((float)GameParameters.OppGoalCenter.X, (float)GameParameters.OppGoalCenter.Y), 0.01f);
                }
                //CenterCircle
                Ellipse center = new Ellipse();
                center.Center = new PointF(0, 0);
                center.RadiusX = 0.5f;
                center.RadiusY = 0.5f;
                renderTarget.DrawEllipse(WhiteBrush, center, 0.01f);
                Ellipse CenterPoint = new Ellipse();
                CenterPoint.Center = new PointF(0, 0);
                CenterPoint.RadiusX = .02f;
                CenterPoint.RadiusY = .02f;
                renderTarget.FillEllipse(WhiteBrush, CenterPoint);
                pg.Dispose();
                gs.Dispose();
            }
            
            {


                PathGeometry pg1 = new PathGeometry(d2dfactory);
                GeometrySink gs1 = pg1.Open();
                //Our Defence Area
                gs1.BeginFigure(new PointF((float)GameParameters.OurGoalCenter.X, (float)(GameParameters.OurGoalCenter.Y + GameParameters.DefenceAreaWidth / 2)), FigureBegin.Hollow);
                gs1.AddLine(new PointF((float)(GameParameters.OurGoalCenter.X - GameParameters.DefenceAreaHeight), (float)(GameParameters.OurGoalCenter.Y + GameParameters.DefenceAreaWidth / 2)));
                gs1.AddLine(new PointF((float)(GameParameters.OurGoalCenter.X - GameParameters.DefenceAreaHeight), (float)(GameParameters.OurGoalCenter.Y - GameParameters.DefenceAreaWidth / 2)));
                gs1.AddLine(new PointF((float)(GameParameters.OurGoalCenter.X), (float)(GameParameters.OurGoalCenter.Y - GameParameters.DefenceAreaWidth / 2)));
                gs1.EndFigure(FigureEnd.Open);
                //Opp Defence Area
                gs1.BeginFigure(new PointF((float)GameParameters.OppGoalCenter.X, (float)(GameParameters.OppGoalCenter.Y + GameParameters.DefenceAreaWidth / 2)), FigureBegin.Hollow);
                gs1.AddLine(new PointF((float)(GameParameters.OppGoalCenter.X + GameParameters.DefenceAreaHeight), (float)(GameParameters.OppGoalCenter.Y + GameParameters.DefenceAreaWidth / 2)));
                gs1.AddLine(new PointF((float)(GameParameters.OppGoalCenter.X + GameParameters.DefenceAreaHeight), (float)(GameParameters.OppGoalCenter.Y - GameParameters.DefenceAreaWidth / 2)));
                gs1.AddLine(new PointF((float)(GameParameters.OppGoalCenter.X), (float)(GameParameters.OppGoalCenter.Y - GameParameters.DefenceAreaWidth / 2)));

                gs1.EndFigure(FigureEnd.Open);
                gs1.Close();
                renderTarget.DrawGeometry(pg1, WhiteBrush, 0.01f);
                pg1.Dispose();
                gs1.Dispose();

            }
            //Our Goal
            {
                PathGeometry pg2 = new PathGeometry(d2dfactory);
                GeometrySink gs2 = pg2.Open();
                gs2.BeginFigure(new PointF((float)(GameParameters.OurGoalLeft.X), (float)GameParameters.OurGoalLeft.Y), FigureBegin.Hollow);
                gs2.AddLine((new Position2D(GameParameters.OurGoalLeft.X, GameParameters.OurGoalLeft.Y) + new Vector2D(GameParameters.GoalDepth, 0)));
                gs2.AddLine((new Position2D(GameParameters.OurGoalRight.X, GameParameters.OurGoalRight.Y) + new Vector2D(GameParameters.GoalDepth, 0)));
                gs2.AddLine((new Position2D(GameParameters.OurGoalRight.X, GameParameters.OurGoalRight.Y)));
                gs2.EndFigure(FigureEnd.Open);
                //Opp Goal
                gs2.BeginFigure(new PointF((float)(GameParameters.OppGoalLeft.X), (float)GameParameters.OppGoalLeft.Y), FigureBegin.Hollow);
                gs2.AddLine((new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y) + new Vector2D(-GameParameters.GoalDepth, 0)));
                gs2.AddLine((new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y) + new Vector2D(-GameParameters.GoalDepth, 0)));
                gs2.AddLine((new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y)));
                gs2.EndFigure(FigureEnd.Open);
                gs2.Close();
                renderTarget.DrawGeometry(pg2, WhiteBrush, 0.02f);
                pg2.Dispose();
                gs2.Dispose();
            }
            //Penalty Points
            Ellipse PenaltyPoint = new Ellipse();
            PenaltyPoint.Center = new PointF((float)GameParameters.OppGoalCenter.X + (float)GameParameters.PenaltyDistanceFromGoalLine, (float)GameParameters.OppGoalCenter.Y);
            PenaltyPoint.RadiusX = 0.005f;
            PenaltyPoint.RadiusY = 0.005f;
            renderTarget.FillEllipse(WhiteBrush, PenaltyPoint);
            PenaltyPoint.Center = new PointF((float)GameParameters.OurGoalCenter.X - (float)GameParameters.PenaltyDistanceFromGoalLine, (float)GameParameters.OurGoalCenter.Y);
            renderTarget.FillEllipse(WhiteBrush, PenaltyPoint);

            #endregion
            WhiteBrush.Dispose();

        }
        /// <summary>
        /// Initialize field transformation
        /// </summary>
        public override void InitializeTransform()
        {
            System.Drawing.RectangleF rect = System.Drawing.RectangleF.FromLTRB((float)GameParameters.OppRightCorner.X, (float)GameParameters.OppRightCorner.Y, (float)GameParameters.OurLeftCorner.X, (float)GameParameters.OurLeftCorner.Y);
            rect.Inflate(GameParameters.FieldMargins);
            //Transform = new Matrix3x2F(126.43f, 0, 0, 122.38f, 414f, 292f);
            Transform = new Matrix3x2() { M11 = 0, M12 = 100, M21 = -100, M22 = 0, M31 = 260, M32 = 340 };
            //if (strategyviewer)
            //    Transform = new Matrix3x2() { M11 = -62.0921f, M12 = 0, M21 = 0, M22 = 6209, M31 = 260.34f, M32 = 144.7911f };
            //Transform = MatrixCalculator.CreateMatrix(new System.Drawing.PointF[] { rect.Location, new System.Drawing.PointF(rect.Right, rect.Top), new System.Drawing.PointF(rect.Left, rect.Bottom) }, this.ClientRectangle);
            // Transform = MatrixCalculator.CreateMatrix(new System.Drawing.PointF[] { rect.Location, new System.Drawing.PointF(rect.Right, rect.Top), new System.Drawing.PointF(rect.Left, rect.Bottom) }, new System.Drawing.Rectangle(0, 0, 1320, 820));
            //Transform = MatrixCalculator.Rotate(new Position2D(0, 0), Transform.Value, 90, System.Drawing.Drawing2D.MatrixOrder.Append);

        }
        ///// <summary>
        ///// Convert position2D to Point2F
        ///// </summary>
        ///// <param name="pos">Position2D</param>
        ///// <returns>Point2F</returns>
        //Point2F (Position2D pos)
        //{
        //    return new Point2F((float)pos.X, (float)pos.Y);
        //}
        /// <summary>
        /// darwin objects(robots , balls)
        /// </summary>
        /// <param name="state"></param>
        /// <param name="RobotID"></param>
        /// <param name="renderTarget"></param>
        private void DrawRobot(SingleObjectState state, int RobotID)
        {
            if (state == null) return;
            if (state.Type == ObjectType.Ball)
            {

                Brush BallBrush = new SolidColorBrush(renderTarget, new Color4(1f, 1f, 129f / 255f, 0f));
                Ellipse ellipse = new Ellipse();
                ellipse.Center = state.Location;
                ellipse.RadiusY = ellipse.RadiusX = (float)GameParameters.BallDiameter / 2;
                renderTarget.FillEllipse(BallBrush, ellipse);
                Brush Ballborder = new SolidColorBrush(renderTarget, new Color4(1f, 209f / 255f, 89f / 255f, 0f));
                renderTarget.DrawEllipse(Ballborder, ellipse, 0.006f);
                Ballborder.Dispose();
                BallBrush.Dispose();
                if (ShowSpeedVector)
                {
                    StrokeStyleProperties st = new StrokeStyleProperties()
                    {
                        StartCap = CapStyle.Flat,
                        EndCap = CapStyle.Triangle,
                        DashStyle = DashStyle.Solid,
                        LineJoin = LineJoin.Round,
                        MiterLimit = 1,
                        DashCap = CapStyle.Flat,
                        DashOffset = 1
                    };
                    StrokeStyle strock = new StrokeStyle(d2dfactory, st);
                    renderTarget.DrawLine(new SolidColorBrush(renderTarget, new Color4(Color.Orange))
                        , state.Location,
                        (state.Location + state.Speed), 0.01f, strock);
                    strock.Dispose();
                }
            }
            else
            {
                Brush RobotBrush;
                if (SelectedRobotID.HasValue && Model.OurRobots.ContainsKey(SelectedRobotID.Value) && SelectedRobotID.Value == RobotID)
                {
                    RobotBrush = new SolidColorBrush(renderTarget, new Color4(1, 0, 0, 0.5f));
                }
                else
                    RobotBrush = new SolidColorBrush(renderTarget, (state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new Color4(1f, 65f / 255f, 126f / 255f, 255 / 255f) : new Color4(1f, 255f / 255f, 243f / 255f, 62f / 255f));
                Brush BorderBrush = new SolidColorBrush(renderTarget, (state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new Color4(1f, 18f / 255f, 59f / 255f, 160f / 255f) : new Color4(1f, 204f / 255f, 157f / 255f, 0f / 255f));
                if (state.Angle.HasValue)
                {
                    Position2D p1 = state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2);
                    Position2D p2 = state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2);

                    if (ShowRobotAngle)
                    {
                        PathGeometry pg1 = new PathGeometry(d2dfactory);
                        GeometrySink gs1 = pg1.Open();
                        gs1.BeginFigure(p2, FigureBegin.Filled);
                        gs1.AddLine(p2 + Vector2D.FromAngleSize(state.Angle.Value * Math.PI / 180.0, 5));
                        gs1.AddLine(p1 + Vector2D.FromAngleSize(state.Angle.Value * Math.PI / 180.0, 5));
                        gs1.AddLine(p1);
                        gs1.EndFigure(FigureEnd.Closed);
                        gs1.Close();
                        BorderBrush.Opacity = .3f;
                        renderTarget.FillGeometry(pg1, BorderBrush);
                        BorderBrush.Opacity = 1f;
                        pg1.Dispose();
                        gs1.Dispose();
                    }
                    PathGeometry pg = new PathGeometry(d2dfactory);
                    GeometrySink gs = pg.Open();
                    gs.BeginFigure(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2), FigureBegin.Filled);
                    ArcSegment arcseg3 = new ArcSegment();
                    arcseg3.ArcSize = ArcSize.Large;
                    arcseg3.EndPoint = state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2);
                    arcseg3.RotationAngle = (float)Math.PI / 2;
                    arcseg3.Size = new SizeF((float)RobotParameters.OurRobotParams.Diameter / 2, (float)RobotParameters.OurRobotParams.Diameter / 2);
                    arcseg3.SweepDirection = SweepDirection.Clockwise;
                    gs.AddArc(arcseg3);
                    gs.AddLine(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2));
                    gs.EndFigure(FigureEnd.Closed);
                    gs.Close();
                    renderTarget.FillGeometry(pg, RobotBrush);
                    renderTarget.DrawGeometry(pg, BorderBrush, 0.01f);
                    pg.Dispose();
                    gs.Dispose();
                }
                else
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Center = state.Location;
                    ellipse.RadiusY = ellipse.RadiusX = (state.Type == ObjectType.OurRobot) ? (float)RobotParameters.OurRobotParams.Diameter / 2 : (float)RobotParameters.OpponentParams.Diameter / 2;
                    renderTarget.FillEllipse(RobotBrush, ellipse);
                    renderTarget.DrawEllipse(BorderBrush, ellipse, 0.01f);
                }
                if (ShowSpeedVector)
                {
                    StrokeStyleProperties st = new StrokeStyleProperties()
                    {
                        StartCap = CapStyle.Flat,
                        EndCap = CapStyle.Triangle,
                        DashStyle = DashStyle.Solid,
                        LineJoin = LineJoin.Round,
                        MiterLimit = 1,
                        DashCap = CapStyle.Flat,
                        DashOffset = 1
                    };
                    StrokeStyle strock = new StrokeStyle(d2dfactory, st);
                    renderTarget.DrawLine(new SolidColorBrush(renderTarget, new Color4(Color.Red)), state.Location, (state.Location + state.Speed), 0.01f, strock);
                    strock.Dispose();
                }
                RobotBrush.Dispose();
                BorderBrush.Dispose();
                // TextBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
                //if (RobotID != 1000)
                //    if (_showTexts)
                //        DrawText(RobotID.ToString(), _textbrush, renderTarget, new System.Drawing.RectangleF(state.Location, new System.Drawing.SizeF((float)(RobotID.ToString().Length * 0.05), 0.1f)));
            }
        }
        /// <summary>
        /// darwin objects(robots , balls)
        /// </summary>
        /// <param name="state"></param>
        /// <param name="RobotID"></param>
        /// <param name="renderTarget"></param>
        /// <param name="opacity"></param>
        private void DrawRobot(SingleObjectState state, int RobotID, float opacity)
        {
            if (Model == null || state == null) return;
            if (state.Type == ObjectType.Ball)
            {
                Brush BallBrush = new SolidColorBrush(renderTarget, new Color4(Color.Orange));
                Ellipse ellipse = new Ellipse();
                ellipse.Center = (state.Location);
                ellipse.RadiusY = ellipse.RadiusX = (float)GameParameters.BallDiameter / 2;
                BallBrush.Opacity = opacity;
                renderTarget.FillEllipse(BallBrush, ellipse);
                BallBrush.Dispose();
            }
            else
            {
                Brush RobotBrush = new SolidColorBrush(renderTarget, (state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new Color4(1f, 65f / 255f, 126f / 255f, 255 / 255f) : new Color4(1f, 255f / 255f, 243f / 255f, 62f / 255f));
                Brush BorderBrush = new SolidColorBrush(renderTarget, (state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new Color4(1f, 18f / 255f, 59f / 255f, 160f / 255f) : new Color4(1f, 204f / 255f, 157f / 255f, 0f / 255f));

                RobotBrush.Opacity = opacity;
                BorderBrush.Opacity = opacity;
                if (state.Angle.HasValue)
                {

                    PathGeometry pg = new PathGeometry(d2dfactory);
                    GeometrySink gs = pg.Open();
                    gs.BeginFigure((state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2)), FigureBegin.Filled);
                    ArcSegment arcseg3 = new ArcSegment();
                    arcseg3.ArcSize = ArcSize.Large;
                    arcseg3.EndPoint = (state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2));
                    arcseg3.RotationAngle = (float)Math.PI / 2;
                    arcseg3.Size = new SizeF((float)RobotParameters.OurRobotParams.Diameter / 2, (float)RobotParameters.OurRobotParams.Diameter / 2);
                    arcseg3.SweepDirection = SweepDirection.Clockwise;
                    gs.AddArc(arcseg3);
                    gs.AddLine((state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2)));
                    gs.EndFigure(FigureEnd.Closed);
                    gs.Close();
                    renderTarget.FillGeometry(pg, RobotBrush);
                    renderTarget.DrawGeometry(pg, BorderBrush, 0.01f);
                    pg.Dispose();
                    gs.Dispose();
                }
                else
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Center = (state.Location);
                    ellipse.RadiusY = ellipse.RadiusX = (state.Type == ObjectType.OurRobot) ? (float)RobotParameters.OurRobotParams.Diameter / 2 : (float)RobotParameters.OpponentParams.Diameter / 2;
                    renderTarget.FillEllipse(RobotBrush, ellipse);
                    renderTarget.DrawEllipse(BorderBrush, ellipse, 0.01f);
                }
                //TextBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
                //if (RobotID != 1000)
                //    if (_showTexts)
                //        DrawText(RobotID.ToString(), _textbrush, renderTarget, new System.Drawing.RectangleF(state.Location, new System.Drawing.SizeF(0.05f, 0.09f)));

                RobotBrush.Dispose();
                BorderBrush.Dispose();

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="type"></param>
        /// <param name="renderTarget"></param>
        private void DrawRobot(SingleObjectState state, ObjectType type)
        {
            if (state == null) return;
            if (type == ObjectType.Ball)
            {
                Brush BallBrush = new SolidColorBrush(renderTarget, new Color4(1f, 1f, 129f / 255f, 0f));
                Ellipse ellipse = new Ellipse();
                ellipse.Center = (state.Location);
                ellipse.RadiusY = ellipse.RadiusX = (float)GameParameters.BallDiameter / 2;
                renderTarget.FillEllipse(BallBrush, ellipse);
                BallBrush = new SolidColorBrush(renderTarget, new Color4(1f, 209f / 255f, 89f / 255f, 0f));
                renderTarget.DrawEllipse(BallBrush, ellipse, 0.006f);
                BallBrush.Dispose();
            }
            else
            {
                Brush RobotBrush = new SolidColorBrush(renderTarget, (type == ObjectType.OurRobot) ? new Color4(1f, 65f / 255f, 126f / 255f, 255 / 255f) : new Color4(1f, 255f / 255f, 243f / 255f, 62f / 255f));
                Brush BorderBrush = new SolidColorBrush(renderTarget, (type == ObjectType.OurRobot) ? new Color4(1f, 18f / 255f, 59f / 255f, 160f / 255f) : new Color4(1f, 204f / 255f, 157f / 255f, 0f / 255f));
                if (state.Angle.HasValue)
                {
                    Position2D p1 = state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2);
                    Position2D p2 = state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2);

                    PathGeometry pg = new PathGeometry(d2dfactory);
                    GeometrySink gs = pg.Open();
                    gs.BeginFigure((state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2)), FigureBegin.Filled);
                    ArcSegment arcseg3 = new ArcSegment();
                    arcseg3.ArcSize = ArcSize.Large;
                    arcseg3.EndPoint = (state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2));
                    arcseg3.RotationAngle = (float)Math.PI / 2;
                    arcseg3.Size = new SizeF((float)RobotParameters.OurRobotParams.Diameter / 2, (float)RobotParameters.OurRobotParams.Diameter / 2);
                    arcseg3.SweepDirection = SweepDirection.Clockwise;
                    gs.AddArc(arcseg3);
                    gs.AddLine((state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2)));
                    gs.EndFigure(FigureEnd.Closed);
                    gs.Close();
                    renderTarget.FillGeometry(pg, RobotBrush);
                    renderTarget.DrawGeometry(pg, BorderBrush, 0.01f);
                    pg.Dispose();
                    gs.Dispose();
                }
                else
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Center = (state.Location);
                    ellipse.RadiusY = ellipse.RadiusX = (state.Type == ObjectType.OurRobot) ? (float)RobotParameters.OurRobotParams.Diameter / 2 : (float)RobotParameters.OpponentParams.Diameter / 2;
                    renderTarget.FillEllipse(RobotBrush, ellipse);
                    renderTarget.DrawEllipse(BorderBrush, ellipse, 0.01f);
                }
                RobotBrush.Dispose();
                BorderBrush.Dispose();
                //TextBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
                //if (RobotID != 1000)
                //    if (_showTexts)
                //        DrawText(RobotID.ToString(), _textbrush, renderTarget, new System.Drawing.RectangleF(state.Location, new System.Drawing.SizeF((float)(RobotID.ToString().Length * 0.05), 0.1f)));
            }
        }
        /// <summary>
        /// darw all robot and balls on field
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawAllRobots()
        {

            Brush BlackBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
            if (_model == null) return;
            WorldModel tModel = new WorldModel(_model);
            DrawRobot(tModel.BallState, 0);
            if (tModel.OurRobots != null && DrawOurRobot)
            {
                foreach (var item in tModel.OurRobots.Keys)
                {
                    if (tModel.OurRobots.ContainsKey(item))
                    {
                        DrawRobot(tModel.OurRobots[item], item);
                        if (tModel.OurRobots.ContainsKey(item))
                            DrawText(item.ToString(), BlackBrush, tModel.OurRobots[item].Location);
                    }
                }

            }
            if (tModel.Opponents != null && DrawOppRobot)
            {
                foreach (var item in tModel.Opponents.Keys)
                {
                    if (tModel.Opponents.ContainsKey(item))
                    {
                        DrawRobot(tModel.Opponents[item], item);
                        DrawText(item.ToString(), BlackBrush, tModel.Opponents[item].Location);
                    }
                }

            }
            BlackBrush.Dispose();

        }
        /// <summary>
        /// 
        /// </summary>
        List<StringDraw> textList = new List<StringDraw>();
        /// <summary>
        /// Draw all Drawing Object
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawObjects()
        {
            if (strategyviewer || DrawingObjects.ObjectTree.Count == 0) return;
            Dictionary<string, object> copy = new Dictionary<string, object>();

            DrawingObjects.drawingObject.ToList().ForEach(p =>
            {
                if (p.Value != null && p.Key != null)
                    //if (!copy.ContainsKey(p.Key))
                    copy[p.Key] = p.Value;
            });
            foreach (var item in copy)
            {
                #region draw globals
                if (item.Value.GetType() == typeof(Line))
                {
                    TreeViewModel ti = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                    if (item.Value.As<Line>().IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        Line line = item.Value.As<Line>();
                        Brush b = ToBrush(line.DrawPen.Color);
                        StrokeStyle s = ToStrockStyle(line.DrawPen);
                        renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                        b.Dispose();
                        s.Dispose();

                    }
                    else if (!item.Value.As<Line>().IsShown)
                    {
                        Line line = item.Value.As<Line>();
                        Brush b = ToBrush(line.DrawPen.Color);
                        StrokeStyle s = ToStrockStyle(line.DrawPen);
                        renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                        b.Dispose();
                        s.Dispose();
                    }
                }

                else if (item.Value.GetType() == typeof(Position2D))
                {
                    TreeViewModel ti = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                    if (item.Value.As<Position2D>().IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        Position2D pos = item.Value.As<Position2D>();
                        DrawToken(pos);
                    }
                    else if (!item.Value.As<Position2D>().IsShown)
                    {
                        Position2D pos = item.Value.As<Position2D>();
                        DrawToken(pos);


                    }
                }
                else if (item.Value.GetType() == typeof(Circle))
                {
                    TreeViewModel ti = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                    if (item.Value.As<Circle>().IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        Circle circle = item.Value.As<Circle>();
                        Brush b = ToBrush(circle.DrawPen.Color);
                        if (circle.IsFill)
                        {
                            b.Opacity = circle.Opacity;
                            renderTarget.FillEllipse(b, new Ellipse()
                            {
                                Center = circle.Center,
                                RadiusX = (float)circle.Radious,
                                RadiusY = (float)circle.Radious
                            });

                        }
                        else
                        {
                            StrokeStyle s = ToStrockStyle(circle.DrawPen);
                            renderTarget.DrawEllipse(b
                                , new Ellipse() { Center = (circle.Center), RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious }, circle.DrawPen.Width, s);
                            s.Dispose();
                        }
                        b.Dispose();



                    }
                    else if (!item.Value.As<Circle>().IsShown)
                    {
                        Circle circle = item.Value.As<Circle>();
                        Brush b = ToBrush(circle.DrawPen.Color);
                        if (circle.IsFill)
                        {

                            b.Opacity = circle.Opacity;
                            renderTarget.FillEllipse(b, new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious });

                        }
                        else
                        {
                            StrokeStyle s = ToStrockStyle(circle.DrawPen);
                            renderTarget.DrawEllipse(b, new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious }, circle.DrawPen.Width, s);
                            s.Dispose();
                        }

                        b.Dispose();
                    }
                }
                else if (item.Value.GetType() == typeof(FlatRectangle))
                {
                    TreeViewModel ti = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                    if (item.Value.As<FlatRectangle>().IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        FlatRectangle rect = item.Value.As<FlatRectangle>();
                        Brush b = ToBrush(rect.FillColor);
                        if (rect.IsFill)
                        {

                            b.Opacity = rect.Opacity;
                            renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                        }
                        else
                        {
                            renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                        }
                        b.Dispose();
                    }
                    else if (!item.Value.As<FlatRectangle>().IsShown)
                    {
                        FlatRectangle rect = item.Value.As<FlatRectangle>();
                        Brush b = ToBrush(rect.FillColor);
                        if (rect.IsFill)
                        {
                            b.Opacity = rect.Opacity;
                            renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                        }
                        else
                        {
                            Brush b1 = ToBrush(rect.BorderColor);
                            renderTarget.DrawRectangle(b1, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                        }
                        b.Dispose();
                    }
                }
                else if (item.Value.GetType() == typeof(SingleObjectState))
                {
                    TreeViewModel ti = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                    if (item.Value.As<SingleObjectState>().IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        SingleObjectState line = item.Value.As<SingleObjectState>();
                        DrawRobot(line, 0, .4f);
                    }
                    else if (!item.Value.As<SingleObjectState>().IsShown)
                    {
                        SingleObjectState line = item.Value.As<SingleObjectState>();
                        DrawRobot(line, 0, .4f);
                    }
                }
                else if (item.Value.GetType() == typeof(StringDraw))
                {
                    TreeViewModel ti = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                    StringDraw text = item.Value.As<StringDraw>();
                    Brush b = ToBrush(text.TextColor);
                    if (text.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        if (text.OnTop)
                            textList.Add(text);
                        else
                            DrawText(text.Content, b, text.Posiotion);
                    }
                    else if (!text.IsShown)
                    {
                        if (text.OnTop)
                            textList.Add(text);
                        else
                            DrawText(text.Content, b, text.Posiotion);

                    }
                    b.Dispose();
                }

                else if (item.Value.GetType() == typeof(DrawRegion))
                {
                    TreeViewModel ti = TreeViewModel.GetItemByName(item.Key, DrawingObjects.ObjectTree[0]);
                    if (item.Value.As<DrawRegion>().IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        DrawRegion region = item.Value.As<DrawRegion>();
                        DrawPath(region);
                    }
                    else if (!item.Value.As<DrawRegion>().IsShown)
                    {
                        DrawRegion region = item.Value.As<DrawRegion>();
                        DrawPath(region);
                    }
                }

                #endregion

                #region draw collection
                else if (item.Value.GetType() == typeof(DrawCollection))
                {
                    DrawCollection cur = item.Value.As<DrawCollection>();

                    foreach (var item1 in item.Value.As<DrawCollection>().drawingObject.ToList())
                    {
                        TreeViewModel curtree = TreeViewModel.GetItemByName(item1.Key, TreeViewModel.GetItemByName(item.Key, TreeViewModel.GetItemByName("Draw Collections", DrawingObjects.ObjectTree[0])));
                        if (cur.drawingObject.ContainsKey(item1.Key))
                        {
                            if (item1.Value.GetType() == typeof(Line))
                            {
                                Line line = item1.Value.As<Line>();
                                Brush b = ToBrush(line.DrawPen.Color);
                                StrokeStyle s = ToStrockStyle(line.DrawPen);
                                if (item1.Value.As<Line>().IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                                {
                                    renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                                }
                                else if (!item1.Value.As<Line>().IsShown)
                                {
                                    renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                                }
                                b.Dispose();
                                s.Dispose();
                            }
                            if (item1.Value.GetType() == typeof(Position2D))
                            {

                                if (item1.Value.As<Position2D>().IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                                {
                                    Position2D pos = item.Value.As<Position2D>();
                                    DrawToken(pos);
                                }
                                else if (!item1.Value.As<Position2D>().IsShown)
                                {
                                    Position2D pos = item1.Value.As<Position2D>();
                                    DrawToken(pos);
                                }
                            }
                            else if (item1.Value.GetType() == typeof(Circle))
                            {
                                if (item1.Value.As<Circle>().IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                                {
                                    Circle circle = item1.Value.As<Circle>();
                                    Brush b = ToBrush(circle.DrawPen.Color);
                                    if (circle.IsFill)
                                    {
                                        b.Opacity = circle.Opacity;
                                        renderTarget.FillEllipse(b, new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious });
                                    }
                                    else
                                    {
                                        StrokeStyle s = ToStrockStyle(circle.DrawPen);
                                        renderTarget.DrawEllipse(b,
                                            new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious }, circle.DrawPen.Width,
                                            s);
                                        s.Dispose();
                                    }
                                    b.Dispose();
                                }
                                else if (!item1.Value.As<Circle>().IsShown)
                                {
                                    Circle circle = item1.Value.As<Circle>();
                                    Brush b = ToBrush(circle.DrawPen.Color);
                                    if (circle.IsFill)
                                    {

                                        b.Opacity = circle.Opacity;
                                        renderTarget.FillEllipse(b, new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious });
                                    }
                                    else
                                    {
                                        StrokeStyle s = ToStrockStyle(circle.DrawPen);
                                        renderTarget.DrawEllipse(b, new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious }, circle.DrawPen.Width,
                                            s);
                                        s.Dispose();
                                    }
                                    b.Dispose();
                                }
                            }
                            else if (item1.Value.GetType() == typeof(SingleObjectState))
                            {
                                if (item1.Value.As<SingleObjectState>().IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                                {
                                    SingleObjectState sin = item1.Value.As<SingleObjectState>();
                                    DrawRobot(sin, 0, .4f);
                                }
                                else if (!item1.Value.As<SingleObjectState>().IsShown)
                                {
                                    SingleObjectState sin = item1.Value.As<SingleObjectState>();
                                    DrawRobot(sin, 0, .4f);
                                }
                            }
                            else if (item1.Value.GetType() == typeof(FlatRectangle))
                            {
                                FlatRectangle rect = item1.Value.As<FlatRectangle>();
                                Brush b = ToBrush(rect.FillColor);
                                if (item1.Value.As<FlatRectangle>().IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                                {
                                    if (rect.IsFill)
                                    {
                                        b.Opacity = rect.Opacity;
                                        renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                                    }
                                    else
                                    {
                                        renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                                    }
                                }
                                else if (!item1.Value.As<FlatRectangle>().IsShown)
                                {
                                    if (rect.IsFill)
                                    {
                                        b.Opacity = rect.Opacity;
                                        renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                                    }
                                    else
                                    {
                                        renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                                    }

                                }
                                b.Dispose();
                            }
                            else if (item1.Value.GetType() == typeof(DrawRegion))
                            {
                                DrawRegion text = item1.Value.As<DrawRegion>();
                                if (item1.Value.As<DrawRegion>().IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                                {
                                    DrawPath(text);
                                }
                                else if (!item1.Value.As<DrawRegion>().IsShown)
                                {
                                    DrawPath(text);
                                }
                            }
                            else if (item1.Value.GetType() == typeof(StringDraw))
                            {
                                StringDraw text = item1.Value.As<StringDraw>();
                                Brush b = ToBrush(text.TextColor);
                                if (item1.Value.As<StringDraw>().IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                                {
                                    if (text.OnTop)
                                        textList.Add(text);
                                    else
                                        DrawText(text.Content, b, text.Posiotion);
                                }
                                else if (!item1.Value.As<StringDraw>().IsShown)
                                {
                                    if (text.OnTop)
                                        textList.Add(text);
                                    else
                                        DrawText(text.Content, b, text.Posiotion);
                                }
                                b.Dispose();
                            }
                        }
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawAllText()
        {
            foreach (var item in textList)
            {
                Brush b = ToBrush(item.TextColor);
                DrawText(item.Content, b, item.Posiotion);
                b.Dispose();
            }
            textList.Clear();
        }
        /// <summary>
        /// draw text
        /// </summary>
        /// <param name="text">text that must be draw</param>
        /// <param name="brush">text brush</param>
        /// <param name="renderTarget"></param>
        /// <param name="pos">text position</param>
        private void DrawText(string text, Brush brush, Position2D pos)
        {
            if (!ShowTexts) return;
            SlimDX.DirectWrite.TextFormat textFormat;
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF(pos, new System.Drawing.SizeF(text.Length * 0.05f, 0.1f));

            rect.Location = new System.Drawing.PointF(rect.Location.X - rect.Width / 2, rect.Location.Y - rect.Height / 2);
            textFormat = new SlimDX.DirectWrite.TextFormat(dwriteFactory, "Berlin Sans FB Demi",
                SlimDX.DirectWrite.FontWeight.Bold
                , SlimDX.DirectWrite.FontStyle.Normal
                , SlimDX.DirectWrite.FontStretch.Normal, 0.08f, "t");
            textFormat.TextAlignment = SlimDX.DirectWrite.TextAlignment.Center;
            textFormat.ParagraphAlignment = SlimDX.DirectWrite.ParagraphAlignment.Center;
            //if (Mode == FieldOrientation.Verticaly)
            //{
            renderTarget.Transform = MatrixCalculator.Rotate(new Position2D(rect.Location.X + rect.Width / 2, rect.Location.Y + rect.Height / 2), renderTarget.Transform, -90, System.Drawing.Drawing2D.MatrixOrder.Append);
            renderTarget.DrawText(text, textFormat, rect, brush);
            renderTarget.Transform = MatrixCalculator.Rotate(new Position2D(rect.Location.X + rect.Width / 2, rect.Location.Y + rect.Height / 2), renderTarget.Transform, 90, System.Drawing.Drawing2D.MatrixOrder.Append);
            //}
            //else if (Mode == FieldOrientation.Horzintaly)
            //renderTarget.DrawText(text, textFormat, new RectF(rect.Left, rect.Top, rect.Right, rect.Bottom), brush);
            textFormat.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawOtherBalls()
        {
            if (_currentWrapper == null || _currentWrapper.AllBalls == null) return;
            Brush BlackBrush = new SolidColorBrush(renderTarget, new Color4(Color.Black));
            foreach (var item in _currentWrapper.AllBalls)
                renderTarget.DrawEllipse(BlackBrush, new Ellipse().GetEllipse((item.Value), (float)GameParameters.BallDiameter / 2 + 0.01f, (float)GameParameters.BallDiameter / 2 + 0.01f), 0.01f);
            BlackBrush.Dispose();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public void AddAnalizeObject(Object obj)
        {
            AnalizeOjects.Add(obj);
            this.Invalidate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawPainted()
        {
            foreach (var item in AnalizeOjects)
            {
                if (item.GetType() == typeof(Line))
                {
                    System.Drawing.Color c = item.As<Line>().DrawPen.Color;
                    float width = item.As<Line>().DrawPen.Width;
                    Brush b = new SolidColorBrush(renderTarget, new Color4(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f));
                    renderTarget.DrawLine(b, (item.As<Line>().Head), (item.As<Line>().Tail), width);
                    b.Dispose();
                }
                else if (item.GetType() == typeof(Circle))
                {
                    System.Drawing.Color c = item.As<Circle>().DrawPen.Color;
                    Brush b = new SolidColorBrush(renderTarget, new Color4(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f));
                    float width = item.As<Circle>().DrawPen.Width;
                    renderTarget.DrawEllipse(b, new Ellipse().GetEllipse(item.As<Circle>().Center, (float)item.As<Circle>().Radious, (float)item.As<Circle>().Radious), width);
                    b.Dispose();
                }
                else if (item.GetType() == typeof(SingleObjectState))
                {
                    DrawRobot(item.As<SingleObjectState>(), item.As<SingleObjectState>().Type);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Undo()
        {
            if (AnalizeOjects.Count > 0)
            {
                AnalizeOjects.RemoveAt(AnalizeOjects.Count - 1);

                momentlyObject = null;
                this.Invalidate();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            AnalizeOjects.Clear();
            momentlyObject = null;
            if (!LogPlayerMode)
            {
                Model = new WorldModel();
                Model.OurRobots = new Dictionary<int, SingleObjectState>();
                Model.Opponents = new Dictionary<int, SingleObjectState>();
                Model.BallState = new SingleObjectState();
            }
            this.Invalidate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void DrawLine(Line line)
        {
            momentlyObject = line;
            this.Invalidate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void DrawRobot(SingleObjectState state)
        {
            momentlyObject = state;
            this.Invalidate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        public void DrawCircle(Circle circle)
        {
            momentlyObject = circle;
            this.Invalidate();
        }
        /// <summary>
        /// 
        /// </summary>
        public void ResetMomentlyObject()
        {
            momentlyObject = null;
            this.Invalidate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="renderTarget"></param>
        private void DrawObj(object item)
        {
            if (item == null) return;
            if (item.GetType() == typeof(Line))
            {
                System.Drawing.Color c = item.As<Line>().DrawPen.Color;
                float width = item.As<Line>().DrawPen.Width;
                Brush b = new SolidColorBrush(renderTarget, new Color4(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f));
                renderTarget.DrawLine(b, (item.As<Line>().Head), (item.As<Line>().Tail), width);
                b.Dispose();
            }
            else if (item.GetType() == typeof(Circle))
            {
                System.Drawing.Color c = item.As<Circle>().DrawPen.Color;
                float width = item.As<Circle>().DrawPen.Width;
                Brush b = new SolidColorBrush(renderTarget, new Color4(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f));
                renderTarget.DrawEllipse(b, new Ellipse().GetEllipse(item.As<Circle>().Center, (float)item.As<Circle>().Radious, (float)item.As<Circle>().Radious), width);
                b.Dispose();
            }
            else if (item.GetType() == typeof(SingleObjectState))
            {
                DrawRobot(item.As<SingleObjectState>(), item.As<SingleObjectState>().Type);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="MouseLocation"></param>
        /// <returns></returns>
        private Position2D PixelToMetric(System.Drawing.Point MouseLocation)
        {
            double X = MouseLocation.X;
            double Y = MouseLocation.Y;
            if (_orentation == FieldOrientation.Verticaly)
            {
                X = (X - Transform.Value.M31) / Transform.Value.M21;
                Y = (Y - Transform.Value.M32) / Transform.Value.M12;
                return new Position2D(Y, X);
            }
            else
            {
                X = (X - Transform.Value.M31) / Transform.Value.M11;
                Y = (Y - Transform.Value.M32) / Transform.Value.M22;
                return new Position2D(X, Y);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="val"></param>
        private void DrawObjects(bool analizeMode)
        {
            if (strategyviewer || !analizeMode && LocalDrawingObject.ObjectTree.Count == 0) return;
            foreach (var item in LocalDrawingObject.drawingObject.Keys.ToList())
            {
                #region ____draw globals____

                #region Lines
                if (LocalDrawingObject.drawingObject[item].GetType() == typeof(Line))
                {
                    LogTreeViewModel ti = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item, LocalDrawingObject.ObjectTree[0]);
                    Line line = LocalDrawingObject.drawingObject[item].As<Line>();
                    Brush b = ToBrush(line.DrawPen.Color);
                    StrokeStyle s = ToStrockStyle(line.DrawPen);
                    if (line.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                        renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                    else if (!line.IsShown)
                    {
                        renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                        if (LocalDrawingObject.drawingObject.ContainsKey(item))
                            LocalDrawingObject.drawingObject.Remove(item);
                    }
                    b.Dispose();
                    s.Dispose();
                }
                #endregion

                #region Circles
                else if (LocalDrawingObject.drawingObject[item].GetType() == typeof(Circle))
                {
                    LogTreeViewModel ti = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item, LocalDrawingObject.ObjectTree[0]);
                    Circle circle = LocalDrawingObject.drawingObject[item].As<Circle>();
                    Brush b = ToBrush(circle.DrawPen.Color);
                    if (circle.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        if (circle.IsFill)
                        {

                            b.Opacity = circle.Opacity;
                            renderTarget.FillEllipse(b, new Ellipse().GetEllipse(circle.Center, (float)circle.Radious, (float)circle.Radious));
                            b.Dispose();
                        }
                        else
                        {
                            StrokeStyle s = ToStrockStyle(circle.DrawPen);
                            renderTarget.DrawEllipse(b, new Ellipse().GetEllipse(circle.Center, (float)circle.Radious, (float)circle.Radious),
                                circle.DrawPen.Width, s);
                            s.Dispose();
                        }
                    }
                    else if (!circle.IsShown)
                    {
                        if (circle.IsFill)
                        {
                            b.Opacity = circle.Opacity;
                            renderTarget.FillEllipse(b, new Ellipse().GetEllipse(circle.Center, (float)circle.Radious, (float)circle.Radious));
                        }
                        else
                        {
                            StrokeStyle s = ToStrockStyle(circle.DrawPen);
                            renderTarget.DrawEllipse(b, new Ellipse().GetEllipse(circle.Center, (float)circle.Radious, (float)circle.Radious),
                                circle.DrawPen.Width, s);
                            s.Dispose();
                        }

                        if (LocalDrawingObject.drawingObject.ContainsKey(item))
                            LocalDrawingObject.drawingObject.Remove(item);
                    }
                    b.Dispose();
                }
                #endregion

                #region Rectangles
                else if (LocalDrawingObject.drawingObject[item].GetType() == typeof(FlatRectangle))
                {
                    LogTreeViewModel ti = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item, LocalDrawingObject.ObjectTree[0]);
                    FlatRectangle rect = LocalDrawingObject.drawingObject[item].As<FlatRectangle>();

                    if (rect.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        if (rect.IsFill)
                        {
                            Brush b = ToBrush(rect.FillColor);
                            b.Opacity = rect.Opacity;
                            renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                            b.Dispose();
                        }
                        else
                        {
                            Brush b = ToBrush(rect.BorderColor);
                            renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                            b.Dispose();
                        }
                    }
                    else if (!rect.IsShown)
                    {
                        if (rect.IsFill)
                        {
                            Brush b = ToBrush(rect.FillColor);
                            b.Opacity = rect.Opacity;
                            renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                            b.Dispose();
                        }
                        else
                        {
                            Brush b = ToBrush(rect.BorderColor);
                            renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                            b.Dispose();
                        }
                        if (LocalDrawingObject.drawingObject.ContainsKey(item))
                            LocalDrawingObject.drawingObject.Remove(item);
                    }
                }
                #endregion

                #region SingleObjectStates
                else if (LocalDrawingObject.drawingObject[item].GetType() == typeof(SingleObjectState))
                {
                    LogTreeViewModel ti = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item, LocalDrawingObject.ObjectTree[0]);
                    SingleObjectState single = LocalDrawingObject.drawingObject[item].As<SingleObjectState>();
                    if (single.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        DrawRobot(single, 0, .4f);
                    }
                    else if (!single.IsShown)
                    {
                        DrawRobot(single, 0, .4f);
                        if (LocalDrawingObject.drawingObject.ContainsKey(item))
                            LocalDrawingObject.drawingObject.Remove(item);
                    }
                }
                #endregion

                #region StringDraws
                else if (LocalDrawingObject.drawingObject[item].GetType() == typeof(StringDraw))
                {
                    LogTreeViewModel ti = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item, LocalDrawingObject.ObjectTree[0]);
                    StringDraw text = LocalDrawingObject.drawingObject[item].As<StringDraw>();
                    Brush b = ToBrush(text.TextColor);
                    if (text.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        DrawText(text.Content, b, text.Posiotion);
                    }
                    else if (!text.IsShown)
                    {
                        DrawText(text.Content, b, text.Posiotion);
                        if (LocalDrawingObject.drawingObject.ContainsKey(item))
                            LocalDrawingObject.drawingObject.Remove(item);
                    }
                    b.Dispose();
                }
                #endregion

                #region DrawRegion
                else if (LocalDrawingObject.drawingObject[item].GetType() == typeof(DrawRegion))
                {
                    LogTreeViewModel ti = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item, LocalDrawingObject.ObjectTree[0]);
                    DrawRegion region = LocalDrawingObject.drawingObject[item].As<DrawRegion>();
                    if (region.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        DrawPath(region);
                    }
                    else if (!region.IsShown)
                    {
                        DrawPath(region);
                        if (LocalDrawingObject.drawingObject.ContainsKey(item))
                            LocalDrawingObject.drawingObject.Remove(item);
                    }
                }
                #endregion

                #region DrawPosition
                else if (LocalDrawingObject.drawingObject[item].GetType() == typeof(Position2D))
                {
                    LogTreeViewModel ti = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item, LocalDrawingObject.ObjectTree[0]);
                    Position2D region = LocalDrawingObject.drawingObject[item].As<Position2D>();
                    if (region.IsShown && ti != null && ti.IsChecked.HasValue && ti.IsChecked.Value)
                    {
                        DrawToken(region);
                    }
                    else if (!region.IsShown)
                    {
                        DrawToken(region);
                        if (LocalDrawingObject.drawingObject.ContainsKey(item))
                            LocalDrawingObject.drawingObject.Remove(item);
                    }
                }
                #endregion

                #endregion
                #region ___draw collection___
                else if (LocalDrawingObject.drawingObject[item].GetType() == typeof(DrawCollection))
                {
                    LogTreeViewModel col = LogTreeViewModel.GetItemByName(item, LogTreeViewModel.GetItemByName("Draw Collections", LocalDrawingObject.ObjectTree[0]));
                    if (col == null || !col.IsChecked.HasValue || !col.IsChecked.Value) continue;
                    foreach (var item1 in LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Keys.ToList())
                    {
                        DrawCollection cur = LocalDrawingObject.drawingObject[item].As<DrawCollection>();
                        LogTreeViewModel curtree = (LogAnalizerMode) ? null : LogTreeViewModel.GetItemByName(item1, LogTreeViewModel.GetItemByName(item, LogTreeViewModel.GetItemByName("Draw Collections", LocalDrawingObject.ObjectTree[0])));

                        #region Lines
                        if (cur.drawingObject[item1].GetType() == typeof(Line))
                        {
                            Line line = cur.drawingObject[item1].As<Line>();
                            Brush b = ToBrush(line.DrawPen.Color);
                            StrokeStyle s = ToStrockStyle(line.DrawPen);
                            if (line.IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                            {
                                renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                            }
                            else if (!line.IsShown)
                            {
                                renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                                if (LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.ContainsKey(item))
                                    LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Remove(item);
                            }
                            b.Dispose();
                            s.Dispose();
                        }
                        #endregion

                        #region circles
                        else if (cur.drawingObject[item1].GetType() == typeof(Circle))
                        {
                            Circle circle = cur.drawingObject[item1].As<Circle>();
                            Brush b = ToBrush(circle.DrawPen.Color);
                            if (circle.IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                            {
                                if (circle.IsFill)
                                {

                                    b.Opacity = circle.Opacity;
                                    renderTarget.FillEllipse(b, new Ellipse().GetEllipse((circle.Center), (float)circle.Radious, (float)circle.Radious));
                                }
                                else
                                {
                                    StrokeStyle s = ToStrockStyle(circle.DrawPen);
                                    renderTarget.DrawEllipse(b, new Ellipse().GetEllipse((circle.Center), (float)circle.Radious, (float)circle.Radious), circle.DrawPen.Width, s);
                                    s.Dispose();
                                }
                            }
                            else if (!circle.IsShown)
                            {
                                if (circle.IsFill)
                                {

                                    b.Opacity = circle.Opacity;
                                    renderTarget.FillEllipse(b, new Ellipse().GetEllipse((circle.Center), (float)circle.Radious, (float)circle.Radious));
                                }
                                else
                                {
                                    StrokeStyle s = ToStrockStyle(circle.DrawPen);
                                    renderTarget.DrawEllipse(b, new Ellipse().GetEllipse((circle.Center), (float)circle.Radious, (float)circle.Radious), circle.DrawPen.Width,
                                        s);
                                    s.Dispose();
                                }
                                if (LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.ContainsKey(item))
                                    LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Remove(item);
                            }
                            b.Dispose();
                        }
                        #endregion

                        #region SingleObjectStates
                        else if (cur.drawingObject[item1].GetType() == typeof(SingleObjectState))
                        {
                            SingleObjectState sin = cur.drawingObject[item1].As<SingleObjectState>();
                            if (sin.IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                            {
                                DrawRobot(sin, 0, .4f);
                            }
                            else if (!sin.IsShown)
                            {
                                DrawRobot(sin, 0, .4f);
                                if (LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.ContainsKey(item))
                                    LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Remove(item);
                            }

                        }
                        #endregion

                        #region FlatRectangle
                        else if (cur.drawingObject[item1].GetType() == typeof(FlatRectangle))
                        {
                            FlatRectangle rect = cur.drawingObject[item1].As<FlatRectangle>();
                            if (rect.IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                            {
                                if (rect.IsFill)
                                {
                                    Brush b = ToBrush(rect.FillColor);
                                    b.Opacity = rect.Opacity;
                                    renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                                    b.Dispose();
                                }
                                else
                                {
                                    Brush b = ToBrush(rect.BorderColor);
                                    renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                                    b.Dispose();
                                }
                            }
                            else if (!rect.IsShown)
                            {
                                if (rect.IsFill)
                                {
                                    Brush b = ToBrush(rect.FillColor);
                                    b.Opacity = rect.Opacity;
                                    renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                                    b.Dispose();
                                }
                                else
                                {
                                    Brush b = ToBrush(rect.BorderColor);
                                    renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                                    b.Dispose();
                                }
                                if (LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.ContainsKey(item))
                                    LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Remove(item);
                            }
                        }
                        #endregion

                        #region StringDraw
                        else if (cur.drawingObject[item1].GetType() == typeof(StringDraw))
                        {
                            StringDraw text = cur.drawingObject[item1].As<StringDraw>();
                            Brush b = ToBrush(text.TextColor);
                            if (text.IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                            {
                                DrawText(text.Content, b, text.Posiotion);
                            }
                            else if (!text.IsShown)
                            {
                                DrawText(text.Content, b, text.Posiotion);
                                if (LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.ContainsKey(item))
                                    LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Remove(item);
                            }
                            b.Dispose();
                        }
                        #endregion

                        #region DrawRegion
                        else if (cur.drawingObject[item1].GetType() == typeof(DrawRegion))
                        {

                            DrawRegion region = cur.drawingObject[item1].As<DrawRegion>();
                            if (region.IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                            {
                                DrawPath(region);
                            }
                            else if (!region.IsShown)
                            {
                                DrawPath(region);
                                if (LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.ContainsKey(item))
                                    LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Remove(item);
                            }
                        }
                        #endregion

                        #region DrawPosition
                        else if (cur.drawingObject[item1].GetType() == typeof(Position2D))
                        {

                            Position2D region = cur.drawingObject[item1].As<Position2D>();
                            if (region.IsShown && curtree != null && curtree.IsChecked.HasValue && curtree.IsChecked.Value)
                            {
                                DrawToken(region);
                            }
                            else if (!region.IsShown)
                            {
                                DrawToken(region);
                                if (LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.ContainsKey(item))
                                    LocalDrawingObject.drawingObject[item].As<DrawCollection>().drawingObject.Remove(item);
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool SelectedBallPosition(out Position2D pos)
        {
            Position2D loc = PixelToMetric(MousePosition);
            pos = new Position2D();
            selected = AnalizeOjects.Where(w => w.GetType() == typeof(SingleObjectState) &&
                         Math.Pow(w.As<SingleObjectState>().Location.X - loc.X, 2) + Math.Pow(w.As<SingleObjectState>().Location.Y - loc.Y, 2)
                        < Math.Pow(GameParameters.BallDiameter / 2, 2)).FirstOrDefault();
            if (selected != null)
                return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawGoalsDepth()
        {
            //Our Goal Fill
            PathGeometry pg1 = new PathGeometry(d2dfactory);
            GeometrySink gs1 = pg1.Open();
            gs1.BeginFigure(GameParameters.OurGoalLeft, FigureBegin.Filled);
            gs1.AddLine((GameParameters.OurGoalLeft + new Vector2D(GameParameters.GoalDepth, 0)));
            gs1.AddLine((GameParameters.OurGoalRight + new Vector2D(GameParameters.GoalDepth, 0)));
            gs1.AddLine(GameParameters.OurGoalRight);
            gs1.EndFigure(FigureEnd.Closed);
            gs1.Close();
            if (Model != null)
            {
                Brush helpBrush = new SolidColorBrush(renderTarget, (true ^ Model.OurMarkerISYellow) ? new Color4(1f, 65f / 255f, 126f / 255f, 255 / 255f) : new Color4(1f, 255f / 255f, 243f / 255f, 62f / 255f));
                helpBrush.Opacity = 0.4f;
                renderTarget.FillGeometry(pg1, helpBrush);
                helpBrush.Dispose();
            }

            //Opp Goal Fill
            PathGeometry pg2 = new PathGeometry(d2dfactory);
            GeometrySink gs2 = pg2.Open();
            gs2.BeginFigure(GameParameters.OppGoalLeft, FigureBegin.Filled);
            gs2.AddLine((GameParameters.OppGoalLeft + new Vector2D(-GameParameters.GoalDepth, 0)));
            gs2.AddLine((GameParameters.OppGoalRight + new Vector2D(-GameParameters.GoalDepth, 0)));
            gs2.AddLine(GameParameters.OppGoalRight);
            gs2.EndFigure(FigureEnd.Closed);
            gs2.Close();
            if (Model != null)
            {
                Brush helpBrush = new SolidColorBrush(renderTarget, (false ^ Model.OurMarkerISYellow) ? new Color4(1f, 65f / 255f, 126f / 255f, 255 / 255f) : new Color4(1f, 255f / 255f, 243f / 255f, 62f / 255f));
                helpBrush.Opacity = 0.4f;
                renderTarget.FillGeometry(pg2, helpBrush);
                helpBrush.Dispose();
            }
            pg1.Dispose();
            pg2.Dispose();
            gs1.Dispose();
            gs2.Dispose();
        }
        /// <summary>
        /// Draw visual effects
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawAccordingToStatus()
        {
            if (Model == null) return;

            if (Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam || Model.Status == GameStatus.IndirectFreeKick_Opponent || Model.Status == GameStatus.DirectFreeKick_Opponent || Model.Status == GameStatus.Stop)
            {
                Brush RobotBrush = null, BorderBrush = null;
                if (Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam || Model.Status == GameStatus.IndirectFreeKick_Opponent || Model.Status == GameStatus.DirectFreeKick_Opponent)
                {
                    RobotBrush = new SolidColorBrush(renderTarget, ((Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam) ^ Model.OurMarkerISYellow) ? new Color4(65f / 255f, 126f / 255f, 255 / 255f, 1f) : new Color4(1f, 243f / 255f, 62f / 255f, 1f));
                    BorderBrush = new SolidColorBrush(renderTarget, ((Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam) ^ Model.OurMarkerISYellow) ? new Color4(18f / 255f, 59f / 255f, 160f / 255f, 1f) : new Color4(0f / 255f, 204f / 255f, 157f / 255f));
                }
                else
                {
                    RobotBrush = new SolidColorBrush(renderTarget, new Color4(1f, 0f / 255f, 0f / 255f, 1f));
                    BorderBrush = new SolidColorBrush(renderTarget, new Color4(125f / 255f, 0f / 255f, 0f / 255f, 1f));
                }
                RobotBrush.Opacity = 0.1f;
                BorderBrush.Opacity = 0.5f;
                Circle cir = new Circle(new Position2D(0.5f * Math.Cos((float)_frameCount / 300f) + Model.BallState.Location.X, 0.5f * Math.Sin((float)_frameCount / 300f) + Model.BallState.Location.Y), 0.01f);
                _hist2.Add(cir);
                int count = 0;
                renderTarget.FillEllipse(RobotBrush, new Ellipse().GetEllipse(Model.BallState.Location, 0.5f, 0.5f));
                foreach (Circle item in _hist2)
                {
                    BorderBrush.Opacity = (float)count / 150;
                    BorderBrush.Opacity = Math.Min(BorderBrush.Opacity, 0.05f);
                    renderTarget.FillEllipse(BorderBrush, new Ellipse().GetEllipse(item.Center, (float)item.Radious, (float)item.Radious));
                    count++;
                }
                if (_hist2.Count > 500)
                    _hist2.RemoveAt(0);
                RobotBrush.Dispose();
                BorderBrush.Dispose();
            }
            else
                _frameCount = 0;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PathData"></param>
        /// <param name="renderTarget"></param>
        private void DrawPath(DrawRegion val)
        {
            if (val.Path == null || val.Path.Count == 0) return;
            if (val.Filled)
            {
                PathGeometry pg = new PathGeometry(d2dfactory);
                GeometrySink gs = pg.Open();
                gs.BeginFigure(val.Path[0], FigureBegin.Filled);
                for (int i = 1; i < val.Path.Count; i++)
                    gs.AddLine(val.Path[i]);
                gs.EndFigure(FigureEnd.Closed);
                gs.Close();
                Brush b = ToBrush(val.FillColor);
                b.Opacity = val.Opacity;
                renderTarget.FillGeometry(pg, b);
                pg.Dispose();
                gs.Dispose();
                b.Dispose();
            }
            else
            {
                Brush helpBrush = ToBrush(val.BorderColor);
                for (int i = 1; i < val.Path.Count; i++)
                    renderTarget.DrawLine(helpBrush, val.Path[i - 1], val.Path[i], 0.01f);
                helpBrush.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawSelectedRobotID()
        {
            if (SelectedRobotID.HasValue && Model.OurRobots.ContainsKey(SelectedRobotID.Value))
            {
                if (Model == null) return;

                SingleObjectState state = Model.OurRobots[SelectedRobotID.Value];
                Brush RobotBrush = new SolidColorBrush(renderTarget, new Color4(1, 0, 0, 1f));
                Brush BorderBrush = new SolidColorBrush(renderTarget, (state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new Color4(18f / 255f, 59f / 255f, 160f / 255f, 1f) : new Color4(204f / 255f, 157f / 255f, 0f / 255f, 1f));

                RobotBrush.Opacity = 0.4f;
                BorderBrush.Opacity = 0.4f;
                if (state.Angle.HasValue)
                {

                    PathGeometry pg = new PathGeometry(d2dfactory);
                    GeometrySink gs = pg.Open();
                    gs.BeginFigure((state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2)), FigureBegin.Filled);
                    ArcSegment arcseg3 = new ArcSegment();
                    arcseg3.ArcSize = ArcSize.Large;
                    arcseg3.EndPoint = (state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2));
                    arcseg3.RotationAngle = (float)Math.PI / 2;
                    arcseg3.Size = new SizeF((float)RobotParameters.OurRobotParams.Diameter / 2, (float)RobotParameters.OurRobotParams.Diameter / 2);
                    arcseg3.SweepDirection = SweepDirection.Clockwise;
                    gs.AddArc(arcseg3);
                    gs.AddLine((state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2)));
                    gs.EndFigure(FigureEnd.Closed);
                    gs.Close();
                    renderTarget.FillGeometry(pg, RobotBrush);
                    renderTarget.DrawGeometry(pg, BorderBrush, 0.01f);
                    pg.Dispose();
                    gs.Dispose();
                }
                else
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Center = (state.Location);
                    ellipse.RadiusY = ellipse.RadiusX = (state.Type == ObjectType.OurRobot) ? (float)RobotParameters.OurRobotParams.Diameter / 2 : (float)RobotParameters.OpponentParams.Diameter / 2;
                    renderTarget.FillEllipse(RobotBrush, ellipse);
                    renderTarget.DrawEllipse(BorderBrush, ellipse, 0.01f);
                }
                BorderBrush.Dispose();
                RobotBrush.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="pos"></param>
        private void DrawToken(Position2D pos)
        {
            Line l1 = new Line(new Position2D(pos.X - 0.02, pos.Y), new Position2D(pos.X + 0.02, pos.Y), new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.01f));
            Line l2 = new Line(new Position2D(pos.X, pos.Y - 0.02), new Position2D(pos.X, pos.Y + 0.02), new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.01f));
            Brush b = ToBrush(pos.DrawColor);
            renderTarget.DrawLine(b, l1.Head, l1.Tail, 0.005f);
            renderTarget.DrawLine(b, l2.Head, l2.Tail, 0.005f);
            renderTarget.DrawEllipse(b, new Ellipse().GetEllipse(pos, 0.02f, 0.02f), 0.01f);
            b.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        private void DrawVisualizerObjects()
        {
            foreach (var item1 in VisualizerObject.drawingObject.ToList())
            {
                if (VisualizerObject.drawingObject.ContainsKey(item1.Key))
                {
                    if (item1.Value.GetType() == typeof(Line))
                    {
                        Line line = item1.Value.As<Line>();
                        Brush b = ToBrush(line.DrawPen.Color);
                        StrokeStyle s = ToStrockStyle(line.DrawPen);
                        renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                        b.Dispose();
                        s.Dispose();
                    }
                    else if (item1.Value.GetType() == typeof(Position2D))
                    {
                        Position2D pos = item1.Value.As<Position2D>();
                        DrawToken(pos);
                    }
                    else if (item1.Value.GetType() == typeof(Circle))
                    {
                        Circle circle = item1.Value.As<Circle>();
                        if (circle.IsFill)
                        {
                            Brush b = ToBrush(circle.DrawPen.Color);
                            b.Opacity = circle.Opacity;
                            renderTarget.FillEllipse(b, new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious });
                            b.Dispose();
                        }
                        else
                        {
                            Brush b = ToBrush(circle.DrawPen.Color);
                            StrokeStyle s = ToStrockStyle(circle.DrawPen);
                            renderTarget.DrawEllipse(b,
                                new Ellipse() { Center = circle.Center, RadiusX = (float)circle.Radious, RadiusY = (float)circle.Radious },
                                circle.DrawPen.Width, s);
                            b.Dispose();
                            s.Dispose();
                        }

                    }
                    else if (item1.Value.GetType() == typeof(SingleObjectState))
                    {

                        SingleObjectState sin = item1.Value.As<SingleObjectState>();
                        DrawRobot(sin, 0, .4f);

                    }
                    else if (item1.Value.GetType() == typeof(FlatRectangle))
                    {
                        FlatRectangle rect = item1.Value.As<FlatRectangle>();
                        if (rect.IsFill)
                        {
                            Brush b = ToBrush(rect.FillColor);
                            b.Opacity = rect.Opacity;
                            renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                            b.Dispose();
                        }
                        else
                        {
                            Brush b = ToBrush(rect.BorderColor);
                            renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                            b.Dispose();
                        }
                    }
                    else if (item1.Value.GetType() == typeof(DrawRegion))
                    {

                        DrawRegion text = item1.Value.As<DrawRegion>();
                        DrawPath(text);

                    }
                    else if (item1.Value.GetType() == typeof(StringDraw))
                    {

                        StringDraw text = item1.Value.As<StringDraw>();
                        DrawText(text.Content, ToBrush(text.TextColor), text.Posiotion);


                    }
                }
            }
            VisualizerObject.drawingObject.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawKickStatus()
        {

            if (_model == null || AnalizeMode) return;
            foreach (var item in _model.OurRobots)
            {
                Position2D center = item.Value.Location + Vector2D.FromAngleSize(Math.PI / -2, 0.05);
                Circle c = new Circle(center, 0.015, new System.Drawing.Pen(System.Drawing.Color.Black, 0.001f));
                Brush b1 = ToBrush(c.DrawPen.Color);
                renderTarget.DrawEllipse(b1, new Ellipse().GetEllipse(center, 0.015f, 0.015f), 0.005f);
                b1.Dispose();
                if (_currentWrapper == null) return;
                if (_currentWrapper.RobotCommnd.ContainsKey(item.Key))
                {
                    if (_currentWrapper.RobotCommnd[item.Key].KickPower > 0)
                    {
                        System.Drawing.Color cf = (_currentWrapper.RobotCommnd[item.Key].isChipKick) ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                        Brush b = ToBrush(cf);
                        renderTarget.FillEllipse(b, new Ellipse().GetEllipse(center, 0.015f, 0.015f));
                        b.Dispose();
                    }
                }
            }

        }

        public void Darwcollection(DrawCollection d)
        {
            if (d == null) return;
            foreach (var item1 in d.drawingObject.Keys.ToList())
            {
                if (d == null) return;
                DrawCollection cur = d;
                #region Lines
                if (cur.drawingObject[item1].GetType() == typeof(Line))
                {
                    Line line = cur.drawingObject[item1].As<Line>();
                    Brush b = ToBrush(line.DrawPen.Color);
                    StrokeStyle s = ToStrockStyle(line.DrawPen);
                    renderTarget.DrawLine(b, (line.Head), (line.Tail), line.DrawPen.Width, s);
                    b.Dispose();
                    s.Dispose();
                }
                #endregion

                #region circles
                else if (cur.drawingObject[item1].GetType() == typeof(Circle))
                {
                    Circle circle = cur.drawingObject[item1].As<Circle>();
                    Brush b = ToBrush(circle.DrawPen.Color);

                    if (circle.IsFill)
                    {

                        b.Opacity = circle.Opacity;
                        renderTarget.FillEllipse(b, new Ellipse().GetEllipse((circle.Center), (float)circle.Radious, (float)circle.Radious));
                    }
                    else
                    {
                        StrokeStyle s = ToStrockStyle(circle.DrawPen);
                        renderTarget.DrawEllipse(b, new Ellipse().GetEllipse((circle.Center), (float)circle.Radious, (float)circle.Radious), circle.DrawPen.Width, s);
                        s.Dispose();
                    }

                    b.Dispose();
                }
                #endregion

                #region SingleObjectStates
                else if (cur.drawingObject[item1].GetType() == typeof(SingleObjectState))
                {
                    SingleObjectState sin = cur.drawingObject[item1].As<SingleObjectState>();

                    DrawRobot(sin, 0, .4f);



                }
                #endregion

                #region FlatRectangle
                else if (cur.drawingObject[item1].GetType() == typeof(FlatRectangle))
                {
                    FlatRectangle rect = cur.drawingObject[item1].As<FlatRectangle>();

                    if (rect.IsFill)
                    {
                        Brush b = ToBrush(rect.FillColor);
                        b.Opacity = rect.Opacity;
                        renderTarget.FillRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y));
                        b.Dispose();
                    }
                    else
                    {
                        Brush b = ToBrush(rect.BorderColor);
                        renderTarget.DrawRectangle(b, new RectangleF((float)rect.Left, (float)rect.Top, (float)rect.BottomRight.X, (float)rect.BottomRight.Y), rect.BorderWidth);
                        b.Dispose();
                    }
                }


                #endregion

                #region StringDraw
                else if (cur.drawingObject[item1].GetType() == typeof(StringDraw))
                {
                    StringDraw text = cur.drawingObject[item1].As<StringDraw>();
                    Brush b = ToBrush(text.TextColor);

                    DrawText(text.Content, b, text.Posiotion);



                    b.Dispose();
                }
                #endregion

                #region DrawRegion
                else if (cur.drawingObject[item1].GetType() == typeof(DrawRegion))
                {

                    DrawRegion region = cur.drawingObject[item1].As<DrawRegion>();

                    DrawPath(region);


                }
                #endregion

                #region DrawPosition
                else if (cur.drawingObject[item1].GetType() == typeof(Position2D))
                {

                    Position2D region = cur.drawingObject[item1].As<Position2D>();

                    DrawToken(region);


                }
                #endregion
            }
        }


    }
}
