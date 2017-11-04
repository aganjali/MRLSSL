using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Enterprise;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    /// <summary>
    /// Representing a circle with a Position2D Center and a Radiouse
    /// </summary>
    public class Circle
    {
        public bool PenIsChanged { get; set; }
        public bool IsFill { get; set; }
        public float Opacity { get; set; }
        private Position2D _center;
        /// <summary>
        /// Center of Circle
        /// </summary>
        public Position2D Center
        {
            get { return _center; }
            set { _center = value; }
        }
        private double _radious;
        /// <summary>
        /// Radiouse of Circle
        /// </summary>
        public double Radious
        {
            get { return _radious; }
            set { _radious = value; }
        }
        /// <summary>
        /// Representing a circle with a Position2D Center and a Radiouse
        /// </summary>
        /// <param name="center"> Center </param>
        /// <param name="radious"> Radiouse</param>
        public Circle(Position2D center, double radious)
        {
            _center = center;
            _radious = radious;
            _drawPen = new Pen(Color.Black, 0.01f);
            PenIsChanged = true;
        }
        /// <summary>
        /// Construct withot any input
        /// </summary>
        public Circle()
        {
        }
        /// <summary>
        /// Representing a circle with a Position2D Center, a Radiouse and Draw pen
        /// </summary>
        /// <param name="center">Center</param>
        /// <param name="radious">Radiuse</param>
        /// <param name="pen">Pen</param>
        public Circle(Position2D center, double radious, Pen pen, bool isShown)
        {
            _isShown = isShown;
            _center = center;
            _radious = radious;
            _drawPen = pen;
            IsFill = false;
            PenIsChanged = true;
        }
        public Circle(Position2D center, double radious, Pen pen)
        {

            _center = center;
            _radious = radious;
            _drawPen = pen;
            IsFill = false;
            PenIsChanged = true;
        }
        public Circle(Position2D center, double radious, Pen pen, bool isFill, float opacity, bool isShown)
        {
            _isShown = isShown;
            _center = center;
            _radious = radious;
            _drawPen = pen;
            IsFill = isFill;
            Opacity = opacity;
            PenIsChanged = true;
        }
        private Pen _drawPen;
        /// <summary>
        /// Pen that using for drawing the circle
        /// </summary>
        public Pen DrawPen
        {
            get { return _drawPen; }
            set { _drawPen = value; }
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
        /// <summary>
        /// String of Circle
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("C={0},R={1}", _center, _radious);
        }
        /// <summary>
        /// Calculate intersections between a line and a circle
        /// </summary>
        /// <param name="l">Target Line</param>
        /// <returns>if there is any intersection,List of Position2D </returns>
        public List<Position2D> Intersect(Line l)
        {

            try
            {
                double epsilon = 0.0001;
                double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
                bool first = false, second = false;
                if (Math.Abs(l.B) > epsilon)
                {
                    double aa = (l.A * l.A + l.B * l.B);
                    double bb = 2 * (l.B * l.C - l.A * l.A * _center.Y + l.A * l.B * _center.X);
                    double cc = l.C * l.C - l.A * l.A * _radious * _radious + l.A * l.A * _center.Y * _center.Y + l.A * l.A * _center.X * _center.X + 2 * l.A * l.C * _center.X;
                    double delta = bb * bb - 4 * aa * cc;
                    if (delta < 0)
                        return new List<Position2D>();
                    delta = Math.Sqrt(delta);
                    y1 = (-bb + delta) / (2 * aa);
                    y2 = (-bb - delta) / (2 * aa);
                    double deltax = _radious * _radious - (y1 - _center.Y) * (y1 - _center.Y);
                    if (deltax >= 0)
                    {

                        deltax = Math.Sqrt(deltax);
                        double tmpx1 = deltax + _center.X;
                        double tmpx2 = -deltax + _center.X;
                        if (Math.Abs(l.A * tmpx1 + l.B * y1 + l.C) < epsilon)
                        {
                            x1 = tmpx1;
                        }
                        else
                            x1 = tmpx2;
                        first = true;
                    }
                    deltax = _radious * _radious - (y2 - _center.Y) * (y2 - _center.Y);
                    if (deltax >= 0)
                    {
                        deltax = Math.Sqrt(deltax);
                        double tmpx1 = deltax + _center.X;
                        double tmpx2 = -deltax + _center.X;
                        if (Math.Abs(l.A * tmpx2 + l.B * y2 + l.C) < epsilon)
                        {
                            x2 = tmpx2;
                        }
                        else
                            x2 = tmpx1;
                        second = true;
                    }
                }
                else if (Math.Abs(l.A) > epsilon)
                {
                    x1 = -l.C / l.A;
                    x2 = x1;
                    double deltay = _radious * _radious - (x1 - _center.X) * (x1 - _center.X);
                    if (deltay < 0)
                        return new List<Position2D>();
                    deltay = Math.Sqrt(deltay);
       
                    y1 = deltay + _center.Y;
                    y2 = -deltay + _center.Y;
                    first = true;
                    second = true;
                }
                else
                    return new List<Position2D>();
                List<Position2D> intersections = new List<Position2D>();
                Position2D p1 = new Position2D(x1, y1);
                Position2D p2 = new Position2D(x2, y2);
                if (first)
                    intersections.Add(p1);
                if (second && (intersections.Count == 0 || p1.DistanceFrom(p2) > epsilon))
                    intersections.Add(p2);
                return intersections;

                //Line perp = l.PerpenducilarLineToPoint(_center);
                //Position2D? tmp = l.IntersectWithLine(perp);
                //Position2D perpfoot;
                //if (!tmp.HasValue)
                //    return new List<Position2D>();
                //perpfoot = tmp.Value;
                //double dist = (perpfoot - _center).Size;
                //List<Position2D> intersections = new List<Position2D>();
                //if (dist < _radious)
                //{
                //    Vector2D v = new Vector2D(-perp.B, perp.A);
                //    double perpAngle = Math.Atan2(v.Y, v.X);

                //    double openingAngle = Math.Acos(dist / _radious);
                //    Vector2D v2 = Vector2D.FromAngleSize(perpAngle + openingAngle, 1);
                    
                //    if (Math.Abs(_center.Y - l.CalculateY(_center.X).Y) < 0.0001)
                //        openingAngle = Math.PI / 2;
                //    intersections.Add(_center + Vector2D.FromAngleSize(perpAngle + openingAngle, _radious));
                //    intersections.Add(_center + Vector2D.FromAngleSize(perpAngle - openingAngle, _radious));
                //}
                //else if (dist == _radious)
                //    intersections.Add(perpfoot);
                return intersections;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return new List<Position2D>();
            }
        }
        /// <summary>
        /// Tangant ?
        /// </summary>
        /// <param name="P"></param>
        /// <param name="TangentLines"></param>
        /// <param name="TangentPoints"></param>
        /// <returns></returns>
        public int GetTangent(Position2D P, out List<Line> TangentLines, out List<Position2D> TangentPoints)
        {
            Vector2D vect = _center - P;
            double dist = vect.Size;
            TangentLines = new List<Line>();
            TangentPoints = new List<Position2D>();
            if (dist >= _radious)
            {
                Line l = new Line(P, _center);
                if (dist == _radious)
                {
                    TangentPoints.Add(P);
                    TangentLines.Add(l.PerpenducilarLineToPoint(_center));
                    return 1;
                }
                else
                {
                    double lineAngle = vect.AngleInRadians;
                    double openingAngle = Math.Asin(_radious / dist);
                    double tangentDist = Math.Sqrt(dist * dist - _radious * _radious);
                    Vector2D v1 = Vector2D.FromAngleSize(lineAngle + openingAngle, tangentDist);
                    TangentLines.Add(new Line(P, P + v1));
                    TangentPoints.Add(P + v1);

                    v1 = Vector2D.FromAngleSize(lineAngle - openingAngle, tangentDist);
                    TangentLines.Add(new Line(P, P + v1));
                    TangentPoints.Add(P + v1);
                    return 2;
                }
            }
            else
                return 0;
        }
        public bool IsInCircle(Position2D P)
        {
            if (this._center.DistanceFrom(P) < this._radious)
                return true;
            return false;
        }
        public List<Position2D> Intersect(Circle circle)
        {
            List<Position2D> ret = new List<Position2D>();
            double d = this.Center.DistanceFrom(circle.Center);
            if ((d > this.Radious + circle.Radious) || (d < Math.Abs(this.Radious - circle.Radious)))
            {
                return new List<Position2D>();
            }
            else if (d == 0 && this.Radious == circle.Radious)
            {
                return new List<Position2D>();
            }
            else
            {
                double rA = this.Radious;
                double rB = circle.Radious;
                double xA = this.Center.X;
                double yA = this.Center.Y;
                double xB = circle.Center.X;
                double yB = circle.Center.Y;


                double K = (1 / 4.0) * Math.Sqrt((Math.Pow((rA + rB), 2) - d * d) * (d * d - Math.Pow((rA - rB), 2)));
                double X1 = (1 / 2.0) * (xB + xA) + (1 / 2.0) * (xB - xA) * (rA * rA - rB * rB) / (d * d) + 2 * (yB - yA) * K / (d * d);
                double Y1 = (1 / 2.0) * (yB + yA) + (1 / 2.0) * (yB - yA) * (rA * rA - rB * rB) / (d * d) - 2 * (xB - xA) * K / (d * d);

                double X2 = (1 / 2.0) * (xB + xA) + (1 / 2.0) * (xB - xA) * (rA * rA - rB * rB) / (d * d) - 2 * (yB - yA) * K / (d * d);
                double Y2 = (1 / 2.0) * (yB + yA) + (1 / 2.0) * (yB - yA) * (rA * rA - rB * rB) / (d * d) + 2 * (xB - xA) * K / (d * d);

                ret.Add(new Position2D(X1, Y1));
                ret.Add(new Position2D(X2, Y2));
            }
            return ret;
        }

    }
}
