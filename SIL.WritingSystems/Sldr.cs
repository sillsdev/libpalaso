using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web;
using Icu;
using Newtonsoft.Json;
using SIL.Extensions;
using SIL.ObjectModel;
using SIL.PlatformUtilities;
using SIL.Threading;
using SIL.Xml;
using SIL.IO;

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

	// TODO Decide where these go because LanguageLookup will want them as well
	public class AllTagEntry
	{
		public bool deprecated { get; set; }
		public string full { get; set; }
		public string iso639_3 { get; set; }
		public string localname { get; set; }
		public string name { get; set; }
		public List<string> names { get; set; }
		public string region { get; set; }
		public List<string> regions { get; set; }
		public bool sldr { get; set; }
		public string tag { get; set; }
		public List<string> tags { get; set; }
		[JsonConverter(typeof(SingleStringToListOfStringConverter))]
		public List<string> iana { get; set; }
		public string regionName { get; set; }
	}

	/// <summary>
	/// This class provides methods for retrieving LDML files and tag data from the SIL Locale Data Repository (SLDR) web service.
	/// </summary>
	public static class Sldr
	{
		public static XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		// public api for the SLDR
		private const string SldrRepository = "https://ldml.api.sil.org/";

		private const string TmpExtension = "tmp";

		// Default parameters for querying the SLDR
		private const string LdmlExtension = "ldml";

		private const string UserAgent = "SIL.WritingSystems Library";

		// Mode to test when the SLDR is unavailable.  Default to false
		private static bool _offlineMode;

		// If the user wants to request a new UID, you use "uid=unknown" and that will create a new random identifier
		public const string DefaultUserId = "unknown";

		// Name of the environment variable that controls using WSTech's SLDR staging site
		public const string SldrStaging = "SLDR_USE_STAGING";

		// in order to avoid deadlocks, SyncRoot should always be acquired first and then SldrCacheMutex
		private static readonly object SyncRoot = new object();
		// multiple applications could read/write to the SLDR cache at the same time, so synchronize access
		private static GlobalMutex _sldrCacheMutex;

		private static IReadOnlyKeyedCollection<string, SldrLanguageTagInfo> _languageTags;

		internal static string DefaultSldrCachePath
		{
			get
			{
				var basePath = Platform.IsMac ? "/Users/Shared" : Platform.IsLinux ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
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

			InitializeGetUnicodeCategoryBasedOnIcu();
		}

		/// <summary>
		/// This initialization sets an ICU-based implementation to get the Unicode character
		/// category of a character in a string. The default implementation used in
		/// StringExtensions is based on the Unicode support in System.Globalization because
		/// SIL.Core does not reference Icu.net, but if a product is using a version of ICU
		/// that has more up-to-date information, that is the preferred source.
		/// </summary>
		private static void InitializeGetUnicodeCategoryBasedOnIcu()
		{
			if (StringExtensions.AltImplGetUnicodeCategory != null)
				return;
			try
			{
				// Unfortunately, this version has to be looked up in the docs. There is no way
				// (e.g. from System.Globalization) to look this up at runtime.
				const double unicodeVersionOfDotNet462 = 8.0;

				if (double.TryParse(Wrapper.UnicodeVersion, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var icuUnicodeVersion) &&
				    icuUnicodeVersion > unicodeVersionOfDotNet462)
				{
					StringExtensions.AltImplGetUnicodeCategory = GetUnicodeCategoryBasedOnICU;
				}
			}
			catch (System.IO.FileLoadException)
			{
				// The program doesn't have the ICU library available.  We can live with this.
			}
		}

		/// <summary>
		/// Uses ICU to get the Unicode category of the character at the indicated position in the
		/// string.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Index out of range. Must be a positive
		/// number less than the length of the <see cref="s"/></exception>
		/// <exception cref="ArgumentException">String contains invalid surrogate characters (e.g.,
		/// a low surrogate that is not preceded by a high surrogate)</exception>
		/// <remarks>Internal for testing.</remarks>
		internal static UnicodeCategory GetUnicodeCategoryBasedOnICU(string s, int index)
		{
			if (index < 0 || index >= s.Length)
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Index out of range. Must be a positive number less than the" +
					$" length ({s.Length}) of the string: {{s}}");

			// Just to be a bit more robust (like the default
			// "UnicodeInfo.GetUnicodeCategory" implementation), if they gave us the index
			// of the low surrogate and there is a valid high surrogate character preceding
			// it, we'll fix things up so it doesn't throw an exception.
			if (char.IsLowSurrogate(s, index) && index > 0 &&
				char.IsHighSurrogate(s, index - 1))
				index--;

			return Character.GetCharType(char.ConvertToUtf32(s, index))
				.ToUnicodeCategory();
		}

		public static bool IsInitialized => _sldrCacheMutex != null;

		/// <summary>
		/// Cleans up the SLDR. This should be called to properly clean up SLDR resources.
		/// </summary>
		public static void Cleanup()
		{
			CheckInitialized();

			_sldrCacheMutex.Dispose();
			_sldrCacheMutex = null;
			_languageTags = null;

			IcuRulesCollator.DisposeCollators();
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
		/// If empty list, the entire LDML file will be requested.</param>
		/// <param name="filename">Saved filename</param>
		/// <returns>Enum status SldrStatus if file could be retrieved and the source</returns>
		public static SldrStatus GetLdmlFile(string destinationPath, string languageTag, IEnumerable<string> topLevelElements, out string filename)
		{
			CheckInitialized();

			if (string.IsNullOrEmpty(destinationPath))
				throw new ArgumentException("destinationPath");
			if (!Directory.Exists(destinationPath))
				throw new DirectoryNotFoundException("destinationPath");
			if (string.IsNullOrEmpty(languageTag) || (!IetfLanguageTag.IsValid(languageTag)))
				throw new ArgumentException("ietfLanguageTag");
			if (topLevelElements == null)
				throw new ArgumentNullException(nameof(topLevelElements));

			var sldrLanguageTag = IetfLanguageTag.Canonicalize(languageTag);
			if (LanguageTags.TryGet(sldrLanguageTag, out var langTagInfo))
				sldrLanguageTag = langTagInfo.SldrLanguageTag;
			var topLevelElementsArray = topLevelElements.ToArray();

			using (_sldrCacheMutex.Lock())
			{
				var status = SldrStatus.NotFound;
				CreateSldrCacheDirectory();
				string sldrCacheFilePath;
				bool redirected;
				do
				{
					var uid = string.Empty;
					if (destinationPath == SldrCachePath)
					{
						filename = $"{sldrLanguageTag}.{LdmlExtension}";
					}
					else
					{
						filename = $"{languageTag}.{LdmlExtension}";
						// Check if LDML file already exists in destination and read revid and uid
						if (!ReadSilIdentity(Path.Combine(destinationPath, filename), out _, out uid))
							uid = DefaultUserId;
					}

					// If languageTag contains fonipa, don't bother trying to access the SLDR
					if (sldrLanguageTag.Contains(WellKnownSubtags.IpaVariant) || sldrLanguageTag.Contains(WellKnownSubtags.AudioScript))
						return SldrStatus.NotFound;

					sldrCacheFilePath = Path.Combine(SldrCachePath, !string.IsNullOrEmpty(uid) && uid != DefaultUserId ? $"{sldrLanguageTag}-{uid}.{LdmlExtension}"
						: $"{sldrLanguageTag}.{LdmlExtension}");
					// Read revid from cache file
					ReadSilIdentity(sldrCacheFilePath, out var revid, out _);

					// Concatenate parameters for url string
					var requestedElements = string.Empty;
					if (topLevelElementsArray.Length > 0)
						requestedElements = $"&inc[]={string.Join("&inc[]=", topLevelElementsArray)}";
					var requestedUserId = !string.IsNullOrEmpty(uid) ? $"&uid={uid}" : string.Empty;
					var requestedRevid = !string.IsNullOrEmpty(revid) ? $"&revid={revid}" : string.Empty;
					var url = BuildLdmlRequestUrl(sldrLanguageTag, requestedElements, requestedUserId, requestedRevid);

					var tempFilePath = sldrCacheFilePath + "." + TmpExtension;

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
						using var webResponse = (HttpWebResponse) webRequest.GetResponse();
						switch (webResponse.StatusCode)
						{
							case HttpStatusCode.NotModified:
								// Report status that file is the most current from SLDR
								status = SldrStatus.FromSldr;
								redirected = false;
								break;
							case HttpStatusCode.MovedPermanently:
							{
								// Extract ietfLanguageTag from the response header
								var parsedResponse = HttpUtility.ParseQueryString(webResponse.Headers["Location"]);
								sldrLanguageTag = parsedResponse.Get("ws_id").Split('?')[0];
								redirected = true;
								break;
							}
							default:
							{
								// Download the LDML file to a temp file in case the transfer gets interrupted
								using (var responseStream = webResponse.GetResponseStream())
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
								break;
							}
						}
					}
					catch (WebException we)
					{
						// Return from 404 error
						var errorResponse = (HttpWebResponse) we.Response;
						if ((we.Status == WebExceptionStatus.ProtocolError) && (errorResponse.StatusCode == HttpStatusCode.NotFound))
							return SldrStatus.NotFound;

						// Download failed so check SLDR cache
						sldrCacheFilePath = GetSldrCacheFilePath(uid, sldrLanguageTag);
						if (File.Exists(sldrCacheFilePath))
						{
							status = SldrStatus.FromCache;
						}
						else
						{
							return SldrStatus.UnableToConnectToSldr;
						}
						redirected = false;
					}
					catch (XmlException)
					{
						// Download failed so check SLDR cache
						sldrCacheFilePath = GetSldrCacheFilePath(uid, sldrLanguageTag);
						if (File.Exists(sldrCacheFilePath))
						{
							status = SldrStatus.FromCache;
						}
						else
						{
							return SldrStatus.UnableToConnectToSldr;
						}
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


		private static string StagingParameter
		{
			get
			{
				var stagingParam = string.Empty;
				var useStaging = Environment.GetEnvironmentVariable(SldrStaging) ?? "false";
				if (useStaging.ToLower().Equals("true") || useStaging.ToLower().Equals("yes"))
				{
					stagingParam = "&staging=1";
				}

				return stagingParam;
			}
		}

		internal static string BuildLdmlRequestUrl(string sldrLanguageTag, string requestedElements, string requestedUserId, string requestedRevid)
		{
			return
				$"{SldrRepository}{sldrLanguageTag}?ext={LdmlExtension}&flatten=1{requestedElements}{requestedUserId}{requestedRevid}{StagingParameter}";
		}

		private static string GetSldrCacheFilePath(string uid, string sldrLanguageTag)
		{
			string sldrCacheFilename;
			if (!string.IsNullOrEmpty(uid) && (uid != DefaultUserId))
				sldrCacheFilename = $"{sldrLanguageTag}-{uid}.{LdmlExtension}";
			else
				sldrCacheFilename = $"{sldrLanguageTag}.{LdmlExtension}";
			return Path.Combine(SldrCachePath, sldrCacheFilename);
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

		public static void InitializeLanguageTags(bool downloadLangTags = true)
		{
			LoadLanguageTagsIfNecessary();
			if (downloadLangTags) LoadLanguageTags();
		}

		/// <summary>
		/// Gets the language tags of the available LDML files in the SLDR.
		/// </summary>
		private static void LoadLanguageTagsIfNecessary()
		{
			if (_languageTags != null)
				return;

			string cachedAllTagsPath;
			using (_sldrCacheMutex.Lock())
			{
				CreateSldrCacheDirectory();

				cachedAllTagsPath = Path.Combine(SldrCachePath, "langtags.json");
				string etagPath;
				etagPath = Path.Combine(SldrCachePath, "langtags.json.etag");
				var sinceTime = _embeddedAllTagsTime.ToUniversalTime();
				if (File.Exists(cachedAllTagsPath))
				{
					var fileTime = File.GetLastWriteTimeUtc(cachedAllTagsPath);
					if (sinceTime > fileTime)
					{
						// delete the old langtags.json file if a newer embedded one is available.
						// this can happen if the application is upgraded to use a newer version of SIL.WritingSystems
						// that has an updated embedded langtags.json file.
						File.Delete(cachedAllTagsPath);
						File.Delete(etagPath);
					}
					else
						sinceTime = fileTime;
				}
				sinceTime += TimeSpan.FromSeconds(1);

			}
			_languageTags = new ReadOnlyKeyedCollection<string, SldrLanguageTagInfo>(ParseAllTagsJson(cachedAllTagsPath));
		}

		public static void LoadLanguageTags()
		{
			CreateSldrCacheDirectory();
			string cachedAllTagsPath;
			cachedAllTagsPath = Path.Combine(SldrCachePath, "langtags.json");
			string etagPath;
			etagPath = Path.Combine(SldrCachePath, "langtags.json.etag");
			string etag;
			string currentEtag;
			try
			{
				if (_offlineMode)
					throw new WebException("Test mode: SLDR offline so accessing cache", WebExceptionStatus.ConnectFailure);
				// get SLDR langtags.json from the SLDR api compressed
				// it will throw WebException or have status HttpStatusCode.NotModified if file is unchanged or not get it
				var langtagsUrl =
					$"{SldrRepository}index.html?query=langtags&ext=json{StagingParameter}";
				var webRequest = (HttpWebRequest)WebRequest.Create(Uri.EscapeUriString(langtagsUrl));
				webRequest.UserAgent = UserAgent;
				webRequest.Timeout = 10000;
				webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				using var webResponse = (HttpWebResponse)webRequest.GetResponse();
				if (File.Exists(etagPath))
				{
					etag = File.ReadAllText(etagPath);
					currentEtag = webResponse.Headers.Get("Etag");
					if (etag == "")
					{
						File.WriteAllText(etagPath, currentEtag);
					}
					else if (!etag.Equals(currentEtag))
					{
						File.WriteAllText(etagPath, etag);
						webRequest.Headers.Set(etag, "If-None-Match");
						if (webResponse.StatusCode != HttpStatusCode.NotModified)
						{
							using Stream output = File.OpenWrite(cachedAllTagsPath);
							using var input = webResponse.GetResponseStream();
							input.CopyTo(output);
						}
					}
				}
				else
				{
					currentEtag = webResponse.Headers.Get("Etag");
					File.WriteAllText(etagPath, currentEtag);
					if (webResponse.StatusCode != HttpStatusCode.NotModified)
					{
						using Stream output = File.OpenWrite(cachedAllTagsPath);
						using var input = webResponse.GetResponseStream();
						input.CopyTo(output);
					}
				}
			}
			catch (WebException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (IOException)
			{
			}
		}


		private static IKeyedCollection<string, SldrLanguageTagInfo> ParseAllTagsJson(string cachedAllTagsPath)
		{
			// read in the json file
			/*		{
						"full": "aa-Latn-ET",
						"name": "Afar",
						"names": [
							"Adal",
							...
						],
						"sldr": true,
						"tag": "aa",
						"tags": [
							"aa-ET",
							"aa-Latn"
						]
					},*/
			// for each entry
			// tag -> langtag which is same as sldrtag
			// sldr -> isAvailable
			// tags -> process to find implicitScript

			List<AllTagEntry> rootObject = null;
			try
			{
				if (cachedAllTagsPath != null && File.Exists(cachedAllTagsPath))
				{
					rootObject = JsonConvert.DeserializeObject<List<AllTagEntry>>(File.ReadAllText(cachedAllTagsPath));
					return DeriveTagsFromJsonEntries(rootObject);
				}
			}
			// If the user somehow got their SLDR locked against access, what can we do but attempt to go on!?
			catch (UnauthorizedAccessException ex)
			{
				Debug.Fail($"Inaccessible SLDR cache at: {cachedAllTagsPath}{Environment.NewLine}{ex.Message}");
			}
			catch (IOException ex)
			{
				Debug.Fail($"Error reading SLDR cache at: {cachedAllTagsPath}{Environment.NewLine}{ex.Message}");
			}
			catch (JsonReaderException)
			{
				SaveBadFile(cachedAllTagsPath);
				Debug.Fail($"Could not parse cached json file: {cachedAllTagsPath}{Environment.NewLine}Did the format change?");
			}
			catch (Exception ex)
			{
				SaveBadFile(cachedAllTagsPath);
				Debug.Fail($"Invalid data in cached json file {cachedAllTagsPath}{Environment.NewLine}{ex.Message}");
			}
			// Either we couldn't get the data from the network or it came back invalid.
			rootObject = JsonConvert.DeserializeObject<List<AllTagEntry>>(LanguageRegistryResources.langTags);
			return DeriveTagsFromJsonEntries(rootObject);
		}

		private static void SaveBadFile(string filepath)
		{
			var savedBadFile = filepath + "-BAD";
			if (RobustFile.Exists(savedBadFile))
				RobustFile.Delete(savedBadFile);
			RobustFile.Move(filepath, savedBadFile);
		}

		private static IKeyedCollection<string, SldrLanguageTagInfo> DeriveTagsFromJsonEntries(List<AllTagEntry> rootObject)
		{
			var tags = new KeyedList<string, SldrLanguageTagInfo>(info => info.LanguageTag, StringComparer.InvariantCultureIgnoreCase);

			foreach (var entry in rootObject)
			{
				// tags starting with x- have undefined structure so ignoring them
				// tags starting with _ showed up in buggy data so we'll drop them also
				if (entry.tag.StartsWith("x-") || entry.tag.StartsWith("_"))
					continue;

				if (!entry.deprecated && !StandardSubtags.RegisteredLanguages.TryGet(entry.tag.Split('-')[0], out var languageTag))
				{
					if (entry.iso639_3 == null)
					{
						StandardSubtags.AddLanguage(entry.tag.Split('-')[0], entry.name, false, entry.tag.Split('-')[0]);
					}
					else if (!StandardSubtags.RegisteredLanguages.TryGet(entry.iso639_3, out languageTag))
					{
						StandardSubtags.AddLanguage(entry.iso639_3, entry.name, false, entry.iso639_3);
					}
				}
				string implicitStringCode = null;

				// the script is always in the full tag
				var scriptCode = entry.full.Split('-')[1];
				if (scriptCode.Length == 4)
				{
					var tagComponents = entry.tag.Split('-');

					if (!StandardSubtags.RegisteredScripts.TryGet(scriptCode, out var scriptTag))
					{
						StandardSubtags.AddScript(scriptCode, scriptCode);
					}

					// if the script is also in the tag then it is explicit not implicit
					if (tagComponents.Length == 1 || tagComponents[1] != scriptCode)
					{
						implicitStringCode = scriptCode;
					}
				}
				tags.Add(new SldrLanguageTagInfo(entry.tag, implicitStringCode, entry.tag, entry.sldr));
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
			revid = string.Empty;
			uid = string.Empty;

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			if (!File.Exists(filePath))
				return false;
			var element = XElement.Load(filePath);
			if (element.Name != "ldml")
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");

			var identityElem = element.Element("identity");

			var specialElem = identityElem?.NonAltElement("special");
			var silIdentityElem = specialElem?.Element(Sil + "identity");
			if (silIdentityElem == null)
				return true;

			revid = (string) silIdentityElem.Attribute("revid") ?? string.Empty;
			uid = (string) silIdentityElem.Attribute("uid") ?? string.Empty;
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
			var element = XElement.Load(filePath);
			if (element.Name != "ldml")
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");

			var uid = string.Empty;
			var sldrCacheFilePath = filePath.Replace("." + TmpExtension, "");

			var identityElem = element.Element("identity");
			var specialElem = identityElem?.NonAltElement("special");
			var silIdentityElem = specialElem?.Element(Sil + "identity");
			if (silIdentityElem != null)
			{
				if ((string) silIdentityElem.Attribute("draft") == "approved")
				{
					// Remove uid attribute
					uid = string.Empty;
					silIdentityElem.SetOptionalAttributeValue("uid", uid);

					// Clean out original LDML file that contains uid in cache
					var originalFile = string.Empty;
					if (!string.IsNullOrEmpty(originalUid) && (originalUid != DefaultUserId))
						originalFile = sldrCacheFilePath.Replace("." + LdmlExtension, "-" + originalUid + "." + LdmlExtension);
					if (File.Exists(originalFile))
						File.Delete(originalFile);
				}
				else
					uid = (string) silIdentityElem.Attribute("uid");
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

			var di = Directory.CreateDirectory(SldrCachePath);
			if (Platform.IsUnix || SldrCachePath.StartsWith(Path.GetTempPath()))
				return;

			// NOTE: GetAccessControl/ModifyAccessRule/SetAccessControl is not implemented in Mono
			var ds = di.GetAccessControl();
			var sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
			AccessRule rule = new FileSystemAccessRule(sid, FileSystemRights.Write | FileSystemRights.ReadAndExecute
				| FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly, AccessControlType.Allow);
			ds.ModifyAccessRule(AccessControlModification.Add, rule, out var modified);
			di.SetAccessControl(ds);
		}
	}
}
