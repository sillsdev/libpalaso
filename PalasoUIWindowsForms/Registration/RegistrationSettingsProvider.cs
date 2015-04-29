using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Settings;

namespace Palaso.UI.WindowsForms.Registration
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
	public class RegistrationSettingsProvider : CrossPlatformSettingsProvider
	{
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
