using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.Extentions;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.CommonClasses;

namespace MRL.SSL.GameDefinitions
{
    public class LoggerDrawingObject
    {
        public object Tag { get; set; }
        public string BallStatus { get; set; }
        public List<LogTreeViewModel> ObjectTree = new List<LogTreeViewModel>();
        /// <summary>
        /// All object that users want to show (each object must have a uniq key)
        /// </summary>
        public Dictionary<string, object> drawingObject = new Dictionary<string, object>();

        public bool AddObject(object obj, string Key)
        {
            if (obj.GetType() == typeof(StringDraw) || obj.GetType() == typeof(Line) || obj.GetType() == typeof(SingleObjectState) || obj.GetType() == typeof(Circle) || obj.GetType() == typeof(FlatRectangle) || obj.GetType() == typeof(DrawCollection) || obj is Position2D)
            {
                if (!drawingObject.ContainsKey(Key))
                {
                    drawingObject.Add(Key, obj);
                    return true;
                }
                else if (drawingObject[Key].GetType() == obj.GetType())
                {
                    //if (drawingObject[Key].GetType() == typeof(Line))
                    //    obj.As<Line>().PenIsChanged = false;
                    //else if (drawingObject[Key].GetType() == typeof(Circle))

                    //    obj.As<Circle>().PenIsChanged = false;

                    //else if (drawingObject[Key].GetType() == typeof(StringDraw))

                    //    obj.As<StringDraw>().ColorChange = false;

                    drawingObject[Key] = obj;
                    return true;
                }
            }
            return false;
        }

        public bool AddObject(string Key, object obj)
        {
            if (obj.GetType() == typeof(StringDraw) || obj.GetType() == typeof(Line) || obj.GetType() == typeof(Circle) || obj.GetType() == typeof(FlatRectangle) || obj.GetType() == typeof(DrawCollection) || obj.GetType() == typeof(SingleObjectState) || obj.GetType() == typeof(DrawRegion) || obj is Position2D)
            {
                if (!drawingObject.ContainsKey(Key))
                {
                    drawingObject.Add(Key, obj);
                    return true;
                }
                else if (drawingObject[Key].GetType() == obj.GetType())
                {
                    //if (drawingObject[Key].GetType() == typeof(Line))
                    //    obj.As<Line>().PenIsChanged = false;
                    //else if (drawingObject[Key].GetType() == typeof(Circle))

                    //    obj.As<Circle>().PenIsChanged = false;

                    //else if (drawingObject[Key].GetType()== typeof(StringDraw))

                    //    obj.As<StringDraw>().ColorChange = false;


                    drawingObject[Key] = obj;
                    return true;
                }
            }
            return false;
        }

        public bool AddObject(object obj)
        {
            if (KeyGenerator(obj) != null && !drawingObject.ContainsKey(KeyGenerator(obj)))
            {
                drawingObject.Add(KeyGenerator(obj), obj);
                return true;
            }

            return false;
        }

        private string KeyGenerator(object obj)
        {
            if (obj.GetType() == typeof(Line))
            {
                return "Line:" + obj.As<Line>().Head.ToString() + ";" + obj.As<Line>().Tail.ToString();
            }
            else if (obj.GetType() == typeof(Circle))
            {
                return "Circle:" + obj.As<Circle>().Center.ToString() + ";" + obj.As<Circle>().Radious.ToString();
            }
            else if (obj.GetType() == typeof(FlatRectangle))
            {
                return "Rectangle:" + obj.As<FlatRectangle>().TopLeft.ToString() + ";" + obj.As<FlatRectangle>().Size.ToString();
            }
            else if (obj.GetType() == typeof(StringDraw))
            {
                return obj.As<StringDraw>().Key;
            }
            else
                return null;
        }
    }
}
