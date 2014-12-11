// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Globalization;

namespace SIL.WritingSystems
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Wrapper interface for System.Windows.Forms.InputLanguage
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public interface IInputLanguage
	{
		/// <summary>
		/// Gets the culture.
		/// </summary>
		CultureInfo Culture { get; }

		/// <summary>
		/// Gets the handle.
		/// </summary>
		IntPtr Handle { get; }

		/// <summary>
		/// Gets the layout name.
		/// </summary>
		string LayoutName { get; }
	}
}
