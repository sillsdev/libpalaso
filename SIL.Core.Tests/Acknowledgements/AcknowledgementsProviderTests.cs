// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using NUnit.Framework;
using SIL.Acknowledgements;

namespace SIL.Tests.Acknowledgements
{
	[TestFixture]
	public class AcknowledgementsProviderTests
	{
		private Dictionary<string, AcknowledgementAttribute> _ackDict;

		[SetUp]
		public void Setup()
		{
			_ackDict = new Dictionary<string, AcknowledgementAttribute>();
		}

		/// <summary>
		/// Not really checking the Html string here, just verifying that the two acknowledgements
		/// are ordered correctly in the output.
		/// </summary>
		[Test]
		public void SortByNameAndConcatenateHtml_SortsCorrectly()
		{
			var ack1 = CreateTestAcknowledgementAttribute("myKey1","my Name");
			var ack2 = CreateTestAcknowledgementAttribute("bobKey", "Bob's project", "MIT License", "2017 Bob Programmer");
			_ackDict.Add(ack1.Key, ack1);
			_ackDict.Add(ack2.Key, ack2);
			var html = AcknowledgementsProvider.SortByNameAndConcatenateHtml(_ackDict);
			Assert.That(html, Is.StringStarting("<li>Bob\'s project"));
			Assert.That(html, Is.StringContaining("my Name"));
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

		/// <summary>
		/// Tests that AssembleAcknowledgements gets the actual acknowledgements from this dll (SIL.Core.dll).
		/// Tests the part of the code that gets acknowledgements on the EXECUTING assembly, since that process
		/// is slightly different than for other assemblies.
		/// </summary>
		[Test]
		public void AssembleAcknowledgements_FindsSILCoreAcknowledgements()
		{
			var html = AcknowledgementsProvider.AssembleAcknowledgements();
			Assert.That(html, Contains.Substring("<a href=\"http://www.codeplex.com/DotNetZip\">Ionic.Zip</a>"));
		}

		private AcknowledgementAttribute CreateTestAcknowledgementAttribute(string key, string name,
			string license = "", string copyright = "", string location = "", string html = "")
		{
			return new AcknowledgementAttribute(key, name, license, copyright, location, html);
		}
	}
}
