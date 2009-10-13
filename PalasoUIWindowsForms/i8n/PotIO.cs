using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Palaso.UI.WindowsForms
{
	class PotIO
	{
		private string _projectId;
		private string _msdIdBugsTo;

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

		public void WriteEntry(string key, List<string> comments, StreamWriter writer)
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

		public void WritePotHeader(StreamWriter writer)
		{
			writer.WriteLine("msgid \"\"");
			writer.WriteLine("msgstr \"\"");
			writer.WriteLine("\"Project-Id-Version: {0}\"", ProjectId);
			writer.WriteLine("\"Report-Msgid-Bugs-To: {0}\"", MsdIdBugsTo);

			writer.WriteLine("\"POT-Creation-Date: {0}\"", DateTime.UtcNow.ToString("s"));
			writer.WriteLine("\"PO-Revision-Date: {0}\"", DateTime.UtcNow.ToString("s"));
			writer.WriteLine("\"Last-Translator: \"");
			writer.WriteLine("\"Language-Team: \"");
			writer.WriteLine("\"MIME-Version: 1.0\"");
			writer.WriteLine("\"Content-Type: text/plain; charset=UTF-8\"");
			writer.WriteLine("\"Content-Transfer-Encoding: 8bit\"");
		}
	}
}
