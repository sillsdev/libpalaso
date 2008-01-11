using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using Palaso.Services.Dictionary.SampleClient.Properties;
using Palaso.UI.WindowsForms;
using Palaso.UI.WindowsForms.PropertyBag_NoTests;

namespace Palaso.Services.Dictionary.SampleClient
{
	public partial class SettingsControl : UserControl
	{
		public SettingsControl()
		{
			InitializeComponent();
		}
		[ReadOnly(true)]
		private void Settings_Load(object sender, EventArgs e)
		{
			PropertyBag bag = new PropertyBag();
			bag.SetValue += new PropertySpecEventHandler(OnSetValue);
			bag.GetValue += new PropertySpecEventHandler(OnGetValue);

			PropertySpec spec = new PropertySpec("dictionaryPath", "Dictionary", typeof(PathForPropertyGrid), "Paths", "Path to a dictionary (e.g. .lift).", null, typeof(FilePathEditor), null);
			bag.Properties.Add(spec);
			bag.Properties.Add(new PropertySpec("applicationPath", "Application", typeof(PathForPropertyGrid), "Paths", "Path to application (e.g. WeSayApp.exe)", null, typeof(FilePathEditor), null));
			bag.Properties.Add(new PropertySpec("wordsWritingSytemId", "Vernacular Writing System Id", typeof(string), "Writing Systems", "Writing system id of words"));
			bag.Properties.Add(new PropertySpec("defsWritingSytemId", "Definition Writing System Id", typeof(string), "Writing Systems", "Writing system id of definitions"));

			_settingsGrid.SelectedObject = bag;// Settings.Default;
		}

		void OnGetValue(object sender, PropertySpecEventArgs e)
		{
			switch (e.Property.ID)
			{
				case "dictionaryPath":
					e.Value = new PathForPropertyGrid(Settings.Default.PathToDictionary,"WeSay Dictionary(.lift)| *.lift");
					break;
				case "applicationPath":
					e.Value = new PathForPropertyGrid(Settings.Default.PathToApplication,"Application (*.exe)| *.exe");
					break;
				case "wordsWritingSytemId":
					e.Value = Settings.Default.WritingSystemIdForWords;
					break;
				case "defsWritingSytemId":
					e.Value = Settings.Default.WritingSystemIdForDefinitions;
					break;
			}
		}

		void OnSetValue(object sender, PropertySpecEventArgs e)
		{
			switch (e.Property.ID)
			{
				case "dictionaryPath":
					Settings.Default.PathToDictionary = ((PathForPropertyGrid) e.Value).Path;
					break;
				case "applicationPath":
					Settings.Default.PathToApplication = ((PathForPropertyGrid)e.Value).Path;
					break;
				case "wordsWritingSytemId":
					Settings.Default.WritingSystemIdForWords = e.Value as string;
					break;
				case "defsWritingSytemId":
					Settings.Default.WritingSystemIdForDefinitions = e.Value as string;
					break;
			}

		}
	}

}
