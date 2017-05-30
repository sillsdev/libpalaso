/*
 * Class to represent metadata (just GUIDs at the moment) about files.
 *
 * Originally from John Hall <john.hall@xjtag.com>. It was named "Metadata.cs"
 * Hatton says: This is used to keep the same GUID for each item,
 * even though we recreate the wix file. I've cleaned it up some, but haven't
 * looked at everything it is doing.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;

namespace SIL.BuildTasks.MakeWixForDirTree
{
	internal class IdToGuidDatabase
	{
		private ILogger _logger;
		private string _filename;
		private bool _changed = false;
		private Dictionary<string, string> _guids = new Dictionary<string, string>();


		#region Construction

		private IdToGuidDatabase(string filename, ILogger logger)
		{
			_filename = filename;
			_logger = logger;
		}

		public static IdToGuidDatabase Create(string filename, ILogger owner)
		{
			if (File.Exists(filename))
			{
				//  try
				{
					XmlReaderSettings settings = new XmlReaderSettings();
					settings.IgnoreComments = true;
					settings.IgnoreWhitespace = true;
					using (XmlReader rdr = XmlTextReader.Create(filename, settings))
					{
						IdToGuidDatabase m = new IdToGuidDatabase(filename, owner);

						// skip XML declaration
						do
						{
							if (!rdr.Read())
								throw new XmlException("Unexpected EOF");
						} while (rdr.NodeType != XmlNodeType.Element);

						if (rdr.Name == "InstallerMetadata")
						{
							while (rdr.Read())
							{
								if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "File")
								{
									string id = rdr.GetAttribute("Id");
									string guid = rdr.GetAttribute("Guid");
									if (id == null || guid == null)
										throw new XmlException("Unexpected format");
									m[id] = guid;
								}
								else if (rdr.NodeType == XmlNodeType.EndElement)
								{
									break;
								}
								else
								{
									throw new XmlException("Unexpected format");
								}
							}
						}

						return m;
					}
				}
//                catch (Exception e)
//                {
//                    // fallthrough
//                    throw e;
//                }
			}

			return new IdToGuidDatabase(filename, owner);
		}

		#endregion



		public bool Changed
		{
			get { return _changed; }
		}

		public string FileName
		{
			get { return _filename; }
		}

		private string this[string id]
		{
			get
			{
				string ret;
				return _guids.TryGetValue(id, out ret) ? ret : null;
			}
			set
			{
				_guids[id] = value;
				_changed = true;
			}
		}


		#region Methods

		public string GetGuid(string id, bool justCheckDontCreate)
		{
			string guid = this[id];

			if (guid == null)
			{
				if (justCheckDontCreate)
				{
					_logger.LogError("No GUID for " + id + " in " + _filename);
					// on an error we do not save the generated GUID
				}
				else
				{
					_logger.LogMessage(MessageImportance.Low, "No GUID for " + id + " in " + _filename);
					guid = Guid.NewGuid().ToString();
					this[id] = guid;
					Write();
				}
			}

			return guid.ToUpper();
		}

		private void Write()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "  ";
			settings.Encoding = Encoding.UTF8;

			using (XmlWriter writer = XmlTextWriter.Create(_filename, settings))
			{
				writer.WriteComment("This file is generated and then updated by an MSBuild task.  It preserves the automatically-generated guids assigned files that will be installed on user machines. So it should be held in source control.");
				writer.WriteStartElement("InstallerMetadata");
				foreach (string id in _guids.Keys)
				{
					writer.WriteStartElement("File");
					writer.WriteAttributeString("Id", id);
					writer.WriteAttributeString("Guid", _guids[id]);
					writer.WriteEndElement();
				}
				writer.WriteEndElement(); // end InstallerMetadata
			}
		}

		#endregion
	}

	public interface ILogger
	{
		void LogErrorFromException(Exception e);

		void LogError(string s);
		void LogWarning(string s);
		void LogMessage( MessageImportance messageImportance,string s);
	}
}