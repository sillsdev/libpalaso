using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SIL.WritingSystems
{
	public class InheritedCollationDefinition : CollationDefinition
	{
		private string _baseLanguageTag;
		private string _baseType;

		public InheritedCollationDefinition(string type)
			: base(type)
		{
		}

		public InheritedCollationDefinition(InheritedCollationDefinition icrd)
			: base(icrd)
		{
			_baseLanguageTag = icrd._baseLanguageTag;
			_baseType = icrd._baseType;
		}

		public string BaseLanguageTag
		{
			get { return _baseLanguageTag; }
			set
			{
				if (!IetfLanguageTag.IsValid(value))
					throw new ArgumentException("A valid language tag is required.", "value");
				if (UpdateString(ref _baseLanguageTag, value))
					ResetCollator();
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
			message = null;
			if (IsValid)
				return true;

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
				Sldr.GetLdmlFile(tempFile, _baseLanguageTag);
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
