// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.HtmlBrowser
{
	public interface IWebBrowser : IDisposable
	{
		bool AllowWebBrowserDrop { get; set; }
		bool CanGoBack { get; }
		bool CanGoForward { get; }
		/// <summary>
		/// Set of the DocumentText will load the given string content into the browser.
		/// If a get for DocumentText proves necessary Jason promises to write the reflective
		/// gecko implementation.
		/// </summary>
		string DocumentText { set; }
		string DocumentTitle { get; }
		bool Focused { get; }
		bool IsBusy { get; }
		bool IsWebBrowserContextMenuEnabled { get; set; }
		string StatusText { get; }
		Uri Url { get; set; }
		bool GoBack();
		bool GoForward();
		void Navigate(string urlString);
		void Navigate(Uri url);
		void Refresh();
		void Refresh(WebBrowserRefreshOption opt);
		void Stop();
		void ScrollLastElementIntoView();
		object NativeBrowser { get; }
		bool WebBrowserShortcutsEnabled { get; set; }
	}
}