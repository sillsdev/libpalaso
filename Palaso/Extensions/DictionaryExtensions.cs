using System;
using System.Text;

namespace Palaso.Extensions
{
	public static class DictionaryExtensions
	{
		public static B GetOrCreate<A, B>(this System.Collections.Generic.Dictionary<A, B> dictionary, A key)
			where B: new()
		{
			B target;
			if(!dictionary.TryGetValue(key, out target))
			{
				target = new B();
				dictionary.Add(key, target);
			}
			return target;
		}
	}
}