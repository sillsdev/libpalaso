using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Palaso.CommandLineProcessing;

namespace Palaso.Reporting.Network
{

	public class RobustNetworkOperation
	{
		/// <summary>
		/// Perform a web action, trying various things to use a proxy if needed, including requesting
		/// (and remembering) user credentials from the user.
		/// </summary>
		/// <returns>the proxy information which was needed to complete the task, or null if it failed.</returns>
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
		/// Used by Chorus to get proxy name, user name, and password of the remote repository
		/// </summary>
		/// <returns>the proxy information which was needed to download the page, or null if it failed.</returns>
		public static IWebProxy DoHttpGetAndGetProxyInfo(string url)
		{
			try
			{
				var client = new WebClient();
				//client.Credentials = CredentialCache.DefaultNetworkCredentials;
				return RobustNetworkOperation.Do(proxy =>
				{
					client.Proxy = proxy;
					client.DownloadData(url);//we don't actually care what comes back
				});
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
