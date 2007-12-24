using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Palaso.DictionaryService.Client
{
	public class ServiceFinder :IDisposable
	{
		//private static System.Collections.Generic.Dictionary<string, DictionaryService.Client.Dictionary> s_dictionaries;
		private IDictionary _testDictionary;
		private bool _isDisposed=false;
		private bool _initialized=false;
		private string _writingSystemIdOfPretendDictionary=string.Empty;
		private Thread _testServerThread;

		public ServiceFinder()
		{
		}

		public IDictionary GetDictionaryService(string writingSystemId)
		{
			EnsureWasInitialized();
			if (writingSystemId == _writingSystemIdOfPretendDictionary)
			{
				return _testDictionary;
			}

			try
			{
				ChannelFactory<IDictionary> channelFactory;
				channelFactory = new ChannelFactory<IDictionary>(
					new NetTcpBinding(),
					"net.tcp://localhost:8000");

				IDictionary service;
				service = channelFactory.CreateChannel();
				(service as ICommunicationObject).Open(); // will throw exception if can't find it
				return service;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		private void EnsureWasInitialized()
		{
			if (!_initialized)
			{
				throw new ApplicationException("Must call Init() on ServiceFinder before using it.");
			}
		}

		public void ClearTestDictionary()
		{
			EnsureWasInitialized();
			if (_testDictionary != null)
			{
				_testDictionary.Dispose();
			}
			_testDictionary = null;
			_writingSystemIdOfPretendDictionary = string.Empty;
		}



			public void Dispose()
		{
			EnsureWasInitialized();
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		protected virtual void Dispose(bool disposing)
		{
			if (_testServerThread != null)
			{
				TestDictionaryServer.Stop();
				_testServerThread.Join();
			}
			ClearTestDictionary();
			_isDisposed = true;
		}

		~ServiceFinder()
		{
			if (!this._isDisposed)
			{
				throw new InvalidOperationException("Disposed not explicitly called on " + GetType().FullName + ".");
			}

		}


		public void Init()
		{
			_initialized = true;
		}

		public void LoadTestDictionary(string writingSystemId, bool createInSeperateThread)
		{
			EnsureWasInitialized();
			_writingSystemIdOfPretendDictionary = writingSystemId;
			if (_testDictionary == null)
			{
				if (!createInSeperateThread)
				{
					_testDictionary = new TestDictionary(writingSystemId);
				}
				else
				{
					// start server
					TestDictionaryServer.writingSystemIdToUseWhenStarting = writingSystemId;
					_testServerThread = new System.Threading.Thread(TestDictionaryServer.Main);
					_testServerThread.IsBackground = true;
					_testServerThread.Start();
					System.Threading.Thread.Sleep(1000);  // wait for server to start up
				}
			}
		}
	}
}
