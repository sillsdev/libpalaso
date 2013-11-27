using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.WritingSystems.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems
{
	public class LdmlInFolderWritingSystemRepository : WritingSystemRepositoryBase
	{
		///<summary>
		/// Returns an instance of an ldml in folder writing system reposistory.
		///</summary>
		///<param name="basePath">base location of the global writing system repository</param>
		///<param name="migrationHandler">Callback if during the initialization any writing system id's are changed</param>
		///<param name="loadProblemHandler">Callback if during the initialization any writing systems cannot be loaded</param>
		public static LdmlInFolderWritingSystemRepository Initialize(
			string basePath,
			LdmlVersion0MigrationStrategy.MigrationHandler migrationHandler,
			WritingSystemLoadProblemHandler loadProblemHandler
		)
		{
			return Initialize(basePath, migrationHandler, loadProblemHandler, WritingSystemCompatibility.Strict);
		}

		///<summary>
		/// Returns an instance of an ldml in folder writing system reposistory.
		///</summary>
		///<param name="basePath">base location of the global writing system repository</param>
		///<param name="migrationHandler">Callback if during the initialization any writing system id's are changed</param>
		///<param name="loadProblemHandler">Callback if during the initialization any writing systems cannot be loaded</param>
		///<param name="roundtripFlex70PrivateUse"></param>
		public static LdmlInFolderWritingSystemRepository Initialize(
			string basePath,
			LdmlVersion0MigrationStrategy.MigrationHandler migrationHandler,
			WritingSystemLoadProblemHandler loadProblemHandler,
			WritingSystemCompatibility compatibilityMode
		)
		{
			var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(basePath, migrationHandler, compatibilityMode);
			migrator.Migrate();

			var instance = new LdmlInFolderWritingSystemRepository(basePath, compatibilityMode);
			instance.LoadAllDefinitions();

			// Call the loadProblemHandler with both migration problems and load problems
			var loadProblems = new List<WritingSystemRepositoryProblem>();
			loadProblems.AddRange(migrator.MigrationProblems);
			loadProblems.AddRange(instance.LoadProblems);
			if (loadProblems.Count > 0)
			{
				loadProblemHandler(loadProblems);
			}

			return instance;
		}

		private const string _kExtension = ".ldml";
		private string _path;
		private IEnumerable<WritingSystemDefinition> _systemWritingSystemProvider;
		private readonly WritingSystemChangeLog _changeLog;
		private readonly IList<WritingSystemRepositoryProblem> _loadProblems = new List<WritingSystemRepositoryProblem>();

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		/// <param name="basePath"></param>
		protected internal LdmlInFolderWritingSystemRepository(string basePath) :
			this(basePath, WritingSystemCompatibility.Strict)
		{
		}

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		/// <param name="basePath"></param>
		/// <param name="compatibilityMode"></param>
		protected internal LdmlInFolderWritingSystemRepository(string basePath, WritingSystemCompatibility compatibilityMode) :
			base(compatibilityMode)
		{
			PathToWritingSystems = basePath;
			_changeLog = new WritingSystemChangeLog(new WritingSystemChangeLogDataMapper(Path.Combine(PathToWritingSystems, "idchangelog.xml")));
		}

		public IList<WritingSystemRepositoryProblem> LoadProblems
		{
			get { return _loadProblems; }
		}

		public string PathToWritingSystems
		{
			get
			{
				return _path;
			}
			set
			{
				_path = value;
				if (!Directory.Exists(_path))
				{
					string parent = Directory.GetParent(_path).FullName;
					if (!Directory.Exists(parent))
					{
						throw new ApplicationException(
							"The writing system repository cannot be created because its parent folder, " + parent +
							", does not exist.");
					}
					Directory.CreateDirectory(_path);
				}
				LoadAllDefinitions();
			}
		}

		///<summary>
		/// Returns the full path to the underlying store for this writing system.
		///</summary>
		///<param name="identifier"></param>
		///<returns>FilePath</returns>
		public string GetFilePathFromIdentifier(string identifier)
		{
			return Path.Combine(PathToWritingSystems, GetFileNameFromIdentifier(identifier));
		}

		protected static string GetFileNameFromIdentifier(string identifier)
		{
			return identifier + _kExtension;
		}

		protected void LoadAllDefinitions()
		{
			Clear();
			foreach (string filePath in Directory.GetFiles(_path, "*.ldml"))
			{
				WritingSystemDefinition wsFromFile;
				try
				{
					wsFromFile = GetWritingSystemFromLdml(filePath);
				}
				catch(Exception e)
				{
					// Add the exception to our list of problems and continue loading
					var problem = new WritingSystemRepositoryProblem
						{
							Consequence = WritingSystemRepositoryProblem.ConsequenceType.WSWillNotBeAvailable,
							Exception = e,
							FilePath = filePath
						};
					_loadProblems.Add(problem);
					continue;
				}

				if (String.Compare(wsFromFile.StoreID, wsFromFile.Bcp47Tag, true) != 0)
				{
					bool badFileName = true;
					if (wsFromFile.StoreID != null && wsFromFile.StoreID.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
					{
						var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
						interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag(wsFromFile.StoreID);
						if (interpreter.RFC5646Tag.Equals(wsFromFile.Bcp47Tag, StringComparison.OrdinalIgnoreCase))
						{
							badFileName = false;
						}
					}
					if(badFileName)
					{// Add the exception to our list of problems and continue loading
						var problem = new WritingSystemRepositoryProblem
						{
							Consequence = WritingSystemRepositoryProblem.ConsequenceType.WSWillNotBeAvailable,
							Exception = new ApplicationException(
								String.Format(
									"The writing system file {0} seems to be named inconsistently. It contains the Rfc5646 tag: '{1}'. The name should have been made consistent with its content upon migration of the writing systems.",
									filePath, wsFromFile.Bcp47Tag)),
							FilePath = filePath
						};
						_loadProblems.Add(problem);
					}
				}
				try
				{
					Set(wsFromFile);
				}
				catch(Exception e){
					// Add the exception to our list of problems and continue loading
					var problem = new WritingSystemRepositoryProblem
					{
						Consequence = WritingSystemRepositoryProblem.ConsequenceType.WSWillNotBeAvailable,
						Exception = e,
						FilePath = filePath
					};
					_loadProblems.Add(problem);
				}
			}
			LoadIdChangeMapFromExistingWritingSystems();
		}

		private WritingSystemDefinition GetWritingSystemFromLdml(string filePath)
		{
			var ws = (WritingSystemDefinition)CreateNew();
			LdmlDataMapper adaptor = CreateLdmlAdaptor();
			if (File.Exists(filePath))
			{
				adaptor.Read(filePath, ws);
				ws.StoreID = Path.GetFileNameWithoutExtension(filePath);
			}
			return ws;
		}

		private bool HaveMatchingDefinitionInTrash(string identifier)
		{
			string path = PathToWritingSystemTrash();
			path = Path.Combine(path, GetFileNameFromIdentifier(identifier));
			return File.Exists(path);
		}

		private void AddActiveOSLanguages()
		{
				foreach (WritingSystemDefinition ws in _systemWritingSystemProvider)
				{
					if (null == FindAlreadyLoadedWritingSystem(ws.Bcp47Tag))
					{
						if (!HaveMatchingDefinitionInTrash(ws.Bcp47Tag))
						{
							Set(ws);
						}
					}
				}
		}

//        /// <summary>
//        /// useful for unit tests
//        /// </summary>
//        public bool DontAddDefaultDefinitions { get; set; }
//
		/// <summary>
		/// Provides writing systems from a repository that comes, for example, with the OS
		/// </summary>
		public IEnumerable<WritingSystemDefinition> SystemWritingSystemProvider {
			get{ return _systemWritingSystemProvider;}
			set
			{
				if(_systemWritingSystemProvider == value){ return;}
				_systemWritingSystemProvider = value;
				AddActiveOSLanguages();
			}
		}

		private IWritingSystemDefinition FindAlreadyLoadedWritingSystem(string bcp47Tag)
		{
			return AllWritingSystems.FirstOrDefault(ws => ws.Bcp47Tag == bcp47Tag);
		}

		internal void SaveDefinition(IWritingSystemDefinition ws)
		{
			SaveDefinition((WritingSystemDefinition)ws);
		}

		public void SaveDefinition(WritingSystemDefinition ws)
		{
			Set(ws);
			string writingSystemFilePath = GetFilePathFromIdentifier(ws.StoreID);
			MemoryStream oldData = null;
			if (!ws.Modified && File.Exists(writingSystemFilePath))
			{
				return; // no need to save (better to preserve the modified date)
			}
			if (File.Exists(writingSystemFilePath))
			{
				// load old data to preserve stuff in LDML that we don't use, but don't throw up an error if it fails
				try
				{
					oldData = new MemoryStream(File.ReadAllBytes(writingSystemFilePath), false);
				}
				catch {}
				// What to do?  Assume that the UI has already checked for existing, asked, and allowed the overwrite.
				File.Delete(writingSystemFilePath); //!!! Should this be move to trash?
			}
			LdmlDataMapper adaptor = CreateLdmlAdaptor();
			adaptor.Write(writingSystemFilePath, ws, oldData);

			ws.Modified = false;

			if (_idChangeMap.Any(p => p.Value == ws.StoreID))
			{
				// log this id change to the writing system change log
				var pair = _idChangeMap.First(p => p.Value == ws.StoreID);
				_changeLog.LogChange(pair.Key, pair.Value);
			} else
			{
				// log this addition
				_changeLog.LogAdd(ws.StoreID);
			}
		}

		public override void Conflate(string wsToConflate, string wsToConflateWith)
		{
			//conflation involves deleting the old writing system. That deletion should not appear int he log. which is what the "_conflating" is used for
			base.Conflate(wsToConflate, wsToConflateWith);
			_changeLog.LogConflate(wsToConflate, wsToConflateWith);
		}

		override public void Remove(string identifier)
		{
			//we really need to get it in the trash, else, if was auto-provided,
			//it'll keep coming back!
			if (!File.Exists(GetFilePathFromIdentifier(identifier)) && Contains(identifier))
			{
				var ws = Get(identifier);
				SaveDefinition(ws);
			}

			if (File.Exists(GetFilePathFromIdentifier(identifier)))
			{
				Directory.CreateDirectory(PathToWritingSystemTrash());
				string destination = Path.Combine(PathToWritingSystemTrash(), GetFileNameFromIdentifier(identifier));
				//clear out any old on already in the trash
				if (File.Exists(destination))
				{
					File.Delete(destination);
				}
				File.Move(GetFilePathFromIdentifier(identifier), destination);
			}
			base.Remove(identifier);
			if (!Conflating)
			{
				_changeLog.LogDelete(identifier);
			}

		}

		private string PathToWritingSystemTrash()
		{
			return Path.Combine(_path, "trash");
		}

		override public void Save()
		{
			//delete anything we're going to delete first, to prevent losing
			//a WS we want by having it deleted by an old WS we don't want
			//(but which has the same identifier)
			var idsToRemove = new List<string>();
			foreach (var ws in AllWritingSystems)
			{
				if (ws.MarkedForDeletion)
				{
					idsToRemove.Add(ws.StoreID);//nb: purposefully not removing from our list, for fear of leading to bugs in the UI. If we did this, we'd want to require the UI to reload the WS list after a save.
				}
			}
			foreach (string id in idsToRemove)
			{
				Remove(id);
			}

			// make a copy and then go through that list - SaveDefinition calls Set which
			// may delete and then insert the same writing system - which would change WritingSystemDefinitions
			// and not be allowed in a foreach loop
			var allDefs = new List<IWritingSystemDefinition>();
			foreach (var ws in AllWritingSystems)
			{
				if (CanSet(ws))
				{
					allDefs.Add(ws);
				}
			}
			foreach (var ws in allDefs)
			{
				SaveDefinition(ws);
				if (!ws.Modified)
				{
					OnChangeNotifySharedStore(ws);
				}
			}


			LoadIdChangeMapFromExistingWritingSystems();
		}

		public override void Set(IWritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			var oldStoreId = ws.StoreID;
			base.Set(ws);
			//Renaming the file here is a bit ugly as the content has not yet been updated. Thus there
			//may be a mismatch between the filename and the contained rfc5646 tag. Doing it here however
			//helps us avoid having to deal with situations where a writing system id is changed to be
			//identical with the old id of another writing sytsem. This could otherwise lead to dataloss.
			//The inconsistency is resolved on Save()
			if (oldStoreId != ws.StoreID && File.Exists(GetFilePathFromIdentifier(oldStoreId)))
			{
				File.Move(GetFilePathFromIdentifier(oldStoreId), GetFilePathFromIdentifier(ws.StoreID));
			}
		}

		public override bool WritingSystemIdHasChanged(string id)
		{
			return _changeLog.HasChangeFor(id);
		}

		public override string WritingSystemIdHasChangedTo(string id)
		{
			return AllWritingSystems.Any(ws => ws.Id.Equals(id)) ? id : _changeLog.GetChangeFor(id);
		}
	}
}