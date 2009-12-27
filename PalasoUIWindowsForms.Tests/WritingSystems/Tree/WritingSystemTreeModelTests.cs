using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems.Tree
{
	[TestFixture]
	public class WritingSystemTreeModelTests
	{
		[Test]
		public void GetTopLevelItems_OtherKnownWritingSystemsIsNull_Ok()
		{
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			var model = new WritingSystemTreeModel(writingSystemStore);
			model.OtherKnownWritingSystems = null;
			var items = model.GetTopLevelItems().ToArray();
			Assert.AreEqual("Add Language", items[0].Text);
			Assert.AreEqual(1, items.Count());
		}


		[Test]
		public void GetTopLevelItems_StoreIsEmptyButOtherLanguagesAreAvailable_GivesOtherLanguageChoiceHeader()
		{
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			var model = new WritingSystemTreeModel(writingSystemStore);
			model.OtherKnownWritingSystems =  new List<WritingSystemDefinition>(new []{new WritingSystemDefinition("xyz")});
			var items = model.GetTopLevelItems().ToArray();
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
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			writingSystemStore.Set(overlapDefinition);
			var model = new WritingSystemTreeModel(writingSystemStore);
			model.OtherKnownWritingSystems = new List<WritingSystemDefinition>(new[] { overlapDefinition, notUsedYetDefinition });
			var items = model.GetTopLevelItems().ToArray();
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
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			writingSystemStore.Set(abc);
			writingSystemStore.Set(xyz);
			var model = new WritingSystemTreeModel(writingSystemStore);
			var items = model.GetTopLevelItems().ToArray();
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
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			writingSystemStore.Set(etrIpa);
			writingSystemStore.Set(etr);
			var model = new WritingSystemTreeModel(writingSystemStore);
			var items = model.GetTopLevelItems().ToArray();
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
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			writingSystemStore.Set(etr);
			var model = new WritingSystemTreeModel(writingSystemStore);
			model.Suggestor = new WritingSystemVariantSuggestor();
			var items = model.GetTopLevelItems();

			var children = items.First().Children;
			Assert.IsTrue(children.Any(item => ((WritingSystemCreationTreeItem)item).Definition.Script == "ipa"));
		}
	}


}