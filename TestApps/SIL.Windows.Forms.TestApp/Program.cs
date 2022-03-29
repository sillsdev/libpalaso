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
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			SetUpErrorHandling();

			Sldr.Initialize();
			Icu.Wrapper.Init();

			var testCommandLineRunner = false;
			var distFilesEnglishStrings = "Palaso.en.tmx";
			var localizationType = TranslationMemory.Tmx;
			var preferredUILocale = "fr";
			if (args.Length > 0)
			{
				if (args[0].Equals(TranslationMemory.XLiff.ToString(), OrdinalIgnoreCase))
				{
					distFilesEnglishStrings =
						Path.ChangeExtension(distFilesEnglishStrings, "xlf");
					localizationType = TranslationMemory.XLiff;
					preferredUILocale = "es";
				}
				else
				{
					testCommandLineRunner = true;
				}
			}

			var localizationFolder = Path.GetDirectoryName(
				FileLocationUtilities.GetFileDistributedWithApplication(distFilesEnglishStrings));
			LocalizationManager.Create(localizationType, preferredUILocale, "Palaso", "Palaso",
				"1.0.0", localizationFolder, "SIL/Palaso", null, "");

			if (testCommandLineRunner)
			{
				for (int i = 0; i < 10; i++)
				{
					Console.WriteLine(i);
					Thread.Sleep(1000);
				}
				return;
			}

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