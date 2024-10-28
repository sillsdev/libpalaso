// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace SIL.Windows.Forms.HtmlBrowser
{
	/// <summary>
	/// This class is an adapter for System.Windows.Forms.WebBrowser class. It is used by
	/// SIL.Windows.Forms.HtmlBrowser.XWebBrowser.
	///
	/// Clients should NOT use this class directly. Instead they should use the XWebBrowser
	/// class.
	/// </summary>
	internal class WinFormsBrowserAdapter: IWebBrowser
	{
		private readonly WebBrowser m_WebBrowser;

		public WinFormsBrowserAdapter(Control parent)
		{
			m_WebBrowser = new WebBrowser { Dock = DockStyle.Fill };
			parent.Controls.Add(m_WebBrowser);

			var callbacks = parent as IWebBrowserCallbacks;
			m_WebBrowser.CanGoBackChanged += (sender, args) => callbacks.OnCanGoBackChanged(args);
			m_WebBrowser.CanGoForwardChanged += (sender, args) => callbacks.OnCanGoForwardChanged(args);
			m_WebBrowser.DocumentCompleted += (sender, args) => callbacks.OnDocumentCompleted(args);
			m_WebBrowser.DocumentTitleChanged += (sender, args) => callbacks.OnDocumentTitleChanged(args);
			m_WebBrowser.Navigated += (sender, args) => callbacks.OnNavigated(args);
			m_WebBrowser.Navigating += (sender, args) => callbacks.OnNavigating(args);
			m_WebBrowser.NewWindow += (sender, args) => callbacks.OnNewWindow(args);
			m_WebBrowser.ProgressChanged += (sender, args) => callbacks.OnProgressChanged(args);
			m_WebBrowser.StatusTextChanged += (sender, args) => callbacks.OnStatusTextChanged(args);
		}

		#region IWebBrowser Members

		public bool CanGoBack => m_WebBrowser.CanGoBack;

		public bool CanGoForward => m_WebBrowser.CanGoForward;

		public void Dispose()
		{
			m_WebBrowser.Dispose();
			// Call GC.SuppressFinalize to take this object off the finalization queue
			// and prevent finalization code for this object from executing a second time.
			GC.SuppressFinalize(this);
		}

		public string DocumentText
		{
			get => m_WebBrowser.DocumentText;
			set => m_WebBrowser.DocumentText = value;
		}

		public string DocumentTitle => m_WebBrowser.DocumentTitle;

		public HtmlDocument Document => m_WebBrowser.Document;

		public bool Focused => m_WebBrowser.Focused;

		public bool IsBusy => m_WebBrowser.IsBusy;

		public bool IsWebBrowserContextMenuEnabled
		{
			get => m_WebBrowser.IsWebBrowserContextMenuEnabled;
			set => m_WebBrowser.IsWebBrowserContextMenuEnabled = value;
		}

		public string StatusText => m_WebBrowser.StatusText;

		public Uri Url
		{
			get => m_WebBrowser.Url;
			set => m_WebBrowser.Url = value;
		}

		public bool GoBack()
		{
			return m_WebBrowser.GoBack();
		}

		public bool GoForward()
		{
			return m_WebBrowser.GoForward();
		}

		public void Navigate(string urlString)
		{
			m_WebBrowser.Navigate(urlString);
		}

		public void Navigate(Uri url)
		{
			m_WebBrowser.Navigate(url);
		}

		public void Refresh()
		{
			m_WebBrowser.Refresh();
		}

		public void Refresh(WebBrowserRefreshOption opt)
		{
			m_WebBrowser.Refresh(opt);
		}

		public void Stop()
		{
			m_WebBrowser.Stop();
		}

		public void ScrollLastElementIntoView()
		{
			if (Document != null && Document.Body != null)
			{
				var childCount = Document.Body.Children.Count;
				Document.Body.Children[childCount - 1].ScrollIntoView(false);
			}
		}

		public object NativeBrowser => m_WebBrowser;

		public bool WebBrowserShortcutsEnabled
		{
			get => m_WebBrowser.WebBrowserShortcutsEnabled;
			set => m_WebBrowser.WebBrowserShortcutsEnabled = value;
		}

		[PublicAPI]
		public bool AllowNavigation
		{
			get => m_WebBrowser.AllowNavigation;
			set => m_WebBrowser.AllowNavigation = value;
		}

		public bool AllowWebBrowserDrop
		{
			get => m_WebBrowser.AllowWebBrowserDrop;
			set => m_WebBrowser.AllowWebBrowserDrop = value;
		}

		#endregion
	}
}
