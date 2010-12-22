using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Palaso.Extensions;
using Palaso.IO;
using Palaso.Linq;

#if MONO
// FIXME: Would prefer that LinqBridge didn't implement ForEach as per standard
using Palaso.Linq;
#endif

namespace Palaso.UI.WindowsForms.ImageGallery
{
	public class ArtOfReadingImageCollection :IImageCollection
	{
		private Dictionary<string, List<string>> _wordToPartialPathIndex;
		private Dictionary<string, string> _partialPathToWordsIndex;

		public ArtOfReadingImageCollection()
		{
		   _wordToPartialPathIndex = new Dictionary<string, List<string>>();
		   _partialPathToWordsIndex = new Dictionary<string, string>();
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
					_partialPathToWordsIndex.Add(partialPath, keyString);
					var keys = keyString.SplitTrimmed(',');
					foreach (var key in keys)
					{
						_wordToPartialPathIndex.GetOrCreate(key).Add(partialPath);
					}
				}
			}
		}

		public IEnumerable<object> GetMatchingPictures(string keywords)
		{
			keywords = GetCleanedUpSearchString(keywords);
			return GetMatchingPictures(keywords.SplitTrimmed(' '));
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
				if (!limitToThoseActuallyAvailable || File.Exists(path))
				{
					yield return path;
				}
			}
		}

		private IEnumerable<object> GetMatchingPictures(IEnumerable<string> keywords)
		{
			List<string> pictures = new List<string>();
			foreach (var key in keywords)
			{
				List<string> picturesForThisKey = new List<string>();

				if (_wordToPartialPathIndex.TryGetValue(key, out picturesForThisKey))
				{
					pictures.AddRange(picturesForThisKey);
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
				if (_wordToPartialPathIndex.ContainsKey(key))
					result += " " + key;
			}
			return result.Trim();
		}

		private static string TryToGetRootImageCatalogPath()
		{
			//look for the cd/dvd
			var cdPath = TryToGetPathToCollectionOnCd();
			if (!string.IsNullOrEmpty(cdPath))
				return cdPath;

			var distributedWithApp = FileLocator.GetDirectoryDistributedWithApplication(true,"ArtOfReading", "images");
			if(!string.IsNullOrEmpty(distributedWithApp) && Directory.Exists(distributedWithApp))
				return distributedWithApp;

			//look for it in a hard-coded location
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				var unixPaths = new[]
									{
										@"c:\art of reading\images", @"/usr/share/wesay/ArtOfReading/images",
										@"/var/share/ArtOfReading/images"
									};

				foreach (var path in unixPaths)
				{
					if (Directory.Exists(path))
						return path;
				}
			}
			else
			{
				var winPaths = new[] {@"c:\art of reading\images", @"c:/ArtOfReading/images"};
				foreach (var path in winPaths)
				{
					if (Directory.Exists(path))
						return path;
				}
			}

			return null;
		}

		public static string TryToGetPathToCollectionOnCd()
		{
			//look for CD
			foreach (var drive in DriveInfo.GetDrives())
			{
				try
				{
					if (drive.VolumeLabel.Contains("Art Of Reading"))
						return Path.Combine(drive.RootDirectory.FullName, "images");
				}
				catch (Exception)
				{
				}
			}
			return null;
		}


		public static IImageCollection FromStandardLocations()
		{
			var c = new ArtOfReadingImageCollection();
			c.RootImagePath = TryToGetRootImageCatalogPath();

			string pathToIndexFile = FileLocator.GetFileDistributedWithApplication("ArtOfReadingIndexV3_en.txt");
			if (String.IsNullOrEmpty(pathToIndexFile))
			{
				throw new FileNotFoundException("Could not find Art of reading index file.");
			}
			c.LoadIndex(pathToIndexFile);
			return c;
		}
	}
}
