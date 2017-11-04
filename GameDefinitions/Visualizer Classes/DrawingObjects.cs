using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.CommonClasses.Extentions;
using MRL.SSL.CommonClasses;
using Enterprise;
using MRL.SSL.CommonCLasses.MathLibarary;

namespace MRL.SSL.GameDefinitions
{
    public static class DrawingObjects
    {
        public static double RobotAngle = 0;
        public static double BallSpeed = 0;
        public static double TetaShoot = 0;
        public static double TetaPass = 0;

        public static class Strategy
        {
            public static string Name = "";
            public static List<string> states = new List<string>();
            public static string CurrentState = "";
            public static List<string> Transitions = new List<string>();
        }

        public static long CurrentPositionReaded = 0;
        public static string BallStatuse = "";

        public static List<TreeViewModel> ObjectTree = new List<TreeViewModel>();
        /// <summary>
        /// All object that users want to show (each object must have a uniq key)
        /// </summary>
        public static Dictionary<string, object> drawingObject = new Dictionary<string, object>();

        public static bool AddObject(object obj, string Key)
        {
            try
            {
                bool b = false;

                if (obj.GetType() == typeof(StringDraw) || obj.GetType() == typeof(Line) || obj.GetType() == typeof(SingleObjectState) || obj.GetType() == typeof(Circle)
                    || obj.GetType() == typeof(FlatRectangle) || obj.GetType() == typeof(DrawCollection) || obj is DrawRegion
                    || obj is DrawRegion3D || obj is Position2D || obj is Position3D)
                {
                    lock (drawingObject)
                    {
                        if (!drawingObject.ContainsKey(Key))
                        {
                            drawingObject.Add(Key, obj);
                            b = true;
                        }
                        else if (drawingObject.ContainsKey(Key) && drawingObject[Key].GetType() == obj.GetType())
                        {
                            drawingObject[Key] = obj;
                            b = true;
                        }
                    }
                }
                return b;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return false;
            }
        }
        /// <summary>
        /// Add an object to draw in visualizer 
        /// </summary>
        /// <param name="Key">key of object</param>
        /// <param name="obj">object to draw that can be Line,Circle,FlatRectangle,DrawRegion,StringDraw</param>
        /// <returns></returns>
        public static bool AddObject(string Key, object obj)
        {
            try
            {
                if (obj.GetType() == typeof(StringDraw) || obj.GetType() == typeof(Line) || obj.GetType() == typeof(Circle)
                    || obj.GetType() == typeof(FlatRectangle) || obj.GetType() == typeof(DrawCollection)
                    || obj.GetType() == typeof(SingleObjectState) || obj is DrawRegion || obj is DrawRegion3D
                    || obj is Position3D || obj is Position2D)
                {
                    bool b = false;
                    lock (drawingObject)
                    {
                        if (!drawingObject.ContainsKey(Key))
                        {
                            drawingObject.Add(Key, obj);
                            b = true;
                        }
                        else if (drawingObject[Key].GetType() == obj.GetType())
                        {
                            drawingObject[Key] = obj;
                            b = true;
                        }
                    }
                    return b;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return false;
            }
        }

        public static bool AddObject(object obj)
        {
            try
            {
                string key = KeyGenerator(obj);
                lock (drawingObject)
                {
                    if (key != null && !drawingObject.ContainsKey(key))
                    {
                        drawingObject.Add(KeyGenerator(obj), obj);
                     
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return false;
            }
        }

        private static string KeyGenerator(object obj)
        {
            try
            {
                if (obj.GetType() == typeof(Position2D))
                {
                    return "Token :" + obj.As<Position2D>().toString();
                }
                if (obj.GetType() == typeof(Line))
                {
                    return "Line:" + obj.As<Line>().Head.toString() + ";" + obj.As<Line>().Tail.toString();
                }
                else if (obj.GetType() == typeof(Circle))
                {
                    return "Circle:(" + obj.As<Circle>().Center.X.ToString("f3") + "," + obj.As<Circle>().Center.Y.ToString("f3") + ")" + ";" + obj.As<Circle>().Radious.ToString("f3");
                }
                else if (obj.GetType() == typeof(FlatRectangle))
                {
                    return "Rectangle:" + obj.As<FlatRectangle>().TopLeft.toString() + ";" + obj.As<FlatRectangle>().Size.ToString();
                }
                else if (obj.GetType() == typeof(StringDraw))
                {
                    return obj.As<StringDraw>().Key;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return null;
            }
        }
    }
}
