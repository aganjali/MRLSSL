using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Threading;

namespace MRL.SSL.GameDefinitions
{

    public class StrategyDrawingInfo
    {
        public StrategyDrawingInfo()
        {
        }

        public StrategyDrawingInfo(WorldModel model)
        {
            Model = model;
            Drawing = null;
        }

        public StrategyDrawingInfo(WorldModel model, DrawCollection collection)
        {
            Model = model;
            Drawing = collection;
        }
        public StrategyDrawingInfo(DrawCollection collection)
        {
            Model = null;
            Drawing = collection;
        }
        public WorldModel Model { get; set; }
        public DrawCollection Drawing { get; set; }
    }

    public class StrategyInfo
    {

        public StrategyInfo(string name, List<Vector2D> region, List<GameStatus> status, int attendance, double probability, bool enabled, string about)
        {
            Name = name;
            Enabled = enabled;
            Region = region;
            AttendanceSize = attendance;
            About = about;
            Probability = probability;
            Status = status;
            DrawingInfo = new List<StrategyDrawingInfo>();
        }

        public StrategyInfo(string name, int attendance, string about)
        {
            Name = name;
            Enabled = false;
            Region = new List<Vector2D>();
            AttendanceSize = attendance;
            About = about;
            Probability = 0.5;
            DrawingInfo = new List<StrategyDrawingInfo>();
        }

        public StrategyInfo()
        {

        }
        public List<StrategyDrawingInfo> DrawingInfo { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<Vector2D> Region { get; set; }
        public List<GameStatus> Status { get; set; }
        public int AttendanceSize { get; set; }
        public string About { get; set; }
        public double Probability { get; set; }
        public static Dictionary<string, StrategyInfo> Strategies = new Dictionary<string, StrategyInfo>();

        public static StrategyInfo Get(string name)
        {
            return Strategies[name];
        }

        public static void Set(string name, StrategyInfo value)
        {
            Strategies[name] = value;
        }

        public static void Add(StrategyInfo val)
        {
            if (!Strategies.ContainsKey(val.Name))
                Strategies[val.Name] = val;
        }

        public static void Save(string filename = "")
        {

            GoogleSerializer gserializer = new GoogleSerializer();
            string fileName = "strategies.str";
            if (filename != "")
                fileName = filename + ".str";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (FileStream f = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                gserializer.Serialize(Strategies);
                byte[] data = gserializer.stream.ToArray();
                f.Write(data, 0, data.Length);
            }
        }

        public static void Load(string filename = "")
        {
            GoogleSerializer gserializer = new GoogleSerializer();
            string fileName = "strategies.str";
            if (filename != "")
                fileName = filename + ".str";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            if (!File.Exists(fileName))
                return;
            using (FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                byte[] data = new byte[f.Length];
                f.Read(data, 0, (int)f.Length);
                MemoryStream memory = new MemoryStream(data);
                memory.Position = 0;
                Strategies = gserializer.DeserializeStrategies(memory);
            }
        }


    }
}
