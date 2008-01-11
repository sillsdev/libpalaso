using System;
using System.Windows.Forms;
using Palaso.DictionaryService.SampleClient.Properties;

namespace Palaso.DictionaryService.SampleClient
{
	static class Program
	{

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);


			Application.Run(new MainWindow());
			Settings.Default.Save();
		}
	}
}