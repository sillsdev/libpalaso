
using System;
using System.Collections.Generic;
using System.Linq;
using Palaso.Extensions;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Lists;
using SIL.Archiving.IMDI.Schema;

namespace SIL.Archiving.IMDI
{
	/// <summary>Collects the data and produces an IMDI corpus to upload</summary>
	public class IMDIPackage : ArchivingPackage
	{
		/// <summary></summary>
		public MetaTranscript BaseImdiFile { get; private set; }

		private readonly bool _corpus;
		private readonly string _packagePath;

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="corpus">Indicates whether this is for an entire project corpus or a
		/// single session</param>
		/// <param name="packagePath"></param>
		/// ------------------------------------------------------------------------------------
		public IMDIPackage(bool corpus, string packagePath)
		{
			_corpus = corpus;
			_packagePath = packagePath;
			//BaseImdiFile = new MetaTranscript(corpus ? MetatranscriptValueType.CORPUS :
			//    MetatranscriptValueType.SESSION);
			BaseImdiFile = new MetaTranscript(MetatranscriptValueType.CORPUS);

			Sessions = new List<IArchivingSession>();
		}

#region Properties
		private IIMDIMajorObject BaseMajorObject
		{
			get { return (IIMDIMajorObject)BaseImdiFile.Items[0]; }
		}

		/// <summary>The path where the corpus imdi file and corpus directory will be created</summary>
		public string PackagePath
		{
			get { return _packagePath;  }
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
		// Test_Corpus\Contributors\Files*.* (contributor/actor files)

		/// <summary>Creates the corpus directory structure, meta data files, and copies content files</summary>
		/// <returns></returns>
		public bool CreateIMDIPackage()
		{
			// list of session files for the corpus
			List<string> sessionFiles = new List<string>();

			// create the session directories
			foreach (var session in Sessions)
			{
				var sessImdi = new MetaTranscript { Items = new object[] { session }, Type = MetatranscriptValueType.SESSION };
				sessionFiles.Add(sessImdi.WriteImdiFile(_packagePath, Name));
			}

			if (!_corpus) return true;

			var corpus = (Corpus) BaseMajorObject;

			// add the session file links
			foreach (var fileName in sessionFiles)
				corpus.CorpusLink.Add(new CorpusLinkType { Value = fileName.Replace("\\", "/"), Name = string.Empty });

			// Create the package catalogue imdi file
			var catalogue = new Catalogue
			{
				Name = Name + " Catalogue",
				Title = Title,
				Date = DateTime.Today.ToISO8601DateOnlyString(),
			};

			foreach (var iso3Id in MetadataIso3LanguageIds)
				catalogue.DocumentLanguages.Language.Add(LanguageList.Find(new ArchivingLanguage(iso3Id)).ToSimpleLanguageType());

			foreach (var iso3Id in ContentIso3LanguageIds)
				catalogue.SubjectLanguages.Language.Add(LanguageList.Find(new ArchivingLanguage(iso3Id)).ToSubjectLanguageType());

			var catImdi = new MetaTranscript { Items = new object[] { catalogue }, Type = MetatranscriptValueType.CATALOGUE };
			corpus.CatalogueLink = catImdi.WriteImdiFile(_packagePath, Name).Replace("\\","/");

			//  Create the corpus imdi file
			BaseImdiFile.WriteImdiFile(_packagePath, Name);

			return true;
		}

		/// <summary>Add a description of the package/corpus</summary>
		/// <param name="description"></param>
		public new void AddDescription(LanguageString description)
		{
			// prevent duplicate description
			foreach (var itm in BaseMajorObject.Description)
			{
				if (itm.LanguageId == description.Iso3LanguageId)
					throw new InvalidOperationException(string.Format("A description for language {0} has already been set", itm.LanguageId));
			}

			BaseMajorObject.Description.Add(description);
		}

		/// <summary>Add a description of the package/corpus</summary>
		/// <param name="sessionId"></param>
		/// <param name="description"></param>
		public void AddDescription(string sessionId, LanguageString description)
		{
			// prevent duplicate description
			if (_corpus)
			{
				foreach (var sess in Sessions.Where(sess => sess.Name == sessionId))
				{
					sess.AddDescription(description);
				}
			}
			else
			{
				if (BaseMajorObject is Session)
				{
					if (Name == sessionId)
						AddDescription(description);
				}

			}
		}

		/// <summary></summary>
		public bool SetMissingInformation()
		{
			if (string.IsNullOrEmpty(BaseMajorObject.Name))
				BaseMajorObject.Name = Name;

			if (string.IsNullOrEmpty(BaseMajorObject.Title))
				BaseMajorObject.Title = Title;

			return true;
		}
	}
}
