using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Palaso.Services.Dictionary;
using Palaso.Services.Dictionary.SampleClient.Properties;

namespace Palaso.Services.Dictionary.SampleClient
{
	public partial class MainWindow : Form
	{
		public static DictionaryAccessor _dictionaryAccessor;
		public static MainWindow Logger;

		public MainWindow()
		{
			InitializeComponent();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			_dictionaryAccessor.Dispose();
			base.OnClosing(e);
		}

		private void Main_Load(object sender, EventArgs e)
		{
			Logger = this;

			LoadFromSettings();
			if (_dictionaryAccessor == null)
			{
				_tabControl.SelectedTab = _settingsTabPage;
			}
		}

		private void LoadFromSettings()
		{
			if(_dictionaryAccessor!=null)
			{
				_dictionaryAccessor.Dispose();
			}
			_dictionaryAccessor = null;

			bool dictionaryPathLooksReasonable = !string.IsNullOrEmpty(Settings.Default.PathToDictionary) &&
												 File.Exists(Settings.Default.PathToDictionary);
			bool applicationPathLooksReasonable = !string.IsNullOrEmpty(Settings.Default.PathToApplication) &&
												  File.Exists(Settings.Default.PathToApplication);
			if (dictionaryPathLooksReasonable && applicationPathLooksReasonable)
			{
				this.Text = Path.GetFileName(Settings.Default.PathToDictionary) + " - " + Application.ProductName;
				_dictionaryAccessor = new DictionaryAccessor(Settings.Default.PathToDictionary,Settings.Default.PathToApplication);
				_dictionaryAccessor.ErrorLog += Log;
			}
			else
			{
				this.Text = "Please Check Dictionary Path - " + Application.ProductName;
			}
			//it's ok if these are null
			((LookupControl)_lookupTabPage.Controls[0]).DictionaryAccessor = _dictionaryAccessor;
			((AddEntry)_addEntryTabPage.Controls[0]).DictionaryAccessor = _dictionaryAccessor;
		}

		public void Log(string s, params object[] args)
		{
			_logText.Text = string.Format(s, args);
		}

		private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			//only really needed if switching from settings tab, but it'll be fast
			LoadFromSettings();
		}


	}
}