﻿using System;
using System.Windows.Forms;
using SIL.WritingSystems;
using SIL.Windows.Forms.Miscellaneous;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class LanguageLookupDialog : FormForUsingPortableClipboard
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

		public bool IsShowRegionalDialectsCheckBoxVisible
		{
			set { _languageLookupControl.IsShowRegionalDialectsCheckBoxVisible = value; }
		}

		public Func<LanguageInfo, bool> MatchingLanguageFilter
		{
			set { _languageLookupControl.MatchingLanguageFilter = value; }
		}

		/// <summary>
		/// Requests that the specified language, if matched, should be displayed with the specified name.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="name"></param>
		public void SetLanguageAlias(string code, string name)
		{
			_languageLookupControl.SetLanguageAlias(code, name);
		}

		/// <summary>
		/// Set up a filter so we don't offer codes 'zh' and 'cmn' at all, and use some more
		/// familiar names (in both English and Chinese) for the four main useful Chinese codes.
		/// </summary>
		public void UseSimplifiedChinese()
		{
			_languageLookupControl.UseSimplifiedChinese();
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