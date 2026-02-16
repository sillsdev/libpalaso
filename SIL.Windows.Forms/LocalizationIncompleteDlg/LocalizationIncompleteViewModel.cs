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
		[CLSCompliant(false)]
		public ILocalizationManager PrimaryLocalizationManager { get; }

		public virtual string EmailAddressForLocalizationRequests => null;

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
		/// <param name="crowdinProjectName">The name of the project in Crowdin. This is the
		/// part of the Crowdin project's URL following https://crowdin.com/project/</param>
		/// <param name="issueRequestForLocalization">An action to handle issuing a localization
		/// request (typically by passing the <see cref="StandardAnalyticsInfo"/> to
		/// DesktopAnalytics.Track</param>
		[CLSCompliant(false)]
		public LocalizationIncompleteViewModel(ILocalizationManager appLm,
			string crowdinProjectName, Action issueRequestForLocalization)
		{
			PrimaryLocalizationManager = appLm;
			_crowdinProjectName = crowdinProjectName;
			_issueRequestForLocalization = issueRequestForLocalization;
		}

		/// <summary>
		/// Gets a value to determine whether the Localization Incomplete dialog box should be
		/// shown to the user. The default implementation simply checks to see whether the
		/// requested locale is supported by the primary localization manager, but subclasses
		/// can override this to provide more nuanced logic.
		/// </summary>
		/// <param name="languageId">The IETF language tag to evaluate. Note that if a client
		/// supports variants of the general code for some purpose but the primary localization
		/// manager contains only the general code, the client should override this method (and
		/// call the base implementation, if appropriate, passing in the general code).</param>
		public virtual bool ShouldShowDialog(string languageId)
		{
			RequestedLanguageId = languageId;
			return PrimaryLocalizationManager == null || !PrimaryLocalizationManager
				.GetAvailableUILanguageTags().Contains(RequestedLanguageId);
		}

		internal void IssueRequestForLocalization()
		{
			_issueRequestForLocalization.Invoke();
		}
	}
}
