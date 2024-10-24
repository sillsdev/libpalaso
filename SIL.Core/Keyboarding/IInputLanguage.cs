// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Globalization;

namespace SIL.Keyboarding
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
