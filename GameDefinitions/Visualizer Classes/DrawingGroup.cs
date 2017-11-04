using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.GameDefinitions
{
    public class DrawingGroup
    {
        /// <summary>
        /// regions that user want to show
        /// </summary>
        public Dictionary<string, List<Position2D>> FillRegions;
        /// <summary>
        /// groups in drawing group
        /// </summary>
        public Dictionary<string, DrawCollection> Groups;
        /// <summary>
        /// Charting Data For Charter
        /// </summary>
        public Dictionary<string, ChartObject> CharterData;
        /// <summary>
        /// tocken tath user want to permanently show 
        /// </summary>
        public Dictionary<string, Position2D> PermanentToken;
        /// <summary>
        /// tocken tath user want to momently show 
        /// </summary>
        public Dictionary<string, Position2D> MomentToken;
        /// <summary>
        /// width of region pixel
        /// </summary>
        public int RegionPixelWidth = 1;
        /// <summary>
        /// Path for PathPlanner to show
        /// </summary>
        public Dictionary<int, List<Position2D>> PathToDraw;
        /// <summary>
        /// Circles that users want to show for Permanent
        /// </summary>
        public Dictionary<string, Circle> PermanentCirclesToDraw;
        /// <summary>
        /// Circles that users want to Momently show 
        /// </summary>
        public Dictionary<string, Circle> MomentlyCirclesToDraw;
        /// <summary>
        /// Lines that users want to permanently show
        /// </summary>
        public Dictionary<string, Line> PermanentlyLineToDraw;
        /// <summary>
        /// Lines that users want to momently show
        /// </summary>
        public Dictionary<string, Line> MomentlyLineToDraw;
        /// <summary>
        /// Variables that users want to tune in AI,Declare in cunstructor
        /// </summary>
        public Dictionary<string, double?> CustomVariables;
        /// <summary>
        /// Rectangle that users want to show
        /// </summary>
        public Dictionary<string, RectangleF> PermanentRectangleToDraw;
        /// <summary>
        /// Rectangle that users want to show
        /// </summary>
        public Dictionary<string, RectangleF> MomentRectangleToDraw;
        /// <summary>
        /// Region that users want to show
        /// </summary>
        public List<Point> RegionToDraw;
        /// <summary>
        /// object that users want to show
        /// </summary>
        public Dictionary<string, SingleObjectState> ObjectToDraw;
        /// <summary>
        /// Text that users want to show (Permanently)
        /// </summary>
        public Dictionary<string, Position2D> PermanentTextToDraw;
        /// <summary>
        /// Text that users want to show (Momently)
        /// </summary>
        public Dictionary<string, Position2D> MomentlyTextToDraw;
        /// <summary>
        /// vector that users want to show (Permanently)
        /// </summary>
        public Dictionary<string, Vector2D> PermanentVector;
        /// <summary>
        /// Vector that users want to show (Momently)
        /// </summary>
        public Dictionary<string, Vector2D> MomentlyVector;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="key"></param>
        public void AddPermanently(Vector2D vec, string key)
        {
            if (PermanentVector == null)
            {
                PermanentVector = new Dictionary<string, Vector2D>();
                PermanentVector.Add(key, vec);
            }
            else if (!PermanentVector.ContainsKey(key))
                PermanentVector.Add(key, vec);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="key"></param>
        public void AddMomently(Vector2D vec, string key)
        {
            if (MomentlyVector == null)
            {
                MomentlyVector = new Dictionary<string, Vector2D>();
                MomentlyVector.Add(key, vec);
            }
            else if (!MomentlyVector.ContainsKey(key))
                MomentlyVector.Add(key, vec);
            else
                MomentlyVector[key] = vec;
        }
        /// <summary>
        /// Adding lines to the visualizer to drawing
        /// </summary>
        /// <param name="line">input line to draw , better to have start and end line</param>
        public void AddPermanently(Line line)
        {
            if (PermanentlyLineToDraw == null)
            {
                PermanentlyLineToDraw = new Dictionary<string, Line>();
                PermanentlyLineToDraw.Add(KeyGenerator(line), line);
            }
            else if (!PermanentlyLineToDraw.ContainsKey(KeyGenerator(line)))
                PermanentlyLineToDraw.Add(KeyGenerator(line), line);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="key"></param>
        public void AddMomently(Position2D Head, Vector2D Tail, Pen p, string key)
        {
            if (MomentlyLineToDraw == null)
            {
                MomentlyLineToDraw = new Dictionary<string, Line>();
                MomentlyLineToDraw.Add(key, new Line(Head, new Position2D(Tail.X, Tail.Y), p));
            }
            else if (!MomentlyLineToDraw.ContainsKey(key))
                MomentlyLineToDraw.Add(key, new Line(Head, new Position2D(Tail.X, Tail.Y), p));
            else
                MomentlyLineToDraw[key] = new Line(Head, new Position2D(Tail.X, Tail.Y), p);
        }
        /// <summary>
        /// Adding lines to the visualizer to drawing
        /// </summary>
        /// <param name="line">input line to draw , better to have start and end line</param>
        public void AddPermanently(Position2D Head, Vector2D Tail, Pen p)
        {
            if (PermanentlyLineToDraw == null)
            {
                PermanentlyLineToDraw = new Dictionary<string, Line>();
                PermanentlyLineToDraw.Add(KeyGenerator(new Line(Head, new Position2D(Tail.X, Tail.Y), p)), new Line(Head, new Position2D(Tail.X, Tail.Y), p));
            }
            else if (!PermanentlyLineToDraw.ContainsKey(KeyGenerator(new Line(Head, new Position2D(Tail.X, Tail.Y), p))))
                lock (PermanentlyLineToDraw)
                    PermanentlyLineToDraw.Add(KeyGenerator(new Line(Head, new Position2D(Tail.X, Tail.Y), p)), new Line(Head, new Position2D(Tail.X, Tail.Y), p));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="key"></param>
        public void AddMomently(Position2D pos, string key)
        {
            if (MomentToken == null)
            {
                MomentToken = new Dictionary<string, Position2D>();
                MomentToken.Add(key, pos);
            }
            else if (!MomentToken.ContainsKey(key))
                MomentToken.Add(key, pos);
            else
                MomentToken[key] = pos;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="key"></param>
        public void AddPermanently(Position2D pos, string key)
        {
            if (PermanentToken == null)
            {
                PermanentToken = new Dictionary<string, Position2D>();
                PermanentToken.Add(key, pos);
            }
            else if (!PermanentToken.ContainsKey(key))
                PermanentToken.Add(key, pos);
        }
        /// <summary>
        /// Adding region with user key to visualizer To Draw
        /// </summary>
        /// <param name="point"></param>
        public void AddPermanently(Point point)
        {
            if (RegionToDraw == null)
            {
                RegionToDraw = new List<Point>();
                RegionToDraw.Add(point);
            }
            else
                RegionToDraw.Add(point);
        }
        /// <summary>
        /// Adding lines whit user key to the visualizer to drawing
        /// </summary>
        /// <param name="line"></param>
        public void AddPermanently(Line line, string key)
        {
            if (PermanentlyLineToDraw == null)
            {
                PermanentlyLineToDraw = new Dictionary<string, Line>();
                PermanentlyLineToDraw.Add(key, line);
            }
            else if (!PermanentlyLineToDraw.ContainsKey(key))
                PermanentlyLineToDraw.Add(key, line);
            else
                PermanentlyLineToDraw[key] = line;
        }
        /// <summary>
        /// Removing Lines whit user key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key, Type type)
        {
            if (type.GetType() == typeof(Line))
            {
                if (PermanentlyLineToDraw != null)
                    if (PermanentlyLineToDraw.ContainsKey(key))
                        PermanentlyLineToDraw.Remove(key);
            }
            else if (type.GetType() == typeof(Circle))
            {
                if (PermanentCirclesToDraw != null)
                    if (PermanentCirclesToDraw.ContainsKey(key))
                        PermanentCirclesToDraw.Remove(key);
            }
            else if (type.GetType() == typeof(SingleObjectState))
            {
                if (ObjectToDraw != null)
                    if (ObjectToDraw.ContainsKey(key))
                        ObjectToDraw.Remove(key);
            }
        }
        /// <summary>
        /// Adding circles to the visualizer to drawing(permanent)
        /// </summary>
        /// <param name="circle">input circles to draw</param>
        public void AddPermanently(Circle circle)
        {
            if (PermanentCirclesToDraw == null)
            {
                PermanentCirclesToDraw = new Dictionary<string, Circle>();
                PermanentCirclesToDraw.Add(KeyGenerator(circle), circle);
            }
            else if (!PermanentCirclesToDraw.ContainsKey(KeyGenerator(circle)))
                PermanentCirclesToDraw.Add(KeyGenerator(circle), circle);
        }
        /// <summary>
        /// Adding circles whit user key to the visualizer to drawing
        /// </summary>
        /// <param name="circle"></param>
        public void AddMomently(Circle circle, string key)
        {
            if (MomentlyCirclesToDraw == null)
            {
                MomentlyCirclesToDraw = new Dictionary<string, Circle>();
                MomentlyCirclesToDraw.Add(key, circle);
            }
            else if (!MomentlyCirclesToDraw.ContainsKey(key))
                MomentlyCirclesToDraw.Add(key, circle);
            else
                MomentlyCirclesToDraw[key] = circle;
        }
        /// <summary>
        /// Adding Custom variables to visualizer for tuning 
        /// </summary>
        /// <param name="name">string name for ai variable</param>
        /// <param name="variable">Data in double format for visualizer to tune</param>
        public void AddPermanently(string name, double variable)
        {
            if (PermanentlyLineToDraw == null)
            {
                PermanentlyLineToDraw = new Dictionary<string, Line>();
            }
            CustomVariables.Add(name, variable);
        }
        /// <summary>
        /// Adding rectangle with user key to visualizer To Draw
        /// </summary>
        /// <param name="region">a region</param>
        public void AddPermanently(string key, RectangleF region, Color color, double opacity)
        {
            key = key + "@" + color.R.ToString() + "@" + color.G.ToString() + "@" + color.B.ToString() + "@" + opacity.ToString();
            if (PermanentRectangleToDraw == null)
            {
                PermanentRectangleToDraw = new Dictionary<string, RectangleF>();
                PermanentRectangleToDraw.Add(key, region);
            }
            else
            {
                if (!PermanentRectangleToDraw.ContainsKey(key))
                    PermanentRectangleToDraw.Add(key, region);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="region"></param>
        public void AddMomently(string key, RectangleF region, Color color, double opacity)
        {
            string newKey = key + "@" + color.R.ToString() + "@" + color.G.ToString() + "@" + color.B.ToString() + "@" + opacity.ToString();
            if (MomentRectangleToDraw == null)
            {
                MomentRectangleToDraw = new Dictionary<string, RectangleF>();
                MomentRectangleToDraw.Add(newKey, region);
            }
            else
            {
                if (!MomentRectangleToDraw.ContainsKey(newKey))
                    MomentRectangleToDraw.Add(newKey, region);
            }
        }
        /// <summary>
        /// Adding object to visualizer To Draw
        /// </summary>
        /// <param name="state">a SingleObjectState</param>
        public void AddPermanently(SingleObjectState state)
        {
            if (ObjectToDraw == null)
            {
                ObjectToDraw = new Dictionary<string, SingleObjectState>();
                ObjectToDraw.Add(KeyGenerator(state), state);
            }
            else if (!ObjectToDraw.ContainsKey(KeyGenerator(state)))
                ObjectToDraw.Add(KeyGenerator(state), state);
        }
        /// <summary>
        /// Adding object with user key to visualizer To Draw
        /// </summary>
        /// <param name="state"></param>
        /// <param name="key"></param>
        public void AddMomently(SingleObjectState state, string key)
        {
            if (ObjectToDraw == null)
            {
                ObjectToDraw = new Dictionary<string, SingleObjectState>();
                ObjectToDraw.Add(key, state);
            }
            else if (!ObjectToDraw.ContainsKey(KeyGenerator(state)))
                ObjectToDraw.Add(key, state);
            else
                ObjectToDraw[key] = state;
        }
        /// <summary>
        /// adding line with user key
        /// </summary>
        /// <param name="line"></param>
        /// <param name="key"></param>
        public void AddMomently(Line line, string key)
        {
            if (MomentlyLineToDraw == null)
            {
                MomentlyLineToDraw = new Dictionary<string, Line>();
                MomentlyLineToDraw.Add(key, line);
            }
            else if (!MomentlyLineToDraw.ContainsKey(key))
                MomentlyLineToDraw.Add(key, line);
            else
                MomentlyLineToDraw[key] = line;
        }
        /// <summary>
        /// adding region with user key , color and opacity
        /// </summary>
        /// <param name="region"></param>
        public void AddPermanently(string key, Color color, float opacity, List<Position2D> region)
        {
            key += "," + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + opacity.ToString();
            if (FillRegions == null)
            {
                FillRegions = new Dictionary<string, List<Position2D>>();
                FillRegions.Add(key, region);
            }
            else if (!FillRegions.ContainsKey(key))
                    FillRegions.Add(key, region);
        }
        /// <summary>
        /// Adding charter data with a key and double value
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="ChartCollection"></param>
        public void AddToChart(string key, double val)
        {
            ChartObject co = new ChartObject(key);
            co.HasX = false;
            co.GraphColor = Color.Black;
            co.ShowInChart = true;
            co.Log = false;
            co.CollectionName = key;
            if (CharterData == null)
            {
                CharterData = new Dictionary<string, ChartObject>();
                CharterData.Add(key, co);
            }
            else if (!CharterData.ContainsKey(key))
            {
                CharterData.Add(key, co);
                CharterData[key].ChartData.Add(new Position2D(0, val));
            }
            else
            {
                CharterData[key].ChartData.Add(new Position2D(0, val));
            }
        }
        /// <summary>
        /// Adding charter data with a key and double value
        /// </summary>
        /// <param name="key">a string for key</param>
        /// <param name="val">double for add to graph</param>
        /// <param name="color">color to show graph</param>
        /// <param name="ShowInChart">will show in charter</param>
        /// <param name="Log">take log</param>
        public void AddToChart(string key, double val, Color color, bool ShowInChart, bool Log)
        {
            ChartObject co = new ChartObject(key);
            co.HasX = false;
            co.GraphColor = color;
            co.ShowInChart = ShowInChart;
            co.Log = Log;
            co.CollectionName = key;
            if (CharterData == null)
            {
                CharterData = new Dictionary<string, ChartObject>();
                CharterData.Add(key, co);
            }
            else if (!CharterData.ContainsKey(key))
            {
                CharterData.Add(key, co);
                CharterData[key].ChartData.Add(new Position2D(0, val));
            }
            else
            {
                CharterData[key].ChartData.Add(new Position2D(0, val));
            }
        }
        /// <summary>
        /// Adding position2D to chart data 
        /// </summary>
        /// <param name="key">string for the key</param>
        /// <param name="val"></param>
        public void AddToChart(string key, Position2D val)
        {
            ChartObject co = new ChartObject(key);
            co.HasX = true;
            co.GraphColor = Color.Black;
            co.ShowInChart = true;
            co.Log = false;
            co.CollectionName = key;
            if (CharterData == null)
            {
                CharterData = new Dictionary<string, ChartObject>();
                CharterData.Add(key, co);
            }
            else if (!CharterData.ContainsKey(key))
            {
                CharterData.Add(key, co);
                CharterData[key].ChartData.Add(val);
            }
            else
            {
                CharterData[key].ChartData.Add(val);
            }
        }
        /// <summary>
        /// add data to charter object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="color"></param>
        /// <param name="Show In Chart"></param>
        /// <param name="Log"></param>
        public void AddToChart(string key, Position2D val, Color color, bool ShowInChart, bool Log)
        {
            ChartObject co = new ChartObject(key);
            co.HasX = true;
            co.GraphColor = color;
            co.ShowInChart = ShowInChart;
            co.Log = Log;
            co.CollectionName = key;
            if (CharterData == null)
            {
                CharterData = new Dictionary<string, ChartObject>();
                CharterData.Add(key, co);
            }
            else if (!CharterData.ContainsKey(key))
            {
                CharterData.Add(key, co);
                CharterData[key].ChartData.Add(val);
            }
            else
            {
                CharterData[key].ChartData.Add(val);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="chartobject"></param>
        public void Add(string key, ChartObject chartobject)
        {
            if (CharterData == null)
            {
                CharterData = new Dictionary<string, ChartObject>();
                CharterData.Add(key, chartobject);
            }
            else if (!CharterData.ContainsKey(key))
                CharterData.Add(key, chartobject);
        }
        /// <summary>
        /// Adding Text to visualizer To Draw (permanently)
        /// </summary>
        /// <param name="text">string</param>
        public void AddPermanently(string text, Color color, float opacity, Position2D pos)
        {
            text += "@" + color.R.ToString() + "@" + color.G.ToString() + "@" + color.B.ToString() + "@" + opacity.ToString();
            if (PermanentTextToDraw == null)
            {
                PermanentTextToDraw = new Dictionary<string, Position2D>();
                PermanentTextToDraw.Add(text, pos);
            }
            else if (!PermanentTextToDraw.ContainsKey(text))
                PermanentTextToDraw.Add(text, pos);
        }
        /// <summary>
        /// Adding Text to visualizer To Draw (Momently)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        public void AddMomently(string text, Color color, float opacity, Position2D pos)
        {
            text += "@" + color.R.ToString() + "@" + color.G.ToString() + "@" + color.B.ToString() + "@" + opacity.ToString();
            if (MomentlyTextToDraw == null)
            {
                MomentlyTextToDraw = new Dictionary<string, Position2D>();
                MomentlyTextToDraw.Add(text, pos);
            }
            else if (!MomentlyTextToDraw.ContainsKey(text))
                MomentlyTextToDraw.Add(text, pos);
            else
                MomentlyTextToDraw[text] = pos;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void Add(string key, DrawCollection val)
        {
            if (Groups == null)
            {
                Groups = new Dictionary<string, DrawCollection>();
                Groups.Add(key, val);
            }
            else if (!Groups.ContainsKey(key))
            {
                Groups.Add(key, val);
            }
            else
                Groups[key] = val;

        }
        /// <summary>
        /// Generating string name from line head and tail for generating a key that for a line to not draw twice
        /// </summary>
        /// <param name="l">Line that want generating its key</param>
        /// <returns>key in string</returns>
        private string KeyGenerator(Line l)
        {
            return l.Head.X.ToString() + l.Head.Y.ToString() + l.Tail.X.ToString() + l.Tail.Y.ToString();
        }
        /// <summary>
        /// Generating string name from circle center and radius for generating a key that for a circle to not draw twice
        /// </summary>
        /// <param name="circle"></param>
        /// <returns></returns>
        private string KeyGenerator(Circle circle)
        {
            return circle.Center.X.ToString() + circle.Center.Y.ToString() + circle.Radious.ToString();
        }
        /// <summary>
        /// Generating string name from region for generating a key that for a region to not draw twice
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private string KeyGenerator(byte[] region)
        {
            string key = "";
            for (int i = 0; i < region.Length / 3; i++)
                key += region[i].ToString();
            return key;
        }
        /// <summary>
        /// Generating string name from singleobjectstate for generating a key that for a object to not draw twice
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private string KeyGenerator(SingleObjectState state)
        {
            return state.Location.ToString() + state.Type.ToString();
        }
        /// <summary>
        /// Generating string name from RectangleF for generating a key that for a rectangle to not draw twice
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        private string KeyGenerator(RectangleF rec)
        {
            return rec.ToString();
        }
        /// <summary>
        /// Clear Permanent Lists
        /// </summary>
        public void ClearPermanentlyLists()
        {
            if (PermanentCirclesToDraw != null)
                PermanentCirclesToDraw.Clear();
            if (PermanentTextToDraw != null)
                PermanentTextToDraw.Clear();
            if (FillRegions != null)
                FillRegions.Clear();
            if (PermanentlyLineToDraw != null)
                PermanentlyLineToDraw.Clear();
            if (PermanentToken != null)
                PermanentToken.Clear();
            if (PermanentVector != null)
                PermanentVector.Clear();
            if (PermanentRectangleToDraw != null)
                PermanentRectangleToDraw.Clear();


        }
        /// <summary>
        /// Clear Moment Lists
        /// </summary>
        public void ClearMomentlyLists()
        {
            if (MomentlyCirclesToDraw != null)
                MomentlyCirclesToDraw.Clear();
            if (MomentlyTextToDraw != null)
                MomentlyTextToDraw.Clear();
            if (MomentlyLineToDraw != null)
                MomentlyLineToDraw.Clear();
        }

    }
}
