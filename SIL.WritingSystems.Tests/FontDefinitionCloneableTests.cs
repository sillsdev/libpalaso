using NUnit.Framework;
using System.Collections.Generic;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.WritingSystems.Tests
{
	public class FontDefinitionCloneableTests : CloneableTests<FontDefinition>
	{
		public override FontDefinition CreateNewCloneable()
		{
			return new FontDefinition("font1");
		}

		protected override bool Equals(FontDefinition x, FontDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList => "|IsChanged|Urls|PropertyChanged|PropertyChanging|";

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(12.0f, 13.0f),
					new ValuesToSet(FontEngines.Graphite, FontEngines.OpenType),
					new ValuesToSet(FontRoles.Default, FontRoles.Emphasis)
				};
			}
		}

		/// <summary>
		/// The generic test that clone copies everything can't handle lists.
		/// </summary>
		[Test]
		public void CloneCopiesUrls()
		{
			var original = new FontDefinition("font1");
			original.Urls.Add("url1");
			original.Urls.Add("url2");
			FontDefinition copy = original.Clone();
			Assert.That(copy.Urls, Is.EqualTo(new string[] {"url1", "url2"}));
		}

		[Test]
		public void ValueEqualsComparesUrls()
		{
			var first = new FontDefinition("font1");
			first.Urls.Add("url1");
			first.Urls.Add("url2");
			var second = new FontDefinition("font1");

			Assert.That(first.ValueEquals(second), Is.False, "font with empty URLs should not equal one with some");
			second.Urls.Add("url1");
			Assert.That(first.ValueEquals(second), Is.False, "fonts with different length URL lists should not be equal");
			second.Urls.Add("url2");
			Assert.That(first.ValueEquals(second), Is.True, "fonts with same URL lists should be equal");

			second.Urls.Clear();
			second.Urls.Add("url1");
			second.Urls.Add("url3");
			Assert.That(first.ValueEquals(second), Is.False, "fonts with same-length lists of different URLs should not be equal");
		}
	}
}
