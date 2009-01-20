//Original code from Sreejai R. Kurup, http://www.codeproject.com/KB/miscctrl/C__based_thumbnail_viewer.aspx

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Palaso.UI.WindowsForms.ImageGallery;

namespace WeSay.LexicalTools.AddPictures
{
	public partial class ThumbnailViewer : ListView
	{
		private BackgroundWorker _thumbnailWorker = new BackgroundWorker();

		public event EventHandler OnLoadComplete;


		public string SelectedPath
		{
			get
			{
				if(SelectedItems == null || SelectedItems.Count ==0)
					return null;
				return SelectedItems[0].Tag as string;
			}
		}
		private int thumbNailSize = 95;
		public int ThumbNailSize
		{
			get { return thumbNailSize; }
			set { thumbNailSize = value; }
		}

		private Color thumbBorderColor = Color.Wheat;
		public Color ThumbBorderColor
		{
			get { return thumbBorderColor; }
			set { thumbBorderColor = value; }
		}

		public bool IsLoading
		{
			get { return _thumbnailWorker.IsBusy; }
		}


		private delegate void SetThumbnailDelegate(Image image);
		private void SetThumbnail(Image image)
		{
			if (Disposing) return;

			if (this.InvokeRequired)
			{
				SetThumbnailDelegate d = new SetThumbnailDelegate(SetThumbnail);
				this.Invoke(d, new object[] { image });
			}
			else
			{
				LargeImageList.Images.Add(image); //Images[i].repl
				int index = LargeImageList.Images.Count - 1;
				Items[index - 1].ImageIndex = index;
			}
		}

		private bool canLoad = false;
		public bool CanLoad
		{
			get { return canLoad; }
			set { canLoad = value; }
		}


		public ThumbnailViewer()
		{
			InitializeComponent();
			ImageList il = new ImageList();
			il.ImageSize = new Size(thumbNailSize, thumbNailSize);
			il.ColorDepth = ColorDepth.Depth32Bit;
			il.TransparentColor = Color.White;
			LargeImageList = il;
			components.Add(_thumbnailWorker);
			_thumbnailWorker.WorkerSupportsCancellation = true;
			_thumbnailWorker.DoWork += new DoWorkEventHandler(bwLoadImages_DoWork);
			_thumbnailWorker.WorkerSupportsCancellation = true;
			_thumbnailWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnRunWorkerCompleted);

		}

		void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (OnLoadComplete != null)
				OnLoadComplete(this, new EventArgs());
		}

		public Image GetThumbNail(string fileName)
		{
			return GetThumbNail(fileName, thumbNailSize, thumbNailSize, thumbBorderColor);
		}

		public static Image GetThumbNail(string fileName, int imgWidth, int imgHeight, Color penColor)
		{
			Bitmap bmp;

			try
			{
				bmp = new Bitmap(fileName);
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

			Pen pn = new Pen(penColor, 1); //Color.Wheat
			grp.DrawRectangle(pn, 0, 0, retBmp.Width - 1, retBmp.Height - 1);

			return retBmp;
		}

		private void AddDefaultThumb()
		{
			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(LargeImageList.ImageSize.Width, LargeImageList.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			Graphics grp = Graphics.FromImage(bmp);
			Brush brs = new SolidBrush(Color.White);
			Rectangle rect = new Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1);
			grp.FillRectangle(brs, rect);
			Pen pn = new Pen(Color.Wheat, 1);

			grp.DrawRectangle(pn, 0, 0, bmp.Width - 1, bmp.Height - 1);
			LargeImageList.Images.Add(bmp);
		}

		private void bwLoadImages_DoWork(object sender, DoWorkEventArgs e)
		{
			var fileList = (IList<string>)e.Argument;

			foreach (string fileName in fileList)
			{
				if (_thumbnailWorker.CancellationPending)
				{
					e.Cancel = true;
					return;
				}
				SetThumbnail(GetThumbNail(fileName));
			}
		}

		public void LoadItems(IList<string> pathList)
		{
			if ((_thumbnailWorker != null) && (_thumbnailWorker.IsBusy))
			{
				_thumbnailWorker.CancelAsync();
				DateTime timeOut = DateTime.Now.AddSeconds(3);
				while(_thumbnailWorker.IsBusy && (DateTime.Now.CompareTo(timeOut)<0))
				{
				   //this doesn't allow the thread to actual cancel  Thread.Sleep(100);
					Application.DoEvents();//this, however, is criminal
				}

				if(_thumbnailWorker.IsBusy)
					MessageBox.Show("timeout");
			}

			BeginUpdate();
			Items.Clear();
			LargeImageList.Images.Clear();
			AddDefaultThumb();//what does this do?

			foreach (string path in pathList)
			{
				string caption;
				if(CaptionMethod != null)
				{
					caption = CaptionMethod.Invoke(path);
				}
				else
				{
					caption = System.IO.Path.GetFileName(path);
				}
				ListViewItem liTemp = Items.Add(caption);
				liTemp.ImageIndex = 0;
				liTemp.Tag = path;
			}

			EndUpdate();
			if (_thumbnailWorker != null)
			{
				if (!_thumbnailWorker.CancellationPending)
					_thumbnailWorker.RunWorkerAsync(pathList);
			}
		}

		public CaptionMethodDelegate CaptionMethod
		{
			get; set;
		}
	}
}