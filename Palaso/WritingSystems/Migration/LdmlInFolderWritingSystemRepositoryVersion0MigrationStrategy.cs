using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryVersion0MigrationStrategy : MigrationStrategyBase
	{
		public delegate void OnWritingSystemIDChanged(IEnumerable<KeyValuePair<string, string>> mapTagOldToNew);
		private readonly OnWritingSystemIDChanged _onWritingSystemIDChanged;

		public LdmlInFolderWritingSystemRepositoryVersion0MigrationStrategy(OnWritingSystemIDChanged onWritingSystemIDChanged) :
			base(0, 1)
		{
			_onWritingSystemIDChanged = onWritingSystemIDChanged;
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
			_onWritingSystemIDChanged(oldToNewRfcTagMap);
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
			WritingSystemDefinition ws = new WritingSystemDefinition();
			var adaptor = new LdmlAdaptor();
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

		public int DuplicateNumber         {
		  get
		  {
			  Regex duplicateTagRegex = new Regex("^dupl[0-9]*$");
			  string duplicateTag = _rfcTag.GetPartMatchingRegExInPrivateUse(duplicateTagRegex);
			  if (String.IsNullOrEmpty(duplicateTag))
			  {
				  return 0;
			  }
			  Regex numberRegex = new Regex("[0-9]*$");
			  //int valueFound = String.IsNullOrEmpty(numberRegex.Match(duplicateTag).Value) ? 0 : Convert.ToInt32(numberRegex.Match(duplicateTag).Value);
			  return Convert.ToInt32(numberRegex.Match(duplicateTag).Value);
		  }

		  set
		  {
			  if (value < 0) { throw new ArgumentOutOfRangeException("We can't have a negaive number of duplicates."); }
			  Regex duplicateTagRegex = new Regex("^dupl[0-9]*$");
			  string duplicateTag = _rfcTag.GetPartMatchingRegExInPrivateUse(duplicateTagRegex);
			  if (!String.IsNullOrEmpty(duplicateTag))
			  {
				  _rfcTag.RemoveFromPrivateUse(duplicateTag);
			  }
			  if (value > 0)
			  {
				  _rfcTag.AddToPrivateUse("dupl" + value);
			  }
		  }
	  }

		private void MigrateIndividualFiles(string directoryToMigrate)
		{
			foreach (var pathToFileToMigrate in Directory.GetFiles(directoryToMigrate))
			{
				FileMigrator individualFileMigrator = CreateMigratorForSingleLdmlFile(pathToFileToMigrate);
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
			WritingSystemDefinition ws = new WritingSystemDefinition();
			var adaptor = new LdmlAdaptor();
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

		private FileMigrator CreateMigratorForSingleLdmlFile(string pathToFileToMigrate)
		{
			var individualFileMigrator = new FileMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, pathToFileToMigrate);
			individualFileMigrator.AddVersionStrategy(new WritingSystemLdmlVersionGetter());
			individualFileMigrator.AddMigrationStrategy(new Version0MigrationStrategy());
			return individualFileMigrator;
		}

		private string GetRfcTagFromFileV1(string pathToFileToMigrate)
		{
			var ws = new WritingSystemDefinition();
			new LdmlAdaptor().Read(pathToFileToMigrate, ws);
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
