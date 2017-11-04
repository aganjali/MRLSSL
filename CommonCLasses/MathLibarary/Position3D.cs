using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.CommonCLasses.MathLibarary
{
    public class Position3D
    {
        double _x, _y, _z;

        public Position3D()
        {
            _x = 0;
            _y = 0;
            _z = 0;
        }

        public Position3D(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public double X
        {
            get { return _x; }
            set { _x = value; }
        }
        public static Position3D operator *(double a, Position3D p)
        {
            return new Position3D(a * p._x, a * p._y, a * p._z);
        }
        public static Position3D operator *(Position3D p, double a)
        {
            return a * p;
        }
        public static Position3D operator /(Position3D p, double a)
        {
            return p * (1.0 / a);
        }
    }
}
