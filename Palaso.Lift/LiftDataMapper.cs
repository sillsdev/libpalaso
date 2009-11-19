using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LiftIO;
using LiftIO.Merging;
using Palaso.Data;
using Palaso.Lift.Migration;
using Palaso.Misc;
using Palaso.Progress;
using Palaso.Reporting;

//using WeSay.LexicalModel.Migration;

namespace Palaso.Lift
{
	public class LiftDataMapper<T>: IDataMapper<T> where T : class, new()
	{
		private readonly ProgressState _progressState;
		private readonly string _liftFilePath;
		private FileStream _liftFileStreamForLocking;
		private int _nextFileOrder;

		private readonly ILiftReaderWriterProvider<T> _ioProvider;
		private MemoryDataMapper<T> _backend;

		public LiftDataMapper(string liftFilePath, ProgressState progressState, ILiftReaderWriterProvider<T> ioProvider)
		{
			//set to true so that an exception in the constructor does not cause the destructor to throw
			_disposed = true;
			Guard.AgainstNull(progressState, "progressState");

			_backend = new MemoryDataMapper<T>();
			_liftFilePath = liftFilePath;
			_progressState = progressState;
			_ioProvider = ioProvider;

			try
			{
				CreateEmptyLiftFileIfNeeded(liftFilePath);
				MigrateLiftIfNeeded(progressState);
				LoadAllLexEntries();
			}
			catch (Exception error)
			{
				// Dispose anything that we've created already.
				_backend.Dispose();
				throw;
			}
			//Now that the constructor has not thrown we can set this back to false
			_disposed = false;
		}

		public DateTime LastModified
		{
			get { return _backend.LastModified; }
		}

		public bool CanQuery
		{
			get { return _backend.CanQuery; }
		}

		private void CreateEmptyLiftFileIfNeeded(string filePath)
		{
			var fileInfo = new FileInfo(_liftFilePath);
			bool DoesEmptyLiftFileNeedtoBeCreated = !fileInfo.Exists || fileInfo.Length == 0;
			if (DoesEmptyLiftFileNeedtoBeCreated)
			{
				CreateEmptyLiftFile(filePath);
			}
		}

		private void MigrateLiftIfNeeded(ProgressState progressState)
		{
			var preparer = new LiftPreparer(_liftFilePath);
			if (preparer.IsMigrationNeeded())
			{
				preparer.MigrateLiftFile(progressState);
			}
			//now done in code as each entry is parsed in: preparer.PopulateDefinitions(progressState);
		}

		private void CreateEmptyLiftFile(string liftFilePath)
		{
			using (ILiftWriter<T> liftWriter = _ioProvider.CreateWriter(liftFilePath))
			{
				liftWriter.End();
			}
		}

		public T CreateItem()
		{
			T item = _backend.CreateItem();
			return item;
		}

		public int CountAllItems()
		{
			return _backend.CountAllItems();
		}

		public RepositoryId GetId(T item)
		{
			return _backend.GetId(item);
		}

		protected int NextFileOrder
		{
			get
			{
				return ++_nextFileOrder;
			}
		}

		public void DeleteItem(RepositoryId id)
		{
			T itemToDelete = GetItem(id);
			DeleteItem(itemToDelete);
		}

		public T GetItem(RepositoryId id)
		{
			return _backend.GetItem(id);
		}

		public void DeleteItem(T item)
		{
			Guard.AgainstNull(item, "item");

			_backend.GetId(item); // Will throw if item isn't in the backend data mapper.
			UpdateLiftFileWithDeleted(item);
			_backend.DeleteItem(item);
		}

		public void DeleteAllItems()
		{
			RepositoryId[] idsOfEntriesInRepository = GetAllItems();
			foreach (RepositoryId id in idsOfEntriesInRepository)
			{
				DeleteItem(id);
			}
		}

		public RepositoryId[] GetAllItems()
		{
			return _backend.GetAllItems();
		}

		public void SaveItem(T item)
		{
			_backend.SaveItem(item);
			UpdateLiftFileWithModified(item);
		}

		public void SaveItems(IEnumerable<T> items)
		{
			_backend.SaveItems(items);
			UpdateLiftFileWithModified(items);
		}

		public ResultSet<T> GetItemsMatching(IQuery<T> query)
		{
			return _backend.GetItemsMatching(query);
		}

		public bool CanPersist
		{
			get { return true; }
		}

		private void LoadAllLexEntries()
		{
			using (ILiftReader<T> reader = _ioProvider.CreateReader())
			{
				reader.Read(_liftFilePath, _backend);
			}
		}

		private string LiftDirectory
		{
			get { return Path.GetDirectoryName(_liftFilePath); }
		}

		private void UpdateLiftFileWithModified(IEnumerable<T> entriesToUpdate)
		{
			CreateFileContainingModified(entriesToUpdate);
			MergeIncrementFiles();
		}

		private void UpdateLiftFileWithModified(T entryToUpdate)
		{
			CreateFileContainingModified(entryToUpdate);
			MergeIncrementFiles();
		}

		private void UpdateLiftFileWithNew(T entryToUpdate)
		{
			CreateFileContainingNew(entryToUpdate);
			MergeIncrementFiles();
		}

		private void UpdateLiftFileWithDeleted(T entryToDelete)
		{
			CreateFileContainingDeleted(entryToDelete);
			MergeIncrementFiles();
		}

		private ILiftWriter<T> CreateExporter()
		{
			ILiftWriter<T> liftWriter = _ioProvider.CreateWriter(MakeIncrementFileName(PreciseDateTime.UtcNow));
			return liftWriter;
		}

		private void CreateFileContainingNew(T entry)
		{
			using (ILiftWriter<T> liftWriter = CreateExporter())
			{
				liftWriter.AddNewEntry(entry);
				liftWriter.End();
			}
		}

		private bool _reentryBugCatcherIn_CreateFileContainingModified = false;
		private void CreateFileContainingModified(T entryToUpdate)
		{
			if (_reentryBugCatcherIn_CreateFileContainingModified)
				throw new ApplicationException("CreateFileContainingModified called again before completing.");

			_reentryBugCatcherIn_CreateFileContainingModified = true;
#if DEBUG
			Logger.WriteMinorEvent("Start CreateFileContainingModified()");
#endif
			try
			{
				using (ILiftWriter<T> liftWriter = CreateExporter())
				{
					liftWriter.Add(entryToUpdate);
					liftWriter.End();
				}
			}
			catch (Exception e)
			{
				Logger.WriteEvent(e.Message);
				throw e;
			}
			finally
			{
				_reentryBugCatcherIn_CreateFileContainingModified = false;
			}

#if DEBUG
			Logger.WriteMinorEvent("End CreateFileContainingModified()");
#endif
		}

		private void CreateFileContainingModified(IEnumerable<T> entriesToUpdate)
		{
			ILiftWriter<T> liftWriter = null;
			foreach (var entry in entriesToUpdate)
			{
				if (liftWriter == null)
				{
					liftWriter = CreateExporter();
				}
				liftWriter.Add(entry);
			}
			if (liftWriter != null)
			{
				liftWriter.End();
				liftWriter.Dispose();
			}
		}

		private void CreateFileContainingDeleted(T entryToDelete)
		{
			using (ILiftWriter<T> liftWriter = CreateExporter())
			{
				liftWriter.AddDeletedEntry(entryToDelete);
				liftWriter.End();
			}
		}

		private void CreateFileContainingDeleted(IEnumerable<T> entriesToDelete)
		{
			ILiftWriter<T> liftWriter = null;
			foreach (T entry in entriesToDelete)
			{
				if (liftWriter == null)
				{
					liftWriter = CreateExporter();
				}
				liftWriter.AddDeletedEntry(entry);
			}
			if (liftWriter != null)
			{
				liftWriter.End();
				liftWriter.Dispose();
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>false if it failed (and it would have already reported the error)</returns>
		private void MergeIncrementFiles()
		{
			//merge the increment files

			if (SynchronicMerger.GetPendingUpdateFiles(_liftFilePath).Length > 0)
			{
#if DEBUG
				Logger.WriteMinorEvent("++before pending updates: {0}", SynchronicMerger.GetPendingUpdateFiles(_liftFilePath).Length);
#endif
				Logger.WriteEvent("Running Synchronic Merger");
				try
				{
					var merger = new SynchronicMerger();
					merger.MergeUpdatesIntoFile(_liftFilePath);
				}
				catch (BadUpdateFileException error)
				{
					string contents = File.ReadAllText(error.PathToNewFile);
					if (contents.Trim().Length == 0)
					{
						ErrorReport.NotifyUserOfProblem(
								"It looks as though WeSay recently crashed while attempting to save.  It will try again to preserve your work, but you will want to check to make sure nothing was lost.");
						File.Delete(error.PathToNewFile);
					}
					else
					{
						File.Move(error.PathToNewFile, error.PathToNewFile + ".bad");
						ErrorReport.NotifyUserOfProblem(
								"WeSay was unable to save some work you did in the previous session.  The work might be recoverable from the file {0}. The next screen will allow you to send a report of this to the developers.",
								error.PathToNewFile + ".bad");
						ErrorReport.ReportNonFatalException(error);
					}
					//return false; //!!! remove CJP
				}
				catch (Exception e)
				{
					ErrorReport.NotifyUserOfProblem(
							"Could not finish updating LIFT dictionary file. Will try again later." + Environment.NewLine + " (" + e.Message + ")");
				}
#if DEBUG
				Logger.WriteMinorEvent("--after pending updates: {0}", SynchronicMerger.GetPendingUpdateFiles(_liftFilePath).Length);
#endif
			}
		}

		private string MakeIncrementFileName(DateTime time)
		{
			while (true)
			{
				string timeString = time.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss'-'FFFFFFF UTC");
				string path = Path.Combine(LiftDirectory, timeString);
				path += SynchronicMerger.ExtensionOfIncrementalFiles;
				if (!File.Exists(path))
				{
					return path;
				}
				time = time.AddTicks(1);
			}
		}

		/// <remark>
		/// The protection provided by this simple approach is obviously limited;
		/// it will keep the lift file safe normally... but could lead to non-data-losing crashes
		/// if some automated process was sitting out there, just waiting to open as soon as we realease
		/// </summary>
		private void UnLockLift()
		{
			Debug.Assert(_liftFileStreamForLocking != null);
			_liftFileStreamForLocking.Close();
			_liftFileStreamForLocking.Dispose();
			_liftFileStreamForLocking = null;
		}

		#region IDisposable Members

#if DEBUG
		~LiftDataMapper()
		{
			if (!_disposed)
			{
				throw new ApplicationException("Disposed not explicitly called on LiftDataMapper.");
			}
		}
#endif

		[CLSCompliantAttribute(false)]
		protected bool _disposed;

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
					_backend.Dispose();
				}

				// shared (dispose and finalizable) cleanup logic
				_disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("LiftDataMapper");
			}
		}

		#endregion

	}

}