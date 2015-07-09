using System.Collections.Generic;
using System.Text;
using SIL.Data;

namespace SIL.Tests.Data
{
	/// <summary>
	/// Nunit 2.5 removed DoAssert and made the IAssert stuff obsolete, sigh.  This simulates it
	/// for the sake of our existing tests.
	/// </summary>
	public class Asserter
	{
		public static void Assert(DictionaryContentAsserter<string, object> asserter)
		{
			NUnit.Framework.Assert.IsTrue(asserter.Test(), asserter.Message);
		}

		public static void Assert<V>(DictionaryContentAsserter<string, V> asserter)
		{
			NUnit.Framework.Assert.IsTrue(asserter.Test(), asserter.Message);
		}
	}

	public class DictionaryContentAsserter<K, V>
	{
		private readonly IDictionary<K, V>[] _expectedResult;
		private readonly IDictionary<K, V>[] _actualResult;

		public DictionaryContentAsserter(IDictionary<K, V>[] expectedResult,
										 IEnumerable<IDictionary<K, V>> actualResult)
		{
			_expectedResult = expectedResult;
			_actualResult = ToArray(actualResult);
		}

		public DictionaryContentAsserter(IDictionary<K, V>[] expectedResult,
										 IEnumerable<Dictionary<K, V>> actualResult)
		{
			_expectedResult = expectedResult;
			_actualResult = ToArray(actualResult);
		}

		public DictionaryContentAsserter(IEnumerable<Dictionary<K, V>> expectedResult,
										 IEnumerable<Dictionary<K, V>> actualResult)
		{
			_expectedResult = ToArray(expectedResult);
			_actualResult = ToArray(actualResult);
		}

		public DictionaryContentAsserter(IEnumerable<IDictionary<K, V>> expectedResult,
										 IEnumerable<IDictionary<K, V>> actualResult)
		{
			_expectedResult = ToArray(expectedResult);
			_actualResult = ToArray(actualResult);
		}

		private static T[] ToArray<T>(IEnumerable<T> result)
		{
			return new List<T>(result).ToArray();
		}

		public DictionaryContentAsserter(IDictionary<K, V>[] expectedResult,
										 IDictionary<K, V>[] actualResult)
		{
			_expectedResult = expectedResult;
			_actualResult = actualResult;
		}

		public bool Test()
		{
			if (_expectedResult.Length != _actualResult.Length)
			{
				return false;
			}
			DictionaryEqualityComparer<K, V> comparer = new DictionaryEqualityComparer<K, V>();
			for (int i = 0;i != _expectedResult.Length;++i)
			{
				if (!comparer.Equals(_expectedResult[i], _actualResult[i]))
				{
					return false;
				}
			}
			return true;
		}

		public string Message
		{
			get
			{
				return "Jagged arrays differ.\n" + "Expected:\n" + Write(_expectedResult) + '\n' +
					   "Actual:\n" + Write(_actualResult);
			}
		}

		private static string Write(IEnumerable<IDictionary<K, V>> dicts)
		{
			StringBuilder sb = new StringBuilder();

			foreach (IDictionary<K, V> dict in dicts)
			{
				foreach (KeyValuePair<K, V> pair in dict)
				{
					sb.Append(pair.Key);
					sb.Append(':');
					sb.Append(pair.Value);
					sb.Append(' ');
				}
				sb.Append('\n');
			}
			return sb.ToString();
		}
	}
}