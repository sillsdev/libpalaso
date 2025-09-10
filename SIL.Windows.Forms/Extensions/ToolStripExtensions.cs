using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using L10NSharp;
using L10NSharp.Windows.Forms;
using SIL.Windows.Forms.LocalizationIncompleteDlg;
using static SIL.WritingSystems.IetfLanguageTag;

namespace SIL.Windows.Forms.Extensions
{
	public static class ToolStripExtensions
	{
		public static void SizeTextRectangleToText(this ToolStripItemTextRenderEventArgs args)
		{
			// ToolStrip does not have UseCompatibleTextRendering.
			var textSize = TextRenderer.MeasureText(args.Graphics, args.Text, args.TextFont, args.TextRectangle.Size, TextFormatFlags.WordBreak);
			const int padding = 2;

			var rc = args.TextRectangle;
			var changed = false;

			// adjust the rectangle to fit the calculated text size
			if (rc.Width < textSize.Width + padding)
			{
				var diffX = textSize.Width + padding - rc.Width;
				rc.X -= diffX / 2;
				rc.Width += diffX;
				changed = true;
			}

			if (rc.Height < textSize.Height + padding)
			{
				var diffY = textSize.Height + padding - rc.Height;
				rc.Y -= diffY / 2;
				rc.Height += diffY;
				changed = true;
			}

			// if nothing changed, return now
			if (!changed) return;

			args.TextRectangle = rc;
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
		/// <param name="localizationIncompleteViewModel">Optional. If provided, and the primary
		/// localization manager does not have any strings translated for the given language,
		/// then a dialog box will be shown informing the user that localization has not yet been
		/// done for application-specific strings in the requested language.</param>
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
		[CLSCompliant(false)]
		public static void InitializeWithAvailableUILocales(this ToolStripDropDownItem menu,
			Func<string, bool> localeSelectedAction = null, //ILocalizationManager lm = null,
			LocalizationIncompleteViewModel localizationIncompleteViewModel = null,
			Func<bool> moreSelected = null, Dictionary<string, string> additionalNamedLocales = null)
		{
			void DropDownOpening(object sender, EventArgs e)
			{
				menu.DropDownItems.OfType<ToolStripMenuItem>()
					.FirstOrDefault(m => (m.Tag as string) == LocalizationManager.UILanguageId)?.Select();
			}

			menu.DropDownOpening -= DropDownOpening;
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
				var item = menu.DropDownItems.Add(locale.Key) as ToolStripMenuItem;
				var languageId = locale.Value;
				item.Tag = languageId;
				item.Click += (a, b) =>
				{
					if (LocalizationManager.UILanguageId == languageId ||
					    (localeSelectedAction != null && !localeSelectedAction.Invoke(languageId)))
						return;

					if (localizationIncompleteViewModel != null &&
					    localizationIncompleteViewModel.ShouldShowDialog(languageId))
					{
						using (var dlg = new LocalizationIncompleteDlg.LocalizationIncompleteDlg(localizationIncompleteViewModel))
							if (dlg.ShowDialog(item.GetCurrentParent().FindForm()) == DialogResult.Cancel)
								return;
					}
					
					LocalizationManagerWinforms.SetUILanguage(languageId, true);
					if (menu is ToolStripDropDownButton btn)
						btn.Text = item.Text;
				};
				if (languageId == LocalizationManager.UILanguageId)
				{
					if (menu is ToolStripDropDownButton btn)
						btn.Text = item.Text;
				}
			}

			menu.DropDownOpening += DropDownOpening;

			/*if (lm != null)
			{
				menu.DropDownItems.Add(new ToolStripSeparator());
				var moreMenu = menu.DropDownItems.Add(LocalizationManager.GetString("Common.MoreMenuItem",
					"&More...", "Last item in menu of UI languages, used to open localization dialog box"));
				moreMenu.Click += (a, b) =>
				{
					if (moreSelected != null && !moreSelected.Invoke())
						return;
					lm.ShowLocalizationDialogBox(false, menu.Owner?.FindForm());
					menu.InitializeWithAvailableUILocales(localeSelectedAction, lm,
						localizationIncompleteViewModel,
						moreSelected, additionalNamedLocales);
				};
			}*/
		}
	}

	/// <summary>
	/// Use the extension to ToolStripItemTextRenderEventArgs to implement a renderer for
	/// ToolStrip objects that provides absolutely no border on any edge.
	/// </summary>
	/// <remarks>
	/// This code originated in the Bloom project, but proved to be useful for the
	/// ArtOfReadingChooser control in SIL.Windows.Forms.
	/// </remarks>
	public class NoBorderToolStripRenderer : ToolStripProfessionalRenderer
	{
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			// Without this, the ToolStrip renderer leaves some border artifacts
			// even when the style is set to "no border".
			e.SizeTextRectangleToText();
			base.OnRenderItemText(e);
		}
	}
}
