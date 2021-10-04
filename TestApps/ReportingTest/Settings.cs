using System.Configuration;
using System.Diagnostics;
using SIL.Settings;

namespace TestApp.Properties {


	// This class allows you to handle specific events on the settings class:
	//  The SettingChanging event is raised before a setting's value is changed.
	//  The PropertyChanged event is raised after a setting's value is changed.
	//  The SettingsLoaded event is raised after the setting values are loaded.
	//  The SettingsSaving event is raised before the setting values are saved.
	internal sealed partial class Settings {

		public Settings()
		{
			//The following loop what is required to get a class to implement a custom settings provider and use it for all settings.
			foreach(SettingsProperty property in Properties)
			{
				Debug.Assert(property.Provider is CrossPlatformSettingsProvider,
					$"Property '{property.Name}' needs the Provider string set to {typeof(CrossPlatformSettingsProvider)}");
			}

			// // To add event handlers for saving and changing settings, uncomment the lines below:
			//
			// this.SettingChanging += this.SettingChangingEventHandler;
			//
			// this.SettingsSaving += this.SettingsSavingEventHandler;
			//
	  }

		private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
			// Add code to handle the SettingChangingEvent event here.
		}

		private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
			// Add code to handle the SettingsSaving event here.
		}
	}
}
