// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Icu.Collation;
using SIL.PlatformUtilities;

namespace SIL.WritingSystems
{
	public class IcuRulesCollator : ICollator
	{
		private readonly RuleBasedCollator _collator;

		public IcuRulesCollator(string rules)
		{
			try
			{
				_collator = new RuleBasedCollator(LdmlCollationParser.ReplaceUnicodeEscapesForIcu(rules));
			}
			catch (DllNotFoundException e)
			{
				throw new DllNotFoundException("IcuRulesCollator requires the ICU dlls to be present", e);
			}
		}

		public static bool ValidateSortRules(string rules, out string message)
		{
			Collator.CollationRuleErrorInfo errorInfo = Collator.CheckRules(LdmlCollationParser.ReplaceUnicodeEscapesForIcu(rules));
			if (errorInfo != null)
			{
				message = string.Format("Invalid ICU rules (Line: {0}, Column: {1}).", errorInfo.Line, errorInfo.Offset);
				return false;
			}
			message = null;
			return true;
		}

		public SortKey GetSortKey(string source)
		{
#if NET461
			return _collator.GetSortKey(source);
#elif NETSTANDARD2_0
			Icu.SortKey icuSortKey = _collator.GetSortKey(source);
			SortKey sortKey = CultureInfo.InvariantCulture.CompareInfo.GetSortKey(string.Empty, CompareOptions.None);
			string keyDataFieldName, origStringFieldName;
			if (Platform.IsDotNetFramework)
			{
				keyDataFieldName = "m_KeyData";
				origStringFieldName = "m_String";
			}
			else if (Platform.IsDotNetCore)
			{
				keyDataFieldName = "_keyData";
				origStringFieldName = "_string";
			}
			else if (Platform.IsMono)
			{
				keyDataFieldName = "key";
				origStringFieldName = "source";
			}
			else
			{
				throw new PlatformNotSupportedException();
			}

			SetInternalFieldForPublicProperty(sortKey, "SortKey.KeyData", keyDataFieldName, icuSortKey.KeyData);
			SetInternalFieldForPublicProperty(sortKey, "SortKey.OriginalString", origStringFieldName,
				icuSortKey.OriginalString);
			return sortKey;
#endif
		}

		///<summary>Compares two strings and returns a value indicating whether one is less than,
		/// equal to, or greater than the other.</summary>
		///
		///<returns>Less than zero when string1 is less than string2.
		///  Zero when string1 equals string2.
		///  Greater than zero when string1 is greater than string2.
		///</returns>
		///
		///<param name="string1">The first string to compare.</param>
		///<param name="string2">The second object to compare.</param>
		public int Compare(string string1, string string2)
		{
			return _collator.Compare(string1, string2);
		}

		public int Compare(object x, object y)
		{
			return Compare((string) x, (string) y);
		}

#if NETSTANDARD2_0
		private static void SetInternalFieldForPublicProperty(object instance, string propertyName, string fieldName,
			object value)
		{
			Type type = instance.GetType();

			FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

			Debug.Assert(fieldInfo != null,
				"Unsupported runtime",
				"Could not figure out an internal field for" + propertyName);

			if (fieldInfo == null)
				throw new PlatformNotSupportedException();

			fieldInfo.SetValue(instance, value);
		}
#endif
	}
}
