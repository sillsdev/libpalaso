// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class FileHelperTests
	{
		private TemporaryFolder _parentFolder;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			_parentFolder = new TemporaryFolder("FileHelperTests");
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_parentFolder.Dispose();
			_parentFolder = null;
		}

		[Test]
		public void IsLocked_FilePathIsNull_ReturnsFalse()
		{
			Assert.IsFalse(FileHelper.IsLocked(null));
		}

		[Test]
		public void IsLocked_FileDoesntExist_ReturnsFalse()
		{
			Assert.IsFalse(FileHelper.IsLocked(@"c:\blahblah.blah"));
		}

		[Test]
		public void IsLocked_FileExistsAndIsNotLocked_ReturnsFalse()
		{
			using (var file = new TempFileFromFolder(_parentFolder))
				Assert.IsFalse(FileHelper.IsLocked(file.Path));
		}

		[Test]
		public void IsLocked_FileExistsAndIsLocked_ReturnsTrue()
		{
			using (var file = new TempFileFromFolder(_parentFolder))
			{
				var stream = File.OpenWrite(file.Path);
				try
				{
					Assert.IsTrue(FileHelper.IsLocked(file.Path));
				}
				finally
				{
					stream.Close();
				}
			}
		}

		[Test]
		public void Grep_FileContainsPattern_True()
		{
			using (var e = new FileTestEnvironment())
			{
				Assert.That(FileHelper.Grep(e.TempFile.Path, "lang='fr'"), Is.True);
			}
		}


		[Test]
		public void Grep_FileDoesNotContainPattern_False()
		{
			using (var e = new FileTestEnvironment())
			{
				Assert.That(FileHelper.Grep(e.TempFile.Path, "lang='ee'"), Is.False);
			}
		}
	}
}
