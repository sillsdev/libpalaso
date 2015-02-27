// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	internal class KeyboardErrorDescription: IKeyboardErrorDescription
	{
		public KeyboardErrorDescription(object details): this(KeyboardType.System, details)
		{
		}

		public KeyboardErrorDescription(KeyboardType type, object details)
		{
			Type = type;
			Details = details;
		}

		#region IKeyboardErrorDescription implementation
		public KeyboardType Type { get; private set; }

		public object Details { get; private set; }
		#endregion
	}
}
