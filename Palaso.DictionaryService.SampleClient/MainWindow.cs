using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.DictionaryService.SampleClient.Properties;

namespace Palaso.DictionaryService.SampleClient
{
	public partial class MainWindow : Form
	{
		public static MainWindow Logger;
		public MainWindow()
		{
			InitializeComponent();
			Logger = this;
		}

		private void Main_Load(object sender, EventArgs e)
		{
			_dictionaryPathLabel.Text = Settings.Default.PathToDictionary;
		}

		public void Log(string s, params object[] args)
		{
			_logText.Text = string.Format(s, args);
		}


	}

}