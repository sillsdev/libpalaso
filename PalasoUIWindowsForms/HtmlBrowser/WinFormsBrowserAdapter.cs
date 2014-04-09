// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.HtmlBrowser
{
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

		public bool CanGoBack
		{
			get { return m_WebBrowser.CanGoBack; }
		}

		public bool CanGoForward
		{
			get { return m_WebBrowser.CanGoForward; }
		}

		public string DocumentText
		{
			get { return m_WebBrowser.DocumentText; }
			set { m_WebBrowser.DocumentText = value; }
		}

		public string DocumentTitle
		{
			get { return m_WebBrowser.DocumentTitle; }
		}

		public bool Focused
		{
			get { return m_WebBrowser.Focused; }
		}

		public bool IsBusy
		{
			get { return m_WebBrowser.IsBusy; }
		}

		public bool IsWebBrowserContextMenuEnabled
		{
			get { return m_WebBrowser.IsWebBrowserContextMenuEnabled; }
			set { m_WebBrowser.IsWebBrowserContextMenuEnabled = value; }
		}

		public string StatusText
		{
			get { return m_WebBrowser.StatusText; }
		}

		public Uri Url
		{
			get { return m_WebBrowser.Url; }
			set { m_WebBrowser.Url = value; }
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

		public object NativeBrowser
		{
			get { return m_WebBrowser; }
		}

		#endregion
	}
}
