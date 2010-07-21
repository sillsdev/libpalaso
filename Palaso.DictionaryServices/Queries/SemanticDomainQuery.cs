using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Lift.Options;

namespace Palaso.DictionaryServices.Queries
{
	class SemanticDomainQuery:IQuery<LexEntry>
	{
		public IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();
			foreach (LexSense sense in entryToQuery.Senses)
			{
				foreach (KeyValuePair<string, object> pair in sense.Properties)
				{
					if (pair.Key == LexSense.WellKnownProperties.SemanticDomainDdp4)
					{
						var semanticDomains = (OptionRefCollection)pair.Value;
						foreach (string semanticDomain in semanticDomains.Keys)
						{
							IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
							string domain = semanticDomain;
							if (String.IsNullOrEmpty(semanticDomain))
							{
								domain = null;
							}
							if (CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain(fieldsandValuesForRecordTokens, domain))
							{
								continue; //This is to avoid duplicates
							}
							tokenFieldsAndValues.Add("SemanticDomain", domain);
							fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
						}
					}
				}
			}
			return fieldsandValuesForRecordTokens;
		}

		private static bool CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain(IEnumerable<IDictionary<string, object>> fieldsandValuesForRecordTokens, string domain)
		{
			foreach (var tokenInfo in fieldsandValuesForRecordTokens)
			{
				if ((string)tokenInfo["SemanticDomain"] == domain)
				{
					return true;
				}
			}
			return false;
		}

		public SortDefinition[] SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[2];
				sortOrder[0] = new SortDefinition("SemanticDomain", StringComparer.InvariantCulture);
				sortOrder[1] = new SortDefinition("Sense", Comparer<int>.Default);
				return sortOrder;
			}
		}

		public string Label
		{
			get { return "SemanticDomainQuery"; }
		}
	}
}
