using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Xml.Serialization;

namespace MRL.SSL.GameDefinitions
{
    [XmlRoot("score")]
    public class Score : IXmlSerializable
    {
        
        public Position2D Robot { get; set; }
        public int Region { get; set; }
        public double PosScore { get; set; }

        public Score Clone()
        {
            return new Score() { Region = this.Region, Robot = this.Robot, PosScore = this.PosScore };
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer Reader = new XmlSerializer(typeof(double));
            XmlSerializer Readerint = new XmlSerializer(typeof(int));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;

            reader.ReadStartElement("X");
            double x = (double)Reader.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("Y");
            double y = (double)Reader.Deserialize(reader);
            reader.ReadEndElement();


            reader.ReadStartElement("region");
            Region = (int)Readerint.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("posScore");
            PosScore = (double)Reader.Deserialize(reader);
            reader.ReadEndElement();



            reader.ReadEndElement();

            Robot = new Position2D(x, y);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer Raeder = new XmlSerializer(typeof(double));
            XmlSerializer Raederint = new XmlSerializer(typeof(int));

            writer.WriteStartElement("X");
            Raeder.Serialize(writer, Robot.X);
            writer.WriteEndElement();

            writer.WriteStartElement("Y");
            Raeder.Serialize(writer, Robot.Y);
            writer.WriteEndElement();

            writer.WriteStartElement("region");
            Raederint.Serialize(writer, Region);
            writer.WriteEndElement();

            writer.WriteStartElement("posScore");
            Raeder.Serialize(writer, PosScore);
            writer.WriteEndElement();

        }

        #endregion
    }
}
