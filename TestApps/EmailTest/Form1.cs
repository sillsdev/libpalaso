using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.Email;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;

namespace EmailTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), "hello");
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
			string name = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0].Name;
			KeyboardController.ActivateKeyboard(name);
		}

		private void _keyman6TestBox_Enter(object sender, EventArgs e)
		{
			if(KeyboardController.EngineAvailable(KeyboardController.Engines.Keyman6))
			{
				string name = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0].Name;
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
			IEmailProvider emailProvider = Palaso.Email.EmailProviderFactory.PreferredEmailProvider();
			IEmailMessage message = emailProvider.CreateMessage();
			message.To.Add("nowhere@example.com");
			message.AttachmentFilePath.Add("EmailTest.exe");
			message.Subject = "Test Message";
			message.Body = "Just a test message.";
			message.Send(emailProvider);
		}

		private void WritingSystemPickerButton_Click(object sender, EventArgs e)
		{
		}

		private void button7_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(@"
			x
x
x
x
x
x
x
x
x
x
x
x
x
xx
x
x
x
the end.");
		}

		private void _probWithExitButton_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(new ShowAlwaysPolicy(), "Foobar", DialogResult.No,
															 "Notice, you can click Foobar.");
		}
	}
}