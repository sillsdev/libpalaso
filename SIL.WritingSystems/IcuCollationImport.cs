using System;

namespace SIL.WritingSystems
{
	public class IcuCollationImport : IEquatable<IcuCollationImport>
	{
		private readonly string _languageTag;
		private readonly string _type;

		public IcuCollationImport(string languageTag, string type = null)
		{
			if (string.IsNullOrEmpty(languageTag))
				throw new ArgumentNullException("languageTag");

			_languageTag = languageTag;
			_type = type ?? string.Empty;
		}

		public string LanguageTag
		{
			get { return _languageTag; }
		}

		public string Type
		{
			get { return _type; }
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as IcuCollationImport);
		}

		public bool Equals(IcuCollationImport other)
		{
			if (other == null)
				return false;

			return _languageTag == other._languageTag && _type == other._type;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + _languageTag.GetHashCode();
			code = code * 31 + _type.GetHashCode();
			return code;
		}
	}
}
