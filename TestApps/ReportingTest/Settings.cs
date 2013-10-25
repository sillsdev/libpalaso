using System.Configuration;
using Palaso.Settings;

namespace TestApp.Properties {


	// This class allows you to handle specific events on the settings class:
	//  The SettingChanging event is raised before a setting's value is changed.
	//  The PropertyChanged event is raised after a setting's value is changed.
	//  The SettingsLoaded event is raised after the setting values are loaded.
	//  The SettingsSaving event is raised before the setting values are saved.
	internal sealed partial class Settings {

		public Settings()
		{
			//The following 6 lines of code are what is required to get a class to implement a custom settings provider and use it for all settings.
			var provider = new CrossPlatformSettingsProvider();
			_providers = new SettingsProviderCollection();
			_providers.Add(provider);
			foreach(SettingsProperty property in this.Properties)
			{
				property.Provider = provider;
			}
		 // // To add event handlers for saving and changing settings, uncomment the lines below:
		 //
		 // this.SettingChanging += this.SettingChangingEventHandler;
		 //
		 // this.SettingsSaving += this.SettingsSavingEventHandler;
		 //
	  }

		private static SettingsProviderCollection _providers;

		public override SettingsProviderCollection Providers
		{
			get
			{
				return _providers;
			}
		}

		private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
			// Add code to handle the SettingChangingEvent event here.
		}

		private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
			// Add code to handle the SettingsSaving event here.
		}
	}
}
