using System;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.TestApp
{
	public static class Program
	{
		internal static ILocalizationManager PrimaryL10NManager;

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
				preferredUILocale = args[0].ToLowerInvariant();
			}

			var localizationFolder = Path.GetDirectoryName(
				FileLocationUtilities.GetFileDistributedWithApplication($"Palaso.{preferredUILocale}.xlf"));
			PrimaryL10NManager = LocalizationManager.Create(TranslationMemory.XLiff, preferredUILocale, "Palaso", "Palaso",
				"1.0.0", localizationFolder, "SIL/Palaso", null, "testapp@sil.org");

			Application.Run(new TestAppForm());

			Sldr.Cleanup();
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.EmailAddress = "bogus_test_app_email_addr@sil.org";
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init(new WinFormsExceptionHandler());
		}
	}
}