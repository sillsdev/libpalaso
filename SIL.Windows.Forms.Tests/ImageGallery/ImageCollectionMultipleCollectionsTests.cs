using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Windows.Forms.ImageGallery;

namespace SIL.Windows.Forms.Tests.ImageGallery
{
	public class ImageCollectionMultipleCollectionsTests
	{
		private TemporaryFolder _testFolder;
		private string _aorRoot;
		private string _additionalRoot;
		private string _additionalCollectionBob;
		private string _additionalCollectionSally;
		private ImageCollection _collection;

		[TestFixtureSetUp]
		public void SetupFakeCollections()
		{
			_testFolder = new TemporaryFolder("AOR_Multiple_Tests");
			_aorRoot = Path.Combine(_testFolder.Path, "AOR");
			_additionalRoot = Path.Combine(_testFolder.Path, "Additional");
			_additionalCollectionBob = Path.Combine(_additionalRoot, "Bob");
			_additionalCollectionSally = Path.Combine(_additionalRoot, "Sally");

			_collection = new ImageCollection();
			_collection.AdditionalCollectionPaths = new[]
				{_additionalCollectionBob, _additionalCollectionSally};
			_collection.RootImagePath = Path.Combine(_aorRoot, ImageCollection.ImageFolder);
			// Don't really need all these for the tests here, but a little bit of data out of the real thing might be good for catching problems
			MakeFakeImageCollection(_aorRoot, "AOR_", "ArtOfReadingMultilingualIndex.txt", @"order	filename	artist	country	en	id	fr	es	ar	hi	bn	pt	th	sw	zh
1	B-3-3		Brazil	boy,child,head,people,shoulder	anak laki-laki,anak,kepala,orang,orang-orang,bahu	garçon,enfant,tête,personnes,épaule	niño,niño,cabeza,gente,hombro	صبي,طفل,رئيس,الناس,كتف	लड़का,बच्चा,सिर,लोग,कंधा	ছেলে,শিশু,মাথা,সম্প্রদায়,অংস	Garoto,criança,cabeça,pessoas,ombro	เด็กผู้ชาย,เด็ก,หัว,คน,ไหล่	mvulana,Mtoto,kichwa,Watu,bega	男孩,孩子,头,人,肩
2	B-NA-6		Brazil	parrot,bird,macaw	burung,nuri,bayan	perroquet,oiseau,ara	loro,pájaro,guacamayo	ببغاء,عصفور,المقو نوع من الببغاء	तोता,पक्षी,एक प्रकार का तोता	তোতাপাখি,পাখি,আমেরিকার কাকাতুয়া	papagaio,pássaro,arara	นกแก้ว,นก,นกมะคอ	parrot,ndege,Macaw	鹦鹉,鸟,金刚鹦鹉
3	B-A-10		Brazil	animal,armadillo	binatang	animal,tatou	animal,armadillo	حيوان,المدرع حيوان ثديي	पशु,Armadillo	পশু,সাঁজোয়া জাহাজ	animal,tatu	สัตว์,ตัวนิ่ม	wanyama,kakakuona	动物,犰狳
4	B-NA-1		Brazil	peccary,pig,animal,wild pig	binatang,babi	pécari,porc,animal,cochon sauvage	pecarí,cerdo,animal,jabalí	حيوان امريكي شبيه بالخنزير,خنزير,حيوان,الخنزير البري	अमेरिका देश का सुअर के आकार का एक चौपाया,सुअर,पशु,जंगली सुअर	দক্ষিণ আমেরিকার শূকসদৃশ প্রাণীবিশেষ,শূকর,পশু,বন্য শূকর	pecari,porco,animal,porco selvagem	สัตว์เพคะริ,หมู,สัตว์,หมูป่า	peccary,nguruwe,wanyama,nguruwe pori	野猪,猪,动物,野猪
5	CMB0012		Cambodia	dish,food,rice	beras,makanan,nasi,piring	plat,aliments,riz	plato,comida,arroz	طبق,طعام,الأرز	थाली,भोजन,चावल	থালা,খাদ্য,চাল	prato,Comida,arroz	จาน,อาหาร,ข้าว	sahani,chakula,mchele	菜,食品,饭");
			MakeFakeImageCollection(_additionalCollectionBob, "Bob_", "BobsMultilingualIndex.txt", @"order	filename	artist	country	en	id	fr	es	ar	hi	bn	pt	th	sw	zh
1	First		Australia	galaxy
1	Christmas Lights		Australia	Christmas,lights,programming,stars,bridge	
2	Hubble Galaxy		Australia	Hubble,stars,galaxy	");
			MakeFakeImageCollection(_additionalCollectionSally, "", "SallysMultilingualIndex.txt", @"order	filename	artist	country	en	id	fr	es	ar	hi	bn	pt	th	sw	zh
1	brokenBridge		Melbourne	bridge,broken,melbourne,skyscraper	bridgeIn,brokenIn,melbourneIn,skyscraperIn
2	Central		Melbourne	skyscraper,bridge,river,melbourne	
3	Yarra		Melbourne	yarra,river,melbourne,skyscraper
4	Central			Sydney	skyscraper,bridge,river,sydney	
5	Bridge		Sydney	bridge,sydney,harbor,child
6	OperaAndBridge		Sydney	bridge,opera,sydney,harbor
7	RainbowWater		Sydney	bridge,opera,sydney,rainbow,harbor");
			var hubblePath = Path.Combine(_additionalCollectionBob, ImageCollection.ImageFolder, "Australia",
				"Bob_Hubble Galaxy.png");
			File.Move(hubblePath, Path.ChangeExtension(hubblePath, "jpg"));
			var christmasLightsPath = Path.Combine(_additionalCollectionBob, ImageCollection.ImageFolder, "Australia",
				"Bob_Christmas Lights.png");
			File.Delete(christmasLightsPath); // make a missing file special case.
			_collection.LoadImageIndex();
		}

		[TestFixtureTearDown]
		public void TearDownFixture()
		{
			_testFolder.Dispose();
		}

		void MakeFakeImageCollection(string rootFolder, string prefix, string indexName, string content)
		{
			Directory.CreateDirectory(rootFolder);
			var indexPath = Path.Combine(rootFolder, indexName);
			File.WriteAllText(indexPath, content);
			var imagesPath = Path.Combine(rootFolder, ImageCollection.ImageFolder);
			var first = true;
			foreach (var line in content.Replace("\r\n", "\n").Split('\n'))
			{
				if (first)
				{
					first = false;
					continue;
				}
				var parts = line.Split('\t');
				var country = parts[3];
				var file = parts[1];
				var countryDir = Path.Combine(imagesPath, country);
				Directory.CreateDirectory(countryDir);
				var fileName = Path.Combine(countryDir, prefix + file + ".png");
				File.WriteAllText(fileName, @"fake");
			}
		}

		/// <summary>
		/// Actually checks a lot of things
		/// - loading main AOR from multilingual index
		/// - whole loading process across multiple image collections
		/// - :1: prefix for things in second collection
		/// - matching in case with no file prefix (Sally's collection)
		/// </summary>
		[Test]
		public void GetMatchingPictures_FindsMatchInMainAndOtherCollection()
		{
			bool foundExactMatches;
			var pics = _collection.GetMatchingPictures("child", out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			Assert.That(pics, Has.Count.EqualTo(2));
			Assert.That(pics, Has.Member("Brazil:AOR_B-3-3.png"));
			Assert.That(pics, Has.Member(":1:Sydney:Bridge.png"));
		}

		[Test]
		public void GetMatchingPictures_FindsInexactMatchInMainAndOtherCollection()
		{
			bool foundExactMatches;
			var pics = _collection.GetMatchingPictures("chil", out foundExactMatches);
			Assert.That(foundExactMatches, Is.False);
			Assert.That(pics, Has.Count.EqualTo(2));
			Assert.That(pics, Has.Member("Brazil:AOR_B-3-3.png"));
			Assert.That(pics, Has.Member(":1:Sydney:Bridge.png"));
		}

		[Test]
		public void GetMatchingPicturesInexact_SkipsDisabledCollection()
		{
			bool foundExactMatches;
			try
			{
				_collection.EnableCollection(_additionalCollectionSally, false);
				var pics = _collection.GetMatchingPictures("chil", out foundExactMatches);
				Assert.That(foundExactMatches, Is.False);
				Assert.That(pics, Has.Count.EqualTo(1));
				Assert.That(pics, Has.Member("Brazil:AOR_B-3-3.png"));
			}
			finally
			{
				_collection.EnableCollection(_additionalCollectionSally, true);
			}
		}

		[Test]
		public void GetMatchingPicturesExact_SkipsDisabledCollection()
		{
			bool foundExactMatches;
			try
			{
				_collection.EnableCollection(_additionalCollectionSally, false);
				var pics = _collection.GetMatchingPictures("child", out foundExactMatches);
				Assert.That(foundExactMatches, Is.True);
				Assert.That(pics, Has.Count.EqualTo(1));
				Assert.That(pics, Has.Member("Brazil:AOR_B-3-3.png"));
			}
			finally
			{
				_collection.EnableCollection(_additionalCollectionSally, true);
			}
		}

		/// <summary>
		/// Other things checked
		/// - comma, space separator in keyword list
		/// </summary>
		[Test]
		public void GetMatchingPictures_FindsMatchesOnMultipleIndonesianWords()
		{
			// Note: NUnit is not supposed to run tests in the same fixture in parallel, so restoring it like this
			// should make other tests safe. If we ever want to run these tests in parallel, we'll need to
			// do better here.
			_collection.ReloadImageIndex("id");
			try
			{
				bool foundExactMatches;
				var pics = _collection.GetMatchingPictures("bridgeIn, kepala", out foundExactMatches);
				Assert.That(foundExactMatches, Is.True);
				Assert.That(pics, Has.Count.EqualTo(2));
				Assert.That(pics, Has.Member("Brazil:AOR_B-3-3.png"));
				Assert.That(pics, Has.Member(":1:Melbourne:brokenBridge.png"));
			}
			finally
			{
				_collection.ReloadImageIndex("en"); // restore for other tests.
			}
		}

		/// <summary>
		/// Other things checked
		/// - Prefix in additional collection works
		/// - different indexes added for different additional collections
		/// </summary>
		[Test]
		public void GetMatchingPictures_FindsMultipleMatchesInAdditionalCollections()
		{
			bool foundExactMatches;
			var pics = _collection.GetMatchingPictures("bridge", out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			Assert.That(pics, Has.Count.EqualTo(6));
			Assert.That(pics, Has.Member(":0:Australia:Bob_Christmas Lights.png"));
			Assert.That(pics, Has.Member(":1:Melbourne:brokenBridge.png"));
			Assert.That(pics, Has.Member(":1:Melbourne:Central.png"));
			Assert.That(pics, Has.Member(":1:Sydney:Bridge.png"));
			Assert.That(pics, Has.Member(":1:Sydney:OperaAndBridge.png"));
			Assert.That(pics, Has.Member(":1:Sydney:RainbowWater.png"));
		}

		[Test]
		public void GetPathsFromResults_IgnoreMissing_FindsBothMissingPngAndRealJpg()
		{
			bool foundExactMatches;
			var pics = _collection.GetMatchingPictures("stars", out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			var results = _collection.GetPathsFromResults(pics, false);
			Assert.That(results.Count(), Is.EqualTo(2));
			Assert.That(results, Has.Member(Path.Combine(_additionalCollectionBob, ImageCollection.ImageFolder, "Australia", "Bob_Christmas Lights.png")));
			Assert.That(results, Has.Member(Path.Combine(_additionalCollectionBob, ImageCollection.ImageFolder, "Australia", "Bob_Hubble Galaxy.jpg")));
		}

		[Test]
		public void GetPathsFromResults_SkipMissingFiles_FindsPngAndJpg()
		{
			bool foundExactMatches;
			var pics = _collection.GetMatchingPictures("galaxy", out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			var results = _collection.GetPathsFromResults(pics, true);
			Assert.That(results.Count(), Is.EqualTo(2));
			Assert.That(results, Has.Member(Path.Combine(_additionalCollectionBob, ImageCollection.ImageFolder, "Australia", "Bob_First.png")));
			Assert.That(results, Has.Member(Path.Combine(_additionalCollectionBob, ImageCollection.ImageFolder, "Australia", "Bob_Hubble Galaxy.jpg")));
		}
	}
}
