using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SIL.Code;
using SIL.Extensions;
using SIL.IO;
using SIL.Reporting;

namespace SIL.Windows.Forms.ImageToolbox.ImageGallery
{
	/// <summary>
	/// Finds and searches all the ImagesCollections on the machine.
	/// While we have now standardized on location of collections and where they must be on disk,
	/// this is backwards-compatible so that people who have previous versions of the Art Of Reading
	/// collection do not need to upgrade that.
	/// </summary>
	public class ImageCollectionManager
	{
		public IEnumerable<ImageCollection> Collections => _collections;

		// For testing.
		internal static string ImageCollectionsFolder { get; set; }

		private readonly List<ImageCollection> _collections;
		private IEnumerable<ImageCollection> EnabledCollections => Collections.Where(c => c.Enabled);
		private string _searchLanguage;
		private bool _indicesLoaded;

		/// <summary>
		/// Factory method that finds and loads any available image collections
		/// </summary>
		public static ImageCollectionManager FromStandardLocations(string searchLanguageId = "en")
		{
			var manager = new ImageCollectionManager(searchLanguageId);
			manager.FindAndLoadCollections();
			if (manager.Collections.Any())
				return manager;
			return null;
		}

		internal ImageCollectionManager(string searchLanguageId)
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
			//unit tests can override this
			ImageCollectionsFolder =
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
					.CombineForPath("SIL", "ImageCollections");
			_searchLanguage = searchLanguageId;
			_collections = new List<ImageCollection>();
		}

		/// <summary>
		/// Language ids that can be selected for searching
		/// </summary>
		public IEnumerable<string> IndexLanguageIds
		{
			get { return Collections.SelectMany(c => c.IndexLanguages).Distinct(); }
		}

		/// <summary>
		/// After construction, need to call this to locate and begin loading the indices.
		/// This launches threads that do the actual loading. The key data is c# locked
		/// so that if you try to do a search before they are done, should at least get
		/// a partial match. (Enhance: we could give a way to wait for them to be done).
		/// </summary>
		/// <param name="explicitFoldersForUnitTests">normally you shold omit this. Unit tests cnn specify for unit tests.</param>
		public void FindAndLoadCollections(IEnumerable<string> explicitFoldersForUnitTests = null)
		{
			List<string> collectionFolders = new List<string>();
			if(explicitFoldersForUnitTests != null)
			{
				collectionFolders.AddRange(explicitFoldersForUnitTests);
			}
			else
			{
				if(Directory.Exists(ImageCollectionsFolder))
				{
					collectionFolders.AddRange(
						Directory.GetDirectories(ImageCollectionsFolder)
							.Where(dir => !string.IsNullOrEmpty(LookForIndex(dir))));
				}

				// if there was no collection that looks like Art Of Reading in the SIL/ImageCollections, look in the old location used prior to Art Of Reading 3.3
				if(!collectionFolders.Any(path => path.Contains("Art") && path.Contains("Reading")))
				{
					var aorPath = TryToGetLegacyArtOfReadingPath();
					if(!string.IsNullOrEmpty(aorPath))
					{
						collectionFolders.Add(aorPath);
					}
				}
			}
			_collections.AddRange(collectionFolders.Select(f => new ImageCollection(f)));

			// Load the index information asynchronously so as not to delay displaying
			// the parent dialog.
			var indexLoadingThread = new Thread(() =>
			{
				foreach(var c in Collections)
				{
					try
					{
						c.LoadIndex(_searchLanguage);
					}
					catch(Exception e)
					{
						// This is not worth localizing. It should be very rare to have a problem in an
						// image collection that has been distributed, and pretty easy for us to get a hold
						// of it and see the real problem.
						ErrorReport.NotifyUserOfProblem($"There was a problem loading the {c.Name} collection, so some images may not be findable. The problem was:{Environment.NewLine}{e.Message}.");
					}
				}
				_indicesLoaded = true;
			});
			indexLoadingThread.Name = "Index Loading Thread";
			indexLoadingThread.Start();
		}

		/// <summary>
		/// Get images that exactly match one or more search terms.
		/// </summary>
		public IEnumerable<string> GetMatchingImages(string searchTerms, bool limitToThoseActuallyAvailable,
			out bool foundExactMatches)
		{
			foundExactMatches = false;

			WaitForLoadingToFinish();
			// If still not done, give up and return no results.
			if(!_indicesLoaded)
			{
				return new string[]{};
			}

			searchTerms = GetCleanedUpSearchString(searchTerms.ToLowerInvariant());
			var searchTermArray = searchTerms.SplitTrimmed(' ');

			var result = new List<string>();
			foreach (var collection in EnabledCollections)
			{
				var matchingImages = collection.GetMatchingImages(searchTermArray);
				if (matchingImages.Any())
				{
					result.AddRange(matchingImages);
					foundExactMatches = true;
				}
			}
			if (!foundExactMatches)
			{
				foreach (var collection in EnabledCollections)
				{
					result.AddRange(collection.GetApproximateMatchingImages(searchTermArray));
				}
			}
			var finalResult = limitToThoseActuallyAvailable ? result.Where(File.Exists) : result;
#if DEBUG
			if (limitToThoseActuallyAvailable)
			{
				System.Diagnostics.Debug.Assert(result.Count == finalResult.Count(), "All of the images in the index should exist");
			}
#endif
			return finalResult;
		}

		private void WaitForLoadingToFinish()
		{
			// On a dev machine, the index loading feels instantaneous with AOR,
			// but if the user were on a super slow computer, and entered a query quickly, he would
			// get a spinning cursor until they are done.
			const int kMaxSecondsToWaitForIndexLoading = 20;
			var whenToGiveUp = DateTime.Now.AddSeconds(kMaxSecondsToWaitForIndexLoading);
			while (!_indicesLoaded && DateTime.Now < whenToGiveUp)
			{
				Thread.Sleep(10);
			}
		}


		/// <summary>
		/// Load the index again, this time for the a different language id
		/// </summary>
		public void ChangeSearchLanguageAndReloadIndex(string searchLanguageId)
		{
			// Usually not needed, but reduce the possibility of a crash from trying to set the
			// same index twice.  See https://issues.bloomlibrary.org/youtrack/issue/BL-9825.
			WaitForLoadingToFinish();
			_searchLanguage = searchLanguageId;
			foreach (var c in Collections)
			{
				c.LoadIndex(_searchLanguage);
			}
		}

		/// <summary>
		/// Include the given collection in searches
		/// </summary>
		/// <remarks>Not obvious that we need this on this class in addition to ImageCollection, except it's convenient for unit tests</remarks>
		internal void SetCollectionEnabledStatus(string folderPath, bool enabled)
		{
			var c = GetExistingCollectionFromPath(folderPath);
			if (c == null)
			{
				return; // we don't have it, so it's enabled-ness is meaningless
			}
			c.Enabled = enabled;
		}

		/// <summary>
		/// Include the given collection in searches
		/// </summary>
		/// <remarks>Not obvious that we need this on this class in addition to ImageCollection, except it's convenient for unit tests</remarks>
		internal bool IsCollectionEnabled(string folderPath)
		{
			var c = GetExistingCollectionFromPath(folderPath);
			Guard.AgainstNull(c, "collection wasn't found in the ImageCollectionManager" + folderPath);
			return c.Enabled;
		}

		private static string TryToGetLegacyArtOfReadingPath()
		{
			// TryToGetLegacyArtOfReadingImagesPath is some legacy code that works but gives us the image folder, rather
			// than the root of the collection folder that it makes more sense to work with. So we just wrap it and
			// return the parent of the Images folder.
			var p = TryToGetLegacyArtOfReadingImagesPath();
			if (p != null)
				p = Path.GetDirectoryName(p);
			return p;
		}

		private static string TryToGetLegacyArtOfReadingImagesPath()
		{
			const string kImages = "Images";
			var distributedWithApp = FileLocationUtilities.GetDirectoryDistributedWithApplication(true, "Art Of Reading", kImages);
			if (!string.IsNullOrEmpty(distributedWithApp) && Directory.Exists(distributedWithApp))
				return distributedWithApp;

			//look for it in a hard-coded location
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				var unixPaths = new[]
				{
					@"/usr/share/ArtOfReading/images", // new package location (standard LSB/FHS location)
                    @"/usr/share/SIL/ArtOfReading/images", // old (lost) package location
                    @"/var/share/ArtOfReading/images" // obsolete test location (?)
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
				var aorInstallerTarget =
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
						.CombineForPath("SIL", "Art Of Reading", kImages);

				//the rest of these are for before we had an installer for AOR
				var appData =
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
						.CombineForPath("Art Of Reading", kImages);
				var appDataNoSpace =
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
						.CombineForPath("ArtOfReading", kImages);
				var winPaths = new[]
				{
					aorInstallerTarget, @"c:\art of reading\images", @"c:/ArtOfReading/images", appData, appDataNoSpace
				};

				foreach (var path in winPaths)
				{
					if (Directory.Exists(path))
						return path;
				}
			}
			return null;
		}

		internal static string GetCleanedUpSearchString(string searchTerms)
		{
			searchTerms = searchTerms.Replace(",", " ");
			searchTerms = searchTerms.Replace(";", " ");
			searchTerms = searchTerms.Replace("-", string.Empty);
			searchTerms = searchTerms.Replace(".", string.Empty);
			searchTerms = searchTerms.Replace("(", string.Empty);
			return searchTerms.Replace(")", string.Empty);
		}

		private ImageCollection GetExistingCollectionFromPath(string folderPath)
			=> Collections.FirstOrDefault(c => c.FolderPath == folderPath);


		private static string LookForIndex(string directory)
		{
			if (String.IsNullOrEmpty(directory) || !Directory.Exists(directory))
				return null;
			// It's somewhat arbitrary what order we use here, but making it alphabetical makes it more
			// predictable, since the order returned by EnumerateFiles is not guaranteed.
			// One thing that is important is that in versions of AOR that have both
			// ArtOfReadingMultilingualIndex.txt and index.txt, the former must be found first.
			return Directory.EnumerateFiles(directory, "*ndex.txt")
				.Where(x => x.ToLowerInvariant().EndsWith("index.txt"))
				// no way to enumerate [iI]ndex.txt on Linux, and we need both
				.OrderBy(x => x.ToLowerInvariant())
				.FirstOrDefault();
		}
	}
}
