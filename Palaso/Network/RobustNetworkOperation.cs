using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Palaso.CommandLineProcessing;

namespace Palaso.Network
{

	public class RobustNetworkOperation
	{
		/// <summary>
		/// Perform a web action, trying various things to use a proxy if needed, including requesting
		/// (and remembering) user credentials from the user.
		/// </summary>
		/// <returns>the proxy information which was needed to complete the task, will THROW if it failed.</returns>
		public static IWebProxy Do(Action<IWebProxy> action)
		{
			IWebProxy proxy = WebRequest.GetSystemWebProxy();

			try
			{
				action(proxy);
			}
			catch (WebException e)
			{
				if (!e.Message.Contains("407"))
					throw e;

				try
				{
					proxy.Credentials = ProxyCredentialsRequestDialog.GetCredentials(true);
					if (proxy.Credentials == null)
					{
						throw new UserCancelledException();
					}
					action(proxy);
				}
				catch (WebException e2)
				{
					if (!e2.Message.Contains("407"))
						throw e2;

					proxy.Credentials = ProxyCredentialsRequestDialog.GetCredentials(false);
					if (proxy.Credentials == null)
					{
						throw new UserCancelledException();
					}
					action(proxy); // if this one throws, just let it go to the caller, we did our best
				}
			}
			return proxy;
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
		public static bool DoHttpGetAndGetProxyInfo(string url, out string hostAndPort, out string userName, out string password)
		{
			hostAndPort = string.Empty;
			userName = string.Empty;
			password = string.Empty;

			var client = new WebClient();

			//note: this can throw
			var proxyInfo = RobustNetworkOperation.Do(proxy =>
														  {
															  client.Proxy = proxy;
															  client.DownloadData(url);
																  //we don't actually care what comes back
														  });

			var destination = new Uri(url);
			var proxyUrl = proxyInfo.GetProxy(destination);
			hostAndPort = proxyUrl.OriginalString;
			if (string.IsNullOrEmpty(hostAndPort))
				return false;

			var networkCredential = proxyInfo.Credentials.GetCredential(destination, "");
			userName = networkCredential.UserName;
			password = networkCredential.Password;
			return true;
		}
	}
}
