using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SIL.Network
{

	public class RobustNetworkOperation
	{
		/// <summary>
		/// A browser-like User Agent string that can be used when making HTTP requests to servers
		/// that reject requests lacking a typical browser User-Agent header. Used in tests and
		/// available to callers that encounter 403 responses due to restrictive server filtering.
		/// </summary>
		public const string kBrowserCompatibleUserAgent =
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) libpalaso";
		
		/// <summary>
		/// Perform a web action, trying various things to use a proxy if needed, including requesting
		/// (and remembering) user credentials from the user.
		/// </summary>
		/// <returns>the proxy information which was needed to complete the task, will THROW if it failed.</returns>
		public static IWebProxy Do(Action<IWebProxy> action, Action<string> verboseLog)
		{
			if (verboseLog != null)
				verboseLog.Invoke("RobustNetworkOperation.Do()");


			var proxy = new WebProxy();
			action(proxy);

			//!!!!!!!!!!!!!! in Sept 2011, Hatton disabled proxy lookup. It was reportedly causing grief in Nigeria,
			//asking for credentials over and over, and SIL PNG doesn't use a proxy anymore. So for now...


/*            IWebProxy proxy;
			try
			{
				proxy = WebRequest.DefaultWebProxy;//.GetSystemWebProxy();
			}
			catch (Exception e)
			{
				Logger.WriteEvent("RobustNetworkOperation:DefaultWebProxy() gave exception: ", e.Message);
				proxy = null;
			}

			try
			{
				action(proxy);
			}
			catch (WebException e)
			{
				if (verboseLog != null)
					verboseLog.Invoke("Doing the action with the SystemWebProxy gave exception: " + e.Message);

				if (!e.Message.Contains("407"))
					throw;

				try
				{
					proxy.Credentials = ProxyCredentialsRequestDialog.GetCredentials(true);
					if (proxy.Credentials == null)
					{
						if (verboseLog != null)
							verboseLog.Invoke("Looks like user cancelled credential dialog");

						throw new UserCancelledException();
					}
					if (verboseLog != null)
						verboseLog.Invoke("Trying action again with user credentials.");

					if (verboseLog != null)
						verboseLog.Invoke("RobustNetworkOperation.Do() action again, with credentials this time.");

					action(proxy);
				}
				catch (WebException e2)
				{
					if (verboseLog != null)
						verboseLog.Invoke("Doing the action with the SystemWebProxy gave exception: " + e.Message);

					if (!e2.Message.Contains("407"))
						throw;
					//I think something like this would tell us the name of the proxy: e.Response.Headers.GetValues("Proxy-Authenticate")

					proxy.Credentials = ProxyCredentialsRequestDialog.GetCredentials(false);
					if (proxy.Credentials == null)
					{
						throw new UserCancelledException();
					}
					if (verboseLog != null)
						verboseLog.Invoke("Trying action again with user credentials.");
					action(proxy); // if this one throws, just let it go to the caller, we did our best
				}
			}
			*/
			return proxy;
		}

		/// <summary>
		/// for testing
		/// </summary>
		public static void ClearCredentialSettings()
		{
			ProxyCredentialSettings.Default.Reset();
		}

		public static string GetClearText(string encryptedString)
		{
			if (string.IsNullOrEmpty(encryptedString))
				return string.Empty;
			byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
			byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, null,
										   DataProtectionScope.CurrentUser);
			return Encoding.Unicode.GetString(clearBytes);
		}

		/// <summary>
		/// Used to determine whether an HTTP GET request requires a proxy and, if so, to retrieve
		/// the proxy host and credentials needed to access the specified remote repository URL.
		/// </summary>
		/// <param name="url">
		/// The full URL to send an HTTP GET request to. Used to test connectivity and determine
		/// whether proxy credentials are required.
		/// </param>
		/// <param name="hostAndPort">
		/// Outputs the proxy host (including port, if applicable) if a proxy is required;
		/// otherwise, an empty string.
		/// </param>
		/// <param name="userName">
		/// Outputs the proxy username if authentication is required; otherwise an empty string.
		/// </param>
		/// <param name="password">
		/// Outputs the proxy password if authentication is required; otherwise an empty string.
		/// </param>
		/// <param name="verboseLog">
		/// Optional callback for logging diagnostic information about the request and proxy
		/// detection process.
		/// </param>
		/// <param name="userAgentHeader">
		/// Optional user agent header string to send with the request. Some servers require a
		/// browser-like user agent and may reject requests that appear to come from automated
		/// clients. See <see cref="kBrowserCompatibleUserAgent"/> for an example value.
		/// </param>
		/// <returns>
		/// True if a proxy is required and credentials were obtained; false if no proxy is needed.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="url"/> is null.
		/// </exception>
		/// <exception cref="UriFormatException">
		/// Thrown if <paramref name="url"/> is not a valid URI.
		/// </exception>
		/// <exception cref="WebException">
		/// Thrown if the HTTP request fails due to network errors, proxy configuration
		/// issues, or authentication failures.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// Thrown if the URI scheme is not supported.
		/// </exception>
		public static bool DoHttpGetAndGetProxyInfo(string url, out string hostAndPort,
			out string userName, out string password, Action<string> verboseLog,
			string userAgentHeader = null)
		{
			hostAndPort = string.Empty;
			userName = string.Empty;
			password = string.Empty;

			var client = new WebClient();

			//note: this can throw
			var proxyInfo = Do(
				proxy =>
				{
					client.Proxy = proxy;
					if (userAgentHeader != null)
						client.Headers[HttpRequestHeader.UserAgent] = userAgentHeader;
					client.DownloadData(url);
					//we don't actually care what comes back
				}, verboseLog
			);

			var destination = new Uri(url);
			var proxyUrl = proxyInfo.GetProxy(destination);
			hostAndPort = proxyUrl.OriginalString;

			if (string.IsNullOrEmpty(hostAndPort))
				return false;

			if (!proxyInfo.IsBypassed(destination))
				return false;

			// In the absence of a proxy it is reported as being the same as the destination url.
			if (proxyUrl.AbsoluteUri == url)
				return false;

			if (proxyInfo.Credentials == null) // This seems to be the normal course of things without a proxy
				return false;

			var networkCredential = proxyInfo.Credentials.GetCredential(destination, "");
			userName = networkCredential.UserName;
			password = networkCredential.Password;
			verboseLog?.Invoke("DoHttpGetAndGetProxyInfo Returning with credentials. UserName is " + userName);

			return true;
		}
	}
}
