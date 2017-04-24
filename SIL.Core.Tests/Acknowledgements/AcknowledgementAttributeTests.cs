// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.Acknowledgements;

namespace SIL.Tests.Acknowledgements
{
	[TestFixture]
	public class AcknowledgementAttributeTests
	{
		[Test]
		public void CreateAnAcknowledgement_HtmlIsNotNull()
		{
			var ack = new AcknowledgementAttribute("testKey", "testName");
			Assert.AreEqual("<li>testName</li>", ack.Html, "default html is different than expected");
		}

		[Test]
		public void CreateAnAcknowledgement_HtmlOverridesDefault()
		{
			var ack = new AcknowledgementAttribute("testKey", "testName", html: "<myOwnStuff>Bob</myOwnStuff>");
			Assert.AreEqual("<myOwnStuff>Bob</myOwnStuff>", ack.Html, "default html should be overridden");
		}

		[TestCase("Key1", "myName", "SIL 2007", "GPLUrl", "defaultLocation",
			ExpectedResult = "<li>myName: SIL 2007 <a href='GPLUrl'>GPLUrl</a></li>")]
		[TestCase("Key2", "myName", "", "", "",
			ExpectedResult = "<li>myName</li>")]
		[TestCase("Key3", "myName", "", "myLicenseUrl", "",
			ExpectedResult = "<li>myName <a href='myLicenseUrl'>myLicenseUrl</a></li>")]
		[TestCase("Key4", "myName", "Bob", "", "some location",
			ExpectedResult = "<li>myName: Bob</li>")]
		public string CreateAnAcknowledgement_TestDefaultHtml(string key, string name,
			string copyright, string license, string location)
		{
			// Apparently even if we make the last 3 parameters above optional, the [TestCase] attribute
			// needs to have values for all 5 to keep from being ignored.
			return new AcknowledgementAttribute(key, name, license, copyright, location).Html;
		}
	}
}
