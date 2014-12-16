using System.Collections.Generic;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	public class FontDefinitionCloneableTests : CloneableTests<FontDefinition>
	{
		public override FontDefinition CreateNewCloneable()
		{
			return new FontDefinition("font1"); 
		}

		protected override bool Equals(FontDefinition x, FontDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList
		{
			get { return "|_roles|Modified|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(12.0f, 13.0f),
					new ValuesToSet(FontEngines.Graphite, FontEngines.OpenType)
				};
			}
		}
	}
}
