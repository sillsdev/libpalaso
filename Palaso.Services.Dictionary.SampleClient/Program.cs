using System;
using System.Windows.Forms;
using Palaso.Services.Dictionary.SampleClient;
using Palaso.Services.Dictionary.SampleClient.Properties;

namespace Palaso.Services.Dictionary.SampleClient
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