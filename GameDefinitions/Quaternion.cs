using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class Quater
    {
        const double Epsilon = 1e-5;
        double x;

        public double X
        {
            get { return x; }
            set { x = value; }
        }
        double y;

        public double Y
        {
            get { return y; }
            set { y = value; }
        }
        double z;

        public double Z
        {
            get { return z; }
            set { z = value; }
        }
        double w;

        public double W
        {
            get { return w; }
            set { w = value; }
        }
        public Quater(double _x, double _y, double _z, double _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }
        public Quater()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }
        public Quater(Quater q)
        {
            x = q.X;
            y = q.Y;
            z = q.Z;
            w = q.W;
        }


        public static Quater operator *(Quater q, Quater r)
        {
            return new Quater(
            r.X = r.W * q.X + r.X * q.W + r.Y * q.Z - r.Z * q.Y,
            r.Y = r.W * q.Y - r.X * q.Z + r.Y * q.W + r.Z * q.x,
            r.Z = r.W * q.Z + r.X * q.Y - r.Y * q.X + r.Z * q.W,
            r.W = r.W * q.W - r.X * q.X - r.Y * q.Y - r.Z * q.Z
            );
        }

        public static Quater operator *(Quater q, Vector3D v)
        {
            return new Quater(
                q.W * v.X + q.Y * v.Z - q.Z * v.Y,
                q.W * v.Y - q.X * v.Z + q.Z * v.X,
                q.W * v.Z + q.X * v.Y - q.Y * v.X,
              - q.X * v.X - q.Y * v.Y - q.Z * v.Z
                );
        }

        public static bool operator ==(Quater q,Quater r)
        {
            return q.X == r.X &&
                   q.Y == r.Y &&
                   q.Z == r.Z &&
                   q.W == r.W;
        }

        public static bool operator !=(Quater q, Quater r)
        {
            return !(q == r);
        }

        public override bool Equals(object obj)
        {
            return (this == (Quater) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Normalize()
        {
            double mag2 = (w * w )+ (x * x )+ (y * y) + (z * z);
            if (Math.Abs(mag2 - 1) > Epsilon)
            {
                double mag = Math.Sqrt(mag2);
                if (mag!=0)
                {
                    w /= mag;
                    x /= mag;
                    y /= mag;
                    z /= mag;
                }
            }
        }
        public void Blend(double d, Quater q)
        {
            double norm = x * q.X + y * q.Y + z * q.Z + w * q.W;
            bool bFlip = false;
            if (norm < 0)
            {
                norm = -norm;
                bFlip = true;
            }
            double inv_d;
            if (1 - norm < Epsilon)
            {
                inv_d = 1 - d;
            }
            else
            {
                double theta = (double)Math.Acos(norm);
                double s = (double)(1/Math.Sin(theta));
                inv_d = (double)Math.Sin((1 - d) * theta) * s;
                d = (double)Math.Sin(d * theta) * s;
            }
            if (bFlip)
            {
                d = -d;
            }
            x = inv_d * x + d * q.X;
            y = inv_d * y + d * q.Y;
            z = inv_d * z + d * q.Z;
            w = inv_d * w + d * q.W;
        }
        public void Clear()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 1;
        }
        public void Conjugate()
        {
            x = -x;
            y = -y;
            z = -z;
        }
        public void Invert()
        {
            Conjugate();
            double norm = x * x + y * y + z * z + w * w;
            if (norm!=0)
            {
                double inv_norm = 1 / norm;
                x *= inv_norm;
                y *= inv_norm;
                z *= inv_norm;
                w *= inv_norm;
            }
        }
        public void Set(double qx, double qy, double qz, double qw)
        {
            x = qx;
            y = qy;
            z = qz;
            w = qw;
        }
        public void SetAxis(Vector3D v, double angle)
        {
            if (angle != 0.0)
            {
                
                angle *= 0.5;
                double sinAngle = Math.Sin(angle);
                Vector3D vn = v;
                vn.Normalize();
                x = (vn.X * sinAngle);
                y = (vn.Y * sinAngle);
                z = (vn.Z * sinAngle);
                w = Math.Cos(angle);
            }
            else
            {
                Clear();
            }
        }
        public Vector3D GetZvector()
        {
            Vector3D v = new Vector3D(0, 0, 1);
            return RotateVectorByQuaternion(v);
        }
        public Vector3D RotateVectorByQuaternion(Vector3D v)
        {
            double x2 = x * x; 
            double y2 = y * y; 
            double z2 = z * z;
            double xy = x * y; 
            double xz = x * z; 
            double yz = y * z; 
            double wx = w * x;
            double wy = w * y;
            double wz = w * z;
            Vector3D res = new Vector3D();
            res.X = (1 - 2 * (y2 + z2)) * v.X + (2 * (xy - wz))     * v.Y + (2 * (xz + wy))     * v.Z;
            res.Y = (2 * (xy + wz))     * v.X + (1 - 2 * (x2 + z2)) * v.Y + (2 * (yz - wx))     * v.Z;
            res.Z = (2 * (xz - wy))     * v.X + (2 * (yz + wx))     * v.Y + (1 - 2 * (x2 + y2)) * v.Z;
            /*
            res[0, 0] = 1 - 2 * (y2 + z2); res[0, 1] = 2 * (xy - wz); res[0, 2] = 2 * (xz + wy);
            res[1, 0] = 2 * (xy + wz); res[1, 1] = 1 - 2 * (x2 + z2); res[1, 2] = 2 * (yz - xw);
            res[2, 0] = 2 * (xz - wy); res[2, 1] = 2 * (yz + xw); res[2, 2] = 1 - 2 * (x2 + y2);*/

            return res;
        }
        public MathMatrix GetMatrix()
        {
            double x2 = x * x;
            double y2 = y * y;
            double z2 = z * z;
            double xy = x * y;
            double xz = x * z;
            double yz = y * z;
            double wx = w * x;
            double wy = w * y;
            double wz = w * z;
            MathMatrix m = new MathMatrix(3,3);
            m[0, 0] = 1 - 2 * (y2 + z2);  m[0, 1] = (2 * (xy - wz));  m[0, 2] = 2 * (xz - wy);
            m[1,0] = 2 * (xy + wz);       m[1,1] = 1 - 2 * (x2 + z2); m[1,2] = 2 * (yz - wx);             
            m[2,0] = 2 * (xz - wy);       m[2,1] = 2 * (yz + wx);     m[2,2] = 1 - 2 * (x2 + y2);         
            return m;
        }
        public void GetAxisAngle(out Vector3D axis,out double angle)
        {
            double scale = Math.Sqrt(x * x + y * y + z * z);
            axis = new Vector3D(x / scale, y / scale ,z / scale);
            angle = Math.Acos(w) * 2;
        }
        public double GetAngle()
        {
            return Math.Acos(w) * 2;
        }
        public void SetEuler(double pitch, double yaw, double roll)
        {
            double c1 = Math.Cos(yaw / 2);
            double s1 = Math.Cos(yaw / 2);
            double c2 = Math.Cos(roll / 2);
            double s2 = Math.Cos(roll / 2);
            double c3 = Math.Cos(pitch / 2);
            double s3 = Math.Cos(pitch / 2);
            w = c1 * c2 * c3 - s1 * s2 * s3;
            x = c1 * c2 * s3 + s1 * s2 * c3;
            y = s1 * c2 * c3 + c1 * s2 * s3;
            z = c1 * s2 * c3 - s1 * c2 * s3;
            Normalize();
        }
        public double GetYaw()
        {
            if (Math.Abs((x * y + z * w) - 0.5) < Epsilon)
            {
                return 2 * Math.Atan2(x, w);
            }
            if (Math.Abs((x * y + z * w) + 0.5) < Epsilon)
            {
                return -2 * Math.Atan2(x, w);
            }
            return Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * (y * y) - 2 * (z * z));
        }
        public double GetPitch()
        {
            if (Math.Abs((x * y + z * w) - 0.5) < Epsilon)
            {
                return 0;
            }
            if (Math.Abs((x * y + z * w) + 0.5) < Epsilon)
            {
                return 0;
            }
            return Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * (x * x) - 2 * (z * z));
        }
        public double GetRoll()
        {
            return Math.Asin(2 * x * y + 2 * z * w);
        }
        public void GetEuler(out double pitch, out double yaw, out double roll)
        {
            pitch = GetPitch();
            yaw = GetYaw();
            roll = GetRoll();
        }
        public static Quater ShortestArc(Vector3D from, Vector3D to)
        {
            Vector3D cross = from * to;
            double dot = from.InnerProduct(to);
            dot = (double)Math.Sqrt(2 * (dot + 1));
            cross /= dot;
            return new Quater(cross.X, cross.Y, cross.Z, -dot / 2);
        }
    }
}
