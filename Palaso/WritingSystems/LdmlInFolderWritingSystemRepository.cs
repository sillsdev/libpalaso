using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class LdmlInFolderWritingSystemRepository
	{
		private string _path;
		private bool _dontAddDefaultDefinitions;
		private List<WritingSystemDefinition> _writingSystems;
		private IWritingSystemProvider _systemWritingSystemProvider;

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
		}

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		/// <param name="path"></param>
		public LdmlInFolderWritingSystemRepository(string path)
		{
			PathToWritingSystems = path;
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
			}
		}

		public IList<WritingSystemDefinition> WritingSystemDefinitions
		{
			get
			{
				if (_writingSystems==null)
				{
					LoadUpDefinitions();
				}
				return _writingSystems;
			}
		}

		private void LoadUpDefinitions()
		{
			_writingSystems = new List<WritingSystemDefinition>();
			foreach (string defPath in Directory.GetFiles(_path, "*.ldml"))
			{
				try
				{
					string identifier = Path.GetFileNameWithoutExtension(defPath);

					_writingSystems.Add(LoadDefinition(identifier));
				}
				catch (Exception error)
				{
#if DEBUG
					throw new ApplicationException("problem loading " + defPath, error);
#endif
				}
			}


			 if (!_dontAddDefaultDefinitions )
			 {
				 AddActiveOSLanguages();
			 }

//
//            if (!_dontAddDefaultDefinitions && FindAlreadyLoadedWritingSystem("en") == null)
//            {
//                WritingSystemDefinition def = new WritingSystemDefinition();
//                LdmlAdaptor adaptor = new LdmlAdaptor();
//                adaptor.FillWithDefaults("en", def);
//                this.WritingSystemDefinitions.Add(def);
//            }
		}

		private bool HaveMatchingDefinitionInTrash(string identifier)
		{
			string path = PathToWritingSystemTrash();
			path = Path.Combine(path, LdmlAdaptor.GetFileNameFromIdentifier(identifier));
			return File.Exists(path);
		}

		private void AddActiveOSLanguages()
		{
			if (_systemWritingSystemProvider != null)
			{
				foreach (WritingSystemDefinition language in _systemWritingSystemProvider.ActiveOSLanguages)
				{
					if (null == FindAlreadyLoadedWritingSystem(language.RFC4646))
					{
						if (!HaveMatchingDefinitionInTrash(language.RFC4646))
						{
							WritingSystemDefinitions.Add(language);
						}
					}
				}
			}
		}

		/// <summary>
		/// useful for unit tests
		/// </summary>
		public bool DontAddDefaultDefinitions
		{
			get
			{
				return _dontAddDefaultDefinitions;
			}
			set
			{
				_dontAddDefaultDefinitions = value;
			}
		}

		/// <summary>
		/// Provides writing systems from a repository that comes, for example, with the OS
		/// </summary>
		public IWritingSystemProvider SystemWritingSystemProvider
		{
			get
			{
				return _systemWritingSystemProvider;
			}
			set
			{
				_systemWritingSystemProvider = value;
			}
		}

		private WritingSystemDefinition FindAlreadyLoadedWritingSystem(string rfc4646)
		{
			foreach (WritingSystemDefinition definition in _writingSystems)
			{
				if(definition.RFC4646 == rfc4646 )
					return definition;
			}
			return null;
		}

		public WritingSystemDefinition AddNewDefinition()
		{
			WritingSystemDefinition def = new WritingSystemDefinition();
			_writingSystems.Add(def);
			return def;
		}


		public void SaveDefinition(WritingSystemDefinition definition)
		{
			LdmlAdaptor adaptor = new LdmlAdaptor();
			adaptor.SaveToRepository(this, definition);
		}

		public WritingSystemDefinition LoadDefinition(string identifier)
		{
			LdmlAdaptor adaptor = new LdmlAdaptor();
			WritingSystemDefinition definition = new WritingSystemDefinition();
			adaptor.Load(this, identifier, definition);
			return definition;
		}

		public string GetFileName(WritingSystemDefinition definition)
		{
			LdmlAdaptor adaptor = new LdmlAdaptor();
			return adaptor.GetFileName(definition);
		}

		public string PathToWritingSystem(WritingSystemDefinition def)
		{
			return Path.Combine(PathToWritingSystems, GetFileName(def));
		}

		public void DeleteDefinition(WritingSystemDefinition def)
		{
			//we really need to get it in the trash, else, if was auto-provided,
			//it'll keep coming back!
			if (!File.Exists(PathToWritingSystem(def)))
			{
				SaveDefinition(def);
			}

			if (File.Exists(PathToWritingSystem(def)))
			{
				Directory.CreateDirectory(PathToWritingSystemTrash());
				string destination = Path.Combine(PathToWritingSystemTrash(), GetFileName(def));
				//clear out any old on already in the trash
				if (File.Exists(destination))
				{
					File.Delete(destination);
				}
				File.Move(PathToWritingSystem(def), destination);
			}
		}

		private string PathToWritingSystemTrash()
		{
			return Path.Combine(_path, "trash");
		}

		public WritingSystemDefinition MakeDuplicate(WritingSystemDefinition definition)
		{
			return definition.Clone();
		}


		public void SaveDefinitions()
		{
			//delete anything we're going to delete first, to prevent loosing
			//a WS we want by having it deleted by an old WS we don't want
			//(but which has the same identifier)
			foreach (WritingSystemDefinition definition in _writingSystems)
			{
				if (definition.MarkedForDeletion)
				{
					DeleteDefinition(definition);//nb: purposefully not removing from our list, for fear of leading to bugs in the UI. If we did this, we'd want to require the UI to reload the WS list after a save.
				}
			}

			foreach (WritingSystemDefinition definition in _writingSystems)
			{
				if (!definition.MarkedForDeletion)
				{
					 SaveDefinition(definition);
				}
			}
		}
	}
}