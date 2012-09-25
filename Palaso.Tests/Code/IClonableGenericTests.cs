using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Palaso.Code;

namespace Palaso.Tests.Code
{

	public abstract class IClonableGenericTests<T> where T:IClonableGeneric<T>
	{
		public abstract T CreateNewClonable();

		protected class DefaultValues
		{
			public DefaultValues(object defaultValue, object notEqualDefaultValue)
			{
				if(defaultValue.Equals(notEqualDefaultValue))
				{
					throw new ArgumentException("Default values must not be equal!");
				}
				DefaultValue = defaultValue;
				NotEqualDefaultValue = notEqualDefaultValue;
			}

			public Type TypeOfDefaultValue { get { return DefaultValue.GetType(); } }
			public object DefaultValue { get; private set; }
			public object NotEqualDefaultValue { get; private set; }
		}

		// Put any fields to ignore in this string surrounded by "|"
		public abstract string ExceptionList { get; }

		protected abstract List<DefaultValues> DefaultValuesForTypes { get; }

		[Test]
		public void CloneCopiesAllNeededMembers()
		{
			var clonable = CreateNewClonable();
			var fieldInfos = GetAllFields(clonable);

			foreach (var fieldInfo in fieldInfos)
			{
				var fieldName = fieldInfo.Name;
				if (fieldInfo.Name.Contains("<"))
				{
					var splitResult = fieldInfo.Name.Split(new[] { '<', '>' });
					fieldName = splitResult[1];
				}
				if (ExceptionList.Contains("|" + fieldName + "|"))
				{
					continue;
				}
				object defaultValue = null;
				try
				{
					defaultValue = DefaultValuesForTypes.Single(dv => dv.TypeOfDefaultValue == fieldInfo.FieldType).DefaultValue;
				}
				catch(InvalidOperationException e)
				{
					Assert.Fail("Unhandled field type - please update the test to handle type {0}. The field that uses this type is {1}.", fieldInfo.FieldType.Name, fieldName);
				}

				fieldInfo.SetValue(clonable, defaultValue);

				var theClone = Convert.ChangeType(clonable.Clone(), clonable.GetType());
				if (fieldInfo.GetValue(clonable).GetType() != typeof(string))  //strings are special in .net so we won't worry about checking them here.
				{
					Assert.AreNotSame(fieldInfo.GetValue(clonable), fieldInfo.GetValue(theClone),
									  "The field {0} refers to the same object, it was not copied.", fieldName);
				}
				Assert.AreEqual(defaultValue, fieldInfo.GetValue(theClone), "Field {0} not copied on Clone()", fieldName);
			}
		}

		private static IEnumerable<FieldInfo> GetAllFields(T clonable)
		{
			var type = clonable.GetType();
			IEnumerable<FieldInfo> fieldInfos = new List<FieldInfo>();
			//here we are traversing up the inheritance tree to get all of the fields on our base types
			while (type != null)
			{
				fieldInfos =
					fieldInfos.Concat(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				type = type.BaseType;
			}
			return fieldInfos;
		}

		[Test]
		public void Equal_AllMembersAreConsidered()
		{
			var item = CreateNewClonable();
			var unequalItem = CreateNewClonable();
			Assert.That(item.Equals(unequalItem));
			var fieldInfos = GetAllFields(item);

			foreach (var fieldInfo in fieldInfos)
			{
				var fieldName = fieldInfo.Name;
				if (fieldInfo.Name.Contains("<"))
				{
					var splitResult = fieldInfo.Name.Split(new[] {'<', '>'});
					fieldName = splitResult[1];
				}
				if (ExceptionList.Contains("|" + fieldName + "|"))
				{
					continue;
				}
				DefaultValues defaultValue = null;
				try
				{
					defaultValue = DefaultValuesForTypes.Single(dv => dv.TypeOfDefaultValue == fieldInfo.FieldType);
				}
				catch (InvalidOperationException e)
				{
					Assert.Fail(
						"Unhandled field type - please update the test to handle type {0}. The field that uses this type is {1}.",
						fieldInfo.FieldType.Name, fieldName);
				}
				fieldInfo.SetValue(item, defaultValue.DefaultValue);
				fieldInfo.SetValue(unequalItem, defaultValue.NotEqualDefaultValue);
				Assert.AreNotEqual(item, unequalItem, "Field \"{0}\" makes no difference to Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList property.", fieldName);
			}
		}
	}
}
