using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;

namespace Palaso.DictionaryServices.Queries
{
	public class MultiTextQuery:IQuery<LexEntry>
	{
		private string _fieldLabel;
		private PalasoDataObjectMultiTextQuery _multiTextQuery;

		public MultiTextQuery(string fieldName, string writingSystemId)
		{
			_multiTextQuery = new PalasoDataObjectMultiTextQuery(fieldName, writingSystemId);
			_fieldLabel = fieldName;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry item)
		{
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>();

			KeyMap guidToEntryMap = new KeyMap{{"Guid", "Entry"}};
			KeyMap guidToSenseMap = new KeyMap{{"Guid", "Sense"}, {"1Guid", "Entry"}};
			KeyMap guidToExampleSentenceMap = new KeyMap{{"Guid", "ExampleSentence"},{"1Guid", "Sense"}, {"2Guid", "Entry"}};

			results.AddRange(_multiTextQuery.RemapKeys(guidToEntryMap).GetResults(item));
			foreach (LexSense sense in item.Senses)
			{
				results.AddRange(_multiTextQuery.RemapKeys(guidToSenseMap).GetResults(sense));
				foreach (LexExampleSentence exampleSentence in sense.ExampleSentences)
				{
					results.AddRange(_multiTextQuery.RemapKeys(guidToExampleSentenceMap).GetResults(exampleSentence));
				}
			}
			return results;
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortorder = new SortDefinition[5];
				sortorder[0] = new SortDefinition(_fieldLabel, Comparer<string>.Default);
				sortorder[1] = new SortDefinition("WritingSystem", Comparer<string>.Default);
				sortorder[2] = new SortDefinition("Entry", Comparer<Guid>.Default);
				sortorder[3] = new SortDefinition("Sense", Comparer<Guid>.Default);
				sortorder[4] = new SortDefinition("ExampleSentence", Comparer<Guid>.Default);
				return sortorder;
			}
		}

		public override string UniqueLabel
		{
			get { throw new NotImplementedException(); }
		}

		public override bool IsUnpopulated(IDictionary<string, object> resultToCheck)
		{
			throw new NotImplementedException();
		}
	}
}
