using LiveDescribe.Model;
using System.Collections.Generic;

namespace LiveDescribe.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings
    {
        public Settings()
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

        /// <summary>
        /// Initializes values to prevent null reference exceptions.
        /// </summary>
        public void InitializeDefaultValuesIfNull()
        {
            if (ColourScheme == null)
                ColourScheme = ColourScheme.DefaultColourScheme;
            if (RecentProjects == null)
                RecentProjects = new List<NamedFilePath>();
            Save();
        }
    }
}
