using System;
using System.Windows;
using System.Reflection;
using System.Linq;
using System.Windows.Media;
//using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;\

using MRL.SSL.CommonClasses.MathLibrary;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using SlimDX.Direct2D;
using SlimDX;


namespace MRL.SSL.CommonClasses.Extentions
{
    public static class CommonExtensions
    {
        public static int ToInt(this string source)
        {
            int i;
            if (int.TryParse(source, out i))
                return i;
            return 0;
        }

        public static Dictionary<string, string> ToDict(this System.Collections.Specialized.StringDictionary source)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (string item in source.Keys)
                ret.Add(item, source[item.ToString()]);
            return ret;
        }

        public static System.Collections.Specialized.StringDictionary ToDict(this Dictionary<string, string> source)
        {
            System.Collections.Specialized.StringDictionary ret = new System.Collections.Specialized.StringDictionary();
            foreach (string item in source.Keys)
                ret.Add(item, source[item.ToString()]);
            return ret;
        }
        public static bool[] ToEnginSetting(this string source)
        {
            string[] sp = source.Split(new Char[] { '.' });
            List<bool> ret = new List<bool>();
            ret.Add(bool.Parse(sp[0]));
            ret.Add(bool.Parse(sp[1]));
            return ret.ToArray();
        }
        /// <summary>
        /// Returns a substitute value if the source is null and the source otherwise.
        /// </summary>
        /// <typeparam x:Name="T"></typeparam>
        /// <param x:Name="source">The object being tested</param>
        /// <param x:Name="substitue">A replacement for the null object</param>
        /// <returns>Substitute value if the source is null and the source otherwise</returns>
        /// <example>
        /// String str = MyString.IsNull("-");
        /// Button btn = NullButton.IsNull(new Button());
        /// </example>
        public static T IsNull<T>(this T source, T substitue) where T : class
        {
            if (source == null)
            {
                return substitue;
            }
            else
            {
                return source;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector2D ToVector2D(this Position2D source)
        {
            return new Vector2D(source.X, source.Y);
        }
        /// <summary>
        /// Determines if the instance is between the given values including the bounds. 
        /// </summary>
        /// <example>
        /// if( "hello".IsBetween("example1", "sample2") ) { ... }
        /// if( someInt.IsBetween(5,6) ){...}
        /// </example>
        public static bool IsBetween<T>(this IComparable<T> instance, T value1, T value2)
        {
            return instance.CompareTo(value1) >= 0 && instance.CompareTo(value2) <= 0;
        }

        //public static Point2F ToPoint2F(this Position2D pos)
        //{
        //    return new Point2F((float)pos.X, (float)pos.Y);
        //}

        public static T As<T>(this object objectToCast)
        {
            if (objectToCast == null)
                return default(T);
            else
                return (T)objectToCast;
        }

        //public static T Clone<T>(this T source) where T : new()
        //{
        //    T newT = new T();
        //    foreach (var p in typeof(T).GetProperties())
        //    {
        //        if (p.CanWrite)
        //        {
        //            p.SetValue(newT, p.GetValue(source, null), null);
        //        }
        //    }
        //    return newT;
        //}
     
     
        public static Type GetType(this object source)
        {
            return source.GetType();
        }

        public static Series Clone(this Series source)
        {
            Series ret = new Series(source.Name) { ChartArea = source.ChartArea, Color = source.Color };
            ret.ChartType = source.ChartType;
            ret.MarkerBorderColor = source.MarkerBorderColor;
            ret.MarkerBorderWidth = source.MarkerBorderWidth;
            ret.MarkerSize = source.MarkerSize;
            ret.MarkerStyle = source.MarkerStyle;
            ret.ToolTip = source.ToolTip;
            ret.MarkerColor = source.MarkerColor;
            foreach (var item in source.Points)
                ret.Points.AddY(item.YValues[0]);
            return ret;
        }

        public static Brush ToBrush(this System.Drawing.Color source, WindowRenderTarget renderTarget)
        {
            Brush b = new SolidColorBrush(renderTarget, new Color4( 1f,source.R / 255f, source.G / 255f, source.B / 255f));
            return b;
        }

        public static StrokeStyle ToStrockStyle(this System.Drawing.Pen source, Factory factory)
        {
            StrokeStyleProperties st = new StrokeStyleProperties()
            {
                StartCap = (CapStyle)((int)source.StartCap),
                EndCap = (CapStyle)((int)source.EndCap),
                DashCap = (CapStyle)((int)source.DashCap),
                LineJoin = (LineJoin)((int)source.LineJoin),
                MiterLimit = 1f,
                DashStyle = (DashStyle)((int)source.DashStyle),
                DashOffset = source.DashOffset
            };
            StrokeStyle strock = new StrokeStyle(factory, st);
            return strock;
        }

        public static void CopyTo<T1, T2>(this T1 source, T2 destObject) where T2 : T1
        {
            foreach (var p in typeof(T1).GetProperties())
            {
                if (p.CanWrite)
                {
                    p.SetValue(destObject, p.GetValue(source, null), null);
                }
            }
        }

        public static Line CopyWithOutPen(this Line source)
        {
            Line res = new Line();
            res.Head = source.Head;
            res.Tail = source.Tail;
            res.IsShown = source.IsShown;
            res.PenIsChanged = source.PenIsChanged;
            return res;
        }

        public static SerializableDictionary<string, bool> ToSerializable(this Dictionary<string, bool> value)
        {
            if (value == null)
                return null;
            SerializableDictionary<string, bool> ret = new SerializableDictionary<string, bool>();
            foreach (var item in value)
                ret.Add(item.Key, item.Value);
            return ret;
        }

        public static SerializableDictionary<string, double> ToSerializable(this Dictionary<string, double> value)
        {
            if (value == null)
                return null;
            SerializableDictionary<string, Double> ret = new SerializableDictionary<string, double>();
            foreach (var item in value)
                ret.Add(item.Key, item.Value);
            return ret;
        }

        public static SerializableDictionary<string, Position2D> ToSerializable(this Dictionary<string, Position2D> value)
        {
            if (value == null)
                return null;
            SerializableDictionary<string, Position2D> ret = new SerializableDictionary<string, Position2D>();
            foreach (var item in value)
                ret.Add(item.Key, item.Value);
            return ret;
        }

        public static SerializableDictionary<string, int> ToSerializable(this Dictionary<string, int> value)
        {
            if (value == null)
                return null;
            SerializableDictionary<string, int> ret = new SerializableDictionary<string, int>();
            foreach (var item in value)
                ret.Add(item.Key, item.Value);
            return ret;
        }

       

    }
}
