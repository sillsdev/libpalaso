// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.Windows.Forms.HtmlBrowser
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

			// set default values
			m_WebBrowserAdapter.AllowWebBrowserDrop = true;
			m_WebBrowserAdapter.IsWebBrowserContextMenuEnabled = true;
			m_WebBrowserAdapter.WebBrowserShortcutsEnabled = true;
		}

		private IWebBrowser CreateBrowser(BrowserType type)
		{
			switch (type)
			{
				case BrowserType.Default:
				case BrowserType.WinForms:
					if (Platform.IsWindows)
						return new WinFormsBrowserAdapter(this);
					// The WinForms adapter works only on Windows. On Linux we fall through to
					// the geckofx case instead.
					goto case BrowserType.GeckoFx;
				case BrowserType.GeckoFx:
					const string geckoFxWebBrowserAdapterType =
						"SIL.Windows.Forms.GeckoBrowserAdapter.GeckoFxWebBrowserAdapter";
					// It's possible that GeckoBrowserAdapter.dll got ilmerged/ilrepacked into
					// the currently executing assembly, so try that first.
					// NOTE: when ilrepacking SIL.Windows.Forms.dll you'll also have to ilrepack
					// GeckoBrowserAdapter.dll, otherwise the IWebBrowser interface definition
					// will be different!
					var browser = Assembly.GetExecutingAssembly().GetType(geckoFxWebBrowserAdapterType);
					if (browser == null)
					{
						var path = Path.Combine(Path.GetDirectoryName(
							new Uri(Assembly.GetExecutingAssembly().Location).LocalPath),
							"SIL.Windows.Forms.GeckoBrowserAdapter.dll");
						if (File.Exists(path))
						{
							var assembly = Assembly.LoadFile(path);
							if (assembly != null)
							{
								browser = assembly.GetType(geckoFxWebBrowserAdapterType);
							}
						}
					}
					if (browser != null)
					{
						try
						{
							return (IWebBrowser) Activator.CreateInstance(browser, this);
						}
						catch (Exception e)
						{
							Logger.WriteError("Ignoring problem creating GeckoFxWebBrowserAdapter.", e);
						}
					}
					//We failed to Create the GeckoFxWebBrowserAdapter, so drop into the fallback case
					goto case BrowserType.Fallback;
				case BrowserType.Fallback:
				default:
					if (Platform.IsWindows)
						return CreateBrowser(BrowserType.WinForms);
					return null;
			}
		}

		#region IWebBrowser Members

		/// <summary>
		/// If false, allows initial page load, but after that it opens new links in the system browser
		/// </summary>
		/// <remarks>Note that we're not using the browser's built-in property; we enforce this in a more
		/// intelligent way by intercepting navigation attempts and doing the right thing.  Also, we want
		/// a purely embedded browser by default that handles navigation internally.</remarks>
		public bool AllowNavigation = true;

		[DefaultValue(true)]
		public bool AllowWebBrowserDrop
		{
			get { return m_WebBrowserAdapter.AllowWebBrowserDrop; }
			set { m_WebBrowserAdapter.AllowWebBrowserDrop = value; }
		}

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

		[DefaultValue(true)]
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

		public void ScrollLastElementIntoView()
		{
			m_WebBrowserAdapter.ScrollLastElementIntoView();
		}

		/// <summary>
		/// Provides access to the native browser control (WebBrowser or GeckoWebBrowser)
		/// </summary>
		/// <value>The native browser.</value>
		public object NativeBrowser
		{
			get { return m_WebBrowserAdapter.NativeBrowser; }
		}

		public IWebBrowser Adapter { get { return m_WebBrowserAdapter; } }

		[DefaultValue(true)]
		public bool WebBrowserShortcutsEnabled
		{
			get { return m_WebBrowserAdapter.WebBrowserShortcutsEnabled; }
			set { m_WebBrowserAdapter.WebBrowserShortcutsEnabled = value; }
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

		public event EventHandler DomClick;

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

		protected bool GetShouldThisNavigationUseExternalBrowser(string urlString)
		{
			//ignore AllowNavigation when we haven't loaded the initial page yet
			return !AllowNavigation && Url != null && Url.OriginalString != "about:blank";
		}

		void IWebBrowserCallbacks.OnNavigating(WebBrowserNavigatingEventArgs e)
		{
			//we're interpretting AllowNavigation==false as meaning "links should open in an external browser"
			if (GetShouldThisNavigationUseExternalBrowser(e.Url.AbsolutePath))
			{
				try
				{
					SIL.Program.Process.SafeStart(e.Url.AbsoluteUri);
				}
				catch (Exception)
				{
					bool localPathWorked = false;
					if (e.Url.AbsoluteUri.StartsWith("file:"))
					{
						try
						{
							SIL.Program.Process.SafeStart(e.Url.LocalPath);
							localPathWorked = true;
						}
						catch
						{
						}
					}
					if (!localPathWorked)
						throw;
				}
				e.Cancel = true;
			}
			else if (Navigating != null)
			{
				Navigating(this, e);
			}
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

		void IWebBrowserCallbacks.OnDomClick(EventArgs e)
		{
			if (DomClick != null)
				DomClick(this, e);
		}

#endregion
	}
}
