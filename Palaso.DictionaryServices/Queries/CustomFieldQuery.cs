using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;

namespace Palaso.DictionaryServices.Queries
{
	public class CustomFieldQuery : IQuery<LexEntry>
	{
		private string _fieldLabel;

		public CustomFieldQuery(string fieldLabel)
		{
			_fieldLabel = fieldLabel;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();
			fieldsandValuesForRecordTokens.AddRange(GetResultsForEntryLevel(entryToQuery));
			fieldsandValuesForRecordTokens.AddRange(GetResultsForSenseLevel(entryToQuery));
			fieldsandValuesForRecordTokens.AddRange(GetResultsForExampleSentenceLevel(entryToQuery));
			return fieldsandValuesForRecordTokens;
		}

		private List<IDictionary<string, object>> GetResultsForEntryLevel(LexEntry entryToQuery)
		{
			var fieldsandValuesForRecordTokens = GetFieldsandValuesFromPropertiesOfPalasoDataObject(entryToQuery);
			foreach (IDictionary<string, object> results in fieldsandValuesForRecordTokens)
			{
				results.Add("Entry", entryToQuery.Guid);
			}
			return fieldsandValuesForRecordTokens;
		}

		private IEnumerable<IDictionary<string, object>> GetResultsForSenseLevel(LexEntry entryToQuery)
		{
			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();
			foreach (LexSense sense in entryToQuery.Senses)
			{
				fieldsandValuesForRecordTokens = GetFieldsandValuesFromPropertiesOfPalasoDataObject(sense);
				foreach (IDictionary<string, object> results in fieldsandValuesForRecordTokens)
				{
					results.Add("Entry", entryToQuery.Guid);
					results.Add("Sense", sense.Guid);
				}
			}
			return fieldsandValuesForRecordTokens;
		}

		private List<IDictionary<string, object>> GetFieldsandValuesFromPropertiesOfPalasoDataObject(PalasoDataObject sense)
		{
			var fieldsandValuesFromProperties = new List<IDictionary<string, object>>();
			foreach (KeyValuePair<string, object> pair in sense.Properties)
			{
				if (pair.Key == _fieldLabel)
				{
					if (pair.Value.GetType() == typeof(OptionRefCollection))
					{
						var semanticDomains = (OptionRefCollection) pair.Value;
						foreach (string semanticDomain in semanticDomains.Keys)
						{
							IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
							string domain = semanticDomain;
							if (String.IsNullOrEmpty(semanticDomain))
							{
								domain = null;
							}
							if (CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain(fieldsandValuesFromProperties,
																						domain))
							{
								continue; //This is to avoid duplicates
							}
							tokenFieldsAndValues.Add(_fieldLabel, domain);
							fieldsandValuesFromProperties.Add(tokenFieldsAndValues);
						}
					}
				}
			}
			return fieldsandValuesFromProperties;
		}

		private IEnumerable<IDictionary<string, object>> GetResultsForExampleSentenceLevel(LexEntry entryToQuery)
		{
			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();
			foreach (LexSense sense in entryToQuery.Senses)
			{
				foreach (LexExampleSentence exampleSentence in sense.ExampleSentences)
				{
					fieldsandValuesForRecordTokens = GetFieldsandValuesFromPropertiesOfPalasoDataObject(exampleSentence);
					foreach (IDictionary<string, object> results in fieldsandValuesForRecordTokens)
					{
						results.Add("Entry", entryToQuery.Guid);
						results.Add("Sense", exampleSentence.Parent.Guid);
						results.Add("ExampleSentence", exampleSentence.Guid);
					}
				}
			}
			return fieldsandValuesForRecordTokens;
		}

		private bool CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain(IEnumerable<IDictionary<string, object>> fieldsandValuesForRecordTokens, string domain)
		{
			foreach (var tokenInfo in fieldsandValuesForRecordTokens)
			{
				if ((string)tokenInfo[_fieldLabel] == domain)
				{
					return true;
				}
			}
			return false;
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[4];
				sortOrder[0] = new SortDefinition(_fieldLabel, StringComparer.InvariantCulture);
				sortOrder[1] = new SortDefinition("Sense", Comparer<Guid>.Default);
				sortOrder[2] = new SortDefinition("Entry", Comparer<Guid>.Default);
				sortOrder[3] = new SortDefinition("ExampleSentence", Comparer<Guid>.Default);
				return sortOrder;
			}
		}

		public override string UniqueLabel
		{
			get { return _fieldLabel + "Query"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			throw new NotImplementedException();
		}
	}
}
