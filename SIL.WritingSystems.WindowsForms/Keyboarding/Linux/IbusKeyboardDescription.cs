// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
#if __MonoCS__
using System;
using System.Windows.Forms;
using Icu;
using IBusDotNet;
using SIL.WritingSystems;
using SIL.WritingSystems.WindowsForms.Keyboarding.InternalInterfaces;

namespace SIL.WritingSystems.WindowsForms.Keyboarding.Linux
{
	internal class IbusKeyboardDescription: KeyboardDescription
	{
		public IBusEngineDesc IBusKeyboardEngine { get; private set;}

		public IbusKeyboardDescription(IKeyboardAdaptor engine, IBusEngineDesc ibusKeyboard):
			base(FormatKeyboardIdentifier(ibusKeyboard), ibusKeyboard.LongName, ibusKeyboard.Language,
			null, engine, KeyboardType.OtherIm, true)
		{
			IBusKeyboardEngine = ibusKeyboard;
		}

		private const string OtherLanguage = "Other Language";

		/// <summary>
		/// Produce IBus keyboard identifier which is similar to the actual ibus switcher menu.
		/// </summary>
		private static string FormatKeyboardIdentifier(IBusEngineDesc engineDesc)
		{
			string id = engineDesc.Language;
			string languageName = string.IsNullOrEmpty(id) ? OtherLanguage :
				new Locale(id).GetDisplayName(new Locale(Application.CurrentCulture.TwoLetterISOLanguageName));
			if (id != null && id.ToLowerInvariant() == languageName.ToLowerInvariant())
				languageName = OtherLanguage;
			return String.Format("{0} - {1}", languageName, engineDesc.Name);
		}

		public string ParentLayout
		{
			get { return IBusKeyboardEngine.Layout; }
		}

		internal int SystemIndex { get; set; }
	}
}
#endif
