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
			ws.ISO = "abc";
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
			Assert.AreEqual(ws.Script, WellKnownSubTags.Audio.Script);
			Assert.AreEqual(ws.Region, "Region");
			Assert.AreEqual(ws.Variant, "Variant-x-audio");
		}

		[Test]
		public void IsVoice_SetToTrue_LeavesIsoCodeAlone()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 ISO = "en-GB"
											 };
			ws.IsVoice = true;
			Assert.AreEqual("en-GB", ws.ISO);
		}

		[Test]
		public void IsVoice_SetToFalseFromTrue_ScriptStaysZxxx()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true
			};
			ws.IsVoice = false;
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.AreEqual("", ws.Region);
			Assert.AreEqual("", ws.Variant);
		}

		[Test]
		public void Script_ChangedToSomethingOtherThanZxxxWhileIsVoiceIsTrue_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 IsVoice = true
											 };
			Assert.Throws<ArgumentException>(() => ws.Script = "change!");
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_ScriptSetToZxxxAndVariantSetToXDashAudio_SetsIsVoiceToTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("",WellKnownSubTags.Audio.Script,"",WellKnownSubTags.Audio.VariantMarker);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_ScriptSetToZxXxAndVariantSetToXDashAuDiO_SetsIsVoiceToTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", "ZxXx", "", "X-AuDiO");
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
		public void Iso_SetToSmthWithDashesWhileIsVoiceIsTrue_IsoIsSet()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 IsVoice = true,
											 };
			ws.ISO = "iso-script-region-variant";
			Assert.AreEqual("iso-script-region-variant", ws.ISO);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Iso_SetToSmthContainingZxxxDashxDashaudioWhileIsVoiceIsTrue_DontKnowWhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true,
			};
			ws.ISO = "iso-Zxxx-x-audio";
			Assert.AreEqual("iso", ws.ISO);
			Assert.AreEqual(WellKnownSubTags.Audio.VariantMarker, ws.Variant);
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.IsTrue(ws.IsVoice);
			throw new NotImplementedException();
		}

		[Test]
		public void IsVoice_ToggledAfterVariantHasBeenSet_DoesNotRemoveVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Variant = "variant"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual("variant", ws.Variant);
		}

		[Test]
		public void IsVoice_ToggledAfterRegionHasBeenSet_DoesNotRemoveRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Region = "Region"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual("Region", ws.Region);
		}

		[Test]
		public void IsVoice_ToggledAfterScriptHasBeenSet_ScriptIsChangedToZxxx()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Script = "Script"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
		}

		[Test]
		public void Variant_IsSetWithDuplicateTags_DontKnowWhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition(){Variant = "duplicate-duplicate"};
			throw new NotImplementedException();
		}

		[Test]
		public void Variant_SetToXDashAudioWhileScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = WellKnownSubTags.Audio.VariantMarker);
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsXDashAudio_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.VariantMarker);
			Assert.Throws<ArgumentException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void Variant_SetToCapitalXDASHAUDIOWhileScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = WellKnownSubTags.Audio.VariantMarker.ToUpper());
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsCapitalXDASHAUDIO_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.VariantMarker.ToUpper());
			Assert.Throws<ArgumentException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void IsVoice_VariantIsPrefixXDashAudioPostFix_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition ();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", "Prefixx-audioPostfix");
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Variant_ContainsXDashAudioAndFonipa_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				()=>ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.VariantMarker + "-" + WellKnownSubTags.Ipa.IpaUnspecified));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhoneticMarker_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.VariantMarker + "-" + WellKnownSubTags.Ipa.IpaPhonetic));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhonemicMarker_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.VariantMarker + "-" + WellKnownSubTags.Ipa.IpaPhonemic));
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IsVoiceIsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(true);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IpaStatusIsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToFalseWhileIpaStatusIsIpa_IsVoiceIsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(false);
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToFalseWhileIpaStatusIsIpa_IpaStatusIsIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(false);
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonetic_IsVoiceIsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonetic);
			ws.SetIsVoice(true);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonetic_IpaStatusIsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonetic);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonemic_IsVoiceIsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonemic);
			ws.SetIsVoice(true);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonemic_IpaStatusIsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonemic);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}
	}
}