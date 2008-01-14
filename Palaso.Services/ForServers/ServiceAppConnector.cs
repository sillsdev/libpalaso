using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Palaso.Services.ForServers
{
	/// <summary>
	/// this is the outward-facing contract. Other apps talk to this one through these methods.
	/// Its main job, though, is just to "exist" on a named pipe, so that only a single application
	/// will be launched to provide the service, regardless of how many times the  user or another
	/// program tells it to open.
	/// </summary>
	[ServiceContract]
	public interface IServiceAppConnector
	{
		[OperationContract]
		void BringToFront();
	}

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class ServiceAppConnector : IServiceAppConnector
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
	}
}