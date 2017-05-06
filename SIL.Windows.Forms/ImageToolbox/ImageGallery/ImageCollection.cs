using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Code;
using SIL.Extensions;
using SIL.Linq;
using SIL.Text;

namespace SIL.Windows.Forms.ImageToolbox.ImageGallery
{
	/// <summary>
	/// A set of images on disk that have an index file with key words in one or more languages.
	/// For example, "Art of Reading" is one of these collections. 
	/// See https://github.com/sillsdev/image-collection-starter and https://github.com/sillsdev/ArtOfReading
	/// </summary>
	public class ImageCollection
    {
        public readonly string FolderPath;
        public IEnumerable<string> IndexLanguages => _imageIndexReader.LanguageIds;
		public bool Enabled = true;
		public string Key => FolderPath;

		internal const string kStandardImageFolderName = "images";
		private Dictionary<string, List<string>> _indexOfWordsToRelativePath;
        private Dictionary<string, string> _indexOfRelativeFilePathToKeywordsCsv;
        private static readonly object _padlock = new object();
        private ImageIndexReader _imageIndexReader;
		private Func<IEnumerable<string>, IEnumerable<string>> _fixPaths = p => p; // identity function, by default

		public ImageCollection(string path)
        {
            Require.That(Directory.Exists(path), "path must be a directory and must exist");
            FolderPath = path;
			LoadLanguageChoices();
		}
      
		/// <summary>
		/// Some sort of name for the collection
		/// </summary>
        public string Name => Path.GetFileName(FolderPath);

		/// <summary>
		/// Figure out in what languages we have keywords
		/// </summary>
        protected void LoadLanguageChoices()
        {
			var pathToIndexFile = FindIndexFileInFolder(FolderPath);
			_imageIndexReader = ImageIndexReader.FromFile(pathToIndexFile);
		}

		/// <summary>
		/// Note: you can call this more than once, in order to change the search language id
		/// </summary>
        public virtual void LoadIndex(string searchLanguageId)
        {
            if(_imageIndexReader == null)
            {
                LoadLanguageChoices();
            }

			_indexOfWordsToRelativePath = new Dictionary<string, List<string>>();
			_indexOfRelativeFilePathToKeywordsCsv = new Dictionary<string, string>();

			using (var f = File.OpenText(FindIndexFileInFolder(FolderPath)))
            {
				//skip header line, which was already read to make the index layout above
				f.ReadLine();
                while(!f.EndOfStream)
                {
                    var line = f.ReadLine();
					var fields = line.Split(new char[] {'\t'});
                    var relativePath = _imageIndexReader.GetImageRelativePath(fields);
                    var csvOfKeywords = _imageIndexReader.GetCSVOfKeywordsOrEmpty(searchLanguageId, fields);
                    if(String.IsNullOrWhiteSpace(csvOfKeywords))
                        continue;
                    lock(_padlock)
                        _indexOfRelativeFilePathToKeywordsCsv.Add(relativePath, csvOfKeywords.Replace(",", ", "));
                    var keys = csvOfKeywords.SplitTrimmed(',');
                    foreach(var key in keys)
                    {
                        lock(_padlock)
                            _indexOfWordsToRelativePath.GetOrCreate(key.ToLowerInvariant()).Add(relativePath);
                    }
                }
            }
        }

		/// <summary>
		/// Get images that exactly match one or more search terms.
		/// </summary>
        public IEnumerable<string> GetMatchingImages(string searchTermsCsv)
        {
            searchTermsCsv = ImageCollectionManager.GetCleanedUpSearchString(searchTermsCsv);
			return GetMatchingImages(searchTermsCsv.SplitTrimmed(' '));
		}

		/// <summary>
		/// Get images that exactly match one or more search terms.
		/// </summary>
		public IEnumerable<string> GetMatchingImages(IEnumerable<string> searchTerms)
		{
			var fullPathsToImages = new List<string>();
			foreach (var term in searchTerms)
			{
				//try for exact matches
				lock (_padlock)
				{
					List<string> imagesForThisSearchTerm;
					if (_indexOfWordsToRelativePath.TryGetValue(term.ToLowerInvariant(), out imagesForThisSearchTerm))
					{
					    fullPathsToImages.AddRange(imagesForThisSearchTerm.Select(GetFullPath));
					}
				}
			}
			var results = new List<string>();
			fullPathsToImages.Distinct().ForEach(p => results.Add(p));
		    return _fixPaths(results);
		}

		/// <summary>
		/// Get images that are within one "edit" of matching an image in the collection
		/// </summary>
		public IEnumerable<string> GetApproximateMatchingImages(IEnumerable<string> searchTerms)
		{
			var fullPathsToImages = new List<string>();
		    foreach(var term in searchTerms)
		    {
		        lock(_padlock)
		        {
		            const int kMaxEditDistance = 1;
		            var itemFormExtractor =
		                new ApproximateMatcher.GetStringDelegate<KeyValuePair<string, List<string>>>(pair => pair.Key);
		            var matches =
		                ApproximateMatcher.FindClosestForms<KeyValuePair<string, List<string>>>(_indexOfWordsToRelativePath,
		                    itemFormExtractor,
		                    term.ToLowerInvariant(),
		                    ApproximateMatcherOptions.None,
		                    kMaxEditDistance);

		            if(matches != null && matches.Count > 0)
		            {
		                foreach(var keyValuePair in matches)
		                {
		                    fullPathsToImages.AddRange(keyValuePair.Value.Select(GetFullPath));
		                }
		            }
		        }
		    }
		    var results = new List<string>();
			fullPathsToImages.Distinct().ForEach(p => results.Add(p));
			return _fixPaths(results);
		}


		public string GetFullPath(string partialPath)
		{
			return Path.Combine( this.FolderPath, ImageCollection.kStandardImageFolderName, partialPath);
		}

		/*		public string StripNonMatchingKeywords(string stringWhichMayContainKeywords)
				{
					string result = string.Empty;
					stringWhichMayContainKeywords = GetCleanedUpSearchString(stringWhichMayContainKeywords);
					var words = stringWhichMayContainKeywords.SplitTrimmed(' ');
					foreach (var key in words)
					{
						lock (_padlock)
						{
							if (_indexOfWordsToRelativePath.ContainsKey(key))
								result += " " + key;
						}
					}
					return result.Trim();
				}
				*/

		/* oddly, we have various methods around for captions, but non were wired up.
         * at the moment, I can't think of why you'd want captions... but not exactly ready
         * to delete all this in case it later occurs to me.... 
		public string GetCaption(string path)
		{
			try
			{
				var partialPath = "";//TODO path.Replace(DefaultAorRootImagePath, "");
				partialPath = partialPath.Replace(Path.DirectorySeparatorChar, ':');
				partialPath = partialPath.Trim(new char[] { ':' });
				lock (_padlock)
					return _indexOfRelativeFilePathToKeywordsCsv[partialPath];
			}
			catch (Exception)
			{
				return "error";
			}
		}*/

		private string FindIndexFileInFolder(string folderPath)
		{
			// original English and Indonesian version of the index ArtOfReadingIndexV3_en.txt
			var p = Path.Combine(folderPath, "ArtOfReadingIndexV3_en.txt");
			if (File.Exists(p))
			{
				_fixPaths = FixOldArtOfReadingPaths;
				return p;
			}

			// Art Of Reading 3.1 and 3.2
			p = Path.Combine(folderPath, "ArtOfReadingMultilingualIndex.txt");
			if (File.Exists(p))
			{
				_fixPaths = FixOldArtOfReadingPaths;
				return p;
			}

			// Art of Reading 3.3+ and any new collections
			p = Path.Combine(folderPath, "index.txt");
			if (File.Exists(p))
			{
				return p;
			}

			return string.Empty; // could not find an index
		}

		private static IEnumerable<string> FixOldArtOfReadingPaths(IEnumerable<string> paths)
		{
			foreach (var path in paths)
			{
				var p = path;
				if (!Path.HasExtension(path))
				{
					p += ".png";
				}
				var f = Path.GetFileName(p);
				if (!f.ToLowerInvariant().StartsWith("aor_"))
				{
					p = Path.Combine(Path.GetDirectoryName(p), "AOR_" + f);
				}
				yield return p;
			}
		}

	}
}
