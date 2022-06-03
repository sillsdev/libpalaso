using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using JetBrains.Annotations;
using L10NSharp;
using SIL.Windows.Forms.Miscellaneous;
using SIL.WritingSystems;
using static SIL.WritingSystems.IetfLanguageTag;

namespace SIL.Windows.Forms.WritingSystems
{
	public static class Extensions
	{
		internal static string ToValidVariantString(this string unknownString)
		{
			// var1-var2-var3
			// var1-privateUse1-x-privateUse2
			unknownString = unknownString.Trim();
			unknownString = Regex.Replace(unknownString, @"[ ,.]", "-");
			unknownString = Regex.Replace(unknownString, @"-+", "-");
			var tokens = unknownString.Split('-');
			var variants = new List<string>();
			var privateUse = new List<string>();
			bool haveSeenX = false;

			foreach (string token in tokens)
			{
				if (token == "x")
				{
					haveSeenX = true;
					continue;
				}
				if (!haveSeenX && StandardSubtags.IsValidRegisteredVariantCode(token))
				{
					variants.Add(token);
				}
				else
				{
					privateUse.Add(token);
				}
			}
			string combinedVariant = String.Join("-", variants.ToArray());
			string combinedPrivateUse = String.Join("-", privateUse.ToArray());
			string variantString = combinedVariant;
			if (!String.IsNullOrEmpty(combinedPrivateUse))
			{
				variantString = "x-" + combinedPrivateUse;
				if (!String.IsNullOrEmpty(combinedVariant))
				{
					variantString = combinedVariant + "-" + variantString;
				}
			}
			return variantString;
		}

		internal static WritingSystemDefinition CreateAndWarnUserIfOutOfDate(this IWritingSystemFactory wsFactory, string langTag)
		{
			WritingSystemDefinition ws;
			bool upToDate;
			WaitCursor.Show();
			try
			{
				upToDate = wsFactory.Create(langTag, out ws);
			}
			finally
			{
				WaitCursor.Hide();
			}

			if (!upToDate)
			{
				if (MessageBox.Show(Form.ActiveForm, LocalizationManager.GetString("WritingSystemSetupView.UnableToConnectToSldrText", "The application is unable to connect to the SIL Locale Data Repository to retrieve the latest information about this language. If you create this writing system, the default settings might be incorrect or out of date. Are you sure you want to create a new writing system?"),
					LocalizationManager.GetString("WritingSystemSetupView.UnableToConnectToSldrCaption", "Unable to connect to SLDR"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
				{
					return null;
				}
			}

			return ws;
		}

		/// <summary>
		/// Adds one entry to this menu for each UI language known to the loaded L10NSharp
		/// LocalizationManagers, plus any additional named locales passed in. The items are added
		/// in order by language name using the current culture's sort order. Unless the
		/// application-specific function returns <c>false</c>, the Click event for each of these
		/// menu items is set up to make that language the default used for retrieving localized
		/// strings. If this menu is a <see cref="ToolStripDropDownButton"/>, it also has its text
		/// set to the name of the selected language.
		/// Finally, if a primary <see cref="ILocalizationManager"/> is provided, a separate "More"
		/// item is added at the bottom of the menu.
		/// </summary>
		/// <param name="menu">The menu whose <see cref="ToolStripDropDownItem.DropDownItems"/>
		/// collection is to be initialized.</param>
		/// <param name="localeSelectedAction">Application-specific function to call when a
		/// different language is selected. This function should normally return <c>true</c> but
		/// may return <c>false</c> to indicate that the default behavior should be suppressed.
		/// </param>
		/// <param name="lm">The primary  <see cref="ILocalizationManager"/> for the application
		/// (used to determine which node in the tree should be selected when displaying the
		/// localization dialog box in response to the user clicking More. If an application does
		/// not support field-based localization, this can be omitted and the More menu item will
		/// not be added.</param>
		/// <param name="moreSelected">Application-specific function to call when the More menu
		/// item is selected. This function should normally return <c>true</c> but may return
		/// <c>false</c> to indicate that the default behavior should be suppressed.
		/// </param>
		/// <param name="additionalNamedLocales">Dictionary of display-name to IetfTag for any
		/// additional locales an application wishes to display (besides those known to the
		/// LocalizationManager). The primary purpose of this is to allow the user to choose a
		/// country- or script-specific variant of a language. Duplicate language codes are
		/// discarded, so it is not possible to have multiple menu items which correspond to the
		/// same code. In addition, to avoid confusion, if the localization manager has entries
		/// for a (general) language which is the same as one or more entries in this collection,
		/// the general language entry will be omitted in favor of the more specific one named in
		/// this collection.</param>
		/// <remarks>REVIEW: It is not entirely clear whether this method belongs here. L10NSharp
		/// currently has a UILanguageComboBox which attempts to present a similar list of language
		/// choices. However, that logic does not have access to
		/// <see cref="GetNativeLanguageNameWithEnglishSubtitle"/>, which is what we really want to
		/// use for determining the name of the language to display. If/when L10nSharp is split
		/// into two DLLs (WinForms and non-WinForms), we may be able to refactor things such that
		/// the WinForms DLL will be able to have access to SIL.WritingSystems, and then this method
		/// could be moved into L10nSharp.Windows.Forms.</remarks>
		[PublicAPI]
		public static void InitializeWithAvailableUILocales(this ToolStripDropDownItem menu,
			Func<string, bool> localeSelectedAction = null, ILocalizationManager lm = null,
			Func<bool> moreSelected = null, Dictionary<string, string> additionalNamedLocales = null)
		{
			menu.DropDownItems.Clear();

			var namedLocales = new SortedDictionary<string, string>(StringComparer.CurrentCulture);

			if (additionalNamedLocales != null)
			{
				foreach (var additionalLocale in additionalNamedLocales)
					if (!namedLocales.ContainsValue(additionalLocale.Value))
						namedLocales[additionalLocale.Key] = additionalLocale.Value;
			}

			foreach (var lang in LocalizationManager.GetUILanguages(true))
			{
				string languageId = lang.IetfLanguageTag;
				if (!namedLocales.Values.Any(t => t == languageId || GetGeneralCode(t) == languageId))
					namedLocales[GetNativeLanguageNameWithEnglishSubtitle(languageId)] = languageId;
			}

			foreach (var locale in namedLocales)
			{
				var item = menu.DropDownItems.Add(locale.Key);
				var languageId = locale.Value;
				item.Tag = languageId;
				item.Click += (a, b) =>
				{
					if (LocalizationManager.UILanguageId == languageId ||
					    (localeSelectedAction != null && !localeSelectedAction.Invoke(languageId)))
						return;
					
					LocalizationManager.SetUILanguage(languageId, true);
					item.Select(); // This doesn't actually do anything noticeable.
					if (menu is ToolStripDropDownButton btn)
						btn.Text = item.Text;
				};
				if (languageId == LocalizationManager.UILanguageId)
				{
					if (menu is ToolStripDropDownButton btn)
						btn.Text = item.Text;
				}
			}

			if (lm != null)
			{
				menu.DropDownItems.Add(new ToolStripSeparator());
				var moreMenu = menu.DropDownItems.Add(LocalizationManager.GetString("Common.MoreMenuItem",
					"&More...", "Last item in menu of UI languages, used to open localization dialog box"));
				moreMenu.Click += (a, b) =>
				{
					if (moreSelected != null && !moreSelected.Invoke())
						return;
					lm.ShowLocalizationDialogBox(false);
					menu.InitializeWithAvailableUILocales(localeSelectedAction, lm,
						moreSelected, additionalNamedLocales);
				};
			}
		}
	}
}
