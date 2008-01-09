using System;
using System.Windows.Forms;

namespace Palaso.DictionaryService.SampleClient
{
	static class Program
	{
		public static ServiceMinder serviceMinder;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			serviceMinder = new ServiceMinder();
			Application.Run(new MainWindow());
		}
	}
}