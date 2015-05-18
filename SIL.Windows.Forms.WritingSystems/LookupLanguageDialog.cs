using System;
using System.Windows.Forms;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class LookupLanguageDialog : Form
	{
		public LookupLanguageDialog()
		{
			InitializeComponent();
			ShowDesiredLanguageNameField = true;
		}

		/// <summary>
		/// Get the name of the desired language to search for.
		/// </summary>
		public string DesiredLanguageName
		{
			get { return _lookupLanguageControl.DesiredLanguageName; }
		}

		/// <summary>
		/// If you wouldn't be paying attention to their requested name, and are only going to look at the code, then
		/// set this to default so that they aren't fooled into thinking they can modify the name they'll see in your application.
		/// </summary>
		public bool ShowDesiredLanguageNameField
		{
			set { _lookupLanguageControl.ShowDesiredLanguageNameField = value; }
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			_lookupLanguageControl.StopTimer();
			base.OnClosing(e);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		public LanguageInfo SelectedLanguage
		{
			set { _lookupLanguageControl.LanguageInfo = value; }
			get { return _lookupLanguageControl.LanguageInfo; }
		}

		public string SearchText
		{
			get { return _lookupLanguageControl.SearchText; }
			set { _lookupLanguageControl.SearchText = value; }
		}

		private void OnChooserDoubleClicked(object sender, EventArgs e)
		{
			_okButton_Click(sender, e);
		}

		private void _lookupLanguageControl_Changed(object sender, EventArgs e)
		{
			_okButton.Enabled = _lookupLanguageControl.HaveSufficientInformation;
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}