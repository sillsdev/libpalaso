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

		[Test]
		// In case we're ever running tests in parallel, setting NamePrefix at a random
		// point in some other test could cause problems.
		[Parallelizable(ParallelScope.None)]
		public void WithNamePrefix_CreatesFileStartingWithThat()
		{
			TempFile.NamePrefix = "MyWonderfulTest";
			var temp = new TempFile();
			Assert.That(Path.GetFileName(temp.Path), Does.StartWith("MyWonderfulTest"));
			temp.Dispose();
			TempFile.NamePrefix = null;
		}

		[Test]
		// In case we're ever running tests in parallel, setting NamePrefix at a random
		// point in some other test could cause problems.
		[Parallelizable(ParallelScope.None)]
		public void WithExtension_Works()
		{
			TempFile.NamePrefix = null; // make sure it works with no NamePrefix
			var temp = TempFile.WithExtension("xfgj");
			Assert.That(Path.GetExtension(temp.Path), Is.EqualTo(".xfgj"));
			temp.Dispose();
		}

		[Test]
		// In case we're ever running tests in parallel, setting NamePrefix at a random
		// point in some other test could cause problems.
		[Parallelizable(ParallelScope.None)]
		public void WithNamePrefixAndExtension_CreatesFileStartingWithThat()
		{
			TempFile.NamePrefix = "MyWonderfulTest";
			var temp = TempFile.WithExtension("xfgj");
			Assert.That(Path.GetFileName(temp.Path), Does.StartWith("MyWonderfulTest"));
			Assert.That(Path.GetExtension(temp.Path), Is.EqualTo(".xfgj"));
			temp.Dispose();
			TempFile.NamePrefix = null;
		}

		[Test]
		// In case we're ever running tests in parallel, setting NamePrefix at a random
		// point in some other test could cause problems.
		[Parallelizable(ParallelScope.None)]
		public void CleanupTempFolder_RemovesFilesAndDirectories()
		{
			TempFile.NamePrefix = "MyWonderfulTest";
			var temp1 = new TempFile();
			var temp2 = new TempFile();
			RobustFile.Delete(temp2.Path);
			Directory.CreateDirectory(temp2.Path);
			var childPath = Path.Combine(temp2.Path, "abcde.txt");
			File.WriteAllText(childPath, "this is trash");
			TempFile.CleanupTempFolder();
			Assert.That(RobustFile.Exists(temp1.Path), Is.False);
			Assert.That(Directory.Exists(temp2.Path), Is.False);
			TempFile.NamePrefix = null;
		}
	}
}