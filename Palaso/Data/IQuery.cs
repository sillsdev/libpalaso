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
		public abstract bool IsUnpopulated(IDictionary<string, object> resultToCheck);

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
								if (!newDictionary.ContainsKey(labelValuePair.Key))
								{
									newDictionary.Add(labelValuePair.Key, labelValuePair.Value);
								}
								else if(newDictionary[labelValuePair.Key] == labelValuePair.Value)
								{
									//do nothing
								}
								else
								{
									throw new InvalidOperationException(String.Format("The key '{0}' occurs in both results and the content is not identical.", labelValuePair.Key));
								}
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

			Predicate<IDictionary<string, object>> newIsUnpopulated =
				delegate(IDictionary<string, object> result)
					{
						return IsUnpopulated(result) & otherQuery.IsUnpopulated(result);
					};

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		public IQuery<T> Merge(IQuery<T> otherQuery)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					List<IDictionary<string, object>> combinedResults = new List<IDictionary<string, object>>();
					combinedResults.AddRange(GetResults(t));
					combinedResults.AddRange(otherQuery.GetResults(t));
					return combinedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".Merge." + otherQuery.UniqueLabel;

			Predicate<IDictionary<string, object>> newIsUnpopulated = IsUnpopulated;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		public IQuery<T> RemapKeys(KeyMap keyMap)
		{

			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					List<IDictionary<string, object>> remappedResults = new List<IDictionary<string, object>>();

					foreach (IDictionary<string, object> dictionary in GetResults(t))
					{
						Dictionary<string, object> dictionaryWithRenamedKeys = keyMap.Remap(dictionary);
						remappedResults.Add(dictionaryWithRenamedKeys);
					}
					return remappedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".Remap";

			Predicate<IDictionary<string, object>> newIsUnpopulated = IsUnpopulated;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		public IQuery<T> GetAlternative(IQuery<T> otherQuery)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					if (NoPopulatedResultsReturnedByFirstQuery(t))
					{

						return otherQuery.GetResults(t);
					}
					return GetResults(t);
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".GetAlternative." + otherQuery.UniqueLabel;

			Predicate<IDictionary<string, object>> newIsUnpopulated =
				delegate(IDictionary<string, object> result)
				{
					return IsUnpopulated(result) & otherQuery.IsUnpopulated(result);
				};

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		private bool NoPopulatedResultsReturnedByFirstQuery(T t)
		{
			bool noPopulatedResultsReturned = true;
			foreach (IDictionary<string, object> result in GetResults(t))
			{
				if (!IsUnpopulated(result))
				{
					noPopulatedResultsReturned = false;
					break;
				}
			}
			return noPopulatedResultsReturned;
		}

		public IQuery<T> StripAllUnpopulatedEntries()
		{
			Predicate<IDictionary<string, object>> unpopulatedPredicate = IsUnpopulated;
			return Strip(unpopulatedPredicate, "Unpopulated");
		}

		public IQuery<T> Strip(Predicate<IDictionary<string, object>> condition, string label)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					ResultsComparer resultsComparer = new ResultsComparer(this.SortDefinitions);
					SortedListAllowsDuplicates<IDictionary<string, object>> strippedResults
						= new SortedListAllowsDuplicates<IDictionary<string, object>>(resultsComparer);
					foreach (IDictionary<string, object> result in GetResults(t))
					{
						if (!condition(result))
						{
							strippedResults.Add(result);
						}
					}

					return strippedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".Strip" + label;

			Predicate<IDictionary<string, object>> newIsUnpopulated = IsUnpopulated;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		public IQuery<T> StripDuplicates()
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					ResultsComparer resultsComparer = new ResultsComparer(this.SortDefinitions);
					SortedListAllowsDuplicates<IDictionary<string, object>> strippedResults
						= new SortedListAllowsDuplicates<IDictionary<string, object>>(resultsComparer);
					foreach (IDictionary<string, object> result in GetResults(t))
					{
						if(!strippedResults.Contains(result))
						{
							strippedResults.Add(result);
						}
					}

					return strippedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".StripDuplicates";

			Predicate<IDictionary<string, object>> newIsUnpopulated = IsUnpopulated;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		public IQuery<T> GetUnpopulatedRecordsOnly()
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					throw new NotImplementedException();
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(this.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".StripDuplicates";

			Predicate<IDictionary<string, object>> newIsUnpopulated = IsUnpopulated;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		private class ResultsComparer:IComparer<IDictionary<string, object>>
		{
			private IEnumerable<SortDefinition> _sortDefinitions;

			public ResultsComparer(IEnumerable<SortDefinition> sortDefinitions)
			{
				_sortDefinitions = sortDefinitions;
			}

			public int Compare(IDictionary<string, object> x, IDictionary<string, object> y)
			{
				int relation = 0;
				foreach (SortDefinition sortDefinition in _sortDefinitions)
				{
					relation = sortDefinition.Comparer.Compare(x[sortDefinition.Field], y[sortDefinition.Field]);
					if(relation != 0) break;
				}
				return relation;
			}
		}
	}
}
