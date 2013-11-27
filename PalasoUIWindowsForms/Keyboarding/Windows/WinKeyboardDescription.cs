// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
#if !__MonoCS__
using System;
using System.Diagnostics.CodeAnalysis;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Windows
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Keyboard description for a Windows system keyboard
	/// </summary>
	/// <remarks>Holds information about a specific keyboard, especially for IMEs (e.g. whether
	/// English input mode is selected) in addition to the default keyboard description. This
	/// is necessary to restore the current setting when switching between fields with
	/// differing keyboards. The user expects that a keyboard keeps its state between fields.
	/// </remarks>
	/// ----------------------------------------------------------------------------------------
	[SuppressMessage("Gendarme.Rules.Design", "TypesWithNativeFieldsShouldBeDisposableRule",
		Justification = "WindowHandle is a reference to a control")]
	internal class WinKeyboardDescription : KeyboardDescription
	{
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Windows.WinKeyboardDescription"/> class.
		/// </summary>
		public WinKeyboardDescription(string name, string layout, string locale,
			IInputLanguage language, IKeyboardAdaptor engine)
			: base(name, layout, locale, language, engine, KeyboardType.System)
		{
			ConversionMode = (int)(Win32.IME_CMODE.NATIVE | Win32.IME_CMODE.SYMBOL);
		}

		internal WinKeyboardDescription(WinKeyboardDescription other): base(other)
		{
			ConversionMode = other.ConversionMode;
			SentenceMode = other.SentenceMode;
			WindowHandle = other.WindowHandle;
		}

		public override IKeyboardDefinition Clone()
		{
			return new WinKeyboardDescription(this);
		}

		public int ConversionMode { get; set; }
		public int SentenceMode { get; set; }
		public IntPtr WindowHandle { get; set; }
	}
}
#endif
