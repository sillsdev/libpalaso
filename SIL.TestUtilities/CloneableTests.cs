using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SIL.ObjectModel;
// ReSharper disable LocalizableElement

namespace SIL.TestUtilities
{
	/// <summary>
	/// Generic test that a cloneable object clones all required fields and uses them in equality testing.
	/// The class we are testing (T) must implement <see cref="ICloneable{TClone}"/>.
	/// However, the Clone() method may not be defined to return type T. For example, DefaultKeyboardDefinition
	/// has a Clone() method in its interface, which needs to return IKeyboardDefinition.
	/// Thus, we have two Type parameters, one for the type that Clone() returns.
	/// </summary>
	/// <typeparam name="T">Implementation class</typeparam>
	/// <typeparam name="TClone">Type that Clone() returns</typeparam>
	public abstract class CloneableTests<T, TClone> where T : ICloneable<TClone>
	{
		public abstract T CreateNewCloneable();

		protected virtual bool Equals(T x, T y)
		{
			return EqualityComparer<object>.Default.Equals(x, y);
		}

		private int Compare(T x, T y)
		{
			return Equals(x, y) ? 0 : 1;
		}

		/// <summary>
		/// If set, this is used to determine whether fields of the T are equal to their expected values.
		/// </summary>
		protected IComparer FieldComparer { get; set; }

		protected class ValuesToSet
		{
			public ValuesToSet(object valueToSet, object notEqualValueToSet)
			{
				if (valueToSet.Equals(notEqualValueToSet))
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
					throw new ArgumentException("Value to set should not be the default value", nameof(valueToSet));
				}
			}

			public virtual Type TypeOfValueToSet => ValueToSet.GetType();
			public object ValueToSet { get; }
			public object NotEqualValueToSet { get; }

			private object DefaultValueForValueToSet => TypeOfValueToSet.IsValueType ? Activator.CreateInstance(TypeOfValueToSet) : null;
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

			public override Type TypeOfValueToSet => typeof(TT);
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
		public virtual string EqualsExceptionList => string.Empty;

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
			T clonable = CreateNewCloneable();
			IEnumerable<FieldInfo> fieldInfos = GetAllFields(clonable);

			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				string fieldName = fieldInfo.Name;
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
				catch (InvalidOperationException)
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

				var fieldEqualConstraint = Is.EqualTo(defaultValue);
				if (FieldComparer != null)
					fieldEqualConstraint = fieldEqualConstraint.Using(FieldComparer);
				Assert.That(fieldInfo.GetValue(theClone), fieldEqualConstraint, "Field \"{0}\" not copied on Clone()", fieldName);
			}
		}

		private static IEnumerable<FieldInfo> GetAllFields(T cloneable)
		{
			Type type = cloneable.GetType();
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
			T itemToGetFieldsFrom = CreateNewCloneable();
			IEnumerable<FieldInfo> fieldInfos = GetAllFields(itemToGetFieldsFrom);

			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				T item = CreateNewCloneable();
				T unequalItem = CreateNewCloneable();
				Assert.That(item, Is.EqualTo(unequalItem).Using<T>(Compare), "The two items were not equal on creation. You may need to override Equals(object other).");
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
				Assert.That(item, Is.Not.EqualTo(unequalItem).Using<T>(Compare), $"Field \"{fieldName}\" is not evaluated in Equals({typeof(T)} other). Please update Equals({typeof(T)} other) or add the field name to the ExceptionList or EqualsExceptionList property.");
				Assert.That(unequalItem, Is.Not.EqualTo(item).Using<T>(Compare), $"Field \"{fieldName}\" is not evaluated in Equals({typeof(T)} other). Please update Equals({typeof(T)} other) or add the field name to the ExceptionList or EqualsExceptionList property.");
			}
		}

		[Test]
		//x.Equals(y) && y.Equals(x) where x==y
		public void Equal_ItemsAreEqual_ReturnsTrue()
		{
			T itemToGetFieldsFrom = CreateNewCloneable();
			IEnumerable<FieldInfo> fieldInfos = GetAllFields(itemToGetFieldsFrom);

			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				T item = CreateNewCloneable();
				T unequalItem = CreateNewCloneable();
				Assert.That(item, Is.EqualTo(unequalItem).Using<T>(Compare), "The two items were not equal on creation. You may need to override Equals(object other).");
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
						$"Unhandled field type - please update the test to handle type \"{fieldInfo.FieldType.Name}\". The field that uses this type is \"{fieldName}\".");
				}
				fieldInfo.SetValue(item, valueToSet.ValueToSet);
				fieldInfo.SetValue(unequalItem, valueToSet.ValueToSet);
				Assert.That(item, Is.EqualTo(unequalItem).Using<T>(Compare), $"Field \"{fieldName}\" is not evaluated in Equals({typeof(T)} other). Please update Equals({typeof(T)} other) or add the field name to the ExceptionList or EqualsExceptionList property.");
				Assert.That(unequalItem, Is.EqualTo(item).Using<T>(Compare), $"Field \"{fieldName}\" is not evaluated in Equals({typeof(T)} other). Please update Equals({typeof(T)} other) or add the field name to the ExceptionList or EqualsExceptionList property.");
			}
		}

		[Test]
		//x.Equals(default) && y.Equals(default) (y/x are left on their default values)
		//though x.Equals(null) is better form, many fields and properties are set to something besides null on construction and can never be null (short of reflection as we do below). So By testing
		//against the original value rather than null, we save ourselves a lot of boilerplate in the Equals method.
		public void Equal_SecondFieldIsNull_ReturnsFalse()
		{
			T itemToGetFieldsFrom = CreateNewCloneable();
			IEnumerable<FieldInfo> fieldInfos = GetAllFields(itemToGetFieldsFrom);

			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				T itemWithFieldToChange = CreateNewCloneable();
				T itemWithDefaultField = CreateNewCloneable();
				Assert.That(itemWithFieldToChange, Is.EqualTo(itemWithDefaultField).Using<T>(Compare), "The two items were not equal on creation. You may need to override Equals(object other).");
				string fieldName = fieldInfo.Name;

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
				//This conditional is here in case the ValueToSet is identical to the field's default value.
				//That way developers who inherit this test case don't have to worry about what the field's default value is.
				//It also works around an issue with bool values. If you have two fields that are initialized with different
				//values (i.e. one is true and the other false) this will ensure that the value is chosen which is not equal
				//to the default value and the test therefore has a chance of succeeding.
				if (valueToSet.ValueToSet.Equals(fieldInfo.GetValue(itemWithDefaultField)))
				{
					fieldInfo.SetValue(itemWithFieldToChange, valueToSet.NotEqualValueToSet);
				}
				else
				{
					fieldInfo.SetValue(itemWithFieldToChange, valueToSet.ValueToSet);
				}

				// We cannot use Assert.AreNotEqual because it goes through a long list of different comparers, including EquatableComparer, which
				// explicitly invokes a version of Equals based on the IEquatable interface that would not normally be invoked on the object, since
				// a more specific version of Equals (that applies to both objects) can be found. This might be a bug or deficiency in NUnit.
				Assert.IsFalse(itemWithFieldToChange.Equals(itemWithDefaultField),
					$"Field \"{fieldName}\" is not evaluated in Equals({typeof(T)} other) or the ValueToSet is equal to the field's default value. " +
					$"Please update Equals({typeof(T)} other) or add the field name to the ExceptionList or EqualsExceptionList property.");
				Assert.IsFalse(itemWithFieldToChange.Equals(itemWithDefaultField),
					$"Field \"{fieldName}\" is not evaluated in Equals({typeof(T)} other) or the ValueToSet is equal to the field's default value. " +
					$"Please update Equals({typeof(T)} other) or add the field name to the ExceptionList or EqualsExceptionList property.");
			}
		}

		[Test]
		public void Equals_OtherIsNull_ReturnsFalse()
		{
			T iEquatableUnderTest = CreateNewCloneable();
			IEquatable<T> nullObject = null;
			// ReSharper disable once ExpressionIsAlwaysNull - that's the point of this test
			Assert.That(iEquatableUnderTest.Equals((T) nullObject), Is.False);
		}

		[Test]
		public void ObjectEquals_OtherIsNull_ReturnsFalse()
		{
			T iEquatableUnderTest = CreateNewCloneable();
			Assert.That(iEquatableUnderTest.Equals(null), Is.False);
		}
	}

	/// <summary>
	/// This supports the common case where the Clone type is the same as the implementation type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CloneableTests<T> : CloneableTests<T, T> where T: ICloneable<T>
	{

	}

}
