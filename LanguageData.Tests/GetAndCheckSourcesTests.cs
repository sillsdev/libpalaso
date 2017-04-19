// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace LanguageData.Tests
{
	[TestFixture]
	public class GetAndCheckSourcesTests
	{
		// used in test to simulate no network
		private class WebClientWithTimeout:WebClient
		{
			protected override WebRequest GetWebRequest(Uri address)
			{
				WebRequest wr = base.GetWebRequest(address);
				wr.Timeout = 1; // timeout in milliseconds (ms)
				return wr;
			}
		}

		private class GetAndCheckSourcesNoNet:GetAndCheckSources
		{
			protected override WebClient CreateWebClient()
			{
				return new WebClientWithTimeout();
			}
		}
		[Test]
		public void Verify_AllFilesDifferent()
		{
			string stringone = "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
			string stringtwo = "22222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222";
			GetAndCheckSources getcheck = new GetAndCheckSources();
			Assert.False(getcheck.AreFilesDifferent(stringone, stringone));
			Assert.True(getcheck.AreFilesDifferent(stringone, stringtwo));
		}

		[Test]
		public void Verify_CheckSourcesAreSame()
		{
			GetAndCheckSources getcheck = new GetAndCheckSources();
			if (!getcheck.GetNewSources())
			{
				Assert.Ignore("Server not available");
			}
			getcheck.WriteNewFiles(".");
			getcheck.GetOldSources(".");
			Assert.False(getcheck.CheckSourcesAreDifferent());
		}

		[Test]
		public void Verify_CheckSourcesAreDifferent()
		{
			GetAndCheckSources getcheck = new GetAndCheckSources();
			string filename = Path.Combine(".", "LanguageIndex.txt");
			File.WriteAllText(filename, "11111111111111111111111111111111111111111111111111111111111111111");
			filename = Path.Combine(".", "ianaSubtagRegistry.txt");
			File.WriteAllText(filename, "22222222222222222222222222222222222222222222222222222222222222222");
			filename = Path.Combine(".", "TwoToThreeCodes.txt");
			File.WriteAllText(filename, "33333333333333333333333333333333333333333333333333333333333333333");
			filename = Path.Combine(".", "LanguageCodes.txt");
			File.WriteAllText(filename, "44444444444444444444444444444444444444444444444444444444444444444");

			getcheck.GetOldSources(".");

			filename = Path.Combine(".", "LanguageIndex.txt");
			File.Delete(filename);
			filename = Path.Combine(".", "LanguageCodes.txt");
			File.Delete(filename);
			filename = Path.Combine(".", "ianaSubtagRegistry.txt");
			File.Delete(filename);
			Path.Combine(".", "TwoToThreeCodes.txt");
			File.Delete(filename);

			if (!getcheck.GetNewSources())
			{
				Assert.Ignore("Server not available");
			}
			Assert.True(getcheck.CheckSourcesAreDifferent());
		}

		// checks that the TwoToThreeCodes checksum is as expected whichever system the test is run on
		[Test]
		public void Verify_TwoToThreeCodes()
		{
			string input_dir = Path.Combine ("..", "..", "SIL.WritingSystems", "Resources");
			string twotothree = File.ReadAllText(Path.Combine (input_dir, @"TwoToThreeCodes.txt"));
			twotothree = twotothree.Replace("\r\n", "\n");
			var sha = new SHA256Managed();
			byte[] checksum_twotothree = sha.ComputeHash(Encoding.UTF8.GetBytes(twotothree));
			string checksumstring = BitConverter.ToString(checksum_twotothree);
			Console.WriteLine("hash of TwoToThreeCodes.txt is: " + checksumstring);
			Assert.AreEqual("F5-3A-3D-8F-B7-F3-47-F1-12-70-B4-B8-0A-EE-51-11-CA-CC-77-AB-1B-5B-DA-06-90-B6-1F-8C-F6-31-2E-12", checksumstring);
		}

		[Test]
		public void GetOldSources_BadInputDir_throws()
		{
			GetAndCheckSources getcheck = new GetAndCheckSources();
			Assert.That(() => getcheck.GetOldSources("gibberish"),
				Throws.TypeOf<DirectoryNotFoundException>());
		}

		[Test]
		public void WriteNewFiles_BadOutputDir_throws()
		{
			GetAndCheckSources getcheck = new GetAndCheckSources();
			Assert.That(() => getcheck.WriteNewFiles("gibberish"),
				Throws.TypeOf<DirectoryNotFoundException>());
		}

		[Test]
		public void GetNewSources_Ok()
		{
			GetAndCheckSources getcheck = new GetAndCheckSources();
			Assert.True(getcheck.GetNewSources());
		}

		[Test]
		public void GetNewSources_NoNet ()
		{
			GetAndCheckSources getcheck = new GetAndCheckSourcesNoNet();
			Assert.False(getcheck.GetNewSources());
		}
	}
}

