using System;
using System.Collections.Generic;
using System.Linq;
using L10NSharp;

namespace SIL.Windows.Forms.LocalizationIncompleteDlg
{
	/// <summary>
	/// The view model used for displaying the <see cref="LocalizationIncompleteDlg"/> dialog box. 
	/// </summary>
	public class LocalizationIncompleteViewModel
	{
		public ILocalizationManager PrimaryLocalizationManager { get; }
		public string EmailAddressForLocalizationRequests { get; }

		private readonly string _crowdinProjectName;
		private readonly Action _issueRequestForLocalization;

		public string CrowdinProjectUrl => _crowdinProjectName == null ? null :
			"https://crowdin.com/project/" + _crowdinProjectName;

		public string RequestedLanguageId { get; private set; }

		public string UserEmailAddress { get; set; }
		public int NumberOfUsers { get; set; }
		public bool AbleToHelp { get; set; }

		/// <summary>
		/// A dictionary of key/value pairs suitable for sending to DesktopAnalytics.Track
		/// </summary>
		public Dictionary<string, string> StandardAnalyticsInfo =>
			new Dictionary<string, string>
			{
				{ "Requested language", RequestedLanguageId },
				{ "User email", UserEmailAddress },
				{ "Number of users", NumberOfUsers.ToString() },
				{ "Can help", AbleToHelp ? "Yes" : "No" },
			};

		/// <summary>
		/// Constructor for a <see cref="LocalizationIncompleteViewModel"/>
		/// </summary>
		/// <param name="appLm">The primary localization manager for the application</param>
		/// <param name="emailAddressForLocalizationRequests">The contact email address for the
		/// application for the purpose of requesting additional information about localizing,
		/// submitting localizations, etc.</param>
		/// <param name="crowdinProjectName">The name of the project in Crowdin. This is the
		/// part of the Crowdin project's URL following https://crowdin.com/project/</param>
		/// <param name="issueRequestForLocalization">An action to handle issuing a localization
		/// request (typically by passing the <see cref="StandardAnalyticsInfo"/> to
		/// DesktopAnalytics.Track</param>
		public LocalizationIncompleteViewModel(ILocalizationManager appLm,
			string emailAddressForLocalizationRequests, string crowdinProjectName,
			Action issueRequestForLocalization)
		{
			PrimaryLocalizationManager = appLm;
			EmailAddressForLocalizationRequests = emailAddressForLocalizationRequests;
			_crowdinProjectName = crowdinProjectName;
			_issueRequestForLocalization = issueRequestForLocalization;
		}

		public virtual bool ShouldShowDialog(string languageId)
		{
			RequestedLanguageId = languageId;
			return PrimaryLocalizationManager != null && PrimaryLocalizationManager
				.GetAvailableUILanguageTags().Contains(RequestedLanguageId);
		}

		internal void IssueRequestForLocalization()
		{
			_issueRequestForLocalization.Invoke();
		}
	}
}
