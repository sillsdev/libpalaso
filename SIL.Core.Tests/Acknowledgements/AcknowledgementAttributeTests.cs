// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Reflection;
using System.Text.RegularExpressions;
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
			// Points at this test assembly itself rather than a third-party DLL: its Copyright
			// comes from Directory.Build.props's <Copyright> property, a value we own and only
			// change deliberately, instead of one that silently drifts on every unrelated
			// dependency bump. The end year in that property gets bumped roughly annually, so
			// rather than hardcode it (needing a matching test edit every bump) or re-derive it
			// dynamically (weakening the assertion), pin the fixed text and require the year to
			// be recent: not older than 3 years (catches "we forgot to bump this"), and not in
			// the future (catches a fat-fingered edit).
			var ack = new AcknowledgementAttribute("testKey") { Name = "testName",
				Location = Assembly.GetExecutingAssembly().Location };

			var match = Regex.Match(ack.Copyright, @"^Copyright © 2010-(\d{4}) SIL Global$");
			Assert.That(match.Success, Is.True, $"Unexpected copyright format: '{ack.Copyright}'");
			var year = int.Parse(match.Groups[1].Value);
			Assert.That(year, Is.InRange(DateTime.Now.Year - 3, DateTime.Now.Year),
				"Copyright end year should be recent; bump it in Directory.Build.props if this is failing because it's stale.");
		}

		[Test]
		public void CreateAnAcknowledgement_NoName_OverriddenByFile()
		{
			// See CreateAnAcknowledgement_NoCopyright_OverriddenByFile: ProductName here comes
			// from Directory.Build.props's <Product> property.
			var ack = new AcknowledgementAttribute("testKey") { Copyright = "myCopyright",
				Location = Assembly.GetExecutingAssembly().Location };
			Assert.That(ack.Name, Is.EqualTo("libpalaso"));
		}
	}
}
