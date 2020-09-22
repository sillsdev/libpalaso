using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using L10NSharp;
using SIL.IO;
using SIL.WritingSystems;

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
			Sldr.Initialize();
			Icu.Wrapper.Init();
			var localizationFolder = Path.GetDirectoryName(FileLocationUtilities.GetFileDistributedWithApplication("Palaso.en.tmx"));
			LocalizationManager.Create(TranslationMemory.Tmx, "fr", "Palaso", "Palaso", "1.0.0", localizationFolder, "SIL/Palaso",
				null, "");
			if(args.Length>0) //for testing commandlinerunner
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

	}
}