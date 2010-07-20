using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Queries
{
	class DefinitionOrGlossQuery:IQuery<LexEntry>
	{
		private WritingSystemDefinition _writingSystemDefinition;

		public DefinitionOrGlossQuery(WritingSystemDefinition WsDef)
		{
			_writingSystemDefinition = WsDef;
		}

		public IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();

			int senseNumber = 0;
			foreach (LexSense sense in entryToQuery.Senses)
			{
				var rawDefinition = sense.Definition[_writingSystemDefinition.Id];
				var definitions = GetTrimmedElementsSeperatedBySemiColon(rawDefinition);

				var rawGloss = sense.Gloss[_writingSystemDefinition.Id];
				var glosses = GetTrimmedElementsSeperatedBySemiColon(rawGloss);

				var definitionAndGlosses = MergeListsWhileExcludingDoublesAndEmptyStrings(definitions, glosses);


				if (definitionAndGlosses.Count == 0)
				{
					IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
					tokenFieldsAndValues.Add("Form", null);
					tokenFieldsAndValues.Add("Sense", senseNumber);
					fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
				}
				else
				{
					foreach (string definition in definitionAndGlosses)
					{
						IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
						tokenFieldsAndValues.Add("Form", definition);
						tokenFieldsAndValues.Add("Sense", senseNumber);
						fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
					}
				}

				senseNumber++;
			}
			return fieldsandValuesForRecordTokens;
		}

		private static List<string> GetTrimmedElementsSeperatedBySemiColon(string text)
		{
			var textElements = new List<string>();
			foreach (string textElement in text.Split(new[] { ';' }))
			{
				string textElementTrimmed = textElement.Trim();
				textElements.Add(textElementTrimmed);
			}
			return textElements;
		}

		private static List<string> MergeListsWhileExcludingDoublesAndEmptyStrings(IEnumerable<string> list1, IEnumerable<string> list2)
		{
			var mergedList = new List<string>();
			foreach (string definitionElement in list1)
			{
				if ((!mergedList.Contains(definitionElement)) && (definitionElement != ""))
				{
					mergedList.Add(definitionElement);
				}
			}
			foreach (string glossElement in list2)
			{
				if (!mergedList.Contains(glossElement) && (glossElement != ""))
				{
					mergedList.Add(glossElement);
				}
			}
			return mergedList;
		}

		public SortDefinition[] SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[2];
				sortOrder[0] = new SortDefinition("Form", _writingSystemDefinition.Collator);
				sortOrder[1] = new SortDefinition("Sense", Comparer<int>.Default);
				return sortOrder;
			}
		}

		public string Label
		{
			get { return "DefinitionOrGlossQuery" + _writingSystemDefinition.Id; }
		}
	}
}
