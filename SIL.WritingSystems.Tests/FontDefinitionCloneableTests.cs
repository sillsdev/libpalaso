using NUnit.Framework;
using System.Collections.Generic;
using Palaso.TestUtilities;

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
			string url1 = "url1";
			string url2 = "url2";
			original.Urls.Add(url1);
			original.Urls.Add(url2);
			FontDefinition copy = original.Clone();
			Assert.That(copy.Urls.Count, Is.EqualTo(2));
			Assert.That(copy.Urls[0] == url1, Is.True);
		}

		[Test]
		public void ValueEqualsComparesUrls()
		{
			FontDefinition first = new FontDefinition("font1");
			string url1 = "url1";
			string url2 = "url2";
			first.Urls.Add(url1);
			first.Urls.Add(url2);
			FontDefinition second = new FontDefinition("font1");
			string url3 = "url1";
			string url4 = "url3";

			Assert.That(first.ValueEquals(second), Is.False, "fd with empty urls should not equal one with some");
			second.Urls.Add(url3);
			Assert.That(first.ValueEquals(second), Is.False, "fd's with different length url lists should not be equal");
			second.Urls.Add(url2);
			Assert.That(first.ValueEquals(second), Is.True, "fd's with same font lists should be equal");

			second = new FontDefinition("font1");
			second.Urls.Add(url3);
			second.Urls.Add(url4);
			Assert.That(first.ValueEquals(second), Is.False, "fd with same-length lists of different URLs should not be equal");
			
		}
	}
}
