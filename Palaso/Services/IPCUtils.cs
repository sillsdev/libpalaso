using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;

namespace SampleDictionaryServicesApplication
{
	public class IPCUtils
	{
		public static ServiceInterface GetExistingService<ServiceInterface>(string address)
			where ServiceInterface : class
		{
			ChannelFactory<ServiceInterface> channelFactory;
			channelFactory = new ChannelFactory<ServiceInterface>(
				new NetNamedPipeBinding(),
			   address);

			ServiceInterface helper = channelFactory.CreateChannel();
			try
			{
				(helper as ICommunicationObject).Open(); // will throw exception if can't find it
			}
			catch (Exception e)
			{
				return null;
			}
			return helper;
		}


	}
}
