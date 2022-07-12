using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.SettingProtection
{
	/// <summary>
	/// This control will hide and challenge for a password, as appropriate.
	/// You can also make a custom control which fits better for your application, and use the SettingsProtectionHelper just like this does.
	/// </summary>
	public partial class SettingsLauncherButton : UserControl
	{
		private readonly SettingsProtectionHelper _helper;

		public SettingsLauncherButton()
		{
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			InitializeComponent();
			betterLinkLabel1.Click += OnLinkClicked;
			_helper = new SettingsProtectionHelper(Container);
			_helper.SetSettingsProtection(this, true);
		}

		/// <summary>
		/// Provide a method which launches your settings dialog/application/etc.
		/// It will be called when the user clicks the link (and potentially enters the password, etc.)
		/// </summary>
		public Func<DialogResult> LaunchSettingsCallback { get; set; }


		private void OnLinkClicked(object sender, EventArgs e)
		{
			_helper.LaunchSettingsIfAppropriate(() => LaunchSettingsCallback());
		}
	}
}
