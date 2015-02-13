
using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Lists;
using SIL.Archiving.IMDI.Schema;
using SIL.Extensions;

namespace SIL.Archiving.IMDI
{
	/// <summary>Collects the data and produces an IMDI corpus to upload</summary>
	public class IMDIPackage : ArchivingPackage
	{
		/// <summary></summary>
		public MetaTranscript BaseImdiFile { get; private set; }

		/// <summary>The file to import into Arbil</summary>
		public string MainExportFile { get; private set; }

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
// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var session in Sessions)
			{
				var sessImdi = new MetaTranscript { Items = new object[] { session }, Type = MetatranscriptValueType.SESSION };
				sessionFiles.Add(sessImdi.WriteImdiFile(_packagePath, Name));
			}

			if (!_corpus)
			{
				MainExportFile = sessionFiles[0];
				return true;
			}

			var corpus = (Corpus) BaseMajorObject;

			// add the session file links
			foreach (var fileName in sessionFiles)
				corpus.CorpusLink.Add(new CorpusLinkType { Value = fileName.Replace("\\", "/"), Name = string.Empty });

			// crate the catalogue
			corpus.CatalogueLink = CreateCorpusCatalogue();

			//  Create the corpus imdi file
			MainExportFile = BaseImdiFile.WriteImdiFile(_packagePath, Name);

			return true;
		}

		private string CreateCorpusCatalogue()
		{
			// Create the package catalogue imdi file
			var catalogue = new Catalogue
			{
				Name = Name + " Catalogue",
				Title = Title,
				Date = DateTime.Today.ToISO8601DateOnlyString(),
			};

			foreach (var language in MetadataIso3Languages)
			{
				var imdiLanguage = LanguageList.Find(language).ToSimpleLanguageType();
				if (imdiLanguage != null)
					catalogue.DocumentLanguages.Language.Add(imdiLanguage);
			}


			foreach (var language in ContentIso3Languages)
			{
				var imdiLanguage = LanguageList.Find(language).ToSubjectLanguageType();
				if (imdiLanguage != null)
					catalogue.SubjectLanguages.Language.Add(imdiLanguage);
			}

			// funding project
			if (FundingProject != null)
				catalogue.Project.Add(new Project(FundingProject));

			// location
			if (Location != null)
				catalogue.Location.Add(new LocationType(Location));

			// content type
			if (!string.IsNullOrEmpty(ContentType))
				catalogue.ContentType.Add(ContentType);

			// applications
			if (!string.IsNullOrEmpty(Applications))
				catalogue.Applications = Applications;

			// author
			if (!string.IsNullOrEmpty(Author))
				catalogue.Author.Add(new CommaSeparatedStringType { Value = Author });

			// publisher
			if (!string.IsNullOrEmpty(Publisher))
				catalogue.Publisher.Add(Publisher);

			// keys
			foreach (var kvp in _keys)
				catalogue.Keys.Key.Add(new KeyType { Name = kvp.Key, Value = kvp.Value });

			// access
			if (!string.IsNullOrEmpty(Access.DateAvailable))
				catalogue.Access.Date = Access.DateAvailable;

			if (!string.IsNullOrEmpty(Access.Owner))
				catalogue.Access.Owner = Access.Owner;

			// write the xml file
			var catImdi = new MetaTranscript { Items = new object[] { catalogue }, Type = MetatranscriptValueType.CATALOGUE };
			return catImdi.WriteImdiFile(_packagePath, Name).Replace("\\", "/");
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
