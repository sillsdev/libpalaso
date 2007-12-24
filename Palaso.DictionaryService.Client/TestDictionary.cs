using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.DictionaryService.Client
{
	internal class TestDictionary : IDictionary
	{
		private List<IEntry> _entries = new List<IEntry>();
		private bool _isDisposed;

		/// <summary>
		/// clients must use a factory method
		/// </summary>
		internal TestDictionary(string writingSystemId)
		{
			Init(writingSystemId);
		}
		internal TestDictionary()
		{
		}

		public void Init(string writingSystemId)
		{
			foreach (string s in new string[] { "apple", "pear", "mango", "orange", "banana", "papaya" })
			{
				TestEntry e = new TestEntry();
				e.AddLexemeForm(writingSystemId, s);
				e.AddPrimaryDefinition("en", string.Format("A kind of fruit."));
				e.AddPrimaryExampleSentence(writingSystemId, string.Format("I want to drink a {0} shake", s));
				AddEntry(e);
			}
		}

		public bool CanAddEntries()
		{
		   return true;
		}

		public IEntry CreateEntryLocally()
		{
			return new TestEntry();
		}

		public void AddEntry(IEntry entry)
		{
			_entries.Add(entry);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			_isDisposed = true;
		}

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. _isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~TestDictionary()
		{
			if (!this._isDisposed)
			{
				throw new InvalidOperationException("Disposed not explicitly called on " + GetType().FullName + ".");
			}

		}



		public IList<IEntry> FindEntries(string writingSystemId, string form, FindMethods method)
		{
			List<IEntry> matches = new List<IEntry>();
			foreach (TestEntry entry in _entries)
			{
				if( entry.IsMatch(writingSystemId, form, method))
				{
					matches.Add(entry);
				}
			}
			return matches;
		}
	}
}
