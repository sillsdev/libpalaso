using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SIL.Xml
{
	public static class XmlSerializationHelper
	{
		#region InternalXmlReader class
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Custom XmlTextReader that can preserve whitespace characters (spaces, tabs, etc.)
		/// that are in XML elements. This allows us to properly handle deserialization of
		/// paragraph runs that contain runs that contain only whitespace characters.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private class InternalXmlReader : XmlTextReader
		{
			private readonly bool m_fKeepWhitespaceInElements;

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Initializes a new instance of the <see cref="InternalXmlReader"/> class.
			/// </summary>
			/// <param name="reader">The stream reader.</param>
			/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
			/// will preserve and return elements that contain only whitespace, otherwise
			/// these elements will be ignored during a deserialization.</param>
			/// --------------------------------------------------------------------------------
			public InternalXmlReader(TextReader reader, bool fKeepWhitespaceInElements) :
				base(reader)
			{
				m_fKeepWhitespaceInElements = fKeepWhitespaceInElements;
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Initializes a new instance of the <see cref="InternalXmlReader"/> class.
			/// </summary>
			/// <param name="filename">The filename.</param>
			/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
			/// will preserve and return elements that contain only whitespace, otherwise
			/// these elements will be ignored during a deserialization.</param>
			/// --------------------------------------------------------------------------------
			public InternalXmlReader(string filename, bool fKeepWhitespaceInElements) :
				base(new StreamReader(filename))
			{
				m_fKeepWhitespaceInElements = fKeepWhitespaceInElements;
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Gets the namespace URI (as defined in the W3C Namespace specification) of the
			/// node on which the reader is positioned.
			/// </summary>
			/// <value></value>
			/// <returns>The namespace URI of the current node; otherwise an empty string.</returns>
			/// --------------------------------------------------------------------------------
			public override string NamespaceURI
			{
				get { return string.Empty; }
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Reads the next node from the stream.
			/// </summary>
			/// --------------------------------------------------------------------------------
			public override bool Read()
			{
				// Since we use this class only for deserialization, catch file not found
				// exceptions for the case when the XML file contains a !DOCTYPE declaration
				// and the specified DTD file is not found. (This is because the base class
				// attempts to open the DTD by merely reading the !DOCTYPE node from the
				// current directory instead of relative to the XML document location.)
				try
				{
					return base.Read();
				}
				catch (FileNotFoundException)
				{
					return true;
				}
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Gets the type of the current node.
			/// </summary>
			/// --------------------------------------------------------------------------------
			public override XmlNodeType NodeType
			{
				get
				{
					if (m_fKeepWhitespaceInElements &&
						(base.NodeType == XmlNodeType.Whitespace || base.NodeType == XmlNodeType.SignificantWhitespace) &&
						Value.IndexOf('\n') < 0 && Value.Trim().Length == 0)
					{
						// We found some whitespace that was most
						// likely whitespace we want to keep.
						return XmlNodeType.Text;
					}

					return base.NodeType;
				}
			}
		}

		#endregion

		#region Methods for XML serializing and deserializing data
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes an object to an XML represented in an array of UTF-8 bytes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static byte[] SerializeToByteArray<T>(T data)
		{
			return SerializeToByteArray(data, false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes an object to an XML represented in an array of UTF-8 bytes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static byte[] SerializeToByteArray<T>(T data, bool omitXmlDeclaration)
		{
			var utf16 = SerializeToString(data, omitXmlDeclaration);
			return Encoding.UTF8.GetBytes(utf16);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes an object to an XML string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string SerializeToString<T>(T data)
		{
			return SerializeToString(data, false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes an object to an XML string. (Of course, the string is a UTF-16 string.)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string SerializeToString<T>(T data, bool omitXmlDeclaration)
		{
			try
			{
				using (var strWriter = new StringWriter())
				{
					var settings = new XmlWriterSettings();
					settings.Indent = true;
					settings.IndentChars = "\t";
					settings.CheckCharacters = true;
					settings.OmitXmlDeclaration = omitXmlDeclaration;

					using (var xmlWriter = XmlWriter.Create(strWriter, settings))
					{
						var nameSpace = new XmlSerializerNamespaces();
						nameSpace.Add(string.Empty, string.Empty);
						var serializer = new XmlSerializer(typeof(T));
						serializer.Serialize(xmlWriter, data, nameSpace);
						xmlWriter.Flush();
						xmlWriter.Close();
						strWriter.Flush();
						strWriter.Close();

						var result = strWriter.ToString();
						return (result == string.Empty ? null : result);
					}
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		public static bool SerializeToFile<T>(string filename, T data)
		{
			return SerializeToFile(filename, data, null);
		}

		/// ------------------------------------------------------------------------------------
		public static bool SerializeToFile<T>(string filename, T data, out Exception e)
		{
			return SerializeToFile(filename, data, null, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes an object to a the specified file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool SerializeToFile<T>(string filename, T data, string rootElementName)
		{
			Exception e;
			return SerializeToFile(filename, data, rootElementName, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes an object to a the specified file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool SerializeToFile<T>(string filename, T data, string rootElementName,
			out Exception e)
		{
			e = null;

			try
			{
				using (TextWriter writer = new StreamWriter(filename))
				{
					XmlSerializerNamespaces nameSpace = new XmlSerializerNamespaces();
					nameSpace.Add(string.Empty, string.Empty);

					XmlSerializer serializer;
					if (string.IsNullOrEmpty(rootElementName))
						serializer = new XmlSerializer(typeof(T));
					else
					{
						var rootAttrib = new XmlRootAttribute();
						rootAttrib.ElementName = rootElementName;
						rootAttrib.IsNullable = true;
						serializer = new XmlSerializer(typeof(T), rootAttrib);
					}

					serializer.Serialize(writer, data, nameSpace);
					writer.Close();
				}

				//var xe = XElement.Load(filename);
				//xe.SetAttributeValue("version", "2.0");
				//xe.Save(filename);
				return true;
			}
			catch (Exception ex)
			{
				e = ex;
				Debug.Fail(ex.Message);
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes the specified data to a string and writes that XML using the specified
		/// writer. Since strings in .Net are UTF16, the serialized XML data string is, of
		/// course, UTF16. Before the string is written it is converted to UTF8. So the
		/// assumption is the writer is expecting UTF8 data.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool SerializeDataAndWriteAsNode<T>(XmlWriter writer, T data)
		{
			string xmlData = SerializeToString(data);

			using (XmlReader reader = XmlReader.Create(new StringReader(xmlData)))
			{
				// Read past declaration and whitespace.
				while (reader.NodeType != XmlNodeType.Element && reader.Read()) { }

				if (!reader.EOF)
				{
					xmlData = reader.ReadOuterXml();
					if (xmlData.Length > 0)
					{
						writer.WriteWhitespace(Environment.NewLine);
						writer.WriteRaw(xmlData);
						writer.WriteWhitespace(Environment.NewLine);
					}

					return true;
				}
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified string to an object of the specified type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromString<T>(string input)
		{
			Exception e;
			return (DeserializeFromString<T>(input, out e));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified string to an object of the specified type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromString<T>(string input, bool fKeepWhitespaceInElements)
		{
			Exception e;
			return (DeserializeFromString<T>(input, fKeepWhitespaceInElements, out e));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified string to an object of the specified type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromString<T>(string input, out Exception e)
		{
			return DeserializeFromString<T>(input, false, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified string to an object of the specified type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromString<T>(string input, bool fKeepWhitespaceInElements,
			out Exception e)
		{
			T data = default(T);
			e = null;

			try
			{
				if (string.IsNullOrEmpty(input))
					return data;

				// Whitespace is not allowed before the XML declaration,
				// so get rid of any that exists.
				input = input.TrimStart();

				using (InternalXmlReader reader = new InternalXmlReader(
					new StringReader(input), fKeepWhitespaceInElements))
				{
					data = DeserializeInternal<T>(reader, null);
				}
			}
			catch (Exception outEx)
			{
				e = outEx;
			}

			return data;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename)
		{
			Exception e;
			return DeserializeFromFile<T>(filename, false, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, string rootElementName)
		{
			Exception e;
			return DeserializeFromFile<T>(filename, rootElementName, false, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
		/// will preserve and return elements that contain only whitespace, otherwise
		/// these elements will be ignored during a deserialization.</param>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, bool fKeepWhitespaceInElements)
		{
			Exception e;
			return DeserializeFromFile<T>(filename, fKeepWhitespaceInElements, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="rootElementName">Name to expect for the root element. This is
		/// good when T is a generic list of some type (e.g. List of strings).</param>
		/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
		/// will preserve and return elements that contain only whitespace, otherwise
		/// these elements will be ignored during a deserialization.</param>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, string rootElementName,
			bool fKeepWhitespaceInElements)
		{
			Exception e;
			return DeserializeFromFile<T>(filename, rootElementName, fKeepWhitespaceInElements, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="e">The exception generated during the deserialization.</param>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, out Exception e)
		{
			return DeserializeFromFile<T>(filename, false, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="rootElementName">Name to expect for the root element. This is
		/// good when T is a generic list of some type (e.g. List of string).</param>
		/// <param name="e">The exception generated during the deserialization.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, string rootElementName,
			out Exception e)
		{
			return DeserializeFromFile<T>(filename, rootElementName, false, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
		/// will preserve and return elements that contain only whitespace, otherwise
		/// these elements will be ignored during a deserialization.</param>
		/// <param name="e">The exception generated during the deserialization.</param>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, bool fKeepWhitespaceInElements,
			out Exception e)
		{
			return DeserializeFromFile<T>(filename, null, fKeepWhitespaceInElements, out e);
		}

		/// --------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="rootElementName">Name to expect for the root element. This is
		/// good when T is a generic list of some type (e.g. List of strings).</param>
		/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
		/// will preserve and return elements that contain only whitespace, otherwise
		/// these elements will be ignored during a deserialization.</param>
		/// <param name="e">The exception generated during the deserialization.</param>
		/// <returns></returns>
		/// --------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, string rootElementName,
			bool fKeepWhitespaceInElements, out Exception e)
		{
			T data = default(T);
			e = null;

			try
			{
				if (!File.Exists(filename))
					return data;

				using (InternalXmlReader reader = new InternalXmlReader(
					filename, fKeepWhitespaceInElements))
				{
					data = DeserializeInternal<T>(reader, rootElementName);
				}
			}
			catch (Exception outEx)
			{
				e = outEx;
			}

			return data;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes an object using the specified reader.
		/// </summary>
		/// <typeparam name="T">The type of object to deserialize</typeparam>
		/// <param name="reader">The reader.</param>
		/// <param name="rootElementName">Name to expect for the of the root element.</param>
		/// <returns>The deserialized object</returns>
		/// ------------------------------------------------------------------------------------
		private static T DeserializeInternal<T>(XmlReader reader, string rootElementName)
		{
			XmlSerializer deserializer;

			if (string.IsNullOrEmpty(rootElementName))
				deserializer = new XmlSerializer(typeof(T));
			else
			{
				var rootAttrib = new XmlRootAttribute();
				rootAttrib.ElementName = rootElementName;
				rootAttrib.IsNullable = true;
				deserializer = new XmlSerializer(typeof(T), rootAttrib);
			}

			deserializer.UnknownAttribute += deserializer_UnknownAttribute;
			return (T)deserializer.Deserialize(reader);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the UnknownAttribute event of the deserializer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Xml.Serialization.XmlAttributeEventArgs"/>
		/// instance containing the event data.</param>
		/// ------------------------------------------------------------------------------------
		static void deserializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			if (e.Attr.LocalName == "lang")
			{
				// This is special handling for the xml:lang attribute that is used to specify
				// the WS for the current paragraph, run in a paragraph, etc. The XmlTextReader
				// treats xml:lang as a special case and basically skips over it (but it does
				// set the current XmlLang to the specified value). This keeps the deserializer
				// from getting xml:lang as an attribute which keeps us from getting these values.
				// The fix for this is to look at the object that is being deserialized and,
				// using reflection, see if it has any fields that have an XmlAttribute looking
				// for the xml:lang and setting it to the value we get here. (TE-8328)
				object obj = e.ObjectBeingDeserialized;
				Type type = obj.GetType();
				foreach (FieldInfo field in type.GetFields())
				{
					object[] bla = field.GetCustomAttributes(typeof(XmlAttributeAttribute), false);
					if (bla.Length == 1 && ((XmlAttributeAttribute)bla[0]).AttributeName == "xml:lang")
					{
						field.SetValue(obj, e.Attr.Value);
						return;
					}
				}

				foreach (PropertyInfo prop in type.GetProperties())
				{
					object[] bla = prop.GetCustomAttributes(typeof(XmlAttributeAttribute), false);
					if (bla.Length == 1 && ((XmlAttributeAttribute)bla[0]).AttributeName == "xml:lang")
					{
						prop.SetValue(obj, e.Attr.Value, null);
						return;
					}
				}
			}
		}

		#endregion
	}
}
