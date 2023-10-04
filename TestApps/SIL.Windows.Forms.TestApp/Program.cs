using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using L10NSharp;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;
using SIL.WritingSystems;
using static System.StringComparison;

namespace SIL.Windows.Forms.TestApp
{
	public static class Program
	{
		internal static ILocalizationManager PrimaryL10NManager;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			SetUpErrorHandling();

			Sldr.Initialize();
			Icu.Wrapper.Init();

			var preferredUILocale = "fr";
			if (args.Length > 0)
			{
				if (args[0].Equals("es", OrdinalIgnoreCase))
				{
					preferredUILocale = "es";
				}
				else
				{
					// Test command-line runner's ability to read console output and time out.
					for (var i = 0; i < 10; i++)
					{
						Console.WriteLine(i);
						Thread.Sleep(1000);
					}
					return;
				}
			}

			var localizationFolder = Path.GetDirectoryName(
				FileLocationUtilities.GetFileDistributedWithApplication($"Palaso.{preferredUILocale}.xlf"));
			PrimaryL10NManager = LocalizationManager.Create(TranslationMemory.XLiff, preferredUILocale, "Palaso", "Palaso",
				"1.0.0", localizationFolder, "SIL/Palaso", null, "testapp@sil.org");

			Application.Run(new TestAppForm());

			Sldr.Cleanup();
		}

		/// ------------------------------------------------------------------------------------
		private static void SetUpErrorHandling()
		{
			ErrorReport.EmailAddress = "bogus_test_app_email_addr@sil.org";
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init(new WinFormsExceptionHandler());
		}
	}
}