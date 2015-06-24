using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class CannotFindMyLanguageDialog : Form
	{
		public CannotFindMyLanguageDialog()
		{
			InitializeComponent();
		}

#if __MonoCS__
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// The multi-line explanation is cropped on Linux and greyed out.
			// The OnPaint method that is supposed to reliably display it nicely
			// is deliberately not called in Mono 2.10.9.  The following adjustments
			// allow the message to be displayed properly for Linux/Mono.
			betterLabel1.Enabled = true;
			betterLabel1.Height = betterLabel1.Height + 20;
		}
#endif

		private void _okButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
