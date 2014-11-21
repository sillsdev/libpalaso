using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Palaso.IO;
using Palaso.UI.WindowsForms.HtmlBrowser;

namespace Palaso.UI.WindowsForms.ImageGallery
{
	/// <summary>
	/// This class can replace a ListViewThumbnailViewer in applications that are using Gecko,
	/// which causes the thumb to stick when scrolling in ListViewThumbnailViewer.
	/// </summary>
	public partial class WebThumbnailViewer : UserControl, IThumbnailViewer
	{
		private XWebBrowser _browser;
		public event EventHandler OnLoadComplete;
		private TempFile _htmlFile;

		public event EventHandler SelectedIndexChanged;

		public WebThumbnailViewer()
		{
			CanLoad = false;
			ThumbBorderColor = Color.Wheat;
			ThumbNailSize = 95;
			InitializeComponent();

			_browser = new XWebBrowser(XWebBrowser.BrowserType.GeckoFx);
			_browser.Dock = DockStyle.Fill;
			Controls.Add(_browser);
			_browser.Navigated += ((sender, args) =>
			{
				if (OnLoadComplete != null)
					OnLoadComplete(this, new EventArgs());
			});
			var browserAdapter = ((GeckoFxWebBrowserAdapter)_browser.Adapter);
			browserAdapter.AddDomClickHandler((id) =>
			{
				HasSelection = true;
				var path = Uri.UnescapeDataString(id);
				if (SelectedPath != path)
				{
					browserAdapter.SetClassOfLastClickTarget("imageWrap");
					browserAdapter.SetClassOfClickTarget("imageWrap selected");
					SelectedPath = path;
					if (SelectedIndexChanged != null)
						SelectedIndexChanged(this, new EventArgs());
				}
			});
		}
		
		public string SelectedPath { get; private set; }

		public int ThumbNailSize { get; set; }

		public Color ThumbBorderColor { get; set; }

		public bool IsLoading
		{
			get { return _browser.IsBusy; }
		}

		public bool CanLoad { get; set; } // Not used here...matching API of the other implementation.

		public Image GetThumbNail(string fileName)
		{
			return ImageUtilities.GetThumbNail(fileName, ThumbNailSize, ThumbNailSize, ThumbBorderColor);
		}

		public void LoadItems(IEnumerable<string> pathList)
		{
			// OK, some of this is weird. I tried a lot of things. The tricky part is getting a reduced-size image
			// to render in the center of a square box of fixed size, keeping its aspect ratio.
			// I tried many things with FlexBox but everything I tried stretches the image vertically to fill the square box.
			// I don't fully understand the approach here; the styles applied to imageWrapOuter, Mid, and Inner are based on
			// http://www.jakpsatweb.cz/css/css-vertical-center-solution.html.
			// The imageWrapRel is there because the trick used to overlay a semi-transparent background on the
			// selected item only works if the parent div of the one overlayed is position:relative, and all the
			// others needed to be something else to make the centering trick work.
			// (It almost works if the :after is applied to imageWrapInner, but that div is moved down for
			// images that are wider than they are high, so the overlay comes in the wrong place.)
			// I could not find any way that works in GeckoFx29 to produce the effect that the ListBox uses,
			// where the image is somehow recolored in shades of blue. Something might be possible with CSS
			// filters, but that's not supported until Gecko 35.
			// Of course it could be done by loading the image into a bitmap and manipulating it, but then we need
			// another temp file to hold the transformed image...seems like too much trouble.
			// There is probably some way to do this without either JavaScript or five layers of div around
			// each image, but I (JohnT) could not find it.
			var sb = new StringBuilder();
			sb.AppendFormat(
@"<!DOCTYPE html>
<html>
<head lang='en'>
    <meta charset='UTF-8'>
	<meta http-equiv='X-UA-Compatible' content='IE=11' >
    <title></title>
    <style>
        body {{margin: 3px}}
        p {{margin:0}}
        div.imageWrap {{border: 1px solid {1}; margin: 0 18px 39px 21px; display: inline-block;}}
		div.imageWrapRel {{position:relative}}
        div.imageWrapOuter {{height: {0}px;width: {0}px;display: table;#position: relative;}}
        div.imageWrapMid {{#position: absolute;#top: 50%;display: table-cell;vertical-align: middle;}}
        div.imageWrapInner {{position: relative;}}
        img.image {{max-height: {0}px;max-width: {0}px;margin: 0 auto;display: block;}}
        div.selected {{border-color: #94BBD9;}}
        div.selected div.imageWrapOuter:after {{content: ' ';position: absolute;left: 0;top:0;height: {0}px;width: {0}px;background-color: rgba(148,187,217,.5);}}
    </style>
</head>
<body>
<p>", ThumbNailSize, ColorToHtmlCode(ThumbBorderColor));
			foreach (string path in pathList)
			{
				var htmlPath = new Uri(path).AbsolutePath;
				sb.AppendLine("<div class='imageWrap' id='" + htmlPath
					+ "'><div class='imageWrapRel'><div class='imageWrapOuter'><div class='imageWrapMid'><div class='imageWrapInner'>");
				sb.AppendLine("<img class='image' src='file://" + htmlPath + "'>");
				sb.AppendLine("</div></div></div></div></div>");
			}
			sb.AppendLine("</p></body></html>");
			// What we want to do here is _browser.NavigateToString(sb.ToString()).
			// But the Windows.Forms WebBrowser class doesn't have that method.
			if (_htmlFile == null)
				_htmlFile = TempFile.WithExtension("htm");
			File.WriteAllText(_htmlFile.Path, sb.ToString(), Encoding.UTF8);
			_browser.Navigate(_htmlFile.Path);
		}

		string ColorToHtmlCode(Color color)
		{
			return string.Format("rgba({0},{1},{2},{3})", color.R, color.G, color.B, color.A/256.0);
		}

		public bool HasSelection { get; private set; }

		public Control TheControl { get { return this; } }

		/// <summary>
		/// Implemented for compatibity with the API of the other implementation, but as it's not needed
		/// for AOR I have not attempted to implement captions.
		/// </summary>
		public CaptionMethodDelegate CaptionMethod
		{
			get; set;
		}

		// For API compatibility, but there's nothing to do here when closing.
		public void Closing()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (_htmlFile != null)
			{
				_htmlFile.Dispose();
				_htmlFile = null;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		public void Clear()
		{
			_browser.DocumentText = "<!DOCTYPE html><html><body></body></html>";
			SelectedPath = null;
			HasSelection = false;
		}
	}
}