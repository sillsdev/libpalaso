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

		// Put any fields to ignore in this string surrounded by "|"
		public abstract string ExceptionList { get; }

		public abstract Dictionary<Type, object> DefaultValuesForTypes { get; }

		[Test]
		public void CloneCopiesAllNeededMembers()
		{
			var clonable = CreateNewClonable();
			var type = clonable.GetType();
			IEnumerable<FieldInfo> fieldInfos = new List<FieldInfo>();
			while(type!=null){
				fieldInfos = fieldInfos.Concat(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				type = type.BaseType;
			}

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
				if (DefaultValuesForTypes.ContainsKey(fieldInfo.FieldType))
				{
					fieldInfo.SetValue(clonable, DefaultValuesForTypes[fieldInfo.FieldType]);
				}
				else
				{
					Assert.Fail("Unhandled field type - please update the test to handle type {0}. The field that uses this type is {1}.", fieldInfo.FieldType.Name, fieldName);
				}
				var theClone = Convert.ChangeType(clonable.Clone(), clonable.GetType());
				if (fieldInfo.GetValue(clonable).GetType() != typeof(string))  //strings are special in .net so we won't worry about checking them here.
				{
					Assert.AreNotSame(fieldInfo.GetValue(clonable), fieldInfo.GetValue(theClone),
									  "The field {0} refers to the same object, it was not copied.", fieldName);
				}
				Assert.AreEqual(DefaultValuesForTypes[fieldInfo.FieldType], fieldInfo.GetValue(theClone), "Field {0} not copied on Clone()", fieldName);
			}
		}
	}
}
