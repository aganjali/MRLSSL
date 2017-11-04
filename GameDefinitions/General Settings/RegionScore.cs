using System.IO;
namespace MRL.SSL.GameDefinitions.General_Settings
{


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class RegionScore
    {

        public RegionScore()
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

        public static void Save(string path)
        {
            GoogleSerializer gs = new GoogleSerializer();
            gs.SerializeRegionScore();
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(gs.stream.ToArray(), 0, gs.stream.ToArray().Length);
            }
        }

        public static void Load(string path)
        {
            GoogleSerializer gs = new GoogleSerializer();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[(int)fs.Length];
                fs.Read(data, 0, (int)fs.Length);

                gs.stream = new MemoryStream(data);
                gs.stream.Position = 0;
                gs.DeserializeRegionScore();
            }
        }
    }
}
