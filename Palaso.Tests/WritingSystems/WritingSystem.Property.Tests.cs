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


		[Test]
		public void DisplayLabelWhenUnknown()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.AreEqual("???", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabel_NoAbbreviation_UsesRFC5646()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO639 = "abc";
			ws.Variant = "xyz";
			Assert.AreEqual("abc-xyz", ws.DisplayLabel);
		}

//        [Test]
//        public void DisplayLabel_HasAbbreviation_ShowsAbbreviation()
//        {
//            WritingSystemDefinition ws = new WritingSystemDefinition();
//            ws.ISO = "abc";
//            ws.Abbreviation = "xyz";
//            Assert.AreEqual("xyz", ws.DisplayLabel);
//        }

		[Test]
		public void DisplayLabel_OnlyHasLanguageName_UsesFirstPartOfLanguageName()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.LanguageName = "abcdefghijk";
			Assert.AreEqual("abcd", ws.DisplayLabel);
		}

		[Test]
		public void Rfc5646_HasOnlyAbbreviation_EmptyString()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition(){Abbreviation = "hello"};
			Assert.AreEqual(string.Empty, ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso","","","","","", false);
			Assert.AreEqual("iso", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoAndScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "scrip", "", "", "", "", false);
			Assert.AreEqual("iso-scrip", ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "", "where", "", "", "", false);
			Assert.AreEqual("iso-where", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoScriptRegionVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "scrip", "regn", "var", "", "", false);
			Assert.AreEqual("iso-scrip-regn-var", ws.RFC5646);
		}

		[Test]
		public void ReadsScriptRegistry()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Greater(WritingSystemDefinition.ScriptOptions.Count, 4);
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
			Assert.Greater(WritingSystemDefinition.ScriptOptions.Count, 40);
		}


		[Test]
		public void CurrentScriptOptionReturnCorrectScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "Kore", "", "", "", "", false);
			Assert.AreEqual("Korean", ws.ScriptOption.Label);
		}

		[Test]
		public void CurrentScriptOptionReturnsNullWithUnrecognizedScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("iso", "blah", "", "", "", "", false);
			Assert.IsNull(ws.ScriptOption);
		}

		[Test]
		public void ModifyingDefinitionSetsModifiedFlag()
		{
			// Put any properties to ignore in this string surrounded by "|"
			const string ignoreProperties = "|Modified|MarkedForDeletion|StoreID|DateModified|Rfc5646TagOnLoad|";
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
			firstValueToSet.Add(typeof(RFC5646Tag), new RFC5646Tag("de", "Ltn", "", "1901"));
			secondValueToSet.Add(typeof(RFC5646Tag), RFC5646Tag.RFC5646TagForVoiceWritingSystem("en", String.Empty));

			firstValueToSet.Add(typeof(IpaStatusChoices), IpaStatusChoices.IpaPhonemic);
			secondValueToSet.Add(typeof(IpaStatusChoices), IpaStatusChoices.NotIpa);

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
				catch(Exception error)
				{
					Assert.Fail("Error setting property WritingSystemDefinition.{0},{1}", propertyInfo.Name, error.ToString());
				}
				Assert.IsTrue(ws.Modified, "Modifying WritingSystemDefinition.{0} did not change modified flag.", propertyInfo.Name);
			}
		}

		[Test]
		public void CloneCopiesAllNeededMembers()
		{
			// Put any fields to ignore in this string surrounded by "|"
			const string ignoreFields = "|Modified|MarkedForDeletion|StoreID|_collator|";
			// values to use for testing different types
			var valuesToSet = new Dictionary<Type, object>
			{
				{typeof (float), 3.14f},
				{typeof (bool), true},
				{typeof (string), "Foo"},
				{typeof (DateTime), DateTime.Now},
				{typeof (WritingSystemDefinition.SortRulesType), WritingSystemDefinition.SortRulesType.CustomICU},
				{typeof (RFC5646Tag), RFC5646Tag.RFC5646TagForVoiceWritingSystem("de", String.Empty)}
			};
			foreach (var fieldInfo in typeof(WritingSystemDefinition).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				var fieldName = fieldInfo.Name;
				if (fieldInfo.Name.Contains("<"))
				{
					var splitResult = fieldInfo.Name.Split(new[] {'<', '>'});
					fieldName = splitResult[1];
				}
				if (ignoreFields.Contains("|" + fieldName + "|"))
				{
					continue;
				}
				var ws = new WritingSystemDefinition();
				if (valuesToSet.ContainsKey(fieldInfo.FieldType))
				{
					fieldInfo.SetValue(ws, valuesToSet[fieldInfo.FieldType]);
				}
				else
				{
					Assert.Fail("Unhandled field type - please update the test to handle type {0}", fieldInfo.FieldType.Name);
				}
				var theClone = ws.Clone();
				Assert.AreEqual(valuesToSet[fieldInfo.FieldType], fieldInfo.GetValue(theClone), "Field {0} not copied on WritingSystemDefinition.Clone()", fieldInfo.Name);
			}
		}

		[Test]
		public void SortUsingDefaultOrdering_ValidateSortRulesWhenEmpty_IsTrue()
		{
			var ws = new WritingSystemDefinition();
			string message;
			Assert.IsTrue(ws.ValidateCollationRules(out message));
		}

		[Test]
		public void SortUsingDefaultOrdering_ValidateSortRulesWhenNotEmpty_IsFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.SortRules = "abcd";
			string message;
			Assert.IsFalse(ws.ValidateCollationRules(out message));
		}

		[Test]
		public void IsVoice_SetToTrue_SetsScriptRegionAndVariantCorrectly()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 Script = "Script",
												 Region = "Region",
												 Variant = "Variant"
											 };
			ws.IsVoice = true;
			Assert.AreEqual(ws.Script, "Zxxx");
			Assert.AreEqual(ws.Region, "Region");
			Assert.AreEqual(ws.Variant, "x-audio");
		}

		[Test]
		public void IsVoice_SetToTrue_RemovesEveryThingAfterTheFirstDashFromTheIsoCode()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 ISO639 = "id-as-per-old-wesay"
											 };
			ws.IsVoice = true;
			Assert.AreEqual(ws.ISO639, "id");
		}

		[Test]
		public void IsVoice_SetToTrue_LeavesIsoCodeWithNoDashesAlone()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				ISO639 = "iso"
			};
			ws.IsVoice = true;
			Assert.AreEqual(ws.ISO639, "iso");
		}

		[Test]
		public void IsVoice_SetToFalseFromTrue_ClearsScriptRegionAndVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true
			};
			ws.IsVoice = false;
			Assert.AreEqual("", ws.Script);
			Assert.AreEqual("", ws.Region);
			Assert.AreEqual("", ws.Variant);
		}

		[Test]
		public void IsVoice_SetToTrueWhenIsoContainsDashes_ShortensIsoToEveryThingPriorToFirstDash()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				ISO639 = "iso-script-region-variant"
			};
			ws.IsVoice = true;
			Assert.AreEqual(ws.ISO639, "iso");
		}

		[Test]
		public void Script_ChangedToSomethingOtherThanZxxxWhileIsVoiceIsTrue_IsVoiceIsStillTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 IsVoice = true
											 };
			ws.Script = "change!";
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Script_ChangedToSomethingOtherThanZxxxWhileIsVoiceIsTrue_ScriptIsZxxx()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true
			};
			ws.Script = "change!";
			Assert.AreEqual("Zxxx", ws.Script);
		}

		[Test]
		public void Variant_SetToXDashAudio_SetsIsVoiceToTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = false,
				Script = "Script",
				Region = "Region",
				Variant = "Variant"
			};
			ws.Variant = "x-audio";
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Variant_ChangedToSomethingOtherThanXDashAudioWhileIsVoiceIsTrue_IsVoiceIsChangedToFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true
			};
			ws.Variant = "change!";
			Assert.AreEqual("change!", ws.Variant);
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Iso_SetToSmthWithDashesWhileIsVoiceIsTrue_IsoIsTruncatedToSubTagBeforeFirstDash()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 IsVoice = true,
											 };
			ws.ISO639 = "iso-script-region-variant";
			Assert.AreEqual("iso", ws.ISO639);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Iso_SetToSmthContainingZxxxDashxDashaudioWhileIsVoiceIsTrue_IsVoiceIsChangedToFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true,
			};
			ws.ISO639 = "iso-Zxxx-x-audio";
			Assert.AreEqual("iso", ws.ISO639);
			Assert.AreEqual("x-audio", ws.Variant);
			Assert.AreEqual("Zxxx", ws.Script);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToFalseAfterVariantHasBeenSet_DoesNotRemoveVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Variant = "variant"
			};
			ws.IsVoice = false;
			Assert.AreEqual("variant", ws.Variant);
		}

		[Test]
		public void IsVoice_SetToFalseAfterRegionHasBeenSet_DoesNotRemoveRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Region = "Region"
			};
			ws.IsVoice = false;
			Assert.AreEqual("Region", ws.Region);
		}

		[Test]
		public void IsVoice_SetToFalseAfterScriptHasBeenSet_DoesNotRemoveScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Script = "Zxxx"
			};
			ws.IsVoice = false;
			Assert.AreEqual("Zxxx", ws.Script);
		}
	}
}