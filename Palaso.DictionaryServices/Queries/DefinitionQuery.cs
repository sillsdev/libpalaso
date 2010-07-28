using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Queries
{
	class DefinitionQuery:IQuery<LexEntry>
	{
		private IComparer _comparer;
		private WritingSystemDefinition _writingSystemDefinition;

		public DefinitionQuery(Comparer<Guid> guidComparer, WritingSystemDefinition wsDef)
		{
			_comparer = guidComparer;
			_writingSystemDefinition = wsDef;
		}

		public DefinitionQuery(WritingSystemDefinition writingSystemDefinition)
		{
			_writingSystemDefinition = writingSystemDefinition;
			_comparer = writingSystemDefinition.Collator;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{

			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();

			foreach (LexSense sense in entryToQuery.Senses)
			{
				var rawDefinition = sense.Definition[_writingSystemDefinition.Id];
				var definitions = GetTrimmedElementsSeperatedBySemiColon(rawDefinition);
				foreach (string definition in definitions)
				{
					IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
					tokenFieldsAndValues.Add("Form", definition);
					fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
				}
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

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Form", _comparer);
				return sortOrder;
			}
		}

		public override string UniqueLabel
		{
			get { return "DefinitionQuery"; }
		}
	}
}
