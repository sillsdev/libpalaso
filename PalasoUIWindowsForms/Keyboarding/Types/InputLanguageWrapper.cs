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
using System;
using System.Globalization;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Types
{
	/// <summary>
	/// We define an interface IInputLanguage in Palaso.dll (which does not reference Windows.Forms) which contains
	/// the properties of InputLanguage (a Windows.Forms class) that interest us. InputLanguageWrapper allows us to pretend
	/// that InputLanguage implements the interface. In particular the implicit operator allows us to simply
	/// pass an InputLanguage as an argument to any method expecting IInputLanguage...a wrapper is automatically
	/// constructed.
	/// </summary>
	public class InputLanguageWrapper : IInputLanguage
	{
		public InputLanguageWrapper(CultureInfo culture, IntPtr keyboardHandle, string layoutName)
		{
			Culture = culture;
			Handle = keyboardHandle;
			LayoutName = layoutName;
		}

		public InputLanguageWrapper(string locale, IntPtr keyboardHandle, string layoutName)
			: this(new CultureInfo(locale), keyboardHandle, layoutName)
		{
		}

		public InputLanguageWrapper(InputLanguage lang)
		{
			Culture = lang.Culture;
			Handle = lang.Handle;
			LayoutName = lang.LayoutName;
		}

		#region IInputLanguage Members
		public CultureInfo Culture { get; private set; }
		public IntPtr Handle { get; private set; }
		public string LayoutName { get; private set; }
		#endregion

		public static implicit operator InputLanguageWrapper(InputLanguage lang)
		{
			return new InputLanguageWrapper(lang);
		}

		/// <summary>
		/// Use with care. If lang is an InputLanguage, new InputLanguage(lang).Equals(lang) is true.
		/// But I'm pretty sure lang.Equals(new InputLanguage(lang)) will not be. This violates the Equals contract.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var other = obj as InputLanguageWrapper;
			if (other == null)
			{
				var lang = obj as InputLanguage;
				other = lang;
			}
			return other != null && LayoutName == other.LayoutName && Culture.Equals(other.Culture) && Handle.Equals(other.Handle);
		}

		/// <summary>
		/// This is the best I can do, though it won't allow a dictionary with a mixture of InputLanguage and InputLanguageWrapper keys to
		/// work properly.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return LayoutName.GetHashCode() ^ Culture.GetHashCode() ^ Handle.GetHashCode();
		}
	}

	public static class InputLanguageExtension
	{
		public static IInputLanguage Interface(this InputLanguage lang)
		{
			return new InputLanguageWrapper(lang);
		}
	}
}
