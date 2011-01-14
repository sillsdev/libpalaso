using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	public class WritingSystemDefinitionValidator
	{
		static private bool ScriptTagIsNonconformantToAudioWritingSystem(WritingSystemDefinition ws)
		{
			if (StringsAreEqualAccordingToLdmlSpec(WellKnownSubTags.Audio.Script, ws.Script)) return false;
			return true;
		}

		static private bool VariantTagIndicatesAudioWritingSystem(WritingSystemDefinition ws)
		{
			if(StringContainsSubstringAccordingToLdmlSpec(ws.Variant, WellKnownSubTags.Audio.VariantMarker)) return true;
			return false;
		}

		private static bool StringsAreEqualAccordingToLdmlSpec(string shouldValue, string isValue)
		{
			return String.Equals(shouldValue, isValue,StringComparison.InvariantCultureIgnoreCase); //This comparer should be extended to be "-"/"_" insensitive as well.
		}

		static private bool LanguageSubTagIsEmpty(WritingSystemDefinition ws)
		{
			if(String.IsNullOrEmpty(ws.ISO)) return true;
			return false;
		}

		static public bool IsValidWritingSystem(WritingSystemDefinition ws)
		{
			if (LanguageSubTagIsEmpty(ws)) return false;
			if (LanguageSubTagContainsXDashAudio(ws)) return false;
			if (LanguageSubTagContainsZxxx(ws)) return false;
			if (VariantTagIndicatesAudioWritingSystem(ws))
			{
				if (ScriptTagIsNonconformantToAudioWritingSystem(ws))   return false;
			}
			return true;
		}

		private static bool LanguageSubTagContainsZxxx(WritingSystemDefinition ws)
		{
			return StringContainsSubstringAccordingToLdmlSpec(ws.ISO, WellKnownSubTags.Audio.Script);
		}

		private static bool LanguageSubTagContainsXDashAudio(WritingSystemDefinition ws)
		{
			return StringContainsSubstringAccordingToLdmlSpec(ws.ISO,WellKnownSubTags.Audio.VariantMarker);
		}

		private static bool StringContainsSubstringAccordingToLdmlSpec(string stringToSearch, string stringToFind)
		{
			return stringToSearch.Contains(stringToFind, StringComparison.InvariantCultureIgnoreCase); //This comparer should be extended to be "-"/"_" insensitive as well.
		}
	}
}
