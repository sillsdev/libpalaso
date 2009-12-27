using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
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
		private readonly IWritingSystemStore _store;
		private readonly WritingSystemSetupPM _setupModel;
		private EventHandler _updateDisplay;

		public WritingSystemTreeModel(IWritingSystemStore store, WritingSystemSetupPM setupModel)
		{
			_store = store;
			_setupModel = setupModel;
		}

		/// <summary>
		/// Given some existing writiting system definitions for a language, what other ones might they want ot add?
		/// </summary>
		public IWritingSystemVariantSuggestor Suggestor{get;set;}

		/// <summary>
		/// Normally, the major-langauge WritingSystems which the user is likely to want to add
		/// </summary>
		public IEnumerable<WritingSystemDefinition> OtherKnownWritingSystems { get; set; }

		public IEnumerable<WritingSystemTreeItem> GetTopLevelItems()
		{
			var items= new List<WritingSystemTreeItem>();

			AddStoreLanguages(items);
			items.Add(new WritingSystemCreateUnknownTreeItem(item=>_setupModel.AddNew()));

			AddOtherLanguages(items);
			return items;
		}

		private void AddStoreLanguages(List<WritingSystemTreeItem> items)
		{
			var x = new List<WritingSystemDefinition>(_store.WritingSystemDefinitions);

			var systemsOfSameLanguage = x.GroupBy(def=>def.ISO);


			foreach (var defsOfSameLanguage in systemsOfSameLanguage)
			{
				var primaryDefinition = ChooseMainDefinitionOfLanguage(defsOfSameLanguage);
				var item = new WritingSystemDefinitionTreeItem(primaryDefinition, OnClickExistingDefinition);
				item.Children = new List<WritingSystemTreeItem>(from def in defsOfSameLanguage
								where def != primaryDefinition
								select (WritingSystemTreeItem) new WritingSystemDefinitionTreeItem(def, OnClickExistingDefinition));
				if (Suggestor != null)
				{
					foreach (
						WritingSystemDefinition oneTheyMightWantToAdd in
							Suggestor.GetSuggestions(primaryDefinition, defsOfSameLanguage))
					{
						item.Children.Add(new WritingSystemCreationTreeItem(oneTheyMightWantToAdd, OnClickAddCertainDefinition));
					}
				}
				items.Add(item);
			}
		}

		private void OnClickExistingDefinition(WritingSystemTreeItem treeItem)
		{
			//review
			_setupModel.SetCurrentIndexFromRfc46464(((WritingSystemDefinitionTreeItem)treeItem).Definition.RFC4646);
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
			if (OtherKnownWritingSystems == null)
				return;
			var item = new WritingSystemTreeItem("Other Languages", null);
			//var otherDefsNotInStore = OtherKnownWritingSystems.Except(_store.WritingSystemDefinitions);
			item.Children = new List<WritingSystemTreeItem>( from definition in OtherKnownWritingSystems
							where !_store.WritingSystemDefinitions.Contains(definition)
							select (WritingSystemTreeItem) new WritingSystemCreationTreeItem(definition, OnClickAddCertainDefinition));
			items.Add(item );
		}

		private void OnClickAddCertainDefinition(WritingSystemTreeItem treeItem)
		{
			_setupModel.AddPredefinedDefinition(((WritingSystemCreationTreeItem)treeItem).Definition);
		}

		public void ViewLoaded(EventHandler handler)
		{
		   _updateDisplay = handler;
		   _updateDisplay.Invoke(this, null);
		}
	}

	public interface IWritingSystemVariantSuggestor
	{
		IEnumerable<WritingSystemDefinition> GetSuggestions(WritingSystemDefinition primary, IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage);
	}

	public  class WritingSystemTreeItem
	{
		protected readonly Action<WritingSystemTreeItem> _clickAction;

		public WritingSystemTreeItem(string text, Action<WritingSystemTreeItem> clickAction)
		{
			_clickAction = clickAction;
			Text=text;
		}

		public string Text { get; set; }

		public List<WritingSystemTreeItem> Children { get; set; }
	   public override string ToString()
		{
			return  Text;
		}

		public TreeNode MakeTreeNode()
		{
			var node = new TreeNode(Text, Children.Select(n => n.MakeTreeNode()).ToArray());
			node.Tag=this;
			node.ForeColor = ForeColor;
			return node;
		}

		protected virtual Color ForeColor
		{
			get { return System.Drawing.Color.Black; }
		}

		public void Clicked()
		{
			if (_clickAction != null)
			{
				_clickAction(this);
			}
		}

	}
	public class WritingSystemDefinitionTreeItem : WritingSystemTreeItem
	{
		public WritingSystemDefinition Definition { get; set; }

		public WritingSystemDefinitionTreeItem(WritingSystemDefinition definition, Action<WritingSystemTreeItem> clickAction)
			: base(definition.DisplayLabel, clickAction)
		{
			Definition = definition;
		}
		public override string ToString()
		{
			return  Definition.DisplayLabel;
		}


	}

	public class WritingSystemCreationTreeItem : WritingSystemDefinitionTreeItem
	{

		public WritingSystemCreationTreeItem(WritingSystemDefinition definition, Action<WritingSystemTreeItem> clickAction)
			: base(definition, clickAction)
		{
		}

		public override string ToString()
		{
			return string.Format("Add {0}", Definition.DisplayLabel);
		}


	}

	public class WritingSystemCreateUnknownTreeItem : WritingSystemTreeItem
	{

		public WritingSystemCreateUnknownTreeItem(Action<WritingSystemTreeItem> clickAction)
			: base("Add Language", clickAction)
		{
		}

		protected virtual Color ForeColor
		{
			get { return System.Drawing.Color.Blue; }
		}

	}
}
