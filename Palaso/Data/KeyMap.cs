using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Data
{
	public class KeyMap:Dictionary<string, string>
	{
		public Dictionary<string, object> Remap(IDictionary<string, object> dictionary)
		{
			Dictionary<string, object> dictionaryWithRenamedKeys = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> pair in dictionary)
			{
				bool KeyExistsInKeyMap = ContainsKey(pair.Key);
				if (KeyExistsInKeyMap)
				{
					dictionaryWithRenamedKeys.Add(this[pair.Key], pair.Value);
				}
				else
				{
					dictionaryWithRenamedKeys.Add(pair.Key, pair.Value);
				}
			}
			return dictionaryWithRenamedKeys;
		}

		public static string EntryGuidFieldLabel
		{
			get { return "EntryGUID"; }
		}

		public static string SenseGuidFieldLabel
		{
			get { return "SenseGUID"; }
		}
	}
}
