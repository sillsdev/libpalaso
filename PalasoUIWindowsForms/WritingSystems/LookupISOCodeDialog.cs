using System;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOCodeDialog : Form
	{
		public LookupISOCodeDialog()
		{
			InitializeComponent();
			ShowDesiredLanguageNameField = true;
		}

		/// <summary>Force the dialog to return 3 letter iso codes even if a 2 letter code is available</summary>
		public bool Force3LetterCodes
		{
			get { return _lookupISOControl.Force3LetterCodes; }
			set { _lookupISOControl.Force3LetterCodes = value; }
		}

		/// <summary>
		/// Get or set the name of the desired language to search for.
		/// </summary>
		public string DesiredLanguageName
		{
			get { return _lookupISOControl.DesiredLanguageName; }
			set { _lookupISOControl.DesiredLanguageName = value; }
		}

		/// <summary>
		/// If you wouldn't be paying attention to their requested name, and are only going to look at the code, then
		/// set this to default so that they aren't fooled into thinking they can modify the name they'll see in your application.
		/// </summary>
		public bool ShowDesiredLanguageNameField
		{
			set { _lookupISOControl.ShowDesiredLanguageNameField = value; }
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			_lookupISOControl.StopTimer();
			base.OnClosing(e);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

//        public Iso639LanguageCode ISOCodeAndName
//        {
//            get
//            {
//				if( DialogResult != DialogResult.OK)
//					return null;
//	            return new Iso639LanguageCode(_lookupISOControl.LanguageInfo.Code, _lookupISOControl.LanguageInfo.Names[0],
//	                                          _lookupISOControl.LanguageInfo.Code);//review: it's not clear which codes these are supposed to be. As is, they are 639-1 if it exists, else 639-3
//            }
//        }

		public LanguageInfo SelectedLanguage
		{
			set { _lookupISOControl.LanguageInfo = value; }
			get
			{
				return _lookupISOControl.LanguageInfo;
			}
		}


		private void OnChooserDoubleClicked(object sender, EventArgs e)
		{
			_okButton_Click(sender, e);
		}

		private void _lookupISOControl_Changed(object sender, EventArgs e)
		{
			_okButton.Enabled = _lookupISOControl.HaveSufficientInformation;
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}