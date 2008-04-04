using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;

namespace Palaso.Services.ForServers
{
	/// <summary>
	/// this is the outward-facing contract. Other apps talk to this one through these methods.
	/// Its main job, though, is just to "exist" on a named pipe, so that only a single application
	/// will be launched to provide the service, regardless of how many times the  user or another
	/// program tells it to open.
	/// </summary>
	public interface IServiceAppConnector : IPingable
	{
		[XmlRpcMethod("ServiceApp.BringToFront", Description = "Request the application to come to the front of the other windows (first coming out of server mode if needed).")]
		void BringToFront();

	}

	/// <summary>
	/// This versino is required by the XmlRpc component; it just adds the IXmlRpcProxy
	/// </summary>
	public interface IServiceAppConnectorWithProxy : IServiceAppConnector, IXmlRpcProxy
	{
	}


	[XmlRpcService(
	  Name = "Service App Connector",
	  Description = "This is an XML-RPC service for finding or launching an instance of a service.",
	  AutoDocumentation = true)]
	public class ServiceAppConnector : MarshalByRefObject, IServiceAppConnector
	{
		public event EventHandler BringToFrontRequest;
		private List<string> _clientIds= new List<string>();

		public List<string> ClientIds
		{
			get { return _clientIds; }
		}

		public void BringToFront()
		{
			if (BringToFrontRequest != null)
			{
				BringToFrontRequest.Invoke(this, null);
			}
		}

		public bool Ping()
		{
			return true;
		}
	}
}