using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using SIL.Extensions;
using SIL.IO;
using SIL.Linq;
using SIL.Text;

namespace SIL.Windows.Forms.ImageGallery
{

	/// <summary>
	/// Originally a wrapper for the Art of Reading image collection, it may now include others.
	/// By default (static method FromstandardLocations()) it loads AOR from any of the places
	/// we know of that installers might put it, and any additional collections found at
	/// %programdata%/SIL/ImageCollections
	/// 
	/// An image collection is a folder plus a master index file.
	/// The root folder contains the master index file. It should be the only file with a name ending in
	/// Index.txt. There could be a more descriptive name before this, e.g., ArtOfReadingMultilingualIndex.txt,
	/// but this is not required...a simple Index.txt will do fine.
	/// The rest of the collection is a folder called Images with a folder for each source country.
	/// (This class of course does not  care whether the folders are really named for countries or
	/// really contain pictures appropriate to that country, but that's how Art of Reading is organized
	/// and the file/folder structure follows that.)
	/// Each folder contains image files.
	/// The master index file contains columnar data, with the columns separated by tabs in each line.
	/// It starts with a heading row indicating that the columns contain order, filename, artist, country,
	/// and then lists of keywords in various languages. The header indicates which languages,
	/// for example the AOR index contains (tab-separated) en	id	fr	es	ar	hi	bn	pt	th	sw	zh.
	/// Each single-language list of keywords is comma-separated.
	/// To locate the actual image file corresponding to a line of the index,
	/// - use the country field as a folder in the Images folder of the collection.
	/// - look for a file whose name (without extension) ends in the filename from the line.
	/// (There is typically a prefix, for example, the line that starts 5	CMB0012		Cambodia
	/// in the AOR index corresponds to a file called Cambodia/AOR_CMB0012.png.)
	/// If such prefixes are used they must be consistent across the entire collection.
	/// Otherwise, use full file names in the index.
	/// (Currently, the class figures out the prefix by looking for the first file specified
	/// in the index. For this reason, the first file must actually exist.)
	/// </summary>
	public class ImageCollection : IImageCollection
	{
		internal const string ImageFolder = "Images";
		private Dictionary<string, List<string>> _wordToPartialPathIndex;
		private Dictionary<string, string> _partialPathToWordsIndex;
		private static readonly object _padlock = new object();
		public string SearchLanguage { get; set; }
		private List<string> _indexLanguages;
		public IEnumerable<string> IndexLanguageIds { get { return _indexLanguages; } }
		private Dictionary<string, bool> _enabledCollections = new Dictionary<string, bool>();

		public ImageCollection()
		{
			_wordToPartialPathIndex = new Dictionary<string, List<string>>();
			_partialPathToWordsIndex = new Dictionary<string, string>();
			SearchLanguage = "en";	// until told otherwise...
		}

		public IEnumerable<string> Collections
		{
			get
			{
				var result = _enabledCollections.Keys.ToList();
				result.Sort((x, y) => Path.GetDirectoryName(x).ToLowerInvariant()
						.CompareTo(Path.GetDirectoryName(y).ToLowerInvariant()));
				return result;
			}
		}

		/// <summary>
		/// This is the default root image path used by LoadMultilingualIndex when not
		/// explicitly passed a rootImagePath argument. This  means it is the root
		/// image path for the original AOR collection, not for any of the others. Note that it's
		/// the path to the (typically) Images directory; the index file is expected to be in
		/// the parent of this directory, unless both are passed explicitly (a case not tested
		/// much if at all).
		/// </summary>
		public string DefaultAorRootImagePath { get; set; }

		public IEnumerable<string> AdditionalCollectionPaths { get; set; }

		internal static bool AllowCollectionWithNoImageFolderForTesting;

		/// <summary>
		/// Load an old-style index. Only applicable to old versions of AOR.
		/// Not used for AdditionalCollectionPaths.
		/// </summary>
		/// <param name="indexFilePath"></param>
		public void LoadIndex(string indexFilePath)
		{
			if (DefaultAorRootImagePath == null)
			{
				DefaultAorRootImagePath = Path.GetDirectoryName(indexFilePath).CombineForPath(ImageFolder);
			}
			if (!Directory.Exists(DefaultAorRootImagePath) && !AllowCollectionWithNoImageFolderForTesting)
				return; // spurious index of some sort, no images.
			RestoreEnabledStateOfCollection(Path.GetDirectoryName(DefaultAorRootImagePath));
			using (var f = File.OpenText(indexFilePath))
			{
				while (!f.EndOfStream)
				{
					var line = f.ReadLine();
					var parts = line.Split(new char[] { '\t' });
					Debug.Assert(parts.Length == 2);
					if (parts.Length != 2)
						continue;
					var partialPath = parts[0];
					var keyString = parts[1].Trim(new char[] { ' ', '"' });//some have quotes, some don't
					lock (_padlock)
						_partialPathToWordsIndex.Add(partialPath, keyString);
					var keys = keyString.SplitTrimmed(',');
					foreach (var key in keys)
					{
						lock (_padlock)
							_wordToPartialPathIndex.GetOrCreate(key).Add(partialPath);
					}
				}
			}
		}

		/// <summary>
		/// This one assumes a standard organization
		/// - images in rootFolder/Images
		/// - index in rootFolder, first file ending in MultilingualIndex.txt
		/// </summary>
		/// <param name="rootFolder"></param>
		internal void LoadMultilingualIndexAtRoot(string rootFolder)
		{
			var pathToIndexFile = TryForPathToMultilingualIndex(rootFolder);
			var rootImagePath = Path.Combine(rootFolder, ImageFolder);
			LoadMultilingualIndex(pathToIndexFile, rootImagePath);
		}

		/// <summary>
		/// Load the multilingual index of the images.
		/// </summary>
		/// <returns>number of index lines successfully loaded</returns>
		public int LoadMultilingualIndex(string pathToIndexFile, string rootImagePath = null)
		{
			if (rootImagePath == null)
				rootImagePath = DefaultAorRootImagePath;
			if (!Directory.Exists(rootImagePath))
				return 0; // spurious index of some sort, no images.
			RestoreEnabledStateOfCollection(Path.GetDirectoryName(pathToIndexFile));
			string filenamePrefix = null;
			const string defaultLang = "en";
			// prefix we will stick on partial paths to indicate which folder they come from.
			// More space-efficient that just storing the full path.
			// GetPathsFromResults must remain consistent with this.
			var pathPrefix = "";
			if (rootImagePath != DefaultAorRootImagePath)
			{
				pathPrefix = ":" + AdditionalCollectionPaths.IndexOf(Path.GetDirectoryName(rootImagePath)) + ":";
			}
			using (var f = File.OpenText(pathToIndexFile))
			{
				var columns = GetColumnHeadersIfValid(f);
				if (columns == null)
					return 0;
				int desiredColumn = -1;
				int defaultColumn = -1;
				// The first four columns are fixed metadata.  The remaining columns contain
				// search words for different languages, one language per column.  The header
				// contains the ISO language codes.
				for (int i = 4; i < columns.Length; ++i)
				{
					if (columns[i] == SearchLanguage)
						desiredColumn = i;
					if (columns[i] == defaultLang)
						defaultColumn = i;
				}
				if (desiredColumn == -1)
					desiredColumn = defaultColumn;
				if (defaultColumn == -1)
					return 0;		// we must have English!!?
				int count = 0;
				while (!f.EndOfStream)
				{
					var line = f.ReadLine();
					var parts = line.Split(new char[]{'\t'});
					Debug.Assert(parts.Length > defaultColumn);
					if (parts.Length <= defaultColumn)
						continue;
					var partialPath = GetPartialPath(rootImagePath, parts[3], parts[1], ref filenamePrefix, pathPrefix);
					string keyString;
					if (parts.Length > desiredColumn)
						keyString = parts[desiredColumn];
					else if (parts.Length > defaultColumn)
						keyString = parts[defaultColumn];
					else
						keyString = String.Empty;
					if (String.IsNullOrWhiteSpace (keyString))
						continue;
					lock (_padlock)
						_partialPathToWordsIndex.Add(partialPath, keyString.Replace(",", ", "));
					var keys = keyString.Split(',');
					foreach (var key in keys)
					{
						lock (_padlock)
							_wordToPartialPathIndex.GetOrCreate(key).Add(partialPath);
					}
					++count;
				}
				return count;
			}
		}

		string GetPartialPath(string rootImagePath, string folder, string filename, ref string filenamePrefix, string pathPrefix)
		{
			if (filenamePrefix == null)
			{
				var pattern = "*" + filename + ".*";
				var searchFolder = Path.Combine(rootImagePath, folder);
				var filePath = Directory.EnumerateFiles(searchFolder, pattern).FirstOrDefault();
				if (filePath == null)
				{
					Debug.Assert(filePath != null, "Could not find expected file in collection to determine prefix");
					return "";
				}
				// We should have gotten something like .../Cambodia/AOR_filename.png
				var realFileName = Path.GetFileNameWithoutExtension(filePath);
				filenamePrefix = realFileName.Substring(0, realFileName.Length - filename.Length);
			}
			return String.Format("{3}{0}:{1}{2}.png", folder, filenamePrefix, filename, pathPrefix);
		}

		public IEnumerable<string> GetMatchingPictures(string keywords, out bool foundExactMatches)
		{
			keywords = GetCleanedUpSearchString(keywords.ToLowerInvariant());
			return GetMatchingPictures(keywords.SplitTrimmed(' '), out foundExactMatches);
		}

		public string GetCleanedUpSearchString(string keywords)
		{
			keywords = keywords.Replace(",", string.Empty);
			keywords = keywords.Replace(";", string.Empty);
			keywords = keywords.Replace("-", string.Empty);
			keywords = keywords.Replace(".", string.Empty);
			keywords = keywords.Replace("(", string.Empty);
			return  keywords.Replace(")", string.Empty);
		}

		public Image GetThumbNail(object imageToken)
		{
			throw new System.NotImplementedException();
		}

		public CaptionMethodDelegate CaptionMethod
		{
			get { return GetCaption; }
		}

		private string GetCaption(string path)
		{
			try
			{
				var partialPath = path.Replace(DefaultAorRootImagePath, "");
				partialPath = partialPath.Replace(Path.DirectorySeparatorChar, ':');
				partialPath = partialPath.Trim(new char[] {':'});
				lock (_padlock)
					return _partialPathToWordsIndex[partialPath];
			}
			catch (Exception)
			{
				return "error";
			}
		}

		/// <summary>
		/// Given a set of internal paths from GetMatchingPictures, return the actual paths to the picture files.
		/// The actual picture files may be .png or .jpg.
		/// </summary>
		/// <param name="internalPaths">Some of the partial paths (typically obtained from GetMatchingPictures())
		/// that one of the LoadIndex methods put into _wordToPartialPathIndex. As such they are
		/// basically country:filename pairs, or :additionalPathIndex:country:filename if from an additional path
		/// rather than the main AOR collection.</param>
		/// <param name="limitToThoseActuallyAvailable"></param>
		/// <returns></returns>
		public IEnumerable<string> GetPathsFromResults(IEnumerable<string> internalPaths, bool limitToThoseActuallyAvailable)
		{
			foreach (string rawInternalPath in internalPaths)
			{
				// internal paths have colons as folder separators and other special properties as noted above.
				var path = GetFileSystemPathFromInternalPath(rawInternalPath);

				if (File.Exists(path))
				{
					yield return path;
					continue; // don't look further
				}

				// User collections might have jpgs instead
				var jpgPath = Path.ChangeExtension(path, "jpg");
				if (File.Exists(jpgPath))
				{
					yield return jpgPath;
					continue; // don't look further
				}

				// These results may be from an older index that has the original file names, and were tifs.
				// The re-republished versions (which have embedd3ed metadata and watermarks) start with AOR_ and end with png
				var updatedPath =
					Path.GetDirectoryName(path).CombineForPath("AOR_" + Path.GetFileName(path).Replace(".tif", ".png"));

				if (File.Exists(updatedPath))
				{
					yield return updatedPath;
					continue; // don't look further
				}

				// In version 3, some country's files (Mexico only?) were split into two subdirectories.
				// Instead of republishing the index and making everybody reinstall AOR,
				// we'll see if we can find the file in a subdirectory.
				var parentDir = Path.GetDirectoryName(path);
				var fileName = Path.GetFileName(path);
				if (Directory.Exists(parentDir))
				{
					var subDirs = Directory.EnumerateDirectories(parentDir);
					foreach (var subDir in subDirs)
					{
						updatedPath = Path.Combine(parentDir, subDir, fileName);
						if (File.Exists(updatedPath))
						{
							yield return updatedPath;
							continue;
						}
					}
				}

				// Tried everything, but we're not limiting things so return the original name anyway.
				// Note: by doing this at the end we're losing any performance benefit of not limiting things.
				// But, it feels really wrong to return a name ending in .png when it isn't there and there
				// IS a jpg. In any case, I don't know of any code that actually passes false, so it probably
				// doesn't matter.
				if (!limitToThoseActuallyAvailable)
				{
					yield return path;
				}
			}
		}

		private string GetFileSystemPathFromInternalPath(string rawInternalPath)
		{
			string relativePath;
			var rootPath = ImageFolderForInternalPath(rawInternalPath, out relativePath);
			return Path.Combine(rootPath, relativePath);
		}

		private string ImageFolderForInternalPath(string rawInternalPath)
		{
			string relativePath;
			return ImageFolderForInternalPath(rawInternalPath, out relativePath);
		}

		// Use one of the methods above if you just want the image folder or want to combine the parts and find the file.
		private string ImageFolderForInternalPath(string rawInternalPath, out string relativePath)
		{
			string rootPath = DefaultAorRootImagePath;
			relativePath = rawInternalPath;
			if (relativePath.StartsWith(":"))
			{
				var secondColonIndex = relativePath.IndexOf(":", 1, StringComparison.InvariantCulture);
				var additionalFileIndex = Int32.Parse(relativePath.Substring(1, secondColonIndex - 1));
				rootPath = Path.Combine(AdditionalCollectionPaths.Skip(additionalFileIndex).First(),
					ImageFolder);
				relativePath = relativePath.Substring(secondColonIndex + 1);
			}
			relativePath = relativePath.Replace(':', Path.DirectorySeparatorChar);
			return rootPath;
		}

		private IEnumerable<string> GetMatchingPictures(IEnumerable<string> keywords, out bool foundExactMatches)
		{
			var pictures = new List<string>();
			foundExactMatches = false;
			foreach (var term in keywords)
			{
				List<string> picturesForThisKey;

				//first, try for exact matches
				lock (_padlock)
				{
					if (_wordToPartialPathIndex.TryGetValue(term, out picturesForThisKey))
					{
						foreach (var path in picturesForThisKey)
						{
							var imageFolder = ImageFolderForInternalPath(path);
							if (_enabledCollections[Path.GetDirectoryName(imageFolder)])
							{
								pictures.Add(path);
								foundExactMatches = true;
							}
						}
					}
					//then look  for approximate matches
					if (!foundExactMatches)
					{
						foundExactMatches = false;
						var kMaxEditDistance = 1;
						var itemFormExtractor = new ApproximateMatcher.GetStringDelegate<KeyValuePair<string, List<string>>>(pair => pair.Key);
						var matches = ApproximateMatcher.FindClosestForms<KeyValuePair<string, List<string>>>(_wordToPartialPathIndex, itemFormExtractor,
							             term,
							             ApproximateMatcherOptions.None,
							             kMaxEditDistance);

						if (matches != null && matches.Count > 0)
						{
							foreach (var keyValuePair in matches)
							{
								foreach (var path in keyValuePair.Value)
								{
									var imageFolder = ImageFolderForInternalPath(path);
									if (_enabledCollections[Path.GetDirectoryName(imageFolder)])
									{
										pictures.Add(path);
									}
								}
							}
						}
					}
				}
			}
			var results = new List<string>();
			pictures.Distinct().ForEach(p => results.Add(p));
			return results;
		}


		public string StripNonMatchingKeywords(string stringWhichMayContainKeywords)
		{
			string result = string.Empty;
			stringWhichMayContainKeywords = GetCleanedUpSearchString(stringWhichMayContainKeywords);
			var words = stringWhichMayContainKeywords.SplitTrimmed(' ');
			foreach (var key in words)
			{
				lock (_padlock)
				{
					if (_wordToPartialPathIndex.ContainsKey(key))
						result += " " + key;
				}
			}
			return result.Trim();
		}

		public static bool IsAvailable()
		{
			string imagesPath = TryToGetRootImageCatalogPath();
			return !string.IsNullOrEmpty(imagesPath) && !string.IsNullOrEmpty(TryToGetPathToIndex(imagesPath));
		}

		// Currently only used for testing.
		internal static string StandardAdditionalDirectoriesRoot { get; set; }

		internal static IEnumerable<string> GetStandardAdditionalDirectories()
		{
			string rootPath = GetImageGalleryRoot();
			if (!Directory.Exists(rootPath))
				return new string[0];
			// Even if Art Of Reading is installed in this directory, it's not additional.
			// This is a bit awkward...if we didn't want to deal with old versions of AOR we could just make this
			// a list of all directories, one of which might be AOR.
			// But we do in fact have various special cases for finding AOR, yet we also need to cope
			// with eventually finding it in this directory. I _think_ it is simplest to allow the
			// code that looks specifically for AOR to find it here and therefore NOT to allow
			// it to count as additional.
			return Directory.GetDirectories(rootPath).Where(x=>TryForPathToMultilingualIndex(x) != null && Path.GetFileName(x) != "Art Of Reading");
		}

		private static string GetImageGalleryRoot()
		{
			string rootPath;
			if (StandardAdditionalDirectoriesRoot != null)
				rootPath = StandardAdditionalDirectoriesRoot;
			else
			{
				// Windows: typically C:\ProgramData\SIL\ImageCollections.
				// Linux: typically /usr/share/SIL/ImageCollections
				// (This is not the typical place for a Linux package to install things
				// and CamelCase is not a standard way to name folders.
				// Typically each package would make its own folder at the root of /user/share.
				// Then something like sil-image-collection might plausibly be part of each
				// folder name.
				// But that will require a whole different strategy for finding them, possibly
				// something like an environment variable, or we could search the whole
				// of /user/share for folders starting with sil-image-collection. Let's wait and see whether anyone
				// actually wants to do this and doesn't find the current proposal satisfactory.)
				rootPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
					.CombineForPath("SIL")
					.CombineForPath("ImageCollections");
			}
			return rootPath;
		}

		public static string TryForPathToMultilingualIndex(string directory)
		{
			if (String.IsNullOrEmpty(directory) || !Directory.Exists(directory))
				return null;
			// It's somewhat arbitrary what order we use here, but making it alphabetical makes it more
			// predictable, since the order returned by EnumerateFiles is not guaranteed.
			// On thing that is important is that in versions of AOR that have both
			// ArtOfReadingMultilingualIndex.txt and index.txt, the former must be found first.
			return Directory.EnumerateFiles(directory, "*ndex.txt")
				.Where(x => x.ToLowerInvariant().EndsWith("index.txt")) // no way to enumerate [iI]ndex.txt on Linux, and we need both
				.OrderBy(x => x.ToLowerInvariant())
				.FirstOrDefault();
		}

		public static string TryToGetRootImageCatalogPath()
		{
			var distributedWithApp = FileLocator.GetDirectoryDistributedWithApplication(true,"Art Of Reading", ImageFolder);
			if(!string.IsNullOrEmpty(distributedWithApp) && Directory.Exists(distributedWithApp))
				return distributedWithApp;

			// Look for it installed alongside other image galleries in the new standard location.
			// We intend the next version of AOR (3.3 and later) to put itself there.
			var galleryRoot = GetImageGalleryRoot();
			if (Directory.Exists(galleryRoot))
			{
				var possiblePath = Path.Combine(galleryRoot, "Art Of Reading", ImageFolder);
				if (Directory.Exists(possiblePath))
					return possiblePath;
			}

			//look for it in a hard-coded location
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				var unixPaths = new[]
				{
					@"/usr/share/ArtOfReading/images",		// new package location (standard LSB/FHS location)
					@"/usr/share/SIL/ArtOfReading/images",	// old (lost) package location
					@"/var/share/ArtOfReading/images"		// obsolete test location (?)
				};

				foreach (var path in unixPaths)
				{
					if (Directory.Exists(path))
						return path;
				}
			}
			else
			{
				//look for the folder created by the ArtOfReadingFree installer before 3.3, the first version to put it in the ImageCollections folder.
				var aorInstallerTarget = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).CombineForPath("SIL", "Art Of Reading", ImageFolder);

				//the rest of these are for before we had an installer for AOR
				var appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).CombineForPath("Art Of Reading", ImageFolder);
				var appDataNoSpace = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).CombineForPath("ArtOfReading", ImageFolder);
				var winPaths = new[] { aorInstallerTarget,  @"c:\art of reading\images", @"c:/ArtOfReading/images", appData, appDataNoSpace };

				foreach (var path in winPaths)
				{
					if (Directory.Exists(path))
						return path;
				}
			}

			return null;
		}

		public static bool DoNotFindArtOfReading_Test = false;

		public static IImageCollection FromStandardLocations()
		{
			return FromStandardLocations("en");
		}

		public static IImageCollection FromStandardLocations(string lang)
		{
			string path = TryToGetRootImageCatalogPath();
			if (DoNotFindArtOfReading_Test)
				path = null;

			var additionalPaths = GetStandardAdditionalDirectories();

			if (path == null && !additionalPaths.Any())
				return null;

			var c = new ImageCollection();

			c.DefaultAorRootImagePath = path;
			c.AdditionalCollectionPaths = additionalPaths;
			c.GetIndexLanguages();
			c.SearchLanguage = lang;

			// We would eventually add these paths to the enabled collection, and correctly
			// set their state, as we load each index. But since that's async and the image toolbox
			// may ask for the state of enabled collections immediately, we need to update that now.
			if (path != null)
				c.RestoreEnabledStateOfCollection(Path.GetDirectoryName(path));
			foreach (var p in additionalPaths)
				c.RestoreEnabledStateOfCollection(p);

			// Load the index information asynchronously so as not to delay displaying
			// the parent dialog.  Loading the file takes a second or two, but should
			// be done before the user finishes typing a search string.
			var thr = new Thread(c.LoadImageIndex);
			thr.Name = "LoadImageIndex";
			thr.Start();

			return c;
		}

		internal void GetIndexLanguages()
		{
			_indexLanguages = null;
			var pathToAorIndexFile = TryToGetPathToMultilingualIndex(DefaultAorRootImagePath);
			var roots = new List<string>();
			if (pathToAorIndexFile != null)
				roots.Add(pathToAorIndexFile);
			roots.AddRange(AdditionalCollectionPaths.Select(p => TryForPathToMultilingualIndex(p)).Where(s => s != null));
			_indexLanguages = new List<string>();
			foreach (var pathToIndexFile in roots)
			{
				if (File.Exists(pathToIndexFile))
				{
					using (var f = File.OpenText(pathToIndexFile))
					{
						var columns = GetColumnHeadersIfValid(f);
						if (columns != null)
						{
							// The first four columns are meta data about an image.  The remaining
							// columns are search words in different languages, one language per column.
							// The header contains the ISO language codes.
							for (int i = 4; i < columns.Length; ++i)
							{
								if (!IndexLanguageIds.Contains(columns[i]))
								_indexLanguages.Add(columns[i]);
							}
						}
						f.Close();
					}
				}
			}
		}

		/// <summary>
		/// Return the column headers from a multilingual index file, or null if it is not valid.
		/// </summary>
		private static string[] GetColumnHeadersIfValid(StreamReader f)
		{
			// The file should start with a line that looks like the following:
			//order	filename	artist	country	en	id	fr	es	ar	hi	bn	pt	th	sw	zh
			var header = f.ReadLine();
			if (String.IsNullOrEmpty(header))
				return null;
			var columns = header.Split(new[]{'\t'});
			// Check for the four fixed columns and at least one language.
			if (columns.Length < 5 ||
				columns[0] != "order" ||
				columns[1] != "filename" ||
				columns[2] != "artist" ||
				columns[3] != "country")
			{
				return null;
			}
			return columns;
		}

		internal void LoadImageIndex()
		{
			int countMulti = 0;
			// do NOT do this here. It runs in the background on initial load, and this could clear things
			// just between when FromStandardLocations initializes them and AORChooser uses them.
			//_enabledCollections.Clear();
			string pathToIndexFile = TryToGetPathToMultilingualIndex(DefaultAorRootImagePath);
			if (!String.IsNullOrEmpty(pathToIndexFile))
			{
				countMulti = LoadMultilingualIndex(pathToIndexFile, DefaultAorRootImagePath);
			}
			if (countMulti == 0)
			{
				pathToIndexFile = TryToGetPathToIndex(DefaultAorRootImagePath);
				LoadIndex(pathToIndexFile);
			}
			foreach (var path in AdditionalCollectionPaths)
			{
				LoadMultilingualIndexAtRoot(path);
			}
		}

		public static string TryToGetPathToMultilingualIndex(string imagesPath)
		{
			if (String.IsNullOrEmpty(imagesPath) || !Directory.Exists(imagesPath))
				return null;
			var path = Directory.GetParent(imagesPath).FullName.CombineForPath("ArtOfReadingMultilingualIndex.txt");
			if (File.Exists(path))
				return path;
			return null;
		}

		public static string TryToGetPathToIndex(string imagesPath)
		{
			// ArtOfReading3FreeSetp.exe installs index.txt in the parent directory 
			// of images.  The previous verison of this code looked in images directory
			// as well.
			if (!string.IsNullOrEmpty(imagesPath))
			{
				var path = Directory.GetParent(imagesPath).FullName.CombineForPath("index.txt");
				if (File.Exists(path))
					return path;

				path = imagesPath.CombineForPath("index.txt");
				if (File.Exists(path))
					return path;
			}

			return FileLocator.GetFileDistributedWithApplication(true, "ArtOfReadingIndexV3_en.txt");
		}

		void RestoreEnabledStateOfCollection(string path)
		{
			_enabledCollections[path] = true;
			var disabledSettings = Properties.Settings.Default.DisabledImageCollections;
			if (disabledSettings == null)
				disabledSettings = new StringCollection();
			if (disabledSettings.IndexOf(path) >= 0)
				_enabledCollections[path] = false;
		}


		public void ReloadImageIndex(string languageId)
		{
			SearchLanguage = languageId;
			_enabledCollections.Clear();
			_wordToPartialPathIndex.Clear();
			_partialPathToWordsIndex.Clear();
			LoadImageIndex();
		}

		public void EnableCollection(string path, bool enabled)
		{
			_enabledCollections[path] = enabled;
			var disabledSettings = Properties.Settings.Default.DisabledImageCollections;
			if (disabledSettings == null)
				Properties.Settings.Default.DisabledImageCollections = disabledSettings = new StringCollection();
			if (enabled && disabledSettings.Contains(path))
				disabledSettings.Remove(path);
			if (!enabled && !disabledSettings.Contains(path))
				disabledSettings.Add(path);
			Properties.Settings.Default.Save();
		}

		public bool IsCollectionEnabled(string path)
		{
			bool result;
			if (!_enabledCollections.TryGetValue(path, out result))
				return true; // default is to enable
			return result;
		}
	}
}
