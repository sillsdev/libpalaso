using System.Runtime.InteropServices;
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
			var ack = new AcknowledgementAttribute("testKey", "testName", aaHtml: "<myOwnStuff>Bob</myOwnStuff>");
			Assert.AreEqual("<myOwnStuff>Bob</myOwnStuff>", ack.Html, "default html should be overridden");
		}

		[TestCase("Key1", "myName", "SIL 2007", "GPL", "defaultLocation", Result = "<li>myName: SIL 2007 (GPL)</li>")]
		[TestCase("Key2", "myName", Result = "<li>myName</li>")]
		[TestCase("Key3", "myName", "", "myLicense", Result = "<li>myName (myLicense)</li>")]
		[TestCase("Key4", "myName", "Bob", "", "some location", Result = "<li>myName: Bob</li>")]
		public string CreateAnAcknowledgement_TestDefaultHtml(string key, string name,
			string copyright, string license, string location)
		{
			return new AcknowledgementAttribute(key, name, license, copyright, location).Html;
		}
	}
}
