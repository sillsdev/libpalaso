using System.IO;
using SIL.Archiving.Generic;

namespace SIL.Archiving.IMDI
{
	public class IMDIContributor : ArchivingActor
	{
		// **** Corpus Layout ****
		//
		// Test_Corpus (directory)
		// Test_Corpus.imdi (corpus meta data file)
		// Test_Corpus\Test_Corpus_Catalog.imdi (catalogue of information)
		// Test_Corpus\Test_Session (directory)
		// Test_Corpus\Test_Session.imdi (session meta data file)
		// Test_Corpus\Test_Session\Contributors (directory - contains files pertaining to contributers/actors)
		// Test_Corpus\Test_Session\Files*.* (session files)
		// Test_Corpus\Test_Session\Contributors\Files*.* (contributor/actor files)

		/// <summary>Add a file for this contributor</summary>
		/// <param name="file"></param>
		public void AddFile(IMDIFile file)
		{
			Files.Add(file);
		}

	}
}
