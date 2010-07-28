using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Data
{
	public abstract class IQuery<T> where T : class, new()
	{
		abstract public IEnumerable<IDictionary<string, object>> GetResults(T item);
		abstract public IEnumerable<SortDefinition> SortDefinitions { get; }
		abstract public string UniqueLabel { get; }

		public IQuery<T> JoinInner(IQuery<T> otherQuery)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					List<IDictionary<string, object>> combinedResults = new List<IDictionary<string, object>>();
					IEnumerable<IDictionary<string, object>> thisQueryResults = this.GetResults(t);
					IEnumerable<IDictionary<string, object>> thatQueryResults = otherQuery.GetResults(t);
					foreach (IDictionary<string, object> thatQueryResult in thatQueryResults)
					{
						foreach (IDictionary<string, object> thisQueryResult in thisQueryResults)
						{
							Dictionary<string,object> newDictionary = new Dictionary<string, object>();
							foreach (KeyValuePair<string, object> labelValuePair in thisQueryResult.Concat(thatQueryResult))
							{
								newDictionary.Add(labelValuePair.Key, labelValuePair.Value);
							}
							combinedResults.Add(newDictionary);
						}
					}
					return combinedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);
			newSortDefinitions.AddRange(otherQuery.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".JoinInner." + otherQuery.UniqueLabel;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel);
		}

		public IQuery<T> Merge(IQuery<T> otherQuery, Dictionary<string, string> keyMap)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					List<IDictionary<string, object>> combinedResults = new List<IDictionary<string, object>>();
					combinedResults.AddRange(this.GetResults(t));
					if (combinedResults.Count != 0)
					{
						CheckThatKeyMapValuesCorrespondToFieldLabelsOfFirstQuery(combinedResults[0], keyMap);
					}
					foreach (IDictionary<string, object> dictionary in otherQuery.GetResults(t))
					{
						Dictionary<string, object> dictionaryWithRenamedKeys = RenameKeysInDictionary(keyMap, dictionary);
						combinedResults.Add(dictionaryWithRenamedKeys);
					}
					return combinedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);
			newSortDefinitions.AddRange(otherQuery.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".Merge." + otherQuery.UniqueLabel;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel);
		}

		private Dictionary<string, object> RenameKeysInDictionary(Dictionary<string, string> keyMap, IDictionary<string, object> dictionary)
		{
			Dictionary<string, object> dictionaryWithRenamedKeys = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> pair in dictionary)
			{
				try
				{
					dictionaryWithRenamedKeys.Add(keyMap[pair.Key], pair.Value);
				}
				catch(KeyNotFoundException)
				{
					throw new InvalidOperationException(String.Format("The results produced by the second query do not contain a field labeled {0}.", pair.Key));
				}
			}
			return dictionaryWithRenamedKeys;
		}

		private void CheckThatKeyMapValuesCorrespondToFieldLabelsOfFirstQuery(IDictionary<string, object> sampleResult, Dictionary<string, string> keyMap)
		{
			foreach (string value in keyMap.Values)
			{
				if (!sampleResult.Keys.Contains(value))
				{
					throw new InvalidOperationException(String.Format("The results produced by the first query do not contain a field labeled {0}.", value));
				}
			}
		}
	}
}
