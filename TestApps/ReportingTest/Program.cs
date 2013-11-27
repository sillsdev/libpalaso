using System;
using System.Windows.Forms;
using Palaso.Reporting;
using TestApp.Properties;

namespace TestApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			  if(Settings.Default.NeedsUpgrade)
			  {
				  Settings.Default.Upgrade();
				  Settings.Default.NeedsUpgrade = false;
				  Settings.Default.Save();
			  }
			  if(Settings.Default.ReportingSettings == null)
			  {
				  Settings.Default.ReportingSettings = new ReportingSettings();
				  Settings.Default.Save();
			  }
			SetupErrorHandling();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new FastXmlSplitterTestForm());
			Application.Run(new Form1());
		}

		private static void SetupErrorHandling()
		{
			Logger.Init();
			ErrorReport.EmailAddress = "nowhere@palaso.org";
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init();
				UsageReporter.Init(Properties.Settings.Default.ReportingSettings, "nowhere.palaso.org", "bogusAccountCode", true);
			  Settings.Default.Save();
		}

	}
}