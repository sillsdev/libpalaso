using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SIL.IO;
using SIL.Windows.Forms.HtmlBrowser;
using SIL.Windows.Forms.ImageToolbox.ImageGallery;

// ReSharper disable once CheckNamespace
namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	/// <summary>
	/// This class can replace a ListViewThumbnailViewer in applications that are using Gecko,
	/// which causes the thumb to stick when scrolling in ListViewThumbnailViewer.
	/// </summary>
	public partial class WebThumbnailViewer : UserControl, IThumbnailViewer
	{
		private readonly XWebBrowser _browser;
		public event EventHandler LoadComplete;
		private TempFile _htmlFile;
		private string _tempDirectoryName;
		private object _lastTargetClicked;
		private object _targetClicked;

		public event EventHandler SelectedIndexChanged;

		public WebThumbnailViewer()
		{
			CanLoad = false;
			ThumbBorderColor = Color.Wheat;
			ThumbBackgroundColor = Color.White;
			ThumbNailSize = 95;
			InitializeComponent();

			_browser = new XWebBrowser(XWebBrowser.BrowserType.GeckoFx);
			_browser.Dock = DockStyle.Fill;
			Controls.Add(_browser);
			_browser.Navigated += ((sender, args) =>
			{
				if (LoadComplete != null)
					LoadComplete(this, new EventArgs());
			});
			_browser.DomClick += OnDomClick;
		}

		/// <summary>
		/// Called when the user clicks in the DOM.
		/// This method figures out the ID of the element clicked or its closest parent that has an ID,
		/// and retrieves the selected path.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDomClick(object sender, EventArgs e)
		{
			// Here is the non-reflection version of this method; we make all these calls
			// using reflection following the pre-existing principle that building LibPalaso
			// should not require an actual reference to GeckoFx.
			// It may be time to abandon that (and rename this assembly) if we add much more
			// gecko-specific functionality.
			// REPLY: The advantage of this approach is that it is fairly independent of a
			// specific Gecko version. Otherwise we might have to create multiple versions
			// of libpalaso for the various gecko versions in use.
			//var ge = e as DomEventArgs;
			//var target = (GeckoHtmlElement)ge.Target.CastToGeckoElement();
			//while (target != null && target.Attributes["id"] == null)
			//	target = target.Parent;
			//if (target == null)
			//	return;
			//var id = target.Attributes["id"].NodeValue;
			// ...
			var geckoHtmlEltType = GeckoFxWebBrowserAdapter.GeckoCoreAssembly.GetType("Gecko.DomEventArgs");
			var targetProp = geckoHtmlEltType.GetProperty("Target");
			var targetRaw = targetProp.GetValue(e, new object[0]);

			var domEventTargetType = GeckoFxWebBrowserAdapter.GeckoCoreAssembly.GetType("Gecko.DOM.DomEventTarget");
			var castToElementMethod = domEventTargetType.GetMethod("CastToGeckoElement");
			var target = castToElementMethod.Invoke(targetRaw, new object[0]);

			var elementType = GeckoFxWebBrowserAdapter.GeckoCoreAssembly.GetType("Gecko.GeckoHtmlElement");
			var attributesProp = elementType.GetProperty("Attributes");
			var dictType = GeckoFxWebBrowserAdapter.GeckoCoreAssembly.GetType("Gecko.DOM.GeckoNamedNodeMap");
			var itemProp = dictType.GetProperty("Item", new Type[] { typeof(string) });
			var parentProp = elementType.GetProperty("Parent");

			while (target != null)
			{
				var attrs = attributesProp.GetValue(target, new object[0]);
				var idAttr = itemProp.GetValue(attrs, new object[] { "id" });
				if (idAttr != null)
				{
					var nodeType = GeckoFxWebBrowserAdapter.GeckoCoreAssembly.GetType("Gecko.GeckoAttribute");
					var valueProp = nodeType.GetProperty("NodeValue");
					string id = (string)valueProp.GetValue(idAttr, new object[0]);
					_lastTargetClicked = _targetClicked;
					_targetClicked = target;
					HasSelection = true;
					var path = Uri.UnescapeDataString(id);
					if (SelectedPath != path)
					{
						SetClassOfLastClickTarget(_lastTargetClicked, "imageWrap");
						SetClassOfClickTarget(_targetClicked, "imageWrap selected");
						SelectedPath = path;
						if (SelectedIndexChanged != null)
							SelectedIndexChanged(this, new EventArgs());
					}
					return;
				}
				target = parentProp.GetValue(target, new object[0]);
			}
		}

		/// <summary>
		/// Set the class of the HTML element previously clicked (that is, the one before the most recent click).
		/// </summary>
		private void SetClassOfLastClickTarget(object lastTargetClicked, string val)
		{
			var browserAdapter = ((GeckoFxWebBrowserAdapter)_browser.Adapter);
			browserAdapter.SetClass(lastTargetClicked, val);
		}

		/// <summary>
		/// Set the class of the HTML element most recently clicked.
		/// </summary>
		private void SetClassOfClickTarget(object targetClicked, string val)
		{
			var browserAdapter = ((GeckoFxWebBrowserAdapter)_browser.Adapter);
			browserAdapter.SetClass(targetClicked, val);
		}



		public string SelectedPath { get; private set; }

		public int ThumbNailSize { get; set; }

		public Color ThumbBorderColor { get; set; }

		public Color ThumbBackgroundColor { get; set; }

		public bool IsLoading
		{
			get { return _browser.IsBusy; }
		}

		public bool CanLoad { get; set; } // Not used here...matching API of the other implementation.

		public Image GetThumbNail(string fileName)
		{
			return ImageUtilities.GetThumbNail(fileName, ThumbNailSize, ThumbNailSize, ThumbBorderColor, ThumbBackgroundColor);
		}

		public void LoadItems(IEnumerable<string> pathList)
		{
			//BL-907: Crashes, probably out-of-memory error
			if (_htmlFile == null)
			{
				_htmlFile = TempFile.WithFilenameInTempFolder("searchresults.htm");
				_tempDirectoryName = Path.GetDirectoryName(_htmlFile.Path);
			}


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
        div.imageWrap {{border: 1px solid {1}; margin: 0 8px 19px 11px; display: inline-block;}}
		div.imageWrapRel {{position:relative}}
        div.imageWrapOuter {{height: {0}px;width: {0}px;display: table;#position: relative;}}
        div.imageWrapMid {{#position: absolute;#top: 50%;display: table-cell;vertical-align: middle;}}
        div.imageWrapInner {{position: relative;}}
        img.image {{max-height: {0}px;max-width: {0}px;margin: 0 auto;display: block;}}
		img[src=''] {{visibility: hidden}}
        div.selected {{border-color: #94BBD9;}}
        div.selected div.imageWrapOuter:after {{content: ' ';position: absolute;left: 0;top:0;height: {0}px;width: {0}px;background-color: rgba(148,187,217,.5);}}
    </style>
</head>
<body>
<p>", ThumbNailSize, ColorToHtmlCode(ThumbBorderColor));
			foreach (var path in pathList)
			{
				var htmlPath = new Uri(path).AbsolutePath;
				sb.AppendLine("<div class='imageWrap' id='" + htmlPath
					+ "'><div class='imageWrapRel'><div class='imageWrapOuter'><div class='imageWrapMid'><div class='imageWrapInner'>");
				// the data-echo attribute is part of lazy loading for very long lists (see below).
				sb.AppendLine("<img class='image' src='' data-echo='file://" + htmlPath + "'>");
				sb.AppendLine("</div></div></div></div></div>");
			}
			sb.AppendLine("</p>");
			sb.AppendLine("<script>");
			// This script (together with the data-echo attributes created above) makes the images load lazily...only retrieved as they become visible.
			//! echo.js v1.6.0 | (c) 2014 @toddmotto | https://github.com/toddmotto/echo
			sb.AppendLine(
				"!function(t,e){\"function\"==typeof define&&define.amd?define(function(){return e(t)}):\"object\"==typeof exports?module.exports=e:t.echo=e(t)}(this,function(t){\"use strict\";var e,n,o,r,c,i={},l=function(){},a=function(t,e){var n=t.getBoundingClientRect();return n.right>=e.l&&n.bottom>=e.t&&n.left<=e.r&&n.top<=e.b},d=function(){(r||!n)&&(clearTimeout(n),n=setTimeout(function(){i.render(),n=null},o))};return i.init=function(n){n=n||{};var a=n.offset||0,u=n.offsetVertical||a,f=n.offsetHorizontal||a,s=function(t,e){return parseInt(t||e,10)};e={t:s(n.offsetTop,u),b:s(n.offsetBottom,u),l:s(n.offsetLeft,f),r:s(n.offsetRight,f)},o=s(n.throttle,250),r=n.debounce!==!1,c=!!n.unload,l=n.callback||l,i.render(),document.addEventListener?(t.addEventListener(\"scroll\",d,!1),t.addEventListener(\"load\",d,!1)):(t.attachEvent(\"onscroll\",d),t.attachEvent(\"onload\",d))},i.render=function(){for(var n,o,r=document.querySelectorAll(\"img[data-echo]\"),d=r.length,u={l:0-e.l,t:0-e.t,b:(t.innerHeight||document.documentElement.clientHeight)+e.b,r:(t.innerWidth||document.documentElement.clientWidth)+e.r},f=0;d>f;f++)o=r[f],a(o,u)?(c&&o.setAttribute(\"data-echo-placeholder\",o.src),o.src=o.getAttribute(\"data-echo\"),c||o.removeAttribute(\"data-echo\"),l(o,\"load\")):c&&(n=o.getAttribute(\"data-echo-placeholder\"))&&(o.src=n,o.removeAttribute(\"data-echo-placeholder\"),l(o,\"unload\"));d||i.detach()},i.detach=function(){document.removeEventListener?t.removeEventListener(\"scroll\",d):t.detachEvent(\"onscroll\",d),clearTimeout(n)},i});");
			sb.AppendLine("echo.init({offset:200});");
			sb.AppendLine("</script></body></html>");

			// What we want to do here is _browser.NavigateToString(sb.ToString()).
			// But the Windows.Forms WebBrowser class doesn't have that method.
			File.WriteAllText(_htmlFile.Path, sb.ToString(), Encoding.UTF8);

			// BL-2729: Hiding the viewer during search prevents a momentary black screen on first query (and only the first).
			// This problem is hard to reproduce from just libpalaso test apps; for a while I was able to
			// get it to show using the testapp by opening the image toolbox and then closing it, then 
			// opening it again and doing a query. But after a while, that stopped working.
			// But on my Windows machine, it was a 100% reproducible inside Bloom, and this change made
			// that problem go away.
			_browser.Visible = false;
			_browser.Navigate(_htmlFile.Path);
			_browser.Visible = true;
		}

		string ColorToHtmlCode(Color color)
		{
			return string.Format("rgba({0},{1},{2},{3})", color.R, color.G, color.B, color.A / 256.0);
		}

		public bool HasSelection { get; private set; }

		public Control TheControl { get { return this; } }

		/// <summary>
		/// Implemented for compatibity with the API of the other implementation, but as it's not needed
		/// for AOR I have not attempted to implement captions.
		/// </summary>
		public Func<string, string> CaptionMethod
		{
			get; set;
		}

		// For API compatibility, but there's nothing to do here when closing.
		public void Closing()
		{
		}

		private bool disposed = false;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			disposed = true;
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				if (_browser != null)
					_browser.Dispose();
				if (_htmlFile != null)
				{
					_htmlFile.Dispose();
					_htmlFile = null;
				}
			}
			base.Dispose(disposing);
		}

		public void Clear()
		{
			// We need at least the charset declaration to prevent Javascript warnings.
			_browser.DocumentText = "<!DOCTYPE html><html><head><meta charset='UTF-8'></head><body></body></html>";
			SelectedPath = null;
			HasSelection = false;
		}
	}
}