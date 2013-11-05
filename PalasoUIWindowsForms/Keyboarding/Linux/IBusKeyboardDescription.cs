// --------------------------------------------------------------------------------------------
// <copyright from='2013' to='2013' company='SIL International'>
// 	Copyright (c) 2013, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------
#if __MonoCS__
using System;
using System.Windows.Forms;
using IBusDotNet;
using Icu;
using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	internal class IBusKeyboardDescription: KeyboardDescription
	{
		public IBusEngineDesc IBusKeyboardEngine { get; private set;}

		public IBusKeyboardDescription(IKeyboardAdaptor engine, IBusEngineDesc ibusKeyboard):
			base(FormatKeyboardIdentifier(ibusKeyboard), ibusKeyboard.LongName, ibusKeyboard.Language,
			null, engine, KeyboardType.OtherIm)
		{
			IBusKeyboardEngine = ibusKeyboard;
		}

		private const string OtherLanguage = "Other Language";

		/// <summary>
		/// Produce IBus keyboard identifier which is simular to the actual ibus switcher menu.
		/// </summary>
		private static string FormatKeyboardIdentifier(IBusEngineDesc engineDesc)
		{
			string id = engineDesc.Language;
			string languageName = string.IsNullOrEmpty(id) ? OtherLanguage :
				new Locale(id).GetDisplayName(new Locale(Application.CurrentCulture.TwoLetterISOLanguageName));
			if (id.ToLowerInvariant() == languageName.ToLowerInvariant())
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