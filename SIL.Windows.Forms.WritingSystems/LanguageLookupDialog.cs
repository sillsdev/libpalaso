using System;
using System.Windows.Forms;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class LanguageLookupDialog : Form
	{
		public LanguageLookupDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// If you wouldn't be paying attention to their requested name, and are only going to look at the code, then
		/// set this to default so that they aren't fooled into thinking they can modify the name they'll see in your application.
		/// </summary>
		public bool IsDesiredLanguageNameFieldVisible
		{
			set { _languageLookupControl.IsDesiredLanguageNameFieldVisible = value; }
		}

		public bool IsShowDialectsCheckBoxVisible
		{
			set { _languageLookupControl.IsShowDialectsCheckBoxVisible = value; }
		}

		public Func<LanguageInfo, bool> MatchingLanguageFilter
		{
			set { _languageLookupControl.MatchingLanguageFilter = value; }
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			_languageLookupControl.StopTimer();
			base.OnClosing(e);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		public void LoadLanguages()
		{
			_languageLookupControl.LoadLanguages();
		}

		public LanguageInfo SelectedLanguage
		{
			get { return _languageLookupControl.SelectedLanguage; }
			set { _languageLookupControl.SelectedLanguage = value; }
		}

		public string DesiredLanguageName
		{
			get { return _languageLookupControl.DesiredLanguageName; }
		}

		public string SearchText
		{
			get { return _languageLookupControl.SearchText; }
			set { _languageLookupControl.SearchText = value; }
		}

		private void OnChooserDoubleClicked(object sender, EventArgs e)
		{
			_okButton_Click(sender, e);
		}

		private void _lookupLanguageControl_Changed(object sender, EventArgs e)
		{
			_okButton.Enabled = _languageLookupControl.HaveSufficientInformation;
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}