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
			if (LanguageSubtagContainsAudioPrivateUseTag)
			{
				MoveAudioPrivateUseTagFromIsoToPrivateUseTag();
				MoveExistingScriptToPrivateUseTag();
				_temporaryRfc5646TagHolder.Script = WellKnownSubTags.Audio.Script;
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
			foreach (var part in ParseSubtagForParts(_temporaryRfc5646TagHolder.Script))
			{
				_temporaryRfc5646TagHolder.AddToPrivateUse(part);

			}
		}

		private void CleanUpRfcTagForMigration()
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

		private void MoveAudioPrivateUseTagFromIsoToPrivateUseTag()
		{
			List<string> partsOfsubtag = ParseSubtagForParts(_temporaryRfc5646TagHolder.Language);
			partsOfsubtag.RemoveAll(str => str == WellKnownSubTags.Audio.PrivateUseSubtag);
			_temporaryRfc5646TagHolder.Language = AssembleSubtag(partsOfsubtag);
			_temporaryRfc5646TagHolder.PrivateUse = WellKnownSubTags.Audio.PrivateUseSubtag;
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
