using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Palaso.WritingSystems
{
	public class LdmlInFolderWritingSystemRepository : WritingSystemRepositoryBase
	{
		private const string _kExtension = ".ldml";
		private string _path;
		private IEnumerable<WritingSystemDefinition> _systemWritingSystemProvider;
		private readonly WritingSystemChangeLog _changeLog;

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		/// <param name="path"></param>
		public LdmlInFolderWritingSystemRepository(string path)
		{
			PathToWritingSystems = path;
			LoadAllDefinitions();
			_changeLog = new WritingSystemChangeLog(new WritingSystemChangeLogDataMapper(Path.Combine(PathToWritingSystems, "idchangelog.xml")));
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

		private string GetFilePathFromIdentifier(string identifier)
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
					throw new ApplicationException(
						String.Format("There was a problem loading one of your writing systems, found in file {0}. The exact error message was '{1}'.\r\nThe contents of the file are:\r\n{2}", filePath, e.Message, File.ReadAllText(filePath)), e);
				}
				if (wsFromFile.StoreID != wsFromFile.RFC5646)
				{
					bool badFileName = true;
					if (wsFromFile.StoreID != null && wsFromFile.StoreID.StartsWith("x", StringComparison.OrdinalIgnoreCase))
					{
						var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
						interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag(wsFromFile.StoreID);
						if (interpreter.RFC5646Tag.Equals(wsFromFile.RFC5646, StringComparison.OrdinalIgnoreCase))
						{
							badFileName = false;
						}
					}
					if(badFileName)
					{
						//Sometimes Flex produces bad filenames (particularly for x-Zxxx-x-audio) so we're letting this slide
						//throw new ApplicationException(
						//        String.Format(
						//            "The writing system file {0} seems to be named inconsistently. Please rename this file to reflect the contained Rfc5646Tag. This should have happened upon migration of the writing systems.",
						//            filePath));
					}
				}
				Set(wsFromFile);
			}
			LoadIdChangeMapFromExistingWritingSystems();
		}

		private WritingSystemDefinition GetWritingSystemFromLdml(string filePath)
		{
			WritingSystemDefinition ws = CreateNew();
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
					if (null == FindAlreadyLoadedWritingSystem(ws.RFC5646))
					{
						if (!HaveMatchingDefinitionInTrash(ws.RFC5646))
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

		private WritingSystemDefinition FindAlreadyLoadedWritingSystem(string rfc4646)
		{
			return AllWritingSystems.FirstOrDefault(ws => ws.RFC5646 == rfc4646);
		}

		public void SaveDefinition(WritingSystemDefinition ws)
		{
			Set(ws);
			string oldWritingSystemFilePath = GetFilePathFromIdentifier(ws.StoreID);
			MemoryStream oldData = null;
			if (!ws.Modified && File.Exists(oldWritingSystemFilePath))
			{
				return; // no need to save (better to preserve the modified date)
			}
			if (File.Exists(oldWritingSystemFilePath))
			{
				// load old data to preserve stuff in LDML that we don't use, but don't throw up an error if it fails
				try
				{
					oldData = new MemoryStream(File.ReadAllBytes(oldWritingSystemFilePath), false);
				}
				catch {}
				// What to do?  Assume that the UI has already checked for existing, asked, and allowed the overwrite.
				File.Delete(oldWritingSystemFilePath);
			}

			ws.StoreID = ws.Id;
			string newWritingSystemFilePath = GetFilePathFromIdentifier(ws.StoreID);
			LdmlDataMapper adaptor = CreateLdmlAdaptor();
			adaptor.Write(newWritingSystemFilePath, ws, oldData);

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

		override public void Remove(string identifier)
		{
			//we really need to get it in the trash, else, if was auto-provided,
			//it'll keep coming back!
			if (!File.Exists(GetFilePathFromIdentifier(identifier)) && Contains(identifier))
			{
				WritingSystemDefinition ws = Get(identifier);
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
			_changeLog.LogDelete(identifier);

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
			var allDefs = new List<WritingSystemDefinition>();
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

		public override void Set(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			base.Set(ws);
		}
	}
}