using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace PalasoUIWindowsForms.Tests.ClearShare
{
	[TestFixture, Ignore("Needs exiftool in the distfiles")]
	public class MetaDataAccessorTests
	{
		private Bitmap _mediaFile;
		private TempFile _tempFile;
		private MetaDataAccess _outgoing;

		[SetUp]
		public void Setup()
		{
			_mediaFile = new Bitmap(10, 10);
			_tempFile = TempFile.WithExtension("png");
			_mediaFile.Save(_tempFile.Path);
		   _outgoing = MetaDataAccess.FromFile(_tempFile.Path);
		 }

		[TearDown]
		public void TearDown()
		{
			_tempFile.Dispose();
		}

		[Test]
		public void RoundTripPng_CopyrightNotice()
		{
			_outgoing.CopyrightNotice = "Copyright Test";
			_outgoing.Write();
			Assert.AreEqual("Copyright Test", MetaDataAccess.FromFile(_tempFile.Path).CopyrightNotice);
		}

		[Test]
		public void RoundTripPng_PNGWithDangerousCharacters_PreservesCopyrightNotice()
		{
			_outgoing.CopyrightNotice = "Copyright <! ' <hello>";
			_outgoing.Write();
			Assert.AreEqual("Copyright <! ' <hello>", MetaDataAccess.FromFile(_tempFile.Path).CopyrightNotice);
		}

		[Test]
		public void RoundTripPng_HasCreativeCommonsLicense_ReadsInSameLicense()
		{
			_outgoing.License =new CreativeCommonsLicense();
			_outgoing.Write();
			 Assert.IsInstanceOf(typeof(CreativeCommonsLicense), MetaDataAccess.FromFile(_tempFile.Path).License);
		}

		[Test]
		public void RoundTripPng_AttributionUrl()
		{
			_outgoing.AttributionUrl = "http://somewhere.com";
			_outgoing.Write();
			Assert.AreEqual("http://somewhere.com", MetaDataAccess.FromFile(_tempFile.Path).AttributionUrl);
		}

		[Test]
		public void RoundTripPng_AttributionName()
		{
			_outgoing.AttributionName = "joe shmo";
			_outgoing.Write();
			Assert.AreEqual("joe shmo", MetaDataAccess.FromFile(_tempFile.Path).AttributionName);
		}

		[Test]
		public void LoadFromFile_CopyrightNotSet_CopyrightGivesNull()
		{
			Assert.IsNull(MetaDataAccess.FromFile(_tempFile.Path).AttributionName);
		}
	}
}
