using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MRL.SSL.GameDefinitions
{
    public class ChipKick : IXmlSerializable
    {
        public ChipKick(double time, double range, int power,int robotid)
        {
            Time = time;
            Range = range;
            Power = power;
            RobotID = robotid;
            
        }
        public ChipKick()
        {
        }

        public double Time { get; set; }
        public double Range { get; set; }
        public int Power { get; set; }
        public int RobotID { get; set; }


        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer Reader = new XmlSerializer(typeof(double));
            XmlSerializer Reader2 = new XmlSerializer(typeof(int));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;

            reader.ReadStartElement("Power");
            Power = (int)Reader2.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("Time");
            Time = (double)Reader.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("Range");
            Range = (double)Reader.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("RobotID");
            RobotID = (int)Reader2.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer Raeder = new XmlSerializer(typeof(double));
            XmlSerializer Raeder2 = new XmlSerializer(typeof(int));


            writer.WriteStartElement("Power");
            Raeder2.Serialize(writer, Power);
            writer.WriteEndElement();

            writer.WriteStartElement("Time");
            Raeder.Serialize(writer, Time);
            writer.WriteEndElement();

            writer.WriteStartElement("Range");
            Raeder.Serialize(writer, Range);
            writer.WriteEndElement();

            writer.WriteStartElement("RobotID");
            Raeder2.Serialize(writer, RobotID);
            writer.WriteEndElement();

        }

        #endregion
    }
}
