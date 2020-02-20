using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Windows.Forms.ImageToolbox.ImageGallery;

namespace SIL.Windows.Forms.Tests.ImageToolbox.ImageGallery
{
	[TestFixture]
	public class ImageCollectionManagerTests
	{
		private TemporaryFolder _testFolder;
		private string _artOfReadingFolder;
		private string _folderToLookForCollectionsIn;
		private string _bobCollectionFolder;
		private string _sallyCollectionFolder;
		private ImageCollectionManager _collectionManager;

		[OneTimeSetUp]
		public void SetupFakeCollections()
		{
			_testFolder = new TemporaryFolder("ImageCollectionManagerTests");
			_artOfReadingFolder = Path.Combine(_testFolder.Path, "AOR");
			_folderToLookForCollectionsIn = Path.Combine(_testFolder.Path, "Additional");
			_bobCollectionFolder = Path.Combine(_folderToLookForCollectionsIn, "Bob");
			_sallyCollectionFolder = Path.Combine(_folderToLookForCollectionsIn, "Sally");

			//_collection.DefaultAorRootImagePath = Path.Combine(_aorRoot, PictureCollection.ImageFolder);
			// Don't really need all these for the tests here, but a little bit of data out of the real thing might be good for catching problems
			MakeFakeImageCollection(_artOfReadingFolder, "AOR_", "ArtOfReadingMultilingualIndex.txt", @"order	filename	artist	country	en	id	fr	es	ar	hi	bn	pt	th	sw	zh
1	AOR_B-3-3.png\t\tBrazil\tboy,child,head,people,shoulder\tanak laki-laki,anak,kepala,orang,orang-orang,bahu\tgarçon,enfant,tête,personnes,épaule	niño,niño,cabeza,gente,hombro	صبي,طفل,رئيس,الناس,كتف	लड़का,बच्चा,सिर,लोग,कंधा	ছেলে,শিশু,মাথা,সম্প্রদায়,অংস	Garoto,criança,cabeça,pessoas,ombro	เด็กผู้ชาย,เด็ก,หัว,คน,ไหล่	mvulana,Mtoto,kichwa,Watu,bega	男孩,孩子,头,人,肩
2	AOR_B-NA-6.png\t\tBrazil	PARROT,bird,macaw	burung,nuri,bayan	perroquet,oiseau,ara	loro,pájaro,guacamayo	ببغاء,عصفور,المقو نوع من الببغاء	तोता,पक्षी,एक प्रकार का तोता	তোতাপাখি,পাখি,আমেরিকার কাকাতুয়া	papagaio,pássaro,arara	นกแก้ว,นก,นกมะคอ	parrot,ndege,Macaw	鹦鹉,鸟,金刚鹦鹉
3	AOR_B-A-10.png\t\tBrazil	animal,armadillo	binatang	animal,tatou	animal,armadillo	حيوان,المدرع حيوان ثديي	पशु,Armadillo	পশু,সাঁজোয়া জাহাজ	animal,tatu	สัตว์,ตัวนิ่ม	wanyama,kakakuona	动物,犰狳
4	AOR_B-NA-1.png\t\tBrazil	peccary,pig,animal,wild pig	binatang,babi	pécari,porc,animal,cochon sauvage	pecarí,cerdo,animal,jabalí	حيوان امريكي شبيه بالخنزير,خنزير,حيوان,الخنزير البري	अमेरिका देश का सुअर के आकार का एक चौपाया,सुअर,पशु,जंगली सुअर	দক্ষিণ আমেরিকার শূকসদৃশ প্রাণীবিশেষ,শূকর,পশু,বন্য শূকর	pecari,porco,animal,porco selvagem	สัตว์เพคะริ,หมู,สัตว์,หมูป่า	peccary,nguruwe,wanyama,nguruwe pori	野猪,猪,动物,野猪
5	AOR_CMB0012.png\t\tCambodia	dish,food,rice	beras,makanan,nasi,piring	plat,aliments,riz	plato,comida,arroz	طبق,طعام,الأرز	थाली,भोजन,चावल	থালা,খাদ্য,চাল	prato,Comida,arroz	จาน,อาหาร,ข้าว	sahani,chakula,mchele	菜,食品,饭");

			MakeFakeImageCollection(_bobCollectionFolder, "Bob_", "index.txt", @"filename\tsubfolder\ten\tid\tde
Bob_First.png	Australia	galaxy
Bob_Christmas Lights.png	Australia	Christmas,lights,programming,stars,bridge
Bob_Hubble Galaxy.png 	Australia	Hubble,stars,galaxy	");
			MakeFakeImageCollection(_sallyCollectionFolder, "", "index.txt", @"filename\tsubfolder\ten\tid\tjt
brokenBridge.png\tMelbourne\tbridge,broken,melbourne,skyscraper\tbridgeIn,brokenIn,melbourneIn,skyscraperIn
Central.png\tMelbourne	foo,skyscraper,bridge,river,melbourne
Yarra.png\tMelbourne	yarra,river,melbourne,skyscraper
Central.png\tSydney	skyscraper,bridge,river,sydney
Bridge.png\tSydney	bridge,sydney,harbor,child
OperaAndBridge.png\tSydney	bridge,opera,sydney,harbor
RainbowWater.png\tSydney	bridge,opera,sydney,rainbow,harbor");
			var christmasLightsPath = Path.Combine(_bobCollectionFolder, ImageCollection.kStandardImageFolderName, "Australia",
				"Bob_Christmas Lights.png");
		}

		[OneTimeTearDown]
		public void TearDownFixture()
		{
			_testFolder.Dispose();
		}

		[SetUp]
		public void Setup()
		{
			_collectionManager = new ImageCollectionManager("en");
			_collectionManager.FindAndLoadCollections(new[] { _artOfReadingFolder, _bobCollectionFolder, _sallyCollectionFolder });
		}

		void MakeFakeImageCollection(string rootFolder, string prefix, string indexName, string content)
		{
			Directory.CreateDirectory(rootFolder);
			var indexPath = Path.Combine(rootFolder, indexName);
			File.WriteAllText(indexPath, content.Replace("\\t", "\t")); //since we @ the string, the \t's get taken literally
		}

		/// <summary>
		/// Actually checks a lot of things
		/// - loading Aor AOR from multilingual index
		/// - whole loading process across multiple image collections
		/// - matching in case with no file prefix (Sally's collection)
		/// </summary>
		[Test]
		public void GetMatchingPictures_FindsMatchInAorAndOtherCollection()
		{
			bool foundExactMatches;
			var pics = _collectionManager.GetMatchingImages("child", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			Assert.That(pics, Has.Count.EqualTo(2));
			AssertHasResultThatEndsInSubpath(pics, "Brazil/AOR_B-3-3.png");
			AssertHasResultThatEndsInSubpath(pics, "Sydney/Bridge.png");
		}

		[Test]
		public void IndexLanguageIds_GivesTheUnionOfAllCollections()
		{
			var langs = _collectionManager.IndexLanguageIds;
			Assert.That(langs, Has.Member("en")); // always
			Assert.That(langs, Has.Member("id")); // common
			Assert.That(langs, Has.Member("jt")); // improbable one from secondary collection
			Assert.That(langs, Has.Member("de")); // minority only
			Assert.That(langs, Has.Member("zh")); // last and in AOR only
			Assert.AreEqual(langs.Distinct().Count(), langs.Count(), "Should not have duplicates");
		}

		[Test]
		public void GetMatchingPictures_FindsInexactMatchInAorAndOtherCollection()
		{
			bool foundExactMatches;
			var pics = _collectionManager.GetMatchingImages("chil", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.False);
			Assert.That(pics, Has.Count.EqualTo(2));
			AssertHasResultThatEndsInSubpath(pics, "Brazil/AOR_B-3-3.png");
			AssertHasResultThatEndsInSubpath(pics, "Sydney/Bridge.png");
		}

		[Test]
		public void GetMatchingPictures_OnlyApproximateMatchExists_SkipsDisabledCollection()
		{
			_collectionManager.SetCollectionEnabledStatus(_sallyCollectionFolder, false);
			bool foundExactMatches;
			var pics = _collectionManager.GetMatchingImages("chil", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.False);
			Assert.That(pics, Has.Count.EqualTo(1));
			AssertHasResultThatEndsInSubpath(pics, "Brazil/AOR_B-3-3.png");
		}

		[Test]
		public void GetMatchingPictures_IndexHadUpperCase_StillFindsIt()
		{
			bool foundExactMatches;
			var pics = _collectionManager.GetMatchingImages("pArRoT", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			Assert.That(pics, Has.Count.EqualTo(1));
			AssertHasResultThatEndsInSubpath(pics, "Brazil/AOR_B-NA-6.png");
		}

		[Test]
		public void GetMatchingPictures_ExactMatchExists_SkipsDisabledCollection()
		{
			bool foundExactMatches;
			_collectionManager.SetCollectionEnabledStatus(_sallyCollectionFolder, false);
			var pics = _collectionManager.GetMatchingImages("child", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			Assert.That(pics, Has.Count.EqualTo(1));
			AssertHasResultThatEndsInSubpath(pics, "Brazil/AOR_B-3-3.png");
		}

		/// <summary>
		/// Other things checked
		/// - comma, space separator in keyword list
		/// </summary>
		[Test]
		public void GetMatchingPictures_FindsMatchesOnMultipleIndonesianWords()
		{
			_collectionManager.ChangeSearchLanguageAndReloadIndex("id");
			bool foundExactMatches; //kepala==boy
			var pics = _collectionManager.GetMatchingImages("bridgeIn, kepala", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			AssertHasResultThatEndsInSubpath(pics, "Brazil/AOR_B-3-3.png");
			AssertHasResultThatEndsInSubpath(pics, "Melbourne/brokenBridge.png");
			Assert.That(pics, Has.Count.EqualTo(2));
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
			var pics = _collectionManager.GetMatchingImages("bridge", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			Assert.That(pics, Has.Count.EqualTo(7));
			AssertHasResultThatEndsInSubpath(pics, "Australia/Bob_Christmas Lights.png");
			AssertHasResultThatEndsInSubpath(pics, "Melbourne/brokenBridge.png");
			AssertHasResultThatEndsInSubpath(pics, "Melbourne/Central.png");
			AssertHasResultThatEndsInSubpath(pics, "Sydney/Central.png");
			AssertHasResultThatEndsInSubpath(pics, "Sydney/Bridge.png");
			AssertHasResultThatEndsInSubpath(pics, "Sydney/OperaAndBridge.png");
			AssertHasResultThatEndsInSubpath(pics, "Sydney/RainbowWater.png");
		}


		[Test]
		public void GetPathsFromResults_SkipMissingFiles()
		{
			bool foundExactMatches;
			var pics = _collectionManager.GetMatchingImages("galaxy", false, out foundExactMatches);
			Assert.That(foundExactMatches, Is.True);
			Assert.That(pics.Count(), Is.EqualTo(2));
			Debug.WriteLine(pics.First());
			Debug.WriteLine(Path.Combine(_bobCollectionFolder, ImageCollection.kStandardImageFolderName, "Australia", "Bob_First.png"));
			Assert.That(pics, Has.Member(Path.Combine(_bobCollectionFolder, ImageCollection.kStandardImageFolderName, "Australia", "Bob_First.png")));
			Assert.That(pics, Has.Member(Path.Combine(_bobCollectionFolder, ImageCollection.kStandardImageFolderName, "Australia", "Bob_Hubble Galaxy.png")));
		}

		[Test]
		public void SetCollectionEnabledStatus_ControlsIsCollectionEnabled()
		{
			_collectionManager.SetCollectionEnabledStatus(_bobCollectionFolder, true);
			Assert.True(_collectionManager.IsCollectionEnabled(_bobCollectionFolder));

			_collectionManager.SetCollectionEnabledStatus(_bobCollectionFolder, false);
			Assert.False(_collectionManager.IsCollectionEnabled(_bobCollectionFolder));

			_collectionManager.SetCollectionEnabledStatus(_bobCollectionFolder, true);
			Assert.True(_collectionManager.IsCollectionEnabled(_bobCollectionFolder));
		}

		[Test]
		public void IsCollectionEnabled_InitiallyAllEnabled()
		{
			Assert.True(_collectionManager.IsCollectionEnabled(_artOfReadingFolder));
			Assert.True(_collectionManager.IsCollectionEnabled(_bobCollectionFolder));
			Assert.True(_collectionManager.IsCollectionEnabled(_sallyCollectionFolder));
		}

		private void AssertHasResultThatEndsInSubpath(IEnumerable<string> paths, string subpathInWindowsOrLinux)
		{
			var path = subpathInWindowsOrLinux.Replace('/', Path.DirectorySeparatorChar)
				.Replace('\\', Path.DirectorySeparatorChar);
			var hassubPath = paths.Any(p => p.EndsWith(path));
			var nl = Environment.NewLine;
			Assert.True(hassubPath, "expected something ending in:" + nl + path + " but got {" + nl + paths.Aggregate((a, b) => a + nl + b).Trim(new[] { ',' }) + "}");
		}
	}
}
