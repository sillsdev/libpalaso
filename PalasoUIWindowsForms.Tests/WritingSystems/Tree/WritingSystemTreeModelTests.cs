using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems.Tree
{
	[TestFixture]
	public class WritingSystemTreeModelTests
	{
		private IWritingSystemStore _writingSystemStore;
		private WritingSystemTreeModel _model;
		private Mock<WritingSystemSetupPM> _mockSetupModel;

		[SetUp]
		public void Setup()
		{
			_writingSystemStore = new WritingSystemStoreBase();
			_mockSetupModel = new Mock<WritingSystemSetupPM>(_writingSystemStore);
			_model = new WritingSystemTreeModel(_writingSystemStore, _mockSetupModel.Object);
		}

		[Test]
		public void GetTopLevelItems_OtherKnownWritingSystemsIsNull_Ok()
		{
			_model.OtherKnownWritingSystems = null;
			var items = _model.GetTopLevelItems().ToArray();
			Assert.AreEqual("Add Language", items[0].Text);
			Assert.AreEqual(1, items.Count());
		}


		[Test]
		public void GetTopLevelItems_StoreIsEmptyButOtherLanguagesAreAvailable_GivesOtherLanguageChoiceHeader()
		{
			_model.OtherKnownWritingSystems =  new List<WritingSystemDefinition>(new []{new WritingSystemDefinition("xyz")});
			var items = _model.GetTopLevelItems().ToArray();
			Assert.AreEqual(2, items.Count());
			Assert.AreEqual("Add Language", items[0].Text);
			Assert.AreEqual("Other Languages", items[1].Text);
			var otherLanguages = items[1].Children;
			Assert.AreEqual(1,otherLanguages.Count());
		}

		/// <summary>
		/// THe point here is, don't show a language under other, once it has been added to the collection
		/// </summary>
		[Test]
		public void GetTopLevelItems_StoreHasAllOsLanguages_DoesNotGiveLanguageChoiceHeader()
		{
			var overlapDefinition = new WritingSystemDefinition("xyz");
			var notUsedYetDefinition = new WritingSystemDefinition("abc");
			_writingSystemStore.Set(overlapDefinition);
			_model.OtherKnownWritingSystems = new List<WritingSystemDefinition>(new[] { overlapDefinition, notUsedYetDefinition });
			var items = _model.GetTopLevelItems().ToArray();
			Assert.AreEqual(3, items.Count());
			Assert.AreEqual("xyz", items[0].Text);
			Assert.AreEqual("Add Language", items[1].Text);
			Assert.AreEqual("Other Languages", items[2].Text);
			var otherLanguages = items[2].Children;
			Assert.AreEqual(1, otherLanguages.Count());
	  }

		[Test]
		public void GetTopLevelItems_TwoLanguagesInStore_GivesBoth()
		{
			var xyz = new WritingSystemDefinition("xyz");
			var abc = new WritingSystemDefinition("abc");
			_writingSystemStore.Set(abc);
			_writingSystemStore.Set(xyz);
			var items = _model.GetTopLevelItems().ToArray();
			Assert.AreEqual(3, items.Count());
			Assert.AreEqual("abc", items[0].Text);
			Assert.AreEqual("xyz", items[1].Text);
			Assert.AreEqual("Add Language", items[2].Text);
		}

		[Test]
		public void GetTopLevelItems_OneLanguageIsChildOfAnother_GivesParentOnly()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "Edolo", "edo", false);
			var etrIpa = new WritingSystemDefinition("etr", "ipa", string.Empty, string.Empty, "Edolo", "edo", false);
			_writingSystemStore.Set(etrIpa);
			_writingSystemStore.Set(etr);
			var items = _model.GetTopLevelItems().ToArray();
			Assert.AreEqual(2, items.Count());
			Assert.AreEqual("edo", items[0].Text);
			Assert.AreEqual("Add Language", items[1].Text);

			var subDefinitions = items.First().Children;
			Assert.AreEqual(1, subDefinitions.Count());
			Assert.AreEqual(etrIpa, ((WritingSystemDefinitionTreeItem)subDefinitions.First()).Definition);

		}

		/// <summary>
		/// Other details of this behavior are tested in the class used as the suggestor
		/// </summary>
		[Test]
		public void GetTopLevelItems_UsesSuggestor()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "Edolo", "edo", false);
			_writingSystemStore.Set(etr);
			_model.Suggestor = new WritingSystemVariantSuggestor();
			var items = _model.GetTopLevelItems();

			var children = items.First().Children;
			Assert.IsTrue(children.Any(item => ((WritingSystemCreationTreeItem)item).Definition.Script == "ipa"));
		}

		[Test]
		public void ClickAddLanguage_AddNewCalledOnSetupModel()
		{
			/* the tree would look like this:
			  Add Language  <-- we're clicking this one
			*/
			var items = _model.GetTopLevelItems();
			items.First().Clicked();
			_mockSetupModel.Setup(m => m.AddNew());
			_mockSetupModel.Verify(m => m.AddNew(), "Should have called the AddNew method on the setup model");
		}

		[Test]
		public void ClickAddPredifinedLanguage_AddNewCalledOnSetupModel()
		{
			/* the tree would look like this:
				Add Language
				Other Languages
				  Add xyz     <-- we're clicking this one
			 */

			var def = new WritingSystemDefinition("xyz");
			_model.OtherKnownWritingSystems = new List<WritingSystemDefinition>(new[] { def });
			var items = _model.GetTopLevelItems();
			_mockSetupModel.Setup(m => m.AddPredefinedDefinition(def));


			items.Last().Children.First().Clicked();
			_mockSetupModel.Verify(m => m.AddPredefinedDefinition(def));
		}

		[Test]
		public void ClickExistingLanguage_SelectCalledOnSetupModel()
		{
			/* the tree would look like this:
				xyz               <-- we're clicking this one
				Add Language
			 */

			var def = new WritingSystemDefinition("xyz");
			_writingSystemStore.Set(def);
			var items = _model.GetTopLevelItems();
			_mockSetupModel.Setup(m => m.SetCurrentIndexFromRfc46464("xyz"));


			items.First().Clicked();
			_mockSetupModel.Verify(m => m.SetCurrentIndexFromRfc46464("xyz"));
		}
	}


}