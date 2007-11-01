using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.ErrorReport.ReportNonFatalMessage("hello");
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.ErrorNotificationDialog.ReportException(new Exception("test"),this,false);
		}

		private void button3_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.ErrorNotificationDialog.ReportException(new Exception("test"),this);

		}
	}
}