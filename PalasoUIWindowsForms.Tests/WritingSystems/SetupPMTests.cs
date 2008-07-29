using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;

using NUnit.Framework;

using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{
	[TestFixture]
	public class SetupPMTests
	{
		SetupPM _model;
		string _testFilePath;

		[SetUp]
		public void Setup()
		{
			_testFilePath = Path.GetTempFileName();
			IWritingSystemStore writingSystemStore = new LdmlInXmlWritingSystemStore();
			_model = new SetupPM(writingSystemStore);
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_testFilePath);
		}

		[Test]
		public void KeyboardNames_HasAtLeastOneKeyboard()
		{
			IEnumerable<string> keyboard = SetupPM.KeyboardNames;
			IEnumerator<string> it = keyboard.GetEnumerator();
			it.MoveNext();
			//Console.WriteLine(String.Format("Current keyboard {0}", it.Current));
			Assert.IsNotNull(it.Current);
			Assert.AreEqual("(default)", it.Current);
		}

		[Test]
		public void FontFamilies_HasAtLeastOneFont()
		{
			IEnumerable<FontFamily> font = SetupPM.FontFamilies;
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
			WritingSystemDefinition wsNew = _model.AddNew();
			Assert.IsNotNull(wsNew);
			bool haveNew = false;
			for (_model.CurrentIndex = _model.WritingSystemCount - 1; _model.HasCurrentSelection; _model.CurrentIndex--)
			{
				haveNew |= _model.CurrentAbbreviation == "New";
			}
			Assert.IsTrue(haveNew);
		}

		[Test]
		public void Rename_CurrentRenamed()
		{
			_model.AddNew();
			_model.CurrentISO = "ws1";
			_model.CurrentAbbreviation = "abc";
			string newName = "xyz";
			_model.RenameCurrent(newName);
			Assert.AreEqual(newName, _model.CurrentAbbreviation);
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

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void DuplicateCurrent_NoCurrent_ThrowsInvalidOperation()
		{
			_model.DuplicateCurrent();
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void DeleteCurrent_NoCurrent_ThrowsInvalidOperation()
		{
			_model.DeleteCurrent();
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
			Assert.IsTrue(SetupPM.SortUsingOptions.GetEnumerator().MoveNext());
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

		[Test]
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
	}
}