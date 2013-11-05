
using System.IO;
using SIL.Archiving.Generic;

namespace SIL.Archiving.IMDI
{
	/// <summary>Collects the data and produces an IMDI corpus to upload</summary>
	public class IMDIPackage : ArchivingPackage
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

		/// <summary>Creates the corpus directory structure, meta data files, and copies content files</summary>
		/// <returns></returns>
		public bool CreateIMDIPackage(string outputDirectoryName, string corpusDirectoryName)
		{
			// create the corpus directory
			var corpusDirInfo = Directory.CreateDirectory(Path.Combine(outputDirectoryName, IMDIArchivingDlgViewModel.NormalizeDirectoryName(corpusDirectoryName)));

			// create the session directories
			foreach (IMDISession session in Sessions)
				session.CreateIMDISession(corpusDirInfo.FullName);

			// TODO: Determine if we need to create the package catalogue imdi file (may not be needed)

			// TODO: Determine if we need to create the corpus imdi file (may not be needed)

			return true;
		}


	}
}
