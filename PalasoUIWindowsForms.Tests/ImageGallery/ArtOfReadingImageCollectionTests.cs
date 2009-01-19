using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Tests;
using Palaso.UI.WindowsForms.ImageGallery;

namespace WeSay.LexicalTools.Tests
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
			var matches = _artCollection.GetMatchingPictures("duck");
			Assert.Less(30, matches.Count);
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
			var matches = _artCollection.GetMatchingPictures("xyz");
			Assert.AreEqual(0, matches.Count);
		}

		[Test]
		public void GetMatchingPictures_TwoKeyWords_GetMatchesOnBoth()
		{
			var duckMatches = _artCollection.GetMatchingPictures("duck");
			var bothMatches = _artCollection.GetMatchingPictures("duck sheep");
			Assert.Greater(bothMatches.Count, duckMatches.Count);
		}

		[Test]
		public void GetMatchingPictures_KeyWordsMatchSamePicture_PictureOnlyListedOnce()
		{
			var batMatches = _artCollection.GetMatchingPictures("bat");
			var bothMatches = _artCollection.GetMatchingPictures("bat bat");
			Assert.AreEqual(bothMatches.Count, batMatches.Count);

			bothMatches = _artCollection.GetMatchingPictures("bat animal");
			List<string> found = new List<string>();
			foreach (var s in bothMatches)
			{
				Assert.IsFalse(found.Contains(s.ToString()));
				found.Add(s.ToString());
			}
		}
	}
}
