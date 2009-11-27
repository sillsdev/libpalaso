using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemPropertyTests
	{
		[SetUp]
		public void Setup()
		{

		}

		[TearDown]
		public void TearDown()
		{

		}


		[Test]
		public void DisplayLabelWhenUnknown()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.AreEqual("???", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabelWhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "abc";
			Assert.AreEqual("abc", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabelWhenHasAbbreviation()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "abc";
			ws.Abbreviation = "xyz";
			Assert.AreEqual("xyz", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabelWhenJustLanguage()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.LanguageName = "abcdefghijk";
			Assert.AreEqual("abcd", ws.DisplayLabel);
		}

		[Test]
		public void Rfc4646WhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso","","","","","", false);
			Assert.AreEqual("iso", ws.RFC4646);
		}
		[Test]
		public void Rfc4646WhenIsoAndScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "scrip", "", "", "", "", false);
			Assert.AreEqual("iso-scrip", ws.RFC4646);
		}

		[Test]
		public void Rfc4646WhenIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "", "where", "", "", "", false);
			Assert.AreEqual("iso-where", ws.RFC4646);
		}
		[Test]
		public void Rfc4646WhenIsoScriptRegionVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "scrip", "regn", "var", "", "", false);
			Assert.AreEqual("iso-scrip-regn-var", ws.RFC4646);
		}

		[Test]
		public void ReadsScriptRegistry()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Greater(ws.ScriptOptions.Count,4);
		}


		[Test]
		public void ReadsISORegistry()
		{
			Assert.Greater(WritingSystemDefinition.LanguageCodes.Count, 100);
		}


		[Test]
		public void VerboseDescriptionWhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "", "", "", "", "", false);
			Assert.AreEqual("???. (iso)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoAndScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "Kore", "", "", "", "", false);
			Assert.AreEqual("??? written in Korean script. (iso-Kore)", ws.VerboseDescription);
		}

		[Test]
		public void VerboseDescriptionWhenIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "", "flubville", "", "foobar", "", false);
			Assert.AreEqual("foobar in flubville. (iso-flubville)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoScriptRegionVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "Kore", "regn", "western", "foobar", "", false);
			Assert.AreEqual("western foobar in regn written in Korean script. (iso-Kore-regn-western)", ws.VerboseDescription);
		}

		[Test]
		public void HasLotsOfScriptOptions()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Greater(ws.ScriptOptions.Count, 40);
		}


		[Test]
		public void CurrentScriptOptionReturnCorrectScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "Kore", "", "", "", "", false);
			Assert.AreEqual("Korean", ws.CurrentScriptOption.Label);
		}

		[Test]
		public void CurrentScriptOptionReturnsNullWithUnrecognizedScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "blah", "", "", "", "", false);
			Assert.IsNull(ws.CurrentScriptOption);
		}

		[Test]
		public void ModifyingDefinitionSetsModifiedFlag()
		{
			// Put any properties to ignore in this string surrounded by "|"
			const string ignoreProperties = "|Modified|MarkedForDeletion|StoreID|DateModified|";
			// special test values to use for properties that are particular
			//Dictionary<string, object> firstValueSpecial = new Dictionary<string, object>();
			//Dictionary<string, object> secondValueSpecial = new Dictionary<string, object>();
			//firstValueSpecial.Add("SortUsing", "CustomSimple");
			//secondValueSpecial.Add("SortUsing", "CustomICU");
			// test values to use based on type
			Dictionary<Type, object> firstValueToSet = new Dictionary<Type, object>();
			Dictionary<Type, object> secondValueToSet = new Dictionary<Type, object>();
			firstValueToSet.Add(typeof (float), 2.18281828459045f);
			secondValueToSet.Add(typeof (float), 3.141592653589f);
			firstValueToSet.Add(typeof (bool), true);
			secondValueToSet.Add(typeof (bool), false);
			firstValueToSet.Add(typeof (string), "X");
			secondValueToSet.Add(typeof (string), "Y");
			firstValueToSet.Add(typeof (DateTime), new DateTime(2007, 12, 31));
			secondValueToSet.Add(typeof (DateTime), new DateTime(2008, 1, 1));
			firstValueToSet.Add(typeof(WritingSystemDefinition.SortRulesType), WritingSystemDefinition.SortRulesType.CustomICU);
			secondValueToSet.Add(typeof(WritingSystemDefinition.SortRulesType), WritingSystemDefinition.SortRulesType.CustomSimple);
			foreach (PropertyInfo propertyInfo in typeof(WritingSystemDefinition).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				// skip read-only or ones in the ignore list
				if (!propertyInfo.CanWrite || ignoreProperties.Contains("|" + propertyInfo.Name + "|"))
				{
					continue;
				}
				WritingSystemDefinition ws = new WritingSystemDefinition();
				ws.Modified = false;
				// We need to ensure that all values we are setting are actually different than the current values.
				// This could be accomplished by comparing with the current value or by setting twice with different values.
				// We use the setting twice method so we don't require a getter on the property.
				try
				{
					//if (firstValueSpecial.ContainsKey(propertyInfo.Name) && secondValueSpecial.ContainsKey(propertyInfo.Name))
					//{
					//    propertyInfo.SetValue(ws, firstValueSpecial[propertyInfo.Name], null);
					//    propertyInfo.SetValue(ws, secondValueSpecial[propertyInfo.Name], null);
					//}
					if (firstValueToSet.ContainsKey(propertyInfo.PropertyType) && secondValueToSet.ContainsKey(propertyInfo.PropertyType))
					{
						propertyInfo.SetValue(ws, firstValueToSet[propertyInfo.PropertyType], null);
						propertyInfo.SetValue(ws, secondValueToSet[propertyInfo.PropertyType], null);
					}
					else
					{
						Assert.Fail("Unhandled property type - please update the test to handle type {0}",
									propertyInfo.PropertyType.Name);
					}
				}
				catch
				{
					Assert.Fail("Error setting property WritingSystemDefinition.{0}", propertyInfo.Name);
				}
				Assert.IsTrue(ws.Modified, "Modifying WritingSystemDefinition.{0} did not change modified flag.", propertyInfo.Name);
			}
		}

		[Test]
		public void CloneCopiesAllNeededMembers()
		{
			// Put any fields to ignore in this string surrounded by "|"
			const string ignoreFields = "|_modified|_markedForDeletion|_storeID|_collator|";
			// values to use for testing different types
			Dictionary<Type, object> valuesToSet = new Dictionary<Type, object>();
			valuesToSet.Add(typeof (float), 3.14f);
			valuesToSet.Add(typeof (bool), true);
			valuesToSet.Add(typeof (string), "Foo");
			valuesToSet.Add(typeof (DateTime), DateTime.Now);
			valuesToSet.Add(typeof (WritingSystemDefinition.SortRulesType), WritingSystemDefinition.SortRulesType.CustomICU);
			foreach (FieldInfo fieldInfo in typeof(WritingSystemDefinition).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				if (ignoreFields.Contains("|" + fieldInfo.Name + "|"))
				{
					continue;
				}
				WritingSystemDefinition ws = new WritingSystemDefinition();
				if (valuesToSet.ContainsKey(fieldInfo.FieldType))
				{
					fieldInfo.SetValue(ws, valuesToSet[fieldInfo.FieldType]);
				}
				else
				{
					Assert.Fail("Unhandled field type - please update the test to handle type {0}", fieldInfo.FieldType.Name);
				}
				Assert.AreEqual(valuesToSet[fieldInfo.FieldType], fieldInfo.GetValue(ws.Clone()), "Field {0} not copied on WritingSystemDefinition.Clone()", fieldInfo.Name);
			}
		}

		[Test]
		public void NoSortUsing_ValidateSortRules_IsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			string message;
			Assert.IsFalse(ws.ValidateCollationRules(out message));
		}
	}
}