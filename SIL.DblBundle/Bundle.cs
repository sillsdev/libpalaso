using System;
using System.IO;
using Ionic.Zip;
using L10NSharp;
using SIL.DblBundle.Text;
using SIL.Reporting;
using SIL.Xml;

namespace SIL.DblBundle
{
	/// <summary>
	/// File utilities for DBL Bundles
	/// </summary>
	public static class DblBundleFileUtils
	{
		public const string kDblBundleExtension = ".zip";
		public const string kVersificationFileName = "versification.vrs";

		/// <summary>
		/// Note: There is some reason to believe that the DotNetZip (Ionic) library is not portable to mono/Linux.
		/// See BL-496, BL-498, BL-504 and https://trello.com/c/PYYhpsUm/26-fix-dotnetzip-reduced.
		/// The problem (which no one seems to remember for sure) may be that subdirectories are not unzipped correctly.
		/// If problems occur, consider switching out DotNetZip for SharpZipLib (and do so for all of libpalaso).
		/// </summary>
		public static string ExtractToTempDirectory(string zipFilePath)
		{
			string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempPath);

			using (ZipFile zip1 = ZipFile.Read(zipFilePath))
				foreach (ZipEntry e in zip1)
					e.Extract(tempPath, ExtractExistingFileAction.OverwriteSilently);

			return tempPath;
		}
	}

	/// <summary>
	/// Interface representing a DBL bundle
	/// </summary>
	public interface IBundle
	{
		/// <summary>
		/// Unique hex ID for the DBL bundle
		/// </summary>
		string Id { get; }
		/// <summary>
		/// 3-letter ISO 639-2 code for the language of the DBL bundle
		/// </summary>
		string LanguageIso { get; }
		/// <summary>
		/// The name of the publication
		/// </summary>
		string Name { get; }
	}

	/// <summary>
	/// A DBL Bundle
	/// </summary>
	public abstract class Bundle<TM, TL> : IBundle, IDisposable
		where TM : DblMetadataBase<TL>
		where TL : DblMetadataLanguage, new()
	{
		private readonly TM m_dblMetadata;
		protected readonly string m_pathToZippedBundle;
		private string m_pathToUnzippedDirectory;

		/// <summary>
		/// Create a DBL Bundle
		/// </summary>
		/// <param name="pathToZippedBundle"></param>
		/// <exception cref="ApplicationException"></exception>
		protected Bundle(string pathToZippedBundle)
		{
			m_pathToZippedBundle = pathToZippedBundle;
			try
			{
				m_pathToUnzippedDirectory = DblBundleFileUtils.ExtractToTempDirectory(m_pathToZippedBundle);
			}
			catch (Exception ex)
			{
				throw new ApplicationException(LocalizationManager.GetString("DblBundle.UnableToExtractBundle",
						"Unable to read contents of Text Release Bundle:") +
					Environment.NewLine + m_pathToZippedBundle, ex);
			}

			m_dblMetadata = LoadMetadata();
		}

		#region Public properties
		/// <summary>
		/// Path to the original (unzipped) DBL bundle
		/// </summary>
		public string BundlePath { get { return m_pathToZippedBundle; } }

		/// <summary>
		/// Representation of the metadata.xml file included in the bundle
		/// </summary>
		public TM Metadata { get { return m_dblMetadata; } }

		/// <summary>
		/// Unique hex ID for the DBL bundle
		/// </summary>
		public string Id { get { return m_dblMetadata.Id; } }

		/// <summary>
		/// 3-letter ISO 639-2 code for the language of the DBL bundle
		/// </summary>
		public string LanguageIso { get { return m_dblMetadata.Language.Iso; } }

		/// <summary>
		/// The name of the publication
		/// </summary>
		public abstract string Name { get; }
		#endregion

		/// <summary>
		/// (Temporary) path of the unzipped contents of the DBL bundle
		/// </summary>
		protected string PathToUnzippedDirectory { get { return m_pathToUnzippedDirectory; } }

		#region Private methods
		private TM LoadMetadata()
		{
			const string filename = "metadata.xml";
			string metadataPath = Path.Combine(m_pathToUnzippedDirectory, filename);

			if (!File.Exists(metadataPath))
			{
				bool sourceBundle = filename.Contains("source") || Directory.Exists(Path.Combine(m_pathToUnzippedDirectory, "gather"));
				if (sourceBundle)
				{
					throw new ApplicationException(
						string.Format(LocalizationManager.GetString("DblBundle.SourceReleaseBundle",
							"This bundle appears to be a source bundle. Only Text Release Bundles are currently supported."), filename) +
						Environment.NewLine + m_pathToZippedBundle);

				}
				throw new ApplicationException(
					string.Format(LocalizationManager.GetString("DblBundle.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + m_pathToZippedBundle);
			}

			Exception exception;
			var dblMetadata = DblMetadataBase<TL>.Load<TM>(metadataPath, out exception);
			if (exception != null)
			{
				Exception metadataBaseDeserializationError;
				DblMetadata metadata = XmlSerializationHelper.DeserializeFromFile<DblMetadata>(metadataPath,
					out metadataBaseDeserializationError);
				if (metadataBaseDeserializationError != null)
				{
					throw new ApplicationException(
						LocalizationManager.GetString("DblBundle.MetadataInvalid",
							"Unable to read metadata. File is not a valid Text Release Bundle:") +
						Environment.NewLine + m_pathToZippedBundle, metadataBaseDeserializationError);
				}

				throw new ApplicationException(
					String.Format(LocalizationManager.GetString("DblBundle.MetadataInvalidVersion",
						"Unable to read metadata. Type: {0}. Version: {1}. File is not a valid Text Release Bundle:"),
						metadata.Type, metadata.TypeVersion) +
					Environment.NewLine + m_pathToZippedBundle);
			}

			if (!dblMetadata.IsTextReleaseBundle)
			{
				throw new ApplicationException(
					String.Format(LocalizationManager.GetString("DblBundle.NotTextReleaseBundle",
						"This metadata in this bundle indicates that it is of type \"{0}\". Only Text Release Bundles are currently supported."),
						dblMetadata.Type));
			}

			return dblMetadata;
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			if (m_pathToUnzippedDirectory != null && Directory.Exists(m_pathToUnzippedDirectory))
			{
				try
				{
					Directory.Delete(m_pathToUnzippedDirectory, true);
				}
				catch (Exception e)
				{
					ErrorReport.ReportNonFatalExceptionWithMessage(e,
						string.Format("Failed to clean up temporary folder where bundle was unzipped: {0}.", m_pathToUnzippedDirectory));
				}
				m_pathToUnzippedDirectory = null;
			}
		}
		#endregion
	}
}
