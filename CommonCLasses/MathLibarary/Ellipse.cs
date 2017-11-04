using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Enterprise;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.CommonCLasses.MathLibrary
{
    public class Ellipse
    {
        private Pen _drawPen;
        private bool _isShown = false;
        private Position2D c;
        private double a;
        private double b;

        public Pen DrawPen
        {
            get { return _drawPen; }
            set { _drawPen = value; }
        }
        public bool IsShown
        {
            get { return _isShown; }
            set { _isShown = value; }
        }
        public bool PenIsChanged { get; set; }
        public bool IsFill { get; set; }
        public float Opacity { get; set; }
        public Position2D Center
        {
            get { return c; }
            set { c = value; }
        }
        public double A
        {
            get { return a; }
            set { a = value; }
        }
        public double B
        {
            get { return b; }
            set { b = value; }
        }


        public Ellipse()
        {

        }
        public Ellipse(Position2D center, double a, double b)
        {
            c = center;
            this.a = a;
            this.b = b;
            _drawPen = new Pen(Color.Black, 0.01f);
            PenIsChanged = true;
        }
        public Ellipse(Position2D center, double a, double b, Pen pen, bool isShown)
        {
            _isShown = isShown;
            c = center;
            this.a = a;
            this.b = b;
            _drawPen = pen;
            IsFill = false;
            PenIsChanged = true;
        }
        public Ellipse(Position2D center, double a, double b, Pen pen)
        {

            c = center;
            this.a = a;
            this.b = b;
            _drawPen = pen;
            IsFill = false;
            PenIsChanged = true;
        }
        public Ellipse(Position2D center, double a, double b, Pen pen, bool isFill, float opacity, bool isShown)
        {
            _isShown = isShown;
            c = center;
            this.a = a;
            this.b = b;
            _drawPen = pen;
            IsFill = isFill;
            Opacity = opacity;
            PenIsChanged = true;
        }


        public override string ToString()
        {
            return string.Format("Center={0} , a={1} , b={2}", c, a, b);
        }

        #region Intersect
        public List<Position2D> Intersect(Line l)
        {
            List<Position2D> res = new List<Position2D>();
            if (l.B != 0)
            {

                double K = Center.Y;
                double h = Center.X;
                double e = (l.C / -l.B) - Center.Y;
                double m = l.A / -l.B;
                double q = (l.C / -l.B) + (m * h);

                double d = (a * a) * (m * m) + (b * b) - (q * q) - (K * K) + (2 * q * K);

                if (d >= 0)
                {
                    double onePartX = h * (b * b) - m * (a * a) * e;
                    double onePartY = (b * b) * q + K * (a * a) * (m * m);
                    double makhraj = (a * a) * (m * m) + (b * b);
                    res.Add(new Position2D((onePartX + a * b * Math.Sqrt(Math.Abs(d))) / makhraj, (onePartY + a * b * m * Math.Sqrt(d)) / makhraj));
                    if (d > 0)
                        res.Add(new Position2D((onePartX - a * b * Math.Sqrt(d)) / makhraj, (onePartY - a * b * m * Math.Sqrt(d)) / makhraj));
                }
            }
            return res;
        }
        #endregion

        //public int GetTangent(Position2D P, out List<Line> TangentLines, out List<Position2D> TangentPoints)
        //{
        //    Vector2D vect = _center - P;
        //    double dist = vect.Size;
        //    TangentLines = new List<Line>();
        //    TangentPoints = new List<Position2D>();
        //    if (dist >= _radious)
        //    {
        //        Line l = new Line(P, _center);
        //        if (dist == _radious)
        //        {
        //            TangentPoints.Add(P);
        //            TangentLines.Add(l.PerpenducilarLineToPoint(_center));
        //            return 1;
        //        }
        //        else
        //        {
        //            double lineAngle = vect.AngleInRadians;
        //            double openingAngle = Math.Asin(_radious / dist);
        //            double tangentDist = Math.Sqrt(dist * dist - _radious * _radious);
        //            Vector2D v1 = Vector2D.FromAngleSize(lineAngle + openingAngle, tangentDist);
        //            TangentLines.Add(new Line(P, P + v1));
        //            TangentPoints.Add(P + v1);

        //            v1 = Vector2D.FromAngleSize(lineAngle - openingAngle, tangentDist);
        //            TangentLines.Add(new Line(P, P + v1));
        //            TangentPoints.Add(P + v1);
        //            return 2;
        //        }
        //    }
        //    else
        //        return 0;
        //}

        //public bool IsInCircle(Position2D P)
        //{
        //    if (this._center.DistanceFrom(P) < this._SmallDiameter)
        //        return true;
        //    return false;
        //}
        //public List<Position2D> Intersect(Ellipse Ellipse)
        //{
        //    List<Position2D> ret = new List<Position2D>();
        //    double d = this.Center.DistanceFrom(Ellipse.Center);

        //    double Aa = this._SmallDiameter;
        //    double Bb = this._LargeDiameter;
        //    double Xx = Ellipse._center.X;
        //    double Yy = Ellipse._center.Y;
        //    double X = 0;
        //    double Y = 0;


        //    double MoadeleEllipse = (X - Ellipse._center.X) * (X - Ellipse._center.X) / (Aa * Aa) + (X - Ellipse._center.Y) * (X - Ellipse._center.Y) / (Bb * Bb);
        //    MoadeleEllipse = 1;
        //    //double rA = this._SmallDiameter;
        //    //double rB = Ellipse._LargeDiameter;
        //    //double xA = this.Center.X;
        //    //double yA = this.Center.Y;
        //    //double xB = circle.Center.X;
        //    //double yB = circle.Center.Y;
        //    //double K = (1 / 4.0) * Math.Sqrt((Math.Pow((rA + rB), 2) - d * d) * (d * d - Math.Pow((rA - rB), 2)));
        //    //double X1 = (1 / 2.0) * (xB + xA) + (1 / 2.0) * (xB - xA) * (rA * rA - rB * rB) / (d * d) + 2 * (yB - yA) * K / (d * d);
        //    //double Y1 = (1 / 2.0) * (yB + yA) + (1 / 2.0) * (yB - yA) * (rA * rA - rB * rB) / (d * d) - 2 * (xB - xA) * K / (d * d);

        //    //double X2 = (1 / 2.0) * (xB + xA) + (1 / 2.0) * (xB - xA) * (rA * rA - rB * rB) / (d * d) - 2 * (yB - yA) * K / (d * d);
        //    //double Y2 = (1 / 2.0) * (yB + yA) + (1 / 2.0) * (yB - yA) * (rA * rA - rB * rB) / (d * d) + 2 * (xB - xA) * K / (d * d);

        //    ret.Add(new Position2D(X1, Y1));
        //    ret.Add(new Position2D(X2, Y2));
        //    return ret;
        //}

    }

}


