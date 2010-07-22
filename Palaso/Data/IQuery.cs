using System;
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

		public static IQuery<T> operator &(IQuery<T> thisQuery, IQuery<T> thatQuery)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					List<IDictionary<string, object>> combinedResults = new List<IDictionary<string, object>>();
					IEnumerable<IDictionary<string, object>> thisQueryResults = thisQuery.GetResults(t);
					IEnumerable<IDictionary<string, object>> thatQueryResults = thatQuery.GetResults(t);
					if (thisQueryResults.Count() < thatQueryResults.Count())
					{
						foreach (IDictionary<string, object> thatQueryResult in thatQueryResults)
						{
							foreach (IDictionary<string, object> thisQueryResult in thisQueryResults)
							{
								Dictionary<string,object> newDictionary = new Dictionary<string, object>();
								foreach (KeyValuePair<string, object> labelValuePair in thisQueryResult.Concat(thatQueryResult))
								{
									newDictionary.Add(labelValuePair.Key, labelValuePair.Value);
									Console.Write("{0},{1}:",labelValuePair.Key, labelValuePair.Value);
								}
								Console.WriteLine();
								combinedResults.Add(newDictionary);
							}
						}
					}
					else
					{
						foreach (IDictionary<string, object> thisQueryResult in thisQueryResults)
						{
							foreach (IDictionary<string, object> thatQueryResult in thatQueryResults)
							{
								Dictionary<string, object> newDictionary = new Dictionary<string, object>();
								foreach (KeyValuePair<string, object> labelValuePair in thisQueryResult.Concat(thatQueryResult))
								{
									newDictionary.Add(labelValuePair.Key, labelValuePair.Value);
								}
								combinedResults.Add(newDictionary);
							}
						}
					}
					return combinedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(thisQuery.SortDefinitions);
			newSortDefinitions.AddRange(thatQuery.SortDefinitions);

			string newUniqueLabel = thisQuery.UniqueLabel + " & " + thatQuery.UniqueLabel;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel);
		}
	}
}
