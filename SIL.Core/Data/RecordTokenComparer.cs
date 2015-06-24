using System;
using System.Collections.Generic;

namespace SIL.Data
{
	public sealed class RecordTokenComparer<T>: IComparer<RecordToken<T>> where T : class, new()
	{
		private readonly IEnumerable<SortDefinition> _sortDefinitions;
		public RecordTokenComparer(params SortDefinition[] sortDefinitions)
		{
			if (sortDefinitions == null)
			{
				throw new ArgumentNullException("sortDefinitions");
			}
			if (sortDefinitions.GetEnumerator().MoveNext() == false)
			{
				throw new ArgumentException("sortDefinitions cannot be an empty array");
			}
			_sortDefinitions = sortDefinitions;
		}

		#region IComparer<RecordToken> Members

		public int Compare(RecordToken<T> x, RecordToken<T> y)
		{
			int result = 0;
			foreach (SortDefinition definition in _sortDefinitions)
			{
				var xValue = GetFieldValue(x, definition);
				var yValue = GetFieldValue(y, definition);
				result = definition.Comparer.Compare(xValue, yValue);
				if (result != 0)
				{
					return result;
				}
				if (xValue != null && xValue.GetHashCode() != yValue.GetHashCode())
				{
					// bugfix WS-33997.  Khmer (invariant culture) strings when compared return "same",
					// when in fact they are different strings.  In this case, use an ordinal compare.
					return string.CompareOrdinal(xValue.ToString(), yValue.ToString());
				}
			}
			result = x.Id.CompareTo(y.Id);
			return result;
		}

		private static object GetFieldValue(RecordToken<T> token, SortDefinition definition)
		{
			object value;
			if (token == null)
			{
				value = null;
			}
			else if (definition.Field == "RepositoryId")
			{
				value = token.Id;
			}
			else
			{
				if (!token.TryGetValue(definition.Field, out value))
				{
					value = null;
				}
			}
			return value;
		}

		#endregion
	} ;
}
