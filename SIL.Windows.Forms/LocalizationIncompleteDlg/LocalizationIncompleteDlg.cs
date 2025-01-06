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
		private readonly LocalizationIncompleteViewModel _model;

		public LocalizationIncompleteDlg(LocalizationIncompleteViewModel model)
		{
			_model = model;
			InitializeComponent();

			var lm = model.PrimaryLocalizationManager;
			if (lm.Name != null)
				Text = lm.Name;

			_lblLocalizationIncomplete.Text = string.Format(_lblLocalizationIncomplete.Text,
				lm.Name, model.RequestedLanguageId);

			_lblUsers.Text = string.Format(_lblUsers.Text, model.RequestedLanguageId);

			_chkAbleToHelp.Text = string.Format(_chkAbleToHelp.Text, lm.Name,
				model.RequestedLanguageId);

			_linkCrowdinAndEmailInstructions.Visible =
				_model.CrowdinProjectUrl != null;

			_lblMoreInformationEmail.Text = string.Format(_lblMoreInformationEmail.Text,
				model.EmailAddressForLocalizationRequests);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			_model.UserEmailAddress = _txtUserEmailAddress.Text;
			_model.NumberOfUsers = (int)_numUsers.Value;
			_model.AbleToHelp = _chkAbleToHelp.Checked;
			_model.IssueRequestForLocalization();
		}

		private void _btnCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(_model.EmailAddressForLocalizationRequests);
		}

		private void _lblCrowdinAndEmailInstructions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.SafeStart(_model.CrowdinProjectUrl);
		}
	}
}
