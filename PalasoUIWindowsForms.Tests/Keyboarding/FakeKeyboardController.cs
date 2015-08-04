// --------------------------------------------------------------------------------------------
// <copyright from='2012' to='2012' company='SIL International'>
// 	Copyright (c) 2012, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	/// <summary>
	/// Implements a fake do-nothing keyboard controller.
	/// </summary>
	internal sealed class FakeKeyboardController: DefaultKeyboardController
	{
		/// <summary>
		/// Installs this fake keyboard controller instead of the real one
		/// </summary>
		[SuppressMessage("Gendarme.Rules.Correctness", "EnsureLocalDisposalRule",
			Justification="FakeKeboardController is a Singleton")]
		public static void Install()
		{
			Keyboard.Controller = new FakeKeyboardController();
		}

		public FakeKeyboardController()
		{
			ActiveKeyboard = new KeyboardDescriptionNull();
		}

		#region IKeyboardController implementation

		/// <summary>
		/// Figures out the system default keyboard for the specified writing system (the one to use if we have no available KnownKeyboards).
		/// The implementation may use obsolete fields such as Keyboard
		/// </summary>
		public override IKeyboardDefinition DefaultForWritingSystem(IWritingSystemDefinition ws)
		{
			return KeyboardDescription.Zero;
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// </summary>
		public override IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			var inputLanguage = new InputLanguageWrapper(new CultureInfo(locale), IntPtr.Zero, layout);
			return new KeyboardDescription(layout, layout, locale, inputLanguage,  KeyboardController.KeyboardRetrievers[0].Adaptor);
		}

		public KeyboardCollection Keyboards
		{
			get
			{
				return new KeyboardCollection();
			}
		}

		#endregion
	}
}
