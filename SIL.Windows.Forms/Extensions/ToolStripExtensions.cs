using System.Windows.Forms;

namespace SIL.Windows.Forms.Extensions
{
	public static class ToolStripExtensions
	{
		public static void SizeTextRectangleToText(this ToolStripItemTextRenderEventArgs args)
		{
			var textSize = args.Graphics.MeasureString(args.Text, args.TextFont);
			const int padding = 2;

			var rc = args.TextRectangle;
			var changed = false;

			// adjust the rectangle to fit the calculated text size
			if (rc.Width < textSize.Width + padding)
			{
				var diffX = (int)System.Math.Ceiling(textSize.Width + padding - rc.Width);
				rc.X -= diffX / 2;
				rc.Width += diffX;
				changed = true;
			}

			if (rc.Height < textSize.Height + padding)
			{
				var diffY = (int)System.Math.Ceiling(textSize.Height + padding - rc.Height);
				rc.Y -= diffY / 2;
				rc.Height += diffY;
				changed = true;
			}

			// if nothing changed, return now
			if (!changed) return;

			args.TextRectangle = rc;
		}
	}

	/// <summary>
	/// Use the extension to ToolStripItemTextRenderEventArgs to implement a renderer for
	/// ToolStrip objects that provides absolutely no border on any edge.
	/// </summary>
	/// <remarks>
	/// This code originated in the Bloom project, but proved to be useful for the
	/// ArtOfReadingChooser control in SIL.Windows.Forms.
	/// </remarks>
	public class NoBorderToolStripRenderer : ToolStripProfessionalRenderer
	{
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			// Without this, the ToolStrip renderer leaves some border artifacts
			// even when the style is set to "no border".
			e.SizeTextRectangleToText();
			base.OnRenderItemText(e);
		}
	}
}
