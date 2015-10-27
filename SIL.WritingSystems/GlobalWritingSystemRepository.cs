using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems
{
	///<summary>
	/// A system wide writing system repository.
	///</summary>
	public class GlobalWritingSystemRepository : GlobalWritingSystemRepository<WritingSystemDefinition>
	{
		///<summary>
		/// Initializes the global writing system repository.  Migrates any ldml files if required,
		/// notifying of any changes of writing system id that occured during migration.
		///</summary>
		public static GlobalWritingSystemRepository Initialize(Action<int, IEnumerable<LdmlMigrationInfo>> migrationHandler = null)
		{
			return InitializeWithBasePath(DefaultBasePath, migrationHandler);
		}

		///<summary>
		/// This initializer is intended for tests as it allows setting of the basePath explicitly.
		///</summary>
		internal static GlobalWritingSystemRepository InitializeWithBasePath(string basePath,
			Action<int, IEnumerable<LdmlMigrationInfo>> migrationHandler)
		{
			var migrator = new GlobalWritingSystemRepositoryMigrator(basePath, migrationHandler);
			if (migrator.NeedsMigration())
				migrator.Migrate();

			var globalRepo = new GlobalWritingSystemRepository(basePath);

			migrator.ResetRemovedProperties(globalRepo);

			return globalRepo;
		}

		protected internal GlobalWritingSystemRepository(string basePath)
			: base(basePath)
		{
		}

		protected override IWritingSystemFactory<WritingSystemDefinition> CreateWritingSystemFactory()
		{
			return new SldrWritingSystemFactory();
		}
	}

	///<summary>
	/// A system wide writing system repository.
	///</summary>
	public abstract class GlobalWritingSystemRepository<T> : IWritingSystemRepository<T>, IDisposable where T : WritingSystemDefinition
	{
		private const string Extension = ".ldml";

		public event EventHandler<WritingSystemIdChangedEventArgs> WritingSystemIdChanged;
		public event EventHandler<WritingSystemDeletedEventArgs> WritingSystemDeleted;
		public event EventHandler<WritingSystemConflatedEventArgs> WritingSystemConflated;

		private readonly string _path;
		/// <summary>Reference to a mutex. The owner of the mutex is the SingletonContainer</summary>
		private readonly Mutex _mutex;
		private IWritingSystemFactory<T> _writingSystemFactory;

		protected internal GlobalWritingSystemRepository(string basePath)
		{
			_path = CurrentVersionPath(basePath);
			if (!Directory.Exists(_path))
			{
				DirectoryInfo di;

				// Provides FW on Linux multi-user access. Overrides the system
				// umask and creates the directory with the permissions "775".
				// The "fieldworks" group was created outside the app during
				// configuration of the package which allows group access.
				using (new FileModeOverride())
				{
					di = Directory.CreateDirectory(_path);
				}

				if (!Platform.IsLinux)
				{
					// NOTE: GetAccessControl/ModifyAccessRule/SetAccessControl is not implemented in Mono
					DirectorySecurity ds = di.GetAccessControl();
					var sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
					AccessRule rule = new FileSystemAccessRule(sid, FileSystemRights.Write | FileSystemRights.ReadAndExecute
						| FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
						PropagationFlags.InheritOnly, AccessControlType.Allow);
					bool modified;
					ds.ModifyAccessRule(AccessControlModification.Add, rule, out modified);
					di.SetAccessControl(ds);
				}
			}
			_mutex = new Mutex(false, _path.Replace('\\', '_').Replace('/', '_'));
		}

		///<summary>
		/// The DefaultBasePath is %CommonApplicationData%\SIL\WritingSystemRepository
		/// On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\
		/// On Linux this must be in /var/lib so that it may be edited
		///</summary>
		public static string DefaultBasePath
		{
			get
			{
				string result = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (result == "/usr/share")
					result = "/var/lib";
				result = Path.Combine(result, "SIL", "WritingSystemRepository");
				return result;
			}
		}

		///<summary>
		/// The CurrentVersionPath is %CommonApplicationData%\SIL\WritingSystemRepository\[CurrentLdmlVersion]
		/// e.g. On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\1
		///</summary>
		public static string CurrentVersionPath(string basePath)
		{
			return Path.Combine(basePath, LdmlDataMapper.CurrentLdmlVersion.ToString(CultureInfo.InvariantCulture));
		}

		public string PathToWritingSystems
		{
			get { return _path; }
		}

		/// <summary>
		/// Adds the writing system to the store or updates the store information about
		/// an already-existing writing system.  Set should be called when there is a change
		/// that updates the IETF language tag information.
		/// </summary>
		public void Set(T ws)
		{
			_mutex.WaitOne();
			MemoryStream oldData = null;
			try
			{
				string writingSystemFileName = GetFileNameFromLanguageTag(ws.LanguageTag);
				string writingSystemFilePath = GetFilePathFromLanguageTag(ws.LanguageTag);
				if (!ws.IsChanged && File.Exists(writingSystemFilePath))
					return; // no need to save (better to preserve the modified date)
				string oldId = ws.Id;
				ws.Id = ws.LanguageTag;
				string incomingFileName = GetFileNameFromLanguageTag(oldId);
				string incomingFilePath = GetFilePathFromLanguageTag(oldId);
				if (!string.IsNullOrEmpty(incomingFileName))
				{
					if (File.Exists(incomingFilePath))
					{
						// load old data to preserve stuff in LDML that we don't use, but don't throw up an error if it fails
						try
						{
							oldData = new MemoryStream(File.ReadAllBytes(incomingFilePath), false);
						}
						catch
						{
						}
						if (writingSystemFileName != incomingFileName)
						{
							File.Delete(incomingFilePath);
							// JohnT: Added this without fully understanding, to get things to compile. I don't fully
							// know when this event should be raised, nor am I sure I am building the argument correctly.
							// However, I don't think anything (at least in our code) actually uses it.
							if (WritingSystemIdChanged != null)
								WritingSystemIdChanged(this, new WritingSystemIdChangedEventArgs(oldId, ws.LanguageTag));
						}
					}
				}
				var ldmlDataMapper = new LdmlDataMapper(WritingSystemFactory);
				try
				{
					// Provides FW on Linux multi-user access. Overrides the system
					// umask and creates the files with the permissions "775".
					// The "fieldworks" group was created outside the app during
					// configuration of the package which allows group access.
					using (new FileModeOverride())
					{
						ldmlDataMapper.Write(writingSystemFilePath, ws, oldData);
					}
				}
				catch (UnauthorizedAccessException)
				{
					// If we can't save the changes, too bad. Inability to save locally is typically caught
					// when we go to open the modify dialog. If we can't make the global store consistent,
					// as we well may not be able to in a client-server mode, too bad.
				}

				ws.AcceptChanges();
			}
			finally
			{
				if (oldData != null)
					oldData.Dispose();
				_mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Returns true if a call to Set should succeed, false if a call to Set would throw
		/// </summary>
		public bool CanSet(T ws)
		{
			return true;
		}

		/// <summary>
		/// Gets the writing system object for the given Store ID
		/// </summary>
		public T Get(string id)
		{
			_mutex.WaitOne();
			try
			{
				string filePath = GetFilePathFromLanguageTag(id);
				if (!File.Exists(filePath))
					throw new ArgumentOutOfRangeException("Missing file for writing system code: " + id);
				return GetFromFilePath(filePath);
			}
			finally
			{
				_mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// If the given writing system were passed to Set, this function returns the
		/// new StoreID that would be assigned.
		/// </summary>
		public string GetNewIdWhenSet(T ws)
		{
			if (ws == null)
				throw new ArgumentNullException("ws");

			return ws.LanguageTag;
		}

		/// <summary>
		/// Returns true if a writing system with the given Store ID exists in the store
		/// </summary>
		public bool Contains(string id)
		{
			_mutex.WaitOne();
			try
			{
				return File.Exists(GetFilePathFromLanguageTag(id));
			}
			finally
			{
				_mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Gives the total number of writing systems in the store
		/// </summary>
		public int Count
		{
			get
			{
				_mutex.WaitOne();
				try
				{
					return Directory.GetFiles(_path, "*.ldml").Length;
				}
				finally
				{
					_mutex.ReleaseMutex();
				}
			}
		}

		/// <summary>
		/// This is a new required interface member. We don't use it, and I hope we don't use anything which uses it!
		/// </summary>
		/// <param name="wsToConflate"></param>
		/// <param name="wsToConflateWith"></param>
		public void Conflate(string wsToConflate, string wsToConflateWith)
		{
			_mutex.WaitOne();
			try
			{
				File.Delete(GetFilePathFromLanguageTag(wsToConflate));
				if (WritingSystemConflated != null)
					WritingSystemConflated(this, new WritingSystemConflatedEventArgs(wsToConflate, wsToConflateWith));
			}
			finally
			{
				_mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Removes the writing system with the specified Store ID from the store.
		/// </summary>
		public void Remove(string id)
		{
			_mutex.WaitOne();
			try
			{
				File.Delete(GetFilePathFromLanguageTag(id));
				if (WritingSystemDeleted != null)
					WritingSystemDeleted(this, new WritingSystemDeletedEventArgs(id));
			}
			finally
			{
				_mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Returns a list of all writing system definitions in the store.
		/// </summary>
		public IEnumerable<T> AllWritingSystems
		{
			get
			{
				_mutex.WaitOne();
				try
				{
					return Directory.GetFiles(_path, "*.ldml").Select(GetFromFilePath).ToArray();
				}
				finally
				{
					_mutex.ReleaseMutex();
				}
			}
		}

		/// <summary>
		/// This is used by the orphan finder, which we don't use (yet). It tells whether, typically in the scope of some
		/// current change log, a writing system ID has changed to something else...call WritingSystemIdHasChangedTo
		/// to find out what.
		/// </summary>
		public bool WritingSystemIdHasChanged(string id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// This is used by the orphan finder, which we don't use (yet). It tells what, typically in the scope of some
		/// current change log, a writing system ID has changed to.
		/// </summary>
		public string WritingSystemIdHasChangedTo(string id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Writes the store to a persistable medium, if applicable.
		/// </summary>
		public void Save()
		{
		}

		public IWritingSystemFactory<T> WritingSystemFactory
		{
			get
			{
				if (_writingSystemFactory == null)
					_writingSystemFactory = CreateWritingSystemFactory();
				return _writingSystemFactory;
			}
		}

		protected abstract IWritingSystemFactory<T> CreateWritingSystemFactory();

		/// <summary>
		/// Since the current implementation of Save does nothing, it's always possible.
		/// </summary>
		public bool CanSave(T ws)
		{
			string filePath = GetFilePathFromLanguageTag(ws.Id);
			if (File.Exists(filePath))
			{
				try
				{
					using (FileStream stream = File.Open(filePath, FileMode.Open))
						stream.Close();
					// don't really want to change anything
				}
				catch (UnauthorizedAccessException)
				{
					return false;
				}
			}
			else if (Directory.Exists(PathToWritingSystems))
			{
				try
				{
					// See whether we're allowed to create the file (but if so, get rid of it).
					// Pathologically we might have create but not delete permission...if so,
					// we'll create an empty file and report we can't save. I don't see how to
					// do better.
					using (FileStream stream = File.Create(filePath))
						stream.Close();
					File.Delete(filePath);
				}
				catch (UnauthorizedAccessException)
				{
					return false;
				}
			}
			else
			{
				try
				{
					Directory.CreateDirectory(PathToWritingSystems);
					// Don't try to clean it up again. This is a vanishingly rare case,
					// I don't think it's even possible to create a writing system store without
					// the directory existing.
				}
				catch (UnauthorizedAccessException)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Gets the specified writing system if it exists.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="ws">The writing system.</param>
		/// <returns></returns>
		public bool TryGet(string id, out T ws)
		{
			_mutex.WaitOne();
			try
			{
				if (Contains(id))
				{
					ws = Get(id);
					return true;
				}

				ws = null;
				return false;
			}
			finally
			{
				_mutex.ReleaseMutex();
			}
		}

		bool IWritingSystemRepository.CanSet(WritingSystemDefinition ws)
		{
			return CanSet((T) ws);
		}

		void IWritingSystemRepository.Set(WritingSystemDefinition ws)
		{
			Set((T) ws);
		}

		WritingSystemDefinition IWritingSystemRepository.Get(string id)
		{
			return Get(id);
		}

		bool IWritingSystemRepository.TryGet(string id, out WritingSystemDefinition ws)
		{
			T result;
			if (TryGet(id, out result))
			{
				ws = result;
				return true;
			}

			ws = null;
			return false;
		}

		string IWritingSystemRepository.GetNewIdWhenSet(WritingSystemDefinition ws)
		{
			return GetNewIdWhenSet((T) ws);
		}

		bool IWritingSystemRepository.CanSave(WritingSystemDefinition ws)
		{
			return CanSave((T) ws);
		}

		IEnumerable<WritingSystemDefinition> IWritingSystemRepository.AllWritingSystems
		{
			get { return AllWritingSystems; }
		}

		IWritingSystemFactory IWritingSystemRepository.WritingSystemFactory
		{
			get { return WritingSystemFactory; }
		}

		private T GetFromFilePath(string filePath)
		{
			try
			{
				T ws = WritingSystemFactory.Create();
				var ldmlDataMapper = new LdmlDataMapper(WritingSystemFactory);
				ldmlDataMapper.Read(filePath, ws);
				ws.Id = ws.LanguageTag;
				ws.AcceptChanges();
				return ws;
			}
			catch (Exception e)
			{
				throw new ArgumentException("GlobalWritingSystemRepository was unable to load the LDML file " + filePath, "filePath", e);
			}
		}

		///<summary>
		/// Returns the full path to the underlying store for this writing system.
		///</summary>
		public string GetFilePathFromLanguageTag(string langTag)
		{
			return Path.Combine(PathToWritingSystems, GetFileNameFromLanguageTag(langTag));
		}

		/// <summary>
		/// Gets the file name from the specified identifier.
		/// </summary>
		protected static string GetFileNameFromLanguageTag(string langTag)
		{
			return langTag + Extension;
		}

		~GlobalWritingSystemRepository()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		public bool IsDisposed { get; private set; }

		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			System.Diagnostics.Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
				_mutex.Dispose();
			IsDisposed = true;
		}
	}
}
