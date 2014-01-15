// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Globalization;

namespace Palaso.WritingSystems
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Wrapper interface for System.Windows.Forms.InputLanguage
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public interface IInputLanguage
	{
		CultureInfo Culture { get; }
		IntPtr Handle { get; }
		string LayoutName { get; }
	}
}
