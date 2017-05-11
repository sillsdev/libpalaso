// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIL.ExtractCopyright
{
	/// <summary>
	/// This class is used to read and write Debian style control files.
	/// See https://www.debian.org/doc/debian-policy/ch-controlfields.
	/// </summary>
	public class DebianControl
	{
		enum ReadingState { BeginParagraph, FieldStarted };

		protected string _filepath;

		/// <summary>
		/// A Debian control file consists of one or more "paragraphs".
		/// </summary>
		public List<DebianParagraph> Paragraphs = new List<DebianParagraph>();

		public DebianControl()
		{
		}

		public DebianControl(string filepath)
		{
			ReadDebianControlFile(filepath);
		}

		public DebianParagraph FindParagraph(string tag, string value)
		{
			foreach (var p in Paragraphs)
			{
				if (p.Fields.Count > 0 && p.Fields[0].Tag == tag && p.Fields[0].Value == value)
					return p;
			}
			return null;
		}

		public void ReadDebianControlFile(string filepath)
		{
			if (!File.Exists(filepath))
				return;
			_filepath = filepath;
			using (var reader = new StreamReader(_filepath, Encoding.UTF8))
			{
				ReadDebianParagraphs(reader);
			}
		}

		public void ReadDebianParagraphs(TextReader reader)
		{
			string line;
			int lineNumber = 0;
			var state = ReadingState.BeginParagraph;
			DebianParagraph paragraph = null;
			DebianField field = null;
			while ((line = reader.ReadLine()) != null)
			{
				++lineNumber;
				if (line.Trim().Length == 0)
				{
					state = ReadingState.BeginParagraph;
					continue;
				}
				switch (state)
				{
				case ReadingState.BeginParagraph:
					if (Char.IsLetter(line[0]))
					{
						paragraph = new DebianParagraph();
						Paragraphs.Add(paragraph);
						field = new DebianField(line);
						paragraph.Fields.Add(field);
						state = ReadingState.FieldStarted;
					}
					else
					{
						Console.WriteLine("ERROR");
					}
					break;

				case ReadingState.FieldStarted:
					if (line[0] == ' ' || line[0] == '\t')	// I've only ever seen spaces, but the standard allows tabs.
					{
						field.FullDescription.Add(line.Substring(1));
					}
					else
					{
						field = new DebianField(line);
						paragraph.Fields.Add(field);
					}
					break;
				}
			}
		}

		public void WriteDebianControlFile()
		{
			using (var writer = new StreamWriter(_filepath, false, Encoding.UTF8))
			{
				WriteDebianParagraphs(writer);
			}
		}

		public void WriteDebianParagraphs(TextWriter writer)
		{
			for (int i = 0; i < Paragraphs.Count; ++i)
			{
				if (i > 0)
					writer.WriteLine();
				for (int j = 0; j < Paragraphs[i].Fields.Count; ++j)
				{
					writer.WriteLine("{0}: {1}", Paragraphs[i].Fields[j].Tag, Paragraphs[i].Fields[j].Value);
					for (int k = 0; k < Paragraphs[i].Fields[j].FullDescription.Count; ++k)
					{
						var line = Paragraphs[i].Fields[j].FullDescription[k];
						if (line.Length == 0)
							line = ".";
						writer.WriteLine(" {0}", line);
					}
				}
			}
		}

	}


	public class DebianField
	{
		public string Tag;
		public string Value;
		public List<string> FullDescription = new List<string>();

		public DebianField(string line)
		{
			var tagEnd = line.IndexOf(':');
			if (tagEnd <= 0)
			{
				Console.WriteLine("ERROR");
			}
			else
			{
				Tag = line.Substring(0, tagEnd);
				Value = line.Substring(tagEnd+1).Trim();
			}
		}

		public DebianField(string tag, string value)
		{
			Tag = tag;
			Value = value;
		}

		public override string ToString ()
		{
			return string.Format ("[{0}: {1}] + {2} lines of description", Tag, Value, FullDescription.Count);
		}
	}

	public class DebianParagraph
	{
		public List<DebianField> Fields = new List<DebianField>();

		public DebianParagraph()
		{
		}

		public override string ToString ()
		{
			if (Fields.Count == 0)
				return string.Format ("[empty CopyrightFileParagraph]");
			else
				return string.Format("[Fields[0] = {0}: {1}] + {2} more fields", Fields[0].Tag, Fields[0].Value, Fields.Count - 1);
		}

		public DebianField FindField(string tag)
		{
			foreach (var f in Fields)
			{
				if (f.Tag == tag)
					return f;
			}
			return null;
		}
	}
}
