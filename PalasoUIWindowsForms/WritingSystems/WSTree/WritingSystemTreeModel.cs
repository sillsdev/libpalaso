using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;
using System.Linq;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	/// <summary>
	/// This is the Presentation Model (e.g., logic, no UI) for the list
	/// of existing writing systems and ones the user might want to add.
	/// </summary>
	public class WritingSystemTreeModel
	{
		private readonly WritingSystemSetupModel _setupModel;
		private EventHandler _updateDisplay;
		public event EventHandler UpdateDisplay;

		public WritingSystemTreeModel(WritingSystemSetupModel setupModel)
		{
			_setupModel = setupModel;
			_setupModel.ItemAddedOrDeleted += new EventHandler(OnSetupModel_ItemAddedOrDeleted);
			_setupModel.CurrentItemUpdated += new EventHandler(OnCurrentItemUpdated);
			Suggestor = new WritingSystemSuggestor();
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
		public WritingSystemSuggestor Suggestor{get; set;}


		public WritingSystemTreeItem GetSelectedItem(IEnumerable<WritingSystemTreeItem> items)
		{
			var currentDefinition = _setupModel.CurrentDefinition;
			if (currentDefinition == null)
				return null;

//            System.Func<WritingSystemTreeItem, IEnumerable<WritingSystemTreeItem>> getAll =
//                item => item.Children.Concat(new[] {item});

			IEnumerable<WritingSystemTreeItem> parentsAndChildren = (from item in items
								  where item is WritingSystemDefinitionTreeItem
								   select item.Children.Concat(new[] { item })).SelectMany(i => i);

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

			var systemsOfSameLanguage = x.GroupBy(def=>def.ISO);

			foreach (var defsOfSameLanguage in systemsOfSameLanguage)
			{
				var primaryDefinition = ChooseMainDefinitionOfLanguage(defsOfSameLanguage);
				var item = MakeExistingDefinitionItem(primaryDefinition);
				item.Children = new List<WritingSystemTreeItem>(from def in defsOfSameLanguage
								where def != primaryDefinition
								select MakeExistingDefinitionItem(def));
				if (Suggestor != null)
				{
					foreach (
						IWritingSystemDefinitionSuggestion suggestion in
							Suggestor.GetSuggestions(primaryDefinition, defsOfSameLanguage))
					{
						var treeItem = new WritingSystemCreationTreeItem(suggestion, OnClickAddCertainDefinition);
						item.Children.Add(treeItem);
					}
				}
				items.Add(item);
			}
		}

		private WritingSystemTreeItem MakeExistingDefinitionItem(WritingSystemDefinition definition)
		{
			var item = new WritingSystemDefinitionTreeItem(definition, OnClickExistingDefinition);
			item.Selected = item.Definition == _setupModel.CurrentDefinition;
			return (WritingSystemTreeItem) item;
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

		private int GetSpecificityScore(WritingSystemDefinition definition)
		{
			int score = 0;
			if(!string.IsNullOrEmpty(definition.Region))
				++score;
			if(!string.IsNullOrEmpty(definition.Script))
				++score;
			if(!string.IsNullOrEmpty(definition.Variant))
				++score;
			return score;
		}

		private void AddOtherLanguages(List<WritingSystemTreeItem> items)
		{
			var item = new WritingSystemTreeItem("Other Languages", null);
			item.Children = new List<WritingSystemTreeItem>(from suggestion in this.Suggestor.GetOtherLanguageSuggestions(_setupModel.WritingSystemDefinitions)
							select (WritingSystemTreeItem) new WritingSystemCreationTreeItem(suggestion, OnClickAddCertainDefinition));
			if(item.Children.Count()>0)
				items.Add(item );
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
