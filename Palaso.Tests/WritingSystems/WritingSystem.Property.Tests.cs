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
			ws.ISO639 = "en";
			ws.Variant = "1901";
			Assert.AreEqual("en-1901", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabel_LanguageTagIsDefaultHasAbbreviation_ShowsAbbreviation()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
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
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.LanguageName = "abcdefghijk";
			Assert.AreEqual("abcd", ws.DisplayLabel);
		}

		[Test]
		public void Rfc5646_HasOnlyAbbreviation_ReturnsQaa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition(){Abbreviation = "hello"};
			Assert.AreEqual("qaa", ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en","","","","", false);
			Assert.AreEqual("en", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoAndScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Zxxx", "", "", "", false);
			Assert.AreEqual("en-Zxxx", ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "", "US", "", "", false);
			Assert.AreEqual("en-US", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoScriptRegionVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Zxxx", "US", "1901", "", false);
			Assert.AreEqual("en-Zxxx-US-1901", ws.RFC5646);
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
			Assert.Greater(WritingSystemDefinition.ValidIso639LanguageCodes.Count, 100);
		}

		[Test]
		public void VerboseDescriptionWhenNoSubtagsSet()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("", "", "", "", "", false);
			Assert.AreEqual("Unknown language. (qaa)", ws.VerboseDescription);
		}

		[Test]
		public void VerboseDescriptionWhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "", "", "", "", false);
			Assert.AreEqual("English. (en)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoAndScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Kore", "", "", "", false);
			Assert.AreEqual("English written in Korean script. (en-Kore)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenOnlyScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("", "Kore", "", "", "", false);
			Assert.AreEqual("Unknown language written in Korean script. (qaa-Kore)", ws.VerboseDescription);
		}

		[Test]
		public void VerboseDescriptionWhenIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "", "US", "", "", false);
			Assert.AreEqual("English in US. (en-US)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoScriptRegionVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Kore", "US", "1901", "", false);
			Assert.AreEqual("English in US written in Korean script. (en-Kore-US-1901)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoIsUnsetButLanguageNameIs()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("", "Kore", "US", "1901", "", false);
			ws.LanguageName = "Eastern lawa";
			Assert.AreEqual("Eastern lawa in US written in Korean script. (qaa-Kore-US-1901)", ws.VerboseDescription);
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
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Kore", "", "", "", false);
			Assert.AreEqual("Korean", ws.Iso15924Script.Label);
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
			firstValueToSet.Add(typeof(RFC5646Tag), new RFC5646Tag("de", "Latn", "", "1901","x-audio"));

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
				{typeof (RFC5646Tag), new RFC5646Tag("en", "Latn", "US", "1901", "x-test")}
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
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 Script = "Latn",
												 Region = "US",
												 Variant = "1901"
											 };
			ws.SetIsVoice(true);
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.AreEqual("US", ws.Region);
			Assert.AreEqual("1901-x-audio", ws.Variant);
			Assert.AreEqual("qaa-Zxxx-US-1901-x-audio", ws.RFC5646);
		}

		[Test]
		public void SetIsVoice_ToTrue_LeavesIsoCodeAlone()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 ISO639 = "en"
											 };
			ws.SetIsVoice(true);
			Assert.AreEqual("en", ws.ISO639);
		}

		[Test]
		public void SetVoice_FalseFromTrue_ClearsScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIsVoice(true);
			ws.SetIsVoice(false);
			Assert.AreEqual("", ws.Script);
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
			ws.SetAllRfc5646LanguageTagComponents("",WellKnownSubTags.Audio.Script,"",WellKnownSubTags.Audio.PrivateUseSubtag);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_ScriptSetToZxXxAndVariantSetToXDashAuDiO_SetsIsVoiceToTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", "ZxXx", "", "x-AuDiO");
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Variant_ChangedToSomethingOtherThanXDashAudioWhileIsVoiceIsTrue_IsVoiceIsChangedToFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIsVoice(true);
			ws.Variant = "1901";
			Assert.AreEqual("1901", ws.Variant);
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Iso_SetToSmthWithDashesWhileIsVoiceIsTrue_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIsVoice(true);
			Assert.Throws<ArgumentException>(() => ws.ISO639 = "iso-script-region-variant");
		}

		[Test]
		public void IsVoice_ToggledAfterVariantHasBeenSet_DoesNotRemoveVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Variant = "1901"
			};
			ws.SetIsVoice(true);
			ws.SetIsVoice(false);
			Assert.AreEqual("1901", ws.Variant);
		}

		[Test]
		public void IsVoice_ToggledAfterRegionHasBeenSet_DoesNotRemoveRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Region = "US"
			};
			ws.SetIsVoice(true);
			ws.SetIsVoice(false);
			Assert.AreEqual("US", ws.Region);
		}

		[Test]
		public void IsVoice_ToggledAfterScriptHasBeenSet_ScriptIsCleared()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Script = "Latn"
			};
			ws.SetIsVoice(true);
			ws.SetIsVoice(false);
			Assert.AreEqual("", ws.Script);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IpaStatusIsSetToNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpaPhontetic_IpaStatusIsSetToNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonetic);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpaPhonemic_IpaStatusIsSetToNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonemic);
			ws.SetIsVoice(true);
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
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag);
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsXDashAudio_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag);
			Assert.Throws<ArgumentException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_VariantSetToPrivateUseOnly_VariantIsSet()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag);
			Assert.AreEqual(ws.Variant, WellKnownSubTags.Audio.PrivateUseSubtag);
		}

		[Test]
		public void Variant_SetToxDashCapitalAUDIOWhileScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = "x-AUDIO");
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsxDashCapitalAUDIO_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", "x-AUDIO");
			Assert.Throws<ArgumentException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void IsVoice_VariantIsxDashPrefixaudioPostFix_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition ();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", "x-PrefixaudioPostfix");
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Variant_ContainsXDashAudioDashFonipa_VariantIsSet()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.VariantSubtag);
			Assert.AreEqual("x-audio-fonipa", ws.Variant);
		}

		[Test]
		public void Variant_ContainsXDashAudioAndFonipa_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Ipa.VariantSubtag + "-" + WellKnownSubTags.Audio.PrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhoneticMarker_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhonemicMarker_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag));
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

		[Test]
		public void Iso_IsEmpty_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(()=>ws.ISO639 = String.Empty);
		}

		[Test]
		public void Variant_ContainsUnderscore_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			Assert.Throws<ArgumentException>(() => ws.Variant = "x_audio");
		}

		[Test]
		public void Variant_ContainsxDashCapitalAUDIOAndScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			ws.Script = "Latn";
			Assert.Throws<ArgumentException>(() => ws.Variant = "x-AUDIO");
		}

		[Test]
		public void Variant_IndicatesThatWsIsAudioAndScriptIsCapitalZXXX_ReturnsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			ws.Script = "ZXXX";
			ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag;
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioButContainsotherThanJustTheNecassaryXDashAudioTagAndScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO639 = "de";
			ws.Script = "latn";
			Assert.Throws<ArgumentException>(()=>ws.Variant = "x-private-audio");
		}

		[Test]
		public void LanguageSubtag_ContainsXDashAudio_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO639 = "de-x-audio");
		}

		[Test]
		public void Language_ContainsZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO639 = "de-Zxxx");
		}

		[Test]
		public void LanguageSubtag_ContainsCapitalXDashAudio_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO639 = "de-X-AuDiO");
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO639 = "bogus");
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO639 = "bogus");
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{

			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO639 = "bogus");
		}

		[Test]
		public void Variant_SetWithPrivateUseTag_VariantisSet()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "x-lalala";
			Assert.AreEqual("x-lalala", ws.Variant);
		}

		[Test]
		public void DuplicateNumber_PrivateUseContainsdupl1_Returns1()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "x-dupl1";
			Assert.AreEqual(1, ws.DuplicateNumber);
		}

		[Test]
		public void DuplicateNumber_PrivateUsedoesNotContainDuplicateMarker_Returns0()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "";
			Assert.AreEqual(0, ws.DuplicateNumber);
		}
	}
}