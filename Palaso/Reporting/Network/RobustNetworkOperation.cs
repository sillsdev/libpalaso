using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
		public static void Do(Action<IWebProxy> action)
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
		}
	}
}
