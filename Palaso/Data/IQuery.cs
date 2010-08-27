using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Data
{
	public abstract class IQuery<T> where T : class, new()
	{
		List<Predicate<string>> _filters = new List<Predicate<string>>();
		abstract public IEnumerable<IDictionary<string, object>> GetResults(T item);
		abstract public IEnumerable<SortDefinition> SortDefinitions { get; }
		abstract public string UniqueLabel { get; }
		abstract public bool IsUnpopulated(IDictionary<string, object> resultToCheck);
		//abstract public IEnumerable<string> FieldLabels{ get;}

		public void AddFilter(Predicate<string> filter)
		{
			_filters.Add(filter);
		}

		public void ClearFilters()
		{
			_filters.Clear();
		}

		public IQuery<T> JoinInner(IQuery<T> otherQuery)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
				{
					List<IDictionary<string,object>> joinedResults = new List<IDictionary<string, object>>();

					foreach (IDictionary<string, object> thisResult in GetResults(t))
					{
						foreach (IDictionary<string, object> otherResult in otherQuery.GetResults(t))
						{
							//it might be faster here to just try and join them and just swallow any throws. Profiling time! --TA
							if(ResultsCanBeJoined(thisResult, otherResult))
							{
								joinedResults.Add(JoinResults(thisResult, otherResult));
							}
						}
					}
					return joinedResults;
				};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(SortDefinitions);
			newSortDefinitions.AddRange(otherQuery.SortDefinitions);

			string newUniqueLabel = this.UniqueLabel + ".JoinInner." + otherQuery.UniqueLabel;

			Predicate<IDictionary<string, object>> newIsUnpopulated =
				delegate(IDictionary<string, object> result)
					{
						return IsUnpopulated(result) & otherQuery.IsUnpopulated(result);
					};

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel, newIsUnpopulated);
		}

		private IDictionary<string, object> JoinResults(IDictionary<string, object> thisResult, IDictionary<string, object> otherResult)
		{
			Dictionary<string,object> joinedResults = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> fieldLabelAndValue in thisResult)
			{
				joinedResults.Add(fieldLabelAndValue.Key,fieldLabelAndValue.Value);
			}
			foreach (KeyValuePair<string, object> fieldLabelAndValue in otherResult)
			{
				if(!thisResult.ContainsKey(fieldLabelAndValue.Key))
				{
					joinedResults.Add(fieldLabelAndValue.Key, fieldLabelAndValue.Value);
				}
			}
			return joinedResults;
		}

		private bool ResultsCanBeJoined(IDictionary<string, object> thisResult, IDictionary<string, object> otherResult)
		{
			bool joinable = true;
			foreach (KeyValuePair<string, object> fieldLabelAndValue in thisResult)
			{
				bool bothResultsContainKey = otherResult.ContainsKey(fieldLabelAndValue.Key);
				if(bothResultsContainKey)
				{
					IComparer comparerForField = null;
					foreach (SortDefinition sortDefinition in SortDefinitions)
					{
						if(sortDefinition.Field == fieldLabelAndValue.Key)
						{
							comparerForField = sortDefinition.Comparer;
						}
					}
					bool valuesAreNotIdentical = comparerForField.Compare(otherResult[fieldLabelAndValue.Key], fieldLabelAndValue.Value) != 0;
					if (valuesAreNotIdentical)
					{
						joinable = false;
					}
					break;
				}
			}
			return joinable;
		}

		public IQuery<T> JoinInner(IEnumerable<IQuery<T>> otherQueries)
		{
			IQuery<T> joinedQueries = this;
			foreach (IQuery<T> query in otherQueries)
			{
				joinedQueries = joinedQueries.JoinInner(query);
			}
			return joinedQueries;
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
