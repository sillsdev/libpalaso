using NUnit.Framework;
using System.Collections.Generic;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.Extensions.Is;

namespace SIL.WritingSystems.Tests
{
	public class SpellCheckDictionaryDefinitionCloneableTests : CloneableTests<SpellCheckDictionaryDefinition>
	{
		public override SpellCheckDictionaryDefinition CreateNewCloneable()
		{
			return new SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat.Hunspell);
		}

		protected override bool Equals(SpellCheckDictionaryDefinition x, SpellCheckDictionaryDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList
		{
			get { return "|IsChanged|_urls|PropertyChanged|PropertyChanging|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(SpellCheckDictionaryFormat.Wordlist, SpellCheckDictionaryFormat.Hunspell)
				};
			}
		}

		/// <summary>
		/// The generic test that clone copies everything can't handle lists.
		/// </summary>
		[Test]
		public void CloneCopiesUrls()
		{
			var original = new SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat.Hunspell);
			original.Urls.Add("url1");
			original.Urls.Add("url2");
			SpellCheckDictionaryDefinition copy = original.Clone();
			Assert.That(copy.Urls, Is.EqualTo(new string[] { "url1", "url2" }));
		}

		[Test]
		public void ValueEqualsComparesUrls()
		{
			var first = new SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat.Hunspell);
			first.Urls.Add("url1");
			first.Urls.Add("url2");
			var second = new SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat.Hunspell);

			Assert.That(first.ValueEquals(second), Is.False, "dict with empty URLs should not equal one with some");
			second.Urls.Add("url1");
			Assert.That(first.ValueEquals(second), Is.False, "dicts with different length URL lists should not be equal");
			second.Urls.Add("url2");
			Assert.That(first.ValueEquals(second), Is.True, "dicts with same URL lists should be equal");

			second.Urls.Clear();
			second.Urls.Add("url1");
			second.Urls.Add("url3");
			Assert.That(first.ValueEquals(second), Is.False, "dicts with same-length lists of different URLs should not be equal");
		}
	}
}
