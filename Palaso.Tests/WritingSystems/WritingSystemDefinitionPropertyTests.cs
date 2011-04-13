using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Palaso.Data;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemDefinitionPropertyTests
	{

		[Test]
		public void FromRFC5646Subtags_AllArgs_SetsOk()
		{
			var ws = WritingSystemDefinition.FromRFC5646Subtags("en", "Latn", "US", "x-whatever");
			Assert.AreEqual(ws.ISO639, "en");
			Assert.AreEqual(ws.Script, "Latn");
			Assert.AreEqual(ws.Region, "US");
			Assert.AreEqual(ws.Variant, "x-whatever");
		}

		private void AssertWritingSystem(WritingSystemDefinition wsDef, string language, string script, string region, string variant)
		{
			Assert.AreEqual(language, wsDef.ISO639);
			Assert.AreEqual(script, wsDef.Script);
			Assert.AreEqual(region, wsDef.Region);
			Assert.AreEqual(variant, wsDef.Variant);
		}

		[Test]
		public void Parse_HasOnlyPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("x-privuse");
			AssertWritingSystem(tag, "qaa", string.Empty, string.Empty, "x-privuse");
		}

		[Test]
		public void Parse_HasMultiplePrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("x-private-use");
			AssertWritingSystem(tag, "qaa", string.Empty, string.Empty, "x-private-use");
		}

		[Test]
		public void Parse_HasLanguage_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("de");
			AssertWritingSystem(tag, "de", string.Empty, string.Empty, string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScript_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-Latn");
			AssertWritingSystem(tag, "en", "Latn", string.Empty, string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegion_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-Latn-US");
			AssertWritingSystem(tag, "en", "Latn", "US", string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndVariant_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("de-Latn-DE-1901");
			AssertWritingSystem(tag, "de", "Latn", "DE", "1901");
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndMultipleVariants_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("de-Latn-DE-1901-bauddha");
			AssertWritingSystem(tag, "de", "Latn", "DE", "1901-bauddha");
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndMultipleVariantsAndPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("de-Latn-DE-1901-bauddha-x-private");
			AssertWritingSystem(tag, "de", "Latn", "DE", "1901-bauddha-x-private");
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndMultipleVariantsAndMultiplePrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("de-Latn-DE-1901-bauddha-x-private-use");
			AssertWritingSystem(tag, "de", "Latn", "DE", "1901-bauddha-x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndVariantAndMultiplePrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("de-Latn-DE-bauddha-x-private-use");
			AssertWritingSystem(tag, "de", "Latn", "DE", "bauddha-x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndRegionAndVariantAndMultiplePrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("de-DE-bauddha-x-private-use");
			AssertWritingSystem(tag, "de", string.Empty, "DE", "bauddha-x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndVariant_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-alalc97");
			AssertWritingSystem(tag, "en", string.Empty, string.Empty, "alalc97");
		}

		[Test]
		public void Parse_HasLanguageAndMultipleVariants_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-alalc97-aluku");
			AssertWritingSystem(tag, "en", string.Empty, string.Empty, "alalc97-aluku");
		}

		[Test]
		public void Parse_HasLanguageAndRegion_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-US");
			AssertWritingSystem(tag, "en", string.Empty, "US", string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-x-private-use");
			AssertWritingSystem(tag, "en", string.Empty, String.Empty, "x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndVariantAndPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-1901-bauddha-x-private-use");
			AssertWritingSystem(tag, "en", string.Empty, String.Empty, "1901-bauddha-x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndRegionAndPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-US-x-private-use");
			AssertWritingSystem(tag, "en", string.Empty, "US", "x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndRegionAndVariant_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-US-1901-bauddha");
			AssertWritingSystem(tag, "en", string.Empty, "US", "1901-bauddha");
		}

		[Test]
		public void Parse_HasLanguageAndRegionAndVariantAndPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-US-1901-bauddha-x-private-use");
			AssertWritingSystem(tag, "en", string.Empty, "US", "1901-bauddha-x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-Latn-x-private-use");
			AssertWritingSystem(tag, "en", "Latn", String.Empty, "x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("en-Latn-US-x-private-use");
			AssertWritingSystem(tag, "en", "Latn", "US", "x-private-use");
		}

		[Test]
		public void DisplayLabelWhenUnknown()
		{
			var ws = new WritingSystemDefinition();
			Assert.AreEqual("???", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabel_NoAbbreviation_UsesRFC5646()
		{
			var ws = new WritingSystemDefinition();
			ws.ISO639 = "en";
			ws.Variant = "1901";
			Assert.AreEqual("en-1901", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabel_LanguageTagIsDefaultHasAbbreviation_ShowsAbbreviation()
		{
			var ws = new WritingSystemDefinition();
			ws.Abbreviation = "xyz";
			Assert.AreEqual("xyz", ws.DisplayLabel);
		}

		[Test]
		public void Variant_ConsistsOnlyOfRfc5646Variant_VariantIsSetCorrectly()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa";
			Assert.AreEqual("fonipa", ws.Variant);
		}

		[Test]
		public void Variant_ConsistsOnlyOfRfc5646PrivateUse_VariantIsSetCorrectly()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "x-etic";
			Assert.AreEqual("x-etic", ws.Variant);
		}

		[Test]
		public void Variant_ConsistsOfBothRfc5646VariantandprivateUse_VariantIsSetCorrectly()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa-x-etic";
			Assert.AreEqual("fonipa-x-etic", ws.Variant);
		}

		[Test]
		public void DisplayLabel_OnlyHasLanguageName_UsesFirstPartOfLanguageName()
		{
			var ws = new WritingSystemDefinition();
			ws.LanguageName = "abcdefghijk";
			Assert.AreEqual("abcd", ws.DisplayLabel);
		}

		[Test]
		public void LanguageName_Default_ReturnsUnknownLanguage()
		{
			var ws = new WritingSystemDefinition();
			Assert.AreEqual("Unknown Language", ws.LanguageName);
		}

		[Test]
		public void LanguageName_SetLanguageEn_ReturnsEnglish()
		{
			var ws = new WritingSystemDefinition();
			ws.ISO639 = "en";
			Assert.AreEqual("English", ws.LanguageName);
		}

		[Test]
		public void LanguageName_SetCustom_ReturnsCustomName()
		{
			var ws = new WritingSystemDefinition();
			ws.LanguageName = "CustomName";
			Assert.AreEqual("CustomName", ws.LanguageName);
		}

		[Test]
		public void Rfc5646_HasOnlyAbbreviation_ReturnsQaa()
		{
			var ws = new WritingSystemDefinition {Abbreviation = "hello"};
			Assert.AreEqual("qaa", ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenJustISO()
		{
			var ws = new WritingSystemDefinition("en","","","","", false);
			Assert.AreEqual("en", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoAndScript()
		{
			var ws = new WritingSystemDefinition("en", "Zxxx", "", "", "", false);
			Assert.AreEqual("en-Zxxx", ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenIsoAndRegion()
		{
			var ws = new WritingSystemDefinition("en", "", "US", "", "", false);
			Assert.AreEqual("en-US", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoScriptRegionVariant()
		{
			var ws = new WritingSystemDefinition("en", "Zxxx", "US", "1901", "", false);
			Assert.AreEqual("en-Zxxx-US-1901", ws.RFC5646);
		}

		[Test]
		public void ReadsISORegistry()
		{
			Assert.Greater(WritingSystemDefinition.ValidIso639LanguageCodes.Count, 100);
		}

		[Test]
		public void ModifyingDefinitionSetsModifiedFlag()
		{
			// Put any properties to ignore in this string surrounded by "|"
			const string ignoreProperties = "|Modified|MarkedForDeletion|StoreID|DateModified|Rfc5646TagOnLoad|";
			// special test values to use for properties that are particular
			Dictionary<string, object> firstValueSpecial = new Dictionary<string, object>();
			Dictionary<string, object> secondValueSpecial = new Dictionary<string, object>();
			firstValueSpecial.Add("Variant", "1901");
			secondValueSpecial.Add("Variant", "biske");
			firstValueSpecial.Add("Region", "US");
			secondValueSpecial.Add("Region", "GB");
			firstValueSpecial.Add("ISO639", "en");
			secondValueSpecial.Add("ISO639", "de");
			firstValueSpecial.Add("ISO", "en");
			secondValueSpecial.Add("ISO", "de");
			firstValueSpecial.Add("Script", "Zxxx");
			secondValueSpecial.Add("Script", "Latn");
			firstValueSpecial.Add("DuplicateNumber", 0);
			secondValueSpecial.Add("DuplicateNumber", 1);
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
			firstValueToSet.Add(typeof(RFC5646Tag), new RFC5646Tag("de", "Latn", "", "1901","audio"));

			firstValueToSet.Add(typeof(IpaStatusChoices), IpaStatusChoices.IpaPhonemic);
			secondValueToSet.Add(typeof(IpaStatusChoices), IpaStatusChoices.NotIpa);

			foreach (PropertyInfo propertyInfo in typeof(WritingSystemDefinition).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				// skip read-only or ones in the ignore list
				if (!propertyInfo.CanWrite || ignoreProperties.Contains("|" + propertyInfo.Name + "|"))
				{
					continue;
				}
				var ws = new WritingSystemDefinition();
				ws.Modified = false;
				// We need to ensure that all values we are setting are actually different than the current values.
				// This could be accomplished by comparing with the current value or by setting twice with different values.
				// We use the setting twice method so we don't require a getter on the property.
				try
				{
					if (firstValueSpecial.ContainsKey(propertyInfo.Name) && secondValueSpecial.ContainsKey(propertyInfo.Name))
					{
						propertyInfo.SetValue(ws, firstValueSpecial[propertyInfo.Name], null);
						propertyInfo.SetValue(ws, secondValueSpecial[propertyInfo.Name], null);
					}
					else if (firstValueToSet.ContainsKey(propertyInfo.PropertyType) && secondValueToSet.ContainsKey(propertyInfo.PropertyType))
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
				{typeof (RFC5646Tag), new RFC5646Tag("en", "Latn", "US", "1901", "test")}
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
					Assert.Fail("Unhandled field type - please update the test to handle type {0}. The field that uses this type is {1}.", fieldInfo.FieldType.Name, fieldName);
				}
				var theClone = ws.Clone();
				if(fieldInfo.GetValue(ws).GetType() != typeof(string))  //strings are special in .net so we won't worry about checking them here.
				{
					Assert.AreNotSame(fieldInfo.GetValue(ws), fieldInfo.GetValue(theClone),
									  "The field {0} refers to the same object, it was not copied.", fieldName);
				}
				Assert.AreEqual(valuesToSet[fieldInfo.FieldType], fieldInfo.GetValue(theClone), "Field {0} not copied on WritingSystemDefinition.Clone()", fieldName);
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
		public void SetIsVoice_SetToTrue_SetsScriptRegionAndVariantCorrectly()
		{
			var ws = new WritingSystemDefinition
					 {
						 Script = "Latn",
						 Region = "US",
						 Variant = "1901",
						 IsVoice = true
					 };
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.AreEqual("US", ws.Region);
			Assert.AreEqual("1901-x-audio", ws.Variant);
			Assert.AreEqual("qaa-Zxxx-US-1901-x-audio", ws.RFC5646);
		}

		[Test]
		public void SetIsVoice_ToTrue_LeavesIsoCodeAlone()
		{
			var ws = new WritingSystemDefinition
					 {
						 ISO639 = "en",
						 IsVoice = true
					 };
			Assert.AreEqual("en", ws.ISO639);
		}

		[Test]
		public void SetVoice_FalseFromTrue_ClearsScript()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual("", ws.Script);
			Assert.AreEqual("", ws.Region);
			Assert.AreEqual("", ws.Variant);
		}

		[Test]
		public void Script_ChangedToSomethingOtherThanZxxxWhileIsVoiceIsTrue_Throws()
		{
			var ws = new WritingSystemDefinition
			{
				IsVoice = true
			};
			Assert.Throws<ValidationException>(() => ws.Script = "change!");
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_ScriptSetToZxxxAndVariantSetToXDashAudio_SetsIsVoiceToTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents(
				"th",
				WellKnownSubTags.Audio.Script,
				"",
				WellKnownSubTags.Audio.PrivateUseSubtag
			);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_ScriptSetToZxXxAndVariantSetToXDashAuDiO_SetsIsVoiceToTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents(
				"th",
				"ZxXx",
				"",
				"x-AuDiO"
			);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Variant_ChangedToSomethingOtherThanXDashAudioWhileIsVoiceIsTrue_IsVoiceIsChangedToFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.Variant = "1901";
			Assert.AreEqual("1901", ws.Variant);
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Iso639_SetValidLanguage_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.ISO639 = "th";
			Assert.AreEqual("th", ws.ISO639);
		}

		[Test]
		public void Iso639_SetInvalidLanguage_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.ISO639 = "xyz");
		}

		[Test]
		public void IsVoice_ToggledAfterVariantHasBeenSet_DoesNotRemoveVariant()
		{
			var ws = new WritingSystemDefinition
					 {
						 Variant = "1901",
						 IsVoice = true
					 };
			ws.IsVoice = false;
			Assert.AreEqual("1901", ws.Variant);
		}

		[Test]
		public void IsVoice_ToggledAfterRegionHasBeenSet_DoesNotRemoveRegion()
		{
			var ws = new WritingSystemDefinition()
			{
				Region = "US"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual("US", ws.Region);
		}

		[Test]
		public void IsVoice_ToggledAfterScriptHasBeenSet_ScriptIsCleared()
		{
			var ws = new WritingSystemDefinition()
			{
				Script = "Latn"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual("", ws.Script);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IpaStatusIsSetToNotIpa()
		{
			var ws = new WritingSystemDefinition
					 {
						 IpaStatus = IpaStatusChoices.Ipa,
						 IsVoice = true
					 };
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpaPhontetic_IpaStatusIsSetToNotIpa()
		{
			var ws = new WritingSystemDefinition
					 {
						 IpaStatus = IpaStatusChoices.IpaPhonetic,
						 IsVoice = true
					 };
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpaPhonemic_IpaStatusIsSetToNotIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			ws.IsVoice = true;
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void Variant_IsSetWithDuplicateTags_DontKnowWhatToDo()
		{
			Assert.Throws<ArgumentException>(()=>new WritingSystemDefinition(){Variant = "duplicate-duplicate"});
		}

		[Test]
		public void Variant_SetToXDashAudioWhileScriptIsNotZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag);
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsXDashAudio_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents(
				"th",
				WellKnownSubTags.Audio.Script,
				"",
				WellKnownSubTags.Audio.PrivateUseSubtag
			);
			Assert.Throws<ValidationException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_VariantSetToPrivateUseOnly_VariantIsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents(
				"th",
				WellKnownSubTags.Audio.Script,
				"",
				WellKnownSubTags.Audio.PrivateUseSubtag
			);
			Assert.AreEqual(ws.Variant, WellKnownSubTags.Audio.PrivateUseSubtag);
		}

		[Test]
		public void Variant_SetToxDashCapitalAUDIOWhileScriptIsNotZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = "x-AUDIO");
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsxDashCapitalAUDIO_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents(
				"th",
				WellKnownSubTags.Audio.Script,
				"",
				"x-AUDIO"
			);
			Assert.Throws<ValidationException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void IsVoice_VariantIsxDashPrefixaudioPostFix_ReturnsFalse()
		{
			var ws = new WritingSystemDefinition ();
			ws.SetAllRfc5646LanguageTagComponents(
				"th",
				WellKnownSubTags.Audio.Script,
				"",
				"x-PrefixaudioPostfix"
			);
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Variant_ContainsXDashAudioDashFonipa_VariantIsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents(
				"th",
				WellKnownSubTags.Audio.Script,
				"",
				WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.VariantSubtag
			);
			Assert.AreEqual("x-audio-fonipa", ws.Variant);
		}

		[Test]
		public void Variant_ContainsXDashAudioAndFonipa_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Ipa.VariantSubtag + "-" + WellKnownSubTags.Audio.PrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhoneticMarker_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhonemicMarker_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag));
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IsVoiceIsTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.Ipa;
			ws.IsVoice = true;
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IpaStatusIsNotIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.Ipa;
			ws.IsVoice = true;
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToFalseWhileIpaStatusIsIpa_IsVoiceIsFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.Ipa;
			ws.IsVoice = false;
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToFalseWhileIpaStatusIsIpa_IpaStatusIsIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.Ipa;
			ws.IsVoice = false;
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonetic_IsVoiceIsTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			ws.IsVoice = true;
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonetic_IpaStatusIsNotIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			ws.IsVoice = true;
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonemic_IsVoiceIsTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			ws.IsVoice = true;
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonemic_IpaStatusIsNotIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			ws.IsVoice = true;
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void Iso639_SetEmpty_ThrowsValidationException()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(()=>ws.ISO639 = String.Empty);
		}

		[Test]
		public void Variant_ContainsUnderscore_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			Assert.Throws<ArgumentException>(() => ws.Variant = "x_audio");
		}

		[Test]
		public void Variant_ContainsxDashCapitalAUDIOAndScriptIsNotZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			ws.Script = "Latn";
			Assert.Throws<ArgumentException>(() => ws.Variant = "x-AUDIO");
		}

		[Test]
		public void Variant_IndicatesThatWsIsAudioAndScriptIsCapitalZXXX_ReturnsTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			ws.Script = "ZXXX";
			ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag;
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioButContainsotherThanJustTheNecassaryXDashAudioTagAndScriptIsNotZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			ws.Script = "latn";
			Assert.Throws<ArgumentException>(()=>ws.Variant = "x-private-audio");
		}

		[Test]
		public void LanguageSubtag_ContainsXDashAudio_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.ISO639 = "de-x-audio");
		}

		[Test]
		public void Language_ContainsZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.ISO639 = "de-Zxxx");
		}

		[Test]
		public void LanguageSubtag_ContainsCapitalXDashAudio_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.ISO639 = "de-X-AuDiO");
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.ISO639 = "bogus");
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.ISO639 = "bogus");
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{

			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.ISO639 = "bogus");
		}

		[Test]
		public void Variant_SetWithPrivateUseTag_VariantisSet()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "x-lalala";
			Assert.AreEqual("x-lalala", ws.Variant);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_Language_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("th", "", "", "");
			Assert.AreEqual("th", ws.ISO639);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadLanguage_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllRfc5646LanguageTagComponents("BadLanguage", "", "", "")
			);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_Script_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("th", "Thai", "", "");
			Assert.AreEqual("Thai", ws.Script);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadScript_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllRfc5646LanguageTagComponents("th", "BadScript", "", "")
			);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_Region_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("th", "Thai", "TH", "");
			Assert.AreEqual("TH", ws.Region);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadRegion_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllRfc5646LanguageTagComponents("th", "Thai", "BadRegion", "")
			);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_Variant_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("th", "Thai", "TH", "1901");
			Assert.AreEqual("1901", ws.Variant);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadVariant_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllRfc5646LanguageTagComponents("th", "Thai", "TH", "BadVariant")
			);
		}

		[Test]
		public void Abbreviation_Sets_GetsSame()
		{
			var ws = new WritingSystemDefinition();
			ws.Abbreviation = "en";
			Assert.AreEqual("en", ws.Abbreviation);
		}

		[Test]
		public void Abbreviation_Uninitialized_ReturnsISO639()
		{
			var writingSystem = new WritingSystemDefinition("en");
			Assert.AreEqual("en", writingSystem.Abbreviation);
		}

	}
}