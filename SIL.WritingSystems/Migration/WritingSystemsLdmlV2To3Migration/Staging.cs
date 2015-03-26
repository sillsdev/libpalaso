using System.Collections.Generic;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV2To3Migration
{
	/// <summary>
	/// This class is used to temporarily hold properties of WritingSystemDefinitionV1 which need to be migrated.
	/// Generally, these are elements from legacy LDML files that are moved to standardized regions of LDML.
	/// Intentionally not using any WritingSystemDefinition subtypes to keep this class decoupled from 
	/// future updates in the WritingSystemDefinition.
	/// </summary>
	internal class Staging
	{
		public Staging()
		{
			KnownKeyboardIds = new List<string>();
			CharacterSets = new Dictionary<string, string>();
		}

		public string WindowsLcid { get; set; }
		public string VariantName { get; set; }

		public string DefaultFontName { get; set; }
		public string DefaultFontFeatures { get; set; }

		public List<string> KnownKeyboardIds { get; private set; }

		// Will only have 1 collation to migrate from legacy files
		public WritingSystemDefinitionV1.SortRulesType SortUsing { get; set; }
		public string SortRules { get; set; }

		/// <summary>
		/// Dictionary of character set type to the string of characters in that set
		/// </summary>
		public Dictionary<string, string> CharacterSets { get; private set; }

	}
}
