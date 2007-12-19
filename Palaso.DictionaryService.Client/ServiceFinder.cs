using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.DictionaryService.Client
{
	public class ServiceFinder :IDisposable
	{
		//private static System.Collections.Generic.Dictionary<string, DictionaryService.Client.Dictionary> s_dictionaries;
		private IDictionary _testDictionary;
		private bool _isDisposed=false;
		private bool _initialized=false;
		private string _writingSystemIdOfPretendDictionary=string.Empty;

		public ServiceFinder()
		{
		}

		public IDictionary GetDictionaryService(string writingSystemId)
		{
			EnsureWasInitialized();
			if (writingSystemId != _writingSystemIdOfPretendDictionary)
				return null;
			return _testDictionary;
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

		public void LoadTestDictionary(string writingSystemId)
		{
			EnsureWasInitialized();
			_writingSystemIdOfPretendDictionary = writingSystemId;
			if (_testDictionary == null)
			{
				_testDictionary = new TestDictionary(writingSystemId);
			}
		}
	}
}
