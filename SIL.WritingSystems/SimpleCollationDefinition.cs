namespace SIL.WritingSystems
{
	public class SimpleCollationDefinition : CollationDefinition
	{
		private string _simpleRules;

		public SimpleCollationDefinition(string type)
			: base(type)
		{
		}

		public SimpleCollationDefinition(SimpleCollationDefinition scrd)
			: base(scrd)
		{
			_simpleRules = scrd._simpleRules;
		}

		public string SimpleRules
		{
			get { return _simpleRules ?? string.Empty; }
			set
			{
				if (Set(() => SimpleRules, ref _simpleRules, value))
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

			var parser = new SimpleRulesParser();
			if (parser.ValidateSimpleRules(SimpleRules, out message))
			{
				IcuRules = parser.ConvertToIcuRules(SimpleRules);
				IsValid = true;
				return true;
			}
			IcuRules = string.Empty;
			IsValid = false;
			return false;
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var scrd = other as SimpleCollationDefinition;
			return scrd != null && Type == scrd.Type && SimpleRules == scrd.SimpleRules;
		}

		public override CollationDefinition Clone()
		{
			return new SimpleCollationDefinition(this);
		}

		public override string ToString()
		{
			return _simpleRules;
		}
	}
}
