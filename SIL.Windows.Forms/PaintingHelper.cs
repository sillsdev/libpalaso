using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms
{
	/// ------------------------------------------------------------------------------------
	/// <summary>
	/// Possible painting states for DrawHotBackground
	/// </summary>
	/// ------------------------------------------------------------------------------------
	public enum PaintState
	{
		Normal,
		Hot,
		HotDown,
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Contains misc. static methods for various customized painting.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class PaintingHelper
	{
		#region OS-specific stuff

		[DllImport("user32.dll", EntryPoint = "GetWindowDC")]
		private static extern IntPtr GetWindowDCWindows(IntPtr hwnd);

		private static IntPtr GetWindowDCLinux(IntPtr hwnd)
		{
			Console.WriteLine("Warning--using unimplemented method GetWindowDC"); // FIXME Linux
			return(IntPtr.Zero);
		}

		public static IntPtr GetWindowDC(IntPtr hwnd)
		{
			return Platform.IsWindows ? GetWindowDCWindows(hwnd) : GetWindowDCLinux(hwnd);
		}

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		private static extern int ReleaseDCWindows(IntPtr hwnd, IntPtr hdc);

		private static int ReleaseDCLinux(IntPtr hwnd, IntPtr hdc)
		{
			Console.WriteLine("Warning--using unimplemented method ReleaseDC"); // FIXME Linux
			return(-1);
		}

		public static int ReleaseDC(IntPtr hwnd, IntPtr hdc)
		{
			return Platform.IsWindows ? ReleaseDCWindows(hwnd, hdc) : ReleaseDCLinux(hwnd, hdc);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetParent")]
		private static extern IntPtr GetParentWindows(IntPtr hWnd);

		private static IntPtr GetParentLinux(IntPtr hWnd)
		{
			Console.WriteLine("Warning--using unimplemented method GetParent"); // FIXME Linux
			return(IntPtr.Zero);
		}

		public static IntPtr GetParent(IntPtr hWnd)
		{
			return Platform.IsWindows ? GetParentWindows(hWnd) : GetParentLinux(hWnd);
		}

		#endregion

		public static int WM_NCPAINT = 0x85;
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a darkened version of the specified image.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Image MakeHotImage(Image img)
		{
			if (img == null)
				return null;

			float[][] colorMatrixElements =
			{
				new[] {0.6f, 0, 0, 0, 0},
				new[] {0, 0.6f, 0, 0, 0},
				new[] {0, 0, 0.6f, 0, 0},
				new[] {0, 0, 0, 1f, 0},
				new[] {0.1f, 0.1f, 0.1f, 0, 1}
			};

			img = img.Clone() as Image;

			using (var imgattr = new ImageAttributes())
			using (var g = Graphics.FromImage(img))
			{
				var cm = new ColorMatrix(colorMatrixElements);
				var rc = new Rectangle(0, 0, img.Width, img.Height);
				imgattr.SetColorMatrix(cm);
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(img, rc, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgattr);
				return img;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws around the specified control, a fixed single border the color of text
		/// boxes in a themed environment. If themes are not enabled, the border is black.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawCustomBorder(Control ctrl)
		{
			DrawCustomBorder(ctrl, CanPaintVisualStyle() ?
				VisualStyleInformation.TextControlBorder : Color.Black);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws around the specified control, a fixed single border of the specified color.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawCustomBorder(Control ctrl, Color clrBorder)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
				// FIXME Linux - is custom border needed on Linux?
			} else { // Windows
				IntPtr hdc = GetWindowDC(ctrl.Handle);

				if (hdc != IntPtr.Zero) {
					using (Graphics g = Graphics.FromHdc(hdc))
					{
						Rectangle rc = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
						ControlPaint.DrawBorder(g, rc, clrBorder, ButtonBorderStyle.Solid);
					}
					ReleaseDC(ctrl.Handle, hdc);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws a background in the specified rectangle that looks like a toolbar button
		/// when the mouse is over it, with consideration for whether the look should be like
		/// the mouse is down or not. Note, when a PaintState of normal is specified, this
		/// method does nothing. Normal background painting is up to the caller.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawHotBackground(Graphics g, Rectangle rc, PaintState state)
		{
			// The caller has to handle painting when the state is normal.
			if (state == PaintState.Normal)
				return;

			var hotDown = (state == PaintState.HotDown);

			var clr1 = (hotDown ? ProfessionalColors.ButtonPressedGradientBegin :
				ProfessionalColors.ButtonSelectedGradientBegin);

			var clr2 = (hotDown ? ProfessionalColors.ButtonPressedGradientEnd :
				 ProfessionalColors.ButtonSelectedGradientEnd);

			using (var br = new LinearGradientBrush(rc, clr1, clr2, 90))
					g.FillRectangle(br, rc);

			var clrBrdr = (hotDown ? ProfessionalColors.ButtonPressedHighlightBorder :
				ProfessionalColors.ButtonSelectedHighlightBorder);

			ControlPaint.DrawBorder(g, rc, clrBrdr, ButtonBorderStyle.Solid);

			//// Determine the highlight color.
			//Color clrHot = (CanPaintVisualStyle() ?
			//    VisualStyleInformation.ControlHighlightHot : SystemColors.MenuHighlight);

			//int alpha = (CanPaintVisualStyle() ? 95 : 120);

			//// Determine the angle and one of the colors for the gradient highlight. When state is
			//// hot down, the gradiant goes from bottom (lighter) to top (darker). When the state
			//// is just hot, the gradient is from top (lighter) to bottom (darker).
			//float angle = (state == PaintState.HotDown ? 270 : 90);
			//Color clr2 = ColorHelper.CalculateColor(Color.White, clrHot, alpha);

			//// Draw the label's background.
			//if (state == PaintState.Hot)
			//{
			//    using (LinearGradientBrush br = new LinearGradientBrush(rc, Color.White, clr2, angle))
			//        g.FillRectangle(br, rc);
			//}
			//else
			//{
			//    using (LinearGradientBrush br = new LinearGradientBrush(rc, clr2, clrHot, angle))
			//        g.FillRectangle(br, rc);
			//}

			//// Draw a black border around the label.
			//ControlPaint.DrawBorder(g, rc, Color.Black, ButtonBorderStyle.Solid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a value indicating whether or not visual style rendering is supported
		/// in the application and if the specified element can be rendered.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool CanPaintVisualStyle(VisualStyleElement element)
		{
			return (CanPaintVisualStyle() && VisualStyleRenderer.IsElementDefined(element));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a value indicating whether or not visual style rendering is supported
		/// in the application.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool CanPaintVisualStyle()
		{
			return (Application.VisualStyleState != VisualStyleState.NoneEnabled &&
				VisualStyleInformation.IsSupportedByOS &&
				VisualStyleInformation.IsEnabledByUser &&
				VisualStyleRenderer.IsSupported);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Because the popup containers forces a little padding above and below, we need to get
		/// the popup's parent (which is the popup container) and paint its background to match
		/// the menu color.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Graphics PaintDropDownContainer(IntPtr hwnd, bool returnGraphics)
		{
			var hwndParent = GetParent(hwnd);
			var g = Graphics.FromHwnd(hwndParent);
			var rc = g.VisibleClipBounds;
			rc.Inflate(-1, -1);
			g.FillRectangle(SystemBrushes.Menu, rc);

			if (!returnGraphics)
			{
				g.Dispose();
				g = null;
			}

			return g;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Fills the specified rectangle with a gradient background consistent with the
		/// current system's color scheme.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc)
		{
			DrawGradientBackground(g, rc, false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Fills the specified rectangle with a gradient background consistent with the
		/// current system's color scheme.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc, bool makeDark)
		{
			Color clrTop;
			Color clrBottom;

			if (makeDark)
			{
				clrTop = ColorHelper.CalculateColor(Color.White,
					SystemColors.ActiveCaption, 70);

				clrBottom = ColorHelper.CalculateColor(SystemColors.ActiveCaption,
					SystemColors.ActiveCaption, 0);
			}
			else
			{
				clrTop = ColorHelper.CalculateColor(Color.White,
					SystemColors.GradientActiveCaption, 190);

				clrBottom = ColorHelper.CalculateColor(SystemColors.ActiveCaption,
					SystemColors.GradientActiveCaption, 50);
			}

			DrawGradientBackground(g, rc, clrTop, clrBottom);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Fills the specified rectangle with a gradient background using the specified
		/// colors.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc, Color clrTop, Color clrBottom)
		{
			try
			{
				if (rc.Width > 0 && rc.Height > 0)
				{
					// Use 89 degrees here instead of 90 because otherwise I noticed sometimes
					// the first row of pixels at the top of the rectangle would be painted with
					// the gradient's bottom color. This was noticed when painting in a
					// DataGridView and I think the problem only manifested itself when the
					// height of the rectangle exceeded a certain amount (which I did not
					// determine).
					using (var br = new LinearGradientBrush(rc, clrTop, clrBottom, 89))
						g.FillRectangle(br, rc);
				}
			}
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc, Color clrTop,
			Color clrBottom, bool useDefaultBlend)
		{
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			if (!useDefaultBlend)
				DrawGradientBackground(g, rc, clrTop, clrBottom);
			else
			{
				var blend = new Blend();
				blend.Positions = new[] { 0.0f, 0.25f, 1.0f };
				blend.Factors = new[] { 0.3f, 0.1f, 1.0f };
				DrawGradientBackground(g, rc, clrTop, clrBottom, blend);
			}
		}

		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc, Color clrTop,
			Color clrBottom, Blend blend)
		{
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			// Use 89 degrees here instead of 90 because otherwise I noticed sometimes
			// the first row of pixels at the top of the rectangle would be painted with
			// the gradient's bottom color. This was noticed when painting in a
			// DataGridView and I think the problem only manifested itself when the
			// height of the rectangle exceeded a certain amount (which I did not
			// determine).
			using (var br = new LinearGradientBrush(rc, clrTop, clrBottom, 89))
			{
				br.Blend = blend;
				g.FillRectangle(br, rc);
			}
		}
	}

	/// ----------------------------------------------------------------------------------------
	public class NoToolStripBorderRenderer : ToolStripProfessionalRenderer
	{
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			// Eat this event.
		}
	}
}
