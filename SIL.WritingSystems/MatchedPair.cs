using System;

namespace SIL.WritingSystems
{
	public class MatchedPair : IEquatable<MatchedPair>
	{
		private readonly string _open;
		private readonly string _close;
		private readonly bool _paragraphClose;

		public MatchedPair(string open, string close, bool paragraphClose)
		{
			_open = open ?? string.Empty;
			_close = close ?? string.Empty;
			_paragraphClose = paragraphClose;
		}

		public string Open
		{
			get { return _open; }
		}

		public string Close
		{
			get { return _close; }
		}

		public bool ParagraphClose
		{
			get { return _paragraphClose; }
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as MatchedPair);
		}

		public bool Equals(MatchedPair other)
		{
			if (other == null)
				return false;

			return _open == other._open && _close == other._close && _paragraphClose == other._paragraphClose;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + _open.GetHashCode();
			code = code * 31 + _close.GetHashCode();
			code = code * 31 + _paragraphClose.GetHashCode();
			return code;
		}
	}
}
