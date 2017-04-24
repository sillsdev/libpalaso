using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using SIL.Acknowledgements;

namespace SIL.Tests.Acknowledgements
{
	[TestFixture]
	class AcknowledgementsProviderTests
	{
		private Dictionary<string, AcknowledgementAttribute> _ackDict;

		[SetUp]
		public void Setup()
		{
			_ackDict = new Dictionary<string, AcknowledgementAttribute>();
		}

		[Test]
		public void SortByNameAndConcatenateHtml_SortsCorrectly()
		{
			var ack1 = CreateTestAcknowledgementAttribute("myKey1","my Name");
			var ack2 = CreateTestAcknowledgementAttribute("bobKey", "Bob's project", "MIT License", "2017 Bob Programmer");
			_ackDict.Add(ack1.Key, ack1);
			_ackDict.Add(ack2.Key, ack2);
			var html = AcknowledgementsProvider.SortByNameAndConcatenateHtml(_ackDict);
			Assert.That(html, Is.StringStarting("<li>Bob\'s project"));
		}

		[Test]
		public void CollectAcknowledgementsAndRemoveDuplicates_HandlesDuplicateKeys()
		{
			var ack1 = CreateTestAcknowledgementAttribute("myKey1", "my Name");
			var ack2 = CreateTestAcknowledgementAttribute("myKey1", "my Other Name");
			var ackList = new List<AcknowledgementAttribute> {ack1, ack2};
			AcknowledgementsProvider.CollectAcknowledgementsAndRemoveDuplicates(_ackDict, ackList);
			Assert.AreEqual(1, _ackDict.Count, "Should have stripped out a duplicate key.");
			Assert.AreEqual("my Name", _ackDict["myKey1"].Name);
		}

		[Test]
		public void AcknowledgementProvider_FindsCoreAcknowledgements()
		{
			var html = AcknowledgementsProvider.AssembleAcknowledgements();
			Assert.That(html, Contains.Substring("<a href=\"http://www.codeplex.com/DotNetZip\">Ionic.Zip</a>"));
		}

		private AcknowledgementAttribute CreateTestAcknowledgementAttribute(string aaKey, string aaName,
			string aaLicense = "", string aaCopyright = "", string aaLocation = "", string aaHtml = "")
		{
			return new AcknowledgementAttribute(aaKey, aaName, aaLicense, aaCopyright, aaLocation, aaHtml);
		}
	}
}
