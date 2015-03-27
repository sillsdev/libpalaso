namespace SIL.WritingSystems
{
	public class SystemCollationDefinition : CollationDefinition
	{
		private string _cultureId;

		public SystemCollationDefinition()
			: base("system")
		{
		}

		public SystemCollationDefinition(SystemCollationDefinition scd)
			: base(scd)
		{
			_cultureId = scd._cultureId;
		}

		public string CultureId
		{
			get { return _cultureId; }
			set
			{
				if (Set(() => CultureId, ref _cultureId, value))
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

			IsValid = SystemCollator.ValidateCultureId(_cultureId, out message);
			return IsValid;
		}

		protected override ICollator CreateCollator()
		{
			return new SystemCollator(_cultureId);
		}

		public override CollationDefinition Clone()
		{
			return new SystemCollationDefinition(this);
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var scd = other as SystemCollationDefinition;
			return scd != null && base.ValueEquals(other) && _cultureId == scd._cultureId;
		}
	}
}
