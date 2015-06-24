using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.SuperToolTip;

namespace SIL.Windows.Forms.Tests.SuperToolTip
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

		private void richTextBox1_MouseMove(object sender, MouseEventArgs e)
		{
		   SuperToolTipInfoWrapper info= superToolTip1.GetSuperStuff(richTextBox1);
		   info.SuperToolTipInfo.BodyText = e.Y.ToString();
		   info.SuperToolTipInfo.OffsetForWhereToDisplay = new Point(10,e.Y);
			this.superToolTip1.SetSuperStuff(this.richTextBox1, info);
			Debug.WriteLine(e.Location);
		}
	}
}