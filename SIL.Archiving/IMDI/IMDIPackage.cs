
using System;
using System.IO;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Schema;

namespace SIL.Archiving.IMDI
{
	/// <summary>Collects the data and produces an IMDI corpus to upload</summary>
	public class IMDIPackage : ArchivingPackage
	{
		public MetaTranscript BaseImdiFile { get; private set; }
		private readonly bool _corpus;
		private DirectoryInfo _corpusDirInfo;
		private string _packagePath;

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="corpus">Indicates whether this is for an entire project corpus or a
		/// single session</param>
		/// ------------------------------------------------------------------------------------
		public IMDIPackage(bool corpus, string packagePath)
		{
			_corpus = corpus;
			_packagePath = packagePath;
			BaseImdiFile = new MetaTranscript(corpus ? MetatranscriptValueType.CORPUS :
				MetatranscriptValueType.SESSION);
		}

		#region Properties
		private IMDIMajorObject BaseMajorObject
		{
			get { return (IMDIMajorObject)BaseImdiFile.Items[0]; }
		}

	#endregion

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
		public bool CreateIMDIPackage()
		{
			// create the base directory
			_corpusDirInfo = Directory.CreateDirectory(_packagePath);

			if (_corpus)
			{
				// create the session directories
				//foreach (var session in Sessions)
				//    session.CreateIMDISession(_corpusDirInfo.FullName);
			}

			// TODO: Determine if we need to create the package catalogue imdi file (may not be needed)

			if (_corpus)
			{
				// TODO: Create the corpus imdi file
			}

			return true;
		}

		public void AddDescription(LanguageString description)
		{
			BaseMajorObject.AddDescription(description);
		}
	}
}
