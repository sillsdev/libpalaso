using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.ImageGallery;
using Palaso.UI.WindowsForms.SIL;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace PalasoUIWindowsForms.TestApp
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

			if(args.Length>0) //for testing commandlinerunner
			{
				for (int i = 0; i < 10; i++)
				{
					Console.WriteLine(i);
					Thread.Sleep(1000);
				}
				return;
			}

#if  TESTING_FolderBrowserControl
			var form = new Form();
			var browser = new Palaso.UI.WindowsForms.FolderBrowserControl.FolderBrowserControl();
			browser.Location = new Point(0,0);
			browser.Width = form.ClientSize.Width;
			browser.Height = form.ClientSize.Height;
			browser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			browser.ShowOnlyMappedDrives = false;
			browser.ShowAddressbar = true;
			form.Controls.Add(browser);
			form.ShowDialog();
			return;
#endif

#if  TESTING_ISOLookup
			var dialog = new LookupISOCodeDialog();
			dialog.ISOCode = "etr";
			Application.Run(dialog);
#endif

//#if  TESTING_WS
			string tempPath = Path.GetTempPath() + "WS-Test";
			Directory.CreateDirectory(tempPath);
			try
			{
				Application.Run(new WritingSystemSetupDialog(tempPath, onMigration, onLoadProblem));
			}
			catch (Exception)
			{
				throw;
			}
//#endif
#if TESTING_ARTOFREADING
			var images = new ArtOfReadingImageCollection();
			images.LoadIndex(@"C:\palaso\output\debug\ImageGallery\artofreadingindexv3_en.txt");
			images.RootImagePath = @"c:\art of reading\images";
			var form = new PictureChooser(images, "duck");
			Application.Run(form);
			Console.WriteLine("REsult: " + form.ChosenPath);
#endif
		}

		public static void onMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}

		public static void onLoadProblem(IEnumerable<WritingSystemRepositoryProblem> migrationInfo)
		{
		}


	}
}