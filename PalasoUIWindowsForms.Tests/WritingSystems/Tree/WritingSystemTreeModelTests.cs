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
			SetDefinitionsInStore(new WritingSystemDefinition[] { });
			_model = new WritingSystemTreeModel(_mockSetupModel.Object);
		}

		[Test]
		public void GetTopLevelItems_OtherKnownWritingSystemsIsNull_Ok()
		{
			SetDefinitionsInStore(new WritingSystemDefinition[] { });
			_model.OtherKnownWritingSystems = null;
			 AssertTreeNodeLabels("Add Language");
		}


		[Test]
		public void GetTopLevelItems_StoreIsEmptyButOtherLanguagesAreAvailable_GivesOtherLanguageChoiceHeader()
		{
			SetDefinitionsInStore(new WritingSystemDefinition[]{});
			_model.OtherKnownWritingSystems =  new List<WritingSystemDefinition>(new []{new WritingSystemDefinition("xyz")});
			AssertTreeNodeLabels("Add Language", "", "Other Languages", "+Add xyz");
		}

		/// <summary>
		/// THe point here is, don't show a language under other, once it has been added to the collection
		/// </summary>
		[Test]
		public void GetTopLevelItems_StoreAlreadyHasAllOsLanguages_DoesNotOfferToCreateItAgain()
		{
			var red = new WritingSystemDefinition("red");
			var blue = new WritingSystemDefinition("blue");
			var osBlue = blue.Clone();
			var green = new WritingSystemDefinition("green");
			_model.OtherKnownWritingSystems = new[]{blue, green};
			SetDefinitionsInStore(new[] { red, blue });
			AssertTreeNodeLabels("red", "blue","", "Add Language","", "Other Languages", "+Add green" /*notice, no blue*/);

	  }


		[Test]
		public void GetTopLevelItems_StoreAlreadyHasAllOsLanguages_DoesNotGiveLanguageChoiceHeader()
		{
			var red = new WritingSystemDefinition("red");
			var blue = new WritingSystemDefinition("blue");
			_model.OtherKnownWritingSystems = new[] { blue };
			SetDefinitionsInStore(new[] { red, blue });
			AssertTreeNodeLabels("red", "blue", "", "Add Language");

		}

		private void AssertTreeNodeLabels(params string[] names)
		{
			var items = _model.GetTreeItems().ToArray();
			int childIndex = 0;
			for (int i = 0, x=-1; i < names.Count(); i++)
			{
				string itemText;
				if (!string.IsNullOrEmpty(names[i]) && names[i].Substring(0, 1) == "+")
				{
					var child = items[x].Children[childIndex];
					itemText = "+"+child.Text;
					++childIndex;
				}
				else
				{
					//if we aren't looking at a child node, move to the next top level node
					++x;
					itemText = items[x].Text;
					childIndex = 0;
				}
				if (names[i] != itemText)
				{
					PrintExpectationsVsActual(names, items);
				}
				Assert.AreEqual(names[i], itemText);
				int total=0;
				foreach (var item in items)
				{
					++total;
					total+=item.Children.Count();
				}
				if(names.Count()!=total)
					PrintExpectationsVsActual(names, items);
				Assert.AreEqual(names.Count(), total,"the actual nodes exceded the number of expected ones");
			}
		}

		private void PrintExpectationsVsActual(string[] names, WritingSystemTreeItem[] items)
		{
			Console.Write("exp: ");
			names.ToList().ForEach(c => Console.Write(c + ", "));
			Console.WriteLine();
			Console.Write("got: ");
			foreach (var item in items)
			{
				Console.Write(item.Text+", ");
				item.Children.ForEach(c=>Console.Write(c.Text+", "));
			}
		}

		[Test]
		public void GetTopLevelItems_TwoLanguagesInStore_GivesBoth()
		{
			var xyz = new WritingSystemDefinition("xyz");
			var abc = new WritingSystemDefinition("abc");
			SetDefinitionsInStore(new[] { abc, xyz });
			AssertTreeNodeLabels( "abc", "xyz","", "Add Language");
		}

		private void SetDefinitionsInStore(IEnumerable<WritingSystemDefinition> defs)
		{
			_mockSetupModel.SetupGet(x => x.WritingSystemDefinitions).Returns(new List<WritingSystemDefinition>(defs));
		}

		[Test]
		public void GetTopLevelItems_OneLanguageIsChildOfAnother_GivesParentOnly()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "Edolo", "edo", false);
			var etrIpa = new WritingSystemDefinition("etr", "ipa", string.Empty, string.Empty, "Edolo", "edo", false);
			SetDefinitionsInStore(new[] { etr,etrIpa });
			AssertTreeNodeLabels("Edolo", "+Edolo (ipa)", "", "Add Language");
		}

		/// <summary>
		/// Other details of this behavior are tested in the class used as the suggestor
		/// </summary>
		[Test]
		public void GetTopLevelItems_UsesSuggestor()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "Edolo", "edo", false);
			SetDefinitionsInStore(new WritingSystemDefinition[] {etr });
			_model.Suggestor = new WritingSystemVariantSuggestor();
			AssertTreeNodeLabels("Edolo", "+Add Edolo (ipa)", "", "Add Language");
		}

		[Test]
		public void ClickAddLanguage_AddNewCalledOnSetupModel()
		{
			/* the tree would look like this:
			  Add Language  <-- we're clicking this one
			*/
			var items = _model.GetTreeItems();
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
			var items = _model.GetTreeItems();
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
			SetDefinitionsInStore(new WritingSystemDefinition[] { def });
			var items = _model.GetTreeItems();
			_mockSetupModel.Setup(m => m.SetCurrentDefinition(def));


			items.First().Clicked();
			_mockSetupModel.Verify(m => m.SetCurrentDefinition(def));
		}
	}


}