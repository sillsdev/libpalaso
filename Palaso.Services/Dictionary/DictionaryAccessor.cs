using System;
using System.Diagnostics;
using Palaso.Services.ForClients;

namespace Palaso.Services.Dictionary
{
	public class DictionaryAccessor : IDisposable
	{
		private bool _isDisposed;
		private string _dictionaryPath;
		private string  _pathToDictionaryServicesApp;
		private bool _isRegisteredWithService=false;

		public delegate void LogEventHandler(string s, params object[] arguments);
		public event LogEventHandler ErrorLog;

		public DictionaryAccessor(string dictionaryPath, string pathToDicionaryServicesApp)
		{
			_dictionaryPath = dictionaryPath;
			_pathToDictionaryServicesApp = pathToDicionaryServicesApp;
		}

		private void Log(string s, params object[] arguments)
		{
			if (ErrorLog != null)
			{
				ErrorLog.Invoke(s, arguments);
			}
		}

		/// <summary>
		/// Important: Do not store this locally. Use this accessor for every call to the service.
		/// </summary>
		/// <remarks>This is important because the serivce may have quit since you last retrieved this
		/// vale; this property will attempt to reconnect or even restart the service, as needed.</remarks>
		protected IDictionaryService Service
		{
			get { return GetDictionaryService(); }
		}

		private IDictionaryService GetDictionaryService()
		{
			IDictionaryService dictionaryService = IPCUtils.GetExistingService<IDictionaryService>(ServiceAddress);
			if (dictionaryService == null)
			{
				string arguments = '"' + _dictionaryPath + '"' + " -server";
				Log("Starting service as [{0} {1}]...", _pathToDictionaryServicesApp, arguments);
				System.Diagnostics.Process.Start(_pathToDictionaryServicesApp, arguments);
				for (int i = 0; i < 20; i++)
				{
					System.Threading.Thread.Sleep(500);
					dictionaryService = IPCUtils.GetExistingService<IDictionaryService>(ServiceAddress);
					if (dictionaryService != null)
					{
						dictionaryService.RegisterClient(Process.GetCurrentProcess().Id);
						_isRegisteredWithService = true;
						break;
					}
				}
			}
			if (dictionaryService == null)
			{
				Log("Failed to locate or start dictionary service");
			}
			return dictionaryService;
		}



		private string ServiceAddress
		{
			get
			{
				return IPCUtils.URLPrefix+"DictionaryServices/"
					   + Uri.EscapeDataString(_dictionaryPath);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		protected virtual void Dispose(bool disposing)
		{
			_isDisposed = true;
			if (_isRegisteredWithService)
			{
				IDictionaryService service = IPCUtils.GetExistingService<IDictionaryService>(ServiceAddress);
				if (service != null)
				{
					try
					{
						service.DeregisterClient(Process.GetCurrentProcess().Id);
						_isRegisteredWithService = false;
					}
					catch (Exception) //swallow
					{
					}
				}
			}
		}

		~DictionaryAccessor()
		{
			if (!this._isDisposed)
			{
				throw new InvalidOperationException("Disposed not explicitly called on " + GetType().FullName + ".");
			}

		}

		/// <summary>
		/// Search the dictionary for an ordered list of entries that may be what the user is looking for.
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <param name="form">The form to search on.  May be used to match on lexeme form, citation form, variants, etc.,
		/// depending on how the implementing dictionary services application.</param>
		/// <param name="method">Controls how matching should happen</param>
		/// <param name="ids">The ids of the returned elements, for use in other calls.</param>
		/// <param name="forms">The headwords of the matched elements.</param>
		public void GetMatchingEntries(string writingSystemId, string form, FindMethods method, out string[] ids, out string[] forms)
		{
			Service.GetMatchingEntries(writingSystemId, form, method, out ids, out forms);
		}

		/// <summary>
		/// Get an HTML representation of one or more entries.
		/// </summary>
		/// <param name="entryIds"></param>
		/// <returns></returns>
		public string GetHtmlForEntries(string[] entryIds)
		{
			return Service.GetHtmlForEntries(entryIds);
		}


		/// <summary>
		/// Cause a gui application to come to the front, focussed on this entry, read to edit
		/// </summary>
		/// <param name="entryId"></param>
		public void JumpToEntry(string entryId)
		{
			Service.JumpToEntry(entryId);
		}

		/// <summary>
		/// Add a new entry to the lexicon
		/// </summary>
		/// <returns>the id that was assigned to the new entry</returns>
		public string AddEntry(string lexemeFormWritingSystemId, string lexemeForm,
							   string definitionWritingSystemId, string definition,
							   string exampleWritingSystemId, string example)
		{
			return Service.AddEntry(lexemeFormWritingSystemId, lexemeForm,
									definitionWritingSystemId, definition,
									exampleWritingSystemId, example);
		}
	}
}