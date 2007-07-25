using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using NUnit.Framework;

namespace Palaso.Tests
{
	public class TestUtilities
	{
		public static string GetTempTestDirectory()
		{
			DirectoryInfo dirProject = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
			return  dirProject.FullName;
		}

		public static void AssertXPathNotNull(string documentPath, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			AssertXPathNotNull(doc, xpath);
		}

		public static void AssertXPathNotNull(XmlNode nodeOrDoc, string xpath)
		{
			Assert.IsNotNull(GetNode(nodeOrDoc, xpath));
		}

		public static void AssertXPathIsNull(XmlNode nodeOrDoc, string xpath)
		{
			Assert.IsNull(GetNode(nodeOrDoc, xpath));
		}

		private static XmlNode GetNode(XmlNode nodeOrDoc, string xpath)
		{
			XmlNode node = nodeOrDoc.SelectSingleNode(xpath);
			if (node == null)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				nodeOrDoc.WriteContentTo(writer);
				writer.Flush();
			}
			return node;
		}

		public static void DeleteFolderThatMayBeInUse(string folder)
		{
			if (Directory.Exists(folder))
			{
				try
				{
					Directory.Delete(folder, true);
				}
				catch (Exception e)
				{
					try
					{
						Console.WriteLine(e.Message);
						//maybe we can at least clear it out a bit
						string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
						foreach (string s in files)
						{
							File.Delete(s);
						}
						//sleep and try again (seems to work)
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
}
