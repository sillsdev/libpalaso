using SIL.Data;

namespace SIL.WritingSystems
{
	public abstract class CollationDefinition : DefinitionBase<CollationDefinition>
	{
		private readonly string _type;
		private ICollator _collator;

		protected CollationDefinition(string type)
		{
			_type = type;
		}

		protected CollationDefinition(CollationDefinition cd)
		{
			_type = cd._type;
		}

		public string Type
		{
			get { return _type; }
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
				{
					string message;
					if (!Validate(out message))
						throw new ValidationException(message);
					_collator = CreateCollator();
				}
				return _collator;
			}
		}

		public abstract bool Validate(out string message);

		public bool IsValid { get; set; }

		protected void ResetCollator()
		{
			_collator = null;
			IsValid = false;
		}

		protected abstract ICollator CreateCollator();

		public override bool ValueEquals(CollationDefinition other)
		{
			return other != null && _type == other._type;
		}
	}
}
