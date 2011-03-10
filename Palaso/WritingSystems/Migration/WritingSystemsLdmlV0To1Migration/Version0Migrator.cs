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
		private enum Subtags
		{
			Language,
			Script,
			Region,
			Variant,
			PrivateUse
		}

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
			if (LanguageSubtagContainsAudioPrivateUseTag)
			{
				MoveContentFromSubtagToSubtag(WellKnownSubTags.Audio.PrivateUseSubtag, Subtags.Language, Subtags.PrivateUse);   //this is redundant but makes the intent clear
				if (_wsToMigrate.Script != WellKnownSubTags.Audio.Script)
				{
					MoveExistingScriptToPrivateUseTag();
				}
				_temporaryRfc5646TagHolder.Script = WellKnownSubTags.Audio.Script;
			}
			if (LanguageSubtagContainsPhoneticVariantMarker)
			{
				MoveContentFromSubtagToSubtag(WellKnownSubTags.Ipa.IpaVariantSubtag, Subtags.Language, Subtags.Variant);
			}
			if (LanguageSubtagContainsValidIso15924ScriptCodes)
			{
				MoveValidIso15924ScriptCodesFromSubtagToScriptTag(Subtags.Language);
			}
			if (LanguageSubtagContainsValidIso3166RegionCodes)
			{
				MoveValidIso3166RegionTagsFromSubtagToRegionTag(Subtags.Language);
			}
			if (LanguageSubtagContainsValidRegisteredVariant)
			{
				MoveValidRegisteredVariantsFromSubtagToVariantTag(Subtags.Language);
			}


			if (ScriptSubtagContainsAudioPrivateUseTag)
			{
				MoveAudioPrivateUseTagFromScriptToPrivateUseTag();
				MoveExistingScriptToPrivateUseTag();
				_temporaryRfc5646TagHolder.Script = WellKnownSubTags.Audio.Script;
			}
			if(LanguageSubtagContainsValidIso639Code)
			{
				SetLanguageSubtagToFirstValidIso639CodeAndMoveAnyOtherSubtagsToPrivateUse();
			}
			if (ScriptSubtagContainsValidIso15924Code)
			{
				SetScriptSubtagToFirstValidIso15924CodeAndMoveAnyOtherSubtagsToPrivateUse();
			}
			if (String.IsNullOrEmpty(_wsToMigrate.ISO639))
			{
				AddPrivateUseLanguageTag();
			}
		}

		private void MoveContentFromSubtagToSubtag(string contentToMove, Subtags subtagToMoveFrom, Subtags subtagToMoveTo)
		{
			RemovePartFromSubtag(contentToMove, subtagToMoveFrom);
			AddPartToSubtag(contentToMove, subtagToMoveTo);
		}

		private void RemovePartFromSubtag(string part, Subtags subtagToRemoveFrom)
		{
			switch (subtagToRemoveFrom)
			{
				case Subtags.Language:
					_temporaryRfc5646TagHolder.RemoveFromLanguage(part);
					break;
				case Subtags.Script:
					_temporaryRfc5646TagHolder.RemoveFromScript(part);
					break;
				case Subtags.Region:
					_temporaryRfc5646TagHolder.RemoveFromRegion(part);
					break;
				case Subtags.Variant:
					_temporaryRfc5646TagHolder.RemoveFromVariant(part);
					break;
				case Subtags.PrivateUse:
					_temporaryRfc5646TagHolder.RemoveFromPrivateUse(part);
					break;
				default:
					throw new ApplicationException(String.Format("{0} is an invalid subtag.", subtagToRemoveFrom));
			}
		}

		private void AddPartToSubtag(string part, Subtags subtagToAddTo)
		{
			switch (subtagToAddTo)
			{
				case Subtags.Language:
					_temporaryRfc5646TagHolder.AddToLanguage(part);
					break;
				case Subtags.Script:
					_temporaryRfc5646TagHolder.AddToScript(part);
					break;
				case Subtags.Region:
					_temporaryRfc5646TagHolder.AddToRegion(part);
					break;
				case Subtags.Variant:
					_temporaryRfc5646TagHolder.AddToVariant(part);
					break;
				case Subtags.PrivateUse:
					_temporaryRfc5646TagHolder.AddToPrivateUse(part);
					break;
				default:
					throw new ApplicationException(String.Format("{0} is an invalid subtag.", subtagToAddTo));
			}
		}

		private void MoveValidIso15924ScriptCodesFromSubtagToScriptTag(Subtags subtagToMoveFrom)
		{
			List<string> scriptCodesInSubtag = new List<string>();
			switch (subtagToMoveFrom)
			{
				case Subtags.Language:
					scriptCodesInSubtag = _temporaryRfc5646TagHolder.GetIso15924CodesInLanguageSubtag();
					break;
				case Subtags.Script:
					scriptCodesInSubtag = _temporaryRfc5646TagHolder.GetIso15924CodesInScriptSubtag();
					break;
				case Subtags.Region:
					scriptCodesInSubtag = _temporaryRfc5646TagHolder.GetIso15924CodesInRegionSubtag();
					break;
				case Subtags.Variant:
					scriptCodesInSubtag = _temporaryRfc5646TagHolder.GetIso15924CodesInVariantSubtag();
					break;
				case Subtags.PrivateUse:
					scriptCodesInSubtag = _temporaryRfc5646TagHolder.GetIso15924CodesInPrivateUseSubtag();
					break;
				default:
					throw new ApplicationException(String.Format("{0} is an invalid subtag.", subtagToMoveFrom));
			}

			foreach (var scriptCode in scriptCodesInSubtag)
			{
				MoveContentFromSubtagToSubtag(scriptCode, subtagToMoveFrom, Subtags.Script);
			}
		}

		private void MoveValidIso3166RegionTagsFromSubtagToRegionTag(Subtags subtagToMoveFrom)
		{
			List<string> regionCodesInSubtag = new List<string>();
			switch (subtagToMoveFrom)
			{
				case Subtags.Language:
					regionCodesInSubtag = _temporaryRfc5646TagHolder.GetIso3166RegionsInLanguageSubtag();
					break;
				case Subtags.Script:
					regionCodesInSubtag = _temporaryRfc5646TagHolder.GetIso3166RegionsInScriptSubtag();
					break;
				case Subtags.Region:
					regionCodesInSubtag = _temporaryRfc5646TagHolder.GetIso3166RegionsInRegionSubtag();
					break;
				case Subtags.Variant:
					regionCodesInSubtag = _temporaryRfc5646TagHolder.GetIso3166RegionsInVariantSubtag();
					break;
				case Subtags.PrivateUse:
					regionCodesInSubtag = _temporaryRfc5646TagHolder.GetIso3166RegionsInPrivateUseSubtag();
					break;
				default:
					throw new ApplicationException(String.Format("{0} is an invalid subtag.", subtagToMoveFrom));
			}

			foreach (var scriptCode in regionCodesInSubtag)
			{
				MoveContentFromSubtagToSubtag(scriptCode, subtagToMoveFrom, Subtags.Region);
			}
		}

		private void MoveValidRegisteredVariantsFromSubtagToVariantTag(Subtags subtagToMoveFrom)
		{
			List<string> registeredVariantsInSubtag = new List<string>();
			switch (subtagToMoveFrom)
			{
				case Subtags.Language:
					registeredVariantsInSubtag = _temporaryRfc5646TagHolder.GetRegisteredVariantsInLanguageSubtag();
					break;
				case Subtags.Script:
					registeredVariantsInSubtag = _temporaryRfc5646TagHolder.GetRegisteredVariantsInScriptSubtag();
					break;
				case Subtags.Region:
					registeredVariantsInSubtag = _temporaryRfc5646TagHolder.GetRegisteredVariantsInRegionSubtag();
					break;
				case Subtags.Variant:
					registeredVariantsInSubtag = _temporaryRfc5646TagHolder.GetRegisteredVariantsInVariantSubtag();
					break;
				case Subtags.PrivateUse:
					registeredVariantsInSubtag = _temporaryRfc5646TagHolder.GetRegisteredVariantsInPrivateUseSubtag();
					break;
				default:
					throw new ApplicationException(String.Format("{0} is an invalid subtag.", subtagToMoveFrom));
			}

			foreach (var registeredVariant in registeredVariantsInSubtag)
			{
				MoveContentFromSubtagToSubtag(registeredVariant, subtagToMoveFrom, Subtags.Variant);
			}
		}

		private void MovePhoneticVariantTagFromLanguageSubtagToVariantTag()
		{
			_temporaryRfc5646TagHolder.RemoveFromLanguage(WellKnownSubTags.Ipa.IpaVariantSubtag);
			_temporaryRfc5646TagHolder.AddToVariant(WellKnownSubTags.Ipa.IpaVariantSubtag);
		}

		private bool LanguageSubtagContainsPhoneticVariantMarker
		{
			get
			{
				return _temporaryRfc5646TagHolder.Language.Contains(WellKnownSubTags.Ipa.IpaVariantSubtag,
																 StringComparison.OrdinalIgnoreCase);
			}
		}

		private void SetScriptSubtagToFirstValidIso15924CodeAndMoveAnyOtherSubtagsToPrivateUse()
		{
			string validSubtag = "";

			foreach (string scriptSubtagpart in ParseSubtagForParts(_temporaryRfc5646TagHolder.Script))
			{
				if (RFC5646TagV1.IsValidIso15924ScriptCode(scriptSubtagpart) && String.IsNullOrEmpty(validSubtag))
				{
					validSubtag = scriptSubtagpart;
				}
				else
				{
					_temporaryRfc5646TagHolder.AddToPrivateUse(scriptSubtagpart);
				}
			}
			_temporaryRfc5646TagHolder.Script = validSubtag;
		}

		protected bool ScriptSubtagContainsValidIso15924Code
		{
			get
			{
				foreach (string scriptSubtagpart in ParseSubtagForParts(_temporaryRfc5646TagHolder.Script))
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
				foreach (string scriptSubtagpart in ParseSubtagForParts(_temporaryRfc5646TagHolder.Language))
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
				foreach (string languageSubtagpart in ParseSubtagForParts(_temporaryRfc5646TagHolder.Language))
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
				foreach (string languageSubtagpart in ParseSubtagForParts(_temporaryRfc5646TagHolder.Language))
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
				foreach (string languageSubtagpart in ParseSubtagForParts(_temporaryRfc5646TagHolder.Language))
				{
					if (RFC5646TagV1.IsValidIso639LanguageCode(languageSubtagpart)) {return true; }
				}
				return false;
			}
		}

		private void SetLanguageSubtagToFirstValidIso639CodeAndMoveAnyOtherSubtagsToPrivateUse()
		{
			string validSubtag = "";

			foreach (string languageSubtagpart in ParseSubtagForParts(_temporaryRfc5646TagHolder.Language))
			{
				if (RFC5646TagV1.IsValidIso639LanguageCode(languageSubtagpart) && String.IsNullOrEmpty(validSubtag))
				{
					validSubtag = languageSubtagpart;
				}
				else
				{
					_temporaryRfc5646TagHolder.AddToPrivateUse(languageSubtagpart);
				}
			}
			_temporaryRfc5646TagHolder.Language = validSubtag;
		}

		private void AddPrivateUseLanguageTag()
		{
			_temporaryRfc5646TagHolder.Language = "qaa";
		}

		private void MoveExistingScriptToPrivateUseTag()
		{
			_temporaryRfc5646TagHolder.AddToPrivateUse(_temporaryRfc5646TagHolder.Script);
		}

		private void CleanUpRfcTagForMigration()
		{
			RemoveAnyXsFromAllSubtags();
		}

		private void RemoveAnyXsFromAllSubtags()
		{
			List<string> partsofSubtag = ParseSubtagForParts(_temporaryRfc5646TagHolder.Language);
			partsofSubtag.RemoveAll(str => str.Equals("x", StringComparison.OrdinalIgnoreCase));
			_temporaryRfc5646TagHolder.Language = AssembleSubtag(partsofSubtag);

			partsofSubtag = ParseSubtagForParts(_temporaryRfc5646TagHolder.Script);
			partsofSubtag.RemoveAll(str => str.Equals("x", StringComparison.OrdinalIgnoreCase));
			_temporaryRfc5646TagHolder.Script = AssembleSubtag(partsofSubtag);

			partsofSubtag = ParseSubtagForParts(_temporaryRfc5646TagHolder.Region);
			partsofSubtag.RemoveAll(str => str.Equals("x", StringComparison.OrdinalIgnoreCase));
			_temporaryRfc5646TagHolder.Region = AssembleSubtag(partsofSubtag);

			partsofSubtag = ParseSubtagForParts(_temporaryRfc5646TagHolder.Variant);
			partsofSubtag.RemoveAll(str => str.Equals("x", StringComparison.OrdinalIgnoreCase));
			_temporaryRfc5646TagHolder.Variant = AssembleSubtag(partsofSubtag);
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

		public static List<string> ParseSubtagForParts(string subtagToParse)
		{
			var parts = new List<string>();
			parts.AddRange(subtagToParse.Split('-'));
			parts.RemoveAll(str => str == "");
			return parts;
		}

		private static string AssembleSubtag(IEnumerable<string> subtag)
		{
			string subtagAsString = "";
			foreach (string part in subtag)
			{
				if (!String.IsNullOrEmpty(subtagAsString))
				{
					subtagAsString = subtagAsString + "-";
				}
				subtagAsString = subtagAsString + part;
			}
			return subtagAsString;
		}

		private void MoveAudioPrivateUseTagFromLanguageSubtagToPrivateUseTag()
		{
			_temporaryRfc5646TagHolder.RemoveFromLanguage(WellKnownSubTags.Audio.PrivateUseSubtag);
			_temporaryRfc5646TagHolder.AddToPrivateUse(WellKnownSubTags.Audio.PrivateUseSubtag);
		}

		private void MoveAudioPrivateUseTagFromScriptToPrivateUseTag()
		{
			List<string> partsOfsubtag = ParseSubtagForParts(_temporaryRfc5646TagHolder.Script);
			partsOfsubtag.RemoveAll(str => str == WellKnownSubTags.Audio.PrivateUseSubtag);
			_temporaryRfc5646TagHolder.Script = AssembleSubtag(partsOfsubtag);
			_temporaryRfc5646TagHolder.PrivateUse = WellKnownSubTags.Audio.PrivateUseSubtag;
		}

		private bool LanguageSubtagContainsAudioPrivateUseTag
		{
			get
			{
				return _temporaryRfc5646TagHolder.Language.Contains(WellKnownSubTags.Audio.PrivateUseSubtag,
																	StringComparison.OrdinalIgnoreCase);
			}
		}

		private bool ScriptSubtagContainsAudioPrivateUseTag
		{
			get
			{
				return _temporaryRfc5646TagHolder.Script.Contains(WellKnownSubTags.Audio.PrivateUseSubtag,
																	StringComparison.OrdinalIgnoreCase);
			}
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
				static public string IpaVariantSubtag
				{
					get { return "fonipa"; }
				}

				static public string IpaPhonemicPrivateUseSubtag
				{
					get { return "-x-emic"; }
				}

				static public string IpaPhoneticPrivateUseSubtag
				{
					get { return "-x-etic"; }
				}
			}
		}
	}
}
