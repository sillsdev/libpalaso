using System;
using System.Windows.Forms;
using Palaso.DictionaryService.SampleClient.Properties;

namespace Palaso.DictionaryService.SampleClient
{
	public partial class SettingsControl : UserControl
	{
		public SettingsControl()
		{
			InitializeComponent();
		}

		private void Settings_Load(object sender, EventArgs e)
		{
			_settingsGrid.SelectedObject = Settings.Default;
		}
	}
}
