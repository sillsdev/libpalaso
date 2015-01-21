using System;
using System.IO;
using NUnit.Framework;
using Palaso.IO;

namespace Palaso.Tests.IO
{
	[TestFixture]
	public class TempFileTests
	{
		[Test]
		public void CreateWithFilenameThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => TempFile.WithFilename(null));
		}

		[Test]
		public void CreateWithFilenameThrowsOnEmptyString()
		{
			Assert.Throws<ArgumentException>(() => TempFile.WithFilename(string.Empty));
		}

		[Test]
		public void CreateWithFilenameThrowsOnOnlyWhitespaceString()
		{
			Assert.Throws<ArgumentException>(() => TempFile.WithFilename(" \t" + Environment.NewLine));
		}

		[Test]
		public void CreateWithFilenameAndExtensionSucceeds()
		{
			const string filename = "MyFile.xml";
			var expectedPathname = Path.Combine(Path.GetTempPath(), filename);
			Assert.IsFalse(File.Exists(expectedPathname));
			using (var specifiedTempFile = TempFile.WithFilename(filename))
			{
				Assert.IsTrue(File.Exists(expectedPathname));
				Assert.AreEqual(expectedPathname, specifiedTempFile.Path);
			}
			Assert.IsFalse(File.Exists(expectedPathname));
		}

		[Test]
		public void CreateWithFilenameAndNoExtensionSucceeds()
		{
			const string filename = "MyFile";
			var expectedPathname = Path.Combine(Path.GetTempPath(), filename);
			Assert.IsFalse(File.Exists(expectedPathname));
			using (var specifiedTempFile = TempFile.WithFilename(filename))
			{
				Assert.IsTrue(File.Exists(expectedPathname));
				Assert.AreEqual(expectedPathname, specifiedTempFile.Path);
			}
			Assert.IsFalse(File.Exists(expectedPathname));
		}

		[Test]
		public void WithFilenameInTempFolder_CreatesAndDeletes()
		{
			var tempFile = TempFile.WithFilenameInTempFolder("myFile.txt");
			Assert.That(Path.GetFileName(tempFile.Path), Is.EqualTo("myFile.txt"));
			var directoryName = Path.GetDirectoryName(tempFile.Path);
			Assert.That(Path.GetDirectoryName(directoryName) + Path.DirectorySeparatorChar, Is.EqualTo(Path.GetTempPath()));
			var extra = Path.Combine(directoryName, "extra.txt");
			File.WriteAllText(extra, "this is a test");
			tempFile.Dispose();
			Assert.That(Directory.Exists(directoryName), Is.False);
		}

		[Test]
		public void CreateAndGetPathButDontMakeTheFile_ReturnsUniqueFilenames()
		{
			using (var file1 = TempFile.CreateAndGetPathButDontMakeTheFile())
			using (var file2 = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				Assert.That(file1.Path, Is.Not.EqualTo(file2.Path));
			}
		}
	}
}