// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace Palaso.Providers
{
	/// <summary>
	/// Implements a testable DateTime provider. Use this class instead of directly calling
	/// the static methods on <c>DateTime</c>. In your tests you can replace the default DateTime
	/// provider with one that gives reproducible results, e.g. ReproducibleDateTimeProvider.
	/// </summary>
	public abstract class DateTimeProvider
		: BaseProvider<DateTimeProvider, DateTimeProvider.DefaultDateTimeProvider>
	{
		#region DefaultDateTimeProvider
		public class DefaultDateTimeProvider: DateTimeProvider
		{
			public override DateTime Now { get { return DateTime.Now; } }

			public override DateTime UtcNow { get { return DateTime.UtcNow; } }

			public override DateTime Today { get { return DateTime.Today; } }
		}
		#endregion

		public abstract DateTime Now { get; }

		public abstract DateTime UtcNow { get; }

		public abstract DateTime Today { get; }
	}
}
