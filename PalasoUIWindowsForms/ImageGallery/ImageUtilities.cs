using System.Drawing;
using System.Drawing.Drawing2D;


namespace Palaso.UI.WindowsForms.ImageGallery
{
	public static class ImageUtilities
	{
		public static Image GetThumbNail(string imagePath, int destinationWidth, int destinationHeight, Color borderColor)
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

			int actualWidth = destinationWidth;
			int actualHeight = destinationHeight;

			if (bmp.Width > bmp.Height)
				actualHeight = (int)(((float)bmp.Height / (float)bmp.Width) * actualWidth);
			else if (bmp.Width < bmp.Height)
				actualWidth = (int)(((float)bmp.Width / (float)bmp.Height) * actualHeight);

			int horizontalOffset = (destinationWidth / 2) - (actualWidth / 2);
			int verticalOffset = (destinationHeight / 2) - (actualHeight / 2);

#if MONO
//    this worked but didn't incorporate the offsets, so when it went back to the caller, it got displayed
//            out of proportion.
//            Image x = bmp.GetThumbnailImage(destinationWidth, destinationHeight, callbackOnAbort, System.IntPtr.Zero);
//            return x;


			Bitmap retBmp = new Bitmap(destinationWidth, destinationHeight);//, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);
			Graphics grp = Graphics.FromImage(retBmp);
			//grp.PixelOffsetMode = PixelOffsetMode.None;
		 //guessing that this is the problem?   grp.InterpolationMode = InterpolationMode.HighQualityBicubic;

			grp.DrawImage(bmp, horizontalOffset, verticalOffset, actualWidth, actualHeight);

//            Pen pn = new Pen(borderColor, 1); //Color.Wheat
//
//
//            grp.DrawRectangle(pn, 0, 0, retBmp.Width - 1, retBmp.Height - 1);

			return retBmp;
#else

			Bitmap retBmp = new Bitmap(destinationWidth, destinationHeight, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);
			Graphics grp = Graphics.FromImage(retBmp);
			grp.PixelOffsetMode = PixelOffsetMode.None;
			grp.InterpolationMode = InterpolationMode.HighQualityBicubic;

			grp.DrawImage(bmp, horizontalOffset, verticalOffset, actualWidth, actualHeight);

			Pen pn = new Pen(borderColor, 1); //Color.Wheat


			grp.DrawRectangle(pn, 0, 0, retBmp.Width - 1, retBmp.Height - 1);

			return retBmp;
#endif
		}

		private static bool callbackOnAbort()
		{
			return false;
		}
	}
}
