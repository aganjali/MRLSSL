using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.CommonClasses.MathLibrary
{
	public struct Vector3D
	{
        const double Epsilon = 1e-5;
        /// <summary>
        /// represent a 3d Vector in X , Y , Z axis
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        
        public Vector3D(double qX, double qY, double qZ)
        {
            _x = qX;
            _y = qY;
            _z = qZ;
        }

        double _x, _y, _z;
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

        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }
        /// <summary>
        /// gets the Size of a vector2D
        /// </summary>
        public double Size
        {
            get { return Math.Sqrt(_x * _x + _y * _y + _z*_z); }
        }
        /// <summary>
        /// gets a Aquare Size of a Vector2D 
        /// </summary>
        public double SquareSize
        {
            get { return _x * _x + _y * _y + _z*_z; }
        }
        /// <summary>
        /// scale x and y with a
        /// </summary>
        /// <param name="a">Scale varibale size</param>
        public void Scale(double a)
        {
            _x *= a;
            _y *= a;
            _z *= a;
        }
        /// <summary>
        /// Normalize a Vector2D in size of 1 
        /// </summary>
        public void Normalize()
        {
            double size = this.Size;
            if (size < Epsilon)
                _x = _z = _y = 0;
            else
            {
                _x /= size;
                _y /= size;
                _z /= size;
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
                _x = _y =  _z = 0;
            else
            {
                _x *= NewLength / size;
                _y *= NewLength / size;
                _z *= NewLength / size;
            }
        }
        /// <summary>
        /// Normalize the Vector2D and return it
        /// </summary>
        /// <returns>Normalized Vector2D</returns>
        public Vector3D GetNormnalizedCopy()
        {
            Vector3D temp = new Vector3D(_x, _y, _z);
            temp.Normalize();
            return temp;
        }
        /// <summary>
        /// Normalize in a specefic size and return a copy
        /// </summary>
        /// <param name="NewLength">length of normalization</param>
        /// <returns>Normalize Vector2D</returns>
        public Vector3D GetNormalizeToCopy(double NewLength)
        {
            Vector3D temp = new Vector3D(_x, _y, _z);
            temp.NormalizeTo(NewLength);
            return temp;
        }
        /// <summary>
        /// A Zero Vector2D with X = 0 and Y = 0
        /// </summary>
        public static Vector3D Zero
        {
            get { return new Vector3D(0, 0, 0); }
        }
        /// <summary>
        /// Scaling the vector
        /// </summary>
        /// <param name="P">The Vector</param>
        /// <param name="a">Scale size</param>
        /// <returns>sacaled vector</returns>
        public static Vector3D operator *(Vector3D P, double a)
        {
            return new Vector3D(P.X * a, P.Y * a, P.Z * a);
        }
        /// <summary>
        /// Scaling the vector
        /// </summary>
        /// <param name="P">The Vector</param>
        /// <param name="a">Scale size</param>
        /// <returns>sacaled vector</returns>
        public static Vector3D operator *(double a, Vector3D P)
        {
            return new Vector3D(P.X * a, P.Y * a, P.Z * a);
        }
        public static Vector3D operator *(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.Y * v2.Z - v2.Y * v1.Z, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        }
        /// <summary>
        /// Deviding the vector "scaling in 1/scale" 
        /// </summary>
        /// <param name="P">The Vector</param>
        /// <param name="a">Scale size</param>
        /// <returns>devided vector</returns>
        public static Vector3D operator /(Vector3D P, double a)
        {
            return new Vector3D(P._x / a, P._y / a, P._z / a);
        }
        /// <summary>
        /// Adding 2 vectors
        /// </summary>
        /// <param name="Left">1st vector</param>
        /// <param name="Right">2nd vector</param>
        /// <returns>Added vector</returns>
        public static Vector3D operator +(Vector3D Left, Vector3D Right)
        {
            return new Vector3D(Left._x + Right._x, Left._y + Right._y, Left._z + Right._z);
        }
        /// <summary>
        /// Subtracting 2 vectors
        /// </summary>
        /// <param name="Left">1st vector</param>
        /// <param name="Right">2nd vector</param>
        /// <returns>Subtracted vector</returns>
        public static Vector3D operator -(Vector3D Left, Vector3D Right)
        {
            return new Vector3D(Left._x - Right._x, Left._y - Right._y, Left._z - Right._z);
        }
        /// <summary>
        /// scaling by -1
        /// </summary>
        /// <param name="V"></param>
        /// <returns>-V</returns>
        public static Vector3D operator -(Vector3D V)
        {
            return new Vector3D(-V._x, -V._y, -V._z);
        }
        /// <summary>
        /// checking equality of 2 Vector2D
        /// </summary>
        /// <param name="Left">1st Vector</param>
        /// <param name="Right">2nd Vector</param>
        /// <returns>True if they are equal</returns>
        public static bool operator ==(Vector3D Left, Vector3D Right)
        {
            return Math.Abs(Left._x - Right._x) < Epsilon && Math.Abs(Left._y - Right._y) < Epsilon && Math.Abs(Left._z - Right._z) < Epsilon;
        }
        /// <summary>
        /// checking NOT equality of 2 Vector2D
        /// </summary>
        /// <param name="Left">1st Vector</param>
        /// <param name="Right">2nd Vector</param>
        /// <returns>True if they are NOT equal</returns>
        public static bool operator !=(Vector3D Left, Vector3D Right)
        {
            return ! (Left == Right);
        }
        /// <summary>
        /// Inner Product of 2 vectors : v1.x * v2.x + v1.y * v2.y 
        /// </summary>
        /// <param name="Left">1st Vector</param>
        /// <param name="Right">2nd Vector</param>
        /// <returns>the inner product value</returns>
        public static double InnerProduct(Vector3D Left, Vector3D Right)
        {
            return Left._x * Right._x + Left._y * Right._y + Left._z * Right._z;
        }
        /// <summary>
        /// Inner Product with another vector
        /// </summary>
        /// <param name="V">the target Vector</param>
        /// <returns>the inner product value</returns>
        public double InnerProduct(Vector3D V)
        {
            return _x * V._x + _y * V._y + _z * V._z;
        }
        
	}
}
