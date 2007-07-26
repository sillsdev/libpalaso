using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Elsehemy;

namespace WritingSystemSetup.Tests
{
	public partial class SuperToolTipTestForm : Form
	{
		public SuperToolTipTestForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			SuperToolTipInfoWrapper info= superToolTip1.GetSuperStuff(richTextBox1);
			info.SuperToolTipInfo.BodyText = "button";
			info.SuperToolTipInfo.ShowFooter = true;
			info.SuperToolTipInfo.FooterText = "clicked";
			info.SuperToolTipInfo.OffsetForWhereToDisplay = new Point(100,100);
			this.superToolTip1.SetSuperStuff(this.richTextBox1, info);
		}
	}
}