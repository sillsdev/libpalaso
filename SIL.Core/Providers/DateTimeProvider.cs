// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace SIL.Providers
{
	/// <summary>
	/// Implements a testable DateTime provider. Use this class instead of directly calling
	/// the static methods on <c>DateTime</c>. In your tests you can replace the default DateTime
	/// provider with one that gives reproducible results, e.g. ReproducibleDateTimeProvider.
	/// </summary>
	/// <remarks>Usage:
	/// Instead of <c>DateTime.Now</c> you write <c>DateTimeProvider.Current.Now</c>
	/// </remarks>
	public abstract class DateTimeProvider
		: BaseProvider<DateTimeProvider, DateTimeProvider.DefaultDateTimeProvider>
	{
		#region DefaultDateTimeProvider
		public class DefaultDateTimeProvider: DateTimeProvider
		{
			public override DateTime Now => DateTime.Now;

			public override DateTime UtcNow => DateTime.UtcNow;

			public override DateTime Today => DateTime.Today;
		}
		#endregion

		public abstract DateTime Now { get; }

		public abstract DateTime UtcNow { get; }

		public abstract DateTime Today { get; }
	}
}
