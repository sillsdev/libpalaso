using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Reporting;
using SIL.WritingSystems;
using SIL.WritingSystems.Tests;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class WritingSystemSetupPMTests
	{
		WritingSystemSetupModel _model;
		TestLdmlInXmlWritingSystemRepository _writingSystemRepository;
		string _testFilePath;

		private class DeleteCurrentTestEnvironment : IDisposable
		{
			public bool AskIfOkToConflateWritingSystemsFired { get; private set; }
			public bool AskUserWhatToDoWithDataInWritingSystemToBeDeletedFired { get; private set; }
			public bool AskIfOkToDeleteWritingSystemFired { get; private set; }

			public void OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Delete(object sender, WhatToDoWithDataInWritingSystemToBeDeletedEventArgs args)
			{
				AskUserWhatToDoWithDataInWritingSystemToBeDeletedFired = true;
				args.WhatToDo = WhatToDos.Delete;
			}

			public void OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Conflate(object sender, WhatToDoWithDataInWritingSystemToBeDeletedEventArgs args)
			{
				AskUserWhatToDoWithDataInWritingSystemToBeDeletedFired = true;
				args.WhatToDo = WhatToDos.Conflate;
				args.WritingSystemIdToConflateWith = new WritingSystemDefinition("de");
			}

			public void OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Nothing(object sender, WhatToDoWithDataInWritingSystemToBeDeletedEventArgs args)
			{
				AskUserWhatToDoWithDataInWritingSystemToBeDeletedFired = true;
				args.WhatToDo = WhatToDos.Nothing;
			}

			public void OnAskIfOkToConflateWritingSystems_No(object sender, AskIfOkToConflateEventArgs args)
			{
				AskIfOkToConflateWritingSystemsFired = true;
				args.CanConflate = false;
			}

			public void OnAskIfOkToConflateWritingSystems_Yes(object sender, AskIfOkToConflateEventArgs args)
			{
				AskIfOkToConflateWritingSystemsFired = true;
				args.CanConflate = true;
			}

			public void OnAskIfOkToDeleteWritingSystem_Yes(object sender, AskIfOkToDeleteEventArgs args)
			{
				AskIfOkToDeleteWritingSystemFired = true;
				args.CanDelete = true;
			}

			public void OnAskIfOkToDeleteWritingSystem_No(object sender, AskIfOkToDeleteEventArgs args)
			{
				AskIfOkToDeleteWritingSystemFired = true;
				args.CanDelete = false;
			}

			public void Dispose()
			{
				//do nothing
			}
		}

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			if (!Sldr.IsInitialized)
				Sldr.Initialize();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
		}

		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();

			_testFilePath = Path.GetTempFileName();
			_writingSystemRepository = new TestLdmlInXmlWritingSystemRepository();
			_model = new WritingSystemSetupModel(_writingSystemRepository);
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_testFilePath);
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
		[Category("DesktopRequired")] // Fails on Jenkins because InputLanguage.InstalledInputLanguages returns an empty list.
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
			using (new DeleteCurrentTestEnvironment())
			{
				_model.AddNew();
				_model.CurrentIso = "pt";
				_model.AddNew();
				_model.CurrentIso = "de";
				_model.AddNew();
				_model.CurrentIso = "th";
				var writingSystems = new List<string>();
				for (_model.CurrentIndex = _model.WritingSystemCount - 1;
					 _model.HasCurrentSelection;
					 _model.CurrentIndex--)
				{
					writingSystems.Insert(0, _model.CurrentIso);
				}
				string deletedWritingSystem = writingSystems[1];
				_model.CurrentIndex = 1;
				_model.DeleteCurrent();
				for (_model.CurrentIndex = _model.WritingSystemCount - 1;
					 _model.HasCurrentSelection;
					 _model.CurrentIndex--)
				{
					Assert.AreNotEqual(deletedWritingSystem, _model.CurrentIso);
				}
			}
		}

		[Test]
		public void CurrentSelectionEmpty_IsFalse()
		{
			Assert.IsFalse(_model.HasCurrentSelection);
		}

		[Test]
		public void SetCurrentIndexFromLanguageTag_NotFound_ReturnsFalse()
		{
			Assert.IsFalse(_model.SetCurrentIndexFromLanguageTag("bogus"));
		}

		[Test]
		public void SetCurrentIndexFromLanguageTag_NotFound_ReturnsTrueAndCurrentIsChanged()
		{
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.CurrentRegion = "BR";
			_model.AddNew();
			_model.CurrentIso = "de";
			Assert.IsTrue(_model.SetCurrentIndexFromLanguageTag("de"));
			Assert.AreEqual(1, _model.CurrentIndex);
			Assert.AreEqual("de", _model.CurrentIso);
			Assert.IsTrue(_model.SetCurrentIndexFromLanguageTag("pt-BR"));
			Assert.AreEqual("pt", _model.CurrentIso);
			Assert.AreEqual(0, _model.CurrentIndex);

		}

		[Test]
		public void CurrentSelectByIndex_GetCurrentCorrect()
		{
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.AddNew();
			_model.CurrentIso = "de";
			_model.AddNew();
			_model.CurrentIso = "th";
			_model.CurrentIndex = 1;
			Assert.AreEqual("de", _model.CurrentIso);
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
		public void Add_DuplicateInList()
		{
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				Assert.AreNotEqual("New", _model.CurrentAbbreviation);
			}
			_model.AddNew();
			_model.AddNew();
			Assert.That(_model.CurrentDefinition.LanguageTag, Is.EqualTo("qaa-x-dupl0"));
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
			_model.CurrentIso = "pt";
			bool eventTriggered = false;
			_model.ItemAddedOrDeleted += delegate { eventTriggered = true; };
			_model.DuplicateCurrent();
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_Delete_TriggersOnAddDelete()
		{
			using (new DeleteCurrentTestEnvironment())
			{
				_model.AddNew();
				_model.CurrentIso = "pt";
				bool eventTriggered = false;
				_model.ItemAddedOrDeleted += delegate { eventTriggered = true; };
				_model.DeleteCurrent();
				Assert.IsTrue(eventTriggered);
			}
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
			_model.CurrentIso = "pt";
			_model.AddNew();
			_model.CurrentIso = "de";
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
			_model.CurrentIso = "pt";
			_model.AddNew();
			_model.CurrentIso = "de";
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
			_model.CurrentIso = "pt";
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
			_model.CurrentIso = "pt";
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
			_model.CurrentIso = "pt";
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
			_model.CurrentIso = "pt";
			Assert.IsTrue(eventTriggered);
		}

		[Test]
		public void Event_CurrentItemAssignedButNotChanged_DoesNotTriggerCurrentItemUpdated()
		{
			_model.AddNew();
			bool eventTriggered = false;
			_model.CurrentIso = "pt";
			_model.CurrentItemUpdated += delegate { eventTriggered = true; };
			_model.CurrentIso = "pt";
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
			_model.CurrentIso = "pt";
			_model.AddNew();
			_model.CurrentIso = "de";
			_model.AddNew();
			_model.CurrentIso = "th";
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
			_model.CurrentIso = "pt";
			_model.AddNew();
			Assert.IsTrue(_model.CanSaveCurrent);
			_model.CurrentIso = "pt";
			Assert.IsFalse(_model.CanSaveCurrent);
		}

		[Test]
		public void EditCurrentSelection_UpdatesCanSaveForAll()
		{
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.AddNew();
			_model.CurrentIso = "de";
			_model.AddNew();
			_model.CurrentIso = "de";
			_model.CurrentIndex = 1;
			_model.CurrentIso = "pt";
			_model.CurrentIndex = 0;
			bool[] canSave = _model.WritingSystemListCanSave;
			Assert.IsFalse(canSave[0]);
			Assert.IsFalse(canSave[1]);
			Assert.IsTrue(canSave[2]);
			_model.CurrentIso = "th";
			canSave = _model.WritingSystemListCanSave;
			Assert.IsTrue(canSave[0]);
			Assert.IsTrue(canSave[1]);
			Assert.IsTrue(canSave[2]);
		}

		[Test]
		public void EditCurrentSelection_UpdatesCanSaveAndFixesCycle()
		{
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.AddNew();
			_model.CurrentIso = "de";
			_model.CurrentIndex = 0;
			_model.CurrentIso = "de";
			Assert.IsFalse(_model.CanSaveCurrent);
			_model.CurrentIndex = 1;
			_model.CurrentIso = "pt";
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
			_model.CurrentIso = "pt";
			foreach (KeyValuePair<string, string> languageOption in _model.SortLanguageOptions)
			{
				Assert.AreNotEqual(_model.CurrentLanguageTag, languageOption.Key);
			}
		}

		[Test]
		[Ignore("This tests wether ldml writing systems are listed as possible sortlanguages. That feature is not implemented. Previously this test was ignored (back in 2008). Should the feature be implmented now?")]
		public void SortLanuageOptions_DoesIncludeOtherWritingSystems()
		{
			_model.AddNew();
			_model.CurrentIso = "pt";
			string key = _model.CurrentLanguageTag;
			_model.AddNew();
			_model.CurrentIso = "de";
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
			_model.CurrentIso = "pt";
			string key = _model.CurrentLanguageTag;
			_model.AddNew();
			_model.CurrentIso = "de";
			_model.CurrentCollationRulesType = "OtherLanguage";
			_model.CurrentCollationRules = key;
			key = _model.CurrentLanguageTag;
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
			_model.CurrentIso = "pt";
			_model.CurrentCollationRulesType = "CustomIcu";
			_model.CurrentCollationRules = "&b<a<c";
			Assert.IsNull(_model.TestSort(null));
		}

		[Test]
		public void TestSort_RulesAndString_SortsCorrectly()
		{
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.CurrentCollationRulesType = "CustomIcu";
			_model.CurrentCollationRules = "&b<a<c";
			Assert.AreEqual("b\r\na\r\nc", _model.TestSort("a\r\nb\r\nc"));
		}

		[Test]
		public void ValidateSortRules_ValidIcuRules_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.CurrentCollationRulesType = "CustomIcu";
			_model.CurrentCollationRules = "&b<a<c";
			Assert.IsTrue(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_InvalidIcuRules_IsFalse()
		{
			string message;
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.CurrentCollationRulesType = "CustomIcu";
			_model.CurrentCollationRules = "&&b<a<c";
			Assert.IsFalse(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_ValidSimpleRules_IsTrue()
		{
			string message;
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.CurrentCollationRulesType = "CustomSimple";
			_model.CurrentCollationRules = "b a c";
			Assert.IsTrue(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_InvalidSimpleRules_IsFalse()
		{
			string message;
			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.CurrentCollationRulesType = "CustomSimple";
			_model.CurrentCollationRules = "ab b b";
			Assert.IsFalse(_model.ValidateCurrentSortRules(out message));
		}

		[Test]
		public void ValidateSortRules_ValidOtherLanguage_IsTrue()
		{
			var enWS = new WritingSystemDefinition("en");
			var cd = new IcuRulesCollationDefinition("standard") {IcuRules = "&b<a<c"};
			string message;
			Assert.That(cd.Validate(out message), Is.True);
			enWS.DefaultCollation = cd;
			_writingSystemRepository.WritingSystemFactory.WritingSystems.Add(enWS);

			_model.AddNew();
			_model.CurrentIso = "pt";
			_model.CurrentCollationRulesType = "OtherLanguage";
			_model.CurrentCollationRules = "en";
			Assert.That(_model.ValidateCurrentSortRules(out message), Is.True);
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
			_model.AddNew();
			var filePath = Path.GetTempFileName();
			try
			{
				_model.ExportCurrentWritingSystemAsFile(filePath);
				Assert.IsTrue(File.Exists(filePath));
			}
			finally
			{
				File.Delete(filePath);
			}
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
			_model.CurrentIso = "en";
			_model.CurrentRegion = "BR";
			_model.CurrentVariant = "fonipa";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.Ipa, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_HasScriptAndIPA_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentIso = "en";
			_model.CurrentScriptCode = "Arab";
			_model.CurrentVariant = "fonipa";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_HasRegion_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentIso = "en";
			_model.CurrentRegion = "BR";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_HasKnownScript_GivesScriptRegionVariant()
		{
			_model.AddNew();
			_model.CurrentIso = "en";
			_model.CurrentScriptCode = "Cyrl";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void SelectionForSpecialCombo_HasImplicitScript_GivesNone()
		{
			_model.AddNew();
			_model.CurrentIso = "en";
			_model.CurrentScriptCode = "Latn";
			Assert.AreEqual(WritingSystemSetupModel.SelectionsForSpecialCombo.None, _model.SelectionForSpecialCombo);
		}

		[Test]
		public void VerboseDescriptionWhenNoSubtagsSet()
		{
			_model.CurrentDefinition = new WritingSystemDefinition();
			Assert.AreEqual("Language Not Listed. (qaa)", _model.VerboseDescription(_model.CurrentDefinition));
		}

		[Test]
		public void VerboseDescriptionWhenJustIso()
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
			Assert.AreEqual("Korean", _model.CurrentIso15924Script.ShortName);
		}

		[Test]
		public void SetAllPossibleAndRemoveOthers_HasWritingSystems_SetsToRepo()
		{
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(0));
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
			_model.AddPredefinedDefinition(new WritingSystemDefinition("de"));
			_model.AddPredefinedDefinition(new WritingSystemDefinition("en"));
			_model.SetAllPossibleAndRemoveOthers();
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(3));
		}

		[Test]
		public void SetAllPossibleAndRemoveOthers_HasDuplicateWritingSystems_SetsToRepo()
		{
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(0));
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
			_model.AddPredefinedDefinition(new WritingSystemDefinition("de"));
			_model.AddPredefinedDefinition(new WritingSystemDefinition("en"));
			_model.AddPredefinedDefinition(new WritingSystemDefinition("en"));
			_model.SetAllPossibleAndRemoveOthers();
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(4));
			Assert.That(_writingSystemRepository.Contains("pt"));
			Assert.That(_writingSystemRepository.Contains("de"));
			Assert.That(_writingSystemRepository.Contains("en"));
			Assert.That(_writingSystemRepository.Contains("en-x-dupl0"));
		}

		[Test]
		public void SetAllPossibleAndRemoveOthers_NewDuplicateWs_SetsToRepo()
		{
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(0));
			//reinitialize the model with a prepopulated repo
			_writingSystemRepository.Set(new WritingSystemDefinition("en"));
			_model = new WritingSystemSetupModel(_writingSystemRepository);
			//add a new writing system definition with identical Id
			_model.AddPredefinedDefinition(new WritingSystemDefinition("en"));
			_model.SetAllPossibleAndRemoveOthers();
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(2));
			Assert.That(_writingSystemRepository.Contains("en"));
			Assert.That(_writingSystemRepository.Contains("en-x-dupl0"));
		}

		[Test]
		public void SetAllPossibleAndRemoveOthers_DuplicateIsCreatedFromWsAlreadyInRepo_OriginalWsIsUpdated()
		{
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(0));
			var ws = new WritingSystemDefinition("en-x-yo");
			//reinitialize the model with a prepopulated repo
			_writingSystemRepository.Set(new WritingSystemDefinition("en"));
			_writingSystemRepository.Set(ws);
			_model = new WritingSystemSetupModel(_writingSystemRepository);
			//Now change the Id so it's a duplicate of another ws already in the repo
			ws.Variants.Clear();
			_model.SetAllPossibleAndRemoveOthers();
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(2));
			Assert.That(_writingSystemRepository.Contains("en"));
			Assert.That(_writingSystemRepository.Contains("en-x-dupl0"));
		}

		[Test]
		public void SetAllPossibleAndRemoveOthers_DuplicateIsCreatedFromWsAlreadyInRepoAndWouldBeRenamedToSelf_SetsToRepo()
		{
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(0));
			var ws = new WritingSystemDefinition("en-x-dupl0");
			//reinitialize the model with a prepopulated repo
			_writingSystemRepository.Set(new WritingSystemDefinition("en"));
			_writingSystemRepository.Set(ws);
			_model = new WritingSystemSetupModel(_writingSystemRepository);
			//Now change the Id so it's a duplicate of another ws already in the repo
			ws.Variants.Clear();
			_model.SetAllPossibleAndRemoveOthers();
			Assert.That(_writingSystemRepository.Count, Is.EqualTo(2));
			Assert.That(_writingSystemRepository.Contains("en"));
			Assert.That(_writingSystemRepository.Contains("en-x-dupl0"));
		}

		[Test]
		public void DeleteCurrent_NoDataInProjectAndAllowedToDelete_WritingSystemIsDeleted()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskIfOkToDeleteWritingSystems += e.OnAskIfOkToDeleteWritingSystem_Yes;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				_model.Save();
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.True);
				_model.DeleteCurrent();
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.False);
			}
		}

		[Test]
		public void DeleteCurrent_NoDataInProjectAndNotAllowedToDelete_ThrowsUserException()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskIfOkToDeleteWritingSystems += e.OnAskIfOkToDeleteWritingSystem_No;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
					() => _model.DeleteCurrent()
					);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToConflateButCannotConflate_ThrowsUserException()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Conflate;
				_model.AskIfOkToConflateWritingSystems += e.OnAskIfOkToConflateWritingSystems_No;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));

				Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
					() => _model.DeleteCurrent()
					);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToConflateAndCanConflate_Deleted()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Conflate;
				_model.AskIfOkToConflateWritingSystems += e.OnAskIfOkToConflateWritingSystems_Yes;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.True);
				_model.DeleteCurrent();
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.False);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToConflateAndNoOneListeningToCanConflate_Deletes()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Conflate;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.True);
				_model.DeleteCurrent();
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.False);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToDeleteButNotAllowedToDelete_ThrowsUserException()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Delete;
				_model.AskIfOkToDeleteWritingSystems += e.OnAskIfOkToDeleteWritingSystem_No;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));

				Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
					() => _model.DeleteCurrent()
					);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToDeleteAndCanDelete_Deletes()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Delete;
				_model.AskIfOkToDeleteWritingSystems += e.OnAskIfOkToDeleteWritingSystem_Yes;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.True);
				_model.DeleteCurrent();
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.False);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToDeleteAndNoOneListeningToCanDelete_Deletes()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Delete;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.True);
				_model.DeleteCurrent();
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.False);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserCancels_NothingHappens()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Nothing;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.True);
				_model.DeleteCurrent();
				Assert.That(_model.WritingSystemDefinitions.Any(ws => ws.LanguageTag == "pt"), Is.True);
			}
		}

		[Test]
		public void DeleteCurrent_AskIfDataExistsInWritingSystemToBeDeletedIsUnhandled_DeleteIsNotInterrupted()
		{
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
			Assert.That(_model.WritingSystemCount, Is.EqualTo(1));
			Assert.That("pt", Is.EqualTo(_model.CurrentDefinition.LanguageTag));
			_model.DeleteCurrent();
			Assert.That(_model.WritingSystemCount, Is.EqualTo(0));
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndNoOneListeningToUserChoiceEvent_OkToDeleteIsFired()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskIfOkToDeleteWritingSystems += e.OnAskIfOkToDeleteWritingSystem_Yes; //just need a listener to verifiy that it did fire
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				_model.DeleteCurrent();
				Assert.That(e.AskIfOkToDeleteWritingSystemFired, Is.True);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProject_AskUserWhatToDoWithDataInWritingSystemToBeDeletedFires()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Nothing;
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				_model.DeleteCurrent();
				Assert.That(e.AskUserWhatToDoWithDataInWritingSystemToBeDeletedFired, Is.True);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToConflate_OkToConflateFired()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Conflate;
				_model.AskIfOkToConflateWritingSystems += e.OnAskIfOkToConflateWritingSystems_Yes; //just need a listener to verifiy that it did fire
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				_model.DeleteCurrent();
				Assert.That(e.AskIfOkToConflateWritingSystemsFired, Is.True);
			}
		}

		[Test]
		public void DeleteCurrent_DataInProjectAndUserChoosesToDelete_OkToDeleteFired()
		{
			using (var e = new DeleteCurrentTestEnvironment())
			{
				_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted +=
					e.OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted_Delete;
				_model.AskIfOkToDeleteWritingSystems += e.OnAskIfOkToDeleteWritingSystem_Yes; //just need a listener to verifiy that it did fire
				_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
				_model.DeleteCurrent();
				Assert.That(e.AskIfOkToDeleteWritingSystemFired, Is.True);
			}
		}

		[Test]
		public void IdentifierComboBox_SelectBasicOptionsAFewTimes_DoesNotThrow()
		{
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
			_model.IdentifierNothingSelected();
			_model.IdentifierIpaSelected();
			_model.IdentifierVoiceSelected();
			_model.IdentifierScriptRegionVariantSelected();
			_model.IdentifierNothingSelected();
			_model.IdentifierVoiceSelected();
			_model.IdentifierIpaSelected();
			_model.IdentifierScriptRegionVariantSelected();
			_model.IdentifierNothingSelected();
			_model.IdentifierIpaSelected();
		}

		[Test]
		public void SortRules_RulesAreEmptyAndSortTypeIsCustomSimple_DefaultSortRules()
		{
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
			Assert.That(_model.CurrentCollationRulesType, Is.EqualTo("DefaultOrdering"));
			_model.CurrentCollationRulesType = "CustomSimple";
			Assert.That(_model.CurrentCollationRules, Is.EqualTo(_model.DefaultCustomSimpleSortRules));
		}

		[Test]
		public void SortRules_HasExistingRules_RulesAreNotReplacedWithDefault()
		{
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
			_model.CurrentCollationRulesType = "CustomSimple";
			_model.CurrentCollationRules = "1 2 3 4 5 a b c d e";
			_model.CurrentCollationRulesType = "CustomSimple";
			Assert.That(_model.CurrentCollationRules, Is.Not.EqualTo(_model.DefaultCustomSimpleSortRules));
			_model.CurrentCollationRulesType = "CustomIcu";
			Assert.That(_model.CurrentCollationRules, Is.Not.EqualTo(_model.DefaultCustomSimpleSortRules));
		}

		[Test]
		public void SortRules_RulesAreEmptyAndSortTypeIsCustomIcu_StillEmpty()
		{
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt"));
			Assert.That(_model.CurrentCollationRulesType, Is.EqualTo("DefaultOrdering"));
			_model.CurrentCollationRulesType = "CustomIcu";
			Assert.That(_model.CurrentCollationRules, Is.Empty);
		}

		[Test]
		public void SortRules_DefaultCollationType_SetAfterSingleWsConstruction()
		{
			var singleWs = new WritingSystemDefinition("auc");
			singleWs.Collations.Add(new IcuRulesCollationDefinition("standard") { IcuRules = "&b < a" });
			string junk;
			Assert.IsTrue(singleWs.DefaultCollation.Validate(out junk));
			// SUT
			var model = new WritingSystemSetupModel(singleWs);
			Assert.That(model.CurrentCollationRulesType, Is.EqualTo("CustomIcu"));
			Assert.That(((IcuRulesCollationDefinition) singleWs.DefaultCollation).IcuRules,
				Is.EqualTo("&b < a"));
			Assert.That(model.CurrentCollationRules, Is.EqualTo("&b < a"));
		}

		[Test]
		public void CurrentDefaultFontName_SetSameFontDifferentCase_NotUpdated()
		{
			var font = new FontDefinition("Test");
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt") {DefaultFont = font});
			_model.CurrentDefaultFontName = "test";
			Assert.That(_model.CurrentDefaultFontName, Is.EqualTo("Test"));
		}

// TODO: DDW - If WritingSystemSetupModel doesn't update the default font until Save, WeSay wasn't allowing the user to change the default font.
// Undoing this feature for now
#if WS_FIX
		[Test]
		public void CurrentDefaultFontName_DifferentFontName_DefaultFontNotUpdatedUntilSave()
		{
			var font = new FontDefinition("Test");
			_model.AddPredefinedDefinition(new WritingSystemDefinition("pt") {DefaultFont = font});
			_model.CurrentDefaultFontName = "NewFont";
			Assert.That(_model.CurrentDefaultFontName, Is.EqualTo("NewFont"));
			Assert.That(_model.CurrentDefinition.DefaultFont, Is.EqualTo(font));
			_model.Save();
			Assert.That(_model.CurrentDefinition.DefaultFont.Name, Is.EqualTo("NewFont"));
			Assert.That(_model.CurrentDefinition.Fonts.Count, Is.EqualTo(2));
		}
#endif
	}
}