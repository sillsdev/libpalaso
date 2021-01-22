using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Threading;
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
	public abstract class GlobalWritingSystemRepository<T> : WritingSystemRepositoryBase<T>, IDisposable where T : WritingSystemDefinition
	{
		private const string Extension = ".ldml";

		private readonly string _path;
		private readonly GlobalMutex _mutex;
		private readonly Dictionary<string, Tuple<DateTime, long>> _lastFileStats;
		private readonly HashSet<string> _addedWritingSystems;

		private static string _defaultBasePath;

		protected internal GlobalWritingSystemRepository(string basePath)
		{
			_lastFileStats = new Dictionary<string, Tuple<DateTime, long>>(StringComparer.OrdinalIgnoreCase);
			_addedWritingSystems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_path = CurrentVersionPath(basePath);
			if (!Directory.Exists(_path))
				CreateGlobalWritingSystemRepositoryDirectory(_path);
			_mutex = new GlobalMutex(_path.Replace('\\', '_').Replace('/', '_'));
			_mutex.Initialize();
		}

		private void UpdateDefinitions()
		{
			var ldmlDataMapper = new LdmlDataMapper(WritingSystemFactory);
			var removedIds = new HashSet<string>(WritingSystems.Keys);
			foreach (var file in Directory.GetFiles(PathToWritingSystems, $"*{Extension}").OrderBy(filename => filename))
			{
				var fi = new FileInfo(file);
				string id = Path.GetFileNameWithoutExtension(file);
				Debug.Assert(id != null);
				T ws;
				if (WritingSystems.TryGetValue(id, out ws))
				{
					// existing writing system

					// preserve this repo's changes
					if (!ws.IsChanged)
					{
						// for performance purposes, we check the last modified timestamp and file size to see if the file has changed
						// hopefully that is good enough for our purposes here
						if (_lastFileStats[id].Item1 != fi.LastWriteTime || _lastFileStats[id].Item2 != fi.Length)
						{
							var errorEncountered = false;
							// modified writing system
							if (!_addedWritingSystems.Contains(id))
							{
								ldmlDataMapper.Read(file, ws, e => { errorEncountered = true; });
								if (string.IsNullOrEmpty(ws.Id))
									ws.Id = ws.LanguageTag;
								ws.AcceptChanges();
							}

							// if an error was encountered in reading the file above, the file will have been moved
							_lastFileStats[id] = Tuple.Create(
								errorEncountered ? DateTime.MinValue : fi.LastWriteTime,
								errorEncountered ? 0 : fi.Length);
						}
					}
					removedIds.Remove(id);
				}
				else
				{
					// new writing system
					ws = WritingSystemFactory.Create();
					var errorEncountered = false;
					ldmlDataMapper.Read(file, ws, e => { errorEncountered = true; });
					ws.Id = ws.LanguageTag;
					ws.AcceptChanges();
					WritingSystems[id] = ws;
					// if an error was encountered in reading the file above, the file will have been moved
					_lastFileStats[id] = Tuple.Create(
						errorEncountered ? DateTime.MinValue : fi.LastWriteTime,
						errorEncountered ? 0 : fi.Length);
				}
			}

			foreach (string id in removedIds)
			{
				// preserve this repo's changes
				if (!WritingSystems[id].IsChanged && !_addedWritingSystems.Contains(id))
					base.Remove(id);
			}
		}

		///<summary>
		/// The DefaultBasePath is %CommonApplicationData%\SIL\WritingSystemRepository
		/// On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\
		/// On Linux this must be in ~/.local/share so that it may be edited
		///</summary>
		public static string DefaultBasePath
		{
			get
			{
				// This allows unit tests to set the _defaultBasePath (through reflection)
				if (string.IsNullOrEmpty(_defaultBasePath))
				{
					var basePath = Platform.IsMac ? "/Users/Shared" : Platform.IsLinux
						? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
						: Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
					_defaultBasePath = Path.Combine(basePath, "SIL", "WritingSystemRepository");
				}

				return _defaultBasePath;
			}
		}

		///<summary>
		/// The CurrentVersionPath is %CommonApplicationData%\SIL\WritingSystemRepository\[CurrentLdmlVersion]
		/// e.g. On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\1
		///</summary>
		public static string CurrentVersionPath(string basePath)
		{
			return Path.Combine(basePath, LdmlDataMapper.CurrentLdmlLibraryVersion.ToString(CultureInfo.InvariantCulture));
		}

		public static void CreateGlobalWritingSystemRepositoryDirectory(string path)
		{
			DirectoryInfo di = Directory.CreateDirectory(path);
			if (!Platform.IsUnix && !path.StartsWith(Path.GetTempPath()))
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

		public string PathToWritingSystems
		{
			get { return _path; }
		}

		/// <summary>
		/// Adds the writing system to the store or updates the store information about
		/// an already-existing writing system.  Set should be called when there is a change
		/// that updates the IETF language tag information.
		/// </summary>
		public override void Set(T ws)
		{
			using (_mutex.Lock())
			{
				UpdateDefinitions();

				string oldStoreId = ws.Id;
				var isNewWs = WritingSystems.Count(kvp => kvp.Value.Id == ws.Id) == 0;

				base.Set(ws);

				//Renaming the file here is a bit ugly as the content has not yet been updated. Thus there
				//may be a mismatch between the filename and the contained rfc5646 tag. Doing it here however
				//helps us avoid having to deal with situations where a writing system id is changed to be
				//identical with the old id of another writing system. This could otherwise lead to dataloss.
				//The inconsistency is resolved on Save()
				if (oldStoreId != ws.Id && File.Exists(GetFilePathFromLanguageTag(oldStoreId)))
					File.Move(GetFilePathFromLanguageTag(oldStoreId), GetFilePathFromLanguageTag(ws.Id));
				else if (isNewWs && !ws.IsChanged && !_addedWritingSystems.Contains(ws.Id))
					_addedWritingSystems.Add(ws.Id);
			}
		}

		/// <summary>
		/// Gets the writing system object for the given Store ID
		/// </summary>
		public override T Get(string id)
		{
			using (_mutex.Lock())
			{
				UpdateDefinitions();
				return base.Get(id);
			}
		}

		/// <summary>
		/// This method will save the global store file in a temporary location while doing the base
		/// Replace (Remove/Set). This will leave the old file content available during the Save method so that
		/// it will round trip correctly.
		/// </summary>
		public override void Replace(string languageTag, T newWs)
		{
			using (new WsStasher(Path.Combine(PathToWritingSystems, languageTag + Extension)))
			{
				base.Replace(languageTag, newWs);
			}
		}

		/// <summary>
		/// Returns true if a writing system with the given Store ID exists in the store
		/// </summary>
		public override bool Contains(string id)
		{
			using (_mutex.Lock())
			{
				UpdateDefinitions();
				return base.Contains(id);
			}
		}

		/// <summary>
		/// Gives the total number of writing systems in the store
		/// </summary>
		public override int Count
		{
			get
			{
				using (_mutex.Lock())
				{
					UpdateDefinitions();
					return base.Count;
				}
			}
		}

		/// <summary>
		/// This is a new required interface member. We don't use it, and I hope we don't use anything which uses it!
		/// </summary>
		/// <param name="wsToConflate"></param>
		/// <param name="wsToConflateWith"></param>
		public override void Conflate(string wsToConflate, string wsToConflateWith)
		{
			using (_mutex.Lock())
			{
				UpdateDefinitions();
				base.Conflate(wsToConflate, wsToConflateWith);
			}
		}

		/// <summary>
		/// Removes the writing system with the specified Store ID from the store.
		/// </summary>
		public override void Remove(string id)
		{
			using (_mutex.Lock())
			{
				UpdateDefinitions();
				base.Remove(id);
				if (_addedWritingSystems.Contains(id))
					_addedWritingSystems.Remove(id);
			}
		}

		/// <summary>
		/// Returns a list of all writing system definitions in the store.
		/// </summary>
		public override IEnumerable<T> AllWritingSystems
		{
			get
			{
				using (_mutex.Lock())
				{
					UpdateDefinitions();
					return base.AllWritingSystems;
				}
			}
		}

		/// <summary>
		/// This is used by the orphan finder, which we don't use (yet). It tells whether, typically in the scope of some
		/// current change log, a writing system ID has changed to something else...call WritingSystemIdHasChangedTo
		/// to find out what.
		/// </summary>
		public override bool WritingSystemIdHasChanged(string id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// This is used by the orphan finder, which we don't use (yet). It tells what, typically in the scope of some
		/// current change log, a writing system ID has changed to.
		/// </summary>
		public override string WritingSystemIdHasChangedTo(string id)
		{
			throw new NotImplementedException();
		}

		protected override void RemoveDefinition(T ws)
		{
			string file = GetFilePathFromLanguageTag(ws.Id);
			if (File.Exists(file))
				File.Delete(file);
			base.RemoveDefinition(ws);
			if (_addedWritingSystems.Contains(ws.Id))
				_addedWritingSystems.Remove(ws.Id);
		}

		private void SaveDefinition(T ws)
		{
			base.Set(ws);

			string writingSystemFilePath = GetFilePathFromLanguageTag(ws.Id);
			if (!File.Exists(writingSystemFilePath) && !string.IsNullOrEmpty(ws.Template))
			{
				// this is a new writing system that was generated from a template, so copy the template over before saving
				File.Copy(ws.Template, writingSystemFilePath);
				ws.Template = null;
			}

			if (!ws.IsChanged && File.Exists(writingSystemFilePath) && !_addedWritingSystems.Contains(ws.Id))
				return; // no need to save (better to preserve the modified date)

			if (ws.IsChanged)
				ws.DateModified = DateTime.UtcNow;

			MemoryStream oldData = GetDataToMergeWithInSave(writingSystemFilePath);
			if (File.Exists(writingSystemFilePath))
			{
				File.Delete(writingSystemFilePath);
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
				var fi = new FileInfo(writingSystemFilePath);
				_lastFileStats[ws.Id] = Tuple.Create(fi.LastWriteTime, fi.Length);
			}
			catch (UnauthorizedAccessException)
			{
				// If we can't save the changes, too bad. Inability to save locally is typically caught
				// when we go to open the modify dialog. If we can't make the global store consistent,
				// as we well may not be able to in a client-server mode, too bad.
			}
			ws.AcceptChanges();
		}

		/// <summary>
		/// Writes the store to a persistable medium, if applicable.
		/// </summary>
		public override void Save()
		{
			using (_mutex.Lock())
			{
				UpdateDefinitions();

				//delete anything we're going to delete first, to prevent losing
				//a WS we want by having it deleted by an old WS we don't want
				//(but which has the same identifier)
				foreach (string id in AllWritingSystems.Where(ws => ws.MarkedForDeletion).Select(ws => ws.Id).ToArray())
					base.Remove(id);

				// make a copy and then go through that list - SaveDefinition calls Set which
				// may delete and then insert the same writing system - which would change WritingSystemDefinitions
				// and not be allowed in a foreach loop
				foreach (T ws in AllWritingSystems.Where(CanSet).ToArray())
					SaveDefinition(ws);

				_addedWritingSystems.Clear();
			}
		}

		/// <summary>
		/// Since the current implementation of Save does nothing, it's always possible.
		/// </summary>
		public override bool CanSave(T ws)
		{
			using (_mutex.Lock())
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
		}

		/// <summary>
		/// Gets the specified writing system if it exists.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="ws">The writing system.</param>
		/// <returns></returns>
		public override bool TryGet(string id, out T ws)
		{
			using (_mutex.Lock())
			{
				UpdateDefinitions();
				return WritingSystems.TryGetValue(id, out ws);
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
			Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
				_mutex.Dispose();
			IsDisposed = true;
		}

		private class WsStasher : IDisposable
		{
			private string _wsFile;
			private const string _localrepoupdate = ".localrepoupdate";

			public WsStasher(string wsFile)
			{
				_wsFile = wsFile;
				RobustFile.Copy(wsFile, wsFile + _localrepoupdate);
			}
			public void Dispose()
			{
				RobustFile.Move($"{_wsFile}{_localrepoupdate}", _wsFile);
			}
		}
	}
}
