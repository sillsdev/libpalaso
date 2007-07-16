using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Palaso
{
	public class SFMReader
	{
		private TextReader buffer;
		private string _text = "";
		private string _extraTxt = "";

		/// <summary>
		/// Construct a new SFMReader with filename
		/// </summary>
		/// <param name="fname"></param>
		public SFMReader(string fname)
		{
			buffer = new StreamReader(fname);
		}

		/// <summary>
		/// Construct a new SFMReader with stream
		/// </summary>
		/// <param name="stream"></param>
		public SFMReader(Stream stream)
		{
			buffer = new StreamReader(stream);
		}

		/// <summary>
		/// Read next tag and return the name only (exclude backslash
		/// and space).
		/// </summary>
		/// <returns>next tag name</returns>
		public string ReadNextTag()
		{
			// build Regex pattern based on tagname
			Regex reTag = new Regex(@".*?\\([^ ]+)[ ]([^\\]*)(.*)");

			// Use extra text, if necessary
			string line;
			if(_extraTxt.Length>0)
				line = _extraTxt;
			else
				line = buffer.ReadLine();

			// Do matching
			Match m = reTag.Match(line);
			string tag = m.Groups[1].Value;
			_text = m.Groups[2].Value;
			_extraTxt = m.Groups[3].Value;
			return tag;
		}

		/// <summary>
		/// Read next text block from stream
		/// </summary>
		/// <returns>Next text</returns>
		public string ReadNextText()
		{
			if(_extraTxt!="")
			{
				return _text;
			} else
			{
				Regex reTag = new Regex(@"([^\\]*)(.*)");

				// It seems that if end of file, ReadLine returns null
				string line = buffer.ReadLine();
				if (line == null) return _text;

				Match m = reTag.Match(line);
				_text += (_text == "" ? "" : " ");
				_text += m.Groups[1].Value;
				_extraTxt = m.Groups[2].Value;

				if(m.Groups[2].Value!="")
				{   // Return available text
					return _text;
				}
				else
				{
					ReadNextText();
				}
			}
			return _text;
		}

		public LinkedList<string> Tokenize(string txt)
		{
			LinkedList<string> rtn = new LinkedList<string>();
			Regex reWords = new Regex(@"\s*(\S+)");
			Match m = reWords.Match(txt);

			return rtn;
		}
	}
}
