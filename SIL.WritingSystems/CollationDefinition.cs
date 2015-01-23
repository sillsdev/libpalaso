using System;

namespace SIL.WritingSystems
{
	public class CollationDefinition : DefinitionBase<CollationDefinition>
	{
		private readonly string _type;
		private string _icuRules;
		private ICollator _collator;

		public CollationDefinition(string type)
		{
			_type = type;
		}

		public CollationDefinition(CollationDefinition crd)
		{
			_type = crd._type;
			_icuRules = crd._icuRules;
		}

		public string Type
		{
			get { return _type; }
		}

		public string IcuRules
		{
			get { return _icuRules ?? string.Empty; }
			set
			{
				if (UpdateString(() => IcuRules, ref _icuRules, value))
					ResetCollator();
			}
		}

		/// <summary>
		/// Returns an ICollator interface that can be used to sort strings based
		/// on the custom collation rules.
		/// </summary>
		public ICollator Collator
		{
			get
			{
				if (_collator == null)
					_collator = CreateCollator();
				return _collator;
			}
		}

		public virtual bool Validate(out string message)
		{
			if (IsValid)
			{
				message = null;
				return true;
			}

			IsValid = IcuRulesCollator.ValidateSortRules(IcuRules, out message);
			return IsValid;
		}

		public bool IsValid { get; set; }

		protected void ResetCollator()
		{
			_collator = null;
			IsValid = false;
		}

		private ICollator CreateCollator()
		{
			string message;
			if (!IsValid && !Validate(out message))
				throw new InvalidOperationException(message);
			return new IcuRulesCollator(IcuRules);
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			return other != null && _type == other._type && IcuRules == other.IcuRules;
		}

		public override CollationDefinition Clone()
		{
			return new CollationDefinition(this);
		}
	}
}
