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
	/// Provides configuration and identity information used for analytics tracking.
	/// </summary>
	/// <remarks>
	/// Implementations determine whether analytics events may be sent,
	/// based on product-level and (optionally) organization-level settings.
	/// </remarks>
	public interface IAnalytics
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
		/// Gets a value indicating whether analytics tracking is currently permitted for this product.
		/// This is determined as follows:
		/// 1. If a product-specific setting exists, it takes precedence.
		/// 2. Otherwise, the global/organization-wide setting is used.
		/// 3. If neither setting is present, tracking is allowed by default.
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
