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

		//This should be a list of unequal values for each type used by the fields of he object under test
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
		//x.Equals(y) && y.Equals(x) where x!= y
		public void Equal_OneFieldDiffers_ReturnsFalse()
		{
			var itemToGetFieldsFrom = CreateNewClonable();
			var fieldInfos = GetAllFields(itemToGetFieldsFrom);

			foreach (var fieldInfo in fieldInfos)
			{
				var item = CreateNewClonable();
				var unequalItem = CreateNewClonable();
				Assert.That(item, Is.EqualTo(unequalItem), "The two items were not equal on creation. You may need to override Equals(object other).");
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
				Assert.AreNotEqual(item, unequalItem, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList property.", fieldName);
				Assert.AreNotEqual(unequalItem, item, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList property.", fieldName);
			}
		}

		[Test]
		//x.Equals(y) && y.Equals(x) where x==y
		public void Equal_ItemsAreEqual_ReturnsTrue()
		{
			var itemToGetFieldsFrom = CreateNewClonable();
			var fieldInfos = GetAllFields(itemToGetFieldsFrom);

			foreach (var fieldInfo in fieldInfos)
			{
				var item = CreateNewClonable();
				var unequalItem = CreateNewClonable();
				Assert.That(item, Is.EqualTo(unequalItem), "The two items were not equal on creation. You may need to override Equals(object other).");
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
				fieldInfo.SetValue(unequalItem, defaultValue.DefaultValue);
				Assert.AreEqual(item, unequalItem, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList property.", fieldName);
				Assert.AreEqual(unequalItem, item, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList property.", fieldName);
			}
		}

		[Test]
		//x.Equals(default) && y.Equals(default) (y/x are left on their default values)
		//though x.Equals(null) is better form, many fields and properties are set to seomthing besides null on construction and can never be null (short of reflection as we do below). So By testing
		//against the original value rather than null, we save ourselves a lot of boilerplate in the Equals method.
		public void Equal_SecondFieldIsNull_ReturnsFalse()
		{
			var itemToGetFieldsFrom = CreateNewClonable();
			var fieldInfos = GetAllFields(itemToGetFieldsFrom);

			foreach (var fieldInfo in fieldInfos)
			{
				var itemWithFieldToChange = CreateNewClonable();
				var itemWithDefaultField = CreateNewClonable();
				Assert.That(itemWithFieldToChange, Is.EqualTo(itemWithDefaultField), "The two items were not equal on creation. You may need to override Equals(object other).");
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
				//This conditional is here in case the DefaultValue is identical to the fields default value.
				//That way developers who inherit this test case don't have to worry about what the fields default value is.
				//It also works around an issue with bool values. If you have two fields that are initialized with different
				//values (i.e. one is true and the other false) this will ensure that the value is chosen which is not equal
				//to the default value and the test therefor has a chance of succeeding.
				if (defaultValue.DefaultValue.Equals(fieldInfo.GetValue(itemWithDefaultField)))
				{
					fieldInfo.SetValue(itemWithFieldToChange, defaultValue.NotEqualDefaultValue);
				}
				else
				{
					fieldInfo.SetValue(itemWithFieldToChange, defaultValue.DefaultValue);
				}
				Assert.AreNotEqual(itemWithFieldToChange, itemWithDefaultField, "Field \"{0}\" is not evaluated in Equals(T other) or the DefaultValue is equal to the fields default value. Please update Equals(T other) or add the field name to the ExceptionList property.", fieldName);
				Assert.AreNotEqual(itemWithDefaultField, itemWithFieldToChange, "Field \"{0}\" is not evaluated in Equals(T other) or the DefaultValue is equal to the fields default value. Please update Equals(T other) or add the field name to the ExceptionList property.", fieldName);
			}
		}

		[Test]
		public void Equals_OtherIsNull_ReturnsFalse()
		{
			var iEquatableUnderTest = CreateNewClonable();
			IEquatable<T> nullObject = null;
			Assert.That(iEquatableUnderTest.Equals((T) nullObject), Is.False);
		}

		[Test]
		public void ObjectEquals_OtherIsNull_ReturnsFalse()
		{
			var iEquatableUnderTest = CreateNewClonable();
			Assert.That(iEquatableUnderTest.Equals(null), Is.False);
		}
	}
}
