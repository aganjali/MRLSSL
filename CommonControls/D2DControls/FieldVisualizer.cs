//using System;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.GameDefinitions;
//using Microsoft.WindowsAPICodePack.DirectX;
//using Microsoft.WindowsAPICodePack.DirectX.Controls;
//using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
//using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
//using System.Collections.Generic;
//using MRL.SSL.CommonControls;
//using System.Windows.Forms;
//using MRL.SSL.CommonControls.Direct2D;

//namespace MRL.SSL.CommonControls.D2DControls
//{
//    public partial class FieldVisualizer : D2DControl
//    {
//        int _ballSign = 200;
//        private bool _showAngle = false;
//        /// <summary>
//        /// Show a long rectangle on robot head
//        /// </summary>
//        public bool ShowAngle
//        {
//            get { return _showAngle; }
//            set { _showAngle = value; }
//        }
//        FieldOrientation _mode;
//        /// <summary>
//        /// Vertical Or Horizental
//        /// </summary>
//        public FieldOrientation Mode
//        {
//            get { return _mode; }
//            set { _mode = value; }
//        }
//        bool _showUserObjects = true;
//        /// <summary>
//        /// show or hide objects that user want to draw
//        /// </summary>
//        public bool ShowUserObjects
//        {
//            get { return _showUserObjects; }
//            set { _showUserObjects = value; }
//        }
//        bool _showRectangle = true;
//        /// <summary>
//        /// show or hide rectangles that user want to draw
//        /// </summary>
//        public bool ShowRectangle
//        {
//            get { return _showRectangle; }
//            set { _showRectangle = value; }
//        }
//        bool _showEllipse = true;
//        /// <summary>
//        /// show or hide ellipse that user want to draw
//        /// </summary>
//        public bool ShowEllipse
//        {
//            get { return _showEllipse; }
//            set { _showEllipse = value; }
//        }
//        bool _showLines = true;
//        /// <summary>
//        /// show or hide lines that user want to draw
//        /// </summary>
//        public bool ShowLines
//        {
//            get { return _showLines; }
//            set { _showLines = value; }
//        }
//        bool _showPaths = true;
//        /// <summary>
//        /// show or hide paths that user want to draw
//        /// </summary>
//        public bool ShowPaths
//        {
//            get { return _showPaths; }
//            set { _showPaths = value; }
//        }
//        bool _showBattery = true;
//        /// <summary>
//        /// show or hide batterys percent that user want to draw
//        /// </summary>
//        public bool ShowBattery
//        {
//            get { return _showBattery; }
//            set { _showBattery = value; }
//        }
//        bool _showTexts = true;
//        /// <summary>
//        /// show or hide texts that user want to draw
//        /// </summary>
//        public bool ShowTexts
//        {
//            get { return _showTexts; }
//            set { _showTexts = value; }
//        }
//        bool _showOurRobots = true;
//        /// <summary>
//        /// show or hide our robots
//        /// </summary>
//        public bool ShowOurRobots
//        {
//            get { return _showOurRobots; }
//            set { _showOurRobots = value; }
//        }
//        bool _showOpponentRobots = true;
//        /// <summary>
//        /// show or hide opponnets
//        /// </summary>
//        public bool ShowOpponentRobots
//        {
//            get { return _showOpponentRobots; }
//            set { _showOpponentRobots = value; }
//        }
//        bool _showRobotTail;
//        /// <summary>
//        /// show tail of robot
//        /// </summary>
//        public bool ShowRobotTail
//        {
//            get { return _showRobotTail; }
//            set { _showRobotTail = value; }
//        }
//        bool _showBallTail;
//        /// <summary>
//        /// show tail of ball
//        /// </summary>
//        public bool ShowBallTail
//        {
//            get { return _showBallTail; }
//            set { _showBallTail = value; }
//        }
//        bool _showRegions = true;
//        /// <summary>
//        /// show or hide regions that user want to draw
//        /// </summary>
//        public bool ShowRegions
//        {
//            get { return _showRegions; }
//            set { _showRegions = value; }
//        }
//        List<string> _gropsStatus;
//        /// <summary>
//        /// show or hide groups that user want to draw
//        /// </summary>
//        public List<string> GropsStatus
//        {
//            get { return _gropsStatus; }
//            set { _gropsStatus = value; }
//        }
//        Dictionary<int, Position2D> _allBalls;
//        /// <summary>
//        /// balls that merger want to draw
//        /// </summary>
//        public Dictionary<int, Position2D> AllBalls
//        {
//            get { return _allBalls; }
//            set { _allBalls = value; }
//        }
//        private WorldModel _model;
//        /// <summary>
//        /// WorldModel of game
//        /// </summary>
//        public WorldModel Model
//        {
//            get { return _model; }
//            set
//            {
//                _model = value;
//                _frameCount++;
//                DataChanged();
//            }
//        }
//        private Dictionary<int, List<Position2D>> _pathsToDraw = new Dictionary<int, List<Position2D>>();
//        /// <summary>
//        /// paths that user want to draw
//        /// </summary>
//        public Dictionary<int, List<Position2D>> PathsToDraw
//        {
//            get { return _pathsToDraw; }
//        }
//        //----
//        System.Drawing.Bitmap bit;
//        System.Drawing.Bitmap orgBitmap;
//        //----
//        /// <summary>
//        /// a list of SingleObjectState for show all balls
//        /// </summary>
//        List<SingleObjectState> _balls;
//        /// <summary>
//        /// a history for show robot tail 
//        /// </summary>
//        Dictionary<int, List<Position2D>> _robotSign = new Dictionary<int, List<Position2D>>();
//        /// <summary>
//        /// temp for visual effects
//        /// </summary>
//        List<Circle> _hist2;
//        long _frameCount = 0;
//        DWriteFactory dwriteFactory;
//        TextFormat textFormat;
//        PathGeometry pg;
//        #region My Brushes
//        StrokeStyle HelperStrock;
//        Brush PathBrush, BorderBrush, BallBrush, RobotBrush, HelperBrush;
//        SolidColorBrush _fieldMarkingPen, _fieldGoalsPen, _ourGoalPaint, _marginPaint, _blackbrush, _textbrush, _fillRegionbrush;
//        #endregion
//        /// <summary>
//        /// custrucy filedVisualizer 
//        /// </summary>
//        public FieldVisualizer()
//            : base()
//        {
//            _balls = new List<SingleObjectState>(_ballSign);
//            //orgBitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"d:\0003.png");
//            this.BackColor = System.Drawing.Color.FromArgb(0, 145, 25);
//            this.MouseClick += new System.Windows.Forms.MouseEventHandler(FieldVisualizer_MouseClick);
//            dwriteFactory = DWriteFactory.CreateFactory();
//            _gropsStatus = new List<string>();
//            _hist2 = new List<Circle>();
//        }
//        /// <summary>
//        /// set show/hide parameters
//        /// </summary>
//        /// <param name="values">an array of boolean</param>
//        public void SetShowSetting(bool[] values)
//        {
//            _showRectangle = values[0];
//            _showEllipse = values[1];
//            _showLines = values[2];
//            _showTexts = values[3];
//            _showOurRobots = values[4];
//            _showOpponentRobots = values[5];
//            _showPaths = values[6];
//            _showBattery = values[7];
//            _showUserObjects = values[8];
//            _showAngle = values[9];
//            _showBallTail = values[10];
//            _showRobotTail = values[11];
//        }
//        /// <summary>
//        /// if Model will set field will invalidate
//        /// </summary>
//        private void VisDataChanged()
//        {
//            Invalidate();
//        }
//        /// <summary>
//        /// if Model will set field will invalidate
//        /// </summary>
//        public void DataChanged()
//        {
//            this.Invalidate();
//        }
//        /// <summary>
//        /// after each invalidate execute this function 
//        /// And draw all figur in filed
//        /// </summary>
//        /// <param name="RenderTarget">data will Render on this object</param>
//        protected override void OnPaintContent(HwndRenderTarget RenderTarget)
//        {
           
//            //#region Field Drawing
//            //_fieldMarkingPen = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.White));
//            //_fieldGoalsPen = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.White));
//            //_ourGoalPaint = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.White));
//            //_marginPaint = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//            ////Main Rectangle
//            //pg = d2dfactory.CreatePathGeometry();
//            //GeometrySink gs = pg.Open();
//            //gs.BeginFigure(P2DToP2F(GameParameters.OurLeftCorner), FigureBegin.Hollow);
//            //gs.AddLine(P2DToP2F(GameParameters.OurRightCorner));
//            //gs.AddLine(P2DToP2F(GameParameters.OppRightCorner));
//            //gs.AddLine(P2DToP2F(GameParameters.OppLeftCorner));
//            //gs.EndFigure(FigureEnd.Closed);
//            //gs.Close();
//            //RenderTarget.DrawGeometry(pg, _fieldMarkingPen, 0.01f);
//            //RenderTarget.DrawLine(P2DToP2F(new System.Drawing.PointF(0, (float)(GameParameters.OppLeftCorner.Y + GameParameters.OurLeftCorner.Y) / 2)), P2DToP2F(new System.Drawing.PointF(0, (float)(float)(GameParameters.OppRightCorner.Y + GameParameters.OurRightCorner.Y) / 2)), _fieldMarkingPen, 0.01f);
//            ////CenterCircle
//            //Ellipse center = new Ellipse();
//            //center.Point = new Point2F(0, 0);
//            //center.RadiusX = 0.5f;
//            //center.RadiusY = 0.5f;
//            //RenderTarget.DrawEllipse(center, _fieldMarkingPen, 0.01f);
//            //Ellipse CenterPoint = new Ellipse();
//            //CenterPoint.Point = new Point2F(0, 0);
//            //CenterPoint.RadiusX = .02f;
//            //CenterPoint.RadiusY = .02f;
//            //RenderTarget.FillEllipse(CenterPoint, _fieldMarkingPen);
//            ////Our Defence Area
//            //pg = d2dfactory.CreatePathGeometry();
//            //gs = pg.Open();
//            //gs.BeginFigure(new Point2F((float)GameParameters.OurGoalCenter.X, (float)(GameParameters.OurGoalCenter.Y + GameParameters.DefenceAreaFrontWidth / 2 + GameParameters.DefenceareaRadii)), FigureBegin.Hollow);
//            //ArcSegment arcseg1 = new ArcSegment();
//            //arcseg1.ArcSize = ArcSize.Small;
//            //arcseg1.Point = P2DToP2F(new System.Drawing.PointF((float)(GameParameters.OurGoalCenter.X - GameParameters.DefenceareaRadii), (float)(GameParameters.OurGoalCenter.Y + GameParameters.DefenceAreaFrontWidth / 2)));
//            //arcseg1.RotationAngle = (float)Math.PI / 2;
//            //arcseg1.Size = new SizeF((float)GameParameters.DefenceareaRadii, (float)GameParameters.DefenceareaRadii);
//            //arcseg1.SweepDirection = SweepDirection.Clockwise;
//            //gs.AddArc(arcseg1);
//            //gs.AddLine(P2DToP2F(new System.Drawing.PointF((float)(GameParameters.OurGoalCenter.X - GameParameters.DefenceareaRadii), (float)(GameParameters.OurGoalCenter.Y - GameParameters.DefenceAreaFrontWidth / 2))));
//            //arcseg1.Point = P2DToP2F(new System.Drawing.PointF((float)GameParameters.OurGoalCenter.X, (float)(GameParameters.OurGoalCenter.Y - GameParameters.DefenceAreaFrontWidth / 2 - GameParameters.DefenceareaRadii)));
//            //gs.AddArc(arcseg1);
//            //gs.EndFigure(FigureEnd.Open);
//            ////Opp Defence Area
//            //gs.BeginFigure(new Point2F((float)GameParameters.OppGoalCenter.X, (float)(GameParameters.OppGoalCenter.Y + GameParameters.DefenceAreaFrontWidth / 2 + GameParameters.DefenceareaRadii)), FigureBegin.Hollow);
//            //ArcSegment arcseg2 = new ArcSegment();
//            //arcseg2.ArcSize = ArcSize.Small;
//            //arcseg2.Point = new Point2F((float)(GameParameters.OppGoalCenter.X + GameParameters.DefenceareaRadii), (float)(GameParameters.OppGoalCenter.Y + GameParameters.DefenceAreaFrontWidth / 2));
//            //arcseg2.RotationAngle = (float)Math.PI / 2;
//            //arcseg2.Size = new SizeF((float)GameParameters.DefenceareaRadii, (float)GameParameters.DefenceareaRadii);
//            //arcseg2.SweepDirection = SweepDirection.CounterClockwise;
//            //gs.AddArc(arcseg2);
//            //gs.AddLine(new Point2F((float)(GameParameters.OppGoalCenter.X + GameParameters.DefenceareaRadii), (float)(GameParameters.OppGoalCenter.Y - GameParameters.DefenceAreaFrontWidth / 2)));
//            //arcseg2.Point = new Point2F((float)GameParameters.OppGoalCenter.X, (float)(GameParameters.OppGoalCenter.Y - GameParameters.DefenceAreaFrontWidth / 2 - GameParameters.DefenceareaRadii));
//            //gs.AddArc(arcseg2);
//            //gs.EndFigure(FigureEnd.Open);
//            //gs.Close();
//            //RenderTarget.DrawGeometry(pg, _fieldMarkingPen, 0.01f);
//            ////Our Goal
//            //pg = d2dfactory.CreatePathGeometry();
//            //gs = pg.Open();
//            //gs.BeginFigure(new Point2F((float)(GameParameters.OurGoalLeft.X), (float)GameParameters.OurGoalLeft.Y-0.01f), FigureBegin.Hollow);
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OurGoalLeft.X, GameParameters.OurGoalLeft.Y-0.01) + new Vector2D(GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OurGoalRight.X, GameParameters.OurGoalRight.Y - 0.01) + new Vector2D(GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OurGoalRight.X, GameParameters.OurGoalRight.Y - 0.01)));
//            //gs.EndFigure(FigureEnd.Open);
//            ////Opp Goal
//            //gs.BeginFigure(new Point2F((float)(GameParameters.OppGoalLeft.X), (float)GameParameters.OppGoalLeft.Y-0.01f), FigureBegin.Hollow);
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y-0.01) + new Vector2D(-GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y - 0.01) + new Vector2D(-GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y - 0.01)));
//            //gs.EndFigure(FigureEnd.Open);
//            //gs.Close();
//            //RenderTarget.DrawGeometry(pg, _fieldGoalsPen, 0.02f);
          
//            ////Penalty Points
//            //Ellipse PenaltyPoint = new Ellipse();
//            //PenaltyPoint.Point = new Point2F((float)GameParameters.OppGoalCenter.X + 0.4f, (float)GameParameters.OppGoalCenter.Y);
//            //PenaltyPoint.RadiusX = 0.005f;
//            //PenaltyPoint.RadiusY = 0.005f;
//            //RenderTarget.FillEllipse(PenaltyPoint, _fieldMarkingPen);
//            //PenaltyPoint.Point = new Point2F((float)GameParameters.OurGoalCenter.X - 0.4f, (float)GameParameters.OurGoalCenter.Y);
//            //RenderTarget.FillEllipse(PenaltyPoint, _fieldMarkingPen);
//            //#endregion

//            //#region Draw Objects
//            //if (_model != null)
//            //{
//            //    DrawAccordingToStatus(RenderTarget);
//            //    _balls.Add(_model.BallState);
//            //    if (_balls.Count > 239)
//            //        _balls.RemoveAt(0);
//            //    if (_model.BallState != null)
//            //    {
//            //        if (_showBallTail)
//            //            for (int i = 0; i < _balls.Count; i++)
//            //                DrawObject(_balls[i], 0, RenderTarget, (i * 1f / 239f));
//            //        DrawObject(Model.BallState, 0, RenderTarget);
//            //    }

//            //    if (_model.OurRobots != null && _showOurRobots)
//            //        foreach (int key in Model.OurRobots.Keys)
//            //        {
//            //            if (_showRobotTail)
//            //            {
                            
//            //                if (!_robotSign.ContainsKey(key))
//            //                    _robotSign.Add(key, new List<Position2D>());

//            //                _robotSign[key].Add(Model.OurRobots[key].Location);
//            //                if (_robotSign[key].Count > 400)
//            //                    _robotSign[key].RemoveAt(0);
//            //                DrawPath(_robotSign[key], RenderTarget);
//            //            }
//            //            else
//            //                _robotSign.Clear();

//            //            DrawObject(Model.OurRobots[key], key, RenderTarget);
                       
//            //        }
//            //    if (_model.Opponents != null && _showOpponentRobots)
//            //        foreach (int key in Model.Opponents.Keys)
//            //            DrawObject(Model.Opponents[key], key, RenderTarget);
//            //    DrawAllBalls(_allBalls, RenderTarget);
//            //    DrawAccordingToStatus(RenderTarget);
//            //}
            
//            //#endregion

//            //#region goals depth
//            ////Our Goal Fill
//            //pg = d2dfactory.CreatePathGeometry();
//            //gs = pg.Open();
//            //gs.BeginFigure(new Point2F((float)(GameParameters.OurGoalLeft.X), (float)GameParameters.OurGoalLeft.Y - 0.01f), FigureBegin.Filled);
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OurGoalLeft.X, GameParameters.OurGoalLeft.Y - 0.01) + new Vector2D(GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OurGoalRight.X, GameParameters.OurGoalRight.Y - 0.01) + new Vector2D(GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OurGoalRight.X, GameParameters.OurGoalRight.Y - 0.01)));
//            //gs.EndFigure(FigureEnd.Closed);
//            //gs.Close();
//            //if (Model != null)
//            //    _marginPaint = RenderTarget.CreateSolidColorBrush((true ^ Model.OurMarkerISYellow) ? new ColorF(65f / 255f, 126f / 255f, 255 / 255f, 1f) : new ColorF(255f / 255f, 243f / 255f, 62f / 255f, 1f));
//            //_marginPaint.Opacity = 0.4f;
//            //RenderTarget.FillGeometry(pg, _marginPaint);

//            ////Opp Goal Fill
//            //pg = d2dfactory.CreatePathGeometry();
//            //gs = pg.Open();
//            //gs.BeginFigure(new Point2F((float)(GameParameters.OppGoalLeft.X), (float)GameParameters.OppGoalLeft.Y - 0.01f), FigureBegin.Filled);
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y - 0.01) + new Vector2D(-GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y - 0.01) + new Vector2D(-GameParameters.GoalDepth, 0)));
//            //gs.AddLine(P2DToP2F(new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y - 0.01)));
//            //gs.EndFigure(FigureEnd.Closed);

//            //gs.Close();
//            //if (Model != null)
//            //    _marginPaint = RenderTarget.CreateSolidColorBrush((false ^ Model.OurMarkerISYellow) ? new ColorF(65f / 255f, 126f / 255f, 255 / 255f, 1f) : new ColorF(255f / 255f, 243f / 255f, 62f / 255f, 1f));
//            //_marginPaint.Opacity = 0.4f;
//            //RenderTarget.FillGeometry(pg, _marginPaint);
//            //#endregion

//            //#region Draw Visualizer Data
//            //HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//            //Ellipse DrawCircle = new Ellipse();
//            //System.Drawing.RectangleF rec = new System.Drawing.RectangleF();
            
//            //#region Draw Region
//            //if (VisualizerData.RegionToDraw != null&&_showRegions)
//            //{
//            //    _marginPaint.Opacity = 0.4f;
//            //    for (int i = 0; i < VisualizerData.RegionToDraw.Count; i++)
//            //    {
//            //        rec = new System.Drawing.RectangleF((VisualizerData.RegionToDraw[i].X / 100f) - 3.5f, (VisualizerData.RegionToDraw[i].Y / 100f) - 2.5f, VisualizerData.RegionPixelWidth * .01f, VisualizerData.RegionPixelWidth * .01f);
//            //        RenderTarget.FillRectangle(new RectF(rec.Left, rec.Top, rec.Right, rec.Bottom), _marginPaint);
//            //    }
//            //}
//            //if (VisualizerData.RegionToDraw != null)
//            //{
//            //    //System.Drawing.Bitmap orgBitmap = SparseMarixToBitmap(VisualizerData.RegionToDraw);
//            //    //orgBitmap.Save(@"d:\r.bmp");
//            //    //System.Drawing.Imaging.BitmapData bitmapData = orgBitmap.LockBits(new System.Drawing.Rectangle(0, 0, orgBitmap.Width, orgBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
//            //    ////SlimDX??DataStrem??????*/
//            //    //SlimDX.DataStream stream = new SlimDX.DataStream(bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, true, false);
//            //    ////DataStream??Direct2D??????????
//            //    //PixelFormat format = new PixelFormat(SlimDX.DXGI.Format.B8G8R8A8_UNorm, SlimDX.Direct2D.AlphaMode.Premultiplied);
//            //    //BitmapProperties bitmapProperties = new SlimDX.Direct2D.BitmapProperties();
//            //    //bitmapProperties.HorizontalDpi = bitmapProperties.VerticalDpi = 96;
//            //    //bitmapProperties.PixelFormat = format;
//            //    //Bitmap bitmap = new Bitmap(RenderTarget, new System.Drawing.Size(orgBitmap.Width, orgBitmap.Height), stream, bitmapData.Stride, bitmapProperties);
//            //    //orgBitmap.UnlockBits(bitmapData);
//            //    //RenderTarget.DrawBitmap(bitmap, new System.Drawing.RectangleF(-3.5f, -2.5f, 7f, 5f), 1f);
//            //}
//            //#endregion

//            //#region Draw Circles
//            //if (_showEllipse)
//            //{
//            //    if (VisualizerData.PermanentCirclesToDraw != null && _showEllipse)
//            //    {
//            //        foreach (string key in VisualizerData.PermanentCirclesToDraw.Keys)
//            //        {
//            //            System.Drawing.Pen p = VisualizerData.PermanentCirclesToDraw[key].DrawPen;

//            //            HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //            HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //            DrawCircle.Point = P2DToP2F(VisualizerData.PermanentCirclesToDraw[key].Center);
//            //            DrawCircle.RadiusX = (float)VisualizerData.PermanentCirclesToDraw[key].Radious;
//            //            DrawCircle.RadiusY = (float)VisualizerData.PermanentCirclesToDraw[key].Radious;
//            //            RenderTarget.DrawEllipse(DrawCircle, HelperBrush, p.Width, HelperStrock);
//            //        }
//            //    }
//            //    if (VisualizerData.MomentlyCirclesToDraw != null && _showEllipse)
//            //    {
//            //        foreach (string key in VisualizerData.MomentlyCirclesToDraw.Keys)
//            //        {
//            //            System.Drawing.Pen p = VisualizerData.MomentlyCirclesToDraw[key].DrawPen;
//            //            HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //            HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //            DrawCircle.Point = P2DToP2F(VisualizerData.MomentlyCirclesToDraw[key].Center);
//            //            DrawCircle.RadiusX = (float)VisualizerData.MomentlyCirclesToDraw[key].Radious;
//            //            DrawCircle.RadiusY = (float)VisualizerData.MomentlyCirclesToDraw[key].Radious;
//            //            RenderTarget.DrawEllipse(DrawCircle, HelperBrush, p.Width, HelperStrock);
//            //        }
//            //    }
//            //}
//            //#endregion

//            //#region Draw Rectangle
//            //if (_showRectangle)
//            //{
//            //    if (VisualizerData.PermanentRectangleToDraw != null)
//            //    {
//            //        foreach (string key in VisualizerData.PermanentRectangleToDraw.Keys)
//            //        {
//            //            string[] result = key.Split(new char[] { '@' });
//            //            float r = int.Parse(result[1]) / 255f;
//            //            float g = int.Parse(result[2]) / 255f;
//            //            float b = int.Parse(result[3]) / 255f;
//            //            float o = (float)double.Parse(result[4]);
//            //            rec = VisualizerData.PermanentRectangleToDraw[key];
//            //            _marginPaint = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, o));
//            //            RenderTarget.FillRectangle(new RectF(rec.Left, rec.Top, rec.Right, rec.Bottom), _marginPaint);
//            //        }
//            //    }
//            //    if (VisualizerData.MomentRectangleToDraw != null)
//            //    {
//            //        foreach (string key in VisualizerData.MomentRectangleToDraw.Keys)
//            //        {
//            //            string[] result = key.Split(new char[] { '@' });
//            //            float r = int.Parse(result[1]) / 255f;
//            //            float g = int.Parse(result[2]) / 255f;
//            //            float b = int.Parse(result[3]) / 255f;
//            //            float o = (float)double.Parse(result[4]);
//            //            rec = VisualizerData.MomentRectangleToDraw[key];
//            //            _marginPaint = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, o));
//            //            rec = VisualizerData.MomentRectangleToDraw[key];
//            //            RenderTarget.FillRectangle(new RectF(rec.Left, rec.Top, rec.Right, rec.Bottom), _marginPaint);
//            //        }
//            //    }
//            //}
//            //#endregion

//            //#region Draw Line
//            //if (_showLines)
//            //{
//            //    if (VisualizerData.PermanentlyLineToDraw != null && _showLines)
//            //    {
//            //        foreach (string key in VisualizerData.PermanentlyLineToDraw.Keys)
//            //        {
//            //            System.Drawing.Pen p = VisualizerData.PermanentlyLineToDraw[key].DrawPen;
//            //            HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //            HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //            RenderTarget.DrawLine(P2DToP2F(VisualizerData.PermanentlyLineToDraw[key].Head), P2DToP2F(VisualizerData.PermanentlyLineToDraw[key].Tail), HelperBrush, p.Width, HelperStrock);
//            //        }
//            //    }
//            //    if (VisualizerData.MomentlyLineToDraw != null && _showLines)
//            //    {
//            //        foreach (string key in VisualizerData.MomentlyLineToDraw.Keys)
//            //        {
//            //            System.Drawing.Pen p = VisualizerData.MomentlyLineToDraw[key].DrawPen;
//            //            HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //            HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //            RenderTarget.DrawLine(P2DToP2F(VisualizerData.MomentlyLineToDraw[key].Head), P2DToP2F(VisualizerData.MomentlyLineToDraw[key].Tail), HelperBrush, p.Width, HelperStrock);
//            //        }
//            //    }
//            //}
//            //#endregion

//            //#region Draw UserObject
//            //if (VisualizerData.ObjectToDraw != null && _showUserObjects)
//            //{
//            //    foreach (string key in VisualizerData.ObjectToDraw.Keys)
//            //    {
//            //        DrawObject(VisualizerData.ObjectToDraw[key], 0, RenderTarget);
//            //    }
//            //}
//            //#endregion

//            //#region Draw Text
//            //if (_showTexts)
//            //{
//            //    if (VisualizerData.PermanentTextToDraw != null)
//            //    {
//            //        foreach (string key in VisualizerData.PermanentTextToDraw.Keys)
//            //        {
//            //            string[] s = key.Split(new char[] { '@', '@', '@', '@' });
//            //            float r = float.Parse(s[1]) / 255f;
//            //            float g = float.Parse(s[2]) / 255f;
//            //            float b = float.Parse(s[3]) / 255f;
//            //            float a = float.Parse(s[4]);
//            //            _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, a));
//            //            DrawText(s[0], _textbrush, RenderTarget, new System.Drawing.RectangleF(VisualizerData.PermanentTextToDraw[key], new System.Drawing.SizeF((float)(key.Length * 0.05), 0.1f)));
//            //        }
//            //    }
//            //    if (VisualizerData.MomentlyTextToDraw != null)
//            //    {
//            //        foreach (string key in VisualizerData.MomentlyTextToDraw.Keys)
//            //        {
//            //            string[] s = key.Split(new char[] { '@', '@', '@', '@' });
//            //            float r = float.Parse(s[1]) / 255f;
//            //            float g = float.Parse(s[2]) / 255f;
//            //            float b = float.Parse(s[3]) / 255f;
//            //            float a = float.Parse(s[4]);
//            //            _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, a));
//            //            DrawText(s[0], _textbrush, RenderTarget, new System.Drawing.RectangleF(VisualizerData.MomentlyTextToDraw[key], new System.Drawing.SizeF((float)(key.Length * 0.05), 0.1f)));
//            //        }
//            //    }
//            //}
//            //#endregion

//            //#region Draw Paths
//            //if (_showPaths)
//            //{
//            //    if (VisualizerData.PathToDraw != null)
//            //    {
//            //        foreach (int key in VisualizerData.PathToDraw.Keys)
//            //        {
//            //            DrawPath(VisualizerData.PathToDraw[key], RenderTarget);
//            //        }
//            //    }
//            //}
//            //#endregion

//            //#region Draw FillRegion
//            //if (VisualizerData.FillRegions != null)
//            //{
//            //    foreach (string key in VisualizerData.FillRegions.Keys)
//            //    {
//            //        string[] s = key.Split(new char[] { ',', ',', ',', ',' });
//            //        float r = float.Parse(s[1]) / 255f;
//            //        float g = float.Parse(s[2]) / 255f;
//            //        float b = float.Parse(s[3]) / 255f;
//            //        float a = float.Parse(s[4]) ;
//            //        DrawFillRegion(VisualizerData.FillRegions[key], new ColorF(r, g, b, a), RenderTarget);
//            //    }
//            //}
//            //#endregion

//            //#region Draw Token
//            //if (VisualizerData.MomentToken != null)
//            //{
//            //    foreach (string item in VisualizerData.MomentToken.Keys)
//            //        DrawToken(RenderTarget, VisualizerData.MomentToken[item]);
//            //}
//            //if (VisualizerData.PermanentToken != null)
//            //{
//            //    foreach (string item in VisualizerData.PermanentToken.Keys)
//            //        DrawToken(RenderTarget, VisualizerData.PermanentToken[item]);
//            //}
//            //#endregion

//            //#region Draw Groups
//            ////*******************************************************************//
//            ////             DRAWING GROUPS(a Little VisualizerData!!!)           //
//            ////*******************************************************************//                  
//            //if (VisualizerData.Groups != null && _gropsStatus!=null)
//            //{
//            //    for (int i = 0; i < _gropsStatus.Count; i++)
//            //    {
//            //        if (VisualizerData.Groups.ContainsKey(_gropsStatus[i]))
//            //        {
//            //            DrawCollection GroupToDraw = VisualizerData.Groups[_gropsStatus[i]];
//            //            _marginPaint.Opacity = 0.4f;

//            //            #region Draw Regions (in Group)
//            //            if (_showRegions)
//            //            {
//            //                if (GroupToDraw.RegionToDraw != null)
//            //                {
//            //                    for (int j = 0; j < GroupToDraw.RegionToDraw.Count; j++)
//            //                    {
//            //                        rec = new System.Drawing.RectangleF((GroupToDraw.RegionToDraw[j].X / 100f) - 3.5f, (GroupToDraw.RegionToDraw[j].Y / 100f) - 2.5f, GroupToDraw.RegionPixelWidth * .01f, GroupToDraw.RegionPixelWidth * .01f);
//            //                        RenderTarget.FillRectangle(new RectF(rec.Left, rec.Top, rec.Right, rec.Bottom), _marginPaint);
//            //                    }
//            //                }

//            //            }
//            //            #endregion

//            //            #region Draw Circles (in Group)
//            //            if (_showEllipse)
//            //            {
//            //                if (GroupToDraw.PermanentCirclesToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.PermanentCirclesToDraw.Keys)
//            //                    {
//            //                        System.Drawing.Pen p = GroupToDraw.PermanentCirclesToDraw[key].DrawPen;
//            //                        HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //                        HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //                        DrawCircle.Point = P2DToP2F(GroupToDraw.PermanentCirclesToDraw[key].Center);
//            //                        DrawCircle.RadiusX = (float)GroupToDraw.PermanentCirclesToDraw[key].Radious;
//            //                        DrawCircle.RadiusY = (float)GroupToDraw.PermanentCirclesToDraw[key].Radious;
//            //                        RenderTarget.DrawEllipse(DrawCircle, HelperBrush, p.Width, HelperStrock);
//            //                    }
//            //                }
//            //                if (GroupToDraw.MomentlyCirclesToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.MomentlyCirclesToDraw.Keys)
//            //                    {
//            //                        System.Drawing.Pen p = GroupToDraw.MomentlyCirclesToDraw[key].DrawPen;
//            //                        HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //                        HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //                        //HelperBrush.Opacity = 0.f;
//            //                        DrawCircle.Point = P2DToP2F(GroupToDraw.MomentlyCirclesToDraw[key].Center);
//            //                        DrawCircle.RadiusX = (float)GroupToDraw.MomentlyCirclesToDraw[key].Radious;
//            //                        DrawCircle.RadiusY = (float)GroupToDraw.MomentlyCirclesToDraw[key].Radious;
//            //                        RenderTarget.DrawEllipse(DrawCircle, HelperBrush, p.Width, HelperStrock);
//            //                    }
//            //                }
//            //            }
//            //            #endregion

//            //            #region Draw Rectangle (in Group)
//            //            if (_showRectangle)
//            //            {
//            //                if (GroupToDraw.PermanentRectangleToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.PermanentRectangleToDraw.Keys)
//            //                    {
//            //                        string[] result = key.Split(new char[] { '@' });
//            //                        float r = int.Parse(result[1]) / 255f;
//            //                        float g = int.Parse(result[2]) / 255f;
//            //                        float b = int.Parse(result[3]) / 255f;
//            //                        float o = (float)double.Parse(result[4]);
//            //                        rec = GroupToDraw.PermanentRectangleToDraw[key];
//            //                        _marginPaint = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, o));
//            //                        rec = GroupToDraw.PermanentRectangleToDraw[key];
//            //                        RenderTarget.FillRectangle(new RectF(rec.Left, rec.Top, rec.Right, rec.Bottom), _marginPaint);
//            //                    }
//            //                }
//            //                if (GroupToDraw.MomentRectangleToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.MomentRectangleToDraw.Keys)
//            //                    {
//            //                        string[] result = key.Split(new char[] { '@' });
//            //                        float r = int.Parse(result[1]) / 255f;
//            //                        float g = int.Parse(result[2]) / 255f;
//            //                        float b = int.Parse(result[3]) / 255f;
//            //                        float o = (float)double.Parse(result[4]);
//            //                        rec = GroupToDraw.MomentRectangleToDraw[key];
//            //                        _marginPaint = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, o));
//            //                        rec = GroupToDraw.MomentRectangleToDraw[key];
//            //                        RenderTarget.FillRectangle(new RectF(rec.Left, rec.Top, rec.Right, rec.Bottom), _marginPaint);
//            //                    }
//            //                }
//            //            }
//            //            #endregion

//            //            #region Draw Lines (in Group)
//            //            if (_showLines)
//            //            {
//            //                if (GroupToDraw.PermanentlyLineToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.PermanentlyLineToDraw.Keys)
//            //                    {
//            //                        System.Drawing.Pen p = GroupToDraw.PermanentlyLineToDraw[key].DrawPen;
//            //                        HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //                        HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //                        RenderTarget.DrawLine(P2DToP2F(GroupToDraw.PermanentlyLineToDraw[key].Head), P2DToP2F(GroupToDraw.PermanentlyLineToDraw[key].Tail), HelperBrush, p.Width, HelperStrock);
//            //                    }
//            //                }
//            //                if (GroupToDraw.MomentlyLineToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.MomentlyLineToDraw.Keys)
//            //                    {
//            //                        System.Drawing.Pen p = GroupToDraw.MomentlyLineToDraw[key].DrawPen;
//            //                        HelperStrock = d2dfactory.CreateStrokeStyle(new StrokeStyleProperties() { StartCap = (CapStyle)(int)p.StartCap, EndCap = (CapStyle)(int)p.EndCap });
//            //                        HelperBrush = RenderTarget.CreateSolidColorBrush(new ColorF((float)p.Color.R / 255f, (float)p.Color.G / 255f, (float)p.Color.B / 255f, 1f));
//            //                        RenderTarget.DrawLine(P2DToP2F(GroupToDraw.MomentlyLineToDraw[key].Head), P2DToP2F(GroupToDraw.MomentlyLineToDraw[key].Tail), HelperBrush, p.Width, HelperStrock);
//            //                    }
//            //                }
//            //            }
//            //            #endregion

//            //            #region Draw UserObjects (in Group)
//            //            if (_showUserObjects)
//            //            {
//            //                if (GroupToDraw.ObjectToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.ObjectToDraw.Keys)
//            //                    {
//            //                        DrawObject(GroupToDraw.ObjectToDraw[key], 1000, RenderTarget, (float)GroupToDraw.ObjectToDraw[key].Opacity);
//            //                    }
//            //                }
//            //            }
//            //            #endregion

//            //            #region Draw Text (in Group)
//            //            if (_showTexts)
//            //            {
//            //                if (GroupToDraw.PermanentTextToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.PermanentTextToDraw.Keys)
//            //                    {
//            //                        string[] s = key.Split(new char[] { '@', '@', '@', '@' });
//            //                        float r = float.Parse(s[1]) / 255f;
//            //                        float g = float.Parse(s[2]) / 255f;
//            //                        float b = float.Parse(s[3]) / 255f;
//            //                        float a = float.Parse(s[4]);
//            //                        _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, a));
//            //                        DrawText(s[0], _textbrush, RenderTarget, new System.Drawing.RectangleF(GroupToDraw.PermanentTextToDraw[key], new System.Drawing.SizeF((float)(key.Length * 0.05), 0.1f)));
//            //                    }
//            //                }
//            //                if (GroupToDraw.MomentlyTextToDraw != null)
//            //                {
//            //                    foreach (string key in GroupToDraw.MomentlyTextToDraw.Keys)
//            //                    {
//            //                        string[] s = key.Split(new char[] { '@', '@', '@', '@' });
//            //                        float r = float.Parse(s[1]) / 255f;
//            //                        float g = float.Parse(s[2]) / 255f;
//            //                        float b = float.Parse(s[3]) / 255f;
//            //                        float a = float.Parse(s[4]);
//            //                        _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(r, g, b, a));
//            //                        DrawText(s[0], _textbrush, RenderTarget, new System.Drawing.RectangleF(GroupToDraw.MomentlyTextToDraw[key], new System.Drawing.SizeF((float)(key.Length * 0.05), 0.1f)));
//            //                    }
//            //                }
//            //            }
//            //            #endregion

//            //            #region Draw Path (in Group)
//            //            if (GroupToDraw.PathToDraw != null)
//            //            {
//            //                foreach (int key in GroupToDraw.PathToDraw.Keys)
//            //                {
//            //                    DrawPath(GroupToDraw.PathToDraw[key], RenderTarget);
//            //                }
//            //            }
//            //            #endregion

//            //            #region Draw FillRegions (in Group)
//            //            if (GroupToDraw.FillRegions != null)
//            //            {
//            //                foreach (string key in GroupToDraw.FillRegions.Keys)
//            //                {
//            //                    string[] s = key.Split(new char[] { ',', ',', ',', ',' });
//            //                    float r = float.Parse(s[1])/255f;
//            //                    float g = float.Parse(s[2])/255f;
//            //                    float b = float.Parse(s[3])/255f;
//            //                    float a = float.Parse(s[4]);
//            //                    if (_fillRegionbrush != null)
//            //                        _fillRegionbrush.Dispose();
//            //                    DrawFillRegion(GroupToDraw.FillRegions[key], new ColorF(r, g, b, a), RenderTarget);
//            //                }
//            //            }
//            //            #endregion

//            //            #region Draw Token (in Group)
//            //            if (GroupToDraw.MomentToken != null)
//            //            {
//            //                foreach (string item in GroupToDraw.MomentToken.Keys)
//            //                    DrawToken(RenderTarget, GroupToDraw.MomentToken[item]);
//            //            }
//            //            if (GroupToDraw.PermanentToken != null)
//            //            {
//            //                foreach (string item in GroupToDraw.PermanentToken.Keys)
//            //                    DrawToken(RenderTarget, GroupToDraw.PermanentToken[item]);
//            //            }
//            //            #endregion
//            //        }
//            //    }
//            //    _gropsStatus.Clear();
//            //}
//            //#endregion

//            //#endregion


//        }
//        /// <summary>
//        /// draw all balls that will come from merger
//        /// </summary>
//        /// <param name="balls">a list of balls</param>
//        /// <param name="RenderTarget">data will Render on this object</param>
//        private void DrawAllBalls(Dictionary<int, Position2D> balls ,RenderTarget RenderTarget)
//        {
//            _blackbrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//            if (balls != null)
//            {
//                foreach (int key in balls.Keys)
//                {

//                    _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//                    RenderTarget.DrawEllipse(new Ellipse(P2DToP2F(balls[key]), (float)(GameParameters.BallDiameter / 2f + .01f), (float)(GameParameters.BallDiameter / 2f + .01f)), _blackbrush, 0.01f);
//                    if ((Model.BallState.Location - balls[key]).Size > 0.009)
//                    {
//                        if (Mode == FieldOrientation.Verticaly)
//                            DrawText("Ball : " + key, _textbrush, RenderTarget, new System.Drawing.RectangleF(new Position2D(balls[key].X - 0.09, balls[key].Y), new System.Drawing.SizeF((float)((key.ToString().Length + 7) * 0.05), 0.1f)));
//                        else
//                            DrawText("Ball : " + key, _textbrush, RenderTarget, new System.Drawing.RectangleF(new Position2D(balls[key].X, balls[key].Y - 0.09), new System.Drawing.SizeF((float)((key.ToString().Length + 7) * 0.05), 0.1f)));
//                    }
//                    else
//                    {
//                        if (Mode == FieldOrientation.Verticaly)
//                            DrawText("Ball : " + key, _textbrush, RenderTarget, new System.Drawing.RectangleF(new Position2D(balls[key].X - 0.18, balls[key].Y), new System.Drawing.SizeF((float)((key.ToString().Length + 7) * 0.05), 0.1f)));
//                        else
//                            DrawText("Ball : " + key, _textbrush, RenderTarget, new System.Drawing.RectangleF(new Position2D(balls[key].X, balls[key].Y - 0.18), new System.Drawing.SizeF((float)((key.ToString().Length + 7) * 0.05), 0.1f)));
//                    }
//                }
//            }
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        int? Selected = null;
//        /// <summary>
//        /// draw paths of robots
//        /// </summary>
//        /// <param name="PathData"></param>
//        /// <param name="RenderTarget"></param>
//        private void DrawPath(List<Position2D> PathData, RenderTarget RenderTarget)
//        {
//            if (PathBrush == null)
//                PathBrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Yellow));
//            for (int i = 1; i < PathData.Count; i++)
//                RenderTarget.DrawLine(P2DToP2F(PathData[i - 1]), P2DToP2F(PathData[i]), PathBrush, 0.01f);
//        }
//        /// <summary>
//        /// darwin objects(robots , balls)
//        /// </summary>
//        /// <param name="state"></param>
//        /// <param name="RobotID"></param>
//        /// <param name="RenderTarget"></param>
//        private void DrawObject(SingleObjectState state, int RobotID, RenderTarget RenderTarget)
//        {
//            if (state.Type == ObjectType.Ball)
//            {
//                BallBrush = RenderTarget.CreateSolidColorBrush(new ColorF(1f, 129f/255f, 0f, 1f));
//                Ellipse ellipse = new Ellipse();
//                ellipse.Point = P2DToP2F(state.Location);
//                ellipse.RadiusY = ellipse.RadiusX = (float)GameParameters.BallDiameter / 2;
//                RenderTarget.FillEllipse(ellipse, BallBrush);
//                BallBrush = RenderTarget.CreateSolidColorBrush(new ColorF(209f/255f, 89f/255f, 0f, 1f));
//                RenderTarget.DrawEllipse(ellipse, BallBrush, 0.006f);
//            }
//            else
//            {
//                RobotBrush = RenderTarget.CreateSolidColorBrush((state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new ColorF(65f / 255f, 126f / 255f, 255 / 255f, 1f) : new ColorF(255f / 255f, 243f / 255f, 62f / 255f, 1f));
//                BorderBrush = RenderTarget.CreateSolidColorBrush((state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new ColorF(18f / 255f, 59f / 255f, 160f / 255f, 1f) : new ColorF(204f / 255f, 157f / 255f, 0f / 255f, 1f));
//                if (state.Angle.HasValue)
//                {
//                    Position2D p1 = state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2);
//                    Position2D p2 = state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2);
//                    pg = d2dfactory.CreatePathGeometry();
//                    GeometrySink gs = pg.Open();
//                    if (_showAngle)
//                    {
//                        gs.BeginFigure(P2DToP2F(p2), FigureBegin.Filled);
//                        gs.AddLine(P2DToP2F(p2 + Vector2D.FromAngleSize(state.Angle.Value * Math.PI / 180.0, 5)));
//                        gs.AddLine(P2DToP2F(p1 + Vector2D.FromAngleSize(state.Angle.Value * Math.PI / 180.0, 5)));
//                        gs.AddLine(P2DToP2F(p1));
//                        gs.EndFigure(FigureEnd.Closed);
//                        gs.Close();
//                        BorderBrush.Opacity = .3f;
//                        RenderTarget.FillGeometry(pg, BorderBrush);
//                        BorderBrush.Opacity = 1f;
//                    }
//                    pg = d2dfactory.CreatePathGeometry();
//                    gs = pg.Open();
//                    gs.BeginFigure(P2DToP2F(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2)), FigureBegin.Filled);
//                    ArcSegment arcseg3 = new ArcSegment();
//                    arcseg3.ArcSize = ArcSize.Large;
//                    arcseg3.Point = P2DToP2F(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2));
//                    arcseg3.RotationAngle = (float)Math.PI / 2;
//                    arcseg3.Size = new SizeF((float)RobotParameters.OurRobotParams.Diameter / 2, (float)RobotParameters.OurRobotParams.Diameter / 2);
//                    arcseg3.SweepDirection = SweepDirection.Clockwise;
//                    gs.AddArc(arcseg3);
//                    gs.AddLine(P2DToP2F(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2)));
//                    gs.EndFigure(FigureEnd.Closed);
//                    gs.Close();
//                    RenderTarget.FillGeometry(pg, RobotBrush);
//                    RenderTarget.DrawGeometry(pg, BorderBrush, 0.01f);
//                }
//                else
//                {
//                    Ellipse ellipse = new Ellipse();
//                    ellipse.Point = P2DToP2F(state.Location);
//                    ellipse.RadiusY = ellipse.RadiusX = (state.Type == ObjectType.OurRobot) ? (float)RobotParameters.OurRobotParams.Diameter / 2 : (float)RobotParameters.OpponentParams.Diameter / 2;
//                    RenderTarget.FillEllipse(ellipse, RobotBrush);
//                    RenderTarget.DrawEllipse(ellipse, BorderBrush, 0.01f);
//                }
//                _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//                if (RobotID != 1000)
//                    if (_showTexts)
//                        DrawText(RobotID.ToString(), _textbrush, RenderTarget, new System.Drawing.RectangleF(state.Location, new System.Drawing.SizeF((float)(RobotID.ToString().Length * 0.05), 0.1f)));
//            }
//        }
//        /// <summary>
//        /// darwin objects(robots , balls)
//        /// </summary>
//        /// <param name="state"></param>
//        /// <param name="RobotID"></param>
//        /// <param name="RenderTarget"></param>
//        /// <param name="opacity"></param>
//        private void DrawObject(SingleObjectState state, int RobotID, RenderTarget RenderTarget, float opacity)
//        {
//            if (state.Type == ObjectType.Ball)
//            {
//                BallBrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Orange));
//                Ellipse ellipse = new Ellipse();
//                ellipse.Point = P2DToP2F(state.Location);
//                ellipse.RadiusY = ellipse.RadiusX = (float)GameParameters.BallDiameter / 2;
//                BallBrush.Opacity = opacity;
//                RenderTarget.FillEllipse(ellipse, BallBrush);
//            }
//            else
//            {
//                RobotBrush = RenderTarget.CreateSolidColorBrush((state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new ColorF(65f / 255f, 126f / 255f, 255 / 255f, 1f) : new ColorF(255f / 255f, 243f / 255f, 62f / 255f, 1f));
//                BorderBrush = RenderTarget.CreateSolidColorBrush((state.Type == ObjectType.OurRobot ^ Model.OurMarkerISYellow) ? new ColorF(18f / 255f, 59f / 255f, 160f / 255f, 1f) : new ColorF(204f / 255f, 157f / 255f, 0f / 255f, 1f));

//                RobotBrush.Opacity = opacity;
//                BorderBrush.Opacity = opacity;
//                if (state.Angle.HasValue)
//                {
                    
//                    pg = d2dfactory.CreatePathGeometry();
//                    GeometrySink gs = pg.Open();
//                    gs.BeginFigure(P2DToP2F(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) - 15, RobotParameters.OurRobotParams.Diameter / 2)), FigureBegin.Filled);
//                    ArcSegment arcseg3 = new ArcSegment();
//                    arcseg3.ArcSize = ArcSize.Large;
//                    arcseg3.Point = P2DToP2F(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2));
//                    arcseg3.RotationAngle = (float)Math.PI / 2;
//                    arcseg3.Size = new SizeF((float)RobotParameters.OurRobotParams.Diameter / 2, (float)RobotParameters.OurRobotParams.Diameter / 2);
//                    arcseg3.SweepDirection = SweepDirection.Clockwise;
//                    gs.AddArc(arcseg3);
//                    gs.AddLine(P2DToP2F(state.Location - Vector2D.FromAngleSize((state.Angle.Value * Math.PI / 180) + 15, RobotParameters.OurRobotParams.Diameter / 2)));
//                    gs.EndFigure(FigureEnd.Closed);
//                    gs.Close();
//                    RenderTarget.FillGeometry(pg, RobotBrush);
//                    RenderTarget.DrawGeometry(pg, BorderBrush, 0.01f);
//                }
//                else
//                {
//                    Ellipse ellipse = new Ellipse();
//                    ellipse.Point = P2DToP2F(state.Location);
//                    ellipse.RadiusY = ellipse.RadiusX = (state.Type == ObjectType.OurRobot) ? (float)RobotParameters.OurRobotParams.Diameter / 2 : (float)RobotParameters.OpponentParams.Diameter / 2;
//                    RenderTarget.FillEllipse(ellipse, RobotBrush);
//                    RenderTarget.DrawEllipse(ellipse, BorderBrush, 0.01f);
//                }
//                _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//                if (RobotID != 1000)
//                    if (_showTexts)
//                        DrawText(RobotID.ToString(), _textbrush, RenderTarget, new System.Drawing.RectangleF(state.Location, new System.Drawing.SizeF(0.05f, 0.09f)));

//            }
//        }
//        /// <summary>
//        /// initial Fiedl transformation
//        /// </summary>
//        public override void InitializeTransform()
//        {
//            System.Drawing.RectangleF rect = System.Drawing.RectangleF.FromLTRB((float)GameParameters.OppRightCorner.X, (float)GameParameters.OppRightCorner.Y, (float)GameParameters.OurLeftCorner.X, (float)GameParameters.OurLeftCorner.Y);
//            rect.Inflate(GameParameters.FieldMargins);
//            Transform = new Matrix3x2F(0, 100, -100, 0, 260, 340);
//            //Transform = MatrixCalculator.CreateMatrix(new System.Drawing.PointF[] { rect.Location, new System.Drawing.PointF(rect.Right, rect.Top), new System.Drawing.PointF(rect.Left, rect.Bottom) }, this.ClientRectangle);
//            //Transform = MatrixCalculator.CreateMatrix(new System.Drawing.PointF[] { rect.Location, new System.Drawing.PointF(rect.Right, rect.Top), new System.Drawing.PointF(rect.Left, rect.Bottom) }, new System.Drawing.Rectangle(0, 0, 660, 410));
//            //Transform = MatrixCalculator.Rotate(new Position2D(0, 0), Transform.Value, 90, System.Drawing.Drawing2D.MatrixOrder.Append);

//        }
//        /// <summary>
//        /// drwing texts
//        /// </summary>
//        /// <param name="text"></param>
//        /// <param name="RenderTarget"></param>
//        /// <param name="rect"></param>
//        public void DrawText(string text,SolidColorBrush brush, RenderTarget RenderTarget, System.Drawing.RectangleF rect)
//        {
//            rect.Location = new System.Drawing.PointF(rect.Location.X - rect.Width / 2, rect.Location.Y - rect.Height / 2);
//           // _textbrush = RenderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//            textFormat = dwriteFactory.CreateTextFormat("Berlin Sans FB Demi", 0.08f, FontWeight.Bold, FontStyle.Normal, FontStretch.Normal);
//            textFormat.TextAlignment = TextAlignment.Center;
//            textFormat.ParagraphAlignment = ParagraphAlignment.Center;
//            if (Mode == FieldOrientation.Verticaly)
//            {
//                RenderTarget.Transform = MatrixCalculator.Rotate(new Position2D(rect.Location.X + rect.Width / 2, rect.Location.Y + rect.Height / 2), RenderTarget.Transform, -90, System.Drawing.Drawing2D.MatrixOrder.Append);
//                RenderTarget.DrawText(text, textFormat, new RectF(rect.Left, rect.Top, rect.Right, rect.Bottom), brush);
//                RenderTarget.Transform = MatrixCalculator.Rotate(new Position2D(rect.Location.X + rect.Width / 2, rect.Location.Y + rect.Height / 2), RenderTarget.Transform, 90, System.Drawing.Drawing2D.MatrixOrder.Append);
//            }
//            else if (Mode == FieldOrientation.Horzintaly)
//                RenderTarget.DrawText(text, textFormat, new RectF(rect.Left, rect.Top, rect.Right, rect.Bottom), brush);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void FieldVisualizer_MouseClick(object sender, MouseEventArgs e)
//        {

//        }
//        /// <summary>
//        /// convert pixel to meter
//        /// </summary>
//        /// <param name="MouseLocation"></param>
//        /// <returns></returns>
//        Position2D pixeltoMetric(System.Drawing.Point MouseLocation)
//        {
//            double X = MouseLocation.X;
//            double Y = MouseLocation.Y;
//            X = (X - Transform.Value.M31) / Transform.Value.M11;
//            Y = (Y - Transform.Value.M32) / Transform.Value.M22;
//            return new Position2D(X, Y);
//        }
//        /// <summary>
//        /// draw bitmap by points
//        /// </summary>
//        /// <param name="val"></param>
//        /// <returns></returns>
//        System.Drawing.Bitmap SparseMarixToBitmap(List<System.Drawing.Point> val)
//        {
//            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(700, 500);
//            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 700, 500);
//            System.Drawing.Imaging.BitmapData bmpData = bm.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bm.PixelFormat);
//            IntPtr ptr = bmpData.Scan0;
//            int bytes = bm.Width * bm.Height * 4;
//            byte[] rgbValues = new byte[bytes];
//            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
//            for (int i = 0; i < val.Count; i++)
//            {
//                int counter = (val[i].Y * bmpData.Stride) + (val[i].X * 3);
//                rgbValues[counter] = 128;
//            }
//            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
//            bm.UnlockBits(bmpData);
//            return bm;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="p1"></param>
//        /// <param name="p2"></param>
//        public void Drawing(System.Drawing.Point p1, System.Drawing.Point p2)
//        {

//        }
//        /// <summary>
//        /// convert position2D to Point2F
//        /// </summary>
//        /// <param name="pos"></param>
//        /// <returns></returns>
//        private Point2F P2DToP2F(Position2D pos)
//        {
//            return new Point2F((float)pos.X, (float)pos.Y);
//        }
//        /// <summary>
//        /// Draw visual effects
//        /// </summary>
//        /// <param name="RenderTarget"></param>
//        private void DrawAccordingToStatus(RenderTarget RenderTarget)
//        {
//            if (Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam || Model.Status == GameStatus.IndirectFreeKick_Opponent || Model.Status == GameStatus.DirectFreeKick_Opponent || Model.Status == GameStatus.Stop)
//            {
//                if (Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam || Model.Status == GameStatus.IndirectFreeKick_Opponent || Model.Status == GameStatus.DirectFreeKick_Opponent)
//                {
//                    RobotBrush = RenderTarget.CreateSolidColorBrush(((Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam) ^ Model.OurMarkerISYellow) ? new ColorF(65f / 255f, 126f / 255f, 255 / 255f, 1f) : new ColorF(255f / 255f, 243f / 255f, 62f / 255f, 1f));
//                    BorderBrush = RenderTarget.CreateSolidColorBrush(((Model.Status == GameStatus.IndirectFreeKick_OurTeam || Model.Status == GameStatus.DirectFreeKick_OurTeam) ^ Model.OurMarkerISYellow) ? new ColorF(18f / 255f, 59f / 255f, 160f / 255f, 1f) : new ColorF(204f / 255f, 157f / 255f, 0f / 255f, 1f));
//                }
//                else
//                {
//                    RobotBrush = RenderTarget.CreateSolidColorBrush(new ColorF(255f / 255f, 0f / 255f, 0f / 255f, 1f));
//                    BorderBrush = RenderTarget.CreateSolidColorBrush(new ColorF(125f / 255f, 0f / 255f, 0f / 255f, 1f));
//                }
//                RobotBrush.Opacity = 0.1f;
//                BorderBrush.Opacity = 0.5f;
//                Circle cir = new Circle(new Position2D(0.5f * Math.Cos((float)_frameCount / 300f) + Model.BallState.Location.X, 0.5f * Math.Sin((float)_frameCount / 300f) + Model.BallState.Location.Y), 0.01f);
//                _hist2.Add(cir);
//                int count = 0;
//                RenderTarget.FillEllipse(new Ellipse(P2DToP2F(Model.BallState.Location), 0.5f, 0.5f), RobotBrush);
//                foreach (Circle item in _hist2)
//                {
//                    BorderBrush.Opacity = (float)count / 150;
//                    BorderBrush.Opacity = Math.Min(BorderBrush.Opacity, 0.05f);
//                    RenderTarget.FillEllipse(new Ellipse(P2DToP2F(item.Center), (float)item.Radious, (float)item.Radious), BorderBrush);
//                    count++;
//                }
//                if (_hist2.Count > 500)
//                    _hist2.RemoveAt(0);
//            }
//            else
//                _frameCount = 0;
//        }
//        /// <summary>
//        /// drawing fiil regions
//        /// </summary>
//        /// <param name="val"></param>
//        /// <param name="RenderTarget"></param>
//        private void DrawFillRegion(List<Position2D> val,ColorF color , RenderTarget renderTarget)
//        {
//            SolidColorBrush brush = renderTarget.CreateSolidColorBrush(color);
//            if (pg != null)
//                pg.Dispose();
//            pg = d2dfactory.CreatePathGeometry();
//            GeometrySink gs = pg.Open();
//            gs.BeginFigure(P2DToP2F(val[0]), FigureBegin.Filled);
//            for (int i = 1; i < val.Count; i++)
//                gs.AddLine(P2DToP2F(val[i]));
//            gs.AddLine(P2DToP2F(val[0]));
//            gs.EndFigure(FigureEnd.Closed);
//            gs.Close();
//            //_fieldMarkingPen.Opacity = 0.2f;
//            renderTarget.FillGeometry(pg, brush);
//            //_fieldMarkingPen.Opacity = 1f;
//        }
//        /// <summary>
//        /// field orientation state
//        /// </summary>
//        public enum FieldOrientation
//        {
//            Verticaly = 0,
//            Horzintaly = 1,
//        }
//        /// <summary>
//        /// drawing tokens
//        /// </summary>
//        /// <param name="renderTarget"></param>
//        /// <param name="pos"></param>
//        private void DrawToken(RenderTarget renderTarget,Position2D pos)
//        {
//            Line l1 = new Line(new Position2D(pos.X - 0.02, pos.Y), new Position2D(pos.X + 0.02, pos.Y), new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.01f));
//            Line l2 = new Line(new Position2D(pos.X, pos.Y - 0.02), new Position2D(pos.X, pos.Y + 0.02), new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.01f));
//            _textbrush=renderTarget.CreateSolidColorBrush(new ColorF(Colors.Black));
//            renderTarget.DrawLine(P2DToP2F(l1.Head), P2DToP2F(l1.Tail), _textbrush, 0.005f);
//            renderTarget.DrawLine(P2DToP2F(l2.Head), P2DToP2F(l2.Tail), _textbrush, 0.005f);
//            renderTarget.DrawEllipse(new Ellipse(P2DToP2F(pos), 0.02f, 0.02f), _textbrush, 0.01f);
//        }
//    }
//}
