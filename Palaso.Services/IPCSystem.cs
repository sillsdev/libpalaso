using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using CookComputing.XmlRpc;
using Palaso.Services.Dictionary;
using Palaso.Services.ForServers;

namespace Palaso.Services.ForClients
{
	/// <summary>
	/// Wraps the inter-process communication mechanism so that this code isn't
	/// spread throughout the implementations.  The details here can be changed, with
	/// less impact on the servers and clients if we ever leave the (.net 2)
	/// remoting and move back to the (.net 3) WPF approach.
	/// </summary>
	public class IpcSystem
	{
		public static int _defaultPort=5678;

		public static string URLPrefix
		{
			get
			{
				//return "net.pipe://localhost/";
				return "http://localhost";
			}
		}


		public static TServiceInterface GetExistingService<TServiceInterface>(string unescapedServiceName)
			where TServiceInterface : class, IXmlRpcProxy
		{
			System.Diagnostics.Debug.Assert(!unescapedServiceName.Contains("%"),"please leave it to this method to figure out the correct escaping to do.");
			System.Diagnostics.Debug.WriteLine("Trying to get service: " + unescapedServiceName);
			System.Diagnostics.Debug.Assert(!Uri.IsWellFormedUriString(unescapedServiceName.Replace("%5", "_"), UriKind.Absolute), "This method needs a service name, not a whole uri.");


			TServiceInterface serviceProxy = XmlRpcProxyGen.Create<TServiceInterface>();
			serviceProxy.Url = GetUrlForService(FixupServiceName(unescapedServiceName), IpcSystem._defaultPort);

			try
			{ //todo: just need some way to see if it's alive
				if(serviceProxy is IServiceAppConnector)
				{
					((IServiceAppConnector)serviceProxy).IsAlive();
				}
				if (serviceProxy is IDictionaryService)
				{
					((IDictionaryService) serviceProxy).GetCurrentUrl();
				}

			}
			catch(Exception e)
			{
				return null;
			}


			return serviceProxy;
		}

		public static string GetUrlForService(string name, int port)
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

		private static void RegisterHttpChannelIfNeeded(int port)
		{
			//nb: only one channel per port+protocol

			IDictionary props = new Hashtable();
			props["port"] = port;
			props["bindTo"] = "127.0.0.1"; //local only
			props["name"] = GetChannelName(port);

			if (null == ChannelServices.GetChannel((string)props["name"]))
			{
				IChannel channel = new HttpChannel(props, null, new XmlRpcServerFormatterSinkProvider());
				ChannelServices.RegisterChannel(channel, false);
			}
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
			System.Diagnostics.Debug.Assert(!Uri.IsWellFormedUriString(unescapedServiceName.Replace("%5","_"), UriKind.Absolute), "This method needs a service name, not a whole uri.");

			try
			{
				IpcSystem.RegisterHttpChannelIfNeeded(IpcSystem._defaultPort);
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

			/* in WPF, we did something like this:
			 *
			 *                 _dictionaryHost = new ServiceHost(objectToServe, new Uri[] {new Uri(DictionaryServiceAddress),});

				_dictionaryHost.AddServiceEndpoint(typeof (IDictionaryService), IPCUtils.CreateBinding(),
												   DictionaryServiceAddress);
				_dictionaryHost.Open();
			   */

		}
	}
}