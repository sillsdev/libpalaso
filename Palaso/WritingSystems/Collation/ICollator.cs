using System.Collections;
using System.Collections.Generic;
using System.Globalization;
namespace Palaso.WritingSystems.Collation
{
	public interface ICollator : IComparer<string>, IComparer
	{
		SortKey GetSortKey(string source);
	}
}
