using System;
using System.Collections.Generic;

namespace Palaso.Data
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
				result = definition.Comparer.Compare(GetFieldValue(x, definition),
													 GetFieldValue(y, definition));
				if (result != 0)
				{
					return result;
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
