using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SIL.WritingSystems
{
	public class InheritedCollationDefinition : CollationDefinition
	{
		private Rfc5646Tag _baseLanguageTag;
		private string _baseType;

		public InheritedCollationDefinition(string type)
			: base(type)
		{
		}

		public InheritedCollationDefinition(InheritedCollationDefinition icrd)
			: base(icrd)
		{
			_baseLanguageTag = icrd._baseLanguageTag == null ? null : icrd._baseLanguageTag.Clone();
			_baseType = icrd._baseType;
		}

		public string BaseLanguageTag
		{
			get { return _baseLanguageTag == null ? string.Empty : _baseLanguageTag.CompleteTag; }
			set
			{
				if ((_baseLanguageTag == null && !string.IsNullOrEmpty(value)) || (_baseLanguageTag != null && _baseLanguageTag.CompleteTag != value))
				{
					_baseLanguageTag = string.IsNullOrEmpty(value) ? null : Rfc5646Tag.Parse(value);
					IsChanged = true;
					ResetCollator();
				}
			}
		}

		public string BaseType
		{
			get { return _baseType ?? string.Empty; }
			set
			{
				if (UpdateString(ref _baseType, value))
					ResetCollator();
			}
		}

		public override bool Validate(out string message)
		{
			if (IsValid)
			{
				message = null;
				return true;
			}

			if (_baseLanguageTag == null)
			{
				IcuRules = string.Empty;
				IsValid = false;
				message = "The base language is undefined.";
				return false;
			}

			string tempFile = Path.GetTempFileName();
			try
			{
				Sldr.GetLdmlFile(tempFile, _baseLanguageTag.CompleteTag);
				XElement rootElem = XElement.Load(tempFile);
				XElement collationsElem = rootElem.Element("collations");
				if (collationsElem != null)
				{
					string baseType = string.IsNullOrEmpty(_baseType) ? (string) collationsElem.Element("defaultCollation") : _baseType;
					if (!string.IsNullOrEmpty(baseType))
					{
						XElement collationElem = collationsElem.Elements("collation").FirstOrDefault(ce => (string) ce.Attribute("type") == baseType);
						if (collationElem != null)
						{
							IcuRules = LdmlCollationParser.GetIcuRulesFromCollationNode(collationElem);
							IsValid = true;
							message = null;
							return true;
						}
					}
				}
			}
			finally
			{
				if (File.Exists(tempFile))
					File.Delete(tempFile);
			}

			// TODO: should we fallback to SystemCollator if we can't reach the SLDR?
			IcuRules = string.Empty;
			IsValid = false;
			message = "Unable to retrieve collation rules from base language.";
			return false;
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var icrd = other as InheritedCollationDefinition;
			return icrd != null && Type == icrd.Type && BaseLanguageTag == icrd.BaseLanguageTag && BaseType == icrd.BaseType;
		}

		public override CollationDefinition Clone()
		{
			return new InheritedCollationDefinition(this);
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", _baseLanguageTag, _baseType);
		}
	}
}
