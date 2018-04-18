// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using SIL.Providers;

namespace SIL.TestUtilities.Providers
{
	/// <summary>
	/// A provider for the static methods of DateTime that allows to specify an exact date/time
	/// that will be returned. This is helpful in unit tests.
	/// </summary>
	/// <remarks>Usage:
	/// <code>
	/// var dateTime = DateTime.Now;
	/// DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(dateTime));
	/// ... tests ...
	/// DateTimeProvider.ResetToDefault();
	/// </code>
	/// </remarks>
	public class ReproducibleDateTimeProvider: DateTimeProvider
	{
		private readonly DateTime _dateTime;

		public ReproducibleDateTimeProvider(): this(DateTime.MinValue)
		{
		}

		public ReproducibleDateTimeProvider(DateTime dateTime)
		{
			_dateTime = dateTime;
		}

		public override DateTime Now
		{
			get { return _dateTime; }
		}

		public override DateTime UtcNow
		{
			get { return _dateTime.ToUniversalTime(); }
		}

		public override DateTime Today
		{
			get { return new DateTime(_dateTime.Year, _dateTime.Month, _dateTime.Day); }
		}
	}
}
