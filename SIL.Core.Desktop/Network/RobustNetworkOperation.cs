using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SIL.Network
{

	public class RobustNetworkOperation
	{
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

			//!!!!!!!!!!!!!! in Sept 2011, hatton disabled proxy lookup. It was reportedly causing grief in Nigeria,
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
		/// Used by Chorus to get proxy name, user name, and password of the remote repository.
		/// </summary>
		/// <returns>true if a proxy is needed. THROWS if it just can't get through</returns>
		public static bool DoHttpGetAndGetProxyInfo(string url, out string hostAndPort, out string userName, out string password, Action<string> verboseLog)
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
			if (verboseLog != null)
				verboseLog.Invoke("DoHttpGetAndGetProxyInfo Returning with credentials. UserName is " + userName);


			return true;
		}
	}
}
