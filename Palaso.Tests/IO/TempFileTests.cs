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
	}
}