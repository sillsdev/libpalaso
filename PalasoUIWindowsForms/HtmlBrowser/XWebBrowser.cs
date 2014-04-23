﻿// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.HtmlBrowser
{
	/// <summary>
	/// This class implements a (simplified) cross-platform web browser control that underneath
	/// uses either the WinForms WebBrowser control or GeckoWebBrowser. Which browser gets used
	/// depends on the platform (Linux: GeckoFx, Windows: WebBrowser), but can also be specified
	/// as a parameter in the constructor. It's also possible to set the
	/// <see cref="DefaultBrowserType"/> that gets used if no explicit value is specified in the
	/// constructor.
	/// </summary>
	/// <remarks>NOTE: in order as not to introduce a dependency on GeckoFx unless needed, the
	/// GeckoFx support is implemented in a separate assembly. If you want to use the GeckoFx
	/// browser you'll have to make sure that the assembly gets copied to the same directory
	/// as this dll.</remarks>
	public partial class XWebBrowser : Control, IWebBrowser, IWebBrowserCallbacks
	{
		private readonly IWebBrowser m_WebBrowserAdapter;

		public static BrowserType DefaultBrowserType { get; set; }

		public enum BrowserType
		{
			Default,
			WinForms,
			GeckoFx,
			Fallback,
		}

		static XWebBrowser()
		{
			DefaultBrowserType = BrowserType.Default;
		}

		public XWebBrowser() : this(DefaultBrowserType)
		{
		}

		public XWebBrowser(BrowserType type)
		{
			InitializeComponent();
			m_WebBrowserAdapter = CreateBrowser(type);
		}

		private IWebBrowser CreateBrowser(BrowserType type)
		{
			while (true)
			{
				switch (type)
				{
					case BrowserType.WinForms:
						return new WinFormsBrowserAdapter(this);
					case BrowserType.GeckoFx:
						var path = Path.Combine(Path.GetDirectoryName(
							new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath),
							"PalasoUIWindowsForms.GeckoFxWebBrowserAdapter.dll");
						if (File.Exists(path))
						{
							var assembly = Assembly.LoadFile(path);
							if (assembly != null)
							{
								var browser = assembly.GetType("Palaso.UI.WindowsForms.HtmlBrowser.GeckoFxWebBrowserAdapter");
								if (browser != null)
									return (IWebBrowser)Activator.CreateInstance(browser, this);
							}
						}
						type = BrowserType.Fallback;
						break;
					case BrowserType.Default:
					case BrowserType.Fallback:
						IWebBrowser webBrowser = null;
						if (PlatformUtilities.Platform.IsWindows)
							webBrowser = CreateBrowser(BrowserType.WinForms);
						if (webBrowser != null || type == BrowserType.Fallback)
							return webBrowser;
						type = BrowserType.GeckoFx;
						break;
				}
			}
		}

		#region IWebBrowser Members

		public bool CanGoBack
		{
			get { return m_WebBrowserAdapter.CanGoBack; }
		}

		public bool CanGoForward
		{
			get { return m_WebBrowserAdapter.CanGoForward; }
		}

		public string DocumentText
		{
			get { return m_WebBrowserAdapter.DocumentText; }
			set { m_WebBrowserAdapter.DocumentText = value; }
		}

		public string DocumentTitle
		{
			get { return m_WebBrowserAdapter.DocumentTitle; }
		}

		public override bool Focused
		{
			get { return m_WebBrowserAdapter.Focused; }
		}

		public bool IsBusy
		{
			get { return m_WebBrowserAdapter.IsBusy; }
		}

		public bool IsWebBrowserContextMenuEnabled
		{
			get { return m_WebBrowserAdapter.IsWebBrowserContextMenuEnabled; }
			set { m_WebBrowserAdapter.IsWebBrowserContextMenuEnabled = value; }
		}

		public string StatusText
		{
			get { return m_WebBrowserAdapter.StatusText; }
		}

		public Uri Url
		{
			get { return m_WebBrowserAdapter.Url; }
			set { m_WebBrowserAdapter.Url = value; }
		}

		public bool GoBack()
		{
			return m_WebBrowserAdapter.GoBack();
		}

		public bool GoForward()
		{
			return m_WebBrowserAdapter.GoForward();
		}

		public void Navigate(string urlString)
		{
			m_WebBrowserAdapter.Navigate(urlString);
		}

		public void Navigate(Uri url)
		{
			m_WebBrowserAdapter.Navigate(url);
		}

		public override void Refresh()
		{
			m_WebBrowserAdapter.Refresh();
		}

		public void Refresh(WebBrowserRefreshOption opt)
		{
			m_WebBrowserAdapter.Refresh(opt);
		}

		public void Stop()
		{
			m_WebBrowserAdapter.Stop();
		}

		/// <summary>
		/// Provides access to the native browser control (WebBrowser or GeckoWebBrowser)
		/// </summary>
		/// <value>The native browser.</value>
		public object NativeBrowser
		{
			get { return m_WebBrowserAdapter.NativeBrowser; }
		}

		#endregion

		public event EventHandler CanGoBackChanged;

		public event EventHandler CanGoForwardChanged;

		public event WebBrowserDocumentCompletedEventHandler DocumentCompleted;

		public event EventHandler DocumentTitleChanged;

		public event WebBrowserNavigatedEventHandler Navigated;

		public event WebBrowserNavigatingEventHandler Navigating;

		public event CancelEventHandler NewWindow;

		public event WebBrowserProgressChangedEventHandler ProgressChanged;

		public event EventHandler StatusTextChanged;

		#region IWebBrowserCallbacks
		void IWebBrowserCallbacks.OnCanGoBackChanged(EventArgs e)
		{
			if (CanGoBackChanged != null)
				CanGoBackChanged(this, e);
		}

		void IWebBrowserCallbacks.OnCanGoForwardChanged(EventArgs e)
		{
			if (CanGoForwardChanged != null)
				CanGoForwardChanged(this, e);
		}

		void IWebBrowserCallbacks.OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
		{
			if (DocumentCompleted != null)
				DocumentCompleted(this, e);
		}

		void IWebBrowserCallbacks.OnDocumentTitleChanged(EventArgs e)
		{
			if (DocumentTitleChanged != null)
				DocumentTitleChanged(this, e);
		}

		void IWebBrowserCallbacks.OnNavigated(WebBrowserNavigatedEventArgs e)
		{
			if (Navigated != null)
				Navigated(this, e);
		}

		void IWebBrowserCallbacks.OnNavigating(WebBrowserNavigatingEventArgs e)
		{
			if (Navigating != null)
				Navigating(this, e);
		}

		void IWebBrowserCallbacks.OnNewWindow(CancelEventArgs e)
		{
			if (NewWindow != null)
				NewWindow(this, e);
		}

		void IWebBrowserCallbacks.OnProgressChanged(WebBrowserProgressChangedEventArgs e)
		{
			if (ProgressChanged != null)
				ProgressChanged(this, e);
		}

		void IWebBrowserCallbacks.OnStatusTextChanged(EventArgs e)
		{
			if (StatusTextChanged != null)
				StatusTextChanged(this, e);
		}

#endregion
	}
}
