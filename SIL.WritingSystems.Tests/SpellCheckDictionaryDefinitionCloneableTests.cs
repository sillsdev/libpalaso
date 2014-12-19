using System.Collections.Generic;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	public class SpellCheckDictionaryDefinitionCloneableTests : CloneableTests<SpellCheckDictionaryDefinition>
	{
		public override SpellCheckDictionaryDefinition CreateNewCloneable()
		{
			return new SpellCheckDictionaryDefinition("dict1");
		}

		protected override bool Equals(SpellCheckDictionaryDefinition x, SpellCheckDictionaryDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList
		{
			get { return "|Modified|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(SpellCheckDictionaryFormat.Hunspell, SpellCheckDictionaryFormat.Wordlist)
				};
			}
		}
	}
}
