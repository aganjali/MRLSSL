using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MRL.SSL.GameDefinitions.Visualizer_Classes
{
    public class ChipKickInfo
    {
        public int Power { get; set; }
        public double Length { get; set; }
        public double SafeRadi { get; set; }
        public bool HasSpin { get; set; }
        public double Time { get; set; }
        public bool BackSensore { get; set; }
    }
    public class MetricChipKick : IXmlSerializable
    {
        public List<ChipKickInfo> KickInfo = new List<ChipKickInfo>();

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer Reader = new XmlSerializer(typeof(double));
            XmlSerializer Reader2 = new XmlSerializer(typeof(int));
            XmlSerializer Reader3 = new XmlSerializer(typeof(bool));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            int count = 0;
            KickInfo = new List<ChipKickInfo>();
            reader.ReadStartElement("Count");
            count = (int)Reader2.Deserialize(reader);
            reader.ReadEndElement();
            for (int i = 0; i < count; i++)
			{
                ChipKickInfo ock = new ChipKickInfo();
                reader.ReadStartElement("Power");
                ock.Power = (int)Reader2.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Lenght");
                ock.Length = (double)Reader.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("SafeRadi");
                ock.SafeRadi = (double)Reader.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("HasSpine");
                ock.HasSpin = (bool)Reader3.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Time");
                ock.Time = (double)Reader.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("BackSensore");
                ock.BackSensore = (bool)Reader3.Deserialize(reader);
                reader.ReadEndElement();


                KickInfo.Add(ock);
            }            
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer Raeder = new XmlSerializer(typeof(double));
            XmlSerializer Raeder2 = new XmlSerializer(typeof(int));
            XmlSerializer Raeder3 = new XmlSerializer(typeof(bool));


            writer.WriteStartElement("Count");
            Raeder2.Serialize(writer, KickInfo.Count);
            writer.WriteEndElement();
            foreach (var item in KickInfo)
            {
                writer.WriteStartElement("Power");
                Raeder2.Serialize(writer, item.Power);
                writer.WriteEndElement();

                writer.WriteStartElement("Lenght");
                Raeder.Serialize(writer, item.Length);
                writer.WriteEndElement();

                writer.WriteStartElement("SafeRadi");
                Raeder.Serialize(writer, item.SafeRadi);
                writer.WriteEndElement();

                writer.WriteStartElement("HasSpine");
                Raeder3.Serialize(writer, item.HasSpin);
                writer.WriteEndElement();

                writer.WriteStartElement("Time");
                Raeder.Serialize(writer, item.Time);
                writer.WriteEndElement();

                writer.WriteStartElement("BackSensore");
                Raeder3.Serialize(writer, item.BackSensore);
                writer.WriteEndElement();
            }
            
        }

        #endregion
    }
}
