using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Net;
using SIL.Xml;

namespace SIL.WritingSystems
{
	/// <summary>
	/// Status for the source of the returned LDML file
	/// </summary>
	public enum SldrStatus
	{
		/// <summary>
		/// LDML file not found in the SLDR or SLDR cache
		/// </summary>
		FileNotFound,
		/// <summary>
		/// Unable to connect to SLDR and LDML file not found in SLDR cache
		/// </summary>
		UnableToConnectToSldr,
		/// <summary>
		/// LDML file from SLDR
		/// </summary>
		FileFromSldr,
		/// <summary>
		/// LDML file not found in SLDR, but in SLDR cache
		/// </summary>
		FileFromSldrCache
	};

	public static class Sldr
	{
		public static XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		// SLDR is hosted on two sites.  Generally we will use the production (public) site.
		private const string ProductionSldrRepository = "https://ldml.api.sil.org/";
		// Staging site listed here for developmental testing
		// private const string StagingSldrRepository = "http://staging.scriptsource.org/ldml/";

		private const string SldrCacheDir = "SldrCache";
		private const string TmpExtension = "tmp";

		// Default parameters for querying the SLDR
		private const string LdmlExtension = "ldml";

		// Default list of elements to request from the SLDR.
		// Identity is always published, so we don't need it on the list.
		public static readonly IEnumerable<string> DefaultTopElements = new List<string>
		{
			"characters",
			"delimiters",
			"layout",
			"numbers",
			"collations",
			"special"
		};
 
		// If the user wants to request a new UID, you use "uid=unknown" and that will create a new random identifier
		public const string DefaultUserId = "unknown";

		/// <summary>
		/// API to query the SLDR for an LDML file and save it locally in the SLDR cache and specified directories
		/// </summary>
		/// <param name="destinationPath">Destination path to save the requested LDML file</param>
		/// <param name="ietfLanguageTag">Current IETF language tag</param>
		/// <param name="topLevelElements">List of top level element names to request. SLDR will always publish identity, so it doesn't need to be requested.
		/// If null, the default list of {"characters", "delimiters", "layout", "numbers", "collations", "special"} will be requested.</param>
		/// <param name="filename">Saved filename</param>
		/// <returns>Enum status SldrStatus if file could be retrieved and the source</returns>
		public static SldrStatus GetLdmlFile(string destinationPath, string ietfLanguageTag, IEnumerable<string> topLevelElements, out string filename)
		{
			if (String.IsNullOrEmpty(destinationPath))
				throw new ArgumentException("destinationPath");
			if (!Directory.Exists(destinationPath))
				throw new DirectoryNotFoundException("destinationPath");
			if (String.IsNullOrEmpty(ietfLanguageTag) || (!IetfLanguageTagHelper.IsValid(ietfLanguageTag)))
				throw new ArgumentException("ietfLanguageTag");
			if (topLevelElements == null)
				throw new ArgumentException("topLevelElements");

			var status = SldrStatus.FileNotFound;
			string sldrCachePath = Path.Combine(Path.GetTempPath(), SldrCacheDir);
			Directory.CreateDirectory(sldrCachePath);
			string sldrCacheFilePath;
			bool redirected;
			const Boolean flatten = true;
			do
			{
				filename = ietfLanguageTag + "." + LdmlExtension;
				string revid, uid;

				// Check if LDML file already exists in destination and read revid and uid
				if (!ReadSilIdentity(Path.Combine(destinationPath, filename), out revid, out uid))
					uid = DefaultUserId;

				// Concatenate parameters for url string
				string requestedElements = string.Join("&inc[]=", topLevelElements);
				string requestedUserId = !String.IsNullOrEmpty(uid) ? String.Format("&uid={0}", uid) : String.Empty;
				string url = string.Format("{0}{1}?ext={2}&inc[]={3}&flatten={4}{5}",
					ProductionSldrRepository, ietfLanguageTag, LdmlExtension,
					requestedElements, Convert.ToInt32(flatten), requestedUserId);

				sldrCacheFilePath = Path.Combine(sldrCachePath, filename);
				string tempFilePath = sldrCacheFilePath + "." + TmpExtension;

				// Using WebRequest instead of WebClient so we have access to disable AllowAutoRedirect
				var webRequest = (HttpWebRequest) WebRequest.Create(Uri.EscapeUriString(url));
				webRequest.AllowAutoRedirect = false;
				webRequest.Timeout = 10000;

				try
				{
					// Check the response header to see if the requested LDML file got redirected
					using (var webResponse = (HttpWebResponse) webRequest.GetResponse())
					{
						if (webResponse.StatusCode == HttpStatusCode.MovedPermanently)
						{
							ietfLanguageTag = webResponse.Headers["Location"].Replace(ProductionSldrRepository, "");
							redirected = true;
						}
						else
						{
							// Download the LDML file to a temp file in case the transfer gets interrupted
							using (Stream responseStream = webResponse.GetResponseStream())
							using (var fs = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write))
							{
								var buff = new byte[102400];
								int c;
								while ((c = responseStream.Read(buff, 0, buff.Length)) > 0)
								{
									fs.Write(buff, 0, c);
									fs.Flush();
								}
							}

							status = SldrStatus.FileFromSldr;
							sldrCacheFilePath = MoveTmpToCache(tempFilePath, uid);
							redirected = false;
						}
					}
				}
				catch (WebException we)
				{
					// Return from 404 error
					var errorResponse = we.Response as HttpWebResponse;
					if ((errorResponse != null) && (errorResponse.StatusCode == HttpStatusCode.NotFound))
						return SldrStatus.FileNotFound;

					string sldrCacheFilename;
					// Download failed so check SLDR cache
					if (!string.IsNullOrEmpty(uid) && (uid != DefaultUserId))
						sldrCacheFilename = string.Format("{0}-{1}.{2}", ietfLanguageTag, uid, LdmlExtension);
					else
						sldrCacheFilename = string.Format("{0}.{1}", ietfLanguageTag, LdmlExtension);
					sldrCacheFilePath = Path.Combine(sldrCachePath, sldrCacheFilename);
					if (File.Exists(sldrCacheFilePath))
						status = SldrStatus.FileFromSldrCache;
					else
						return SldrStatus.UnableToConnectToSldr;
					redirected = false;
				}
			} while (redirected);

			// Copy from Cache to destination (w/o uid in filename), overwriting whatever used to be there
			File.Copy(sldrCacheFilePath, Path.Combine(destinationPath, filename), true);

			return status;
		}

		/// <summary>
		/// API request to return an LDML file and save it
		/// </summary>
		/// <param name="destinationPath">Destination path to save the requested LDML file</param>
		/// <param name="ietfLanguageTag">Current IETF language tag</param>
		/// <param name="filename">Saved filename</param>
		/// <returns>Enum status SldrStatus if file could be retrieved and the source</returns>
		public static SldrStatus GetLdmlFile(string destinationPath, string ietfLanguageTag, out string filename)
		{
			return GetLdmlFile(destinationPath, ietfLanguageTag, DefaultTopElements, out filename);
		}

		/// <summary>
		/// Utility to read the SIL:Identity element and return the values to the revid and uid attributes.
		/// Returns boolean if the LDML file exists.
		/// </summary>
		/// <param name="filePath">Full path to the LDML file to parse</param>
		/// <param name="revid">This contains the SHA of the git revision that was current when the user pulled the LDML file. 
		/// This attribute is stripped from files before inclusion in the SLDR</param>
		/// <param name="uid">This holds a unique id that identifies a particular editor of a file. Notice that no two uids will be the same even across LDML files. 
		/// Thus the uid is a unique identifier for an LDML file as edited by a user. If a user downloads a file and they don't already have a 
		/// uid for that file then they should use the given uid. On subsequent downloads they must update the uid to the existing uid for that file.
		/// In implementation terms the UID is calculated as the 32-bit timestamp of the file request from the server, 
		/// with another 16-bit collision counter appended and represented in MIME64 as 8 characters. 
		/// This attribute is stripped from files before inclusion in the SLDR.</param>
		/// <returns>Boolean if the LDML file exists</returns>
		internal static bool ReadSilIdentity(string filePath, out string revid, out string uid)
		{
			revid = String.Empty;
			uid = String.Empty;

			if (String.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");
			if (!File.Exists(filePath))
				return false;
			XElement element = XElement.Load(filePath);
			if (element.Name != "ldml")
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");

			XElement identityElem = element.Element("identity");
			if (identityElem != null)
			{
				XElement specialElem = identityElem.NonAltElement("special");
				if (specialElem != null)
				{
					XElement silIdentityElem = specialElem.Element(Sil + "identity");
					if (silIdentityElem != null)
					{
						revid = (string) silIdentityElem.Attribute("revid") ?? string.Empty;
						uid = (string) silIdentityElem.Attribute("uid") ?? string.Empty;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Process the tmp file and move to Sldr cache.  First, check the LDML tmp file to see if draft attribute is "approved".
		/// If approved: uid attribute is removed. tmp file is saved to cache as "filename", and delete "filename + uid" in cache
		/// Otherwise, tmp file is saved to cache as "filename + uid"
		/// </summary>
		/// <param name="filePath">Full path to the tmp LDML file</param>
		/// <param name="originalUid">Uid read from the exisiting LDML file, before the SLDR query</param>
		/// <returns>Path to the LDML file in SLDR cache</returns>
		internal static string MoveTmpToCache(string filePath, string originalUid)
		{
			XElement element = XElement.Load(filePath);
			if (element.Name != "ldml")
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");

			string uid = string.Empty;
			string sldrCacheFilePath = filePath.Replace("." + TmpExtension, "");

			XElement identityElem = element.Element("identity");
			if (identityElem != null)
			{
				XElement specialElem = identityElem.NonAltElement("special");
				if (specialElem != null)
				{
					XElement silIdentityElem = specialElem.Element(Sil + "identity");
					if (silIdentityElem != null)
					{
						if ((string) silIdentityElem.Attribute("draft") == "approved")
						{
							// Remove uid attribute
							uid = string.Empty;
							silIdentityElem.SetOptionalAttributeValue("uid", uid);

							// Clean out original LDML file that contains uid in cache
							string originalFile = sldrCacheFilePath.Replace("." + LdmlExtension, "-" + originalUid + "." + LdmlExtension);
							if (File.Exists(originalFile))
								File.Delete(originalFile);
						}
						else
							uid = (string) silIdentityElem.Attribute("uid");
					}
				}
			}

			// sldrCache/filename.ldml.tmp
			// Remove tmp extension and append -uid if needed
			if (!string.IsNullOrEmpty(uid))
				sldrCacheFilePath = sldrCacheFilePath.Replace("." + LdmlExtension, "-" + uid + "." + LdmlExtension);

			// Use Canonical xml settings suitable for use in Chorus applications
			// except NewLineOnAttributes to conform to SLDR files
			var writerSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
			writerSettings.NewLineOnAttributes = false;

			using (var writer = XmlWriter.Create(sldrCacheFilePath, writerSettings))
				element.WriteTo(writer);

			if (filePath != sldrCacheFilePath)
				File.Delete(filePath);
				
			return sldrCacheFilePath;	
		}
	}
}
