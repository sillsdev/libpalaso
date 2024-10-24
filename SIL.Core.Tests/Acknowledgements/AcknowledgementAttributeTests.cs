// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
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
			var ack = new AcknowledgementAttribute("testKey") { Name = "testName" };
			Assert.AreEqual("<li>testName</li>", ack.Html, "default html is different than expected");
		}

		[Test]
		public void CreateAnAcknowledgement_HtmlOverridesDefault()
		{
			var ack = new AcknowledgementAttribute("testKey") { Name = "testName", Html = "<myOwnStuff>Bob</myOwnStuff>"};
			Assert.AreEqual("<myOwnStuff>Bob</myOwnStuff>", ack.Html, "default html should be overridden");
		}

		[TestCase("Key1", "myName", "", "SIL 2007", "GPLUrl", "defaultLocation",
			ExpectedResult = "<li>myName: SIL 2007 <a href='GPLUrl'>GPLUrl</a></li>")] // test name, copyright and license
		[TestCase("Key2", "myName", "", "", "", "",
			ExpectedResult = "<li>myName</li>")] // test optionality on all but name; ctor can't test FileVersionInfo stuff
		[TestCase("Key3", "myName", "", "", "myLicenseUrl", "",
			ExpectedResult = "<li>myName <a href='myLicenseUrl'>myLicenseUrl</a></li>")] // test license, w/o copyright
		[TestCase("Key4", "myName", "", "Bob", "", "some location",
			ExpectedResult = "<li>myName: Bob</li>")] // test (unused) location and copyright
		[TestCase("Key4", "myName", "www.fakeUrl", "Bob", "", "some location",
			ExpectedResult = "<li><a href='www.fakeUrl'>myName</a>: Bob</li>")] // test url/name interaction
		public string CreateAnAcknowledgement_TestDefaultHtml(string key, string name, string url,
			string copyright, string license, string location)
		{
			// Apparently even if we make the last 4 parameters above optional, the [TestCase] attribute
			// needs to have values for all 6 to keep from being ignored.
			return new AcknowledgementAttribute(key) { Name = name, Url = url, LicenseUrl = license, Copyright = copyright, Location = location}.Html;
		}

		[Test]
		public void CreateAnAcknowledgement_NoCopyright_OverriddenByFile()
		{
			var ack = new AcknowledgementAttribute("testKey") { Name = "testName",
				Location = GetDllWithPathInTestAssemblyFolder("nunit.framework.dll") } ;
			Assert.That(ack.Copyright, Is.EqualTo("Copyright (c) 2022 Charlie Poole, Rob Prouse"));
		}

		[Test]
		public void CreateAnAcknowledgement_NoName_OverriddenByFile()
		{
			var ack = new AcknowledgementAttribute("testKey") { Copyright = "myCopyright",
				Location = GetDllWithPathInTestAssemblyFolder("nunit.framework.dll") };
			Assert.That(ack.Name, Is.EqualTo("NUnit 3"));
		}

		private static string GetDllWithPathInTestAssemblyFolder(string dllName)
		{
			var testAssemblyFolder =
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Assert.IsNotNull(testAssemblyFolder, "test setup problem");
			var dllPath = Path.Combine(testAssemblyFolder, dllName);
			Assert.True(File.Exists(dllPath), "test setup problem");
			return dllPath;
		}
	}
}
