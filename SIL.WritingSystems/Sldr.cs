using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Globalization;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIL.Extensions;
using SIL.Network;
using SIL.ObjectModel;
using SIL.PlatformUtilities;
using SIL.Threading;
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

	/// <summary>
	/// This class provides methods for retrieving LDML files and tag data from the SIL Locale Data Repository (SLDR) web service.
	/// </summary>
	public static class Sldr
	{
		public static XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		// SLDR is hosted on two sites.
#if STAGING_SLDR
		// Staging site listed here for developmental testing
		private const string SldrRepository = "http://staging.scriptsource.org/ldml/";
#else
		// Generally we will use the production (public) site.
		private const string SldrRepository = "https://ldml.api.sil.org/";
#endif

		private const string SldrGitHubRepo = "https://api.github.com/repos/silnrsi/sldr/";

		private const string TmpExtension = "tmp";

		// Default parameters for querying the SLDR
		private const string LdmlExtension = "ldml";

		private const string UserAgent = "SIL.WritingSystems Library";

		// Mode to test when the SLDR is unavailable.  Default to false
		private static bool _offlineMode;

		// If the user wants to request a new UID, you use "uid=unknown" and that will create a new random identifier
		public const string DefaultUserId = "unknown";

		// in order to avoid deadlocks, SyncRoot should always be acquired first and then SldrCacheMutex
		private static readonly object SyncRoot = new object();
		// multiple applications could read/write to the SLDR cache at the same time, so synchronize access
		private static GlobalMutex _sldrCacheMutex;

		private static IReadOnlyKeyedCollection<string, SldrLanguageTagInfo> _languageTags;

		internal static string DefaultSldrCachePath
		{
			get
			{
				string basePath = Platform.IsLinux ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
					: Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				return Path.Combine(basePath, "SIL", "SldrCache");
			}
		}

		/// <summary>
		/// Gets the SLDR cache path.
		/// </summary>
		public static string SldrCachePath { get; private set; }

		private static readonly DateTime DefaultEmbeddedAllTagsTime = DateTime.Parse(LanguageRegistryResources.AllTagsTime, CultureInfo.InvariantCulture);
		private static DateTime _embeddedAllTagsTime;

		/// <summary>
		/// Initializes the SLDR. This should be called before calling other methods or properties.
		/// </summary>
		public static void Initialize(bool offlineMode = false)
		{
			Initialize(offlineMode, DefaultSldrCachePath);
		}

		/// <summary>
		/// This method is used for testing purposes.
		/// </summary>
		internal static void Initialize(bool offlineMode, string sldrCachePath)
		{
			Initialize(offlineMode, sldrCachePath, DefaultEmbeddedAllTagsTime);
		}

		/// <summary>
		/// This method is used for testing purposes.
		/// </summary>
		internal static void Initialize(bool offlineMode, string sldrCachePath, DateTime embeddedAllTagsTime)
		{
			if (IsInitialized)
				throw new InvalidOperationException("The SLDR has already been initialized.");

			_sldrCacheMutex = new GlobalMutex("SldrCache");
			_sldrCacheMutex.Initialize();
			_offlineMode = offlineMode;
			SldrCachePath = sldrCachePath;
			_embeddedAllTagsTime = embeddedAllTagsTime;
		}

		public static bool IsInitialized
		{
			get { return _sldrCacheMutex != null; }
		}

		/// <summary>
		/// Cleans up the SLDR. This should be called to properly clean up SLDR resources.
		/// </summary>
		public static void Cleanup()
		{
			CheckInitialized();

			_sldrCacheMutex.Dispose();
			_sldrCacheMutex = null;
			_languageTags = null;
		}

		private static void CheckInitialized()
		{
			if (!IsInitialized)
				throw new InvalidOperationException("The SLDR has not been initialized.");
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
			CheckInitialized();

			if (String.IsNullOrEmpty(destinationPath))
				throw new ArgumentException("destinationPath");
			if (!Directory.Exists(destinationPath))
				throw new DirectoryNotFoundException("destinationPath");
			if (String.IsNullOrEmpty(languageTag) || (!IetfLanguageTag.IsValid(languageTag)))
				throw new ArgumentException("ietfLanguageTag");
			if (topLevelElements == null)
				throw new ArgumentNullException("topLevelElements");

			string sldrLanguageTag = IetfLanguageTag.Canonicalize(languageTag);
			SldrLanguageTagInfo langTagInfo;
			if (LanguageTags.TryGet(sldrLanguageTag, out langTagInfo))
				sldrLanguageTag = langTagInfo.SldrLanguageTag;
			string[] topLevelElementsArray = topLevelElements.ToArray();

			using (_sldrCacheMutex.Lock())
			{
				var status = SldrStatus.NotFound;
				CreateSldrCacheDirectory();
				string sldrCacheFilePath;
				bool redirected;
				do
				{
					string revid, uid = "", tempString;
					if (destinationPath == SldrCachePath)
					{
						filename = string.Format("{0}.{1}", sldrLanguageTag, LdmlExtension);
					}
					else
					{
						filename = string.Format("{0}.{1}", languageTag, LdmlExtension);
						// Check if LDML file already exists in destination and read revid and uid
						if (!ReadSilIdentity(Path.Combine(destinationPath, filename), out tempString, out uid))
							uid = DefaultUserId;
					}

					// If languageTag contains fonipa, don't bother trying to access the SLDR
					if (sldrLanguageTag.Contains(WellKnownSubtags.IpaVariant) || sldrLanguageTag.Contains(WellKnownSubtags.AudioScript))
						return SldrStatus.NotFound;

					sldrCacheFilePath = Path.Combine(SldrCachePath, !string.IsNullOrEmpty(uid) && uid != DefaultUserId ? string.Format("{0}-{1}.{2}", sldrLanguageTag, uid, LdmlExtension)
						: string.Format("{0}.{1}", sldrLanguageTag, LdmlExtension));
					// Read revid from cache file
					ReadSilIdentity(sldrCacheFilePath, out revid, out tempString);

					// Concatenate parameters for url string
					string requestedElements = string.Empty;
					if (topLevelElementsArray.Length > 0)
						requestedElements = string.Format("&inc[]={0}", string.Join("&inc[]=", topLevelElementsArray));
					string requestedUserId = !string.IsNullOrEmpty(uid) ? string.Format("&uid={0}", uid) : string.Empty;
					string requestedRevid = !string.IsNullOrEmpty(revid) ? string.Format("&revid={0}", revid) : string.Empty;
					string url = string.Format("{0}{1}?ext={2}&flatten=1{3}{4}{5}",
						SldrRepository, sldrLanguageTag, LdmlExtension,
						requestedElements, requestedUserId, requestedRevid);

					string tempFilePath = sldrCacheFilePath + "." + TmpExtension;

					// Using WebRequest instead of WebClient so we have access to disable AllowAutoRedirect
					var webRequest = (HttpWebRequest) WebRequest.Create(Uri.EscapeUriString(url));
					webRequest.AllowAutoRedirect = false;
					webRequest.UserAgent = UserAgent;
					webRequest.Timeout = 10000;

					try
					{
						if (_offlineMode)
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
								var parsedresponse = HttpUtilityFromMono.ParseQueryString(webResponse.Headers["Location"]);
								sldrLanguageTag = parsedresponse.Get("ws_id").Split('?')[0];
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
							sldrCacheFilename = string.Format("{0}-{1}.{2}", sldrLanguageTag, uid, LdmlExtension);
						else
							sldrCacheFilename = string.Format("{0}.{1}", sldrLanguageTag, LdmlExtension);
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
		}

		/// <summary>
		/// API request to return an LDML file and save it
		/// </summary>
		/// <param name="destinationPath">Destination path to save the requested LDML file</param>
		/// <param name="canonicalLanguageTag">Current IETF language tag</param>
		/// <param name="filename">Saved filename</param>
		/// <returns>Enum status SldrStatus if file could be retrieved and the source</returns>
		public static SldrStatus GetLdmlFile(string destinationPath, string canonicalLanguageTag, out string filename)
		{
			return GetLdmlFile(destinationPath, canonicalLanguageTag, Enumerable.Empty<string>(), out filename);
		}

		public static IReadOnlyKeyedCollection<string, SldrLanguageTagInfo> LanguageTags
		{
			get
			{
				CheckInitialized();

				lock (SyncRoot)
				{
					LoadLanguageTagsIfNecessary();
					return _languageTags;
				}
			}
		}

		/// <summary>
		/// Gets the language tags of the available LDML files in the SLDR.
		/// </summary>
		private static void LoadLanguageTagsIfNecessary()
		{
			if (_languageTags != null)
				return;

			string allTagsContent;
			using (_sldrCacheMutex.Lock())
			{
				CreateSldrCacheDirectory();

				string cachedAllTagsPath = Path.Combine(SldrCachePath, "alltags.txt");
				DateTime latestCommitTime = DateTime.MinValue;
				DateTime sinceTime = _embeddedAllTagsTime;
				if (File.Exists(cachedAllTagsPath))
				{
					DateTime fileTime = File.GetLastWriteTime(cachedAllTagsPath);
					if (sinceTime > fileTime)
						// delete the old alltags.txt file if a newer embedded one is available.
						// this can happen if the application is upgraded to use a newer version of SIL.WritingySystems
						// that has an updated embedded alltags.txt file.
						File.Delete(cachedAllTagsPath);
					else
						sinceTime = fileTime;
				}
				sinceTime += TimeSpan.FromSeconds(1);
				try
				{
					if (_offlineMode)
						throw new WebException("Test mode: SLDR offline so accessing cache", WebExceptionStatus.ConnectFailure);

					// query the SLDR Git repo to see if there is an updated version of alltags.txt
					string commitUrl = string.Format("{0}commits?path=extras/alltags.txt&since={1:O}",
						SldrGitHubRepo, sinceTime);
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
								// get the timestamp of the most recent commit
								JArray commits = JArray.Load(new JsonTextReader(responseReader));
								foreach (JObject commit in commits.Children<JObject>())
								{
									var time = commit["commit"]["author"]["date"].ToObject<DateTime>();
									if (time > latestCommitTime)
										latestCommitTime = time;
								}
							}
						}
					}

					if (latestCommitTime > DateTime.MinValue)
					{
						// there is an updated version of the alltags.txt file in the SLDR Git repo, so get it
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
									File.SetLastWriteTime(cachedAllTagsPath, latestCommitTime);
								}
							}
						}
					}
				}
				catch (WebException)
				{
				}

				allTagsContent = File.Exists(cachedAllTagsPath) ? File.ReadAllText(cachedAllTagsPath) : LanguageRegistryResources.alltags;
			}

			_languageTags = new ReadOnlyKeyedCollection<string, SldrLanguageTagInfo>(ParseAllTags(allTagsContent));
		}

		internal static IKeyedCollection<string, SldrLanguageTagInfo> ParseAllTags(string allTagsContent)
		{
			string[] allTags = allTagsContent.Replace("\r\n", "\n").Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
			var tags = new KeyedList<string, SldrLanguageTagInfo>(info => info.LanguageTag, StringComparer.InvariantCultureIgnoreCase);
			foreach (string line in allTags)
			{
				string tagsStr = line;
				// trim off the explicit inheritance relationship information, we don't care about inheritance
				int index = line.LastIndexOf('>');
				if (index != -1)
					tagsStr = line.Substring(0, index).Trim();
				// split the the line into groups of equivalent language tags
				// the bar character is used to show implicit inheritance relationships between tags,
				// we don't care about inheritance
				string[] equivalentTagsStrs = tagsStr.Split('|');
				foreach (string equivalentTagsStr in equivalentTagsStrs)
				{
					// split each group of equivalent language tags into individual language tags
					string[] tagStrs = equivalentTagsStr.Split('=');
					for (int i = 0; i < tagStrs.Length; i++)
						tagStrs[i] = tagStrs[i].Trim();
					// check if language tag is available in the SLDR
					bool isAvailable = tagStrs[0].StartsWith("*");
					if (isAvailable)
						tagStrs[0] = tagStrs[0].Substring(1);
					string sldrLangTag = tagStrs[0];
					// check if a tag with a script code is equivalent to a tag without a script code
					// this tells us that the script is implicit
					string langTag, implicitStringCode;
					if (equivalentTagsStrs.Length == 1 && tagStrs.Length == 1)
					{
						// special case where there is a single tag on a line
						// if it contains a script code, then the script is implicit
						string[] components = tagStrs[0].Split('-');
						if (components.Length == 2 && components[1].Length == 4)
						{
							langTag = components[0];
							implicitStringCode = components[1];
						}
						else
						{
							langTag = tagStrs[0];
							implicitStringCode = null;
						}
					}
					else
					{
						var minTag = tagStrs.Select(t => new {Tag = t, Components = t.Split('-')}).MinBy(t => t.Components.Length);
						langTag = minTag.Tag;
						implicitStringCode = null;
						// only look for an implicit script code if the minimal tag has no script code
						if (minTag.Components.Length < 2 || minTag.Components[1].Length != 4)
						{
							foreach (string tagStr in tagStrs)
							{
								string[] components = tagStr.Split('-');
								if (components.Length == minTag.Components.Length + 1 && components[1].Length == 4)
									implicitStringCode = components[1];
							}
						}
					}
					SldrLanguageTagInfo existingTag;
					if (tags.TryGet(langTag, out existingTag))
					{
						// alltags.txt can contain multiple lines that contain the same language tag.
						// if one of the lines contains information on an implicit script for a language tag, 
						// we don't want to lose that information, so we preserve it by not replacing the
						// SldrLanguageTagInfo object for this language tag.
						if (existingTag.ImplicitScriptCode != null)
							continue;
						tags.Remove(langTag);
					}
					tags.Add(new SldrLanguageTagInfo(langTag, implicitStringCode, sldrLangTag, isAvailable));
				}
			}

			return tags;
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

		private static void CreateSldrCacheDirectory()
		{
			if (Directory.Exists(SldrCachePath))
				return;

			DirectoryInfo di = Directory.CreateDirectory(SldrCachePath);
			if (!Platform.IsLinux && !SldrCachePath.StartsWith(Path.GetTempPath()))
			{
				// NOTE: GetAccessControl/ModifyAccessRule/SetAccessControl is not implemented in Mono
				DirectorySecurity ds = di.GetAccessControl();
				var sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
				AccessRule rule = new FileSystemAccessRule(sid, FileSystemRights.Write | FileSystemRights.ReadAndExecute
					| FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
					PropagationFlags.InheritOnly, AccessControlType.Allow);
				bool modified;
				ds.ModifyAccessRule(AccessControlModification.Add, rule, out modified);
				di.SetAccessControl(ds);
			}
		}
	}
}
