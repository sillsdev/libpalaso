using System;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.Windows.Forms;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.TestApp
{
	public static class Program
	{
		internal const string kSupportEmailAddress = "bogus_test_app_email_addr@sil.org";
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
			PrimaryL10NManager = LocalizationManagerWinforms.Create(preferredUILocale, "Palaso", "Palaso",
				"1.0.0", localizationFolder, "SIL/Palaso", null, new [] {"SIL."});

			Application.Run(new TestAppForm());

			Sldr.Cleanup();
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.EmailAddress = kSupportEmailAddress;
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init(new WinFormsExceptionHandler());
		}
	}
}