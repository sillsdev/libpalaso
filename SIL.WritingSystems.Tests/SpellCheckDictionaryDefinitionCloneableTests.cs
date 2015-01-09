using NUnit.Framework;
using System.Collections.Generic;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	public class SpellCheckDictionaryDefinitionCloneableTests : CloneableTests<SpellCheckDictionaryDefinition>
	{
		public override SpellCheckDictionaryDefinition CreateNewCloneable()
		{
			return new SpellCheckDictionaryDefinition("language-Tag", SpellCheckDictionaryFormat.Hunspell);
		}

		protected override bool Equals(SpellCheckDictionaryDefinition x, SpellCheckDictionaryDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList
		{
			get { return "|IsChanged|_urls|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(SpellCheckDictionaryFormat.Hunspell, SpellCheckDictionaryFormat.Wordlist)
				};
			}
		}

		/// <summary>
		/// The generic test that clone copies everything can't handle lists.
		/// </summary>
		[Test]
		public void CloneCopiesUrls()
		{
			var original = new SpellCheckDictionaryDefinition("language-Tag", SpellCheckDictionaryFormat.Hunspell);
			string url1 = "url1";
			string url2 = "url2";
			original.Urls.Add(url1);
			original.Urls.Add(url2);
			SpellCheckDictionaryDefinition copy = original.Clone();
			Assert.That(copy.Urls.Count, Is.EqualTo(2));
			Assert.That(copy.Urls[0] == url1, Is.True);
		}

		[Test]
		public void ValueEqualsComparesUrls()
		{
			SpellCheckDictionaryDefinition first = new SpellCheckDictionaryDefinition("language-Tag", SpellCheckDictionaryFormat.Hunspell);
			string url1 = "url1";
			string url2 = "url2";
			first.Urls.Add(url1);
			first.Urls.Add(url2);
			SpellCheckDictionaryDefinition second = new SpellCheckDictionaryDefinition("language-Tag", SpellCheckDictionaryFormat.Hunspell);
			string url3 = "url1";
			string url4 = "url3";

			Assert.That(first.ValueEquals(second), Is.False, "sd with empty urls should not equal one with some");
			second.Urls.Add(url3);
			Assert.That(first.ValueEquals(second), Is.False, "sd's with different length url lists should not be equal");
			second.Urls.Add(url2);
			Assert.That(first.ValueEquals(second), Is.True, "sd's with same url lists should be equal");

			second = new SpellCheckDictionaryDefinition("language-Tag", SpellCheckDictionaryFormat.Hunspell);
			second.Urls.Add(url3);
			second.Urls.Add(url4);
			Assert.That(first.ValueEquals(second), Is.False, "sd with same-length lists of different URLs should not be equal");

		}
	}
}
