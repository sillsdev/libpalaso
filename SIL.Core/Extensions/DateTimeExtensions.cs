using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SIL.Extensions
{
	public static class DateTimeExtensions
	{
		/// <summary>
		/// We expect to handle ISO 8601 format.  CVS format is deprecated
		/// </summary>
		public const string ISO8601TimeFormatWithTimeZone = "yyyy-MM-ddTHH:mm:sszzzz";
		public const string ISO8601TimeFormatWithUTC = "yyyy-MM-ddTHH:mm:ssZ";
		public const string ISO8601TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ss";
		public const string ISO8601TimeFormatDateOnly = "yyyy-MM-dd";

		public static string ToLiftDateTimeFormat(this DateTime when)
		{
			return when.ToISO8601TimeFormatWithUTCString();
		}

		// No method for ToISO8601TimeFormatWithTimeZoneString

		//the invariantCulture here ensures we get what we asked for. Else, we can actually get '.' instead of ':' in the time separators.

		public static string ToISO8601TimeFormatWithUTCString(this DateTime when)
		{
			return when.ToUniversalTime().ToString(ISO8601TimeFormatWithUTC, CultureInfo.InvariantCulture);
		}

		public static string ToISO8601TimeFormatNoTimeZoneString(this DateTime when)
		{
			return when.ToString(ISO8601TimeFormatNoTimeZone, CultureInfo.InvariantCulture);
		}

		public static string ToISO8601TimeFormatDateOnlyString(this DateTime when)
		{
			return when.ToString(ISO8601TimeFormatDateOnly, CultureInfo.InvariantCulture);
		}

		public static DateTime ParseISO8601DateTime(string when)
		{
			var formats = new []
								  {
									  ISO8601TimeFormatNoTimeZone,
									  ISO8601TimeFormatWithTimeZone,
									  ISO8601TimeFormatWithUTC,
									  ISO8601TimeFormatDateOnly
								  };
			try
			{
				DateTime result = DateTime.ParseExact(when,
													  formats,
													  new DateTimeFormatInfo(),
													  DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
				Debug.Assert(result.Kind == DateTimeKind.Utc);
				return result;
			}
			catch (FormatException e)
			{
				ThrowInformativeException(when, formats, e);
			}
			return default(DateTime);//will never get here
		}

		private static void ThrowInformativeException(string when, string[] formats, FormatException e)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("One of the date fields contained a date/time format which could not be parsed ({0})." + Environment.NewLine, when);
			builder.Append("This program can parse the following formats: ");
			foreach (var format in formats)
			{
				builder.Append(format + Environment.NewLine);
			}
			builder.Append("See: http://en.wikipedia.org/wiki/ISO_8601 for an explanation of these symbols.");
			throw new ApplicationException(builder.ToString(), e);
		}


//		/// <summary>
//		/// if we can't parse it, we stick in the min value & write a warning to the progress
//		/// </summary>
//
//		public static DateTime ParseDateTimePermissivelyWithFallbackToDefaultAndWarningToProgress(string when, IProgress progress)
//		{
//			try
//			{
//				return ParseDateTimePermissivelyWithException(when);
//			}
//			catch (Exception error)
//			{
//				progress.WriteWarning(error.Message);
//				return default(DateTime);
//			}
//		}

		/// <summary>
		/// The one day in the future is an attempt to avoid off-by-one problems with time
		/// zones, which can cause a local date representing today to appear to be in the future.
		/// </summary>
		private static DateTime MaxPermissivePastDate => DateTime.Today + TimeSpan.FromDays(1);

		[Obsolete("Use ParseModernPastDateTimePermissivelyWithException (or one of the other specific permissive parsing methods as appropriate).")]
		public static DateTime ParseDateTimePermissivelyWithException(this string when)
		{
			return when.ParseModernPastDateTimePermissivelyWithException();
		}
		
		/// <summary>
		/// This method parses dates permissively to handle cases where the caller either has no
		/// control over the origin of the date to be parsed or needs to deal with a past mistake
		/// of storing dates in a local format, rather than a universal one. This overload should
		/// work well when dates are expected to be in the recent past (i.e., modern times), as it
		/// prefers a parse which lands the date in the range from 1/1/1900 through one day in the
		/// future. (The one day in the future is an attempt to avoid off-by-one problems with time
		/// zones, which can cause a local date representing today to appear to be in the future.)
		/// </summary>
		/// <remarks>We originally released versions of SayMore which used the local format,
		/// rather than a universal one.</remarks>
		public static DateTime ParseModernPastDateTimePermissivelyWithException(this string when)
		{
			var reasonableMin = new DateTime(1900, 1, 1);
			return when.ParseDateTimePermissivelyWithException(reasonableMin, MaxPermissivePastDate);
		}
		
		/// <summary>
		/// This method parses dates permissively to handle cases where the caller either has no
		/// control over the origin of the date to be parsed or needs to deal with a past mistake
		/// of storing dates in a local format, rather than a universal one. This overload should
		/// work well when dates are expected to be in the recent past (i.e., modern times), as it
		/// prefers a parse which lands the date in the range from 1/1/1900 through one day in the
		/// future. (The one day in the future is an attempt to avoid off-by-one problems with time
		/// zones, which can cause a local date representing today to appear to be in the future.)
		/// </summary>
		public static DateTime ParsePastDateTimePermissivelyWithException(this string when)
		{
			return when.ParseDateTimePermissivelyWithException(DateTime.MinValue, MaxPermissivePastDate);
		}

		/// <summary>
		/// This method parses dates permissively to handle cases where the caller either has no
		/// control over the origin of the date to be parsed or needs to deal with a past mistake
		/// of storing dates in a local format, rather than a universal one. Note that the returned
		/// date is not guaranteed to fall within the specified range, but a parse which lands the
		/// date within that range will be preferred over one that does not. Supplying a reasonable
		/// range can help disambiguate between month and day, as well as between years, when
		/// different calendar systems are used.
		/// </summary>
		/// <remarks>Current implementation only attempt to deal with Gregorian vs. Buddhist
		/// calendar differences. It does not deal with lunar calendars.</remarks>
		public static DateTime ParseDateTimePermissivelyWithException(this string when,
			DateTime reasonableMin, DateTime reasonableMax)
		{
			try
			{
				return ParseISO8601DateTime(when);
			}
			catch (Exception)
			{
				// Up-until mid-version SayMore 1.1, we were accidentally saving locale-specific dates

				// First try a few common cultures
				var culturesToTry = new List<CultureInfo>(new[]
					{
						Thread.CurrentThread.CurrentCulture,
						CultureInfo.CurrentCulture,
						CultureInfo.CreateSpecificCulture("en-US"),
						CultureInfo.CreateSpecificCulture("en-GB"),
						CultureInfo.CreateSpecificCulture("ru")
					});
				culturesToTry.AddRange(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
				if (TryParseWithTheseCultures(when, reasonableMin, reasonableMax, out var date, culturesToTry))
					return date;

				// If still not found, give up and re-throw the exception.
				throw;
			}
		}

		private static bool TryParseWithTheseCultures(string when, DateTime reasonableMin,
			DateTime reasonableMax, out DateTime result, IEnumerable<CultureInfo> cultures)
		{
			result = DateTime.MinValue;
			bool success = false;
			foreach (var cultureInfo in cultures)
			{
				if (DateTime.TryParse(when, cultureInfo.DateTimeFormat, DateTimeStyles.None,
				    out var parsed))
				{
					if (!success)
					{
						result = parsed;
						success = true;
					}

					if (parsed >= reasonableMin && parsed <= reasonableMax)
					{
						result = parsed;
						return true;
					}

					// Try switching to Thai/Buddhist calendar. Note that other calendars (Hijri,
					// Hebrew, etc.) could also be tried, in order of likelihood, but we have never
					// seen evidence of data suggesting that it is necessary.
					var altCulture = (CultureInfo)cultureInfo.Clone();
					var originalCalendar = altCulture.DateTimeFormat.Calendar;
					try
					{
						if (originalCalendar is ThaiBuddhistCalendar)
						{
							altCulture.DateTimeFormat.Calendar = new GregorianCalendar();
						}
						else if (originalCalendar is GregorianCalendar)
						{
							altCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
						}
						else
						{
							// Unsupported calendar switch. Consider other options.
							continue;
						}
					}
					catch (ArgumentOutOfRangeException)
					{
						// Couldn't switch to alternate calendar.
						continue;
					}

					// Try again with adjusted calendar
					if (DateTime.TryParse(when, altCulture.DateTimeFormat, DateTimeStyles.None, out parsed)
					    && parsed >= reasonableMin && parsed <= reasonableMax)
					{
						result = parsed;
						return true;
					}
				}
			}

			// We didn't get any parse that fell within the reasonable range. The result will be
			// the first successful parse, if any. Otherwise, we will return failure.
			return success;
		}

		/// <summary />
		public static bool IsISO8601Date(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			var rx = new Regex(@"^\d{4}[-]\d{2}[-]\d{2}$");
			var m = rx.Match(value);

			if (!m.Success) return false;

			return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
		}

		/// <summary>
		/// Check if a date time string is of the following valid ISO 8601 formats:
		/// yyyy-MM-ddTHH:mm:ssZ
		/// yyyy-MM-ddTHH:mm:ss
		/// yyyy-MM-ddTHH:mm:sszzzz (equivalent with) yyyy-MM-ddTHH:mm:sszz:zz
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsISO8601DateTime(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			if (!Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z?$") &&
				!Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{2}:?\d{2}$"))
				return false;

			return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
		}
	}
}
