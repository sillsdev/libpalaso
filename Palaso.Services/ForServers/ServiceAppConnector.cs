using System;
using System.Collections.Generic;
#if !MONO
using System.ServiceModel;
#endif

namespace Palaso.Services.ForServers
{
	/// <summary>
	/// this is the outward-facing contract. Other apps talk to this one through these methods.
	/// Its main job, though, is just to "exist" on a named pipe, so that only a single application
	/// will be launched to provide the service, regardless of how many times the  user or another
	/// program tells it to open.
	/// </summary>
#if !MONO
	[ServiceContract]
#endif
	public interface IServiceAppConnector
	{
#if !MONO
		[OperationContract]
#endif
		void BringToFront();
	}

#if !MONO
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
		IncludeExceptionDetailInFaults = true)]
#endif
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