// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using NUnit.Framework;
using Palaso.IO;

namespace Palaso.Tests.IO
{
	[TestFixture]
	public class PathUtilitiesTests
	{
		[Test]
		public void DeleteToRecycleBin_FileDeleted()
		{
			// Setup
			var file = Path.GetTempFileName();

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(file);

			// Verify
			Assert.That(result, Is.True);
			Assert.That(File.Exists(file), Is.False);
		}

		/// <summary>
		/// Finds the name of the trashed file for <paramref name="filePath"/>
		/// </summary>
		private string GetTrashedFileName(string trashBinPath, string filePath)
		{
			foreach (var possibleTrashedFile in Directory.EnumerateFiles(Path.Combine(trashBinPath, "files")))
			{
				var metaFile = Path.Combine(trashBinPath, "info",
					Path.GetFileName(possibleTrashedFile) + ".trashinfo");
				var metaFileContent = File.ReadAllText(metaFile);
				var lines = metaFileContent.Split('\n');
				if (lines.Length < 2)
					continue;

				if (lines[1].Substring("Path=".Length) == filePath)
					return possibleTrashedFile;
			}
			return null;
		}

		[Test]
		[Platform(Exclude = "Win", Reason="Don't know how to test this on Windows")]
		public void DeleteToRecycleBin_MovedToTrashBin()
		{
			// Setup
			var file = Path.GetTempFileName();
			var content = Guid.NewGuid().ToString();
			File.WriteAllText(file, content);

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(file);

			// Verify
			Assert.That(result, Is.True);
			var trashBinPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Trash");

			var trashedFile = GetTrashedFileName(trashBinPath, file);
			using (TempFile.TrackExisting(trashedFile))
			{
				Assert.That(File.Exists(trashedFile), Is.True);
				Assert.That(File.ReadAllText(trashedFile), Is.EqualTo(content));

				var metaFile = Path.Combine(trashBinPath, "info", Path.GetFileName(trashedFile) + ".trashinfo");
				using (TempFile.TrackExisting(metaFile))
				{
					Assert.That(File.Exists(metaFile), Is.True);

					var metaFileContent = File.ReadAllText(metaFile);
					var lines = metaFileContent.Split('\n');
					Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
					Assert.That(lines[0], Is.EqualTo("[Trash Info]"));
					Assert.That(lines[1], Is.StringStarting("Path="));
					Assert.That(lines[1], Is.StringEnding(file));
					Assert.That(lines[2], Is.StringMatching(@"DeletionDate=\d\d\d\d\d\d\d\dT\d\d:\d\d:\d\d"));
				}
			}
		}

		[Test]
		public void DeleteToRecycleBin_DirectoryDeleted()
		{
			// Setup
			var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(dir);

			var file = Path.Combine(dir, Path.GetRandomFileName());
			File.WriteAllText(file, "Some content");

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(dir);

			// Verify
			Assert.That(result, Is.True);
			Assert.That(File.Exists(file), Is.False);
			Assert.That(Directory.Exists(dir), Is.False);
		}

		[Test]
		public void DeleteToRecycleBin_NonexistingFileReturnsFalse()
		{
			// Setup
			var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(file);

			// Verify
			Assert.That(result, Is.False);
		}

	}
}

