// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Interfaces
{
	/// <summary>
	/// Describes a keyboard layout that either gave an exception or other error trying to
	/// get more information. We don't have enough information for these keyboard layouts
	/// to include them in the list of installed keyboards.
	/// </summary>
	public interface IKeyboardErrorDescription
	{
		/// <summary>
		/// Gets the type of this keyboard (system or other)
		/// </summary>
		KeyboardType Type { get; }

		/// <summary>
		/// Gets the details about the error, e.g. layout name.
		/// </summary>
		object Details { get; }
	}
}
