// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;

namespace SIL.ExtractCopyright
{
	/// <summary>
	/// This class represents a "paragraph" in a Debian control file, which consists of one or more
	/// "fields".  See https://www.debian.org/doc/debian-policy/ch-controlfields#s-controlsyntax.
	/// </summary>
	public class DebianParagraph
	{
		public List<DebianField> Fields = new List<DebianField>();

		public DebianParagraph()
		{
		}

		public override string ToString ()
		{
			if (Fields.Count == 0)
				return string.Format ("[empty DebianParagraph]");
			else
				return string.Format("[Fields[0] = {0}: {1}] + {2} more fields", Fields[0].Tag, Fields[0].Value, Fields.Count - 1);
		}

		/// <summary>
		/// Return the first field in the paragraph with the given tag, or null if not found.
		/// </summary>
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
