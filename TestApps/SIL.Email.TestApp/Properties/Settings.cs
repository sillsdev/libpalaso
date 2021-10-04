using System.Configuration;
using System.Diagnostics;
using SIL.Settings;

namespace SIL.Email.TestApp.Properties
{
	internal sealed partial class Settings
	{

		public Settings()
		{
			//The following loop what is required to get a class to implement a custom settings provider and use it for all settings.
			foreach (SettingsProperty property in Properties)
			{
				Debug.Assert(property.Provider is CrossPlatformSettingsProvider,
					$"Property '{property.Name}' needs the Provider string set to {typeof(CrossPlatformSettingsProvider)}");
			}
		}
	}
}