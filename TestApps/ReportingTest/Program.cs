using System;
using System.Windows.Forms;
using Palaso.Reporting;

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
			SetupErrorHandling();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		private static void SetupErrorHandling()
		{
			Logger.Init();
			ErrorReport.EmailAddress = "nowhere@palaso.org";
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init();
			UsageReporter.Init(new ReportingSettings(), "nowhere.palaso.org", "bogusAccountCode", true );
		}

	}
}