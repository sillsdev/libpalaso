using System;

namespace SIL.WritingSystems
{
	public enum PunctuationPatternContext
	{
		Initial,
		Medial,
		Final,
		Break,
		Isolate
	}

	public class PunctuationPattern : IEquatable<PunctuationPattern>
	{
		private readonly string _pattern;
		private readonly PunctuationPatternContext _context;

		public PunctuationPattern(string pattern, PunctuationPatternContext context)
		{
			_pattern = pattern ?? string.Empty;
			_context = context;
		}

		public string Pattern
		{
			get { return _pattern; }
		}

		public PunctuationPatternContext Context
		{
			get { return _context; }
		}

		public bool Equals(PunctuationPattern other)
		{
			if (other == null)
				return false;

			return _pattern == other._pattern && _context == other._context;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PunctuationPattern);
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + _pattern.GetHashCode();
			code = code * 31 + _context.GetHashCode();
			return code;
		}
	}
}
