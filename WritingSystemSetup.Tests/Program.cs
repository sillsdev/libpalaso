using System;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;
using WritingSystemSetup.Tests;

namespace Palaso
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
		   // Application.Run(new SuperToolTipTestForm());
		  //  Application.Run(new Palaso.UI.WritingSystems.WSListDialog());
			Application.Run(new WritingSystemChooserTestForm());
	  //      Application.Run(new LookupISOCodeDialog());
		}
	}
}