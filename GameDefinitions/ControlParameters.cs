using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace MRL.SSL.GameDefinitions
{

    public class ParameterList
    {
        public double Accel { get; set; }
        public double Decel { get; set; }
        public double aFactor { get; set; }
        public double WaFactor { get; set; }
        public double Accuercy { get; set; }
        public double Waccuercy { get; set; }
        public double MaxSpeed { get; set; }
        public double wAccel { get; set; }
        public double wDecel { get; set; }
        public double wMaxS { get; set; }
        public double TunningDist { get; set; }
        public double TunningAngle { get; set; }
        public double Mean { get; set; }
        public double Variance { get; set; }
        public double PathWeight { get; set; }
        public double K_v0 { get; set; }
        public double K_lf { get; set; }
        public double K_total { get; set; }
        public double K_ang { get; set; }
        public double K_sumAng { get; set; }
        public Position2D P0 { get; set; }
        public Position2D P1 { get; set; }
        public Position2D Q0 { get; set; }
        public Position2D Q1 { get; set; }
        public Position2D wP0 { get; set; }
        public Position2D wP1 { get; set; }
        public Position2D wQ0 { get; set; }
        public Position2D wQ1 { get; set; }

        public ParameterList(ParameterList p)
        {
            Accel = p.Accel;
            Decel = p.Decel;
            aFactor = p.aFactor;
            Accuercy = p.Accuercy;
            MaxSpeed = p.MaxSpeed;
            P0 = p.P0;
            P1 = p.P1;
            Q0 = p.Q0;
            Q1 = p.Q1;
            TunningAngle = p.TunningAngle;
            TunningDist = p.TunningDist;
            wAccel = p.wAccel;
            Waccuercy = p.Waccuercy;
            WaFactor = p.WaFactor;
            wDecel = p.wDecel;
            wMaxS = p.wMaxS;
            wP0 = p.wP0;
            wP1 = p.wP1;
            wQ0 = p.wQ0;
            wQ1 = p.wQ1;
            Mean = p.Mean;
            Variance = p.Variance;
            K_v0 = p.K_v0;
            K_lf = p.K_lf;
            K_total = p.K_total;
            K_sumAng = p.K_sumAng;
            K_ang = p.K_ang;
        }
        public ParameterList()
        {
        }



    }

    public static class ControlParameters
    {
        public static bool BallIsMoved = false;
        public static int GoalieID { get; set; }
        public static double Accel { get; set; }
        public static double Decel { get; set; }
        public static double aFactor { get; set; }
        public static double WaFactor { get; set; }
        public static double Accuercy { get; set; }
        public static double Waccuercy { get; set; }
        public static double MaxSpeed { get; set; }
        public static double wAccel { get; set; }
        public static double wDecel { get; set; }
        public static double wMaxS { get; set; }
        public static double TunningDist { get; set; }
        public static double TunningAngle { get; set; }
        public static double Mean { get; set; }
        public static double Variance { get; set; }
        public static double PathWeight { get; set; }
        public static double K_v0 { get; set; }
        public static double K_lf { get; set; }
        public static double K_total { get; set; }
        public static double K_ang { get; set; }
        public static double K_sumAng { get; set; }
        public static Position2D P0 { get; set; }
        public static Position2D P1 { get; set; }
        public static Position2D Q0 { get; set; }
        public static Position2D Q1 { get; set; }
        public static Position2D wP0 { get; set; }
        public static Position2D wP1 { get; set; }
        public static Position2D wQ0 { get; set; }
        public static Position2D wQ1 { get; set; }
        public static Dictionary<int, ParameterList> robotControlParams = new Dictionary<int, ParameterList>();

        //public static void SetParams(ParameterList param)
        //{
        //    Accel = param.Accel;
        //    Decel = param.Decel;
        //    MaxSpeed = param.MaxSpeed;
        //}

        public static void Set(int RobotID, ParameterList param)
        {
            robotControlParams[RobotID] = param;
        }

        public static ParameterList Get(int RobotID)
        {
            if (robotControlParams.ContainsKey(RobotID))
                return robotControlParams[RobotID];
            else
                return new ParameterList()
            {
                Accel = Accel,
                Decel = Decel,
                aFactor = aFactor,
                Accuercy = Accuercy,
                MaxSpeed = MaxSpeed,
                P0 = P0,
                P1 = P1,
                Q0 = Q0,
                Q1 = Q1,
                TunningAngle = TunningAngle,
                TunningDist = TunningDist,
                wAccel = wAccel,
                Waccuercy = Waccuercy,
                WaFactor = WaFactor,
                wDecel = wDecel,
                wMaxS = wMaxS,
                wP0 = wP0,
                wP1 = wP1,
                wQ0 = wQ0,
                wQ1 = wQ1,
                Mean = Mean,
                Variance = Variance,
                K_ang = K_ang,
                K_lf = K_lf,
                K_sumAng = K_sumAng,
                K_total = K_total,
                K_v0 = K_v0,
            };

        }
        public static void Save(string fileName, bool saveas = false)
        {
            if (!saveas)
            {
                if (!fileName.Contains(".xml"))
                    fileName += ".xml";
                fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            }
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("ControlParameters");

                #region Doubles
                writer.WriteStartElement("accel");
                writer.WriteValue(Accel);
                writer.WriteEndElement();

                writer.WriteStartElement("decel");
                writer.WriteValue(Decel);
                writer.WriteEndElement();

                writer.WriteStartElement("aFactor");
                writer.WriteValue(aFactor);
                writer.WriteEndElement();

                writer.WriteStartElement("WaFactor");
                writer.WriteValue(WaFactor);
                writer.WriteEndElement();

                writer.WriteStartElement("Accuercy");
                writer.WriteValue(Accuercy);
                writer.WriteEndElement();

                writer.WriteStartElement("Waccuercy");
                writer.WriteValue(Waccuercy);
                writer.WriteEndElement();

                writer.WriteStartElement("MaxSpeed");
                writer.WriteValue(MaxSpeed);
                writer.WriteEndElement();

                writer.WriteStartElement("wAccel");
                writer.WriteValue(wAccel);
                writer.WriteEndElement();

                writer.WriteStartElement("wDecel");
                writer.WriteValue(wDecel);
                writer.WriteEndElement();

                writer.WriteStartElement("wMaxS");
                writer.WriteValue(wMaxS);
                writer.WriteEndElement();

                writer.WriteStartElement("TunningDist");
                writer.WriteValue(TunningDist);
                writer.WriteEndElement();

                writer.WriteStartElement("TunningAngle");
                writer.WriteValue(TunningAngle);
                writer.WriteEndElement();

                writer.WriteStartElement("Mean");
                writer.WriteValue(Mean);
                writer.WriteEndElement();

                writer.WriteStartElement("Variance");
                writer.WriteValue(Variance);
                writer.WriteEndElement();

                writer.WriteStartElement("PathWeight");
                writer.WriteValue(PathWeight);
                writer.WriteEndElement();

                writeValue(writer, "K_v0", K_v0);
                writeValue(writer, "K_ang", K_ang);
                writeValue(writer, "K_lf", K_lf);
                writeValue(writer, "K_total", K_total);
                writeValue(writer, "K_sumAng", K_sumAng);
                #endregion

                #region Positions
                writer.WriteStartElement("P0");
                writer.WriteStartElement("X");
                writer.WriteValue(P0.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(P0.Y);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("P1");
                writer.WriteStartElement("X");
                writer.WriteValue(P1.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(P1.Y);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("Q0");
                writer.WriteStartElement("X");
                writer.WriteValue(Q0.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(Q0.Y);
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteStartElement("Q1");
                writer.WriteStartElement("X");
                writer.WriteValue(Q1.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(Q1.Y);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("wP0");
                writer.WriteStartElement("X");
                writer.WriteValue(wP0.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(wP0.Y);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("wP1");
                writer.WriteStartElement("X");
                writer.WriteValue(wP1.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(wP1.Y);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("wQ0");
                writer.WriteStartElement("X");
                writer.WriteValue(wQ0.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(wQ0.Y);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("wQ1");
                writer.WriteStartElement("X");
                writer.WriteValue(wQ1.X);
                writer.WriteEndElement();
                writer.WriteStartElement("Y");
                writer.WriteValue(wQ1.Y);
                writer.WriteEndElement();
                #endregion

                writer.WriteFullEndElement();
            }
        }

        public static void Load(string fileName, bool saveas = false)
        {
            if (!fileName.Contains(".xml"))
                fileName += ".xml";
            if (!saveas)
                fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            if (!File.Exists(fileName))
                return;
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {

                reader.ReadStartElement();

                #region Doubles
                reader.ReadStartElement("accel");
                Accel = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("decel");
                Decel = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("aFactor");
                aFactor = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("WaFactor");
                WaFactor = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("Accuercy");
                Accuercy = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("Waccuercy");
                Waccuercy = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("MaxSpeed");
                MaxSpeed = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("wAccel");
                wAccel = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("wDecel");
                wDecel = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("wMaxS");
                wMaxS = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("TunningDist");
                TunningDist = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("TunningAngle");
                TunningAngle = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("Mean");
                Mean = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("Variance");
                Variance = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                reader.ReadStartElement("PathWeight");
                PathWeight = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                K_v0 = double.Parse(readValue(reader, "K_v0").ToString());
                K_ang = double.Parse(readValue(reader, "K_ang").ToString());
                K_lf = double.Parse(readValue(reader, "K_lf").ToString());
                K_total = double.Parse(readValue(reader, "K_total").ToString());
                K_sumAng = double.Parse(readValue(reader, "K_sumAng").ToString());


                #endregion

                #region Positions

                double x, y;
                reader.ReadStartElement("P0");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                P0 = new Position2D(x, y);

                reader.ReadStartElement("P1");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                P1 = new Position2D(x, y);

                reader.ReadStartElement("Q0");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                Q0 = new Position2D(x, y);

                reader.ReadStartElement("Q1");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                Q1 = new Position2D(x, y);

                reader.ReadStartElement("wP0");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                wP0 = new Position2D(x, y);

                reader.ReadStartElement("wP1");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                wP1 = new Position2D(x, y);

                reader.ReadStartElement("wQ0");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                wQ0 = new Position2D(x, y);

                reader.ReadStartElement("wQ1");
                reader.ReadStartElement("X");
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadStartElement("Y");
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();
                reader.ReadEndElement();
                wQ1 = new Position2D(x, y);

                #endregion

                reader.ReadEndElement();
            }
        }

        public static List<ParameterList> GetList()
        {
            return new List<ParameterList>(){ new ParameterList()
            {
                Accel = Accel,
                Decel = Decel,
                aFactor=aFactor,
                Accuercy=Accuercy,
                MaxSpeed=MaxSpeed,
                P0=P0,
                P1=P1,
                Q0=Q0,
                Q1=Q1,
                TunningAngle=TunningAngle,
                TunningDist=TunningDist,
                wAccel=wAccel,
                Waccuercy=Waccuercy,
                WaFactor=WaFactor,
                wDecel=wDecel,
                wMaxS=wMaxS,
                wP0=wP0,
                wP1=wP1,
                wQ0=wQ0,
                wQ1=wQ1,
                Mean=Mean,
                PathWeight=PathWeight,
                Variance=Variance,
                K_ang=K_ang,
                K_lf=K_lf,
                K_sumAng=K_sumAng,
                K_total=K_total,
                K_v0=K_v0,
            
            }};

        }

        private static void writeValue(XmlTextWriter xml, string tagName, object value)
        {
            xml.WriteStartElement(tagName);
            xml.WriteValue(value);
            xml.WriteEndElement();
        }

        private static object readValue(XmlTextReader xml, string tagName)
        {
            xml.ReadStartElement(tagName);
            var ret = xml.ReadContentAsObject();
            xml.ReadEndElement();
            return ret;
        }

    }
}
