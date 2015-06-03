using System;
using System.Threading;
using System.Windows.Forms;

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

	   }



	}
}