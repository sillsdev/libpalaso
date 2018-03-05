// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class RobustFileTests
	{
		[TestCase("abc")]
		[TestCase("")]
		[TestCase("wyjście𐐷")]
		public void WriteAllTextNoBom(string text)
		{
			byte[] correct = null;
			using (var temp = new TempFile())
			{
				File.WriteAllText(temp.Path, text);
				correct = File.ReadAllBytes(temp.Path);
			}
			byte[] result;
			using (var temp = new TempFile())
			{
				RobustFile.WriteAllText(temp.Path, text);
				result = File.ReadAllBytes(temp.Path);
			}
			Assert.That(result, Is.EqualTo(correct));
		}

		[Test]
		public void WriteAllTextExceptions()
		{
			using (var temp = new TempFile())
			{
				Assert.Throws<ArgumentNullException>(() => RobustFile.WriteAllText(temp.Path, null));
				Assert.Throws<ArgumentNullException>(() => RobustFile.WriteAllText(null, "nonsense"));
			}
		}

		[TestCase("abc")]
		[TestCase("wyjście𐐷")]
		// Here empty string gets its own test because it is a special case.
		public void WriteAllTextUtf8(string text)
		{
			WriteAllTextEncoding(text, Encoding.UTF8);
		}

		[TestCase("abc")]
		[TestCase("wyjście𐐷")]
		// Here empty string gets its own test because it is a special case.
		public void WriteAllTextBigEndian(string text)
		{
			WriteAllTextEncoding(text, Encoding.BigEndianUnicode);
		}

		// Unfortunately, Encoding.UTF8 and friends are not constants we can use in TestCase annotations.
		// Hence the two methods above to produce cases of Encoding.
		public void WriteAllTextEncoding(string text, Encoding encoding)
		{
			byte[] correct = null;
			using (var temp = new TempFile())
			{
				File.WriteAllText(temp.Path, text, encoding);
				correct = File.ReadAllBytes(temp.Path);
			}
			byte[] result;
			using (var temp = new TempFile())
			{
				RobustFile.WriteAllText(temp.Path, text, encoding);
				result = File.ReadAllBytes(temp.Path);
			}
			Assert.That(result, Is.EqualTo(correct));
		}

		public void WriteAllText_EmptyString_Encoding()
		{
			byte[] result;
			using (var temp = new TempFile())
			{
				RobustFile.WriteAllText(temp.Path, "", Encoding.UTF8);
				result = File.ReadAllBytes(temp.Path);
			}
			// Linux behaves unexpectedly as of Mono 3.4 and omits the preamble for empty strings.
			// The documentation for the method is unclear...the most natural reading is that
			// an empty content string would throw an exception, but this doesn't happen on
			// either platform.
			// We decided our version should consistently follow the Windows .NET behavior
			// so we don't get the correct value by using the original function in this case.
			Assert.That(result, Is.EqualTo(Encoding.UTF8.GetPreamble()));
		}

		[Test]
		public void WriteAllTextEncodingExceptions()
		{
			using (var temp = new TempFile())
			{
				Assert.Throws<ArgumentNullException>(() => RobustFile.WriteAllText(temp.Path, null, Encoding.UTF8));
				Assert.Throws<ArgumentNullException>(() => RobustFile.WriteAllText(null, "nonsense", Encoding.BigEndianUnicode));
				Assert.Throws<ArgumentNullException>(() => RobustFile.WriteAllText(temp.Path, "nonsense", null));
			}
		}

		[Test]
		public void CreateText()
		{
			byte[] correct = null;
			using (var temp = new TempFile())
			{
				var writer = File.CreateText(temp.Path);
				WriteTestDataToStreamWriter(writer);
				correct = File.ReadAllBytes(temp.Path);
			}
			byte[] result;
			using (var temp = new TempFile())
			{
				var writer = RobustFile.CreateText(temp.Path);
				WriteTestDataToStreamWriter(writer);
				result = File.ReadAllBytes(temp.Path);
			}
			Assert.That(result, Is.EqualTo(correct));
		}

		private static void WriteTestDataToStreamWriter(StreamWriter writer)
		{
			writer.Write("some stuff");
			writer.Write(23);
			writer.WriteLine("done");
			writer.Close();
		}

		[Test]
		public void Create()
		{
			byte[] correct;
			using (var temp = new TempFile())
			{
				var writer = File.Create(temp.Path);
				WriteTestDataToStream(writer);
				correct = File.ReadAllBytes(temp.Path);
			}
			byte[] result;
			using (var temp = new TempFile())
			{
				var writer = RobustFile.Create(temp.Path);
				WriteTestDataToStream(writer);
				result = File.ReadAllBytes(temp.Path);
			}
			Assert.That(result, Is.EqualTo(correct));
		}

		private static void WriteTestDataToStream(Stream writer)
		{
			writer.Write(Encoding.UTF8.GetBytes("some stuff"), 0, 8);
			writer.Write(new byte[] {23, 77}, 0, 2);
			writer.Close();
		}

		[Test]
		public void Copy()
		{
			using (var temp = new TempFile())
			{
				RobustFile.WriteAllText(temp.Path, "This is a test");
				using (var output = new TempFile())
				{
					RobustFile.Delete(output.Path); // apparently we can encounter a left-over temp file
					RobustFile.Copy(temp.Path, output.Path);
					Assert.That(File.ReadAllText(output.Path), Is.EqualTo("This is a test"));
					// output file now exists, w/o 3rd argument can't overwrite
					Assert.Throws<IOException>(() => RobustFile.Copy(temp.Path, output.Path));
					RobustFile.WriteAllText(temp.Path, "This is another test");
					RobustFile.Copy(temp.Path, output.Path, true); // overwrite, no exception
					Assert.That(File.ReadAllText(output.Path), Is.EqualTo("This is another test"));
				}
			}
		}

		[Test]
		public void CopyBiggerThanBuffer()
		{
			using (var temp = new TempFile())
			{
				var bldr = new StringBuilder();
				for (int i = 0; i < 512; i++)
					bldr.AppendLine("This is a test");
				var input = bldr.ToString();
				Assert.That(input.Length > 4096);
				var oldBufSize = RobustFile.BufferSize;
				RobustFile.BufferSize = 4096; // To avoid writing a large file in a unit test
				RobustFile.WriteAllText(temp.Path, input);
				using (var output = new TempFile())
				{
					RobustFile.Delete(output.Path); // apparently we can encounter a left-over temp file
					RobustFile.Copy(temp.Path, output.Path);
					Assert.That(File.ReadAllText(output.Path), Is.EqualTo(input));
				}
				RobustFile.BufferSize = oldBufSize;
			}
		}

		[Test]
		public void CopyEmptyFile()
		{
			// Create an empty file.
			var emptyFile = Path.GetTempFileName();
			Assert.IsTrue(RobustFile.Exists(emptyFile));
			var info = new FileInfo(emptyFile);
			Assert.AreEqual(0, info.Length);

			var destName = emptyFile + "-xyzzy";
			// This was throwing a System.ArgumentOutOfRangeException exception before being fixed.
			RobustFile.Copy(emptyFile, destName, true);
			Assert.IsTrue(RobustFile.Exists(destName));
			var infoDest = new FileInfo(destName);
			Assert.AreEqual(0, infoDest.Length);

			// Clean up after ourselves.
			RobustFile.Delete(emptyFile);
			Assert.IsFalse(RobustFile.Exists(emptyFile));
			RobustFile.Delete(destName);
			Assert.IsFalse(RobustFile.Exists(destName));
		}
	}
}
