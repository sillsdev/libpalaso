using System;

namespace SIL.WritingSystems
{
	public abstract class CollationDefinition : DefinitionBase<CollationDefinition>
	{
		private readonly string _type;
		private string _collationRules;
		private ICollator _collator;

		protected CollationDefinition(string type)
		{
			_type = type;
		}

		protected CollationDefinition(CollationDefinition crd)
		{
			_type = crd._type;
			_collationRules = crd._collationRules;
		}

		public string Type
		{
			get { return _type; }
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

			IsValid = IcuRulesCollator.ValidateSortRules(CollationRules, out message);
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
			return new IcuRulesCollator(CollationRules);
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			return other != null && _type == other._type;
		}
	}
}
