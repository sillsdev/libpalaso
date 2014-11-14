﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Palaso.Extensions
{
	public static class DateTimeExtensions
	{
		public const string TimeFormatWithTimeZone = "yyyy-MM-ddTHH:mm:sszzzz";
		public const string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
		public const string DateOnlyFormat = "yyyy-MM-dd";


		public static string ToLiftDateTimeFormat(this DateTime when)
		{
			return when.ToISO8601DateAndUTCTimeString();
		}

		public static string ToISO8601DateAndUTCTimeString(this DateTime  when)
		{
			//the invariantCulture here ensures we get what we asked for. Else, we can actually get '.' instead of ':' in the time separators.
			return when.ToString(TimeFormatNoTimeZone, CultureInfo.InvariantCulture);
		}

		public static string ToISO8601DateOnlyString(this DateTime when)
		{
			return when.ToString(DateOnlyFormat, CultureInfo.InvariantCulture);
		}

		public static DateTime ParseISO8601DateTime(string when)
		{
			var formats = new string[]
								  {
									  TimeFormatNoTimeZone, TimeFormatWithTimeZone,
									  DateOnlyFormat
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


		/// <summary>
		/// if we can't parse it, we stick in the min value & write a warning to the progress
//        /// </summary>
//
//        public static DateTime ParseDateTimePermissivelyWithFallbackToDefaultAndWarningToProgress(string when, IProgress progress)
//        {
//            try
//            {
//                return ParseDateTimePermissivelyWithException(when);
//            }
//            catch (Exception error)
//            {
//                progress.WriteWarning(error.Message);
//                return default(DateTime);
//            }
//        }

		/// <summary>
		/// We have this permsissive business because we released versions of SayMore which used the local
		/// format, rather than a universal one.
		/// </summary>
		public static DateTime ParseDateTimePermissivelyWithException(string when)
		{
			try
			{
				return DateTimeExtensions.ParseISO8601DateTime(when);
			}
			catch (Exception e)
			{
				//Up-until mid-version 1.1, we were accidentally saving locale-specific dates
				DateTime date;
				List<CultureInfo> culturesToTry = new List<CultureInfo>(new[]
																			{
																				Thread.CurrentThread.CurrentCulture,
																				CultureInfo.CurrentCulture,
																				CultureInfo.CreateSpecificCulture("en-US"),
																				CultureInfo.CreateSpecificCulture("en-GB"),
																				CultureInfo.CreateSpecificCulture("ru")
																			});
				foreach (var cultureInfo in culturesToTry)
				{
					if (DateTime.TryParse(when, cultureInfo.DateTimeFormat, DateTimeStyles.None,
									   out date))
						return date;
				}

				throw e;
			}
		}
	}
}
