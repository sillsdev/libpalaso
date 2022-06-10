using System;
using System.Windows.Forms;
using SIL.Program;

namespace SIL.Windows.Forms.LocalizationIncompleteDlg
{
	/// <summary>
	/// A dialog box allowing for interaction with the user when they select to see the UI in a
	/// language that is "advertised" on the menu as being available but whose localization has
	/// not yet been initiated for the current application. (It will typically have some common
	/// Palaso strings localized, but that's all.)
	/// </summary>
	public partial class LocalizationIncompleteDlg : Form
	{
		private readonly LocalizationIncompleteViewModel _localizationViewModel;

		public LocalizationIncompleteDlg(LocalizationIncompleteViewModel localizationViewModel)
		{
			_localizationViewModel = localizationViewModel;
			InitializeComponent();

			var lm = localizationViewModel.PrimaryLocalizationManager;
			if (lm.Name != null)
				Text = lm.Name;

			_lblLocalizationIncomplete.Text = string.Format(_lblLocalizationIncomplete.Text,
				lm.Name, localizationViewModel.RequestedLanguageId);

			_lblUsers.Text = string.Format(_lblUsers.Text, localizationViewModel.RequestedLanguageId);

			_chkAbleToHelp.Text = string.Format(_chkAbleToHelp.Text, lm.Name,
				localizationViewModel.RequestedLanguageId);

			_linkCrowdinAndEmailInstructions.Visible =
				_localizationViewModel.CrowdinProjectUrl != null;

			_lblMoreInromationEmail.Text = string.Format(_lblMoreInromationEmail.Text,
				localizationViewModel.EmailAddressForLocalizationRequests);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			_localizationViewModel.UserEmailAddress = _txtUserEmailAddress.Text;
			_localizationViewModel.NumberOfUsers = (int)_numUsers.Value;
			_localizationViewModel.AbleToHelp = _chkAbleToHelp.Checked;
			_localizationViewModel.IssueRequestForLocalization();
		}

		private void _btnCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(_localizationViewModel.EmailAddressForLocalizationRequests);
		}

		private void _lblCrowdinAndEmailInstructions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.SafeStart(_localizationViewModel.CrowdinProjectUrl);
		}
	}
}
