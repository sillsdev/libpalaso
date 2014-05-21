// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.HtmlBrowser
{
	public interface IWebBrowser
	{
		bool CanGoBack { get; }
		bool CanGoForward { get; }
		string DocumentText { get; set; }
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
	}
}