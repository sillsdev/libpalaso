using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Palaso.Extensions;
using Palaso.Xml;

namespace Palaso.WritingSystems
{
	public class WritingSystemChangeLogDataMapper : IWritingSystemChangeLogDataMapper
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
		//      <Delete Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-05T13:15:30Z'>
		//          <Id>bbb</Id>
		//      </Delete>
		//      <Add Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-05T13:15:30Z'>
		//          <Id>ddd</Id>
		//      </Add>
		//  </Changes>
		//  </WritingSystemsChangeLog>

		///</summary>
		public WritingSystemChangeLogDataMapper(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			FilePath = filePath;
			_tempWriteFilePath = filePath + "_appended";

			// xmlreader settings
			_xmlReadSettings = new XmlReaderSettings();
			_xmlReadSettings.ValidationType = ValidationType.None;
			_xmlReadSettings.XmlResolver = null;
			_xmlReadSettings.ProhibitDtd = false;
			_xmlReadSettings.IgnoreWhitespace = true;
		}

		private XmlReaderSettings _xmlReadSettings;
		private string _tempWriteFilePath;
		public string FilePath { get; set; }

		public void Read(WritingSystemChangeLog log)
		{

			if (File.Exists(FilePath))
			{
				using (StreamReader streamReader = File.OpenText(FilePath))
				{
					using (XmlReader reader = XmlReader.Create(streamReader, _xmlReadSettings))
					{
						ReadLog(reader, log);
					}
				}
			}
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
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Changes"))
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name)
					{
						case "Change":
							ReadChangeElement(reader, log);
							break;
						case "Add":
							ReadAddElement(reader, log);
							break;
						case "Delete":
							ReadDeleteElement(reader, log);
							break;
					}
				}
			}
		}

		private static void ReadChangeElement(XmlReader reader, WritingSystemChangeLog log)
		{
			AssertOnElement(reader, "Change");
			string producer = reader.GetAttribute("Producer") ?? string.Empty;
			string producerVersion = reader.GetAttribute("ProducerVersion") ?? string.Empty;
			string dateTimeString = reader.GetAttribute("TimeStamp") ?? string.Empty;
			var dateTime = DateTime.Parse(dateTimeString);

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
			log.AddEvent(new WritingSystemLogChangeEvent(from, to) {DateTime = dateTime, Producer = producer, ProducerVersion = producerVersion});
		}

		private static void ReadAddElement(XmlReader reader, WritingSystemChangeLog log)
		{
			AssertOnElement(reader, "Add");
			string producer = reader.GetAttribute("Producer") ?? string.Empty;
			string producerVersion = reader.GetAttribute("ProducerVersion") ?? string.Empty;
			string dateTimeString = reader.GetAttribute("TimeStamp") ?? string.Empty;
			var dateTime = DateTime.Parse(dateTimeString);

			string id = "";
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Add"))
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name)
					{
						case "Id":
							reader.Read(); // get to the text node
							id = reader.Value;
							break;
					}
				}
			}
			log.AddEvent(new WritingSystemLogAddEvent(id) { DateTime = dateTime, Producer = producer, ProducerVersion = producerVersion });
		}

		private static void ReadDeleteElement(XmlReader reader, WritingSystemChangeLog log)
		{
			AssertOnElement(reader, "Delete");
			string producer = reader.GetAttribute("Producer") ?? string.Empty;
			string producerVersion = reader.GetAttribute("ProducerVersion") ?? string.Empty;
			string dateTimeString = reader.GetAttribute("TimeStamp") ?? string.Empty;
			var dateTime = DateTime.Parse(dateTimeString);

			string id = "";
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Delete"))
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name)
					{
						case "Id":
							reader.Read(); // get to the text node
							id = reader.Value;
							break;
					}
				}
			}
			log.AddEvent(new WritingSystemLogDeleteEvent(id) { DateTime = dateTime, Producer = producer, ProducerVersion = producerVersion });
		}

		public void AppendEvent(WritingSystemLogEvent logEvent)
		{
			using (StreamReader streamReader = File.OpenText(FilePath))
			{
				using (XmlReader reader = XmlReader.Create(streamReader, _xmlReadSettings))
				{
					using (var streamWriter = new StreamWriter(_tempWriteFilePath))
					{
						using (var writer = XmlWriter.Create(streamWriter, CanonicalXmlSettings.CreateXmlWriterSettings()))
						{
							CopyUntilEndOfChanges(reader, writer);
							WriteLogEvent(writer, logEvent);
							writer.WriteEndElement(); // Changes
							writer.WriteEndElement(); // WritingSystemChangeLog
							writer.Close();
						}
					}
				}
			}
			File.Delete(FilePath);
			File.Move(_tempWriteFilePath, FilePath);
		}

		public void Write(WritingSystemChangeLog log)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (!File.Exists(FilePath))
			{
				string logDirectory = Path.GetDirectoryName(FilePath);
				Directory.CreateDirectory(logDirectory);
			}


			using (var streamWriter = new StreamWriter(FilePath))
			{
				using (var writer = XmlWriter.Create(streamWriter, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("WritingSystemChangeLog");
					writer.WriteAttributeString("Version", log.Version);
					writer.WriteStartElement("Changes");
					WriteChanges(writer, log);
					writer.WriteEndElement(); // Changes
					writer.WriteEndElement(); // WritingSystemChangeLog
					writer.Close();
				}
			}
		}

		private static void CopyUntilEndOfChanges(XmlReader reader, XmlWriter writer)
		{
			Debug.Assert(writer != null);
			Debug.Assert(reader != null);
			reader.MoveToContent();
			while (!(reader.EOF || reader.NodeType == XmlNodeType.EndElement && reader.Name == "Changes"))
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					writer.WriteStartElement(reader.Name);
					writer.WriteAttributes(reader, false);
				}
				else if (reader.NodeType == XmlNodeType.Text)
				{
					writer.WriteString(reader.Value);
				}
				else if (reader.NodeType == XmlNodeType.EndElement)
				{
					writer.WriteEndElement();
				}
				reader.Read();
			}
		}

		private static void WriteLogEvent(XmlWriter writer, WritingSystemLogEvent logEvent)
		{
			writer.WriteStartElement(logEvent.Type);
			writer.WriteAttributeString("Producer", logEvent.Producer);
			writer.WriteAttributeString("ProducerVersion", logEvent.ProducerVersion);
			writer.WriteAttributeString("TimeStamp", logEvent.DateTime.ToISO8601DateAndUTCTimeString());
			switch (logEvent.Type)
			{
				case "Change":
					var changeEvent = (WritingSystemLogChangeEvent)logEvent;
					writer.WriteElementString("From", changeEvent.From);
					writer.WriteElementString("To", changeEvent.To);
					break;
				case "Delete":
					var deleteEvent = (WritingSystemLogDeleteEvent)logEvent;
					writer.WriteElementString("Id", deleteEvent.Id);
					break;
				case "Add":
					var addEvent = (WritingSystemLogAddEvent)logEvent;
					writer.WriteElementString("Id", addEvent.Id);
					break;
			}
			writer.WriteEndElement();
		}

		private static void WriteChanges(XmlWriter writer, WritingSystemChangeLog log)
		{
			foreach (var logEvent in log.Events)
			{
				WriteLogEvent(writer, logEvent);
			}
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
