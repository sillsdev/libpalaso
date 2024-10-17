// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
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

		public override string ToString ()
		{
			return Fields.Count == 0 ? "[empty DebianParagraph]" :
				$"[Fields[0] = {Fields[0].Tag}: {Fields[0].Value}] + {Fields.Count - 1} more fields";
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
