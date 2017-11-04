using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    /// <summary>
    /// Represnt Line as Ax+By+C=0
    /// </summary>
    public class Line
    {
        public bool PenIsChanged { get; set; }
        private double _a, _b, _c;
        /// <summary>
        /// ...+C
        /// </summary>
        public double C
        {
            get { return _c; }
            set { _c = value; }
        }
        /// <summary>
        /// ... + By + ...
        /// </summary>
        public double B
        {
            get { return _b; }
            set { _b = value; }
        }
        /// <summary>
        /// Ax + ...
        /// </summary>
        public double A
        {
            get { return _a; }
            set { _a = value; }
        }
        /// <summary>
        /// Angle of Line atan2(b,a)
        /// </summary>
        public double Angle
        {
            get { return Math.Atan2(_b, _a); }
        }
        private Position2D _head;
        /// <summary>
        /// Head of line
        /// </summary>
        public Position2D Head
        {
            get { return _head; }
            set { _head = value; }
        }
        private Position2D _tail;
        public Position2D Tail
        {
            get { return _tail; }
            set { _tail = value; }
        }
        /// <summary>
        /// representing a line between 2 Position
        /// </summary>
        /// <param name="P1">1st Position</param>
        /// <param name="P2">2nd Position</param>
        public Line(Position2D P1, Position2D P2)
        {
            
            this._a = P2.Y - P1.Y;
            this._b = P1.X - P2.X;
            this._c = -(_a * P1.X + _b * P1.Y);
            _head = P1;
            _tail = P2;
            _drawPen = new Pen(Brushes.Black, 0.01f);
            PenIsChanged = true;
        }
        /// <summary>
        /// representing a line between 2 Position And a DrawPen
        /// </summary>
        /// <param name="P1">1st Position</param>
        /// <param name="P2">2nd Position</param>
        /// <param name="pen">Draw Pen</param>
        public Line(Position2D P1, Position2D P2,Pen pen)
        {
            this._a = P2.Y - P1.Y;
            this._b = P1.X - P2.X;
            this._c = -(_a * P1.X + _b * P1.Y);
            _head = P1;
            _tail = P2;
            _drawPen = pen;
            PenIsChanged = true;
        }
        /// <summary>
        /// Construct withoun any input
        /// </summary>
        public Line()
        {
        }
        /// <summary>
        /// representing a line with A , B , C in Ax+By+C = 0
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <param name="c">C</param>
        public Line(double a,double b, double c)
        {
            this._a = a;
            this._b = b;
            this._c = c;
        }
        private bool _isShown = false;
        /// <summary>
        /// 
        /// </summary>
        public bool IsShown
        {
            get { return _isShown; }
            set { _isShown = value; }
        }
        private Pen _drawPen;
        /// <summary>
        /// Pen that using for drawing the line
        /// </summary>
        public Pen DrawPen
        {
            get { return _drawPen; }
            set { _drawPen = value; }
        }
        /// <summary>
        /// Distance a Position from a Line "Size of Perpenducilar Line between Point and Perpenducilar Position on the line"
        /// </summary>
        /// <param name="P">Position that the distance calculate from</param>
        /// <returns>Distance of a position2D and Line</returns>
        public double Distance(Position2D P)
        {
            return Math.Abs((_a * P.X + _b * P.Y + _c)) / Math.Sqrt(_a * _a + _b * _b);
        }
        /// <summary>
        /// Perpenducilar Line between Point and Perpenducilar Position on the line
        /// </summary>
        /// <param name="P">Position that the Perpenducilar calculate from</param>
        /// <returns>a Perpenducilar Line</returns>
        public Line PerpenducilarLineToPoint(Position2D From)
        {
            return new Line(this.B, -this.A, -(this.B * From.X + -this.A * From.Y));
              //line.A * target.Y - line.B * target.X;
        }
        /// <summary>
        /// Intersection between lines
        /// </summary>
        /// <param name="l2">Target Line</param>
        /// <returns>if there is a Intersection Return the Position2D</returns>
        public Position2D? IntersectWithLine(Line l2)
        {
            Position2D a = new Position2D();
            double det = this.A * l2.B - l2.A * this.B;
            if (Math.Abs(det) > 0.0001f)
            {
                a.X = ((l2.C * this.B - this.C * l2.B) / det);
                a.Y = ((this.C * l2.A - l2.C * this.A) / det);
                return a;
            }
            else
                return null;
        }
        public bool IntersectWithLine(Line l2, ref Position2D Intersect)
        {
            double det = this.A * l2.B - l2.A * this.B;
            if (Math.Abs(det) > 0.0001f)
            {
                Intersect.X = ((l2.C * this.B - this.C * l2.B) / det);
                Intersect.Y = ((this.C * l2.A - l2.C * this.A) / det);
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// String of a Line
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}x+{1}y+{2}=0", _a, _b, _c);
        }
        /// <summary>
        /// Calculating Y with the specefic X
        /// </summary>
        /// <param name="x">X coordinate of a Position</param>
        /// <returns>Y of the position that is in line</returns>
        public Position2D CalculateY(double x)
        {
            Position2D res = new Position2D();
            res.X = x;
            res.Y = (-this.C - this.A * x) / this.B;
            return res;
        }
        public Position2D CalculateX(double y)
        {
            Position2D res = new Position2D();
            res.Y = y;
            res.X = (-this.C - this.B * y) / this.A;
            return res;
        }
    }
}
