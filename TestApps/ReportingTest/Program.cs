using System;
using System.Windows.Forms;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;
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
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

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
			//Application.Run(new FastXmlSplitterTestForm());
			Application.Run(new Form1());
		}

		private static void SetupErrorHandling()
		{
			Logger.Init();
			ErrorReport.EmailAddress = "nowhere@palaso.org";
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init(new WinFormsExceptionHandler());
			UsageReporter.Init(Properties.Settings.Default.ReportingSettings, "nowhere.palaso.org", "bogusAccountCode", true);
			Settings.Default.Save();
		}

	}
}