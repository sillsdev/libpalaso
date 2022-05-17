using System.IO;
using NUnit.Framework;
using SIL.IO;
// ReSharper disable LocalizableElement

namespace SIL.Tests.IO
{
	[TestFixture]
	public class TempFileForSafeWritingTests
	{
		[TestCase(false)]
		[TestCase(true)] // Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		public void WriteWasSuccessful_TargetDidNotExist_TargetHasContents(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "hello");
			f.WriteWasSuccessful();
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("hello"));
		}

		[TestCase(false)]
		[TestCase(true)] // Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		public void WriteWasSuccessful_TargetDidExist_TargetHasContents(bool simulateVolumeThatCannotHandleFileReplace)
		{
			var targetPath = Path.GetTempFileName();
			File.WriteAllText(targetPath, "old");
			var f = new TempFileForSafeWriting(targetPath);
			f.SimulateVolumeThatCannotHandleFileReplace = simulateVolumeThatCannotHandleFileReplace;
			File.WriteAllText(f.TempFilePath, "new");
			f.WriteWasSuccessful();
			Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new"));
		}

		[TestCase(false)]
		[TestCase(true)] // Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		public void WriteWasSuccessful_TargetAndBackupDidNotExist_BackupFileStillDoesNotExist(bool simulateVolumeThatCannotHandleFileReplace)
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

		[TestCase(false)]
		[TestCase(true)] // Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		public void WriteWasSuccessful_BackupDidNotExist_HasContentsOfReplacedFile(bool simulateVolumeThatCannotHandleFileReplace)
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

		[TestCase(false)]
		[TestCase(true)] // Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		public void WriteWasSuccessful_BackupExistedButTargetDidNot_BackupLeftAlone(bool simulateVolumeThatCannotHandleFileReplace)
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

		[TestCase(false)]
		[TestCase(true)] // Simulate situation on the JAARS network in 2016, .net's built-in File.Replace fails on the network drives.
		public void WriteWasSuccessful_BackupDidExist_HasContentsOfReplacedFile(bool simulateVolumeThatCannotHandleFileReplace)
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