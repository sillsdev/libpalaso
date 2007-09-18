using System.Collections.Generic;
using System.Globalization;
namespace Palaso.WritingSystems.Collation
{
	public interface ICollator : IComparer<string>
	{
		SortKey GetSortKey(string source);
	}
}
