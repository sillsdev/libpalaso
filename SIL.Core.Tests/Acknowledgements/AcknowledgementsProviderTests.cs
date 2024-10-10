// Copyright (c) 2024 SIL Global
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
			var ack1 = new AcknowledgementAttribute("myKey1") { Name = "my Name" };
			var ack2 = new AcknowledgementAttribute("bobKey")
				{ Name = "Bob's project", LicenseUrl = "MIT License", Copyright = "2017 Bob Programmer" };
			_ackDict.Add(ack1.Key, ack1);
			_ackDict.Add(ack2.Key, ack2);
			var html = AcknowledgementsProvider.SortByNameAndConcatenateHtml(_ackDict);
			Assert.That(html, Does.StartWith("<li>Bob\'s project"));
			Assert.That(html, Does.Contain("my Name"));
		}

		[Test]
		public void CollectAcknowledgementsAndRemoveDuplicates_HandlesDuplicateKeys()
		{
			var ack1 = new AcknowledgementAttribute("myKey1") { Name = "my Name" };
			var ack2 = new AcknowledgementAttribute("myKey1") { Name = "my Other Name" };
			var ackList = new List<AcknowledgementAttribute> {ack1, ack2};
			AcknowledgementsProvider.CollectAcknowledgementsAndRemoveDuplicates(_ackDict, ackList);
			Assert.AreEqual(1, _ackDict.Count, "Should have stripped out a duplicate key.");
			Assert.AreEqual("my Name", _ackDict["myKey1"].Name);
		}

		/// <summary>
		/// Tests that AssembleAcknowledgements gets the actual acknowledgements from the SIL.Core dll.
		/// Tests the part of the code that gets acknowledgements on the EXECUTING assembly, since that process
		/// is slightly different than for other assemblies.
		/// </summary>
		[Test]
		public void AssembleAcknowledgements_FindsSILCoreAcknowledgements()
		{
			var html = AcknowledgementsProvider.AssembleAcknowledgements();
			Assert.That(html, Contains.Substring("<a href='https://www.nuget.org/packages/Newtonsoft.Json/'>Json.NET</a>"));
		}
	}
}
