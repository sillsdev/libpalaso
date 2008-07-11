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
			_model = new SetupPM();
			_testFilePath = Path.GetTempFileName();
			IWritingSystemStore writingSystemStore = new LdmlInXmlWritingSystemStore();
			_model.WritingSystemStore = writingSystemStore;
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_testFilePath);
		}

		[Test]
		public void KeyboardNames_HasAtLeastOneKeyboard()
		{
			IEnumerable<string> keyboard = _model.KeyboardNames;
			IEnumerator<string> it = keyboard.GetEnumerator();
			it.MoveNext();
			//Console.WriteLine(String.Format("Current keyboard {0}", it.Current));
			Assert.IsNotNull(it.Current);
			Assert.AreEqual("(default)", it.Current);
		}

		[Test]
		public void FontFamilies_HasAtLeastOneFont()
		{
			IEnumerable<FontFamily> font = _model.FontFamilies;
			IEnumerator<FontFamily> it = font.GetEnumerator();
			it.MoveNext();
			//Console.WriteLine(String.Format("Current font {0}", it.Current.Name));
			Assert.IsNotNull(it.Current);
		}

		[Test]
		public void EnumerateWritingSystems_HasMoreThanOne()
		{
			IEnumerable<WritingSystemDefinition> ws = _model.WritingSystemsDefinitions;
			IEnumerator<WritingSystemDefinition> it = ws.GetEnumerator();
			it.MoveNext();
			Console.WriteLine(String.Format("Current writingsystem {0}", it.Current.DisplayLabel));
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
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws1"));
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws2"));
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws3"));
			List<string> writingSystems = new List<string>();
			foreach (WritingSystemDefinition ws in _model.WritingSystemsDefinitions)
			{
				writingSystems.Add(ws.Abbreviation);
			}
			string deletedWS = writingSystems[1];
			_model.SelectCurrentByIndex(1);
			_model.DeleteCurrent();
			foreach (WritingSystemDefinition ws in _model.WritingSystemsDefinitions)
			{
				Assert.AreNotEqual(deletedWS, ws.Abbreviation);
			}
		}

		[Test]
		public void CurrentGetEmpty_IsNull()
		{
			Assert.IsNull(_model.Current);
		}

		[Test]
		public void CurrentSelectByIndex_GetCurrentCorrect()
		{
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws1"));
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws2"));
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws3"));
			_model.SelectCurrentByIndex(1);
			WritingSystemDefinition ws = _model.Current;
			Assert.AreEqual("ws2", ws.Abbreviation);
		}

		[Test]
		public void Add_NewInList()
		{
			foreach (WritingSystemDefinition ws in _model.WritingSystemsDefinitions)
			{
				Assert.AreNotEqual("New", ws.Abbreviation);
			}
			WritingSystemDefinition wsNew = _model.AddNew();
			Assert.IsNotNull(wsNew);
			bool haveNew = false;
			foreach (WritingSystemDefinition ws in _model.WritingSystemsDefinitions)
			{
				if (ws.Abbreviation == "New")
				{
					haveNew = true;
				}
			}
			Assert.IsTrue(haveNew);
		}

		[Test]
		public void Rename_CurrentRenamed()
		{
			WritingSystemDefinition d = _model.Current;
			string oldName = d.Abbreviation;
			string newName = "xyz";
			_model.RenameCurrent(newName);
			WritingSystemDefinition test = _model.Current;
			Assert.AreEqual(newName, test.Abbreviation);
		}

		[Test]
		public void Event_Add_TriggersOnAddDelete()
		{
			Assert.IsTrue(false, "NYI");
		}

		[Test]
		public void Event_Duplicate_TriggersOnAddDelete()
		{
			Assert.IsTrue(false, "NYI");
		}

		[Test]
		public void Event_Delete_TriggersOnAddDelete()
		{
			Assert.IsTrue(false, "NYI");
		}

		[Test]
		public void Event_Update_TriggersEvent()
		{
			Assert.IsTrue(false, "NYI");
		}

		[Test]
		public void DuplicateCurrent_AppearsInList()
		{
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws1"));
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws2"));
			_model.WritingSystemStore.Set(new WritingSystemDefinition("ws3"));
			_model.SelectCurrentByIndex(1);
			_model.DuplicateCurrent();
			Assert.AreEqual(4, _model.WritingSystemStore.Count);
		}

	}

}