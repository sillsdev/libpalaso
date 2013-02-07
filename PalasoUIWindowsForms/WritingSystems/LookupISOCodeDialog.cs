using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOCodeDialog : Form
	{
		public LookupISOCodeDialog()
		{
			InitializeComponent();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			_lookupISOControl.StopTimer();
			base.OnClosing(e);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		public Iso639LanguageCode ISOCodeAndName
		{
			get
			{
				return  DialogResult == DialogResult.OK ? _lookupISOControl.ISOCodeAndName : null;
			}
		}

		public string ISOCode
		{
			get { return DialogResult == DialogResult.OK ? _lookupISOControl.ISOCode : string.Empty; }
			set { _lookupISOControl.ISOCode = value; }
		}


		private void OnChooserDoubleClicked(object sender, EventArgs e)
		{
			_okButton_Click(sender, e);
		}

		private void _lookupISOControl_Changed(object sender, EventArgs e)
		{
			_okButton.Enabled = _lookupISOControl.ISOCodeAndName != null;
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}