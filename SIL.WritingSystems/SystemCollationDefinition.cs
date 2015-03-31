namespace SIL.WritingSystems
{
	public class SystemCollationDefinition : CollationDefinition
	{
		private string _ietfLanguageTag;

		public SystemCollationDefinition()
			: base("system")
		{
		}

		public SystemCollationDefinition(SystemCollationDefinition scd)
			: base(scd)
		{
			_ietfLanguageTag = scd._ietfLanguageTag;
		}

		public string IetfLanguageTag
		{
			get { return _ietfLanguageTag; }
			set
			{
				if (Set(() => IetfLanguageTag, ref _ietfLanguageTag, value))
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

			IsValid = SystemCollator.ValidateIetfLanguageTag(_ietfLanguageTag, out message);
			return IsValid;
		}

		protected override ICollator CreateCollator()
		{
			return new SystemCollator(_ietfLanguageTag);
		}

		public override CollationDefinition Clone()
		{
			return new SystemCollationDefinition(this);
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var scd = other as SystemCollationDefinition;
			return scd != null && base.ValueEquals(other) && _ietfLanguageTag == scd._ietfLanguageTag;
		}
	}
}
