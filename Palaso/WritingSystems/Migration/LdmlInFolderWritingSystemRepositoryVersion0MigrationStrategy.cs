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
		private ConsumerLevelRfcTagChanger _rfcTagChanger;

		public LdmlInFolderWritingSystemRepositoryVersion0MigrationStrategy(ConsumerLevelRfcTagChanger rfcTagChanger)
		{
			_rfcTagChanger = rfcTagChanger;
		}

		public int FromVersion
		{
			get { return 0; }
		}

		public int ToVersion
		{
			get { return 1; }
		}

		private Dictionary<string, KeyValuePair<string, string>> _fileToOldAndNewRfcTagMap = new Dictionary<string, KeyValuePair<string, string>>();
		private Dictionary<string, string> _uniqueRfcTagsToFilenameMap = new Dictionary<string, string>();

		public void Migrate(string directoryToMigrate, string destinationDirectory)
		{
			MigrateIndividualFiles(directoryToMigrate);

			DisambiguateWritingSystemsInLdmlRepo();

			MoveFilesToFinalDestinationWhileRenamingThemToMatchContainedRfcTags(directoryToMigrate, destinationDirectory);

			Dictionary<string, string> oldToNewRfcTagMap = new Dictionary<string, string>();
			foreach (var map in _fileToOldAndNewRfcTagMap)
			{
				oldToNewRfcTagMap.Add(map.Value.Key, map.Value.Value);
			}
			_rfcTagChanger(oldToNewRfcTagMap);
		}

		private void MoveFilesToFinalDestinationWhileRenamingThemToMatchContainedRfcTags(string directoryToMigrate, string destinationDirectory)
		{
			foreach (var file in Directory.GetFiles(directoryToMigrate))
			{
				string newFileName = _fileToOldAndNewRfcTagMap[file].Value + ".ldml";
				string pathTomoveTo = Path.Combine(destinationDirectory, newFileName);
				File.Move(file, pathTomoveTo);
			}
		}

		private void DisambiguateWritingSystemsInLdmlRepo()
		{

			List<string> ldmlFiles = new List<string>();
			ldmlFiles.AddRange(_fileToOldAndNewRfcTagMap.Keys);
			foreach (var file in ldmlFiles)
			{
				string RfcTagWithoutDuplicateMarker = RemoveDuplicateMarkerFromWsInLdmlFile(file);
				UpdateFileToOldAndNewRfcTagMap(file, RfcTagWithoutDuplicateMarker);
			}

			var copyOfFileToOldAndNewRfcTagMap = new Dictionary<string, KeyValuePair<string, string>>();
			foreach (var map in _fileToOldAndNewRfcTagMap)
			{
				copyOfFileToOldAndNewRfcTagMap.Add(map.Key, map.Value);
			}

			foreach (var map in copyOfFileToOldAndNewRfcTagMap)
			{
				string rfcTagInQuestion = map.Value.Value;
				if (_uniqueRfcTagsToFilenameMap.Keys.Any(rfcTag => rfcTag.Equals(rfcTagInQuestion, StringComparison.OrdinalIgnoreCase)))
				{
					if(map.Value.Key.Equals(rfcTagInQuestion, StringComparison.OrdinalIgnoreCase))
					{
						string duplicateRfcTag =
							_uniqueRfcTagsToFilenameMap.Keys.First(
								rfcTag => rfcTag.Equals(rfcTagInQuestion, StringComparison.OrdinalIgnoreCase)); //this construct is needed because the duplicate rfctag may differ in case
						string fileContainingRfcTag = _uniqueRfcTagsToFilenameMap[duplicateRfcTag];

						string rfcTagWithDuplicateNumber = UpDuplicateNumberOfWsInLdmlFileAndReturnNewRfcTag(fileContainingRfcTag);
						UpdateFileToOldAndNewRfcTagMap(fileContainingRfcTag, rfcTagWithDuplicateNumber);
						_uniqueRfcTagsToFilenameMap.Add(rfcTagWithDuplicateNumber, fileContainingRfcTag);
						_uniqueRfcTagsToFilenameMap[rfcTagInQuestion] = map.Key;
					}
					else
					{
						string rfcTagWithDuplicateNumber = UpDuplicateNumberOfWsInLdmlFileAndReturnNewRfcTag(map.Key);
						UpdateFileToOldAndNewRfcTagMap(map.Key, rfcTagWithDuplicateNumber);
						_uniqueRfcTagsToFilenameMap[rfcTagWithDuplicateNumber] = map.Key;
					}
				}
				else
				{
					_uniqueRfcTagsToFilenameMap[rfcTagInQuestion] = map.Key;
				}
			}
		}

		private string RemoveDuplicateMarkerFromWsInLdmlFile(string file)
		{
			WritingSystemDefinitionV1 ws = new WritingSystemDefinitionV1();
			var adaptor = new LdmlAdaptorV1();
			adaptor.Read(file, ws);
			ws.DuplicateNumber = 0;
			string pathToBackupTo = file + ".bak";
			if (File.Exists(pathToBackupTo))
			{
				File.Delete(pathToBackupTo);
			}
			File.Move(file, pathToBackupTo);
			var streamFromOldFile = new FileStream(pathToBackupTo, FileMode.Open);
			adaptor.Write(file, ws, streamFromOldFile);
			streamFromOldFile.Close();
			File.Delete(pathToBackupTo);
			return ws.RFC5646;
		}

		private void MigrateIndividualFiles(string directoryToMigrate)
		{
			foreach (var pathToFileToMigrate in Directory.GetFiles(directoryToMigrate))
			{
				Migrator individualFileMigrator = GetMigratorForSingleLdmlFile(pathToFileToMigrate);
				if(individualFileMigrator.NeedsMigration())
				{
					string rfcTagBeforeMigration = GetRfcTagFromFileV0(pathToFileToMigrate);
					individualFileMigrator.Migrate();
					string rfcTagAfterMigration = GetRfcTagFromFileV1(pathToFileToMigrate);

					var oldToNewRfcTagMap = new KeyValuePair<string, string>(rfcTagBeforeMigration, rfcTagAfterMigration);
					_fileToOldAndNewRfcTagMap.Add(pathToFileToMigrate, oldToNewRfcTagMap);
				}
			}
		}

		private void UpdateFileToOldAndNewRfcTagMap(string fileContainingRfcTag, string rfcTagWithDuplicateNumber)
		{
			var oldToNewRfcTagsMap = new KeyValuePair<string, string>(_fileToOldAndNewRfcTagMap[fileContainingRfcTag].Key, rfcTagWithDuplicateNumber);
			_fileToOldAndNewRfcTagMap[fileContainingRfcTag] = oldToNewRfcTagsMap;
		}

		private string UpDuplicateNumberOfWsInLdmlFileAndReturnNewRfcTag(string fileContainingRfcTag)
		{
			WritingSystemDefinitionV1 ws = new WritingSystemDefinitionV1();
			var adaptor = new LdmlAdaptorV1();
			adaptor.Read(fileContainingRfcTag, ws);
			do
			{
				ws.DuplicateNumber++;
			} while (
				_uniqueRfcTagsToFilenameMap.Keys.Any(
					rfcTag => rfcTag.Equals(ws.RFC5646, StringComparison.OrdinalIgnoreCase)));
			string pathToBackupTo = fileContainingRfcTag + ".bak";
			if (File.Exists(pathToBackupTo))
			{
				File.Delete(pathToBackupTo);
			}
			File.Move(fileContainingRfcTag, pathToBackupTo);
			var streamFromOldFile = new FileStream(pathToBackupTo, FileMode.Open);
			adaptor.Write(fileContainingRfcTag, ws, streamFromOldFile);
			streamFromOldFile.Close();
			File.Delete(pathToBackupTo);
			return ws.RFC5646;
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
	}
}
