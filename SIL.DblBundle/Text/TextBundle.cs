using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.WritingSystems;

namespace SIL.DblBundle.Text
{
	/// <summary>
	/// Interface representing a DBL Text Release Bundle
	/// </summary>
	public interface ITextBundle : IBundle
	{
		/// <summary>
		/// Defines the styles for this bundle
		/// </summary>
		Stylesheet Stylesheet { get; }

		/// <summary>
		/// Defines the writing system definition for this bundle
		/// </summary>
		WritingSystemDefinition WritingSystemDefinition { get; }

		/// <summary>
		/// Try to get the book as a UsxDocument
		/// </summary>
		/// <returns>True if the book exists, false otherwise</returns>
		bool TryGetBook(string bookId, out UsxDocument book);

		/// <summary>
		/// Books which are included and have valid data and metadata
		/// </summary>
		IEnumerable<UsxDocument> UsxBooksToInclude { get; }
	}

	/// <summary>
	/// A DBL Text Release Bundle
	/// </summary>
	public class TextBundle<TM, TL> : Bundle<TM, TL>, ITextBundle
		where TM : DblTextMetadata<TL>
		where TL : DblMetadataLanguage, new()
	{
		/// <summary>mapping from the book ID to a USX document for the book</summary>
		protected readonly IDictionary<string, UsxDocument> m_books = new Dictionary<string, UsxDocument>();
		private readonly Stylesheet m_stylesheet;
		private readonly WritingSystemDefinition m_ws;

		/// <summary>
		/// Create a new DBL Text Release Bundle from the given zip file
		/// </summary>
		public TextBundle(string pathToZippedBundle) : base(pathToZippedBundle)
		{
			ExtractBooks();
			m_stylesheet = LoadStylesheet();
			m_ws = LoadWritingSystemDefinition();
		}

		#region Public properties
		/// <summary>
		/// Defines the styles for this bundle
		/// </summary>
		public Stylesheet Stylesheet { get { return m_stylesheet; } }

		/// <summary>
		/// Defines the writing system definition for this bundle
		/// </summary>
		public WritingSystemDefinition WritingSystemDefinition { get { return m_ws; } }

		/// <summary>
		/// The name of the publication
		/// </summary>
		public override string Name { get { return Metadata.Identification.Name; } }

		/// <summary>
		/// Books which are included and have valid data and metadata
		/// </summary>
		public IEnumerable<UsxDocument> UsxBooksToInclude
		{
			get
			{
				foreach (var book in Metadata.AvailableBibleBooks.Where(b => b.IncludeInScript))
				{
					UsxDocument usxBook;
					if (TryGetBook(book.Code, out usxBook))
						yield return usxBook;
				}
			}
		}
		#endregion

		#region Private methods/properties
		private string _ldmlFilePath;
		private string LdmlFilePath
		{
			get
			{
				if (_ldmlFilePath != null)
					return _ldmlFilePath;

				_ldmlFilePath = Directory.GetFiles(PathToUnzippedBundleInnards, "*.ldml").FirstOrDefault();

				if (_ldmlFilePath == null)
					_ldmlFilePath = Path.Combine(PathToUnzippedBundleInnards, DblBundleFileUtils.kLegacyLdmlFileName);
				return _ldmlFilePath;
			}
		}

		private Stylesheet LoadStylesheet()
		{
			const string filename = "styles.xml";
			string stylesheetPath = Path.Combine(PathToUnzippedBundleInnards, filename);

			if (!File.Exists(stylesheetPath))
			{
				throw new ApplicationException(
					string.Format(Localizer.GetString("DblBundle.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + _pathToZippedBundle);
			}

			Exception exception;
			var stylesheet = Stylesheet.Load(stylesheetPath, out exception);
			if (exception != null)
			{
				throw new ApplicationException(Localizer.GetString("DblBundle.StylesheetInvalid",
						"Unable to read stylesheet. File is not a valid Text Release Bundle:") +
					Environment.NewLine + _pathToZippedBundle, exception);
			}

			return stylesheet;
		}

		private WritingSystemDefinition LoadWritingSystemDefinition()
		{
			if (!ContainsLdmlFile())
				return null;

			var ldmlAdaptor = new LdmlDataMapper(new WritingSystemFactory());

			var wsFromLdml = new WritingSystemDefinition();
			ldmlAdaptor.Read(LdmlFilePath, wsFromLdml);

			return wsFromLdml;
		}

		private void ExtractBooks()
		{
			DblMetadataCanon defaultCanon = Metadata.Canons.FirstOrDefault(c => c.Default);
			if (defaultCanon != null)
			{
				ExtractBooksInCanon(defaultCanon.CanonId);
			}
			foreach (DblMetadataCanon canon in Metadata.Canons.Where(c => !c.Default).OrderBy(c => c.CanonId))
			{
				ExtractBooksInCanon(canon.CanonId);
			}
			if (!m_books.Any())
			{
				throw new ApplicationException(Localizer.GetString("DblBundle.UnableToLoadAnyBooks",
						"Unable to load any books. File may not be a valid Text Release Bundle or it may contain a problem:") +
					Environment.NewLine + _pathToZippedBundle);
			}
		}

		private void ExtractBooksInCanon(string canonId)
		{
			string pathToCanon = GetPathToCanon(canonId);
			if (!Directory.Exists(pathToCanon))
				return;

			foreach (string filePath in Directory.GetFiles(pathToCanon, "*.usx"))
			{
				var fi = new FileInfo(filePath);
				string bookId = Path.GetFileNameWithoutExtension(fi.Name);
				if (bookId.Length == 3 && !m_books.ContainsKey(bookId))
					m_books.Add(bookId, new UsxDocument(filePath));
			}
		}

		private string GetPathToCanon(string canonId)
		{
			return Path.Combine(PathToUnzippedBundleInnards, "USX_" + canonId);
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Copy the versification file from the text bundle to destinationPath
		/// </summary>
		public void CopyVersificationFile(string destinationPath)
		{
			string versificationPath = Path.Combine(PathToUnzippedBundleInnards, DblBundleFileUtils.kVersificationFileName);

			if (!File.Exists(versificationPath))
				throw new ApplicationException(
					string.Format("Attempted to copy {0} from the bundle but {0} does not exist in this bundle.", DblBundleFileUtils.kVersificationFileName));

			RobustFile.Copy(versificationPath, destinationPath, true);
		}

		/// <summary>
		/// Copies any font files (*.ttf) in the bundle to the given destination directory
		/// </summary>
		public bool CopyFontFiles(string destinationDir, out ISet<string> filesWhichFailedToCopy)
		{
			filesWhichFailedToCopy = new HashSet<string>();
			foreach (var ttfFile in Directory.GetFiles(PathToUnzippedBundleInnards, "*.ttf"))
			{
				string newPath = Path.Combine(destinationDir, Path.GetFileName(ttfFile));
				try
				{
					if (!File.Exists(newPath))
						File.Copy(ttfFile, newPath, true);
				}
				catch (Exception)
				{
					filesWhichFailedToCopy.Add(Path.GetFileName(ttfFile));
				}
			}

			return !filesWhichFailedToCopy.Any();
		}

		/// <summary>
		/// <returns>true if an LDML file exists in the bundle, false otherwise</returns>
		/// </summary>
		public bool ContainsLdmlFile()
		{
			return File.Exists(LdmlFilePath);
		}

		/// <summary>
		/// Copies ldml.xml from the text bundle to the destinationPath (if it exists)
		/// </summary>
		public bool CopyLdmlFile(string destinationDir)
		{
			if (ContainsLdmlFile())
			{
				string newPath = Path.Combine(destinationDir, LanguageIso + DblBundleFileUtils.kUnzippedLdmlFileExtension);
				File.Copy(LdmlFilePath, newPath, true);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Try to get the book as a UsxDocument
		/// </summary>
		/// <returns>True if the book exists, false otherwise</returns>
		public bool TryGetBook(string bookId, out UsxDocument book)
		{
			return m_books.TryGetValue(bookId, out book);
		}
		#endregion
	}
}
