﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using L10NSharp;
using SIL.DblBundle.Usx;
using SIL.Reporting;

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
		/// Try to get the book as a UsxDocument
		/// </summary>
		/// <returns>True if the book exists, false otherwise</returns>
		bool TryGetBook(string bookId, out UsxDocument book);
	}

	/// <summary>
	/// A DBL Text Release Bundle
	/// </summary>
	public class TextBundle<TM, TL> : Bundle<TM, TL>, ITextBundle
		where TM : DblTextMetadata<TL>
		where TL : DblMetadataLanguage, new()
	{
		protected readonly IDictionary<string, UsxDocument> m_books = new Dictionary<string, UsxDocument>();
		private readonly Stylesheet m_stylesheet;

		/// <summary>
		/// Create a new DBL Text Release Bundle from the given zip file
		/// </summary>
		public TextBundle(string pathToZippedBundle) : base(pathToZippedBundle)
		{
			ExtractBooks();
			m_stylesheet = LoadStylesheet();
		}

		#region Public properties
		/// <summary>
		/// Defines the styles for this bundle
		/// </summary>
		public Stylesheet Stylesheet { get { return m_stylesheet; } }

		/// <summary>
		/// The name of the publication
		/// </summary>
		public override string Name { get { return Metadata.Identification.Name; } }
		#endregion

		#region Private methods
		private Stylesheet LoadStylesheet()
		{
			const string filename = "styles.xml";
			string stylesheetPath = Path.Combine(PathToUnzippedDirectory, filename);

			if (!File.Exists(stylesheetPath))
			{
				throw new ApplicationException(
					string.Format(LocalizationManager.GetString("DblBundle.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + m_pathToZippedBundle);
			}

			Exception exception;
			var stylesheet = Stylesheet.Load(stylesheetPath, out exception);
			if (exception != null)
			{
				throw new ApplicationException(
					LocalizationManager.GetString("DblBundle.StylesheetInvalid",
						"Unable to read stylesheet. File is not a valid Text Release Bundle:") +
					Environment.NewLine + m_pathToZippedBundle, exception);
			}

			return stylesheet;
		}

		private void ExtractBooks()
		{
			DblMetadataCanon defaultCanon = Metadata.Canons.FirstOrDefault(c => c.Default);
			if (defaultCanon != null)
				ExtractBooksInCanon(GetPathToCanon(defaultCanon.CanonId));
			foreach (DblMetadataCanon canon in Metadata.Canons.Where(c => !c.Default).OrderBy(c => c.CanonId))
				ExtractBooksInCanon(GetPathToCanon(canon.CanonId));
		}

		private void ExtractBooksInCanon(string pathToCanon)
		{
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
			return Path.Combine(PathToUnzippedDirectory, "USX_" + canonId);
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Copy the versification file from the text bundle to destinationPath
		/// </summary>
		public void CopyVersificationFile(string destinationPath)
		{
			string versificationPath = Path.Combine(PathToUnzippedDirectory, DblBundleFileUtils.kVersificationFileName);

			if (!File.Exists(versificationPath))
				throw new ApplicationException(
					string.Format("Attempted to copy {0} from the bundle but {0} does not exist in this bundle.", DblBundleFileUtils.kVersificationFileName));

			File.Copy(versificationPath, destinationPath, true);
		}

		/// <summary>
		/// Copies any font files (*.ttf) in the bundle to the given destination directory
		/// </summary>
		public void CopyFontFiles(string destinationDir)
		{
			foreach (var ttfFile in Directory.GetFiles(PathToUnzippedDirectory, "*.ttf"))
			{
				string newPath = Path.Combine(destinationDir, Path.GetFileName(ttfFile));
				try
				{
					if (!File.Exists(newPath))
						File.Copy(ttfFile, newPath, true);
				}
				catch (IOException e)
				{
					ErrorReport.ReportNonFatalExceptionWithMessage(e,
						LocalizationManager.GetString("DblBundle.FontFileCopyFailed", "An attempt to copy font file {0} from the bundle to {1} failed."), Path.GetFileName(ttfFile), destinationDir);
				}
			}
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
