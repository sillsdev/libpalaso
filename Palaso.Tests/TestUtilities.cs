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

		//--------------
		public static void AssertXPathNotNull(string documentPath, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			AssertXPathNotNull(doc, xpath);
		}

		public static void AssertXPathNotNull(string documentPath, string xpath, XmlNamespaceManager nameSpaceManager )
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			AssertXPathNotNull(doc, xpath, nameSpaceManager);
		}
		public static void AssertXPathNotNull(XmlNode nodeOrDoc, string xpath, XmlNamespaceManager nameSpaceManager)
		{
			XmlNode node = GetNode(nodeOrDoc, xpath, nameSpaceManager);
			if (node == null)
			{
				PrintNodeToConsole(nodeOrDoc);
			}
			Assert.IsNotNull(node);
		}

		public static void AssertXPathNotNull(XmlNode nodeOrDoc, string xpath)
		{
			XmlNode node = GetNode(nodeOrDoc, xpath, new XmlNamespaceManager(new NameTable()));
			if (node == null)
			{
				PrintNodeToConsole(nodeOrDoc);
			}
			Assert.IsNotNull(node);
		}

		//------------

		public static void AssertXPathIsNull(string documentPath, string xpath, XmlNamespaceManager nameSpaceManager)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			AssertXPathIsNull(doc, xpath, nameSpaceManager);
		}
		public static void AssertXPathIsNull(string documentPath, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(documentPath);
			AssertXPathIsNull(doc, xpath, new XmlNamespaceManager(new NameTable()));
		}

		public static void AssertXPathIsNull(XmlNode nodeOrDoc, string xpath)
		{
			XmlNode node = GetNode(nodeOrDoc, xpath, new XmlNamespaceManager(new NameTable()));
			if (node != null)
			{
				PrintNodeToConsole(nodeOrDoc);
			}
			Assert.IsNull(node);
		}

		public static void AssertXPathIsNull(XmlNode nodeOrDoc, string xpath, XmlNamespaceManager nameSpaceManager)
		{
			XmlNode node = GetNode(nodeOrDoc, xpath, nameSpaceManager);
			if (node != null)
			{
				PrintNodeToConsole(nodeOrDoc);
			}
			Assert.IsNull(node);
		}

		private static void PrintNodeToConsole(XmlNode nodeOrDoc)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
			nodeOrDoc.WriteContentTo(writer);
			writer.Flush();
		}


		private static XmlNode GetNode(XmlNode nodeOrDoc, string xpath, XmlNamespaceManager nameSpaceManager  )
		{
			XmlNode node = nodeOrDoc.SelectSingleNode(xpath, nameSpaceManager);
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
