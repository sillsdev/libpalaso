using L10NSharp;
using L10NSharp.Windows.Forms;
using SIL.IO;

namespace ArchivingTestApp
{
	internal static class Program
	{
		internal static ILocalizationManager? PrimaryL10NManager;

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			//ApplicationConfiguration.Initialize();

			var preferredUILocale = "en";
			if (args.Length > 0)
				preferredUILocale = args[0].ToLowerInvariant();

			var localizationFolder = Path.GetDirectoryName(
				FileLocationUtilities.GetFileDistributedWithApplication($"Palaso.{preferredUILocale}.xlf"));
			PrimaryL10NManager = LocalizationManagerWinforms.Create(preferredUILocale, "Palaso", "Palaso",
				"1.0.0", localizationFolder, "SIL/Palaso", null, ["SIL."]);

			Application.Run(new MainForm());
		}
	}
}