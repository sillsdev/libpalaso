using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;
using static SIL.Xml.XmlSerializationHelper;

namespace SIL.Tests.Xml
{
	[TestFixture]
	public class XmlSerializationHelperTests
	{
		[XmlRoot("MyRoot")]
		public class TestObject
		{
			public TestObject()
			{
			}

			public TestObject(string name)
			{
				TestObjName = name;
				AllThings = new List<Thing>(2) {new Thing {Id = 32}, new Thing {Id = 51}};
			}

			public class Thing
			{
				[XmlAttribute("Id")]
				public int Id { get; set; }
			}

			[XmlAttribute("Name")]
			public string TestObjName { get; set; }

			[XmlElement("ListOfThings")]
			public List<Thing> AllThings { get; set; }
		}

		private class MyStringWriter : TextWriter
		{
			private static bool s_inDispose = false;
			private readonly StringWriter m_internalWriter;
			private bool m_gotDisposed;

			public MyStringWriter(StringBuilder sb)
			{
				m_internalWriter = new StringWriter(sb);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (s_inDispose)
						return;
					s_inDispose = true;
					try
					{
						Assert.IsFalse(m_gotDisposed,
							"It's not illegal to dispose twice, but in these tests, we don't expect it.");
						m_gotDisposed = true;
						m_internalWriter.Dispose();
						base.Dispose();
					}
					finally
					{
						s_inDispose = false;
					}
				}
			}

			public void VerifyDisposed()
			{
				Assert.IsTrue(m_gotDisposed);
			}

			public void VerifyNotDisposed()
			{
				Assert.IsFalse(m_gotDisposed);
			}

			#region override members to pass through to m_internalWriter
			public override Encoding Encoding => m_internalWriter.Encoding;

			public override IFormatProvider FormatProvider => m_internalWriter.FormatProvider;

			public override string NewLine
			{
				get => m_internalWriter.NewLine;
				set => m_internalWriter.NewLine = value;
			}

			public override void Close()
			{
				m_internalWriter.Close();
			}

			public override void Flush()
			{
				m_internalWriter.Flush();
			}

			public override void Write(char value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(char[] buffer)
			{
				m_internalWriter.Write(buffer);
			}

			public override void Write(char[] buffer, int index, int count)
			{
				m_internalWriter.Write(buffer, index, count);
			}

			public override void Write(bool value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(int value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(uint value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(long value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(ulong value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(float value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(double value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(decimal value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(string value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(object value)
			{
				m_internalWriter.Write(value);
			}

			public override void Write(string format, object arg0)
			{
				m_internalWriter.Write(format, arg0);
			}

			public override void Write(string format, object arg0, object arg1)
			{
				m_internalWriter.Write(format, arg0, arg1);
			}

			public override void Write(string format, object arg0, object arg1, object arg2)
			{
				m_internalWriter.Write(format, arg0, arg1, arg2);
			}

			public override void Write(string format, params object[] arg)
			{
				m_internalWriter.Write(format, arg);
			}

			public override void WriteLine()
			{
				m_internalWriter.WriteLine();
			}

			public override void WriteLine(char value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(char[] buffer)
			{
				m_internalWriter.WriteLine(buffer);
			}

			public override void WriteLine(char[] buffer, int index, int count)
			{
				m_internalWriter.WriteLine(buffer, index, count);
			}

			public override void WriteLine(bool value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(int value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(uint value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(long value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(ulong value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(float value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(double value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(decimal value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(string value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(object value)
			{
				m_internalWriter.WriteLine(value);
			}

			public override void WriteLine(string format, object arg0)
			{
				m_internalWriter.WriteLine(format, arg0);
			}

			public override void WriteLine(string format, object arg0, object arg1)
			{
				m_internalWriter.WriteLine(format, arg0, arg1);
			}

			public override void WriteLine(string format, object arg0, object arg1, object arg2)
			{
				m_internalWriter.WriteLine(format, arg0, arg1, arg2);
			}

			public override void WriteLine(string format, params object[] arg)
			{
				m_internalWriter.WriteLine(format, arg);
			}
			#endregion
		}

		private class MyStringReader : TextReader
		{
			private static bool s_inDispose = false;
			private readonly StringReader m_internalReader;
			private bool m_gotDisposed;

			public MyStringReader(string str)
			{
				m_internalReader = new StringReader(str);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (s_inDispose)
						return;
					s_inDispose = true;
					try
					{
						Assert.IsFalse(m_gotDisposed,
							"It's not illegal to dispose twice, but in these tests, we don't expect it.");
						m_gotDisposed = true;
						m_internalReader.Dispose();
						base.Dispose();
					}
					finally
					{
						s_inDispose = false;
					}
				}
			}

			public void VerifyDisposed()
			{
				Assert.IsTrue(m_gotDisposed);
			}

			public void VerifyNotDisposed()
			{
				Assert.IsFalse(m_gotDisposed);
			}

			#region override members to pass through to m_internalReader
			public override void Close()
			{
				m_internalReader.Close();
			}

			public override int Peek()
			{
				return m_internalReader.Peek();
			}

			public override int Read()
			{
				return m_internalReader.Read();
			}

			public override int Read(char[] buffer, int index, int count)
			{
				return m_internalReader.Read(buffer, index, count);
			}

			public override string ReadToEnd()
			{
				return m_internalReader.ReadToEnd();
			}

			public override int ReadBlock(char[] buffer, int index, int count)
			{
				return m_internalReader.ReadBlock(buffer, index, count);
			}

			public override string ReadLine()
			{
				return m_internalReader.ReadLine();
			}
			#endregion
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Deserialize_NullTextReader_GetsDefaultObject(bool dispose)
		{
			Assert.IsNull(Deserialize<object>(null, dispose));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void DeserializeAndSerialize_Normal_RoundTripObject(bool dispose)
		{
			var testObj = new TestObject("Fred");
			var sb = new StringBuilder();
			var stringWriter = new MyStringWriter(sb);
			try
			{
				Serialize(stringWriter, testObj, out var error, null,
					dispose);
				Assert.IsNull(error);

				var textReader = new MyStringReader(sb.ToString());
				try
				{
					var result =
						Deserialize<TestObject>(textReader, dispose);
					Assert.AreEqual(testObj.TestObjName, result.TestObjName);
					Assert.AreEqual(testObj.AllThings.Count, result.AllThings.Count);
					Assert.IsTrue(testObj.AllThings.Select(t => t.Id)
						.SequenceEqual(result.AllThings.Select(t => t.Id)));
				}
				finally
				{
					if (dispose)
						textReader.VerifyDisposed();
					else
					{
						textReader.VerifyNotDisposed();
						textReader.Dispose();
					}
				}
			}
			finally
			{
				if (dispose)
					stringWriter.VerifyDisposed();
				else
				{
					stringWriter.VerifyNotDisposed();
					stringWriter.Dispose();
				}
			}
		}

		[Test]
		public void SerializeToString_NoEncodingSpecified_XmlHeaderHasDefaultUtf16Encoding()
		{
			var result = SerializeToString(new TestObject("Fred"));
			var lines = result.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
			Assert.IsTrue(lines[0].StartsWith("<?xml"));
			Assert.That(lines[0].Contains("encoding=\"utf-16\""));
			Assert.IsTrue(lines[1].StartsWith("<MyRoot"));
			Assert.IsTrue(lines.Last().EndsWith("MyRoot>"));
		}

		[Test]
		public void SerializeToString_EncodingSpecified_XmlHeaderHasExpectedEncoding()
		{
			var result = SerializeToString(new TestObject("Fred"),
				Encoding.UTF8);
			var lines = result.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
			Assert.IsTrue(lines[0].StartsWith("<?xml"));
			Assert.That(lines[0].Contains("encoding=\"utf-8\""));
			Assert.IsTrue(lines[1].StartsWith("<MyRoot"));
			Assert.IsTrue(lines.Last().EndsWith("MyRoot>"));
		}

		[Test]
		public void SerializeToString_OmitXmlHeading_XmlHeaderHasExpectedEncoding()
		{
			var result = SerializeToString(new TestObject("Fred"),
				true);
			Assert.IsFalse(result.StartsWith("<?xml"));
			var lines = result.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
			Assert.IsTrue(lines.First().StartsWith("<MyRoot"));
			Assert.IsTrue(lines.Last().EndsWith("MyRoot>"));
		}

		[Test]
		public void SerializeToFileWithWriteThrough_Normal_FileCreated()
		{
			TemporaryFolder parentFolder = new TemporaryFolder("XmlSerializationHelperTests");
			var path = parentFolder.Combine("test.xml");

			try
			{
				XElement element = new XElement("test");
				SerializeToFileWithWriteThrough(path, element);
				Assert.IsTrue(File.Exists(path));
			}
			finally
			{
				File.Delete(path);
			}
		}

		[Test]
		public void SerializeToFileWithWriteThrough_BogusFile_ErrorReturned()
		{
			XElement element = new XElement("test");
			SerializeToFileWithWriteThrough(@":\....Bogus:path", element, out var error);
			Assert.That(error, Is.Not.Null);
		}

		[Test]
		public void SerializeToFileWithWriteThrough_NullData_DeserializableFileCreated()
		{
			var path = Path.GetTempFileName();
			try
			{
				SerializeToFileWithWriteThrough(path, (XElement)null, out var error);
				Assert.That(error, Is.Null);
				Assert.IsTrue(File.Exists(path));

				var deserializedNullObject = DeserializeFromFile<XElement>(path);
				Assert.That(deserializedNullObject, Is.Null);
			}
			finally
			{
				RobustFile.Delete(path);
			}
		}
	}
}
