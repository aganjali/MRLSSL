using System.IO;
using MRL.SSL.CommonClasses.MathLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using MRL.SSL.CommonClasses.Communication;
using ProtoBuf;
using MRL.SSL.CommonClasses.Extentions;
using System.Windows.Forms.DataVisualization.Charting;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions.Visualizer_Classes;
using System.Threading;
using System.Threading.Tasks;
using Enterprise;
using MRL.SSL.CommonCLasses.MathLibarary;


namespace MRL.SSL.GameDefinitions
{
    public class GoogleSerializer
    {
        /// <summary>
        /// if lockuptable be in deserializing this flag be true
        /// </summary>
        private bool _waitLockupTable = false;
        /// <summary>
        /// if customVariable be in deserializing this flag be true
        /// </summary>
        private bool _waitCustom = false;
        /// <summary>
        /// if this flag be true ai dont send any data to visualizer
        /// </summary>
        private bool _disableVisualizerData = false;
        /// <summary>
        /// if input stream be a log this flag be true
        /// </summary>
        private bool _isLogDeserializing;
        /// <summary>
        /// if input stream be a log this flag be true
        /// </summary>
        public bool IsLogDeserializing
        {
            get
            {
                return _isLogDeserializing;
            }
            set
            {
                _isLogDeserializing = value;
            }
        }
        /// <summary>
        /// Main memory stram
        /// </summary>
        public MemoryStream stream;
        /// <summary>
        /// cunstruct with new straem
        /// </summary>
        public GoogleSerializer()
        {
            stream = new MemoryStream();
        }
        private int _vint;
        /// <summary>
        /// Integer variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"vint", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public int Vint
        {
            get
            {
                return _vint;
            }
            set
            {
                _vint = value;
            }
        }
        /// <summary>
        /// Serialize an integer value by google protocol and save to stream
        /// </summary>
        /// <param name="val">A integer value</param>
        public void Serialize(int val)
        {
            Vint = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<int>(stream, Vint, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize an int value from a stream
        /// </summary>
        /// <returns>Deserialized int</returns>
        public int DeserializeInt()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<int>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Serialize an array of integers
        /// </summary>
        /// <param name="val">an array of integers</param>
        public void Serialize(int[] val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Length);
                for (int i = 0 ; i < val.Length ; i++)
                    Serialize(val[i]);
            }
        }
        /// <summary>
        /// deserialize an array of integers 
        /// </summary>
        /// <returns>an array of integers</returns>
        public int[] DeserializeIntArray()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                int[] ret = new int[count];
                for (int i = 0 ; i < count ; i++)
                    ret[i] = DeserializeInt();
                return ret;
            }
            return null;
        }
        private int? _vnint;
        /// <summary>
        /// nullable integer variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"vnint", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(int))]
        public int? Vnint
        {
            get
            {
                return _vnint;
            }
            set
            {
                _vnint = value;
            }
        }
        /// <summary>
        /// Serialize a nullable int value by google protocol and save to stream
        /// </summary>
        /// <param name="val">A nullable integer value</param>
        private void Serialize(int? val)
        {
            Vnint = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<int?>(stream, Vnint, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a nullable int value from a stream
        /// </summary>
        /// <returns>Deserialized nullable int</returns>	
        private int? DeserializeNullableInt()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<int?>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private double _vdouble;
        /// <summary>
        /// Double variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name = @"vdouble", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public double Vdouble
        {
            get
            {
                return _vdouble;
            }
            set
            {
                _vdouble = value;
            }
        }
        /// <summary>
        /// Serialize a double value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a double value</param>
        private void Serialize(double val)
        {
            Vdouble = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<double>(stream, Vdouble, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="stval"></param>
        public void Serialize(double val, MemoryStream stval)
        {
            stream = stval;
            Vdouble = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<double>(stream, Vdouble, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a double value from a stream
        /// </summary>
        /// <returns>Deserialized double</returns>
        private double DeserializeDouble()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<double>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stval"></param>
        /// <returns></returns>
        public double DeserializeDouble(MemoryStream stval)
        {
            stream = stval;
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<double>(stream, ProtoBuf.PrefixStyle.Base128);
        }

        private double? _vndouble;
        /// <summary>
        /// nullable double variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name = @"vndouble", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public double? Vndouble
        {
            get
            {
                return _vndouble;
            }
            set
            {
                _vndouble = value;
            }
        }
        /// <summary>
        /// Serialize a nullable double value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a nullable double value</param>
        private void Serialize(double? val)
        {
            Vndouble = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<double?>(stream, Vndouble, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a nullable double value from a stream
        /// </summary>
        /// <returns>Deserialized nullabe double</returns>
        public double? DeserializeNullableDouble()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<double?>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private float _vfloat;
        /// <summary>
        /// float variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name = @"vfloat", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public float Vfloat
        {
            get
            {
                return _vfloat;
            }
            set
            {
                _vfloat = value;
            }
        }
        /// <summary>
        /// Serialize a float value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a float value</param>
        private void Serialize(float val)
        {
            Vfloat = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<float>(stream, Vfloat, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a float value from a stream
        /// </summary>
        /// <returns>Deserialized float</returns>
        float DeserializeFloat()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<float>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private float? _vnfloat;
        /// <summary>
        /// nullable float variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name = @"vnfloat", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(float))]
        public float? Vnfloat
        {
            get
            {
                return _vnfloat;
            }
            set
            {
                _vnfloat = value;
            }
        }
        /// <summary>
        /// Serialize a nullable float value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a nullable float value</param>
        private void Serialize(float? val)
        {
            Vnfloat = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<float?>(stream, Vnfloat, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a nullable float value from a stream
        /// </summary>
        /// <returns>Deserialized nullable float</returns>
        private float? DeserializeNullableFoat()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<float?>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Serialize an object of Position2D's Class by google protocol and save to stream
        /// </summary>
        /// <param name="val">an object of Position2D's Class</param>
        private void Serialize(Position2D val)
        {
            Serialize(val.X);
            Serialize(val.Y);
            Serialize(val.IsShown);
            Serialize(val.DrawColor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(Position3D val)
        {
            Serialize(val.X);
            Serialize(val.Y);
            Serialize(val.Z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Position3D DeserializePosition3D()
        {
            Position3D pos = new Position3D();
            pos.X = DeserializeDouble();
            pos.Y = DeserializeDouble();
            pos.Z = DeserializeDouble();
            return pos;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="stval"></param>
        public void Serialize(Position2D val, MemoryStream stval)
        {
            stream = stval;
            Serialize(val.X);
            Serialize(val.Y);
            Serialize(val.IsShown);
            Serialize(val.DrawColor);
        }
        /// <summary>
        /// Diserialize a Position2D from a stream
        /// </summary>
        /// <returns>Deserialized Position2D</returns>
        private Position2D DeserializePosition2D()
        {

            double x = DeserializeDouble();
            double y = DeserializeDouble();
            bool isshwon = DeserializeBool();
            Color drawpen = DeserializeColor();
            return new Position2D(x, y)
            {
                IsShown = isshwon,
                DrawColor = drawpen
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stval"></param>
        /// <returns></returns>
        public Position2D DeserializePosition2D(MemoryStream stval)
        {
            stream = stval;
            double x = DeserializeDouble();
            double y = DeserializeDouble();
            bool isshwon = DeserializeBool();
            Color drawpen = DeserializeColor();
            return new Position2D(x, y)
            {
                IsShown = isshwon,
                DrawColor = drawpen
            };
        }
        /// <summary>
        /// Serialize a list of Position2D by google protocol and save to stream
        /// </summary>
        /// <param name="val">a list of Position2D</param>
        private void Serialize(List<Position2D> listPosition)
        {
            Serialize(listPosition != null);
            if (listPosition != null)
            {
                Serialize(listPosition.Count);
                for (int i = 0 ; i < listPosition.Count ; i++)
                    Serialize(listPosition[i]);
            }
        }
        /// <summary>
        /// Diserialize a list of position2D from a stream
        /// </summary>
        /// <returns>Deserialized Position2D</returns>
        private List<Position2D> DeserializelistPosition2D()
        {
            if (DeserializeBool())
            {
                List<Position2D> lp = new List<Position2D>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    lp.Add(DeserializePosition2D());
                return lp;
            }
            return null;
        }
        /// <summary>
        /// Serialize a list of Position2D by google protocol and save to stream
        /// </summary>
        /// <param name="val">a list of Position2D</param>
        private void Serialize(List<Position3D> listPosition)
        {
            Serialize(listPosition != null);
            if (listPosition != null)
            {
                Serialize(listPosition.Count);
                for (int i = 0 ; i < listPosition.Count ; i++)
                    Serialize(listPosition[i]);
            }
        }
        /// <summary>
        /// Diserialize a list of position2D from a stream
        /// </summary>
        /// <returns>Deserialized Position2D</returns>
        private List<Position3D> DeserializelistPosition3D()
        {
            if (DeserializeBool())
            {
                List<Position3D> lp = new List<Position3D>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    lp.Add(DeserializePosition3D());
                return lp;
            }
            return null;
        }
        /// <summary>
        /// Serialize an object of Vector2D's Class by google protocol and save to stream
        /// </summary>
        /// <param name="val">an object of Vector2D's Class</param>
        private void Serialize(Vector2D val)
        {
            Serialize(val.X);
            Serialize(val.Y);
        }
        /// <summary>
        /// Diserialize a Vector2D from a stream
        /// </summary>
        /// <returns>Deserialized Vector</returns>
        private Vector2D DeserializeVector2D()
        {
            double x = DeserializeDouble();
            double y = DeserializeDouble();
            return new Vector2D(x, y);
        }
        private bool _vBool;
        /// <summary>
        /// bool variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(7, IsRequired = true, Name = @"vBool", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public bool vBool
        {
            get
            {
                return _vBool;
            }
            set
            {
                _vBool = value;
            }
        }
        /// <summary>
        /// Serialize a bool value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a bool value</param>
        public void Serialize(bool val)
        {
            vBool = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<bool>(stream, vBool, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a bool value from a stream
        /// </summary>
        /// <returns>Deserialized bool</returns>
        public bool DeserializeBool()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<bool>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private bool? _vnBool;
        /// <summary>
        /// nullable bool variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name = @"vnBool", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(bool))]
        public bool? VnBool
        {
            get
            {
                return _vnBool;
            }
            set
            {
                _vnBool = value;
            }
        }
        /// <summary>
        /// Serialize a nullable bool value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a nullable bool value</param>
        void Serialize(bool? val)
        {
            VnBool = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<bool?>(stream, VnBool, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a nullable bool value from a stream
        /// </summary>
        /// <returns>Deserialized nullable bool</returns>
        bool? DeserializeNullableBool()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<bool?>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private string _vstring;
        /// <summary>
        /// string variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(9, IsRequired = true, Name = @"vString", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public string vString
        {
            get
            {
                return _vstring;
            }
            set
            {
                _vstring = value;
            }
        }
        /// <summary>
        /// Serialize a string value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a string value</param>
        public void Serialize(string val)
        {
            vString = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<string>(stream, vString, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a string value from a stream
        /// </summary>
        /// <returns>Deserialized string</returns>
        string DeserializeString()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<string>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private short _vshort;
        /// <summary>
        /// short variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(10, IsRequired = true, Name = @"vShort", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public short vShort
        {
            get
            {
                return _vshort;
            }
            set
            {
                _vshort = value;
            }
        }
        /// <summary>
        /// Serialize a short value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a short value</param>
        void Serialize(short val)
        {
            vShort = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<short>(stream, vShort, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a short value from a stream
        /// </summary>
        /// <returns>Deserialized short</returns>
        short DeserializeShort()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<short>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private short? _vnshort;
        /// <summary>
        /// nullable short variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(11, IsRequired = true, Name = @"vnShort", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public short? vnShort
        {
            get
            {
                return _vnshort;
            }
            set
            {
                _vnshort = value;
            }
        }
        /// <summary>
        /// serialize a nullable short value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a nullable short value</param>
        void Serialize(short? val)
        {
            vnShort = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<short>(stream, vShort, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a nullable Short value from a stream
        /// </summary>
        /// <returns>Deserialized nullable short</returns>
        short? DeserializeNullableShort()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<short?>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private byte _vbyte;
        /// <summary>
        /// byte variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(12, IsRequired = true, Name = @"vByte", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public byte vByte
        {
            get
            {
                return _vbyte;
            }
            set
            {
                _vbyte = value;
            }
        }
        /// <summary>
        /// Serialize a byte value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a byte value</param>
        void Serialize(byte val)
        {
            vByte = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<byte>(stream, vByte, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a byte value from a stream
        /// </summary>
        /// <returns>Deserialized byte</returns>
        byte DeserializeByte()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<byte>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        private byte? _vnbyte;
        /// <summary>
        /// nullable byte variable defined by google protocol
        /// </summary>
        [global::ProtoBuf.ProtoMember(13, IsRequired = true, Name = @"vnByte", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public byte? vnByte
        {
            get
            {
                return _vnbyte;
            }
            set
            {
                _vnbyte = value;
            }
        }
        /// <summary>
        /// Serialize a nullable value by google protocol and save to stream
        /// </summary>
        /// <param name="val">a nullable byte value</param>
        void Serialize(byte? val)
        {
            vnByte = val;
            ProtoBuf.Serializer.SerializeWithLengthPrefix<byte?>(stream, vnByte, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Diserialize a nullable byte value from a stream
        /// </summary>
        /// <returns>Deserialized nullable byte</returns>
        byte? DeserializeNullableByte()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<byte?>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        /// <summary>
        /// Serialize point
        /// </summary>
        /// <param name="val">a Point</param>
        void Serialize(Point val)
        {
            Serialize(val.X);
            Serialize(val.Y);
        }
        /// <summary>
        /// Deserialize point 
        /// </summary>
        /// <returns>Deserialized point</returns>
        Point DeserializePoint()
        {
            return new Point(DeserializeInt(), DeserializeInt());
        }
        /// <summary>
        /// Seialize List Point
        /// </summary>
        /// <param name="val">List point</param>
        void Serialize(List<Point> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                for (int i = 0 ; i < val.Count ; i++)
                {
                    Serialize(val[i].X);
                    Serialize(val[i].Y);
                }
            }
        }
        /// <summary>
        /// Deserialize list point
        /// </summary>
        /// <returns>Deserialized list point</returns>
        List<Point> DeserializeListPoint()
        {
            if (DeserializeBool())
            {
                List<Point> ret = new List<Point>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializePoint());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a RectangleF
        /// </summary>
        /// <param name="val">a RectangleF</param>
        void Serialize(RectangleF val)
        {
            Serialize(val.X);
            Serialize(val.Y);
            Serialize(val.Width);
            Serialize(val.Height);
        }
        /// <summary>
        /// Deserialize a RectangleF
        /// </summary>
        /// <returns>Deserialize RectangleF</returns>
        RectangleF DeserializeRectangleF()
        {
            return new RectangleF(DeserializeFloat(), DeserializeFloat(), DeserializeFloat(), DeserializeFloat());
        }
        /// <summary>
        /// serialize dictionary string RectangleF
        /// </summary>
        /// <param name="val">dictionary string RectangleF</param>
        void Serialize(Dictionary<string, RectangleF> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (string key in val.Keys)
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// deserialize
        /// </summary>
        /// <returns></returns>
        Dictionary<string, RectangleF> DeserializeDictionaryStringRectangleF()
        {
            if (DeserializeBool())
            {
                Dictionary<string, RectangleF> ret = new Dictionary<string, RectangleF>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeRectangleF());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize an object of Pen's Class by google protocol and save to stream
        /// </summary>
        /// <param name="val">an object of Pen's Class</param>
        void Serialize(Pen val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Color.R);
                Serialize(val.Color.G);
                Serialize(val.Color.B);
                Serialize(val.Width);
                Serialize((int)val.StartCap);
                Serialize((int)val.EndCap);
                Serialize((int)val.DashStyle);
            }
        }
        /// <summary>
        /// Diserialize an object of Pen from a stream
        /// </summary>
        /// <returns>Deserialized Pen</returns>
        Pen DeserializePen()
        {
            if (DeserializeBool())
            {
                byte r = DeserializeByte();
                byte g = DeserializeByte();
                byte b = DeserializeByte();
                float width = DeserializeFloat();
                Pen pen = new Pen(Color.FromArgb(r, g, b))
                {
                    Color = Color.FromArgb(r, g, b),
                    Width = width,
                    StartCap = (System.Drawing.Drawing2D.LineCap)DeserializeInt(),
                    EndCap = (System.Drawing.Drawing2D.LineCap)DeserializeInt(),
                    DashStyle = (System.Drawing.Drawing2D.DashStyle)DeserializeInt(),
                };
                return pen;
            }
            return Pens.Black;
        }
        /// <summary>
        /// Serialize an object of Circle's Class by google protocol and save to stream
        /// </summary>
        /// <param name="val">an object of Circle's Class</param>
        void Serialize(Circle val)
        {
            Serialize(val.Center);
            Serialize(val.Radious);
            Serialize(val.DrawPen);
            Serialize(val.IsShown);
            Serialize(val.IsFill);
            Serialize(val.Opacity);
            Serialize(val.PenIsChanged);
        }
        /// <summary>
        /// Diserialize an object of circle value from a stream
        /// </summary>
        /// <returns>Deserialized circle</returns>
        Circle DeserializeCircle()
        {
            Circle c = new Circle();
            c.Center = DeserializePosition2D();
            c.Radious = DeserializeDouble();
            c.DrawPen = DeserializePen();
            c.IsShown = DeserializeBool();
            c.IsFill = DeserializeBool();
            c.Opacity = DeserializeFloat();
            c.PenIsChanged = DeserializeBool();
            return c;
        }
        /// <summary>
        /// Serialize a List of Cirlce's Class by google protocol and save to stream
        /// </summary>
        /// <param name="val">a list of Circle's Class</param>
        public void Serialize(List<Circle> lc)
        {
            Serialize(lc != null);
            if (lc != null)
            {
                Serialize(lc.Count);
                for (int i = 0 ; i < lc.Count ; i++)
                    Serialize(lc[i]);
            }
        }
        /// <summary>
        /// Diserialize a List of Circle from a stream
        /// </summary>
        /// <returns>Deserialized List of circle</returns>
        public List<Circle> DeserializeCircleList()
        {
            List<Circle> lc = new List<Circle>();
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    lc.Add(DeserializeCircle());
                return lc;
            }
            return null;
        }
        /// <summary>
        /// Serialize a Dictionary of string,Cirlce  by google protocol and save to stream
        /// </summary>
        /// <param name="val">a Dictionary of String,Circle's</param>
        void Serialize(Dictionary<string, Circle> dic)
        {
            Serialize(dic != null);
            if (dic != null)
            {
                Serialize(dic.Count);
                foreach (string key in dic.Keys)
                {
                    Serialize(key);
                    Serialize(dic[key]);
                }
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of string,Cirlce from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of string,circle</returns>
        Dictionary<string, Circle> DeserializeDictionarySrtingCircle()
        {
            Dictionary<string, Circle> ret = new Dictionary<string, Circle>();
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeCircle());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize an object of Line's Class by google protocol and save to stream
        /// </summary>
        /// <param name="val">an object of Line's Class</param>
        void Serialize(Line line)
        {
            Serialize(line.Head);
            Serialize(line.Tail);
            Serialize(line.DrawPen);
            Serialize(line.IsShown);
            Serialize(line.PenIsChanged);
        }
        /// <summary>
        /// Diserialize a Line from a stream
        /// </summary>
        /// <returns>Deserialized Line</returns>
        Line DeserializeLine()
        {
            Line line = new Line();
            line.Head = DeserializePosition2D();
            line.Tail = DeserializePosition2D();
            line.DrawPen = DeserializePen();
            line.IsShown = DeserializeBool();
            line.PenIsChanged = DeserializeBool();
            return line;

        }
        /// <summary>
        /// Serialize a List of Line by google protocol and save to stream
        /// </summary>
        /// <param name="val">a List of Line</param>
        void Serialize(List<Line> listLine)
        {
            Serialize(listLine != null);
            if (listLine != null)
            {
                Serialize(listLine.Count);
                foreach (Line l in listLine)
                    Serialize(l);
            }
        }
        /// <summary>
        /// Diserialize a List of Line from a stream
        /// </summary>
        /// <returns>Deserialized List of Line</returns>
        List<Line> DeserializelistLine()
        {
            if (DeserializeBool())
            {
                List<Line> lineList = new List<Line>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    lineList.Add(DeserializeLine());
                return lineList;
            }
            return null;
        }
        /// <summary>
        /// Serialize a Dictionary of String,Line by google protocol and save to stream
        /// </summary>
        /// <param name="val">Dictionary of String,Line</param>
        void Serialize(Dictionary<string, Line> dic)
        {
            Serialize(dic != null);
            if (dic != null)
            {
                Serialize(dic.Count);
                foreach (string key in dic.Keys)
                {
                    Serialize(key);
                    Serialize(dic[key]);
                }
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of String,Line from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of String,Line</returns>
        Dictionary<string, Line> DeserializeDictionaryStringLine()
        {
            Dictionary<string, Line> ret = new Dictionary<string, Line>();
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeLine());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a dictionary double,double  by google protocol and save to stream
        /// </summary>
        /// <param name="val">a dictionary double,double</param>
        private void Serialize(Dictionary<double, double> dict)
        {
            Serialize(dict != null);
            if (dict != null)
            {
                Serialize(dict.Count);
                foreach (double key in dict.Keys)
                {
                    Serialize(key);
                    Serialize(dict[key]);
                }
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of doubel,double from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of doubel,double</returns>
        private Dictionary<double, double> DeSerializeDictionaryDoubleDouble()
        {
            if (!DeserializeBool())
                return null;
            int count = DeserializeInt();
            Dictionary<double, double> dict = new Dictionary<double, double>(count);
            for (int i = 0 ; i < count ; i++)
            {
                double key = DeserializeDouble();
                double value = DeserializeDouble();
                dict.Add(key, value);
            }
            return dict;
        }
        /// <summary>
        /// Serialize a dictionary of int Position2D
        /// </summary>
        /// <param name="val">dictionary of int Position2D</param>
        void Serialize(Dictionary<int, Position2D> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Dictionary<int, Position2D> copy = new Dictionary<int, Position2D>();
                lock (val)
                {
                    foreach (var p in val.Keys.ToList())
                    {
                        if (val[p] != null)
                            copy[p] = val[p];
                    }

                }

                Serialize(copy.Count);
                foreach (int key in copy.Keys.ToList())
                {
                    Serialize(key);
                    Serialize(copy[key]);
                }
            }
        }
        /// <summary>
        /// Deserialize a dictionary of int position2D
        /// </summary>
        /// <returns>Deserialized dictionary of int position2D</returns>
        Dictionary<int, Position2D> DeserializeDictionaryIntPosiotion2D()
        {
            if (DeserializeBool())
            {
                Dictionary<int, Position2D> ret = new Dictionary<int, Position2D>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeInt(), DeserializePosition2D());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a dictionary int,SingleObjectState by google protocol and save to stream
        /// </summary>
        /// <param name="val">a dictionary int,SingleObjectState</param>
        void Serialize(Dictionary<int, SingleObjectState> states)
        {
            Serialize(states != null);
            if (states != null)
            {
                Serialize(states.Count);
                int counter = 0;
                foreach (int key in states.Keys.ToList())
                {
                    counter++;
                    Serialize(key);
                    Serialize(states[key]);
                }
                if (counter != states.Count)
                    throw new Exception("This should never happen");
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of int,SingleObjectState from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of int,SingleObjectState</returns>
        Dictionary<int, SingleObjectState> DeserializeDictionaryIntState()
        {
            if (!DeserializeBool())
                return null;
            Dictionary<int, SingleObjectState> ret = new Dictionary<int, SingleObjectState>();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
            {
                int key = DeserializeInt();
                SingleObjectState state = DeserializeSingleObjectState();
                if (!ret.ContainsKey(key))
                    ret.Add(key, state);
            }
            return ret;
        }
        /// <summary>
        /// serizlize a dictionary of string and SingleObject State
        /// </summary>
        /// <param name="state">a dictionary of string and single Object State</param>
        void Serialize(Dictionary<string, SingleObjectState> state)
        {
            Serialize(state != null);
            if (state != null)
            {
                Serialize(state.Count);
                foreach (string key in state.Keys)
                {
                    Serialize(key);
                    Serialize(state[key]);
                }
            }
        }
        /// <summary>
        /// deserialize an dictionary of string and singleObjectSate
        /// </summary>
        /// <returns>a disctionary of string and singleobjectstate</returns>
        Dictionary<string, SingleObjectState> DeserializeDictionaryStringSingleObjectState()
        {
            if (DeserializeBool())
            {
                Dictionary<string, SingleObjectState> ret = new Dictionary<string, SingleObjectState>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeSingleObjectState());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a Dictionary(int,List(Position2D)) by google protocol and save to stream
        /// </summary>
        /// <param name="val">a Dictionary(int,List(Position2D))</param>
        void Serialize(Dictionary<int, List<Position2D>> paths)
        {
            Serialize(paths != null);
            if (paths != null)
            {
                Serialize(paths.Count);
                int counter = 0;
                foreach (int key in paths.Keys)
                {
                    counter++;
                    Serialize(key);
                    Serialize(paths[key]);
                }
                if (counter != paths.Count)
                    throw new Exception("This should never happen");
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of int,list(Position2D) from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of int,list(Position2D)</returns>
        Dictionary<int, List<Position2D>> DeserializeDictionaryIntListPosition2D()
        {
            if (!DeserializeBool())
                return null;
            Dictionary<int, List<Position2D>> ret = new Dictionary<int, List<Position2D>>();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
            {
                int key = DeserializeInt();
                List<Position2D> positions = DeserializelistPosition2D();
                ret.Add(key, positions);
            }
            return ret;
        }

        /// <summary>
        /// Serialize a Dictionary(string,Double) by google protocol and save to stream
        /// </summary>
        /// <param name="val">a Dictionary(string,Double)</param>
        void Serialize(Dictionary<string, double> dict)
        {
            Serialize(dict != null);
            if (dict != null)
            {
                lock (dict)
                {
                    int count = dict.Count;
                    Dictionary<string, double> temp = new Dictionary<string, double>();
                    foreach (string key in dict.Keys)
                        if (key != null)
                            temp.Add(key, dict[key]);
                    Serialize(temp.Count);
                    foreach (string key in temp.Keys)
                    {
                        Serialize(key);
                        Serialize(temp[key]);
                    }
                }
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of string,double from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of string,double</returns>
        Dictionary<string, double> DeserializeDictionaryStringDouble()
        {
            if (!DeserializeBool())
                return null;
            Dictionary<string, double> ret = new Dictionary<string, double>();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
            {
                string key = DeserializeString();
                double var = DeserializeDouble();
                ret.Add(key, var);
            }
            return ret;
        }
        /// <summary>
        /// Serialize a Dictionary string, Position2D
        /// </summary>
        /// <param name="val">a Dictionary of string, Position2D</param>
        void Serialize(Dictionary<string, Position2D> val)
        {

            Serialize(val != null);
            if (val != null)
            {
                Dictionary<string, Position2D> copy = new Dictionary<string, Position2D>();
                val.ToList().ForEach(p => copy.Add(p.Key, p.Value));
                Serialize(copy.Count);
                foreach (string key in copy.Keys)
                {
                    Serialize(key);
                    Serialize(copy[key]);
                }
            }

        }
        /// <summary>
        /// serizlize a dictionary string and list of position2ds
        /// </summary>
        /// <param name="val">a dictionary string and list of position2ds</param>
        void Serialize(Dictionary<string, List<Position2D>> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (string key in val.Keys)
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// Deserialize a Dictionary string, Position2D
        /// </summary>
        /// <returns>Deserialized a Dictionary string, Position2D</returns>
        Dictionary<string, Position2D> DeserializeDictionaryStringPosition2D()
        {
            if (DeserializeBool())
            {
                Dictionary<string, Position2D> ret = new Dictionary<string, Position2D>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializePosition2D());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// deserialize a dictionary string and list of positiopn2ds
        /// </summary>
        /// <returns>a dictionary string and list of position2ds</returns>
        Dictionary<string, List<Position2D>> DeserializeDictionaryStringListPosition2D()
        {
            if (DeserializeBool())
            {
                Dictionary<string, List<Position2D>> ret = new Dictionary<string, List<Position2D>>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializelistPosition2D());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a Dictionary(string,bool) by google protocol and save to stream
        /// </summary>
        /// <param name="val">a Dictionary(string,bool)</param>
        void Serialize(Dictionary<string, bool> dic)
        {
            Serialize(dic != null);
            if (dic != null)
            {
                Serialize(dic.Count);
                foreach (string key in dic.Keys)
                {
                    Serialize(key);
                    Serialize(dic[key]);
                }
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of string,bool from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of string,bool</returns>
        Dictionary<string, bool> DeserializeDictionaryStringbool()
        {
            if (DeserializeBool())
            {
                Dictionary<string, bool> ret = new Dictionary<string, bool>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeBool());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Seialize a byte[] value
        /// </summary>
        /// <param name="val"></param>
        void Serialize(byte[] val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Length);
                foreach (byte b in val)
                    Serialize(b);
            }
        }
        /// <summary>
        /// Deserialize a byte[] value
        /// </summary>
        /// <returns>Deseriliazed byte[]</returns>
        byte[] DeserializeArrayByte()
        {
            if (DeserializeBool())
            {
                int length = DeserializeInt();
                byte[] ret = new byte[length];
                for (int i = 0 ; i < length ; i++)
                    ret[i] = DeserializeByte();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize Dictionary string,byte[]
        /// </summary>
        /// <param name="val">Dictionary string,byte[]</param>
        void Serialize(Dictionary<string, byte[]> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (string key in val.Keys)
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// Deserialize a Dictionary string,byte[]
        /// </summary>
        /// <returns>Deserialized Dictionary string,byte[]</returns>
        Dictionary<string, byte[]> DeserializeDictionaryStringByteArray()
        {
            if (DeserializeBool())
            {
                Dictionary<string, byte[]> ret = new Dictionary<string, byte[]>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                {
                    string key = DeserializeString();
                    byte[] val = DeserializeArrayByte();
                    ret.Add(key, val);
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a bitmap
        /// </summary>
        /// <param name="val">a bitmap</param>
        void Serialize(Bitmap val)
        {
            MemoryStream ms = new MemoryStream();
            val.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bitmapData = ms.ToArray();
            Serialize(bitmapData);
        }
        /// <summary>
        /// Deserialize Bitmap from stream
        /// </summary>
        /// <returns>Deserialized Bitmap</returns>
        Bitmap DeserializeBitmap()
        {
            byte[] byt = DeserializeArrayByte();
            if (byt != null)
            {
                MemoryStream _s = new MemoryStream(byt);
                Bitmap ret = (Bitmap)Image.FromStream(_s);
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize Dictionary string bitmap
        /// </summary>
        /// <param name="val">Dictionary string, Bitmap</param>
        void Serialize(Dictionary<Position2D, Bitmap> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (Position2D key in val.Keys)
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// Deserialize a Dictionary string, Bitmap
        /// </summary>
        /// <returns>Deserialized Dictionary string, Bitmap</returns>
        Dictionary<Position2D, Bitmap> DeserializeDictionaryStringBitmap()
        {
            if (DeserializeBool())
            {
                Dictionary<Position2D, Bitmap> ret = new Dictionary<Position2D, Bitmap>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                {
                    ret.Add(DeserializePosition2D(), DeserializeBitmap());
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a SingleObjectState by google protocol and save to stream
        /// </summary>
        /// <param name="val">a SingleObjectState</param>
        void Serialize(SingleObjectState state)
        {
            Serialize(state != null);
            if (state != null)
            {
                Serialize(state.Acceleration);
                Serialize(state.Angle);
                Serialize(state.AngularSpeed);
                Serialize(state.ChangedInSimulutor);
                Serialize(state.Location);
                Serialize(state.Speed);
                Serialize((int)state.Type);
                Serialize(state.Opacity);
                Serialize(state.IsShown);
                Serialize(state.TimeOfCapture);
            }
        }
        /// <summary>
        /// Diserialize an object of SingleObjectState from a stream
        /// </summary>
        /// <returns>Deserialized SingleObjectState</returns>
        private SingleObjectState DeserializeSingleObjectState()
        {
            if (DeserializeBool())
            {
                SingleObjectState state = new SingleObjectState();

                state.Acceleration = DeserializeVector2D();
                state.Angle = DeserializeNullableFoat();
                state.AngularSpeed = DeserializeNullableFoat();
                state.ChangedInSimulutor = DeserializeBool();
                state.Location = DeserializePosition2D();
                state.Speed = DeserializeVector2D();
                state.Type = (ObjectType)DeserializeInt();
                state.Opacity = DeserializeDouble();
                state.IsShown = DeserializeBool();
                state.TimeOfCapture = DeserializeDouble();
                return state;
            }
            return null;
        }
        /// <summary>
        /// Serialize a TimeSpan by google protocol and save to stream
        /// </summary>
        /// <param name="val">a TimeSpan Class</param>
        void Serialize(TimeSpan val)
        {

            Serialize(val.Days);
            Serialize(val.Hours);
            Serialize(val.Minutes);
            Serialize(val.Seconds);
            Serialize(val.Milliseconds);
        }
        /// <summary>
        /// Diserialize an object of TimeSpan from a stream
        /// </summary>
        /// <returns>Deserialized TimeSpan</returns>
        TimeSpan DeserializeTimeSpan()
        {
            TimeSpan ret = new TimeSpan(DeserializeInt(), DeserializeInt(), DeserializeInt(), DeserializeInt(), DeserializeInt());
            return ret;
        }
        /// <summary>
        /// Serialize a DateTime by google protocol and save to stream
        /// </summary>
        /// <param name="val">a DateTime Class</param>
        void Serialize(DateTime val)
        {
            Serialize(val.Year);
            Serialize(val.Month);
            Serialize(val.Day);
            Serialize(val.Hour);
            Serialize(val.Minute);
            Serialize(val.Second);
            Serialize(val.Millisecond);
        }
        /// <summary>
        /// Diserialize a DateTime from a stream
        /// </summary>
        /// <returns>Deserialized DateTime</returns>
        DateTime DeserializeDateTime()
        {
            DateTime ret = new DateTime();
            ret.AddYears(DeserializeInt());
            ret.AddMonths(DeserializeInt());
            ret.AddDays(DeserializeInt());
            ret.AddHours(DeserializeInt());
            ret.AddMinutes(DeserializeInt());
            ret.AddSeconds(DeserializeInt());
            ret.AddMilliseconds(DeserializeInt());
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        void Serialize(GameEvents val)
        {
            Serialize(val.BlueScore);
            Serialize(val.YellowScore);
            Serialize(val.TimeOfstage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        GameEvents DeserializeGameEvents()
        {
            return new GameEvents()
            {
                BlueScore = DeserializeInt(),
                YellowScore = DeserializeInt(),
                TimeOfstage = DeserializeInt()
            };
        }
        /// <summary>
        /// Serialize an object of WorldModel by google protocol and save to stream
        /// Use in whole googleserializer for serilizing AiToVisDataWrapper
        /// </summary>
        /// <param name="val">an object of WorldModel</param>
        private void Serialize(WorldModel Model)
        {
            Serialize(Model != null);
            if (Model != null)
            {

                Serialize(Model.BallHeight);
                Serialize(Model.BallState);
                Serialize(Model.BallStateSlow);
                Serialize(Model.BallStateFast);

                Serialize(Model.FieldIsInverted);
                Serialize(Model.GlobalKickingProhibited);
                Serialize(Model.GoalieID);
                Serialize(Model.Opponents);
                Serialize(Model.OpponentScore);
                Serialize(Model.OurMarkerISYellow);
                Serialize(Model.OurRobots);
                Serialize(Model.OurScore);
                Serialize(Model.SequenceNumber);
                Serialize((int)Model.Status);
                Serialize(Model.TimeElapsed);
                Serialize(Model.TimeOfAction);

                Serialize(Model.TimeLeft);
            }
        }


        /// <summary>
        /// Serialize an object of WorldModel by google protocol and save to stream
        /// </summary>
        /// <param name="Model">an object of WorldModel</param>
        public void SerializeWorldModel(WorldModel Model)
        {
            stream = new MemoryStream();
            Serialize(Model != null);
            if (Model != null)
            {
                Serialize(Model.BallHeight);
                Serialize(Model.BallState);
                Serialize(Model.BallStateSlow);
                Serialize(Model.BallStateFast);

                Serialize(Model.FieldIsInverted);
                Serialize(Model.GlobalKickingProhibited);
                Serialize(Model.GoalieID);
                Serialize(Model.Opponents);
                Serialize(Model.OpponentScore);
                Serialize(Model.OurMarkerISYellow);
                Serialize(Model.OurRobots);
                Serialize(Model.OurScore);
                Serialize(Model.SequenceNumber);
                Serialize((int)Model.Status);

                Serialize(Model.TimeElapsed);
                Serialize(Model.TimeOfAction);
                Serialize(Model.TimeLeft);
            }
        }
        /// <summary>
        /// Diserialize a WorldModel from a stream
        /// </summary>
        /// <returns>Deserialized WorldModel</returns>
        public WorldModel DeserializeWorldModel(MemoryStream strem)
        {
            stream = strem;
            if (DeserializeBool())
            {
                WorldModel model = new WorldModel();
                model.BallHeight = DeserializeNullableDouble();
                model.BallState = DeserializeSingleObjectState();
                model.BallStateSlow = DeserializeSingleObjectState();
                model.BallStateFast = DeserializeSingleObjectState();

                model.FieldIsInverted = DeserializeBool();
                model.GlobalKickingProhibited = DeserializeBool();
                model.GoalieID = DeserializeNullableInt();
                model.Opponents = DeserializeDictionaryIntState();
                model.OpponentScore = DeserializeInt();
                model.OurMarkerISYellow = DeserializeBool();
                model.OurRobots = DeserializeDictionaryIntState();
                model.OurScore = DeserializeInt();
                model.SequenceNumber = DeserializeInt();
                model.Status = (GameStatus)DeserializeInt();

                model.TimeElapsed = DeserializeTimeSpan();
                model.TimeOfAction = DeserializeDateTime();
                model.TimeLeft = DeserializeTimeSpan();
                return model;
            }
            return null;
        }
        /// <summary>
        /// Serialize Dictionary(int,SingleWirelessCommand) by google protocol and save to stream
        /// Use in whole googleserializer for serilizing AiToVisDataWrapper 
        /// </summary>
        /// <param name="val">a Dictionary(int,SingleWirelessCommand) object</param>
        private void Serialize(Dictionary<int, SingleWirelessCommand> swcs)
        {
            lock (swcs)
            {


                Serialize(swcs != null);
                if (swcs != null)
                {
                    Serialize(swcs.Count);
                    int counter = 0;
                    foreach (int key in swcs.Keys.ToList())
                    {
                        counter++;
                        Serialize(key);
                        Serialize(swcs[key]);
                    }
                }
                //if (counter != swcs.Count)
                //    throw new Exception("This should never happen");
            }
        }
        ///<summary>
        /// Serialize Dictionary(int,SingleWirelessCommand) by google protocol and save to stream
        /// </summary>
        /// <param name="val">a Dictionary(int,SingleWirelessCommand) object</param>
        /// <returns>stream memory</returns>
        public MemoryStream SerializeDictionaryintSinqleWirelessCommand(Dictionary<int, SingleWirelessCommand> swcs)
        {
            stream = new MemoryStream();
            Serialize(swcs != null);
            if (swcs != null)
            {
                Serialize(swcs.Count);
                int counter = 0;
                foreach (int key in swcs.Keys)
                {
                    counter++;
                    Serialize(key);
                    Serialize(swcs[key]);
                }
                if (counter != swcs.Count)
                    throw new Exception("This should never happen");
            }
            return stream;
        }
        /// <summary>
        /// Diserialize Dictionary of int,SingleWirelessCommand from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of int,SingleWirelessCommand</returns>
        public Dictionary<int, SingleWirelessCommand> DeserializeDictionaryIntSingleWirelssCommand()
        {
            if (!DeserializeBool())
                return null;
            Dictionary<int, SingleWirelessCommand> ret = new Dictionary<int, SingleWirelessCommand>();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
            {
                int key = DeserializeInt();
                SingleWirelessCommand swc = DeserializeSingleWirelessCommand();
                ret.Add(key, swc);
            }
            return ret;
        }
        /// <summary>
        /// Serialize a SingleWirelessCommand by google protocol and save to stream
        /// </summary>
        /// <param name="val">an object of SingleWirelessCommand</param>
        private void Serialize(SingleWirelessCommand SWC)
        {
            Serialize(SWC != null);
            if (SWC != null)
            {
                Serialize(SWC.isChipKick);
                Serialize(SWC.isDelayedKick);
                Serialize(SWC._kickPower);
                Serialize(SWC._kickPowerByte);
                Serialize(SWC.Motor1);
                Serialize(SWC.Motor2);
                Serialize(SWC.Motor3);
                Serialize(SWC.Motor4);
                Serialize(SWC.SpinBack);
                Serialize(SWC.Vx);
                Serialize(SWC.Vy);
                Serialize(SWC.W);
                Serialize(SWC.Color);
                Serialize(SWC.BackSensor);
            }
        }
        /// <summary>
        /// Diserialize a SingleWirelessCommand from a stream
        /// </summary>
        /// <returns>Deserialized SingleWirelessCommand</returns>
        private SingleWirelessCommand DeserializeSingleWirelessCommand()
        {
            if (DeserializeBool())
            {
                SingleWirelessCommand swc = new SingleWirelessCommand()
                {
                    isChipKick = DeserializeBool(),
                    isDelayedKick = DeserializeBool(),
                    _kickPower = DeserializeDouble(),
                    _kickPowerByte = DeserializeByte(),
                    Motor1 = DeserializeShort(),
                    Motor2 = DeserializeShort(),
                    Motor3 = DeserializeShort(),
                    Motor4 = DeserializeShort(),
                    SpinBack = DeserializeDouble(),
                    Vx = DeserializeDouble(),
                    Vy = DeserializeDouble(),
                    W = DeserializeDouble(),
                    Color = DeserializeColor(),
                    BackSensor = DeserializeBool()
                };
                return swc;
            }
            return null;
        }
        /// <summary>
        /// serizlize a dictionary(int , int)
        /// </summary>
        /// <param name="dict">a dictionary(int , int)</param>
        private void Serialize(Dictionary<int, int> dict)
        {
            Serialize(dict != null);
            if (dict != null)
            {
                Serialize(dict.Count);
                foreach (int key in dict.Keys)
                {
                    Serialize(key);
                    Serialize(dict[key]);
                }
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of int,int from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of int,int</returns>
        private Dictionary<int, int> DeSerializeDictionaryIntInt()
        {
            if (!DeserializeBool())
                return null;
            int count = DeserializeInt();
            Dictionary<int, int> dict = new Dictionary<int, int>(count);
            for (int i = 0 ; i < count ; i++)
            {
                int key = DeserializeInt();
                int value = DeserializeInt();
                dict.Add(key, value);
            }
            return dict;
        }
        /// <summary>
        /// serialize networksetting
        /// </summary>
        /// <param name="val">a networksetting object</param>
        void Serialize(NetworkSettings val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.AiName);
                Serialize(val.AiPort);
                Serialize(val.CMCname);
                Serialize(val.CMCport);
                Serialize(val.RefIP);
                Serialize(val.RefPort);
                Serialize(val.SimulatorName);
                Serialize(val.SimulatorSendPort);
                Serialize(val.SimulatorRecievePort);
                Serialize(val.SSLVisionIP);
                Serialize(val.SSLVisionPort);
            }
        }
        /// <summary>
        /// Deserialize a networksetting from stream
        /// </summary>
        /// <returns>a networksetting object</returns>
        NetworkSettings DeserializeNetworkSettings()
        {
            NetworkSettings ret = new NetworkSettings();
            if (DeserializeBool())
            {
                ret.AiName = DeserializeString();
                ret.AiPort = DeserializeInt();
                ret.CMCname = DeserializeString();
                ret.CMCport = DeserializeInt();
                ret.RefIP = DeserializeString();
                ret.RefPort = DeserializeInt();
                ret.SimulatorName = DeserializeString();
                ret.SimulatorSendPort = DeserializeInt();
                ret.SimulatorRecievePort = DeserializeInt();
                ret.SSLVisionIP = DeserializeString();
                ret.SSLVisionPort = DeserializeInt();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize dictionary(int,engine)
        /// </summary>
        /// <param name="val">a dictionary(int,engine)</param>
        void Serialize(Dictionary<int, Engines> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (int key in val.Keys.ToList())
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// deserialize a dictionary(int,engine)
        /// </summary>
        /// <returns>a dictionary(int,engine)</returns>
        Dictionary<int, Engines> DeserializeDictionaryIntEngines()
        {
            if (DeserializeBool())
            {
                Dictionary<int, Engines> ret = new Dictionary<int, Engines>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeInt(), DeserializeEngines());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize a dictionary(int,robots)
        /// </summary>
        /// <param name="val">a dictionary(int,robots)</param>
        void Serialize(Dictionary<int, Robots> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (int key in val.Keys)
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// deserialize dictionary(int ,robots)
        /// </summary>
        /// <returns>a dictionary(int ,robots)</returns>
        Dictionary<int, Robots> DeserializeDictionaryIntRobots()
        {
            if (DeserializeBool())
            {
                Dictionary<int, Robots> ret = new Dictionary<int, Robots>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeInt(), DeserializeRobots());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize a dictionary(int, string)
        /// </summary>
        /// <param name="val">a dictionary(int, string)</param>
        void Serialize(Dictionary<int, string> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (int key in val.Keys)
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// deserialize a dictionary(int, string)
        /// </summary>
        /// <returns>a dictionary(int, string)</returns>
        Dictionary<int, string> DeserializeDictionaryIntString()
        {
            if (DeserializeBool())
            {
                Dictionary<int, string> ret = new Dictionary<int, string>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeInt(), DeserializeString());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serizlie a list of string 
        /// </summary>
        /// <param name="val">a list of string</param>
        void Serialize(List<string> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                for (int i = 0 ; i < val.Count ; i++)
                    Serialize(val[i]);
            }
        }
        /// <summary>
        /// deserialize list(string)
        /// </summary>
        /// <returns></returns>
        List<string> DeserializeListString()
        {
            if (DeserializeBool())
            {
                List<string> ret = new List<string>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        void Serialize(Color val)
        {
            Serialize(val.R);
            Serialize(val.G);
            Serialize(val.B);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Color DeserializeColor()
        {
            return Color.FromArgb((byte)DeserializeInt(), (byte)DeserializeInt(), (byte)DeserializeInt());
        }
        /// <summary>
        /// serialize a engine
        /// </summary>
        /// <param name="val"></param>
        public void Serialize(Engines val)
        {
            Serialize(val.Id);
            Serialize(val.ReverseColor);
            Serialize(val.ReverseSide);
        }
        /// <summary>
        /// deserialize a engine
        /// </summary>
        /// <returns></returns>
        public Engines DeserializeEngines()
        {
            Engines ret = new Engines();
            ret.Id = DeserializeInt();
            ret.ReverseColor = DeserializeBool();
            ret.ReverseSide = DeserializeBool();
            return ret;
        }
        /// <summary>
        /// serialize an Object[,] (Engine Setting Table)
        /// </summary>
        /// <param name="val">object[,]</param>
        /// <param name="rowCount">count of object[,]'s rows</param>
        public void Serialize(List<Engines> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                for (int i = 0 ; i < val.Count ; i++)
                    Serialize(val[i]);
            }
        }
        /// <summary>
        /// Deserialize a list of Engins
        /// </summary>
        /// <returns>Deserialized list of Engins</returns>
        public List<Engines> DeserializeListEngines()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<Engines> ret = new List<Engines>();
                for (int i = 0 ; i < count ; i++)
                {
                    Engines e = new Engines()
                    {
                        Id = DeserializeInt(),
                        ReverseColor = DeserializeBool(),
                        ReverseSide = DeserializeBool(),
                    };
                    ret.Add(e);
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize a robot
        /// </summary>
        /// <param name="val"></param>
        void Serialize(Robots val)
        {
            Serialize(val.Id);
            Serialize(val.EngineId);
            Serialize(val.TeamColor);
        }
        /// <summary>
        /// deserialize a robot
        /// </summary>
        /// <returns></returns>
        Robots DeserializeRobots()
        {
            Robots ret = new Robots();
            ret.Id = DeserializeInt();
            ret.EngineId = DeserializeInt();
            ret.TeamColor = DeserializeColor();
            return ret;
        }
        /// <summary>
        /// serizlie a list of robot
        /// </summary>
        /// <param name="val"></param>
        public void Serialize(List<Robots> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                for (int i = 0 ; i < val.Count ; i++)
                {
                    Serialize(val[i]);
                }
            }
        }
        /// <summary>
        /// deserialize a list of robot
        /// </summary>
        /// <returns></returns>
        public List<Robots> DeserializeListRobots()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<Robots> ret = new List<Robots>();
                for (int i = 0 ; i < count ; i++)
                {
                    Robots r = DeserializeRobots();

                    ret.Add(r);
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize a (Network Setting Table)
        /// </summary>
        /// <param name="val">object[,]</param>
        /// <param name="rowCount">count of object[,]'s rows</param>
        public void SerializeNetsetting(object[,] val, int rowCount)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(rowCount);
                for (int i = 0 ; i < rowCount ; i++)
                {
                    Serialize((int)val[i, 0]);
                    Serialize((string)val[i, 1]);
                    Serialize((int)val[i, 2]);
                    Serialize((string)val[i, 3]);
                    Serialize((int)val[i, 4]);
                    Serialize((string)val[i, 5]);
                    Serialize((int)val[i, 6]);
                }
            }
        }
        /// <summary>
        /// Deserialize an object[,] (Network Setting Table)
        /// </summary>
        /// <returns>Deserialized object[,]</returns>
        public object[,] DeserializeNetsetting()
        {
            if (DeserializeBool())
            {
                int rowcount = DeserializeInt();
                object[,] ret = new object[rowcount, 7];
                for (int i = 0 ; i < rowcount ; i++)
                {
                    ret[i, 0] = DeserializeInt();
                    ret[i, 1] = DeserializeString();
                    ret[i, 2] = DeserializeInt();
                    ret[i, 3] = DeserializeString();
                    ret[i, 4] = DeserializeInt();
                    ret[i, 5] = DeserializeString();
                    ret[i, 6] = DeserializeInt();
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a ChartObject
        /// </summary>
        /// <param name="val">a ChartObject</param>
        private void Serialize(ChartObject val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.GraphColor.R);
                Serialize(val.GraphColor.G);
                Serialize(val.GraphColor.B);
                Serialize(val.CollectionName);
                Serialize(val.HasX);
                Serialize(val.Log);
                Serialize(val.ShowInChart);
                Serialize(val.ChartData);
            }
        }
        /// <summary>
        ///  Deserialize a ChartObject
        /// </summary>
        /// <returns>deserialized chartobject</returns>
        private ChartObject DeserializeChartObject()
        {
            if (DeserializeBool())
            {
                ChartObject ret = new ChartObject();
                ret.GraphColor = Color.FromArgb(DeserializeByte(), DeserializeByte(), DeserializeByte());
                ret.CollectionName = DeserializeString();
                ret.HasX = DeserializeBool();
                ret.Log = DeserializeBool();
                ret.ShowInChart = DeserializeBool();
                ret.ChartData = DeserializelistPosition2D();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize Dictionary string,ChartObject
        /// </summary>
        /// <param name="val">a Dictionary string,ChartObject </param>
        private void Serialize(Dictionary<string, ChartObject> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (string key in val.Keys)
                {
                    Serialize(key);
                    Serialize(val[key]);
                }
            }
        }
        /// <summary>
        /// Deserialize a Dictionary Color, ChartObject 
        /// </summary>
        /// <returns>Deserialized Dictionary Color, ChartObject</returns>
        private Dictionary<string, ChartObject> DeserializeDictionaryStringChartObject()
        {
            if (DeserializeBool())
            {
                Dictionary<string, ChartObject> ret = new Dictionary<string, ChartObject>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                {
                    string val = DeserializeString();
                    ChartObject co = DeserializeChartObject();
                    ret.Add(val, co);
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serizlize darwing group
        /// </summary>
        /// <param name="val">an object of DrawCollection</param>
        void Serialize(DrawingGroup val)
        {
            Serialize(val.RegionPixelWidth);

            Serialize(val.FillRegions != null);
            if (val.FillRegions != null)
                Serialize(val.FillRegions);

            Serialize(val.CharterData != null);
            if (val.CharterData != null)
            {
                Serialize(val.CharterData);
                val.CharterData.Clear();
            }

            Serialize(val.ObjectToDraw != null);
            if (val.ObjectToDraw != null)
                Serialize(val.ObjectToDraw);

            Serialize(val.PermanentCirclesToDraw != null);
            if (val.PermanentCirclesToDraw != null)
                Serialize(val.PermanentCirclesToDraw);

            Serialize(val.PermanentlyLineToDraw != null);
            if (val.PermanentlyLineToDraw != null)
                Serialize(val.PermanentlyLineToDraw);

            Serialize(val.MomentlyLineToDraw != null);
            if (val.MomentlyLineToDraw != null)
                Serialize(val.MomentlyLineToDraw);

            Serialize(val.PathToDraw != null);
            if (val.PathToDraw != null)
                Serialize(val.PathToDraw);

            Serialize(val.PermanentRectangleToDraw != null);
            if (val.PermanentRectangleToDraw != null)
                Serialize(val.PermanentRectangleToDraw);

            Serialize(val.MomentRectangleToDraw != null);
            if (val.MomentRectangleToDraw != null)
                Serialize(val.MomentRectangleToDraw);

            Serialize(val.PermanentTextToDraw != null);
            if (val.PermanentTextToDraw != null)
                Serialize(val.PermanentTextToDraw);

            Serialize(val.RegionToDraw != null);
            if (val.RegionToDraw != null)
                Serialize(val.RegionToDraw);

            Serialize(val.MomentlyCirclesToDraw != null);
            if (val.MomentlyCirclesToDraw != null)
                Serialize(val.MomentlyCirclesToDraw);

            Serialize(val.MomentlyTextToDraw != null);
            if (val.MomentlyTextToDraw != null)
                Serialize(val.MomentlyTextToDraw);

            Serialize(val.MomentToken != null);
            if (val.MomentToken != null)
                Serialize(val.MomentToken);

            Serialize(val.PermanentToken != null);
            if (val.PermanentToken != null)
                Serialize(val.PermanentToken);

            Serialize(val.MomentlyVector != null);
            if (val.MomentlyVector != null)
                Serialize(val.MomentlyVector);

            Serialize(val.PermanentVector != null);
            if (val.PermanentVector != null)
                Serialize(val.PermanentVector);


        }
        /// <summary>
        /// Deserialize A drawing group
        /// </summary>
        /// <returns></returns>
        DrawingGroup DeserializeDrawingGroup()
        {
            DrawingGroup ret = new DrawingGroup();
            ret.RegionPixelWidth = DeserializeInt();

            if (DeserializeBool())
                ret.FillRegions = DeserializeDictionaryStringListPosition2D();
            else
                ret.FillRegions = null;

            if (DeserializeBool())
            {
                Dictionary<string, ChartObject> d1 = DeserializeDictionaryStringChartObject();
                foreach (string key in d1.Keys)
                {
                    if (ret.CharterData != null)
                    {
                        if (!ret.CharterData.ContainsKey(key))
                            ret.Add(key, d1[key]);
                        else
                        {
                            for (int i = 0 ; i < d1[key].ChartData.Count ; i++)
                            {
                                ret.CharterData[key].ChartData.Add(d1[key].ChartData[i]);
                            }
                        }
                    }
                    else
                    {
                        ret.Add(key, d1[key]);
                    }
                }
            }
            else
                ret.CharterData = null;

            if (DeserializeBool())
                ret.ObjectToDraw = DeserializeDictionaryStringSingleObjectState();
            else
                ret.ObjectToDraw = null;

            if (DeserializeBool())
                ret.PermanentCirclesToDraw = DeserializeDictionarySrtingCircle();
            else
                ret.PermanentCirclesToDraw = null;

            if (DeserializeBool())
                ret.PermanentlyLineToDraw = DeserializeDictionaryStringLine();
            else
                ret.PermanentlyLineToDraw = null;

            if (DeserializeBool())
                ret.MomentlyLineToDraw = DeserializeDictionaryStringLine();
            else
                ret.MomentlyLineToDraw = null;

            if (DeserializeBool())
                ret.PathToDraw = DeserializeDictionaryIntListPosition2D();
            else
                ret.PathToDraw = null;

            if (DeserializeBool())
                ret.PermanentRectangleToDraw = DeserializeDictionaryStringRectangleF();
            else
                ret.PermanentRectangleToDraw = null;

            if (DeserializeBool())
                ret.MomentRectangleToDraw = DeserializeDictionaryStringRectangleF();
            else
                ret.MomentRectangleToDraw = null;

            if (DeserializeBool())
            {
                Dictionary<string, Position2D> d2 = DeserializeDictionaryStringPosition2D();
                foreach (string key in d2.Keys)
                {
                    string[] s = key.Split(new char[] { '@', '@', '@', '@' });
                    float r = float.Parse(s[1]) / 255f;
                    float g = float.Parse(s[2]) / 255f;
                    float b = float.Parse(s[3]) / 255f;
                    float a = float.Parse(s[4]);
                    ret.AddPermanently(key, Color.FromArgb((byte)r, (byte)g, (byte)b), a, d2[key]);
                }
            }
            else
                ret.PermanentTextToDraw = null;

            if (DeserializeBool())
                ret.RegionToDraw = DeserializeListPoint();
            else
                ret.RegionToDraw = null;

            if (DeserializeBool())
                ret.MomentlyCirclesToDraw = DeserializeDictionarySrtingCircle();
            else
                ret.MomentlyCirclesToDraw = null;

            if (DeserializeBool())
                ret.MomentlyTextToDraw = DeserializeDictionaryStringPosition2D();
            else
                ret.MomentlyTextToDraw = null;


            if (DeserializeBool())
                ret.MomentToken = DeserializeDictionaryStringPosition2D();
            else
                ret.MomentToken = null;

            if (DeserializeBool())
                ret.PermanentToken = DeserializeDictionaryStringPosition2D();
            else
                ret.PermanentToken = null;

            if (DeserializeBool())
                ret.MomentlyVector = DeserializeDictionaryStringVector2D();
            else
                ret.MomentlyVector = null;

            if (DeserializeBool())
                ret.PermanentVector = DeserializeDictionaryStringVector2D();
            else
                ret.PermanentVector = null;

            return ret;
        }
        /// <summary>
        /// Serialize a merger and tracker setting class
        /// </summary>
        /// <param name="val"></param>
        public void Serialize(MergerAndTrackerSetting val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.CorrectAngleError);
                Serialize(val.OnGame);
                Serialize(val.MaxFrameToShadow);
                Serialize(val.ActionDelay);
                Serialize(val.CalculateRegion);
                Serialize((int)val.CamState);
                Serialize(val.MaxBallDist);
                Serialize(val.MaxNotSeen);
                Serialize(val.MaxOpponenetDistance);
                Serialize(val.MaxToImagine);
            }
        }
        /// <summary>
        /// Deserialize a merger and tracket settings
        /// </summary>
        /// <returns></returns>
        public MergerAndTrackerSetting DeserializeMergerTrackerSettings()
        {
            if (DeserializeBool())
            {
                MergerAndTrackerSetting ret = new MergerAndTrackerSetting();
                ret.CorrectAngleError = DeserializeBool();
                ret.OnGame = DeserializeBool();
                ret.MaxFrameToShadow = DeserializeInt();
                ret.ActionDelay = DeserializeDouble();
                ret.CalculateRegion = DeserializeBool();
                ret.CamState = (MergerAndTrackerSetting.CameraState)DeserializeInt();
                ret.MaxBallDist = DeserializeDouble();
                ret.MaxNotSeen = DeserializeInt();
                ret.MaxOpponenetDistance = DeserializeDouble();
                ret.MaxToImagine = DeserializeInt();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Deserialize a dictionary of string DrawCollection
        /// </summary>
        /// <returns>deserialized DrawCollection</returns>
        Dictionary<string, DrawingGroup> DeserializeDictionaryStringDrawCollection()
        {
            if (DeserializeBool())
            {
                Dictionary<string, DrawingGroup> ret = new Dictionary<string, DrawingGroup>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                {
                    string key = DeserializeString();
                    DrawingGroup val = DeserializeDrawingGroup();
                    ret.Add(key, val);
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize a dictionary of string, DrawCollection
        /// </summary>
        /// <param name="val">a dictionary of string, DrawCollection</param>
        void Serialize(Dictionary<string, DrawCollection> val)
        {
            Serialize(val != null);
            if (val != null)
            {

                lock (val)
                {
                    int count = val.Count;
                    Dictionary<string, DrawCollection> temp = new Dictionary<string, DrawCollection>();
                    foreach (string key in val.Keys)
                        if (key != null && val[key] != null)
                            temp.Add(key, val[key]);
                    Serialize(temp.Count);
                    foreach (string key in temp.Keys)
                    {
                        Serialize(key);
                        Serialize(temp[key]);
                    }
                }
            }
        }
        /// <summary>
        /// Serialize a Dictionary(string,Double) by google protocol and save to stream
        /// </summary>
        /// <param name="val">a Dictionary(string,Double)</param>
        void Serialize(Dictionary<string, double?> dict)
        {
            Serialize(dict != null);
            if (dict != null)
            {
                Serialize(dict.Count);
                int counter = 0;
                foreach (string key in dict.Keys)
                {
                    counter++;
                    Serialize(key);
                    Serialize(dict[key]);
                }
                if (counter != dict.Count)
                    throw new Exception("This should never happen");
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of string,double from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of string,double</returns>
        Dictionary<string, double?> DeserializeDictionaryStringNullDouble()
        {
            if (!DeserializeBool())
                return null;
            Dictionary<string, double?> ret = new Dictionary<string, double?>();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
            {
                string key = DeserializeString();
                double? var = DeserializeNullableDouble();
                ret.Add(key, var);
            }
            return ret;
        }
        ///// <summary>
        ///// Serialize an object of AiToVisDataWrapper.visualizerData by google protocol and save to stream
        ///// </summary>
        ///// <param name="val">an object of AiToVisDataWrapper.visualizerData</param>
        //private void SerializeVisualizerData()
        //{
        //    Serialize(VisualizerData.CurrentSate);

        //    Serialize(VisualizerData.CurrentTechniques);

        //    Serialize(VisualizerData.BallStatus);

        //    Serialize(VisualizerData.CustomVariables != null);
        //    if(VisualizerData.CustomVariables != null)
        //        Serialize(VisualizerData.CustomVariables);

        //    Serialize(VisualizerData.Groups != null);
        //    if (VisualizerData.Groups != null)
        //    {
        //        Serialize(VisualizerData.Groups);
        //        //VisualizerData.Groups.Clear();
        //    }

        //    Serialize(VisualizerData.RegionPixelWidth);

        //    Serialize(VisualizerData.FillRegions != null);
        //    if (VisualizerData.FillRegions != null)
        //        Serialize(VisualizerData.FillRegions);

        //    Serialize(VisualizerData.ObjectToDraw != null);
        //    if (VisualizerData.ObjectToDraw != null)
        //        Serialize(VisualizerData.ObjectToDraw);

        //    Serialize(VisualizerData.MomentlyLineToDraw != null);
        //    if (VisualizerData.MomentlyLineToDraw != null)
        //        Serialize(VisualizerData.MomentlyLineToDraw);

        //    Serialize(VisualizerData.CharterData != null);
        //    if (VisualizerData.CharterData != null)
        //    {
        //        Serialize(VisualizerData.CharterData);
        //        VisualizerData.CharterData.Clear();
        //    }

        //    Serialize(VisualizerData.PermanentCirclesToDraw != null);
        //    if (VisualizerData.PermanentCirclesToDraw != null)
        //        Serialize(VisualizerData.PermanentCirclesToDraw);

        //    Serialize(VisualizerData.PermanentlyLineToDraw != null);
        //    if (VisualizerData.PermanentlyLineToDraw != null)
        //        Serialize(VisualizerData.PermanentlyLineToDraw);

        //    Serialize(VisualizerData.PathToDraw != null);
        //    if (VisualizerData.PathToDraw != null)
        //    {
        //        Serialize(VisualizerData.PathToDraw);
        //        VisualizerData.PathToDraw.Clear();
        //    }

        //    Serialize(VisualizerData.PermanentRectangleToDraw != null);
        //    if (VisualizerData.PermanentRectangleToDraw != null)
        //        Serialize(VisualizerData.PermanentRectangleToDraw);

        //    Serialize(VisualizerData.MomentRectangleToDraw != null);
        //    if (VisualizerData.MomentRectangleToDraw != null)
        //        Serialize(VisualizerData.MomentRectangleToDraw);

        //    Serialize(VisualizerData.PermanentTextToDraw!=null);
        //    if (VisualizerData.PermanentTextToDraw != null)
        //        Serialize(VisualizerData.PermanentTextToDraw);

        //    Serialize(VisualizerData.RegionToDraw != null);
        //    if (VisualizerData.RegionToDraw != null)
        //        Serialize(VisualizerData.RegionToDraw);

        //    Serialize(VisualizerData.MomentlyCirclesToDraw != null);
        //    if (VisualizerData.MomentlyCirclesToDraw != null)
        //        Serialize(VisualizerData.MomentlyCirclesToDraw);

        //    Serialize(VisualizerData.MomentlyTextToDraw != null);
        //    if (VisualizerData.MomentlyTextToDraw != null)
        //        Serialize(VisualizerData.MomentlyTextToDraw);



        //        Serialize(VisualizerData.MomentToken != null);
        //        if (VisualizerData.MomentToken != null)
        //            lock (VisualizerData.MomentToken)
        //                Serialize(VisualizerData.MomentToken);



        //        Serialize(VisualizerData.PermanentToken != null);
        //        if (VisualizerData.PermanentToken != null)
        //            lock (VisualizerData.PermanentToken)
        //                Serialize(VisualizerData.PermanentToken);


        //    Serialize(VisualizerData.MomentlyVector != null);
        //    if (VisualizerData.MomentlyVector != null)
        //        Serialize(VisualizerData.MomentlyVector);

        //    Serialize(VisualizerData.PermanentVector != null);
        //    if (VisualizerData.PermanentVector != null)
        //        Serialize(VisualizerData.PermanentVector);

        //}
        ///// <summary>
        ///// Diserialize a AiToVisDataWrapper.visualizerData from a stream
        ///// </summary>
        ///// <returns>Deserialized SingleWirelessCommand</returns>
        //private void DeserializevisualizerData()
        //{
        //    VisualizerData.CurrentSate = DeserializeString();

        //    VisualizerData.CurrentTechniques = DeserializeString();

        //    VisualizerData.BallStatus = DeserializeString();

        //    if (DeserializeBool())
        //    {
        //        Dictionary<string, double?> vardic = DeserializeDictionaryStringNullDouble();
        //        foreach (string item in vardic.Keys)
        //            DrawingObjects.AddObject(item, vardic[item]);
        //    }
        //    else
        //        VisualizerData.CustomVariables = null;


        //    if (DeserializeBool())
        //    {
        //        //VisualizerData.Groups = DeserializeDictionaryStringDrawCollection();
        //        Dictionary<string, DrawCollection> Gdict = DeserializeDictionaryStringDrawCollection();
        //        foreach (string key in Gdict.Keys)
        //            VisualizerData.Add(key, Gdict[key]);
        //    }
        //    else
        //        VisualizerData.Groups = null;

        //    VisualizerData.RegionPixelWidth = DeserializeInt();

        //    if (DeserializeBool())
        //        VisualizerData.FillRegions = DeserializeDictionaryStringListPosition2D();
        //    else
        //        VisualizerData.FillRegions = null;

        //    if (DeserializeBool())
        //        VisualizerData.ObjectToDraw = DeserializeDictionaryStringSingleObjectState();
        //    else
        //        VisualizerData.ObjectToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.MomentlyLineToDraw = DeserializeDictionaryStringLine();
        //    else
        //        VisualizerData.MomentlyLineToDraw = null;

        //    if (DeserializeBool())
        //    {
        //        Dictionary<string, ChartObject> d1 = DeserializeDictionaryStringChartObject();
        //        foreach (string key in d1.Keys)
        //        {    
        //            if (VisualizerData.CharterData != null)
        //            {
        //                if (!VisualizerData.CharterData.ContainsKey(key))
        //                    VisualizerData.Add(key,d1[key]);
        //                else
        //                {
        //                    for (int i = 0; i < d1[key].ChartData.Count; i++)
        //                    {
        //                        VisualizerData.CharterData[key].ChartData.Add(d1[key].ChartData[i]);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                VisualizerData.Add(key, d1[key]);
        //            }   
        //        }
        //    }
        //    else
        //        VisualizerData.CharterData = null;

        //    if (DeserializeBool())
        //        VisualizerData.PermanentCirclesToDraw = DeserializeDictionarySrtingCircle();
        //    else
        //        VisualizerData.PermanentCirclesToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.PermanentlyLineToDraw = DeserializeDictionaryStringLine();
        //    else
        //        VisualizerData.PermanentlyLineToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.PathToDraw = DeserializeDictionaryIntListPosition2D();
        //    else
        //        VisualizerData.PathToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.PermanentRectangleToDraw = DeserializeDictionaryStringRectangleF();
        //    else
        //        VisualizerData.PermanentRectangleToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.MomentRectangleToDraw = DeserializeDictionaryStringRectangleF();
        //    else
        //        VisualizerData.MomentRectangleToDraw = null;

        //    if (DeserializeBool())
        //    {
        //        Dictionary<string,Position2D> d2 = DeserializeDictionaryStringPosition2D();
        //        //foreach (string key in d2.Keys)
        //        //        DrawingObjects.AddObject(key, d2[key]);
        //        VisualizerData.PermanentTextToDraw = d2;
        //    }
        //    else
        //        VisualizerData.PermanentTextToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.RegionToDraw = DeserializeListPoint();
        //    else
        //        VisualizerData.RegionToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.MomentlyCirclesToDraw = DeserializeDictionarySrtingCircle();
        //    else
        //        VisualizerData.MomentlyCirclesToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.MomentlyTextToDraw = DeserializeDictionaryStringPosition2D();
        //    else
        //        VisualizerData.MomentlyTextToDraw = null;

        //    if (DeserializeBool())
        //        VisualizerData.MomentToken = DeserializeDictionaryStringPosition2D();
        //    else
        //        VisualizerData.MomentToken = null;

        //    if (DeserializeBool())
        //        VisualizerData.PermanentToken = DeserializeDictionaryStringPosition2D();
        //    else
        //        VisualizerData.PermanentToken = null;

        //    if (DeserializeBool())
        //        VisualizerData.MomentlyVector = DeserializeDictionaryStringVector2D();
        //    else
        //        VisualizerData.MomentlyVector = null;

        //    if (DeserializeBool())
        //        VisualizerData.PermanentVector = DeserializeDictionaryStringVector2D();
        //    else
        //        VisualizerData.PermanentVector = null;

        //}
        ///// <summary>
        ///// Serialize an object of AiToVisWrapper by google protocol and save to stream
        ///// </summary>
        ///// <param name="val">an object of AiToVisWrapper</param>
        //public MemoryStream Serialize(MRL.SSL.GameDefinitions.AiToVisDataWrapper vdw)
        //{
        //    stream = new MemoryStream();
        //    Serialize(vdw != null);
        //    if (vdw != null)
        //    {
        //        //ballstatus
        //        Serialize(vdw.BallStatus);
        //        //coustom variable
        //        Serialize(vdw.CustomVariables);
        //        //model
        //        Serialize(vdw.Model);
        //        //requaetTable
        //        Serialize(vdw.RequestTable);
        //        //visualizerdata
        //        if (!_disableVisualizerData)
        //            SerializeVisualizerData();
        //        //All Balls
        //        Serialize(vdw.Balls);
        //        //Gameparameters
        //        SerializeGameParameters();
        //        //Loockup Table
        //        SerializeLoockUpTable();
        //        //Custom variable
        //        SerializeCustomVariable();
        //        //Techniques
        //        Serialize(AISettings.SendTechniques);
        //        //clear permanently lists
        //        VisualizerData.ClearPermanentlyLists();
        //    }
        //    return stream;
        //}
        ///// <summary>
        ///// Diserialize a AiToVisDataWrapper from a stream
        ///// </summary>
        ///// <returns>Deserialized AiToVisDataWrapper</returns>
        //public AiToVisDataWrapper DeserializeAiToVisData(bool wait)
        //{
        //    if (!_isLogDeserializing)
        //        stream.Seek(0, 0);
        //    if (DeserializeBool())
        //    {
        //        AiToVisDataWrapper vdw = new AiToVisDataWrapper();
        //        vdw.BallStatus = DeserializeString();
        //        vdw.CustomVariables = DeserializeDictionaryStringDouble();
        //        vdw.Model = DeserilizeWorldModel(stream);
        //        vdw.RequestTable = DeserializeDictionaryStringbool();
        //        if (!_disableVisualizerData)
        //            DeserializevisualizerData();
        //        vdw.Balls = DeserializeDictionaryIntPosiotion2D();
        //        DeserializeGameParameters();
        //        DeserializeLoockUpTable(wait);
        //        DeserializeCustomVariable(wait);
        //        AISettings.SendTechniques = DeserializeTechniques();
        //        return vdw;
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// Serialize an object of VisToAiWrapper by google protocol and save to stream
        ///// </summary>
        ///// <param name="val">an object of VisToAiWrapper</param>
        //public bool Serialize(VisToAiDataWrapper vtoaDW)
        //{
        //    stream = new MemoryStream();
        //    bool changed = false;
        //    Serialize(vtoaDW != null);
        //    if (vtoaDW != null)
        //    {
        //        Serialize(vtoaDW.SendCustomVar);
        //        Serialize(vtoaDW.IsSimulating);
        //        Serialize(vtoaDW.CustomVariables);
        //        if (vtoaDW.RefreeCommandChange == true)
        //        {
        //            Serialize(true);
        //            changed = true;
        //        }
        //        else
        //            Serialize(false);
        //        Serialize(vtoaDW.RefreeCommand);
        //        Serialize(vtoaDW.RequestTable);
        //        Serialize(vtoaDW.Robotcommand);
        //        Serialize(vtoaDW.SelectionBallMode);
        //        Serialize(vtoaDW.SelectedBallIndex);
        //        Serialize(vtoaDW.BallLocation);
        //        Serialize(vtoaDW.ChangSim);
        //        Serialize(vtoaDW.ChangTechniques);
        //        Serialize(vtoaDW.SendTechinques);
        //        Serialize(vtoaDW.SendPlaySetting);
        //    }
        //    return changed;
        //}
        ///// <summary>
        ///// Diserialize a VisToAiDataWrapper from a stream
        ///// </summary>
        ///// <returns>Deserialized VisToVisDataWrapper</returns>
        //public VisToAiDataWrapper DeserializeVisToAiData()
        //{
        //    VisToAiDataWrapper vtoaDW = new VisToAiDataWrapper();
        //    if (DeserializeBool())
        //    {
        //        vtoaDW.SendCustomVar = DeserializeBool();
        //        vtoaDW.IsSimulating = DeserializeBool();
        //        vtoaDW.CustomVariables = DeserializeDictionaryStringDouble();
        //        vtoaDW.RefreeCommandChange = DeserializeBool();
        //        vtoaDW.RefreeCommand = DeserializeString();
        //        vtoaDW.RequestTable = DeserializeDictionaryStringbool();
        //        vtoaDW.Robotcommand = DeserializeDictionaryIntSingleWirelssCommand();
        //        vtoaDW.SelectionBallMode = DeserializeBool();
        //        vtoaDW.SelectedBallIndex = DeserializeNullableInt();
        //        vtoaDW.BallLocation = DeserializePosition2D();
        //        vtoaDW.ChangSim = DeserializeBool();
        //        vtoaDW.ChangTechniques = DeserializeBool();
        //        vtoaDW.SendTechinques = DeserializeBool();
        //        vtoaDW.SendPlaySetting = DeserializeBool();
        //        return vtoaDW;
        //    }
        //    return null;

        //}
        /// <summary>
        /// Serialize coustom varialble
        /// </summary>
        public void SerializeCustomVariable()
        {
            if (!_waitCustom)
            {
                //
                if (TuneVariables.Default.Doubles != null)
                    lock (TuneVariables.Default.Doubles)
                        Serialize(TuneVariables.Default.Doubles);
                else
                    Serialize(TuneVariables.Default.Doubles);
                //
                if (TuneVariables.Default.Integers != null)
                    lock (TuneVariables.Default.Integers)
                        Serialize(TuneVariables.Default.Integers);
                else
                    Serialize(TuneVariables.Default.Integers);
                //
                if (TuneVariables.Default.Position2Ds != null)
                    lock (TuneVariables.Default.Position2Ds)
                        Serialize(TuneVariables.Default.Position2Ds);
                else
                    Serialize(TuneVariables.Default.Position2Ds);

                if (TuneVariables.Default.Booleans != null)
                    lock (TuneVariables.Default.Booleans)
                        Serialize(TuneVariables.Default.Booleans);
                else
                    Serialize(TuneVariables.Default.Booleans);
                //
                //if (CustomVariables.Vector2Ds != null)
                //    lock (CustomVariables.Vector2Ds)
                //        Serialize(CustomVariables.Vector2Ds);
                //else
                //    Serialize(CustomVariables.Vector2Ds);
            }
        }
        /// <summary>
        /// Deserialize Custom variable
        /// </summary>
        public void DeserializeCustomVariable(bool Wait)
        {
            _waitCustom = true;
            SerializableDictionary<string, double> dic1 = DeserializeDictionaryStringDouble().ToSerializable();
            SerializableDictionary<string, int> dic2 = DeserializeDictionaryStringInt().ToSerializable();
            SerializableDictionary<string, Position2D> dic3 = DeserializeDictionaryStringPosition2D().ToSerializable();
            SerializableDictionary<string, bool> dic4 = DeserializeDictionaryStringbool().ToSerializable();
            // Dictionary<string, Vector2D> dic4 = DeserializeDictionaryStringVector2D();

            if (!Wait)
            {

                TuneVariables.Default.Refresh(dic1, dic2, dic3, dic4);
                //if (dic4 != null)
                //{
                //    if (CustomVariables.Vector2Ds == null)
                //        CustomVariables.Vector2Ds = dic4;
                //    else
                //        foreach (string key in dic4.Keys)
                //        {
                //            if (!CustomVariables.Vector2Ds.ContainsKey(key))
                //                CustomVariables.Vector2Ds.Add(key, dic4[key]);
                //            else
                //                CustomVariables.Vector2Ds[key] = dic4[key];
                //        }
                //}
                TuneVariables.Default.Save();
            }
            _waitCustom = false;
        }
        /// <summary>
        /// Deserialize SSL vision packet wrapper
        /// </summary>
        /// <param name="Strem">memory stream from vision</param>
        /// <returns>ssl vision wrapper</returns>
        public messages_robocup_ssl_wrapper.SSL_WrapperPacket DeserializeSSLVisionPacket(MemoryStream Strem)
        {
            stream = Strem;
            //  messages_robocup_ssl_wrapper.SSL_WrapperPacket visionpacket = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
            return ProtoBuf.Serializer.Deserialize<messages_robocup_ssl_wrapper.SSL_WrapperPacket>(stream);
            // return visionpacket;
        }
        public referee.SSL_Referee DeserializeSSLRefereePacket(MemoryStream Strem)
        {
            stream = Strem;
            //  messages_robocup_ssl_wrapper.SSL_WrapperPacket visionpacket = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
            return ProtoBuf.Serializer.Deserialize<referee.SSL_Referee>(stream);
            // return visionpacket;
        }
        /// <summary>
        /// Serialize AISettings
        /// </summary>
        public void SerializeAISettings()
        {
            //Serialize(AISettings.NetworkSettingChanged);
            //Serialize(AISettings.AllowedPlays);
            //Serialize(AISettings.EngineSettings);
            //Serialize(AISettings.NetworkSettings);
            //Serialize(AISettings.PlayNemes);
            //Serialize(AISettings.RobotSettings);
            //Serialize(AISettings.visPriority);


        }
        /// <summary>
        /// Deserialize AIsettings
        /// </summary>
        public void DeserializeAISettings()
        {
            //AISettings.NetworkSettingChanged = DeserializeBool();

            //Dictionary<int, string> AllowedPlays = DeserializeDictionaryIntString();
            //if (AllowedPlays != null)
            //    AISettings.AllowedPlays = AllowedPlays;

            //Dictionary<int, Engines> EngineSettings = DeserializeDictionaryIntEngines();
            //if (EngineSettings != null)
            //    AISettings.EngineSettings = EngineSettings;

            //NetworkSettings netset = DeserializeNetworkSettings();
            //if (netset != null)
            //    AISettings.NetworkSettings = netset;

            //List<string> paly = DeserializeListString();
            //if (paly != null)
            //    AISettings.PlayNemes = paly;

            //Dictionary<int, int> robotstting = DeSerializeDictionaryIntInt();
            //if (robotstting != null)
            //    AISettings.RobotSettings = robotstting;

            //AISettings.visPriority = DeserializeDribleState();
        }
        /// <summary>
        /// serialize a dribleState array
        /// </summary>
        /// <param name="val">a dribleState array</param>
        private void Serialize(DribleState[] val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize((int)val[0]);
                Serialize((int)val[1]);
                Serialize((int)val[2]);
            }
        }
        /// <summary>
        /// deserilize a DribleState array
        /// </summary>
        /// <returns>a DribleState array</returns>
        private DribleState[] DeserializeDribleState()
        {
            if (DeserializeBool())
            {
                DribleState[] ret = new DribleState[3];
                ret[0] = (DribleState)DeserializeInt();
                ret[1] = (DribleState)DeserializeInt();
                ret[2] = (DribleState)DeserializeInt();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize a dictionary (string , int)
        /// </summary>
        /// <param name="val">a dictionary (string , int)</param>
        private void Serialize(Dictionary<string, int> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                lock (val)
                    foreach (string key in val.Keys)
                    {
                        Serialize(key);
                        Serialize(val[key]);
                    }
            }
        }
        /// <summary>
        /// deserialize a dictionary(string , int)
        /// </summary>
        /// <returns>a dictionary(string , int)</returns>
        private Dictionary<string, int> DeserializeDictionaryStringInt()
        {
            if (DeserializeBool())
            {
                Dictionary<string, int> ret = new Dictionary<string, int>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeInt());
                return ret;
            }
            return null;
        }

        private void Serialize(Score val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.PosScore);
                Serialize(val.Region);
                Serialize(val.Robot);
            }
        }
        private Score DeserializeScore()
        {
            if (DeserializeBool())
            {
                Score ret = new Score();
                ret.PosScore = DeserializeDouble();
                ret.Region = DeserializeInt();
                Position2D p = DeserializePosition2D();
                ret.Robot = new Position2D(p.X , p.Y);
                return ret;
            }
            return null;
        }
        private void Serialize(List<Score> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in val)
                    Serialize(item);
            }
        }
        private List<Score> DeserializeListScore()
        {
            if (DeserializeBool())
            {
                List<Score> ret = new List<Score>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeScore());
                return ret;
            }
            return null;
        }
        public void SerializeRegionScore()
        {
            if (RegionScore.Default.Data == null)
                RegionScore.Default.Data = new List<Score>();
            if (RegionScore.Default.Scores == null)
                RegionScore.Default.Scores = new Dictionary<int, float[,]>();
            Serialize(RegionScore.Default.Data);
            Serialize(RegionScore.Default.Scores);
            Serialize(RegionScore.Default.SigmaX);
            Serialize(RegionScore.Default.SigmaY);
            Serialize(RegionScore.Default.SigmaXt);
            Serialize(RegionScore.Default.SigmaYt);
        }

        public void DeserializeRegionScore()
        {
            RegionScore.Default.Data = new List<Score>();
            RegionScore.Default.Scores = new Dictionary<int, float[,]>();
            RegionScore.Default.Data = DeserializeListScore();
            RegionScore.Default.Scores = DeserializeDictionaryIntArrayFloat();
            RegionScore.Default.SigmaX = DeserializeDouble();
            RegionScore.Default.SigmaY = DeserializeDouble();
            RegionScore.Default.SigmaXt = DeserializeDouble();
            RegionScore.Default.SigmaYt = DeserializeDouble();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(Dictionary<int, float[,]> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                lock (val)
                {
                    int count = val.Count;

                    Serialize(count);
                    foreach (var item in val.ToList())
                    {
                        Serialize(item.Key);
                        Serialize(item.Value);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, float[,]> DeserializeDictionaryIntArrayFloat()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                Dictionary<int, float[,]> ret = new Dictionary<int, float[,]>();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeInt(), DeserializeArrayFloat());

                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(float[,] val)
        {
            int d0 = val.GetLength(0), d1 = val.GetLength(1);
            Serialize(d0);
            Serialize(d1);
            for (int i = 0 ; i < d0 ; i++)
            {
                for (int j = 0 ; j < d1 ; j++)
                {
                    Serialize(val[i, j]);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float[,] DeserializeArrayFloat()
        {
            int d0 = DeserializeInt();
            int d1 = DeserializeInt();
            float[,] ret = new float[d0, d1];
            for (int i = 0 ; i < d0 ; i++)
            {
                for (int j = 0 ; j < d1 ; j++)
                {
                    ret[i, j] = DeserializeFloat();
                }
            }
            return ret;
        }
        /// <summary>
        /// serialize a dictionary (string , Vector2D)
        /// </summary>
        /// <param name="val">a dictionary (string , Vector2D)</param>
        private void Serialize(Dictionary<string, Vector2D> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                lock (val)
                {
                    int count = val.Count;
                    Dictionary<string, Vector2D> temp = new Dictionary<string, Vector2D>();
                    foreach (string key in val.Keys)
                        if (key != null && val[key] != null)
                            temp.Add(key, val[key]);
                    Serialize(temp.Count);
                    foreach (string key in temp.Keys)
                    {
                        Serialize(key);
                        Serialize(temp[key]);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Vector2D> DeserializeDictionaryStringVector2D()
        {
            if (DeserializeBool())
            {
                Dictionary<string, Vector2D> ret = new Dictionary<string, Vector2D>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeVector2D());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serialize all game Parameters
        /// </summary>
        void SerializeGameParameters()
        {

            Serialize(GameParameters.OurGoalCenter);
            Serialize(GameParameters.OurLeftCorner);
            Serialize(GameParameters.OurRightCorner);
            Serialize(GameParameters.OurGoalLeft);
            Serialize(GameParameters.OurGoalRight);

            Serialize(GameParameters.OppGoalCenter);
            Serialize(GameParameters.OppLeftCorner);
            Serialize(GameParameters.OppRightCorner);
            Serialize(GameParameters.OppGoalLeft);
            Serialize(GameParameters.OppGoalRight);

            Serialize(GameParameters.FieldCenterCircleDiameter);
        }
        /// <summary>
        /// deserialize and update game parameters
        /// </summary>
        void DeserializeGameParameters()
        {

            GameParameters.OurGoalCenter = DeserializePosition2D();
            GameParameters.OurLeftCorner = DeserializePosition2D();
            GameParameters.OurRightCorner = DeserializePosition2D();
            GameParameters.OurGoalLeft = DeserializePosition2D();
            GameParameters.OurGoalRight = DeserializePosition2D();

            GameParameters.OppGoalCenter = DeserializePosition2D();
            GameParameters.OppLeftCorner = DeserializePosition2D();
            GameParameters.OppRightCorner = DeserializePosition2D();
            GameParameters.OppGoalLeft = DeserializePosition2D();
            GameParameters.OppGoalRight = DeserializePosition2D();


            GameParameters.FieldCenterCircleDiameter = DeserializeDouble();
        }
        /// <summary>
        /// serialize a dictionary (int , chipkick)
        /// </summary>
        /// <param name="val">a dictionary (int , chipkick)</param>
        //void Serialize(Dictionary<int,ChipKick> val)
        //{
        //    Serialize(val != null);
        //    if (val != null)
        //    {
        //        Serialize(val.Count);
        //        foreach (int key in val.Keys)
        //        {
        //            Serialize(key);
        //            Serialize(val[key].Range);
        //          //  Serialize(val[key].SafeRadius);
        //        }
        //    }
        //}
        /// <summary>
        /// deserialize a dictionary(int , chipkick)
        /// </summary>
        /// <returns></returns>
        //Dictionary<int, ChipKick> DeserializeDictionaryIntChipkick()
        //{
        //    if (DeserializeBool())
        //    {
        //        Dictionary<int, ChipKick> ret = new Dictionary<int, ChipKick>();
        //        int count = DeserializeInt();
        //        for (int i = 0; i < count; i++)
        //            ret.Add(DeserializeInt(), new ChipKick(DeserializeDouble(), DeserializeDouble()));
        //        return ret;
        //    }
        //    return null;
        //}
        /// <summary>
        /// serialize a dictinary (int , double)
        /// </summary>
        /// <param name="val">a dictinary (int , double)</param>
        void Serialize(Dictionary<int, double> val)
        {
            lock (val)
            {
                Serialize(val != null);
                if (val != null)
                {
                    Serialize(val.Count);
                    foreach (int key in val.Keys)
                    {
                        Serialize(key);
                        Serialize(val[key]);
                    }
                }
            }
        }
        /// <summary>
        /// deserialize a dictionary (int,double)
        /// </summary>
        /// <returns>a dictionary (int,double)</returns>
        Dictionary<int, double> DeserializeDictionaryIntDouble()
        {
            if (DeserializeBool())
            {
                Dictionary<int, double> ret = new Dictionary<int, double>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeInt(), DeserializeDouble());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// Serizlie lockupTable
        /// </summary>
        //public void SerializeLoockUpTable()
        //{
        //    if (!_waitLockupTable)
        //    {
        //        Serialize(KickLookUpTable.SpinBackward);
        //        Serialize(KickLookUpTable.KICK);
        //        Serialize(KickLookUpTable.IsSpin);
        //        Serialize(KickLookUpTable.IsDelayed);
        //        Serialize(KickLookUpTable.IsChipKick);
        //        lock (KickLookUpTable.DirectKickTable)
        //            Serialize(KickLookUpTable.DirectKickTable);
        //        lock (KickLookUpTable.ChipKickTable)
        //            Serialize(KickLookUpTable.ChipKickTable);
        //        lock (KickLookUpTable.DirectKickTableSpinKick)
        //            Serialize(KickLookUpTable.DirectKickTableSpinKick);
        //        lock (KickLookUpTable.ChipKickTableSpinKick)
        //            Serialize(KickLookUpTable.ChipKickTableSpinKick);
        //    }
        //}
        /// <summary>
        /// Deserialize lockupTable
        /// </summary>
        //public void DeserializeLoockUpTable(bool wait)
        //{
        //    _waitLockupTable = true;
        //    KickLookUpTable.SpinBackward = DeserializeBool();
        //    KickLookUpTable.KICK = DeserializeInt();
        //    KickLookUpTable.IsSpin = DeserializeBool();
        //    KickLookUpTable.IsDelayed = DeserializeBool();
        //    KickLookUpTable.IsChipKick = DeserializeBool();
        //    Dictionary<int, double> dic1 = DeserializeDictionaryIntDouble();
        //    Dictionary<int, ChipKick> dic2 = DeserializeDictionaryIntChipkick();
        //    Dictionary<int, double> dic3 = DeserializeDictionaryIntDouble();
        //    Dictionary<int, ChipKick> dic4 = DeserializeDictionaryIntChipkick();
        //    if (!wait)
        //    {

        //        if (dic2 != null)
        //            foreach (int item in dic2.Keys)
        //                KickLookUpTable.Add(item, dic2[item]);
        //        if (dic1 != null)
        //            foreach (int item in dic1.Keys)
        //                KickLookUpTable.Add(item, dic1[item]);

        //        if (dic3 != null)
        //            foreach (int item in dic3.Keys)
        //                KickLookUpTable.AddSpinKick(item, dic3[item]);
        //        if (dic4 != null)
        //            foreach (int item in dic4.Keys)
        //                KickLookUpTable.AddSpinKick(item, dic4[item]);

        //    }
        //    _waitLockupTable = false;
        //}
        /// <summary>
        /// serialize an object of simulatorparameters
        /// </summary>
        /// <param name="val">an object of simulator parameters</param>
        public void Serialize(SimulatorParameters val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Model);
                Serialize(val.Commands);
            }

        }
        /// <summary>
        /// serizlie an object of simulatorParameters to a new strem
        /// </summary>
        /// <param name="val">n object of simulatorParameters</param>
        /// <returns>a memory stream</returns>
        public MemoryStream SerializeSimulatorParameters(SimulatorParameters val)
        {
            stream = new MemoryStream();
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Model);
                Serialize(val.Commands);
            }

            return stream;

        }
        /// <summary>
        /// deserialize a simulator parameters 
        /// </summary>
        /// <returns>a simulator parameters</returns>
        public SimulatorParameters DeserilializeSimParameters()
        {
            if (DeserializeBool())
            {
                SimulatorParameters ret = new SimulatorParameters();
                ret.Model = DeserializeWorldModel(stream);
                ret.Commands = DeserializeDictionaryIntSingleWirelssCommand();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize a dictionary (string , string)
        /// </summary>
        /// <param name="val">a dictionary (string , string)</param>
        private void Serialize(Dictionary<string, string> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                lock (val)
                {
                    int count = val.Count;
                    Dictionary<string, string> temp = new Dictionary<string, string>();
                    foreach (string key in val.Keys)
                        if (key != null && val[key] != null)
                            temp.Add(key, val[key]);
                    Serialize(temp.Count);
                    foreach (string key in temp.Keys)
                    {
                        Serialize(key);
                        Serialize(temp[key]);
                    }
                }
            }
        }
        /// <summary>
        /// deserialize a dictionary (string , string )
        /// </summary>
        /// <returns>a dictionary (string , string )</returns>
        private Dictionary<string, string> DeserializeDictionaryStringString()
        {
            if (DeserializeBool())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeString());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// serialize Technuques (dictionart sring , string )
        /// </summary>
        /// <param name="val">dictionart(sring , string)</param>
        public void SerializeTechniques(Dictionary<string, string> val)
        {
            Serialize(val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> DeserializeTechniques()
        {
            return DeserializeDictionaryStringString();
        }
        /// <summary>
        /// Serialize PlaySettings(Tactic)
        /// </summary>
        public void SerializeTactics()
        {
            Serialize(PlaySettings.Name);
            Serialize((int)PlaySettings.OurKickOffMode);
            Serialize((int)PlaySettings.OurCornerKickMode);
            Serialize((int)PlaySettings.OurPenaltyGoaller);
            Serialize((int)PlaySettings.OurIndirectFreeKick);
            Serialize((int)PlaySettings.OurNormalGame);
        }
        /// <summary>
        /// Deserialize PlaySettings(Tactic)
        /// </summary>
        public void DeserializeTactics()
        {
            PlaySettings.Name = DeserializeString();
            PlaySettings.OurKickOffMode = (OurKickOff)DeserializeInt();
            PlaySettings.OurCornerKickMode = (OurCornerKick)DeserializeInt();
            PlaySettings.OurPenaltyGoaller = (OurPenaltyGoaller)DeserializeInt();
            PlaySettings.OurIndirectFreeKick = (OurIndirectFreeKick)DeserializeInt();
            PlaySettings.OurNormalGame = (OurNormalGame)DeserializeInt();
        }
        //private void Serialize(ObjectDraw val)
        //{
        //    if (val.Obj.GetType() == typeof(Line))
        //    {
        //        Serialize((int)DrawingObjectType.Line);
        //        Serialize(val.Obj.As<Line>());
        //    }
        //    else if (val.Obj.GetType() == typeof(Circle))
        //    {
        //        Serialize((int)DrawingObjectType.Circle);
        //        Serialize(val.Obj.As<Circle>());
        //    }
        //    else if (val.Obj.GetType() == typeof(System.Drawing.RectangleF))
        //    {
        //        Serialize((int)DrawingObjectType.Rectangle);
        //        Serialize(val.Obj.As<RectangleF>());
        //    }
        //    else if (val.Obj.GetType() == typeof(DrawCollection))
        //    {
        //        Serialize((int)DrawingObjectType.DrawCollection);
        //        Serialize(val.Obj.As<DrawCollection>());
        //    }
        //    Serialize(val.IsChecked);
        //    Serialize(val.color);

        //}
        //private ObjectDraw DeserializeObjectDraw()
        //{
        //    ObjectDraw ret = new ObjectDraw();
        //    int type = DeserializeInt();
        //    if ((DrawingObjectType)type == DrawingObjectType.Line)
        //        ret.Obj = DeserializeLine();
        //    else if ((DrawingObjectType)type == DrawingObjectType.Circle)
        //        ret.Obj = DeserializeCircle();
        //    else if ((DrawingObjectType)type == DrawingObjectType.Rectangle)
        //        ret.Obj = DeserializeRectangleF();
        //    else if ((DrawingObjectType)type == DrawingObjectType.DrawCollection)
        //        ret.Obj = DeserializeDrawingCollection();
        //    ret.IsChecked = DeserializeBool();
        //    ret.color = DeserializeColor();
        //    return ret;
        //}
        /// <summary>
        /// 
        /// </summary>

        void removeFromDrawing(string key)
        {
            lock (DrawingObjects.drawingObject)
                DrawingObjects.drawingObject.Remove(key);
        }

        public void SerializeDrawingObjects()
        {
            try
            {
                Serialize(DrawingObjects.RobotAngle);
                Serialize(DrawingObjects.TetaPass);
                Serialize(DrawingObjects.TetaShoot);
                Serialize(DrawingObjects.BallSpeed);

                Serialize(DrawingObjects.BallStatuse);
                Serialize(DrawingObjects.Strategy.Name);
                Serialize(DrawingObjects.Strategy.states);
                Serialize(DrawingObjects.Strategy.CurrentState);
                Serialize(DrawingObjects.Strategy.Transitions);

                Serialize(DrawingObjects.drawingObject.Count);
                Dictionary<string, object> copy = new Dictionary<string, object>();

                lock (DrawingObjects.drawingObject)
                    DrawingObjects.drawingObject.ToList().ForEach(p =>
                    {
                        if (p.Key != null && p.Value != null)
                        {
                            if (!copy.ContainsKey(p.Key))
                                copy.Add(p.Key, p.Value);
                        }
                    });

                foreach (var item in copy.Keys)
                {
                    Serialize(item);
                    if (copy[item].GetType() == typeof(DrawRegion3D))
                    {
                        Serialize((int)DrawingObjectType.Region3D);
                        Serialize(copy[item].As<DrawRegion3D>());
                        if (!copy[item].As<DrawRegion3D>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(Line))
                    {
                        Serialize((int)DrawingObjectType.Line);
                        Serialize(copy[item].As<Line>());
                        if (!copy[item].As<Line>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(Circle))
                    {
                        Serialize((int)DrawingObjectType.Circle);
                        Serialize(copy[item].As<Circle>());
                        if (!copy[item].As<Circle>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(StringDraw))
                    {
                        Serialize((int)DrawingObjectType.Text);
                        Serialize(copy[item].As<StringDraw>());
                        if (!copy[item].As<StringDraw>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(SingleObjectState))
                    {
                        Serialize((int)DrawingObjectType.SingleObjectState);
                        Serialize(copy[item].As<SingleObjectState>());
                        if (!copy[item].As<SingleObjectState>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(DrawCollection))
                    {
                        Serialize((int)DrawingObjectType.DrawCollection);
                        Serialize(copy[item].As<DrawCollection>());
                    }
                    else if (copy[item].GetType() == typeof(FlatRectangle))
                    {
                        Serialize((int)DrawingObjectType.Rectangle);
                        Serialize(copy[item].As<FlatRectangle>());
                        if (!copy[item].As<FlatRectangle>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(DrawRegion))
                    {
                        Serialize((int)DrawingObjectType.Region);
                        Serialize(copy[item].As<DrawRegion>());
                        if (!copy[item].As<DrawRegion>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(Position2D))
                    {
                        Serialize((int)DrawingObjectType.Position);
                        Serialize(copy[item].As<Position2D>());
                        if (!copy[item].As<Position2D>().IsShown)
                            removeFromDrawing(item);
                    }
                    else if (copy[item].GetType() == typeof(Position3D))
                    {
                        Serialize((int)DrawingObjectType.Position3D);
                        Serialize(copy[item].As<Position3D>());
                        removeFromDrawing(item);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                //  VisualizerConsole.WriteLine(ex.ToString(), Color.Red);
            }

        }
        /// <summary>
        /// 
        /// </summary>

        public void DeserializeDrawingObjects()
        {
            try
            {
                DrawingObjects.RobotAngle = DeserializeDouble();
                DrawingObjects.TetaPass = DeserializeDouble();
                DrawingObjects.TetaShoot = DeserializeDouble();
                DrawingObjects.BallSpeed = DeserializeDouble();

                DrawingObjects.BallStatuse = DeserializeString();
                DrawingObjects.Strategy.Name = DeserializeString();
                DrawingObjects.Strategy.states = DeserializeListString();
                DrawingObjects.Strategy.CurrentState = DeserializeString();
                DrawingObjects.Strategy.Transitions = DeserializeListString();

                int count = DeserializeInt();
                Dictionary<string, object> copy = new Dictionary<string, object>();
                //lock (DrawingObjects.drawingObject)
                //{
                //    DrawingObjects.drawingObject.ToList().ForEach(p =>
                //    {
                //        if (p.Key != null)
                //            copy[p.Key] = p.Value;
                //    });
                //}
                for (int i = 0 ; i < count ; i++)
                {
                    string key = DeserializeString();
                    int type = DeserializeInt();
                    if (key != null)
                    {

                        if ((DrawingObjectType)type == DrawingObjectType.Line)
                        {
                            Line l = DeserializeLine();
                            //DrawingObjects.AddObject(key, l);
                            copy[key] = l;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.Circle)
                        {
                            Circle cir = DeserializeCircle();
                            //DrawingObjects.AddObject(key, cir);
                            copy[key] = cir;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.Rectangle)
                        {
                            FlatRectangle fl = DeserializeFlatRectangle();
                            //DrawingObjects.AddObject(key, fl);
                            copy[key] = fl;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.SingleObjectState)
                        {
                            SingleObjectState si = DeserializeSingleObjectState();
                            //DrawingObjects.AddObject(key, si);
                            copy[key] = si;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.Text)
                        {
                            StringDraw sd = DeserializeStringDraw();
                            //DrawingObjects.AddObject(key, sd);
                            copy[key] = sd;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.DrawCollection)
                        {
                            DrawCollection dc = DeserializeDrawingCollection();
                            //DrawingObjects.AddObject(key, dc);
                            copy[key] = dc;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.Region)
                        {
                            DrawRegion dr = DeserializeDrawRegion();
                            //DrawingObjects.AddObject(key, dr);
                            copy[key] = dr;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.Region3D)
                        {
                            DrawRegion3D dr = DeserializeDrawRegion3D();
                            //DrawingObjects.AddObject(key, dr);
                            copy[key] = dr;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.Position)
                        {
                            Position2D dr = DeserializePosition2D();
                            //DrawingObjects.AddObject(key, dr);
                            copy[key] = dr;
                        }
                        else if ((DrawingObjectType)type == DrawingObjectType.Position3D)
                        {
                            Position3D dr = DeserializePosition3D();
                            //DrawingObjects.AddObject(key, dr);
                            copy[key] = dr;
                        }
                    }
                }
                lock (DrawingObjects.drawingObject)
                    DrawingObjects.drawingObject = copy;

            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                // VisualizerConsole.WriteLine(ex.ToString(), Color.Red);
            }
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(DrawRegion val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.BorderColor);
                Serialize(val.FillColor);
                Serialize(val.Filled);
                Serialize(val.IsShown);
                Serialize(val.Path);
                Serialize(val.Opacity);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DrawRegion DeserializeDrawRegion()
        {
            if (DeserializeBool())
            {
                DrawRegion ret = new DrawRegion();
                ret.BorderColor = DeserializeColor();
                ret.FillColor = DeserializeColor();
                ret.Filled = DeserializeBool();
                ret.IsShown = DeserializeBool();
                ret.Path = DeserializelistPosition2D();
                ret.Opacity = DeserializeFloat();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(DrawRegion3D val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.BorderColor);
                Serialize(val.FillColor);
                Serialize(val.Filled);
                Serialize(val.IsShown);
                Serialize(val.Path);
                Serialize(val.Opacity);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DrawRegion3D DeserializeDrawRegion3D()
        {
            if (DeserializeBool())
            {
                DrawRegion3D ret = new DrawRegion3D();
                ret.BorderColor = DeserializeColor();
                ret.FillColor = DeserializeColor();
                ret.Filled = DeserializeBool();
                ret.IsShown = DeserializeBool();
                ret.Path = DeserializelistPosition3D();
                ret.Opacity = DeserializeFloat();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public LoggerDrawingObject DeserializeDrawingObjectLog()
        {



            LoggerDrawingObject ldo = new LoggerDrawingObject();
            DeserializeDouble();
            DeserializeDouble();
            DeserializeDouble();
            DeserializeDouble();
            ldo.BallStatus = DeserializeString();
            DeserializeString();
            DeserializeListString();
            DeserializeString();
            DeserializeListString();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
            {
                string key = DeserializeString();
                if (key != null)
                {
                    int type = DeserializeInt();
                    if ((DrawingObjectType)type == DrawingObjectType.Line)
                        ldo.drawingObject[key] = DeserializeLine();
                    else if ((DrawingObjectType)type == DrawingObjectType.Circle)
                        ldo.drawingObject[key] = DeserializeCircle();
                    else if ((DrawingObjectType)type == DrawingObjectType.Rectangle)
                        ldo.drawingObject[key] = DeserializeFlatRectangle();
                    else if ((DrawingObjectType)type == DrawingObjectType.SingleObjectState)
                        ldo.drawingObject[key] = DeserializeSingleObjectState();
                    else if ((DrawingObjectType)type == DrawingObjectType.Text)
                        ldo.drawingObject[key] = DeserializeStringDraw();
                    else if ((DrawingObjectType)type == DrawingObjectType.DrawCollection)
                        ldo.drawingObject[key] = DeserializeDrawingCollection();
                    else if ((DrawingObjectType)type == DrawingObjectType.Region)
                        ldo.drawingObject[key] = DeserializeDrawRegion();
                    else if ((DrawingObjectType)type == DrawingObjectType.Position)
                        ldo.drawingObject[key] = DeserializePosition2D();

                }
            }
            return ldo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(DrawCollection val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.drawingObject.Count);
                Dictionary<string, object> copy = new Dictionary<string, object>();
                val.drawingObject.ToList().ForEach(p => copy.Add(p.Key, p.Value));
                foreach (var item in copy.Keys)
                {
                    Serialize(item);
                    if (copy[item].GetType() == typeof(Line))
                    {
                        Serialize((int)DrawingObjectType.Line);
                        Serialize(copy[item].As<Line>());
                        if (!copy[item].As<Line>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (copy[item].GetType() == typeof(DrawRegion3D))
                    {
                        Serialize((int)DrawingObjectType.Region3D);
                        Serialize(copy[item].As<DrawRegion3D>());
                        if (!copy[item].As<DrawRegion3D>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (val.drawingObject[item].GetType() == typeof(Circle))
                    {
                        Serialize((int)DrawingObjectType.Circle);
                        Serialize(copy[item].As<Circle>());
                        if (!copy[item].As<Circle>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (copy[item].GetType() == typeof(FlatRectangle))
                    {
                        Serialize((int)DrawingObjectType.Rectangle);
                        Serialize(copy[item].As<FlatRectangle>());
                        if (!copy[item].As<FlatRectangle>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (copy[item].GetType() == typeof(SingleObjectState))
                    {
                        Serialize((int)DrawingObjectType.SingleObjectState);
                        Serialize(copy[item].As<SingleObjectState>());
                        if (!copy[item].As<SingleObjectState>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (copy[item].GetType() == typeof(StringDraw))
                    {
                        Serialize((int)DrawingObjectType.Text);
                        Serialize(copy[item].As<StringDraw>());
                        if (!copy[item].As<StringDraw>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (copy[item].GetType() == typeof(DrawCollection))
                    {
                        Serialize((int)DrawingObjectType.DrawCollection);
                        Serialize(copy[item].As<DrawCollection>());
                    }
                    else if (copy[item].GetType() == typeof(DrawRegion))
                    {
                        Serialize((int)DrawingObjectType.Region);
                        Serialize(copy[item].As<DrawRegion>());
                        if (!copy[item].As<DrawRegion>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (copy[item].GetType() == typeof(Position2D))
                    {
                        Serialize((int)DrawingObjectType.Position);
                        Serialize(copy[item].As<Position2D>());
                        if (!copy[item].As<Position2D>().IsShown)
                            val.drawingObject.Remove(item);
                    }
                    else if (copy[item].GetType() == typeof(Position3D))
                    {
                        Serialize((int)DrawingObjectType.Position3D);
                        Serialize(copy[item].As<Position3D>());
                        val.drawingObject.Remove(item);
                    }
                }
            }
        }
        private void Serialize(DrawCollection val, bool isStrategy)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.drawingObject.Count);
                Dictionary<string, object> copy = new Dictionary<string, object>();
                val.drawingObject.ToList().ForEach(p => copy.Add(p.Key, p.Value));
                foreach (var item in copy.Keys)
                {
                    Serialize(item);
                    if (copy[item].GetType() == typeof(Line))
                    {
                        Serialize((int)DrawingObjectType.Line);
                        Serialize(copy[item].As<Line>());

                    }
                    else if (copy[item].GetType() == typeof(DrawRegion3D))
                    {
                        Serialize((int)DrawingObjectType.Region3D);
                        Serialize(copy[item].As<DrawRegion3D>());

                    }
                    else if (val.drawingObject[item].GetType() == typeof(Circle))
                    {
                        Serialize((int)DrawingObjectType.Circle);
                        Serialize(copy[item].As<Circle>());

                    }
                    else if (copy[item].GetType() == typeof(FlatRectangle))
                    {
                        Serialize((int)DrawingObjectType.Rectangle);
                        Serialize(copy[item].As<FlatRectangle>());

                    }
                    else if (copy[item].GetType() == typeof(SingleObjectState))
                    {
                        Serialize((int)DrawingObjectType.SingleObjectState);
                        Serialize(copy[item].As<SingleObjectState>());

                    }
                    else if (copy[item].GetType() == typeof(StringDraw))
                    {
                        Serialize((int)DrawingObjectType.Text);
                        Serialize(copy[item].As<StringDraw>());

                    }
                    else if (copy[item].GetType() == typeof(DrawCollection))
                    {
                        Serialize((int)DrawingObjectType.DrawCollection);
                        Serialize(copy[item].As<DrawCollection>());
                    }
                    else if (copy[item].GetType() == typeof(DrawRegion))
                    {
                        Serialize((int)DrawingObjectType.Region);
                        Serialize(copy[item].As<DrawRegion>());

                    }
                    else if (copy[item].GetType() == typeof(Position2D))
                    {
                        Serialize((int)DrawingObjectType.Position);
                        Serialize(copy[item].As<Position2D>());

                    }
                    else if (copy[item].GetType() == typeof(Position3D))
                    {
                        Serialize((int)DrawingObjectType.Position3D);
                        Serialize(copy[item].As<Position3D>());

                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DrawCollection DeserializeDrawingCollection()
        {
            DrawCollection ret = new DrawCollection();
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                {
                    string key = DeserializeString();
                    int type = DeserializeInt();
                    //ret.AddObject(DeserializeObjectDraw(), key);
                    if (key != null)
                    {
                        if ((DrawingObjectType)type == DrawingObjectType.Line)
                            ret.drawingObject[key] = DeserializeLine();
                        else if ((DrawingObjectType)type == DrawingObjectType.Circle)
                            ret.drawingObject[key] = DeserializeCircle();
                        else if ((DrawingObjectType)type == DrawingObjectType.Rectangle)
                            ret.drawingObject[key] = DeserializeFlatRectangle();
                        else if ((DrawingObjectType)type == DrawingObjectType.SingleObjectState)
                            ret.drawingObject[key] = DeserializeSingleObjectState();
                        else if ((DrawingObjectType)type == DrawingObjectType.Text)
                            ret.drawingObject[key] = DeserializeStringDraw();
                        else if ((DrawingObjectType)type == DrawingObjectType.DrawCollection)
                            ret.drawingObject[key] = DeserializeDrawingCollection();
                        else if ((DrawingObjectType)type == DrawingObjectType.Region)
                            ret.drawingObject[key] = DeserializeDrawRegion();
                        else if ((DrawingObjectType)type == DrawingObjectType.Region3D)
                            ret.drawingObject[key] = DeserializeDrawRegion3D();
                        else if ((DrawingObjectType)type == DrawingObjectType.Position)
                            ret.drawingObject[key] = DeserializePosition2D();
                        else if ((DrawingObjectType)type == DrawingObjectType.Position3D)
                            ret.drawingObject[key] = DeserializePosition3D();
                    }
                }
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(Series val)
        {
            Serialize(val.Name);
            Serialize(val.ChartArea);
            Serialize(val.Color);
            Serialize((int)val.ChartType);
            Serialize(val.MarkerBorderColor);
            Serialize(val.MarkerBorderWidth);
            Serialize(val.MarkerSize);
            Serialize((int)val.MarkerStyle);
            Serialize(val.ToolTip);
            Serialize(val.MarkerColor);
            Serialize(val.Points.Count);
            foreach (var item in val.Points)
                Serialize(item.YValues[0]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Series DeserializeSeries()
        {
            System.Windows.Forms.DataVisualization.Charting.Series ret = new System.Windows.Forms.DataVisualization.Charting.Series(DeserializeString())
            {
                ChartArea = DeserializeString(),
                Color = DeserializeColor()
            };
            ret.ChartType = (SeriesChartType)DeserializeInt();
            ret.MarkerBorderColor = DeserializeColor();
            ret.MarkerBorderWidth = DeserializeInt();
            ret.MarkerSize = DeserializeInt();
            ret.MarkerStyle = (MarkerStyle)DeserializeInt();
            ret.ToolTip = DeserializeString();
            ret.MarkerColor = DeserializeColor();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
                ret.Points.AddY(DeserializeDouble());
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chartarea"></param>
        private void Serialize(ChartArea chartarea)
        {
            Serialize(chartarea.Name);
            Serialize(chartarea.Area3DStyle.Enable3D);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ChartArea DeserializeChartArea()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea ret = new System.Windows.Forms.DataVisualization.Charting.ChartArea(DeserializeString());
            ret.Area3DStyle.Enable3D = DeserializeBool();
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(List<Series> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in val)
                    Serialize(item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<Series> DeserializeListSeries()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<System.Windows.Forms.DataVisualization.Charting.Series> ret = new List<System.Windows.Forms.DataVisualization.Charting.Series>();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeSeries());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(List<ChartArea> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in val)
                    Serialize(item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(ChartDataInfo val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Time);
                Serialize(val.Value);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ChartDataInfo DeserializeChartDataInfo()
        {
            if (DeserializeBool())
            {
                ChartDataInfo ch = new ChartDataInfo();
                ch.Time = DeserializeInt();//DeserializeDateTime();
                ch.Value = DeserializeDouble();
                return ch;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(List<ChartDataInfo> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in val)
                    Serialize(item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<ChartDataInfo> DeserializeListChartDataInfo()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<ChartDataInfo> ret = new List<ChartDataInfo>();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeChartDataInfo());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<ChartArea> DeserializeListChartArea()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<System.Windows.Forms.DataVisualization.Charting.ChartArea> ret = new List<System.Windows.Forms.DataVisualization.Charting.ChartArea>();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeChartArea());
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void Serialize(List<double> value)
        {
            Serialize(value != null);
            if (value != null)
            {
                Serialize(value.Count);
                foreach (var item in value)
                    Serialize(item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<double> DeserializeListDouble()
        {
            if (DeserializeBool())
            {
                List<double> ret = new List<double>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeDouble());
                return ret;
            }
            return null;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void Serialize(ChartItem value)
        {
            Serialize(value != null);
            if (value != null)
            {
                Serialize(value.Name);
                Serialize(value.Points);
                Serialize(value.Color);
                Serialize(value.BorderWhidth);
                Serialize((int)value.Xtype);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ChartItem DeserializeChartItem()
        {
            if (DeserializeBool())
            {
                ChartItem ch = new ChartItem();
                ch.Name = DeserializeString();
                ch.Points = DeserializeListChartDataInfo();
                ch.Color = DeserializeColor();
                ch.BorderWhidth = DeserializeDouble();
                ch.Xtype = (ChartItem.XType)DeserializeInt();
                return ch;
            }
            return null;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void Serialize(Dictionary<string, ChartItem> value)
        {
            Serialize(value != null && value.Count > 0);
            if (value != null && value.Count > 0)
            {
                Serialize(value.Count);
                foreach (var item in value)
                {
                    Serialize(item.Key);
                    Serialize(item.Value);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, ChartItem> DeserializeDictionaryStringChartItem()
        {
            if (DeserializeBool())
            {
                Dictionary<string, ChartItem> dic = new Dictionary<string, ChartItem>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                {
                    string key = DeserializeString();
                    ChartItem ci = DeserializeChartItem();
                    if (key != null && ci != null)
                        dic.Add(key, ci);
                }
                return dic;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        public void SerializeChart()
        {
            Serialize(CharterData.Series);
            //Serialize(CharterData.series);
            //CharterData.ChartAreas.Clear();
            CharterData.Series.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        public void DeserializeChart(bool addTodata)
        {
            if (CharterData.Series == null)
                CharterData.Series = new Dictionary<string, ChartItem>();
            lock (CharterData.Series)
            {
                bool ne = false;
                List<string> newseries = new List<string>();
                Dictionary<string, ChartItem> ch = DeserializeDictionaryStringChartItem();
                if (ch != null)
                {
                    if (addTodata)
                    {
                        foreach (var item in ch)
                        {
                            if (CharterData.Series.ContainsKey(item.Key))
                                foreach (var item2 in item.Value.Points)
                                    CharterData.Series[item.Key].Points.Add(item2);
                            else
                            {
                                CharterData.AddSeries(item.Key, item.Value);
                                newseries.Add(item.Key);
                                ne = true;
                            }
                            if (CharterData.Series[item.Key].Points.Count > 1000)
                                CharterData.Series[item.Key].Points.RemoveRange(0, CharterData.Series[item.Key].Points.Count - 1000);
                        }
                        if (ne && SerieChanged != null)
                            SerieChanged(newseries);
                    }
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addTodata"></param>
        public LogCharterData DeserializeLogChart(bool addTodata)
        {
            LogCharterData lcd = new LogCharterData();
            if (lcd.Series == null)
                lcd.Series = new Dictionary<string, ChartItem>();
            lock (lcd.Series)
            {
                List<string> newseries = new List<string>();
                Dictionary<string, ChartItem> ch = DeserializeDictionaryStringChartItem();
                if (addTodata)
                {
                    foreach (var item in ch)
                    {
                        if (lcd.Series.ContainsKey(item.Key))
                            foreach (var item2 in item.Value.Points)
                                lcd.Series[item.Key].Points.Add(item2);
                        else
                        {
                            lcd.AddSeries(item.Key, item.Value);
                            newseries.Add(item.Key);
                        }
                        if (lcd.Series[item.Key].Points.Count > 1000)
                            lcd.Series[item.Key].Points.RemoveRange(0, lcd.Series[item.Key].Points.Count - 1000);
                    }

                }
            }
            return lcd;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void SerializeAiToVisWrapper(AiToVisualizerWrapper val)
        {
            try
            {
                stream = new MemoryStream();
                if (val.StrategySended)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Strategies);
                    Serialize(StrategyInfo.Strategies);
                }
                if (val.Model != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Model);
                    Serialize(val.Model);
                }
                if (val.GlobalModel != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.GlobalModel);
                    Serialize(val.GlobalModel);
                }
                if (val.AllBalls != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Balls);
                    Serialize(val.AllBalls);
                }
                if (val.Techniques != null && val.Techniques.Count > 0)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Techniques);
                    Serialize(val.Techniques);
                }
                if (val.Engines != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Engines);
                    Serialize(val.Engines);
                }
                if (VisualizerConsole.Data != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Console);
                    SerializeVisualizerConsole();
                    VisualizerConsole.Data.Clear();
                }
                if (val.RobotCommnd != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.RobotCommand);
                    Serialize(val.RobotCommnd);
                }
                if (val.SendTuneVariables)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Customvariables);
                    SerializeCustomVariable();
                }
                if (val.GameParametersSend)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.GameParameters);
                    SerializeGameParameters();
                }
                if (val.BallStatus != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.BallStatus);
                    Serialize(val.BallStatus);
                }
                if (val.MtrixData != null)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.RobotData);
                    Serialize(val.MtrixData);
                }
                if (ActiveRoleSettings.Default != null && ActiveRoleSettings.Default.Parameters != null && val.SendActiveSetting)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.ActiveSetting);
                    Serialize(ActiveRoleSettings.Default.Parameters);
                    Serialize(ActiveRoleSettings.Default.ConfilictPolicy);
                    Serialize(ActiveRoleSettings.Default.KickPolicy);
                    Serialize(ActiveRoleSettings.Default.MainPolicy);
                }
                if (val.StrategySended)
                {
                    Serialize((int)StreamControlCode.Continue);
                    Serialize((int)SerializedDataType.Strategies);
                    Serialize(StrategyInfo.Strategies);
                }
                Serialize((int)StreamControlCode.End);
                Serialize(val.StrategySended);
                SerializeDrawingObjects();
                SerializeChart();
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                // VisualizerConsole.WriteLine(ex.ToString(), Color.Red);
            }
        }





        private void Serialize(SerializableDictionary<int, ActiveRoleSettingTemplate> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in val)
                {
                    Serialize(item.Key);
                    Serialize(item.Value.propeties.Count);
                    foreach (var item2 in item.Value.propeties)
                    {
                        Serialize(item2.Key);
                        Serialize(item2.Value);
                    }
                }
            }
        }

        private void DeserializeSerializableIntActiveRole(bool apply)
        {
            if (DeserializeBool())
            {
                if (apply)
                    ActiveRoleSettings.Default.Parameters = new SerializableDictionary<int, ActiveRoleSettingTemplate>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                {
                    int id = DeserializeInt();
                    ActiveRoleSettings.Default.Parameters[id] = new ActiveRoleSettingTemplate();
                    int count2 = DeserializeInt();
                    for (int j = 0 ; j < count2 ; j++)
                    {
                        string key = DeserializeString();
                        double val = DeserializeDouble();
                        if (apply)
                            ActiveRoleSettings.Default.Parameters[id].propeties[key] = val;

                    }
                    if (apply)
                        ActiveRoleSettings.Default.Save();
                }
            }
            ActiveRoleSettings.Default.KickPolicy = DeserializeInt();
            ActiveRoleSettings.Default.MainPolicy = DeserializeInt();
            ActiveRoleSettings.Default.ConfilictPolicy = DeserializeInt();


            ActiveRoleSettings.Default.DangerAreaX = DeserializeDouble();
            ActiveRoleSettings.Default.DangerZoneDist = DeserializeDouble();
            ActiveRoleSettings.Default.KickPower = DeserializeDouble();
            ActiveRoleSettings.Default.MinGoodness = DeserializeDouble();
            ActiveRoleSettings.Default.KickAwayInField = DeserializeBool();
            ActiveRoleSettings.Default.Chip = DeserializeBool();
            ActiveRoleSettings.Default.IsSpinBackWard = DeserializeBool();
            ActiveRoleSettings.Default.Save();
        }

        private void Serialize(RobotData robotData)
        {
            Serialize(robotData != null);
            if (robotData == null)
                return;
            Serialize(robotData.vx);
            Serialize(robotData.vy);
            Serialize(robotData.w);
        }
        RobotData DeserializeRobotData()
        {
            if (DeserializeBool())
            {
                RobotData ret = new RobotData();
                ret.vx = DeserializeDouble();
                ret.vy = DeserializeDouble();
                ret.w = DeserializeDouble();
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public AiToVisualizerWrapper DeserializeAiToVisWrapper(MemoryStream data, bool chartdataUpdate)
        {
            try
            {
                stream = data;
                AiToVisualizerWrapper ret = new AiToVisualizerWrapper();
                while (DeserializeInt().As<StreamControlCode>() == StreamControlCode.Continue)
                {
                    SerializedDataType type = DeserializeInt().As<SerializedDataType>();
                    if (type == SerializedDataType.Model)
                        ret.Model = DeserializeWorldModel(stream);
                    if (type == SerializedDataType.GlobalModel)
                        ret.GlobalModel = DeserializeWorldModel(stream);
                    else if (type == SerializedDataType.Balls)
                        ret.AllBalls = DeserializeDictionaryIntPosiotion2D();
                    else if (type == SerializedDataType.Techniques)
                        ret.Techniques = DeserializeDictionaryStringString();
                    else if (type == SerializedDataType.Engines)
                        ret.Engines = DeserializeDictionaryIntEngines();
                    else if (type == SerializedDataType.Console)
                        DeserializeVisualizerConsole();
                    else if (type == SerializedDataType.RobotCommand)
                    {
                        Dictionary<int, SingleWirelessCommand> des = DeserializeDictionaryIntSingleWirelssCommand();
                        if (des == null)
                            ret.RobotCommnd = null;
                        else
                        {
                            ret.RobotCommnd = new Dictionary<int, SingleWirelessCommand>();
                            des.ToList().ForEach(p => ret.RobotCommnd.Add(p.Key, p.Value));
                        }
                    }
                    else if (type == SerializedDataType.Customvariables)
                        DeserializeCustomVariable(false);
                    else if (type == SerializedDataType.GameParameters)
                        DeserializeGameParameters();
                    else if (type == SerializedDataType.BallStatus)
                        ret.BallStatus = DeserializeString();
                    else if (type == SerializedDataType.RobotData)
                        ret.MtrixData = DeserializeRobotData();
                    else if (type == SerializedDataType.ActiveSetting)
                        DeserializeActiveParameters();
                    else if (type == SerializedDataType.Strategies)
                    {
                        var r = DeserializeStrategies(stream);
                        r.ToList().ForEach(p =>
                        {
                            if (!StrategyInfo.Strategies.ContainsKey(p.Key))
                                StrategyInfo.Strategies[p.Key] = p.Value;
                        });

                        StrategyInfo.Save();
                    }
                    else if (type == SerializedDataType.RotateParameter)
                        DeserializeRotateParameter();
                    //else if (type == SerializedDataType.PassShootTune)
                    //    DeserializePassShootParameter();
                }
                ret.StrategySended = DeserializeBool();

                DeserializeDrawingObjects();
                DeserializeChart(chartdataUpdate);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                return null;
                // VisualizerConsole.WriteLine(ex.ToString(), Color.Red);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void SerializeVisToAiWrapper(VisualizerToAiWrapper val)
        {
            stream = new MemoryStream();
            Serialize(val.RequestTable);
            Serialize(val.GoalieChanged);
            if (val.GoalieChanged)
                Serialize(ControlParameters.GoalieID);
            if (val.SendData.Contains("MergerTracker"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.MergerSetting);
                Serialize(val.MergerTracker);
            }
            if (val.SendData.Contains("CustomVariable"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Customvariables);
                SerializeCustomVariable();
                TuneVariables.Default.Save();
            }
            if (val.SendData.Contains("LockupTable"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.LockupTable);
                SerializeLoockUpTable();
                LookupTable.Save();
            }
            if (val.SendData.Contains("Techniques"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Techniques);
                Serialize(val.Techniques);
            }
            if (val.SendData.Contains("Strategy"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Strategies);
                Serialize(StrategyInfo.Strategies);
                StrategyInfo.Save();
            }
            if (val.SendData.Contains("BallIndex"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.BallIndex);
                Serialize(val.SelectedBallIndex.Value);
                Serialize(val.SelectedBallLoc);
            }
            if (val.SendData.Contains("RefreeCommand"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Refree);
                Serialize(val.RefreeCommand);
            }
            if (val.SendData.Contains("Engines"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Engines);
                Serialize(val.Engine);
            }
            if (val.SendData.Contains("SendDevice"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.SendDevice);
                Serialize((int)val.SenderDevice);
            }
            if (val.SendData.Contains("Model"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Model);
                Serialize(val.Model);
            }
            if (val.SendData.Contains("RecieveMode"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.RecieveMode);
                Serialize((int)val.RecieveMode);
            }
            if (val.SendData.Contains("MovedObj"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.SingleObjectSate);
                Serialize(val.SelectedToMove);
                Serialize(val.SelectedID);
            }
            if (val.SendData.Contains("Tactic"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Tactic);
                Serialize(GameSettings.Default.Tactic);
                GameSettings.Default.Save();
            }
            if (val.SendData.Contains("BatteryRequest"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.BatteryFlag);
                Serialize(val.BatteryRequest);
                Serialize(val.RobotID);
            }
            if (val.SendData.Contains("SenderDevice"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.SenderDevice);
                Serialize((int)val.SenderDevice);
            }
            if (val.SendData.Contains("Command"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.ComponentsCommand);
                Serialize(RobotComponentsController.RobotCommands);
            }
            if (val.SendData.Contains("MoveRobot"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.MoveRobot);
                Serialize(RobotComponentsController.Angle);
                Serialize(RobotComponentsController.SelectedID);
                Serialize(RobotComponentsController.Target);
            }
            if (val.SendData.Contains("Bool"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Bool);
                Serialize(val.WithMatrix);
            }
            if (val.SendData.Contains("Robots"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Robot);
                Serialize(val.RobotList);
            }
            if (val.SendData.Contains("AngleError"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.AngleError);
                Serialize(TuneVariables.Default.AngleError);
            }
            if (val.SendData.Contains("Matrixs"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.OPMatrix);
                Serialize(val.OPMatrix);
            }
            if (val.SendData.Contains("ActiveSettings"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.ActiveSetting);
                //Serialize(ActiveRoleSettings.Default.Parameters);
                //Serialize(ActiveRoleSettings.Default.KickPolicy);
                //Serialize(ActiveRoleSettings.Default.MainPolicy);
                //Serialize(ActiveRoleSettings.Default.ConfilictPolicy);

                //Serialize(ActiveRoleSettings.Default.DangerAreaX);
                //Serialize(ActiveRoleSettings.Default.DangerZoneDist);
                //Serialize(ActiveRoleSettings.Default.KickPower);
                //Serialize(ActiveRoleSettings.Default.MinGoodness);
                //Serialize(ActiveRoleSettings.Default.KickAwayInField);
                //Serialize(ActiveRoleSettings.Default.Chip);
                //Serialize(ActiveRoleSettings.Default.IsSpinBackWard);
                SerializeActiveParameters();
            }

            if (val.SendData.Contains("RotateSetting"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.RotateParameter);
                SerializeRotateParameter();
            }

            if (val.SendData.Contains("PassShootTuneTool"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.PassShootTune);
                SerializePassShootParameter();
            }

            if (val.SendData.Contains("Sensors"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.Sensore);
                Serialize(val.SensoreState);
            }
            if (val.SendData.Contains("ControlParameters"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.ControlData);
                SerializeControlParameters();
            }
            if (val.SendData.Contains("FourCamMergerSetting"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.FourCamMergerSetting);
                Serialize4CamMerger();
            }
            if (val.SendData.Contains("ballPlacementPos"))
            {
                Serialize((int)StreamControlCode.Continue);
                Serialize((int)SerializedDataType.ballPlacementPos);
                SerializeBallPlacementPos();
            }
            Serialize((int)StreamControlCode.Continue);
            Serialize((int)SerializedDataType.SendedDataList);
            Serialize(val.SendData);
            Serialize((int)StreamControlCode.End);

        }

        private void Serialize(Dictionary<int, bool> dictionary)
        {
            Serialize(dictionary != null);
            if (dictionary != null)
            {
                Serialize(dictionary.Count);
                foreach (var item in dictionary)
                {
                    Serialize(item.Key);
                    Serialize(item.Value);
                }
            }

        }

        Dictionary<int, bool> DeserializeDictionaryIntBool()
        {
            Dictionary<int, bool> ret = null;
            if (DeserializeBool())
            {
                ret = new Dictionary<int, bool>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeInt(), DeserializeBool());
                return ret;
            }
            return null;
        }


        private void Serialize(OptimizationMatrix optimizationMatrix)
        {

            for (int i = 0 ; i < optimizationMatrix.E.Rows ; i++)
                for (int j = 0 ; j < optimizationMatrix.E.Cols ; j++)
                    Serialize(optimizationMatrix.E[i, j]);

            for (int i = 0 ; i < optimizationMatrix.D.Rows ; i++)
                for (int j = 0 ; j < optimizationMatrix.D.Cols ; j++)
                    Serialize(optimizationMatrix.D[i, j]);
        }

        private OptimizationMatrix DeserializeOPMatrix()
        {
            OptimizationMatrix ret = new OptimizationMatrix();
            ret.Identitymatrix();
            for (int i = 0 ; i < 2 ; i++)
                for (int j = 0 ; j < 3 ; j++)
                    ret.E[i, j] = DeserializeDouble();

            for (int j = 0 ; j < 3 ; j++)
                ret.D[0, j] = DeserializeDouble();
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        private void SerializeLoockUpTable()
        {
            //if (LookUpTable.Default.DirectKick == null)
            //    LookUpTable.Default.DirectKick = new SerializableDictionary<string, double>();
            if (LookupTable.Data == null)
                LookupTable.Data = new SerializableDictionary<int, MetricChipKick>();
            //Serialize(LookUpTable.Default.DirectKick);
            Serialize(LookupTable.Data);

        }

        void Serialize(ChipKickInfo val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.HasSpin);
                Serialize(val.Length);
                Serialize(val.Power);
                Serialize(val.SafeRadi);
                Serialize(val.Time);
            }
        }

        ChipKickInfo DeserializeOldChipKick()
        {
            ChipKickInfo ock = null;
            if (DeserializeBool())
            {
                ock = new ChipKickInfo();
                ock.HasSpin = DeserializeBool();
                ock.Length = DeserializeDouble();
                ock.Power = DeserializeInt();
                ock.SafeRadi = DeserializeDouble();
                ock.Time = DeserializeDouble();
            }
            return ock;
        }

        void Serialize(MetricChipKick val)
        {
            Serialize(val.KickInfo.Count);
            foreach (var item in val.KickInfo)
                Serialize(item);
        }

        MetricChipKick DeserializeMetricChipKick()
        {
            int count = DeserializeInt();
            MetricChipKick m = new MetricChipKick();
            for (int i = 0 ; i < count ; i++)
                m.KickInfo.Add(DeserializeOldChipKick());
            return m;
        }

        void Serialize(Dictionary<int, MetricChipKick> val)
        {
            Serialize(val.Count);
            foreach (var item in val)
            {
                Serialize(item.Key);
                Serialize(item.Value);
            }
        }

        Dictionary<int, MetricChipKick> DeserializeDictionaryIntMetricChipKick()
        {
            Dictionary<int, MetricChipKick> ret = new Dictionary<int, MetricChipKick>();
            int count = DeserializeInt();
            for (int i = 0 ; i < count ; i++)
            {

                ret.Add(DeserializeInt(), DeserializeMetricChipKick());
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public VisualizerToAiWrapper DeserializeVisToAiWrapper(MemoryStream data)
        {
            stream = data;
            VisualizerToAiWrapper ret = new VisualizerToAiWrapper();
            ret.RequestTable = DeserializeDictionaryStringbool();
            ret.GoalieChanged = DeserializeBool();
            if (ret.GoalieChanged)
                ControlParameters.GoalieID = DeserializeInt();
            while (DeserializeInt().As<StreamControlCode>() == StreamControlCode.Continue)
            {
                SerializedDataType type = DeserializeInt().As<SerializedDataType>();
                if (type == SerializedDataType.Strategies)
                {
                    var str = DeserializeStrategies(stream);
                    StrategyInfo.Strategies = str;
                }
                if (type == SerializedDataType.Customvariables)
                {
                    DeserializeCustomVariable(false);
                    TuneVariables.Default.Save();
                }
                else if (type == SerializedDataType.LockupTable)
                {
                    DeserializeLookUpTable(false);
                    LookUpTable.Default.Initialize();
                }
                else if (type == SerializedDataType.Techniques)
                {
                    ret.Techniques = DeserializeDictionaryStringString();
                }

                else if (type == SerializedDataType.BallIndex)
                {
                    ret.SelectedBallIndex = DeserializeInt();
                    ret.SelectedBallLoc = DeserializePosition2D();
                }
                else if (type == SerializedDataType.Refree)
                {
                    ret.RefreeCommand = DeserializeString();
                }
                else if (type == SerializedDataType.SendedDataList)
                {
                    ret.SendData = DeserializeListString();
                }
                else if (type == SerializedDataType.MergerSetting)
                {
                    ret.MergerTracker = DeserializeMergerTrackerSettings();
                }
                else if (type == SerializedDataType.Engines)
                {
                    ret.Engine = DeserializeDictionaryIntEngines();
                }
                else if (type == SerializedDataType.SendDevice)
                {
                    ret.SenderDevice = (WirelessSenderDevice)DeserializeInt();
                }
                else if ((type == SerializedDataType.Model))
                {
                    ret.Model = DeserializeWorldModel(this.stream);
                }
                else if (type == SerializedDataType.RecieveMode)
                {
                    ret.RecieveMode = (ModelRecieveMode)DeserializeInt();
                }
                else if (type == SerializedDataType.SingleObjectSate)
                {
                    ret.SelectedToMove = DeserializeSingleObjectState();
                    ret.SelectedID = DeserializeNullableInt();
                }
                else if (type == SerializedDataType.Tactic)
                {
                    Dictionary<string, int> des = DeserializeDictionaryStringInt();
                    GameSettings.Default.Tactic = new SerializableDictionary<string, int>();
                    foreach (var item in des)
                        GameSettings.Default.Tactic.Add(item.Key, item.Value);
                    GameSettings.Default.Save();
                }
                else if (type == SerializedDataType.BatteryFlag)
                {
                    ret.BatteryRequest = DeserializeBool();
                    ret.RobotID = DeserializeInt();
                }
                else if (type == SerializedDataType.SenderDevice)
                {
                    ret.SenderDevice = (WirelessSenderDevice)DeserializeInt();
                }
                else if (type == SerializedDataType.ComponentsCommand)
                {
                    Dictionary<int, SingleWirelessCommand> des = DeserializeDictionaryIntSingleWirelssCommand();
                    RobotComponentsController.RobotCommands = new Dictionary<int, SingleWirelessCommand>();
                    if (des != null)
                        des.ToList().ForEach(p => RobotComponentsController.RobotCommands.Add(p.Key, p.Value));

                }
                else if (type == SerializedDataType.MoveRobot)
                {
                    RobotComponentsController.Angle = DeserializeDouble();
                    RobotComponentsController.SelectedID = DeserializeNullableInt();
                    RobotComponentsController.Target = DeserializePosition2D();
                }
                else if (type == SerializedDataType.Bool)
                {
                    ret.WithMatrix = DeserializeBool();
                }
                else if (type == SerializedDataType.Robot)
                {
                    ret.RobotList = DeserializeListRobots();
                }
                else if (type == SerializedDataType.AngleError)
                {
                    Dictionary<int, double> des = DeserializeDictionaryIntDouble();
                    if (des != null)
                    {

                        TuneVariables.Default.AngleError = new SerializableDictionary<int, double>();
                        des.ToList().ForEach(p => TuneVariables.Default.AngleError[p.Key] = p.Value);
                        TuneVariables.Default.Save();
                    }
                }
                else if (type == SerializedDataType.OPMatrix)
                {
                    ret.OPMatrix = DeserializeOPMatrix();
                }

                else if (type == SerializedDataType.ActiveSetting)
                {
                    DeserializeActiveParameters();
                }
                else if (type == SerializedDataType.Sensore)
                {
                    ret.SensoreState = DeserializeDictionaryIntBool();
                }
                else if (type == SerializedDataType.ControlData)
                {
                    DeserializeControlParameters();
                }
                else if (type == SerializedDataType.RotateParameter)
                {
                    DeserializeRotateParameter();
                }
                else if (type == SerializedDataType.PassShootTune)
                {
                    DeserializePassShootParameter();
                }
                else if (type == SerializedDataType.FourCamMergerSetting)
                {
                    Deserialize4CamMerger();
                }
                else if (type == SerializedDataType.ballPlacementPos)
                {
                    DeserializeBallPlacementPos();
                }
            }
            return ret;
        }

        private void DeserializeLookUpTable(bool s)
        {
            //if (LookUpTable.Default.DirectKick == null)
            //    LookUpTable.Default.DirectKick = new SerializableDictionary<string, double>();
            if (LookupTable.Data == null)
                LookupTable.Data = new SerializableDictionary<int, MetricChipKick>();

            //Dictionary<string, double> des = DeserializeDictionaryStringDouble();
            Dictionary<int, MetricChipKick> des2 = DeserializeDictionaryIntMetricChipKick();
            //des.ToList().ForEach(p => LookUpTable.Default.DirectKick[p.Key] = p.Value);
            des2.ToList().ForEach(p => LookupTable.Data[p.Key] = p.Value);
            LookupTable.Save();
            LookupTable.Initialize();
        }


        private void DeserializeLookUp()
        {

            if (LookupTable.Data == null)
                LookupTable.Data = new SerializableDictionary<int, MetricChipKick>();
            Dictionary<int, MetricChipKick> des2 = DeserializeDictionaryIntMetricChipKick();
            des2.ToList().ForEach(p => LookupTable.Data[p.Key] = p.Value);
            LookupTable.Save();
            LookupTable.Initialize();
        }

        public void DeserializeLoockUp(MemoryStream ms)
        {
            stream = ms;
            if (ms != null)
                DeserializeLookUp();
        }

        public MemoryStream SerializeLoockup()
        {
            if (LookUpTable.Default.MetricChipKick == null)
                LookUpTable.Default.MetricChipKick = new SerializableDictionary<int, MetricChipKick>();
            Serialize(LookUpTable.Default.MetricChipKick);
            return stream;
        }

        //      public SaveModelData SMD = new SaveModelData(7, false);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<AiToVisualizerWrapper> DeserializeLogFile(MemoryStream data, out string comment, out List<LoggerDrawingObject> ldo, out List<LogCharterData> lcd, string tag)
        {
            this.stream = data;
            ldo = new List<LoggerDrawingObject>();
            lcd = new List<LogCharterData>();
            comment = "";
            List<AiToVisualizerWrapper> result = new List<AiToVisualizerWrapper>();
            comment = DeserializeString();
            List<LogCharterData> lcdtemp = new List<LogCharterData>();
            List<LoggerDrawingObject> ldotemp = new List<LoggerDrawingObject>();
            int counter = 0;
            Task t = new Task((Action)(() =>
            {
                while (data.Position < data.Length - 1)
                {
                    counter++;
                    try
                    {
                        DrawingObjects.CurrentPositionReaded = data.Position;
                        AiToVisualizerWrapper a = new AiToVisualizerWrapper();
                        AiToVisualizerWrapper ret = new AiToVisualizerWrapper();

                        while (DeserializeInt().As<StreamControlCode>() == StreamControlCode.Continue)
                        {
                            SerializedDataType type = DeserializeInt().As<SerializedDataType>();
                            if (type == SerializedDataType.Model)
                                ret.Model = DeserializeWorldModel(stream);
                            if (type == SerializedDataType.GlobalModel)
                                ret.GlobalModel = DeserializeWorldModel(stream);
                            else if (type == SerializedDataType.Balls)
                                ret.AllBalls = DeserializeDictionaryIntPosiotion2D();
                            else if (type == SerializedDataType.Techniques)
                                ret.Techniques = DeserializeDictionaryStringString();
                            else if (type == SerializedDataType.Engines)
                                ret.Engines = DeserializeDictionaryIntEngines();
                            else if (type == SerializedDataType.Console)
                                DeserializeVisualizerConsole();
                            else if (type == SerializedDataType.RobotCommand)
                            {
                                Dictionary<int, SingleWirelessCommand> des = DeserializeDictionaryIntSingleWirelssCommand();
                                if (des == null)
                                    ret.RobotCommnd = null;
                                else
                                {
                                    ret.RobotCommnd = new Dictionary<int, SingleWirelessCommand>();
                                    des.ToList().ForEach(p => ret.RobotCommnd.Add(p.Key, p.Value));
                                }
                            }
                            else if (type == SerializedDataType.Customvariables)
                                DeserializeCustomVariable(false);
                            else if (type == SerializedDataType.GameParameters)
                                DeserializeGameParameters();
                            else if (type == SerializedDataType.BallStatus)
                                ret.BallStatus = DeserializeString();
                            else if (type == SerializedDataType.RobotData)
                                ret.MtrixData = DeserializeRobotData();
                            else if (type == SerializedDataType.ActiveSetting)
                                DeserializeActiveParameters();
                            else if (type == SerializedDataType.Strategies)
                            {
                                DeserializeStrategies(data);
                            }
                            else if (type == SerializedDataType.RotateParameter)
                            {
                                DeserializeRotateParameter();
                            }
                            else if (type == SerializedDataType.PassShootTune)
                            {
                                DeserializePassShootParameter();
                            }
                        }
                        ret.StrategySended = DeserializeBool();

                        LogLoading.CurrentLogPlayerTag = tag;
                        LoggerDrawingObject ld = DeserializeDrawingObjectLog();
                        ldotemp.Add(ld);
                        LogLoading.LoadedDrawingObject = ld;

                        //  SMD.Model = new WorldModel(ret.Model);
                        //SingleObjectState st = null;

                        //var ff = ld.drawingObject.SingleOrDefault(s => s.Value is DrawCollection && s.Key == "Merger Tracker");
                        //if (ff.Value != null)
                        //    st = ff.Value.As<DrawCollection>().drawingObject.SingleOrDefault(s => s.Value is SingleObjectState && s.Key == "*7")
                        //        .Value.As<SingleObjectState>();
                        //messages_robocup_ssl_wrapper.SSL_WrapperPacket ss /*= SMD.SSLPacket*/ = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
                        //ss.detection = new messages_robocup_ssl_detection.SSL_DetectionFrame();
                        ////Position2D? b = null;
                        ////if (ret.AllBalls.Count != 0)
                        ////{
                        ////    //b = ret.AllBalls.ElementAt(0).Value;
                        ////    double min = ret.AllBalls.Min(m => m.Value.DistanceFrom(ret.Model.BallState.Location));
                        ////    b = ret.AllBalls.First(f => f.Value.DistanceFrom(ret.Model.BallState.Location) <= min).Value;
                        ////}
                        ////if(b.HasValue)
                        ////    ss.detection.balls.Add(new messages_robocup_ssl_detection.SSL_DetectionBall() { x = (float)b.Value.X, y = (float)b.Value.Y });
                        //if (st != null)
                        //    ss.detection.robots_blue.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                        //    {
                        //        x = (float)st.Location.X,
                        //        y = (float)st.Location.Y,
                        //        orientation = st.Angle.Value,
                        //        robot_id = 7
                        //    });
                        //else
                        //    ss.detection.robots_yellow.Add(null);
                        //  SMD.SSLPacket = (messages_robocup_ssl_wrapper.SSL_WrapperPacket)ss.Clone();

                        //SMD.Add();

                        lcdtemp.Add(DeserializeLogChart(false));

                        if (ret.Model != null)
                        {
                            result.Add(ret);

                            LogLoading.Loaded = ret;
                        }
                        DrawingObjects.CurrentPositionReaded = data.Position;
                        //if (counter == 17720)
                        //    break;
                    }
                    catch (OutOfMemoryException ex)
                    {
                        Logger.Write(LogType.Exception, ex.ToString());

                        Task.WaitAll();
                        break;
                    }
                }
                //using (FileStream file = new FileStream(@"d:\data.txt", FileMode.Create, FileAccess.Write))
                //{
                //    MemoryStream ms = new MemoryStream();
                //    using (StreamWriter fs = new StreamWriter(ms))
                //    {

                //        fs.Write("X\tY\tVx\tVy\tAx\tAy\n");
                //        foreach (var item in result)
                //        {
                //            fs.Write(item.Model.BallState.Location.X.ToString("f3") + "\t");
                //            fs.Write(item.Model.BallState.Location.Y.ToString("f3") + "\t");
                //            fs.Write(item.Model.BallState.Speed.X.ToString("f3") + "\t");
                //            fs.Write(item.Model.BallState.Speed.Y.ToString("f3") + "\t");
                //            fs.Write(item.Model.BallState.Acceleration.X.ToString("f3") + "\t");
                //            fs.Write(item.Model.BallState.Acceleration.Y.ToString("f3") + "\n");
                //        }
                //        file.Write(ms.ToArray(), 0, (int)ms.Length);
                //    }
                //}
            }));
            t.Start();



            lcd = lcdtemp;
            ldo = ldotemp;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(StringDraw val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Key);
                Serialize(val.Content);
                Serialize(val.TextColor);
                Serialize(val.Posiotion);
                Serialize(val.IsShown);
                Serialize(val.ColorChange);
                Serialize(val.OnTop);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        StringDraw DeserializeStringDraw()
        {
            if (DeserializeBool())
            {
                StringDraw result = new StringDraw()
                {
                    Key = DeserializeString(),
                    Content = DeserializeString(),
                    TextColor = DeserializeColor(),
                    Posiotion = DeserializePosition2D(),
                    IsShown = DeserializeBool(),
                    ColorChange = DeserializeBool(),
                    OnTop = DeserializeBool(),
                };
                return result;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(FlatRectangle val)
        {
            Serialize(val.BorderColor);
            Serialize(val.BorderWidth);
            Serialize(val.FillColor);
            Serialize(val.IsShown);
            Serialize(val.IsFill);
            Serialize(val.Opacity);
            Serialize(val.Size);
            Serialize(val.TopLeft);
            Serialize(val.PenIsChanged);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private FlatRectangle DeserializeFlatRectangle()
        {
            FlatRectangle result = new FlatRectangle();
            result.BorderColor = DeserializeColor();
            result.BorderWidth = DeserializeFloat();
            result.FillColor = DeserializeColor();
            result.IsShown = DeserializeBool();
            result.IsFill = DeserializeBool();
            result.Opacity = DeserializeFloat();
            result.Size = DeserializeVector2D();
            result.TopLeft = DeserializePosition2D();
            result.PenIsChanged = DeserializeBool();
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        private void Serialize(ConsoleObject val)
        {
            Serialize(val.Content);
            Serialize(val.ControlChar);
            Serialize(val.ForgroundColor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ConsoleObject DeserializeConsoleObject()
        {
            ConsoleObject res = new ConsoleObject();
            res.Content = DeserializeString();
            res.ControlChar = DeserializeString();
            res.ForgroundColor = DeserializeColor();
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void Serialize(List<ConsoleObject> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in val)
                    Serialize(item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ConsoleObject> DeserializeListConsoleObject()
        {
            if (DeserializeBool())
            {
                List<ConsoleObject> res = new List<ConsoleObject>();
                int count = DeserializeInt();
                for (int i = 0 ; i < count ; i++)
                    res.Add(DeserializeConsoleObject());
                return res;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        private void SerializeVisualizerConsole()
        {
            Serialize(VisualizerConsole.Data);
        }
        /// <summary>
        /// 
        /// </summary>
        private void DeserializeVisualizerConsole()
        {
            VisualizerConsole.Data = DeserializeListConsoleObject();
        }

        private void SerializeControlParameters()
        {
            Serialize(ControlParameters.Accel);
            Serialize(ControlParameters.Accuercy);
            Serialize(ControlParameters.aFactor);
            Serialize(ControlParameters.Decel);
            Serialize(ControlParameters.MaxSpeed);
            Serialize(ControlParameters.P0);
            Serialize(ControlParameters.P1);
            Serialize(ControlParameters.Q0);
            Serialize(ControlParameters.Q1);
            Serialize(ControlParameters.TunningAngle);
            Serialize(ControlParameters.TunningDist);
            Serialize(ControlParameters.wAccel);
            Serialize(ControlParameters.Waccuercy);
            Serialize(ControlParameters.WaFactor);
            Serialize(ControlParameters.wDecel);
            Serialize(ControlParameters.wMaxS);
            Serialize(ControlParameters.wP0);
            Serialize(ControlParameters.wP1);
            Serialize(ControlParameters.wQ0);
            Serialize(ControlParameters.wQ1);
            Serialize(ControlParameters.K_ang);
            Serialize(ControlParameters.K_lf);
            Serialize(ControlParameters.K_sumAng);
            Serialize(ControlParameters.K_total);
            Serialize(ControlParameters.K_v0);
        }

        private void DeserializeControlParameters()
        {
            ControlParameters.Accel = DeserializeDouble();
            ControlParameters.Accuercy = DeserializeDouble();
            ControlParameters.aFactor = DeserializeDouble();
            ControlParameters.Decel = DeserializeDouble();
            ControlParameters.MaxSpeed = DeserializeDouble();
            ControlParameters.P0 = DeserializePosition2D();
            ControlParameters.P1 = DeserializePosition2D();
            ControlParameters.Q0 = DeserializePosition2D();
            ControlParameters.Q1 = DeserializePosition2D();
            ControlParameters.TunningAngle = DeserializeDouble();
            ControlParameters.TunningDist = DeserializeDouble();
            ControlParameters.wAccel = DeserializeDouble();
            ControlParameters.Waccuercy = DeserializeDouble();
            ControlParameters.WaFactor = DeserializeDouble();
            ControlParameters.wDecel = DeserializeDouble();
            ControlParameters.wMaxS = DeserializeDouble();
            ControlParameters.wP0 = DeserializePosition2D();
            ControlParameters.wP1 = DeserializePosition2D();
            ControlParameters.wQ0 = DeserializePosition2D();
            ControlParameters.wQ1 = DeserializePosition2D();

            ControlParameters.K_ang = DeserializeDouble();
            ControlParameters.K_lf = DeserializeDouble();
            ControlParameters.K_sumAng = DeserializeDouble();
            ControlParameters.K_total = DeserializeDouble();
            ControlParameters.K_v0 = DeserializeDouble();
        }

        public void Serialize(Dictionary<string, StrategyInfo> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in StrategyInfo.Strategies)
                {
                    Serialize(item.Key);
                    Serialize(item.Value);
                }
            }
        }

        public Dictionary<string, StrategyInfo> DeserializeStrategies(MemoryStream stream)
        {
            this.stream = stream;
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                Dictionary<string, StrategyInfo> ret = new Dictionary<string, StrategyInfo>();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeString(), DeserializeStrategyInfo());
                return ret;
            }
            return null;
        }

        private void Serialize(StrategyInfo val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.About);
                Serialize(val.AttendanceSize);
                Serialize(val.DrawingInfo);
                Serialize(val.Enabled);
                Serialize(val.Name);
                Serialize(val.Probability);
                Serialize(val.Region);
                Serialize(val.Status);
            }
        }

        private void Serialize(List<GameStatus> list)
        {
            Serialize(list != null);
            if (list != null)
            {
                Serialize(list.Count);
                foreach (var item in list)
                    Serialize((int)item);
            }
        }

        private void Serialize(Dictionary<int, Dictionary<int, double>> Dic)
        {
            lock (Dic)
            {
                Serialize(Dic != null);
                if (Dic != null)
                {
                    Serialize(Dic.Count);
                    foreach (var item in Dic.Keys.ToList())
                    {
                        Serialize(item);
                        Serialize(Dic[item]);
                    }
                }
            }
        }

        private void SerializeRotateParameter()
        {
            Serialize(RotateParameters.VyValues);
            Serialize(RotateParameters.OmegaValues);
            Serialize(RotateParameters.angle);
            Serialize(RotateParameters.Omegacoeff);
            Serialize(RotateParameters.RoboID);
            Serialize(RotateParameters.TuneFlag);
            Serialize(RotateParameters.Vycoeff);
        }


        private void SerializePassShootParameter()
        {
            Serialize(PassShootParameter.ActiveFlag);
            Serialize(PassShootParameter.Passa);
            Serialize(PassShootParameter.Passb);
            Serialize(PassShootParameter.PasserID);
            Serialize(PassShootParameter.passSpeed);
            Serialize(PassShootParameter.Shoota);
            Serialize(PassShootParameter.Shootb);
            Serialize(PassShootParameter.shooterDistance);
            Serialize(PassShootParameter.ShooterID);
            Serialize(PassShootParameter.shootSpeed);
            Serialize(PassShootParameter.start);
            Serialize(PassShootParameter.test);
            Serialize(PassShootParameter.YDistance);
            Serialize(PassShootParameter.AcceptData);
            Serialize(PassShootParameter.spinBack);
        }


        private Dictionary<int, Dictionary<int, double>> DeserializeDictionaryIntDictionaryIntDouble()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                Dictionary<int, Dictionary<int, double>> ret = new Dictionary<int, Dictionary<int, double>>();

                for (int i = 0 ; i < count ; i++)
                {
                    ret.Add(DeserializeInt(), DeserializeDictionaryIntDouble());
                }

                return ret;

            }
            return null;
        }

        private void DeserializeRotateParameter()
        {

            RotateParameters.VyValues = DeserializeDictionaryIntDictionaryIntDouble();
            RotateParameters.OmegaValues = DeserializeDictionaryIntDictionaryIntDouble();
            RotateParameters.angle = DeserializeInt();
            RotateParameters.Omegacoeff = DeserializeDouble();
            RotateParameters.RoboID = DeserializeInt();
            RotateParameters.TuneFlag = DeserializeBool();
            RotateParameters.Vycoeff = DeserializeDouble();
        }

        private void DeserializePassShootParameter()
        {

            PassShootParameter.ActiveFlag = DeserializeBool();
            PassShootParameter.Passa = DeserializeDouble();
            PassShootParameter.Passb = DeserializeDouble();
            PassShootParameter.PasserID = DeserializeInt();
            PassShootParameter.passSpeed = DeserializeDouble();
            PassShootParameter.Shoota = DeserializeDouble();
            PassShootParameter.Shootb = DeserializeDouble();
            PassShootParameter.shooterDistance = DeserializeDouble();
            PassShootParameter.ShooterID = DeserializeInt();
            PassShootParameter.shootSpeed = DeserializeDouble();
            PassShootParameter.start = DeserializeBool();
            PassShootParameter.test = DeserializeBool();
            PassShootParameter.YDistance = DeserializeDouble();
            PassShootParameter.AcceptData = DeserializeBool();
            PassShootParameter.spinBack = DeserializeBool();
        }

        private List<GameStatus> DeserializeListStatus()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<GameStatus> ret = new List<GameStatus>();
                ;
                for (int i = 0 ; i < count ; i++)
                    ret.Add((GameStatus)DeserializeInt());
                return ret;
            }
            return null;
        }
        private void Serialize(List<Vector2D> list)
        {
            Serialize(list != null);
            if (list != null)
            {
                Serialize(list.Count);
                foreach (var item in list)
                    Serialize(item);
            }
        }

        private List<Vector2D> DeserializeListVector2D()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<Vector2D> ret = new List<Vector2D>();
                ;
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeVector2D());
                return ret;
            }
            return null;
        }

        private StrategyInfo DeserializeStrategyInfo()
        {
            if (DeserializeBool())
            {
                StrategyInfo ret = new StrategyInfo();
                ret.About = DeserializeString();
                ret.AttendanceSize = DeserializeInt();
                ret.DrawingInfo = DeserializeListStrategyDarwingInfo();
                ret.Enabled = DeserializeBool();
                ret.Name = DeserializeString();
                ret.Probability = DeserializeDouble();
                ret.Region = DeserializeListVector2D();
                ret.Status = DeserializeListStatus();
                return ret;
            }
            return null;
        }





        private void Serialize(List<StrategyDrawingInfo> list)
        {
            Serialize(list != null);
            if (list != null)
            {
                Serialize(list.Count);
                foreach (var item in list)
                {
                    Serialize(item.Model);
                    Serialize(item.Drawing, true);
                }
            }
        }

        private List<StrategyDrawingInfo> DeserializeListStrategyDarwingInfo()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                List<StrategyDrawingInfo> ret = new List<StrategyDrawingInfo>();
                for (int i = 0 ; i < count ; i++)
                {
                    StrategyDrawingInfo item = new StrategyDrawingInfo();
                    item.Model = DeserializeWorldModel(stream);
                    item.Drawing = DeserializeDrawingCollection();
                    ret.Add(item);
                }
                return ret;
            }
            return null;
        }



        private void SerializeActiveParameters()
        {
            Serialize(ActiveParameters.sweepZone);
            Serialize(ActiveParameters.kickAnyWayRegion);
            Serialize(ActiveParameters.kickRegionAcuercy);
            Serialize(ActiveParameters.kickAcuercy);
            Serialize(ActiveParameters.minGoodness);
            Serialize(ActiveParameters.nearIncomingRadi);
            Serialize(ActiveParameters.nearBallSpeedTresh);
            Serialize(ActiveParameters.incomingBallDistanceTresh);
            Serialize(ActiveParameters.outgoingSideAngleTresh);
            Serialize(ActiveParameters.kickedToUsRadi);
            Serialize(ActiveParameters.kickedToUsBallSpeedTresh);
            Serialize(ActiveParameters.clearRobotZone);
            Serialize(ActiveParameters.ConfilictZone);
            Serialize(ActiveParameters.IncomingBackBall);
            Serialize(ActiveParameters.KpSideY);
            Serialize(ActiveParameters.KdSideY);
            Serialize(ActiveParameters.KiSideY);
            Serialize(ActiveParameters.KpSideX);
            Serialize(ActiveParameters.KiSideX);

            Serialize(ActiveParameters.KdSideX);
            Serialize(ActiveParameters.vyOffsetSide);
            Serialize(ActiveParameters.KpxVySide);
            Serialize(ActiveParameters.KpyVySide);
            Serialize(ActiveParameters.KpTotalVySide);
            Serialize(ActiveParameters.KpxVxSide);
            Serialize(ActiveParameters.KpBackY);
            Serialize(ActiveParameters.KiBackY);
            Serialize(ActiveParameters.KdBackY);
            Serialize(ActiveParameters.KpBackX);
            Serialize(ActiveParameters.KiBackX);
            Serialize(ActiveParameters.KdBackX);
            Serialize(ActiveParameters.KpyVyBack);
            Serialize(ActiveParameters.KpyVyBack);
            Serialize(ActiveParameters.KpxVyBack);
            Serialize(ActiveParameters.vyOffsetBack);
            Serialize(ActiveParameters.KpTotalVyBack);
            Serialize(ActiveParameters.KpTotalVyBack);
            Serialize(ActiveParameters.KpxVxBack);

            Serialize(ActiveParameters.UseChipDrible);
            Serialize(ActiveParameters.UseSpaceDrible);
            Serialize(ActiveParameters.KickInRegion);

            Serialize((int)ActiveParameters.sweepMode);
            Serialize((int)ActiveParameters.kickDefult);
            Serialize((int)ActiveParameters.playMode);
            Serialize((int)ActiveParameters.conflictMode);
            Serialize((int)ActiveParameters.clearMode);

            Serialize(ActiveParameters.RobotMotionCoefs);


            Serialize(ActiveParameters.NewActiveParameters.UseSpaceDrible);
            Serialize(ActiveParameters.NewActiveParameters.KickInRegion);

            Serialize((int)ActiveParameters.NewActiveParameters.sweepMode);
            Serialize((int)ActiveParameters.NewActiveParameters.kickDefult);
            Serialize((int)ActiveParameters.NewActiveParameters.playMode);
            Serialize((int)ActiveParameters.NewActiveParameters.conflictMode);
            Serialize((int)ActiveParameters.NewActiveParameters.clearMode);

            Serialize(ActiveParameters.NewActiveParameters.KickInRegionAcc);
            Serialize(ActiveParameters.NewActiveParameters.ConfilictZone);
            Serialize(ActiveParameters.NewActiveParameters.kickAnyWayRegion);
            Serialize(ActiveParameters.NewActiveParameters.sweepZone);
            Serialize(ActiveParameters.NewActiveParameters.minGoodness);
            Serialize(ActiveParameters.NewActiveParameters.clearRobotZone);
            Serialize(ActiveParameters.NewActiveParameters.kickAccuracy);

            ActiveParameters.Save();
        }

        private void Serialize(double[,] p)
        {
            Serialize(p.GetLength(0));
            Serialize(p.GetLength(1));
            for (int i = 0 ; i < p.GetLength(0) ; i++)
                for (int j = 0 ; j < p.GetLength(1) ; j++)
                    Serialize(p[i, j]);
        }
        private double[,] Deserialize2DArrayDouble()
        {

            int firstdim = DeserializeInt();
            int seconddim = DeserializeInt();
            double[,] p = new double[firstdim, seconddim];
            for (int i = 0 ; i < firstdim ; i++)
                for (int j = 0 ; j < seconddim ; j++)
                    p[i, j] = DeserializeDouble();
            return p;
        }

        private void DeserializeActiveParameters()
        {
            ActiveParameters.sweepZone = DeserializeDouble();
            ActiveParameters.kickAnyWayRegion = DeserializeDouble();
            ActiveParameters.kickAcuercy = DeserializeDouble();
            ActiveParameters.kickRegionAcuercy = DeserializeDouble();
            ActiveParameters.minGoodness = DeserializeDouble();
            ActiveParameters.nearIncomingRadi = DeserializeDouble();
            ActiveParameters.nearBallSpeedTresh = DeserializeDouble();
            ActiveParameters.incomingBallDistanceTresh = DeserializeDouble();
            ActiveParameters.outgoingSideAngleTresh = DeserializeDouble();
            ActiveParameters.kickedToUsRadi = DeserializeDouble();
            ActiveParameters.kickedToUsBallSpeedTresh = DeserializeDouble();
            ActiveParameters.clearRobotZone = DeserializeDouble();
            ActiveParameters.ConfilictZone = DeserializeDouble();
            ActiveParameters.IncomingBackBall = DeserializeDouble();
            ActiveParameters.KpSideY = DeserializeDouble();
            ActiveParameters.KdSideY = DeserializeDouble();
            ActiveParameters.KiSideY = DeserializeDouble();
            ActiveParameters.KpSideX = DeserializeDouble();
            ActiveParameters.KiSideX = DeserializeDouble();
            ActiveParameters.KdSideX = DeserializeDouble();
            ActiveParameters.vyOffsetSide = DeserializeDouble();
            ActiveParameters.KpxVySide = DeserializeDouble();
            ActiveParameters.KpyVySide = DeserializeDouble();
            ActiveParameters.KpTotalVySide = DeserializeDouble();
            ActiveParameters.KpxVxSide = DeserializeDouble();
            ActiveParameters.KpBackY = DeserializeDouble();
            ActiveParameters.KiBackY = DeserializeDouble();
            ActiveParameters.KdBackY = DeserializeDouble();
            ActiveParameters.KpBackX = DeserializeDouble();
            ActiveParameters.KiBackX = DeserializeDouble();
            ActiveParameters.KdBackX = DeserializeDouble();
            ActiveParameters.KpyVyBack = DeserializeDouble();
            ActiveParameters.KpyVyBack = DeserializeDouble();
            ActiveParameters.KpxVyBack = DeserializeDouble();
            ActiveParameters.vyOffsetBack = DeserializeDouble();
            ActiveParameters.KpTotalVyBack = DeserializeDouble();
            ActiveParameters.KpTotalVyBack = DeserializeDouble();
            ActiveParameters.KpxVxBack = DeserializeDouble();
            ActiveParameters.UseChipDrible = DeserializeBool();
            ActiveParameters.UseSpaceDrible = DeserializeBool();
            ActiveParameters.KickInRegion = DeserializeBool();
            ActiveParameters.sweepMode = (ActiveParameters.SweepDefult)DeserializeInt();
            ActiveParameters.kickDefult = (ActiveParameters.KickDefult)DeserializeInt();
            ActiveParameters.playMode = (ActiveParameters.PlayMode)DeserializeInt();
            ActiveParameters.conflictMode = (ActiveParameters.ConflictMode)DeserializeInt();
            ActiveParameters.clearMode = (ActiveParameters.NonClearMode)DeserializeInt();
            ActiveParameters.RobotMotionCoefs = Deserialize2DArrayDouble();

            ActiveParameters.NewActiveParameters.UseSpaceDrible = DeserializeBool();
            ActiveParameters.NewActiveParameters.KickInRegion = DeserializeBool();
            ActiveParameters.NewActiveParameters.sweepMode = (ActiveParameters.NewActiveParameters.SweepDefult)DeserializeInt();
            ActiveParameters.NewActiveParameters.kickDefult = (ActiveParameters.NewActiveParameters.KickDefult)DeserializeInt();
            ActiveParameters.NewActiveParameters.playMode = (ActiveParameters.NewActiveParameters.PlayMode)DeserializeInt();
            ActiveParameters.NewActiveParameters.conflictMode = (ActiveParameters.NewActiveParameters.ConflictMode)DeserializeInt();
            ActiveParameters.NewActiveParameters.clearMode = (ActiveParameters.NewActiveParameters.NonClearMode)DeserializeInt();
            
            ActiveParameters.NewActiveParameters.KickInRegionAcc = DeserializeDouble();
            ActiveParameters.NewActiveParameters.ConfilictZone = DeserializeDouble();
            ActiveParameters.NewActiveParameters.kickAnyWayRegion = DeserializeDouble();
            ActiveParameters.NewActiveParameters.sweepZone = DeserializeDouble();
            ActiveParameters.NewActiveParameters.minGoodness = DeserializeDouble();
            ActiveParameters.NewActiveParameters.clearRobotZone = DeserializeDouble();
            ActiveParameters.NewActiveParameters.kickAccuracy = DeserializeDouble();

            ActiveParameters.Save();
        }

        private void Serialize(RotateDetail val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.VyCoef);
                Serialize(val.Wcoef);
            }
        }
        private RotateDetail DeserializeRotateDetail()
        {
            if (DeserializeBool())
            {
                var vy = DeserializeDouble();
                var w = DeserializeDouble();
                return new RotateDetail()
                {
                    VyCoef = vy,
                    Wcoef = w
                };
            }
            return null;
        }
        private void Serialize(Dictionary<double, RotateDetail> val)
        {
            Serialize(val != null);
            if (val != null)
            {
                Serialize(val.Count);
                foreach (var item in val)
                {
                    Serialize(item.Key);
                    Serialize(item.Value);
                }
            }
        }
        private Dictionary<double, RotateDetail> DeserializeDictionaryDoubleRotateDetail()
        {
            if (DeserializeBool())
            {
                int count = DeserializeInt();
                Dictionary<double, RotateDetail> ret = new Dictionary<double, RotateDetail>();
                for (int i = 0 ; i < count ; i++)
                    ret.Add(DeserializeDouble(), DeserializeRotateDetail());
                return ret;
            }
            return null;
        }


        #region mamad
        /// <summary>
        /// Serialize a Dictionary(int, MathMatrix) by google protocol and save to stream
        /// </summary>
        /// <param name="val">a Dictionary(int, MathMatrix)</param>
        void Serialize(Dictionary<int, MathMatrix> paths)
        {
            Serialize(paths != null);
            if (paths != null)
            {
                Serialize(paths.Count);
                int counter = 0;
                foreach (int key in paths.Keys)
                {
                    counter++;
                    Serialize(key);
                    Serialize(paths[key]);
                }
                if (counter != paths.Count)
                    throw new Exception("This should never happen");
            }
        }
        /// <summary>
        /// Diserialize a Dictionary of int, MathMatrix from a stream
        /// </summary>
        /// <returns>Deserialized Dictionary of int, MathMatrix</returns>
        Dictionary<int, MathMatrix> DeserializeDictionaryIntMathMatrix()
        {
            if (!DeserializeBool())
                return null;
            Dictionary<int, MathMatrix> ret = new Dictionary<int, MathMatrix>();
            int count = DeserializeInt();
            for (int i = 0; i < count; i++)
            {
                int key = DeserializeInt();
                MathMatrix matrix = DeserializeMathMatrix();
                ret.Add(key, matrix);
            }
            return ret;
        }

        /// <summary>
        /// Serialize a MathMatrix by google protocol and save to stream
        /// </summary>
        /// <param name="val">MathMatrix</param>
        private void Serialize(MathMatrix matrix)
        {
            Serialize(matrix != null);
            if (matrix != null)
            {
                Serialize(matrix.Rows);
                Serialize(matrix.Cols);
                for (int i = 0; i < matrix.Rows; i++)
                    for (int j = 0; j < matrix.Cols; j++)
                        Serialize(matrix[i, j]);
            }
        }
        /// <summary>
        /// Diserialize a MathMatrix from a stream
        /// </summary>
        /// <returns>Deserialized MathMatrix</returns>
        private MathMatrix DeserializeMathMatrix()
        {
            if (DeserializeBool())
            {

                int rows = DeserializeInt();
                int cols = DeserializeInt();
                MathMatrix matrix = new MathMatrix(rows, cols);
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        matrix[i, j] = (DeserializeDouble());
                return matrix;
            }
            return null;
        }
        private void Serialize4CamMerger()
        {
            Serialize(MergerParameters.CoefMatrix);
            Serialize(MergerParameters.MidCoefMat);
            Serialize(MergerParameters.AvailableCamIds.ToArray());
        }
        private void Deserialize4CamMerger()
        {
            MergerParameters.CoefMatrix = DeserializeDictionaryIntMathMatrix();
            MergerParameters.MidCoefMat = DeserializeDictionaryIntMathMatrix();
            MergerParameters.AvailableCamIds = DeserializeIntArray().ToList();
        }
        private void SerializeBallPlacementPos()
        {
            Serialize(StaticVariables.ballPlacementPos);
        }
        private void DeserializeBallPlacementPos()
        {
            StaticVariables.ballPlacementPos = DeserializePosition2D();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSeries"></param>
        public delegate void SerieseChanged(List<string> newSeries);
        public static event SerieseChanged SerieChanged;

        public delegate void LogSerieseChanged(List<string> newSeries);
        public static event LogSerieseChanged LogSerieChanged;
    }
}
