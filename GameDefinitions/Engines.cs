using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Drawing;

namespace MRL.SSL.GameDefinitions
{
    [XmlRoot("engine")]
    public class Engines : IXmlSerializable
    {
        /// <summary>
        /// construct engine
        /// </summary>
        public Engines()
        {
        }
        /// <summary>
        /// construct engine with id,color and side
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="ReverseColor">color</param>
        /// <param name="ReverseSide">side</param>
        public Engines(int id,bool ReverseColor, bool ReverseSide)
        {
            _id = id;
            _reverseColor = ReverseColor;
            _reverseSide = ReverseSide;
        }
        private int _id;
        /// <summary>
        /// engine id
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private bool _reverseColor;
        /// <summary>
        /// if be true our color is yellow
        /// </summary>
        public bool ReverseColor
        {
            get { return _reverseColor; }
            set { _reverseColor = value; }
        }
        private bool _reverseSide;
        /// <summary>
        /// if be true our side is reverse
        /// </summary>
        public bool ReverseSide
        {
            get { return _reverseSide; }
            set { _reverseSide = value; }
        }

        public Color GetColor
        {
            get
            {
                return (_reverseColor) ? Color.Yellow : Color.Blue;
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer intReader = new XmlSerializer(typeof(int));
            XmlSerializer boolRaeder = new XmlSerializer(typeof(bool));
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;

            reader.ReadStartElement("id");
            _id = (int)intReader.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("reversecolor");
            _reverseColor = (bool)boolRaeder.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("reverseside");
            _reverseSide = (bool)boolRaeder.Deserialize(reader);
            reader.ReadEndElement();

            
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer intReader = new XmlSerializer(typeof(int));
            XmlSerializer boolRaeder = new XmlSerializer(typeof(bool));


            writer.WriteStartElement("id");
            intReader.Serialize(writer, _id);
            writer.WriteEndElement();

            writer.WriteStartElement("reversecolor");
            boolRaeder.Serialize(writer, _reverseColor);
            writer.WriteEndElement();

            writer.WriteStartElement("reverseside");
            boolRaeder.Serialize(writer, _reverseSide);
            writer.WriteEndElement();

        }

        #endregion
    }    
}
