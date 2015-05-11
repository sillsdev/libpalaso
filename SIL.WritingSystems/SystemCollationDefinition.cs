namespace SIL.WritingSystems
{
	public class SystemCollationDefinition : CollationDefinition
	{
		private string _languageTag;

		public SystemCollationDefinition()
			: base("system")
		{
		}

		public SystemCollationDefinition(SystemCollationDefinition scd)
			: base(scd)
		{
			_languageTag = scd._languageTag;
		}

		public string LanguageTag
		{
			get { return _languageTag; }
			set
			{
				if (Set(() => LanguageTag, ref _languageTag, value))
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

			IsValid = SystemCollator.ValidateLanguageTag(_languageTag, out message);
			return IsValid;
		}

		protected override ICollator CreateCollator()
		{
			return new SystemCollator(_languageTag);
		}

		public override CollationDefinition Clone()
		{
			return new SystemCollationDefinition(this);
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var scd = other as SystemCollationDefinition;
			return scd != null && base.ValueEquals(other) && _languageTag == scd._languageTag;
		}
	}
}
