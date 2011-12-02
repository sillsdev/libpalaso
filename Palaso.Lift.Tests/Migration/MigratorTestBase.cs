using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Palaso.Lift.Tests.Migration
{
	public class MigratorTestBase
	{
		public void AssertXPathNotFound(string xpath, string filePath)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(filePath);
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				Console.WriteLine(File.ReadAllText(filePath));
			}
			AssertXPathNotFound(doc, xpath);
		}

		public void AssertXPathNotFound(XmlDocument doc, string xpath)
		{
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node != null)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNull(node);
		}

		public void AssertXPathAtLeastOne(string xpath, string filePath)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(filePath);
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				Console.WriteLine(File.ReadAllText(filePath));
			}
			AssertXPathAtLeastOne(doc, xpath);
		}

		public void AssertXPathAtLeastOne(XmlDocument doc, string xpath)
		{
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
}