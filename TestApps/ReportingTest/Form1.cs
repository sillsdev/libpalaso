using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;

namespace TestApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			ErrorReport.SetUpForParseDotCom("https://parse.com/apps/palasoreportingtest", "xnOou71DhkahT0rOn5NU6TmTq1lg3XKParHnI3Pb", "VtMY6GSkk5wdP6m1yiuUCSP8iEAb7rDmgt19WWJR");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), "There was a problem connecting to the Internet.\r\nWarning: This machine does not have a live network connection.\r\nConnection attempt failed.");
		}

		private void button2_Click(object sender, EventArgs e)
		{
			ErrorReport.ReportNonFatalException(new Exception("test"));
		}

		private void button3_Click(object sender, EventArgs e)
		{
			Logger.Init();
			Logger.WriteEvent("testing");

			ErrorReport.ReportFatalException(new Exception("test"));
		}

		private void _keyman7TestBox_Enter(object sender, EventArgs e)
		{
			string name = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0].ShortName;
			KeyboardController.ActivateKeyboard(name);
		}

		private void _keyman6TestBox_Enter(object sender, EventArgs e)
		{
			if(KeyboardController.EngineAvailable(KeyboardController.Engines.Keyman6))
			{
				string name = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0].ShortName;
				KeyboardController.ActivateKeyboard(name);
			}
			MessageBox.Show("keyman 6 not available");
		}

		private void OnExceptionWithPolicyClick(object sender, EventArgs e)
		{
			try
			{
				throw new Exception("hello");
			}
			catch (Exception exception)
			{
				ErrorReport.ReportNonFatalException(exception, new ShowOncePerSessionBasedOnExactMessagePolicy() );
			}
		}

		private void OnNonFatalMessageWithStack(object sender, EventArgs e)
		{
			ErrorReport.ReportNonFatalMessageWithStackTrace("{0} {1}", "hello","there");
		}

		private void button6_Click(object sender, EventArgs e)
		{
			var d = new WritingSystemTest();
			d.ShowDialog();
		}

		private void WritingSystemPickerButton_Click(object sender, EventArgs e)
		{
			var d = new WritingSystemPickerTestForm();
			d.ShowDialog();

		}

		private void button7_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(@"Should see 11 lines or a scroll following:
1
2
3
4
5
6
7
8
9
10
the end.");
		}

		private void button9_Click(object sender, EventArgs e)
		{
			var bldr = new StringBuilder("Should be 100 lines with VScrollbar." + Environment.NewLine);

			for (int i = 1; i <= 100; i++)
				bldr.AppendLine("Line " + i);

			ErrorReport.NotifyUserOfProblem(bldr.ToString());
		}


		private void _probWithExitButton_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(new ShowAlwaysPolicy(), "Foobar", DialogResult.No,
															 "Notice, you can click Foobar.");
		}

		private void button8_Click(object sender, EventArgs e)
		{
			ErrorReport.NotifyUserOfProblem(new ApplicationException("testing"),
											"Bloom was appalled by the irony of that text.");
		}

		private void button10_Click(object sender, EventArgs e)
		{
			throw new ApplicationException("test2");
		}
	}
}