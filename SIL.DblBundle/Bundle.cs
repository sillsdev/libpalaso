using System;
using System.IO;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.Xml;

namespace SIL.DblBundle
{
	/// <summary>
	/// File utilities for DBL Bundles
	/// </summary>
	public static class DblBundleFileUtils
	{
		/// <summary>Extension used on DBL bundles</summary>
		public const string kDblBundleExtension = ".zip";
		/// <summary>File that contains versification</summary>
		public const string kVersificationFileName = "versification.vrs";
		/// <summary>File that contains language definition</summary>
		public const string kLdmlFileName = "ldml.xml";
		/// <summary>File extension for language definition files</summary>
		public const string kUnzippedLdmlFileExtension = ".ldml";

		/// <summary>
		/// Extract the contents of a zip file located in the specified path to a temp directory
		/// </summary>
		/// <returns>path of the temporary directory</returns>
		public static string ExtractToTempDirectory(string zipFilePath)
		{
			if (!File.Exists(zipFilePath))
				throw new ArgumentException("Zip file must exist.", "zipFilePath");

			string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempPath);

			ZipUtilities.ExtractToDirectory(zipFilePath, tempPath);

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

		private readonly TM _dblMetadata;
		/// <summary>temporary directory path where the zip bundle is extracted</summary>
		protected readonly string _pathToZippedBundle;
		private string _pathToUnzippedDirectory;

		/// <summary>
		/// Create a DBL Bundle
		/// </summary>
		/// <param name="pathToZippedBundle"></param>
		/// <exception cref="ApplicationException"></exception>
		protected Bundle(string pathToZippedBundle)
		{
			_pathToZippedBundle = pathToZippedBundle;
			try
			{
				_pathToUnzippedDirectory = DblBundleFileUtils.ExtractToTempDirectory(_pathToZippedBundle);
			}
			catch (Exception ex)
			{
				throw new ApplicationException(Localizer.GetString("DblBundle.UnableToExtractBundle",
						"Unable to read contents of Text Release Bundle:") +
					Environment.NewLine + _pathToZippedBundle, ex);
			}

			_dblMetadata = LoadMetadata();
		}

		#region Public properties
		/// <summary>
		/// Typically, this should be set to SIL.WritingSystems.WellKnownSubtags.UnlistedLanguage by the client.
		/// We would have probably just hard-coded to that, but we didn't want to have to reference SIL.WritingSystems
		/// just for that. Anyway, this gives a bit more versatility.
		/// </summary>
		public static string DefaultLanguageIsoCode { get; set; }

		/// <summary>
		/// Path to the original (unzipped) DBL bundle
		/// </summary>
		public string BundlePath => _pathToZippedBundle;

		/// <summary>
		/// Representation of the metadata.xml file included in the bundle
		/// </summary>
		public TM Metadata => _dblMetadata;

		/// <summary>
		/// Unique hex ID for the DBL bundle
		/// </summary>
		public string Id => _dblMetadata.Id;

		/// <summary>
		/// 3-letter ISO 639-2 code for the language of the DBL bundle. If the metadata's language does
		/// not identify an ISO 639-2 code, this returns the default language ISO code, which clients
		/// are encouraged to set to "qaa" or some other value that will allow it to be properly
		/// treated as "unknown".
		/// </summary>
		public string LanguageIso
		{
			get
			{
				var isoCode = _dblMetadata.Language.Iso;
				return String.IsNullOrEmpty(isoCode) ? DefaultLanguageIsoCode : isoCode;
			}
		}

		/// <summary>
		/// The name of the publication
		/// </summary>
		public abstract string Name { get; }
		#endregion

		/// <summary>
		/// (Temporary) path of the unzipped contents of the DBL bundle
		/// </summary>
		protected string PathToUnzippedDirectory => _pathToUnzippedDirectory;

		#region Private methods
		private TM LoadMetadata()
		{
			const string filename = "metadata.xml";
			string metadataPath = Path.Combine(_pathToUnzippedDirectory, filename);

			if (!File.Exists(metadataPath))
			{
				bool sourceBundle = filename.Contains("source") || Directory.Exists(Path.Combine(_pathToUnzippedDirectory, "gather"));
				if (sourceBundle)
				{
					throw new ApplicationException(
						string.Format(Localizer.GetString("DblBundle.SourceReleaseBundle",
							"This bundle appears to be a source bundle. Only Text Release Bundles are currently supported."), filename) +
						Environment.NewLine + _pathToZippedBundle);

				}
				throw new ApplicationException(
					string.Format(Localizer.GetString("DblBundle.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + _pathToZippedBundle);
			}

			var dblMetadata = DblMetadataBase<TL>.Load<TM>(metadataPath, out var exception);
			if (exception != null)
			{
				DblMetadata metadata = XmlSerializationHelper.DeserializeFromFile<DblMetadata>(metadataPath,
					out var metadataBaseDeserializationError);
				if (metadataBaseDeserializationError != null)
				{
					throw new ApplicationException(Localizer.GetString("DblBundle.MetadataInvalid",
							"Unable to read metadata. File is not a valid Text Release Bundle:") +
						Environment.NewLine + _pathToZippedBundle, metadataBaseDeserializationError);
				}

				throw new ApplicationException(
					String.Format(Localizer.GetString("DblBundle.MetadataInvalidVersion",
						"Unable to read metadata. Type: {0}. Version: {1}. File is not a valid Text Release Bundle:"),
						metadata.Type, metadata.TypeVersion) +
					Environment.NewLine + _pathToZippedBundle);
			}

			if (!dblMetadata.IsTextReleaseBundle)
			{
				throw new ApplicationException(
					String.Format(Localizer.GetString("DblBundle.NotTextReleaseBundle",
						"This metadata in this bundle indicates that it is of type \"{0}\". Only Text Release Bundles are currently supported."),
						dblMetadata.Type));
			}

			return dblMetadata;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Dispose method for this class.
		/// </summary>
		public void Dispose()
		{
			if (_pathToUnzippedDirectory != null && Directory.Exists(_pathToUnzippedDirectory))
			{
				try
				{
					Directory.Delete(_pathToUnzippedDirectory, true);
				}
				catch (Exception)
				{
					// Oh well, we tried
				}
				_pathToUnzippedDirectory = null;
			}
		}
		#endregion
	}
}
