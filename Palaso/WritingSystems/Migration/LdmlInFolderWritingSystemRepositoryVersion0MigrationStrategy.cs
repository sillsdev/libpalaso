using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV1To2Migration;
using System.Text.RegularExpressions;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryVersion0MigrationStrategy : IMigrationStrategy
	{
		public int FromVersion
		{
			get { return 0; }
		}

		public int ToVersion
		{
			get { return 1; }
		}

		private Dictionary<string, KeyValuePair<string, string>> _fileToOldAndNewRfcTagMap = new Dictionary<string, KeyValuePair<string, string>>();

		public void Migrate(string directoryToMigrate, string destinationDirectory)
		{
			foreach (var pathToFileToMigrate in Directory.GetFiles(directoryToMigrate))
			{
				Migrator individualFileMigrator = GetMigratorForSingleLdmlFile(pathToFileToMigrate);
				if(individualFileMigrator.NeedsMigration())
				{
					string rfcTagBeforeMigration = GetRfcTagFromFileV0(pathToFileToMigrate);
					individualFileMigrator.Migrate();
					var ws = new WritingSystemDefinitionV1();
					new LdmlAdaptorV1().Read(pathToFileToMigrate, ws);
					if(ws.DuplicateNumber != 0)
					{
						ws.DuplicateNumber = 0; //this removes all duplicate markers
					}
					string rfcTagAfterMigration = GetRfcTagFromFileV1(pathToFileToMigrate);

					var oldToNewRfcTagMap = new KeyValuePair<string, string>(rfcTagBeforeMigration, rfcTagAfterMigration);
					_fileToOldAndNewRfcTagMap.Add(pathToFileToMigrate, oldToNewRfcTagMap);
				}
			}

			var copyOfFileToOldAndNewRfcTagMap = new Dictionary<string, KeyValuePair<string, string>>();
			foreach (var map in _fileToOldAndNewRfcTagMap)
			{
				copyOfFileToOldAndNewRfcTagMap.Add(map.Key, map.Value);
			}

			var uniqueRfcTagsToFilenameMap = new Dictionary<string, string>();
			foreach (var map in copyOfFileToOldAndNewRfcTagMap)
			{
				string rfcTagInQuestion = map.Value.Value;
				if (uniqueRfcTagsToFilenameMap.Keys.Any(rfcTag => rfcTag.Equals(rfcTagInQuestion, StringComparison.OrdinalIgnoreCase)))
				{
					if(map.Value.Key.Equals(rfcTagInQuestion, StringComparison.OrdinalIgnoreCase))
					{
						WritingSystemDefinitionV1 ws = new WritingSystemDefinitionV1();
						var adaptor = new LdmlAdaptorV1();
						string duplicateRfcTag =
							uniqueRfcTagsToFilenameMap.Keys.First(
								rfcTag => rfcTag.Equals(rfcTagInQuestion, StringComparison.OrdinalIgnoreCase));
						string fileContainingRfcTag = uniqueRfcTagsToFilenameMap[duplicateRfcTag];
						adaptor.Read(fileContainingRfcTag, ws);
						ws.DuplicateNumber++;
						File.Move(fileContainingRfcTag, fileContainingRfcTag + ".bak");
						var streamFromOldFile = new FileStream(fileContainingRfcTag + ".bak", FileMode.Open);
						adaptor.Write(fileContainingRfcTag, ws, streamFromOldFile);
						streamFromOldFile.Close();
						File.Delete(fileContainingRfcTag + ".bak");
						var oldToNewRfcTagsMap = _fileToOldAndNewRfcTagMap[fileContainingRfcTag];
						oldToNewRfcTagsMap = new KeyValuePair<string, string>(oldToNewRfcTagsMap.Key, ws.RFC5646);
						_fileToOldAndNewRfcTagMap[fileContainingRfcTag] = oldToNewRfcTagsMap;
						uniqueRfcTagsToFilenameMap.Add(ws.RFC5646, fileContainingRfcTag);
						uniqueRfcTagsToFilenameMap[rfcTagInQuestion] = map.Key;
					}
					else
					{
						WritingSystemDefinitionV1 ws = new WritingSystemDefinitionV1();
						var adaptor = new LdmlAdaptorV1();
						adaptor.Read(map.Key, ws);
						ws.DuplicateNumber++;
						File.Move(map.Key, map.Key + ".bak");
						var streamFromOldFile = new FileStream(map.Key + ".bak", FileMode.Open);
						adaptor.Write(map.Key, ws, streamFromOldFile);
						streamFromOldFile.Close();
						File.Delete(map.Key + ".bak");
						var rfcTagsInFileToRfcTagsMap = _fileToOldAndNewRfcTagMap[map.Key];
						rfcTagsInFileToRfcTagsMap = new KeyValuePair<string, string>(rfcTagsInFileToRfcTagsMap.Key, ws.RFC5646);
						_fileToOldAndNewRfcTagMap[map.Key] = rfcTagsInFileToRfcTagsMap;
						uniqueRfcTagsToFilenameMap[ws.RFC5646] = map.Key;
					}
				}
				else
				{
					uniqueRfcTagsToFilenameMap[rfcTagInQuestion] = map.Key;
				}
			}

			foreach (var file in Directory.GetFiles(directoryToMigrate))
			{
				string newFileName = _fileToOldAndNewRfcTagMap[file].Value + ".ldml";
				string pathTomoveTo = Path.Combine(destinationDirectory, newFileName);
				File.Move(file, pathTomoveTo);
			}
		}

		private void MarkWritingSystemAsNextFreeDuplicateAndRenameFileAccordingly(string pathToFileToFindFreePathFor)
		{
			string pathToDirectory = Path.GetDirectoryName(pathToFileToFindFreePathFor);
			string fileExtension = Path.GetExtension(pathToFileToFindFreePathFor);
			WritingSystemDefinitionV1 ws = new WritingSystemDefinitionV1();
			var ldmlV1Adaptor = new LdmlAdaptorV1();
			ldmlV1Adaptor.Read(pathToFileToFindFreePathFor, ws);
			string nextCandidatePath;
			do
			{
				ws.DuplicateNumber++;
				nextCandidatePath = Path.Combine(pathToDirectory, ws.RFC5646) + fileExtension;
			} while (File.Exists(nextCandidatePath));
			var streamFromoldFile = new FileStream(pathToFileToFindFreePathFor, FileMode.Open);
			ldmlV1Adaptor.Write(nextCandidatePath, ws, streamFromoldFile);
			streamFromoldFile.Close();
			File.Delete(pathToFileToFindFreePathFor);
		}

		private Migrator GetMigratorForSingleLdmlFile(string pathToFileToMigrate)
		{
			var individualFileMigrator = new Migrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, pathToFileToMigrate);
			individualFileMigrator.AddVersionStrategy(new WritingSystemLdmlVersionGetter());
			IMigrationStrategy ldmlV0MigrationStrategy = new Version0MigrationStrategy();
			individualFileMigrator.AddMigrationStrategy(ldmlV0MigrationStrategy);
			return individualFileMigrator;
		}

		private string GetRfcTagFromFileV1(string pathToFileToMigrate)
		{
			var ws = new WritingSystemDefinitionV1();
			new LdmlAdaptorV1().Read(pathToFileToMigrate, ws);
			return ws.RFC5646;
		}

		private string GetRfcTagFromFileV0(string pathToFileToMigrate)
		{
			var ws = new WritingSystemDefinitionV0();
			new LdmlAdaptorV0().Read(pathToFileToMigrate, ws);
			return ws.Rfc5646;
		}


		private bool NameOfFileMatchesRfcTagInFile(string pathToFileToMigrate)
		{
			var ws = new WritingSystemDefinitionV1();
			new LdmlAdaptorV1().Read(pathToFileToMigrate, ws);
			return Path.GetFileNameWithoutExtension(pathToFileToMigrate).Equals(ws.RFC5646);
		}
	}
}
