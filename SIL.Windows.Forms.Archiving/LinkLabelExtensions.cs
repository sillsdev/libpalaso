using System;
using System.Text;
using System.Windows.Forms;
using SIL.Archiving;

namespace SIL.Windows.Forms.Archiving
{
	/// ------------------------------------------------------------------------------------
	public static class LinkLabelExtensions
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

						while (MeasureText(linkLabel, g, thisSegment, linkLabel.Font).Width > w)
						{
							var line = string.Empty;
							var lastSpace = 0;

							for (var i = 0; i < thisSegment.Length; i++)
							{
								if (char.IsWhiteSpace(thisSegment[i]))
								{
									if (MeasureText(linkLabel, g, line, linkLabel.Font).Width > w)
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

				var size = MeasureText(linkLabel, g, linkLabel.Text, linkLabel.Font, new System.Drawing.Size(w, Int32.MaxValue));
				linkLabel.Height = size.Height;
			}
		}
		private static System.Drawing.Size MeasureText(this LinkLabel linkLabel, System.Drawing.Graphics g, string text, System.Drawing.Font font)
		{
			if (linkLabel.UseCompatibleTextRendering)
				return g.MeasureString(text, font).ToSize();
			else
				return TextRenderer.MeasureText(g, text, font);
		}
		private static System.Drawing.Size MeasureText(this LinkLabel linkLabel, System.Drawing.Graphics g, string text, System.Drawing.Font font, System.Drawing.Size proposedSize)
		{
			if (linkLabel.UseCompatibleTextRendering)
				return g.MeasureString(text, font, proposedSize.Width).ToSize();
			else
				return TextRenderer.MeasureText(g, text, font, proposedSize, TextFormatFlags.WordBreak);
		}
	}
}
