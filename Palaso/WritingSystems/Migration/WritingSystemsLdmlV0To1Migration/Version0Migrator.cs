using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV1To2Migration;

namespace Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	public class Version0Migrator : IMigrationStrategy
	{
		readonly LdmlAdaptorV0 _adaptorToReadLdmlV0 = new LdmlAdaptorV0();
		readonly LdmlAdaptorV1 _adaptorToWriteLdmlV1 = new LdmlAdaptorV1();
		readonly WritingSystemDefinitionV0 _wsToMigrate = new WritingSystemDefinitionV0();
		readonly WritingSystemDefinitionV1 _migratedWs = new WritingSystemDefinitionV1();
		private RFC5646TagV0 _temporaryRfc5646TagHolder;

		public int FromVersion
		{
			get { return 0; }
		}

		public int ToVersion
		{
			get { return 1; }
		}

		public void Migrate(string sourceFilePath, string destinationFilePath)
		{
			_adaptorToReadLdmlV0.Read(sourceFilePath, _wsToMigrate);

			CopyRfc5646InfoFromWsToMigrateIntoRfc5646ClassForEasyManipulation(_wsToMigrate);
			CleanUpRfcTagForMigration();

			FixRfc5646Subtags();

			CopyTemporaryRfc5646InfoToMigratedWs(_migratedWs);

			WriteOutMigratedWs(sourceFilePath, destinationFilePath);
		}

		private void FixRfc5646Subtags()
		{
			MoveRegisteredVariantsInAllSubtagsToVariantSubtag();
			MoveIso3166CodesInAllSubtagsToRegionSubtag();
			MoveIso15924CodesInAllSubtagsToScriptSubtag();
			MoveIso639CodesInAllSubtagsToLanguageSubtag();
			MoveNonValidDataInAllSubtagsToPrivateUse();

			MoveAllButFirstPartInSubtagToPrivateUse(RFC5646TagV0.Subtags.Language);
			MoveAllButFirstPartInSubtagToPrivateUse(RFC5646TagV0.Subtags.Script);
			MoveAllButFirstPartInSubtagToPrivateUse(RFC5646TagV0.Subtags.Region);
			MoveDuplicatesinVariantToPrivateUse();

			bool privateUseTagContainsAudioMarker = _temporaryRfc5646TagHolder.SubtagContainsPart(WellKnownSubTags.Audio.PrivateUseSubtag, RFC5646TagV0.Subtags.PrivateUse);
			if(privateUseTagContainsAudioMarker)
			{
				SetScriptFieldToAudioConformantScript();
			}

			if (_temporaryRfc5646TagHolder.SubtagContainsPart(WellKnownSubTags.Ipa.VariantSubtag, RFC5646TagV0.Subtags.PrivateUse))
			{
				MoveContentFromSubtagToSubtag(WellKnownSubTags.Ipa.VariantSubtag, RFC5646TagV0.Subtags.PrivateUse, RFC5646TagV0.Subtags.Variant);
			}

			RemoveDuplicatesFromPrivateUse();
			RemoveAllNonAlphaNumericCharactersFromPrivateUse();
			if (String.IsNullOrEmpty(_temporaryRfc5646TagHolder.Language))
			{
				AddPrivateUseLanguageTag();
			}
		}

		private void RemoveAllNonAlphaNumericCharactersFromPrivateUse()
		{
			List<string> privateUseWithoutIllegalCharacters = new List<string>();
			foreach (var part in _temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.PrivateUse))
			{
				StringBuilder stringWithoutIllegalCharacters = new StringBuilder();
				foreach (char c in part)
				{
					if(Char.IsLetter(c) || Char.IsNumber(c))
					{
						stringWithoutIllegalCharacters.Append(c);
					}
				}
				if(!String.IsNullOrEmpty(stringWithoutIllegalCharacters.ToString()))
				{
					privateUseWithoutIllegalCharacters.Add(stringWithoutIllegalCharacters.ToString());
				}
			}
			_temporaryRfc5646TagHolder.PrivateUse = String.Empty;
			foreach (string part in privateUseWithoutIllegalCharacters)
			{
				_temporaryRfc5646TagHolder.AddToSubtag(part, RFC5646TagV0.Subtags.PrivateUse);
			}
		}

		private void MoveDuplicatesinVariantToPrivateUse()
		{
			List<string> duplicates = GetDuplicatesInSubtag(RFC5646TagV0.Subtags.Variant);
			List<string> variantWithOutDuplicates = new List<string>();
			variantWithOutDuplicates.AddRange(_temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.Variant));
			foreach (var part in duplicates)
			{
				variantWithOutDuplicates.Remove(part);
				_temporaryRfc5646TagHolder.AddToSubtag(part, RFC5646TagV0.Subtags.PrivateUse);
			}
			_temporaryRfc5646TagHolder.Variant = String.Empty;
			foreach (var part in variantWithOutDuplicates)
			{
				_temporaryRfc5646TagHolder.AddToSubtag(part, RFC5646TagV0.Subtags.Variant);
			}
		}

		private void RemoveDuplicatesFromPrivateUse()
		{
			List<string> duplicates = GetDuplicatesInSubtag(RFC5646TagV0.Subtags.PrivateUse);
			List<string> privateUseWithOutDuplicates = new List<string>();
			privateUseWithOutDuplicates.AddRange(_temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.PrivateUse));
			foreach (var part in duplicates)
			{
				privateUseWithOutDuplicates.Remove(part);
			}
			_temporaryRfc5646TagHolder.PrivateUse = String.Empty;
			foreach (var part in privateUseWithOutDuplicates)
			{
				_temporaryRfc5646TagHolder.AddToSubtag(part, RFC5646TagV0.Subtags.PrivateUse);
			}
		}

		private List<string> GetDuplicatesInSubtag(RFC5646TagV0.Subtags subtagToSearch)
		{
			List<string> duplicates = new List<string>();
			List<string> acceptedParts = new List<string>();
			foreach (var candidatePart in _temporaryRfc5646TagHolder.GetPartsOfSubtag(subtagToSearch))
			{
				bool doNotAcceptPart = false;
				foreach (var acceptedPart in acceptedParts)
				{
					if (candidatePart.Equals(acceptedPart, StringComparison.OrdinalIgnoreCase))
					{
						doNotAcceptPart = true;
						break;
					}
				}
				if (doNotAcceptPart)
				{
					duplicates.Add(candidatePart);
				}
				else
				{
					acceptedParts.Add(candidatePart);
				}
			}
			return duplicates;
		}

		private void SetScriptFieldToAudioConformantScript()
		{
			if (!_temporaryRfc5646TagHolder.Script.Equals(WellKnownSubTags.Audio.Script, StringComparison.OrdinalIgnoreCase))
			{
				MoveContentFromSubtagToSubtag(_temporaryRfc5646TagHolder.Script, RFC5646TagV0.Subtags.Script,
											  RFC5646TagV0.Subtags.PrivateUse);
				_temporaryRfc5646TagHolder.Script = WellKnownSubTags.Audio.Script;
			}
		}

		private void MoveNonValidDataInAllSubtagsToPrivateUse()
		{
			List<string> validCodes = new List<string>();

			validCodes = _temporaryRfc5646TagHolder.GetIso639CodesInSubtag(RFC5646TagV0.Subtags.Language);
			MovePartsOfSubtagNotFoundInListToPrivateUse(validCodes, RFC5646TagV0.Subtags.Language);

			validCodes = _temporaryRfc5646TagHolder.GetIso15924CodesInSubtag(RFC5646TagV0.Subtags.Script);
			MovePartsOfSubtagNotFoundInListToPrivateUse(validCodes, RFC5646TagV0.Subtags.Script);

			validCodes = _temporaryRfc5646TagHolder.GetIso3166RegionsInSubtag(RFC5646TagV0.Subtags.Region);
			MovePartsOfSubtagNotFoundInListToPrivateUse(validCodes, RFC5646TagV0.Subtags.Region);

			validCodes = _temporaryRfc5646TagHolder.GetRegisteredVariantsInSubtag(RFC5646TagV0.Subtags.Variant);
			MovePartsOfSubtagNotFoundInListToPrivateUse(validCodes, RFC5646TagV0.Subtags.Variant);
		}

		private void MovePartsOfSubtagNotFoundInListToPrivateUse(List<string> validCodes, RFC5646TagV0.Subtags subtagToMoveFrom)
		{
			List<string> partsInSubtagToMoveFrom = new List<string>();
			partsInSubtagToMoveFrom.AddRange(_temporaryRfc5646TagHolder.GetPartsOfSubtag(subtagToMoveFrom));
			foreach (string part in partsInSubtagToMoveFrom)
			{
				if(!validCodes.Contains(part))
				{
					MoveContentFromSubtagToSubtag(part, subtagToMoveFrom, RFC5646TagV0.Subtags.PrivateUse);
				}
			}
		}

		private void MoveRegisteredVariantsInAllSubtagsToVariantSubtag()
		{
			MoveValidRegisteredVariantsFromSubtagToVariantTag(RFC5646TagV0.Subtags.Language);
			MoveValidRegisteredVariantsFromSubtagToVariantTag(RFC5646TagV0.Subtags.Script);
			MoveValidRegisteredVariantsFromSubtagToVariantTag(RFC5646TagV0.Subtags.Region);
			MoveValidRegisteredVariantsFromSubtagToVariantTag(RFC5646TagV0.Subtags.PrivateUse);
		}

		private void MoveIso3166CodesInAllSubtagsToRegionSubtag()
		{
			MoveValidIso3166RegionTagsFromSubtagToRegionTag(RFC5646TagV0.Subtags.Language);
			MoveValidIso3166RegionTagsFromSubtagToRegionTag(RFC5646TagV0.Subtags.Script);
			MoveValidIso3166RegionTagsFromSubtagToRegionTag(RFC5646TagV0.Subtags.Variant);
			MoveValidIso3166RegionTagsFromSubtagToRegionTag(RFC5646TagV0.Subtags.PrivateUse);
		}

		private void MoveIso15924CodesInAllSubtagsToScriptSubtag()
		{
			MoveValidIso15924ScriptCodesFromSubtagToScriptTag(RFC5646TagV0.Subtags.Language);
			MoveValidIso15924ScriptCodesFromSubtagToScriptTag(RFC5646TagV0.Subtags.Region);
			MoveValidIso15924ScriptCodesFromSubtagToScriptTag(RFC5646TagV0.Subtags.Variant);
			MoveValidIso15924ScriptCodesFromSubtagToScriptTag(RFC5646TagV0.Subtags.PrivateUse);
		}

		private void MoveIso639CodesInAllSubtagsToLanguageSubtag()
		{
			MoveValidIso639ScriptCodesFromSubtagToLanguageTag(RFC5646TagV0.Subtags.Script);
			MoveValidIso639ScriptCodesFromSubtagToLanguageTag(RFC5646TagV0.Subtags.Region);
			MoveValidIso639ScriptCodesFromSubtagToLanguageTag(RFC5646TagV0.Subtags.Variant);
			MoveValidIso639ScriptCodesFromSubtagToLanguageTag(RFC5646TagV0.Subtags.PrivateUse);
		}

		private void MoveAllButFirstPartInSubtagToPrivateUse(RFC5646TagV0.Subtags subtag)
		{
			if (_temporaryRfc5646TagHolder.GetPartsOfSubtag(subtag).Count != 0)
			{
				List<string> partsToBeMoved = new List<string>();
				partsToBeMoved.AddRange(_temporaryRfc5646TagHolder.GetPartsOfSubtag(subtag));
				string partToSetSubtagTo = partsToBeMoved[0];
				partsToBeMoved.RemoveAt(0);
				foreach (string part in partsToBeMoved)
				{
					_temporaryRfc5646TagHolder.RemoveFromSubtag(part, subtag);
					_temporaryRfc5646TagHolder.AddToSubtag(part, RFC5646TagV0.Subtags.PrivateUse);
				}
				_temporaryRfc5646TagHolder.SetSubtag(partToSetSubtagTo, subtag);
			}
		}

		private void MoveContentFromSubtagToSubtag(string contentToMove, RFC5646TagV0.Subtags subtagToMoveFrom, RFC5646TagV0.Subtags subtagToMoveTo)
		{
			RemovePartFromSubtag(contentToMove, subtagToMoveFrom);
			AddPartToSubtag(contentToMove, subtagToMoveTo);
		}

		private void RemovePartFromSubtag(string part, RFC5646TagV0.Subtags subtagToRemoveFrom)
		{
			_temporaryRfc5646TagHolder.RemoveFromSubtag(part, subtagToRemoveFrom);
		}

		private void AddPartToSubtag(string part, RFC5646TagV0.Subtags subtagToAddTo)
		{
			_temporaryRfc5646TagHolder.AddToSubtag(part, subtagToAddTo);
		}

		private void MoveValidIso639ScriptCodesFromSubtagToLanguageTag(RFC5646TagV0.Subtags subtagToMoveFrom)
		{
			List<string> languageCodesInSubtag = _temporaryRfc5646TagHolder.GetIso639CodesInSubtag(subtagToMoveFrom);

			foreach (var languageCode in languageCodesInSubtag)
			{
				MoveContentFromSubtagToSubtag(languageCode, subtagToMoveFrom, RFC5646TagV0.Subtags.Language);
			}
		}

		private void MoveValidIso15924ScriptCodesFromSubtagToScriptTag(RFC5646TagV0.Subtags subtagToMoveFrom)
		{
			List<string> scriptCodesInSubtag = _temporaryRfc5646TagHolder.GetIso15924CodesInSubtag(subtagToMoveFrom);

			foreach (var scriptCode in scriptCodesInSubtag)
			{
				MoveContentFromSubtagToSubtag(scriptCode, subtagToMoveFrom, RFC5646TagV0.Subtags.Script);
			}
		}

		private void MoveValidIso3166RegionTagsFromSubtagToRegionTag(RFC5646TagV0.Subtags subtagToMoveFrom)
		{
			List<string> regionCodesInSubtag = _temporaryRfc5646TagHolder.GetIso3166RegionsInSubtag(subtagToMoveFrom);

			foreach (var scriptCode in regionCodesInSubtag)
			{
				MoveContentFromSubtagToSubtag(scriptCode, subtagToMoveFrom, RFC5646TagV0.Subtags.Region);
			}
		}

		private void MoveValidRegisteredVariantsFromSubtagToVariantTag(RFC5646TagV0.Subtags subtagToMoveFrom)
		{
			List<string> registeredVariantsInSubtag =_temporaryRfc5646TagHolder.GetRegisteredVariantsInSubtag(subtagToMoveFrom);

			foreach (var registeredVariant in registeredVariantsInSubtag)
			{
				MoveContentFromSubtagToSubtag(registeredVariant, subtagToMoveFrom, RFC5646TagV0.Subtags.Variant);
			}
		}

		protected bool ScriptSubtagContainsValidIso15924Code
		{
			get
			{
				foreach (string scriptSubtagpart in _temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.Script))
				{
					if (RFC5646TagV1.IsValidIso15924ScriptCode(scriptSubtagpart)) { return true; }
				}
				return false;
			}
		}

		protected bool LanguageSubtagContainsValidIso15924ScriptCodes
		{
			get
			{
				foreach (string scriptSubtagpart in _temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.Language))
				{
					if (RFC5646TagV1.IsValidIso15924ScriptCode(scriptSubtagpart)) { return true; }
				}
				return false;
			}
		}

		protected bool LanguageSubtagContainsValidRegisteredVariant
		{
			get
			{
				foreach (string languageSubtagpart in _temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.Language))
				{
					if (RFC5646TagV1.IsValidRegisteredVariant(languageSubtagpart)) { return true; }
				}
				return false;
			}
		}

		protected bool LanguageSubtagContainsValidIso3166RegionCodes
		{
			get
			{
				foreach (string languageSubtagpart in _temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.Language))
				{
					if (RFC5646TagV1.IsValidIso3166Region(languageSubtagpart)) { return true; }
				}
				return false;
			}
		}

		protected bool LanguageSubtagContainsValidIso639Code
		{
			get
			{
				foreach (string languageSubtagpart in _temporaryRfc5646TagHolder.GetPartsOfSubtag(RFC5646TagV0.Subtags.Language))
				{
					if (RFC5646TagV1.IsValidIso639LanguageCode(languageSubtagpart)) {return true; }
				}
				return false;
			}
		}

		private void AddPrivateUseLanguageTag()
		{
			_temporaryRfc5646TagHolder.Language = "qaa";
		}

		private void CleanUpRfcTagForMigration()
		{
			RemoveAnyXsFromAllSubtags();
		}

		private void RemoveAnyXsFromAllSubtags()
		{
			_temporaryRfc5646TagHolder.RemoveFromSubtag("x", RFC5646TagV0.Subtags.Language);
			_temporaryRfc5646TagHolder.RemoveFromSubtag("x", RFC5646TagV0.Subtags.Script);
			_temporaryRfc5646TagHolder.RemoveFromSubtag("x", RFC5646TagV0.Subtags.Region);
			_temporaryRfc5646TagHolder.RemoveFromSubtag("x", RFC5646TagV0.Subtags.Variant);
			_temporaryRfc5646TagHolder.RemoveFromSubtag("x", RFC5646TagV0.Subtags.PrivateUse);
		}

		private void WriteOutMigratedWs(string sourceFilePath, string destinationFilePath)
		{
			Stream streamOfOldFile = new FileStream(sourceFilePath, FileMode.Open);
			_adaptorToWriteLdmlV1.Write(destinationFilePath, _migratedWs, streamOfOldFile);
			streamOfOldFile.Close();
		}

		private void CopyTemporaryRfc5646InfoToMigratedWs(WritingSystemDefinitionV1 migratedWs)
		{
			migratedWs.ISO639 = _temporaryRfc5646TagHolder.Language;
			migratedWs.Script = _temporaryRfc5646TagHolder.Script;
			migratedWs.Region = _temporaryRfc5646TagHolder.Region;
			migratedWs.Variant = _temporaryRfc5646TagHolder.VariantAndPrivateUse;
		}

		private void CopyRfc5646InfoFromWsToMigrateIntoRfc5646ClassForEasyManipulation(WritingSystemDefinitionV0 wsToMigrate)
		{
			_temporaryRfc5646TagHolder = new RFC5646TagV0(wsToMigrate.ISO639, wsToMigrate.Script, wsToMigrate.Region, wsToMigrate.Variant, String.Empty);
		}

		private class WellKnownSubTags
		{
			public class Audio
			{
				static public string PrivateUseSubtag
				{
					get { return "audio"; }
				}
				static public string Script
				{
					get { return "Zxxx"; }
				}
			}

			public class Ipa
			{
				static public string VariantSubtag
				{
					get { return "fonipa"; }
				}

				static public string PhonemicPrivateUseSubtag
				{
					get { return "-x-emic"; }
				}

				static public string PhoneticPrivateUseSubtag
				{
					get { return "-x-etic"; }
				}
			}
		}
	}
}
