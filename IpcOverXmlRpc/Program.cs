using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using CookComputing.XmlRpc;
using Palaso.Services.ForServers;

namespace IpcOverXmlRpc
{
	[XmlRpcService]
	public class MathService : MarshalByRefObject, IMathService
	{
		public int Square(int x)
		{
			return x*x;
		}
	}

	public interface IMathService
	{
		[XmlRpcMethod("MathService.Square")]
		int Square(int x);
	}

	public interface IMathServiceWithProxy : IMathService, IXmlRpcProxy
	{
	}

	class Program
	{
		static void Main(string[] args)
		{
			 int port = 8888;
			if (args.Length > 0)
			{
				RegisterHttpChannel(port);

				//start service
				MathService service = new MathService();
				RemotingServices.Marshal(service, "math");
				Console.WriteLine("Press <ENTER> to shutdown");
				Console.ReadLine();
			}
			else
			{
				Console.WriteLine("Testing Client...");

				 //consume service
				IMathServiceWithProxy proxy = XmlRpcProxyGen.Create<IMathServiceWithProxy>();
				proxy.Url = "http://localhost:"+port+"/math";
				Debug.Assert(4 == proxy.Square(2));
				Console.WriteLine("OK");
				Console.WriteLine("Press <ENTER> to shutdown");
				Console.ReadLine();
			}
		}

		private static void RegisterHttpChannel(int port)
		{
			IDictionary props = new Hashtable();
			props["port"] = port;
			IChannel aChannel = new HttpChannel(props, null, new XmlRpcServerFormatterSinkProvider());
			ChannelServices.RegisterChannel(aChannel, false);
		}
	}
}
