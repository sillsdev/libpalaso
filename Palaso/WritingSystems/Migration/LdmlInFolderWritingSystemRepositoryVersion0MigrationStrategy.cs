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

			var migratedFiles = new List<string>();
			var unmigratedFiles = new List<string>();   //We use this instead of the Directory.GetFiles directly, because we would otherwise be changing the thing we are enumerating over
			unmigratedFiles.AddRange(Directory.GetFiles(directoryToMigrate));

			foreach (var pathToCheckNameFor in unmigratedFiles)
			{
				if(migratedFiles.Contains(pathToCheckNameFor, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (!NameOfFileMatchesRfcTagInFile(pathToCheckNameFor))
				{
					string rfcTagBeforeMigration = _fileToOldAndNewRfcTagMap[pathToCheckNameFor].Key;
					string rfcTagAfterMigration = _fileToOldAndNewRfcTagMap[pathToCheckNameFor].Value;
					string rfcConformPath = Path.Combine(Path.GetDirectoryName(pathToCheckNameFor),
												  GetRfcTagFromFileV1(pathToCheckNameFor) + ".ldml");
					if (rfcTagBeforeMigration.Equals(rfcTagAfterMigration, StringComparison.OrdinalIgnoreCase))
					{
						if (File.Exists(rfcConformPath))
						{
							MarkWritingSystemAsNextFreeDuplicateAndRenameFileAccordingly(rfcConformPath);
							migratedFiles.Add(rfcConformPath);
						}
						File.Move(pathToCheckNameFor, rfcConformPath);
						migratedFiles.Add(pathToCheckNameFor);
					}
					else
					{
						if (File.Exists(rfcConformPath))
						{
							MarkWritingSystemAsNextFreeDuplicateAndRenameFileAccordingly(pathToCheckNameFor);
							migratedFiles.Add(pathToCheckNameFor);
						}
					}
				}
			}

			foreach (var file in Directory.GetFiles(directoryToMigrate))
			{
				string pathTomoveTo = Path.Combine(destinationDirectory, Path.GetFileName(file));
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
