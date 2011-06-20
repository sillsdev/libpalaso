using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class LdmlInFolderWritingSystemRepository : WritingSystemRepositoryBase
	{
		private const string _kExtension = ".ldml";
		private string _path;
		private IEnumerable<WritingSystemDefinition> _systemWritingSystemProvider;
		private WritingSystemChangeLog _changeLog;

		public static int LatestVersion
		{
			get{ return 1;}
		}

		/// <summary>
		/// Use the default repository
		/// </summary>
		public LdmlInFolderWritingSystemRepository()
		{
			string p =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
			Directory.CreateDirectory(p);
			p = Path.Combine(p, "WritingSystemRepository");
			Directory.CreateDirectory(p);
			PathToWritingSystems = p;
			LoadAllDefinitions();
			_changeLog = new WritingSystemChangeLog(new WritingSystemChangeLogDataMapper(Path.Combine(PathToWritingSystems, "idchangelog.xml")));
		}

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

		private static string GetFileNameFromIdentifier(string identifier)
		{
			return identifier + _kExtension;
		}

		private void LoadAllDefinitions()
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
						throw new ApplicationException(
								String.Format(
									"The writing system file {0} seems to be named inconsistently. Please rename this file to reflect the contained Rfc5646Tag. This should have happened upon migration of the writing systems.",
									filePath));
					}
				}
				Set(wsFromFile);
			}
			LoadIdChangeMapFromExistingWritingSystems();
		}

		private WritingSystemDefinition GetWritingSystemFromLdml(string filePath)
		{
			WritingSystemDefinition ws = CreateNew();
			LdmlAdaptor adaptor = CreateLdmlAdaptor();
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
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				if(ws.RFC5646 == rfc4646 )
					return ws;
			}
			return null;
		}

		public void SaveDefinition(WritingSystemDefinition ws)
		{
			string incomingFileName = GetFileNameFromIdentifier(ws.StoreID);
			Set(ws);
			string writingSystemFilePath = GetFilePathFromIdentifier(ws.StoreID);
			MemoryStream oldData = null;
			if (!ws.Modified && File.Exists(writingSystemFilePath))
			{
				return; // no need to save (better to preserve the modified date)
			}
			if (!String.IsNullOrEmpty(incomingFileName))
			{
				string previousFilePath = Path.Combine(PathToWritingSystems, incomingFileName);
				if (File.Exists(previousFilePath))
				{
					// load old data to preserve stuff in LDML that we don't use, but don't throw up an error if it fails
					try
					{
						oldData = new MemoryStream(File.ReadAllBytes(previousFilePath), false);
					}
					catch {}
					// What to do?  Assume that the UI has already checked for existing, asked, and allowed the overwrite.
					File.Delete(previousFilePath); //!!! Should this be move to trash?
				}
			}
			LdmlAdaptor adaptor = CreateLdmlAdaptor();
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

		override public void Remove(string identifier)
		{
			//we really need to get it in the trash, else, if was auto-provided,
			//it'll keep coming back!
			if (!File.Exists(GetFilePathFromIdentifier(identifier)) && Exists(identifier))
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
			List<string> idsToRemove = new List<string>();
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
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
			List<WritingSystemDefinition> allDefs = new List<WritingSystemDefinition>();
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				if (CanSet(ws))
				{
					allDefs.Add(ws);
				}
			}
			foreach (WritingSystemDefinition ws in allDefs)
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
			// Renaming files on Set keeps the file names consistent with StoreID which is changed in the base.
			// This allows us to avoid creating duplicate files and to preserve LDML data which is not used
			// in palaso.
			string oldFileName = GetFileNameFromIdentifier(ws.StoreID);
			string oldFilePath = string.IsNullOrEmpty(oldFileName) ? string.Empty : Path.Combine(PathToWritingSystems, oldFileName);
			string oldID = ws.StoreID;
			base.Set(ws);
			if (oldID == ws.StoreID || string.IsNullOrEmpty(oldFileName) || !File.Exists(oldFilePath))
			{
				return;
			}
			string writingSystemFilePath = GetFilePathFromIdentifier(ws.StoreID);
			File.Move(oldFilePath, writingSystemFilePath);
		}
	}
}