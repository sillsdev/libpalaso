using System.Collections.Generic;
using System.IO;
using System.Linq;

using Palaso.UI.WindowsForms.ImageGallery;

using NUnit.Framework;

namespace PalasoUIWindowsForms.Tests.ImageGallery
{


	[TestFixture]
	public class ArtOfReadingImageCollectionTests
	{
		private ArtOfReadingImageCollection _artCollection;

		[SetUp]
		public void Setup()
		{
			_artCollection = new Palaso.UI.WindowsForms.ImageGallery.ArtOfReadingImageCollection();
			_artCollection.LoadIndex(IndexPath);
		}

		[TearDown]
		public void TearDown()
		{
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
				ArtOfReadingImageCollection.DoNotFindArtOfReading_Test = true;
				Assert.IsNull(ArtOfReadingImageCollection.FromStandardLocations());
			}
			finally
			{
				ArtOfReadingImageCollection.DoNotFindArtOfReading_Test = false;
			}
		}

	}
}
