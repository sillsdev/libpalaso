// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;

namespace SIL.ExtractCopyright
{
	/// <summary>
	/// This class represents a "field" in a Debian control file.  It consists of one or more lines of text in a
	/// special format described at https://www.debian.org/doc/debian-policy/ch-controlfields#s-controlsyntax.
	/// </summary>
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
			return string.Format ("[{0}: {1}] + {2} additional lines", Tag, Value, FullDescription.Count);
		}
	}
}
