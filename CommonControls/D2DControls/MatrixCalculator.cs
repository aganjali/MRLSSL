using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
//using Microsoft.WindowsAPICodePack.DirectX;
//using Microsoft.WindowsAPICodePack.DirectX.Controls;
//using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
//using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using MatrixD = System.Drawing.Drawing2D.Matrix;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;
using SlimDX;

namespace MRL.SSL.CommonControls.Direct2D
{
    public class MatrixCalculator
    {
        public static Matrix3x2 CreateMatrix(PointF[] SourcePoints, RectangleF DestPoints)
        {
            MatrixD mtx = new MatrixD(DestPoints, SourcePoints);
            mtx.Invert();
            return Convert(mtx);
        }

        public static Matrix3x2 Convert(MatrixD mtx)
        {
            Matrix3x2 m = new Matrix3x2();
            m.M11 = mtx.Elements[0];
            m.M12 = mtx.Elements[1];
            m.M21 = mtx.Elements[2];
            m.M22 = mtx.Elements[3];
            m.M31 = mtx.Elements[4];
            m.M32 = mtx.Elements[5];
            return m;
        }

        public static MatrixD Convert(Matrix3x2 m)
        {
            MatrixD mtx = new MatrixD(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
            return mtx;
        }

        public static Matrix3x2 Scale(Matrix3x2 Current, float ScaleX, float ScaleY, MatrixOrder Order)
        {
            MatrixD mtx = Convert(Current);
            mtx.Scale(ScaleX, ScaleY, Order);
            return Convert(mtx);
            //return Matrix3x2.Multiply(Current, Matrix3x2.Scale(ScaleX, ScaleY));
        }


        public static Matrix3x2 Rotate(Position2D Center, Matrix3x2 Current, float angle,MatrixOrder Order)
        {
            
            MatrixD mtx = Convert(Current);
            mtx.RotateAt(angle,Center);

            return Convert(mtx);
        }
        internal static Matrix3x2 Translate(Matrix3x2 Current, float OffsetX, float OffsetY, MatrixOrder Order)
        {
            //return Matrix3x2.Multiply(Current, Matrix3x2.Translation(OffsetX, OffsetY, out Current);
            //SlimDX.Matrix c = new SlimDX.Matrix();
           
            MatrixD mtx = Convert(Current);
            mtx.Translate(OffsetX, OffsetY, Order);
            return Convert(mtx);
        }

        //internal static PointF Translate(Matrix3x2 Current, float X, float Y, MatrixOrder Order)
        //{
        //    Matrix mtx = Convert(Current);
        //    //mtx.Shear(
        //    //return Convert(mtx);
        //    return null;
        //}
    }
}
