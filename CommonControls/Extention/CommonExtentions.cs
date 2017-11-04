using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
//using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using MRL.SSL.GameDefinitions;
using SlimDX;

namespace MRL.SSL.CommonControls.Extention
{
    public static class CommonExtentions
    {
        public static Position2D ToPosition(this Point Pixel, Matrix3x2? transform)
        {
            double X = Pixel.X;
            double Y = Pixel.Y;
            //if (ShowMode == FieldOrientation.Verticaly)
            //{
            //    X = (X - fieldVisualizer1.Transform.Value.M31) / fieldVisualizer1.Transform.Value.M21;
            //    Y = (Y - fieldVisualizer1.Transform.Value.M32) / fieldVisualizer1.Transform.Value.M12;
            //    return new Position2D(Y, X);
            //}
            //else
            //{
            X = (X - transform.Value.M31) / transform.Value.M11;
            Y = (Y - transform.Value.M32) / transform.Value.M22;
            return new Position2D(X, Y);
        }

        public static Position2D ToPosition(this PointF Pixel, Matrix3x2? transform)
        {
            float X = Pixel.X;
            float Y = Pixel.Y;
            //if (ShowMode == FieldOrientation.Verticaly)
            //{
            //    X = (X - fieldVisualizer1.Transform.Value.M31) / fieldVisualizer1.Transform.Value.M21;
            //    Y = (Y - fieldVisualizer1.Transform.Value.M32) / fieldVisualizer1.Transform.Value.M12;
            //    return new Position2D(Y, X);
            //}
            //else
            //{
            X = (X - transform.Value.M31) / transform.Value.M11;
            Y = (Y - transform.Value.M32) / transform.Value.M22;
            return new Position2D(X, Y);
        }

        //public static Point2F ToPoint2F(this Position2D pos)
        //{
        //    return new Point2F((float)pos.X, (float)pos.Y);
        //}

        //public static T As<T>(this object objectToCast)
        //{
        //    if (objectToCast == null)
        //        return default(T);
        //    else
        //    {
        //        return (T)objectToCast;
        //    }

        //}

        public static SlimDX.Direct2D.Ellipse GetEllipse(this SlimDX.Direct2D.Ellipse source, Position2D center, float xradius, float yradius)
        {
            return new SlimDX.Direct2D.Ellipse()
            {
                Center = center,
                RadiusX = xradius,
                RadiusY = yradius
            };
        }
    
    }
}
