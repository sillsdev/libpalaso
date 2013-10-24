
using SIL.Archiving.Generic;

namespace SIL.Archiving.IMDI
{
	/// <summary>
	/// Collects the data needed to produce an IMDI corpus to upload
	/// </summary>
	public class IMDIData : ArchivingPackage
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



	}
}
