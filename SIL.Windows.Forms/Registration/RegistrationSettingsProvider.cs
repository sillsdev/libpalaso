using System;
using SIL.Settings;

namespace SIL.Windows.Forms.Registration
{
	/// <summary>
	/// This class is used to store the settings for the Registration object.
	/// In addition to the benefits of using the CrossPlatformSettingsProvider, it allows control
	/// of the string used as the application name. This allows applications like Bloom, which
	/// support parallel installation of different 'channel' versions of the product, to have them
	/// all share a single location for these settings. This in turn saves the user from having
	/// to register separately for each channel, and prevents us from counting the same user
	/// multiple times in different channels.
	/// </summary>
	[CLSCompliant (false)]
	public class RegistrationSettingsProvider : CrossPlatformSettingsProvider
	{
		public RegistrationSettingsProvider()
		{
			RenamedSections["Palaso.UI.WindowsForms.Registration.Registration"] = "SIL.Windows.Forms.Registration.Registration";
		}

		private static string _productName ;
		public static void SetProductName(string product)
		{
			_productName = product;
		}

		protected override string ProductName
		{
			get { return _productName; }
		}
	}
}
