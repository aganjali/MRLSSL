using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Enterprise;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    /// <summary>
    /// MRL.SSL vectors will represent by Vector2D
    /// </summary>
    public struct Vector2D
    {
        const double Epsilon = 1e-5;
        /// <summary>
        /// represent a 2d Vector in X and Y axis
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public Vector2D(double X, double Y)
        {
            _x = X;
            _y = Y;
        }

        double _x, _y;
        /// <summary>
        /// X axis
        /// </summary>
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        /// Y axis
        /// </summary>
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }
        /// <summary>
        /// gets the Size of a vector2D
        /// </summary>
        public double Size
        {
            get { return Math.Sqrt(_x * _x + _y * _y); }
        }
        /// <summary>
        /// gets a Aquare Size of a Vector2D 
        /// </summary>
        public double SquareSize
        {
            get { return _x * _x + _y * _y; }
        }
        /// <summary>
        /// scale x and y with a
        /// </summary>
        /// <param name="a">Scale varibale size</param>
        public void Scale(double a)
        {
            _x *= a;
            _y *= a;
        }
        /// <summary>
        /// Normalize a Vector2D in size of 1 
        /// </summary>
        public void Normnalize()
        {
            double size = this.Size;
            if (size < Epsilon)
                _x = _y = 0;
            else
            {
                _x /= size;
                _y /= size;
            }
        }
        /// <summary>
        /// Normalize a Vector2D in a specefic size
        /// </summary>
        /// <param name="NewLength">length of normalization</param>
        public void NormalizeTo(double NewLength)
        {
            double size = this.Size;
            if (size < Epsilon)
                _x = _y = 0;
            else
            {
                _x *= NewLength / size;
                _y *= NewLength / size;
            }
        }
        /// <summary>
        /// Normalize the Vector2D and return it
        /// </summary>
        /// <returns>Normalized Vector2D</returns>
        public Vector2D GetNormnalizedCopy()
        {
            Vector2D temp = new Vector2D(_x, _y);
            temp.Normnalize();
            return temp;
        }
        /// <summary>
        /// Normalize in a specefic size and return a copy
        /// </summary>
        /// <param name="NewLength">length of normalization</param>
        /// <returns>Normalize Vector2D</returns>
        public Vector2D GetNormalizeToCopy(double NewLength)
        {
            Vector2D temp = new Vector2D(_x, _y);
            temp.NormalizeTo(NewLength);
            return temp;
        }
        /// <summary>
        /// A Zero Vector2D with X = 0 and Y = 0
        /// </summary>
        public static Vector2D Zero
        {
            get { return new Vector2D(0, 0); }
        }
        /// <summary>
        /// Calculate Angle between 2 Vector2D in radians
        /// </summary>
        /// <param name="P1">1st Vector</param>
        /// <param name="P2">2nd Vector</param>
        /// <returns>Angle between 2 vectors in Radian</returns>
        public static double AngleBetweenInRadians(Vector2D P1, Vector2D P2)
        {
            double a1 = Math.Atan2(P1.Y, P1.X), a2 = Math.Atan2(P2.Y, P2.X);
            double d = a1 - a2;
            while (d > Math.PI)
                d -= 2 * Math.PI;

            while (d < -Math.PI)
                d += 2 * Math.PI;
            return d;
        }
        /// <summary>
        /// Calculate Angle between 2 Vector2D in degree
        /// </summary>
        /// <param name="P1">1st Vector</param>
        /// <param name="P2">2nd Vector</param>
        /// <returns>Angle between 2 vectors in degree</returns>
        public static double AngleBetweenInDegrees(Vector2D P1, Vector2D P2)
        {
            return AngleBetweenInRadians(P1, P2) * 180 / Math.PI;
        }
        /// <summary>
        /// Bisector of to Vector
        /// </summary>
        /// <param name="v1">1st Vector</param>
        /// <param name="v2">2nd Vector</param>
        /// <param name="common">the common Position between vectors v1 and v2</param>
        /// <returns>a Line that is besictor line of the vectors</returns>
        public static Line Bisector(Vector2D v1, Vector2D v2, Position2D common)
        {
            double AngleBetween = AngleBetweenInRadians(v1, v2);
            return new Line(common, common + FromAngleSize(AngleBetween / 2 + v2.AngleInRadians, 1));
        }
        /// <summary>
        /// Scaling the vector
        /// </summary>
        /// <param name="P">The Vector</param>
        /// <param name="a">Scale size</param>
        /// <returns>sacaled vector</returns>
        public static Vector2D operator *(Vector2D P, double a)
        {
            return new Vector2D(P.X * a, P.Y * a);
        }
        /// <summary>
        /// Scaling the vector
        /// </summary>
        /// <param name="P">The Vector</param>
        /// <param name="a">Scale size</param>
        /// <returns>sacaled vector</returns>
        public static Vector2D operator *(double a, Vector2D P)
        {
            return new Vector2D(P.X * a, P.Y * a);
        }

        public static Vector3D operator *(Vector2D v1, Vector2D v2)
        {
            return new Vector3D(0, 0, v1.X * v2.Y - v1.Y * v2.X);
        }
        /// <summary>
        /// Deviding the vector "scaling in 1/scale" 
        /// </summary>
        /// <param name="P">The Vector</param>
        /// <param name="a">Scale size</param>
        /// <returns>devided vector</returns>
        public static Vector2D operator /(Vector2D P, double a)
        {
            return new Vector2D(P._x / a, P._y / a);
        }
        /// <summary>
        /// Adding 2 vectors
        /// </summary>
        /// <param name="Left">1st vector</param>
        /// <param name="Right">2nd vector</param>
        /// <returns>Added vector</returns>
        public static Vector2D operator +(Vector2D Left, Vector2D Right)
        {
            return new Vector2D(Left._x + Right._x, Left._y + Right._y);
        }
        /// <summary>
        /// Subtracting 2 vectors
        /// </summary>
        /// <param name="Left">1st vector</param>
        /// <param name="Right">2nd vector</param>
        /// <returns>Subtracted vector</returns>
        public static Vector2D operator -(Vector2D Left, Vector2D Right)
        {
            return new Vector2D(Left._x - Right._x, Left._y - Right._y);
        }
        /// <summary>
        /// scaling by -1
        /// </summary>
        /// <param name="V"></param>
        /// <returns>-V</returns>
        public static Vector2D operator -(Vector2D V)
        {
            return new Vector2D(-V._x, -V._y);
        }
        /// <summary>
        /// checking equality of 2 Vector2D
        /// </summary>
        /// <param name="Left">1st Vector</param>
        /// <param name="Right">2nd Vector</param>
        /// <returns>True if they are equal</returns>
        public static bool operator ==(Vector2D Left, Vector2D Right)
        {
            return Math.Abs(Left._x - Right._x) < Epsilon && Math.Abs(Left._y - Right._y) < Epsilon;
        }
        /// <summary>
        /// checking NOT equality of 2 Vector2D
        /// </summary>
        /// <param name="Left">1st Vector</param>
        /// <param name="Right">2nd Vector</param>
        /// <returns>True if they are NOT equal</returns>
        public static bool operator !=(Vector2D Left, Vector2D Right)
        {
            return !(Left == Right);
        }
        /// <summary>
        /// Inner Product of 2 vectors : v1.x * v2.x + v1.y * v2.y 
        /// </summary>
        /// <param name="Left">1st Vector</param>
        /// <param name="Right">2nd Vector</param>
        /// <returns>the inner product value</returns>
        public static double InnerProduct(Vector2D Left, Vector2D Right)
        {
            return Left._x * Right._x + Left._y * Right._y;
        }
        /// <summary>
        /// Inner Product with another vector
        /// </summary>
        /// <param name="V">the target Vector</param>
        /// <returns>the inner product value</returns>
        public double InnerProduct(Vector2D V)
        {
            return _x * V._x + _y * V._y;
        }
        /// <summary>
        /// Castabele to SizeF
        /// </summary>
        /// <param name="V">The Vector</param>
        /// <returns>Casting SizeF from Vector2D</returns>
        public static implicit operator System.Drawing.SizeF(Vector2D V)
        {
            return new System.Drawing.SizeF((float)V._x, (float)V._y);
        }
        /// <summary>
        /// Castabele to Vectore
        /// </summary>
        /// <param name="V">The Vector2D</param>
        /// <returns>Casting Vector from Vector2D</returns>
        public static implicit operator System.Windows.Vector(Vector2D V)
        {
            return new System.Windows.Vector((float)V._x, (float)V._y);
        }
        /// <summary>
        /// Castabele from Vectore
        /// </summary>
        /// <param name="V">The Vector</param>
        /// <returns>Casting Vector2D from Vector</returns>
        public static implicit operator Vector2D(System.Windows.Vector v)
        {
            return new Vector2D(v.X, v.Y);
        }
        /// <summary>
        /// Creates a Vector from its direction and Size
        /// </summary>
        /// <param name="Angle">The direction of the Vector in radians</param>
        /// <param name="Size">The size of the vector</param>
        /// <returns>a Vector from its direction and Size</returns>
        public static Vector2D FromAngleSize(double Angle, double Size)
        {
            //Vector2D p = new Vector2D(Size * Math.Cos(Angle), Size * Math.Sin(Angle)); 
            return new Vector2D(Size * Math.Cos(Angle), Size * Math.Sin(Angle));
        }
        /// <summary>
        /// Angle with Atan2(y,X) in Radians
        /// </summary>
        public double AngleInRadians
        {
            get
            {
                return Math.Atan2(Y, X);
            }
        }
        /// <summary>
        /// Angle with Atan2(y,X) in Degree
        /// </summary>
        public double AngleInDegrees
        {
            get
            {
                return AngleInRadians * 180 / Math.PI;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// String of Vector
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0},{1})", _x, _y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector2D))
            {
                return false;
            }
            Vector2D tf = (Vector2D)obj;
            return (((tf.X == this.X) && (tf.Y == this.Y)) && tf.GetType().Equals(base.GetType()));
        }

        public Vector2D GetRotate(double a)
        {
            return FromAngleSize(a, Size);
        }

        public void Rotate(double a)
        {
            Vector2D v = FromAngleSize(a, Size);
            _x = v.X;
            _y = v.Y;
        }

        public Vector2D GetPerp()
        {
            return new Vector2D(-_y, _x);
        }

        public void perp()
        {
            double tmp = _x;
            _x = -_y;
            _y = tmp;
        }
        public static double offset_to_line(Position2D x0, Position2D x1, Position2D p)
        {
            //Vector2D n = Vector2D.Zero;
            //// get normal to line
            //n = (x1 - x0).GetPerp().GetNormnalizedCopy();

            //return (n.InnerProduct(p - x0));
            double x = x1.X - x0.X;
            double y = x1.Y - x0.Y;
            double size = Math.Sqrt(x * x + y * y);
            if (size < Epsilon)
            {
                return 0;
            }
            return Math.Abs(((-y * (p.X -x0.X) + x * (p.Y - x0.Y)) / size));
        }

        public static double offset_along_line(Position2D x0, Position2D x1, Position2D p)
        {
            Vector2D n, v;

            // get normal to line
            n = x1 - x0;
            n.Normnalize();

            v = p - x0;

            return (n.InnerProduct(v));
        }

        public static Position2D? Intersect(Position2D p1, Vector2D v1, Position2D p2, Vector2D v2)
        {
            Vector2D p1p2Vec = p2 - p1;
            Vector2D p2p1Vec = p1 - p2;
            double a1 = Vector2D.AngleBetweenInRadians(p1p2Vec, v1);
            double a2 = -Vector2D.AngleBetweenInRadians(p2p1Vec, v2);
            double teta = Math.PI - Math.Abs(a1) - Math.Abs(a2);
            double d = p1p2Vec.Size;
            if (Math.Sign(a1 * a2) == 1 && (Math.Abs(a1) + Math.Abs(a2)) < Math.PI)
            {
                double r = Math.Sin(Math.Abs(a2)) * d / Math.Sin(teta);
                return p1 + v1.GetNormalizeToCopy(r);
            }
            return null;

        }

        public Position2D PrependecularPoint(Position2D Start, Position2D From)
        {
            try
            {
                Vector2D startFromVec = From - Start;
                Vector2D fromStartVec = Start - From;
                double teta = Vector2D.AngleBetweenInRadians(this, startFromVec);
                double s = 1;
                if (Math.Abs(teta) > Math.PI / 2)
                {
                    s = -1;
                    teta = (Math.PI - Math.Abs(teta)) * Math.Sign(teta);
                }
                double d = Math.Abs(startFromVec.Size * Math.Sin(teta));
                double alfa = Math.PI / 2 - Math.Abs(teta);
                return From + Vector2D.FromAngleSize(fromStartVec.AngleInRadians - s * Math.Sign(teta) * alfa, d);
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return new Position2D();
            }
        }

        public static bool IsBetween(Vector2D right, Vector2D left, Vector2D v)
        {
            Vector3D n = left * right;
            double innerL = n.InnerProduct(left * v), innerR = n.InnerProduct(v * right);
            return innerL >= 0 && innerR >= 0;

        }
        public static bool IsBetweenWithDirection(Vector2D right, Vector2D left, Vector2D v)
        {
            double angle = right.AngleInRadians - left.AngleInRadians;
            if (angle < 0)
                angle += (2 * Math.PI);
            double anglev = v.AngleInRadians - left.AngleInRadians;
            if (anglev < 0)
                anglev += (2 * Math.PI);
            return anglev <= angle;
        }
    }
}
