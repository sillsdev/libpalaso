using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class TempFileForSafeWritingTests
	{
		[Test]
		public void WriteWasSuccessful_TargetDidNotExist_TargetHasContents()
		{
			_WriteWasSuccessful_TargetDidNotExist_TargetHasContents(false);
		}

		// Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		[Test]
		public void ReplaceFails_WriteWasSuccessful_TargetDidNotExist_TargetHasContents()
		{
			_WriteWasSuccessful_TargetDidNotExist_TargetHasContents(true);
		}

		private static void _WriteWasSuccessful_TargetDidNotExist_TargetHasContents(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "hello");
			f.WriteWasSuccessful();
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("hello"));
		}


		[Test]
		public void WriteWasSuccessful_TargetDidExist_TargetHasContents()
		{
			_WriteWasSuccessful_TargetDidExist_TargetHasContents(false);
		}


		// Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		[Test]
		public void ReplaceFails_WriteWasSuccessful_TargetDidExist_TargetHasContents()
		{
			_WriteWasSuccessful_TargetDidExist_TargetHasContents(true);
		}

		private static void _WriteWasSuccessful_TargetDidExist_TargetHasContents(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			File.WriteAllText(targetPath, "old");
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "new");
			f.WriteWasSuccessful();
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new"));
		}


		public void WriteWasSuccessful_TargetAndBakcupDidNotExist_BackupFileStillDoesNotExist()
		{
			_WriteWasSuccessful_TargetAndBakcupDidNotExist_BackupFileStillDoesNotExist(false);
		}

		// Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		[Test]
		public void ReplaceFails_WriteWasSuccessful_TargetAndBakcupDidNotExist_BackupFileStillDoesNotExist()
		{
			_WriteWasSuccessful_TargetAndBakcupDidNotExist_BackupFileStillDoesNotExist(true);
		}

		private static void _WriteWasSuccessful_TargetAndBakcupDidNotExist_BackupFileStillDoesNotExist(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			File.Delete(targetPath);
			var backup = targetPath + ".bak";
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "new");
			f.WriteWasSuccessful();
			Assert.That(File.Exists(backup), Is.False);
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new"));
		}

		[Test]
		public void WriteWasSuccessful_BackupDidNotExist_HasContentsOfReplacedFile()
		{
			_WriteWasSuccessful_BackupDidNotExist_HasContentsOfReplacedFile(false);
		}

		// Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		[Test]
		public void ReplaceFails_WriteWasSuccessful_BackupDidNotExist_HasContentsOfReplacedFile()
		{
			_WriteWasSuccessful_BackupDidNotExist_HasContentsOfReplacedFile(true);
		}

		private static void _WriteWasSuccessful_BackupDidNotExist_HasContentsOfReplacedFile(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			var backup = targetPath + ".bak";
			File.WriteAllText(targetPath, "old");
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "new");
			f.WriteWasSuccessful();
			Assert.That(File.ReadAllText(backup), Is.EqualTo("old"));
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new"));
		}

		[Test]
		public void WriteWasSuccessful_BackupExistedButTargetDidNot_BackupLeftAlone()
		{
			_WriteWasSuccessful_BackupExistedButTargetDidNot_BackupLeftAlone(false);
		}

		// Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		[Test]
		public void ReplaceFails_WriteWasSuccessful_BackupExistedButTargetDidNot_BackupLeftAlone()
		{
			_WriteWasSuccessful_BackupExistedButTargetDidNot_BackupLeftAlone(true);
		}

		private static void _WriteWasSuccessful_BackupExistedButTargetDidNot_BackupLeftAlone(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			File.Delete(targetPath);
			var backup = targetPath + ".bak";
			File.WriteAllText(backup, "old bak");
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "new");
			f.WriteWasSuccessful();
			Assert.That(File.ReadAllText(backup), Is.EqualTo("old bak"));
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new"));
		}

		[Test]
		public void WriteWasSuccessful_BackupDidExist_HasContentsOfReplacedFile()
		{
			_WriteWasSuccessful_BackupDidExist_HasContentsOfReplacedFile(false);
		}

		// Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		[Test]
		public void ReplaceFails_WriteWasSuccessful_BackupDidExist_HasContentsOfReplacedFile()
		{
			_WriteWasSuccessful_BackupDidExist_HasContentsOfReplacedFile(true);
		}

		private static void _WriteWasSuccessful_BackupDidExist_HasContentsOfReplacedFile(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			var backup = targetPath + ".bak";
			File.WriteAllText(backup, "ancient");
			File.WriteAllText(targetPath, "old");
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "new");
			f.WriteWasSuccessful();

			Assert.That(File.ReadAllText(backup), Is.EqualTo("old"));
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new"));
		}
	}
}