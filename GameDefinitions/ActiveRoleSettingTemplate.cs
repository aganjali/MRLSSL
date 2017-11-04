using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MRL.SSL.GameDefinitions
{
    public class ActiveRoleSettingTemplate :IXmlSerializable
    {
        public int RobotID { get; set; }

        public Dictionary<string, double> propeties = new Dictionary<string, double>();

        public double this[string key]
        {
            get 
            {
                return propeties[key];
            }
            set 
            {
                propeties[key] = value;
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer ReaderD = new XmlSerializer(typeof(double));
            XmlSerializer ReaderI = new XmlSerializer(typeof(int));
            XmlSerializer ReaderS = new XmlSerializer(typeof(string));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            int count = 0;
            propeties = new Dictionary<string, double>();

            reader.ReadStartElement("ID");
            RobotID = (int)ReaderI.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("Count");
            count = (int)ReaderI.Deserialize(reader);
            reader.ReadEndElement();

            for (int i = 0; i < count; i++)
            {
                
                reader.ReadStartElement("Name");
                string key= (string)ReaderS.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Value");
                double val = (double)ReaderD.Deserialize(reader);
                reader.ReadEndElement();

                propeties[key] = val;
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer writeD = new XmlSerializer(typeof(double));
            XmlSerializer writeS = new XmlSerializer(typeof(string));
            XmlSerializer writeI = new XmlSerializer(typeof(int));

            writer.WriteStartElement("ID");
            writeI.Serialize(writer, RobotID);
            writer.WriteEndElement();

            writer.WriteStartElement("Count");
            writeI.Serialize(writer, propeties.Count);
            writer.WriteEndElement();
            
            foreach (var item in propeties)
            {
                writer.WriteStartElement("Name");
                writeS.Serialize(writer, item.Key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                writeD.Serialize(writer, item.Value);
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
