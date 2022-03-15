﻿using System;
using System.Windows.Forms;
using SIL.Reporting;

namespace TestAppKeyboard
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Logger.Init();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new KeyboardForm());
		}
	}
}
