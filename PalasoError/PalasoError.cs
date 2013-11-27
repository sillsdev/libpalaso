using System;
using System.Windows.Forms;
using Palaso.Reporting;

namespace Palaso.Error
{
	static class Program
	{
		/// <summary>
		/// PalasoError isLethal fileName emailAddress emailSubject
		 /// isLethal - 0 non-fatal error
		 ///            1 fatal error, program quits
		 /// fileName - path to the saved error log
		 /// emailAddress - address to set up the email to send the log to
		 /// emailSubject - subject of the generated email
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			if (args.Length != 4)
			{
				Console.WriteLine("Please enter the arguments.");
				Console.WriteLine("Usage: PalasoError <isLethal> <fileName> <emailAddress> <emailSubject>");
				return 1;
			}

			int num;
			bool test = int.TryParse(args[0], out num);
			if (test == false)
			{
				Console.WriteLine(args[0] + " is invalid.");
				Console.WriteLine("Please enter 0 or 1 for isLethal.");
				Console.WriteLine("Usage: PalasoError <isLethal> <fileName> <emailAddress> <emailSubject>");
				return 1;
			}

			if (!System.IO.File.Exists(args[1]))
			{
				Console.WriteLine(args[1] + " is invalid.");
				Console.WriteLine("Please enter a valid file path.");
				Console.WriteLine("Usage: PalasoError <isLethal> <fileName> <emailAddress> <emailSubject>");
				return 1;
			}

			if (!args[2].Contains("@"))
			{
				Console.WriteLine(args[2] + " is invalid.");
				Console.WriteLine("Please enter an email address.");
				Console.WriteLine("Usage: PalasoError <isLethal> <fileName> <emailAddress> <emailSubject>");
				return 1;
			}

			bool isLethal = Convert.ToBoolean(num);
			string fileName = args[1];
			string emailAddress = args[2];
			string emailSubject = args[3];

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new PalasoErrorDialog(isLethal, fileName, emailAddress, emailSubject));
			return 0;
		}
	}
}
