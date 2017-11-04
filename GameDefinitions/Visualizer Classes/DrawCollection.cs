using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.CommonClasses.Extentions;
using System.Drawing;
using Enterprise;

namespace MRL.SSL.GameDefinitions
{
    public class DrawCollection
    {
        public Dictionary<string, object> drawingObject = new Dictionary<string, object>();

        public  bool AddObject(object obj, string Key)
        {
            try
            {
                if (obj.GetType() == typeof(StringDraw) || obj.GetType() == typeof(Line) || obj.GetType() == typeof(SingleObjectState) || obj.GetType() == typeof(Circle) || obj.GetType() == typeof(FlatRectangle) || obj.GetType() == typeof(DrawCollection)
                    || obj is DrawRegion || obj is Position2D)
                {
                    if (!drawingObject.ContainsKey(Key))
                    {
                        drawingObject.Add(Key, obj);
                        return true;
                    }
                    else if (drawingObject[Key].GetType() == obj.GetType())
                    {
                        drawingObject[Key] = obj;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return false;
            }
        }

        public  bool AddObject(string Key, object obj)
        {
            try
            {
                if (obj.GetType() == typeof(StringDraw) || obj.GetType() == typeof(Line) || obj.GetType() == typeof(SingleObjectState) || obj.GetType() == typeof(Circle) || obj.GetType() == typeof(FlatRectangle) || obj.GetType() == typeof(DrawCollection)
                    || obj is DrawRegion || obj is Position2D)
                {
                    if (!drawingObject.ContainsKey(Key))
                    {
                        drawingObject.Add(Key, obj);
                        return true;
                    }
                    else if (drawingObject[Key].GetType() == obj.GetType())
                    {
                        drawingObject[Key] = obj;
                        return true;
                    }
                }
                return false;
            }
            catch(Exception ex) 
            {
                Logger.Write(LogType.Exception, ex);
                return false; 
            }
        }

        public  bool AddObject(object obj)
        {
            try
            {
                string key = KeyGenerator(obj);
                if (key!= null && !drawingObject.ContainsKey(key))
                {
                    drawingObject.Add(key, obj);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return false;
            }
        }

        private string KeyGenerator(object obj)
        {
            try
            {
                if (obj.GetType() == typeof(Position2D))
                {
                    return "Token:" + obj.As<Position2D>().ToString();
                }
                if (obj.GetType() == typeof(Line))
                {
                    return "Line:" + obj.As<Line>().Head.ToString() + ";" + obj.As<Line>().Tail.ToString();
                }
                else if (obj.GetType() == typeof(Circle))
                {
                    return "Circle:(" + obj.As<Circle>().Center.X.ToString("f3") + "," + obj.As<Circle>().Center.Y.ToString("f3") + ")" + ";" + obj.As<Circle>().Radious.ToString("f3");
                }
                else if (obj.GetType() == typeof(FlatRectangle))
                {
                    return "Rectangle:" + obj.As<FlatRectangle>().TopLeft.ToString() + ";" + obj.As<FlatRectangle>().Size.ToString();
                }
                else if (obj.GetType() == typeof(StringDraw))
                {
                    return "Text:" + obj.As<StringDraw>().Key;
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
