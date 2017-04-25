using System;
using System.Collections.Generic;
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

		public ImageCollection()
		{
			_wordToPartialPathIndex = new Dictionary<string, List<string>>();
			_partialPathToWordsIndex = new Dictionary<string, string>();
			SearchLanguage = "en";	// until told otherwise...
		}

		/// <summary>
		/// This is actually the default root image path used by LoadMultilingualIndex when not
		/// explicitly passed a rootImagePath argument. This effectively means it is the root
		/// image path for the original AOR collection, not for any of the others. Note that it's
		/// the path to the (typically) Images directory; the index file is expected to be in
		/// the parent of this directory, unless both are passed explicitly (a case not tested
		/// much if at all).
		/// Would be nice to rename to something like DefaultAorRootImagePath, but I don't want
		/// to change the public API of a library class since it's hard to be sure of tracking down
		/// all the clients that might be affected.
		/// </summary>
		public string RootImagePath { get; set; }

		public IEnumerable<string> AdditionalCollectionPaths { get; set; }

		/// <summary>
		/// Load an old-style index. Only applicable to old versions of AOR.
		/// Not used for AdditionalCollectionPaths.
		/// </summary>
		/// <param name="indexFilePath"></param>
		public void LoadIndex(string indexFilePath)
		{
			if (RootImagePath == null)
				RootImagePath = Path.GetDirectoryName(indexFilePath).CombineForPath(ImageFolder);
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
			Debug.WriteLine($"starting to load index for {pathToIndexFile} at {DateTime.Now.ToString("o")}");
			if (rootImagePath == null)
				rootImagePath = RootImagePath;
			string filenamePrefix = null;
			const string defaultLang = "en";
			// prefix we will stick on partial paths to indicate which folder they come from.
			// More space-efficient that just storing the full path.
			// GetPathsFromResults must remain consistent with this.
			var pathPrefix = "";
			if (rootImagePath != RootImagePath)
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
				Debug.WriteLine($"finished loading index for {pathToIndexFile} at {DateTime.Now.ToString("o")}");
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
				var partialPath = path.Replace(RootImagePath, "");
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
				string rootPath = RootImagePath;
				string internalPath = rawInternalPath;
				if (internalPath.StartsWith(":"))
				{
					var secondColonIndex = internalPath.IndexOf(":", 1, StringComparison.InvariantCulture);
					var additionalFileIndex = Int32.Parse(internalPath.Substring(1, secondColonIndex - 1));
					rootPath = Path.Combine(AdditionalCollectionPaths.Skip(additionalFileIndex).First(), ImageFolder);
					internalPath = internalPath.Substring(secondColonIndex + 1);
				}
				var path = Path.Combine(rootPath, internalPath.Replace(':', Path.DirectorySeparatorChar));

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
						pictures.AddRange(picturesForThisKey);
						foundExactMatches = true;
					}
					//then look  for approximate matches
					else
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
								pictures.AddRange(keyValuePair.Value);
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
			if (!Directory.Exists(rootPath))
				return new string[0];
			return Directory.GetDirectories(rootPath).Where(x=>TryForPathToMultilingualIndex(x) != null);
		}

		public static string TryForPathToMultilingualIndex(string directory)
		{
			if (String.IsNullOrEmpty(directory) || !Directory.Exists(directory))
				return null;
			return Directory.EnumerateFiles(directory, "*Index.txt").FirstOrDefault();
		}

		public static string TryToGetRootImageCatalogPath()
		{
			var distributedWithApp = FileLocator.GetDirectoryDistributedWithApplication(true,"Art Of Reading", ImageFolder);
			if(!string.IsNullOrEmpty(distributedWithApp) && Directory.Exists(distributedWithApp))
				return distributedWithApp;

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
				//look for the folder created by the ArtOfReadingFree installer
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

			// This seems reasonable but the implication is that if any 'additional' image collections
			// have been installed, ImageChooser won't report that AOR is missing.
			if (path == null && !additionalPaths.Any())
				return null;

			var c = new ImageCollection();

			c.RootImagePath = path;
			c.GetIndexLanguages();
			c.SearchLanguage = lang;
			c.AdditionalCollectionPaths = additionalPaths;

			// Load the index information asynchronously so as not to delay displaying
			// the parent dialog.  Loading the file takes a second or two, but should
			// be done before the user finishes typing a search string.
			var thr = new Thread(c.LoadImageIndex);
			thr.Name = "LoadArtOfReadingIndex";
			thr.Start();

			return c;
		}

		internal void GetIndexLanguages()
		{
			_indexLanguages = null;
			var pathToIndexFile = TryToGetPathToMultilingualIndex(RootImagePath);
			if (File.Exists(pathToIndexFile))
			{
				using (var f = File.OpenText(pathToIndexFile))
				{
					var columns = GetColumnHeadersIfValid(f);
					if (columns != null)
					{
						_indexLanguages = new List<string>();
						// The first four columns are meta data about an image.  The remaining
						// columns are search words in different languages, one language per column.
						// The header contains the ISO language codes.
						for (int i = 4; i < columns.Length; ++i)
							_indexLanguages.Add(columns[i]);
					}
					f.Close();
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
			string pathToIndexFile = TryToGetPathToMultilingualIndex(RootImagePath);
			if (!String.IsNullOrEmpty(pathToIndexFile))
			{
				countMulti = LoadMultilingualIndex(pathToIndexFile, RootImagePath);
			}
			if (countMulti == 0)
			{
				pathToIndexFile = TryToGetPathToIndex(RootImagePath);
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


		public void ReloadImageIndex(string languageId)
		{
			SearchLanguage = languageId;
			_wordToPartialPathIndex.Clear();
			_partialPathToWordsIndex.Clear();
			LoadImageIndex();
		}
	}
}
