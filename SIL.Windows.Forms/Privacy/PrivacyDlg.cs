// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2026, SIL Global. All Rights Reserved.
// <copyright from='2026' to='2026' company='SIL Global'>
//		Copyright (c) 2026, SIL Global. All Rights Reserved.
//
//		Distributable under the terms of the MIT License (https://sil.mit-license.org/)
// </copyright>
#endregion
// --------------------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using SIL.Core.Desktop.Privacy;
using static System.Environment;
using static System.Windows.Forms.MessageBoxIcon;

namespace SIL.Windows.Forms.Privacy
{
	public partial class PrivacyDlg : Form
	{
		private readonly bool _initialAnalyticsEnabledValue;
		private readonly bool _initialGlobalInSync;
		private readonly IAnalytics _analyticsImpl;
		private readonly string _fmtDescription;
		private readonly string _fmtProductCheckboxLabel;
		private readonly string _fmtOrganizationCheckboxLabel;

		public Color RestartLabelColor
		{
			get => _labelRestartNote.ForeColor;
			set => _labelRestartNote.ForeColor = value;
		}
		
		public PrivacyDlg(IAnalytics analyticsImpl)
		{
			InitializeComponent();

			_analyticsImpl = analyticsImpl;

			// Substitute product/brand names into the localizable format strings.
			_fmtDescription = _labelDescription.Text;
			_fmtProductCheckboxLabel = _chkProductAnalytics.Text;
			_fmtOrganizationCheckboxLabel = _chkPropagateDecisionGlobally.Text;

			_initialAnalyticsEnabledValue = _analyticsImpl?.AllowTracking ?? true;
			// Global checkbox: show as checked when the global setting is set and its value
			// matches the current product setting (i.e., they are in sync). Once the user
			// has selected this
			var globalValue = _analyticsImpl?.OrganizationAnalyticsEnabled;
			_initialGlobalInSync = globalValue == _initialAnalyticsEnabledValue;

			// Populate checkboxes from current runtime state.
			_chkProductAnalytics.Checked = _initialAnalyticsEnabledValue;
			SetInitialGlobalCheckboxState();

			_chkProductAnalytics.CheckedChanged += HandleCheckboxChanged;
			_chkPropagateDecisionGlobally.CheckedChanged += HandleCheckboxChanged;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			_labelDescription.Text = string.Format(_fmtDescription,
				_analyticsImpl.ProductName, _analyticsImpl.OrganizationName);
			_chkProductAnalytics.Text = string.Format(_fmtProductCheckboxLabel,
				_analyticsImpl.ProductName);
			_chkPropagateDecisionGlobally.Text = string.Format(_fmtOrganizationCheckboxLabel,
				_analyticsImpl.OrganizationName);
		}

		private void HandleCheckboxChanged(object sender, EventArgs e)
		{
			if (_chkProductAnalytics.Checked == _initialAnalyticsEnabledValue)
				SetInitialGlobalCheckboxState();
			else
				_chkPropagateDecisionGlobally.Enabled = true;

			// Show the restart note whenever the global checkbox is checked and clicking OK
			// would write a new or changed value to the shared SIL analytics registry key.
			var globalWillChange = _chkPropagateDecisionGlobally.Checked &&
				(_chkProductAnalytics.Checked != _initialAnalyticsEnabledValue || !_initialGlobalInSync);
			_labelRestartNote.Visible = globalWillChange;
		}
		private void SetInitialGlobalCheckboxState()
		{
			_chkPropagateDecisionGlobally.Checked = false;
			_chkPropagateDecisionGlobally.Enabled = false;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (DialogResult != DialogResult.OK)
				return;

			if (_chkProductAnalytics.Checked != _initialAnalyticsEnabledValue)
			{
				try
				{
					_analyticsImpl.Update(_chkProductAnalytics.Checked,
						_chkPropagateDecisionGlobally.Checked);
				}
				catch (UnauthorizedAccessException exception)
				{
					MessageBox.Show(LocalizationManager.GetString(
							"PrivacyDialog.UnauthorizedAccess",
							"Sorry. Your preferences could not be saved.") +
					                NewLine + NewLine + exception.Message,
						$"{_analyticsImpl.ProductName} - {Text}", MessageBoxButtons.OK,
						Warning);
				}
			}
		}
	}
}
