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
