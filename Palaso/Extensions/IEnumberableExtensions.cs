using System.Collections.Generic;

namespace Palaso.Extensions
{
	public static class IEnumberableExtensions
	{
		public static string Concat<T>(this IEnumerable<T> list, string seperator)
		{
			string s = string.Empty;

			foreach (var part in list)
			{
				s += part.ToString()+seperator;
			}
			if (s.Length > 0)
			{
				return s.Substring(0, s.Length - seperator.Length);
			}
			return string.Empty;
		}
	}
}