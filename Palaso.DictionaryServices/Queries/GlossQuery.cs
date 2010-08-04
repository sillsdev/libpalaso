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
		private string _fieldLabel = "Gloss";
		private IComparer _comparer;
		private WritingSystemDefinition _writingSystemDefinition;

		public GlossQuery(Comparer<string> glossComparer , WritingSystemDefinition wsDef)
		{
			_comparer = glossComparer;
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
			if (entryToQuery.Senses.Count == 0)
			{
				IDictionary<string, object> tokenFieldsAndValues =
						PopulateResults(null, entryToQuery.Guid, null);
				fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
			}
			foreach (LexSense sense in entryToQuery.Senses)
			{
				var rawGloss = sense.Gloss[_writingSystemDefinition.Id];
				if (String.IsNullOrEmpty(rawGloss) || rawGloss.Trim() == ";")
				{
					IDictionary<string, object> tokenFieldsAndValues =
						PopulateResults(null, entryToQuery.Guid, sense.GetOrCreateId());
					fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
				}
				else
				{
					List<string> glosses = GetTrimmedElementsSeperatedBySemiColon(rawGloss);
					foreach (string gloss in glosses)
					{
						IDictionary<string, object> tokenFieldsAndValues =
							PopulateResults(gloss, entryToQuery.Guid, sense.GetOrCreateId());
						fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
					}
				}
			}

			return fieldsandValuesForRecordTokens;
		}

		private IDictionary<string, object> PopulateResults(string definition, Guid entryGuid, string senseGuid)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			tokenFieldsAndValues.Add(_fieldLabel, definition);
			tokenFieldsAndValues.Add("EntryGUID", entryGuid);
			tokenFieldsAndValues.Add("SenseGUID", senseGuid);
			return tokenFieldsAndValues;
		}

		private IDictionary<string, object> GetUnpopulatedResult(LexEntry entryToQuery, LexSense sense)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			tokenFieldsAndValues.Add(_fieldLabel, null);
			tokenFieldsAndValues.Add("EntryGUID", entryToQuery.Guid);
			tokenFieldsAndValues.Add("SenseGUID", sense.GetOrCreateId());
			return tokenFieldsAndValues;
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
				var sortOrder = new SortDefinition[3];
				sortOrder[0] = new SortDefinition(_fieldLabel, _comparer);
				sortOrder[1] = new SortDefinition(KeyMap.EntryGuidFieldLabel, Comparer<Guid>.Default);
				sortOrder[2] = new SortDefinition(KeyMap.SenseGuidFieldLabel, Comparer<String>.Default);
				return sortOrder;
			}
		}

		public override string UniqueLabel
		{
			get { return _fieldLabel + "Query"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			return entryToCheckAgainst[_fieldLabel] == null;
		}
	}
}
