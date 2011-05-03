using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;

using Palaso.Reporting;
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
			ErrorReport.IsOkToInteractWithUser = false;
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();

			_testFilePath = Path.GetTempFileName();
			IWritingSystemRepository writingSystemRepository = new LdmlInXmlWritingSystemRepository();
			_model = new WritingSystemSetupModel(writingSystemRepository);
			Palaso.Reporting.ErrorReport.IsOkToInteractWithUser = false;
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_testFilePath);
		}

		[Test]
		public void KeyboardNames_HasAtLeastOneKeyboard()
		{
			IEnumerable<string> keyboard = WritingSystemSetupModel.KeyboardNames;
			IEnumerator<string> it = keyboard.GetEnumerator();
			it.MoveNext();
			//Console.WriteLine(String.Format("Current keyboard {0}", it.Current));
			Assert.IsNotNull(it.Current);
			Assert.AreEqual("(default)", it.Current);
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
			_model.CurrentISO = "pt";
			_model.AddNew();
			_model.CurrentISO = "de";
			_model.AddNew();
			_model.CurrentISO = "th";
			var writingSystems = new List<string>();
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				writingSystems.Insert(0, _model.CurrentISO);
			}
			string deletedWritingSystem = writingSystems[1];
			_model.CurrentIndex = 1;
			_model.DeleteCurrent();
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				Assert.AreNotEqual(deletedWritingSystem, _model.CurrentISO);
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
			_model.CurrentISO = "pt";
			_model.CurrentRegion = "BR";
			_model.AddNew();
			_model.CurrentISO = "de";
			Assert.IsTrue(_model.SetCurrentIndexFromRfc46464("de"));
			Assert.AreEqual(1, _model.CurrentIndex);
			Assert.AreEqual("de", _model.CurrentISO);
			Assert.IsTrue(_model.SetCurrentIndexFromRfc46464("pt-BR"));
			Assert.AreEqual("pt", _model.CurrentISO);
			Assert.AreEqual(0, _model.CurrentIndex);

		}

		[Test]
		public void CurrentSelectByIndex_GetCurrentCorrect()
		{
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.AddNew();
			_model.CurrentISO = "de";
			_model.AddNew();
			_model.CurrentISO = "th";
			_model.CurrentIndex = 1;
			Assert.AreEqual("de", _model.CurrentISO);
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
			_model.CurrentISO = "pt";
			bool eventTriggered = false;
			_model.ItemAddedOrDeleted += delegate { eventTriggered = true; };
			_model.DuplicateCurrent();
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_Delete_TriggersOnAddDelete()
		{
			_model.AddNew();
			_model.CurrentISO = "pt";
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
			_model.CurrentISO = "pt";
			_model.AddNew();
			_model.CurrentISO = "de";
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
			_model.CurrentISO = "pt";
			_model.AddNew();
			_model.CurrentISO = "de";
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
			_model.CurrentISO = "pt";
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
			_model.CurrentISO = "pt";
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
			_model.CurrentISO = "pt";
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
			_model.CurrentISO = "pt";
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_CurrentItemAssignedButNotChanged_DoesNotTriggerCurrentItemUpdated()
		{
			_model.AddNew();
			bool eventTriggered = false;
			_model.CurrentISO = "pt";
			_model.CurrentItemUpdated += delegate { eventTriggered = true; };
			_model.CurrentISO = "pt";
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
			_model.CurrentISO = "pt";
			_model.AddNew();
			_model.CurrentISO = "de";
			_model.AddNew();
			_model.CurrentISO = "th";
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
			_model.CurrentISO = "pt";
			_model.AddNew();
			Assert.IsTrue(_model.CanSaveCurrent);
			_model.CurrentISO = "pt";
			Assert.IsFalse(_model.CanSaveCurrent);
		}

		[Test]
		public void EditCurrentSelection_UpdatesCanSaveForAll()
		{
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.AddNew();
			_model.CurrentISO = "de";
			_model.AddNew();
			_model.CurrentISO = "de";
			_model.CurrentIndex = 1;
			_model.CurrentISO = "pt";
			_model.CurrentIndex = 0;
			bool[] canSave = _model.WritingSystemListCanSave;
			Assert.IsFalse(canSave[0]);
			Assert.IsFalse(canSave[1]);
			Assert.IsTrue(canSave[2]);
			_model.CurrentISO = "th";
			canSave = _model.WritingSystemListCanSave;
			Assert.IsTrue(canSave[0]);
			Assert.IsTrue(canSave[1]);
			Assert.IsTrue(canSave[2]);
		}

		[Test]
		public void EditCurrentSelection_UpdatesCanSaveAndFixesCycle()
		{
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.AddNew();
			_model.CurrentISO = "de";
			_model.CurrentIndex = 0;
			_model.CurrentISO = "de";
			Assert.IsFalse(_model.CanSaveCurrent);
			_model.CurrentIndex = 1;
			_model.CurrentISO = "pt";
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
		public void SortLanguageOptions_DoesNotIncludeCurrnt()
		{
			_model.AddNew();
			_model.CurrentISO = "pt";
			foreach (KeyValuePair<string, string> languageOption in _model.SortLanguageOptions)
			{
				Assert.AreNotEqual(_model.CurrentRFC4646, languageOption.Key);
			}
		}

		[Test]
		[Ignore("This tests wether ldml writing systems are listed as possible sortlanguages. That feature is not implemented. Previously this test was ignored (back in 2008). Should the feature be implmented now?")]
		public void SortLanuageOptions_DoesIncludeOtherWritingSystems()
		{
			_model.AddNew();
			_model.CurrentISO = "pt";
			string key = _model.CurrentRFC4646;
			_model.AddNew();
			_model.CurrentISO = "de";
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
			_model.CurrentISO = "pt";
			string key = _model.CurrentRFC4646;
			_model.AddNew();
			_model.CurrentISO = "de";
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
			Assert.IsFalse(_model.UsingWritingSystemRepository);
		}

		[Test]
		public void NormalMode_UsingStore_IsTrue()
		{
			Assert.IsTrue(_model.UsingWritingSystemRepository);
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
			_model.CurrentISO = "pt";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&b<a<c";
			Assert.IsNull(_model.TestSort(null));
		}

		[Test]
		public void TestSort_RulesAndString_SortsCorrectly()
		{
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&b<a<c";
			Assert.AreEqual("b\r\na\r\nc", _model.TestSort("a\r\nb\r\nc"));
		}

		[Test]
		public void ValidateSortRules_ValidIcuRules_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&b<a<c";
			Assert.IsTrue(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_InvalidIcuRules_IsFalse()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			_model.CurrentSortRules = "&&b<a<c";
			Assert.IsFalse(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_ValidSimpleRules_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomSimple.ToString();
			_model.CurrentSortRules = "b a c";
			Assert.IsTrue(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_InvalidSimpleRules_IsFalse()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "pt";
			_model.CurrentSortUsing = WritingSystemDefinition.SortRulesType.CustomSimple.ToString();
			_model.CurrentSortRules = "ab b b";
			Assert.IsFalse(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_ValidOtherLanguage_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentISO = "pt";
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
		public void SelectionForSpecialCombo_HasRegionAndIPA_GivesIPA()
		{
			_model.AddNew();
			_model.CurrentISO = "en";
			_model.CurrentRegion = "BR";
			_model.CurrentVariant = "fonipa";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.Ipa, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_HasRegion_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentISO = "en";
			_model.CurrentRegion = "BR";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_HasKnownScript_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentISO = "en";
			_model.CurrentScriptCode = "Cyrl";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void VerboseDescriptionWhenNoSubtagsSet()
		{
			_model.CurrentDefinition = new WritingSystemDefinition();
			Assert.AreEqual("Unlisted Language. (qaa)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void VerboseDescriptionWhenJustISO()
		{
			_model.CurrentDefinition = new WritingSystemDefinition("en", "", "", "", "", false);
			Assert.AreEqual("English. (en)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void VerboseDescriptionWhenIsoAndScript()
		{
			_model.CurrentDefinition = new WritingSystemDefinition("en", "Kore", "", "", "", false);
			Assert.AreEqual("English written in Korean script. (en-Kore)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void VerboseDescriptionWhenIsoAndRegion()
		{
			_model.CurrentDefinition = new WritingSystemDefinition("en", "", "US", "", "", false);
			Assert.AreEqual("English in US. (en-US)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void VerboseDescriptionWhenIsoScriptRegionVariantPrivateUse()
		{
			_model.CurrentDefinition = new WritingSystemDefinition("en", "Kore", "US", "1901-x-bogus", "", false);
			Assert.AreEqual("English in US written in Korean script. (en-Kore-US-1901-x-bogus)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void VerboseDescriptionWhenIsoAndVariant()
		{
			_model.CurrentDefinition = new WritingSystemDefinition("en", "", "", "1901", "", false);
			Assert.AreEqual("English. (en-1901)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void VerboseDescriptionWhenIsoAndPrivateUse()
		{
			_model.CurrentDefinition = new WritingSystemDefinition("en", "", "", "x-bogus", "", false);
			Assert.AreEqual("English. (en-x-bogus)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void CurrentScriptOptionReturnCorrectScript()
		{
			_model.CurrentDefinition = new WritingSystemDefinition("en", "Kore", "", "", "", false);
			Assert.AreEqual("Korean", _model.CurrentIso15924Script.ShortLabel());
		}

		[Test]
		public void CodeFromPrivateUseInVariant_Empty_Empty()
		{
			_model.AddNew();
			_model.CurrentVariant = "";
			Assert.AreEqual("", _model.CodeFromPrivateUseInVariant());
		}

		[Test]
		public void CodeFromPrivateUseInVariant_NoX_Empty()
		{
			_model.AddNew();
			_model.CurrentVariant = "1901";
			Assert.AreEqual("", _model.CodeFromPrivateUseInVariant());
		}

		[Test]
		public void CodeFromPrivateUseInVariant_HasXWith1Token_FirstToken()
		{
			_model.AddNew();
			_model.CurrentVariant = "1901-x-puu";
			Assert.AreEqual("puu", _model.CodeFromPrivateUseInVariant());
		}

		[Test]
		public void CodeFromPrivateUseInVariant_HasXWith2Tokens_FirstNotWellKnownToken()
		{
			_model.AddNew();
			_model.CurrentScriptCode = "Zxxx";
			_model.CurrentVariant = "x-audio-puu-bogus";
			Assert.AreEqual("puu", _model.CodeFromPrivateUseInVariant());
		}

		[Test]
		public void SetCurrentVariantFromUnlistedLanguageCode_Empty_Empty()
		{
			_model.AddNew();
			_model.SetCurrentVariantFromUnlistedLanguageCode("");
			Assert.That(_model.CurrentVariant, Is.Empty);
		}

		[Test]
		public void SetCurrentVariantFromUnlistedLanguageCode_NoPrivateUse_SetsPrivateUseToLanguageCode()
		{
			_model.AddNew();
			_model.SetCurrentVariantFromUnlistedLanguageCode("ecc");
			Assert.That(_model.CurrentVariant, Is.EqualTo("x-ecc"));
		}

		[Test]
		public void SetCurrentVariantFromUnlistedLanguageCode_ExistingPrivateUse_InsertsLanguageCodeProperly()
		{
			_model.AddNew();
			_model.CurrentVariant = "x-whatever";
			_model.SetCurrentVariantFromUnlistedLanguageCode("ecc");
			Assert.That(_model.CurrentVariant, Is.EqualTo("x-ecc-whatever"));
		}

		[Test]
		public void SetCurrentVariantFromUnlistedLanguageCode_ExistingVariantAndPrivateUse_InsertsLanguageCodeProperly()
		{
			_model.AddNew();
			_model.CurrentVariant = "1901-x-whatever";
			_model.SetCurrentVariantFromUnlistedLanguageCode("ecc");
			Assert.That(_model.CurrentVariant, Is.EqualTo("1901-x-ecc-whatever"));
		}

		[Test]
		public void SetCurrentVariantFromUnlistedLanguageCode_ExistingVariant_InsertsLanguageCodeProperly()
		{
			_model.AddNew();
			_model.CurrentVariant = "1901";
			_model.SetCurrentVariantFromUnlistedLanguageCode("ecc");
			Assert.That(_model.CurrentVariant, Is.EqualTo("1901-x-ecc"));
		}

		[Test]
		public void SetCurrentVariantFromUnlistedLanguageCode_ExistingPrivateUse_DoesNotSetDuplicatePrivateUse()
		{
			_model.AddNew();
			_model.CurrentVariant = "x-aaa";
			_model.SetCurrentVariantFromUnlistedLanguageCode("aaa");
			Assert.That(_model.CurrentVariant, Is.EqualTo("x-aaa"));
		}

		[Test]
		public void SetCurrentVariantFromUnlistedLanguageCode_ExistingCasedPrivateUse_CaseInsensitiveDoesNotDuplicatePrivateUse()
		{
			_model.AddNew();
			_model.CurrentVariant = "x-AAA";
			_model.SetCurrentVariantFromUnlistedLanguageCode("aAa");
			Assert.That(_model.CurrentVariant, Is.EqualTo("x-AAA"));
		}

	}
}