using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using CookComputing.XmlRpc;
using Palaso.Services.Dictionary;
using Palaso.Services.ForServers;

namespace Palaso.Services
{
	/// <summary>
	/// Wraps the inter-process communication mechanism so that this code isn't
	/// spread throughout the implementations.  The details here can be changed, with
	/// less impact on the servers and clients if we ever leave the (.net 2)
	/// remoting and move back to the (.net 3) WPF approach.
	/// </summary>
	public class IpcSystem
	{
		/// <summary>
		/// Because .net remoting (.net 2) requires each application to have its own port,
		/// when providing services, we need to first find an open port, and when we go
		/// looking for providers, we may need to query on serveral ports.
		/// </summary>
		public static int StartingPort=5678;

		public static readonly int NumberOfPortsToTry = 3;

		public static string URLPrefix
		{
			get
			{
				//return "net.pipe://localhost/";
				return "http://localhost";
			}
		}


		public static TServiceInterface GetExistingService<TServiceInterface>(string unescapedServiceName)
			where TServiceInterface : class, IXmlRpcProxy, IPingable
		{
			System.Diagnostics.Debug.Assert(!unescapedServiceName.Contains("%"),"please leave it to this method to figure out the correct escaping to do.");
			System.Diagnostics.Debug.Assert(!IsWellFormedUriStringMonoSafe(unescapedServiceName), "This method needs a service name, not a whole uri.");
		  //  System.Diagnostics.Debug.WriteLine("("+unescapedServiceName+") trying to get service: " + unescapedServiceName);


			TServiceInterface serviceProxy = XmlRpcProxyGen.Create<TServiceInterface>();
			for (int port = StartingPort; port < StartingPort + NumberOfPortsToTry; port++)
			{
				//nb: this timeout does work, but reducing it to the point where it
				//actually returns earlier just makes it unreliable (consisten test failures to find
				//existing services on my fast machine). 100,000 is default timeout, but
				//you get a "nope, not there" in a second or two

				//serviceProxy.Timeout = 1000;//2000 is safe on my fast machine
				serviceProxy.Url = GetUrlForService(FixupServiceName(unescapedServiceName), port);

				try
				{
					Debug.WriteLine("Checking for service at " + serviceProxy.Url);
					//hack: just need some way to see if it's alive, there should be a lower-level way to do that
					((IPingable)serviceProxy).Ping();
					Debug.WriteLine("   Found & pinged.");
					return serviceProxy;//found one
				}
				catch (Exception e) //swallow
				{
					Debug.WriteLine("   " + e.Message + " (not necessarily a problem)");
				}
			}
			return null;
		}



		private static string GetUrlForService(string name, int port)
		{
			return URLPrefix + ":" + port + "/" + FixupServiceName(name);
		}

		private static string FixupServiceName(string name)
		{
			return name.Replace(@"\", "/");
		}

		public static string GetRemotingNameForService(string name)
		{
			return name;
		}

		private static int GetAChannelWeCanUse()
		{
			//nb: only one channel per port+protocol

			IDictionary props = new Hashtable();
			props["bindTo"] = "127.0.0.1"; //local only
			props["name"] = GetChannelName(StartingPort);
			props["port"] = StartingPort;

			//this only seems to work if the channel belongs to our process!
			if (null != ChannelServices.GetChannel((string)props["name"]))
			{
				return StartingPort; //we already own this port in this process
			}

			int port = StartingPort;
			for (; port < StartingPort + NumberOfPortsToTry; port++)
			{
				props["port"] = port;
				props["name"] = GetChannelName(port);
				try
				{
#if DEBUG
					Console.WriteLine("Looking for free port: " + port);
#endif
					IChannel channel = new HttpChannel(props, null, new XmlRpcServerFormatterSinkProvider());
					ChannelServices.RegisterChannel(channel, false);
					break;
				}
				catch (Exception e)
				{
				}
			}
			if (port < StartingPort + NumberOfPortsToTry)
			{
				return port;
			}
			throw new ApplicationException("Could not find a free port on which to create the service.");
		}

		private static string GetChannelName(int port)
		{
			return "http."+port;
		}

		static private void UnregisterHttpChannel(int port)
		{
			IDictionary props = new Hashtable();
			props["port"] = port;
			//see if one with this name is registered
			IChannel channel = ChannelServices.GetChannel(GetChannelName(port));
			if (channel != null)
			{
				ChannelServices.UnregisterChannel(channel);
			}
		}

		public static void StartServingObject(string unescapedServiceName,MarshalByRefObject objectToServe)
		{
			System.Diagnostics.Debug.Assert(!unescapedServiceName.Contains("%"), "please leave it to this method to figure out the correct escaping to do.");
			System.Diagnostics.Debug.Assert(!IsWellFormedUriStringMonoSafe(unescapedServiceName), "This method needs a service name, not a whole uri.");

			try
			{
				int port = IpcSystem.GetAChannelWeCanUse();
#if DEBUG
				Console.WriteLine("(we *think* we are) registering on port: " + port);
#endif
			}
			catch (System.Runtime.Remoting.RemotingException e)
			{
				throw new ApplicationException("An error occured which happens when we try to get a service that was created on the same thread", e);
			}

			//QUESTION: how does the system know to serve up this service on that channel we
			//just made?

			RemotingServices.Marshal(objectToServe, FixupServiceName(unescapedServiceName));

			System.Diagnostics.Debug.WriteLine("Now serving " + FixupServiceName(unescapedServiceName));
			//         File.WriteAllText(@"c:\temp\StartServingObjectLog.txt", "Now serving " + FixupServiceName(unescapedServiceName));
		}

		/// <summary>
		/// mono bug number 376692
		/// </summary>
		/// <param name="unescapedServiceName"></param>
		/// <returns></returns>
		private static bool IsWellFormedUriStringMonoSafe(string unescapedServiceName)
		{
			return IsWellFormedUriStringMonoSafe(unescapedServiceName.Replace("%5", "_"), UriKind.Absolute);
		}

		private static bool IsWellFormedUriStringMonoSafe(string unescapedServiceName, UriKind uriKind)
		{
			try
			{
				return Uri.IsWellFormedUriString(unescapedServiceName.Replace("%5", "_"), uriKind);
			}
			catch (Exception)
			{
				return false; // mono will, incorrectly, fall through to here
			}
		}
	}


	public interface IPingable
	{
		[XmlRpcMethod("IpcSystem.Ping", Description = "Always returns true.")]
		bool Ping();
	}

}