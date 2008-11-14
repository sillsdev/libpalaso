using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Palaso.BuildTasks.MakePot
{
	public class MakePot: Task
	{
		Dictionary<string, List<string>> _entries = new Dictionary<string, List<string>>();
		private string _projectId;
		private string _msdIdBugsTo;
		private string _outputFile;
		private ITaskItem[] _csharpFiles;
		private ITaskItem[] _xmlFiles;
		private string _xpathToStrings;


		public ITaskItem[] CSharpFiles
		{
			get
			{
				return _csharpFiles;
			}
			set
			{
				_csharpFiles = value;
			}
		}

		public ITaskItem[] XmlFiles
		{
			get
			{
				return _xmlFiles;
			}
			set
			{
				_xmlFiles = value;
			}
		}

		[Required]
		public string ProjectId
		{
			get { return _projectId; }
			set { _projectId = value; }
		}

		public string MsdIdBugsTo
		{
			get { return _msdIdBugsTo; }
			set { _msdIdBugsTo = value; }
		}

		[Required]
		public string OutputFile
		{
			get { return _outputFile; }
			set { _outputFile = value; }
		}

		public string XpathToStrings
		{
			get { return _xpathToStrings; }
			set { _xpathToStrings = value; }
		}

		public override bool Execute()
		{
			using (StreamWriter writer = File.CreateText(_outputFile))
			{
				if (_xmlFiles != null)
				{
					foreach (ITaskItem file in _xmlFiles)
					{
						ProcessXmlFile(file);
					}
				}
				if (_csharpFiles != null)
				{
					foreach (ITaskItem file in _csharpFiles)
					{
						ProcessSrcFile(file.ItemSpec);
					}
				}

				WritePotHeader(writer);

				foreach (KeyValuePair<string, List<string>> pair in _entries)
				{
					WriteEntry(pair.Key, pair.Value, writer);
				}

				this.Log.LogMessage(MessageImportance.High, "MakePot wrote " + _entries.Count + " strings to " + OutputFile);
			}
			return true;
		}

		private void WritePotHeader(StreamWriter writer)
		{
			writer.WriteLine("msgid \"\"");
			writer.WriteLine("msgstr \"\"");
			writer.WriteLine("\"Project-Id-Version: {0}\n\"", ProjectId);
			writer.WriteLine("\"Report-Msgid-Bugs-To: {0}\n\"", MsdIdBugsTo);

			writer.WriteLine("\"POT-Creation-Date: {0}\n\"", DateTime.UtcNow.ToString("s"));
			writer.WriteLine("\"PO-Revision-Date: {0}\n\"", DateTime.UtcNow.ToString("s"));
			writer.WriteLine("\"Last-Translator: \n\"");
			writer.WriteLine("\"Language-Team: \n\"");
			writer.WriteLine("\"MIME-Version: 1.0\n\"");
			writer.WriteLine("\"Content-Type: text/plain; charset=UTF-8\n\"");
			writer.WriteLine("\"Content-Transfer-Encoding: 8bit\n\"");
		}


		private void ProcessXmlFile(ITaskItem  fileSpec)
		{
			if (string.IsNullOrEmpty(XpathToStrings))
			{
				this.Log.LogError("You must define XPathToStrings if you include anything in XPathFiles");
				return;
			}
			this.Log.LogMessage("Processing {0}", fileSpec.ItemSpec);
			XmlDocument doc = new XmlDocument();
			doc.Load(fileSpec.ItemSpec);
			foreach (XmlNode node in doc.SelectNodes(XpathToStrings))
			{
				AddStringInstance(node.InnerText, String.Empty);
			}
		}

		private void AddStringInstance(string stringToTranslate, string commentsForTranslator)
		{
			if (!_entries.ContainsKey(stringToTranslate)) //first time we've encountered this string?
			{
				this.Log.LogMessage(MessageImportance.Low, "Found '{0}'", stringToTranslate);
				_entries.Add(stringToTranslate, new List<string>());
			}
			_entries[stringToTranslate].Add(commentsForTranslator);//add this reference
		}

		private void ProcessSrcFile(string filePath)
		{
			this.Log.LogMessage("Processing {0}", filePath);
			string contents = File.ReadAllText(filePath);
			System.Text.RegularExpressions.Regex pattern =
				new System.Text.RegularExpressions.Regex(@"""~([^""]*)""\s*(,\s*""(.*)"")?", System.Text.RegularExpressions.RegexOptions.Compiled);

			foreach (System.Text.RegularExpressions.Match match in pattern.Matches(contents))
			{
				string str = match.Groups[1].Value;
				if (!_entries.ContainsKey(str)) //first time we've encountered this string?
				{
					this.Log.LogMessage(MessageImportance.Low, "Found '{0}'", str);
					_entries.Add(str, new List<string>());
				}
				string comments = "#; " + filePath;
				if (match.Groups.Count >= 3 && match.Groups[3].Length > 0)
				{
					string comment = match.Groups[3].Value;
					this.Log.LogMessage(MessageImportance.Low, "  with comment '{0}'", comment);
					comments += System.Environment.NewLine + "#. " + comment;
				}
				_entries[str].Add(comments);//add this reference
			}
		}

		private void WriteEntry(string key, List<string> comments, StreamWriter writer)
		{
			writer.WriteLine("");
			foreach (string s in comments)
			{
				writer.WriteLine(s);
			}
			key = key.Replace("\"", "\\\"");
			writer.WriteLine("msgid \"" + key + "\"");
			writer.WriteLine("msgstr \"\"");
		}
	}
}