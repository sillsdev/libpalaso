// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2026, SIL Global. All Rights Reserved.
// <copyright from='2026' to='2026' company='SIL Global'>
//		Copyright (c) 2026, SIL Global. All Rights Reserved.
//
//		Distributable under the terms of the MIT License (https://sil.mit-license.org/)
// </copyright>
#endregion
// --------------------------------------------------------------------------------------------
using System;

namespace SIL.Core.Desktop.Privacy
{
	/// <summary>
	/// Provides data for the AllowTrackingChanged event.
	/// </summary>
	public sealed class AllowTrackingChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets a value indicating whether analytics tracking is now permitted.
		/// </summary>
		public bool IsTrackingAllowed { get; }

		public AllowTrackingChangedEventArgs(bool isTrackingAllowed)
		{
			IsTrackingAllowed = isTrackingAllowed;
		}
	}
}
