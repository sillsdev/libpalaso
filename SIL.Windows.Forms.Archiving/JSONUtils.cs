using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SIL.Windows.Forms.Archiving
{
	public class JSONUtils
	{
		/// ------------------------------------------------------------------------------------
		public static string MakeKeyValuePair(string key, string value)
		{
			return MakeKeyValuePair(key, value, false);
		}

		/// ------------------------------------------------------------------------------------
		public static string MakeKeyValuePair(string key, string value, bool bracketValue)
		{
			if (key == null || value == null || key + value == string.Empty)
				return null;

			var fmt = (bracketValue ? "\"{0}\":[\"{1}\"]" : "\"{0}\":\"{1}\"");
			return string.Format(fmt, key, value);
		}

		/// ------------------------------------------------------------------------------------
		public static string MakeBracketedListFromValues(string key, IEnumerable<string> values)
		{
			if (string.IsNullOrEmpty(key) ||  values == null)
				return null;

			var valueList = values.ToArray();
			if (valueList.Length == 0)
				return null;

			var bldr = new StringBuilder();
			foreach (var value in valueList)
				bldr.AppendFormat("\"{0}\",", value);

			// Get rid of last comma.
			bldr.Length--;

			return string.Format("\"{0}\":[{1}]", key, bldr);
		}

		/// ------------------------------------------------------------------------------------
		public static string MakeArrayFromValues(string key, IEnumerable<string> values)
		{
			var bldr = new StringBuilder();
			int i = 0;
			foreach (var val in values)
				bldr.AppendFormat("\"{0}\":{{{1}}},", i++, val);

			// Get rid of last comma.
			bldr.Length--;

			return string.Format("\"{0}\":{{{1}}}", key, bldr);
		}

		/// ------------------------------------------------------------------------------------
		public static string EncodeData(string data)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
		}

		/// ------------------------------------------------------------------------------------
		public static string DecodeData(string data)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(data));
		}
	}
}
