// Copyright (c) 2013-2015, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
#if __MonoCS__
using System;
using System.Windows.Forms;
using IBusDotNet;
using Icu;
using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	class IbusKeyboardDescription: KeyboardDescription
	{
		private const string OtherLanguage = "Other Language";

		public IBusEngineDesc IBusKeyboardEngine { get; private set;}

		internal uint SystemIndex { get; private set; }

		public IbusKeyboardDescription(IKeyboardSwitchingAdaptor engine, IBusEngineDesc ibusKeyboard,
			uint systemIndex):
			base(FormatKeyboardIdentifier(ibusKeyboard), ibusKeyboard.LongName, ibusKeyboard.Language,
			null, engine, KeyboardType.OtherIm)
		{
			IBusKeyboardEngine = ibusKeyboard;
			SystemIndex = systemIndex;
		}

		internal IbusKeyboardDescription(IbusKeyboardDescription other): base(other)
		{
			IBusKeyboardEngine = other.IBusKeyboardEngine;
			SystemIndex = other.SystemIndex;
		}

		public override IKeyboardDefinition Clone()
		{
			return new IbusKeyboardDescription(this);
		}

		/// <summary>
		/// Produce IBus keyboard identifier which is similar to the actual ibus switcher menu.
		/// </summary>
		private static string FormatKeyboardIdentifier(IBusEngineDesc engineDesc)
		{
			string languageCode = AlternateLanguageCodes.GetLanguageCode(engineDesc.Language);
			string languageName = string.IsNullOrEmpty(languageCode) ? OtherLanguage :
				new Locale(languageCode).GetDisplayName(new Locale(Application.CurrentCulture.TwoLetterISOLanguageName));
			if (languageCode != null && languageCode.ToLowerInvariant() == languageName.ToLowerInvariant())
				languageName = OtherLanguage;
			return String.Format("{0} - {1}", languageName, engineDesc.Name);
		}

		public string ParentLayout
		{
			get { return IBusKeyboardEngine.Layout; }
		}
	}
}
#endif