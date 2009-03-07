using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Palaso.UI.WindowsForms.ImageGallery
{
	public static class ImageUtilities
	{
		public static Image GetThumbNail(string imagePath, int imgWidth, int imgHeight, Color borderColor)
		{
			Bitmap bmp;

			try
			{
				bmp = new Bitmap(imagePath);
			}
			catch
			{
				bmp = new Bitmap(imgWidth, imgHeight); //If we cant load the image, create a blank one with ThumbSize
			}


			imgWidth = bmp.Width > imgWidth ? imgWidth : bmp.Width;
			imgHeight = bmp.Height > imgHeight ? imgHeight : bmp.Height;

			Bitmap retBmp = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);

			Graphics grp = Graphics.FromImage(retBmp);


			int tnWidth = imgWidth, tnHeight = imgHeight;

			if (bmp.Width > bmp.Height)
				tnHeight = (int)(((float)bmp.Height / (float)bmp.Width) * tnWidth);
			else if (bmp.Width < bmp.Height)
				tnWidth = (int)(((float)bmp.Width / (float)bmp.Height) * tnHeight);

			int iLeft = (imgWidth / 2) - (tnWidth / 2);
			int iTop = (imgHeight / 2) - (tnHeight / 2);

			grp.PixelOffsetMode = PixelOffsetMode.None;
			grp.InterpolationMode = InterpolationMode.HighQualityBicubic;

			grp.DrawImage(bmp, iLeft, iTop, tnWidth, tnHeight);

			Pen pn = new Pen(borderColor, 1); //Color.Wheat
			grp.DrawRectangle(pn, 0, 0, retBmp.Width - 1, retBmp.Height - 1);

			return retBmp;
		}
	}
}
