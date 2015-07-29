using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Palaso.Code;

namespace Palaso.Tests.Code
{
	/// <summary>
	/// Generic test that a cloneable object clones all required fields and uses them in equality testing.
	/// The class we are testing (T) must implement IClonableGeneric<T>.
	/// However, the Clone() method may not be defined to return type T. For example, DefaultKeyboardDefinition
	/// has a Clone() method in its interface, which needs to return IKeyboardDefinition.
	/// Thus, we have two Type parameters, one for the type that Clone() returns.
	/// </summary>
	/// <typeparam name="T">Implementation class</typeparam>
	/// <typeparam name="TClone">Type that Clone() returns</typeparam>
	public abstract class IClonableGenericTests<T, TClone> where T:IClonableGeneric<TClone>
	{
		public abstract T CreateNewClonable();

		protected class ValuesToSet
		{
			public ValuesToSet(object valueToSet, object notEqualValueToSet)
			{
				if(valueToSet.Equals(notEqualValueToSet))
				{
					throw new ArgumentException("Values to set must not be equal!");
				}
				ValueToSet = valueToSet;
				NotEqualValueToSet = notEqualValueToSet;

				if (valueToSet.Equals(DefaultValueForValueToSet))
				{
					// the valueToSet should be different from the default value so that we can
					// test that the members actually get copied. Otherwise we don't know if the
					// member got copied or if the test passed because we just happened to get the
					// default value.
					throw new ArgumentException("Value to set should not be the default value", "valueToSet");
				}
			}

			public virtual Type TypeOfValueToSet { get { return ValueToSet.GetType(); } }
			public object ValueToSet { get; private set; }
			public object NotEqualValueToSet { get; private set; }

			private object DefaultValueForValueToSet
			{
				get { return TypeOfValueToSet.IsValueType ? Activator.CreateInstance(TypeOfValueToSet) : null; }
			}
		}

		/// <summary>
		/// This class is useful where the type of the property is an interface or abstract class.
		/// T should be the signature of the cloneable property.
		/// Unlike ValuesToSet, this class allows the test instances to belong to a subclass.
		/// </summary>
		/// <typeparam name="TT"></typeparam>
		protected class SubclassValuesToSet<TT> : ValuesToSet
		{
			public SubclassValuesToSet(TT defaultValue, TT notEqualDefaultValue) : base(defaultValue, notEqualDefaultValue)
			{
			}

			public override Type TypeOfValueToSet
			{
				get
				{
					return typeof(TT);
				}
			}
		}

		/// <summary>
		/// List of fields to ignore, surrounded by "|". The tests won't check that these fields
		/// get cloned nor that the Equals() method considers them.
		/// </summary>
		/// <example><code>
		/// public override string ExceptionList { get { return "|Type|Name|IsAvailable|"; } }
		/// </code></example>
		public abstract string ExceptionList { get; }

		/// <summary>
		/// List of fields to ignore in the Equals() method, surrounded by "|". The tests won't
		/// check that these fields are considered in the Equals() method. This list is in
		/// addition to the ExceptionList.
		/// </summary>
		public virtual string EqualsExceptionList { get { return string.Empty; } }

		/// <summary>
		/// List of unequal values for each type used by the fields of the object under test
		/// </summary>
		/// <example><code>
		///	protected override List&lt;ValuesToSet&gt; DefaultValuesForTypes
		///	{
		///		get
		///		{
		///			return new List&lt;ValuesToSet&gt;
		///			{
		///				new ValuesToSet(false, true),
		///				new ValuesToSet("to be", "!(to be)"),
		///				new SubclassValuesToSet&lt;IClass&gt;(
		///					new MyClass("one thing", 1),
		///					new MyClass("other thing", 2))
		///			};
		///		}
		/// }
		/// </code></example>
		protected abstract List<ValuesToSet> DefaultValuesForTypes { get; }

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
					defaultValue = DefaultValuesForTypes.Single(dv => dv.TypeOfValueToSet == fieldInfo.FieldType).ValueToSet;
				}
				catch(InvalidOperationException e)
				{
					Assert.Fail("Unhandled field type - please update the test to handle type \"{0}\". The field that uses this type is \"{1}\".", fieldInfo.FieldType.Name, fieldName);
				}

				fieldInfo.SetValue(clonable, defaultValue);

				var theClone = Convert.ChangeType(clonable.Clone(), clonable.GetType());
				if (fieldInfo.GetValue(clonable).GetType() != typeof(string))  //strings are special in .net so we won't worry about checking them here.
				{
					Assert.AreNotSame(fieldInfo.GetValue(clonable), fieldInfo.GetValue(theClone),
									  "The field \"{0}\" refers to the same object, it was not copied.", fieldName);
				}
				Assert.AreEqual(defaultValue, fieldInfo.GetValue(theClone), "Field \"{0}\" not copied on Clone()", fieldName);
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
				if (ExceptionList.Contains("|" + fieldName + "|") || EqualsExceptionList.Contains("|" + fieldName + "|"))
				{
					continue;
				}
				ValuesToSet valueToSet = null;
				try
				{
					valueToSet = DefaultValuesForTypes.Single(dv => dv.TypeOfValueToSet == fieldInfo.FieldType);
				}
				catch (InvalidOperationException)
				{
					Assert.Fail(
						"Unhandled field type - please update the test to handle type \"{0}\". The field that uses this type is \"{1}\".",
						fieldInfo.FieldType.Name, fieldName);
				}
				fieldInfo.SetValue(item, valueToSet.ValueToSet);
				fieldInfo.SetValue(unequalItem, valueToSet.NotEqualValueToSet);
				Assert.AreNotEqual(item, unequalItem, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList or EqualsExceptionList property.", fieldName);
				Assert.AreNotEqual(unequalItem, item, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList or EqualsExceptionList property.", fieldName);
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
				if (ExceptionList.Contains("|" + fieldName + "|") || EqualsExceptionList.Contains("|" + fieldName + "|"))
				{
					continue;
				}
				ValuesToSet valueToSet = null;
				try
				{
					valueToSet = DefaultValuesForTypes.Single(dv => dv.TypeOfValueToSet == fieldInfo.FieldType);
				}
				catch (InvalidOperationException)
				{
					Assert.Fail(
						"Unhandled field type - please update the test to handle type \"{0}\". The field that uses this type is \"{1}\".",
						fieldInfo.FieldType.Name, fieldName);
				}
				fieldInfo.SetValue(item, valueToSet.ValueToSet);
				fieldInfo.SetValue(unequalItem, valueToSet.ValueToSet);
				Assert.AreEqual(item, unequalItem, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList or EqualsExceptionList property.", fieldName);
				Assert.AreEqual(unequalItem, item, "Field \"{0}\" is not evaluated in Equals(T other). Please update Equals(T other) or add the field name to the ExceptionList or EqualsExceptionList property.", fieldName);
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
				if (ExceptionList.Contains("|" + fieldName + "|") || EqualsExceptionList.Contains("|" + fieldName + "|"))
				{
					continue;
				}
				ValuesToSet valueToSet = null;
				try
				{
					valueToSet = DefaultValuesForTypes.Single(dv => dv.TypeOfValueToSet == fieldInfo.FieldType);
				}
				catch (InvalidOperationException)
				{
					Assert.Fail(
						"Unhandled field type - please update the test to handle type \"{0}\". The field that uses this type is \"{1}\".",
						fieldInfo.FieldType.Name, fieldName);
				}
				//This conditional is here in case the ValueToSet is identical to the fields default value.
				//That way developers who inherit this test case don't have to worry about what the fields default value is.
				//It also works around an issue with bool values. If you have two fields that are initialized with different
				//values (i.e. one is true and the other false) this will ensure that the value is chosen which is not equal
				//to the default value and the test therefor has a chance of succeeding.
				if (valueToSet.ValueToSet.Equals(fieldInfo.GetValue(itemWithDefaultField)))
				{
					fieldInfo.SetValue(itemWithFieldToChange, valueToSet.NotEqualValueToSet);
				}
				else
				{
					fieldInfo.SetValue(itemWithFieldToChange, valueToSet.ValueToSet);
				}
				Assert.AreNotEqual(itemWithFieldToChange, itemWithDefaultField, "Field \"{0}\" is not evaluated in Equals(T other) or the ValueToSet is equal to the fields default value. Please update Equals(T other) or add the field name to the ExceptionList or EqualsExceptionList property.", fieldName);
				Assert.AreNotEqual(itemWithDefaultField, itemWithFieldToChange, "Field \"{0}\" is not evaluated in Equals(T other) or the ValueToSet is equal to the fields default value. Please update Equals(T other) or add the field name to the ExceptionList or EqualsExceptionList property.", fieldName);
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

	/// <summary>
	/// This supports the common case where the Clone type is the same as the implementation type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class IClonableGenericTests<T> : IClonableGenericTests<T, T> where T: IClonableGeneric<T>
	{

	}

}
