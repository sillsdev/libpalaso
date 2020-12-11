using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Windows.Forms.WritingSystems.WSTree;
using SIL.WritingSystems;
using SIL.WritingSystems.Tests;

namespace SIL.Windows.Forms.WritingSystems.Tests.Tree
{
	[TestFixture]
	[OfflineSldr]
	public class WritingSystemTreeModelTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _folder = new TemporaryFolder("WritingSystemTreeModelTests");
			private readonly TestWritingSystemFactory _testWritingSystemFactory;

			public TestEnvironment()
			{
				var writingSystemRepository = new TestLdmlInFolderWritingSystemRepository(_folder.Path);
				_testWritingSystemFactory = new TestWritingSystemFactory();
				MockSetupModel = new Mock<WritingSystemSetupModel>(writingSystemRepository);
				SetDefinitionsInStore(new WritingSystemDefinition[] { });
			}

			public Mock<WritingSystemSetupModel> MockSetupModel { get; private set; }

			public WritingSystemTreeModel CreateModel()
			{
				var model = new WritingSystemTreeModel(MockSetupModel.Object);
				model.Suggestor = new WritingSystemSuggestor(_testWritingSystemFactory)
				{
					SuggestIpa = false,
					SuggestOther = false,
					SuggestDialects = false,
					SuggestVoice = false,
					OtherKnownWritingSystems = null
				};
				return model;
			}

			public TestWritingSystemFactory TestWritingSystemFactory
			{
				get { return _testWritingSystemFactory; }
			}

			public void SetDefinitionsInStore(IEnumerable<WritingSystemDefinition> defs)
			{
				MockSetupModel.SetupGet(x => x.WritingSystemDefinitions).Returns(new List<WritingSystemDefinition>(defs));
			}

			public void Dispose()
			{
				_folder.Dispose();
			}
		}

		[Test] // ok
		public void GetTopLevelItems_OtherKnownWritingSystemsIsNull_Ok()
		{
			using (var e = new TestEnvironment())
			{
				e.SetDefinitionsInStore(new WritingSystemDefinition[] {});
				var model = e.CreateModel();
				model.Suggestor.OtherKnownWritingSystems = null;
				AssertTreeNodeLabels(model, "Add Language");
			}
		}


		[Test] // ok
		public void GetTopLevelItems_StoreIsEmptyButOtherLanguagesAreAvailable_GivesOtherLanguageChoiceHeader()
		{
			using (var e = new TestEnvironment())
			{
				e.SetDefinitionsInStore(new WritingSystemDefinition[] {});
				var model = e.CreateModel();
				model.Suggestor.OtherKnownWritingSystems = new List<Tuple<string, string>>()
				{
					new Tuple<string, string>("en", string.Empty)
				};
				AssertTreeNodeLabels(model, "Add Language", "", "Other Languages", "+Add English");
			}
		}

		/// <summary>
		/// THe point here is, don't show a language under other, once it has been added to the collection
		/// </summary>
		[Test] // ok
		public void GetTopLevelItems_StoreAlreadyHasAllOsLanguages_DoesNotOfferToCreateItAgain()
		{
			using (var e = new TestEnvironment())
			{
				var en = new WritingSystemDefinition("en");
				var de = new WritingSystemDefinition("de");
				var model = e.CreateModel();
				model.Suggestor.OtherKnownWritingSystems = new List<Tuple<string, string>>()
				{
					new Tuple<string, string>("de", string.Empty),
					new Tuple<string, string>("fr", string.Empty)
				};
				e.SetDefinitionsInStore(new[] {en, de});
				AssertTreeNodeLabels(
					model,
					"English", "German", "", "Add Language", "", "Other Languages", "+Add French" /*notice, no de*/
				);
			}
		}

		[Test] // ok
		public void GetTopLevelItems_StoreAlreadyHasAllOsLanguages_DoesNotGiveLanguageChoiceHeader()
		{
			using (var e = new TestEnvironment())
			{
				var en = new WritingSystemDefinition("en");
				var de = new WritingSystemDefinition("de");
				var model = e.CreateModel();
				model.Suggestor.OtherKnownWritingSystems = new List<Tuple<string, string>>()
				{
					new Tuple<string, string>("de", string.Empty)
				};
				e.SetDefinitionsInStore(new[] {en, de});
				AssertTreeNodeLabels(model, "English", "German", "", "Add Language");
			}
		}

		private static void AssertTreeNodeLabels(WritingSystemTreeModel model, params string[] names)
		{
			var items = model.GetTreeItems().ToArray();
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
				Assert.AreEqual(names.Count(), total,"the actual nodes exceeded the number of expected ones");
			}
		}

		private static void PrintExpectationsVsActual(string[] names, WritingSystemTreeItem[] items)
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

		[Test] // ok
		public void GetTopLevelItems_TwoLanguagesInStore_GivesBoth()
		{
			using (var e = new TestEnvironment())
			{
				var xyz = new WritingSystemDefinition("en");
				var abc = new WritingSystemDefinition("de");
				var model = e.CreateModel();
				e.SetDefinitionsInStore(new[] { abc, xyz });
				AssertTreeNodeLabels(model, "German", "English", "", "Add Language");
			}
		}

		[Test] // ok
		public void GetTopLevelItems_OneLanguageIsChildOfAnother_GivesParentOnly()
		{
			using (var e = new TestEnvironment())
			{
				var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "edo", false);
				var etrIpa = new WritingSystemDefinition("etr", string.Empty, string.Empty, "fonipa", "edo", false);
				e.SetDefinitionsInStore(new[] {etr, etrIpa});
				var model = e.CreateModel();
				model.Suggestor.SuggestIpa = true;
				AssertTreeNodeLabels(model, "Edolo", "+Edolo (IPA)", "", "Add Language");
			}
		}


		/// <summary>
		/// related to http://projects.palaso.org/issues/show/482
		/// </summary>
		[Test] // ok
		public void GetTopLevelItems_ThreeVariantsAreSyblings_ListsAllUnderGroupHeading()
		{
			using (var e = new TestEnvironment())
			{
				var thai = new WritingSystemDefinition("bzi", "Thai", string.Empty, string.Empty, "bt", false);
				var my = new WritingSystemDefinition("bzi", "Mymr", string.Empty, string.Empty, "bm", false);
				var latin = new WritingSystemDefinition("bzi", "Latn", string.Empty, string.Empty, "bl", false);
				e.SetDefinitionsInStore(new[] {thai, my, latin});
				var model = e.CreateModel();
				// 2018-10-26 Thai is implicit script for bzi so +Bisu (Thai) will not appear separately
				AssertTreeNodeLabels(model, "Bisu", "+Bisu", "+Bisu (Mymr)", "+Bisu (Latn)", "", "Add Language");
			}
		}

		/// <summary>
		/// Other details of this behavior are tested in the class used as the suggestor
		/// </summary>
		[Test, Category("KnownMonoIssue")]
		public void GetTopLevelItems_UsesSuggestor()
		{
			using (var e = new TestEnvironment())
			{
				var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "edo", false);
				e.SetDefinitionsInStore(new[] {etr});
				var model = e.CreateModel();
				model.Suggestor.SuggestIpa = true;
				AssertTreeNodeLabels(model, "Edolo", "+Add IPA input system for Edolo", "", "Add Language");
			}
		}

		[Test]
		public void ClickAddLanguage_AddNewCalledOnSetupModel()
		{
			/* the tree would look like this:
			  Add Language  <-- we're clicking this one
			*/
			using (var e = new TestEnvironment())
			{
				var model = e.CreateModel();
				var items = model.GetTreeItems();
				items.First().Clicked();
				e.MockSetupModel.Setup(m => m.AddNew());
				e.MockSetupModel.Verify(m => m.AddNew(), "Should have called the AddNew method on the setup model");
			}
		}

		[Test] // ok
		public void ClickAddPredefinedLanguage_AddNewCalledOnSetupModel()
		{
			/* the tree would look like this:
				Add Language
				Other Languages
				  Add xyz     <-- we're clicking this one
			 */

			using (var e = new TestEnvironment())
			{
				var def = new WritingSystemDefinition("en")
				{
					DefaultFontSize = 12
				};
				e.TestWritingSystemFactory.WritingSystems.Add(def);
				var model = e.CreateModel();
				model.Suggestor.OtherKnownWritingSystems = new List<Tuple<string, string>>()
				{
					new Tuple<string, string>("en", string.Empty)
				};
				var items = model.GetTreeItems();
				e.MockSetupModel.Setup(m => m.AddPredefinedDefinition(def));


				items.Last().Children.First().Clicked();
				e.MockSetupModel.Verify(m => m.AddPredefinedDefinition(def));
			}
		}

		[Test] // ok
		public void ClickExistingLanguage_SelectCalledOnSetupModel()
		{
			/* the tree would look like this:
				xyz               <-- we're clicking this one
				Add Language
			 */

			using (var e = new TestEnvironment())
			{
				var def = new WritingSystemDefinition("en");
				e.SetDefinitionsInStore(new[] {def});
				var model = e.CreateModel();
				var items = model.GetTreeItems();
				e.MockSetupModel.Setup(m => m.SetCurrentDefinition(def));


				items.First().Clicked();
				e.MockSetupModel.Verify(m => m.SetCurrentDefinition(def));
			}
		}
	}

}