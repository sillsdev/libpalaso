// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Gecko;
using Palaso.IO;
using Palaso.Reporting;

namespace Palaso.UI.WindowsForms.HtmlBrowser
{
	class GeckoFxWebBrowserAdapter: IWebBrowser
	{
		private readonly GeckoWebBrowser _webBrowser;

		public GeckoFxWebBrowserAdapter(Control parent)
		{
			SetUpXulRunner();
			_webBrowser = new GeckoWebBrowser { Dock = DockStyle.Fill };
			parent.Controls.Add(_webBrowser);

			var callbacks = parent as IWebBrowserCallbacks;
			_webBrowser.CanGoBackChanged += (sender, e) => callbacks.OnCanGoBackChanged(e);
			_webBrowser.CanGoForwardChanged += (sender, e) => callbacks.OnCanGoForwardChanged(e);
			_webBrowser.DocumentCompleted += (sender, e) => callbacks.OnDocumentCompleted(new WebBrowserDocumentCompletedEventArgs(_webBrowser.Url));
			_webBrowser.DocumentTitleChanged += (sender, e) => callbacks.OnDocumentTitleChanged(e);
			_webBrowser.Navigated += (sender, e) => callbacks.OnNavigated(new WebBrowserNavigatedEventArgs(e.Uri));
			_webBrowser.Navigating += (sender, e) => {
				var ev = new WebBrowserNavigatingEventArgs(e.Uri, string.Empty);
				callbacks.OnNavigating(ev);
				e.Cancel = ev.Cancel;
			};
			_webBrowser.CreateWindow2 += (sender, e) => {
				var ev = new CancelEventArgs();
				callbacks.OnNewWindow(ev);
				e.Cancel = ev.Cancel;
			};
			_webBrowser.ProgressChanged += (sender, e) => callbacks.OnProgressChanged(new WebBrowserProgressChangedEventArgs(e.CurrentProgress, e.MaximumProgress));
			_webBrowser.StatusTextChanged += (sender, e) => callbacks.OnStatusTextChanged(e);
		}

		private static int XulRunnerVersion
		{
			get
			{
				var geckofx = Assembly.GetAssembly(typeof(GeckoWebBrowser));
				if (geckofx == null)
					return 0;

				var versionAttribute = geckofx.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)
					.FirstOrDefault() as AssemblyFileVersionAttribute;
				return versionAttribute == null ? 0 : new Version(versionAttribute.Version).Major;
			}
		}

		private static void SetUpXulRunner()
		{
			if (Xpcom.IsInitialized)
				return;

			string xulRunnerPath = Environment.GetEnvironmentVariable("XULRUNNER");
			if (!Directory.Exists(xulRunnerPath))
			{
				xulRunnerPath = Path.Combine(FileLocator.DirectoryOfApplicationOrSolution, "xulrunner");
				if (!Directory.Exists(xulRunnerPath))
				{
					//if this is a programmer, go look in the lib directory
					xulRunnerPath = Path.Combine(FileLocator.DirectoryOfApplicationOrSolution,
						Path.Combine("lib", "xulrunner"));

					//on my build machine, I really like to have the dir labelled with the version.
					//but it's a hassle to update all the other parts (installer, build machine) with this number,
					//so we only use it if we don't find the unnumbered alternative.
					if (!Directory.Exists(xulRunnerPath))
					{
						xulRunnerPath = Path.Combine(FileLocator.DirectoryOfApplicationOrSolution,
							Path.Combine("lib", "xulrunner" + XulRunnerVersion));
					}

					if (!Directory.Exists(xulRunnerPath))
					{
						throw new ConfigurationException(
							"Can't find the directory where xulrunner (version {0}) is installed",
							XulRunnerVersion);
					}
				}
			}

			Xpcom.Initialize(xulRunnerPath);
			Application.ApplicationExit += OnApplicationExit;
		}

		private static void OnApplicationExit(object sender, EventArgs e)
		{
			// We come here iff we initialized Xpcom. In that case we want to call shutdown,
			// otherwise the app might not exit properly.
			if (Xpcom.IsInitialized)
				Xpcom.Shutdown();
			Application.ApplicationExit -= OnApplicationExit;
		}

		#region IWebBrowser Members

		public bool CanGoBack
		{
			get { return _webBrowser.CanGoBack; }
		}

		public bool CanGoForward
		{
			get { return _webBrowser.CanGoForward; }
		}

		public string DocumentText
		{
			get { return _webBrowser.Text; }
			set { _webBrowser.Text = value; }
		}

		public string DocumentTitle
		{
			get { return _webBrowser.DocumentTitle; }
		}

		public bool Focused
		{
			get { return _webBrowser.Focused; }
		}

		public bool IsBusy
		{
			get { return _webBrowser.IsBusy; }
		}

		public bool IsWebBrowserContextMenuEnabled
		{
			get { return !_webBrowser.NoDefaultContextMenu; }
			set { _webBrowser.NoDefaultContextMenu = !value; }
		}

		public string StatusText
		{
			get { return _webBrowser.StatusText; }
		}

		public Uri Url
		{
			get { return _webBrowser.Url; }
			set { _webBrowser.Navigate(value.OriginalString); }
		}

		public bool GoBack()
		{
			return _webBrowser.GoBack();
		}

		public bool GoForward()
		{
			return _webBrowser.GoForward();
		}

		public void Navigate(string urlString)
		{
			_webBrowser.Navigate(urlString);
		}

		public void Navigate(Uri url)
		{
			_webBrowser.Navigate(url.AbsoluteUri);
		}

		public void Refresh()
		{
			_webBrowser.Refresh();
		}

		public void Refresh(WebBrowserRefreshOption opt)
		{
			_webBrowser.Refresh();
		}

		public void Stop()
		{
			_webBrowser.Stop();
		}

		public object NativeBrowser
		{
			get { return _webBrowser; }
		}
		#endregion
	}
}
