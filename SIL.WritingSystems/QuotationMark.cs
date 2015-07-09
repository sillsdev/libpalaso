using System;

namespace SIL.WritingSystems
{
	/// <summary>
	/// Type of quotation marking system in use
	/// </summary>
	public enum QuotationMarkingSystemType
	{
		/// <summary>
		/// Normal
		/// </summary>
		Normal,
		/// <summary>
		/// Narrative
		/// </summary>
		Narrative
	}
	
	public class QuotationMark : IEquatable<QuotationMark>
	{
		private readonly string _open;
		private readonly string _close;
		private readonly string _continue;
		private readonly int _level;
		private readonly QuotationMarkingSystemType _type;

		public QuotationMark(string open, string close, string cont, int level, QuotationMarkingSystemType type)
		{
			_open = open ?? string.Empty;
			_close = close ?? string.Empty;
			_continue = cont ?? string.Empty;
			_level = level;
			_type = type;
		}

		public string Open
		{
			get { return _open; }
		}

		public string Close
		{
			get { return _close; }
		}

		public string Continue
		{
			get { return _continue; }
		}

		public int Level
		{
			get { return _level; }
		}

		public QuotationMarkingSystemType Type
		{
			get { return _type; }
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as QuotationMark);
		}

		public bool Equals(QuotationMark other)
		{
			if (other == null)
				return false;

			return _open == other._open && _close == other._close && _continue == other._continue && _level == other._level && _type == other._type;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + _open.GetHashCode();
			code = code * 31 + _close.GetHashCode();
			code = code * 31 + _continue.GetHashCode();
			code = code * 31 + _level.GetHashCode();
			code = code * 31 + _type.GetHashCode();
			return code;
		}
	}
}
