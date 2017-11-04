using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace MRL.SSL.GameDefinitions
{
    public static class PlaySettings
    {
        /// <summary>
        /// strategy name
        /// </summary>
        public static string Name = "Normal";
        /// <summary>
        /// our kick off strategy
        /// </summary>
        public static OurKickOff OurKickOffMode = OurKickOff.Direct;
        /// <summary>
        /// Our Corner Strategy
        /// </summary>
        public static OurCornerKick OurCornerKickMode = OurCornerKick.Heading;
        /// <summary>
        /// our corner kick strategy
        /// </summary>
        public static OurPenaltyGoaller OurPenaltyGoaller = OurPenaltyGoaller.MoveByOppAngle;
        /// <summary>
        /// our indirectfreekick strategy
        /// </summary>
        public static OurIndirectFreeKick OurIndirectFreeKick = OurIndirectFreeKick.Normal;
        /// <summary>
        /// our indirectfreekick strategy
        /// </summary>
        public static OurIndirectFreeKickMiddle OurIndirectFreeKickMiddle = OurIndirectFreeKickMiddle.Normal;
        /// <summary>
        /// our indirectfreekick strategy
        /// </summary>
        public static OurIndirectFreeKickOpponent OurIndirectFreeKickOpponent = OurIndirectFreeKickOpponent.Normal;
        /// <summary>
        /// Our Normal Game Strategy
        /// </summary>
        public static OurNormalGame OurNormalGame = OurNormalGame.Normal;
        /// <summary>
        /// Save PlaySetting in CMCSettings and AISettings
        /// </summary>
        public static void Save()
        {
            #region Save Into AISetting
            {
                string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AISettings");
                string SaveFile = Path.Combine(SettingsFolder, "PlaySetting.TAC");
                if (!File.Exists(SettingsFolder))
                    System.IO.Directory.CreateDirectory(SettingsFolder);
                XmlTextWriter _XmlWriter = new XmlTextWriter(SaveFile, null);
                _XmlWriter.Formatting = Formatting.Indented;

                _XmlWriter.WriteStartElement("PlaySetting");//Start

                _XmlWriter.WriteStartElement("OurKickOff");
                _XmlWriter.WriteValue((int)OurKickOffMode);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurCornerKick");
                _XmlWriter.WriteValue((int)OurCornerKickMode);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurPenaltyGoaller");
                _XmlWriter.WriteValue((int)OurPenaltyGoaller);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurIndirectFreeKick");
                _XmlWriter.WriteValue((int)OurIndirectFreeKick);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurIndirectFreeKickMiddle");
                _XmlWriter.WriteValue((int)OurIndirectFreeKickMiddle);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurIndirectFreeKickOpponent");
                _XmlWriter.WriteValue((int)OurIndirectFreeKickOpponent);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurNormalGame");
                _XmlWriter.WriteValue((int)OurNormalGame);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteEndElement();//End

                _XmlWriter.Flush();
                _XmlWriter.Close();
            }
            #endregion

            #region CMCSetting
            {
                string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CMCSettings");
                string SaveFile = Path.Combine(SettingsFolder, Name + ".TAC");
                if (!File.Exists(SettingsFolder))
                    System.IO.Directory.CreateDirectory(SettingsFolder);
                XmlTextWriter _XmlWriter = new XmlTextWriter(SaveFile, null);
                _XmlWriter.Formatting = Formatting.Indented;

                _XmlWriter.WriteStartElement("PlaySetting");//Start

                _XmlWriter.WriteStartElement("OurKickOff");
                _XmlWriter.WriteValue((int)OurKickOffMode);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurCornerKick");
                _XmlWriter.WriteValue((int)OurCornerKickMode);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurPenaltyGoaller");
                _XmlWriter.WriteValue((int)OurPenaltyGoaller);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurIndirectFreeKick");
                _XmlWriter.WriteValue((int)OurIndirectFreeKick);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurIndirectFreeKickMiddle");
                _XmlWriter.WriteValue((int)OurIndirectFreeKickMiddle);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurIndirectFreeKickOpponent");
                _XmlWriter.WriteValue((int)OurIndirectFreeKickOpponent);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("OurNormalGame");
                _XmlWriter.WriteValue((int)OurNormalGame);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteEndElement();//End

                _XmlWriter.Flush();
                _XmlWriter.Close();
            }
            #endregion

        }
        /// <summary>
        /// Load PlaySettings By File Address
        /// </summary>
        public static void Load(string Address)
        {
            FileStream Stream = new FileStream(Address, FileMode.Open, FileAccess.Read, FileShare.None);
            XmlTextReader _XmlReader = new XmlTextReader(Stream);

            _XmlReader.ReadToFollowing("PlaySetting");//Start

            _XmlReader.ReadToFollowing("OurKickOff");
            OurKickOffMode = (OurKickOff)_XmlReader.ReadElementContentAsInt();

            _XmlReader.ReadToFollowing("OurCornerKick");
            OurCornerKickMode = (OurCornerKick)_XmlReader.ReadElementContentAsInt();

            _XmlReader.ReadToFollowing("OurPenaltyGoaller");
            OurPenaltyGoaller = (OurPenaltyGoaller)_XmlReader.ReadElementContentAsInt();

            _XmlReader.ReadToFollowing("OurIndirectFreeKick");
            OurIndirectFreeKick = (OurIndirectFreeKick)_XmlReader.ReadElementContentAsInt();

            _XmlReader.ReadToFollowing("OurIndirectFreeKickMiddle");
            OurIndirectFreeKickMiddle = (OurIndirectFreeKickMiddle)_XmlReader.ReadElementContentAsInt();

            _XmlReader.ReadToFollowing("OurIndirectFreeKickOpponent");
            OurIndirectFreeKickOpponent = (OurIndirectFreeKickOpponent)_XmlReader.ReadElementContentAsInt();

            _XmlReader.ReadToFollowing("OurNormalGame");
            OurNormalGame = (OurNormalGame)_XmlReader.ReadElementContentAsInt();

            Stream.Close();
        }
    }
}
