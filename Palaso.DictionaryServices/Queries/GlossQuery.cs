using System;
using System.Collections;
using System.Collections.Generic;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Queries
{
	class GlossQuery : IQuery<LexEntry>
	{
		private IComparer _comparer;
		private WritingSystemDefinition _writingSystemDefinition;

		public GlossQuery(Comparer<Guid> guidComparer, WritingSystemDefinition wsDef)
		{
			_comparer = guidComparer;
			_writingSystemDefinition = wsDef;
		}

		public GlossQuery(WritingSystemDefinition writingSystemDefinition)
		{
			_writingSystemDefinition = writingSystemDefinition;
			_comparer = writingSystemDefinition.Collator;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{

			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();

			foreach (LexSense sense in entryToQuery.Senses)
			{
				var rawGloss = sense.Gloss[_writingSystemDefinition.Id];
				var glosses = GetTrimmedElementsSeperatedBySemiColon(rawGloss);
				foreach (string gloss in glosses)
				{
					IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
					tokenFieldsAndValues.Add("Gloss", gloss);
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
				sortOrder[0] = new SortDefinition("Gloss", _comparer);
				return sortOrder;
			}
		}

		public override string UniqueLabel
		{
			get { return "GlossQuery"; }
		}
	}
}
