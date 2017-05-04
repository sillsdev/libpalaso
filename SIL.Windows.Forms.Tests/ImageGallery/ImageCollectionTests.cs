using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Windows.Forms.ImageGallery;

namespace SIL.Windows.Forms.Tests.ImageGallery
{


	[TestFixture]
	public class ImageCollectionTests
	{
		private ImageCollection _artCollection;

		[SetUp]
		public void Setup()
		{
			ImageCollection.AllowCollectionWithNoImageFolderForTesting = true; // before we load it!
			_artCollection = new ImageCollection();
			_artCollection.LoadOldStyleIndex(IndexPath);
		}

		[TearDown]
		public void TearDown()
		{
			ImageCollection.AllowCollectionWithNoImageFolderForTesting = false;
		}


		[Test]
		public void GetMatchingPictures_OnKeyWordHasManyMatches_GetManyMatches()
		{
			bool foundExactMatches;
			var matches = _artCollection.GetMatchingPictures("duck", out foundExactMatches);
			Assert.Less(30, matches.Count());
		}

		/// <summary>
		/// the index should be copied into the output directory
		/// </summary>
		private string IndexPath
		{
			get { return Path.Combine("ImageGallery","ArtOfReadingIndexV3_en.txt"); }
		}

		[Test]
		public void GetMatchingPictures_OnKeyWordHasTwoMatches_GetTwoMatches()
		{
			bool foundExactMatches;
			var matches = _artCollection.GetMatchingPictures("xyz", out foundExactMatches);
			Assert.AreEqual(0, matches.Count());
		}


		[Test]
		public void GetMatchingKeywords_2Good1Not_GivesGood()
		{
			var wordsForSeach = _artCollection.StripNonMatchingKeywords("duck blah dog");

			Assert.AreEqual("duck dog", wordsForSeach);
		}

		[Test]
		public void GetMatchingPictures_TwoKeyWords_GetMatchesOnBoth()
		{
			bool foundExactMatches;
			var duckMatches = _artCollection.GetMatchingPictures("duck", out foundExactMatches);
			var bothMatches = _artCollection.GetMatchingPictures("duck sheep", out foundExactMatches);
			Assert.Greater(bothMatches.Count(), duckMatches.Count());
		}

		[Test]
		public void GetMatchingPictures_UpperCaseQuery_GetsMatches()
		{
			bool foundExactMatches;
			var treesMatches = _artCollection.GetMatchingPictures("trees", out foundExactMatches);
			var TREESMatches = _artCollection.GetMatchingPictures("TREES", out foundExactMatches);
			Assert.AreEqual(treesMatches.Count(), TREESMatches.Count());
		}


		[Test]
		public void GetMatchingPictures_WordsFollowedByPunctuation_StillMatches()
		{
			bool foundExactMatches;
			var duckMatches = _artCollection.GetMatchingPictures("duck, blah", out foundExactMatches);
			Assert.Less(0, duckMatches.Count());
		}

		[Test]
		public void GetMatchingPictures_KeyWordsMatchSamePicture_PictureOnlyListedOnce()
		{
			bool foundExactMatches;
			var batMatches = _artCollection.GetMatchingPictures("bat", out foundExactMatches);
			var bothMatches = _artCollection.GetMatchingPictures("bat bat", out foundExactMatches);
			Assert.AreEqual(bothMatches.Count(), batMatches.Count());

			bothMatches = _artCollection.GetMatchingPictures("bat animal", out foundExactMatches);
			List<string> found = new List<string>();
			foreach (var s in bothMatches)
			{
				Assert.IsFalse(found.Contains(s.ToString()));
				found.Add(s.ToString());
			}
		}

		[Test]
		public void FromStandardLocations_NoArtOfReadingInstalled_Null()
		{
			try
			{
				ImageCollection.DoNotFindArtOfReading_Test = true;
				using (var tempfolder = new TemporaryFolder("No image folders"))
				{
					ImageCollection.StandardAdditionalDirectoriesRoot = tempfolder.Path;
					Assert.IsNull(ImageCollection.FromStandardLocations());
				}
			}
			finally
			{
				ImageCollection.DoNotFindArtOfReading_Test = false;
			}
		}
//
//	    [Test]
//	    public void LoadMultilingualIndex_HasNoCountryOrArtist_blah()
//	    {
//	        using(var index = new TempFile())
//	        {
//				File.WriteAllText(index.Path, "order\tfilename\tartist\tcountry\ten\tfr");
//	            var collection = new ImageCollection();
//	            collection.LoadMultilingualIndex(index.Path);
//	            collection.GetIndexLanguages();
//				Assert.AreEqual(2,collection.IndexLanguageIds);
//	        }
//	    }
	}
}
