// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
using System.Windows.Forms;
using IBusDotNet;
using Icu;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	internal class IbusKeyboardDescription : KeyboardDescription
	{
		private const string OtherLanguage = "Other Language";

		protected IBusEngineDesc _ibusKeyboard;

		public IbusKeyboardDescription(string id, string layout, string locale, IKeyboardSwitchingAdaptor engine)
			: base (id, FormatKeyboardIdentifier(layout, locale), layout, locale, false, engine)
		{
		}

		public IbusKeyboardDescription(string id, IBusEngineDesc ibusKeyboard, IKeyboardSwitchingAdaptor engine)
			: base(id, FormatKeyboardIdentifier(ibusKeyboard.Name, ibusKeyboard.Language), ibusKeyboard.LongName, ibusKeyboard.Language, true, engine)
		{
			IBusKeyboardEngine = ibusKeyboard;
		}

		/// <summary>
		/// Produce IBus keyboard identifier which is similar to the actual ibus switcher menu.
		/// </summary>
		private static string FormatKeyboardIdentifier(string layout, string locale)
		{
			string languageCode = AlternateLanguageCodes.GetLanguageCode(locale);
			string languageName = string.IsNullOrEmpty(languageCode) ? OtherLanguage :
				new Locale(languageCode).GetDisplayName(new Locale(Application.CurrentCulture.TwoLetterISOLanguageName));
			if (languageCode != null && languageCode.ToLowerInvariant() == languageName.ToLowerInvariant())
				languageName = OtherLanguage;
			return string.Format("{0} - {1}", languageName, layout);
		}

		protected virtual string KeyboardIdentifier => FormatKeyboardIdentifier(_ibusKeyboard.Name, _ibusKeyboard.Language);

		public string ParentLayout
		{
			get { return IBusKeyboardEngine.Layout; }
		}

		public IBusEngineDesc IBusKeyboardEngine
		{
			get { return _ibusKeyboard; }
			set
			{
				_ibusKeyboard = value;
				Name = KeyboardIdentifier;
			}
		}

		public uint SystemIndex { get; set; }

		public void SetIsAvailable(bool isAvailable)
		{
			IsAvailable = isAvailable;
		}
	}
}
