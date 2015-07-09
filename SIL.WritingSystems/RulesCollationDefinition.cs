namespace SIL.WritingSystems
{
	public abstract class RulesCollationDefinition : CollationDefinition
	{
		private string _collationRules;

		protected RulesCollationDefinition(string type)
			: base(type)
		{
		}

		protected RulesCollationDefinition(RulesCollationDefinition rcd)
			: base(rcd)
		{
			_collationRules = rcd._collationRules;
		}

		public string CollationRules
		{
			get { return _collationRules ?? string.Empty; }
			protected internal set
			{
				if (Set(() => CollationRules, ref _collationRules, value))
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

			IsValid = IcuRulesCollator.ValidateSortRules(CollationRules, out message);
			return IsValid;
		}

		protected override ICollator CreateCollator()
		{
			return new IcuRulesCollator(CollationRules);
		}
	}
}
