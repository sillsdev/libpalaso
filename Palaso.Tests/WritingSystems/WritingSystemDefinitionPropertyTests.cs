using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Palaso.Code;
using Palaso.Data;
using Palaso.Tests.Code;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	public class WritingSystemDefinitionIClonableGenericTests : IClonableGenericTests<WritingSystemDefinition>
	{
		public override WritingSystemDefinition CreateNewClonable()
		{
			return new WritingSystemDefinition();
		}

		public override string ExceptionList
		{
			// We do want to clone KnownKeyboards, but I don't think the automatic cloneable test for it can handle a list.
			get { return "|Modified|MarkedForDeletion|StoreID|_collator|_knownKeyboards|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								 new ValuesToSet(3.14f, 2.72f),
								 new ValuesToSet(false, true),
								 new ValuesToSet("to be", "!(to be)"),
								 new ValuesToSet(DateTime.Now, DateTime.MinValue),
								 new ValuesToSet(WritingSystemDefinition.SortRulesType.CustomICU, WritingSystemDefinition.SortRulesType.DefaultOrdering),
								 new ValuesToSet(new RFC5646Tag("en", "Latn", "US", "1901", "test"), RFC5646Tag.Parse("de")),
								 new SubclassValuesToSet<IKeyboardDefinition>(new DefaultKeyboardDefinition() {Layout="mine"}, new DefaultKeyboardDefinition(){Layout="theirs"})
							 };
			}
		}

		/// <summary>
		/// The generic test that clone copies everything can't, I believe, handle lists.
		/// </summary>
		[Test]
		public void CloneCopiesKnownKeyboards()
		{
			var original = new WritingSystemDefinition();
			var kbd1 = new DefaultKeyboardDefinition() {Layout = "mine"};
			var kbd2 = new DefaultKeyboardDefinition() {Layout = "yours"};
			original.AddKnownKeyboard(kbd1);
			original.AddKnownKeyboard(kbd2);
			var copy = original.Clone();
			Assert.That(copy.KnownKeyboards.Count(), Is.EqualTo(2));
			Assert.That(copy.KnownKeyboards.First(), Is.EqualTo(kbd1));
			Assert.That(ReferenceEquals(copy.KnownKeyboards.First(), kbd1), Is.False);
		}

		/// <summary>
		/// The generic test that Equals compares everything can't, I believe, handle lists.
		/// </summary>
		[Test]
		public void EqualsComparesKnownKeyboards()
		{
			var first = new WritingSystemDefinition();
			var kbd1 = new DefaultKeyboardDefinition() { Layout = "mine" };
			var kbd2 = new DefaultKeyboardDefinition() { Layout = "yours" };
			first.AddKnownKeyboard(kbd1);
			first.AddKnownKeyboard(kbd2);
			var second = new WritingSystemDefinition();
			var kbd3 = new DefaultKeyboardDefinition() { Layout = "mine" }; // equal to kbd1
			var kbd4 = new DefaultKeyboardDefinition() {Layout = "theirs"};

			Assert.That(first.Equals(second), Is.False, "ws with empty known keyboards should not equal one with some");
			second.AddKnownKeyboard(kbd3);
			Assert.That(first.Equals(second), Is.False, "ws's with different length known keyboard lits should not be equal");
			second.AddKnownKeyboard(kbd2.Clone());
			Assert.That(first.Equals(second), Is.True, "ws's with same known keyboard lists should be equal");

			second = new WritingSystemDefinition();
			second.AddKnownKeyboard(kbd3);
			second.AddKnownKeyboard(kbd4);
			Assert.That(first.Equals(second), Is.False, "ws with same-length lists of different known keyboards should not be equal");
		}
	}

	[TestFixture]
	public class WritingSystemDefinitionPropertyTests
	{
		[Test]
		public void FromRFC5646Subtags_AllArgs_SetsOk()
		{
			var ws = WritingSystemDefinition.FromSubtags("en", "Latn", "US", "x-whatever");
			Assert.AreEqual(ws.Language, "en");
			Assert.AreEqual(ws.Script, "Latn");
			Assert.AreEqual(ws.Region, "US");
			Assert.AreEqual(ws.Variant, "x-whatever");
		}

		private void AssertWritingSystem(WritingSystemDefinition wsDef, string language, string script, string region, string variant)
		{
			Assert.AreEqual(language, wsDef.Language);
			Assert.AreEqual(script, wsDef.Script);
			Assert.AreEqual(region, wsDef.Region);
			Assert.AreEqual(variant, wsDef.Variant);
		}

		[Test]
		public void Parse_HasOnlyPrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("x-privuse");
			AssertWritingSystem(tag, string.Empty, string.Empty, string.Empty, "x-privuse");
		}

		[Test]
		public void Parse_HasMultiplePrivateUse_WritingSystemHasExpectedFields()
		{
			var tag = WritingSystemDefinition.Parse("x-private-use");
			AssertWritingSystem(tag, string.Empty, string.Empty, string.Empty, "x-private-use");
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
		public void Parse_HasBadSubtag_Throws()
		{
			Assert.Throws<ValidationException>(() => WritingSystemDefinition.Parse("qaa-dupl1"));
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
			ws.Language = "en";
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
		public void InvalidTagOkWhenRequiresValidTagFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.RequiresValidTag = false;
			ws.Language = "Kalaba";
			Assert.That(ws.Language, Is.EqualTo("Kalaba"));
		}

		[Test]
		public void DuplicatePrivateUseOkWhenRequiresValidTagFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.RequiresValidTag = false;
			ws.Variant = "x-nong-nong";
			Assert.That(ws.Variant, Is.EqualTo("x-nong-nong"));
		}

		[Test]
		public void InvalidTagThrowsWhenRequiresValidTagSetToTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.RequiresValidTag = false;
			ws.Language = "Kalaba";
			Assert.Throws(typeof (ValidationException), () => ws.RequiresValidTag = true);
		}

		[Test]
		public void DuplicatePrivateUseThrowsWhenRequiresValidTagSetToTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.RequiresValidTag = false;
			ws.Variant = "x-nong-nong";
			Assert.Throws(typeof(ValidationException), () => ws.RequiresValidTag = true);
		}

		[Test]
		public void Variant_ConsistsOnlyOfRfc5646PrivateUse_VariantIsSetCorrectly()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "x-test";
			Assert.AreEqual("x-test", ws.Variant);
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
			Assert.AreEqual("Language Not Listed", ws.LanguageName);
		}

		[Test]
		public void LanguageName_SetLanguageEn_ReturnsEnglish()
		{
			var ws = new WritingSystemDefinition();
			ws.Language = "en";
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
			Assert.AreEqual("qaa", ws.Bcp47Tag);
		}

		[Test]
		public void Rfc5646WhenJustISO()
		{
			var ws = new WritingSystemDefinition("en","","","","", false);
			Assert.AreEqual("en", ws.Bcp47Tag);
		}
		[Test]
		public void Rfc5646WhenIsoAndScript()
		{
			var ws = new WritingSystemDefinition("en", "Zxxx", "", "", "", false);
			Assert.AreEqual("en-Zxxx", ws.Bcp47Tag);
		}

		[Test]
		public void Rfc5646WhenIsoAndRegion()
		{
			var ws = new WritingSystemDefinition("en", "", "US", "", "", false);
			Assert.AreEqual("en-US", ws.Bcp47Tag);
		}
		[Test]
		public void Rfc5646WhenIsoScriptRegionVariant()
		{
			var ws = new WritingSystemDefinition("en", "Zxxx", "US", "1901", "", false);
			Assert.AreEqual("en-Zxxx-US-1901", ws.Bcp47Tag);
		}

		[Test]
		public void Constructor_OnlyVariantContainingOnlyPrivateUseisPassedIn_RfcTagConsistsOfOnlyPrivateUse()
		{
			var ws = new WritingSystemDefinition("", "", "", "x-private", "", false);
			Assert.AreEqual("x-private", ws.Bcp47Tag);
		}

		[Test]
		public void Parse_OnlyPrivateUseIsPassedIn_RfcTagConsistsOfOnlyPrivateUse()
		{
			var ws = WritingSystemDefinition.Parse("x-private");
			Assert.AreEqual("x-private", ws.Bcp47Tag);
		}

		[Test]
		public void FromRFC5646Subtags_OnlyVariantContainingOnlyPrivateUseisPassedIn_RfcTagConsistsOfOnlyPrivateUse()
		{
			var ws = WritingSystemDefinition.FromSubtags("", "", "", "x-private");
			Assert.AreEqual("x-private", ws.Bcp47Tag);
		}

		[Test]
		public void Constructor_OnlyVariantIsPassedIn_Throws()
		{
			Assert.Throws<ValidationException>(()=>new WritingSystemDefinition("", "", "", "bogus", "", false));
		}

		[Test]
		public void ReadsISORegistry()
		{
			Assert.Greater(StandardTags.ValidIso639LanguageCodes.Count, 100);
		}

		[Test]
		public void ModifyingDefinitionSetsModifiedFlag()
		{
			// Put any properties to ignore in this string surrounded by "|"
			// ObsoleteWindowsLcid has no public setter; it only gets a value by reading from an old file.
			const string ignoreProperties = "|Modified|MarkedForDeletion|StoreID|DateModified|Rfc5646TagOnLoad|RequiresValidTag|WindowsLcid|";
			// special test values to use for properties that are particular
			Dictionary<string, object> firstValueSpecial = new Dictionary<string, object>();
			Dictionary<string, object> secondValueSpecial = new Dictionary<string, object>();
			firstValueSpecial.Add("Variant", "1901");
			secondValueSpecial.Add("Variant", "biske");
			firstValueSpecial.Add("Region", "US");
			secondValueSpecial.Add("Region", "GB");
			firstValueSpecial.Add("ISO639", "en");
			secondValueSpecial.Add("ISO639", "de");
			firstValueSpecial.Add("Language", "en");
			secondValueSpecial.Add("Language", "de");
			firstValueSpecial.Add("ISO", "en");
			secondValueSpecial.Add("ISO", "de");
			firstValueSpecial.Add("Script", "Zxxx");
			secondValueSpecial.Add("Script", "Latn");
			firstValueSpecial.Add("DuplicateNumber", 0);
			secondValueSpecial.Add("DuplicateNumber", 1);
			firstValueSpecial.Add("LocalKeyboard", new DefaultKeyboardDefinition() {Layout="mine"});
			secondValueSpecial.Add("LocalKeyboard", new DefaultKeyboardDefinition() { Layout = "yours" });
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
			// _knownKeyboards and _localKeyboard are tested by the similar test in WritingSystemDefintionICloneableGenericTests.
			// I (JohnT) suspect that this whole test is redundant but am keeping it in case this version
			// confirms something subtly different.
			const string ignoreFields = "|Modified|MarkedForDeletion|StoreID|_collator|_knownKeyboards|_localKeyboard|";
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
		public void SortUsingOtherLanguage_NullRules_DoesNotThrow()
		{
			// This is the current policy for 'OtherLanguage' which currently returns a SystemCollator.
			// If SystemCollator can't determine the other langauge it uses Invariant very quietly.
			// review: Is this the behaviour we want? CP 2011-04
			var ws = new WritingSystemDefinition();
			ws.SortUsingOtherLanguage("NotAValidLanguageCode");
			var collator = ws.Collator;
			int result1 = collator.Compare("b", "A");
			int result2 = collator.Compare("b", "a");
			Assert.AreEqual(result1, result2);
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
			Assert.AreEqual("qaa-Zxxx-US-1901-x-audio", ws.Bcp47Tag);
		}

		[Test]
		public void SetIsVoice_ToTrue_LeavesIsoCodeAlone()
		{
			var ws = new WritingSystemDefinition
					 {
						 Language = "en",
						 IsVoice = true
					 };
			Assert.AreEqual("en", ws.Language);
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
			ws.SetAllComponents(
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
			ws.SetAllComponents(
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
			ws.Language = "th";
			Assert.AreEqual("th", ws.Language);
		}

		[Test]
		public void Iso639_SetInvalidLanguage_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.Language = "xyz");
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
		public void Script_ScriptNull_EmptyString()
		{
			var ws = new WritingSystemDefinition();
			ws.Script = null;
			Assert.That("", Is.EqualTo(ws.Script));
		}

		[Test]
		public void Variant_VariantNull_EmptyString()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = null;
			Assert.That("", Is.EqualTo(ws.Variant));
		}

		[Test]
		public void Region_RegionNull_EmptyString()
		{
			var ws = new WritingSystemDefinition();
			ws.Region = null;
			Assert.That("", Is.EqualTo(ws.Region));
		}

		[Test]
		public void Language_LangaugeNull_EmptyString()
		{
			var ws = new WritingSystemDefinition();
			// Set private use so that we can legally set the language to null
			ws.Variant = "x-any";
			ws.Language = null;
			Assert.That("", Is.EqualTo(ws.Language));
		}

		[Test]
		public void Variant_ResetToEmptyString_ClearsVariant()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "Biske";
			Assert.AreEqual("Biske", ws.Variant);
			ws.Variant = "";
			Assert.AreEqual("", ws.Variant);
		}

		[Test]
		public void Variant_IsSetWithDuplicateTags_DontKnowWhatToDo()
		{
			Assert.Throws<ValidationException>(
				() => new WritingSystemDefinition {Variant = "duplicate-duplicate"}
			);
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
			ws.SetAllComponents(
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
			ws.SetAllComponents(
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
			ws.SetAllComponents(
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
			ws.SetAllComponents(
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
			ws.SetAllComponents(
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
			Assert.Throws<ArgumentException>(
				() => ws.SetAllComponents("qaa", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Ipa.VariantSubtag + "-" + WellKnownSubTags.Audio.PrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashEticAndNotFonipaInVariant_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllComponents("qaa", "", "", WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashEmicAndNotFonipaInVariant_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllComponents("qaa", "", "", WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashEticAndFonipaInPrivateUse_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllComponents("qaa", "", "", WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag + '-' + WellKnownSubTags.Ipa.VariantSubtag));
		}

		[Test]
		public void Variant_ContainsXDashEmicAndAndFonipaInPrivateUse_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllComponents("qaa", "", "", WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag + '-' + WellKnownSubTags.Ipa.VariantSubtag));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhoneticMarker_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllComponents("qaa", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + "etic"));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhonemicMarker_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllComponents("qaa", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + "emic"));
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
		public void IsVoice_SetToTrueOnEntirelyPrivateUseLanguageTag_markerForUnlistedLanguageIsInserted()
		{
			var ws = WritingSystemDefinition.Parse("x-private");
			Assert.That(ws.Variant, Is.EqualTo("x-private"));
			ws.IsVoice = true;
			Assert.That(ws.Language, Is.EqualTo(WellKnownSubTags.Unlisted.Language));
			Assert.That(ws.Script, Is.EqualTo(WellKnownSubTags.Unwritten.Script));
			Assert.That(ws.Region, Is.EqualTo(""));
			Assert.That(ws.Variant, Is.EqualTo("x-private-audio"));
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
			Assert.Throws<ValidationException>(()=>ws.Language = String.Empty);
		}

		[Test]
		public void Variant_ContainsUnderscore_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.Language = "de";
			Assert.Throws<ValidationException>(() => ws.Variant = "x_audio");
		}

		[Test]
		public void Variant_ContainsxDashCapitalAUDIOAndScriptIsNotZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.Language = "de";
			ws.Script = "Latn";
			Assert.Throws<ArgumentException>(() => ws.Variant = "x-AUDIO");
		}

		[Test]
		public void Variant_IndicatesThatWsIsAudioAndScriptIsCapitalZXXX_ReturnsTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.Language = "de";
			ws.Script = "ZXXX";
			ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag;
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioButContainsotherThanJustTheNecassaryXDashAudioTagAndScriptIsNotZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			ws.Language = "de";
			ws.Script = "latn";
			Assert.Throws<ArgumentException>(()=>ws.Variant = "x-private-audio");
		}

		[Test]
		public void LanguageSubtag_ContainsXDashAudio_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.Language = "de-x-audio");
		}

		[Test]
		public void Language_ContainsZxxx_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.Language = "de-Zxxx");
		}

		[Test]
		public void LanguageSubtag_ContainsCapitalXDashAudio_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.Language = "de-X-AuDiO");
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.Language = "bogus");
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.Language = "bogus");
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{

			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(() => ws.Language = "bogus");
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
			ws.SetAllComponents("th", "", "", "");
			Assert.AreEqual("th", ws.Language);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadLanguage_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllComponents("BadLanguage", "", "", "")
			);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_Script_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllComponents("th", "Thai", "", "");
			Assert.AreEqual("Thai", ws.Script);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadScript_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllComponents("th", "BadScript", "", "")
			);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_Region_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllComponents("th", "Thai", "TH", "");
			Assert.AreEqual("TH", ws.Region);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadRegion_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllComponents("th", "Thai", "BadRegion", "")
			);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_Variant_IsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllComponents("th", "Thai", "TH", "1901");
			Assert.AreEqual("1901", ws.Variant);
		}

		[Test]
		public void SetRfc5646LanguageTagComponents_BadVariant_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(
				() => ws.SetAllComponents("th", "Thai", "TH", "BadVariant")
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

		[Test]
		public void IpaStatus_SetToIpaRfcTagStartsWithxDash_InsertsUnknownlanguagemarkerAsLanguageSubtag()
		{
			var writingSystem = new WritingSystemDefinition("x-bogus");
			writingSystem.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual(WellKnownSubTags.Unlisted.Language, writingSystem.Language);
			Assert.AreEqual("qaa-fonipa-x-bogus", writingSystem.Bcp47Tag);
		}

		[Test]
		public void IpaStatus_SetToPhoneticRfcTagStartsWithxDash_InsertsUnknownlanguagemarkerAsLanguageSubtag()
		{
			var writingSystem = new WritingSystemDefinition("x-bogus");
			writingSystem.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.AreEqual(WellKnownSubTags.Unlisted.Language, writingSystem.Language);
			Assert.AreEqual("qaa-fonipa-x-bogus-etic", writingSystem.Bcp47Tag);
		}

		[Test]
		public void IpaStatus_SetToPhonemicRfcTagStartsWithxDash_InsertsUnknownlanguagemarkerAsLanguageSubtag()
		{
			var writingSystem = new WritingSystemDefinition("x-bogus");
			writingSystem.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.AreEqual(WellKnownSubTags.Unlisted.Language, writingSystem.Language);
			Assert.AreEqual("qaa-fonipa-x-bogus-emic", writingSystem.Bcp47Tag);
		}


		[Test]
		public void CloneContructor_VariantStartsWithxDash_VariantIsCopied()
		{
			var writingSystem = new WritingSystemDefinition(new WritingSystemDefinition("x-bogus"));
			Assert.AreEqual("x-bogus", writingSystem.Bcp47Tag);
		}

		[Test]
		public void Language_Set_Idchanged()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.Language = "de";
			Assert.AreEqual("de-Zxxx-1901-x-audio", writingSystem.Id);
		}

		[Test]
		public void Script_Set_Idchanged()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-bogus");
			writingSystem.Script = "Latn";
			Assert.AreEqual("en-Latn-1901-x-bogus", writingSystem.Id);
		}

		[Test]
		public void Region_Set_Idchanged()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-bogus");
			writingSystem.Region = "US";
			Assert.AreEqual("en-Zxxx-US-1901-x-bogus", writingSystem.Id);
		}

		[Test]
		public void Variant_Set_Idchanged()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-bogus");
			writingSystem.Variant = "x-audio";
			Assert.AreEqual("en-Zxxx-x-audio", writingSystem.Id);
		}

		[Test]
		public void Ctor1_IdIsSet()
		{
			var writingSystem = new WritingSystemDefinition();
			Assert.AreEqual("qaa", writingSystem.Id);
		}

		[Test]
		public void Ctor2_IdIsSet()
		{
			var writingSystem = new WritingSystemDefinition("en","Zxxx","","1901-x-audio","abb",true);
			Assert.AreEqual("en-Zxxx-1901-x-audio", writingSystem.Id);
		}

		[Test]
		public void Ctor3_IdIsSet()
		{
			var writingSystem = new WritingSystemDefinition("en-Zxxx-1901-x-audio");
			Assert.AreEqual("en-Zxxx-1901-x-audio", writingSystem.Id);
		}

		[Test]
		public void Ctor4_IdIsSet()
		{
			var writingSystem = new WritingSystemDefinition(new WritingSystemDefinition("en-Zxxx-1901-x-audio"));
			Assert.AreEqual("en-Zxxx-1901-x-audio", writingSystem.Id);
		}

		[Test]
		public void Parse_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.Parse("en-Zxxx-1901-x-audio");
			Assert.AreEqual("en-Zxxx-1901-x-audio", writingSystem.Id);
		}

		[Test]
		public void FromLanguage_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.Parse("en-Zxxx-1901-x-audio");
			Assert.AreEqual("en-Zxxx-1901-x-audio", writingSystem.Id);
		}

		[Test]
		public void FromRfc5646Subtags_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			Assert.AreEqual("en-Zxxx-1901-x-audio", writingSystem.Id);
		}

		[Test]
		public void IpaStatus_Set_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.AreEqual("en-Zxxx-1901-fonipa-x-etic", writingSystem.Id);
		}

		[Test]
		public void IsVoice_Set_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.IsVoice = false;
			Assert.AreEqual("en-1901", writingSystem.Id);
		}

		[Test]
		public void AddToVariant_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.AddToVariant("bauddha");
			Assert.AreEqual("en-Zxxx-1901-bauddha-x-audio", writingSystem.Id);
		}

		[Test]
		public void AddToVariant_NonRegisteredVariant_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.AddToVariant("bogus");
			Assert.AreEqual("en-Zxxx-1901-x-audio-bogus", writingSystem.Id);
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.SetAllComponents("de","Latn","US","fonipa-x-etic");
			Assert.AreEqual("de-Latn-US-fonipa-x-etic", writingSystem.Id);
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_IdChanged_ModifiedisTrue()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.SetAllComponents("de", "Latn", "US", "fonipa-x-etic");
			Assert.AreEqual(writingSystem.Modified, true);
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_IdUnchanged_ModifiedisTrue()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.SetAllComponents("en", "Zxxx", "", "1901-x-audio");
			Assert.AreEqual(writingSystem.Modified, false);
		}

		[Test]
		public void SetRfc5646FromString_IdIsSet()
		{
			var writingSystem = WritingSystemDefinition.FromSubtags("en", "Zxxx", "", "1901-x-audio");
			writingSystem.SetTagFromString("de-Latn-US-fonipa-x-etic");
			Assert.AreEqual("de-Latn-US-fonipa-x-etic", writingSystem.Id);
		}

		[Test]
		public void MakeUnique_IsAlreadyUnique_NothingChanges()
		{
			var existingTags = new[] {"en-Zxxx-x-audio"};
			var ws = new WritingSystemDefinition("de");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, existingTags);
			Assert.That(newWs.Id, Is.EqualTo("de"));
		}

		[Test]
		public void MakeUnique_IsNotUnique_DuplicateMarkerIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, existingTags);
			Assert.That(newWs.Id, Is.EqualTo("en-Zxxx-x-audio-dupl0"));
		}

		[Test]
		public void MakeUnique_ADuplicateAlreadyExists_DuplicatemarkerWithHigherNumberIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio", "en-Zxxx-x-audio-dupl0" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, existingTags);
			Assert.That(newWs.Id, Is.EqualTo("en-Zxxx-x-audio-dupl1"));
		}

		[Test]
		public void MakeUnique_ADuplicatewithHigherNumberAlreadyExists_DuplicateMarkerWithLowNumberIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio", "en-Zxxx-x-audio-dupl1" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, existingTags);
			Assert.That(newWs.Id, Is.EqualTo("en-Zxxx-x-audio-dupl0"));
		}

		[Test]
		public void MakeUnique_StoreIdIsNull()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("de");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, existingTags);
			Assert.That(newWs.StoreID, Is.EqualTo(null));
		}

		[Test]
		public void MakeUnique_IdAlreadyContainsADuplicateMarker_DuplicateNumberIsMaintainedAndNewOneIsIntroduced()
		{
			var existingTags = new[] { "en-Zxxx-x-dupl0-audio", "en-Zxxx-x-audio-dupl1" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-dupl0-audio");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, existingTags);
			Assert.That(newWs.Id, Is.EqualTo("en-Zxxx-x-dupl0-audio-dupl1"));
		}

		[Test]
		public void GetDefaultFontSizeOrMinimum_DefaultConstructor_GreaterThanSix()
		{
			Assert.Greater(new WritingSystemDefinition().GetDefaultFontSizeOrMinimum(),6);
		}
		[Test]
		public void GetDefaultFontSizeOrMinimum_SetAt0_GreaterThanSix()
		{
			var ws = new WritingSystemDefinition()
						 {
							 DefaultFontSize = 0
						 };
			Assert.Greater(ws.GetDefaultFontSizeOrMinimum(), 6);
		}

		[Test]
		public void ListLabel_ScriptRegionVariantEmpty_LabelIsLanguage()
		{
			var ws = new WritingSystemDefinition("de");
			Assert.That(ws.ListLabel, Is.EqualTo("German"));
		}

		[Test]
		public void ListLabel_ScriptSet_LabelIsLanguageWithScriptInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.Script = "Armi";
			Assert.That(ws.ListLabel, Is.EqualTo("German (Armi)"));
		}


		[Test]
		public void ListLabel_RegionSet_LabelIsLanguageWithRegionInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.Region = "US";
			Assert.That(ws.ListLabel, Is.EqualTo("German (US)"));
		}

		[Test]
		public void ListLabel_ScriptRegionSet_LabelIsLanguageWithScriptandRegionInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.Script = "Armi";
			ws.Region = "US";
			Assert.That(ws.ListLabel, Is.EqualTo("German (Armi-US)"));
		}

		[Test]
		public void ListLabel_ScriptVariantSet_LabelIsLanguageWithScriptandVariantInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.Script = "Armi";
			ws.AddToVariant("smth");
			Assert.That(ws.ListLabel, Is.EqualTo("German (Armi-x-smth)"));
		}

		[Test]
		public void ListLabel_RegionVariantSet_LabelIsLanguageWithRegionAndVariantInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.Region = "US";
			ws.AddToVariant("smth");
			Assert.That(ws.ListLabel, Is.EqualTo("German (US-x-smth)"));
		}

		[Test]
		public void ListLabel_VariantSetToIpa_LabelIsLanguageWithIPAInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.Variant = WellKnownSubTags.Ipa.VariantSubtag;
			Assert.That(ws.ListLabel, Is.EqualTo("German (IPA)"));
		}

		[Test]
		public void ListLabel_VariantSetToPhonetic_LabelIsLanguageWithIPADashEticInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.That(ws.ListLabel, Is.EqualTo("German (IPA-etic)"));
		}

		[Test]
		public void ListLabel_VariantSetToPhonemic_LabelIsLanguageWithIPADashEmicInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.That(ws.ListLabel, Is.EqualTo("German (IPA-emic)"));
		}

		[Test]
		public void ListLabel_WsIsVoice_LabelIsLanguageWithVoiceInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.IsVoice = true;
			Assert.That(ws.ListLabel, Is.EqualTo("German (Voice)"));
		}

		[Test]
		public void ListLabel_VariantContainsDuplwithNumber_LabelIsLanguageWithCopyAndNumberInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, new[]{"de", "de-x-dupl0"});
			Assert.That(newWs.ListLabel, Is.EqualTo("German (Copy1)"));
		}

		[Test]
		public void ListLabel_VariantContainsDuplwithZero_LabelIsLanguageWithCopyAndNoNumberInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, new[] { "de" });
			Assert.That(newWs.ListLabel, Is.EqualTo("German (Copy)"));
		}

		[Test]
		public void ListLabel_VariantContainsmulitpleDuplswithNumber_LabelIsLanguageWithCopyAndNumbersInBrackets()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, new[] { "de", "de-x-dupl0" });
			Assert.That(newWs.ListLabel, Is.EqualTo("German (Copy-Copy1)"));
		}

		[Test]
		public void ListLabel_VariantContainsUnknownVariant_LabelIsLanguageWithVariantInBrackets()
		{
			var ws = new WritingSystemDefinition("de");
			ws.AddToVariant("garble");
			Assert.That(ws.ListLabel, Is.EqualTo("German (x-garble)"));
		}

		[Test]
		public void ListLabel_AllSortsOfThingsSet_LabelIsCorrect()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var newWs = WritingSystemDefinition.CreateCopyWithUniqueId(ws, new[] { "de", "de-x-dupl0" });
			newWs.Region = "US";
			newWs.Script = "Armi";
			newWs.IpaStatus = IpaStatusChoices.IpaPhonetic;
			newWs.AddToVariant("garble");
			newWs.AddToVariant("1901");
			Assert.That(newWs.ListLabel, Is.EqualTo("German (IPA-etic-Copy-Copy1-Armi-US-1901-x-garble)"));
		}

		[Test]
		public void OtherAvailableKeyboards_DefaultsToAllAvailable()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var kbd1 = new DefaultKeyboardDefinition() {Layout = "something", Locale="en-US"};
			var kbd2 = new DefaultKeyboardDefinition() { Layout = "somethingElse", Locale = "en-GB" };
			var controller = new MockKeyboardController();
			var keyboardList = new List<IKeyboardDefinition>();
			keyboardList.Add(kbd1);
			keyboardList.Add(kbd2);
			controller.AllAvailableKeyboards = keyboardList;
			Keyboard.Controller = controller;

			var result = ws.OtherAvailableKeyboards;

			Assert.That(result, Has.Member(kbd1));
			Assert.That(result, Has.Member(kbd2));
		}

		[Test]
		public void OtherAvailableKeyboards_OmitsKnownKeyboards()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var kbd1 = new DefaultKeyboardDefinition() { Layout = "something", Locale = "en-US" };
			var kbd2 = new DefaultKeyboardDefinition() { Layout = "somethingElse", Locale = "en-GB" };
			var kbd3 = new DefaultKeyboardDefinition() { Layout = "something", Locale = "en-US" }; // equal to kbd1
			var controller = new MockKeyboardController();
			var keyboardList = new List<IKeyboardDefinition>();
			keyboardList.Add(kbd1);
			keyboardList.Add(kbd2);
			controller.AllAvailableKeyboards = keyboardList;
			Keyboard.Controller = controller;
			ws.AddKnownKeyboard(kbd3);

			var result = ws.OtherAvailableKeyboards.ToList();

			Assert.That(result, Has.Member(kbd2));
			Assert.That(result, Has.No.Member(kbd1));
		}

		class MockKeyboardController : IKeyboardController
		{
			/// <summary>
			/// Tries to get the keyboard with the specified <paramref name="layoutName"/>.
			/// </summary>
			/// <returns>
			/// Returns <c>KeyboardDescription.Zero</c> if no keyboard can be found.
			/// </returns>
			public IKeyboardDefinition GetKeyboard(string layoutName)
			{
				throw new NotImplementedException();
			}

			public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Tries to get the keyboard for the specified <paramref name="writingSystem"/>.
			/// </summary>
			/// <returns>
			/// Returns <c>KeyboardDescription.Zero</c> if no keyboard can be found.
			/// </returns>
			public IKeyboardDefinition GetKeyboard(IWritingSystemDefinition writingSystem)
			{
				throw new NotImplementedException();
			}

			public IKeyboardDefinition GetKeyboard(IInputLanguage language)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Sets the keyboard
			/// </summary>
			public void SetKeyboard(IKeyboardDefinition keyboard)
			{
				throw new NotImplementedException();
			}

			public void SetKeyboard(string layoutName)
			{
				throw new NotImplementedException();
			}

			public void SetKeyboard(string layoutName, string locale)
			{
				throw new NotImplementedException();
			}

			public void SetKeyboard(IWritingSystemDefinition writingSystem)
			{
				throw new NotImplementedException();
			}

			public void SetKeyboard(IInputLanguage language)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Activates the keyboard of the default input language
			/// </summary>
			public void ActivateDefaultKeyboard()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<IKeyboardDefinition> AllAvailableKeyboards { get; set; }

			public IKeyboardDefinition Default;
			public IWritingSystemDefinition ArgumentPassedToDefault;
			public void UpdateAvailableKeyboards()
			{
				throw new NotImplementedException();
			}

			public IKeyboardDefinition DefaultForWritingSystem(IWritingSystemDefinition ws)
			{
				ArgumentPassedToDefault = ws;
				return Default;
			}

			/// <summary>
			/// Creates and returns a keyboard definition object based on the layout and locale.
			/// </summary>
			/// <remarks>The keyboard controller implementing this method will have to check the
			/// availability of the keyboard and what engine provides it.</remarks>
			public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Gets or sets the currently active keyboard
			/// </summary>
			public IKeyboardDefinition ActiveKeyboard { get; set; }

			#region Implementation of IDisposable
			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				GC.SuppressFinalize(this);
			}
			#endregion
		}

		[Test]
		public void SettingLocalKeyboard_AddsToKnownKeyboards()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var kbd1 = new DefaultKeyboardDefinition() { Layout = "something", Locale = "en-US" };

			ws.LocalKeyboard = kbd1;

			Assert.That(ws.LocalKeyboard, Is.EqualTo(kbd1));
			Assert.That(ws.KnownKeyboards.ToList(), Has.Member(kbd1));
		}

		/// <summary>
		/// This incidentally tests that AddKnownKeyboard sets the Modified flag when it DOES change something.
		/// </summary>
		[Test]
		public void AddKnownKeyboard_DoesNotMakeDuplicates()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var kbd1 = new DefaultKeyboardDefinition() { Layout = "something", Locale = "en-US" };
			var kbd2 = new DefaultKeyboardDefinition() { Layout = "something", Locale = "en-US" };

			ws.AddKnownKeyboard(kbd1);
			Assert.That(ws.Modified, Is.True);
			ws.Modified = false;
			ws.AddKnownKeyboard(kbd2);
			Assert.That(ws.Modified, Is.False);

			Assert.That(ws.KnownKeyboards.Count(), Is.EqualTo(1));
		}

		[Test]
		public void AddKnownKeyboard_Null_DoesNothing()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");

			Assert.DoesNotThrow(() => ws.AddKnownKeyboard(null));
		}

		[Test]
		public void LocalKeyboard_DefaultsToFirstKnownAvailable()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var kbd1 = new DefaultKeyboardDefinition() { Layout = "something", Locale = "en-US" };
			var kbd2 = new DefaultKeyboardDefinition() { Layout = "somethingElse", Locale = "en-US" };
			var kbd3 = new DefaultKeyboardDefinition() { Layout = "somethingElse", Locale = "en-US" };

			ws.AddKnownKeyboard(kbd1);
			ws.AddKnownKeyboard(kbd2);

			var controller = new MockKeyboardController();
			var keyboardList = new List<IKeyboardDefinition>();
			keyboardList.Add(kbd3);
			controller.AllAvailableKeyboards = keyboardList;
			Keyboard.Controller = controller;

			Assert.That(ws.LocalKeyboard, Is.EqualTo(kbd2));
		}

		[Test]
		public void LocalKeyboard_DefersToController_WhenNoKnownAvailable()
		{
			var ws = new WritingSystemDefinition("de-x-dupl0");
			var kbd1 = new DefaultKeyboardDefinition() { Layout = "something", Locale = "en-US" };

			var controller = new MockKeyboardController();
			var keyboardList = new List<IKeyboardDefinition>();
			controller.AllAvailableKeyboards = keyboardList;
			controller.Default = kbd1;
			Keyboard.Controller = controller;

			Assert.That(ws.LocalKeyboard, Is.EqualTo(kbd1));
			Assert.That(controller.ArgumentPassedToDefault, Is.EqualTo(ws));
		}
	}
}