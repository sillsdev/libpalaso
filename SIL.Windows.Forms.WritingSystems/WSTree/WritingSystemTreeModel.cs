using System;
using System.Collections.Generic;
using System.Linq;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.WSTree
{
	/// <summary>
	/// This is the Presentation Model (e.g., logic, no UI) for the list
	/// of existing writing systems and ones the user might want to add.
	/// </summary>
	public class WritingSystemTreeModel
	{
		private readonly WritingSystemSetupModel _setupModel;
		public event EventHandler UpdateDisplay;

		public WritingSystemTreeModel(WritingSystemSetupModel setupModel)
		{
			_setupModel = setupModel;
			_setupModel.ItemAddedOrDeleted += OnSetupModel_ItemAddedOrDeleted;
			_setupModel.CurrentItemUpdated += OnCurrentItemUpdated;
			Suggestor = new WritingSystemSuggestor(_setupModel.WritingSystemFactory);
		}

		void OnCurrentItemUpdated(object sender, EventArgs e)
		{
			UpdateDisplayNow();
		}

		void OnSetupModel_ItemAddedOrDeleted(object sender, EventArgs e)
		{
			UpdateDisplayNow();
		}

		private void UpdateDisplayNow()
		{
			if(UpdateDisplay !=null)
				UpdateDisplay.Invoke(this, null);
		}

		/// <summary>
		/// Given some existing writiting system definitions for a language, what other ones might they want ot add?
		/// </summary>
		public WritingSystemSuggestor Suggestor { get; set; }


		public WritingSystemTreeItem GetSelectedItem(IEnumerable<WritingSystemTreeItem> items)
		{
			var currentDefinition = _setupModel.CurrentDefinition;
			if (currentDefinition == null)
				return null;

			IEnumerable<WritingSystemTreeItem> parentsAndChildren = items.SelectMany(i => i.Children).Concat(items);

			return (from item in parentsAndChildren
				   where item is WritingSystemDefinitionTreeItem
				   && ((WritingSystemDefinitionTreeItem)item).Definition == currentDefinition
				   select item).FirstOrDefault();
		}

		public IEnumerable<WritingSystemTreeItem> GetTreeItems()
		{
			var items= new List<WritingSystemTreeItem>();

			AddExistingDefinitionsAndSuggestions(items);
			if(items.Count>0)
			{
				items.Add(new NullTreeItem());
			}
			items.Add(new WritingSystemCreateUnknownTreeItem(item=>_setupModel.AddNew()));

			items.Add(new NullTreeItem());

			AddOtherLanguages(items);

			while(items.Last() is NullTreeItem)
				items.RemoveAt(items.Count -1);

			return items;
		}

		private void AddExistingDefinitionsAndSuggestions(List<WritingSystemTreeItem> items)
		{
			var x = new List<WritingSystemDefinition>(_setupModel.WritingSystemDefinitions);

			IEnumerable<IGrouping<string, WritingSystemDefinition>> systemsOfSameLanguage = x.GroupBy(def=>def.Language.Name);

			foreach (IGrouping<string, WritingSystemDefinition> defsOfSameLanguage in systemsOfSameLanguage)
			{
				WritingSystemTreeItem parent;
				WritingSystemDefinition itemToUseForSuggestions;
				if (OneWritingSystemIsASuitableParent(defsOfSameLanguage))
				{
					var primaryDefinition = ChooseMainDefinitionOfLanguage(defsOfSameLanguage);
					parent = MakeExistingDefinitionItem(primaryDefinition);
					parent.Children = new List<WritingSystemTreeItem>(from def in defsOfSameLanguage
									where def != primaryDefinition
									select MakeExistingDefinitionItem(def));
					itemToUseForSuggestions = primaryDefinition;
				}
				else
				{
					parent = new GroupTreeItem(defsOfSameLanguage.First().Language.Name);
					parent.Children = new List<WritingSystemTreeItem>(from def in defsOfSameLanguage
									select MakeExistingDefinitionItem(def));
					itemToUseForSuggestions = defsOfSameLanguage.First();//unprincipled
				}

				if (Suggestor != null)
				{
					foreach (
						IWritingSystemDefinitionSuggestion suggestion in
							Suggestor.GetSuggestions(itemToUseForSuggestions, defsOfSameLanguage))
					{
						var treeItem = new WritingSystemCreationTreeItem(suggestion, OnClickAddCertainDefinition);
						parent.Children.Add(treeItem);
					}
				}

				if (itemToUseForSuggestions.Language == WellKnownSubtags.UnlistedLanguage)
				{
					var treeItem = new WritingSystemRenameUnlistedLanguageTreeItem(item => _setupModel.RenameIsoCode(itemToUseForSuggestions));
					parent.Children.Add(treeItem);
				}

				items.Add(parent);
			}
		}

		private WritingSystemTreeItem MakeExistingDefinitionItem(WritingSystemDefinition definition)
		{
			var item = new WritingSystemDefinitionTreeItem(definition, OnClickExistingDefinition);
			item.Selected = item.Definition == _setupModel.CurrentDefinition;
			return item;
		}

		private void OnClickExistingDefinition(WritingSystemTreeItem treeItem)
		{
			_setupModel.SetCurrentDefinition(((WritingSystemDefinitionTreeItem)treeItem).Definition);
		}

		private WritingSystemDefinition ChooseMainDefinitionOfLanguage(IEnumerable<WritingSystemDefinition> definitions)
		{
			var x = definitions.OrderBy(def => GetSpecificityScore(def));
			return x.First();
		}

		private bool OneWritingSystemIsASuitableParent(IEnumerable<WritingSystemDefinition> definitions)
		{
			if (definitions.Count() == 1)
				return true;
			var x = definitions.OrderBy(GetSpecificityScore).ToArray();
			return GetSpecificityScore(x[0]) != GetSpecificityScore(x[1]);
		}

		private int GetSpecificityScore(WritingSystemDefinition definition)
		{
			int score = 0;
			if (definition.Region != null)
				++score;
			if (definition.Script != null)
				++score;
			if (definition.Variants.Count > 0)
				++score;
			return score;
		}

		private void AddOtherLanguages(List<WritingSystemTreeItem> items)
		{
			var item = new WritingSystemTreeItem("Other Languages", null);
			item.Children = new List<WritingSystemTreeItem>(
							from suggestion in Suggestor.GetOtherLanguageSuggestions(
								_setupModel.WritingSystemDefinitions)
							select new WritingSystemCreationTreeItem(
								suggestion, OnClickAddCertainDefinition));
			if (item.Children.Any())
				items.Add(item);
		}

		private void OnClickAddCertainDefinition(WritingSystemTreeItem treeItem)
		{
			var suggestionItem = (WritingSystemCreationTreeItem)treeItem;
			var def = suggestionItem.ShowDialogIfNeededAndGetDefinition();
			if (def != null)//if the didn't cancel
			{
				_setupModel.AddPredefinedDefinition(def);
			}
		}

		public void ViewLoaded()
		{
			UpdateDisplayNow();
		}
	}
}
