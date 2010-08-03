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
		private string _fieldLabel = "Form";
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
			if(entryToQuery.Senses.Count == 0)
			{
				IDictionary<string, object> tokenFieldsAndValues =
						PopulateResults(null, entryToQuery.Guid, null);
				fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
			}
			foreach (LexSense sense in entryToQuery.Senses)
			{
				var rawDefinition = sense.Definition[_writingSystemDefinition.Id];
				if (String.IsNullOrEmpty(rawDefinition) || rawDefinition.Trim() == ";")
				{
					IDictionary<string, object>  tokenFieldsAndValues =
						PopulateResults(null, entryToQuery.Guid, sense.GetOrCreateId());
					fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
				}
				else
				{
					List<string> definitions = GetTrimmedElementsSeperatedBySemiColon(rawDefinition);
					foreach (string definition in definitions)
					{
						IDictionary<string, object> tokenFieldsAndValues =
							PopulateResults(definition, entryToQuery.Guid, sense.GetOrCreateId());
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
			get { return "DefinitionQuery"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			return entryToCheckAgainst[_fieldLabel] == null;
		}
	}
}
