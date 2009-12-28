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
		private readonly WritingSystemSetupPM _setupModel;
		private EventHandler _updateDisplay;
		public event EventHandler UpdateDisplay;

		public WritingSystemTreeModel(WritingSystemSetupPM setupModel)
		{
			_setupModel = setupModel;
			_setupModel.ItemAddedOrDeleted += new EventHandler(OnSetupModel_ItemAddedOrDeleted);
			_setupModel.CurrentItemUpdated += new EventHandler(OnCurrentItemUpdated);
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
		public IWritingSystemVariantSuggestor Suggestor{get;set;}

		/// <summary>
		/// Normally, the major-langauge WritingSystems which the user is likely to want to add
		/// </summary>
		public IEnumerable<WritingSystemDefinition> OtherKnownWritingSystems { get; set; }

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
						WritingSystemDefinition oneTheyMightWantToAdd in
							Suggestor.GetSuggestions(primaryDefinition, defsOfSameLanguage))
					{
						var treeItem = new WritingSystemCreationTreeItem(oneTheyMightWantToAdd, OnClickAddCertainDefinition);
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
			if (OtherKnownWritingSystems == null)
				return;
			var item = new WritingSystemTreeItem("Other Languages", null);
			//var otherDefsNotInStore = OtherKnownWritingSystems.Except(_store.WritingSystemDefinitions);
			item.Children = new List<WritingSystemTreeItem>( from definition in OtherKnownWritingSystems
							where !_setupModel.WritingSystemDefinitions.Contains(definition)
							select (WritingSystemTreeItem) new WritingSystemCreationTreeItem(definition, OnClickAddCertainDefinition));
			items.Add(item );
		}

		private void OnClickAddCertainDefinition(WritingSystemTreeItem treeItem)
		{
			_setupModel.AddPredefinedDefinition(((WritingSystemCreationTreeItem)treeItem).Definition);
		}

		public void ViewLoaded()
		{
			UpdateDisplayNow();
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
			Children = new List<WritingSystemTreeItem>();
			_clickAction = clickAction;
			Text=text;
		}

		public bool Selected { get; set; }
		public string Text { get; set; }

		public List<WritingSystemTreeItem> Children { get; set; }


		public TreeNode MakeTreeNode()
		{
			var node = new TreeNode(Text, Children.Select(n => n.MakeTreeNode()).ToArray());
			node.Tag=this;
			node.ForeColor = ForeColor;
			if(this is WritingSystemCreationTreeItem)
			{
				node.NodeFont = new Font(SystemFonts.MessageBoxFont.Name, 8);
			}
			else
				node.NodeFont = new Font(SystemFonts.MessageBoxFont.Name, 11);
			return node;
		}

		protected virtual Color ForeColor
		{
			get { return System.Drawing.Color.Black; }
		}

		public virtual void Clicked()
		{
			if (_clickAction != null)
			{
				_clickAction(this);
			}
		}

	}
	public class NullTreeItem :WritingSystemTreeItem
	{
		public NullTreeItem() : base(string.Empty, new Action<WritingSystemTreeItem>(x=> { }))
		{
		}
	}
	public class WritingSystemDefinitionTreeItem : WritingSystemTreeItem
	{
		public WritingSystemDefinition Definition { get; set; }


		public WritingSystemDefinitionTreeItem(WritingSystemDefinition definition, Action<WritingSystemTreeItem> clickAction)
			: base(definition.ListLabel, clickAction)
		{
			Definition = definition;
		}
   }

	public class WritingSystemCreationTreeItem : WritingSystemDefinitionTreeItem
	{

		public WritingSystemCreationTreeItem(WritingSystemDefinition definition, Action<WritingSystemTreeItem> clickAction)
			: base(definition, clickAction)
		{
			Text = "Add " + definition.ListLabel;
		}
		protected override Color ForeColor
		{
			get { return System.Drawing.Color.Blue; }
		}
	}

	public class WritingSystemCreateUnknownTreeItem : WritingSystemTreeItem
	{

		public WritingSystemCreateUnknownTreeItem(Action<WritingSystemTreeItem> clickAction)
			: base("Add Language", clickAction)
		{
		}

		protected override Color ForeColor
		{
			get { return System.Drawing.Color.Blue; }
		}

	}
}
