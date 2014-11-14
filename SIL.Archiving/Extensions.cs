﻿using System;
using System.Text;
using System.Windows.Forms;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	public static class Extensions
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Used to size a link label in mono because it is not working at all
		/// </summary>
		/// <param name="linkLabel"></param>
		/// ------------------------------------------------------------------------------------
		public static void SizeToContents(this LinkLabel linkLabel)
		{
			var w = linkLabel.ClientSize.Width;

			using (var g = linkLabel.CreateGraphics())
			{

				if (ArchivingDlgViewModel.IsMono)
				{
					// split at the existing like breaks
					var segments = linkLabel.Text.Replace("\r", "").Split(new[] {'\n'}, StringSplitOptions.None);
					var newText = new StringBuilder();

					foreach (var segment in segments)
					{
						var thisSegment = segment.Trim();

						while (g.MeasureString(thisSegment, linkLabel.Font).Width > w)
						{
							var line = string.Empty;
							var lastSpace = 0;

							for (var i = 0; i < thisSegment.Length; i++)
							{
								if (char.IsWhiteSpace(thisSegment[i]))
								{
									if (g.MeasureString(line, linkLabel.Font).Width > w)
									{
										newText.AppendLine(thisSegment.Substring(0, lastSpace));
										thisSegment = thisSegment.Substring(lastSpace + 1);
										break;
									}

									lastSpace = i;
								}
								line += thisSegment[i];
							}
						}

						// check for left-overs
						if (thisSegment.Length > 0)
							newText.AppendLine(thisSegment);
					}

					linkLabel.Text = newText.ToString();
				}

				var size = g.MeasureString(linkLabel.Text, linkLabel.Font, w);
				linkLabel.Height = (int)Math.Ceiling(size.Height);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Combines the functionality fo StringBuilder.AppendFormat and
		/// StringBuilder.AppendLine. Also allows for the delimiter to be specified.  If the
		/// delimiter is null, Environment.NewLine will be used.
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <param name="delimiter"></param>
		/// ------------------------------------------------------------------------------------
		public static void AppendLineFormat(this StringBuilder sb, string format, object[] args, string delimiter)
		{
			if (delimiter == null) delimiter = Environment.NewLine;
			if (sb.Length != 0) sb.Append(delimiter);
			sb.AppendFormat(format, args);
		}
	}
}
