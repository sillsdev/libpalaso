using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using SIL.Windows.Forms.ImageGallery;

namespace SIL.Windows.Forms.Tests.ImageGallery
{
	[TestFixture]
	public class ImageIndexLayoutTests
	{
	    [Test]
		[TestCase("")] // empty
		[TestCase("filename")] // no languages
		[TestCase("en fr es")] // no filename
		public void Constructor_HeaderHasErrors_ExceptionThrown(string header)
	    {
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(header)))
			using (var reader = new StreamReader(stream))
			{
				Assert.Throws<InvalidOperationException>(() => new ImageIndexLayout(reader));
			}
	    }

		[TestCase("filename\ten", 1)]
		[TestCase("filename\tsubfolder\tlicense\ten\tsome comment\tid\tfoobar", 2)]
		public void Constructor_HeaderIsGood_HasExpectedNumberLanguages(string header, int languageCount)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(header)))
			using (var reader = new StreamReader(stream))
			{
			    var layout = new ImageIndexLayout(reader);
				Assert.AreEqual(languageCount,layout.LanguageIds.Count);
			}
		}

		[Test]
		public void Constructor_AOR3dot2_ReadCorrectly()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("order\tfilename\tartist\tcountry\ten\tru\tid\tfr\tes\tar\thi\tbn\tpt\tth\tsw\tzh")))
			using (var reader = new StreamReader(stream))
			{
				var layout = new ImageIndexLayout(reader);
				Assert.AreEqual(12, layout.LanguageIds.Count);
				Assert.AreEqual("ru",layout.LanguageIds[1]);
			    var row = "1	B-3-3	TheArtist	Brazil	boy,child,head,people,shoulder	мальчик,ребёнок,голова,люди,плечо	anak laki-laki,anak,kepala,orang,orang-orang,bahu";
			    var rowparts = row.Split('\t');
				Assert.AreEqual("B-3-3", layout.GetFilename(rowparts));
				Assert.AreEqual("Brazil", layout.GetSubFolderOrEmpty(rowparts));
				Assert.AreEqual("мальчик,ребёнок,голова,люди,плечо", layout.GetCSVOfKeywordsOrEmpty("ru", rowparts));
			}
		}
	}
}
