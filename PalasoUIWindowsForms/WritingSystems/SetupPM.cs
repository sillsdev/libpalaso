using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

using System.Windows.Forms;

using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public class SetupPM
	{
		private WritingSystemDefinition _currentWritingSystem;
		private int _currentIndex;
		private readonly IWritingSystemStore _writingSystemStore;
		private readonly List<WritingSystemDefinition> _writingSystemDefinitions;
		private readonly List<WritingSystemDefinition> _deletedWritingSystemDefinitions;

		public SetupPM(IWritingSystemStore writingSystemStore)
		{
			if (writingSystemStore == null)
			{
				throw new ArgumentNullException("writingSystemStore");
			}
			_writingSystemStore = writingSystemStore;
			_writingSystemDefinitions = new List<WritingSystemDefinition>(_writingSystemStore.WritingSystemDefinitions);
			_deletedWritingSystemDefinitions = new List<WritingSystemDefinition>();
			_currentIndex = -1;
		}

		#region Properties
		public IEnumerable<String> KeyboardNames
		{
			get
			{
				List<String> keyboards = new List<string>();
				keyboards.Add("(default)");

				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize(false))
				{
					foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in keymanLink.Keyboards)
					{
						keyboards.Add(keyboard.KbdName);
					}
				}

				foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
				{
					keyboards.Add(lang.LayoutName);
				}
				return keyboards;
			}
		}

		public IEnumerable<FontFamily> FontFamilies
		{
			get
			{
				foreach (FontFamily family in System.Drawing.FontFamily.Families)
				{
					yield return family;
				}
			}
		}

		private WritingSystemDefinition Current
		{
			get
			{
				return _currentWritingSystem;
			}
			set
			{
				if (_currentWritingSystem == value)
					return;
				SetCurrentAndOthersIfPossible();
				_currentWritingSystem = value;
				_currentIndex = value == null ? -1 : _writingSystemDefinitions.FindIndex(value.Equals);
				OnSelectionChanged();
			}
		}

		public int CurrentIndex
		{
			get
			{
				return _currentIndex;
			}
			set
			{
				if (value < -1 || value >= _writingSystemDefinitions.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				SetCurrentAndOthersIfPossible();
				_currentIndex = value;
				WritingSystemDefinition oldCurrentWS = _currentWritingSystem;
				_currentWritingSystem = value == -1 ? null : _writingSystemDefinitions[value];
				// we can't just check indexes as it doesn't work if the list has changed
				if (oldCurrentWS != _currentWritingSystem)
				{
					OnSelectionChanged();
				}
			}
		}

		public bool HasCurrentSelection
		{
			get
			{
				return Current != null;
			}
		}

		public string[] WritingSystemListColumns
		{
			get
			{
				return new string[] {"Writing System"};
			}
		}

		public IEnumerable<string[]> WritingSystemListItems
		{
			get
			{
				foreach (WritingSystemDefinition definition in _writingSystemDefinitions)
				{
					yield return new string[]{definition.DisplayLabel};
				}
			}
		}

		public bool[] WritingSystemListCanSave
		{
			get
			{
				bool[] canSave = new bool[_writingSystemDefinitions.Count];
				for (int i = 0; i < _writingSystemDefinitions.Count; i++)
				{
					canSave[i] = _writingSystemStore.CanSet(_writingSystemDefinitions[i]);
				}
				return canSave;
			}
		}

		public string[] WritingSystemListCurrentItem
		{
			get
			{
				return new string[] { Current.DisplayLabel } ;
			}
		}

		public int WritingSystemCount
		{
			get
			{
				return _writingSystemDefinitions.Count;
			}
		}

		public bool CanSaveCurrent
		{
			get
			{
				return _writingSystemStore.CanSet(Current);
			}
		}

		#endregion

		#region CurrentWritingSystemProperties

// Properties not (yet) mirrored:
//                Current.CurrentScriptOption; // ro - ScriptOption
//                Current.ScriptOptions; // ro - List<ScriptOption>
//                Current.MarkedForDeletion; // bool
//                Current.Modified; // bool
//                Current.StoreID; // string

		public string CurrentLanguageName
		{
			get { return Current.LanguageName; }
			set
			{
				if (Current.LanguageName != value)
				{
					Current.LanguageName = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentAbbreviation
		{
			get { return Current.Abbreviation; }
			set
			{
				if (Current.Abbreviation != value)
				{
					Current.Abbreviation = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public DateTime CurrentDateModified
		{
			get { return Current.DateModified; }
			set
			{
				if (Current.DateModified != value)
				{
					Current.DateModified = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentDefaultFontName
		{
			get { return Current.DefaultFontName; }
			set
			{
				if (Current.DefaultFontName != value)
				{
					Current.DefaultFontName = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentDisplayLabel
		{
			get { return Current.DisplayLabel; }
		}

		public string CurrentISO
		{
			get { return Current.ISO; }
			set
			{
				if (Current.ISO != value)
				{
					Current.ISO = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentKeyboard
		{
			get { return Current.Keyboard; }
			set
			{
				if (Current.Keyboard != value)
				{
					Current.Keyboard = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentNativeName
		{
			get { return Current.NativeName; }
			set
			{
				if (Current.NativeName != value)
				{
					Current.NativeName = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentRegion
		{
			get { return Current.Region; }
			set
			{
				if (Current.Region != value)
				{
					Current.Region = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentRFC4646
		{
			get { return Current.RFC4646; }
		}

		public bool CurrentRightToLeftScript
		{
			get { return Current.RightToLeftScript; }
			set
			{
				if (Current.RightToLeftScript != value)
				{
					Current.RightToLeftScript = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentScript
		{
			get { return Current.Script; }
			set
			{
				if (Current.Script != value)
				{
					Current.Script = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentVariant
		{
			get { return Current.Variant; }
			set
			{
				if (Current.Variant != value)
				{
					Current.Variant = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentVerboseDescription
		{
			get { return Current.VerboseDescription; }
		}

		public string CurrentVersionDescription
		{
			get { return Current.VersionDescription; }
			set
			{
				if (Current.VersionDescription != value)
				{
					Current.VersionDescription = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentVersionNumber
		{
			get { return Current.VersionNumber; }
			set
			{
				if (Current.VersionNumber != value)
				{
					Current.VersionNumber = value;
					OnCurrentItemUpdated();
				}
			}
		}

		#endregion


		#region Events

		public event EventHandler ItemAddedOrDeleted;
		public event EventHandler ListColumnsChanged;
		public event EventHandler SelectionChanged;
		public event EventHandler CurrentItemUpdated;

		#endregion

		public InputLanguage FindInputLanguage(string name)
		{
			if (InputLanguage.InstalledInputLanguages != null) // as is the case on Linux
			{
				foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
				{
					if (l.LayoutName == name)
					{
						return l;
					}
				}
			}
			return null;
		}

		public void ClearSelection()
		{
			Current = null;
		}

		public void DeleteCurrent()
		{
			if (!HasCurrentSelection)
			{
				throw new InvalidOperationException("Unable to delete current selection when there is no current selection.");
			}
			// new index will be next writing system or previous if this was the last in the list
			int newIndex = (CurrentIndex == WritingSystemCount - 1) ? CurrentIndex - 1 : CurrentIndex;
			string idToDelete = Current.StoreID;
			Current.MarkedForDeletion = true;
			_deletedWritingSystemDefinitions.Add(Current);
			_writingSystemDefinitions.RemoveAt(CurrentIndex);
			CurrentIndex = newIndex;
			// if it doesn't have a store ID, it shouldn't be in the store
			if (!string.IsNullOrEmpty(idToDelete))
			{
				_writingSystemStore.Remove(idToDelete);
			}
			OnAddOrDelete();
		}

		public void DuplicateCurrent()
		{
			if (!HasCurrentSelection)
			{
				throw new InvalidOperationException("Unable to duplicate current selection when there is no current selection.");
			}
			WritingSystemDefinition ws = _writingSystemStore.MakeDuplicate(Current);
			_writingSystemDefinitions.Insert(CurrentIndex+1, ws);
			OnAddOrDelete();
			Current = ws;
		}

		public WritingSystemDefinition AddNew()
		{
			WritingSystemDefinition ws = _writingSystemStore.CreateNew();
			ws.Abbreviation = "New";
			_writingSystemDefinitions.Add(ws);
			OnAddOrDelete();
			Current = ws;
			return ws;
		}

		private void OnAddOrDelete()
		{
			if (ItemAddedOrDeleted != null)
			{
				ItemAddedOrDeleted(this, new EventArgs());
			}
		}

		private void OnListColumnsChanged()
		{
			if (ListColumnsChanged != null)
			{
				ListColumnsChanged(this, new EventArgs());
			}
		}

		private void OnSelectionChanged()
		{
			if (SelectionChanged != null)
			{
				SelectionChanged(this, new EventArgs());
			}
		}

		private void OnCurrentItemUpdated()
		{
			if (CurrentItemUpdated != null)
			{
				CurrentItemUpdated(this, new EventArgs());
			}
		}

		public void RenameCurrent(string newShortName)
		{
			CurrentAbbreviation = newShortName;
		}

		public void Save()
		{
			_writingSystemStore.Save();
		}

		private void SetCurrentAndOthersIfPossible()
		{
			bool[] canSaveBefore = WritingSystemListCanSave;
			if (CanSaveCurrent)
			{
				_writingSystemStore.Set(Current);
			}
			bool done = false;
			while (!done)
			{
				done = true;
				for (int i = 0; i < _writingSystemDefinitions.Count; i++)
				{
					if (!canSaveBefore[i] && _writingSystemStore.CanSet(_writingSystemDefinitions[i]))
					{
						_writingSystemStore.Set(_writingSystemDefinitions[i]);
						canSaveBefore[i] = true;
						done = false;
					}
				}
			}
		}
	}
}
