using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace MRL.SSL.GameDefinitions
{
    public class RotateDetail
    {
        public double VyCoef { get; set; }
        public double Wcoef { get; set; }
    }
    public class RotateParameters
    {
        public static Dictionary<int, Dictionary<int, double>> VyValues = new Dictionary<int, Dictionary<int, double>>();
        public static Dictionary<int, Dictionary<int, double>> OmegaValues = new Dictionary<int, Dictionary<int, double>>();

        public static int RoboID { get; set; }
        public static double Vycoeff { get; set; }
        public static double Omegacoeff { get; set; }
        public static int angle { get; set; }
        public static bool TuneFlag { get; set; }

        private static void writeValue(XmlTextWriter xml, string tagName, object value)
        {
            xml.WriteStartElement(tagName);
            xml.WriteValue(value.ToString());
            xml.WriteEndElement();
        }

        private static object readValue(XmlTextReader xml, string tagName)
        {
            xml.ReadStartElement(tagName);
            var ret = xml.ReadContentAsObject();
            xml.ReadEndElement();
            return ret;
        }
        public static void Save(string fileName)
        {
            if (!fileName.Contains(".xml"))
                fileName += ".xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("RotateParameter");

                //writeValue(writer, "Count", OmegaValues.Count);
                //if (OmegaValues != null && VyValues[RoboID].ContainsKey(angle))
                //{
                //    for (int i = 0; i < OmegaValues.Count; i++)
                //    {
                //        writeValue(writer, "Angle", angle);
                //        writeValue(writer, "Vycoef", VyValues[RoboID][angle]);
                //        writeValue(writer, "Wcoef", OmegaValues[RoboID][angle]);
                //    }
                //}
                for (int i = 0; i < 12; i++)
                {
                    writeValue(writer, "Robot" + i + "30W", OmegaValues[i][30]);
                    writeValue(writer, "Robot" + i + "30V", VyValues[i][30]);
                    writeValue(writer, "Robot" + i + "40W", OmegaValues[i][40]);
                    writeValue(writer, "Robot" + i + "40V", VyValues[i][40]);
                    writeValue(writer, "Robot" + i + "50W", OmegaValues[i][50]);
                    writeValue(writer, "Robot" + i + "50V", VyValues[i][50]);
                    writeValue(writer, "Robot" + i + "60W", OmegaValues[i][60]);
                    writeValue(writer, "Robot" + i + "60V", VyValues[i][60]);
                    writeValue(writer, "Robot" + i + "70W", OmegaValues[i][70]);
                    writeValue(writer, "Robot" + i + "70V", VyValues[i][70]);
                    writeValue(writer, "Robot" + i + "80W", OmegaValues[i][80]);
                    writeValue(writer, "Robot" + i + "80V", VyValues[i][80]);
                    writeValue(writer, "Robot" + i + "90W", OmegaValues[i][90]);
                    writeValue(writer, "Robot" + i + "90V", VyValues[i][90]);
                    writeValue(writer, "Robot" + i + "100W", OmegaValues[i][100]);
                    writeValue(writer, "Robot" + i + "100V", VyValues[i][100]);
                    writeValue(writer, "Robot" + i + "110W", OmegaValues[i][110]);
                    writeValue(writer, "Robot" + i + "110V", VyValues[i][110]);
                    writeValue(writer, "Robot" + i + "120W", OmegaValues[i][120]);
                    writeValue(writer, "Robot" + i + "120V", VyValues[i][120]);
                }

                writer.WriteFullEndElement();
            }
        }
        public static void Save()
        {
            string fileName = "RotateParameters.xml";

            Save(fileName);

        }
        public static void Load(string fileName)
        {
            RotateParameters.VyValues[0] = new Dictionary<int, double>();
            RotateParameters.VyValues[1] = new Dictionary<int, double>();
            RotateParameters.VyValues[2] = new Dictionary<int, double>();
            RotateParameters.VyValues[3] = new Dictionary<int, double>();
            RotateParameters.VyValues[4] = new Dictionary<int, double>();
            RotateParameters.VyValues[5] = new Dictionary<int, double>();
            RotateParameters.VyValues[6] = new Dictionary<int, double>();
            RotateParameters.VyValues[7] = new Dictionary<int, double>();
            RotateParameters.VyValues[8] = new Dictionary<int, double>();
            RotateParameters.VyValues[9] = new Dictionary<int, double>();
            RotateParameters.VyValues[10] = new Dictionary<int, double>();
            RotateParameters.VyValues[11] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[0] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[1] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[2] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[3] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[4] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[5] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[6] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[7] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[8] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[9] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[10] = new Dictionary<int, double>();
            RotateParameters.OmegaValues[11] = new Dictionary<int, double>();
            if (!fileName.Contains(".xml"))
                fileName += ".xml";
            if (!File.Exists(fileName))
                return;

            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                reader.ReadStartElement();

                for (int i = 0; i < 12; i++)
                {

                    OmegaValues[i][30] = double.Parse(readValue(reader, "Robot" + i + "30W").ToString());
                    VyValues[i][30] = double.Parse(readValue(reader, "Robot" + i + "30V").ToString());
                    OmegaValues[i][40] = double.Parse(readValue(reader, "Robot" + i + "40W").ToString());
                    VyValues[i][40] = double.Parse(readValue(reader, "Robot" + i + "40V").ToString());
                    OmegaValues[i][50] = double.Parse(readValue(reader, "Robot" + i + "50W").ToString());
                    VyValues[i][50] = double.Parse(readValue(reader, "Robot" + i + "50V").ToString());
                    OmegaValues[i][60] = double.Parse(readValue(reader, "Robot" + i + "60W").ToString());
                    VyValues[i][60] = double.Parse(readValue(reader, "Robot" + i + "60V").ToString());
                    OmegaValues[i][70] = double.Parse(readValue(reader, "Robot" + i + "70W").ToString());
                    VyValues[i][70] = double.Parse(readValue(reader, "Robot" + i + "70V").ToString());
                    OmegaValues[i][80] = double.Parse(readValue(reader, "Robot" + i + "80W").ToString());
                    VyValues[i][80] = double.Parse(readValue(reader, "Robot" + i + "80V").ToString());
                    OmegaValues[i][90] = double.Parse(readValue(reader, "Robot" + i + "90W").ToString());
                    VyValues[i][90] = double.Parse(readValue(reader, "Robot" + i + "90V").ToString());
                    OmegaValues[i][100] = double.Parse(readValue(reader, "Robot" + i + "100W").ToString());
                    VyValues[i][100] = double.Parse(readValue(reader, "Robot" + i + "100V").ToString());
                    OmegaValues[i][110] = double.Parse(readValue(reader, "Robot" + i + "110W").ToString());
                    VyValues[i][110] = double.Parse(readValue(reader, "Robot" + i + "110V").ToString());
                    OmegaValues[i][120] = double.Parse(readValue(reader, "Robot" + i + "120W").ToString());
                    VyValues[i][120] = double.Parse(readValue(reader, "Robot" + i + "120V").ToString());

                }
                reader.ReadEndElement();

            }
        }
        public static void Load()
        {
            string fileName = "RotateParameters.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            Load(fileName);
        }

        public static void rotatepar()
        {
            RotateParameters.TuneFlag = true;
        }
        public static void set(string VyCoeff, string OmegaCoeff)
        {
            TuneFlag = true;
        }


        public static void Init()
        {

        }
    }


}
