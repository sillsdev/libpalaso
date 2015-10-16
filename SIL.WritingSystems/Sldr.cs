using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIL.Xml;

namespace SIL.WritingSystems
{
	/// <summary>
	/// Status for the source of the returned LDML file
	/// </summary>
	public enum SldrStatus
	{
		/// <summary>
		/// Data not found in the SLDR
		/// </summary>
		NotFound,
		/// <summary>
		/// Unable to connect to SLDR and data not found in SLDR cache
		/// </summary>
		UnableToConnectToSldr,
		/// <summary>
		/// Data retrieved from SLDR
		/// </summary>
		FromSldr,
		/// <summary>
		/// Unable to connect to SLDR, but data retrieved from SLDR cache
		/// </summary>
		FromCache
	};

	public static class Sldr
	{
		public static XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		// SLDR is hosted on two sites.
#if DEBUG
		// Staging site listed here for developmental testing
		private const string SldrRepository = "http://staging.scriptsource.org/ldml/";
#else
		// Generally we will use the production (public) site.
		private const string SldrRepository = "https://ldml.api.sil.org/";
#endif

		private const string SldrGitHubRepo = "https://api.github.com/repos/silnrsi/sldr/";

		private const string SldrCacheDir = "SldrCache";
		private const string TmpExtension = "tmp";

		// Default parameters for querying the SLDR
		private const string LdmlExtension = "ldml";

		private const string UserAgent = "SIL.WritingSystems Library";

		// Mode to test when the SLDR is unavailable.  Default to false
		public static bool OfflineMode { get; set; }

		// If the user wants to request a new UID, you use "uid=unknown" and that will create a new random identifier
		public const string DefaultUserId = "unknown";

		/// <summary>
		/// Gets the SLDR cache path.
		/// </summary>
		public static string SldrCachePath
		{
			get { return Path.Combine(Path.GetTempPath(), SldrCacheDir); }
		}

		static Sldr()
		{
			OfflineMode = false;
		}

		/// <summary>
		/// API to query the SLDR for an LDML file and save it locally in the SLDR cache and specified directories
		/// </summary>
		/// <param name="destinationPath">Destination path to save the requested LDML file</param>
		/// <param name="languageTag">Current IETF language tag</param>
		/// <param name="topLevelElements">List of top level element names to request. SLDR will always publish identity, so it doesn't need to be requested.
		/// If null, the entire LDML file will be requested.</param>
		/// <param name="filename">Saved filename</param>
		/// <returns>Enum status SldrStatus if file could be retrieved and the source</returns>
		public static SldrStatus GetLdmlFile(string destinationPath, string languageTag, IEnumerable<string> topLevelElements, out string filename)
		{
			if (String.IsNullOrEmpty(destinationPath))
				throw new ArgumentException("destinationPath");
			if (!Directory.Exists(destinationPath))
				throw new DirectoryNotFoundException("destinationPath");
			if (String.IsNullOrEmpty(languageTag) || (!IetfLanguageTag.IsValid(languageTag)))
				throw new ArgumentException("ietfLanguageTag");
			if (topLevelElements == null)
				throw new ArgumentNullException("topLevelElements");

			languageTag = IetfLanguageTag.Normalize(languageTag, IetfLanguageTagNormalizationForm.Canonical);
			string[] topLevelElementsArray = topLevelElements.ToArray();

			var status = SldrStatus.NotFound;
			Directory.CreateDirectory(SldrCachePath);
			string sldrCacheFilePath;
			bool redirected;
			do
			{
				string revid, uid = "", tempString;
				if (destinationPath == SldrCachePath)
				{
					filename = string.Format("{0}.{1}", languageTag, LdmlExtension);
				}
				else
				{
					filename = string.Format("{0}.{1}", IetfLanguageTag.Normalize(languageTag, IetfLanguageTagNormalizationForm.SilCompatible), LdmlExtension);
					// Check if LDML file already exists in destination and read revid and uid
					if (!ReadSilIdentity(Path.Combine(destinationPath, filename), out tempString, out uid))
						uid = DefaultUserId;
				}

				// If languageTag contains fonipa, don't bother trying to access the SLDR
				if (Regex.Match(languageTag, @"fonipa").Success)
				{
					return SldrStatus.NotFound;
				}

				sldrCacheFilePath = Path.Combine(SldrCachePath, !string.IsNullOrEmpty(uid) && uid != DefaultUserId ? string.Format("{0}-{1}.{2}", languageTag, uid, LdmlExtension)
					: string.Format("{0}.{1}", languageTag, LdmlExtension));
				// Read revid from cache file
				ReadSilIdentity(sldrCacheFilePath, out revid, out tempString);

				// Concatenate parameters for url string
				string requestedElements = string.Empty;
				if (topLevelElementsArray.Length > 0)
					requestedElements = string.Format("&inc[]={0}", string.Join("&inc[]=", topLevelElementsArray));
				string requestedUserId = !string.IsNullOrEmpty(uid) ? string.Format("&uid={0}", uid) : string.Empty;
				string requestedRevid = !string.IsNullOrEmpty(revid) ? string.Format("&revid={0}", revid) : string.Empty;
				string url = string.Format("{0}{1}?ext={2}&flatten=1{3}{4}{5}",
					SldrRepository, languageTag, LdmlExtension,
					requestedElements, requestedUserId, requestedRevid);

				string tempFilePath = sldrCacheFilePath + "." + TmpExtension;

				// Using WebRequest instead of WebClient so we have access to disable AllowAutoRedirect
				var webRequest = (HttpWebRequest) WebRequest.Create(Uri.EscapeUriString(url));
				webRequest.AllowAutoRedirect = false;
				webRequest.UserAgent = UserAgent;
				webRequest.Timeout = 10000;

				try
				{
					if (OfflineMode)
						throw new WebException("Test mode: SLDR offline so accessing cache", WebExceptionStatus.ConnectFailure);

					// Check the response header to see if the requested LDML file got redirected
					using (var webResponse = (HttpWebResponse) webRequest.GetResponse())
					{
						if (webResponse.StatusCode == HttpStatusCode.NotModified)
						{
							// Report status that file is the most current from SLDR
							status = SldrStatus.FromSldr;
							redirected = false;
						}
						else if (webResponse.StatusCode == HttpStatusCode.MovedPermanently)
						{
							// Extract ietfLanguageTag from the response header
							string responseString = webResponse.Headers["Location"].Replace(SldrRepository, "");
							languageTag = responseString.Split('?')[0];
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

							status = SldrStatus.FromSldr;
							sldrCacheFilePath = MoveTmpToCache(tempFilePath, uid);
							redirected = false;
						}
					}
				}
				catch (WebException we)
				{
					// Return from 404 error
					var errorResponse = (HttpWebResponse) we.Response;
					if ((we.Status == WebExceptionStatus.ProtocolError) && (errorResponse.StatusCode == HttpStatusCode.NotFound))
						return SldrStatus.NotFound;

					string sldrCacheFilename;
					// Download failed so check SLDR cache
					if (!string.IsNullOrEmpty(uid) && (uid != DefaultUserId))
						sldrCacheFilename = string.Format("{0}-{1}.{2}", languageTag, uid, LdmlExtension);
					else
						sldrCacheFilename = string.Format("{0}.{1}", languageTag, LdmlExtension);
					sldrCacheFilePath = Path.Combine(SldrCachePath, sldrCacheFilename);
					if (File.Exists(sldrCacheFilePath))
						status = SldrStatus.FromCache;
					else
						return SldrStatus.UnableToConnectToSldr;
					redirected = false;
				}
				finally
				{
					if (File.Exists(tempFilePath))
						File.Delete(tempFilePath);
				}
			} while (redirected);

			if (destinationPath != SldrCachePath)
			{
				// Copy from Cache to destination (w/o uid in filename), overwriting whatever used to be there
				File.Copy(sldrCacheFilePath, Path.Combine(destinationPath, filename), true);
			}

			return status;
		}

		/// <summary>
		/// API request to return an LDML file and save it
		/// </summary>
		/// <param name="destinationPath">Destination path to save the requested LDML file</param>
		/// <param name="languageTag">Current IETF language tag</param>
		/// <param name="filename">Saved filename</param>
		/// <returns>Enum status SldrStatus if file could be retrieved and the source</returns>
		public static SldrStatus GetLdmlFile(string destinationPath, string languageTag, out string filename)
		{
			return GetLdmlFile(destinationPath, languageTag, Enumerable.Empty<string>(), out filename);
		}

		/// <summary>
		/// Gets the language tags of the available LDML files in the SLDR.
		/// </summary>
		public static bool GetAvailableLanguageTags(out IEnumerable<string> langTags)
		{
			Directory.CreateDirectory(SldrCachePath);

			string cachedAllTagsPath = Path.Combine(SldrCachePath, "alltags.txt");
			bool checkedGitHub = true;
			DateTime latestCommitTime = DateTime.MinValue;
			try
			{
				if (OfflineMode)
					throw new WebException("Test mode: SLDR offline so accessing cache", WebExceptionStatus.ConnectFailure);

				string commitUrl = string.Format("{0}commits?path=extras/alltags.txt", SldrGitHubRepo);
				if (File.Exists(cachedAllTagsPath))
				{
					DateTime sinceTime = File.GetLastWriteTimeUtc(cachedAllTagsPath);
					sinceTime += TimeSpan.FromSeconds(1);
					commitUrl += string.Format("&since={0:O}", sinceTime);
				}

				var webRequest = (HttpWebRequest) WebRequest.Create(Uri.EscapeUriString(commitUrl));
				webRequest.UserAgent = UserAgent;
				webRequest.Timeout = 10000;
				using (var webResponse = (HttpWebResponse) webRequest.GetResponse())
				{
					Stream stream = webResponse.GetResponseStream();
					if (stream != null)
					{
						using (StreamReader responseReader = new StreamReader(stream))
						{
							JArray commits = JArray.Load(new JsonTextReader(responseReader));
							foreach (JObject commit in commits.Children<JObject>())
							{
								DateTime time = DateTime.Parse((string) commit["commit"]["author"]["date"]);
								if (time > latestCommitTime)
									latestCommitTime = time;
							}
						}
					}
				}

				if (latestCommitTime > DateTime.MinValue)
				{
					// there is an update to the alltags.txt file
					string contentsUrl = string.Format("{0}contents/extras/alltags.txt", SldrGitHubRepo);
					webRequest = (HttpWebRequest) WebRequest.Create(Uri.EscapeUriString(contentsUrl));
					webRequest.UserAgent = UserAgent;
					webRequest.Timeout = 10000;
					using (var webResponse = (HttpWebResponse) webRequest.GetResponse())
					{
						Stream stream = webResponse.GetResponseStream();
						if (stream != null)
						{
							using (StreamReader responseReader = new StreamReader(stream))
							{
								JObject blob = JObject.Load(new JsonTextReader(responseReader));
								File.WriteAllBytes(cachedAllTagsPath, Convert.FromBase64String((string) blob["content"]));
								File.SetLastWriteTimeUtc(cachedAllTagsPath, latestCommitTime);
							}
						}
					}
				}
			}
			catch (WebException)
			{
				checkedGitHub = false;
			}

			string allTagsContents = File.Exists(cachedAllTagsPath) ? File.ReadAllText(cachedAllTagsPath) : LanguageRegistryResources.alltags;
			string[] allTags = allTagsContents.Replace("\r\n", "\n").Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
			var tags = new HashSet<string>();
			foreach (string line in allTags)
			{
				var tag = new StringBuilder();
				bool available = false;
				foreach (char ch in line)
				{
					switch (ch)
					{
						case '*':
							available = true;
							break;

						case '|':
						case '=':
						case '>':
							if (available)
								tags.Add(tag.ToString());
							tag = new StringBuilder();
							available = false;
							break;

						case ' ':
							break;

						default:
							tag.Append(ch);
							break;
					}
				}
				if (available)
					tags.Add(tag.ToString());
			}

			langTags = tags;
			return checkedGitHub;
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
							string originalFile = string.Empty;
							if (!string.IsNullOrEmpty(originalUid) && (originalUid != DefaultUserId))
								originalFile = sldrCacheFilePath.Replace("." + LdmlExtension, "-" + originalUid + "." + LdmlExtension);
							if (File.Exists(originalFile))
								File.Delete(originalFile);
						}
						else
							uid = (string) silIdentityElem.Attribute("uid");
					}
				}
			}

			// sldrCache/filename.ldml
			// append -uid if needed
			if (!string.IsNullOrEmpty(uid))
				sldrCacheFilePath = sldrCacheFilePath.Replace("." + LdmlExtension, "-" + uid + "." + LdmlExtension);

			// Use Canonical xml settings suitable for use in Chorus applications
			// except NewLineOnAttributes to conform to SLDR files
			var writerSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
			writerSettings.NewLineOnAttributes = false;

			using (var writer = XmlWriter.Create(sldrCacheFilePath, writerSettings))
				element.WriteTo(writer);

			File.Delete(filePath);
				
			return sldrCacheFilePath;	
		}
	}
}
