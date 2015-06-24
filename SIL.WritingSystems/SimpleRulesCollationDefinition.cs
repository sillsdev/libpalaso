namespace SIL.WritingSystems
{
	public class SimpleRulesCollationDefinition : RulesCollationDefinition
	{
		private string _simpleRules;

		public SimpleRulesCollationDefinition(string type)
			: base(type)
		{
		}

		public SimpleRulesCollationDefinition(SimpleRulesCollationDefinition srcd)
			: base(srcd)
		{
			_simpleRules = srcd._simpleRules;
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
				CollationRules = parser.ConvertToIcuRules(SimpleRules);
				IsValid = true;
				return true;
			}
			CollationRules = string.Empty;
			IsValid = false;
			return false;
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var srcd = other as SimpleRulesCollationDefinition;
			return srcd != null && base.ValueEquals(other) && SimpleRules == srcd.SimpleRules;
		}

		public override CollationDefinition Clone()
		{
			return new SimpleRulesCollationDefinition(this);
		}
	}
}
