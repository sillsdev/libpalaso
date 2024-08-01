using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	[Category("SkipOnTeamCity")]
	class AccessProtocolListTests
	{
		[Test]
		public void AccessProtocols_Load_LoadsProtocols()
		{
			var protocols = AccessProtocols.Load();

			Assert.NotNull(protocols);
			Assert.GreaterOrEqual(protocols.Count, 2);
		}

		[Test]
		public void SetChoicesFromCsv_Null_ThrowsArgumentNullException()
		{
			var arbitraryAP = AccessProtocols.Load().First();
			Assert.That(() => arbitraryAP.SetChoicesFromCsv(null), Throws.ArgumentNullException);
		}

		[Test]
		public void SetChoicesFromCsv_Duplicates_DuplicatesDiscarded()
		{
			var arbitraryAP = AccessProtocols.Load().First();
			arbitraryAP.SetChoicesFromCsv("First , Second, Third, Second ");
			Assert.That(arbitraryAP.Choices.Select(c => c.OptionName), Is.EquivalentTo(new [] {
				"First", "Second", "Third"}));
		}

		[Test]
		public void GetDocumentationUri_ProgramDirectoryNotSpecified_ReturnsRootedPathToExistingFile()
		{
			var uriFilePrefix = $"{System.Uri.UriSchemeFile}:///";
			foreach (var protocol in AccessProtocols.Load())
			{
				var uri = protocol.GetDocumentationUri();
				Assert.True(Uri.IsWellFormedUriString(uri, UriKind.Absolute));
				Assert.That(uri.IndexOf(uriFilePrefix), Is.EqualTo(0));
				var filename = uri.Substring(uriFilePrefix.Length);
				Assert.True(File.Exists(filename));
				Assert.That(Path.GetFileName(uri), Is.EqualTo(protocol.DocumentationFile));
			}
		}
	}
}
