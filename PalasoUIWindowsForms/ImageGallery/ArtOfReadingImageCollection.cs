using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Palaso.Extensions;
using Palaso.IO;
using Palaso.Linq;
using Palaso.Text;

namespace Palaso.UI.WindowsForms.ImageGallery
{
	public class ArtOfReadingImageCollection :IImageCollection
	{
		private Dictionary<string, List<string>> _wordToPartialPathIndex;
		private Dictionary<string, string> _partialPathToWordsIndex;
		private static readonly object _padlock = new object();
		public string SearchLanguage { get; set; }
		private List<string> _indexLanguages;
		public IEnumerable<string> IndexLanguageIds { get { return _indexLanguages; } }

		public ArtOfReadingImageCollection()
		{
			_wordToPartialPathIndex = new Dictionary<string, List<string>>();
			_partialPathToWordsIndex = new Dictionary<string, string>();
			SearchLanguage = "en";	// until told otherwise...
		}

		public string RootImagePath { get; set; }

		public void LoadIndex(string indexFilePath)
		{
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
		/// Load the multilingual index of the images.
		/// </summary>
		/// <returns>number of index lines successfully loaded</returns>
		public int LoadMultilingualIndex(string pathToIndexFile)
		{
			const string defaultLang = "en";
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
					var partialPath = String.Format("{0}:AOR_{1}.png", parts[3], parts[1]);
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

		public IEnumerable<object> GetMatchingPictures(string keywords, out bool foundExactMatches)
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


		public IEnumerable<string> GetPathsFromResults(IEnumerable<object> results, bool limitToThoseActuallyAvailable)
		{
			foreach (var macPath in results)
			{
				var path = Path.Combine(RootImagePath, ((string) macPath).Replace(':', Path.DirectorySeparatorChar));
				if (!limitToThoseActuallyAvailable)
				{
					yield return path;
					continue; // don't look further
				}

				if (File.Exists(path))
				{
					yield return path;
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
		}

		private IEnumerable<object> GetMatchingPictures(IEnumerable<string> keywords, out bool foundExactMatches)
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
			var results = new List<object>();
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

		public static string TryToGetRootImageCatalogPath()
		{
			var distributedWithApp = FileLocator.GetDirectoryDistributedWithApplication(true,"Art Of Reading", "images");
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
				var aorInstallerTarget = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).CombineForPath("SIL", "Art Of Reading", "images");

				//the rest of these are for before we had an installer for AOR
				var appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).CombineForPath("Art Of Reading", "images");
				var appDataNoSpace = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).CombineForPath("ArtOfReading", "images");
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

			if (path == null)
				return null;

			var c = new ArtOfReadingImageCollection();

			c.RootImagePath = path;
			c.GetIndexLanguages();
			c.SearchLanguage = lang;

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
			_indexLanguages = new List<string>();
			var pathToIndexFile = TryToGetPathToMultilingualIndex(RootImagePath);
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

		void LoadImageIndex()
		{
			int countMulti = 0;
			string pathToIndexFile = TryToGetPathToMultilingualIndex(RootImagePath);
			if (!String.IsNullOrEmpty(pathToIndexFile))
			{
				countMulti = LoadMultilingualIndex(pathToIndexFile);
			}
			if (countMulti == 0)
			{
				pathToIndexFile = TryToGetPathToIndex(RootImagePath);
				LoadIndex(pathToIndexFile);
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
