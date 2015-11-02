using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using SIL.Extensions;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This implements the attributes that belong to the LIFT concept of "extensible".
	/// </summary>
	public class Extensible
	{
		private string _id;		//not actually part of extensible (as of 8/1/2007)
		private Guid _guid;		//not actually part of extensible
		private DateTime _creationTime;
		private DateTime _modificationTime;
		// TODO? - The following would complete the concept of "extensible".
		//private List<Trait> _traits;
		//private List<Field> _fields;
		//private List<Annotation> _annotations;

		///<summary></summary>
		public const string LiftTimeFormatWithTimeZone = DateTimeExtensions.ISO8601TimeFormatWithTimeZone;
		///<summary></summary>
		public const string LiftTimeFormatWithUTC = DateTimeExtensions.ISO8601TimeFormatWithUTC;
		///<summary></summary>
		public const string LiftDateOnlyFormat = DateTimeExtensions.ISO8601TimeFormatDateOnly;

		///<summary>
		/// Constructor.
		///</summary>
		public Extensible()
		{
			_creationTime = DateTime.UtcNow;
			_modificationTime = _creationTime;
			_guid = Guid.NewGuid();
		}

		///<summary>
		/// Get/set the creation time (if any) of this object.
		///</summary>
		public DateTime CreationTime
		{
			get { return _creationTime; }
			set
			{
				Debug.Assert(value == default(DateTime) || value.Kind == DateTimeKind.Utc);
				_creationTime = value;
			}
		}
		///<summary>
		/// Try to parse a date/time string using one of the three allowed formats.
		///</summary>
		///<exception cref="LiftFormatException"></exception>
		public static DateTime ParseDateTimeCorrectly(string time)
		{
			var formats = new[]
								  {
									  LiftTimeFormatWithUTC, LiftTimeFormatWithTimeZone,
									  LiftDateOnlyFormat
								  };
			try
			{
				DateTime result = DateTime.ParseExact(time,
													  formats,
													  new DateTimeFormatInfo(),
													  DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
				Debug.Assert(result.Kind == DateTimeKind.Utc);
				return result;
			}
			catch (FormatException e)
			{
				var builder = new StringBuilder();
				builder.AppendFormat("One of the date fields contained a date/time format which could not be parsed ({0})." + Environment.NewLine, time);
				builder.Append("This program can parse the following formats: ");
				foreach (var format in formats)
				{
					builder.Append(format + Environment.NewLine);
				}
				builder.Append("See: http://en.wikipedia.org/wiki/ISO_8601 for an explanation of these symbols.");
				throw new LiftFormatException(builder.ToString(), e);
			}
		}

		///<summary>
		/// Get/set the modification time (if any) of this object.
		///</summary>
		public DateTime ModificationTime
		{
			get { return _modificationTime; }
			set
			{
				Debug.Assert(value==default(DateTime) || value.Kind == DateTimeKind.Utc);
				_modificationTime = value;
			}
		}

		///<summary>
		/// Get/set the id tag of this object.
		///</summary>
		public string Id
		{
			get
			{
				if (_id == null)
				{
					return string.Empty;
				}
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		///<summary>
		/// Get/set the Guid value of this object.
		///</summary>
		public Guid Guid
		{
			get
			{
				return _guid;
			}
			set
			{
				_guid = value;
			}
		}

		/// <summary></summary>
		public override string ToString()
		{
			string s = _id;
			if (Guid != Guid.Empty)
			{
				s += "/" + Guid;
			}
			s += ";";

			if (default(DateTime) != _creationTime)
			{
				s += _creationTime.ToString(LiftTimeFormatWithUTC);
			}
			s += ";";
			if (default(DateTime) != _modificationTime)
			{
				s += _modificationTime.ToString(LiftTimeFormatWithUTC);
			}
			s += ";";

			return s;
		}
	}
}