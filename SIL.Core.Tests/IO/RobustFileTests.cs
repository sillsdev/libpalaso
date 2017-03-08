using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class RobustFileTests
	{

		[Test]
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
		[TestCase("abc")]
		[TestCase("")]
		[TestCase("wyjście𐐷")]
		public void WriteAllTextUtf8(string text)
		{
			byte[] correct = null;
			using (var temp = new TempFile())
			{
				File.WriteAllText(temp.Path, text, Encoding.UTF8);
				correct = File.ReadAllBytes(temp.Path);
			}
			byte[] result;
			using (var temp = new TempFile())
			{
				RobustFile.WriteAllText(temp.Path, text, Encoding.UTF8);
				result = File.ReadAllBytes(temp.Path);
			}
			Assert.That(result, Is.EqualTo(correct));
		}

		[Test]
		[TestCase("abc")]
		[TestCase("")]
		[TestCase("wyjście𐐷")]
		public void WriteAllTextBigEndian(string text)
		{
			byte[] correct = null;
			using (var temp = new TempFile())
			{
				File.WriteAllText(temp.Path, text, Encoding.BigEndianUnicode);
				correct = File.ReadAllBytes(temp.Path);
			}
			byte[] result;
			using (var temp = new TempFile())
			{
				RobustFile.WriteAllText(temp.Path, text, Encoding.BigEndianUnicode);
				result = File.ReadAllBytes(temp.Path);
			}
			Assert.That(result, Is.EqualTo(correct));
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
			byte[] correct = null;
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
					Assert.Throws<IOException>(() => RobustFile.Copy(temp.Path, output.Path));
					RobustFile.WriteAllText(temp.Path, "This is another test");
					RobustFile.Copy(temp.Path, output.Path, true); // overwrite, no exception
					Assert.That(File.ReadAllText(output.Path), Is.EqualTo("This is another test"));

				}
			}
		}
	}
}
