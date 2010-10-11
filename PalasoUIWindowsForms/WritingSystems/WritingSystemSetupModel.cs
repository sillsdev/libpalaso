using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using System.Windows.Forms;
using Palaso.Code;
using Palaso.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	/// <summary>
	/// This is the presentation model for the UI for setting up Writing Systems using either
	/// a writing system store or a single writing system.
	/// In order to use any of the provided UI elements within your own forms, you need to
	/// instantiate a WritingSystemSetupModel object and bind the UI elements to that object.
	/// WritingSystemSetupDialog provides its own WritingSystemSetupModel object and can be used by itself.
	/// </summary>
	/// <example><code>
	/// WritingSystemSetupModel model = new WritingSystemSetupModel(new LdmlInFolderWritingSystemStore();
	/// WritingSystemSetupView panel = new WritingSystemSetupView();
	/// panel.BindToModel(model);
	/// </code></example>
	public class WritingSystemSetupModel
	{
		private readonly bool _usingStore;
		private WritingSystemDefinition _currentWritingSystem;
		private int _currentIndex;
		private readonly IWritingSystemStore _writingSystemStore;
		private readonly List<WritingSystemDefinition> _writingSystemDefinitions;
		private readonly List<WritingSystemDefinition> _deletedWritingSystemDefinitions;


		/// <summary>
		/// Use this to set the appropriate kinds of writing systems according to your
		/// application.  For example, is the user of your app likely to want voice? ipa? dialects?
		/// </summary>
		public WritingSystemSuggestor WritingSystemSuggestor { get; private set; }

		/// <summary>
		/// UI layer can set this to something which shows a dialog to get the basic info
		/// </summary>
		public Func<WritingSystemDefinition> MethodToShowUiToBootstrapNewDefinition;

		/// <summary>
		/// Creates the presentation model object based off of a writing system store of some sort.
		/// </summary>
		public WritingSystemSetupModel(IWritingSystemStore writingSystemStore)
		{
			if (writingSystemStore == null)
			{
				throw new ArgumentNullException("writingSystemStore");
			}
			WritingSystemSuggestor = new WritingSystemSuggestor();

			_writingSystemStore = writingSystemStore;
			_writingSystemDefinitions = new List<WritingSystemDefinition>(_writingSystemStore.WritingSystemDefinitions);
			_deletedWritingSystemDefinitions = new List<WritingSystemDefinition>();
			_currentIndex = -1;
			_usingStore = true;
		}

		/// <summary>
		/// Creates the presentation model object based off of a single writing system definition.
		/// This is the easiest form to use if you only want part of the UI elements or only operate on
		/// one WritingSystemDefiniion
		/// </summary>
		public WritingSystemSetupModel(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			WritingSystemSuggestor = new WritingSystemSuggestor();

			_currentWritingSystem = ws;
			_currentIndex = 0;
			_writingSystemStore = null;
			_writingSystemDefinitions = new List<WritingSystemDefinition>(1);
			WritingSystemDefinitions.Add(ws);
			_deletedWritingSystemDefinitions = null;
			_usingStore = false;
		}

		#region Properties
		/// <summary>
		/// Provides a list of all possible installed keyboards.
		/// </summary>
		public static IEnumerable<String> KeyboardNames
		{
			get
			{
				List<String> keyboards = new List<string>();
				keyboards.Add("(default)");
				foreach (KeyboardDescriptor keyboard in
					KeyboardController.GetAvailableKeyboards(Engines.All))
				{
					keyboards.Add(keyboard.KeyboardName);
				}
				return keyboards;
			}
		}

		/// <summary>
		/// Provides a list of all available Font family names on the system.
		/// </summary>
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

		/// <summary>
		/// For internal use only.  To access information about the writing system,
		/// you should use the CurrentXXX Properties which mirror the public properties
		/// available on WritingSystemDefinition.  This is needed to ensure that the UI
		/// stays up to date with any changes to the underlying WritingSystemDefinition.
		/// </summary>
		internal WritingSystemDefinition CurrentDefinition
		{
			get
			{
				return _currentWritingSystem;
			}
			set
			{
				if (!_usingStore)
				{
					throw new InvalidOperationException("Unable to change selection without writing system store.");
				}
				if (_currentWritingSystem == value)
					return;
				_currentWritingSystem = value;
				_currentIndex = value == null ? -1 : WritingSystemDefinitions.FindIndex(value.Equals);
				OnSelectionChanged();
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="rfc4646"></param>
		/// <returns>false if the code wasn't found</returns>
		public virtual bool SetCurrentIndexFromRfc46464(string rfc4646)
		{
			var index = WritingSystemDefinitions.FindIndex(d => d.RFC5646 == rfc4646);
			if(index<0)
			{
				return false;
			}
			CurrentIndex = index;
			return true;
		}

		public virtual bool SetCurrentDefinition(WritingSystemDefinition definition)
		{
			var index = WritingSystemDefinitions.FindIndex(d => d == definition);
			if (index < 0)
			{
				return false;
			}
			CurrentIndex = index;
			return true;
		}

		/// <summary>
		/// The index of the currently selected WritingSystemDefinition from the list of
		/// available definitions.  This will be -1 if there is no selection.
		/// </summary>
		public int CurrentIndex
		{
			get
			{
				return _currentIndex;
			}
			set
			{
				if (!_usingStore)
				{
					throw new InvalidOperationException("Unable to change selection without writing system store.");
				}
				if (value < -1 || value >= WritingSystemDefinitions.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				_currentIndex = value;
				WritingSystemDefinition oldCurrentWS = _currentWritingSystem;
				_currentWritingSystem = value == -1 ? null : WritingSystemDefinitions[value];
				// we can't just check indexes as it doesn't work if the list has changed
				if (oldCurrentWS != _currentWritingSystem)
				{
					OnSelectionChanged();
				}
			}
		}

//        public WritingSystemDefinition CurrentDefinition
//        {
//            get
//            {
//                if(CurrentIndex>-1)
//                    return _writingSystemDefinitions[CurrentIndex];
//                return null;
//            }
//        }

		/// <summary>
		/// Returns true if there is a WritingSystemDefinition currently selected
		/// for display and editing.
		/// </summary>
		public bool HasCurrentSelection
		{
			get
			{
				return CurrentDefinition != null;
			}
		}

		/// <summary>
		/// Columns to include in a list of WritingSystemDefinitions.
		/// </summary>
		public string[] WritingSystemListColumns
		{
			get
			{
				return new string[] {"Writing System"};
			}
		}

		/// <summary>
		/// Returns a list of available WritingSystemDefinitions.  The arrays returned will have
		/// elements corresponding with the columns returned by WritingSystemListColumns.
		/// </summary>
		public IEnumerable<string[]> WritingSystemListItems
		{
			get
			{
				foreach (WritingSystemDefinition definition in WritingSystemDefinitions)
				{
					yield return new string[]{definition.DisplayLabel};
				}
			}
		}

		/// <summary>
		/// Gives a true/false flag for all available WritingSystemDefinitions on whether or
		/// not the underlying store will be able to save them.  This could be false due to
		/// two definitions having the same identifying information.
		/// </summary>
		public bool[] WritingSystemListCanSave
		{
			get
			{
				if (!_usingStore)
				{
					return new bool[] {false};
				}
				Dictionary<string, int> idList = new Dictionary<string, int>();
				bool[] canSave = new bool[WritingSystemDefinitions.Count];
				for (int i = 0; i < WritingSystemDefinitions.Count; i++)
				{
					string id = _writingSystemStore.GetNewStoreIDWhenSet(WritingSystemDefinitions[i]);
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

		/// <summary>
		/// The columns to display in a list for only the currently selected writing system.
		/// </summary>
		public string[] WritingSystemListCurrentItem
		{
			get
			{
				return new string[] { CurrentDefinition.DisplayLabel } ;
			}
		}

		/// <summary>
		/// Returns the total number of writing systems available.
		/// </summary>
		public int WritingSystemCount
		{
			get
			{
				return WritingSystemDefinitions.Count;
			}
		}

		/// <summary>
		/// Whether or not the underlying store will be able to save the currently
		/// selected writing system.
		/// </summary>
		public bool CanSaveCurrent
		{
			get
			{
				if (!_usingStore)
				{
					return false;
				}
				return CurrentDefinition == null ? false : WritingSystemListCanSave[CurrentIndex];
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
				// create a list of languages we have to disallow to prevent a cycle
				// in the sort options
				List<string> prohibitedList = new List<string>();
				if (CurrentDefinition != null && !string.IsNullOrEmpty(CurrentDefinition.RFC5646))
				{
					// don't allow the current language to be picked
					prohibitedList.Add(CurrentDefinition.RFC5646);
				}
				for (int i = 0; i < WritingSystemDefinitions.Count; i++)
				{
					WritingSystemDefinition ws = WritingSystemDefinitions[i];
					// don't allow if it references another language on our prohibited list and this one
					// isn't already on the prohibited list
					if (ws.SortUsing == WritingSystemDefinition.SortRulesType.OtherLanguage
						&& !string.IsNullOrEmpty(ws.RFC5646) && prohibitedList.Contains(ws.SortRules)
						&& !prohibitedList.Contains(ws.RFC5646))
					{
						prohibitedList.Add(ws.RFC5646);
						// Restart the scan through all the writing systems every time we add a prohibited one.
						// This ensuers that we catch all possible cycles.
						i = -1;
					}
				}
				bool returnedOne = false;
				//NOTE: not currently listing other writing system definitions as it doesn't work yet.
				// add languages from other writing system definitions to the top of the list
				// but don't include empty definitions or ones that would cause a cycle
				//foreach (WritingSystemDefinition ws in _writingSystemDefinitions)
				//{
				//    if (string.IsNullOrEmpty(ws.RFC5646) || prohibitedList.Contains(ws.RFC5646))
				//    {
				//        continue;
				//    }
				//    returnedOne = true;
				//    yield return new KeyValuePair<string, string>(ws.RFC5646, ws.DisplayLabel);
				//}
				if (returnedOne)
				{
					// include separator only if we've included our own languages at the top of the list
					yield return new KeyValuePair<string, string>(string.Empty, "-----");
				}
				// populate the rest of the list with all languages from the OS
				foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
				{
					yield return
						new KeyValuePair<string, string>(cultureInfo.IetfLanguageTag, cultureInfo.DisplayName);
				}
			}
		}

		/// <summary>
		/// True if the model has an underlying writing system store,
		/// false if it is operating on only a single WritingSystemDefinition.
		/// </summary>
		public bool UsingWritingSystemStore
		{
			get { return _usingStore; }
		}

		#endregion

		#region CurrentWritingSystemProperties

// Properties not (yet) mirrored:
//                Current.ScriptOption; // ro - ScriptOption
//                Current.ScriptOptions; // ro - List<ScriptOption>
//                Current.MarkedForDeletion; // bool
//                Current.Modified; // bool
//                Current.StoreID; // string

		public string CurrentLanguageName
		{
			get { return CurrentDefinition.LanguageName ?? string.Empty; }
			set
			{
				if (CurrentDefinition.LanguageName != value)
				{
					CurrentDefinition.LanguageName = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentAbbreviation
		{
			get { return CurrentDefinition.Abbreviation ?? string.Empty; }
			set
			{
				if (CurrentDefinition.Abbreviation != value)
				{
					CurrentDefinition.Abbreviation = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public DateTime CurrentDateModified
		{
			get { return CurrentDefinition.DateModified; }
			set
			{
				if (CurrentDefinition.DateModified != value)
				{
					CurrentDefinition.DateModified = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentDefaultFontName
		{
			get { return CurrentDefinition.DefaultFontName ?? string.Empty; }
			set
			{
				if (CurrentDefinition.DefaultFontName != value)
				{
					CurrentDefinition.DefaultFontName = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public float CurrentDefaultFontSize
		{
			get { return CurrentDefinition.DefaultFontSize; }
			set
			{
				if (CurrentDefinition.DefaultFontSize != value)
				{
					CurrentDefinition.DefaultFontSize = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentDisplayLabel
		{
			get { return CurrentDefinition.DisplayLabel ?? string.Empty; }
		}

		public string CurrentISO
		{
			get { return CurrentDefinition == null ? string.Empty : (CurrentDefinition.ISO ?? string.Empty); }
			set
			{
				if (CurrentDefinition.ISO != value)
				{
					CurrentDefinition.ISO = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentKeyboard
		{
			get
			{
				if(CurrentDefinition==null)
					return string.Empty;
				return string.IsNullOrEmpty(CurrentDefinition.Keyboard) ? "(default)" : CurrentDefinition.Keyboard;
			}
			set
			{
				if (value == "(default)")
				{
					value = string.Empty;
				}
				if (CurrentDefinition.Keyboard != value)
				{
					CurrentDefinition.Keyboard = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentNativeName
		{
			get { return CurrentDefinition == null ? string.Empty : (CurrentDefinition.NativeName ?? string.Empty); }
			set
			{
				if (CurrentDefinition.NativeName != value)
				{
					CurrentDefinition.NativeName = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentRegion
		{
			get { return  CurrentDefinition==null? string.Empty : (CurrentDefinition.Region ?? string.Empty); }
			set
			{
				if (CurrentDefinition.Region != value)
				{
					CurrentDefinition.Region = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentRFC4646
		{
			get
			{
				if (CurrentDefinition == null)
				{
					return string.Empty;
				}
				else
				{
					return CurrentDefinition.RFC5646 ?? string.Empty;
				}
			}
//            set
//            {
//                Palaso.Code.Guard.AgainstNull(CurrentDefinition, "CurrentDefinition");
//                CurrentDefinition.RFC5646 = value;
//            }
		}

		public bool CurrentRightToLeftScript
		{
			get { return CurrentDefinition.RightToLeftScript; }
			set
			{
				if (CurrentDefinition.RightToLeftScript != value)
				{
					CurrentDefinition.RightToLeftScript = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentScriptCode
		{
			get { return CurrentDefinition.Script ?? string.Empty; }
			set
			{
				if (CurrentDefinition.Script != value)
				{
					CurrentDefinition.Script = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public bool CurrentIsVoice
		{
			get { return CurrentDefinition==null ? false : _currentWritingSystem.IsVoice; }
			set
			{
				Guard.AgainstNull(CurrentDefinition,"CurrentDefinition");
				if (CurrentDefinition.IsVoice != value)
				{
					CurrentDefinition.IsVoice = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentVariant
		{
			get { return CurrentDefinition.Variant ?? string.Empty; }
			set
			{
				if (CurrentDefinition.Variant != value)
				{
					CurrentDefinition.Variant = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentVerboseDescription
		{
			get { return CurrentDefinition!=null ? CurrentDefinition.VerboseDescription : string.Empty; }
		}

		public string CurrentVersionDescription
		{
			get { return CurrentDefinition.VersionDescription ?? string.Empty; }
			set
			{
				if (CurrentDefinition.VersionDescription != value)
				{
					CurrentDefinition.VersionDescription = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentVersionNumber
		{
			get { return CurrentDefinition.VersionNumber ?? string.Empty; }
			set
			{
				if (CurrentDefinition.VersionNumber != value)
				{
					CurrentDefinition.VersionNumber = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentSpellCheckingId
		{
			get { return CurrentDefinition.SpellCheckingId ?? string.Empty; }
			set
			{
				if (CurrentDefinition.SpellCheckingId != value)
				{
					CurrentDefinition.SpellCheckingId = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentSortUsing
		{
			get { return CurrentDefinition.SortUsing.ToString(); }
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					var valueAsSortUsing = (WritingSystemDefinition.SortRulesType)Enum.Parse(typeof (WritingSystemDefinition.SortRulesType), value);
					if (valueAsSortUsing != CurrentDefinition.SortUsing)
					{
						CurrentDefinition.SortUsing = valueAsSortUsing;
						OnCurrentItemUpdated();
					}
				}
			}
		}

		public string CurrentSortRules
		{
			get { return CurrentDefinition.SortRules ?? string.Empty; }
			set
			{
				if (CurrentDefinition.SortRules != value)
				{
					CurrentDefinition.SortRules = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public virtual List<WritingSystemDefinition> WritingSystemDefinitions
		{
			get { return _writingSystemDefinitions; }
		}

		public enum SelectionsForSpecialCombo
		{
			None=0,
			Ipa=1,
			Voice=2,
			ScriptRegionVariant=3,
			Custom=4
		}
		public SelectionsForSpecialCombo SelectionForSpecialCombo
		{
			get
			{
				//TODO: this is really too simplistic


				if (_currentWritingSystem.IsVoice)
				{
					return SelectionsForSpecialCombo.Voice;
				}
//                if (!string.IsNullOrEmpty(_currentWritingSystem.Variant))
//                {
//                    return SelectionsForSpecialCombo.Custom;
//                }
				if (!string.IsNullOrEmpty(_currentWritingSystem.Script))
				{
					if(WritingSystemDefinition.ScriptOptions.Any(s=>s.Code==_currentWritingSystem.Script))
					{
						return SelectionsForSpecialCombo.ScriptRegionVariant;
					}
					return SelectionsForSpecialCombo.Custom;
				}

				if (!string.IsNullOrEmpty(_currentWritingSystem.Region))
				{
					return SelectionsForSpecialCombo.ScriptRegionVariant;
				}
				if (!string.IsNullOrEmpty(_currentWritingSystem.Variant))
				{
					if(!_currentWritingSystem.Variant.StartsWith("fonipa"))
						return SelectionsForSpecialCombo.ScriptRegionVariant;
				}
				if (_currentWritingSystem.IpaStatus != IpaStatusChoices.NotIpa)
				{
					return SelectionsForSpecialCombo.Ipa;
				}
				return SelectionsForSpecialCombo.None;
			}
		}

		public IpaStatusChoices CurrentIpaStatus
		{
			 get
			{
				if(_currentWritingSystem==null  )
					return IpaStatusChoices.NotIpa;
				return _currentWritingSystem.IpaStatus;
			}
			set
			{
				Guard.AgainstNull(_currentWritingSystem, "CurrentWritingSystem");
				if (_currentWritingSystem.IpaStatus != value)
				{
					_currentWritingSystem.IpaStatus = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public ScriptOption CurrentScriptOption
		{
			get
			{
				if(_currentWritingSystem==null  )
					return null;

				return _currentWritingSystem.ScriptOption;
				//return WritingSystemDefinition.ScriptOptions.FirstOrDefault(o => o.Code == CurrentScriptCode);
			}
			set
			{
				if(value==null)
				{
					CurrentScriptCode=string.Empty;
				}
				else
				{
					CurrentScriptCode = value.Code;
				}
			}
		}

		#endregion


		#region Events

		/// <summary>
		/// Fires when a WritingSystemDefinition is added or deleted.
		/// </summary>
		public event EventHandler ItemAddedOrDeleted;

		/// <summary>
		/// Fires whenever the columns in a list of writing systems change.
		/// </summary>
		public event EventHandler ListColumnsChanged;

		/// <summary>
		/// Fired when the selection changes to a different WritingSystemDefinition.
		/// When the currently selected item is deleted, this event will fire first to
		/// select another WritingSystemDefinition, and then the ItemAddedOrDeleted event will fire.
		/// When an item is added, ItemAddedOrDeleted will fire first, and then this event
		/// will fire to indicated the selection of the new item.
		/// </summary>
		public event EventHandler SelectionChanged;

		/// <summary>
		/// This event is fired whenever the currently selected WritingSystemDefinition
		/// is changed.
		/// </summary>
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
			CurrentDefinition = null;
		}

		/// <summary>
		/// Deletes the currently selected writing system.
		/// </summary>
		public void DeleteCurrent()
		{
			if (!_usingStore)
			{
				throw new InvalidOperationException("Unable to delete current selection when there is no writing system store.");
			}
			if (!HasCurrentSelection)
			{
				throw new InvalidOperationException("Unable to delete current selection when there is no current selection.");
			}
			// new index will be next writing system or previous if this was the last in the list
			int newIndex = (CurrentIndex == WritingSystemCount - 1) ? CurrentIndex - 1 : CurrentIndex;
			string idToDelete = CurrentDefinition.StoreID;
			CurrentDefinition.MarkedForDeletion = true;
			_deletedWritingSystemDefinitions.Add(CurrentDefinition);
			WritingSystemDefinitions.RemoveAt(CurrentIndex);
			CurrentIndex = newIndex;
			// if it doesn't have a store ID, it shouldn't be in the store
			if (!string.IsNullOrEmpty(idToDelete))
			{
				_writingSystemStore.Remove(idToDelete);
			}
			OnAddOrDelete();
		}

		/// <summary>
		/// Makes a copy of the currently selected writing system and selects the new copy.
		/// </summary>
		public void DuplicateCurrent()
		{
			if (!_usingStore)
			{
				throw new InvalidOperationException("Unable to duplicate current selection when there is no writing system store.");
			}
			if (!HasCurrentSelection)
			{
				throw new InvalidOperationException("Unable to duplicate current selection when there is no current selection.");
			}
			WritingSystemDefinition ws = _writingSystemStore.MakeDuplicate(CurrentDefinition);
			WritingSystemDefinitions.Insert(CurrentIndex+1, ws);
			OnAddOrDelete();
			CurrentDefinition = ws;
		}

		/// <summary>
		/// Creates a new writing system and selects it.
		/// </summary>
		/// <returns></returns>
		public virtual void AddNew()
		{
			if (!_usingStore)
			{
				throw new InvalidOperationException("Unable to add new writing system definition when there is no store.");
			}
			WritingSystemDefinition ws=null;
			if (MethodToShowUiToBootstrapNewDefinition == null)
			{
				ws = _writingSystemStore.CreateNew();
				ws.Abbreviation = "New";
			}
			else
			{
				ws = MethodToShowUiToBootstrapNewDefinition();
			}
			if(ws==null)//cancelled
				return;
			WritingSystemDefinitions.Add(ws);
			OnAddOrDelete();
			CurrentDefinition = ws;
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

		/// <summary>
		/// Saves all writing systems in the store, if possible.
		/// </summary>
		public void Save()
		{
			if (!_usingStore)
			{
				throw new InvalidOperationException("Unable to save when there is no writing system store.");
			}
			SetAllPossibleAndRemoveOthers();
			_writingSystemStore.Save();
		}

		/// <summary>
		/// Exports the current writing system to a file.
		/// </summary>
		/// <param name="filePath"></param>
		public void ExportCurrentWritingSystemAsFile(string filePath)
		{
			if (!HasCurrentSelection) {
				throw new InvalidOperationException ("Unable to export current selection when there is no current selection.");
			}
			LdmlAdaptor adaptor = new LdmlAdaptor ();
			adaptor.Write (filePath, _currentWritingSystem, null);
		}

		private void SetAllPossibleAndRemoveOthers()
		{
			// Set everything that we can, then change stuff until we can set it, then change it back and try again.
			// The reason to do this is to solve problems with cycles that could prevent saving.
			// Example:
			// ws1 has ID "a" and ws2 has ID "b"
			// Set ws1 to ID "b" and ws2 to ID "a"
			// The store will not allow you to set either of these because of the conflict
			// NOTE: It is not a good idea to remove and then add all writing systems, even though it would
			// NOTE: accomplish the same goal as any information in the LDML file not used by palaso would be lost.
			Dictionary<WritingSystemDefinition, string> cantSet = new Dictionary<WritingSystemDefinition, string>();
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				if (_writingSystemStore.CanSet(ws))
				{
					_writingSystemStore.Set(ws);
				}
				else
				{
					cantSet.Add(ws, ws.ISO);
				}
			}
			foreach (KeyValuePair<WritingSystemDefinition, string> kvp in cantSet)
			{
				while (!_writingSystemStore.CanSet(kvp.Key))
				{
					kvp.Key.ISO += "X";
				}
				_writingSystemStore.Set(kvp.Key);
			}
			foreach (KeyValuePair<WritingSystemDefinition, string> kvp in cantSet)
			{
				kvp.Key.ISO = kvp.Value;
				if (_writingSystemStore.CanSet(kvp.Key))
				{
					_writingSystemStore.Set(kvp.Key);
				}
				else
				{
					_writingSystemStore.Remove(kvp.Key.StoreID);
				}
			}
		}

		/// <summary>
		/// Activates the custom keyboard for the currently selected writing system.
		/// </summary>
		public void ActivateCurrentKeyboard()
		{
			if (CurrentDefinition == null)
				return;
			if (!string.IsNullOrEmpty(CurrentDefinition.Keyboard))
			{
				KeyboardController.ActivateKeyboard(CurrentDefinition.Keyboard);
			}
		}

		/// <summary>
		/// Performs a sort on the given string using the sort rules defined
		/// in the current writing system.
		/// </summary>
		/// <param name="testString">String to sort, separated by newlines</param>
		/// <returns>Sorted string</returns>
		public string TestSort(string testString)
		{
			if (CurrentDefinition == null)
				return testString;
			if (testString == null)
				return testString;
			if (CurrentDefinition.Collator == null)
				return testString;
			List<SortKey> stringList = new List<SortKey>();
			foreach (string str in testString.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
			{
				stringList.Add(CurrentDefinition.Collator.GetSortKey(str));
			}
			stringList.Sort(SortKey.Compare);
			string result = string.Empty;
			foreach (SortKey sk in stringList)
			{
				result += sk.OriginalString + "\r\n";
			}
			return result.TrimEnd(new char[] {'\r', '\n'});
		}

		/// <summary>
		/// Validates the current custom sort rules.
		/// </summary>
		/// <param name="message">Error message, if any.  This will only be set if false is returned.</param>
		/// <returns>True if rules are valid, otherwise false.</returns>
		public bool ValidateCurrentSortRules(out string message)
		{
			if (CurrentDefinition == null)
			{
				message = null;
				return true;
			}
			return CurrentDefinition.ValidateCollationRules(out message);
		}

		/// <summary>
		/// Imports the given file into the writing system store.
		/// </summary>
		/// <param name="fileName">Full path of file to import</param>
		public void ImportFile(string fileName)
		{
			if (!_usingStore)
			{
				throw new InvalidOperationException("Unable to import file when not using writing system store.");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (!System.IO.File.Exists(fileName))
			{
				throw new ArgumentException("File does not exist.", "fileName");
			}
			LdmlAdaptor _adaptor = new LdmlAdaptor();
			WritingSystemDefinition ws = _writingSystemStore.CreateNew();
			_adaptor.Read(fileName, ws);
			WritingSystemDefinitions.Add(ws);
			OnAddOrDelete();
			CurrentDefinition = ws;
		}

//        /// <summary>
//        /// Cause the model to reload, if you've made changes
//        /// (e.g., using a dialog to edit another copy of the mode)
//        /// </summary>
//        public void Refresh()
//        {
//
//        }
		public virtual void AddPredefinedDefinition(WritingSystemDefinition definition)
		{
			if (!_usingStore)
			{
				throw new InvalidOperationException("Unable to add new writing system definition when there is no store.");
			}
			WritingSystemDefinitions.Add(definition);
			OnAddOrDelete();
			CurrentDefinition = definition;
		}
	}
}
