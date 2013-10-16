using System;
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
#if __MonoCS__

				// split at the existing like breaks
				var segments = linkLabel.Text.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.None);
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
#endif
				var size = g.MeasureString(linkLabel.Text, linkLabel.Font, w);
				linkLabel.Height = (int)Math.Ceiling(size.Height);
			}
		}
	}
}
