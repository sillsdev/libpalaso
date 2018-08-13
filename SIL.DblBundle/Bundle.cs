using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using L10NSharp;
using SIL.DblBundle.Properties;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.Reporting;
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
		public const string kLegacyLdmlFileName = "ldml.xml";
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
		private readonly TM m_dblMetadata;
		/// <summary>temporary directory path where the zip bundle is extracted</summary>
		protected readonly string m_pathToZippedBundle;
		private string m_pathToUnzippedDirectory;
		private Exception m_loadException;

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
				PathToUnzippedBundleInnards = m_pathToUnzippedDirectory;
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
		/// Typically, this should be set to SIL.WritingSystems.WellKnownSubtags.UnlistedLanguage by the client.
		/// We would have probably just hard-coded to that, but we didn't want to have to reference SIL.WritingSystems
		/// just for that. Anyway, this gives a bit more versatility.
		/// </summary>
		public static string DefaultLanguageIsoCode { get; set; }

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
		/// 3-letter ISO 639-2 code for the language of the DBL bundle. If the metadata's language does
		/// not identify an ISO 639-2 code, this returns the default language ISO code, which clients
		/// are encouraged to set to "qaa" or some other value that will allow it to be properly
		/// treated as "unknown".
		/// </summary>
		public string LanguageIso
		{
			get
			{
				var isoCode = m_dblMetadata.Language.Iso;
				return String.IsNullOrEmpty(isoCode) ? DefaultLanguageIsoCode : isoCode;
			}
		}

		/// <summary>
		/// The name of the publication
		/// </summary>
		public abstract string Name { get; }
		#endregion

		/// <summary>
		/// (Temporary) path of the unzipped contents (perhaps excluding metadata.xml) of the DBL bundle
		/// </summary>
		protected string PathToUnzippedBundleInnards { get; private set; }


		#region Private methods
		private TM LoadMetadata()
		{
			const string filename = "metadata.xml";
			string metadataPath = Path.Combine(m_pathToUnzippedDirectory, filename);

			if (!File.Exists(metadataPath))
			{
				// There aren't any rock solid ways to know if we are dealing with a source bundle, but here are some attempts.
				// It only slightly modifies the message to the user, so no big deal if we miss it one way or the other.
				bool sourceBundle = metadataPath.Contains("source") || Directory.Exists(Path.Combine(m_pathToUnzippedDirectory, "gather"));
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

			TM metadataData;
			try
			{
				metadataPath = ConvertMetadataIfNecessary(metadataPath);
				metadataData = LoadMetadataInternal(metadataPath);
			}
			catch (Exception e)
			{
				if (m_loadException != null)
					throw m_loadException;
				throw e;
			}
			File.Delete(metadataPath);
			return metadataData;
		}

		private TM LoadMetadataInternal(string metadataPath)
		{
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

		private string ConvertMetadataIfNecessary(string metadataPath)
		{
			decimal? version = GetVersionFromMetadata(metadataPath);
			if (version == null)
			{
				// If we are unable to determine the version number, we store that information.
				// We'll still attempt to convert and read the metadata.
				// If we hit an exception during that attempt, we will display this message to the user rather than the specific exception.
				m_loadException =
					new ApplicationException(
						"Unable to determine the DBL metadata version. You may need a newer version of the application to read this bundle.");
			}
			else if (version < 2)
			{
				return metadataPath;
			}
			else if (version > 2.1M)
			{
				// If the version number is higher than we know how to handle, we store that information.
				// We'll still attempt to convert and read the metadata.
				// If we hit an exception during that attempt, we will display this message to the user rather than the specific exception.
				m_loadException =
					new ApplicationException(
						$"The metadata version for this DBL bundle is {version} which is greater than the highest known version (2.1). " +
						"You may need a new version of the application to read this bundle.");
			}

			var likelyInnardsDirectory = Path.Combine(m_pathToUnzippedDirectory, "release");
			if (Directory.Exists(likelyInnardsDirectory))
				PathToUnzippedBundleInnards = likelyInnardsDirectory;

			return ConvertMetadata(metadataPath, version);
		}

		private string ConvertMetadata(string metadataPath, decimal? version)
		{
			using (var convertedMetadata = TempFile.WithExtension("xml"))
			{
				var myXslTrans = new XslCompiledTransform();
				if (version == 2.0M)
					myXslTrans.Load(new XmlTextReader(new StringReader(Resources.text_2_0_to_1_5_xsl)));
				else
					myXslTrans.Load(new XmlTextReader(new StringReader(Resources.text_2_1_to_1_5_xsl)));
				myXslTrans.Transform(metadataPath, convertedMetadata.Path);

				convertedMetadata.Detach();
				return convertedMetadata.Path;
			}
		}

		/// <summary>
		/// Previous to metadata version 2, the version was stored in the typeVersion attribute.
		/// For version 2, it is stored in the version attribute.
		/// Here's hoping they don't move it again in version 3...
		/// </summary>
		private decimal? GetVersionFromMetadata(string metadataPath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(metadataPath);
			string versionStr = null;
			var versionAttr = doc.SelectSingleNode("/DBLMetadata/@version");
			if (versionAttr != null)
				versionStr = versionAttr.Value;
			if (string.IsNullOrWhiteSpace(versionStr))
			{
				versionAttr = doc.SelectSingleNode("/DBLMetadata/@typeVersion");
				if (versionAttr != null)
					versionStr = versionAttr.Value;
			}
			decimal version;
			if (Decimal.TryParse(versionStr, out version))
				return version;
			return null;
		}

		#endregion

		#region IDisposable Members
		/// <summary>
		/// Dispose method for this class.
		/// </summary>
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
