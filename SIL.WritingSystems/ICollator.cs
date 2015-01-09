using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SIL.WritingSystems
{
	public interface ICollator : IComparer<string>, IComparer
	{
		SortKey GetSortKey(string source);
	}
}
