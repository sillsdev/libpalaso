using System;
using System.Collections;
using System.Collections.Generic;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Text;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Queries
{
	public class AllGlossesQuery : IQuery<LexEntry>
	{
		private string _fieldLabel = "Gloss";
		private string _writingSystemLabel = "GlossWritingSystem";
		private IComparer _comparer;
		private WritingSystemDefinition _writingSystemDefinition;

		public AllGlossesQuery(Comparer<string> glossComparer)
		{
			_comparer = glossComparer;
		}

		public AllGlossesQuery()
		{
			_comparer = StringComparer.InvariantCulture;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{

			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();
			if (entryToQuery.Senses.Count == 0)
			{
				IDictionary<string, object> tokenFieldsAndValues =
						PopulateResults(null, null, entryToQuery.Guid, null);
				fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
			}
			foreach (LexSense sense in entryToQuery.Senses)
			{
				foreach (LanguageForm form in sense.Gloss.Forms)
				{
					var rawGloss = form.Form;
					if (String.IsNullOrEmpty(rawGloss) || rawGloss.Trim() == ";")
					{
						IDictionary<string, object> tokenFieldsAndValues =
							PopulateResults(null, form.WritingSystemId, entryToQuery.Guid, sense.GetOrCreateId());
						fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
					}
					else
					{
						List<string> glosses = GetTrimmedElementsSeperatedBySemiColon(rawGloss);
						foreach (string gloss in glosses)
						{
							IDictionary<string, object> tokenFieldsAndValues =
								PopulateResults(gloss, form.WritingSystemId, entryToQuery.Guid, sense.GetOrCreateId());
							fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
						}
					}
				}
			}

			return fieldsandValuesForRecordTokens;
		}

		private IDictionary<string, object> PopulateResults(string definition, string writingsystemId, Guid entryGuid, string senseGuid)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			tokenFieldsAndValues.Add(_fieldLabel, definition);
			tokenFieldsAndValues.Add(_writingSystemLabel, writingsystemId);
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
			get { return "AllGlossesQuery"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			return entryToCheckAgainst[_fieldLabel] == null;
		}
	}
}
