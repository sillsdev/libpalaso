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
	/// Represents the user's consent state for analytics tracking and provides access to the
	/// effective tracking permission for the current product.
	/// </summary>
	/// <remarks>
	/// Implementations determine whether analytics events may be sent,
	/// based on product-level and (optionally) organization-level settings.
	/// </remarks>
	public interface IAnalyticsConsent
	{
		event EventHandler<AllowTrackingChangedEventArgs> AllowTrackingChanged;
		
		/// <summary>
		/// Gets the name of the product (suitable for displaying in the UI) sending analytics
		/// events.
		/// </summary>
		string ProductName { get; }

		/// <summary>
		/// Gets the name of the organization associated with the product (suitable for displaying
		/// in the UI).
		/// </summary>
		string OrganizationName { get; }

		/// <summary>
		/// Gets a value indicating whether analytics tracking is currently permitted for this product
		/// after applying product and organization preferences.
		/// </summary>
		bool AllowTracking { get; }

		/// <summary>
		/// Gets a value indicating whether analytics is enabled at the organization level.
		/// </summary>
		/// <remarks>
		/// A value of <c>true</c> tracking is currently permitted at the organization level.
		/// A value of <c>false</c> tracking is currently disallowed at the organization level.
		/// A value of <c>null</c> no organization-level preference has been specified.
		/// </remarks>
		bool? OrganizationAnalyticsEnabled { get; }

		/// <summary>
		/// Updates with the specified product-specific tracking permission, applying the setting
		/// organization-wide if so requested.
		/// </summary>
		/// <param name="allowTracking">A value indicating whether analytics tracking is permitted
		/// for this product.</param>
		/// <param name="applyOrganizationWide">A value indicating whether the update should apply
		/// to all desktop programs published the organization.</param>
		/// <exception cref="T:System.UnauthorizedAccessException">The settings cannot be written
		/// because the user does not have the necessary access rights.</exception>
		/// <remarks>
		/// In practice, this *either* sets the global value (and removes the product-specific
		/// setting if present) OR it sets only the product-specific setting. This ensures that if
		/// a later decision is made in a different product to change the global setting, it will
		/// apply to this product as well (i.e., the product-specific setting won't override it).
		/// </remarks>
		void Update(bool allowTracking, bool applyOrganizationWide = false);
	}
}
