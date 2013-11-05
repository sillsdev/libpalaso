// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------
using System;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	/// <summary>
	/// This implements a no-op keyboard that can be used where we don't know what keyboard to use
	/// </summary>
	internal class KeyboardDescriptionNull: IKeyboardDefinition
	{
		internal const string DefaultKeyboardName = "(default)";

		#region IKeyboardDefinition implementation
		public void Activate()
		{
		}

		public IKeyboardDefinition Clone()
		{
			return new KeyboardDescriptionNull();
		}

		public string Id
		{
			get { return string.Empty; }
		}

		public KeyboardType Type
		{
			get { return KeyboardType.System; }
		}

		public string Name
		{
			get { return DefaultKeyboardName; }
		}

		/// <summary>
		/// Gets the layout name of the keyboard
		/// </summary>
		public string Layout
		{
			get { return DefaultKeyboardName; }
		}

		/// <summary>
		/// One operating system on which the keyboard is known to work.
		/// </summary>
		public PlatformID OperatingSystem
		{
			get { return Environment.OSVersion.Platform; }
		}

		public string Locale
		{
			get { return "en-US"; } // arbitrary but at least a valid locale. Is there something more neutral we could use?
		}

		public bool IsAvailable
		{
			get { return false; }
		}

		public IKeyboardAdaptor Engine
		{
			get
			{
				throw new NotImplementedException();
			}
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

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescriptionNull"/>.
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is KeyboardDescriptionNull;
		}

		/// <summary>
		/// Serves as a hash function for a
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescriptionNull"/> object.
		/// </summary>
		public override int GetHashCode()
		{
			return 0;
		}
	}
}
