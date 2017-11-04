using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//using Microsoft.WindowsAPICodePack.DirectX;
//using Microsoft.WindowsAPICodePack.DirectX.Controls;
//using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
//using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using SlimDX.Direct2D;


using MRL.SSL.CommonControls;
using MRL.SSL.CommonControls.Direct2D;
using SlimDX;

namespace MRL.SSL.CommonControls.D2DControls
{
    public partial class D2DControl : UserControl, IDisposable
    {
        public Factory d2dfactory;
        public WindowRenderTarget renderTarget;

        public bool WheelActive { get; set; }

        private Matrix3x2? _transform = null;
        public Matrix3x2? Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        bool rightisDown = false;

        public D2DControl()
        {
            InitializeComponent();
            WheelActive = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, true);

            d2dfactory = new Factory();//FactoryType.Multithreaded
            RenewRenderTarger();
            this.MouseWheel += new MouseEventHandler(D2DControl_MouseWheel);
            this.MouseMove += new MouseEventHandler(D2DControl_MouseMove);
            this.Resize += new EventHandler(D2DControl_Resize);
            this.MouseDown += new MouseEventHandler(D2DControl_MouseDown);
            this.MouseUp += new MouseEventHandler(D2DControl_MouseUp);
            _fieldColor = new SolidColorBrush(renderTarget, new Color4(Color.Green));
            _fieldColor.Opacity = .5f;
        }

        private void RenewRenderTarger()
        {
            Size size = new Size(this.Width, this.Height);
            WindowRenderTargetProperties hwndProps = new WindowRenderTargetProperties() { Handle = this.Handle, PixelSize = size, PresentOptions = PresentOptions.Immediately };
            renderTarget = new WindowRenderTarget(d2dfactory, hwndProps);
        }

        void D2DControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                rightisDown = false;
        }

        void D2DControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                rightisDown = true;
        }

        void D2DControl_MouseWheel(object sender, MouseEventArgs e)
        {

            if (this.Focus())
                if (Transform.HasValue && WheelActive && !rightisDown)
                {
                    float scale = Convert.ToSingle(Math.Pow(1.1f, (double)((float)e.Delta / 120f)));

                    Transform = MatrixCalculator.Scale(Transform.Value, scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                    Transform = MatrixCalculator.Translate(Transform.Value, -e.X * (scale - 1), -e.Y * (scale - 1), System.Drawing.Drawing2D.MatrixOrder.Append);
                    if (UserZoomed != null)
                        UserZoomed(this, EventArgs.Empty);
                    Invalidate();
                }
                else if (Transform.HasValue && rightisDown)
                {
                    Transform = MatrixCalculator.Rotate(new MRL.SSL.CommonClasses.MathLibrary.Position2D(0, 0), Transform.Value, 90, System.Drawing.Drawing2D.MatrixOrder.Append);
                    Invalidate();
                }

        }

        public event EventHandler UserZoomed;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                renderTarget.Dispose();
                d2dfactory.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public virtual void InitializeTransform()
        {
            _transform = Matrix3x2.Identity;

        }

        SolidColorBrush _fieldColor;

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (renderTarget != null)
            {
                renderTarget.BeginDraw();
                if (!_transform.HasValue)
                    InitializeTransform();
                renderTarget.Transform = _transform.Value;
                renderTarget.Clear(_fieldColor.Color);
                OnPaintContent();
                renderTarget.EndDraw();
            }
            base.OnPaint(pe);
        }

        protected virtual void OnPaintContent()
        {

        }

        private void D2DControl_Resize(object sender, EventArgs e)
        {
            Size resize = new Size(this.Width, this.Height);
            if (renderTarget != null)
                renderTarget.Resize(resize);


        }

        private void D2DControl_Click(object sender, EventArgs e)
        {
            this.Focus();
        }

        Point? LastMouseLoc = null;

        private void D2DControl_MouseMove(object sender, MouseEventArgs e)
        {

            if (Transform.HasValue)
            {
                if ((Control.MouseButtons & MouseButtons.Middle) != 0 && LastMouseLoc.HasValue)
                {
                    Transform = MatrixCalculator.Translate(Transform.Value, e.X - LastMouseLoc.Value.X, e.Y - LastMouseLoc.Value.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
                    Invalidate();
                }
                if ((Control.MouseButtons & MouseButtons.Middle) != 0)
                    LastMouseLoc = e.Location;
                else
                    LastMouseLoc = null;
            }
        }

        public SlimDX.Direct2D.Brush ToBrush(System.Drawing.Color source)
        {
            SlimDX.Direct2D.Brush b = new SlimDX.Direct2D.SolidColorBrush(renderTarget, new Color4(1f, source.R / 255f, source.G / 255f, source.B / 255f));
            return b;
        }

        public SlimDX.Direct2D.StrokeStyle ToStrockStyle(System.Drawing.Pen source)
        {
            SlimDX.Direct2D.StrokeStyleProperties st = new SlimDX.Direct2D.StrokeStyleProperties()
            {
                StartCap = (SlimDX.Direct2D.CapStyle)((int)source.StartCap),
                EndCap = (SlimDX.Direct2D.CapStyle)((int)source.EndCap),
                DashCap = (SlimDX.Direct2D.CapStyle)((int)source.DashCap),
                LineJoin = (SlimDX.Direct2D.LineJoin)((int)source.LineJoin),
                MiterLimit = 1f,
                DashStyle = (SlimDX.Direct2D.DashStyle)((int)source.DashStyle),
                DashOffset = source.DashOffset
            };
            SlimDX.Direct2D.StrokeStyle strock = new SlimDX.Direct2D.StrokeStyle(d2dfactory, st);
            return strock;
        }
    }
}
