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
		public void GetTopLevelItems_OtherKnownWritingSystemsIsNull_GivesNoOtherLanguagesHeader()
		{
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			var model = new WritingSystemTreeModel(writingSystemStore);
			model.OtherKnownWritingSystems = null;
			Assert.AreEqual(0, model.GetTopLevelItems().Count());
		}

		[Test]
		public void GetTopLevelItems_StoreIsEmptyButOtherLanguagesAreAvailable_GivesOtherLanguageChoiceHeader()
		{
			IWritingSystemStore writingSystemStore = new WritingSystemStoreBase();
			var model = new WritingSystemTreeModel(writingSystemStore);
			model.OtherKnownWritingSystems =  new List<WritingSystemDefinition>(new []{new WritingSystemDefinition("xyz")});
			var items = model.GetTopLevelItems();
			Assert.AreEqual(1, items.Count());
			Assert.AreEqual("Other Languages", items.First().Text);
			var otherLanguages = items.First().Children;
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
			var items = model.GetTopLevelItems();
			Assert.AreEqual(1, items.Count());
			Assert.AreEqual("Other Languages", items.First().Text);
			var otherLanguages = items.First().Children;
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
			var items = model.GetTopLevelItems();
			Assert.AreEqual(2, items.Count());
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
			var items = model.GetTopLevelItems();
			Assert.AreEqual(1, items.Count());

			var subDefinitions = items.First().Children;
			Assert.AreEqual(1, subDefinitions.Count());
			Assert.AreEqual(etrIpa, ((WritingSystemDefinitionTreeItem)subDefinitions.First()).Definition);

		}

	}


}