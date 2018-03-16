using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.IO;
using SIL.Windows.Forms.ImageToolbox.ImageGallery;

namespace SIL.Windows.Forms.Tests.ImageToolbox.ImageGallery
{


	[TestFixture]
	public class ImageCollectionTests
	{
		private ImageCollection _collection;
		const int kDuckPicturesInArtOfReading = 51;

		[SetUp]
		public void Setup()
		{
			_collection = new ImageCollection(Path.GetDirectoryName(FileLocationUtilities.GetFileDistributedWithApplication("ArtOfReadingIndexV3_en.txt")));
			//PictureCollection.AllowCollectionWithNoImageFolderForTesting = true; // before we load it!
			_collection.LoadIndex("en");
		}

		[TearDown]
		public void TearDown()
		{
			//PictureCollection.AllowCollectionWithNoImageFolderForTesting = false;
		}


		[Test]
		public void GetMatchingPictures_OnKeyWordHasManyMatches_GetManyMatches()
		{
			var matches = _collection.GetMatchingImages("duck");
		    Assert.AreEqual(kDuckPicturesInArtOfReading, matches.Count());
		}

		[Test]
		public void GetMatchingPictures_OnKeyWordHasNoMatches_GetNoMatches()
		{
			var matches = _collection.GetMatchingImages("xy3z");
			Assert.AreEqual(0, matches.Count());
		}

		[Test]
		public void GetMatchingPictures_TwoKeyWords_GetMatchesOnBoth()
		{
			var duckMatches = _collection.GetMatchingImages("duck");
			var bothMatches = _collection.GetMatchingImages("duck sheep");
			Assert.Greater(bothMatches.Count(), duckMatches.Count());
		}

		[Test]
		public void GetMatchingPictures_UpperCaseQuery_GetsMatches()
		{
			var treesMatches = _collection.GetMatchingImages("trees");
			var TREESMatches = _collection.GetMatchingImages("TREES");
			Assert.AreEqual(treesMatches.Count(), TREESMatches.Count());
		}


		[Test]
		public void GetMatchingPictures_WordsFollowedByPunctuation_StillMatches()
		{
			var duckMatches = _collection.GetMatchingImages("duck,");
			Assert.AreEqual(kDuckPicturesInArtOfReading, duckMatches.Count());
		}

		[Test]
		public void GetMatchingPictures_HasExtraNonMatchingWords_StillMatches()
		{
			var duckMatches = _collection.GetMatchingImages("duck blah");
			Assert.AreEqual(kDuckPicturesInArtOfReading, duckMatches.Count());
		}

		[Test]
		public void GetMatchingPictures_KeyWordsMatchSamePicture_PictureOnlyListedOnce()
		{
			var batMatches = _collection.GetMatchingImages("bat");
			var bothMatches = _collection.GetMatchingImages("bat bat");
			Assert.AreEqual(bothMatches.Count(), batMatches.Count());

			bothMatches = _collection.GetMatchingImages("bat animal");
			List<string> found = new List<string>();
			foreach (var s in bothMatches)
			{
				Assert.IsFalse(found.Contains(s.ToString()));
				found.Add(s.ToString());
			}
		}
	}
}
