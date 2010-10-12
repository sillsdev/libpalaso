using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;

using NUnit.Framework;
using Palaso.Keyboarding;
using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemSetupPMTests
	{
		WritingSystemSetupModel _model;
		string _testFilePath;

		[SetUp]
		public void Setup()
		{
			_testFilePath = Path.GetTempFileName();
			IWritingSystemStore writingSystemStore = new LdmlInXmlWritingSystemStore();
			_model = new WritingSystemSetupModel(writingSystemStore);
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_testFilePath);
		}

		[Test]
		public void KeyboardNames_HasAtLeastOneKeyboard()
		{
			IEnumerable<KeyboardDescriptor> keyboard = WritingSystemSetupModel.Keyboards;
			IEnumerator<KeyboardDescriptor> it = keyboard.GetEnumerator();
			it.MoveNext();
			//Console.WriteLine(String.Format("Current keyboard {0}", it.Current));
			Assert.IsNotNull(it.Current);
			Assert.AreEqual(KeyboardDescriptor.DefaultKeyboard, it.Current);
		}

		[Test]
		public void FontFamilies_HasAtLeastOneFont()
		{
			IEnumerable<FontFamily> font = WritingSystemSetupModel.FontFamilies;
			IEnumerator<FontFamily> it = font.GetEnumerator();
			it.MoveNext();
			//Console.WriteLine(String.Format("Current font {0}", it.Current.Name));
			Assert.IsNotNull(it.Current);
		}

		[Test]
		public void FindInputLanguage_KnownLanguageCanBeFound()
		{
			string knownLanguage = "";
			foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
			{
				//Console.WriteLine("Have language {0}", l.LayoutName);
				knownLanguage = l.LayoutName;
			}
			InputLanguage language = _model.FindInputLanguage(knownLanguage);
			Assert.IsNotNull(language);
			Assert.AreEqual(knownLanguage, language.LayoutName);
			//Console.WriteLine(string.Format("Found language {0}", language.LayoutName));
		}

		[Test]
		public void DeleteCurrent_NoLongerInList()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.AddNew();
			_model.CurrentISO = "ws2";
			_model.AddNew();
			_model.CurrentISO = "ws3";
			List<string> writingSystems = new List<string>();
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				writingSystems.Insert(0, _model.CurrentISO);
			}
			string deletedWS = writingSystems[1];
			_model.CurrentIndex = 1;
			_model.DeleteCurrent();
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				Assert.AreNotEqual(deletedWS, _model.CurrentISO);
			}
		}

		[Test]
		public void CurrentSelectionEmpty_IsFalse()
		{
			Assert.IsFalse(_model.HasCurrentSelection);
		}

		[Test]
		public void SetCurrentIndexFromRfc4646_NotFound_ReturnsFalse()
		{
			Assert.IsFalse(_model.SetCurrentIndexFromRfc46464("bogus"));
		}

		[Test]
		public void SetCurrentIndexFromRfc4646_NotFound_ReturnsTrueAndCurrentIsChanged()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentRegion = "r";
			_model.AddNew();
			_model.CurrentISO = "ws2";
			Assert.IsTrue(_model.SetCurrentIndexFromRfc46464("ws2"));
			Assert.AreEqual(1, _model.CurrentIndex);
			Assert.AreEqual("ws2", _model.CurrentISO);
			Assert.IsTrue(_model.SetCurrentIndexFromRfc46464("ws1-r"));
			Assert.AreEqual("ws1", _model.CurrentISO);
			Assert.AreEqual(0, _model.CurrentIndex);

		}

		[Test]
		public void CurrentSelectByIndex_GetCurrentCorrect()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.AddNew();
			_model.CurrentISO = "ws2";
			_model.AddNew();
			_model.CurrentISO = "ws3";
			_model.CurrentIndex = 1;
			Assert.AreEqual("ws2", _model.CurrentISO);
		}

		[Test]
		public void Add_NewInList()
		{
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				Assert.AreNotEqual("New", _model.CurrentAbbreviation);
			}
			_model.AddNew();
			bool haveNew = false;
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				haveNew |= _model.CurrentAbbreviation == "New";
			}
			Assert.IsTrue(haveNew);
		}

		[Test]
		public void Event_Add_TriggersOnAddDelete()
		{
			bool eventTriggered = false;
			_model.ItemAddedOrDeleted += delegate { eventTriggered = true; };
			_model.AddNew();
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_Duplicate_TriggersOnAddDelete()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			bool eventTriggered = false;
			_model.ItemAddedOrDeleted += delegate { eventTriggered = true; };
			_model.DuplicateCurrent();
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_Delete_TriggersOnAddDelete()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			bool eventTriggered = false;
			_model.ItemAddedOrDeleted += delegate { eventTriggered = true; };
			_model.DeleteCurrent();
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_AddNew_TriggersSelectionChanged()
		{
			bool eventTriggered = false;
			_model.SelectionChanged += delegate { eventTriggered = true; };
			_model.AddNew();
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_SameItemSelected_DoesNotTriggerSelectionChanged()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.AddNew();
			_model.CurrentISO = "ws2";
			bool eventTriggered = false;
			_model.CurrentIndex = 0;
			_model.SelectionChanged += delegate { eventTriggered = true; };
			_model.CurrentIndex = 0;
			Assert.IsFalse(eventTriggered);
		}

		[Test]
		public void Event_DifferentItemSelected_TriggersSelectionChanged()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.AddNew();
			_model.CurrentISO = "ws2";
			bool eventTriggered = false;
			_model.CurrentIndex = 0;
			_model.SelectionChanged += delegate { eventTriggered = true; };
			_model.CurrentIndex = 1;
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_ItemSelectedSelectNegative1_TriggersSelectionChanged()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			bool eventTriggered = false;
			_model.CurrentIndex = 0;
			_model.SelectionChanged += delegate { eventTriggered = true; };
			_model.CurrentIndex = -1;
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_ItemSelectedClearSelection_TriggersSelectionChanged()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			bool eventTriggered = false;
			_model.CurrentIndex = 0;
			_model.SelectionChanged += delegate { eventTriggered = true; };
			_model.ClearSelection();
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_NoItemSelectedClearSelection_DoesNotTriggerSelectionChanged()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			bool eventTriggered = false;
			_model.ClearSelection();
			_model.SelectionChanged += delegate { eventTriggered = true; };
			_model.ClearSelection();
			Assert.IsFalse(eventTriggered);
		}

		[Test]
		public void Event_CurrentItemUpdated_TriggersCurrentItemUpdated()
		{
			_model.AddNew();
			bool eventTriggered = false;
			_model.CurrentItemUpdated += delegate { eventTriggered = true; };
			_model.CurrentISO = "ws1";
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_CurrentItemAssignedButNotChanged_DoesNotTriggerCurrentItemUpdated()
		{
			_model.AddNew();
			bool eventTriggered = false;
			_model.CurrentISO = "ws1";
			_model.CurrentItemUpdated += delegate { eventTriggered = true; };
			_model.CurrentISO = "ws1";
			Assert.IsFalse(eventTriggered);
		}

		[Test]
		public void EmptyAddNew_NewItemSelected()
		{
			_model.AddNew();
			Assert.IsTrue(_model.HasCurrentSelection);
		}

		[Test]
		public void DuplicateCurrent_AppearsInList()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.AddNew();
			_model.CurrentISO = "ws2";
			_model.AddNew();
			_model.CurrentISO = "ws3";
			_model.CurrentIndex = 1;
			_model.DuplicateCurrent();
			Assert.AreEqual(4, _model.WritingSystemCount);
		}

		[Test]
		public void DuplicateCurrent_NoCurrent_ThrowsInvalidOperation()
		{
			Assert.Throws<InvalidOperationException>(
				() => _model.DuplicateCurrent()
			);
		}

		[Test]
		public void DeleteCurrent_NoCurrent_ThrowsInvalidOperation()
		{
			Assert.Throws<InvalidOperationException>(
				() => _model.DeleteCurrent()
			);
		}

		[Test]
		public void EditCurrentSelection_UpdatesCanSaveForCurrent()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.AddNew();
			Assert.IsTrue(_model.CanSaveCurrent);
			_model.CurrentISO = "ws1";
			Assert.IsFalse(_model.CanSaveCurrent);
		}

		[Test]
		public void EditCurrentSelection_UpdatesCanSaveForAll()
		{
			_model.AddNew();
			_model.CurrentISO = "a";
			_model.AddNew();
			_model.CurrentISO = "b";
			_model.AddNew();
			_model.CurrentISO = "b";
			_model.CurrentIndex = 1;
			_model.CurrentISO = "a";
			_model.CurrentIndex = 0;
			bool[] canSave = _model.WritingSystemListCanSave;
			Assert.IsFalse(canSave[0]);
			Assert.IsFalse(canSave[1]);
			Assert.IsTrue(canSave[2]);
			_model.CurrentISO = "c";
			canSave = _model.WritingSystemListCanSave;
			Assert.IsTrue(canSave[0]);
			Assert.IsTrue(canSave[1]);
			Assert.IsTrue(canSave[2]);
		}

		[Test]
		public void EditCurrentSelection_UpdatesCanSaveAndFixesCycle()
		{
			_model.AddNew();
			_model.CurrentISO = "a";
			_model.AddNew();
			_model.CurrentISO = "b";
			_model.CurrentIndex = 0;
			_model.CurrentISO = "b";
			Assert.IsFalse(_model.CanSaveCurrent);
			_model.CurrentIndex = 1;
			_model.CurrentISO = "a";
			bool[] canSave = _model.WritingSystemListCanSave;
			Assert.IsTrue(canSave[0]);
			Assert.IsTrue(canSave[1]);
		}

		[Test]
		public void SortUsingOptions_ReturnsAtLeastOne()
		{
			Assert.IsTrue(WritingSystemSetupModel.SortUsingOptions.GetEnumerator().MoveNext());
		}

		[Test]
		public void SortLanguageOptions_ReturnsAtLeastOne()
		{
			Assert.IsTrue(_model.SortLanguageOptions.GetEnumerator().MoveNext());
		}

		[Test]
		public void SortLanuageOptions_DoesNotIncludeCurrnt()
		{
			_model.AddNew();
			_model.CurrentISO = "TestLanguage";
			foreach (KeyValuePair<string, string> languageOption in _model.SortLanguageOptions)
			{
				Assert.AreNotEqual(_model.CurrentRFC4646, languageOption.Key);
			}
		}

		[Test, Ignore("Not implemented")]
		public void SortLanuageOptions_DoesIncludeOtherWritingSystems()
		{
			_model.AddNew();
			_model.CurrentISO = "TestLanguage1";
			string key = _model.CurrentRFC4646;
			_model.AddNew();
			_model.CurrentISO = "TestLanguage2";
			bool found = false;
			foreach (KeyValuePair<string, string> languageOption in _model.SortLanguageOptions)
			{
				found |= key == languageOption.Key;
			}
			Assert.IsTrue(found);
		}

		[Test]
		public void SortLanuageOptions_DoesNotIncludeOtherWritingSystemsThatMakeACycle()
		{
			_model.AddNew();
			_model.CurrentISO = "TestLanguage1";
			string key = _model.CurrentRFC4646;
			_model.AddNew();
			_model.CurrentISO = "TestLanguage2";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.OtherLanguage.ToString();
			_model.CurrentSortRules = key;
			key = _model.CurrentRFC4646;
			_model.CurrentIndex = 0;
			foreach (KeyValuePair<string, string> languageOption in _model.SortLanguageOptions)
			{
				Assert.AreNotEqual(key, languageOption.Key);
			}
		}

		[Test]
		public void SingleWSMode_DeleteThrows()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.Throws<InvalidOperationException>(
				() => _model.DeleteCurrent()
			);
		}

		[Test]
		public void SingleWSMode_AddNewThrows()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.Throws<InvalidOperationException>(
				() => _model.AddNew()
			);
		}

		[Test]
		public void SingleWSMode_ClearSelectionThrows()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.Throws<InvalidOperationException>(
				() => _model.ClearSelection()
			);
		}

		[Test]
		public void SingleWSMode_ChangingCurrentIndex_Throws()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.Throws<InvalidOperationException>(
				() => _model.CurrentIndex = -1
			);
		}

		[Test]
		public void SingleWSMode_DuplicateCurrent_Throws()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.Throws<InvalidOperationException>(
				() => _model.DuplicateCurrent()
			);
		}

		[Test]
		public void SingleWSMode_Save_Throws()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());

			Assert.Throws<InvalidOperationException>(
				() => _model.Save()
			);
		}

		[Test]
		public void SingleWSMode_HasOnlyOne()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.AreEqual(1, _model.WritingSystemCount);
		}

		[Test]
		public void SingleWSMode_WSIsSelected()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.IsTrue(_model.HasCurrentSelection);
		}

		[Test]
		public void SingleWSMode_UsingStore_IsFalse()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.IsFalse(_model.UsingWritingSystemStore);
		}

		[Test]
		public void NormalMode_UsingStore_IsTrue()
		{
			Assert.IsTrue(_model.UsingWritingSystemStore);
		}

		[Test]
		public void TestSort_NoSelection_DoesNothing()
		{
			_model.ClearSelection();
			Assert.AreEqual("bar\r\nfoo", _model.TestSort("bar\r\nfoo"));
		}

		[Test]
		public void TestSort_NullString_DoesNothing()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&b<a<c";
			Assert.IsNull(_model.TestSort(null));
		}

		[Test]
		public void TestSort_RulesAndString_SortsCorrectly()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&b<a<c";
			Assert.AreEqual("b\r\na\r\nc", _model.TestSort("a\r\nb\r\nc"));
		}

		[Test]
		public void ValidateSortRules_ValidIcuRules_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&b<a<c";
			Assert.IsTrue(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_InvalidIcuRules_IsFalse()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&&b<a<c";
			Assert.IsFalse(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_ValidSimpleRules_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomSimple.ToString();
			_model.CurrentSortRules = "b a c";
			Assert.IsTrue(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_InvalidSimpleRules_IsFalse()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomSimple.ToString();
			_model.CurrentSortRules = "ab b b";
			Assert.IsFalse(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_ValidOtherLanguage_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.OtherLanguage.ToString();
			_model.CurrentSortRules = "en";
			Assert.IsTrue(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ImportFile_SingleWSMode_Throws()
		{
			_model = new WritingSystemSetupModel(new WritingSystemDefinition());
			Assert.Throws<InvalidOperationException>(
				() => _model.ImportFile("foo.xml")
			);
		}

		[Test]
		public void ImportFile_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _model.ImportFile(null)
			);
		}

		[Test]
		public void ImportFile_EmptyString_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => _model.ImportFile(String.Empty)
			);
		}

		[Test]
		public void ImportFile_FileDoesntExist_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => _model.ImportFile("Hopefully this file does not exist.xml")
			);
		}

		[Test]
		public void Export_NoCurrentSelection_Throws()
		{
			_model.ClearSelection ();
			Assert.Throws<InvalidOperationException>(
				() => _model.ExportCurrentWritingSystemAsFile("a.ldml")
			);
		}

		[Test]
		public void Export_CreatesFile()
		{
			_model.AddNew ();
			string filePath = Path.GetTempFileName ();
			_model.ExportCurrentWritingSystemAsFile (filePath);
			Assert.IsTrue (File.Exists (filePath));
		}

		[Test]
		public void SelectionForSpecialCombo_IpaIsOnlyQualifier_GivesIpa()
		{
			_model.AddNew();
			_model.CurrentDefinition.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.Ipa, _model.SelectionForSpecialCombo);
			_model.CurrentDefinition.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.Ipa, _model.SelectionForSpecialCombo);
			_model.CurrentDefinition.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.Ipa, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_VoiceIsOnlyQualifier_GivesVoice()
		{
			_model.AddNew();
			_model.CurrentDefinition.IsVoice = true;
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.Voice, _model.SelectionForSpecialCombo);
		}
		[Test]
		public void SelectionForSpecialCombo_HasRegionAndIPA_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentRegion = "r";
			_model.CurrentVariant = "fonipa";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_HasRegion_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentRegion = "x";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}
		[Test]
		public void SelectionForSpecialCombo_HasKnownScript_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentScriptCode = "Cyrl";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}
		[Test]
		public void SelectionForSpecialCombo_HasUnknownScript_GivesCustom()
		{
			_model.AddNew();
			_model.CurrentScriptCode = "foobar";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.Custom, _model.SelectionForSpecialCombo);
		}

	}
}