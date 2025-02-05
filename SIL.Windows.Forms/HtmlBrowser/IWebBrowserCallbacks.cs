// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SIL.Windows.Forms.HtmlBrowser
{
	public interface IWebBrowserCallbacks
	{
		void OnCanGoBackChanged(EventArgs e);
		void OnCanGoForwardChanged(EventArgs e);
		void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e);
		void OnDocumentTitleChanged(EventArgs e);
		void OnNavigated(WebBrowserNavigatedEventArgs e);
		void OnNavigating(WebBrowserNavigatingEventArgs e);
		void OnNewWindow(CancelEventArgs e);
		void OnProgressChanged(WebBrowserProgressChangedEventArgs e);
		void OnStatusTextChanged(EventArgs e);
		void OnDomClick(EventArgs e);
	}
}