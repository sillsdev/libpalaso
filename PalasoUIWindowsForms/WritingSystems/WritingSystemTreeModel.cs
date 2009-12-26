using System;
using System.Collections.Generic;
using System.Text;
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

		public WritingSystemTreeModel(IWritingSystemStore store)
		{
			_store = store;
		}

		/// <summary>
		/// Normally, the major-langauge WritingSystems which the user is likely to want to add
		/// </summary>
		public IEnumerable<WritingSystemDefinition> OtherKnownWritingSystems { get; set; }

		public IEnumerable<WritingSystemTreeItem> GetTopLevelItems()
		{
			var items= new List<WritingSystemTreeItem>();

			AddStoreLanguages(items);
			AddOtherLanguages(items);
			return items;
		}

		private void AddStoreLanguages(List<WritingSystemTreeItem> items)
		{
			var x = new List<WritingSystemDefinition>(_store.WritingSystemDefinitions);

			var isoGroups = x.GroupBy(def=>def.ISO);


			foreach (var defsWithSameIso in isoGroups)
			{
				var mainDef = ChooseMainDefinitionOfLanguage(defsWithSameIso);
				var item = new WritingSystemDefinitionTreeItem(mainDef);
				item.Children = from def in defsWithSameIso
								where def != mainDef
								select (WritingSystemTreeItem) new WritingSystemDefinitionTreeItem(def);
				items.Add(item);
			}
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
			var item = new WritingSystemTreeItem("Other Languages");
			//var otherDefsNotInStore = OtherKnownWritingSystems.Except(_store.WritingSystemDefinitions);
			item.Children = from definition in OtherKnownWritingSystems
							where !_store.WritingSystemDefinitions.Contains(definition)
							select new WritingSystemTreeItem(definition.DisplayLabel);
			items.Add(item );
		}
	}

	public class WritingSystemTreeItem
	{
		public WritingSystemTreeItem(string text)
		{
			Text=text;
		}
		public string Text { get; set; }

		public IEnumerable<WritingSystemTreeItem> Children { get; set; }

	}
	public class WritingSystemDefinitionTreeItem : WritingSystemTreeItem
	{
		public WritingSystemDefinition Definition { get; set; }

		public WritingSystemDefinitionTreeItem(WritingSystemDefinition definition):base(definition.DisplayLabel)
		{
			Definition = definition;
		}
	}
}
