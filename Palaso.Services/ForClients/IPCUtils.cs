using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Palaso.Services.ForClients
{
	public class IPCUtils
	{
		private static bool _isWcfAvailable = (Environment.OSVersion.Platform != PlatformID.Unix);


		//this is exposed for tests to simulate the Feb 2008 linux situation, with no wcf that we can use
		internal static bool IsWcfAvailable
		{
			get { return _isWcfAvailable; }
			set { _isWcfAvailable = value; }
		}

		public  static Binding  CreateBinding()
		{
			AssertWcfAvailable();
			return new NetNamedPipeBinding();
		}

		public static string URLPrefix
		{
			get
			{
				AssertWcfAvailable();
				return "net.pipe://localhost/";
			}
		}
		public static ServiceInterface GetExistingService<ServiceInterface>(string address)
			where ServiceInterface : class
		{
			AssertWcfAvailable();
			ChannelFactory<ServiceInterface> channelFactory;
			Binding binding = CreateBinding();

			channelFactory = new ChannelFactory<ServiceInterface>(
				binding,
				address);

			ServiceInterface helper = channelFactory.CreateChannel();
			try
			{
				(helper as ICommunicationObject).Open(); // will throw exception if can't find it
			}
			catch (Exception)
			{
				return null;
			}
			return helper;
		}

		private static void AssertWcfAvailable()
		{
			Debug.Assert(IPCUtils._isWcfAvailable, "This should not be called if we don't have WCF");
		}
	}
}