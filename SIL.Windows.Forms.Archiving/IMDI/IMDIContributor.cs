using System.Collections.Generic;
using System.Linq;
using SIL.Windows.Forms.Archiving.Generic;

namespace SIL.Windows.Forms.Archiving.IMDI
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
		// Test_Corpus\Contributors\Files*.* (contributor/actor files)

		/// <summary>Add a file for this contributor</summary>
		/// <param name="file"></param>
		public void AddFile(IMDIFile file)
		{
			Files.Add(file);
		}

		public IEnumerable<IMDIFile> MediaFiles
		{
			get { return Files.Cast<IMDIFile>().Where(file => file.IsMediaFile).ToList(); }
		}

		public IEnumerable<IMDIFile> WrittenResources
		{
			get { return Files.Cast<IMDIFile>().Where(file => file.IsWrittenResource).ToList(); }
		}
	}
}
