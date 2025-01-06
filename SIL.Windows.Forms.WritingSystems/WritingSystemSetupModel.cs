using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Enchant;
using SIL.Code;
using SIL.Extensions;
using SIL.i18n;
using SIL.Keyboarding;
using SIL.Reporting;
using SIL.Windows.Forms.Keyboarding;
using SIL.Windows.Forms.WritingSystems.WSTree;
using SIL.WritingSystems;
using static SIL.WritingSystems.IetfLanguageTag;
using static SIL.WritingSystems.WellKnownSubtags;

namespace SIL.Windows.Forms.WritingSystems
{
	/// <summary>
	/// This is the presentation model for the UI for setting up Writing Systems using either
	/// a writing system store or a single writing system.
	/// In order to use any of the provided UI elements within your own forms, you need to
	/// instantiate a WritingSystemSetupModel object and bind the UI elements to that object.
	/// WritingSystemSetupDialog provides its own WritingSystemSetupModel object and can be used by itself.
	/// </summary>
	/// <example><code>
	/// WritingSystemSetupModel model = new WritingSystemSetupModel(new LdmlInFolderWritingSystemRepository();
	/// WritingSystemSetupView panel = new WritingSystemSetupView();
	/// panel.BindToModel(model);
	/// </code></example>
	public class WritingSystemSetupModel
	{
		/// <summary>
		/// Collation rules types
		/// </summary>
		private enum CollationRulesType
		{
			/// <summary>
			/// Default Unicode ordering rules (actually CustomIcu without any rules)
			/// </summary>
			[Description("Default Ordering")]
			DefaultOrdering,
			/// <summary>
			/// Custom Simple (Shoebox/Toolbox) style rules
			/// </summary>
			[Description("Custom Simple (Shoebox style) rules")]
			CustomSimple,
			/// <summary>
			/// Custom ICU rules
			/// </summary>
			[Description("Custom ICU rules")]
			CustomIcu,
			/// <summary>
			/// Use the sort rules from another language. When this is set, the SortRules are interpreted as a cultureId for the language to sort like.
			/// </summary>
			[Description("Same as another language")]
			OtherLanguage
		}

		private readonly bool _usingRepository;
		private WritingSystemDefinition _currentWritingSystem;
		private int _currentIndex;
		private readonly IWritingSystemRepository _writingSystemRepository;
		private readonly IWritingSystemFactory _writingSystemFactory;
		private readonly List<WritingSystemDefinition> _writingSystemDefinitions;
		private readonly List<WritingSystemDefinition> _deletedWritingSystemDefinitions;
		private CollationRulesType _currentCollationRulesType;

		internal string DefaultCustomSimpleSortRules = "A a-B b-C c-D d-E e-F f-G g-H h-I i-J j-K k-L l-M m-N n-O o-P p-Q q-R r-S s-T t-U u-V v-W w-X x-Y y-Z z".Replace("-", "\r\n");

		/// <summary>
		/// Set this to limit the Special combo to only one selection
		/// </summary>
		public SelectionsForSpecialCombo LockedSpecialCombo { get; private set; }

		/// <summary>
		/// Set this to false to keep ScriptRegionVariantView from showing its scary warning label
		/// </summary>
		public bool DisplayScriptRegionVariantWarningLabel { get; private set; }

		/// <summary>
		/// Use this to set the appropriate kinds of writing systems according to your
		/// application.  For example, is the user of your app likely to want voice? IPA? dialects?
		/// </summary>
		public WritingSystemSuggestor WritingSystemSuggestor { get; private set; }

		public IWritingSystemRepository WritingSystems { get { return _writingSystemRepository; } }

		public IWritingSystemFactory WritingSystemFactory
		{
			get { return _writingSystemFactory; }
		}

		internal bool IsSpecialComboLocked
		{
			get { return LockedSpecialCombo != SelectionsForSpecialCombo.None; }
		}

		/// <summary>
		/// UI layer can set this to something which shows a dialog to get the basic info
		/// </summary>
		public Func<WritingSystemDefinition> MethodToShowUiToBootstrapNewDefinition;

		private List<SpellCheckInfo> _spellCheckerItems = new List<SpellCheckInfo>();

		/// <summary>
		/// Creates the presentation model object based off of a writing system store of some sort.
		/// </summary>
		public WritingSystemSetupModel(IWritingSystemRepository writingSystemRepository)
		{
			if (writingSystemRepository == null)
			{
				throw new ArgumentNullException("writingSystemRepository");
			}
			_writingSystemFactory = writingSystemRepository.WritingSystemFactory;
			WritingSystemSuggestor = new WritingSystemSuggestor(_writingSystemFactory);

			_writingSystemRepository = writingSystemRepository;
			_writingSystemDefinitions = new List<WritingSystemDefinition>(_writingSystemRepository.AllWritingSystems);
			_deletedWritingSystemDefinitions = new List<WritingSystemDefinition>();
			_currentIndex = -1;
			_usingRepository = true;
		}

		/// <summary>
		/// Creates the presentation model object based off of a single writing system definition.
		/// This is the easiest form to use if you only want part of the UI elements or only operate on
		/// one WritingSystemDefiniion
		/// </summary>
		/// <param name="ws">A writing system definition</param>
		/// <param name="lockedSpecialComboSelection">Optional parameter if we want to limit the Special combo to one value</param>
		/// <param name="displayScriptRegionVariantWarningLabel">Optional parameter used to hide unwanted label</param>
		public WritingSystemSetupModel(WritingSystemDefinition ws,
			SelectionsForSpecialCombo lockedSpecialComboSelection = SelectionsForSpecialCombo.None,
			bool displayScriptRegionVariantWarningLabel = true)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			_writingSystemFactory = new SldrWritingSystemFactory();
			WritingSystemSuggestor = new WritingSystemSuggestor(_writingSystemFactory);

			_currentWritingSystem = ws;
			_currentIndex = 0;
			_writingSystemRepository = null;
			_writingSystemDefinitions = new List<WritingSystemDefinition> {ws};
			_deletedWritingSystemDefinitions = null;
			_usingRepository = false;
			LockedSpecialCombo = lockedSpecialComboSelection;
			DisplayScriptRegionVariantWarningLabel = displayScriptRegionVariantWarningLabel;
			SetCurrentCollationRulesTypeFromDefinition();
		}

		public IEnumerable<IKeyboardDefinition> KnownKeyboards
		{
			get
			{
				if (_currentWritingSystem != null)
				{
					// If there aren't any known, possibly this WS is being migrated from a legacy writing system.
					// If so, we'd like to show the keyboard indicated by the legacy fields as a known keyboard.
					// It's tempting to actually modify the KnownKeyboards list and put it in, but that would be a
					// very dubious thing for a getter to do, and also, would put it there permanently, even
					// if the user does not confirm it.
					if (_currentWritingSystem != null && _currentWritingSystem.KnownKeyboards.Count == 0)
					{
						IKeyboardDefinition legacyKeyboard = _currentWritingSystem.LegacyKeyboard;
						if (legacyKeyboard != null)
							yield return legacyKeyboard;
					}
					foreach (IKeyboardDefinition knownKeyboard in _currentWritingSystem.KnownKeyboards)
						yield return knownKeyboard;
				}
			}
		}

		public IEnumerable<IKeyboardDefinition> OtherAvailableKeyboards
		{
			get
			{
				List<IKeyboardDefinition> result = PossibleKeyboardsToChoose.ToList();
				if (_currentWritingSystem != null)
				{
					foreach (IKeyboardDefinition knownKeyboard in _currentWritingSystem.KnownKeyboards)
						result.Remove(knownKeyboard);
					if (_currentWritingSystem.KnownKeyboards.Count == 0)
					{
						// If there's a legacy keyboard and no known keyboards, we move the legacy one to 'known';
						// so don't show it here.
						IKeyboardDefinition legacyKeyboard = _currentWritingSystem.LegacyKeyboard;
						if (legacyKeyboard != null)
							result.Remove(legacyKeyboard);
					}
				}
				return result;
			}
		}


		#region Properties
		/// <summary>
		/// Provides a list of all possible installed keyboards, or 'default' (which does nothing)
		/// if there are none.
		/// This is typically used to populate a list of keyboards that could be chosen.
		/// </summary>
		public static IEnumerable<IKeyboardDefinition> PossibleKeyboardsToChoose
		{
			get
			{
				// Returns default keyboard if there are no keyboards
				if (!Keyboard.Controller.AvailableKeyboards.Any())
					yield return KeyboardController.NullKeyboard;

				foreach (IKeyboardDefinition keyboard in Keyboard.Controller.AvailableKeyboards)
					yield return keyboard;
			}
		}

		/// <summary>
		/// Provides a list of all available Font family names on the system.
		/// </summary>
		public static IEnumerable<FontFamily> FontFamilies
		{
			get
			{
				foreach (FontFamily family in FontFamily.Families)
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
				if (!_usingRepository)
				{
					throw new InvalidOperationException("Unable to change selection without writing system store.");
				}
				if (_currentWritingSystem == value)
					return;
				_currentWritingSystem = value;
				_currentIndex = value == null ? -1 : WritingSystemDefinitions.FindIndex(value.Equals);
				SetCurrentCollationRulesTypeFromDefinition();
				OnSelectionChanged();
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="languageTag"></param>
		/// <returns>false if the code wasn't found</returns>
		public virtual bool SetCurrentIndexFromLanguageTag(string languageTag)
		{
			var index = WritingSystemDefinitions.FindIndex(d => d.LanguageTag == languageTag);
			if (index < 0)
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
				if (!_usingRepository)
				{
					throw new InvalidOperationException("Unable to change selection without writing system store.");
				}
				if (value < -1 || value >= WritingSystemDefinitions.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				_currentIndex = value;
				var oldCurrentWS = _currentWritingSystem;
				_currentWritingSystem = value == -1 ? null : WritingSystemDefinitions[value];
				// we can't just check indexes as it doesn't work if the list has changed
				if (oldCurrentWS != _currentWritingSystem)
				{
					SetCurrentCollationRulesTypeFromDefinition();
					OnSelectionChanged();
				}
			}
		}

		private void SetCurrentCollationRulesTypeFromDefinition()
		{
			if (CurrentDefinition == null || CurrentDefinition.DefaultCollation == null)
			{
				_currentCollationRulesType = CollationRulesType.DefaultOrdering;
			}
			else if (CurrentDefinition.DefaultCollation is SimpleRulesCollationDefinition)
			{
				_currentCollationRulesType = CollationRulesType.CustomSimple;
			}
			else if (CurrentDefinition.DefaultCollation is IcuRulesCollationDefinition)
			{
				var icuCollation = (IcuRulesCollationDefinition) CurrentDefinition.DefaultCollation;
				if (!string.IsNullOrEmpty(icuCollation.IcuRules) || icuCollation.Imports.Count > 0)
					_currentCollationRulesType = CollationRulesType.CustomIcu;
				else
					_currentCollationRulesType = CollationRulesType.DefaultOrdering;
			}
			else if (CurrentDefinition.DefaultCollation is SystemCollationDefinition)
			{
				_currentCollationRulesType = CollationRulesType.OtherLanguage;
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
				return new[] {"Writing System"};
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
					yield return new[]{definition.DisplayLabel};
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
				if (!_usingRepository)
				{
					return new bool[] {false};
				}
				Dictionary<string, int> idList = new Dictionary<string, int>();
				bool[] canSave = new bool[WritingSystemDefinitions.Count];
				for (int i = 0; i < WritingSystemDefinitions.Count; i++)
				{
					string id = WritingSystemDefinitions[i].LanguageTag;
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
				if (!_usingRepository)
				{
					return false;
				}
				return CurrentDefinition != null && WritingSystemListCanSave[CurrentIndex];
			}
		}

		/// <summary>
		/// Returns a list of possible sort options as (option, description) pairs
		/// </summary>
		public static IEnumerable<KeyValuePair<string, string>> SortUsingOptions
		{
			get
			{
				foreach (Enum customSortRulesType in Enum.GetValues(typeof(CollationRulesType)))
				{
					FieldInfo fi = customSortRulesType.GetType().GetField(customSortRulesType.ToString());

					var descriptions =
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
				var prohibitedList = new List<string>();
				if (CurrentDefinition != null && !string.IsNullOrEmpty(CurrentDefinition.LanguageTag))
				{
					// don't allow the current language to be picked
					prohibitedList.Add(CurrentDefinition.LanguageTag);
				}
				for (int i = 0; i < WritingSystemDefinitions.Count; i++)
				{
					WritingSystemDefinition ws = WritingSystemDefinitions[i];
					var systemCollation = ws.DefaultCollation as SystemCollationDefinition;
					// don't allow if it references another language on our prohibited list and this one
					// isn't already on the prohibited list
					if (systemCollation != null && !string.IsNullOrEmpty(ws.LanguageTag)
						&& prohibitedList.Contains(systemCollation.LanguageTag)
						&& !prohibitedList.Contains(ws.LanguageTag))
					{
						prohibitedList.Add(ws.LanguageTag);
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
				foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(info => info.Name))
				{
					if (prohibitedList.Contains(cultureInfo.Name, StringComparison.OrdinalIgnoreCase))
						continue;

					// Do not add languages that are not installed.
					if (!SystemCollator.ValidateLanguageTag(cultureInfo.Name, out _))
						continue;

					yield return new KeyValuePair<string, string>(cultureInfo.Name, cultureInfo.DisplayName);
				}
			}
		}

		/// <summary>
		/// True if the model has an underlying writing system store,
		/// false if it is operating on only a single WritingSystemDefinition.
		/// </summary>
		public bool UsingWritingSystemRepository
		{
			get { return _usingRepository; }
		}

		#endregion

		#region CurrentWritingSystemProperties

// Properties not (yet) mirrored:
//                Current.Iso15924Script; // ro - Iso15924Script
//                Current.ScriptOptions; // ro - List<Iso15924Script>
//                Current.MarkedForDeletion; // bool
//                Current.Modified; // bool
//                Current.StoreID; // string

		public string CurrentLanguageName
		{
			get { return CurrentDefinition.Language.Name; }
			set
			{
				string valueBeforeSet = CurrentDefinition.Language.Name;
				CurrentDefinition.Language = new LanguageSubtag(CurrentDefinition.Language, value);
				if (CurrentDefinition.Language.Name != valueBeforeSet)
				{
					OnCurrentItemUpdated();
				}
				//if (CurrentDefinition.LanguageName != value)
				//{
				//    var languageNameBeforeSet = CurrentDefinition.LanguageName;
				//    CurrentDefinition.LanguageName = value;
				//    //When we set the LanguageName on a writingSystem to Empty and call get it returns the
				//    //ianasubtag registry default. So if the result
				//    if (languageNameBeforeSet == CurrentDefinition.LanguageName)
				//    {
				//        return;
				//    }
				//    OnCurrentItemUpdated();
				//}
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
			get
			{
				return (CurrentDefinition == null || CurrentDefinition.DefaultFont == null)
					? string.Empty
					: CurrentDefinition.DefaultFont.Name;
			}
			set
			{
				if (CurrentDefinition != null && !CurrentDefaultFontName.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					FontDefinition font;
					if (!CurrentDefinition.Fonts.TryGet(value, out font))
						font = new FontDefinition(value);
					CurrentDefinition.DefaultFont = font;
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

		public string CurrentIso
		{
			get { return CurrentDefinition == null ? string.Empty : ((string) CurrentDefinition.Language ?? string.Empty); }
			set
			{
				if (CurrentDefinition.Language != value)
				{
					CurrentDefinition.Language = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public IKeyboardDefinition CurrentKeyboard
		{
			get
			{
				return CurrentDefinition?.LocalKeyboard;
			}
			set
			{
				if (CurrentDefinition == null)
					return; // Hopefully can't happen
				if (CurrentDefinition.LocalKeyboard != null && CurrentDefinition.LocalKeyboard.Equals(value))
					return;
				CurrentDefinition.LocalKeyboard = value == KeyboardController.NullKeyboard ? null : value;
				OnCurrentItemUpdated();
			}
		}

		public string CurrentRegion
		{
			get { return  CurrentDefinition == null ? string.Empty : ((string) CurrentDefinition.Region ?? string.Empty); }
			set
			{
				if (CurrentDefinition.Region != value)
				{
					if (String.IsNullOrEmpty(CurrentDefinition.Language))
					{
						CurrentDefinition.Language = UnlistedLanguage;
					}
					CurrentDefinition.Region = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentLanguageTag
		{
			get
			{
				if (CurrentDefinition == null)
					return string.Empty;
				return CurrentDefinition.LanguageTag ?? string.Empty;
			}
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
					if(String.IsNullOrEmpty(CurrentDefinition.Language))
					{
						CurrentDefinition.Language = UnlistedLanguage;
					}
					CurrentDefinition.Script = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public bool CurrentIsVoice
		{
			get { return CurrentDefinition != null && _currentWritingSystem.IsVoice; }
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
			get { return GetVariantCodes(CurrentDefinition.Variants) ?? string.Empty; }
			set
			{
				if (GetVariantCodes(CurrentDefinition.Variants) != value)
				{
					string fixedVariant = value.ToValidVariantString();
					if (string.IsNullOrEmpty(CurrentDefinition.Language) && !fixedVariant.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
						CurrentDefinition.Language = UnlistedLanguage;

					IEnumerable<VariantSubtag> variantSubtags;
					if (TryGetVariantSubtags(fixedVariant, out variantSubtags))
					{
						VariantSubtag[] originalVariantSubtags = CurrentDefinition.Variants.ToArray();
						CurrentDefinition.Variants.ReplaceAll(variantSubtags);
						string message;
						if (CurrentDefinition.ValidateLanguageTag(out message))
						{
							OnCurrentItemUpdated();
						}
						else
						{
							CurrentDefinition.Variants.ReplaceAll(originalVariantSubtags);
							ErrorReport.NotifyUserOfProblem(message);
						}
					}
					else
					{
						ErrorReport.NotifyUserOfProblem("Variant tags are invalid.");
					}
				}
			}
		}

		public string CurrentVerboseDescription
		{
			get
			{
				return CurrentDefinition != null ? VerboseDescription(CurrentDefinition) : String.Empty;
			}
		}

		virtual public string VerboseDescription(WritingSystemDefinition writingSystem)
		{
			var summary = new StringBuilder();
			summary.AppendFormat(" {0}", writingSystem.Language.Name);
			if (writingSystem.Region != null)
				summary.AppendFormat(" in {0}", writingSystem.Region.Code);
			if (writingSystem.Script != null && !IsScriptImplied(writingSystem.LanguageTag))
				summary.AppendFormat(" written in {0} script", CurrentIso15924Script.ShortName);

			summary.AppendFormat(". ({0})", writingSystem.LanguageTag);
			return summary.ToString().Trim();
		}

		public ScriptSubtag CurrentIso15924Script
		{
			get
			{
				if (_currentWritingSystem == null)
					return null;

				return _currentWritingSystem.Script ?? "Latn";
			}
			set
			{
				if (value == null)
				{
					CurrentScriptCode = string.Empty;
				}
				else
				{
					CurrentScriptCode = value.Code;
				}
			}
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

		public NumberingSystemDefinition CurrentNumberingSystemDefinition
		{
			get { return CurrentDefinition.NumberingSystem; }
			set
			{
				if (!CurrentDefinition.NumberingSystem.Equals(value))
				{
					CurrentDefinition.NumberingSystem = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public string CurrentCollationRulesType
		{
			get { return _currentCollationRulesType.ToString(); }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					var type = (CollationRulesType) Enum.Parse(typeof(CollationRulesType), value);
					if (type != _currentCollationRulesType)
					{
						switch (type)
						{
							case CollationRulesType.DefaultOrdering:
							case CollationRulesType.CustomIcu:
								CurrentDefinition.DefaultCollation = new IcuRulesCollationDefinition(CurrentDefinition.DefaultCollationType) {WritingSystemFactory = _writingSystemFactory};
								break;

							case CollationRulesType.CustomSimple:
								CurrentDefinition.DefaultCollation = new SimpleRulesCollationDefinition(CurrentDefinition.DefaultCollationType);
								break;

							case CollationRulesType.OtherLanguage:
								CurrentDefinition.DefaultCollation = new SystemCollationDefinition();
								break;
						}
						_currentCollationRulesType = type;
						OnCurrentItemUpdated();
					}
				}
			}
		}

		public string CurrentCollationRules
		{
			get
			{
				switch (_currentCollationRulesType)
				{
					case CollationRulesType.DefaultOrdering:
						return string.Empty;
					case CollationRulesType.CustomIcu:
						return ((IcuRulesCollationDefinition) CurrentDefinition.DefaultCollation).CollationRules;
					case CollationRulesType.CustomSimple:
						var simpleCollation = (SimpleRulesCollationDefinition) CurrentDefinition.DefaultCollation;
						return simpleCollation.SimpleRules == string.Empty ? DefaultCustomSimpleSortRules : simpleCollation.SimpleRules;
					case CollationRulesType.OtherLanguage:
						return ((SystemCollationDefinition) CurrentDefinition.DefaultCollation).LanguageTag ?? string.Empty;
				}
				return string.Empty;
			}
			set
			{
				switch (_currentCollationRulesType)
				{
					case CollationRulesType.CustomIcu:
						var icuCollation = (IcuRulesCollationDefinition) CurrentDefinition.DefaultCollation;
						if (icuCollation.IcuRules != value || icuCollation.Imports.Count > 0)
						{
							icuCollation.Imports.Clear();
							icuCollation.IcuRules = value;
							OnCurrentItemUpdated();
						}
						break;
					case CollationRulesType.CustomSimple:
						var simpleCollation = (SimpleRulesCollationDefinition) CurrentDefinition.DefaultCollation;
						if (simpleCollation.SimpleRules != value)
						{
							simpleCollation.SimpleRules = value;
							OnCurrentItemUpdated();
						}
						break;
					case CollationRulesType.OtherLanguage:
						var systemCollation = (SystemCollationDefinition) CurrentDefinition.DefaultCollation;
						if (systemCollation.LanguageTag != value)
						{
							systemCollation.LanguageTag = value;
							OnCurrentItemUpdated();
						}
						break;
				}
			}
		}

		public virtual List<WritingSystemDefinition> WritingSystemDefinitions =>
			_writingSystemDefinitions;

		public enum SelectionsForSpecialCombo
		{
			//Note this list is a bit restrictive in that you may want to change the region for a given audio writing system i.e. en-Zxxx-GB-x-audio vs en-Zxxx-US-x-audio
			None=0,
			Ipa=1,
			Voice=2,
			ScriptRegionVariant=3,
			UnlistedLanguageDetails=4
		}

		public SelectionsForSpecialCombo SelectionForSpecialCombo
		{
			get
			{
				if (LockedSpecialCombo != SelectionsForSpecialCombo.None)
				{
					return LockedSpecialCombo;
				}

				if (_currentWritingSystem.IsVoice)
				{
					return SelectionsForSpecialCombo.Voice;
				}
				// IPA writing systems really shouldn't have scripts. We want to clear the script if a user selects IPA from the combobox
				// If there is a non-default script present on a LanguageTag with IPA load it up in the Script/Region/Variant instead.
				// This will avoid silently clearing script codes from existing IPA writing systems that didn't follow best practice on scripts.
				if (_currentWritingSystem.IpaStatus != IpaStatusChoices.NotIpa
					&& (_currentWritingSystem.Script == null || IsScriptImplied(_currentWritingSystem.LanguageTag)))
				{
					return SelectionsForSpecialCombo.Ipa;
				}
				if (_currentWritingSystem.Language == UnlistedLanguage)
				{
					return SelectionsForSpecialCombo.UnlistedLanguageDetails;
				}
				if ((_currentWritingSystem.Script != null && !IsScriptImplied(_currentWritingSystem.LanguageTag))
					|| _currentWritingSystem.Region != null
					|| _currentWritingSystem.Variants.Count > 0
				)
				{
					return SelectionsForSpecialCombo.ScriptRegionVariant;
				}
				return SelectionsForSpecialCombo.None;
			}
		}

		public IpaStatusChoices CurrentIpaStatus
		{
			 get
			{
				if (_currentWritingSystem == null)
					return IpaStatusChoices.NotIpa;
				return _currentWritingSystem.IpaStatus;
			}
			set
			{
				Guard.AgainstNull(_currentWritingSystem, "CurrentWritingSystem");
				if (_currentWritingSystem.IpaStatus != value)
				{
					if(String.IsNullOrEmpty(_currentWritingSystem.Language))
					{
						_currentWritingSystem.Language = UnlistedLanguage;
					}
					_currentWritingSystem.IpaStatus = value;
					OnCurrentItemUpdated();
				}
			}
		}

		// This is currently only useful for setting the region combobox, since it is expecting an IANASubtag
		public RegionSubtag CurrentRegionTag
		{
			get
			{
				if (CurrentDefinition == null)
					return null;

				return CurrentRegion;
			}
			set { throw new NotImplementedException(); }
		}
		#endregion

		#region Spellcheck methods and SpellCheckInfo class

		public string CurrentSpellCheckingId
		{
			get
			{
				if (CurrentDefinition == null)
					return string.Empty;
				return CurrentDefinition.SpellCheckingId;
			}
			set
			{
				if (CurrentDefinition.SpellCheckingId != value)
				{
					CurrentDefinition.SpellCheckingId = value;
					OnCurrentItemUpdated();
				}
			}
		}

		public SpellCheckInfo CurrentSpellChecker
		{
			get
			{
				if (_spellCheckerItems == null || _spellCheckerItems.Count == 0)
					return null;
				return _spellCheckerItems.FirstOrDefault(item => item.GetSpellCheckingId() == CurrentSpellCheckingId);
			}
			set
			{
				CurrentSpellCheckingId = value.GetSpellCheckingId();
			}
		}

		public List<SpellCheckInfo> GetSpellCheckComboBoxItems()
		{
			_spellCheckerItems.Clear();

			// add None option
			_spellCheckerItems.Add(new SpellCheckInfo(""));

			try
			{
				using (var broker = new Broker())
				{
					// add installed dictionaries
					_spellCheckerItems.AddRange(broker.Dictionaries.Select(dictionaryInfo => new SpellCheckInfo(dictionaryInfo)));

					// add current dictionary, if not installed
					if (!string.IsNullOrEmpty(CurrentSpellCheckingId))
					{
						if (!broker.DictionaryExists(CurrentSpellCheckingId))
							_spellCheckerItems.Add(new SpellCheckInfo(CurrentSpellCheckingId));
					}
				}
			}
			catch (DllNotFoundException)
			{
				//If Enchant is not installed we expect an exception.
			}
			catch (Exception e)//there are other errors we can get from the enchant binding
			{
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
												"The Enchant Spelling engine encountered an error: " + e.Message);
			}

			return _spellCheckerItems;
		}

		public class SpellCheckInfo
		{
			private readonly DictionaryInfo _info;
			private readonly string _id = "";

			public SpellCheckInfo(DictionaryInfo info)
			{
				_info = info;
			}

			public SpellCheckInfo(string notInstalledId)
			{
				_id = notInstalledId;
			}

			public bool HasDictionaryInfo()
			{
				if (_info != null)
				{
					return true;
				}
				return false;
			}

			public DictionaryInfo GetDictionaryInfo()
			{
				return _info;
			}

			public override string ToString()
			{
				if (_info != null)
				{
					try
					{
						string id = _info.Language.Replace('_', '-');
						CultureInfo cultureInfo = CultureInfo.GetCultureInfo(id);
						return _info.Language + " (" + cultureInfo.NativeName + ")";
					}
					catch
					{
						// don't care if this fails it was just to add a little more info to user.
						//mono doesn't support this now
						return _info.Language;
					}
				}
				if (!String.IsNullOrEmpty(_id))
				{
					return _id + " (" + StringCatalog.Get("Not installed") + ")";
				}
				return "None";
			}

			public string GetSpellCheckingId()
			{
				if (HasDictionaryInfo())
				{
					return GetDictionaryInfo().Language;
				}
				return _id;
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

		/// <summary>
		/// Fired when the user chooses to "Conflate" a writing systems but before the WS is deleted in the repo.
		/// Used to notify the consuming application of a conflation and process a boolean response
		/// from the app as to whether it can conflate the WS or not (i.e. the WS is in use)
		/// </summary>
		public event AskIfOkToConflateEventHandler AskIfOkToConflateWritingSystems;

		/// <summary>
		/// Fired when the user chooses to "Delete" a writing systems but before the WS is deleted in the repo.
		/// Used to notify the consuming application of a deletion and process a boolean response
		/// from the app as to whether it can delete the WS or not (i.e. the WS is in use)
		/// </summary>
		public event AskIfOkToDeleteEventHandler AskIfOkToDeleteWritingSystems;

		/// <summary>
		/// Fired when we need to know what writing systems to conflate.
		/// </summary>
		public event DecideWhatToDoWithDataInWritingSystemToBeDeleteEventHandler AskUserWhatToDoWithDataInWritingSystemToBeDeleted;

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
			if (!_usingRepository)
			{
				throw new InvalidOperationException("Unable to delete current selection when there is no writing system store.");
			}
			if (!HasCurrentSelection)
			{
				throw new InvalidOperationException("Unable to delete current selection when there is no current selection.");
			}

			var whatToDo = new WhatToDoWithDataInWritingSystemToBeDeletedEventArgs(CurrentDefinition);
			if(AskUserWhatToDoWithDataInWritingSystemToBeDeleted != null)
			{
				AskUserWhatToDoWithDataInWritingSystemToBeDeleted(this, whatToDo);
			}

			switch(whatToDo.WhatToDo)
			{
				case WhatToDos.Nothing:
						return;
				case WhatToDos.Conflate:
					var wsToConflateWith = whatToDo.WritingSystemIdToConflateWith;
						var okToConflateEventArgs = new AskIfOkToConflateEventArgs(CurrentDefinition.LanguageTag,
																					wsToConflateWith.LanguageTag);
						if (AskIfOkToConflateWritingSystems != null)
						{
							AskIfOkToConflateWritingSystems(this, okToConflateEventArgs);
						}
						if (!okToConflateEventArgs.CanConflate)
						{
							string message = okToConflateEventArgs.ErrorMessage ?? String.Empty;
							ErrorReport.NotifyUserOfProblem(
								String.Format("Can not conflate the input system {0} to {1}. {2}",
												CurrentDefinition.LanguageTag,
												wsToConflateWith, message));
							return;
						}
						if (CurrentDefinition != null && _writingSystemRepository.Contains(CurrentDefinition.LanguageTag))
						{
							if (wsToConflateWith != null)
							{
								_writingSystemRepository.Conflate(CurrentDefinition.LanguageTag, wsToConflateWith.LanguageTag);
							}
						}
					break;
				case WhatToDos.Delete:
					var okToDeleteEventArgs = new AskIfOkToDeleteEventArgs(CurrentDefinition.LanguageTag);
					if (AskIfOkToDeleteWritingSystems != null)
					{
						AskIfOkToDeleteWritingSystems(this, okToDeleteEventArgs);
					}
					if (!okToDeleteEventArgs.CanDelete)
					{
						string message = okToDeleteEventArgs.ErrorMessage ?? String.Empty;
						ErrorReport.NotifyUserOfProblem(
							String.Format("Can not delete the input system {0}. {1}",
											CurrentDefinition.LanguageTag, message));
						return;
					}
					// If you play around with renaming/revising writing systems, the Id assigned to
					// the writing system keeps up with the changes, but the StoreID which is the
					// real key for IWritingSystemRepository methods stays the same.  I find this
					// a questionable design decision myself, but there may be good reasons for it.
					// However, not calling _writingSystemRepository.Remove() can cause problems
					// with data getting out of sync.  (See https://jira.sil.org/browse/WS-281 for
					// an example of such problems.)
					if (CurrentDefinition != null && _writingSystemRepository.Contains(CurrentDefinition.Id))
					{
						_writingSystemRepository.Remove(CurrentDefinition.Id);
					}
					break;
			}
			// new index will be next writing system or previous if this was the last in the list
			int newIndex = (CurrentIndex == WritingSystemCount - 1) ? CurrentIndex - 1 : CurrentIndex;
			CurrentDefinition.MarkedForDeletion = true;
			_deletedWritingSystemDefinitions.Add(CurrentDefinition);
			WritingSystemDefinitions.RemoveAt(CurrentIndex);
			CurrentIndex = newIndex;
			OnAddOrDelete();
		}

		/// <summary>
		/// Get list of existing known ids, both permanent and temporary and determine a unique language tag
		/// </summary>
		private string ToUniqueLanguageTag(string ietfLanguageTag)
		{
			var wsIds = new List<string>();
			foreach (WritingSystemDefinition wsT in _writingSystemRepository.AllWritingSystems)
				wsIds.Add(wsT.LanguageTag);
			foreach (WritingSystemDefinition wsT in WritingSystemDefinitions)
				wsIds.Add(wsT.LanguageTag);
			return IetfLanguageTag.ToUniqueLanguageTag(ietfLanguageTag, wsIds);
		}

		/// <summary>
		/// Makes a copy of the currently selected writing system and selects the new copy.
		/// </summary>
		public void DuplicateCurrent()
		{
			if (!_usingRepository)
			{
				throw new InvalidOperationException("Unable to duplicate current selection when there is no writing system store.");
			}
			if (!HasCurrentSelection)
			{
				throw new InvalidOperationException("Unable to duplicate current selection when there is no current selection.");
			}
			// Doing a true clone type duplication is too dangerous -- it's easy for a user to delete
			// existing writing systems unknowingly, thus deleting data in those writing systems.
			// Get list of existing known ids, both permanent and temporary so that we don't risk
			// replacing/deleting them unknowingly.
			WritingSystemDefinition ws = CurrentDefinition.Clone();
			ws.LanguageTag = ToUniqueLanguageTag(ws.LanguageTag);
			WritingSystemDefinitions.Insert(CurrentIndex + 1, ws);
			OnAddOrDelete();
			CurrentDefinition = ws;
		}

		/// <summary>
		/// Creates a new writing system and selects it.
		/// </summary>
		/// <returns></returns>
		public virtual void AddNew()
		{
			if (!_usingRepository)
			{
				throw new InvalidOperationException("Unable to add new input system definition when there is no store.");
			}
			WritingSystemDefinition ws;
			if (MethodToShowUiToBootstrapNewDefinition == null)
			{
				ws = _writingSystemFactory.Create();
				ws.Abbreviation = "New";
			}
			else
			{
				ws = MethodToShowUiToBootstrapNewDefinition();
			}
			if (ws == null) //cancelled
			{
				// this causes the Add Language item to be unselected
				OnAddOrDelete();
				return;
			}

			if (ws.Abbreviation == UnlistedLanguage) // special case for Unlisted Language
			{
				ws.Abbreviation = "v"; // TODO magic string!!! UnlistedLanguageView.DefaultAbbreviation;
			}
			// Make sure new writing system is unique before adding
			ws.LanguageTag = ToUniqueLanguageTag(ws.LanguageTag);
			WritingSystemDefinitions.Add(ws);
			CurrentDefinition = ws;
			OnAddOrDelete();
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
			if (!_usingRepository)
			{
				throw new InvalidOperationException("Unable to save when there is no writing system store.");
			}

			SetAllPossibleAndRemoveOthers();
			_writingSystemRepository.Save();
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
			var adaptor = new LdmlDataMapper(_writingSystemFactory);
			adaptor.Write(filePath, _currentWritingSystem, null);
		}

		public void SetAllPossibleAndRemoveOthers()
		{
			// NOTE: It is not a good idea to remove and then add all writing systems, even though it would
			// NOTE: accomplish the same goal as any information in the LDML file not used by palaso would be lost.
			var unsettableWritingSystems = new List<WritingSystemDefinition>();
			foreach (var ws in WritingSystemDefinitions)
			{
				if (_writingSystemRepository.CanSet(ws))
				{
					_writingSystemRepository.Set(ws);
				}
				else
				{
					unsettableWritingSystems.Add(ws);
				}
			}
			foreach (WritingSystemDefinition unsettableWs in unsettableWritingSystems)
			{
				unsettableWs.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(unsettableWs.LanguageTag, _writingSystemRepository.AllWritingSystems.Select(ws => ws.LanguageTag));
				OnAddOrDelete();
				_writingSystemRepository.Set(unsettableWs);
			}
		}

		/// <summary>
		/// Activates the custom keyboard for the currently selected writing system.
		/// </summary>
		public void ActivateCurrentKeyboard()
		{
			CurrentDefinition?.LocalKeyboard?.Activate();
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
				return null;
			if (CurrentDefinition.DefaultCollation.Collator == null)
				return testString;
			var stringList = new List<SortKey>();
			foreach (string str in testString.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
				stringList.Add(CurrentDefinition.DefaultCollation.Collator.GetSortKey(str));
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
			return CurrentDefinition.DefaultCollation.Validate(out message);
		}

		/// <summary>
		/// Imports the given file into the writing system store.
		/// </summary>
		/// <param name="fileName">Full path of file to import</param>
		public void ImportFile(string fileName)
		{
			if (!_usingRepository)
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
			var adaptor = new LdmlDataMapper(_writingSystemFactory);
			WritingSystemDefinition ws = _writingSystemFactory.Create();
			adaptor.Read(fileName, ws);
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
			if (!_usingRepository)
			{
				throw new InvalidOperationException("Unable to add new writing system definition when there is no store.");
			}
			// Make sure new writing system is unique before adding
			definition.LanguageTag = ToUniqueLanguageTag(definition.LanguageTag);
			WritingSystemDefinitions.Add(definition);
			CurrentDefinition = definition;
			OnAddOrDelete();
		}

		internal void RenameIsoCode(WritingSystemDefinition existingWs)
		{
			WritingSystemDefinition newWs = null;
			if (!_usingRepository)
			{
				throw new InvalidOperationException("Unable to add new writing system definition when there is no store.");
			}
			if (MethodToShowUiToBootstrapNewDefinition != null)
			{
				 newWs = MethodToShowUiToBootstrapNewDefinition();
			}
			if (newWs == null) //cancelled
				return;

			existingWs.Language = newWs.Language;

			// Remove First Not WellKnownPrivateUseTag
			for (int i = 0; i < existingWs.Variants.Count; i++)
			{
				if (existingWs.Variants[i].IsPrivateUse && !StandardSubtags.CommonPrivateUseVariants.Contains(existingWs.Variants[i].Code))
				{
					existingWs.Variants.RemoveAt(i);
					break;
				}
			}

			OnSelectionChanged();
			OnCurrentItemUpdated();
		}

		public void IdentifierVoiceSelected()
		{
			CurrentIsVoice = true;
		}

		public void IdentifierNothingSelected()
		{
			// Don't clear the language name here.  See https://jira.sil.org/browse/WS-44.
			// FieldWorks marks custom languages, regions, and scripts with the first
			// available code designated for such purposes, but also adds information to the
			// variant in the private use area.  Don't throw any of that away here.
			if (CurrentIso != UnlistedLanguage && CurrentRegion != "QM" && CurrentScriptCode != "Qaaa")
				CurrentVariant = string.Empty;
			if (CurrentRegion != "QM")
				CurrentRegion = string.Empty;
			if (CurrentScriptCode != "Qaaa")
				CurrentScriptCode = string.Empty;
		}

		public void IdentifierIpaSelected()
		{
			CurrentIsVoice = false;
			CurrentScriptCode = string.Empty;

			//if we're here, the user wants some kind of IPA
			if (CurrentIpaStatus == IpaStatusChoices.NotIpa)
			{
				CurrentIpaStatus = IpaStatusChoices.Ipa;
			}
		}

		public void IdentifierScriptRegionVariantSelected()
		{
			if (CurrentDefinition != null)
			{
				CurrentVariant = GetVariantCodes(CurrentDefinition.Variants);
				CurrentRegion = CurrentDefinition.Region;
				CurrentScriptCode = CurrentDefinition.Script;
				CurrentIsVoice = CurrentDefinition.IsVoice;
				CurrentIpaStatus = CurrentDefinition.IpaStatus;
			}
		}

		public void IdentifierUnlistedLanguageSelected()
		{
			// doesn't do anything at the moment
		}
	}

	public delegate void AskIfDataExistsInWritingSystemToBeDeletedEventHandler(object sender, AskIfDataExistsInWritingSystemToBeDeletedEventArgs args);

	public class AskIfDataExistsInWritingSystemToBeDeletedEventArgs : EventArgs
	{
		public AskIfDataExistsInWritingSystemToBeDeletedEventArgs(string writingSystemid)
		{
			WritingSystemId = writingSystemid;
		}
		public string WritingSystemId{get; private set;}
		public bool ProjectContainsDataInWritingSystemToBeDeleted = false;
		public string ErrorMessage { get; set; }
	}

	public delegate void AskIfOkToConflateEventHandler(object sender, AskIfOkToConflateEventArgs args);

	public class AskIfOkToConflateEventArgs : EventArgs
	{
		public AskIfOkToConflateEventArgs(string writingSystemIdToConflate, string writingSystemIdToConflateWith)
		{
			WritingSystemIdToConflate = writingSystemIdToConflate;
			WritingSystemIdToConflateWith = writingSystemIdToConflateWith;
		}
		public string WritingSystemIdToConflate { get; private set; }
		public string WritingSystemIdToConflateWith { get; private set; }
		public bool CanConflate = true;
		public string ErrorMessage { get; set; }
	}

	public delegate void AskIfOkToDeleteEventHandler(object sender, AskIfOkToDeleteEventArgs args);

	public class AskIfOkToDeleteEventArgs : EventArgs
	{
		public AskIfOkToDeleteEventArgs(string writingSystemIdToDelete)
		{
			WritingSystemIdToDelete = writingSystemIdToDelete;
		}
		public string WritingSystemIdToDelete { get; private set; }
		public bool CanDelete = true;
		public string ErrorMessage { get; set; }
	}

	public delegate void DecideWhatToDoWithDataInWritingSystemToBeDeleteEventHandler(object sender, WhatToDoWithDataInWritingSystemToBeDeletedEventArgs args);

	public class WhatToDoWithDataInWritingSystemToBeDeletedEventArgs : EventArgs
	{
		private WritingSystemDefinition _writingSystemIdToConflateWith;

		public WhatToDoWithDataInWritingSystemToBeDeletedEventArgs(WritingSystemDefinition writingSystemIdToDelete)
		{
			WritingSystemIdToDelete = writingSystemIdToDelete;
			WhatToDo = WhatToDos.Delete;
		}

		public WhatToDos WhatToDo { get; set; }
		public WritingSystemDefinition WritingSystemIdToDelete { get; private set; }
		public WritingSystemDefinition WritingSystemIdToConflateWith
		{
			get
			{
				if(WhatToDo==WhatToDos.Nothing || WhatToDo==WhatToDos.Delete)
				{
					return null;
				}
				return _writingSystemIdToConflateWith;
			}
			set { _writingSystemIdToConflateWith = value; }
		}
	}

	public enum WhatToDos
	{
		Nothing,
		Conflate,
		Delete
	}
}
