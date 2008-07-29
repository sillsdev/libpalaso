using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;

using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;
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
		public static IEnumerable<String> KeyboardNames
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

		public static IEnumerable<FontFamily> FontFamilies
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
				Dictionary<string, int> idList = new Dictionary<string, int>();
				bool[] canSave = new bool[_writingSystemDefinitions.Count];
				for (int i = 0; i < _writingSystemDefinitions.Count; i++)
				{
					string id = _writingSystemStore.GetNewStoreIDWhenSet(_writingSystemDefinitions[i]);
					if (idList.ContainsKey(id))
					{
						canSave[i] = false;
						canSave[idList[id]] = false;
					}
					else
					{
						canSave[i] = true;
						idList.Add(id, i);
					}
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
				return Current == null ? false : WritingSystemListCanSave[CurrentIndex];
			}
		}

		/// <summary>
		/// Returns a list of possible sort options as (option, description) pairs
		/// </summary>
		public static IEnumerable<KeyValuePair<string, string>> SortUsingOptions
		{
			get
			{
				foreach (Enum customSortRulesType in Enum.GetValues(typeof(WritingSystemDefinition.SortRulesType)))
				{
					FieldInfo fi =
							customSortRulesType.GetType().GetField(customSortRulesType.ToString());

					DescriptionAttribute[] descriptions =
						(DescriptionAttribute[])
						fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
					string description;
					if (descriptions.Length == 0)
					{
						description = customSortRulesType.ToString();
					}
					else
					{
						description = descriptions[0].Description;
					}
					yield return new KeyValuePair<string, string>(customSortRulesType.ToString(), description);
				}
			}
		}

		/// <summary>
		/// Gets a list of (value, description) pairs for language options for sorting
		/// </summary>
		public IEnumerable<KeyValuePair<string, string>> SortLanguageOptions
		{
			get
			{
				foreach (WritingSystemDefinition ws in _writingSystemDefinitions)
				{
					if (string.IsNullOrEmpty(ws.RFC4646))
					{
						continue;
					}
					yield return new KeyValuePair<string, string>(ws.RFC4646, ws.DisplayLabel);
				}
				yield return new KeyValuePair<string, string>(null, "-----");
				foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
				{
					yield return new KeyValuePair<string, string>(cultureInfo.IetfLanguageTag, cultureInfo.DisplayName);
				}
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
			get { return Current.LanguageName ?? string.Empty; }
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
			get { return Current.Abbreviation ?? string.Empty; }
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
			get { return Current.DefaultFontName ?? string.Empty; }
			set
			{
				if (Current.DefaultFontName != value)
				{
					Current.DefaultFontName = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public float CurrentDefaultFontSize
		{
			get { return Current.DefaultFontSize; }
			set
			{
				if (Current.DefaultFontSize != value)
				{
					Current.DefaultFontSize = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentDisplayLabel
		{
			get { return Current.DisplayLabel ?? string.Empty; }
		}

		public string CurrentISO
		{
			get { return Current.ISO ?? string.Empty; }
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
			get { return string.IsNullOrEmpty(Current.Keyboard) ? "(default)" : Current.Keyboard; }
			set
			{
				if (value == "(default)")
				{
					value = string.Empty;
				}
				if (Current.Keyboard != value)
				{
					Current.Keyboard = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentNativeName
		{
			get { return Current.NativeName ?? string.Empty; }
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
			get { return Current.Region ?? string.Empty; }
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
			get { return Current.RFC4646 ?? string.Empty; }
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
			get { return Current.Script ?? string.Empty; }
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
			get { return Current.Variant ?? string.Empty; }
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
			get { return Current.VerboseDescription ?? string.Empty; }
		}

		public string CurrentVersionDescription
		{
			get { return Current.VersionDescription ?? string.Empty; }
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
			get { return Current.VersionNumber ?? string.Empty; }
			set
			{
				if (Current.VersionNumber != value)
				{
					Current.VersionNumber = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentSpellCheckingId
		{
			get { return Current.SpellCheckingId ?? string.Empty; }
			set
			{
				if (Current.SpellCheckingId != value)
				{
					Current.SpellCheckingId = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentAutoReplaceRules
		{
			get { return Current.AutoReplaceRules ?? string.Empty; }
			set
			{
				if (Current.AutoReplaceRules != value)
				{
					Current.AutoReplaceRules = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentSortUsing
		{
			get { return Current.SortUsing ?? string.Empty; }
			set
			{
				if (Current.SortUsing != value)
				{
					Current.SortUsing = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentSortRules
		{
			get { return Current.SortRules ?? string.Empty; }
			set
			{
				if (Current.SortRules != value)
				{
					Current.SortRules = value;
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
			SetAllPossibleAndRemoveOthers();
			_writingSystemStore.Save();
		}

		private void SetAllPossibleAndRemoveOthers()
		{
			// Remove everything from the store, then set everything that we can
			// The reason to do this is to solve problems with cycles that could prevent saving.
			// Example:
			// ws1 has ID "a" and ws2 has ID "b"
			// Set ws1 to ID "b" and ws2 to ID "a"
			// The store will not allow you to set either of these because of the conflict
			// but if we remove them first and then set them, it will work.
			foreach (WritingSystemDefinition ws in _writingSystemDefinitions)
			{
				if (!string.IsNullOrEmpty(ws.StoreID))
				{
					_writingSystemStore.Remove(ws.StoreID);
					ws.StoreID = string.Empty;
				}
			}
			foreach (WritingSystemDefinition ws in _writingSystemDefinitions)
			{
				if (_writingSystemStore.CanSet(ws))
				{
					_writingSystemStore.Set(ws);
				}
			}
		}

		public void ActivateCurrentKeyboard()
		{
			if (Current == null)
				return;
			if (!string.IsNullOrEmpty(Current.Keyboard))
			{
				KeyboardController.ActivateKeyboard(Current.Keyboard);
			}
		}
	}
}
