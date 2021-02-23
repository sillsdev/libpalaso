namespace SIL.DictionaryServices.Model
{
	public class LexRelationType
	{
		public enum Multiplicities
		{
			One,
			Many
		}

		public enum TargetTypes
		{
			Entry,
			Sense
		}

		public LexRelationType(string id, Multiplicities multiplicity, TargetTypes targetType)
		{
			ID = id;
			TargetType = targetType;
			Multiplicity = multiplicity;
		}

		public string ID { get; }

		public Multiplicities Multiplicity { get; }

		public TargetTypes TargetType { get; }
	}
}