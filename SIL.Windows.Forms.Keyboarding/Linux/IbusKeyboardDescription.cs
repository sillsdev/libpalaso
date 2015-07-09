// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).

#if __MonoCS__
using System;
using System.Windows.Forms;
using IBusDotNet;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	internal class IbusKeyboardDescription : KeyboardDescription
	{
		private IBusEngineDesc _ibusKeyboard;

		public IbusKeyboardDescription(string id, string layout, string locale, IKeyboardAdaptor engine)
			: base (id, FormatKeyboardIdentifier(layout, locale), layout, locale, false, engine)
		{
		}

		public IbusKeyboardDescription(string id, IBusEngineDesc ibusKeyboard, IKeyboardAdaptor engine)
			: base(id, FormatKeyboardIdentifier(ibusKeyboard.Name, ibusKeyboard.Language), ibusKeyboard.LongName, ibusKeyboard.Language, true, engine)
		{
			IBusKeyboardEngine = ibusKeyboard;
		}

		private const string OtherLanguage = "Other Language";

		/// <summary>
		/// Produce IBus keyboard identifier which is similar to the actual ibus switcher menu.
		/// </summary>
		private static string FormatKeyboardIdentifier(string layout, string locale)
		{
			string languageName = string.IsNullOrEmpty(locale) ? OtherLanguage :
				new Icu.Locale(locale).GetDisplayName(new Icu.Locale(Application.CurrentCulture.TwoLetterISOLanguageName));
			if (locale != null && locale.ToLowerInvariant() == languageName.ToLowerInvariant())
				languageName = OtherLanguage;
			return String.Format("{0} - {1}", languageName, layout);
		}

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
				Name = FormatKeyboardIdentifier(_ibusKeyboard.Name, _ibusKeyboard.Language);
			}
		}

		public int SystemIndex { get; set; }

		public void SetIsAvailable(bool isAvailable)
		{
			IsAvailable = isAvailable;
		}
	}
}
#endif
