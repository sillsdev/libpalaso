using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Text;

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
			foreach (KeyValuePair<string, object> fieldLabelAndFieldPair in sense.Properties)
			{
				if (fieldLabelAndFieldPair.Key == _fieldLabel)
				{
					bool fieldIsAnOptionRefCollection = fieldLabelAndFieldPair.Value.GetType() == typeof(OptionRefCollection);
					bool fieldIsAMultiText = fieldLabelAndFieldPair.Value.GetType() == typeof(MultiText);

					if (fieldIsAnOptionRefCollection)
					{
						OptionRefCollection optionRefs = (OptionRefCollection)fieldLabelAndFieldPair.Value;
						foreach (string optionRef in optionRefs.Keys)
						{
							IDictionary<string, object> newFieldAndValue = new Dictionary<string, object>();
							string domain = optionRef;
							if (String.IsNullOrEmpty(optionRef))
							{
								domain = null;
							}
							//if (CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain(fieldsandValuesFromProperties, domain))
							//{
							//    continue; //This is to avoid duplicates
							//}
							newFieldAndValue.Add(_fieldLabel, domain);
							fieldsandValuesFromProperties.Add(newFieldAndValue);
						}
					}
					else if (fieldIsAMultiText)
					{
						MultiText multiText = (MultiText) fieldLabelAndFieldPair.Value;
						foreach (LanguageForm text in multiText)
						{
							IDictionary<string, object> newFieldsAndValues = new Dictionary<string, object>();
							newFieldsAndValues.Add(_fieldLabel, text.Form);
							newFieldsAndValues.Add("WritingSystem", text.WritingSystemId);
							fieldsandValuesFromProperties.Add(newFieldsAndValues);

						}
					}
					else
					{
						throw new InvalidOperationException(String.Format("The field associated with field label '{0}' is of an unknown type '{1}' and can not be queried. Consider extending the query to support this type of field.", _fieldLabel, fieldLabelAndFieldPair.Value.GetType()));
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
			get { return _fieldLabel + "Query"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			return entryToCheckAgainst[_fieldLabel] == null;
		}
	}
}
