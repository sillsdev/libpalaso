using System.Drawing;
using System.Drawing.Drawing2D;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.ImageToolbox.ImageGallery
{
	public static class ImageUtilities
	{
		public static Image GetThumbNail(string imagePath, int destinationWidth, int destinationHeight, Color borderColor, Color backgroundColor)
		{

			Bitmap bmp;

			try
			{
				bmp = new Bitmap(imagePath);
			}
			catch
			{
				bmp = new Bitmap(destinationWidth, destinationHeight); //If we cant load the image, create a blank one with ThumbSize
			}

			//get the lesser of the desired and original size
			destinationWidth = bmp.Width > destinationWidth ? destinationWidth : bmp.Width;
			destinationHeight = bmp.Height > destinationHeight ? destinationHeight : bmp.Height;

			var actualWidth = destinationWidth;
			var actualHeight = destinationHeight;

			if (bmp.Width > bmp.Height)
				actualHeight = (int)((bmp.Height / (float)bmp.Width) * actualWidth);
			else if (bmp.Width < bmp.Height)
				actualWidth = (int)((bmp.Width / (float)bmp.Height) * actualHeight);

			var horizontalOffset = (destinationWidth / 2) - (actualWidth / 2);
			var verticalOffset = (destinationHeight / 2) - (actualHeight / 2);

			var retBmp = Platform.IsWindows
				? new Bitmap(destinationWidth, destinationHeight, System.Drawing.Imaging.PixelFormat.Format64bppPArgb)
				: new Bitmap(destinationWidth, destinationHeight);

			using (var grp = Graphics.FromImage(retBmp))
			{
				grp.PixelOffsetMode = PixelOffsetMode.None;
				grp.InterpolationMode = InterpolationMode.HighQualityBicubic;

				// fill with background color
				using (var b = new SolidBrush(backgroundColor))
				{
					grp.FillRectangle(b, 0, 0, destinationWidth, destinationHeight);
				}

				// draw the image
				grp.DrawImage(bmp, horizontalOffset, verticalOffset, actualWidth, actualHeight);

				// draw border
				using (var pn = new Pen(borderColor, 1))
				{
					grp.DrawRectangle(pn, 0, 0, retBmp.Width - 1, retBmp.Height - 1);
				}
			}

			return retBmp;
		}

		public static Image GetThumbNail(string imagePath, int destinationWidth, int destinationHeight, Color borderColor)
		{
			return GetThumbNail(imagePath, destinationWidth, destinationHeight, borderColor, Color.White);
		}
	}
}
