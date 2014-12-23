using System;

namespace SIL.WritingSystems
{
	public class QuotationMark : IEquatable<QuotationMark>
	{
		private readonly string _open;
		private readonly string _close;
		private readonly string _continue;

		public QuotationMark(string open, string close, string cont)
		{
			_open = open ?? string.Empty;
			_close = close ?? string.Empty;
			_continue = cont ?? string.Empty;
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

		public override bool Equals(object obj)
		{
			return Equals(obj as QuotationMark);
		}

		public bool Equals(QuotationMark other)
		{
			if (other == null)
				return false;

			return _open == other._open && _close == other._close && _continue == other._continue;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + _open.GetHashCode();
			code = code * 31 + _close.GetHashCode();
			code = code * 31 + _continue.GetHashCode();
			return code;
		}
	}
}
