using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using SIL.Code;
using SIL.IO;
using SIL.Reporting;

namespace SIL.Windows.Forms.ImageToolbox.Cropping
{
	public partial class ImageCropper : UserControl, IImageToolboxControl
	{
		private PalasoImage _image;
		private Grip _gripperBeingDragged;
		private Rectangle _sourceImageArea;
		private const int GripThickness = 20;
		private const int GripLength = 80;
		private const int BorderSize = GripThickness;
		private Grip _bottomGrip;
		private Grip _topGrip;
		private Grip _leftGrip;
		private Grip _rightGrip;

		/// <summary>
		/// Used to mark the spot where the user first started dragging the mouse, when he clicks somewhere other than one of the grips.
		/// We use this to create the new crop rectangle as he continues the drag.
		/// </summary>
		private Point _startOfDrag = default(Point);

		//we will be cropping the image, so we need to keep the original lest we be cropping the crop, so to speak
		private ImageFormat _originalFormat;
		private TempFile _savedOriginalImage;
		private Image _croppingImage;

		private const int MarginAroundPicture = GripThickness;
		private const int MinDistanceBetweenGrips = 20;
		private bool didReportThatUserCameInHere;

		public ImageCropper()
		{
			InitializeComponent();

			Application.Idle += new EventHandler(Application_Idle);
		}

		private void Application_Idle(object sender, EventArgs e)
		{
			if (_image == null)
				return;

			if (_startOfDrag != default(Point))
			{
				DoCropDrag();
			}
			else if (_gripperBeingDragged != null)
			{
				DoGripDrag();
			}
			Invalidate();
		}

		private void DoGripDrag()
		{
			Point mouse = PointToClient(MousePosition);
			if (_gripperBeingDragged.MovesVertically)
			{
				_gripperBeingDragged.Value = mouse.Y - MarginAroundPicture;

				//adjust the vertical position of other axis' grips
				foreach (var grip in HorizontalControlGrips)
				{
					grip.UpdateRectangle();
				}
			}
			else
			{
				_gripperBeingDragged.Value = mouse.X - MarginAroundPicture;
				foreach (var grip in VerticalControlGrips)
				{
					grip.UpdateRectangle();
				}
			}
			if(!didReportThatUserCameInHere)
			{
				didReportThatUserCameInHere = true;
				UsageReporter.SendNavigationNotice("ImageToolbox:Cropper");
			}
		}

		private void DoCropDrag()
		{
			return;
			// REVIEW: Is this code intentionally disabled?
			// REVIEW (Hasso) 2020.04: looks like hatton started work on this in 2010-12 and gave up in 2012-04
			Grip hStart, vStart, hEnd, vEnd;

			Point mouse = PointToClient(MousePosition);
			if (_startOfDrag.X < mouse.X)
			{
				hStart = _leftGrip;
				hEnd = _rightGrip;
			}
			else
			{
				hEnd = _leftGrip;
				hStart = _rightGrip;
			}

			if (_startOfDrag.Y < mouse.Y)
			{
				vStart = _topGrip;
				vEnd = _bottomGrip;
			}
			else
			{
				vEnd = _topGrip;
				vStart = _bottomGrip;
			}

			hStart.Value = _startOfDrag.X - MarginAroundPicture;
			vStart.Value = _startOfDrag.Y - MarginAroundPicture;
			hEnd.Value = mouse.X - MarginAroundPicture;
			vEnd.Value = mouse.Y - MarginAroundPicture;
		}

		protected int MiddleOfVerticalGrips()
		{
			return _leftGrip.Right + ((_rightGrip.Left - _leftGrip.Right)/2);
		}

		protected int MiddleOfHorizontalGrips()
		{
			return _topGrip.Bottom + ((_bottomGrip.Top - _topGrip.Bottom)/2);
		}

		public PalasoImage Image
		{
			get { return _image; }
			set
			{
				if (value == null)
					return;

				//other code changes the image of this palaso image, at which time the PI disposes of its copy,
				//so we better keep our own.

				// save the original in a temp file instead of an Image object to free up memory
				_savedOriginalImage = TempFile.CreateAndGetPathButDontMakeTheFile();
				value.Image.Save(_savedOriginalImage.Path, ImageFormat.Png);
				
				// make a reasonable sized copy to crop
				if ((value.Image.Width > 1000) || (value.Image.Width > 1000))
				{
					_croppingImage = CreateCroppingImage(value.Image.Height, value.Image.Width);
					
					var srcRect = new Rectangle(0, 0, value.Image.Width, value.Image.Height);
					var destRect = new Rectangle(0, 0, _croppingImage.Width, _croppingImage.Height);

					using (var g = Graphics.FromImage(_croppingImage))
					{
						g.DrawImage(value.Image, destRect, srcRect, GraphicsUnit.Pixel);
					}
				}
				else
				{
					_croppingImage = (Image)value.Image.Clone();
				}

				_image = value;

				CalculateSourceImageArea();
				CreateGrips();

				foreach (var grip in Grips)
				{
					grip.UpdateRectangle();
				}

				Invalidate();
			}
		}

		/// <summary>
		/// To conserve memory we will shrink large images before cropping 
		/// </summary>
		/// <param name="oldHeight"></param>
		/// <param name="oldWidth"></param>
		/// <returns></returns>
		private static Image CreateCroppingImage(int oldHeight, int oldWidth)
		{
			int newHeight;
			int newWidth;

			if (oldHeight > oldWidth)
			{
				newHeight = 800;
				newWidth = newHeight * oldWidth / oldHeight;
			}
			else
			{
				newWidth = 800;
				newHeight = newWidth * oldHeight / oldWidth;
			}

			return new Bitmap(newWidth, newHeight);
		}

		private void CreateGrips()
		{
			_bottomGrip = new Grip(_sourceImageArea.Height, GripLength, GripThickness, Grip.Sides.Bottom,
								   MiddleOfVerticalGrips,
								   () => _topGrip.Value + MinDistanceBetweenGrips,
								   () => _sourceImageArea.Height);

			_topGrip = new Grip(0, GripLength, GripThickness, Grip.Sides.Top,
								MiddleOfVerticalGrips,
								() => 0,
								() => _bottomGrip.Value - MinDistanceBetweenGrips);
			_leftGrip = new Grip(0, GripThickness, GripLength, Grip.Sides.Left,
								 MiddleOfHorizontalGrips,
								 () => 0,
								 () => _rightGrip.Value - MinDistanceBetweenGrips);

			_rightGrip = new Grip(_sourceImageArea.Width, GripThickness, GripLength, Grip.Sides.Right,
								  MiddleOfHorizontalGrips,
								  () => _leftGrip.Value + MinDistanceBetweenGrips,
								  () => _sourceImageArea.Width);
		}


		private void ImageCropper_Resize(object sender, EventArgs e)
		{
			if (_image == null)
				return;

			var old = _sourceImageArea;


			CalculateSourceImageArea();

			if (old.Width == 0 || old.Height == 0)
				return;

			float horizontalGrowthFactor = ((float) _sourceImageArea.Width)/((float) old.Width);
			float verticalGrowthFactor = ((float) _sourceImageArea.Height)/((float) old.Height);

			foreach (var grip in VerticalControlGrips)
			{
				grip.Value = (int) ((float) grip.Value*verticalGrowthFactor);
			}
			foreach (var grip in HorizontalControlGrips)
			{
				grip.Value = (int) ((float) grip.Value*horizontalGrowthFactor);
			}

			foreach (var grip in Grips)
			{
				grip.UpdateRectangle();
			}

			Invalidate();
		}

		private void CalculateSourceImageArea()
		{
			float imageToCanvaseScaleFactor = GetImageToCanvasScaleFactor(_croppingImage);
			_sourceImageArea = new Rectangle(GripThickness, GripThickness,
											 (int)(_croppingImage.Width*imageToCanvaseScaleFactor),
											 (int)(_croppingImage.Height*imageToCanvaseScaleFactor));
		}

		private float GetImageToCanvasScaleFactor(Image img)
		{
			var availArea = new Rectangle(BorderSize, BorderSize, Width - (2*BorderSize), Height - (2*BorderSize));
			float hProportion = (float)availArea.Width / ((float)img.Width);
			float vProportion = (float)availArea.Height / ((float)img.Height);
			return Math.Min(hProportion, vProportion);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_croppingImage == null || _sourceImageArea.Width == 0)
				return;

			try
			{

				e.Graphics.FillRectangle(Brushes.Gray, ClientRectangle);
				e.Graphics.DrawImage(
					_croppingImage,
					_sourceImageArea,
					new Rectangle( // Source
						0, 0,
						_croppingImage.Width, _croppingImage.Height),
					GraphicsUnit.Pixel);

				using (Brush brush = new Pen(Color.FromArgb(150, Color.LightBlue)).Brush)
				{
					e.Graphics.FillRectangle(brush, _leftGrip.InnerEdge, _bottomGrip.InnerEdge,
											 _rightGrip.InnerEdge - _leftGrip.InnerEdge
											 /*this avoids overlapp which makes it twice as light*/
											 , Height - _bottomGrip.InnerEdge);
					e.Graphics.FillRectangle(brush, _leftGrip.InnerEdge, 0, _rightGrip.InnerEdge - _leftGrip.InnerEdge,
											 _topGrip.InnerEdge);
					e.Graphics.FillRectangle(brush, 0, 0, _leftGrip.InnerEdge, Height);
					e.Graphics.FillRectangle(brush, _rightGrip.InnerEdge, 0, Width - _rightGrip.InnerEdge, Height);
				}
				e.Graphics.DrawRectangle(Pens.LightBlue, _leftGrip.Right, _topGrip.Bottom,
										 _rightGrip.Left - _leftGrip.Right, _bottomGrip.Top - _topGrip.Bottom);

				_bottomGrip.Paint(e.Graphics);
				_topGrip.Paint(e.Graphics);
				_leftGrip.Paint(e.Graphics);
				_rightGrip.Paint(e.Graphics);

			}
			catch (Exception error)
			{
				Debug.Fail(error.Message);
				// UserControl does not have UseCompatibleTextRendering.
				TextRenderer.DrawText(e.Graphics, "Error in OnPaint()", SystemFonts.DefaultFont, new Point(20,20), Color.Red);
				//swallow in release build
			}
		}

		private Grip[] Grips
		{
			get { return new Grip[] {_bottomGrip, _topGrip, _leftGrip, _rightGrip}; }
		}

		private Grip[] VerticalControlGrips
		{
			get { return new Grip[] {_bottomGrip, _topGrip}; }
		}

		private Grip[] HorizontalControlGrips
		{
			get { return new Grip[] {_leftGrip, _rightGrip}; }
		}

		private void ImageCropper_MouseDown(object sender, MouseEventArgs e)
		{

			foreach (var grip in Grips)
			{
				if (grip.Contains(e.Location))
				{
					_gripperBeingDragged = grip;
					return;
				}
			}
			_startOfDrag = e.Location;
		}

		private void ImageCropper_MouseUp(object sender, MouseEventArgs e)
		{
			_gripperBeingDragged = null;

			_startOfDrag = default(Point);
			if (ImageChanged != null)
				ImageChanged.Invoke(this, null);
		}

//        public void CheckBug()
//        {
//            var z = 1.0/GetImageToCanvasScaleFactor();
//            double d = z*_leftGrip.Value + z*(_rightGrip.Value - _leftGrip.Value);
//            {
//                var s = "";
//                if (d > _image.Width)
//                    s = " > ImageWidth="+_image.Width;
//                Debug.WriteLine(string.Format("scale={0} z={1} left={2} right={3} left+(right-left)={4} "+s,
//                                              GetImageToCanvasScaleFactor(), z, _leftGrip.Value, _rightGrip.Value, d));
//            }
//        }

		public Image GetCroppedImage()
		{
			if (_image == null || _image.Disposed)
				return null;

			try
			{
				//jpeg = b96b3c  *AE* -0728-11d3-9d7b-0000f81ef32e
				//bitmap = b96b3c  *AA* -0728-11d3-9d7b-0000f81ef32e

				//NB: this worked for tiff and png, but would crash with Out Of Memory for jpegs.
				//This may be because I closed the stream? THe doc says you have to keep that stream open.
				//Also, note that this method, too, lost our jpeg encoding:
				//          return bmp.Clone(selection, _image.PixelFormat);
				//So now, I first copy it, then clone with the bounds of our crop:

				using (var originalImage = new Bitmap(_savedOriginalImage.Path)) //**** here we lose the jpeg rawimageformat, if it's a jpeg. Grrr.
				{
					double z = 1.0 / GetImageToCanvasScaleFactor(originalImage);

					int width = Math.Max(1, _rightGrip.Value - _leftGrip.Value);
					int height = Math.Max(1, _bottomGrip.Value - _topGrip.Value);
					var selection = new Rectangle((int)Math.Round(z * _leftGrip.Value),
												  (int)Math.Round(z * _topGrip.Value),
												  (int)Math.Round(z * width),
												  (int)Math.Round(z * height));

					var cropped = originalImage.Clone(selection, originalImage.PixelFormat); //do the actual cropping

					if (_originalFormat.Guid == ImageFormat.Jpeg.Guid)
					{
						//We've sadly lost our jpeg formatting, so now we encode a new image in jpeg
						using (var stream = new MemoryStream())
						{
							cropped.Save(stream, ImageFormat.Jpeg);
							var oldCropped = cropped;
							cropped = System.Drawing.Image.FromStream(stream) as Bitmap;
							oldCropped.Dispose();
							Require.That(ImageFormat.Jpeg.Guid == cropped.RawFormat.Guid, "lost jpeg formatting");
						}
					}
					return cropped;
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				ErrorReport.NotifyUserOfProblem(e, "Sorry, there was a problem getting the image");
				return null;
			}
		}

		public void SetImage(PalasoImage image)
		{
			if (image == null)
			{
				Image = null;
			}
			else
			{
				_originalFormat = image.Image.RawFormat;
				Image = image;
			}
		}

		public PalasoImage GetImage()
		{
			Image x = GetCroppedImage();
			// BL-5830 somehow user was cropping an image and this PalasoImage was already disposed
			if (x == null || _image.Disposed)
				return null;
			//we want to retain the metdata of the PalasoImage we started with; we just want to update its actual image
			_image.Image = x;

			return _image;
		}

		public event EventHandler ImageChanged;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
					components = null;
				}

				try
				{
					if (_savedOriginalImage != null)
					{
						_savedOriginalImage.Dispose();
						_savedOriginalImage = null;
					}

					if (_croppingImage != null)
					{
						_croppingImage.Dispose();
						_croppingImage = null;
					}
				}
				// BL-2680, somehow user can get in a state where we CAN'T delete a temp file.
				// I think we can afford to just ignore it. One temp file will be leaked.
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
			base.Dispose(disposing);
		}
	}
}
