using System;

namespace SIL.WritingSystems
{
	public class IcuCollationImport : IEquatable<IcuCollationImport>
	{
		private readonly string _ietfLanguageTag;
		private readonly string _type;

		public IcuCollationImport(string ietfLanguageTag, string type = null)
		{
			if (string.IsNullOrEmpty(ietfLanguageTag))
				throw new ArgumentNullException("ietfLanguageTag");

			_ietfLanguageTag = ietfLanguageTag;
			_type = type ?? string.Empty;
		}

		public string IetfLanguageTag
		{
			get { return _ietfLanguageTag; }
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

			return _ietfLanguageTag == other._ietfLanguageTag && _type == other._type;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + _ietfLanguageTag.GetHashCode();
			code = code * 31 + _type.GetHashCode();
			return code;
		}
	}
}
