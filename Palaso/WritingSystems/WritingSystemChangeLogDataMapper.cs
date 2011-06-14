using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Palaso.Xml;

namespace Palaso.WritingSystems
{
	public class WritingSystemChangeLogDataMapper
	{
		///<summary>
		/// The WritingSystemChangeLogDataMapper reads a change log having the following typical XML structure:
		///
		/// <WritingSystemChangeLog version='1'>
		//  <Changes>
		//      <Change Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-05T13:15:30Z'>
		//          <From>aaa</from>
		//          <To>ccc</to>
		//      </Change>
		//  </Changes>
		//  </WritingSystemsChangeLog>

		///</summary>
		///<param name="filePath"></param>
		///<returns></returns>
		///<exception cref="ArgumentNullException"></exception>
		public static WritingSystemChangeLog Read(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			var settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.None;
			settings.XmlResolver = null;
			settings.ProhibitDtd = false;
			settings.IgnoreWhitespace = true;
			var log = new WritingSystemChangeLog();
			using (StreamReader streamReader = File.OpenText(filePath))
			{
				using (XmlReader reader = XmlReader.Create(streamReader, settings))
				{
					ReadLog(reader, log);
				}
			}
			return log;
		}

		public static WritingSystemChangeLog Read(XmlReader reader)
		{
			var log = new WritingSystemChangeLog();
			ReadLog(reader, log);
			return log;
		}

		public static WritingSystemChangeLog ReadOrNew(string filePath)
		{
			if (File.Exists(filePath))
			{
				return Read(filePath);
			}
			return new WritingSystemChangeLog();

		}

		private static void ReadLog(XmlReader reader, WritingSystemChangeLog log)
		{
			Debug.Assert(reader != null);
			if (reader.MoveToContent() != XmlNodeType.Element || reader.Name != "WritingSystemChangeLog")
			{
				throw new ApplicationException("Unable to load writing system definition: Missing <WritingSystemChangeLog> tag.");
			}

			log.Version = reader.GetAttribute("Version") ?? string.Empty;
			if (FindStartElement(reader, "Changes"))
			{
				ReadChangesElement(reader, log);
			}
		}

		private static void ReadChangesElement(XmlReader reader, WritingSystemChangeLog log)
		{
			AssertOnElement(reader, "Changes");
			while (FindStartElement(reader, "Change"))
			{
				ReadChangeElement(reader, log);
			}
		}

		private static void ReadChangeElement(XmlReader reader, WritingSystemChangeLog log)
		{
			AssertOnElement(reader, "Change");
			string producer = reader.GetAttribute("Producer") ?? string.Empty;
			string producerVersion = reader.GetAttribute("ProducerVersion") ?? string.Empty;
			string datetime = reader.GetAttribute("TimeStamp") ?? string.Empty;
			string from = "";
			string to = "";
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Change"))
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name)
					{
						case "From":
							reader.Read(); // get to the text node
							from = reader.Value;
							break;
						case "To":
							reader.Read(); // get to the text node
							to = reader.Value;
							break;
					}
				}
			}
			log.Set(from, to, producer, producerVersion, datetime);
		}

		public static void Write(string filePath, WritingSystemChangeLog log)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			using (var streamWriter = new StreamWriter(filePath))
			{
				using (var writer = XmlWriter.Create(streamWriter, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteStartDocument();
					WriteLog(writer, log);
					writer.Close();
				}
			}
		}

		private static void WriteLog(XmlWriter writer, WritingSystemChangeLog log)
		{
			writer.WriteStartElement("WritingSystemChangeLog");
			writer.WriteAttributeString("Version", log.Version);
			WriteChanges(writer, log);
			writer.WriteEndElement();
		}

		private static void WriteChanges(XmlWriter writer, WritingSystemChangeLog log)
		{
			writer.WriteStartElement("Changes");
			foreach (var change in log.Items)
			{
				writer.WriteStartElement("Change");
				writer.WriteAttributeString("Producer", change.Producer);
				writer.WriteAttributeString("ProducerVersion", change.ProducerVersion);
				writer.WriteAttributeString("TimeStamp", change.DateTime);
				writer.WriteElementString("From", change.From);
				writer.WriteElementString("To", change.To);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private static void AssertOnElement(XmlReader reader, string elementName)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == elementName);
		}

		private static bool FindStartElement(XmlReader reader, string elementName)
		{
			while (reader.Read())
			{
				if (reader.Name == elementName && reader.IsStartElement())
				{
					return true;
				}
			}
			return false;
		}
	}
}
