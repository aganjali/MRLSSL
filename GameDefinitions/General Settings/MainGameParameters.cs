using System.IO;
using System.Xml;
using System.Configuration;
namespace MRL.SSL.GameDefinitions.General_Settings
{


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class MainGameParameters
    {

        public MainGameParameters()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Add code to handle the SettingsSaving event here.
        }

        public void SaveToFile(string path)
        {
            FileStream Stream = new FileStream(path + ".AiParams", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter _XmlWriter = new XmlTextWriter(Stream, null);
            _XmlWriter.Formatting = Formatting.Indented;
            _XmlWriter.WriteStartElement("Params");

            foreach (SettingsProperty item in MainGameParameters.Default.Properties)
            {
                _XmlWriter.WriteStartElement("Name");
                _XmlWriter.WriteString(item.Name);
                _XmlWriter.WriteEndElement();

                _XmlWriter.WriteStartElement("Value");
                _XmlWriter.WriteValue(MainGameParameters.Default[item.Name]);
                _XmlWriter.WriteEndElement();
            }
            _XmlWriter.WriteEndElement();

            _XmlWriter.Flush();
            _XmlWriter.Close();

        }

        public void LoadFromFile(string path)
        {
            FileStream Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            XmlTextReader _XmlReader = new XmlTextReader(Stream);
            _XmlReader.ReadStartElement("Params");
            while (_XmlReader.NodeType != XmlNodeType.EndElement)
            {
                _XmlReader.ReadToFollowing("Name");
                string name = _XmlReader.ReadElementContentAsString();


                _XmlReader.ReadToFollowing("Value");
                object val = _XmlReader.ReadElementContentAsObject();

                if (val is int)
                    MainGameParameters.Default[name] = (int)val;
                else if (val is double)
                    MainGameParameters.Default[name] = (double)val;
                else if (val is string)
                    MainGameParameters.Default[name] = (string)val;
                else if (val is bool)
                    MainGameParameters.Default[name] = (bool)val;
            }
        }
    }
}
