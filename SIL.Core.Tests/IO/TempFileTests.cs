using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Tests.IO
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

		[Test]
		public void InFolderOf_WorksProperly()
		{
			string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string originalPath = Uri.UnescapeDataString(uri.Path);
			string filePath, filePath2;
			using (var file1 = TempFile.InFolderOf(originalPath))
			using (var file2 = TempFile.InFolderOf(originalPath))
			{
				Assert.That(file1.Path, Is.Not.EqualTo(originalPath));
				Assert.That(file2.Path, Is.Not.EqualTo(originalPath));
				Assert.That(file1.Path, Is.Not.EqualTo(file2.Path));
				Assert.That(Path.GetDirectoryName(file1.Path), Is.EqualTo(Path.GetDirectoryName(file2.Path)));
				Assert.That(Path.GetDirectoryName(file1.Path), Is.EqualTo(Path.GetDirectoryName(originalPath)));
				Assert.IsFalse(File.Exists(file1.Path));	// doesn't actually create file
				filePath = file1.Path;
				using (FileStream fs = File.Create(file2.Path))
				{
					Byte[] data = new System.Text.UTF8Encoding(true).GetBytes("This is a test.");
					// Add some information to the file.
					fs.Write(data, 0, data.Length);
				}
				Assert.IsTrue(File.Exists(file2.Path));		// file was created by test code.
				filePath2 = file2.Path;
			}
			Assert.IsFalse(File.Exists(filePath));		// doesn't crash "deleting" non-existent file
			Assert.IsFalse(File.Exists(filePath2));		// deletes file that was created
		}
	}
}