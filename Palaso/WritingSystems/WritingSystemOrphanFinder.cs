using System.Collections.Generic;
using Palaso.WritingSystems.Migration;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// The WritingSystemOrphanFinder class provides a uniform way for dealing with orphaned writing system ids.
	/// Orphaned writing system ids are writing system ids found in a file that are not contained
	/// in a given writing system repository.
	/// The algorithm used:
	/// 1. Find all writing system id contained in the file
	/// 2. For each of these writing system ids check if it is contained in the repo.
	///    If so goto step 6.
	/// 3. Check with the repo whether the writing system id has been changed.
	///    If so, rename the writing system throughout the file and goto step 6.
	/// 4. Run the tag in question through the RfcTagCleaner and check if the new tag is in the repo.
	///    If so, rename the writing system throughout the file and goto step 6.
	/// 5. Create a new writing system with "cleaned" id and set it in the repo.
	/// 6. Complete
	/// </summary>
	public class WritingSystemOrphanFinder
	{
		public delegate void IdReplacementStrategy(string newId, string oldId);

		public static void FindOrphans(
			IEnumerable<string> idsInFile,
			IdReplacementStrategy replaceIdsInFile,
			LdmlInFolderWritingSystemRepository writingSystemRepository
			)
		{
			foreach (var wsId in idsInFile)
			{
				// Check if it's in the repo
				if (writingSystemRepository.Contains(wsId))
				{
					continue;
				}
				string newId;
				if (writingSystemRepository.WritingSystemIdHasChanged(wsId))
				{
					newId = writingSystemRepository.WritingSystemIdHasChangedTo(wsId);
				}
				else
				{
					// It's an orphan
					// Clean it
					var rfcTagCleaner = new Rfc5646TagCleaner(wsId);
					rfcTagCleaner.Clean();
					newId = rfcTagCleaner.GetCompleteTag();
				}
				var conformantWritingSystem = WritingSystemDefinition.Parse(newId);
				// If it changed, then change
				if (conformantWritingSystem.RFC5646 != wsId)
				{
					conformantWritingSystem = WritingSystemDefinition.CreateCopyWithUniqueId(conformantWritingSystem, idsInFile);
					replaceIdsInFile(wsId, conformantWritingSystem.RFC5646);
				}
				// Check if it's in the repo
				if (writingSystemRepository.Contains(conformantWritingSystem.RFC5646))
				{
					continue;
				}
				// It's not in the repo so set it
				writingSystemRepository.Set(conformantWritingSystem);
			}
			writingSystemRepository.Save();
		}
	}
}