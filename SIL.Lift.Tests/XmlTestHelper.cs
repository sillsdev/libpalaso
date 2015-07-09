using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using NUnit.Framework;

namespace SIL.Lift.Tests
{
	public class XmlTestHelper
	{
		public static void AssertXPathMatchesExactlyOne(string xml, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			AssertXPathMatchesExactlyOneInner(doc, xpath);
		}
		public static void AssertXPathMatchesExactlyOne(XmlNode node, string xpath)
		{
			 XmlDocument doc = new XmlDocument();
			doc.LoadXml(node.OuterXml);
			AssertXPathMatchesExactlyOneInner(doc, xpath);
	   }

		private static void AssertXPathMatchesExactlyOneInner(XmlDocument doc, string xpath)
		{
			XmlNodeList nodes = doc.SelectNodes(xpath);
			if (nodes == null || nodes.Count != 1)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
				if (nodes != null && nodes.Count > 1)
				{
					Assert.Fail("Too Many matches for XPath: {0}", xpath);
				}
				else
				{
					Assert.Fail("No Match: XPath failed: {0}", xpath);
				}
			}
		}

		public static void AssertXPathNotNull(string documentPath, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node == null)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNotNull(node);
		}
	}

	public class TempFile : IDisposable
	{
		private string _path;

		public TempFile()
		{
			_path = System.IO.Path.GetTempFileName();
		}


		public TempFile(string contents)
			: this()
		{
			File.WriteAllText(_path, contents);
		}

		public static TempFile CreateWithXmlHeader(string xmlForEntries)
		{
				string content =
					"<?xml version=\"1.0\" encoding=\"utf-8\"?><lift producer=\"test\" >" +
					xmlForEntries + "</lift>";
			return new TempFile(content);

		}

		public string Path
		{
			get { return _path; }
		}
		public void Dispose()
		{
			File.Delete(_path);
		}

		private TempFile(string existingPath, bool dummy)
		{
			_path = existingPath;
		}

		public static TempFile TrackExisting(string path)
		{
			return new TempFile(path, false);
		}
	}

	public class TempFolder : IDisposable
	{
		private readonly string _path;

		public TempFolder(string testName)
		{
			_path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), testName);
			if (Directory.Exists(_path))
			{
				TestUtilities.DeleteFolderThatMayBeInUse(_path);
			}
			Directory.CreateDirectory(_path);
		}

		public string Path
		{
			get { return _path; }
		}

		public void Dispose()
		{
			TestUtilities.DeleteFolderThatMayBeInUse(_path);
		}

		public TempFile GetPathForNewTempFile(bool doCreateTheFile)
		{
			string s = System.IO.Path.GetRandomFileName();
			s = System.IO.Path.Combine(_path, s);
			if (doCreateTheFile)
			{
				File.Create(s).Close();
			}
			return TempFile.TrackExisting(s);
		}

		public string Combine(string innerFileName)
		{
			return System.IO.Path.Combine(_path, innerFileName);
		}
	}
	public class TestUtilities
	{
		public static void DeleteFolderThatMayBeInUse(string folder)
		{
			if (Directory.Exists(folder))
			{
				for (int i = 0; i < 50; i++)//wait up to five seconds
				{
					try
					{
						Directory.Delete(folder, true);
						return;
					}
					catch (Exception)
					{
					}
					Thread.Sleep(100);
				}
				//maybe we can at least clear it out a bit
				try
				{
					Debug.WriteLine("TestUtilities.DeleteFolderThatMayBeInUse(): gave up trying to delete the whole folder. Some files may be abandoned in your temp folder.");

					string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
					foreach (string s in files)
					{
						File.Delete(s);
					}
					//sleep and try again
					Thread.Sleep(1000);
					Directory.Delete(folder, true);
				}
				catch (Exception)
				{
				}

			}
		}
	}
}
