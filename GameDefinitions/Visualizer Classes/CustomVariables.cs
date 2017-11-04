using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Xml;
using System.IO;

namespace MRL.SSL.GameDefinitions
{
    public static class CustomVariables
    {
        /// <summary>
        /// doubles that user want to change in visuaizer
        /// </summary>
        public static Dictionary<string, double> Doubles;
        /// <summary>
        /// Vector2ds that user want to change in visuaizer
        /// </summary>
        public static Dictionary<string, Vector2D> Vector2Ds;
        /// <summary>
        /// position2ds that user want to change in visuaizer
        /// </summary>
        public static Dictionary<string, Position2D> Position2Ds;
        /// <summary>
        /// integers that user want to change in visuaizer
        /// </summary>
        public static Dictionary<string, int> Integers;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public static void Add(string name, double val)
        {
            if (Doubles == null)
            {
                Doubles = new Dictionary<string, double>();
                Doubles.Add(name, val);
            }
            else if (!Doubles.ContainsKey(name))
                Doubles.Add(name, val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public static void Add(string name, Vector2D val)
        {
            if (Vector2Ds == null)
            {
                Vector2Ds = new Dictionary<string, Vector2D>();
                Vector2Ds.Add(name, val);
            }
            else if (!Vector2Ds.ContainsKey(name))
                Vector2Ds.Add(name, val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public static void Add(string name, Position2D val)
        {
            if (Position2Ds == null)
            {
                Position2Ds = new Dictionary<string, Position2D>();
                Position2Ds.Add(name, val);
            }
            else if (!Position2Ds.ContainsKey(name))
                Position2Ds.Add(name, val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public static void AddToint(string name, int val)
        {
            if (Integers == null)
            {
                Integers = new Dictionary<string, int>();
                Integers.Add(name, val);
            }
            else if (!Integers.ContainsKey(name))
                Integers.Add(name, val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object Read<T>(string name)
        {
            if (typeof(T) == typeof(double))
            {
                if (Doubles != null)
                    if (Doubles.ContainsKey(name))
                        return Doubles[name];
            }
            else if (typeof(T) == typeof(Vector2D))
            {
                if (Vector2Ds != null)
                    if (Vector2Ds.ContainsKey(name))
                        return Vector2Ds[name];
            }
            else if (typeof(T) == typeof(Position2D))
            {
                if (Position2Ds != null)
                    if (Position2Ds.ContainsKey(name))
                        return Position2Ds[name];
            }
            else if (typeof(T) == typeof(int))
            {
                if (Integers != null)
                    if (Integers.ContainsKey(name))
                        return Integers[name];
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        public static void Save(string FileName)
        {
            string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AISettings");
            string SaveFile = Path.Combine(SettingsFolder, FileName + ".cv");
            if (!File.Exists(SettingsFolder))
                System.IO.Directory.CreateDirectory(SettingsFolder);

            XmlTextWriter _XmlWriter = new XmlTextWriter(SaveFile, null);
            _XmlWriter.Formatting = Formatting.Indented;
            _XmlWriter.WriteStartElement("CustomVariable");
            #region Write Doubles
            _XmlWriter.WriteStartElement("Doubles");
            if (Doubles == null)
                Doubles = new Dictionary<string, double>();
            _XmlWriter.WriteStartElement("ItemsCount");
            _XmlWriter.WriteValue(Doubles.Count);
            _XmlWriter.WriteEndElement();
            foreach (string key in Doubles.Keys)
            {
                _XmlWriter.WriteStartElement("ItemName");
                _XmlWriter.WriteString(key);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("ItemValue");
                _XmlWriter.WriteValue(Doubles[key]);
                _XmlWriter.WriteEndElement();
            }

            _XmlWriter.WriteEndElement();
            #endregion

            #region Write Integers
            _XmlWriter.WriteStartElement("Integers");
            if (Integers == null)
                Integers = new Dictionary<string, int>();
            _XmlWriter.WriteStartElement("ItemsCount");
            _XmlWriter.WriteValue(Integers.Count);
            _XmlWriter.WriteEndElement();
            foreach (string key in Integers.Keys)
            {
                _XmlWriter.WriteStartElement("ItemName");
                _XmlWriter.WriteString(key);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("ItemValue");
                _XmlWriter.WriteValue(Integers[key]);
                _XmlWriter.WriteEndElement();
            }

            _XmlWriter.WriteEndElement();
            #endregion

            #region Write Position2Ds
            _XmlWriter.WriteStartElement("Position2Ds");
            if (Position2Ds == null)
                Position2Ds = new Dictionary<string, Position2D>();
            _XmlWriter.WriteStartElement("ItemsCount");
            _XmlWriter.WriteValue(Position2Ds.Count);
            _XmlWriter.WriteEndElement();

            foreach (string key in Position2Ds.Keys)
            {
                _XmlWriter.WriteStartElement("ItemName");
                _XmlWriter.WriteString(key);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("ItemValueX");
                _XmlWriter.WriteValue(Position2Ds[key].X);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("ItemValueY");
                _XmlWriter.WriteValue(Position2Ds[key].Y);
                _XmlWriter.WriteEndElement();
            }

            _XmlWriter.WriteEndElement();
            #endregion

            #region Write vectors
            _XmlWriter.WriteStartElement("Vectors");
            if (Vector2Ds == null)
                Vector2Ds = new Dictionary<string, Vector2D>();
            _XmlWriter.WriteStartElement("ItemsCount");
            _XmlWriter.WriteValue(Vector2Ds.Count);
            _XmlWriter.WriteEndElement();

            foreach (string key in Vector2Ds.Keys)
            {
                _XmlWriter.WriteStartElement("ItemName");
                _XmlWriter.WriteString(key);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("ItemValueX");
                _XmlWriter.WriteValue(Vector2Ds[key].X);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("ItemValueY");
                _XmlWriter.WriteValue(Vector2Ds[key].Y);
                _XmlWriter.WriteEndElement();
            }

            _XmlWriter.WriteEndElement();
            #endregion

            _XmlWriter.WriteEndElement();

            _XmlWriter.Flush();
            _XmlWriter.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static bool Load(string FileName)
        {
            Doubles = new Dictionary<string, double>();
            Integers = new Dictionary<string, int>();
            Position2Ds = new Dictionary<string, Position2D>();
            Vector2Ds = new Dictionary<string, Vector2D>();

            string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AISettings");
            string SavedFile = Path.Combine(SettingsFolder, FileName + ".cv");

            if (!File.Exists(SavedFile))
                return false;

            FileStream Stream = new FileStream(SavedFile, FileMode.Open, FileAccess.Read, FileShare.None);
            XmlTextReader _XmlReader = new XmlTextReader(Stream);
            int count = 0;
            #region Read Doubles
            _XmlReader.ReadToFollowing("Doubles");
            _XmlReader.ReadToFollowing("ItemsCount");
            count = _XmlReader.ReadElementContentAsInt();
            for (int i = 0; i < count; i++)
            {
                _XmlReader.ReadToFollowing("ItemName");
                string name = _XmlReader.ReadElementContentAsString();
                _XmlReader.ReadToFollowing("ItemValue");
                double val = _XmlReader.ReadElementContentAsDouble();
                Doubles.Add(name, val);
            }
            #endregion

            #region Raed Integers
            _XmlReader.ReadToFollowing("Integers");
            _XmlReader.ReadToFollowing("ItemsCount");
            count = _XmlReader.ReadElementContentAsInt();
            for (int i = 0; i < count; i++)
            {
                _XmlReader.ReadToFollowing("ItemName");
                string name = _XmlReader.ReadElementContentAsString();
                _XmlReader.ReadToFollowing("ItemValue");
                int val = _XmlReader.ReadElementContentAsInt();
                Integers.Add(name, val);
            }
            #endregion

            #region Raed Positions
            _XmlReader.ReadToFollowing("Position2Ds");
            _XmlReader.ReadToFollowing("ItemsCount");
            count = _XmlReader.ReadElementContentAsInt();
            for (int i = 0; i < count; i++)
            {
                _XmlReader.ReadToFollowing("ItemName");
                string name = _XmlReader.ReadElementContentAsString();
                Position2D val = new Position2D();
                _XmlReader.ReadToFollowing("ItemValueX");
                val.X = _XmlReader.ReadElementContentAsDouble();
                _XmlReader.ReadToFollowing("ItemValueY");
                val.Y = _XmlReader.ReadElementContentAsDouble();
                Position2Ds.Add(name, val);
            }
            #endregion

            #region Raed Vectors
            _XmlReader.ReadToFollowing("Vectors");
            _XmlReader.ReadToFollowing("ItemsCount");
            count = _XmlReader.ReadElementContentAsInt();
            for (int i = 0; i < count; i++)
            {
                _XmlReader.ReadToFollowing("ItemName");
                string name = _XmlReader.ReadElementContentAsString();
                Vector2D val = new Vector2D();
                _XmlReader.ReadToFollowing("ItemValueX");
                val.X = _XmlReader.ReadElementContentAsDouble();
                _XmlReader.ReadToFollowing("ItemValueY");
                val.Y = _XmlReader.ReadElementContentAsDouble();
                Vector2Ds.Add(name, val);
            }
            #endregion
            Stream.Close();
            _XmlReader.Close();
            return true;
        }
    }
}
