//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Xml;
//using System.Xml.Serialization;

//namespace MRL.SSL.GameDefinitions
//{
//    public static class AISettings
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        public static string LoggerAddress = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SSLLogs");
//        /// <summary>
//        /// 
//        /// </summary>
//        public static bool TechniquesRequest = false;
//        /// <summary>
//        /// 
//        /// </summary>
//        public static Dictionary<string, string> SendTechniques;
//        /// <summary>
//        /// 
//        /// </summary>
//        public static Dictionary<string, string> RecieveTechniques;
//        /// <summary>
//        /// 
//        /// </summary>
//        public static bool NetworkSettingChanged = false;
//        /// <summary>
//        /// Network Setting For Ai
//        /// </summary>
//        public static NetworkSettings NetworkSettings;
//        /// <summary>
//        /// Engines records
//        /// </summary>
//        public static Dictionary<int, Engines> EngineSettings;
//        /// <summary>
//        /// Robots records
//        /// </summary>
//        public static Dictionary<int, int> RobotSettings;
//        /// <summary>
//        /// play names
//        /// </summary>
//        public static List<String> PlayNemes;
//        /// <summary>
//        /// 
//        /// </summary>
//        public static Dictionary<int, string> AllowedPlays;
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="EngineID"></param>
//        /// <param name="engine"></param>
//        public static void Add(int EngineID, Engines engine)
//        {
//            if (EngineSettings == null)
//                EngineSettings = new Dictionary<int, Engines>();
//            if (!EngineSettings.ContainsKey(EngineID))
//                EngineSettings.Add(EngineID, engine);
//            else
//                EngineSettings[EngineID] = engine;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="RobotID"></param>
//        /// <param name="EngineID"></param>
//        public static void Add(int RobotID, int EngineID)
//        {
//            if (RobotSettings == null)
//                RobotSettings = new Dictionary<int, int>();
//            if (!RobotSettings.ContainsKey(RobotID))
//                RobotSettings.Add(RobotID, EngineID);
//            else if (RobotSettings[RobotID] != EngineID)
//                RobotSettings[RobotID] = EngineID;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="playname"></param>
//        public static void Add(string playname)
//        {
//            if (!playname.Contains(playname))
//                PlayNemes.Add(playname);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="EngineID"></param>
//        /// <param name="playName"></param>
//        public static void Add(int EngineID, String playName)
//        {
//            if (AllowedPlays == null)
//                AllowedPlays = new Dictionary<int, string>();
//            if (AllowedPlays.ContainsKey(EngineID))
//                AllowedPlays.Add(EngineID, playName);
//            else
//                AllowedPlays[EngineID] = playName;
//        }
//        /// <summary>
//        /// All data Write To a file
//        /// </summary>
//        /// <param name="address"></param>
//        public static void WriteToFile(string address)
//        {
//            //FileStream Stream = new FileStream(address, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
//            string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AISettings");
//            string SaveFile = Path.Combine(SettingsFolder, address + ".ai");
//            if (!File.Exists(SettingsFolder))
//                System.IO.Directory.CreateDirectory(SettingsFolder);

//            XmlTextWriter _XmlWriter = new XmlTextWriter(SaveFile, null);
//            _XmlWriter.Formatting = Formatting.Indented;
//            _XmlWriter.WriteStartElement("AISettings");

//            #region Write NetworkSettings
//            _XmlWriter.WriteStartElement("NetworkSettings");

//            _XmlWriter.WriteStartElement("AiName");
//            _XmlWriter.WriteString(NetworkSettings.AiName);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("AiPort");
//            _XmlWriter.WriteValue(NetworkSettings.AiPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("CMCName");
//            _XmlWriter.WriteString(NetworkSettings.CMCname);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("CMCPort");
//            _XmlWriter.WriteValue(NetworkSettings.CMCport);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("RefIP");
//            _XmlWriter.WriteString(NetworkSettings.RefIP);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("RefPort");
//            _XmlWriter.WriteValue(NetworkSettings.RefPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SimulatorName");
//            _XmlWriter.WriteString(NetworkSettings.SimulatorName);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SimulatorSendPort");
//            _XmlWriter.WriteValue(NetworkSettings.SimulatorSendPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SimulatorRecievePort");
//            _XmlWriter.WriteValue(NetworkSettings.SimulatorRecievePort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SSLVisionIP");
//            _XmlWriter.WriteString(NetworkSettings.SSLVisionIP);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SSLVisionPort");
//            _XmlWriter.WriteValue(NetworkSettings.SSLVisionPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write EngineSetting
//            _XmlWriter.WriteStartElement("EngineSettings");
//            _XmlWriter.WriteStartElement("RowCount");

//            _XmlWriter.WriteValue(EngineSettings.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (int key in EngineSettings.Keys)
//            {
//                _XmlWriter.WriteStartElement("key");
//                _XmlWriter.WriteValue(key);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("id");
//                _XmlWriter.WriteValue(EngineSettings[key].Id);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("ReverseColor");
//                _XmlWriter.WriteValue(EngineSettings[key].ReverseColor);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("ReverseSide");
//                _XmlWriter.WriteValue(EngineSettings[key].ReverseSide);
//                _XmlWriter.WriteEndElement();
//            }

//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write RobotSetting
//            if (RobotSettings == null)
//                RobotSettings = new Dictionary<int, int>();
//            _XmlWriter.WriteStartElement("RobotSettings");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(RobotSettings.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (int val in RobotSettings.Keys)
//            {
//                _XmlWriter.WriteStartElement("EngineNum");
//                _XmlWriter.WriteValue(val);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("RobotID");
//                _XmlWriter.WriteValue(RobotSettings[val]);
//                _XmlWriter.WriteEndElement();
//            }
//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write PlayNames
//            if (PlayNemes == null)
//                PlayNemes = new List<string>();
//            _XmlWriter.WriteStartElement("PlayNames");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(PlayNemes.Count);
//            _XmlWriter.WriteEndElement();
//            for (int i = 0; i < PlayNemes.Count; i++)
//            {
//                _XmlWriter.WriteStartElement("PlayName");
//                _XmlWriter.WriteString(PlayNemes[i]);
//                _XmlWriter.WriteEndElement();
//            }
//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region WriteAllowedPlays
//            if (AllowedPlays == null)
//                AllowedPlays = new Dictionary<int, string>();
//            _XmlWriter.WriteStartElement("AllowedPlayNames");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(AllowedPlays.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (int key in AllowedPlays.Keys)
//            {
//                _XmlWriter.WriteStartElement("Key");
//                _XmlWriter.WriteValue(key);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("Playname");
//                _XmlWriter.WriteString(AllowedPlays[key]);
//                _XmlWriter.WriteEndElement();
//            }
//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write Techniques
//            if (RecieveTechniques == null)
//                RecieveTechniques = new Dictionary<string, string>();
//            _XmlWriter.WriteStartElement("TechniquesSetting");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(RecieveTechniques.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (string key in RecieveTechniques.Keys)
//            {
//                _XmlWriter.WriteStartElement("Key");
//                _XmlWriter.WriteValue(key);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("Value");
//                _XmlWriter.WriteString(RecieveTechniques[key]);
//                _XmlWriter.WriteEndElement();
//            }

//            _XmlWriter.WriteEndElement();
//            #endregion

//            _XmlWriter.WriteEndElement();

//            _XmlWriter.Flush();
//            _XmlWriter.Close();

//        }
//        /// <summary>
//        /// Read All Data From a file
//        /// </summary>
//        /// <param name="address"></param>
//        public static void ReadFromFile(string address, bool cmc)
//        {
//            NetworkSettings = new NetworkSettings();
//            EngineSettings = new Dictionary<int, Engines>();
//            RobotSettings = new Dictionary<int, int>();
//            PlayNemes = new List<string>();
//            AllowedPlays = new Dictionary<int, string>();
//            RecieveTechniques = new Dictionary<string, string>();

//            FileStream Stream = new FileStream(address, FileMode.Open, FileAccess.Read, FileShare.None);
//            XmlTextReader _XmlReader = new XmlTextReader(Stream);

//            int count = 0;

//            #region Read NetWork Settings
//            _XmlReader.ReadToFollowing("NetworkSettings");
//            _XmlReader.ReadToFollowing("AiName");
//            NetworkSettings.AiName = _XmlReader.ReadElementContentAsString();
//            _XmlReader.ReadToFollowing("AiPort");
//            NetworkSettings.AiPort = _XmlReader.ReadElementContentAsInt();
//            _XmlReader.ReadToFollowing("CMCName");
//            NetworkSettings.CMCname = _XmlReader.ReadElementContentAsString();
//            _XmlReader.ReadToFollowing("CMCPort");
//            NetworkSettings.CMCport = _XmlReader.ReadElementContentAsInt();
//            _XmlReader.ReadToFollowing("RefIP");
//            NetworkSettings.RefIP = _XmlReader.ReadElementContentAsString();
//            _XmlReader.ReadToFollowing("RefPort");
//            NetworkSettings.RefPort = _XmlReader.ReadElementContentAsInt();
//            _XmlReader.ReadToFollowing("SimulatorName");
//            NetworkSettings.SimulatorName = _XmlReader.ReadElementContentAsString();
//            _XmlReader.ReadToFollowing("SimulatorSendPort");
//            NetworkSettings.SimulatorSendPort = _XmlReader.ReadElementContentAsInt();
//            _XmlReader.ReadToFollowing("SimulatorRecievePort");
//            NetworkSettings.SimulatorRecievePort = _XmlReader.ReadElementContentAsInt();
//            _XmlReader.ReadToFollowing("SSLVisionIP");
//            NetworkSettings.SSLVisionIP = _XmlReader.ReadElementContentAsString();
//            _XmlReader.ReadToFollowing("SSLVisionPort");
//            NetworkSettings.SSLVisionPort = _XmlReader.ReadElementContentAsInt();

//            #endregion

//            #region Read Engine
//            _XmlReader.ReadToFollowing("EngineSettings");
//            _XmlReader.ReadToFollowing("RowCount");
//            count = _XmlReader.ReadElementContentAsInt();
//            for (int i = 0; i < count; i++)
//            {
//                Engines e = new Engines();
//                _XmlReader.ReadToFollowing("key");
//                int key = _XmlReader.ReadElementContentAsInt();
//                _XmlReader.ReadToFollowing("id");
//                e.Id = _XmlReader.ReadElementContentAsInt();
//                _XmlReader.ReadToFollowing("ReverseColor");
//                e.ReverseColor = _XmlReader.ReadElementContentAsBoolean();
//                _XmlReader.ReadToFollowing("ReverseSide");
//                e.ReverseSide = _XmlReader.ReadElementContentAsBoolean();
//                EngineSettings.Add(key, e);
//            }
//            #endregion

//            #region read robotsettings
//            _XmlReader.ReadToFollowing("RobotSettings");
//            _XmlReader.ReadToFollowing("RowCount");
//            count = _XmlReader.ReadElementContentAsInt();
//            for (int i = 0; i < count; i++)
//            {
//                _XmlReader.ReadToFollowing("EngineNum");
//                int key = _XmlReader.ReadElementContentAsInt();
//                _XmlReader.ReadToFollowing("RobotID");
//                int id = _XmlReader.ReadElementContentAsInt();
//                RobotSettings.Add(key, id);
//            }
//            #endregion

//            #region Read Play Names
//            _XmlReader.ReadToFollowing("PlayNames");
//            _XmlReader.ReadToFollowing("RowCount");
//            count = _XmlReader.ReadElementContentAsInt();
//            for (int i = 0; i < count; i++)
//            {
//                _XmlReader.ReadToFollowing("PlayName");
//                PlayNemes.Add(_XmlReader.ReadElementContentAsString());
//            }
//            #endregion

//            #region Read Allowed Plays
//            _XmlReader.ReadToFollowing("AllowedPlayNames");
//            _XmlReader.ReadToFollowing("RowCount");
//            count = _XmlReader.ReadElementContentAsInt();
//            for (int i = 0; i < count; i++)
//            {
//                _XmlReader.ReadToFollowing("Key");
//                int key = _XmlReader.ReadElementContentAsInt();
//                _XmlReader.ReadToFollowing("Playname");
//                string playname = _XmlReader.ReadElementContentAsString();
//                AllowedPlays.Add(key, playname);
//            }
//            #endregion

//            #region Read Techniques
//            _XmlReader.ReadToFollowing("TechniquesSetting");
//            _XmlReader.ReadToFollowing("RowCount");
//            count = _XmlReader.ReadElementContentAsInt();
//            for (int i = 0; i < count; i++)
//            {
//                _XmlReader.ReadToFollowing("Key");
//                string key = _XmlReader.ReadElementContentAsString();
//                _XmlReader.ReadToFollowing("Value");
//                string value = _XmlReader.ReadElementContentAsString();
//                RecieveTechniques.Add(key, value);
//            }
//            #endregion

//            #region Read Log Address
//            if (cmc)
//            {
//                _XmlReader.ReadToFollowing("LogAddress");
//                LoggerAddress = _XmlReader.ReadElementContentAsString();
//            }
//            #endregion

//            Stream.Close();
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        public static void WriteToFileCMC()
//        {
//            //FileStream Stream = new FileStream(address, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
//            string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CMCSettings");
//            string SaveFile = Path.Combine(SettingsFolder, "_default.cmc");
//            if (!File.Exists(SettingsFolder))
//                System.IO.Directory.CreateDirectory(SettingsFolder);

//            XmlTextWriter _XmlWriter = new XmlTextWriter(SaveFile, null);
//            _XmlWriter.Formatting = Formatting.Indented;
//            _XmlWriter.WriteStartElement("AISettings");

//            #region Write NetworkSettings
//            _XmlWriter.WriteStartElement("NetworkSettings");

//            _XmlWriter.WriteStartElement("AiName");
//            _XmlWriter.WriteString(NetworkSettings.AiName);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("AiPort");
//            _XmlWriter.WriteValue(NetworkSettings.AiPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("CMCName");
//            _XmlWriter.WriteString(NetworkSettings.CMCname);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("CMCPort");
//            _XmlWriter.WriteValue(NetworkSettings.CMCport);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("RefIP");
//            _XmlWriter.WriteString(NetworkSettings.RefIP);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("RefPort");
//            _XmlWriter.WriteValue(NetworkSettings.RefPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SimulatorName");
//            _XmlWriter.WriteString(NetworkSettings.SimulatorName);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SimulatorSendPort");
//            _XmlWriter.WriteValue(NetworkSettings.SimulatorSendPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SimulatorRecievePort");
//            _XmlWriter.WriteValue(NetworkSettings.SimulatorRecievePort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SSLVisionIP");
//            _XmlWriter.WriteString(NetworkSettings.SSLVisionIP);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteStartElement("SSLVisionPort");
//            _XmlWriter.WriteValue(NetworkSettings.SSLVisionPort);
//            _XmlWriter.WriteEndElement();

//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write EngineSetting
//            _XmlWriter.WriteStartElement("EngineSettings");
//            _XmlWriter.WriteStartElement("RowCount");

//            _XmlWriter.WriteValue(EngineSettings.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (int key in EngineSettings.Keys)
//            {
//                _XmlWriter.WriteStartElement("key");
//                _XmlWriter.WriteValue(key);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("id");
//                _XmlWriter.WriteValue(EngineSettings[key].Id);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("ReverseColor");
//                _XmlWriter.WriteValue(EngineSettings[key].ReverseColor);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("ReverseSide");
//                _XmlWriter.WriteValue(EngineSettings[key].ReverseSide);
//                _XmlWriter.WriteEndElement();
//            }

//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write RobotSetting
//            if (RobotSettings == null)
//                RobotSettings = new Dictionary<int, int>();
//            _XmlWriter.WriteStartElement("RobotSettings");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(RobotSettings.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (int val in RobotSettings.Keys)
//            {
//                _XmlWriter.WriteStartElement("EngineNum");
//                _XmlWriter.WriteValue(val);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("RobotID");
//                _XmlWriter.WriteValue(RobotSettings[val]);
//                _XmlWriter.WriteEndElement();
//            }
//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write PlayNames
//            if (PlayNemes == null)
//                PlayNemes = new List<string>();
//            _XmlWriter.WriteStartElement("PlayNames");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(PlayNemes.Count);
//            _XmlWriter.WriteEndElement();
//            for (int i = 0; i < PlayNemes.Count; i++)
//            {
//                _XmlWriter.WriteStartElement("PlayName");
//                _XmlWriter.WriteString(PlayNemes[i]);
//                _XmlWriter.WriteEndElement();
//            }
//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region WriteAllowedPlays
//            if (AllowedPlays == null)
//                AllowedPlays = new Dictionary<int, string>();
//            _XmlWriter.WriteStartElement("AllowedPlayNames");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(AllowedPlays.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (int key in AllowedPlays.Keys)
//            {
//                _XmlWriter.WriteStartElement("Key");
//                _XmlWriter.WriteValue(key);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("Playname");
//                _XmlWriter.WriteString(AllowedPlays[key]);
//                _XmlWriter.WriteEndElement();
//            }
//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Write Techniques
//            if (RecieveTechniques == null)
//                RecieveTechniques = new Dictionary<string, string>();
//            _XmlWriter.WriteStartElement("TechniquesSetting");

//            _XmlWriter.WriteStartElement("RowCount");
//            _XmlWriter.WriteValue(RecieveTechniques.Count);
//            _XmlWriter.WriteEndElement();

//            foreach (string key in RecieveTechniques.Keys)
//            {
//                _XmlWriter.WriteStartElement("Key");
//                _XmlWriter.WriteValue(key);
//                _XmlWriter.WriteEndElement();

//                _XmlWriter.WriteStartElement("Value");
//                _XmlWriter.WriteString(RecieveTechniques[key]);
//                _XmlWriter.WriteEndElement();
//            }

//            _XmlWriter.WriteEndElement();
//            #endregion

//            #region Logg Address
//            _XmlWriter.WriteStartElement("LogAddress");
//            _XmlWriter.WriteValue(LoggerAddress);
//            _XmlWriter.WriteEndElement();
//            #endregion

//            _XmlWriter.WriteEndElement();

//            _XmlWriter.Flush();
//            _XmlWriter.Close();

//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        public static DribleState[] visPriority = { DribleState.Target, DribleState.Horizontal, DribleState.Vertical };
//    }
//}
