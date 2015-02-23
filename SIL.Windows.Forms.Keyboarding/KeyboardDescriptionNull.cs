// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding
{
	/// <summary>
	/// This implements a no-op keyboard that can be used where we don't know what keyboard to use
	/// </summary>
	internal class KeyboardDescriptionNull : IKeyboardDefinition
	{
		private const string DefaultKeyboardName = "(default)";

		#region IKeyboardDefinition implementation
		public void Activate()
		{
		}

		public KeyboardFormat Format
		{
			get { return KeyboardFormat.Unknown; }
		}

		public IList<string> Urls
		{
			get { return null; }
		}

		public string ID
		{
			get { return string.Empty; }
		}

		public KeyboardAdaptorType Type
		{
			get { return KeyboardAdaptorType.System; }
		}

		public string Name
		{
			get { return DefaultKeyboardName; }
		}

		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		public string LocalizedName
		{
			get { return Name; }
		}

		public string Locale
		{
			get { return string.Empty; }
		}

		public string Layout
		{
			get { return DefaultKeyboardName; }
		}

		public bool IsAvailable
		{
			get { return false; }
		}

		public IKeyboardAdaptor Engine
		{
			get { throw new NotSupportedException(); }
		}
		#endregion

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescriptionNull"/>.
		/// </summary>
		public override string ToString()
		{
			return "<no keyboard>";
		}
	}
}
